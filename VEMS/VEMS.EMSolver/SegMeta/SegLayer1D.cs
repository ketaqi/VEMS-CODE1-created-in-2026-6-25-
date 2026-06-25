using VEMS.MathCore;
using Complex = System.Numerics.Complex;

namespace VEMS.EMSolver
{

    /// <summary>
    /// segmented (1D) metasurface calculator
    /// </summary>
    public class SegLayer1D : UniformSeg1D
    {
        #region properties



        #endregion
        #region constructors

        /// <summary>
        /// constructs segmentation for 1D layer medium
        /// with uniform rectangular segments
        /// </summary>
        /// <param name="centers"> centers of the segments </param>
        /// <param name="unit"> elementary segment unit with varying center </param>
        public SegLayer1D(GridInfo1D centers,
            Segment1D.CosRect unit)
            : base(centers, unit)
        { }

        #endregion
        #region methods

        /// <summary>
        /// takes a segment from a 1D layer medium 
        /// e.g. a metasurface
        /// </summary>
        /// <param name="iSeg"> index of the segment </param>
        /// <param name="medium"> layer medium </param>
        /// <returns> content within the segment </returns>
        public Layer1DMedium TakeFrom(int iSeg, Layer1DMedium medium)
        {
            // defines segment's center according to index
            //Unit.Center = Centers[iSeg];
            // takes epsilon
            Func<double, double, Complex> epsilon = (w, x) =>
            {
                Func<double, Complex> fx =
                    Units[iSeg].TakeFrom(fIn: (x) => medium.Epsilon(w, x));
                return fx(x);
            };
            // takes mu
            Func<double, double, Complex>? mu = null;
            if (medium.Mu != null)
            {
                mu = (w, x) =>
                {
                    Func<double, Complex> fx 
                        = Units[iSeg].TakeFrom(fIn: (x) => medium.Mu(w, x));
                    return fx(x);
                };
            }
            // makes a new medium and return
            return new Layer1DMedium(epsilon: epsilon, mu: mu);
        }

        #endregion
    }
}
