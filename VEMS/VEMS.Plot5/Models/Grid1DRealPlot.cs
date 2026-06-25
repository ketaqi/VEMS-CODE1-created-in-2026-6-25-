using ScottPlot.Plottables;
using ScottPlot.WPF;
using VEMS.MathCore;

namespace VEMS.Plot5
{

    /// <summary>
    /// real-valued 1D grid plot
    /// </summary>
    public class Grid1DRealPlot : Grid1DRealData
    {

        #region properties

        /// <summary>
        /// reference frame from WpfPlot
        /// </summary>
        public WpfPlot Dock { get; set; }

        /// <summary>
        /// Plottable object from ScottPlot
        /// </summary>
        public Signal Plot { get; set; }

        /// <summary>
        /// collection of plot visual properties
        /// </summary>
        public Grid1DRealPlotPrty PlotProperty { get; set; }

        #endregion
        #region constructors

        /// <summary>
        /// constructs a real-valued plot by adding data 
        /// to a reference frame
        /// </summary>
        /// <param name="dock"> reference frame from WpfPlot </param>
        /// <param name="values"> real-valued data to plot </param>
        /// <param name="grid"> uniform sampling information </param>
        /// <param name="plotProperty"> collection of plot properties </param>
        public Grid1DRealPlot(WpfPlot dock,
            VectorD values,
            GridInfo1D? grid = null,
            Grid1DRealPlotPrty? plotProperty = null)
            : base(values)
        {
            // basic properties
            Dock = dock;
            GridInfo = (grid == null) ? new(values.Count) : new(grid);
            PlotProperty = plotProperty ?? new();

            #region data plot

            // adds plot to the reference frame
            Plot = Dock.Plot.Add.Signal(ys: VMath.ConvertVectorToArray(Values));
            // sets sampling info
            Plot.Data.Period = GridInfo.Spacing;
            Plot.Data.XOffset = GridInfo.Start;

            #endregion
            #region visual properties

            // handles line parameters
            if (PlotProperty.LineWidth != null) { Plot.LineWidth = (float)PlotProperty.LineWidth; }
            Plot.LineStyle.Pattern = Helpers.ParseLinePattern(PlotProperty.LinePattern);
            Plot.Color = Helpers.ParseColor(PlotProperty.PlotColor);
            Plot.Label = PlotProperty.Label;
            // handles visibility option and y-axis
            PlotVisibilityChange(PlotProperty.DisplayOption);
            PlotYAxisChange(PlotProperty.DisplayOption);

            #endregion
        }

        #endregion
        #region methods

        /// <summary>
        /// changes the visibility of the plot according to the display option
        /// </summary>
        /// <param name="disp"> display option </param>
        private void PlotVisibilityChange(DisplayOption? disp)
        {
            if (disp == null) { return; }
            Plot.IsVisible = disp switch
            {
                DisplayOption.Hidden => false,
                DisplayOption.Left => true,
                DisplayOption.Right => true,
                _ => true
            };
        }

        /// <summary>
        /// changes y-axis of the plot according to the display option
        /// </summary>
        /// <param name="disp"> display option </param>
        private void PlotYAxisChange(DisplayOption? disp)
        {
            if(disp == null) { return; }
            Plot.Axes.YAxis = disp switch
            {
                DisplayOption.Hidden => Dock.Plot.Axes.Left,
                DisplayOption.Left => Dock.Plot.Axes.Left,
                DisplayOption.Right => Dock.Plot.Axes.Right,
                _ => Dock.Plot.Axes.Left
            };
        }

        /// <summary>
        /// sets the properties of the line
        /// </summary>
        /// <param name="lineWidth"></param>
        /// <param name="linePattern"></param>
        /// <param name="plotColor"></param>
        /// <param name="disp"></param>
        /// <param name="label"></param>
        public void SetPlotProperty(double? lineWidth = null, 
            LinePattern? linePattern = null, 
            PlotColor? plotColor = null,
            DisplayOption? disp = null,
            string? label = null)
        {
            if (lineWidth != null)
            {
                Plot.LineWidth = (float)lineWidth;
                PlotProperty.LineWidth = lineWidth;
            }
            if (linePattern != null)
            {
                Plot.LineStyle.Pattern = Helpers.ParseLinePattern(linePattern);
                PlotProperty.LinePattern = linePattern;
            }
            if (plotColor != null)
            {
                Plot.Color = Helpers.ParseColor(plotColor);
                PlotProperty.PlotColor = plotColor;
            }
            if (label != null)
            {
                Plot.Label = label;
                PlotProperty.Label = label;
            }
            if(disp != null)
            {
                // handles visibility option and y-axis
                PlotVisibilityChange(disp);
                PlotYAxisChange(disp);
                PlotProperty.DisplayOption = disp;
            }
        }



        #endregion

    }
}
