using System.Runtime.InteropServices;
using Complex = System.Numerics.Complex;

namespace VEMS.MathCore
{
    /// <summary>
    /// Intel MKL class
    /// IBLAS, ILAPACK, IVMF, IFFT 
    /// </summary>
    public class IntelMKL : /*ILAPACK,*/ IFFT, IVSL
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

            // wrapper
            /// <summary>
            /// computes the sum of magnitudes of the elements
            /// dasum := |x[0]| + |x[1]| + ... + |x[n-1]|
            /// </summary>
            /// <typeparam name="T"> ArrayBase[double] </typeparam>
            /// <param name="n"> number of array elements </param>
            /// <param name="x"> array x </param>
            /// <param name="incx"> increment for indexing x </param>
            /// <returns> sum of the elements magnitudes </returns>
            public double AsumD<T>(long n, T x,
                long incx = 1) where T : DenseArrayBase<double>
                => IntelMKLNative.cblas_dasum_64(n, x.DataPtr, incx);

            /// <summary>
            /// computes the sum of magnitudes of the elements
            /// dzasum := |Re(x[0])| + |Im(x[0])| + |Re(x[1])| + |Im(x[1])| + ... + |Re(x[n-1])| + |Im(x[n-1])|
            /// </summary>
            /// <typeparam name="T"> ArrayBase[Complex] </typeparam>
            /// <param name="n"> number of array elements </param>
            /// <param name="x"> array x </param>
            /// <param name="incx"> increment for indexing x </param>
            /// <returns> sum of the elements magnitudes </returns>
            public double AsumZ<T>(long n, T x,
                long incx = 1) where T : DenseArrayBase<Complex>
                => IntelMKLNative.cblas_dzasum_64(n, x.DataPtr, incx);

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


            // wrapper
            /// <summary>
            /// computes a scalar-array product and 
            /// adds the result to another array
            /// y := a*x + y
            /// </summary>
            /// <typeparam name="T"> ArrayBase[double] </typeparam>
            /// <param name="n"> number of array elements </param>
            /// <param name="a"> scalar a </param>
            /// <param name="x"> array x </param>
            /// <param name="y"> array y </param>
            /// <param name="incx"> increment for indexing x </param>
            /// <param name="incy"> increment for indexing y </param>
            public void AxpyD<T>(long n, double a, T x, ref T y,
                long incx = 1, long incy = 1) where T : DenseArrayBase<double>
                => IntelMKLNative.cblas_daxpy_64(n, a, x.DataPtr, incx, y.DataPtr, incy);

            /// <summary>
            /// computes a scalar-array product and 
            /// adds the result to another array
            /// y := a*x + y
            /// </summary>
            /// <typeparam name="T"> ArrayBase[Complex] </typeparam>
            /// <param name="n"> number of array elements </param>
            /// <param name="a"> scalar a </param>
            /// <param name="x"> array x </param>
            /// <param name="y"> array y </param>
            /// <param name="incx"> increment for indexing x </param>
            /// <param name="incy"> increment for indexing y </param>
            public void AxpyZ<T>(long n, Complex a, T x, ref T y,
                long incx = 1, long incy = 1) where T : DenseArrayBase<Complex>
                => IntelMKLNative.cblas_zaxpy_64(n, ref a, x.DataPtr, incx, y.DataPtr, incy);

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

            // temp ...
            /// <summary>
            /// copies x to y with pointers
            /// y := x
            /// </summary>
            /// <param name="n"> number of elements </param>
            /// <param name="x"> pointer to array x </param>
            /// <param name="y"> pointer to array y </param>
            /// <param name="incx"> increment for indexing x </param>
            /// <param name="incy"> increment for indexing y </param>
            public unsafe void Copy(long n, Complex* x, Complex* y,
                long incx = 1, long incy = 1)
                => IntelMKLNative.cblas_zcopy_64(n, x, incx, y, incy);

            // wrapper
            /// <summary>
            /// copies x to y
            /// y := x
            /// </summary>
            /// <typeparam name="T"> ArrayBase[double] </typeparam>
            /// <param name="n"> number of elements </param>
            /// <param name="x"> array x </param>
            /// <param name="y"> array y </param>
            /// <param name="incx"> increment for indexing x </param>
            /// <param name="incy"> increment for indexing y </param>
            public void CopyD<T>(long n, T x, ref T y,
                long incx = 1, long incy = 1) where T : DenseArrayBase<double>
                => IntelMKLNative.cblas_dcopy_64(n, x.DataPtr, incx, y.DataPtr, incy);

            /// <summary>
            /// copies x to y
            /// y := x
            /// </summary>
            /// <typeparam name="T"> ArrayBase[Complex] </typeparam>
            /// <param name="n"> number of elements </param>
            /// <param name="x"> array x </param>
            /// <param name="y"> array y </param>
            /// <param name="incx"> increment for indexing x </param>
            /// <param name="incy"> increment for indexing y </param>
            public void CopyZ<T>(long n, T x, ref T y,
                long incx = 1, long incy = 1) where T : DenseArrayBase<Complex>
                => IntelMKLNative.cblas_zcopy_64(n, x.DataPtr, incx, y.DataPtr, incy);

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


            // wrapper
            /// <summary>
            /// computes a vector-vector dot product
            /// res = x[0]*y[0] + x[1]*y[1] +...+ x[n-1]*y[n-1]
            /// </summary>
            /// <typeparam name="T"> ArrayBase[double] </typeparam>
            /// <param name="n"> number of elements in x </param>
            /// <param name="x"> array x </param>
            /// <param name="y"> array y </param>
            /// <param name="incx"> increment for indexing x </param>
            /// <param name="incy"> increment for indexing y </param>
            /// <returns> result of the dot product of x and y </returns>
            public double DotD<T>(long n, T x, T y,
                long incx = 1, long incy = 1) where T : DenseArrayBase<double>
                => IntelMKLNative.cblas_ddot_64(n, x.DataPtr, incx, y.DataPtr, incy);

            /// <summary>
            /// computes a vector-vector dot product
            /// res = x[0]*y[0] + x[1]*y[1] +...+ x[n-1]*y[n-1]
            /// </summary>
            /// <typeparam name="T"> ArrayBase[Complex] </typeparam>
            /// <param name="n"> number of elements in x </param>
            /// <param name="x"> array x </param>
            /// <param name="y"> array y </param>
            /// <param name="incx"> increment for indexing x </param>
            /// <param name="incy"> increment for indexing y </param>
            /// <returns> result of the dot product of x and y </returns>
            public Complex DotZ<T>(long n, T x, T y,
                long incx = 1, long incy = 1) where T : DenseArrayBase<Complex>
            {
                Complex dotu = 0.0;
                IntelMKLNative.cblas_zdotu_sub_64(n, x.DataPtr, incx, y.DataPtr, incy, ref dotu);
                return dotu;
            }

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
            

            // wrapper
            /// <summary>
            /// computes a dot product of a conjugated vector with another vector
            /// res = conj(x[0])*y[0] + ... + conj(x[n-1])*y[n-1]
            /// </summary>
            /// <typeparam name="T"> ArrayBase[Complex] </typeparam>
            /// <param name="n"> number of elements in x </param>
            /// <param name="x"> array x (to be conjugated) </param>
            /// <param name="y"> array y </param>
            /// <param name="incx"> increment for indexing x </param>
            /// <param name="incy"> increment for indexing y </param>
            /// <returns> result of the conjugate product </returns>
            public Complex DotcZ<T>(long n, T x, T y,
                long incx = 1, long incy = 1) where T : DenseArrayBase<Complex>
            {
                Complex dotc = 0.0;
                IntelMKLNative.cblas_zdotc_sub_64(n, x.DataPtr, incx, y.DataPtr, incy, ref dotc);
                return dotc;
            }

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


            // wrapper
            /// <summary>
            /// computes the Euclidean norm of an array
            /// res = ||x||
            /// </summary>
            /// <typeparam name="T"> ArrayBase[double] </typeparam>
            /// <param name="n"> number of elements in x </param>
            /// <param name="x"> array x </param>
            /// <param name="incx"> increment for indexing x </param>
            /// <returns> Euclidean norm </returns>
            public double Nrm2D<T>(long n, T x,
                long incx = 1) where T : DenseArrayBase<double>
                => IntelMKLNative.cblas_dnrm2_64(n, x.DataPtr, incx);

            /// <summary>
            /// computes the Euclidean norm of an array
            /// res = ||x||
            /// </summary>
            /// <typeparam name="T"> ArrayBase[Complex] </typeparam>
            /// <param name="n"> number of elements in x </param>
            /// <param name="x"> array x </param>
            /// <param name="incx"> increment for indexing x </param>
            /// <returns> Euclidean norm </returns>
            public double Nrm2Z<T>(long n, T x,
                long incx = 1) where T : DenseArrayBase<Complex>
                => IntelMKLNative.cblas_dznrm2_64(n, x.DataPtr, incx);

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


            // wrapper
            /// <summary>
            /// performs rotation of points in the plane
            /// xi = c * xi + s * yi
            /// yi = c * yi - s * xi
            /// </summary>
            /// <typeparam name="T"> ArrayBase[double] </typeparam>
            /// <param name="n"> number of elements in x </param>
            /// <param name="x"> array x </param>
            /// <param name="y"> array y </param>
            /// <param name="c"> scalar c </param>
            /// <param name="s"> scalar s </param>
            /// <param name="incx"> increment for indexing x </param>
            /// <param name="incy"> increment for indexing y </param>
            public void RotD<T>(long n, ref T x, ref T y,
                double c, double s,
                long incx = 1, long incy = 1) where T : DenseArrayBase<double>
                => IntelMKLNative.cblas_drot_64(n, x.DataPtr, incx, y.DataPtr, incy, c, s);

            /// <summary>
            /// performs rotation of points in the plane
            /// xi = c * xi + s * yi
            /// yi = c * yi - s * xi
            /// </summary>
            /// <typeparam name="T"> ArrayBase[Complex] </typeparam>
            /// <param name="n"> number of elements in x </param>
            /// <param name="x"> array x </param>
            /// <param name="y"> array y </param>
            /// <param name="c"> scalar c </param>
            /// <param name="s"> scalar s </param>
            /// <param name="incx"> increment for indexing x </param>
            /// <param name="incy"> increment for indexing y </param>
            public void RotZ<T>(long n, ref T x, ref T y,
                double c, double s,
                long incx = 1, long incy = 1) where T : DenseArrayBase<Complex>
                => IntelMKLNative.cblas_zdrot_64(n, x.DataPtr, incx, y.DataPtr, incy, c, s);

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


            // wrapper
            /// <summary>
            /// computes the product of an array by a scalar
            /// x = a*x 
            /// </summary>
            /// <typeparam name="T"> ArrayBase[double] </typeparam>
            /// <param name="n"> number of elements in x </param>
            /// <param name="a"> scalar a </param>
            /// <param name="x"> array x x </param>
            /// <param name="incx"> increment for indexing x </param>
            public void ScalD<T>(long n, double a, ref T x,
                long incx = 1) where T : DenseArrayBase<double> 
                => IntelMKLNative.cblas_dscal_64(n, a, x.DataPtr, incx);

            /// <summary>
            /// computes the product of an array by a scalar
            /// x = a*x 
            /// </summary>
            /// <typeparam name="T"> ArrayBase[Complex] </typeparam>
            /// <param name="n"> number of elements in x </param>
            /// <param name="a"> scalar a </param>
            /// <param name="x"> array x x </param>
            /// <param name="incx"> increment for indexing x </param>
            public void ScalZd<T>(long n, double a, ref T x,
                long incx = 1) where T : DenseArrayBase<Complex>
                => IntelMKLNative.cblas_zdscal_64(n, a, x.DataPtr, incx);

            /// <summary>
            /// computes the product of an array by a scalar
            /// x = a*x 
            /// </summary>
            /// <param name="n"> number of elements in x </param>
            /// <param name="a"> scalar a </param>
            /// <param name="x"> array x x </param>
            /// <param name="incx"> increment for indexing x </param>
            public void ScalZ<T>(long n, Complex a, ref T x,
                long incx = 1) where T : DenseArrayBase<Complex> 
                => IntelMKLNative.cblas_zscal_64(n, ref a, x.DataPtr, incx);

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


            // wrapper
            /// <summary>
            /// given two arrays x and y, returns array y and x swapped
            /// </summary>
            /// <typeparam name="T"> ArrayBase[double] </typeparam>
            /// <param name="n"> number of elements </param>
            /// <param name="x"> array x </param>
            /// <param name="y"> array y </param>
            /// <param name="incx"> increment for indexing x </param>
            /// <param name="incy"> increment for indexing y </param>
            public void SwapD<T>(long n, ref T x, ref T y,
                long incx = 1, long incy = 1) where T : DenseArrayBase<double>
                => IntelMKLNative.cblas_dswap_64(n, x.DataPtr, incx, y.DataPtr, incy);

            /// <summary>
            /// given two arrays x and y, returns array y and x swapped
            /// </summary>
            /// <typeparam name="T"> ArrayBase[Complex] </typeparam>
            /// <param name="n"> number of elements </param>
            /// <param name="x"> array x </param>
            /// <param name="y"> array y </param>
            /// <param name="incx"> increment for indexing x </param>
            /// <param name="incy"> increment for indexing y </param>
            public void SwapZ<T>(long n, ref T x, ref T y,
                long incx = 1, long incy = 1) where T : DenseArrayBase<Complex>
                => IntelMKLNative.cblas_zswap_64(n, x.DataPtr, incx, y.DataPtr, incy);

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


            // wrapper
            /// <summary>
            /// finds the index of the element with maximum absolute value
            /// </summary>
            /// <typeparam name="T"> ArrayBase[double] </typeparam>
            /// <param name="n"> number of elements in x </param>
            /// <param name="x"> array x </param>
            /// <param name="incx"> increment for indexing x </param>
            /// <returns> index of the element with largest absolute value </returns>
            public long IamaxD<T>(long n, T x,
                long incx = 1) where T : DenseArrayBase<double>
                => IntelMKLNative.cblas_idamax_64(n, x.DataPtr, incx);

            /// <summary>
            /// finds the index of the element with maximum absolute value
            /// </summary>
            /// <typeparam name="T"> ArrayBase[Complex] </typeparam>
            /// <param name="n"> number of elements in x </param>
            /// <param name="x"> array x </param>
            /// <param name="incx"> increment for indexing x </param>
            /// <returns> index of the element with largest absolute value </returns>
            public long IamaxZ<T>(long n, T x,
                long incx = 1) where T : DenseArrayBase<Complex>
                => IntelMKLNative.cblas_izamax_64(n, x.DataPtr, incx);

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
            

            // wrapper
            /// <summary>
            /// finds the index of the element with minimum absolute value
            /// </summary>
            /// <typeparam name="T"> ArrayBase[double] </typeparam>
            /// <param name="n"> number of elements in x </param>
            /// <param name="x"> array x </param>
            /// <param name="incx"> increment for indexing x </param>
            /// <returns> index of the element with smallest absolute value </returns>
            public long IaminD<T>(long n, T x,
                long incx = 1) where T : DenseArrayBase<double>
                => IntelMKLNative.cblas_idamin_64(n, x.DataPtr, incx);

            /// <summary>
            /// finds the index of the element with minimum absolute value
            /// </summary>
            /// <typeparam name="T"> ArrayBase[Complex] </typeparam>
            /// <param name="n"> number of elements in x </param>
            /// <param name="x"> array x </param>
            /// <param name="incx"> increment for indexing x </param>
            /// <returns> index of the element with smallest absolute value </returns>
            public long IaminZ<T>(long n, T x,
                long incx = 1) where T : DenseArrayBase<Complex>
                => IntelMKLNative.cblas_izamin_64(n, x.DataPtr, incx);

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


            // wrapper
            /// <summary>
            /// computes a matrix-vector product using a general matrix
            /// y := alpha * op(a) * x + beta * y           
            /// </summary>
            /// <typeparam name="T1"> Matrix[double] </typeparam>
            /// <typeparam name="T2"> Vector[double] </typeparam>
            /// <param name="layout"> specifies array storage: row- or column-major </param>
            /// <param name="trans"> specifies matrix a transpose operation </param>
            /// <param name="m"> number of rows of the matrix a </param>
            /// <param name="n"> number of columns of the matrix a </param>
            /// <param name="alpha"> scalar alpha </param>
            /// <param name="a"> array (matrix) a </param>
            /// <param name="lda"> leading dimension of a </param>
            /// <param name="x"> array (vector) x </param>
            /// <param name="beta"> scalar beta </param>
            /// <param name="y"> array (vector) y </param>
            /// <param name="incx"> increment for the elements of x </param>
            /// <param name="incy"> increment for the elements of y </param>
            public void GemvD<T1, T2>(BLAS_Layout layout, BLAS_Transpose trans,
                long m, long n, double alpha, T1 a, long lda, 
                T2 x, double beta, ref T2 y,
                long incx = 1, long incy = 1) 
                where T1 : Matrix<double>
                where T2 : Vector<double>
                => IntelMKLNative.cblas_dgemv_64(layout, trans, m, n, 
                    alpha, a.DataPtr, lda, x.DataPtr, incx, beta, y.DataPtr, incy);

            /// <summary>
            /// computes a matrix-vector product using a general matrix
            /// y := alpha * op(a) * x + beta * y           
            /// </summary>
            /// <typeparam name="T1"> Matrix[Complex] </typeparam>
            /// <typeparam name="T2"> Vector[Complex] </typeparam>
            /// <param name="layout"> specifies array storage: row- or column-major </param>
            /// <param name="trans"> specifies matrix a transpose operation </param>
            /// <param name="m"> number of rows of the matrix a </param>
            /// <param name="n"> number of columns of the matrix a </param>
            /// <param name="alpha"> scalar alpha </param>
            /// <param name="a"> array (matrix) a </param>
            /// <param name="lda"> leading dimension of a </param>
            /// <param name="x"> array (vector) x </param>
            /// <param name="beta"> scalar beta </param>
            /// <param name="y"> array (vector) y </param>
            /// <param name="incx"> increment for the elements of x </param>
            /// <param name="incy"> increment for the elements of y </param>
            public void GemvZ<T1, T2>(BLAS_Layout layout, BLAS_Transpose trans,
                long m, long n, Complex alpha, T1 a, long lda,
                T2 x, Complex beta, ref T2 y,
                long incx = 1, long incy = 1)
                where T1 : Matrix<Complex>
                where T2 : Vector<Complex>
                => IntelMKLNative.cblas_zgemv_64(layout, trans, m, n,
                    ref alpha, a.DataPtr, lda, x.DataPtr, incx, ref beta, y.DataPtr, incy);

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


            // wrapper
            /// <summary>
            /// computes a matrix-vector product with a general band matrix
            /// y := alpha * A * x + beta * y
            /// </summary>
            /// <typeparam name="T1"> Matrix[double] </typeparam>
            /// <typeparam name="T2"> Vector[double] </typeparam>
            /// <param name="layout"> specifies array storage: row- or column-major </param>
            /// <param name="trans"> specifies matrix a transpose operation </param>
            /// <param name="m"> number of rows of the matrix a </param>
            /// <param name="n"> number of columns of the matrix a </param>
            /// <param name="kl"> number of sub-diagonals of the matrix a </param>
            /// <param name="ku"> the number of super-diagonals of the matrix a </param>
            /// <param name="alpha"> scalar alpha </param>
            /// <param name="a"> array (matrix) a </param>
            /// <param name="lda"> leading dimension of a </param>
            /// <param name="x"> array (vector) x </param>
            /// <param name="y"> array (vector) y </param>
            /// <param name="beta"> scalar beta </param>
            /// <param name="incx"> increment for the elements of x </param>
            /// <param name="incy"> increment for the elements of y </param>
            public void GemvD<T1, T2>(BLAS_Layout layout, BLAS_Transpose trans,
                long m, long n, long kl, long ku, 
                double alpha, T1 a, long lda, T2 x, double beta, ref T2 y,
                long incx = 1, long incy = 1) 
                where T1: Matrix<double>
                where T2: Vector<double>
                => IntelMKLNative.cblas_dgbmv_64(layout, trans, m, n, kl, ku,
                    alpha, a.DataPtr, lda, x.DataPtr, incx, 
                    beta, y.DataPtr, incy);

            /// <summary>
            /// computes a matrix-vector product with a general band matrix
            /// y := alpha * A * x + beta * y
            /// </summary>
            /// <typeparam name="T1"> Matrix[Complex] </typeparam>
            /// <typeparam name="T2"> Vector[Complex] </typeparam>
            /// <param name="layout"> specifies array storage: row- or column-major </param>
            /// <param name="trans"> specifies matrix a transpose operation </param>
            /// <param name="m"> number of rows of the matrix a </param>
            /// <param name="n"> number of columns of the matrix a </param>
            /// <param name="kl"> number of sub-diagonals of the matrix a </param>
            /// <param name="ku"> the number of super-diagonals of the matrix a </param>
            /// <param name="alpha"> scalar alpha </param>
            /// <param name="a"> array (matrix) a </param>
            /// <param name="lda"> leading dimension of a </param>
            /// <param name="x"> array (vector) x </param>
            /// <param name="y"> array (vector) y </param>
            /// <param name="beta"> scalar beta </param>
            /// <param name="incx"> increment for the elements of x </param>
            /// <param name="incy"> increment for the elements of y </param>
            public void GemvZ<T1, T2>(BLAS_Layout layout, BLAS_Transpose trans,
                long m, long n, long kl, long ku,
                Complex alpha, T1 a, long lda, T2 x, Complex beta, ref T2 y,
                long incx = 1, long incy = 1) 
                where T1 : Matrix<Complex>
                where T2 : Vector<Complex>
                => IntelMKLNative.cblas_zgbmv_64(layout, trans, m, n, kl, ku,
                    ref alpha, a.DataPtr, lda, x.DataPtr, incx,
                    ref beta, y.DataPtr, incy);

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


            // warpper
            /// <summary>
            /// computes a matrix-vector product using a triangular matrix
            /// x := op(A) * x
            /// </summary>
            /// <typeparam name="T1"> Matrix[double] </typeparam>
            /// <typeparam name="T2"> Vector[double] </typeparam>
            /// <param name="layout"> specifies array storage: row- or column-major </param>
            /// <param name="uplo"> specifies whether the matrix a is upper or lower triangular </param>
            /// <param name="trans"> specifies matrix a transpose operation </param>
            /// <param name="diag"> specifies whether the matrix a is unit triangular </param>
            /// <param name="n"> the order of the matrix a </param>
            /// <param name="a"> array (matrix) a </param>
            /// <param name="lda"> leading dimension of a </param>
            /// <param name="x"> array (vector) x </param>
            /// <param name="incx"> increment for the elements of x </param>
            public void TrmvD<T1, T2>(BLAS_Layout layout, BLAS_Uplo uplo, 
                BLAS_Transpose trans, BLAS_Diag diag,
                long n, T1 a, long lda, ref T2 x, long incx = 1) 
                where T1 : Matrix<double>
                where T2 : Vector<double>
                => IntelMKLNative.cblas_dtrmv_64(layout, uplo,
                    trans, diag, n, a.DataPtr, lda, x.DataPtr, incx);

            /// <summary>
            /// computes a matrix-vector product using a triangular matrix
            /// x := op(A) * x
            /// </summary>
            /// <typeparam name="T1"> Matrix[Complex] </typeparam>
            /// <typeparam name="T2"> Vector[Complex] </typeparam>
            /// <param name="layout"> specifies array storage: row- or column-major </param>
            /// <param name="uplo"> specifies whether the matrix a is upper or lower triangular </param>
            /// <param name="trans"> specifies matrix a transpose operation </param>
            /// <param name="diag"> specifies whether the matrix a is unit triangular </param>
            /// <param name="n"> the order of the matrix a </param>
            /// <param name="a"> array (matrix) a </param>
            /// <param name="lda"> leading dimension of a </param>
            /// <param name="x"> array (vector) x </param>
            /// <param name="incx"> increment for the elements of x </param>
            public void TrmvZ<T1, T2>(BLAS_Layout layout, BLAS_Uplo uplo,
                BLAS_Transpose trans, BLAS_Diag diag,
                long n, T1 a, long lda, ref T2 x, long incx = 1)
                where T1 : Matrix<Complex>
                where T2 : Vector<Complex>
                => IntelMKLNative.cblas_dtrmv_64(layout, uplo,
                    trans, diag, n, a.DataPtr, lda, x.DataPtr, incx);

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


            // wrapper
            /// <summary>
            /// computes a matrix-vector product using a triangular band matrix
            /// x := op(A) * x,
            /// </summary>
            /// <typeparam name="T1"> Matrix[double] </typeparam>
            /// <typeparam name="T2"> Vector[double] </typeparam>
            /// <param name="layout"> specifies array storage: row- or column-major </param>
            /// <param name="uplo"> specifies whether the matrix a is upper or lower triangular </param>
            /// <param name="trans"> specifies matrix a transpose operation </param>
            /// <param name="diag"> specifies whether the matrix a is unit triangular </param>
            /// <param name="n"> the order of the matrix a </param>
            /// <param name="k"> the number of super-diagonals of the matrix a </param>
            /// <param name="a"> array (matrix) a </param>
            /// <param name="lda"> leading dimension of a </param>
            /// <param name="x"> array (vector) x </param>
            /// <param name="incx"> increment for the elements of x </param>
            public void TbmvD<T1, T2>(BLAS_Layout layout, BLAS_Uplo uplo,
                BLAS_Transpose trans, BLAS_Diag diag,
                long n, long k, T1 a, long lda, ref T2 x, long incx = 1)
                where T1 : Matrix<double>
                where T2 : Vector<double>
                => IntelMKLNative.cblas_dtbmv_64(layout, uplo,
                    trans, diag, n, k, a.DataPtr, lda, x.DataPtr, incx);

            /// <summary>
            /// computes a matrix-vector product using a triangular band matrix
            /// x := op(A) * x,
            /// </summary>
            /// <typeparam name="T1"> Matrix[Complex] </typeparam>
            /// <typeparam name="T2"> Vector[Complex] </typeparam>
            /// <param name="layout"> specifies array storage: row- or column-major </param>
            /// <param name="uplo"> specifies whether the matrix a is upper or lower triangular </param>
            /// <param name="trans"> specifies matrix a transpose operation </param>
            /// <param name="diag"> specifies whether the matrix a is unit triangular </param>
            /// <param name="n"> the order of the matrix a </param>
            /// <param name="k"> the number of super-diagonals of the matrix a </param>
            /// <param name="a"> array (matrix) a </param>
            /// <param name="lda"> leading dimension of a </param>
            /// <param name="x"> array (vector) x </param>
            /// <param name="incx"> increment for the elements of x </param>
            public void TbmvZ<T1, T2>(BLAS_Layout layout, BLAS_Uplo uplo,
                BLAS_Transpose trans, BLAS_Diag diag,
                long n, long k, T1 a, long lda, ref T2 x, long incx = 1)
                where T1 : Matrix<Complex>
                where T2 : Vector<Complex>
                => IntelMKLNative.cblas_ztbmv_64(layout, uplo,
                    trans, diag, n, k, a.DataPtr, lda, x.DataPtr, incx);

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


            // wrapper
            /// <summary>
            /// computes a matrix-vector product using a triangular packed matrix
            /// x := op(A) * x,
            /// </summary>
            /// <typeparam name="T1"> Matrix[double] </typeparam>
            /// <typeparam name="T2"> Vector[double] </typeparam>
            /// <param name="layout"> specifies array storage: row- or column-major </param>
            /// <param name="uplo"> specifies whether the matrix a is upper or lower triangular </param>
            /// <param name="trans"> specifies matrix a transpose operation </param>
            /// <param name="diag"> specifies whether the matrix a is unit triangular </param>
            /// <param name="n"> the order of the matrix a </param>
            /// <param name="k"> the number of super-diagonals of the matrix a </param>
            /// <param name="a"> array (matrix) a </param>
            /// <param name="x"> array (vector) x </param>
            /// <param name="incx"> increment for the elements of x </param>
            public void TpmvD<T1, T2>(BLAS_Layout layout, BLAS_Uplo uplo,
                BLAS_Transpose trans, BLAS_Diag diag,
                long n, long k, T1 a, ref T2 x, long incx = 1)
                where T1 : Matrix<double>
                where T2 : Vector<double>
                => IntelMKLNative.cblas_dtpmv_64(layout, uplo,
                    trans, diag, n, k, a.DataPtr, x.DataPtr, incx);

            /// <summary>
            /// computes a matrix-vector product using a triangular packed matrix
            /// x := op(A) * x,
            /// </summary>
            /// <typeparam name="T1"> Matrix[Complex] </typeparam>
            /// <typeparam name="T2"> Vector[Complex] </typeparam>
            /// <param name="layout"> specifies array storage: row- or column-major </param>
            /// <param name="uplo"> specifies whether the matrix a is upper or lower triangular </param>
            /// <param name="trans"> specifies matrix a transpose operation </param>
            /// <param name="diag"> specifies whether the matrix a is unit triangular </param>
            /// <param name="n"> the order of the matrix a </param>
            /// <param name="k"> the number of super-diagonals of the matrix a </param>
            /// <param name="a"> array (matrix) a </param>
            /// <param name="x"> array (vector) x </param>
            /// <param name="incx"> increment for the elements of x </param>
            public void TpmvZ<T1, T2>(BLAS_Layout layout, BLAS_Uplo uplo,
                BLAS_Transpose trans, BLAS_Diag diag,
                long n, long k, T1 a, ref T2 x, long incx = 1)
                where T1 : Matrix<Complex> 
                where T2 : Vector<Complex>
                => IntelMKLNative.cblas_ztpmv_64(layout, uplo,
                    trans, diag, n, k, a.DataPtr, x.DataPtr, incx);

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


            // wrapper
            /// <summary>
            /// solves a system of linear equations whose coefficients
            /// are in a triangular matrix op(A) * x = b
            /// </summary>
            /// <typeparam name="T1"> Matrix[double] </typeparam>
            /// <typeparam name="T2"> Vector[double] </typeparam>
            /// <param name="layout"> specifies array storage: row- or column-major </param>
            /// <param name="uplo"> specifies whether the matrix a is upper or lower triangular </param>
            /// <param name="trans"> specifies matrix a transpose operation </param>
            /// <param name="diag"> specifies whether the matrix a is unit triangular </param>
            /// <param name="n"> the order of the matrix a </param>
            /// <param name="a"> array (matrix) a </param>
            /// <param name="lda"> leading dimension of a </param>
            /// <param name="x"> array (vector) x </param>
            /// <param name="incx"> increment for the elements of x </param>
            public void TrsvD<T1, T2>(BLAS_Layout layout, BLAS_Uplo uplo,
                BLAS_Transpose trans, BLAS_Diag diag,
                long n, T1 a, long lda, ref T2 x, long incx = 1)
                where T1 : Matrix<double>
                where T2 : Vector<double>
                => IntelMKLNative.cblas_dtrsv_64(layout, uplo,
                    trans, diag, n, a.DataPtr, lda, x.DataPtr, incx);

            /// <summary>
            /// solves a system of linear equations whose coefficients
            /// are in a triangular matrix op(A) * x = b
            /// </summary>
            /// <typeparam name="T1"> Matrix[Complex] </typeparam>
            /// <typeparam name="T2"> Vector[Complex] </typeparam>
            /// <param name="layout"> specifies array storage: row- or column-major </param>
            /// <param name="uplo"> specifies whether the matrix a is upper or lower triangular </param>
            /// <param name="trans"> specifies matrix a transpose operation </param>
            /// <param name="diag"> specifies whether the matrix a is unit triangular </param>
            /// <param name="n"> the order of the matrix a </param>
            /// <param name="a"> array (matrix) a </param>
            /// <param name="lda"> leading dimension of a </param>
            /// <param name="x"> array (vector) x </param>
            /// <param name="incx"> increment for the elements of x </param>
            public void TrsvZ<T1, T2>(BLAS_Layout layout, BLAS_Uplo uplo,
                BLAS_Transpose trans, BLAS_Diag diag,
                long n, T1 a, long lda, ref T2 x, long incx = 1)
                where T1 : Matrix<Complex> 
                where T2 : Vector<Complex>
                => IntelMKLNative.cblas_ztrsv_64(layout, uplo,
                    trans, diag, n, a.DataPtr, lda, x.DataPtr, incx);

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


            // wrapper
            /// <summary>
            /// solves a system of linear equations whose coefficients
            /// are in a triangular band matrix op(A) * x = b
            /// </summary>
            /// <typeparam name="T1"> Matrix[double] </typeparam>
            /// <typeparam name="T2"> Vector[double] </typeparam>
            /// <param name="layout"> specifies array storage: row- or column-major </param>
            /// <param name="uplo"> specifies whether the matrix a is upper or lower triangular </param>
            /// <param name="trans"> specifies matrix a transpose operation </param>
            /// <param name="diag"> specifies whether the matrix a is unit triangular </param>
            /// <param name="n"> the order of the matrix a </param>
            /// <param name="k"> the number of superdiagonals of the matrix a </param>
            /// <param name="a"> array (matrix) a </param>
            /// <param name="lda"> leading dimension of a </param>
            /// <param name="x"> array (vector) x </param>
            /// <param name="incx"> increment for the elements of x </param>
            public void TbsvD<T1, T2>(BLAS_Layout layout, BLAS_Uplo uplo,
                BLAS_Transpose trans, BLAS_Diag diag,
                long n, long k, T1 a, long lda, ref T2 x, long incx = 1)
                where T1 : Matrix<double>
                where T2 : Vector<double>
                => IntelMKLNative.cblas_dtbsv_64(layout, uplo,
                    trans, diag, n, k, a.DataPtr, lda, x.DataPtr, incx);

            /// <summary>
            /// solves a system of linear equations whose coefficients
            /// are in a triangular band matrix op(A) * x = b
            /// </summary>
            /// <typeparam name="T1"> Matrix[Complex] </typeparam>
            /// <typeparam name="T2"> Vector[Complex] </typeparam>
            /// <param name="layout"> specifies array storage: row- or column-major </param>
            /// <param name="uplo"> specifies whether the matrix a is upper or lower triangular </param>
            /// <param name="trans"> specifies matrix a transpose operation </param>
            /// <param name="diag"> specifies whether the matrix a is unit triangular </param>
            /// <param name="n"> the order of the matrix a </param>
            /// <param name="k"> the number of superdiagonals of the matrix a </param>
            /// <param name="a"> array (matrix) a </param>
            /// <param name="lda"> leading dimension of a </param>
            /// <param name="x"> array (vector) x </param>
            /// <param name="incx"> increment for the elements of x </param>
            public void TbsvZ<T1, T2>(BLAS_Layout layout, BLAS_Uplo uplo,
                BLAS_Transpose trans, BLAS_Diag diag,
                long n, long k, T1 a, long lda, ref T2 x, long incx = 1)
                where T1 : Matrix<Complex> 
                where T2 : Vector<Complex>
                => IntelMKLNative.cblas_ztbsv_64(layout, uplo,
                    trans, diag, n, k, a.DataPtr, lda, x.DataPtr, incx);

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


            // wrapper
            /// <summary>
            /// solves a system of linear equations whose coefficients
            /// are in a triangular packed matrix op(A) * x = b
            /// </summary>
            /// <typeparam name="T1"> Matrix[double] </typeparam>
            /// <typeparam name="T2"> Vector[double] </typeparam>
            /// <param name="layout"> specifies array storage: row- or column-major </param>
            /// <param name="uplo"> specifies whether the matrix a is upper or lower triangular </param>
            /// <param name="trans"> specifies matrix a transpose operation </param>
            /// <param name="diag"> specifies whether the matrix a is unit triangular </param>
            /// <param name="n"> the order of the matrix a </param>
            /// <param name="a"> array (matrix) a </param>
            /// <param name="x"> array (vector) x </param>
            /// <param name="incx"> increment for the elements of x </param>
            public void TpsvD<T1, T2>(BLAS_Layout layout, BLAS_Uplo uplo,
                BLAS_Transpose trans, BLAS_Diag diag,
                long n, T1 a, ref T2 x, long incx = 1)
                where T1 : Matrix<double>
                where T2 : Vector<double>
                => IntelMKLNative.cblas_dtpsv_64(layout, uplo,
                    trans, diag, n, a.DataPtr, x.DataPtr, incx);

            /// <summary>
            /// solves a system of linear equations whose coefficients
            /// are in a triangular packed matrix op(A) * x = b
            /// </summary>
            /// <typeparam name="T1"> Matrix[Complex] </typeparam>
            /// <typeparam name="T2"> Vector[Complex] </typeparam>
            /// <param name="layout"> specifies array storage: row- or column-major </param>
            /// <param name="uplo"> specifies whether the matrix a is upper or lower triangular </param>
            /// <param name="trans"> specifies matrix a transpose operation </param>
            /// <param name="diag"> specifies whether the matrix a is unit triangular </param>
            /// <param name="n"> the order of the matrix a </param>
            /// <param name="a"> array (matrix) a </param>
            /// <param name="x"> array (vector) x </param>
            /// <param name="incx"> increment for the elements of x </param>
            public void TpsvZ<T1, T2>(BLAS_Layout layout, BLAS_Uplo uplo,
                BLAS_Transpose trans, BLAS_Diag diag,
                long n, T1 a, ref T2 x, long incx = 1)
                where T1 : Matrix<Complex> 
                where T2 : Vector<Complex>
                => IntelMKLNative.cblas_ztpsv_64(layout, uplo,
                    trans, diag, n, a.DataPtr, x.DataPtr, incx);

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


            // wrapper
            /// <summary>
            /// computes a matrix-vector product for a symmetric matrix
            /// y := alpha * A * x + beta * y
            /// </summary>
            /// <typeparam name="T1"> Matrix[double] </typeparam>
            /// <typeparam name="T2"> Vector[double] </typeparam>
            /// <param name="layout"> specifies array storage: row- or column-major </param>
            /// <param name="uplo"> specifies whether the matrix a is upper or lower triangular </param>
            /// <param name="n"> the order of the matrix a </param>
            /// <param name="alpha"> scalar alpha </param>
            /// <param name="a"> array (matrix) a </param>
            /// <param name="lda"> leading dimension of a </param>
            /// <param name="x"> array (vector) x </param>
            /// <param name="beta"> scalar beta </param>
            /// <param name="y"> array (vector) y </param>
            /// <param name="incx"> increment for the elements of x </param>
            /// <param name="incy"> increment for the elements of y </param>
            public void SymvD<T1, T2>(BLAS_Layout layout, BLAS_Uplo uplo,
                long n, double alpha, T1 a, long lda, T2 x, double beta, ref T2 y,
                long incx = 1, long incy = 1)
                where T1 : Matrix<double>
                where T2 : Vector<double>
                => IntelMKLNative.cblas_dsymv_64(layout, uplo, n, alpha, 
                    a.DataPtr, lda, x.DataPtr, incx, beta, y.DataPtr, incy);

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


            // wrapper
            /// <summary>
            /// computes a matrix-vector product for a symmetric band matrix
            /// y := alpha * A * x + beta * y
            /// </summary>
            /// <typeparam name="T1"> Matrix[double] </typeparam>
            /// <typeparam name="T2"> Vector[double] </typeparam>
            /// <param name="layout"> specifies array storage: row- or column-major </param>
            /// <param name="uplo"> specifies whether the matrix a is upper or lower triangular </param>
            /// <param name="n"> the order of the matrix a </param>
            /// <param name="k"> the number of super-diagonals of the matrix a </param>
            /// <param name="alpha"> scalar alpha </param>
            /// <param name="a"> array (matrix) a </param>
            /// <param name="lda"> leading dimension of a </param>
            /// <param name="x"> array (vector) x </param>
            /// <param name="beta"> scalar beta </param>
            /// <param name="y"> array (vector) y </param>
            /// <param name="incx"> increment for the elements of x </param>
            /// <param name="incy"> increment for the elements of y </param>
            public void SbmvD<T1, T2>(BLAS_Layout layout, BLAS_Uplo uplo,
                long n, long k, double alpha, T1 a, long lda, T2 x,
                double beta, ref T2 y, long incx = 1, long incy = 1)
                where T1 : Matrix<double>
                where T2 : Vector<double>
                => IntelMKLNative.cblas_dsbmv_64(layout, uplo, n, k, alpha,
                    a.DataPtr, lda, x.DataPtr, incx, beta, y.DataPtr, incy);

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


            // wrapper
            /// <summary>
            /// computes a matrix-vector product for a symmetric packed matrix
            /// y := alpha * A * x + beta * y
            /// </summary>
            /// <typeparam name="T1"> Matrix[double] </typeparam>
            /// <typeparam name="T2"> Vector[double] </typeparam>
            /// <param name="layout"> specifies array storage: row- or column-major </param>
            /// <param name="uplo"> specifies whether the matrix a is upper or lower triangular </param>
            /// <param name="n"> the order of the matrix a </param>
            /// <param name="k"> the number of super-diagonals of the matrix a </param>
            /// <param name="alpha"> scalar alpha </param>
            /// <param name="a"> array (matrix) a </param>
            /// <param name="lda"> leading dimension of a </param>
            /// <param name="x"> array (vector) x </param>
            /// <param name="beta"> scalar beta </param>
            /// <param name="y"> array (vector) y </param>
            /// <param name="incx"> increment for the elements of x </param>
            /// <param name="incy"> increment for the elements of y </param>
            public void SpmvD<T1, T2>(BLAS_Layout layout, BLAS_Uplo uplo,
                long n, double alpha, T1 a, long lda, T2 x,
                double beta, ref T2 y, long incx = 1, long incy = 1)
                where T1 : Matrix<double>
                where T2 : Vector<double>
                => IntelMKLNative.cblas_dspmv_64(layout, uplo, n, alpha,
                    a.DataPtr, lda, x.DataPtr, incx, beta, y.DataPtr, incy);

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


            // wrapper
            /// <summary>
            /// performs a rank-1 update of a general matrix
            /// A := alpha * x * y' + A
            /// </summary>
            /// <typeparam name="T1"> Vector[double] </typeparam>
            /// <typeparam name="T2"> Matrix[double] </typeparam>
            /// <param name="layout"> specifies array storage: row- or column-major </param>
            /// <param name="m"> number of rows of the matrix a </param>
            /// <param name="n"> number of columns of the matrix a </param>
            /// <param name="alpha"> scalar alpha </param>
            /// <param name="x"> array (vector) x </param>
            /// <param name="y"> array (vector) y </param>
            /// <param name="a"> array (matrix) a </param>
            /// <param name="lda"> leading dimension of a </param>
            /// <param name="incx"> increment for the elements of x </param>
            /// <param name="incy"> increment for the elements of y </param>
            public void GerD<T1, T2>(BLAS_Layout layout, long m, long n,
                double alpha, T1 x, T1 y, ref T2 a, long lda, 
                long incx = 1, long incy = 1)
                where T1 : Vector<double>
                where T2 : Matrix<double>
                => IntelMKLNative.cblas_dger_64(layout,m, n, alpha,
                    x.DataPtr, incx, y.DataPtr, incy, a.DataPtr, lda);

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


            // wrapper
            /// <summary>
            /// performs a rank-1 update of a symmetric matrix
            /// A := alpha * x * x' + A
            /// </summary>
            /// <typeparam name="T1"> Vector[double] </typeparam>
            /// <typeparam name="T2"> Matrix[double] </typeparam>
            /// <param name="layout"> specifies array storage: row- or column-major </param>
            /// <param name="uplo"> specifies whether the matrix a is upper or lower triangular </param>
            /// <param name="n"> number of columns of the matrix a </param>
            /// <param name="alpha"> scalar alpha </param>
            /// <param name="x"> array (vector) x </param>
            /// <param name="a"> array (matrix) a </param>
            /// <param name="lda"> leading dimension of a </param>
            /// <param name="incx"> increment for the elements of x </param>
            public void SyrD<T1, T2>(BLAS_Layout layout, BLAS_Uplo uplo,
                long n, double alpha, T1 x, ref T2 a, long lda, long incx = 1)
                where T1 : Vector<double>
                where T2 : Matrix<double>
                => IntelMKLNative.cblas_dsyr_64(layout, uplo, n, alpha,
                    x.DataPtr, incx, a.DataPtr, lda);

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

            // wrapper
            /// <summary>
            /// performs a rank-1 update of a symmetric packed matrix
            /// A := alpha * x * x' + A
            /// </summary>
            /// <typeparam name="T1"> Vector[double] </typeparam>
            /// <typeparam name="T2"> Matrix[double] </typeparam>
            /// <param name="layout"> specifies array storage: row- or column-major </param>
            /// <param name="uplo"> specifies whether the matrix a is upper or lower triangular </param>
            /// <param name="n"> number of columns of the matrix a </param>
            /// <param name="alpha"> scalar alpha </param>
            /// <param name="x"> array (vector) x </param>
            /// <param name="a"> array (matrix) a </param>
            /// <param name="incx"> increment for the elements of x </param>
            public void SprD<T1, T2>(BLAS_Layout layout, BLAS_Uplo uplo,
                long n, double alpha, T1 x, ref T2 a, long incx = 1)
                where T1 : Vector<double>
                where T2 : Matrix<double>
                => IntelMKLNative.cblas_dspr_64(layout, uplo, n, alpha,
                    x.DataPtr, incx, a.DataPtr);

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


            // wrapper
            /// <summary>
            /// performs a rank-2 update of a symmetric matrix
            /// A := alpha * x * y' + alpha * y * x' + A
            /// </summary>
            /// <typeparam name="T1"> Vector[double] </typeparam>
            /// <typeparam name="T2"> Matrix[double] </typeparam>
            /// <param name="layout"> specifies array storage: row- or column-major </param>
            /// <param name="uplo"> specifies whether the matrix a is upper or lower triangular </param>
            /// <param name="n"> number of columns of the matrix a </param>
            /// <param name="alpha"> scalar alpha </param>
            /// <param name="x"> array (vector) x </param>
            /// <param name="y"> array (vector) y </param>
            /// <param name="a"> array (matrix) a </param>
            /// <param name="lda"> leading dimension of a </param>
            /// <param name="incx"> increment for the elements of x </param>
            /// <param name="incy"> increment for the elements of y </param>
            public void Syr2D<T1, T2>(BLAS_Layout layout, BLAS_Uplo uplo,
                long n, double alpha, T1 x, T1 y, ref T2 a, long lda, 
                long incx = 1, long incy = 1)
                where T1 : Vector<double>
                where T2 : Matrix<double>
                => IntelMKLNative.cblas_dsyr2_64(layout, uplo, n, alpha,
                    x.DataPtr, incx, y.DataPtr, incy, a.DataPtr, lda);

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


            // wrapper
            /// <summary>
            /// performs a rank-2 update of a symmetric packed matrix
            /// A := alpha * x * y' + alpha * y * x' + A
            /// </summary>
            /// <typeparam name="T1"> Vector[double] </typeparam>
            /// <typeparam name="T2"> Matrix[double] </typeparam>
            /// <param name="layout"> specifies array storage: row- or column-major </param>
            /// <param name="uplo"> specifies whether the matrix a is upper or lower triangular </param>
            /// <param name="n"> number of columns of the matrix a </param>
            /// <param name="alpha"> scalar alpha </param>
            /// <param name="x"> array (vector) x </param>
            /// <param name="y"> array (vector) y </param>
            /// <param name="a"> array (matrix) a </param>
            /// <param name="incx"> increment for the elements of x </param>
            /// <param name="incy"> increment for the elements of y </param>
            public void Spr2D<T1, T2>(BLAS_Layout layout, BLAS_Uplo uplo,
                long n, double alpha, T1 x, T1 y, ref T2 a, 
                long incx = 1, long incy = 1)
                where T1 : Vector<double>
                where T2 : Matrix<double>
                => IntelMKLNative.cblas_dspr2_64(layout, uplo, n, alpha,
                    x.DataPtr, incx, y.DataPtr, incy, a.DataPtr);

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


            // wrapper
            /// <summary>
            /// computes a matrix-vector product using a Hermitian matrix
            /// y := alpha * A * x + beta * y
            /// </summary>
            /// <typeparam name="T1"> Matrix[Complex] </typeparam>
            /// <typeparam name="T2"> Vector[Complex] </typeparam>
            /// <param name="layout"> specifies array storage: row- or column-major </param>
            /// <param name="uplo"> specifies whether the matrix a is upper or lower triangular </param>
            /// <param name="n"> the order of the matrix a </param>
            /// <param name="alpha"> scalar alpha </param>
            /// <param name="a"> array (matrix) a </param>
            /// <param name="lda"> leading dimension of a </param>
            /// <param name="x"> array (vector) x </param>
            /// <param name="beta"> scalar beta </param>
            /// <param name="y"> array (vector) y </param>
            /// <param name="incx"> increment for the elements of x </param>
            /// <param name="incy"> increment for the elements of y </param>
            public void HemvZ<T1, T2>(BLAS_Layout layout, BLAS_Uplo uplo,
                long n, Complex alpha, T1 a, long lda, T2 x, 
                Complex beta, ref T2 y, long incx = 1, long incy = 1)
                where T1 : Matrix<Complex>
                where T2 : Vector<Complex>
                => IntelMKLNative.cblas_zhemv_64(layout, uplo, n, ref alpha,
                    a.DataPtr, lda, x.DataPtr, incx, ref beta, y.DataPtr, incy);

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


            // wrapper
            /// <summary>
            /// computes a matrix-vector product using a Hermitian band matrix
            /// y := alpha * A * x + beta * y
            /// </summary>
            /// <typeparam name="T1"> Matrix[Complex] </typeparam>
            /// <typeparam name="T2"> Vector[Complex] </typeparam>
            /// <param name="layout"> specifies array storage: row- or column-major </param>
            /// <param name="uplo"> specifies whether the matrix a is upper or lower triangular </param>
            /// <param name="n"> the order of the matrix a </param>
            /// <param name="k"> he number of super-diagonals of the matrix a </param>
            /// <param name="alpha"> scalar alpha </param>
            /// <param name="a"> array (matrix) a </param>
            /// <param name="lda"> leading dimension of a </param>
            /// <param name="x"> array (vector) x </param>
            /// <param name="beta"> scalar beta </param>
            /// <param name="y"> array (vector) y </param>
            /// <param name="incx"> increment for the elements of x </param>
            /// <param name="incy"> increment for the elements of y </param>
            public void HbmvZ<T1, T2>(BLAS_Layout layout, BLAS_Uplo uplo,
                long n, long k, Complex alpha, T1 a, long lda, T2 x,
                Complex beta, ref T2 y, long incx = 1, long incy = 1)
                where T1 : Matrix<Complex>
                where T2 : Vector<Complex>
                => IntelMKLNative.cblas_zhbmv_64(layout, uplo, n, k, ref alpha,
                    a.DataPtr, lda, x.DataPtr, incx, ref beta, y.DataPtr, incy);

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


            // wrapper
            /// <summary>
            /// computes a matrix-vector product using a Hermitian packed matrix
            /// y := alpha * A * x + beta * y
            /// </summary>
            /// <typeparam name="T1"> Matrix[Complex] </typeparam>
            /// <typeparam name="T2"> Vector[Complex] </typeparam>
            /// <param name="layout"> specifies array storage: row- or column-major </param>
            /// <param name="uplo"> specifies whether the matrix a is upper or lower triangular </param>
            /// <param name="n"> the order of the matrix a </param>
            /// <param name="alpha"> scalar alpha </param>
            /// <param name="a"> array (matrix) a </param>
            /// <param name="x"> array (vector) x </param>
            /// <param name="beta"> scalar beta </param>
            /// <param name="y"> array (vector) y </param>
            /// <param name="incx"> increment for the elements of x </param>
            /// <param name="incy"> increment for the elements of y </param>
            public void HpmvZ<T1, T2>(BLAS_Layout layout, BLAS_Uplo uplo,
                long n, Complex alpha, T1 a, T2 x,
                Complex beta, ref T2 y, long incx = 1, long incy = 1)
                where T1 : Matrix<Complex>
                where T2 : Vector<Complex>
                => IntelMKLNative.cblas_zhpmv_64(layout, uplo, n, ref alpha,
                    a.DataPtr, x.DataPtr, incx, ref beta, y.DataPtr, incy);

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


            // wrapper
            /// <summary>
            /// performs a rank-1 update (unconjugated) of a general matrix
            /// A := alpha * x * y' + A
            /// </summary>
            /// <typeparam name="T1"> Vector[Complex] </typeparam>
            /// <typeparam name="T2"> Matrix[Complex] </typeparam>
            /// <param name="layout"> specifies array storage: row- or column-major </param>
            /// <param name="m"> number of rows of the matrix a </param>
            /// <param name="n"> number of columns of the matrix a </param>
            /// <param name="alpha"> scalar alpha </param>
            /// <param name="a"> array (matrix) a </param>
            /// <param name="lda"> leading dimension of a </param>
            /// <param name="x"> array (vector) x </param>
            /// <param name="y"> array (vector) y </param>
            /// <param name="incx"> increment for the elements of x </param>
            /// <param name="incy"> increment for the elements of y </param>
            public void GeruZ<T1, T2>(BLAS_Layout layout, long m, long n, 
                Complex alpha, T1 x, T1 y, ref T2 a, long lda,
                long incx = 1, long incy = 1) 
                where T1 : Vector<Complex>
                where T2 : Matrix<Complex>
                => IntelMKLNative.cblas_zgeru_64(layout, m, n,
                    ref alpha, x.DataPtr, incx, y.DataPtr, incy, a.DataPtr, lda);

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


            // wrapper
            /// <summary>
            /// performs a rank-1 update (conjugated) of a general matrix
            /// A := alpha * x * y' + A
            /// </summary>
            /// <typeparam name="T1"> Vector[Complex] </typeparam>
            /// <typeparam name="T2"> Matrix[Complex] </typeparam>
            /// <param name="layout"> specifies array storage: row- or column-major </param>
            /// <param name="m"> number of rows of the matrix a </param>
            /// <param name="n"> number of columns of the matrix a </param>
            /// <param name="alpha"> scalar alpha </param>
            /// <param name="a"> array (matrix) a </param>
            /// <param name="lda"> leading dimension of a </param>
            /// <param name="x"> array (vector) x </param>
            /// <param name="y"> array (vector) y </param>
            /// <param name="incx"> increment for the elements of x </param>
            /// <param name="incy"> increment for the elements of y </param>
            public void GercZ<T1, T2>(BLAS_Layout layout, long m, long n,
                Complex alpha, T1 x, T1 y, ref T2 a, long lda,
                long incx = 1, long incy = 1) 
                where T1 : Vector<Complex>
                where T2 : Matrix<Complex>
                => IntelMKLNative.cblas_zgerc_64(layout, m, n,
                    ref alpha, x.DataPtr, incx, y.DataPtr, incy, a.DataPtr, lda);

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


            // wrapper
            /// <summary>
            /// performs a rank-1 update of a Hermitian matrix
            /// A := alpha * x * cong(x') + A
            /// </summary>
            /// <typeparam name="T1"> Vector[Complex] </typeparam>
            /// <typeparam name="T2"> Matrix[Complex] </typeparam>
            /// <param name="layout"> specifies array storage: row- or column-major </param>
            /// <param name="uplo"> specifies whether the matrix a is upper or lower triangular </param>
            /// <param name="n"> number of columns of the matrix a </param>
            /// <param name="alpha"> scalar alpha </param>
            /// <param name="a"> array (matrix) a </param>
            /// <param name="lda"> leading dimension of a </param>
            /// <param name="x"> array (vector) x </param>
            /// <param name="incx"> increment for the elements of x </param>
            public void HerZ<T1, T2>(BLAS_Layout layout, BLAS_Uplo uplo, 
                long n, double alpha, T1 x, ref T2 a, long lda, long incx = 1) 
                where T1 : Vector<Complex>
                where T2 : Matrix<Complex>
                => IntelMKLNative.cblas_zher_64(layout, uplo, n,
                    alpha, x.DataPtr, incx, a.DataPtr, lda);

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


            // wrapper
            /// <summary>
            /// performs a rank-1 update of a Hermitian packed matrix
            /// A := alpha * x * cong(x') + A
            /// </summary>
            /// <typeparam name="T1"> Vector[Complex] </typeparam>
            /// <typeparam name="T2"> Matrix[Complex] </typeparam>
            /// <param name="layout"> specifies array storage: row- or column-major </param>
            /// <param name="uplo"> specifies whether the matrix a is upper or lower triangular </param>
            /// <param name="n"> number of columns of the matrix a </param>
            /// <param name="alpha"> scalar alpha </param>
            /// <param name="a"> array (matrix) a </param>
            /// <param name="x"> array (vector) x </param>
            /// <param name="incx"> increment for the elements of x </param>
            public void HprZ<T1, T2>(BLAS_Layout layout, BLAS_Uplo uplo,
                long n, double alpha, T1 x, ref T2 a, long incx = 1) 
                where T1 : Vector<Complex>
                where T2 : Matrix<Complex>
                => IntelMKLNative.cblas_zhpr_64(layout, uplo, n,
                    alpha, x.DataPtr, incx, a.DataPtr);

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


            // wrapper
            /// <summary>
            /// performs a rank-2 update of a Hermitian matrix
            /// A := alpha * x* conj(y') + conj(alpha) * y * conj(x') + A
            /// </summary>
            /// <typeparam name="T1"> Vector[Complex] </typeparam>
            /// <typeparam name="T2"> Matrix[Complex] </typeparam>
            /// <param name="layout"> specifies array storage: row- or column-major </param>
            /// <param name="uplo"> specifies whether the matrix a is upper or lower triangular </param>
            /// <param name="n"> number of columns of the matrix a </param>
            /// <param name="alpha"> scalar alpha </param>
            /// <param name="a"> array (matrix) a </param>
            /// <param name="lda"> leading dimension of a </param>
            /// <param name="x"> array (vector) x </param>
            /// <param name="y"> array (vector) y </param>
            /// <param name="incx"> increment for the elements of x </param>
            /// <param name="incy"> increment for the elements of y </param>
            public void Her2Z<T1, T2>(BLAS_Layout layout, BLAS_Uplo uplo,
                long n, Complex alpha, T1 x, T1 y, ref T2 a, long lda,
                long incx = 1, long incy = 1) 
                where T1 : Vector<Complex>
                where T2 : Matrix<Complex>
                => IntelMKLNative.cblas_zher2_64(layout, uplo, n,
                    ref alpha, x.DataPtr, incx, y.DataPtr, incy, a.DataPtr, lda);

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


            // wrapper
            /// <summary>
            /// performs a rank-2 update of a Hermitian packed matrix
            /// A := alpha * x* conj(y') + conj(alpha) * y * conj(x') + A
            /// </summary>
            /// <typeparam name="T1"> Vector[Complex] </typeparam>
            /// <typeparam name="T2"> Matrix[Complex] </typeparam>
            /// <param name="layout"> specifies array storage: row- or column-major </param>
            /// <param name="uplo"> specifies whether the matrix a is upper or lower triangular </param>
            /// <param name="n"> number of columns of the matrix a </param>
            /// <param name="alpha"> scalar alpha </param>
            /// <param name="a"> array (matrix) a </param>
            /// <param name="x"> array (vector) x </param>
            /// <param name="y"> array (vector) y </param>
            /// <param name="incx"> increment for the elements of x </param>
            /// <param name="incy"> increment for the elements of y </param>
            public void Hpr2Z<T1, T2>(BLAS_Layout layout, BLAS_Uplo uplo,
                long n, Complex alpha, T1 x, T1 y, ref T2 a,
                long incx = 1, long incy = 1) 
                where T1 : Vector<Complex>
                where T2 : Matrix<Complex>
                => IntelMKLNative.cblas_zhpr2_64(layout, uplo, n,
                    ref alpha, x.DataPtr, incx, y.DataPtr, incy, a.DataPtr);

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


            // wrapper
            /// <summary>
            /// computes a matrix-matrix product with general matrices
            /// C := alpha * op(A) * op(B) + beta * C
            /// </summary>
            /// <typeparam name="T"> Matrix[double] </typeparam>
            /// <param name="layout"> specifies array storage: row- or column-major </param>
            /// <param name="transa"> specifies matrix a transpose operation </param>
            /// <param name="transb"> specifies matrix b transpose operation </param>
            /// <param name="m"> number of rows of the matrix op(a) </param>
            /// <param name="n"> number of columns of the matrix op(b) </param>
            /// <param name="k"> number of columns of the matrix op(a) </param>
            /// <param name="alpha"> scalar alpha </param>
            /// <param name="a"> array (matrix) a </param>
            /// <param name="lda"> leading dimension of matrix a </param>
            /// <param name="b"> array (matrix) b </param>
            /// <param name="ldb"> leading dimension of matrix b</param>
            /// <param name="beta"> scalar beta </param>
            /// <param name="c"> array (matrix) c </param>
            /// <param name="ldc"> leading dimension of matrix c </param>
            public void GemmD<T>(BLAS_Layout layout, BLAS_Transpose transa, BLAS_Transpose transb,
                long m, long n, long k, double alpha, T a, long lda, 
                T b, long ldb, double beta, ref T c, long ldc) 
                where T : Matrix<double>
                => IntelMKLNative.cblas_dgemm_64(layout, transa, transb, m, n, k, 
                    alpha, a.DataPtr, lda, b.DataPtr, ldb, beta, c.DataPtr, ldc);

            /// <summary>
            /// computes a matrix-matrix product with general matrices
            /// C := alpha * op(A) * op(B) + beta * C
            /// </summary>
            /// <typeparam name="T"> Matrix[Complex] </typeparam>
            /// <param name="layout"> specifies array storage: row- or column-major </param>
            /// <param name="transa"> specifies matrix a transpose operation </param>
            /// <param name="transb"> specifies matrix b transpose operation </param>
            /// <param name="m"> number of rows of the matrix op(a) </param>
            /// <param name="n"> number of columns of the matrix op(b) </param>
            /// <param name="k"> number of columns of the matrix op(a) </param>
            /// <param name="alpha"> scalar alpha </param>
            /// <param name="a"> array (matrix) a </param>
            /// <param name="lda"> leading dimension of matrix a </param>
            /// <param name="b"> array (matrix) b </param>
            /// <param name="ldb"> leading dimension of matrix b</param>
            /// <param name="beta"> scalar beta </param>
            /// <param name="c"> array (matrix) c </param>
            /// <param name="ldc"> leading dimension of matrix c </param>
            public void GemmZ<T>(BLAS_Layout layout, BLAS_Transpose transa, BLAS_Transpose transb,
                long m, long n, long k, Complex alpha, T a, long lda,
                T b, long ldb, Complex beta, ref T c, long ldc) 
                where T : Matrix<Complex>
                => IntelMKLNative.cblas_zgemm_64(layout, transa, transb, m, n, k, 
                    ref alpha, a.DataPtr, lda, b.DataPtr, ldb, ref beta, c.DataPtr, ldc);

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


            // wrapper
            /// <summary>
            /// computes a matrix-matrix product where one input matrix is symmetric
            /// C := alpha * A * B + beta * C, or
            /// C := alpha * B * A + beta * C
            /// </summary>
            /// <typeparam name="T"> Matrix[double] </typeparam>
            /// <param name="layout"> specifies array storage: row- or column-major </param>
            /// <param name="side"> specifies whether the symmetric matrix a appears on the left or right in the operation </param>
            /// <param name="uplo"> specifies whether the upper or lower triangular part of the symmetric matrix a is used </param>
            /// <param name="m"> number of rows of the matrix op(a) </param>
            /// <param name="n"> number of columns of the matrix op(b) </param>
            /// <param name="alpha"> scalar alpha </param>
            /// <param name="a"> array (matrix) a </param>
            /// <param name="lda"> leading dimension of matrix a </param>
            /// <param name="b"> array (matrix) b </param>
            /// <param name="ldb"> leading dimension of matrix b</param>
            /// <param name="beta"> scalar beta </param>
            /// <param name="c"> array (matrix) c </param>
            /// <param name="ldc"> leading dimension of matrix c </param>
            public void SymmD<T>(BLAS_Layout layout, BLAS_Side side, BLAS_Uplo uplo,
                long m, long n, double alpha, T a, long lda,
                T b, long ldb, double beta, ref T c, long ldc)
                where T : Matrix<double>
                => IntelMKLNative.cblas_dsymm_64(layout, side, uplo, m, n,
                    alpha, a.DataPtr, lda, b.DataPtr, ldb, beta, c.DataPtr, ldc);

            /// <summary>
            /// computes a matrix-matrix product where one input matrix is symmetric
            /// C := alpha * A * B + beta * C, or
            /// C := alpha * B * A + beta * C
            /// </summary>
            /// <typeparam name="T"> Matrix[Complex] </typeparam>
            /// <param name="layout"> specifies array storage: row- or column-major </param>
            /// <param name="side"> specifies whether the symmetric matrix a appears on the left or right in the operation </param>
            /// <param name="uplo"> specifies whether the upper or lower triangular part of the symmetric matrix a is used </param>
            /// <param name="m"> number of rows of the matrix op(a) </param>
            /// <param name="n"> number of columns of the matrix op(b) </param>
            /// <param name="alpha"> scalar alpha </param>
            /// <param name="a"> array (matrix) a </param>
            /// <param name="lda"> leading dimension of matrix a </param>
            /// <param name="b"> array (matrix) b </param>
            /// <param name="ldb"> leading dimension of matrix b</param>
            /// <param name="beta"> scalar beta </param>
            /// <param name="c"> array (matrix) c </param>
            /// <param name="ldc"> leading dimension of matrix c </param>
            public void SymmZ<T>(BLAS_Layout layout, BLAS_Side side, BLAS_Uplo uplo,
                long m, long n, Complex alpha, T a, long lda,
                T b, long ldb, Complex beta, ref T c, long ldc)
                where T : Matrix<Complex>
                => IntelMKLNative.cblas_zsymm_64(layout, side, uplo, m, n,
                    ref alpha, a.DataPtr, lda, b.DataPtr, ldb, ref beta, 
                    c.DataPtr, ldc);

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


            // wrapper 
            /// <summary>
            /// performs a symmetric rank-k update
            /// C := alpha * A * A' + beta * C, or
            /// C := alpha * A' * A + beta * C
            /// </summary>
            /// <typeparam name="T"> Matrix[double] </typeparam>
            /// <param name="layout"> specifies array storage: row- or column-major </param>
            /// <param name="uplo"> specifies whether the upper or lower triangular part of the symmetric matrix a is used </param>
            /// <param name="trans"> specifies matrix a transpose operation </param>
            /// <param name="n"> number of columns of the matrix op(b) </param>
            /// <param name="k"> the number of columns/rows (NoTrans/Trans) of the matrix a</param>
            /// <param name="alpha"> scalar alpha </param>
            /// <param name="a"> array (matrix) a </param>
            /// <param name="lda"> leading dimension of matrix a </param>
            /// <param name="beta"> scalar beta </param>
            /// <param name="c"> array (matrix) c </param>
            /// <param name="ldc"> leading dimension of matrix c </param>
            public void SyrkD<T>(BLAS_Layout layout, BLAS_Uplo uplo,
                BLAS_Transpose trans, long n, long k, 
                double alpha, T a, long lda,
                double beta, ref T c, long ldc) where T : Matrix<double>
                => IntelMKLNative.cblas_dsyrk_64(layout, uplo, trans, n, k,
                    alpha, a.DataPtr, lda, beta, c.DataPtr, ldc);

            /// <summary>
            /// performs a symmetric rank-k update
            /// C := alpha * A * A' + beta * C, or
            /// C := alpha * A' * A + beta * C
            /// </summary>
            /// <typeparam name="T"> Matrix[Complex] </typeparam>
            /// <param name="layout"> specifies array storage: row- or column-major </param>
            /// <param name="uplo"> specifies whether the upper or lower triangular part of the symmetric matrix a is used </param>
            /// <param name="trans"> specifies matrix a transpose operation </param>
            /// <param name="n"> number of columns of the matrix op(b) </param>
            /// <param name="k"> the number of columns/rows (NoTrans/Trans) of the matrix a</param>
            /// <param name="alpha"> scalar alpha </param>
            /// <param name="a"> array (matrix) a </param>
            /// <param name="lda"> leading dimension of matrix a </param>
            /// <param name="beta"> scalar beta </param>
            /// <param name="c"> array (matrix) c </param>
            /// <param name="ldc"> leading dimension of matrix c </param>
            public void SyrkZ<T>(BLAS_Layout layout, BLAS_Uplo uplo,
                BLAS_Transpose trans, long n, long k, 
                Complex alpha, T a, long lda,
                Complex beta, ref T c, long ldc) where T : Matrix<Complex>
                => IntelMKLNative.cblas_zsyrk_64(layout, uplo, trans, n, k,
                    ref alpha, a.DataPtr, lda, ref beta, c.DataPtr, ldc);

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


            // wrapper
            /// <summary>
            /// performs a symmetric rank-2k update
            /// C := alpha * A * B' + alpha * B * A' + beta * C, or
            /// C := alpha * A' * B + alpha * B' * A + beta * C
            /// </summary>
            /// <typeparam name="T"> Matrix[double] </typeparam>
            /// <param name="layout"> specifies array storage: row- or column-major </param>
            /// <param name="uplo"> specifies whether the upper or lower triangular part of the symmetric matrix a is used </param>
            /// <param name="trans"> specifies matrix a transpose operation </param>
            /// <param name="n"> number of columns of the matrix op(b) </param>
            /// <param name="k"> the number of columns/rows (NoTrans/Trans) of the matrix a </param>
            /// <param name="alpha"> scalar alpha </param>
            /// <param name="a"> array (matrix) a </param>
            /// <param name="lda"> leading dimension of matrix a </param>
            /// <param name="b"> array (matrix) b </param>
            /// <param name="ldb"> leading dimension of matrix b </param>
            /// <param name="beta"> scalar beta </param>
            /// <param name="c"> array (matrix) c </param>
            /// <param name="ldc"> leading dimension of matrix c </param>
            public void Syr2kD<T>(BLAS_Layout layout, BLAS_Uplo uplo,
                BLAS_Transpose trans, long n, long k,
                double alpha, T a, long lda, T b, long ldb,
                double beta, ref T c, long ldc) where T : Matrix<double>
                => IntelMKLNative.cblas_dsyr2k_64(layout, uplo, trans, n, k,
                    alpha, a.DataPtr, lda, b.DataPtr, ldb, beta, c.DataPtr, ldc);

            /// <summary>
            /// performs a symmetric rank-2k update
            /// C := alpha * A * B' + alpha * B * A' + beta * C, or
            /// C := alpha * A' * B + alpha * B' * A + beta * C
            /// </summary>
            /// <typeparam name="T"> Matrix[Complex] </typeparam>
            /// <param name="layout"> specifies array storage: row- or column-major </param>
            /// <param name="uplo"> specifies whether the upper or lower triangular part of the symmetric matrix a is used </param>
            /// <param name="trans"> specifies matrix a transpose operation </param>
            /// <param name="n"> number of columns of the matrix op(b) </param>
            /// <param name="k"> the number of columns/rows (NoTrans/Trans) of the matrix a </param>
            /// <param name="alpha"> scalar alpha </param>
            /// <param name="a"> array (matrix) a </param>
            /// <param name="lda"> leading dimension of matrix a </param>
            /// <param name="b"> array (matrix) b </param>
            /// <param name="ldb"> leading dimension of matrix b </param>
            /// <param name="beta"> scalar beta </param>
            /// <param name="c"> array (matrix) c </param>
            /// <param name="ldc"> leading dimension of matrix c </param>
            public void Syr2kZ<T>(BLAS_Layout layout, BLAS_Uplo uplo,
                BLAS_Transpose trans, long n, long k,
                Complex alpha, T a, long lda, T b, long ldb,
                Complex beta, ref T c, long ldc) where T : Matrix<Complex>
                => IntelMKLNative.cblas_zsyr2k_64(layout, uplo, trans, n, k,
                    ref alpha, a.DataPtr, lda, b.DataPtr, ldb,
                    ref beta, c.DataPtr, ldc);

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


            // wrapper
            /// <summary>
            /// computes a matrix-matrix product where one input matrix is triangular
            /// B := alpha * op(A) * B, or
            /// B := alpha * B * op(A)
            /// </summary>
            /// <typeparam name="T"> Matrix[double] </typeparam>
            /// <param name="layout"> specifies array storage: row- or column-major </param>
            /// <param name="side"> specifies whether the symmetric matrix a appears on the left or right in the operation </param>
            /// <param name="uplo"> specifies whether the upper or lower triangular part of the symmetric matrix a is used </param>
            /// <param name="trans"> specifies matrix a transpose operation </param>
            /// <param name="diag"> specifies whether the matrix a is unit triangular </param>
            /// <param name="m"> number of rows of the matrix op(a) </param>
            /// <param name="n"> number of columns of the matrix op(b) </param>
            /// <param name="alpha"> scalar alpha </param>
            /// <param name="a"> array (matrix) a </param>
            /// <param name="lda"> leading dimension of matrix a </param>
            /// <param name="b"> array (matrix) b </param>
            /// <param name="ldb"> leading dimension of matrix b</param>
            public void TrmmD<T>(BLAS_Layout layout, BLAS_Side side, BLAS_Uplo uplo,
                BLAS_Transpose trans, BLAS_Diag diag, long m, long n,
                double alpha, T a, long lda,
                ref T b, long ldb) where T : Matrix<double>
                => IntelMKLNative.cblas_dtrmm_64(layout, side, uplo, trans, diag,
                    m, n, alpha, a.DataPtr, lda, b.DataPtr, ldb);

            /// <summary>
            /// computes a matrix-matrix product where one input matrix is triangular
            /// B := alpha * op(A) * B, or
            /// B := alpha * B * op(A)
            /// </summary>
            /// <typeparam name="T"> Matrix[Complex] </typeparam>
            /// <param name="layout"> specifies array storage: row- or column-major </param>
            /// <param name="side"> specifies whether the symmetric matrix a appears on the left or right in the operation </param>
            /// <param name="uplo"> specifies whether the upper or lower triangular part of the symmetric matrix a is used </param>
            /// <param name="trans"> specifies matrix a transpose operation </param>
            /// <param name="diag"> specifies whether the matrix a is unit triangular </param>
            /// <param name="m"> number of rows of the matrix op(a) </param>
            /// <param name="n"> number of columns of the matrix op(b) </param>
            /// <param name="alpha"> scalar alpha </param>
            /// <param name="a"> array (matrix) a </param>
            /// <param name="lda"> leading dimension of matrix a </param>
            /// <param name="b"> array (matrix) b </param>
            /// <param name="ldb"> leading dimension of matrix b</param>
            public void TrmmZ<T>(BLAS_Layout layout, BLAS_Side side, BLAS_Uplo uplo,
                BLAS_Transpose trans, BLAS_Diag diag, long m, long n,
                Complex alpha, T a, long lda,
                ref T b, long ldb) where T : Matrix<Complex>
                => IntelMKLNative.cblas_ztrmm_64(layout, side, uplo, trans, diag,
                    m, n, ref alpha, a.DataPtr, lda, b.DataPtr, ldb);

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


            // wrapper
            /// <summary>
            /// solves a triangular matrix equation
            /// op(A) * X = alpha * B, or
            /// X * op(A) = alpha * B
            /// </summary>
            /// <typeparam name="T"> Matrix[double] </typeparam>
            /// <param name="layout"> specifies array storage: row- or column-major </param>
            /// <param name="side"> specifies whether the symmetric matrix a appears on the left or right in the operation </param>
            /// <param name="uplo"> specifies whether the upper or lower triangular part of the symmetric matrix a is used </param>
            /// <param name="trans"> specifies matrix a transpose operation </param>
            /// <param name="diag"> specifies whether the matrix a is unit triangular </param>
            /// <param name="m"> number of rows of the matrix op(a) </param>
            /// <param name="n"> number of columns of the matrix op(b) </param>
            /// <param name="alpha"> scalar alpha </param>
            /// <param name="a"> array (matrix) a </param>
            /// <param name="lda"> leading dimension of matrix a </param>
            /// <param name="b"> array (matrix) b </param>
            /// <param name="ldb"> leading dimension of matrix b</param>
            public void TrsmD<T>(BLAS_Layout layout, BLAS_Side side, BLAS_Uplo uplo,
                BLAS_Transpose trans, BLAS_Diag diag, long m, long n,
                double alpha, T a, long lda,
                ref T b, long ldb) where T : Matrix<double>
                => IntelMKLNative.cblas_dtrsm_64(layout, side, uplo, trans, diag,
                    m, n, alpha, a.DataPtr, lda, b.DataPtr, ldb);

            /// <summary>
            /// solves a triangular matrix equation
            /// op(A) * X = alpha * B, or
            /// X * op(A) = alpha * B
            /// </summary>
            /// <typeparam name="T"> Matrix[Complex] </typeparam>
            /// <param name="layout"> specifies array storage: row- or column-major </param>
            /// <param name="side"> specifies whether the symmetric matrix a appears on the left or right in the operation </param>
            /// <param name="uplo"> specifies whether the upper or lower triangular part of the symmetric matrix a is used </param>
            /// <param name="trans"> specifies matrix a transpose operation </param>
            /// <param name="diag"> specifies whether the matrix a is unit triangular </param>
            /// <param name="m"> number of rows of the matrix op(a) </param>
            /// <param name="n"> number of columns of the matrix op(b) </param>
            /// <param name="alpha"> scalar alpha </param>
            /// <param name="a"> array (matrix) a </param>
            /// <param name="lda"> leading dimension of matrix a </param>
            /// <param name="b"> array (matrix) b </param>
            /// <param name="ldb"> leading dimension of matrix b</param>
            public void TrsmZ<T>(BLAS_Layout layout, BLAS_Side side, BLAS_Uplo uplo,
                BLAS_Transpose trans, BLAS_Diag diag, long m, long n,
                Complex alpha, T a, long lda,
                ref T b, long ldb) where T : Matrix<Complex>
                => IntelMKLNative.cblas_ztrsm_64(layout, side, uplo, trans, diag,
                    m, n, ref alpha, a.DataPtr, lda, b.DataPtr, ldb);

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


            // wrapper
            /// <summary>
            /// computes a matrix-matrix product where one input matrix is Hermitian
            /// C := alpha * A * B + beta * C, or
            /// C := alpha * B * A + beta * C
            /// </summary>
            /// <typeparam name="T"> Matrix[Complex] </typeparam>
            /// <param name="layout"> specifies array storage: row- or column-major </param>
            /// <param name="side"> specifies whether the symmetric matrix a appears on the left or right in the operation </param>
            /// <param name="uplo"> specifies whether the upper or lower triangular part of the symmetric matrix a is used </param>
            /// <param name="m"> number of rows of the matrix op(a) </param>
            /// <param name="n"> number of columns of the matrix op(b) </param>
            /// <param name="alpha"> scalar alpha </param>
            /// <param name="a"> array (matrix) a </param>
            /// <param name="lda"> leading dimension of matrix a </param>
            /// <param name="b"> array (matrix) b </param>
            /// <param name="ldb"> leading dimension of matrix b</param>
            /// <param name="beta"> scalar beta </param>
            /// <param name="c"> array (matrix) c </param>
            /// <param name="ldc"> leading dimension of matrix c </param>
            public void HemmZ<T>(BLAS_Layout layout, BLAS_Side side, BLAS_Uplo uplo,
                long m, long n, Complex alpha, T a, long lda,
                T b, long ldb, Complex beta, ref T c, long ldc)
                where T : Matrix<Complex>
                => IntelMKLNative.cblas_zhemm_64(layout, side, uplo, m, n,
                    ref alpha, a.DataPtr, lda, b.DataPtr, ldb, ref beta, c.DataPtr, ldc);

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


            // wrapper
            /// <summary>
            /// performs a Hermitian rank-k update
            /// C := alpha * A * AH + beta * C, or
            /// C := alpha * AH * A + beta * C
            /// </summary>
            /// <typeparam name="T"> Matrix[Complex] </typeparam>
            /// <param name="layout"> specifies array storage: row- or column-major </param>
            /// <param name="uplo"> specifies whether the upper or lower triangular part of the symmetric matrix a is used </param>
            /// <param name="trans"> specifies matrix a transpose operation </param>
            /// <param name="n"> number of columns of the matrix op(b) </param>
            /// <param name="k"> the number of columns/rows (NoTrans/Trans) of the matrix a </param>
            /// <param name="alpha"> scalar alpha </param>
            /// <param name="a"> array (matrix) a </param>
            /// <param name="lda"> leading dimension of matrix a </param>
            /// <param name="beta"> scalar beta </param>
            /// <param name="c"> array (matrix) c </param>
            /// <param name="ldc"> leading dimension of matrix c </param>
            public void HerkZ<T>(BLAS_Layout layout, BLAS_Uplo uplo,
                BLAS_Transpose trans, long n, long k, 
                double alpha, T a, long lda, double beta, ref T c, long ldc)
                where T : Matrix<Complex>
                => IntelMKLNative.cblas_zherk_64(layout, uplo, trans, n, k,
                    alpha, a.DataPtr, lda, beta, c.DataPtr, ldc);

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


            // warpper
            /// <summary>
            /// performs a Hermitian rank-2k update
            /// C := alpha * A * BH + conj(alpha) * B * AH + beta * C, or
            /// C := alpha * AH * B + conj(alpha) * BH * A + beta * C
            /// </summary>
            /// <typeparam name="T"> Matrix[Complex] </typeparam>
            /// <param name="layout"> specifies array storage: row- or column-major </param>
            /// <param name="uplo"> specifies whether the upper or lower triangular part of the symmetric matrix a is used </param>
            /// <param name="trans"> specifies matrix a transpose operation </param>
            /// <param name="n"> number of columns of the matrix op(b) </param>
            /// <param name="k"> the number of columns/rows (NoTrans/Trans) of the matrix a </param>
            /// <param name="alpha"> scalar alpha </param>
            /// <param name="a"> array (matrix) a </param>
            /// <param name="lda"> leading dimension of matrix a </param>
            /// <param name="b"> array (matrix) b </param>
            /// <param name="ldb"> leading dimension of matrix b </param>
            /// <param name="beta"> scalar beta </param>
            /// <param name="c"> array (matrix) c </param>
            /// <param name="ldc"> leading dimension of matrix c </param>
            public void Her2kZ<T>(BLAS_Layout layout, BLAS_Uplo uplo,
                BLAS_Transpose trans, long n, long k,
                Complex alpha, T a, long lda, T b, long ldb,
                Complex beta, ref T c, long ldc)
                where T : Matrix<Complex>
                => IntelMKLNative.cblas_zher2k_64(layout, uplo, trans, n, k,
                    ref alpha, a.DataPtr, lda, b.DataPtr, ldb, 
                    ref beta, c.DataPtr, ldc);

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


            // wrapper
            /// <summary>
            /// performs scaling and in-place transposition/copying of matrices
            /// </summary>
            /// <typeparam name="T"> Matrix[double] </typeparam>
            /// <param name="layout"> layout of the input matrix </param>
            /// <param name="operation"> whether to transpose the matrix </param>
            /// <param name="rows"> number of rows before operation </param>
            /// <param name="cols"> number of columns before operation </param>
            /// <param name="alpha"> scaling factor alpha </param>
            /// <param name="ab"> array (matrix) ab - overwritten on exit </param>
            /// <param name="lda"> leading dimension of matrix before operation </param>
            /// <param name="ldb"> leading dimension of matrix after operation </param>
            public void ImatCopyD<T>(BLAS_Layout layout, BLAS_Transpose operation,
                long rows, long cols, double alpha, ref T ab, 
                long lda, long ldb) where T : Matrix<double>
                => IntelMKLNative.MKL_Dimatcopy(GetLayoutChar(layout), GetOperationChar(operation),
                    rows, cols, alpha, ab.DataPtr, lda, ldb);

            /// <summary>
            /// performs scaling and in-place transposition/copying of matrices
            /// </summary>
            /// <typeparam name="T"> Matrix[Complex] </typeparam>
            /// <param name="layout"> layout of the input matrix </param>
            /// <param name="operation"> whether to transpose the matrix </param>
            /// <param name="rows"> number of rows before operation </param>
            /// <param name="cols"> number of columns before operation </param>
            /// <param name="alpha"> scaling factor alpha </param>
            /// <param name="ab"> array (matrix) ab - overwritten on exit </param>
            /// <param name="lda"> leading dimension of matrix before operation </param>
            /// <param name="ldb"> leading dimension of matrix after operation </param>
            public void ImatCopyZ<T>(BLAS_Layout layout, BLAS_Transpose operation,
                long rows, long cols, Complex alpha, ref T ab, 
                long lda, long ldb) where T : Matrix<Complex>
                => IntelMKLNative.MKL_Zimatcopy(GetLayoutChar(layout), GetOperationChar(operation),
                    rows, cols, ref alpha, ab.DataPtr, lda, ldb);

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


            // wrapper
            /// <summary>
            /// performs scaling and out-place transposition/copying of matrices
            /// </summary>
            /// <typeparam name="T"> Matrix[double] </typeparam>
            /// <param name="layout"> layout of the input matrix </param>
            /// <param name="operation"> whether to transpose the matrix </param>
            /// <param name="rows"> number of rows before operation </param>
            /// <param name="cols"> number of columns before operation </param>
            /// <param name="alpha"> scaling factor alpha </param>
            /// <param name="a"> array (matrix) a before operation </param>
            /// <param name="lda"> leading dimension of matrix before operation </param>
            /// <param name="b"> array (matrix) b after operation </param>
            /// <param name="ldb"> leading dimension of matrix after operation </param>
            public void OmatCopyD<T>(BLAS_Layout layout, BLAS_Transpose operation,
                long rows, long cols, double alpha, T a, long lda,
                ref T b, long ldb) where T : Matrix<double>
                => IntelMKLNative.MKL_Domatcopy(GetLayoutChar(layout), GetOperationChar(operation),
                    rows, cols, alpha, a.DataPtr, lda, b.DataPtr, ldb);

            /// <summary>
            /// performs scaling and out-place transposition/copying of matrices
            /// </summary>
            /// <typeparam name="T"> Matrix[Complex] </typeparam>
            /// <param name="layout"> layout of the input matrix </param>
            /// <param name="operation"> whether to transpose the matrix </param>
            /// <param name="rows"> number of rows before operation </param>
            /// <param name="cols"> number of columns before operation </param>
            /// <param name="alpha"> scaling factor alpha </param>
            /// <param name="a"> array (matrix) a before operation </param>
            /// <param name="lda"> leading dimension of matrix before operation </param>
            /// <param name="b"> array (matrix) b after operation </param>
            /// <param name="ldb"> leading dimension of matrix after operation </param>
            public void OmatCopyZ<T>(BLAS_Layout layout, BLAS_Transpose operation,
                long rows, long cols, Complex alpha, T a, long lda,
                ref T b, long ldb) where T : Matrix<Complex>
                => IntelMKLNative.MKL_Zomatcopy(GetLayoutChar(layout), GetOperationChar(operation),
                    rows, cols, ref alpha, a.DataPtr, lda, b.DataPtr, ldb);

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


            // wrapper
            /// <summary>
            /// computes the sum of magnitudes of the elements
            /// dasum := |x[0]| + |x[1]| + ... + |x[n-1]|
            /// </summary>
            /// <typeparam name="T"> SPVector[double] </typeparam>
            /// <param name="n"> number of non-zero array elements </param>
            /// <param name="x"> sparse vector x </param>
            /// <param name="incx"> increment for indexing x </param>
            /// <returns> sum of the elements magnitudes </returns>
            public double AsumD<T>(long n, T x,
                long incx = 1) where T : SPVector<double>
                => IntelMKLNative.cblas_dasum_64(n, x.NzValues.DataPtr, incx);

            /// <summary>
            /// computes the sum of magnitudes of the elements
            /// dzasum := |Re(x[0])| + |Im(x[0])| + |Re(x[1])| + |Im(x[1])| + ... + |Re(x[n-1])| + |Im(x[n-1])|
            /// </summary>
            /// <typeparam name="T"> SPVector[Complex] </typeparam>
            /// <param name="n"> number of non-zero array elements </param>
            /// <param name="x"> sparse vector x </param>
            /// <param name="incx"> increment for indexing x </param>
            /// <returns> sum of the elements magnitudes </returns>
            public double AsumZ<T>(long n, T x,
                long incx = 1) where T : SPVector<Complex>
                => IntelMKLNative.cblas_dzasum_64(n, x.NzValues.DataPtr, incx);

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


            // wrapper
            /// <summary>
            /// adds a scalar multiple of compressed sparse vector to
            /// a full-storage vector  y := a * x + y
            /// </summary>
            /// <typeparam name="T1"> SPVector[double] </typeparam>
            /// <typeparam name="T2"> Vector[double] </typeparam>
            /// <param name="n"> number of non-zero array elements </param>
            /// <param name="a"> scalar a </param>
            /// <param name="x"> sparse vector x </param>
            /// <param name="y"> dense vector y </param>
            public void AxpyD<T1, T2>(long n, double a, T1 x, ref T2 y) 
                where T1 : SPVector<double>
                where T2 : Vector<double>
                => IntelMKLNative.cblas_daxpyi_64(n, a, 
                    x.NzValues.DataPtr, x.NzIndices.DataPtr, 
                    y.DataPtr);

            /// <summary>
            /// adds a scalar multiple of compressed sparse vector to
            /// a full-storage vector  y := a * x + y
            /// </summary>
            /// <typeparam name="T1"> SPVector[Complex] </typeparam>
            /// <typeparam name="T2"> Vector[Complex] </typeparam>
            /// <param name="n"> number of non-zero array elements </param>
            /// <param name="a"> scalar a </param>
            /// <param name="x"> sparse vector x </param>
            /// <param name="y"> dense vector y </param>
            public void AxpyZ<T1, T2>(long n, Complex a, T1 x, ref T2 y)
                where T1 : SPVector<Complex>
                where T2 : Vector<Complex>
                => IntelMKLNative.cblas_zaxpyi_64(n, ref a,
                    x.NzValues.DataPtr, x.NzIndices.DataPtr,
                    y.DataPtr);

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


            // wrapper
            /// <summary>
            /// copies x to y
            /// y := x
            /// </summary>
            /// <typeparam name="T"> SPVector[double] </typeparam>
            /// <param name="n"> number non-zero of elements </param>
            /// <param name="x"> sparse vector x </param>
            /// <param name="y"> sparse vector y </param>
            /// <param name="incx"> increment for indexing x </param>
            /// <param name="incy"> increment for indexing y </param>
            public void CopyD<T>(long n, T x, ref T y,
                long incx = 1, long incy = 1) where T : SPVector<double>
                => IntelMKLNative.cblas_dcopy_64(n, 
                    x.NzValues.DataPtr, incx, 
                    y.NzValues.DataPtr, incy);

            /// <summary>
            /// copies x to y
            /// y := x
            /// </summary>
            /// <typeparam name="T"> SPVector[Complex] </typeparam>
            /// <param name="n"> number non-zero of elements </param>
            /// <param name="x"> sparse vector x </param>
            /// <param name="y"> sparse vector y </param>
            /// <param name="incx"> increment for indexing x </param>
            /// <param name="incy"> increment for indexing y </param>
            public void CopyZ<T>(long n, T x, ref T y,
                long incx = 1, long incy = 1) where T : SPVector<Complex>
                => IntelMKLNative.cblas_zcopy_64(n, 
                    x.NzValues.DataPtr, incx, 
                    y.NzValues.DataPtr, incy);

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


            // wrapper
            /// <summary>
            /// computes a vector-vector dot product
            /// res = x[0]*y[indx[0]] + x[1]*y[indx[1]] +...+ x[nz-1]*y[indx[nz-1]]
            /// </summary>
            /// <typeparam name="T1"> SPVector[double] </typeparam>
            /// <typeparam name="T2"> Vector[double] </typeparam>
            /// <param name="n"> number of non-zero elements in x </param>
            /// <param name="x"> sparse vector x </param>
            /// <param name="y"> dense vector y </param>
            /// <returns> result of the dot product of x and y </returns>
            public double DotD<T1, T2>(long n, T1 x, T2 y) 
                where T1 : SPVector<double>
                where T2 : Vector<double>
                => IntelMKLNative.cblas_ddoti_64(n, 
                    x.NzValues.DataPtr, x.NzIndices.DataPtr, 
                    y.DataPtr);

            /// <summary>
            /// computes a vector-vector dot product
            /// res = x[0]*y[indx[0]] + x[1]*y[indx[1]] +...+ x[nz-1]*y[indx[nz-1]]
            /// </summary>
            /// <typeparam name="T1"> SPVector[double] </typeparam>
            /// <typeparam name="T2"> Vector[double] </typeparam>
            /// <param name="n"> number of non-zero elements in x </param>
            /// <param name="x"> sparse vector x </param>
            /// <param name="y"> dense vector y </param>
            /// <returns> result of the dot product of x and y </returns>
            public Complex DotZ<T1, T2>(long n, T1 x, T2 y)
                where T1 : SPVector<Complex>
                where T2 : Vector<Complex>
            {
                Complex dotu = 0.0;
                IntelMKLNative.cblas_zdotui_sub_64(n,
                    x.NzValues.DataPtr, x.NzIndices.DataPtr,
                    y.DataPtr, ref dotu);
                return dotu;
            }

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


            // wrapper
            /// <summary>
            /// computes a dot product of a conjugated vector with another vector
            /// res = conj(x[0])*y[0] + ... + conj(x[n-1])*y[n-1]
            /// </summary>
            /// <typeparam name="T1"> SPVector[double] </typeparam>
            /// <typeparam name="T2"> Vector[double] </typeparam>
            /// <param name="n"> number of non-zero elements in x </param>
            /// <param name="x"> sparse vector x (to be conjugated) </param>
            /// <param name="y"> dense vector y </param>
            /// <returns> result of the conjugate product </returns>
            public Complex DotcZ<T1, T2>(long n, T1 x, T2 y) 
                where T1 : SPVector<Complex>
                where T2 : Vector<Complex> 
            {
                Complex dotc = 0.0;
                IntelMKLNative.cblas_zdotci_sub_64(n, 
                    x.NzValues.DataPtr, x.NzIndices.DataPtr,
                    y.DataPtr, ref dotc);
                return dotc;
            }

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


            // wrapper
            /// <summary>
            /// computes the Euclidean norm of an array
            /// res = ||x||
            /// </summary>
            /// <typeparam name="T"> SPVector[double] </typeparam>
            /// <param name="n"> number of non-zero elements in x </param>
            /// <param name="x"> sparse vector x </param>
            /// <param name="incx"> increment for indexing x </param>
            /// <returns> Euclidean norm </returns>
            public double Nrm2D<T>(long n, T x,
                long incx = 1) where T : SPVector<double>
                => IntelMKLNative.cblas_dnrm2_64(n, x.NzValues.DataPtr, incx);

            /// <summary>
            /// computes the Euclidean norm of an array
            /// res = ||x||
            /// </summary>
            /// <typeparam name="T"> SPVector[Complex] </typeparam>
            /// <param name="n"> number of non-zero elements in x </param>
            /// <param name="x"> sparse vector x </param>
            /// <param name="incx"> increment for indexing x </param>
            /// <returns> Euclidean norm </returns>
            public double Nrm2Z<T>(long n, T x,
                long incx = 1) where T : SPVector<Complex>
                => IntelMKLNative.cblas_dznrm2_64(n, x.NzValues.DataPtr, incx);

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


            // wrapper
            /// <summary>
            /// performs rotation of points in the plane
            /// x[i] = c*x[i] + s*y[indx[i]]
            /// y[indx[i]] = c*y[indx[i]] - s*x[i]
            /// </summary>
            /// <typeparam name="T1"> SPVector[double] </typeparam>
            /// <typeparam name="T2"> Vector[double] </typeparam>
            /// <param name="n"> number of non-zero elements in x </param>
            /// <param name="x"> sparse vector x </param>
            /// <param name="y"> array y </param>
            /// <param name="c"> scalar c </param>
            /// <param name="s"> scalar s </param>
            public void RotD<T1, T2>(long n, ref T1 x, ref T2 y,
                double c, double s) 
                where T1 : SPVector<double>
                where T2: Vector<double>
                => IntelMKLNative.cblas_droti_64(n, 
                    x.NzValues.DataPtr, x.NzIndices.DataPtr,
                    y.DataPtr, c, s);

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


            // wrapper
            /// <summary>
            /// computes the product of an array by a scalar
            /// x = a*x 
            /// </summary>
            /// <typeparam name="T"> SPVector[double] </typeparam>
            /// <param name="n"> number of non-zero elements in x </param>
            /// <param name="a"> scalar a </param>
            /// <param name="x"> sparse vector x </param>
            /// <param name="incx"> increment for indexing x </param>
            public void ScalD<T>(long n, double a, ref T x,
                long incx = 1) where T : SPVector<double>
                => IntelMKLNative.cblas_dscal_64(n, a, 
                    x.NzValues.DataPtr, incx);

            /// <summary>
            /// computes the product of an array by a scalar
            /// x = a*x 
            /// </summary>
            /// <typeparam name="T"> SPVector[Complex] </typeparam>
            /// <param name="n"> number of non-zero elements in x </param>
            /// <param name="a"> scalar a </param>
            /// <param name="x"> sparse vector x </param>
            /// <param name="incx"> increment for indexing x </param>
            public void ScalZd<T>(long n, double a, ref T x,
                long incx = 1) where T : SPVector<Complex>
                => IntelMKLNative.cblas_zdscal_64(n, a, 
                    x.NzValues.DataPtr, incx);

            /// <summary>
            /// computes the product of an array by a scalar
            /// x = a*x 
            /// </summary>
            /// <typeparam name="T"> SPVector[Complex] </typeparam>
            /// <param name="n"> number of non-zero elements in x </param>
            /// <param name="a"> scalar a </param>
            /// <param name="x"> sparse vector x </param>
            /// <param name="incx"> increment for indexing x </param>
            public void ScalZ<T>(long n, Complex a, ref T x,
                long incx = 1) where T : SPVector<Complex>
                => IntelMKLNative.cblas_zscal_64(n, ref a, 
                    x.NzValues.DataPtr, incx);

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


            // wrapper
            /// <summary>
            /// finds the index of the element with maximum absolute value
            /// </summary>
            /// <typeparam name="T"> SPVector[double] </typeparam>
            /// <param name="n"> number of non-zero elements in x </param>
            /// <param name="x"> sparse vector x </param>
            /// <param name="incx"> increment for indexing x </param>
            /// <returns> index of the element with largest absolute value </returns>
            public long IamaxD<T>(long n, T x,
                long incx = 1) where T : SPVector<double>
                => IntelMKLNative.cblas_idamax_64(n, x.NzValues.DataPtr, incx);

            /// <summary>
            /// finds the index of the element with maximum absolute value
            /// </summary>
            /// <typeparam name="T"> SPVector[Complex] </typeparam>
            /// <param name="n"> number of non-zero elements in x </param>
            /// <param name="x"> sparse vector x </param>
            /// <param name="incx"> increment for indexing x </param>
            /// <returns> index of the element with largest absolute value </returns>
            public long IamaxZ<T>(long n, T x,
                long incx = 1) where T : SPVector<Complex>
                => IntelMKLNative.cblas_izamax_64(n, x.NzValues.DataPtr, incx);

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


            // wrapper
            /// <summary>
            /// finds the index of the element with minimum absolute value
            /// </summary>
            /// <typeparam name="T"> SPVector[double] </typeparam>
            /// <param name="n"> number of non-zero elements in x </param>
            /// <param name="x"> sparse vector x </param>
            /// <param name="incx"> increment for indexing x </param>
            /// <returns> index of the element with smallest absolute value </returns>
            public long IaminD<T>(long n, T x,
                long incx = 1) where T : SPVector<double>
                => IntelMKLNative.cblas_idamin_64(n, x.NzValues.DataPtr, incx);

            /// <summary>
            /// finds the index of the element with minimum absolute value
            /// </summary>
            /// <typeparam name="T"> SPVector[Complex] </typeparam>
            /// <param name="n"> number of non-zero elements in x </param>
            /// <param name="x"> sparse vector x </param>
            /// <param name="incx"> increment for indexing x </param>
            /// <returns> index of the element with smallest absolute value </returns>
            public long IaminZ<T>(long n, T x,
                long incx = 1) where T : SPVector<Complex>
                => IntelMKLNative.cblas_izamin_64(n, x.NzValues.DataPtr, incx);

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


            // wrapper
            /// <summary>
            /// gathers a full-storage sparse vector's elements into
            /// compressed form x[i] = y[indx[i]]
            /// </summary>
            /// <typeparam name="T1"> Vector[double] </typeparam>
            /// <typeparam name="T2"> SPVector[double] </typeparam>
            /// <param name="n"> number of non-zero array elements </param>
            /// <param name="y"> dense vector y </param>
            /// <param name="x"> sparse vector x </param>
            public void GthrD<T1, T2>(long n, T1 y, ref T2 x)
                where T1 : Vector<double>
                where T2 : SPVector<double>
                => IntelMKLNative.cblas_dgthr_64(n, y.DataPtr,
                    x.NzValues.DataPtr, x.NzIndices.DataPtr);

            /// <summary>
            /// gathers a full-storage sparse vector's elements into
            /// compressed form x[i] = y[indx[i]]
            /// </summary>
            /// <typeparam name="T1"> Vector[Complex] </typeparam>
            /// <typeparam name="T2"> SPVector[Complex] </typeparam>
            /// <param name="n"> number of non-zero array elements </param>
            /// <param name="y"> dense vector y </param>
            /// <param name="x"> sparse vector x </param>
            public void GthrZ<T1, T2>(long n, T1 y, ref T2 x)
                where T1 : Vector<Complex>
                where T2 : SPVector<Complex>
                => IntelMKLNative.cblas_zgthr_64(n, y.DataPtr,
                    x.NzValues.DataPtr, x.NzIndices.DataPtr);

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


            // wrapper
            /// <summary>
            /// gathers a full-storage sparse vector's elements into
            /// compressed form, replacing them by zeros
            /// </summary>
            /// <typeparam name="T1"> Vector[double] </typeparam>
            /// <typeparam name="T2"> SPVector[double] </typeparam>
            /// <param name="n"> number of non-zero array elements </param>
            /// <param name="y"> dense vector y </param>
            /// <param name="x"> sparse vector x </param>
            public void GthrzD<T1, T2>(long n, ref T1 y, ref T2 x)
                where T1 : Vector<double>
                where T2 : SPVector<double>
                => IntelMKLNative.cblas_dgthrz_64(n, y.DataPtr,
                    x.NzValues.DataPtr, x.NzIndices.DataPtr);

            /// <summary>
            /// gathers a full-storage sparse vector's elements into
            /// compressed form, replacing them by zeros
            /// </summary>
            /// <typeparam name="T1"> Vector[Complex] </typeparam>
            /// <typeparam name="T2"> SPVector[Complex] </typeparam>
            /// <param name="n"> number of non-zero array elements </param>
            /// <param name="y"> dense vector y </param>
            /// <param name="x"> sparse vector x </param>
            public void GthrzZ<T1, T2>(long n, ref T1 y, ref T2 x)
                where T1 : Vector<Complex>
                where T2 : SPVector<Complex>
                => IntelMKLNative.cblas_zgthrz_64(n, y.DataPtr,
                    x.NzValues.DataPtr, x.NzIndices.DataPtr);

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


            //wrapper
            /// <summary>
            /// converts compressed sparse vector into full-storage form
            /// </summary>
            /// <typeparam name="T1"> SPVector[double] </typeparam>
            /// <typeparam name="T2"> Vector[double] </typeparam>
            /// <param name="n"> number of non-zero array elements </param>
            /// <param name="x"> sparse vector x </param>
            /// <param name="y"> dense vector y </param>
            public void SctrD<T1, T2>(long n, T1 x, ref T2 y)
                where T1 : SPVector<double>
                where T2 : Vector<double>
                => IntelMKLNative.cblas_dsctr_64(n, 
                    x.NzValues.DataPtr, x.NzIndices.DataPtr,
                    y.DataPtr);

            /// <summary>
            /// converts compressed sparse vector into full-storage form
            /// </summary>
            /// <typeparam name="T1"> SPVector[Complex] </typeparam>
            /// <typeparam name="T2"> Vector[Complex] </typeparam>
            /// <param name="n"> number of non-zero array elements </param>
            /// <param name="x"> sparse vector x </param>
            /// <param name="y"> dense vector y </param>
            public void SctrZ<T1, T2>(long n, T1 x, ref T2 y)
                where T1 : SPVector<Complex>
                where T2 : Vector<Complex>
                => IntelMKLNative.cblas_zsctr_64(n,
                    x.NzValues.DataPtr, x.NzIndices.DataPtr,
                    y.DataPtr);

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


            // wrapper
            /// <summary>
            /// computes the QR decomposition for the matrix of a sparse
            /// linear system and calculates the solution A * x = b
            /// </summary>
            /// <param name="operation"> specifies operation op() on sparse matrix a </param>
            /// <param name="a"> sparse matrix handle </param>
            /// <param name="descr"> structure specifying sparse matrix properties </param>
            /// <param name="layout"> describes the storage scheme for the dense matrix </param>
            /// <param name="columns"> number of columns in matrix b </param>
            /// <param name="x"> array with size of at least rows * cols </param>
            /// <param name="ldx"> specifies the leading dimension of matrix x </param>
            /// <param name="b"> array with size of at least rows * cols </param>
            /// <param name="ldb"> specifies the leading dimension of matrix b </param>
            /// <returns> result status </returns>
            public SPARSE_Status QR(SPARSE_Operation operation, IntPtr a,
                SPARSE_MatrixDescr descr, SPARSE_Layout layout,
                long columns, IntPtr x, long ldx,
                [In] IntPtr b, long ldb)
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


            // wrapper
            /// <summary>
            /// factorization step of SPARSE QR solver
            /// </summary>
            /// <param name="a"> sparse matrix handle </param>
            /// <param name="alt_values"> array with alternative values; 
            /// must be the size of the non-zeroes in the initial input matrix </param>
            /// <returns> result status </returns>
            public SPARSE_Status QR_Factorize(IntPtr a, IntPtr alt_values)
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


            // wrapper
            /// <summary>
            /// solving step of SPARSE QR solver
            /// </summary>
            /// <param name="operation"> specifies operation op() on sparse matrix a </param>
            /// <param name="a"> sparse matrix handle </param>
            /// <param name="alt_values"> array with alternative values; </param>
            /// <param name="layout"> describes the storage scheme for the dense matrix </param>
            /// <param name="columns"> number of columns in matrix b </param>
            /// <param name="x"> array with size of at least rows * cols </param>
            /// <param name="ldx"> specifies the leading dimension of matrix x </param>
            /// <param name="b"> array with size of at least rows * cols </param>
            /// <param name="ldb"> specifies the leading dimension of matrix b </param>
            /// <returns> result status </returns>
            public SPARSE_Status QR_Solve(SPARSE_Operation operation, IntPtr a, 
                IntPtr alt_values, SPARSE_Layout layout, 
                long columns, IntPtr x, long ldx, IntPtr b, long ldb)
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


            // wrapper
            /// <summary>
            /// first stage of the solving step of the SPARSE QR solver
            /// </summary>
            /// <param name="operation"> specifies operation op() on sparse matrix a </param>
            /// <param name="a"> sparse matrix handle </param>
            /// <param name="layout"> describes the storage scheme for the dense matrix </param>
            /// <param name="columns"> number of columns in matrix b </param>
            /// <param name="x"> array with size of at least rows * cols </param>
            /// <param name="ldx"> specifies the leading dimension of matrix x </param>
            /// <param name="b"> array with size of at least rows * cols </param>
            /// <param name="ldb"> specifies the leading dimension of matrix b </param>
            /// <returns> result status </returns>
            public SPARSE_Status QR_QMult(SPARSE_Operation operation, IntPtr a, 
                SPARSE_Layout layout, long columns, IntPtr x, long ldx,
                [In] IntPtr b, long ldb)
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


            // wrapper
            /// <summary>
            /// second stage of the solving step of the SPARSE solver
            /// </summary>
            /// <param name="operation"> specifies operation op() on sparse matrix a </param>
            /// <param name="a"> sparse matrix handle </param>
            /// <param name="layout"> describes the storage scheme for the dense matrix </param>
            /// <param name="columns"> number of columns in matrix b </param>
            /// <param name="x"> array with size of at least rows * cols </param>
            /// <param name="ldx"> specifies the leading dimension of matrix x </param>
            /// <param name="b"> array with size of at least rows * cols </param>
            /// <param name="ldb"> specifies the leading dimension of matrix b </param>
            /// <returns> result status </returns>
            public SPARSE_Status QR_RSolve(SPARSE_Operation operation, IntPtr a, 
                SPARSE_Layout layout, long columns, IntPtr x, long ldx,
                [In] IntPtr b, long ldb)
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


            // wrapper
            /// <summary>
            /// creates a handle for a sparse matrix in COO format
            /// </summary>
            /// <typeparam name="T1"> ArrayBase[long] </typeparam>
            /// <typeparam name="T2"> ArrayBase[double] </typeparam>
            /// <param name="a"> sparse matrix handle </param>
            /// <param name="indexing"> zero-based (C-style) or one-based (Fortran-style) </param>
            /// <param name="rows"> number of rows of matrix a </param>
            /// <param name="cols"> number of cols of matrix a </param>
            /// <param name="nnz"> number of non-zero elements of the matrix a </param>
            /// <param name="row_indx"> row indices of the non-zero elements </param>
            /// <param name="col_indx"> column indices of the non-zero elements </param>
            /// <param name="values"> values of the non-zero elements </param>
            /// <returns> result status </returns>
            public SPARSE_Status CreateCOOD<T1, T2>(ref IntPtr a, 
                SPARSE_IndexBase indexing, long rows, long cols, long nnz,
                T1 row_indx, T1 col_indx, T2 values)
                where T1 : DenseArrayBase<long>
                where T2 : DenseArrayBase<double>
                => IntelMKLNative.mkl_sparse_d_create_coo_64(ref a, 
                    indexing, rows, cols, nnz,
                    row_indx.DataPtr, col_indx.DataPtr, values.DataPtr);

            /// <summary>
            /// creates a handle for a sparse matrix in COO format
            /// </summary>
            /// <typeparam name="T1"> ArrayBase[long] </typeparam>
            /// <typeparam name="T2"> ArrayBase[Complex] </typeparam>
            /// <param name="a"> sparse matrix handle </param>
            /// <param name="indexing"> zero-based (C-style) or one-based (Fortran-style) </param>
            /// <param name="rows"> number of rows of matrix a </param>
            /// <param name="cols"> number of cols of matrix a </param>
            /// <param name="nnz"> number of non-zero elements of the matrix a </param>
            /// <param name="row_indx"> row indices of the non-zero elements </param>
            /// <param name="col_indx"> column indices of the non-zero elements </param>
            /// <param name="values"> values of the non-zero elements </param>
            /// <returns> result status </returns>
            public SPARSE_Status CreateCOOZ<T1, T2>(ref IntPtr a,
                SPARSE_IndexBase indexing, long rows, long cols, long nnz,
                T1 row_indx, T1 col_indx, T2 values)
                where T1 : DenseArrayBase<long>
                where T2 : DenseArrayBase<Complex>
                => IntelMKLNative.mkl_sparse_z_create_coo_64(ref a, 
                    indexing, rows, cols, nnz,
                    row_indx.DataPtr, col_indx.DataPtr, values.DataPtr);

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


            // wrapper
            /// <summary>
            /// creates a handle for a sparse matrix in CSR format
            /// </summary>
            /// <typeparam name="T1"> ArrayBase[long] </typeparam>
            /// <typeparam name="T2"> ArrayBase[double] </typeparam>
            /// <param name="a"> sparse matrix handle </param>
            /// <param name="indexing"> zero-based (C-style) or one-based (Fortran-style) </param>
            /// <param name="rows"> number of rows of matrix a </param>
            /// <param name="cols"> number of cols of matrix a </param>
            /// <param name="row_ptr"> row start/end indices of the non-zero elements </param>
            /// <param name="col_indx"> column indices of the non-zero elements </param>
            /// <param name="values"> values of the non-zero elements </param>
            /// <returns> result status </returns>
            public SPARSE_Status CreateCSRD<T1, T2>(ref IntPtr a,
                SPARSE_IndexBase indexing, long rows, long cols,
                T1 row_ptr, T1 col_indx, T2 values)
                where T1 : DenseArrayBase<long>
                where T2 : DenseArrayBase<double>
                => IntelMKLNative.mkl_sparse_d_create_csr_64(ref a, 
                    indexing, rows, cols,
                    row_ptr.DataPtr, col_indx.DataPtr, values.DataPtr);

            /// <summary>
            /// creates a handle for a sparse matrix in CSR format
            /// </summary>
            /// <typeparam name="T1"> ArrayBase[long] </typeparam>
            /// <typeparam name="T2"> ArrayBase[Complex] </typeparam>
            /// <param name="a"> sparse matrix handle </param>
            /// <param name="indexing"> zero-based (C-style) or one-based (Fortran-style) </param>
            /// <param name="rows"> number of rows of matrix a </param>
            /// <param name="cols"> number of cols of matrix a </param>
            /// <param name="row_ptr"> row start/end indices of the non-zero elements </param>
            /// <param name="col_indx"> column indices of the non-zero elements </param>
            /// <param name="values"> values of the non-zero elements </param>
            /// <returns> result status </returns>
            public SPARSE_Status CreateCSRZ<T1, T2>(ref IntPtr a,
                SPARSE_IndexBase indexing, long rows, long cols,
                T1 row_ptr, T1 col_indx, T2 values)
                where T1 : DenseArrayBase<long>
                where T2 : DenseArrayBase<Complex>
                => IntelMKLNative.mkl_sparse_z_create_csr_64(ref a,
                    indexing, rows, cols,
                    row_ptr.DataPtr, col_indx.DataPtr, values.DataPtr);

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


            // wrapper
            /// <summary>
            /// creates a handle for a sparse matrix in CSC format
            /// </summary>
            /// <typeparam name="T1"> ArrayBase[long] </typeparam>
            /// <typeparam name="T2"> ArrayBase[double] </typeparam>
            /// <param name="a"> sparse matrix handle </param>
            /// <param name="indexing"> zero-based (C-style) or one-based (Fortran-style) </param>
            /// <param name="rows"> number of rows of matrix a </param>
            /// <param name="cols"> number of cols of matrix a </param>
            /// <param name="col_ptr"> column start/end indices of the non-zero elements </param>
            /// <param name="row_indx"> row indices of the non-zero elements </param>
            /// <param name="values"> values of the non-zero elements </param>
            /// <returns> result status </returns>
            public SPARSE_Status CreateCSCD<T1, T2>(ref IntPtr a,
                SPARSE_IndexBase indexing, long rows, long cols,
                T1 col_ptr, T1 row_indx, T2 values)
                where T1 : DenseArrayBase<long>
                where T2 : DenseArrayBase<double>
                => IntelMKLNative.mkl_sparse_d_create_csc_64(ref a,
                    indexing, rows, cols,
                    col_ptr.DataPtr, row_indx.DataPtr, values.DataPtr);

            /// <summary>
            /// creates a handle for a sparse matrix in CSC format
            /// </summary>
            /// <typeparam name="T1"> ArrayBase[long] </typeparam>
            /// <typeparam name="T2"> ArrayBase[Complex] </typeparam>
            /// <param name="a"> sparse matrix handle </param>
            /// <param name="indexing"> zero-based (C-style) or one-based (Fortran-style) </param>
            /// <param name="rows"> number of rows of matrix a </param>
            /// <param name="cols"> number of cols of matrix a </param>
            /// <param name="col_ptr"> column start/end indices of the non-zero elements </param>
            /// <param name="row_indx"> row indices of the non-zero elements </param>
            /// <param name="values"> values of the non-zero elements </param>
            /// <returns> result status </returns>
            public SPARSE_Status CreateCSCZ<T1, T2>(ref IntPtr a,
                SPARSE_IndexBase indexing, long rows, long cols,
                T1 col_ptr, T1 row_indx, T2 values)
                where T1 : DenseArrayBase<long>
                where T2 : DenseArrayBase<Complex>
                => IntelMKLNative.mkl_sparse_z_create_csc_64(ref a,
                    indexing, rows, cols,
                    col_ptr.DataPtr, row_indx.DataPtr, values.DataPtr);

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


            // wrapper
            /// <summary>
            /// exports a sparse matrix in CSR format
            /// </summary>
            /// <param name="a"> sparse matrix handle </param>
            /// <param name="indexing"> zero-based (C-style) or one-based (Fortran-style) </param>
            /// <param name="rows"> number of rows of matrix a </param>
            /// <param name="cols"> number of cols of matrix a </param>
            /// <param name="row_start"> row start indices of the non-zero elements </param>
            /// <param name="row_end"> row end indices of the non-zero elements </param>
            /// <param name="col_indx"> column indices of the non-zero elements </param>
            /// <param name="values"> values of the non-zero elements </param>
            /// <returns> result status </returns>
            public SPARSE_Status ExportCSRD(IntPtr a, 
                ref SPARSE_IndexBase indexing, ref long rows, ref long cols, 
                ref IntPtr row_start, ref IntPtr row_end,
                ref IntPtr col_indx, ref IntPtr values)
                => IntelMKLNative.mkl_sparse_d_export_csr_64(a, 
                    ref indexing, ref rows, ref cols,
                    ref row_start, ref row_end,
                    ref col_indx, ref values);

            /// <summary>
            /// exports a sparse matrix in CSR format
            /// </summary>
            /// <param name="a"> sparse matrix handle </param>
            /// <param name="indexing"> zero-based (C-style) or one-based (Fortran-style) </param>
            /// <param name="rows"> number of rows of matrix a </param>
            /// <param name="cols"> number of cols of matrix a </param>
            /// <param name="row_start"> row start indices of the non-zero elements </param>
            /// <param name="row_end"> row end indices of the non-zero elements </param>
            /// <param name="col_indx"> column indices of the non-zero elements </param>
            /// <param name="values"> values of the non-zero elements </param>
            /// <returns> result status </returns>
            public SPARSE_Status ExportCSRZ(IntPtr a,
                ref SPARSE_IndexBase indexing, ref long rows, ref long cols,
                ref IntPtr row_start, ref IntPtr row_end,
                ref IntPtr col_indx, ref IntPtr values)
                => IntelMKLNative.mkl_sparse_z_export_csr_64(a,
                    ref indexing, ref rows, ref cols,
                    ref row_start, ref row_end,
                    ref col_indx, ref values);

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
            public SPARSE_Status SetValueD(IntPtr a, long row, long col, double value)
                => IntelMKLNative.mkl_sparse_d_set_value_64(a, row, col, value);

            /// <summary>
            /// changes a single value of matrix in internal representation
            /// </summary>
            /// <param name="a"> sparse matrix handle </param>
            /// <param name="row"> indicates row of matrix in which to set value </param>
            /// <param name="col"> indicates column of matrix in which to set value </param>
            /// <param name="value"> target value </param>
            /// <returns> result status </returns>
            public SPARSE_Status SetValueZ(IntPtr a, long row, long col, Complex value)
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
            public SPARSE_Status UpdateValuesD(IntPtr a, long nvalues,
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
            public SPARSE_Status UpdateValuesZ(IntPtr a, long nvalues,
                long* indx, long* indy, void* values)
                => IntelMKLNative.mkl_sparse_z_update_values_64(a,
                    nvalues, indx, indy, values);


            // wrapper
            /// <summary>
            /// changes all or selected matrix values in internal representation
            /// </summary>
            /// <typeparam name="T1"> ArrayBase[long] </typeparam>
            /// <typeparam name="T2"> ArrayBase[double] </typeparam>
            /// <param name="a"> sparse matrix handle </param>
            /// <param name="nvalues"> total number of elements changed </param>
            /// <param name="indx"> row indices for the new values </param>
            /// <param name="indy"> column indices for the new values </param>
            /// <param name="values"> new values </param>
            /// <returns> result status </returns>
            public SPARSE_Status UpdateValuesD<T1, T2>(IntPtr a, long nvalues, 
                T1 indx, T1 indy, T2 values)
                where T1 : DenseArrayBase<long>
                where T2 : DenseArrayBase<double>
                => IntelMKLNative.mkl_sparse_d_update_values_64(a, 
                    nvalues, indx.DataPtr, indy.DataPtr, values.DataPtr);

            /// <summary>
            /// changes all or selected matrix values in internal representation
            /// </summary>
            /// <typeparam name="T1"> ArrayBase[long] </typeparam>
            /// <typeparam name="T2"> ArrayBase[Complex] </typeparam>
            /// <param name="a"> sparse matrix handle </param>
            /// <param name="nvalues"> total number of elements changed </param>
            /// <param name="indx"> row indices for the new values </param>
            /// <param name="indy"> column indices for the new values </param>
            /// <param name="values"> new values </param>
            /// <returns> result status </returns>
            public SPARSE_Status UpdateValuesZ<T1, T2>(IntPtr a, long nvalues,
                T1 indx, T1 indy, T2 values)
                where T1 : DenseArrayBase<long>
                where T2 : DenseArrayBase<Complex>
                => IntelMKLNative.mkl_sparse_z_update_values_64(a,
                    nvalues, indx.DataPtr, indy.DataPtr, values.DataPtr);

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


            // wrapper
            /// <summary>
            /// computes a sparse matrix-vector product
            /// y = alpha * op(A) * x + beta * y
            /// </summary>
            /// <typeparam name="T1"> SPMatrix[double] </typeparam>
            /// <typeparam name="T2"> Vector[double] </typeparam>
            /// <param name="operation"> specifies operation op() on sparse matrix a </param>
            /// <param name="alpha"> scalar constant alpha </param>
            /// <param name="a"> sparse matrix a </param>
            /// <param name="descr"> structure specifying sparse matrix properties </param>
            /// <param name="x"> dense vector x </param>
            /// <param name="beta"> scalar constant beta </param>
            /// <param name="y"> dense vector y </param>
            /// <returns> result status </returns>
            public SPARSE_Status MvD<T1, T2>(SPARSE_Operation operation, 
                double alpha, T1 a, SPARSE_MatrixDescr descr, 
                T2 x, double beta, ref T2 y)
                where T1 : SPMatrix<double>
                where T2 : Vector<double>
                => IntelMKLNative.mkl_sparse_d_mv_64(operation, 
                    alpha, a.Handle, descr, x.DataPtr, beta, y.DataPtr);

            /// <summary>
            /// computes a sparse matrix-vector product
            /// y = alpha * op(A) * x + beta * y
            /// </summary>
            /// <typeparam name="T1"> SPMatrix[Complex] </typeparam>
            /// <typeparam name="T2"> Vector[Complex] </typeparam>
            /// <param name="operation"> specifies operation op() on sparse matrix a </param>
            /// <param name="alpha"> scalar constant alpha </param>
            /// <param name="a"> sparse matrix a </param>
            /// <param name="descr"> structure specifying sparse matrix properties </param>
            /// <param name="x"> dense vector x </param>
            /// <param name="beta"> scalar constant beta </param>
            /// <param name="y"> dense vector y </param>
            /// <returns> result status </returns>
            public SPARSE_Status MvZ<T1, T2>(SPARSE_Operation operation,
                Complex alpha, T1 a, SPARSE_MatrixDescr descr,
                T2 x, Complex beta, ref T2 y)
                where T1 : SPMatrix<Complex>
                where T2 : Vector<Complex>
                => IntelMKLNative.mkl_sparse_z_mv_64(operation,
                    ref alpha, a.Handle, descr, x.DataPtr, ref beta, y.DataPtr);

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


            // wrapper
            /// <summary>
            /// computes y = alpha * A * x + beta * y
            /// and d = (x, y) - the l2 inner product
            /// </summary>
            /// <typeparam name="T1"> SPMatrix[double] </typeparam>
            /// <typeparam name="T2"> Vector[double] </typeparam>
            /// <param name="operation"> specifies operation op() on sparse matrix a </param>
            /// <param name="alpha"> scalar constant alpha </param>
            /// <param name="a"> sparse matrix a </param>
            /// <param name="descr"> structure specifying sparse matrix properties </param>
            /// <param name="x"> dense vector x </param>
            /// <param name="beta"> scalar constant beta </param>
            /// <param name="y"> dense vector y </param>
            /// <param name="d"> inner product </param>
            /// <returns> result status </returns>
            public SPARSE_Status DotMvD<T1, T2>(SPARSE_Operation operation,
                double alpha, T1 a, SPARSE_MatrixDescr descr,
                T2 x, double beta, ref T2 y, ref double d)
                where T1 : SPMatrix<double>
                where T2 : Vector<double>
                => IntelMKLNative.mkl_sparse_d_dotmv_64(operation,
                    alpha, a.Handle, descr, x.DataPtr, beta, y.DataPtr, ref d);

            /// <summary>
            /// computes y = alpha * A * x + beta * y
            /// and d = (x, y) - the l2 inner product
            /// </summary>
            /// <typeparam name="T1"> SPMatrix[Complex] </typeparam>
            /// <typeparam name="T2"> Vector[Complex] </typeparam>
            /// <param name="operation"> specifies operation op() on sparse matrix a </param>
            /// <param name="alpha"> scalar constant alpha </param>
            /// <param name="a"> sparse matrix a </param>
            /// <param name="descr"> structure specifying sparse matrix properties </param>
            /// <param name="x"> dense vector x </param>
            /// <param name="beta"> scalar constant beta </param>
            /// <param name="y"> dense vector y </param>
            /// <param name="d"> inner product </param>
            /// <returns> result status </returns>
            public SPARSE_Status DotMvZ<T1, T2>(SPARSE_Operation operation,
                Complex alpha, T1 a, SPARSE_MatrixDescr descr,
                T2 x, Complex beta, T2 y, ref Complex d)
                where T1 : SPMatrix<Complex>
                where T2 : Vector<Complex>
                => IntelMKLNative.mkl_sparse_z_dotmv_64(operation,
                    ref alpha, a.Handle, descr, x.DataPtr, ref beta, y.DataPtr, ref d);

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


            // wrapper
            /// <summary>
            /// solves triangular system
            /// y = alpha * A^{-1} * x 
            /// </summary>
            /// <typeparam name="T1"> SPMatrix[double] </typeparam>
            /// <typeparam name="T2"> Vector[double] </typeparam>
            /// <param name="operation"> specifies operation op() on sparse matrix a </param>
            /// <param name="alpha"> scalar constant alpha </param>
            /// <param name="a"> sparse matrix a </param>
            /// <param name="descr"> structure specifying sparse matrix properties </param>
            /// <param name="x"> dense vector x </param>
            /// <param name="y"> dense vector y </param>
            /// <returns> result status </returns>
            public SPARSE_Status TrsvD<T1, T2>(SPARSE_Operation operation,
                double alpha, T1 a, SPARSE_MatrixDescr descr,
                T2 x, ref T2 y)
                where T1 : SPMatrix<double>
                where T2 : Vector<double>
                => IntelMKLNative.mkl_sparse_d_trsv_64(operation,
                    alpha, a.Handle, descr, x.DataPtr, y.DataPtr);

            /// <summary>
            /// solves triangular system
            /// y = alpha * A^{-1} * x 
            /// </summary>
            /// <typeparam name="T1"> SPMatrix[Complex] </typeparam>
            /// <typeparam name="T2"> Vector[Complex] </typeparam>
            /// <param name="operation"> specifies operation op() on sparse matrix a </param>
            /// <param name="alpha"> scalar constant alpha </param>
            /// <param name="a"> sparse matrix a </param>
            /// <param name="descr"> structure specifying sparse matrix properties </param>
            /// <param name="x"> dense vector x </param>
            /// <param name="y"> dense vector y </param>
            /// <returns> result status </returns>
            public SPARSE_Status TrsvZ<T1, T2>(SPARSE_Operation operation,
                Complex alpha, T1 a, SPARSE_MatrixDescr descr,
                T2 x, ref T2 y)
                where T1 : SPMatrix<Complex>
                where T2 : Vector<Complex>
                => IntelMKLNative.mkl_sparse_z_trsv_64(operation,
                    ref alpha, a.Handle, descr, x.DataPtr, y.DataPtr);

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


            // wrapper
            /// <summary>
            /// applies symmetric Gauss-Seidel preconditioner to
            /// symmetric system A * x = b
            /// </summary>
            /// <typeparam name="T1"> SPMatrix[double] </typeparam>
            /// <typeparam name="T2"> Vector[double] </typeparam>
            /// <param name="operation"> specifies operation op() on sparse matrix a </param>
            /// <param name="a"> sparse matrix a </param>
            /// <param name="descr"> structure specifying sparse matrix properties </param>
            /// <param name="alpha"> scalar constant alpha </param>
            /// <param name="b"> dense vector b </param>
            /// <param name="x"> dense vector x </param>
            /// <returns> result status </returns>
            public SPARSE_Status SymgsD<T1, T2>(SPARSE_Operation operation,
                T1 a, SPARSE_MatrixDescr descr, double alpha, T2 b, ref T2 x)
                where T1 : SPMatrix<double>
                where T2 : Vector<double>
                => IntelMKLNative.mkl_sparse_d_symgs_64(operation,
                    a.Handle, descr, alpha, b.DataPtr, x.DataPtr);

            /// <summary>
            /// applies symmetric Gauss-Seidel preconditioner to
            /// symmetric system A * x = b
            /// </summary>
            /// <typeparam name="T1"> SPMatrix[Complex] </typeparam>
            /// <typeparam name="T2"> Vector[Complex] </typeparam>
            /// <param name="operation"> specifies operation op() on sparse matrix a </param>
            /// <param name="a"> sparse matrix a </param>
            /// <param name="descr"> structure specifying sparse matrix properties </param>
            /// <param name="alpha"> scalar constant alpha </param>
            /// <param name="b"> dense vector b </param>
            /// <param name="x"> dense vector x </param>
            /// <returns> result status </returns>
            public SPARSE_Status SymgsZ<T1, T2>(SPARSE_Operation operation,
                T1 a, SPARSE_MatrixDescr descr, Complex alpha, T2 b, ref T2 x)
                where T1 : SPMatrix<Complex>
                where T2 : Vector<Complex>
                => IntelMKLNative.mkl_sparse_z_symgs_64(operation,
                    a.Handle, descr, ref alpha, b.DataPtr, x.DataPtr);

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


            // wrapper
            /// <summary>
            /// applies symmetric Gauss-Seidel preconditioner to
            /// symmetric system A * x = b, followed by a matr-vector product
            /// and returns y = A * x
            /// </summary>
            /// <typeparam name="T1"> SPMatrix[double] </typeparam>
            /// <typeparam name="T2"> Vector[double] </typeparam>
            /// <param name="operation"> specifies operation op() on sparse matrix a </param>
            /// <param name="a"> sparse matrix a </param>
            /// <param name="descr"> structure specifying sparse matrix properties </param>
            /// <param name="alpha"> scalar constant alpha </param>
            /// <param name="b"> dense vector b </param>
            /// <param name="x"> dense vector x </param>
            /// <param name="y"> dense vector y </param>
            /// <returns> result status </returns>
            public SPARSE_Status SymgsMvD<T1, T2>(SPARSE_Operation operation,
                T1 a, SPARSE_MatrixDescr descr, double alpha, T2 b, 
                ref T2 x, ref T2 y)
                where T1 : SPMatrix<double>
                where T2 : Vector<double>
                => IntelMKLNative.mkl_sparse_d_symgs_mv_64(operation,
                    a.Handle, descr, alpha, b.DataPtr, x.DataPtr, y.DataPtr);

            /// <summary>
            /// applies symmetric Gauss-Seidel preconditioner to
            /// symmetric system A * x = b, followed by a matr-vector product
            /// and returns y = A * x
            /// </summary>
            /// <typeparam name="T1"> SPMatrix[Complex] </typeparam>
            /// <typeparam name="T2"> Vector[Complex] </typeparam>
            /// <param name="operation"> specifies operation op() on sparse matrix a </param>
            /// <param name="a"> sparse matrix a </param>
            /// <param name="descr"> structure specifying sparse matrix properties </param>
            /// <param name="alpha"> scalar constant alpha </param>
            /// <param name="b"> dense vector b </param>
            /// <param name="x"> dense vector x </param>
            /// <param name="y"> dense vector y </param>
            /// <returns> result status </returns>
            public SPARSE_Status SymgsMvZ<T1, T2>(SPARSE_Operation operation,
                T1 a, SPARSE_MatrixDescr descr, Complex alpha, T2 b,
                ref T2 x, ref T2 y)
                where T1 : SPMatrix<Complex>
                where T2 : Vector<Complex>
                => IntelMKLNative.mkl_sparse_z_symgs_mv_64(operation,
                    a.Handle, descr, ref alpha, b.DataPtr, x.DataPtr, y.DataPtr);

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


            // wrapper
            /// <summary>
            /// computes the product of a sparse matrix and a dense
            /// matrix and stores the result as a dense matrix
            /// y = alpha * op(A) * x + beta * y
            /// </summary>
            /// <typeparam name="T1"> SPMatrix[double] </typeparam>
            /// <typeparam name="T2"> Matrix[double] </typeparam>
            /// <param name="operation"> specifies operation op() on sparse matrix a </param>
            /// <param name="alpha"> scalar alpha </param>
            /// <param name="a"> sparse matrix a </param>
            /// <param name="descr"> structure specifying sparse matrix properties </param>
            /// <param name="layout"> describes the storage scheme for the dense matrix </param>
            /// <param name="x"> dense matrix x </param>
            /// <param name="columns"> number of columns of matrix y </param>
            /// <param name="ldx"> leading dimension of matrix x </param>
            /// <param name="beta"> scalar beta </param>
            /// <param name="y"> dense matrix y </param>
            /// <param name="ldy"> leading dimension of matrix y </param>
            /// <returns> result status </returns>
            public SPARSE_Status MmD<T1, T2>(SPARSE_Operation operation,
                double alpha, T1 a, SPARSE_MatrixDescr descr, SPARSE_Layout layout,
                T2 x, long columns, long ldx,
                double beta, ref T2 y, long ldy)
                where T1 : SPMatrix<double>
                where T2 : Matrix<double>
                => IntelMKLNative.mkl_sparse_d_mm_64(operation,
                    alpha, a.Handle, descr, layout, 
                    x.DataPtr, columns, ldx, beta, y.DataPtr, ldy);

            /// <summary>
            /// computes the product of a sparse matrix and a dense
            /// matrix and stores the result as a dense matrix
            /// y = alpha * op(A) * x + beta * y
            /// </summary>
            /// <typeparam name="T1"> SPMatrix[Complex] </typeparam>
            /// <typeparam name="T2"> Matrix[Complex] </typeparam>
            /// <param name="operation"> specifies operation op() on sparse matrix a </param>
            /// <param name="alpha"> scalar alpha </param>
            /// <param name="a"> sparse matrix a </param>
            /// <param name="descr"> structure specifying sparse matrix properties </param>
            /// <param name="layout"> describes the storage scheme for the dense matrix </param>
            /// <param name="x"> dense matrix x </param>
            /// <param name="columns"> number of columns of matrix y </param>
            /// <param name="ldx"> leading dimension of matrix x </param>
            /// <param name="beta"> scalar beta </param>
            /// <param name="y"> dense matrix y </param>
            /// <param name="ldy"> leading dimension of matrix y </param>
            /// <returns> result status </returns>
            public SPARSE_Status MmZ<T1, T2>(SPARSE_Operation operation,
                Complex alpha, T1 a, SPARSE_MatrixDescr descr, SPARSE_Layout layout,
                T2 x, long columns, long ldx,
                Complex beta, ref T2 y, long ldy)
                where T1 : SPMatrix<Complex>
                where T2 : Matrix<Complex>
                => IntelMKLNative.mkl_sparse_z_mm_64(operation,
                    ref alpha, a.Handle, descr, layout,
                    x.DataPtr, columns, ldx, ref beta, y.DataPtr, ldy);

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


            // wrapper
            /// <summary>
            /// solves a system of linear equations with multiple 
            /// right hand sides for a triangular sparse matrix
            /// y = alpha * op(A)^{-1} * x
            /// </summary>
            /// <typeparam name="T1"> SPMatrix[double] </typeparam>
            /// <typeparam name="T2"> Matrix[double] </typeparam>
            /// <param name="operation"> specifies operation op() on sparse matrix a </param>
            /// <param name="alpha"> scalar alpha </param>
            /// <param name="a"> sparse matrix a </param>
            /// <param name="descr"> structure specifying sparse matrix properties </param>
            /// <param name="layout"> describes the storage scheme for the dense matrix </param>
            /// <param name="x"> dense matrix x </param>
            /// <param name="columns"> number of columns of matrix y </param>
            /// <param name="ldx"> leading dimension of matrix x </param>
            /// <param name="y"> dense matrix y </param>
            /// <param name="ldy"> leading dimension of matrix y </param>
            /// <returns> result status </returns>
            public SPARSE_Status TrsmD<T1, T2>(SPARSE_Operation operation,
                double alpha, T1 a, SPARSE_MatrixDescr descr, SPARSE_Layout layout,
                T2 x, long columns, long ldx,
                ref T2 y, long ldy)
                where T1 : SPMatrix<double>
                where T2 : Matrix<double>
                => IntelMKLNative.mkl_sparse_d_trsm_64(operation,
                    alpha, a.Handle, descr, layout,
                    x.DataPtr, columns, ldx,
                    y.DataPtr, ldy);

            /// <summary>
            /// solves a system of linear equations with multiple 
            /// right hand sides for a triangular sparse matrix
            /// y = alpha * op(A)^{-1} * x
            /// </summary>
            /// <typeparam name="T1"> SPMatrix[Complex] </typeparam>
            /// <typeparam name="T2"> Matrix[Complex] </typeparam>
            /// <param name="operation"> specifies operation op() on sparse matrix a </param>
            /// <param name="alpha"> scalar alpha </param>
            /// <param name="a"> sparse matrix a </param>
            /// <param name="descr"> structure specifying sparse matrix properties </param>
            /// <param name="layout"> describes the storage scheme for the dense matrix </param>
            /// <param name="x"> dense matrix x </param>
            /// <param name="columns"> number of columns of matrix y </param>
            /// <param name="ldx"> leading dimension of matrix x </param>
            /// <param name="y"> dense matrix y </param>
            /// <param name="ldy"> leading dimension of matrix y </param>
            /// <returns> result status </returns>
            public SPARSE_Status TrsmZ<T1, T2>(SPARSE_Operation operation,
                Complex alpha, T1 a, SPARSE_MatrixDescr descr, SPARSE_Layout layout,
                T2 x, long columns, long ldx,
                ref T2 y, long ldy)
                where T1 : SPMatrix<Complex>
                where T2 : Matrix<Complex>
                => IntelMKLNative.mkl_sparse_z_trsm_64(operation,
                    ref alpha, a.Handle, descr, layout,
                    x.DataPtr, columns, ldx,
                    y.DataPtr, ldy);

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


            // wrapper
            /// <summary>
            /// computes the sum of two sparse matrices
            /// C = alpha * op(A) + B
            /// the result is stored in a newly allocated sparse matrix
            /// </summary>
            /// <typeparam name="T"> SPMatrix[double] </typeparam>
            /// <param name="operation"> specifies operation op() on sparse matrix a </param>
            /// <param name="a"> sparse matrix a </param>
            /// <param name="alpha"> scalar alpha </param>
            /// <param name="b"> sparse matrix b </param>
            /// <param name="c"> sparse matrix c </param>
            /// <returns> result status </returns>
            public SPARSE_Status AddD<T>(SPARSE_Operation operation,
                T a, double alpha, T b, ref T c)
                where T : SPMatrix<double>
            {
                IntPtr p = c.Handle;
                return IntelMKLNative.mkl_sparse_d_add_64(operation,
                    a.Handle, alpha, b.Handle, ref p);
            }

            /// <summary>
            /// computes the sum of two sparse matrices
            /// C = alpha * op(A) + B
            /// the result is stored in a newly allocated sparse matrix
            /// </summary>
            /// <typeparam name="T"> SPMatrix[Complex] </typeparam>
            /// <param name="operation"> specifies operation op() on sparse matrix a </param>
            /// <param name="a"> sparse matrix a </param>
            /// <param name="alpha"> scalar alpha </param>
            /// <param name="b"> sparse matrix b </param>
            /// <param name="c"> sparse matrix c </param>
            /// <returns> result status </returns>
            public SPARSE_Status AddZ<T>(SPARSE_Operation operation,
                T a, Complex alpha, T b, ref T c)
                where T : SPMatrix<Complex>
            {
                IntPtr p = c.Handle;
                return IntelMKLNative.mkl_sparse_z_add_64(operation,
                    a.Handle, ref alpha, b.Handle, ref p);
            }

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


            // wrapper
            /// <summary>
            /// computes the product of two sparse matrices
            /// C = op(A) * B
            /// the result is stored in a newly allocated sparse matrix
            /// </summary>
            /// <typeparam name="T"> SPMatrix[double] </typeparam>
            /// <param name="operation"> specifies operation op() on sparse matrix a </param>
            /// <param name="a"> sparse matrix a </param>
            /// <param name="b"> sparse matrix b </param>
            /// <param name="c"> sparse matrix c </param>
            /// <returns> result status </returns>
            public SPARSE_Status SpmmD<T>(SPARSE_Operation operation,
                T a, T b, ref T c)
                where T : SPMatrix<double>
            {
                IntPtr p = c.Handle;
                return IntelMKLNative.mkl_sparse_spmm_64(operation,
                    a.Handle, b.Handle, ref p);
            }

            /// <summary>
            /// computes the product of two sparse matrices
            /// C = op(A) * B
            /// the result is stored in a newly allocated sparse matrix
            /// </summary>
            /// <typeparam name="T"> SPMatrix[Complex] </typeparam>
            /// <param name="operation"> specifies operation op() on sparse matrix a </param>
            /// <param name="a"> sparse matrix a </param>
            /// <param name="b"> sparse matrix b </param>
            /// <param name="c"> sparse matrix c </param>
            /// <returns> result status </returns>
            public SPARSE_Status SpmmZ<T>(SPARSE_Operation operation,
                T a, T b, ref T c)
                where T : SPMatrix<Complex>
            {
                IntPtr p = c.Handle;
                return IntelMKLNative.mkl_sparse_spmm_64(operation,
                    a.Handle, b.Handle, ref p);
            }

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


            // wrapper
            /// <summary>
            /// computes the product of two sparse matrices
            /// C = opA(A) * opB(B)
            /// the result is stored in a newly allocated sparse matrix
            /// </summary>
            /// <typeparam name="T"> SPMatrix[double] </typeparam>
            /// <param name="transA"> specifies operation op() on sparse matrix a </param>
            /// <param name="descrA"> structure specifying sparse matrix a's properties</param>
            /// <param name="a"> sparse matrix a </param>
            /// <param name="transB"> specifies operation op() on sparse matrix b </param>
            /// <param name="descrB"> structure specifying sparse matrix b's properties </param>
            /// <param name="b"> sparse matrix b </param>
            /// <param name="request"> specifies whether the full computations are performed at once 
            /// or using the two-stage algorithm </param>
            /// <param name="c"> sparse matrix c </param>
            /// <returns> result status </returns>
            public SPARSE_Status Sp2mD<T>(
                SPARSE_Operation transA, SPARSE_MatrixDescr descrA, T a, 
                SPARSE_Operation transB, SPARSE_MatrixDescr descrB, T b, 
                SPARSE_Request request, ref T c)
                where T : SPMatrix<double>
            {
                IntPtr p = c.Handle;
                return IntelMKLNative.mkl_sparse_sp2m_64(transA, descrA, a.Handle, 
                    transB, descrB, b.Handle, 
                    request, ref p);
            }

            /// <summary>
            /// computes the product of two sparse matrices
            /// C = opA(A) * opB(B)
            /// the result is stored in a newly allocated sparse matrix
            /// </summary>
            /// <typeparam name="T"> SPMatrix[Complex] </typeparam>
            /// <param name="transA"> specifies operation op() on sparse matrix a </param>
            /// <param name="descrA"> structure specifying sparse matrix a's properties</param>
            /// <param name="a"> sparse matrix a </param>
            /// <param name="transB"> specifies operation op() on sparse matrix b </param>
            /// <param name="descrB"> structure specifying sparse matrix b's properties </param>
            /// <param name="b"> sparse matrix b </param>
            /// <param name="request"> specifies whether the full computations are performed at once 
            /// or using the two-stage algorithm </param>
            /// <param name="c"> sparse matrix c </param>
            /// <returns> result status </returns>
            public SPARSE_Status Sp2mZ<T>(
                SPARSE_Operation transA, SPARSE_MatrixDescr descrA, T a,
                SPARSE_Operation transB, SPARSE_MatrixDescr descrB, T b,
                SPARSE_Request request, ref T c)
                where T : SPMatrix<Complex>
            {
                IntPtr p = c.Handle;
                return IntelMKLNative.mkl_sparse_sp2m_64(transA, descrA, a.Handle,
                    transB, descrB, b.Handle,
                    request, ref p);
            }

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


            // wrapper
            /// <summary>
            /// computes the product of sparse matrix with its transpose 
            /// C = op(A) * op(A)^T 
            /// and stores the result in a newly allocated sparse matrix
            /// </summary>
            /// <typeparam name="T"> SPMatrix[double] </typeparam>
            /// <param name="operation"> specifies operation op() on sparse matrix a </param>
            /// <param name="a"> sparse matrix a </param>
            /// <param name="c"> sparse matrix c </param>
            /// <returns> result status </returns>
            public SPARSE_Status SyrkD<T>(SPARSE_Operation operation,
                T a, ref T c)
                where T : SPMatrix<double>
            {
                IntPtr p = c.Handle;
                return IntelMKLNative.mkl_sparse_syrk_64(operation,
                    a.Handle, ref p);
            }

            /// <summary>
            /// computes the product of sparse matrix with its conjugate transpose 
            /// C = op(A) * op(A)^H
            /// and stores the result in a newly allocated sparse matrix
            /// </summary>
            /// <typeparam name="T"> SPMatrix[Complex] </typeparam>
            /// <param name="operation"> specifies operation op() on sparse matrix a </param>
            /// <param name="a"> sparse matrix a </param>
            /// <param name="c"> sparse matrix c </param>
            /// <returns> result status </returns>
            public SPARSE_Status SyrkZ<T>(SPARSE_Operation operation,
                T a, ref T c)
                where T : SPMatrix<Complex>
            {
                IntPtr p = c.Handle;
                return IntelMKLNative.mkl_sparse_syrk_64(operation,
                    a.Handle, ref p);
            }

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


            // wrapper
            /// <summary>
            /// computes the symmetric product of three sparse
            /// C = op(A) * B * op(A)^T
            /// and stores the result in a newly allocated sparse matrix
            /// </summary>
            /// <typeparam name="T"> SPMatrix[double] </typeparam>
            /// <param name="transA"> specifies operation op() on sparse matrix a </param>
            /// <param name="a"> sparse matrix a </param>
            /// <param name="b"> sparse matrix b </param>
            /// <param name="descrB"> structure specifying sparse matrix b's properties </param>
            /// <param name="c"> sparse matrix c </param>
            /// <param name="request"> specifies whether the full computations are performed at once 
            /// or using the two-stage algorithm </param>
            /// <returns> result status </returns>
            public SPARSE_Status SyprD<T>(
                SPARSE_Operation transA, T a, T b, SPARSE_MatrixDescr descrB, 
                ref T c, SPARSE_Request request)
                where T : SPMatrix<double>
            {
                IntPtr p = c.Handle;
                return IntelMKLNative.mkl_sparse_sypr_64(transA, a.Handle,
                    b.Handle, descrB, ref p, request);
            }

            /// <summary>
            /// computes the symmetric product of three sparse
            /// C = op(A) * B * op(A)^H
            /// and stores the result in a newly allocated sparse matrix
            /// </summary>
            /// <typeparam name="T"> SPMatrix[Complex] </typeparam>
            /// <param name="transA"> specifies operation op() on sparse matrix a </param>
            /// <param name="a"> sparse matrix a </param>
            /// <param name="b"> sparse matrix b </param>
            /// <param name="descrB"> structure specifying sparse matrix b's properties </param>
            /// <param name="c"> sparse matrix c </param>
            /// <param name="request"> specifies whether the full computations are performed at once 
            /// or using the two-stage algorithm </param>
            /// <returns> result status </returns>
            public SPARSE_Status Sypr<T>(
                SPARSE_Operation transA, T a, T b, SPARSE_MatrixDescr descrB,
                ref T c, SPARSE_Request request)
                where T : SPMatrix<Complex>
            {
                IntPtr p = c.Handle;
                return IntelMKLNative.mkl_sparse_sypr_64(transA, a.Handle,
                    b.Handle, descrB, ref p, request);
            }

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


            // wrapper
            /// <summary>
            /// computes the product of two sparse matrices
            /// C = op(A) * B
            /// and stores the result as a dense matrix
            /// </summary>
            /// <typeparam name="T1"> SPMatrix[double] </typeparam>
            /// <typeparam name="T2"> Matrix[double] </typeparam>
            /// <param name="operation"> specifies operation op() on sparse matrix a </param>
            /// <param name="a"> sparse matrix a </param>
            /// <param name="b"> sparse matrix b </param>
            /// <param name="layout"> describes the storage scheme for the dense matrix </param>
            /// <param name="c"> dense matrix c </param>
            /// <param name="ldc"> leading dimension of matrix c </param>
            /// <returns> result status </returns>
            public SPARSE_Status SpmmdD<T1, T2>(SPARSE_Operation operation,
                T1 a, T1 b, SPARSE_Layout layout,
                T2 c, long ldc)
                where T1 : SPMatrix<double>
                where T2 : Matrix<double>
                => IntelMKLNative.mkl_sparse_d_spmmd_64(operation,
                    a.Handle, b.Handle, layout, c.DataPtr, ldc);

            /// <summary>
            /// computes the product of two sparse matrices
            /// C = op(A) * B
            /// and stores the result as a dense matrix
            /// </summary>
            /// <typeparam name="T1"> SPMatrix[Complex] </typeparam>
            /// <typeparam name="T2"> Matrix[Complex] </typeparam>
            /// <param name="operation"> specifies operation op() on sparse matrix a </param>
            /// <param name="a"> sparse matrix a </param>
            /// <param name="b"> sparse matrix b </param>
            /// <param name="layout"> describes the storage scheme for the dense matrix </param>
            /// <param name="c"> dense matrix c </param>
            /// <param name="ldc"> leading dimension of matrix c </param>
            /// <returns> result status </returns>
            public SPARSE_Status SpmmdZ<T1, T2>(SPARSE_Operation operation,
                T1 a, T1 b, SPARSE_Layout layout,
                T2 c, long ldc)
                where T1 : SPMatrix<Complex>
                where T2 : Matrix<Complex>
                => IntelMKLNative.mkl_sparse_z_spmmd_64(operation,
                    a.Handle, b.Handle, layout, c.DataPtr, ldc);

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


            // wrapper
            /// <summary>
            /// computes the product of two sparse matrices
            /// C = alpha * opA(A) * opB(B) + beta * C
            /// and stores the result as a dense matrix
            /// </summary>
            /// <typeparam name="T1"> SPMatrix[double] </typeparam>
            /// <typeparam name="T2"> Matrix[double] </typeparam>
            /// <param name="transA"> specifies operation op() on sparse matrix a </param>
            /// <param name="descrA"> structure specifying sparse matrix a's properties</param>
            /// <param name="a"> sparse matrix a </param>
            /// <param name="transB"> specifies operation op() on sparse matrix b </param>
            /// <param name="descrB"> structure specifying sparse matrix b's properties </param>
            /// <param name="b"> sparse matrix b </param>
            /// <param name="alpha"> scalar constant alpha </param>
            /// <param name="beta"> scalar constant beta </param>
            /// <param name="c"> dense matrix c </param>
            /// <param name="layout"> cescribes the storage scheme for the dense matrix </param>
            /// <param name="ldc"> leading dimension of matrix c </param>
            /// <returns> result status </returns>
            public SPARSE_Status Sp2mdD<T1, T2>(
                SPARSE_Operation transA, SPARSE_MatrixDescr descrA, T1 a,
                SPARSE_Operation transB, SPARSE_MatrixDescr descrB, T1 b,
                double alpha, double beta, T2 c, SPARSE_Layout layout, long ldc)
                where T1 : SPMatrix<double>
                where T2 : Matrix<double>
                => IntelMKLNative.mkl_sparse_d_sp2md_64(
                    transA, descrA, a.Handle,
                    transB, descrB, b.Handle, 
                    alpha, beta, c.DataPtr, layout, ldc);

            /// <summary>
            /// computes the product of two sparse matrices
            /// C = alpha * opA(A) * opB(B) + beta * C
            /// and stores the result as a dense matrix
            /// </summary>
            /// <typeparam name="T1"> SPMatrix[Complex] </typeparam>
            /// <typeparam name="T2"> Matrix[Complex] </typeparam>
            /// <param name="transA"> specifies operation op() on sparse matrix a </param>
            /// <param name="descrA"> structure specifying sparse matrix a's properties</param>
            /// <param name="a"> sparse matrix a </param>
            /// <param name="transB"> specifies operation op() on sparse matrix b </param>
            /// <param name="descrB"> structure specifying sparse matrix b's properties </param>
            /// <param name="b"> sparse matrix b </param>
            /// <param name="alpha"> scalar constant alpha </param>
            /// <param name="beta"> scalar constant beta </param>
            /// <param name="c"> dense matrix c </param>
            /// <param name="layout"> cescribes the storage scheme for the dense matrix </param>
            /// <param name="ldc"> leading dimension of matrix c </param>
            /// <returns> result status </returns>
            public SPARSE_Status Sp2mdZ<T1, T2>(
                SPARSE_Operation transA, SPARSE_MatrixDescr descrA, T1 a,
                SPARSE_Operation transB, SPARSE_MatrixDescr descrB, T1 b,
                Complex alpha, Complex beta, T2 c, SPARSE_Layout layout, long ldc)
                where T1 : SPMatrix<Complex>
                where T2 : Matrix<Complex>
                => IntelMKLNative.mkl_sparse_z_sp2md_64(
                    transA, descrA, a.Handle,
                    transB, descrB, b.Handle,
                    ref alpha, ref beta, c.DataPtr, layout, ldc);

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


            // wrapper
            /// <summary>
            /// computes the product of sparse matrix with its transpose 
            /// C = beta * C + alpha * op(A) * op(A)^T 
            /// and stores the result as a dense matrix
            /// </summary>
            /// <typeparam name="T1"> SPMatrix[double] </typeparam>
            /// <typeparam name="T2"> Matrix[double] </typeparam>
            /// <param name="operation"> specifies operation op() on sparse matrix a </param>
            /// <param name="a"> sparse matrix a </param>
            /// <param name="alpha"> scalar constant alpha </param>
            /// <param name="beta"> scalar constant beta </param>
            /// <param name="c"> dense matrix c </param>
            /// <param name="layout"> describes the storage scheme for the dense matrix </param>
            /// <param name="ldc"> leading dimension of matrix c </param>
            /// <returns> result status </returns>
            public SPARSE_Status SyrkdD<T1, T2>(SPARSE_Operation operation,
                T1 a, double alpha, double beta, 
                T2 c, SPARSE_Layout layout, long ldc)
                where T1 : SPMatrix<double>
                where T2 : Matrix<double>
                => IntelMKLNative.mkl_sparse_d_syrkd_64(operation,
                    a.Handle, alpha, beta, c.DataPtr, layout, ldc);

            /// <summary>
            /// computes the product of sparse matrix with its conjugate transpose 
            /// C = beta * C + alpha * op(A) * op(A)^H 
            /// and stores the result as a dense matrix
            /// </summary>
            /// <typeparam name="T1"> SPMatrix[Complex] </typeparam>
            /// <typeparam name="T2"> Matrix[Complex] </typeparam>
            /// <param name="operation"> specifies operation op() on sparse matrix a </param>
            /// <param name="a"> sparse matrix a </param>
            /// <param name="alpha"> scalar constant alpha </param>
            /// <param name="beta"> scalar constant beta </param>
            /// <param name="c"> dense matrix c </param>
            /// <param name="layout"> describes the storage scheme for the dense matrix </param>
            /// <param name="ldc"> leading dimension of matrix c </param>
            /// <returns> result status </returns>
            public SPARSE_Status SyrkdZ<T1, T2>(SPARSE_Operation operation,
                T1 a, Complex alpha, Complex beta,
                T2 c, SPARSE_Layout layout, long ldc)
                where T1 : SPMatrix<Complex>
                where T2 : Matrix<Complex>
                => IntelMKLNative.mkl_sparse_z_syrkd_64(operation,
                    a.Handle, ref alpha, ref beta, c.DataPtr, layout, ldc);

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


            // wrapper
            /// <summary>
            /// computes the symmetric triple product of 
            /// a sparse matrix and a dense matrix
            /// C = alpha * op(A) * B * op(A)^T + beta * C
            /// and stores the result as a dense matrix
            /// </summary>
            /// <typeparam name="T1"> SPMatrix[double] </typeparam>
            /// <typeparam name="T2"> Matrix[double] </typeparam>
            /// <param name="operation"> specifies operation op() on sparse matrix a </param>
            /// <param name="a"> sparse matrix a </param>
            /// <param name="b"> dense matrix b </param>
            /// <param name="layoutB"> describes the storage scheme for the dense matrix b </param>
            /// <param name="ldb"> leading dimension of matrix b </param>
            /// <param name="alpha"> scalar constant alpha </param>
            /// <param name="beta"> scalar constant beta </param>
            /// <param name="c"> dense matrix c </param>
            /// <param name="layoutC"> describes the storage scheme for the dense matrix c </param>
            /// <param name="ldc"> leading dimension of matrix c </param>
            /// <returns> result status </returns>
            public SPARSE_Status SyprdD<T1, T2>(SPARSE_Operation operation, 
                T1 a, T2 b, SPARSE_Layout layoutB, long ldb,
                double alpha, double beta, 
                T2 c, SPARSE_Layout layoutC, long ldc)
                where T1 : SPMatrix<double>
                where T2 : Matrix<double>
                => IntelMKLNative.mkl_sparse_d_syprd_64(operation,
                    a.Handle, b.DataPtr, layoutB, ldb, alpha, beta,
                    c.DataPtr, layoutC, ldc);

            /// <summary>
            /// computes the symmetric triple product of 
            /// a sparse matrix and a dense matrix
            /// C = alpha * op(A) * B * op(A)^H + beta * C
            /// and stores the result as a dense matrix
            /// </summary>
            /// <typeparam name="T1"> SPMatrix[Complex] </typeparam>
            /// <typeparam name="T2"> Matrix[Complex] </typeparam>
            /// <param name="operation"> specifies operation op() on sparse matrix a </param>
            /// <param name="a"> sparse matrix a </param>
            /// <param name="b"> dense matrix b </param>
            /// <param name="layoutB"> describes the storage scheme for the dense matrix b </param>
            /// <param name="ldb"> leading dimension of matrix b </param>
            /// <param name="alpha"> scalar constant alpha </param>
            /// <param name="beta"> scalar constant beta </param>
            /// <param name="c"> dense matrix c </param>
            /// <param name="layoutC"> describes the storage scheme for the dense matrix c </param>
            /// <param name="ldc"> leading dimension of matrix c </param>
            /// <returns> result status </returns>
            public SPARSE_Status SyprdZ<T1, T2>(SPARSE_Operation operation,
                T1 a, T2 b, SPARSE_Layout layoutB, long ldb,
                Complex alpha, Complex beta,
                T2 c, SPARSE_Layout layoutC, long ldc)
                where T1 : SPMatrix<Complex>
                where T2 : Matrix<Complex>
                => IntelMKLNative.mkl_sparse_z_syprd_64(operation,
                    a.Handle, b.DataPtr, layoutB, ldb, ref alpha, ref beta,
                    c.DataPtr, layoutC, ldc);

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


            // wrapper  
            /// <summary>
            /// computes forward, backward sweeps or a symmetric
            /// successive over-relaxation preconditioner operation
            /// </summary>
            /// <typeparam name="T1"> SPMatrix[double] </typeparam>
            /// <typeparam name="T2"> Matrix[double] </typeparam>
            /// <param name="type"> specifies the operation performed by the SORV preconditioner </param>
            /// <param name="descrA"> structure specifying sparse matrix properties </param>
            /// <param name="a"> sparse matrix a </param>
            /// <param name="omega"> relaxation factor </param>
            /// <param name="alpha"> parameter that could be used to normalize or set to zero the vector x that holds the initial guess </param>
            /// <param name="x"> initial guess on input (dense) vector x </param>
            /// <param name="b"> righr-hand side (dense) vector b </param>
            /// <returns> result status </returns>
            public SPARSE_Status SorvD<T1, T2>(SPARSE_SorType type,
                SPARSE_MatrixDescr descrA, T1 a, double omega, double alpha,                 
                T2 x, T2 b)
                where T1 : SPMatrix<double>
                where T2 : Matrix<double>
                => IntelMKLNative.mkl_sparse_d_sorv_64(type, descrA,
                    a.Handle, omega, alpha, x.DataPtr, b.DataPtr);

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


            // wrappper
            /// <summary>
            /// computes absolute value of array elements
            /// </summary>
            /// <typeparam name="T"> ArrayBase[double] </typeparam>
            /// <param name="n"> number of elements </param>
            /// <param name="a"> input array a </param>
            /// <param name="y"> result array y </param>
            public void AbsD<T>(long n, T a, ref T y)
                where T : DenseArrayBase<double>
                => IntelMKLNative.vdAbs_64(n, a.DataPtr, y.DataPtr);

            /// <summary>
            /// computes absolute value of array elements
            /// </summary>
            /// <typeparam name="T1"> ArrayBase[Complex] </typeparam>
            /// <typeparam name="T2"> ArrayBase[double] </typeparam>
            /// <param name="n"> number of elements </param>
            /// <param name="a"> input array a </param>
            /// <param name="y"> result array y </param>
            public void AbsZ<T1, T2>(long n, T1 a, ref T2 y)
                where T1 : DenseArrayBase<Complex>
                where T2 : DenseArrayBase<double>
                => IntelMKLNative.vzAbs_64(n, a.DataPtr, y.DataPtr);

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


            // wrapper
            /// <summary>
            /// computes argument of a complex array's elements
            /// </summary>
            /// <typeparam name="T1"> ArrayBase[Complex] </typeparam>
            /// <typeparam name="T2"> ArrayBase[double] </typeparam>
            /// <param name="n"> number of elements </param>
            /// <param name="a"> input array a </param>
            /// <param name="y"> result array y </param>
            public void ArgZ<T1, T2>(long n, T1 a, ref T2 y)
                where T1 : DenseArrayBase<Complex>
                where T2 : DenseArrayBase<double>
                => IntelMKLNative.vzArg_64(n, a.DataPtr, y.DataPtr);

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


            // wrapper
            /// <summary>
            /// performs element by element addition of array a and array b
            /// y = a + b
            /// </summary>
            /// <typeparam name="T"> ArrayBase[double] </typeparam>
            /// <param name="n"> number of elements </param>
            /// <param name="a"> input array a </param>
            /// <param name="b"> input array b </param>
            /// <param name="y"> result array y </param>
            public void AddD<T>(long n, T a, T b, ref T y)
                where T : DenseArrayBase<double>
                => IntelMKLNative.vdAdd_64(n, a.DataPtr, b.DataPtr, y.DataPtr);

            /// <summary>
            /// performs element by element addition of array a and array b
            /// y = a + b
            /// </summary>
            /// <typeparam name="T"> ArrayBase[Complex] </typeparam>
            /// <param name="n"> number of elements </param>
            /// <param name="a"> input array a </param>
            /// <param name="b"> input array b </param>
            /// <param name="y"> result array y </param>
            public void AddZ<T>(long n, T a, T b, ref T y)
                where T : DenseArrayBase<Complex>
                => IntelMKLNative.vzAdd_64(n, a.DataPtr, b.DataPtr, y.DataPtr);

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


            // wrapper
            /// <summary>
            /// performs element by element substraction of array b from array a
            /// y = a - b
            /// </summary>
            /// <typeparam name="T"> ArrayBase[double] </typeparam>
            /// <param name="n"> number of elements </param>
            /// <param name="a"> input array a </param>
            /// <param name="b"> input array b </param>
            /// <param name="y"> result array y </param>
            public void SubD<T>(long n, T a, T b, ref T y)
                where T : DenseArrayBase<double>
                => IntelMKLNative.vdSub_64(n, a.DataPtr, b.DataPtr, y.DataPtr);

            /// <summary>
            /// performs element by element substraction of array b from array a
            /// y = a - b
            /// </summary>
            /// <typeparam name="T"> ArrayBase[Complex] </typeparam>
            /// <param name="n"> number of elements </param>
            /// <param name="a"> input array a </param>
            /// <param name="b"> input array b </param>
            /// <param name="y"> result array y </param>
            public void SubZ<T>(long n, T a, T b, ref T y)
                where T : DenseArrayBase<Complex>
                => IntelMKLNative.vzSub_64(n, a.DataPtr, b.DataPtr, y.DataPtr);


            internal unsafe void Sub(long n, DenseArrayBase<double> a, DenseArrayBase<double> b, 
                 DenseArrayBase<double> y, long starta = 0, long startb = 0, long starty = 0)
            {
                double* pa = (double*)a.DataPtr.ToPointer() + starta;
                double* pb = (double*)b.DataPtr.ToPointer() + startb;
                double* py = (double*)y.DataPtr.ToPointer() + starty;
                IntelMKLNative.vdSub_64(n, pa, pb, py);
            }

            internal unsafe void Sub(long n, DenseArrayBase<Complex> a, DenseArrayBase<Complex> b,
                DenseArrayBase<Complex> y, long starta = 0, long startb = 0, long starty = 0)
            {
                Complex* pa = (Complex*)a.DataPtr.ToPointer() + starta;
                Complex* pb = (Complex*)b.DataPtr.ToPointer() + startb;
                Complex* py = (Complex*)y.DataPtr.ToPointer() + starty;
                IntelMKLNative.vzSub_64(n, pa, pb, py);
            }

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


            // wrapper
            /// <summary>
            /// performs element by element inversion of the array
            /// y[i] = 1.0 / a[i]
            /// </summary>
            /// <typeparam name="T"> ArrayBase[double] </typeparam>
            /// <param name="n"> number of elements </param>
            /// <param name="a"> input array a </param>
            /// <param name="y"> result array y </param>
            public void InvD<T>(long n, T a, ref T y)
                where T : DenseArrayBase<double>
                => IntelMKLNative.vdInv_64(n, a.DataPtr, y.DataPtr);

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


            // wrapper
            /// <summary>
            /// computes a square root of array elements
            /// </summary>
            /// <typeparam name="T"> ArrayBase[double] </typeparam>
            /// <param name="n"> number of elements </param>
            /// <param name="a"> input array a </param>
            /// <param name="y"> result array y </param>
            public void SqrtD<T>(long n, T a, ref T y)
                where T : DenseArrayBase<double>
                => IntelMKLNative.vdSqrt_64(n, a.DataPtr, y.DataPtr);

            /// <summary>
            /// computes a square root of array elements
            /// </summary>
            /// <typeparam name="T"> ArrayBase[Complex] </typeparam>
            /// <param name="n"> number of elements </param>
            /// <param name="a"> input array a </param>
            /// <param name="y"> result array y </param>
            public void SqrtZ<T>(long n, T a, ref T y)
                where T : DenseArrayBase<Complex>
                => IntelMKLNative.vzSqrt_64(n, a.DataPtr, y.DataPtr);

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


            // wrapper
            /// <summary>
            /// computes an inverse square root of array elements
            /// </summary>
            /// <typeparam name="T"> ArrayBase[double] </typeparam>
            /// <param name="n"> number of elements </param>
            /// <param name="a"> input array a </param>
            /// <param name="y"> result array y </param>
            public void InvSqrtD<T>(long n, T a, ref T y)
                where T : DenseArrayBase<double>
                => IntelMKLNative.vdInvSqrt_64(n, a.DataPtr, y.DataPtr);

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


            // wrapper
            /// <summary>
            /// computes a cube root of array elements
            /// </summary>
            /// <typeparam name="T"> ArrayBase[double] </typeparam>
            /// <param name="n"> number of elements </param>
            /// <param name="a"> input array a </param>
            /// <param name="y"> result array y </param>
            public void CbrtD<T>(long n, T a, ref T y)
                where T : DenseArrayBase<double>
                => IntelMKLNative.vdCbrt_64(n, a.DataPtr, y.DataPtr);

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


            // wrapper
            /// <summary>
            /// computes an inverse cube root of array elements
            /// </summary>
            /// <typeparam name="T"> ArrayBase[double] </typeparam>
            /// <param name="n"> number of elements </param>
            /// <param name="a"> input array a </param>
            /// <param name="y"> result array y </param>
            public void InvCbrtD<T>(long n, T a, ref T y)
                where T : DenseArrayBase<double>
                => IntelMKLNative.vdInvCbrt_64(n, a.DataPtr, y.DataPtr);

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


            // wrapper
            /// <summary>
            /// performs element by element squaring of the array
            /// </summary>
            /// <typeparam name="T"> ArrayBase[double] </typeparam>
            /// <param name="n"> number of elements </param>
            /// <param name="a"> input array a </param>
            /// <param name="y"> result array y </param>
            public void SqrD<T>(long n, T a, ref T y)
                where T : DenseArrayBase<double>
                => IntelMKLNative.vdSqr_64(n, a.DataPtr, y.DataPtr);

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


            // wrapper
            /// <summary>
            /// computes an exponential of array elements
            /// </summary>
            /// <typeparam name="T"> ArrayBase[double] </typeparam>
            /// <param name="n"> number of elements </param>
            /// <param name="a"> input array a </param>
            /// <param name="y"> result array y </param>
            public void ExpD<T>(long n, T a, ref T y)
                where T : DenseArrayBase<double>
                => IntelMKLNative.vdExp_64(n, a.DataPtr, y.DataPtr);

            /// <summary>
            /// computes an exponential of array elements
            /// </summary>
            /// <typeparam name="T"> ArrayBase[Complex] </typeparam>
            /// <param name="n"> number of elements </param>
            /// <param name="a"> input array a </param>
            /// <param name="y"> result array y </param>
            public void ExpZ<T>(long n, T a, ref T y)
                where T : DenseArrayBase<Complex>
                => IntelMKLNative.vzExp_64(n, a.DataPtr, y.DataPtr);

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


            // wrapper
            /// <summary>
            /// computes natural logarithm of array elements
            /// </summary>
            /// <typeparam name="T"> ArrayBase[double] </typeparam>
            /// <param name="n"> number of elements </param>
            /// <param name="a"> input array a </param>
            /// <param name="y"> result array y </param>
            public void LnD<T>(long n, T a, ref T y)
                where T : DenseArrayBase<double>
                => IntelMKLNative.vdLn_64(n, a.DataPtr, y.DataPtr);

            /// <summary>
            /// computes natural logarithm of array elements
            /// </summary>
            /// <typeparam name="T"> ArrayBase[Complex] </typeparam>
            /// <param name="n"> number of elements </param>
            /// <param name="a"> input array a </param>
            /// <param name="y"> result array y </param>
            public void LnZ<T>(long n, T a, ref T y)
                where T : DenseArrayBase<Complex>
                => IntelMKLNative.vzLn_64(n, a.DataPtr, y.DataPtr);

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


            // wrapper
            /// <summary>
            /// computes the base 10 logarithm of array elements
            /// </summary>
            /// <typeparam name="T"> ArrayBase[double] </typeparam>
            /// <param name="n"> number of elements </param>
            /// <param name="a"> input array a </param>
            /// <param name="y"> result array y </param>
            public void Log10D<T>(long n, T a, ref T y)
                where T : DenseArrayBase<double>
                => IntelMKLNative.vdLog10_64(n, a.DataPtr, y.DataPtr);

            /// <summary>
            /// computes the base 10 logarithm of array elements
            /// </summary>
            /// <typeparam name="T"> ArrayBase[double] </typeparam>
            /// <param name="n"> number of elements </param>
            /// <param name="a"> input array a </param>
            /// <param name="y"> result array y </param>
            public void Log10Z<T>(long n, T a, ref T y)
                where T : DenseArrayBase<Complex>
                => IntelMKLNative.vzLog10_64(n, a.DataPtr, y.DataPtr);

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


            // wrapper
            /// <summary>
            /// computes cosine of array elements
            /// </summary>
            /// <typeparam name="T"> ArrayBase[double] </typeparam>
            /// <param name="n"> number of elements </param>
            /// <param name="a"> input array a </param>
            /// <param name="y"> result array y </param>
            public void CosD<T>(long n, T a, ref T y)
                where T : DenseArrayBase<double>
                => IntelMKLNative.vdCos_64(n, a.DataPtr, y.DataPtr);

            /// <summary>
            /// computes cosine of array elements
            /// </summary>
            /// <typeparam name="T"> ArrayBase[Complex] </typeparam>
            /// <param name="n"> number of elements </param>
            /// <param name="a"> input array a </param>
            /// <param name="y"> result array y </param>
            public void CosZ<T>(long n, T a, ref T y)
                where T : DenseArrayBase<Complex>
                => IntelMKLNative.vzCos_64(n, a.DataPtr, y.DataPtr);

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


            // wrapper
            /// <summary>
            /// computes sine of array elements
            /// </summary>
            /// <typeparam name="T"> ArrayBase[double] </typeparam>
            /// <param name="n"> number of elements </param>
            /// <param name="a"> input array a </param>
            /// <param name="y"> result array y </param>
            public void SinD<T>(long n, T a, ref T y)
                where T : DenseArrayBase<double>
                => IntelMKLNative.vdSin_64(n, a.DataPtr, y.DataPtr);

            /// <summary>
            /// computes sine of array elements
            /// </summary>
            /// <typeparam name="T"> ArrayBase[Complex] </typeparam>
            /// <param name="n"> number of elements </param>
            /// <param name="a"> input array a </param>
            /// <param name="y"> result array y </param>
            public void SinZ<T>(long n, T a, ref T y)
                where T : DenseArrayBase<Complex>
                => IntelMKLNative.vzSin_64(n, a.DataPtr, y.DataPtr);

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


            // warpper
            /// <summary>
            /// computes tangent of array elements
            /// </summary>
            /// <typeparam name="T"> ArrayBase[double] </typeparam>
            /// <param name="n"> number of elements </param>
            /// <param name="a"> input array a </param>
            /// <param name="y"> result array y </param>
            public void TanD<T>(long n, T a, ref T y)
                where T : DenseArrayBase<double>
                => IntelMKLNative.vdTan_64(n, a.DataPtr, y.DataPtr);

            /// <summary>
            /// computes tangent of array elements
            /// </summary>
            /// <typeparam name="T"> ArrayBase[Complex] </typeparam>
            /// <param name="n"> number of elements </param>
            /// <param name="a"> input array a </param>
            /// <param name="y"> result array y </param>
            public void TanZ<T>(long n, T a, ref T y)
                where T : DenseArrayBase<Complex>
                => IntelMKLNative.vzTan_64(n, a.DataPtr, y.DataPtr);

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


            // wrapper
            /// <summary>
            /// computes hyperbolic cosine of array elements
            /// </summary>
            /// <typeparam name="T"> ArrayBase[double] </typeparam>
            /// <param name="n"> number of elements </param>
            /// <param name="a"> input array a </param>
            /// <param name="y"> result array y </param>
            public void CoshD<T>(long n, T a, ref T y)
                where T : DenseArrayBase<double>
                => IntelMKLNative.vdCosh_64(n, a.DataPtr, y.DataPtr);

            /// <summary>
            /// computes hyperbolic cosine of array elements
            /// </summary>
            /// <typeparam name="T"> ArrayBase[Complex] </typeparam>
            /// <param name="n"> number of elements </param>
            /// <param name="a"> input array a </param>
            /// <param name="y"> result array y </param>
            public void CoshZ<T>(long n, T a, ref T y)
                where T : DenseArrayBase<Complex>
                => IntelMKLNative.vzCosh_64(n, a.DataPtr, y.DataPtr);

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


            // wrapper
            /// <summary>
            /// computes hyperbolic sine of array elements
            /// </summary>
            /// <typeparam name="T"> ArrayBase[double] </typeparam>
            /// <param name="n"> number of elements </param>
            /// <param name="a"> input array a </param>
            /// <param name="y"> result array y </param>
            public void SinhD<T>(long n, T a, ref T y)
                where T : DenseArrayBase<double>
                => IntelMKLNative.vdSinh_64(n, a.DataPtr, y.DataPtr);

            /// <summary>
            /// computes hyperbolic sine of array elements
            /// </summary>
            /// <typeparam name="T"> ArrayBase[Complex] </typeparam>
            /// <param name="n"> number of elements </param>
            /// <param name="a"> input array a </param>
            /// <param name="y"> result array y </param>
            public void SinhZ<T>(long n, T a, ref T y)
                where T : DenseArrayBase<Complex>
                => IntelMKLNative.vzSinh_64(n, a.DataPtr, y.DataPtr);

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


            // wrapper
            /// <summary>
            /// computes hyperbolic tangent of array elements
            /// </summary>
            /// <typeparam name="T"> ArrayBase[double] </typeparam>
            /// <param name="n"> number of elements </param>
            /// <param name="a"> input array a </param>
            /// <param name="y"> result array y </param>
            public void TanhD<T>(long n, T a, ref T y)
                where T : DenseArrayBase<double>
                => IntelMKLNative.vdTanh_64(n, a.DataPtr, y.DataPtr);

            /// <summary>
            /// computes hyperbolic tangent of array elements
            /// </summary>
            /// <typeparam name="T"> ArrayBase[Complex] </typeparam>
            /// <param name="n"> number of elements </param>
            /// <param name="a"> input array a </param>
            /// <param name="y"> result array y </param>
            public void TanhZ<T>(long n, T a, ref T y)
                where T : DenseArrayBase<Complex>
                => IntelMKLNative.vzTanh_64(n, a.DataPtr, y.DataPtr);

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


            // wrapper
            /// <summary>
            /// computes inverse cosine of array elements
            /// </summary>
            /// <typeparam name="T"> ArrayBase[double] </typeparam>
            /// <param name="n"> number of elements </param>
            /// <param name="a"> input array a </param>
            /// <param name="y"> result array y </param>
            public void AcosD<T>(long n, T a, ref T y)
                where T : DenseArrayBase<double>
                => IntelMKLNative.vdAcos_64(n, a.DataPtr, y.DataPtr);

            /// <summary>
            /// computes inverse cosine of array elements
            /// </summary>
            /// <typeparam name="T"> ArrayBase[Complex] </typeparam>
            /// <param name="n"> number of elements </param>
            /// <param name="a"> input array a </param>
            /// <param name="y"> result array y </param>
            public void AcosZ<T>(long n, T a, ref T y)
                where T : DenseArrayBase<Complex>
                => IntelMKLNative.vzAcos_64(n, a.DataPtr, y.DataPtr);

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


            // wrapper
            /// <summary>
            /// computes inverse sine of array elements
            /// </summary>
            /// <typeparam name="T"> ArrayBase[double] </typeparam>
            /// <param name="n"> number of elements </param>
            /// <param name="a"> input array a </param>
            /// <param name="y"> result array y </param>
            public void AsinD<T>(long n, T a, ref T y)
                where T : DenseArrayBase<double>
                => IntelMKLNative.vdAsin_64(n, a.DataPtr, y.DataPtr);

            /// <summary>
            /// computes inverse sine of array elements
            /// </summary>
            /// <typeparam name="T"> ArrayBase[Complex] </typeparam>
            /// <param name="n"> number of elements </param>
            /// <param name="a"> input array a </param>
            /// <param name="y"> result array y </param>
            public void AsinZ<T>(long n, T a, ref T y)
                where T : DenseArrayBase<Complex>
                => IntelMKLNative.vzAsin_64(n, a.DataPtr, y.DataPtr);

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


            // wrapper
            /// <summary>
            /// Computes inverse tangent of array elements
            /// </summary>
            /// <typeparam name="T"> ArrayBase[double] </typeparam>
            /// <param name="n"> number of elements </param>
            /// <param name="a"> input array a </param>
            /// <param name="y"> result array y </param>
            public void AtanD<T>(long n, T a, ref T y)
                where T : DenseArrayBase<double>
                => IntelMKLNative.vdAtan_64(n, a.DataPtr, y.DataPtr);

            /// <summary>
            /// Computes inverse tangent of array elements
            /// </summary>
            /// <typeparam name="T"> ArrayBase[Complex] </typeparam>
            /// <param name="n"> number of elements </param>
            /// <param name="a"> input array a </param>
            /// <param name="y"> result array y </param>
            public void AtanZ<T>(long n, T a, ref T y)
                where T : DenseArrayBase<Complex>
                => IntelMKLNative.vzAtan_64(n, a.DataPtr, y.DataPtr);

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


            // wrapper
            /// <summary>
            /// computes inverse hyperbolic cosine (nonnegative) of array elements
            /// </summary>
            /// <typeparam name="T"> ArrayBase[double] </typeparam>
            /// <param name="n"> number of elements </param>
            /// <param name="a"> input array a </param>
            /// <param name="y"> result array y </param>
            public void AcoshD<T>(long n, T a, ref T y)
                where T : DenseArrayBase<double>
                => IntelMKLNative.vdAcosh_64(n, a.DataPtr, y.DataPtr);

            /// <summary>
            /// computes inverse hyperbolic cosine (nonnegative) of array elements
            /// </summary>
            /// <typeparam name="T"> ArrayBase[Complex] </typeparam>
            /// <param name="n"> number of elements </param>
            /// <param name="a"> input array a </param>
            /// <param name="y"> result array y </param>
            public void AcoshZ<T>(long n, T a, ref T y)
                where T : DenseArrayBase<Complex>
                => IntelMKLNative.vzAcosh_64(n, a.DataPtr, y.DataPtr);

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


            // wrapper
            /// <summary>
            /// computes inverse hyperbolic sine of array elements
            /// </summary>
            /// <typeparam name="T"> ArrayBase[double] </typeparam>
            /// <param name="n"> number of elements </param>
            /// <param name="a"> input array a </param>
            /// <param name="y"> result array y </param>
            public void AsinhD<T>(long n, T a, ref T y)
                where T : DenseArrayBase<double>
                => IntelMKLNative.vdAsinh_64(n, a.DataPtr, y.DataPtr);

            /// <summary>
            /// computes inverse hyperbolic sine of array elements
            /// </summary>
            /// <typeparam name="T"> ArrayBase[Complex] </typeparam>
            /// <param name="n"> number of elements </param>
            /// <param name="a"> input array a </param>
            /// <param name="y"> result array y </param>
            public void AsinhZ<T>(long n, T a, ref T y)
                where T : DenseArrayBase<Complex>
                => IntelMKLNative.vzAsinh_64(n, a.DataPtr, y.DataPtr);

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


            // wrapper
            /// <summary>
            /// computes inverse hyperbolic tangent of array elements
            /// </summary>
            /// <typeparam name="T"> ArrayBase[double] </typeparam>
            /// <param name="n"> number of elements </param>
            /// <param name="a"> input array a </param>
            /// <param name="y"> result array y </param>
            public void AtanhD<T>(long n, T a, ref T y)
                where T : DenseArrayBase<double>
                => IntelMKLNative.vdAtanh_64(n, a.DataPtr, y.DataPtr);

            /// <summary>
            /// computes inverse hyperbolic tangent of array elements
            /// </summary>
            /// <typeparam name="T"> ArrayBase[Complex] </typeparam>
            /// <param name="n"> number of elements </param>
            /// <param name="a"> input array a </param>
            /// <param name="y"> result array y </param>
            public void AtanhZ<T>(long n, T a, ref T y)
                where T : DenseArrayBase<Complex>
                => IntelMKLNative.vzAtanh_64(n, a.DataPtr, y.DataPtr);

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


            // wrapper
            /// <summary>
            /// computes a square root of sum of two squared elements
            /// </summary>
            /// <typeparam name="T"> ArrayBase[double] </typeparam>
            /// <param name="n"> number of elements </param>
            /// <param name="a"> input array a </param>
            /// <param name="b"> input array b </param>
            /// <param name="y"> result array y </param>
            public void HypotD<T>(long n, T a, T b, ref T y)
                where T : DenseArrayBase<double>
                => IntelMKLNative.vdHypot_64(n, a.DataPtr, b.DataPtr, y.DataPtr);

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


            // wrapper
            /// <summary>
            /// computes four-quadrant inverse tangent of elements of two vectors
            /// </summary>
            /// <typeparam name="T"> ArrayBase[double] </typeparam>
            /// <param name="n"> number of elements </param>
            /// <param name="a"> input vector a </param>
            /// <param name="b"> input vector b </param>
            /// <param name="y"> result vector y </param>
            public void Atan2D<T>(long n, T a, T b, ref T y)
                where T : DenseArrayBase<double>
                => IntelMKLNative.vdAtan2_64(n, a.DataPtr, b.DataPtr, y.DataPtr);

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


            // wrapper
            /// <summary>
            /// performs element by element multiplication of array a and array b
            /// </summary>
            /// <typeparam name="T"> ArrayBase[double] </typeparam>
            /// <param name="n"> number of elements </param>
            /// <param name="a"> input array a </param>
            /// <param name="b"> input array b </param>
            /// <param name="y"> result array y </param>
            public void MulD<T>(long n, T a, T b, ref T y)
                where T : DenseArrayBase<double>
                => IntelMKLNative.vdMul_64(n, a.DataPtr, b.DataPtr, y.DataPtr);

            /// <summary>
            /// performs element by element multiplication of array a and array b
            /// </summary>
            /// <typeparam name="T"> ArrayBase[Complex] </typeparam>
            /// <param name="n"> number of elements </param>
            /// <param name="a"> input array a </param>
            /// <param name="b"> input array b </param>
            /// <param name="y"> result array y </param>
            public void MulZ<T>(long n, T a, T b, ref T y)
                where T : DenseArrayBase<Complex>
                => IntelMKLNative.vzMul_64(n, a.DataPtr, b.DataPtr, y.DataPtr);

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


            // wrapper
            /// <summary>
            /// performs element by element division of array a by array b
            /// </summary>
            /// <typeparam name="T"> ArrayBase[double] </typeparam>
            /// <param name="n"> number of elements </param>
            /// <param name="a"> input array a </param>
            /// <param name="b"> input array b </param>
            /// <param name="y"> result array y </param>
            public void DivD<T>(long n, T a, T b, ref T y)
                where T : DenseArrayBase<double>
                => IntelMKLNative.vdDiv_64(n, a.DataPtr, b.DataPtr, y.DataPtr);

            /// <summary>
            /// performs element by element division of array a by array b
            /// </summary>
            /// <typeparam name="T"> ArrayBase[Complex] </typeparam>
            /// <param name="n"> number of elements </param>
            /// <param name="a"> input array a </param>
            /// <param name="b"> input array b </param>
            /// <param name="y"> result array y </param>
            public void DivZ<T>(long n, T a, T b, ref T y)
                where T : DenseArrayBase<Complex>
                => IntelMKLNative.vzDiv_64(n, a.DataPtr, b.DataPtr, y.DataPtr);

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


            // wrapper
            /// <summary>
            /// computes a to the power b for elements of two arrays
            /// </summary>
            /// <typeparam name="T"> ArrayBase[double] </typeparam>
            /// <param name="n"> number of elements </param>
            /// <param name="a"> input arrays a </param>
            /// <param name="b"> input arrays b </param>
            /// <param name="y"> result arrays y </param>
            public void PowD<T>(long n, T a, T b, ref T y)
                where T : DenseArrayBase<double>
                => IntelMKLNative.vdPow_64(n, a.DataPtr, b.DataPtr, y.DataPtr);

            /// <summary>
            /// computes a to the power b for elements of two arrays
            /// </summary>
            /// <typeparam name="T"> ArrayBase[Complex] </typeparam>
            /// <param name="n"> number of elements </param>
            /// <param name="a"> input array a </param>
            /// <param name="b"> input array b </param>
            /// <param name="y"> result array y </param>
            public void PowZ<T>(long n, T a, T b, ref T y)
                where T : DenseArrayBase<Complex>
                => IntelMKLNative.vzPow_64(n, a.DataPtr, b.DataPtr, y.DataPtr);

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


            // wrapper
            /// <summary>
            /// computes the square root of the cube of each array element
            /// </summary>
            /// <typeparam name="T"> ArrayBase[double] </typeparam>
            /// <param name="n"> number of elements </param>
            /// <param name="a"> input array a </param>
            /// <param name="y"> result array y </param>
            public void Pow3o2D<T>(long n, T a, ref T y)
                where T : DenseArrayBase<double>
                => IntelMKLNative.vdPow3o2_64(n, a.DataPtr, y.DataPtr);

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


            // wrapper
            /// <summary>
            /// computes the cube root of the square of each array element
            /// </summary>
            /// <typeparam name="T"> ArrayBase[double] </typeparam>
            /// <param name="n"> number of elements </param>
            /// <param name="a"> input array a </param>
            /// <param name="y"> result array y </param>
            public void Pow2o3D<T>(long n, T a, ref T y)
                where T : DenseArrayBase<double>
                => IntelMKLNative.vdPow2o3_64(n, a.DataPtr, y.DataPtr);

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


            // wrapper
            /// <summary>
            /// computes each element of array a to the scalar power b
            /// </summary>
            /// <typeparam name="T"> ArrayBase[double] </typeparam>
            /// <param name="n"> number of elements </param>
            /// <param name="a"> input array a </param>
            /// <param name="b"> input scalar b </param>
            /// <param name="y"> result array y </param>
            public void PowxD<T>(long n, T a, double b, ref T y)
                where T : DenseArrayBase<double>
                => IntelMKLNative.vdPowx_64(n, a.DataPtr, b, y.DataPtr);

            /// <summary>
            /// computes each element of array a to the scalar power b
            /// </summary>
            /// <typeparam name="T"> ArrayBase[Complex] </typeparam>
            /// <param name="n"> number of elements </param>
            /// <param name="a"> input array a </param>
            /// <param name="b"> input scalar b </param>
            /// <param name="y"> result array y </param>
            public void PowxZ<T>(long n, T a, Complex b, ref T y)
                where T : DenseArrayBase<Complex>
                => IntelMKLNative.vzPowx_64(n, a.DataPtr, ref b, y.DataPtr);

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


            // wrapper
            /// <summary>
            /// computes sine and cosine of array elements
            /// </summary>
            /// <typeparam name="T"> ArrayBase[double] </typeparam>
            /// <param name="n"> number of elements </param>
            /// <param name="a"> input array a </param>
            /// <param name="sin"> result array sin </param>
            /// <param name="cos"> result array cos </param>
            public void SinCosD<T>(long n, T a, ref T sin, ref T cos)
                where T : DenseArrayBase<double>
                => IntelMKLNative.vdSinCos_64(n, a.DataPtr, sin.DataPtr, cos.DataPtr);

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


            // wrapper
            /// <summary>
            /// performs linear fraction transformation of 
            /// vectors a and b with scalar parameters
            /// y[i] = (scalea*a[i]+shifta) / (scaleb*b[i]+shiftb)
            /// </summary>
            /// <typeparam name="T"> ArrayBase[double] </typeparam>
            /// <param name="n"> number of elements </param>
            /// <param name="a"> input array a </param>
            /// <param name="b"> input array b </param>
            /// <param name="scalea"> scale of a </param>
            /// <param name="shifta"> shift of a </param>
            /// <param name="scaleb"> scale of b </param>
            /// <param name="shiftb"> shift of b </param>
            /// <param name="y"> result array y </param>
            public void LinearFracD<T>(long n, T a, T b,
                double scalea, double shifta, double scaleb, double shiftb,
                ref T y) where T : DenseArrayBase<double>
                => IntelMKLNative.vdLinearFrac_64(n, a.DataPtr, b.DataPtr,
                    scalea, shifta, scaleb, shiftb, y.DataPtr);

            /// <summary>
            /// performs linear fraction transformation of 
            /// vectors a and b with scalar parameters
            /// real-part: y[i].Re = (scalea*a[i].Re + shifta.Re) / (scaleb*b[i].Re + shiftb.Re)
            /// imag-part: y[i].Im = (scalea*a[i].Im + shifta.Im) / (scaleb*b[i].Im + shiftb.Im)
            /// </summary>
            /// <typeparam name="T"> ArrayBase[Complex] </typeparam>
            /// <param name="n"> number of elements </param>
            /// <param name="a"> input array a </param>
            /// <param name="b"> input array b </param>
            /// <param name="scalea"> scale of a </param>
            /// <param name="shifta"> shift of a </param>
            /// <param name="scaleb"> scale of b </param>
            /// <param name="shiftb"> shift of b </param>
            /// <param name="y"> result array y </param>
            public unsafe void LinearFracZ<T>(long n, T a, T b,
                double scalea, Complex shifta, double scaleb, Complex shiftb,
                ref T y) where T : DenseArrayBase<Complex>
            {
                Complex* pa = (Complex*)a.DataPtr.ToPointer();
                Complex* pb = (Complex*)b.DataPtr.ToPointer();
                Complex* py = (Complex*)y.DataPtr.ToPointer();
                // real-part
                IntelMKLNative.vdLinearFracI(n, (double*)pa + 0, 2, (double*)pb + 0, 2,
                    scalea, shifta.Real, scaleb, shiftb.Real, (double*)py + 0, 2);
                // imag-part
                if (shifta.Imaginary != 0.0 || shiftb.Imaginary != 0.0)
                {
                    IntelMKLNative.vdLinearFracI(n, (double*)pa + 1, 2, (double*)pb + 1, 2,
                        scalea, shifta.Imaginary, scaleb, shiftb.Imaginary, (double*)py + 1, 2);
                }
            }

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


            // wrapper
            /// <summary>
            /// performs element by element conjugation of the array
            /// </summary>
            /// <typeparam name="T"> ArrayBase[Complex] </typeparam>
            /// <param name="n"> number of elements </param>
            /// <param name="a"> input array a </param>
            /// <param name="y"> result array y </param>
            public void ConjZ<T>(long n, T a, ref T y)
                where T : DenseArrayBase<Complex>
                => IntelMKLNative.vzConj_64(n, a.DataPtr, y.DataPtr);

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

            // wrapper
            /// <summary>
            /// performs element by element multiplication of array a element 
            /// and conjugated array b element
            /// </summary>
            /// <typeparam name="T"> ArrayBase[Complex] </typeparam>
            /// <param name="n"> number of elements </param>
            /// <param name="a"> input array a </param>
            /// <param name="b"> input array b </param>
            /// <param name="y"> result array y </param>
            public void MulByConjZ<T>(long n, T a, T b, ref T y)
                where T : DenseArrayBase<Complex>
                => IntelMKLNative.vzMulByConj_64(n, a.DataPtr, b.DataPtr, y.DataPtr);

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


            // warpper
            /// <summary>
            /// copies elements of an array with specified indexing 
            /// to an array with unit increment
            /// </summary>
            /// <typeparam name="T"> ArrayBase[double] </typeparam>
            /// <param name="n"> number of elements </param>
            /// <param name="a"> input array a </param>
            /// <param name="y"> result array y </param>
            /// <param name="inca"> increment for the elements of a </param>
            public void PackID<T>(long n, T a, ref T y,
                long inca = 1) where T : DenseArrayBase<double>
                => IntelMKLNative.vdPackI(n, a.DataPtr, inca, y.DataPtr);

            /// <summary>
            /// copies elements of an array with specified indexing 
            /// to an array with unit increment
            /// </summary>
            /// <typeparam name="T"> ArrayBase[Complex] </typeparam>
            /// <param name="n"> number of elements </param>
            /// <param name="a"> input array a </param>
            /// <param name="inca"> increment for the elements of a </param>
            /// <param name="y"> result array y </param>
            public void PackIZ<T>(long n, T a, ref T y,
                long inca = 1) where T : DenseArrayBase<Complex>
                => IntelMKLNative.vzPackI(n, a.DataPtr, inca, y.DataPtr);

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


            // wrapper
            /// <summary>
            /// takes out specified elements from one array to another
            /// </summary>
            /// <typeparam name="T"> ArrayBase[double] </typeparam>
            /// <param name="n"> number of elements to be taken </param>
            /// <param name="a"> source array </param>
            /// <param name="ia"> array of element indices </param>
            /// <param name="y"> result array </param>
            public void PackVD<T>(long n, T a, long[] ia, ref T y)
                where T : DenseArrayBase<double>
            {
                //fixed (long* pia = &ia[0])
                //    IntelMKLNative.vdPackV(n, pa, pia, py);
                throw new NotImplementedException();
            }

            /// <summary>
            /// takes out specified elements from one array to another
            /// </summary>
            /// <typeparam name="T"> ArrayBase[Complex] </typeparam>
            /// <param name="n"> number of elements to be taken </param>
            /// <param name="a"> source array </param>
            /// <param name="ia"> array of element indices </param>
            /// <param name="y"> result array </param>
            public void PackVZ<T>(long n, T a, long[] ia, ref T y)
                where T : DenseArrayBase<Complex>
            {
                //fixed (long* pia = &ia[0])
                //    IntelMKLNative.vzPackV(n, pa, pia, py);
                throw new NotImplementedException();
            }

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


            // warpper
            /// <summary>
            /// Copies elements of an array with unit increment 
            /// to an array with specified indexing
            /// </summary>
            /// <typeparam name="T"> ArrayBase[double] </typeparam>
            /// <param name="n"> number of elements </param>
            /// <param name="a"> input array a </param>
            /// <param name="y"> result array y </param>
            /// <param name="incy"> increment for the elements of y </param>
            public void UnpackID<T>(long n, T a, ref T y,
                long incy = 1) where T : DenseArrayBase<double>
                => IntelMKLNative.vdUnpackI(n, a.DataPtr, y.DataPtr, incy);

            /// <summary>
            /// Copies elements of an array with unit increment 
            /// to an array with specified indexing
            /// </summary>
            /// <typeparam name="T"> ArrayBase[Complex] </typeparam>
            /// <param name="n"> number of elements </param>
            /// <param name="a"> input array a </param>
            /// <param name="y"> result array y </param>
            /// <param name="incy"> increment for the elements of y </param>
            public void UnpackIZ<T>(long n, T a, ref T y,
                long incy = 1) where T : DenseArrayBase<Complex>
                => IntelMKLNative.vzUnpackI(n, a.DataPtr, y.DataPtr, incy);

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

            // warpper
            /// <summary>
            /// takes the real part of each complex array element
            /// </summary>
            /// <typeparam name="T1"> ArrayBase[Complex] </typeparam>
            /// <typeparam name="T2"> ArrayBase[double] </typeparam>
            /// <param name="n"> number of elements </param>
            /// <param name="a"> input array a </param>
            /// <param name="y"> result array y </param>
            public void RealPart<T1, T2>(long n, T1 a, ref T2 y)
                where T1 : DenseArrayBase<Complex>
                where T2 : DenseArrayBase<double>
            {
                IntelMKLNative.vdPackI(n, a.DataPtr, 2, y.DataPtr);
            }

            /// <summary>
            /// takes the imaginary part of each complex array element
            /// </summary>
            /// <typeparam name="T1"> ArrayBase[Complex] </typeparam>
            /// <typeparam name="T2"> ArrayBase[double] </typeparam>
            /// <param name="n"> number of elements </param>
            /// <param name="a"> input array a </param>
            /// <param name="y"> result array y </param>
            public void ImagPart<T1, T2>(long n, T1 a, ref T2 y)
                where T1 : DenseArrayBase<Complex>
                where T2 : DenseArrayBase<double>
            {
                IntPtr a1 = IntPtr.Add(a.DataPtr, 1 * Marshal.SizeOf<double>());
                IntelMKLNative.vdPackI(n, a1, 2, y.DataPtr);
            }

            /// <summary>
            /// takes the real and imaginary parts of each complex array element
            /// </summary>
            /// <typeparam name="T1"> ArrayBase[Complex] </typeparam>
            /// <typeparam name="T2"> ArrayBase[double] </typeparam>
            /// <param name="n"> number of elements </param>
            /// <param name="a"> input array a </param>
            /// <param name="re"> result real part array </param>
            /// <param name="im"> result imaginary part array </param>
            public void RealImagParts<T1, T2>(long n, T1 a, ref T2 re, ref T2 im)
                where T1 : DenseArrayBase<Complex>
                where T2 : DenseArrayBase<double>
            {
                IntPtr a1 = IntPtr.Add(a.DataPtr, 1 * Marshal.SizeOf<double>());
                IntelMKLNative.vdPackI(n, a.DataPtr, 2, re.DataPtr);
                IntelMKLNative.vdPackI(n, a1, 2, im.DataPtr);
            }

            #endregion
            #region ---- Modify ----

            /// <summary>
            /// modifies the real and imaginary part of a complex array
            /// </summary>
            /// <typeparam name="T1"> ArrayBase[double] </typeparam>
            /// <typeparam name="T2"> ArrayBase[Complex] </typeparam>
            /// <param name="n"> number of elements </param>
            /// <param name="re"> real part array </param>
            /// <param name="im"> imaginary part array </param>
            /// <param name="y"> result array (input and modified) </param>
            public void Modify<T1, T2>(long n, T1 re, T1 im, ref T2 y)
                where T1 : DenseArrayBase<double>
                where T2 : DenseArrayBase<Complex>
            {
                IntPtr y1 = IntPtr.Add(y.DataPtr, 1 * Marshal.SizeOf<double>());
                IntelMKLNative.vdUnpackI(n, re.DataPtr, y.DataPtr, 2);
                IntelMKLNative.vdUnpackI(n, im.DataPtr, y1, 2);
            }

            /// <summary>
            /// modifies the real part of a complex array
            /// </summary>
            /// <typeparam name="T1"> ArrayBase[double] </typeparam>
            /// <typeparam name="T2"> ArrayBase[Complex] </typeparam>
            /// <param name="n"> number of elements </param>
            /// <param name="re"> real part </param>
            /// <param name="y"> input and modified array result </param>
            public void ModifyReal<T1, T2>(long n, T1 re, ref T2 y)
                where T1 : DenseArrayBase<double>
                where T2 : DenseArrayBase<Complex>
            {
                IntelMKLNative.vdUnpackI(n, re.DataPtr, y.DataPtr, 2);
            }

            /// <summary>
            /// modifies the imaginary part of a complex array
            /// </summary>
            /// <typeparam name="T1"> ArrayBase[double] </typeparam>
            /// <typeparam name="T2"> ArrayBase[Complex] </typeparam>
            /// <param name="n"> number of elements </param>
            /// <param name="im"> imaginary part </param>
            /// <param name="y"> input and modified array result </param>
            public void ModifyImag<T1, T2>(long n, T1 im, ref T2 y)
                where T1 : DenseArrayBase<double>
                where T2 : DenseArrayBase<Complex>
            {
                IntPtr y1 = IntPtr.Add(y.DataPtr, 1 * Marshal.SizeOf<double>());
                IntelMKLNative.vdUnpackI(n, im.DataPtr, y1, 2);
            }

            #endregion
        }



        #region LAPACK

        #region --------- Getrf ---------

        /// <summary>
        /// computes the LU factorization of a general m-by-n matrix
        /// </summary>
        /// <param name="layout"> matrix storage layout is row or column major </param>
        /// <param name="m"> number of rows in matrix a </param>
        /// <param name="n"> number of columns in matrix a </param>
        /// <param name="a"> matrix a (overwritten by L and U on exit) </param>
        /// <param name="lda"> leading dimension of a </param>
        /// <param name="ipiv"> pivot indices </param>
        /// <param name="starta"> starting index in a </param>
        /// <param name="startipiv"> starting index in ipiv </param>
        public unsafe void Getrf(LAPACK_Layout layout, long m, long n,
            DenseArrayBase<double> a, long lda, DenseArrayBase<long> ipiv,
            long starta = 0, long startipiv = 0)
        {
            double* pa = (double*)a.DataPtr.ToPointer() + starta;
            long* pipiv = (long*)ipiv.DataPtr.ToPointer() + startipiv;
            _ = IntelMKLNative.LAPACKE_dgetrf((int)layout, m, n, pa, lda, pipiv);            
        }

        /// <summary>
        /// computes the LU factorization of a general m-by-n matrix
        /// </summary>
        /// <param name="layout"> matrix storage layout is row or column major </param>
        /// <param name="m"> number of rows in matrix a </param>
        /// <param name="n"> number of columns in matrix a </param>
        /// <param name="a"> matrix a (overwritten by L and U on exit) </param>
        /// <param name="lda"> leading dimension of a </param>
        /// <param name="ipiv"> pivot indices </param>
        /// <param name="starta"> starting index in a </param>
        /// <param name="startipiv"> starting index in ipiv </param>
        public unsafe void Getrf(LAPACK_Layout layout, long m, long n,
            DenseArrayBase<Complex> a, long lda, DenseArrayBase<long> ipiv,
            long starta = 0, long startipiv = 0)
        {
            Complex* pa = (Complex*)a.DataPtr.ToPointer() + starta;
            long* pipiv = (long*)ipiv.DataPtr.ToPointer() + startipiv;
            _ = IntelMKLNative.LAPACKE_zgetrf((int)layout, m, n, pa, lda, pipiv);
        }

        #endregion
        #region --------- Getrs ---------

        /// <summary>
        /// solves a system of linear equations a * x = b
        /// with an LU-factored square coefficient matrix
        /// </summary>
        /// <param name="layout"> matrix storage layout is row or column major </param>
        /// <param name="operation"> whether to solve the system with matrix a transposed </param>
        /// <param name="n"> number of columns in a; number of rows in b </param>
        /// <param name="nrhs"> number of right-hand sides </param>
        /// <param name="a"> result of LU factorization of a </param>
        /// <param name="lda"> leading dimension of a </param>
        /// <param name="ipiv"> pivot indices </param>
        /// <param name="b"> vector or matrix b (overwritte by x on exit) </param>
        /// <param name="ldb"> leading dimension of b </param>
        /// <param name="starta"> starting index in a </param>
        /// <param name="startipiv"> starting index in ipiv </param>
        /// <param name="startb"> starting index in b </param>
        public unsafe void Getrs(LAPACK_Layout layout, LAPACK_Transpose operation,
            long n, long nrhs, DenseArrayBase<double> a, long lda, DenseArrayBase<long> ipiv, 
            DenseArrayBase<double> b, long ldb,
            long starta = 0, long startipiv = 0, long startb = 0)
        {
            double* pa = (double*)a.DataPtr.ToPointer() + starta;
            double* pb = (double*)b.DataPtr.ToPointer() + startb;
            long* pipiv = (long*)ipiv.DataPtr.ToPointer() + startipiv;
            _ = IntelMKLNative.LAPACKE_dgetrs((int)layout, (char)operation, n, nrhs,
                    pa, lda, pipiv, pb, ldb);
        }

        /// <summary>
        /// solves a system of linear equations a * x = b
        /// with an LU-factored square coefficient matrix
        /// </summary>
        /// <param name="layout"> matrix storage layout is row or column major </param>
        /// <param name="operation"> whether to solve the system with matrix a transposed </param>
        /// <param name="n"> number of columns in a; number of rows in b </param>
        /// <param name="nrhs"> number of right-hand sides </param>
        /// <param name="a"> result of LU factorization of a </param>
        /// <param name="lda"> leading dimension of a </param>
        /// <param name="ipiv"> pivot indices </param>
        /// <param name="b"> vector or matrix b (overwritte by x on exit) </param>
        /// <param name="ldb"> leading dimension of b </param>
        /// <param name="starta"> starting index in a </param>
        /// <param name="startipiv"> starting index in ipiv </param>
        /// <param name="startb"> starting index in b </param>
        public unsafe void Getrs(LAPACK_Layout layout, LAPACK_Transpose operation,
            long n, long nrhs, DenseArrayBase<Complex> a, long lda, DenseArrayBase<long> ipiv,
            DenseArrayBase<Complex> b, long ldb,
            long starta = 0, long startipiv = 0, long startb = 0)
        {
            Complex* pa = (Complex*)a.DataPtr.ToPointer()+starta;
            Complex* pb = (Complex*)b.DataPtr.ToPointer() + startb;
            long* pipiv = (long*)ipiv.DataPtr.ToPointer() + startipiv;
            _ = IntelMKLNative.LAPACKE_zgetrs((int)layout, (char)operation, n, nrhs,
                    pa, lda, pipiv, pb, ldb);
        }

        #endregion
        #region --------- Getri ---------

        /// <summary>
        /// computes the inverse of an LU-factored general matrix
        /// </summary>
        /// <param name="layout"> matrix storage layout is row or column major </param>
        /// <param name="n"> number of columns in a; number of rows in b </param>
        /// <param name="a"> result of LU factorization of a </param>
        /// <param name="lda"> leading dimension of a </param>
        /// <param name="ipiv"> pivot indices </param>
        /// <param name="starta"> starting index in a </param>
        /// <param name="startipiv"> starting index in ipiv </param>
        public unsafe void Getri(LAPACK_Layout layout, long n, 
            DenseArrayBase<double> a, long lda, DenseArrayBase<long> ipiv,
            long starta = 0, long startipiv = 0)
        {
            double* pa = (double*)a.DataPtr.ToPointer() + starta;
            long* pipiv = (long*)ipiv.DataPtr.ToPointer() + startipiv;
            _ = IntelMKLNative.LAPACKE_dgetri((int)layout, n, pa, lda, pipiv);
        }

        /// <summary>
        /// computes the inverse of an LU-factored general matrix
        /// </summary>
        /// <param name="layout"> matrix storage layout is row or column major </param>
        /// <param name="n"> number of columns in a; number of rows in b </param>
        /// <param name="a"> result of LU factorization of a </param>
        /// <param name="lda"> leading dimension of a </param>
        /// <param name="ipiv"> pivot indices </param>
        /// <param name="starta"> starting index in a </param>
        /// <param name="startipiv"> starting index in ipiv </param>
        public unsafe void Getri(LAPACK_Layout layout, long n,
            DenseArrayBase<Complex> a, long lda, DenseArrayBase<long> ipiv,
            long starta = 0, long startipiv = 0)
        {
            Complex* pa = (Complex*)a.DataPtr.ToPointer() + starta;
            long* pipiv = (long*)ipiv.DataPtr.ToPointer() + startipiv;
            _ = IntelMKLNative.LAPACKE_zgetri((int)layout, n, pa, lda, pipiv);
        }

        #endregion
        #region --------- Gesv ---------

        /// <summary>
        /// Computes the solution to the system of linear equations 
        /// with a square coefficient matrix A and multiple right-hand sides
        /// </summary>
        /// <param name="layout"> matrix storage layout is row or column major </param>
        /// <param name="n"> the number of linear equations i.e. order of matrix a </param>
        /// <param name="nrhs"> the number of right-hand sides i.e. columns of matrix b </param>
        /// <param name="a"> matrix a (overwritten by the factor L and U from the factorization) </param>
        /// <param name="lda"> leading dimension of a </param>
        /// <param name="ipiv"> the pivot indices that define the permutation matrix p </param>
        /// <param name="b"> vector b (overwritten by the solution vector x) </param>
        /// <param name="ldb"> leading dimension of b </param>
        /// <param name="starta"> starting index in a </param>
        /// <param name="startipiv"> starting index in ipiv </param>
        /// <param name="startb"> starting index in b </param>
        public unsafe void Gesv(LAPACK_Layout layout, long n, long nrhs,
            DenseArrayBase<double> a, long lda, DenseArrayBase<long> ipiv,
            DenseArrayBase<double> b, long ldb,
            long starta = 0, long startipiv = 0, long startb = 0)
        {
            double* pa = (double*)a.DataPtr.ToPointer() + starta;
            double* pb = (double*)b.DataPtr.ToPointer() + startb;
            long* pipiv = (long*)ipiv.DataPtr.ToPointer() + startipiv;
            _ = IntelMKLNative.LAPACKE_dgesv((int)layout, n, nrhs,
                    pa, lda, pipiv, pb, ldb);
        }

        /// <summary>
        /// Computes the solution to the system of linear equations 
        /// with a square coefficient matrix A and multiple right-hand sides
        /// </summary>
        /// <param name="layout"> matrix storage layout is row or column major </param>
        /// <param name="n"> the number of linear equations i.e. order of matrix a </param>
        /// <param name="nrhs"> the number of right-hand sides i.e. columns of matrix b </param>
        /// <param name="a"> matrix a (overwritten by the factor L and U from the factorization) </param>
        /// <param name="lda"> leading dimension of a </param>
        /// <param name="ipiv"> the pivot indices that define the permutation matrix p </param>
        /// <param name="b"> vector b (overwritten by the solution vector x) </param>
        /// <param name="ldb"> leading dimension of b </param>
        /// <param name="starta"> starting index in a </param>
        /// <param name="startipiv"> starting index in ipiv </param>
        /// <param name="startb"> starting index in b </param>
        public unsafe void Gesv(LAPACK_Layout layout, long n, long nrhs,
            DenseArrayBase<Complex> a, long lda, DenseArrayBase<long> ipiv,
            DenseArrayBase<Complex> b, long ldb,
            long starta = 0, long startipiv = 0, long startb = 0)
        {
            Complex* pa = (Complex*)a.DataPtr.ToPointer() + starta;
            Complex* pb = (Complex*)b.DataPtr.ToPointer() + startb;
            long* pipiv = (long*)ipiv.DataPtr.ToPointer() + startipiv;
            _ = IntelMKLNative.LAPACKE_zgesv((int)layout, n, nrhs,
                    pa, lda, pipiv, pb, ldb);
        }

        #endregion
        #region --------- Gels ---------

        /// <summary>
        /// solves overdetermined or underdetermined linear least square problem
        /// </summary>
        /// <param name="layout"> matrix storage layout is row or column major </param>
        /// <param name="operation"> whether to transpose the matrix a. 'N' no transpose; 'T' transpose matrix a </param>
        /// <param name="m"> number of rows of the matrix a </param>
        /// <param name="n"> number of columns of the matrix a </param>
        /// <param name="nrhs"> number of right-hand sides; number of columns in b </param>
        /// <param name="a"> matrix a (overwritten by the factorization) </param>
        /// <param name="lda"> leading dimension of the matrix a </param>
        /// <param name="b"> vector b (overwritten by the solution vector x) </param>
        /// <param name="ldb"> leading dimension of b </param>
        /// <param name="starta"> starting index in a </param>
        /// <param name="startb"> starting index in a </param>
        public unsafe void Gels(LAPACK_Layout layout, LAPACK_Transpose operation,
            long m, long n, long nrhs,
            DenseArrayBase<double> a, long lda,
            DenseArrayBase<double> b, long ldb,
            long starta = 0, long startb = 0)
        {
            double* pa = (double*)a.DataPtr.ToPointer() + starta;
            double* pb = (double*)b.DataPtr.ToPointer() + startb;
            _ = IntelMKLNative.LAPACKE_dgels((int)layout, (char)operation, m, n, nrhs,
                pa, lda, pb, ldb);
        }

        /// <summary>
        /// solves overdetermined or underdetermined linear least square problem
        /// </summary>
        /// <param name="layout"> matrix storage layout is row or column major </param>
        /// <param name="operation"> whether to transpose the matrix a. 'N' no transpose; 'T' transpose matrix a </param>
        /// <param name="m"> number of rows of the matrix a </param>
        /// <param name="n"> number of columns of the matrix a </param>
        /// <param name="nrhs"> number of right-hand sides; number of columns in b </param>
        /// <param name="a"> matrix a (overwritten by the factorization) </param>
        /// <param name="lda"> leading dimension of the matrix a </param>
        /// <param name="b"> vector b (overwritten by the solution vector x) </param>
        /// <param name="ldb"> leading dimension of b </param>
        /// <param name="starta"> starting index in a </param>
        /// <param name="startb"> starting index in a </param>
        public unsafe void Gels(LAPACK_Layout layout, LAPACK_Transpose operation,
            long m, long n, long nrhs,
            DenseArrayBase<Complex> a, long lda,
            DenseArrayBase<Complex> b, long ldb,
            long starta = 0, long startb = 0)
        {
            Complex* pa = (Complex*)a.DataPtr.ToPointer() + starta;
            Complex* pb = (Complex*)b.DataPtr.ToPointer() + startb;
            _ = IntelMKLNative.LAPACKE_zgels((int)layout, (char)operation, m, n, nrhs,
                pa, lda, pb, ldb);
        }

        #endregion
        #region --------- Geev ---------

        /// <summary>
        /// Computes the eigenvalues and left and right eigenvectors of a general matrix
        /// </summary>
        /// <param name="layout"> matrix storage layout is row or column major </param>
        /// <param name="jobvl"> 'N' left eigenvectors not computed; 'V' left eigenvectors computed </param>
        /// <param name="jobvr"> 'N' right eigenvectors not computed; 'V' right eigenvectors computed </param>
        /// <param name="n"> order of matrix a </param>
        /// <param name="a"> matrix a (overwritten on exit) </param>
        /// <param name="lda"> leading dimension of a </param>
        /// <param name="wr"> real part of the computed eigenvalues </param>
        /// <param name="wi"> imaginary part of the computed eigenvalues </param>
        /// <param name="vl"> left eigenvectors </param>
        /// <param name="ldvl"> leading dimensino of vl </param>
        /// <param name="vr"> right eigenvectors </param>
        /// <param name="ldvr"> leading dimension of vr </param>
        /// <param name="starta"> starting index in a </param>
        /// <param name="startwr"> starting index in wr </param>
        /// <param name="startwi"> starting index in wi </param>
        /// <param name="startvl"> starting index in vl </param>
        /// <param name="startvr"> starting index in vr </param>
        public unsafe void Geev(LAPACK_Layout layout, LAPACK_Job jobvl, LAPACK_Job jobvr, long n,
            DenseArrayBase<double> a, long lda,
            DenseArrayBase<double> wr, DenseArrayBase<double> wi,
            DenseArrayBase<double> vl, long ldvl, DenseArrayBase<double> vr, long ldvr,
            long starta = 0, long startwr = 0, long startwi = 0,
            long startvl = 0, long startvr = 0)
        {
            double* pa = (double*)a.DataPtr.ToPointer() + starta;
            double* pwr = (double*)wr.DataPtr.ToPointer() + startwr;
            double* pwi = (double*)wi.DataPtr.ToPointer() + startwi;
            double* pvl = (double*)vl.DataPtr.ToPointer() + startvl;
            double* pvr = (double*)vr.DataPtr.ToPointer() + startvr;
            _ = IntelMKLNative.LAPACKE_dgeev((int)layout, (char)jobvl, (char)jobvr, n,
                pa, lda, pwr, pwi, pvl, ldvl, pvr, ldvr);
        }

        /// <summary>
        /// Computes the eigenvalues and left and right eigenvectors of a general matrix
        /// </summary>
        /// <param name="layout"> matrix storage layout is row or column major </param>
        /// <param name="jobvl"> 'N' left eigenvectors not computed; 'V' left eigenvectors computed </param>
        /// <param name="jobvr"> 'N' right eigenvectors not computed; 'V' right eigenvectors computed </param>
        /// <param name="n"> order of matrix a </param>
        /// <param name="a"> matrix a (overwritten on exit) </param>
        /// <param name="lda"> leading dimension of a </param>
        /// <param name="w"> eigenvalues </param>
        /// <param name="vl"> left eigenvectors </param>
        /// <param name="ldvl"> leading dimensino of vl </param>
        /// <param name="vr"> right eigenvectors </param>
        /// <param name="ldvr"> leading dimension of vr </param>
        /// <param name="starta"> starting index in a </param>
        /// <param name="startw"> starting index in wr </param>
        /// <param name="startvl"> starting index in vl </param>
        /// <param name="startvr"> starting index in vr </param>
        public unsafe void Geev(LAPACK_Layout layout, LAPACK_Job jobvl, LAPACK_Job jobvr, long n,
            DenseArrayBase<Complex> a, long lda,
            DenseArrayBase<Complex> w,
            DenseArrayBase<Complex> vl, long ldvl, DenseArrayBase<Complex> vr, long ldvr,
            long starta = 0, long startw = 0, 
            long startvl = 0, long startvr = 0)
        {
            Complex* pa = (Complex*)a.DataPtr.ToPointer() + starta;
            Complex* pw = (Complex*)w.DataPtr.ToPointer() + startw;
            Complex* pvl = (Complex*)vl.DataPtr.ToPointer() + startvl;
            Complex* pvr = (Complex*)vr.DataPtr.ToPointer() + startvr;
            _ = IntelMKLNative.LAPACKE_zgeev((int)layout, (char)jobvl, (char)jobvr, n,
                pa, lda, pw, pvl, ldvl, pvr, ldvr);
        }

        #endregion
        #region --------- Ggev ---------

        /// <summary>
        /// Computes the generalized eigenvalues and left and right eigenvectors of general matrixs
        /// </summary>
        /// <param name="layout"> matrix storage layout is row or column major </param>
        /// <param name="jobvl"> 'N' left eigenvectors not computed; 'V' left eigenvectors computed </param>
        /// <param name="jobvr"> 'N' right eigenvectors not computed; 'V' right eigenvectors computed </param>
        /// <param name="n"> order of matrix a </param>
        /// <param name="a"> matrix a (overwritten on exit) </param>
        /// <param name="lda"> leading dimension of a </param>
        /// <param name="b"> matrix b (overwritten on exit) </param>
        /// <param name="ldb"> leading dimension of b </param>
        /// <param name="alphar"> real part of a part of the computed eigenvalues => alpha/beta = lambda </param>
        /// <param name="alphai"> imaginary part of a part of the computed eigenvalues => alpha/beta = lambda </param>
        /// <param name="beta"> part of the computed eigenvalues => alpha/beta = lambda </param>
        /// <param name="vl"> left eigenvectors </param>
        /// <param name="ldvl"> leading dimensino of vl </param>
        /// <param name="vr"> right eigenvectors </param>
        /// <param name="ldvr"> leading dimension of vr </param>
        /// <param name="starta"> starting index in a </param>
        /// <param name="startb"> starting index in b </param>
        /// <param name="startalphar"> starting index in alphar </param>
        /// <param name="startalphai"> starting index in alphai </param>
        /// <param name="startbeta"> starting index in beta </param>
        /// <param name="startvl"> starting index in vl </param>
        /// <param name="startvr"> starting index in vr </param>
        public unsafe void Ggev(LAPACK_Layout layout, LAPACK_Job jobvl, LAPACK_Job jobvr, long n,
            DenseArrayBase<double> a, long lda,
            DenseArrayBase<double> b, long ldb,
            DenseArrayBase<double> alphar, DenseArrayBase<double> alphai, DenseArrayBase<double> beta,
            DenseArrayBase<double> vl, long ldvl, DenseArrayBase<double> vr, long ldvr,
            long starta = 0, long startb = 0,
            long startalphar = 0, long startalphai = 0, long startbeta = 0,
            long startvl = 0, long startvr = 0)
        {
            double* pa = (double*)a.DataPtr.ToPointer() + starta;
            double* pb = (double*)b.DataPtr.ToPointer() + startb;
            double* palphar = (double*)alphar.DataPtr.ToPointer() + startalphar;
            double* palphai = (double*)alphai.DataPtr.ToPointer() + startalphai;
            double* pbeta = (double*)beta.DataPtr.ToPointer() + startbeta;
            double* pvl = (double*)vl.DataPtr.ToPointer() + startvl;
            double* pvr = (double*)vr.DataPtr.ToPointer() + startvr;
            _ = IntelMKLNative.LAPACKE_dggev((int)layout, (char)jobvl, (char)jobvr, n,
                pa, lda, pb, ldb, palphar, palphai, pbeta, pvl, ldvl, pvr, ldvr);
        }

        /// <summary>
        /// Computes the generalized eigenvalues and left and right eigenvectors of general matrixs
        /// </summary>
        /// <param name="layout"> matrix storage layout is row or column major </param>
        /// <param name="jobvl"> 'N' left eigenvectors not computed; 'V' left eigenvectors computed </param>
        /// <param name="jobvr"> 'N' right eigenvectors not computed; 'V' right eigenvectors computed </param>
        /// <param name="n"> order of matrix a </param>
        /// <param name="a"> matrix a (overwritten on exit) </param>
        /// <param name="lda"> leading dimension of a </param>
        /// <param name="b"> matrix b (overwritten on exit) </param>
        /// <param name="ldb"> leading dimension of b </param>
        /// <param name="alpha">part of the computed eigenvalues => alpha/beta = lambda </param>
        /// <param name="beta"> part of the computed eigenvalues => alpha/beta = lambda </param>
        /// <param name="vl"> left eigenvectors </param>
        /// <param name="ldvl"> leading dimensino of vl </param>
        /// <param name="vr"> right eigenvectors </param>
        /// <param name="ldvr"> leading dimension of vr </param>
        /// <param name="starta"> starting index in a </param>
        /// <param name="startb"> starting index in b </param>
        /// <param name="startalpha"> starting index in alpha </param>
        /// <param name="startbeta"> starting index in beta </param>
        /// <param name="startvl"> starting index in vl </param>
        /// <param name="startvr"> starting index in vr </param>
        public unsafe void Ggev(LAPACK_Layout layout, LAPACK_Job jobvl, LAPACK_Job jobvr, long n,
            DenseArrayBase<Complex> a, long lda,
            DenseArrayBase<Complex> b, long ldb,
            DenseArrayBase<Complex> alpha, DenseArrayBase<Complex> beta,
            DenseArrayBase<Complex> vl, long ldvl, DenseArrayBase<Complex> vr, long ldvr,
            long starta = 0, long startb = 0,
            long startalpha = 0, long startbeta = 0,
            long startvl = 0, long startvr = 0)
        {
            Complex* pa = (Complex*)a.DataPtr.ToPointer() + starta;
            Complex* pb = (Complex*)b.DataPtr.ToPointer() + startb;
            Complex* palpha = (Complex*)alpha.DataPtr.ToPointer() + startalpha;
            Complex* pbeta = (Complex*)beta.DataPtr.ToPointer() + startbeta;
            Complex* pvl = (Complex*)vl.DataPtr.ToPointer() + startvl;
            Complex* pvr = (Complex*)vr.DataPtr.ToPointer() + startvr;
            _ = IntelMKLNative.LAPACKE_zggev((int)layout, (char)jobvl, (char)jobvr, n,
                pa, lda, pb, ldb, palpha, pbeta, pvl, ldvl, pvr, ldvr);
        }

        #endregion
        #region --------- Syev ---------

        /// <summary>
        /// computes all eigenvalues and, optionally, eigenvectors 
        /// of a real symmetric matrix
        /// </summary>
        /// <param name="layout"> matrix storage layout is row or column major </param>
        /// <param name="jobz"> 'N' only eigenvalues computed; 'V' eigenvectors computed too </param>
        /// <param name="uplo"> 'U' stores upper triangular part of a; 'L' stores lower triangular part of a </param>
        /// <param name="n"> order of matrix a </param>
        /// <param name="a"> real symmetric matrix a, either the upper or lower triangular part (overwritten on exit) </param>
        /// <param name="lda"> leading dimension of a </param>
        /// <param name="w"> computed eigenvalues </param>
        /// <param name="starta"> starting index in a </param>
        /// <param name="startw"> starting index in wr </param>
        public unsafe void Syev(LAPACK_Layout layout, LAPACK_Job jobz, char uplo, long n,
            DenseArrayBase<double> a, long lda, DenseArrayBase<double> w,
            long starta = 0, long startw = 0)
        {
            double* pa = (double*)a.DataPtr.ToPointer() + starta;
            double* pw = (double*)w.DataPtr.ToPointer() + startw;
            _ = IntelMKLNative.LAPACKE_dsyev((int)layout, (char)jobz, uplo, n, pa, lda, pw);
        }

        #endregion
        #region --------- Heev ---------

        /// <summary>
        /// computes all eigenvalues and, optionally, eigenvectors 
        /// eigenvectors of a Hermitian matrix
        /// </summary>
        /// <param name="layout"> matrix storage layout is row or column major </param>
        /// <param name="jobz"> 'N' only eigenvalues computed; 'V' eigenvectors computed too </param>
        /// <param name="uplo"> 'U' stores upper triangular part of a; 'L' stores lower triangular part of a </param>
        /// <param name="n"> order of matrix a </param>
        /// <param name="a"> Hermitian matrix a, either the upper or lower triangular part (overwritten on exit) </param>
        /// <param name="lda"> leading dimension of a </param>
        /// <param name="w"> computed eigenvalues </param>
        /// <param name="starta"> starting index in a </param>
        /// <param name="startw"> starting index in wr </param>
        public unsafe void Heev(LAPACK_Layout layout, LAPACK_Job jobz, char uplo, long n,
            DenseArrayBase<Complex> a, long lda, DenseArrayBase<double> w,
            long starta = 0, long startw = 0)
        {
            Complex* pa = (Complex*)a.DataPtr.ToPointer() + starta;
            double* pw = (double*)w.DataPtr.ToPointer() + startw;
            _ = IntelMKLNative.LAPACKE_zheev((int)layout, (char)jobz, uplo, n, pa, lda, pw);
        }

        #endregion
        #region --------- Gesvd ---------

        /// <summary>
        /// computes the singular value decomposition of a general rectangular matrix
        /// </summary>
        /// <param name="layout"> matrix storage layout is row or column major </param>
        /// <param name="jobu"> ... to be added </param>
        /// <param name="jobvt"> ... to be added </param>
        /// <param name="m"> number of rows of matrix a </param>
        /// <param name="n"> number of columns of matrix a </param>
        /// <param name="a"> matrix a </param>
        /// <param name="lda"> leading dimension of matrix a </param>
        /// <param name="s"> singular values, sorted so that s[i] >= s[i+1] </param>
        /// <param name="u"> ... to be added </param>
        /// <param name="ldu"> leading dimension of u </param>
        /// <param name="vt"> ... to be added </param>
        /// <param name="ldvt"> leading dimension of vt </param>
        /// <param name="superb"> unconverged superdiagonal elements </param>
        public unsafe void Gesvd(LAPACK_Layout layout, LAPACK_Job jobu, LAPACK_Job jobvt,
            long m, long n, 
            DenseArrayBase<double> a, long lda, DenseArrayBase<double> s,
            DenseArrayBase<double> u, long ldu,
            DenseArrayBase<double> vt, long ldvt, DenseArrayBase<double> superb)
        {
            double* pa = (double*)a.DataPtr.ToPointer();
            double* ps = (double*)s.DataPtr.ToPointer();
            double* pu = (double*)u.DataPtr.ToPointer();
            double* pvt = (double*)vt.DataPtr.ToPointer();
            double* psuperb = (double*)superb.DataPtr.ToPointer();
            _ = IntelMKLNative.LAPACKE_dgesvd((int)layout, (char)jobu, (char)jobvt,
                m, n, pa, lda, ps, pu, ldu, pvt, ldvt, psuperb);
        }

        /// <summary>
        /// computes the singular value decomposition of a general rectangular matrix
        /// </summary>
        /// <param name="layout"> matrix storage layout is row or column major </param>
        /// <param name="jobu"> ... to be added </param>
        /// <param name="jobvt"> ... to be added </param>
        /// <param name="m"> number of rows of matrix a </param>
        /// <param name="n"> number of columns of matrix a </param>
        /// <param name="a"> matrix a </param>
        /// <param name="lda"> leading dimension of matrix a </param>
        /// <param name="s"> singular values, sorted so that s[i] >= s[i+1] </param>
        /// <param name="u"> ... to be added </param>
        /// <param name="ldu"> leading dimension of u </param>
        /// <param name="vt"> ... to be added </param>
        /// <param name="ldvt"> leading dimension of vt </param>
        /// <param name="superb"> unconverged superdiagonal elements </param>
        public unsafe void Gesvd(LAPACK_Layout layout, LAPACK_Job jobu, LAPACK_Job jobvt,
            long m, long n, 
            DenseArrayBase<Complex> a, long lda, DenseArrayBase<double> s,
            DenseArrayBase<Complex> u, long ldu,
            DenseArrayBase<Complex> vt, long ldvt, DenseArrayBase<double> superb)
        {
            Complex* pa = (Complex*)a.DataPtr.ToPointer();
            double* ps = (double*)s.DataPtr.ToPointer();
            Complex* pu = (Complex*)u.DataPtr.ToPointer();
            Complex* pvt = (Complex*)vt.DataPtr.ToPointer();
            double* psuperb = (double*)superb.DataPtr.ToPointer();
            _ = IntelMKLNative.LAPACKE_zgesvd((int)layout, (char)jobu, (char)jobvt,
                m, n, pa, lda, ps, pu, ldu, pvt, ldvt, psuperb);
        }

        #endregion

        #endregion
        #region DFTI

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
            DenseArrayBase<Complex> x)
        {
            Complex* px = (Complex*)x.DataPtr.ToPointer();
            return IntelMKLNative.DftiComputeForward(desc, px);
        }

        /// <summary>
		/// DftiComputeForward wrapper
		/// </summary>
		/// <param name="desc"> DFTI descriptor </param>
        /// <param name="x_in"> input array data </param>
        /// <param name="x_out"> output array data </param>
        /// <returns> error information </returns>
        public unsafe int DftiComputeForward(IntPtr desc,
            DenseArrayBase<Complex> x_in, DenseArrayBase<Complex> x_out)
        {
            Complex* pin = (Complex*)x_in.DataPtr.ToPointer();
            Complex* pout = (Complex*)x_out.DataPtr.ToPointer();
            return IntelMKLNative.DftiComputeForward(desc, pin, pout);
        }

        /// <summary>
		/// DftiComputeBackward wrapper
		/// </summary>
		/// <param name="desc"> DFTI descriptor </param>
        /// <param name="x"> array data x (in / out) </param>
        /// <returns> error information </returns>
        public unsafe int DftiComputeBackward(IntPtr desc,
            DenseArrayBase<Complex> x)
        {
            Complex* px = (Complex*)x.DataPtr.ToPointer();
            return IntelMKLNative.DftiComputeBackward(desc, px);
        }

        /// <summary>
		/// DftiComputeBackward wrapper
		/// </summary>
		/// <param name="desc"> DFTI descriptor </param>
        /// <param name="x_in"> input array data </param>
        /// <param name="x_out"> output array data </param>
        /// <returns> error information </returns>
        public unsafe int DftiComputeBackward(IntPtr desc,
            DenseArrayBase<Complex> x_in, DenseArrayBase<Complex> x_out)
        {
            Complex* pin = (Complex*)x_in.DataPtr.ToPointer();
            Complex* pout = (Complex*)x_out.DataPtr.ToPointer();
            return IntelMKLNative.DftiComputeBackward(desc, pin, pout);
        }

        /// <summary>
		/// DftiFreeDescriptor wrapper
		/// </summary>
		/// <param name="desc"> DFTI descriptor </param>
		/// <returns> error information </returns>
        public int DftiFreeDescriptor(ref IntPtr desc)
            => IntelMKLNative.DftiFreeDescriptor(ref desc);

        #endregion
        #region VSL

        #region --------- Random ---------
        #region stream
        /// <summary>
        /// creates and initializes a random stream
        /// </summary>
        /// <param name="stream"> pointer to the stream </param>
        /// <param name="brng"> index of the basic generator to initialize the stream </param>
        /// <param name="seed"> innitial condition of the stream </param>
        /// <returns> VSL status </returns>
        public int NewStream(ref IntPtr stream, VSLBRNG brng, ulong seed)
            => IntelMKLNative.vslNewStream(ref stream, (int)brng, seed);

        /// <summary>
        /// deletes a random stream
        /// </summary>
        /// <param name="stream"> pointer to the stream </param>
        /// <returns> VSL status </returns>
        public int DeleteStream(ref IntPtr stream)
            => IntelMKLNative.vslDeleteStream(ref stream);
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
        public unsafe int RngUniform(VSLRNGMethod method, IntPtr stream,
            long n, DenseArrayBase<double> r, double a, double b)
        {
            double* pr = (double*)r.DataPtr.ToPointer();
            return IntelMKLNative.vdRngUniform((int)method, stream,
                n, pr, a, b);
        }

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
        public unsafe int RngGaussian(VSLRNGMethod method, IntPtr stream,
            long n, DenseArrayBase<double> r, double a, double sigma)
        {
            double* pr = (double*)r.DataPtr.ToPointer();
            return IntelMKLNative.vdRngGaussian((int)method, stream,
                n, pr, a, sigma);
        }

        /// <summary>
        /// generates random numbers from multivariate normal distribution [continuous]
        /// </summary>
        /// <param name="method"> generation method </param>
        /// <param name="stream"> pointer to the stream </param>
        /// <param name="n"> number of random values to be generated </param>
        /// <param name="r"> array of n random numbers </param>
        /// <param name="dimen"> dimension of output random vectors </param>
        /// <param name="mstorage"> matrix storage scheme for lower triangular matrix </param>
        /// <param name="a"> mean vector a </param>
        /// <param name="t"> elements of the lower triangular matrix </param>
        /// <returns> VSL status </returns>
        public unsafe int RngGaussianMV(VSLRNGMethod method, IntPtr stream,
            long n, DenseArrayBase<double> r, long dimen, long mstorage, 
            DenseArrayBase<double> a, DenseArrayBase<double> t)
        {
            double* pr = (double*)r.DataPtr.ToPointer();
            double* pa = (double*)a.DataPtr.ToPointer();
            double* pt = (double*)t.DataPtr.ToPointer();
            return IntelMKLNative.vdRngGaussianMV((int)method, stream,
                n, pr, dimen, mstorage, pa, pt);
        }

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
        public unsafe int RngExponential(VSLRNGMethod method, IntPtr stream,
            long n, DenseArrayBase<double> r, double a, double beta)
        {
            double* pr = (double*)r.DataPtr.ToPointer();
            return IntelMKLNative.vdRngExponential((int)method, stream,
                n, pr, a, beta);
        }

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
        public unsafe int RngLaplace(VSLRNGMethod method, IntPtr stream,
            long n, DenseArrayBase<double> r, double a, double beta)
        {
            double* pr = (double*)r.DataPtr.ToPointer();
            return IntelMKLNative.vdRngLaplace((int)method, stream,
                n, pr, a, beta);
        }

        /// <summary>
        /// generates Weibull distributed random numbers [continuous]
        /// </summary>
        /// <param name="method"> generation method </param>
        /// <param name="stream"> pointer to the stream </param>
        /// <param name="n"> number of random values to be generated </param>
        /// <param name="r"> array of n random numbers </param>
        /// <param name="alpha"> shape </param>
        /// <param name="a"> displacement </param>
        /// <param name="b"> scale factor </param>
        /// <returns> VSL status </returns>
        public unsafe int RngWeibull(VSLRNGMethod method, IntPtr stream,
            long n, DenseArrayBase<double> r, double alpha, double a, double b)
        {
            double* pr = (double*)r.DataPtr.ToPointer();
            return IntelMKLNative.vdRngWeibull((int)method, stream,
                n, pr, alpha, a, b);
        }

        /// <summary>
        /// generates Cauchy distributed random numbers [continuous]
        /// </summary>
        /// <param name="method"> generation method </param>
        /// <param name="stream"> pointer to the stream </param>
        /// <param name="n"> number of random values to be generated </param>
        /// <param name="r"> array of n random numbers </param>
        /// <param name="a"> displacement </param>
        /// <param name="beta"> scale factor </param>
        /// <returns> VSL status </returns>
        public unsafe int RngCauchy(VSLRNGMethod method, IntPtr stream,
            long n, DenseArrayBase<double> r, double a, double beta)
        {
            double* pr = (double*)r.DataPtr.ToPointer();
            return IntelMKLNative.vdRngCauchy((int)method, stream,
                n, pr, a, beta);
        }

        /// <summary>
        /// generates Rayleigh distributed random numbers [continuous]
        /// </summary>
        /// <param name="method"> generation method </param>
        /// <param name="stream"> pointer to the stream </param>
        /// <param name="n"> number of random values to be generated </param>
        /// <param name="r"> array of n random numbers </param>
        /// <param name="a"> displacement </param>
        /// <param name="beta"> scale factor </param>
        /// <returns> VSL status </returns>
        public unsafe int RngRayleigh(VSLRNGMethod method, IntPtr stream,
            long n, DenseArrayBase<double> r, double a, double beta)
        {
            double* pr = (double*)r.DataPtr.ToPointer();
            return IntelMKLNative.vdRngRayleigh((int)method, stream,
                n, pr, a, beta);
        }

        /// <summary>
        /// generates logmormally distributed random numbers [continuous]
        /// </summary>
        /// <param name="method"> generation method </param>
        /// <param name="stream"> pointer to the stream </param>
        /// <param name="n"> number of random values to be generated </param>
        /// <param name="r"> array of n random numbers </param>
        /// <param name="a"> average a of the subject normal distribution </param>
        /// <param name="sigma"> standard deviation </param>
        /// <param name="b"> displacement </param>
        /// <param name="beta"> scale factor </param>
        /// <returns> VSL status </returns>
        public unsafe int RngLognormal(VSLRNGMethod method, IntPtr stream,
            long n, DenseArrayBase<double> r, double a, double sigma, double b, double beta)
        {
            double* pr = (double*)r.DataPtr.ToPointer();
            return IntelMKLNative.vdRngLognormal((int)method, stream,
                n, pr, a, sigma, b, beta);
        }

        /// <summary>
        /// generates Gumbel distributed random numbers [continuous]
        /// </summary>
        /// <param name="method"> generation method </param>
        /// <param name="stream"> pointer to the stream </param>
        /// <param name="n"> number of random values to be generated </param>
        /// <param name="r"> array of n random numbers </param>
        /// <param name="a"> displacement </param>
        /// <param name="beta"> scale factor </param>
        /// <returns> VSL status </returns>
        public unsafe int RngGumbel(VSLRNGMethod method, IntPtr stream,
            long n, DenseArrayBase<double> r, double a, double beta)
        {
            double* pr = (double*)r.DataPtr.ToPointer();
            return IntelMKLNative.vdRngGumbel((int)method, stream,
                n, pr, a, beta);
        }

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
        public unsafe int RngGamma(VSLRNGMethod method, IntPtr stream,
            long n, DenseArrayBase<double> r, double alpha, double a, double beta)
        {
            double* pr = (double*)r.DataPtr.ToPointer();
            return IntelMKLNative.vdRngGamma((int)method, stream,
                n, pr, alpha, a, beta);
        }

        /// <summary>
        /// generates beta distributed random numbers [continuous]
        /// </summary>
        /// <param name="method"> generation method </param>
        /// <param name="stream"> pointer to the stream </param>
        /// <param name="n"> number of random values to be generated </param>
        /// <param name="r"> array of n random numbers </param>
        /// <param name="p"> shape p </param>
        /// <param name="q"> shape q </param>
        /// <param name="a"> displacement </param>
        /// <param name="beta"> scale factor </param>
        /// <returns> VSL status </returns>
        public unsafe int RngBeta(VSLRNGMethod method, IntPtr stream,
            long n, DenseArrayBase<double> r, double p, double q, double a, double beta)
        {
            double* pr = (double*)r.DataPtr.ToPointer();
            return IntelMKLNative.vdRngBeta((int)method, stream,
                n, pr, p, q, a, beta);
        }

        /// <summary>
        /// generates chi-square distributed random numbers [continuous]
        /// </summary>
        /// <param name="method"> generation method </param>
        /// <param name="stream"> pointer to the stream </param>
        /// <param name="n"> number of random values to be generated </param>
        /// <param name="r"> array of n random numbers </param>
        /// <param name="v"> degrees of freedom </param>
        /// <returns> VSL status </returns>
        public unsafe int RngChiSquare(VSLRNGMethod method, IntPtr stream,
            long n, DenseArrayBase<double> r, long v)
        {
            double* pr = (double*)r.DataPtr.ToPointer();
            return IntelMKLNative.vdRngChiSquare((int)method, stream,
                n, pr, v);
        }
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
        public unsafe int RngUniform(VSLRNGMethod method, IntPtr stream,
            long n, long[] r, long a, long b)
        {
            fixed (long* pr = &r[0])
                return IntelMKLNative.viRngUniform((int)method, stream, n, pr, a, b);
        }

        /// <summary>
        /// generates bits of underlying BRNG integer recurrence [discrete]
        /// </summary>
        /// <param name="method"> generation method </param>
        /// <param name="stream"> pointer to the stream </param>
        /// <param name="n"> number of random values to be generated </param>
        /// <param name="r"> array of n random numbers </param>
        /// <returns> VSL status </returns>
        public unsafe int RngUniformBits(VSLRNGMethod method, IntPtr stream,
            long n, ulong[] r)
        {
            fixed (ulong* pr = &r[0])
                return IntelMKLNative.viRngUniformBits((int)method, stream, n, pr);
        }

        /// <summary>
        /// generates uniformly distributed bits in 32-bit chunks [discrete]
        /// </summary>
        /// <param name="method"> generation method </param>
        /// <param name="stream"> pointer to the stream </param>
        /// <param name="n"> number of random values to be generated </param>
        /// <param name="r"> array of n random numbers </param>
        /// <returns> VSL status </returns>
        public unsafe int RngUniformBits32(VSLRNGMethod method, IntPtr stream,
            long n, ulong[] r)
        {
            fixed (ulong* pr = &r[0])
                return IntelMKLNative.viRngUniformBits32((int)method, stream, n, pr);
        }

        /// <summary>
        /// generates uniformly distributed bits in 64-bit chunks [discrete]
        /// </summary>
        /// <param name="method"> generation method </param>
        /// <param name="stream"> pointer to the stream </param>
        /// <param name="n"> number of random values to be generated </param>
        /// <param name="r"> array of n random numbers </param>
        /// <returns> VSL status </returns>
        public unsafe int RngUniformBits64(VSLRNGMethod method, IntPtr stream,
            long n, ulong[] r)
        {
            fixed (ulong* pr = &r[0])
                return IntelMKLNative.viRngUniformBits64((int)method, stream, n, pr);
        }

        /// <summary>
        /// generates Bernoulli distributed random values [discrete]
        /// </summary>
        /// <param name="method"> generation method </param>
        /// <param name="stream"> pointer to the stream </param>
        /// <param name="n"> number of random values to be generated </param>
        /// <param name="r"> array of n random numbers </param>
        /// <param name="p"> success probability of a trial </param>
        /// <returns> VSL status </returns>
        public unsafe int RngBernoulli(VSLRNGMethod method, IntPtr stream,
            long n, long[] r, double p)
        {
            fixed (long* pr = &r[0])
                return IntelMKLNative.viRngBernoulli((int)method, stream, n, pr, p);
        }

        /// <summary>
        /// generates geometrically distributed random values [discrete]
        /// </summary>
        /// <param name="method"> generation method </param>
        /// <param name="stream"> pointer to the stream </param>
        /// <param name="n"> number of random values to be generated </param>
        /// <param name="r"> array of n random numbers </param>
        /// <param name="p"> success probability of a trial </param>
        /// <returns> VSL status </returns>
        public unsafe int RngGeometric(VSLRNGMethod method, IntPtr stream,
            long n, long[] r, double p)
        {
            fixed (long* pr = &r[0])
                return IntelMKLNative.viRngGeometric((int)method, stream, n, pr, p);
        }

        /// <summary>
        /// generates binomially distributed random values [discrete]
        /// </summary>
        /// <param name="method"> generation method </param>
        /// <param name="stream"> pointer to the stream </param>
        /// <param name="n"> number of random values to be generated </param>
        /// <param name="r"> array of n random numbers </param>
        /// <param name="ntrial"> number of independent trials </param>
        /// <param name="p"> success probability of a trial </param>
        /// <returns> VSL status </returns>
        public unsafe int RngBinomial(VSLRNGMethod method, IntPtr stream,
            long n, long[] r, long ntrial, double p)
        {
            fixed (long* pr = &r[0])
                return IntelMKLNative.viRngBinomial((int)method, stream, n, pr, ntrial, p);
        }

        /// <summary>
        /// generates hypergeometrically distributed random values [discrete]
        /// </summary>
        /// <param name="method"> generation method </param>
        /// <param name="stream"> pointer to the stream </param>
        /// <param name="n"> number of random values to be generated </param>
        /// <param name="r"> array of n random numbers </param>
        /// <param name="l"> lot size </param>
        /// <param name="s"> size of sampling without replacement </param>
        /// <param name="m"> number of marked elements </param>
        /// <returns> VSL status </returns>
        public unsafe int RngHypergeometric(VSLRNGMethod method, IntPtr stream,
            long n, long[] r, long l, long s, long m)
        {
            fixed (long* pr = &r[0])
                return IntelMKLNative.viRngHypergeometric((int)method, stream,
                    n, pr, l, s, m);
        }

        /// <summary>
        /// generates Poisson distributed random values [discrete]
        /// </summary>
        /// <param name="method"> generation method </param>
        /// <param name="stream"> pointer to the stream </param>
        /// <param name="n"> number of random values to be generated </param>
        /// <param name="r"> array of n random numbers </param>
        /// <param name="lambda"> distribution parameter </param>
        /// <returns> VSL status </returns>
        public unsafe int RngPoisson(VSLRNGMethod method, IntPtr stream,
            long n, long[] r, double lambda)
        {
            fixed (long* pr = &r[0])
                return IntelMKLNative.viRngPoisson((int)method, stream, n, pr, lambda);
        }

        /// <summary>
        /// generates Poisson distributed random values [discrete]
        /// with varying mean 
        /// </summary>
        /// <param name="method"> generation method </param>
        /// <param name="stream"> pointer to the stream </param>
        /// <param name="n"> number of random values to be generated </param>
        /// <param name="r"> array of n random numbers </param>
        /// <param name="lambda"> array of n distribution parameters </param>
        /// <returns> VSL status </returns>
        public unsafe int RngPoissonV(VSLRNGMethod method, IntPtr stream,
            long n, long[] r, double[] lambda)
        {
            fixed (long* pr = &r[0])
            fixed (double* pl = &lambda[0])
                return IntelMKLNative.viRngPoissonV((int)method, stream, n, pr, pl);
        }

        /// <summary>
        /// generates random values [discrete]
        /// with negative binomial distribution
        /// </summary>
        /// <param name="method"> generation method </param>
        /// <param name="stream"> pointer to the stream </param>
        /// <param name="n"> number of random values to be generated </param>
        /// <param name="r"> array of n random numbers </param>
        /// <param name="a"> the first distribution parameter </param>
        /// <param name="p"> the second distribution parameter </param>
        /// <returns> VSL status </returns>
        public unsafe int RngNegbinomial(VSLRNGMethod method, IntPtr stream,
            long n, long[] r, double a, double p)
        {
            fixed (long* pr = &r[0])
                return IntelMKLNative.viRngNegbinomial((int)method, stream, n, pr, a, p);
        }

        /// <summary>
        /// generates multinomially distributed random values [discrete]
        /// </summary>
        /// <param name="method"> generation method </param>
        /// <param name="stream"> pointer to the stream </param>
        /// <param name="n"> number of random values to be generated </param>
        /// <param name="r"> array of n random numbers </param>
        /// <param name="ntrial"> number of independent trials </param>
        /// <param name="k"> number of possible outcomes </param>
        /// <param name="p"> probability vector of k possible outcomes </param>
        /// <returns> VSL status </returns>
        public unsafe int RngMultinomial(VSLRNGMethod method, IntPtr stream,
            long n, long[] r, long ntrial, long k, double[] p)
        {
            fixed (long* pr = &r[0])
            fixed (double* pp = &p[0])
                return IntelMKLNative.viRngMultinomial((int)method, stream,
                    n, pr, ntrial, k, pp);
        }
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
        public int ConvNewTaskReal(ref IntPtr task,
            ConvMode mode, int dims, long[] xshape, long[] yshape, long[] zshape)
            => IntelMKLNative.vsldConvNewTask(ref task,
                (int)mode, dims, xshape, yshape, zshape);

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
        public int ConvNewTaskComplex(ref IntPtr task,
            ConvMode mode, int dims, long[] xshape, long[] yshape, long[] zshape)
            => IntelMKLNative.vslzConvNewTask(ref task,
                (int)mode, dims, xshape, yshape, zshape);

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
        public int ConvNewTaskReal1D(ref IntPtr task,
            ConvMode mode, long xshape, long yshape, long zshape)
            => IntelMKLNative.vsldConvNewTask1D(ref task,
                (int)mode, xshape, yshape, zshape);

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
        public int ConvNewTaskComplex1D(ref IntPtr task,
            ConvMode mode, long xshape, long yshape, long zshape)
            => IntelMKLNative.vslzConvNewTask1D(ref task,
                (int)mode, xshape, yshape, zshape);

        /// <summary>
        /// changes the value of the parameter mode 
        /// in the convolution task descriptor
        /// </summary>
        /// <param name="task"> pointer to the task descriptor </param>
        /// <param name="mode"> new value of the parameter mode </param>
        /// <returns> VSL status </returns>
        public int ConvSetMode(IntPtr task, ConvMode mode)
            => IntelMKLNative.vslConvSetMode(task, (int)mode);

        /// <summary>
        /// changes the value of the parameter internal_precision 
        /// in the convolution task descriptor
        /// </summary>
        /// <param name="task"> pointer to the task descriptor </param>
        /// <param name="precision"> new value of the parameter internal-precision </param>
        /// <returns> VSL status </returns>
        public int ConvSetInternalPrecision(IntPtr task, ConvPrecision precision)
            => IntelMKLNative.vslConvSetInternalPrecision(task, (int)precision);

        /// <summary>
        /// changes the value of the parameter start 
        /// in the convolution task descriptor
        /// </summary>
        /// <param name="task"> pointer to the task descriptor </param>
        /// <param name="start"> new value of the parameter start </param>
        /// <returns> VSL status </returns>
        public int ConvSetStart(IntPtr task, long[] start)
            => IntelMKLNative.vslConvSetStart(task, start);

        /// <summary>
        /// changes the value of the parameter decimation 
        /// in the convolution task descriptor
        /// </summary>
        /// <param name="task"> pointer to the task descriptor </param>
        /// <param name="decimation"> new value of the parameter decimation </param>
        /// <returns></returns>
        public int ConvSetDecimation(IntPtr task, long[] decimation)
            => IntelMKLNative.vslConvSetDecimation(task, decimation);

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
        public unsafe int ConvExec(IntPtr task, DenseArrayBase<double> x, long[] xstride,
            DenseArrayBase<double> y, long[] ystride,
            DenseArrayBase<double> z, long[] zstride)
        {
            double* px = (double*)x.DataPtr.ToPointer();
            double* py = (double*)y.DataPtr.ToPointer();
            double* pz = (double*)z.DataPtr.ToPointer();
            return IntelMKLNative.vsldConvExec(task, px, xstride, py, ystride, pz, zstride);
        }

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
        public unsafe int ConvExec(IntPtr task, DenseArrayBase<Complex> x, long[] xstride,
            DenseArrayBase<Complex> y, long[] ystride,
            DenseArrayBase<Complex> z, long[] zstride)
        {
            Complex* px = (Complex*)x.DataPtr.ToPointer();
            Complex* py = (Complex*)y.DataPtr.ToPointer();
            Complex* pz = (Complex*)z.DataPtr.ToPointer();
            return IntelMKLNative.vslzConvExec(task, px, xstride, py, ystride, pz, zstride);
        }

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
        public unsafe int ConvExec(IntPtr task, DenseArrayBase<double> x, long xstride,
            DenseArrayBase<double> y, long ystride, DenseArrayBase<double> z, long zstride)
        {
            double* px = (double*)x.DataPtr.ToPointer();
            double* py = (double*)y.DataPtr.ToPointer();
            double* pz = (double*)z.DataPtr.ToPointer();
            return IntelMKLNative.vsldConvExec1D(task, px, xstride, py, ystride, pz, zstride);
        }

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
        public unsafe int ConvExec(IntPtr task, DenseArrayBase<Complex> x, long xstride,
            DenseArrayBase<Complex> y, long ystride, DenseArrayBase<Complex> z, long zstride)
        {
            Complex* px = (Complex*)x.DataPtr.ToPointer();
            Complex* py = (Complex*)y.DataPtr.ToPointer();
            Complex* pz = (Complex*)z.DataPtr.ToPointer();
            return IntelMKLNative.vslzConvExec1D(task, px, xstride, py, ystride, pz, zstride);
        }

        /// <summary>
        /// destroys the task object and frees the memory
        /// </summary>
        /// <param name="task"> pointer to the task descriptor </param>
        /// <returns> VSL status </returns>
        public int ConvDeleteTask(ref IntPtr task)
            => IntelMKLNative.vslConvDeleteTask(ref task);

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
        public int CorrNewTaskReal(ref IntPtr task,
            CorrMode mode, int dims, long[] xshape, long[] yshape, long[] zshape)
            => IntelMKLNative.vsldCorrNewTask(ref task, 
                (int)mode, dims, xshape, yshape, zshape);

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
        public int CorrNewTaskComplex(ref IntPtr task,
            CorrMode mode, int dims, long[] xshape, long[] yshape, long[] zshape)
            => IntelMKLNative.vslzCorrNewTask(ref task,
                (int)mode, dims, xshape, yshape, zshape);

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
        public int CorrNewTaskReal1D(ref IntPtr task,
            CorrMode mode, long xshape, long yshape, long zshape)
            => IntelMKLNative.vsldCorrNewTask1D(ref task,
                (int)mode, xshape, yshape, zshape);

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
        public int CorrNewTaskComplex1D(ref IntPtr task,
            CorrMode mode, long xshape, long yshape, long zshape)
            => IntelMKLNative.vslzCorrNewTask1D(ref task,
                (int)mode, xshape, yshape, zshape);

        /// <summary>
        /// changes the value of the parameter mode 
        /// in the convolution task descriptor
        /// </summary>
        /// <param name="task"> pointer to the task descriptor </param>
        /// <param name="mode"> new value of the parameter mode </param>
        /// <returns> VSL status </returns>
        public int CorrSetMode(IntPtr task, CorrMode mode)
            => IntelMKLNative.vslCorrSetMode(task, (int)mode);

        /// <summary>
        /// changes the value of the parameter internal_precision 
        /// in the convolution task descriptor
        /// </summary>
        /// <param name="task"> pointer to the task descriptor </param>
        /// <param name="precision"> new value of the parameter internal-precision </param>
        /// <returns> VSL status </returns>
        public int CorrSetInternalPrecision(IntPtr task, CorrPrecision precision)
            => IntelMKLNative.vslCorrSetInternalPrecision(task, (int)precision);

        /// <summary>
        /// changes the value of the parameter start 
        /// in the convolution task descriptor
        /// </summary>
        /// <param name="task"> pointer to the task descriptor </param>
        /// <param name="start"> new value of the parameter start </param>
        /// <returns> VSL status </returns>
        public int CorrSetStart(IntPtr task, long[] start)
            => IntelMKLNative.vslCorrSetStart(task, start);

        /// <summary>
        /// changes the value of the parameter decimation 
        /// in the convolution task descriptor
        /// </summary>
        /// <param name="task"> pointer to the task descriptor </param>
        /// <param name="decimation"> new value of the parameter decimation </param>
        /// <returns> VSL status </returns>
        public int CorrSetDecimation(IntPtr task, long[] decimation)
            => IntelMKLNative.vslCorrSetDecimation(task, decimation);

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
        public unsafe int CorrExec(IntPtr task, DenseArrayBase<double> x, long[] xstride,
            DenseArrayBase<double> y, long[] ystride, DenseArrayBase<double> z, long[] zstride)
        {
            double* px = (double*)x.DataPtr.ToPointer();
            double* py = (double*)y.DataPtr.ToPointer();
            double* pz = (double*)z.DataPtr.ToPointer();
            return IntelMKLNative.vsldCorrExec(task, px, xstride, py, ystride, pz, zstride);
        }

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
        public unsafe int CorrExec(IntPtr task, DenseArrayBase<Complex> x, long[] xstride,
            DenseArrayBase<Complex> y, long[] ystride, DenseArrayBase<Complex> z, long[] zstride)
        {
            Complex* px = (Complex*)x.DataPtr.ToPointer();
            Complex* py = (Complex*)y.DataPtr.ToPointer();
            Complex* pz = (Complex*)z.DataPtr.ToPointer();
            return IntelMKLNative.vslzConvExec(task, px, xstride, py, ystride, pz, zstride);
        }

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
        public unsafe int CorrExec(IntPtr task, DenseArrayBase<double> x, long xstride,
            DenseArrayBase<double> y, long ystride, DenseArrayBase<double> z, long zstride)
        {
            double* px = (double*)x.DataPtr.ToPointer();
            double* py = (double*)y.DataPtr.ToPointer();
            double* pz = (double*)z.DataPtr.ToPointer();
            return IntelMKLNative.vsldCorrExec1D(task, px, xstride, py, ystride, pz, zstride);
        }

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
        public unsafe int CorrExec(IntPtr task, DenseArrayBase<Complex> x, long xstride,
            DenseArrayBase<double> y, long ystride, DenseArrayBase<double> z, long zstride)
        {
            Complex* px = (Complex*)x.DataPtr.ToPointer();
            Complex* py = (Complex*)y.DataPtr.ToPointer();
            Complex* pz = (Complex*)z.DataPtr.ToPointer();
            return IntelMKLNative.vslzCorrExec1D(task, px, xstride, py, ystride, pz, zstride);
        }

        /// <summary>
        /// destroys the task object and frees the memory
        /// </summary>
        /// <param name="task"> pointer to the task descriptor </param>
        /// <returns> VSL status </returns>
        public int CorrDeleteTask(ref IntPtr task)
            => IntelMKLNative.vslCorrDeleteTask(ref task);

        #endregion

        #endregion

    }




}
