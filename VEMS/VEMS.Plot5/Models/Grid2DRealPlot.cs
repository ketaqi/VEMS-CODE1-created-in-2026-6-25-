using ScottPlot.Panels;
using ScottPlot.Plottables;
using ScottPlot.WPF;
using VEMS.MathCore;

namespace VEMS.Plot5
{

    /// <summary>
    /// real-valued 2D grid plot
    /// </summary>
    public class Grid2DRealPlot : Grid2DRealData
    {

        #region properties

        /// <summary>
        /// reference frame from WpfPlot
        /// </summary>
        private WpfPlot Frame {  get; set; }

        /// <summary>
        /// Plottable object from ScottPlot
        /// </summary>
        public Heatmap Plot { get; set; }

        /// <summary>
        /// ColorBar of the graph
        /// </summary>
        public ColorBar CBar {  get; set; }

        #endregion
        #region constructors

        /// <summary>
        /// constructs a plot by adding data to a reference frame
        /// </summary>
        /// <param name="frame"> reference frame from WpfPlot </param>
        /// <param name="values"> real-valued 2D data </param>
        /// <param name="grid"> data sampling information </param>
        /// <param name="colormap"> colormap of the graph </param>
        public Grid2DRealPlot(ref WpfPlot frame, MatrixD values,
            GridInfo2D? grid = null,
            PlotColormap? colormap = null)
            //GraphInterpolationMode? smooth = null,
            //string? label = null) 
            : base(values)
        {
            Frame = frame;
            // grid handling
            if (grid != null) { GridInfo = new(grid); }
            else { GridInfo = new(values.Rows, values.Cols); }
            // adds plot to the reference frame
            Plot = Frame.Plot.Add.Heatmap(intensities: VMath.ConvertMatrixToArray(values, revRows: true));
            // sets sampling info
            Plot.Extent = new(left: GridInfo.StartX, right: GridInfo.EndX + GridInfo.SpacingX,
                bottom: GridInfo.StartY, top: GridInfo.EndY + GridInfo.SpacingY);
            // adds colorbar
            CBar = Frame.Plot.Add.ColorBar(Plot);
            CBar.Width = (float)25.0;
            // sets other parameters
            Plot.Colormap = Helpers.ParseColormap(colormap);
        }

        #endregion
        #region methods

        // ...


        #endregion

    }
}
