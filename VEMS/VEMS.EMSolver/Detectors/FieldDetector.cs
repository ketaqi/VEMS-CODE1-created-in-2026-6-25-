namespace VEMS.EMSolver
{

    /// <summary>
    /// Interface for field detectors, specifying the required properties for detection.
    /// </summary>
    public interface IDetector
    {

        // polarization ...

        /// <summary>
        /// Gets or sets the field quantity to detect.
        /// </summary>
        DetectQuantity Quantity { get; set; }

        /// <summary>
        /// Gets or sets the pixelation mode of the detector.
        /// </summary>
        PixelationMode PixelMode { get; set; }

    }


    /// <summary>
    /// Specifies the mode for summing fields in the detector.
    /// </summary>
    public enum SumMode
    {
        /// <summary>
        /// Coherent sum of fields, where both amplitude and phase are considered.
        /// </summary>
        Coherent,

        /// <summary>
        /// Incoherent sum of fields, where only the intensities are summed.
        /// </summary>
        Incoherent
    }

    /// <summary>
    /// Specifies the quantity of the field to be detected by the detector.
    /// </summary>
    public enum DetectQuantity
    {
        /// <summary>
        /// The real part of the complex field.
        /// </summary>
        RealPart,

        /// <summary>
        /// The imaginary part of the complex field.
        /// </summary>
        ImagPart,

        /// <summary>
        /// The magnitude (absolute value) of the complex field.
        /// </summary>
        Magnitude,

        /// <summary>
        /// The phase (argument) of the complex field, in radians.
        /// </summary>
        Argument,

        /// <summary>
        /// The squared magnitude (intensity) of the complex field.
        /// </summary>
        SquaredMagnitude,

        /// <summary>
        /// The power density of the field.
        /// </summary>
        PowerDensity
    }

    /// <summary>
    /// Specifies the pixelation mode for the detector, determining how the field values are sampled or approximated within each pixel.
    /// </summary>
    public enum PixelationMode
    {
        /// <summary>
        /// Pixel value is determined by integrating the field within the pixel area.
        /// </summary>
        Integral,

        /// <summary>
        /// Pixel value is approximated using a linear fit of the field within the pixel.
        /// </summary>
        LinearFit,

        /// <summary>
        /// Pixel value is approximated by the nearest neighbor method, using the value at the closest point.
        /// </summary>
        Nearest,

        /// <summary>
        /// Pixel value is determined using the original interpolation method of the provided data.
        /// </summary>
        Original
    }

}
