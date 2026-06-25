namespace VEMS.MathCore
{

    /// <summary>
    /// Two-dimensional complex-valued scattered data
    /// </summary>
    public class Scat2DCplxData
    {
        #region properties

        /// <summary>
        /// data values
        /// </summary>
        public VectorZ Values { get; set; }

        /// <summary>
        /// scattered sampling information
        /// </summary>
        public ScatInfo2D ScatInfo { get; set; }

        /// <summary>
        /// x-component of the locations of sampling points
        /// </summary>
        public VectorD PointsX { get => ScatInfo.Xs; }

        /// <summary>
        /// y-component of the locations of sampling points
        /// </summary>
        public VectorD PointsY { get => ScatInfo.Ys; }

        #endregion
        #region constructors

        /// <summary>
        /// constructs a default Scat2DCmpxData
        /// </summary>
        public Scat2DCplxData()
        {
            ScatInfo = new(1);
            Values = new(1);
        }


        /// <summary>
        /// constructs a Scat2DRealData with given 
        /// scattered sampling information and data values
        /// without deep copy of data
        /// </summary>
        /// <param name="scatInfo"> scattered sampling information </param>
        /// <param name="values"> values at the sampling points </param>
        public Scat2DCplxData(ScatInfo2D scatInfo, VectorZ values)
        {
            if(scatInfo.Count != values.Count) { throw new ArgumentException(); }
            ScatInfo = scatInfo;
            Values = values;
        }

        #endregion
    }


}
