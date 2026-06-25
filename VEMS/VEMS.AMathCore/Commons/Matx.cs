using System.Numerics;
using VEMS.AMathCore.XTMethods;

namespace VEMS.AMathCore
{

    /// <summary>
    /// Represents a dense matrix of elements of type <typeparamref name="T"/>.
    /// Inherits from <see cref="DenseArray{T}"/> and implements <see cref="IMatx{T}"/>.
    /// </summary>
    /// <typeparam name="T">A numeric type implementing <see cref="INumber{T}"/>.</typeparam>
    public class Matx<T> : DenseArray<T>, IMatx<T> 
        where T : INumber<T>
    {
        #region properties

        /// <summary>
        /// Gets or sets the number of rows in the matrix.
        /// </summary>
        public long Rows { get; set; }

        /// <summary>
        /// Gets or sets the number of columns in the matrix.
        /// </summary>
        public long Cols { get; set; }

        #region ---- element ----

        /// <summary>
        /// Gets or sets the element at the specified row and column indices (Int64).
        /// </summary>
        /// <param name="iRow">The zero-based row index.</param>
        /// <param name="iCol">The zero-based column index.</param>
        /// <param name="checkBound">Whether to check bounds before accessing the element.</param>
        /// <returns>The element at the specified position.</returns>
        public unsafe T this[long iRow, long iCol, bool checkBound = true] 
        {
            get
            {
                if (checkBound)
                {
                    if ((ulong)iRow >= (ulong)Rows || (ulong)iCol >= (ulong)Cols)
                    { throw new IndexOutOfRangeException(); }
                }
                long offset = iRow * Cols + iCol;
                return *((T*)VPtr + offset);
            }
            set
            {
                if (checkBound)
                {
                    if ((ulong)iRow >= (ulong)Rows || (ulong)iCol >= (ulong)Cols)
                    { throw new IndexOutOfRangeException(); }
                }
                long offset = iRow * Cols + iCol;
                *((T*)VPtr + offset) = value;
            }
        }

        /// <summary>
        /// Gets or sets the element at the specified row and column indices (Int32).
        /// </summary>
        /// <param name="iRow">The zero-based row index.</param>
        /// <param name="iCol">The zero-based column index.</param>
        /// <param name="checkBound">Whether to check bounds before accessing the element.</param>
        /// <returns>The element at the specified position.</returns>
        public T this[int iRow, int iCol, bool checkBound = true] 
        {
            get => this[(long)iRow, (long)iCol, checkBound]; 
            set => this[(long)iRow, (long)iCol, checkBound] = value; 
        }

        /// <summary>
        /// Gets or sets the element at the specified row and column indices using <see cref="Index"/>.
        /// </summary>
        /// <param name="iRow">The row index as an <see cref="Index"/>.</param>
        /// <param name="iCol">The column index as an <see cref="Index"/>.</param>
        /// <param name="checkBound">Whether to check bounds before accessing the element.</param>
        /// <returns>The element at the specified position.</returns>
        public T this[Index iRow, Index iCol, bool checkBound = true] 
        { 
            get => this[iRow.ToLong(Rows), iCol.ToLong(Cols), checkBound]; 
            set => this[iRow.ToLong(Rows), iCol.ToLong(Cols), checkBound] = value; 
        }

        #endregion
        #region ---- slice ----

        /// <summary>
        /// Gets the range representing all rows in the current context.
        /// </summary>
        public LongRange AllRows => new(start: 0, end: Rows);

        /// <summary>
        /// Gets the range representing all columns in the current context.
        /// </summary>
        public LongRange AllCols => new(start: 0, end: Cols);

        /// <summary>
        /// Gets or sets a vector representing a slice of the specified row, defined by a column range.
        /// </summary>
        /// <param name="iRow">The zero-based row index.</param>
        /// <param name="colRng">The range of columns to include in the slice, as a <see cref="LongRange"/>.</param>
        /// <returns>
        /// A <see cref="Vect{T}"/> containing the elements from the specified row and column range.
        /// </returns>
        /// <exception cref="IndexOutOfRangeException">
        /// Thrown if <paramref name="iRow"/> is not a valid row index or <paramref name="colRng"/> is not a valid column range.
        /// </exception>
        public unsafe Vect<T> this[long iRow, LongRange colRng]
        {
            get
            {
                if (!IsRowIndexValid(iRow)) { throw new IndexOutOfRangeException("Invalid row index"); }
                if (!IsColRangeValid(colRng)) { throw new IndexOutOfRangeException("Invalid range"); }
                // finds the slice
                long n = colRng.End - colRng.Start;
                Vect<T> x = new(count: n, initMode: ArrayInitMode.Malloc);
                // memory copy
                long offset = iRow * Cols + colRng.Start;
                Buffer.MemoryCopy(
                    source: (byte*)VPtr + offset * ElementByteSize,
                    destination: x.VPtr,
                    destinationSizeInBytes: n * ElementByteSize,
                    sourceBytesToCopy: n * ElementByteSize);
                return x;
            }
            set
            {
                if (!IsRowIndexValid(iRow)) { throw new IndexOutOfRangeException("Invalid row index"); }
                if (!IsColRangeValid(colRng)) { throw new IndexOutOfRangeException("Invalid range"); }
                // finds the slice
                long n = colRng.End - colRng.Start;
                // memory copy
                long offset = iRow * Cols + colRng.Start;
                Buffer.MemoryCopy(
                    source: value.VPtr,
                    destination: (byte*)VPtr + offset * ElementByteSize,
                    destinationSizeInBytes: n * ElementByteSize,
                    sourceBytesToCopy: n * ElementByteSize);
            }
        }

        /// <summary>
        /// Gets or sets a vector representing a slice of the specified row, defined by a column range using <see cref="Index"/> and <see cref="Range"/>.
        /// </summary>
        /// <param name="iRow">The row index as an <see cref="Index"/>.</param>
        /// <param name="colRng">The range of columns to include in the slice, as a <see cref="Range"/>.</param>
        /// <returns>
        /// A <see cref="Vect{T}"/> containing the elements from the specified row and column range.
        /// </returns>
        /// <exception cref="IndexOutOfRangeException">
        /// Thrown if <paramref name="iRow"/> is not a valid row index or <paramref name="colRng"/> is not a valid column range.
        /// </exception>
        public Vect<T> this[Index iRow, Range colRng]
        {
            get => this[iRow.ToLong(Rows), new LongRange(colRng, Cols)];
            set => this[iRow.ToLong(Rows), new LongRange(colRng, Cols)] = value;
        }

        /// <summary>
        /// Gets or sets a vector representing a slice of the specified column, defined by a row range.
        /// </summary>
        /// <param name="rowRng">The range of rows to include in the slice, as a <see cref="LongRange"/>.</param>
        /// <param name="iCol">The zero-based column index.</param>
        /// <returns>
        /// A <see cref="Vect{T}"/> containing the elements from the specified column and row range.
        /// </returns>
        /// <exception cref="IndexOutOfRangeException">
        /// Thrown if <paramref name="iCol"/> is not a valid column index or <paramref name="rowRng"/> is not a valid row range.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown in the setter if the input vector length does not match the row range.
        /// </exception>
        public unsafe Vect<T> this[LongRange rowRng, long iCol]
        {
            get
            {
                if (!IsColIndexValid(iCol)) throw new IndexOutOfRangeException("Invalid column index");
                if (!IsRowRangeValid(rowRng)) throw new IndexOutOfRangeException("Invalid row range");
                // finds the slice
                long n = rowRng.End - rowRng.Start;
                Vect<T> x = new(count: n, initMode: ArrayInitMode.Malloc);
                // pointer copy
                T* src = (T*)VPtr + rowRng.Start * Cols + iCol;
                T* dst = (T*)x.VPtr;
                for (long i = 0; i < n; i++)
                {
                    dst[i] = *src;
                    src += Cols;
                }
                return x;
            }
            set
            {
                if (!IsColIndexValid(iCol)) throw new IndexOutOfRangeException("Invalid column index");
                if (!IsRowRangeValid(rowRng)) throw new IndexOutOfRangeException("Invalid row range");
                // finds the slice
                long n = rowRng.End - rowRng.Start;
                if (value.Count != n) throw new ArgumentException("Input vector length does not match row range.");
                // pointer copy
                T* dst = (T*)VPtr + rowRng.Start * Cols + iCol;
                T* src = (T*)value.VPtr;
                for (long i = 0; i < n; i++)
                {
                    *dst = src[i];
                    dst += Cols;
                }
            }
        }

        /// <summary>
        /// Gets or sets a vector representing a slice of the specified column, defined by a row range using <see cref="Range"/> and <see cref="Index"/>.
        /// </summary>
        /// <param name="rowRng">The range of rows to include in the slice, as a <see cref="Range"/>.</param>
        /// <param name="iCol">The column index as an <see cref="Index"/>.</param>
        /// <returns>
        /// A <see cref="Vect{T}"/> containing the elements from the specified column and row range.
        /// </returns>
        /// <exception cref="IndexOutOfRangeException">
        /// Thrown if <paramref name="iCol"/> is not a valid column index or <paramref name="rowRng"/> is not a valid row range.
        /// </exception>
        public Vect<T> this[Range rowRng, Index iCol]
        {
            get => this[new LongRange(rowRng, Rows), iCol.ToLong(Cols)];
            set => this[new LongRange(rowRng, Rows), iCol.ToLong(Cols)] = value;
        }

        #endregion
        #region ---- block ----

        /// <summary>
        /// Gets or sets a submatrix (block) defined by the specified row and column ranges.
        /// </summary>
        /// <param name="rowRng">
        /// The range of rows to include in the block, as a <see cref="LongRange"/>.
        /// The <see cref="LongRange.Start"/> is inclusive, <see cref="LongRange.End"/> is exclusive.
        /// </param>
        /// <param name="colRng">
        /// The range of columns to include in the block, as a <see cref="LongRange"/>.
        /// The <see cref="LongRange.Start"/> is inclusive, <see cref="LongRange.End"/> is exclusive.
        /// </param>
        /// <returns>
        /// A new <see cref="Matx{T}"/> containing the elements from the specified block.
        /// </returns>
        /// <exception cref="IndexOutOfRangeException">
        /// Thrown if <paramref name="rowRng"/> or <paramref name="colRng"/> is not a valid range for this matrix.
        /// </exception>
        public unsafe Matx<T> this[LongRange rowRng, LongRange colRng]
        {
            get
            {
                if (!IsRowRangeValid(rowRng)) throw new IndexOutOfRangeException("Invalid range");
                if (!IsColRangeValid(colRng)) throw new IndexOutOfRangeException("Invalid range");

                long rows = rowRng.End - rowRng.Start;
                long cols = colRng.End - colRng.Start;
                Matx<T> x = new(rows, cols, ArrayInitMode.Malloc);

                T* srcBase = (T*)VPtr + rowRng.Start * Cols + colRng.Start;
                T* dstBase = (T*)x.VPtr;
                long rowBytes = cols * ElementByteSize;

                if (cols == Cols)
                {
                    Buffer.MemoryCopy(
                        source: srcBase,
                        destination: dstBase,
                        destinationSizeInBytes: rows * rowBytes,
                        sourceBytesToCopy: rows * rowBytes);
                }
                else if (cols == 1)
                {
                    // Optimize for single-column block: use pointer arithmetic instead of Buffer.MemoryCopy
                    for (long i = 0; i < rows; i++)
                    { dstBase[i] = srcBase[i * Cols]; }
                }
                else
                {
                    // Copy row by row
                    T* src = srcBase;
                    T* dst = dstBase;
                    for (long iRow = 0; iRow < rows; iRow++)
                    {
                        Buffer.MemoryCopy(
                            source: src,
                            destination: dst,
                            destinationSizeInBytes: rowBytes,
                            sourceBytesToCopy: rowBytes);
                        src += Cols;
                        dst += cols;
                    }
                }
                return x;
            }
            set
            {
                if (!IsRowRangeValid(rowRng)) throw new IndexOutOfRangeException("Invalid range");
                if (!IsColRangeValid(colRng)) throw new IndexOutOfRangeException("Invalid range");

                long rows = rowRng.End - rowRng.Start;
                long cols = colRng.End - colRng.Start;

                T* srcBase = (T*)value.VPtr;
                T* dstBase = (T*)VPtr + rowRng.Start * Cols + colRng.Start;
                long rowBytes = cols * ElementByteSize;

                if (cols == Cols)
                {
                    Buffer.MemoryCopy(
                        source: srcBase,
                        destination: dstBase,
                        destinationSizeInBytes: rows * rowBytes,
                        sourceBytesToCopy: rows * rowBytes);
                }
                else if (cols == 1)
                {
                    // Optimize for single-column block: use pointer arithmetic instead of Buffer.MemoryCopy
                    for (long i = 0; i < rows; i++)
                    { dstBase[i * Cols] = srcBase[i]; }
                }
                else
                {
                    // Copy row by row
                    T* src = srcBase;
                    T* dst = dstBase;
                    for (long iRow = 0; iRow < rows; iRow++)
                    {
                        Buffer.MemoryCopy(
                            source: src,
                            destination: dst,
                            destinationSizeInBytes: rowBytes,
                            sourceBytesToCopy: rowBytes);
                        src += cols;
                        dst += Cols;
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets a submatrix (block) defined by the specified row and column ranges using <see cref="Range"/>.
        /// </summary>
        /// <param name="rowRng">
        /// The range of rows to include in the block, as a <see cref="Range"/>.
        /// The start is inclusive, the end is exclusive.
        /// </param>
        /// <param name="colRng">
        /// The range of columns to include in the block, as a <see cref="Range"/>.
        /// The start is inclusive, the end is exclusive.
        /// </param>
        /// <returns>
        /// A new <see cref="Matx{T}"/> containing the elements from the specified block.
        /// </returns>
        /// <exception cref="IndexOutOfRangeException">
        /// Thrown if <paramref name="rowRng"/> or <paramref name="colRng"/> is not a valid range for this matrix.
        /// </exception>
        public Matx<T> this[Range rowRng, Range colRng]
        {
            get => this[new LongRange(rowRng, Rows), new LongRange(colRng, Cols)];
            set => this[new LongRange(rowRng, Rows), new LongRange(colRng, Cols)] = value;
        }

        #endregion

        #endregion
        #region constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        internal Matx() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Matx{T}"/> class with the specified number of rows and columns.
        /// </summary>
        /// <param name="rows">The number of rows in the matrix.</param>
        /// <param name="cols">The number of columns in the matrix.</param>
        /// <param name="initMode">
        /// The array initialization mode. 
        /// Defaults to <see cref="ArrayInitMode.Calloc"/>, which allocates memory and initializes all values to zero.
        /// </param>
        public Matx(long rows, long cols,
            ArrayInitMode initMode = ArrayInitMode.Calloc)
            : base(count: rows * cols, initMode: initMode)
        {
            Rows = rows;
            Cols = cols;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Matx{T}"/> class by copying the contents of another matrix.
        /// </summary>
        /// <param name="source">The source matrix to copy from.</param>
        /// <param name="copyMode">
        /// The copy mode to use when copying the data. 
        /// Defaults to <see cref="ArrayCopyMode.Deep"/>, which performs a deep copy of the data.
        /// </param>
        public Matx(Matx<T> source,
            ArrayCopyMode copyMode = ArrayCopyMode.Deep)
            : base(source, copyMode)
        {
            Rows = source.Rows;
            Cols = source.Cols;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Matx{T}"/> class by copying from the specified two-dimensional array.
        /// </summary>
        /// <remarks>The <paramref name="copyMode"/> parameter determines whether the matrix will maintain
        /// its own copy of the data or reference the original array. If <see cref="ArrayCopyMode.Shallow"/> is used,
        /// changes to the original array will be reflected in the matrix.</remarks>
        /// <param name="source">A two-dimensional array containing the elements to initialize the matrix.</param>
        /// <param name="copyMode">Specifies the copy mode for the array. Use <see cref="ArrayCopyMode.Deep"/> to create a deep copy of the
        /// array, or <see cref="ArrayCopyMode.Shallow"/> to reference the original array directly.</param>
        public Matx(T[,] source,
            ArrayCopyMode copyMode = ArrayCopyMode.Deep)
            : base(source, copyMode) 
        {
            Rows = source.GetLength(0);
            Cols = source.GetLength(1);
        }

        #endregion
        #region methods

        #region ---- create ----

        /// <summary>
        /// Creates a new matrix with the specified dimensions and initializes all elements to the given value.
        /// </summary>
        /// <remarks>If <paramref name="x0"/> is <see langword="default"/> for <typeparamref name="T"/>,
        /// the matrix is initialized using zero-initialization. Otherwise, all elements are explicitly set to <paramref
        /// name="x0"/>.</remarks>
        /// <param name="rows">The number of rows in the matrix. Must be greater than zero.</param>
        /// <param name="cols">The number of columns in the matrix. Must be greater than zero.</param>
        /// <param name="x0">The value to initialize each element of the matrix. If <see langword="default"/> for <typeparamref
        /// name="T"/>, the matrix will be zero-initialized.</param>
        /// <returns>A new instance of <see cref="Matx{T}"/> with the specified dimensions and initialized values.</returns>
        public static unsafe Matx<T> Create(long rows, long cols, T x0)
        {
            Matx<T> x = new (rows, cols, initMode: x0.Equals(T.Zero) ? 
                ArrayInitMode.Calloc : ArrayInitMode.Malloc);
            if (!x0.Equals(T.Zero))
            {
                T* ptr = (T*)x.VPtr;
                for (long i = 0; i < x.Count; i++)
                { ptr[i] = x0; }
            }
            return x;
        }

        /// <summary>
        /// Creates a matrix with the specified dimensions, initializing its elements to a sequence of values.
        /// </summary>
        /// <remarks>The matrix is initialized such that the first element is <paramref name="x0"/>, and
        /// each subsequent element is incremented by <paramref name="dx"/>. The sequence is stored in row-major
        /// order.</remarks>
        /// <param name="rows">The number of rows in the matrix. Must be greater than zero.</param>
        /// <param name="cols">The number of columns in the matrix. Must be greater than zero.</param>
        /// <param name="x0">The initial value of the sequence.</param>
        /// <param name="dx">The increment applied to each subsequent element in the sequence.</param>
        /// <returns>A matrix of type <typeparamref name="T"/> with the specified dimensions, where each element is initialized
        /// to a value in the sequence starting at <paramref name="x0"/> and incremented by <paramref name="dx"/>.</returns>
        public static unsafe Matx<T> Create(long rows, long cols, T x0, T dx)
        {
            Matx<T> x = new (rows, cols, initMode: ArrayInitMode.Malloc);
            T* ptr = (T*)x.VPtr;
            T xi = x0;
            for (long i = 0; i < x.Count; i++)
            {
                ptr[i] = xi;
                xi += dx;
            }
            return x;
        }

        // ...

        #endregion
        #region ---- re-size ----

        // ...

        /// <summary>
        /// padding according to target matrix parameters
        /// </summary>
        /// <param name="targetRows"> target number of rows in the padded matrix </param>
        /// <param name="targetCols"> target number of columns in the padded matrix </param>
        /// <param name="startRowIndex"> starting row index in the padded matrix </param>
        /// <param name="startColIndex"> starting column index in the padded matrix </param>
        /// <param name="paddingValue"> value used for the padding </param>
        /// <returns> result matrix after padding </returns>
        [Obsolete]
        public Matx<T> Padding(long targetRows, long targetCols,
            long startRowIndex, long startColIndex,
            T paddingValue = default!)
        {
            if (targetRows <= Rows) throw new ArgumentOutOfRangeException($"{nameof(targetRows)} must be greater than the current value");
            if (targetCols <= Cols) throw new ArgumentOutOfRangeException($"{nameof(targetCols)} must be greater than the current value");

            Matx<T> y = Matx<T>.Create(rows: targetRows, cols: targetCols, x0: paddingValue);
            LongRange rowRng = new(startRowIndex, startRowIndex + Rows);
            LongRange colRng = new(startColIndex, startColIndex + Cols);
            y[rowRng, colRng] = this;

            return y;
        }

        /// <summary>
        /// centered zero-padding around each side
        /// </summary>
        /// <param name="targetRows"> target number of rows </param>
        /// <param name="targetCols"> target number of columns </param>
        /// <returns> result matrix after padding </returns>
        [Obsolete]
        public Matx<T> Padding(long targetRows, long targetCols)
            => Padding(targetRows, targetCols, (targetRows - Rows) / 2, (targetCols - Cols) / 2);

        #endregion
        #region ---- reverse ----

        /// <summary>
        /// Reverses the order of the rows in the current matrix.
        /// </summary>
        /// <remarks>This method swaps the rows of the matrix in place, such that the first row becomes
        /// the last, the second row becomes the second-to-last, and so on. The operation is performed directly on the
        /// underlying memory, and no additional memory is allocated.  If the matrix contains only one row or no rows,
        /// the method returns immediately without making any changes.</remarks>
        public unsafe void ReverseRows()
        {
            if (Rows <= 1) return; // No need to reverse if there's only one row

            long start = 0;
            long end = Rows - 1;

            T* ptr = (T*)VPtr;
            long offsetStart, offsetEnd;
            while (start < end)
            {
                for (long i = 0; i < Cols; i++)
                {
                    offsetStart = start * Cols + i;
                    offsetEnd = end * Cols + i;
                    // exchanges elements using tuple
                    (ptr[offsetStart], ptr[offsetEnd]) = (ptr[offsetEnd], ptr[offsetStart]);
                }
                // updates indices
                start++;
                end--;
            }
        }

        /// <summary>
        /// Reverses the order of elements in each column of a two-dimensional array.
        /// </summary>
        /// <remarks>This method operates on a two-dimensional array represented by the current instance. 
        /// It reverses the elements in each column, swapping elements from the start and end of  the column until the
        /// middle is reached. If the array has only one column, no changes  are made.</remarks>
        public unsafe void ReverseCols()
        {
            if (Cols <= 1) return; // No need to reverse if there's only one column

            long start = 0;
            long end = Cols - 1;

            T* ptr = (T*)VPtr;
            long offsetStart, offsetEnd;
            while (start < end)
            {
                for (long i = 0; i < Rows; i++)
                {
                    offsetStart = i * Cols + start;
                    offsetEnd = i * Cols + end;
                    // exchanges elements using tuple
                    (ptr[offsetStart], ptr[offsetEnd]) = (ptr[offsetEnd], ptr[offsetStart]);
                }
                // update indices
                start++;
                end--;
            }

        }

        /// <summary>
        /// Reverses the elements of the matrix in place.
        /// </summary>
        /// <remarks>This method reverses the elements in place, modifying the original matrix.  If
        /// the collection contains zero or one element, the method performs no operation.</remarks>
        public unsafe void Reverse()
        {
            if (Count <= 1) { return; } // No need to reverse if there's only one element

            long start = 0;
            long end = Count - 1;

            T* ptr = (T*)VPtr;
            while (start < end)
            {
                // exchanges elements using tuple
                (ptr[start], ptr[end]) = (ptr[end], ptr[start]);
                // updates indices
                start++;
                end--;
            }
        }

        #endregion



        private bool IsRowIndexValid(long i)
            => IsIndexValid(i, Rows, GetType().Name + " [row]");

        private bool IsColIndexValid(long i)
            => IsIndexValid(i, Cols, GetType().Name + " [column]");

        private bool IsRowRangeValid(LongRange rng)
            => IsRangeValid(rng, Rows, GetType().Name + " [row range]");

        private bool IsColRangeValid(LongRange rng)
            => IsRangeValid(rng, Cols, GetType().Name + " [column range]");

        #endregion
    }

}