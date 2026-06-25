using VEMS.MathCore;

namespace VEMS.EMSolver
{

    /// <summary>
    /// segmentation for 1D field
    /// </summary>
    public class SegField1D : UniformSeg1D
    {
        #region properties

        // ...

        #endregion
        #region constructors

        /// <summary>
        /// constructs segmentation for 1D field
        /// with uniform rectangular segments
        /// </summary>
        /// <param name="centers"> centers of the segments </param>
        /// <param name="unit"> elementary segment unit with varying center </param>
        public SegField1D(GridInfo1D centers,
            Segment1D.CosRect unit)
            : base(centers, unit)
        { }

        #endregion
        #region methods

        // takes from ...

        #endregion
    }
}
