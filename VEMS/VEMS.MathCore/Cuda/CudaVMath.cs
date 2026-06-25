using System.Net.Sockets;
using Complex = System.Numerics.Complex;


namespace VEMS.MathCore
{

    /// <summary>
    /// collection of vector-math methods 
    /// based on CUDA
    /// </summary>
    public unsafe class CudaVMath
    {

        #region --------- IndexMaxAbs ---------

        #region real-valued

        /// <summary>
        /// finds the (smallest) index of the element 
        /// with the maximum magnitude in the vector x
        /// </summary>
        /// <param name="x"> device vector x </param>
        /// <param name="incx"> increment in vector x </param>
        /// <returns> index of the element with maximum magnitude </returns>
        public static long IndexMaxAbs(CudaVectorD x, 
            long incx = 1)
        {
            IntPtr cuHandle = CudaHelper.CreateCuBLAS();
            long idx = 0;
            CuBLASNative.cublasIdamax_v2_64(cuHandle, x.Count, x.VPtr, incx, ref idx);
            CudaHelper.DestroyCuBLAS(cuHandle);
            return idx;
        }

        #endregion
        #region complex-valued

        /// <summary>
        /// finds the (smallest) index of the element 
        /// with the maximum magnitude in the vector x
        /// </summary>
        /// <param name="x"> device vector x </param>
        /// <param name="incx"> increment in vector x </param>
        /// <returns> index of the element with maximum magnitude </returns>
        public static long IndexMaxAbs(CudaVectorZ x,
            long incx = 1)
        {
            IntPtr cuHandle = CudaHelper.CreateCuBLAS();
            long idx = 0;
            CuBLASNative.cublasIzamax_v2_64(cuHandle, x.Count, x.VPtr, incx, ref idx);
            CudaHelper.DestroyCuBLAS(cuHandle);
            return idx;
        }

        #endregion

        #endregion
        #region --------- AbsSum ---------

        #region real-valued

        /// <summary>
        /// computes the sum of the absolute values 
        /// of the elements in the vector x
        /// </summary>
        /// <param name="x"> device vector x </param>
        /// <param name="incx"> increment in vector x </param>
        /// <returns> sum of absolute values </returns>
        public static double AbsSum(CudaVectorD x, long incx = 1)
        {
            IntPtr cuHandle = CudaHelper.CreateCuBLAS();
            double sum = 0.0;
            CuBLASNative.cublasDasum_v2_64(cuHandle, x.Count, x.VPtr, incx, ref sum);
            CudaHelper.DestroyCuBLAS(cuHandle);
            return sum;
        }

        /// <summary>
        /// computes the sum of the absolute values 
        /// of the elements in the matrix x
        /// </summary>
        /// <param name="x"> device matrix x </param>
        /// <param name="incx"> increment in matrix x </param>
        /// <returns> sum of absolute values </returns>
        public static double AbsSum(CudaMatrixD x, long incx = 1)
        {
            IntPtr cuHandle = CudaHelper.CreateCuBLAS();
            double sum = 0.0;
            CuBLASNative.cublasDasum_v2_64(cuHandle, x.Count, x.VPtr, incx, ref sum);
            CudaHelper.DestroyCuBLAS(cuHandle);
            return sum;
        }

        #endregion
        #region complex-valued

        /// <summary>
        /// computes the sum of the absolute values 
        /// of the elements in the vector x
        /// </summary>
        /// <param name="x"> device vector x </param>
        /// <param name="incx"> increment in vector x </param>
        /// <returns> sum of absolute values </returns>
        public static double AbsSum(CudaVectorZ x, long incx = 1)
        {
            IntPtr cuHandle = CudaHelper.CreateCuBLAS();
            double sum = 0.0;
            CuBLASNative.cublasDzasum_v2_64(cuHandle, x.Count, x.VPtr, incx, ref sum);
            CudaHelper.DestroyCuBLAS(cuHandle);
            return sum;
        }

        /// <summary>
        /// computes the sum of the absolute values
        /// of the elements in the matrix x
        /// </summary>
        /// <param name="x"> device matrix x </param>
        /// <param name="incx"> invrement in matrix x </param>
        /// <returns> sum of absolute values </returns>
        public static double AbsSum(CudaMatrixZ x, long incx = 1)
        {
            IntPtr cuHandle = CudaHelper.CreateCuBLAS();
            double sum = 0.0;
            CuBLASNative.cublasDzasum_v2_64(cuHandle, x.Count, x.VPtr, incx, ref sum);
            CudaHelper.DestroyCuBLAS(cuHandle);
            return sum;
        }

        #endregion

        #endregion
        #region --------- Copy ---------

        #region real-valued

        /// <summary>
        /// copies vector x to vector y
        /// </summary>
        /// <param name="x"> device vector x </param>
        /// <param name="y"> device vector y - to be overwritten </param>
        /// <param name="incx"> increment in vector x </param>
        /// <param name="incy"> increment in vector y </param>
        public static void Copy(CudaVectorD x, ref CudaVectorD y,
            long incx = 1, long incy = 1)
        {
            IntPtr cuHandle = CudaHelper.CreateCuBLAS();
            CuBLASNative.cublasDcopy_v2_64(cuHandle, x.Count, x.VPtr, incx, y.VPtr, incy);
            CudaHelper.DestroyCuBLAS(cuHandle);
        }

        #endregion
        #region complex-valued

        /// <summary>
        /// copies vector x to vector y
        /// </summary>
        /// <param name="x"> device vector x </param>
        /// <param name="y"> device vector y - to be overwritten </param>
        /// <param name="incx"> increment in vector x </param>
        /// <param name="incy"> increment in vector y </param>
        public static void Copy(CudaVectorZ x, ref CudaVectorZ y,
            long incx = 1, long incy = 1)
        {
            IntPtr cuHandle = CudaHelper.CreateCuBLAS();
            CuBLASNative.cublasZcopy_v2_64(cuHandle, x.Count, x.VPtr, incx, y.VPtr, incy);
            CudaHelper.DestroyCuBLAS(cuHandle);
        }

        #endregion

        #endregion
        #region --------- Swap ---------

        #region real-valued

        /// <summary>
        /// given two vectors x and y, returns vector y and x swapped
        /// </summary>
        /// <param name="x"> device vector x </param>
        /// <param name="y"> device vectot y </param>
        /// <param name="incx"> increment in vector x </param>
        /// <param name="incy"> increment in vector y </param>
        public static void Swap(ref VectorD x, ref VectorD y,
            long incx = 1, long incy = 1)
        {
            IntPtr cuHandle = CudaHelper.CreateCuBLAS();
            CuBLASNative.cublasDswap_v2_64(cuHandle, x.Count, x.VPtr, incx, y.VPtr, incy);
            CudaHelper.DestroyCuBLAS(cuHandle);
        }

        #endregion
        #region complex-valued

        /// <summary>
        /// given two vectors x and y, returns vector y and x swapped
        /// </summary>
        /// <param name="x"> device vector x </param>
        /// <param name="y"> device vectot y </param>
        /// <param name="incx"> increment in vector x </param>
        /// <param name="incy"> increment in vector y </param>
        public static void Swap(ref CudaVectorZ x, ref CudaVectorZ y,
            long incx = 1, long incy = 1)
        {
            IntPtr cuHandle = CudaHelper.CreateCuBLAS();
            CuBLASNative.cublasZswap_v2_64(cuHandle, x.Count, x.VPtr, incx, y.VPtr, incy);
            CudaHelper.DestroyCuBLAS(cuHandle);
        }

        #endregion

        #endregion
        #region --------- AddTo ---------

        #region real-valued

        /// <summary>
        /// adds vector x to vector y
        /// y := alpha * x + y
        /// </summary>
        /// <param name="x"> device vector x </param>
        /// <param name="y"> device vector y (to be overwritten) </param>
        /// <param name="alpha"> scalar constant alpha </param>
        /// <param name="incx"> increment in vector x </param>
        /// <param name="incy"> increment in vector y </param>
        public static void AddTo(CudaVectorD x, ref CudaVectorD y,
            double alpha = 1.0, long incx = 1, long incy = 1)
        {
            IntPtr cuHandle = CudaHelper.CreateCuBLAS();
            CuBLASNative.cublasDaxpy_v2_64(cuHandle, x.Count, 
                ref alpha, x.VPtr, incx, y.VPtr, incy);
            CudaHelper.DestroyCuBLAS(cuHandle);
        }

        #endregion
        #region complex-valued

        /// <summary>
        /// adds vector x to vector y
        /// y := alpha * x + y
        /// </summary>
        /// <param name="x"> device vector x </param>
        /// <param name="y"> device vector y (to be overwritten) </param>
        /// <param name="alphaRe"> real-part of scalar constant alpha </param>
        /// <param name="alphaIm"> imag-part of scalar constant alpha </param>
        /// <param name="incx"> increment in vector x </param>
        /// <param name="incy"> increment in vector y </param>
        public static void AddTo(CudaVectorZ x, ref CudaVectorZ y,
            double alphaRe = 1.0, double alphaIm = 0.0, long incx = 1, long incy = 1)
        {
            Complex alpha = new(alphaRe, alphaIm);
            IntPtr cuHandle = CudaHelper.CreateCuBLAS();
            CuBLASNative.cublasZaxpy_v2_64(cuHandle, x.Count,
                ref alpha, x.VPtr, incx, y.VPtr, incy);
            CudaHelper.DestroyCuBLAS(cuHandle);
        }

        #endregion

        #endregion
        #region --------- SubtractFrom ---------

        #region real-valued

        /// <summary>
        /// subtracts vector y from vector x
        /// x := x - alpha * y
        /// </summary>
        /// <param name="x"> device vector x (to be overwritten) </param>
        /// <param name="y"> device vector y </param>
        /// <param name="alpha"> constant scalar alpha </param>
        /// <param name="incx"> increment in vector x </param>
        /// <param name="incy"> increment in vector y </param>
        public static void SubstractFrom(ref CudaVectorD x, CudaVectorD y,
            double alpha = 1.0, long incx = 1, long incy = 1)
            => AddTo(y, ref x, -alpha, incy, incx);

        #endregion
        #region complex-valued

        /// <summary>
        /// subtracts vector y from vector x
        /// x := x - alpha * y
        /// </summary>
        /// <param name="x"> device vector x (to be overwritten) </param>
        /// <param name="y"> device vector y </param>
        /// <param name="alphaRe"> real-part of constant scalar alpha </param>
        /// <param name="alphaIm"> imag-part of constant scalar alpha </param>
        /// <param name="incx"> increment in vector x </param>
        /// <param name="incy"> increment in vector y </param>
        public static void SubstractFrom(ref CudaVectorZ x, CudaVectorZ y,
            double alphaRe = 1.0, double alphaIm = 0.0,
            long incx = 1, long incy = 1)
            => AddTo(y, ref x, -alphaRe, -alphaIm, incy, incx);

        #endregion

        #endregion
        #region --------- ScaleOn ---------

        #region real-valued

        /// <summary>
        /// scales a vector x by constant a
        /// x := a * x
        /// </summary>
        /// <param name="x"> device vector x </param>
        /// <param name="a"> constant scalar a </param>
        /// <param name="incx"> increment in vector x </param>
        public static void ScaleOn(ref CudaVectorD x, double a,
            long incx = 1)
        {
            IntPtr cuHandle = CudaHelper.CreateCuBLAS();
            CuBLASNative.cublasDscal_v2_64(cuHandle, x.Count, ref a, x.VPtr, incx);
            CudaHelper.DestroyCuBLAS(cuHandle);
        }

        /// <summary>
        /// scales a matrix x by constant a
        /// x := a * x
        /// </summary>
        /// <param name="x"> device matrix x </param>
        /// <param name="a"> constant scalar a </param>
        /// <param name="incx"> increment in matrix x </param>
        public static void ScaleOn(ref CudaMatrixD x, double a,
            long incx = 1)
        {
            IntPtr cuHandle = CudaHelper.CreateCuBLAS();
            CuBLASNative.cublasDscal_v2_64(cuHandle, x.Count, ref a, x.VPtr, incx);
            CudaHelper.DestroyCuBLAS(cuHandle);
        }

        #endregion
        #region complex-valued

        /// <summary>
        /// scales a vector x by constant a
        /// x := a * x
        /// </summary>
        /// <param name="x"> device vector x </param>
        /// <param name="a"> constant scalar a </param>
        /// <param name="incx"> increment in vector x </param>
        public static void ScaleOn(ref CudaVectorZ x, Complex a,
            long incx = 1)
        {
            IntPtr cuHandle = CudaHelper.CreateCuBLAS();
            CuBLASNative.cublasZscal_v2_64(cuHandle, x.Count, ref a, x.VPtr, incx);
            CudaHelper.DestroyCuBLAS(cuHandle);
        }

        /// <summary>
        /// scales a vector x by constant a
        /// x := a * x
        /// </summary>
        /// <param name="x"> device vector x </param>
        /// <param name="a"> constant scalar a </param>
        /// <param name="incx"> increment in vector x </param>
        public static void ScaleOn(ref CudaVectorZ x, double a,
            long incx = 1)
        {
            IntPtr cuHandle = CudaHelper.CreateCuBLAS();
            CuBLASNative.cublasZdscal_v2_64(cuHandle, x.Count, ref a, x.VPtr, incx);
            CudaHelper.DestroyCuBLAS(cuHandle);
        }

        #endregion

        #endregion
        #region --------- Norm ---------

        #region real-valued

        /// <summary>
        /// computes the Euclidean norm of a vector
        /// </summary>
        /// <param name="x"> device vector x </param>
        /// <param name="incx"> increment in vector x </param>
        /// <returns> Euclidean norm </returns>
        public static double Norm(CudaVectorD x,
            long incx = 1)
        {
            IntPtr cuHandle = CudaHelper.CreateCuBLAS();
            double nrm2 = 0.0;
            CuBLASNative.cublasDnrm2_v2_64(cuHandle, x.Count, x.VPtr, incx, ref nrm2);
            CudaHelper.DestroyCuBLAS(cuHandle);
            return nrm2;
        }

        #endregion
        #region complex-valued

        /// <summary>
        /// computes the Euclidean norm of a vector
        /// </summary>
        /// <param name="x"> device vector x </param>
        /// <param name="incx"> increment in vector x </param>
        /// <returns> Euclidean norm </returns>
        public static double Norm(CudaVectorZ x,
            long incx = 1)
        {
            IntPtr cuHandle = CudaHelper.CreateCuBLAS();
            double nrm2 = 0.0;
            CuBLASNative.cublasDznrm2_v2_64(cuHandle, x.Count, x.VPtr, incx, ref nrm2);
            CudaHelper.DestroyCuBLAS(cuHandle);
            return nrm2;
        }

        #endregion

        #endregion
        #region --------- Rotation ---------

        #region real-valued

        /// <summary>
        /// performs rotation of points in the plane
        /// xi = c*xi + s*yi
        /// yi = c*yi - s*xi
        /// </summary>
        /// <param name="x"> device vector x (replaced by c*x+s*y) </param>
        /// <param name="y"> device vector y (replaced by c*y-s*x) </param>
        /// <param name="c"> constant scalar c </param>
        /// <param name="s"> constant scalar s </param>
        /// <param name="incx"> increment in vector x </param>
        /// <param name="incy"> increment in vector y </param>
        public static void Rotation(ref CudaVectorD x, ref CudaVectorD y,
            double c, double s,
            long incx = 1, long incy = 1)
        {
            IntPtr cuHandle = CudaHelper.CreateCuBLAS();
            CuBLASNative.cublasDrot_v2_64(cuHandle, x.Count, x.VPtr, incx,
                y.VPtr, incy, ref c, ref s);
            CudaHelper.DestroyCuBLAS(cuHandle);
        }

        #endregion
        #region complex-valued

        /// <summary>
        /// performs rotation of points in the plane
        /// xi = c*xi + s*yi
        /// yi = c*yi - s*xi
        /// </summary>
        /// <param name="x"> device vector x (replaced by c*x+s*y) </param>
        /// <param name="y"> device vector y (replaced by c*y-s*x) </param>
        /// <param name="c"> constant scalar c </param>
        /// <param name="s"> constant scalar s </param>
        /// <param name="incx"> increment in vector x </param>
        /// <param name="incy"> increment in vector y </param>
        public static void Rotation(ref CudaVectorZ x, ref CudaVectorZ y,
            double c, double s,
            long incx = 1, long incy = 1)
        {
            IntPtr cuHandle = CudaHelper.CreateCuBLAS();
            CuBLASNative.cublasZdrot_v2_64(cuHandle, x.Count, x.VPtr, incx,
                y.VPtr, incy, ref c, ref s);
            CudaHelper.DestroyCuBLAS(cuHandle);
        }

        #endregion

        #endregion

    }
}
