using Complex = System.Numerics.Complex;

namespace VEMS.MathCore
{

    /// <summary>
    /// finite-difference for 1D real-valued grid data
    /// </summary>
    public class Grid1DRealFiDi
    {
        #region properties

        private long Count { get; set; }

        /// <summary>
        /// finds derivative value at specific grid index 
        /// <para> arg #1: index that specifies a grid location </para>
        /// <para> result: derivative value at this location </para>
        /// </summary>
        public Func<long, double>? FindValue { get; set; }

        #endregion
        #region constructors

        /// <summary>
        /// default constructor
        /// </summary>
        internal Grid1DRealFiDi() { }

        /// <summary>
        /// constructs a Grid1DRealFiDi class
        /// </summary>
        /// <param name="vs"> function values stored in a vector </param>
        /// <param name="grid"> sampling grid of the function values </param>
        /// <param name="option"> derivative option </param>
        public Grid1DRealFiDi(VectorD vs, 
            GridInfo1D? grid = null,
            FiDi1DOption option = FiDi1DOption.Dt)
        {
            if(grid != null && vs.Count != grid.Count) { throw new ArgumentException(); }
            
            Count = vs.Count;
            FindValue = option switch
            {
                FiDi1DOption.Dt => (i) => PointFiDi.Dt(vs, i, grid, checkBound: false),
                FiDi1DOption.Dtt => (i) => PointFiDi.Dtt(vs, i, grid, checkBound: false),
                _ => (i) => PointFiDi.Dt(vs, i, grid, checkBound: false)
            };
        }

        #endregion
        #region methods

        /// <summary>
        /// evaluates the derivative values at
        /// all the grid locations 
        /// </summary>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> resulting derivative values at all grid locations </returns>
        public VectorD EvaluateAll(LoopMode loopMode = Defaults.LoopOption)
        {
            if(FindValue == null) { throw new ArgumentNullException(nameof(FindValue)); }
            
            // initialize
            VectorD ve = new(Count);
            // defines loop operation
            Loop1D loop = new(operation: (i) => ve[i, false] = FindValue(i), 
                start: 0, end: Count, step: 1);
            loop.Evaluate(mode: loopMode);

            // return
            return ve;
        }

        #endregion
    }

    /// <summary>
    /// finite-difference for 1D complex-valued grid data
    /// </summary>
    public class Grid1DCplxFiDi
    {
        #region properties

        private long Count { get; set; }

        /// <summary>
        /// finds derivative value at specific grid index 
        /// <para> arg #1: index that specifies a grid location </para>
        /// <para> result: derivative value at this location </para>
        /// </summary>
        public Func<long, Complex>? FindValue { get; set; }

        #endregion
        #region constructors

        /// <summary>
        /// default constructor
        /// </summary>
        internal Grid1DCplxFiDi() { }

        /// <summary>
        /// constructs a Grid1DCplxFiDi class
        /// </summary>
        /// <param name="vs"> function values stored in a vector </param>
        /// <param name="grid"> sampling grid of the function values </param>
        /// <param name="option"> derivative option </param>
        public Grid1DCplxFiDi(VectorZ vs, 
            GridInfo1D? grid = null,
            FiDi1DOption option = FiDi1DOption.Dt)
        {
            if (grid != null && vs.Count != grid.Count) { throw new ArgumentException(); }
            
            Count = vs.Count;
            FindValue = option switch
            {
                FiDi1DOption.Dt => (i) => PointFiDi.Dt(vs, i, grid, checkBound: false),
                FiDi1DOption.Dtt => (i) => PointFiDi.Dtt(vs, i, grid, checkBound: false),
                _ => (i) => PointFiDi.Dt(vs, i, grid, checkBound: false)
            };
        }

        #endregion
        #region methods

        /// <summary>
        /// evaluates the derivative values at
        /// all the grid locations 
        /// </summary>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> resulting derivative values at all grid locations </returns>
        public VectorZ EvaluateAll(LoopMode loopMode = Defaults.LoopOption)
        {
            if(FindValue == null) { throw new ArgumentNullException(nameof(FindValue)); }

            // initialize
            VectorZ ve = new(Count);
            // defines loop operation
            Loop1D loop = new(operation: (i) => ve[i, false] = FindValue(i),
                start: 0, end: Count, step: 1);
            loop.Evaluate(mode: loopMode);

            // return
            return ve;
        }

        #endregion
    }

    /// <summary>
    /// finite-difference for 2D real-valued grid data
    /// </summary>
    public class Grid2DRealFiDi
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
        public Func<long, long, double>? FindValue { get; set; }

        #endregion
        #region constructors

        /// <summary>
        /// default constructor
        /// </summary>
        internal Grid2DRealFiDi() { }

        /// <summary>
        /// constructs a Grid1DRealFiDi class
        /// </summary>
        /// <param name="vs"> function values stored in a vector </param>
        /// <param name="grid"> sampling grid of the function values </param>
        /// <param name="option"> derivative option </param>
        public Grid2DRealFiDi(MatrixD vs,
            GridInfo2D? grid = null,
            FiDi2DOption option = FiDi2DOption.Dx)
        {
            if (grid != null && (vs.Rows != grid.Rows || vs.Cols != grid.Cols)) 
            { throw new ArgumentException(); }
            Rows = vs.Rows;
            Cols = vs.Cols;
            FindValue = option switch
            {
                FiDi2DOption.Dx => (iRow, iCol) => PointFiDi.Dx(vs, iRow, iCol, grid, checkBound: false),
                FiDi2DOption.Dy => (iRow, iCol) => PointFiDi.Dy(vs, iRow, iCol, grid, checkBound: false),
                FiDi2DOption.Dxx => (iRow, iCol) => PointFiDi.Dxx(vs, iRow, iCol, grid, checkBound: false),
                FiDi2DOption.Dyy => (iRow, iCol) => PointFiDi.Dyy(vs, iRow, iCol, grid, checkBound: false),
                FiDi2DOption.Dxy => (iRow, iCol) => PointFiDi.Dxy(vs, iRow, iCol, grid, checkBound: false),
                _ => (iRow, iCol) => PointFiDi.Dx(vs, iRow, iCol, grid, checkBound: false)
            }; 
        }

        #endregion
        #region methods

        /// <summary>
        /// evaluates the derivative values at
        /// all the grid locations 
        /// </summary>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> resulting derivative values at all grid locations </returns>
        public MatrixD EvaluateAll(LoopMode loopMode = Defaults.LoopOption)
        {
            if(FindValue == null) { throw new ArgumentNullException(nameof(FindValue)); }

            // initialize
            MatrixD ve = new(rows: Rows, cols: Cols);
            // defines loop operation
            Loop2D loop = new(operation: (iRow, iCol) => ve[iRow, iCol, false] = FindValue(iRow, iCol),
                rowStart: 0, rowEnd: Rows,
                colStart: 0, colEnd: Cols,
                rowStep: 1, colStep: 1);
            loop.Evaluate();

            // return
            return ve;
        }

        #endregion
    }

    /// <summary>
    /// finite-difference for 2D complex-valued grid data
    /// </summary>
    public class Grid2DCplxFiDi
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
        public Func<long, long, Complex>? FindValue { get; set; }

        #endregion
        #region constructors

        /// <summary>
        /// default constructor
        /// </summary>
        internal Grid2DCplxFiDi() { }

        /// <summary>
        /// constructs a Grid1DRealFiDi class
        /// </summary>
        /// <param name="vs"> function values stored in a vector </param>
        /// <param name="grid"> sampling grid of the function values </param>
        /// <param name="option"> derivative option </param>
        public Grid2DCplxFiDi(MatrixZ vs, 
            GridInfo2D? grid = null,
            FiDi2DOption option = FiDi2DOption.Dx)
        {
            if (grid != null && (vs.Rows != grid.Rows || vs.Cols != grid.Cols))
            { throw new ArgumentException(); }
            Rows = vs.Rows;
            Cols = vs.Cols;
            FindValue = option switch
            {
                FiDi2DOption.Dx => (iRow, iCol) => PointFiDi.Dx(vs, iRow, iCol, grid, checkBound: false),
                FiDi2DOption.Dy => (iRow, iCol) => PointFiDi.Dy(vs, iRow, iCol, grid, checkBound: false),
                FiDi2DOption.Dxx => (iRow, iCol) => PointFiDi.Dxx(vs, iRow, iCol, grid, checkBound: false),
                FiDi2DOption.Dyy => (iRow, iCol) => PointFiDi.Dyy(vs, iRow, iCol, grid, checkBound: false),
                FiDi2DOption.Dxy => (iRow, iCol) => PointFiDi.Dxy(vs, iRow, iCol, grid, checkBound: false),
                _ => (iRow, iCol) => PointFiDi.Dx(vs, iRow, iCol, grid, checkBound: false)
            };
        }

        #endregion
        #region methods

        /// <summary>
        /// evaluates the derivative values at
        /// all the grid locations 
        /// </summary>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> resulting derivative values at all grid locations </returns>
        public MatrixZ EvaluateAll(LoopMode loopMode = Defaults.LoopOption)
        {
            if(FindValue == null) { throw new ArgumentNullException(nameof(FindValue)); }

            // initialize
            MatrixZ ve = new(rows: Rows, cols: Cols);
            // define loop operation
            Loop2D loop = new(operation: (iRow, iCol) => ve[iRow, iCol, false] = FindValue(iRow, iCol),
                rowStart: 0, rowEnd: Rows,
                colStart: 0, colEnd: Cols,
                rowStep: 1, colStep: 1);
            loop.Evaluate();

            // return
            return ve;
        }

        #endregion
    }

}
 