namespace VEMS.EMSolver
{
    /// <summary>
    /// interface of array layout
    /// which is used like a "container"
    /// </summary>
    public interface IArrangement
    {
        #region properties

        /// <summary>
        /// array layout type
        /// </summary>
        ArrayType Arrangement { get; }

        ///// <summary>
        ///// Micro cell horizontal spacing
        ///// </summary>
        //double HorizontalSpacing { get; }

        ///// <summary>
        ///// Micro cell vertical spacing
        ///// </summary>
        //double VerticalSpacing { get; }

        #endregion
        #region methods

        /// <summary>
        /// local cooridnates mapping kernal
        /// </summary>
        /// <param name="x">global coordinate in x direction</param>
        /// <param name="y">global coordinate in y direction</param>
        /// <param name="error">boundary judgment error</param>
        /// <returns> (col, row, xLocal, yLocal) </returns>
        (int col, int row, double xLocal, double yLocal) MapCoordinates(double x, double y, double error);

        #endregion
    }

    /// <summary>
    /// base class for array layout method
    /// </summary>
    public class ArrayBase
    {
        #region properties

        /// <summary>
        /// array layout type
        /// </summary>
        public IArrangement Arrangement { get; }

        #endregion
        #region constructors

        /// <summary>
        /// universal constructor of ArrayBase
        /// </summary>
        /// <param name="arrangement">array layout type(rectangule or hexagon)</param>
        public ArrayBase(IArrangement arrangement)
        {
            // fundational parameters set
            Arrangement = arrangement;
        }

        /// <summary>
        /// convenient constructor of ArrayBase
        /// </summary>
        /// <param name="type"> array type option </param>
        /// <param name="rectangularSize"> size of rectangular grid </param>
        /// <param name="hexagonalSize"> size of hexagonal grid </param>
        public ArrayBase(
            ArrayType type,
            Size2D? rectangularSize = null,
            Size2D? hexagonalSize = null)
            : this(CreateArrangement(type, rectangularSize, hexagonalSize))
        { }

        #endregion
        #region methods

        /// <summary>
        /// creates arrangement based on the specified type and sizes
        /// </summary>
        /// <param name="type"> array type </param>
        /// <param name="rectangularSize"> size of rectangular grid </param>
        /// <param name="hexagonalSize"> size of hexagonal grid </param>
        /// <returns> arrangement </returns>
        /// <exception cref="NotImplementedException"></exception>
        private static IArrangement CreateArrangement(
            ArrayType type,
            Size2D? rectangularSize,
            Size2D? hexagonalSize)
        {
            return type switch
            {
                ArrayType.Rectangular => new RectangularArray(
                    rectangularSize ?? throw new ArgumentNullException(nameof(rectangularSize))),
                ArrayType.Hexagonal => new HexagonalArray(
                    hexagonalSize ?? throw new ArgumentNullException(nameof(hexagonalSize))),
                _ => throw new NotImplementedException("Unsupported arrangement type.")              
            };
        }

        #endregion
        #region ---- Size2D ----

        /// <summary>
        /// cell sizes class used for array definition
        /// </summary>
        public struct Size2D
        {
            #region properties

            /// <summary>
            /// micro cell size 
            /// along x direction
            /// </summary>
            public double X { get; set; }
            /// <summary>
            /// micro cell size
            /// along y direction
            /// </summary>
            public double Y { get; set; }

            #endregion
            #region construct

            /// <summary>
            /// Micro cell sizes of rectangular array layout
            /// </summary>
            /// <param name="x">x direction size</param>
            /// <param name="y">y direction size</param>
            public Size2D(double x, double y)
            {
                X = x;
                Y = y;
            }

            #endregion
        }

        #endregion
    }

    /// <summary>
    /// solving local coordinates under rectangular arrangement
    /// </summary>
    public sealed class RectangularArray : IArrangement
    {
        #region properties

        /// <summary>
        /// array layout type, implement interface property of Arrangement
        /// </summary>
        public ArrayType Arrangement { get => ArrayType.Rectangular; }

        /// <summary>
        /// horizontal spacing
        /// </summary>
        public double HorizontalSpacing { get; }

        /// <summary>
        /// vertical spacing
        /// </summary>
        public double VerticalSpacing { get; }

        #endregion
        #region constructor

        /// <summary>
        /// constructor of RectangularArray
        /// </summary>
        /// <param name="horizontalSpacing"> horizontal spacing </param>
        /// <param name="verticalSpacing"> vertical spacing </param>
        public RectangularArray(double horizontalSpacing, double verticalSpacing)
        {
            HorizontalSpacing = horizontalSpacing;
            VerticalSpacing   = verticalSpacing;
        }

        /// <summary>
        /// constructor of RectangularArray
        /// </summary>
        /// <param name="cellSize">Micro cell size of rectangular arrangement</param>
        public RectangularArray(ArrayBase.Size2D cellSize)
        {
            HorizontalSpacing = cellSize.X;
            VerticalSpacing   = cellSize.Y;
        }

        #endregion
        #region methods

        /// <summary>
        /// implement interface property of Mapcoordinates of RetangularArray
        /// </summary>
        /// <param name="x">global coordinate in x direction</param>
        /// <param name="y">global coordinate in y direction</param>
        /// <param name="tolerance"> tolerance used for boundary judgment </param>
        /// <returns> (col, row, xLocal, yLocal) </returns>
        public (int col, int row, double xLocal, double yLocal) MapCoordinates(double x, double y, 
            double tolerance = 1e-14)
        {
            // global coordinates
            double xglobal = x;
            double yglobal = y;

            // index of micro cell array elements
            int col = (int)(Math.Sign(xglobal) * Math.Floor((Math.Abs(xglobal) + HorizontalSpacing / 2 - tolerance) / HorizontalSpacing));
            int row = (int)(Math.Sign(yglobal) * Math.Floor((Math.Abs(yglobal) + VerticalSpacing   / 2 - tolerance) / VerticalSpacing));

            // mirco cell array's center coordinates (global)
            double xcenter = col * HorizontalSpacing;
            double ycenter = row * VerticalSpacing;

            // micro cell array's local coordinates (local)
            double xlocal = Math.Sign(xglobal) * (Math.Abs(xglobal) - Math.Abs(xcenter));
            double ylocal = Math.Sign(yglobal) * (Math.Abs(yglobal) - Math.Abs(ycenter));

            // return
            return (col, row, xlocal, ylocal);
        }
        
        #endregion
    }

    /// <summary>
    /// solving local coordinates under hexagonal arrangement
    /// </summary>
    public sealed class HexagonalArray : IArrangement
    {
        #region properties

        /// <summary>
        /// array layout type, implement interface property of Arrangement
        /// </summary>
        public ArrayType Arrangement { get => ArrayType.Hexagonal; }

        /// <summary>
        /// Micro cell horizontal spacing, implement interface property of HorizontalSpacing
        /// </summary>
        internal double HorizontalSpacing { get; }

        /// <summary>
        /// Micro cell vertical spacing, implement interface property of HorizontalSpacing
        /// </summary>
        internal double VerticalSpacing { get; }

        /// <summary>
        /// spacing
        /// </summary>
        public double Spacing { get; }

        #endregion
        #region constructor

        /// <summary>
        /// constructor of HexagonalArray
        /// </summary>
        /// <param name="spacing"> spacing </param>
        public HexagonalArray(double spacing)
        {
            Spacing = spacing;
        }

        /// <summary>
        /// constructor of HexagonalArray
        /// </summary>
        /// <param name="cellSize">circumscribed circle diameter</param>
        internal HexagonalArray(ArrayBase.Size2D cellSize)
        {
            //HorizontalSpacing = cellSize.X;
            //VerticalSpacing = cellSize.Y;
        }

        #endregion
        #region methods

        /// <summary>
        /// implement interface property of Mapcoordinates of HexagonalArray
        /// </summary>
        /// <param name="x">global coordinate in x direction</param>
        /// <param name="y">global coordinate in y direction</param>
        /// <param name="tolerance"> tolerance used for boundary judgment </param>
        /// <returns> (col, row, xLocal, yLocal) </returns>
        public (int col, int row, double xLocal, double yLocal) MapCoordinates(double x, double y, 
            double tolerance = 1e-14)
        {
            // boundary judgement error
            //double delta = VerticalSpacing * 3.0 / 4.0;
            double delta = Spacing * 3.0 / 4.0;

            // translate from Cartesian to Cube coordinate
            double ycube = +Math.Sqrt(3) / 2 * x + 0.5 * y;
            double xcube = -Math.Sqrt(3) / 2 * x + 0.5 * y;
            double zcube = -y;

            // cube index calculation
            int q = (int)(Math.Round((xcube - tolerance) / delta));
            int r = (int)(Math.Round((ycube - tolerance) / delta));
            int s = (int)(Math.Round((zcube - tolerance) / delta));
            double q_diff = Math.Abs(q * delta - xcube);
            double r_diff = Math.Abs(r * delta - ycube);
            double s_diff = Math.Abs(s * delta - zcube);

            // index correction
            if (q_diff > r_diff && q_diff > s_diff)
            { q = -r - s; }
            else if (r_diff > s_diff)
            { r = -q - s; }
            else
            { s = -q - r; }

            // global to local in cube coordinate
            double xcube_local = xcube - q * delta;
            double ycube_local = ycube - r * delta;
            double zcube_local = zcube - s * delta;

            // return to cartesion coordinate
            double xlocal = (xcube_local - ycube_local) / Math.Sqrt(3);
            double ylocal = (xcube_local + ycube_local);

            // return
            return (q, r, xlocal, ylocal);
        }

        #endregion
    }

    /// <summary>
    /// array layout method
    /// </summary>
    public enum ArrayType
    {
        /// <summary>
        /// Rectangular array layout
        /// </summary>
        Rectangular,

        /// <summary>
        /// Hexagonal array layout
        /// </summary>
        Hexagonal
    }

}
