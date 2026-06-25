namespace VEMS.EMSolver
{

    /// <summary>
    /// base class for 1D RCWA
    /// </summary>
    public class RCWA1DBase
    {
        #region properties

        /// <summary>
        /// fixed working wavelength defined in vacuum
        /// </summary>
        public double Wavelength { get; set; }

        /// <summary>
        /// uniform layer in front 
        /// </summary>
        public UniformLayer? LayerFront { get; set; }

        /// <summary>
        /// 1D-periodic layer in the middle
        /// </summary>
        public Periodic1DLayer? LayerMiddle { get; set; }

        /// <summary>
        /// uniform layer behind
        /// </summary>
        public UniformLayer? LayerBehind { get; set; }

        #endregion
        #region constructors

        /// <summary>
        /// constructs a default 1D-RCWA solver
        /// for in-plane case
        /// </summary>
        internal RCWA1DBase() { }

        /// <summary>
        /// constructs a RCWA (1D) base solver
        /// </summary>
        /// <param name="wavelength"> working wavelength given in vacuum </param>
        /// <param name="materialFront"> material of the front layer </param>
        /// <param name="mediumMiddle"> medium of the middle layer </param>
        /// <param name="period"> period of the middle layer </param>
        /// <param name="thickness"> thickness of the middle layer </param>
        /// <param name="materialBehind"> material of the behind layer </param>
        public RCWA1DBase(double wavelength,
            Material materialFront,
            ILayerMedium mediumMiddle,
            double period, double thickness,
            Material materialBehind)
        {
            Wavelength = wavelength;
            // layers
            LayerFront = new(m: materialFront);
            LayerMiddle = new(period: period, medium: mediumMiddle, thickness: thickness);
            LayerBehind = new(m: materialBehind);
        }

        #endregion
        // methods ...
    }


}
