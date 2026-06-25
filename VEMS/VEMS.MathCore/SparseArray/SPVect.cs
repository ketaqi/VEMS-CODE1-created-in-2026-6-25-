using System.Numerics;
using VEMS.MathCore.XTMethods;

namespace VEMS.MathCore
{

    /// <summary>
    /// Represents a sparse vector of elements of type <typeparamref name="T"/>.
    /// Stores only non-zero elements and their indices for memory efficiency.
    /// </summary>
    /// <typeparam name="T">The element type of the vector. Must implement <see cref="INumber{T}"/>.</typeparam>
    public class SPVect<T> : IVect<T> where T : INumber<T>
    {
        #region properties

        /// <summary>
        /// Gets the total number of elements in the vector.
        /// </summary>
        public long Count { get; private set; }

        /// <summary>
        /// Gets the number of non-zero elements in the vector.
        /// </summary>
        public long NzCount { get; private set; }

        /// <summary>
        /// Gets the dense array containing the indices of non-zero elements.
        /// </summary>
        public DenseArray<long> NzIndices { get; private set; }

        /// <summary>
        /// Gets the dense array containing the values of non-zero elements.
        /// </summary>
        public DenseArray<T> NzValues { get; private set; }

        #endregion
        #region indexing

        /// <summary>
        /// Gets or sets the value at the specified index.
        /// </summary>
        /// <param name="i">The index of the element.</param>
        /// <param name="checkBound">Whether to check for index bounds.</param>
        /// <returns>The value at the specified index, or <see cref="T.Zero"/> if not present.</returns>
        /// <exception cref="IndexOutOfRangeException">Thrown if <paramref name="i"/> is out of bounds and <paramref name="checkBound"/> is true.</exception>
        public unsafe T this[long i, bool checkBound = true]
        {
            get
            {
                if (checkBound)
                {
                    if ((ulong)i >= (ulong)Count)
                    { throw new IndexOutOfRangeException(nameof(i)); }
                }

                if (!NaiveMath.BSearchSpan(NzIndices, i, out long idx))
                { return T.Zero; }

                var valuesPtr = (T*)NzValues.VPtr;
                return *(valuesPtr + idx);
            }
            set
            {
                if (checkBound)
                {
                    if ((ulong)i >= (ulong)Count)
                    { throw new IndexOutOfRangeException(nameof(i)); }
                }

                if (!NaiveMath.BSearchSpan(NzIndices, i, out long idx))
                { return; }

                var valuesPtr = (T*)NzValues.VPtr;
                *(valuesPtr + idx) = value;
            }
        }

        /// <summary>
        /// Gets or sets the value at the specified integer index.
        /// </summary>
        /// <param name="i">The index of the element.</param>
        /// <param name="checkBound">Whether to check for index bounds.</param>
        /// <returns>The value at the specified index.</returns>
        public T this[int i, bool checkBound = true]
        {
            get => this[(long)i, checkBound];
            set => this[(long)i, checkBound] = value;
        }

        /// <summary>
        /// Gets or sets the value at the specified <see cref="Index"/>.
        /// </summary>
        /// <param name="i">The index of the element.</param>
        /// <param name="checkBound">Whether to check for index bounds.</param>
        /// <returns>The value at the specified index.</returns>
        public T this[Index i, bool checkBound = true]
        {
            get => this[i.ToLong(Count), checkBound];
            set => this[i.ToLong(Count), checkBound] = value;
        }

        /// <summary>
        /// Gets or sets a subvector specified by a <see cref="LongRange"/>.
        /// </summary>
        /// <param name="rng">The range of indices.</param>
        /// <returns>The subvector for the specified range.</returns>
        /// <exception cref="NotImplementedException">Always thrown.</exception>
        public Vect<T> this[LongRange rng]
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        /// <summary>
        /// Gets or sets a subvector specified by a <see cref="Range"/>.
        /// </summary>
        /// <param name="rng">The range of indices.</param>
        /// <returns>The subvector for the specified range.</returns>
        /// <exception cref="NotImplementedException">Always thrown.</exception>
        public Vect<T> this[Range rng]
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        #endregion
        #region constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SPVect{T}"/> class with zero elements.
        /// </summary>
        internal SPVect()
        {
            Count = 0;
            NzCount = 0;
            NzIndices = new DenseArray<long>();
            NzValues = new DenseArray<T>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SPVect{T}"/> class with the specified size and number of non-zero elements.
        /// </summary>
        /// <param name="n">The total number of elements.</param>
        /// <param name="nnz">The number of non-zero elements.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="nnz"/> is greater than <paramref name="n"/>.</exception>
        public SPVect(long n, long nnz)
        {
            if (nnz > n) { throw new ArgumentException($"Number of non-zero elements must not be larger that the total number of elements"); }

            Count = n;
            NzCount = nnz;

            NzIndices = nnz == 0 ? new DenseArray<long>()
                : new DenseArray<long>(count: nnz, initMode: ArrayInitMode.Malloc);
            NzValues = nnz == 0 ? new DenseArray<T>()
                : new DenseArray<T>(count: nnz, initMode: ArrayInitMode.Malloc);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SPVect{T}"/> class with the specified size, non-zero count, indices, and values.
        /// </summary>
        /// <param name="n">The total number of elements.</param>
        /// <param name="nnz">The number of non-zero elements.</param>
        /// <param name="nzIdx">The indices of non-zero elements.</param>
        /// <param name="nzVal">The values of non-zero elements.</param>
        /// <param name="copyMode">The array copy mode (shallow or deep).</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="nnz"/> is greater than <paramref name="n"/>, or if the counts are inconsistent.</exception>
        public SPVect(long n, long nnz,
            DenseArray<long> nzIdx, DenseArray<T> nzVal,
            ArrayCopyMode copyMode = ArrayCopyMode.Shallow)
        {
            if (nnz > n) { throw new ArgumentException($"Number of non-zero elements must not be larger that the total number of elements"); }
            if (nzIdx.Count != nnz || nzVal.Count != nnz)
            { throw new ArgumentException($"Inconsistent number of non-zero elements"); }

            Count = n;
            NzCount = nnz;

            switch (copyMode)
            {
                case ArrayCopyMode.Shallow:
                    NzIndices = nzIdx;
                    NzValues = nzVal;
                    break;
                case ArrayCopyMode.Deep:
                    NzIndices = new DenseArray<long>(other: nzIdx, copyMode: copyMode);
                    NzValues = new DenseArray<T>(other: nzVal, copyMode: copyMode);
                    break;
                default: goto case ArrayCopyMode.Shallow;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SPVect{T}"/> class using a <see cref="SPVectInfo{T}"/> object.
        /// </summary>
        /// <param name="n">The total number of elements.</param>
        /// <param name="nzInfo">The non-zero element information.</param>
        /// <param name="copyMode">The array copy mode (shallow or deep).</param>
        public SPVect(long n, SPVectInfo<T> nzInfo,
            ArrayCopyMode copyMode = ArrayCopyMode.Shallow)
            : this(n, nzInfo.NzCount, nzInfo.NzIndices, nzInfo.NzValues, copyMode)
        { }

        #endregion
        #region methods

        /// <summary>
        /// Checks if a given index is valid for this vector.
        /// </summary>
        /// <param name="i">The index to check.</param>
        /// <returns><c>true</c> if the index is valid; otherwise, <c>false</c>.</returns>
        private bool IsIndexValid(long i)
            => i >= 0 && i < Count;

        #endregion
        #region dispose (?)

        // ...

        #endregion
    }



    /// <summary>
    /// Holds information about the non-zero elements of a sparse vector.
    /// </summary>
    /// <typeparam name="T">The element type of the vector. Must implement <see cref="INumber{T}"/>.</typeparam>
    public class SPVectInfo<T> where T : INumber<T>
    {
        /// <summary>
        /// Gets or sets the number of non-zero elements.
        /// </summary>
        public long NzCount { get; set; }

        /// <summary>
        /// Gets or sets the dense array containing the indices of non-zero elements.
        /// </summary>
        public DenseArray<long> NzIndices { get; set; }

        /// <summary>
        /// Gets or sets the dense array containing the values of non-zero elements.
        /// </summary>
        public DenseArray<T> NzValues { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SPVectInfo{T}"/> class with the specified indices and values.
        /// </summary>
        /// <param name="nzIndx">The dense array of indices of non-zero elements.</param>
        /// <param name="nzVals">The dense array of values of non-zero elements.</param>
        /// <exception cref="ArgumentException">Thrown if the number of indices does not match the number of values.</exception>
        public SPVectInfo(DenseArray<long> nzIndx, DenseArray<T> nzVals)
        {
            if (nzIndx.Count != nzVals.Count)
            { throw new ArgumentException($"Inconsistent number of non-zero elements"); }

            NzCount = nzIndx.Count;
            NzIndices = nzIndx;
            NzValues = nzVals;
        }
    }



    /// <summary>
    /// Represents a sparse vector of doubles.
    /// </summary>
    internal class SPVectorD : SPVect<double>
    {
        
        internal SPVectorD() 
            : base() { }

        public SPVectorD(long n, long nnz) 
            : base(n, nnz) { }


        public SPVectorD(long n, long nnz,
            DenseArray<long> nzIdx, DenseArray<double> nzVal,
            ArrayCopyMode copyMode = ArrayCopyMode.Shallow)
            : base(n, nnz, nzIdx, nzVal, copyMode) { }


        public SPVectorD(long n, SPVectInfo<double> nzInfo,
            ArrayCopyMode copyMode = ArrayCopyMode.Shallow)
            : base(n, nzInfo, copyMode) { }


        //public VectorD Scatter()
        //{
        //    VectorD y = new VectorD(count: Count, mode: ArrayInitMode.Malloc);
        //    //Sparse
        //}

    }

    /// <summary>
    /// Represents a sparse vector of complex numbers.
    /// </summary>
    internal class SPVectorZ : SPVect<Cplx>
    {

    }

}
