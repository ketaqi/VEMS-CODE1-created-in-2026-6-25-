using ScottPlot.Plottables;
using ScottPlot.WPF;
using VEMS.MathCore;

namespace VEMS.Plot5
{

    /// <summary>
    /// complex-valued 1D grid plot
    /// </summary>
    public class Grid1DCplxPlot : Grid1DCplxData
    {

        #region properties

        /// <summary>
        /// reference frame from WpfPlot
        /// </summary>
        private WpfPlot Dock { get; set; }

        /// <summary>
        /// Plottable object from ScottPlot
        /// </summary>
        public Signal Plot { get; set; }

        /// <summary>
        /// collection of plot visual properties
        /// </summary>
        public Grid1DCplxPlotPrty PlotProperty { get; set; }

        #endregion
        #region constructors

        /// <summary>
        /// constructs a complex-valued plot by adding data 
        /// to a reference frame
        /// </summary>
        /// <param name="dock"> reference frame from WpfPlot </param>
        /// <param name="values"> complex-valued data to plot </param>
        /// <param name="grid"> uniform sampling information </param>
        /// <param name="plotProperty"> collection of plot properties </param>
        public Grid1DCplxPlot(WpfPlot dock,
            VectorZ values,
            GridInfo1D? grid = null,
            Grid1DCplxPlotPrty? plotProperty = null)
            : base(values)
        {
            // basic properties
            Dock = dock;
            GridInfo = (grid == null) ? new(values.Count) : new(grid);
            PlotProperty = plotProperty?? new();

            #region data plot

            // takes part of the complex data
            VectorD v = TakePart(values, PlotProperty.ComplexPart);
            // adds plot to the reference frame
            Plot = Dock.Plot.Add.Signal(ys: VMath.ConvertVector2Array(v));
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
        /// takes selected part from input complex data
        /// </summary>
        /// <param name="values"> input complex data </param>
        /// <param name="part"> selected part: real, imaginary, magnitude, phase </param>
        /// <returns> selected part of the data </returns>
        private VectorD TakePart(VectorZ values,
            ComplexPart? part = null)
        {
            VectorD v = part switch
            {
                null => VMath.RealPart(values),
                ComplexPart.RealPart => VMath.RealPart(values),
                ComplexPart.ImagPart => VMath.ImagPart(values),
                ComplexPart.Magnitude => VMath.Abs(values),
                ComplexPart.Argument => VMath.Arg(values),
                _ => VMath.RealPart(values)
            };
            return v;
        }

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
            if (disp == null) { return; }
            Plot.Axes.YAxis = disp switch
            {
                DisplayOption.Hidden => Dock.Plot.Axes.Left,
                DisplayOption.Left => Dock.Plot.Axes.Left,
                DisplayOption.Right => Dock.Plot.Axes.Right,
                _ => Dock.Plot.Axes.Left
            };
        }


        ///// <summary>
        ///// switches the selected part of complex data
        ///// </summary>
        ///// <param name="targetPart"> target part of complex data </param>
        //public void SwitchPart(ComplexPart targetPart)
        //{
        //    if(targetPart == Part) { return; }
        //    Part = targetPart;
        //    // finds the index of current plot
        //    int idx = Frame.Plot.PlottableList.IndexOf(Plot);
        //    if(idx < 0) { throw new IndexOutOfRangeException(); }
        //    // saves current parameters
        //    float width = Plot.LineWidth;
        //    ScottPlot.LinePattern pattern = Plot.LineStyle.Pattern;
        //    ScottPlot.Color color = Plot.Color;
        //    bool isVisible = Plot.IsVisible;
        //    ScottPlot.IAxes axes = Plot.Axes;
        //    string? label = Plot.Label;
        //    // removes current from the list
        //    Frame.Plot.Remove(Plot);
        //    // makes a new plot according to the target part
        //    VectorD v = TakePart(Values, targetPart);
        //    Plot = new(new SignalSourceDouble(ys: VMath.ConvertVectorToArray(v),
        //        period: Grid.Spacing));
        //    // copies properties from the previous plot
        //    Plot.LineWidth = width;
        //    Plot.LineStyle.Pattern = pattern;
        //    Plot.Color = color;
        //    Plot.IsVisible = isVisible;
        //    Plot.Axes = axes;
        //    Plot.Label = label;
        //    // inserts back to the list
        //    Frame.Plot.PlottableList.Insert(idx, Plot);
        //}

        #endregion

    }
}
