using System.Numerics;

namespace VEMS.MathCore
{

    /// <summary>
    /// uniform-sampler for two-dimensional 
    /// complex-valued function
    /// </summary>
    public class Samp2DCplxFunc
    {
        #region properties

        /// <summary>
        /// internally stored two-variable function F(x,y)
        /// for fixed parameters {p}
        /// </summary>
        private Func<double, double, Complex> F { get; set; }

        #endregion
        #region constructor

        /// <summary>
        /// default construcotr
        /// </summary>
        internal Samp2DCplxFunc() 
        {
            F = (x, y) => Complex.Zero;
        }

        /// <summary>
        /// constructs a sampler for
        /// two-dimensional complex-valued function
        /// </summary>
        /// <param name="f"> complex-valued function F(x,y) </param>
        public Samp2DCplxFunc(Func<double, double, Complex> f)
        {
            F = f;
        }

        /// <summary>
        /// constructs a sampler for
        /// two-dimensional complex-valued function
        /// </summary>
        /// <param name="f"> complex-valued function F(x,y; {p}) with parameters {p} </param>
        /// <param name="p"> parameters {p} used to define the function </param>
        public Samp2DCplxFunc(Func<double, double, List<double>?, Complex> f,
            List<double>? p = null)
        {
            // defines two-variable function with given parameters {p}
            F = (x, y) => f(x, y, p);
        }

        #endregion
        #region methods

        #region ----- sampling -----

        /// <summary>
        /// samples the function on a set of (x,y) locations
        /// either uniform or scattered
        /// </summary>
        /// <param name="rho"> sample locations, either uniform or scattered </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> sampled function values on these locations </returns>
        public VectorZ Sample(ScatInfo2D rho, 
            LoopMode loopMode = Defaults.LoopOption)
        {
            // initialization
            VectorZ fs = new(rho.Count);
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
        public MatrixZ Sample(ScatInfoXY xy,
            LoopMode loopMode = Defaults.LoopOption)
        {
            // initialization
            MatrixZ f = new(rows: xy.Rows, cols: xy.Cols);
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
        public MatrixZ Sample(GridInfo2D grid,
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
        public void AddTo(ref MatrixZ x, GridInfo2D grid,
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
                t[iRow, iCol, false] += F(xi, yi);
            };
            Loop2D loop = new(operation: a,
                rowStart: 0, rowEnd: grid.Rows,
                colStart: 0, colEnd: grid.Cols,
                rowStep: 1, colStep: 1);
            loop.Evaluate(mode: loopMode);
        }

        /// <summary>
        /// scales the function on a uniformly sampled target data
        /// </summary>
        /// <param name="x"> uniformly sampled target data (in/out) </param>
        /// <param name="grid"> sampling grid of the data </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        public void ScaleOn(ref MatrixZ x, GridInfo2D grid,
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
                t[iRow, iCol, false] *= F(xi, yi);
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
