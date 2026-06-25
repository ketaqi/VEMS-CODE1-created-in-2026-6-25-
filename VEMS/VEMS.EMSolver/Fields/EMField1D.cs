using System.Numerics;
using VEMS.MathCore;

namespace VEMS.EMSolver
{
    /// <summary>
    /// EM field class
    /// special version in XZ
    /// e.g., Ey(x,z), Hx(x, z)
    /// </summary>
    public class EMField1D : FieldBase
    {
        #region properties

        /// <summary>
        /// flag indicating whether the eigen info is ready for use or not
        /// </summary>
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
        public GridInfo1D GridInfo { get; set; }

        /// <summary>
        /// transverse spatial frequencies
        /// </summary>
        public VectorD Kx { get; set; }

        /// <summary>
        /// normalized transverse spatial frequencies
        /// Nx = Kx / K0
        /// </summary>
        public VectorD Nx => Kx / K0;

        /// <summary>
        /// normalized eigenvalues Nz = Kz/K0 
        /// </summary>
        public VectorZ Nz { get; set; }

        /// <summary>
        /// eigenvector / mode parameter
        /// </summary>
        public VectorZ W { get; set; }

        /// <summary>
        /// Kz = K0 * Nz
        /// </summary>
        public VectorZ Kz => K0 * Nz;

        /// <summary>
        /// eigenvalues gamma
        /// </summary>
        public VectorZ Gamma => Complex.ImaginaryOne * Kz;

        /// <summary>
        /// complex field coefficients
        /// equals to E field in the k-domain
        /// </summary>
        [Obsolete("Probably not needed ...")]
        private VectorZ C { get; set; }

        /// <summary>
        /// complex E field component
        /// </summary>
        public VectorZ E { get; set; }

        /// <summary>
        /// complex H field component
        /// </summary>
        public VectorZ H { get; set; }

        // Poynting vector components
        /// <summary>
        /// x-component of the Poynting vector
        /// </summary>
        public VectorD Sx { get; set; }

        /// <summary>
        /// y-component of the Poynting vector
        /// </summary>
        public VectorD Sy { get; set; }

        /// <summary>
        /// z-component of the Poynting vector
        /// </summary>
        public VectorD Sz { get; set; }

        #endregion
        #region constructors

        /// <summary>
        /// constructs an EMFieldXZ
        /// </summary>
        /// <param name="wavelength"> wavelength in vacuum </param>
        /// <param name="epsilon"> relative permittivity </param>
        /// <param name="gridInfo"> 1D sampling grid information </param>
        /// <param name="fieldValues"> vector that contains the field values </param>
        /// <param name="domain"> modeling domain in which the field is specified </param>
        /// <param name="direction"> sign factor for the direction </param>
        /// <param name="polarization"> polarization mode either TE or TM </param>
        public EMField1D(double wavelength, Complex epsilon,
            GridInfo1D gridInfo, VectorZ fieldValues,
            ModelingDomain domain = ModelingDomain.Spatial,
            SignFactor direction = SignFactor.Positive,
            PolarizationMode polarization = PolarizationMode.TE) : base(wavelength, epsilon)
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
            GridInfo1D gridInfo = new(other: GridInfo);
            if (Domain == ModelingDomain.Spatial)
                //gridInfo.GetConjugated(); // = GridInfo.ComputeConjugateInfo();
                gridInfo.GetConjugated(isForward: true);
            else
                gridInfo = GridInfo;
            // compute Kx
            Kx = ComputeKx(gridInfo);
            // compute Nz
            Nz = ComputeNz(Nx, Epsilon, Mu);
            // change the gamma flag
            _gammaReady = true;
        }

        /// <summary>
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
        }

        /// <summary>
        /// computes kx coordinates from grid sampling information
        /// </summary>
        /// <param name="gridInfo"> grid information in the spatial frequency domain </param>
        /// <returns> kx coordinates </returns>
        private static VectorD ComputeKx(GridInfo1D gridInfo)
            => gridInfo.GetCoordinates();

        /// <summary>
        /// computes nz using dispersion relation
        /// </summary>
        /// <param name="nx"> nx = kx / k0 </param>
        /// <param name="epsilon"> permittivity </param>
        /// <param name="mu"> permeability </param>
        /// <returns> nz vector </returns>
        private static VectorZ ComputeNz(VectorD nx,
            Complex epsilon, Complex mu)
        {
            Complex pem = epsilon * mu;
            VectorZ nz = new(nx.Count);
            Parallel.For(0, nz.Count, i =>
            {
                double inx = nx[i];
                nz[i] = Complex.Sqrt(pem - inx * inx);
            });
            return nz;
            //LinearAlgebra linAlg = new();
            //return linAlg.Sqrt(epsilon * mu - linAlg.Square(nx));
        }

        /// <summary>
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
            Complex epsilon) => epsilon / nz;

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
                VectorZ t = E;
                MathCore.Transform.FFT1D(x: ref t,
                    scalFac: GridInfo.Spacing / Math.Sqrt(2.0 * Math.PI), // !!!
                    direction: FFTOptions.Direction.Forward);
                // define the grid info in the target domain
                GridInfo.GetConjugated(isForward: true); // = GridInfo.ComputeConjugateInfo();
                // change the domain flag
                Domain = ModelingDomain.SpatialFrequency;
            }
            else
            {
                // perform backward transform
                //MathCore.Transform.FFT(E, FFTOption.Backward,
                //    GridInfo.Spacing / Math.Sqrt(2.0 * Math.PI));
                VectorZ t = E;
                MathCore.Transform.FFT1D(x: ref t,
                    scalFac: GridInfo.Spacing / Math.Sqrt(2.0 * Math.PI),
                    direction: FFTOptions.Direction.Backward);
                // define the grid info in the target domain
                GridInfo.GetConjugated(isForward: false); // = GridInfo.ComputeConjugateInfo();
                // change the domain flag
                Domain = ModelingDomain.Spatial;
            }
        }

        ///// <summary>
        ///// forward Fourier transform wrapper
        ///// </summary>
        ///// <param name="E"> data (input and output) </param>
        ///// <param name="scalFac"> scaling factor </param>
        //private static void ForwardTransform(VectorZ E, double scalFac)
        //    => new LinearAlgebra().ForwardTransform1D(ref E, scalFac);

        ///// <summary>
        ///// backward Fourier transform wrapper
        ///// </summary>
        ///// <param name="E"> data (input and output) </param>
        ///// <param name="scalFac"> scaling factor </param>
        //private static void BackwardTransform(VectorZ E, double scalFac)
        //    => new LinearAlgebra().BackwardTransform1D(ref E, scalFac);

        #endregion
        #region propagators
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
            //Parallel.For(0, E.Count, i =>
            //{ E[i] *= Complex.Exp(Complex.ImaginaryOne * k0D * Nz[i]); });
            for (long i = 0; i < E.Count; i++)
                E[i] *= Complex.Exp(Complex.ImaginaryOne * k0D * Nz[i]);

        }

        /// <summary>
        /// computes propagated field value at a given point
        /// assuming the input field is located at x = 0, z = 0
        /// </summary>
        /// <param name="z"> distance along z-axis </param>
        /// <param name="x"> distance along x-axis </param>
        public Complex PropagateToPoint(double z, double x)
        {
            if (Domain == ModelingDomain.Spatial)
                Transform();// transform to spatial frequency domain
            // get dKx
            double dKx = GridInfo.Spacing;
            // loop sum
            Complex res = 0.0;
            for (long i = 0; i < E.Count; i++)
            {
                double kx = GridInfo.GetCoordinate(i);
                Complex kz = Complex.Sqrt(K0 * K0 * Epsilon * Mu - kx * kx);
                res += E[i] * Complex.Exp(Complex.ImaginaryOne * kx * x)
                    * Complex.Exp(Complex.ImaginaryOne * kz * z) * dKx / Math.Sqrt(2.0 * Math.PI);
            }
            return res;
        }

        /// <summary>
        /// propagates onto a tilted plane at a given distance
        /// and a given rotation angle
        /// </summary>
        /// <param name="distance"> propagation distance </param>
        /// <param name="rotAngle"> rotation angle </param>
        /// <param name="targetGrid"> sampling info on the plane after rotation</param>
        /// <returns> result in a vector </returns>
        public VectorZ PropagateToTiltPlane(double distance, double rotAngle, GridInfo1D targetGrid)
        {
            VectorZ res = new(targetGrid.Count, 0.0);
            // loop over all points of target grid
            for (long i = 0; i < targetGrid.Count; i++)
            {
                double tx = targetGrid.GetCoordinate(i);
                double dx = tx * Math.Cos(rotAngle);
                double dz = tx * Math.Sin(rotAngle);
                double x = 0.0 + dx;
                double z = distance + dz;
                res[i] = PropagateToPoint(z, x);
            }
            return res;
        }

        #endregion

        #endregion

    }
}
