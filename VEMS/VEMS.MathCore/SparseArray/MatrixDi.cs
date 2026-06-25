using WMathCore;

namespace VEMS.MathCore
{

    ///// <summary>
    ///// 
    ///// </summary>
    //public unsafe class MatDi
    //{
    //    #region properties

    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public long Rows { get; set; }
        
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public long Cols { get; set; }

    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public IntPtr Handle { get; set; }

    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public SPARSE_Status Status { get; set; }

    //    #endregion
    //    #region constructors

    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public MatDi()
    //    {
    //        Handle = new IntPtr();
    //    }

    //    /// <summary>
    //    /// CSR constructor
    //    /// </summary>
    //    /// <param name="rows"></param>
    //    /// <param name="cols"></param>
    //    /// <param name="rowPtr"></param>
    //    /// <param name="colIndx"></param>
    //    /// <param name="values"></param>
    //    public MatDi(long rows, long cols,
    //        VectorI rowPtr, VectorI colIndx, VectorD values)
    //    {
    //        Rows = rows;
    //        Cols = cols;
    //        Handle = new IntPtr();
    //        nint handle = Handle;
    //        //long* r = (long*)rowPtr.DataPtr.ToPointer();
    //        //long* c = (long*)colIndx.DataPtr.ToPointer();
    //        //double* v = (double*)values.DataPtr.ToPointer();
    //        Status = IntelMKLNative.mkl_sparse_d_create_csr_64(
    //            ref handle, SPARSE_IndexBase.ZeroBase, rows, cols,
    //            rowPtr.DataPtr, colIndx.DataPtr, values.DataPtr);
    //        Handle = handle;
    //    }

    //    /// <summary>
    //    /// COO constructor
    //    /// </summary>
    //    /// <param name="rows"></param>
    //    /// <param name="cols"></param>
    //    /// <param name="nnz"></param>
    //    /// <param name="rowIndx"></param>
    //    /// <param name="colIndx"></param>
    //    /// <param name="values"></param>
    //    public MatDi(long rows, long cols, long nnz,
    //        VectorI rowIndx, VectorI colIndx, VectorD values)
    //    {
    //        Rows = rows;
    //        Cols = cols;
    //        Handle = new IntPtr();
    //        nint handle = Handle;
    //        SPARSE_Status status = IntelMKLNative.mkl_sparse_d_create_coo_64(
    //            ref handle, SPARSE_IndexBase.ZeroBase, rows, cols, nnz,
    //            rowIndx.DataPtr, colIndx.DataPtr, values.DataPtr);
    //        Handle = handle;
    //        //Printer.Write($"sparse creation [coo]: {status}");
    //    }

    //    #endregion

    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    /// <param name="rows"></param>
    //    /// <param name="cols"></param>
    //    public void Export(out long rows, out long cols)
    //    {
    //        SPARSE_IndexBase indexBase = SPARSE_IndexBase.OneBase;
    //        rows = 0; cols = 0;
    //        IntPtr rows_start = new();
    //        IntPtr rows_end = new();
    //        IntPtr col_indx = new();
    //        IntPtr values = new();
    //        SPARSE_Status status = IntelMKLNative.mkl_sparse_d_export_csr_64(
    //            Handle, ref indexBase, ref rows, ref cols,
    //            ref rows_start, ref rows_end, ref col_indx, ref values);
    //        Printer.Write($"sparse export: {status}");
            
    //        long* rStart = (long*)rows_start.ToPointer();
    //        long* rEnd = (long*)rows_end.ToPointer();
    //        long* cIndx = (long*)col_indx.ToPointer();
    //        double* v = (double*)values.ToPointer();

    //        long nnz = *(rEnd + rows - 1);
    //        Printer.Write($"nnz = {nnz}");
    //        for (long i = 0; i < nnz; i++)
    //        {
    //            Printer.Write($"value[{i}]: {*(v + i)}");
    //        }

    //    }


    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    /// <param name="x"></param>
    //    /// <param name="y"></param>
    //    public void MV(VectorD x, out VectorD y)
    //    {
    //        y = new (count: Rows, 0.0);
    //        SPARSE_MatrixDescr matDes = new()
    //        {
    //            Type = SPARSE_MatrixType.General,
    //            Mode = SPARSE_FillMode.Full,
    //            Diag = SPARSE_DiagType.NonUnit
    //        };
    //        SPARSE_Status status = IntelMKLNative.mkl_sparse_d_mv_64(
    //            SPARSE_Operation.NonTranspose, 1.0, Handle,
    //            matDes, x.DataPtr, 1.0, y.DataPtr);
    //        Printer.Write($"sparse mv: {status}");
    //    }

    //}

    /// <summary>
    /// real-valued sparse matrix
    /// wrapper of [WMatrixDi] and extensions
    /// </summary>
    public class MatrixDi : WMatrixDi
    {
        #region properties

        private VectorI? Id0 { get; set; }
        private VectorI? Id1 { get; set; }
        private VectorD? Nzv {  get; set; }

        #endregion
        #region constructors

        /// <summary>
        /// constructs a WMatrixDi with given parameters
        /// </summary>
        /// <param name="rows"> number of rows </param>
        /// <param name="cols"> number of columns </param>
        /// <param name="nnz"> number of non-zero elements </param>
        public MatrixDi(long rows, long cols, long nnz)
            : base(rows, cols, nnz) 
        { }

        /// <summary>
        /// constructs a WMatrixDi with information 
        /// specified in the CSR format
        /// </summary>
        /// <param name="rows"> number of rows </param>
		/// <param name="cols"> number of columns </param>
		/// <param name="nnz"> number of non-zero elements </param>
        /// <param name="nzInfo"> non-zero information in the CSR format </param>
        public unsafe MatrixDi(long rows, long cols, long nnz, 
            MDiCSRInfo nzInfo)
            : base(rows, cols, nnz)
        {
            WLinAlg.SparseCreateCSR(a: this,
                rowPtr: nzInfo.RowPtr.TPtr,
                col_indx: nzInfo.ColIdx.TPtr,
                values: nzInfo.NzValues.SPtr);
            Id0 = nzInfo.RowPtr;
            Id1 = nzInfo.ColIdx;
            Nzv = nzInfo.NzValues;
        }

        /// <summary>
        /// constructs a WMatrixDi with information 
        /// specified in the CSC format
        /// </summary>
        /// <param name="rows"> number of rows </param>
		/// <param name="cols"> number of columns </param>
		/// <param name="nnz"> number of non-zero elements </param>
        /// <param name="nzInfo"> non-zero information in the CSC format </param>
        public unsafe MatrixDi(long rows, long cols, long nnz,
            MDiCSCInfo nzInfo)
            : base(rows, cols, nnz)
        {
            WLinAlg.SparseCreateCSC(a: this,
                colPtr: nzInfo.ColPtr.TPtr,
                row_indx: nzInfo.RowIdx.TPtr,
                values: nzInfo.NzValues.SPtr);
        }

        /// <summary>
        /// constructs a WMatrixDi with information 
        /// specified in the COO format
        /// </summary>
        /// <param name="rows"> number of rows </param>
		/// <param name="cols"> number of columns </param>
		/// <param name="nnz"> number of non-zero elements </param>
        /// <param name="nzInfo"> non-zero information in the COO format </param>
        public unsafe MatrixDi(long rows, long cols, long nnz,
            MDiCOOInfo nzInfo)
            : base(rows, cols, nnz)
        {
            WLinAlg.SparseCreateCOO(a: this,
                row_indx: nzInfo.RowIdx.TPtr,
                col_indx: nzInfo.ColIdx.TPtr,
                values: nzInfo.NzValues.SPtr);
        }

        /// <summary>
        /// copy constructor (deep copy)
        /// </summary>
        /// <param name="other"> another sparse matrix as the source </param>
        public unsafe MatrixDi(MatrixDi other)
            : base(other)
        { }

        #endregion
        #region methods


        //public void test()
        //{
            
        //}

        #endregion
    }

    /// <summary>
    /// non-zero information for a sparse matrix
    /// in the CSR format
    /// </summary>
    public class MDiCSRInfo
    {
        #region properties

        /// <summary>
        /// number of non-zero elements
        /// </summary>
        public long NZeros { get; set; }

        /// <summary>
        /// indices of row start/end
        /// </summary>
        public VectorI RowPtr { get; set; }

        /// <summary>
        /// column indices of non-zero elements
        /// </summary>
        public VectorI ColIdx { get; set; }

        /// <summary>
        /// values of non-zero elements
        /// </summary>
        public VectorD NzValues { get; set; }

        #endregion
        #region constructor

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="rowPtr"> indices of row start/end </param>
        /// <param name="colIdx"> column indices of non-zero elements </param>
        /// <param name="nzVal"> values of non-zero elements </param>
        public MDiCSRInfo(VectorI rowPtr, VectorI colIdx, VectorD nzVal)
        {
            if(colIdx.Count != nzVal.Count) { throw new ArgumentException(); }
            RowPtr = rowPtr;
            ColIdx = colIdx;
            NzValues = nzVal;
            NZeros = nzVal.Count;
        }

        #endregion
    }

    /// <summary>
    /// non-zero information for a sparse matrix
    /// in the CSC format
    /// </summary>
    public class MDiCSCInfo
    {
        #region properties

        /// <summary>
        /// number of non-zero elements
        /// </summary>
        public long NZeros { get; set; }

        /// <summary>
        /// indices of column start/end
        /// </summary>
        public VectorI ColPtr { get; set; }

        /// <summary>
        /// row indices of non-zero elements
        /// </summary>
        public VectorI RowIdx { get; set; }

        /// <summary>
        /// values of non-zero elements
        /// </summary>
        public VectorD NzValues { get; set; }

        #endregion
        #region constructor

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="colPtr"> indices of column start/end </param>
        /// <param name="rowIdx"> row indices of non-zero elements </param>
        /// <param name="nzVal"> values of non-zero elements </param>
        public MDiCSCInfo(VectorI colPtr, VectorI rowIdx, VectorD nzVal)
        {
            if (rowIdx.Count != nzVal.Count) { throw new ArgumentException(); }
            ColPtr = colPtr;
            RowIdx = rowIdx;
            NzValues = nzVal;
            NZeros = nzVal.Count;
        }

        #endregion
    }

    /// <summary>
    /// non-zero information for a sparse matrix
    /// in the COO format
    /// </summary>
    public class MDiCOOInfo
    {
        #region properties

        /// <summary>
        /// number of non-zero elements
        /// </summary>
        public long NZeros { get; set; }

        /// <summary>
        /// row indices of non-zero elements
        /// </summary>
        public VectorI RowIdx { get; set; }

        /// <summary>
        /// column indices of non-zero elements
        /// </summary>
        public VectorI ColIdx { get; set; }

        /// <summary>
        /// values of non-zero elements
        /// </summary>
        public VectorD NzValues { get; set; }

        #endregion
        #region constructor

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="rowIdx"> row indices of non-zero elements </param>
        /// <param name="colIdx"> column indices of non-zero elements </param>
        /// <param name="nzVal"> values of non-zero elements </param>
        public MDiCOOInfo(VectorI rowIdx, VectorI colIdx, VectorD nzVal)
        {
            if (rowIdx.Count != nzVal.Count || colIdx.Count != nzVal.Count) 
            { throw new ArgumentException(); }
            RowIdx = rowIdx;
            ColIdx = colIdx;
            NzValues = nzVal;
            NZeros = nzVal.Count;
        }

        #endregion
    }

}
