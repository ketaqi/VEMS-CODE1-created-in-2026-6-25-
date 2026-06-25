using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using VEMS.MathCore;

namespace VEMS.EMSolver
{
    /// <summary>
    /// helper methods for idealized gratings
    /// </summary>
    public class IdealGratingHelper
    {
        #region 1D diffraction order management
        /// <summary>
        /// Combines all diffraction orders for 1D fields into a single field representation.
        /// Interpolates each order onto a common grid and applies appropriate phase shifts.
        /// </summary>
        /// <param name="v">List of 1D field representations for each diffraction order</param>
        /// <param name="loopMode">Loop mode for interpolation and evaluation (default from Defaults.LoopOption)</param>
        /// <returns>Single SCField1D containing the coherent sum of all diffraction orders</returns>
        public SCField1D OrderAdds1D(List<SCField1D> v,
            LoopMode loopMode = Defaults.LoopOption)
        {
            return TakeOrderSum1D(v, loopMode);
        }
        /// <summary>
        /// Core implementation for summing 1D diffraction orders.
        /// Performs grid alignment, interpolation, phase adjustment, and summation.
        /// </summary>
        /// <param name="v">List of SCField1D orders to combine</param>
        /// <param name="loopMode">Loop mode for interpolation/evaluation operations</param>
        /// <returns>Combined field containing the superposition of all input orders</returns>
        /// <exception cref="ArgumentNullException">Thrown when input list is null</exception>
        private static SCField1D TakeOrderSum1D(List<SCField1D> v,
            LoopMode loopMode)
        {
            if (v == null) { throw new ArgumentNullException(nameof(v)); }
            if (v.Count == 0) { return new SCField1D(); }

            // Determine the start and end coordinates across all grids
            double minVal = v[0].UGrid.Start;
            double maxVal = v[0].UGrid.Start;
            double maxEnd = v[0].UGrid.End;
            for (int i = 1; i < v.Count; i++)
            {
                double start = v[i].UGrid.Start;
                if (start < minVal)
                {
                    minVal = start;
                }
                else if (start > maxVal)
                {
                    maxVal = start;
                    maxEnd = v[i].UGrid.End;
                }
            }

            double srcStart = minVal;
            double srcEnd = maxEnd;

            // Source index range (clamped to source grid)
            const double eps = 1e-12;
            double gridSpacing = v[0].UGrid.Spacing;
            long srcCount = (long)(Math.Floor((srcEnd - srcStart + eps) / gridSpacing)) + 1;

            // Extract source submatrix (only overlapping region)
            VectorZ srcSub = new(count: srcCount);
            GridInfo1D srcSubGrid = new(
                n: srcCount,
                spacing: gridSpacing,
                refPoint: srcStart
                );

            // Accumulate all fields onto the source grid
            for (int i = 0; i < v.Count; i++)
            {
                SCField1D currentOrder = v[i];

                // Interpolate current order onto the common grid
                Grid1DCplxInterpolation interpolator = new Grid1DCplxInterpolation(
                    v: currentOrder.UValues,
                    grid: currentOrder.UGrid,
                    method: InterpolationMethod.Linear
                    );
                VectorZ dSub = interpolator.Evaluate(
                    targetGrid: srcSubGrid,
                    loopMode: loopMode
                    );
                // Apply phase shift corresponding to the diffraction order's k-vector
                Samp1DCplxFunc phaseAdjustment = new(f: (x) => Complex.Exp(Complex.ImaginaryOne * (v[i].ShiftKx * x)));
                phaseAdjustment.ScaleOn(ref dSub, srcSubGrid, loopMode);
                // Accumlate the phase-adjusted field
                srcSub += dSub;
            }

            // Build output SCField1D using the first input as template
            SCField1D vout = new SCField1D(v[0]);
            vout.UValues = srcSub;
            vout.UGrid = srcSubGrid;
            return vout;
        }
        #endregion
        #region 2D diffraction order management
        /// <summary>
        /// Combines all diffraction orders for 2D fields into a single field representation.
        /// Interpolates each order onto a common 2D grid and applies appropriate 2D phase shifts.
        /// </summary>
        /// <param name="v">List of 2D field representations for each diffraction order</param>
        /// <param name="loopMode">Loop mode for interpolation and evaluation (default from Defaults.LoopOption)</param>
        /// <returns>Single SCField containing the coherent sum of all 2D diffraction orders</returns>
        public static SCField OrderAdds(List<SCField> v,
            LoopMode loopMode = Defaults.LoopOption)
        {
            return TakeOrderSum(v, loopMode);
        }

        /// <summary>
        /// Core implementation for summing 2D diffraction orders.
        /// Performs 2D grid alignment, bilinear interpolation, 2D phase adjustment, and summation.
        /// </summary>
        /// <param name="v">List of SCField orders to combine</param>
        /// <param name="loopMode">Loop mode for interpolation/evaluation operations</param>
        /// <returns>Combined 2D field containing the superposition of all input orders</returns>
        /// <exception cref="ArgumentNullException">Thrown when input list is null</exception>
        private static SCField TakeOrderSum(List<SCField> v,
            LoopMode loopMode)
        {

            if (v == null) { throw new ArgumentNullException(nameof(v)); }
            if (v.Count == 0) { return new SCField(); }

            // Determine the start and end coordinates across all 2D grids
            double minValx = v[0].UGrid.StartX;
            double maxValx = v[0].UGrid.StartX;
            double minValy = v[0].UGrid.StartY;
            double maxValy = v[0].UGrid.StartY;
            double maxEndx = v[0].UGrid.EndX;
            double maxEndy = v[0].UGrid.EndY;
            for (int i = 1; i < v.Count; i++)
            {
                double currentStartX = v[i].UGrid.StartX;
                double currentStartY = v[i].UGrid.StartY;
                if (currentStartX < minValx)
                {
                    minValx = currentStartX;
                }
                else if (currentStartX > maxValx)
                {
                    maxValx = currentStartX;
                    maxEndx = v[i].UGrid.EndX;
                }

                if (currentStartY < minValy)
                {
                    minValy = currentStartY;
                }
                else if (currentStartY > maxValy)
                {
                    maxValy = currentStartY;
                    maxEndy = v[i].UGrid.EndY;
                }
            }

            double srcStartX = minValx;
            double srcStartY = minValy;
            double srcEndX = maxEndx;
            double srcEndY = maxEndy;

            // Source index range (clamped to source grid)
            const double eps = 1e-12;
            double gridSpacingX = v[0].UGrid.SpacingX;
            double gridSpacingY = v[0].UGrid.SpacingY;
            long srcCol1 = (long)(Math.Floor((srcEndX - srcStartX + eps) / gridSpacingX)) + 1;
            long srcRow1 = (long)(Math.Floor((srcEndY - srcStartY + eps) / gridSpacingY)) + 1;

            // Extract source submatrix (only overlapping region)
            MatrixZ srcSub = new(rows: srcRow1, cols: srcCol1);
            GridInfo2D srcSubGrid = new(
                rows: srcRow1,
                cols: srcCol1,
                spacingY: v[0].UGrid.SpacingY,
                spacingX: v[0].UGrid.SpacingX,
                refPointY: srcStartY,
                refPointX: srcStartX
                );

            // Accumulate all fields onto the source 2D grid
            for (int i = 0; i < v.Count; i++)
            {
                SCField currentOrder = v[i];

                // Interpolate current order onto the common 2D grid
                Grid2DCplxInterpolation interpolator = new Grid2DCplxInterpolation(
                    v: currentOrder.UValues,
                    grid: currentOrder.UGrid,
                    method: InterpolationMethod.Linear
                );

                MatrixZ interpolatedField = interpolator.Evaluate(
                    targetGrid: srcSubGrid,
                    loopMode: loopMode
                );

                // Apply 2D phase shift corresponding to the diffraction order's k-vector
                Samp2DCplxFunc phaseAdjustment = new Samp2DCplxFunc(
                    f: (x, y) => Complex.Exp(Complex.ImaginaryOne *
                    (currentOrder.ShiftKx * x + currentOrder.ShiftKy * y))
                    );

                phaseAdjustment.ScaleOn(
                    ref interpolatedField,
                    grid: srcSubGrid,
                    loopMode: loopMode
                    );

                // Accumulate the interpolated field
                srcSub += interpolatedField;
            }

            // Build output SCField using the first input as template
            SCField vout = new SCField(v[0]);
            vout.UValues = srcSub;
            vout.UGrid = srcSubGrid;
            return vout;
        }
        #endregion
    }
}
