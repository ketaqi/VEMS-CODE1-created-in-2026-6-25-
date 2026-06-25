namespace VEMS.EMSolver
{

    /// <summary>
    /// base-class for ideal lenses
    /// </summary>
    [Obsolete]
    public class IdealLensBase
    {
        #region properties

        /// <summary>
        /// focal length of the lens
        /// </summary>
        public double FocalLength { get; set; }

        /// <summary>
        /// working wavelength in vacuum
        /// </summary>
        public double Wavelength { get; set; }

        /// <summary>
        /// refractive index [real or real-part only]
        /// </summary>
        public Func<double, double>? NReal { get; set; }

        /// <summary>
        /// constant offset of the phase function
        /// </summary>
        public double Offset { get; set; }

        #endregion
    }

    /// <summary>
    /// lens model interface
    /// </summary>
    public interface ILens
    {

        /// <summary>
        /// focal length
        /// </summary>
        double FocalLength { get; set; }

        /// <summary>
        /// working wavelength in vacuum
        /// </summary>
        public double Wavelength { get; set; }

        /// <summary>
        /// refractive index [real or real-part only]
        /// </summary>
        public Func<double, double> NReal { get; set; }

        /// <summary>
        /// constant offset of the phase function
        /// </summary>
        public double Offset { get; set; }

    }


}
