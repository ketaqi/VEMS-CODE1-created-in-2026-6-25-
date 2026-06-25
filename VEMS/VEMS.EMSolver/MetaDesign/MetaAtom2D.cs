using VEMS.MathCore;
using Complex = System.Numerics.Complex;

namespace VEMS.EMSolver
{
    /// <summary>
    /// meta-atom with 2D varying permittivity/permeability distribution
    /// </summary>
    public class MetaAtom2D : Layer2DMedium
    {
        #region properties

        /// <summary>
        /// period of the meta-atom/unit-cell along x-direction
        /// </summary>
        public double PeriodX { get; set; }

        /// <summary>
        /// period of the meta-atom/unit-cell along y-direction
        /// </summary>
        public double PeriodY { get; set; }

        /// <summary>
        /// height of the meta-atom/unit-cell
        /// </summary>
        public double Height { get; set; }

        #endregion
        #region constructors

        /// <summary>
        /// default constructor
        /// </summary>
        internal MetaAtom2D() { }

        /// <summary>
        /// constructs a meta-atom with permittvity/permeability definition
        /// </summary>
        /// <param name="periodX"> period of the meta-atom along x-direction </param>
        /// <param name="periodY"> period of the meta-atom along y-direction </param>
        /// <param name="height"> height of the meta-atom </param>
        /// <param name="epsilon"> permittivity distribution epsilon = f(λ,x,y) in the meta-layer </param>
        /// <param name="mu"> permeability distribution mu = f(λ,x,y) in the meta-layer </param>
        public MetaAtom2D(double periodX, double periodY, 
            double height,
            Func<double, double, double, Complex> epsilon,
            Func<double, double, double, Complex>? mu = null)
            : base(epsilon, mu)
        {
            PeriodX = periodX;
            PeriodY = periodY;
            Height = height;
        }

        /// <summary>
        /// constructs a meta-atom with refractive index definition
        /// </summary>
        /// <param name="periodX"> period of the meta-atom along x-direction </param>
        /// <param name="periodY"> period of the meta-atom along y-direction </param>
        /// <param name="height"> height of the meta-atom </param>
        /// <param name="n"> refractive index distribution n = f(λ,x,y) in the meta-layer </param>
        public MetaAtom2D(double periodX, double periodY, 
            double height,
            Func<double, double, double, Complex> n)
            : base(n)
        {
            PeriodX = periodX;
            PeriodY = periodY;
            Height = height;
        }

        #endregion
        #region methods

        /// <summary>
        /// samples the permittivity or permeability distribution
        /// on a uniform grid with specific numbers of samples
        /// </summary>
        /// <param name="wavelength"> wavelength in vacuum </param>
        /// <param name="nRows"> number of rows along y-direction </param>
        /// <param name="nCols"> number of columns along x-direction </param>
        /// <param name="matProperty"> material property option </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <param name="saveSampledData"> saves the sampled data or not </param>
        /// <returns> sampled permittivity or permeability on the uniform grid </returns>
        public (MatrixZ, GridInfo2D) Sample(double wavelength, 
            long nRows, long nCols, 
            MaterialProperty matProperty = EMDefaults.MatProperty,
            LoopMode loopMode = Defaults.LoopOption,
            bool saveSampledData = false)
        {
            // constructs uniform sampling grid
            GridInfo2D g = new (rows: nRows, cols: nCols, spacingY: PeriodY / nRows, spacingX: PeriodX / nCols);
            // calls base method
            MatrixZ v = Sample(wavelength: wavelength,
                grid: g,
                matProperty: matProperty,
                loopMode: loopMode,
                cacheSampleData: saveSampledData);
            return (v, g);
        }

        /// <summary>
        /// creates RCWA2D solver for this meta-atom
        /// at fixed wavelength for given front and behind materials
        /// </summary>
        /// <param name="wavelength"> working wavelength in vacuum </param>
        /// <param name="front"> material of the front layer </param>
        /// <param name="behind"> material of the behind layer </param>
        /// <returns> resulting RCWA2D solver </returns>
        public RCWA2D CreateSolver(double wavelength, 
            Material front, Material behind)
            => new(wavelength: wavelength,
                materialFront: front,
                mediumMiddle: this,
                periodX: PeriodX, periodY: PeriodY, thickness: Height,
                materialBehind: behind);


        public Complex ComputeCplxModulation(double wavelength,
            Material front, Material behind)
        {
            RCWA2D solver = CreateSolver(wavelength, front, behind);
            solver.ComputeHalfSMatrix(kx0: 0.0);

            return Complex.One;
        }

        #endregion
        #region sub-classes

        /// <summary>
        /// meta-atom with 2D rectangular geometry
        /// </summary>
        public class Rect : MetaAtom2D
        {
            #region properties

            /// <summary>
            /// diameter of the rectangle along x direction
            /// </summary>
            public double DiameterX { get; set; }

            /// <summary>
            /// diameter of the rectangle along y direction
            /// </summary>
            public double DiameterY { get; set; }

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
            /// constructs a 2D rectangular meta-atom
            /// </summary>
            /// <param name="periodX"> period of the meta-layer along x-direction </param>
            /// <param name="periodY"> period of the meta-layer along y-direction </param>
            /// <param name="height"> height of the meta-layer </param>
            /// <param name="diameterX"> diameter of the rectangle along x-direction </param>
            /// <param name="diameterY"> diameter of the rectangle along y-direction </param>
            /// <param name="materialEmbed"> embedding material outside the rectangular region </param>
            /// <param name="materialFill"> filling material inside the rectangular region </param>
            public Rect(double periodX, double periodY, 
                double height,
                double diameterX, double diameterY,
                Material materialEmbed,
                Material materialFill)
            {
                // basic geometry
                PeriodX = periodX;
                PeriodY = periodY;
                Height = height;
                // additional parameters
                DiameterX = diameterX;
                DiameterY = diameterY;
                MaterialEmbed = materialEmbed;
                MaterialFill = materialFill;
                // preparation ...
                Epsilon = (w, x, y) => (Math.Abs(x) <= 0.5 * DiameterX && Math.Abs(y) <= 0.5 * DiameterY) ?
                    materialFill.Epsilon(w) : materialEmbed.Epsilon(w);
                N = (w, x, y) => Complex.Sqrt(Epsilon(w, x, y));
                if (materialEmbed.Mu != null || materialFill.Mu != null)
                {
                    materialEmbed.Mu ??= (w) => Complex.One;
                    materialFill.Mu ??= (w) => Complex.One;
                    Mu = (w, x, y) => (Math.Abs(x) <= 0.5 * DiameterX && Math.Abs(y) <= 0.5 * DiameterY) ?
                        materialFill.Mu(w) : materialEmbed.Mu(w);
                }
            }

            #endregion
        }

        /// <summary>
        /// meta-atom with 2D circular geometry
        /// </summary>
        public class Circ : MetaAtom2D
        {
            #region properties

            /// <summary>
            /// diameter of the circle
            /// </summary>
            public double Diameter { get; set; }

            /// <summary>
            /// embedding material outside the circular region
            /// </summary>
            public Material MaterialEmbed { get; set; }

            /// <summary>
            /// filling material inside the circular region
            /// </summary>
            public Material MaterialFill { get; set; }

            #endregion
            #region constructor

            /// <summary>
            /// constructs a 2D circular meta-atom
            /// </summary>
            /// <param name="periodX"> period of the meta-layer along x-direction </param>
            /// <param name="periodY"> period of the meta-layer along y-direction </param>
            /// <param name="height"> height of the meta-layer </param>
            /// <param name="diameter"> diameter of the circle </param>
            /// <param name="materialEmbed"> embedding material outside the rectangular region </param>
            /// <param name="materialFill"> filling material inside the rectangular region </param>
            public Circ(double periodX, double periodY,
                double height,
                double diameter,
                Material materialEmbed,
                Material materialFill)
            {
                // basic geometry
                PeriodX = periodX;
                PeriodY = periodY;
                Height = height;
                // additional parameters
                Diameter = diameter;
                MaterialEmbed = materialEmbed;
                MaterialFill = materialFill;
                // preparation ...
                Epsilon = (w, x, y) => (Math.Sqrt(x*x+y*y) <= 0.5 * Diameter) ?
                    materialFill.Epsilon(w) : materialEmbed.Epsilon(w);
                N = (w, x, y) => Complex.Sqrt(Epsilon(w, x, y));
                if (materialEmbed.Mu != null || materialFill.Mu != null)
                {
                    materialEmbed.Mu ??= (w) => Complex.One;
                    materialFill.Mu ??= (w) => Complex.One;
                    Mu = (w, x, y) => (Math.Sqrt(x * x + y * y) <= 0.5 * Diameter) ?
                        materialFill.Mu(w) : materialEmbed.Mu(w);
                }
            }

            #endregion
        }

        #endregion
    }
}
