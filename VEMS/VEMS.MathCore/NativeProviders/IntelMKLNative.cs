using System.Security;
using System.Runtime.InteropServices;
using Complex = System.Numerics.Complex;

namespace VEMS.MathCore
{

    #region ----- MKL Enums -----

    /// <summary>
    /// Intel MKL interface options 
    /// </summary>
    public enum MKL_Interface : int
    {
        /// <summary>
        /// LP64 option
        /// </summary>
        LP64 = 0,

        /// <summary>
        /// ILP64 option (default)
        /// </summary>
        ILP64 = 1,

        /// <summary>
        /// GNU option
        /// </summary>
        GNU = 2
    }

    /// <summary>
    /// Intel MKL instruction options
    /// </summary>
    public enum MKL_Instructions
    {
        /// <summary>
        /// SSE4.2
        /// </summary>
        SSE4_2 = 0,

        /// <summary>
        /// AVX
        /// </summary>
        AVX = 1,

        /// <summary>
        /// AVX2
        /// </summary>
        AVX2 = 2,

        /// <summary>
        /// AVX512_MIC
        /// </summary>
        AVX512_MIC = 3,

        /// <summary>
        /// AVX512
        /// </summary>
        AVX512 = 4,

        /// <summary>
        /// AVX512_MIC_E1
        /// </summary>
        AVX512_MIC_E1 = 5,

        /// <summary>
        /// AVX512_MIC_E2
        /// </summary>
        AVX512_E1 = 6,

        /// <summary>
        /// AVX512_E2  
        /// </summary>
        AVX512_E2 = 7,

        /// <summary>
        /// AVX512_E3
        /// </summary>
        AVX512_E3 = 8,

        /// <summary>
        /// AVX512_E4
        /// </summary>
        AVX512_E4 = 9,

        /// <summary>
        /// AVX2_E1
        /// </summary>
        AVX2_E1 = 10,

        /// <summary>
        /// AVX512_E5
        /// </summary>
        AVX512_E5 = 11
    }

    /// <summary>
    /// Intel MKL version
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct MKLVersion
    {
        /// <summary>
        /// major version number
        /// </summary>
        public long Major;

        /// <summary>
        /// minor version number
        /// </summary>
        public long Minor;

        /// <summary>
        /// update number
        /// </summary>
        public long Update;

        /// <summary>
        /// patch number
        /// </summary>
        public int Patch;

        /// <summary>
        /// product status
        /// </summary>
        public IntPtr ProductStatus;

        /// <summary>
        /// build
        /// </summary>
        public IntPtr Build;

        /// <summary>
        /// processor
        /// </summary>
        public IntPtr Processor;

        /// <summary>
        /// platform
        /// </summary>
        public IntPtr Platform;
    }

    internal enum MKL_Layout
    {
        RowMajor = 101,
        ColMajor = 102
    }

    internal enum MKL_Transpose
    {
        NoTrans = 111,
        Trans = 112,
        ConjTrans = 113,
        Conj = 114
    }

    internal enum MKL_Uplo
    {
        Upper = 121,
        Lower = 122
    }

    internal enum MKL_Diag
    {
        NonUnit = 131,
        Unit = 132
    }

    internal enum MKL_Side
    {
        Left = 141,
        Right = 142
    }

    internal enum MKL_CompactPack
    {
        SSE = 181,
        AVX = 182,
        AVX512 = 183
    }

    /// <summary>
    /// Jit status
    /// </summary>
    public enum MKL_JitStatus
    {
        /// <summary>
        /// Success
        /// </summary>
        Success = 0,

        /// <summary>
        /// No Jit
        /// </summary>
        NoJit = 1,

        /// <summary>
        /// Jit error
        /// </summary>
        Error = 2

    }


    #endregion

    /// <summary>
    /// Intel MKL native providers
    /// </summary>
    [SuppressUnmanagedCodeSecurity]
    public sealed unsafe class IntelMKLNative
    {
        private IntelMKLNative() { }

        private const string DllName = "mkl_rt.2";

        #region Support Functions 

        #region ---- version ----

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // returns the Intel oneAPI Math Kernel Library (oneMKL) version
        internal static extern void mkl_get_version(ref MKLVersion version);


        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // returns the Intel oneAPI Math Kernel Library (oneMKL) version
        internal static extern void mkl_get_version_string(IntPtr buffer, int len);

        #endregion
        #region ---- SDL control ----

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // sets the interface layer for Intel MKL at run time
        internal static extern int mkl_set_interface_layer(ref int requiredInterface);


        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // sets the threading layer for Intel MKL at run time
        internal static extern int mkl_set_threading_layer(ref int requiredThreading);

        #endregion
        #region ---- threading ----

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // gets the number of OpenMP threads targeted for parallelism
        internal static extern int mkl_get_max_threads();


        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // specifies the number of OpenMP threads to use
        internal static extern void mkl_set_num_threads(ref int nt);


        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // gets the number of OpenMP threads targeted for parallelism 
        // for a particular function domain
        internal static extern int mkl_domain_get_max_threads(int domain);


        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // specifies the number of OpenMP threads for a particular function domain
        internal static extern int mkl_domain_set_num_threads(ref int nt, int domain);


        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // specifies the number of OpenMP threads for all Intel MKL functions
        // on the current execution thread
        internal static extern int mkl_set_num_threads_local(ref int nt);


        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // determines whether Intel MKL is enabled to dynamically change 
        // the number of OpenMP threads
        internal static extern int mkl_get_dynamic();


        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // enables Intel MKL to dynamically change the number of OpenMP threads
        internal static extern void mkl_set_dynamic(ref int flag);


        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // specifies the number of partitions along the leading dimension of
        // the output matrix for parallel ?gemm functions
        internal static extern void mkl_set_num_stripes(ref int ns);


        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // gets the number of partitions along the leading dimension of the
        // output matrix for parallel ?gemm functions
        internal static extern int mkl_get_num_stripes();

        #endregion 
        #region ---- memory ----

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // frees unused memory allocated by the Intel MKL Memory Allocator
        internal static extern void mkl_free_buffers();


        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // frees unused memory allocated by the Intel MKL Memory Allocator
        // in the current thread
        internal static extern void mkl_thread_free_buffers();


        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // turns off the Intel Memory Allocator for Intel MKL functions to
        // directly us the system malloc/free functions
        internal static extern int mkl_disable_fast_mm();


        //[DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        //// allocates an aligned memory buffer
        //internal static extern void* MKL_malloc(long alloc_size, long alignment);


        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // allocates an aligned memory buffer
        internal static extern IntPtr MKL_malloc(IntPtr alloc_size, long alignment);


        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // allocates and initializes an aligned memory buffer
        internal static extern IntPtr MKL_calloc(long num, long element_size, long alignment);


        //[DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        //// changes the size of memory byffer allocated by mkl_malloc/mkl_calloc
        //internal static extern void* MKL_realloc(void* ptr, long size);


        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // changes the size of memory byffer allocated by mkl_malloc/mkl_calloc
        internal static extern IntPtr MKL_realloc(IntPtr ptr, long size);

        //[DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        //// frees the aligned memory buffer allocated by mkl_malloc/mkl_alloc
        //internal static extern void MKL_free(void* ptr);


        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // frees the aligned memory buffer allocated by mkl_malloc/mkl_calloc
        internal static extern void MKL_free(IntPtr ptr);

        #endregion
        #region ---- timing ----

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // returns elapsed time in seconds and can be used to 
        // estimate real time between two calls to this function
        internal static extern double dsecnd();


        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // returns elapsed CPU clocks
        internal static extern void mkl_get_cpu_clocks(ref long clocks);


        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // returns the current CPU frequency value in GHz
        internal static extern double mkl_get_cpu_frequency();


        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // returns the maximum CPU frequency value in GHz
        internal static extern double mkl_get_max_cpu_frequency();


        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // returns the frequency value in GHz based on constant-rate Time Stamp Counter
        internal static extern double mkl_get_clocks_frequency();

        #endregion
        #region ---- misc ----

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // enables dispatching for new Intel® architectures or
        // restricts the set of Intel® instruction sets available
        internal static extern int mkl_enable_instructions(int isa);

        #endregion

        #endregion

        #region BLAS Level-1

        #region ---- dasum, dzasum ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes the sum of magnitudes of the matrix elements
        // dasum := |x[0]| + |x[1]| + ... + |x[n-1]|
        internal static extern double cblas_dasum_64(long n,
            [In] IntPtr x, long incx);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes the sum of magnitudes of the matrix elements
        // dzasum := |Re(x[0])| + |Im(x[0])| + |Re(x[1])| + |Im(x[1])| + ... + |Re(x[n-1])| + |Im(x[n-1])|
        internal static extern double cblas_dzasum_64(long n,
            [In] IntPtr x, long incx);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        // computes the sum of magnitudes of the matrix elements
        internal static extern double cblas_dasum_64(long n,
            [In] double* x, long incx);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
             ExactSpelling = true, SetLastError = false)]
        // computes the sum of magnitudes of the matrix elements
        internal static extern double cblas_dzasum_64(long n,
            [In] void* x, long incx);

        #endregion
        #region ---- daxpy, zaxpy ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // compute a matrix-scalar product and adds the result to a matrix
        // y := a * x + y
        internal static extern void cblas_daxpy_64(long n,
            double a, [In] IntPtr x, long incx,
            [In, Out] IntPtr y, long incy);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // compute a matrix-scalar product and adds the result to a matrix
        // y := a * x + y
        internal static extern void cblas_zaxpy_64(long n,
            ref Complex a, [In] IntPtr x, long incx,
            [In, Out] IntPtr y, long incy);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        // compute a matrix-scalar product and adds the result to a matrix
        internal static extern void cblas_daxpy_64(long n,
            double a, [In] double* x, long incx,
            [In, Out] double* y, long incy);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
             ExactSpelling = true, SetLastError = false)]
        // compute a matrix-scalar product and adds the result to a matrix
        internal static extern void cblas_zaxpy_64(long n,
            void* a, [In] void* x, long incx,
            [In, Out] void* y, long incy);

        #endregion
        #region ---- dcopy, zcopy ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // copies vector x to vector y
        // y := x
        internal static extern void cblas_dcopy_64(long n,
            [In] IntPtr x, long incx,
            [In, Out] IntPtr y, long incy);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // copies vector x to vector y
        // y := x
        internal static extern void cblas_zcopy_64(long n,
            [In] IntPtr x, long incx,
            [In, Out] IntPtr y, long incy);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        // copies vector x to vector y
        internal static extern void cblas_dcopy_64(long n,
            [In] double* x, long incx,
            [Out] double* y, long incy);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
             ExactSpelling = true, SetLastError = false)]
        // copies vector x to vector y
        internal static extern void cblas_zcopy_64(long n,
            [In] void* x, long incx,
            [Out] void* y, long incy);

        #endregion
        #region  ---- ddot, zdotu, zdotc ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes a vector-vector dot product
        internal static extern double cblas_ddot_64(long n,
            [In] IntPtr x, long incx,
            [In] IntPtr y, long incy);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes a vector-vector dot product
        internal static extern void cblas_zdotu_sub_64(long n,
            [In] IntPtr x, long incx,
            [In] IntPtr y, long incy,
            [In, Out] ref Complex dotu);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // compute a dot product of a conjugated vector with another vector
        internal static extern void cblas_zdotc_sub_64(long n,
            [In] IntPtr x, long incx,
            [In] IntPtr y, long incy,
            [In, Out] ref Complex dotc);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
             ExactSpelling = true, SetLastError = false)]
        // computes a vector-vector dot product
        internal static extern double cblas_ddot_64(long n,
            [In] double* x, long incx,
            [In] double* y, long incy);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
             ExactSpelling = true, SetLastError = false)]
        // computes a vector-vector dot product
        internal static extern void cblas_zdotu_sub_64(long n,
            [In] void* x, long incx,
            [In] void* y, long incy,
            [Out] void* dotu);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
             ExactSpelling = true, SetLastError = false)]
        // compute a dot product of a conjugated vector with another vector
        internal static extern void cblas_zdotc_sub_64(long n,
            [In] void* x, long incx,
            [In] void* y, long incy,
            [Out] void* dotc);

        #endregion
        #region ---- dnrm2, dznrm2 ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes the Euclidean norm of a vector
        // res = ||x||
        internal static extern double cblas_dnrm2_64(long n,
            [In] IntPtr x, long incx);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes the Euclidean norm of a vector
        // res = ||x||
        internal static extern double cblas_dznrm2_64(long n,
            [In] IntPtr x, long incx);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        // computes the Euclidean norm of a vector
        internal static extern double cblas_dnrm2_64(long n,
            [In] double* x, long incx);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
             ExactSpelling = true, SetLastError = false)]
        // computes the Euclidean norm of a vector
        internal static extern double cblas_dznrm2_64(long n,
            [In] void* x, long incx);

        #endregion
        #region ---- drot, zdrot ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // performs rotation of points in the plane
        // xi = c * xi + s * yi
        // yi = c * yi - s * xi
        internal static extern void cblas_drot_64(long n,
            [In, Out] IntPtr x, long incx,
            [In, Out] IntPtr y, long incy,
            double c, double s);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // performs rotation of points in the plane
        // xi = c * xi + s * yi
        // yi = c * yi - s * xi
        internal static extern void cblas_zdrot_64(long n,
            [In, Out] IntPtr x, long incx,
            [In, Out] IntPtr y, long incy,
            double c, double s);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
             ExactSpelling = true, SetLastError = false)]
        // performs rotation of points in the plane
        internal static extern void cblas_drot_64(long n,
            [In, Out] double* x, long incx,
            [In, Out] double* y, long incy,
            double c, double s);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
             ExactSpelling = true, SetLastError = false)]
        // performs rotation of points in the plane
        internal static extern void cblas_zdrot_64(long n,
            [In, Out] void* x, long incx,
            [In, Out] void* y, long incy,
            double c, double s);

        #endregion
        #region ------------- drotg, zrotg (?) ----------------

        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        // given the Cartesian coordinates (a, b) of a point
        // returns the parameters c, s, r, and z
        internal static extern void cblas_drotg([In, Out] double* a,
            [In, Out] double* b,
            [Out] double* c, [Out] double* s);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        // given the Cartesian coordinates (a, b) of a point
        // returns the parameters c, s, r, and z
        internal static extern void cblas_zrotg([In, Out] void* a,
            [In, Out] void* b,
            [Out] void* c, [Out] void* s);

        #endregion
        #region ------------- drotm (?) ---------------

        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        // performs rotation of points in the plane
        internal static extern void cblas_drotm(long n,
            [In, Out] double* x, int incx,
            [In, Out] double* y, int incy,
            double* param);

        #endregion
        #region ------------- drotmg (?) ---------------

        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        // given Cartesian coordinates (x1, y1) of an input vector
        // computes the components of a modified Givens transformation matrix
        internal static extern void cblas_drotmg([In, Out] double* d1,
            [In, Out] double* d2,
            [In, Out] double* x1, [In] double* y1, [Out] double* param);

        #endregion
        #region ---- dscal, zdscal, zscal  ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes the product of a matrix by a scalar
        // x := a * x
        internal static extern void cblas_dscal_64(long n, double a,
            [In, Out] IntPtr x, long incx);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes the product of a matrix by a scalar
        // x := a * x
        internal static extern void cblas_zdscal_64(long n, double a,
            [In, Out] IntPtr x, long incx);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes the product of a matrix by a scalar
        // x := a * x
        internal static extern void cblas_zscal_64(long n, ref Complex a,
            [In, Out] IntPtr x, long incx);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        // computes the product of a matrix by a scalar
        internal static extern void cblas_dscal_64(long n, double a,
            [In] double* x, long incx);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
             ExactSpelling = true, SetLastError = false)]
        // computes the product of a matrix by a scalar
        internal static extern void cblas_zdscal_64(long n, double a,
            [In] void* x, long incx);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
             ExactSpelling = true, SetLastError = false)]
        // computes the product of a matrix by a scalar
        internal static extern void cblas_zscal_64(long n, void* pa,
            [In] void* x, long incx);

        #endregion
        #region ---- dswap, zswap ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // given two vectors x and y, returns vector y and x swapped
        internal static extern void cblas_dswap_64(long n,
            [In, Out] IntPtr x, long incx,
            [In, Out] IntPtr y, long incy);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // given two vectors x and y, returns vector y and x swapped
        internal static extern void cblas_zswap_64(long n,
            [In, Out] IntPtr x, long incx,
            [In, Out] IntPtr y, long incy);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        // given two vectors x and y, returns vector y and x swapped
        internal static extern void cblas_dswap_64(long n,
            [In, Out] double* x, long incx,
            [In, Out] double* y, long incy);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
             ExactSpelling = true, SetLastError = false)]
        // given two vectors x and y, returns vector y and x swapped
        internal static extern void cblas_zswap_64(long n,
            [In, Out] void* x, long incx,
            [In, Out] void* y, long incy);

        #endregion
        #region ---- idamax, izamax ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // finds the index of the element with maximum absolute value
        internal static extern long cblas_idamax_64(long n,
            [In] IntPtr x, long incx);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // finds the index of the element with maximum absolute value
        internal static extern long cblas_izamax_64(long n,
            [In] IntPtr x, long incx);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        // finds the index of the element with maximum absolute value
        internal static extern long cblas_idamax_64(long n,
            [In] double* x, long incx);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
             ExactSpelling = true, SetLastError = false)]
        // finds the index of the element with maximum absolute value
        internal static extern long cblas_izamax_64(long n,
            [In] void* x, long incx);

        #endregion
        #region ---- idamin, izamin ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // finds the index of the element with the snmallest absolute value
        internal static extern long cblas_idamin_64(long n,
            [In] IntPtr x, long incx);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // finds the index of the element with the smallest absolute value
        internal static extern long cblas_izamin_64(long n,
            [In] IntPtr x, long incx);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        // finds the index of the element with the snmallest absolute value
        internal static extern long cblas_idamin_64(long n,
            [In] double* x, long incx);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        // finds the index of the element with the smallest absolute value
        internal static extern long cblas_izamin_64(long n,
            [In] void* x, long incx);

        #endregion

        #endregion
        #region BLAS Level-2

        #region ---- dgemv, zgemv ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes a matrix-vector product using a general matrix
        // y := alpha * op(A) * x + beta * y
        internal static extern void cblas_dgemv_64(BLAS_Layout layout, 
            BLAS_Transpose trans, long m, long n, 
            double alpha, [In] IntPtr a, long lda, [In] IntPtr x, long incx,
            double beta, [In, Out] IntPtr y, long incy);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes a matrix-vector product using a general matrix
        // y := alpha * op(A) * x + beta * y
        internal static extern void cblas_zgemv_64(BLAS_Layout layout, 
            BLAS_Transpose trans, long m, long n, 
            ref Complex alpha, [In] IntPtr a, long lda, [In] IntPtr x, long incx,
            ref Complex beta, [In, Out] IntPtr y, long incy);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        // computes a matrix-vector product using a general matrix
        // y := alpha*A*x + beta*y
        internal static extern void cblas_dgemv_64(BLAS_Layout layout,
            BLAS_Transpose trans, long m, long n,
            double alpha, [In] double* a, long lda,
            [In] double* x, long incx,
            double beta, [In, Out] double* y, long incy);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
             ExactSpelling = true, SetLastError = false)]
        // computes a matrix-vector product using a general matrix
        internal static extern void cblas_zgemv_64(BLAS_Layout layout,
            BLAS_Transpose trans, long m, long n,
            void* alpha, [In] void* a, long lda,
            [In] void* x, long incx,
            void* beta, [In, Out] void* y, long incy);

        #endregion
        #region ---- dgbmv, zgbmv ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes a matrix-vector product with a general band matrix
        // y := alpha * A * x + beta * y
        internal static extern void cblas_dgbmv_64(BLAS_Layout layout, 
            BLAS_Transpose trans, long m, long n, long kl, long ku, 
            double alpha, [In] IntPtr a, long lda, [In] IntPtr x, long incx,
            double beta, [In, Out] IntPtr y, long incy);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes a matrix-vector product with a general band matrix
        // y := alpha * A * x + beta * y
        internal static extern void cblas_zgbmv_64(BLAS_Layout layout,
            BLAS_Transpose trans, long m, long n, long kl, long ku, 
            ref Complex alpha, [In] IntPtr a, long lda, [In] IntPtr x, long incx,
            ref Complex beta, [In, Out] IntPtr y, long incy);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes a matrix-vector product with a general band matrix
        // y := alpha * A * x + beta * y
        internal static extern void cblas_dgbmv_64(BLAS_Layout layout,
            BLAS_Transpose trans, long m, long n, long kl, long ku,
            double alpha, [In] double* a, long lda, [In] double* x, long incx,
            double beta, [In, Out] double* y, long incy);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes a matrix-vector product with a general band matrix
        // y := alpha * A * x + beta * y
        internal static extern void cblas_zgbmv_64(BLAS_Layout layout,
            BLAS_Transpose trans, long m, long n, long kl, long ku,
            void* alpha, [In] void* a, long lda, [In] void* x, long incx,
            void* beta, [In, Out] void* y, long incy);

        #endregion
        #region ---- dtrmv, ztrmv ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes a matrix-vector product using a triangular matrix
        // x := op(A) * x
        internal static extern void cblas_dtrmv_64(BLAS_Layout layout,
            BLAS_Uplo uplo, BLAS_Transpose trans, BLAS_Diag diag,
            long n, [In] IntPtr a, long lda, 
            [In, Out] IntPtr x, long incx);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes a matrix-vector product using a triangular matrix
        // x := op(A) * x
        internal static extern void cblas_ztrmv_64(BLAS_Layout layout,
            BLAS_Uplo uplo, BLAS_Transpose trans, BLAS_Diag diag,
            long n, [In] IntPtr a, long lda,
            [In, Out] IntPtr x, long incx);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes a matrix-vector product using a triangular matrix
        // x := op(A) * x
        internal static extern void cblas_dtrmv_64(BLAS_Layout layout,
            BLAS_Uplo uplo, BLAS_Transpose trans, BLAS_Diag diag,
            long n, [In] double* a, long lda,
            [In, Out] double* x, long incx);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes a matrix-vector product using a triangular matrix
        // x := op(A) * x
        internal static extern void cblas_ztrmv_64(BLAS_Layout layout,
            BLAS_Uplo uplo, BLAS_Transpose trans, BLAS_Diag diag,
            long n, [In] void* a, long lda,
            [In, Out] void* x, long incx);

        #endregion
        #region ---- dtbmv, ztbmv ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes a matrix-vector product using a triangular band matrix
        // x := op(A) * x
        internal static extern void cblas_dtbmv_64(BLAS_Layout layout,
            BLAS_Uplo uplo, BLAS_Transpose trans, BLAS_Diag diag,
            long n, long k, [In] IntPtr a, long lda,
            [In, Out] IntPtr x, long incx);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes a matrix-vector product using a triangular band matrix
        // x := op(A) * x
        internal static extern void cblas_ztbmv_64(BLAS_Layout layout,
            BLAS_Uplo uplo, BLAS_Transpose trans, BLAS_Diag diag,
            long n, long k, [In] IntPtr a, long lda,
            [In, Out] IntPtr x, long incx);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes a matrix-vector product using a triangular band matrix
        // x := op(A) * x
        internal static extern void cblas_dtbmv_64(BLAS_Layout layout,
            BLAS_Uplo uplo, BLAS_Transpose trans, BLAS_Diag diag,
            long n, long k, [In] double* a, long lda,
            [In, Out] double* x, long incx);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes a matrix-vector product using a triangular band matrix
        // x := op(A) * x
        internal static extern void cblas_ztbmv_64(BLAS_Layout layout,
            BLAS_Uplo uplo, BLAS_Transpose trans, BLAS_Diag diag,
            long n, long k, [In] void* a, long lda,
            [In, Out] void* x, long incx);

        #endregion
        #region ---- dtpmv, ztpmv ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes a matrix-vector product using a triangular packed matrix
        // x := op(A) * x
        internal static extern void cblas_dtpmv_64(BLAS_Layout layout,
            BLAS_Uplo uplo, BLAS_Transpose trans, BLAS_Diag diag,
            long n, long k, [In] IntPtr a, [In, Out] IntPtr x, long incx);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes a matrix-vector product using a triangular packed matrix
        // x := op(A) * x
        internal static extern void cblas_ztpmv_64(BLAS_Layout layout,
            BLAS_Uplo uplo, BLAS_Transpose trans, BLAS_Diag diag,
            long n, long k, [In] IntPtr a, [In, Out] IntPtr x, long incx);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes a matrix-vector product using a triangular packed matrix
        // x := op(A) * x
        internal static extern void cblas_dtpmv_64(BLAS_Layout layout,
            BLAS_Uplo uplo, BLAS_Transpose trans, BLAS_Diag diag,
            long n, long k, [In] double* a, [In, Out] double* x, long incx);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes a matrix-vector product using a triangular packed matrix
        // x := op(A) * x
        internal static extern void cblas_ztpmv_64(BLAS_Layout layout,
            BLAS_Uplo uplo, BLAS_Transpose trans, BLAS_Diag diag,
            long n, long k, [In] void* a, [In, Out] void* x, long incx);

        #endregion
        #region ---- dtrsv, ztrsv ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // solves a system of linear equations whose coefficients
        // are in a triangular matrix op(A) * x = b
        internal static extern void cblas_dtrsv_64(BLAS_Layout layout,
            BLAS_Uplo uplo, BLAS_Transpose trans, BLAS_Diag diag,
            long n, [In] IntPtr a, long lda,
            [In, Out] IntPtr x, long incx);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // solves a system of linear equations whose coefficients
        // are in a triangular matrix op(A) * x = b
        internal static extern void cblas_ztrsv_64(BLAS_Layout layout,
            BLAS_Uplo uplo, BLAS_Transpose trans, BLAS_Diag diag,
            long n, [In] IntPtr a, long lda,
            [In, Out] IntPtr x, long incx);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // solves a system of linear equations whose coefficients
        // are in a triangular matrix op(A) * x = b
        internal static extern void cblas_dtrsv_64(BLAS_Layout layout,
            BLAS_Uplo uplo, BLAS_Transpose trans, BLAS_Diag diag,
            long n, [In] double* a, long lda,
            [In, Out] double* x, long incx);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // solves a system of linear equations whose coefficients
        // are in a triangular matrix op(A) * x = b
        internal static extern void cblas_ztrsv_64(BLAS_Layout layout,
            BLAS_Uplo uplo, BLAS_Transpose trans, BLAS_Diag diag,
            long n, [In] void* a, long lda,
            [In, Out] void* x, long incx);

        #endregion
        #region ---- dtbsv, ztbsv ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // solves a system of linear equations whose coefficients
        // are in a triangular band matrix op(A) * x = b
        internal static extern void cblas_dtbsv_64(BLAS_Layout layout,
            BLAS_Uplo uplo, BLAS_Transpose trans, BLAS_Diag diag,
            long n, long k, [In] IntPtr a, long lda,
            [In, Out] IntPtr x, long incx);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // solves a system of linear equations whose coefficients
        // are in a triangular band matrix op(A) * x = b
        internal static extern void cblas_ztbsv_64(BLAS_Layout layout,
            BLAS_Uplo uplo, BLAS_Transpose trans, BLAS_Diag diag,
            long n, long k, [In] IntPtr a, long lda,
            [In, Out] IntPtr x, long incx);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // solves a system of linear equations whose coefficients
        // are in a triangular band matrix op(A) * x = b
        internal static extern void cblas_dtbsv_64(BLAS_Layout layout,
            BLAS_Uplo uplo, BLAS_Transpose trans, BLAS_Diag diag,
            long n, long k, [In] double* a, long lda,
            [In, Out] double* x, long incx);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // solves a system of linear equations whose coefficients
        // are in a triangular band matrix op(A) * x = b
        internal static extern void cblas_ztbsv_64(BLAS_Layout layout,
            BLAS_Uplo uplo, BLAS_Transpose trans, BLAS_Diag diag,
            long n, long k, [In] void* a, long lda,
            [In, Out] void* x, long incx);

        #endregion
        #region ---- dtpsv, ztpsv ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // solves a system of linear equations whose coefficients
        // are in a triangular packed matrix op(A) * x = b
        internal static extern void cblas_dtpsv_64(BLAS_Layout layout,
            BLAS_Uplo uplo, BLAS_Transpose trans, BLAS_Diag diag,
            long n, [In] IntPtr a, [In, Out] IntPtr x, long incx);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // solves a system of linear equations whose coefficients
        // are in a triangular packed matrix op(A) * x = b
        internal static extern void cblas_ztpsv_64(BLAS_Layout layout,
            BLAS_Uplo uplo, BLAS_Transpose trans, BLAS_Diag diag,
            long n, [In] IntPtr a, [In, Out] IntPtr x, long incx);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // solves a system of linear equations whose coefficients
        // are in a triangular packed matrix op(A) * x = b
        internal static extern void cblas_dtpsv_64(BLAS_Layout layout,
            BLAS_Uplo uplo, BLAS_Transpose trans, BLAS_Diag diag,
            long n, [In] double* a, [In, Out] double* x, long incx);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // solves a system of linear equations whose coefficients
        // are in a triangular packed matrix op(A) * x = b
        internal static extern void cblas_ztpsv_64(BLAS_Layout layout,
            BLAS_Uplo uplo, BLAS_Transpose trans, BLAS_Diag diag,
            long n, [In] void* a, [In, Out] void* x, long incx);

        #endregion
        #region ---- dsymv ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes a matrix-vector product for a symmetric matrix
        // y := alpha * A * x + beta * y
        internal static extern void cblas_dsymv_64(BLAS_Layout layout,
            BLAS_Uplo uplo, long n, 
            double alpha, [In] IntPtr a, long lda, [In] IntPtr x, long incx,
            double beta, [In, Out] IntPtr y, long incy);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes a matrix-vector product for a symmetric matrix
        // y := alpha * A * x + beta * y
        internal static extern void cblas_dsymv_64(BLAS_Layout layout,
            BLAS_Uplo uplo, long n,
            double alpha, [In] double* a, long lda, [In] double* x, long incx,
            double beta, [In, Out] double* y, long incy);

        #endregion
        #region ---- dsbmv ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes a matrix-vector product for a symmetric band matrix
        // y := alpha * A * x + beta * y
        internal static extern void cblas_dsbmv_64(BLAS_Layout layout,
            BLAS_Uplo uplo, long n, long k, 
            double alpha, [In] IntPtr a, long lda, [In] IntPtr x, long incx,
            double beta, [In, Out] IntPtr y, long incy);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes a matrix-vector product for a symmetric band matrix
        // y := alpha * A * x + beta * y
        internal static extern void cblas_dsbmv_64(BLAS_Layout layout,
            BLAS_Uplo uplo, long n, long k,
            double alpha, [In] double* a, long lda, [In] double* x, long incx,
            double beta, [In, Out] double* y, long incy);

        #endregion
        #region ---- dspmv ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes a matrix-vector product for a symmetric packed matrix
        // y := alpha * A * x + beta * y
        internal static extern void cblas_dspmv_64(BLAS_Layout layout,
            BLAS_Uplo uplo, long n,
            double alpha, [In] IntPtr a, long lda, [In] IntPtr x, long incx,
            double beta, [In, Out] IntPtr y, long incy);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes a matrix-vector product for a symmetric packed matrix
        // y := alpha * A * x + beta * y
        internal static extern void cblas_dspmv_64(BLAS_Layout layout,
            BLAS_Uplo uplo, long n,
            double alpha, [In] double* a, long lda, [In] double* x, long incx,
            double beta, [In, Out] double* y, long incy);

        #endregion
        #region ---- dger ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // performs a rank-1 update of a general matrix
        // A := alpha * x * y' + A
        internal static extern void cblas_dger_64(BLAS_Layout layout,
            long m, long n, double alpha, [In] IntPtr x, long incx,
            [In] IntPtr y, long incy, [In, Out] IntPtr a, long lda);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // performs a rank-1 update of a general matrix
        // A := alpha * x * y' + A
        internal static extern void cblas_dger_64(BLAS_Layout layout,
            long m, long n, double alpha, [In] double* x, long incx,
            [In] double* y, long incy, [In, Out] double* a, long lda);

        #endregion
        #region ---- dsyr ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // performs a rank-1 update of a symmetric matrix
        // A := alpha * x * x' + A
        internal static extern void cblas_dsyr_64(BLAS_Layout layout,
            BLAS_Uplo uplo, long n, double alpha, [In] IntPtr x, long incx,
            [In, Out] IntPtr a, long lda);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // performs a rank-1 update of a symmetric matrix
        // A := alpha * x * x' + A
        internal static extern void cblas_dsyr_64(BLAS_Layout layout,
            BLAS_Uplo uplo, long n, double alpha, [In] double* x, long incx,
            [In, Out] double* a, long lda);

        #endregion
        #region ---- dspr ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // performs a rank-1 update of a symmetric packed matrix
        // A := alpha * x * x' + A
        internal static extern void cblas_dspr_64(BLAS_Layout layout,
            BLAS_Uplo uplo, long n, double alpha, [In] IntPtr x, long incx,
            [In, Out] IntPtr a);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // performs a rank-1 update of a symmetric packed matrix
        // A := alpha * x * x' + A
        internal static extern void cblas_dspr_64(BLAS_Layout layout,
            BLAS_Uplo uplo, long n, double alpha, [In] double* x, long incx,
            [In, Out] double* a);

        #endregion
        #region ---- dsyr2 ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // performs a rank-2 update of a symmetric matrix
        // A := alpha * x * y' + alpha * y * x' + A
        internal static extern void cblas_dsyr2_64(BLAS_Layout layout,
            BLAS_Uplo uplo, long n, double alpha, [In] IntPtr x, long incx,
            [In] IntPtr y, long incy, [In, Out] IntPtr a, long lda);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // performs a rank-2 update of a symmetric matrix
        // A := alpha * x * y' + alpha * y * x' + A
        internal static extern void cblas_dsyr2_64(BLAS_Layout layout,
            BLAS_Uplo uplo, long n, double alpha, [In] double* x, long incx,
            [In] double* y, long incy, [In, Out] double* a, long lda);

        #endregion
        #region ---- dspr2 ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // performs a rank-2 update of a symmetric packed matrix
        // A := alpha * x * y' + alpha * y * x' + A
        internal static extern void cblas_dspr2_64(BLAS_Layout layout,
            BLAS_Uplo uplo, long n, double alpha, [In] IntPtr x, long incx,
            [In] IntPtr y, long incy, [In, Out] IntPtr a);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // performs a rank-2 update of a symmetric packed matrix
        // A := alpha * x * y' + alpha * y * x' + A
        internal static extern void cblas_dspr2_64(BLAS_Layout layout,
            BLAS_Uplo uplo, long n, double alpha, [In] double* x, long incx,
            [In] double* y, long incy, [In, Out] double* a);

        #endregion
        #region ---- zhemv ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes a matrix-vector product using a Hermitian matrix
        // y := alpha * A * x + beta * y
        internal static extern void cblas_zhemv_64(BLAS_Layout layout,
            BLAS_Uplo uplo, long n, ref Complex alpha,
            [In] IntPtr a, long lda, [In] IntPtr x, long incx,
            ref Complex beta, [In, Out] IntPtr y, long incy);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes a matrix-vector product using a Hermitian matrix
        // y := alpha * A * x + beta * y
        internal static extern void cblas_zhemv_64(BLAS_Layout layout,
            BLAS_Uplo uplo, long n, void* alpha,
            [In] void* a, long lda, [In] void* x, long incx,
            void* beta, [In, Out] void* y, long incy);

        #endregion
        #region ---- zhbmv ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes a matrix-vector product using a Hermitian band matrix
        // y := alpha * A * x + beta * y
        internal static extern void cblas_zhbmv_64(BLAS_Layout layout,
            BLAS_Uplo uplo, long n, long k, ref Complex alpha,
            [In] IntPtr a, long lda, [In] IntPtr x, long incx,
            ref Complex beta, [In, Out] IntPtr y, long incy);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes a matrix-vector product using a Hermitian band matrix
        // y := alpha * A * x + beta * y
        internal static extern void cblas_zhbmv_64(BLAS_Layout layout,
            BLAS_Uplo uplo, long n, long k, void* alpha,
            [In] void* a, long lda, [In] void* x, long incx,
            void* beta, [In, Out] void* y, long incy);

        #endregion
        #region ---- zhpmv ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes a matrix-vector product using a Hermitian packed matrix
        // y := alpha * A * x + beta * y
        internal static extern void cblas_zhpmv_64(BLAS_Layout layout,
            BLAS_Uplo uplo, long n, ref Complex alpha,
            [In] IntPtr a, [In] IntPtr x, long incx,
            ref Complex beta, [In, Out] IntPtr y, long incy);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes a matrix-vector product using a Hermitian packed matrix
        // y := alpha * A * x + beta * y
        internal static extern void cblas_zhpmv_64(BLAS_Layout layout,
            BLAS_Uplo uplo, long n, void* alpha,
            [In] void* a, [In] void* x, long incx,
            void* beta, [In, Out] void* y, long incy);

        #endregion
        #region ---- zgeru ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // performs a rank-1 update (unconjugated) of a general matrix
        // A := alpha * x * y' + A
        internal static extern void cblas_zgeru_64(BLAS_Layout layout,
            long m, long n, ref Complex alpha, [In] IntPtr x, long incx,
            [In] IntPtr y, long incy, [In, Out] IntPtr a, long lda);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // performs a rank-1 update (unconjugated) of a general matrix
        // A := alpha * x * y' + A
        internal static extern void cblas_zgeru_64(BLAS_Layout layout,
            long m, long n, void* alpha, [In] void* x, long incx,
            [In] void* y, long incy, [In, Out] void* a, long lda);

        #endregion
        #region ---- zgerc ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // performs a rank-1 update (conjugated) of a general matrix
        // A := alpha * x * y' + A,
        internal static extern void cblas_zgerc_64(BLAS_Layout layout,
            long m, long n, ref Complex alpha, [In] IntPtr x, long incx,
            [In] IntPtr y, long incy, [In, Out] IntPtr a, long lda);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // performs a rank-1 update (conjugated) of a general matrix
        // A := alpha * x * y' + A,
        internal static extern void cblas_zgerc_64(BLAS_Layout layout,
            long m, long n, void* alpha, [In] void* x, long incx,
            [In] void* y, long incy, [In, Out] void* a, long lda);

        #endregion
        #region ---- zher ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // performs a rank-1 update of a Hermitian matrix
        // A := alpha * x * cong(x') + A
        internal static extern void cblas_zher_64(BLAS_Layout layout,
            BLAS_Uplo uplo, long n, double alpha, [In] IntPtr x, long incx,
            [In, Out] IntPtr a, long lda);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // performs a rank-1 update of a Hermitian matrix
        // A := alpha * x * cong(x') + A
        internal static extern void cblas_zher_64(BLAS_Layout layout,
            BLAS_Uplo uplo, long n, double alpha, [In] void* x, long incx,
            [In, Out] void* a, long lda);

        #endregion
        #region ---- zhpr ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // performs a rank-1 update of a Hermitian packed matrix
        // A := alpha * x * cong(x') + A
        internal static extern void cblas_zhpr_64(BLAS_Layout layout,
            BLAS_Uplo uplo, long n, double alpha, [In] IntPtr x, long incx,
            [In, Out] IntPtr a);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // performs a rank-1 update of a Hermitian packed matrix
        // A := alpha * x * cong(x') + A
        internal static extern void cblas_zhpr_64(BLAS_Layout layout,
            BLAS_Uplo uplo, long n, double alpha, [In] void* x, long incx,
            [In, Out] void* a);

        #endregion
        #region ---- zher2 ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // performs a rank-2 update of a Hermitian matrix
        // A := alpha * x* conj(y') + conj(alpha) * y * conj(x') + A
        internal static extern void cblas_zher2_64(BLAS_Layout layout,
            BLAS_Uplo uplo, long n, ref Complex alpha, 
            [In] IntPtr x, long incx, [In] IntPtr y, long incy,
            [In, Out] IntPtr a, long lda);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // performs a rank-2 update of a Hermitian matrix
        // A := alpha * x* conj(y') + conj(alpha) * y * conj(x') + A
        internal static extern void cblas_zher2_64(BLAS_Layout layout,
            BLAS_Uplo uplo, long n, void* alpha,
            [In] void* x, long incx, [In] void* y, long incy,
            [In, Out] void* a, long lda);

        #endregion
        #region ---- zhpr2 ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // performs a rank-2 update of a Hermitian packed matrix
        // A := alpha * x* conj(y') + conj(alpha) * y * conj(x') + A
        internal static extern void cblas_zhpr2_64(BLAS_Layout layout,
            BLAS_Uplo uplo, long n, ref Complex alpha,
            [In] IntPtr x, long incx, [In] IntPtr y, long incy,
            [In, Out] IntPtr a);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // performs a rank-2 update of a Hermitian packed matrix
        // A := alpha * x* conj(y') + conj(alpha) * y * conj(x') + A
        internal static extern void cblas_zhpr2_64(BLAS_Layout layout,
            BLAS_Uplo uplo, long n, void* alpha,
            [In] void* x, long incx, [In] void* y, long incy,
            [In, Out] void* a);

        #endregion

        #endregion
        #region BLAS Level-3

        #region ---- dgemm, zgemm ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes a matrix-matrix product with general matrices
        // C := alpha * op(A) * op(B) + beta * C
        internal static extern void cblas_dgemm_64(BLAS_Layout layout, 
            BLAS_Transpose transa, BLAS_Transpose transb, long m, long n, long k,
            double alpha, [In] IntPtr a, long lda, [In] IntPtr b, long ldb,
            double beta, [In, Out] IntPtr c, long ldc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes a matrix-matrix product with general matrices
        // C := alpha * op(A) * op(B) + beta * C
        internal static extern void cblas_zgemm_64(BLAS_Layout layout, 
            BLAS_Transpose transa, BLAS_Transpose transb, long m, long n, long k,
            ref Complex alpha, [In] IntPtr a, long lda, [In] IntPtr b, long ldb,
            ref Complex beta, [In, Out] IntPtr c, long ldc);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        // computes a matrix-matrix product with general matrices
        internal static extern void cblas_dgemm_64(BLAS_Layout layout,
            BLAS_Transpose transa, BLAS_Transpose transb,
            long m, long n, long k,
            double alpha, [In] double* a, long lda, [In] double* b, long ldb,
            double beta, [In, Out] double* c, long ldc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        // computes a matrix-matrix product with general matrices
        internal static extern void cblas_zgemm_64(BLAS_Layout layout,
            BLAS_Transpose transa, BLAS_Transpose transb,
            long m, long n, long k,
            void* alpha, [In] void* a, long lda, [In] void* b, long ldb,
            void* beta, [In, Out] void* c, long ldc);

        #endregion
        #region ---- dsymm, zsymm ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes a matrix-matrix product where one input
        // matrix is symmetric C := alpha * A * B + beta * C, or
        // C := alpha * B * A + beta * C
        internal static extern void cblas_dsymm_64(BLAS_Layout layout,
            BLAS_Side side, BLAS_Uplo uplo, long m, long n,
            double alpha, [In] IntPtr a, long lda, [In] IntPtr b, long ldb,
            double beta, [In, Out] IntPtr c, long ldc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes a matrix-matrix product where one input
        // matrix is symmetric C := alpha * A * B + beta * C, or
        // C := alpha * B * A + beta * C
        internal static extern void cblas_zsymm_64(BLAS_Layout layout,
            BLAS_Side side, BLAS_Uplo uplo, long m, long n,
            ref Complex alpha, [In] IntPtr a, long lda, [In] IntPtr b, long ldb,
            ref Complex beta, [In, Out] IntPtr c, long ldc);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes a matrix-matrix product where one input
        // matrix is symmetric C := alpha * A * B + beta * C, or
        // C := alpha * B * A + beta * C
        internal static extern void cblas_dsymm_64(BLAS_Layout layout,
            BLAS_Side side, BLAS_Uplo uplo, long m, long n,
            double alpha, [In] double* a, long lda, [In] double* b, long ldb,
            double beta, [In, Out] double* c, long ldc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes a matrix-matrix product where one input
        // matrix is symmetric C := alpha * A * B + beta * C, or
        // C := alpha * B * A + beta * C
        internal static extern void cblas_zsymm_64(BLAS_Layout layout,
            BLAS_Side side, BLAS_Uplo uplo, long m, long n,
            void* alpha, [In] void* a, long lda, [In] void* b, long ldb,
            void* beta, [In, Out] void* c, long ldc);

        #endregion
        #region ---- dsyrk, zsyrk ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // performs a symmetric rank-k update
        // C := alpha * A * A' + beta * C, or
        // C := alpha * A' * A + beta * C
        internal static extern void cblas_dsyrk_64(BLAS_Layout layout,
            BLAS_Uplo uplo, BLAS_Transpose trans, long n, long k,
            double alpha, [In] IntPtr a, long lda,
            double beta, [In, Out] IntPtr c, long ldc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // performs a symmetric rank-k update
        // C := alpha * A * A' + beta * C, or
        // C := alpha * A' * A + beta * C
        internal static extern void cblas_zsyrk_64(BLAS_Layout layout,
            BLAS_Uplo uplo, BLAS_Transpose trans, long n, long k,
            ref Complex alpha, [In] IntPtr a, long lda,
            ref Complex beta, [In, Out] IntPtr c, long ldc);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // performs a symmetric rank-k update
        // C := alpha * A * A' + beta * C, or
        // C := alpha * A' * A + beta * C
        internal static extern void cblas_dsyrk_64(BLAS_Layout layout,
            BLAS_Uplo uplo, BLAS_Transpose trans, long n, long k,
            double alpha, [In] double* a, long lda,
            double beta, [In, Out] double* c, long ldc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // performs a symmetric rank-k update
        // C := alpha * A * A' + beta * C, or
        // C := alpha * A' * A + beta * C
        internal static extern void cblas_zsyrk_64(BLAS_Layout layout,
            BLAS_Uplo uplo, BLAS_Transpose trans, long n, long k,
            void* alpha, [In] void* a, long lda,
            void* beta, [In, Out] void* c, long ldc);

        #endregion
        #region ---- dsyr2k, zsyr2k ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // performs a symmetric rank-2k update
        // C := alpha * A * B' + alpha * B * A' + beta * C, or
        // C := alpha * A' * B + alpha * B' * A + beta * C
        internal static extern void cblas_dsyr2k_64(BLAS_Layout layout,
            BLAS_Uplo uplo, BLAS_Transpose trans, long n, long k,
            double alpha, [In] IntPtr a, long lda, [In] IntPtr b, long ldb,
            double beta, [In, Out] IntPtr c, long ldc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // performs a symmetric rank-2k update
        // C := alpha * A * B' + alpha * B * A' + beta * C, or
        // C := alpha * A' * B + alpha * B' * A + beta * C
        internal static extern void cblas_zsyr2k_64(BLAS_Layout layout,
            BLAS_Uplo uplo, BLAS_Transpose trans, long n, long k,
            ref Complex alpha, [In] IntPtr a, long lda, [In] IntPtr b, long ldb,
            ref Complex beta, [In, Out] IntPtr c, long ldc);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // performs a symmetric rank-2k update
        // C := alpha * A * B' + alpha * B * A' + beta * C, or
        // C := alpha * A' * B + alpha * B' * A + beta * C
        internal static extern void cblas_dsyr2k_64(BLAS_Layout layout,
            BLAS_Uplo uplo, BLAS_Transpose trans, long n, long k,
            double alpha, [In] void* a, long lda, [In] void* b, long ldb,
            double beta, [In, Out] void* c, long ldc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // performs a symmetric rank-2k update
        // C := alpha * A * B' + alpha * B * A' + beta * C, or
        // C := alpha * A' * B + alpha * B' * A + beta * C
        internal static extern void cblas_zsyr2k_64(BLAS_Layout layout,
            BLAS_Uplo uplo, BLAS_Transpose trans, long n, long k,
            void* alpha, [In] void* a, long lda, [In] void* b, long ldb,
            void* beta, [In, Out] void* c, long ldc);

        #endregion
        #region ---- dtrmm, ztrmm ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes a a matrix-matrix product where one input
        // matrix is triangular B := alpha * op(A) * B, or
        // B := alpha * B * op(A)
        internal static extern void cblas_dtrmm_64(BLAS_Layout layout,
            BLAS_Side side, BLAS_Uplo uplo, BLAS_Transpose trans, BLAS_Diag diag,
            long m, long n, double alpha, [In] IntPtr a, long lda, 
            [In, Out] IntPtr b, long ldb);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes a a matrix-matrix product where one input
        // matrix is triangular B := alpha * op(A) * B, or
        // B := alpha * B * op(A)
        internal static extern void cblas_ztrmm_64(BLAS_Layout layout,
            BLAS_Side side, BLAS_Uplo uplo, BLAS_Transpose trans, BLAS_Diag diag,
            long m, long n, ref Complex alpha, [In] IntPtr a, long lda,
            [In, Out] IntPtr b, long ldb);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes a a matrix-matrix product where one input
        // matrix is triangular B := alpha * op(A) * B, or
        // B := alpha * B * op(A)
        internal static extern void cblas_dtrmm_64(BLAS_Layout layout,
            BLAS_Side side, BLAS_Uplo uplo, BLAS_Transpose trans, BLAS_Diag diag,
            long m, long n, double alpha, [In] double* a, long lda,
            [In, Out] double* b, long ldb);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes a a matrix-matrix product where one input
        // matrix is triangular B := alpha * op(A) * B, or
        // B := alpha * B * op(A)
        internal static extern void cblas_ztrmm_64(BLAS_Layout layout,
            BLAS_Side side, BLAS_Uplo uplo, BLAS_Transpose trans, BLAS_Diag diag,
            long m, long n, void* alpha, [In] void* a, long lda,
            [In, Out] void* b, long ldb);

        #endregion
        #region ---- dtrsm, ztrsm ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // solves a triangular matrix equation
        // op(A) * X = alpha * B, or
        // X * op(A) = alpha * B
        internal static extern void cblas_dtrsm_64(BLAS_Layout layout,
            BLAS_Side side, BLAS_Uplo uplo, BLAS_Transpose trans, BLAS_Diag diag,
            long m, long n, double alpha, [In] IntPtr a, long lda,
            [In, Out] IntPtr b, long ldb);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // solves a triangular matrix equation
        // op(A) * X = alpha * B, or
        // X * op(A) = alpha * B
        internal static extern void cblas_ztrsm_64(BLAS_Layout layout,
            BLAS_Side side, BLAS_Uplo uplo, BLAS_Transpose trans, BLAS_Diag diag,
            long m, long n, ref Complex alpha, [In] IntPtr a, long lda,
            [In, Out] IntPtr b, long ldb);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // solves a triangular matrix equation
        // op(A) * X = alpha * B, or
        // X * op(A) = alpha * B
        internal static extern void cblas_dtrsm_64(BLAS_Layout layout,
            BLAS_Side side, BLAS_Uplo uplo, BLAS_Transpose trans, BLAS_Diag diag,
            long m, long n, double alpha, [In] double* a, long lda,
            [In, Out] double* b, long ldb);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // solves a triangular matrix equation
        // op(A) * X = alpha * B, or
        // X * op(A) = alpha * B
        internal static extern void cblas_ztrsm_64(BLAS_Layout layout,
            BLAS_Side side, BLAS_Uplo uplo, BLAS_Transpose trans, BLAS_Diag diag,
            long m, long n, void* alpha, [In] void* a, long lda,
            [In, Out] void* b, long ldb);

        #endregion
        #region ---- zhemm ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes a matrix-matrix product where one input
        // matrix is Hermitian C := alpha * A * B + beta * C, or
        // C := alpha * B * A + beta * C
        internal static extern void cblas_zhemm_64(BLAS_Layout layout,
            BLAS_Side side, BLAS_Uplo uplo, long m, long n, 
            ref Complex alpha, [In] IntPtr a, long lda, [In] IntPtr b, long ldb,
            ref Complex beta, [In, Out] IntPtr c, long ldc);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes a matrix-matrix product where one input
        // matrix is Hermitian C := alpha * A * B + beta * C, or
        // C := alpha * B * A + beta * C
        internal static extern void cblas_zhemm_64(BLAS_Layout layout,
            BLAS_Side side, BLAS_Uplo uplo, long m, long n,
            void* alpha, [In] void* a, long lda, [In] void* b, long ldb,
            void* beta, [In, Out] void* c, long ldc);

        #endregion
        #region ---- zherk ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // performs a Hermitian rank-k update
        // C := alpha * A * AH + beta * C, or
        // C := alpha * AH * A + beta * C
        internal static extern void cblas_zherk_64(BLAS_Layout layout,
            BLAS_Uplo uplo, BLAS_Transpose trans, long n, long k,
            double alpha, [In] IntPtr a, long lda,
            double beta, [In, Out] IntPtr c, long ldc);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // performs a Hermitian rank-k update
        // C := alpha * A * AH + beta * C, or
        // C := alpha * AH * A + beta * C
        internal static extern void cblas_zherk_64(BLAS_Layout layout,
            BLAS_Uplo uplo, BLAS_Transpose trans, long n, long k,
            double alpha, [In] void* a, long lda,
            double beta, [In, Out] void* c, long ldc);

        #endregion
        #region ---- zher2k ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // performs a Hermitian rank-2k update
        // C := alpha * A * BH + conj(alpha) B * AH + beta * C, or
        // C := alpha* AH * B + conj(alpha) * BH* A + beta* C
        internal static extern void cblas_zher2k_64(BLAS_Layout layout,
            BLAS_Uplo uplo, BLAS_Transpose trans, long n, long k,
            ref Complex alpha, [In] IntPtr a, long lda, [In] IntPtr b, long ldb,
            ref Complex beta, [In, Out] IntPtr c, long ldc);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // performs a Hermitian rank-2k update
        // C := alpha * A * BH + conj(alpha) B * AH + beta * C, or
        // C := alpha* AH * B + conj(alpha) * BH* A + beta* C
        internal static extern void cblas_zher2k_64(BLAS_Layout layout,
            BLAS_Uplo uplo, BLAS_Transpose trans, long n, long k,
            void* alpha, [In] void* a, long lda, [In] void* b, long ldb,
            void* beta, [In, Out] void* c, long ldc);

        #endregion

        #endregion
        #region BLAS-like Extensions

        #region ---- daxpby, zaxpby ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        // scales two matrices, adds them to one another and stores result in the matrix
        internal static extern void cblas_daxpby_64(long n,
            double a, [In] IntPtr x, long incx,
            double b, [In, Out] IntPtr y, long incy);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        // scales two matrices, adds them to one another and stores result in the matrix
        internal static extern void cblas_zaxpby_64(long n,
            ref Complex a, [In] IntPtr x, long incx,
            ref Complex b, [In, Out] IntPtr y, long incy);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        // scales two matrices, adds them to one another and stores result in the matrix
        internal static extern void cblas_daxpby_64(long n,
            double a, [In] double* x, long incx,
            double b, [In, Out] double* y, long incy);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        // scales two matrices, adds them to one another and stores result in the matrix
        internal static extern void cblas_zaxpby_64(long n,
            void* a, [In] void* x, long incx,
            void* b, [In, Out] void* y, long incy);

        #endregion
        #region ---- zgemm3m ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        // computes a scalar-matrix-matrix product
        internal static extern void cblas_zgemm3m_64(BLAS_Layout layout, 
            BLAS_Transpose transa, BLAS_Transpose transb,
            long m, long n, long k,
            ref Complex alpha, [In] IntPtr a, long lda, 
            [In] IntPtr b, long ldb,
            ref Complex beta, [In, Out] IntPtr c, long ldc);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        // computes a scalar-matrix-matrix product
        internal static extern void cblas_zgemm3m_64(BLAS_Layout layout,
            BLAS_Transpose transa, BLAS_Transpose transb,
            long m, long n, long k,
            void* alpha, [In] void* a, long lda, [In] void* b, long ldb,
            void* beta, [In, Out] void* c, long ldc);

        #endregion
        #region ---- dimatcopy, zimatcopy ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // performs scaling and in-place transposition/copying of matrices
        internal static extern void MKL_Dimatcopy(byte layout, byte operation,
            long rows, long cols,
            double alpha, [In, Out] IntPtr ab, long lda, long ldb);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // performs scaling and in-place transposition/copying of matrices
        internal static extern void MKL_Zimatcopy(byte layout, byte trans,
            long rows, long cols,
            ref Complex alpha, [In, Out] IntPtr ab, long lda, long ldb);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // performs scaling and in-place transposition/copying of matrices
        internal static extern void MKL_Dimatcopy(byte layout, byte operation,
            long rows, long cols,
            double alpha, [In, Out] double* ab, long lda, long ldb);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // performs scaling and in-place transposition/copying of matrices
        internal static extern void MKL_Zimatcopy(byte layout, byte trans,
            long rows, long cols,
            void* alpha, [In, Out] void* ab, long lda, long ldb);

        #endregion
        #region ---- domatcopy, zomatcopy ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // performs scaling and out-place transposition/copying of matrices
        internal static extern void MKL_Domatcopy(byte layout, byte trans,
            long rows, long cols,
            double alpha, [In] IntPtr a, long lda, [Out] IntPtr b, long ldb);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // performs scaling and out-place transposition/copying of matrices
        internal static extern void MKL_Zomatcopy(byte layout, byte trans,
            long rows, long cols,
            ref Complex alpha, [In] IntPtr a, long lda, [Out] IntPtr b, long ldb);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        // performs scaling and out-place transposition/copying of matrices
        internal static extern void MKL_Domatcopy(byte layout, byte trans,
            long rows, long cols,
            double alpha, double* a, long lda, double* b, long ldb);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        // performs scaling and out-place transposition/copying of matrices
        internal static extern void MKL_Zomatcopy(byte layout, byte trans,
            long rows, long cols,
            void* alpha, void* a, long lda, void* b, long ldb);

        #endregion
        #region ---- jit create ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // creates a GEMM kernel that computes a scalar-matrix-matrix
        // product and adds the result to a scalar-matrix product
        // C := alpha * op(A) * op(B) + beta * C
        internal static extern MKL_JitStatus mkl_cblas_jit_create_dgemm_64(
            ref IntPtr jitter, MKL_Layout layout,
            MKL_Transpose transa, MKL_Transpose transb,
            long m, long n, long k, double alpha, long lda, long ldb,
            double beta, long ldc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // creates a GEMM kernel that computes a scalar-matrix-matrix
        // product and adds the result to a scalar-matrix product
        // C := alpha * op(A) * op(B) + beta * C
        internal static extern MKL_JitStatus mkl_cblas_jit_create_zgemm_64(
            ref IntPtr jitter, MKL_Layout layout,
            MKL_Transpose transa, MKL_Transpose transb,
            long m, long n, long k, ref Complex alpha, long lda, long ldb,
            ref Complex beta, long ldc);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // creates a GEMM kernel that computes a scalar-matrix-matrix
        // product and adds the result to a scalar-matrix product
        // C := alpha * op(A) * op(B) + beta * C
        internal static extern MKL_JitStatus mkl_cblas_jit_create_dgemm_64(
            //ref IntPtr jitter, MKL_Layout layout,
            void** jitter, MKL_Layout layout,
            MKL_Transpose transa, MKL_Transpose transb,
            long m, long n, long k, double alpha, long lda, long ldb,
            double beta, long ldc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // creates a GEMM kernel that computes a scalar-matrix-matrix
        // product and adds the result to a scalar-matrix product
        // C := alpha * op(A) * op(B) + beta * C
        internal static extern MKL_JitStatus mkl_cblas_jit_create_zgemm_64(
            //ref IntPtr jitter, MKL_Layout layout,
            void** jitter, MKL_Layout layout,
            MKL_Transpose transa, MKL_Transpose transb,
            long m, long n, long k, void* alpha, long lda, long ldb,
            void* beta, long ldc);

        #endregion
        #region ---- jit get (?) ----

        //[DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        //// returns the GEMM kernel associated with a jitter previously created 
        //internal static extern MKL_JitStatus mkl_cblas_jit_create_dgemm_64(
        // function pointer???

        #endregion

        #endregion

        #region Sparse BLAS Level-1

        // BLAS Level-1 routines that can work with sparse vectors
        // ---- dasum, zasum ----
        // ---- dcopy, zcopy ----
        // ---- dnrm2, znrm2 ----
        // ---- dscal, zscal ----
        // ---- idamax, izamax ---- 
        // ---- idamin, izamin ----

        #region ---- daxpyi, zaxpyi ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // adds a scalar multiple of compressed sparse vector to
        // a full-storage vector  y := a * x + y
        internal static extern void cblas_daxpyi_64(long n,
            double a, [In] IntPtr x, [In] IntPtr indx,
            [In, Out] IntPtr y);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // adds a scalar multiple of compressed sparse vector to
        // a full-storage vector  y := a * x + y
        internal static extern void cblas_zaxpyi_64(long n,
            ref Complex a, [In] IntPtr x, [In] IntPtr indx,
            [In, Out] IntPtr y);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // adds a scalar multiple of compressed sparse vector to
        // a full-storage vector  y := a * x + y
        internal static extern void cblas_daxpyi_64(long n,
            double a, [In] double* x, [In] long* indx,
            [In, Out] double* y);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // adds a scalar multiple of compressed sparse vector to
        // a full-storage vector  y := a * x + y
        internal static extern void cblas_zaxpyi_64(long n,
            void* a, [In] void* x, [In] long* indx,
            [In, Out] void* y);

        #endregion
        #region ---- ddoti, zdotui, zdotci ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes the dot product of a compressed sparse
        // vector by a full-storage vector
        // res = x[0]*y[indx[0]] + x[1]*y[indx[1]] +...+ x[nz-1]*y[indx[nz-1]]
        internal static extern double cblas_ddoti_64(long n,
            [In] IntPtr x, [In] IntPtr indx, [In] IntPtr y);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes the dot product of a compressed sparse
        // vector by a full-storage vector
        // res = x[0]*y[indx[0]] + x[1]*y[indx[1]] +...+ x[nz-1]*y[indx[nz-1]]
        internal static extern void cblas_zdotui_sub_64(long n,
            [In] IntPtr x, [In] IntPtr indx, [In] IntPtr y, ref Complex dotu);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes the dot product of a compressed sparse
        // vector by a full-storage vector
        // res = conj(x[0])*y[indx[0]] + ... + conj(x[nz-1])*y[indx[nz-1]]
        internal static extern void cblas_zdotci_sub_64(long n,
            [In] IntPtr x, [In] IntPtr indx, [In] IntPtr y, ref Complex dotc);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes the dot product of a compressed sparse
        // vector by a full-storage vector
        // res = x[0]*y[indx[0]] + x[1]*y[indx[1]] +...+ x[nz-1]*y[indx[nz-1]]
        internal static extern double cblas_ddoti_64(long n,
            [In] double* x, [In] long* indx, [In] double* y);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes the dot product of a compressed sparse
        // vector by a full-storage vector
        // res = x[0]*y[indx[0]] + x[1]*y[indx[1]] +...+ x[nz-1]*y[indx[nz-1]]
        internal static extern void cblas_zdotui_sub_64(long n,
            [In] void* x, [In] long* indx, [In] void* y, void* dotu);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes the dot product of a compressed sparse
        // vector by a full-storage vector
        // res = conj(x[0])*y[indx[0]] + ... + conj(x[nz-1])*y[indx[nz-1]]
        internal static extern void cblas_zdotci_sub_64(long n,
            [In] void* x, [In] long* indx, [In] void* y, void* dotc);

        #endregion
        #region ---- dgthr, zgthr ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // gathers a full-storage sparse vector's elements into
        // compressed form x[i] = y[indx[i]]
        internal static extern void cblas_dgthr_64(long n,
            [In] IntPtr y, [In, Out] IntPtr x, [In] IntPtr indx);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // gathers a full-storage sparse vector's elements into
        // compressed form x[i] = y[indx[i]]
        internal static extern void cblas_zgthr_64(long n,
            [In] IntPtr y, [In, Out] IntPtr x, [In] IntPtr indx);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // gathers a full-storage sparse vector's elements into
        // compressed form x[i] = y[indx[i]]
        internal static extern void cblas_dgthr_64(long n,
            [In] double* y, [In, Out] double* x, [In] long* indx);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // gathers a full-storage sparse vector's elements into
        // compressed form x[i] = y[indx[i]]
        internal static extern void cblas_zgthr_64(long n,
            [In] void* y, [In, Out] void* x, [In] long* indx);

        #endregion
        #region ---- dgthrz, zgthrz ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // gathers a a sparse vector's elements into compressed
        // form, replacing them by zeros
        internal static extern void cblas_dgthrz_64(long n,
            [In, Out] IntPtr y, [In, Out] IntPtr x, [In] IntPtr indx);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // gathers a a sparse vector's elements into compressed
        // form, replacing them by zeros
        internal static extern void cblas_zgthrz_64(long n,
            [In, Out] IntPtr y, [In, Out] IntPtr x, [In] IntPtr indx);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // gathers a a sparse vector's elements into compressed
        // form, replacing them by zeros
        internal static extern void cblas_dgthrz_64(long n,
            [In, Out] double* y, [In, Out] double* x, [In] long* indx);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // gathers a a sparse vector's elements into compressed
        // form, replacing them by zeros
        internal static extern void cblas_zgthrz_64(long n,
            [In, Out] void* y, [In, Out] void* x, [In] long* indx);

        #endregion
        #region ---- droti ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // applies Givens rotation to sparse vectors
        // one of which is in compressed form
        // x[i] = c*x[i] + s*y[indx[i]]
        // y[indx[i]] = c*y[indx[i]] - s*x[i]
        internal static extern void cblas_droti_64(long n,
            [In, Out] IntPtr x, [In] IntPtr indx,
            [In, Out] IntPtr y, double c, double s);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // applies Givens rotation to sparse vectors
        // one of which is in compressed form
        // x[i] = c*x[i] + s*y[indx[i]]
        // y[indx[i]] = c*y[indx[i]] - s*x[i]
        internal static extern void cblas_droti_64(long n,
            [In, Out] double* x, [In] long* indx,
            [In, Out] double* y, double c, double s);

        #endregion
        #region ---- dsctr, zsctr ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // converts compressed sparse vector into full-storage form
        internal static extern double cblas_dsctr_64(long nz,
            [In] IntPtr x, [In] IntPtr indx, [In, Out] IntPtr y);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        // converts compressed sparse vector into full-storage form
        internal static extern double cblas_zsctr_64(long nz,
            [In] IntPtr x, [In] IntPtr indx, [In, Out] IntPtr y);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // converts compressed sparse vector into full-storage form
        internal static extern double cblas_dsctr_64(long nz,
            [In] double* x, [In] long* indx, [In, Out] double* y);
            
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        // converts compressed sparse vector into full-storage form
        internal static extern double cblas_zsctr_64(long nz,
            [In] void* x, [In] long* indx, [In, Out] void* y);

        #endregion

        #endregion
        #region Sparse BLAS Level-2 [DEPRECATED]

        // MKL_DEPRECATED ...

        #endregion
        #region Sparse BLAS Level-3 [DEPRECATED]

        // MKL_DEPRECATED ...

        #endregion
        #region Sparse QR routines

        #region ---- set QR hint ----

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // defines the pivot strategy for further calls of mkl_sparse_? _qr
        internal static extern SPARSE_Status mkl_sparse_set_qr_hint(IntPtr A,
            SPARSE_QRHint hint);

        #endregion
        #region ---- QR ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes the QR decomposition for the matrix of a sparse
        // linear system and calculates the solution A * x = b
        internal static extern SPARSE_Status mkl_sparse_d_qr(
            SPARSE_Operation operation, IntPtr A, SPARSE_MatrixDescr descr, 
            SPARSE_Layout layout, long columns, IntPtr x, long ldx, 
            [In] IntPtr b, long ldb);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes the QR decomposition for the matrix of a sparse
        // linear system and calculates the solution A * x = b
        internal static extern SPARSE_Status mkl_sparse_d_qr(
            SPARSE_Operation operation, IntPtr A, SPARSE_MatrixDescr descr,
            SPARSE_Layout layout, long columns, [In, Out] double* x, long ldx,
            [In] double* b, long ldb);

        #endregion
        #region ---- reorder ----

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // reordering step of SPARSE QR solver
        internal static extern SPARSE_Status mkl_sparse_qr_reorder(IntPtr A, 
            SPARSE_MatrixDescr descr);

        #endregion
        #region ---- factorize ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // factorization step of SPARSE QR solver
        internal static extern SPARSE_Status mkl_sparse_d_qr_factorize(IntPtr A,
            IntPtr alt_values);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // factorization step of SPARSE QR solver
        internal static extern SPARSE_Status mkl_sparse_d_qr_factorize(IntPtr A,
            double* alt_values);

        #endregion
        #region ---- solve ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // solving step of SPARSE QR solver
        internal static extern SPARSE_Status mkl_sparse_d_qr_solve(
            SPARSE_Operation operation, IntPtr A, IntPtr alt_values,
            SPARSE_Layout layout, long columns, IntPtr x, long ldx,
            [In] IntPtr b, long ldb);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // solving step of SPARSE QR solver
        internal static extern SPARSE_Status mkl_sparse_d_qr_solve(
            SPARSE_Operation operation, IntPtr A, double* alt_values,
            SPARSE_Layout layout, long columns, double* x, long ldx,
            [In] double* b, long ldb);

        #endregion
        #region ---- qmult ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // first stage of the solving step of the SPARSE QR solver
        internal static extern SPARSE_Status mkl_sparse_d_qr_qmult(
            SPARSE_Operation operation, IntPtr A, SPARSE_Layout layout, 
            long columns, IntPtr x, long ldx,
            [In] IntPtr b, long ldb);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // first stage of the solving step of the SPARSE QR solver
        internal static extern SPARSE_Status mkl_sparse_d_qr_qmult(
            SPARSE_Operation operation, IntPtr A, SPARSE_Layout layout,
            long columns, double* x, long ldx,
            [In] double* b, long ldb);

        #endregion
        #region ---- rsolve ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // second stage of the solving step of the SPARSE solver
        internal static extern SPARSE_Status mkl_sparse_d_qr_rsolve(
            SPARSE_Operation operation, IntPtr A, SPARSE_Layout layout, 
            long columns, IntPtr x, long ldx,
            [In] IntPtr b, long ldb);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // second stage of the solving step of the SPARSE solver
        internal static extern SPARSE_Status mkl_sparse_d_qr_rsolve(
            SPARSE_Operation operation, IntPtr A, SPARSE_Layout layout,
            long columns, double* x, long ldx,
            [In] double* b, long ldb);

        #endregion

        #endregion
        #region Sparse BLAS inspector-executer

        #region ---- creation [COO] ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // creates a handle for a matrix in COO format
        internal static extern SPARSE_Status mkl_sparse_d_create_coo_64(
            ref IntPtr a, SPARSE_IndexBase indexing, long rows, long cols, long nnz,
            [In] IntPtr rows_indx, [In] IntPtr col_indx, [In] IntPtr values);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // creates a handle for a matrix in COO format
        internal static extern SPARSE_Status mkl_sparse_z_create_coo_64(
            ref IntPtr a, SPARSE_IndexBase indexing, long rows, long cols, long nnz,
            [In] IntPtr rows_indx, [In] IntPtr col_indx, [In] IntPtr values);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // creates a handle for a matrix in COO format
        internal static extern SPARSE_Status mkl_sparse_d_create_coo_64(
            ref IntPtr a, SPARSE_IndexBase indexing, long rows, long cols, long nnz,
            [In] long* rows_indx, [In] long* col_indx, [In] double* values);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // creates a handle for a matrix in COO format
        internal static extern SPARSE_Status mkl_sparse_z_create_coo_64(
            ref IntPtr a, SPARSE_IndexBase indexing, long rows, long cols, long nnz,
            [In] long* rows_indx, [In] long* col_indx, [In] void* values);

        #endregion
        #region ---- creation [CSR] ----

        //// => IntPtr ...
        //[DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        //// creates a handle for a matrix in CSR format
        //internal static extern SPARSE_Status mkl_sparse_d_create_csr_64(
        //    ref IntPtr a, SPARSE_IndexBase indexing, long rows, long cols,
        //    [In] IntPtr rows_start, [In] IntPtr rows_end,
        //    [In] IntPtr col_indx, [In] IntPtr values);

        //[DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        //// creates a handle for a matrix in CSR format
        //internal static extern SPARSE_Status mkl_sparse_z_create_csr_64(
        //    ref IntPtr a, SPARSE_IndexBase indexing, long rows, long cols,
        //    [In] IntPtr rows_start, [In] IntPtr rows_end,
        //    [In] IntPtr col_indx, [In] IntPtr values);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // creates a handle for a matrix in CSR format
        internal static extern SPARSE_Status mkl_sparse_d_create_csr_64(
            ref IntPtr a, SPARSE_IndexBase indexing, long rows, long cols,
            [In] long* rows_start, [In] long* rows_end,
            [In] long* col_indx, [In] double* values);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // creates a handle for a matrix in CSR format
        internal static extern SPARSE_Status mkl_sparse_z_create_csr_64(
            ref IntPtr a, SPARSE_IndexBase indexing, long rows, long cols,
            [In] long* rows_start, [In] long* rows_end,
            [In] long* col_indx, [In] void* values);


        // => Wrapper ...
        // creates a handle for a matrix in CSR format
        internal static SPARSE_Status mkl_sparse_d_create_csr_64(
            ref IntPtr a, SPARSE_IndexBase indexing, long rows, long cols,
            [In] IntPtr rows_ptr, [In] IntPtr col_indx, [In] IntPtr values)
        {
            long* r = (long*)rows_ptr.ToPointer();
            long* c = (long*)col_indx.ToPointer();
            double* v = (double*)values.ToPointer();
            return mkl_sparse_d_create_csr_64(ref a, indexing, rows, cols,
                r, r + 1, c, v);
        }

        // creates a handle for a matrix in CSR format
        internal static SPARSE_Status mkl_sparse_z_create_csr_64(
            ref IntPtr a, SPARSE_IndexBase indexing, long rows, long cols,
            [In] IntPtr rows_ptr, [In] IntPtr col_indx, [In] IntPtr values)
        {
            long* r = (long*)rows_ptr.ToPointer();
            long* c = (long*)col_indx.ToPointer();
            Complex* v = (Complex*)values.ToPointer();
            return mkl_sparse_z_create_csr_64(ref a, indexing, rows, cols,
                r, r + 1, c, v);
        }

        #endregion
        #region ---- creation [CSC] ----

        //// => IntPtr ...
        //[DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        //// creates a handle for a matrix in CSC format
        //internal static extern SPARSE_Status mkl_sparse_d_create_csc_64(
        //    ref IntPtr a, SPARSE_IndexBase indexing, long rows, long cols,
        //    [In] IntPtr cols_start, [In] IntPtr cols_end,
        //    [In] IntPtr row_indx, [In] IntPtr values);

        //[DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        //// creates a handle for a matrix in CSR format
        //internal static extern SPARSE_Status mkl_sparse_z_create_csc_64(
        //    ref IntPtr a, SPARSE_IndexBase indexing, long rows, long cols,
        //    [In] IntPtr cols_start, [In] IntPtr cols_end,
        //    [In] IntPtr row_indx, [In] IntPtr values);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // creates a handle for a matrix in CSC format
        internal static extern SPARSE_Status mkl_sparse_d_create_csc_64(
            ref IntPtr a, SPARSE_IndexBase indexing, long rows, long cols,
            [In] long* cols_start, [In] long* cols_end,
            [In] long* row_indx, [In] double* values);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // creates a handle for a matrix in CSR format
        internal static extern SPARSE_Status mkl_sparse_z_create_csc_64(
            ref IntPtr a, SPARSE_IndexBase indexing, long rows, long cols,
            [In] long* cols_start, [In] long* cols_end,
            [In] long* row_indx, [In] void* values);


        // => Wrapper ...
        // creates a handle for a matrix in CSC format
        internal static SPARSE_Status mkl_sparse_d_create_csc_64(
            ref IntPtr a, SPARSE_IndexBase indexing, long rows, long cols,
            [In] IntPtr cols_ptr, [In] IntPtr row_indx, [In] IntPtr values)
        {
            long* c = (long*)cols_ptr.ToPointer();
            long* r = (long*)row_indx.ToPointer();
            double* v = (double*)values.ToPointer();
            return mkl_sparse_d_create_csc_64(ref a, indexing, rows, cols,
                c, c + 1, r, v);
        }

        // creates a handle for a matrix in CSC format
        internal static SPARSE_Status mkl_sparse_z_create_csc_64(
            ref IntPtr a, SPARSE_IndexBase indexing, long rows, long cols,
            [In] IntPtr cols_ptr, [In] IntPtr row_indx, [In] IntPtr values)
        {
            long* c = (long*)cols_ptr.ToPointer();
            long* r = (long*)row_indx.ToPointer();
            Complex* v = (Complex*)values.ToPointer();
            return mkl_sparse_z_create_csc_64(ref a, indexing, rows, cols,
                c, c + 1, r, v);
        }

        #endregion
        #region ---- creation [BSR] ----

        // => IntPtr ...


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // creates a handle for a matrix in BSR format
        internal static extern SPARSE_Status mkl_sparse_d_create_bsr_64(
            ref IntPtr a, SPARSE_IndexBase indexing, SPARSE_Layout block_layout,
            long rows, long cols, long block_size,
            [In] long* row_start, [In] long* row_end,
            [In] long* col_indx, [In] double* values);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // creates a handle for a matrix in BSR format
        internal static extern SPARSE_Status mkl_sparse_z_create_bsr_64(
            ref IntPtr a, SPARSE_IndexBase indexing, SPARSE_Layout block_layout,
            long rows, long cols, long block_size,
            [In] long* row_start, [In] long* row_end,
            [In] long* col_indx, [In] Complex* values);


        // => Warpper ...
        // creates a handle for a matrix in BSR format
        internal static SPARSE_Status mkl_sparse_d_create_bsr_64(
            ref IntPtr a, SPARSE_IndexBase indexing, SPARSE_Layout block_layout,
            long rows, long cols, long block_size,
            [In] IntPtr row_ptr, [In] IntPtr col_indx, [In] IntPtr values)
        {
            long* r = (long*)row_ptr.ToPointer();
            long* c = (long*)col_indx.ToPointer();
            double* v = (double*)values.ToPointer();
            return mkl_sparse_d_create_bsr_64(ref a, indexing, block_layout,
                rows, cols, block_size, r, r + 1, c, v);
        }

        // creates a handle for a matrix in BSR format
        internal static SPARSE_Status mkl_sparse_z_create_bsr_64(
            ref IntPtr a, SPARSE_IndexBase indexing, SPARSE_Layout block_layout,
            long rows, long cols, long block_size,
            [In] IntPtr row_ptr, [In] IntPtr col_indx, [In] IntPtr values)
        {
            long* r = (long*)row_ptr.ToPointer();
            long* c = (long*)col_indx.ToPointer();
            Complex* v = (Complex*)values.ToPointer();
            return mkl_sparse_z_create_bsr_64(ref a, indexing, block_layout,
                rows, cols, block_size, r, r + 1, c, v);
        }

        #endregion
        #region ---- copy ----

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // creates a copy of a matrix handle
        internal static extern SPARSE_Status mkl_sparse_copy_64(IntPtr source,
            SPARSE_MatrixDescr descr, ref IntPtr dest);

        #endregion
        #region ---- destroy ----

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // frees memory allocated for matrix handle
        internal static extern SPARSE_Status mkl_sparse_destroy_64(IntPtr A);

        #endregion
        #region ---- convert ----

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // converters internal matrix representation from current format to CSR format
        internal static extern SPARSE_Status mkl_sparse_convert_csr_64(IntPtr source,
            SPARSE_Operation operation, ref IntPtr dest);


        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // converters internal matrix representation from current format to BSR format
        internal static extern SPARSE_Status mkl_sparse_convert_bsr_64(IntPtr source,
            long block_size, SPARSE_Layout block_layout,
            SPARSE_Operation operation, ref IntPtr dest);

        #endregion
        #region ---- export [CSR] ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // exports CSR matrix from internal representation
        internal static extern SPARSE_Status mkl_sparse_d_export_csr_64(
            IntPtr source, ref SPARSE_IndexBase indexing,
            ref long rows, ref long cols,
            ref IntPtr rows_start, ref IntPtr rows_end,
            ref IntPtr col_indx, ref IntPtr values);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // exports CSR matrix from internal representation
        internal static extern SPARSE_Status mkl_sparse_z_export_csr_64(
            IntPtr source, ref SPARSE_IndexBase indexing,
            ref long rows, ref long cols,
            ref IntPtr rows_start, ref IntPtr rows_end,
            ref IntPtr col_indx, ref IntPtr values);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // exports CSR matrix from internal representation
        internal static extern SPARSE_Status mkl_sparse_d_export_csr_64(
            IntPtr source, ref SPARSE_IndexBase indexing,
            long* rows, long* cols,
            long** rows_start, long** rows_end,
            long** col_indx, double** values);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // exports CSR matrix from internal representation
        internal static extern SPARSE_Status mkl_sparse_z_export_csr_64(
            IntPtr source, ref SPARSE_IndexBase indexing,
            long* rows, long* cols,
            long** rows_start, long** rows_end,
            long** col_indx, void** values);

        #endregion
        #region ---- export [CSC] ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // exports CSC matrix from internal representation
        internal static extern SPARSE_Status mkl_sparse_d_export_csc_64(
            IntPtr source, ref SPARSE_IndexBase indexing,
            ref long rows, ref long cols,
            ref IntPtr cols_start, ref IntPtr cols_end,
            ref IntPtr row_indx, ref IntPtr values);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // exports CSC matrix from internal representation
        internal static extern SPARSE_Status mkl_sparse_z_export_csc_64(
            IntPtr source, ref SPARSE_IndexBase indexing,
            ref long rows, ref long cols,
            ref IntPtr cols_start, ref IntPtr cols_end,
            ref IntPtr row_indx, ref IntPtr values);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // exports CSC matrix from internal representation
        internal static extern SPARSE_Status mkl_sparse_d_export_csc_64(
            IntPtr source, ref SPARSE_IndexBase indexing,
            long* rows, long* cols,
            long** cols_start, long** cols_end,
            long** row_indx, double** values);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // exports CSC matrix from internal representation
        internal static extern SPARSE_Status mkl_sparse_z_export_csc_64(
            IntPtr source, ref SPARSE_IndexBase indexing,
            long* rows, long* cols,
            long** cols_start, long** cols_end,
            long** row_indx, void** values);

        #endregion
        #region ---- export [BSR] ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // exports BSR matrix from internal representation
        internal static extern SPARSE_Status mkl_sparse_d_export_bsr_64(
            IntPtr source, ref SPARSE_IndexBase indexing,
            ref long rows, ref long cols, ref long block_size,
            ref IntPtr rows_start, ref IntPtr rows_end,
            ref IntPtr col_indx, ref IntPtr values);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // exports BSR matrix from internal representation
        internal static extern SPARSE_Status mkl_sparse_z_export_bsr_64(
            IntPtr source, ref SPARSE_IndexBase indexing,
            ref long rows, ref long cols, ref long block_size,
            ref IntPtr rows_start, ref IntPtr rows_end,
            ref IntPtr col_indx, ref IntPtr values);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // exports BSR matrix from internal representation
        internal static extern SPARSE_Status mkl_sparse_d_export_bsr_64(
            IntPtr source, ref SPARSE_IndexBase indexing,
            long* rows, long* cols, long* block_size,
            long** rows_start, long** rows_end,
            long** col_indx, double** values);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // exports BSR matrix from internal representation
        internal static extern SPARSE_Status mkl_sparse_z_export_bsr_64(
            IntPtr source, ref SPARSE_IndexBase indexing,
            long* rows, long* cols, long* block_size,
            long** rows_start, long** rows_end,
            long** col_indx, void** values);

        #endregion
        #region ---- set ----

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // updates existing value in the matrix
        internal static extern SPARSE_Status mkl_sparse_d_set_value_64(
            IntPtr A, long row, long col, double value);


        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // updates existing value in the matrix
        internal static extern SPARSE_Status mkl_sparse_z_set_value_64(
            IntPtr A, long row, long col, Complex value);

        #endregion
        #region ---- update ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // updates existing values in the matrix for internal storage only
        internal static extern SPARSE_Status mkl_sparse_d_update_values_64(
            IntPtr A, long nvalues, IntPtr indx, IntPtr indy, IntPtr values);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // updates existing values in the matrix for internal storage only
        internal static extern SPARSE_Status mkl_sparse_z_update_values_64(
            IntPtr A, long nvalues, IntPtr indx, IntPtr indy, IntPtr values);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // updates existing values in the matrix for internal storage only
        internal static extern SPARSE_Status mkl_sparse_d_update_values_64(
            IntPtr A, long nvalues, long* indx, long* indy, double* values);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // updates existing values in the matrix for internal storage only
        internal static extern SPARSE_Status mkl_sparse_z_update_values_64(
            IntPtr A, long nvalues, long* indx, long* indy, void* values);

        #endregion
        #region ---- order ----

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // orders the matrix
        internal static extern SPARSE_Status mkl_sparse_order_64(IntPtr A);

        #endregion
        #region ---- optimization ----

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // describes expected operations with amount of iterations
        internal static extern SPARSE_Status mkl_sparse_set_mv_hint_64(IntPtr A,
            SPARSE_Operation operation, SPARSE_MatrixDescr descr, long calls);


        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // optimize matrix described by the handle
        internal static extern SPARSE_Status mkl_sparse_optimize_64(IntPtr A);


        #endregion
        #region ---- mv ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes a sparse matrix-vector product
        // y = alpha * op(A) * x + beta * y
        internal static extern SPARSE_Status mkl_sparse_d_mv_64(
            SPARSE_Operation operation, double alpha,
            [In] IntPtr A, SPARSE_MatrixDescr descr,
            [In] IntPtr x, double beta, [In, Out] IntPtr y);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes a sparse matrix-vector product
        // y = alpha * op(A) * x + beta * y
        internal static extern SPARSE_Status mkl_sparse_z_mv_64(
            SPARSE_Operation operation, ref Complex alpha,
            [In] IntPtr A, SPARSE_MatrixDescr descr,
            [In] IntPtr x, ref Complex beta, [In, Out] IntPtr y);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes a sparse matrix-vector product
        // y = alpha * op(A) * x + beta * y
        internal static extern SPARSE_Status mkl_sparse_d_mv_64(
            SPARSE_Operation operation, double alpha,
            [In] IntPtr A, SPARSE_MatrixDescr descr,
            [In] double* x, double beta, [In, Out] double* y);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes a sparse matrix-vector product
        // y = alpha * op(A) * x + beta * y
        internal static extern SPARSE_Status mkl_sparse_z_mv_64(
            SPARSE_Operation operation, void* alpha,
            [In] IntPtr A, SPARSE_MatrixDescr descr,
            [In] void* x, void* beta, [In, Out] void* y);

        #endregion
        #region ---- dotmv ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes y = alpha * A * x + beta * y
        // and d = <x, y> , the l2 inner product
        internal static extern SPARSE_Status mkl_sparse_d_dotmv_64(
            SPARSE_Operation transA, double alpha,
            [In] IntPtr A, SPARSE_MatrixDescr descr,
            [In] IntPtr x, double beta, [In, Out] IntPtr y, ref double d);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes y = alpha * A * x + beta * y
        // and d = <x, y> , the l2 inner product
        internal static extern SPARSE_Status mkl_sparse_z_dotmv_64(
            SPARSE_Operation transA, ref Complex alpha,
            [In] IntPtr A, SPARSE_MatrixDescr descr,
            [In] IntPtr x, ref Complex beta, [In, Out] IntPtr y, ref Complex d);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes y = alpha * A * x + beta * y
        // and d = <x, y> , the l2 inner product
        internal static extern SPARSE_Status mkl_sparse_d_dotmv_64(
            SPARSE_Operation transA, double alpha,
            [In] IntPtr A, SPARSE_MatrixDescr descr,
            [In] double* x, double beta, [In, Out] double* y, double* d);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes y = alpha * A * x + beta * y
        // and d = <x, y> , the l2 inner product
        internal static extern SPARSE_Status mkl_sparse_z_dotmv_64(
            SPARSE_Operation transA, void* alpha,
            [In] IntPtr A, SPARSE_MatrixDescr descr,
            [In] void* x, void* beta, [In, Out] void* y, void* d);

        #endregion
        #region ---- trsv ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // solves triangular system
        // y = alpha * A^{-1} * x 
        internal static extern SPARSE_Status mkl_sparse_d_trsv_64(
            SPARSE_Operation operation, double alpha,
            [In] IntPtr A, SPARSE_MatrixDescr descr,
            [In] IntPtr x, [Out] IntPtr y);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // solves triangular system
        // y = alpha * A^{-1} * x 
        internal static extern SPARSE_Status mkl_sparse_z_trsv_64(
            SPARSE_Operation operation, ref Complex alpha,
            [In] IntPtr A, SPARSE_MatrixDescr descr,
            [In] IntPtr x, [Out] IntPtr y);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // solves triangular system
        // y = alpha * A^{-1} * x 
        internal static extern SPARSE_Status mkl_sparse_d_trsv_64(
            SPARSE_Operation operation, double alpha,
            [In] IntPtr A, SPARSE_MatrixDescr descr,
            [In] double* x, [Out] double* y);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // solves triangular system
        // y = alpha * A^{-1} * x 
        internal static extern SPARSE_Status mkl_sparse_z_trsv_64(
            SPARSE_Operation operation, void* alpha,
            [In] IntPtr A, SPARSE_MatrixDescr descr,
            [In] void* x, [Out] void* y);

        #endregion
        #region ---- symgs ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // applies symmetric Gauss-Seidel preconditioner to
        // symmetric system A * x = b
        internal static extern SPARSE_Status mkl_sparse_d_symgs_64(
            SPARSE_Operation op, [In] IntPtr A, SPARSE_MatrixDescr descr,
            double alpha, [In] IntPtr b, [Out] IntPtr x);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // applies symmetric Gauss-Seidel preconditioner to
        // symmetric system A * x = b
        internal static extern SPARSE_Status mkl_sparse_z_symgs_64(
            SPARSE_Operation op, [In] IntPtr A, SPARSE_MatrixDescr descr,
            ref Complex alpha, [In] IntPtr b, [Out] IntPtr x);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // applies symmetric Gauss-Seidel preconditioner to
        // symmetric system A * x = b
        internal static extern SPARSE_Status mkl_sparse_d_symgs_64(
            SPARSE_Operation op, [In] IntPtr A, SPARSE_MatrixDescr descr,
            double alpha, [In] double* b, [Out] double* x);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // applies symmetric Gauss-Seidel preconditioner to
        // symmetric system A * x = b
        internal static extern SPARSE_Status mkl_sparse_z_symgs_64(
            SPARSE_Operation op, [In] IntPtr A, SPARSE_MatrixDescr descr,
            void* alpha, [In] void* b, [Out] void* x);

        #endregion
        #region ---- symgs_mv ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // applies symmetric Gauss-Seidel preconditioner to
        // symmetric system A * x = b
        // also returns y = A * x
        internal static extern SPARSE_Status mkl_sparse_d_symgs_mv_64(
            SPARSE_Operation op, [In] IntPtr A, SPARSE_MatrixDescr descr,
            double alpha, [In] IntPtr b, [Out] IntPtr x, [Out] IntPtr y);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // applies symmetric Gauss-Seidel preconditioner to
        // symmetric system A * x = b
        // also returns y = A * x
        internal static extern SPARSE_Status mkl_sparse_z_symgs_mv_64(
            SPARSE_Operation op, [In] IntPtr A, SPARSE_MatrixDescr descr,
            ref Complex alpha, [In] IntPtr b, [Out] IntPtr x, [Out] IntPtr y);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // applies symmetric Gauss-Seidel preconditioner to
        // symmetric system A * x = b
        // also returns y = A * x
        internal static extern SPARSE_Status mkl_sparse_d_symgs_mv_64(
            SPARSE_Operation op, [In] IntPtr A, SPARSE_MatrixDescr descr,
            double alpha, [In] double* b, [Out] double* x, [Out] double* y);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // applies symmetric Gauss-Seidel preconditioner to
        // symmetric system A * x = b
        // also returns y = A * x
        internal static extern SPARSE_Status mkl_sparse_z_symgs_mv_64(
            SPARSE_Operation op, [In] IntPtr A, SPARSE_MatrixDescr descr,
            void* alpha, [In] void* b, [Out] void* x, [Out] void* y);

        #endregion
        #region ---- lu smoother ----

        // ...

        #endregion
        #region ---- mm ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes the product of a sparse matrix and a dense
        // matrix and stores the result as a dense matrix
        // y = alpha * op(A) * x + beta * y
        internal static extern SPARSE_Status mkl_sparse_d_mm_64(
            SPARSE_Operation operation, double alpha,
            [In] IntPtr A, SPARSE_MatrixDescr descr, SPARSE_Layout layout,
            [In] IntPtr x, long columns, long ldx, 
            double beta, [In, Out] IntPtr y, long ldy);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes the product of a sparse matrix and a dense
        // matrix and stores the result as a dense matrix
        // y = alpha * op(A) * x + beta * y
        internal static extern SPARSE_Status mkl_sparse_z_mm_64(
            SPARSE_Operation operation, ref Complex alpha,
            [In] IntPtr A, SPARSE_MatrixDescr descr, SPARSE_Layout layout,
            [In] IntPtr x, long columns, long ldx,
            ref Complex beta, [In, Out] IntPtr y, long ldy);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes the product of a sparse matrix and a dense
        // matrix and stores the result as a dense matrix
        // y = alpha * op(A) * x + beta * y
        internal static extern SPARSE_Status mkl_sparse_d_mm_64(
            SPARSE_Operation operation, double alpha,
            [In] IntPtr A, SPARSE_MatrixDescr descr, SPARSE_Layout layout,
            [In] double* x, long columns, long ldx,
            double beta, [In, Out] double* y, long ldy);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes the product of a sparse matrix and a dense
        // matrix and stores the result as a dense matrix
        // y = alpha * op(A) * x + beta * y
        internal static extern SPARSE_Status mkl_sparse_z_mm_64(
            SPARSE_Operation operation, void* alpha,
            [In] IntPtr A, SPARSE_MatrixDescr descr, SPARSE_Layout layout,
            [In] void* x, long columns, long ldx,
            void* beta, [In, Out] void* y, long ldy);

        #endregion
        #region ---- trsm ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // solves triangular system y = alpha * A^{-1} * x
        internal static extern SPARSE_Status mkl_sparse_d_trsm_64(
            SPARSE_Operation operation, double alpha,
            [In] IntPtr A, SPARSE_MatrixDescr descr, SPARSE_Layout layout,
            [In] IntPtr x, long columns, long ldx,
            [In, Out] IntPtr y, long ldy);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // solves triangular system y = alpha * A^{-1} * x
        internal static extern SPARSE_Status mkl_sparse_z_trsm_64(
            SPARSE_Operation operation, ref Complex alpha,
            [In] IntPtr A, SPARSE_MatrixDescr descr, SPARSE_Layout layout,
            [In] IntPtr x, long columns, long ldx,
            [In, Out] IntPtr y, long ldy);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // solves triangular system y = alpha * A^{-1} * x
        internal static extern SPARSE_Status mkl_sparse_d_trsm_64(
            SPARSE_Operation operation, double alpha,
            [In] IntPtr A, SPARSE_MatrixDescr descr, SPARSE_Layout layout,
            [In] double* x, long columns, long ldx,
            [In, Out] double* y, long ldy);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // solves triangular system y = alpha * A^{-1} * x
        internal static extern SPARSE_Status mkl_sparse_z_trsm_64(
            SPARSE_Operation operation, void* alpha,
            [In] IntPtr A, SPARSE_MatrixDescr descr, SPARSE_Layout layout,
            [In] void* x, long columns, long ldx,
            [In, Out] void* y, long ldy);

        #endregion
        #region ---- add ----

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes sum of sparse matrices
        // C = alpha * op(A) + B, result is sparse
        internal static extern SPARSE_Status mkl_sparse_z_add_64(
            SPARSE_Operation operation, [In] IntPtr A,
            void* alpha, [In] IntPtr B, [In, Out] ref IntPtr C);


        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes sum of sparse matrices
        // C = alpha * op(A) + B, result is sparse
        internal static extern SPARSE_Status mkl_sparse_d_add_64(
            SPARSE_Operation operation, [In] IntPtr A, 
            double alpha, [In] IntPtr B, [In, Out] ref IntPtr C);


        // wrapper
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes sum of sparse matrices
        // C = alpha * op(A) + B, result is sparse
        internal static extern SPARSE_Status mkl_sparse_z_add_64(
            SPARSE_Operation operation, [In] IntPtr A,
            ref Complex alpha, [In] IntPtr B, [In, Out] ref IntPtr C);

        #endregion
        #region ---- spmm ----

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes product of sparse matrices
        // C = op(A) * B, result is sparse
        internal static extern SPARSE_Status mkl_sparse_spmm_64(
            SPARSE_Operation operation, [In] IntPtr A,
            [In] IntPtr B, [In, Out] ref IntPtr C);

        #endregion
        #region ---- sp2m ----

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes product of sparse matrices
        // C = opA(A) * opB(B), result is sparse
        internal static extern SPARSE_Status mkl_sparse_sp2m_64(
            SPARSE_Operation transA, SPARSE_MatrixDescr descrA, [In] IntPtr A,
            SPARSE_Operation transB, SPARSE_MatrixDescr descrB, [In] IntPtr B,
            SPARSE_Request request, [In, Out] ref IntPtr C);

        #endregion
        #region ---- syrk ----

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes product of sparse matrix with its transpose (or conjugate transpose)
        // C = op(A) * (op(A))^{T for real or H for complex}
        // result is sparse
        internal static extern SPARSE_Status mkl_sparse_syrk_64(
            SPARSE_Operation operation, [In] IntPtr A, [In, Out] ref IntPtr C);

        #endregion
        #region ---- sypr ----

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes product of sparse matrices:
        // C = op(A) * B * (op(A))^{T for real or H for complex}
        // result is sparse   
        internal static extern SPARSE_Status mkl_sparse_sypr_64(
            SPARSE_Operation transA, [In] IntPtr A, [In] IntPtr B,
            SPARSE_MatrixDescr descrB, [In, Out] ref IntPtr C,
            SPARSE_Request request);

        #endregion
        #region ---- spmmd ----

        // Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes product of sparse matrices:
        // C = op(A) * B
        // A and B are a sparse input matrices
        // layout describes the storage scheme of the dense matrix
        // result is dense
        internal static extern SPARSE_Status mkl_sparse_d_spmmd_64(
            SPARSE_Operation operation, [In] IntPtr A,
            [In] IntPtr B, SPARSE_Layout layout, 
            [Out] double* C, long ldc);


        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes product of sparse matrices:
        // C = op(A) * B
        // A and B are a sparse input matrices
        // layout describes the storage scheme of the dense matrix
        // result is dense
        internal static extern SPARSE_Status mkl_sparse_z_spmmd_64(
            SPARSE_Operation operation, [In] IntPtr A,
            [In] IntPtr B, SPARSE_Layout layout,
            [Out] void* C, long ldc);


        // IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes product of sparse matrices:
        // C = op(A) * B
        // A and B are a sparse input matrices
        // layout describes the storage scheme of the dense matrix
        // result is dense
        internal static extern SPARSE_Status mkl_sparse_d_spmmd_64(
            SPARSE_Operation operation, [In] IntPtr A,
            [In] IntPtr B, SPARSE_Layout layout,
            [Out] IntPtr C, long ldc);


        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes product of sparse matrices:
        // C = op(A) * B
        // A and B are a sparse input matrices
        // layout describes the storage scheme of the dense matrix
        // result is dense
        internal static extern SPARSE_Status mkl_sparse_z_spmmd_64(
            SPARSE_Operation operation, [In] IntPtr A,
            [In] IntPtr B, SPARSE_Layout layout,
            [Out] IntPtr C, long ldc);

        #endregion
        #region ---- sp2md ----

        // pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes product of sparse matrices:
        // C = alpha * opA(A) *opB(B) + beta*C
        // A and B are a sparse input matrices
        // layout describes the storage scheme of the dense matrix
        // result is dense
        internal static extern SPARSE_Status mkl_sparse_d_sp2md_64(
            SPARSE_Operation transA, SPARSE_MatrixDescr descrA, [In] IntPtr A,
            SPARSE_Operation transB, SPARSE_MatrixDescr descrB, [In] IntPtr B,
            double alpha, double beta,
            [Out] double* C, SPARSE_Layout layout, long ldc);


        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes product of sparse matrices:
        // C = alpha * opA(A) *opB(B) + beta*C
        // A and B are a sparse input matrices
        // layout describes the storage scheme of the dense matrix
        // result is dense
        internal static extern SPARSE_Status mkl_sparse_z_sp2md_64(
            SPARSE_Operation transA, SPARSE_MatrixDescr descrA, [In] IntPtr A,
            SPARSE_Operation transB, SPARSE_MatrixDescr descrB, [In] IntPtr B,
            void* alpha, void* beta,
            [Out] void* C, SPARSE_Layout layout, long ldc);


        // IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes product of sparse matrices:
        // C = alpha * opA(A) *opB(B) + beta*C
        // A and B are a sparse input matrices
        // layout describes the storage scheme of the dense matrix
        // result is dense
        internal static extern SPARSE_Status mkl_sparse_d_sp2md_64(
            SPARSE_Operation transA, SPARSE_MatrixDescr descrA, [In] IntPtr A,
            SPARSE_Operation transB, SPARSE_MatrixDescr descrB, [In] IntPtr B, 
            double alpha, double beta, 
            [Out] IntPtr C, SPARSE_Layout layout, long ldc);


        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes product of sparse matrices:
        // C = alpha * opA(A) *opB(B) + beta*C
        // A and B are a sparse input matrices
        // layout describes the storage scheme of the dense matrix
        // result is dense
        internal static extern SPARSE_Status mkl_sparse_z_sp2md_64(
            SPARSE_Operation transA, SPARSE_MatrixDescr descrA, [In] IntPtr A,
            SPARSE_Operation transB, SPARSE_MatrixDescr descrB, [In] IntPtr B,
            ref Complex alpha, ref Complex beta,
            [Out] IntPtr C, SPARSE_Layout layout, long ldc);

        #endregion
        #region ---- syrkd ----

        // pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes product of sparse matrices
        // C = beta*C + alpha*A*op(A), or
        // C = beta*C + alpha*op(A)*A
        // result is dense
        internal static extern SPARSE_Status mkl_sparse_d_syrkd_64(
            SPARSE_Operation operation, [In] IntPtr A, 
            double alpha, double beta, [In, Out] double* C,
            SPARSE_Layout layout, long ldc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes product of sparse matrices
        // C = beta*C + alpha*A*op(A), or
        // C = beta*C + alpha*op(A)*A
        // result is dense
        internal static extern SPARSE_Status mkl_sparse_z_syrkd_64(
            SPARSE_Operation operation, [In] IntPtr A,
            void* alpha, void* beta, [In, Out] void* C,
            SPARSE_Layout layout, long ldc);


        // IntPtr
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes product of sparse matrices
        // C = beta*C + alpha*A*op(A), or
        // C = beta*C + alpha*op(A)*A
        // result is dense
        internal static extern SPARSE_Status mkl_sparse_d_syrkd_64(
            SPARSE_Operation operation, [In] IntPtr A,
            double alpha, double beta, [In, Out] IntPtr C,
            SPARSE_Layout layout, long ldc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes product of sparse matrices
        // C = beta*C + alpha*A*op(A), or
        // C = beta*C + alpha*op(A)*A
        // result is dense
        internal static extern SPARSE_Status mkl_sparse_z_syrkd_64(
            SPARSE_Operation operation, [In] IntPtr A,
            ref Complex alpha, ref Complex beta, [In, Out] IntPtr C,
            SPARSE_Layout layout, long ldc);

        #endregion
        #region ---- syprd ----

        // pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes product of sparse matrices:
        // C = op(A) * B * (op(A))^{T for real or H for complex}
        // B is a dense input matrix
        // result is dense
        internal static extern SPARSE_Status mkl_sparse_d_syprd_64(
            SPARSE_Operation operation, [In] IntPtr A,
            [In] IntPtr B, SPARSE_Layout layoutB, long ldb,
            double alpha, double beta, [Out] double* C,
            SPARSE_Layout layoutC, long ldc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes product of sparse matrices:
        // C = op(A) * B * (op(A))^{T for real or H for complex}
        // B is a dense input matrix
        // result is dense
        internal static extern SPARSE_Status mkl_sparse_z_syprd_64(
            SPARSE_Operation operation, [In] IntPtr A,
            [In] IntPtr B, SPARSE_Layout layoutB, long ldb,
            void* alpha, void* beta, [Out] void* C,
            SPARSE_Layout layoutC, long ldc);


        // IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes product of sparse matrices:
        // C = op(A) * B * (op(A))^{T for real or H for complex}
        // B is a dense input matrix
        // result is dense
        internal static extern SPARSE_Status mkl_sparse_d_syprd_64(
            SPARSE_Operation operation, [In] IntPtr A,
            [In] IntPtr B, SPARSE_Layout layoutB, long ldb,
            double alpha, double beta, [Out] IntPtr C,
            SPARSE_Layout layoutC, long ldc);


        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes product of sparse matrices:
        // C = op(A) * B * (op(A))^{T for real or H for complex}
        // B is a dense input matrix
        // result is dense
        internal static extern SPARSE_Status mkl_sparse_z_syprd_64(
            SPARSE_Operation operation, [In] IntPtr A,
            [In] IntPtr B, SPARSE_Layout layoutB, long ldb,
            ref Complex alpha, ref Complex beta, [Out] IntPtr C,
            SPARSE_Layout layoutC, long ldc);

        #endregion
        #region ---- sorv ----

        // pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes forward or backward sweep of successive
        // over-relaxation (SOR), or Symmetric successive
        // over-relaxation(SSOR)
        internal static extern SPARSE_Status mkl_sparse_d_sorv_64(
            SPARSE_SorType type,   /* choice of forward, backward sweep or SSOR operation */
            SPARSE_MatrixDescr descrA,
            [In] IntPtr A,
            double omega,
            double alpha,  /* alpha equals to 0 mean zero initial guess */
            [In, Out] double* x,      /* solution vector and alpha * x is initial guess */
            [In] double* b);    /* right-hand side */


        // IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes forward or backward sweep of successive
        // over-relaxation (SOR), or Symmetric successive
        // over-relaxation(SSOR)
        internal static extern SPARSE_Status mkl_sparse_d_sorv_64(
            SPARSE_SorType type,   /* choice of forward, backward sweep or SSOR operation */
            SPARSE_MatrixDescr descrA,
            [In] IntPtr A,
            double omega,
            double alpha,  /* alpha equals to 0 mean zero initial guess */
            [In, Out] IntPtr x,      /* solution vector and alpha * x is initial guess */
            [In] IntPtr b);    /* right-hand side */

        #endregion

        #endregion

        #region LAPACK

        #region ---- dgetrf, zgetrf ----

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes the LU factorization of a general m-by-n matrix
        internal static extern long LAPACKE_dgetrf_64(LAPACK_Layout layout, 
            long m, long n, [In, Out] double* a, long lda, [Out] long* ipiv);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes the LU factorization of a general m-by-n matrix
        internal static extern long LAPACKE_zgetrf_64(LAPACK_Layout layout, 
            long m, long n, [In, Out] void* a, long lda, [Out] long* ipiv);


        // obsolete
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        // computes the LU factorization of a general m-by-n matrix
        internal static extern int LAPACKE_dgetrf(int layout, long m, long n,
            [In, Out] double* a, long lda, [Out] long* ipiv);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        // computes the LU factorization of a general m-by-n matrix
        internal static extern int LAPACKE_zgetrf(int layout, long m, long n,
            [In, Out] Complex* a, long lda, [Out] long* ipiv);

        #endregion
        #region ---- dgetrs, zgetrs ----

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // solves a system of linear equations 
        // with an LU-factored square coefficient matrix
        internal static extern long LAPACKE_dgetrs_64(LAPACK_Layout layout, 
            LAPACK_Transpose trans, long n, long nrhs, double* a, long lda, 
            long* ipiv, double* b, long ldb);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // solves a system of linear equations 
        // with an LU-factored square coefficient matrix
        internal static extern long LAPACKE_zgetrs_64(LAPACK_Layout layout, 
            LAPACK_Transpose trans, long n, long nrhs, void* a, long lda, 
            long* ipiv, void* b, long ldb);


        // obsolete
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        // solves a system of linear equations 
        // with an LU-factored square coefficient matrix
        internal static extern int LAPACKE_dgetrs(int layout, char trans,
            long n, long nrhs, double* a, long lda, long* ipiv, double* b, long ldb);



        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        // solves a system of linear equations 
        // with an LU-factored square coefficient matrix
        internal static extern int LAPACKE_zgetrs(int layout, char trans,
            long n, long nrhs, Complex* a, long lda, long* ipiv, Complex* b, long ldb);

        #endregion
        #region ---- dgetri, zgetri ----

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes the inverse of an LU-factored general matrix
        internal static extern long LAPACKE_dgetri_64(LAPACK_Layout layout, long n,
            [In, Out] double* a, long lda, [In] long* ipiv);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes the inverse of an LU-factored general matrix
        internal static extern long LAPACKE_zgetri_64(LAPACK_Layout layout, long n,
            [In, Out] void* a, long lda, [In] long* ipiv);


        // obsolete
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, 
            ExactSpelling = true, SetLastError = false)]
        // computes the inverse of an LU-factored general matrix
        internal static extern int LAPACKE_dgetri(int layout, long n,
            [In, Out] double* a, long lda, [In] long* ipiv);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        // computes the inverse of an LU-factored general matrix
        internal static extern int LAPACKE_zgetri(int layout, long n,
            [In, Out] Complex* a, long lda, [In] long* ipiv);
        
        #endregion
        #region ---- dgesv, zgesv ----
        
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // Computes the solution to the system of linear equations 
        // with a square coefficient matrix A and multiple right-hand sides
        internal static extern long LAPACKE_dgesv_64(LAPACK_Layout layout, 
            long n, long nrhs, double* a, long lda, long* ipiv,
            double* b, long ldb);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // Computes the solution to the system of linear equations 
        // with a square coefficient matrix A and multiple right-hand sides
        internal static extern long LAPACKE_zgesv_64(LAPACK_Layout layout, 
            long n, long nrhs, void* a, long lda, long* ipiv,
            void* b, long ldb);


        // obsolete
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        // Computes the solution to the system of linear equations 
        // with a square coefficient matrix A and multiple right-hand sides
        internal static extern int LAPACKE_dgesv(int layout, long n, long nrhs,
            double* a, long lda, long* ipiv,
            double* b, long ldb);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        // Computes the solution to the system of linear equations 
        // with a square coefficient matrix A and multiple right-hand sides
        internal static extern int LAPACKE_zgesv(int layout, long n, long nrhs,
            Complex* a, long lda, long* ipiv,
            Complex* b, long ldb);
        
        #endregion
        #region ---- dgeev, zgeev ----

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // Computes the eigenvalues and left and right eigenvectors of a general matrix
        internal static extern long LAPACKE_dgeev_64(LAPACK_Layout layout, 
            LAPACK_Job jobvl, LAPACK_Job jobvr, long n,
            double* a, long lda, double* wr, double* wi, 
            double* vl, long ldvl, double* vr, long ldvr);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
             ExactSpelling = true, SetLastError = false)]
        // Computes the eigenvalues and left and right eigenvectors of a general matrix
        internal static extern long LAPACKE_zgeev_64(LAPACK_Layout layout, 
            LAPACK_Job jobvl, LAPACK_Job jobvr, long n,
            void* a, long lda, void* w,
            void* vl, long ldvl, void* vr, long ldvr);


        // obsolete
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
             ExactSpelling = true, SetLastError = false)]
        // Computes the eigenvalues and left and right eigenvectors of a general matrix
        internal static extern int LAPACKE_dgeev(int layout, char jobvl, char jobvr, long n,
            double* a, long lda,
            double* wr, double* wi,
            double* vl, long ldvl, double* vr, long ldvr);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
             ExactSpelling = true, SetLastError = false)]
        // Computes the eigenvalues and left and right eigenvectors of a general matrix
        internal static extern int LAPACKE_zgeev(int layout, char jobvl, char jobvr, long n,
            Complex* a, long lda,
            Complex* w,
            Complex* vl, long ldvl, Complex* vr, long ldvr);

        #endregion
        #region ---- dggev, zggev ---- 

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // Computes the general eigenvalues
        internal static extern long LAPACKE_dggev_64(LAPACK_Layout layout, 
            LAPACK_Job jobvl, LAPACK_Job jobvr, long n,
            double* a, long lda, double* b, long ldb,
            double* alphar, double* alphai, double* beta,
            double* vl, long ldvl, double* vr, long ldvr);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // Computes the general eigenvalues about a complex matrix
        internal static extern long LAPACKE_zggev_64(LAPACK_Layout layout, 
            LAPACK_Job jobvl, LAPACK_Job jobvr, long n,
            void* a, long lda, void* b, long ldb,
            void* alpha, void* beta,
            void* vl, long ldvl, void* vr, long ldvr);


        // obsolete
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        // Computes the general eigenvalues
        internal static extern int LAPACKE_dggev(int layout, char jobvl, char jobvr, long n,
            double* a, long ida,
            double* b, long idb,
            double* alphar, double* alphai, double* beta,
            double* vl, long ldvl,
            double* vr, long ldvr);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        // Computes the general eigenvalues about a complex matrix
        internal static extern int LAPACKE_zggev(int layout, char jobvl, char jobvr, long n,
            Complex* a, long ida,
            Complex* b, long idb,
            Complex* alpha, Complex* beta,
            Complex* vl, long ldvl,
            Complex* vr, long ldvr);

        #endregion
        #region ---- dsyev ----

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // Computes all eigenvalues and, optionally, eigenvectors 
        // of a real symmetric matrix
        internal static extern long LAPACKE_dsyev_64(LAPACK_Layout layout, 
            LAPACK_Job jobz, char uplo, long n,
            double* a, long lda, double* w);


        // obsolete
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
             ExactSpelling = true, SetLastError = false)]
        // Computes all eigenvalues and, optionally, eigenvectors 
        // of a real symmetric matrix
        internal static extern int LAPACKE_dsyev(int layout, char jobz, char uplo, long n,
            double* a, long lda, double* w);

        #endregion
        #region ---- zheev ----

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // Computes all eigenvalues and, optionally, eigenvectors
        // of a Hermitian matrix
        internal static extern long LAPACKE_zheev_64(LAPACK_Layout layout, 
            LAPACK_Job jobz, char uplo, long n,
            void* a, long lda, double* w);


        // obsolete
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
             ExactSpelling = true, SetLastError = false)]
        // Computes all eigenvalues and, optionally, eigenvectors
        // of a Hermitian matrix
        internal static extern int LAPACKE_zheev(int layout, char jobz, char uplo, long n,
            Complex* a, long lda, double* w);
        
        #endregion
        #region ---- dgesvd, zgesvd ----

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // Computes the singular value decomposition of a general rectangular matrix
        internal static extern long LAPACKE_dgesvd_64(LAPACK_Layout layout, 
            LAPACK_Job jobu, LAPACK_Job jobvt, long m, long n,
            double* a, long lda, double* s, double* u, long ldu,
            double* vt, long ldvt, double* superb);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // Computes the singular value decomposition of a general rectangular matrix
        internal static extern long LAPACKE_zgesvd_64(LAPACK_Layout layout, 
            LAPACK_Job jobu, LAPACK_Job jobvt, long m, long n,
            void* a, long lda, double* s, void* u, long ldu,
            void* vt, long ldvt, double* superb);


        // obsolete
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
             ExactSpelling = true, SetLastError = false)]
        // Computes the singular value decomposition of a general rectangular matrix
        internal static extern int LAPACKE_dgesvd(int layout, char jobu, char jobvt,
            long m, long n,
            double* a, long lda, double* s,
            double* u, long ldu,
            double* vt, long ldvt, double* superb);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
             ExactSpelling = true, SetLastError = false)]
        // Computes the singular value decomposition of a general rectangular matrix
        internal static extern int LAPACKE_zgesvd(int layout, char jobu, char jobvt,
            long m, long n,
            Complex* a, long lda, double* s,
            Complex* u, long ldu,
            Complex* vt, long ldvt, double* superb);
        
        #endregion
        #region ---- dgels, zgels ----
        
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // Uses QR or LQ factorization to solve a overdetermined or 
        // underdetermined linear system with full rank
        internal static extern long LAPACKE_dgels_64(LAPACK_Layout layout, 
            LAPACK_Transpose trans, long m, long n, long nrhs,
            double* a, long lda, double* b, long ldb);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // Uses QR or LQ factorization to solve a overdetermined or 
        // underdetermined linear system with full rank
        internal static extern long LAPACKE_zgels_64(LAPACK_Layout layout, 
            LAPACK_Transpose trans, long m, long n, long nrhs,
            void* a, long lda, void* b, long ldb);


        // obsolete
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
             ExactSpelling = true, SetLastError = false)]
        // Uses QR or LQ factorization to solve a overdetermined or 
        // underdetermined linear system with full rank
        internal static extern int LAPACKE_dgels(int layout, char trans,
            long m, long n, long nrhs,
            double* a, long lda,
            double* b, long ldb);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
             ExactSpelling = true, SetLastError = false)]
        // Uses QR or LQ factorization to solve a overdetermined or 
        // underdetermined linear system with full rank
        internal static extern int LAPACKE_zgels(int layout, char trans,
            long m, long n, long nrhs,
            Complex* a, long lda,
            Complex* b, long ldb);

        #endregion
        #region ---- zlacgv ----
        
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // Conjugates a complex vector
        internal static extern long LAPACKE_zlacgv_64(long n,
            void* x, long incx);
        
        #endregion

        #endregion
    
        #region VMF

        #region ---- vdAbs, vzAbs ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes absolute value of vector elements 
        // y[i] = |a[i]|
        internal static extern void vdAbs_64(long n, 
            [In] IntPtr a, 
            [Out] IntPtr y);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes absolute value of vector elements
        // y[i] = |a[i]|
        internal static extern void vzAbs_64(long n, 
            [In] IntPtr a, 
            [Out] IntPtr y);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes absolute value of vector elements
        // y[i] = |a[i]|
        internal static extern void vdAbs_64(long n,
            [In] double* a, 
            [Out] double* y);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes absolute value of vector elements
        // y[i] = |a[i]|
        internal static extern void vzAbs_64(long n,
            [In] void* a, 
            [Out] double* y);

        #endregion
        #region ---- vzArg ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes argument of vector elements
        // y[i] = arg(a[i])
        internal static extern void vzArg_64(long n, 
            [In] IntPtr a, 
            [Out] IntPtr y);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        // computes argument of vector elements
        // y[i] = arg(a[i])
        internal static extern void vzArg_64(long n,
            [In] void* a, 
            [Out] double* y);

        #endregion
        #region ---- vdAdd, vzAdd ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // performs element by element addition of vector a and vector b
        // y[i] = a[i] + b[i]
        internal static extern void vdAdd_64(long n, 
            [In] IntPtr a, [In] IntPtr b,
            [Out] IntPtr y);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // performs element by element addition of vector a and vector b
        // y[i] = a[i] + b[i]
        internal static extern void vzAdd_64(long n, 
            [In] IntPtr a, [In] IntPtr b,
            [Out] IntPtr y);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
         ExactSpelling = true, SetLastError = false)]
        // performs element by element addition of vector a and vector b
        // y[i] = a[i] + b[i]
        internal static extern void vdAdd_64(long n,
            [In] double* a, [In] double* b,
            [Out] double* y);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
             ExactSpelling = true, SetLastError = false)]
        // performs element by element addition of vector a and vector b
        // y[i] = a[i] + b[i]
        internal static extern void vzAdd_64(long n,
            [In] void* a, [In] void* b,
            [Out] void* y);

        #endregion
        #region ---- vdSub, vzSub ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // performs element by element substraction of vector b from vector a
        // y[i] = a[i] - b[i]
        internal static extern void vdSub_64(long n, 
            [In] IntPtr a, [In] IntPtr b,
            [Out] IntPtr y);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // performs element by element substraction of vector b from vector a
        // y[i] = a[i] - b[i]
        internal static extern void vzSub_64(long n, 
            [In] IntPtr a, [In] IntPtr b,
            [Out] IntPtr y);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // performs element by element substraction of vector b from vector a
        // y[i] = a[i] - b[i]
        internal static extern void vdSub_64(long n,
            [In] double* a, [In] double* b,
            [Out] double* y);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // performs element by element substraction of vector b from vector a
        // y[i] = a[i] - b[i]
        internal static extern void vzSub_64(long n,
            [In] void* a, [In] void* b,
            [Out] void* y);

        #endregion
        #region ---- vdInv ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // performs element by element inversion of the vector
        // y[i] = 1.0 / a[i]
        internal static extern void vdInv_64(long n, 
            [In] IntPtr a, 
            [Out] IntPtr y);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // performs element by element inversion of the vector
        // y[i] = 1.0 / a[i]
        internal static extern void vdInv_64(long n,
            [In] double* a,
            [Out] double* y);

        #endregion
        #region ---- vdSqrt, vzSqrt ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes a square root of vector elements
        // y[i] = a[i]^0.5
        internal static extern void vdSqrt_64(long n, 
            [In] IntPtr a, 
            [Out] IntPtr y);


        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes a square root of vector elements
        // y[i] = a[i]^0.5
        internal static extern void vzSqrt_64(long n, 
            [In] IntPtr a, 
            [Out] IntPtr y);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes a square root of vector elements
        // y[i] = a[i]^0.5
        internal static extern void vdSqrt_64(long n,
            [In] double* a,
            [Out] double* y);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes a square root of vector elements
        // y[i] = a[i]^0.5
        internal static extern void vzSqrt_64(long n,
            [In] void* a,
            [Out] void* y);

        #endregion
        #region ---- vdInvSqrt ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes an inverse square root of vector elements
        // y[i] = 1/a[i]^0.5
        internal static extern void vdInvSqrt_64(long n, 
            [In] IntPtr a, 
            [Out] IntPtr y);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes an inverse square root of vector elements
        // y[i] = 1/a[i]^0.5
        internal static extern void vdInvSqrt_64(long n,
            [In] double* a,
            [Out] double* y);

        #endregion
        #region ---- vdCbrt ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes a cube root of vector elements
        // y[i] = a[i]^(1/3)
        internal static extern void vdCbrt_64(long n, 
            [In] IntPtr a, 
            [Out] IntPtr y);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes a cube root of vector elements
        // y[i] = a[i]^(1/3)
        internal static extern void vdCbrt_64(long n,
            [In] double* a,
            [Out] double* y);

        #endregion
        #region ---- vdInvCbrt ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes an inverse cube root of vector elements
        // y[i] = 1/a[i]^(1/3)
        internal static extern void vdInvCbrt_64(long n, 
            [In] IntPtr a, 
            [Out] IntPtr y);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes an inverse cube root of vector elements
        // y[i] = 1/a[i]^(1/3)
        internal static extern void vdInvCbrt_64(long n,
            [In] double* a,
            [Out] double* y);

        #endregion
        #region ---- vdSqr ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // performs element by element squaring of the vector
        // y[i] = a[i]^2
        internal static extern void vdSqr_64(long n, 
            [In] IntPtr a, 
            [Out] IntPtr y);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // performs element by element squaring of the vector
        // y[i] = a[i]^2
        internal static extern void vdSqr_64(long n,
            [In] double* a,
            [Out] double* y);

        #endregion
        #region ---- vdExp, vzExp ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes an exponential of vector elements
        // y[i] = e^a[i]
        internal static extern void vdExp_64(long n, 
            [In] IntPtr a, 
            [Out] IntPtr y);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes an exponential of vector elements
        // y[i] = e^a[i]
        internal static extern void vzExp_64(long n, 
            [In] IntPtr a, 
            [Out] IntPtr y);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes an exponential of vector elements
        // y[i] = e^a[i]
        internal static extern void vdExp_64(long n,
            [In] double* a,
            [Out] double* y);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes an exponential of vector elements
        // y[i] = e^a[i]
        internal static extern void vzExp_64(long n,
            [In] void* a,
            [Out] void* y);

        #endregion
        #region ---- vdExp2 ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes an exponential (base 2) of vector elements
        // y[i] = 2^a[i]
        internal static extern void vdExp2_64(long n,
            [In] IntPtr a,
            [Out] IntPtr y);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes an exponential (base 2) of vector elements
        // y[i] = 2^a[i]
        internal static extern void vdExp2_64(long n,
            [In] double* a,
            [Out] double* y);

        #endregion
        #region ---- vdExp10 ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes an exponential (base 10) of vector elements
        // y[i] = 10^a[i]
        internal static extern void vdExp10_64(long n,
            [In] IntPtr a,
            [Out] IntPtr y);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes an exponential (base 10) of vector elements
        // y[i] = 10^a[i]
        internal static extern void vdExp10_64(long n,
            [In] double* a,
            [Out] double* y);

        #endregion
        #region ---- vdExpm1 ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes an exponential of arguments decreased by 1
        // y[i] = e^a[i] - 1
        internal static extern void vdExpm1_64(long n,
            [In] IntPtr a,
            [Out] IntPtr y);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes an exponential of arguments decreased by 1
        // y[i] = e^a[i] - 1
        internal static extern void vdExpm1_64(long n,
            [In] double* a,
            [Out] double* y);

        #endregion
        #region ---- vdLn, vzLn ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes natural logarithm of vector elements
        // y[i] = ln(a[i])
        internal static extern void vdLn_64(long n, 
            [In] IntPtr a, 
            [Out] IntPtr y);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes natural logarithm of vector elements
        // y[i] = ln(a[i])
        internal static extern void vzLn_64(long n, 
            [In] IntPtr a, 
            [Out] IntPtr y);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes natural logarithm of vector elements
        // y[i] = ln(a[i])
        internal static extern void vdLn_64(long n,
            [In] double* a,
            [Out] double* y);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes natural logarithm of vector elements
        // y[i] = ln(a[i])
        internal static extern void vzLn_64(long n,
            [In] void* a,
            [Out] void* y);

        #endregion
        #region ---- vdLog2 ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes logarithm (base 2) of vector elements
        // y[i] = lb(a[i])
        internal static extern void vdLog2_64(long n,
            [In] IntPtr a,
            [Out] IntPtr y);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes logarithm (base 2) of vector elements
        // y[i] = lb(a[i])
        internal static extern void vdLog2_64(long n,
            [In] double* a,
            [Out] double* y);

        #endregion
        #region ---- vdLog10, vzLog10 ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes logarithm (base 10) of vector elements
        // y[i] = lg(a[i])
        internal static extern void vdLog10_64(long n,
            [In] IntPtr a,
            [Out] IntPtr y);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes logarithm (base 10) of vector elements
        // y[i] = lg(a[i])
        internal static extern void vzLog10_64(long n,
            [In] IntPtr a,
            [Out] IntPtr y);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes logarithm (base 10) of vector elements
        // y[i] = lg(a[i])
        internal static extern void vdLog10_64(long n,
            [In] double* a,
            [Out] double* y);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes logarithm (base 10) of vector elements
        // y[i] = lg(a[i])
        internal static extern void vzLog10_64(long n,
            [In] void* a,
            [Out] void* y);

        #endregion
        #region ---- vdLog1p ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes logarithm (base e) of arguments increased by 1
        // y[i] = log(1+a[i])
        internal static extern void vdLog1p_64(long n,
            [In] IntPtr a,
            [Out] IntPtr y);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes logarithm (base e) of arguments increased by 1
        // y[i] = log(1+a[i])
        internal static extern void vdLog1p_64(long n,
            [In] double* a,
            [Out] double* y);

        #endregion
        #region ---- vdLogb ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes the exponent
        // y[i] = logb(a[i])
        internal static extern void vdLogb_64(long n,
            [In] IntPtr a,
            [Out] IntPtr y);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes the exponent
        // y[i] = logb(a[i])
        internal static extern void vdLogb_64(long n,
            [In] double* a,
            [Out] double* y);

        #endregion
        #region ---- vdCos, vzCos ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes cosine of vector elements
        // y[i] = cos(a[i])
        internal static extern void vdCos_64(long n, 
            [In] IntPtr a, 
            [Out] IntPtr y);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes cosine of vector elements
        // y[i] = cos(a[i])
        internal static extern void vzCos_64(long n, 
            [In] IntPtr a, 
            [Out] IntPtr y);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        // computes cosine of vector elements
        // y[i] = cos(a[i])
        internal static extern void vdCos_64(long n,
            [In] double* a,
            [Out] double* y);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        // computes cosine of vector elements        
        // y[i] = cos(a[i])
        internal static extern void vzCos_64(long n,
            [In] void* a,
            [Out] void* y);

        #endregion
        #region ---- vdSin, vzSin ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes sine of vector elements
        // y[i] = sin(a[i])
        internal static extern void vdSin_64(long n, 
            [In] IntPtr a, 
            [Out] IntPtr y);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes sine of vector elements
        // y[i] = sin(a[i])
        internal static extern void vzSin_64(long n, 
            [In] IntPtr a, 
            [Out] IntPtr y);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes sine of vector elements
        // y[i] = sin(a[i])
        internal static extern void vdSin_64(long n,
            [In] double* a,
            [Out] double* y);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes sine of vector elements
        // y[i] = sin(a[i])
        internal static extern void vzSin_64(long n,
            [In] void* a,
            [Out] void* y);

        #endregion
        #region ---- vdTan, vzTan ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes tangent of vector elements
        // y[i] = tan(a[i])
        internal static extern void vdTan_64(long n, 
            [In] IntPtr a, 
            [Out] IntPtr y);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes tangent of vector elements
        // y[i] = tan(a[i])
        internal static extern void vzTan_64(long n, 
            [In] IntPtr a, 
            [Out] IntPtr y);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes tangent of vector elements
        // y[i] = tan(a[i])
        internal static extern void vdTan_64(long n,
            [In] double* a,
            [Out] double* y);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes tangent of vector elements
        // y[i] = tan(a[i])
        internal static extern void vzTan_64(long n,
            [In] void* a,
            [Out] void* y);

        #endregion
        #region ---- vdCospi ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes cosine PI of vector elements
        // y[i] = cos(a[i]*PI)
        internal static extern void vdCospi_64(long n,
            [In] IntPtr a,
            [Out] IntPtr y);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes cosine PI of vector elements
        // y[i] = cos(a[i]*PI)
        internal static extern void vdCospi_64(long n,
            [In] double* a,
            [Out] double* y);

        #endregion
        #region ---- vdSinpi ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes sine PI of vector elements
        // y[i] = sin(a[i]*PI)
        internal static extern void vdSiinpi_64(long n,
            [In] IntPtr a,
            [Out] IntPtr y);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computessine PI of vector elements
        // y[i] = sin(a[i]*PI)
        internal static extern void vdSinpi_64(long n,
            [In] double* a,
            [Out] double* y);

        #endregion
        #region ---- vdTanpi ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes tangent PI of vector elements
        // y[i] = tan(a[i]*PI)
        internal static extern void vdTanpi_64(long n,
            [In] IntPtr a,
            [Out] IntPtr y);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes tangent PI of vector elements
        // y[i] = tan(a[i]*PI)
        internal static extern void vdTanpi_64(long n,
            [In] double* a,
            [Out] double* y);

        #endregion
        #region ---- vdCosd ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes cosine degree of vector elements
        // y[i] = cos(a[i]*PI/180)
        internal static extern void vdCosd_64(long n,
            [In] IntPtr a,
            [Out] IntPtr y);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes cosine degree of vector elements
        // y[i] = cos(a[i]*PI/180)
        internal static extern void vdCosd_64(long n,
            [In] double* a,
            [Out] double* y);

        #endregion
        #region ---- vdSind ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes sine degree of vector elements
        // y[i] = sin(a[i]*PI/180)
        internal static extern void vdSind_64(long n,
            [In] IntPtr a,
            [Out] IntPtr y);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes sine degree of vector elements
        // y[i] = sin(a[i]*PI/180)
        internal static extern void vdSind_64(long n,
            [In] double* a,
            [Out] double* y);

        #endregion
        #region ---- vdTand ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes tangent degree of vector elements
        // y[i] = tan(a[i]*PI/180)
        internal static extern void vdTand_64(long n,
            [In] IntPtr a,
            [Out] IntPtr y);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes tangent degree of vector elements
        // y[i] = tan(a[i]*PI/180)
        internal static extern void vdTand_64(long n,
            [In] double* a,
            [Out] double* y);

        #endregion
        #region ---- vdCosh, vzCosh ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes hyperbolic cosine of vector elements
        // y[i] = ch(a[i])
        internal static extern void vdCosh_64(long n, 
            [In] IntPtr a, 
            [Out] IntPtr y);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes hyperbolic cosine of vector elements
        // y[i] = ch(a[i])
        internal static extern void vzCosh_64(long n, 
            [In] IntPtr a, 
            [Out] IntPtr y);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes hyperbolic cosine of vector elements
        // y[i] = ch(a[i])
        internal static extern void vdCosh_64(long n,
            [In] double* a,
            [Out] double* y);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
             ExactSpelling = true, SetLastError = false)]
        // computes hyperbolic cosine of vector elements
        // y[i] = ch(a[i])
        internal static extern void vzCosh_64(long n,
            [In] void* a,
            [Out] void* y);

        #endregion
        #region ---- vdSinh, vzSinh ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes hyperbolic sine of vector elementes
        // y[i] = sh(a[i])
        internal static extern void vdSinh_64(long n, 
            [In] IntPtr a, 
            [Out] IntPtr y);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes hyperbolic sine of vector elementes
        // y[i] = sh(a[i])
        internal static extern void vzSinh_64(long n, 
            [In] IntPtr a, 
            [Out] IntPtr y);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes hyperbolic sine of vector elementes
        // y[i] = sh(a[i])
        internal static extern void vdSinh_64(long n,
            [In] double* a,
            [Out] double* y);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes hyperbolic sine of vector elementes
        // y[i] = sh(a[i])
        internal static extern void vzSinh_64(long n,
            [In] void* a,
            [Out] void* y);

        #endregion
        #region ---- vdTanh, vzTanh ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes hyperbolic tangent of vector elements
        // y[i] = th(a[i])
        internal static extern void vdTanh_64(long n, 
            [In] IntPtr a, 
            [Out] IntPtr y);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes hyperbolic tangent of vector elements
        // y[i] = th(a[i])
        internal static extern void vzTanh_64(long n, 
            [In] IntPtr a, 
            [Out] IntPtr y);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes hyperbolic tangent of vector elements
        // y[i] = th(a[i])
        internal static extern void vdTanh_64(long n,
            [In] double* a,
            [Out] double* y);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes hyperbolic tangent of vector elements
        // y[i] = th(a[i])
        internal static extern void vzTanh_64(long n,
            [In] void* a,
            [Out] void* y);

        #endregion
        #region ---- vdAcos, vzAcos ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes inverse cosine of vector elements
        // y[i] = arccos(a[i])
        internal static extern void vdAcos_64(long n, 
            [In] IntPtr a, 
            [Out] IntPtr y);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes inverse cosine of vector elements
        // y[i] = arccos(a[i])
        internal static extern void vzAcos_64(long n, 
            [In] IntPtr a, 
            [Out] IntPtr y);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes inverse cosine of vector elements
        // y[i] = arccos(a[i])
        internal static extern void vdAcos_64(long n,
            [In] double* a,
            [Out] double* y);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes inverse cosine of vector elements
        // y[i] = arccos(a[i])
        internal static extern void vzAcos_64(long n,
            [In] void* a,
            [Out] void* y);

        #endregion
        #region ---- vdAsin, vzAsin ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes inverse sine of vector elements
        // y[i] = arcsin(a[i])
        internal static extern void vdAsin_64(long n, 
            [In] IntPtr a, 
            [Out] IntPtr y);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes inverse sine of vector elements
        // y[i] = arcsin(a[i])
        internal static extern void vzAsin_64(long n, 
            [In] IntPtr a, 
            [Out] IntPtr y);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes inverse sine of vector elements
        // y[i] = arcsin(a[i])
        internal static extern void vdAsin_64(long n,
            [In] double* a,
            [Out] double* y);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes inverse sine of vector elements
        // y[i] = arcsin(a[i])
        internal static extern void vzAsin_64(long n,
            [In] void* a,
            [Out] void* y);

        #endregion
        #region ---- vdAtan, vzAtan ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes inverse tangent of vector elements
        // y[i] = arctan(a[i])
        internal static extern void vdAtan_64(long n, 
            [In] IntPtr a, 
            [Out] IntPtr y);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes inverse tangent of vector elements
        // y[i] = arctan(a[i])
        internal static extern void vzAtan_64(long n, 
            [In] IntPtr a, 
            [Out] IntPtr y);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes inverse tangent of vector elements
        // y[i] = arctan(a[i])
        internal static extern void vdAtan_64(long n,
            [In] double* a,
            [Out] double* y);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes inverse tangent of vector elements
        // y[i] = arctan(a[i])
        internal static extern void vzAtan_64(long n,
            [In] void* a,
            [Out] void* y);

        #endregion
        #region ---- vdAcospi ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes inverse cosine PI of vector elements
        // y[i] = arccos(a[i])/PI
        internal static extern void vdAcospi_64(long n,
            [In] IntPtr a,
            [Out] IntPtr y);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes inverse cosine PI of vector elements
        // y[i] = arccos(a[i])/PI
        internal static extern void vdAcospi_64(long n,
            [In] double* a,
            [Out] double* y);

        #endregion
        #region ---- vdAsinpi ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes inverse sine PI of vector elements
        // y[i] = arcsin(a[i])/PI
        internal static extern void vdAsinpi_64(long n,
            [In] IntPtr a,
            [Out] IntPtr y);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes inverse sine PI of vector elements
        // y[i] = arcsin(a[i])/PI
        internal static extern void vdAsinpi_64(long n,
            [In] double* a,
            [Out] double* y);

        #endregion
        #region ---- vdAtanpi ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes inverse tangent PI of vector elements
        // y[i] = arctan(a[i])/PI
        internal static extern void vdAtanpi_64(long n,
            [In] IntPtr a,
            [Out] IntPtr y);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes inverse tangent PI of vector elements
        // y[i] = arctan(a[i])/PI
        internal static extern void vdAtanpi_64(long n,
            [In] double* a,
            [Out] double* y);

        #endregion
        #region ---- vdAcosh, vzAcosh ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes inverse hyperbolic cosine (nonnegative) of vector elements
        // y[i] = arcch(a[i])
        internal static extern void vdAcosh_64(long n, 
            [In] IntPtr a, 
            [Out] IntPtr y);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes inverse hyperbolic cosine (nonnegative) of vector elements
        // y[i] = arcch(a[i])
        internal static extern void vzAcosh_64(long n, 
            [In] IntPtr a, 
            [Out] IntPtr y);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes inverse hyperbolic cosine (nonnegative) of vector elements
        // y[i] = arcch(a[i])
        internal static extern void vdAcosh_64(long n,
            [In] double* a,
            [Out] double* y);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes inverse hyperbolic cosine (nonnegative) of vector elements
        // y[i] = arcch(a[i])
        internal static extern void vzAcosh_64(long n,
            [In] void* a,
            [Out] void* y);

        #endregion
        #region ---- vdAsinh, vzAsinh ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes inverse hyperbolic sine of vector elements
        // y[i] = arcsh(a[i])
        internal static extern void vdAsinh_64(long n, 
            [In] IntPtr a, 
            [Out] IntPtr y);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes inverse hyperbolic sine of vector elements
        // y[i] = arcsh(a[i])
        internal static extern void vzAsinh_64(long n, 
            [In] IntPtr a, 
            [Out] IntPtr y);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes inverse hyperbolic sine of vector elements
        // y[i] = arcsh(a[i])
        internal static extern void vdAsinh_64(long n,
            [In] double* a,
            [Out] double* y);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes inverse hyperbolic sine of vector elements
        // y[i] = arcsh(a[i])
        internal static extern void vzAsinh_64(long n,
            [In] void* a,
            [Out] void* y);

        #endregion
        #region ---- vdAtanh, vzAtanh ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes inverse hyperbolic tangent of vector elements
        // y[i] = arcth(a[i])
        internal static extern void vdAtanh_64(long n, 
            [In] IntPtr a, 
            [Out] IntPtr y);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes inverse hyperbolic tangent of vector elements
        // y[i] = arcth(a[i])
        internal static extern void vzAtanh_64(long n, [In] IntPtr a, [Out] IntPtr y);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes inverse hyperbolic tangent of vector elements
        // y[i] = arcth(a[i])
        internal static extern void vdAtanh_64(long n,
            [In] double* a,
            [Out] double* y);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes inverse hyperbolic tangent of vector elements
        // y[i] = arcth(a[i])
        internal static extern void vzAtanh_64(long n,
            [In] void* a,
            [Out] void* y);

        #endregion
        #region ---- vdErf ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes error function of vector elements
        // y[i] = erf(a[i])
        internal static extern void vdErf_64(long n,
            [In] IntPtr a,
            [Out] IntPtr y);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes error function of vector elements
        // y[i] = erf(a[i])
        internal static extern void vdErf_64(long n,
            [In] double* a,
            [Out] double* y);

        #endregion
        #region ---- vdErfInv ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes inverse error function of vector elements
        // y[i] = erfinv(a[i])
        internal static extern void vdErfInv_64(long n,
            [In] IntPtr a,
            [Out] IntPtr y);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes inverse error function of vector elements
        // y[i] = erfinv(a[i])
        internal static extern void vdErfInv_64(long n,
            [In] double* a,
            [Out] double* y);

        #endregion
        #region ---- vdHypot ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes a square root of sum of two squared elements
        // y[i] = hypot(a[i],b[i])
        internal static extern void vdHypot_64(long n,
            [In] IntPtr a, [In] IntPtr b, [Out] IntPtr y);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes a square root of sum of two squared elements
        // y[i] = hypot(a[i],b[i])
        internal static extern void vdHypot_64(long n,
            [In] double* a, [In] double* b,
            [Out] double* y);

        #endregion
        #region ---- vdErfc ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes complementary error function of vector elements
        // y[i] = 1 - erf(a[i])
        internal static extern void vdErfc_64(long n,
            [In] IntPtr a,
            [Out] IntPtr y);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes complementary error function of vector elements
        // y[i] = 1 - erf(a[i])
        internal static extern void vdErfc_64(long n,
            [In] double* a,
            [Out] double* y);

        #endregion
        #region ---- vdErfcInv ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes inverse complementary error function of vector elements
        // y[i] = erfcinv(a[i])
        internal static extern void vdErfcInv_64(long n,
            [In] IntPtr a,
            [Out] IntPtr y);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes inverse complementary error function of vector elements
        // y[i] = erfcinv(a[i])
        internal static extern void vdErfcInv_64(long n,
            [In] double* a,
            [Out] double* y);

        #endregion
        #region ---- vdErfcx ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes scaled complementary error function of vector elements
        // y[i] = erfcx(a[i])
        internal static extern void vdErfcx_64(long n,
            [In] IntPtr a,
            [Out] IntPtr y);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes scaled complementary error function of vector elements
        // y[i] = erfcx(a[i])
        internal static extern void vdErfcx_64(long n,
            [In] double* a,
            [Out] double* y);

        #endregion
        #region ---- vdCdfNorm ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes cumulative normal distribution function of vector elements
        // y[i] = cdfnorm(a[i])
        internal static extern void vdCdfNorm_64(long n,
            [In] IntPtr a,
            [Out] IntPtr y);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes cumulative normal distribution function of vector elements
        // y[i] = cdfnorm(a[i])
        internal static extern void vdCdfNorm_64(long n,
            [In] double* a,
            [Out] double* y);

        #endregion
        #region ---- vdCdfNormInv ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes inverse cumulative normal distribution function of vector elements
        // y[i] = cdfnorminv(a[i])
        internal static extern void vdCdfNormInv_64(long n,
            [In] IntPtr a,
            [Out] IntPtr y);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes inverse cumulative normal distribution function of vector elements
        // y[i] = cdfnorminv(a[i])
        internal static extern void vdCdfNormInv_64(long n,
            [In] double* a,
            [Out] double* y);

        #endregion
        #region ---- vdLGamma ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes logarithm (base e) of the absolute value of gamma function
        // y[i] = lgamma(a[i])
        internal static extern void vdLGamma_64(long n,
            [In] IntPtr a,
            [Out] IntPtr y);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes logarithm (base e) of the absolute value of gamma function
        // y[i] = lgamma(a[i])
        internal static extern void vdLGamma_64(long n,
            [In] double* a,
            [Out] double* y);

        #endregion
        #region ---- vdTGamma ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes gamma function of vector elements
        // y[i] = tgamma(a[i])
        internal static extern void vdTGamma_64(long n,
            [In] IntPtr a,
            [Out] IntPtr y);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes gamma function of vector elements
        // y[i] = tgamma(a[i])
        internal static extern void vdTGamma_64(long n,
            [In] double* a,
            [Out] double* y);

        #endregion
        #region ---- vdI0 ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes I0 Bessel function function of vector elements
        // y[i] = i0(a[i])
        internal static extern void vdI0_64(long n,
            [In] IntPtr a,
            [Out] IntPtr y);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes I0 Bessel function function of vector elements
        // y[i] = i0(a[i])
        internal static extern void vdI0_64(long n,
            [In] double* a,
            [Out] double* y);

        #endregion
        #region ---- vdI1 ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes I1 Bessel function function of vector elements
        // y[i] = i1(a[i])
        internal static extern void vdI1_64(long n,
            [In] IntPtr a,
            [Out] IntPtr y);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes I1 Bessel function function of vector elements
        // y[i] = i1(a[i])
        internal static extern void vdI1_64(long n,
            [In] double* a,
            [Out] double* y);

        #endregion
        #region ---- vdJ0 ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes J0 Bessel function function of vector elements
        // y[i] = j0(a[i])
        internal static extern void vdJ0_64(long n,
            [In] IntPtr a,
            [Out] IntPtr y);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes J0 Bessel function function of vector elements
        // y[i] = j0(a[i])
        internal static extern void vdJ0_64(long n,
            [In] double* a,
            [Out] double* y);

        #endregion
        #region ---- vdJ1 ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes J1 Bessel function function of vector elements
        // y[i] = j1(a[i])
        internal static extern void vdJ1_64(long n,
            [In] IntPtr a,
            [Out] IntPtr y);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes J1 Bessel function function of vector elements
        // y[i] = j1(a[i])
        internal static extern void vdJ1_64(long n,
            [In] double* a,
            [Out] double* y);

        #endregion
        #region ---- vdY0 ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes Y0 Bessel function function of vector elements
        // y[i] = y0(a[i])
        internal static extern void vdY0_64(long n,
            [In] IntPtr a,
            [Out] IntPtr y);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes Y0 Bessel function function of vector elements
        // y[i] = y0(a[i])
        internal static extern void vdY0_64(long n,
            [In] double* a,
            [Out] double* y);

        #endregion
        #region ---- vdY1 ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes Y1 Bessel function function of vector elements
        // y[i] = y1(a[i])
        internal static extern void vdY1_64(long n,
            [In] IntPtr a,
            [Out] IntPtr y);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes Y1 Bessel function function of vector elements
        // y[i] = y1(a[i])
        internal static extern void vdY1_64(long n,
            [In] double* a,
            [Out] double* y);

        #endregion
        #region ---- vdJn ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes Jn Bessel function function of vector elements
        // y[i] = jn(a[i],b)
        internal static extern void vdJn_64(long n,
            [In] IntPtr a, double b,
            [Out] IntPtr y);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes Jn Bessel function function of vector elements
        // y[i] = jn(a[i],b)
        internal static extern void vdJn_64(long n,
            [In] double* a, double b,
            [Out] double* y);

        #endregion
        #region ---- vdYn ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes Yn Bessel function function of vector elements
        // y[i] = yn(a[i],b)
        internal static extern void vdYn_64(long n,
            [In] IntPtr a, double b,
            [Out] IntPtr y);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes Yn Bessel function function of vector elements
        // y[i] = yn(a[i],b)
        internal static extern void vdYn_64(long n,
            [In] double* a, double b,
            [Out] double* y);

        #endregion
        #region ---- vdAtan2 ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes four-quadrant inverse tangent of elements of two vectors
        // r[i] = arctan(a[i]/b[i])
        internal static extern void vdAtan2_64(long n,
            [In] IntPtr a, [In] IntPtr b, 
            [Out] IntPtr y);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes four-quadrant inverse tangent of elements of two vectors
        // r[i] = arctan(a[i]/b[i])
        internal static extern void vdAtan2_64(long n,
            [In] double* a, [In] double* b,
            [Out] double* y);

        #endregion
        #region ---- vdAtan2pi ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes four-quadrant inverse tangent devided by PI of elements of two vectors
        // r[i] = arctan(a[i]/b[i])/PI
        internal static extern void vdAtan2pi_64(long n,
            [In] IntPtr a, [In] IntPtr b,
            [Out] IntPtr y);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes four-quadrant inverse tangent devided by PI of elements of two vectors
        // r[i] = arctan(a[i]/b[i])/PI
        internal static extern void vdAtan2pi_64(long n,
            [In] double* a, [In] double* b,
            [Out] double* y);

        #endregion
        #region ---- vdMul, vzMul ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // performs element by element multiplication of vector a and vector b
        // y[i] = a[i] * b[i]
        internal static extern void vdMul_64(long n, 
            [In] IntPtr a, [In] IntPtr b,
            [Out] IntPtr y);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // performs element by element multiplication of vector a and vector b
        // y[i] = a[i] * b[i]
        internal static extern void vzMul_64(long n, 
            [In] IntPtr a, [In] IntPtr b,
            [Out] IntPtr y);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // performs element by element multiplication of vector a and vector b
        // y[i] = a[i] * b[i]
        internal static extern void vdMul_64(long n,
            [In] double* a, [In] double* b,
            [Out] double* y);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
             ExactSpelling = true, SetLastError = false)]
        // performs element by element multiplication of vector a and vector b
        // y[i] = a[i] * b[i]
        internal static extern void vzMul_64(long n,
            [In] void* a, [In] void* b,
            [Out] void* y);

        #endregion
        #region ---- vdDiv, vzDiv ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // performs element by element division of vector a by vector b
        // y[i] = a[i] / b[i]
        internal static extern void vdDiv_64(long n, 
            [In] IntPtr a, [In] IntPtr b,
            [Out] IntPtr y);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // performs element by element division of vector a by vector b
        // y[i] = a[i] / b[i]
        internal static extern void vzDiv_64(long n, [In] IntPtr a, [In] IntPtr b,
            [Out] IntPtr y);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // performs element by element division of vector a by vector b
        // y[i] = a[i] / b[i]
        internal static extern void vdDiv_64(long n,
            [In] double* a, [In] double* b,
            [Out] double* y);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // performs element by element division of vector a by vector b
        // y[i] = a[i] / b[i]
        internal static extern void vzDiv_64(long n,
            [In] void* a, [In] void* b,
            [Out] void* y);

        #endregion
        #region ---- vdPow, vzPow ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes a to the power b for elements of two vectors
        // y[i] = a[i]^b[i]
        internal static extern void vdPow_64(long n,
            [In] IntPtr a, [In] IntPtr b, 
            [Out] IntPtr y);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes a to the power b for elements of two vectors
        // y[i] = a[i]^b[i]
        internal static extern void vzPow_64(long n,
            [In] IntPtr a, [In] IntPtr b, 
            [Out] IntPtr y);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes a to the power b for elements of two vectors
        // y[i] = a[i]^b[i]
        internal static extern void vdPow_64(long n,
            [In] double* a, [In] double* b,
            [Out] double* y);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes a to the power b for elements of two vectors
        // y[i] = a[i]^b[i]
        internal static extern void vzPow_64(long n,
            [In] void* a, [In] void* b,
            [Out] void* y);

        #endregion
        #region ---- vdPow3o2 ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes the square root of the cube of each vector element
        // y[i] = a[i]^(3/2)
        internal static extern void vdPow3o2_64(long n, 
            [In] IntPtr a,
            [Out] IntPtr y);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes the square root of the cube of each vector element
        // y[i] = a[i]^(3/2)
        internal static extern void vdPow3o2_64(long n,
            [In] double* a,
            [Out] double* y);

        #endregion
        #region ---- vdPow2o3 ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes the cube root of the square of each vector element
        // y[i] = a[i]^(2/3)
        internal static extern void vdPow2o3_64(long n, 
            [In] IntPtr a,
            [Out] IntPtr y);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes the cube root of the square of each vector element
        // y[i] = a[i]^(2/3)
        internal static extern void vdPow2o3_64(long n,
            [In] double* a,
            [Out] double* y);

        #endregion
        #region ---- vdPowx, vzPowx ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // compute vector a to the scalar power b
        // y[i] = a[i]^b
        internal static extern void vdPowx_64(long n,
            [In] IntPtr a, double b, 
            [Out] IntPtr y);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // compute vector a to the scalar power b
        // y[i] = a[i]^b
        internal static extern void vzPowx_64(long n,
            [In] IntPtr a, ref Complex b, 
            [Out] IntPtr y);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // compute vector a to the scalar power b
        // y[i] = a[i]^b
        internal static extern void vdPowx_64(long n,
            [In] double* a, double b,
            [Out] double* y);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // compute vector a to the scalar power b
        // y[i] = a[i]^b
        internal static extern void vzPowx_64(long n,
            [In] void* a, void* b,
            [Out] void* y);

        #endregion
        #region ---- vdPowr ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes power function with a[i]>=0
        // y[i] = a[i]^b[i]
        internal static extern void vdPowr_64(long n,
            [In] IntPtr a, [In] IntPtr b,
            [Out] IntPtr y);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes power function with a[i]>=0
        // y[i] = a[i]^b[i]
        internal static extern void vdPowr_64(long n,
            [In] double* a, [In] double* b,
            [Out] double* y);

        #endregion
        #region ---- vdSinCos ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes sine and cosine of vector elements
        // y[i] = sin(a[i]), z[i]=cos(a[i])
        internal static extern void vdSinCos_64(long n, 
            [In] IntPtr a,
            [Out] IntPtr y, [Out] IntPtr z);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes sine and cosine of vector elements
        // y[i] = sin(a[i]), z[i]=cos(a[i])
        internal static extern void vdSinCos_64(long n,
            [In] double* a,
            [Out] double* y, [Out] double* z);

        #endregion
        #region ---- vdSinCospi ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes sine PI and cosine PI of vector elements
        // y[i] = sinpi(a[i]), z[i]=cospi(a[i])
        internal static extern void vdSinCospi_64(long n,
            [In] IntPtr a,
            [Out] IntPtr y, [Out] IntPtr z);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes sine PI and cosine PI of vector elements
        // y[i] = sinpi(a[i]), z[i]=cospi(a[i])
        internal static extern void vdSinCospi_64(long n,
            [In] double* a,
            [Out] double* y, [Out] double* z);

        #endregion
        #region ---- vdLinearFrac ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // performs linear fraction transformation of vectors a and b with scalar parameters
        // y[i] = (a[i]*scalea + shifta)/(b[i]*scaleb + shiftb)
        internal static extern void vdLinearFrac_64(long n,
            [In] IntPtr a, [In] IntPtr b,
            double scalea, double shifta, double scaleb, double shiftb,
            [Out] IntPtr y);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // performs linear fraction transformation of vectors a and b with scalar parameters
        // y[i] = (a[i]*scalea + shifta)/(b[i]*scaleb + shiftb)
        internal static extern void vdLinearFrac_64(long n,
            [In] double* a, [In] double* b,
            double scalea, double shifta, double scaleb, double shiftb,
            [Out] double* y);

        #endregion
        #region ---- vdCeil ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // performs integer value rounded towards plus infinity
        // y[i] = ceil(a[i])
        internal static extern void vdCeil_64(long n,
            [In] IntPtr a,
            [Out] IntPtr y);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // performs integer value rounded towards plus infinity
        // y[i] = ceil(a[i])
        internal static extern void vdCeil_64(long n,
            [In] double* a, 
            [Out] double* y);

        #endregion
        #region ---- vdFloor ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // performs integer value rounded towards minus infinity
        // y[i] = floor(a[i])
        internal static extern void vdFloor_64(long n,
            [In] IntPtr a,
            [Out] IntPtr y);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // performs integer value rounded towards minus infinity
        // y[i] = floor(a[i])
        internal static extern void vdFloor_64(long n,
            [In] double* a,
            [Out] double* y);

        #endregion
        #region ---- vdFrac ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes signed fraction part of vector elements
        // y[i] = a[i] - |a[i]|
        internal static extern void vdFrac_64(long n,
            [In] IntPtr a,
            [Out] IntPtr y);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes signed fraction part of vector elements
        // y[i] = a[i] - |a[i]|
        internal static extern void vdFrac_64(long n,
            [In] double* a,
            [Out] double* y);

        #endregion
        #region ---- vdModf ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes truncated integer value and the remaining fraction part of vector elements
        // y[i] = |a[i]|, z[i] = a[i] - |a[i]|
        internal static extern void vdModf_64(long n,
            [In] IntPtr a,
            [Out] IntPtr y, [Out] IntPtr z);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes truncated integer value and the remaining fraction part of vector elements
        // y[i] = |a[i]|, z[i] = a[i] - |a[i]|
        internal static extern void vdModf_64(long n,
            [In] double* a,
            [Out] double* y, [Out] double* z);

        #endregion
        #region ---- vdFmod ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes modulus function of vector elements
        // y[i] = fmod(a[i], b[i])
        internal static extern void vdFmod_64(long n,
            [In] IntPtr a, [In] IntPtr b,
            [Out] IntPtr y);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes modulus function of vector elements
        // y[i] = fmod(a[i], b[i])
        internal static extern void vdFmod_64(long n,
            [In] double* a, [In] double* b,
            [Out] double* y);

        #endregion
        #region ---- vdRemainder ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes remainder function of vector elements
        // y[i] = remainder(a[i], b[i])
        internal static extern void vdRemainder_64(long n,
            [In] IntPtr a, [In] IntPtr b,
            [Out] IntPtr y);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes remainder function of vector elements
        // y[i] = remainder(a[i], b[i])
        internal static extern void vdRemainder_64(long n,
            [In] double* a, [In] double* b,
            [Out] double* y);

        #endregion
        #region ---- vdNextAfter ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes next after function of vector elements
        // y[i] = nextafter(a[i], b[i])
        internal static extern void vdNextAfter_64(long n,
            [In] IntPtr a, [In] IntPtr b,
            [Out] IntPtr y);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes next after function of vector elements
        // y[i] = nextafter(a[i], b[i])
        internal static extern void vdNextAfter_64(long n,
            [In] double* a, [In] double* b,
            [Out] double* y);

        #endregion
        #region ---- vdCopySign ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes copy sign function of vector elements
        // y[i] = copysign(a[i], b[i])
        internal static extern void vdCopySign_64(long n,
            [In] IntPtr a, [In] IntPtr b,
            [Out] IntPtr y);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes copy sign function of vector elements
        // y[i] = copysign(a[i], b[i])
        internal static extern void vdCopySign_64(long n,
            [In] double* a, [In] double* b,
            [Out] double* y);

        #endregion
        #region ---- vdFdim ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes positive difference function of vector elements
        // y[i] = fdim(a[i], b[i])
        internal static extern void vdFdim_64(long n,
            [In] IntPtr a, [In] IntPtr b,
            [Out] IntPtr y);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes positive difference function of vector elements
        // y[i] = fdim(a[i], b[i])
        internal static extern void vdFdim_64(long n,
            [In] double* a, [In] double* b,
            [Out] double* y);

        #endregion
        #region ---- vdFmax ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes maximum function of vector elements
        // y[i] = fmax(a[i], b[i])
        internal static extern void vdFmax_64(long n,
            [In] IntPtr a, [In] IntPtr b,
            [Out] IntPtr y);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes maximum function of vector elements
        // y[i] = fmax(a[i], b[i])
        internal static extern void vdFmax_64(long n,
            [In] double* a, [In] double* b,
            [Out] double* y);

        #endregion
        #region ---- vdFmin ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes minimum function of vector elements
        // y[i] = fmin(a[i], b[i])
        internal static extern void vdFmin_64(long n,
            [In] IntPtr a, [In] IntPtr b,
            [Out] IntPtr y);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes minimum function of vector elements
        // y[i] = fmin(a[i], b[i])
        internal static extern void vdFmin_64(long n,
            [In] double* a, [In] double* b,
            [Out] double* y);

        #endregion
        #region ---- vdMaxMag ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes maximum magnitude function of vector elements
        // y[i] = maxmag(a[i], b[i])
        internal static extern void vdMaxMag_64(long n,
            [In] IntPtr a, [In] IntPtr b,
            [Out] IntPtr y);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes maximum magnitude function of vector elements
        // y[i] = maxmag(a[i], b[i])
        internal static extern void vdMaxMag_64(long n,
            [In] double* a, [In] double* b,
            [Out] double* y);

        #endregion
        #region ---- vdMinMag ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes minimum magnitude function of vector elements
        // y[i] = minmag(a[i], b[i])
        internal static extern void vdMinMag_64(long n,
            [In] IntPtr a, [In] IntPtr b,
            [Out] IntPtr y);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes minimum magnitude function of vector elements
        // y[i] = minmag(a[i], b[i])
        internal static extern void vdMinMag_64(long n,
            [In] double* a, [In] double* b,
            [Out] double* y);

        #endregion
        #region ---- vdRound ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes value rounded to the nearest integer 
        // y[i] = round(a[i])
        internal static extern void vdRound_64(long n,
            [In] IntPtr a,
            [Out] IntPtr y);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes value rounded to the nearest integer 
        // y[i] = round(a[i])
        internal static extern void vdRound_64(long n,
            [In] double* a,
            [Out] double* y);

        #endregion
        #region ---- vzConj ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // performs element by element conjugation of the vector
        // y[i] = conj(a[i])
        internal static extern void vzConj_64(long n, 
            [In] IntPtr a, 
            [Out] IntPtr y);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // performs element by element conjugation of the vector
        // y[i] = conj(a[i])
        internal static extern void vzConj_64(long n,
            [In] void* a,
            [Out] void* y);

        #endregion
        #region ---- vzMulByConj ----

        // => IntPtr ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // performs element by element multiplication of vector a element and 
        // conjugated vector b element
        // y[i] = mulbyconj(a[i],b[i])
        internal static extern void vzMulByConj_64(long n,
            [In] IntPtr a, [In] IntPtr b, 
            [Out] IntPtr y);


        // => Pointer ...
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // performs element by element multiplication of vector a element and 
        // conjugated vector b element
        // y[i] = mulbyconj(a[i],b[i])
        internal static extern void vzMulByConj_64(long n,
            [In] void* a, [In] void* b,
            [Out] void* y);

        #endregion

        // ...
        #region vdLinearFracI

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // performs linear fraction transformation of vectors a
        // and b with scalar parameters
        internal static extern void vdLinearFracI(long n,
            [In] IntPtr a, long inca, [In] IntPtr b, long incb,
            double scalea, double shifta, double scaleb, double shiftb,
            [Out] IntPtr y, long incy);





        [Obsolete]
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
         ExactSpelling = true, SetLastError = false)]
        // performs linear fraction transformation of vectors a
        // and b with scalar parameters
        internal static extern void vdLinearFracI(long n,
            [In] double* a, long inca, [In] double* b, long incb,
            double scalea, double shifta, double scaleb, double shiftb,
            [Out] double* y, long incy);

        #endregion

        #region ---- vdPackI ----

        // => IntPtr
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // copies elements of an array with specified indexing to a vector with unit increment.
        internal static extern void vdPackI(long n, 
            [In] IntPtr a, long inca,
            [Out] IntPtr y);


        // => Pointer
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // copies elements of an array with specified indexing to a vector with unit increment.
        internal static extern void vdPackI(long n, 
            [In] double* a, long inca,
            [Out] double* y);

        #endregion
        #region ---- vzPackI ----

        // => IntPtr
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // copies elements of an array with specified indexing to a vector with unit increment.
        internal static extern void vzPackI(long n, 
            [In] IntPtr a, long inca,
            [Out] IntPtr y);


        // => Pointer
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // copies elements of an array with specified indexing to a vector with unit increment.
        internal static extern void vzPackI(long n, 
            [In] void* a, long inca,
            [Out] void* y);

        #endregion
        #region ---- vdPackV ----

        // => IntPtr
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // copies elements of an array with specified indexing to a vector with unit increment.
        internal static extern void vdPackV(long n, 
            [In] IntPtr a, IntPtr ia,
            [Out] IntPtr y);


        // => Pointer
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // copies elements of an array with specified indexing to a vector with unit increment.
        internal static extern void vdPackV(long n,
            [In] double* a, long* ia,
            [Out] double* y);

        #endregion
        #region ---- vzPackV ----

        // => IntPtr
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // copies elements of an array with specified indexing to a vector with unit increment.
        internal static extern void vzPackV(long n, 
            [In] IntPtr a, IntPtr ia,
            [Out] IntPtr y);


        // => Pointer
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // copies elements of an array with specified indexing to a vector with unit increment.
        internal static extern void vzPackV(long n,
            [In] void* a, long* ia,
            [Out] void* y);

        #endregion
        #region ---- vdPackM ----

        // => IntPtr
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // copies elements of an array with specified indexing to a vector with unit increment.
        internal static extern void vdPackM(long n, 
            [In] IntPtr a, IntPtr ma, 
            [Out] IntPtr y);


        // => Pointer
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // copies elements of an array with specified indexing to a vector with unit increment.
        internal static extern void vdPackM(long n,
            [In] double* a, long* ma,
            [Out] double* y);

        #endregion
        #region ---- vzPackM ----

        // => IntPtr
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // copies elements of an array with specified indexing to a vector with unit increment.
        internal static extern void vzPackM(long n, 
            [In] IntPtr a, IntPtr ma,
            [Out] IntPtr y);


        // => Pointer
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,ExactSpelling = true, SetLastError = false)]
        // copies elements of an array with specified indexing to a vector with unit increment.
        internal static extern void vzPackM(long n,
            [In] void* a, long* ma,
            [Out] void* y);

        #endregion
        #region ---- vdUnpackI ----

        // => IntPtr 
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // copies elements of a vector with unit increment to an array with specified indexing.
        internal static extern void vdUnpackI(long n, 
            [In] IntPtr a, 
            [Out] IntPtr y, long incy);


        // => Pointer
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // copies elements of a vector with unit increment to an array with specified indexing.
        internal static extern void vdUnpackI(long n,
            [In] double* a,
            [Out] double* y, long incy);

        #endregion
        #region ---- vzUnpackI ----

        // => IntPtr
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // copies elements of a vector with unit increment to an array with specified indexing.
        internal static extern void vzUnpackI(long n, 
            [In] IntPtr a,
            [Out] IntPtr y, long incy);


        // => Pointer
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // copies elements of a vector with unit increment to an array with specified indexing.
        internal static extern void vzUnpackI(long n,
            [In] void* a,
            [Out] void* y, long incy);

        #endregion
        #region ---- vdUnpackV ----

        // => IntPtr
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // copies elements of a vector with unit increment to an array with specified indexing.
        internal static extern void vdUnpackV(long n, 
            [In] IntPtr a,
            [Out] IntPtr y, IntPtr iy);


        // => Pointer
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // copies elements of a vector with unit increment to an array with specified indexing.
        internal static extern void vdUnpackV(long n,
            [In] double* a,
            [Out] double* y, long* iy);

        #endregion
        #region ---- vzUnpackV ----

        // => IntPtr
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // copies elements of a vector with unit increment to an array with specified indexing.
        internal static extern void vzUnpackV(long n, 
            [In] IntPtr a,
            [Out] IntPtr y, IntPtr iy);


        // => Pointer
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // copies elements of a vector with unit increment to an array with specified indexing.
        internal static extern void vzUnpackV(long n,
            [In] void* a,
            [Out] void* y, long* iy);

        #endregion
        #region ---- vdUnpackM ----

        // => IntPtr
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // copies elements of a vector with unit increment to an array with specified indexing.
        internal static extern void vdUnpackM(long n, 
            [In] IntPtr a,
            [Out] IntPtr y, IntPtr my);


        // => Pointer
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,ExactSpelling = true, SetLastError = false)]
        // copies elements of a vector with unit increment to an array with specified indexing.
        internal static extern void vdUnpackM(long n,
            [In] double* a,
            [Out] double* y, long* my);

        #endregion
        #region ---- vzUnpackM ----

        // => IntPtr
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // copies elements of a vector with unit increment to an array with specified indexing.
        internal static extern void vzUnpackM(long n, 
            [In] IntPtr a,
            [Out] IntPtr y, IntPtr my);


        // => Pointer
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // copies elements of a vector with unit increment to an array with specified indexing.
        internal static extern void vzUnpackM(long n,
            [In] void* a,
            [Out] void* y, long* my);
        
        #endregion

        #endregion
        
        #region VSL

        #region ------------- Convolution -------------
        
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        // creates a new convolution task descriptor 
        // for multidimensional case
        internal static extern int vsldConvNewTask(ref IntPtr task,
            int mode, int dims, long[] xshape, long[] yshape, long[] zshape);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        // creates a new convolution task descriptor 
        // for multidimensional case
        internal static extern int vslzConvNewTask(ref IntPtr task,
            int mode, int dims, long[] xshape, long[] yshape, long[] zshape);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        // creates a new convolution task descriptor 
        // for one-dimensional case
        internal static extern int vsldConvNewTask1D(ref IntPtr task,
            int mode, long xshape, long yshape, long zshape);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        // creates a new convolution task descriptor 
        // for one-dimensional case
        internal static extern int vslzConvNewTask1D(ref IntPtr task,
            int mode, long xshape, long yshape, long zshape);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        // changes the value of the parameter mode 
        // in the convolution task descriptor
        internal static extern int vslConvSetMode(IntPtr task, int newmode);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        // changes the value of the parameter internal_precision 
        // in the convolution task descriptor
        internal static extern int vslConvSetInternalPrecision(IntPtr task, int precision);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        // changes the value of the parameter decimation 
        // in the convolution task descriptor
        internal static extern int vslConvSetStart(IntPtr task, long[] decimation);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        // changes the value of the parameter mode 
        // in the convolution task descriptor
        internal static extern int vslConvSetDecimation(IntPtr task, long[] newmode);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        // computes convolution for multidimensional case
        internal static extern int vsldConvExec(IntPtr task,
            double* x, long[] xstride, double* y, long[] ystride, 
            double* z, long[] zstride);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        // computes convolution for multidimensional case
        internal static extern int vslzConvExec(IntPtr task,
            Complex* x, long[] xstride, Complex* y, long[] ystride,
            Complex* z, long[] zstride);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        // computes convolution for one-dimensional case
        internal static extern int vsldConvExec1D(IntPtr task,
            double* x, long xstride, double* y, long ystride,
            double* z, long zstride);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        // computes convolution for one-dimensional case
        internal static extern int vslzConvExec1D(IntPtr task,
            Complex* x, long xstride, Complex* y, long ystride,
            Complex* z, long zstride);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        // destroys the task object and frees the memory
        internal static extern int vslConvDeleteTask(ref IntPtr task);

        #endregion
        #region ------------- Correlation -------------
        
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        // creates a new correlation task descriptor 
        // for multidimensional case
        internal static extern int vsldCorrNewTask(ref IntPtr task,
            int mode, int dims, long[] xshape, long[] yshape, long[] zshape);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        // creates a new correlation task descriptor 
        // for multidimensional case
        internal static extern int vslzCorrNewTask(ref IntPtr task,
            int mode, int dims, long[] xshape, long[] yshape, long[] zshape);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        // creates a new correlation task descriptor 
        // for one-dimensional case
        internal static extern int vsldCorrNewTask1D(ref IntPtr task,
            int mode, long xshape, long yshape, long zshape);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        // creates a new correlation task descriptor 
        // for one-dimensional case
        internal static extern int vslzCorrNewTask1D(ref IntPtr task,
            int mode, long xshape, long yshape, long zshape);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        // changes the value of the parameter mode 
        // in the correlation task descriptor
        internal static extern int vslCorrSetMode(IntPtr task, int newmode);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        // changes the value of the parameter internal_precision 
        // in the correlation task descriptor
        internal static extern int vslCorrSetInternalPrecision(IntPtr task, int precision);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        // changes the value of the parameter decimation 
        // in the correlation task descriptor
        internal static extern int vslCorrSetStart(IntPtr task, long[] start);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        // changes the value of the parameter mode 
        // in the correlation task descriptor
        internal static extern int vslCorrSetDecimation(IntPtr task, long[] decimation);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        // computes correlation for multidimensional case
        internal static extern int vsldCorrExec(IntPtr task,
            double* x, long[] xstride, double* y, long[] ystride,
            double* z, long[] zstride);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        // computes correlation for multidimensional case
        internal static extern int vslzCorrExec(IntPtr task,
            Complex* x, long[] xstride, Complex* y, long[] ystride,
            Complex* z, long[] zstride);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        // computes correlation for one-dimensional case
        internal static extern int vsldCorrExec1D(IntPtr task,
            double* x, long xstride, double* y, long ystride,
            double* z, long zstride);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        // computes correlation for one-dimensional case
        internal static extern int vslzCorrExec1D(IntPtr task,
            Complex* x, long xstride, Complex* y, long ystride,
            Complex* z, long zstride);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        // destroys the task object and frees the memory
        internal static extern int vslCorrDeleteTask(ref IntPtr task);

        #endregion
        #region ------------- Random -------------

        #region stream

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        // creates and initializes a random stream
        internal static extern int vslNewStream(ref IntPtr stream,
            int brng, ulong seed);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        // deletes a random stream
        internal static extern int vslDeleteStream(ref IntPtr stream);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        // creates a stream using the leapfrog method
        internal static extern int vslLeapfrogStream(IntPtr stream,
            long k, long nstreams);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        // initializes a stream using the block-splitting method
        internal static extern int vslSkipAheadStream(ref IntPtr stream,
            long nskip);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        // returns index of a basic generator used for generation 
        // of a given random stream
        internal static extern int vslGetStreamStateBrng(IntPtr stream);

        #endregion
        #region RNGs [continuous]

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        // generates random numbers with uniform distribution [continuous]
        internal static extern int vdRngUniform(int method, IntPtr stream,
            long n, double* r, double a, double b);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        // generates normally distributed random numbers [continuous]
        internal static extern int vdRngGaussian(int method, IntPtr stream,
            long n, double* r, double a, double sigma);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        // generates random numbers from multivariate normal distribution [continuous]
        internal static extern int vdRngGaussianMV(int method, IntPtr stream,
            long n, double* r, long dimen, long mstorage, double* a, double* t);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        // generates exponentially distributed random numbers [continuous]
        internal static extern int vdRngExponential(int method, IntPtr stream,
            long n, double* r, double a, double beta);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        // generates random numbers with laplace distribution [continuous]
        internal static extern int vdRngLaplace(int method, IntPtr stream,
            long n, double* r, double a, double beta);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        // generates Weibull distributed random numbers [continuous]
        internal static extern int vdRngWeibull(int method, IntPtr stream,
            long n, double* r, double alpha, double a, double b);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        // generates Cauchy distributed random numbers [continuous]
        internal static extern int vdRngCauchy(int method, IntPtr stream,
            long n, double* r, double a, double beta);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        // generates Rayleigh distributed random numbers [continuous]
        internal static extern int vdRngRayleigh(int method, IntPtr stream,
            long n, double* r, double a, double beta);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        // generates logmormally distributed random numbers [continuous]
        internal static extern int vdRngLognormal(int method, IntPtr stream,
            long n, double* r, double a, double sigma, double b, double beta);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        // generates Gumbel distributed random numbers [continuous]
        internal static extern int vdRngGumbel(int method, IntPtr stream,
            long n, double* r, double a, double beta);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        // generates gamma distributed random numbers [continuous]
        internal static extern int vdRngGamma(int method, IntPtr stream,
            long n, double* r, double alpha, double a, double beta);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        // generates beta distributed random numbers [continuous]
        internal static extern int vdRngBeta(int method, IntPtr stream,
            long n, double* r, double p, double q, double a, double beta);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        // generates chi-square distributed random numbers [continuous]
        internal static extern int vdRngChiSquare(int method, IntPtr stream,
            long n, double* r, long v);

        #endregion
        #region RNGs [discrete]
        
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        // generates random numbers with uniform distribution [discrete]
        internal static extern int viRngUniform(int method, IntPtr stream,
            long n, long* r, long a, long b);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        // generates bits of underlying BRNG integer recurrence [discrete]
        internal static extern int viRngUniformBits(int method, IntPtr stream,
            long n, ulong* r);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        // generates uniformly distributed bits in 32-bit chunks [discrete]
        internal static extern int viRngUniformBits32(int method, IntPtr stream,
            long n, ulong* r);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        // generates uniformly distributed bits in 64-bit chunks [discrete]
        internal static extern int viRngUniformBits64(int method, IntPtr stream,
            long n, ulong* r);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        // generates Bernoulli distributed random values [discrete]
        internal static extern int viRngBernoulli(int method, IntPtr stream,
            long n, long* r, double p);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        // generates geometrically distributed random values [discrete]
        internal static extern int viRngGeometric(int method, IntPtr stream,
            long n, long* r, double p);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        // generates binomially distributed random values [discrete]
        internal static extern int viRngBinomial(int method, IntPtr stream,
            long n, long* r, long ntrial, double p);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        // generates hypergeometrically distributed random values [discrete]
        internal static extern int viRngHypergeometric(int method, IntPtr stream,
            long n, long* r, long l, long s, long m);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        // generates Poisson distributed random values [discrete]
        internal static extern int viRngPoisson(int method, IntPtr stream,
            long n, long* r, double lambda);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        // generates Poisson distributed random values [discrete]
        // with varying mean 
        internal static extern int viRngPoissonV(int method, IntPtr stream,
            long n, long* r, double* lambda);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        // generates random values [discrete]
        // with negative binomial distribution
        internal static extern int viRngNegbinomial(int method, IntPtr stream,
            long n, long* r, double a, double p);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        // generates multinomially distributed random values [discrete]
        internal static extern int viRngMultinomial(int method, IntPtr stream,
            long n, long* r, long ntrial, long k, double* p);

        #endregion

        #endregion

        #endregion

        #region DFTI

        // DFTI native DftiCreateDescriptor declaration (1D version)
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        internal static extern int DftiCreateDescriptor(ref IntPtr desc,
            int precision, int domain, int dimention, int length);

        // DFTI native DftiCreateDescriptor declaration (1D version)
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        internal static extern int DftiCreateDescriptor(ref IntPtr desc,
            int precision, int domain, int dimention, long length);

        // DFTI native DftiCreateDescriptor declaration (2D version)
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        internal static extern int DftiCreateDescriptor(ref IntPtr desc,
            int precision, int domain, int dimention, int[] lengths);

        // DFTI native DftiCreateDescriptor declaration (2D version)
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        internal static extern int DftiCreateDescriptor(ref IntPtr desc,
            int precision, int domain, int dimention, long[] lengths);

        // DFTI native DftiCommitDescriptor declaration
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        internal static extern int DftiCommitDescriptor(IntPtr desc);

        // DFTI native DftiFreeDescriptor declaration
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        internal static extern int DftiFreeDescriptor(ref IntPtr desc);

        // DFTI native DftiSetValue declaration
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        internal static extern int DftiSetValue(IntPtr desc,
            int config_param, __arglist);

        // DFTI native DftiSetValue declaration
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
             ExactSpelling = true, SetLastError = false)]
        internal static extern int DftiSetValue(IntPtr desc,
            int config_param, int config_val);

        // DFTI native DftiSetValue declaration
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        internal static extern int DftiSetValue(IntPtr desc,
            int config_param, double config_val);

        // DFTI native DftiGetValue declaration
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        internal static extern int DftiGetValue(IntPtr desc,
            int config_param, ref double config_val);

        // DFTI native DftiComputeForward in-place declaration
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        internal static extern int DftiComputeForward(IntPtr desc,
            [In, Out] Complex[] x);

        // DFTI native DftiComputeForward in-place declaration
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        internal static extern int DftiComputeForward(IntPtr desc,
            [In, Out] Complex[,] x);

        // DFTI native DftiComputeForward in-place declaration
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        internal static extern int DftiComputeForward(IntPtr desc,
            [In, Out] Complex* data);

        // DFTI native DftiComputeForward out-of-place declaration
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        internal static extern int DftiComputeForward(IntPtr desc,
            [In] Complex[] x_in, [Out] Complex[] x_out);

        // DFTI native DftiComputeForward out-of-place declaration
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        internal static extern int DftiComputeForward(IntPtr desc,
            [In] Complex[,] x_in, [Out] Complex[,] x_out);

        // DFTI native DftiComputeForward out-of-place declaration
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        internal static extern int DftiComputeForward(IntPtr desc,
            [In] Complex* x_in, [Out] Complex* x_out);

        // DFTI native DftiComputeBackward in-place declaration
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        internal static extern int DftiComputeBackward(IntPtr desc,
            [In, Out] Complex[] data);

        // DFTI native DftiComputeBackward in-place declaration
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        internal static extern int DftiComputeBackward(IntPtr desc,
            [In, Out] Complex[,] data);

        // DFTI native DftiComputeBackward in-place declaration
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        internal static extern int DftiComputeBackward(IntPtr desc,
            [In, Out] Complex* px);

        // DFTI native DftiComputeBackward out-of-place declaration
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        internal static extern int DftiComputeBackward(IntPtr desc,
            [In] Complex[] x_in, [Out] Complex[] x_out);

        // DFTI native DftiComputeBackward out-of-place declaration
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        internal static extern int DftiComputeBackward(IntPtr desc,
            [In] Complex[,] x_in, [Out] Complex[,] x_out);

        // DFTI native DftiComputeBackward out-of-place declaration
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        internal static extern int DftiComputeBackward(IntPtr desc,
            [In] Complex* x_in, [Out] Complex* x_out);

        #endregion

    }

}