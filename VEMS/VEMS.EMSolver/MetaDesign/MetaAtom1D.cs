using VEMS.MathCore;
using Complex = System.Numerics.Complex;

namespace VEMS.EMSolver
{
    /// <summary>
    /// meta-atom with 1D varying permittivity/permeability distribution
    /// </summary>
    public class MetaAtom1D : Layer1DMedium
    {
        #region properties

        /// <summary>
        /// period of the meta-atom / unit cell
        /// </summary>
        public double Period { get; set; }

        /// <summary>
        /// height of the meta-atom / unit cell
        /// </summary>
        public double Height { get; set; }

        ///// <summary>
        ///// material in front of the mate-layer
        ///// on the incidence/reflection side
        ///// </summary>
        //public Material? MaterialFront { get; set; }

        ///// <summary>
        ///// material behind the meta-layer
        ///// on the transmission side
        ///// </summary>
        //public Material? MaterialBehind { get; set; }

        #endregion
        #region constructors 

        /// <summary>
        /// default constructor
        /// </summary>
        internal MetaAtom1D() { }

        /// <summary>
        /// constructs a meta-atom with permittvity/permeability definition
        /// </summary>
        /// <param name="period"> period of the meta-layer </param>
        /// <param name="height"> height of the meta-layer </param>
        /// <param name="epsilon"> permittivity distribution in the meta-layer </param>
        /// <param name="mu"> permeability distribution in the meta-layer </param>
        public MetaAtom1D(double period, double height,
            Func<double, double, Complex> epsilon,
            Func<double, double, Complex>? mu = null)
            : base(epsilon, mu)
        {
            Period = period;
            Height = height;
        }

        /// <summary>
        /// constructs a meta-atom with refractive index definition
        /// </summary>
        /// <param name="period"> period of the meta-layer </param>
        /// <param name="height"> height of the meta-layer </param>
        /// <param name="n"> refractive index distribution in the meta-layer </param>
        public MetaAtom1D(double period, double height,
            Func<double, double, Complex> n)
            : base(n)
        {
            Period = period;
            Height = height;
        }

        #endregion
        #region methods

        /// <summary>
        /// samples the permittivity or permeability distribution
        /// on a uniform grid with specific number of samples
        /// </summary>
        /// <param name="wavelength"> wavelength in vacuum </param>
        /// <param name="n"> number of sampling points </param>
        /// <param name="matProperty"> material property option </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <param name="saveSampledData"> saves the sampled data or not </param>
        /// <returns> sampled permittivity or permeability on the uniform grid </returns>
        public (VectorZ, GridInfo1D) Sample(double wavelength, long n,
            MaterialProperty matProperty = EMDefaults.MatProperty,
            LoopMode loopMode = Defaults.LoopOption,
            bool saveSampledData = false)
        {
            // constructs uniform sampling grid
            GridInfo1D g = new (n: n, spacing: Period/n);
            // calls base method
            VectorZ v = Sample(wavelength: wavelength, 
                grid: g, 
                matProperty: matProperty,
                loopMode: loopMode, 
                cacheSampleData: saveSampledData);
            return (v, g);
        }

        /// <summary>
        /// creates RCWA1Dp solver for this meta-atom
        /// at fixed wavelength and polarization for given front and behind materials
        /// </summary>
        /// <param name="wavelength"> working wavelength given in vacuum </param>
        /// <param name="polarization"> in-plane polarization mode option </param>
        /// <param name="front"> material of the front layer </param>
        /// <param name="behind"> material of the behind layer </param>
        /// <returns> resulting RCWA1Dp solver </returns>
        public RCWA1Dp CreateSolver(double wavelength,
            InPlanePolMode polarization,
            Material front, Material behind)
            => new(wavelength: wavelength,
                polarization: polarization,
                materialFront: front,
                mediumMiddle: this, period: Period, thickness: Height,
                materialBehind: behind);

        /// <summary>
        /// computes the 0th-order efficiency and phase change
        /// for an incident plane wave with specific parameters
        /// </summary>
        /// <param name="wavelength"> wavelength in vacuum </param>
        /// <param name="polarization"> in-plane polarization mode option </param>
        /// <param name="front"> material in front of the meta-layer </param>
        /// <param name="behind"> material behind the meta-layer </param>
        /// <param name="isTransmission"> whether to calculate transmission or reflection </param>
        /// <param name="kx0"> shift of spatial frequency kx </param>
        /// <param name="fieldsSampling"> sampling number for E/H-fields </param>
        /// <param name="mediumSampling"> sampling number for medium </param>
        /// <returns> (efficiency, phase change) </returns>
        public (double, double) ComputeModulation(double wavelength,
            InPlanePolMode polarization,
            Material front, Material behind,
            bool isTransmission,
            double kx0 = 0.0,
            long fieldsSampling = 0,
            long mediumSampling = 0)
        {
            // constructs RCWA solver & compute S-matrix
            RCWA1Dp solver = CreateSolver(wavelength: wavelength, 
                polarization: polarization,
                front: front, behind: behind);
            solver.ComputeHalfSMatrix(kx0: kx0,
                fieldsSampling: fieldsSampling,
                mediumSampling: mediumSampling);
            // computes coefficients for plane wave incidence
            PlaneWaveXZ pIn = new(wavelength: wavelength,
                n: front.N(wavelength), kx: kx0, polMode: polarization);
            VectorZ c = isTransmission ? solver.ComputeTCoefficients(pw: pIn).Item1 :
                solver.ComputeRCoefficients(pw: pIn).Item1;
            // converts from coefficients to plane waves, and takes only 0th order
            long idx = (c.Count - 1) / 2 + 0; // 0-th order index
            Material outputMaterial = isTransmission ? behind : front;
            PlaneWaveXZ pOut = RCWAHelper.CoefficientToPlaneWave(c: c[idx], period: Period,
                wavelength: wavelength, epsilon: outputMaterial.Epsilon(wavelength),
                mu: (outputMaterial.Mu == null) ? 1.0 : outputMaterial.Mu(wavelength),
                kx: kx0, polarization: polarization);
            // efficiency and phase
            double dPhase = pOut.E.Phase - pIn.E.Phase;
            double efficiency = pOut.ComputeSz() / pIn.ComputeSz();
            // return
            return (efficiency, dPhase);
        }

        /// <summary>
        /// computes the 0th-order complex coefficients
        /// for an incident plane wave with specific parameters
        /// </summary>
        /// <param name="wavelength"> wavelength in vacuum </param>
        /// <param name="polarization"> in-plane polarization mode option </param>
        /// <param name="front"> material in front of the meta-layer </param>
        /// <param name="behind"> material behind the meta-layer </param>
        /// <param name="isTransmission"> whether to calculate transmission or reflection </param>
        /// <param name="kx0"> shift of spatial frequency kx </param>
        /// <param name="fieldsSampling"> sampling number for E/H-fields </param>
        /// <param name="mediumSampling"> sampling number for medium </param>
        /// <returns> complex coefficients, either transmission or reflection </returns>
        public Complex ComputeCplxModulation(double wavelength,
            InPlanePolMode polarization,
            Material front, Material behind,
            bool isTransmission,
            double kx0 = 0.0,
            long fieldsSampling = 0,
            long mediumSampling = 0)
        {
            // constructs RCWA solver & compute S-matrix
            RCWA1Dp solver = CreateSolver(wavelength: wavelength,
                polarization: polarization,
                front: front, behind: behind);
            solver.ComputeHalfSMatrix(kx0: kx0,
                fieldsSampling: fieldsSampling,
                mediumSampling: mediumSampling);
            // compute coefficients for central spatial frequency
            MatrixZ s = isTransmission ? solver.S11![kx0] : solver.S21![kx0];
            long n = s.Rows;
            VectorZ cIn = new(count: n);
            cIn[(n - 1) / 2, false] = 1.0;
            VectorZ cOut = LinAlg.Dot(s, cIn);
            return cOut[(n - 1) / 2, false];
        }

        // generates look-up tables ...

        #endregion
        #region sub-classes

        /// <summary>
        /// meta-atom with 1D rectangular geometry
        /// </summary>
        public class Rect : MetaAtom1D
        {
            #region properties

            /// <summary>
            /// diameter of the rectangle 
            /// </summary>
            public double Diameter { get; set; }

            /// <summary>
            /// embedding material outside the rectangular region
            /// </summary>
            public Material MaterialEmbed { get; set; }

            /// <summary>
            /// filling material inside the rectangular region
            /// </summary>
            public Material MaterialFill { get; set; }

            #endregion
            #region constructor

            /// <summary>
            /// constructs a 1D rectangular meta-atom
            /// </summary>
            /// <param name="period"> period of the meta-layer </param>
            /// <param name="height"> height of the meta-layer </param>
            /// <param name="diameter"> diameter of the rectangle </param>
            /// <param name="materialEmbed"> embedding material outside the rectangle </param>
            /// <param name="materialFill"> filling material inside the rectangle </param>
            public Rect(double period, double height,
                double diameter,
                Material materialEmbed, Material materialFill)
            {
                // basic geometry
                Period = period;
                Height = height;
                // specific parameters
                Diameter = diameter;
                MaterialEmbed = materialEmbed;
                MaterialFill = materialFill;
                // preparation ...
                Epsilon = (w, x) => (Math.Abs(x) <= 0.5 * Diameter) ?
                    materialFill.Epsilon(w) : materialEmbed.Epsilon(w);
                N = (w, x) => Complex.Sqrt(Epsilon(w, x));
                if (materialEmbed.Mu != null || materialFill.Mu != null)
                {
                    materialEmbed.Mu ??= (w) => Complex.One;
                    materialFill.Mu ??= (w) => Complex.One;
                    Mu = (w, x) => (Math.Abs(x) <= 0.5 * Diameter) ?
                        materialFill.Mu(w) : materialEmbed.Mu(w);
                }
            }

            #endregion
        }

        #endregion

    }
}
