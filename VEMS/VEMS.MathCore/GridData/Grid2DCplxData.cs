using System.Diagnostics.CodeAnalysis;
using Complex = System.Numerics.Complex;

namespace VEMS.MathCore
{

    /// <summary>
    /// Two-dimensional complex-valued grid data
    /// </summary>
    public class Grid2DCplxData
    {
        #region properties

        /// <summary>
        /// data values in a matrix
        /// </summary>
        public MatrixZ Values { get; set; }

        /// <summary>
        /// sampling grid information
        /// </summary>
        public GridInfo2D GridInfo { get; set; }

        /// <summary>
        /// analytical phase term
        /// currently only linear phase term is supported
        /// </summary>
        public Analyt2DPhase Phase { get; set; }

        /// <summary>
        /// data boundary option along x: periodic or zero
        /// </summary>
        public DataBoundary BoundaryX { get; set; }

        /// <summary>
        /// data boundary option along y: periodic or zero
        /// </summary>
        public DataBoundary BoundaryY { get; set; }

        /// <summary>
        /// internal interpolation of the data
        /// </summary>
        public Grid2DCplxInterpolation Interpolation { get; set; }

        /// <summary>
        /// interpolation method used for continuation
        /// </summary>
        public InterpolationMethod IntrplMethod
        {
            get => Interpolation.Method;
            [MemberNotNull(nameof(Interpolation))]
            set => Interpolation = new(v: Values, grid: GridInfo,
                method: value, boundX: BoundaryX, boundY: BoundaryY);
        }

        #endregion
        #region constructors

        /// <summary>
        /// constructs a default GridMat
        /// with one value of 0.0
        /// </summary>
        public Grid2DCplxData()
        {
            GridInfo = new();
            Values = new(rows: 1, cols: 1);
            Phase = new();
            BoundaryX = Defaults.BoundaryOption;
            BoundaryY = Defaults.BoundaryOption;
            IntrplMethod = Defaults.IntrplOption;
        }

        /// <summary>
        /// constructs a new Grid2DCplxData with given
        /// sampling grid information and data values
        /// without deep copy of data
        /// </summary>
        /// <param name="values"> data values in a matrix </param>
        /// <param name="gridInfo"> sampling grid information </param>
        /// <param name="c1x"> linear (1st-order) coefficient along x </param>
        /// <param name="c1y"> linear (1st-order) coefficient along y </param>
        /// <param name="intrpl"> interpolation method </param>
        /// <param name="boundX"> data boundary along x-axis </param>
        /// <param name="boundY"> data boundary along y-axis </param>
        public Grid2DCplxData(MatrixZ values, GridInfo2D? gridInfo = null,
            double c1x = 0.0, double c1y = 0.0,
            InterpolationMethod intrpl = Defaults.IntrplOption,
            DataBoundary boundX = Defaults.BoundaryOption,
            DataBoundary boundY = Defaults.BoundaryOption)
        {
            if (gridInfo != null && (gridInfo.Rows != values.Rows || gridInfo.Cols != values.Cols))
            { throw new ArgumentException($"Unequal number of input sampling"); }
            
            Values = values;
            GridInfo = gridInfo ?? new(rows: values.Rows, cols: values.Cols);
            Phase = new(c1x: c1x, c1y: c1y);
            BoundaryX = boundX;
            BoundaryY = boundY;
            IntrplMethod = intrpl;
        }

        /// <summary>
        /// constructs a GridMat with given count, 
        /// start and spacing, with a given initial value
        /// </summary>
        /// <param name="rows"> number of elements along y </param>
        /// <param name="cols"> number of elements along x </param>
        /// <param name="startY"> first grid point coordinate along y </param>
        /// <param name="startX"> first grid point coordinate along x </param>
        /// <param name="spacingY"> grid spacing along y </param>
        /// <param name="spacingX"> grid spacing along x </param>
        /// <param name="initVal"> the common initial value </param>
        /// <param name="intrpl"> interpolation method </param>
        /// <param name="boundX"> data boundary along x-axis </param>
        /// <param name="boundY"> data boundary along y-axis </param>
        public Grid2DCplxData(long rows, long cols,
            double startY, double startX,
            double spacingY, double spacingX,
            double initVal,
            InterpolationMethod intrpl = Defaults.IntrplOption,
            DataBoundary boundX = Defaults.BoundaryOption,
            DataBoundary boundY = Defaults.BoundaryOption)
        {
            GridInfo = new(rows: rows, cols: cols,
                spacingY: spacingY, spacingX: spacingX,
                refPointY: startY, refTypeY: GridRefType.Start,
                refPointX: startX, refTypeX: GridRefType.Start);
            Values = new(rows, cols, initVal);
            Phase = new();
            BoundaryX = boundX;
            BoundaryY = boundY;
            IntrplMethod = intrpl;
        }

        /// <summary>
        /// constructs a GridMat with given count
        /// with a given initial value
        /// </summary>
        /// <param name="rows"> number of elements along y </param>
        /// <param name="cols"> number of elements along x </param>
        /// <param name="spacingY"> grid spacing along y </param>
        /// <param name="spacingX"> grid spacing along x </param>
        /// <param name="initVal"> the common initial value </param>
        /// <param name="intrpl"> interpolation method </param>
        /// <param name="boundX"> data boundary along x-axis </param>
        /// <param name="boundY"> data boundary along y-axis </param>
        public Grid2DCplxData(long rows, long cols,
            double spacingY, double spacingX,
            double initVal,
            InterpolationMethod intrpl = Defaults.IntrplOption,
            DataBoundary boundX = Defaults.BoundaryOption,
            DataBoundary boundY = Defaults.BoundaryOption)
        {
            GridInfo = new(rows, cols, spacingY, spacingX);
            Values = new(rows, cols, initVal);
            Phase = new();
            BoundaryX = boundX;
            BoundaryY = boundY;
            IntrplMethod = intrpl;
        }

        /// <summary>
        /// constructs a GridMat with given count
        /// with a given initial value
        /// </summary>
        /// <param name="rows"> number of elements along y </param>
        /// <param name="cols"> number of elements along x </param>
        /// <param name="initVal"> the common initial value </param>
        /// <param name="intrpl"> interpolation method </param>
        /// <param name="boundX"> data boundary along x-axis </param>
        /// <param name="boundY"> data boundary along y-axis </param>
        public Grid2DCplxData(long rows, long cols, double initVal,
            InterpolationMethod intrpl = Defaults.IntrplOption,
            DataBoundary boundX = Defaults.BoundaryOption,
            DataBoundary boundY = Defaults.BoundaryOption)
        {
            GridInfo = new(rows, cols);
            Values = new(rows, cols, initVal);
            Phase = new();
            BoundaryX = boundX;
            BoundaryY = boundY;
            IntrplMethod = intrpl;
        }

        #endregion
        #region methods

        #region ----- Convert2Scat -----

        /// <summary>
        /// converts the grid matrix to scattered format
        /// with all then coordinates explicitly given
        /// </summary>
        /// <returns> result scatter matrix </returns>
        public Scat2DCplxData Convert2ScatFormat()
        {
            long n = GridInfo.Cols * GridInfo.Rows;
            VectorD pointsX = new(n, 0.0);
            VectorD pointsY = new(n, 0.0);
            VectorZ values = new(n, 0.0);
            VectorD xs = GridInfo.GetCoordinatesX();
            VectorD ys = GridInfo.GetCoordinatesY();
            LongRange allCols = new(0, GridInfo.Cols);

            for(long iRow = 0; iRow < GridInfo.Rows; iRow++)
            {
                LongRange iRng = new(iRow * GridInfo.Cols, (iRow + 1) * GridInfo.Cols);
                pointsX[iRng] = xs;
                pointsY[iRng] = new VectorD(GridInfo.Cols, ys[iRow]);
                values[iRng] = Values[iRow, allCols];
            }

            return new(scatInfo: new(pointsY, pointsX), values: values);
        }

        #endregion
        #region ----- Padding -----

        /// <summary>
        /// padding according to target parameters
        /// </summary>
        /// <param name="targetRows"> target number of rows after padding </param>
        /// <param name="targetCols"> target number of columns after padding </param>
        /// <param name="startRowIndex"> starting row index after padding </param>
        /// <param name="startColIndex"> starting column index after padding </param>
        /// <param name="paddingValue"> value used for the padding </param>
        /// <returns> result grid data after padding </returns>
        public Grid2DCplxData? Padding(long targetRows, long targetCols,
            long startRowIndex, long startColIndex,
            double paddingValue = 0.0)
        {
            // modifies values
            MatrixZ nv = Values.Padding(targetRows, targetCols,
                startRowIndex, startColIndex, paddingValue);
            if (nv == MatrixZ.Empty) { return null; }

            // modifies grid
            GridInfo2D ng = new(rows: targetRows, cols: targetCols,
                startY: GridInfo.StartY - startRowIndex * GridInfo.SpacingY,
                startX: GridInfo.StartX - startColIndex * GridInfo.SpacingX,
                spacingY: GridInfo.SpacingY, spacingX: GridInfo.SpacingX);
            // output
            return new(values: nv, gridInfo: ng);
        }

        /// <summary>
        /// centered zero-padding on each side
        /// </summary>
        /// <param name="targetRows"> target [even] number of rows after padding </param>
        /// <param name="targetCols"> target [even] number of columns after padding </param>
        /// <returns> result grid data after padding </returns>
        public Grid2DCplxData? Padding(long targetRows, long targetCols)
        {
            // modifies values
            MatrixZ nv = Values.Padding(targetRows, targetCols);
            if (nv == MatrixZ.Empty) { return null; }

            // modifies grid
            GridInfo2D ng = new(rows: targetRows, cols: targetCols,
                startY: GridInfo.StartY - (targetRows - GridInfo.Rows) / 2 * GridInfo.SpacingY,
                startX: GridInfo.StartX - (targetCols - GridInfo.Cols) / 2 * GridInfo.SpacingX,
                spacingY: GridInfo.SpacingY, spacingX: GridInfo.SpacingX);
            // output
            return new(values: nv, gridInfo: ng);
        }

        #endregion
        #region ----- FindValues -----

        /// <summary>
        /// evaluates value at a single location  
        /// </summary>
        /// <param name="x"> x-coordinate of the evaluation location </param>
        /// <param name="y"> y-coordinate of the evaluation location </param>
        /// <returns> interpolated value on the evaluation location </returns>
        public Complex FindValue(double x, double y)
            => Interpolation.FindValue(x, y);

        /// <summary>
        /// evaluates values on a set of scattered locations
        /// </summary>
        /// <param name="rho"> set of scattered locations for evaluation </param>
        /// <param name="loopMode"> computational option for loops </param>
        /// <returns> interpolated values on those evaluation locations </returns>
        public VectorZ FindValues(ScatInfo2D rho,
            LoopMode loopMode = Defaults.LoopOption)
            => Interpolation.Evaluate(rho: rho, loopMode: loopMode);

        /// <summary>
        /// evaluates values on a set of x/y-separable scattered locations
        /// </summary>
        /// <param name="xy"> set of x/y-separable scattered locations for evaluation </param>
        /// <param name="loopMode"> computational option for loops </param>
        /// <returns> interpolated values on those evaluation locations </returns>
        public MatrixZ FindValues(ScatInfoXY xy,
            LoopMode loopMode = Defaults.LoopOption)
            => Interpolation.Evaluate(xy: xy, loopMode: loopMode);

        /// <summary>
        /// evaluates values on a target grid
        /// </summary>
        /// <param name="targetGrid"> target grid for evaluation </param>
        /// <param name="loopMode"> computational option for loops </param>
        /// <returns> interpolated values on the target grid </returns>
        public MatrixZ FindValues(GridInfo2D targetGrid,
            LoopMode loopMode = Defaults.LoopOption)
            => Interpolation.Evaluate(targetGrid: targetGrid, loopMode: loopMode);

        #endregion
        #region ----- Sample Phase ----

        /// <summary>
        /// samples the phase and adds to the data
        /// (may cause under-sampling ...)
        /// <param name="loopMode"> loop-computational mode options </param>
        /// </summary>
        public void SamplePhase(LoopMode loopMode = Defaults.LoopOption)
        {
            MatrixZ t = Values;
            // modifies values
            Phase.AddTo(x: ref t, grid: GridInfo, 
                part: ComplexPart.Argument, loopMode: loopMode);
            // reset phase parameters
            Phase.C1x = 0.0;
            Phase.C1y = 0.0;
        }

        #endregion

        #endregion
    }

}
