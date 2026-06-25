using System.Numerics;

namespace VEMS.AMathCore
{
    /// <summary>
    /// Provides finite-difference derivative evaluation for 2D grid-valued data.
    /// </summary>
    /// <typeparam name="T">Numeric type used for matrix values; must implement <see cref="INumber{T}"/>.</typeparam>
    public class GridFiDi2D<T> where T : INumber<T>
    {
        #region properties

        /// <summary>
        /// Number of rows in the input matrix.
        /// </summary>
        private long Rows { get; set; }

        /// <summary>
        /// Number of columns in the input matrix.
        /// </summary>
        private long Cols { get; set; }

        /// <summary>
        /// Function that computes the derivative at a given grid index.
        /// </summary>
        /// <value>
        /// A function that accepts a row index and a column index and returns
        /// the derivative value of type <typeparamref name="T"/> at that location.
        /// May be <see langword="null"/> if not initialized.
        /// </value>
        public Func<long, long, T>? FindDerivative { get; set; }

        #endregion
        #region constructors

        /// <summary>
        /// Initializes a new internal instance of the <see cref="GridFiDi2D{T}"/> class.
        /// Intended for internal use where the derivative function is assigned later.
        /// </summary>
        internal GridFiDi2D() { }

        /// <summary>
        /// Constructs a <see cref="GridFiDi2D{T}"/> instance and assigns a derivative
        /// evaluation function according to the specified <paramref name="option"/>.
        /// </summary>
        /// <param name="vs">Function values stored in a matrix.</param>
        /// <param name="grid">Optional sampling grid of the function values. If provided, spacing information is used to scale derivatives.</param>
        /// <param name="option">Derivative option that selects which finite-difference operator to use.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="grid"/> is provided and its dimensions do not match <paramref name="vs"/>.</exception>
        public GridFiDi2D(Matx<T> vs,
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
        /// Evaluates the derivative values at all grid locations.
        /// </summary>
        /// <param name="loopMode">Loop-computational mode options used to evaluate the loop.</param>
        /// <returns>A <see cref="Matx{T}"/> containing the derivative value at each grid location.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the derivative function <see cref="FindDerivative"/> has not been set.</exception>
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
        #region static methods

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
            if (checkBound && (iCol < 0 || iCol >= f.Cols || iRow < 0 || iRow >= f.Rows))
                return T.Zero;

            // within bound
            if (iCol == 0)
            {
                // force to forward difference
                d = f[iRow, iCol + 1, false] - f[iRow, iCol, false]; 
            }
            else if (iCol == f.Cols - 1)
            {
                // force to backward difference
                d = f[iRow, iCol, false] - f[iRow, iCol - 1, false]; 
            }
            else
            {
                // using central difference
                T right = f[iRow, iCol + 1, false];
                T left = f[iRow, iCol - 1, false];
                d = (right - left) / two; 
            }

            if (grid != null) 
            { 
                T spacingX = T.CreateChecked(grid.SpacingX);
                d /= spacingX; 
            }

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
            if (checkBound && (iCol < 0 || iCol >= f.Cols || iRow < 0 || iRow >= f.Rows))
                return T.Zero;
            
            // within bound
            if (iRow == 0) 
            {
                // force to forward difference
                d = f[iRow + 1, iCol, false] - f[iRow, iCol, false]; 
            }
            else if (iRow == f.Rows - 1) 
            {
                // force to backward difference
                d = f[iRow, iCol, false] - f[iRow - 1, iCol, false]; 
            }
            else 
            {
                // using central difference
                d = (f[iRow + 1, iCol, false] - f[iRow - 1, iCol, false]) / two; 
            }

            if (grid != null) 
            {
                T spacingY = T.CreateChecked(grid.SpacingY);
                d /= spacingY;
            }

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
            if (checkBound && (iCol < 0 || iCol >= f.Cols || iRow < 0 || iRow >= f.Rows))
                return T.Zero;
            
            // within bound
            if (iCol == 0) 
            {
                // force to forward difference
                d = f[iRow, iCol + 2, false] + f[iRow, iCol, false] 
                    - two * f[iRow, iCol + 1, false]; 
            }
            else if (iCol == f.Cols - 1) 
            {
                // force to backward difference
                d = f[iRow, iCol, false] + f[iRow, iCol - 2, false] 
                    - two * f[iRow, iCol - 1, false]; 
            }
            else 
            {
                // using central difference
                d = f[iRow, iCol + 1, false] + f[iRow, iCol - 1, false] 
                    - two * f[iRow, iCol, false]; 
            }

            if (grid != null) 
            {
                T spacingX = T.CreateChecked(grid.SpacingX);
                d /= spacingX * spacingX; 
            }

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
            if (checkBound && (iCol < 0 || iCol >= f.Cols || iRow < 0 || iRow >= f.Rows))
                d = T.Zero;
            
            // within bound
            if (iRow == 0) 
            {
                // force to forward difference
                d = f[iRow + 2, iCol, false] + f[iRow, iCol, false] 
                    - two * f[iRow + 1, iCol, false]; 
            }
            else if (iRow == f.Rows - 1) 
            {
                // force to backward difference
                d = f[iRow, iCol, false] + f[iRow - 2, iCol, false] 
                    - two * f[iRow - 1, iCol, false]; 
            }
            else 
            {
                // using central difference
                d = f[iRow + 1, iCol, false] + f[iRow - 1, iCol, false] 
                    - two * f[iRow, iCol, false]; 
            }

            if (grid != null) 
            { 
                T spacingY = T.CreateChecked(grid.SpacingY);
                d /= spacingY * spacingY; 
            }

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
            if (checkBound && (iCol < 0 || iCol >= f.Cols || iRow < 0 || iRow >= f.Rows))
                d = T.Zero;
            
            // within bound
            // cases
            if (iRow == 0) 
            {
                // forward difference along y
                //long iRowp = iRow + 1;
                T dxp = Dx(f, iRow + 1, iCol, grid, checkBound);
                T dx = Dx(f, iRow, iCol, grid, checkBound);
                d = dxp - dx; // /dy;
            }
            else if (iRow == f.Rows - 1) 
            {
                // backward difference along y
                //long iym = iRow - 1;
                T dx = Dx(f, iRow, iCol, grid, checkBound);
                T dxm = Dx(f, iRow - 1, iCol, grid, checkBound);
                d = dx - dxm; // / dy;
            }
            else 
            {
                // central difference along y
                //long iyp = iy + 1;
                //long iym = iy - 1;
                T dxp = Dx(f, iRow + 1, iCol, grid, checkBound);
                T dxm = Dx(f, iRow - 1, iCol, grid, checkBound);
                d = (dxp - dxm) / two; // (2.0 * dy);
            }

            if (grid != null) 
            {
                T spacingY = T.CreateChecked(grid.SpacingY);
                d /= spacingY; 
            }
            
            return d;
        }

        #endregion
    }


}
