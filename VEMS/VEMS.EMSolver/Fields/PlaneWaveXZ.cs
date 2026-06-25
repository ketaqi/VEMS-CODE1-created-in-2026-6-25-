using Complex = System.Numerics.Complex;

namespace VEMS.EMSolver
{
    /// <summary>
    /// plane wave class
    /// special version in XZ space
    /// </summary>
    public class PlaneWaveXZ : FieldBase
    {
        #region properties

        ///// <summary>
        ///// flag indicating whether the eigen info is ready for use or not
        ///// </summary>
        //private bool _eigenInfoReady { get; set; }

        /// <summary>
        /// sign factor according to direction
        /// !!! obsolete ???
        /// </summary>
        public SignFactor Direction { get; set; }

        /// <summary>
        /// polarization mode
        /// TE or TM mode
        /// </summary>
        public InPlanePolMode Polarization { get; set; }

        /// <summary>
        /// transverse spatial frequency
        /// here we let ky = 0.0
        /// </summary>
        public double Kx { get; set; }

        ///// <summary>
        ///// normalized transverse spatial frequency
        ///// Nx = Kx / K0
        ///// </summary>
        //public double Nx => Kx / K0;

        /// <summary>
        /// longitudinal spatial frequency 
        /// Kz = K0 * Nz
        /// </summary>
        public Complex Kz { get; set; }

        ///// <summary>
        ///// normalized propagation constant
        ///// Nz = Kz / K0
        ///// </summary>
        //public Complex Nz { get; set; }

        ///// <summary>
        ///// eigenvalue gamma
        ///// </summary>
        //public Complex Gamma => Complex.ImaginaryOne * Kz;

        /// <summary>
        /// eigenvector / mode parameter
        /// since W1 = 1.0 only W2 is needed and W is W2
        /// </summary>
        public Complex W { get; set; }

        ///// <summary>
        ///// eigenvector / mode parameter
        ///// </summary>
        //public Complex Wc { get; set; }

        ///// <summary>
        ///// eigenvector / mode parameter
        ///// </summary>
        //public Complex Wd { get; set; }

        /// <summary>
        /// complex E field component
        /// equals to the mode coefficient C
        /// </summary>
        public Complex E { get; set; }

        /// <summary>
        /// complex H field component
        /// </summary>
        public Complex H { get; set; } // => E * W;

        
        ///// <summary>
        ///// x-component of the Poynting vector
        ///// </summary>
        //public double Sx { get; set; }

        ///// <summary>
        ///// y-component of the Poynting vector
        ///// </summary>
        //public double Sy { get; set; }

        ///// <summary>
        ///// z-component of the Poynting vector
        ///// </summary>
        //public double Sz { get; set; }

        #endregion
        #region constructors

        /// <summary>
        /// default constructor
        /// </summary>
        public PlaneWaveXZ() { }

        /// <summary>
        /// constructs a PlaneWaveXZ without
        /// specifying the field value(s)
        /// </summary>
        /// <param name="wavelength"> wavelength in vacuum </param>
        /// <param name="epsilon"> relative permittivity @wavelength </param>
        /// <param name="mu"> relative permeability @wavelength </param>
        /// <param name="kx"> transverse spatial frequency </param>
        /// <param name="polMode"> polarization mode either TE or TM </param>
        /// <param name="direction"> sign factor for the direction </param>
        /// <param name="initializeEigenInfo"> whether to compute the eigen info when constructed </param>
        public PlaneWaveXZ(double wavelength, 
            Complex epsilon, Complex mu, double kx,
            InPlanePolMode polMode,
            SignFactor direction = SignFactor.Positive,
            bool initializeEigenInfo = true)
            : base(wavelength, epsilon, mu)
        {
            // gets input parameters
            Kx = kx;
            Direction = direction;
            Polarization = polMode;

            // initializes eigen properties
            if (initializeEigenInfo) { ComputeEigenInfo(); }
            // initializes E-field value
            E = 1.0;
        }

        /// <summary>
        /// constructs a PlaneWaveXZ without
        /// specifying the field value(s)
        /// </summary>
        /// <param name="wavelength"> wavelength in vacuum </param>
        /// <param name="n"> complex refractive index @wavelength </param>
        /// <param name="kx"> transverse spatial frequency </param>
        /// <param name="polMode"> polarization mode either TE or TM </param>
        /// <param name="direction"> sign factor for the direction </param>
        /// <param name="initializeEigenInfo"> whether to compute the eigen info when constructed </param>
        public PlaneWaveXZ(double wavelength,
            Complex n, double kx,
            InPlanePolMode polMode,
            SignFactor direction = SignFactor.Positive,
            bool initializeEigenInfo = true)
            : this(wavelength: wavelength, epsilon: n * n, mu: 1.0, kx: kx,
                  direction: direction, polMode: polMode,
                  initializeEigenInfo: initializeEigenInfo)
        { }

        #endregion
        #region methods

        ///// <summary>
        ///// computes the eigen information
        ///// for the current plane wave for 
        ///// both polarization modes
        ///// </summary>
        //internal void ComputeEigenInfoFull()
        //{
        //    (Complex nz, Complex wc, Complex wd) = 
        //        FreeSpace.ComputeInPlaneModes(epsilon: Epsilon, mu: Mu, 
        //        nx: Kx / K0,
        //        direction: Direction);
        //    Kz = K0 * nz;
        //    Wc = wc;
        //    Wd = wd;
        //}

        /// <summary>
        /// computes the eigen information 
        /// for the current plane wave with
        /// specific polarization mode
        /// </summary>
        public void ComputeEigenInfo()
        {
            // constructs UniformLayer for EigenInfo calculation
            UniformLayer freeSpace = new(epsilon: Epsilon, mu: Mu, thickness: 0.0);
            (Complex nz, _, Complex w2) = freeSpace.ComputeInPlaneModes(wavelength: Wavelength,
                nx: Kx / K0, mode: Polarization);

            // saves the eigen-info
            Kz = K0 * nz;
            W = w2;
        }

        ///// <summary>
        ///// computes the H-field
        ///// </summary>
        //public void ComputeHField()
        //{
        //    if (_eigenInfoReady == false)
        //        ComputeEigenInfo();
        //    if (E != Complex.NaN)
        //        H = E * W;
        //}

        ///// <summary>
        ///// computes the Poynting vector
        ///// </summary>
        //private void ComputePoyntingVector()
        //{
        //    // ...
        //}

        /// <summary>
        /// computes the z-component of the Poynting vector
        /// </summary>
        /// <returns> Sz-component </returns>
        public double ComputeSz()
        {
            if(E == Complex.NaN || W == Complex.NaN) { return double.NaN; }
            // computes H first
            H = E * W; // !!! W can be NaN ...
            Complex cEH = Polarization switch
            {
                InPlanePolMode.TE => -E * Complex.Conjugate(H), // 
                InPlanePolMode.TM => E * Complex.Conjugate(H),
                _ => -E * Complex.Conjugate(H)
            };
            return 0.5 * cEH.Real;
        }

        /// <summary>
        /// propagates by a given distance
        /// along the z-direction
        /// </summary>
        /// <param name="d"> propagation distance </param>
        public void Propagate(double d)
        {
            if (E != Complex.NaN)
                E *= Complex.Exp(Complex.ImaginaryOne * Kz * d);
        }

        #endregion
    }

}
