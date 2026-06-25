using System.Numerics;
using VEMS.AMathCore.XTMethods;

namespace VEMS.AMathCore
{

    /// <summary>
    /// Represents a sparse vector of elements of type <typeparamref name="T"/>.
    /// Stores only non-zero elements and their indices for memory efficiency.
    /// </summary>
    /// <typeparam name="T">The element type of the vector. Must implement <see cref="INumber{T}"/>.</typeparam>
    public class SPVect<T> : IVect<T> 
        where T : INumber<T>
    {
        #region properties

        /// <summary>
        /// Gets the total number of elements in the vector.
        /// </summary>
        public Int Count { get; private set; }

        /// <summary>
        /// Gets the number of non-zero elements in the vector.
        /// </summary>
        public Int NzCount { get; private set; }

        /// <summary>
        /// Gets the dense array containing the indices of non-zero elements.
        /// </summary>
        public DenseArray<Int> NzIndices { get; private set; }

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
        public unsafe T this[Int i, bool checkBound = true]
        {
            get
            {
                throw new NotImplementedException();
                //if (checkBound)
                //{
                //    if ((ulong)i >= (ulong)Count)
                //    { throw new IndexOutOfRangeException(nameof(i)); }
                //}

                //if (!NaiveMath.BSearchSpan(NzIndices, i, out long idx))
                //{ return T.Zero; }

                //var valuesPtr = (T*)NzValues.VPtr;
                //return *(valuesPtr + idx);
            }
            set
            {
                throw new NotImplementedException();
                //if (checkBound)
                //{
                //    if ((ulong)i >= (ulong)Count)
                //    { throw new IndexOutOfRangeException(nameof(i)); }
                //}

                //if (!NaiveMath.BSearchSpan(NzIndices, i, out long idx))
                //{ return; }

                //var valuesPtr = (T*)NzValues.VPtr;
                //*(valuesPtr + idx) = value;
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
            get => this[(Int)i, checkBound];
            set => this[(Int)i, checkBound] = value;
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
        public SPVect(Int n, Int nnz)
        {
            //if (nnz > n) { throw new ArgumentException($"Number of non-zero elements must not be larger that the total number of elements"); }

            Count = n;
            NzCount = nnz;

            NzIndices = nnz == 0 ? new DenseArray<Int>()
                : new DenseArray<Int>(count: nnz, initMode: ArrayInitMode.Malloc);
            NzValues = nnz == 0 ? new DenseArray<T>()
                : new DenseArray<T>(count: nnz, initMode: ArrayInitMode.Malloc);
        }

        #endregion
        #region methods

        /// <summary>
        /// Checks if a given index is valid for this vector.
        /// </summary>
        /// <param name="i">The index to check.</param>
        /// <returns><c>true</c> if the index is valid; otherwise, <c>false</c>.</returns>
        private bool IsIndexValid(Int i)
            => i >= 0 && i < Count;

        #region ---- creation ----

        /// <summary>
        /// Creates a new sparse vector with the specified size, 
        /// number of non-zero elements, indices, and values.
        /// </summary>
        /// <param name="n">The total number of elements.</param>
        /// <param name="nnz">The number of non-zero elements.</param>
        /// <param name="nzIdx">The indices of non-zero elements.</param>
        /// <param name="nzVal">The values of non-zero elements.</param>
        /// <returns>A new sparse vector instance.</returns>
        public static SPVect<T> Create(Int n, Int nnz,
            DenseArray<Int> nzIdx, DenseArray<T> nzVal)
        {
            SPVect<T> x = new(n, nnz)
            { NzIndices = nzIdx, NzValues = nzVal };
            return x;
        }

        #endregion
        #region ---- gather ----

        /// <summary>
        /// Gathers elements from a dense vector <paramref name="source"/> 
        /// and creates a sparse vector using the indices specified by <paramref name="x.NzIndices"/>.
        /// </summary>
        /// <param name="n">The total number of elements.</param>
        /// <param name="nnz">The number of non-zero elements.</param>
        /// <param name="nzIdx">The indices of non-zero elements.</param>
        /// <param name="source">The dense vector to gather values from.</param>
        /// <returns>A new sparse vector instance.</returns>
        public unsafe static SPVect<T> Gather(long n, long nnz,
            DenseArray<Int> nzIdx, DenseArray<T> source)
        {
            SPVect<T> x = new(n, nnz)
            { NzIndices = nzIdx };

            if (typeof(T) == typeof(Real))
                Defaults.ISPBLAS.Gthr(x.NzCount, source.DPtr,
                    x.NzValues.DPtr, x.NzIndices.TPtr);
            else if (typeof(T) == typeof(Cplx))
                Defaults.ISPBLAS.Gthr(x.NzCount, source.VPtr,
                    x.NzValues.VPtr, x.NzIndices.TPtr);
            else throw new NotSupportedException($"Type {typeof(T)} not supported.");

            return x;
        }

        #endregion
        #region ---- scatter ----


        internal unsafe void Scatter(ref DenseArray<T> y)
        {
            if (y.Count != this.Count)
                throw new ArgumentException($"The dense array count {y.Count} does not match the sparse vector count {this.Count}.");

            if (typeof(T) == typeof(Real))
                Defaults.ISPBLAS.Sctr(NzCount, NzValues.DPtr,
                    NzIndices.TPtr, y.DPtr);
            else if (typeof(T) == typeof(Cplx))
                Defaults.ISPBLAS.Sctr(NzCount, NzValues.VPtr,
                    NzIndices.TPtr, y.VPtr);
            else throw new NotSupportedException($"Type {typeof(T)} not supported.");
        }

        ///// <summary>
        ///// Converts a compressed sparse vector into a full-storage dense vector.
        ///// </summary>
        ///// <returns>The dense vector as result.</returns>
        //public DenseArray<T> Scatter()
        //{
        //    DenseArray<T> y = new(count: Count,
        //        initMode: ArrayInitMode.Malloc);
        //    this.Scatter(ref y);
        //    return y;
        //}

        /// <summary>
        /// Converts a compressed sparse vector into a full-storage dense vector.
        /// </summary>
        /// <returns>The dense vector as result.</returns>
        public Vect<T> Scatter()
        {
            Vect<T> y = new(count: Count, initMode: ArrayInitMode.Malloc);
            DenseArray<T> t = y;
            Scatter(ref t);
            return y;
        }

        #endregion

        #endregion
        #region dispose (?)

        // ...

        #endregion
    }

}
