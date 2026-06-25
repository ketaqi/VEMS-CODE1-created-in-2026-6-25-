using WMathCore;

namespace VEMS.MathCore
{
    /// <summary>
    /// complex-valued sparse matrix
    /// wrapper of [WMatrixZi] and extensions
    /// </summary>
    public class MatrixZi : WMatrixZi
    {
        #region constructors

        /// <summary>
        /// constructs a WMatrixZi with given parameters
        /// </summary>
        /// <param name="rows"> number of rows </param>
        /// <param name="cols"> number of columns </param>
        /// <param name="nnz"> number of non-zero elements </param>
        public MatrixZi(long rows, long cols, long nnz)
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
        public unsafe MatrixZi(long rows, long cols, long nnz,
            MZiCSRInfo nzInfo)
            : base(rows, cols, nnz)
        {
            WLinAlg.SparseCreateCSR(a: this,
                rowPtr: nzInfo.RowPtr.TPtr,
                col_indx: nzInfo.ColIdx.TPtr,
                values: nzInfo.NzValues.SPtr);
        }

        /// <summary>
        /// constructs a WMatrixZi with information 
        /// specified in the CSC format
        /// </summary>
        /// <param name="rows"> number of rows </param>
		/// <param name="cols"> number of columns </param>
		/// <param name="nnz"> number of non-zero elements </param>
        /// <param name="nzInfo"> non-zero information in the CSC format </param>
        public unsafe MatrixZi(long rows, long cols, long nnz,
            MZiCSCInfo nzInfo)
            : base(rows, cols, nnz)
        {
            WLinAlg.SparseCreateCSC(a: this,
                colPtr: nzInfo.ColPtr.TPtr,
                row_indx: nzInfo.RowIdx.TPtr,
                values: nzInfo.NzValues.SPtr);
        }

        /// <summary>
        /// constructs a WMatrixZi with information 
        /// specified in the COO format
        /// </summary>
        /// <param name="rows"> number of rows </param>
		/// <param name="cols"> number of columns </param>
		/// <param name="nnz"> number of non-zero elements </param>
        /// <param name="nzInfo"> non-zero information in the COO format </param>
        public unsafe MatrixZi(long rows, long cols, long nnz,
            MZiCOOInfo nzInfo)
            : base(rows, cols, nnz)
        {
            WLinAlg.SparseCreateCOO(a: this,
                row_indx: nzInfo.RowIdx.TPtr,
                col_indx: nzInfo.ColIdx.TPtr,
                values: nzInfo.NzValues.SPtr);
        }

        #endregion

    }

    /// <summary>
    /// non-zero information for a sparse matrix
    /// in the CSR format
    /// </summary>
    public class MZiCSRInfo
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
        public VectorZ NzValues { get; set; }

        #endregion
        #region constructor

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="rowPtr"> indices of row start/end </param>
        /// <param name="colIdx"> column indices of non-zero elements </param>
        /// <param name="nzVal"> values of non-zero elements </param>
        public MZiCSRInfo(VectorI rowPtr, VectorI colIdx, VectorZ nzVal)
        {
            if (colIdx.Count != nzVal.Count) { throw new ArgumentException(); }
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
    public class MZiCSCInfo
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
        public VectorZ NzValues { get; set; }

        #endregion
        #region constructor

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="colPtr"> indices of column start/end </param>
        /// <param name="rowIdx"> row indices of non-zero elements </param>
        /// <param name="nzVal"> values of non-zero elements </param>
        public MZiCSCInfo(VectorI colPtr, VectorI rowIdx, VectorZ nzVal)
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
    public class MZiCOOInfo
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
        public VectorZ NzValues { get; set; }

        #endregion
        #region constructor

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="rowIdx"> row indices of non-zero elements </param>
        /// <param name="colIdx"> column indices of non-zero elements </param>
        /// <param name="nzVal"> values of non-zero elements </param>
        public MZiCOOInfo(VectorI rowIdx, VectorI colIdx, VectorZ nzVal)
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
