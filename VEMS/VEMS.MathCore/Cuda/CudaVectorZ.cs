using Complex = System.Numerics.Complex;

namespace VEMS.MathCore
{
    /// <summary>
    /// complex-valued vector on Cuda device
    /// </summary>
    public unsafe class CudaVectorZ : CudaArray<Complex> 
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
        public CudaVectorZ(long count) : base(count) { }

        /// <summary>
        /// constructs a CudaVector from a given
        /// vector x on the host memory space
        /// </summary>
        /// <param name="x"> host vector x </param>
        public CudaVectorZ(VectorZ x) : this(x.Count)
            => SetValues(x);

        #endregion
        #region methods

        /// <summary>
        /// copies a vector x in host memory space 
        /// to the vector in device memory space
        /// </summary>
        /// <param name="x"> host vector x as source </param>
        public void SetValues(VectorZ x)
            => CudaHelper.SetVector(x.Count, sizeof(Complex), x.VPtr, 1, VPtr, 1);

        /// <summary>
        /// copies the vector in device memory space
        /// to a vector y in host memory space
        /// </summary>
        /// <param name="y"> host vector y as the target</param>
        public void GetValues(ref VectorZ y)
            => CudaHelper.GetVector(Count, sizeof(Complex), VPtr, 1, y.VPtr, 1);

        /// <summary>
        /// copies the vector in device memory space
        /// to a vector y in host memory space
        /// </summary>
        /// <returns> result host vector y </returns>
        public VectorZ GetValues()
        {
            VectorZ y = new(Count);
            GetValues(ref y);
            return y;
        }

        /// <summary>
        /// computes the sum of the absolute values of all elements
        /// </summary>
        /// <returns> sum of absolute values </returns>
        public double ASum()
            => CudaVMath.AbsSum(this);

        #endregion
    }
}
