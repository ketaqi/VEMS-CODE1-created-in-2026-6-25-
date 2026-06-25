using Complex = System.Numerics.Complex;
using VEMS.MathCore;

namespace VEMS.EMSolver
{

    /// <summary>
    /// EM field class
    /// general version in 3D
    /// </summary>
    public class EMField : FieldBase
    {
        #region properties

        private bool _eigenInfoReady { get; set; }
        private bool _gammaReady { get; set; }

        /// <summary>
        /// sign factor according to direction
        /// </summary>
        public SignFactor Direction { get; set; }

        /// <summary>
        /// polarization mode
        /// TE or TM mode
        /// </summary>
        public PolarizationMode Polarization { get; set; }

        /// <summary>
        /// domain in which EM field is given/calculated
        /// </summary>
        public ModelingDomain Domain { get; set; }

        /// <summary>
        /// sampling grid information
        /// in the current domain
        /// </summary>
        public GridInfo2D GridInfo { get; set; }


        /// <summary>
        /// transverse spatial frequencies Kx
        /// </summary>
        public VectorD Kx { get; set; }

        /// <summary>
        /// transverse spatial frequencies Ky
        /// </summary>
        public VectorD Ky { get; set; }

        /// <summary>
        /// normalized transverse spatial frequencies
        /// Nx = Kx / K0
        /// </summary>
        public VectorD Nx => Kx / K0;

        /// <summary>
        /// normalized transverse spatial frequencies
        /// Ny = Ky / K0
        /// </summary>
        public VectorD Ny => Ky / K0;

        /// <summary>
        /// normalized eigenvalues Nz = Kz/K0 
        /// </summary>
        public MatrixZ Nz { get; set; }

        /// <summary>
        /// eigenvector / mode parameter
        /// </summary>
        public MatrixZ W { get; set; }

        /// <summary>
        /// Kz = K0 * Nz
        /// </summary>
        public MatrixZ Kz => K0 * Nz;

        /// <summary>
        /// eigenvalues gamma
        /// </summary>
        public MatrixZ Gamma => Complex.ImaginaryOne * K0 * Nz;

        /// <summary>
        /// complex E field component
        /// </summary>
        public MatrixZ E { get; set; }

        /// <summary>
        /// complex H field component
        /// </summary>
        public MatrixZ H { get; set; }

        // Poynting vector components
        /// <summary>
        /// x-component of the Poynting vector
        /// </summary>
        public MatrixD Sx { get; set; }

        /// <summary>
        /// y-component of the Poynting vector
        /// </summary>
        public MatrixD Sy { get; set; }

        /// <summary>
        /// z-component of the Poynting vector
        /// </summary>
        public MatrixD Sz { get; set; }

        #endregion
        #region constructor

        /// <summary>
        /// constructs an EMField
        /// </summary>
        /// <param name="wavelength"> wavelength in vacuum </param>
        /// <param name="epsilon"> relative permittivity </param>
        /// <param name="gridInfo"> 1D sampling grid information </param>
        /// <param name="fieldValues"> vector that contains the field values </param>
        /// <param name="domain"> modeling domain in which the field is specified </param>
        /// <param name="direction"> sign factor for the direction </param>
        /// <param name="polarization"> polarization mode either TE or TM </param>
        public EMField(double wavelength, Complex epsilon,
            GridInfo2D gridInfo, MatrixZ fieldValues,
            ModelingDomain domain = ModelingDomain.Spatial,
            SignFactor direction = SignFactor.Positive,
            PolarizationMode polarization = PolarizationMode.TE)
            : base(wavelength, epsilon)
        {
            _gammaReady = false;
            _eigenInfoReady = false;

            Direction = direction;
            Polarization = polarization;
            Domain = domain;

            GridInfo = gridInfo;
            E = fieldValues;
        }

        #endregion
        #region methods

        #region eigen info
        /// <summary>
        /// computes the eigenvalue gamma
        /// </summary>
        public void ComputeGamma()
        {
            // check domain
            GridInfo2D gridInfo = new(GridInfo);
            if (Domain == ModelingDomain.Spatial)
                gridInfo.GetConjugated(isForward: true); //= GridInfo.ComputeConjugateInfo();
            else
                gridInfo = GridInfo;
            // compute Kx
            Kx = ComputeKx(gridInfo);
            Ky = ComputeKy(gridInfo);
            // compute Nz
            Nz = ComputeNz(Nx, Ny, Epsilon, Mu);
            // change the gamma flag
            _gammaReady = true;
        }

        /*/// <summary>
        /// computes all the eigen information
        /// eigenvalue: gamma 
        /// eigenvector parameter: w
        /// </summary>
        public void ComputeEigenInfo()
        {
            // check gamma
            if (_gammaReady == false)
                ComputeGamma();

            // compute W
            if (Polarization == PolarizationMode.TE)
                W = ComputeWTE(Nz, Mu);
            else
                W = ComputeWTM(Nz, Epsilon);
            // change the eigen info flag
            _eigenInfoReady = true;
        }*/

        /// <summary>
        /// computes kx coordinates from grid sampling information
        /// </summary>
        /// <param name="gridInfo"> grid information in the spatial frequency domain </param>
        /// <returns> kx coordinates </returns>
        private static VectorD ComputeKx(GridInfo2D gridInfo)
            => gridInfo.GetCoordinatesX();

        /// <summary>
        /// computes ky coordinates from grid sampling information
        /// </summary>
        /// <param name="gridInfo"> grid information in the spatial frequency domain </param>
        /// <returns> ky coordinates </returns>
        private static VectorD ComputeKy(GridInfo2D gridInfo)
            => gridInfo.GetCoordinatesY();

        /// <summary>
        /// computes nz using dispersion relation
        /// </summary>
        /// <param name="nx"> nx = kx / k0 </param>
        /// <param name="ny"> nx = ky / k0 </param>
        /// <param name="epsilon"> permittivity </param>
        /// <param name="mu"> permeability </param>
        /// <returns> nz vector </returns>
        private static MatrixZ ComputeNz(VectorD nx, VectorD ny,
            Complex epsilon, Complex mu)
        {
            Complex pem = epsilon * mu;
            MatrixZ nz = new(ny.Count, nx.Count);
            Parallel.For(0, nz.Rows, iRow =>
            {
                double iny2 = ny[iRow];
                iny2 *= iny2;
                for (long iCol = 0; iCol < nz.Cols; iCol++)
                {
                    double inx2 = nx[iCol];
                    inx2 *= inx2;
                    nz[iRow, iCol] = Complex.Sqrt(pem - inx2 - iny2);
                }
            });
            return nz;
        }

        /*/// <summary>
        /// compute eigenvector parameter w for TE mode
        /// </summary>
        /// <param name="nz"> nz vector </param>
        /// <param name="mu"> permeability </param>
        /// <returns> w vector </returns>
        private static VectorZ ComputeWTE(VectorZ nz,
            Complex mu) => -nz / mu;

        /// <summary>
        /// compute eigenvector parameter w for TM mode
        /// </summary>
        /// <param name="nz"> nz vector </param>
        /// <param name="epsilon"> permittivity </param>
        /// <returns> w vector </returns>
        private static VectorZ ComputeWTM(VectorZ nz,
            Complex epsilon) => epsilon / nz;*/

        #endregion
        #region transform
        /// <summary>
        /// transform from current domain to the other
        /// changes E, GridInfo, Domain
        /// </summary>
        public void Transform()
        {
            if (Domain == ModelingDomain.Spatial)
            {
                // perform forward transform
                MatrixZ t = E;
                MathCore.Transform.FFT2D(x: ref t,
                    scalFac: GridInfo.SpacingX * GridInfo.SpacingY / (2.0 * Math.PI), // !!!
                    direction: FFTOptions.Direction.Forward);
                // define the grid info in the target domain
                GridInfo.GetConjugated(isForward: true); // = GridInfo.ComputeConjugateInfo();
                // change the domain flag
                Domain = ModelingDomain.SpatialFrequency;
            }
            else
            {
                // perform backward transform
                MatrixZ t = E;
                MathCore.Transform.FFT2D(x: ref t,
                    scalFac: GridInfo.SpacingX * GridInfo.SpacingY / (2.0 * Math.PI), // !!!
                    direction: FFTOptions.Direction.Backward);
                // define the grid info in the target domain
                GridInfo.GetConjugated(isForward: false); // = GridInfo.ComputeConjugateInfo();
                // change the domain flag
                Domain = ModelingDomain.Spatial;
            }
        }

        #endregion
        #region propagate
        /// <summary>
        /// propagation in the spatial frequency domain
        /// </summary>
        /// <param name="d"> propagation distance along z-axis </param>
        public void Propagate(double d)
        {
            if (Domain == ModelingDomain.Spatial)
                Transform();// transform to spatial frequency domain

            // computes gamma
            ComputeGamma();
            // multiply phase term
            double k0D = K0 * d;
            //Parallel.For(0, E.Rows, iRow =>
            for (long iRow = 0; iRow < E.Rows; iRow++)
            {
                for (long iCol = 0; iCol < E.Cols; iCol++)
                    E[iRow, iCol] *= Complex.Exp(Complex.ImaginaryOne * k0D * Nz[iRow, iCol]);
            }//);
        }

        #endregion

        #endregion

    }

}
