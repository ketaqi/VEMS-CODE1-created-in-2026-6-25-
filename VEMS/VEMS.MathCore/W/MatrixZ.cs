using System.Numerics;
using WMathCore;

namespace VEMS.MathCore.W
{

    /// <summary>
    /// complex-valued matrix class
    /// wrapper of [WMatrixZ] and extensions
    /// </summary>
    public class MatrixZ : WMatrixZ, IMatrix<Complex>
    {
        #region fields

        /// <summary>
        /// empty matrix with ZERO row and column count
        /// </summary>
        public static MatrixZ Empty = new(0, 0);

        #endregion
        #region constructors

        #region === wrappers ===

        /// <summary>
        /// constructs a matrix with given length
        /// </summary>
        /// <param name="rows"> number of rows </param>
        /// <param name="cols"> number of columns </param>
        public MatrixZ(long rows, long cols)
            : base(rows, cols)
        { }

        /// <summary>
        /// constructs a matrix with given size
        /// and sets all elements to the same value 
        /// </summary>
        /// <param name="rows"> number of rows </param>
        /// <param name="cols"> number of cols </param>
        /// <param name="initVal"> initial value for all the elements </param>
        public MatrixZ(long rows, long cols, Complex initVal)
            : base(rows, cols, new WComplex(initVal.Real, initVal.Imaginary), mode)
        { }

        /// <summary>
        /// constructs a matrix by copying from another
        /// </summary>
        /// <param name="source"> another matrix </param>
        public MatrixZ(MatrixZ source)
            : base(source, mode)
        { }

        /// <summary>
        /// constructs a complex matrix with its
        /// real- or imaginary part only
        /// </summary>
        /// <param name="part"> part of the complex vector </param>
        /// <param name="isRealPart"> is real- or imaginary-part </param>
        public MatrixZ(MatrixD part, bool isRealPart = true)
            : base(part, isRealPart, mode)
        { }

        #endregion
        #region === extended ===

        // ...

        #endregion

        #endregion
        #region properties

        #region === helpers ===

        /// <summary>
        /// mode option
        /// </summary>
        internal const int mode = 1;

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
        #region === wrappers ===

        /// <summary>
        /// number of rows
        /// </summary>
        public new long Rows
        {
            get => base.Rows;
            set => base.Rows = value;
        }

        /// <summary>
        /// number of columns
        /// </summary>
        public new long Cols
        {
            get => base.Cols;
            set => base.Cols = value;
        }

        /// <summary>
        /// gets / sets the value of a matrix element
        /// using row and column indices
        /// </summary>
        /// <param name="iRow"> row index [Int64] </param>
        /// <param name="iCol"> column index [Int64] </param>
        /// <param name="checkBound"> whether to check if the index is outside bound </param>
        /// <returns> element value </returns>
        public Complex this[long iRow, long iCol,
            bool checkBound = true]
        {
            get
            {
                bool invalidIndex = checkBound &&
                    (!IsRowIndexValid(iRow) || !IsColIndexValid(iCol));
                if (invalidIndex) { throw new IndexOutOfRangeException(); }
                WComplex val = base[iRow, iCol];
                return new Complex(val.Real, val.Imag);
            }
            set
            {
                bool invalidIndex = checkBound &&
                    (!IsRowIndexValid(iRow) || !IsColIndexValid(iCol));
                if (invalidIndex) { throw new IndexOutOfRangeException(); }
                WComplex val = new(value.Real, value.Imaginary);
                base[iRow, iCol] = val;
            }
        }

        #endregion
        #region === extended ===

        /// <summary>
        /// gets / sets the value of a matrix element
        /// using row and column indices
        /// </summary>
        /// <param name="iRow"> row index [Int32] </param>
        /// <param name="iCol"> column index [Int32] </param>
        /// <param name="checkBound"> whether to check if the index is outside bound </param>
        /// <returns> element value </returns>
        public Complex this[int iRow, int iCol,
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
        public Complex this[Index iRow, Index iCol,
            bool checkBound = true]
        {
            get => this[iRow.Value, iCol.Value, checkBound];
            set => this[iRow.Value, iCol.Value, checkBound] = value;
        }

        #endregion

        #endregion
        // methods ...
        // operators ...

    }
}
