namespace VEMS.MathCore
{

    /// <summary>
    /// One-dimensional complex-valued scattered data
    /// </summary>
    public class Scat1DCplxData
    {
        #region properties

        /// <summary>
        /// data values
        /// </summary>
        public VectorZ Values { get; set; }

        /// <summary>
        /// scattered sampling info
        /// </summary>
        public ScatInfo1D ScatInfo { get; set; }

        /// <summary>
        /// locations of sampling points
        /// </summary>
        public VectorD Points { get => ScatInfo.Xs; }

        #endregion
        #region constructors

        /// <summary>
        /// constructs a default Scat1DCplxData
        /// with only one point and one value
        /// </summary>
        public Scat1DCplxData() 
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
        public Scat1DCplxData(ScatInfo1D scatInfo, VectorZ values)
        {
            if(scatInfo.Count != values.Count) { throw new ArgumentException(); }
            ScatInfo = scatInfo;
            Values = values;
        }

        /// <summary>
        /// constructs a Scat1DCplxData with given
        /// point locations and values
        /// </summary>
        /// <param name="xs"> locations of sampling points </param>
        /// <param name="values"> values at sampling points </param>
        [Obsolete]
        public Scat1DCplxData(VectorD xs, VectorZ values)
        {
            ScatInfo = new(xs);
            Values = new(values, true);
        }

        #endregion
    }

}
