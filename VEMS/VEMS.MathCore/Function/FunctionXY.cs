namespace VEMS.MathCore
{

    /// <summary>
    /// X/Y-separable 2D functions
    /// </summary>
    public class FunctionXY
    {
        #region properties

        /// <summary>
        /// 1D function fx(x) along x-direction
        /// </summary>
        public Func<double, List<double>?, double> Fx { get; set; }

        /// <summary>
        /// 1D function fy(y) along y-direction
        /// </summary>
        public Func<double, List<double>?, double> Fy { get; set; }

        #endregion
        #region constructor

        /// <summary>
        /// default constructor
        /// </summary>
        public FunctionXY()
        {
            Fx = Function1D.Constant;
            Fy = Function1D.Constant;
        }

        /// <summary>
        /// constructs a separable 2D function
        /// f(x,y) = fx(x) * fy(y)
        /// </summary>
        /// <param name="fx"> constructing function fx(x) along x-direciton </param>
        /// <param name="fy"> constructing function fy(y) along x-direciton </param>
        public FunctionXY(Func<double, List<double>?, double> fx,
            Func<double, List<double>?, double> fy)
        {
            Fx = fx;
            Fy = fy;
        }

        #endregion
        #region methods

        /// <summary>
        /// evaluates at the x-y coordinates with 
        /// specific parameters ({px}, {py})
        /// </summary>
        /// <param name="x"> variable x </param>
        /// <param name="y"> variable y </param>
        /// <param name="px"> parameters {px} </param>
        /// <param name="py"> parameters {py} </param>
        /// <returns> result function value </returns>
        public double Evaluate(double x, double y,
            List<double>? px, List<double>? py)
            => Fx(x, px) * Fy(y, py);

        /// <summary>
        /// evaluates at the polar coordinates with
        /// specific parameters ({px}, {py})
        /// </summary>
        /// <param name="r"> radial variable r </param>
        /// <param name="theta"> azimuthal variable theta </param>
        /// <param name="px"> parameters {px} </param>
        /// <param name="py"> parameters {py} </param>
        /// <returns> result function value </returns>
        public double EvaluateRT(double r, double theta,
            List<double>? px, List<double>? py)
        {
            // converts from polar to x-y coordinates
            (double x, double y) = Converter.Polar2Cartesian(r, theta);
            // evalutes at (x, y)
            return Evaluate(x, y, px, py);
        }

        #endregion
        #region derived sub-classes

        /// <summary>
        /// x,y-separable 2D Gaussian function
        /// </summary>
        public class Gaussian : FunctionXY
        {
            /// <summary>
            /// constructs a x,y-separable 2D Gaussian function
            /// f(x, y; {px}, {py}) = Gx(x; {px}) * Gy(y; {py})
            /// </summary>
            public Gaussian()
                : base(fx: Function1D.Gaussian, fy: Function1D.Gaussian)
            { }
        }

        /// <summary>
        /// x,y-separable 2D rectangular function
        /// </summary>
        public class Rectangle : FunctionXY
        {
            /// <summary>
            /// constructs a x,y-separable 2D rectangular function
            /// f(x, y; {px}, {py}) = RectX(x; {px}) * RectY(y; {py})
            /// </summary>
            public Rectangle()
                : base(fx: Function1D.Rectangle, fy: Function1D.Rectangle)
            { }
        }

        /// <summary>
        /// x,y-separable 2D rectangular function with 
        /// cosine-smoothed edges on both sides
        /// </summary>
        public class CosEdgeRectangle : FunctionXY
        {
            /// <summary>
            /// constructs a x,y-separable 2D rectangular function
            /// with cosine-smoothed edges on both sides
            /// f(x, y; {px}, {py}) = RectX(x; {px}) * RectY(y; {py})
            /// </summary>
            public CosEdgeRectangle()
                : base(fx: Function1D.CosEdgeRectangle, fy: Function1D.CosEdgeRectangle)
            { }
        }

        /// <summary>
        /// x,y-separable 2D rectangular function with
        /// Gaussian smoothed edges on both sides
        /// </summary>
        public class GaussEdgeRectangle : FunctionXY
        {
            /// <summary>
            /// constructs a x,y-separable 2D rectangular function 
            /// with Gaussian smoothed edges on both sides
            /// f(x, y; {px}, {py}) = RectX(x; {px}) * RectY(y; {py})
            /// </summary>
            public GaussEdgeRectangle()
                : base(fx: Function1D.GaussEdgeRectangle, fy: Function1D.GaussEdgeRectangle)
            { }
        }

        #endregion
    }


}
