using Complex = System.Numerics.Complex;

namespace VEMS.MathCore
{

    /// <summary>
    /// sampler for separable two-dimensional (2D)
    /// real-valued function
    /// </summary>
    public class SampXYCplxFunc
    {
        #region properties

        /// <summary>
        /// internally stored single-variable function Fx(x)
        /// for fixed parameters {Px}
        /// </summary>
        private Func<double, Complex> Fx { get; set; }

        /// <summary>
        /// internally stored single-variable function Fy(x)
        /// for fixed parameters {Py}
        /// </summary>
        private Func<double, Complex> Fy { get; set; }

        #endregion
        #region constructors

        /// <summary>
        /// default constructor
        /// </summary>
        internal SampXYCplxFunc() 
        {
            Fx = (x) => Complex.Zero;
            Fy = (y) => Complex.Zero;
        }

        /// <summary>
        /// constructs a sampler for x,y-separable
        /// two-dimensional (2D) complex--valued function
        /// </summary>
        /// <param name="fx"> complex-valued function Fx(x) </param>
        /// <param name="fy"> complex-valued function Fy(y) </param>
        public SampXYCplxFunc(Func<double, Complex> fx, 
            Func<double, Complex> fy)
        {
            Fx = fx;
            Fy = fy;
        }

        /// <summary>
        /// constructs a sampler for x,y-separable 
        /// two-dimensional (2D) complex-valued function
        /// </summary>
        /// <param name="fx"> fx = fx(x; {px}) </param>
        /// <param name="fy"> fy = fy(y; {py}) </param>
        /// <param name="px"> list of parameters {px} </param>
        /// <param name="py"> list of parameters {py} </param>
        public SampXYCplxFunc(Func<double, List<double>?, Complex> fx, 
            Func<double, List<double>?, Complex> fy, 
            List<double>? px = null, 
            List<double>? py = null)
        {
            // defines single-variable functions with given parameters
            Fx = (x) => fx(x, px);
            Fy = (y) => fy(y, py);
        }

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
        public VectorZ Sample(ScatInfo2D rho,
            LoopMode loopMode = Defaults.LoopOption)
        {
            // initialization
            VectorZ fs = new(count: rho.Count);
            // defines loop operation
            Action<long> a = (i) =>
            {
                (double y, double x) = rho[i, false];
                fs[i, false] = Fx(x) * Fy(y);
            };
            Loop1D loop = new(operation: a,
                start: 0, end: rho.Count, step: 1);
            loop.Evaluate(mode: loopMode);
            // return
            return fs;
        }

        /// <summary>
        /// samples the function on a set of (x,y) locations
        /// either uniform or scattered
        /// </summary>
        /// <param name="xy"> scattered sample locations x and y </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> sampled function values on these locations </returns>
        /// <exception cref="ArgumentNullException"></exception>
        public MatrixZ Sample(ScatInfoXY xy,
            LoopMode loopMode = Defaults.LoopOption)
        {
            // initialization
            MatrixZ fs = new(rows: xy.Rows, cols: xy.Cols);
            // defines loop operation
            Action<long, long> a = (iRow, iCol) =>
            {
                (double y, double x) = xy[iRow, iCol, false];
                fs[iRow, iCol, false] = Fx(x) * Fy(y);
            };
            Loop2D loop = new(operation: a,
                rowStart: 0, rowEnd: xy.Rows,
                colStart: 0, colEnd: xy.Cols,
                rowStep: 1, colStep: 1);
            loop.Evaluate(mode: loopMode);
            // return
            return fs;
        }

        /// <summary>
        /// samples the function on a target uniform grid
        /// </summary>
        /// <param name="grid"> target grid </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> sampled function values on the target grid </returns>
        public MatrixZ Sample(GridInfo2D grid,
            LoopMode loopMode = Defaults.LoopOption)
            => Sample(xy: (ScatInfoXY)grid,
                loopMode: loopMode);

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
                t[iRow, iCol, false] += Fx(xi) * Fy(yi);
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
                t[iRow, iCol, false] *= Fx(xi) * Fy(yi);
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
