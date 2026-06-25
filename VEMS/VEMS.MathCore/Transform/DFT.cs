using System.Numerics;

namespace VEMS.MathCore
{

    /// <summary>
    /// DFT kernels for uniformly sampled data
    /// definition follows from that of FFT with 2Pi factor
    /// </summary>
    internal class DFTKernel
    {
        #region ---- 1D ----

        /// <summary>
        /// transform kernel for 1D case
        /// </summary>
        /// <param name="preFac"> pre-factor that determines forward / backward transform </param>
        /// <param name="x"> input complex vector x </param>
        /// <param name="j"> desired output index j; can be non-integer number </param>
        /// <param name="scalFac"> scaling factor </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> transformed result at index j </returns>
        internal static Complex Transform1D(Complex preFac,
            VectorZ x, double j,
            double scalFac = 1.0,
            LoopMode loopMode = Defaults.LoopOption)
        {
            // initialization
            Complex res = Complex.Zero;
            double n0 = -0.5 * x.Count + 0.5;

            // defines loop operation
            Action<long> a = (i) =>
            {
                res += x[i, false] * Complex.Exp(preFac * (n0 + i) * j / x.Count);
            };
            Loop1D loop = new(operation: a, start: 0, end: x.Count, step: 1);
            loop.Evaluate(mode: loopMode);

            // return
            return (scalFac * res);

            //// loop n
            //for (long n = 0; n < x.Count; n++)
            //{
            //    double nn = n0 + n;
            //    res += scalFac * x[n, false]
            //        * Complex.Exp(preFac * nn * j / x.Count);    
            //}
        }

        /// <summary>
        /// transform kernel for 1D case
        /// </summary>
        /// <param name="preFac"> pre-factor that defines forward / backward transform </param>
        /// <param name="x"> input complex vector x </param>
        /// <param name="js"> list of desired output indices j; can be non-integer number </param>
        /// <param name="scalFac"> scaling factor </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> transformed data </returns>
        internal static VectorZ Transform1D(Complex preFac,
            VectorZ x, VectorD js,
            double scalFac = 1.0,
            LoopMode loopMode = Defaults.LoopOption)
        {
            // initializes output
            VectorZ res = new(count: js.Count);

            // defines loop operation (j)
            Action<long> a = (j) =>
            {
                res[j, false] = Transform1D(preFac, x, js[j, false], scalFac,
                    loopMode: LoopMode.Sequential); // sets inner loop to sequential
            };
            Loop1D loop = new(operation: a,
                start: 0, end: js.Count, step: 1);
            loop.Evaluate(mode: loopMode);

            // return
            return res;
        }

        /// <summary>
        /// transform kernel for 1D case
        /// </summary>
        /// <param name="preFac"> pre-factor that helps determine forward / backward transform </param>
        /// <param name="x"> data vector x </param>
        /// <param name="scalFac"> scaling factor </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> transformed data </returns>
        internal static VectorZ Transform1D(Complex preFac,
            VectorZ x, double scalFac = 1.0,
            LoopMode loopMode = Defaults.LoopOption)
        {
            VectorD js = new(count: x.Count,
                initVal: -0.5 * x.Count + 0.5, increment: 1);
            return Transform1D(preFac, x, js, scalFac, loopMode);
        }

        #endregion
        #region ---- 2D ----

        /// <summary>
        /// transform kernel for 2D case
        /// </summary>
        /// <param name="preFac"> pre-factor that helps determine forward / backward transform </param>
        /// <param name="x"> data matrix x </param>
        /// <param name="jy"> desired output index jy; can be non-integer number </param>
        /// <param name="jx"> desired output index jx; can be non-integer number </param>
        /// <param name="scalFac"> scaling factor </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> transformed data at (jy, jx) </returns>
        internal static Complex Transform2D(Complex preFac,
            MatrixZ x, double jy, double jx,
            double scalFac = 1.0,
            LoopMode loopMode = Defaults.LoopOption)
        {
            Complex res = Complex.Zero;
            double ny0 = -0.5 * x.Rows + 0.5;
            double nx0 = -0.5 * x.Cols + 0.5;

            // defines loop operation
            Action<long, long> a = (iRow, iCol) =>
            {
                double nny = ny0 + iRow;
                double nnx = nx0 + iCol;
                res += x[iRow, iCol, false]
                    * Complex.Exp(preFac * (nny * jy / x.Rows + nnx * jx / x.Cols));
            };
            Loop2D loop = new(operation: a,
                rowStart: 0, rowEnd: x.Rows,
                colStart: 0, colEnd: x.Cols,
                rowStep: 1, colStep: 1);
            loop.Evaluate(mode: loopMode);

            // return
            return (scalFac * res);

            //// loop
            //for (long i = 0; i < x.Rows * x.Cols; i++)
            //{
            //    long ny = i / x.Cols;
            //    long nx = i % x.Cols;
            //    double nny = ny0 + ny;
            //    double nnx = nx0 + nx;
            //    res += scalFac * x[ny, nx, false]
            //        * Complex.Exp(preFac * (nny * jy / x.Rows + nnx * jx / x.Cols));
            //}
            //return res;
        }

        /// <summary>
        /// transform kernel for 1D case
        /// </summary>
        /// <param name="preFac"> pre-factor that helps determine forward / backward transform </param>
        /// <param name="x"> data matrix x </param>
        /// <param name="jys"> list of desired output indices jy; can be non-integer </param>
        /// <param name="jxs"> list of desired output indiced jx; can be non-integer </param>
        /// <param name="scalFac"> scaling factor </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> transformed data </returns>
        internal static MatrixZ Transform2D(Complex preFac,
            MatrixZ x, VectorD jys, VectorD jxs,
            double scalFac = 1.0,
            LoopMode loopMode = Defaults.LoopOption)
        {
            MatrixZ res = new(rows: jys.Count, cols: jxs.Count);

            // defines loop operation
            Action<long, long> a = (jRow, jCol) =>
            {
                res[jRow, jCol, false] = Transform2D(preFac, x, 
                    jys[jRow, false], jxs[jCol, false], scalFac,
                    loopMode: LoopMode.Sequential); // sets inner loop to sequential
            };
            Loop2D loop = new(operation: a, 
                rowStart: 0, rowEnd: jys.Count, 
                colStart: 0, colEnd: jxs.Count,
                rowStep: 1, colStep: 1);
            loop.Evaluate(mode: loopMode);

            // return
            return res;
        }

        /// <summary>
        /// transform kernel for 2D
        /// </summary>
        /// <param name="preFac"> pre-factor that helps determine forward / backward transform </param>
        /// <param name="x"> data matrix x </param>
        /// <param name="scalFac"> scaling factor </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> transformed data </returns>
        internal static MatrixZ Transform2D(Complex preFac,
            MatrixZ x, double scalFac = 1.0,
            LoopMode loopMode = Defaults.LoopOption)
        {
            VectorD jys = new(count: x.Rows,
                initVal: -0.5 * x.Rows + 0.5, increment: 1);
            VectorD jxs = new(count: x.Cols,
                initVal: -0.5 * x.Cols + 0.5, increment: 1);
            return Transform2D(preFac, x, jys, jxs, scalFac, loopMode);
        }

        #endregion
    }

    /// <summary>
    /// DFT kernel for piecewise-constant data
    /// </summary>
    [Obsolete]
    internal class PDFTKernel
    {

        /// <summary>
        /// transform kernel for piecewise-constant data
        /// </summary>
        /// <param name="preFac"> pre-factor that helps determine forward / backward transform </param>
        /// <param name="x"> input piecewise-constant data x </param>
        /// <param name="j"> desired output index j </param>
        /// <param name="scalFac"> scaling factor </param>
        /// <returns> transformed result at index j </returns>
        internal static Complex? Transform1D(Complex preFac,
            Pwct1DCplxData x, long j,
            double scalFac = 1.0)
        {
            // period
            double period = x.Range;

            Complex res = 0.0;
            VectorZ gridsExp = new(x.Spans.Count);
            if (j == 0)
            {
                // loop n
                for (long n = 0; n < x.Pieces; n++)
                {
                    double diff = x.Spans[n + 1] - x.Spans[n];
                    res += diff * x.Values[n];
                }
                res /= period;
            }
            else
            {
                // calculate exp terms for each grid points
                //VMath.ScaleOn(ref gridsExp, preFac * j / period);
                //gridsExp = VMath.Exp(gridsExp);
                for (long i = 0; i < gridsExp.Count; i++)
                    gridsExp[i] = Complex.Exp(preFac * j / period * x.Spans[i]);

                // loop n
                for (long n = 0; n < x.Pieces; n++)
                {
                    Complex diff = gridsExp[n + 1] - gridsExp[n];
                    res += diff * x.Values[n];
                }
                res /= (preFac * j);
            }

            // scaling factor handling
            if (scalFac != 1)
                res *= scalFac;

            return res;
        }

        /// <summary>
        /// transform kernel for piecewise-constant data
        /// </summary>
        /// <param name="preFac"> pre-factor that helps determine forward / backward transform </param>
        /// <param name="x"> input piecewise-constant data x </param>
        /// <param name="startIndex"> desired starting index for output </param>
        /// <param name="numCoeff"> desired number of Fourier coefficients </param>
        /// <param name="scalFac"> scaling factor </param>
        /// <returns> transformed data </returns>
        internal static VectorZ? Transform1D(Complex preFac,
            Pwct1DCplxData x, long startIndex, long numCoeff,
            double scalFac = 1.0)
        {
            // period 
            double period = x.Range;
            // initialize result coefficients vector
            VectorZ res = new(numCoeff, 0.0);

            VectorZ gridsExp = new(x.Spans.Count, 0.0);
            for (long j = 0; j < numCoeff; j++)
            {
                long jj = j + startIndex;

                if (jj == 0.0)
                {
                    // loop n
                    for (long n = 0; n < x.Pieces; n++)
                    {
                        double diff = x.Spans[n + 1] - x.Spans[n];
                        res[j] += diff * x.Values[n];
                    }
                    res[j] = res[j] / period;
                }
                else
                {
                    // calculate exp terms for each grid points
                    //VMath.ScaleOn(ref gridsExp, preFac * jj / period);
                    //gridsExp = VMath.Exp(gridsExp);
                    for (long i = 0; i < gridsExp.Count; i++)
                        gridsExp[i] = Complex.Exp(preFac * jj / period * x.Spans[i]);

                    // loop n
                    for (long n = 0; n < x.Pieces; n++)
                    {
                        Complex diff = gridsExp[n + 1] - gridsExp[n];
                        res[j] += diff * x.Values[n];
                    }
                    res[j] = res[j] / (preFac * jj);
                }
            }

            // scale factor
            if (scalFac != 1.0)
                VMath.ScaleOn(ref res, scalFac);

            return res;
        }


    }

    /// <summary>
    /// DFT kernel for piecewise-constant data 
    /// </summary>
    internal class PwctDFTKernel
    {

        /// <summary>
        /// transform kernel for 1D piecewise-constant data
        /// </summary>
        /// <param name="preFac"> pre-factor that helps determine forward / backward transform </param>
        /// <param name="x"> input piecewise-constant data x </param>
        /// <param name="j"> desired output index j; can be non-integer number </param>
        /// <param name="scalFac"> scaling factor </param>
        /// <returns> transformed result at index j </returns>
        internal static Complex Transform1D(Complex preFac,
            Pwct1DCplxData x, double j,
            double scalFac = 1.0)
        {
            // initialization
            Complex res = 0.0;
            Complex exp = Complex.Exp(preFac * j * x.Spans[0]);

            // computation kernels
            if(j == 0.0)
            {
                for(long n = 0; n < x.Pieces; n++)
                {
                    double diff = x.Spans[n + 1] - x.Spans[n];
                    res += diff * x.Values[n];
                }
            }
            else
            {
                for (long n = 0; n < x.Pieces; n++)
                {
                    Complex expn = Complex.Exp(preFac * j * x.Spans[n+1]);
                    Complex diff = expn - exp;
                    res += diff * x.Values[n];
                    exp = expn;
                }
                res /= (preFac * j);
            }

            // return
            return res;
        }

        /// <summary>
        /// transform kernel for 1D piecewise-constant data
        /// </summary>
        /// <param name="preFac"> pre-factor that helps determine forward / backward transform </param>
        /// <param name="x"> input piecewise-constant data x </param>
        /// <param name="js"> list of desired output indices j; can be non-integer number </param>
        /// <param name="scalFac"> scaling factor </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> transformed data </returns>
        internal static VectorZ Transform1D(Complex preFac,
            Pwct1DCplxData x, VectorD js,
            double scalFac = 1.0,
            LoopMode loopMode = Defaults.LoopOption)
        {
            // initializes output
            VectorZ res = new(count: js.Count);

            // defines loop operation (j)
            Action<long> a = (j) =>
            {
                res[j, false] = Transform1D(preFac, x, js[j, false], scalFac);
            };
            Loop1D loop = new(operation: a,
                start: 0, end: js.Count, step: 1);
            loop.Evaluate(mode: loopMode);

            // return
            return res;
        }

        /// <summary>
        /// transform kernel for 1D piecewise-constant data
        /// </summary>
        /// <param name="preFac"> pre-factor that helps determine forward / backward transform </param>
        /// <param name="x"> input piecewise-constant data x </param>
        /// <param name="startIndex"> desired starting index for output </param>
        /// <param name="numCoeff"> desired number of Fourier coefficients </param>
        /// <param name="scalFac"> scaling factor </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> transformed data </returns>
        internal static VectorZ Transform1D(Complex preFac,
            Pwct1DCplxData x, long startIndex, long numCoeff,
            double scalFac = 1.0,
            LoopMode loopMode = Defaults.LoopOption)
        {
            VectorD js = new(count: numCoeff,
                initVal: startIndex, increment: 1);
            return Transform1D(preFac, x, js, scalFac, loopMode);
        }

    }

}
