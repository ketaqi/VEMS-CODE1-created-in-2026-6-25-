namespace VEMS.AMathCore
{
    /// <summary>
    /// loop-computational mode options
    /// </summary>
    public enum LoopMode
    {
        /// <summary>
        /// using sequential loop
        /// </summary>
        Sequential,

        /// <summary>
        /// using parallel loop
        /// </summary>
        Parallel,

        /// <summary>
        /// using vectorized math
        /// </summary>
        Vectorized
    }
}
