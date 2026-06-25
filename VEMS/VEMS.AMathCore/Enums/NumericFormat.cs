namespace VEMS.AMathCore
{
    /// <summary>
    /// format of number
    /// </summary>
    public enum NumericFormat
    {

        /// <summary>
        /// currency value
        /// </summary>
        Currency,

        /// <summary>
        /// exponential notation (scientific)
        /// </summary>
        Exponential,

        /// <summary>
        /// fixed decimal notation
        /// </summary>
        FixedPoint,

        /// <summary>
        /// compact format of either fixed-point 
        /// or scientific notation
        /// </summary>
        General,

        /// <summary>
        /// number group separators notation
        /// </summary>
        Number,

        /// <summary>
        /// multiplied by 100 and displayed with 
        /// a percent symbol
        /// </summary>
        Percent
    }

}
