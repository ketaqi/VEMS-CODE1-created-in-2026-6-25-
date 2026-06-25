using VEMS.MathCore;
using Complex = System.Numerics.Complex;

namespace VEMS.EMSolver
{
    /// <summary>
    /// plane wave class 
    /// general version in 3D
    /// </summary>
    public class PlaneWave : FieldBase
    {
        #region properties

        ///// <summary>
        ///// sign factor according to direction
        ///// +1 for positive direction
        ///// -1 for negative direction
        ///// </summary>
        //private double SignFactor { get; set; }

        /// <summary>
        /// sign factor according to direction
        /// !!! obsolete ???
        /// </summary>
        public SignFactor Direction { get; set; }

        /// <summary>
        /// x-component of spatial frequency
        /// </summary>
        public double Kx { get; set; }

        /// <summary>
        /// y-component of spatial frequency
        /// </summary>
        public double Ky { get; set; }

        /// <summary>
        /// z-component of spatial frequency
        /// </summary>
        public Complex Kz { get; set; } //=> SignFactor *
                                        //Complex.Sqrt(Epsilon * Mu * K0 * K0 - Kx * Kx - Ky * Ky);

        ///// <summary>
        ///// normalized quantity nx = kx / k0
        ///// </summary>
        //private double nx { get => Kx / K0; }

        ///// <summary>
        ///// normalized quantity ny = ky / k0
        ///// </summary>
        //private double ny { get => Ky / K0; }

        ///// <summary>
        ///// normalized quantity nz = kz / k0
        ///// </summary>
        //private Complex nz { get => Kz / K0; }

        /// <summary>
        /// eigenvector / mode 
        /// since W1 = 1.0 only W2 is needed and W is W2
        /// here W is a 2x2 matrix
        /// </summary>
        public MatrixZ? W { get; set; }

        /// <summary>
        /// eigenvector parameter wB
        /// </summary>
        public Complex Wb { get; set; } //=> nx * ny / (Mu * nz);

        /// <summary>
        /// eigenvector parameter wC
        /// </summary>
        public Complex Wc { get; set; } //=> nz / Mu + nx * nx / (Mu * nz);

        /// <summary>
        /// eigenvector parameter wD
        /// </summary>
        public Complex Wd { get; set; } //=> nz / Mu + ny * ny / (Mu * nz);

        /// <summary>
        /// complex field component Ex
        /// equals to the mode coefficient
        /// </summary>
        public Complex Ex { get; set; }

        /// <summary>
        /// complex field component Ey
        /// equals to the mode coefficient
        /// </summary>
        public Complex Ey { get; set; }

        /// <summary>
        /// complex field component Hx
        /// </summary>
        public Complex Hx { get; set; } //=> -(Ex * wB + Ey * wD);

        /// <summary>
        /// get complex field component Hy
        /// </summary>
        public Complex Hy { get; set; } //=> Ex * wC + Ey * wB;
        //public Complex Hz { get; }

        ///// <summary>
        ///// complex field component Ez
        ///// </summary>
        //public Complex Ez { get; set; } //=> -(nx * Hy - ny * Hx) / Epsilon;

        ///// <summary>
        ///// complex field component Hz
        ///// </summary>
        //public Complex Hz { get; set; } //=> (nx * Ey - ny * Ex) / Mu;

        ///// <summary>
        ///// x-component of the Poynting vector
        ///// </summary>
        //public double Sx { get; }

        ///// <summary>
        ///// y-component of the Poynting vector
        ///// </summary>
        //public double Sy { get; }

        ///// <summary>
        ///// z-component of the Poynting vector
        ///// </summary>
        //public double Sz { get; set; }
        //{
        //    get
        //    {
        //        Complex cEH = Ex * Complex.Conjugate(Hy) - Ey * Complex.Conjugate(Hx);
        //        return 0.5 * cEH.Real;
        //    }
        //}

        #endregion
        #region constructor

        /// <summary>
        /// default empty constructor
        /// </summary>
        public PlaneWave() { }

        /// <summary>
        /// constructs a PlaneWave 
        /// with given wavelength and
        /// transverse spatial frequencies kx, ky
        /// </summary>
        /// <param name="wavelength"> wavelength in vacuum </param>
        /// <param name="epsilon"> relative permitivvity @wavelength </param>
        /// <param name="mu"> relative permeability @wavelength </param>
        /// <param name="kx"> transverse spatial frequency kx </param>
        /// <param name="ky"> transverse spatial frequency ky </param>
        /// <param name="direction"> sign factor for the direction </param>
        /// <param name="initializeEigenInfo"> whether to compute the eigen info when constructed </param>
        public PlaneWave(double wavelength, 
            Complex epsilon, Complex mu, double kx, double ky,
            SignFactor direction = SignFactor.Positive,
            bool initializeEigenInfo = true)
            : base(wavelength, epsilon, mu)
        {
            // gets input parameters
            Kx = kx;
            Ky = ky;
            Direction = direction;

            // initializes eigen properties
            if (initializeEigenInfo) { ComputeEigenInfo(); }
            // initializes E-field values
            Ex = 1.0;
            Ey = 1.0;
        }

        /// <summary>
        /// constructs a PlaneWave 
        /// with given wavelength and
        /// transverse spatial frequencies kx, ky
        /// </summary>
        /// <param name="wavelength"> wavelength in vacuum </param>
        /// <param name="n"> complex refractive index @wavelength </param>
        /// <param name="kx"> transverse spatial frequency kx </param>
        /// <param name="ky"> transverse spatial frequency ky </param>
        /// <param name="direction"> sign factor for the direction </param>
        /// <param name="initializeEigenInfo"> whether to compute the eigen info when constructed </param>
        public PlaneWave(double wavelength, 
            Complex n, double kx, double ky,
            SignFactor direction = SignFactor.Positive,
            bool initializeEigenInfo = true)
            : this(wavelength: wavelength, epsilon: n * n, mu: 1.0, 
                  kx: kx, ky: ky, direction: direction,
                  initializeEigenInfo: initializeEigenInfo)
        { }

        #endregion
        #region methods

        /// <summary>
        /// computes the eigen information 
        /// for the current plane wave
        /// </summary>
        public void ComputeEigenInfo()
        {
            // using UniformLayer for EigenInfo calculation
            UniformLayer freeSpace = new(epsilon: Epsilon, mu: Mu, thickness: 0.0);
            (Complex nz, _, MatrixZ w2) = freeSpace.ComputeConicalModes(wavelength: Wavelength,
                nx: Kx / K0, ny: Ky / K0);
            // saves the eigen-info
            Kz = K0 * nz;
            W = w2;
        }

        /// <summary>
        /// computes the z-component of the Poynting vector
        /// </summary>
        /// <returns> Sz-component</returns>
        public double ComputeSz()
        {
            if(Ex == Complex.NaN || Ey == Complex.NaN) { return double.NaN; }
            if(W == null) { throw new ArgumentNullException(nameof(W)); }
            // computes H first
            VectorZ ExEy = new (count: 2) { [0] = Ex, [1] = Ey };
            VectorZ HxHy = LinAlg.Dot(W, ExEy);
            Hx = HxHy[0]; 
            Hy = HxHy[1];
            Complex cEH = Ex * Complex.Conjugate(Hy) - Ey * Complex.Conjugate(Hx);
            return 0.5 * cEH.Real;
        }

        #endregion
    }



}
