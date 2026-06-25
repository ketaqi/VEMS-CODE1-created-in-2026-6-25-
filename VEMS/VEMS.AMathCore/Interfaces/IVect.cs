using System.Numerics;

namespace VEMS.AMathCore
{
    /// <summary>
    /// Interface for a vector of type <see cref="INumber{T}"/>.
    /// Provides indexers for accessing and modifying vector elements by various index types.
    /// </summary>
    /// <typeparam name="T">A numeric type implementing <see cref="INumber{T}"/>.</typeparam>
    public interface IVect<T> where T : INumber<T>
    {
        #region properties

        /// <summary>
        /// Gets or sets the value of a vector element at the specified <see cref="Int64"/> index.
        /// </summary>
        /// <param name="i">The index of the element (Int64).</param>
        /// <param name="checkBound">Whether to check if the index is outside the valid bounds.</param>
        /// <returns>The value of the element at the specified index.</returns>
        T this[long i, bool checkBound = true] { get; set; }

        /// <summary>
        /// Gets or sets the value of a vector element at the specified <see cref="Int32"/> index.
        /// </summary>
        /// <param name="i">The index of the element (Int32).</param>
        /// <param name="checkBound">Whether to check if the index is outside the valid bounds.</param>
        /// <returns>The value of the element at the specified index.</returns>
        T this[int i, bool checkBound = true] { get; set; }

        /// <summary>
        /// Gets or sets the value of a vector element at the specified <see cref="Index"/> index.
        /// </summary>
        /// <param name="i">The index of the element (Index).</param>
        /// <param name="checkBound">Whether to check if the index is outside the valid bounds.</param>
        /// <returns>The value of the element at the specified index.</returns>
        T this[Index i, bool checkBound = true] { get; set; }


        /// <summary>
        /// Gets or sets a subvector defined by a <see cref="LongRange"/>.
        /// </summary>
        /// <param name="rng">
        /// The <see cref="LongRange"/> specifying the inclusive start and exclusive end indices of the subvector.
        /// </param>
        /// <returns>
        /// A <see cref="Vect{T}"/> containing the elements within the specified range.
        /// </returns>
        /// <remarks>
        /// Getting returns a new vector containing the elements in the specified range.
        /// Setting replaces the elements in the specified range with those from the provided vector.
        /// </remarks>
        Vect<T> this[LongRange rng] { get; set; }

        /// <summary>
        /// Gets or sets a subvector defined by a <see cref="LongRange"/>.
        /// </summary>
        /// <param name="rng">
        /// The <see cref="LongRange"/> specifying the inclusive start and exclusive end indices of the subvector.
        /// </param>
        /// <returns>
        /// A <see cref="Vect{T}"/> containing the elements within the specified range.
        /// </returns>
        /// <remarks>
        /// Getting returns a new vector containing the elements in the specified range.
        /// Setting replaces the elements in the specified range with those from the provided vector.
        /// </remarks>
        Vect<T> this[Range rng] { get; set; }

        #endregion
        #region methods

        ///// <summary>
        ///// Checks if a given index is valid.
        ///// Lower bound is zero; upper bound is <c>Count</c>.
        ///// </summary>
        ///// <param name="i">Input index.</param>
        ///// <returns><c>true</c> if the index is valid; otherwise, <c>false</c>.</returns>
        //bool IsIndexValid(long i);

        // ...

        #endregion
        #region operators

        // ...

        #endregion
    }
}
