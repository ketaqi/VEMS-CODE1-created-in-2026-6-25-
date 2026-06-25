using System.Numerics;

namespace VEMS.MathCore
{
    internal class VSKernel
    {
        internal IVSL iVSL { get; set; }
        internal VSKernel()
        {
            iVSL = Defaults.IVSL; // Config.DefaultIVSL;
        }
    }

    /// <summary>
    /// vector statistics methods
    /// </summary>
    public class VStat
    {
        private static VSKernel kernel = new();

        #region Basic setting methods

        /// <summary>
        /// set up the VSL interface with options
        /// from IntelMKL, OpenBLAS, etc
        /// </summary>
        /// <param name="option"> VSL interface options </param>
        public static void SetIVSL(IVSL option)
            => kernel.iVSL = option;

        #endregion

        #region --------- Random ---------

        #region uniform [coutinuous]
        /// <summary>
        /// generates random numbers with uniform distribution
        /// </summary>
        /// <param name="n"> number of random values to be generated </param>
        /// <param name="rand"> array of n random numbers </param>
        /// <param name="a"> left bound </param>
        /// <param name="b"> right bound </param>
        private static void RngUniform(long n, DenseArrayBase<double> rand, double a, double b)
        {
            // create a stream
            IntPtr stream = new();
            kernel.iVSL.NewStream(ref stream, VSLBRNG.MCG31, 1);
            // generate randoms
            kernel.iVSL.RngUniform(VSLRNGMethod.Uniform_Std, stream, n, rand, a, b);
            // delete the stream
            kernel.iVSL.DeleteStream(ref stream);
        }

        /// <summary>
        /// generates random numbers with uniform distribution
        /// </summary>
        /// <param name="n"> number of random values to be generated </param>
        /// <param name="a"> left bound </param>
        /// <param name="b"> right bound </param>
        /// <returns> vector of n random numbers </returns>
        public static VectorD RngUniform(long n, double a = 0.0, double b = 1.0)
        {
            VectorD rand = new(n);
            RngUniform(n, rand, a, b);
            return rand;
        }

        /// <summary>
        /// generates random numbers with uniform distribution
        /// </summary>
        /// <param name="rows"> number of rows </param>
        /// <param name="cols"> number of columns </param>
        /// <param name="a"> left bound </param>
        /// <param name="b"> right bound </param>
        /// <returns> matrix of (rows*cols) random numbers </returns>
        public static MatrixD RngUniform(long rows, long cols, double a = 0.0, double b = 1.0)
        {
            MatrixD rand = new(rows, cols);
            RngUniform(rand.Count, rand, a, b);
            return rand;
        }
        #endregion
        #region Gaussian [continuous]
        /// <summary>
        /// generates normally distributed random numbers
        /// </summary>
        /// <param name="n"> number of random values to be generated </param>
        /// <param name="rand"> array of n random numbers </param>
        /// <param name="a"> mean value </param>
        /// <param name="sigma"> standard deviation </param>
        private static void RngGaussian(long n, DenseArrayBase<double> rand, double a, double sigma)
        {
            // create a stream
            IntPtr stream = new();
            kernel.iVSL.NewStream(ref stream, VSLBRNG.MCG31, 1);
            // generate randoms
            kernel.iVSL.RngGaussian(VSLRNGMethod.Gaussian_Boxmuller, stream, n, rand, a, sigma);
            // delete the stream
            kernel.iVSL.DeleteStream(ref stream);
        }

        /// <summary>
        /// generates normally distributed random numbers
        /// </summary>
        /// <param name="n"> number of random values to be generated </param>
        /// <param name="a"> mean value </param>
        /// <param name="sigma"> standard deviation </param>
        /// <returns> vector of n random numbers </returns>
        public static VectorD RngGaussian(long n, double a, double sigma)
        {
            VectorD rand = new(n);
            RngGaussian(n, rand, a, sigma);
            return rand;
        }

        /// <summary>
        /// generates normally distributed random numbers
        /// </summary>
        /// <param name="rows"> number of rows </param>
        /// <param name="cols"> number of columns </param>
        /// <param name="a"> mean value </param>
        /// <param name="sigma"> standard deviation </param>
        /// <returns> matrix of (rows*cols) random numbers </returns>
        public static MatrixD RngGaussian(long rows, long cols, double a, double sigma)
        {
            MatrixD rand = new(rows, cols);
            RngGaussian(rand.Count, rand, a, sigma);
            return rand;
        }
        #endregion
        #region exponential [continuous]
        /// <summary>
        /// generates exponentially distributed random numbers
        /// </summary>
        /// <param name="n"> number of random values to be generated </param>
        /// <param name="rand"> array of n random numbers </param>
        /// <param name="a"> displacement </param>
        /// <param name="beta"> scale factor </param>
        private static void RngExponential(long n, DenseArrayBase<double> rand, double a, double beta)
        {
            // create a stream
            IntPtr stream = new();
            kernel.iVSL.NewStream(ref stream, VSLBRNG.MCG31, 1);
            // generate randoms
            kernel.iVSL.RngExponential(VSLRNGMethod.Exponential_ICDF, stream, n, rand, a, beta);
            // delete the stream
            kernel.iVSL.DeleteStream(ref stream);
        }

        /// <summary>
        /// generates exponentially distributed random numbers
        /// </summary>
        /// <param name="n"> number of random values to be generated </param>
        /// <param name="a"> displacement </param>
        /// <param name="beta"> scale factor </param>
        /// <returns> vector of n random numbers </returns>
        public static VectorD RngExponential(long n, double a, double beta)
        {
            VectorD rand = new(n);
            RngExponential(n, rand, a, beta);
            return rand;
        }

        /// <summary>
        /// generates exponentially distributed random numbers
        /// </summary>
        /// <param name="rows"> number of rows </param>
        /// <param name="cols"> number of columns </param>
        /// <param name="a"> displacement </param>
        /// <param name="beta"> scale factor </param>
        /// <returns> matrix of (rows*cols) random numbers </returns>
        public static MatrixD RngExponential(long rows, long cols, double a, double beta)
        {
            MatrixD rand = new(rows, cols);
            RngExponential(rand.Count, rand, a, beta);
            return rand;
        }
        #endregion
        #region Laplace [continuous]
        /// <summary>
        /// generates random numbers with laplace distribution
        /// </summary>
        /// <param name="n"> number of random values to be generated </param>
        /// <param name="rand"> array of n random numbers </param>
        /// <param name="a"> mean value </param>
        /// <param name="beta"> scale factor </param>
        private static void RngLaplace(long n, DenseArrayBase<double> rand, double a, double beta)
        {
            // create a stream
            IntPtr stream = new();
            kernel.iVSL.NewStream(ref stream, VSLBRNG.MCG31, 1);
            // generate randoms
            kernel.iVSL.RngLaplace(VSLRNGMethod.Laplace_ICDF, stream, n, rand, a, beta);
            // delete the stream
            kernel.iVSL.DeleteStream(ref stream);
        }

        /// <summary>
        /// generates random numbers with laplace distribution
        /// </summary>
        /// <param name="n"> number of random values to be generated </param>
        /// <param name="a"> mean value </param>
        /// <param name="beta"> scale factor </param>
        /// <returns> vector of n random numbers </returns>
        public static VectorD RngLaplace(long n, double a, double beta)
        {
            VectorD rand = new(n);
            RngLaplace(n, rand, a, beta);
            return rand;
        }

        /// <summary>
        /// generates random numbers with laplace distribution
        /// </summary>
        /// <param name="rows"> number of rows </param>
        /// <param name="cols"> number of columns </param>
        /// <param name="a"> mean value </param>
        /// <param name="beta"> scale factor </param>
        /// <returns> matrix of (rows*cols) random numbers </returns>
        public static MatrixD RngLaplace(long rows, long cols, double a, double beta)
        {
            MatrixD rand = new(rows, cols);
            RngLaplace(rand.Count, rand, a, beta);
            return rand;
        }
        #endregion
        #region Gamma [continuous]
        /// <summary>
        /// generates gamma distributed random numbers
        /// </summary>
        /// <param name="n"> number of random values to be generated </param>
        /// <param name="rand"> array of n random numbers </param>
        /// <param name="alpha"> shape </param>
        /// <param name="a"> displacement </param>
        /// <param name="beta"> scale factor </param>
        private static void RngGamma(long n, DenseArrayBase<double> rand, double alpha, double a, double beta)
        {
            // create a stream
            IntPtr stream = new();
            kernel.iVSL.NewStream(ref stream, VSLBRNG.MCG31, 1);
            // generate randoms
            kernel.iVSL.RngGamma(VSLRNGMethod.Gamma_Gnorm, stream, n, rand, alpha, a, beta);
            // delete the stream
            kernel.iVSL.DeleteStream(ref stream);
        }

        /// <summary>
        /// generates gamma distributed random numbers
        /// </summary>
        /// <param name="n"> number of random values to be generated </param>
        /// <param name="alpha"> shape </param>
        /// <param name="a"> displacement </param>
        /// <param name="beta"> scale factor </param>
        /// <returns> vector of n random numbers </returns>
        public static VectorD RngGamma(long n, double alpha, double a, double beta)
        {
            VectorD rand = new(n);
            RngGamma(n, rand, alpha, a, beta);
            return rand;
        }

        /// <summary>
        /// generates gamma distributed random numbers
        /// </summary>
        /// <param name="rows"> number of rows </param>
        /// <param name="cols"> number of columns </param>
        /// <param name="alpha"> shape </param>
        /// <param name="a"> displacement </param>
        /// <param name="beta"> scale factor </param>
        /// <returns> matrix of (rows*cols) random numbers </returns>
        public static MatrixD RngGamma(long rows, long cols, double alpha, double a, double beta)
        {
            MatrixD rand = new(rows, cols);
            RngGamma(rand.Count, rand, alpha, a, beta);
            return rand;
        }
        #endregion
        #region uniform [discrete]
        /// <summary>
        /// generates random numbers with uniform distribution
        /// </summary>
        /// <param name="n"> number of random values to be generated </param>
        /// <param name="rand"> array of n random numbers </param>
        /// <param name="a"> left bound </param>
        /// <param name="b"> right bound </param>
        private static void RngUniform(long n, long[] rand, long a, long b)
        {
            // create a stream
            IntPtr stream = new();
            kernel.iVSL.NewStream(ref stream, VSLBRNG.MCG31, 1);
            // generate randoms
            kernel.iVSL.RngUniform(VSLRNGMethod.Uniform_Std, stream, n, rand, a, b);
            // delete the stream
            kernel.iVSL.DeleteStream(ref stream);
        }

        /// <summary>
        /// generates random numbers with uniform distribution
        /// </summary>
        /// <param name="n"> number of random values to be generated </param>
        /// <param name="a"> left bound </param>
        /// <param name="b"> right bound </param>
        /// <returns> array of n random numbers </returns>
        public static long[] RngUniform(long n, long a, long b)
        {
            long[] rand = new long[n];
            RngUniform(n, rand, a, b);
            return rand;
        }
        #endregion
        #region uniformBits32 [discrete]
        /// <summary>
        /// generates uniformly distributed bits in 32-bit chunks
        /// </summary>
        /// <param name="n"> number of random values to be generated </param>
        /// <param name="rand"> array of n random numbers </param>
        private static void RngUniformBits32(long n, ulong[] rand)
        {
            // create a stream
            IntPtr stream = new();
            kernel.iVSL.NewStream(ref stream, VSLBRNG.MCG31, 1);
            // generate randoms
            kernel.iVSL.RngUniformBits32(VSLRNGMethod.UniformBits32_Std, stream, n, rand);
            // delete the stream
            kernel.iVSL.DeleteStream(ref stream);
        }

        /// <summary>
        /// generates uniformly distributed bits in 32-bit chunks
        /// </summary>
        /// <param name="n"> number of random values to be generated </param>
        /// <returns> array of n random numbers </returns>
        public static ulong[] RanUniformBits32(long n)
        {
            ulong[] rand = new ulong[n];
            RngUniformBits32(n, rand);
            return rand;
        }
        #endregion
        #region uniformBits64 [discrete]
        /// <summary>
        /// generates uniformly distributed bits in 64-bit chunks
        /// </summary>
        /// <param name="n"> number of random values to be generated </param>
        /// <param name="rand"> array of n random numbers </param>
        private static void RngUniformBits64(long n, ulong[] rand)
        {
            // create a stream
            IntPtr stream = new();
            kernel.iVSL.NewStream(ref stream, VSLBRNG.MCG31, 1);
            // generate randoms
            kernel.iVSL.RngUniformBits64(VSLRNGMethod.UniformBits64_Std, stream, n, rand);
            // delete the stream
            kernel.iVSL.DeleteStream(ref stream);
        }

        /// <summary>
        /// generates uniformly distributed bits in 64-bit chunks
        /// </summary>
        /// <param name="n"> number of random values to be generated </param>
        /// <returns> array of n random numbers </returns>
        public static ulong[] RngUniformBits64(long n)
        {
            ulong[] rand = new ulong[n];
            RngUniformBits64(n, rand);
            return rand;
        }
        #endregion
        #region geometric [discrete]
        /// <summary>
        /// generates geometrically distributed random values
        /// </summary>
        /// <param name="n"> number of random values to be generated </param>
        /// <param name="rand"> array of n random numbers </param>
        /// <param name="p"> success probability of a trial </param>
        private static void RngGeometric(long n, long[] rand, double p)
        {
            // create a stream
            IntPtr stream = new();
            kernel.iVSL.NewStream(ref stream, VSLBRNG.MCG31, 1);
            // generate randoms
            kernel.iVSL.RngGeometric(VSLRNGMethod.Geometric_ICDF, stream, n, rand, p);
            // delete the stream
            kernel.iVSL.DeleteStream(ref stream);
        }

        /// <summary>
        /// generates geometrically distributed random values
        /// </summary>
        /// <param name="n"> number of random values to be generated </param>
        /// <param name="p"> success probability of a trial </param>
        /// <returns> array of n random numbers </returns>
        public static long[] RngGeometric(long n, double p)
        {
            long[] rand = new long[n];
            RngGeometric(n, rand, p);
            return rand;
        }
        #endregion
        #region Poisson [discrete]
        /// <summary>
        /// generates Poisson distributed random values
        /// </summary>
        /// <param name="n"> number of random values to be generated </param>
        /// <param name="rand"> array of n random numbers </param>
        /// <param name="lambda"> distribution parameter </param>
        private static void RngPoisson(long n, long[] rand, double lambda)
        {
            // create a stream
            IntPtr stream = new();
            kernel.iVSL.NewStream(ref stream, VSLBRNG.MCG31, 1);
            // generate randoms
            kernel.iVSL.RngPoisson(VSLRNGMethod.Poisson_PTPE, stream, n, rand, lambda);
            // delete the stream
            kernel.iVSL.DeleteStream(ref stream);
        }

        /// <summary>
        /// generates Poisson distributed random values
        /// </summary>
        /// <param name="n"> number of random values to be generated </param>
        /// <param name="lambda"> distribution parameter </param>
        /// <returns> array of n random numbers </returns>
        public static long[] RngPoisson(long n, double lambda)
        {
            long[] rand = new long[n];
            RngPoisson(n, rand, lambda);
            return rand;
        }
        #endregion

        #endregion
        #region --------- 1D Convolution ---------

        /// <summary>
        /// performs convolution between vector x and y
        /// and save the result into vector z
        /// </summary>
        /// <param name="x"> input vector x </param>
        /// <param name="y"> input vector y </param>
        /// <param name="z"> output vector z </param>
        /// <param name="mode"> convolution computation mode option </param>
        /// <param name="start"> start index in the output vector z </param>
        private static void Convolution(DenseArrayBase<double> x, 
            DenseArrayBase<double> y, DenseArrayBase<double> z,
            ConvMode mode = ConvMode.Auto, long start = 0)
        {
            // create a task
            IntPtr task = new();
            kernel.iVSL.ConvNewTaskReal1D(ref task, mode, x.Count, y.Count, z.Count);
            // set the start in output z
            if (start != 0)
                kernel.iVSL.ConvSetStart(task, new long[1] { start });
            // execute the task
            kernel.iVSL.ConvExec(task, x, 1, y, 1, z, 1);
            // delete the task after execution
            kernel.iVSL.ConvDeleteTask(ref task);
        }

        /// <summary>
        /// performs convolution between vector x and y
        /// </summary>
        /// <param name="x"> input vector x </param>
        /// <param name="y"> input vector y </param>
        /// <param name="mode"> convolution computation mode option </param>
        /// <param name="start"> start index in the output vector </param>
        /// <returns> output vector </returns>
        public static VectorD Convolution(VectorD x, VectorD y,
            ConvMode mode = ConvMode.Auto, long start = 0)
        {
            // initialize output
            VectorD z = new(x.Count + y.Count - 1);
            // perform convolution
            Convolution(x, y, z, mode, start);
            return z;
        }

        /// <summary>
        /// performs convolution between vector x and y
        /// and save the result into vector z
        /// </summary>
        /// <param name="x"> input vector x </param>
        /// <param name="y"> input vector y </param>
        /// <param name="z"> output vector z </param>
        /// <param name="mode"> convolution computation mode option </param>
        /// <param name="start"> start index in the output vector z </param>
        private static void Convolution(DenseArrayBase<Complex> x, 
            DenseArrayBase<Complex> y, DenseArrayBase<Complex> z,
            ConvMode mode = ConvMode.Auto, long start = 0)
        {
            // create a task
            IntPtr task = new();
            _ = kernel.iVSL.ConvNewTaskComplex1D(ref task, mode, x.Count, y.Count, z.Count);
            // set the start in output z
            if (start != 0)
                _ = kernel.iVSL.ConvSetStart(task, new long[1] { start });
            // execute the task
            _ = kernel.iVSL.ConvExec(task, x, 1, y, 1, z, 1);
            // delete the task after execution
            _ = kernel.iVSL.ConvDeleteTask(ref task);
        }

        /// <summary>
        /// performs convolution between vector x and y
        /// </summary>
        /// <param name="x"> input vector x </param>
        /// <param name="y"> input vector y </param>
        /// <param name="mode"> convolution computation mode option </param>
        /// <param name="start"> start index in the output vector </param>
        /// <returns> output vector </returns>
        public static VectorZ Convolution(VectorZ x, VectorZ y,
            ConvMode mode = ConvMode.Auto, long start = 0)
        {
            // initialize output
            VectorZ z = new(x.Count + y.Count - 1);
            // perform convolution
            Convolution(x, y, z, mode, start);
            return z;
        }

        #endregion
        #region --------- 2D Convolution ---------

        /// <summary>
        /// performs convolution between matrix x and y
        /// and save the result into matrix z
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <param name="y"> input matrix y </param>
        /// <param name="z"> output matrix z </param>
        /// <param name="mode"> convolution computation mode option </param>
        /// <param name="startRow"> start row index in the output matrix z </param>
        /// <param name="startCol"> start column index in the output matrix z </param>
        private static void Convolution(MatrixD x, 
            MatrixD y, MatrixD z,
            ConvMode mode = ConvMode.Auto, long startRow = 0, long startCol = 0)
        {
            // create a task
            IntPtr task = new();
            _ = kernel.iVSL.ConvNewTaskReal(ref task, mode, 2,
                new long[2] { x.Cols, x.Rows }, //new long[2] { x.Rows, x.Cols },
                new long[2] { y.Cols, y.Rows }, //new long[2] { y.Rows, y.Cols },
                new long[2] { z.Cols, z.Rows }); //new long[2] { z.Rows, z.Cols });
            // set the start in output z
            if (startRow != 0 || startCol != 0)
                _ = kernel.iVSL.ConvSetStart(task, new long[] { startRow, startCol });
            // execute the task
            _ = kernel.iVSL.ConvExec(task,
                x, null,
                y, null,
                z, null);
            // delete the task after execution
            _ = kernel.iVSL.ConvDeleteTask(ref task);
        }

        /// <summary>
        /// performs convolution between matrix x and y
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <param name="y"> input matrix y </param>
        /// <param name="mode"> convolution computation mode option </param>
        /// <param name="startRow"> start row index in the output matrix z </param>
        /// <param name="startCol"> start column index in the output matrix z </param>
        /// <returns> output matrix </returns>
        public static MatrixD Convolution(MatrixD x, MatrixD y,
            ConvMode mode = ConvMode.Auto, long startRow = 0, long startCol = 0)
        {
            // initialize output
            MatrixD z = new(x.Rows + y.Rows - 1, x.Cols + y.Cols - 1);
            // perform convolution
            Convolution(x, y, z, mode, startRow, startCol);
            return z;
        }

        /// <summary>
        /// performs convolution between matrix x and y
        /// and save the result into matrix z
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <param name="y"> input matrix y </param>
        /// <param name="z"> output matrix z </param>
        /// <param name="mode"> convolution computation mode option </param>
        /// <param name="startRow"> start row index in the output matrix z </param>
        /// <param name="startCol"> start column index in the output matrix z </param>
        private static void Convolution(MatrixZ x, 
            MatrixZ y, MatrixZ z,
            ConvMode mode = ConvMode.Auto, long startRow = 0, long startCol = 0)
        {
            // create a task
            IntPtr task = new();
            _ = kernel.iVSL.ConvNewTaskComplex(ref task, mode, 2,
                new long[2] { x.Cols, x.Rows }, //new long[2] { x.Rows, x.Cols },
                new long[2] { y.Cols, y.Rows }, //new long[2] { y.Rows, y.Cols },
                new long[2] { z.Cols, z.Rows }); // new long[2] { z.Rows, z.Cols });
            // set the start in output z
            if (startRow != 0 || startCol != 0)
                _ = kernel.iVSL.ConvSetStart(task, new long[] { startRow, startCol });
            // execute the task
            _ = kernel.iVSL.ConvExec(task,
                x, null,
                y, null,
                z, null);
            // delete the task after execution
            _ = kernel.iVSL.ConvDeleteTask(ref task);
        }

        /// <summary>
        /// performs convolution between matrix x and y
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <param name="y"> input matrix y </param>
        /// <param name="mode"> convolution computation mode option </param>
        /// <param name="startRow"> start row index in the output matrix z </param>
        /// <param name="startCol"> start column index in the output matrix z </param>
        /// <returns> output matrix </returns>
        public static MatrixZ Convolution(MatrixZ x, MatrixZ y,
            ConvMode mode = ConvMode.Auto, long startRow = 0, long startCol = 0)
        {
            // initialize output
            MatrixZ z = new(x.Rows + y.Rows - 1, x.Cols + y.Cols - 1);
            // perform convolution
            Convolution(x, y, z, mode, startRow, startCol);
            return z;
        }

        #endregion

    }
}
