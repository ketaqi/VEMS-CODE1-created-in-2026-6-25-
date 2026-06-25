namespace VEMS.MathCore
{

    /// <summary>
    /// data fitting class
    /// </summary>
    public class Fitting
    {

        #region B-Spline [static]

        /// <summary>
        /// fits data according to another uniform grid
        /// </summary>
        /// <param name="v"> uniformly sampled input values </param>
        /// <param name="inputGrid"> input sampling grid </param>
        /// <param name="targetGrid"> target sampling grid </param>
        /// <param name="degree"> polynomial degree, default is 3 </param>
        /// <param name="nFactor"> factor between 0.0 and 1.0 for defining the number of knots spans </param>
        /// <param name="knots"></param>
        /// <param name="checkFitError"></param>
        /// <returns> interpolated values on the target grid </returns>
        public static VectorD BSpline(VectorD v, GridInfo1D inputGrid,
            GridInfo1D targetGrid,
            long degree = 3, double nFactor = 0.7, GridInfo1D? knots = null,
            bool checkFitError = false)
        {
            // makes clamped B-spline fitter
            ClampedBSpline1D fitter = new(grid: inputGrid, values: v,
                degree: degree, nFactor: nFactor, knots: knots,
                checkFitError: checkFitError);
            // evaluates
            return fitter.Evaluate(targetGrid.GetCoordinates());
        }

        /// <summary>
        /// fits data according to another uniform grid
        /// </summary>
        /// <param name="v"> uniformly sampled input values </param>
        /// <param name="inputGrid"> input sampling grid </param>
        /// <param name="targetGrid"> target sampling grid </param>
        /// <param name="degree"> polynomial degree, default is 3 </param>
        /// <param name="nFactor"> factor between 0.0 and 1.0 for defining the number of knots spans </param>
        /// <param name="knots"></param>
        /// <param name="checkFitError"></param>
        /// <returns> interpolated values on the target grid </returns>
        public static VectorZ BSpline(VectorZ v, GridInfo1D inputGrid,
            GridInfo1D targetGrid,
            long degree = 3, double nFactor = 0.7, GridInfo1D? knots = null,
            bool checkFitError = false)
        {
            // extracts real and imag-parts
            VectorD vRe = new(inputGrid.Count);
            VectorD vIm = new(inputGrid.Count);
            VMath.RealImagParts(v, ref vRe, ref vIm);
            // fits respectively
            VectorD yRe = BSpline(vRe, inputGrid, targetGrid,
                degree, nFactor, knots, checkFitError);
            VectorD yIm = BSpline(vIm, inputGrid, targetGrid,
                degree, nFactor, knots, checkFitError);
            // return
            return VMath.Construct(yRe, yIm);
        }

        #endregion


    }
}
