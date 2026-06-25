using Complex = System.Numerics.Complex;

namespace VEMS.MathCore
{
    /// <summary>
    /// sampler for 1D real-valued function
    /// </summary>
    public class Samp1DRealFunc 
    {
        #region properties

        /// <summary>
        /// internally stored single-variable finction F(x)
        /// for fixed parameters {p}
        /// </summary>
        private Func<double, double> F { get; set; }

        #endregion
        #region constructor

        /// <summary>
        /// default constructor
        /// </summary>
        internal Samp1DRealFunc() 
        {
            F = (x) => 0.0;
        }

        /// <summary>
        /// constructs a sampler for
        /// 1D real-valued function
        /// </summary>
        /// <param name="f"> real-valued function F(x) </param>
        public Samp1DRealFunc(Func<double, double> f)
        {
            F = f;
        }

        /// <summary>
        /// constructs a sampler for
        /// 1D real-valued function
        /// </summary>
        /// <param name="f"> real-valued function F(x; {p}) with parameters {p} </param>
        /// <param name="p"> parameters {p} used to define the function </param>
        public Samp1DRealFunc(Func<double, List<double>?, double> f,
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
        public VectorD Sample(ScatInfo1D xs, 
            LoopMode loopMode = Defaults.LoopOption)
        {
            // initialization
            VectorD fs = new(count: xs.Count);
            // defines loop operation
            Loop1D loop = new(operation: (i) => fs[i, false] = F(xs[i, false]), 
                start: 0, end: xs.Count, step: 1);
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
        public VectorD Sample(GridInfo1D grid,
            LoopMode loopMode = Defaults.LoopOption)
            => Sample(xs: (ScatInfo1D)grid, loopMode: loopMode);

        /// <summary>
        /// samples the function on a target 2D uniform grid
        /// </summary>
        /// <param name="grid"> target 2D uniform grid </param>
        /// <param name="isFuncAlongX"> whether the function is defined along x or y direction </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> sampled function values on the target grid </returns>
        public MatrixD Sample(GridInfo2D grid, bool isFuncAlongX,
            LoopMode loopMode = Defaults.LoopOption)
        {
            MatrixD v = new(rows: grid.Rows, cols: grid.Cols);
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
        public void AddTo(ref VectorD x, GridInfo1D grid,
            LoopMode loopMode = Defaults.LoopOption)
        {
            // consistency check
            if (x.Count != grid.Count)
            { throw new ArgumentException($"Inconsistent number of input elements"); }
            
            VectorD t = x;
            // defines loop operation
            Loop1D loop = new(operation: (i) => t[i, checkBound: false] += F(grid[i]),
                start: 0, end: grid.Count, step: 1);
            loop.Evaluate(mode: loopMode);
        }

        /// <summary>
        /// adds the function to uniformly sampled target data
        /// </summary>
        /// <param name="x"> uniformly sampled target data (in/out) </param>
        /// <param name="grid"> sampling grid of the data ` </param>
        /// <param name="isFuncAlongX"> whether the function is defined along x or y direction </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        public void AddTo(ref MatrixD x, GridInfo2D grid, bool isFuncAlongX,
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
                t[iRow, iCol, checkBound: false] += isFuncAlongX ? F(xi) : F(yi);
            };
            Loop2D loop = new(operation: a,
                rowStart: 0, rowEnd: grid.Rows,
                colStart: 0, colEnd: grid.Cols,
                rowStep: 1, colStep: 1);
            loop.Evaluate(mode: loopMode);
        }

        /// <summary>
        /// adds the function to uniformly sampled target data
        /// </summary>
        /// <param name="x"> uniformly sampled target data (in/out) </param>
        /// <param name="grid"> sampling grid of the data </param>
        /// <param name="part"> target complex part in the data to modify </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        public void AddTo(ref VectorZ x, GridInfo1D grid,
            ComplexPart part = ComplexPart.RealPart, 
            LoopMode loopMode = Defaults.LoopOption)
        {
            // consistency check
            if (x.Count != grid.Count)
            { throw new ArgumentException($"Inconsistent number of input elements"); }

            VectorZ t = x;
            // defines loop operation
            Action<long> a = (i) =>
            {
                double xi = grid[i];
                switch (part)
                {
                    case ComplexPart.RealPart:
                        t[i, checkBound: false] += F(xi);
                        break;
                    case ComplexPart.ImagPart:
                        t[i, checkBound: false] += F(xi) * Complex.ImaginaryOne;
                        break;
                    case ComplexPart.Magnitude:
                        {
                            Complex ti = t[i, checkBound: false];
                            t[i, checkBound: false] += F(xi) * Complex.Exp(Complex.ImaginaryOne * ti.Phase);
                            break;
                        }
                    case ComplexPart.Argument:
                        t[i, checkBound: false] *= Complex.Exp(Complex.ImaginaryOne * F(xi));
                        break;
                }
            };
            Loop1D loop = new(operation: a, start: 0, end: grid.Count, step: 1);
            loop.Evaluate(mode: loopMode);
        }

        /// <summary>
        /// adds the function to uniformly sampled target data
        /// </summary>
        /// <param name="x"> uniformly sampled target data (in/out) </param>
        /// <param name="grid"> sampling grid of the data </param>
        /// <param name="isFuncAlongX"> whether the function is defined along x or y direction </param>
        /// <param name="part"> target complex part in the data to modify </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        public void AddTo(ref MatrixZ x, GridInfo2D grid, bool isFuncAlongX,
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
                        t[iRow, iCol, checkBound: false] += isFuncAlongX ? F(xi) : F(yi);
                        break;
                    case ComplexPart.ImagPart:
                        t[iRow, iCol, checkBound: false] += isFuncAlongX ? 
                        F(xi) * Complex.ImaginaryOne : F(yi) * Complex.ImaginaryOne;
                        break;
                    case ComplexPart.Magnitude:
                        {
                            Complex ti = t[iRow, iCol, checkBound: false];
                            t[iRow, iCol, checkBound: false] += isFuncAlongX ? 
                            F(xi) * Complex.Exp(Complex.ImaginaryOne * ti.Phase) : F(yi) * Complex.Exp(Complex.ImaginaryOne * ti.Phase);
                            break;
                        }
                    case ComplexPart.Argument:
                        t[iRow, iCol, checkBound: false] *= isFuncAlongX ?
                        Complex.Exp(Complex.ImaginaryOne * F(xi)) : Complex.Exp(Complex.ImaginaryOne * F(yi));
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
        /// multiplies the function on uniformly sampled target data
        /// </summary>
        /// <param name="x"> uniformly sampled target data (in/out) </param>
        /// <param name="grid"> sampling grid of the data </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        public void ScaleOn(ref VectorD x, GridInfo1D grid,
            LoopMode loopMode = Defaults.LoopOption)
        {
            // consistency check
            if (x.Count != grid.Count)
            { throw new ArgumentException($"Inconsistent number of input elements"); }

            VectorD t = x;
            // defines loop operation
            Loop1D loop = new(operation: (i) => t[i, checkBound: false] *= F(grid[i]),
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
        public void ScaleOn(ref MatrixD x, GridInfo2D grid, bool isFuncAlongX,
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
                t[iRow, iCol, checkBound: false] *= isFuncAlongX ? F(xi) : F(yi);
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
        /// <param name="part"> target complex part in the data to modify </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        public void ScaleOn(ref VectorZ x, GridInfo1D grid,
            ComplexPart part = ComplexPart.RealPart,
            LoopMode loopMode = Defaults.LoopOption)
        {
            // consistency check
            if (x.Count != grid.Count)
            { throw new ArgumentException($"Inconsistent number of input elements"); }

            VectorZ t = x;
            // defines loop operation
            Action<long> a = (i) =>
            {
                double xi = grid[i];
                switch (part)
                {
                    case ComplexPart.RealPart:
                        {
                            Complex ti = t[i, checkBound: false];
                            t[i, checkBound: false] += (F(xi) - 1.0) * ti.Real;
                            break;
                        }
                    case ComplexPart.ImagPart:
                        {
                            Complex ti = t[i, checkBound: false];
                            t[i, checkBound: false] += (F(xi) - 1.0) * ti.Imaginary * Complex.ImaginaryOne;
                            break;
                        }
                    case ComplexPart.Magnitude:
                        t[i, checkBound: false] *= F(xi);
                        break;
                    case ComplexPart.Argument:
                        {
                            Complex ti = t[i, checkBound: false];
                            t[i, checkBound: false] *= Complex.Exp(Complex.ImaginaryOne * (F(xi) - 1.0) * ti.Phase);
                            break;
                        }
                }
            };
            Loop1D loop = new(operation: a, start: 0, end: grid.Count, step: 1);
            loop.Evaluate(mode: loopMode);
        }

        /// <summary>
        /// multiplies the function on uniformly sampled target data
        /// </summary>
        /// <param name="x"> uniformly sampled target data (in/out) </param>
        /// <param name="grid"> sampling grid of the data </param>
        /// <param name="isFuncAlongX"> whether the function is defined along x or y direction </param>
        /// <param name="part"> target complex part in the data to modify </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        public void ScaleOn(ref MatrixZ x, GridInfo2D grid, bool isFuncAlongX,
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
                            t[iRow, iCol, checkBound: false] += isFuncAlongX ?
                            (F(xi) - 1.0) * ti.Real : (F(yi) - 1.0) * ti.Real;
                            break;
                        }
                    case ComplexPart.ImagPart:
                        {
                            Complex ti = t[iRow, iCol, checkBound: false];
                            t[iRow, iCol, checkBound: false] += isFuncAlongX ?
                            (F(xi) - 1.0) * ti.Imaginary * Complex.ImaginaryOne : (F(yi) - 1.0) * ti.Imaginary * Complex.ImaginaryOne;
                            break;
                        }
                    case ComplexPart.Magnitude:
                        t[iRow, iCol, checkBound: false] *= isFuncAlongX ? F(xi) : F(yi);
                        break;
                    case ComplexPart.Argument:
                        {
                            Complex ti = t[iRow, iCol, checkBound: false];
                            t[iRow, iCol, checkBound: false] *= isFuncAlongX ?
                            Complex.Exp(Complex.ImaginaryOne * (F(xi) - 1.0) * ti.Phase) : Complex.Exp(Complex.ImaginaryOne * (F(yi) - 1.0) * ti.Phase);
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
