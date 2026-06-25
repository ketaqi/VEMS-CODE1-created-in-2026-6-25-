namespace VEMS.AMathCore
{
    /// <summary>
    /// Array initialization mode options.
    /// </summary>
    /// <remarks>
    /// Specifies how memory for a <see cref="DenseArray{T}"/> should be initialized when the instance is created:
    /// <list type="bullet">
    /// <item><description><see cref="None"/>: Do not allocate memory (shallow); <see cref="DenseArray{T}.DataPtr"/> will be <see cref="IntPtr.Zero"/>.</description></item>
    /// <item><description><see cref="Calloc"/>: Allocate and zero-initialize memory (deep allocation using <c>IntelMKL.Calloc</c> when enabled).</description></item>
    /// <item><description><see cref="Malloc"/>: Allocate memory without initializing values (deep allocation using <c>IntelMKL.Malloc</c> when enabled).</description></item>
    /// </list>
    /// Use the mode that best matches your allocation and initialization requirements. Note that uninitialized memory (<see cref="Malloc"/>) may contain arbitrary data and should be explicitly initialized before use.
    /// </remarks>
    /// <seealso cref="DenseArray{T}"/>
    public enum ArrayInitMode
    {
        /// <summary>
        /// No memory allocation is performed for the array.
        /// <para>Use this mode for shallow generation, deferred allocation, or when wrapping existing native memory.</para>
        /// <para>When selected, <see cref="DenseArray{T}.DataPtr"/> is set to <see cref="IntPtr.Zero"/> and <see cref="DenseArray{T}.UseMKLalloc"/> is set to <c>false</c>.</para>
        /// </summary>
        None, // = 00, // commented out to avoid possible ambiguity ...

        /// <summary>
        /// Deep allocation with zero-initialization of all bytes.
        /// <para>Typically implemented via <c>IntelMKL.Calloc(Count, ElementByteSize)</c> when MKL allocation is enabled.</para>
        /// <para>Choose this mode when you require all elements to start at their default zero value.</para>
        /// </summary>
        Calloc, // = 01,

        /// <summary>
        /// Deep allocation without initializing the allocated memory.
        /// <para>Typically implemented via <c>IntelMKL.Malloc(totalByteSize)</c> when MKL allocation is enabled.</para>
        /// <para>This is faster than <see cref="Calloc"/> but leaves memory contents undefined; initialize elements before use.</para>
        /// </summary>
        Malloc // = 02
    }

}
