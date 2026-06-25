namespace VEMS.MathCore
{

    /// <summary>
    /// Provides 1D polynomial fitting functionality.
    /// </summary>
    public class Polyn1D
    {
        #region properties

        /// <summary>
        /// Gets or sets the degree of the polynomial.
        /// </summary>
        public long Degree { get; set; }

        /// <summary>
        /// Gets or sets the matrix composed of the polynomial
        /// basis functions for data fitting.
        /// </summary>
        public MatrixD? NFit { get; set; }

        /// <summary>
        /// Gets or sets the matrix composed of the polynomial
        /// basis functions for value evaluation.
        /// </summary>
        public MatrixD? NEvaluate { get; set; }

        /// <summary>
        /// Gets or sets the weights of the polynomial basis functions.
        /// </summary>
        public VectorD? Weights { get; set; }

        #endregion
        #region constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="Polyn1D"/> class.
        /// </summary>
        internal Polyn1D() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Polyn1D"/> class and fits a polynomial to the provided data.
        /// </summary>
        /// <param name="xs">Input sample locations.</param>
        /// <param name="ys">Input sample values.</param>
        /// <param name="degree">Polynomial degree. Default is 3.</param>
        /// <param name="checkFitError">Whether to check the fitting error (standard deviation with respect to input samples).</param>
        public Polyn1D(VectorD xs, VectorD ys,
            long degree = 3,
            bool checkFitError = false)
        {
            // sets parameters
            Degree = degree;

            // generates the fitting matrix
            NFit = NMatrix(degree: Degree, x: xs);
            // computes the weights
            Weights = LinAlg.LeastSquare(NFit, ys);

            // checks fitting error
            if (checkFitError)
            {
                VectorD checkValues = Evaluate(xs);
                double stdDev = VMath.StandardDeviation(checkValues, ys);
                Printer.Logging($"Standard deviation of the fitting is {stdDev}");
            }
        }

        #endregion
        #region methods

        /// <summary>
        /// Generates the matrix composed of polynomial 
        /// basis functions for the given locations.
        /// </summary>
        /// <param name="degree">Degree of the polynomials.</param>
        /// <param name="x">Vector containing the locations.</param>
        /// <returns>Matrix composed of basis functions.</returns>
        internal static MatrixD NMatrix(long degree, VectorD x)
        {
            MatrixD f = new(rows: x.Count, cols: degree + 1,
                mode: ArrayInitMode.Malloc);
            //LongRange allRows = new(0, f.Rows);
            //for (long iCol = 0; iCol < f.Cols; iCol++)
            //    f[allRows, iCol] = VMath.Powx(x, iCol);
            for (long iRow = 0; iRow < x.Count; iRow++)
            {
                double val = 1.0;
                f[iRow, 0, false] = val;
                double xVal = x[iRow, false];
                for (long iCol = 1; iCol <= degree; iCol++)
                {
                    val *= xVal;
                    f[iRow, iCol, false] = val;
                }
            }
            return f;
        }

        /// <summary>
        /// Evaluates the fitted polynomial at specific locations.
        /// </summary>
        /// <param name="xe">Evaluation locations.</param>
        /// <returns>Values evaluated at the specified locations.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <see cref="Weights"/> is null.</exception>
        public VectorD Evaluate(VectorD xe)
        {
            if (Weights == null) { throw new ArgumentNullException("Weights"); }
            NEvaluate = NMatrix(degree: Degree, x: xe);
            return LinAlg.Dot(NEvaluate, Weights);
        }

        /// <summary>
        /// Evaluates the fitted polynomial at a single location.
        /// </summary>
        /// <param name="xe">Single location for value evaluation.</param>
        /// <returns>Value evaluated at the single location.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <see cref="Weights"/> is null.</exception>
        public double Evaluate(double xe)
        {
            //VectorD vxe = new(1) { [0] = xe };
            //return Evaluate(vxe)[0];
            if (Weights == null) throw new ArgumentNullException(nameof(Weights));
            double result = 0.0;
            double xPow = 1.0;
            for (long i = 0; i <= Degree; i++)
            {
                result += Weights[i, false] * xPow;
                xPow *= xe;
            }
            return result;
        }

        /// <summary>
        /// Computes the derivatives of the fitted polynomial at specific locations
        /// for a desired order.
        /// </summary>
        /// <param name="xe">Evaluation locations.</param>
        /// <param name="dOrder">Order of derivative. Default is 1.</param>
        /// <returns>Derivative values at the specified locations.</returns>
        /// <exception cref="ArgumentException">Thrown if the derivative order exceeds the polynomial's degree.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <see cref="Weights"/> is null.</exception>
        public VectorD Derivative(VectorD xe, long dOrder = 1)
        {
            if (dOrder > Degree) { throw new ArgumentException("Derivative order exceeds the polynomial's degree."); }
            if (Weights == null) { throw new ArgumentNullException("Weights"); }
            NEvaluate = NMatrix(degree: Degree - dOrder, x: xe);
            // calculates new weights for the derivative at desired order
            VectorD q = new(count: Weights.Count - dOrder,
                mode: ArrayInitMode.Malloc);
            for (long i = dOrder; i < Weights.Count; i++)
            {
                long fac = 1;
                for (long d = 0; d < dOrder; d++) { fac *= (i - d); }
                q[i - dOrder, false] = fac * Weights[i, false];
            }
            // evaluates
            return LinAlg.Dot(NEvaluate, q);
        }

        #endregion
    }

}
