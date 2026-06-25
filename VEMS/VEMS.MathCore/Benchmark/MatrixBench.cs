using System.Diagnostics;

namespace VEMS.MathCore
{

    /// <summary>
    /// standard benchmark for [square] matrix operations
    /// </summary>
    public class MatrixBench
    {
        #region properties

        /// <summary>
        /// Gets or sets the number of runs for each matrix size in the benchmark.
        /// </summary>
        public int Runs { get; set; } = 3;

        /// <summary>
        /// Gets or sets the default collection of [square] matrix sizes used in the benchmark.
        /// </summary>
        public List<long> MatrixSizes { get; set; }
            = [101, 201, 501, 1001, 2001, 5001];

        /// <summary>
        /// Gets or sets the unit test function to be used in the benchmark.
        /// The function takes the matrix size <paramref name="n"/> as input and returns the time cost of the test.
        /// </summary>
        public Func<long, TimeSpan>? UnitTest { get; set; }

        #endregion
        #region constructor

        /// <summary>
        /// constructs a benchmark for [square] matrix operations
        /// </summary>
        /// <param name="unitTest"> unit test definition </param>
        /// <param name="runs"> number of runs for each matrix size </param>
        /// <param name="matSizes"> collection of matrix sizes </param>
        public MatrixBench(Func<long, TimeSpan> unitTest,
            int? runs = null, List<long>? matSizes = null)
        {
            UnitTest = unitTest;
            if (runs != null) { Runs = runs.Value; }
            if (matSizes != null) { MatrixSizes = matSizes; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MatrixBench"/> class for [square] matrix operations.
        /// </summary>
        /// <param name="runs">The number of runs for each matrix size. If null, the default value is used.</param>
        /// <param name="matSizes">The collection of matrix sizes to use in the benchmark. If null, the default sizes are used.</param>
        public MatrixBench(int? runs = null, List<long>? matSizes = null)
        {
            if (runs != null) { Runs = runs.Value; }
            if (matSizes != null) { MatrixSizes = matSizes; }
        }

        #endregion
        #region methods

        /// <summary>
        /// Runs the benchmark for all configured matrix sizes and outputs a summary of the results.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown if <see cref="UnitTest"/> is not set.</exception>
        public void Run()
        {
            // exception handling
            if (UnitTest == null)
            { throw new ArgumentNullException(nameof(UnitTest)); }

            // initialize summary string
            string summary = $"\n";
            summary += $"Matrix Size \t || \t AVG [ms] \t || \t MIN [ms] \t || \t MAX [ms] \n";

            // loop over n
            foreach (var n in MatrixSizes)
            {
                Printer.WriteLine($"Running test for matrix size {n}x{n} ...");
                TimeSpan[] ts = new TimeSpan[Runs];
                TimeSpan sum = new();

                for (int i = 0; i < Runs; i++)
                {
                    Printer.WriteLine($"------- Test Run #{i} -------");
                    TimeSpan t = UnitTest(n);
                    ts[i] = t;
                    sum += t;
                }

                double avg = sum.TotalMilliseconds / Runs;
                double min = ts.Min().TotalMilliseconds;
                double max = ts.Max().TotalMilliseconds;

                Printer.WriteLine($"Time Cost: [Average] {avg} [ms]; " +
                    $"[Minimum] {min} [ms]; " +
                    $"[Maximum] {max} [ms]. ");

                // adds into summary
                summary += $"[{n}x{n}] \t || " +
                    $"\t {Converter.NumberToString(avg)} \t || " +
                    $"\t {Converter.NumberToString(min)} \t || " +
                    $"\t {Converter.NumberToString(max)} \n";
            }

            // prints summary out, at the end
            Printer.WriteLine(summary);
        }

        #endregion
        #region derived

        #region ---- matrix product ----

        /// <summary>
        /// Represents a benchmark for matrix product operations.
        /// </summary>
        public class MatrixProduct : MatrixBench
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="MatrixProduct"/> class for benchmarking matrix product operations.
            /// </summary>
            /// <param name="runs">The number of runs for each matrix size. If null, the default value is used.</param>
            /// <param name="matSizes">The collection of matrix sizes to use in the benchmark. If null, the default sizes are used.</param>
            /// <param name="isComplex">Indicates whether the matrix is complex-valued. If true, benchmarks complex matrix product; otherwise, real matrix product.</param>
            public MatrixProduct(int? runs = null,
                List<long>? matSizes = null,
                bool isComplex = true)
                : base(runs, matSizes)
            {
                if (isComplex) { UnitTest = CplxMatrixProduct; }
                else { UnitTest = RealMatrixProduct; }
            }
        }

        /// <summary>
        /// Multiplies two identical real matrices of size <paramref name="n"/> x <paramref name="n"/> and measures the time taken.
        /// </summary>
        /// <param name="n">The size of the square matrix to multiply.</param>
        /// <returns>The elapsed time required to compute the matrix product.</returns>
        private static TimeSpan RealMatrixProduct(long n)
        {
            MatrixD a = VStat.RngUniform(rows: n, cols: n);

            Stopwatch sw = Stopwatch.StartNew();
            _ = LinAlg.Dot(a, a);
            sw.Stop();

            return sw.Elapsed;
        }

        /// <summary>
        /// Multiplies two identical complex matrices of size <paramref name="n"/> x <paramref name="n"/> and measures the time taken.
        /// </summary>
        /// <param name="n">The size of the square matrix to multiply.</param>
        /// <returns>The elapsed time required to compute the matrix product.</returns>
        private static TimeSpan CplxMatrixProduct(long n)
        {
            MatrixD aRe = VStat.RngUniform(rows: n, cols: n);
            MatrixD aIm = VStat.RngGaussian(rows: n, cols: n, a: 1.0, sigma: 0.33);
            MatrixZ a = VMath.Construct(realPart: aRe, imagPart: aIm);

            Stopwatch sw = Stopwatch.StartNew();
            _ = LinAlg.Dot(a, a);
            sw.Stop();

            return sw.Elapsed;
        }

        #endregion
        #region ---- linear system solve ----

        /// <summary>
        /// benchmark for linear system solution
        /// </summary>
        public class LinearSolve : MatrixBench
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="LinearSolve"/> class for benchmarking linear system solutions.
            /// </summary>
            /// <param name="runs">The number of runs for each matrix size. If null, the default value is used.</param>
            /// <param name="matSizes">The collection of matrix sizes to use in the benchmark. If null, the default sizes are used.</param>
            /// <param name="isComplex">Indicates whether the matrix is complex-valued. If true, benchmarks complex linear system solutions; otherwise, real linear system solutions.</param>
            public LinearSolve(int? runs = null,
                List<long>? matSizes = null,
                bool isComplex = true)
                : base(runs, matSizes)
            {
                if (isComplex) { UnitTest = CplxLinearSolve; }
                else { UnitTest = RealLinearSolve; }
            }
        }

        /// <summary>
        /// Solves a linear system of equations for two identical real matrices of size <paramref name="n"/> x <paramref name="n"/> and measures the time taken.
        /// </summary>
        /// <param name="n">The size of the square matrix to solve.</param>
        /// <returns>The elapsed time required to solve the linear system.</returns>
        private static TimeSpan RealLinearSolve(long n)
        {
            MatrixD a = VStat.RngUniform(rows: n, cols: n);

            Stopwatch sw = Stopwatch.StartNew();
            _ = LinAlg.LinearSolve(a, a);
            sw.Stop();

            return sw.Elapsed;
        }

        /// <summary>
        /// Solves a linear system of equations for two identical complex matrices of size <paramref name="n"/> x <paramref name="n"/> and measures the time taken.
        /// </summary>
        /// <param name="n">The size of the square matrix to solve.</param>
        /// <returns>The elapsed time required to solve the linear system.</returns>
        private static TimeSpan CplxLinearSolve(long n)
        {
            MatrixD aRe = VStat.RngUniform(rows: n, cols: n);
            MatrixD aIm = VStat.RngGaussian(rows: n, cols: n, a: 1.0, sigma: 0.33);
            MatrixZ a = VMath.Construct(realPart: aRe, imagPart: aIm);

            Stopwatch sw = Stopwatch.StartNew();
            _ = LinAlg.LinearSolve(a, a);
            sw.Stop();

            return sw.Elapsed;
        }

        #endregion
        #region ---- SVD ----

        /// <summary>
        /// Provides a benchmark for singular value decomposition (SVD) operations on square matrices.
        /// </summary>
        /// <remarks>
        /// This class benchmarks the performance of SVD for both real and complex matrices of various sizes.
        /// The benchmark is configurable for the number of runs and the matrix sizes to test.
        /// </remarks>
        public class SVD : MatrixBench
        {
            /// <summary>
            /// constructs a benchmark for singular value decomposition
            /// </summary>
            /// <param name="runs"> number of runs for each matrix size </param>
            /// <param name="matSizes"> collection of matrix sizes </param>
            /// <param name="isComplex"> whether the matrix is complex-valued </param>
            public SVD(int? runs = null,
                List<long>? matSizes = null,
                bool isComplex = true)
                : base(runs, matSizes)
            {
                if (isComplex) { UnitTest = CplxMatrixSVD; }
                else { UnitTest = RealMatrixSVD; }
            }
        }

        /// <summary>
        /// Performs singular value decomposition (SVD) of a real matrix of size <paramref name="n"/> x <paramref name="n"/>.
        /// </summary>
        /// <param name="n">The size of the square matrix to decompose.</param>
        /// <returns>The elapsed time required to compute the SVD.</returns>
        private static TimeSpan RealMatrixSVD(long n)
        {
            MatrixD a = VStat.RngUniform(rows: n, cols: n);

            Stopwatch sw = Stopwatch.StartNew();
            LinAlg.SVDecompose(a, out _, out _, out _);
            sw.Stop();

            return sw.Elapsed;
        }

        /// <summary>
        /// Performs singular value decomposition (SVD) of a complex matrix of size <paramref name="n"/> x <paramref name="n"/>.
        /// </summary>
        /// <param name="n">The size of the square matrix to decompose.</param>
        /// <returns>The elapsed time required to compute the SVD.</returns>
        private static TimeSpan CplxMatrixSVD(long n)
        {
            MatrixD aRe = VStat.RngUniform(rows: n, cols: n);
            MatrixD aIm = VStat.RngGaussian(rows: n, cols: n, a: 1.0, sigma: 0.33);
            MatrixZ a = VMath.Construct(realPart: aRe, imagPart: aIm);

            Stopwatch sw = Stopwatch.StartNew();
            LinAlg.SVDecompose(a, out _, out _, out _);
            sw.Stop();

            return sw.Elapsed;
        }

        #endregion
        #region ---- Eigen ----

        /// <summary>
        /// Provides a benchmark for eigen decomposition of square matrices.
        /// </summary>
        /// <remarks>
        /// This class benchmarks the performance of eigen decomposition for both real and complex matrices of various sizes.
        /// The benchmark is configurable for the number of runs and the matrix sizes to test.
        /// </remarks>
        public class Eigen : MatrixBench
        {
            /// <summary>
            /// constructs a benchmark for eigen decomposition
            /// </summary>
            /// <param name="runs"> number of runs for each matrix size </param>
            /// <param name="matSizes"> collection of matrix sizes </param>
            /// <param name="isComplex"> whether the matrix is complex-valued </param>
            public Eigen(int? runs = null,
                List<long>? matSizes = null,
                bool isComplex = true)
                : base(runs, matSizes)
            {
                if (isComplex) { UnitTest = CplxMatrixEigen; }
                else { UnitTest = RealMatrixEigen; }
            }
        }

        /// <summary>
        /// Performs eigen decomposition of a real square matrix of size <paramref name="n"/> x <paramref name="n"/>.
        /// </summary>
        /// <param name="n">The size of the square matrix to decompose.</param>
        /// <returns>The elapsed time required to compute the eigen decomposition.</returns>
        private static TimeSpan RealMatrixEigen(long n)
        {
            MatrixD a = VStat.RngUniform(rows: n, cols: n);

            Stopwatch sw = Stopwatch.StartNew();
            LinAlg.EigenSystem(ref a, out _, out _);
            sw.Stop();

            return sw.Elapsed;
        }

        /// <summary>
        /// Performs eigen decomposition of a complex square matrix of size <paramref name="n"/> x <paramref name="n"/>.
        /// </summary>
        /// <param name="n">The size of the square matrix to decompose.</param>
        /// <returns>The elapsed time required to compute the eigen decomposition.</returns>
        private static TimeSpan CplxMatrixEigen(long n)
        {
            MatrixD aRe = VStat.RngUniform(rows: n, cols: n);
            MatrixD aIm = VStat.RngGaussian(rows: n, cols: n, a: 1.0, sigma: 0.33);
            MatrixZ a = VMath.Construct(realPart: aRe, imagPart: aIm);

            Stopwatch sw = Stopwatch.StartNew();
            LinAlg.EigenSystem(ref a, out _, out _);
            sw.Stop();

            return sw.Elapsed;
        }

        #endregion
        #region ---- FFT ----

        /// <summary>
        /// benchmark for FFT
        /// </summary>
        public class FFT : MatrixBench
        {
            /// <summary>
            /// constructs a benchmark for FFT
            /// </summary>
            /// <param name="runs"> number of runs for each matrix size </param>
            /// <param name="matSizes"> collection of matrix sizes </param>
            /// <param name="isComplex"> whether the matrix is complex-valued </param>
            public FFT(int? runs = null,
                List<long>? matSizes = null,
                bool isComplex = true)
                : base(runs, matSizes)
            {
                if (isComplex) { UnitTest = CplxMatrixFFT; }
                else { throw new NotSupportedException(); }
            }

        }

        /// <summary>
        /// Performs a two-dimensional fast Fourier transform (FFT) on a complex matrix of size <paramref name="n"/> x <paramref name="n"/>.
        /// </summary>
        /// <param name="n">The size of the square matrix to transform.</param>
        /// <returns>The elapsed time required to compute the FFT.</returns>
        public static TimeSpan CplxMatrixFFT(long n)
        {
            MatrixD aRe = VStat.RngUniform(rows: n, cols: n);
            MatrixD aIm = VStat.RngGaussian(rows: n, cols: n, a: 1.0, sigma: 0.33);
            MatrixZ a = VMath.Construct(realPart: aRe, imagPart: aIm);
            //GridInfo2D g = new(n, n);

            Stopwatch sw = Stopwatch.StartNew();
            Transform.FFT2D(ref a, //ref g,
                direction: FFTOptions.Direction.Forward,
                convention: FFTOptions.Convention.ZeroCentered,
                conversion: FFTOptions.Conversion.DataShift,
                copyMode: FFTOptions.CopyMode.Block,
                loopMode: FFTOptions.LoopMode.Parallel);
            
            sw.Stop();

            return sw.Elapsed;
        }

        #endregion

        #endregion


        // temporary test code
        ///// <summary>
        ///// for loop test with least square operation
        ///// </summary>
        ///// <param name="nRows"> total number of rows in the loop </param>
        ///// <param name="nCols"> total number of columns in the loop </param>
        ///// <param name="aRows"> number of rows in test matrix a </param>
        ///// <param name="aCols"> number of columns in test matrix a </param>
        ///// <param name="loopMode"> loop-computational mode option </param>
        //public static void LeastSquareFor(long nRows, long nCols,
        //    long aRows, long aCols,
        //    LoopMode loopMode = LoopMode.Sequential)
        //{
        //    // generates matrix a as the unit
        //    MatrixD a = VStat.RngUniform(rows: aRows, cols: aCols);
        //    VectorD b = VStat.RngUniform(n: aRows);

        //    // defines loop kernel operation 
        //    Action<long, long> op = (iRow, iCol) =>
        //    {
        //        VectorD x = LinAlg.QRLeastSquare(a, b);
        //    };

        //    // loop ...
        //    Loop2D loop = new(operation: op,
        //        rowStart: 0, rowEnd: nRows,
        //        colStart: 0, colEnd: nCols);
        //    loop.Evaluate(loopMode);
        //}

    }

}
