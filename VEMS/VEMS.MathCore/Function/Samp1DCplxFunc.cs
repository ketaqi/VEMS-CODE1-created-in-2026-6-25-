using System.Numerics;

namespace VEMS.MathCore
{
    /// <summary>
    /// sampler for 1D complex-valued function
    /// </summary>
    public class Samp1DCplxFunc
    {
        #region properties

        /// <summary>
        /// internally stored single-value finction F(x)
        /// with fixed parameters {p}
        /// </summary>
        private Func<double, Complex> F { get; set; }

        #endregion
        #region constructor

        /// <summary>
        /// default constructor
        /// </summary>
        internal Samp1DCplxFunc() 
        {
            F = (x) => Complex.Zero;
        }

        /// <summary>
        /// constructs a sampler for
        /// one-dimensional complex-valued function
        /// </summary>
        /// <param name="f"> complex-valued function F(x) </param>
        public Samp1DCplxFunc(Func<double, Complex> f)
        {
            F = f;
        }

        /// <summary>
        /// constructs a sampler for
        /// one-dimensional complex-valued function
        /// </summary>
        /// <param name="f"> complex-valued function F(x; {p}) with parameters {p} </param>
        /// <param name="p"> parameters {p} used to define the function </param>
        public Samp1DCplxFunc(Func<double, List<double>?, Complex> f,
            List<double>? p = null)
        {
            // defines single-variable function with given parameters {p}
            F = (x) => f(x, p);
        }

        #endregion
        #region methods 

        #region ----- sampling -----

        /// <summary>
        /// samples the function on a set of locations
        /// either uniform or scattered
        /// </summary>
        /// <param name="xs"> sample locations, either uniform or scattered </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> sampled function values on these locations </returns>
        public VectorZ Sample(ScatInfo1D xs,
            LoopMode loopMode = Defaults.LoopOption)
        {          
            // initialization
            VectorZ fs = new(count: xs.Count);
            // defines loop operation
            Loop1D loop = new(operation: (i) => fs[i, false] = F(xs[i, false]),
                start: 0, end: xs.Count, step: 1);
            loop.Evaluate(mode: loopMode);
            // return
            return fs;
        }

        /// <summary>
        /// samples the function on a given uniform grid
        /// </summary>
        /// <param name="grid"> target grid </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> sampled function values on the grid </returns>
        public VectorZ Sample(GridInfo1D grid,
            LoopMode loopMode = Defaults.LoopOption)
            => Sample(xs: (ScatInfo1D)grid, loopMode: loopMode);

        /// <summary>
        /// samples the function on a target 2D uniform grid
        /// </summary>
        /// <param name="grid"> target 2D uniform grid </param>
        /// <param name="isFuncAlongX"> whether the function is defined along x or y direction </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> sampled function values on the target grid </returns>
        public MatrixZ Sample(GridInfo2D grid, bool isFuncAlongX,
            LoopMode loopMode = Defaults.LoopOption)
        {
            MatrixZ v = new(rows: grid.Rows, cols: grid.Cols);
            // defines loop operation
            Action<long, long> a = (iRow, iCol) =>
            {
                (double yi, double xi) = grid[iRow, iCol];
                v[iRow, iCol, checkBound: false] = isFuncAlongX ? F(xi) : F(yi);
            };
            Loop2D loop = new(operation: a,
                rowStart: 0, rowEnd: grid.Rows,
                colStart: 0, colEnd: grid.Cols,
                rowStep: 1, colStep: 1);
            loop.Evaluate(mode: loopMode);
            // return 
            return v;
        }

        #endregion
        #region ----- modification -----

        /// <summary>
        /// adds the function to uniformly sampled target data
        /// </summary>
        /// <param name="x"> uniformly sampled target data (in/out) </param>
        /// <param name="grid"> sampling grid of the data </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        public void AddTo(ref VectorZ x, GridInfo1D grid,
            LoopMode loopMode = Defaults.LoopOption)
        {
            // consistency check
            if (x.Count != grid.Count)
            { throw new ArgumentException($"Inconsistent number of input elements"); }

            VectorZ t = x;
            // defines loop operation
            Loop1D loop = new(operation: (i) => t[i, checkBound: false] += F(grid[i]),
                start: 0, end: grid.Count, step: 1);
            loop.Evaluate(mode: loopMode);
        }

        /// <summary>
        /// adds the function to uniformly sampled target data
        /// </summary>
        /// <param name="x"> uniformly sampled target data (in/out) </param>
        /// <param name="grid"> sampling grid of the data </param>
        /// <param name="isFuncAlongX"> whether the function is defined along x or y direction </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        public void AddTo(ref MatrixZ x, GridInfo2D grid, bool isFuncAlongX,
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
                t[iRow, iCol, checkBound: false] += isFuncAlongX ? F(xi) : F(yi);
            };
            Loop2D loop = new(operation: a,
                rowStart: 0, rowEnd: grid.Rows,
                colStart: 0, colEnd: grid.Cols,
                rowStep: 1, colStep: 1);
            loop.Evaluate(mode: loopMode);
        }

        /// <summary>
        /// multiplies the function on uniformly sampled target data
        /// </summary>
        /// <param name="x"> uniformly sampled target data (in/out) </param>
        /// <param name="grid"> sampling grid of the data </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        public void ScaleOn(ref VectorZ x, GridInfo1D grid,
            LoopMode loopMode = Defaults.LoopOption)
        {
            // consistency check
            if (x.Count != grid.Count)
            { throw new ArgumentException($"Inconsistent number of input elements"); }

            VectorZ t = x;
            // defines loop operation
            Loop1D loop = new(operation: (i) => t[i, false] *= F(grid[i]),
                start: 0, end: grid.Count, step: 1);
            loop.Evaluate(mode: loopMode);
        }

        /// <summary>
        /// multiplies the function on uniformly sampled target data
        /// </summary>
        /// <param name="x"> uniformly sampled target data (in/out) </param>
        /// <param name="grid"> sampling grid of the data </param>
        /// <param name="isFuncAlongX"> whether the function is defined along x or y direction </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        public void ScaleOn(ref MatrixZ x, GridInfo2D grid, bool isFuncAlongX,
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
                t[iRow, iCol, checkBound: false] *= isFuncAlongX ? F(xi) : F(yi);
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
