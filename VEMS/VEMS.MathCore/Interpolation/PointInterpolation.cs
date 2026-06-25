using Complex = System.Numerics.Complex;

namespace VEMS.MathCore
{
    /// <summary>
    /// point-wise interpolation
    /// </summary>
    internal class PointInterpolation
    {
        #region ------- Sinc -------

        #region ===== aux function =====

        /// <summary>
        /// sinc function used for sinc interpolation
        /// </summary>
        /// <param name="x"> variable </param>
        /// <returns> function value </returns>
        private static double Sinc(double x)
        {
            if (x == 0.0)
            { return 1.0; }
            return Math.Sin(Math.PI * x) / (Math.PI * x);
        }

        // vectorized version?

        #endregion
        #region ===== 1D case =====

        /// <summary>
        /// finds the value f(x) for given variable x
        /// by sinc interpolation
        /// </summary>
        /// <param name="v"> uniformly sampled input values </param>
        /// <param name="grid"> sampling grid for the input values </param>
        /// <param name="x"> arbitrary variable x </param>
        /// <param name="bound"> data boundary option </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> interpolated value f(x) </returns>
        public static double Sinc(VectorD v, GridInfo1D grid,
            double x,
            DataBoundary bound = Defaults.BoundaryOption,
            LoopMode loopMode = Defaults.LoopOption)
        {
            // copies coordinate x
            double cx = x;
            // finds grid span
            bool p = bound switch { DataBoundary.Periodic => true, _ => false };
            _ = grid.FindGridSpan(ref cx, periodic: p);

            // initializes output value
            double res = 0.0;
            // defines loop operation
            Action<long> a = (i) =>
            {
                double xi = grid[i];
                double dx = cx - xi;
                res += v[i, false] * Sinc(dx / grid.Spacing);
            };
            Loop1D loop = new(operation: a,
                start: 0, end: v.Count, step: 1);
            loop.Evaluate(mode: loopMode);

            // return
            return res;
        }

        /// <summary>
        /// finds the value f(x) for given variable x
        /// by sinc interpolation
        /// </summary>
        /// <param name="v"> uniformly sampled input values </param>
        /// <param name="grid"> sampling grid for the input values </param>
        /// <param name="x"> arbitrary variable x </param>
        /// <param name="bound"> data boundary option </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> interpolated value f(x) </returns>
        public static Complex Sinc(VectorZ v, GridInfo1D grid,
            double x,
            DataBoundary bound = Defaults.BoundaryOption,
            LoopMode loopMode = Defaults.LoopOption)
        {
            // copies coordinate x
            double cx = x;
            // finds grid span
            bool p = bound switch { DataBoundary.Periodic => true, _ => false };
            _ = grid.FindGridSpan(ref cx, periodic: p);

            // initializes output value
            Complex res = Complex.Zero;
            // defines loop operation
            Action<long> a = (i) =>
            {
                double xi = grid[i];
                double dx = cx - xi;
                res += v[i, false] * Sinc(dx / grid.Spacing);
            };
            Loop1D loop = new(operation: a,
                start: 0, end: v.Count, step: 1);
            loop.Evaluate(mode: loopMode);

            // return
            return res;
        }

        #endregion
        #region ===== 2D case =====

        /// <summary>
        /// finds the value f(x, y) for given 
        /// x and y, by sinc interpolation
        /// </summary>
        /// <param name="v"> uniformly sampled input values </param>
        /// <param name="grid"> sampling grid for the input values </param>
        /// <param name="x"> arbitrary variable x </param>
        /// <param name="y"> arbitrary variable y </param>
        /// <param name="boundX"> data boundary option along x </param>
        /// <param name="boundY"> data boundary option along y </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> interpolated value f(x, y) </returns>
        public static double Sinc(MatrixD v, GridInfo2D grid,
            double x, double y,
            DataBoundary boundX = Defaults.BoundaryOption, 
            DataBoundary boundY = Defaults.BoundaryOption,
            LoopMode loopMode = Defaults.LoopOption)
        {
            // copies coordinates
            double cx = x;
            double cy = y;
            // finds grid span
            bool px = boundX switch { DataBoundary.Periodic => true, _ => false };
            bool py = boundY switch { DataBoundary.Periodic => true, _ => false };
            _ = grid.FindGridSpan(ref cy, ref cx, periodicY: py, periodicX: px);

            // initializes output value
            double res = 0.0;
            // defines loop operation
            Action<long, long> a = (iRow, iCol) =>
            {
                (double yi, double xi) = grid[iRow, iCol];
                double dx = cx - xi;
                double dy = cy - yi;
                res += v[iRow, iCol, false] * Sinc(dx / grid.SpacingX) * Sinc(dy / grid.SpacingY);
            };
            Loop2D loop = new(operation: a,
                rowStart: 0, rowEnd: grid.Rows,
                colStart: 0, colEnd: grid.Cols,
                rowStep: 1, colStep: 1);
            loop.Evaluate(mode: loopMode);

            // returns 
            return res;
        }

        /// <summary>
        /// finds the value f(x, y) for given 
        /// x and y, by sinc interpolation
        /// </summary>
        /// <param name="v"> uniformly sampled input values </param>
        /// <param name="grid"> sampling grid for the input values </param>
        /// <param name="x"> arbitrary variable x </param>
        /// <param name="y"> arbitrary variable y </param>
        /// <param name="boundX"> data boundary option along x </param>
        /// <param name="boundY"> data boundary option along y </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> interpolated value f(x, y) </returns>
        public static Complex Sinc(MatrixZ v, GridInfo2D grid,
            double x, double y,
            DataBoundary boundX = Defaults.BoundaryOption,
            DataBoundary boundY = Defaults.BoundaryOption,
            LoopMode loopMode = Defaults.LoopOption)
        {
            // copies coordinates
            double cx = x;
            double cy = y;
            // finds grid span
            bool px = boundX switch { DataBoundary.Periodic => true, _ => false };
            bool py = boundY switch { DataBoundary.Periodic => true, _ => false };
            _ = grid.FindGridSpan(ref cy, ref cx, periodicY: py, periodicX: px);

            // initializes output value
            Complex res = Complex.Zero;
            // defines loop operation
            Action<long, long> a = (iRow, iCol) =>
            {
                (double yi, double xi) = grid[iRow, iCol];
                double dx = cx - xi;
                double dy = cy - yi;
                res += v[iRow, iCol, false] * Sinc(dx / grid.SpacingX) * Sinc(dy / grid.SpacingY);
            };
            Loop2D loop = new(operation: a,
                rowStart: 0, rowEnd: grid.Rows,
                colStart: 0, colEnd: grid.Cols,
                rowStep: 1, colStep: 1);
            loop.Evaluate(mode: loopMode);

            // returns 
            return res;
        }

        #endregion

        #endregion
        #region ------- Nearest -------

        #region ===== 1D case =====

        /// <summary>
        /// finds the value f(x) for given variable x
        /// by nearest-neighbor interpolation
        /// </summary>
        /// <param name="v"> uniformly sampled input values </param>
        /// <param name="grid"> sampling grid for the input values </param>
        /// <param name="x"> arbitrary variable x </param>
        /// <param name="bound"> data boundary option </param>
        /// <returns> interpolated value f(x) </returns>
        public static double Nearest(VectorD v, GridInfo1D grid,
            double x,
            DataBoundary bound = Defaults.BoundaryOption)
        {
            // copies coordinate x
            double cx = x;
            // finds grid span
            bool p = bound switch { DataBoundary.Periodic => true, _ => false };
            long j = grid.FindGridSpan(ref cx, periodic: p);
            // returns value
            if (j == -1) { return 0.0; } // simple zero or boundary continuation?
            else { return v[j, false]; }
        }

        /// <summary>
        /// finds the value f(x) for given variable x
        /// by nearest-neighbor interpolation
        /// </summary>
        /// <param name="v"> uniformly sampled input values </param>
        /// <param name="grid"> sampling grid for the input values </param>
        /// <param name="x"> arbitrary variable x </param>
        /// <param name="bound"> data boundary option </param>
        /// <returns> interpolated value f(x) </returns>
        public static Complex Nearest(VectorZ v, GridInfo1D grid,
            double x,
            DataBoundary bound = Defaults.BoundaryOption)
        {
            // copies coordinate x
            double cx = x;
            // finds grid span
            bool p = bound switch { DataBoundary.Periodic => true, _ => false };
            long j = grid.FindGridSpan(ref cx, periodic: p);
            // returns value
            if (j == -1) { return Complex.Zero; } // simple zero or boundary continuation?
            else { return v[j, false]; }
        }

        #endregion
        #region ===== 2D case =====

        /// <summary>
        /// finds the value f(x, y) for given 
        /// x and y, by nearest-neighbor interpolation
        /// </summary>
        /// <param name="v"> uniformly sampled input values </param>
        /// <param name="grid"> sampling grid for the input values </param>
        /// <param name="x"> arbitrary variable x </param>
        /// <param name="y"> arbitrary variable y </param>
        /// <param name="boundX"> data boundary option along x </param>
        /// <param name="boundY"> data boundary option along y </param>
        /// <returns> interpolated value f(x, y) </returns>
        public static double Nearest(MatrixD v, GridInfo2D grid,
            double x, double y,
            DataBoundary boundX = Defaults.BoundaryOption,
            DataBoundary boundY = Defaults.BoundaryOption)
        {
            // copies coordinates
            double cx = x;
            double cy = y;
            // finds grid span
            bool px = boundX switch { DataBoundary.Periodic => true, _ => false };
            bool py = boundY switch { DataBoundary.Periodic => true, _ => false };
            (long iRow, long iCol) = grid.FindGridSpan(ref cy, ref cx, periodicY: py, periodicX: px);

            // returns value
            if (iRow == -1 || iCol == -1) { return 0.0; } // simple zero or boundary continuation?
            else { return v[iRow, iCol, false]; }
        }

        /// <summary>
        /// finds the value f(x, y) for given 
        /// x and y, by nearest-neighbor interpolation
        /// </summary>
        /// <param name="v"> uniformly sampled input values </param>
        /// <param name="grid"> sampling grid for the input values </param>
        /// <param name="x"> arbitrary variable x </param>
        /// <param name="y"> arbitrary variable y </param>
        /// <param name="boundX"> data boundary option along x </param>
        /// <param name="boundY"> data boundary option along y </param>
        /// <returns> interpolated value f(x, y) </returns>
        public static Complex Nearest(MatrixZ v, GridInfo2D grid,
            double x, double y,
            DataBoundary boundX = Defaults.BoundaryOption,
            DataBoundary boundY = Defaults.BoundaryOption)
        {
            // copies coordinates
            double cx = x;
            double cy = y;
            // finds grid span
            bool px = boundX switch { DataBoundary.Periodic => true, _ => false };
            bool py = boundY switch { DataBoundary.Periodic => true, _ => false };
            (long iRow, long iCol) = grid.FindGridSpan(ref cy, ref cx, periodicY: py, periodicX: px);

            // returns value
            if (iRow == -1 || iCol == -1) { return Complex.Zero; } // simple zero or boundary continuation?
            else { return v[iRow, iCol, false]; }
        }

        #endregion

        #endregion
        #region ------- Linear -------

        #region ===== 1D case =====

        /// <summary>
        /// finds the value f(x) for given variable x
        /// by linear interpolation
        /// </summary>
        /// <param name="v"> uniformly sampled input values </param>
        /// <param name="grid"> sampling grid for the input values </param>
        /// <param name="shrinkedGrid"> shrinked grid for span finding </param>
        /// <param name="x"> target location x </param>
        /// <param name="bound"> data boundary option </param>
        /// <returns> interpolated value y = f(x) </returns>
        internal static double Linear(VectorD v, GridInfo1D grid,
            GridInfo1D shrinkedGrid,
            double x,
            DataBoundary bound = Defaults.BoundaryOption)
        {
            // copies coordinate x
            double cx = x;
            // finds span with help of the shrinked grid
            bool p = bound switch { DataBoundary.Periodic => true, _ => false };
            long j = shrinkedGrid.FindGridSpan(ref cx, periodic: p);
            if (j == -1) { return 0.0; } // simple zero or boundary continuation?

            // local distance w.r.t. lower index
            double d = cx - grid[j];
            if (d == 0.0) // coincides
            { return v[j, false]; }
            else // applies linear interpolation
            {
                double t = d * v[j + 1, false]
                    + (grid.Spacing - d) * v[j, false];
                return t / grid.Spacing;
            }
        }

        /// <summary>
        /// finds the value f(x) for given variable x
        /// by linear interpolation
        /// </summary>
        /// <param name="v"> uniformly sampled input values </param>
        /// <param name="grid"> sampling grid for the input values </param>
        /// <param name="x"> target location x </param>
        /// <param name="bound"> data boundary option </param>
        /// <returns> interpolated value y = f(x) </returns>
        public static double Linear(VectorD v, GridInfo1D grid,
            double x,
            DataBoundary bound = Defaults.BoundaryOption)
        {
            // makes a new grid, where values are on the grid edges
            GridInfo1D g = new(n: grid.Count - 1, spacing: grid.Spacing,
                start: grid.Start + 0.5 * grid.Spacing);
            return Linear(v: v, grid: grid, shrinkedGrid: g, x: x, bound: bound);
        }

        /// <summary>
        /// finds the value f(x) for given variable x
        /// by linear interpolation
        /// </summary>
        /// <param name="v"> uniformly sampled input values </param>
        /// <param name="grid"> sampling grid for the input values </param>
        /// <param name="shrinkedGrid"> shrinked grid for span finding </param>
        /// <param name="x"> target location x </param>
        /// <param name="bound"> data boundary option </param>
        /// <returns> interpolated value y = f(x) </returns>
        internal static Complex Linear(VectorZ v, GridInfo1D grid,
            GridInfo1D shrinkedGrid,
            double x,
            DataBoundary bound = Defaults.BoundaryOption)
        {
            // copies coordinate x
            double cx = x;
            // finds span with help of the shrinked grid
            bool p = bound switch { DataBoundary.Periodic => true, _ => false };
            long j = shrinkedGrid.FindGridSpan(ref cx, periodic: p);
            if (j == -1) { return Complex.Zero; } // simple zero or boundary continuation?

            // local distance w.r.t. lower index
            double d = cx - grid[j];
            if (d == 0.0) // coincides
            { return v[j, false]; }
            else // applies linear interpolation
            {
                Complex t = d * v[j + 1, false]
                    + (grid.Spacing - d) * v[j, false];
                return t / grid.Spacing;
            }
        }

        /// <summary>
        /// finds the value f(x) for given variable x
        /// by linear interpolation
        /// </summary>
        /// <param name="v"> uniformly sampled input values </param>
        /// <param name="grid"> sampling grid for the input values </param>
        /// <param name="x"> target location x </param>
        /// <param name="bound"> data boundary option </param>
        /// <returns> interpolated value y = f(x) </returns>
        public static Complex Linear(VectorZ v, GridInfo1D grid,
            double x,
            DataBoundary bound = Defaults.BoundaryOption)
        {
            // makes a new grid, where values are on the grid edges
            GridInfo1D g = new(n: grid.Count - 1, spacing: grid.Spacing,
                start: grid.Start + 0.5 * grid.Spacing);
            return Linear(v: v, grid: grid, shrinkedGrid: g, x: x, bound: bound);
        }

        #endregion
        #region ===== 2D case =====

        /// <summary>
        /// finds the value f(x, y) for given 
        /// x and y, by linear interpolation
        /// </summary>
        /// <param name="v"> uniformly sampled input values </param>
        /// <param name="grid"> sampling grid for the input values </param>
        /// <param name="shrinkedGrid"> shrinked grid for span finding </param>
        /// <param name="x"> arbitrary variable x </param>
        /// <param name="y"> arbitrary variable y </param>
        /// <param name="boundX"> data boundary option along x </param>
        /// <param name="boundY"> data boundary option along y </param>
        /// <returns> interpolated value f(x, y) </returns>
        internal static double Linear(MatrixD v, GridInfo2D grid,
            GridInfo2D shrinkedGrid,
            double x, double y,
            DataBoundary boundX = Defaults.BoundaryOption,
            DataBoundary boundY = Defaults.BoundaryOption)
        {
            // copies coordinates
            double cx = x;
            double cy = y;

            // finds span with help of the shrinked grid
            bool px = boundX switch { DataBoundary.Periodic => true, _ => false };
            bool py = boundY switch { DataBoundary.Periodic => true, _ => false };
            (long iRow, long iCol) = shrinkedGrid.FindGridSpan(ref cy, ref cx, periodicY: py, periodicX: px);
            if(iRow == -1 || iCol == -1) { return 0.0; }

            // local distance w.r.t. left-lower corner
            double tdy = cy - grid.GetCoordinateY(iRow);
            double tdx = cx - grid.GetCoordinateX(iCol);

            if (tdx == 0.0 && tdy == 0.0) // coincide
            { return v[iRow, iCol, false]; }
            else if (tdx == 0.0) // => linear 1D
            {
                // only need to interpolate along y
                double t = tdy * v[iRow + 1, iCol, false]
                    + (grid.SpacingY - tdy) * v[iRow, iCol, false];
                return t / grid.SpacingY;
            }
            else if (tdy == 0.0) // => linear 1D
            {
                // only need to interpolate along x
                double t = tdx * v[iRow, iCol + 1, false]
                    + (grid.SpacingX - tdx) * v[iRow, iCol, false];
                return t / grid.SpacingX;
            }
            else
            {
                // need two interpolations along x 
                double t1 = tdx * v[iRow, iCol + 1, false]
                    + (grid.SpacingX - tdx) * v[iRow, iCol, false];
                t1 /= grid.SpacingX;
                double t2 = tdx * v[iRow + 1, iCol + 1, false]
                    + (grid.SpacingX - tdx) * v[iRow + 1, iCol, false];
                t2 /= grid.SpacingX;
                // and one interpolation along y
                double t = tdy * t2 + (grid.SpacingY - tdy) * t1;
                return t / grid.SpacingY;
            }
        }

        /// <summary>
        /// finds the value f(x, y) for given 
        /// x and y, by linear interpolation
        /// </summary>
        /// <param name="v"> uniformly sampled input values </param>
        /// <param name="grid"> sampling grid for the input values </param>
        /// <param name="x"> arbitrary variable x </param>
        /// <param name="y"> arbitrary variable y </param>
        /// <param name="boundX"> data boundary option along x </param>
        /// <param name="boundY"> data boundary option along y </param>
        /// <returns> interpolated value f(x, y) </returns>
        public static double Linear(MatrixD v, GridInfo2D grid,
            double x, double y,
            DataBoundary boundX = Defaults.BoundaryOption,
            DataBoundary boundY = Defaults.BoundaryOption)
        {
            // makes a new grid, where values are on the grid edges
            GridInfo2D g = new(rows: grid.Rows - 1, cols: grid.Cols - 1,
                spacingY: grid.SpacingY, spacingX: grid.SpacingX,
                startY: grid.StartY + 0.5 * grid.SpacingY, startX: grid.StartY + 0.5 * grid.SpacingX);
            return Linear(v: v, grid: grid, shrinkedGrid: g, x: x, y: y, 
                boundX: boundX, boundY: boundY);
        }

        /// <summary>
        /// finds the value f(x, y) for given 
        /// x and y, by linear interpolation
        /// </summary>
        /// <param name="v"> uniformly sampled input values </param>
        /// <param name="grid"> sampling grid for the input values </param>
        /// <param name="shrinkedGrid"> shrinked grid for span finding </param>
        /// <param name="x"> arbitrary variable x </param>
        /// <param name="y"> arbitrary variable y </param>
        /// <param name="boundX"> data boundary option along x </param>
        /// <param name="boundY"> data boundary option along y </param>
        /// <returns> interpolated value f(x, y) </returns>
        internal static Complex Linear(MatrixZ v, GridInfo2D grid,
            GridInfo2D shrinkedGrid,
            double x, double y,
            DataBoundary boundX = Defaults.BoundaryOption,
            DataBoundary boundY = Defaults.BoundaryOption)
        {
            // copies coordinates
            double cx = x;
            double cy = y;

            // finds span with help of the shrinked grid
            bool px = boundX switch { DataBoundary.Periodic => true, _ => false };
            bool py = boundY switch { DataBoundary.Periodic => true, _ => false };
            (long iRow, long iCol) = shrinkedGrid.FindGridSpan(ref cy, ref cx, periodicY: py, periodicX: px);
            if(iRow == -1 || iCol == -1) { return Complex.Zero; }

            // local distance w.r.t. left-lower corner
            double tdy = cy - grid.GetCoordinateY(iRow);
            double tdx = cx - grid.GetCoordinateX(iCol);

            if (tdx == 0.0 && tdy == 0.0) // coincide
            { return v[iRow, iCol, false]; }
            else if (tdx == 0.0) // => linear 1D
            {
                // only need to interpolate along y
                Complex t = tdy * v[iRow + 1, iCol, false]
                    + (grid.SpacingY - tdy) * v[iRow, iCol, false];
                return t / grid.SpacingY;
            }
            else if (tdy == 0.0) // => linear 1D
            {
                // only need to interpolate along x
                Complex t = tdx * v[iRow, iCol + 1, false]
                    + (grid.SpacingX - tdx) * v[iRow, iCol, false];
                return t / grid.SpacingX;
            }
            else
            {
                // need two interpolations along x 
                Complex t1 = tdx * v[iRow, iCol + 1, false]
                    + (grid.SpacingX - tdx) * v[iRow, iCol, false];
                t1 /= grid.SpacingX;
                Complex t2 = tdx * v[iRow + 1, iCol + 1, false]
                    + (grid.SpacingX - tdx) * v[iRow + 1, iCol, false];
                t2 /= grid.SpacingX;
                // and one interpolation along y
                Complex t = tdy * t2 + (grid.SpacingY - tdy) * t1;
                return t / grid.SpacingY;
            }
        }

        /// <summary>
        /// finds the value f(x, y) for given 
        /// x and y, by linear interpolation
        /// </summary>
        /// <param name="v"> uniformly sampled input values </param>
        /// <param name="grid"> sampling grid for the input values </param>
        /// <param name="x"> arbitrary variable x </param>
        /// <param name="y"> arbitrary variable y </param>
        /// <param name="boundX"> data boundary option along x </param>
        /// <param name="boundY"> data boundary option along y </param>
        /// <returns> interpolated value f(x, y) </returns>
        public static Complex Linear(MatrixZ v, GridInfo2D grid,
            double x, double y,
            DataBoundary boundX = Defaults.BoundaryOption,
            DataBoundary boundY = Defaults.BoundaryOption)
        {
            // makes a new grid, where values are on the grid edges
            GridInfo2D g = new(rows: grid.Rows - 1, cols: grid.Cols - 1,
                spacingY: grid.SpacingY, spacingX: grid.SpacingX,
                startY: grid.StartY + 0.5 * grid.SpacingY, startX: grid.StartY + 0.5 * grid.SpacingX);
            return Linear(v: v, grid: grid, shrinkedGrid: g, x: x, y: y,
                boundX: boundX, boundY: boundY);
        }

        #endregion
        
        #endregion
        #region ------- Cubic -------
        
        #region ===== aux parameters =====

        private static MatrixD ax = new(4, 4)
        {
            [0, 0] = 1.0,
            [1, 2] = 1.0,
            [2, 0] = -3.0,
            [2, 1] = 3.0,
            [2, 2] = -2.0,
            [2, 3] = -1.0,
            [3, 0] = 2.0,
            [3, 1] = -2.0,
            [3, 2] = 1.0,
            [3, 3] = 1.0
        };

        private static MatrixD ay = new(4, 4)
        {
            [0, 0] = 1.0,
            [0, 2] = -3.0,
            [0, 3] = 2.0,
            [1, 2] = 3.0,
            [1, 3] = -2.0,
            [2, 1] = 1.0,
            [2, 2] = -2.0,
            [2, 3] = 1.0,
            [3, 2] = -1.0,
            [3, 3] = 1.0
        };

        #endregion
        #region ===== 1D case =====

        /// <summary>
        /// finds the value f(x) for given variable x
        /// by cubic interpolation
        /// </summary>
        /// <param name="v"> uniformly sampled input values </param>
        /// <param name="grid"> sampling grid for the input values </param>
        /// <param name="shrinkedGrid"> shrinked grid for span finding </param>
        /// <param name="fx"> finite-difference [fx] for null grid </param>
        /// <param name="x"> target location x </param>
        /// <param name="bound"> data boundary option </param>
        /// <returns> interpolated value y = f(x) </returns>
        internal static double Cubic(VectorD v, GridInfo1D grid,
            GridInfo1D shrinkedGrid, VectorD fx,
            double x,
            DataBoundary bound = Defaults.BoundaryOption)
        {
            // copies coordinate x
            double cx = x;
            // finds span with help of the shrinked grid
            bool p = bound switch { DataBoundary.Periodic => true, _ => false };
            long j = shrinkedGrid.FindGridSpan(ref cx, periodic: p);
            if (j == -1) { return 0.0; } 

            // local distance w.r.t. lower index
            double xp = cx - grid.GetCoordinate(j);
            if (xp == 0.0) // coincide
            { return v[j, false]; }
            else // apply cubic interpolation
            {
                // initial values
                VectorD f = new(4)
                {
                    [0] = v[j, false],
                    [1] = v[j + 1, false],
                    [2] = fx[j, false], // for [0, 1] unit range
                    [3] = fx[j + 1, false] // for [0, 1] unit range
                };
                // computes coefficients
                VectorD a = LinAlg.Dot(ax, f);
                // evaluates value
                xp /= grid.Spacing;
                VectorD xs = new(4)
                {
                    [0] = 1.0,
                    [1] = xp,
                    [2] = Math.Pow(xp, 2.0),
                    [3] = Math.Pow(xp, 3.0)
                };
                return LinAlg.Dot(a, xs);
            }
        }

        /// <summary>
        /// finds the value f(x) for given variable x
        /// by cubic interpolation
        /// </summary>
        /// <param name="v"> uniformly sampled input values </param>
        /// <param name="grid"> sampling grid for the input values </param>
        /// <param name="x"> target location x </param>
        /// <param name="bound"> data boundary option </param>
        /// <returns> interpolated value y = f(x) </returns>
        public static double Cubic(VectorD v, GridInfo1D grid,
            double x,
            DataBoundary bound = Defaults.BoundaryOption)
        {
            // makes a new grid, where values are on the grid edges
            GridInfo1D g = new(n: grid.Count - 1, spacing: grid.Spacing,
                start: grid.Start + 0.5 * grid.Spacing);
            // prepares finite difference - fx
            VectorD fx = new Grid1DRealFiDi(vs: v, grid: null, option: FiDi1DOption.Dt).EvaluateAll();
            // calls and return
            return Cubic(v: v, grid: grid, shrinkedGrid: g, fx: fx, 
                x: x, bound: bound);
        }

        /// <summary>
        /// finds the value f(x) for given variable x
        /// by cubic interpolation
        /// </summary>
        /// <param name="v"> uniformly sampled input values </param>
        /// <param name="grid"> sampling grid for the input values </param>
        /// <param name="shrinkedGrid"> shrinked grid for span finding </param>
        /// <param name="fx"> finite-difference [fx] for null grid </param>
        /// <param name="x"> target location x </param>
        /// <param name="bound"> data boundary option </param>
        /// <returns> interpolated value y = f(x) </returns>
        internal static Complex Cubic(VectorZ v, GridInfo1D grid,
            GridInfo1D shrinkedGrid, VectorZ fx,
            double x,
            DataBoundary bound = Defaults.BoundaryOption)
        {
            // copies coordinate x
            double cx = x;
            // finds span with help of the shrinked grid
            bool p = bound switch { DataBoundary.Periodic => true, _ => false };
            long j = shrinkedGrid.FindGridSpan(ref cx, periodic: p);
            if (j == -1) { return Complex.Zero; }

            // local distance w.r.t. lower index
            double xp = cx - grid.GetCoordinate(j);
            if (xp == 0.0) // coincide
            { return v[j, false]; }
            else // apply cubic interpolation
            {
                // initial values
                VectorZ f = new(4)
                {
                    [0] = v[j, false],
                    [1] = v[j + 1, false],
                    [2] = fx[j, false], // for [0, 1] unit range
                    [3] = fx[j + 1, false] // for [0, 1] unit range
                };
                // computes coefficients
                VectorZ a = LinAlg.Dot((MatrixZ)ax, f);
                // evaluates value
                xp /= grid.Spacing;
                VectorD xs = new(4)
                {
                    [0] = 1.0,
                    [1] = xp,
                    [2] = Math.Pow(xp, 2.0),
                    [3] = Math.Pow(xp, 3.0)
                };
                return LinAlg.Dot(a, (VectorZ)xs);
            }
        }

        /// <summary>
        /// finds the value f(x) for given variable x
        /// by cubic interpolation
        /// </summary>
        /// <param name="v"> uniformly sampled input values </param>
        /// <param name="grid"> sampling grid for the input values </param>
        /// <param name="x"> target location x </param>
        /// <param name="bound"> data boundary option </param>
        /// <returns> interpolated value y = f(x) </returns>
        public static Complex Cubic(VectorZ v, GridInfo1D grid,
            double x,
            DataBoundary bound = Defaults.BoundaryOption)
        {
            //makes a new grid, where values are on the grid edges
            GridInfo1D g = new(n: grid.Count - 1, spacing: grid.Spacing,
                start: grid.Start + 0.5 * grid.Spacing);
            // prepares finite difference - fx
            VectorZ fx = new Grid1DCplxFiDi(vs: v, grid: null, option: FiDi1DOption.Dt).EvaluateAll();
            // calls and return
            return Cubic(v: v, grid: grid, shrinkedGrid: g, fx: fx,
                x: x, bound: bound);
        }

        #endregion
        #region ===== 2D case =====

        /// <summary>
        /// finds the value f(x, y) for given 
        /// variables (x, y) by cubic interpolation
        /// !!! this is a very slow method, needs improvement !!!
        /// </summary>
        /// <param name="v"> uniformly sampled input values </param>
        /// <param name="grid"> sampling grid for the input values </param>
        /// <param name="shrinkedGrid"> shrinked grid for span finding </param>
        /// <param name="fx"> finite difference [fx] </param>
        /// <param name="fy"> finite difference [fy] </param>
        /// <param name="fxy"> finite difference [fxy] </param>
        /// <param name="x"> arbitrary variable x </param>
        /// <param name="y"> arbitrary variable y </param>
        /// <param name="boundX"> data boundary option along x </param>
        /// <param name="boundY"> data boundary option along y </param>
        /// <returns> interpolated value f(x, y) </returns>
        internal static double Cubic(MatrixD v, GridInfo2D grid,
            GridInfo2D shrinkedGrid, MatrixD fx, MatrixD fy, MatrixD fxy,
            double x, double y,
            DataBoundary boundX = Defaults.BoundaryOption,
            DataBoundary boundY = Defaults.BoundaryOption)
        {
            // copies coordinates
            double cx = x;
            double cy = y;

            // finds span with help of the shrinked grid
            bool px = boundX switch { DataBoundary.Periodic => true, _ => false };
            bool py = boundY switch { DataBoundary.Periodic => true, _ => false };
            (long iRow, long iCol) = shrinkedGrid.FindGridSpan(ref cy, ref cx, periodicY: py, periodicX: px);
            if (iRow == -1 || iCol == -1) { return 0.0; }

            // local distance w.r.t. lower index
            double yp = cy - grid.GetCoordinateY(iRow);
            double xp = cx - grid.GetCoordinateX(iCol);

            if (xp == 0.0 && yp == 0.0) // coincide
            { return v[iRow, iCol, false]; }
            else if (xp == 0.0) // => cubic 1D
            {
                // only need to interpolate along y
                VectorD f = new(4)
                {
                    [0] = v[iRow, iCol, false],
                    [1] = v[iRow + 1, iCol, false],
                    [2] = fy[iRow, iCol, false], // fy.FindValue(j), // fy[0]
                    [3] = fy[iRow + 1, iCol, false] // fy.FindValue(j + 1) // fy[1]
                };
                // computes coefficients
                VectorD a = LinAlg.Dot(ax, f); // ax or ay???
                // evaluates value
                yp /= grid.SpacingY;
                VectorD ys = new(4)
                {
                    [0] = 1.0,
                    [1] = yp,
                    [2] = Math.Pow(yp, 2.0),
                    [3] = Math.Pow(yp, 3.0)
                };
                return LinAlg.Dot(a, ys);
            }
            else if (yp == 0.0) // => cubic 1D
            {
                // only need to interpolate along x
                VectorD f = new(4)
                {
                    [0] = v[iRow, iCol, false],
                    [1] = v[iRow, iCol + 1, false],
                    [2] = fx[iRow, iCol, false], // fx.FindValue(j), // fx[0]
                    [3] = fx[iRow, iCol + 1, false] // fx.FindValue(j + 1) // fx[1]
                };
                // computes coefficients
                VectorD a = LinAlg.Dot(ax, f);
                // evaluates value
                xp /= grid.SpacingX;
                VectorD xs = new(4)
                {
                    [0] = 1.0,
                    [1] = xp,
                    [2] = Math.Pow(xp, 2.0),
                    [3] = Math.Pow(xp, 3.0)
                };
                return LinAlg.Dot(a, xs);
            }
            else // apply bi-cubic interpolation
            {
                // initial values
                MatrixD f = new(4, 4)
                {
                    [0, 0] = v[iRow: iRow + 0, iCol: iCol + 0, false], // f[0,0]
                    [0, 1] = v[iRow: iRow + 1, iCol: iCol + 0, false], // f[0,1]
                    [0, 2] = fy[iRow: iRow + 0, iCol: iCol + 1, false], //fy.FindValue(iRow + 0, iCol + 0), // fy[0,0]
                    [0, 3] = fy[iRow: iRow + 1, iCol: iCol + 0, false], //fy.FindValue(iRow + 1, iCol + 0), // fy[0,1]
                    [1, 0] = v[iRow: iRow + 0, iCol: iCol + 1, false], // f[1,0]
                    [1, 1] = v[iRow: iRow + 1, iCol: iCol + 1, false], // f[1,1]
                    [1, 2] = fy[iRow: iRow + 0, iCol: iCol + 1, false], //fy.FindValue(iRow + 0, iCol + 1), // fy[1,0]
                    [1, 3] = fy[iRow: iRow + 1, iCol: iCol + 1, false], //fy.FindValue(iRow + 1, iCol + 1), // fy[1,1]
                    [2, 0] = fx[iRow: iRow + 0, iCol: iCol + 0, false], //fx.FindValue(iRow + 0, iCol + 0), // fx[0,0]
                    [2, 1] = fx[iRow: iRow + 1, iCol: iCol + 0, false], //fx.FindValue(iRow + 1, iCol + 0), // fx[0,1]
                    [2, 2] = fxy[iRow: iRow + 0, iCol: iCol + 0, false], //fxy.FindValue(iRow + 0, iCol + 0), // fxy[0,0]
                    [2, 3] = fxy[iRow: iRow + 1, iCol: iCol + 0, false], //fxy.FindValue(iRow + 1, iCol + 0), // fxy[0,1]
                    [3, 0] = fx[iRow: iRow + 0, iCol: iCol + 1, false], //fx.FindValue(iRow + 0, iCol + 1), // fx[1,0]
                    [3, 1] = fx[iRow: iRow + 1, iCol: iCol + 1, false], //fx.FindValue(iRow + 1, iCol + 1), // fx[1,1]
                    [3, 2] = fxy[iRow: iRow + 0, iCol: iCol + 1, false], //fxy.FindValue(iRow + 0, iCol + 1), // fxy[1,0]
                    [3, 3] = fxy[iRow: iRow + 1, iCol: iCol + 1, false] //fxy.FindValue(iRow + 1, iCol + 1) // fxy[1,1]
                };
                // computes coefficients
                MatrixD a = LinAlg.Dot(LinAlg.Dot(ax, f), ay);
                // evaluates value
                xp /= grid.SpacingX;
                yp /= grid.SpacingY;
                VectorD xs = new(4)
                {
                    [0] = 1.0,
                    [1] = xp,
                    [2] = Math.Pow(xp, 2.0),
                    [3] = Math.Pow(xp, 3.0)
                };
                VectorD ys = new(4)
                {
                    [0] = 1.0,
                    [1] = yp,
                    [2] = Math.Pow(yp, 2.0),
                    [3] = Math.Pow(yp, 3.0)
                };
                return LinAlg.Dot(xs, LinAlg.Dot(a, ys));
            }
        }

        /// <summary>
        /// finds the value f(x, y) for given 
        /// variables (x, y) by cubic interpolation
        /// </summary>
        /// <param name="v"> uniformly sampled input values </param>
        /// <param name="grid"> sampling grid for the input values </param>
        /// <param name="x"> arbitrary variable x </param>
        /// <param name="y"> arbitrary variable y </param>
        /// <param name="boundX"> data boundary option along x </param>
        /// <param name="boundY"> data boundary option along y </param>
        /// <returns> interpolated value f(x, y) </returns>
        public static double Cubic(MatrixD v, GridInfo2D grid,
            double x, double y,
            DataBoundary boundX = Defaults.BoundaryOption,
            DataBoundary boundY = Defaults.BoundaryOption)
        {
            // makes a new grid, where values are on the grid edges
            GridInfo2D g = new(rows: grid.Rows - 1, cols: grid.Cols - 1,
                spacingY: grid.SpacingY, spacingX: grid.SpacingX,
                startY: grid.StartY + 0.5 * grid.SpacingY, startX: grid.StartX + 0.5 * grid.SpacingX);
            // prepares finite difference - fx, fy, fxy
            MatrixD fx = new Grid2DRealFiDi(vs: v, grid: null, option: FiDi2DOption.Dx).EvaluateAll();
            MatrixD fy = new Grid2DRealFiDi(vs: v, grid: null, option: FiDi2DOption.Dy).EvaluateAll();
            MatrixD fxy = new Grid2DRealFiDi(vs: v, grid: null, option: FiDi2DOption.Dxy).EvaluateAll();
            // calls and return
            return Cubic(v: v, grid: grid, shrinkedGrid: g, fx: fx, fy: fy, fxy: fxy,
                x: x, y: y, boundX: boundX, boundY: boundY);
        }

        /// <summary>
        /// finds the value f(x, y) for given 
        /// variables (x, y) by cubic interpolation
        /// !!! this is a very slow method, needs improvement !!!
        /// </summary>
        /// <param name="v"> uniformly sampled input values </param>
        /// <param name="grid"> sampling grid for the input values </param>
        /// <param name="shrinkedGrid"> shrinked grid for span finding </param>
        /// <param name="fx"> finite difference [fx] </param>
        /// <param name="fy"> finite difference [fy] </param>
        /// <param name="fxy"> finite difference [fxy] </param>
        /// <param name="x"> arbitrary variable x </param>
        /// <param name="y"> arbitrary variable y </param>
        /// <param name="boundX"> data boundary option along x </param>
        /// <param name="boundY"> data boundary option along y </param>
        /// <returns> interpolated value f(x, y) </returns>
        internal static Complex Cubic(MatrixZ v, GridInfo2D grid,
            GridInfo2D shrinkedGrid, MatrixZ fx, MatrixZ fy, MatrixZ fxy,
            double x, double y,
            DataBoundary boundX = Defaults.BoundaryOption,
            DataBoundary boundY = Defaults.BoundaryOption)
        {
            // copies coordinates
            double cx = x;
            double cy = y;

            // finds span with help of the shrinked grid
            bool px = boundX switch { DataBoundary.Periodic => true, _ => false };
            bool py = boundY switch { DataBoundary.Periodic => true, _ => false };
            (long iRow, long iCol) = shrinkedGrid.FindGridSpan(ref cy, ref cx, periodicY: py, periodicX: px);
            if (iRow == -1 || iCol == -1) { return Complex.Zero; }

            // local distance w.r.t. lower index
            double yp = cy - grid.GetCoordinateY(iRow);
            double xp = cx - grid.GetCoordinateX(iCol);
            if (xp == 0.0 && yp == 0.0) // coincide
            { return v[iRow, iCol, false]; }
            else if (xp == 0.0) // => cubic 1D
            {
                // only need to interpolate along y
                VectorZ f = new(4)
                {
                    [0] = v[iRow, iCol, false],
                    [1] = v[iRow + 1, iCol, false],
                    [2] = fy[iRow, iCol, false], // fy.FindValue(j), // fy[0]
                    [3] = fy[iRow + 1, iCol, false] // fy.FindValue(j + 1) // fy[1]
                };
                // computes coefficients
                VectorZ a = LinAlg.Dot((MatrixZ)ax, f); // ax or ay???
                // evaluates value
                yp /= grid.SpacingY;
                VectorD ys = new(4)
                {
                    [0] = 1.0,
                    [1] = yp,
                    [2] = Math.Pow(yp, 2.0),
                    [3] = Math.Pow(yp, 3.0)
                };
                return LinAlg.Dot(a, (VectorZ)ys);
            }
            else if (yp == 0.0) // => cubic 1D
            {
                // only need to interpolate along x
                VectorZ f = new(4)
                {
                    [0] = v[iRow, iCol, false],
                    [1] = v[iRow, iCol + 1, false],
                    [2] = fx[iRow, iCol, false], // fx.FindValue(j), // fx[0]
                    [3] = fx[iRow, iCol + 1, false] // fx.FindValue(j + 1) // fx[1]
                };
                // computes coefficients
                VectorZ a = LinAlg.Dot((MatrixZ)ax, f);
                // evaluates value
                xp /= grid.SpacingX;
                VectorD xs = new(4)
                {
                    [0] = 1.0,
                    [1] = xp,
                    [2] = Math.Pow(xp, 2.0),
                    [3] = Math.Pow(xp, 3.0)
                };
                return LinAlg.Dot(a, (VectorZ)xs);
            }
            else // apply bi-cubic interpolation
            {
                // initial values
                MatrixZ f = new(4, 4)
                {
                    [0, 0] = v[iRow: iRow + 0, iCol: iCol + 0, false], // f[0,0]
                    [0, 1] = v[iRow: iRow + 1, iCol: iCol + 0, false], // f[0,1]
                    [0, 2] = fy[iRow: iRow + 0, iCol: iCol + 0, false], // fy[0,0]
                    [0, 3] = fy[iRow: iRow + 1, iCol: iCol + 0, false], // fy[0,1]
                    [1, 0] = v[iRow: iRow + 0, iCol: iCol + 1, false], // f[1,0]
                    [1, 1] = v[iRow: iRow + 1, iCol: iCol + 1, false], // f[1,1]
                    [1, 2] = fy[iRow: iRow + 0, iCol: iCol + 1, false], // fy[1,0]
                    [1, 3] = fy[iRow: iRow + 1, iCol: iCol + 1, false], // fy[1,1]
                    [2, 0] = fx[iRow: iRow + 0, iCol: iCol + 0, false], // fx[0,0]
                    [2, 1] = fx[iRow: iRow + 1, iCol: iCol + 0, false], // fx[0,1]
                    [2, 2] = fxy[iRow: iRow + 0, iCol: iCol + 0, false], // fxy[0,0]
                    [2, 3] = fxy[iRow: iRow + 1, iCol: iCol + 0, false], // fxy[0,1]
                    [3, 0] = fx[iRow: iRow + 0, iCol: iCol + 1, false], // fx[1,0]
                    [3, 1] = fx[iRow: iRow + 1, iCol: iCol + 1, false], // fx[1,1]
                    [3, 2] = fxy[iRow: iRow + 0, iCol: iCol + 1, false], // fxy[1,0]
                    [3, 3] = fxy[iRow: iRow + 1, iCol: iCol + 1, false] // fxy[1,1]
                };
                // computes coefficients
                MatrixZ a = LinAlg.Dot(LinAlg.Dot((MatrixZ)ax, f), (MatrixZ)ay);
                // evaluates value
                xp /= grid.SpacingX;
                yp /= grid.SpacingY;
                VectorD xs = new(4)
                {
                    [0] = 1.0,
                    [1] = xp,
                    [2] = Math.Pow(xp, 2.0),
                    [3] = Math.Pow(xp, 3.0)
                };
                VectorD ys = new(4)
                {
                    [0] = 1.0,
                    [1] = yp,
                    [2] = Math.Pow(yp, 2.0),
                    [3] = Math.Pow(yp, 3.0)
                };
                return LinAlg.Dot((VectorZ)xs, LinAlg.Dot(a, (VectorZ)ys));
            }
        }

        /// <summary>
        /// finds the value f(x, y) for given 
        /// variables (x, y) by cubic interpolation
        /// </summary>
        /// <param name="v"> uniformly sampled input values </param>
        /// <param name="grid"> sampling grid for the input values </param>
        /// <param name="x"> arbitrary variable x </param>
        /// <param name="y"> arbitrary variable y </param>
        /// <param name="boundX"> data boundary option along x </param>
        /// <param name="boundY"> data boundary option along y </param>
        /// <returns> interpolated value f(x, y) </returns>
        public static Complex Cubic(MatrixZ v, GridInfo2D grid,
            double x, double y,
            DataBoundary boundX = Defaults.BoundaryOption,
            DataBoundary boundY = Defaults.BoundaryOption)
        {
            // makes a new grid, where values are on the grid edges
            GridInfo2D g = new(rows: grid.Rows - 1, cols: grid.Cols - 1,
                spacingY: grid.SpacingY, spacingX: grid.SpacingX,
                startY: grid.StartY + 0.5 * grid.SpacingY, startX: grid.StartX + 0.5 * grid.SpacingX);
            // prepares finite difference - fx, fy, fxy
            MatrixZ fx = new Grid2DCplxFiDi(vs: v, grid: null, option: FiDi2DOption.Dx).EvaluateAll();
            MatrixZ fy = new Grid2DCplxFiDi(vs: v, grid: null, option: FiDi2DOption.Dy).EvaluateAll();
            MatrixZ fxy = new Grid2DCplxFiDi(vs: v, grid: null, option: FiDi2DOption.Dxy).EvaluateAll();
            // calls and return
            return Cubic(v: v, grid: grid, shrinkedGrid: g, fx: fx, fy: fy, fxy: fxy,
                x: x, y: y, boundX: boundX, boundY: boundY);
        }

        #endregion

        #endregion
    }

}
