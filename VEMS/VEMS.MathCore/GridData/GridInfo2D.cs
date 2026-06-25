using System.Numerics;
using VEMS.MathCore.XTMethods;
using static VEMS.MathCore.FFTOptions;

namespace VEMS.MathCore
{

    /// <summary>
    /// Represents grid sampling information for 2D data.
    /// Provides properties and methods for describing and manipulating a uniform 2D grid,
    /// including grid point count, spacing, start position, and coordinate calculations.
    /// Supports grid modification, conjugation (for Fourier transforms), and equality operations.
    /// </summary>
    public class GridInfo2D : IEquatable<GridInfo2D>,
        IEqualityOperators<GridInfo2D, GridInfo2D, bool>
    {
        #region properties

        /// <summary>
        /// Gets or sets the number of columns along the x-direction of the grid.
        /// </summary>
        public long Cols { get; set; }

        /// <summary>
        /// Gets or sets the number of rows along the y-direction of the grid.
        /// </summary>
        public long Rows { get; set; }

        /// <summary>
        /// Gets or sets the spacing between two adjacent grid points in the x-direction.
        /// This is the distance between two adjacent columns in the grid.
        /// </summary>
        public double SpacingX { get; set; }

        /// <summary>
        /// Gets or sets the spacing between two adjacent grid points in the y-direction.
        /// This is the distance between two adjacent rows in the grid.
        /// </summary>
        public double SpacingY { get; set; }

        /// <summary>
        /// Gets the full sampling range along the x-direction.
        /// This is the total length covered by all columns in the grid, calculated as the product of the number of columns and the spacing between columns.
        /// </summary>
        public double RangeX => Cols * SpacingX;

        /// <summary>
        /// Gets the full sampling range along the y-direction.
        /// This is the total length covered by all rows in the grid, calculated as the product of the number of rows and the spacing between rows.
        /// </summary>
        public double RangeY => Rows * SpacingY;

        /// <summary>
        /// Gets or sets the coordinate of the first grid point in the x-direction.
        /// This corresponds to the coordinate of the first column in the grid.
        /// </summary>
        public double StartX { get; set; }

        /// <summary>
        /// Gets or sets the coordinate of the first grid point in the y-direction.
        /// This corresponds to the coordinate of the first row in the grid.
        /// </summary>
        public double StartY { get; set; }

        /// <summary>
        /// Gets or sets the center coordinate along the x-direction (columns).
        /// This value is not necessarily located on a grid point or column.
        /// </summary>
        public double CenterX
        {
            get => StartX + 0.5 * (RangeX - SpacingX);
            set => StartX = value - 0.5 * (RangeX - SpacingX);
        }

        /// <summary>
        /// Gets or sets the center coordinate along the y-direction (rows).
        /// This value is not necessarily located on a grid point or row.
        /// </summary>
        public double CenterY
        {
            get => StartY + 0.5 * (RangeY - SpacingY);
            set => StartY = value - 0.5 * (RangeY - SpacingY);
        }

        /// <summary>
        /// Gets the coordinate of the last grid point in the x-direction.
        /// This corresponds to the coordinate of the last column in the grid.
        /// </summary>
        public double EndX => StartX + RangeX - SpacingX;

        /// <summary>
        /// Gets the coordinate of the last grid point in the y-direction.
        /// This corresponds to the coordinate of the last row in the grid.
        /// </summary>
        public double EndY => StartY + RangeY - SpacingY;

        /// <summary>
        /// Gets the lower bound of the full range along the x-direction (columns).
        /// This is the coordinate of the first grid point minus half the grid spacing in x.
        /// </summary>
        public double LowerBoundX => StartX - 0.5 * SpacingX;

        /// <summary>
        /// Gets the upper bound of the full range along the x-direction (columns).
        /// This is the coordinate of the last grid point plus half the grid spacing in x.
        /// </summary>
        public double UpperBoundX => StartX + RangeX - 0.5 * SpacingX;

        /// <summary>
        /// Gets the lower bound of the full range along the y-direction (rows).
        /// This is the coordinate of the first grid point minus half the grid spacing in y.
        /// </summary>
        public double LowerBoundY => StartY - 0.5 * SpacingY;

        /// <summary>
        /// Gets the upper bound of the full range along the y-direction (rows).
        /// This is the coordinate of the last grid point plus half the grid spacing in y.
        /// </summary>
        public double UpperBoundY => StartY + RangeY - 0.5 * SpacingY;

        /// <summary>
        /// Gets the (y, x)-coordinates at the specified (row, column) index pair.
        /// </summary>
        /// <param name="iRow">The row index (y-direction).</param>
        /// <param name="iCol">The column index (x-direction).</param>
        /// <returns>
        /// A tuple containing the coordinates (y, x) corresponding to the specified row and column indices.
        /// </returns>
        public (double, double) this[long iRow, long iCol]
        {
            get => (GetCoordinateY(iRow), GetCoordinateX(iCol));
        }

        /// <summary>
        /// Gets the (y, x)-coordinates at the specified (row, column) index pair.
        /// </summary>
        /// <param name="iRow">The row index (y-direction).</param>
        /// <param name="iCol">The column index (x-direction).</param>
        /// <returns>
        /// A tuple containing the coordinates (y, x) corresponding to the specified row and column indices.
        /// </returns>
        public (double, double) this[int iRow, int iCol]
        {
            get => this[(long)iRow, (long)iCol];
        }

        /// <summary>
        /// Gets the (y, x)-coordinates at the specified (row, column) index pair.
        /// </summary>
        /// <param name="iRow">The row index (y-direction).</param>
        /// <param name="iCol">The column index (x-direction).</param>
        /// <returns>
        /// A tuple containing the coordinates (y, x) corresponding to the specified row and column indices.
        /// </returns>
        public (double, double) this[Index iRow, Index iCol]
        {
            get => this[iRow.ToLong(Rows), iCol.ToLong(Cols)];
        }

        #endregion
        #region constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="GridInfo2D"/> class with default values.
        /// The default grid has a single point at coordinate (0.0, 0.0) with spacings of 1.0 in both directions.
        /// </summary>
        public GridInfo2D()
        {
            Rows = 1;
            Cols = 1;
            SpacingX = 1.0;
            SpacingY = 1.0;
            StartX = 0.0;
            StartY = 0.0;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GridInfo2D"/> class
        /// with the specified number of rows and columns, grid spacings, reference points, and reference types.
        /// </summary>
        /// <param name="rows">The number of rows (grid points in the y-direction).</param>
        /// <param name="cols">The number of columns (grid points in the x-direction).</param>
        /// <param name="spacingY">The grid spacing in the y-direction.</param>
        /// <param name="spacingX">The grid spacing in the x-direction.</param>
        /// <param name="refPointY">The reference point location in the y-direction.</param>
        /// <param name="refPointX">The reference point location in the x-direction.</param>
        /// <param name="refTypeY">
        /// The type of reference point in the y-direction.
        /// If <see cref="GridRefType.Start"/>, <paramref name="refPointY"/> is the coordinate of the first grid point.
        /// If <see cref="GridRefType.Center"/>, <paramref name="refPointY"/> is the center coordinate of the grid.
        /// </param>
        /// <param name="refTypeX">
        /// The type of reference point in the x-direction.
        /// If <see cref="GridRefType.Start"/>, <paramref name="refPointX"/> is the coordinate of the first grid point.
        /// If <see cref="GridRefType.Center"/>, <paramref name="refPointX"/> is the center coordinate of the grid.
        /// </param>
        public GridInfo2D(long rows, long cols,
            double spacingY, double spacingX,
            double refPointY, double refPointX,
            GridRefType refTypeY = GridRefType.Start,
            GridRefType refTypeX = GridRefType.Start)
        {
            Rows = rows;
            SpacingY = spacingY;
            switch (refTypeY)
            {
                case GridRefType.Start:
                    StartY = refPointY;
                    break;
                case GridRefType.Center:
                    StartY = refPointY - 0.5 * (Rows - 1) * SpacingY;
                    break;
                case GridRefType.LowerBound:
                    StartY = refPointY + 0.5 * SpacingY;
                    break;
                case GridRefType.UpperBound:
                    StartY = refPointY - RangeY + 0.5 * SpacingY;
                    break;
                default: goto case GridRefType.Start;
            }

            Cols = cols;
            SpacingX = spacingX;
            switch (refTypeX)
            {
                case GridRefType.Start:
                    StartX = refPointX;
                    break;
                case GridRefType.Center:
                    StartX = refPointX - 0.5 * (Cols - 1) * SpacingX;
                    break;
                case GridRefType.LowerBound:
                    StartX = refPointX + 0.5 * SpacingX;
                    break;
                case GridRefType.UpperBound:
                    StartX = refPointX - RangeX + 0.5 * SpacingX;
                    break;
                default: goto case GridRefType.Start;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GridInfo2D"/> class
        /// with the specified number of grid points and grid spacings.
        /// The grid is centered at (0.0, 0.0) by default.
        /// </summary>
        /// <param name="rows">The number of grid points in the y-direction.</param>
        /// <param name="cols">The number of grid points in the x-direction.</param>
        /// <param name="spacingY">The grid spacing in the y-direction.</param>
        /// <param name="spacingX">The grid spacing in the x-direction.</param>
        public GridInfo2D(long rows, long cols,
            double spacingY, double spacingX)
            : this(rows: rows, cols: cols,
                  spacingY: spacingY, spacingX: spacingX,
                  refPointY: 0.0, refPointX: 0.0,
                  refTypeY: GridRefType.Center, refTypeX: GridRefType.Center)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="GridInfo2D"/> class
        /// with the specified number of grid points along x and y.
        /// The grid spacings are set to 1.0 and the grid is centered at (0.0, 0.0) by default.
        /// </summary>
        /// <param name="rows">The number of grid points in the y-direction.</param>
        /// <param name="cols">The number of grid points in the x-direction.</param>
        public GridInfo2D(long rows, long cols)
            : this(rows: rows, cols: cols, spacingY: 1.0, spacingX: 1.0)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="GridInfo2D"/> class by deep copying from another instance.
        /// </summary>
        /// <param name="other">The <see cref="GridInfo2D"/> instance to copy from.</param>
        public GridInfo2D(GridInfo2D other)
        {
            Rows = other.Rows;
            SpacingY = other.SpacingY;
            StartY = other.StartY;

            Cols = other.Cols;
            SpacingX = other.SpacingX;
            StartX = other.StartX;
        }


        /// <summary>
        /// constructs a SampleInfo2D
        /// with given grid points, grid spacings, 
        /// and start coordinates
        /// </summary>
        /// <param name="rows"> number of grid points in the y direction </param>
        /// <param name="cols"> number of grid points in the x direction </param>
        /// <param name="startY"> start coordinate in the y direction </param>
        /// <param name="startX"> start coordinate in the x direction </param>
        /// <param name="spacingY"> grid spacing in the y direction </param>
        /// <param name="spacingX"> grid spacing in the x direction </param>
        [Obsolete]
        public GridInfo2D(long rows, long cols,
            double startY, double startX,
            double spacingY, double spacingX)
        {
            Cols = cols;
            Rows = rows;
            StartX = startX;
            StartY = startY;
            SpacingX = spacingX;
            SpacingY = spacingY;
        }

        #endregion
        #region methods

        #region ---- coordinate ----

        /// <summary>
        /// Gets the coordinate value of a grid point with the given column index along the x-direction.
        /// </summary>
        /// <param name="iCol">Grid point index along the x-direction (column index).</param>
        /// <returns>The coordinate value at the specified column index.</returns>
        public double GetCoordinateX(long iCol)
            => (StartX + iCol * SpacingX);

        /// <summary>
        /// Gets the coordinate value of a grid point with the given row index along the y-direction.
        /// </summary>
        /// <param name="iRow">Grid point index along the y-direction (row index).</param>
        /// <returns>The coordinate value at the specified row index.</returns>
        public double GetCoordinateY(long iRow)
            => (StartY + iRow * SpacingY);

        /// <summary>
        /// Finds the grid span (row and column indices) that contains the given coordinates.
        /// Optionally applies periodic boundary conditions along each direction.
        /// </summary>
        /// <param name="y">Coordinate y along the row direction (will be updated if periodic).</param>
        /// <param name="x">Coordinate x along the column direction (will be updated if periodic).</param>
        /// <param name="periodicY">Whether to assume periodic boundary along the y-direction.</param>
        /// <param name="periodicX">Whether to assume periodic boundary along the x-direction.</param>
        /// <returns>
        /// A tuple containing the row and column indices of the grid span that contains the given coordinates.
        /// Returns -1 for an index if the coordinate is outside the grid and periodic is false for that direction.
        /// </returns>
        public (long, long) FindGridSpan(ref double y, ref double x,
            bool periodicY = false, bool periodicX = false)
        {
            long iRow, iCol;
            // calculates the signed multiple
            long mx = (long)Math.Floor((x - LowerBoundX) / RangeX);
            long my = (long)Math.Floor((y - LowerBoundY) / RangeY);
            // updates local distance and finds the grid
            double xp = x - mx * RangeX;
            double yp = y - my * RangeY;
            // row
            if (my == 0 || periodicY)
            { y = yp; iRow = (long)((y - LowerBoundY) / SpacingY); }
            else
            { iRow = -1; }
            // col
            if (mx == 0 || periodicX)
            { x = xp; iCol = (long)((x - LowerBoundX) / SpacingX); }
            else
            { iCol = -1; }
            // return
            return (iRow, iCol);
        }


        private unsafe VectorD GetCoordinates(bool isAlongX)
        {
            long n = isAlongX ? Cols : Rows;
            double start = isAlongX ? StartX : StartY;
            double delta = isAlongX ? SpacingX : SpacingY;
            VectorD vals = new(count: n, mode: ArrayInitMode.Malloc);
            double* ptr = (double*)vals.VPtr;
            for (long i = 0; i < n; i++)
            { ptr[i] = start + i * delta; }
            return vals;
        }

        /// <summary>
        /// Computes the coordinates for the grid points along the x-direction (columns).
        /// </summary>
        /// <returns>
        /// A <see cref="VectorD"/> containing the coordinates of all grid points along the x-direction.
        /// </returns>
        public VectorD GetCoordinatesX()
            => GetCoordinates(isAlongX: true);
        //=> new(count: Cols, initVal: StartX, increment: SpacingX);

        /// <summary>
        /// computes the coordinates for the grid points
        /// along the y-direction
        /// </summary>
        /// <returns> coordinates in a vector </returns>
        public VectorD GetCoordinatesY()
            => GetCoordinates(isAlongX: false);
        //=> new(count: Rows, initVal: StartY, increment: SpacingY);

        /// <summary>  
        /// Retrieves the grid coordinates for all grid points in the 2D grid.  
        /// </summary>  
        /// <remarks>  
        /// This method computes the x and y coordinates for each grid point in the 2D grid.  
        /// The coordinates are returned as two separate <see cref="VectorD"/> instances,  
        /// where the first vector contains the y-coordinates and the second vector contains the x-coordinates.  
        /// </remarks>  
        /// <returns>  
        /// A tuple containing two <see cref="VectorD"/> instances:  
        /// <list type="bullet">  
        /// <item><description>The first vector contains the y-coordinates of all grid points.</description></item>  
        /// <item><description>The second vector contains the x-coordinates of all grid points.</description></item>  
        /// </list>  
        /// </returns>
        public (VectorD, VectorD) GetGrid()
        {
            VectorD x = GetCoordinatesX();
            VectorD y = GetCoordinatesY();
            VectorD ys = new(Rows * Cols);
            VectorD xs = new(Rows * Cols);
            // loop for all rows
            for (long i = 0; i < Rows; i++)
            {
                LongRange iRng = new(start: i * Cols, (i + 1) * Cols);
                xs[iRng] = new(x, true);
                ys[iRng] = new(count: Cols, initVal: y[i, false]);
            }
            // return
            return (ys, xs);
        }

        /// <summary>
        /// Retrieves the coordinates of all grid points in the 2D grid as flat vectors.
        /// </summary>
        /// <remarks>
        /// This method computes the x and y coordinates for each grid point in the 2D grid.
        /// The coordinates are returned as two separate <see cref="VectorD"/> instances,
        /// where the first vector contains the y-coordinates and the second vector contains the x-coordinates.
        /// The vectors are flattened such that for each grid point (iRow, iCol), the corresponding index is <c>iRow * Cols + iCol</c>.
        /// </remarks>
        /// <returns>
        /// A tuple containing two <see cref="VectorD"/> instances:
        /// <list type="bullet">
        /// <item><description>The first vector contains the y-coordinates of all grid points (flattened).</description></item>
        /// <item><description>The second vector contains the x-coordinates of all grid points (flattened).</description></item>
        /// </list>
        /// </returns>
        public unsafe (VectorD, VectorD) GetAllCoordinates()
        {
            // Precompute total count
            long n = Rows * Cols;
            VectorD xs = GetCoordinatesX();
            VectorD ys = GetCoordinatesY();
            VectorD allCoordXs = new(count: n, mode: ArrayInitMode.Malloc);
            VectorD allCoordYs = new(count: n, mode: ArrayInitMode.Malloc);

            double* xsPtr = (double*)xs.VPtr;
            double* ysPtr = (double*)ys.VPtr;
            double* aCoordXsPtr = (double*)allCoordXs.VPtr;
            double* aCoordYsPtr = (double*)allCoordYs.VPtr;

            long idx = 0;
            for (long iRow = 0; iRow < Rows; iRow++)
            {
                double y = ysPtr[iRow];
                for (long iCol = 0; iCol < Cols; iCol++, idx++)
                {
                    aCoordXsPtr[idx] = xsPtr[iCol];
                    aCoordYsPtr[idx] = y;
                }
            }

            return (ys, xs);
        }

        /// <summary>
        /// Retrieves the coordinates of the main diagonal points for a square 2D grid.
        /// </summary>
        /// <remarks>
        /// Only valid for square grids (Rows == Cols).
        /// Returns two <see cref="VectorD"/>: the first is the y-coordinates, the second is the x-coordinates.
        /// </remarks>
        /// <returns>
        /// A tuple of <see cref="VectorD"/>: (ys, xs) for the main diagonal points.
        /// </returns>
        public (VectorD, VectorD) GetMainDiagonal()
        {
            if (Rows != Cols) throw new InvalidOperationException("Grid must be square (Rows == Cols) to extract the main diagonal.");

            VectorD ys = new(count: Rows, mode: ArrayInitMode.Malloc);
            VectorD xs = new(count: Rows, mode: ArrayInitMode.Malloc);
            for (long i = 0; i < Rows; i++)
            {
                ys[i] = GetCoordinateY(i);
                xs[i] = GetCoordinateX(Rows - 1 - i);
            }
            return (ys, xs);
        }

        #endregion
        #region ---- conjugate ----

        /// <summary>
        /// Updates the grid information to its conjugated domain after a Fourier transform,
        /// taking into account the shift theorem and linear phase.
        /// </summary>
        /// <param name="isForward">
        /// Indicates whether the transform is forward (<c>true</c>) or backward (<c>false</c>).
        /// </param>
        /// <param name="cLinearX">
        /// The linear phase coefficient along the x direction. On input, this is the linear phase in the original domain.
        /// On output, it is updated to the linear phase in the conjugated domain.
        /// </param>
        /// <param name="cLinearY">
        /// The linear phase coefficient along the y direction. On input, this is the linear phase in the original domain.
        /// On output, it is updated to the linear phase in the conjugated domain.
        /// </param>
        public void GetConjugated(bool isForward,
            ref double cLinearX, ref double cLinearY)
        {
            // number of samples remains the same
            long rows = Rows;
            long cols = Cols;
            // spacing in the conjugated domain
            double spacingX = 2.0 * Math.PI / RangeX;
            double spacingY = 2.0 * Math.PI / RangeY;

            // finds the center in the conjugated domain, first
            double centerX = isForward ? cLinearX : -cLinearX;
            double centerY = isForward ? cLinearY : -cLinearY;
            // updates the linear phase factor, then
            cLinearX = isForward ? -CenterX : CenterX;
            cLinearY = isForward ? -CenterY : CenterY;

            // updates the output grid
            SpacingX = spacingX;
            SpacingY = spacingY;
            StartX = centerX - 0.5 * (cols - 1) * SpacingX;
            StartY = centerY - 0.5 * (rows - 1) * SpacingY;
        }

        /// <summary>
        /// Updates the grid information to its conjugated domain after a Fourier transform,
        /// taking into account the shift theorem and linear phase.
        /// </summary>
        /// <param name="isForward">
        /// Indicates whether the transform is forward (<c>true</c>) or backward (<c>false</c>).
        /// </param>
        public void GetConjugated(bool isForward)
        {
            double cLinearX = 0.0;
            double cLinearY = 0.0;
            GetConjugated(isForward, ref cLinearX, ref cLinearY);
        }

        /// <summary>
        /// Updates the grid information to its conjugated domain after a Fourier transform,
        /// taking into account the shift theorem and linear phase.
        /// </summary>
        /// <param name="option">
        /// Fourier transform option: forward or backward.
        /// </param>
        /// <param name="linearPhaseFactorX">
        /// The linear phase factor along the x direction (in/out).
        /// </param>
        /// <param name="linearPhaseFactorY">
        /// The linear phase factor along the y direction (in/out).
        /// </param>
        [Obsolete("Use GetConjugated(bool isForward, ref double cLinearX, ref double cLinearY) instead.")]
        public void GetConjugated(FTOption option,
            ref double linearPhaseFactorX, ref double linearPhaseFactorY)
        {
            // number of samples remains the same
            long rows = Rows;
            long cols = Cols;
            // spacing in the conjugated domain
            double spacingX = 2.0 * Math.PI / RangeX;
            double spacingY = 2.0 * Math.PI / RangeY;

            // finds the center in the conjugated domain, first
            double centerX = (option == FTOption.Forward) ? linearPhaseFactorX : -linearPhaseFactorX;
            double centerY = (option == FTOption.Forward) ? linearPhaseFactorY : -linearPhaseFactorY;
            // updates the linear phase factor, then
            linearPhaseFactorX = (option == FTOption.Forward) ? -CenterX : CenterX;
            linearPhaseFactorY = (option == FTOption.Forward) ? -CenterY : CenterY;

            // updates the output grid
            SpacingX = spacingX;
            SpacingY = spacingY;
            StartX = centerX - 0.5 * (cols - 1) * SpacingX;
            StartY = centerY - 0.5 * (rows - 1) * SpacingY;
        }

        /// <summary>
        /// Updates the grid information to its conjugated domain after a Fourier transform,
        /// taking into account the shift theorem and linear phase.
        /// </summary>
        /// <param name="option">
        /// Fourier transform option: forward or backward.
        /// </param>
        [Obsolete("Use GetConjugated(bool isForward) instead.")]
        public void GetConjugated(FTOption option)
        {
            double linearPhaseFactorX = 0.0;
            double linearPhaseFactorY = 0.0;
            GetConjugated(option, ref linearPhaseFactorX, ref linearPhaseFactorY);
        }

        #endregion
        #region ---- modify ----

        /// <summary>
        /// Modifies the current sampling grid in-place according to the specified scaling and shift parameters.
        /// The method updates the grid spacing, number of rows and columns, and the start coordinates
        /// based on the provided scaling factors and center shifts for both x and y directions.
        /// </summary>
        /// <param name="rngFactorX">Sampling range scaling factor along the x direction. The total range will be multiplied by this value.</param>
        /// <param name="rngFactorY">Sampling range scaling factor along the y direction. The total range will be multiplied by this value.</param>
        /// <param name="spdFactorX">Sampling distance (spacing) scaling factor along the x direction. The spacing will be multiplied by this value.</param>
        /// <param name="spdFactorY">Sampling distance (spacing) scaling factor along the y direction. The spacing will be multiplied by this value.</param>
        /// <param name="ctrShiftX">Center shift value along the x direction. The center of the grid will be shifted by this value.</param>
        /// <param name="ctrShiftY">Center shift value along the y direction. The center of the grid will be shifted by this value.</param>
        public void GetModified(double rngFactorX = 1.0, double rngFactorY = 1.0,
            double spdFactorX = 1.0, double spdFactorY = 1.0,
            double ctrShiftX = 0.0, double ctrShiftY = 0.0)
        {
            // updates properties
            // first, sampling distance 
            if (spdFactorY != 1.0)
            { SpacingY *= spdFactorY; }
            if (spdFactorX != 1.0)
            { SpacingX *= spdFactorX; }
            // then, range can be larger
            if (spdFactorY != 1.0 || rngFactorY != 1.0)
            { Rows = (long)Math.Ceiling(RangeY * rngFactorY / SpacingY); }
            if (spdFactorX != 1.0 || rngFactorX != 1.0)
            { Cols = (long)Math.Ceiling(RangeX * rngFactorX / SpacingX); }
            // and, finds start point ...
            if (spdFactorY != 1.0 || rngFactorY != 1.0 || ctrShiftY != 0.0)
            { StartY = CenterY + ctrShiftY - 0.5 * (Rows - 1) * SpacingY; }
            if (spdFactorX != 1.0 || rngFactorX != 1.0 || ctrShiftX != 0.0)
            { StartX = CenterX + ctrShiftX - 0.5 * (Cols - 1) * SpacingX; }
        }

        /// <summary>
        /// Generates a new <see cref="GridInfo2D"/> instance representing a modified sampling grid
        /// according to the specified scaling and shift parameters. The original grid is not modified.
        /// </summary>
        /// <param name="rngFactorX">Sampling range scaling factor along the x direction. The total range will be multiplied by this value.</param>
        /// <param name="rngFactorY">Sampling range scaling factor along the y direction. The total range will be multiplied by this value.</param>
        /// <param name="spdFactorX">Sampling distance (spacing) scaling factor along the x direction. The spacing will be multiplied by this value.</param>
        /// <param name="spdFactorY">Sampling distance (spacing) scaling factor along the y direction. The spacing will be multiplied by this value.</param>
        /// <param name="ctrShiftX">Center shift value along the x direction. The center of the grid will be shifted by this value.</param>
        /// <param name="ctrShiftY">Center shift value along the y direction. The center of the grid will be shifted by this value.</param>
        /// <returns>A new <see cref="GridInfo2D"/> instance representing the modified sampling grid.</returns>
        public GridInfo2D Modify(double rngFactorX = 1.0, double rngFactorY = 1.0,
            double spdFactorX = 1.0, double spdFactorY = 1.0,
            double ctrShiftX = 0.0, double ctrShiftY = 0.0)
        {
            GridInfo2D g = new(other: this);
            g.GetModified(rngFactorX, rngFactorY, spdFactorX, spdFactorY, ctrShiftX, ctrShiftY);
            return g;
        }

        #endregion
        #region ---- equality ----

        /// <summary>
        /// Determines whether the current <see cref="GridInfo2D"/> instance is equal to another <see cref="GridInfo2D"/> instance.
        /// </summary>
        /// <param name="other">The other <see cref="GridInfo2D"/> instance to compare with this instance.</param>
        /// <returns>
        /// <c>true</c> if the specified <see cref="GridInfo2D"/> is equal to the current instance; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(GridInfo2D? other)
        {
            if (ReferenceEquals(this, other)) { return true; }
            if (other is null) { return false; }
            // use BitConverter.DoubleToInt64Bits for exact bitwise comparison of doubles
            return Rows == other.Rows && Cols == other.Cols
                && BitConverter.DoubleToInt64Bits(StartY) == BitConverter.DoubleToInt64Bits(other.StartY)
                && BitConverter.DoubleToInt64Bits(StartX) == BitConverter.DoubleToInt64Bits(other.StartX)
                && BitConverter.DoubleToInt64Bits(SpacingY) == BitConverter.DoubleToInt64Bits(other.SpacingY)
                && BitConverter.DoubleToInt64Bits(SpacingX) == BitConverter.DoubleToInt64Bits(other.SpacingY);
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current <see cref="GridInfo2D"/> instance.
        /// </summary>
        /// <param name="obj">The object to compare with the current <see cref="GridInfo2D"/> instance.</param>
        /// <returns>
        /// <c>true</c> if the specified object is a <see cref="GridInfo2D"/> and is equal to the current instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object? obj)
        {
            return Equals(obj as GridInfo1D);
        }

        #endregion
        #region ---- misc ----

        /// <summary>
        /// Returns a hash code for the current <see cref="GridInfo2D"/> instance.
        /// </summary>
        /// <remarks>
        /// The hash code is computed using the number of rows and columns, and the bitwise representation of the start coordinates and spacings.
        /// This ensures that grids with identical properties produce the same hash code.
        /// </remarks>
        /// <returns>
        /// An integer hash code representing the current <see cref="GridInfo2D"/> instance.
        /// </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + Rows.GetHashCode();
                hash = hash * 31 + Cols.GetHashCode();
                hash = hash * 31 + BitConverter.DoubleToInt64Bits(StartY).GetHashCode();
                hash = hash * 31 + BitConverter.DoubleToInt64Bits(StartX).GetHashCode();
                hash = hash * 31 + BitConverter.DoubleToInt64Bits(SpacingY).GetHashCode();
                hash = hash * 31 + BitConverter.DoubleToInt64Bits(SpacingX).GetHashCode();
                return hash;
            }
        }

        /// <summary>
        /// Gets the 1D sampling information along the x-direction (columns).
        /// </summary>
        /// <returns>
        /// A <see cref="GridInfo1D"/> instance representing the 1D grid information along the x-direction.
        /// The grid is constructed using the number of columns, the spacing between columns, the start coordinate,
        /// and the reference type set to <see cref="GridRefType.Center"/>.
        /// </returns>
        public GridInfo1D GetGridInfoX()
            => new(n: Cols, spacing: SpacingX, refPoint: StartX, refType: GridRefType.Center);

        /// <summary>
        /// Gets the 1D sampling information along the y-direction (rows).
        /// </summary>
        /// <returns>
        /// A <see cref="GridInfo1D"/> instance representing the 1D grid information along the y-direction.
        /// The grid is constructed using the number of rows, the spacing between rows, the start coordinate,
        /// and the reference type set to <see cref="GridRefType.Center"/>.
        /// </returns>
        public GridInfo1D GetGridInfoY()
            => new(n: Rows, spacing: SpacingY, refPoint: StartY, refType: GridRefType.Center);

        #endregion
        #region ---- unit scale ----

        /// <summary>
        /// Returns a new <see cref="GridInfo2D"/> instance with the grid spacings in the y and x directions
        /// scaled by the specified factors, using the given reference types to preserve the reference points.
        /// The number of rows and columns remains unchanged.
        /// </summary>
        /// <param name="scaleY">The scale factor to apply to the grid spacing in the y-direction. If 1.0, the spacing is unchanged.</param>
        /// <param name="scaleX">The scale factor to apply to the grid spacing in the x-direction. If 1.0, the spacing is unchanged.</param>
        /// <param name="refTypeY">
        /// The type of reference point to use for scaling in the y-direction.
        /// <see cref="GridRefType.Start"/> uses the start coordinate,
        /// <see cref="GridRefType.Center"/> uses the center coordinate,
        /// <see cref="GridRefType.LowerBound"/> uses the lower bound,
        /// <see cref="GridRefType.UpperBound"/> uses the upper bound.
        /// </param>
        /// <param name="refTypeX">
        /// The type of reference point to use for scaling in the x-direction.
        /// <see cref="GridRefType.Start"/> uses the start coordinate,
        /// <see cref="GridRefType.Center"/> uses the center coordinate,
        /// <see cref="GridRefType.LowerBound"/> uses the lower bound,
        /// <see cref="GridRefType.UpperBound"/> uses the upper bound.
        /// </param>
        /// <returns>
        /// A new <see cref="GridInfo2D"/> instance with the same number of rows and columns as the current instance,
        /// but with grid spacings multiplied by the specified scaling factors and the reference points preserved as specified.
        /// If both <paramref name="scaleY"/> and <paramref name="scaleX"/> are 1.0, the current instance is returned.
        /// </returns>
        public GridInfo2D Scale(double scaleY, double scaleX,
            GridRefType refTypeY = GridRefType.Center,
            GridRefType refTypeX = GridRefType.Center)
        {
            double refPointY = refTypeY switch
            {
                GridRefType.Start => StartY,
                GridRefType.Center => CenterY,
                GridRefType.LowerBound => LowerBoundY,
                GridRefType.UpperBound => UpperBoundY,
                _ => throw new ArgumentOutOfRangeException(nameof(refTypeY), "Invalid reference type for Y.")
            };

            double refPointX = refTypeX switch
            {
                GridRefType.Start => StartX,
                GridRefType.Center => CenterX,
                GridRefType.LowerBound => LowerBoundX,
                GridRefType.UpperBound => UpperBoundX,
                _ => throw new ArgumentOutOfRangeException(nameof(refTypeX), "Invalid reference type for X.")
            };

            return new GridInfo2D(rows: Rows, cols: Cols,
                spacingY: SpacingY * scaleY, spacingX: SpacingX * scaleX,
                refPointY: refPointY, refPointX: refPointX,
                refTypeY: refTypeY, refTypeX: refTypeX);
        }

        #endregion

        #endregion
        #region operators

        #region ---- explicit ----

        /// <summary>
        /// Performs an explicit conversion from <see cref="GridInfo2D"/> to <see cref="ScatInfoXY"/>.
        /// Converts the 2D grid information to a scattered format with all y and x coordinates explicitly given.
        /// </summary>
        /// <param name="g">The input <see cref="GridInfo2D"/> instance to convert.</param>
        /// <returns>
        /// A <see cref="ScatInfoXY"/> instance containing the y and x coordinates of the grid points.
        /// </returns>
        public static explicit operator ScatInfoXY(GridInfo2D g)
            => new(ys: g.GetCoordinatesY(), xs: g.GetCoordinatesX());

        /// <summary>
        /// Performs an explicit conversion from <see cref="GridInfo2D"/> to <see cref="ScatInfo2D"/>.
        /// Converts the 2D grid information to a scattered format with all y and x coordinates explicitly given.
        /// </summary>
        /// <param name="g">The input <see cref="GridInfo2D"/> instance to convert.</param>
        /// <returns>
        /// A <see cref="ScatInfo2D"/> instance containing the y and x coordinates of all grid points in a scattered format.
        /// </returns>
        //public static explicit operator ScatInfo2D(GridInfo2D g)
        //{
        //    VectorD x = g.GetCoordinatesX();
        //    VectorD y = g.GetCoordinatesY();
        //    VectorD ys = new(g.Rows * g.Cols);
        //    VectorD xs = new(g.Rows * g.Cols);
        //    // loop for all rows
        //    for (long i = 0; i < g.Rows; i++)
        //    {
        //        LongRange iRng = new(start: i * g.Cols, (i + 1) * g.Cols);
        //        xs[iRng] = new(x, true);
        //        ys[iRng] = new(count: g.Cols, initVal: y[i, false]);
        //    }
        //    // return
        //    return new(ys, xs);
        //}
        public static explicit operator ScatInfo2D(GridInfo2D g)
        {
            // Precompute total count
            long n = g.Rows * g.Cols;
            VectorD x = g.GetCoordinatesX();
            VectorD y = g.GetCoordinatesY();
            VectorD ys = new(count: n, mode: ArrayInitMode.Malloc);
            VectorD xs = new(count: n, mode: ArrayInitMode.Malloc);

            // Fill xs and ys in a single loop to improve cache locality
            for (long iRow = 0, idx = 0; iRow < g.Rows; iRow++)
            {
                double yVal = y[iRow, false];
                for (long iCol = 0; iCol < g.Cols; iCol++, idx++)
                {
                    xs[idx, false] = x[iCol, false];
                    ys[idx, false] = yVal;
                }
            }
            return new(ys, xs);
        }

        #endregion
        #region ---- equility ----

        /// <summary>
        /// Determines whether two <see cref="GridInfo2D"/> instances are equal.
        /// </summary>
        /// <param name="left">The first <see cref="GridInfo2D"/> instance to compare, or <see langword="null"/>.</param>
        /// <param name="right">The second <see cref="GridInfo2D"/> instance to compare, or <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if the two instances are equal or both are <see langword="null"/>;  otherwise, <see
        /// langword="false"/>.</returns>
        public static bool operator ==(GridInfo2D? left, GridInfo2D? right)
        {
            if (ReferenceEquals(left, right)) { return true; }
            if (left is null || right is null) { return false; }
            return left.Equals(right);
        }

        /// <summary>
        /// Determines whether two <see cref="GridInfo2D"/> instances are not equal.
        /// </summary>
        /// <param name="left">The first <see cref="GridInfo2D"/> instance to compare, or <see langword="null"/>.</param>
        /// <param name="right">The second <see cref="GridInfo2D"/> instance to compare, or <see langword="null"/>.</param>
        /// <returns>
        /// <see langword="true"/> if the two instances are not equal; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool operator !=(GridInfo2D? left, GridInfo2D? right)
            => !(left == right);

        #endregion
        #region ---- unit scale ----

        /// <summary>
        /// Scales the grid spacing in both the y and x directions by the specified factors.
        /// The grid remains centered at the same center coordinates, and the number of rows and columns is unchanged.
        /// </summary>
        /// <param name="g">The <see cref="GridInfo2D"/> instance to scale.</param>
        /// <param name="scale">
        /// A tuple containing the scaling factors for the y-direction (Item1) and x-direction (Item2).
        /// If both factors are 1.0, the original grid is returned.
        /// </param>
        /// <returns>
        /// A new <see cref="GridInfo2D"/> instance with the same number of rows and columns as <paramref name="g"/>,
        /// but with grid spacings multiplied by the specified scaling factors and centered at the same coordinates.
        /// </returns>
        public static GridInfo2D operator *(GridInfo2D g, (double, double) scale)
        {
            if (scale.Item1 == 1.0 && scale.Item2 == 1.0) { return g; }
            //return new GridInfo2D(rows: g.Rows, cols: g.Cols,
            //    spacingY: g.SpacingY * scale.Item1, spacingX: g.SpacingX * scale.Item2,
            //    refPointY: g.CenterY, refTypeY: GridRefType.Center,
            //    refPointX: g.CenterX, refTypeX: GridRefType.Center);
            return g.Scale(scaleY: scale.Item1, scaleX: scale.Item2,
                refTypeY: GridRefType.Center, refTypeX: GridRefType.Center);
        }

        /// <summary>
        /// Scales the grid spacing in both the y and x directions by the specified factors.
        /// The grid remains centered at the same center coordinates, and the number of rows and columns is unchanged.
        /// This overload allows the scaling tuple to appear on the left-hand side of the multiplication operator.
        /// </summary>
        /// <param name="scale">
        /// A tuple containing the scaling factors for the y-direction (Item1) and x-direction (Item2).
        /// </param>
        /// <param name="g">The <see cref="GridInfo2D"/> instance to scale.</param>
        /// <returns>
        /// A new <see cref="GridInfo2D"/> instance with the same number of rows and columns as <paramref name="g"/>,
        /// but with grid spacings multiplied by the specified scaling factors and centered at the same coordinates.
        /// </returns>
        public static GridInfo2D operator *((double, double) scale, GridInfo2D g)
            => g * scale;

        #endregion

        #endregion
    }
}
