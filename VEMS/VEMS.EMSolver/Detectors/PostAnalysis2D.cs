using VEMS.MathCore;

namespace VEMS.EMSolver
{


    /// <summary>
    /// Provides post-analysis methods for 2D grid data, such as centroid and radius calculations.
    /// </summary>
    public class PostAnalysis2D
    {
        /// <summary>
        /// Finds the centroid (center of mass) of the given 2D intensity grid.
        /// </summary>
        /// <param name="intensity">The 2D grid data representing intensity values.</param>
        /// <param name="loopMode">The loop computation mode (default: Defaults.LoopOption).</param>
        /// <returns>
        /// A tuple containing the x and y coordinates of the centroid.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if the intensity values are null.</exception>
        public static (double, double) FindCentroid(
            Grid2DRealData intensity,
            LoopMode loopMode = Defaults.LoopOption)
        {
            if (intensity.Values == null) { throw new ArgumentNullException(nameof(intensity.Values)); }

            // local variables
            MatrixD vals = intensity.Values;
            GridInfo2D grid = intensity.GridInfo;
            long rows = grid.Rows;
            long cols = grid.Cols;

            // first-order momentum
            double xISum = 0.0;
            double yISum = 0.0;
            double iSum = 0.0;

            void op(long iRow, long iCol)
            {
                double t = vals[iRow, iCol, checkBound: false];
                iSum += t;
                xISum += t * grid.GetCoordinateX(iCol);
                yISum += t * grid.GetCoordinateY(iRow);
            }
            Loop2D loop = new(operation: op,
                rowStart: 0, rowEnd: rows,
                colStart: 0, colEnd: cols);
            loop.Evaluate(mode: loopMode);

            return (xISum / iSum, yISum / iSum);
        }

        /// <summary>
        /// Finds the beam radii (widths) along the x and y directions based on the second-order moments of the intensity distribution.
        /// </summary>
        /// <param name="intensity">The 2D grid data representing intensity values.</param>
        /// <param name="refCenterX">Optional reference center x-coordinate. If null, the centroid is used.</param>
        /// <param name="refCenterY">Optional reference center y-coordinate. If null, the centroid is used.</param>
        /// <param name="printCenterInfo"></param>
        /// <param name="loopMode">The loop computation mode (default: Defaults.LoopOption).</param>
        /// <returns>
        /// A tuple containing the beam radii (widths) along the x and y directions.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if the intensity values are null.</exception>
        public static (double, double) FindRadius(
            Grid2DRealData intensity,
            double? refCenterX = null, double? refCenterY = null,
            bool printCenterInfo = false,
            LoopMode loopMode = Defaults.LoopOption)
        {
            if (intensity.Values == null) { throw new ArgumentNullException(nameof(intensity.Values)); }

            // centroid?
            if (refCenterX == null || refCenterY == null)
            {
                (double xc, double yc) = FindCentroid(intensity, loopMode);
                refCenterX ??= xc;
                refCenterY ??= yc;
                if (printCenterInfo)
                {
                    Printer.WriteLine($"Reference beam center calculated at {refCenterX.Value} along x-direction");
                    Printer.WriteLine($"Reference beam center calculated at {refCenterY.Value} along y-direction");
                }
            }

            // local variables
            MatrixD vals = intensity.Values;
            GridInfo2D grid = intensity.GridInfo;
            long rows = grid.Rows;
            long cols = grid.Cols;
            double x0 = refCenterX.Value;
            double y0 = refCenterY.Value;

            // second-order momentum
            double x2ISum = 0.0;
            double y2ISum = 0.0;
            double iSum = 0.0;

            void op(long iRow, long iCol)
            {
                double t = vals[iRow, iCol, checkBound: false];
                iSum += t;
                double xi = grid.GetCoordinateX(iCol) - refCenterX.Value;
                x2ISum += t * xi * xi;
                double yi = grid.GetCoordinateY(iRow) - refCenterY.Value;
                y2ISum += t * yi * yi;
            }
            Loop2D loop = new(operation: op,
                rowStart: 0, rowEnd: rows,
                colStart: 0, colEnd: cols);
            loop.Evaluate(mode: loopMode);

            // beam radius calculation
            double wx = 2.0 * Math.Sqrt(x2ISum / iSum);
            double wy = 2.0 * Math.Sqrt(y2ISum / iSum);
            return (wx, wy);
        }
    }
}
