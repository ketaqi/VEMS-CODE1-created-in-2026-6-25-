using System;
using System.Collections.Generic;
using System.Linq;
using Complex = System.Numerics.Complex;
using System.Text;
using System.Threading.Tasks;

namespace VEMS.MathCore
{
    /// <summary>
    /// Chirp Fourier Transform (CFT) kernel.
    /// Provides methods for performing 1D CFT using FFT and quadratic phase modulation.
    /// Includes helper functions for applying quadratic phase terms.
    /// </summary>
    public static class CFTKernel
    {
        #region ---- 1D ----
        /// <summary>
        /// Performs in-place 1D Chirp Fourier Transform (CFT) using FFT and quadratic phase modulation
        /// with zero-centered convention.
        /// Scaling factors and the quadratic parameter q are updated accordingly.
        /// Supports parallel computation.
        /// </summary>
        /// <param name="v">Input/output data vector.</param>
        /// <param name="scalFac">Additional scaling factor (in/out).</param>
        /// <param name="q">Quadratic phase coefficient (in/out).</param>
        /// <param name="isForward">Indicates forward or backward transform.</param>
        /// <param name="useParallelFor">Enable parallel for-loop (for quadratic phase calculation or element-wise copy).</param>
        internal static void Transform1D(ref Vector<Complex> v, ref double scalFac, ref double q, bool isForward = true, bool useParallelFor = false)
        {
            LoopMode loopMode = useParallelFor ? LoopMode.Parallel : Defaults.LoopOption;
            // Step 1: FFT on input vector
            FFTKernel.Transform1D(ref v, scalFac, isForward: isForward, useParallelFor: useParallelFor);
            scalFac = 1.0 / (v.Count * scalFac);

            // Step 2: Apply quadratic term modulation
            if (q != 0.0)
            {
                AddQuadTerm(v: ref v, spacing: scalFac * Math.Sqrt(2.0 * Math.PI), q: -1.0 / (4.0 * q), scalFac: Complex.Sqrt(Complex.ImaginaryOne / (2.0 * q)), loopMode: loopMode);

                // Step 3: Inverse FFT on modulated vector
                FFTKernel.Transform1D(ref v, scalFac, isForward: false, useParallelFor: useParallelFor);
                scalFac = 1.0 / (v.Count * scalFac);

                // Step 4: Update scaling factor
                if (q < 0.0) v.Reverse();
                scalFac = Math.Abs(scalFac * 2.0 * q);

                // Step 5: Update q
                q = -1.0 / (4.0 * q);
            }
        }
        #endregion

        #region ---- 2D ----
        /// <summary>
        /// Performs a 2D transformation on a complex matrix, applying FFT and optional quadratic phase modulation with
        /// scaling and direction control.
        /// </summary>
        /// <param name="v">The complex matrix to be transformed.</param>
        /// <param name="scalFacX">The scaling factor for the X dimension, updated during the transformation.</param>
        /// <param name="scalFacY">The scaling factor for the Y dimension, updated during the transformation.</param>
        /// <param name="q">Quadratic phase parameter for the X dimension.</param>
        /// <param name="p">Quadratic phase parameter for the Y dimension.</param>
        /// <param name="isForward">Indicates whether to perform a forward or inverse transformation.</param>
        /// <param name="useParallelFor">Specifies whether to use parallel processing for the transformation.</param>
        internal static void Transform2D(ref Matrix<Complex> v, ref double scalFacX, ref double scalFacY, ref double q, ref double p, bool isForward = true, bool useParallelFor = false)
        { 
            LoopMode loopMode = useParallelFor ? LoopMode.Parallel : LoopMode.Sequential;
            // Step 1: FFT on input data
            FFTKernel.Transform2D(ref v, scalFacX * scalFacY, isForward: isForward, useParallelFor: useParallelFor);
            scalFacX = 1.0 / (v.Cols * scalFacX);
            scalFacY = 1.0 / (v.Rows * scalFacY);

            // Step 2: Apply quadratic phase term
            if (q != 0.0 && p != 0.0)
            {
                
                AddQuadTerm2D(ref v, spacingY: scalFacY * Math.Sqrt(2.0 * Math.PI), spacingX: scalFacX * Math.Sqrt(2.0 * Math.PI),
                    q: -1.0 / (4.0 * q),
                    p: -1.0 / (4.0 * p),
                    scalFac: Complex.Sqrt(-1.0 / (4 * q * p)) * ((q < 0 && p < 0)? -1 : 1),
                    loopMode: loopMode);

                // Step 3: Inverse FFT on modulated data
                FFTKernel.Transform2D(ref v, scalFacX * scalFacY, isForward: false, useParallelFor: useParallelFor);
                scalFacX = 1.0 / (v.Cols * scalFacX);
                scalFacY = 1.0 / (v.Rows * scalFacY);

                // Step 4: Update scaling factor
                if (q < 0.0) v.ReverseCols();
                if (p < 0.0) v.ReverseRows();
                scalFacX = Math.Abs(scalFacX * 2.0 * q);
                scalFacY = Math.Abs(scalFacY * 2.0 * p);

                // Step 5: Update q and p
                q = -1.0 / (4.0 * q);
                p = -1.0 / (4.0 * p);
            }

            else if (q != 0)
            {
                Matrix<Complex> x = v;
                AddQuadTerm2D(ref x, spacingY: scalFacY * Math.Sqrt(2.0 * Math.PI), spacingX: scalFacX * Math.Sqrt(2.0 * Math.PI),
                    q: -1.0 / (4.0 * q),
                    p: 0.0,
                    scalFac: Complex.Sqrt(Complex.ImaginaryOne / (2.0 * q)),
                    loopMode: loopMode);

                double scalFac = scalFacX;
                Action<long> op = (iRow) =>
                {
                    Vector<Complex> temp = x[iRow, new LongRange(0, x.Cols)];
                    FFTKernel.Transform1D(ref temp, scalFac, isForward: false, useParallelFor: useParallelFor);
                    x[iRow, new LongRange(0, x.Cols)] = temp;
                };
                Loop1D loop = new(operation: op,
                    start: 0, end: x.Rows,
                    step: 1);
                loop.Evaluate(loopMode);
                scalFacX = 1.0 / (x.Cols * scalFacX);

                if (q < 0.0) x.ReverseCols();
                scalFacX = Math.Abs(scalFacX * 2.0 * q);

                q = -1.0 / (4.0 * q);
            }

            else if (p != 0)
            {
                Matrix<Complex> x = v;
                AddQuadTerm2D(ref x, spacingY: scalFacY * Math.Sqrt(2.0 * Math.PI), spacingX: scalFacX * Math.Sqrt(2.0 * Math.PI),
                    q: 0.0,
                    p: -1.0 / (4.0 * p),
                    scalFac: Complex.Sqrt(Complex.ImaginaryOne / (2.0 * p)),
                    loopMode: loopMode);

                double scalFac = scalFacY;
                Action<long> op = (iCol) =>
                {
                    Vector<Complex> temp = x[new LongRange(0, x.Rows), iCol];
                    FFTKernel.Transform1D(ref temp, scalFac, isForward: false, useParallelFor: useParallelFor);
                    x[new LongRange(0, x.Rows), iCol] = temp;
                };
                Loop1D loop = new(operation: op,
                    start: 0, end: x.Cols,
                    step: 1);
                loop.Evaluate(loopMode);
                scalFacY = 1.0 / (x.Rows * scalFacY);

                if (p < 0.0) x.ReverseRows();
                scalFacY = Math.Abs(scalFacY * 2.0 * p);

                p = -1.0 / (4.0 * p);
            }
        }
        #endregion
        #region helper methods
        /// <summary>
        /// Applies a quadratic phase term to each element of the input vector.
        /// The modulation is performed in-place.
        /// </summary>
        /// <param name="v">Input/output complex vector.</param>
        /// <param name="spacing">Grid spacing for the quadratic phase calculation.</param>
        /// <param name="q">Quadratic phase coefficient.</param>
        /// <param name="scalFac">Optional scaling factor for modulation.</param>
        /// <param name="loopMode">Loop computation mode (sequential or parallel).</param>
        internal static void AddQuadTerm(ref Vector<Complex> v,
            double spacing,
            double q,
            Complex scalFac = default,
            LoopMode loopMode = Defaults.LoopOption)
        {
            // Initializes scaling factor
            Complex a = scalFac == default ? Complex.One : scalFac;

            // Define grid
            GridInfo1D grid = new(v.Count, spacing);

            // Define quadratic function
            Func<double, double> Quad = (x) => Function1D.Quadratic(x, [q]);

            // Define loop operation
            var t = v;
            void op(long i)
            {
                double x = grid[i];
                double psi = Quad(x);
                t[i, false] *= a * Complex.Exp(Complex.ImaginaryOne * psi);
            }
            Loop1D loop = new(operation: op,
                start: 0, end: t.Count,
                step: 1);
            loop.Evaluate(mode: loopMode);
        }

        /// <summary>
        /// Applies a quadratic phase term to each element of the input vector.
        /// The modulation is performed in-place.
        /// </summary>
        /// <param name="v">Input/output complex Matrix.</param>
        /// <param name="spacingY">Grid spacing along Y for the quadratic phase calculation.</param>
        /// <param name="spacingX">Grid spacing along X for the quadratic phase calculation.</param>
        /// <param name="q">Quadratic phase coefficient along X.</param>
        /// <param name="p">Quadratic phase coefficient along Y.</param>
        /// <param name="scalFac">Optional scaling factor for modulation.</param>
        /// <param name="loopMode">Loop computation mode (sequential or parallel).</param>
        internal static void AddQuadTerm2D(ref Matrix<Complex> v, double spacingY, double spacingX,
            double q, double p,
            Complex scalFac = default,
            LoopMode loopMode = Defaults.LoopOption)
        {
            // Initializes scaling factor
            Complex a = scalFac == default ? Complex.One : scalFac;

            // Define grid
            GridInfo2D grid = new(v.Rows, v.Cols, spacingY, spacingX);

            // Define quadratic function
            Func<double, double, double> Quad = (x, y) => Function2D.Quadratic(x, y, [q, p]);

            // Define loop operation
            Matrix<Complex> t = v;
            Action<long, long> op = (iRow, iCol) =>
            {
                (double y, double x) = grid[iRow, iCol];
                double psi = Quad(x, y);
                t[iRow, iCol] *= a * Complex.Exp(Complex.ImaginaryOne * psi);
            };
            Loop2D loop = new(operation: op,
                rowStart: 0, rowEnd: grid.Rows,
                colStart: 0, colEnd: grid.Cols,
                rowStep: 1, colStep: 1);
            loop.Evaluate(loopMode);
        }
        #endregion
    }
}
