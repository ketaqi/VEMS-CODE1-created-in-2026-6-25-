using System.Numerics;

namespace VEMS.AMathCore
{
    /// <summary>
    /// Defines the interface for a matrix of elements of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">A numeric type implementing <see cref="INumber{T}"/>.</typeparam>
    public interface IMatx<T> where T : INumber<T>
    {
        #region properties

        /// <summary>
        /// Gets or sets the number of rows in the matrix.
        /// </summary>
        long Rows { get; set; }

        /// <summary>
        /// Gets or sets the number of columns in the matrix.
        /// </summary>
        long Cols { get; set; }

        /// <summary>
        /// Gets or sets the element at the specified row and column indices (Int64).
        /// </summary>
        /// <param name="iRow">The zero-based row index.</param>
        /// <param name="iCol">The zero-based column index.</param>
        /// <param name="checkBound">Whether to check bounds before accessing the element.</param>
        /// <returns>The element at the specified position.</returns>
        T this[long iRow, long iCol, bool checkBound = true] { get; set; }

        /// <summary>
        /// Gets or sets the element at the specified row and column indices (Int32).
        /// </summary>
        /// <param name="iRow">The zero-based row index.</param>
        /// <param name="iCol">The zero-based column index.</param>
        /// <param name="checkBound">Whether to check bounds before accessing the element.</param>
        /// <returns>The element at the specified position.</returns>
        T this[int iRow, int iCol, bool checkBound = true] { get; set; }

        /// <summary>
        /// Gets or sets the element at the specified row and column indices using <see cref="Index"/>.
        /// </summary>
        /// <param name="iRow">The row index as an <see cref="Index"/>.</param>
        /// <param name="iCol">The column index as an <see cref="Index"/>.</param>
        /// <param name="checkBound">Whether to check bounds before accessing the element.</param>
        /// <returns>The element at the specified position.</returns>
        T this[Index iRow, Index iCol, bool checkBound = true] { get; set; }

        #endregion
        #region methods

        // ...

        #endregion
        #region operators

        // ...

        #endregion
    }
}
