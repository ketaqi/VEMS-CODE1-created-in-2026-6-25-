using Complex = System.Numerics.Complex;

namespace VEMS.MathCore
{

    /// <summary>
    /// two-dimensional segment class
    /// </summary>
    public class Segment2D
    {
        #region properties

        /// <summary>
        /// center position of the segment along x-axis
        /// with respect to the global coordinate system
        /// </summary>
        public double X0 { get; set; }

        /// <summary>
        /// center position of the segment along y-axis
        /// with respect to the global coordinate system
        /// </summary>
        public double Y0 { get; set; }

        /// <summary>
        /// function that defines the segment's 
        /// aperture/profile p = f(x, y)
        /// <para> variable #1: x - local coordinate x centered around x0 </para>
        /// <para> variable #2: y - local coordinate y centered around y0 </para>    
        /// </summary>
        public Func<double, double, double> Profile { get; set; }

        #endregion
        #region constructor

        /// <summary>
        /// default constructor
        /// </summary>
        internal Segment2D()
        {
            Profile = (x, y) => 0.0;
        }

        /// <summary>
        /// constructs a 2D segment with given parameters
        /// </summary>
        /// <param name="x0"> center of the segment along x-axis </param>
        /// <param name="y0"> center of the segment along y-axis </param>
        /// <param name="profile"> function that defines the aperture/profile of the segment </param>
        public Segment2D(double x0, double y0,
            Func<double, double, double> profile = null)
        {
            X0 = x0;
            Y0 = y0;
            Profile = profile;
        }

        #endregion
        #region methods

        /// <summary>
        /// takes the segment out of a given input function
        /// </summary>
        /// <param name="f"> input function </param>
        /// <returns> segmented function in the local coordinate system </returns>
        /// <exception cref="ArgumentNullException"></exception>
        public Func<double, double, double> TakeFrom(Func<double, double, double> f)
            => (x, y) => f(x + X0, y + Y0) * Profile(x, y);

        /// <summary>
        /// takes the segment out of a given input function
        /// </summary>
        /// <param name="f"> input function </param>
        /// <returns> segmented function in the local coordinate system </returns>
        /// <exception cref="ArgumentNullException"></exception>
        public Func<double, double, Complex> TakeFrom(Func<double, double, Complex> f)
            => (x, y) => f(x + X0, y + Y0) * Profile(x, y);

        /// <summary>
        /// takes the segment out of a given input data distribution
        /// with specific interpolation method
        /// </summary>
        /// <param name="d"> input data distribution </param>
        /// <returns> segmented distribution (as a function) in the local coordinate system </returns>
        /// <exception cref="ArgumentNullException"></exception>
        public Func<double, double, double> TakeFrom(Grid2DRealData d)
            => (x, y) => d.FindValue(x + X0, y + Y0) * Profile(x, y);

        /// <summary>
        /// takes the segment out of a given input data distribution
        /// with specific interpolation method
        /// </summary>
        /// <param name="d"> input data distribution </param>
        /// <returns> segmented distribution (as a function) in the local coordinate system </returns>
        /// <exception cref="ArgumentNullException"></exception>
        public Func<double, double, Complex> TakeFrom(Grid2DCplxData d)
            => (x, y) => d.FindValue(x + X0, y + Y0) * Profile(x, y);

        /// <summary>
        /// takes the segment out of a given input data distribution
        /// and samples it on a target uniform grid
        /// </summary>
        /// <param name="d"> input data distribution </param>
        /// <param name="g"> target uniform sampling grid </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> sampled values within the segment </returns>
        public MatrixD SampleFrom(Grid2DRealData d, GridInfo2D g,
            LoopMode loopMode = Defaults.LoopOption)
        {
            Func<double, double, double> f = TakeFrom(d);
            Samp2DRealFunc sf = new(f: f);
            return sf.Sample(grid: g, loopMode: loopMode);
        }

        /// <summary>
        /// takes the segment out of a given input data distribution
        /// and samples it on a target uniform grid
        /// </summary>
        /// <param name="d"> input data distribution </param>
        /// <param name="g"> target uniform sampling grid </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> sampled values within the segment </returns>
        public MatrixZ SampleFrom(Grid2DCplxData d, GridInfo2D g,
            LoopMode loopMode = Defaults.LoopOption)
        {
            Func<double, double, Complex> f = TakeFrom(d);
            Samp2DCplxFunc sf = new(f: f);
            return sf.Sample(grid: g, loopMode: loopMode);
        }

        #endregion
        #region derived sub-classes

        /// <summary>
        /// 2D segment with (cosine-edged) rectangular profiles
        /// along both x- and y-direction
        /// </summary>
        public class CosRect : Segment2D
        {
            #region properties

            /// <summary>
            /// full width of the rectangle along x-axis
            /// [when there is no smooth edges]
            /// </summary>
            public double WidthX { get; set; }

            /// <summary>
            /// full width of the rectangle along y-axis
            /// [when there is no smooth edges]
            /// </summary>
            public double WidthY { get; set; }

            /// <summary>
            /// absolute edge width along x
            /// [half within, half outside]
            /// </summary>
            public double EdgeX { get; set; }

            /// <summary>
            /// absolute edge width along y
            /// [half within, half outside]
            /// </summary>
            public double EdgeY { get; set; }

            #endregion
            #region constructor

            /// <summary>
            /// constructs a 2D segment with cosine-edged 
            /// rectangular profile
            /// </summary>
            /// <param name="x0"> center of the segment along x-axis </param>
            /// <param name="y0"> center of the segment along y-axis </param>
            /// <param name="widthX"> full width of the rectangle along x-axis </param>
            /// <param name="widthY"> full width of the rectangle along y-axis </param>
            /// <param name="edgeX"> absolute edge width along x-axis </param>
            /// <param name="edgeY"> absolute edge width along y-axis </param>
            public CosRect(double x0, double y0,
                double widthX, double widthY,
                double edgeX = 0.0, double edgeY = 0.0)
                : base(x0, y0)
            {
                // parameters
                WidthX = widthX;
                WidthY = widthY;
                EdgeX = edgeX;
                EdgeY = edgeY;
                // set the profile function
                Profile = (x, y) =>
                {
                    double fx = Function1D.CosEdgeRectangle(x, new List<double> { WidthX, EdgeX, 0.0, 1.0 });
                    double fy = Function1D.CosEdgeRectangle(y, new List<double> { WidthY, EdgeY, 0.0, 1.0 });
                    return fx * fy;
                };
            }

            #endregion
        }

        /// <summary>
        /// 2D segment with Gaussian profiles
        /// along both x- and y-direction
        /// </summary>
        public class Gaussian : Segment2D
        {
            #region properties

            /// <summary>
            /// waist radius of the Gaussian profile along x-axis
            /// </summary>
            public double WaistX { get; set; }

            /// <summary>
            /// waist radius of the Gaussian profile along y-axis
            /// </summary>
            public double WaistY { get; set; }

            #endregion
            #region constructor

            /// <summary>
            /// constructs a 2D segment with 
            /// Gaussian profile
            /// </summary>
            /// <param name="x0"> center of the segment along x-axis </param>
            /// <param name="y0"> center of the segment along y-axis </param>
            /// <param name="waistX"> waist radius along x-axis </param>
            /// <param name="waistY"> waist radius along y-axis </param>
            public Gaussian(double x0, double y0,
                double waistX, double waistY)
                : base(x0, y0)
            {
                // parameters
                WaistX = waistX;
                WaistY = waistY;
                // set the profile function
                Profile = (x, y) =>
                {
                    double fx = Function1D.Gaussian(x, new List<double> { WaistX, 0.0, 1.0 });
                    double fy = Function1D.Gaussian(y, new List<double> { WaistY, 0.0, 1.0 });
                    return fx * fy;
                };
            }

            #endregion
        }

        #endregion
    }
}
