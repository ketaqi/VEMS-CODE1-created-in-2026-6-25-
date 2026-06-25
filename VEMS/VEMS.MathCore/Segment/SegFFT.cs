using System.Numerics;

namespace VEMS.MathCore
{
    /// <summary>
    /// segmented Fourier transform class
    /// using rectangular segment's aperture
    /// with smoothed edges on both sides
    /// </summary>
    public class SegFFT1D : UniformSeg1D
    {
        #region properties

        // ...

        #endregion
        #region constructors

        /// <summary>
        /// constructs a uniform segmentation
        /// with complementary rectangular segments
        /// </summary>
        /// <param name="ns"> number of segments </param>
        /// <param name="unitWidth"> width of the unit/elementary segment </param>
        /// <param name="edgeRatio"> edge ratio of the elementary segment </param>
        public SegFFT1D(int ns,
            double unitWidth,
            double edgeRatio = 0.0)
            : base(ns, unitWidth, edgeRatio)
        { }

        #endregion
        #region methods

        /// <summary>
        /// transforms the input data using segmented approach
        /// </summary>
        /// <param name="dIn"> input distribution (as GridData) </param>
        /// <param name="opt"> Fourier transform option: forward or backward </param>
        /// <param name="gLocal"> local uniform sampling grid for each segment </param>
        /// <param name="intrpl"> interpolation method used for the segment </param>
        /// <param name="bound"> data boundary option used for the segment </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> list of transformed results for each segment </returns>
        public List<Grid1DCplxData> Transform(Grid1DCplxData dIn, FFTOption opt,
            GridInfo1D gLocal,
            InterpolationMethod? intrpl = null,
            DataBoundary? bound = null,
            LoopMode loopMode = Defaults.LoopOption)
        {
            // takes all segments from input and samples them
            List<Grid1DCplxData> ds = TakeEachFrom(dIn, gLocal, intrpl, bound, loopMode);

            // loop over all segments
            for (int i = 0; i < Units.Count; i++)
            {
                // FFT kernel
                // !!! MathCore.Transform.FFT(x: ds[i].Values, grid: ds[i].GridInfo, opt: opt);
                // linear phase handling
                Samp1DCplxFunc expLinPhase = new(f:
                    (kx) => Complex.Exp(-Complex.ImaginaryOne * Centers[i] * kx));
                VectorZ t = ds[i].Values;
                expLinPhase.ScaleOn(x: ref t, grid: ds[i].GridInfo, loopMode: loopMode);
            }

            // return
            return ds;
        }

        #endregion
    }




}
