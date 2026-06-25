using System.Numerics;

namespace VEMS.MathCore
{
    /// <summary>
    /// Sparse Vector[T] class
    /// </summary>
    /// <typeparam name="T"> double or complex </typeparam>
    public class SPVector<T> : IVector<T> where T : struct
    {
        #region fields

        /// <summary>
        /// empty vector with ZERO count
        /// </summary>
        public static SPVector<T> Empty = new();

        #endregion
        #region properties

        /// <summary>
        /// number of elements in the vector, including zeros
        /// </summary>
        public long Count { get; set; }

        /// <summary>
        /// number of non-zero elements in the vector
        /// </summary>
        public long NzCount { get; set; }

        /// <summary>
        /// indices of non-zero elements in the vector
        /// </summary>
        //public DenseArrayBase<long> NzIndices { get; set; }
        public DenseArray<long> NzIndices { get; set; }

        /// <summary>
        /// values of non-zero elements in the vector
        /// </summary>
        public DenseArrayBase<T> NzValues { get; set; }

        #endregion
        #region indexing

        /// <summary>
        /// get / set the value of a vector element 
        /// </summary>
        /// <param name="i"> index of the element in the vector including zeros </param>
        /// <param name="checkBound"> whether to check if the index is outside bound </param>
        /// <returns> elemetn value </returns>
        public unsafe T this[long i, bool checkBound = true] 
        {
            get
            {
                bool invalidIndex = checkBound && !IsIndexValid(i);
                if (invalidIndex) { throw new IndexOutOfRangeException(nameof(i)); }

                // check if the index corresponds to non-zero values
                bool findSpan = NaiveMath.BSearchSpan(NzIndices, i, out long idx);
                if (!findSpan) { return default; }
                long idxVal = *((long*)NzIndices.VPtr + idx);
                if (i == idxVal)
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
                { return *((T*)NzValues.VPtr + idx); }
#pragma warning restore CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
                else 
                { return default; }
            }
            set
            {
                bool invalidIndex = checkBound && !IsIndexValid(i);
                if (invalidIndex) { throw new IndexOutOfRangeException(nameof(i)); }

                // check if the index corresponds to non-zero values
                bool findSpan = NaiveMath.BSearchSpan(NzIndices, i, out long idx);
                if (!findSpan) { return; }
                long idxVal = *((long*)NzIndices.VPtr + idx);
                if (i == idxVal)
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
                { *((T*)NzValues.VPtr + idx) = value; }
#pragma warning restore CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
                else 
                { Printer.Error($"cannot set the value of element with zero value"); }

            }
        }

        /// <summary>
        /// get / set the value of a vector element 
        /// </summary>
        /// <param name="i"> index of the element in the vector including zeros </param>
        /// <param name="checkBound"> whether to check if the index is outside bound </param>
        /// <returns> elemetn value </returns>
        public T this[int i, bool checkBound = true] 
        { 
            get => this[(long)i, checkBound]; 
            set => this[(long)i, checkBound] = value; 
        }

        /// <summary>
        /// get / set the value of a vector element 
        /// </summary>
        /// <param name="i"> index of the element in the vector including zeros </param>
        /// <param name="checkBound"> whether to check if the index is outside bound </param>
        /// <returns> elemetn value </returns>
        public T this[Index i, bool checkBound = true] 
        {
            get => this[Converter.IndexToInt(i, Count), checkBound];
            set => this[Converter.IndexToInt(i, Count), checkBound] = value; 
        }

        #endregion
        #region constructors

        /// <summary>
        /// default constructor
        /// </summary>
        internal SPVector() 
        {
            Count = 0;
            NzCount = 0;
            NzIndices = new VectorI();
            NzValues = new DenseArrayBase<T>();
        }

        /// <summary>
        /// constructs a sparse vector with total number 
        /// of elements and non-zero element number
        /// </summary>
        /// <param name="n"> total number of elements including zeros </param>
        /// <param name="nnz"> number of non-zero elements </param>
        public SPVector(long n, long nnz)
        {
            if (nnz > n) { throw new ArgumentException($"Too many non-zero elements"); }
            Count = n;
            NzCount = nnz;
            //NzIndices = nnz == 0 ? new DenseArrayBase<long>()
            //    : new DenseArrayBase<long>(count: nnz, mode: ArrayInitMode.Malloc);
            NzIndices = nnz == 0 ? new VectorI() 
                : new VectorI(count: nnz, initMode: ArrayInitMode.Malloc);
            NzValues = nnz == 0 ? new DenseArrayBase<T>() 
                : new DenseArrayBase<T>(count: nnz, mode: ArrayInitMode.Malloc);
        }

        /// <summary>
        /// constructs a sparse vector with total number 
        /// of elements and non-zero element number
        /// </summary>
        /// <param name="n"> total number of elements including zeros </param>
        /// <param name="nnz"> number of non-zero elements </param>
        /// <param name="nzIdx"> indices of non-zero elements </param>
        /// <param name="nzVal"> values of non-zero elements </param>
        /// <param name="copyMode"> array copy mode option </param>
        public SPVector(long n, long nnz, //DenseArrayBase<long> nzIdx,
            DenseArray<long> nzIdx,
            DenseArrayBase<T> nzVal,
            ArrayCopyMode copyMode = ArrayCopyMode.Shallow)
        {
            if (nnz > n) { throw new ArgumentException($"Too many non-zero elements"); }
            if (nzIdx.Count != nnz || nzVal.Count != nnz)
            { throw new ArgumentException($"Inconsistent number of non-zero elements"); }
            Count = n;
            NzCount = nnz;
            // copy options
            switch (copyMode)
            {
                case ArrayCopyMode.Shallow:
                    NzIndices = nzIdx;
                    NzValues = nzVal;
                    break;
                case ArrayCopyMode.Deep:
                    NzIndices = new(other: nzIdx, copyMode: ArrayCopyMode.Deep);
                    NzValues = new(other: nzVal, copyMode: ArrayCopyMode.Deep);
                    break;
                default: goto case ArrayCopyMode.Shallow;
            }
        }

        /// <summary>
        /// constructs a sparse vector with given non-zero information
        /// </summary>
        /// <param name="n"> total number of vector elements </param>
        /// <param name="nzInfo"> information of non-zero elements </param>
        /// <param name="copyMode"> array copy mode option </param>
        public SPVector(long n, SPVInfo<T> nzInfo,
            ArrayCopyMode copyMode = ArrayCopyMode.Shallow)
            : this(n, nzInfo.NzCount, nzInfo.NzIndices, nzInfo.NzValues, copyMode)
        { }

        #endregion
        #region methods

        /// <summary>
        /// check if a given index is valid
        /// </summary>
        /// <param name="i"> index </param>
        /// <returns> result </returns>
        public bool IsIndexValid(long i)
            => i >= 0 && i < Count;

        #endregion
        #region dispose (?)

        // ...

        #endregion
    }

    /// <summary>
    /// data structure for sparse vector
    /// </summary>
    /// <typeparam name="T"> double or complex </typeparam>
    public class SPVInfo<T> where T : struct
    {
        /// <summary>
        /// number of non-zero elements in the vector
        /// </summary>
        public long NzCount { get; set; }

        /// <summary>
        /// indices of non-zero elements in the vector
        /// </summary>
        //public DenseArrayBase<long> NzIndices { get; set; }
        public DenseArray<long> NzIndices { get; set; }

        /// <summary>
        /// values of non-zero elements in the vector
        /// </summary>
        public DenseArrayBase<T> NzValues { get; set; }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="nzIndx"> indices of non-zero elements </param>
        /// <param name="nzVals"> values of non-zero elements </param>
        //public SPVInfo(DenseArrayBase<long> nzIndx, DenseArrayBase<T> nzVals)
        //{
        //    if(nzIndx.Count != nzVals.Count) 
        //    { throw new ArgumentException($"Inconsistent number of non-zero elements"); }
        //    NzCount = nzIndx.Count;
        //    NzIndices = nzIndx;
        //    NzValues = nzVals;
        //}
        public SPVInfo(DenseArray<long> nzIndx, DenseArrayBase<T> nzVals)
        {
            if (nzIndx.Count != nzVals.Count)
            { throw new ArgumentException($"Inconsistent number of non-zero elements"); }
            NzCount = nzIndx.Count;
            NzIndices = nzIndx;
            NzValues = nzVals;
        }

    }


    /// <summary>
    /// real-valued sparse vector
    /// </summary>
    public class VectorDi : SPVector<double>
    {
        #region constructors

        /// <summary>
        /// constructs a sparse vector with totol number 
        /// of elements and non-zero element number
        /// </summary>
        /// <param name="n"> total number of vector elements </param>
        /// <param name="nnz"> number of non-zero elements </param>
        public VectorDi(long n, long nnz)
            : base(n, nnz) { }

        /// <summary>
        /// constructs a sparse vector with given non-zero data
        /// </summary>
        /// <param name="n"> total number of vector elements </param>
        /// <param name="nnz"> number of non-zero elements </param>
        /// <param name="nzIdx"> indices of non-zero elements </param>
        /// <param name="nzVal"> values of non-zero elements </param>
        /// <param name="copyMode"> array copy mode option </param>
        public VectorDi(long n, long nnz, //DenseArrayBase<long> nzIdx,
            DenseArray<long> nzIdx,
            DenseArrayBase<double> nzVal,
            ArrayCopyMode copyMode = ArrayCopyMode.Shallow)
            : base(n, nnz, nzIdx, nzVal, copyMode) { }

        /// <summary>
        /// constructs a sparse vector with given non-zero information
        /// </summary>
        /// <param name="n"> total number of vector elements </param>
        /// <param name="nzInfo"> information of non-zero elements </param>
        /// <param name="copyMode"> array copy mode option </param>
        public VectorDi(long n, SPVInfo<double> nzInfo,
            ArrayCopyMode copyMode = ArrayCopyMode.Shallow)
            : base(n, nzInfo, copyMode) { }

        /// <summary>
        /// constructs a sparse vector by gathering from a dense vector
        /// </summary>
        /// <param name="y"> input dense vector y </param>
        /// <param name="nzIdx"> non-zero indices in vector y </param>
        /// <param name="copyMode"> array copy mode option </param>
        public VectorDi(VectorD y, VectorI nzIdx,
            ArrayCopyMode copyMode = ArrayCopyMode.Shallow)
            : this(y.Count, nzIdx.Count)
        {
            NzIndices = copyMode switch
            {
                ArrayCopyMode.Shallow => nzIdx,
                //ArrayCopyMode.Deep => new(other: nzIdx, deepCopy: true),
                ArrayCopyMode.Deep => new(nzIdx, ArrayCopyMode.Deep),
                _ => nzIdx
            };
            VectorDi t = this;
            Sparse.Gthr(y, ref t);
        }

        ///// <summary>
        ///// constructs a vector by copying from another
        ///// [deep copy]
        ///// </summary>
        ///// <param name="source"> another vector </param>
        //public VectorDi(VectorDi source)
        //    : base(source, mode)
        //{ }

        #endregion
        #region methods

        /// <summary>
        /// generates a dense vector by scattering the
        /// non-zero element from a sparse vector
        /// </summary>
        /// <returns> dense vector </returns>
        public VectorD Scatter()
        {
            VectorD y = new(count: Count, mode: ArrayInitMode.Calloc);
            //Defaults.ISPBLAS.SctrD(NzCount, this, ref y);
            Sparse.Sctr(this, ref y);
            return y;        
        }



        #endregion
    }

    /// <summary>
    /// complex-valued sparse vector
    /// </summary>
    public class VectorZi : SPVector<Complex>
    {
        #region constructors

        /// <summary>
        /// constructs a sparse vector with totol number 
        /// of elements and non-zero element number
        /// </summary>
        /// <param name="n"> total number of vector elements </param>
        /// <param name="nnz"> number of non-zero elements </param>
        /// </summary>
        public VectorZi(long n, long nnz)
            : base(n, nnz) { }

        /// <summary>
        /// constructs a sparse vector with given non-zero data
        /// </summary>
        /// <param name="n"> total number of vector elements </param>
        /// <param name="nnz"> number of non-zero elements </param>
        /// <param name="nzIdx"> indices of non-zero elements </param>
        /// <param name="nzVal"> values of non-zero elements </param>
        /// <param name="copyMode"> array copy mode option </param>
        public VectorZi(long n, long nnz,
            VectorI nzIdx, DenseArrayBase<Complex> nzVal,
            ArrayCopyMode copyMode = ArrayCopyMode.Shallow)
            : base(n, nnz, nzIdx, nzVal, copyMode) { }

        /// <summary>
        /// constructs a sparse vector with given non-zero information
        /// </summary>
        /// <param name="n"> total number of vector elements </param>
        /// <param name="nzInfo"> information of non-zero elements </param>
        /// <param name="copyMode"> array copy mode option </param>
        public VectorZi(long n, SPVInfo<Complex> nzInfo,
            ArrayCopyMode copyMode = ArrayCopyMode.Shallow)
            : base(n, nzInfo, copyMode) { }

        /// <summary>
        /// constructs a sparse vector by gathering from a dense vector
        /// </summary>
        /// <param name="y"> input dense vector y </param>
        /// <param name="nzIdx"> non-zero indices in vector y </param>
        /// <param name="copyMode"> array copy mode option </param>
        public VectorZi(VectorZ y, VectorI nzIdx,
            ArrayCopyMode copyMode = ArrayCopyMode.Shallow)
            : this(y.Count, nzIdx.Count)
        {
            NzIndices = copyMode switch
            {
                ArrayCopyMode.Shallow => nzIdx,
                //ArrayCopyMode.Deep => new(other: nzIdx, deepCopy: true),
                ArrayCopyMode.Deep => new(nzIdx, ArrayCopyMode.Deep),
                _ => nzIdx
            };
            VectorZi t = this;
            Sparse.Gthr(y, ref t);
        }

        ///// <summary>
        ///// constructs a vector by copying from another
        ///// [deep copy]
        ///// </summary>
        ///// <param name="source"> another vector </param>
        //public VectorZi(VectorZi source)
        //    : base(source, mode)
        //{ }

        #endregion
        #region methods

        /// <summary>
        /// generates a dense vector by scattering the
        /// non-zero element from a sparse vector
        /// </summary>
        /// <returns> dense vector </returns>
        public VectorZ Scatter()
        {
            VectorZ y = new(count: Count, mode: ArrayInitMode.Calloc);
            Defaults.ISPBLAS.SctrZ(NzCount, this, ref y);
            return y;
        }

        #endregion
    }

}
