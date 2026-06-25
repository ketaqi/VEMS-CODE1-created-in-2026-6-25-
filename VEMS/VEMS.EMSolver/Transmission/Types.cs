
namespace VEMS.EMSolver
{
    /// <summary>
    /// type of transmission: amplitude or phase
    /// (or both i.e. complex?)
    /// </summary>
    public enum TransmissionType
    {
        /// <summary>
        /// amplitude-modulation
        /// </summary>
        Amplitude,

        /// <summary>
        /// phase modulation
        /// </summary>
        Phase,

        ///// <summary>
        ///// complex modulation
        ///// </summary>
        //Complex
    }

    /// <summary>
    /// types of 2D aperture
    /// </summary>
    public enum ApertureShape
    {
        /// <summary>
        /// rectangular truncation
        /// </summary>
        Rectangular,

        /// <summary>
        /// elliptical truncation
        /// </summary>
        Elliptical,

        /// <summary>
        /// circular truncation
        /// </summary>
        Circular

    }
}
