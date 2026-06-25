namespace VEMS.MathCore
{

    /// <summary>
    /// 2D polynomial fitting
    /// </summary>
    public class Polyn2D
    {
        #region properties

        /// <summary>
        /// degree of the x-polynomials
        /// </summary>
        public long DegreeX {  get; set; }

        /// <summary>
        /// degree of the y-polynomials
        /// </summary>
        public long DegreeY { get; set; }

        /// <summary>
        /// matrix composed of the polynomial
        /// basis functions for data fitting
        /// </summary>
        public MatrixD? NFit {  get; set; }

        /// <summary>
        /// matrix composed of the polynomial
        /// basis functions for value evaluation
        /// </summary>
        public MatrixD? NEvaluate { get; set; }

        /// <summary>
        /// weights of the polynomial basis functions
        /// </summary>
        public VectorD? Weights { get; set; }

        #endregion
        #region constructor

        /// <summary>
        /// default constructor
        /// </summary>
        internal Polyn2D() { }

        /// <summary>
        /// constructs a 2D polynomial fitter
        /// </summary>
        /// <param name="xs"> input sample x-locations </param>
        /// <param name="ys"> input sample y-locations </param>
        /// <param name="vs"> input sample values </param>
        /// <param name="degreeX"> x-polynomial degree, default is 3 </param>
        /// <param name="degreeY"> y-polynomial degree, default is 3 </param>
        /// <param name="checkFitError"> whether to check the fitting error (standard deviation w.r.t. input samples) </param>
        public Polyn2D(VectorD xs, VectorD ys, VectorD vs,
            long degreeX = 3, long degreeY = 3,
            bool checkFitError = false)
        {
            // set parameters
            DegreeX = degreeX;
            DegreeY = degreeY;

            // generates the fitting matrix
            NFit = NMatrix(degreeX: DegreeX, degreeY: DegreeY, 
                x: xs, y: ys);
            // computes the weights
            Weights = LinAlg.LeastSquare(NFit, vs);

            // checks fitting error
            if (checkFitError)
            {
                VectorD checkValues = Evaluate(xe: xs, ye: ys);
                double stdDev = VMath.StandardDeviation(checkValues, vs);
                Printer.Logging($"Standard deviation of the fitting is {stdDev}");
            }
        }

        #endregion
        #region methods

        /// <summary>
        /// generates the matrix composed of polynomial 
        /// basis functions, for given locations
        /// </summary>
        /// <param name="degreeX"> degree of x-polynomials </param>
        /// <param name="degreeY"> degree of y-polynomials </param>
        /// <param name="x"> vector containing the x-locations </param>
        /// <param name="y"> vector containing the y-locations </param>
        /// <returns> matrix composed of basis functions </returns>
        internal static MatrixD NMatrix(long degreeX, long degreeY,
            VectorD x, VectorD y)
        {
            MatrixD f = new(rows: x.Count, cols: (degreeX + 1) * (degreeY + 1));
            LongRange allRows = new(0, f.Rows);
            for (long l = 0; l < f.Cols; l++)
            {
                long m = l / (degreeX + 1);
                long n = l % (degreeX + 1);
                f[allRows, l] = VMath.Powx(x, n) * VMath.Powx(y, m);
            }
            return f;
        }

        /// <summary>
        /// evaluates values at specific locations
        /// </summary>
        /// <param name="xe"> evaluation x-locations </param>
        /// <param name="ye"> evaluation y-locations </param>
        /// <returns> values evaluated at those locations </returns>
        /// <exception cref="ArgumentNullException"></exception>
        public VectorD Evaluate(VectorD xe, VectorD ye)
        {
            if (Weights == null) { throw new ArgumentNullException("Weights"); }
            NEvaluate = NMatrix(degreeX: DegreeX, degreeY: DegreeY, x: xe, y: ye);
            VectorD ve = LinAlg.Dot(NEvaluate, Weights);
            return ve;
        }

        #endregion
    }
}
