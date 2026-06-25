using WMathCore;

namespace VEMS.MathCore.W
{

    /// <summary>
    /// real-valued matrix class
    /// wrapper of [WMatrixD] and extensions 
    /// </summary>
    public class MatrixD : WMatrixD, IMatrix<double>
    {
        #region fields

        /// <summary>
        /// empty matrix with ZERO row and column count
        /// </summary>
        public static MatrixD Empty = new(0, 0);

        #endregion
        #region properties

        /// <summary>
        /// mode option
        /// </summary>
        internal const int mode = 1;

        #region === interface ===

        /// <summary>
        /// gets / sets the value of a matrix element
        /// using row and column indices
        /// </summary>
        /// <param name="iRow"> row index [Int64] </param>
        /// <param name="iCol"> column index [Int64] </param>
        /// <param name="checkBound"> whether to check if the index is outside bound </param>
        /// <returns> element value </returns>
        public double this[long iRow, long iCol,
            bool checkBound = true]
        {
            get
            {
                bool invalidIndex = checkBound &&
                    (!IsRowIndexValid(iRow) || !IsColIndexValid(iCol));
                if (invalidIndex) { throw new IndexOutOfRangeException(); }
                return base[iRow, iCol];
            }
            set
            {
                bool invalidIndex = checkBound &&
                    (!IsRowIndexValid(iRow) || !IsColIndexValid(iCol));
                if (invalidIndex) { throw new IndexOutOfRangeException(); }
                base[iRow, iCol] = value;
            }
        }

        /// <summary>
        /// gets / sets the value of a matrix element
        /// using row and column indices
        /// </summary>
        /// <param name="iRow"> row index [Int32] </param>
        /// <param name="iCol"> column index [Int32] </param>
        /// <param name="checkBound"> whether to check if the index is outside bound </param>
        /// <returns> element value </returns>
        public double this[int iRow, int iCol,
            bool checkBound = true]
        {
            get => this[Convert.ToInt64(iRow), Convert.ToInt64(iCol), checkBound];
            set => this[Convert.ToInt64(iRow), Convert.ToInt64(iCol), checkBound] = value;
        }

        /// <summary>
        /// gets / sets the value of a matrix element
        /// using row and column indices
        /// </summary>
        /// <param name="iRow"> row index [Int32] </param>
        /// <param name="iCol"> column index [Int32] </param>
        /// <param name="checkBound"> whether to check if the index is outside bound </param>
        /// <returns> element value </returns>
        public double this[Index iRow, Index iCol,
            bool checkBound = true]
        {
            get => this[iRow.Value, iCol.Value, checkBound];
            set => this[iRow.Value, iCol.Value, checkBound] = value;
        }

        #endregion

        #endregion
        #region constructors

        #region === wrappers ===

        /// <summary>
        /// constructs a matrix with given length
        /// </summary>
        /// <param name="rows"> number of rows </param>
        /// <param name="cols"> number of columns </param>
        public MatrixD(long rows, long cols)
            : base(rows, cols)
        { }

        /// <summary>
        /// constructs a matrix with given size
        /// and sets all elements to the same value 
        /// </summary>
        /// <param name="rows"> number of rows </param>
        /// <param name="cols"> number of cols </param>
        /// <param name="initVal"> initial value for all the elements </param>
        public MatrixD(long rows, long cols, double initVal)
            : base(rows, cols, initVal, mode)
        { }

        /// <summary>
        /// constructs a matrix by copying from another
        /// </summary>
        /// <param name="source"> another matrix </param>
        public MatrixD(MatrixD source)
            : base(source, mode)
        { }

        #endregion
        #region === extended ===

        // ...

        #endregion

        #endregion
        #region methods 

        #region === interface ===

        /// <summary>
        /// checks if a given row index is valid
        /// [lower bound = zero] 
        /// [upper bound = Rows]
        /// </summary>
        /// <param name="i"> input row index </param>
        /// <returns> true: if index is valid </returns>
        public bool IsRowIndexValid(long i)
        {
            if (i >= 0 && i < Rows) { return true; }
            else { return false; }
        }

        /// <summary>
        /// checks if a given column index is valid
        /// [lower bound = zero] 
        /// [upper bound = Cols]
        /// </summary>
        /// <param name="i"> input column index </param>
        /// <returns> true: if index is valid </returns>
        public bool IsColIndexValid(long i)
        {
            if (i >= 0 && i < Cols) { return true; }
            else { return false; }
        }

        #endregion

        #endregion

        // operators ...

    }



    public class NA
    {
        public void test()
        {
            MatrixD m = new(3, 4);
            
        }
    }

}
