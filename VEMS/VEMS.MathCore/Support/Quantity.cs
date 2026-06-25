namespace VEMS.MathCore
{
    /// <summary>
    /// physical quantity with value and unit
    /// </summary>
    public class PhysicalQuantity
    {
        #region properties

        /// <summary>
        /// internal value in SI unit
        /// </summary>
        private double val;

        /// <summary>
        /// value in the given unit 
        /// </summary>
        public double Value 
        { 
            get => val / Unit.Scaling;
            set => val = value * Unit.Scaling;
        }

        /// <summary>
        /// unit of the physical quantity
        /// </summary>
        public Unit Unit { get; set; }

        #endregion
        #region constructor

        /// <summary>
        /// constructs a physical quantity with value and unit
        /// </summary>
        /// <param name="value"></param>
        /// <param name="unit"></param>
        public PhysicalQuantity(double value, Unit unit)
        {
            val = value * unit.Scaling;
            Unit = unit;
        }

        #endregion
    }

}
