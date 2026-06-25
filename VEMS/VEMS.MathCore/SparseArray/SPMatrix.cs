using Complex = System.Numerics.Complex;

namespace VEMS.MathCore
{

    /// <summary>
    /// Sparse Matrix[T] class
    /// </summary>
    /// <typeparam name="T"> double or complex </typeparam>
    public class SPMatrix<T> : IDisposable, IMatrix<T> where T : struct
    {
        #region fields

        /// <summary>
        /// empty matrix with ZERO count
        /// </summary>
        public static SPVector<T> Empty = new();

        #endregion
        #region properties

        /// <summary>
        /// number of rows in the matrix
        /// including zeros
        /// </summary>
        public long Rows { get; set; }

        /// <summary>
        /// number of columns in the matrix
        /// including zeros
        /// </summary>
        public long Cols { get; set; }

        /// <summary>
        /// number of non-zero elements in the matrix
        /// </summary>
        public long NzCount { get; set; }

        /// <summary>
        /// handle to the sparse matrix
        /// </summary>
        public IntPtr Handle { get; set; }

        /// <summary>
        /// status of the sparse matrix
        /// </summary>
        public SPARSE_Status Status { get; set; }

        #endregion
        #region indexing

        public T this[long iRow, long iCol, bool checkBound = true] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public T this[int iRow, int iCol, bool checkBound = true] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public T this[Index iRow, Index iCol, bool checkBound = true] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        #endregion
        #region constructors

        /// <summary>
        /// default constructor
        /// </summary>
        internal SPMatrix()
        {
            Rows = 0;
            Cols = 0;
            NzCount = 0;
            Handle = IntPtr.Zero;
            Status = SPARSE_Status.Success;
        }

        /// <summary>
        /// constructs a sparse matrix with given size
        /// and non-zero element number
        /// </summary>
        /// <param name="rows"> number of rows </param>
        /// <param name="cols"> number of columns </param>
        /// <param name="nnz"> number of non-zero elements in the matrix </param>
        public SPMatrix(long rows, long cols, long nnz)
        {
            if (nnz > rows * cols) { throw new ArgumentException($"Too many non-zero elements"); }
            Rows = rows;
            Cols = cols;
            NzCount = nnz;
            Handle = IntPtr.Zero;
            Status = SPARSE_Status.Success;
        }

        /// <summary>
        /// constructs a sparse matrix in the CSR format
        /// with given non-zero information
        /// </summary>
        /// <param name="rows"> number of rows </param>
        /// <param name="cols"> number of columns </param>
        /// <param name="nnz"> number of non-zero elements in the matrix </param>
        /// <param name="rowPtr"> row start/end indices of non-zero elements </param>
        /// <param name="colIdx"> column indices of non-zero elements </param>
        /// <param name="nzVal"> values of non-zero elements </param>
        internal SPMatrix(long rows, long cols, long nnz, 
            DenseArrayBase<long> rowPtr, DenseArrayBase<long> colIdx, 
            DenseArrayBase<T> nzVal)
            : this(rows, cols, nnz)
        {
            if (colIdx.Count != nnz || nzVal.Count != nnz)
            { throw new ArgumentException($"Inconsistent number of non-zero elements"); }
            IntPtr handle = Handle;
            //long* r = (long*)rowPtr.VPtr;
            //long* c = (long*)colIdx.VPtr;
            //double* v = (double*)nzVal.VPtr;
            Status = IntelMKLNative.mkl_sparse_d_create_csr_64(
                ref handle, SPARSE_IndexBase.ZeroBase, rows, cols,
                rowPtr.DataPtr, colIdx.DataPtr, nzVal.DataPtr);
            Handle = handle;
        }

        /// <summary>
        /// constructs a sparse matrix in the CSR format
        /// with given non-zero information
        /// </summary>
        /// <param name="rows"> number of rows </param>
        /// <param name="cols"> number of columns </param>
        /// <param name="nnz"> number of non-zero elements in the matrix </param>
        /// <param name="nzInfo"> information of non-zero elements in CSR format </param>
        internal SPMatrix(long rows, long cols, long nnz,
            SPMInfoCSR<T> nzInfo) : this(rows, cols, nnz)
        {
            IntPtr handle = Handle;
            //long* r = (long*)nzInfo.RowPtr.VPtr;
            //long* c = (long*)nzInfo.ColIdx.VPtr;
            //double* v = (double*)nzInfo.NzValues.VPtr;
            Status = IntelMKLNative.mkl_sparse_d_create_csr_64(
                ref handle, SPARSE_IndexBase.ZeroBase, rows, cols,
                nzInfo.RowPtr.DataPtr, nzInfo.ColIdx.DataPtr, nzInfo.NzValues.DataPtr);
            Handle = handle;
        }

        /// <summary>
        /// constructs a sparse matrix in the CSC format
        /// with given non-zero information
        /// </summary>
        /// <param name="rows"> number of rows </param>
        /// <param name="cols"> number of columns </param>
        /// <param name="nnz"> number of non-zero elements in the matrix </param>
        /// <param name="nzInfo"> information of non-zero elements in CSR format </param>
        internal SPMatrix(long rows, long cols, long nnz,
            SPMInfoCSC<T> nzInfo) : this(rows, cols, nnz)
        {
            IntPtr handle = Handle;
            //long* c = (long*)nzInfo.ColPtr.VPtr;
            //long* r = (long*)nzInfo.RowIdx.VPtr;
            //double* v = (double*)nzInfo.NzValues.VPtr;
            Status = IntelMKLNative.mkl_sparse_d_create_csc_64(
                ref handle, SPARSE_IndexBase.ZeroBase, rows, cols,
                nzInfo.ColPtr.DataPtr, nzInfo.RowIdx.DataPtr, nzInfo.NzValues.DataPtr);
            Handle = handle;
        }

        /// <summary>
        /// constructs a sparse matrix in the COO format
        /// with given non-zero information
        /// </summary>
        /// <param name="rows"> number of rows </param>
        /// <param name="cols"> number of columns </param>
        /// <param name="nnz"> number of non-zero elements in the matrix </param>
        /// <param name="nzInfo"> information of non-zero elements in COO format </param>
        internal SPMatrix(long rows, long cols, long nnz,
            SPMInfoCOO<T> nzInfo) : this(rows, cols, nnz)
        {
            IntPtr handle = Handle;
            //long* r = (long*)nzInfo.RowIdx.VPtr;
            //long* c = (long*)nzInfo.ColIdx.VPtr;
            //double* v = (double*)nzInfo.NzValues.VPtr;
            Status = IntelMKLNative.mkl_sparse_d_create_coo_64(
                ref handle, SPARSE_IndexBase.ZeroBase, rows, cols, nnz,
                nzInfo.RowIdx.DataPtr, nzInfo.ColIdx.DataPtr, nzInfo.NzValues.DataPtr);
            Handle = handle;
        }

        #endregion
        #region methods

        public bool IsColIndexValid(long iCol)
        {
            throw new NotImplementedException();
        }

        public bool IsRowIndexValid(long iRow)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// converts internal matrix representation to CSR format
        /// </summary>
        /// <param name="operation"> specifies operation op() on the matrix </param>
        /// <exception cref="ArgumentException"></exception>
        public void Convert2CSR(SPARSE_Operation operation = SPARSE_Operation.NonTranspose)
        {
            if (Handle == IntPtr.Zero) { throw new ArgumentException($"Invalid handle"); }
            IntPtr handle = Handle;
            Status = Defaults.ISPBLAS.ConvertCSR(handle, operation, ref handle);
            Handle = handle;
        }


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
            //IntelMKLNative.mkl_sparse_destroy_64(Handle);
            Sparse.Destroy(Handle);
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
                //IntelMKLNative.mkl_sparse_destroy_64(Handle);
                Sparse.Destroy(Handle);
                _disposed = true;
            }
        }

        /// <summary>
        /// uses C# destructor syntax for finalization code.
        /// This destructor will run only if the Dispose method
        /// does not get called.
        /// it gives your base class the opportunity to finalize.
        /// </summary>
        ~SPMatrix()
        {
            Dispose(false);
        }

        #endregion
    }


    /// <summary>
    /// non-zero information of a sparse matrix
    /// in the CSR format
    /// </summary>
    /// <typeparam name="T"> double or complex </typeparam>
    public class SPMInfoCSR<T> where T : struct
    {
        /// <summary>
        /// number of non-zero elements in the matrix
        /// </summary>
        public long NzCount { get; set; }

        /// <summary>
        /// row start/end indices of non-zero elements
        /// </summary>
        public DenseArrayBase<long> RowPtr { get; set; }

        /// <summary>
        /// column indices of non-zero elements
        /// </summary>
        public DenseArrayBase<long> ColIdx { get; set; }

        /// <summary>
        /// values of non-zero elements
        /// </summary>
        public DenseArrayBase<T> NzValues { get; set; }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="rowPtr"> row start/end indices of non-zero elements </param>
        /// <param name="colIdx"> column indices of non-zero elements </param>
        /// <param name="nzVal"> values of non-zero elements </param>
        public SPMInfoCSR(DenseArrayBase<long> rowPtr, DenseArrayBase<long> colIdx, 
            DenseArrayBase<T> nzVal)
        {
            if (colIdx.Count != nzVal.Count)
            { throw new ArgumentException($"Inconsistent number of non-zero elements"); }
            NzCount = nzVal.Count;
            RowPtr = rowPtr;
            ColIdx = colIdx;
            NzValues = nzVal;
        }

    }

    /// <summary>
    /// non-zero information of a sparse matrix
    /// in the CSC format
    /// </summary>
    /// <typeparam name="T"> double or complex</typeparam>
    public class SPMInfoCSC<T> where T : struct
    {
        /// <summary>
        /// number of non-zero elements in the matrix
        /// </summary>
        public long NzCount { get; set; }

        /// <summary>
        /// column start/end indices of non-zero elements
        /// </summary>
        public DenseArrayBase<long> ColPtr { get; set; }

        /// <summary>
        /// row indices of non-zero elements
        /// </summary>
        public DenseArrayBase<long> RowIdx { get; set; }

        /// <summary>
        /// values of non-zero elements
        /// </summary>
        public DenseArrayBase<T> NzValues { get; set; }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="colPtr"> column start/end indices of non-zero elements </param>
        /// <param name="rowIdx"> row indices of non-zero elements </param>
        /// <param name="nzVal"> values of non-zero elements </param>
        public SPMInfoCSC(DenseArrayBase<long> colPtr, DenseArrayBase<long> rowIdx, 
            DenseArrayBase<T> nzVal)
        {
            if (rowIdx.Count != nzVal.Count)
            { throw new ArgumentException($"Inconsistent number of non-zero elements"); }
            NzCount = nzVal.Count;
            ColPtr = colPtr;
            RowIdx = rowIdx;
            NzValues = nzVal;
        }

    }

    /// <summary>
    /// non-zero information of a sparse matrix
    /// in the COO format
    /// </summary>
    /// <typeparam name="T"> double or complex </typeparam>
    public class SPMInfoCOO<T> where T : struct
    {
        /// <summary>
        /// number of non-zero elements in the matrix
        /// </summary>
        public long NzCount { get; set; }

        /// <summary>
        /// row indices of non-zero elements
        /// </summary>
        public DenseArrayBase<long> RowIdx { get; set; }

        /// <summary>
        /// column indices of non-zero elements
        /// </summary>
        public DenseArrayBase<long> ColIdx { get; set; }

        /// <summary>
        /// values of non-zero elements
        /// </summary>
        public DenseArrayBase<T> NzValues { get; set; }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="rowIdx"> row indices of non-zero elements </param>
        /// <param name="colIdx"> column indices of non-zero elements </param>
        /// <param name="nzVal"> values of non-zero elements </param>
        public SPMInfoCOO(DenseArrayBase<long> rowIdx, DenseArrayBase<long> colIdx, 
            DenseArrayBase<T> nzVal)
        {
            if (rowIdx.Count != nzVal.Count || colIdx.Count != nzVal.Count)
            { throw new ArgumentException($"Inconsistent number of non-zero elements"); }
            NzCount = nzVal.Count;
            RowIdx = rowIdx;
            ColIdx = colIdx;
            NzValues = nzVal;
        }

    }


    /// <summary>
    /// real-valued sparse matrix
    /// </summary>
    public class MatDi : SPMatrix<double>
    {
        #region constructors

        /// <summary>
        /// constructs a sparse matrix with given size
        /// and non-zero element number
        /// </summary>
        /// <param name="rows"> number of rows </param>
        /// <param name="cols"> number of columns </param>
        /// <param name="nnz"> number of non-zero elements in the matrix </param>
        public MatDi(long rows, long cols, long nnz)
            : base(rows, cols, nnz) { }

        /// <summary>
        /// constructs a sparse matrix in the CSR format
        /// with given non-zero information
        /// </summary>
        /// <param name="rows"> number of rows </param>
        /// <param name="cols"> number of columns </param>
        /// <param name="nnz"> number of non-zero elements in the matrix </param>
        /// <param name="rowPtr"> row start/end indices of non-zero elements </param>
        /// <param name="colIdx"> column indices of non-zero elements </param>
        /// <param name="nzVal"> values of non-zero elements </param>
        public MatDi(long rows, long cols, long nnz,
            DenseArrayBase<long> rowPtr, DenseArrayBase<long> colIdx,
            DenseArrayBase<double> nzVal)
            : this(rows, cols, nnz)
        {
            if (colIdx.Count != nnz || nzVal.Count != nnz)
            { throw new ArgumentException($"Inconsistent number of non-zero elements"); }

            MatDi a = this;
            Sparse.CreateCSRD(ref a, rowPtr, colIdx, nzVal,
                indexing: SPARSE_IndexBase.ZeroBase);
        }

        /// <summary>
        /// constructs a sparse matrix in the CSR format
        /// with given non-zero information
        /// </summary>
        /// <param name="rows"> number of rows </param>
        /// <param name="cols"> number of columns </param>
        /// <param name="nnz"> number of non-zero elements in the matrix </param>
        /// <param name="nzInfo"> information of non-zero elements in CSR format </param>
        public MatDi(long rows, long cols, long nnz,
            SPMInfoCSR<double> nzInfo) 
            : this(rows, cols, nnz)
        {
            MatDi a = this;
            Sparse.CreateCSRD(ref a, nzInfo.RowPtr, nzInfo.ColIdx, nzInfo.NzValues,
                indexing: SPARSE_IndexBase.ZeroBase);
        }

        /// <summary>
        /// constructs a sparse matrix in the CSC format
        /// with given non-zero information
        /// </summary>
        /// <param name="rows"> number of rows </param>
        /// <param name="cols"> number of columns </param>
        /// <param name="nnz"> number of non-zero elements in the matrix </param>
        /// <param name="nzInfo"> information of non-zero elements in CSR format </param>
        public MatDi(long rows, long cols, long nnz,
            SPMInfoCSC<double> nzInfo) 
            : this(rows, cols, nnz)
        {
            MatDi a = this;
            Sparse.CreateCSCD(ref a, nzInfo.ColPtr, nzInfo.RowIdx, nzInfo.NzValues,
                indexing: SPARSE_IndexBase.ZeroBase);
        }

        /// <summary>
        /// constructs a sparse matrix in the COO format
        /// with given non-zero information
        /// </summary>
        /// <param name="rows"> number of rows </param>
        /// <param name="cols"> number of columns </param>
        /// <param name="nnz"> number of non-zero elements in the matrix </param>
        /// <param name="nzInfo"> information of non-zero elements in COO format </param>
        public MatDi(long rows, long cols, long nnz,
            SPMInfoCOO<double> nzInfo) : this(rows, cols, nnz)
        {
            MatDi a = this;
            Sparse.CreateCOOD(ref a, nzInfo.RowIdx, nzInfo.ColIdx, nzInfo.NzValues,
                indexing: SPARSE_IndexBase.ZeroBase);
        }

        #endregion
        #region methods

        // ...

        #endregion
    }

    /// <summary>
    /// complex-valued sparse matrix
    /// </summary>
    public class MatZi : SPMatrix<Complex>
    {
        #region constructors

        /// <summary>
        /// constructs a sparse matrix with given size
        /// and non-zero element number
        /// </summary>
        /// <param name="rows"> number of rows </param>
        /// <param name="cols"> number of columns </param>
        /// <param name="nnz"> number of non-zero elements in the matrix </param>
        public MatZi(long rows, long cols, long nnz)
            : base(rows, cols, nnz) { }

        /// <summary>
        /// constructs a sparse matrix in the CSR format
        /// with given non-zero information
        /// </summary>
        /// <param name="rows"> number of rows </param>
        /// <param name="cols"> number of columns </param>
        /// <param name="nnz"> number of non-zero elements in the matrix </param>
        /// <param name="rowPtr"> row start/end indices of non-zero elements </param>
        /// <param name="colIdx"> column indices of non-zero elements </param>
        /// <param name="nzVal"> values of non-zero elements </param>
        public MatZi(long rows, long cols, long nnz,
            DenseArrayBase<long> rowPtr, DenseArrayBase<long> colIdx,
            DenseArrayBase<Complex> nzVal)
            : this(rows, cols, nnz)
        {
            if (colIdx.Count != nnz || nzVal.Count != nnz)
            { throw new ArgumentException($"Inconsistent number of non-zero elements"); }

            MatZi a = this;
            Sparse.CreateCSRZ(ref a, rowPtr, colIdx, nzVal,
                indexing: SPARSE_IndexBase.ZeroBase);
        }

        /// <summary>
        /// constructs a sparse matrix in the CSR format
        /// with given non-zero information
        /// </summary>
        /// <param name="rows"> number of rows </param>
        /// <param name="cols"> number of columns </param>
        /// <param name="nnz"> number of non-zero elements in the matrix </param>
        /// <param name="nzInfo"> information of non-zero elements in CSR format </param>
        public MatZi(long rows, long cols, long nnz,
            SPMInfoCSR<Complex> nzInfo)
            : this(rows, cols, nnz)
        {
            MatZi a = this;
            Sparse.CreateCSRZ(ref a, nzInfo.RowPtr, nzInfo.ColIdx, nzInfo.NzValues,
                indexing: SPARSE_IndexBase.ZeroBase);
        }

        /// <summary>
        /// constructs a sparse matrix in the CSC format
        /// with given non-zero information
        /// </summary>
        /// <param name="rows"> number of rows </param>
        /// <param name="cols"> number of columns </param>
        /// <param name="nnz"> number of non-zero elements in the matrix </param>
        /// <param name="nzInfo"> information of non-zero elements in CSR format </param>
        public MatZi(long rows, long cols, long nnz,
            SPMInfoCSC<Complex> nzInfo)
            : this(rows, cols, nnz)
        {
            MatZi a = this;
            Sparse.CreateCSCZ(ref a, nzInfo.ColPtr, nzInfo.RowIdx, nzInfo.NzValues,
                indexing: SPARSE_IndexBase.ZeroBase);
        }

        /// <summary>
        /// constructs a sparse matrix in the COO format
        /// with given non-zero information
        /// </summary>
        /// <param name="rows"> number of rows </param>
        /// <param name="cols"> number of columns </param>
        /// <param name="nnz"> number of non-zero elements in the matrix </param>
        /// <param name="nzInfo"> information of non-zero elements in COO format </param>
        public MatZi(long rows, long cols, long nnz,
            SPMInfoCOO<Complex> nzInfo) : this(rows, cols, nnz)
        {
            MatZi a = this;
            Sparse.CreateCOOZ(ref a, nzInfo.RowIdx, nzInfo.ColIdx, nzInfo.NzValues,
                indexing: SPARSE_IndexBase.ZeroBase);
        }

        #endregion
        #region methods

        // ...

        #endregion
    }

}
