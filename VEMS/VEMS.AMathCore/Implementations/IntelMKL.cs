using System.Numerics;
using System.Runtime.InteropServices;

namespace VEMS.AMathCore
{

    /// <summary>
    /// Intel MKL class
    /// IBLAS, ILAPACK, IVMF, IFFT 
    /// </summary>
    public class IntelMKL // : ILAPACK, // IFFT, IVSL
    {
        #region properties

        ///// <summary>
        ///// get / set the InterfaceOption
        ///// </summary>
        //public MKL_Interface InterfaceOption
        //{
        //    set => SetInterfaceLayer(value);
        //}

        ///// <summary>
        ///// get / set the number of OpenMP threads to use
        ///// </summary>
        //public int MaxNumThreads
        //{
        //    get => IntelMKLNative.mkl_get_max_threads();
        //    set => IntelMKLNative.mkl_set_num_threads(ref value);
        //}

        ///// <summary>
        ///// get / set the flag whether to enable Intel MKL 
        ///// to dynamically change the number of OpenMP threads
        ///// </summary>
        //public bool EnableDynamicThreads
        //{
        //    get => Convert.ToBoolean(IntelMKLNative.mkl_get_dynamic());
        //    set
        //    {
        //        int opt = Convert.ToInt32(value);
        //        IntelMKLNative.mkl_set_dynamic(ref opt);
        //    }
        //}

        #endregion
        #region constructor

        /// <summary>
        /// constructs an IntelMKL class 
        /// with default options
        /// </summary>
        /// <param name="interfaceOption"> MKL interface option, default is ILP64 </param>
        /// <param name="enableDynamicThreads"> whether to enable dynamic threads, default is true </param>
        public IntelMKL(MKL_Interface interfaceOption = Defaults.mkl_Interface,
            bool enableDynamicThreads = true)
        {
            //InterfaceOption = interfaceOption;
            //EnableDynamicThreads = enableDynamicThreads;
            SetInterfaceLayer(interfaceOption);
            EnableDynamicThreads(enableDynamicThreads);
        }

        #endregion
        #region === static MKLSupport methods ===

        #region ---- version ----

        /// <summary>
        /// gets the version of the Intel MKL library
        /// </summary>
        /// <returns> version string </returns>
        public static string? GetVersion()
        {
            int len = 198;
            nint buffer = Marshal.AllocHGlobal(len);
            IntelMKLNative.mkl_get_version_string(buffer, len);
            string? version = Marshal.PtrToStringAnsi(buffer);
            Marshal.FreeHGlobal(buffer);
            return version;
        }

        #endregion
        #region ---- SDL control ----

        /// <summary>
        /// sets the interface layer for Intel MKL functions
        /// </summary>
        /// <param name="intf"> required interface </param>
        public static void SetInterfaceLayer(MKL_Interface intf)
        {
            int code = (int)intf;
            int res = IntelMKLNative.mkl_set_interface_layer(ref code);
            if (res == -1) { Printer.Error("Intel MKL Interface Setting Failed ..."); }
        }

        #endregion
        #region ---- threading ----

        /// <summary>
        /// returns the number of OpenMP threads available for 
        /// Intel MKL to use in internal parallel regions
        /// </summary>
        /// <returns> maximum number of threads </returns>
        public static int GetMaxThreads()
            => IntelMKLNative.mkl_get_max_threads();

        /// <summary>
        /// specifies the number of OpenMP threads to use
        /// </summary>
        /// <param name="nt"> number of threads </param>
        public static void SetMaxThreads(int nt)
            => IntelMKLNative.mkl_set_num_threads(ref nt);

        /// <summary>
        /// enables dynamic adjustment of the number of 
        /// OpenMP threads
        /// </summary>
        public static void EnableDynamicThreads(bool enable)
        {
            int code = Convert.ToInt32(enable);
            IntelMKLNative.mkl_set_dynamic(ref code);
        }

        #endregion
        #region ---- memory ----

        /// <summary>
        /// allocates an aligned memory buffer
        /// </summary>
        /// <param name="byteSize"> total size in byte to allocate </param>
        /// <param name="alignment"> alignment of buffer, default is 64 (bytes) </param>
        /// <returns> pointer to the allocated memory </returns>
        public static IntPtr Malloc(IntPtr byteSize, long alignment = 64)
            => IntelMKLNative.MKL_malloc(byteSize, alignment);

        /// <summary>
        /// allocates and initializes an aligned memory buffer
        /// </summary>
        /// <param name="num"> number of element </param>
        /// <param name="elementSize"> size of each element in byte </param>
        /// <param name="alignment"> alignment of buffer, default is 64 (bytes) </param>
        /// <returns> pointer to the allocated memory </returns>
        public static IntPtr Calloc(long num, long elementSize, long alignment = 64)
            => IntelMKLNative.MKL_calloc(num, elementSize, alignment);

        /// <summary>
        /// frees the aligned memory buffer allocated by mkl_malloc/mkl_alloc
        /// </summary>
        /// <param name="ptr"> pointer to the data in the memory </param>
        public static void Free(IntPtr ptr)
            => IntelMKLNative.MKL_free(ptr);

        /// <summary>
        /// frees unused memory allocated by the Intel MKL Memory Allocator
        /// </summary>
        internal static void FreeBuffers()
            => IntelMKLNative.mkl_free_buffers();

        /// <summary>
        /// frees unused memory allocated by the Intel MKL Memory Allocator
        /// in the current thread
        /// </summary>
        internal static void FreeThreadBuffers()
            => IntelMKLNative.mkl_thread_free_buffers();

        /// <summary>
        /// turns off the Intel Memory Allocator for Intel MKL functions to
        /// directly us the system malloc/free functions
        /// </summary>
        internal static void DisableFastMM()
            => IntelMKLNative.mkl_disable_fast_mm();

        #endregion
        #region ---- timing ----

        /// <summary>
        /// gets the current CPU frequency in GHz
        /// </summary>
        /// <returns> current CPU freqency in GHz </returns>
        public static double GetCpuFrequency()
            => IntelMKLNative.mkl_get_cpu_frequency();

        /// <summary>
        /// gets the maximum CPU frequency in GHz
        /// </summary>
        /// <returns> maximum CPU frequency in GHz </returns>
        public static double GetMaxCpuFrequency()
            => IntelMKLNative.mkl_get_max_cpu_frequency();

        #endregion
        #region ---- misc ----

        /// <summary>
        /// enables dispatching for new Intel® architectures or
        /// restricts the set of Intel® instruction sets available
        /// </summary>
        /// <param name="inst"> instruction option </param>
        public static void EnableInstructions(MKL_Instructions inst)
        {
            int isa = (int)inst;
            int res = IntelMKLNative.mkl_enable_instructions(isa);
            if (res == 0) { Printer.Error("Request to Enable MKL Instructions Rejected ..."); }
        }

        #endregion

        #endregion


        /// <summary>
        /// BLAS class
        /// </summary>
        public unsafe class BLAS : IBLAS
        {
            #region level 1

            #region ---- Asum [D/Z] ----

            /// <summary>
            /// Computes the sum of magnitudes of the elements in a double-precision array.
            /// dasum := |x[0]| + |x[1]| + ... + |x[n-1]|
            /// </summary>
            /// <param name="n">Number of elements in the array.</param>
            /// <param name="x">Pointer to the double-precision array.</param>
            /// <param name="incx">Increment for indexing x.</param>
            /// <returns>Sum of the magnitudes of the elements.</returns>
            public double Asum(long n, [In] double* x,
                long incx = 1)
                => IntelMKLNative.cblas_dasum_64(n, x, incx);

            /// <summary>
            /// Computes the sum of magnitudes of the elements in a complex double-precision array.
            /// dzasum := |Re(x[0])| + |Im(x[0])| + |Re(x[1])| + |Im(x[1])| + ... + |Re(x[n-1])| + |Im(x[n-1])|
            /// </summary>
            /// <param name="n">Number of elements in the array.</param>
            /// <param name="x">Pointer to the complex double-precision array (as void*).</param>
            /// <param name="incx">Increment for indexing x.</param>
            /// <returns>Sum of the magnitudes of the real and imaginary parts of the elements.</returns>
            public double Asum(long n, [In] void* x,
                long incx = 1)
                => IntelMKLNative.cblas_dzasum_64(n, x, incx);

            #endregion
            #region ---- Axpy [D/Z] ----

            /// <summary>
            /// Computes a scalar-array product and adds the result to another array (double precision).
            /// Performs the operation: y := a * x + y
            /// </summary>
            /// <param name="n">Number of array elements.</param>
            /// <param name="a">Scalar multiplier.</param>
            /// <param name="x">Pointer to the input array x.</param>
            /// <param name="y">Pointer to the input/output array y.</param>
            /// <param name="incx">Increment for indexing x (default is 1).</param>
            /// <param name="incy">Increment for indexing y (default is 1).</param>
            public void Axpy(long n, double a, [In] double* x,
                [In, Out] double* y,
                long incx = 1, long incy = 1)
                => IntelMKLNative.cblas_daxpy_64(n, a, x, incx, y, incy);

            /// <summary>
            /// Computes a scalar-array product and adds the result to another array (complex double precision).
            /// Performs the operation: y := a * x + y
            /// </summary>
            /// <param name="n">Number of array elements.</param>
            /// <param name="a">Pointer to the scalar multiplier (complex).</param>
            /// <param name="x">Pointer to the input array x (complex).</param>
            /// <param name="y">Pointer to the input/output array y (complex).</param>
            /// <param name="incx">Increment for indexing x (default is 1).</param>
            /// <param name="incy">Increment for indexing y (default is 1).</param>
            public void Axpy(long n, void* a, [In] void* x,
                [In, Out] void* y,
                long incx = 1, long incy = 1)
                => IntelMKLNative.cblas_zaxpy_64(n, a, x, incx, y, incy);

            #endregion
            #region ---- Copy [D/Z] ----

            /// <summary>
            /// Copies elements from the source array <paramref name="x"/> to the destination array <paramref name="y"/>.
            /// y := x
            /// </summary>
            /// <param name="n">Number of elements to copy.</param>
            /// <param name="x">Pointer to the source array.</param>
            /// <param name="y">Pointer to the destination array.</param>
            /// <param name="incx">Increment for the elements of <paramref name="x"/>. Default is 1.</param>
            /// <param name="incy">Increment for the elements of <paramref name="y"/>. Default is 1.</param>
            public void Copy(long n, [In] double* x,
                [Out] double* y,
                long incx = 1, long incy = 1)
                => IntelMKLNative.cblas_dcopy_64(n, x, incx, y, incy);

            /// <summary>
            /// Copies elements from the source array <paramref name="x"/> to the destination array <paramref name="y"/> (complex version).
            /// y := x
            /// </summary>
            /// <param name="n">Number of elements to copy.</param>
            /// <param name="x">Pointer to the source array (complex values).</param>
            /// <param name="y">Pointer to the destination array (complex values).</param>
            /// <param name="incx">Increment for the elements of <paramref name="x"/>. Default is 1.</param>
            /// <param name="incy">Increment for the elements of <paramref name="y"/>. Default is 1.</param>
            public void Copy(long n, [In] void* x,
                [Out] void* y,
                long incx = 1, long incy = 1)
                => IntelMKLNative.cblas_zcopy_64(n, x, incx, y, incy);

            //// temp ...
            ///// <summary>
            ///// copies x to y with pointers
            ///// y := x
            ///// </summary>
            ///// <param name="n"> number of elements </param>
            ///// <param name="x"> pointer to array x </param>
            ///// <param name="y"> pointer to array y </param>
            ///// <param name="incx"> increment for indexing x </param>
            ///// <param name="incy"> increment for indexing y </param>
            //public unsafe void Copy(long n, Complex* x, Complex* y,
            //    long incx = 1, long incy = 1)
            //    => IntelMKLNative.cblas_zcopy_64(n, x, incx, y, incy);

            #endregion
            #region ---- Dot [D/Z] ----

            /// <summary>
            /// Computes the vector-vector dot product for double-precision arrays.
            /// res = x[0]*y[0] + x[1]*y[1] +...+ x[n-1]*y[n-1]
            /// </summary>
            /// <param name="n">Number of elements in the vectors.</param>
            /// <param name="x">Pointer to the first input vector.</param>
            /// <param name="y">Pointer to the second input vector.</param>
            /// <param name="incx">Increment for indexing elements of <paramref name="x"/>. Default is 1.</param>
            /// <param name="incy">Increment for indexing elements of <paramref name="y"/>. Default is 1.</param>
            /// <returns>The result of the dot product of <paramref name="x"/> and <paramref name="y"/>.</returns>
            public double Dot(long n, [In] double* x, [In] double* y,
                long incx = 1, long incy = 1)
                => IntelMKLNative.cblas_ddot_64(n, x, incx, y, incy);

            /// <summary>
            /// Computes the unconjugated dot product of two complex double-precision vectors.
            /// res = x[0]*y[0] + x[1]*y[1] +...+ x[n-1]*y[n-1]
            /// </summary>
            /// <param name="n">Number of elements in the vectors.</param>
            /// <param name="x">Pointer to the first input vector.</param>
            /// <param name="y">Pointer to the second input vector.</param>
            /// <param name="dotu">Pointer to the result (output) complex value.</param>
            /// <param name="incx">Increment for indexing elements of <paramref name="x"/>. Default is 1.</param>
            /// <param name="incy">Increment for indexing elements of <paramref name="y"/>. Default is 1.</param>
            public void Dot(long n, [In] void* x, [In] void* y, void* dotu,
                long incx = 1, long incy = 1)
                => IntelMKLNative.cblas_zdotu_sub_64(n, x, incx, y, incy, dotu);

            #endregion
            #region ---- Dotc [Z] ----

            /// <summary>
            /// Computes the dot product of two complex vectors, conjugating the first vector.
            /// <para>dotc = conj(x[0])*y[0] + conj(x[1])*y[1] + ... + conj(x[n-1])*y[n-1]</para>
            /// </summary>
            /// <param name="n">Number of elements in the vectors.</param>
            /// <param name="x">Pointer to the first complex vector (to be conjugated).</param>
            /// <param name="y">Pointer to the second complex vector.</param>
            /// <param name="dotc">Pointer to the result (output).</param>
            /// <param name="incx">Increment for indexing <paramref name="x"/> (default is 1).</param>
            /// <param name="incy">Increment for indexing <paramref name="y"/> (default is 1).</param>
            public void Dotc(long n, [In] void* x, [In] void* y, void* dotc,
                long incx = 1, long incy = 1)
                => IntelMKLNative.cblas_zdotc_sub_64(n, x, incx, y, incy, dotc);

            #endregion
            #region ---- Nrm2 [D/Z] ----

            /// <summary>
            /// Computes the Euclidean norm (2-norm) of a vector of double-precision values.
            /// res = ||x||
            /// </summary>
            /// <param name="n">The number of elements in the vector <paramref name="x"/>.</param>
            /// <param name="x">Pointer to the first element of the vector.</param>
            /// <param name="incx">The increment for the elements of <paramref name="x"/>. Default is 1.</param>
            /// <returns>The Euclidean norm of the vector.</returns>
            public double Nrm2(long n, [In] double* x,
                long incx = 1)
                => IntelMKLNative.cblas_dnrm2_64(n, x, incx);

            /// <summary>
            /// Computes the Euclidean norm (2-norm) of a vector of complex double-precision values.
            /// res = ||x||
            /// </summary>
            /// <param name="n">The number of elements in the vector <paramref name="x"/>.</param>
            /// <param name="x">Pointer to the first element of the complex vector.</param>
            /// <param name="incx">The increment for the elements of <paramref name="x"/>. Default is 1.</param>
            /// <returns>The Euclidean norm of the complex vector.</returns>
            public double Nrm2(long n, [In] void* x,
                long incx = 1)
                => IntelMKLNative.cblas_dznrm2_64(n, x, incx);

            #endregion
            #region ---- Rot [D/Z] ----

            /// <summary>
            /// Performs rotation of points in the plane for double-precision arrays.
            /// xi = c * xi + s * yi
            /// yi = c * yi - s * xi
            /// </summary>
            /// <param name="n">Number of elements in the arrays.</param>
            /// <param name="x">Pointer to the first element of array x.</param>
            /// <param name="y">Pointer to the first element of array y.</param>
            /// <param name="c">Cosine of the rotation angle.</param>
            /// <param name="s">Sine of the rotation angle.</param>
            /// <param name="incx">Increment for indexing x (default is 1).</param>
            /// <param name="incy">Increment for indexing y (default is 1).</param>
            public void Rot(long n,
                [In, Out] double* x, [In, Out] double* y,
                double c, double s,
                long incx = 1, long incy = 1)
                => IntelMKLNative.cblas_drot_64(n, x, incx, y, incy, c, s);

            /// <summary>
            /// Performs rotation of points in the plane for complex arrays (applies real rotation to complex data).
            /// xi = c * xi + s * yi
            /// yi = c * yi - s * xi
            /// </summary>
            /// <param name="n">Number of elements in the arrays.</param>
            /// <param name="x">Pointer to the first element of array x (complex data).</param>
            /// <param name="y">Pointer to the first element of array y (complex data).</param>
            /// <param name="c">Cosine of the rotation angle.</param>
            /// <param name="s">Sine of the rotation angle.</param>
            /// <param name="incx">Increment for indexing x (default is 1).</param>
            /// <param name="incy">Increment for indexing y (default is 1).</param>
            public void Rot(long n,
                [In, Out] void* x, [In, Out] void* y,
                double c, double s,
                long incx = 1, long incy = 1)
                => IntelMKLNative.cblas_zdrot_64(n, x, incx, y, incy, c, s);

            #endregion
            #region ---- Scal [D/Z] ----

            /// <summary>
            /// Scales a vector by a scalar value (double precision).
            /// x = a * x
            /// </summary>
            /// <param name="n">Number of elements in the vector x.</param>
            /// <param name="a">Scalar multiplier.</param>
            /// <param name="x">Pointer to the vector to be scaled.</param>
            /// <param name="incx">Increment for the elements of x (default is 1).</param>
            public void Scal(long n, double a,
                [In, Out] double* x,
                long incx = 1)
                => IntelMKLNative.cblas_dscal_64(n, a, x, incx);

            /// <summary>
            /// Scales a complex vector by a real scalar value (double precision).
            /// x = a * x, where x is complex and a is real.
            /// </summary>
            /// <param name="n">Number of elements in the vector x.</param>
            /// <param name="a">Scalar multiplier (real).</param>
            /// <param name="x">Pointer to the complex vector to be scaled.</param>
            /// <param name="incx">Increment for the elements of x (default is 1).</param>
            public void Scal(long n, double a,
                [In, Out] void* x,
                long incx = 1)
                => IntelMKLNative.cblas_zdscal_64(n, a, x, incx);

            /// <summary>
            /// Scales a complex vector by a complex scalar value.
            /// x = a * x, where both a and x are complex.
            /// </summary>
            /// <param name="n">Number of elements in the vector x.</param>
            /// <param name="a">Pointer to the complex scalar multiplier.</param>
            /// <param name="x">Pointer to the complex vector to be scaled.</param>
            /// <param name="incx">Increment for the elements of x (default is 1).</param>
            public void Scal(long n, void* a,
                [In, Out] void* x,
                long incx = 1)
                => IntelMKLNative.cblas_zscal_64(n, a, x, incx);

            #endregion
            #region ---- Swap [D/Z] ----

            /// <summary>
            /// Swaps the elements of two double-precision arrays using pointers.
            /// </summary>
            /// <param name="n">Number of elements to swap.</param>
            /// <param name="x">Pointer to the first array.</param>
            /// <param name="y">Pointer to the second array.</param>
            /// <param name="incx">Increment for the elements of <paramref name="x"/>. Default is 1.</param>
            /// <param name="incy">Increment for the elements of <paramref name="y"/>. Default is 1.</param>
            public void Swap(long n,
                [In, Out] double* x, [In, Out] double* y,
                long incx = 1, long incy = 1)
                => IntelMKLNative.cblas_dswap_64(n, x, incx, y, incy);

            /// <summary>
            /// Swaps the elements of two complex arrays using pointer access.
            /// </summary>
            /// <param name="n">The number of elements to swap.</param>
            /// <param name="x">Pointer to the first array.</param>
            /// <param name="y">Pointer to the second array.</param>
            /// <param name="incx">Increment for indexing <paramref name="x"/> (default is 1).</param>
            /// <param name="incy">Increment for indexing <paramref name="y"/> (default is 1).</param>
            public void Swap(long n,
                [In, Out] void* x, [In, Out] void* y,
                long incx = 1, long incy = 1)
                => IntelMKLNative.cblas_zswap_64(n, x, incx, y, incy);

            #endregion
            #region ---- Iamax [D/Z] ----

            /// <summary>
            /// Finds the index of the element with the maximum absolute value in a double array.
            /// </summary>
            /// <param name="n">Number of elements in the array.</param>
            /// <param name="x">Pointer to the array of doubles.</param>
            /// <param name="incx">Increment for indexing x (default is 1).</param>
            /// <returns>Index of the element with the largest absolute value.</returns>
            public long Iamax(long n, [In] double* x,
                long incx = 1)
                => IntelMKLNative.cblas_idamax_64(n, x, incx);

            /// <summary>
            /// Finds the index of the element with the maximum absolute value in a complex array.
            /// </summary>
            /// <param name="n">Number of elements in the array.</param>
            /// <param name="x">Pointer to the array of complex numbers (as void*).</param>
            /// <param name="incx">Increment for indexing x (default is 1).</param>
            /// <returns>Index of the element with the largest absolute value.</returns>
            public long Iamax(long n, [In] void* x,
                long incx = 1)
                => IntelMKLNative.cblas_izamax_64(n, x, incx);

            #endregion
            #region ---- Iamin [D/Z] ----

            /// <summary>
            /// Finds the index of the element with the smallest absolute value in a double array.
            /// </summary>
            /// <param name="n">Number of elements in the array <paramref name="x"/>.</param>
            /// <param name="x">Pointer to the array of doubles.</param>
            /// <param name="incx">Increment for indexing <paramref name="x"/> (default is 1).</param>
            /// <returns>Index of the element with the smallest absolute value.</returns>
            public long Iamin(long n, [In] double* x,
                long incx = 1)
                => IntelMKLNative.cblas_idamin_64(n, x, incx);

            /// <summary>
            /// Finds the index of the element with the smallest absolute value in a complex array.
            /// </summary>
            /// <param name="n">Number of elements in the array <paramref name="x"/>.</param>
            /// <param name="x">Pointer to the array of complex numbers (as void*).</param>
            /// <param name="incx">Increment for indexing <paramref name="x"/> (default is 1).</param>
            /// <returns>Index of the element with the smallest absolute value.</returns>
            public long Iamin(long n, [In] void* x,
                long incx = 1)
                => IntelMKLNative.cblas_izamin_64(n, x, incx);

            #endregion

            #endregion
            #region level 2

            #region ---- Gemv [D/Z] ----

            /// <summary>
            /// Computes a matrix-vector product using a general matrix (double precision).
            /// y := alpha * op(a) * x + beta * y
            /// </summary>
            /// <param name="layout">Specifies array storage: row- or column-major.</param>
            /// <param name="trans">Specifies matrix a transpose operation.</param>
            /// <param name="m">Number of rows of the matrix a.</param>
            /// <param name="n">Number of columns of the matrix a.</param>
            /// <param name="alpha">Scalar alpha.</param>
            /// <param name="a">Pointer to array (matrix) a.</param>
            /// <param name="lda">Leading dimension of a.</param>
            /// <param name="x">Pointer to array (vector) x.</param>
            /// <param name="beta">Scalar beta.</param>
            /// <param name="y">Pointer to array (vector) y.</param>
            /// <param name="incx">Increment for the elements of x.</param>
            /// <param name="incy">Increment for the elements of y.</param>
            public void Gemv(BLAS_Layout layout, BLAS_Transpose trans,
                long m, long n, double alpha, [In] double* a, long lda,
                [In] double* x, double beta, [In, Out] double* y,
                long incx = 1, long incy = 1)
                => IntelMKLNative.cblas_dgemv_64(layout, trans, m, n,
                    alpha, a, lda, x, incx, beta, y, incy);

            /// <summary>
            /// Computes a matrix-vector product using a general matrix (complex precision).
            /// y := alpha * op(a) * x + beta * y
            /// </summary>
            /// <param name="layout">Specifies array storage: row- or column-major.</param>
            /// <param name="trans">Specifies matrix a transpose operation.</param>
            /// <param name="m">Number of rows of the matrix a.</param>
            /// <param name="n">Number of columns of the matrix a.</param>
            /// <param name="alpha">Pointer to scalar alpha (complex).</param>
            /// <param name="a">Pointer to array (matrix) a (complex).</param>
            /// <param name="lda">Leading dimension of a.</param>
            /// <param name="x">Pointer to array (vector) x (complex).</param>
            /// <param name="beta">Pointer to scalar beta (complex).</param>
            /// <param name="y">Pointer to array (vector) y (complex).</param>
            /// <param name="incx">Increment for the elements of x.</param>
            /// <param name="incy">Increment for the elements of y.</param>
            public void Gemv(BLAS_Layout layout, BLAS_Transpose trans,
                long m, long n, void* alpha, [In] void* a, long lda,
                [In] void* x, void* beta, [In, Out] void* y,
                long incx = 1, long incy = 1)
                => IntelMKLNative.cblas_zgemv_64(layout, trans, m, n,
                    alpha, a, lda, x, incx, beta, y, incy);

            #endregion
            #region ---- Gbmv [D/Z] ----

            /// <summary>
            /// Computes a matrix-vector product with a general band matrix (double precision).
            /// y := alpha * A * x + beta * y
            /// </summary>
            /// <param name="layout">Specifies array storage: row- or column-major.</param>
            /// <param name="trans">Specifies matrix A transpose operation.</param>
            /// <param name="m">Number of rows of the matrix A.</param>
            /// <param name="n">Number of columns of the matrix A.</param>
            /// <param name="kl">Number of sub-diagonals of the matrix A.</param>
            /// <param name="ku">Number of super-diagonals of the matrix A.</param>
            /// <param name="alpha">Scalar alpha.</param>
            /// <param name="a">Pointer to the band matrix A.</param>
            /// <param name="lda">Leading dimension of A.</param>
            /// <param name="x">Pointer to the vector x.</param>
            /// <param name="beta">Scalar beta.</param>
            /// <param name="y">Pointer to the vector y (result is stored here).</param>
            /// <param name="incx">Increment for the elements of x.</param>
            /// <param name="incy">Increment for the elements of y.</param>
            public void Gbmv(BLAS_Layout layout, BLAS_Transpose trans,
                long m, long n, long kl, long ku,
                double alpha, [In] double* a, long lda, [In] double* x,
                double beta, [In, Out] double* y,
                long incx = 1, long incy = 1)
                => IntelMKLNative.cblas_dgbmv_64(layout, trans,
                    m, n, kl, ku, alpha, a, lda, x, incx,
                    beta, y, incy);

            /// <summary>
            /// Computes a matrix-vector product with a general band matrix (complex double precision).
            /// y := alpha * A * x + beta * y
            /// </summary>
            /// <param name="layout">Specifies array storage: row- or column-major.</param>
            /// <param name="trans">Specifies matrix A transpose operation.</param>
            /// <param name="m">Number of rows of the matrix A.</param>
            /// <param name="n">Number of columns of the matrix A.</param>
            /// <param name="kl">Number of sub-diagonals of the matrix A.</param>
            /// <param name="ku">Number of super-diagonals of the matrix A.</param>
            /// <param name="alpha">Pointer to the scalar alpha (complex).</param>
            /// <param name="a">Pointer to the band matrix A (complex).</param>
            /// <param name="lda">Leading dimension of A.</param>
            /// <param name="x">Pointer to the vector x (complex).</param>
            /// <param name="beta">Pointer to the scalar beta (complex).</param>
            /// <param name="y">Pointer to the vector y (complex, result is stored here).</param>
            /// <param name="incx">Increment for the elements of x.</param>
            /// <param name="incy">Increment for the elements of y.</param>
            public void Gbmv(BLAS_Layout layout, BLAS_Transpose trans,
                long m, long n, long kl, long ku,
                void* alpha, [In] void* a, long lda, [In] void* x,
                void* beta, [In, Out] void* y,
                long incx = 1, long incy = 1)
                => IntelMKLNative.cblas_zgbmv_64(layout, trans,
                    m, n, kl, ku, alpha, a, lda, x, incx,
                    beta, y, incy);

            #endregion
            #region ---- Trmv [D/Z] ----

            /// <summary>
            /// Computes a matrix-vector product using a triangular matrix (double precision).
            /// x := op(A) * x
            /// </summary>
            /// <param name="layout">Specifies array storage: row- or column-major.</param>
            /// <param name="uplo">Specifies whether the matrix A is upper or lower triangular.</param>
            /// <param name="trans">Specifies matrix A transpose operation.</param>
            /// <param name="diag">Specifies whether the matrix A is unit triangular.</param>
            /// <param name="n">The order of the matrix A.</param>
            /// <param name="a">Pointer to the array (matrix) A.</param>
            /// <param name="lda">Leading dimension of A.</param>
            /// <param name="x">Pointer to the array (vector) x.</param>
            /// <param name="incx">Increment for the elements of x.</param>
            public void Trmv(BLAS_Layout layout, BLAS_Uplo uplo,
                BLAS_Transpose trans, BLAS_Diag diag,
                long n, [In] double* a, long lda,
                [In, Out] double* x, long incx = 1)
                => IntelMKLNative.cblas_dtrmv_64(layout, uplo,
                    trans, diag, n, a, lda, x, incx);

            /// <summary>
            /// Computes a matrix-vector product using a triangular matrix (complex precision).
            /// x := op(A) * x
            /// </summary>
            /// <param name="layout">Specifies array storage: row- or column-major.</param>
            /// <param name="uplo">Specifies whether the matrix A is upper or lower triangular.</param>
            /// <param name="trans">Specifies matrix A transpose operation.</param>
            /// <param name="diag">Specifies whether the matrix A is unit triangular.</param>
            /// <param name="n">The order of the matrix A.</param>
            /// <param name="a">Pointer to the array (matrix) A (complex type).</param>
            /// <param name="lda">Leading dimension of A.</param>
            /// <param name="x">Pointer to the array (vector) x (complex type).</param>
            /// <param name="incx">Increment for the elements of x.</param>
            public void Trmv(BLAS_Layout layout, BLAS_Uplo uplo,
                BLAS_Transpose trans, BLAS_Diag diag,
                long n, [In] void* a, long lda,
                [In, Out] void* x, long incx = 1)
                => IntelMKLNative.cblas_ztrmv_64(layout, uplo,
                    trans, diag, n, a, lda, x, incx);

            #endregion
            #region ---- Tbmv [D/Z] ----

            /// <summary>
            /// Computes a matrix-vector product using a triangular band matrix (double precision).
            /// x := op(A) * x
            /// </summary>
            /// <param name="layout">Specifies array storage: row- or column-major.</param>
            /// <param name="uplo">Specifies whether the matrix A is upper or lower triangular.</param>
            /// <param name="trans">Specifies matrix A transpose operation.</param>
            /// <param name="diag">Specifies whether the matrix A is unit triangular.</param>
            /// <param name="n">The order of the matrix A.</param>
            /// <param name="k">The number of super-diagonals of the matrix A.</param>
            /// <param name="a">Pointer to the matrix A.</param>
            /// <param name="lda">Leading dimension of A.</param>
            /// <param name="x">Pointer to the vector x (input/output).</param>
            /// <param name="incx">Increment for the elements of x.</param>
            public void Tbmv(BLAS_Layout layout, BLAS_Uplo uplo,
                BLAS_Transpose trans, BLAS_Diag diag,
                long n, long k, [In] double* a, long lda,
                [In, Out] double* x, long incx = 1)
                => IntelMKLNative.cblas_dtbmv_64(layout, uplo,
                    trans, diag, n, k, a, lda, x, incx);

            /// <summary>
            /// Computes a matrix-vector product using a triangular band matrix (complex double precision).
            /// x := op(A) * x
            /// </summary>
            /// <param name="layout">Specifies array storage: row- or column-major.</param>
            /// <param name="uplo">Specifies whether the matrix A is upper or lower triangular.</param>
            /// <param name="trans">Specifies matrix A transpose operation.</param>
            /// <param name="diag">Specifies whether the matrix A is unit triangular.</param>
            /// <param name="n">The order of the matrix A.</param>
            /// <param name="k">The number of super-diagonals of the matrix A.</param>
            /// <param name="a">Pointer to the matrix A (complex values).</param>
            /// <param name="lda">Leading dimension of A.</param>
            /// <param name="x">Pointer to the vector x (complex, input/output).</param>
            /// <param name="incx">Increment for the elements of x.</param>
            public void Tbmv(BLAS_Layout layout, BLAS_Uplo uplo,
                BLAS_Transpose trans, BLAS_Diag diag,
                long n, long k, [In] void* a, long lda,
                [In, Out] void* x, long incx = 1)
                => IntelMKLNative.cblas_ztbmv_64(layout, uplo,
                    trans, diag, n, k, a, lda, x, incx);

            #endregion
            #region ---- Tpmv [D/Z] ----

            /// <summary>
            /// Computes a matrix-vector product using a triangular packed matrix for double-precision values.
            /// x := op(A) * x,
            /// </summary>
            /// <param name="layout">Specifies array storage: row- or column-major.</param>
            /// <param name="uplo">Specifies whether the matrix A is upper or lower triangular.</param>
            /// <param name="trans">Specifies matrix A transpose operation.</param>
            /// <param name="diag">Specifies whether the matrix A is unit triangular.</param>
            /// <param name="n">The order of the matrix A.</param>
            /// <param name="k">The number of super-diagonals of the matrix A.</param>
            /// <param name="a">Pointer to the packed matrix A.</param>
            /// <param name="x">Pointer to the vector x (overwritten on exit).</param>
            /// <param name="incx">Increment for the elements of x.</param>
            public void Tpmv(BLAS_Layout layout, BLAS_Uplo uplo,
                BLAS_Transpose trans, BLAS_Diag diag,
                long n, long k, [In] double* a,
                [In, Out] double* x, long incx = 1)
                => IntelMKLNative.cblas_dtpmv_64(layout, uplo,
                    trans, diag, n, k, a, x, incx);

            /// <summary>
            /// Computes a matrix-vector product using a triangular packed matrix for complex values.
            /// x := op(A) * x,
            /// </summary>
            /// <param name="layout">Specifies array storage: row- or column-major.</param>
            /// <param name="uplo">Specifies whether the matrix A is upper or lower triangular.</param>
            /// <param name="trans">Specifies matrix A transpose operation.</param>
            /// <param name="diag">Specifies whether the matrix A is unit triangular.</param>
            /// <param name="n">The order of the matrix A.</param>
            /// <param name="k">The number of super-diagonals of the matrix A.</param>
            /// <param name="a">Pointer to the packed matrix A (complex values).</param>
            /// <param name="x">Pointer to the vector x (complex values, overwritten on exit).</param>
            /// <param name="incx">Increment for the elements of x.</param>
            public void Tpmv(BLAS_Layout layout, BLAS_Uplo uplo,
                BLAS_Transpose trans, BLAS_Diag diag,
                long n, long k, [In] void* a,
                [In, Out] void* x, long incx = 1)
                => IntelMKLNative.cblas_ztpmv_64(layout, uplo,
                    trans, diag, n, k, a, x, incx);

            #endregion
            #region ---- Trsv [D/Z] ----

            /// <summary>
            /// Solves a system of linear equations whose coefficients are in a triangular matrix.
            /// Solves op(A) * x = b for x, where A is a triangular matrix.
            /// </summary>
            /// <param name="layout">Specifies array storage: row- or column-major.</param>
            /// <param name="uplo">Specifies whether the matrix A is upper or lower triangular.</param>
            /// <param name="trans">Specifies matrix A transpose operation.</param>
            /// <param name="diag">Specifies whether the matrix A is unit triangular.</param>
            /// <param name="n">The order of the matrix A.</param>
            /// <param name="a">Pointer to the array (matrix) A.</param>
            /// <param name="lda">Leading dimension of A.</param>
            /// <param name="x">Pointer to the array (vector) x (right-hand side, overwritten by the solution).</param>
            /// <param name="incx">Increment for the elements of x.</param>
            public void Trsv(BLAS_Layout layout, BLAS_Uplo uplo,
                BLAS_Transpose trans, BLAS_Diag diag,
                long n, [In] double* a, long lda,
                [In, Out] double* x, long incx = 1)
                => IntelMKLNative.cblas_dtrsv_64(layout, uplo,
                    trans, diag, n, a, lda, x, incx);

            /// <summary>
            /// Solves a system of linear equations whose coefficients are in a triangular matrix (complex version).
            /// Solves op(A) * x = b for x, where A is a triangular matrix.
            /// </summary>
            /// <param name="layout">Specifies array storage: row- or column-major.</param>
            /// <param name="uplo">Specifies whether the matrix A is upper or lower triangular.</param>
            /// <param name="trans">Specifies matrix A transpose operation.</param>
            /// <param name="diag">Specifies whether the matrix A is unit triangular.</param>
            /// <param name="n">The order of the matrix A.</param>
            /// <param name="a">Pointer to the array (matrix) A (complex values).</param>
            /// <param name="lda">Leading dimension of A.</param>
            /// <param name="x">Pointer to the array (vector) x (right-hand side, overwritten by the solution).</param>
            /// <param name="incx">Increment for the elements of x.</param>
            public void Trsv(BLAS_Layout layout, BLAS_Uplo uplo,
                BLAS_Transpose trans, BLAS_Diag diag,
                long n, [In] void* a, long lda,
                [In, Out] void* x, long incx = 1)
                => IntelMKLNative.cblas_ztrsv_64(layout, uplo,
                    trans, diag, n, a, lda, x, incx);

            #endregion
            #region ---- Tbsv [D/Z] ----

            /// <summary>
            /// Solves a system of linear equations whose coefficients are in a triangular band matrix.
            /// The operation performed is op(A) * x = b, where op(A) is determined by <paramref name="trans"/>.
            /// </summary>
            /// <param name="layout">Specifies array storage: row- or column-major.</param>
            /// <param name="uplo">Specifies whether the matrix A is upper or lower triangular.</param>
            /// <param name="trans">Specifies the matrix A transpose operation.</param>
            /// <param name="diag">Specifies whether the matrix A is unit triangular.</param>
            /// <param name="n">The order of the matrix A.</param>
            /// <param name="k">The number of superdiagonals of the matrix A.</param>
            /// <param name="a">Pointer to the array containing the matrix A.</param>
            /// <param name="lda">Leading dimension of A.</param>
            /// <param name="x">Pointer to the array containing the vector x (right-hand side and solution).</param>
            /// <param name="incx">Increment for the elements of x.</param>
            public void Tbsv(BLAS_Layout layout, BLAS_Uplo uplo,
                BLAS_Transpose trans, BLAS_Diag diag,
                long n, long k, [In] double* a, long lda,
                [In, Out] double* x, long incx = 1)
                => IntelMKLNative.cblas_dtbsv_64(layout, uplo,
                    trans, diag, n, k, a, lda, x, incx);

            /// <summary>
            /// Solves a system of linear equations whose coefficients are in a complex triangular band matrix.
            /// The operation performed is op(A) * x = b, where op(A) is determined by <paramref name="trans"/>.
            /// </summary>
            /// <param name="layout">Specifies array storage: row- or column-major.</param>
            /// <param name="uplo">Specifies whether the matrix A is upper or lower triangular.</param>
            /// <param name="trans">Specifies the matrix A transpose operation.</param>
            /// <param name="diag">Specifies whether the matrix A is unit triangular.</param>
            /// <param name="n">The order of the matrix A.</param>
            /// <param name="k">The number of superdiagonals of the matrix A.</param>
            /// <param name="a">Pointer to the array containing the complex matrix A.</param>
            /// <param name="lda">Leading dimension of A.</param>
            /// <param name="x">Pointer to the array containing the complex vector x (right-hand side and solution).</param>
            /// <param name="incx">Increment for the elements of x.</param>
            public void Tbsv(BLAS_Layout layout, BLAS_Uplo uplo,
                BLAS_Transpose trans, BLAS_Diag diag,
                long n, long k, [In] void* a, long lda,
                [In, Out] void* x, long incx = 1)
                => IntelMKLNative.cblas_ztbsv_64(layout, uplo,
                    trans, diag, n, k, a, lda, x, incx);

            #endregion
            #region ---- Tpsv [D/Z] ----

            /// <summary>
            /// Solves a system of linear equations whose coefficients
            /// are in a triangular packed matrix <c>op(A) * x = b</c> for double-precision values.
            /// </summary>
            /// <param name="layout">Specifies array storage: row- or column-major.</param>
            /// <param name="uplo">Specifies whether the matrix A is upper or lower triangular.</param>
            /// <param name="trans">Specifies matrix A transpose operation.</param>
            /// <param name="diag">Specifies whether the matrix A is unit triangular.</param>
            /// <param name="n">The order of the matrix A.</param>
            /// <param name="a">Pointer to the packed triangular matrix A.</param>
            /// <param name="x">Pointer to the vector x (right-hand side, overwritten by the solution).</param>
            /// <param name="incx">Increment for the elements of x.</param>
            public void Tpsv(BLAS_Layout layout, BLAS_Uplo uplo,
                BLAS_Transpose trans, BLAS_Diag diag,
                long n, [In] double* a,
                [In, Out] double* x, long incx = 1)
                => IntelMKLNative.cblas_dtpsv_64(layout, uplo,
                    trans, diag, n, a, x, incx);

            /// <summary>
            /// Solves a system of linear equations whose coefficients
            /// are in a triangular packed matrix <c>op(A) * x = b</c> for complex values.
            /// </summary>
            /// <param name="layout">Specifies array storage: row- or column-major.</param>
            /// <param name="uplo">Specifies whether the matrix A is upper or lower triangular.</param>
            /// <param name="trans">Specifies matrix A transpose operation.</param>
            /// <param name="diag">Specifies whether the matrix A is unit triangular.</param>
            /// <param name="n">The order of the matrix A.</param>
            /// <param name="a">Pointer to the packed triangular matrix A (complex values).</param>
            /// <param name="x">Pointer to the vector x (right-hand side, overwritten by the solution, complex values).</param>
            /// <param name="incx">Increment for the elements of x.</param>
            public void Tpsv(BLAS_Layout layout, BLAS_Uplo uplo,
                BLAS_Transpose trans, BLAS_Diag diag,
                long n, [In] void* a,
                [In, Out] void* x, long incx = 1)
                => IntelMKLNative.cblas_ztpsv_64(layout, uplo,
                    trans, diag, n, a, x, incx);

            #endregion
            #region ---- Symv [D] ----

            /// <summary>
            /// Computes a matrix-vector product for a symmetric matrix using double-precision values.
            /// Performs the operation y := alpha * A * x + beta * y, where A is a symmetric matrix.
            /// </summary>
            /// <param name="layout">Specifies array storage: row- or column-major.</param>
            /// <param name="uplo">Specifies whether the matrix A is upper or lower triangular.</param>
            /// <param name="n">The order of the matrix A.</param>
            /// <param name="alpha">Scalar multiplier for A * x.</param>
            /// <param name="a">Pointer to the symmetric matrix A.</param>
            /// <param name="lda">Leading dimension of A.</param>
            /// <param name="x">Pointer to the input vector x.</param>
            /// <param name="beta">Scalar multiplier for y.</param>
            /// <param name="y">Pointer to the output vector y (overwritten on exit).</param>
            /// <param name="incx">Increment for the elements of x.</param>
            /// <param name="incy">Increment for the elements of y.</param>
            public void Symv(BLAS_Layout layout, BLAS_Uplo uplo,
                long n, double alpha, [In] double* a, long lda,
                [In] double* x, double beta, [In, Out] double* y,
                long incx = 1, long incy = 1)
                => IntelMKLNative.cblas_dsymv_64(layout, uplo,
                    n, alpha, a, lda, x, incx, beta, y, incy);

            #endregion
            #region ---- Sbmv [D] ----

            /// <summary>
            /// Computes a matrix-vector product for a symmetric band matrix.
            /// Performs the operation y := alpha * A * x + beta * y, where A is a symmetric band matrix.
            /// </summary>
            /// <param name="layout">Specifies array storage: row- or column-major.</param>
            /// <param name="uplo">Specifies whether the matrix A is upper or lower triangular.</param>
            /// <param name="n">The order of the matrix A.</param>
            /// <param name="k">The number of super-diagonals of the matrix A.</param>
            /// <param name="alpha">Scalar alpha.</param>
            /// <param name="a">Pointer to the array containing the symmetric band matrix A.</param>
            /// <param name="lda">Leading dimension of A.</param>
            /// <param name="x">Pointer to the array containing the vector x.</param>
            /// <param name="beta">Scalar beta.</param>
            /// <param name="y">Pointer to the array containing the vector y (result is stored here).</param>
            /// <param name="incx">Increment for the elements of x.</param>
            /// <param name="incy">Increment for the elements of y.</param>
            public void Sbmv(BLAS_Layout layout, BLAS_Uplo uplo,
                long n, long k, double alpha, [In] double* a, long lda,
                [In] double* x, double beta, [In, Out] double* y,
                long incx = 1, long incy = 1)
                => IntelMKLNative.cblas_dsbmv_64(layout, uplo,
                    n, k, alpha, a, lda, x, incx, beta, y, incy);

            #endregion
            #region ---- Spmv [D] ----

            /// <summary>
            /// Computes a matrix-vector product for a symmetric packed matrix.
            /// Performs the operation y := alpha * A * x + beta * y, where A is a symmetric packed matrix.
            /// </summary>
            /// <param name="layout">Specifies array storage: row- or column-major.</param>
            /// <param name="uplo">Specifies whether the matrix A is upper or lower triangular.</param>
            /// <param name="n">The order of the matrix A.</param>
            /// <param name="alpha">Scalar alpha.</param>
            /// <param name="a">Pointer to the packed symmetric matrix A.</param>
            /// <param name="lda">Leading dimension of A.</param>
            /// <param name="x">Pointer to the input vector x.</param>
            /// <param name="beta">Scalar beta.</param>
            /// <param name="y">Pointer to the output vector y (overwritten on exit).</param>
            /// <param name="incx">Increment for the elements of x.</param>
            /// <param name="incy">Increment for the elements of y.</param>
            public void Spmv(BLAS_Layout layout, BLAS_Uplo uplo,
                long n, double alpha, [In] double* a, long lda,
                [In] double* x, double beta, [In, Out] double* y,
                long incx = 1, long incy = 1)
                => IntelMKLNative.cblas_dspmv_64(layout, uplo,
                    n, alpha, a, lda, x, incx, beta, y, incy);

            #endregion
            #region ---- Ger [D] ----

            /// <summary>
            /// Performs a rank-1 update of a general matrix using double-precision values.
            /// Computes A := alpha * x * y' + A, where A is an m-by-n matrix, x is a vector of length m, and y is a vector of length n.
            /// </summary>
            /// <param name="layout">Specifies array storage: row- or column-major.</param>
            /// <param name="m">Number of rows of the matrix A.</param>
            /// <param name="n">Number of columns of the matrix A.</param>
            /// <param name="alpha">Scalar multiplier for the rank-1 update.</param>
            /// <param name="x">Pointer to the vector x (length m).</param>
            /// <param name="y">Pointer to the vector y (length n).</param>
            /// <param name="a">Pointer to the matrix A (size m-by-n).</param>
            /// <param name="lda">Leading dimension of A.</param>
            /// <param name="incx">Increment for the elements of x. Default is 1.</param>
            /// <param name="incy">Increment for the elements of y. Default is 1.</param>
            public void Ger(BLAS_Layout layout, long m, long n,
                double alpha, [In] double* x, [In] double* y,
                [In, Out] double* a, long lda,
                long incx = 1, long incy = 1)
                => IntelMKLNative.cblas_dger_64(layout, m, n, alpha,
                    x, incx, y, incy, a, lda);

            #endregion
            #region ---- Syr [D] ----

            /// <summary>
            /// Performs a rank-1 update of a symmetric matrix using double-precision values.
            /// Computes A := alpha * x * x' + A, where A is a symmetric matrix.
            /// </summary>
            /// <param name="layout">Specifies array storage: row- or column-major.</param>
            /// <param name="uplo">Specifies whether the matrix A is upper or lower triangular.</param>
            /// <param name="n">The order of the matrix A.</param>
            /// <param name="alpha">Scalar alpha.</param>
            /// <param name="x">Pointer to the vector x.</param>
            /// <param name="a">Pointer to the symmetric matrix A (overwritten on exit).</param>
            /// <param name="lda">Leading dimension of A.</param>
            /// <param name="incx">Increment for the elements of x (default is 1).</param>
            public void Syr(BLAS_Layout layout, BLAS_Uplo uplo,
                long n, double alpha, [In] double* x,
                [In, Out] double* a, long lda,
                long incx = 1)
                => IntelMKLNative.cblas_dsyr_64(layout, uplo, n, alpha,
                    x, incx, a, lda);

            #endregion
            #region ---- Spr [D] ----

            /// <summary>
            /// Performs a symmetric rank-1 update of a symmetric packed matrix.
            /// A := alpha * x * x' + A
            /// </summary>
            /// <param name="layout">Specifies array storage: row- or column-major.</param>
            /// <param name="uplo">Specifies whether the matrix a is upper or lower triangular.</param>
            /// <param name="n">Number of columns of the matrix a.</param>
            /// <param name="alpha">Scalar alpha.</param>
            /// <param name="x">Pointer to array (vector) x.</param>
            /// <param name="a">Pointer to array (matrix) a.</param>
            /// <param name="incx">Increment for the elements of x. Default is 1.</param>
            public void Spr(BLAS_Layout layout, BLAS_Uplo uplo,
                long n, double alpha, [In] double* x,
                [In, Out] double* a,
                long incx = 1)
                => IntelMKLNative.cblas_dspr_64(layout, uplo, n, alpha,
                    x, incx, a);

            #endregion
            #region ---- Syr2 [D] ----

            /// <summary>
            /// Performs a rank-2 update of a symmetric matrix.
            /// Computes A := alpha * x * y' + alpha * y * x' + A,
            /// where A is a symmetric matrix, x and y are vectors, and alpha is a scalar.
            /// </summary>
            /// <param name="layout">Specifies array storage: row- or column-major.</param>
            /// <param name="uplo">Specifies whether the matrix A is upper or lower triangular.</param>
            /// <param name="n">The order of the matrix A.</param>
            /// <param name="alpha">Scalar alpha.</param>
            /// <param name="x">Pointer to the first input vector x.</param>
            /// <param name="y">Pointer to the second input vector y.</param>
            /// <param name="a">Pointer to the symmetric matrix A (updated in place).</param>
            /// <param name="lda">Leading dimension of A.</param>
            /// <param name="incx">Increment for the elements of x (default is 1).</param>
            /// <param name="incy">Increment for the elements of y (default is 1).</param>
            public void Syr2(BLAS_Layout layout, BLAS_Uplo uplo,
                long n, double alpha, [In] double* x, [In] double* y,
                [In, Out] double* a, long lda,
                long incx = 1, long incy = 1)
                => IntelMKLNative.cblas_dsyr2_64(layout, uplo, n, alpha,
                    x, incx, y, incy, a, lda);

            #endregion
            #region ---- Spr2 [D] ----

            /// <summary>
            /// Performs a symmetric rank-2 update of a symmetric packed matrix.
            /// <para>
            /// A := alpha * x * y' + alpha * y * x' + A
            /// </para>
            /// </summary>
            /// <param name="layout">Specifies array storage: row- or column-major.</param>
            /// <param name="uplo">Specifies whether the matrix <paramref name="a"/> is upper or lower triangular.</param>
            /// <param name="n">The order of the matrix <paramref name="a"/>.</param>
            /// <param name="alpha">Scalar multiplier for the rank-2 update.</param>
            /// <param name="x">Pointer to the first input vector <c>x</c>.</param>
            /// <param name="y">Pointer to the second input vector <c>y</c>.</param>
            /// <param name="a">Pointer to the packed symmetric matrix <c>a</c> (updated in place).</param>
            /// <param name="incx">Increment for the elements of <paramref name="x"/> (default is 1).</param>
            /// <param name="incy">Increment for the elements of <paramref name="y"/> (default is 1).</param>
            public void Spr2(BLAS_Layout layout, BLAS_Uplo uplo,
                long n, double alpha, [In] double* x, [In] double* y,
                [In, Out] double* a,
                long incx = 1, long incy = 1)
                => IntelMKLNative.cblas_dspr2_64(layout, uplo, n, alpha,
                    x, incx, y, incy, a);

            #endregion
            #region ---- Hemv [Z] ----

            /// <summary>
            /// Computes a matrix-vector product using a Hermitian matrix.
            /// <para>y := alpha * A * x + beta * y</para>
            /// </summary>
            /// <param name="layout">Specifies array storage: row- or column-major.</param>
            /// <param name="uplo">Specifies whether the matrix A is upper or lower triangular.</param>
            /// <param name="n">The order of the matrix A.</param>
            /// <param name="alpha">Pointer to the scalar alpha.</param>
            /// <param name="a">Pointer to the Hermitian matrix A.</param>
            /// <param name="lda">Leading dimension of A.</param>
            /// <param name="x">Pointer to the vector x.</param>
            /// <param name="beta">Pointer to the scalar beta.</param>
            /// <param name="y">Pointer to the vector y.</param>
            /// <param name="incx">Increment for the elements of x.</param>
            /// <param name="incy">Increment for the elements of y.</param>
            public void Hemv(BLAS_Layout layout, BLAS_Uplo uplo,
                long n, void* alpha, [In] void* a, long lda,
                [In] void* x, void* beta, [In, Out] void* y,
                long incx = 1, long incy = 1)
                => IntelMKLNative.cblas_zhemv_64(layout, uplo, n, alpha,
                    a, lda, x, incx, beta, y, incy);

            #endregion
            #region ---- Hbmv [Z] ----

            /// <summary>
            /// Computes a matrix-vector product using a Hermitian band matrix.
            /// <para>y := alpha * A * x + beta * y</para>
            /// </summary>
            /// <param name="layout">Specifies array storage: row- or column-major.</param>
            /// <param name="uplo">Specifies whether the matrix A is upper or lower triangular.</param>
            /// <param name="n">The order of the matrix A.</param>
            /// <param name="k">The number of super-diagonals of the matrix A.</param>
            /// <param name="alpha">Pointer to the scalar alpha.</param>
            /// <param name="a">Pointer to the Hermitian band matrix A.</param>
            /// <param name="lda">Leading dimension of A.</param>
            /// <param name="x">Pointer to the vector x.</param>
            /// <param name="beta">Pointer to the scalar beta.</param>
            /// <param name="y">Pointer to the vector y (result).</param>
            /// <param name="incx">Increment for the elements of x.</param>
            /// <param name="incy">Increment for the elements of y.</param>
            public void Hbmv(BLAS_Layout layout, BLAS_Uplo uplo,
                long n, long k, void* alpha, [In] void* a, long lda,
                [In] void* x, void* beta, [In, Out] void* y,
                long incx = 1, long incy = 1)
                => IntelMKLNative.cblas_zhbmv_64(layout, uplo,
                    n, k, alpha, a, lda, x, incx, beta, y, incy);

            #endregion
            #region ---- Hpmv [Z] ----

            /// <summary>
            /// Computes a matrix-vector product using a Hermitian packed matrix.
            /// Performs the operation y := alpha * A * x + beta * y, where A is a Hermitian packed matrix.
            /// </summary>
            /// <param name="layout">Specifies array storage: row- or column-major.</param>
            /// <param name="uplo">Specifies whether the matrix A is upper or lower triangular.</param>
            /// <param name="n">The order of the matrix A.</param>
            /// <param name="alpha">Pointer to the scalar alpha.</param>
            /// <param name="a">Pointer to the Hermitian packed matrix A.</param>
            /// <param name="x">Pointer to the vector x.</param>
            /// <param name="beta">Pointer to the scalar beta.</param>
            /// <param name="y">Pointer to the vector y (result).</param>
            /// <param name="incx">Increment for the elements of x. Default is 1.</param>
            /// <param name="incy">Increment for the elements of y. Default is 1.</param>
            public void Hpmv(BLAS_Layout layout, BLAS_Uplo uplo,
                long n, void* alpha, [In] void* a,
                [In] void* x, void* beta, [In, Out] void* y,
                long incx = 1, long incy = 1)
                => IntelMKLNative.cblas_zhpmv_64(layout, uplo, n, alpha,
                    a, x, incx, beta, y, incy);

            #endregion
            #region ---- Geru [Z] ----

            /// <summary>
            /// Performs a rank-1 update (unconjugated) of a general complex matrix.
            /// </summary>
            /// <param name="layout">Specifies array storage: row- or column-major.</param>
            /// <param name="m">Number of rows of the matrix <paramref name="a"/>.</param>
            /// <param name="n">Number of columns of the matrix <paramref name="a"/>.</param>
            /// <param name="alpha">Pointer to the scalar multiplier for the rank-1 update.</param>
            /// <param name="x">Pointer to the complex vector <c>x</c> of length at least <paramref name="m"/>.</param>
            /// <param name="y">Pointer to the complex vector <c>y</c> of length at least <paramref name="n"/>.</param>
            /// <param name="a">Pointer to the complex matrix <c>a</c> to be updated (size at least <paramref name="lda"/> × <paramref name="n"/>).</param>
            /// <param name="lda">Leading dimension of <paramref name="a"/>.</param>
            /// <param name="incx">Increment for the elements of <paramref name="x"/> (default is 1).</param>
            /// <param name="incy">Increment for the elements of <paramref name="y"/> (default is 1).</param>
            /// <remarks>
            /// The operation performed is: <c>A := alpha * x * y' + A</c>,
            /// where <c>y'</c> is the conjugate transpose of <c>y</c>.
            /// This method wraps the native Intel MKL cblas_zgeru_64 function.
            /// </remarks>
            public void Geru(BLAS_Layout layout, long m, long n,
                void* alpha, [In] void* x, [In] void* y,
                [In, Out] void* a, long lda,
                long incx = 1, long incy = 1)
                => IntelMKLNative.cblas_zgeru_64(layout, m, n,
                    alpha, x, incx, y, incy, a, lda);

            #endregion
            #region ---- Gerc [Z] ----

            /// <summary>
            /// Performs a rank-1 update (conjugated) of a general matrix.
            /// Computes A := alpha * x * y' + A, where y' is the conjugate transpose of y.
            /// </summary>
            /// <param name="layout">Specifies array storage: row- or column-major.</param>
            /// <param name="m">Number of rows of the matrix a.</param>
            /// <param name="n">Number of columns of the matrix a.</param>
            /// <param name="alpha">Pointer to the scalar alpha (Complex*).</param>
            /// <param name="x">Pointer to the vector x (Complex*).</param>
            /// <param name="y">Pointer to the vector y (Complex*).</param>
            /// <param name="a">Pointer to the matrix a (Complex*), updated in place.</param>
            /// <param name="lda">Leading dimension of a.</param>
            /// <param name="incx">Increment for the elements of x.</param>
            /// <param name="incy">Increment for the elements of y.</param>
            public void Gerc(BLAS_Layout layout, long m, long n,
                void* alpha, [In] void* x, [In] void* y,
                [In, Out] void* a, long lda,
                long incx = 1, long incy = 1)
                => IntelMKLNative.cblas_zgerc_64(layout, m, n,
                    alpha, x, incx, y, incy, a, lda);

            #endregion
            #region ---- Her [Z] ----

            /// <summary>
            /// Performs a rank-1 update of a Hermitian matrix using double-precision data.
            /// <para>
            /// A := alpha * x * conj(x') + A
            /// </para>
            /// </summary>
            /// <param name="layout">Specifies array storage: row- or column-major.</param>
            /// <param name="uplo">Specifies whether the matrix <paramref name="a"/> is upper or lower triangular.</param>
            /// <param name="n">The order of the matrix <paramref name="a"/>.</param>
            /// <param name="alpha">The real scalar multiplier for the rank-1 update.</param>
            /// <param name="x">Pointer to the input vector <c>x</c> (complex values, interleaved as double[]).</param>
            /// <param name="a">Pointer to the Hermitian matrix <c>a</c> (complex values, interleaved as double[]), updated in-place.</param>
            /// <param name="lda">The leading dimension of <paramref name="a"/>.</param>
            /// <param name="incx">Increment for the elements of <paramref name="x"/> (default is 1).</param>
            public void Her(BLAS_Layout layout, BLAS_Uplo uplo,
                long n, double alpha, [In] double* x,
                [In, Out] double* a, long lda,
                long incx = 1)
                => IntelMKLNative.cblas_zher_64(layout, uplo, n,
                    alpha, x, incx, a, lda);

            #endregion
            #region ---- Hpr [Z] ----

            /// <summary>
            /// Performs a rank-1 update of a Hermitian packed matrix.
            /// <para>A := alpha * x * conj(x') + A</para>
            /// </summary>
            /// <param name="layout">Specifies array storage: row- or column-major.</param>
            /// <param name="uplo">Specifies whether the matrix a is upper or lower triangular.</param>
            /// <param name="n">The order of the matrix a.</param>
            /// <param name="alpha">Scalar alpha.</param>
            /// <param name="x">Pointer to the input vector x.</param>
            /// <param name="a">Pointer to the packed Hermitian matrix a (updated in place).</param>
            /// <param name="incx">Increment for indexing x (default is 1).</param>
            public void Hpr(BLAS_Layout layout, BLAS_Uplo uplo,
                long n, double alpha, [In] void* x,
                [In, Out] void* a,
                long incx = 1)
                => IntelMKLNative.cblas_zhpr_64(layout, uplo, n, alpha,
                    x, incx, a);

            #endregion
            #region ---- Her2 [Z] ----

            /// <summary>
            /// Performs a rank-2 update of a Hermitian matrix using complex double-precision data.
            /// <para>
            /// A := alpha * x * conjg(y') + conjg(alpha) * y * conjg(x') + A
            /// </para>
            /// </summary>
            /// <param name="layout">Specifies array storage: row- or column-major.</param>
            /// <param name="uplo">Specifies whether the matrix A is upper or lower triangular.</param>
            /// <param name="n">The order of the matrix A.</param>
            /// <param name="alpha">Pointer to the scalar alpha (complex double).</param>
            /// <param name="x">Pointer to the first vector x (complex double).</param>
            /// <param name="y">Pointer to the second vector y (complex double).</param>
            /// <param name="a">Pointer to the Hermitian matrix A (complex double), updated in-place.</param>
            /// <param name="lda">Leading dimension of the matrix A.</param>
            /// <param name="incx">Increment for the elements of x. Default is 1.</param>
            /// <param name="incy">Increment for the elements of y. Default is 1.</param>
            public void Her2(BLAS_Layout layout, BLAS_Uplo uplo,
                long n, void* alpha, [In] void* x, [In] void* y,
                [In, Out] void* a, long lda,
                long incx = 1, long incy = 1)
                => IntelMKLNative.cblas_zher2_64(layout, uplo, n,
                    alpha, x, incx, y, incy, a, lda);

            #endregion
            #region ---- Hpr2 [Z] ----

            /// <summary>
            /// Performs a rank-2 update of a Hermitian packed matrix.
            /// <para>
            /// A := alpha * x * conj(y') + conj(alpha) * y * conj(x') + A
            /// </para>
            /// </summary>
            /// <param name="layout">Specifies array storage: row- or column-major.</param>
            /// <param name="uplo">Specifies whether the matrix <paramref name="a"/> is upper or lower triangular.</param>
            /// <param name="n">The order of the matrix <paramref name="a"/>.</param>
            /// <param name="alpha">Pointer to the scalar alpha (complex value).</param>
            /// <param name="x">Pointer to the array (vector) x.</param>
            /// <param name="y">Pointer to the array (vector) y.</param>
            /// <param name="a">Pointer to the array (matrix) a (packed Hermitian matrix).</param>
            /// <param name="incx">Increment for the elements of <paramref name="x"/>. Default is 1.</param>
            /// <param name="incy">Increment for the elements of <paramref name="y"/>. Default is 1.</param>
            public void Hpr2(BLAS_Layout layout, BLAS_Uplo uplo,
                long n, void* alpha, [In] void* x, [In] void* y,
                [In, Out] void* a,
                long incx = 1, long incy = 1)
                => IntelMKLNative.cblas_zhpr2_64(layout, uplo, n,
                    alpha, x, incx, y, incy, a);

            #endregion

            #endregion
            #region level 3

            #region ---- Gemm [D/Z] ----

            /// <summary>
            /// Computes a matrix-matrix product with general matrices (double precision).
            /// C := alpha * op(A) * op(B) + beta * C
            /// </summary>
            /// <param name="layout">Specifies array storage: row- or column-major.</param>
            /// <param name="transa">Specifies matrix A transpose operation.</param>
            /// <param name="transb">Specifies matrix B transpose operation.</param>
            /// <param name="m">Number of rows of the matrix op(A).</param>
            /// <param name="n">Number of columns of the matrix op(B).</param>
            /// <param name="k">Number of columns of the matrix op(A).</param>
            /// <param name="alpha">Scalar alpha.</param>
            /// <param name="a">Pointer to array (matrix) A.</param>
            /// <param name="lda">Leading dimension of matrix A.</param>
            /// <param name="b">Pointer to array (matrix) B.</param>
            /// <param name="ldb">Leading dimension of matrix B.</param>
            /// <param name="beta">Scalar beta.</param>
            /// <param name="c">Pointer to array (matrix) C.</param>
            /// <param name="ldc">Leading dimension of matrix C.</param>
            public void Gemm(BLAS_Layout layout,
                BLAS_Transpose transa, BLAS_Transpose transb,
                long m, long n, long k, double alpha, [In] double* a, long lda,
                [In] double* b, long ldb, double beta,
                [In, Out] double* c, long ldc)
                => IntelMKLNative.cblas_dgemm_64(layout, transa, transb,
                    m, n, k, alpha, a, lda, b, ldb, beta, c, ldc);

            /// <summary>
            /// Computes a matrix-matrix product with general matrices (complex double precision).
            /// C := alpha * op(A) * op(B) + beta * C
            /// </summary>
            /// <param name="layout">Specifies array storage: row- or column-major.</param>
            /// <param name="transa">Specifies matrix A transpose operation.</param>
            /// <param name="transb">Specifies matrix B transpose operation.</param>
            /// <param name="m">Number of rows of the matrix op(A).</param>
            /// <param name="n">Number of columns of the matrix op(B).</param>
            /// <param name="k">Number of columns of the matrix op(A).</param>
            /// <param name="alpha">Pointer to scalar alpha (complex).</param>
            /// <param name="a">Pointer to array (matrix) A (complex).</param>
            /// <param name="lda">Leading dimension of matrix A.</param>
            /// <param name="b">Pointer to array (matrix) B (complex).</param>
            /// <param name="ldb">Leading dimension of matrix B.</param>
            /// <param name="beta">Pointer to scalar beta (complex).</param>
            /// <param name="c">Pointer to array (matrix) C (complex).</param>
            /// <param name="ldc">Leading dimension of matrix C.</param>
            public void Gemm(BLAS_Layout layout,
                BLAS_Transpose transa, BLAS_Transpose transb,
                long m, long n, long k, void* alpha, [In] void* a, long lda,
                [In] void* b, long ldb, void* beta,
                [In, Out] void* c, long ldc)
                => IntelMKLNative.cblas_zgemm_64(layout, transa, transb,
                    m, n, k, alpha, a, lda, b, ldb, beta, c, ldc);

            #endregion
            #region ---- Symm [D/Z] ----

            /// <summary>
            /// Computes a matrix-matrix product where one input matrix is symmetric.
            /// C := alpha * A * B + beta * C, or
            /// C := alpha * B * A + beta * C
            /// </summary>
            /// <param name="layout">Specifies array storage: row- or column-major.</param>
            /// <param name="side">Specifies whether the symmetric matrix A appears on the left or right in the operation.</param>
            /// <param name="uplo">Specifies whether the upper or lower triangular part of the symmetric matrix A is used.</param>
            /// <param name="m">Number of rows of the matrix op(A).</param>
            /// <param name="n">Number of columns of the matrix op(B).</param>
            /// <param name="alpha">Scalar alpha.</param>
            /// <param name="a">Pointer to the symmetric matrix A.</param>
            /// <param name="lda">Leading dimension of matrix A.</param>
            /// <param name="b">Pointer to matrix B.</param>
            /// <param name="ldb">Leading dimension of matrix B.</param>
            /// <param name="beta">Scalar beta.</param>
            /// <param name="c">Pointer to matrix C (result).</param>
            /// <param name="ldc">Leading dimension of matrix C.</param>
            public void Symm(BLAS_Layout layout,
                BLAS_Side side, BLAS_Uplo uplo,
                long m, long n, double alpha, [In] double* a, long lda,
                [In] double* b, long ldb, double beta,
                [In, Out] double* c, long ldc)
                => IntelMKLNative.cblas_dsymm_64(layout, side, uplo, m, n,
                    alpha, a, lda, b, ldb, beta, c, ldc);

            /// <summary>
            /// Computes a matrix-matrix product where one input matrix is symmetric (complex version).
            /// C := alpha * A * B + beta * C, or
            /// C := alpha * B * A + beta * C
            /// </summary>
            /// <param name="layout">Specifies array storage: row- or column-major.</param>
            /// <param name="side">Specifies whether the symmetric matrix A appears on the left or right in the operation.</param>
            /// <param name="uplo">Specifies whether the upper or lower triangular part of the symmetric matrix A is used.</param>
            /// <param name="m">Number of rows of the matrix op(A).</param>
            /// <param name="n">Number of columns of the matrix op(B).</param>
            /// <param name="alpha">Pointer to scalar alpha (complex).</param>
            /// <param name="a">Pointer to the symmetric matrix A (complex).</param>
            /// <param name="lda">Leading dimension of matrix A.</param>
            /// <param name="b">Pointer to matrix B (complex).</param>
            /// <param name="ldb">Leading dimension of matrix B.</param>
            /// <param name="beta">Pointer to scalar beta (complex).</param>
            /// <param name="c">Pointer to matrix C (complex, result).</param>
            /// <param name="ldc">Leading dimension of matrix C.</param>
            public void Symm(BLAS_Layout layout,
                BLAS_Side side, BLAS_Uplo uplo,
                long m, long n, void* alpha, [In] void* a, long lda,
                [In] void* b, long ldb, void* beta,
                [In, Out] void* c, long ldc)
                => IntelMKLNative.cblas_zsymm_64(layout, side, uplo, m, n,
                    alpha, a, lda, b, ldb, beta, c, ldc);

            #endregion
            #region ---- Syrk [D/Z] ----

            /// <summary>
            /// Performs a symmetric rank-k update for double-precision matrices.
            /// Computes C := alpha * A * A' + beta * C, or C := alpha * A' * A + beta * C.
            /// </summary>
            /// <param name="layout">Specifies array storage: row- or column-major.</param>
            /// <param name="uplo">Specifies whether the upper or lower triangular part of the symmetric matrix is used.</param>
            /// <param name="trans">Specifies matrix A transpose operation.</param>
            /// <param name="n">Number of columns of the matrix op(B).</param>
            /// <param name="k">The number of columns/rows (NoTrans/Trans) of the matrix A.</param>
            /// <param name="alpha">Scalar alpha.</param>
            /// <param name="a">Pointer to the input matrix A.</param>
            /// <param name="lda">Leading dimension of matrix A.</param>
            /// <param name="beta">Scalar beta.</param>
            /// <param name="c">Pointer to the output matrix C.</param>
            /// <param name="ldc">Leading dimension of matrix C.</param>
            public void Syrk(BLAS_Layout layout,
                BLAS_Uplo uplo, BLAS_Transpose trans,
                long n, long k, double alpha, [In] double* a, long lda,
                double beta, [In, Out] double* c, long ldc)
                => IntelMKLNative.cblas_dsyrk_64(layout, uplo, trans,
                    n, k, alpha, a, lda, beta, c, ldc);

            /// <summary>
            /// Performs a symmetric rank-k update for complex double-precision matrices.
            /// Computes C := alpha * A * A' + beta * C, or C := alpha * A' * A + beta * C.
            /// </summary>
            /// <param name="layout">Specifies array storage: row- or column-major.</param>
            /// <param name="uplo">Specifies whether the upper or lower triangular part of the symmetric matrix is used.</param>
            /// <param name="trans">Specifies matrix A transpose operation.</param>
            /// <param name="n">Number of columns of the matrix op(B).</param>
            /// <param name="k">The number of columns/rows (NoTrans/Trans) of the matrix A.</param>
            /// <param name="alpha">Pointer to the scalar alpha (complex).</param>
            /// <param name="a">Pointer to the input matrix A (complex).</param>
            /// <param name="lda">Leading dimension of matrix A.</param>
            /// <param name="beta">Pointer to the scalar beta (complex).</param>
            /// <param name="c">Pointer to the output matrix C (complex).</param>
            /// <param name="ldc">Leading dimension of matrix C.</param>
            public void Syrk(BLAS_Layout layout,
                BLAS_Uplo uplo, BLAS_Transpose trans,
                long n, long k, void* alpha, [In] void* a, long lda,
                void* beta, [In, Out] void* c, long ldc)
                => IntelMKLNative.cblas_zsyrk_64(layout, uplo, trans,
                    n, k, alpha, a, lda, beta, c, ldc);

            #endregion
            #region ---- Syr2k [D/Z] ----

            /// <summary>
            /// Performs a symmetric rank-2k update for double-precision matrices.
            /// C := alpha * A * B' + alpha * B * A' + beta * C, or
            /// C := alpha * A' * B + alpha * B' * A + beta * C
            /// </summary>
            /// <param name="layout">Specifies array storage: row- or column-major.</param>
            /// <param name="uplo">Specifies whether the upper or lower triangular part of the symmetric matrix C is used.</param>
            /// <param name="trans">Specifies matrix transpose operation.</param>
            /// <param name="n">Number of columns of the matrix C.</param>
            /// <param name="k">Number of columns/rows (NoTrans/Trans) of the matrices A and B.</param>
            /// <param name="alpha">Scalar alpha.</param>
            /// <param name="a">Pointer to matrix A.</param>
            /// <param name="lda">Leading dimension of matrix A.</param>
            /// <param name="b">Pointer to matrix B.</param>
            /// <param name="ldb">Leading dimension of matrix B.</param>
            /// <param name="beta">Scalar beta.</param>
            /// <param name="c">Pointer to matrix C.</param>
            /// <param name="ldc">Leading dimension of matrix C.</param>
            public void Syr2k(BLAS_Layout layout,
                BLAS_Uplo uplo, BLAS_Transpose trans,
                long n, long k, double alpha, [In] double* a, long lda,
                [In] double* b, long ldb, double beta,
                [In, Out] double* c, long ldc)
                => IntelMKLNative.cblas_dsyr2k_64(layout, uplo, trans,
                    n, k, alpha, a, lda, b, ldb, beta, c, ldc);

            /// <summary>
            /// Performs a symmetric rank-2k update for complex double-precision matrices.
            /// C := alpha * A * B' + alpha * B * A' + beta * C, or
            /// C := alpha * A' * B + alpha * B' * A + beta * C
            /// </summary>
            /// <param name="layout">Specifies array storage: row- or column-major.</param>
            /// <param name="uplo">Specifies whether the upper or lower triangular part of the symmetric matrix C is used.</param>
            /// <param name="trans">Specifies matrix transpose operation.</param>
            /// <param name="n">Number of columns of the matrix C.</param>
            /// <param name="k">Number of columns/rows (NoTrans/Trans) of the matrices A and B.</param>
            /// <param name="alpha">Pointer to scalar alpha (complex).</param>
            /// <param name="a">Pointer to matrix A (complex).</param>
            /// <param name="lda">Leading dimension of matrix A.</param>
            /// <param name="b">Pointer to matrix B (complex).</param>
            /// <param name="ldb">Leading dimension of matrix B.</param>
            /// <param name="beta">Pointer to scalar beta (complex).</param>
            /// <param name="c">Pointer to matrix C (complex).</param>
            /// <param name="ldc">Leading dimension of matrix C.</param>
            public void Syr2k(BLAS_Layout layout,
                BLAS_Uplo uplo, BLAS_Transpose trans,
                long n, long k, void* alpha, [In] void* a, long lda,
                [In] void* b, long ldb, void* beta,
                [In, Out] void* c, long ldc)
                => IntelMKLNative.cblas_zsyr2k_64(layout, uplo, trans,
                    n, k, alpha, a, lda, b, ldb, beta, c, ldc);

            #endregion
            #region ---- Trmm [D/Z] ----

            /// <summary>
            /// Computes a matrix-matrix product where one input matrix is triangular.
            /// B := alpha * op(A) * B, or B := alpha * B * op(A)
            /// </summary>
            /// <param name="layout">Specifies array storage: row- or column-major.</param>
            /// <param name="side">Specifies whether the triangular matrix A appears on the left or right in the operation.</param>
            /// <param name="uplo">Specifies whether the upper or lower triangular part of the matrix A is used.</param>
            /// <param name="trans">Specifies matrix A transpose operation.</param>
            /// <param name="diag">Specifies whether the matrix A is unit triangular.</param>
            /// <param name="m">Number of rows of the matrix op(A).</param>
            /// <param name="n">Number of columns of the matrix op(B).</param>
            /// <param name="alpha">Scalar alpha.</param>
            /// <param name="a">Pointer to array (matrix) A.</param>
            /// <param name="lda">Leading dimension of matrix A.</param>
            /// <param name="b">Pointer to array (matrix) B.</param>
            /// <param name="ldb">Leading dimension of matrix B.</param>
            public void Trmm(BLAS_Layout layout, BLAS_Side side,
                BLAS_Uplo uplo, BLAS_Transpose trans, BLAS_Diag diag,
                long m, long n, double alpha, [In] double* a, long lda,
                [In, Out] double* b, long ldb)
                => IntelMKLNative.cblas_dtrmm_64(layout, side,
                    uplo, trans, diag, m, n, alpha, a, lda, b, ldb);

            /// <summary>
            /// Computes a matrix-matrix product where one input matrix is triangular (complex version).
            /// B := alpha * op(A) * B, or B := alpha * B * op(A)
            /// </summary>
            /// <param name="layout">Specifies array storage: row- or column-major.</param>
            /// <param name="side">Specifies whether the triangular matrix A appears on the left or right in the operation.</param>
            /// <param name="uplo">Specifies whether the upper or lower triangular part of the matrix A is used.</param>
            /// <param name="trans">Specifies matrix A transpose operation.</param>
            /// <param name="diag">Specifies whether the matrix A is unit triangular.</param>
            /// <param name="m">Number of rows of the matrix op(A).</param>
            /// <param name="n">Number of columns of the matrix op(B).</param>
            /// <param name="alpha">Pointer to scalar alpha (complex).</param>
            /// <param name="a">Pointer to array (matrix) A (complex).</param>
            /// <param name="lda">Leading dimension of matrix A.</param>
            /// <param name="b">Pointer to array (matrix) B (complex).</param>
            /// <param name="ldb">Leading dimension of matrix B.</param>
            public void Trmm(BLAS_Layout layout, BLAS_Side side,
                BLAS_Uplo uplo, BLAS_Transpose trans, BLAS_Diag diag,
                long m, long n, void* alpha, [In] void* a, long lda,
                [In, Out] void* b, long ldb)
                => IntelMKLNative.cblas_ztrmm_64(layout, side,
                    uplo, trans, diag, m, n, alpha, a, lda, b, ldb);

            #endregion
            #region ---- Trsm [D/Z] ----

            /// <summary>
            /// Computes the solution to a triangular matrix equation for double-precision matrices.
            /// Solves op(A) * X = alpha * B or X * op(A) = alpha * B, where A is a triangular matrix.
            /// </summary>
            /// <param name="layout">Specifies array storage: row- or column-major.</param>
            /// <param name="side">Specifies whether the triangular matrix A appears on the left or right in the operation.</param>
            /// <param name="uplo">Specifies whether the matrix A is upper or lower triangular.</param>
            /// <param name="trans">Specifies matrix A transpose operation.</param>
            /// <param name="diag">Specifies whether the matrix A is unit triangular.</param>
            /// <param name="m">Number of rows of the matrix op(A).</param>
            /// <param name="n">Number of columns of the matrix op(B).</param>
            /// <param name="alpha">Scalar alpha.</param>
            /// <param name="a">Pointer to the array (matrix) A.</param>
            /// <param name="lda">Leading dimension of matrix A.</param>
            /// <param name="b">Pointer to the array (matrix) B.</param>
            /// <param name="ldb">Leading dimension of matrix B.</param>
            public void Trsm(BLAS_Layout layout, BLAS_Side side,
                BLAS_Uplo uplo, BLAS_Transpose trans, BLAS_Diag diag,
                long m, long n, double alpha, [In] double* a, long lda,
                [In, Out] double* b, long ldb)
                => IntelMKLNative.cblas_dtrsm_64(layout, side,
                    uplo, trans, diag, m, n, alpha, a, lda, b, ldb);

            /// <summary>
            /// Computes the solution to a triangular matrix equation for complex double-precision matrices.
            /// Solves op(A) * X = alpha * B or X * op(A) = alpha * B, where A is a triangular matrix.
            /// </summary>
            /// <param name="layout">Specifies array storage: row- or column-major.</param>
            /// <param name="side">Specifies whether the triangular matrix A appears on the left or right in the operation.</param>
            /// <param name="uplo">Specifies whether the matrix A is upper or lower triangular.</param>
            /// <param name="trans">Specifies matrix A transpose operation.</param>
            /// <param name="diag">Specifies whether the matrix A is unit triangular.</param>
            /// <param name="m">Number of rows of the matrix op(A).</param>
            /// <param name="n">Number of columns of the matrix op(B).</param>
            /// <param name="alpha">Pointer to the scalar alpha (complex).</param>
            /// <param name="a">Pointer to the array (matrix) A (complex).</param>
            /// <param name="lda">Leading dimension of matrix A.</param>
            /// <param name="b">Pointer to the array (matrix) B (complex).</param>
            /// <param name="ldb">Leading dimension of matrix B.</param>
            public void Trsm(BLAS_Layout layout, BLAS_Side side,
                BLAS_Uplo uplo, BLAS_Transpose trans, BLAS_Diag diag,
                long m, long n, void* alpha, [In] void* a, long lda,
                [In, Out] double* b, long ldb)
                => IntelMKLNative.cblas_ztrsm_64(layout, side,
                    uplo, trans, diag, m, n, alpha, a, lda, b, ldb);

            #endregion
            #region ---- Hemm [Z] ----

            /// <summary>
            /// Computes a matrix-matrix product where one input matrix is Hermitian.
            /// C := alpha * A * B + beta * C, or
            /// C := alpha * B * A + beta * C
            /// </summary>
            /// <param name="layout">Specifies array storage: row- or column-major.</param>
            /// <param name="side">Specifies whether the Hermitian matrix A appears on the left or right in the operation.</param>
            /// <param name="uplo">Specifies whether the upper or lower triangular part of the Hermitian matrix A is used.</param>
            /// <param name="m">Number of rows of the matrix op(A).</param>
            /// <param name="n">Number of columns of the matrix op(B).</param>
            /// <param name="alpha">Pointer to the scalar alpha.</param>
            /// <param name="a">Pointer to the Hermitian matrix A.</param>
            /// <param name="lda">Leading dimension of matrix A.</param>
            /// <param name="b">Pointer to the matrix B.</param>
            /// <param name="ldb">Leading dimension of matrix B.</param>
            /// <param name="beta">Pointer to the scalar beta.</param>
            /// <param name="c">Pointer to the matrix C (result).</param>
            /// <param name="ldc">Leading dimension of matrix C.</param>
            public void Hemm(BLAS_Layout layout, BLAS_Side side, BLAS_Uplo uplo,
                long m, long n, void* alpha, [In] void* a, long lda,
                [In] void* b, long ldb, void* beta,
                [In, Out] void* c, long ldc)
                => IntelMKLNative.cblas_zhemm_64(layout, side, uplo,
                    m, n, alpha, a, lda, b, ldb, beta, c, ldc);

            #endregion
            #region ---- Herk [Z] ----

            /// <summary>
            /// Performs a Hermitian rank-k update operation on a complex matrix.
            /// <para>
            /// C := alpha * A * A<sup>H</sup> + beta * C, or
            /// C := alpha * A<sup>H</sup> * A + beta * C
            /// </para>
            /// </summary>
            /// <param name="layout">Specifies array storage: row- or column-major.</param>
            /// <param name="uplo">Specifies whether the upper or lower triangular part of the Hermitian matrix C is used.</param>
            /// <param name="trans">Specifies the operation applied to matrix A.</param>
            /// <param name="n">The order of the matrix C (number of rows and columns).</param>
            /// <param name="k">If <paramref name="trans"/> is NoTrans, the number of columns of A; otherwise, the number of rows of A.</param>
            /// <param name="alpha">Scaling factor for the rank-k product.</param>
            /// <param name="a">Pointer to the input matrix A.</param>
            /// <param name="lda">Leading dimension of matrix A.</param>
            /// <param name="beta">Scaling factor for matrix C.</param>
            /// <param name="c">Pointer to the Hermitian matrix C (updated in place).</param>
            /// <param name="ldc">Leading dimension of matrix C.</param>
            public void Herk(BLAS_Layout layout, BLAS_Uplo uplo, BLAS_Transpose trans,
                long n, long k, double alpha, [In] void* a, long lda,
                double beta, [In, Out] void* c, long ldc)
                => IntelMKLNative.cblas_zherk_64(layout, uplo, trans,
                    n, k, alpha, a, lda, beta, c, ldc);

            #endregion
            #region ---- Her2k [Z] ----

            /// <summary>
            /// Performs a Hermitian rank-2k update for complex double-precision matrices.
            /// <para>
            /// C := alpha * A * Bᴴ + conj(alpha) * B * Aᴴ + beta * C, or
            /// C := alpha * Aᴴ * B + conj(alpha) * Bᴴ * A + beta * C
            /// </para>
            /// </summary>
            /// <param name="layout">Specifies array storage: row- or column-major.</param>
            /// <param name="uplo">Specifies whether the upper or lower triangular part of the Hermitian matrix C is used.</param>
            /// <param name="trans">Specifies the operation applied to matrices A and B.</param>
            /// <param name="n">The order of the matrix C (number of rows and columns).</param>
            /// <param name="k">The number of columns (if not transposed) or rows (if transposed) of matrices A and B.</param>
            /// <param name="alpha">Pointer to the scalar alpha (complex double).</param>
            /// <param name="a">Pointer to the matrix A (complex double).</param>
            /// <param name="lda">Leading dimension of matrix A.</param>
            /// <param name="b">Pointer to the matrix B (complex double).</param>
            /// <param name="ldb">Leading dimension of matrix B.</param>
            /// <param name="beta">Pointer to the scalar beta (complex double).</param>
            /// <param name="c">Pointer to the Hermitian matrix C (complex double), updated in place.</param>
            /// <param name="ldc">Leading dimension of matrix C.</param>
            public void Her2k(BLAS_Layout layout, BLAS_Uplo uplo, BLAS_Transpose trans,
                long n, long k, void* alpha, [In] void* a, long lda,
                void* b, long ldb, void* beta, [In, Out] void* c, long ldc)
                => IntelMKLNative.cblas_zher2k_64(layout, uplo, trans,
                    n, k, alpha, a, lda, b, ldb, beta, c, ldc);

            #endregion

            #endregion
            #region exteisions

            #region helphers

            /// <summary>
            /// Returns the character code for the specified BLAS matrix layout.
            /// </summary>
            /// <param name="layout">The BLAS matrix layout (row-major or column-major).</param>
            /// <returns>
            /// The character code representing the layout:
            /// 'R' for row-major, 'C' for column-major.
            /// </returns>
            private static byte GetLayoutChar(BLAS_Layout layout)
            {
                byte layoutChar;
                switch (layout)
                {
                    case BLAS_Layout.RowMajor:
                        layoutChar = (byte)'R';
                        break;
                    case BLAS_Layout.ColMajor:
                        layoutChar = (byte)'C';
                        break;
                    default: goto case BLAS_Layout.RowMajor;
                }
                return layoutChar;
            }

            /// <summary>
            /// Returns the character code for the specified BLAS transpose operation.
            /// </summary>
            /// <param name="operation">The BLAS transpose operation (no transpose, transpose, or conjugate transpose).</param>
            /// <returns>
            /// The character code representing the operation:
            /// 'N' for no transpose, 'T' for transpose, 'C' for conjugate transpose.
            /// </returns>
            private static byte GetOperationChar(BLAS_Transpose operation)
            {
                byte operationChar;
                switch (operation)
                {
                    case BLAS_Transpose.NoTrans:
                        operationChar = (byte)'N';
                        break;
                    case BLAS_Transpose.Trans:
                        operationChar = (byte)'T';
                        break;
                    case BLAS_Transpose.ConjTrans:
                        operationChar = (byte)'C';
                        break;
                    default: goto case BLAS_Transpose.NoTrans;
                }
                return operationChar;
            }

            #endregion
            #region ---- Axpby [D/Z] ----

            /// <summary>
            /// Computes a scaled vector addition with two scalars.
            /// Performs the operation y := a*x + b*y for double-precision arrays.
            /// </summary>
            /// <param name="n">Number of elements in the vectors.</param>
            /// <param name="a">Scalar multiplier for x.</param>
            /// <param name="x">Pointer to the input vector x.</param>
            /// <param name="b">Scalar multiplier for y.</param>
            /// <param name="y">Pointer to the input/output vector y.</param>
            /// <param name="incx">Increment for the elements of x (default is 1).</param>
            /// <param name="incy">Increment for the elements of y (default is 1).</param>
            public void Axpby(long n, double a, [In] double* x,
                double b, [In, Out] double* y,
                long incx = 1, long incy = 1)
                => IntelMKLNative.cblas_daxpby_64(n, a, x, incx, b, y, incy);

            /// <summary>
            /// Computes a scaled vector addition with two scalars for complex arrays.
            /// Performs the operation y := a*x + b*y for complex vectors.
            /// </summary>
            /// <param name="n">Number of elements in the vectors.</param>
            /// <param name="a">Pointer to the scalar multiplier for x (complex).</param>
            /// <param name="x">Pointer to the input vector x (complex).</param>
            /// <param name="b">Pointer to the scalar multiplier for y (complex).</param>
            /// <param name="y">Pointer to the input/output vector y (complex).</param>
            /// <param name="incx">Increment for the elements of x (default is 1).</param>
            /// <param name="incy">Increment for the elements of y (default is 1).</param>
            public void Axpby(long n, void* a, [In] void* x,
                void* b, [In, Out] void* y,
                long incx = 1, long incy = 1)
                => IntelMKLNative.cblas_zaxpby_64(n, a, x, incx, b, y, incy);

            #endregion
            #region ---- Gemm3m [Z] ----

            /// <summary>
            /// Computes a matrix-matrix product for complex matrices using the 3m algorithm.
            /// C := alpha * op(A) * op(B) + beta * C
            /// </summary>
            /// <param name="layout">Specifies array storage: row- or column-major.</param>
            /// <param name="transa">Specifies matrix A transpose operation.</param>
            /// <param name="transb">Specifies matrix B transpose operation.</param>
            /// <param name="m">Number of rows of the matrix op(A).</param>
            /// <param name="n">Number of columns of the matrix op(B).</param>
            /// <param name="k">Number of columns of the matrix op(A) and rows of op(B).</param>
            /// <param name="alpha">Pointer to the scalar alpha (complex).</param>
            /// <param name="a">Pointer to the input matrix A (complex).</param>
            /// <param name="lda">Leading dimension of matrix A.</param>
            /// <param name="b">Pointer to the input matrix B (complex).</param>
            /// <param name="ldb">Leading dimension of matrix B.</param>
            /// <param name="beta">Pointer to the scalar beta (complex).</param>
            /// <param name="c">Pointer to the output matrix C (complex), overwritten on exit.</param>
            /// <param name="ldc">Leading dimension of matrix C.</param>
            public void Gemm3m(BLAS_Layout layout,
                BLAS_Transpose transa, BLAS_Transpose transb,
                long m, long n, long k, void* alpha,
                [In] void* a, long lda, [In] void* b, long ldb,
                void* beta, [In, Out] void* c, long ldc)
                => IntelMKLNative.cblas_zgemm3m_64(layout, transa, transb,
                    m, n, k, alpha, a, lda, b, ldb, beta, c, ldc);


            #endregion
            #region ---- Imatcopy ----

            /// <summary>
            /// Performs in-place scaling and transposition/copying of a double-precision matrix.
            /// </summary>
            /// <param name="layout">Specifies the layout of the input matrix (row-major or column-major).</param>
            /// <param name="operation">Specifies the operation to perform (e.g., transpose).</param>
            /// <param name="rows">Number of rows before the operation.</param>
            /// <param name="cols">Number of columns before the operation.</param>
            /// <param name="alpha">Scaling factor applied to the matrix elements.</param>
            /// <param name="ab">Pointer to the matrix data (overwritten on exit).</param>
            /// <param name="lda">Leading dimension of the matrix before the operation.</param>
            /// <param name="ldb">Leading dimension of the matrix after the operation.</param>
            public void ImatCopy(BLAS_Layout layout, BLAS_Transpose operation,
                long rows, long cols, double alpha,
                [In, Out] double* ab, long lda, long ldb)
                => IntelMKLNative.MKL_Dimatcopy(GetLayoutChar(layout),
                    GetOperationChar(operation),
                    rows, cols, alpha, ab, lda, ldb);

            /// <summary>
            /// Performs in-place scaling and transposition/copying of a complex-precision matrix.
            /// </summary>
            /// <param name="layout">Specifies the layout of the input matrix (row-major or column-major).</param>
            /// <param name="operation">Specifies the operation to perform (e.g., transpose).</param>
            /// <param name="rows">Number of rows before the operation.</param>
            /// <param name="cols">Number of columns before the operation.</param>
            /// <param name="alpha">Pointer to the scaling factor (complex) applied to the matrix elements.</param>
            /// <param name="ab">Pointer to the matrix data (overwritten on exit).</param>
            /// <param name="lda">Leading dimension of the matrix before the operation.</param>
            /// <param name="ldb">Leading dimension of the matrix after the operation.</param>
            public void ImatCopy(BLAS_Layout layout, BLAS_Transpose operation,
                long rows, long cols, void* alpha,
                [In, Out] void* ab, long lda, long ldb)
                => IntelMKLNative.MKL_Zimatcopy(GetLayoutChar(layout),
                    GetOperationChar(operation),
                    rows, cols, alpha, ab, lda, ldb);

            #endregion
            #region ---- Omatcopy ----

            /// <summary>
            /// Performs scaling and out-place transposition/copying of matrices for double-precision values.
            /// </summary>
            /// <param name="layout">Layout of the input matrix (row-major or column-major).</param>
            /// <param name="operation">Specifies whether to transpose the matrix.</param>
            /// <param name="rows">Number of rows before the operation.</param>
            /// <param name="cols">Number of columns before the operation.</param>
            /// <param name="alpha">Scaling factor alpha.</param>
            /// <param name="a">Pointer to the input matrix data before the operation.</param>
            /// <param name="lda">Leading dimension of the input matrix before the operation.</param>
            /// <param name="b">Pointer to the output matrix data after the operation.</param>
            /// <param name="ldb">Leading dimension of the output matrix after the operation.</param>
            public void OmatCopy(BLAS_Layout layout, BLAS_Transpose operation,
                long rows, long cols, double alpha, [In] double* a, long lda,
                [Out] double* b, long ldb)
                => IntelMKLNative.MKL_Domatcopy(GetLayoutChar(layout),
                    GetOperationChar(operation),
                    rows, cols, alpha, a, lda, b, ldb);

            /// <summary>
            /// Performs scaling and out-place transposition/copying of matrices for complex values.
            /// </summary>
            /// <param name="layout">Layout of the input matrix (row-major or column-major).</param>
            /// <param name="operation">Specifies whether to transpose the matrix.</param>
            /// <param name="rows">Number of rows before the operation.</param>
            /// <param name="cols">Number of columns before the operation.</param>
            /// <param name="alpha">Pointer to the scaling factor alpha (complex).</param>
            /// <param name="a">Pointer to the input matrix data before the operation (complex).</param>
            /// <param name="lda">Leading dimension of the input matrix before the operation.</param>
            /// <param name="b">Pointer to the output matrix data after the operation (complex).</param>
            /// <param name="ldb">Leading dimension of the output matrix after the operation.</param>
            public void OmatCopy(BLAS_Layout layout, BLAS_Transpose operation,
                long rows, long cols, void* alpha, [In] void* a, long lda,
                [Out] void* b, long ldb)
                => IntelMKLNative.MKL_Zomatcopy(GetLayoutChar(layout),
                    GetOperationChar(operation),
                    rows, cols, alpha, a, lda, b, ldb);

            #endregion

            #endregion
        }

        
        /// <summary>
        /// Sparse BLAS class
        /// </summary>
        public unsafe class SPBLAS : ISPBLAS
        {
            #region Level 1

            #region ---- Asum [D/Z] ----

            /// <summary>
            /// Computes the sum of magnitudes of the non-zero elements in a sparse double-precision array.
            /// dasum := |x[0]| + |x[1]| + ... + |x[n-1]|
            /// </summary>
            /// <param name="n">Number of non-zero elements in the array.</param>
            /// <param name="x">Pointer to the array of double values.</param>
            /// <param name="incx">Increment for indexing x (default is 1).</param>
            /// <returns>Sum of the magnitudes of the non-zero elements.</returns>
            public double Asum(long n, [In] double* x, long incx = 1)
                => IntelMKLNative.cblas_dasum_64(n, x, incx);

            /// <summary>
            /// Computes the sum of magnitudes of the non-zero elements in a sparse complex double-precision array.
            /// dzasum := |Re(x[0])| + |Im(x[0])| + |Re(x[1])| + |Im(x[1])| + ... + |Re(x[n-1])| + |Im(x[n-1])|
            /// </summary>
            /// <param name="n">Number of non-zero elements in the array.</param>
            /// <param name="x">Pointer to the array of complex values (as void*).</param>
            /// <param name="incx">Increment for indexing x (default is 1).</param>
            /// <returns>Sum of the magnitudes of the real and imaginary parts of the elements.</returns>
            public double Asum(long n, [In] void* x, long incx = 1)
                => IntelMKLNative.cblas_dzasum_64(n, x, incx);

            #endregion
            #region ---- Axpy [D/Z] ----

            /// <summary>
            /// Adds a scalar multiple of a compressed sparse vector to a full-storage vector.
            /// Computes y := a * x + y, where x is a sparse vector and y is a dense vector.
            /// </summary>
            /// <param name="n">Number of non-zero elements in the sparse vector x.</param>
            /// <param name="a">Scalar multiplier.</param>
            /// <param name="x">Pointer to the values of the sparse vector x.</param>
            /// <param name="indx">Pointer to the indices of the sparse vector x.</param>
            /// <param name="y">Pointer to the dense vector y.</param>
            public void Axpy(long n, double a,
                [In] double* x, [In] long* indx,
                [In, Out] double* y)
                => IntelMKLNative.cblas_daxpyi_64(n, a, x, indx, y);

            /// <summary>
            /// Adds a scalar multiple of a compressed sparse complex vector to a full-storage complex vector.
            /// Computes y := a * x + y, where x is a sparse complex vector and y is a dense complex vector.
            /// </summary>
            /// <param name="n">Number of non-zero elements in the sparse vector x.</param>
            /// <param name="a">Pointer to the scalar multiplier (complex).</param>
            /// <param name="x">Pointer to the values of the sparse complex vector x.</param>
            /// <param name="indx">Pointer to the indices of the sparse vector x.</param>
            /// <param name="y">Pointer to the dense complex vector y.</param>
            public void Axpy(long n, void* a,
                [In] void* x, [In] long* indx,
                [In, Out] void* y)
                => IntelMKLNative.cblas_zaxpyi_64(n, a, x, indx, y);

            #endregion
            #region ---- Copy [D/Z] ----

            /// <summary>
            /// Copies elements from the source array <paramref name="x"/> to the destination array <paramref name="y"/>.
            /// </summary>
            /// <param name="n">Number of elements to copy.</param>
            /// <param name="x">Pointer to the source array.</param>
            /// <param name="y">Pointer to the destination array.</param>
            /// <param name="incx">Increment for the elements of <paramref name="x"/>. Default is 1.</param>
            /// <param name="incy">Increment for the elements of <paramref name="y"/>. Default is 1.</param>
            public void Copy(long n, [In] double* x,
                [In, Out] double* y,
                long incx = 1, long incy = 1)
                => IntelMKLNative.cblas_dcopy_64(n, x, incx, y, incy);

            /// <summary>
            /// Copies elements from the source complex array <paramref name="x"/> to the destination complex array <paramref name="y"/>.
            /// </summary>
            /// <param name="n">Number of elements to copy.</param>
            /// <param name="x">Pointer to the source complex array.</param>
            /// <param name="y">Pointer to the destination complex array.</param>
            /// <param name="incx">Increment for the elements of <paramref name="x"/>. Default is 1.</param>
            /// <param name="incy">Increment for the elements of <paramref name="y"/>. Default is 1.</param>
            public void Copy(long n, [In] void* x,
                [In, Out] void* y,
                long incx = 1, long incy = 1)
                => IntelMKLNative.cblas_zcopy_64(n, x, incx, y, incy);

            #endregion
            #region ---- Dot [D/Z] ----

            /// <summary>
            /// Computes the dot product of a sparse vector and a dense vector.
            /// res = x[0]*y[indx[0]] + x[1]*y[indx[1]] +...+ x[nz-1]*y[indx[nz-1]]
            /// </summary>
            /// <param name="n">Number of non-zero elements in the sparse vector <paramref name="x"/>.</param>
            /// <param name="x">Pointer to the sparse vector values.</param>
            /// <param name="indx">Pointer to the indices of the non-zero elements in the dense vector <paramref name="y"/>.</param>
            /// <param name="y">Pointer to the dense vector values.</param>
            /// <returns>The result of the dot product of <paramref name="x"/> and <paramref name="y"/>.</returns>
            public double Dot(long n, [In] double* x, [In] long* indx,
                [In] double* y)
                => IntelMKLNative.cblas_ddoti_64(n, x, indx, y);

            /// <summary>
            /// Computes the dot product of a sparse complex vector and a dense complex vector.
            /// res = x[0]*y[indx[0]] + x[1]*y[indx[1]] +...+ x[nz-1]*y[indx[nz-1]]
            /// </summary>
            /// <param name="n">Number of non-zero elements in the sparse vector <paramref name="x"/>.</param>
            /// <param name="x">Pointer to the sparse complex vector values.</param>
            /// <param name="indx">Pointer to the indices of the non-zero elements in the dense vector <paramref name="y"/>.</param>
            /// <param name="y">Pointer to the dense complex vector values.</param>
            /// <param name="dotu">Pointer to the result of the dot product (output parameter).</param>
            public void Dot(long n, [In] void* x, [In] long* indx,
                [In] void* y, void* dotu)
                => IntelMKLNative.cblas_zdotui_sub_64(n, x, indx, y, dotu);

            #endregion
            #region ---- Dotc [Z] ----

            /// <summary>
            /// Computes the dot product of a conjugated complex sparse vector with a dense complex vector.
            /// <para>res = conj(x[0])*y[indx[0]] + ... + conj(x[n-1])*y[indx[n-1]]</para>
            /// </summary>
            /// <param name="n">Number of non-zero elements in the sparse vector x.</param>
            /// <param name="x">Pointer to the sparse complex vector x (to be conjugated).</param>
            /// <param name="indx">Pointer to the index array for the sparse vector x.</param>
            /// <param name="y">Pointer to the dense complex vector y.</param>
            /// <param name="dotc">Pointer to the result (output complex value).</param>
            public void Dotc(long n, [In] void* x, [In] long* indx,
                [In] void* y, void* dotc)
                => IntelMKLNative.cblas_zdotci_sub_64(n, x, indx, y, dotc);

            #endregion
            #region ---- Nrm2 [D/Z] ----

            /// <summary>
            /// Computes the Euclidean norm (L2 norm) of a vector of doubles.
            /// </summary>
            /// <param name="n">The number of elements in the vector.</param>
            /// <param name="x">Pointer to the vector of doubles.</param>
            /// <param name="incx">The increment for the elements of <paramref name="x"/>. Default is 1.</param>
            /// <returns>The Euclidean norm of the vector.</returns>
            public double Nrm2(long n, [In] double* x, long incx = 1)
                => IntelMKLNative.cblas_dnrm2_64(n, x, incx);

            /// <summary>
            /// Computes the Euclidean norm (L2 norm) of a vector of complex numbers.
            /// </summary>
            /// <param name="n">The number of elements in the vector.</param>
            /// <param name="x">Pointer to the vector of complex numbers.</param>
            /// <param name="incx">The increment for the elements of <paramref name="x"/>. Default is 1.</param>
            /// <returns>The Euclidean norm of the vector.</returns>
            public double Nrm2(long n, [In] void* x, long incx = 1)
                => IntelMKLNative.cblas_dznrm2_64(n, x, incx);

            #endregion
            #region ---- Rot [D] ----

            /// <summary>
            /// Performs a Givens rotation of points in the plane for sparse vectors.
            /// </summary>
            /// <param name="n">Number of non-zero elements in <paramref name="x"/>.</param>
            /// <param name="x">Pointer to the sparse vector <c>x</c>.</param>
            /// <param name="indx">Pointer to the index array for <c>y</c>.</param>
            /// <param name="y">Pointer to the dense vector <c>y</c>.</param>
            /// <param name="c">Cosine component of the rotation.</param>
            /// <param name="s">Sine component of the rotation.</param>
            /// <remarks>
            /// For each i: 
            /// <c>x[i] = c * x[i] + s * y[indx[i]]</c><br/>
            /// <c>y[indx[i]] = c * y[indx[i]] - s * x[i]</c>
            /// </remarks>
            public void Rot(long n, [In, Out] double* x, [In] long* indx,
                [In, Out] double* y, double c, double s)
                => IntelMKLNative.cblas_droti_64(n, x, indx, y, c, s);

            #endregion
            #region ---- Scal [D/Z] ----

            /// <summary>
            /// Scales a vector by a scalar value (double precision, dense).
            /// </summary>
            /// <param name="n">Number of elements in the vector.</param>
            /// <param name="a">Scalar multiplier.</param>
            /// <param name="x">Pointer to the vector to be scaled.</param>
            /// <param name="incx">Increment for the elements of <paramref name="x"/> (default is 1).</param>
            public void Scal(long n, double a,
                [In, Out] double* x,
                long incx = 1)
                => IntelMKLNative.cblas_dscal_64(n, a, x, incx);

            /// <summary>
            /// Scales a vector by a scalar value (double precision, complex, dense).
            /// </summary>
            /// <param name="n">Number of elements in the vector.</param>
            /// <param name="a">Scalar multiplier.</param>
            /// <param name="x">Pointer to the complex vector to be scaled.</param>
            /// <param name="incx">Increment for the elements of <paramref name="x"/> (default is 1).</param>
            public void Scal(long n, double a,
                [In, Out] void* x,
                long incx = 1)
                => IntelMKLNative.cblas_zdscal_64(n, a, x, incx);

            /// <summary>
            /// Scales a vector by a scalar value (complex precision, dense).
            /// </summary>
            /// <param name="n">Number of elements in the vector.</param>
            /// <param name="a">Pointer to the complex scalar multiplier.</param>
            /// <param name="x">Pointer to the complex vector to be scaled.</param>
            /// <param name="incx">Increment for the elements of <paramref name="x"/> (default is 1).</param>
            public void Scal(long n, void* a,
                [In, Out] void* x,
                long incx = 1)
                => IntelMKLNative.cblas_zscal_64(n, a, x, incx);

            #endregion
            #region ---- Iamax [D/Z] ----

            /// <summary>
            /// Finds the index of the element with the maximum absolute value in a double array.
            /// </summary>
            /// <param name="n">Number of elements in the array.</param>
            /// <param name="x">Pointer to the array of doubles.</param>
            /// <param name="incx">Increment for indexing x (default is 1).</param>
            /// <returns>Index of the element with the largest absolute value.</returns>
            public long Iamax(long n, [In] double* x,
                long incx = 1)
                => IntelMKLNative.cblas_idamax_64(n, x, incx);

            /// <summary>
            /// Finds the index of the element with the maximum absolute value in a complex array.
            /// </summary>
            /// <param name="n">Number of elements in the array.</param>
            /// <param name="x">Pointer to the array of complex numbers (as void*).</param>
            /// <param name="incx">Increment for indexing x (default is 1).</param>
            /// <returns>Index of the element with the largest absolute value.</returns>
            public long Iamax(long n, [In] void* x,
                long incx = 1)
                => IntelMKLNative.cblas_izamax_64(n, x, incx);

            #endregion
            #region ---- Iamin [D/Z] ----

            /// <summary>
            /// Finds the index of the element with the smallest absolute value in a double array.
            /// </summary>
            /// <param name="n">Number of elements in the array <paramref name="x"/>.</param>
            /// <param name="x">Pointer to the array of doubles.</param>
            /// <param name="incx">Increment for indexing <paramref name="x"/> (default is 1).</param>
            /// <returns>Index of the element with the smallest absolute value.</returns>
            public long Iamin(long n, [In] double* x,
                long incx = 1)
                => IntelMKLNative.cblas_idamin_64(n, x, incx);

            /// <summary>
            /// Finds the index of the element with the smallest absolute value in a complex array.
            /// </summary>
            /// <param name="n">Number of elements in the array <paramref name="x"/>.</param>
            /// <param name="x">Pointer to the array of complex numbers (as void*).</param>
            /// <param name="incx">Increment for indexing <paramref name="x"/> (default is 1).</param>
            /// <returns>Index of the element with the smallest absolute value.</returns>
            public long Iamin(long n, [In] void* x,
                long incx = 1)
                => IntelMKLNative.cblas_izamin_64(n, x, incx);

            #endregion
            #region ---- Gthr [D/Z] ----

            /// <summary>
            /// Gathers elements from a dense vector <paramref name="y"/> into a sparse vector <paramref name="x"/>,
            /// using the indices specified by <paramref name="indx"/>.
            /// For each i in [0, n), sets x[i] = y[indx[i]].
            /// </summary>
            /// <param name="n">Number of elements to gather.</param>
            /// <param name="y">Pointer to the dense source vector.</param>
            /// <param name="x">Pointer to the sparse destination vector.</param>
            /// <param name="indx">Pointer to the array of indices specifying which elements to gather.</param>
            public void Gthr(long n, [In] double* y,
                [In, Out] double* x, [In] long* indx)
                => IntelMKLNative.cblas_dgthr_64(n, y, x, indx);

            /// <summary>
            /// Gathers elements from a dense complex vector <paramref name="y"/> into a sparse complex vector <paramref name="x"/>,
            /// using the indices specified by <paramref name="indx"/>.
            /// For each i in [0, n), sets x[i] = y[indx[i]].
            /// </summary>
            /// <param name="n">Number of elements to gather.</param>
            /// <param name="y">Pointer to the dense complex source vector.</param>
            /// <param name="x">Pointer to the sparse complex destination vector.</param>
            /// <param name="indx">Pointer to the array of indices specifying which elements to gather.</param>
            public void Gthr(long n, [In] void* y,
                [In, Out] void* x, [In] long* indx)
                => IntelMKLNative.cblas_zgthr_64(n, y, x, indx);

            #endregion
            #region ---- Gthrz [D/Z] ----

            /// <summary>
            /// Gathers a full-storage sparse vector's elements into compressed form, replacing them by zeros.
            /// For each i in [0, n-1]: x[i] = y[indx[i]]; y[indx[i]] = 0.
            /// </summary>
            /// <param name="n">Number of non-zero array elements.</param>
            /// <param name="y">Pointer to the dense vector y.</param>
            /// <param name="x">Pointer to the sparse vector x.</param>
            /// <param name="indx">Pointer to the index array.</param>
            public void Gthrz(long n, [In, Out] double* y,
                [In, Out] double* x, [In] long* indx)
                => IntelMKLNative.cblas_dgthrz_64(n, y, x, indx);

            /// <summary>
            /// Gathers a full-storage sparse complex vector's elements into compressed form, replacing them by zeros.
            /// For each i in [0, n-1]: x[i] = y[indx[i]]; y[indx[i]] = 0.
            /// </summary>
            /// <param name="n">Number of non-zero array elements.</param>
            /// <param name="y">Pointer to the dense complex vector y.</param>
            /// <param name="x">Pointer to the sparse complex vector x.</param>
            /// <param name="indx">Pointer to the index array.</param>
            public void Gthrz(long n, [In, Out] void* y,
                [In, Out] void* x, [In] long* indx)
                => IntelMKLNative.cblas_zgthrz_64(n, y, x, indx);

            #endregion
            #region ---- Sctr [D/Z] ----

            /// <summary>
            /// Converts a compressed sparse vector into full-storage form for double precision values.
            /// </summary>
            /// <param name="n">Number of non-zero elements in the sparse vector.</param>
            /// <param name="x">Pointer to the values of the sparse vector.</param>
            /// <param name="indx">Pointer to the indices of the sparse vector elements.</param>
            /// <param name="y">Pointer to the dense vector to be updated.</param>
            public void Sctr(long n, [In] double* x, [In] long* indx,
                [Out] double* y)
                => IntelMKLNative.cblas_dsctr_64(n, x, indx, y);

            /// <summary>
            /// Converts a compressed sparse vector into full-storage form for complex values.
            /// </summary>
            /// <param name="n">Number of non-zero elements in the sparse vector.</param>
            /// <param name="x">Pointer to the values of the sparse vector (complex).</param>
            /// <param name="indx">Pointer to the indices of the sparse vector elements.</param>
            /// <param name="y">Pointer to the dense vector to be updated (complex).</param>
            public void Sctr(long n, [In] void* x, [In] long* indx,
                [Out] void* y)
                => IntelMKLNative.cblas_zsctr_64(n, x, indx, y);

            #endregion

            #endregion
            #region Level 2 [DEPRECATED]

            // MKL_DEPRECATED ...

            #endregion
            #region Level 3 [DEPRECATED]

            // MKL_DEPRECATED ...

            #endregion
            #region QR routines

            #region ---- QR ----

            /// <summary>
            /// computes the QR decomposition for the matrix of a sparse
            /// linear system and calculates the solution A * x = b
            /// </summary>
            /// <param name="operation">Specifies operation op() on sparse matrix a.</param>
            /// <param name="a">Sparse matrix handle.</param>
            /// <param name="descr">Structure specifying sparse matrix properties.</param>
            /// <param name="layout">Describes the storage scheme for the dense matrix.</param>
            /// <param name="columns">Number of columns in matrix b.</param>
            /// <param name="x">Pointer to the solution array x (size at least rows * columns).</param>
            /// <param name="ldx">Specifies the leading dimension of matrix x.</param>
            /// <param name="b">Pointer to the right-hand side array b (size at least rows * columns).</param>
            /// <param name="ldb">Specifies the leading dimension of matrix b.</param>
            /// <returns>Result status of the operation.</returns>
            public SPARSE_Status QR(SPARSE_Operation operation, IntPtr a,
                SPARSE_MatrixDescr descr, SPARSE_Layout layout,
                long columns, [In, Out] double* x, long ldx,
                [In] double* b, long ldb)
                => IntelMKLNative.mkl_sparse_d_qr(operation, a,
                    descr, layout, columns, x, ldx, b, ldb);

            #endregion
            #region ---- set hint ----

            /// <summary>
            /// defines the pivot strategy for further calls 
            /// mkl_sparse_? _qr
            /// </summary>
            /// <param name="a"> sparse matrix handle </param>
            /// <param name="hint"> value specifying whether to use pivoting </param>
            /// <returns> result status </returns>
            public SPARSE_Status QR_SetHint(IntPtr a, SPARSE_QRHint hint)
                => IntelMKLNative.mkl_sparse_set_qr_hint(a, hint);

            #endregion
            #region ---- reorder ----

            /// <summary>
            /// reordering step of SPARSE QR solver
            /// </summary>
            /// <param name="a"> sparse matrix handle </param>
            /// <param name="descr"> structure specifying sparse matrix properties </param>
            /// <returns> result status </returns>
            public SPARSE_Status QR_Reorder(IntPtr a, SPARSE_MatrixDescr descr)
                => IntelMKLNative.mkl_sparse_qr_reorder(a, descr);

            #endregion
            #region ---- factorize ----

            /// <summary>
            /// factorization step of SPARSE QR solver
            /// </summary>
            /// <param name="a">Sparse matrix handle.</param>
            /// <param name="alt_values">
            /// Pointer to an array with alternative values; must be the size of the non-zeroes in the initial input matrix.
            /// </param>
            /// <returns>
            /// <see cref="SPARSE_Status"/> indicating the result of the operation.
            /// </returns>
            public SPARSE_Status QR_Factorize(IntPtr a, double* alt_values)
                => IntelMKLNative.mkl_sparse_d_qr_factorize(a, alt_values);

            #endregion
            #region ---- solve ----

            /// <summary>
            /// Solves a system of linear equations A * x = b using the QR decomposition for a sparse matrix.
            /// </summary>
            /// <param name="operation">Specifies the operation op() to be performed on the sparse matrix <paramref name="a"/>.</param>
            /// <param name="a">Handle to the sparse matrix.</param>
            /// <param name="alt_values">Pointer to an array of alternative values for the matrix (can be null).</param>
            /// <param name="layout">Describes the storage scheme for the dense matrix (row-major or column-major).</param>
            /// <param name="columns">Number of columns in the right-hand side matrix <paramref name="b"/>.</param>
            /// <param name="x">Pointer to the solution array (output), with size at least rows * columns.</param>
            /// <param name="ldx">Leading dimension of the solution matrix <paramref name="x"/>.</param>
            /// <param name="b">Pointer to the right-hand side array, with size at least rows * columns.</param>
            /// <param name="ldb">Leading dimension of the right-hand side matrix <paramref name="b"/>.</param>
            /// <returns>Status code indicating the result of the operation.</returns>
            public SPARSE_Status QR_Solve(SPARSE_Operation operation, IntPtr a,
                double* alt_values, SPARSE_Layout layout,
                long columns, [In, Out] double* x, long ldx,
                [In] double* b, long ldb)
                => IntelMKLNative.mkl_sparse_d_qr_solve(operation, a,
                    alt_values, layout, columns, x, ldx, b, ldb);

            #endregion
            #region ---- qmult ----

            /// <summary>
            /// Computes the matrix product Q * x or Q<sup>T</sup> * x for the QR decomposition of a sparse matrix.
            /// </summary>
            /// <param name="operation">Specifies the operation op() on the sparse matrix <paramref name="a"/>.</param>
            /// <param name="a">Handle to the sparse matrix.</param>
            /// <param name="layout">Describes the storage scheme for the dense matrix.</param>
            /// <param name="columns">Number of columns in matrix <paramref name="b"/>.</param>
            /// <param name="x">Pointer to the input matrix x (size: rows * columns).</param>
            /// <param name="ldx">Specifies the leading dimension of matrix x.</param>
            /// <param name="b">Pointer to the output matrix b (size: rows * columns).</param>
            /// <param name="ldb">Specifies the leading dimension of matrix b.</param>
            /// <returns>Status of the operation as <see cref="SPARSE_Status"/>.</returns>
            public SPARSE_Status QR_QMult(SPARSE_Operation operation, IntPtr a,
                SPARSE_Layout layout, long columns, [In, Out] double* x, long ldx,
                [In] double* b, long ldb)
                => IntelMKLNative.mkl_sparse_d_qr_qmult(operation, a,
                    layout, columns, x, ldx, b, ldb);

            #endregion
            #region ---- rsolve ----

            /// <summary>
            /// Performs the second stage of the solving step of the SPARSE QR solver.
            /// Solves the system R * x = b, where R is the upper triangular matrix from the QR factorization.
            /// </summary>
            /// <param name="operation">Specifies the operation op() on the sparse matrix <paramref name="a"/>.</param>
            /// <param name="a">Sparse matrix handle.</param>
            /// <param name="layout">Describes the storage scheme for the dense matrix.</param>
            /// <param name="columns">Number of columns in matrix <paramref name="b"/>.</param>
            /// <param name="x">Pointer to the solution array with size of at least rows * columns.</param>
            /// <param name="ldx">Specifies the leading dimension of matrix <paramref name="x"/>.</param>
            /// <param name="b">Pointer to the right-hand side array with size of at least rows * columns.</param>
            /// <param name="ldb">Specifies the leading dimension of matrix <paramref name="b"/>.</param>
            /// <returns>Returns the result status of the operation.</returns>
            public SPARSE_Status QR_RSolve(SPARSE_Operation operation, IntPtr a,
                SPARSE_Layout layout, long columns, [In, Out] double* x, long ldx,
                [In] double* b, long ldb)
                => IntelMKLNative.mkl_sparse_d_qr_rsolve(operation, a,
                    layout, columns, x, ldx, b, ldb);

            #endregion

            #endregion
            #region inspector-executer

            // manipulation routines
            #region ---- CreateCOO [D/Z] ----

            /// <summary>
            /// Creates a handle for a sparse matrix in COO format with double values.
            /// </summary>
            /// <param name="a">Reference to the sparse matrix handle to be created.</param>
            /// <param name="indexing">Specifies zero-based (C-style) or one-based (Fortran-style) indexing.</param>
            /// <param name="rows">Number of rows of the matrix.</param>
            /// <param name="cols">Number of columns of the matrix.</param>
            /// <param name="nnz">Number of non-zero elements in the matrix.</param>
            /// <param name="row_indx">Pointer to the array of row indices for non-zero elements.</param>
            /// <param name="col_indx">Pointer to the array of column indices for non-zero elements.</param>
            /// <param name="values">Pointer to the array of non-zero values.</param>
            /// <returns>Status of the sparse matrix creation operation.</returns>
            public SPARSE_Status CreateCOO(ref IntPtr a,
                SPARSE_IndexBase indexing, long rows, long cols, long nnz,
                [In] long* row_indx, [In] long* col_indx, [In] double* values)
                => IntelMKLNative.mkl_sparse_d_create_coo_64(ref a,
                    indexing, rows, cols, nnz,
                    row_indx, col_indx, values);

            /// <summary>
            /// Creates a handle for a sparse matrix in COO format with complex values.
            /// </summary>
            /// <param name="a">Reference to the sparse matrix handle to be created.</param>
            /// <param name="indexing">Specifies zero-based (C-style) or one-based (Fortran-style) indexing.</param>
            /// <param name="rows">Number of rows of the matrix.</param>
            /// <param name="cols">Number of columns of the matrix.</param>
            /// <param name="nnz">Number of non-zero elements in the matrix.</param>
            /// <param name="row_indx">Pointer to the array of row indices for non-zero elements.</param>
            /// <param name="col_indx">Pointer to the array of column indices for non-zero elements.</param>
            /// <param name="values">Pointer to the array of non-zero values (complex type).</param>
            /// <returns>Status of the sparse matrix creation operation.</returns>
            public SPARSE_Status CreateCOO(ref IntPtr a,
                SPARSE_IndexBase indexing, long rows, long cols, long nnz,
                [In] long* row_indx, [In] long* col_indx, [In] void* values)
                => IntelMKLNative.mkl_sparse_z_create_coo_64(ref a,
                    indexing, rows, cols, nnz,
                    row_indx, col_indx, values);

            #endregion
            #region ---- CreateCSR [D/Z] ----

            /// <summary>
            /// Creates a handle for a sparse matrix in CSR format with double values.
            /// </summary>
            /// <param name="a">Reference to the sparse matrix handle to be created.</param>
            /// <param name="indexing">Specifies zero-based (C-style) or one-based (Fortran-style) indexing.</param>
            /// <param name="rows">Number of rows of the matrix.</param>
            /// <param name="cols">Number of columns of the matrix.</param>
            /// <param name="rows_start">Pointer to the array of row start indices (size: rows).</param>
            /// <param name="rows_end">Pointer to the array of row end indices (size: rows).</param>
            /// <param name="col_indx">Pointer to the array of column indices of the non-zero elements.</param>
            /// <param name="values">Pointer to the array of non-zero values (double).</param>
            /// <returns>Status of the sparse matrix creation operation.</returns>
            public SPARSE_Status CreateCSR(ref IntPtr a,
                SPARSE_IndexBase indexing, long rows, long cols,
                [In] long* rows_start, [In] long* rows_end,
                [In] long* col_indx, [In] double* values)
                => IntelMKLNative.mkl_sparse_d_create_csr_64(ref a,
                    indexing, rows, cols, rows_start, rows_end,
                    col_indx, values);

            /// <summary>
            /// Creates a handle for a sparse matrix in CSR format with complex values.
            /// </summary>
            /// <param name="a">Reference to the sparse matrix handle to be created.</param>
            /// <param name="indexing">Specifies zero-based (C-style) or one-based (Fortran-style) indexing.</param>
            /// <param name="rows">Number of rows of the matrix.</param>
            /// <param name="cols">Number of columns of the matrix.</param>
            /// <param name="rows_start">Pointer to the array of row start indices (size: rows).</param>
            /// <param name="rows_end">Pointer to the array of row end indices (size: rows).</param>
            /// <param name="col_indx">Pointer to the array of column indices of the non-zero elements.</param>
            /// <param name="values">Pointer to the array of non-zero values (complex, as void*).</param>
            /// <returns>Status of the sparse matrix creation operation.</returns>
            public SPARSE_Status CreateCSR(ref IntPtr a,
                SPARSE_IndexBase indexing, long rows, long cols,
                [In] long* rows_start, [In] long* rows_end,
                [In] long* col_indx, [In] void* values)
                => IntelMKLNative.mkl_sparse_z_create_csr_64(ref a,
                    indexing, rows, cols, rows_start, rows_end,
                    col_indx, values);

            #endregion
            #region ---- CreateCSC [D/Z] ----

            /// <summary>
            /// Creates a handle for a sparse matrix in CSC (Compressed Sparse Column) format with double values.
            /// </summary>
            /// <param name="a">Reference to the sparse matrix handle to be created.</param>
            /// <param name="indexing">Specifies zero-based (C-style) or one-based (Fortran-style) indexing.</param>
            /// <param name="rows">Number of rows in the matrix.</param>
            /// <param name="cols">Number of columns in the matrix.</param>
            /// <param name="cols_start">Pointer to the array of column start indices.</param>
            /// <param name="cols_end">Pointer to the array of column end indices.</param>
            /// <param name="row_indx">Pointer to the array of row indices for non-zero elements.</param>
            /// <param name="values">Pointer to the array of non-zero values (double).</param>
            /// <returns>Status of the sparse matrix creation operation.</returns>
            public SPARSE_Status CreateCSC(ref IntPtr a,
                SPARSE_IndexBase indexing, long rows, long cols,
                [In] long* cols_start, [In] long* cols_end,
                [In] long* row_indx, [In] double* values)
                => IntelMKLNative.mkl_sparse_d_create_csc_64(ref a,
                    indexing, rows, cols, cols_start, cols_end, row_indx, values);

            /// <summary>
            /// Creates a handle for a sparse matrix in CSC (Compressed Sparse Column) format with complex values.
            /// </summary>
            /// <param name="a">Reference to the sparse matrix handle to be created.</param>
            /// <param name="indexing">Specifies zero-based (C-style) or one-based (Fortran-style) indexing.</param>
            /// <param name="rows">Number of rows in the matrix.</param>
            /// <param name="cols">Number of columns in the matrix.</param>
            /// <param name="cols_start">Pointer to the array of column start indices.</param>
            /// <param name="cols_end">Pointer to the array of column end indices.</param>
            /// <param name="row_indx">Pointer to the array of row indices for non-zero elements.</param>
            /// <param name="values">Pointer to the array of non-zero values (complex, as void*).</param>
            /// <returns>Status of the sparse matrix creation operation.</returns>
            public SPARSE_Status CreateCSC(ref IntPtr a,
                SPARSE_IndexBase indexing, long rows, long cols,
                [In] long* cols_start, [In] long* cols_end,
                [In] long* row_indx, [In] void* values)
                => IntelMKLNative.mkl_sparse_z_create_csc_64(ref a,
                    indexing, rows, cols, cols_start, cols_end, row_indx, values);

            #endregion
            #region ---- Copy ----

            /// <summary>
            /// creates a copy of a sparse matrix handle
            /// </summary>
            /// <param name="source"> handle of the source sparse matrix </param>
            /// <param name="descr"> structure specifying sparse matrix properties </param>
            /// <param name="dest"> copied handle containing internal data </param>
            /// <returns> result status </returns>
            public SPARSE_Status Copy(IntPtr source, SPARSE_MatrixDescr descr,
                ref IntPtr dest)
                => IntelMKLNative.mkl_sparse_copy_64(source, descr, ref dest);

            #endregion
            #region ---- Destroy ----

            /// <summary>
            /// frees memory allocated for a sparse matrix handle
            /// </summary>
            /// <param name="a"> handle of the sparse matrix </param>
            /// <returns> result status </returns>
            public SPARSE_Status Destroy(IntPtr a)
                => IntelMKLNative.mkl_sparse_destroy_64(a);

            #endregion
            #region ---- Convert ----

            /// <summary>
            /// converts internal matrix representation to CSR format
            /// </summary>
            /// <param name="source"> handle of the source sparse matrix </param>
            /// <param name="operation"> specifies operation op() on input matrix </param>
            /// <param name="dest"> result handle containing internal data </param>
            /// <returns> result status </returns>
            public SPARSE_Status ConvertCSR(IntPtr source, SPARSE_Operation operation,
                ref IntPtr dest)
                => IntelMKLNative.mkl_sparse_convert_csr_64(source, operation, ref dest);

            #endregion
            #region ---- ExportCSR [D/Z] ---

            /// <summary>
            /// Exports a sparse matrix in CSR format with double values.
            /// </summary>
            /// <param name="a">Sparse matrix handle.</param>
            /// <param name="indexing">Zero-based (C-style) or one-based (Fortran-style) indexing.</param>
            /// <param name="rows">Pointer to the number of rows of the matrix.</param>
            /// <param name="cols">Pointer to the number of columns of the matrix.</param>
            /// <param name="row_start">Pointer to the array of row start indices.</param>
            /// <param name="row_end">Pointer to the array of row end indices.</param>
            /// <param name="col_indx">Pointer to the array of column indices.</param>
            /// <param name="values">Pointer to the array of non-zero values.</param>
            /// <returns>Status of the export operation.</returns>
            public SPARSE_Status ExportCSR(IntPtr a,
                ref SPARSE_IndexBase indexing, long* rows, long* cols,
                long** row_start, long** row_end,
                long** col_indx, double** values)
                => IntelMKLNative.mkl_sparse_d_export_csr_64(a,
                    ref indexing, rows, cols,
                    row_start, row_end,
                    col_indx, values);

            /// <summary>
            /// Exports a sparse matrix in CSR format with complex values.
            /// </summary>
            /// <param name="a">Sparse matrix handle.</param>
            /// <param name="indexing">Zero-based (C-style) or one-based (Fortran-style) indexing.</param>
            /// <param name="rows">Pointer to the number of rows of the matrix.</param>
            /// <param name="cols">Pointer to the number of columns of the matrix.</param>
            /// <param name="row_start">Pointer to the array of row start indices.</param>
            /// <param name="row_end">Pointer to the array of row end indices.</param>
            /// <param name="col_indx">Pointer to the array of column indices.</param>
            /// <param name="values">Pointer to the array of non-zero values (complex type).</param>
            /// <returns>Status of the export operation.</returns>
            public SPARSE_Status ExportCSR(IntPtr a,
                ref SPARSE_IndexBase indexing, long* rows, long* cols,
                long** row_start, long** row_end,
                long** col_indx, void** values)
                => IntelMKLNative.mkl_sparse_z_export_csr_64(a,
                    ref indexing, rows, cols,
                    row_start, row_end,
                    col_indx, values);

            #endregion
            #region ---- SetValue [D/Z] ----

            /// <summary>
            /// changes a single value of matrix in internal representation
            /// </summary>
            /// <param name="a"> sparse matrix handle </param>
            /// <param name="row"> indicates row of matrix in which to set value </param>
            /// <param name="col"> indicates column of matrix in which to set value </param>
            /// <param name="value"> target value </param>
            /// <returns> result status </returns>
            public SPARSE_Status SetValue(IntPtr a, long row, long col, Real value)
                => IntelMKLNative.mkl_sparse_d_set_value_64(a, row, col, value);

            /// <summary>
            /// changes a single value of matrix in internal representation
            /// </summary>
            /// <param name="a"> sparse matrix handle </param>
            /// <param name="row"> indicates row of matrix in which to set value </param>
            /// <param name="col"> indicates column of matrix in which to set value </param>
            /// <param name="value"> target value </param>
            /// <returns> result status </returns>
            public SPARSE_Status SetValue(IntPtr a, long row, long col, Cplx value)
                => IntelMKLNative.mkl_sparse_z_set_value_64(a, row, col, value);

            #endregion
            #region ---- UpdateValues [D/Z] ----

            /// <summary>
            /// Changes all or selected matrix values in internal representation for a real-valued sparse matrix.
            /// </summary>
            /// <param name="a">Sparse matrix handle.</param>
            /// <param name="nvalues">Total number of elements to change.</param>
            /// <param name="indx">Pointer to the row indices for the new values.</param>
            /// <param name="indy">Pointer to the column indices for the new values.</param>
            /// <param name="values">Pointer to the new values.</param>
            /// <returns>Result status of the update operation.</returns>
            public SPARSE_Status UpdateValues(IntPtr a, long nvalues,
                long* indx, long* indy, double* values)
                => IntelMKLNative.mkl_sparse_d_update_values_64(a,
                    nvalues, indx, indy, values);

            /// <summary>
            /// Changes all or selected matrix values in internal representation for a complex-valued sparse matrix.
            /// </summary>
            /// <param name="a">Sparse matrix handle.</param>
            /// <param name="nvalues">Total number of elements to change.</param>
            /// <param name="indx">Pointer to the row indices for the new values.</param>
            /// <param name="indy">Pointer to the column indices for the new values.</param>
            /// <param name="values">Pointer to the new complex values.</param>
            /// <returns>Result status of the update operation.</returns>
            public SPARSE_Status UpdateValues(IntPtr a, long nvalues,
                long* indx, long* indy, void* values)
                => IntelMKLNative.mkl_sparse_z_update_values_64(a,
                    nvalues, indx, indy, values);

            #endregion
            #region ---- Order ----

            /// <summary>
            /// performs ordering of column indexes of the matrix in CSR format
            /// </summary>
            /// <param name="a"> sparse matrix handle </param>
            /// <returns> result status </returns>
            public SPARSE_Status Order(IntPtr a)
                => IntelMKLNative.mkl_sparse_order_64(a);

            #endregion
            // analysis routines
            // ...
            // execution routines
            #region ---- Optimize ----

            /// <summary>
            /// optimize matrix described by the handle
            /// </summary>
            /// <param name="a"> sparse matrix handle </param>
            /// <returns> result status </returns>
            public SPARSE_Status Optimize(IntPtr a)
                => IntelMKLNative.mkl_sparse_optimize_64(a);

            #endregion
            #region ---- Mv [D/Z] ----

            /// <summary>
            /// Computes the sparse matrix-vector product for a real-valued sparse matrix.
            /// </summary>
            /// <param name="operation">Specifies the operation op() to apply to the sparse matrix <paramref name="a"/>.</param>
            /// <param name="alpha">Scalar multiplier for the matrix-vector product.</param>
            /// <param name="a">Handle to the sparse matrix.</param>
            /// <param name="descr">Descriptor specifying the properties of the sparse matrix.</param>
            /// <param name="x">Pointer to the input dense vector.</param>
            /// <param name="beta">Scalar multiplier for the output vector <paramref name="y"/>.</param>
            /// <param name="y">Pointer to the output dense vector, which is updated in place.</param>
            /// <returns>Status code indicating the result of the operation.</returns>
            public SPARSE_Status Mv(SPARSE_Operation operation,
                double alpha, [In] IntPtr a, SPARSE_MatrixDescr descr,
                [In] double* x, double beta, [In, Out] double* y)
                => IntelMKLNative.mkl_sparse_d_mv_64(operation,
                    alpha, a, descr, x, beta, y);

            /// <summary>
            /// Computes the sparse matrix-vector product for a complex-valued sparse matrix.
            /// </summary>
            /// <param name="operation">Specifies the operation op() to apply to the sparse matrix <paramref name="a"/>.</param>
            /// <param name="alpha">Pointer to the scalar multiplier for the matrix-vector product (complex value).</param>
            /// <param name="a">Handle to the sparse matrix.</param>
            /// <param name="descr">Descriptor specifying the properties of the sparse matrix.</param>
            /// <param name="x">Pointer to the input dense vector (complex values).</param>
            /// <param name="beta">Pointer to the scalar multiplier for the output vector <paramref name="y"/> (complex value).</param>
            /// <param name="y">Pointer to the output dense vector (complex values), which is updated in place.</param>
            /// <returns>Status code indicating the result of the operation.</returns>
            public SPARSE_Status Mv(SPARSE_Operation operation,
                void* alpha, [In] IntPtr a, SPARSE_MatrixDescr descr,
                [In] void* x, void* beta, [In, Out] void* y)
                => IntelMKLNative.mkl_sparse_z_mv_64(operation,
                    alpha, a, descr, x, beta, y);

            #endregion
            #region ---- DotMv [D/Z] ----

            /// <summary>
            /// Computes y = alpha * op(A) * x + beta * y and d = (x, y) - the l2 inner product
            /// for a sparse matrix and dense vectors (double precision).
            /// </summary>
            /// <param name="operation">Specifies the operation op() on the sparse matrix a.</param>
            /// <param name="alpha">Scalar constant alpha.</param>
            /// <param name="a">Handle to the sparse matrix.</param>
            /// <param name="descr">Structure specifying sparse matrix properties.</param>
            /// <param name="x">Pointer to the dense input vector x.</param>
            /// <param name="beta">Scalar constant beta.</param>
            /// <param name="y">Pointer to the dense output vector y (in/out).</param>
            /// <param name="d">Pointer to the result inner product (output).</param>
            /// <returns>Status of the operation.</returns>
            public SPARSE_Status DotMv(SPARSE_Operation operation,
                double alpha, [In] IntPtr a, SPARSE_MatrixDescr descr,
                [In] double* x, double beta,
                [In, Out] double* y, double* d)
                => IntelMKLNative.mkl_sparse_d_dotmv_64(operation,
                    alpha, a, descr, x, beta, y, d);

            /// <summary>
            /// Computes y = alpha * op(A) * x + beta * y and d = (x, y) - the l2 inner product
            /// for a sparse matrix and dense vectors (complex precision).
            /// </summary>
            /// <param name="operation">Specifies the operation op() on the sparse matrix a.</param>
            /// <param name="alpha">Pointer to the scalar constant alpha (complex).</param>
            /// <param name="a">Handle to the sparse matrix.</param>
            /// <param name="descr">Structure specifying sparse matrix properties.</param>
            /// <param name="x">Pointer to the dense input vector x (complex).</param>
            /// <param name="beta">Pointer to the scalar constant beta (complex).</param>
            /// <param name="y">Pointer to the dense output vector y (complex, in/out).</param>
            /// <param name="d">Pointer to the result inner product (complex, output).</param>
            /// <returns>Status of the operation.</returns>
            public SPARSE_Status DotMv(SPARSE_Operation operation,
                void* alpha, [In] IntPtr a, SPARSE_MatrixDescr descr,
                [In] void* x, void* beta,
                [In, Out] void* y, void* d)
                => IntelMKLNative.mkl_sparse_z_dotmv_64(operation,
                    alpha, a, descr, x, beta, y, d);

            #endregion
            #region ---- Trsv [D/Z] ----

            /// <summary>
            /// Solves a triangular sparse linear system y = alpha * A^{-1} * x for real-valued matrices.
            /// </summary>
            /// <param name="operation">Specifies the operation op() on the sparse matrix a.</param>
            /// <param name="alpha">Scalar constant alpha.</param>
            /// <param name="a">Handle to the sparse matrix.</param>
            /// <param name="descr">Structure specifying sparse matrix properties.</param>
            /// <param name="x">Pointer to the input dense vector x.</param>
            /// <param name="y">Pointer to the output dense vector y.</param>
            /// <returns>Status of the sparse operation.</returns>
            public SPARSE_Status Trsv(SPARSE_Operation operation,
                double alpha, [In] IntPtr a, SPARSE_MatrixDescr descr,
                [In] double* x, [Out] double* y)
                => IntelMKLNative.mkl_sparse_d_trsv_64(operation,
                    alpha, a, descr, x, y);

            /// <summary>
            /// Solves a triangular sparse linear system y = alpha * A^{-1} * x for complex-valued matrices.
            /// </summary>
            /// <param name="operation">Specifies the operation op() on the sparse matrix a.</param>
            /// <param name="alpha">Pointer to the scalar constant alpha (complex).</param>
            /// <param name="a">Handle to the sparse matrix.</param>
            /// <param name="descr">Structure specifying sparse matrix properties.</param>
            /// <param name="x">Pointer to the input dense vector x (complex).</param>
            /// <param name="y">Pointer to the output dense vector y (complex).</param>
            /// <returns>Status of the sparse operation.</returns>
            public SPARSE_Status Trsv(SPARSE_Operation operation,
                void* alpha, [In] IntPtr a, SPARSE_MatrixDescr descr,
                [In] void* x, [Out] void* y)
                => IntelMKLNative.mkl_sparse_z_trsv_64(operation,
                    alpha, a, descr, x, y);

            #endregion
            #region ---- Symgs [D/Z] ----

            /// <summary>
            /// Applies the symmetric Gauss-Seidel preconditioner to a symmetric system A * x = b
            /// for a real-valued sparse matrix.
            /// </summary>
            /// <param name="operation">Specifies the operation op() on the sparse matrix a.</param>
            /// <param name="a">Handle to the sparse matrix.</param>
            /// <param name="descr">Structure specifying sparse matrix properties.</param>
            /// <param name="alpha">Scalar constant alpha.</param>
            /// <param name="b">Pointer to the dense vector b.</param>
            /// <param name="x">Pointer to the dense vector x (output).</param>
            /// <returns>Result status of the operation.</returns>
            public SPARSE_Status Symgs(SPARSE_Operation operation,
                [In] IntPtr a, SPARSE_MatrixDescr descr,
                double alpha, [In] double* b, [Out] double* x)
                => IntelMKLNative.mkl_sparse_d_symgs_64(operation,
                    a, descr, alpha, b, x);

            /// <summary>
            /// Applies the symmetric Gauss-Seidel preconditioner to a symmetric system A * x = b
            /// for a complex-valued sparse matrix.
            /// </summary>
            /// <param name="operation">Specifies the operation op() on the sparse matrix a.</param>
            /// <param name="a">Handle to the sparse matrix.</param>
            /// <param name="descr">Structure specifying sparse matrix properties.</param>
            /// <param name="alpha">Pointer to the scalar constant alpha (complex).</param>
            /// <param name="b">Pointer to the dense vector b (complex).</param>
            /// <param name="x">Pointer to the dense vector x (complex, output).</param>
            /// <returns>Result status of the operation.</returns>
            public SPARSE_Status Symgs(SPARSE_Operation operation,
                [In] IntPtr a, SPARSE_MatrixDescr descr,
                void* alpha, [In] void* b, [Out] void* x)
                => IntelMKLNative.mkl_sparse_z_symgs_64(operation,
                    a, descr, alpha, b, x);

            #endregion
            #region ---- SymgsMv [D/Z] ----

            /// <summary>
            /// Applies the symmetric Gauss-Seidel preconditioner to a symmetric system A * x = b,
            /// followed by a matrix-vector product, and returns y = A * x for double-precision values.
            /// </summary>
            /// <param name="operation">Specifies the operation op() on the sparse matrix a.</param>
            /// <param name="a">Handle to the sparse matrix.</param>
            /// <param name="descr">Structure specifying sparse matrix properties.</param>
            /// <param name="alpha">Scalar constant alpha.</param>
            /// <param name="b">Pointer to the dense vector b.</param>
            /// <param name="x">Pointer to the dense vector x (input/output).</param>
            /// <param name="y">Pointer to the dense vector y (output).</param>
            /// <returns>Status of the operation as <see cref="SPARSE_Status"/>.</returns>
            public SPARSE_Status SymgsMv(SPARSE_Operation operation,
                [In] IntPtr a, SPARSE_MatrixDescr descr,
                double alpha, [In] double* b, [In, Out] double* x,
                [Out] double* y)
                => IntelMKLNative.mkl_sparse_d_symgs_mv_64(operation,
                    a, descr, alpha, b, x, y);

            /// <summary>
            /// Applies the symmetric Gauss-Seidel preconditioner to a symmetric system A * x = b,
            /// followed by a matrix-vector product, and returns y = A * x for complex values.
            /// </summary>
            /// <param name="operation">Specifies the operation op() on the sparse matrix a.</param>
            /// <param name="a">Handle to the sparse matrix.</param>
            /// <param name="descr">Structure specifying sparse matrix properties.</param>
            /// <param name="alpha">Pointer to the scalar constant alpha (complex).</param>
            /// <param name="b">Pointer to the dense vector b (complex).</param>
            /// <param name="x">Pointer to the dense vector x (complex, input/output).</param>
            /// <param name="y">Pointer to the dense vector y (complex, output).</param>
            /// <returns>Status of the operation as <see cref="SPARSE_Status"/>.</returns>
            public SPARSE_Status SymgsMv(SPARSE_Operation operation,
                [In] IntPtr a, SPARSE_MatrixDescr descr,
                void* alpha, [In] void* b, [In, Out] void* x,
                [Out] void* y)
                => IntelMKLNative.mkl_sparse_z_symgs_mv_64(operation,
                    a, descr, alpha, b, x, y);

            #endregion
            #region ---- LU smoother ---- 

            // ...

            #endregion
            #region ---- Mm [D/Z] ----

            /// <summary>
            /// Computes the product of a sparse matrix and a dense matrix and stores the result as a dense matrix.
            /// y = alpha * op(A) * x + beta * y
            /// </summary>
            /// <param name="operation">Specifies the operation op() on the sparse matrix a.</param>
            /// <param name="alpha">Scalar alpha.</param>
            /// <param name="a">Sparse matrix handle.</param>
            /// <param name="descr">Structure specifying sparse matrix properties.</param>
            /// <param name="layout">Describes the storage scheme for the dense matrix.</param>
            /// <param name="x">Pointer to the dense matrix x.</param>
            /// <param name="columns">Number of columns of matrix y.</param>
            /// <param name="ldx">Leading dimension of matrix x.</param>
            /// <param name="beta">Scalar beta.</param>
            /// <param name="y">Pointer to the dense matrix y.</param>
            /// <param name="ldy">Leading dimension of matrix y.</param>
            /// <returns>Result status.</returns>
            public SPARSE_Status Mm(SPARSE_Operation operation,
                double alpha, [In] IntPtr a, SPARSE_MatrixDescr descr,
                SPARSE_Layout layout, [In] double* x, long columns, long ldx,
                double beta, [In, Out] double* y, long ldy)
                => IntelMKLNative.mkl_sparse_d_mm_64(operation,
                    alpha, a, descr, layout, x, columns, ldx, beta, y, ldy);

            /// <summary>
            /// Computes the product of a sparse complex matrix and a dense complex matrix and stores the result as a dense matrix.
            /// y = alpha * op(A) * x + beta * y
            /// </summary>
            /// <param name="operation">Specifies the operation op() on the sparse matrix a.</param>
            /// <param name="alpha">Pointer to the scalar alpha (complex).</param>
            /// <param name="a">Sparse matrix handle.</param>
            /// <param name="descr">Structure specifying sparse matrix properties.</param>
            /// <param name="layout">Describes the storage scheme for the dense matrix.</param>
            /// <param name="x">Pointer to the dense matrix x (complex).</param>
            /// <param name="columns">Number of columns of matrix y.</param>
            /// <param name="ldx">Leading dimension of matrix x.</param>
            /// <param name="beta">Pointer to the scalar beta (complex).</param>
            /// <param name="y">Pointer to the dense matrix y (complex).</param>
            /// <param name="ldy">Leading dimension of matrix y.</param>
            /// <returns>Result status.</returns>
            public SPARSE_Status Mm(SPARSE_Operation operation,
                void* alpha, [In] IntPtr a, SPARSE_MatrixDescr descr,
                SPARSE_Layout layout, [In] void* x, long columns, long ldx,
                void* beta, [In, Out] void* y, long ldy)
                => IntelMKLNative.mkl_sparse_z_mm_64(operation,
                    alpha, a, descr, layout, x, columns, ldx, beta, y, ldy);

            #endregion
            #region ---- Trsm [D/Z] ----

            /// <summary>
            /// Solves a system of linear equations with multiple right hand sides for a triangular sparse matrix.
            /// Computes <c>y = alpha * op(A)^{-1} * x</c> for a real-valued matrix.
            /// </summary>
            /// <param name="operation">Specifies the operation <c>op()</c> on the sparse matrix <paramref name="a"/>.</param>
            /// <param name="alpha">Scalar multiplier for the solution.</param>
            /// <param name="a">Handle to the sparse matrix.</param>
            /// <param name="descr">Structure specifying sparse matrix properties.</param>
            /// <param name="layout">Describes the storage scheme for the dense matrix.</param>
            /// <param name="x">Pointer to the input dense matrix <c>x</c>.</param>
            /// <param name="columns">Number of columns in matrix <c>y</c>.</param>
            /// <param name="ldx">Leading dimension of matrix <c>x</c>.</param>
            /// <param name="y">Pointer to the output dense matrix <c>y</c>.</param>
            /// <param name="ldy">Leading dimension of matrix <c>y</c>.</param>
            /// <returns>Result status of the operation.</returns>
            public SPARSE_Status Trsm(SPARSE_Operation operation,
                double alpha, [In] IntPtr a, SPARSE_MatrixDescr descr,
                SPARSE_Layout layout, [In] double* x, long columns, long ldx,
                [In, Out] double* y, long ldy)
                => IntelMKLNative.mkl_sparse_d_trsm_64(operation,
                    alpha, a, descr, layout, x, columns, ldx, y, ldy);

            /// <summary>
            /// Solves a system of linear equations with multiple right hand sides for a triangular sparse matrix.
            /// Computes <c>y = alpha * op(A)^{-1} * x</c> for a complex-valued matrix.
            /// </summary>
            /// <param name="operation">Specifies the operation <c>op()</c> on the sparse matrix <paramref name="a"/>.</param>
            /// <param name="alpha">Pointer to the scalar multiplier for the solution (complex value).</param>
            /// <param name="a">Handle to the sparse matrix.</param>
            /// <param name="descr">Structure specifying sparse matrix properties.</param>
            /// <param name="layout">Describes the storage scheme for the dense matrix.</param>
            /// <param name="x">Pointer to the input dense matrix <c>x</c> (complex values).</param>
            /// <param name="columns">Number of columns in matrix <c>y</c>.</param>
            /// <param name="ldx">Leading dimension of matrix <c>x</c>.</param>
            /// <param name="y">Pointer to the output dense matrix <c>y</c> (complex values).</param>
            /// <param name="ldy">Leading dimension of matrix <c>y</c>.</param>
            /// <returns>Result status of the operation.</returns>
            public SPARSE_Status Trsm(SPARSE_Operation operation,
                void* alpha, [In] IntPtr a, SPARSE_MatrixDescr descr,
                SPARSE_Layout layout, [In] void* x, long columns, long ldx,
                [In, Out] void* y, long ldy)
                => IntelMKLNative.mkl_sparse_z_trsm_64(operation,
                    alpha, a, descr, layout, x, columns, ldx, y, ldy);

            #endregion
            #region ---- Add [D/Z] ----

            /// <summary>
            /// Computes the sum of two sparse matrices for double-precision values.
            /// C = alpha * op(A) + B, result is sparse
            /// </summary>
            /// <param name="operation">Specifies the operation op() on sparse matrix a.</param>
            /// <param name="a">Handle to the first sparse matrix.</param>
            /// <param name="alpha">Scalar multiplier for matrix a.</param>
            /// <param name="b">Handle to the second sparse matrix.</param>
            /// <param name="c">Reference to the handle for the resulting sparse matrix. The result is stored in a newly allocated matrix.</param>
            /// <returns>Status of the sparse operation.</returns>
            public SPARSE_Status Add(SPARSE_Operation operation,
                [In] IntPtr a, double alpha, [In] IntPtr b,
                ref IntPtr c)
                => IntelMKLNative.mkl_sparse_d_add_64(operation,
                    a, alpha, b, ref c);

            /// <summary>
            /// Computes the sum of two sparse matrices for complex values.
            /// C = alpha * op(A) + B, result is sparse
            /// </summary>
            /// <param name="operation">Specifies the operation op() on sparse matrix a.</param>
            /// <param name="a">Handle to the first sparse matrix.</param>
            /// <param name="alpha">Pointer to the scalar multiplier for matrix a (complex value).</param>
            /// <param name="b">Handle to the second sparse matrix.</param>
            /// <param name="c">Reference to the handle for the resulting sparse matrix. The result is stored in a newly allocated matrix.</param>
            /// <returns>Status of the sparse operation.</returns>
            public SPARSE_Status Add(SPARSE_Operation operation,
                [In] IntPtr a, void* alpha, [In] IntPtr b,
                ref IntPtr c)
                => IntelMKLNative.mkl_sparse_z_add_64(operation,
                    a, alpha, b, ref c);

            #endregion
            #region ---- Spmm ----

            /// <summary>
            /// Computes the product of two sparse matrices.
            /// C = op(A) * B, result is sparse
            /// </summary>
            /// <param name="operation">Specifies the operation op() on sparse matrix a.</param>
            /// <param name="a">Handle to the first sparse matrix.</param>
            /// <param name="b">Handle to the second sparse matrix.</param>
            /// <param name="c">Reference to the handle for the resulting sparse matrix. The result is stored in a newly allocated sparse matrix.</param>
            /// <returns>Status of the sparse matrix-matrix multiplication operation.</returns>
            public SPARSE_Status Spmm(SPARSE_Operation operation,
                [In] IntPtr a, [In] IntPtr b, ref IntPtr c)
                => IntelMKLNative.mkl_sparse_spmm_64(operation, a, b, ref c);

            #endregion
            #region ---- Sp2mm ----

            /// <summary>
            /// Computes the product of two sparse matrices using the Intel MKL SPBLAS interface.
            /// C = opA(A) * opB(B)
            /// </summary>
            /// <param name="transA">Specifies the operation op() to be applied to sparse matrix <paramref name="a"/>.</param>
            /// <param name="descrA">Structure specifying properties of sparse matrix <paramref name="a"/>.</param>
            /// <param name="a">Handle to the first sparse matrix.</param>
            /// <param name="transB">Specifies the operation op() to be applied to sparse matrix <paramref name="b"/>.</param>
            /// <param name="descrB">Structure specifying properties of sparse matrix <paramref name="b"/>.</param>
            /// <param name="b">Handle to the second sparse matrix.</param>
            /// <param name="request">Specifies whether the full computation is performed at once or using a two-stage algorithm.</param>
            /// <param name="c">Reference to the handle of the resulting sparse matrix. The result is stored in a newly allocated sparse matrix.</param>
            /// <returns>Status of the sparse operation.</returns>
            public SPARSE_Status Sp2mm(SPARSE_Operation transA,
                SPARSE_MatrixDescr descrA, [In] IntPtr a,
                SPARSE_Operation transB, SPARSE_MatrixDescr descrB,
                [In] IntPtr b, SPARSE_Request request, ref IntPtr c)
                => IntelMKLNative.mkl_sparse_sp2m_64(transA, descrA, a,
                    transB, descrB, b, request, ref c);

            #endregion
            #region ---- Syrk ----

            /// <summary>
            /// Computes the product of a sparse matrix with its transpose or conjugate transpose.
            /// C = op(A) * op(A)^T (for real) or C = op(A) * op(A)^H (for complex).
            /// The result is stored in a newly allocated sparse matrix.
            /// </summary>
            /// <param name="operation">Specifies the operation op() to apply to the sparse matrix <paramref name="a"/>.</param>
            /// <param name="a">Handle to the input sparse matrix.</param>
            /// <param name="c">Reference to the handle of the output sparse matrix. The result will be stored here.</param>
            /// <returns>Status of the sparse operation.</returns>
            public SPARSE_Status Syrk(SPARSE_Operation operation,
                [In] IntPtr a, ref IntPtr c)
                => IntelMKLNative.mkl_sparse_syrk_64(operation, a, ref c);

            #endregion
            #region ---- Sypr ----

            /// <summary>
            /// Computes the symmetric product of three sparse matrices:
            /// C = op(A) * B * op(A)<sup>H</sup>,
            /// and stores the result in a newly allocated sparse matrix.
            /// </summary>
            /// <param name="transA">Specifies the operation op() on sparse matrix A.</param>
            /// <param name="a">Handle to the first sparse matrix (A).</param>
            /// <param name="b">Handle to the second sparse matrix (B).</param>
            /// <param name="descrB">Structure specifying properties of matrix B.</param>
            /// <param name="c">Reference to the handle for the resulting sparse matrix (C).</param>
            /// <param name="request">Specifies whether the full computation is performed at once or using a two-stage algorithm.</param>
            /// <returns>Status of the operation as <see cref="SPARSE_Status"/>.</returns>
            public SPARSE_Status Sypr(SPARSE_Operation transA,
                [In] IntPtr a, [In] IntPtr b, SPARSE_MatrixDescr descrB,
                ref IntPtr c, SPARSE_Request request)
                => IntelMKLNative.mkl_sparse_sypr_64(transA, a, b, descrB,
                    ref c, request);

            #endregion
            #region ---- Spmmd [D/Z] ----

            /// <summary>
            /// Computes the product of two sparse matrices and stores the result as a dense matrix (double precision).
            /// C = op(A) * B, result is dense
            /// </summary>
            /// <param name="operation">Specifies the operation op() on sparse matrix a.</param>
            /// <param name="a">Handle to the first sparse matrix.</param>
            /// <param name="b">Handle to the second sparse matrix.</param>
            /// <param name="layout">Describes the storage scheme for the dense matrix.</param>
            /// <param name="c">Pointer to the output dense matrix.</param>
            /// <param name="ldc">Leading dimension of matrix c.</param>
            /// <returns>Result status of the operation.</returns>
            public SPARSE_Status Spmmd(SPARSE_Operation operation,
                [In] IntPtr a, [In] IntPtr b, SPARSE_Layout layout,
                [Out] double* c, long ldc)
                => IntelMKLNative.mkl_sparse_d_spmmd_64(operation,
                    a, b, layout, c, ldc);

            /// <summary>
            /// Computes the product of two sparse matrices and stores the result as a dense matrix (complex precision).
            /// C = op(A) * B, result is dense
            /// </summary>
            /// <param name="operation">Specifies the operation op() on sparse matrix a.</param>
            /// <param name="a">Handle to the first sparse matrix.</param>
            /// <param name="b">Handle to the second sparse matrix.</param>
            /// <param name="layout">Describes the storage scheme for the dense matrix.</param>
            /// <param name="c">Pointer to the output dense matrix.</param>
            /// <param name="ldc">Leading dimension of matrix c.</param>
            /// <returns>Result status of the operation.</returns>
            public SPARSE_Status Spmmd(SPARSE_Operation operation,
                [In] IntPtr a, [In] IntPtr b, SPARSE_Layout layout,
                [Out] void* c, long ldc)
                => IntelMKLNative.mkl_sparse_z_spmmd_64(operation,
                    a, b, layout, c, ldc);

            #endregion
            #region ---- Sp2md [D/Z] ----

            /// <summary>
            /// Computes the product of two sparse matrices and stores the result as a dense matrix.
            /// C = alpha * opA(A) * opB(B) + beta * C
            /// </summary>
            /// <param name="transA">Specifies operation op() on sparse matrix A.</param>
            /// <param name="descrA">Structure specifying sparse matrix A's properties.</param>
            /// <param name="a">Sparse matrix A handle.</param>
            /// <param name="transB">Specifies operation op() on sparse matrix B.</param>
            /// <param name="descrB">Structure specifying sparse matrix B's properties.</param>
            /// <param name="b">Sparse matrix B handle.</param>
            /// <param name="alpha">Scalar constant alpha.</param>
            /// <param name="beta">Scalar constant beta.</param>
            /// <param name="c">Pointer to the output dense matrix C.</param>
            /// <param name="layout">Describes the storage scheme for the dense matrix.</param>
            /// <param name="ldc">Leading dimension of matrix C.</param>
            /// <returns>Result status.</returns>
            public SPARSE_Status Sp2md(SPARSE_Operation transA,
                SPARSE_MatrixDescr descrA, [In] IntPtr a,
                SPARSE_Operation transB, SPARSE_MatrixDescr descrB,
                [In] IntPtr b, double alpha, double beta,
                [In, Out] double* c, SPARSE_Layout layout, long ldc)
                => IntelMKLNative.mkl_sparse_d_sp2md_64(transA, descrA, a,
                    transB, descrB, b, alpha, beta, c, layout, ldc);

            /// <summary>
            /// Computes the product of two sparse matrices and stores the result as a dense matrix (complex version).
            /// C = alpha * opA(A) * opB(B) + beta * C
            /// </summary>
            /// <param name="transA">Specifies operation op() on sparse matrix A.</param>
            /// <param name="descrA">Structure specifying sparse matrix A's properties.</param>
            /// <param name="a">Sparse matrix A handle.</param>
            /// <param name="transB">Specifies operation op() on sparse matrix B.</param>
            /// <param name="descrB">Structure specifying sparse matrix B's properties.</param>
            /// <param name="b">Sparse matrix B handle.</param>
            /// <param name="alpha">Pointer to scalar constant alpha (complex).</param>
            /// <param name="beta">Pointer to scalar constant beta (complex).</param>
            /// <param name="c">Pointer to the output dense matrix C (complex).</param>
            /// <param name="layout">Describes the storage scheme for the dense matrix.</param>
            /// <param name="ldc">Leading dimension of matrix C.</param>
            /// <returns>Result status.</returns>
            public SPARSE_Status Sp2md(SPARSE_Operation transA,
                SPARSE_MatrixDescr descrA, [In] IntPtr a,
                SPARSE_Operation transB, SPARSE_MatrixDescr descrB,
                [In] IntPtr b, void* alpha, void* beta,
                [In, Out] void* c, SPARSE_Layout layout, long ldc)
                => IntelMKLNative.mkl_sparse_z_sp2md_64(transA, descrA, a,
                    transB, descrB, b, alpha, beta, c, layout, ldc);

            #endregion
            #region ---- Syrkd [D/Z] ----

            /// <summary>
            /// Computes the symmetric rank-k update for a sparse matrix and stores the result as a dense matrix.
            /// C = beta * C + alpha * op(A) * op(A)^T
            /// </summary>
            /// <param name="operation">Specifies the operation op() on the sparse matrix a.</param>
            /// <param name="a">Handle to the sparse matrix a.</param>
            /// <param name="alpha">Scalar constant alpha.</param>
            /// <param name="beta">Scalar constant beta.</param>
            /// <param name="c">Handle to the dense matrix c (output).</param>
            /// <param name="layout">Describes the storage scheme for the dense matrix.</param>
            /// <param name="ldc">Leading dimension of matrix c.</param>
            /// <returns>Result status of the operation.</returns>
            public SPARSE_Status Syrkd(SPARSE_Operation operation,
                [In] IntPtr a, double alpha, double beta,
                [In, Out] double* c, SPARSE_Layout layout, long ldc)
                => IntelMKLNative.mkl_sparse_d_syrkd_64(operation,
                    a, alpha, beta, c, layout, ldc);

            /// <summary>
            /// Computes the symmetric rank-k update for a sparse complex matrix and stores the result as a dense matrix.
            /// C = beta * C + alpha * op(A) * op(A)^H
            /// </summary>
            /// <param name="operation">Specifies the operation op() on the sparse matrix a.</param>
            /// <param name="a">Handle to the sparse matrix a.</param>
            /// <param name="alpha">Pointer to the scalar constant alpha (complex).</param>
            /// <param name="beta">Pointer to the scalar constant beta (complex).</param>
            /// <param name="c">Handle to the dense matrix c (output).</param>
            /// <param name="layout">Describes the storage scheme for the dense matrix.</param>
            /// <param name="ldc">Leading dimension of matrix c.</param>
            /// <returns>Result status of the operation.</returns>
            public SPARSE_Status Syrkd(SPARSE_Operation operation,
                [In] IntPtr a, void* alpha, void* beta,
                [In, Out] void* c, SPARSE_Layout layout, long ldc)
                => IntelMKLNative.mkl_sparse_z_syrkd_64(operation,
                    a, alpha, beta, c, layout, ldc);

            #endregion
            #region ---- Syprd [D/Z] ----

            /// <summary>
            /// Computes the symmetric triple product of a sparse matrix and a dense matrix,
            /// C = alpha * op(A) * B * op(A)^T + beta * C,
            /// and stores the result as a dense matrix.
            /// </summary>
            /// <param name="operation">Specifies operation op() on sparse matrix a.</param>
            /// <param name="a">Handle to the sparse matrix A.</param>
            /// <param name="b">Pointer to the dense matrix B.</param>
            /// <param name="layoutB">Describes the storage scheme for the dense matrix B.</param>
            /// <param name="ldb">Leading dimension of matrix B.</param>
            /// <param name="alpha">Scalar constant alpha.</param>
            /// <param name="beta">Scalar constant beta.</param>
            /// <param name="c">Pointer to the dense matrix C (input/output).</param>
            /// <param name="layoutC">Describes the storage scheme for the dense matrix C.</param>
            /// <param name="ldc">Leading dimension of matrix C.</param>
            /// <returns>Result status of the operation.</returns>
            public SPARSE_Status Syprd(SPARSE_Operation operation,
                [In] IntPtr a, [In] IntPtr b, SPARSE_Layout layoutB, long ldb,
                double alpha, double beta,
                [In, Out] double* c, SPARSE_Layout layoutC, long ldc)
                => IntelMKLNative.mkl_sparse_d_syprd_64(operation,
                    a, b, layoutB, ldb, alpha, beta, c, layoutC, ldc);

            /// <summary>
            /// Computes the symmetric triple product of a sparse matrix and a dense matrix (complex version),
            /// C = alpha * op(A) * B * op(A)^H + beta * C,
            /// and stores the result as a dense matrix.
            /// </summary>
            /// <param name="operation">Specifies operation op() on sparse matrix a.</param>
            /// <param name="a">Handle to the sparse matrix A.</param>
            /// <param name="b">Pointer to the dense matrix B.</param>
            /// <param name="layoutB">Describes the storage scheme for the dense matrix B.</param>
            /// <param name="ldb">Leading dimension of matrix B.</param>
            /// <param name="alpha">Pointer to the scalar constant alpha (complex).</param>
            /// <param name="beta">Pointer to the scalar constant beta (complex).</param>
            /// <param name="c">Pointer to the dense matrix C (input/output, complex).</param>
            /// <param name="layoutC">Describes the storage scheme for the dense matrix C.</param>
            /// <param name="ldc">Leading dimension of matrix C.</param>
            /// <returns>Result status of the operation.</returns>
            public SPARSE_Status Syprd(SPARSE_Operation operation,
                [In] IntPtr a, [In] IntPtr b, SPARSE_Layout layoutB, long ldb,
                void* alpha, void* beta,
                [In, Out] void* c, SPARSE_Layout layoutC, long ldc)
                => IntelMKLNative.mkl_sparse_z_syprd_64(operation,
                    a, b, layoutB, ldb, alpha, beta, c, layoutC, ldc);

            #endregion
            #region ---- Sorv [D] ----

            /// <summary>
            /// Computes forward, backward sweeps or a symmetric
            /// successive over-relaxation (SOR) preconditioner operation
            /// for a sparse matrix.
            /// </summary>
            /// <param name="type">Specifies the operation performed by the SORV preconditioner.</param>
            /// <param name="descrA">Structure specifying sparse matrix properties.</param>
            /// <param name="a">Sparse matrix handle.</param>
            /// <param name="omega">Relaxation factor.</param>
            /// <param name="alpha">Parameter that could be used to normalize or set to zero the vector x that holds the initial guess.</param>
            /// <param name="x">Initial guess on input (dense) vector x. Modified in place.</param>
            /// <param name="b">Right-hand side (dense) vector b.</param>
            /// <returns>Result status of the SORV operation.</returns>
            public SPARSE_Status Sorv(SPARSE_SorType type,
                SPARSE_MatrixDescr descrA, [In] IntPtr a, double omega,
                double alpha, [In, Out] double* x, [In] double* b)
                => IntelMKLNative.mkl_sparse_d_sorv_64(type, descrA,
                    a, omega, alpha, x, b);

            #endregion

            #endregion

        }

        
        /// <summary>
        /// LAPACK class
        /// </summary>
        public unsafe class LAPACK : ILAPACK
        {
            #region ---- Getrf [D/Z] ----

            /// <summary>
            /// Computes the LU factorization of a general m-by-n matrix (double precision).
            /// A = P*L*U, where P is a permutation matrix, 
            /// L is lower triangular with unit diagonal elements (lower trapezoidal if m>n),
            /// and U is upper triangular (upper trapezoidal if n>m).
            /// </summary>
            /// <param name="layout">Matrix storage layout (row or column major).</param>
            /// <param name="m">Number of rows in matrix <paramref name="a"/>.</param>
            /// <param name="n">Number of columns in matrix <paramref name="a"/>.</param>
            /// <param name="a">Pointer to the matrix <paramref name="a"/> (overwritten by L and U on exit).</param>
            /// <param name="lda">Leading dimension of <paramref name="a"/>.</param>
            /// <param name="ipiv">Pointer to the pivot indices.</param>
            /// <returns>Status code from the native LAPACK routine.</returns>
            public long Getrf(LAPACK_Layout layout, long m, long n,
                [In, Out] double* a, long lda, [Out] long* ipiv)
                => IntelMKLNative.LAPACKE_dgetrf_64(layout, m, n, a, lda, ipiv);

            /// <summary>
            /// Computes the LU factorization of a general m-by-n matrix (complex double precision).
            /// A = P*L*U, where P is a permutation matrix, 
            /// L is lower triangular with unit diagonal elements (lower trapezoidal if m>n),
            /// and U is upper triangular (upper trapezoidal if n>m).
            /// </summary>
            /// <param name="layout">Matrix storage layout (row or column major).</param>
            /// <param name="m">Number of rows in matrix <paramref name="a"/>.</param>
            /// <param name="n">Number of columns in matrix <paramref name="a"/>.</param>
            /// <param name="a">Pointer to the matrix <paramref name="a"/> (overwritten by L and U on exit, complex values).</param>
            /// <param name="lda">Leading dimension of <paramref name="a"/>.</param>
            /// <param name="ipiv">Pointer to the pivot indices.</param>
            /// <returns>Status code from the native LAPACK routine.</returns>
            public long Getrf(LAPACK_Layout layout, long m, long n,
                [In, Out] void* a, long lda, [Out] long* ipiv)
                => IntelMKLNative.LAPACKE_zgetrf_64(layout, m, n, a, lda, ipiv);

            #endregion
            #region ---- Getrs [D/Z] ----

            /// <summary>
            /// Solves a system of linear equations A * X = B, A^T * X = B, or A^H * X = B with a general N-by-N matrix A using the LU factorization computed by <c>Getrf</c>.
            /// </summary>
            /// <param name="layout">Specifies the matrix layout (row-major or column-major).</param>
            /// <param name="trans">Specifies the form of the system of equations (no transpose, transpose, or conjugate transpose).</param>
            /// <param name="n">The order of the matrix A (number of linear equations, i.e., the number of rows and columns in A).</param>
            /// <param name="nrhs">The number of right-hand sides, i.e., the number of columns of the matrix B.</param>
            /// <param name="a">Pointer to the LU-factored matrix A as computed by <c>Getrf</c>.</param>
            /// <param name="lda">The leading dimension of the array A.</param>
            /// <param name="ipiv">Pointer to the pivot indices as returned by <c>Getrf</c>.</param>
            /// <param name="b">Pointer to the right-hand side matrix B (input) and the solution matrix X (output).</param>
            /// <param name="ldb">The leading dimension of the array B.</param>
            public void Getrs(LAPACK_Layout layout, LAPACK_Transpose trans,
                long n, long nrhs, [In] double* a, long lda,
                [In] long* ipiv, [In, Out] double* b, long ldb)
                => IntelMKLNative.LAPACKE_dgetrs_64(layout, trans,
                    n, nrhs, a, lda, ipiv, b, ldb);

            /// <summary>
            /// Solves a system of linear equations A * X = B, A^T * X = B, or A^H * X = B with a general N-by-N complex matrix A using the LU factorization computed by <c>Getrf</c>.
            /// </summary>
            /// <param name="layout">Specifies the matrix layout (row-major or column-major).</param>
            /// <param name="trans">Specifies the form of the system of equations (no transpose, transpose, or conjugate transpose).</param>
            /// <param name="n">The order of the matrix A (number of linear equations, i.e., the number of rows and columns in A).</param>
            /// <param name="nrhs">The number of right-hand sides, i.e., the number of columns of the matrix B.</param>
            /// <param name="a">Pointer to the LU-factored complex matrix A as computed by <c>Getrf</c>.</param>
            /// <param name="lda">The leading dimension of the array A.</param>
            /// <param name="ipiv">Pointer to the pivot indices as returned by <c>Getrf</c>.</param>
            /// <param name="b">Pointer to the right-hand side complex matrix B (input) and the solution matrix X (output).</param>
            /// <param name="ldb">The leading dimension of the array B.</param>
            public void Getrs(LAPACK_Layout layout, LAPACK_Transpose trans,
                long n, long nrhs, [In] void* a, long lda,
                [In] long* ipiv, [In, Out] void* b, long ldb)
                => IntelMKLNative.LAPACKE_zgetrs_64(layout, trans,
                    n, nrhs, a, lda, ipiv, b, ldb);

            #endregion
            #region ---- Getri [D/Z] ----

            /// <summary>
            /// Computes the inverse of an LU-factored general matrix (double precision).
            /// </summary>
            /// <param name="layout">Specifies the matrix storage layout (row- or column-major).</param>
            /// <param name="n">The order of the matrix A (number of rows and columns).</param>
            /// <param name="a">Pointer to the LU-factored matrix A (overwritten by the inverse on exit).</param>
            /// <param name="lda">The leading dimension of the matrix A.</param>
            /// <param name="ipiv">Pointer to the pivot indices from the LU factorization.</param>
            public void Getri(LAPACK_Layout layout, long n,
                [In, Out] double* a, long lda, [In] long* ipiv)
                => IntelMKLNative.LAPACKE_dgetri_64(layout, n, a, lda, ipiv);

            /// <summary>
            /// Computes the inverse of an LU-factored general matrix (complex double precision).
            /// </summary>
            /// <param name="layout">Specifies the matrix storage layout (row- or column-major).</param>
            /// <param name="n">The order of the matrix A (number of rows and columns).</param>
            /// <param name="a">Pointer to the LU-factored complex matrix A (overwritten by the inverse on exit).</param>
            /// <param name="lda">The leading dimension of the matrix A.</param>
            /// <param name="ipiv">Pointer to the pivot indices from the LU factorization.</param>
            public void Getri(LAPACK_Layout layout, long n,
                [In, Out] void* a, long lda, [In] long* ipiv)
                => IntelMKLNative.LAPACKE_zgetri_64(layout, n, a, lda, ipiv);

            #endregion
            #region ---- Gesv [D/Z] ----

            /// <summary>
            /// Computes the solution to a system of linear equations with a square coefficient matrix A and multiple right-hand sides (double precision).
            /// This method overwrites the input matrix <paramref name="a"/> with the factors L and U from the factorization, and overwrites <paramref name="b"/> with the solution matrix X.
            /// A*X = B
            /// </summary>
            /// <param name="layout">Specifies the matrix storage layout (row-major or column-major).</param>
            /// <param name="n">The order of the matrix A (number of linear equations).</param>
            /// <param name="nrhs">The number of right-hand sides (number of columns of matrix B).</param>
            /// <param name="a">On entry, the coefficient matrix A; on exit, overwritten by the factors L and U from the factorization.</param>
            /// <param name="lda">The leading dimension of A.</param>
            /// <param name="ipiv">The pivot indices that define the permutation matrix P; size at least n.</param>
            /// <param name="b">On entry, the right-hand side matrix B; on exit, overwritten by the solution matrix X.</param>
            /// <param name="ldb">The leading dimension of B.</param>
            public void Gesv(LAPACK_Layout layout, long n, long nrhs,
                [In, Out] double* a, long lda, [In, Out] long* ipiv,
                [In, Out] double* b, long ldb)
                => IntelMKLNative.LAPACKE_dgesv_64(layout, n, nrhs, a, lda, ipiv, b, ldb);

            /// <summary>
            /// Computes the solution to a system of linear equations with a square coefficient matrix A and multiple right-hand sides (complex double precision).
            /// This method overwrites the input matrix <paramref name="a"/> with the factors L and U from the factorization, and overwrites <paramref name="b"/> with the solution matrix X.
            /// A*X = B
            /// </summary>
            /// <param name="layout">Specifies the matrix storage layout (row-major or column-major).</param>
            /// <param name="n">The order of the matrix A (number of linear equations).</param>
            /// <param name="nrhs">The number of right-hand sides (number of columns of matrix B).</param>
            /// <param name="a">On entry, the coefficient matrix A (complex); on exit, overwritten by the factors L and U from the factorization.</param>
            /// <param name="lda">The leading dimension of A.</param>
            /// <param name="ipiv">The pivot indices that define the permutation matrix P; size at least n.</param>
            /// <param name="b">On entry, the right-hand side matrix B (complex); on exit, overwritten by the solution matrix X.</param>
            /// <param name="ldb">The leading dimension of B.</param>
            public void Gesv(LAPACK_Layout layout, long n, long nrhs,
                [In, Out] void* a, long lda, [In, Out] long* ipiv,
                [In, Out] void* b, long ldb)
                => IntelMKLNative.LAPACKE_zgesv_64(layout, n, nrhs, a, lda, ipiv, b, ldb);

            #endregion
            #region ---- Geev [D/Z] ----

            /// <summary>
            /// Computes the eigenvalues and, optionally, the left and/or right eigenvectors for a real nonsymmetric matrix.
            /// </summary>
            /// <param name="layout">Specifies the matrix layout (row-major or column-major).</param>
            /// <param name="jobvl">Specifies whether to compute the left eigenvectors.</param>
            /// <param name="jobvr">Specifies whether to compute the right eigenvectors.</param>
            /// <param name="n">The order of the matrix A.</param>
            /// <param name="a">Pointer to the input matrix A. Overwritten on exit.</param>
            /// <param name="lda">The leading dimension of A.</param>
            /// <param name="wr">Pointer to the output array for real parts of computed eigenvalues.</param>
            /// <param name="wi">Pointer to the output array for imaginary parts of computed eigenvalues.</param>
            /// <param name="vl">Pointer to the output array for left eigenvectors (if requested).</param>
            /// <param name="ldvl">The leading dimension of VL.</param>
            /// <param name="vr">Pointer to the output array for right eigenvectors (if requested).</param>
            /// <param name="ldvr">The leading dimension of VR.</param>
            public void Geev(LAPACK_Layout layout,
                LAPACK_Job jobvl, LAPACK_Job jobvr,
                long n, [In, Out] double* a, long lda,
                [Out] double* wr, [Out] double* wi,
                [Out] double* vl, long ldvl,
                [Out] double* vr, long ldvr)
                => IntelMKLNative.LAPACKE_dgeev_64(layout, jobvl, jobvr,
                    n, a, lda, wr, wi, vl, ldvl, vr, ldvr);

            /// <summary>
            /// Computes the eigenvalues and, optionally, the left and/or right eigenvectors for a complex nonsymmetric matrix.
            /// </summary>
            /// <param name="layout">Specifies the matrix layout (row-major or column-major).</param>
            /// <param name="jobvl">Specifies whether to compute the left eigenvectors.</param>
            /// <param name="jobvr">Specifies whether to compute the right eigenvectors.</param>
            /// <param name="n">The order of the matrix A.</param>
            /// <param name="a">Pointer to the input matrix A (complex). Overwritten on exit.</param>
            /// <param name="lda">The leading dimension of A.</param>
            /// <param name="w">Pointer to the output array for computed eigenvalues (complex).</param>
            /// <param name="vl">Pointer to the output array for left eigenvectors (if requested, complex).</param>
            /// <param name="ldvl">The leading dimension of VL.</param>
            /// <param name="vr">Pointer to the output array for right eigenvectors (if requested, complex).</param>
            /// <param name="ldvr">The leading dimension of VR.</param>
            public void Geev(LAPACK_Layout layout,
                LAPACK_Job jobvl, LAPACK_Job jobvr,
                long n, [In, Out] void* a, long lda,
                [Out] void* w,
                [Out] void* vl, long ldvl,
                [Out] void* vr, long ldvr)
                => IntelMKLNative.LAPACKE_zgeev_64(layout, jobvl, jobvr,
                    n, a, lda, w, vl, ldvl, vr, ldvr);

            #endregion
            #region ---- Ggev [D/Z] ----

            /// <summary>
            /// Computes the generalized eigenvalues and, optionally, the left and/or right eigenvectors for a pair of real matrices (double precision).
            /// </summary>
            /// <param name="layout">Specifies the matrix layout (row-major or column-major).</param>
            /// <param name="jobvl">Specifies whether to compute the left eigenvectors.</param>
            /// <param name="jobvr">Specifies whether to compute the right eigenvectors.</param>
            /// <param name="n">The order of the matrices A and B.</param>
            /// <param name="a">Pointer to the first element of matrix A.</param>
            /// <param name="lda">The leading dimension of matrix A.</param>
            /// <param name="b">Pointer to the first element of matrix B.</param>
            /// <param name="ldb">The leading dimension of matrix B.</param>
            /// <param name="alphar">Pointer to the output array for the real parts of the generalized eigenvalues.</param>
            /// <param name="alphai">Pointer to the output array for the imaginary parts of the generalized eigenvalues.</param>
            /// <param name="beta">Pointer to the output array for the denominators of the generalized eigenvalues.</param>
            /// <param name="vl">Pointer to the output array for the left eigenvectors.</param>
            /// <param name="ldvl">The leading dimension of the left eigenvector matrix.</param>
            /// <param name="vr">Pointer to the output array for the right eigenvectors.</param>
            /// <param name="ldvr">The leading dimension of the right eigenvector matrix.</param>
            public void Ggev(LAPACK_Layout layout,
                LAPACK_Job jobvl, LAPACK_Job jobvr,
                long n, [In, Out] double* a, long lda,
                [In] double* b, long ldb,
                [Out] double* alphar, [Out] double* alphai, [Out] double* beta,
                [Out] double* vl, long ldvl,
                [Out] double* vr, long ldvr)
                => IntelMKLNative.LAPACKE_dggev_64(layout, jobvl, jobvr,
                    n, a, lda, b, ldb, alphar, alphai, beta, vl, ldvl, vr, ldvr);

            /// <summary>
            /// Computes the generalized eigenvalues and, optionally, the left and/or right eigenvectors for a pair of complex matrices.
            /// </summary>
            /// <param name="layout">Specifies the matrix layout (row-major or column-major).</param>
            /// <param name="jobvl">Specifies whether to compute the left eigenvectors.</param>
            /// <param name="jobvr">Specifies whether to compute the right eigenvectors.</param>
            /// <param name="n">The order of the matrices A and B.</param>
            /// <param name="a">Pointer to the first element of matrix A (complex).</param>
            /// <param name="lda">The leading dimension of matrix A.</param>
            /// <param name="b">Pointer to the first element of matrix B (complex).</param>
            /// <param name="ldb">The leading dimension of matrix B.</param>
            /// <param name="alpha">Pointer to the output array for the generalized eigenvalues (complex).</param>
            /// <param name="beta">Pointer to the output array for the denominators of the generalized eigenvalues (complex).</param>
            /// <param name="vl">Pointer to the output array for the left eigenvectors (complex).</param>
            /// <param name="ldvl">The leading dimension of the left eigenvector matrix.</param>
            /// <param name="vr">Pointer to the output array for the right eigenvectors (complex).</param>
            /// <param name="ldvr">The leading dimension of the right eigenvector matrix.</param>
            public void Ggev(LAPACK_Layout layout,
                LAPACK_Job jobvl, LAPACK_Job jobvr,
                long n, [In, Out] void* a, long lda,
                [In] void* b, long ldb,
                [Out] void* alpha, [Out] void* beta,
                [Out] void* vl, long ldvl,
                [Out] void* vr, long ldvr)
                => IntelMKLNative.LAPACKE_zggev_64(layout, jobvl, jobvr,
                    n, a, lda, b, ldb, alpha, beta, vl, ldvl, vr, ldvr);

            #endregion
            #region ---- Syev [D] ----

            /// <summary>
            /// Computes all eigenvalues and, optionally, eigenvectors of a real symmetric matrix using the LAPACK dsyev routine.
            /// </summary>
            /// <param name="layout">Specifies the matrix layout (row-major or column-major).</param>
            /// <param name="jobz">Specifies whether to compute eigenvectors ('V') or eigenvalues only ('N').</param>
            /// <param name="uplo">Specifies whether the upper ('U') or lower ('L') triangle of the matrix is used.</param>
            /// <param name="n">The order of the matrix A. Must be at least zero.</param>
            /// <param name="a">Pointer to the input matrix A. On exit, contains eigenvectors if requested.</param>
            /// <param name="lda">The leading dimension of the array A. Must be at least max(1, n).</param>
            /// <param name="w">Pointer to the output array of eigenvalues, in ascending order.</param>
            public void Syev(LAPACK_Layout layout, LAPACK_Job jobz,
                char uplo, long n, [In, Out] double* a, long lda,
                [Out] double* w)
                => IntelMKLNative.LAPACKE_dsyev_64(layout, jobz, uplo, n, a, lda, w);

            #endregion
            #region ---- Heev [Z] ----

            /// <summary>
            /// Computes all eigenvalues and, optionally, eigenvectors of a complex Hermitian matrix using LAPACK's zheev routine.
            /// </summary>
            /// <param name="layout">Specifies the matrix layout (row-major or column-major).</param>
            /// <param name="jobz">Specifies whether to compute eigenvectors ('N' for eigenvalues only, 'V' for eigenvalues and eigenvectors).</param>
            /// <param name="uplo">Specifies whether the upper or lower triangular part of the matrix is used ('U' or 'L').</param>
            /// <param name="n">The order of the matrix <paramref name="a"/>.</param>
            /// <param name="a">Pointer to the Hermitian matrix. On exit, contains the eigenvectors if requested.</param>
            /// <param name="lda">The leading dimension of <paramref name="a"/>.</param>
            /// <param name="w">Pointer to the output array for eigenvalues, in ascending order.</param>
            public void Heev(LAPACK_Layout layout, LAPACK_Job jobz,
                char uplo, long n, [In, Out] void* a, long lda,
                [Out] double* w)
                => IntelMKLNative.LAPACKE_zheev_64(layout, jobz, uplo, n, a, lda, w);

            #endregion
            #region ---- Gesvd [D/Z] ----

            /// <summary>
            /// Computes the singular value decomposition (SVD) of a real M-by-N matrix A.
            /// </summary>
            /// <param name="layout">Specifies the matrix layout (row-major or column-major).</param>
            /// <param name="jobu">Specifies options for computing all or part of the matrix U.</param>
            /// <param name="jobvt">Specifies options for computing all or part of the matrix VT.</param>
            /// <param name="m">The number of rows of the matrix A. m &gt;= 0.</param>
            /// <param name="n">The number of columns of the matrix A. n &gt;= 0.</param>
            /// <param name="a">On entry, the M-by-N matrix A. On exit, contents are overwritten.</param>
            /// <param name="lda">The leading dimension of the array A.</param>
            /// <param name="s">The singular values of A, sorted in decreasing order.</param>
            /// <param name="u">The left singular vectors, if requested.</param>
            /// <param name="ldu">The leading dimension of the array U.</param>
            /// <param name="vt">The right singular vectors, if requested.</param>
            /// <param name="ldvt">The leading dimension of the array VT.</param>
            /// <param name="superb">Array used for intermediate computations.</param>
            public void Gesvd(LAPACK_Layout layout,
                LAPACK_Job jobu, LAPACK_Job jobvt, long m, long n,
                [In, Out] double* a, long lda,
                [Out] double* s, [Out] double* u, long ldu,
                [Out] double* vt, long ldvt, [Out] double* superb)
                => IntelMKLNative.LAPACKE_dgesvd_64(layout, jobu, jobvt,
                    m, n, a, lda, s, u, ldu, vt, ldvt, superb);

            /// <summary>
            /// Computes the singular value decomposition (SVD) of a complex M-by-N matrix A.
            /// </summary>
            /// <param name="layout">Specifies the matrix layout (row-major or column-major).</param>
            /// <param name="jobu">Specifies options for computing all or part of the matrix U.</param>
            /// <param name="jobvt">Specifies options for computing all or part of the matrix VT.</param>
            /// <param name="m">The number of rows of the matrix A. m &gt;= 0.</param>
            /// <param name="n">The number of columns of the matrix A. n &gt;= 0.</param>
            /// <param name="a">On entry, the M-by-N complex matrix A. On exit, contents are overwritten.</param>
            /// <param name="lda">The leading dimension of the array A.</param>
            /// <param name="s">The singular values of A, sorted in decreasing order.</param>
            /// <param name="u">The left singular vectors, if requested.</param>
            /// <param name="ldu">The leading dimension of the array U.</param>
            /// <param name="vt">The right singular vectors, if requested.</param>
            /// <param name="ldvt">The leading dimension of the array VT.</param>
            /// <param name="superb">Array used for intermediate computations.</param>
            public void Gesvd(LAPACK_Layout layout,
                LAPACK_Job jobu, LAPACK_Job jobvt, long m, long n,
                [In, Out] void* a, long lda,
                [Out] double* s, [Out] void* u, long ldu,
                [Out] void* vt, long ldvt, [Out] double* superb)
                => IntelMKLNative.LAPACKE_zgesvd_64(layout, jobu, jobvt,
                    m, n, a, lda, s, u, ldu, vt, ldvt, superb);

            #endregion
            #region ---- Gels [D/Z] ----

            /// <summary>
            /// Computes the minimum-norm solution to a real linear least squares problem using the LAPACK DGELS routine.
            /// </summary>
            /// <param name="layout">Specifies the matrix layout (row-major or column-major).</param>
            /// <param name="trans">Specifies the options for transposing the matrix.</param>
            /// <param name="m">The number of rows of the matrix A.</param>
            /// <param name="n">The number of columns of the matrix A.</param>
            /// <param name="nrhs">The number of right-hand sides, i.e., the number of columns of the matrix B.</param>
            /// <param name="a">Pointer to the matrix A. On exit, contains details of its QR or LQ factorization.</param>
            /// <param name="lda">The leading dimension of the array A.</param>
            /// <param name="b">Pointer to the matrix B. On exit, contains the solution matrix X.</param>
            /// <param name="ldb">The leading dimension of the array B.</param>
            public void Gels(LAPACK_Layout layout, LAPACK_Transpose trans,
                long m, long n, long nrhs, [In, Out] double* a, long lda,
                [In, Out] double* b, long ldb)
                => IntelMKLNative.LAPACKE_dgels_64(layout, trans,
                    m, n, nrhs, a, lda, b, ldb);

            /// <summary>
            /// Computes the minimum-norm solution to a complex linear least squares problem using the LAPACK ZGELS routine.
            /// </summary>
            /// <param name="layout">Specifies the matrix layout (row-major or column-major).</param>
            /// <param name="trans">Specifies the options for transposing the matrix.</param>
            /// <param name="m">The number of rows of the matrix A.</param>
            /// <param name="n">The number of columns of the matrix A.</param>
            /// <param name="nrhs">The number of right-hand sides, i.e., the number of columns of the matrix B.</param>
            /// <param name="a">Pointer to the complex matrix A. On exit, contains details of its QR or LQ factorization.</param>
            /// <param name="lda">The leading dimension of the array A.</param>
            /// <param name="b">Pointer to the complex matrix B. On exit, contains the solution matrix X.</param>
            /// <param name="ldb">The leading dimension of the array B.</param>
            public void Gels(LAPACK_Layout layout, LAPACK_Transpose trans,
                long m, long n, long nrhs, [In, Out] void* a, long lda,
                [In, Out] void* b, long ldb)
                => IntelMKLNative.LAPACKE_zgels_64(layout, trans,
                    m, n, nrhs, a, lda, b, ldb);

            #endregion
            #region ---- Lacgv ----

            // ...

            #endregion
        }


        
        /// <summary>
        /// Vector Math Functions   
        /// </summary>
        public unsafe class VMF : IVMF
        {
            #region ---- Abs ----

            /// <summary>
            /// Computes the absolute value of each element in the input array <paramref name="a"/> and stores the result in <paramref name="y"/>.
            /// y[i] = |a[i]|
            /// </summary>
            /// <param name="n">The number of elements in the input array.</param>
            /// <param name="a">Pointer to the input array of double values.</param>
            /// <param name="y">Pointer to the output array where the absolute values will be stored.</param>
            public void Abs(long n, [In] double* a, [Out] double* y)
                => IntelMKLNative.vdAbs_64(n, a, y);

            /// <summary>
            /// Computes the absolute value of each element in the input complex array <paramref name="a"/> and stores the result in <paramref name="y"/>.
            /// y[i] = |a[i]|
            /// </summary>
            /// <param name="n">The number of elements in the input array.</param>
            /// <param name="a">Pointer to the input array of complex values.</param>
            /// <param name="y">Pointer to the output array where the absolute values will be stored as doubles.</param>
            public void Abs(long n, [In] void* a, [Out] double* y)
                => IntelMKLNative.vzAbs_64(n, a, y);

            #endregion
            #region ---- Arg ----

            /// <summary>
            /// Computes the argument (phase angle) of each element in a complex array.
            /// y[i] = arg(a[i])
            /// </summary>
            /// <param name="n">The number of elements in the array.</param>
            /// <param name="a">Pointer to the input array of complex numbers.</param>
            /// <param name="y">Pointer to the output array for the computed arguments (in radians).</param>
            public void Arg(long n, [In] void* a, [Out] double* y)
                => IntelMKLNative.vzArg_64(n, a, y);

            #endregion
            #region ---- Add ----

            /// <summary>
            /// Performs element-wise addition of two double arrays.
            /// y[i] = a[i] + b[i]
            /// </summary>
            /// <param name="n">Number of elements in the arrays.</param>
            /// <param name="a">Pointer to the first input array.</param>
            /// <param name="b">Pointer to the second input array.</param>
            /// <param name="y">Pointer to the output array where the result is stored.</param>
            public void Add(long n, [In] double* a, [In] double* b, [Out] double* y)
                => IntelMKLNative.vdAdd_64(n, a, b, y);

            /// <summary>
            /// Performs element-wise addition of two complex arrays.
            /// y[i] = a[i] + b[i]
            /// </summary>
            /// <param name="n">Number of elements in the arrays.</param>
            /// <param name="a">Pointer to the first input array (complex values).</param>
            /// <param name="b">Pointer to the second input array (complex values).</param>
            /// <param name="y">Pointer to the output array where the result is stored (complex values).</param>
            public void Add(long n, [In] void* a, [In] void* b, [Out] void* y)
                => IntelMKLNative.vzAdd_64(n, a, b, y);

            #endregion
            #region ---- Sub ----

            /// <summary>
            /// Performs element-wise subtraction of two arrays of doubles.
            /// y[i] = a[i] - b[i]
            /// </summary>
            /// <param name="n">Number of elements.</param>
            /// <param name="a">Pointer to the first input array.</param>
            /// <param name="b">Pointer to the second input array.</param>
            /// <param name="y">Pointer to the output array where the result is stored.</param>
            public void Sub(long n, [In] double* a, [In] double* b, [Out] double* y)
                => IntelMKLNative.vdSub_64(n, a, b, y);

            /// <summary>
            /// Performs element-wise subtraction of two arrays of complex numbers.
            /// y[i] = a[i] - b[i]
            /// </summary>
            /// <param name="n">Number of elements.</param>
            /// <param name="a">Pointer to the first input array (complex).</param>
            /// <param name="b">Pointer to the second input array (complex).</param>
            /// <param name="y">Pointer to the output array where the result is stored (complex).</param>
            public void Sub(long n, [In] void* a, [In] void* b, [Out] void* y)
                => IntelMKLNative.vzSub_64(n, a, b, y);

            #endregion
            #region ---- Inv ----

            /// <summary>
            /// Computes the element-wise inverse of a double-precision array.
            /// y[i] = 1.0 / a[i]
            /// </summary>
            /// <param name="n">The number of elements in the input array.</param>
            /// <param name="a">Pointer to the input array.</param>
            /// <param name="y">Pointer to the output array where the result is stored.</param>
            public void Inv(long n, [In] double* a, [Out] double* y)
                => IntelMKLNative.vdInv_64(n, a, y);

            #endregion
            #region ---- Sqrt ----

            /// <summary>
            /// Computes the square root of each element in the input array <paramref name="a"/> and stores the result in <paramref name="y"/>.
            /// y[i] = a[i]^0.5
            /// </summary>
            /// <param name="n">The number of elements in the input and output arrays.</param>
            /// <param name="a">Pointer to the input array of double values.</param>
            /// <param name="y">Pointer to the output array where the square roots will be stored.</param>
            public void Sqrt(long n, [In] double* a, [Out] double* y)
                => IntelMKLNative.vdSqrt_64(n, a, y);

            /// <summary>
            /// Computes the square root of each element in the input array <paramref name="a"/> (complex values) and stores the result in <paramref name="y"/>.
            /// y[i] = a[i]^0.5
            /// </summary>
            /// <param name="n">The number of elements in the input and output arrays.</param>
            /// <param name="a">Pointer to the input array of complex values.</param>
            /// <param name="y">Pointer to the output array where the square roots will be stored.</param>
            public void Sqrt(long n, [In] void* a, [Out] void* y)
                => IntelMKLNative.vzSqrt_64(n, a, y);

            #endregion
            #region ---- InvSqrt ----

            /// <summary>
            /// Computes the inverse square root of each element in the input array <paramref name="a"/> and stores the result in the output array <paramref name="y"/>.
            /// y[i] = 1/a[i]^0.5
            /// </summary>
            /// <param name="n">The number of elements to process.</param>
            /// <param name="a">Pointer to the input array of double values.</param>
            /// <param name="y">Pointer to the output array where the results will be stored.</param>
            public void InvSqrt(long n, [In] double* a, [Out] double* y)
                => IntelMKLNative.vdInvSqrt_64(n, a, y);

            #endregion
            #region ---- Cbrt ----

            /// <summary>
            /// Computes the cube root of each element in the input array <paramref name="a"/> and stores the result in the output array <paramref name="y"/>.
            /// y[i] = a[i]^(1/3)
            /// </summary>
            /// <param name="n">The number of elements in the input and output arrays.</param>
            /// <param name="a">Pointer to the input array of double-precision values.</param>
            /// <param name="y">Pointer to the output array where the cube root results will be stored.</param>
            public void Cbrt(long n, [In] double* a, [Out] double* y)
                => IntelMKLNative.vdCbrt_64(n, a, y);

            #endregion
            #region ---- InvCbrt ----

            /// <summary>
            /// Computes the inverse cube root of each element in the input array <paramref name="a"/> and stores the result in <paramref name="y"/>.
            /// y[i] = 1/a[i]^(1/3)
            /// </summary>
            /// <param name="n">The number of elements in the input and output arrays.</param>
            /// <param name="a">Pointer to the input array of doubles.</param>
            /// <param name="y">Pointer to the output array of doubles where the results will be stored.</param>
            public void InvCbrt(long n, [In] double* a, [Out] double* y)
                => IntelMKLNative.vdInvCbrt_64(n, a, y);

            #endregion
            #region ---- Sqr ----

            /// <summary>
            /// Computes the element-wise square of the input array <paramref name="a"/> and stores the result in <paramref name="y"/>.
            /// y[i] = a[i]^2
            /// </summary>
            /// <param name="n">The number of elements in the input array.</param>
            /// <param name="a">Pointer to the input array of doubles.</param>
            /// <param name="y">Pointer to the output array of doubles where the result will be stored.</param>
            public void Sqr(long n, [In] double* a, [Out] double* y)
                => IntelMKLNative.vdSqr_64(n, a, y);

            #endregion
            #region ---- Exp ----

            /// <summary>
            /// Computes the exponential of each element in the input array <paramref name="a"/> and stores the result in <paramref name="y"/>.
            /// y[i] = e^a[i]
            /// </summary>
            /// <param name="n">The number of elements in the input and output arrays.</param>
            /// <param name="a">Pointer to the input array of doubles.</param>
            /// <param name="y">Pointer to the output array of doubles where the results will be stored.</param>
            public void Exp(long n, [In] double* a, [Out] double* y)
                => IntelMKLNative.vdExp_64(n, a, y);

            /// <summary>
            /// Computes the exponential of each element in the input array <paramref name="a"/> (complex values) and stores the result in <paramref name="y"/>.
            /// y[i] = e^a[i]
            /// </summary>
            /// <param name="n">The number of elements in the input and output arrays.</param>
            /// <param name="a">Pointer to the input array of complex values.</param>
            /// <param name="y">Pointer to the output array of complex values where the results will be stored.</param>
            public void Exp(long n, [In] void* a, [Out] void* y)
                => IntelMKLNative.vzExp_64(n, a, y);

            #endregion
            #region ---- Exp2 ----

            /// <summary>
            /// Computes the base-2 exponential of each element in the input array <paramref name="a"/> and stores the result in the output array <paramref name="y"/>.
            /// y[i] = 2^a[i]
            /// </summary>
            /// <param name="n">The number of elements to process.</param>
            /// <param name="a">Pointer to the input array of double-precision values.</param>
            /// <param name="y">Pointer to the output array where the results will be stored.</param>
            public void Exp2(long n, [In] double* a, [Out] double* y)
                => IntelMKLNative.vdExp2_64(n, a, y);

            #endregion
            #region ---- Exp10 ----

            /// <summary>
            /// Computes the base 10 exponential of each element in the input array <paramref name="a"/> and stores the result in the output array <paramref name="y"/>.
            /// y[i] = 10^a[i]
            /// </summary>
            /// <param name="n">The number of elements to process.</param>
            /// <param name="a">Pointer to the input array of double values.</param>
            /// <param name="y">Pointer to the output array where the results will be stored.</param>
            public void Exp10(long n, [In] double* a, [Out] double* y)
                => IntelMKLNative.vdExp10_64(n, a, y);

            #endregion
            #region ---- Expm1 ----

            /// <summary>
            /// Computes exp(a) - 1 for each element of the input array <paramref name="a"/> and stores the result in <paramref name="y"/>.
            /// y[i] = e^a[i] - 1
            /// </summary>
            /// <param name="n">The number of elements in the input and output arrays.</param>
            /// <param name="a">Pointer to the input array of double values.</param>
            /// <param name="y">Pointer to the output array where the results will be stored.</param>
            public void Expm1(long n, [In] double* a, [Out] double* y)
                => IntelMKLNative.vdExpm1_64(n, a, y);

            #endregion
            #region ---- Ln ----

            /// <summary>
            /// Computes the natural logarithm of each element in a double-precision array.
            /// y[i] = ln(a[i])
            /// </summary>
            /// <param name="n">Number of elements in the input array.</param>
            /// <param name="a">Pointer to the input array of double values.</param>
            /// <param name="y">Pointer to the output array where the results will be stored.</param>
            public void Ln(long n, [In] double* a, [Out] double* y)
                => IntelMKLNative.vdLn_64(n, a, y);

            /// <summary>
            /// Computes the natural logarithm of each element in a complex double-precision array.
            /// y[i] = ln(a[i])
            /// </summary>
            /// <param name="n">Number of elements in the input array.</param>
            /// <param name="a">Pointer to the input array of complex values.</param>
            /// <param name="y">Pointer to the output array where the results will be stored.</param>
            public void Ln(long n, [In] void* a, [Out] void* y)
                => IntelMKLNative.vzLn_64(n, a, y);

            #endregion
            #region ---- Log2 ----

            /// <summary>
            /// Computes the base 2 logarithm of each element in the input array <paramref name="a"/> and stores the result in the output array <paramref name="y"/>.
            /// y[i] = lb(a[i])
            /// </summary>
            /// <param name="n">The number of elements to process.</param>
            /// <param name="a">Pointer to the input array of double values.</param>
            /// <param name="y">Pointer to the output array where the results will be stored.</param>
            public void Log2(long n, [In] double* a, [Out] double* y)
                => IntelMKLNative.vdLog2_64(n, a, y);

            #endregion
            #region ---- Log10 ----

            /// <summary>
            /// Computes the base 10 logarithm of each element in the input array <paramref name="a"/> and stores the result in <paramref name="y"/>.
            /// y[i] = lg(a[i])
            /// </summary>
            /// <param name="n">The number of elements in the input array.</param>
            /// <param name="a">Pointer to the input array of type double.</param>
            /// <param name="y">Pointer to the output array of type double where the results will be stored.</param>
            public void Log10(long n, [In] double* a, [Out] double* y)
                => IntelMKLNative.vdLog10_64(n, a, y);

            /// <summary>
            /// Computes the base 10 logarithm of each element in the input array <paramref name="a"/> (complex values) and stores the result in <paramref name="y"/>.
            /// y[i] = lg(a[i])
            /// </summary>
            /// <param name="n">The number of elements in the input array.</param>
            /// <param name="a">Pointer to the input array of complex values.</param>
            /// <param name="y">Pointer to the output array where the results will be stored.</param>
            public void Log10(long n, [In] void* a, [Out] void* y)
                => IntelMKLNative.vzLog10_64(n, a, y);

            #endregion
            #region ---- Log1p ----

            /// <summary>
            /// Computes the natural logarithm of (1 + a) for each element of the input array <paramref name="a"/>,
            /// and stores the result in the output array <paramref name="y"/>.
            /// y[i] = log(1+a[i])
            /// </summary>
            /// <param name="n">The number of elements in the input and output arrays.</param>
            /// <param name="a">Pointer to the input array of double values.</param>
            /// <param name="y">Pointer to the output array where the results will be stored.</param>
            public void Log1p(long n, [In] double* a, [Out] double* y)
                => IntelMKLNative.vdLog1p_64(n, a, y);

            #endregion
            #region ---- Logb ----

            /// <summary>
            /// Computes the unbiased exponent (base 2) for each element of the input array <paramref name="a"/>,
            /// and stores the result in the output array <paramref name="y"/>.
            /// y[i] = logb(a[i])
            /// </summary>
            /// <param name="n">The number of elements in the input and output arrays.</param>
            /// <param name="a">Pointer to the input array of double-precision floating-point values.</param>
            /// <param name="y">Pointer to the output array where the results will be stored.</param>
            public void Logb(long n, [In] double* a, [Out] double* y)
                => IntelMKLNative.vdLogb_64(n, a, y);

            #endregion
            #region ---- Cos ----

            /// <summary>
            /// Computes the cosine of each element in the input array <paramref name="a"/> and stores the result in <paramref name="y"/>.
            /// y[i] = cos(a[i])
            /// </summary>
            /// <param name="n">The number of elements in the input array.</param>
            /// <param name="a">Pointer to the input array of double values.</param>
            /// <param name="y">Pointer to the output array where the cosine values will be stored.</param>
            public void Cos(long n, [In] double* a, [Out] double* y)
                => IntelMKLNative.vdCos_64(n, a, y);

            /// <summary>
            /// Computes the cosine of each element in the input array <paramref name="a"/> (complex values) and stores the result in <paramref name="y"/>.
            /// y[i] = cos(a[i])
            /// </summary>
            /// <param name="n">The number of elements in the input array.</param>
            /// <param name="a">Pointer to the input array of complex values.</param>
            /// <param name="y">Pointer to the output array where the cosine values will be stored.</param>
            public void Cos(long n, [In] void* a, [Out] void* y)
                => IntelMKLNative.vzCos_64(n, a, y);

            #endregion
            #region ---- Sin ----

            /// <summary>
            /// Computes the sine of each element in the input array <paramref name="a"/> and stores the result in <paramref name="y"/>.
            /// y[i] = sin(a[i])
            /// </summary>
            /// <param name="n">The number of elements in the input array.</param>
            /// <param name="a">Pointer to the input array of type double.</param>
            /// <param name="y">Pointer to the output array of type double where the results will be stored.</param>
            public void Sin(long n, [In] double* a, [Out] double* y)
                => IntelMKLNative.vdSin_64(n, a, y);

            /// <summary>
            /// Computes the sine of each element in the input array <paramref name="a"/> (complex values) and stores the result in <paramref name="y"/>.
            /// y[i] = sin(a[i])
            /// </summary>
            /// <param name="n">The number of elements in the input array.</param>
            /// <param name="a">Pointer to the input array of complex values.</param>
            /// <param name="y">Pointer to the output array of complex values where the results will be stored.</param>
            public void Sin(long n, [In] void* a, [Out] void* y)
                => IntelMKLNative.vzSin_64(n, a, y);

            #endregion
            #region ---- Tan ----

            /// <summary>
            /// Computes the tangent of each element in the input array <paramref name="a"/> and stores the result in <paramref name="y"/>.
            /// y[i] = tan(a[i])
            /// </summary>
            /// <param name="n">The number of elements in the input array.</param>
            /// <param name="a">Pointer to the input array of type double.</param>
            /// <param name="y">Pointer to the output array of type double, where the results will be stored.</param>
            public void Tan(long n, [In] double* a, [Out] double* y)
                => IntelMKLNative.vdTan_64(n, a, y);

            /// <summary>
            /// Computes the tangent of each element in the input array <paramref name="a"/> (complex values) and stores the result in <paramref name="y"/>.
            /// y[i] = tan(a[i])
            /// </summary>
            /// <param name="n">The number of elements in the input array.</param>
            /// <param name="a">Pointer to the input array of complex values.</param>
            /// <param name="y">Pointer to the output array of complex values, where the results will be stored.</param>
            public void Tan(long n, [In] void* a, [Out] void* y)
                => IntelMKLNative.vzTan_64(n, a, y);

            #endregion
            #region ---- Cospi ----

            /// <summary>
            /// Computes the cosine of each element in the input array <paramref name="a"/>,
            /// where the argument is interpreted as a multiple of π (i.e., computes cos(π * a[i]) for each element).
            /// y[i] = cos(a[i]*PI)
            /// </summary>
            /// <param name="n">The number of elements in the input array.</param>
            /// <param name="a">Pointer to the input array of double values.</param>
            /// <param name="y">Pointer to the output array where the results will be stored.</param>
            public void Cospi(long n, [In] double* a, [Out] double* y)
                => IntelMKLNative.vdCospi_64(n, a, y);

            #endregion
            #region ---- Sinpi ----

            /// <summary>
            /// Computes the sine of each element of the input array multiplied by pi.
            /// y[i] = sin(a[i]*PI)
            /// </summary>
            /// <param name="n">The number of elements in the input array.</param>
            /// <param name="a">Pointer to the input array of double values.</param>
            /// <param name="y">Pointer to the output array where the results will be stored.</param>
            public void Sinpi(long n, [In] double* a, [Out] double* y)
                => IntelMKLNative.vdSinpi_64(n, a, y);

            #endregion
            #region ---- Tanpi ----

            /// <summary>
            /// Computes the tangent of each element of the input array multiplied by pi.
            /// y[i] = tan(a[i]*PI)
            /// </summary>
            /// <param name="n">The number of elements in the input array.</param>
            /// <param name="a">Pointer to the input array of double values.</param>
            /// <param name="y">Pointer to the output array where the results will be stored.</param>
            public void Tanpi(long n, [In] double* a, [Out] double* y)
                => IntelMKLNative.vdTanpi_64(n, a, y);

            #endregion
            #region ---- Cosd ----

            /// <summary>
            /// Computes the cosine of each element in the input array <paramref name="a"/> (in degrees)
            /// and stores the result in the output array <paramref name="y"/>.
            /// y[i] = cos(a[i]*PI/180)
            /// </summary>
            /// <param name="n">The number of elements in the input and output arrays.</param>
            /// <param name="a">Pointer to the input array of double values (in degrees).</param>
            /// <param name="y">Pointer to the output array where the cosine values will be stored.</param>
            public void Cosd(long n, [In] double* a, [Out] double* y)
                => IntelMKLNative.vdCosd_64(n, a, y);

            #endregion
            #region ---- Sind ----

            /// <summary>
            /// Computes the sine of each element in the input array <paramref name="a"/> (in degrees)
            /// and stores the result in the output array <paramref name="y"/>.
            /// y[i] = sin(a[i]*PI/180)
            /// </summary>
            /// <param name="n">The number of elements in the input and output arrays.</param>
            /// <param name="a">Pointer to the input array of double values (in degrees).</param>
            /// <param name="y">Pointer to the output array where the sine values will be stored.</param>
            public void Sind(long n, [In] double* a, [Out] double* y)
                => IntelMKLNative.vdSind_64(n, a, y);

            #endregion
            #region ---- Tand ----

            /// <summary>
            /// Computes the tangent (in degrees) of each element in the input array <paramref name="a"/>,
            /// storing the result in the output array <paramref name="y"/>.
            /// y[i] = tan(a[i]*PI/180)
            /// </summary>
            /// <param name="n">The number of elements in the input and output arrays.</param>
            /// <param name="a">Pointer to the input array of double values (angles in degrees).</param>
            /// <param name="y">Pointer to the output array where the tangent values will be stored.</param>
            public void Tand(long n, [In] double* a, [Out] double* y)
                => IntelMKLNative.vdTand_64(n, a, y);

            #endregion
            #region ---- Cosh ----

            /// <summary>
            /// Computes the hyperbolic cosine of each element in the input array <paramref name="a"/> and stores the result in <paramref name="y"/>.
            /// y[i] = ch(a[i])
            /// </summary>
            /// <param name="n">The number of elements in the input array.</param>
            /// <param name="a">Pointer to the input array of type double.</param>
            /// <param name="y">Pointer to the output array of type double where the results will be stored.</param>
            public void Cosh(long n, [In] double* a, [Out] double* y)
                => IntelMKLNative.vdCosh_64(n, a, y);

            /// <summary>
            /// Computes the hyperbolic cosine of each element in the input array <paramref name="a"/> (complex values) and stores the result in <paramref name="y"/>.
            /// y[i] = ch(a[i])
            /// </summary>
            /// <param name="n">The number of elements in the input array.</param>
            /// <param name="a">Pointer to the input array of complex values.</param>
            /// <param name="y">Pointer to the output array of complex values where the results will be stored.</param>
            public void Cosh(long n, [In] void* a, [Out] void* y)
                => IntelMKLNative.vzCosh_64(n, a, y);

            #endregion
            #region ---- Sinh ----

            /// <summary>
            /// Computes the hyperbolic sine of each element in the input array <paramref name="a"/> and stores the result in <paramref name="y"/>.
            /// y[i] = sh(a[i])
            /// </summary>
            /// <param name="n">The number of elements in the input array.</param>
            /// <param name="a">Pointer to the input array of type double.</param>
            /// <param name="y">Pointer to the output array of type double where the results will be stored.</param>
            public void Sinh(long n, [In] double* a, [Out] double* y)
                => IntelMKLNative.vdSinh_64(n, a, y);

            /// <summary>
            /// Computes the hyperbolic sine of each element in the input array <paramref name="a"/> (complex values) and stores the result in <paramref name="y"/>.
            /// y[i] = sh(a[i])
            /// </summary>
            /// <param name="n">The number of elements in the input array.</param>
            /// <param name="a">Pointer to the input array of complex values.</param>
            /// <param name="y">Pointer to the output array of complex values where the results will be stored.</param>
            public void Sinh(long n, [In] void* a, [Out] void* y)
                => IntelMKLNative.vzSinh_64(n, a, y);

            #endregion
            #region ---- Tanh ----

            /// <summary>
            /// Computes the hyperbolic tangent of each element in the input array <paramref name="a"/> and stores the result in <paramref name="y"/>.
            /// y[i] = th(a[i])
            /// </summary>
            /// <param name="n">The number of elements in the input array.</param>
            /// <param name="a">Pointer to the input array of type double.</param>
            /// <param name="y">Pointer to the output array of type double where the results are stored.</param>
            public void Tanh(long n, [In] double* a, [Out] double* y)
                => IntelMKLNative.vdTanh_64(n, a, y);

            /// <summary>
            /// Computes the hyperbolic tangent of each element in the input array <paramref name="a"/> (complex values) and stores the result in <paramref name="y"/>.
            /// y[i] = th(a[i])
            /// </summary>
            /// <param name="n">The number of elements in the input array.</param>
            /// <param name="a">Pointer to the input array of complex values.</param>
            /// <param name="y">Pointer to the output array of complex values where the results are stored.</param>
            public void Tanh(long n, [In] void* a, [Out] void* y)
                => IntelMKLNative.vzTanh_64(n, a, y);

            #endregion
            #region ---- Acos ----

            /// <summary>
            /// Computes the inverse cosine (arccos) of each element in the input array <paramref name="a"/>,
            /// storing the result in the output array <paramref name="y"/>.
            /// y[i] = arccos(a[i])
            /// /summary>
            /// <param name="n">The number of elements in the input and output arrays.</param>
            /// <param name="a">Pointer to the input array of double values.</param>
            /// <param name="y">Pointer to the output array where the results will be stored.</param>
            public void Acos(long n, [In] double* a, [Out] double* y)
                => IntelMKLNative.vdAcos_64(n, a, y);

            /// <summary>
            /// Computes the inverse cosine (arccos) of each element in the input array <paramref name="a"/>,
            /// storing the result in the output array <paramref name="y"/>. This overload supports complex input.
            /// y[i] = arccos(a[i])
            /// </summary>
            /// <param name="n">The number of elements in the input and output arrays.</param>
            /// <param name="a">Pointer to the input array of complex values.</param>
            /// <param name="y">Pointer to the output array where the results will be stored.</param>
            public void Acos(long n, [In] void* a, [Out] void* y)
                => IntelMKLNative.vzAcos_64(n, a, y);

            #endregion
            #region ---- Asin ----

            /// <summary>
            /// Computes the inverse sine (arcsin) of each element in the input array <paramref name="a"/>,
            /// storing the result in the output array <paramref name="y"/>.
            /// y[i] = arcsin(a[i])
            /// </summary>
            /// <param name="n">The number of elements in the input and output arrays.</param>
            /// <param name="a">Pointer to the input array of type double.</param>
            /// <param name="y">Pointer to the output array of type double, where the results will be stored.</param>
            public void Asin(long n, [In] double* a, [Out] double* y)
                => IntelMKLNative.vdAsin_64(n, a, y);

            /// <summary>
            /// Computes the inverse sine (arcsin) of each element in the input array <paramref name="a"/> (complex),
            /// storing the result in the output array <paramref name="y"/> (complex).
            /// y[i] = arcsin(a[i])
            /// </summary>
            /// <param name="n">The number of elements in the input and output arrays.</param>
            /// <param name="a">Pointer to the input array of type complex (void*).</param>
            /// <param name="y">Pointer to the output array of type complex (void*), where the results will be stored.</param>
            public void Asin(long n, [In] void* a, [Out] void* y)
                => IntelMKLNative.vzAsin_64(n, a, y);

            #endregion
            #region ---- Atan ----

            /// <summary>
            /// Computes the element-wise inverse tangent (arctangent) of a double-precision array.
            /// y[i] = arctan(a[i])
            /// </summary>
            /// <param name="n">Number of elements in the input array.</param>
            /// <param name="a">Pointer to the input array of double values.</param>
            /// <param name="y">Pointer to the output array where the results will be stored.</param>
            public void Atan(long n, [In] double* a, [Out] double* y)
                => IntelMKLNative.vdAtan_64(n, a, y);

            /// <summary>
            /// Computes the element-wise inverse tangent (arctangent) of a complex array.
            /// y[i] = arctan(a[i])
            /// </summary>
            /// <param name="n">Number of elements in the input array.</param>
            /// <param name="a">Pointer to the input array of complex values.</param>
            /// <param name="y">Pointer to the output array where the results will be stored.</param>
            public void Atan(long n, [In] void* a, [Out] void* y)
                => IntelMKLNative.vzAtan_64(n, a, y);

            #endregion
            #region ---- Acospi ----

            /// <summary>
            /// Computes the inverse cosine (in units of pi) of each element in the input array <paramref name="a"/>,
            /// storing the result in the output array <paramref name="y"/>.
            /// y[i] = arccos(a[i])/PI
            /// </summary>
            /// <param name="n">The number of elements in the input and output arrays.</param>
            /// <param name="a">Pointer to the input array of double values.</param>
            /// <param name="y">Pointer to the output array where the results will be stored.</param>
            public void Acospi(long n, [In] double* a, [Out] double* y)
                => IntelMKLNative.vdAcospi_64(n, a, y);

            #endregion
            #region ---- Asinpi ----

            /// <summary>
            /// Computes the inverse sine of each element in the input array <paramref name="a"/> (in units of pi), 
            /// and stores the result in the output array <paramref name="y"/>.
            /// y[i] = arcsin(a[i])/PI
            /// </summary>
            /// <param name="n">The number of elements in the input and output arrays.</param>
            /// <param name="a">Pointer to the input array of double values.</param>
            /// <param name="y">Pointer to the output array where the results will be stored.</param>
            public void Asinpi(long n, [In] double* a, [Out] double* y)
                => IntelMKLNative.vdAsinpi_64(n, a, y);

            #endregion
            #region ---- Atanpi ----

            /// <summary>
            /// Computes the inverse tangent (arctangent) of each element in the input array <paramref name="a"/>,
            /// with the result expressed in multiples of π (pi), and stores the result in the output array <paramref name="y"/>.
            /// y[i] = arctan(a[i])/PI
            /// </summary>
            /// <param name="n">The number of elements in the input and output arrays.</param>
            /// <param name="a">Pointer to the input array of double-precision values.</param>
            /// <param name="y">Pointer to the output array where the results will be stored.</param>
            public void Atanpi(long n, [In] double* a, [Out] double* y)
                => IntelMKLNative.vdAtanpi_64(n, a, y);

            #endregion
            #region ---- Acosh ----

            /// <summary>
            /// Computes the inverse hyperbolic cosine (acosh) of each element in the input array <paramref name="a"/>,
            /// storing the result in the output array <paramref name="y"/>.
            /// y[i] = arcch(a[i])
            /// </summary>
            /// <param name="n">The number of elements in the input and output arrays.</param>
            /// <param name="a">Pointer to the input array of double-precision values.</param>
            /// <param name="y">Pointer to the output array where the results will be stored.</param>
            public void Acosh(long n, [In] double* a, [Out] double* y)
                => IntelMKLNative.vdAcosh_64(n, a, y);

            /// <summary>
            /// Computes the inverse hyperbolic cosine (acosh) of each element in the input array <paramref name="a"/>,
            /// storing the result in the output array <paramref name="y"/>. This overload supports complex values.
            /// y[i] = arcch(a[i])
            /// </summary>
            /// <param name="n">The number of elements in the input and output arrays.</param>
            /// <param name="a">Pointer to the input array of complex values.</param>
            /// <param name="y">Pointer to the output array where the results will be stored.</param>
            public void Acosh(long n, [In] void* a, [Out] void* y)
                => IntelMKLNative.vzAcosh_64(n, a, y);

            #endregion
            #region ---- Asinh ----

            /// <summary>
            /// Computes the inverse hyperbolic sine (asinh) of each element in a double-precision array.
            /// y[i] = arcsh(a[i])
            /// </summary>
            /// <param name="n">Number of elements in the input array.</param>
            /// <param name="a">Pointer to the input array of type double.</param>
            /// <param name="y">Pointer to the output array of type double, where the results will be stored.</param>
            public void Asinh(long n, [In] double* a, [Out] double* y)
                => IntelMKLNative.vdAsinh_64(n, a, y);

            /// <summary>
            /// Computes the inverse hyperbolic sine (asinh) of each element in a complex array.
            /// y[i] = arcsh(a[i])
            /// </summary>
            /// <param name="n">Number of elements in the input array.</param>
            /// <param name="a">Pointer to the input array of type Complex (as void*).</param>
            /// <param name="y">Pointer to the output array of type Complex (as void*), where the results will be stored.</param>
            public void Asinh(long n, [In] void* a, [Out] void* y)
                => IntelMKLNative.vzAsinh_64(n, a, y);

            #endregion
            #region ---- Atanh ----

            /// <summary>
            /// Computes the inverse hyperbolic tangent (atanh) of each element in the input array <paramref name="a"/>,
            /// storing the result in the output array <paramref name="y"/>.
            /// y[i] = arcth(a[i])
            /// </summary>
            /// <param name="n">The number of elements in the input and output arrays.</param>
            /// <param name="a">Pointer to the input array of type double.</param>
            /// <param name="y">Pointer to the output array of type double.</param>
            public void Atanh(long n, [In] double* a, [Out] double* y)
                => IntelMKLNative.vdAtanh_64(n, a, y);

            /// <summary>
            /// Computes the inverse hyperbolic tangent (atanh) of each element in the input array <paramref name="a"/>,
            /// storing the result in the output array <paramref name="y"/>. This overload supports complex input and output.
            /// y[i] = arcth(a[i])
            /// </summary>
            /// <param name="n">The number of elements in the input and output arrays.</param>
            /// <param name="a">Pointer to the input array of type Complex (as void*).</param>
            /// <param name="y">Pointer to the output array of type Complex (as void*).</param>
            public void Atanh(long n, [In] void* a, [Out] void* y)
                => IntelMKLNative.vzAtanh_64(n, a, y);

            #endregion
            #region ---- Erf ----

            /// <summary>
            /// Computes the error function erf(a) for each element of the input array <paramref name="a"/>,
            /// storing the result in the output array <paramref name="y"/>.
            /// y[i] = erf(a[i])
            /// </summary>
            /// <param name="n">The number of elements in the input and output arrays.</param>
            /// <param name="a">Pointer to the input array of double values.</param>
            /// <param name="y">Pointer to the output array where the computed error function values will be stored.</param>
            public void Erf(long n, [In] double* a, [Out] double* y)
                => IntelMKLNative.vdErf_64(n, a, y);

            #endregion
            #region ---- ErfInv ----

            /// <summary>
            /// Computes the inverse error function for each element of the input array.
            /// y[i] = erfinv(a[i])
            /// </summary>
            /// <param name="n">The number of elements in the input array.</param>
            /// <param name="a">Pointer to the input array of double values.</param>
            /// <param name="y">Pointer to the output array where the results will be stored.</param>
            public void ErfInv(long n, [In] double* a, [Out] double* y)
                => IntelMKLNative.vdErfInv_64(n, a, y);

            #endregion
            #region ---- Hypot ----

            /// <summary>
            /// Computes the element-wise hypotenuse of two arrays.
            /// For each element i: y[i] = sqrt(a[i]^2 + b[i]^2)
            /// </summary>
            /// <param name="n">The number of elements in the input arrays.</param>
            /// <param name="a">Pointer to the first input array.</param>
            /// <param name="b">Pointer to the second input array.</param>
            /// <param name="y">Pointer to the output array where results are stored.</param>
            public void Hypot(long n, [In] double* a, [In] double* b,
                [Out] double* y)
                => IntelMKLNative.vdHypot_64(n, a, b, y);

            #endregion
            #region ---- Erfc ----

            /// <summary>
            /// Computes the complementary error function for each element in the input array.
            /// y[i] = 1 - erf(a[i])
            /// </summary>
            /// <param name="n">The number of elements in the input array.</param>
            /// <param name="a">Pointer to the input array of double values.</param>
            /// <param name="y">Pointer to the output array where the results will be stored.</param>
            public void Erfc(long n, [In] double* a, [Out] double* y)
                => IntelMKLNative.vdErfc_64(n, a, y);

            #endregion
            #region ---- ErfcInv ----

            /// <summary>
            /// Computes the inverse complementary error function for each element of the input array.
            /// y[i] = erfcinv(a[i])
            /// </summary>
            /// <param name="n">The number of elements in the input array.</param>
            /// <param name="a">Pointer to the input array of double values.</param>
            /// <param name="y">Pointer to the output array where the results will be stored.</param>
            public void ErfcInv(long n, [In] double* a, [Out] double* y)
                => IntelMKLNative.vdErfcInv_64(n, a, y);

            #endregion
            #region ---- Erfcx ----

            /// <summary>
            /// Computes the scaled complementary error function for an array of double-precision values.
            /// y[i] = erfcx(a[i])
            /// </summary>
            /// <param name="n">The number of elements in the input and output arrays.</param>
            /// <param name="a">Pointer to the input array of double-precision values.</param>
            /// <param name="y">Pointer to the output array where the results will be stored.</param>
            public void Erfcx(long n, [In] double* a, [Out] double* y)
                => IntelMKLNative.vdErfcx_64(n, a, y);

            #endregion
            #region ---- CdfNorm ----

            /// <summary>
            /// Computes the cumulative distribution function (CDF) of the standard normal distribution
            /// for each element in the input array <paramref name="a"/> and stores the result in <paramref name="y"/>.
            /// y[i] = cdfnorm(a[i])
            /// </summary>
            /// <param name="n">The number of elements in the input and output arrays.</param>
            /// <param name="a">Pointer to the input array containing the values for which to compute the CDF.</param>
            /// <param name="y">Pointer to the output array where the computed CDF values will be stored.</param>
            public void CdfNorm(long n, [In] double* a, [Out] double* y)
                => IntelMKLNative.vdCdfNorm_64(n, a, y);

            #endregion
            #region ---- CdfNormInv ----

            /// <summary>
            /// Computes the inverse of the cumulative distribution function (quantile function) 
            /// of the standard normal distribution for each element in the input array.
            /// y[i] = cdfnorminv(a[i])
            /// </summary>
            /// <param name="n">The number of elements in the input array <paramref name="a"/> and output array <paramref name="y"/>.</param>
            /// <param name="a">Pointer to the input array containing probability values (in the range [0, 1]).</param>
            /// <param name="y">Pointer to the output array where the computed quantile values will be stored.</param>
            public void CdfNormInv(long n, [In] double* a, [Out] double* y)
                => IntelMKLNative.vdCdfNormInv_64(n, a, y);

            #endregion
            #region ---- LGamma ----

            /// <summary>
            /// Computes the natural logarithm of the gamma function for each element in the input array.
            /// y[i] = lgamma(a[i])
            /// </summary>
            /// <param name="n">The number of elements in the input array.</param>
            /// <param name="a">Pointer to the input array of double values.</param>
            /// <param name="y">Pointer to the output array where the results will be stored.</param>
            public void LGamma(long n, [In] double* a, [Out] double* y)
                => IntelMKLNative.vdLGamma_64(n, a, y);

            #endregion
            #region ---- TGamma ----

            /// <summary>
            /// Computes the gamma function for each element of the input array <paramref name="a"/> and stores the result in <paramref name="y"/>.
            /// y[i] = tgamma(a[i])
            /// </summary>
            /// <param name="n">The number of elements in the input and output arrays.</param>
            /// <param name="a">Pointer to the input array of double values.</param>
            /// <param name="y">Pointer to the output array where the computed gamma values will be stored.</param>
            public void TGamma(long n, [In] double* a, [Out] double* y)
                => IntelMKLNative.vdTGamma_64(n, a, y);

            #endregion
            #region ---- I0 ----

            /// <summary>
            /// Computes the modified Bessel function of the first kind of order zero (I0) for each element in the input array.
            /// y[i] = i0(a[i])
            /// </summary>
            /// <param name="n">The number of elements in the input array <paramref name="a"/> and output array <paramref name="y"/>.</param>
            /// <param name="a">Pointer to the input array of double values.</param>
            /// <param name="y">Pointer to the output array where the computed I0 values will be stored.</param>
            public void I0(long n, [In] double* a, [Out] double* y)
                => IntelMKLNative.vdI0_64(n, a, y);

            #endregion
            #region ---- I1 ----

            /// <summary>
            /// Computes the modified Bessel function of the first kind, order 1, for each element in the input array.
            /// y[i] = i1(a[i])
            /// </summary>
            /// <param name="n">The number of elements in the input array <paramref name="a"/> and output array <paramref name="y"/>.</param>
            /// <param name="a">Pointer to the input array of double values.</param>
            /// <param name="y">Pointer to the output array where the computed values will be stored.</param>
            public void I1(long n, [In] double* a, [Out] double* y)
                => IntelMKLNative.vdI1_64(n, a, y);

            #endregion
            #region ---- J0 ----

            /// <summary>
            /// Computes the Bessel function of the first kind of order 0 (J0) for each element in the input array.
            /// y[i] = j0(a[i])
            /// </summary>
            /// <param name="n">The number of elements in the input array <paramref name="a"/> and output array <paramref name="y"/>.</param>
            /// <param name="a">Pointer to the input array of double values.</param>
            /// <param name="y">Pointer to the output array where the computed J0 values will be stored.</param>
            public void J0(long n, [In] double* a, [Out] double* y)
                => IntelMKLNative.vdJ0_64(n, a, y);

            #endregion
            #region ---- J1 ----

            /// <summary>
            /// Computes the Bessel function of the first kind of order 1 for each element in the input array.
            /// y[i] = j1(a[i])
            /// </summary>
            /// <param name="n">The number of elements in the input array.</param>
            /// <param name="a">Pointer to the input array of double values.</param>
            /// <param name="y">Pointer to the output array where the results will be stored.</param>
            public void J1(long n, [In] double* a, [Out] double* y)
                => IntelMKLNative.vdJ1_64(n, a, y);

            #endregion
            #region ---- Y0 ----

            /// <summary>
            /// Computes the Bessel function of the second kind, order zero, for each element in the input array.
            /// y[i] = y0(a[i])
            /// </summary>
            /// <param name="n">The number of elements in the input array <paramref name="a"/> and output array <paramref name="y"/>.</param>
            /// <param name="a">Pointer to the input array of double values.</param>
            /// <param name="y">Pointer to the output array where the computed results will be stored.</param>
            public void Y0(long n, [In] double* a, [Out] double* y)
                => IntelMKLNative.vdY0_64(n, a, y);

            #endregion
            #region ---- Y1 ----

            /// <summary>
            /// Computes the Bessel function of the second kind, order one (Y1), for each element in the input array.
            /// y[i] = y1(a[i])
            /// </summary>
            /// <param name="n">The number of elements in the input array <paramref name="a"/> and output array <paramref name="y"/>.</param>
            /// <param name="a">Pointer to the input array of double values.</param>
            /// <param name="y">Pointer to the output array where the computed Y1 values will be stored.</param>
            public void Y1(long n, [In] double* a, [Out] double* y)
                => IntelMKLNative.vdY1_64(n, a, y);

            #endregion
            #region ---- Jn ----

            /// <summary>
            /// Computes the Bessel function of the first kind Jn(a[i], b) for each element of the input array.
            /// y[i] = jn(a[i],b)
            /// </summary>
            /// <param name="n">Number of elements in the input array <paramref name="a"/> and output array <paramref name="y"/>.</param>
            /// <param name="a">Pointer to the input array of double values (orders for the Bessel function).</param>
            /// <param name="b">Scalar double value (argument for the Bessel function).</param>
            /// <param name="y">Pointer to the output array of double values where the results will be stored.</param>
            public void Jn(long n, [In] double* a, double b,
                [Out] double* y)
                => IntelMKLNative.vdJn_64(n, a, b, y);

            #endregion
            #region ---- Yn ----

            /// <summary>
            /// Computes the Bessel function of the second kind Yn(a[i], b) for each element in the input array.
            /// y[i] = yn(a[i],b)
            /// </summary>
            /// <param name="n">The number of elements in the input array <paramref name="a"/> and output array <paramref name="y"/>.</param>
            /// <param name="a">Pointer to the input array of orders for the Bessel function.</param>
            /// <param name="b">The value at which the Bessel function is evaluated for all elements.</param>
            /// <param name="y">Pointer to the output array where the results are stored.</param>
            public void Yn(long n, [In] double* a, double b,
                [Out] double* y)
                => IntelMKLNative.vdYn_64(n, a, b, y);

            #endregion
            #region ---- Atan2 ----

            /// <summary>
            /// Computes the four-quadrant inverse tangent (arctangent) of elements of two input arrays.
            /// For each element i, computes y[i] = atan2(a[i], b[i]).
            /// r[i] = arctan(a[i]/b[i])
            /// </summary>
            /// <param name="n">Number of elements in the input arrays.</param>
            /// <param name="a">Pointer to the first input array (numerator values).</param>
            /// <param name="b">Pointer to the second input array (denominator values).</param>
            /// <param name="y">Pointer to the output array where results are stored.</param>
            public void Atan2(long n, [In] double* a, [In] double* b,
                [Out] double* y)
                => IntelMKLNative.vdAtan2_64(n, a, b, y);

            #endregion
            #region ---- Atan2pi ----

            /// <summary>
            /// Computes the four-quadrant inverse tangent (atan2) of elements of two input arrays, divided by π.
            /// r[i] = arctan(a[i]/b[i])/PI
            /// </summary>
            /// <param name="n">The number of elements in the input arrays.</param>
            /// <param name="a">Pointer to the first input array.</param>
            /// <param name="b">Pointer to the second input array.</param>
            /// <param name="y">Pointer to the output array where the results are stored.</param>
            /// <remarks>
            /// For each element i, computes y[i] = atan2(a[i], b[i]) / π.
            /// </remarks>
            public void Atan2pi(long n, [In] double* a, [In] double* b,
                [Out] double* y)
                => IntelMKLNative.vdAtan2pi_64(n, a, b, y);

            #endregion
            #region ---- Mul ----

            /// <summary>
            /// Performs element-wise multiplication of two double arrays.
            /// y[i] = a[i] * b[i]
            /// </summary>
            /// <param name="n">Number of elements in the arrays.</param>
            /// <param name="a">Pointer to the first input array.</param>
            /// <param name="b">Pointer to the second input array.</param>
            /// <param name="y">Pointer to the output array where the result is stored.</param>
            public void Mul(long n, [In] double* a, [In] double* b,
                [Out] double* y)
                => IntelMKLNative.vdMul_64(n, a, b, y);

            /// <summary>
            /// Performs element-wise multiplication of two complex arrays.
            /// y[i] = a[i] * b[i]
            /// </summary>
            /// <param name="n">Number of elements in the arrays.</param>
            /// <param name="a">Pointer to the first input array (complex values).</param>
            /// <param name="b">Pointer to the second input array (complex values).</param>
            /// <param name="y">Pointer to the output array where the result is stored (complex values).</param>
            public void Mul(long n, [In] void* a, [In] void* b,
                [Out] void* y)
                => IntelMKLNative.vzMul_64(n, a, b, y);

            #endregion
            #region ---- Div ----

            /// <summary>
            /// Performs element-wise division of two arrays of doubles.
            /// y[i] = a[i] / b[i]
            /// </summary>
            /// <param name="n">Number of elements in the arrays.</param>
            /// <param name="a">Pointer to the first input array (dividend).</param>
            /// <param name="b">Pointer to the second input array (divisor).</param>
            /// <param name="y">Pointer to the output array (result).</param>
            public void Div(long n, [In] double* a, [In] double* b,
                [Out] double* y)
                => IntelMKLNative.vdDiv_64(n, a, b, y);

            /// <summary>
            /// Performs element-wise division of two arrays of complex numbers.
            /// y[i] = a[i] / b[i]
            /// </summary>
            /// <param name="n">Number of elements in the arrays.</param>
            /// <param name="a">Pointer to the first input array (dividend).</param>
            /// <param name="b">Pointer to the second input array (divisor).</param>
            /// <param name="y">Pointer to the output array (result).</param>
            public void Div(long n, [In] void* a, [In] void* b,
                [Out] void* y)
                => IntelMKLNative.vzDiv_64(n, a, b, y);

            #endregion
            #region ---- Pow ----

            /// <summary>
            /// Computes the element-wise power of two arrays of double-precision floating-point numbers.
            /// Each element of the output array y is computed as y[i] = a[i] ^ b[i].
            /// </summary>
            /// <param name="n">The number of elements in the input arrays.</param>
            /// <param name="a">Pointer to the base array.</param>
            /// <param name="b">Pointer to the exponent array.</param>
            /// <param name="y">Pointer to the output array where the result is stored.</param>
            public void Pow(long n, [In] double* a, [In] double* b,
                [Out] double* y)
                => IntelMKLNative.vdPow_64(n, a, b, y);

            /// <summary>
            /// Computes the element-wise power of two arrays of complex numbers.
            /// Each element of the output array y is computed as y[i] = a[i] ^ b[i].
            /// </summary>
            /// <param name="n">The number of elements in the input arrays.</param>
            /// <param name="a">Pointer to the base array (complex values).</param>
            /// <param name="b">Pointer to the exponent array (complex values).</param>
            /// <param name="y">Pointer to the output array where the result is stored (complex values).</param>
            public void Pow(long n, [In] void* a, [In] void* b,
                [Out] void* y)
                => IntelMKLNative.vzPow_64(n, a, b, y);

            #endregion
            #region ---- Pow3o2 ----

            /// <summary>
            /// Computes the square root of the cube of each element in the input array.
            /// That is, for each element a[i], computes y[i] = a[i]^(3/2).
            /// </summary>
            /// <param name="n">The number of elements in the input array.</param>
            /// <param name="a">Pointer to the input array of doubles.</param>
            /// <param name="y">Pointer to the output array of doubles, where the results are stored.</param>
            public void Pow3o2(long n, [In] double* a, [Out] double* y)
                => IntelMKLNative.vdPow3o2_64(n, a, y);

            #endregion
            #region ---- Pow2o3 ----

            /// <summary>
            /// Computes the cube root of the square of each element in the input array.
            /// y[i] = a[i]^(2/3)
            /// </summary>
            /// <param name="n">The number of elements in the input array.</param>
            /// <param name="a">Pointer to the input array of doubles.</param>
            /// <param name="y">Pointer to the output array of doubles where the results will be stored.</param>
            public void Pow2o3(long n, [In] double* a, [Out] double* y)
                => IntelMKLNative.vdPow2o3_64(n, a, y);

            #endregion
            #region ---- Powx ----

            /// <summary>
            /// Raises each element of the input array <paramref name="a"/> to the scalar power <paramref name="b"/> (double precision).
            /// y[i] = a[i]^b
            /// </summary>
            /// <param name="n">Number of elements in the input array.</param>
            /// <param name="a">Pointer to the input array.</param>
            /// <param name="b">Scalar exponent.</param>
            /// <param name="y">Pointer to the output array where the results are stored.</param>
            public void Powx(long n, [In] double* a, double b, [Out] double* y)
                => IntelMKLNative.vdPowx_64(n, a, b, y);

            /// <summary>
            /// Raises each element of the input array <paramref name="a"/> to the scalar power <paramref name="b"/> (complex double precision).
            /// y[i] = a[i]^b
            /// </summary>
            /// <param name="n">Number of elements in the input array.</param>
            /// <param name="a">Pointer to the input array (complex values).</param>
            /// <param name="b">Pointer to the scalar exponent (complex value).</param>
            /// <param name="y">Pointer to the output array where the results are stored (complex values).</param>
            public void Powx(long n, [In] void* a, void* b, [Out] void* y)
                => IntelMKLNative.vzPowx_64(n, a, b, y);

            #endregion
            #region ---- Powr ----

            /// <summary>
            /// Raises each element of the input array <paramref name="a"/> to the power of the corresponding element in array <paramref name="b"/>,
            /// storing the result in array <paramref name="y"/>.
            /// y[i] = a[i]^b[i]
            /// </summary>
            /// <param name="n">The number of elements in the arrays.</param>
            /// <param name="a">Pointer to the input array of base values.</param>
            /// <param name="b">Pointer to the input array of exponent values.</param>
            /// <param name="y">Pointer to the output array where the results are stored.</param>
            public void Powr(long n, [In] double* a, double* b, [Out] double* y)
                => IntelMKLNative.vdPowr_64(n, a, b, y);

            #endregion
            #region ---- SinCos ----

            /// <summary>
            /// Computes the sine and cosine of each element in the input array <paramref name="a"/>.
            /// y[i] = sin(a[i]), z[i]=cos(a[i])
            /// </summary>
            /// <param name="n">The number of elements in the input array.</param>
            /// <param name="a">Pointer to the input array of double values (angles in radians).</param>
            /// <param name="sin">Pointer to the output array that will receive the sine values.</param>
            /// <param name="cos">Pointer to the output array that will receive the cosine values.</param>
            public void SinCos(long n, [In] double* a,
                [Out] double* sin, [Out] double* cos)
                => IntelMKLNative.vdSinCos_64(n, a, sin, cos);

            #endregion
            #region ---- SinCospi ----

            /// <summary>
            /// Computes the sine and cosine of each element in the input array, where the input values are interpreted as multiples of π (pi).
            /// For each element a[i], computes sin(π * a[i]) and cos(π * a[i]).
            /// y[i] = sinpi(a[i]), z[i]=cospi(a[i])
            /// </summary>
            /// <param name="n">The number of elements in the input array.</param>
            /// <param name="a">Pointer to the input array of double values, each representing a multiple of π.</param>
            /// <param name="sin">Pointer to the output array that will receive the sine values.</param>
            /// <param name="cos">Pointer to the output array that will receive the cosine values.</param>
            public void SinCospi(long n, [In] double* a,
                [Out] double* sin, [Out] double* cos)
                => IntelMKLNative.vdSinCospi_64(n, a, sin, cos);

            #endregion
            #region ---- LinearFrac ----

            /// <summary>
            /// Performs a linear fraction transformation of two double arrays with scalar parameters.
            /// Computes y[i] = (scalea * a[i] + shifta) / (scaleb * b[i] + shiftb) for each element.
            /// </summary>
            /// <param name="n">Number of elements in the arrays.</param>
            /// <param name="a">Pointer to the first input array.</param>
            /// <param name="b">Pointer to the second input array.</param>
            /// <param name="scalea">Scale factor for array a.</param>
            /// <param name="shifta">Shift value for array a.</param>
            /// <param name="scaleb">Scale factor for array b.</param>
            /// <param name="shiftb">Shift value for array b.</param>
            /// <param name="y">Pointer to the output array where the result is stored.</param>
            public void LinearFrac(long n, [In] double* a, [In] double* b,
                double scalea, double shifta, double scaleb, double shiftb,
                [Out] double* y)
                => IntelMKLNative.vdLinearFrac_64(n, a, b, scalea, shifta, scaleb, shiftb, y);

            #endregion
            #region ---- Ceil ----

            /// <summary>
            /// Computes the ceiling of each element in the input array <paramref name="a"/> and stores the result in <paramref name="y"/>.
            /// y[i] = ceil(a[i])
            /// </summary>
            /// <param name="n">The number of elements in the input array.</param>
            /// <param name="a">Pointer to the input array of double values.</param>
            /// <param name="y">Pointer to the output array where the ceiling values will be stored.</param>
            public void Ceil(long n, [In] double* a, [Out] double* y)
                => IntelMKLNative.vdCeil_64(n, a, y);

            #endregion
            #region ---- Floor ----

            /// <summary>
            /// Computes the floor of each element in the input array <paramref name="a"/> and stores the result in <paramref name="y"/>.
            /// y[i] = floor(a[i])
            /// </summary>
            /// <param name="n">The number of elements in the input and output arrays.</param>
            /// <param name="a">Pointer to the input array of double values.</param>
            /// <param name="y">Pointer to the output array where the floored values will be stored.</param>
            public void Floor(long n, [In] double* a, [Out] double* y)
                => IntelMKLNative.vdFloor_64(n, a, y);

            #endregion
            #region ---- Frac ----

            /// <summary>
            /// Computes the fractional part of each element in the input array <paramref name="a"/>,
            /// storing the result in the output array <paramref name="y"/>.
            /// y[i] = a[i] - |a[i]|
            /// </summary>
            /// <param name="n">The number of elements to process.</param>
            /// <param name="a">Pointer to the input array of doubles.</param>
            /// <param name="y">Pointer to the output array of doubles, where the fractional parts will be stored.</param>
            public void Frac(long n, [In] double* a, [Out] double* y)
                => IntelMKLNative.vdFrac_64(n, a, y);

            #endregion
            #region ---- Modf ----

            /// <summary>
            /// Splits each element of the input array <paramref name="a"/> into its fractional and integer parts.
            /// y[i] = |a[i]|, z[i] = a[i] - |a[i]|
            /// </summary>
            /// <param name="n">The number of elements in the input array.</param>
            /// <param name="a">Pointer to the input array of doubles.</param>
            /// <param name="y">Pointer to the output array that will receive the fractional parts.</param>
            /// <param name="i">Pointer to the output array that will receive the integer parts.</param>
            public void Modf(long n, [In] double* a,
                [Out] double* y, [Out] double* i)
                => IntelMKLNative.vdModf_64(n, a, y, i);

            #endregion
            #region ---- Fmod ----

            /// <summary>
            /// Computes the element-wise floating-point remainder of division operation
            /// for two input arrays a and b, storing the result in y.
            /// y[i] = fmod(a[i], b[i])
            /// </summary>
            /// <param name="n">Number of elements in the input arrays.</param>
            /// <param name="a">Pointer to the first input array.</param>
            /// <param name="b">Pointer to the second input array.</param>
            /// <param name="y">Pointer to the output array where the result is stored.</param>
            public void Fmod(long n, [In] double* a, [In] double* b, [Out] double* y)
                => IntelMKLNative.vdFmod_64(n, a, b, y);

            #endregion
            #region ---- Remainder ----

            /// <summary>
            /// Computes the element-wise remainder of division of two double arrays.
            /// y[i] = remainder(a[i], b[i])
            /// </summary>
            /// <param name="n">The number of elements in the arrays.</param>
            /// <param name="a">Pointer to the first input array.</param>
            /// <param name="b">Pointer to the second input array.</param>
            /// <param name="y">Pointer to the output array where the result is stored.</param>
            public void Remainder(long n, [In] double* a, [In] double* b, [Out] double* y)
                => IntelMKLNative.vdRemainder_64(n, a, b, y);

            #endregion
            #region ---- NextAfter ----

            /// <summary>
            /// Computes the next representable double-precision floating-point value after each element of <paramref name="a"/> in the direction of the corresponding element in <paramref name="b"/>.
            /// y[i] = nextafter(a[i], b[i])
            /// </summary>
            /// <param name="n">The number of elements in the input arrays.</param>
            /// <param name="a">Pointer to the input array of double-precision values.</param>
            /// <param name="b">Pointer to the input array indicating the direction for each element in <paramref name="a"/>.</param>
            /// <param name="y">Pointer to the output array where the result is stored.</param>
            public void NextAfter(long n, [In] double* a, [In] double* b, [Out] double* y)
                => IntelMKLNative.vdNextAfter_64(n, a, b, y);

            #endregion
            #region ---- CopySign ----

            /// <summary>
            /// Copies the sign of each element in array <paramref name="b"/> to the corresponding element in array <paramref name="a"/>,
            /// storing the result in array <paramref name="y"/>.
            /// y[i] = copysign(a[i], b[i])
            /// </summary>
            /// <param name="n">The number of elements to process.</param>
            /// <param name="a">Pointer to the input array whose magnitudes are used.</param>
            /// <param name="b">Pointer to the input array whose signs are used.</param>
            /// <param name="y">Pointer to the output array where the result is stored.</param>
            public void CopySign(long n, [In] double* a, [In] double* b, [Out] double* y)
                => IntelMKLNative.vdCopySign_64(n, a, b, y);

            #endregion
            #region ---- Fdim ----

            /// <summary>
            /// Computes the positive difference of two arrays element-wise.
            /// For each element i: y[i] = max(a[i] - b[i], 0)
            /// </summary>
            /// <param name="n">Number of elements in the arrays.</param>
            /// <param name="a">Pointer to the first input array.</param>
            /// <param name="b">Pointer to the second input array.</param>
            /// <param name="y">Pointer to the output array where the result is stored.</param>
            public void Fdim(long n, [In] double* a, [In] double* b, [Out] double* y)
                => IntelMKLNative.vdFdim_64(n, a, b, y);

            #endregion
            #region ---- Fmax ----

            /// <summary>
            /// Computes the element-wise maximum of two double-precision arrays.
            /// y[i] = fmax(a[i], b[i])
            /// </summary>
            /// <param name="n">The number of elements in the input arrays.</param>
            /// <param name="a">Pointer to the first input array.</param>
            /// <param name="b">Pointer to the second input array.</param>
            /// <param name="y">Pointer to the output array where the result is stored.</param>
            public void Fmax(long n, [In] double* a, [In] double* b, [Out] double* y)
                => IntelMKLNative.vdFmax_64(n, a, b, y);

            #endregion
            #region ---- Fmin ----

            /// <summary>
            /// Computes the element-wise minimum of two double-precision arrays.
            /// y[i] = fmin(a[i], b[i])
            /// </summary>
            /// <param name="n">The number of elements in the input arrays.</param>
            /// <param name="a">Pointer to the first input array.</param>
            /// <param name="b">Pointer to the second input array.</param>
            /// <param name="y">Pointer to the output array where the result is stored.</param>
            public void Fmin(long n, [In] double* a, [In] double* b, [Out] double* y)
                => IntelMKLNative.vdFmin_64(n, a, b, y);

            #endregion
            #region ---- MaxMag ----

            /// <summary>
            /// Computes the element-wise maximum magnitude of two double arrays.
            /// y[i] = maxmag(a[i], b[i])
            /// </summary>
            /// <param name="n">The number of elements in the input arrays.</param>
            /// <param name="a">Pointer to the first input array.</param>
            /// <param name="b">Pointer to the second input array.</param>
            /// <param name="y">Pointer to the output array where the result is stored.</param>
            public void MaxMag(long n, [In] double* a, [In] double* b, [Out] double* y)
                => IntelMKLNative.vdMaxMag_64(n, a, b, y);

            #endregion
            #region ---- MinMag ----

            /// <summary>
            /// Computes the element-wise minimum magnitude of two double arrays.
            /// For each element i, y[i] = min(|a[i]|, |b[i]|).
            /// </summary>
            /// <param name="n">Number of elements in the arrays.</param>
            /// <param name="a">Pointer to the first input array.</param>
            /// <param name="b">Pointer to the second input array.</param>
            /// <param name="y">Pointer to the output array where the result is stored.</param>
            public void MinMag(long n, [In] double* a, [In] double* b, [Out] double* y)
                => IntelMKLNative.vdMinMag_64(n, a, b, y);

            #endregion
            #region ---- Round ----

            /// <summary>
            /// Rounds each element of the input array <paramref name="a"/> to the nearest integer value
            /// and stores the result in the output array <paramref name="y"/>.
            /// y[i] = round(a[i])
            /// </summary>
            /// <param name="n">The number of elements to process.</param>
            /// <param name="a">Pointer to the input array of double values.</param>
            /// <param name="y">Pointer to the output array where rounded values will be stored.</param>
            public void Round(long n, [In] double* a, [Out] double* y)
                => IntelMKLNative.vdRound_64(n, a, y);

            #endregion
            #region ---- Conj ----

            /// <summary>
            /// Performs element-wise conjugation of a complex array.
            /// y[i] = conj(a[i])
            /// </summary>
            /// <param name="n">Number of elements in the array.</param>
            /// <param name="a">Pointer to the input array of complex numbers.</param>
            /// <param name="y">Pointer to the output array where the conjugated values will be stored.</param>
            public void Conj(long n, [In] void* a, [Out] void* y)
                => IntelMKLNative.vzConj_64(n, a, y);

            #endregion
            #region ---- MulByConj ----

            /// <summary>
            /// Performs element-wise multiplication of array <paramref name="a"/> and the conjugate of array <paramref name="b"/>.
            /// The result is stored in <paramref name="y"/>.
            /// y[i] = mulbyconj(a[i],b[i])
            /// </summary>
            /// <param name="n">Number of elements to process.</param>
            /// <param name="a">Pointer to the first input array.</param>
            /// <param name="b">Pointer to the second input array (to be conjugated).</param>
            /// <param name="y">Pointer to the output array where the result is stored.</param>
            public void MulByConj(long n, [In] void* a, [In] void* b, [Out] void* y)
                => IntelMKLNative.vzMulByConj_64(n, a, b, y);

            #endregion

            // ...
            #region ---- PackI ----

            /// <summary>
            /// Copies elements of an array with specified indexing to an array with unit increment (double precision).
            /// </summary>
            /// <param name="n">Number of elements to copy.</param>
            /// <param name="a">Pointer to the input array <c>a</c>.</param>
            /// <param name="y">Pointer to the output array <c>y</c> (unit increment).</param>
            /// <param name="inca">Increment for the elements of <c>a</c>. Default is 1.</param>
            public void PackI(long n, [In] double* a, [Out] double* y,
                long inca = 1)
                => IntelMKLNative.vdPackI(n, a, inca, y);

            /// <summary>
            /// Copies elements of a complex array with specified indexing to an array with unit increment.
            /// </summary>
            /// <param name="n">Number of elements to copy.</param>
            /// <param name="a">Pointer to the input complex array <c>a</c>.</param>
            /// <param name="y">Pointer to the output complex array <c>y</c> (unit increment).</param>
            /// <param name="inca">Increment for the elements of <c>a</c>. Default is 1.</param>
            public void PackI(long n, [In] void* a, [Out] void* y,
                long inca = 1)
                => IntelMKLNative.vzPackI(n, a, inca, y);

            #endregion
            #region ---- PackV ----

            /// <summary>
            /// takes out specified elements from one array to another
            /// </summary>
            /// <param name="n"> number of elements to be taken </param>
            /// <param name="a"> source array </param>
            /// <param name="ia"> array of element indices </param>
            /// <param name="y"> result array </param>
            public void PackV(long n, [In] double* a, long* ia, [Out] double* y)
                => IntelMKLNative.vdPackV(n, a, ia, y);

            /// <summary>
            /// takes out specified elements from one array to another
            /// </summary>
            /// <param name="n"> number of elements to be taken </param>
            /// <param name="a"> source array </param>
            /// <param name="ia"> array of element indices </param>
            /// <param name="y"> result array </param>
            public void PackV(long n, [In] void* a, long* ia, [Out] void* y)
                => IntelMKLNative.vzPackV(n, a, ia, y);

            #endregion
            #region ---- PackM ----

            /// <summary>
            /// copies elements of an array with specified indexing 
            /// to an array with unit increment
            /// </summary>
            /// <param name="n"> number of elements </param>
            /// <param name="a"> input array a </param>
            /// <param name="ma"> ... </param>
            /// <param name="y"> result array y </param>
            public void PackM(long n, [In] double* a, long* ma, [Out] double* y)
                => IntelMKLNative.vdPackM(n, a, ma, y);

            /// <summary>
            /// copies elements of an array with specified indexing 
            /// to an array with unit increment
            /// </summary>
            /// <param name="n"> number of elements </param>
            /// <param name="a"> input array a </param>
            /// <param name="y"> result array y </param>
            /// <param name="inca"> increment for the elements of a </param>
            public void PackM(long n, [In] void* a, long* ma, [Out] void* y)
                => IntelMKLNative.vzPackM(n, a, ma, y);

            #endregion

            #region ---- UnpackI ----

            /// <summary>
            /// Copies elements of an array with unit increment to an array with specified indexing (double precision).
            /// </summary>
            /// <param name="n">Number of elements to copy.</param>
            /// <param name="a">Pointer to the input array <c>a</c> (double precision, unit increment).</param>
            /// <param name="y">Pointer to the output array <c>y</c> (double precision, with specified increment).</param>
            /// <param name="incy">Increment for the elements of <c>y</c>. Default is 1.</param>
            public void UnpackI(long n, [In] double* a, [Out] double* y,
                long incy = 1)
                => IntelMKLNative.vdUnpackI(n, a, y, incy);

            /// <summary>
            /// Copies elements of an array with unit increment to an array with specified indexing (complex double precision).
            /// </summary>
            /// <param name="n">Number of elements to copy.</param>
            /// <param name="a">Pointer to the input array <c>a</c> (complex double precision, unit increment).</param>
            /// <param name="y">Pointer to the output array <c>y</c> (complex double precision, with specified increment).</param>
            /// <param name="incy">Increment for the elements of <c>y</c>. Default is 1.</param>
            public void UnpackI(long n, [In] void* a, [Out] void* y,
                long incy = 1)
                => IntelMKLNative.vzUnpackI(n, a, y, incy);

            #endregion
            #region ---- UppackV ----

            /// <summary>
            /// Unpacks a sparse vector into a dense vector using the specified indices.
            /// For each i in [0, n), sets y[iy[i]] = a[i].
            /// </summary>
            /// <param name="n">Number of elements in the sparse vector <paramref name="a"/>.</param>
            /// <param name="a">Pointer to the input sparse vector of double values.</param>
            /// <param name="y">Pointer to the output dense vector of double values.</param>
            /// <param name="iy">Pointer to the array of indices specifying where to unpack each element.</param>
            public void UnpackV(long n, [In] double* a, [Out] double* y, long* iy)
                => IntelMKLNative.vdUnpackV(n, a, y, iy);

            /// <summary>
            /// Unpacks a sparse complex vector into a dense complex vector using the specified indices.
            /// For each i in [0, n), sets y[iy[i]] = a[i].
            /// </summary>
            /// <param name="n">Number of elements in the sparse complex vector <paramref name="a"/>.</param>
            /// <param name="a">Pointer to the input sparse vector of complex values.</param>
            /// <param name="y">Pointer to the output dense vector of complex values.</param>
            /// <param name="iy">Pointer to the array of indices specifying where to unpack each element.</param>
            public void UnpackV(long n, [In] void* a, [Out] void* y, long* iy)
                => IntelMKLNative.vzUnpackV(n, a, y, iy);

            #endregion
            #region ---- UnpackM ----

            /// <summary>
            /// Converts a compressed sparse vector into full-storage form for double precision values.
            /// </summary>
            /// <param name="n">Number of non-zero elements in the sparse vector.</param>
            /// <param name="a">Pointer to the values of the sparse vector.</param>
            /// <param name="y">Pointer to the dense vector to be updated.</param>
            /// <param name="my">Pointer to the indices of the sparse vector elements.</param>
            public void UnpackM(long n, [In] double* a, [Out] double* y, long* my)
                => IntelMKLNative.vdUnpackM(n, a, y, my);

            /// <summary>
            /// Converts a compressed sparse vector into full-storage form for complex values.
            /// </summary>
            /// <param name="n">Number of non-zero elements in the sparse vector.</param>
            /// <param name="a">Pointer to the values of the sparse vector (complex).</param>
            /// <param name="y">Pointer to the dense vector to be updated (complex).</param>
            /// <param name="my">Pointer to the indices of the sparse vector elements.</param>
            public void UnpackM(long n, [In] void* a, [Out] void* y, long* my)
                => IntelMKLNative.vzUnpackM(n, a, y, my);

            #endregion

            #region ---- Part ----

            //...

            #endregion
            #region ---- Modify ----

            //...

            #endregion
        }

        

        public unsafe class DFTI : IFFT
        {
            /// <summary>
            /// DftiCreateDescriptor wrapper (1D)
            /// </summary>
            /// <param name="desc"> DFTI descriptor </param>
            /// <param name="precision"> precision (ConfigValue) </param>
            /// <param name="domain"> domain (ConfigValue) </param>
            /// <param name="dimension"> dimension of the transform </param>
            /// <param name="length"> length given in long format </param>
            /// <returns> error information </returns>
            public int DftiCreateDescriptor(ref IntPtr desc,
                FFTConfigValue precision, FFTConfigValue domain, int dimension, long length)
                => IntelMKLNative.DftiCreateDescriptor(ref desc,
                    (int)precision, (int)domain, dimension, length);

            /// <summary>
            /// DftiCreateDescriptor wrapper (2D)
            /// </summary>
            /// <param name="desc"> DFTI descriptor </param>
            /// <param name="precision"> precision (ConfigValue) </param>
            /// <param name="domain"> domain (ConfigValue) </param>
            /// <param name="dimension"> dimension of the transform </param>
            /// <param name="lengths"> lengths given in long format </param>
            /// <returns> error information </returns>
            public int DftiCreateDescriptor(ref IntPtr desc,
                FFTConfigValue precision, FFTConfigValue domain, int dimension, long[] lengths)
                => IntelMKLNative.DftiCreateDescriptor(ref desc,
                    (int)precision, (int)domain, dimension, lengths);

            /// <summary>
            /// DftiSetValue wrapper
            /// </summary>
            /// <param name="desc"> DFTI descriptor </param>
            /// <param name="config_param"> config_param (ConfigParam) </param>
            /// <param name="config_val"> config_val (int)</param>
            /// <returns> error information </returns>
            public int DftiSetValue(IntPtr desc,
                FFTConfigParam config_param, FFTConfigValue config_val)
                => IntelMKLNative.DftiSetValue(desc,
                    (int)config_param, __arglist(config_val));

            /// <summary>
            /// DftiSetValue wrapper
            /// </summary>
            /// <param name="desc"> DFTI descriptor </param>
            /// <param name="config_param"> config_param (ConfigParam) </param>
            /// <param name="config_val"> config_val (double)</param>
            /// <returns> error information </returns> 
            public int DftiSetValue(IntPtr desc,
                FFTConfigParam config_param, double config_val)
                => IntelMKLNative.DftiSetValue(desc,
                    (int)config_param, __arglist(config_val));

            /// <summary>
            /// DftiCommitDescriptor wrapper
            /// </summary>
            /// <param name="desc"> DFTI descriptor </param>
            /// <returns> error information </returns>
            public int DftiCommitDescriptor(IntPtr desc)
                => IntelMKLNative.DftiCommitDescriptor(desc);

            /// <summary>
            /// DftiComputeForward wrapper
            /// </summary>
            /// <param name="desc"> DFTI descriptor </param>
            /// <param name="x"> array data x (in / out) </param>
            /// <returns> error information </returns>
            public unsafe int DftiComputeForward(IntPtr desc,
                DenseArray<Cplx> x)
            {
                //Complex* px = (Complex*)x.DataPtr.ToPointer();
                //return IntelMKLNative.DftiComputeForward(desc, px);
                return IntelMKLNative.DftiComputeForward(desc, x.VPtr);
            }

            /// <summary>
            /// DftiComputeForward wrapper
            /// </summary>
            /// <param name="desc"> DFTI descriptor </param>
            /// <param name="x_in"> input array data </param>
            /// <param name="x_out"> output array data </param>
            /// <returns> error information </returns>
            public unsafe int DftiComputeForward(IntPtr desc,
                DenseArray<Cplx> x_in, DenseArray<Cplx> x_out)
            {
                //Complex* pin = (Complex*)x_in.DataPtr.ToPointer();
                //Complex* pout = (Complex*)x_out.DataPtr.ToPointer();
                //return IntelMKLNative.DftiComputeForward(desc, pin, pout);
                return IntelMKLNative.DftiComputeForward(desc, 
                    x_in.VPtr, x_out.VPtr);
            }

            /// <summary>
            /// DftiComputeBackward wrapper
            /// </summary>
            /// <param name="desc"> DFTI descriptor </param>
            /// <param name="x"> array data x (in / out) </param>
            /// <returns> error information </returns>
            public unsafe int DftiComputeBackward(IntPtr desc,
                DenseArray<Cplx> x)
            {
                //Complex* px = (Complex*)x.DataPtr.ToPointer();
                //return IntelMKLNative.DftiComputeBackward(desc, px);
                return IntelMKLNative.DftiComputeBackward(desc, x.VPtr);
            }

            /// <summary>
            /// DftiComputeBackward wrapper
            /// </summary>
            /// <param name="desc"> DFTI descriptor </param>
            /// <param name="x_in"> input array data </param>
            /// <param name="x_out"> output array data </param>
            /// <returns> error information </returns>
            public unsafe int DftiComputeBackward(IntPtr desc,
                DenseArray<Cplx> x_in, DenseArray<Cplx> x_out)
            {
                //Complex* pin = (Complex*)x_in.DataPtr.ToPointer();
                //Complex* pout = (Complex*)x_out.DataPtr.ToPointer();
                //return IntelMKLNative.DftiComputeBackward(desc, pin, pout);
                return IntelMKLNative.DftiComputeBackward(desc,
                    x_in.VPtr, x_out.VPtr);
            }

            /// <summary>
            /// DftiFreeDescriptor wrapper
            /// </summary>
            /// <param name="desc"> DFTI descriptor </param>
            /// <returns> error information </returns>
            public int DftiFreeDescriptor(ref IntPtr desc)
                => IntelMKLNative.DftiFreeDescriptor(ref desc);

        }

    }


}
