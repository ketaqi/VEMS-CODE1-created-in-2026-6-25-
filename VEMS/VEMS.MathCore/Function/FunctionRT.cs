namespace VEMS.MathCore
{
    /// <summary>
    /// R/Theta-separable 2D functions
    /// </summary>
    public class FunctionRT
    {
        #region properties

        /// <summary>
        /// 1D function fr(r) along radial direction
        /// </summary>
        public Func<double, List<double>?, double>? Fr { get; set; }

        /// <summary>
        /// 1D function ft(theta) along azimuthal direction
        /// </summary>
        public Func<double, List<double>?, double>? Ft { get; set; }

        /// <summary>
        /// composed 2D function f(r,theta)
        /// </summary>
        public Func<double, List<double>?, double, List<double>?, double>? Frt
        {
            get
            {
                if(Fr == null || Ft == null) { throw new ArgumentNullException(); }
                Func<double, List<double>?, double, List<double>?, double> frt
                    = (r, pr, t, pt) => Fr(r, pr) * Ft(t, pt);
                return frt;
            }
        }

        #endregion
        #region constructor

        /// <summary>
        /// default constructor
        /// </summary>
        public FunctionRT() { }

        /// <summary>
        /// constructs a separable 2D function
        /// f(r,theta) = fr(r) * ft(theta)
        /// </summary>
        /// <param name="fr"> constructing function fr(r) along  radial direciton </param>
        /// <param name="ft"> constructing function ft(theta) along azimuthal direciton </param>
        public FunctionRT(Func<double, List<double>?, double> fr,
            Func<double, List<double>?, double> ft)
        {
            Fr = fr;
            Ft = ft;
        }

        #endregion
        #region methods

        /// <summary>
        /// evaluates at the polar coordinates with 
        /// specific parameters ({px}, {py})
        /// </summary>
        /// <param name="r"> radial variable r </param>
        /// <param name="t"> azimuthal variable theta </param>
        /// <param name="pr"> parameters {pr} </param>
        /// <param name="pt"> parameters {pt} </param>
        /// <returns> result function value </returns>
        public double Evaluate(double r, double t,
            List<double> pr, List<double> pt)
        {
            if(Fr == null || Ft == null) { return 0.0; }
            else return Fr.Invoke(r, pr) * Ft.Invoke(t, pt);
        }

        /// <summary>
        /// evaluates at the x-y coordinates with
        /// specific parameters ({pr}, {pt})
        /// </summary>
        /// <param name="x"> Cartesian coordiante variable x </param>
        /// <param name="y"> Cartesian coordinate variable y </param>
        /// <param name="pr"> parameters {pr} </param>
        /// <param name="pt"> parameters {pt} </param>
        /// <returns> result function value </returns>
        public double EvaluateXY(double x, double y,
            List<double> pr, List<double> pt)
        {
            // converts from x-y to polar coordinates
            (double r, double theta) = Converter.Cartesian2Polar(x, y);
            // evalutes in polar coordinate system
            return Evaluate(r, theta, pr, pt);
        }

        #endregion
        #region derived sub-classes

        /// <summary>
        /// circle fuction defined in polar coordinate
        /// </summary>
        public class Circle : FunctionRT
        {
            /// <summary>
            /// 
            /// </summary>
            public Circle()
                : base(fr: Function1D.Rectangle, ft: Function1D.Constant)
            { }

        }

        #endregion
    }

}
