namespace VEMS.AMathCore
{
    /// <summary>
    /// options for handling of values beyond data range
    /// </summary>
    public enum DataBoundary
    {
        /// <summary>
        /// constant zero beyond data range
        /// </summary>
        ConstantZero,

        /// <summary>
        /// periodic replicated beyond data range
        /// </summary>
        Periodic,

        /// <summary>
        /// Represents a constant value that does not change during the execution of the program.
        /// </summary>
        /// <remarks>This class or member is typically used to define immutable values that are shared
        /// across multiple parts of an application. Constant values are often used for configuration, default settings,
        /// or fixed data.</remarks>
        ConstantValue,

        // ...
    }

}
