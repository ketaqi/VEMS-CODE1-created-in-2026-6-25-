using Complex = System.Numerics.Complex;

namespace VEMS.MathCore
{
    /// <summary>
    /// VSL interface
    /// </summary>
    public interface IVSL
    {

        #region --------- Random ---------
        #region stream
        /// <summary>
        /// creates and initializes a random stream
        /// </summary>
        /// <param name="stream"> pointer to the stream </param>
        /// <param name="brng"> index of the basic generator to initialize the stream </param>
        /// <param name="seed"> innitial condition of the stream </param>
        /// <returns> VSL status </returns>
        int NewStream(ref IntPtr stream, VSLBRNG brng, ulong seed);

        /// <summary>
        /// deletes a random stream
        /// </summary>
        /// <param name="stream"> pointer to the stream </param>
        /// <returns> VSL status </returns>
        int DeleteStream(ref IntPtr stream);
        #endregion
        #region RNGs [continuous]
        /// <summary>
        /// generates random numbers with uniform distribution [continuous]
        /// </summary>
        /// <param name="method"> generation method </param>
        /// <param name="stream"> pointer to the stream </param>
        /// <param name="n"> number of random values to be generated </param>
        /// <param name="r"> array of n random numbers </param>
        /// <param name="a"> left bound </param>
        /// <param name="b"> right bound </param>
        /// <returns> VSL status </returns>
        int RngUniform(VSLRNGMethod method, IntPtr stream,
            long n, DenseArrayBase<double> r, double a, double b);

        /// <summary>
        /// generates normally distributed random numbers [continuous]
        /// </summary>
        /// <param name="method"> generation method </param>
        /// <param name="stream"> pointer to the stream </param>
        /// <param name="n"> number of random values to be generated </param>
        /// <param name="r"> array of n random numbers </param>
        /// <param name="a"> mean value </param>
        /// <param name="sigma"> standard deviation </param>
        /// <returns> VSL status </returns>
        int RngGaussian(VSLRNGMethod method, IntPtr stream,
            long n, DenseArrayBase<double> r, double a, double sigma);

        /// <summary>
        /// generates exponentially distributed random numbers [continuous]
        /// </summary>
        /// <param name="method"> generation method </param>
        /// <param name="stream"> pointer to the stream </param>
        /// <param name="n"> number of random values to be generated </param>
        /// <param name="r"> array of n random numbers </param>
        /// <param name="a"> displacement </param>
        /// <param name="beta"> scale factor </param>
        /// <returns> VSL status </returns>
        int RngExponential(VSLRNGMethod method, IntPtr stream,
            long n, DenseArrayBase<double> r, double a, double beta);

        /// <summary>
        /// generates random numbers with laplace distribution [continuous]
        /// </summary>
        /// <param name="method"> generation method </param>
        /// <param name="stream"> pointer to the stream </param>
        /// <param name="n"> number of random values to be generated </param>
        /// <param name="r"> array of n random numbers </param>
        /// <param name="a"> mean value </param>
        /// <param name="beta"> scale factor </param>
        /// <returns> VSL status </returns>
        int RngLaplace(VSLRNGMethod method, IntPtr stream,
            long n, DenseArrayBase<double> r, double a, double beta);

        /// <summary>
        /// generates gamma distributed random numbers [continuous]
        /// </summary>
        /// <param name="method"> generation method </param>
        /// <param name="stream"> pointer to the stream </param>
        /// <param name="n"> number of random values to be generated </param>
        /// <param name="r"> array of n random numbers </param>
        /// <param name="alpha"> shape </param>
        /// <param name="a"> displacement </param>
        /// <param name="beta"> scale factor </param>
        /// <returns> VSL status </returns>
        int RngGamma(VSLRNGMethod method, IntPtr stream,
            long n, DenseArrayBase<double> r, double alpha, double a, double beta);
        #endregion
        #region RNGs [discrete]
        /// <summary>
        /// generates random numbers with uniform distribution [discrete]
        /// </summary>
        /// <param name="method"> generation method </param>
        /// <param name="stream"> pointer to the stream </param>
        /// <param name="n"> number of random values to be generated </param>
        /// <param name="r"> array of n random numbers </param>
        /// <param name="a"> left bound </param>
        /// <param name="b"> right bound </param>
        /// <returns> VSL status </returns>
        int RngUniform(VSLRNGMethod method, IntPtr stream,
            long n, long[] r, long a, long b);

        /// <summary>
        /// generates uniformly distributed bits in 32-bit chunks [discrete]
        /// </summary>
        /// <param name="method"> generation method </param>
        /// <param name="stream"> pointer to the stream </param>
        /// <param name="n"> number of random values to be generated </param>
        /// <param name="r"> array of n random numbers </param>
        /// <returns> VSL status </returns>
        int RngUniformBits32(VSLRNGMethod method, IntPtr stream,
            long n, ulong[] r);

        /// <summary>
        /// generates uniformly distributed bits in 64-bit chunks [discrete]
        /// </summary>
        /// <param name="method"> generation method </param>
        /// <param name="stream"> pointer to the stream </param>
        /// <param name="n"> number of random values to be generated </param>
        /// <param name="r"> array of n random numbers </param>
        /// <returns> VSL status </returns>
        public unsafe int RngUniformBits64(VSLRNGMethod method, IntPtr stream,
            long n, ulong[] r);

        /// <summary>
        /// generates geometrically distributed random values [discrete]
        /// </summary>
        /// <param name="method"> generation method </param>
        /// <param name="stream"> pointer to the stream </param>
        /// <param name="n"> number of random values to be generated </param>
        /// <param name="r"> array of n random numbers </param>
        /// <param name="p"> success probability of a trial </param>
        /// <returns> VSL status </returns>
        int RngGeometric(VSLRNGMethod method, IntPtr stream,
            long n, long[] r, double p);

        /// <summary>
        /// generates Poisson distributed random values [discrete]
        /// </summary>
        /// <param name="method"> generation method </param>
        /// <param name="stream"> pointer to the stream </param>
        /// <param name="n"> number of random values to be generated </param>
        /// <param name="r"> array of n random numbers </param>
        /// <param name="lambda"> distribution parameter </param>
        /// <returns> VSL status </returns>
        int RngPoisson(VSLRNGMethod method, IntPtr stream,
            long n, long[] r, double lambda);
        #endregion
        #endregion
        #region --------- Convolution ---------

        /// <summary>
        /// creates a new convolution task descriptor 
        /// for multidimensional real-valued case
        /// </summary>
        /// <param name="task"> pointer to the task descriptor </param>
        /// <param name="mode"> convolution computational mode </param>
        /// <param name="dims"> rank of data (must be in the range from 1 to 7) </param>
        /// <param name="xshape"> shape of the input data in the array x </param>
        /// <param name="yshape"> shape of the input data in the array y </param>
        /// <param name="zshape"> shape of the output data in the array z </param>
        /// <returns> VSL status </returns>
        int ConvNewTaskReal(ref IntPtr task,
            ConvMode mode, int dims, long[] xshape, long[] yshape, long[] zshape);

        /// <summary>
        /// creates a new convolution task descriptor 
        /// for multidimensional complex-valued case
        /// </summary>
        /// <param name="task"> pointer to the task descriptor </param>
        /// <param name="mode"> convolution computational mode </param>
        /// <param name="dims"> rank of data (must be in the range from 1 to 7) </param>
        /// <param name="xshape"> shape of the input data in the array x </param>
        /// <param name="yshape"> shape of the input data in the array y </param>
        /// <param name="zshape"> shape of the output data in the array z </param>
        /// <returns> VSL status </returns>
        int ConvNewTaskComplex(ref IntPtr task,
            ConvMode mode, int dims, long[] xshape, long[] yshape, long[] zshape);

        /// <summary>
        /// creates a new convolution task descriptor 
        /// for one-dimensional real-valued case
        /// </summary>
        /// <param name="task"> pointer to the task descriptor </param>
        /// <param name="mode"> convolution computational mode </param>
        /// <param name="xshape"> shape of the input data in the array x </param>
        /// <param name="yshape"> shape of the input data in the array y </param>
        /// <param name="zshape"> shape of the output data in the array z </param>
        /// <returns> VSL status </returns>
        int ConvNewTaskReal1D(ref IntPtr task,
            ConvMode mode, long xshape, long yshape, long zshape);

        /// <summary>
        /// creates a new convolution task descriptor 
        /// for one-dimensional complex-valued case
        /// </summary>
        /// <param name="task"> pointer to the task descriptor </param>
        /// <param name="mode"> convolution computational mode </param>
        /// <param name="xshape"> shape of the input data in the array x </param>
        /// <param name="yshape"> shape of the input data in the array y </param>
        /// <param name="zshape"> shape of the output data in the array z </param>
        /// <returns> VSL status </returns>
        int ConvNewTaskComplex1D(ref IntPtr task,
            ConvMode mode, long xshape, long yshape, long zshape);

        /// <summary>
        /// changes the value of the parameter mode 
        /// in the convolution task descriptor
        /// </summary>
        /// <param name="task"> pointer to the task descriptor </param>
        /// <param name="mode"> new value of the parameter mode </param>
        /// <returns> VSL status </returns>
        int ConvSetMode(IntPtr task, ConvMode mode);

        /// <summary>
        /// changes the value of the parameter internal_precision 
        /// in the convolution task descriptor
        /// </summary>
        /// <param name="task"> pointer to the task descriptor </param>
        /// <param name="precision"> new value of the parameter internal-precision </param>
        /// <returns> VSL status </returns>
        int ConvSetInternalPrecision(IntPtr task, ConvPrecision precision);

        /// <summary>
        /// changes the value of the parameter start 
        /// in the convolution task descriptor
        /// </summary>
        /// <param name="task"> pointer to the task descriptor </param>
        /// <param name="start"> new value of the parameter start </param>
        /// <returns> VSL status </returns>
        int ConvSetStart(IntPtr task, long[] start);

        /// <summary>
        /// changes the value of the parameter decimation 
        /// in the convolution task descriptor
        /// </summary>
        /// <param name="task"> pointer to the task descriptor </param>
        /// <param name="decimation"> new value of the parameter decimation </param>
        /// <returns></returns>
        int ConvSetDecimation(IntPtr task, long[] decimation);

        /// <summary>
        /// computes convolution for multidimensional real-valued case
        /// </summary>
        /// <param name="task"> pointer to the task descriptor </param>
        /// <param name="x"> array containing input data x </param>
        /// <param name="xstride"> strides for input data x </param>
        /// <param name="y"> array containing input data y </param>
        /// <param name="ystride"> stride for input data y </param>
        /// <param name="z"> array containing output data z </param>
        /// <param name="zstride"> stride for output data z </param>
        /// <returns> VSL status </returns>
        int ConvExec(IntPtr task, DenseArrayBase<double> x, long[] xstride,
            DenseArrayBase<double> y, long[] ystride,
            DenseArrayBase<double> z, long[] zstride);

        /// <summary>
        /// computes convolution for multidimensional complex-valued case
        /// </summary>
        /// <param name="task"> pointer to the task descriptor </param>
        /// <param name="x"> array containing input data x </param>
        /// <param name="xstride"> strides for input data x </param>
        /// <param name="y"> array containing input data y </param>
        /// <param name="ystride"> stride for input data y </param>
        /// <param name="z"> array containing output data z </param>
        /// <param name="zstride"> stride for output data z </param>
        /// <returns> VSL status </returns>
        int ConvExec(IntPtr task, DenseArrayBase<Complex> x, long[] xstride,
            DenseArrayBase<Complex> y, long[] ystride,
            DenseArrayBase<Complex> z, long[] zstride);

        /// <summary>
        /// computes convolution for one-dimensional real-valued case
        /// </summary>
        /// <param name="task"> pointer to the task descriptor </param>
        /// <param name="x"> array containing input data x </param>
        /// <param name="xstride"> strides for input data x </param>
        /// <param name="y"> array containing input data y </param>
        /// <param name="ystride"> stride for input data y </param>
        /// <param name="z"> array containing output data z </param>
        /// <param name="zstride"> stride for output data z </param>
        /// <returns> VSL status </returns>
        int ConvExec(IntPtr task, DenseArrayBase<double> x, long xstride,
            DenseArrayBase<double> y, long ystride, DenseArrayBase<double> z, long zstride);

        /// <summary>
        /// computes convolution for one-dimensional complex-valued case
        /// </summary>
        /// <param name="task"> pointer to the task descriptor </param>
        /// <param name="x"> array containing input data x </param>
        /// <param name="xstride"> strides for input data x </param>
        /// <param name="y"> array containing input data y </param>
        /// <param name="ystride"> stride for input data y </param>
        /// <param name="z"> array containing output data z </param>
        /// <param name="zstride"> stride for output data z </param>
        /// <returns> VSL status </returns>
        int ConvExec(IntPtr task, DenseArrayBase<Complex> x, long xstride,
            DenseArrayBase<Complex> y, long ystride, DenseArrayBase<Complex> z, long zstride);

        /// <summary>
        /// destroys the task object and frees the memory
        /// </summary>
        /// <param name="task"> pointer to the task descriptor </param>
        /// <returns> VSL status </returns>
        int ConvDeleteTask(ref IntPtr task);

        #endregion
        #region --------- Correlation ---------

        /// <summary>
        /// creates a new correlation task descriptor 
        /// for multidimensional real-valued case
        /// </summary>
        /// <param name="task"> pointer to the task descriptor </param>
        /// <param name="mode"> correlation computational mode </param>
        /// <param name="dims"> rank of data (must be in the range from 1 to 7) </param>
        /// <param name="xshape"> shape of the input data in the array x </param>
        /// <param name="yshape"> shape of the input data in the array y </param>
        /// <param name="zshape"> shape of the output data in the array z </param>
        /// <returns> VSL status </returns>
        int CorrNewTaskReal(ref IntPtr task,
            CorrMode mode, int dims, long[] xshape, long[] yshape, long[] zshape);

        /// <summary>
        /// creates a new correlation task descriptor 
        /// for multidimensional complex-valued case
        /// </summary>
        /// <param name="task"> pointer to the task descriptor </param>
        /// <param name="mode"> correlation computational mode </param>
        /// <param name="dims"> rank of data (must be in the range from 1 to 7) </param>
        /// <param name="xshape"> shape of the input data in the array x </param>
        /// <param name="yshape"> shape of the input data in the array y </param>
        /// <param name="zshape"> shape of the output data in the array z </param>
        /// <returns> VSL status </returns>
        int CorrNewTaskComplex(ref IntPtr task,
            CorrMode mode, int dims, long[] xshape, long[] yshape, long[] zshape);

        /// <summary>
        /// creates a new correlation task descriptor 
        /// for one-dimensional real-valued case
        /// </summary>
        /// <param name="task"> pointer to the task descriptor </param>
        /// <param name="mode"> correlation computational mode </param>
        /// <param name="xshape"> shape of the input data in the array x </param>
        /// <param name="yshape"> shape of the input data in the array y </param>
        /// <param name="zshape"> shape of the output data in the array z </param>
        /// <returns> VSL status </returns>
        int CorrNewTaskReal1D(ref IntPtr task,
            CorrMode mode, long xshape, long yshape, long zshape);

        /// <summary>
        /// creates a new correlation task descriptor 
        /// for one-dimensional complex-valued case
        /// </summary>
        /// <param name="task"> pointer to the task descriptor </param>
        /// <param name="mode"> correlation computational mode </param>
        /// <param name="xshape"> shape of the input data in the array x </param>
        /// <param name="yshape"> shape of the input data in the array y </param>
        /// <param name="zshape"> shape of the output data in the array z </param>
        /// <returns> VSL status </returns>
        int CorrNewTaskComplex1D(ref IntPtr task,
            CorrMode mode, long xshape, long yshape, long zshape);

        /// <summary>
        /// changes the value of the parameter mode 
        /// in the convolution task descriptor
        /// </summary>
        /// <param name="task"> pointer to the task descriptor </param>
        /// <param name="mode"> new value of the parameter mode </param>
        /// <returns> VSL status </returns>
        int CorrSetMode(IntPtr task, CorrMode mode);

        /// <summary>
        /// changes the value of the parameter internal_precision 
        /// in the convolution task descriptor
        /// </summary>
        /// <param name="task"> pointer to the task descriptor </param>
        /// <param name="precision"> new value of the parameter internal-precision </param>
        /// <returns> VSL status </returns>
        int CorrSetInternalPrecision(IntPtr task, CorrPrecision precision);

        /// <summary>
        /// changes the value of the parameter start 
        /// in the convolution task descriptor
        /// </summary>
        /// <param name="task"> pointer to the task descriptor </param>
        /// <param name="start"> new value of the parameter start </param>
        /// <returns> VSL status </returns>
        int CorrSetStart(IntPtr task, long[] start);

        /// <summary>
        /// changes the value of the parameter decimation 
        /// in the convolution task descriptor
        /// </summary>
        /// <param name="task"> pointer to the task descriptor </param>
        /// <param name="decimation"> new value of the parameter decimation </param>
        /// <returns> VSL status </returns>
        int CorrSetDecimation(IntPtr task, long[] decimation);

        /// <summary>
        /// computes correlation for multidimensional real-valued case
        /// </summary>
        /// <param name="task"> pointer to the task descriptor </param>
        /// <param name="x"> array containing input data x </param>
        /// <param name="xstride"> strides for input data x </param>
        /// <param name="y"> array containing input data y </param>
        /// <param name="ystride"> stride for input data y </param>
        /// <param name="z"> array containing output data z </param>
        /// <param name="zstride"> stride for output data z </param>
        /// <returns> VSL status </returns>
        int CorrExec(IntPtr task, DenseArrayBase<double> x, long[] xstride,
            DenseArrayBase<double> y, long[] ystride, DenseArrayBase<double> z, long[] zstride);

        /// <summary>
        /// computes correlation for multidimensional complex-valued case
        /// </summary>
        /// <param name="task"> pointer to the task descriptor </param>
        /// <param name="x"> array containing input data x </param>
        /// <param name="xstride"> strides for input data x </param>
        /// <param name="y"> array containing input data y </param>
        /// <param name="ystride"> stride for input data y </param>
        /// <param name="z"> array containing output data z </param>
        /// <param name="zstride"> stride for output data z </param>
        /// <returns> VSL status </returns>
        int CorrExec(IntPtr task, DenseArrayBase<Complex> x, long[] xstride,
            DenseArrayBase<Complex> y, long[] ystride, DenseArrayBase<Complex> z, long[] zstride);

        /// <summary>
        /// computes correlation for one-dimensional real-valued case
        /// </summary>
        /// <param name="task"> pointer to the task descriptor </param>
        /// <param name="x"> array containing input data x </param>
        /// <param name="xstride"> strides for input data x </param>
        /// <param name="y"> array containing input data y </param>
        /// <param name="ystride"> stride for input data y </param>
        /// <param name="z"> array containing output data z </param>
        /// <param name="zstride"> stride for output data z </param>
        /// <returns> VSL status </returns>
        int CorrExec(IntPtr task, DenseArrayBase<double> x, long xstride,
            DenseArrayBase<double> y, long ystride, DenseArrayBase<double> z, long zstride);

        /// <summary>
        /// computes correlation for one-dimensional complex-valued case
        /// </summary>
        /// <param name="task"> pointer to the task descriptor </param>
        /// <param name="x"> array containing input data x </param>
        /// <param name="xstride"> strides for input data x </param>
        /// <param name="y"> array containing input data y </param>
        /// <param name="ystride"> stride for input data y </param>
        /// <param name="z"> array containing output data z </param>
        /// <param name="zstride"> stride for output data z </param>
        /// <returns> VSL status </returns>
        int CorrExec(IntPtr task, DenseArrayBase<Complex> x, long xstride,
            DenseArrayBase<double> y, long ystride, DenseArrayBase<double> z, long zstride);

        /// <summary>
        /// destroys the task object and frees the memory
        /// </summary>
        /// <param name="task"> pointer to the task descriptor </param>
        /// <returns> VSL status </returns>
        int CorrDeleteTask(ref IntPtr task);

        #endregion

    }


    /// <summary>
    /// convolution computation mode options
    /// </summary>
    public enum ConvMode : int
    {
        /// <summary>
        /// automatic decision = 0
        /// </summary>
        Auto = 0,
        /// <summary>
        /// direct computation = 1
        /// </summary>
        Direct = 1,
        /// <summary>
        /// computation via FFT = 2
        /// </summary>
        FFT = 2
    }

    /// <summary>
    /// correlation computation mode options
    /// </summary>
    public enum CorrMode : int
    {
        /// <summary>
        /// automatic decision
        /// </summary>
        Auto  = 0,
        /// <summary>
        /// direction computation = 1
        /// </summary>
        Direct = 1,
        /// <summary>
        /// conputation via FFT = 2
        /// </summary>
        FFT = 2
    }

    /// <summary>
    /// convolution computation precision options
    /// </summary>
    public enum ConvPrecision : int
    {
        /// <summary>
        /// single precision = 1
        /// </summary>
        Single = 1,
        /// <summary>
        /// double precision = 2
        /// </summary>
        Double = 2
    }

    /// <summary>
    /// correlation computation precision options
    /// </summary>
    public enum CorrPrecision : int
    {
        /// <summary>
        /// single precision = 1
        /// </summary>
        Single = 1,
        /// <summary>
        /// double precision = 2
        /// </summary>
        Double = 2
    }

    /// <summary>
    /// VSL error options
    /// </summary>
    internal enum VSLError : int
    {
        NotImplemented = -2000,
        AllocationFailure = -2001,
        BadDescriptor = -2200,
        ServiceFailure = -2210,
        EditFailure = -2211,
        EditProhibited = -2212,
        CommitFailure = -2220,
        CopyFailure = -2230,
        DeleteFailure = -2240,
        BadArgument = -2300,
        Dims = -2301,
        Start = -2302,
        Decimation = -2303,
        Xshape = -2311,
        Yshape = -2312,
        Zshape = -2313,
        Xstride = -2321,
        Ystride = -2322,
        Zstride = -2323,
        X = -2331,
        Y = -2332,
        Z = -2333,
        Job = -2100,
        Kind = -2110,
        Mode = -2120,
        Type = -2130,
        Precision = -2140,
        ExternalPrecision = -2141,
        InternalPrecision = -2142,
        Method = -2400,
        Other = -2800
    }


    /// <summary>
    /// VSL BRNG options
    /// </summary>
    /*public class VSLBRNG
    {
        static int MaxRegBRNGs = 512;
        static int BRNG_Shift = 20;
        static int BRNG_Inc = 1;
        //
        static int BRNG_MCG31 = BRNG_Inc;
        static int BRNG_R250 = BRNG_MCG31 + BRNG_Inc;
        static int BRNG_MRG32K3A = BRNG_R250 + BRNG_Inc;
        static int BRNG_MCG59 = BRNG_MRG32K3A + BRNG_Inc;
        static int BRNG_WH = BRNG_MCG59 + BRNG_Inc;
        static int BRNG_SOBOL = BRNG_WH + BRNG_Inc;
        static int BRNG_NIEDERR = BRNG_SOBOL + BRNG_Inc;
        static int BRNG_MT19937 = BRNG_NIEDERR + BRNG_Inc;
        static int BRNG_MT2203 = BRNG_MT19937 + BRNG_Inc;
        static int BRNG_IABSTRACT = BRNG_MT2203 + BRNG_Inc;
        static int BRNG_DABSTRACT = BRNG_IABSTRACT + BRNG_Inc;
        static int BRNG_SABSTRACT = BRNG_DABSTRACT + BRNG_Inc;
        static int BRNG_SFMT19937 = BRNG_SABSTRACT + BRNG_Inc;
        static int BRNG_NONDETERM = BRNG_SFMT19937 + BRNG_Inc;
        static int BRNG_ARS5 = BRNG_NONDETERM + BRNG_Inc;
        static int BRNG_PHILOX4X32X10 = BRNG_ARS5 + BRNG_Inc;

        static int BRNG_RDRAND = 0x0;
        static int BRNG_NONDETERM_NRETRIES = 10;
    }*/

    /// <summary>
    /// VSL BRNG options
    /// </summary>
    public enum VSLBRNG : int
    {
        /// <summary>
        /// MCG31, default option
        /// </summary>
        MCG31 = 1,
        /// <summary>
        /// R250
        /// </summary>
        R250 = 2,
        /// <summary>
        /// MRG32K3A
        /// </summary>
        MRG32K3A = 3,
        /// <summary>
        /// MCG59 
        /// </summary>
        MCG59 = 4,
        /// <summary>
        /// WH
        /// </summary>
        WH = 5,
        /// <summary>
        /// SOBOL
        /// </summary>
        SOBOL = 6,
        /// <summary>
        /// NIEDERR
        /// </summary>
        NIEDERR = 7,
        /// <summary>
        /// MT19937 
        /// </summary>
        MT19937 = 8,
        /// <summary>
        /// MT2203
        /// </summary>
        MT2203 = 9,
        /// <summary>
        /// IABSTRACT
        /// </summary>
        IABSTRACT = 10,
        /// <summary>
        /// DABSTRACT
        /// </summary>
        DABSTRACT = 11,
        /// <summary>
        /// SABSTRACT
        /// </summary>
        SABSTRACT = 12,
        /// <summary>
        /// SFMT19937 
        /// </summary>
        SFMT19937 = 13,
        /// <summary>
        /// NONDETERM 
        /// </summary>
        NONDETERM = 14,
        /// <summary>
        /// ARS5
        /// </summary>
        ARS5 = 15,
        /// <summary>
        /// PHILOX4X32X10 
        /// </summary>
        PHILOX4X32X10 = 16
    }

    /// <summary>
    /// VSL RNG method options
    /// </summary>
    public enum VSLRNGMethod : int
    {
        /// <summary>
        /// STD: standard method. Currently there is only one method for 
        /// this  distribution generator
        /// </summary>
        Uniform_Std = 0,
        /// <summary>
        /// STD: standard method. Currently there is only one method for 
        /// this  distribution generator
        /// </summary>
        UniformBits_Std = 0,
        /// <summary>
        /// STD: standard method. Currently there is only one method for 
        /// this  distribution generator
        /// </summary>
        UniformBits32_Std = 0,
        /// <summary>
        /// STD: standard method. Currently there is only one method for 
        /// this  distribution generator
        /// </summary>
        UniformBits64_Std = 0,
        /// <summary>
        /// BOXMULLER: generates normally distributed random number x thru 
        /// the pair of uniformly distributed numbers u1 and u2 according 
        /// to the formula:
        /// x=sqrt(-ln(u1))*sin(2*Pi*u2)
        /// </summary>
        Gaussian_Boxmuller = 0,
        /// <summary>
        /// BOXMULLER2: generates pair of normally distributed random numbers 
        /// x1 and x2 thru the pair of uniformly dustributed numbers u1 and u2
        /// according to the formula
        /// x1=sqrt(-ln(u1))*sin(2*Pi*u2)
        /// x2=sqrt(-ln(u1))*cos(2*Pi*u2)
        /// </summary>
        Gaussian_BoxMuller2 = 1,
        /// <summary>
        /// ICDF: inverse cumulative distribution function method
        /// </summary>
        Gaussian_ICDF = 2,
        /// <summary>
        /// BOXMULLER: generates normally distributed random number x thru 
        /// the pair of uniformly distributed numbers u1 and u2 according 
        /// to the formula:
        /// x=sqrt(-ln(u1))*sin(2*Pi*u2)
        /// </summary>
        GaussianMV_Boxmuller = 0,
        /// <summary>
        /// BOXMULLER2: generates pair of normally distributed random numbers 
        /// x1 and x2 thru the pair of uniformly dustributed numbers u1 and u2
        /// according to the formula
        /// x1=sqrt(-ln(u1))*sin(2*Pi*u2)
        /// x2=sqrt(-ln(u1))*cos(2*Pi*u2)
        /// </summary>
        GaussianMV_Boxmuller2 = 1,
        /// <summary>
        /// ICDF: inverse cumulative distribution function method
        /// </summary>
        GaussianMV_ICDF = 2,
        /// <summary>
        /// ICDF: inverse cumulative distribution function method
        /// </summary>
        Exponential_ICDF = 0,
        /// <summary>
        /// ICDF: inverse cumulative distribution function method
        /// </summary>
        Laplace_ICDF = 0,
        /// <summary>
        /// ICDF: inverse cumulative distribution function method
        /// </summary>
        Weibull_ICDF = 0,
        /// <summary>
        /// ICDF: inverse cumulative distribution function method
        /// </summary>
        Cauchy_ICDF = 0,
        /// <summary>
        /// ICDF: inverse cumulative distribution function method
        /// </summary>
        Rayleigh_ICDF = 0,
        /// <summary>
        /// BOXMULLER2: Box-Muller 2 algorithm based method
        /// </summary>
        Lognormal_Boxmuller2 = 0,
        /// <summary>
        /// ICDF: inverse cumulative distribution function method
        /// </summary>
        Lognormal_ICDF = 1,
        /// <summary>
        /// ICDF: inverse cumulative distribution function method
        /// </summary>
        Gumbel_ICDF = 0,
        /// <summary>
        /// alpha>1: - algorithm of Marsaglia is used, nonlinear 
        /// transformation of gaussian numbers based on acceptance/rejection 
        /// method with squeezes;
        /// alpha>=0.6, alpha<1: - rejection from the Weibull distribution is used;
        /// alpha<0.6: - transformation of exponential power distribution (EPD)
        /// is used, EPD random numbers are generated by means of 
        /// acceptance/rejection technique;
        /// alpha=1: - gamma distribution reduces to exponential distribution
        /// </summary>
        Gamma_Gnorm = 0,
        /// <summary>
        /// CJA: stands for first letters of Cheng, Johnk, and Atkinson.
        /// </summary>
        Beta_CJA = 0,
        /// <summary>
        /// v = 1, v = 3: - chi-square distributed random number is generated
        /// as a sum of squares of v independent normal random numbers;
        /// v is even and v = 16: - chi-square distributed random number is 
        /// generated using the following formula:
        /// x = -2*ln(u[0]*...*u[v/2-1]),
        /// where u[i] - random numbers uniformly
        /// distributed over the interval (0,1);
        /// v > 16, v is odd and v > 3: - chi-square distribution reduces 
        /// to gamma distribution;
        /// </summary>
        ChiSquare_Chi2Gamma = 0,
        /// <summary>
        /// ICDF: inverse cumulative distribution function method
        /// </summary>
        Bernoulli_ICDF = 0,
        /// <summary>
        /// ICDF: inverse cumulative distribution function method
        /// </summary>
        Geometric_ICDF = 0,
        /// <summary>
        /// BTPE: for ntrial*min(p,1-p)>30 acceptance/rejection method with
        /// decomposition onto 4 regions:
        /// * 2 parallelograms;
        /// * triangle;
        /// * left exponential tail;
        /// * right exponential tail.
        ///othewise table lookup method is used
        /// </summary>
        Binomial_BTPE = 0,
        /// <summary>
        /// MULTPOISSON: Poisson Approximation of Multinomial Distribution method
        /// </summary>
        Multinomial_MultPoisson = 0,
        /// <summary>
        /// H2PE: if mode of distribution is large, acceptance/rejection 
        /// method is used with decomposition onto 3 regions:
        /// * rectangular;
        /// * left exponential tail;
        /// * right exponential tail.
        /// othewise table lookup method is used
        /// </summary>
        Hypergeometric_H2PE = 0,
        /// <summary>
        /// PTPE: if lambda>=27, acceptance/rejection method is used 
        /// with decomposition onto 4 regions:
        /// * 2 parallelograms;
        /// * triangle;
        /// * left exponential tail;
        /// * right exponential tail.
        /// othewise table lookup method is used.
        /// </summary>
        Poisson_PTPE = 0,
        /// <summary>
        /// POISNORM: for lambda>=1 method is based on Poisson inverse CDF
        /// approximation by Gaussian inverse CDF; for lambda<1
        /// table lookup method is used.
        /// </summary>
        Poisson_PoisNorm = 1,
        /// <summary>
        /// POISNORM: for lambda>=1 method is based on Poisson inverse CDF
        /// approximation by Gaussian inverse CDF; for lambda<1
        /// ICDF method is used.
        /// </summary>
        PoissonMV_PoisNorm = 0,
        /// <summary>
        /// NBAR: if (a-1)*(1-p)/p>=100, acceptance/rejection method 
        /// is used with decomposition onto 5 regions:
        /// * rectangular;
        /// * 2 trapezoid;
        /// * left exponential tail;
        /// * right exponential tail.
        /// othewise table lookup method is used.
        /// </summary>
        NegBinomial_Nbar = 0
    }

}
