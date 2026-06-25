using System.Numerics;

namespace VEMS.MathCore
{
    /// <summary>
    /// Matrix[T] class
    /// </summary>
    /// <typeparam name="T"> double or complex </typeparam>
    public class Matrix<T> : DenseArrayBase<T>, IMatrix<T> where T : struct
    {
        #region properties

        /// <summary>
        /// number of rows
        /// </summary>
        public long Rows { get; set; }

        /// <summary>
        /// number of columns
        /// </summary>
        public long Cols { get; set; }

        /// <summary>
        /// get / set the value of a matrix element
        /// using row and column indices
        /// </summary>
        /// <param name="iRow"> row index [Int64] </param>
        /// <param name="iCol"> column index [Int64] </param>
        /// <param name="checkBound"> whether to check if the index is outside bound </param>
        /// <returns> value of the element </returns>
        public unsafe T this[long iRow, long iCol, bool checkBound = true]
        {
            get
            {
                bool invalidIndex = checkBound &&
                    (!IsRowIndexValid(iRow) || !IsColIndexValid(iCol));
                if (invalidIndex) { throw new IndexOutOfRangeException(); }

#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
                T* p = (T*)DataPtr.ToPointer();
#pragma warning restore CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
                T res = *(p + iRow * Cols + iCol);
                return res;
            }
            set
            {
                bool invalidIndex = checkBound &&
                    (!IsRowIndexValid(iRow) || !IsColIndexValid(iCol));
                if (invalidIndex) { throw new IndexOutOfRangeException(); }

#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
                T* p = (T*)DataPtr.ToPointer();
#pragma warning restore CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
                *(p + iRow * Cols + iCol) = value;
            }
        }

        /// <summary>
        /// get / set the value of a matrix element
        /// using row and column indices
        /// </summary>
        /// <param name="iRow"> row index [Int32] </param>
        /// <param name="iCol"> column index [Int32] </param>
        /// <param name="checkBound"> whether to check if the index is outside bound </param>
        /// <returns> value of the element </returns>
        public T this[int iRow, int iCol, bool checkBound = true]
        {
            get => this[(long)iRow, (long)iCol, checkBound];
            set => this[(long)iRow, (long)iCol, checkBound] = value;
        }

        /// <summary>
        /// get / set the value of a matrix element
        /// using row and column indices
        /// </summary>
        /// <param name="iRow"> row index [Int32] </param>
        /// <param name="iCol"> column index [Int32] </param>
        /// <param name="checkBound"> whether to check if the index is outside bound </param>
        /// <returns> value of the element </returns>
        public T this[Index iRow, Index iCol, bool checkBound = true]
        {
            get => this[IndexToInt(iRow, Rows), IndexToInt(iCol, Cols), checkBound];
            set => this[IndexToInt(iRow, Rows), IndexToInt(iCol, Cols), checkBound] = value;
        }

        /// <summary>
        /// Gets the range representing all rows in the current context.
        /// </summary>
        public LongRange AllRows => new(start: 0, end: Rows);

        /// <summary>
        /// Gets the range representing all columns in the current context.
        /// </summary>
        public LongRange AllCols => new(start: 0, end: Cols);

        /// <summary>
        /// get / set a row slice from / into the matrix
        /// </summary>
        /// <param name="iRow"> row index [Int64]</param>
        /// <param name="colRng"> column range with inclusive start and exclusive end indices [Int64]  </param>
        /// <param name="loopMode"> loop computational mode option </param>
        /// <returns> row slice vector </returns>
        public Vector<T> this[long iRow, LongRange colRng, 
            LoopMode loopMode = Defaults.LoopOption]
        {
            get
            {
                if (!IsRowIndexValid(iRow)) { throw new IndexOutOfRangeException("Invalid row index"); }
                if (!IsColRangeValid(colRng)) { throw new IndexOutOfRangeException("Invalid range"); }
                
                long n = colRng.End - colRng.Start;
                Vector<T> x = new(count: n, mode: ArrayInitMode.Malloc);
                
                Action<long> a = (iCol) => { x[iCol - colRng.Start, false] = this[iRow, iCol, false]; };
                Loop1D loop = new(operation: a, start: colRng.Start, end: colRng.End);
                loop.Evaluate(loopMode);
                
                return x;
            }
            set
            {
                if (!IsRowIndexValid(iRow)) { throw new IndexOutOfRangeException("Invalid row index"); }
                if (!IsColRangeValid(colRng)) { throw new IndexOutOfRangeException("Invalid range"); }
                
                Action<long> a = (iCol) => { this[iRow, iCol, false] = value[iCol - colRng.Start, false]; };
                Loop1D loop = new(operation: a, start: colRng.Start, end: colRng.End);
                loop.Evaluate(loopMode);
            }
        }

        /// <summary>
        /// get / set a row slice from / into the matrix
        /// </summary>
        /// <param name="iRow"> row index [Int32]</param>
        /// <param name="colRng"> column range with inclusive start and exclusive end indices [Int32]  </param>
        /// <param name="loopMode"> loop computational mode option </param>
        /// <returns> row slice vector </returns>
        public Vector<T> this[Index iRow, Range colRng, 
            LoopMode loopMode = Defaults.LoopOption]
        {
            get => this[IndexToInt(iRow, Rows), new LongRange(colRng, Cols), loopMode];
            set => this[IndexToInt(iRow, Rows), new LongRange(colRng, Cols), loopMode] = value;
        }

        /// <summary>
        /// get / set a column slice from / into the matrix
        /// </summary>
        /// <param name="rowRng"> row range with inclusive start and exclusive end indices [Int64] </param>
        /// <param name="iCol"> column index [Int64] </param>
        /// <param name="loopMode"> loop computational mode option </param>
        /// <returns> column slice vector </returns>
        public Vector<T> this[LongRange rowRng, long iCol,
            LoopMode loopMode = Defaults.LoopOption]
        {
            get
            {
                if (!IsColIndexValid(iCol)) { throw new IndexOutOfRangeException("Invalid column index"); }
                if (!IsRowRangeValid(rowRng)) { throw new IndexOutOfRangeException("Invalid range"); }
                
                long n = rowRng.End - rowRng.Start;
                Vector<T> x = new(count: n, mode: ArrayInitMode.Malloc);
                
                Action<long> a = (iRow) => { x[iRow - rowRng.Start, false] = this[iRow, iCol, false]; };
                Loop1D loop = new(operation: a, start: rowRng.Start, end: rowRng.End);
                loop.Evaluate(loopMode);

                return x;
            }
            set
            {
                if (!IsColIndexValid(iCol)) { throw new IndexOutOfRangeException("Invalid column index"); }
                if (!IsRowRangeValid(rowRng)) { throw new IndexOutOfRangeException("Invalid range"); }
                
                Action<long> a = (iRow) => { this[iRow, iCol, false] = value[iRow - rowRng.Start, false]; };
                Loop1D loop = new(operation: a, start: rowRng.Start, end: rowRng.End);
                loop.Evaluate(loopMode);
            }
        }

        /// <summary>
        /// get / set a column slice from / into the matrix
        /// </summary>
        /// <param name="rowRng"> row range with inclusive start and exclusive end indices [Int32] </param>
        /// <param name="iCol"> column index [Int32] </param>
        /// <param name="loopMode"> loop computational mode option </param>
        /// <returns> column slice vector </returns>
        public Vector<T> this[Range rowRng, Index iCol, 
            LoopMode loopMode = Defaults.LoopOption]
        {
            get => this[new LongRange(rowRng, Rows), IndexToInt(iCol, Cols)];
            set => this[new LongRange(rowRng, Rows), IndexToInt(iCol, Cols)] = value;
        }

        /// <summary>
        /// get / set a block from / into the matrix
        /// </summary>
        /// <param name="rowRng"> row range with inclusive start and exclusive end indices [Int64] </param>
        /// <param name="colRng"> column range with inclusive start and exclusive end indices [Int64] </param>
        /// <param name="loopMode"> loop computational mode option </param>
        /// <returns> sub-matrix </returns>
        public Matrix<T> this[LongRange rowRng, LongRange colRng, 
            LoopMode loopMode = Defaults.LoopOption]
        {
            get
            {
                if (!IsRowRangeValid(rowRng)) { throw new IndexOutOfRangeException("Invalid range"); }
                if (!IsColRangeValid(colRng)) { throw new IndexOutOfRangeException("Invalid range"); }
                
                long rows = rowRng.End - rowRng.Start;
                long cols = colRng.End - colRng.Start;
                Matrix<T> x = new(rows: rows, cols: cols, mode: ArrayInitMode.Malloc);
                
                Action<long, long> a = (iRow, iCol) => { x[iRow - rowRng.Start, iCol - colRng.Start, false] = this[iRow, iCol, false]; };
                Loop2D loop = new(operation: a, 
                    rowStart: rowRng.Start, rowEnd: rowRng.End,
                    colStart: colRng.Start, colEnd: colRng.End);
                loop.Evaluate(loopMode);

                return x;
            }
            set
            {
                if (!IsRowRangeValid(rowRng)) { throw new IndexOutOfRangeException("Invalid range"); }
                if (!IsColRangeValid(colRng)) { throw new IndexOutOfRangeException("Invalid range"); }

                Action<long, long> a = (iRow, iCol) => { this[iRow, iCol, false] = value[iRow - rowRng.Start, iCol - colRng.Start, false]; };
                Loop2D loop = new(operation: a,
                    rowStart: rowRng.Start, rowEnd: rowRng.End,
                    colStart: colRng.Start, colEnd: colRng.End);
                loop.Evaluate(loopMode);
            }
        }

        /// <summary>
        /// get / set a block from / into the matrix
        /// </summary>
        /// <param name="rowRng"> row range with inclusive start and exclusive end indices [Int32] </param>
        /// <param name="colRng"> column range with inclusive start and exclusive end indices [Int32] </param>
        /// <param name="loopMode"> loop computational mode option </param>
        /// <returns> sub-matrix </returns>
        public Matrix<T> this[Range rowRng, Range colRng,
            LoopMode loopMode = Defaults.LoopOption]
        {
            get => this[new LongRange(rowRng, Rows), new LongRange(colRng, Cols)];
            set => this[new LongRange(rowRng, Rows), new LongRange(colRng, Cols)] = value;
        }


        // ------------------------------------------------ //

        /// <summary>
        /// get / set the value of a matrix element 
        /// using linear index (w.r.t. total element count)
        /// </summary>
        /// <param name="i"> index of the element [Int64] </param>
        /// <param name="checkBound"> whether to check if the index is outside bound </param>
        /// <returns> element value </returns>
        internal unsafe T this[long i, bool checkBound = true]
        {
            get
            {
                bool invalidIndex = checkBound && !IsIndexValid(i, Count);
                if (invalidIndex) { throw new ArgumentOutOfRangeException(); }

#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
                T* iPtr = (T*)DataPtr.ToPointer();
#pragma warning restore CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
                T iVal = *(iPtr + i);
                return iVal;
            }
            set
            {
                bool invalidIndex = checkBound && !IsIndexValid(i, Count);
                if (invalidIndex) { throw new ArgumentOutOfRangeException(); }

#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
                T* iPtr = (T*)DataPtr.ToPointer();
#pragma warning restore CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
                *(iPtr + i) = value;
            }
        }

        /// <summary>
        /// get / set the value of a matrix element 
        /// using linear index (w.r.t. total element count)
        /// </summary>
        /// <param name="i"> index of the element [Int32] </param>
        /// <param name="checkBound"> whether to check if the index is outside bound </param>
        /// <returns> element value </returns>
        internal T this[int i, bool checkBound = true]
        {
            get => this[(long)i, checkBound];
            set => this[(long)i, checkBound] = value;
        }

        /// <summary>
        /// get / set the value of a matrix element 
        /// using linear index (w.r.t. total element count)
        /// </summary>
        /// <param name="i"> index of the element [Int32] </param>
        /// <param name="checkBound"> whether to check if the index is outside bound </param>
        /// <returns> element value </returns>
        internal T this[Index i, bool checkBound = true]
        {
            get => this[IndexToInt(i, Count), checkBound];
            set => this[IndexToInt(i, Count), checkBound] = value;
        }

        #endregion
        #region constructors

        /// <summary>
        /// default constructor
        /// </summary>
        internal Matrix() { }

        /// <summary>
        /// constructs a matrix with given length
        /// by default, initializes element values to zeros
        /// </summary>
        /// <param name="rows"> number of rows </param>
        /// <param name="cols"> number of columns </param>
        /// <param name="mode"> construct mode option; default is CALLOC </param>
        public Matrix(long rows, long cols,
            ArrayInitMode mode = ArrayInitMode.Calloc)
            : base(rows * cols, mode)
        {
            Rows = rows;
            Cols = cols;
        }

        /// <summary>
        /// constructs a matrix with given length
        /// and sets all elements to the same initial value
        /// </summary>
        /// <param name="rows"> number of rows </param>
        /// <param name="cols"> number of columns </param>
        /// <param name="initVal"> initial value for all the elements </param>
        /// <param name="loopMode"> loop-computation mode option </param>
        public Matrix(long rows, long cols, T initVal,
            LoopMode loopMode = Defaults.LoopOption)
            : base(rows * cols, initVal, loopMode) 
        {
            Rows = rows;
            Cols = cols;
        }

        /// <summary>
        /// constructs by copying from another
        /// </summary>
        /// <param name="other"> another vector as source </param>
        /// <param name="copyMode"> copy mode option </param>
        public Matrix(Matrix<T> other,
            ArrayCopyMode copyMode = ArrayCopyMode.Deep)
            : base(other, copyMode)
        {
            Rows = other.Rows;
            Cols = other.Cols;
        }

        #endregion
        #region methods

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


        /// <summary>
        /// checks whether a given row index 
        /// is within [0, Rows)
        /// </summary>
        /// <param name="i"> input row index </param>
        /// <returns> return true when valid </returns>
        public bool IsRowIndexValid(long i)
            => IsIndexValid(i, Rows, GetType().Name + " [row]");

        /// <summary>
        /// checks whether a given row range 
        /// is within [0, Rows)
        /// </summary>
        /// <param name="rng"> input row range </param>
        /// <returns> return true when valid </returns>
        public bool IsRowRangeValid(LongRange rng)
            => IsRangeValid(rng, Rows, GetType().Name + " [row range]");

        /// <summary>
        /// checks whether a given column index 
        /// is within [0, Cols)
        /// </summary>
        /// <param name="i"> input column index </param>
        /// <returns> return true when valid </returns>
        public bool IsColIndexValid(long i)
            => IsIndexValid(i, Cols, GetType().Name + " [column]");

        /// <summary>
        /// checks whether a given column range 
        /// is within [0, Cols)
        /// </summary>
        /// <param name="rng"> input column range </param>
        /// <returns> return true when valid </returns>
        public bool IsColRangeValid(LongRange rng)
            => IsRangeValid(rng, Cols, GetType().Name + " [column range]");

        #endregion
    }

    /// <summary>
    /// real matrix class
    /// </summary>
    public class MatrixD : Matrix<double>
    {
        #region fields

        /// <summary>
        /// empty matrix with ZERO row and column count
        /// </summary>
        public static MatrixD Empty = new(0, 0);

        #endregion
        #region properties

        /// <summary>
        /// gets the type-specific pointer to the values
        /// </summary>
        public unsafe double* SPtr
        {
            get => (double*)DataPtr.ToPointer();
        }

        /// <summary>
        /// get / set a row slice from / into the matrix
        /// </summary>
        /// <param name="iRow"> row index [Int64]</param>
        /// <param name="colRng"> column range with inclusive start and exclusive end indices [Int64]  </param>
        /// <param name="loopMode"> loop computational mode option </param>
        /// <returns> row slice vector </returns>
        public new VectorD this[long iRow, LongRange colRng,
            LoopMode loopMode = Defaults.LoopOption]
        {
            get
            {
                if (!IsRowIndexValid(iRow)) { throw new IndexOutOfRangeException("Invalid row index"); }
                if (!IsColRangeValid(colRng)) { throw new IndexOutOfRangeException("Invalid range"); }

                long n = colRng.End - colRng.Start;
                VectorD x = new(count: n, mode: ArrayInitMode.Malloc);
                //VectorD x = new(count: n, initMode: ArrayInitMode.Malloc);

                Action<long> a = (iCol) => { x[iCol - colRng.Start, false] = this[iRow, iCol, false]; };
                Loop1D loop = new(operation: a, start: colRng.Start, end: colRng.End);
                loop.Evaluate(loopMode);

                return x;
            }
            set
            {
                //if (value.GetType() != typeof(VectorD)) { throw new ArgumentException("Invalid input", nameof(value)); }
                if (!IsRowIndexValid(iRow)) { throw new IndexOutOfRangeException("Invalid row index"); }
                if (!IsColRangeValid(colRng)) { throw new IndexOutOfRangeException("Invalid range"); }

                Action<long> a = (iCol) => { this[iRow, iCol, false] = value[iCol - colRng.Start, false]; };
                Loop1D loop = new(operation: a, start: colRng.Start, end: colRng.End);
                loop.Evaluate(loopMode);
            }
        }

        /// <summary>
        /// get / set a row slice from / into the matrix
        /// </summary>
        /// <param name="iRow"> row index [Int32]</param>
        /// <param name="colRng"> column range with inclusive start and exclusive end indices [Int32]  </param>
        /// <param name="loopMode"> loop computational mode option </param>
        /// <returns> row slice vector </returns>
        public new VectorD this[Index iRow, Range colRng,
            LoopMode loopMode = Defaults.LoopOption]
        {
            get => this[IndexToInt(iRow, Rows), new LongRange(colRng, Cols), loopMode];
            set => this[IndexToInt(iRow, Rows), new LongRange(colRng, Cols), loopMode] = value;
        }

        /// <summary>
        /// get / set a column slice from / into the matrix
        /// </summary>
        /// <param name="rowRng"> row range with inclusive start and exclusive end indices [Int64] </param>
        /// <param name="iCol"> column index [Int64] </param>
        /// <param name="loopMode"> loop computational mode option </param>
        /// <returns> column slice vector </returns>
        public new VectorD this[LongRange rowRng, long iCol,
            LoopMode loopMode = Defaults.LoopOption]
        {
            get
            {
                if (!IsColIndexValid(iCol)) { throw new IndexOutOfRangeException("Invalid column index"); }
                if (!IsRowRangeValid(rowRng)) { throw new IndexOutOfRangeException("Invalid range"); }

                long n = rowRng.End - rowRng.Start;
                VectorD x = new(count: n, mode: ArrayInitMode.Malloc);
                //VectorD x = new(count:n, initMode: ArrayInitMode.Malloc);

                Action<long> a = (iRow) => { x[iRow - rowRng.Start, false] = this[iRow, iCol, false]; };
                Loop1D loop = new(operation: a, start: rowRng.Start, end: rowRng.End);
                loop.Evaluate(loopMode);

                return x;
            }
            set
            {
                //if (value.GetType() != typeof(VectorD)) { throw new ArgumentException("Invalid input", nameof(value)); }
                if (!IsColIndexValid(iCol)) { throw new IndexOutOfRangeException("Invalid column index"); }
                if (!IsRowRangeValid(rowRng)) { throw new IndexOutOfRangeException("Invalid range"); }

                Action<long> a = (iRow) => { this[iRow, iCol, false] = value[iRow - rowRng.Start, false]; };
                Loop1D loop = new(operation: a, start: rowRng.Start, end: rowRng.End);
                loop.Evaluate(loopMode);
            }
        }

        /// <summary>
        /// get / set a column slice from / into the matrix
        /// </summary>
        /// <param name="rowRng"> row range with inclusive start and exclusive end indices [Int32] </param>
        /// <param name="iCol"> column index [Int32] </param>
        /// <param name="loopMode"> loop computational mode option </param>
        /// <returns> column slice vector </returns>
        public new VectorD this[Range rowRng, Index iCol,
            LoopMode loopMode = Defaults.LoopOption]
        {
            get => this[new LongRange(rowRng, Rows), IndexToInt(iCol, Cols), loopMode];
            set => this[new LongRange(rowRng, Rows), IndexToInt(iCol, Cols), loopMode] = value;
        }

        /// <summary>
        /// get / set a block from / into the matrix
        /// </summary>
        /// <param name="rowRng"> row range with inclusive start and exclusive end indices [Int64] </param>
        /// <param name="colRng"> column range with inclusive start and exclusive end indices [Int64] </param>
        /// <param name="loopMode"> loop computational mode option </param>
        /// <returns> sub-matrix </returns>
        public new MatrixD this[LongRange rowRng, LongRange colRng,
            LoopMode loopMode = Defaults.LoopOption]
        {
            get
            {
                if (!IsRowRangeValid(rowRng)) { throw new IndexOutOfRangeException("Invalid range"); }
                if (!IsColRangeValid(colRng)) { throw new IndexOutOfRangeException("Invalid range"); }

                long rows = rowRng.End - rowRng.Start;
                long cols = colRng.End - colRng.Start;
                MatrixD x = new(rows: rows, cols: cols, mode: ArrayInitMode.Malloc);

                Action<long, long> a = (iRow, iCol) => { x[iRow - rowRng.Start, iCol - colRng.Start, false] = this[iRow, iCol, false]; };
                Loop2D loop = new(operation: a,
                    rowStart: rowRng.Start, rowEnd: rowRng.End,
                    colStart: colRng.Start, colEnd: colRng.End);
                loop.Evaluate(loopMode);

                return x;
            }
            set
            {
                //if (value.GetType() != typeof(MatrixD)) { throw new ArgumentException("Invalid input", nameof(value)); }
                if (!IsRowRangeValid(rowRng)) { throw new IndexOutOfRangeException("Invalid range"); }
                if (!IsColRangeValid(colRng)) { throw new IndexOutOfRangeException("Invalid range"); }

                Action<long, long> a = (iRow, iCol) => { this[iRow, iCol, false] = value[iRow - rowRng.Start, iCol - colRng.Start, false]; };
                Loop2D loop = new(operation: a,
                    rowStart: rowRng.Start, rowEnd: rowRng.End,
                    colStart: colRng.Start, colEnd: colRng.End);
                loop.Evaluate(loopMode);
            }
        }

        /// <summary>
        /// get / set a block from / into the matrix
        /// </summary>
        /// <param name="rowRng"> row range with inclusive start and exclusive end indices [Int32] </param>
        /// <param name="colRng"> column range with inclusive start and exclusive end indices [Int32] </param>
        /// <param name="loopMode"> loop computational mode option </param>
        /// <returns> sub-matrix </returns>
        public new MatrixD this[Range rowRng, Range colRng,
            LoopMode loopMode = Defaults.LoopOption]
        {
            get => this[new LongRange(rowRng, Rows), new LongRange(colRng, Cols), loopMode];
            set => this[new LongRange(rowRng, Rows), new LongRange(colRng, Cols), loopMode] = value;
        }

        ///// <summary>
        ///// get / set the value of a matrix element 
        ///// using linear index (w.r.t. total element count)
        ///// </summary>
        ///// <param name="i"> index of the element [Int64] </param>
        ///// <param name="checkBound"> whether to check if the index is outside bound </param>
        ///// <returns> element value </returns>
        //internal unsafe double this[long i, bool checkBound = true]
        //{
        //    get
        //    {
        //        bool invalidIndex = checkBound && !IsIndexValid(i, Count);
        //        if (invalidIndex) { throw new ArgumentOutOfRangeException(); }

        //        double* iPtr = (double*)DataPtr.ToPointer();
        //        double iVal = *(iPtr + i);
        //        return iVal;
        //    }
        //    set
        //    {
        //        bool invalidIndex = checkBound && !IsIndexValid(i, Count);
        //        if (invalidIndex) { throw new ArgumentOutOfRangeException(); }

        //        double* iPtr = (double*)DataPtr.ToPointer();
        //        *(iPtr + i) = value;
        //    }
        //}

        #endregion
        #region constructor

        /// <summary>
        /// constructs a matrix with given length
        /// by default, does not initialize element values
        /// </summary>
        /// <param name="rows"> number of rows </param>
        /// <param name="cols"> number of columns </param>
        /// <param name="mode"> construct mode option; default is CALLOC </param>
        public MatrixD(long rows, long cols,
            ArrayInitMode mode = ArrayInitMode.Calloc)
            : base(rows, cols, mode) { }

        /// <summary>
        /// constructs a matrix with given size
        /// and sets all elements to the same value 
        /// </summary>
        /// <param name="rows"> number of rows </param>
        /// <param name="cols"> number of cols </param>
        /// <param name="initVal"> initial value for all the elements </param>
        /// <param name="loopMode"> loop-computation mode option </param>
        public MatrixD(long rows, long cols, double initVal,
            LoopMode loopMode = Defaults.LoopOption)
            : base(rows, cols, initVal, loopMode) { }

        ///// <summary>
        ///// constructs a matrix with given size,
        ///// initial value and increment
        ///// </summary>
        ///// <param name="rows"> number of rows </param>
        ///// <param name="cols"> number of cols </param>
        ///// <param name="initVal"> initial value for the first element </param>
        ///// <param name="increment"> increment between two elements </param>
        ///// <param name="loopMode"> loop-computation mode option </param>
        //public MatrixD(long rows, long cols, double initVal, double increment,
        //    LoopMode loopMode = Defaults.LoopOption)
        //    : this(rows, cols)
        //{
        //    Action<long> a = (i) => { this[i, false] = initVal + i * increment; };
        //    Loop1D loop = new(operation: a, start: 0, end: Count);
        //    loop.Evaluate(loopMode);
        //}

        ///// <summary>
        ///// constructs a matrix with given size,
        ///// m[iRow, iCol] = initVal + iRow * incRow + iCol * incCol
        ///// </summary>
        ///// <param name="rows"> number of rows </param>
        ///// <param name="cols"> number of columns </param>
        ///// <param name="initVal"> initial value for the first element </param>
        ///// <param name="incRow"> increment between two rows </param>
        ///// <param name="incCol"> increment between two columns </param>
        ///// <param name="loopMode"> loop-computation mode option </param>
        //public MatrixD(long rows, long cols, double initVal,
        //    double incRow, double incCol,
        //    LoopMode loopMode = Defaults.LoopOption) : this(rows, cols)
        //{
        //    Action<long, long> a = (iRow, iCol) =>
        //    { this[iRow, iCol, false] = initVal + iRow * incRow + iCol * incCol; };
        //    Loop2D loop = new(operation: a, rowStart: 0, rowEnd: Rows, colStart: 0, colEnd: Cols);
        //    loop.Evaluate(loopMode);
        //}

        ///// <summary>
        ///// constructs a matrix with given size,
        ///// initial values for each row, and increment between columns
        ///// </summary>
        ///// <param name="rows"> number of rows </param>
        ///// <param name="cols"> number of cols </param>
        ///// <param name="rowInits"> initial values for each row </param>
        ///// <param name="colIncrs"> increment between two columns </param>
        ///// <param name="loopMode"> loop-computation mode option </param>
        //public MatrixD(long rows, long cols,
        //    VectorD rowInits, double colIncrs,
        //    LoopMode loopMode = Defaults.LoopOption) : this(rows, cols)
        //{
        //    if (rowInits.Count != rows) { throw new NotSupportedException("Length of initVals vector unequal to matrix rows"); }

        //    Action<long, long> a = (iRow, iCol) =>
        //    { this[iRow, iCol, false] = rowInits[iRow, false] + iCol * colIncrs; };
        //    Loop2D loop = new(operation: a, rowStart: 0, rowEnd: Rows, colStart: 0, colEnd: Cols);
        //    loop.Evaluate(loopMode);
        //}

        ///// <summary>
        ///// constructs a matrix with given size,
        ///// initial values for each row, and the 
        ///// increment between two elements in each row
        ///// </summary>
        ///// <param name="rows"> number of rows </param>
        ///// <param name="cols"> number of cols </param>
        ///// <param name="rowInits"> initial values for each row </param>
        ///// <param name="colIncrs"> increment between two elements for each row </param>
        ///// <param name="loopMode"> loop-computation mode option </param>
        //public MatrixD(long rows, long cols,
        //    VectorD rowInits, VectorD colIncrs,
        //    LoopMode loopMode = Defaults.LoopOption)
        //    : this(rows, cols)
        //{
        //    if (rowInits.Count != rows || colIncrs.Count != rows) { throw new NotSupportedException("Length of initVals/increments vector unequal to matrix rows"); }

        //    Action<long, long> a = (iRow, iCol) =>
        //    { this[iRow, iCol, false] = rowInits[iRow, false] + iCol * colIncrs[iRow, false]; };
        //    Loop2D loop = new(operation: a, rowStart: 0, rowEnd: Rows, colStart: 0, colEnd: Cols);
        //    loop.Evaluate(loopMode);
        //}

        /// <summary>
        /// constructs a matrix by copying from another
        /// </summary>
        /// <param name="other"> another matrix </param>
        /// <param name="deepCopy"> copy mode option; default is deep copy </param>
        public MatrixD(MatrixD other, bool deepCopy = true)
            : this(other.Rows, other.Cols, deepCopy ? ArrayInitMode.Malloc : ArrayInitMode.None)
        {
            if (deepCopy)
            {
                MatrixD t = this;
                //VMath.CopyD(other, ref t);
                unsafe { Defaults.IBLAS.Copy(n: other.Count, x: (double*)other.SPtr, 
                    y: (double*)t.SPtr, incx: 1, incy: 1); }
            }
            else
            { DataPtr = other.DataPtr; }
        }

        /// <summary>
        /// constructs a matrix by copying from a given ArrayBase
        /// </summary>
        /// <param name="other"> another ArrayBase </param>
        /// <param name="rows"> number of rows </param>
        /// <param name="cols"> number of columns </param>
        /// <param name="deepCopy"> copy mode option; default is deep copy </param>
        public MatrixD(DenseArrayBase<double> other, long rows, long cols, bool deepCopy = true)
            : this(rows, cols, deepCopy ? ArrayInitMode.Malloc : ArrayInitMode.None)
        {
            if (deepCopy)
            {
                DenseArrayBase<double> t = this;
                //VMath.CopyD(other, ref t);
                unsafe { Defaults.IBLAS.Copy(n: other.Count, x: (double*)other.VPtr, 
                    y: (double*)t.VPtr, incx: 1, incy: 1); }
            }
            else
            { DataPtr = other.DataPtr; }
        }

        #endregion
        #region naive methods

        ///// <summary>
        ///// sums up all the elements
        ///// </summary>
        ///// <returns> summation result </returns>
        //public double Sum(LoopMode mode = Defaults.LoopOption)
        //    => VMath.Sum(x: this, loopMode: mode);

        ///// <summary>
        ///// finds the index of the element with the largest value
        ///// </summary>
        ///// <returns> (index, value) </returns>
        //public (long, double) IndexMax(LoopMode mode = Defaults.LoopOption)
        //    => VMath.Max(x: this, loopMode: mode);

        ///// <summary>
        ///// finds the index of the element with the smallest value
        ///// </summary>
        ///// <returns> (index, value) </returns>
        //public (long, double) IndexMin(LoopMode mode = Defaults.LoopOption)
        //    => VMath.Min(x: this, loopMode: mode);

        ///// <summary>
        ///// sorts the elements in the vector, from smallest to largest
        ///// </summary>
        //public void Sort()
        //{
        //    MatrixD t = this;
        //    VMath.Sort(x: ref t);
        //}

        ///// <summary>
        ///// converts to array
        ///// </summary>
        ///// <returns> result array </returns>
        //public double[,] ToArray(LoopMode mode = Defaults.LoopOption)
        //    => VMath.ConvertMatrixToArray(x: this, loopMode: mode);

        #endregion
        #region extra methods

        ///// <summary>
        ///// checks whether a given row index 
        ///// is within [0, Rows)
        ///// </summary>
        ///// <param name="i"> input row index </param>
        ///// <returns> return true when valid </returns>
        //public new bool IsRowIndexValid(long i)
        //    => IsIndexValid(i, Rows, GetType().Name + " [row]");

        ///// <summary>
        ///// checks whether a given row range 
        ///// is within [0, Rows)
        ///// </summary>
        ///// <param name="rng"> input row range </param>
        ///// <returns> return true when valid </returns>
        //public new bool IsRowRangeValid(LongRange rng)
        //    => IsRangeValid(rng, Rows, GetType().Name + " [row range]");

        ///// <summary>
        ///// checks whether a given column index 
        ///// is within [0, Cols)
        ///// </summary>
        ///// <param name="i"> input column index </param>
        ///// <returns> return true when valid </returns>
        //public new bool IsColIndexValid(long i)
        //    => IsIndexValid(i, Cols, GetType().Name + " [column]");

        ///// <summary>
        ///// checks whether a given column range 
        ///// is within [0, Cols)
        ///// </summary>
        ///// <param name="rng"> input column range </param>
        ///// <returns> return true when valid </returns>
        //public new bool IsColRangeValid(LongRange rng)
        //    => IsRangeValid(rng, Cols, GetType().Name + " [column range]");

        ///// <summary>
        ///// checks whether this matrix has
        ///// the same dimension as the other
        ///// </summary>
        ///// <param name="other"> the other matrix </param>
        ///// <returns> return true if same; otherwise false </returns>
        //public bool HasSameDimension(MatrixD other)
        //{
        //    if (other.Rows == Rows && other.Cols == Cols)
        //        return true;
        //    else
        //        Console.Write("Unequal matrix dimension");
        //    return false;
        //}

        ///// <summary>
        ///// checks whether this matrix has
        ///// the same dimension as the other
        ///// </summary>
        ///// <param name="other"> the other matrix </param>
        ///// <returns> return true if same; otherwise false </returns>
        //public bool HasSameDimension(MatrixZ other)
        //{
        //    if (other.Rows == Rows && other.Cols == Cols)
        //        return true;
        //    else
        //        Console.Write("Unequal matrix dimension");
        //    return false;
        //}

        /// <summary>
        /// padding according to target matrix parameters
        /// </summary>
        /// <param name="targetRows"> target number of rows in the padded matrix </param>
        /// <param name="targetCols"> target number of columns in the padded matrix </param>
        /// <param name="startRowIndex"> starting row index in the padded matrix </param>
        /// <param name="startColIndex"> starting column index in the padded matrix </param>
        /// <param name="paddingValue"> value used for the padding </param>
        /// <returns> result matrix after padding </returns>
        public MatrixD Padding(long targetRows, long targetCols,
            long startRowIndex, long startColIndex,
            double paddingValue = 0.0)
        {
            if (targetRows <= Rows)
            {
                Printer.Warning($"{nameof(targetRows)} must be greater than the current value");
                return Empty;
            }
            if (targetCols <= Cols)
            {
                Printer.Warning($"{nameof(targetCols)} must be greater than the current value");
                return Empty;
            }

            MatrixD y = new(targetRows, targetCols, paddingValue);
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
        public MatrixD Padding(long targetRows, long targetCols)
        {
            if ((targetRows - Rows) % 2 != 0)
            {
                Printer.Warning($"{nameof(targetRows)} must be an even addition to the current value");
                return Empty;
            }
            if ((targetCols - Cols) % 2 != 0)
            {
                Printer.Warning($"{nameof(targetCols)} must be an even addition to the current value");
                return Empty;
            }
            return Padding(targetRows, targetCols, (targetRows - Rows) / 2, (targetCols - Cols) / 2, 0.0);
        }

        ///// <summary>
        ///// truncates current matrix according to target parameters
        ///// </summary>
        ///// <param name="targetRows"> target number of rows in the truncated matrix </param>
        ///// <param name="targetCols"> target number of columns in the truncated matrix </param>
        ///// <param name="startRowIndex"> starting row index in the original matrix </param>
        ///// <param name="startColIndex"> starting column index in the original matrix </param>
        ///// <returns> result matrix after truncation </returns>
        //public MatrixD Truncate(long targetRows, long targetCols,
        //    long startRowIndex, long startColIndex)
        //{
        //    if (startRowIndex + targetRows >= Rows)
        //    {
        //        Printer.Warning($"invalid combination of parameters {nameof(targetRows)} and {nameof(startRowIndex)}");
        //        return Empty;
        //    }
        //    if (startColIndex + targetCols >= Cols)
        //    {
        //        Printer.Warning($"invalid combination of parameters {nameof(targetCols)} and {nameof(startColIndex)}");
        //        return Empty;
        //    }

        //    LongRange rowRng = new(startRowIndex, startRowIndex + targetRows);
        //    LongRange colRng = new(startColIndex, startColIndex + targetCols);
        //    return this[rowRng, colRng];
        //}

        ///// <summary>
        ///// centered truncation on each side of the matrix
        ///// </summary>
        ///// <param name="targetRows"> target number of rows </param>
        ///// <param name="targetCols"> target number of columns </param>
        ///// <returns> result matrix after truncation </returns>
        //public MatrixD Truncate(long targetRows, long targetCols)
        //{
        //    if ((Rows - targetRows) % 2 != 0)
        //    {
        //        Printer.Warning($"{nameof(targetRows)} must be an even subtraction of the current value");
        //        return Empty;
        //    }
        //    if ((Cols - targetCols) % 2 != 0)
        //    {
        //        Printer.Warning($"{nameof(targetCols)} must be an even subtraction of the current value");
        //        return Empty;
        //    }
        //    return Truncate(targetRows, targetCols, (Rows - targetRows) / 2, (Cols - targetCols) / 2);
        //}

        ///// <summary>
        ///// replicates according to target number of rows and columns
        ///// </summary>
        ///// <param name="targetRows"> target number of rows </param>
        ///// <param name="targetCols"> target number of columns </param>
        ///// <returns> replicated result </returns>
        //public MatrixD Replicate(long targetRows, long targetCols)
        //{
        //    if (targetRows <= Rows) { Printer.Warning($"Target number of rows not greater than the current"); }
        //    if (targetCols <= Cols) { Printer.Warning($"Target number of columns not greater than the current"); }
        //    MatrixD rp = new(targetRows, targetCols);
        //    // computes replication multiples and residual counts
        //    long mRow = (long)(targetRows / Rows);
        //    long mCol = (long)(targetCols / Cols);
        //    long restRows = targetRows - mRow * Rows;
        //    long restCols = targetCols - mCol * Cols;
        //    // loop for multiple replications
        //    for (long nRow = 0; nRow < mRow; nRow++)
        //    {
        //        for (long nCol = 0; nCol < mCol; nCol++)
        //        {
        //            LongRange uRow = new(nRow * Rows + 0, nRow * Rows + Rows);
        //            LongRange uCol = new(nCol * Cols + 0, nCol * Cols + Cols);
        //            rp[uRow, uCol] = this;
        //        }
        //    }
        //    // loop for the residuals
        //    for (long iRow = 0; iRow < restRows; iRow++)
        //    {
        //        for (long iCol = 0; iCol < restCols; iCol++)
        //        {
        //            rp[mRow * Rows + iRow, mCol * Cols + iCol, false] = this[iRow, iCol, false];
        //        }
        //    }

        //    return rp;
        //}

        #endregion
        #region operators

        #region m-m add [real]
        /// <summary>
        /// computes the sum of two matrices x and y
        /// res[i,j] = x[i,j] + y[i,j]
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <param name="y"> input matrix y </param>
        /// <returns> result matrix </returns>
        public static MatrixD operator +(MatrixD x, MatrixD y)
            => VMath.Add(x, y);
        #endregion
        #region m-s add [real]
        /// <summary>
        /// adds a scalar s to matrix x
        /// res[i,j] = x[i,j] + s
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <param name="s"> input scalar s </param>
        /// <returns> result matrix </returns>
        public static MatrixD operator +(MatrixD x, double s)
            => VMath.Add(x, s);

        /// <summary>
        /// adds a scalar s to matrix x
        /// res[i,j] = x[i,j] + s
        /// </summary>
        /// <param name="s"> input scalar s </param>
        /// <param name="x"> input matrix x </param>
        /// <returns> result matrix </returns>
        public static MatrixD operator +(double s, MatrixD x)
            => (x + s);
        #endregion
        #region m-s add [mixed]
        /// <summary>
        /// adds a scalar s to matrix x
        /// res[i,j] = x[i,j] + s
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <param name="s"> input scalar s </param>
        /// <returns> result matrix </returns>
        public static MatrixZ operator +(MatrixD x, Complex s)
            => new MatrixZ(x) + s;

        /// <summary>
        /// adds a scalar s to matrix x
        /// res[i,j] = s + x[i,j]
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <param name="s"> input scalar s </param>
        /// <returns> result matrix </returns>
        public static MatrixZ operator +(Complex s, MatrixD x)
            => x + s;
        #endregion
        #region m-m subtract [real]
        /// <summary>
        /// subtracts one matrix y from another matrix x
        /// res[i,j] = x[i,j] - y[i,j]
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <param name="y"> input matrix y </param>
        /// <returns> result matrix </returns>
        public static MatrixD operator -(MatrixD x, MatrixD y)
            => VMath.Sub(x, y);
        #endregion
        #region m-s subtract [real]
        /// <summary>
        /// subtracts a scalar s from matrix x
        /// res[i,j] = x[i,j] - s
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <param name="s"> input scalar s </param>
        /// <returns> result matrix </returns>
        public static MatrixD operator -(MatrixD x, double s)
            => VMath.Sub(x, s);

        /// <summary>
        /// subtracts each element of matrix x from a scalar s
        /// res[i,j] = s - x[i,j]
        /// </summary>
        /// <param name="s"> input scalar s </param>
        /// <param name="x"> input matrix x </param>
        /// <returns> result matrix </returns>
        public static MatrixD operator -(double s, MatrixD x)
            => VMath.Sub(s, x);
        #endregion
        #region m-s subtract [mixed]
        /// <summary>
        /// substracts a scalar s from matrix x
        /// res[i,j] = x[i,j] - s
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <param name="s"> input scalar s </param>
        /// <returns> result matrix </returns>
        public static MatrixZ operator -(MatrixD x, Complex s)
            => new MatrixZ(x) - s;

        /// <summary>
        /// subtracts a scalar s from matrix x
        /// res[i,j] = s - x[i,j]
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <param name="s"> input scalar s </param>
        /// <returns> result matrix </returns>
        public static MatrixZ operator -(Complex s, MatrixD x)
            => s - new MatrixZ(x);
        #endregion
        #region m-m multiply [real]
        /// <summary>
        /// performs element by element multiplication 
        /// of two matrices x and y
        /// res[i,j] = x[i,j] * y[i,j]
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <param name="y"> input matrix y </param>
        /// <returns> result matrix </returns>
        public static MatrixD operator *(MatrixD x, MatrixD y)
            => VMath.Mul(x, y);
        #endregion
        #region m-s multiply [real]
        /// <summary>
        /// multiplies a matrix x with a scalar s
        /// res[i,j] = x[i,j] * s
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <param name="s"> input scalar s </param>
        /// <returns> result matrix </returns>
        public static MatrixD operator *(MatrixD x, double s)
        {
            MatrixD y = new (rows: x.Rows, cols: x.Cols, mode: ArrayInitMode.Malloc);
            VMath.Copy(x, ref y);
            VMath.ScaleOn(ref y, s);
            return y;
        }
            //=> VMath.Scale(x, s);

        /// <summary>
        /// multiplies a scalar s with a matrix x
        /// res[i,j] = s * x[i,j]
        /// </summary>
        /// <param name="s"> input scalar s </param>
        /// <param name="x"> input matrix x </param>
        /// <returns> result matrix </returns>
        public static MatrixD operator *(double s, MatrixD x)
            => (x * s);
        #endregion
        #region m-s multiply [mixed]
        /// <summary>
        /// multiplies a matrix x with a scalar s
        /// res[i,j] = x[i,j] * s
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <param name="s"> input scalar s </param>
        /// <returns> result matrix </returns>
        public static MatrixZ operator *(MatrixD x, Complex s)
            => new MatrixZ(x) * s;

        /// <summary>
        /// multiplies a scalar s with a matrix x
        /// res[i,j] = s * x[i,j]
        /// </summary>
        /// <param name="s"> input scalar s </param>
        /// <param name="x"> input matrix x </param>
        /// <returns> result matrix </returns>
        public static MatrixZ operator *(Complex s, MatrixD x)
            => x * s;
        #endregion
        #region m-m divide [real]
        /// <summary>
        /// performs element by element inversion 
        /// of two matrices x and y
        /// res[i,j] = x[i,j] / y[i,j]
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <param name="y"> input matrix y </param>
        /// <returns> result matrix </returns>
        public static MatrixD operator /(MatrixD x, MatrixD y)
            => VMath.Div(x, y);
        #endregion
        #region m-s divide [real]
        /// <summary>
        /// divides a matrix x by scalar s
        /// res[i,j] = x[i,j] / s
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <param name="s"> input scalar s </param>
        /// <returns> result matrix </returns>
        public static MatrixD operator /(MatrixD x, double s)
        {
            MatrixD y = new MatrixD(rows: x.Rows, cols: x.Cols, mode: ArrayInitMode.Malloc);
            VMath.Copy(x, ref y);
            VMath.ScaleOn(ref y, 1.0 / s);
            return y;
        }
            //=> VMath.Scale(x, 1.0 / s);

        /// <summary>
        /// divides a scalar s by matrix x
        /// res[i,j] = s / x[i,j]
        /// </summary>
        /// <param name="s"> input scalar s </param>
        /// <param name="x"> input matrix x </param>
        /// <returns> result matrix </returns>
        public static MatrixD operator /(double s, MatrixD x)
        {
            MatrixD res = VMath.Inv(x);
            VMath.ScaleOn(ref res, s);
            return res;
        }
        #endregion
        #region m-s divide [mixed]
        /// <summary>
        /// divides a matrix x by scalar s
        /// res[i,j] = x[i,j] / s
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <param name="s"> input scalar s </param>
        /// <returns> result matrix </returns>
        public static MatrixZ operator /(MatrixD x, Complex s)
            => new MatrixZ(x) / s;

        /// <summary>
        /// divides a scalar s by matrix x
        /// res[i,j] = s / x[i,j]
        /// </summary>
        /// <param name="s"> input scalar s </param>
        /// <param name="x"> input matrix x </param>
        /// <returns> result matrix </returns>
        public static MatrixZ operator /(Complex s, MatrixD x)
            => s / new MatrixZ(x);
        #endregion
        #region negative
        /// <summary>
        /// takes the negative of the input matrix
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <returns> result negative matrix </returns>
        public static MatrixD operator -(MatrixD x)
        {
            MatrixD y = new(rows: x.Rows, cols: x.Cols, mode: ArrayInitMode.Malloc);
            VMath.Copy(x, ref y);
            VMath.ScaleOn(ref y, -1.0);
            return y;
        }
            //=> VMath.Scale(x, -1.0);
        #endregion
        #region explicit conversion

        /// <summary>
        /// explicit conversion from MatrixD to MatrixZ
        /// </summary>
        /// <param name="x"> input real-part matrix </param>
        public static explicit operator MatrixZ(MatrixD x)
            => new(part: x, option: ComplexPart.RealPart);

        #endregion

        #endregion
    }

    /// <summary>
    /// complex matrix class
    /// </summary>
    public class MatrixZ : Matrix<Complex>
    {
        #region fields

        /// <summary>
        /// empty matrix with ZERO row and column count
        /// </summary>
        public static MatrixZ Empty = new(0, 0);

        #endregion
        #region properties

        /// <summary>
        /// gets the type-specific pointer to the values
        /// </summary>
        public unsafe Complex* SPtr
        {
            get => (Complex*)DataPtr.ToPointer();
        }

        ///// <summary>
        ///// gets the real part of the complex matrix
        ///// </summary>
        //public MatrixD RealPart => VMath.RealPart(this);

        ///// <summary>
        ///// gets the imaginary part of the complex matrix
        ///// </summary>
        //public MatrixD ImagPart => VMath.ImagPart(this);

        ///// <summary>
        ///// gets the magnitude of the complex matrix
        ///// </summary>
        //public MatrixD Magnitude => VMath.Abs(this);

        ///// <summary>
        ///// gets the argument of the complex matrix
        ///// </summary>
        //public MatrixD Argument => VMath.Arg(this);

        /// <summary>
        /// get / set a row slice from / into the matrix
        /// </summary>
        /// <param name="iRow"> row index [Int64]</param>
        /// <param name="colRng"> column range with inclusive start and exclusive end indices [Int64]  </param>
        /// <param name="loopMode"> loop computational mode option </param>
        /// <returns> row slice vector </returns>
        public new VectorZ this[long iRow, LongRange colRng,
            LoopMode loopMode = Defaults.LoopOption]
        {
            get
            {
                if (!IsRowIndexValid(iRow)) { throw new IndexOutOfRangeException("Invalid row index"); }
                if (!IsColRangeValid(colRng)) { throw new IndexOutOfRangeException("Invalid range"); }

                long n = colRng.End - colRng.Start;
                VectorZ x = new(count: n, mode: ArrayInitMode.Malloc);

                Action<long> a = (iCol) => { x[iCol - colRng.Start, false] = this[iRow, iCol, false]; };
                Loop1D loop = new(operation: a, start: colRng.Start, end: colRng.End);
                loop.Evaluate(loopMode);

                return x;
            }
            set
            {
                //if (value.GetType() != typeof(VectorZ)) { throw new ArgumentException("Invalid input", nameof(value)); }
                if (!IsRowIndexValid(iRow)) { throw new IndexOutOfRangeException("Invalid row index"); }
                if (!IsColRangeValid(colRng)) { throw new IndexOutOfRangeException("Invalid range"); }

                Action<long> a = (iCol) => { this[iRow, iCol, false] = value[iCol - colRng.Start, false]; };
                Loop1D loop = new(operation: a, start: colRng.Start, end: colRng.End);
                loop.Evaluate(loopMode);
            }
        }

        /// <summary>
        /// get / set a row slice from / into the matrix
        /// </summary>
        /// <param name="iRow"> row index [Int32]</param>
        /// <param name="colRng"> column range with inclusive start and exclusive end indices [Int32]  </param>
        /// <param name="loopMode"> loop computational mode option </param>
        /// <returns> row slice vector </returns>
        public new VectorZ this[Index iRow, Range colRng,
            LoopMode loopMode = Defaults.LoopOption]
        {
            get => this[IndexToInt(iRow, Rows), new LongRange(colRng, Cols), loopMode];
            set => this[IndexToInt(iRow, Rows), new LongRange(colRng, Cols), loopMode] = value;
        }

        /// <summary>
        /// get / set a column slice from / into the matrix
        /// </summary>
        /// <param name="rowRng"> row range with inclusive start and exclusive end indices [Int64] </param>
        /// <param name="iCol"> column index [Int64] </param>
        /// <param name="loopMode"> loop computational mode option </param>
        /// <returns> column slice vector </returns>
        public new VectorZ this[LongRange rowRng, long iCol,
            LoopMode loopMode = Defaults.LoopOption]
        {
            get
            {
                if (!IsColIndexValid(iCol)) { throw new IndexOutOfRangeException("Invalid column index"); }
                if (!IsRowRangeValid(rowRng)) { throw new IndexOutOfRangeException("Invalid range"); }

                long n = rowRng.End - rowRng.Start;
                VectorZ x = new(count: n, mode: ArrayInitMode.Malloc);

                Action<long> a = (iRow) => { x[iRow - rowRng.Start, false] = this[iRow, iCol, false]; };
                Loop1D loop = new(operation: a, start: rowRng.Start, end: rowRng.End);
                loop.Evaluate(loopMode);

                return x;
            }
            set
            {
                //if (value.GetType() != typeof(VectorZ)) { throw new ArgumentException("Invalid input", nameof(value)); }
                if (!IsColIndexValid(iCol)) { throw new IndexOutOfRangeException("Invalid column index"); }
                if (!IsRowRangeValid(rowRng)) { throw new IndexOutOfRangeException("Invalid range"); }

                Action<long> a = (iRow) => { this[iRow, iCol, false] = value[iRow - rowRng.Start, false]; };
                Loop1D loop = new(operation: a, start: rowRng.Start, end: rowRng.End);
                loop.Evaluate(loopMode);
            }
        }

        /// <summary>
        /// get / set a column slice from / into the matrix
        /// </summary>
        /// <param name="rowRng"> row range with inclusive start and exclusive end indices [Int32] </param>
        /// <param name="iCol"> column index [Int32] </param>
        /// <param name="loopMode"> loop computational mode option </param>
        /// <returns> column slice vector </returns>
        public new VectorZ this[Range rowRng, Index iCol,
            LoopMode loopMode = Defaults.LoopOption]
        {
            get => this[new LongRange(rowRng, Rows), IndexToInt(iCol, Cols), loopMode];
            set => this[new LongRange(rowRng, Rows), IndexToInt(iCol, Cols), loopMode] = value;
        }

        /// <summary>
        /// get / set a block from / into the matrix
        /// </summary>
        /// <param name="rowRng"> row range with inclusive start and exclusive end indices [Int64] </param>
        /// <param name="colRng"> column range with inclusive start and exclusive end indices [Int64] </param>
        /// <param name="loopMode"> loop computational mode option </param>
        /// <returns> sub-matrix </returns>
        public new MatrixZ this[LongRange rowRng, LongRange colRng,
            LoopMode loopMode = Defaults.LoopOption]
        {
            get
            {
                if (!IsRowRangeValid(rowRng)) { throw new IndexOutOfRangeException("Invalid range"); }
                if (!IsColRangeValid(colRng)) { throw new IndexOutOfRangeException("Invalid range"); }

                long rows = rowRng.End - rowRng.Start;
                long cols = colRng.End - colRng.Start;
                MatrixZ x = new(rows: rows, cols: cols, mode: ArrayInitMode.Malloc);

                Action<long, long> a = (iRow, iCol) => { x[iRow - rowRng.Start, iCol - colRng.Start, false] = this[iRow, iCol, false]; };
                Loop2D loop = new(operation: a,
                    rowStart: rowRng.Start, rowEnd: rowRng.End,
                    colStart: colRng.Start, colEnd: colRng.End);
                loop.Evaluate(loopMode);

                return x;
            }
            set
            {
                //if (value.GetType() != typeof(MatrixZ)) { throw new ArgumentException("Invalid input", nameof(value)); }
                if (!IsRowRangeValid(rowRng)) { throw new IndexOutOfRangeException("Invalid range"); }
                if (!IsColRangeValid(colRng)) { throw new IndexOutOfRangeException("Invalid range"); }

                Action<long, long> a = (iRow, iCol) => { this[iRow, iCol, false] = value[iRow - rowRng.Start, iCol - colRng.Start, false]; };
                Loop2D loop = new(operation: a,
                    rowStart: rowRng.Start, rowEnd: rowRng.End,
                    colStart: colRng.Start, colEnd: colRng.End);
                loop.Evaluate(loopMode);
            }
        }

        /// <summary>
        /// get / set a block from / into the matrix
        /// </summary>
        /// <param name="rowRng"> row range with inclusive start and exclusive end indices [Int32] </param>
        /// <param name="colRng"> column range with inclusive start and exclusive end indices [Int32] </param>
        /// <param name="loopMode"> loop computational mode option </param>
        /// <returns> sub-matrix </returns>
        public new MatrixZ this[Range rowRng, Range colRng,
            LoopMode loopMode = Defaults.LoopOption]
        {
            get => this[new LongRange(rowRng, Rows), new LongRange(colRng, Cols), loopMode];
            set => this[new LongRange(rowRng, Rows), new LongRange(colRng, Cols), loopMode] = value;
        }

        ///// <summary>
        ///// get / set the value of a matrix element 
        ///// using linear index (w.r.t. total element count)
        ///// </summary>
        ///// <param name="i"> index of the element [Int64] </param>
        ///// <param name="checkBound"> whether to check if the index is outside bound </param>
        ///// <returns> element value </returns>
        //internal unsafe Complex this[long i, bool checkBound = true]
        //{
        //    get
        //    {
        //        bool invalidIndex = checkBound && !IsIndexValid(i, Count);
        //        if (invalidIndex) { throw new IndexOutOfRangeException(); }

        //        Complex* iPtr = (Complex*)DataPtr.ToPointer();
        //        Complex iVal = *(iPtr + i);
        //        return iVal;
        //    }
        //    set
        //    {
        //        bool invalidIndex = checkBound && !IsIndexValid(i, Count);
        //        if (invalidIndex) { throw new IndexOutOfRangeException(); }

        //        Complex* iPtr = (Complex*)DataPtr.ToPointer();
        //        *(iPtr + i) = value;
        //    }
        //}

        ///// <summary>
        ///// get / set the value of a matrix element 
        ///// using linear index (w.r.t. total element count)
        ///// </summary>
        ///// <param name="i"> index of the element [Int32] </param>
        ///// <param name="checkBound"> whether to check if the index is outside bound </param>
        ///// <returns> element value </returns>
        //internal Complex this[int i, bool checkBound = true]
        //{
        //    get => this[(long)i];
        //    set => this[(long)i] = value;
        //}

        ///// <summary>
        ///// get / set the value of a matrix element 
        ///// using linear index (w.r.t. total element count)
        ///// </summary>
        ///// <param name="i"> index of the element [Int32] </param>
        ///// <param name="checkBound"> whether to check if the index is outside bound </param>
        ///// <returns> element value </returns>
        //internal Complex this[Index i, bool checkBound = true]
        //{
        //    get => this[IndexToInt(i, Count), checkBound];
        //    set => this[IndexToInt(i, Count), checkBound] = value;
        //}

        #endregion
        #region constructor

        /// <summary>
        /// constructs a matrix with given length
        /// by default, does not initialize element values
        /// </summary>
        /// <param name="rows"> number of rows </param>
        /// <param name="cols"> number of columns </param>
        /// <param name="mode"> construct mode option; default is CALLOC </param>
        public MatrixZ(long rows, long cols,
            ArrayInitMode mode = ArrayInitMode.Calloc)
            : base(rows, cols, mode) { }

        /// <summary>
        /// constructs a matrix with given size
        /// and sets all elements to the same value 
        /// </summary>
        /// <param name="rows"> number of rows </param>
        /// <param name="cols"> number of cols </param>
        /// <param name="initVal"> initial value for all the elements </param>
        /// <param name="loopMode"> loop-computation mode option </param>
        public MatrixZ(long rows, long cols, Complex initVal,
            LoopMode loopMode = Defaults.LoopOption)
            : base(rows, cols, initVal) { }

        /// <summary>
        /// constructs a matrix with given size
        /// and sets all elements to the same value 
        /// </summary>
        /// <param name="rows"> number of rows </param>
        /// <param name="cols"> number of cols </param>
        /// <param name="initVal"> initial value for all the elements </param>
        /// <param name="loopMode"> loop-computation mode option </param>
        public MatrixZ(long rows, long cols, double initVal,
            LoopMode loopMode = Defaults.LoopOption) :
            this(rows, cols, new Complex(initVal, 0.0), loopMode)
        { }

        ///// <summary>
        ///// constructs a matrix with given size,
        ///// initial value and increment between two elements
        ///// </summary>
        ///// <param name="rows"> number of rows </param>
        ///// <param name="cols"> number of columns </param>
        ///// <param name="initVal"> initial value of the first element </param>
        ///// <param name="increment"> increment between two elements </param>
        ///// <param name="loopMode"> loop-computation mode option </param>
        //public MatrixZ(long rows, long cols, Complex initVal, Complex increment,
        //    LoopMode loopMode = Defaults.LoopOption)
        //    : this(rows, cols)
        //{
        //    Action<long> a = (i) => { this[i, false] = initVal + i * increment; };
        //    Loop1D loop = new(operation: a, start: 0, end: Count);
        //    loop.Evaluate(loopMode);
        //}

        ///// <summary>
        ///// constructs a matrix with given size,
        ///// m[iRow, iCol] = initVal + iRow * incRow + iCol * incCol
        ///// </summary>
        ///// <param name="rows"> number of rows </param>
        ///// <param name="cols"> number of columns </param>
        ///// <param name="initVal"> initial value for the first element </param>
        ///// <param name="incRow"> increment between two rows </param>
        ///// <param name="incCol"> increment between two columns </param>
        ///// <param name="loopMode"> loop-computation mode option </param>
        //public MatrixZ(long rows, long cols, Complex initVal,
        //    Complex incRow, Complex incCol,
        //    LoopMode loopMode = Defaults.LoopOption)
        //    : this(rows, cols)
        //{
        //    Action<long, long> a = (iRow, iCol) =>
        //    { this[iRow, iCol, false] = initVal + iRow * incRow + iCol * incCol; };
        //    Loop2D loop = new(operation: a, rowStart: 0, rowEnd: Rows, colStart: 0, colEnd: Cols);
        //    loop.Evaluate(loopMode);
        //}

        ///// <summary>
        ///// constructs a matrix with given size,
        ///// initial values for each row, and the 
        ///// increment between two columns
        ///// </summary>
        ///// <param name="rows"> number of rows </param>
        ///// <param name="cols"> number of columns </param>
        ///// <param name="rowInits"> initial values for each row </param>
        ///// <param name="colIncrs"> increment between two columns </param>
        ///// <param name="loopMode"> loop-computation mode option </param>
        //public MatrixZ(long rows, long cols,
        //    VectorZ rowInits, Complex colIncrs,
        //    LoopMode loopMode = Defaults.LoopOption)
        //    : this(rows, cols)
        //{
        //    if (rowInits.Count != rows) { throw new NotSupportedException("Length of initVals vector unequal to matrix rows"); }

        //    Action<long, long> a = (iRow, iCol) =>
        //    { this[iRow, iCol, false] = rowInits[iRow, false] + iCol * colIncrs; };
        //    Loop2D loop = new(operation: a, rowStart: 0, rowEnd: Rows, colStart: 0, colEnd: Cols);
        //    loop.Evaluate(loopMode);
        //}

        ///// <summary>
        ///// constructs a matrix with given size,
        ///// initial values for each row, and the 
        ///// increment between two elements in each row
        ///// </summary>
        ///// <param name="rows"> number of rows </param>
        ///// <param name="cols"> number of columns </param>
        ///// <param name="rowInits"> initial values for each row </param>
        ///// <param name="colIncrs"> increment between two elements for each row </param>
        //public MatrixZ(long rows, long cols,
        //    VectorZ rowInits, VectorZ colIncrs)
        //    : this(rows, cols)
        //{
        //    if (rowInits.Count != rows || colIncrs.Count != rows) { throw new NotSupportedException("Length of initVals/increments vector unequal to matrix rows"); }

        //    Action<long, long> a = (iRow, iCol) =>
        //    { this[iRow, iCol, false] = rowInits[iRow, false] + iCol * colIncrs[iRow, false]; };
        //    Loop2D loop = new(operation: a, rowStart: 0, rowEnd: Rows, colStart: 0, colEnd: Cols);
        //    loop.Evaluate();
        //}

        /// <summary>
        /// constructs a matrix by copying from another
        /// </summary>
        /// <param name="other"> another matrix </param>
        /// <param name="deepCopy"> copy mode option; default is deep copy </param>
        public MatrixZ(MatrixZ other, bool deepCopy = true)
            : this(other.Rows, other.Cols, deepCopy ? ArrayInitMode.Malloc : ArrayInitMode.None)
        {
            if (deepCopy)
            {
                MatrixZ t = this;
                //VMath.CopyZ(other, ref t);
                unsafe { Defaults.IBLAS.Copy(n: other.Count, x: other.VPtr, 
                    y: t.VPtr, incx: 1, incy: 1); }
            }
            else
            { DataPtr = other.DataPtr; }
        }

        /// <summary>
        /// constructs a matrix by copying from a given ArrayBase
        /// </summary>
        /// <param name="other"> another ArrayBase </param>
        /// <param name="rows"> number of rows </param>
        /// <param name="cols"> number of columns </param>
        /// <param name="deepCopy"> copy mode option; default is deep copy </param>
        public MatrixZ(DenseArrayBase<Complex> other, long rows, long cols, bool deepCopy = true)
            : this(rows, cols, deepCopy ? ArrayInitMode.Malloc : ArrayInitMode.None)
        {
            if (deepCopy)
            {
                DenseArrayBase<Complex> t = this;
                //VMath.CopyZ(other, ref t);
                unsafe { Defaults.IBLAS.Copy(n: other.Count, x: other.VPtr, 
                    y: t.VPtr, incx: 1, incy: 1); }
            }
            else
            { DataPtr = other.DataPtr; }
        }

        /// <summary>
        /// constructs a complex matrix 
        /// with its real or imaginary part only
        /// </summary>
        /// <param name="part"> part of the matrix </param>
        /// <param name="option"> option for the complex part; default is real-part </param>
        public MatrixZ(MatrixD part, ComplexPart option = ComplexPart.RealPart)
            : this(part.Rows, part.Cols, 0.0)
        {
            switch (option)
            {
                case ComplexPart.RealPart:
                    {
                        MatrixZ t = this;
                        VMath.ModifyReal(part, ref t);
                        break;
                    }
                case ComplexPart.ImagPart:
                    {
                        MatrixZ t = this;
                        VMath.ModifyImag(part, ref t);
                        break;
                    }
                default: goto case ComplexPart.RealPart;
            }
        }

        #endregion
        #region naive methods

        ///// <summary>
        ///// sums up all the elements
        ///// </summary>
        ///// <returns> summation result </returns>
        //public Complex Sum(LoopMode mode = Defaults.LoopOption)
        //    => VMath.Sum(x: this, loopMode: mode);

        ///// <summary>
        ///// converts to array
        ///// </summary>
        ///// <returns> result array </returns>
        //public Complex[,] ToArray(LoopMode mode = Defaults.LoopOption)
        //    => VMath.ConvertMatrixToArray(x: this, loopMode: mode);

        #endregion
        #region extra methods

        ///// <summary>
        ///// checks whether a given row index 
        ///// is within [0, Rows)
        ///// </summary>
        ///// <param name="i"> input row index </param>
        ///// <returns> return true when valid </returns>
        //public new bool IsRowIndexValid(long i)
        //    => IsIndexValid(i, Rows, GetType().Name);

        ///// <summary>
        ///// checks whether a given row range 
        ///// is within [0, Rows)
        ///// </summary>
        ///// <param name="rng"> input row range </param>
        ///// <returns> return true when valid </returns>
        //public new bool IsRowRangeValid(LongRange rng)
        //    => IsRangeValid(rng, Rows, GetType().Name);

        ///// <summary>
        ///// checks whether a given column index 
        ///// is within [0, Cols)
        ///// </summary>
        ///// <param name="i"> input column index </param>
        ///// <returns> return true when valid </returns>
        //public new bool IsColIndexValid(long i)
        //    => IsIndexValid(i, Cols, GetType().Name);

        ///// <summary>
        ///// checks whether a given column range 
        ///// is within [0, Cols)
        ///// </summary>
        ///// <param name="rng"> input column range </param>
        ///// <returns> return true when valid </returns>
        //public new bool IsColRangeValid(LongRange rng)
        //    => IsRangeValid(rng, Cols, GetType().Name);

        ///// <summary>
        ///// checks whether this matrix has
        ///// the same dimension as the other
        ///// </summary>
        ///// <param name="other"> the other matrix </param>
        ///// <returns> return true if same; otherwise false </returns>
        //public bool HasSameDimension(MatrixD other)
        //{
        //    if (other.Rows == Rows && other.Cols == Cols)
        //        return true;
        //    else
        //        Console.Write("Unequal matrix dimension");
        //    return false;
        //}

        ///// <summary>
        ///// checks whether this matrix has
        ///// the same dimension as the other
        ///// </summary>
        ///// <param name="other"> the other matrix </param>
        ///// <returns> return true if same; otherwise false </returns>
        //public bool HasSameDimension(MatrixZ other)
        //{
        //    if (other.Rows == Rows && other.Cols == Cols)
        //        return true;
        //    else
        //        Console.Write("Unequal matrix dimension");
        //    return false;
        //}

        /// <summary>
        /// padding according to target matrix parameters
        /// </summary>
        /// <param name="targetRows"> target number of rows in the padded matrix </param>
        /// <param name="targetCols"> target number of columns in the padded matrix </param>
        /// <param name="startRowIndex"> starting row index in the padded matrix </param>
        /// <param name="startColIndex"> starting column index in the padded matrix </param>
        /// <param name="paddingValueRe"> real-part of the padding value </param>
        /// <param name="paddingValueIm"> imag-part of the padding value </param>
        /// <returns> result matrix after padding </returns>
        public MatrixZ Padding(long targetRows, long targetCols,
            long startRowIndex, long startColIndex,
            double paddingValueRe = 0.0, double paddingValueIm = 0.0)
        {
            if (targetRows <= Rows)
            {
                Printer.Warning($"{nameof(targetRows)} must be greater than the current value");
                return Empty;
            }
            if (targetCols <= Cols)
            {
                Printer.Warning($"{nameof(targetCols)} must be greater than the current value");
                return Empty;
            }

            Complex paddingValue = new(paddingValueRe, paddingValueIm);
            MatrixZ y = new(targetRows, targetCols, paddingValue);
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
        public MatrixZ Padding(long targetRows, long targetCols)
        {
            if ((targetRows - Rows) % 2 != 0)
            {
                Printer.Warning($"{nameof(targetRows)} must be an even addition to the current value");
                return Empty;
            }
            if ((targetCols - Cols) % 2 != 0)
            {
                Printer.Warning($"{nameof(targetCols)} must be an even addition to the current value");
                return Empty;
            }
            return Padding(targetRows, targetCols, (targetRows - Rows) / 2, (targetCols - Cols) / 2, 0.0, 0.0);
        }

        /// <summary>
        /// truncates current matrix according to target parameters
        /// </summary>
        /// <param name="targetRows"> target number of rows in the truncated matrix </param>
        /// <param name="targetCols"> target number of columns in the truncated matrix </param>
        /// <param name="startRowIndex"> starting row index in the original matrix </param>
        /// <param name="startColIndex"> starting column index in the original matrix </param>
        /// <returns> result matrix after truncation </returns>
        public MatrixZ Truncate(long targetRows, long targetCols,
            long startRowIndex, long startColIndex)
        {
            if (startRowIndex + targetRows >= Rows)
            {
                Printer.Warning($"invalid combination of parameters {nameof(targetRows)} and {nameof(startRowIndex)}");
                return Empty;
            }
            if (startColIndex + targetCols >= Cols)
            {
                Printer.Warning($"invalid combination of parameters {nameof(targetCols)} and {nameof(startColIndex)}");
                return Empty;
            }

            LongRange rowRng = new(startRowIndex, startRowIndex + targetRows);
            LongRange colRng = new(startColIndex, startColIndex + targetCols);
            return this[rowRng, colRng];
        }

        /// <summary>
        /// centered truncation on each side of the matrix
        /// </summary>
        /// <param name="targetRows"> target number of rows </param>
        /// <param name="targetCols"> target number of columns </param>
        /// <returns> result matrix after truncation </returns>
        public MatrixZ Truncate(long targetRows, long targetCols)
        {
            if ((Rows - targetRows) % 2 != 0)
            {
                Printer.Warning($"{nameof(targetRows)} must be an even subtraction of the current value");
                return Empty;
            }
            if ((Cols - targetCols) % 2 != 0)
            {
                Printer.Warning($"{nameof(targetCols)} must be an even subtraction of the current value");
                return Empty;
            }
            return Truncate(targetRows, targetCols, (Rows - targetRows) / 2, (Cols - targetCols) / 2);
        }

        /// <summary>
        /// replicates according to target number of rows and columns
        /// </summary>
        /// <param name="targetRows"> target number of rows </param>
        /// <param name="targetCols"> target number of columns </param>
        /// <returns> replicated result </returns>
        public MatrixZ Replicate(long targetRows, long targetCols)
        {
            if (targetRows <= Rows) { Printer.Warning($"Target number of rows not greater than the current"); }
            if (targetCols <= Cols) { Printer.Warning($"Target number of columns not greater than the current"); }
            MatrixZ rp = new(targetRows, targetCols);
            // computes replication multiples and residual counts
            long mRow = (long)(targetRows / Rows);
            long mCol = (long)(targetCols / Cols);
            long restRows = targetRows - mRow * Rows;
            long restCols = targetCols = mCol * Cols;
            // loop for multiple replications
            for (long nRow = 0; nRow < mRow; nRow++)
            {
                for (long nCol = 0; nCol < mCol; nCol++)
                {
                    LongRange uRow = new(nRow * Rows + 0, nRow * Rows + Rows);
                    LongRange uCol = new(nCol * Cols + 0, nCol * Cols + Cols);
                    rp[uRow, uCol] = this;
                }
            }
            // loop for the residuals
            for (long iRow = 0; iRow < restRows; iRow++)
            {
                for (long iCol = 0; iCol < restCols; iCol++)
                {
                    rp[mRow * Rows + iRow, mCol * Cols + iCol, false] = this[iRow, iCol, false];
                }
            }

            return rp;
        }

        #endregion
        #region operators

        #region m-m add [complex]
        /// <summary>
        /// computes the sum of two matrices x and y
        /// res[i,j] = x[i,j] + y[i,j]
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <param name="y"> input matrix y </param>
        /// <returns> result matrix </returns>
        public static MatrixZ operator +(MatrixZ x, MatrixZ y)
            => VMath.Add(x, y);
        #endregion
        #region m-m add [mixed]
        /// <summary>
        /// computes the sum of two matrices x and y
        /// res[i,j] = x[i,j] + y[i,j]
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <param name="y"> input matrix y </param>
        /// <returns> result matrix </returns>
        public static MatrixZ operator +(MatrixZ x, MatrixD y)
            => x + new MatrixZ(y);

        /// <summary>
        /// computes the sum of two matrices x and y
        /// res[i,j] = x[i,j] + y[i,j]
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <param name="y"> input matrix y </param>
        /// <returns> result matrix </returns>
        public static MatrixZ operator +(MatrixD x, MatrixZ y)
            => y + x;
        #endregion
        #region m-s add [complex]
        /// <summary>
        /// adds a scalar s to matrix x
        /// res[i,j] = x[i,j] + s
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <param name="s"> input scalar s </param>
        /// <returns> result matrix </returns>
        public static MatrixZ operator +(MatrixZ x, Complex s)
            => VMath.Add(x, s);

        /// <summary>
        /// adds a scalar s to matrix x
        /// res[i,j] = x[i,j] + s
        /// </summary>
        /// <param name="s"> input scalar s </param>
        /// <param name="x"> input matrix x </param>
        /// <returns> result matrix </returns>
        public static MatrixZ operator +(Complex s, MatrixZ x)
            => (x + s);
        #endregion
        #region m-s add [mixed]
        /// <summary>
        /// adds a scalar s to matrix x
        /// res[i,j] = x[i,j] + s
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <param name="s"> input scalar s </param>
        /// <returns> result matrix </returns>
        public static MatrixZ operator +(MatrixZ x, double s)
            => x + new Complex(s, 0.0);

        /// <summary>
        /// adds a scalar s to matrix x
        /// res[i,j] = x[i,j] + s
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <param name="s"> input scalar s </param>
        /// <returns> result matrix </returns>
        public static MatrixZ operator +(double s, MatrixZ x)
            => x + s;
        #endregion
        #region m-m subtract [complex]
        /// <summary>
        /// subtracts one matrix y from another matrix x
        /// res[i,j] = x[i,j] - y[i,j]
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <param name="y"> input matrix y </param>
        /// <returns> result matrix </returns>
        public static MatrixZ operator -(MatrixZ x, MatrixZ y)
            => VMath.Sub(x, y);
        #endregion
        #region m-m subtract [mixed]
        /// <summary>
        /// subtracts one matrix y from another matrix x
        /// res[i,j] = x[i,j] - y[i,j]
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <param name="y"> input matrix y </param>
        /// <returns> result matrix </returns>
        public static MatrixZ operator -(MatrixZ x, MatrixD y)
            => x - new MatrixZ(y);

        /// <summary>
        /// substracts one matrix y from another matrix x
        /// res[i,j] = x[i,j] - y[i,j]
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <param name="y"> input matrix y </param>
        /// <returns> result matrix </returns>
        public static MatrixZ operator -(MatrixD x, MatrixZ y)
            => new MatrixZ(x) - y;
        #endregion
        #region m-s subtract [complex]
        /// <summary>
        /// subtracts a scalar s from matrix x
        /// res[i,j] = x[i,j] - s
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <param name="s"> input scalar s </param>
        /// <returns> result matrix </returns>
        public static MatrixZ operator -(MatrixZ x, Complex s)
            => VMath.Sub(x, s);

        /// <summary>
        /// subtracts each element of matrix x from a scalar s
        /// res[i,j] = s - x[i,j]
        /// </summary>
        /// <param name="s"> input scalar s </param>
        /// <param name="x"> input matrix x </param>
        /// <returns> result matrix </returns>
        public static MatrixZ operator -(Complex s, MatrixZ x)
            => VMath.Sub(s, x);
        #endregion
        #region m-s subtract [mixed]
        /// <summary>
        /// subtracts a scalar s from matrix x
        /// res[i,j] = x[i,j] - s
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <param name="s"> input scalar s </param>
        /// <returns> result matrix </returns>
        public static MatrixZ operator -(MatrixZ x, double s)
            => x - new Complex(s, 0.0);

        /// <summary>
        /// subtracts each element of matrix x from a scalar s
        /// res[i,j] = s - x[i,j]
        /// </summary>
        /// <param name="s"> input scalar s </param>
        /// <param name="x"> input matrix x </param>
        /// <returns> result matrix </returns>
        public static MatrixZ operator -(double s, MatrixZ x)
            => new Complex(s, 0.0) - x;
        #endregion
        #region m-m multiply [complex]
        /// <summary>
        /// performs element by element multiplication 
        /// of two matrices x and y
        /// res[i,j] = x[i,j] * y[i,j]
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <param name="y"> input matrix y </param>
        /// <returns> result matrix </returns>
        public static MatrixZ operator *(MatrixZ x, MatrixZ y)
            => VMath.Mul(x, y);
        #endregion
        #region m-m multiply [mixed]
        /// <summary>
        /// performs element by element multiplication 
        /// of two matrices x and y
        /// res[i,j] = x[i,j] * y[i,j]
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <param name="y"> input matrix y </param>
        /// <returns> result matrix </returns>
        public static MatrixZ operator *(MatrixZ x, MatrixD y)
            => x * new MatrixZ(y);

        /// <summary>
        /// performs element by element multiplication 
        /// of two matrices x and y
        /// res[i,j] = x[i,j] * y[i,j]
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <param name="y"> input matrix y </param>
        /// <returns> result matrix </returns>
        public static MatrixZ operator *(MatrixD x, MatrixZ y)
            => y * x;
        #endregion
        #region m-s multiply [complex]
        /// <summary>
        /// multiplies a matrix x with a scalar s
        /// res[i,j] = x[i,j] * s
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <param name="s"> input scalar s </param>
        /// <returns> result matrix </returns>
        public static MatrixZ operator *(MatrixZ x, Complex s)
        {
            MatrixZ y = new MatrixZ(rows: x.Rows, cols: x.Cols, mode: ArrayInitMode.Malloc);
            VMath.Copy(x, ref y);
            VMath.ScaleOn(ref y, s);
            return y;
        }
            //=> VMath.Scale(x, s);

        /// <summary>
        /// multiplies a scalar s with a matrix x
        /// res[i,j] = s * x[i,j]
        /// </summary>
        /// <param name="s"> input scalar s </param>
        /// <param name="x"> input matrix x </param>
        /// <returns> result matrix </returns>
        public static MatrixZ operator *(Complex s, MatrixZ x)
            => (x * s);
        #endregion
        #region m-s multiply [mixed]
        /// <summary>
        /// multiplies a matrix x with a scalar s
        /// res[i,j] = x[i,j] * s
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <param name="s"> input scalar s </param>
        /// <returns> result matrix </returns>
        public static MatrixZ operator *(MatrixZ x, double s)
            => x * new Complex(s, 0.0);

        /// <summary>
        /// multiplies a scalar s with a matrix x
        /// res[i,j] = s * x[i,j]
        /// </summary>
        /// <param name="s"> input scalar s </param>
        /// <param name="x"> input matrix x </param>
        /// <returns> result matrix </returns>
        public static MatrixZ operator *(double s, MatrixZ x)
            => x * s;
        #endregion
        #region m-m divide [complex]
        /// <summary>
        /// performs element by element inversion 
        /// of two matrices x and y
        /// res[i,j] = x[i,j] / y[i,j]
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <param name="y"> input matrix y </param>
        /// <returns> result matrix </returns>
        public static MatrixZ operator /(MatrixZ x, MatrixZ y)
            => VMath.Div(x, y);
        #endregion
        #region m-m divide [mixed]
        /// <summary>
        /// performs element by element inversion 
        /// of two matrices x and y
        /// res[i,j] = x[i,j] / y[i,j]
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <param name="y"> input matrix y </param>
        /// <returns> result matrix </returns>
        public static MatrixZ operator /(MatrixZ x, MatrixD y)
            => x / new MatrixZ(y);

        /// <summary>
        /// performs element by element inversion 
        /// of two matrices x and y
        /// res[i,j] = x[i,j] / y[i,j]
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <param name="y"> input matrix y </param>
        /// <returns> result matrix </returns>
        public static MatrixZ operator /(MatrixD x, MatrixZ y)
            => new MatrixZ(x) / y;
        #endregion
        #region m-s divide [complex]
        /// <summary>
        /// divides a matrix x by scalar s
        /// res[i,j] = x[i,j] / s
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <param name="s"> input scalar s </param>
        /// <returns> result matrix </returns>
        public static MatrixZ operator /(MatrixZ x, Complex s)
        {
            MatrixZ y = new MatrixZ(rows: x.Rows, cols: x.Cols, mode: ArrayInitMode.Malloc);
            VMath.Copy(x, ref y);
            VMath.ScaleOn(ref y, 1.0 / s);
            return y;
        }
            //=> VMath.Scale(x, 1.0 / s);

        /// <summary>
        /// divides a scalar s by matrix x
        /// res[i,j] = s / x[i,j]
        /// </summary>
        /// <param name="s"> input scalar s </param>
        /// <param name="x"> input matrix x </param>
        /// <returns> result matrix </returns>
        public static MatrixZ operator /(Complex s, MatrixZ x)
        {
            MatrixZ res = new(x.Rows, x.Cols, s);
            return VMath.Div(res, x);
        }
        #endregion
        #region m-s divide [mixed]
        /// <summary>
        /// divides a matrix x by scalar s
        /// res[i,j] = x[i,j] / s
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <param name="s"> input scalar s </param>
        /// <returns> result matrix </returns>
        public static MatrixZ operator /(MatrixZ x, double s)
            => x / new Complex(s, 0.0);

        /// <summary>
        /// divides a scalar s by matrix x
        /// res[i,j] = s / x[i,j]
        /// </summary>
        /// <param name="s"> input scalar s </param>
        /// <param name="x"> input matrix x </param>
        /// <returns> result matrix </returns>
        public static MatrixZ operator /(double s, MatrixZ x)
            => new Complex(s, 0.0) / x;
        #endregion
        #region negative
        /// <summary>
        /// takes the negative of the input matrix
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <returns> result negative matrix </returns>
        public static MatrixZ operator -(MatrixZ x)
        {
            MatrixZ y = new MatrixZ(rows: x.Rows, cols: x.Cols, mode: ArrayInitMode.Malloc);
            VMath.Copy(x, ref y);
            VMath.ScaleOn(ref y, -1.0);
            return y;
        }
            //=> VMath.Scale(x, -1.0);
        #endregion

        #endregion
    }


    /// <summary>
    /// 2x2-dimension real-valued matrix class
    /// </summary>
    public class MatD2x2
    {
        #region fields

        //public static MatD2x2 Zeros = new()

        #endregion
        #region properties

        /// <summary>
        /// matrix element (1,1)
        /// </summary>
        public double M11 { get; set; }

        /// <summary>
        /// matrix element (1,2)
        /// </summary>
        public double M12 { get; set; }

        /// <summary>
        /// matrix element (2,1)
        /// </summary>
        public double M21 { get; set; }

        /// <summary>
        /// matrix element (2,2)
        /// </summary>
        public double M22 { get; set; }

        /// <summary>
        /// first row of the matrix
        /// </summary>
        public VecD2 Row1
        {
            get => new() { X = M11, Y = M12 };
            set
            {
                M11 = value.X;
                M12 = value.Y;
            }
        }

        /// <summary>
        /// second row of the matrix
        /// </summary>
        public VecD2 Row2
        {
            get => new() { X = M21, Y = M22 };
            set
            {
                M21 = value.X;
                M22 = value.Y;
            }
        }

        /// <summary>
        /// first column of the matrix
        /// </summary>
        public VecD2 Col1
        {
            get => new() { X = M11, Y = M21 };
            set
            {
                M11 = value.X;
                M21 = value.Y;
            }
        }

        /// <summary>
        /// second column of the matrix
        /// </summary>
        public VecD2 Col2
        {
            get => new() { X = M21, Y = M22 };
            set
            {
                M21 = value.X;
                M22 = value.Y;
            }
        }

        #endregion
        #region constructors

        /// <summary>
        /// constructs a MatD2x2
        /// </summary>
        /// <param name="m11"> matrix element (1,1) </param>
        /// <param name="m12"> matrix element (1,2) </param>
        /// <param name="m21"> matrix element (2,1) </param>
        /// <param name="m22"> matrix element (2,2) </param>
        public MatD2x2(double m11, double m12, double m21, double m22)
        {
            M11 = m11; M12 = m12;
            M21 = m21; M22 = m22;
        }

        /// <summary>
        /// constructs a MatD2x2 by copying from another
        /// </summary>
        /// <param name="a"> another matrix </param>
        public MatD2x2(MatD2x2 a)
        {
            M11 = a.M11; M12 = a.M12;
            M21 = a.M21; M22 = a.M22;
        }

        /// <summary>
        /// constructs a default MatD2x2
        /// with all elements set to zero
        /// </summary>
        public MatD2x2() : this(0.0, 0.0, 0.0, 0.0) { }

        #endregion
        #region methods


        private void Test()
        {
            VecD2 v = new(1.0, 2.0);

        }

        #endregion
        #region static methods

        /// <summary>
        /// computes the dot-product between 
        /// a 2x2 matrix and a 2D vector
        /// </summary>
        /// <param name="a"> input matrix a </param>
        /// <param name="v"> input vector v </param>
        /// <returns> output vector x = a*v </returns>
        public static VecD2 Dot(MatD2x2 a, VecD2 v)
            => new()
            {
                X = a.M11 * v.X + a.M12 * v.Y,
                Y = a.M21 * v.X + a.M22 * v.Y
            };

        /// <summary>
        /// computes the dot-product between
        /// two 2x2 matrices
        /// </summary>
        /// <param name="a"> input matrix a </param>
        /// <param name="b"> input matrix b </param>
        /// <returns> output 2x2 matrix c = a*b </returns>
        public static MatD2x2 Dot(MatD2x2 a, MatD2x2 b)
            => new()
            {
                M11 = a.M11 * b.M11 + a.M12 * b.M21,
                M12 = a.M11 * b.M12 + a.M12 * b.M22,
                M21 = a.M21 * b.M11 + a.M22 * b.M21,
                M22 = a.M21 * b.M12 + a.M22 * b.M22
            };

        #endregion
        #region operators

        #region ===== plus =====

        /// <summary>
        /// sum of two 2x2 matrix
        /// </summary>
        /// <param name="a"> input matrix a </param>
        /// <param name="b"> input matrix b </param>
        /// <returns> output matrix c = a + b </returns>
        public static MatD2x2 operator +(MatD2x2 a, MatD2x2 b)
            => new()
            {
                M11 = a.M11 + b.M11,
                M12 = a.M12 + b.M12,
                M21 = a.M21 + b.M21,
                M22 = a.M22 + b.M22
            };

        /// <summary>
        /// plus a scalar to each element of 
        /// a 2x2 matrix
        /// </summary>
        /// <param name="a"> input matrix a </param>
        /// <param name="s"> input scalar s </param>
        /// <returns> output matrix c_ij = a_ij + s </returns>
        public static MatD2x2 operator +(MatD2x2 a, double s)
            => new()
            {
                M11 = a.M11 + s,
                M12 = a.M12 + s,
                M21 = a.M21 + s,
                M22 = a.M22 + s
            };

        /// <summary>
        /// plus a scalar to each element of 
        /// a 2x2 matrix
        /// </summary>
        /// <param name="s"> input scalar s </param>
        /// <param name="a"> input matrix a </param>
        /// <returns> output matrix c_ij = s + a_ij </returns>
        public static MatD2x2 operator +(double s, MatD2x2 a)
            => a + s;

        #endregion
        #region ===== minus =====

        /// <summary>
        /// subtracts a 2x2 matrix from another
        /// </summary>
        /// <param name="a"> input matrix a </param>
        /// <param name="b"> input matrix b </param>
        /// <returns> result matrix c = a - b </returns>
        public static MatD2x2 operator -(MatD2x2 a, MatD2x2 b)
            => new()
            {
                M11 = a.M11 = b.M11,
                M12 = a.M12 - b.M12,
                M21 = a.M21 - b.M21,
                M22 = a.M22 - b.M22
            };

        /// <summary>
        /// subtracts a scalar from each element of 
        /// a 2x2 matrix
        /// </summary>
        /// <param name="a"> input matrix a </param>
        /// <param name="s"> input scalar s </param>
        /// <returns> result matrix c_ij = a_ij - s </returns>
        public static MatD2x2 operator -(MatD2x2 a, double s)
            => new()
            {
                M11 = a.M11 - s,
                M12 = a.M12 - s,
                M21 = a.M21 - s,
                M22 = a.M22 - s
            };

        /// <summary>
        /// subtracts a scalar by each element of 
        /// a 2x2 matrix
        /// </summary>
        /// <param name="s"> input scalar s </param>
        /// <param name="a"> inpupt matrix a </param>
        /// <returns> output matrix c_ij = s - a_ij </returns>
        public static MatD2x2 operator -(double s, MatD2x2 a)
            => new()
            {
                M11 = s - a.M11,
                M12 = s - a.M12,
                M21 = s - a.M21,
                M22 = s - a.M22
            };

        #endregion
        #region ===== multiply =====

        /// <summary>
        /// element-wise multiplication of 
        /// two 2x2 matrices
        /// </summary>
        /// <param name="a"> input matrix a </param>
        /// <param name="b"> input matrix b </param>
        /// <returns> output matrix c_ij = a_ij * b_ij </returns>
        public static MatD2x2 operator *(MatD2x2 a, MatD2x2 b)
            => new()
            {
                M11 = a.M11 * b.M11,
                M12 = a.M12 * b.M12,
                M21 = a.M21 * b.M21,
                M22 = a.M22 * b.M22
            };

        /// <summary>
        /// multiplies a scalar on each element 
        /// of a 2x2 matrix
        /// </summary>
        /// <param name="a"> input matrix a </param>
        /// <param name="s"> input scalar s </param>
        /// <returns> output matrix c_ij = a_ij * s </returns>
        public static MatD2x2 operator *(MatD2x2 a, double s)
            => new()
            {
                M11 = a.M11 * s,
                M12 = a.M12 * s,
                M21 = a.M21 * s,
                M22 = a.M22 * s
            };

        /// <summary>
        /// /// multiplies a scalar on each element 
        /// of a 2x2 matrix
        /// </summary>
        /// <param name="s"> input scalar s </param>
        /// <param name="a"> input matrix a </param>
        /// <returns> output matrix c_ij = s * a_ij </returns>
        public static MatD2x2 operator *(double s, MatD2x2 a)
            => a * s;

        #endregion
        #region ===== divide =====

        /// <summary>
        /// element-wise division of two 2x2 matrices
        /// </summary>
        /// <param name="a"> input matrix a </param>
        /// <param name="b"> input matrix b </param>
        /// <returns> output matrix c_ij = a_ij/b_ij </returns>
        public static MatD2x2 operator /(MatD2x2 a, MatD2x2 b)
            => new()
            {
                M11 = a.M11 / b.M11,
                M12 = a.M12 / b.M12,
                M21 = a.M21 / b.M21,
                M22 = a.M22 / b.M22
            };

        /// <summary>
        /// divides each element of a 2x2 matrix
        /// by a scalr
        /// </summary>
        /// <param name="a"> input matrix a </param>
        /// <param name="s"> input scalar s </param>
        /// <returns> output matrix c_ij = a_ij/s </returns>
        public static MatD2x2 operator /(MatD2x2 a, double s)
            => new()
            {
                M11 = a.M11 / s,
                M12 = a.M12 / s,
                M21 = a.M21 / s,
                M22 = a.M22 / s
            };

        /// <summary>
        /// divides a scalar by each element of
        /// a 2x2 matrix
        /// </summary>
        /// <param name="s"> input scalar s </param>
        /// <param name="a"> input matrix a </param>
        /// <returns> output matrix c_ij = s/a_ij </returns>
        public static MatD2x2 operator /(double s, MatD2x2 a)
            => new()
            {
                M11 = s / a.M11,
                M12 = s / a.M12,
                M21 = s / a.M21,
                M22 = s / a.M22
            };

        #endregion
        #region ===== negative =====

        /// <summary>
        /// takes the negative of a 2x2 matrix
        /// </summary>
        /// <param name="a"> input matrix a </param>
        /// <returns> output matrix c = -a </returns>
        public static MatD2x2 operator -(MatD2x2 a)
            => new()
            {
                M11 = -a.M11,
                M12 = -a.M12,
                M21 = -a.M21,
                M22 = -a.M22
            };

        #endregion

        #endregion
    }

    /// <summary>
    /// 3x3-dimension real-valued matrix class
    /// </summary>
    public class MatD3x3
    {
        #region fields

        #endregion
        #region properties

        /// <summary>
        /// matrix element (1,1)
        /// </summary>
        public double M11 { get; set; }

        /// <summary>
        /// matrix element (1,2)
        /// </summary>
        public double M12 { get; set; }

        /// <summary>
        /// matrix element (1,3)
        /// </summary>
        public double M13 { get; set; }

        /// <summary>
        /// matrix element (2,1)
        /// </summary>
        public double M21 { get; set; }

        /// <summary>
        /// matrix element (2,2)
        /// </summary>
        public double M22 { get; set; }

        /// <summary>
        /// matrix element (2,3)
        /// </summary>
        public double M23 { get; set; }

        /// <summary>
        /// matrix element (3,1)
        /// </summary>
        public double M31 { get; set; }

        /// <summary>
        /// matrix element (3,2)
        /// </summary>
        public double M32 { get; set; }

        /// <summary>
        /// matrix element (3,3)
        /// </summary>
        public double M33 { get; set; }

        /// <summary>
        /// first row of the 3x3 matrix
        /// </summary>
        public VecD3 Row1
        {
            get => new() { X = M11, Y = M12, Z = M13 };
            set { M11 = value.X; M12 = value.Y; M13 = value.Z; }
        }

        /// <summary>
        /// second row of the 3x3 matrix
        /// </summary>
        public VecD3 Row2
        {
            get => new() { X = M21, Y = M22, Z = M23 };
            set { M21 = value.X; M22 = value.Y; M23 = value.Z; }
        }

        /// <summary>
        /// third row od the 3x3 matrix
        /// </summary>
        public VecD3 Row3
        {
            get => new() { X = M31, Y = M32, Z = M33 };
            set { M31 = value.X; M32 = value.Y; M33 = value.Z; }
        }

        /// <summary>
        /// first column of the 3x3 matrix
        /// </summary>
        public VecD3 Col1
        {
            get => new() { X = M11, Y = M21, Z = M31 };
            set { M11 = value.X; M21 = value.Y; M31 = value.Z; }
        }

        /// <summary>
        /// second column of the 3x3 matrix
        /// </summary>
        public VecD3 Col2
        {
            get => new() { X = M12, Y = M22, Z = M32 };
            set { M12 = value.X; M22 = value.Y; M32 = value.Z; }
        }

        /// <summary>
        /// third column of the 3x3 matrix
        /// </summary>
        public VecD3 Col3
        {
            get => new() { X = M13, Y = M23, Z = M33 };
            set { M13 = value.X; M23 = value.Y; M33 = value.Z; }
        }

        #endregion
        #region constructors

        /// <summary>
        /// constructs a 3x3 matrix with its all elements
        /// </summary>
        /// <param name="m11"> matrix element (1,1) </param>
        /// <param name="m12"> matrix element (1,2) </param>
        /// <param name="m13"> matrix element (1,3) </param>
        /// <param name="m21"> matrix element (2,1) </param>
        /// <param name="m22"> matrix element (2,2) </param>
        /// <param name="m23"> matrix element (2,3) </param>
        /// <param name="m31"> matrix element (3,1) </param>
        /// <param name="m32"> matrix element (3,2) </param>
        /// <param name="m33"> matrix element (3,3) </param>
        public MatD3x3(double m11, double m12, double m13,
            double m21, double m22, double m23,
            double m31, double m32, double m33)
        {
            M11 = m11; M12 = m12; M13 = m13;
            M21 = m21; M22 = m22; M23 = m23;
            M31 = m31; M32 = m32; M33 = m33;
        }

        /// <summary>
        /// constructs a default 3x3 matrix with all
        /// element values set to zero
        /// </summary>
        public MatD3x3() : this(0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0) { }

        /// <summary>
        /// constructs a 3x3 matrix by 
        /// copying from another
        /// </summary>
        /// <param name="a"> another matrix </param>
        public MatD3x3(MatD3x3 a)
        {
            M11 = a.M11; M12 = a.M12; M13 = a.M13;
            M21 = a.M21; M22 = a.M22; M23 = a.M23;
            M31 = a.M31; M32 = a.M32; M33 = a.M33;
        }

        #endregion
        #region methods


        #endregion
        #region static methods

        /// <summary>
        /// computes the dot-product between 
        /// a 3x3 matrix and a 3D vector
        /// </summary>
        /// <param name="a"> input matrix a </param>
        /// <param name="v"> input vector v </param>
        /// <returns> output vector x = a*v </returns>
        public static VecD3 Dot(MatD3x3 a, VecD3 v)
            => new()
            {
                X = a.M11 * v.X + a.M12 * v.Y + a.M13 * v.Z,
                Y = a.M21 * v.X + a.M22 * v.Y + a.M23 * v.Z,
                Z = a.M31 * v.X + a.M32 * v.Y + a.M33 * v.Z
            };

        /// <summary>
        /// computes the dot-products between
        /// two 3x3 matrices 
        /// </summary>
        /// <param name="a"> input matrix a </param>
        /// <param name="b"> input matrix b </param>
        /// <returns> output matrix c = a*b </returns>
        public static MatD3x3 Dot(MatD3x3 a, MatD3x3 b)
            => new()
            {
                M11 = a.M11 * b.M11 + a.M12 * b.M21 + a.M13 * b.M31,
                M12 = a.M11 * b.M12 + a.M12 * b.M22 + a.M13 * b.M32,
                M13 = a.M11 * b.M13 + a.M12 * b.M23 + a.M13 * b.M33,
                M21 = a.M21 * b.M11 + a.M22 * b.M21 + a.M23 * b.M31,
                M22 = a.M21 * b.M12 + a.M22 * b.M22 + a.M23 * b.M32,
                M23 = a.M21 * b.M13 + a.M22 * b.M23 + a.M23 * b.M33,
                M31 = a.M31 * b.M11 + a.M32 * b.M21 + a.M33 * b.M31,
                M32 = a.M31 * b.M12 + a.M32 * b.M22 + a.M33 * b.M32,
                M33 = a.M31 * b.M13 + a.M32 * b.M23 + a.M33 * b.M33
            };

        #endregion
        #region operators 

        #region ===== plus =====

        /// <summary>
        /// sum of two 3x3 matrices
        /// </summary>
        /// <param name="a"> input matrix a </param>
        /// <param name="b"> input matrix b </param>
        /// <returns> output matrix c = a + b </returns>
        public static MatD3x3 operator +(MatD3x3 a, MatD3x3 b)
            => new()
            {
                M11 = a.M11 + b.M11,
                M12 = a.M12 + b.M12,
                M13 = a.M13 + b.M13,
                M21 = a.M21 + b.M21,
                M22 = a.M22 + b.M22,
                M23 = a.M23 + b.M23,
                M31 = a.M31 + b.M31,
                M32 = a.M32 + b.M32,
                M33 = a.M33 + b.M33
            };

        /// <summary>
        /// plus a scalar to each element of a 3x3 matrix 
        /// </summary>
        /// <param name="a"> input matrix a </param>
        /// <param name="s"> input scalar s </param>
        /// <returns> output matrix c_ij = a_ij + s </returns>
        public static MatD3x3 operator +(MatD3x3 a, double s)
            => new()
            {
                M11 = a.M11 + s,
                M12 = a.M12 + s,
                M13 = a.M13 + s,
                M21 = a.M21 + s,
                M22 = a.M22 + s,
                M23 = a.M23 + s,
                M31 = a.M31 + s,
                M32 = a.M32 + s,
                M33 = a.M33 + s
            };

        /// <summary>
        /// plus a scalar to each element of a 3x3 matrix 
        /// </summary>
        /// <param name="s"> input scalar s </param>
        /// <param name="a"> input matrix a </param>
        /// <returns> output matrix c_ij = s + a_ij </returns>
        public static MatD3x3 operator +(double s, MatD3x3 a)
            => a + s;

        #endregion
        #region ===== minus =====

        /// <summary>
        /// subtracts a 3x3 matrix from another
        /// </summary>
        /// <param name="a"> input matrix a </param>
        /// <param name="b"> input matrix b </param>
        /// <returns> output matrix c = a - b </returns>
        public static MatD3x3 operator -(MatD3x3 a, MatD3x3 b)
            => new()
            {
                M11 = a.M11 - b.M11,
                M12 = a.M12 - b.M12,
                M13 = a.M13 - b.M13,
                M21 = a.M21 - b.M21,
                M22 = a.M22 - b.M22,
                M23 = a.M23 - b.M23,
                M31 = a.M31 - b.M31,
                M32 = a.M32 - b.M32,
                M33 = a.M33 - b.M33
            };

        /// <summary>
        /// subtracts a scalar from each element 
        /// of a 3x3 matrix
        /// </summary>
        /// <param name="a"> input matrix a </param>
        /// <param name="s"> input scalar s </param>
        /// <returns> output matrix c_ij = a_ij - s </returns>
        public static MatD3x3 operator -(MatD3x3 a, double s)
            => new()
            {
                M11 = a.M11 - s,
                M12 = a.M12 - s,
                M13 = a.M13 - s,
                M21 = a.M21 - s,
                M22 = a.M22 - s,
                M23 = a.M23 - s,
                M31 = a.M31 - s,
                M32 = a.M32 - s,
                M33 = a.M33 - s
            };

        /// <summary>
        /// subtracts a scalar by each element
        /// of a 3x3 matrix
        /// </summary>
        /// <param name="s"> input scalar s </param>
        /// <param name="a"> input matrix a </param>
        /// <returns> output matrix c_ij = s - a_ij </returns>
        public static MatD3x3 operator -(double s, MatD3x3 a)
            => new()
            {
                M11 = s - a.M11,
                M12 = s - a.M12,
                M13 = s - a.M13,
                M21 = s - a.M21,
                M22 = s - a.M22,
                M23 = s - a.M23,
                M31 = s - a.M31,
                M32 = s - a.M32,
                M33 = s - a.M33
            };

        #endregion
        #region ===== multiply =====

        /// <summary>
        /// element-wise multiplication between two 3x3 matrices
        /// </summary>
        /// <param name="a"> input matrix a </param>
        /// <param name="b"> input matrix b </param>
        /// <returns> output matrix c_ij = a_ij * b_ij </returns>
        public static MatD3x3 operator *(MatD3x3 a, MatD3x3 b)
            => new()
            {
                M11 = a.M11 * b.M11,
                M12 = a.M12 * b.M12,
                M13 = a.M13 * b.M13,
                M21 = a.M21 * b.M21,
                M22 = a.M22 * b.M21,
                M23 = a.M23 * b.M23,
                M31 = a.M31 * b.M31,
                M32 = a.M32 * b.M32,
                M33 = a.M33 * b.M33
            };

        /// <summary>
        /// multiplies each element of a 3x3 matrix
        /// with a scalar
        /// </summary>
        /// <param name="a"> input matrix a </param>
        /// <param name="s"> input scalar s </param>
        /// <returns> output matrix c_ij = a_ij * s </returns>
        public static MatD3x3 operator *(MatD3x3 a, double s)
            => new()
            {
                M11 = a.M11 * s,
                M12 = a.M12 * s,
                M13 = a.M13 * s,
                M21 = a.M21 * s,
                M22 = a.M22 * s,
                M23 = a.M23 * s,
                M31 = a.M31 * s,
                M32 = a.M32 * s,
                M33 = a.M33 * s
            };

        /// <summary>
        /// multiplies a scalar on each element 
        /// of a 3x3 matrix 
        /// </summary>
        /// <param name="s"> input scalar s </param>
        /// <param name="a"> input matrix a </param>
        /// <returns> output matrix c_ij = s * a_ij </returns>
        public static MatD3x3 operator *(double s, MatD3x3 a)
            => a * s;

        #endregion
        #region ===== divide =====

        /// <summary>
        /// element-wise division between two 3x3 matrices
        /// </summary>
        /// <param name="a"> input matrix a </param>
        /// <param name="b"> input matrix b </param>
        /// <returns> output matrix c_ij = a_ij / b_ij </returns>
        public static MatD3x3 operator /(MatD3x3 a, MatD3x3 b)
            => new()
            {
                M11 = a.M11 / b.M11,
                M12 = a.M12 / b.M12,
                M13 = a.M13 / b.M13,
                M21 = a.M21 / b.M21,
                M22 = a.M22 / b.M22,
                M23 = a.M23 / b.M23,
                M31 = a.M31 / b.M31,
                M32 = a.M32 / b.M32,
                M33 = a.M33 / b.M33
            };

        /// <summary>
        /// divides each element of a 3x3 matrix
        /// by a scalar
        /// </summary>
        /// <param name="a"> input matrix a </param>
        /// <param name="s"> input scalar s </param>
        /// <returns> output matrix c_ij = a_ij / s </returns>
        public static MatD3x3 operator /(MatD3x3 a, double s)
            => new()
            {
                M11 = a.M11 / s,
                M12 = a.M12 / s,
                M13 = a.M13 / s,
                M21 = a.M21 / s,
                M22 = a.M22 / s,
                M23 = a.M23 / s,
                M31 = a.M31 / s,
                M32 = a.M32 / s,
                M33 = a.M33 / s
            };

        /// <summary>
        /// divides a scalar by each element of
        /// a 3x3 matrix
        /// </summary>
        /// <param name="s"> input scalar s </param>
        /// <param name="a"> input matrix a </param>
        /// <returns> output matrix c_ij = s / a_ij </returns>
        public static MatD3x3 operator /(double s, MatD3x3 a)
            => new()
            {
                M11 = s / a.M11,
                M12 = s / a.M12,
                M13 = s / a.M13,
                M21 = s / a.M21,
                M22 = s / a.M22,
                M23 = s / a.M23,
                M31 = s / a.M31,
                M32 = s / a.M32,
                M33 = s / a.M33
            };

        #endregion
        #region ===== negative =====

        /// <summary>
        /// takes the negative of the input 3x3 matrix
        /// </summary>
        /// <param name="a"> input matrix a </param>
        /// <returns> output matrix c = -a </returns>
        public static MatD3x3 operator -(MatD3x3 a)
            => new()
            {
                M11 = -a.M11,
                M12 = -a.M12,
                M13 = -a.M13,
                M21 = -a.M21,
                M22 = -a.M22,
                M23 = -a.M23,
                M31 = -a.M31,
                M32 = -a.M32,
                M33 = -a.M33
            };

        #endregion

        #endregion
    }

}
