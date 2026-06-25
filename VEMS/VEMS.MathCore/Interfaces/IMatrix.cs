
namespace VEMS.MathCore
{
    /// <summary>
    /// matrix interface
    /// </summary>
    /// <typeparam name="T"> double or comple </typeparam>
    public interface IMatrix<T> where T : struct
    {
        #region properties

        /// <summary>
        /// number of rows
        /// </summary>
        long Rows { get; set; }

        /// <summary>
        /// number of columns
        /// </summary>
        long Cols { get; set; }

        /// <summary>
        /// get / set the value of a matrix element
        /// using row and column indices
        /// </summary>
        /// <param name="iRow"> row index [Int64] </param>
        /// <param name="iCol"> column index [Int64] </param>
        /// <param name="checkBound"> whether to check if the index is outside bound </param>
        /// <returns> value of the element </returns>
        T this[long iRow, long iCol, bool checkBound = true] { get;set; }

        /// <summary>
        /// get / set the value of a matrix element
        /// using row and column indices
        /// </summary>
        /// <param name="iRow"> row index [Int32] </param>
        /// <param name="iCol"> column index [Int32] </param>
        /// <param name="checkBound"> whether to check if the index is outside bound </param>
        /// <returns> value of the element </returns>
        T this[int iRow, int iCol, bool checkBound = true] { get;set; }

        /// <summary>
        /// get / set the value of a matrix element
        /// using row and column indices
        /// </summary>
        /// <param name="iRow"> row index [Int32] </param>
        /// <param name="iCol"> column index [Int32] </param>
        /// <param name="checkBound"> whether to check if the index is outside bound </param>
        /// <returns> value of the element </returns>
        T this[Index iRow, Index iCol, bool checkBound = true] { get;set; }

        #endregion
        #region methods

        /// <summary>
        /// checks if a given row index is valid
        /// [lower bound = zero] 
        /// [upper bound = Rows]
        /// </summary>
        /// <param name="iRow"> input row index </param>
        /// <returns> true: if index is valid </returns>
        bool IsRowIndexValid(long iRow);

        /// <summary>
        /// checks if a given column index is valid
        /// [lower bound = zero] 
        /// [upper bound = Cols]
        /// </summary>
        /// <param name="iCol"> input column index </param>
        /// <returns> true: if index is valid </returns>
        bool IsColIndexValid(long iCol);

        ///// <summary>
        ///// sums up all the vector elements
        ///// </summary>
        ///// <returns> summed result </returns>
        //T Sum();

        ///// <summary>
        ///// finds the index of the element 
        ///// with minimum absolute value
        ///// </summary>
        ///// <returns> index of the element with minimum abs </returns>
        //long FindMinAbsIndex();

        ///// <summary>
        ///// finds the index of the element 
        ///// with maximum absolute value
        ///// </summary>
        ///// <returns> index of the element with maximum abs </returns>
        //long FindMaxAbsIndex();

        #endregion
    }



}
