using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace VEMS.MathCore
{
    /// <summary>
    /// DFT kernel for 2D piecewise-constant data on a periodic cell.
    /// Current version supports analytic Fourier coefficients for Rectangle regions.
    /// Other shape types are reserved for future extension.
    /// </summary>
    public static class Pwct2DPDFTKernel
    {
        #region ---- single coefficient ----

        /// <summary>
        /// Computes one 2D Fourier coefficient at desired indices (jy, jx).
        /// Convention:
        ///     c(jy,jx) = 1/(Ly*Lx) * ∬ f(y,x) exp(preFac * (jy*y/Ly + jx*x/Lx)) dA
        /// where preFac = -i*2π for forward transform, +i*2π for backward transform.
        /// </summary>
        public static Complex Transform2D(
            Complex preFac,
            Pwct2DCplxData x,
            double jy,
            double jx,
            double scalFac = 1.0)
        {
            double Ly = x.PeriodY;
            double Lx = x.PeriodX;
            double area = Ly * Lx;

            Complex res = Complex.Zero;

            // background contribution:
            // ∬ bg * exp(...) dA / area
            // only survives at (jy, jx) = (0, 0)
            if (IsZero(jy) && IsZero(jx))
            {
                res += x.BackgroundValue;
            }

            // region contributions:
            // add only the contrast against background
            foreach (var region in x.Regions)
            {
                Complex dv = region.Value - x.BackgroundValue;
                if (dv == Complex.Zero) { continue; }

                Complex integ = RegionIntegral(preFac, x, region, jy, jx);
                res += dv * integ / area;
            }

            return scalFac * res;
        }

        #endregion

        #region ---- coefficient block ----

        /// <summary>
        /// Computes a block of 2D Fourier coefficients.
        /// Row index corresponds to jy, column index corresponds to jx.
        /// </summary>
        public static MatrixZ Transform2D(
            Complex preFac,
            Pwct2DCplxData x,
            long startIndexY,
            long numCoeffY,
            long startIndexX,
            long numCoeffX,
            double scalFac = 1.0,
            LoopMode loopMode = Defaults.LoopOption)
        {
            MatrixZ res = new(rows: numCoeffY, cols: numCoeffX);

            Action<long, long> op = (iRow, iCol) =>
            {
                double jy = startIndexY + iRow;
                double jx = startIndexX + iCol;

                res[iRow, iCol, false] = Transform2D(
                    preFac: preFac,
                    x: x,
                    jy: jy,
                    jx: jx,
                    scalFac: scalFac
                );
            };

            Loop2D loop = new(
                operation: op,
                rowStart: 0,
                rowEnd: numCoeffY,
                colStart: 0,
                colEnd: numCoeffX,
                rowStep: 1,
                colStep: 1
            );

            loop.Evaluate(mode: loopMode);
            return res;
        }

        #endregion

        #region ---- region integral dispatcher ----

        /// <summary>
        /// Computes ∬_region exp(preFac*(jy*y/Ly + jx*x/Lx)) dA
        /// for one region.
        /// </summary>
        private static Complex RegionIntegral(
            Complex preFac,
            Pwct2DCplxData data,
            Pwct2DCplxData.Region2D region,
            double jy,
            double jx)
        {
            switch (region.Shape)
            {
                case Pwct2DCplxData.Shape2D.Rectangle:
                    return RectangleIntegral(preFac, data, region, jy, jx);

                case Pwct2DCplxData.Shape2D.Circle:
                    throw new NotSupportedException(
                        "Circle analytic coefficient is not implemented yet in this version.");

                case Pwct2DCplxData.Shape2D.Ellipse:
                    throw new NotSupportedException(
                        "Ellipse analytic coefficient is not implemented yet in this version.");

                case Pwct2DCplxData.Shape2D.Diamond:
                    throw new NotSupportedException(
                        "Diamond analytic coefficient is not implemented yet in this version.");

                default:
                    throw new ArgumentOutOfRangeException(nameof(region.Shape),
                        "Unsupported 2D shape type.");
            }
        }

        #endregion

        #region ---- rectangle ----

        /// <summary>
        /// Rectangle analytic integral.
        ///
        /// Region definition:
        /// centered at (CenterX, CenterY),
        /// half width = A, half height = B,
        /// no rotation in current version.
        ///
        /// Integral:
        ///     ∬_rect exp(preFac*(jy*y/Ly + jx*x/Lx)) dA
        ///   = Iy * Ix
        /// </summary>
        private static Complex RectangleIntegral(
            Complex preFac,
            Pwct2DCplxData data,
            Pwct2DCplxData.Region2D region,
            double jy,
            double jx)
        {
            if (!IsZero(region.Angle))
            {
                throw new NotSupportedException(
                    "Rotated Rectangle analytic coefficient is not implemented yet.");
            }

            double cx = region.CenterX;
            double cy = region.CenterY;
            double a = region.A; // half width
            double b = region.B; // half height

            double Lx = data.PeriodX;
            double Ly = data.PeriodY;

            Complex ax = preFac * jx / Lx;
            Complex ay = preFac * jy / Ly;

            Complex ix = CenteredIntervalIntegral(ax, cx, a);
            Complex iy = CenteredIntervalIntegral(ay, cy, b);

            return iy * ix;
        }

        /// <summary>
        /// Computes
        /// ∫_{c-h}^{c+h} exp(alpha * t) dt
        /// </summary>
        private static Complex CenteredIntervalIntegral(
            Complex alpha,
            double center,
            double halfWidth)
        {
            if (halfWidth <= 0.0) { return Complex.Zero; }

            // alpha == 0  => interval length
            if (IsZero(alpha.Real) && IsZero(alpha.Imaginary))
            {
                return 2.0 * halfWidth;
            }

            Complex p1 = Complex.Exp(alpha * (center + halfWidth));
            Complex p0 = Complex.Exp(alpha * (center - halfWidth));
            return (p1 - p0) / alpha;
        }

        #endregion

        #region ---- helpers ----

        private static bool IsZero(double x, double tol = 1e-12)
            => Math.Abs(x) < tol;

        #endregion
    }
}
