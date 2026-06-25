using Complex = System.Numerics.Complex;

namespace VEMS.MathCore
{

    /// <summary>
    /// sampler for 2D real-valued function
    /// </summary>
    public class Samp2DRealFunc
    {
        #region properties

        /// <summary>
        /// internally stored two-variable function F(x,y)
        /// for fixed parameters {p}
        /// <para> parameter #1: x position </para>
        /// <para> parameter #2: y position </para>
        /// <para> result: function value </para>
        /// </summary>
        private Func<double, double, double> F { get; set; }

        #endregion
        #region constructor

        /// <summary>
        /// default constructor
        /// </summary>
        internal Samp2DRealFunc() 
        {
            F = (x, y) => 0.0;
        }

        /// <summary>
        /// constructs a sampler for
        /// two-dimensional real-valued function
        /// </summary>
        /// <param name="f"> real-valued function F(x,y) </param>
        public Samp2DRealFunc(Func<double, double, double> f)
        {
            F = f;
        }

        /// <summary>
        /// constructs a sampler for
        /// two-dimensional real-valued function
        /// </summary>
        /// <param name="f"> real-valued function F(x,y; {p}) with parameters {p} </param>
        /// <param name="p"> parameters {p} used to define the function </param>
        public Samp2DRealFunc(Func<double, double, List<double>?, double> f,
            List<double>? p = null)
        {
            // defines two-variable function with fixed parameters {p}
            F = (x, y) => f(x, y, p);
        }

        ///// <summary>
        ///// constructs a uniform-sampler for
        ///// two-dimensional real-valued function
        ///// defined in polar coordinate system
        ///// </summary>
        ///// <param name="frt"></param>
        ///// <param name="pr"></param>
        ///// <param name="pt"></param>
        //public Grid2DRealFunc(FunctionRT frt, 
        //    List<double> pr, List<double> pt) 
        //{
        //    // defines two-variable function with fixed parameters {pr} and {pt}
        //    Fxy = (x, y) => frt.EvaluateXY(x, y, pr, pt);
        //}

        #endregion
        #region methods

        #region ----- sampling -----

        /// <summary>
        /// samples the function on a set of (x,y) locations
        /// either uniform or scattered
        /// </summary>
        /// <param name="rho"> scattered sample locations </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> sampled function values on these locations </returns>
        public VectorD Sample(ScatInfo2D rho, 
            LoopMode loopMode = Defaults.LoopOption)
        {
            // initialization
            VectorD fs = new(count: rho.Count);
            // defines loop operation
            Action<long> a = (i) =>
            {
                (double y, double x) = rho[i, false];
                fs[i, false] = F(x, y);
            };
            Loop1D loop = new(operation: a, 
                start: 0, end: rho.Count, step: 1);
            loop.Evaluate(mode: loopMode);
            // return
            return fs;
        }

        /// <summary>
        /// samples the functin on a set of (x,y) locations
        /// </summary>
        /// <param name="xy"> x/y-separable scattered sample locations </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> sampled function values on these locations </returns>
        public MatrixD Sample(ScatInfoXY xy,
            LoopMode loopMode = Defaults.LoopOption)
        {
            // initialization
            MatrixD f = new(rows: xy.Rows, cols: xy.Cols);
            // defines loop operation
            Action<long, long> a = (iRow, iCol) =>
            {
                (double y, double x) = xy[iRow, iCol, false];
                f[iRow, iCol, false] = F(x, y);
            };
            Loop2D loop = new(operation: a, 
                rowStart: 0, rowEnd: xy.Rows, 
                colStart: 0, colEnd: xy.Cols,
                rowStep: 1, colStep: 1);
            loop.Evaluate(mode: loopMode);
            // return
            return f;
        }

        /// <summary>
        /// samples the function on a given uniform grid
        /// </summary>
        /// <param name="grid"> target grid </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> sampled function values on the grid </returns>
        public MatrixD Sample(GridInfo2D grid,
            LoopMode loopMode = Defaults.LoopOption)
            => Sample(xy: (ScatInfoXY)grid, loopMode: loopMode);

        #endregion
        #region ----- modification -----

        /// <summary>
        /// adds the function to a uniformly sampled target data
        /// </summary>
        /// <param name="x"> uniformly sampled target data (in/out) </param>
        /// <param name="grid"> sampling grid of the data </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        public void AddTo(ref MatrixD x, GridInfo2D grid,
            LoopMode loopMode = Defaults.LoopOption)
        {
            // consistency check
            if (x.Rows != grid.Rows || x.Cols != grid.Cols)
            { throw new ArgumentException($"Inconsistent number of input elements"); }

            MatrixD t = x;
            // defines loop operation
            Action<long, long> a = (iRow, iCol) =>
            {
                (double yi, double xi) = grid[iRow, iCol];
                t[iRow, iCol, false] += F(xi, yi);
            };
            Loop2D loop = new(operation: a,
                rowStart: 0, rowEnd: grid.Rows,
                colStart: 0, colEnd: grid.Cols,
                rowStep: 1, colStep: 1);
            loop.Evaluate(mode: loopMode);
        }

        /// <summary>
        /// adds the function to a uniformly sampled target data
        /// </summary>
        /// <param name="x"> uniformly sampled target data (in/out) </param>
        /// <param name="grid"> sampling grid of the data </param>
        /// <param name="part"> target complex part in the data to modify </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        public void AddTo(ref MatrixZ x, GridInfo2D grid,
            ComplexPart part = ComplexPart.RealPart,
            LoopMode loopMode = Defaults.LoopOption)
        {
            // consistency check
            if (x.Rows != grid.Rows || x.Cols != grid.Cols)
            { throw new ArgumentException($"Inconsistent number of input elements"); }

            MatrixZ t = x;
            // defines loop operation
            Action<long, long> a = (iRow, iCol) =>
            {
                (double yi, double xi) = grid[iRow, iCol];
                switch (part)
                {
                    case ComplexPart.RealPart:
                        t[iRow, iCol, checkBound: false] += F(xi, yi);
                        break;
                    case ComplexPart.ImagPart:
                        t[iRow, iCol, checkBound: false] += F(xi, yi) * Complex.ImaginaryOne;
                        break;
                    case ComplexPart.Magnitude:
                        {
                            Complex ti = t[iRow, iCol, checkBound: false];
                            t[iRow, iCol, checkBound: false] += F(xi, yi) * Complex.Exp(Complex.ImaginaryOne * ti.Phase);
                            break;
                        }
                    case ComplexPart.Argument:
                        t[iRow, iCol, checkBound: false] *= Complex.Exp(Complex.ImaginaryOne * F(xi, yi));
                        break;
                }
            };
            Loop2D loop = new(operation: a,
                rowStart: 0, rowEnd: grid.Rows,
                colStart: 0, colEnd: grid.Cols,
                rowStep: 1, colStep: 1);
            loop.Evaluate(mode: loopMode);
        }

        /// <summary>
        /// multiplies the function on a uniformly sampled target data
        /// </summary>
        /// <param name="x"> uniformly sampled target data (in/out) </param>
        /// <param name="grid"> sampling grid of the data </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        public void ScaleOn(ref MatrixD x, GridInfo2D grid,
            LoopMode loopMode = Defaults.LoopOption)
        {
            // consistency check
            if (x.Rows != grid.Rows || x.Cols != grid.Cols)
            { throw new ArgumentException($"Inconsistent number of input elements"); }

            MatrixD t = x;
            // defines loop operation
            Action<long, long> a = (iRow, iCol) =>
            {
                (double yi, double xi) = grid[iRow, iCol];
                t[iRow, iCol, false] *= F(xi, yi);
            };
            Loop2D loop = new(operation: a,
                rowStart: 0, rowEnd: grid.Rows,
                colStart: 0, colEnd: grid.Cols,
                rowStep: 1, colStep: 1);
            loop.Evaluate(mode: loopMode);
        }

        /// <summary>
        /// multiplies the function on a uniformly sampled target data
        /// </summary>
        /// <param name="x"> uniformly sampled target data (in/out) </param>
        /// <param name="grid"> sampling grid of the data </param>
        /// <param name="part"> target complex part in the data to modify </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        public void ScaleOn(ref MatrixZ x, GridInfo2D grid,
            ComplexPart part = ComplexPart.RealPart,
            LoopMode loopMode = Defaults.LoopOption)
        {
            // consistency check
            if (x.Rows != grid.Rows || x.Cols != grid.Cols)
            { throw new ArgumentException($"Inconsistent number of input elements"); }

            MatrixZ t = x;
            // defines loop operation
            Action<long, long> a = (iRow, iCol) =>
            {
                (double yi, double xi) = grid[iRow, iCol];
                switch (part)
                {
                    case ComplexPart.RealPart:
                        {
                            Complex ti = t[iRow, iCol, checkBound: false];
                            t[iRow, iCol, checkBound: false] += (F(xi, yi) - 1.0) * ti.Real;
                            break;
                        }
                    case ComplexPart.ImagPart:
                        {
                            Complex ti = t[iRow, iCol, checkBound: false];
                            t[iRow, iCol, checkBound: false] += (F(xi, yi) - 1.0) * ti.Imaginary * Complex.ImaginaryOne;
                            break;
                        }
                    case ComplexPart.Magnitude:
                        t[iRow, iCol, checkBound: false] *= F(xi, yi);
                        break;
                    case ComplexPart.Argument:
                        {
                            Complex ti = t[iRow, iCol, checkBound: false];
                            t[iRow, iCol, checkBound: false] *= Complex.Exp(Complex.ImaginaryOne * (F(xi, yi) - 1.0) * ti.Phase);
                            break;
                        }
                }
            };
            Loop2D loop = new(operation: a,
                rowStart: 0, rowEnd: grid.Rows,
                colStart: 0, colEnd: grid.Cols,
                rowStep: 1, colStep: 1);
            loop.Evaluate(mode: loopMode);
        }

        #endregion

        #endregion
    }

}
