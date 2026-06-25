using Complex = System.Numerics.Complex;

namespace VEMS.MathCore
{ 
    /// <summary>
    /// interpolation for 1D real-valued grid data
    /// </summary>
    public class Grid1DRealInterpolation
    {
        #region properties

        /// <summary>
        /// pointwise evaluation function
        /// <para> variable: location x for the evaluation </para>
        /// <para> result: interpolated value </para>
        /// </summary>
        public Func<double, double> FindValue { get; set; }

        /// <summary>
        /// input data values
        /// </summary>
        public VectorD Values { get; set; }

        /// <summary>
        /// sampling grid for the input data
        /// </summary>
        public GridInfo1D Grid { get; set; }

        /// <summary>
        /// data boundary option: periodic or zero
        /// </summary>
        public DataBoundary Boundary { get; set; }

        /// <summary>
        /// interpolation method option
        /// <para> Nearest: nearest-neighbor interpolation </para>
        /// <para> Linear: linear interpolation </para>
        /// <para> Cubic: cubic interpolation </para>
        /// </summary>
        public InterpolationMethod Method { get; set; }

        /// <summary>
        /// internally stored shrinked grid for interpolation
        /// [only used for linear and cubic methods]
        /// </summary>
        private GridInfo1D? ShrinkedGrid { get; set; }

        /// <summary>
        /// internally stored finite difference [Dx] values
        /// [only used for cubic method]
        /// </summary>
        private VectorD? Dx { get; set; }

        #endregion
        #region constructor

        /// <summary>
        /// constructs an interpolation class
        /// for 1D real-valued grid data
        /// </summary>
        /// <param name="v"> input data </param>
        /// <param name="grid"> sampling grid for the input data </param>
        /// <param name="method"> selected interpolation method </param>
        /// <param name="bound"> data boundary option </param>
        public Grid1DRealInterpolation(VectorD v, GridInfo1D grid,
            InterpolationMethod method = Defaults.IntrplOption,
            DataBoundary bound =  Defaults.BoundaryOption)
        {
            // sets input data
            Values = v;
            Grid = grid;
            Boundary = bound;
            //Periodic = periodic;
            Method = method;
            // shrinked grid?
            if (Method == InterpolationMethod.Linear || Method == InterpolationMethod.Cubic)
            { ShrinkedGrid = new(n: Grid.Count - 1, spacing: Grid.Spacing, start: Grid.Start + 0.5 * Grid.Spacing); }
            // derivatives?
            if (Method == InterpolationMethod.Cubic)
            {
                Grid1DRealFiDi fx = new(vs: Values, grid: null, option: FiDi1DOption.Dt);
                Dx = fx.EvaluateAll(); 
            }
            // defines FindValue 
            switch (Method)
            {
                case InterpolationMethod.Sinc:
                    FindValue = (x) => PointInterpolation.Sinc(Values, Grid, x, Boundary);
                    break;
                case InterpolationMethod.SincFFT:
                    throw new NotImplementedException();
                case InterpolationMethod.Nearest:
                    FindValue = (x) => PointInterpolation.Nearest(Values, Grid, x, Boundary);
                    break;
                case InterpolationMethod.Linear:
                    FindValue = (x) => PointInterpolation.Linear(Values, Grid, ShrinkedGrid!, x, Boundary);
                    break;
                case InterpolationMethod.Cubic:
                    FindValue = (x) => PointInterpolation.Cubic(Values, Grid, ShrinkedGrid!, Dx!, x, Boundary);
                    break;
                default: goto case InterpolationMethod.Nearest;
            };
        }

        #endregion
        #region methods

        /// <summary>
        /// evaluates values on a set of scattered locations
        /// </summary>
        /// <param name="xs"> set of scattered locations for evaluation </param>
        /// <param name="loopMode"> computational option for loops </param>
        /// <returns> interpolated values on those evaluation locations </returns>
        public VectorD Evaluate(ScatInfo1D xs,
            LoopMode loopMode = Defaults.LoopOption)
        {           
            // initialize
            VectorD vs = new(xs.Count);
            // defines loop operation
            Loop1D loop = new(operation: (i) => vs[i, false] = FindValue(xs[i, false]),
                start: 0, end: xs.Count, step: 1);
            loop.Evaluate(mode: loopMode);

            // return
            return vs;
        }

        /// <summary>
        /// evaluates values on a target grid
        /// </summary>
        /// <param name="targetGrid"> target grid for evaluation </param>
        /// <param name="loopMode"> computational option for loops </param>
        /// <returns> interpolated values on the target grid </returns>
        public VectorD Evaluate(GridInfo1D targetGrid,
            LoopMode loopMode = Defaults.LoopOption)
            => Evaluate(xs: (ScatInfo1D)targetGrid, loopMode: loopMode);

        #endregion
    }

    /// <summary>
    /// interpolation for 1D real-valued grid data
    /// </summary>
    public class Grid1DCplxInterpolation
    {
        #region properties

        /// <summary>
        /// pointwise evaluation function
        /// <para> variable: location x for the evaluation </para>
        /// <para> result: interpolated value </para>
        /// </summary>
        public Func<double, Complex> FindValue { get; set; }

        /// <summary>
        /// input data values
        /// </summary>
        public VectorZ Values { get; set; }

        /// <summary>
        /// sampling grid for the input data
        /// </summary>
        public GridInfo1D Grid { get; set; }

        /// <summary>
        /// data boundary option: periodic or zero
        /// </summary>
        public DataBoundary Boundary { get; set; }

        /// <summary>
        /// interpolation method option
        /// <para> Nearest: nearest-neighbor interpolation </para>
        /// <para> Linear: linear interpolation </para>
        /// <para> Cubic: cubic interpolation </para>
        /// </summary>
        public InterpolationMethod Method { get; set; }

        /// <summary>
        /// internally stored shrinked grid for interpolation
        /// [only used for linear and cubic methods]
        /// </summary>
        private GridInfo1D? ShrinkedGrid { get; set; }

        /// <summary>
        /// internally stored finite difference [Dx] values
        /// [only used for cubic method]
        /// </summary>
        private VectorZ? Dx { get; set; }

        #endregion
        #region constructor

        /// <summary>
        /// constructs an interpolation class
        /// for 1D complex-valued grid data
        /// </summary>
        /// <param name="v"> input data </param>
        /// <param name="grid"> sampling grid for the input data </param>
        /// <param name="method"> selected interpolation method </param>
        /// <param name="bound"> data boundary option </param>
        public Grid1DCplxInterpolation(VectorZ v, GridInfo1D grid,
            InterpolationMethod method = Defaults.IntrplOption,
            DataBoundary bound = Defaults.BoundaryOption)
        {
            // sets input data
            Values = v;
            Grid = grid;
            Boundary = bound;
            Method = method;
            // shrinked grid?
            if (Method == InterpolationMethod.Linear || Method == InterpolationMethod.Cubic)
            { ShrinkedGrid = new(n: Grid.Count - 1, spacing: Grid.Spacing, start: Grid.Start + 0.5 * Grid.Spacing); }
            // derivatives
            if (Method == InterpolationMethod.Cubic)
            {
                Grid1DCplxFiDi fx = new(vs: Values, grid: null, option: FiDi1DOption.Dt);
                Dx = fx.EvaluateAll();
            }
            // defines FindValue 
            switch (Method)
            {
                case InterpolationMethod.Sinc:
                    FindValue = (x) => PointInterpolation.Sinc(Values, Grid, x, Boundary);
                    break;
                case InterpolationMethod.SincFFT:
                    throw new NotImplementedException();
                case InterpolationMethod.Nearest:
                    FindValue = (x) => PointInterpolation.Nearest(Values, Grid, x, Boundary);
                    break;
                case InterpolationMethod.Linear:
                    FindValue = (x) => PointInterpolation.Linear(Values, Grid, ShrinkedGrid!, x, Boundary);
                    break;
                case InterpolationMethod.Cubic:
                    FindValue = (x) => PointInterpolation.Cubic(Values, Grid, ShrinkedGrid!, Dx!, x, Boundary);
                    break;
                default: goto case InterpolationMethod.Nearest;
            };
        }

        #endregion
        #region methods

        /// <summary>
        /// evaluates values on a set of scattered locations
        /// </summary>
        /// <param name="xs"> set of scattered locations for evaluation </param>
        /// <param name="loopMode"> computational option for loops </param>
        /// <returns> interpolated values on those evaluation locations </returns>
        public VectorZ Evaluate(ScatInfo1D xs,
            LoopMode loopMode = Defaults.LoopOption)
        {
            // initialize
            VectorZ vs = new(xs.Count);
            // defines loop operation
            Loop1D loop = new(operation: (i) => vs[i, false] = FindValue(xs[i, false]),
                start: 0, end: xs.Count, step: 1);
            loop.Evaluate(mode: loopMode);

            // return
            return vs;
        }

        /// <summary>
        /// evaluates values on a target grid
        /// </summary>
        /// <param name="targetGrid"> target grid for evaluation </param>
        /// <param name="loopMode"> computational option for loops </param>
        /// <returns> interpolated values on the target grid </returns>
        public VectorZ Evaluate(GridInfo1D targetGrid,
            LoopMode loopMode = Defaults.LoopOption)
            => Evaluate(xs: (ScatInfo1D)targetGrid, loopMode: loopMode);

        #endregion
    }

    /// <summary>
    /// interpolation for 2D real-valued grid data
    /// </summary>
    public class Grid2DRealInterpolation
    {
        #region properties

        /// <summary>
        /// pointwise evaluation function
        /// <para> variable #1: location x for the evaluation </para>
        /// <para> variable #2: location y for the evaluation </para>
        /// <para> result: interpolated value </para>
        /// </summary>
        public Func<double, double, double> FindValue { get; set; }

        /// <summary>
        /// input data values
        /// </summary>
        public MatrixD Values { get; set; }

        /// <summary>
        /// sampling grid for the input data
        /// </summary>
        public GridInfo2D Grid { get; set; }

        /// <summary>
        /// data boundary option along x : periodic or zero
        /// </summary>
        public DataBoundary BoundaryX { get; set; }

        /// <summary>
        /// data boundary option along y : periodic or zero
        /// </summary>
        public DataBoundary BoundaryY { get; set; }

        /// <summary>
        /// interpolation method option
        /// <para> Nearest: nearest-neighbor interpolation </para>
        /// <para> Linear: linear interpolation </para>
        /// <para> Cubic: cubic interpolation </para>
        /// </summary>
        public InterpolationMethod Method { get; set; }

        /// <summary>
        /// internally stored shrinked grid for interpolation
        /// [only used for linear and cubic methods]
        /// </summary>
        private GridInfo2D? ShrinkedGrid { get; set; }

        /// <summary>
        /// internally stored finite difference [Dx] values
        /// [only used for cubic method]
        /// </summary>
        private MatrixD? Dx { get; set; }

        /// <summary>
        /// internally stored finite difference [Dy] values
        /// [only used for cubic method]
        /// </summary>
        private MatrixD? Dy { get; set; }

        /// <summary>
        /// internally stored finite difference [Dxy] values
        /// [only used for cubic method]
        /// </summary>
        private MatrixD? Dxy { get; set; }

        #endregion
        #region constructor

        /// <summary>
        /// constructs an interpolation class
        /// for 2D real-valued grid data
        /// </summary>
        /// <param name="v"> input data </param>
        /// <param name="grid"> sampling grid for the input data </param>
        /// <param name="method"> selected interpolation method </param>
        /// <param name="boundX"> data boundary option along x-axis </param>
        /// <param name="boundY"> data boundary option along y-axis </param>
        public Grid2DRealInterpolation(MatrixD v, GridInfo2D grid,
            InterpolationMethod method = Defaults.IntrplOption,
            DataBoundary boundX = Defaults.BoundaryOption,
            DataBoundary boundY = Defaults.BoundaryOption)
        {
            // sets input data
            Values = v;
            Grid = grid;
            BoundaryX = boundX;
            BoundaryY = boundY;
            Method = method;
            // shrinked grid?
            if (Method == InterpolationMethod.Linear || Method == InterpolationMethod.Cubic)
            {
                ShrinkedGrid = new(rows: Grid.Rows - 1, cols: Grid.Cols - 1,
                    spacingY: Grid.SpacingY, spacingX: Grid.SpacingX,
                    startY: Grid.StartY + 0.5 * Grid.SpacingY, startX: Grid.StartX + 0.5 * Grid.SpacingX); 
            }
            // derivatives
            if (Method == InterpolationMethod.Cubic)
            {
                Grid2DRealFiDi Fx = new(vs: Values, grid: null, option: FiDi2DOption.Dx);
                Grid2DRealFiDi Fy = new(vs: Values, grid: null, option: FiDi2DOption.Dy);
                Grid2DRealFiDi Fxy = new(vs: Values, grid: null, option: FiDi2DOption.Dxy);
                Dx = Fx.EvaluateAll();
                Dy = Fy.EvaluateAll();
                Dxy = Fxy.EvaluateAll();
            }
            // defines FindValue
            switch (Method)
            {
                case InterpolationMethod.Sinc:
                    FindValue = (x, y) => PointInterpolation.Sinc(Values, Grid, 
                        x, y, BoundaryX, BoundaryY);
                    break;
                case InterpolationMethod.SincFFT:
                    throw new NotImplementedException();
                case InterpolationMethod.Nearest:
                    FindValue = (x, y) => PointInterpolation.Nearest(Values, Grid,
                        x, y, BoundaryX, BoundaryY);
                    break;
                case InterpolationMethod.Linear:
                    FindValue = (x, y) => PointInterpolation.Linear(Values, Grid, ShrinkedGrid!,
                        x, y, BoundaryX, BoundaryY);
                    break;
                case InterpolationMethod.Cubic:
                    FindValue = (x, y) => PointInterpolation.Cubic(Values, Grid, ShrinkedGrid!, Dx!, Dy!, Dxy!,
                        x, y, BoundaryX, BoundaryY);
                    break;
                default: goto case InterpolationMethod.Nearest;
            };
        }

        #endregion
        #region methods

        /// <summary>
        /// evaluates values on a set of scattered locations
        /// </summary>
        /// <param name="rho"> set of scattered locations for evaluation </param>
        /// <param name="loopMode"> computational option for loops </param>
        /// <returns> interpolated values on those evaluation locations </returns>
        public VectorD Evaluate(ScatInfo2D rho,
            LoopMode loopMode = Defaults.LoopOption)
        {
            // initialize
            VectorD vs = new(count: rho.Count);
            // defines loop operation
            Action<long> a = (i) =>
            {
                (double y, double x) = rho[i, false];
                vs[i, false] = FindValue(arg1: x, arg2: y);
            };
            Loop1D loop = new(operation: a,
                start: 0, end: rho.Count, step: 1);
            loop.Evaluate(mode: loopMode);

            // return
            return vs;
        }

        /// <summary>
        /// evaluates values on a set of x/y-separable scattered locations
        /// </summary>
        /// <param name="xy"> set of x/y-separable scattered locations for evaluation </param>
        /// <param name="loopMode"> computational option for loops </param>
        /// <returns> interpolated values on those evaluation locations </returns>
        public MatrixD Evaluate(ScatInfoXY xy,
            LoopMode loopMode = Defaults.LoopOption)
        {
            // initialize
            MatrixD vs = new(rows: xy.Rows, cols: xy.Cols);
            // defines loop operation
            Action<long, long> a = (iRow, iCol) =>
            {
                (double y, double x) = xy[iRow, iCol, false];
                vs[iRow, iCol, false] = FindValue(arg1: x, arg2: y);
            };
            Loop2D loop = new(operation: a,
                rowStart: 0, rowEnd: xy.Rows, 
                colStart: 0, colEnd: xy.Cols,
                rowStep: 1, colStep: 1);
            loop.Evaluate(mode: loopMode);

            // return
            return vs;
        }

        /// <summary>
        /// evaluates values on a target grid
        /// </summary>
        /// <param name="targetGrid"> target grid for evaluation </param>
        /// <param name="loopMode"> computational option for loops </param>
        /// <returns> interpolated values on the target grid </returns>
        public MatrixD Evaluate(GridInfo2D targetGrid,
            LoopMode loopMode = Defaults.LoopOption)
            => Evaluate(xy: (ScatInfoXY)targetGrid, loopMode: loopMode);

        #endregion
    }

    /// <summary>
    /// interpolation for 2D real-valued grid data
    /// </summary>
    public class Grid2DCplxInterpolation
    {
        #region properties

        /// <summary>
        /// pointwise evaluation function
        /// <para> variable #1: location x for the evaluation </para>
        /// <para> variable #2: location y for the evaluation </para>
        /// <para> result: interpolated value </para>
        /// </summary>
        public Func<double, double, Complex> FindValue { get; set; }

        /// <summary>
        /// input data values
        /// </summary>
        public MatrixZ Values { get; set; }

        /// <summary>
        /// sampling grid for the input data
        /// </summary>
        public GridInfo2D Grid { get; set; }

        /// <summary>
        /// data boundary option along x : periodic or zero
        /// </summary>
        public DataBoundary BoundaryX { get; set; }

        /// <summary>
        /// data boundary option along y : periodic or zero
        /// </summary>
        public DataBoundary BoundaryY { get; set; }

        /// <summary>
        /// interpolation method option
        /// <para> Nearest: nearest-neighbor interpolation </para>
        /// <para> Linear: linear interpolation </para>
        /// <para> Cubic: cubic interpolation </para>
        /// </summary>
        public InterpolationMethod Method { get; set; }

        /// <summary>
        /// internally stored shrinked grid for interpolation
        /// [only used for linear and cubic methods]
        /// </summary>
        private GridInfo2D? ShrinkedGrid { get; set; }

        /// <summary>
        /// internally stored finite difference [Dx] values
        /// [only used for cubic method]
        /// </summary>
        private MatrixZ? Dx { get; set; }

        /// <summary>
        /// internally stored finite difference [Dy] values
        /// [only used for cubic method]
        /// </summary>
        private MatrixZ? Dy { get; set; }

        /// <summary>
        /// internally stored finite difference [Dxy] values
        /// [only used for cubic method]
        /// </summary>
        private MatrixZ? Dxy { get; set; }

        #endregion
        #region constructor

        /// <summary>
        /// constructs an interpolation class
        /// for 2D real-valued grid data
        /// </summary>
        /// <param name="v"> input data </param>
        /// <param name="grid"> sampling grid for the input data </param>
        /// <param name="method"> selected interpolation method </param>
        /// <param name="boundX"> data boundary option along x-axis </param>
        /// <param name="boundY"> data boundary option along y-axis </param>
        public Grid2DCplxInterpolation(MatrixZ v, GridInfo2D grid,
            InterpolationMethod method = Defaults.IntrplOption,
            DataBoundary boundX = Defaults.BoundaryOption,
            DataBoundary boundY = Defaults.BoundaryOption)
        {
            // sets input data
            Values = v;
            Grid = grid;
            BoundaryX = boundX;
            BoundaryY = boundY;
            Method = method;
            // shrinked grid?
            if (Method == InterpolationMethod.Linear || Method == InterpolationMethod.Cubic)
            {
                ShrinkedGrid = new(rows: Grid.Rows - 1, cols: Grid.Cols - 1,
                    spacingY: Grid.SpacingY, spacingX: Grid.SpacingX,
                    startY: Grid.StartY + 0.5 * Grid.SpacingY, startX: Grid.StartX + 0.5 * Grid.SpacingX);
            }
            // derivatives
            if (Method == InterpolationMethod.Cubic)
            {
                Grid2DCplxFiDi Fx = new(vs: Values, grid: null, option: FiDi2DOption.Dx);
                Grid2DCplxFiDi Fy = new(vs: Values, grid: null, option: FiDi2DOption.Dy);
                Grid2DCplxFiDi Fxy = new(vs: Values, grid: null, option: FiDi2DOption.Dxy);
                Dx = Fx.EvaluateAll();
                Dy = Fy.EvaluateAll();
                Dxy = Fxy.EvaluateAll();
            }
            // defines FindValue
            switch (Method)
            {
                case InterpolationMethod.Sinc:
                    FindValue = (x, y) => PointInterpolation.Sinc(Values, Grid,
                        x, y, BoundaryX, BoundaryY);
                    break;
                case InterpolationMethod.SincFFT:
                    throw new NotImplementedException();
                case InterpolationMethod.Nearest:
                    FindValue = (x, y) => PointInterpolation.Nearest(Values, Grid,
                        x, y, BoundaryX, BoundaryY);
                    break;
                case InterpolationMethod.Linear:
                    FindValue = (x, y) => PointInterpolation.Linear(Values, Grid, ShrinkedGrid!,
                        x, y, BoundaryX, BoundaryY);
                    break;
                case InterpolationMethod.Cubic:
                    FindValue = (x, y) => PointInterpolation.Cubic(Values, Grid, ShrinkedGrid!, Dx!, Dy!, Dxy!,
                        x, y, BoundaryX, BoundaryY);
                    break;
                default: goto case InterpolationMethod.Nearest;
            };

        }

        #endregion
        #region methods

        /// <summary>
        /// evaluates values on a set of scattered locations
        /// </summary>
        /// <param name="rho"> set of scattered locations for evaluation </param>
        /// <param name="loopMode"> computational option for loops </param>
        /// <returns> interpolated values on those evaluation locations </returns>
        public VectorZ Evaluate(ScatInfo2D rho,
            LoopMode loopMode = Defaults.LoopOption)
        {
            // initialize
            VectorZ vs = new(count: rho.Count);
            // defines loop operation
            Action<long> a = (i) =>
            {
                (double y, double x) = rho[i, false];
                vs[i, false] = FindValue(arg1: x, arg2: y);
            };
            Loop1D loop = new(operation: a, 
                start: 0, end: rho.Count, step: 1);
            loop.Evaluate(mode: loopMode);

            // return
            return vs;
        }

        /// <summary>
        /// evaluates values on a set of x/y-separable scattered locations
        /// </summary>
        /// <param name="xy"> set of x/y-separable scattered locations for evaluation </param>
        /// <param name="loopMode"> computational option for loops </param>
        /// <returns> interpolated values on those evaluation locations </returns>
        public MatrixZ Evaluate(ScatInfoXY xy,
            LoopMode loopMode = Defaults.LoopOption)
        {
            // initialize
            MatrixZ vs = new(rows: xy.Rows, cols: xy.Cols); 
            // defines loop operation
            Action<long, long> a = (iRow, iCol) =>
            {
                (double y, double x) = xy[iRow, iCol, false];
                vs[iRow, iCol, false] = FindValue(arg1: x, arg2: y);
            };
            Loop2D loop = new(operation: a,
                rowStart: 0, rowEnd: xy.Rows, 
                colStart: 0, colEnd: xy.Cols,
                rowStep: 1, colStep: 1);
            loop.Evaluate(mode: loopMode);

            // return
            return vs;

        }

        /// <summary>
        /// evaluates values on a target grid
        /// </summary>
        /// <param name="targetGrid"> target grid for evaluation </param>
        /// <param name="loopMode"> computational option for loops </param>
        /// <returns> interpolated values on the target grid </returns>
        public MatrixZ Evaluate(GridInfo2D targetGrid,
            LoopMode loopMode = Defaults.LoopOption)
            => Evaluate(xy: (ScatInfoXY)targetGrid, loopMode: loopMode);

        #endregion
    }

}
