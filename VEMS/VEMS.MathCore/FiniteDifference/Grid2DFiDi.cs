using System.Numerics;

namespace VEMS.MathCore
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Grid2DFiDi<T>
        where T : INumber<T>
    {
        #region properties

        private long Rows { get; set; }
        private long Cols { get; set; }

        /// <summary>
        /// finds derivative value at specific grid index 
        /// <para> arg #1: row index that specifies a grid location </para>
        /// <para> arg #2: column index that specifies a grid location </para>
        /// <para> result: derivative value at this location </para>
        /// </summary>
        public Func<long, long, T>? FindDerivative { get; set; }

        #endregion
        #region constructors

        /// <summary>
        /// default constructor
        /// </summary>
        internal Grid2DFiDi() { }

        /// <summary>
        /// constructs a Grid1DRealFiDi class
        /// </summary>
        /// <param name="vs"> function values stored in a vector </param>
        /// <param name="grid"> sampling grid of the function values </param>
        /// <param name="option"> derivative option </param>
        public Grid2DFiDi(Matx<T> vs,
            GridInfo2D? grid = null,
            FiDi2DOption option = FiDi2DOption.Dx)
        {
            if (grid != null && (vs.Rows != grid.Rows || vs.Cols != grid.Cols))
            { throw new ArgumentException(); }
            
            Rows = vs.Rows;
            Cols = vs.Cols;
            switch (option)
            {
                case FiDi2DOption.Dx:
                    FindDerivative = (iRow, iCol) => Dx(vs, iRow, iCol, grid, checkBound: false);
                    break;
                case FiDi2DOption.Dy:
                    FindDerivative = (iRow, iCol) => Dy(vs, iRow, iCol, grid, checkBound: false);
                    break;
                case FiDi2DOption.Dxx:
                    FindDerivative = (iRow, iCol) => Dxx(vs, iRow, iCol, grid, checkBound: false);
                    break;
                case FiDi2DOption.Dyy:
                    FindDerivative = (iRow, iCol) => Dyy(vs, iRow, iCol, grid, checkBound: false);
                    break;
                case FiDi2DOption.Dxy:
                    FindDerivative = (iRow, iCol) => Dxy(vs, iRow, iCol, grid, checkBound: false);
                    break;
                default: goto case FiDi2DOption.Dx;
            }
        }

        #endregion
        #region methods

        /// <summary>
        /// evaluates the derivative values at
        /// all the grid locations 
        /// </summary>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> resulting derivative values at all grid locations </returns>
        public Matx<T> EvaluateAll(LoopMode loopMode = Defaults.LoopOption)
        {
            if (FindDerivative == null) { throw new ArgumentNullException(nameof(FindDerivative)); }

            // initialize
            Matx<T> ve = new(rows: Rows, cols: Cols, initMode: ArrayInitMode.Malloc);
            // defines loop operation
            Loop2D loop = new(operation: (iRow, iCol) => ve[iRow, iCol, false] = FindDerivative(iRow, iCol),
                rowStart: 0, rowEnd: Rows,
                colStart: 0, colEnd: Cols,
                rowStep: 1, colStep: 1);
            loop.Evaluate(loopMode);

            // return
            return ve;
        }

        #endregion
        #region pointwise static methods

        /// <summary>
        /// Computes the first-order finite difference (derivative) along the x-direction
        /// at a specific matrix location for a generic matrix type. Uses central difference
        /// except at the boundaries, where forward or backward difference is used.
        /// </summary>
        /// <typeparam name="T">The numeric type of the matrix elements, must implement <see cref="INumber{T}"/>.</typeparam>
        /// <param name="f">Input matrix of values.</param>
        /// <param name="iRow">Target row index (along y-direction).</param>
        /// <param name="iCol">Target column index (along x-direction).</param>
        /// <param name="grid">Optional: Uniform sampling grid of the input values. If provided, the result is divided by the grid spacing in x.</param>
        /// <param name="checkBound">If true, checks if the indices are out of bounds and returns zero if so.</param>
        /// <returns>The first-order derivative along x at the specified matrix location.</returns>
        public static T Dx(Matx<T> f, long iRow, long iCol,
            GridInfo2D? grid = null,
            bool checkBound = false)
        {
            T d;
            T two = T.CreateChecked(2.0);

            // outside bound?
            if (checkBound)
            {
                if (iCol < 0 || iCol > f.Cols - 1) { d = T.Zero; }
                if (iRow < 0 || iRow > f.Rows - 1) { d = T.Zero; }
            }
            // within bound
            if (iCol == 0) // force to forward difference
            { d = f[iRow, iCol + 1, false] - f[iRow, iCol, false]; }
            else if (iCol == f.Cols - 1) // force to backward difference
            { d = f[iRow, iCol, false] - f[iRow, iCol - 1, false]; }
            else // using central difference
            { d = (f[iRow, iCol + 1, false] - f[iRow, iCol - 1, false]) / two; }

            if (grid != null) { d /= T.CreateChecked(grid.SpacingX); }
            return d;
        }

        /// <summary>
        /// Computes the first-order finite difference (derivative) along the y-direction
        /// at a specific matrix location for a generic matrix type. Uses central difference
        /// except at the boundaries, where forward or backward difference is used.
        /// </summary>
        /// <typeparam name="T">The numeric type of the matrix elements, must implement <see cref="INumber{T}"/>.</typeparam>
        /// <param name="f">Input matrix of values.</param>
        /// <param name="iRow">Target row index (along y-direction).</param>
        /// <param name="iCol">Target column index (along x-direction).</param>
        /// <param name="grid">Optional: Uniform sampling grid of the input values. If provided, the result is divided by the grid spacing in y.</param>
        /// <param name="checkBound">If true, checks if the indices are out of bounds and returns zero if so.</param>
        /// <returns>The first-order derivative along y at the specified matrix location.</returns>
        public static T Dy(Matx<T> f, long iRow, long iCol,
            GridInfo2D? grid = null,
            bool checkBound = false)
        {
            T d;
            T two = T.CreateChecked(2.0);

            // outside bound?
            if (checkBound)
            {
                if (iCol < 0 || iCol > f.Cols - 1) { d = T.Zero; }
                if (iRow < 0 || iRow > f.Rows - 1) { d = T.Zero; }
            }
            // within bound
            if (iRow == 0) // force to forward difference
            { d = f[iRow + 1, iCol, false] - f[iRow, iCol, false]; }
            else if (iRow == f.Rows - 1) // force to backward difference
            { d = f[iRow, iCol, false] - f[iRow - 1, iCol, false]; }
            else // using central difference
            { d = (f[iRow + 1, iCol, false] - f[iRow - 1, iCol, false]) / two; }

            if (grid != null) { d /= T.CreateChecked(grid.SpacingY); }
            return d;
        }


        /// <summary>
        /// Computes the second-order finite difference (second derivative) along the x-direction
        /// at a specific matrix location for a generic matrix type. Uses central difference
        /// except at the boundaries, where forward or backward difference is used.
        /// </summary>
        /// <typeparam name="T">The numeric type of the matrix elements, must implement <see cref="INumber{T}"/>.</typeparam>
        /// <param name="f">Input matrix of values.</param>
        /// <param name="iRow">Target row index (along y-direction).</param>
        /// <param name="iCol">Target column index (along x-direction).</param>
        /// <param name="grid">Optional: Uniform sampling grid of the input values. If provided, the result is divided by the square of the grid spacing in x.</param>
        /// <param name="checkBound">If true, checks if the indices are out of bounds and returns zero if so.</param>
        /// <returns>The second-order derivative along x at the specified matrix location.</returns>
        public static T Dxx(Matx<T> f, long iRow, long iCol,
            GridInfo2D? grid = null,
            bool checkBound = false)
        {
            T d;
            T two = T.CreateChecked(2.0);

            // outside bound?
            if (checkBound)
            {
                if (iCol < 0 || iCol > f.Cols - 1) { d = T.Zero; }
                if (iRow < 0 || iRow > f.Rows - 1) { d = T.Zero; }
            }
            // within bound
            if (iCol == 0) // force to forward difference
            { d = f[iRow, iCol + 2, false] + f[iRow, iCol, false] - two * f[iRow, iCol + 1, false]; }
            else if (iCol == f.Cols - 1) // force to backward difference
            { d = f[iRow, iCol, false] + f[iRow, iCol - 2, false] - two * f[iRow, iCol - 1, false]; }
            else // using central difference
            { d = f[iRow, iCol + 1, false] + f[iRow, iCol - 1, false] - two * f[iRow, iCol, false]; }

            if (grid != null) { d /= T.CreateChecked(grid.SpacingX * grid.SpacingX); }
            return d;
        }

        /// <summary>
        /// Computes the second-order finite difference (second derivative) along the y-direction
        /// at a specific matrix location for a generic matrix type. Uses central difference
        /// except at the boundaries, where forward or backward difference is used.
        /// </summary>
        /// <typeparam name="T">The numeric type of the matrix elements, must implement <see cref="INumber{T}"/>.</typeparam>
        /// <param name="f">Input matrix of values.</param>
        /// <param name="iRow">Target row index (along y-direction).</param>
        /// <param name="iCol">Target column index (along x-direction).</param>
        /// <param name="grid">Optional: Uniform sampling grid of the input values. If provided, the result is divided by the square of the grid spacing in y.</param>
        /// <param name="checkBound">If true, checks if the indices are out of bounds and returns zero if so.</param>
        /// <returns>The second-order derivative along y at the specified matrix location.</returns>
        public static T Dyy(Matx<T> f, long iRow, long iCol,
            GridInfo2D? grid = null,
            bool checkBound = false)
        {
            T d;
            T two = T.CreateChecked(2.0);

            // outside bound?
            if (checkBound)
            {
                if (iCol < 0 || iCol > f.Cols - 1) { d = T.Zero; }
                if (iRow < 0 || iRow > f.Rows - 1) { d = T.Zero; }
            }
            // within bound
            if (iRow == 0) // force to forward difference
            { d = f[iRow + 2, iCol, false] + f[iRow, iCol, false] - two * f[iRow + 1, iCol, false]; }
            else if (iRow == f.Rows - 1) // force to backward difference
            { d = f[iRow, iCol, false] + f[iRow - 2, iCol, false] - two * f[iRow - 1, iCol, false]; }
            else // using central difference
            { d = f[iRow + 1, iCol, false] + f[iRow - 1, iCol, false] - two * f[iRow, iCol, false]; }

            if (grid != null) { d /= T.CreateChecked(grid.SpacingY * grid.SpacingY); }
            return d;
        }


        /// <summary>
        /// Computes the mixed second-order finite difference (cross-derivative) ∂²f/∂x∂y
        /// at a specific matrix location for a generic matrix type. The derivative is computed
        /// by first applying the finite difference along the x-direction and then along the y-direction.
        /// Uses central difference except at the boundaries, where forward or backward difference is used.
        /// </summary>
        /// <typeparam name="T">The numeric type of the matrix elements, must implement <see cref="INumber{T}"/>.</typeparam>
        /// <param name="f">Input matrix of values.</param>
        /// <param name="iRow">Target row index (along y-direction).</param>
        /// <param name="iCol">Target column index (along x-direction).</param>
        /// <param name="grid">Optional: Uniform sampling grid of the input values. If provided, the result is divided by the grid spacing in y.</param>
        /// <param name="checkBound">If true, checks if the indices are out of bounds and returns zero if so.</param>
        /// <returns>The mixed second-order derivative (cross-derivative) at the specified matrix location.</returns>
        public static T Dxy(Matx<T> f, long iRow, long iCol,
            GridInfo2D? grid = null,
            bool checkBound = false)
        {
            T d;
            T two = T.CreateChecked(2.0);

            // outside bound?
            if (checkBound)
            {
                if (iCol < 0 || iCol > f.Cols - 1) { d = T.Zero; }
                if (iRow < 0 || iRow > f.Rows - 1) { d = T.Zero; }
            }
            // within bound
            // cases
            if (iRow == 0) // forward difference along y
            {
                //long iRowp = iRow + 1;
                T dxp = Dx(f, iRow + 1, iCol, grid, checkBound);
                T dx = Dx(f, iRow, iCol, grid, checkBound);
                d = dxp - dx; // /dy;
            }
            else if (iRow == f.Rows - 1) // backward difference along y
            {
                //long iym = iRow - 1;
                T dx = Dx(f, iRow, iCol, grid, checkBound);
                T dxm = Dx(f, iRow - 1, iCol, grid, checkBound);
                d = dx - dxm; // / dy;
            }
            else // central difference along y
            {
                //long iyp = iy + 1;
                //long iym = iy - 1;
                T dxp = Dx(f, iRow + 1, iCol, grid, checkBound);
                T dxm = Dx(f, iRow - 1, iCol, grid, checkBound);
                d = (dxp - dxm) / two; // (2.0 * dy);
            }

            if (grid != null) { d /= T.CreateChecked(grid.SpacingY); }
            return d;
        }

        #endregion
    }
}
