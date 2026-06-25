using System.Numerics;

namespace VEMS.MathCore
{
    /// <summary>
    /// collection of linear algebra methods 
    /// based on CUDA
    /// </summary>
    public unsafe class CudaLinAlg
    {

        #region --------- Dot (Vector-Vector) ---------

        #region real-valued

        /// <summary>
        /// computes a vector-vector dot product
        /// </summary>
        /// <param name="x"> input device vector x</param>
        /// <param name="y"> input device vector y </param>
        /// <param name="incx"> increment in vector x </param>
        /// <param name="incy"> increment in vector y </param>
        /// <returns> dot product result </returns>
        public static double Dot(CudaVectorD x, CudaVectorD y,
            long incx = 1, long incy = 1)
        {
            double dot = 0.0;
            IntPtr cuHandle = CudaHelper.CreateCuBLAS();
            CuBLASNative.cublasDdot_v2_64(cuHandle, x.Count, x.VPtr, incx, y.VPtr, incy, ref dot);
            CudaHelper.DestroyCuBLAS(cuHandle);
            return dot;
        }

        #endregion
        #region complex-valued

        /// <summary>
        /// computes a vector-vector dot product
        /// </summary>
        /// <param name="x"> input device vector x </param>
        /// <param name="y"> input device vector y </param>
        /// <param name="incx"> increment in vector x </param>
        /// <param name="incy"> increment in vector y </param>
        /// <returns> dot product result </returns>
        public static Complex Dot(CudaVectorZ x, CudaVectorZ y,
            long incx = 1, long incy = 1)
        {
            Complex dot = 0.0;
            IntPtr cuHandle = CudaHelper.CreateCuBLAS();
            CuBLASNative.cublasZdotu_v2_64(cuHandle, x.Count, x.VPtr, incx, y.VPtr, incy, ref dot);
            CudaHelper.DestroyCuBLAS(cuHandle);
            return dot;
        }

        #endregion

        #endregion
        #region --------- Dot Conjugate (Vector-Vector) ---------

        /// <summary>
        /// computes a dot product of a conjugated 
        /// vector x with another vector y
        /// </summary>
        /// <param name="x"> input device vector x, to be conjugated </param>
        /// <param name="y"> input device vector y </param>
        /// <param name="incx"> increment in vector x </param>
        /// <param name="incy"> increment in vector y </param>
        /// <returns> dot product result </returns>
        public static Complex DotConjugate(CudaVectorZ x, CudaVectorZ y,
            long incx = 1, long incy = 1)
        {
            Complex dot = 0.0;
            IntPtr cuHandle = CudaHelper.CreateCuBLAS();
            CuBLASNative.cublasZdotc_v2_64(cuHandle, x.Count, x.VPtr, incx, y.VPtr, incy, ref dot);
            CudaHelper.DestroyCuBLAS(cuHandle);
            return dot;
        }

        #endregion
        #region --------- Dot (Matrix-Vector) ---------

        #region real-valued

        /// <summary>
        /// computes a matrix-vector product
        /// y: = alpha * a * x + beta * y
        /// </summary>
        /// <param name="a"> device matrix a </param>
        /// <param name="x"> device vector x </param>
        /// <param name="y"> device vector y (overwritten as output) </param>
        /// <param name="alpha"> scalar constant alpha </param>
        /// <param name="incx"> increment in vector x </param>
        /// <param name="beta"> scalar constant beta </param>
        /// <param name="incy"> increment in vector y </param>
        /// <param name="operation"> operation on matrix a </param>
        public static void Dot(CudaMatrixD a, CudaVectorD x, ref CudaVectorD y,
            double alpha = 1.0, long incx = 1, double beta = 0.0, long incy = 1,
            CuBLAS_Operation operation = CuBLAS_Operation.NoTrans)
        {
            IntPtr cuHandle = CudaHelper.CreateCuBLAS();
            CuBLASNative.cublasDgemv_v2_64(cuHandle, operation,
                a.Rows, a.Cols, ref alpha, a.VPtr, a.Rows,
                x.VPtr, incx, ref beta, y.VPtr, incy);
            CudaHelper.DestroyCuBLAS(cuHandle);
        }

        #endregion
        #region complex-valued


        //public static void Dot(CudaMatrixZ a, CudaVectorZ x, ref CudaVectorZ y,
        //    double alphaRe = 1.0, double alphaIm = 0.0, long incx = 1,
        //    double betaRe = 0.0, double betaIm = 0.0, long incy = 1,
        //    CuBLASOperation operation = CuBLASOperation.NoTrans)
        //{

        //}


        #endregion

        #endregion
        #region --------- Dot (Matrix-Matrix) ---------

        #region real-valued

        /// <summary>
        /// computes a matrix-matrix product
        /// c: = alpha * op(a) * op(b) + beta * c
        /// </summary>
        /// <param name="a"> device matrix a </param>
        /// <param name="b"> device matrix b </param>
        /// <param name="c"> device matrix c (overwritten as output) </param>
        /// <param name="alpha"> scalar constant alpha </param>
        /// <param name="beta"> scalar constant bete </param>
        /// <param name="operationA"> operation on matrix a </param>
        /// <param name="operationB"> operation on matrix b </param>
        public static void Dot(CudaMatrixD a, CudaMatrixD b, ref CudaMatrixD c,
            double alpha = 1.0, double beta = 0.0,
            CuBLAS_Operation operationA = CuBLAS_Operation.NoTrans,
            CuBLAS_Operation operationB = CuBLAS_Operation.NoTrans)
        {
            IntPtr cuHandle = CudaHelper.CreateCuBLAS();
            CuBLASNative.cublasDgemm_v2_64(cuHandle,
                operationA, operationB,
                a.Rows, b.Cols, a.Cols, 
                ref alpha, 
                a.VPtr, a.Rows,
                b.VPtr, b.Rows, 
                ref beta, 
                c.VPtr, c.Rows);
            CudaHelper.DestroyCuBLAS(cuHandle);
        }

        #endregion
        #region complex-valued

        // ...

        #endregion

        #endregion

    }
}
