namespace VEMS.AMathCore
{
    /// <summary>
    /// Specifies the copy semantics for constructing or duplicating a <see cref="DenseArray{T}"/>.
    /// </summary>
    /// <remarks>
    /// Use <see cref="Deep"/> to allocate new memory and copy contents. Use <see cref="Shallow"/> to reference the same underlying buffer.
    /// </remarks>
    public enum ArrayCopyMode
    {
        /// <summary>
        /// Shallow copy: instance will reference the same underlying memory buffer as the source.
        /// </summary>
        Shallow = 00,

        /// <summary>
        /// Deep copy: allocate new memory for the destination and copy the contents from the source.
        /// </summary>
        Deep = 01,
    }

}
