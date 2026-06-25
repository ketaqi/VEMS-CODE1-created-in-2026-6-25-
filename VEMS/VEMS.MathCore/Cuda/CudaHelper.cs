using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using VEMS.MathCore.NativeProviders;

namespace VEMS.MathCore
{

    /// <summary>
    /// CUDA Helper class
    /// </summary>
    public unsafe class CudaHelper
    {

        #region --------- Device Management ---------

        ///// <summary>
        ///// gets the number of CUDA cores of 
        ///// the specific device
        ///// </summary>
        ///// <param name="devIndex"> device index </param>
        ///// <returns> number of CUDA cores </returns>
        //public static int GetDeviceCUDACores(int devIndex)
        //    => CudaRTNative.GetDeviceCUDACores(devIndex);

        #endregion
        #region --------- Thread Management ---------

        ///// <summary>
        ///// exits and clean up from CUDA launches
        ///// </summary>
        //public static void CudaThreadExit()
        //    => CudaRTNative.cudaThreadExit();

        #endregion

        #region --------- Set/Get Data --------- 

        /// <summary>
        /// copies n elements from a vector x in host memory space 
        /// to a vector y in GPU memory space
        /// </summary>
        /// <param name="n"> number of elements </param>
        /// <param name="elemSize"> byte-size of a single element </param>
        /// <param name="x"> pointer to the host vector x </param>
        /// <param name="incx"> increment in x </param>
        /// <param name="y"> pointer to the device vector y </param>
        /// <param name="incy"> increment in y </param>
        public static void SetVector(long n, long elemSize,
            void* x, long incx, void* y, long incy)
            => CuBLASNative.cublasSetVector_64(n, elemSize, x, incx, y, incy);

        /// <summary>
        /// copies n elements from a vector x in GPU memory space 
        /// to a vector y in host memory spac
        /// </summary>
        /// <param name="n"> number of elements </param>
        /// <param name="elemSize"> byte-size of a single element </param>
        /// <param name="x"> pointer to the device vector x </param>
        /// <param name="incx"> increment in x </param>
        /// <param name="y"> pointer to the host vector y </param>
        /// <param name="incy"> increment in y </param>
        public static void GetVector(long n, long elemSize,
            void* x, long incx, void* y, long incy)
            => CuBLASNative.cublasGetVector_64(n, elemSize, x, incx, y, incy);

        /// <summary>
        /// copies a tile of rows x cols elements from 
        /// a matrix a in host memory space 
        /// to a matrix b in GPU memory space
        /// </summary>
        /// <param name="rows"> number of rows </param>
        /// <param name="cols"> number of columns </param>
        /// <param name="elemSize"> byte-size of a single element </param>
        /// <param name="a"> pointer to the host matrix a </param>
        /// <param name="lda"> leading dimension of matrix a </param>
        /// <param name="b"> pointer to the device matrix b </param>
        /// <param name="ldb"> leading dimension of matrix b </param>
        public static void SetMatrix(long rows, long cols, long elemSize,
            void* a, long lda, void* b, long ldb)
            => CuBLASNative.cublasSetMatrix_64(rows, cols, elemSize, a, lda, b, ldb);

        /// <summary>
        /// copies a tile of rows x cols elements from 
        /// a matrix a in GPU memory space 
        /// to a matrix b in host memory space
        /// </summary>
        /// <param name="rows"> number of rows </param>
        /// <param name="cols"> number of columns </param>
        /// <param name="elemSize"> byte-size of a single element </param>
        /// <param name="a"> pointer to the device matrix a </param>
        /// <param name="lda"> leading dimension of matrix a </param>
        /// <param name="b"> pointer to the host matrix b </param>
        /// <param name="ldb"> leading dimension of matrix b </param>
        public static void GetMatrix(long rows, long cols, long elemSize,
            void* a, long lda, void* b, long ldb)
            => CuBLASNative.cublasGetMatrix_64(rows, cols, elemSize, a, lda, b, ldb);

        #endregion
        #region --------- CuBLAS ---------

        /// <summary>
        /// initializes the cuBLAS library and creates 
        /// a handle to an opaque structure
        /// </summary>
        /// <returns> pointer to the handle </returns>
        public static IntPtr CreateCuBLAS()
        {
            IntPtr cuHandle = new();
            CuBLASNative.cublasCreate_v2(ref cuHandle);
            return cuHandle;
        }

        /// <summary>
        /// releases hardware resources used by the 
        /// cuBLAS library
        /// </summary>
        /// <param name="cuHandle"> pointer to the handle </param>
        public static void DestroyCuBLAS(IntPtr cuHandle)
            => CuBLASNative.cublasDestroy_v2(cuHandle);

        /// <summary>
        /// returns the version number of the cuBLAS library
        /// </summary>
        /// <param name="cuHandle"> pointer to the handle </param>
        /// <returns> version number </returns>
        public static int GetCuBLASVersion(IntPtr cuHandle)
        {
            int version = 0;
            CuBLASNative.cublasGetVersion_v2(cuHandle, ref version);
            return version;
        }

        #endregion
    }

}
