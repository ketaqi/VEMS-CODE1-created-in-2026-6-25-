using System.Runtime.InteropServices;

namespace VEMS.MathCore
{

    /// <summary>
    /// real-valued vector on Cuda device
    /// </summary>
    public unsafe class CudaVectorD : CudaArray<double> 
    {
        #region properties

        ///// <summary>
        ///// internal flag whether disposed or not
        ///// </summary>
        //private bool Disposed;

        ///// <summary>
        ///// gets the length of the vector
        ///// </summary>
        //public long Count { get; }

        ///// <summary>
        ///// gest the pointer to the data
        ///// </summary>
        //public IntPtr DataPtr { get; }

        ///// <summary>
        ///// void pointer to the data
        ///// </summary>
        //public void* VPtr => (void*)DataPtr.ToPointer();

        #endregion
        #region constructors

        /// <summary>
        /// constructs an empty CudaVectorD
        /// with given number of elements 
        /// </summary>
        /// <param name="count"> number of elements </param>
        public CudaVectorD(long count) : base(count) { }

        /// <summary>
        /// constructs a CudaVector from a given
        /// vector x in the host memory space
        /// </summary>
        /// <param name="x"> host vector x as the source </param>
        public CudaVectorD(VectorD x) : this(x.Count)
            => SetValues(x);

        #endregion
        #region methods

        /// <summary>
        /// copies a vector x in host memory space 
        /// to the vector in device memory space
        /// </summary>
        /// <param name="x"> host vector x as the source </param>
        public void SetValues(VectorD x)
            => CudaHelper.SetVector(x.Count, sizeof(double), x.VPtr, 1, VPtr, 1);

        /// <summary>
        /// copies the vector in device memory space
        /// to a vector y in host memory space
        /// </summary>
        /// <param name="y"> host vector y as the target </param>
        public void GetValues(ref VectorD y)
            => CudaHelper.GetVector(Count, sizeof(double), VPtr, 1, y.VPtr, 1);

        /// <summary>
        /// copies the vector in device memory space
        /// to a vector y in host memory space
        /// </summary>
        /// <returns> result host vector y </returns>
        public VectorD GetValues()
        {
            VectorD y = new(Count);
            GetValues(ref y);
            return y;
        }

        /// <summary>
        /// computes the sum of the absolute values of all elements
        /// </summary>
        /// <returns> sum of absolute values </returns>
        public double ASum()
            => CudaVMath.AbsSum(this);
        //{
        //    // create cuBLAS handle
        //    IntPtr cuHandle = new();
        //    CuBLASNative.cublasCreate_v2(ref cuHandle);
        //    // call dasum() method
        //    double s = 0;
        //    //CuBLASNative.cublasDasum_v2(cuHandle, (int)Count,
        //    //    (double*)DataPtr.ToPointer(), 1, ref s);
        //    CuBLASNative.cublasDasum_v2_64(cuHandle, Count,
        //        VPtr, 1, ref s);

        //    return s;
        //}

        #endregion
    }

}
