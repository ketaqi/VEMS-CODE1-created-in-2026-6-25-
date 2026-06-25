namespace VEMS.MathCore
{

    /// <summary>
    /// One-dimensional real-valued scattered data
    /// </summary>
    public class Scat1DRealData
    {
        #region properties

        /// <summary>
        /// data values in a vector
        /// </summary>
        public VectorD Values { get; set; }

        /// <summary>
        /// scattered sampling information
        /// </summary>
        public ScatInfo1D ScatInfo { get; set; }

        /// <summary>
        /// locations of sampling points
        /// </summary>
        public VectorD Points { get => ScatInfo.Xs; }

        #endregion
        #region constructors

        /// <summary>
        /// constructs a default Scat1DRealData
        /// with only one point and one value
        /// </summary>
        public Scat1DRealData() 
        {
            ScatInfo = new(1);
            Values = new(1);
        }

        /// <summary>
        /// constructs a Scat1DRealData with given
        /// scattered sampling info and values 
        /// without deep copy of data
        /// </summary>
        /// <param name="scatInfo"> scattered sampling points info </param>
        /// <param name="values"> values at sampling points </param>
        public Scat1DRealData(ScatInfo1D scatInfo, VectorD values)
        {
            if(scatInfo.Count != values.Count) { throw new ArgumentException(); }
            ScatInfo = scatInfo; 
            Values = values; 
        }

        /// <summary>
        /// constructs a Scat1DRealData with given
        /// point locations and values
        /// </summary>
        /// <param name="xs"> locations of scattered sampling points </param>
        /// <param name="values"> values at the sampling points </param>
        [Obsolete]
        public Scat1DRealData(VectorD xs, VectorD values)
        {
            ScatInfo = new(xs);
            Values = values;
        }

        #endregion
    }

}
