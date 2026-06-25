namespace VEMS.AMathCore
{
    /// <summary>
    /// options for interpolation method
    /// </summary>
    public enum InterpolationMethod
    {
        /// <summary>
        /// sinc interpolation 
        /// </summary>
        Sinc,

        /// <summary>
        /// sinc-FFT interpolation
        /// </summary>
        SincFFT,

        /// <summary>
        /// nearest interpolation
        /// </summary>
        Nearest,

        /// <summary>
        /// (bi)-linear interpolation
        /// </summary>
        Linear,

        /// <summary>
        /// (bi)-cubic interpolation
        /// </summary>
        Cubic
    }

}
