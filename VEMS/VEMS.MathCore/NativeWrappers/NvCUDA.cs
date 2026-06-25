using System;
using System.Numerics;
using System.Text;

namespace VEMS.MathCore
{
    /// <summary>
    /// nvidia CUDA
    /// </summary>
    public class NvCUDA<T> where T : struct //: IBLAS
    {
        #region constructor

        /// <summary>
        /// constructs a default NvCUDA class
        /// </summary>
        public NvCUDA() { }

        #endregion
        #region BLAS

        #region --------- Asum ---------

        /// <summary>
        /// computes the sum of magnitudes of the elements
        /// </summary>
        /// <param name="n"></param>
        /// <param name="x"></param>
        /// <param name="incx"></param>
        /// <returns></returns>
        public unsafe double Asum(long n, CudaArray<T> x, long incx)
        {
            // create cuBLAS handle
            IntPtr cuHandle = new();
            CuBLASNative.cublasCreate_v2(ref cuHandle);
            // call dasum() method
            double s = 0.0;
            if(typeof(T) == typeof(double))
                CuBLASNative.cublasDasum_v2_64(cuHandle, n, x.VPtr, incx, ref s);
            else if(typeof(T) == typeof(Complex))
                CuBLASNative.cublasDzasum_v2_64(cuHandle, n, x.VPtr, incx, ref s);
            // destroy cuBLAS handle
            CuBLASNative.cublasDestroy_v2(cuHandle);
            return s;
        }

        #endregion

        //public void Axpy(long n, double a, ArrayBase<double> x, ArrayBase<double> y, long incx = 1, long incy = 1, long startx = 0, long starty = 0)
        //{
        //    throw new NotImplementedException();
        //}

        //public void Axpy(long n, Complex a, ArrayBase<Complex> x, ArrayBase<Complex> y, long incx = 1, long incy = 1, long startx = 0, long starty = 0)
        //{
        //    throw new NotImplementedException();
        //}

        //public void Copy(long n, ArrayBase<double> x, ArrayBase<double> y, long incx = 1, long incy = 1, long startx = 0, long starty = 0)
        //{
        //    throw new NotImplementedException();
        //}

        //public void Copy(long n, ArrayBase<Complex> x, ArrayBase<Complex> y, long incx = 1, long incy = 1, long startx = 0, long starty = 0)
        //{
        //    throw new NotImplementedException();
        //}

        //public double Dot(long n, ArrayBase<double> x, ArrayBase<double> y, long incx = 1, long incy = 1, long startx = 0, long starty = 0)
        //{
        //    throw new NotImplementedException();
        //}

        //public Complex Dot(long n, ArrayBase<Complex> x, ArrayBase<Complex> y, long incx = 1, long incy = 1, long startx = 0, long starty = 0)
        //{
        //    throw new NotImplementedException();
        //}

        //public Complex Dotc(long n, ArrayBase<Complex> x, ArrayBase<Complex> y, long incx = 1, long incy = 1, long startx = 0, long starty = 0)
        //{
        //    throw new NotImplementedException();
        //}

        //public void Gemm(BLAS_Layout layout, BLAS_Transpose transa, BLAS_Transpose transb, long m, long n, long k, double alpha, ArrayBase<double> a, long lda, ArrayBase<double> b, long ldb, double beta, ArrayBase<double> c, long ldc, long starta = 0, long startb = 0, long startc = 0)
        //{
        //    throw new NotImplementedException();
        //}

        //public void Gemm(BLAS_Layout layout, BLAS_Transpose transa, BLAS_Transpose transb, long m, long n, long k, Complex alpha, ArrayBase<Complex> a, long lda, ArrayBase<Complex> b, long ldb, Complex beta, ArrayBase<Complex> c, long ldc, long starta = 0, long startb = 0, long startc = 0)
        //{
        //    throw new NotImplementedException();
        //}

        //public void Gemv(BLAS_Layout layout, BLAS_Transpose trans, long m, long n, double alpha, ArrayBase<double> a, long lda, ArrayBase<double> x, double beta, ArrayBase<double> y, long incx = 1, long incy = 1, long starta = 0, long startx = 0, long starty = 0)
        //{
        //    throw new NotImplementedException();
        //}

        //public void Gemv(BLAS_Layout layout, BLAS_Transpose trans, long m, long n, Complex alpha, ArrayBase<Complex> a, long lda, ArrayBase<Complex> x, Complex beta, ArrayBase<Complex> y, long incx = 1, long incy = 1, long starta = 0, long startx = 0, long starty = 0)
        //{
        //    throw new NotImplementedException();
        //}

        //public long Iamax(long n, ArrayBase<double> x, long incx = 1, long startx = 0)
        //{
        //    throw new NotImplementedException();
        //}

        //public long Iamax(long n, ArrayBase<Complex> x, long incx = 1, long startx = 0)
        //{
        //    throw new NotImplementedException();
        //}

        //public long Iamin(long n, ArrayBase<double> x, long incx = 1, long startx = 0)
        //{
        //    throw new NotImplementedException();
        //}

        //public long Iamin(long n, ArrayBase<Complex> x, long incx = 1, long startx = 0)
        //{
        //    throw new NotImplementedException();
        //}

        //public void ImatCopy(BLAS_Layout layout, BLAS_Transpose operation, long rows, long cols, double alpha, ArrayBase<double> ab, long lda, long ldb, long startab = 0)
        //{
        //    throw new NotImplementedException();
        //}

        //public void ImatCopy(BLAS_Layout layout, BLAS_Transpose operation, long rows, long cols, Complex alpha, ArrayBase<Complex> ab, long lda, long ldb, long startab = 0)
        //{
        //    throw new NotImplementedException();
        //}

        //public double Nrm2(long n, ArrayBase<double> x, long incx = 1, long startx = 0)
        //{
        //    throw new NotImplementedException();
        //}

        //public double Nrm2(long n, ArrayBase<Complex> x, long incx = 1, long startx = 0)
        //{
        //    throw new NotImplementedException();
        //}

        //public void OmatCopy(BLAS_Layout layout, BLAS_Transpose operation, long rows, long cols, double alpha, ArrayBase<double> a, long lda, ArrayBase<double> b, long ldb, long starta = 0, long startb = 0)
        //{
        //    throw new NotImplementedException();
        //}

        //public void OmatCopy(BLAS_Layout layout, BLAS_Transpose operation, long rows, long cols, Complex alpha, ArrayBase<Complex> a, long lda, ArrayBase<Complex> b, long ldb, long starta = 0, long startb = 0)
        //{
        //    throw new NotImplementedException();
        //}

        //public void Rot(long n, ArrayBase<double> x, ArrayBase<double> y, double c, double s, long incx = 1, long incy = 1, long startx = 0, long starty = 0)
        //{
        //    throw new NotImplementedException();
        //}

        //public void Rot(long n, ArrayBase<Complex> x, ArrayBase<Complex> y, double c, double s, long incx = 1, long incy = 1, long startx = 0, long starty = 0)
        //{
        //    throw new NotImplementedException();
        //}

        //public void Scal(long n, double a, ArrayBase<double> x, long incx = 1, long startx = 0)
        //{
        //    throw new NotImplementedException();
        //}

        //public void Scal(long n, double a, ArrayBase<Complex> x, long incx = 1, long startx = 0)
        //{
        //    throw new NotImplementedException();
        //}

        //public void Scal(long n, Complex a, ArrayBase<Complex> x, long incx = 1, long startx = 0)
        //{
        //    throw new NotImplementedException();
        //}

        //public void Swap(long n, ArrayBase<double> x, ArrayBase<double> y, long incx = 1, long incy = 1, long startx = 0, long starty = 0)
        //{
        //    throw new NotImplementedException();
        //}

        //public void Swap(long n, ArrayBase<Complex> x, ArrayBase<Complex> y, long incx = 1, long incy = 1, long startx = 0, long starty = 0)
        //{
        //    throw new NotImplementedException();
        //}



        #endregion
    }

    /// <summary>
    /// cuda
    /// </summary>
    public class CUDA
    {
        /// <summary>
        /// sub-class: cuda Runtime
        /// </summary>
        public static class Runtime
        {
            #region ---- device ----

            /// <summary>
            /// gets the number of devices with compute capability 
            /// greater or equal to 2.0 that are available for execution
            /// </summary>
            /// <param name="count"> number of devices </param>
            /// <returns> result CUDA status </returns>
            public static CUDA_Error GetDeviceCount(ref int count)
                => CudaRTNative.cudaGetDeviceCount(ref count);

            /// <summary>
            /// sets device to be used for GPU executions
            /// </summary>
            /// <param name="device"> device to be used </param>
            /// <returns> result CUDA status </returns>
            public static CUDA_Error SetDevice(int device)
                => CudaRTNative.cudaSetDevice(device);

            /// <summary>
            /// gets which device is currently being used
            /// </summary>
            /// <param name="device"> device that is currently being used </param>
            /// <returns> result CUDA status </returns>
            public static CUDA_Error GetDevice(ref int device)
                => CudaRTNative.cudaGetDevice(ref device);

            /// <summary>
            /// gets the properties of device
            /// </summary>
            /// <param name="prop"> property of the device </param>
            /// <param name="device"> device index </param>
            /// <returns> result CUDA status </returns>
            public static CUDA_Error GetDeviceProp(ref CUDA_DeviceProp prop, int device)
                => CudaRTNative.cudaGetDeviceProperties(ref prop, device);

            /// <summary>
            /// explicitly destroys and cleans up all resources 
            /// associated with the current device in the current process
            /// </summary>
            /// <returns> result CUDA status </returns>
            public static CUDA_Error DeviceReset()
                => CudaRTNative.cudaDeviceReset();

            /// <summary>
            /// blocks until the device has completed all 
            /// preceding requested tasks
            /// </summary>
            /// <returns> result CUDA status </returns>
            public static CUDA_Error DeviceSynchronize()
                => CudaRTNative.cudaDeviceSynchronize();

            /// <summary>
            /// prints the basic information of the selected device
            /// </summary>
            /// <param name="device"> device selected </param>
            public static void PrintDeviceBasicInfo(int device)
            {
                CUDA_DeviceProp prop = new();
                GetDeviceProp(ref prop, device);
                Printer.Write($"device name: {GetDeviceName(prop)}");
                Printer.Write($"- multiprocessors: {prop.multiProcessorCount}");
                //Printer.Write($"- CUDA cores: {prop.}")
                Printer.Write($"- clock rate: {prop.clockRate} [kHz]");
                Printer.Write($"- total memory: {prop.totalGlobalMem / 1E6} [MB]");
                Printer.Write($"- memory clock rate: {prop.memoryClockRate} [kHz]");
                Printer.Write($"- single to double ratio: {prop.singleToDoublePrecisionPerfRatio}");
                Printer.Write($"- compute capability: {prop.major}.{prop.minor}");
            }

            /// <summary>
            /// gets the name of the device from the device properties
            /// </summary>
            /// <param name="prop"> device properties </param>
            /// <returns> name of the device </returns>
            private static string GetDeviceName(CUDA_DeviceProp prop)
                => Encoding.UTF8.GetString(prop.name);

            #endregion
            #region ---- memory ----

            /// <summary>
            /// allocates memory on the device
            /// </summary>
            /// <param name="devPtr"> pointer to allocated device memory </param>
            /// <param name="byteSize"> requested allocation size in bytes </param>
            /// <returns> result CUDA status </returns>
            public static CUDA_Error Malloc(ref IntPtr devPtr, long byteSize)
                => CudaRTNative.cudaMalloc(ref devPtr, byteSize);

            /// <summary>
            /// copies data between host and device
            /// </summary>
            /// <param name="dst"> destination </param>
            /// <param name="src"> source </param>
            /// <param name="byteSize"> size in bytes </param>
            /// <param name="kind"> copy options </param>
            /// <returns> result CUDA status </returns>
            public static CUDA_Error Memcpy(IntPtr dst, IntPtr src,
                long byteSize, CUDA_MemcpyKind kind)
                => CudaRTNative.cudaMemcpy(dst, src, byteSize, kind);

            /// <summary>
            /// frees memory on the device
            /// </summary>
            /// <param name="devPtr"> pointer to allocated device memory </param>
            /// <returns> result CUDA status </returns>
            public static CUDA_Error Free(IntPtr devPtr)
                => CudaRTNative.cudaFree(devPtr);

            #endregion
        }

        /// <summary>
        /// sub-class: cuBLAS 
        /// </summary>
        public static class BLAS
        {
            // ...
        }

        /// <summary>
        /// sub-class: cuFFT
        /// </summary>
        public static class FFT
        {
            /// <summary>
            /// gets the version of cuFFT library
            /// </summary>
            /// <param name="version"> version number </param>
            /// <returns> cuFFT result </returns>
            public static CuFFT_Result GetFFTVersion(ref int version)
                => CuFFTNative.cufftGetVersion(ref version);

            /// <summary>
            /// 
            /// </summary>
            /// <param name="plan"></param>
            /// <param name="nx"></param>
            /// <param name="type"></param>
            /// <param name="batch"></param>
            /// <returns></returns>
            public static CuFFT_Result Plan1D(ref IntPtr plan,
                long nx, CuFFT_Type type, long batch)
                => CuFFTNative.cufftPlan1d(ref plan, nx, type, batch);



        }



    }

}
