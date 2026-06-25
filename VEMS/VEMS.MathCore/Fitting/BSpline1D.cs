using WMathCore;

namespace VEMS.MathCore
{

    /// <summary>
    /// 1D B-spline fitting class
    /// </summary>
    public class BSpline1D
    {
        #region properties

        /// <summary>
        /// degree of the BSpline polynomials
        /// </summary>
        public long Degree { get; set; }
        
        /// <summary>
        /// numerical factor for determining the number of knot spans
        /// </summary>
        public double NumFactor { get; set; }

        /// <summary>
        /// options for knots type: clamped or uniform
        /// </summary>
        public BSpline.KnotsType KnotsType { get; set; }

        /// <summary>
        /// knots data
        /// </summary>
        public VectorD? Knots { get; set; }
        
        /// <summary>
        /// matrix composed of the basis functions
        /// for data fitting
        /// </summary>
        public WMatrixDi? NFit { get; set; }

        /// <summary>
        /// weights of the basis functions
        /// </summary>
        public VectorD? Weights { get; set; }

        /// <summary>
        /// matrix composed of the basis function
        /// for value evaluation
        /// </summary>
        public WMatrixDi? NEvaluate { get; set; }

        #endregion
        #region constructor

        /// <summary>
        /// default constructor
        /// </summary>
        internal BSpline1D() { }

        /// <summary>
        /// constructs a 1D B-spline fitter
        /// </summary>
        /// <param name="xs"> input sample locations </param>
        /// <param name="ys"> input sample values </param>
        /// <param name="degree"> polynomial degree, default is 3 </param>
        /// <param name="nFactor"> numerical factor between 0.0 and 1.0 for defining the number of knots spans </param>
        /// <param name="knots"> manually defined knots, default is null and will be defined automatically </param>
        /// <param name="knotsType"> type of the knots </param>
        /// <param name="checkFitError"> whether to check the fitting error (standard deviation w.r.t. input samples) </param>
        public BSpline1D(VectorD xs, VectorD ys,
            long degree = 3,
            double nFactor = 0.7,
            VectorD? knots = null,
            BSpline.KnotsType knotsType = BSpline.KnotsType.Clamped,
            bool checkFitError = false)
        {
            // sets parameters
            Degree = degree;
            NumFactor = nFactor;
            KnotsType = knotsType;
            if (knots != null) { Knots = knots; }
            else { Knots = BSpline.GenerateKnots(us: xs, p: Degree, nFactor: NumFactor, type: KnotsType); }
                
            // generates the fitting matrix
            NFit = BSpline.NMatrixi(Knots, Degree, xs);
            // computes the weights
            Weights = Sparse.LeastSquare(NFit, ys);

            // checks fitting error
            if (checkFitError)
            {
                VectorD checkValues = Evaluate(xs);
                double stdDev = VMath.StandardDeviation(checkValues, ys);
                Printer.Logging($"Standard deviation of the fitting is {stdDev}");
            }
        }

        /// <summary>
        /// constructs a 1D B-spline fitter
        /// </summary>
        /// <param name="samples"> Scattered data containing the sample locations and values </param>
        /// <param name="degree"> polynomial degree, default is 3 </param>
        /// <param name="nFactor"> numerical factor between 0.0 and 1.0 for defining the number of knots spans </param>
        /// <param name="knots"> manually defined knots, default is null and will be defined automatically </param>
        /// <param name="knotsType"> type of the knots </param>
        /// <param name="checkFitError"> whether to check the fitting error (standard deviation w.r.t. input samples) </param>
        public BSpline1D(Scat1DRealData samples,
            long degree = 3,
            double nFactor = 0.7,
            VectorD? knots = null,
            BSpline.KnotsType knotsType = BSpline.KnotsType.Clamped,
            bool checkFitError = false)
            : this(xs: samples.Points, ys: samples.Values, 
                  degree: degree, nFactor: nFactor, knots: knots, 
                  knotsType: knotsType, checkFitError: checkFitError)
        { }

        #endregion
        #region methods

        /// <summary>
        /// evaluates values at specific locations
        /// </summary>
        /// <param name="xe"> evaluation locations </param>
        /// <returns> values evaluated at those locations </returns>
        public VectorD Evaluate(VectorD xe)
        {
            if(Weights == null || Knots == null) { throw new ArgumentNullException("Weights"); }
            NEvaluate = BSpline.NMatrixi(Knots, Degree, xe);
            VectorD ye = new(xe.Count);
            Sparse.MV(NEvaluate, Weights, ref ye);
            return ye;
        }

        /// <summary>
        /// evaluates value at a specific location
        /// </summary>
        /// <param name="xe"> single location for value evaluation </param>
        /// <returns> value evaluated at this position </returns>
        public double Evaluate(double xe)
        {
            VectorD vxe = new(1) { [0] = xe };
            return Evaluate(vxe)[0];
        }

        /// <summary>
        /// computes the derivative at specific locations
        /// for a desired order
        /// </summary>
        /// <param name="xe"> evaluation locations </param>
        /// <param name="dOrder"> order of derivative, default is 1 </param>
        /// <returns> derivative values </returns>
        public VectorD Derivative(VectorD xe, long dOrder = 1)
        {
            if (dOrder > Degree) { throw new ArgumentException("Derivative order exceeds the polynomial's degree."); }
            if (Weights == null || Knots == null) { throw new ArgumentNullException("Weights"); }           
            NEvaluate = BSpline.NMatrixi(Knots, Degree - dOrder, xe);
            // calculates new weights for the derivative
            VectorD q = new(Knots.Count);
            VectorD w = new(Weights, true);
            for(long d = 0; d < dOrder; d++)
            {
                long p = Degree - d;
                for(long i = d + 1; i < q.Count; i++)
                {
                    double dw = w[i, false] - w[i - 1, false];
                    double du = Knots[i + p, false] - Knots[i, false];
                    q[i, false] = p * dw / du;
                }
                w = new(q, true);
            }
            // evaluates 
            VectorD ye = new(xe.Count);
            Sparse.MV(NEvaluate, q, ref ye);
            return ye;
        }

        #endregion

    }

}
