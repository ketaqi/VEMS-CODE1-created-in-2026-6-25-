using System.Diagnostics;
using WMathCore;

namespace VEMS.MathCore
{
    /// <summary>
    /// The node vector of a clamped B-spline curve is U={a,... ,a,u(p+1),... ,u(m-p-1),b,... ,b}
    /// Contains m+1 nodes and m node ranges
    /// n+1 basis function where n=m-p-1
    /// </summary>
    public class ClampedBSpline1D 
    {
        #region properties

        private long degree { get; set; }
        private double numFactor { get; set; }
        private VectorD? knots { get; set; }
        private WMatrixDi? nFit { get; set; }
        private VectorD? weights { get; set; }
        private WMatrixDi? nEva { get; set; }

        /// <summary>
        /// degree of the BSpline polynomial
        /// </summary>
        public long Degree
        {
            get => degree;
            set => degree = value;
        }

        /// <summary>
        /// numerical factor for determining the number of knot spans
        /// </summary>
        public double NumFactor
        {
            get => numFactor;
            set => numFactor = value;
        }

        /// <summary>
        /// uniform knots
        /// </summary>
        public VectorD? Knots
        {
            get => knots;
            set => knots = value;
        }

        /// <summary>
        /// matrix composed of basis functions
        /// for data fitting
        /// </summary>
        public WMatrixDi? NFit
        {
            get => nFit;
            set => nFit = value;
        }

        /// <summary>
        /// weights of the basis functions
        /// </summary>
        public VectorD? Weights
        {
            get => weights;
            set => weights = value;
        }

        /// <summary>
        /// matrix composed of basis functions
        /// for value evaluation
        /// </summary>
        public WMatrixDi? NEva
        {
            get => nEva;
            set => nEva = value;
        }

        #endregion
        #region constructor

        /// <summary>
        /// constructs a 1D clamped BSpline fitter
        /// and fits to the given samples
        /// </summary>
        /// <param name="samples"> samples (locations and function values) </param>
        /// <param name="degree"> polynomial degree, default is 3 </param>
        /// <param name="nFactor"> factor between 0.0 and 1.0 for defining the number of knots spans </param>
        /// <param name="knots"> manually defined knots, default is null and will be defined automatically </param>
        /// <param name="checkFitError"> whether to check the fitting error (standard deviation w.r.t input) </param>
        public ClampedBSpline1D(Scat1DRealData samples,
            long degree = 3,
            double nFactor = 0.7,
            GridInfo1D? knots = null,
            bool checkFitError = false)
        {
            // set degree
            Degree = degree;
            // set numerical paramerer
            NumFactor = nFactor;

            // set knots, if not specified as input
            if (knots == null || Knots == null)
                Knots = ClampedBSplineCommons.UniKnots(samples.Points, NumFactor, Degree);

            // perform data fitting
            nFit = ClampedBSplineCommons.NWMatrixDi(Knots, Degree, samples.Points);
            Weights = Sparse.LeastSquare(nFit, samples.Values);

            if (checkFitError)
            {
                VectorD checkValues = Evaluate(samples.Points);
                double stdDev = VMath.StandardDeviation(checkValues, samples.Values);
                Printer.Logging($"Standard deviation of the fitting is {stdDev}");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="points"></param>
        /// <param name="values"></param>
        /// <param name="degree"></param>
        /// <param name="nFactor"></param>
        /// <param name="knots"></param>
        /// <param name="checkFitError"></param>
        public ClampedBSpline1D(VectorD points, VectorD values,
            long degree = 3,
            double nFactor = 0.7,
            GridInfo1D? knots = null,
            bool checkFitError = false)
            : this(new Scat1DRealData(points, values), degree, nFactor, knots, checkFitError) 
        { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="grid"></param>
        /// <param name="values"></param>
        /// <param name="degree"></param>
        /// <param name="nFactor"></param>
        /// <param name="knots"></param>
        /// <param name="checkFitError"></param>
        public ClampedBSpline1D(GridInfo1D grid, VectorD values,
            long degree = 3,
            double nFactor = 0.7,
            GridInfo1D? knots = null,
            bool checkFitError = false)
            : this(new Scat1DRealData(grid.GetCoordinates(), values), degree, nFactor, knots, checkFitError)
        { }

        #endregion
        #region methods
        /// <summary>
        /// Calculate the value of the function for the target position
        /// </summary>
        /// <param name="evaluationLocations"></param>
        /// <returns></returns>
        public VectorD Evaluate(VectorD evaluationLocations)
        {
            if (Knots == null)
            {
                Printer.Error("Knots must be created first");
                return new VectorD(0);
            }
            if (Weights == null)
            {
                Printer.Error("Data must be fitted first");
                return new VectorD(0);
            }

            // generate the coefficient matrix 
            nEva = ClampedBSplineCommons.NWMatrixDi(knots: Knots,
                degree: Degree,
                sampleLocations: evaluationLocations);

            // matrix-vector product
            VectorD temp = new(evaluationLocations.Count);
            Sparse.MV(nEva, Weights, ref temp);
            return temp;
        }

        /// <summary>
        /// Calculate the derivative of the target position
        /// </summary>
        /// <param name="derivationLocations"></param>
        /// <param name="order"></param>
        /// <returns></returns>
        public VectorD Derive(VectorD derivationLocations, long order = 1)
        {
            if (Degree - order < 0)
            {
                Printer.Error("Derivation order must be less than or equal to degree");
                return new VectorD(0);
            }
            if (Knots == null)
            {
                Printer.Error("Knots must be created first");
                return new VectorD(0);
            }
            if (Weights == null)
            {
                Printer.Error("Data must be fitted first");
                return new VectorD(0);
            }

            //Compute the NDerMatrix of the order_th derivative
            MatrixD NDer = ClampedBSplineCommons.NDerMatrix(knots, degree, derivationLocations, order);
            return LinAlg.Dot(NDer, Weights);
        }

        #endregion
    }

    /// <summary>
    /// The two node vectors of a quasi-uniform B-spline surface are: U={a1,...,a1,u(p+1),...,u(r-p-1),b1,...,b1}; 
    /// V={a2,...,a2,v(q+1),...,v(s-q-1),b2,...,b2}
    /// There are (r+1) nodes in the U direction, and (s+1) nodes in the V direction
    /// </summary>
    public class ClampedBSpline2D
    {
        #region properties
        //X direction
        private long degreeX { get; set; }
        private double numFactorX { get; set; }
        private VectorD? knotsX { get; set; }
        private MatrixD? nFitX { get; set; }
        private MatrixD? nEvaX { get; set; }

        //Y direction
        private long degreeY { get; set; }
        private double numFactorY { get; set; }
        private VectorD? knotsY { get; set; }
        private MatrixD? nFitY { get; set; }
        private MatrixD? nEvaY { get; set; }

        //Common
        private MatrixD? weights { get; set; }
        private List<MatrixD> nFitXY { get; set; }
        private List<MatrixD>? nEva { get; set; }

        /// <summary>
        /// degree of the BSpline polynomial in X direction
        /// </summary>
        public long DegreeX
        {
            get => degreeX;
            set => degreeX = value;
        }

        /// <summary>
        /// degree of the BSpline polynomial in Y direction
        /// </summary>
        public long DegreeY
        {
            get => degreeY;
            set => degreeY = value;
        }

        /// <summary>
        /// numerical factor for determining the number of knot spans in X direction
        /// </summary>
        public double NumFactorX
        {
            get => numFactorX;
            set => numFactorX = value;
        }

        /// <summary>
        /// numerical factor for determining the number of knot spans in Y direction
        /// </summary>
        public double NumFactorY
        {
            get => numFactorY;
            set => numFactorY = value;
        }

        /// <summary>
        /// uniform knots in X direction
        /// </summary>
        public VectorD? KnotsX
        {
            get => knotsX;
            set => knotsX = value;
        }

        /// <summary>
        /// uniform knots in Y direction
        /// </summary>
        public VectorD? KnotsY
        {
            get => knotsY;
            set => knotsY = value;
        }

        /// <summary>
        /// matrix composed of basis functions in X direction
        /// for data fitting
        /// </summary>
        public MatrixD? NFitX
        {
            get => nFitX;
            set => nFitX = value;
        }

        /// <summary>
        /// matrix composed of basis functions in Y direction
        /// for data fitting
        /// </summary>
        public MatrixD? NFitY
        {
            get => nFitY;
            set => nFitY = value;
        }

        /// <summary>
        /// weights of the basis functions
        /// </summary>
        public MatrixD? Weights
        {
            get => weights;
            set => weights = value;
        }

        /// <summary>
        /// matrix composed of basis functions
        /// for value evaluation(in X direction)
        /// </summary>
        public MatrixD? NEvaX
        {
            get => nEvaX;
            set => nEvaX = value;
        }

        /// <summary>
        /// matrix composed of basis functions
        /// for value evaluation(in Y direction)
        /// </summary>
        public MatrixD? NEvaY
        {
            get => nEvaY;
            set => nEvaY = value;
        }

        /// <summary>
        /// matrix composed of basis functions
        /// for value evaluation
        /// </summary>
        public List<MatrixD>? NEva
        {
            get => nEva;
            set => nEva = value;
        }
        #endregion
        #region constructor
        /// <summary>
        /// constructs a 2D uniform BSpline fitter
        /// and fits to the given samples
        /// </summary>
        /// <param name="samples"> samples (locations and function values) </param>
        /// <param name="degreeX"> polynomial degree in X direction, default is 3 </param>
        /// <param name="degreeY"> polynomial degree in Y direction, default is 3 </param>
        /// <param name="nFactorX"> factor in X direction between 0.0 and 1.0 for defining the number of knots spans </param>
        /// <param name="nFactorY"> factor in Y direction between 0.0 and 1.0 for defining the number of knots spans </param>
        /// <param name="knotsX"> manually defined knots in X direction, default is null and will be defined automatically </param>
        /// <param name="knotsY"> manually defined knots in Y direction, default is null and will be defined automatically </param>
        /// <param name="checkFitError"> whether to check the fitting error (standard deviation w.r.t input) </param>
        public ClampedBSpline2D(Scat2DRealData samples,
            long degreeX = 3, long degreeY = 3,
            double nFactorX = 0.03, double nFactorY = 0.03,
            VectorD? knotsX = null, VectorD? knotsY = null,
            bool checkFitError = false)
        {
            // set degree
            DegreeX = degreeX;
            DegreeY = degreeY;
            // set numerical paramerer
            NumFactorX = nFactorX;
            NumFactorY = nFactorY;
            //Sort the coordinates for generating UniKnots
            VectorD PointsX = new VectorD(samples.PointsX, deepCopy: true);
            VectorD PointsY = new VectorD(samples.PointsY, deepCopy: true);
            //PointsX.Sort();
            //PointsY.Sort();
            VMath.Sort(ref PointsX);
            VMath.Sort(ref PointsY);


            nFitXY = new List<MatrixD>();
            // set knots, if not specified as input
            if (knotsX == null || knotsY == null || KnotsX == null || KnotsY == null)
            {
                KnotsX = ClampedBSplineCommons.UniKnots(PointsX, NumFactorX, degreeX);
                KnotsY = ClampedBSplineCommons.UniKnots(PointsY, NumFactorY, degreeY);
                //Printer.Logging("Uniform 2D knots generated automatically");
            }

            // perform data fitting
            nFitX = ClampedBSplineCommons.NMatrix(KnotsX, DegreeX, samples.PointsX);
            nFitY = ClampedBSplineCommons.NMatrix(KnotsY, DegreeY, samples.PointsY);
            Weights = new(nFitY.Cols, nFitX.Cols);

            //For each input sample, there is a MatrixD of basis function
            LongRange YRange = new(0, nFitY.Cols);
            LongRange XRange = new(0, nFitX.Cols);
            for (long i = 0; i < samples.PointsX.Count; i++)
            {
                MatrixD XMat = NFitX[new LongRange(i, i + 1), XRange];
                MatrixD YMat = NFitY[new LongRange(i, i + 1), YRange];
                nFitXY.Add(LinAlg.Dot(LinAlg.Transpose(YMat), XMat));
            }
            if (nFitXY.Count == 0)
            {
                Printer.Logging("nFit 2D generated failed");
            }

            //Flatten nFitXY to MatrixD to use least squares
            MatrixD NFitUnwrapped = new(samples.PointsX.Count, nFitX.Cols * nFitY.Cols);
            for (int k = 0; k < nFitXY.Count; k++)
            {
                for (int i = 0; i < nFitXY[k].Rows; i++)
                {
                    for (int j = 0; j < nFitXY[k].Cols; j++)
                    {
                        NFitUnwrapped[k, i * nFitXY[k].Cols + j] = nFitXY[k][i, j];
                    }
                }
            }

            //Get Weights1D by using the least squares method, and fold it into MatrixD
            VectorD Weights1D = LinAlg.LeastSquare(NFitUnwrapped, samples.Values);
            for (int i = 0; i < Weights.Rows; i++)
            {
                for (int j = 0; j < Weights.Cols; j++)
                {
                    Weights[i, j] = Weights1D[i * Weights.Cols + j];
                }
            }

            //if (checkFitError)
            //{
            //    VectorD checkValues = Evaluate(samples.Points);
            //    double stdDev = VMath.StandardDeviation(checkValues, samples.Values);
            //    Printer.Logging($"Standard deviation of the fitting is {stdDev}");
            //}
        }
        #endregion

        #region methods
        /// <summary>
        /// evaluate the fitted result at given locations
        /// </summary>
        /// <param name="evaluationLocationsX"> evaluation locations in X direction </param>
        /// <param name="evaluationLocationsY"> evaluation locations in Y direction </param>
        /// <returns> fitted values at evaluation locations </returns>
        public VectorD Evaluate(VectorD evaluationLocationsX, VectorD evaluationLocationsY)
        {
            VectorD EvaluateResult = new(evaluationLocationsX.Count, 0.0);
            if (KnotsX == null || KnotsY == null)
            {
                Printer.Error("Knots must be created first");
                return new VectorD(0);
            }
            if (Weights == null)
            {
                Printer.Error("Data must be fitted first");
                return new VectorD(0);
            }

            // generate the coefficient matrix 
            NEvaX = ClampedBSplineCommons.NMatrix(KnotsX, DegreeX, evaluationLocationsX);
            NEvaY = ClampedBSplineCommons.NMatrix(KnotsY, DegreeY, evaluationLocationsY);
            NEva = new List<MatrixD>();
            LongRange YRange = new(0, NEvaY.Cols);
            LongRange XRange = new(0, NEvaX.Cols);
            for (long i = 0; i < evaluationLocationsX.Count; i++)
            {
                MatrixD XMat = NEvaX[new LongRange(i, i + 1), XRange];
                MatrixD YMat = NEvaY[new LongRange(i, i + 1), YRange];
                MatrixD MatTemp = LinAlg.Dot(LinAlg.Transpose(YMat), XMat);
                NEva.Add(MatTemp);
                EvaluateResult[i] = VMath.Sum(MatTemp * Weights); //(MatTemp * Weights).Sum();
            }
            return EvaluateResult;
        }

        #endregion
    }



    /// <summary>
    /// Clamped B-spline generic method class
    /// </summary>
    public class ClampedBSplineCommons
    {
        /// <summary>
        /// Returns the subscript of the node interval in which the parameter u resides
        /// </summary>
        /// <param name="n">Node interval number</param>
        /// <param name="p">degree</param>
        /// <param name="u"></param>
        /// <param name="U">Node vector U</param>
        /// <returns></returns>
        public static long FindSpan(long n, long p, double u, VectorD U)
        {
            if (u == U[n])
                return n - p - 1;
            long low = p;
            long high = n;
            long mid = (low + high) / 2;
            while (u < U[mid] || u >= U[mid + 1])
            {
                if (u < U[mid])
                    high = mid;
                else low = mid;
                mid = (low + high) / 2;
            }
            return mid;
        }

        /// <summary>
        /// Computes the values of all non-zero B-spline basis functions
        /// </summary>
        /// <param name="i">Node interval subscript</param>
        /// <param name="p">degree</param>
        /// <param name="u"></param>
        /// <param name="U">Node vector</param>
        /// <returns></returns>
        public static VectorD BasisFuns(long i, long p, double u, VectorD U)
        {
            VectorD N = new(p + 1, 0.0);
            N[0] = 1.0;
            VectorD left = new(p + 1, 0.0);
            VectorD right = new(p + 1, 0.0);        
            for (int j = 1; j <= p; j++)
            {
                double saved = 0.0;
                left[j] = u - U[i + 1 - j];
                right[j] = U[i + j] - u;
                for (int r = 0; r < j; r++)
                {
                    double temp = N[r] / (right[r + 1] + left[j - r]);//右边项的一部分
                    N[r] = saved + right[r + 1] * temp;//左边项 +右边项
                    saved = left[j - r] * temp;//下一个左边项
                }
                N[j] = saved;
            }
            return N;
        }

        /// <summary>
        /// Compute the Nth derivative of the basis function at position u
        /// </summary>
        /// <param name="i">Node interval subscript</param>
        /// <param name="p">degree</param>
        /// <param name="u"></param>
        /// <param name="U">Node vector</param>
        /// <param name="n">The order of the derivative</param>
        public static MatrixD DersBasisFuns(long i, long p, double u, VectorD U, long n)
        {
            MatrixD ders = new(n + 1, p + 1);
            MatrixD ndu = new(p + 1, p + 1);//将部分基函数值和节点差放到二维数组中
            ndu[0,0] = 1.0;
            VectorD left = new(p + 1, 0.0);
            VectorD right = new(p + 1, 0.0);
            for (int j = 1; j <= p; j++)
            {
                left[j] = u - U[i + 1 - j];
                right[j] = U[i + j] - u;
                double saved = 0.0;
                for (long rr = 0; rr < j; rr++)
                {
                    //下三角
                    ndu[j,rr] = right[rr + 1] + left[j - rr];
                    double temp = ndu[rr, j - 1] / ndu[j, rr];
                    //上三角
                    ndu[rr, j] = saved + right[rr + 1] * temp;
                    saved = left[j - rr] * temp;
                }
                ndu[j, j] = saved;
            }

            //基函数的值
            for (long j = 0; j <= p; j++)
            {
                ders[0, j] = ndu[j, p];
            }
            //计算导数
            for (long rr = 0; rr <= p; rr++)
            {
                //改变数组a的行
                long s1 = 0;
                long s2 = 1;
                MatrixD a = new(n + 1, n + 1);
                a[0, 0] = 1.0;
                //循环计算k阶导数
                for (int k = 1; k <= n; k++)
                {
                    double d = 0.0;
                    long rk = rr - k;
                    long pk = p - k;
                    if (rr >= k)
                    {
                        a[s2, 0] = a[s1, 0] / ndu[pk + 1, rk];
                        d = a[s2, 0] * ndu[rk, pk];
                    }
                    long j1, j2;
                    if (rk >= -1) j1 = 1;
                    else j1 = -rk;
                    if (rr - 1 <= pk) j2 = k - 1;
                    else j2 = p - rr;
                    for (long jj = j1; jj <= j2; jj++)
                    {
                        a[s2, jj] = (a[s1, jj] - a[s1, jj - 1]) / ndu[pk + 1, rk + jj];
                        d += a[s2, jj] * ndu[rk + jj, pk];
                    }
                    if (rr <= pk)
                    {
                        a[s2, k] = -a[s1, k - 1] / ndu[pk + 1, rr];
                        d += a[s2, k] * ndu[rr, pk];
                    }
                    ders[k, rr] = d;
                    long j = s1;
                    s1 = s2;
                    s2 = j;
                }
            }
            long r = p;
            for (int k = 1; k <= n; k++)
            {
                for (int j = 0; j <= p; j++)
                    ders[k, j] *= r;
                r *= (p - k);
            }
            return ders;
        }


        internal static VectorD UniKnots(VectorD sampleLocations,
            double nFactor = 0.7,
            long degree = 3)
        {
            // nFactor range handling
            if (nFactor < 0.0)
            {
                nFactor = 0.0;
                Printer.Warning("nFactor automatically set to 0.0");
            }
            if (nFactor > 1.0)
            {
                nFactor = 1.0;
                Printer.Warning("nFactor automatically set to 1.0");
            }

            // u-sample boundaries
            double uMin = sampleLocations[0];
            double uMax = sampleLocations[^1];
            // define u-sample range division ... from experience factor
            long nDiv = (long)Math.Ceiling(nFactor * sampleLocations.Count);
            // exceptions
            if (nDiv <= 0) { nDiv = 1; }
            if (nDiv + 2 * degree >= sampleLocations.Count) { nDiv = sampleLocations.Count - 1; }

            // compute uniform knot span size
            double du = (uMax - uMin) / nDiv;
            // define knot spans
            //long spans = nDiv;
            long m = nDiv + 2 * degree;
            // define uniform knots
            VectorD temp = new VectorD(m + 1, 0.0);
            for (long i = 0; i < degree + 1; i++)
                temp[i] = uMin;
            for (long i = degree + 1; i < m - degree + 1; i++)
                temp[i] = temp[i - 1] + du;
            for (long i = m - degree + 1; i < m + 1; i++)
                temp[i] = uMax;
            return temp;
        }

        /// <summary>
        /// Origin Nmatrix calculation method with dense matrix
        /// </summary>
        /// <param name="knots"></param>
        /// <param name="degree"></param>
        /// <param name="sampleLocations"></param>
        /// <returns></returns>
        [Obsolete]
        internal static MatrixD NMatrix(VectorD knots, long degree, VectorD sampleLocations)
        {
            // initialize
            MatrixD N = new(sampleLocations.Count, knots.Count-1-degree, 0.0);

            // loop over rows
            for (long iRow = 0; iRow < N.Rows; iRow++)
            {
                double u = sampleLocations[iRow];
               
                // find the non-zero knot span
                long i = FindSpan(knots.Count - 1, degree, u, knots);
                VectorD n = BasisFuns(i, degree, u, knots);
                for (long j = 0; j <= degree; j++)  
                {
                    N[iRow, i - degree + j] = n[j];
                }
            }
            return N;           
        }

        /// <summary>
        /// NMatrix calculation method with sparse matrix
        /// </summary>
        /// <param name="knots"></param>
        /// <param name="degree"></param>
        /// <param name="sampleLocations"></param>
        /// <returns></returns>
        internal static WMatrixDi NWMatrixDi(VectorD knots, long degree, VectorD sampleLocations)
        {           
            // initialize NW for sparse matrix
            WMatrixDi NW = Sparse.InitWMatrixDi(sampleLocations.Count, 
                knots.Count - 1 - degree, (degree + 1) * sampleLocations.Count);
            VectorI rowPtr = new(sampleLocations.Count + 1);
            rowPtr[0] = 0;
            VectorI colIdx = new((degree + 1) * sampleLocations.Count);
            VectorD nzCSR = new((degree + 1) * sampleLocations.Count);

            // loop over rows
            for (long iRow = 0; iRow < sampleLocations.Count; iRow++)
            {
                double u = sampleLocations[iRow];

                // Basis function value Vector for each input u
                long i = FindSpan(knots.Count - 1, degree, u, knots);
                VectorD n = BasisFuns(i, degree, u, knots);

                rowPtr[iRow + 1] = (iRow + 1) * (degree + 1);
                for (long j = 0; j <= degree; j++)
                {
                    colIdx[iRow * (degree + 1) + j] = i - degree + j;
                    nzCSR[iRow * (degree + 1) + j] = n[j];
                }
            }

            Sparse.FillWMatrixDiCSR(ref NW, rowPtr, colIdx, nzCSR);
            return NW;
        }

        /// <summary>
        /// Calculation method of derivative value matrix
        /// </summary>
        /// <param name="knots"></param>
        /// <param name="degree"></param>
        /// <param name="sampleLocations"></param>
        /// <param name="order"></param>
        /// <returns></returns>
        internal static MatrixD NDerMatrix(VectorD knots, long degree, VectorD sampleLocations, long order)
        {
            // initialize
            MatrixD N = new(sampleLocations.Count, knots.Count - 1 - degree, 0.0);
            // loop over rows
            for (long iRow = 0; iRow < N.Rows; iRow++)
            {
                double u = sampleLocations[iRow];

                // find the non-zero knot span for p = 0
                long i = FindSpan(knots.Count - 1, degree, u, knots);
                MatrixD temp = DersBasisFuns(i, degree, u, knots, order);
                for (long j = 0; j <= degree; j++)
                {
                    N[iRow, i - degree + j] = temp[order, j];
                }
            }
            return N;
        }
    }
}
