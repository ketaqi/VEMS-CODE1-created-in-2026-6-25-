using ScottPlot;
using ScottPlot.Plottable;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Controls;
using VEMS.MathCore;
using PlotColor = VEMS.Plot.Options.PlotColor;
using LineStyle = VEMS.Plot.Options.LineStyle;
using MarkerShape = VEMS.Plot.Options.MarkerShape;
using VisualOption = VEMS.Plot.Options.VisualOption;
using Image = ScottPlot.Plottable.Image;
using Complex = System.Numerics.Complex;
using static VEMS.Plot.Options;

namespace VEMS.Plot
{
    /// <summary>
    /// Frame1D.xaml 的交互逻辑
    /// </summary>
    public partial class FrameLite : UserControl
    {
        #region defaults

        // view ranges
        internal double defaultXMin = -2.0;
        internal double defaultXMax = 2.0;
        internal double defaultYMin = -0.5;
        internal double defaultYMax = 3.5;

        // axis ticks
        internal double defaultXTickDensity = 1.0;
        internal double defaultYTickDensity = 1.0;
        internal double defaultXTickSize = 12.0;
        internal double defaultYTickSize = 12.0;

        // figure texts
        internal string defaultTitle = "VEMS Plot(s)";
        internal string defaultLabelX = "X";
        internal string defaultLabelY = "Y";
        internal double defaultTitleSize = 20.0;
        internal double defaultLabelXSize = 18.0;
        internal double defaultLabelYSize = 18.0;

        #endregion

        #region properties

        /// <summary>
        /// list of all the data for the plots
        /// </summary>
        public List<Object> AllData { get; set; }

        /// <summary>
        /// list of all the plots in the frame
        /// </summary>
        public List<Object> AllPlots { get; set; }

        /// <summary>
        /// list of all the colorbars in the frame
        /// </summary>
        public List<Colorbar> AllColorbars { get; set; }

        /// <summary>
        /// list of all the parts of the data
        /// </summary>
        public List<ComplexPart> AllParts { get; set; }

        /// <summary>
        /// flag whether user specified x-axis limits
        /// </summary>
        public bool UserSpecifiedXAxisLimits { get; set; } = false;

        /// <summary>
        /// flag whether user specified y-axis limits
        /// </summary>
        public bool UserSpecifiedYAxisLimits { get; set; } = false;

        /// <summary>
        /// horizontal span with lines at both sides
        /// </summary>
        public VSpanWithLines VSpanLines { get; set; }

        /// <summary>
        /// vertical span with lines at both sides
        /// </summary>
        public HSpanWithLines HSpanLines { get; set; }

        /// <summary>
        /// Displays the cross axis where the mouse is located
        /// </summary>
        public Crosshair MouseTrack { get; set; }

        #endregion
        #region constructors

        /// <summary>
        /// constructs a Frame1D class
        /// with default properties
        /// </summary>
        public FrameLite()
        {
            InitializeComponent();

            // set the view range by default
            SetXAxisLimits(min: defaultXMin, max: defaultXMax);
            SetYAxisLimits(min: defaultYMin, max: defaultYMax);
            // default limits => not user specified !!!
            UserSpecifiedXAxisLimits = false;
            UserSpecifiedYAxisLimits = false;

            // axis ticks
            SetXAxisTicks(tickDensity: defaultXTickDensity, fontSize: defaultXTickSize);
            SetYAxisTicks(tickDensity: defaultYTickDensity, fontSize: defaultYTickSize);

            // frame text by default
            SetTitle(content: defaultTitle, fontSize: defaultTitleSize);
            SetLabelX(content: defaultLabelX, fontSize: defaultLabelXSize);
            SetLabelY(content: defaultLabelY, fontSize: defaultLabelYSize);

            // plot frame settings
            PlotFrame.RightClicked -= PlotFrame.DefaultRightClickEvent;

            // initialize plot list
            AllData = new();
            AllPlots = new();
            AllColorbars = new();
            AllParts = new();

            // initialize spans
            VSpanLines = new(PlotFrame, isVisible: false);
            HSpanLines = new(PlotFrame, isVisible: false);

            //Crosshair
            MouseTrack = PlotFrame.Plot.AddCrosshair(0, 0);
            MouseTrack.IsVisible = false;
            // set the default color palette
            PlotFrame.Plot.Palette = Palette.Category10;
            // ...

        }

        /// <summary>
        /// constructs a Frame1D class, with
        /// optionally given properties
        /// </summary>
        /// <param name="title"> title of the frame </param>
        /// <param name="titleSize"> font size of the title </param>
        /// <param name="labelX"> horizontal label of the frame </param>
        /// <param name="labelXSize"> font size of the horizontal label </param>
        /// <param name="labelY"> vertical label of the frame </param>
        /// <param name="labelYSize"> font size of the vertical label </param>
        /// <param name="xTickDensity"> tick density of the horizontal axis </param>
        /// <param name="xTickSize"> font size of horizontal ticks </param>
        /// <param name="yTickDensity"> tick density of the vertical axis </param>
        /// <param name="yTickSize"> font size of the vertical ticks </param>
        /// <param name="xMin"> minimum view range in the horizontal direction </param>
        /// <param name="xMax"> maximum view range in the horizontal direction </param>
        /// <param name="yMin"> minimum view range in the vertical direction </param>
        /// <param name="yMax"> maximum view range in the vertical direction </param>
        public FrameLite(string? title = null, double? titleSize = null,
            string? labelX = null, double? labelXSize = null, 
            string? labelY = null, double? labelYSize = null,
            double? xTickDensity = null, double? xTickSize = null,
            double? yTickDensity = null, double? yTickSize = null,
            double? xMin = null, double? xMax = null, 
            double? yMin = null, double? yMax = null)
        {
            InitializeComponent();

            // set the view range by default
            SetXAxisLimits(min: xMin ?? defaultXMin, max: xMax ?? defaultXMax);
            SetYAxisLimits(min: yMin ?? defaultYMin, max: yMax ?? defaultYMax);
            UserSpecifiedXAxisLimits = (xMin != null || xMax != null) ? true : false;
            UserSpecifiedYAxisLimits = (yMin != null || yMax != null) ? true : false;

            // axis ticks
            SetXAxisTicks(tickDensity: xTickDensity ?? defaultXTickDensity, fontSize: xTickSize ?? defaultXTickSize);
            SetYAxisTicks(tickDensity: yTickDensity ?? defaultYTickDensity, fontSize: yTickSize ?? defaultYTickSize);

            // frame text by default
            SetTitle(content: title ?? defaultTitle, fontSize: titleSize ?? defaultTitleSize);
            SetLabelX(content: labelX ?? defaultLabelX, fontSize: labelXSize ?? defaultLabelXSize);
            SetLabelY(content: labelY ?? defaultLabelY, fontSize: labelYSize ?? defaultLabelYSize);
            //SetLabelYRight(content: "");

            // plot frame settings
            PlotFrame.RightClicked -= PlotFrame.DefaultRightClickEvent;

            // initialize plot list
            AllData = new();
            AllPlots = new();
            AllColorbars = new();
            AllParts = new();
        }

        #endregion
        #region methods

        #region ------- frame -------

        #region ===== ticks =====

        /// <summary>
        /// sets the horizontal axis 
        /// by defining tick density
        /// </summary>
        /// <param name="enableGrid"> enable grid </param>
        /// <param name="enableTicks"> enable ticks </param>
        /// <param name="tickDensity"> density of the ticks </param>
        /// <param name="fontBold"> whether to set the ticks bold </param>
        /// <param name="fontSize"> font size of the ticks </param>
        /// <param name="fontFamily"> font family of the ticks </param>
        /// <param name="fontColor"> color of the ticks </param>
        public void SetXAxisTicks(bool enableGrid = true,
            bool enableTicks = true,
            double tickDensity = FrameDefaults.TickDensity,
            bool fontBold = false,
            double fontSize = FrameDefaults.TickSize,
            string fontFamily = FrameDefaults.FontFamily,
            PlotColor fontColor = FrameDefaults.Color)
            => FrameCommons.SetXAxisTicks(PlotFrame, 
                enableGrid, enableTicks, tickDensity,
                fontBold, fontSize, fontFamily, fontColor);

        /// <summary>
        /// sets the vertical axis on the left
        /// by defining tick density
        /// </summary>
        /// <param name="enableGrid"> enable grid </param>
        /// <param name="enableTicks"> enable ticks </param>
        /// <param name="tickDensity"> density of the ticks </param>
        /// <param name="fontBold"> whether to set the ticks bold </param>
        /// <param name="fontSize"> font size of the ticks </param>
        /// <param name="fontFamily"> font family of the ticks </param>
        /// <param name="fontColor"> color of the ticks </param>
        public void SetYAxisTicks(bool enableGrid = true,
            bool enableTicks = true,
            double tickDensity = FrameDefaults.TickDensity,
            bool fontBold = false,
            double fontSize = FrameDefaults.TickSize,
            string fontFamily = FrameDefaults.FontFamily,
            PlotColor fontColor = FrameDefaults.Color)
            => FrameCommons.SetYAxisTicks(PlotFrame, 
                enableGrid, enableTicks, tickDensity,
                fontBold, fontSize, fontFamily, fontColor);

        /// <summary>
        /// sets the vertical axis on the right
        /// by defining tick density
        /// </summary>
        /// <param name="enableGrid"> enable grid </param>
        /// <param name="enableTicks"> enable ticks </param>
        /// <param name="tickDensity"> density of the ticks </param>
        /// <param name="fontBold"> whether to set the ticks bold </param>
        /// <param name="fontSize"> font size of the ticks </param>
        /// <param name="fontFamily"> font family of the ticks </param>
        /// <param name="fontColor"> color of the ticks </param>
        public void SetYRightAxisTicks(bool enableGrid = true,
            bool enableTicks = true,
            double tickDensity = FrameDefaults.TickDensity,
            bool fontBold = false,
            double fontSize = FrameDefaults.TickSize,
            string fontFamily = FrameDefaults.FontFamily,
            PlotColor fontColor = FrameDefaults.Color)
            => FrameCommons.SetYRightAxisTicks(PlotFrame,
                enableGrid, enableTicks, tickDensity,
                fontBold, fontSize, fontFamily, fontColor);

        #endregion
        #region ===== range =====

        /// <summary>
        /// sets the horizontal axis limits 
        /// </summary>
        /// <param name="min"> minimum value </param>
        /// <param name="max"> maximum value </param>
        public void SetXAxisLimits(double min, double max)
        {
            FrameCommons.SetXAxisLimits(PlotFrame, min, max);
            UserSpecifiedXAxisLimits = true;
        }

        /// <summary>
        /// sets the vertical axis limits on the left
        /// </summary>
        /// <param name="min"> minimum value </param>
        /// <param name="max"> maximum value </param>
        public void SetYAxisLimits(double min, double max)
        {
            FrameCommons.SetYAxisLimits(PlotFrame, min, max);
            UserSpecifiedYAxisLimits = true;
        }

        /// <summary>
        /// sets the vertical axis limits on the right
        /// </summary>
        /// <param name="min"> minimum value </param>
        /// <param name="max"> maximum value </param>
        public void SetYRightAxisLimits(double min, double max)
        {
            FrameCommons.SetYRightAxisLimits(PlotFrame, min, max);
            UserSpecifiedYAxisLimits = true;
        }

        /// <summary>
        /// detects the axis limits automatically
        /// </summary>
        /// <param name="xMin"> minimum value along horizontal direction </param>
        /// <param name="xMax"> maximum value along horizontal direction </param>
        /// <param name="yMin"> minimum value along vertical direction </param>
        /// <param name="yMax"> maximum value along vertical direction </param>
        public void DetectAxisLimits(out double xMin, out double xMax,
            out double yMin, out double yMax, out double yRightMin, out double yRightMax)
            => FrameCommons.DetectAxisLimits(PlotFrame,
                out xMin, out xMax, out yMin, out yMax, out yRightMin, out yRightMax);

        /// <summary>
        /// detects the x-axis limits automatically
        /// </summary>
        /// <returns> (xMin, xMax) </returns>
        public (double, double) DetectXAxisLimits()
            => FrameCommons.DetectXAxisLimits(PlotFrame);

        /// <summary>
        /// detects the y-axis limits automatically
        /// </summary>
        /// <returns> (yMin, yMax) </returns>
        public (double, double) DetectYAxisLimits()
            => FrameCommons.DetectYAxisLimits(PlotFrame);

        /// <summary>
        /// detects the y-axis (right) limits automatically
        /// </summary>
        /// <returns> (yMin, yMax) </returns>
        public (double, double) DetectYRightAxisLimits()
            => FrameCommons.DetectYRightAxisLimits(PlotFrame);

        #endregion
        #region ===== scale =====

        /// <summary>
        /// locks or unlocks the axis scale
        /// </summary>
        /// <param name="lockScale"> true: locks scale; false: unlocks </param>
        public void LockAxisScale(bool lockScale = true)
            => FrameCommons.LockAxisScale(PlotFrame, lockScale);

        #endregion
        #region ===== title =====

        /// <summary>
        /// sets the title of the frame
        /// </summary>
        /// <param name="content"> title of the frame </param>
        /// <param name="fontBold"> whether to set the title bold </param>
        /// <param name="fontSize"> font size of the title </param>
        /// <param name="fontFamily"> font family of the title </param>
        /// <param name="fontColor"> color of the title </param>
        public void SetTitle(string content,
            bool fontBold = true,
            double fontSize = FrameDefaults.TickSize,
            string fontFamily = FrameDefaults.FontFamily,
            PlotColor fontColor = FrameDefaults.Color)
            => FrameCommons.SetTitle(PlotFrame, content,
                fontBold, fontSize, fontFamily, fontColor);

        #endregion
        #region ===== label =====

        /// <summary>
        /// sets the label of the horizontal axis
        /// </summary>
        /// <param name="content"> label of the horizontal axis </param>
        /// <param name="fontBold"> whether to set the label bold </param>
        /// <param name="fontSize"> font size of the label </param>
        /// <param name="fontFamily"> font family of the label </param>
        /// <param name="fontColor"> color of the label </param>
        public void SetLabelX(string content,
            bool fontBold = false,
            double fontSize = FrameDefaults.AxisLabelSize,
            string fontFamily = FrameDefaults.FontFamily,
            PlotColor fontColor = FrameDefaults.Color)
            => FrameCommons.SetLabelX(PlotFrame, content,
                fontBold, fontSize, fontFamily, fontColor);

        /// <summary>
        /// sets the label of the vertical axis on the left
        /// </summary>
        /// <param name="content"> label of the vertical axis </param>
        /// <param name="fontBold"> whether to set the label bold </param>
        /// <param name="fontSize"> font size of the label </param>
        /// <param name="fontFamily"> font family of the label </param>
        /// <param name="fontColor"> color of the label </param>
        public void SetLabelY(string content,
            bool fontBold = false,
            double fontSize = FrameDefaults.AxisLabelSize,
            string fontFamily = FrameDefaults.FontFamily,
            PlotColor fontColor = FrameDefaults.Color)
            => FrameCommons.SetLabelY(PlotFrame, content,
                fontBold, fontSize, fontFamily, fontColor);

        /// <summary>
        /// sets the label of the vertical axis on the right
        /// </summary>
        /// <param name="content"> label of the vertical axis </param>
        /// <param name="fontBold"> whether to set the label bold </param>
        /// <param name="fontSize"> font size of the label </param>
        /// <param name="fontFamily"> font family of the label </param>
        /// <param name="fontColor"> color of the label </param>
        public void SetLabelYRight(string content,
            bool fontBold = false,
            double fontSize = FrameDefaults.AxisLabelSize,
            string fontFamily = FrameDefaults.FontFamily,
            PlotColor fontColor = FrameDefaults.Color)
            => FrameCommons.SetLabelYRight(PlotFrame, content,
                fontBold, fontSize, fontFamily, fontColor);

        #endregion
        #region ===== legend =====

        /// <summary>
        /// sets the legend of the frame
        /// </summary>
        /// <param name="enable"> whether to enable the legend </param>
        /// <param name="location"> if enabled, set its location </param>
        /// <param name="fontSize"> font size of the legend </param>
        public void SetLegend(bool enable = true,
            LegendLocation location = LegendLocation.LowerRight,
            double fontSize = FrameDefaults.LegendSize)
            => FrameCommons.SetLegend(PlotFrame, enable, location, fontSize);

        #endregion
        #region ===== save image =====

        /// <summary>
        /// saves the content of the frame into 
        /// .JPG(.JPEG)/.PNG/.TIF/.TIFF/.BMP format
        /// </summary>
        /// <param name="fullPath"> target file path (full)  </param>
        /// <param name="width"> resize the plot to this width (pixels) before rendering  </param>
        /// <param name="height"> resize the plot to this height (pixels) before rendering  </param>
        /// <param name="lowQuality"> if true, anti-aliasing will be disabled for this render. Default false </param>
        /// <param name="scale"> scale the size of the output image by this fraction (without resizing the plot) </param>
        public void SaveFig(String fullPath, int? width = null, int? height = null, bool lowQuality = false, double scale = 1.0)
        =>FrameCommons.SaveFig(PlotFrame, fullPath, width, height, lowQuality, scale);
        /// <summary>
        /// Returns the actual width of frame
        /// </summary>
        public int GetActualWidth()
            => FrameCommons.GetActualWidth(PlotFrame);

        /// <summary>
        /// Returns the actual height of frame
        /// </summary>
        public int GetActualHeight()
            => FrameCommons.GetActualHeight(PlotFrame);

        #endregion
        #region ===== copy image to clipboard =====

        /// <summary>
        /// Copy the picture to the clipboard
        /// </summary>
        /// <param name="lowQuality"> if true, anti-aliasing will be disabled for this render. Default false </param>
        public void CopyToClipboard( bool lowQuality = false)
            => FrameCommons.CopyToClipboard(PlotFrame, lowQuality);

        #endregion
        #region ===== span =====

        /// <summary>
        /// modifies the properties of the vertical axial span
        /// </summary>
        /// <param name="start"> start of the span </param>
        /// <param name="end"> end of the span </param>
        /// <param name="fillColor"> fill color of the span </param>
        /// <param name="opacity"> opacity of the fill color, between 0 and 1 </param>
        /// <param name="lineStyle"> style of the line </param>
        /// <param name="numFormat"> number format </param>
        /// <param name="numDigits"> number of digits to display </param>
        /// <param name="isVisible"> whether to show the span or not </param>
        public void SetVerticalSpan(double start, double end,
            PlotColor fillColor = FrameDefaults.VSpanColor,
            double opacity = FrameDefaults.SpanColorOpacity,
            LineStyle lineStyle = FrameDefaults.LineType,
            NumericFormat numFormat = Defaults.NumberFormat,
            int numDigits = Defaults.NumberOfDigits,
            bool isVisible = true)
            => VSpanLines.SetProperties(start, end,
                fillColor, opacity, lineStyle, numFormat, numDigits, isVisible);

        /// <summary>
        /// modifies the properties of the horizontal axial span
        /// </summary>
        /// <param name="start"> start of the span </param>
        /// <param name="end"> end of the span </param>
        /// <param name="color"> fill color of the span </param>
        /// <param name="opacity"> opacity of the fill color, between 0 and 1 </param>
        /// <param name="lineStyle"> style of the line </param>
        /// <param name="numFormat"> number format </param>
        /// <param name="numDigits"> number of digits to display </param>
        /// <param name="isVisible"> whether to show the span or not </param>
        public void SetHorizontalSpan(double start, double end,
            PlotColor color = FrameDefaults.HSpanColor,
            double opacity = FrameDefaults.SpanColorOpacity,
            LineStyle lineStyle = FrameDefaults.LineType,
            NumericFormat numFormat = Defaults.NumberFormat,
            int numDigits = Defaults.NumberOfDigits,
            bool isVisible = true)
            => HSpanLines.SetProperties(start, end,
                color, opacity, lineStyle, numFormat, numDigits, isVisible);

        #endregion
        #region===== annotation =====

        /// <summary>
        /// sets the annotation of the frame
        /// </summary>
        /// <param name="label"> content  of the annotation  </param>
        /// <param name="fontColor"> font color of the annotation </param>
        /// <param name="backgroudColor"> font backgroud of the annotation </param>
        /// <param name="fontSize"> font size of the annotation  </param>
        /// <param name="location"> location of the annotation  </param>
        /// <param name="marginX"> distance (in pixels) from the edge of the plot area to place the annotation</param>
        /// <param name="marginY"> distance (in pixels) from the edge of the plot area to place the annotation </param>
        public void SetAnnotation(string label, 
            PlotColor fontColor, 
            PlotColor backgroudColor, 
            double fontSize, 
            LegendLocation location, 
            double marginX, 
            double marginY )
        {
            Annotation annotation = PlotFrame.Plot.AddAnnotation(label);
            FrameCommons.SetAnnotation(annotation, fontColor, backgroudColor, fontSize, location, marginX, marginY);
        }

        #endregion
        #region===== crosshair =====

        /// <summary>
        /// the Crosshair plot type draws vertical and horizontal lines
        /// </summary>
        /// <param name="x"> x-axis coordinate (physical) </param>
        /// <param name="y"> y-axis coordinate (physical) </param>
        /// <param name="color"> color to draw the lines </param>
        /// <param name="style"> line style </param>
        /// <param name="numFormat"> number format </param>
        /// <param name="numDigits"> number of digits </param>
        /// <param name="isVisible"> whether to show or not </param>
        public void SetCrosshair(double x, double y,
            PlotColor color = FrameDefaults.CrosshairColor,
            LineStyle style = FrameDefaults.CrosshairLineType,
            NumericFormat numFormat = Defaults.NumberFormat,
            int numDigits = Defaults.NumberOfDigits,
            bool isVisible = true)
        {
            FrameCommons.SetCrosshair(MouseTrack, x, y,
                color, style, numFormat, numDigits, isVisible);
            Refresh(); 
        }

        #endregion

        #endregion
        #region ------- plot -------

        #region ===== add real-valued GridPlot =====

        /// <summary>
        /// adds a real-valued grid plot into the frame
        /// </summary>
        /// <param name="values"> data to plot </param>
        /// <param name="grid"> sampling grid information </param>
        /// <param name="lineWidth"> line width </param>
        /// <param name="lineStyle"> line style </param>
        /// <param name="markerSize"> marker size </param>
        /// <param name="markerShape"> marker shape </param>
        /// <param name="plotColor"> plot color </param>
        /// <param name="visualOption"> visual option </param>
        /// <param name="label"> label of the data </param>
        /// <returns> resulting plot </returns>
        public SignalPlot AddGridPlot(VectorD values,
            GridInfo1D? grid = null,
            double lineWidth = FrameDefaults.LineWidth,
            LineStyle lineStyle = FrameDefaults.LineType,
            double markerSize = FrameDefaults.MarkerSize,
            MarkerShape markerShape = FrameDefaults.MarkerType,
            PlotColor? plotColor = null, //FrameCommons.defaultPlotColor,
            VisualOption visualOption = FrameDefaults.ViewOption,
            string? label = null)
        {
            // handling the default label
            label ??= $"# { AllPlots.Count + 1 }: GridData [Real]";

            // generate the plot
            grid ??= new(values.Count);
            SignalPlot sgPlot = FrameCommons.AddGridPlot(PlotFrame, grid, values,
                lineWidth, lineStyle, markerSize, markerShape, plotColor, visualOption, label);

            // add to lists
            AllData.Add(null); // no need to store actual data
            AllPlots.Add(sgPlot);
            AllColorbars.Add(null); // no colorbar here
            AllParts.Add(ComplexPart.RealPart);

            return sgPlot;
        }

        /// <summary>
        /// adds a real-valued grid plot into the frame
        /// </summary>
        /// <param name="gv"> grid vector to plot </param>
        /// <param name="lineWidth"> line width </param>
        /// <param name="lineStyle"> line style </param>
        /// <param name="markerSize"> marker size </param>
        /// <param name="markerShape"> marker shape </param>
        /// <param name="plotColor"> plot color </param>
        /// <param name="visualOption"> visual option </param>
        /// <param name="label"> label of the data </param>
        /// <returns> resulting plot </returns>
        public SignalPlot AddGridPlot(Grid1DRealData gv,
            double lineWidth = FrameDefaults.LineWidth,
            LineStyle lineStyle = FrameDefaults.LineType,
            double markerSize = FrameDefaults.MarkerSize,
            MarkerShape markerShape = FrameDefaults.MarkerType,
            PlotColor plotColor = FrameDefaults.Color,
            VisualOption visualOption = FrameDefaults.ViewOption,
            string? label = null)
            => AddGridPlot(gv.Values, gv.GridInfo,
                    lineWidth, lineStyle,
                    markerSize, markerShape, plotColor,
                    visualOption, label);

        #endregion
        #region ===== add complex-valued GridPlot =====

        /// <summary>
        /// adds a complex-valued grid plot into the frame
        /// </summary>
        /// <param name="values"> data to plot </param>
        /// <param name="grid"> sampling grid information </param>
        /// <param name="plotPart"> part of the complex data to plot </param>
        /// <param name="lineWidth"> line width </param>
        /// <param name="lineStyle"> line style </param>
        /// <param name="markerSize"> marker size </param>
        /// <param name="markerShape"> marker shape </param>
        /// <param name="plotColor"> plot color </param>
        /// <param name="visualOption"> visual option </param>
        /// <param name="label"> label of the data </param>
        /// <returns> resulting plot </returns>
        public SignalPlot AddGridPlot(VectorZ values,
            GridInfo1D? grid = null,
            ComplexPart plotPart = ComplexPart.Magnitude,
            double lineWidth = FrameDefaults.LineWidth,
            LineStyle lineStyle = FrameDefaults.LineType,
            double markerSize = FrameDefaults.MarkerSize,
            MarkerShape markerShape = FrameDefaults.MarkerType,
            PlotColor plotColor = FrameDefaults.Color,
            VisualOption visualOption = FrameDefaults.ViewOption,
            string? label = null)
        {
            // take out the part to plot
            VectorD valuePart = FrameCommons.TakePart(values, plotPart);

            // handling the default lable
            label ??= $"# {AllPlots.Count + 1}: GridData [Complex]";
            
            // generate the plot
            if(grid == null) { grid = new(values.Count); }
            SignalPlot sgPlot = FrameCommons.AddGridPlot(PlotFrame, grid, valuePart,
                lineWidth, lineStyle, markerSize, markerShape, plotColor, visualOption, label);

            // add to list
            AllData.Add(values);
            AllPlots.Add(sgPlot);
            AllColorbars.Add(null); // no colorbar here
            AllParts.Add(plotPart);

            return sgPlot;
        }

        /// <summary>
        /// adds a complex-valued grid plot into the frame
        /// </summary>
        /// <param name="gv"> grid vector to plot </param>
        /// <param name="plotPart"> part of the complex data to plot </param>
        /// <param name="lineWidth"> line width </param>
        /// <param name="lineStyle"> line style </param>
        /// <param name="markerSize"> marker size </param>
        /// <param name="markerShape"> marker shape </param>
        /// <param name="plotColor"> plot color </param>
        /// <param name="visualOption"> visual option </param>
        /// <param name="label"> label of the data </param>
        /// <returns> resulting plot </returns>
        public SignalPlot AddGridPlot(Grid1DCplxData gv, 
            ComplexPart plotPart = ComplexPart.Magnitude,
            double lineWidth = FrameDefaults.LineWidth,
            LineStyle lineStyle = FrameDefaults.LineType,
            double markerSize = FrameDefaults.MarkerSize,
            MarkerShape markerShape = FrameDefaults.MarkerType,
            PlotColor plotColor = FrameDefaults.Color,
            VisualOption visualOption = FrameDefaults.ViewOption,
            string? label = null)
            => AddGridPlot(gv.Values, gv.GridInfo, plotPart,
                lineWidth, lineStyle,
                markerSize, markerShape, plotColor,
                visualOption, label);

        #endregion
        #region ===== add real-valued ScatPlot =====

        /// <summary>
        /// adds a real-valued scatter plot into the frame
        /// </summary>
        /// <param name="locations"> locations of the samples </param>
        /// <param name="values"> values of the samples </param>
        /// <param name="lineWidth"> line width </param>
        /// <param name="lineStyle"> line style </param>
        /// <param name="markerSize"> marker size </param>
        /// <param name="markerShape"> marker shape </param>
        /// <param name="plotColor"> marker color </param>
        /// <param name="visualOption"> visual option </param>
        /// <param name="label"> label of the data </param>
        /// <returns> resulting plot </returns>
        public ScatterPlot AddScatPlot(VectorD locations, VectorD values,
            double lineWidth = FrameDefaults.LineWidth,
            LineStyle lineStyle = FrameDefaults.LineType,
            double markerSize = FrameDefaults.MarkerSize,
            MarkerShape markerShape = FrameDefaults.MarkerType,
            PlotColor plotColor = FrameDefaults.Color,
            VisualOption visualOption = FrameDefaults.ViewOption,
            string? label = null)
        {
            // handling the default label
            label ??= $"# {AllPlots.Count + 1}: ScatData [Real]";

            // generate the plot
            ScatterPlot scPlot = FrameCommons.AddScatPlot(PlotFrame, locations, values,
                lineWidth, lineStyle, markerSize, markerShape, plotColor, visualOption, label);

            // add to list & generate DaraPropExpander
            AllData.Add(null); // no need to store actual data
            AllPlots.Add(scPlot);
            AllColorbars.Add(null); // no colorbar here
            AllParts.Add(ComplexPart.RealPart);

            return scPlot;
        }

        /// <summary>
        /// adds a real-valued scatter plot into the frame
        /// </summary>
        /// <param name="sv"> scatter vector to plot </param>
        /// <param name="lineWidth"> line width </param>
        /// <param name="lineStyle"> line style </param>
        /// <param name="markerSize"> marker size </param>
        /// <param name="markerShape"> marker shape </param>
        /// <param name="plotColor"> plot color </param>
        /// <param name="visualOption"> visual option </param>
        /// <param name="label"> label of the data </param>
        /// <returns> resulting plot </returns>
        public ScatterPlot AddScatPlot(Scat1DRealData sv,
            double lineWidth = FrameDefaults.LineWidth,
            LineStyle lineStyle = FrameDefaults.LineType,
            double markerSize = FrameDefaults.MarkerSize,
            MarkerShape markerShape = FrameDefaults.MarkerType,
            PlotColor plotColor = FrameDefaults.Color,
            VisualOption visualOption = FrameDefaults.ViewOption,
            string? label = null)
            => AddScatPlot(sv.Points, sv.Values,
                lineWidth, lineStyle,
                markerSize, markerShape, plotColor,
                visualOption, label);

        #endregion
        #region ===== add complex-valued ScatPlot =====

        /// <summary>
        /// adds a complex-valued scatter plot into the frame
        /// </summary>
        /// <param name="locations"> locations of the samples </param>
        /// <param name="values"> values of the samples </param>
        /// <param name="plotPart"> part of the complex data to plot </param>
        /// <param name="lineWidth"> line width </param>
        /// <param name="lineStyle"> line style </param>
        /// <param name="markerSize"> marker size </param>
        /// <param name="markerShape"> marker shape </param>
        /// <param name="plotColor"> marker color </param>
        /// <param name="visualOption"> visual option </param>
        /// <param name="label"> label of the data </param>
        /// <returns> resulting plot </returns>
        public ScatterPlot AddScatPlot(VectorD locations, VectorZ values, 
            ComplexPart plotPart = ComplexPart.Magnitude,
            double lineWidth = FrameDefaults.LineWidth,
            LineStyle lineStyle = FrameDefaults.LineType,
            double markerSize = FrameDefaults.MarkerSize,
            MarkerShape markerShape = FrameDefaults.MarkerType,
            PlotColor plotColor = FrameDefaults.Color,
            VisualOption visualOption = FrameDefaults.ViewOption,
            string? label = null)
        {
            // take out the part to plot
            VectorD valuePart = FrameCommons.TakePart(values, plotPart);

            // handling the default label
            label ??= $"# {AllPlots.Count + 1}: ScatData [Complex]";


            // generate the plot
            ScatterPlot scPlot = FrameCommons.AddScatPlot(PlotFrame, locations, valuePart,
                lineWidth, lineStyle, markerSize, markerShape, plotColor, visualOption, label);

            // add to list & generate DaraPropExpander
            AllData.Add(values);
            AllPlots.Add(scPlot);
            AllColorbars.Add(null); // no colorbar here
            AllParts.Add(plotPart);

            return scPlot;
        }

        /// <summary>
        /// adds a complex-valued scatter plot into the frame
        /// </summary>
        /// <param name="sv"> scatter vector to plot </param>
        /// <param name="plotPart"> part of the complex data to plot </param>
        /// <param name="lineWidth"> line width </param>
        /// <param name="lineStyle"> line style </param>
        /// <param name="markerSize"> marker size </param>
        /// <param name="markerShape"> marker shape </param>
        /// <param name="plotColor"> marker color </param>
        /// <param name="visualOption"> visual option </param>
        /// <param name="label"> label of the data </param>
        /// <returns> resulting plot </returns>
        public ScatterPlot AddScatPlot(Scat1DCplxData sv, 
            ComplexPart plotPart = ComplexPart.Magnitude,
            double lineWidth = FrameDefaults.LineWidth,
            LineStyle lineStyle = FrameDefaults.LineType,
            double markerSize = FrameDefaults.MarkerSize, 
            MarkerShape markerShape = FrameDefaults.MarkerType,
            PlotColor plotColor = FrameDefaults.Color,
            VisualOption visualOption = FrameDefaults.ViewOption,
            string? label = null)
            => AddScatPlot(sv.Points, sv.Values, plotPart,
                lineWidth, lineStyle,
                markerSize, markerShape, plotColor,
                visualOption, label);

        #endregion
        #region ===== add real-valued FuncPlot =====

        /// <summary>
        /// adds a real-valued function plot into the frame
        /// </summary>
        /// <param name="f"> function to plot </param>
        /// <param name="lineWidth"> line width </param>
        /// <param name="lineStyle"> line style </param>
        /// <param name="plotColor"> plot color </param>
        /// <param name="visualOption"> visual option </param>
        /// <param name="label"> label of the data </param>
        /// <returns> resulting plot </returns>
        public FunctionPlot AddFuncPlot(Func<double, double?> f,
            double lineWidth = FrameDefaults.LineWidth,
            LineStyle lineStyle = FrameDefaults.LineType,
            PlotColor plotColor = FrameDefaults.Color,
            VisualOption visualOption = FrameDefaults.ViewOption,
            string? label = null)
        {
            // handling the default label
            label ??= $"# {AllPlots.Count + 1}: Function [Real]";

            // generate the plot
            FunctionPlot fcPlot = FrameCommons.AddFuncPlot(PlotFrame, f,
                lineWidth, lineStyle, plotColor, visualOption, label);

            // add to lists
            AllData.Add(null); // no need to store actual data
            AllPlots.Add(fcPlot);
            AllColorbars.Add(null); // no colorbar here
            AllParts.Add(ComplexPart.RealPart);

            return fcPlot;
        }

        #endregion
        #region ===== add complex-valued FuncPlot =====

        /// <summary>
        /// adds a complex-valued function plot into the frame
        /// </summary>
        /// <param name="f"> function to plot </param>
        /// <param name="plotPart"> part of the complex data to plot </param>
        /// <param name="lineWidth"> line width </param>
        /// <param name="lineStyle"> line style </param>
        /// <param name="plotColor"> plot color </param>
        /// <param name="visualOption"> visual option </param>
        /// <param name="label"> label of the data </param>
        /// <returns> resulting plot </returns>
        public FunctionPlot AddFuncPlot(Func<double, Complex?> f, 
            ComplexPart plotPart = ComplexPart.Magnitude,
            double lineWidth = FrameDefaults.LineWidth,
            LineStyle lineStyle = FrameDefaults.LineType,
            PlotColor plotColor = FrameDefaults.Color,
            VisualOption visualOption = FrameDefaults.ViewOption,
            string? label = null)
        {
            // take out the part to plot
            Func<double, double?> valuePart = FrameCommons.TakePart(f, plotPart);

            // handling the default lable
            label ??= $"# {AllPlots.Count + 1}: Function [Complex]";

            // generate the plot
            FunctionPlot fcPlot = FrameCommons.AddFuncPlot(PlotFrame, valuePart,
                lineWidth, lineStyle, plotColor, visualOption, label);

            // add to list
            AllData.Add(f);
            AllPlots.Add(fcPlot);
            AllColorbars.Add(null); // no colorbar here
            AllParts.Add(plotPart);

            return fcPlot;
        }

        #endregion
        #region ===== add real-valued GridGraph =====

        /// <summary>
        /// adds a real-valued GridGraph into the frame
        /// </summary>
        /// <param name="values"> values to plot in a mtrix </param>
        /// <param name="grid"> sampling grid 2D </param>
        /// <param name="colormap"> colormap of the plot </param>
        /// <param name="smoothMode"> graph smooth mode </param>
        /// <param name="visualOption"> visual option </param>
        /// <param name="label"> label of the data </param>
        /// <returns> resulting plot </returns>
        public Heatmap AddGridGraph(MatrixD values,
            GridInfo2D? grid = null,
            PlotColormap colormap = FrameDefaults.Colormap,
            GraphInterpolationMode smoothMode = FrameDefaults.SmoothMode,
            VisualOption visualOption = FrameDefaults.ViewOption,
            string? label = null)
        {
            // handling the default label
            label ??= $"# {AllPlots.Count + 1}: GridGraph [Real]";

            // generate the plot
            if (grid == null) { grid = new(values.Rows, values.Cols); }
            (Heatmap htMap, Colorbar colorbar) = FrameCommons.AddGridGraph(PlotFrame,
                grid, values, colormap, smoothMode, visualOption, label);

            // add to lists
            AllData.Add(values); // need to store the actual data in this case
            AllPlots.Add(htMap);
            AllColorbars.Add(colorbar);
            AllParts.Add(ComplexPart.RealPart);

            return htMap;
        }

        /// <summary>
        /// adds a real-valued GridGraph into the frame
        /// </summary>
        /// <param name="gm"> grid matrix to plot </param>
        /// <param name="colormap"> colormap of the plot </param>
        /// <param name="smoothMode"> graph smooth mode </param>
        /// <param name="visualOption"> visual option </param>
        /// <param name="label"> label of the data </param>
        /// <returns> resulting plot </returns>
        public Heatmap AddGridGraph(Grid2DRealData gm,
            PlotColormap colormap = FrameDefaults.Colormap,
            GraphInterpolationMode smoothMode = FrameDefaults.SmoothMode,
            VisualOption visualOption = FrameDefaults.ViewOption,
            string? label = null)
            => AddGridGraph(gm.Values, gm.GridInfo, 
                colormap, smoothMode, visualOption, label);

        #endregion
        #region ===== add complex-valued GridGraph =====

        /// <summary>
        /// adds a complex-valued grid graph into the frame
        /// </summary>
        /// <param name="values"> data to plot </param>
        /// <param name="grid"> sampling grid information </param>
        /// <param name="plotPart"> part of the complex data to plot </param>
        /// <param name="colormap"> colormap of the plot </param>
        /// <param name="smoothMode"> graph smooth mode </param>
        /// <param name="visualOption"> visual option </param>
        /// <param name="label"> label of the data </param>
        /// <returns> resulting graph </returns>
        public Heatmap AddGridGraph(MatrixZ values,
            GridInfo2D? grid = null, 
            ComplexPart plotPart = ComplexPart.Magnitude,
            PlotColormap colormap = FrameDefaults.Colormap,
            GraphInterpolationMode smoothMode = FrameDefaults.SmoothMode,
            VisualOption visualOption = FrameDefaults.ViewOption,
            string? label = null)
        {
            // take out the part to plot
            MatrixD valuePart = FrameCommons.TakePart(values, plotPart);

            // handling the default lable
            label ??= $"# {AllPlots.Count + 1}: GridGraph [Complex]";

            // generate the plot
            if(grid == null) { grid = new(values.Rows, values.Cols); }
            (Heatmap htMap, Colorbar colorbar) = FrameCommons.AddGridGraph(PlotFrame, grid, valuePart,
                colormap, smoothMode, visualOption, label);

            // add to list
            AllData.Add(values);
            AllPlots.Add(htMap);
            AllColorbars.Add(colorbar);
            AllParts.Add(plotPart);

            return htMap;
        }

        /// <summary>
        /// adds a complex-valued grid graph into the frame
        /// </summary>
        /// <param name="gm"> grid matrix to plot </param>
        /// <param name="plotPart"> part of the complex data to plot </param>
        /// <param name="colormap"> colormap of the plot </param>
        /// <param name="smoothMode"> graph smooth mode </param>
        /// <param name="visualOption"> visual option </param>
        /// <param name="label"> label of the data </param>
        /// <returns> resulting graph </returns>
        public Heatmap AddGridGraph(Grid2DCplxData gm, 
            ComplexPart plotPart = ComplexPart.Magnitude,
            PlotColormap colormap = FrameDefaults.Colormap,
            GraphInterpolationMode smoothMode = FrameDefaults.SmoothMode,
            VisualOption visualOption = FrameDefaults.ViewOption,
            string? label = null)
            => AddGridGraph(gm.Values, gm.GridInfo, plotPart, 
                colormap, smoothMode, visualOption, label);

        #endregion
        #region ===== add real-valued ScatGraph =====

        /// <summary>
        /// adds a real-valued ScatGraph into the frame
        /// </summary>
        /// <param name="x"> x-positions of the dots </param>
        /// <param name="y"> y-positions of the dots </param>
        /// <param name="markerSize"> marker size </param>
        /// <param name="markerShape"> marker shape </param>
        /// <param name="plotColor"> marker color </param>
        /// <param name="visualOption"> visual option </param>
        /// <param name="label"> label of the data </param>
        /// <returns> resulting plot </returns>
        public FrameCommons.DotsMap AddScatGraph(VectorD x, VectorD y,
            double markerSize = FrameDefaults.MarkerSize,
            MarkerShape markerShape = FrameDefaults.MarkerType,
            PlotColor plotColor = FrameDefaults.Color,
            VisualOption visualOption = FrameDefaults.ViewOption,
            string? label = null)
        {
            // handling the default label
            label ??= $"# {AllPlots.Count + 1}: Scat Graph [Real]";

            // generate the plot
            FrameCommons.DotsMap dotsMap = FrameCommons.AddScatGraph(PlotFrame, x,y,
                markerSize, markerShape, plotColor, visualOption, label);

            // add to lists
            AllData.Add(null);
            AllPlots.Add(dotsMap);
            AllParts.Add(ComplexPart.RealPart);

            return dotsMap;
        }

        #endregion
        #region ===== add BMPGraph =====

        /// <summary>
        /// adds a RGB??? BMPGraph into the frame
        /// </summary>
        /// <param name="image"> bitmap image to plot </param>
        /// <param name="x"> anchor position x </param>
        /// <param name="y"> anchor position y </param>
        /// <param name="anchor"> anchor alignment </param>
        /// <param name="visualOption"> visual option </param>
        /// <param name="label"> label of the data </param>
        /// <returns> resulting plot </returns>
        public Image AddBMPGraph(Bitmap image, 
            double x = 0.0,
            double y = 0.0,
            Alignment anchor = Alignment.UpperLeft,
            VisualOption visualOption = FrameDefaults.ViewOption,
            string? label = null)
        {
            // handling the default label
            label ??= $"# {AllPlots.Count + 1}: BMPGraph";

            // generate the plot
            Image img = FrameCommons.AddBMPGraph(PlotFrame,
                image, x, y, anchor, visualOption, label);

            // add to lists
            AllData.Add(null);
            AllPlots.Add(img);
            AllParts.Add(ComplexPart.RealPart);

            return img;
        }

        #endregion
        #region ===== refresh =====

        /// <summary>
        /// expose the refresh method
        /// </summary>
        public void Refresh() => PlotFrame.Refresh();

        #endregion

        // 
        public void DeletePlot(int i)
        {
            int num = AllPlots.Count;
            Printer.Write($"total plot number = {num}");
            Printer.Write($"deleting the {i}-th plot ...");
            FrameCommons.DeletePlot(PlotFrame, i);
            AllPlots.RemoveAt(i);
            Printer.Write($"finished");
        }
        //

        #endregion

        #endregion

    }
}
