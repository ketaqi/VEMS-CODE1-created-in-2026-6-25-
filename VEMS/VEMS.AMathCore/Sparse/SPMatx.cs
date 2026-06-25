using System.Numerics;
using System.Reflection.Metadata;

namespace VEMS.AMathCore
{

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SPMatx<T> : IMatx<T> 
        where T : INumber<T>
    {
        #region properties

        /// <summary>
        /// Gets or sets the number of rows in the matrix.
        /// </summary>
        public Int Rows { get; set; }

        /// <summary>
        /// Gets or sets the number of columns in the matrix.
        /// </summary>
        public Int Cols { get; set; }

        /// <summary>
        /// Gets the number of non-zero elements in the vector.
        /// </summary>
        public Int NzCount { get; internal set; }

        /// <summary>
        /// 
        /// </summary>
        public IntPtr Handle { get; internal set; }

        /// <summary>
        /// 
        /// </summary>
        public SPARSE_Status Status { get; private set; }


        public SPARSE_MatrixDescr MatrixDescr { get; set; }

        #endregion
        #region indexing

        public T this[long iRow, long iCol, bool checkBound = true] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public T this[int iRow, int iCol, bool checkBound = true] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public T this[Index iRow, Index iCol, bool checkBound = true] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        #endregion
        #region constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public SPMatx()
        {
            Rows = 0;
            Cols = 0;
            NzCount = 0;
            Handle = IntPtr.Zero;
            Status = SPARSE_Status.Success;
            MatrixDescr = new SPARSE_MatrixDescr()
            {
                Type = SPARSE_MatrixType.General,
                Mode = SPARSE_FillMode.Full,
                Diag = SPARSE_DiagType.NonUnit
            };
        }

        /// <summary>
        /// Constructs a sparse matrix with given size
        /// and non-zero element number.
        /// </summary>
        /// <param name="rows">Number of rows.</param>
        /// <param name="cols">Number of columns.</param>
        /// <param name="nnz">Number of non-zero elements in the matrix.</param>
        public SPMatx(Int rows, Int cols, Int nnz,
            SPARSE_MatrixDescr? matrixDescr = null)
        {
            //if (nnz > rows * cols) { throw new ArgumentException($"Too many non-zero elements"); }
            
            Rows = rows;
            Cols = cols;
            NzCount = nnz;
            Handle = IntPtr.Zero;
            Status = SPARSE_Status.Success;
            MatrixDescr = matrixDescr ?? new SPARSE_MatrixDescr()
            {
                Type = SPARSE_MatrixType.General,
                Mode = SPARSE_FillMode.Full,
                Diag = SPARSE_DiagType.NonUnit
            };
        }

        #endregion
        #region methods

        #region ---- creation ----

        /// <summary>
        /// Creates a sparse matrix in CSR format.
        /// </summary>
        /// <param name="rows">Number of rows.</param>
        /// <param name="cols">Number of columns.</param>
        /// <param name="nnz">Number of non-zero elements.</param>
        /// <param name="rowPtr">Row pointer array.</param>
        /// <param name="colIdx">Column index array.</param>
        /// <param name="nzVal">Non-zero values array.</param>
        /// <returns>Created sparse matrix in CSR format.</returns>
        public unsafe static SPMatx<T> CreateCSR(Int rows, 
            Int cols, Int nnz, DenseArray<Int> rowPtr, 
            DenseArray<Int> colIdx, DenseArray<T> nzVal,
            SPARSE_MatrixDescr? matrixDescr = null)
        {
            //if (colIdx.Count != nnz || nzVal.Count != nnz)
            //{ throw new ArgumentException($"Inconsistent number of non-zero elements"); }

            SPMatx<T> x = new (rows, cols, nnz, matrixDescr);

            IntPtr handle = x.Handle;
            Int* r = rowPtr.TPtr; 
            Int* c = colIdx.TPtr; 
            if (typeof(T) == typeof(double))
            {
                Real* v = nzVal.DPtr;
                x.Status = Defaults.ISPBLAS.CreateCSR(ref handle,
                    SPARSE_IndexBase.ZeroBase, rows, cols, r, r + 1, c, v);
                if (x.Status != SPARSE_Status.Success) throw new InvalidOperationException($"Error creating CSR matrix: {x.Status}");
            }
            else if (typeof(T) == typeof(Cplx))
            {
                void* v = nzVal.VPtr;
                x.Status = Defaults.ISPBLAS.CreateCSR(ref handle,
                    SPARSE_IndexBase.ZeroBase, rows, cols, r, r + 1, c, v);
                if (x.Status != SPARSE_Status.Success) throw new InvalidOperationException($"Error creating CSR matrix: {x.Status}");
            }
            else throw new NotSupportedException($"SPMatx<{typeof(T)}> is not supported.");
            x.Handle = handle;

            return x;
        }

        /// <summary>
        /// Creates a sparse matrix in CSC format.
        /// </summary>
        /// <param name="rows">Number of rows.</param>
        /// <param name="cols">Number of columns.</param>
        /// <param name="nnz">Number of non-zero elements.</param>
        /// <param name="colPtr">Column pointer array.</param>
        /// <param name="rowIdx">Row index array.</param>
        /// <param name="nzVal">Non-zero values array.</param>
        /// <returns>Created sparse matrix in CSC format.</returns>
        public unsafe static SPMatx<T> CreateCSC(Int rows,
            Int cols, Int nnz, DenseArray<Int> colPtr,
            DenseArray<Int> rowIdx, DenseArray<T> nzVal, 
            SPARSE_MatrixDescr? matrixDescr = null)
        {
            //if (rowIdx.Count != nnz || nzVal.Count != nnz)
            //{ throw new ArgumentException($"Inconsistent number of non-zero elements"); }
            
            SPMatx<T> x = new(rows, cols, nnz, matrixDescr);

            IntPtr handle = x.Handle;
            Int* c = colPtr.TPtr; 
            Int* r = rowIdx.TPtr; 
            if (typeof(T) == typeof(double))
            {
                Real* v = nzVal.DPtr;
                x.Status = Defaults.ISPBLAS.CreateCSC(ref handle,
                    SPARSE_IndexBase.ZeroBase, rows, cols, c, c + 1, r, v);
                if (x.Status != SPARSE_Status.Success) throw new InvalidOperationException($"Error creating CSC matrix: {x.Status}");
            }
            else if (typeof(T) == typeof(Cplx))
            {
                void* v = nzVal.VPtr;
                x.Status = Defaults.ISPBLAS.CreateCSC(ref handle,
                    SPARSE_IndexBase.ZeroBase, rows, cols, c, c + 1, r, v);
                if (x.Status != SPARSE_Status.Success) throw new InvalidOperationException($"Error creating CSC matrix: {x.Status}");
            }
            else throw new NotSupportedException($"SPMatx<{typeof(T)}> is not supported.");
            x.Handle = handle;

            return x;
        }

        /// <summary>
        /// Creates a sparse matrix in COO format.
        /// </summary>
        /// <param name="rows">Number of rows.</param>
        /// <param name="cols">Number of columns.</param>
        /// <param name="nnz">Number of non-zero elements.</param>
        /// <param name="rowIdx">Row index array.</param>
        /// <param name="colIdx">Column index array.</param>
        /// <param name="nzVal">Non-zero values array.</param>
        /// <returns>Created sparse matrix in COO format.</returns>
        public unsafe static SPMatx<T> CreateCOO(Int rows,
            Int cols, Int nnz, DenseArray<Int> rowIdx,
            DenseArray<Int> colIdx, DenseArray<T> nzVal,
            SPARSE_MatrixDescr? matrixDescr = null)
        {
            //if (rowIdx.Count != nnz || colIdx.Count != nnz || nzVal.Count != nnz)
            //{ throw new ArgumentException($"Inconsistent number of non-zero elements"); }
            
            SPMatx<T> x = new(rows, cols, nnz, matrixDescr);
            IntPtr handle = x.Handle;
            Int* r = rowIdx.TPtr; 
            Int* c = colIdx.TPtr; 
            if (typeof(T) == typeof(double))
            {
                Real* v = nzVal.DPtr;
                x.Status = Defaults.ISPBLAS.CreateCOO(ref handle,
                    SPARSE_IndexBase.ZeroBase, rows, cols, nnz, r, c, v);
                if (x.Status != SPARSE_Status.Success) throw new InvalidOperationException($"Error creating COO matrix: {x.Status}");
            }
            else if (typeof(T) == typeof(Cplx))
            {
                void* v = nzVal.VPtr;
                x.Status = Defaults.ISPBLAS.CreateCOO(ref handle,
                    SPARSE_IndexBase.ZeroBase, rows, cols, nnz, r, c, v);
                if (x.Status != SPARSE_Status.Success) throw new InvalidOperationException($"Error creating COO matrix: {x.Status}");
            }
            else throw new NotSupportedException($"SPMatx<{typeof(T)}> is not supported.");
            x.Handle = handle;
            
            return x;
        }

        #endregion
        #region ---- conversion ----

        /// <summary>
        /// Converts the internal matrix representation to CSR format.
        /// </summary>
        /// <param name="operation">Specifies operation op() on the matrix.</param>
        /// <exception cref="ArgumentException"></exception>
        public void Convert2CSR(SPARSE_Operation operation = SPARSE_Operation.NonTranspose)
        {
            //if (Handle == IntPtr.Zero) { throw new ArgumentException($"Invalid handle"); }
            IntPtr handle = Handle;
            Status = Defaults.ISPBLAS.ConvertCSR(handle, operation, ref handle);
            if (Status != SPARSE_Status.Success) throw new InvalidOperationException($"Error converting to CSR format: {Status}");
            Handle = handle;
        }


        public void Convert2CSR(ref SPMatx<T> target, 
            SPARSE_Operation operation = SPARSE_Operation.NonTranspose)
        {
            IntPtr src = Handle;
            IntPtr dst = target.Handle;
            Status = Defaults.ISPBLAS.ConvertCSR(src, operation, ref dst);
            if (Status != SPARSE_Status.Success) throw new InvalidOperationException($"Error converting to CSR format: {Status}");
            target.Rows = Rows;
            target.Cols = Cols;
            target.NzCount = NzCount;
            target.Handle = dst;
        }

        #endregion
        #region ---- special ----

        /// <summary>
        /// Creates a square sparse matrix of size <paramref name="nnz"/> that contains
        /// only diagonal elements supplied by <paramref name="diagVal"/>.
        /// The returned matrix is in CSR format and its <see cref="MatrixDescr"/>
        /// is set to indicate a diagonal matrix.
        /// </summary>
        /// <param name="nnz">The dimension of the square matrix (number of rows and columns). Must be non-negative.</param>
        /// <param name="diagVal">A dense array containing the diagonal.</param>
        /// <returns>
        /// A new <see cref="SPMatx{T}"/> representing the diagonal matrix. The matrix descriptor
        /// (<see cref="MatrixDescr"/>) will be set to <see cref="SPARSE_MatrixType.Diagonal"/>
        /// with <see cref="SPARSE_DiagType.NonUnit"/>.
        /// </returns>
        public static unsafe SPMatx<T> Diagonal(DenseArray<T> diag)
        {
            Int nnz = diag.Count;
            Vect<Int> rowPtr = Vect<Int>.Create(n: nnz + 1, x0: 0, dx: 1);
            Vect<Int> colIdx = Vect<Int>.Create(n: nnz, x0: 0, dx: 1);

            SPMatx<T> x = CreateCSR(rows: nnz, cols: nnz, nnz: nnz,
                rowPtr: rowPtr, colIdx: colIdx, nzVal: diag,
                matrixDescr: new SPARSE_MatrixDescr()
                {
                    Type = SPARSE_MatrixType.Diagonal,
                    Mode = SPARSE_FillMode.Full,
                    Diag = SPARSE_DiagType.NonUnit
                });

            return x;
        }

        /// <summary>
        /// Creates an identity sparse matrix of dimension <paramref name="nnz"/> in CSR format.
        /// </summary>
        /// <param name="nnz">The number of rows and columns of the identity matrix. Must be non-negative.</param>
        /// <returns>
        /// A new <see cref="SPMatx{T}"/> representing the identity matrix with size <paramref name="nnz"/>.
        /// The matrix descriptor (<see cref="MatrixDescr"/>) is set to indicate a diagonal matrix with unit diagonal.
        /// </returns>
        public static unsafe SPMatx<T> Identity(Int nnz)
        {
            Vect<T> diag = Vect<T>.Create(n: nnz, x0: T.CreateChecked(1));
            
            SPMatx<T> x = Diagonal(diag);
            x.MatrixDescr = new SPARSE_MatrixDescr()
            {
                Type = SPARSE_MatrixType.Diagonal,
                Mode = SPARSE_FillMode.Full,
                Diag = SPARSE_DiagType.Unit
            };
            
            return x;
        }

        #endregion
        #region ---- export ----

        /// <summary>
        /// Exports the internal sparse matrix representation in CSR (Compressed Sparse Row) format.
        /// </summary>
        /// <param name="indexing">
        /// When the method returns, contains the index base used by the exported arrays
        /// (<see cref="SPARSE_IndexBase.ZeroBase"/> or <see cref="SPARSE_IndexBase.OneBase"/>).
        /// </param>
        /// <param name="rows">When the method returns, contains the number of rows of the matrix.</param>
        /// <param name="cols">When the method returns, contains the number of columns of the matrix.</param>
        /// <param name="nnz">
        /// When the method returns, contains the number of non-zero entries in the matrix.
        /// The value is derived from the row pointer array and the reported <paramref name="indexing"/>.
        /// </param>
        /// <param name="rowPtr">
        /// When the method returns, contains a pointer to the row pointer array. The array length is <c>rows + 1</c>.
        /// The returned pointer references internal memory owned by the sparse handle; the caller must not attempt to free it.
        /// </param>
        /// <param name="colIdx">
        /// When the method returns, contains a pointer to the column indices array. The array length is <paramref name="nnz"/>.
        /// Indices are expressed according to <paramref name="indexing"/>.
        /// </param>
        /// <param name="nzVal">
        /// When the method returns, contains a pointer to the non-zero values array. The element type depends on the generic type parameter <c>T</c>:
        /// - For <c>T</c> equal to <see cref="Real"/> (typically <c>double</c>), the pointer is to <see cref="Real"/>.
        /// - For <c>T</c> equal to <see cref="Cplx"/>, the pointer is to complex values.
        /// The returned pointer references internal memory owned by the sparse handle; the caller must not attempt to free it.
        /// </param>
        public unsafe void ExportCSR(out SPARSE_IndexBase indexing,
            out Int rows, out Int cols, out Int nnz,
            out Int* rowPtr, out Int* colIdx, out T* nzVal)
        {
            // Initialize inputs and pointers
            indexing = SPARSE_IndexBase.ZeroBase;

            Int localRows = 0;
            Int localCols = 0;
            // Local unmanaged pointers that we can take addresses of
            Int* pr = &localRows;
            Int* pc = &localCols;
            Int* localRowPtr = null;
            Int* localRowsEnd = null;
            Int* localColIdx = null;

            if (typeof(T) == typeof(Real))
            {
                Real* localValues = null;
                SPARSE_Status status = Defaults.ISPBLAS.ExportCSR(
                    Handle, ref indexing, pr, pc,
                    &localRowPtr, &localRowsEnd, &localColIdx, &localValues);
                nzVal = (T*)localValues;
            }
            else if (typeof(T) == typeof(Cplx))
            {
                void* localValues = null;
                SPARSE_Status status = Defaults.ISPBLAS.ExportCSR(
                    Handle, ref indexing, pr, pc,
                    &localRowPtr, &localRowsEnd, &localColIdx, &localValues);
                nzVal = (T*)localValues;
            }
            else throw new NotSupportedException($"SPMatx<{typeof(T)}> is not supported.");

            // propagate pointers back to caller (out param)
            rows = localRows;
            cols = localCols;
            rowPtr = localRowPtr;
            colIdx = localColIdx;
            // calculate nnz
            nnz = rowPtr[rows] - (indexing == SPARSE_IndexBase.ZeroBase ? 0 : 1);
        }

        #endregion
        #region ---- scatter ----

        /// <summary>
        /// Converts the sparse matrix to a dense <see cref="Matx{T}"/> by exporting to CSR and
        /// scattering non-zero values into a newly allocated dense matrix.
        /// </summary>
        /// <returns>
        /// A dense matrix with dimensions equal to this sparse matrix's <see cref="Rows"/> and <see cref="Cols"/>.
        /// Entries not present in the sparse representation are set to the default (zero) value.
        /// </returns>
        /// <remarks>
        /// The method first calls <see cref="ExportCSR"/> to obtain CSR pointers. The returned
        /// pointer arrays reference internal memory; the implementation only reads from them.
        /// Index base adjustments are applied depending on <see cref="SPARSE_IndexBase"/>.
        /// </remarks>
        public unsafe Matx<T> Scatter()
        {
            // exports to CSR first
            ExportCSR(out SPARSE_IndexBase indexing,
                out Int rows, out Int cols, out Int nnz,
                out Int* rowPtr, out Int* colIdx, out T* nzVal);

            // initializes the output dense matrix
            Matx<T> a = new (rows: Rows, cols: Cols, 
                initMode: ArrayInitMode.Calloc);

            // fills the dense matrix
            for (Int iRow = 0; iRow < rows; iRow ++)
            {
                Int start = rowPtr[iRow] - (indexing == SPARSE_IndexBase.ZeroBase ? 0 : 1);
                Int end = rowPtr[iRow + 1] - (indexing == SPARSE_IndexBase.ZeroBase ? 0 : 1);
                for (Int idx = start; idx < end; idx ++)
                {
                    Int jCol = colIdx[idx] - (indexing == SPARSE_IndexBase.ZeroBase ? 0 : 1);
                    a[iRow, jCol] = nzVal[idx];
                }
            }

            return a;
        }

        #endregion

        #endregion
        #region dispose

        /// <summary>
        /// internal flag whether disposed or not
        /// </summary>
        private bool _disposed;

        /// <summary>
        /// implements IDisposable
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            Status = Defaults.ISPBLAS.Destroy(Handle);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// disposes
        /// if disposing equals true, the method has been called directly
        /// if disposing equals false, the method has been called by the 
        /// runtime from inside the finalizer
        /// </summary>
        /// <param name="disposing"> flag </param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed == false)
            {
                if (disposing) { }
                Status = Defaults.ISPBLAS.Destroy(Handle);
                _disposed = true;
            }
        }

        /// <summary>
        /// uses C# destructor syntax for finalization code.
        /// This destructor will run only if the Dispose method
        /// does not get called.
        /// it gives your base class the opportunity to finalize.
        /// </summary>
        ~SPMatx()
        {
            Dispose(false);
        }

        #endregion
    }

}
