using System.Runtime.CompilerServices;
using VEMS.MathCore;

namespace VEMS.EMSolver
{

    /// <summary>
    /// Provides static methods for post-analysis of 1D detector results, such as calculating the beam centroid and radius
    /// from a given intensity distribution.
    /// </summary>
    public class PostAnalysis1D
    {
        #region ---- centroid ----

        /// <summary>
        /// Finds the centroid of a beam using the first-order momentum of the provided intensity distribution.
        /// </summary>
        /// <param name="intensity">The (paraxial) intensity distribution as a <see cref="Grid1DRealData"/> object.</param>
        /// <param name="loopMode">The loop-computational mode option. Default is <see cref="Defaults.LoopOption"/>.</param>
        /// <returns>The calculated centroid of the beam.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="intensity"/> or its <c>Values</c> property is null.</exception>
        public static double FindCentroid(Grid1DRealData intensity,
            LoopMode loopMode = Defaults.LoopOption)
        {
            if (intensity.Values == null) { throw new ArgumentNullException(nameof(intensity.Values)); }

            // local variables
            VectorD vals = intensity.Values;
            GridInfo1D grid = intensity.GridInfo;
            long n = grid.Count;

            // first-order momentum
            double xISum = 0.0;
            double iSum = 0.0;
            if (loopMode == LoopMode.Sequential)
            {
                for (long i = 0; i < n; i++)
                {
                    double t = vals[i, checkBound: false];
                    iSum += t;
                    xISum += t * grid[i];
                }
            }
            else
            {
                void op(long i)
                {
                    double t = vals[i, checkBound: false];
                    Interlocked.Add(ref Unsafe.As<double, long>(ref iSum), BitConverter.DoubleToInt64Bits(t));
                    Interlocked.Add(ref Unsafe.As<double, long>(ref xISum), BitConverter.DoubleToInt64Bits(t * grid[i]));
                }
                Loop1D loop = new(operation: op, start: 0, end: n);
                loop.Evaluate(mode: loopMode);
            }

            return xISum / iSum;
        }

        #endregion
        #region ---- radius ----

        /// <summary>
        /// Finds the beam radius using the second-order momentum of the provided intensity distribution.
        /// </summary>
        /// <param name="intensity">The (paraxial) intensity distribution as a <see cref="Grid1DRealData"/> object.</param>
        /// <param name="refCenter">
        /// The reference beam center. If <c>null</c>, the centroid will be calculated automatically using <see cref="FindCentroid"/>.
        /// </param>
        /// <param name="printCenterInfo"></param>
        /// <param name="loopMode">The loop-computational mode option. Default is <see cref="Defaults.LoopOption"/>.</param>
        /// <returns>The calculated radius of the beam.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="intensity"/> or its <c>Values</c> property is null.</exception>
        public static double FindRadius(Grid1DRealData intensity,
            double? refCenter = null, bool printCenterInfo = false,
            LoopMode loopMode = Defaults.LoopOption)
        {
            if (intensity.Values == null) { throw new ArgumentNullException(nameof(intensity.Values)); }

            // centroid?
            refCenter ??= FindCentroid(intensity, loopMode);
            if (printCenterInfo)
            { Printer.WriteLine($"Reference beam center calculated at {refCenter.Value}"); }

            // local variables
            VectorD vals = intensity.Values;
            GridInfo1D grid = intensity.GridInfo;
            long n = grid.Count;
            double x0 = refCenter.Value;

            // second-order momentum
            double x2ISum = 0.0;
            double iSum = 0.0;

            if (loopMode == LoopMode.Sequential)
            {
                for (long i = 0; i < n; i++)
                {
                    double t = vals[i, checkBound: false];
                    iSum += t;
                    double xi = grid[i] - x0;
                    x2ISum += t * xi * xi;
                }
            }
            else
            {
                void op(long i)
                {
                    double t = vals[i, checkBound: false];
                    double xi = intensity.GridInfo[i] - x0;
                    Interlocked.Add(ref Unsafe.As<double, long>(ref iSum), BitConverter.DoubleToInt64Bits(t));
                    Interlocked.Add(ref Unsafe.As<double, long>(ref x2ISum), BitConverter.DoubleToInt64Bits(t * xi * xi));
                }
                Loop1D loop = new(operation: op, start: 0, end: n);
                loop.Evaluate(mode: loopMode);

                //// Use thread-local accumulators to avoid contention
                //int maxThreads = Environment.ProcessorCount;
                //double[] x2ISumLocals = new double[maxThreads];
                //double[] iSumLocals = new double[maxThreads];

                //Parallel.For(0, (int)n, () => (x2: 0.0, i: 0.0), (i, state, local) =>
                //{
                //    double t = vals[i, checkBound: false];
                //    local.i += t;
                //    double xi = grid[i] - center;
                //    local.x2 += t * xi * xi;
                //    return local;
                //},
                //local =>
                //{
                //    int idx = Thread.GetCurrentProcessorId() % maxThreads;
                //    Interlocked.Add(ref Unsafe.As<double, long>(ref iSumLocals[idx]), BitConverter.DoubleToInt64Bits(local.i));
                //    Interlocked.Add(ref Unsafe.As<double, long>(ref x2ISumLocals[idx]), BitConverter.DoubleToInt64Bits(local.x2));
                //});

                //for (int i = 0; i < maxThreads; i++)
                //{
                //    iSum += iSumLocals[i];
                //    x2ISum += x2ISumLocals[i];
                //}

            }

            // beam radius calculation
            return 2.0 * Math.Sqrt(x2ISum / iSum);
        }

        #endregion

    }

}
