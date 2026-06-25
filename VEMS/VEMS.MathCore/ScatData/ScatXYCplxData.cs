namespace VEMS.MathCore
{

    /// <summary>
    /// Separable two-dimensional real-valued scattered data
    /// </summary>
    public class ScatXYCplxData
    {
        #region properties

        /// <summary>
        /// values distributed on the scattered points
        /// </summary>
        public MatrixZ Values { get; set; }

        /// <summary>
        /// scattered sampling information
        /// </summary>
        public ScatInfoXY ScatInfo { get; set; }

        /// <summary>
        /// scattered locations in the x direction
        /// </summary>
        public VectorD PointsX { get => ScatInfo.Xs; }

        /// <summary>
        /// scattered locations in the y direction
        /// </summary>
        public VectorD PointsY { get => ScatInfo.Ys; }

        #endregion
        #region constructors

        /// <summary>
        /// constructs a default x-y-separably scattered data
        /// </summary>
        public ScatXYCplxData()
        {
            ScatInfo = new(1, 1);
            Values = new(1, 1);
        }

        /// <summary>
        /// constructs a x-y-separably scattered data with given
        /// scattered sampling information and data values
        /// without deep copy of data
        /// </summary>
        /// <param name="scatInfo"> scattered sampling information </param>
        /// <param name="values"> values distributed on the scattered points </param>
        public ScatXYCplxData(ScatInfoXY scatInfo, MatrixZ values)
        {
            if (scatInfo.Rows != values.Rows || scatInfo.Cols != values.Cols)
                throw new ArgumentException();
            ScatInfo = scatInfo;
            Values = values;
        }

        #endregion
    }
}
