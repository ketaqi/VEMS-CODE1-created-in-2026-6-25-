using ScottPlot.Plottable;
using ScottPlot;
using System;
using System.Windows;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Microsoft.Win32;

using VEMS.MathCore;
using VEMS.Plot;
using PlotColor = VEMS.Plot.Options.PlotColor;
using LineStyle = VEMS.Plot.Options.LineStyle;
using MarkerShape = VEMS.Plot.Options.MarkerShape;
using VisualOption = VEMS.Plot.Options.VisualOption;
using PlotColorMap = VEMS.Plot.Options.PlotColormap;
using Image = ScottPlot.Plottable.Image;
using Complex = System.Numerics.Complex;
using System.Drawing;
using static VEMS.Plot.Options;



namespace VEMS.WorkBench
{
    /// <summary>
    /// VFrame.xaml 的交互逻辑
    /// </summary>
    public partial class VFrame : Window, INotifyPropertyChanged
    {
        #region property changed ...
        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string properyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(properyName));
        #endregion

        #region properties

        /// <summary>
        /// link to the list of all the VFrames 
        /// </summary>
        private ObservableCollection<VFrame> appFrames = ((App)Application.Current).Frames;

        /// <summary>
        /// list of all the VFrames
        /// </summary>
        public ObservableCollection<VFrame> AppFrames
        {
            get => appFrames;
            set
            {
                appFrames = value;
                OnPropertyChanged(nameof(AppFrames));
            }
        }
        private SaveFileDialog FileSaver { get; set; }

        private int docIndex { get; set; }
        private string? docName { get; set; }
        private string? usrComment { get; set; }
        
        /// <summary>
        /// VFrame document index
        /// </summary>
        public int DocIndex
        {
            get => docIndex;
            set
            {
                docIndex = value;
                OnPropertyChanged($"{nameof(Index)}");
            }
        }

        /// <summary>
        /// VFrame document display name
        /// </summary>
        public string? DocName
        {
            get => docName;
            set
            {
                docName = value;
                OnPropertyChanged($"{nameof(DocName)}");
                Title = docName;
                OnPropertyChanged($"{nameof(Title)}");
            }
        }

        /// <summary>
        /// user comment on the VFrame document
        /// </summary>
        public string? UserComment
        {
            get => usrComment;
            set
            {
                usrComment = value;
                OnPropertyChanged($"{nameof(UserComment)}");
            }
        }

        #endregion
        #region constructor

        /// <summary>
        /// constructs a VFrame window
        /// </summary>
        public VFrame()
        {
            InitializeComponent();
            InitiateSaveFileDialog();
            // handling window close
            Closed += (o,e) => AppFrames.Remove(this);
        }

        #endregion
        #region methods

        #region ===== Initialze =====
        /// <summary>
        /// initializes the SaveFile dialog
        /// </summary>
        private void InitiateSaveFileDialog() =>
            FileSaver = new SaveFileDialog
            {
                FileName = "Image",
                InitialDirectory = "C:\\Users\\admin\\Desktop",
                Filter = "PNG (*.png)|*.png|JPEG (*.jpg)|*.jpg|TIF (*.tif)|*.tif|TIFF (*.tiff)|*.tiff|BMP (*.bmp)|*.bmp",
                FilterIndex = 1,
                Title = "Save VEMS Image File",
                DefaultExt = "png",
                AddExtension = true
            };
        #endregion

        #region ------- window operations -------

        /// <summary>
        /// minimizes this VFrame window
        /// </summary>
        internal void MinimizeWindow()
            => WindowState= WindowState.Minimized;

        /// <summary>
        /// shows up this VFrame window 
        /// in the front end
        /// </summary>
        internal void ShowUpWindow()
        {
            if(WindowState == WindowState.Minimized)
            {
                WindowState = WindowState.Normal;
            }
            Activate();
        }

        #endregion
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
        private void SetXAxisTicks(bool enableGrid = true,
            bool enableTicks = true,
            double tickDensity = FrameDefaults.TickDensity,
            bool fontBold = false,
            double fontSize = FrameDefaults.TickSize,
            string fontFamily = FrameDefaults.FontFamily,
            PlotColor fontColor = FrameDefaults.Color)
            => FrameExt.SetXAxisTicks(enableGrid, enableTicks, tickDensity,
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
        private void SetYAxisTicks(bool enableGrid = true,
            bool enableTicks = true,
            double tickDensity = FrameDefaults.TickDensity,
            bool fontBold = false,
            double fontSize = FrameDefaults.TickSize,
            string fontFamily = FrameDefaults.FontFamily,
            PlotColor fontColor = FrameDefaults.Color)
            => FrameExt.SetYAxisTicks(enableGrid, enableTicks, tickDensity,
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
        private void SetYRightAxisTicks(bool enableGrid = true,
            bool enableTicks = true,
            double tickDensity = FrameDefaults.TickDensity,
            bool fontBold = false,
            double fontSize = FrameDefaults.TickSize,
            string fontFamily = FrameDefaults.FontFamily,
            PlotColor fontColor = FrameDefaults.Color)
            => FrameExt.SetYRightAxisTicks(enableGrid, enableTicks, tickDensity,
                fontBold, fontSize, fontFamily, fontColor);

        #endregion
        #region ===== range =====

        /// <summary>
        /// sets the horizontal axis limits 
        /// </summary>
        /// <param name="min"> minimum value </param>
        /// <param name="max"> maximum value </param>
        private void SetXAxisLimits(double min, double max)
            => FrameExt.SetXAxisLimits(min, max);

        /// <summary>
        /// sets the vertical axis limits on the left
        /// </summary>
        /// <param name="min"> minimum value </param>
        /// <param name="max"> maximum value </param>
        private void SetYAxisLimits(double min, double max)
            => FrameExt.SetYAxisLimits(min, max);

        /// <summary>
        /// detects the axis limits automatically
        /// </summary>
        private void DetectAxisLimits()
            => FrameExt.DetectAxisLimits();

        /// <summary>
        /// detects the x-axis limits automatically
        /// </summary>
        private void DetectXAxisLimits()
            => FrameExt.DetectXAxisLimits();

        /// <summary>
        /// detects the y-axis limits automatically
        /// </summary>
        private void DetectYAxisLimits()
            => FrameExt.DetectYAxisLimits();

        #endregion
        #region ==== scale ====

        /// <summary>
        /// locks or unlocks the axis scale
        /// </summary>
        /// <param name="lockScale"> true: locks scale; false: unlocks </param>
        private void LockAxisScale(bool lockScale = true)
            => FrameExt.LockAxisScale(lockScale);

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
        private void SetTitle(string content,
            bool fontBold = true,
            double fontSize = FrameDefaults.TitleSize,
            string fontFamily = FrameDefaults.FontFamily,
            PlotColor fontColor = FrameDefaults.Color)
            => FrameExt.SetTitle(content, fontBold, fontSize, fontFamily, fontColor);

        #endregion
        #region ===== labels =====

        /// <summary>
        /// sets the label of the horizontal axis
        /// </summary>
        /// <param name="content"> label of the horizontal axis </param>
        /// <param name="fontBold"> whether to set the label bold </param>
        /// <param name="fontSize"> font size of the label </param>
        /// <param name="fontFamily"> font family of the label </param>
        /// <param name="fontColor"> color of the label </param>
        private void SetLabelX(string content,
            bool fontBold = false,
            double fontSize = FrameDefaults.AxisLabelSize,
            string fontFamily = FrameDefaults.FontFamily,
            PlotColor fontColor = FrameDefaults.Color)
            => FrameExt.SetLabelX(content, fontBold, fontSize, fontFamily, fontColor);

        /// <summary>
        /// sets the label of the vertical axis
        /// </summary>
        /// <param name="content"> label of the vertical axis </param>
        /// <param name="fontBold"> whether to set the label bold </param>
        /// <param name="fontSize"> font size of the label </param>
        /// <param name="fontFamily"> font family of the label </param>
        /// <param name="fontColor"> color of the label </param>
        private void SetLabelY(string content,
            bool fontBold = false,
            double fontSize = FrameDefaults.AxisLabelSize,
            string fontFamily = FrameDefaults.FontFamily,
            PlotColor fontColor = FrameDefaults.Color)
            => FrameExt.SetLabelY(content, fontBold, fontSize, fontFamily, fontColor);

        /// <summary>
        /// sets the label of the vertical axis on the right
        /// </summary>
        /// <param name="content"> label of the vertical axis </param>
        /// <param name="fontBold"> whether to set the label bold </param>
        /// <param name="fontSize"> font size of the label </param>
        /// <param name="fontFamily"> font family of the label </param>
        /// <param name="fontColor"> color of the label </param>
        private void SetLabelYRight(string content,
            bool fontBold = false,
            double fontSize = FrameDefaults.AxisLabelSize,
            string fontFamily = FrameDefaults.FontFamily,
            PlotColor fontColor = FrameDefaults.Color)
            => FrameExt.SetLabelYRight(content, fontBold, fontSize, fontFamily, fontColor);

        #endregion
        #region ===== legend =====

        /// <summary>
        /// sets the legend of the frame
        /// </summary>
        /// <param name="enable"> whether to enable the legend </param>
        /// <param name="location"> if enabled, set its location </param>
        /// <param name="fontSize"> font size of the legend </param>
        private void SetLegend(bool enable = true,
            LegendLocation location = LegendLocation.LowerRight,
            double fontSize = FrameDefaults.LegendSize)
            => FrameExt.SetLegend(enable, location, fontSize);

        #endregion
        #region===== save image =====

        /// <summary>
        /// saves the content of the frame into 
        /// .JPG(.JPEG)/.PNG/.TIF/.TIFF/.BMP format
        /// </summary>
        /// <param name="fullPath"> target file path (full)  </param>
        /// <param name="width"> resize the plot to this width (pixels) before rendering  </param>
        /// <param name="height"> resize the plot to this height (pixels) before rendering  </param>
        /// <param name="lowQuality"> if true, anti-aliasing will be disabled for this render. Default false </param>
        /// <param name="scale"> scale the size of the output image by this fraction (without resizing the plot) </param>
        internal void SaveFrame(String fullPath, int? width = null, int? height = null, bool lowQuality = false, double scale = 1.0)
        =>FrameExt.SaveFig(fullPath, width, height, lowQuality, scale);

        /// <summary>
        /// The Save picture Settings window opens 
        /// </summary>
        public void Open_ImageWindows()
        {
            if (FileSaver.ShowDialog() == true)
            {
                ImageSizeSp imgSp = new(FileSaver.FileName, this);
                imgSp.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                imgSp.ResizeMode = ResizeMode.CanMinimize;
                imgSp.ShowDialog();
            }
        }
        /// <summary>
        /// The menu button is triggered to open the Settings picture window
        /// </summary>
        private void Show_PathDialog(object sender,RoutedEventArgs e)
        {
            Open_ImageWindows();
        }
        /// <summary>
        /// The shortcut key is triggered to open the Settings picture window
        /// </summary>
        private void SaveImage_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            Open_ImageWindows();
        }
        /// <summary>
        /// Returns the actual width of frame
        /// </summary>
        public int GetActualWidth()
        => FrameExt.GetActualWidth();

        /// <summary>
        /// Returns the actual height of frame
        /// </summary>
        public int GetActualHeight()
        => FrameExt.GetActualHeight();

        #endregion
        #region ===== copy image to clipboard =====

        /// <summary>
        /// Copy the picture to the clipboard
        /// </summary>
        /// <param name="lowQuality"> if true, anti-aliasing will be disabled for this render. Default false </param>
        public void CopyToClipboard(bool lowQuality = false)
        => FrameExt.CopyToClipboard(lowQuality);

        /// <summary>
        /// Menu button triggers copy picture to clipboard
        /// </summary>
        private void Copy_ImgeClick(object sender, RoutedEventArgs e)
        {
            CopyToClipboard(false);
        }

        /// <summary>
        /// The shortcut key triggers copying the picture to the clipboard
        /// </summary>
        private void CopyImage_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            CopyToClipboard(false);
        }

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
        /// <param name="numDigits"> number of digits for display </param>
        /// <param name="isVisible"> whether to show the span or not </param>
        internal void SetVerticalSpan(double start, double end,
            PlotColor fillColor = FrameDefaults.VSpanColor,
            double opacity = FrameDefaults.SpanColorOpacity,
            LineStyle lineStyle = FrameDefaults.LineType,
            NumericFormat numFormat = MathCore.Defaults.NumberFormat,
            int numDigits = MathCore.Defaults.NumberOfDigits,
            bool isVisible = true)
            => FrameExt.SetVerticalSpan(start, end, 
                fillColor, opacity, lineStyle, numFormat, numDigits, isVisible);

        /// <summary>
        /// modifies the properties of the horizontal axial span
        /// </summary>
        /// <param name="start"> start of the span </param>
        /// <param name="end"> end of the span </param>
        /// <param name="fillColor"> fill color of the span </param>
        /// <param name="opacity"> opacity of the fill color, between 0 and 1 </param>
        /// <param name="lineStyle"> style of the line </param>
        /// <param name="numFormat"> number format </param>
        /// <param name="numDigits"> number of digits for display </param>
        /// <param name="isVisible"> whether to show the span or not </param>
        internal void SetHorizontalSpan(double start, double end,
            PlotColor fillColor = FrameDefaults.HSpanColor,
            double opacity = FrameDefaults.SpanColorOpacity,
            LineStyle lineStyle = FrameDefaults.LineType,
            NumericFormat numFormat = MathCore.Defaults.NumberFormat,
            int numDigits = MathCore.Defaults.NumberOfDigits,
            bool isVisible = true)
            => FrameExt.SetHorizontalSpan(start, end, 
                fillColor, opacity, lineStyle, numFormat, numDigits, isVisible);

        #endregion
        #region===== annotation =====

        /// <summary>
        /// sets the annotation of the frame
        /// </summary>
        /// <param name="label"> content of the annotation  </param>
        /// <param name="fontColor"> font color of the annotation </param>
        /// <param name="BackgroudColor"> font backgroud of the annotation </param>
        /// <param name="fontSize"> font size of the annotation  </param>
        /// <param name="location"> location of the annotation  </param>
        /// <param name="marginX"> distance (in pixels) from the edge of the plot area to place the annotation</param>
        /// <param name="marginY"> distance (in pixels) from the edge of the plot area to place the annotation </param>
        internal void SetAnnotation(string label, 
            PlotColor fontColor = FrameDefaults.Color, 
            PlotColor BackgroudColor = FrameDefaults.VSpanColor, 
            double fontSize = FrameDefaults.AxisLabelSize, 
            LegendLocation location = LegendLocation.UpperRight, 
            double marginX = 20, 
            double marginY = 20)
            => FrameExt.SetAnnotation(label, fontColor, BackgroudColor, fontSize, location, marginX, marginY);
        
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
        private SignalPlot AddGridPlot(VectorD values,
            GridInfo1D? grid = null,
            double lineWidth = FrameDefaults.LineWidth,
            LineStyle lineStyle = FrameDefaults.LineType,
            double markerSize = FrameDefaults.MarkerSize,
            MarkerShape markerShape = FrameDefaults.MarkerType,
            PlotColor plotColor = FrameDefaults.Color,
            VisualOption visualOption = FrameDefaults.ViewOption,
            string? label = null)
            => FrameExt.AddGridPlot(values, grid, 
                lineWidth, lineStyle, markerSize, markerShape,
                plotColor, visualOption, label);

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
        private SignalPlot AddGridPlot(Grid1DRealData gv,
            double lineWidth = FrameDefaults.LineWidth,
            LineStyle lineStyle = FrameDefaults.LineType,
            double markerSize = FrameDefaults.MarkerSize,
            MarkerShape markerShape = FrameDefaults.MarkerType,
            PlotColor plotColor = FrameDefaults.Color,
            VisualOption visualOption = FrameDefaults.ViewOption,
            string? label = null) 
            => FrameExt.AddGridPlot(gv, 
                lineWidth, lineStyle, markerSize, markerShape,
                plotColor, visualOption, label);

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
        private SignalPlot AddGridPlot(VectorZ values,
            GridInfo1D? grid = null,
            ComplexPart plotPart = ComplexPart.Magnitude,
            double lineWidth = FrameDefaults.LineWidth,
            LineStyle lineStyle = FrameDefaults.LineType,
            double markerSize = FrameDefaults.MarkerSize,
            MarkerShape markerShape = FrameDefaults.MarkerType,
            PlotColor plotColor = FrameDefaults.Color,
            VisualOption visualOption = FrameDefaults.ViewOption,
            string? label = null) 
            => FrameExt.AddGridPlot(values, grid, 
                plotPart, lineWidth, lineStyle, markerSize, markerShape, 
                plotColor, visualOption, label);

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
        private SignalPlot AddGridPlot(Grid1DCplxData gv, 
            ComplexPart plotPart = ComplexPart.Magnitude,
            double lineWidth = FrameDefaults.LineWidth,
            LineStyle lineStyle = FrameDefaults.LineType,
            double markerSize = FrameDefaults.MarkerSize,
            MarkerShape markerShape = FrameDefaults.MarkerType,
            PlotColor plotColor = FrameDefaults.Color,
            VisualOption visualOption = FrameDefaults.ViewOption,
            string? label = null) 
            => FrameExt.AddGridPlot(gv, 
                plotPart, lineWidth, lineStyle, markerSize, markerShape, 
                plotColor, visualOption, label);

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
            => FrameExt.AddScatPlot(locations, values, lineWidth, lineStyle,
                markerSize, markerShape, plotColor, visualOption, label);

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
            => FrameExt.AddScatPlot(sv, lineWidth, lineStyle,
                markerSize, markerShape, plotColor, visualOption, label);

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
        private ScatterPlot AddScatPlot(VectorD locations, VectorZ values, 
            ComplexPart plotPart = ComplexPart.Magnitude,
            double lineWidth = FrameDefaults.LineWidth,
            LineStyle lineStyle = FrameDefaults.LineType,
            double markerSize = FrameDefaults.MarkerSize,
            MarkerShape markerShape = FrameDefaults.MarkerType,
            PlotColor plotColor = FrameDefaults.Color,
            VisualOption visualOption = FrameDefaults.ViewOption,
            string? label = null) 
            => FrameExt.AddScatPlot(locations, values, plotPart, 
                lineWidth, lineStyle, markerSize, markerShape, 
                plotColor, visualOption, label);

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
        private ScatterPlot AddScatPlot(Scat1DCplxData sv, 
            ComplexPart plotPart = ComplexPart.Magnitude,
            double lineWidth = FrameDefaults.LineWidth,
            LineStyle lineStyle = FrameDefaults.LineType,
            double markerSize = FrameDefaults.MarkerSize,
            MarkerShape markerShape = FrameDefaults.MarkerType,
            PlotColor plotColor = FrameDefaults.Color,
            VisualOption visualOption = FrameDefaults.ViewOption,
            string? label = null) 
            => FrameExt.AddScatPlot(sv, plotPart, 
                lineWidth, lineStyle, markerSize, markerShape, 
                plotColor, visualOption, label);

        #endregion
        #region ===== add real-valued FuncPlot =====

        /// <summary>
        /// adds a real-valued function plot into the frame
        /// </summary>
        /// <param name="f"> function to plot </param>
        /// <param name="lineWidth"> line width </param>
        /// <param name="lineStyle"> line style </param>
        /// <param name="plotColor"> marker color </param>
        /// <param name="visualOption"> visual option </param>
        /// <param name="label"> label of the data </param>
        /// <returns> resulting plot </returns>
        private FunctionPlot AddFuncPlot(Func<double, double?> f,
            double lineWidth = FrameDefaults.LineWidth,
            LineStyle lineStyle = FrameDefaults.LineType,
            PlotColor plotColor = FrameDefaults.Color,
            VisualOption visualOption = FrameDefaults.ViewOption,
            string? label = null) 
            => FrameExt.AddFuncPlot(f, lineWidth, lineStyle,
                plotColor, visualOption, label);

        #endregion
        #region ===== add complex-valued FuncPlot =====

        /// <summary>
        /// adds a complex-valued function plot into the frame
        /// </summary>
        /// <param name="f"> function to plot </param>
        /// <param name="plotPart"> part of the complex data to plot </param>
        /// <param name="lineWidth"> line width </param>
        /// <param name="lineStyle"> line style </param>
        /// <param name="plotColor"> marker color </param>
        /// <param name="visualOption"> visual option </param>
        /// <param name="label"> label of the data </param>
        /// <returns> resulting plot </returns>
        private FunctionPlot AddFuncPlot(Func<double, Complex?> f, 
            ComplexPart plotPart = ComplexPart.Magnitude,
            double lineWidth = FrameDefaults.LineWidth,
            LineStyle lineStyle = FrameDefaults.LineType,
            PlotColor plotColor = FrameDefaults.Color,
            VisualOption visualOption = FrameDefaults.ViewOption,
            string? label = null)
            => FrameExt.AddFuncPlot(f, plotPart, lineWidth, lineStyle,
                plotColor, visualOption, label);

        #endregion
        #region ===== add real-valued GridGraph =====

        /// <summary>
        /// adds a real-valued GridGraph into the frame
        /// </summary>
        /// <param name="values"> values to plot in a mtrix </param>
        /// <param name="grid"> sampling grid information </param>
        /// <param name="colormap"> colormap of the plot </param>
        /// <param name="smoothMode"> graph smooth mode </param>
        /// <param name="visualOption"> visual option </param>
        /// <param name="label"> label of the data </param>
        /// <returns> resulting plot </returns>
        private Heatmap AddGridGraph(MatrixD values,
            GridInfo2D? grid = null,
            PlotColorMap colormap = FrameDefaults.Colormap,
            GraphInterpolationMode smoothMode = FrameDefaults.SmoothMode,
            VisualOption visualOption = FrameDefaults.ViewOption,
            string? label = null) 
            => FrameExt.AddGridGraph(values, grid,
                colormap, smoothMode, visualOption, label);

        /// <summary>
        /// adds a real-valued GridGraph into the frame
        /// </summary>
        /// <param name="gm"> grid matrix to plot </param>
        /// <param name="colormap"> colormap of the plot </param>
        /// <param name="smoothMode"> graph smooth mode </param>
        /// <param name="visualOption"> visual option </param>
        /// <param name="label"> label of the data </param>
        /// <returns> resulting plot </returns>
        private Heatmap AddGridGraph(Grid2DRealData gm,
            PlotColorMap colormap = FrameDefaults.Colormap,
            GraphInterpolationMode smoothMode = FrameDefaults.SmoothMode,
            VisualOption visualOption = FrameDefaults.ViewOption,
            string? label = null)
            => FrameExt.AddGridGraph(gm, colormap, smoothMode, visualOption, label);

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
        private Heatmap AddGridGraph(MatrixZ values,
            GridInfo2D? grid = null, 
            ComplexPart plotPart = ComplexPart.Magnitude,
            PlotColorMap colormap = FrameDefaults.Colormap,
            GraphInterpolationMode smoothMode = FrameDefaults.SmoothMode,
            VisualOption visualOption = FrameDefaults.ViewOption,
            string? label = null)
            => FrameExt.AddGridGraph(values, grid, plotPart, 
                colormap, smoothMode, visualOption, label);

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
        private Heatmap AddGridGraph(Grid2DCplxData gm, ComplexPart plotPart,
            PlotColorMap colormap = FrameDefaults.Colormap,
            GraphInterpolationMode smoothMode = FrameDefaults.SmoothMode,
            VisualOption visualOption = FrameDefaults.ViewOption,
            string? label = null)
            => FrameExt.AddGridGraph(gm, plotPart, 
                colormap, smoothMode, visualOption, label);

        #endregion
        #region ===== add real-valued ScatGraph =====

        /// <summary>
        /// adds a ScatGraph into the frame
        /// </summary>
        /// <param name="x"> x-positions of the dots </param>
        /// <param name="y"> y-positions of the dots </param>
        /// <param name="markerSize"> marker size </param>
        /// <param name="markerShape"> marker shape </param>
        /// <param name="plotColor"> plot color </param>
        /// <param name="visualOption"> visual option </param>
        /// <param name="label"> label of the data </param>
        /// <returns> resulting graph </returns>
        private FrameCommons.DotsMap AddScatGraph(VectorD x, VectorD y,
            double markerSize = FrameDefaults.MarkerSize,
            MarkerShape markerShape = FrameDefaults.MarkerType,
            PlotColor plotColor = FrameDefaults.Color,
            VisualOption visualOption = FrameDefaults.ViewOption,
            string? label = null)
            => FrameExt.AddScatGraph(x,y,
                markerSize, markerShape, plotColor, visualOption, label);

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
        private Image AddBMPGraph(Bitmap image,
            double x = 0.0,
            double y = 0.0,
            Alignment anchor = Alignment.UpperLeft,
            VisualOption visualOption = FrameDefaults.ViewOption,
            string? label = null)
            => FrameExt.AddBMPGraph(image, x, y, anchor, visualOption, label);

        #endregion

        //
        private void DeletePlot(int i) => FrameExt.DeletePlot(i);
        //

        #endregion
        #region ------- refresh & show -------

        /// <summary>
        /// refresh and show the frame
        /// </summary>
        private void RefreshShow()
        {
            // call refresh method
            FrameExt.Refresh();
            // only set document info before showing it
            DocIndex = AppFrames.Count == 0 ? 1 : AppFrames[^1].DocIndex + 1; ; 
            DocName = $"VEMS | Frame Document # {DocIndex}";
            UserComment = $"{DocName} ...";
            AppFrames.Add(this);
            // show window
            Show();
        }

        #endregion

        private void WidthInfo_Click(object sender, RoutedEventArgs e)
        {
            Printer.Write($"Window Actual Width = {this.ActualWidth}");
        }

        private void DeleteLast_Click(object sender, RoutedEventArgs e)
        {
            int num = FrameExt.AllPlots.Count;
            DeletePlot(num-1);
        }

        #endregion
        #region static methods

        #region ------- create -------

        /// <summary>
        /// creates a frame
        /// </summary>
        /// <returns> frame </returns>
        public static VFrame CreateFrame() 
        {
            VFrame? f = null;
            Application.Current.Dispatcher.Invoke(() =>
            {
                f = new VFrame();
            });
            return f ?? new VFrame();
        }

        #endregion
        #region ------- frame -------

        #region ===== ticks =====

        /// <summary>
        /// sets the horizontal ticks 
        /// </summary>
        /// <param name="f"> the frame </param>
        /// <param name="enableGrid"> enable grid </param>
        /// <param name="enableTicks"> enable ticks </param>
        /// <param name="tickDensity"> density of the ticks </param>
        /// <param name="fontSize"> font size of the ticks </param>
        /// <param name="fontBold"> whether to set the ticks bold </param>
        /// <param name="fontFamily"> font family of the ticks </param>
        /// <param name="fontColor"> color of the ticks </param>
        public static void SetXAxisTicks(VFrame f,
            bool enableGrid = true,
            bool enableTicks = true,
            double tickDensity = FrameDefaults.TickDensity,
            double fontSize = FrameDefaults.TickSize,
            bool fontBold = false,
            string fontFamily = FrameDefaults.FontFamily,
            PlotColor fontColor = FrameDefaults.Color)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                f.SetXAxisTicks(enableGrid: enableGrid, enableTicks: enableTicks,
                    tickDensity, fontBold, fontSize, fontFamily, fontColor);
            });
        }

        /// <summary>
        /// sets the vertical ticks on the left
        /// </summary>
        /// <param name="f"> the frame </param>
        /// <param name="enableGrid"> enable grid </param>
        /// <param name="enableTicks"> enable ticks </param>
        /// <param name="tickDensity"> density of the ticks </param>
        /// <param name="fontSize"> font size of the ticks </param>
        /// <param name="fontBold"> whether to set the ticks bold </param>
        /// <param name="fontFamily"> font family of the ticks </param>
        /// <param name="fontColor"> color of the ticks </param>
        public static void SetYAxisTicks(VFrame f,
            bool enableGrid = true,
            bool enableTicks = true,
            double tickDensity = FrameDefaults.TickDensity,
            double fontSize = FrameDefaults.TickSize,
            bool fontBold = false,
            string fontFamily = FrameDefaults.FontFamily,
            PlotColor fontColor = FrameDefaults.Color)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                f.SetYAxisTicks(enableGrid: enableGrid, enableTicks: enableTicks,
                    tickDensity, fontBold, fontSize, fontFamily, fontColor);
            });
        }

        /// <summary>
        /// sets the vertical ticks on the right
        /// </summary>
        /// <param name="f"> the frame </param>
        /// <param name="enableGrid"> enable grid </param>
        /// <param name="enableTicks"> enable ticks </param>
        /// <param name="tickDensity"> density of the ticks </param>
        /// <param name="fontSize"> font size of the ticks </param>
        /// <param name="fontBold"> whether to set the ticks bold </param>
        /// <param name="fontFamily"> font family of the ticks </param>
        /// <param name="fontColor"> color of the ticks </param>
        public static void SetYRightTicks(VFrame f,
            bool enableGrid = true,
            bool enableTicks = true,
            double tickDensity = FrameDefaults.TickDensity,
            double fontSize = FrameDefaults.TickSize,
            bool fontBold = false,
            string fontFamily = FrameDefaults.FontFamily,
            PlotColor fontColor = FrameDefaults.Color)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                f.SetYRightAxisTicks(enableGrid: enableGrid, enableTicks: enableTicks,
                    tickDensity, fontBold, fontSize, fontFamily, fontColor);
            });
        }

        #endregion
        #region ===== range =====

        /// <summary>
        /// sets the horizontal axis limits 
        /// </summary>
        /// <param name="f"> the frame </param>
        /// <param name="min"> minimum value </param>
        /// <param name="max"> maximum value </param>
        public static void SetXAxisLimits(VFrame f, double min, double max)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                f.SetXAxisLimits(min, max);
            });
        }

        /// <summary>
        /// sets the vertical axis limits on the left
        /// </summary>
        /// <param name="f"> the frame </param>
        /// <param name="min"> minimum value </param>
        /// <param name="max"> maximum value </param>
        public static void SetYAxisLimits(VFrame f, double min, double max)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                f.SetYAxisLimits(min, max);
            });
        }

        /// <summary>
        /// detects the axis limits automatically
        /// </summary>
        /// <param name="f"> the frame </param>
        public static void DetectAxisLimits(VFrame f)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                f.DetectAxisLimits();
            });
        }

        /// <summary>
        /// detects the x-axis limits automatically
        /// </summary>
        /// <param name="f"> the frame </param>
        public static void DetectXAxisLimits(VFrame f)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                f.DetectXAxisLimits();
            });
        }

        /// <summary>
        /// detects the y-axis limits automatically
        /// </summary>
        /// <param name="f"> the frame </param>
        public static void DetectYAxisLimits(VFrame f)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                f.DetectYAxisLimits();
            });
        }

        #endregion
        #region ===== scale =====

        /// <summary>
        /// locks or unlocks the axis scale
        /// </summary>
        /// <param name="f"> reference frame </param>
        /// <param name="lockScale"> true:lock scale; false: unlock </param>
        public static void LockAxieScale(VFrame f, bool lockScale = true)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                f.LockAxisScale(lockScale); 
            });

        }

        #endregion
        #region ===== title =====

        /// <summary>
        /// sets the title of the frame
        /// </summary>
        /// <param name="f"> the frame </param>
        /// <param name="content"> title of the frame </param>
        /// <param name="fontBold"> whether to set the title bold </param>
        /// <param name="fontSize"> font size of the title </param>
        /// <param name="fontFamily"> font family of the title </param>
        /// <param name="fontColor"> color of the title </param>
        public static void SetTitle(VFrame f, string content,
            bool fontBold = true,
            double fontSize = FrameDefaults.TitleSize,
            string fontFamily = FrameDefaults.FontFamily,
            PlotColor fontColor = FrameDefaults.Color)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                f.SetTitle(content, fontBold, fontSize, fontFamily, fontColor);
            });
        }

        #endregion
        #region ===== labels =====

        /// <summary>
        /// sets the label of the horizontal axis
        /// </summary>
        /// <param name="f"> the frame </param>
        /// <param name="content"> label of the horizontal axis </param>
        /// <param name="fontBold"> whether to set the label bold </param>
        /// <param name="fontSize"> font size of the label </param>
        /// <param name="fontFamily"> font family of the label </param>
        /// <param name="fontColor"> color of the label </param>
        public static void SetLabelX(VFrame f, string content,
            bool fontBold = false,
            double fontSize = FrameDefaults.AxisLabelSize,
            string fontFamily = FrameDefaults.FontFamily,
            PlotColor fontColor = FrameDefaults.Color)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                f.SetLabelX(content, fontBold, fontSize, fontFamily, fontColor);
            });
        }

        /// <summary>
        /// sets the label of the vertical axis
        /// </summary>
        /// <param name="f"> the frame </param>
        /// <param name="content"> label of the vertical axis </param>
        /// <param name="fontBold"> whether to set the label bold </param>
        /// <param name="fontSize"> font size of the label </param>
        /// <param name="fontFamily"> font family of the label </param>
        /// <param name="fontColor"> color of the label </param>
        public static void SetLabelY(VFrame f, string content,
            bool fontBold = false,
            double fontSize = FrameDefaults.AxisLabelSize,
            string fontFamily = FrameDefaults.FontFamily,
            PlotColor fontColor = FrameDefaults.Color)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                f.SetLabelY(content, fontBold, fontSize, fontFamily, fontColor);
            });
        }

        /// <summary>
        /// sets the label of the vertical axis on the right
        /// </summary>
        /// <param name="f"> the frame </param>
        /// <param name="content"> label of the vertical axis </param>
        /// <param name="fontBold"> whether to set the label bold </param>
        /// <param name="fontSize"> font size of the label </param>
        /// <param name="fontFamily"> font family of the label </param>
        /// <param name="fontColor"> color of the label </param>
        public static void SetLabelYRight(VFrame f, string content,
            bool fontBold = false,
            double fontSize = FrameDefaults.AxisLabelSize,
            string fontFamily = FrameDefaults.FontFamily,
            PlotColor fontColor = FrameDefaults.Color)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                f.SetLabelYRight(content, fontBold, fontSize, fontFamily, fontColor);
            });
        }

        #endregion
        #region ===== legend =====

        /// <summary>
        /// sets the legend of the frame
        /// </summary>
        /// <param name="f"> the frame </param>
        /// <param name="location"> location of the legend </param>
        /// <param name="fontSize"> font size of the legend </param>
        public static void SetLegend(VFrame f,
            LegendLocation location = LegendLocation.LowerRight,
            double fontSize = FrameDefaults.LegendSize)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                f.SetLegend(true, location, fontSize);
            });
        }

        #endregion
        #region ===== saveFig=====

        /// <summary>
        /// saves the content of the frame into 
        /// .JPG(.JPEG)/.PNG/.TIF/.TIFF/.BMP format
        /// </summary>
        /// <param name="f"> the frame </param>
        /// <param name="fullPath"> target file path (full)  </param>
        /// <param name="width"> resize the plot to this width (pixels) before rendering  </param>
        /// <param name="height"> resize the plot to this height (pixels) before rendering  </param>
        /// <param name="lowQuality"> if true, anti-aliasing will be disabled for this render. Default false </param>
        /// <param name="scale"> scale the size of the output image by this fraction (without resizing the plot) </param>
        public static void SaveFig(VFrame f, string fullPath, int? width = null, int? height = null, bool lowQuality = false, double scale = 1.0)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                f.SaveFrame(fullPath, width, height, lowQuality, scale);
            });
        }

        #endregion
        #region ===== multi-parameters =====

        /// <summary>
        /// sets multiple frame parameters
        /// </summary>
        /// <param name="f"> the frame </param>
        /// <param name="xAxisTickDensity"> density of the ticks along the horizontal direction </param>
        /// <param name="xAxisTickSize"> font size of the horizontal ticks </param>
        /// <param name="yAxisTickDensity"> density of the ticks along the vertical direction </param>
        /// <param name="yAxisTickSize"> font size of the vertical ticks </param>
        /// <param name="xMin"> minimum value of the horizontal axis </param>
        /// <param name="xMax"> maximum value of the horizontal axis </param>
        /// <param name="yMin"> minimum value of the vertical axis </param>
        /// <param name="yMax"> maximum value of the vertical axis </param>
        /// <param name="title"> title of the frame </param>
        /// <param name="titleSize"> font size of the title </param>
        /// <param name="xLabel"> label of the horizontal axis </param>
        /// <param name="xLabelSize"> font size of the label X </param>
        /// <param name="yLabel"> label of the vertical axis </param>
        /// <param name="yLabelSize"> font size of the label Y </param>
        public static void SetFrameParameters(VFrame f,
            double xAxisTickDensity = FrameDefaults.TickDensity,
            double xAxisTickSize = FrameDefaults.TickSize,
            double yAxisTickDensity = FrameDefaults.TickDensity,
            double yAxisTickSize = FrameDefaults.TickSize,
            double xMin = FrameDefaults.XMin,
            double xMax = FrameDefaults.XMax,
            double yMin = FrameDefaults.YMin,
            double yMax = FrameDefaults.YMax,
            string title = FrameDefaults.Title,
            double titleSize = FrameDefaults.TitleSize,
            string xLabel = FrameDefaults.LabelX,
            double xLabelSize = FrameDefaults.AxisLabelSize,
            string yLabel = FrameDefaults.LabelY,
            double yLabelSize = FrameDefaults.AxisLabelSize)
        {
            if(xAxisTickDensity != FrameDefaults.TickDensity
                || xAxisTickSize != FrameDefaults.TickSize)
                SetXAxisTicks(f, tickDensity: xAxisTickDensity, fontSize: xAxisTickSize);
            if(yAxisTickDensity != FrameDefaults.TickDensity
                || yAxisTickSize != FrameDefaults.TickSize)
                SetYAxisTicks(f, tickDensity: yAxisTickDensity, fontSize: yAxisTickSize);
            if(xMin != FrameDefaults.XMin || xMax != FrameDefaults.XMax)
                SetXAxisLimits(f, min: xMin, max: xMax);
            if(yMin != FrameDefaults.YMin || yMax != FrameDefaults.YMax)
                SetYAxisLimits(f, min: yMin, max: yMax);
            if(title != FrameDefaults.Title
                || titleSize != FrameDefaults.TitleSize)
                SetTitle(f, content: title, fontSize: titleSize);
            if(xLabel != FrameDefaults.LabelX
                || xLabelSize != FrameDefaults.AxisLabelSize)
                SetLabelX(f, content: xLabel, fontSize: xLabelSize);
            if(yLabel != FrameDefaults.LabelY
                || yLabelSize != FrameDefaults.AxisLabelSize)
                SetLabelY(f, content: yLabel, fontSize: yLabelSize);
        }

        #endregion
        #region ===== span =====

        /// <summary>
        /// modifies the properties of the vertical axial span
        /// </summary>
        /// <param name="f"> the frame </param>
        /// <param name="start"> start of the span </param>
        /// <param name="end"> end of the span </param>
        /// <param name="fillColor"> fill color of the span </param>
        /// <param name="opacity"> opacity of the fill color, between 0 and 1 </param>
        /// <param name="lineStyle"> style of the line </param>
        /// <param name="numFormat"> number format </param>
        /// <param name="numDigits"> number of digits for display </param>
        /// <param name="isVisible"> whether to show the span or not </param>
        public static void SetVerticalSpan(VFrame f, double start, double end,
            PlotColor fillColor = FrameDefaults.VSpanColor,
            double opacity = FrameDefaults.SpanColorOpacity,
            LineStyle lineStyle = FrameDefaults.LineType,
            NumericFormat numFormat = MathCore.Defaults.NumberFormat,
            int numDigits = MathCore.Defaults.NumberOfDigits,
            bool isVisible = true)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                f.SetVerticalSpan(start, end, 
                    fillColor, opacity, lineStyle, numFormat, numDigits, isVisible); 
            });
        }

        /// <summary>
        /// modifies the properties of the horizontal axial span
        /// </summary>
        /// <param name="f"> the frame </param>
        /// <param name="start"> start of the span </param>
        /// <param name="end"> end of the span </param>
        /// <param name="fillColor"> fill color of the span </param>
        /// <param name="opacity"> opacity of the fill color, between 0 and 1 </param>
        /// <param name="lineStyle"> style of the line </param>
        /// <param name="numFormat"> number format </param>
        /// <param name="numDigits"> number of digits for display </param>
        /// <param name="isVisible"> whether to show the span or not </param>
        public static void SetHorizontalSpan(VFrame f, double start, double end,
            PlotColor fillColor = FrameDefaults.HSpanColor,
            double opacity = FrameDefaults.SpanColorOpacity,
            LineStyle lineStyle = FrameDefaults.LineType,
            NumericFormat numFormat = MathCore.Defaults.NumberFormat,
            int numDigits = MathCore.Defaults.NumberOfDigits,
            bool isVisible = true)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                f.SetHorizontalSpan(start, end, 
                    fillColor, opacity, lineStyle, numFormat, numDigits, isVisible);
            });
        }

        #endregion
        #region===== annotation =====

        /// <summary>
        /// sets the annotation of the frame
        /// </summary>
        /// <param name="f"> the frame </param>
        /// <param name="label"> content  of the annotation  </param>
        /// <param name="fontColor"> font color of the annotation </param>
        /// <param name="BackgroudColor"> font backgroud of the annotation </param>
        /// <param name="fontSize"> font size of the annotation  </param>
        /// <param name="location"> location of the annotation  </param>
        /// <param name="marginX"> distance (in pixels) from the edge of the plot area to place the annotation</param>
        /// <param name="marginY"> distance (in pixels) from the edge of the plot area to place the annotation </param>
        public static void SetAnnotation(VFrame f, 
            string label, 
            PlotColor fontColor = FrameDefaults.Color, 
            PlotColor BackgroudColor = FrameDefaults.VSpanColor, 
            double fontSize = FrameDefaults.AxisLabelSize, 
            LegendLocation location = LegendLocation.UpperRight, 
            double marginX = 20, 
            double marginY = 20)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                f.SetAnnotation(label, fontColor, BackgroudColor, fontSize, location, marginX, marginY);
            });
        } 

        #endregion

        #endregion
        #region ------- plot -------

        #region ===== add real-valued GridPlot =====

        /// <summary>
        /// adds a real-valued grid plot into the frame
        /// </summary>
        /// <param name="f"> the frame </param>
        /// <param name="values"> data to plot </param>
        /// <param name="grid"> sampling grid information </param>
        /// <param name="lineWidth"> line width </param>
        /// <param name="lineStyle"> line style </param>
        /// <param name="markerSize"> marker size </param>
        /// <param name="markerShape"> marker shape </param>
        /// <param name="plotColor"> plot color </param>
        /// <param name="visualOption"> visual option </param>
        /// <param name="label"> label of the data </param>
        public static void AddToFrame(VFrame f, VectorD values,
            GridInfo1D? grid = null,
            double lineWidth = FrameDefaults.LineWidth,
            LineStyle lineStyle = FrameDefaults.LineType,
            double markerSize = FrameDefaults.MarkerSize,
            MarkerShape markerShape = FrameDefaults.MarkerType,
            PlotColor plotColor = FrameDefaults.Color,
            VisualOption visualOption = FrameDefaults.ViewOption,
            string? label = null)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if(grid == null) { grid = new(values.Count); }
                f.AddGridPlot(values, grid, lineWidth, lineStyle,
                    markerSize, markerShape, plotColor, 
                    visualOption, label);
            });
        }

        /// <summary>
        /// adds a real-valued grid plot into the frame
        /// </summary>
        /// <param name="f"> the frame </param>
        /// <param name="gv"> grid vector to plot </param>
        /// <param name="lineWidth"> line width </param>
        /// <param name="lineStyle"> line style </param>
        /// <param name="markerSize"> marker size </param>
        /// <param name="markerShape"> marker shape </param>
        /// <param name="plotColor"> plot color </param>
        /// <param name="visualOption"> visual option </param>
        /// <param name="label"> label of the data </param>
        public static void AddToFrame(VFrame f, Grid1DRealData gv,
            double lineWidth = FrameDefaults.LineWidth,
            LineStyle lineStyle = FrameDefaults.LineType,
            double markerSize = FrameDefaults.MarkerSize,
            MarkerShape markerShape = FrameDefaults.MarkerType,
            PlotColor plotColor = FrameDefaults.Color,
            VisualOption visualOption = FrameDefaults.ViewOption,
            string? label = null)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                f.AddGridPlot(gv, lineWidth, lineStyle,
                    markerSize, markerShape, plotColor,
                    visualOption, label);
            });
        }

        #endregion
        #region ===== add complex-valued GridPlot =====

        /// <summary>
        /// adds a complex-valued grid plot into the frame
        /// </summary>
        /// <param name="f"> the frame </param>
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
        public static void AddToFrame(VFrame f, VectorZ values,
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
            Application.Current.Dispatcher.Invoke(() =>
            {
                if(grid == null) { grid = new(values.Count); }
                f.AddGridPlot(values, grid, plotPart, lineWidth, lineStyle,
                    markerSize, markerShape, plotColor,
                    visualOption, label);
            });
        }

        /// <summary>
        /// adds a complex-valued grid plot into the frame
        /// </summary>
        /// <param name="f"> the frame </param>
        /// <param name="gv"> grid vector to plot </param>
        /// <param name="plotPart"> part of the complex data to plot </param>
        /// <param name="lineWidth"> line width </param>
        /// <param name="lineStyle"> line style </param>
        /// <param name="markerSize"> marker size </param>
        /// <param name="markerShape"> marker shape </param>
        /// <param name="plotColor"> plot color </param>
        /// <param name="visualOption"> visual option </param>
        /// <param name="label"> label of the data </param>
        public static void AddToFrame(VFrame f, Grid1DCplxData gv, 
            ComplexPart plotPart = ComplexPart.Magnitude,
            double lineWidth = FrameDefaults.LineWidth,
            LineStyle lineStyle = FrameDefaults.LineType,
            double markerSize = FrameDefaults.MarkerSize,
            MarkerShape markerShape = FrameDefaults.MarkerType,
            PlotColor plotColor = FrameDefaults.Color,
            VisualOption visualOption = FrameDefaults.ViewOption,
            string? label = null)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                f.AddGridPlot(gv, plotPart, lineWidth, lineStyle,
                    markerSize, markerShape, plotColor,
                    visualOption, label);
            });
        }

        #endregion
        #region ===== add real-valued ScatPlot =====

        /// <summary>
        /// adds a real-valued scatter plot into the frame
        /// </summary>
        /// <param name="f"> the frame </param>
        /// <param name="locations"> locations of the samples </param>
        /// <param name="values"> values of the samples </param>
        /// <param name="lineWidth"> line width </param>
        /// <param name="lineStyle"> line style </param>
        /// <param name="markerSize"> marker size </param>
        /// <param name="markerShape"> marker shape </param>
        /// <param name="plotColor"> marker color </param>
        /// <param name="visualOption"> visual option </param>
        /// <param name="label"> label of the data </param>
        public static void AddToFrame(VFrame f, VectorD locations, VectorD values,
            double lineWidth = FrameDefaults.LineWidth,
            LineStyle lineStyle = FrameDefaults.LineType,
            double markerSize = FrameDefaults.MarkerSize,
            MarkerShape markerShape = FrameDefaults.MarkerType,
            PlotColor plotColor = FrameDefaults.Color,
            VisualOption visualOption = FrameDefaults.ViewOption,
            string? label = null)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                f.AddScatPlot(locations, values, lineWidth, lineStyle,
                    markerSize, markerShape, plotColor,
                    visualOption, label);
            });
        }

        /// <summary>
        /// adds a real-valued scatter plot into the frame
        /// </summary>
        /// <param name="f"> the frame </param>
        /// <param name="sv"> scatter vector to plot </param>
        /// <param name="lineWidth"> line width </param>
        /// <param name="lineStyle"> line style </param>
        /// <param name="markerSize"> marker size </param>
        /// <param name="markerShape"> marker shape </param>
        /// <param name="plotColor"> marker color </param>
        /// <param name="visualOption"> visual option </param>
        /// <param name="label"> label of the data </param>
        public static void AddToFrame(VFrame f, Scat1DRealData sv,
            double lineWidth = FrameDefaults.LineWidth,
            LineStyle lineStyle = FrameDefaults.LineType,
            double markerSize = FrameDefaults.MarkerSize,
            MarkerShape markerShape = FrameDefaults.MarkerType,
            PlotColor plotColor = FrameDefaults.Color,
            VisualOption visualOption = FrameDefaults.ViewOption,
            string? label = null)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                f.AddScatPlot(sv, lineWidth, lineStyle,
                    markerSize, markerShape, plotColor,
                    visualOption, label);
            });
        }

        #endregion
        #region ===== add complex-valued ScatPlot =====

        /// <summary>
        /// adds a complex-valued scatter plot into the frame
        /// </summary>
        /// <param name="f"> the frame </param>
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
        public static void AddToFrame(VFrame f, VectorD locations, VectorZ values, 
            ComplexPart plotPart = ComplexPart.Magnitude,
            double lineWidth = FrameDefaults.LineWidth,
            LineStyle lineStyle = FrameDefaults.LineType,
            double markerSize = FrameDefaults.MarkerSize,
            MarkerShape markerShape = FrameDefaults.MarkerType,
            PlotColor plotColor = FrameDefaults.Color,
            VisualOption visualOption = FrameDefaults.ViewOption,
            string? label = null)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                f.AddScatPlot(locations, values, plotPart, lineWidth, lineStyle,
                    markerSize, markerShape, plotColor,
                    visualOption, label);
            });
        }

        /// <summary>
        /// adds a complex-valued scatter plot into the frame
        /// </summary>
        /// <param name="f"> the frame </param>
        /// <param name="sv"> scatter vector to plot </param>
        /// <param name="plotPart"> part of the complex data to plot </param>
        /// <param name="lineWidth"> line width </param>
        /// <param name="lineStyle"> line style </param>
        /// <param name="markerSize"> marker size </param>
        /// <param name="markerShape"> marker shape </param>
        /// <param name="plotColor"> marker color </param>
        /// <param name="visualOption"> visual option </param>
        /// <param name="label"> label of the data </param>
        public static void AddToFrame(VFrame f, Scat1DCplxData sv,
            ComplexPart plotPart = ComplexPart.Magnitude,
            double lineWidth = FrameDefaults.LineWidth,
            LineStyle lineStyle = FrameDefaults.LineType,
            double markerSize = FrameDefaults.MarkerSize,
            MarkerShape markerShape = FrameDefaults.MarkerType,
            PlotColor plotColor = FrameDefaults.Color,
            VisualOption visualOption = FrameDefaults.ViewOption,
            string? label = null)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                f.AddScatPlot(sv, plotPart, lineWidth, lineStyle,
                    markerSize, markerShape, plotColor,
                    visualOption, label);
            });
        }

        #endregion
        #region ===== add real-valued FuncPlot ====

        /// <summary>
        /// adds a real-valued function plot into the frame
        /// </summary>
        /// <param name="f"> the frame </param>
        /// <param name="func"> function to plot </param>
        /// <param name="lineWidth"> line width </param>
        /// <param name="lineStyle"> line style </param>
        /// <param name="plotColor"> plot color </param>
        /// <param name="visualOption"> visual options </param>
        /// <param name="label"> label of the data </param>
        public static void AddToFrame(VFrame f, Func<double, double?> func,
            double lineWidth = FrameDefaults.LineWidth,
            LineStyle lineStyle = FrameDefaults.LineType,
            PlotColor plotColor = FrameDefaults.Color,
            VisualOption visualOption = FrameDefaults.ViewOption,
            string? label = null)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                f.AddFuncPlot(func, lineWidth, lineStyle,
                    plotColor, visualOption, label);
            });
        }

        #endregion
        #region ===== add complex-valued FuncPlot =====

        /// <summary>
        /// adds a complex-valued function plot into the frame
        /// </summary>
        /// <param name="f"> the frame </param>
        /// <param name="func"> function to plot </param>
        /// <param name="plotPart"> part of the complex data to plot </param>
        /// <param name="lineWidth"> line width </param>
        /// <param name="lineStyle"> line style </param>
        /// <param name="plotColor"> plot color </param>
        /// <param name="visualOption"> visual options </param>
        /// <param name="label"> label of the data </param>
        public static void AddToFrame(VFrame f, Func<double, Complex?> func, 
            ComplexPart plotPart = ComplexPart.Magnitude,
            double lineWidth = FrameDefaults.LineWidth,
            LineStyle lineStyle = FrameDefaults.LineType,
            PlotColor plotColor = FrameDefaults.Color,
            VisualOption visualOption = FrameDefaults.ViewOption,
            string? label = null)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                f.AddFuncPlot(func, plotPart, lineWidth, lineStyle,
                    plotColor, visualOption, label);
            });
        }

        #endregion
        #region ===== add real-valued GridGraph =====

        /// <summary>
        /// adds a real-valued grid graph into the frame
        /// </summary>
        /// <param name="f"> the frame </param>
        /// <param name="values"> data to plot </param>
        /// <param name="grid"> sampling grid information </param>
        /// <param name="colormap"> colormap used for the graph </param>
        /// <param name="smoothMode"> graph smooth mode </param>
        /// <param name="visualOption"> visual option </param>
        /// <param name="label"> label of the data </param>
        public static void AddToFrame(VFrame f, MatrixD values,
            GridInfo2D? grid = null,
            PlotColorMap colormap = FrameDefaults.Colormap,
            GraphInterpolationMode smoothMode = FrameDefaults.SmoothMode,
            VisualOption visualOption = FrameDefaults.ViewOption,
            string? label = null)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (grid == null) { grid = new(values.Rows, values.Cols); }
                f.AddGridGraph(values, grid, colormap, smoothMode, visualOption, label);
            });
        }

        /// <summary>
        /// adds a real-valued grid graph into the frame
        /// </summary>
        /// <param name="f"> the frame </param>
        /// <param name="gm"> grid matrix to plot </param>
        /// <param name="colormap"> colormap used for the graph </param>
        /// <param name="smoothMode"> graph smooth mode </param>
        /// <param name="visualOption"> visual option </param>
        /// <param name="label"> label of the data </param>
        public static void AddToFrame(VFrame f, Grid2DRealData gm,
            PlotColorMap colormap = FrameDefaults.Colormap,
            GraphInterpolationMode smoothMode = FrameDefaults.SmoothMode,
            VisualOption visualOption = FrameDefaults.ViewOption,
            string? label = null)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                f.AddGridGraph(gm.Values, gm.GridInfo, colormap, smoothMode, visualOption, label);
            });
        }

        #endregion
        #region ===== add complex-valued GridGraph =====

        /// <summary>
        /// adds a complex-valued grid graph into the frame
        /// </summary>
        /// <param name="f"> the frame </param>
        /// <param name="values"> data to plot </param>
        /// <param name="grid"> sampling grid information </param>
        /// <param name="plotPart"> part of the complex data to plot </param>
        /// <param name="colormap"> colormap used for the graph </param>
        /// <param name="smoothMode"> graph smooth mode </param>
        /// <param name="visualOption"> visual option </param>
        /// <param name="label"> label of the data </param>
        public static void AddToFrame(VFrame f, MatrixZ values,
            GridInfo2D? grid = null,
            ComplexPart plotPart = ComplexPart.Magnitude,
            PlotColorMap colormap = FrameDefaults.Colormap,
            GraphInterpolationMode smoothMode = FrameDefaults.SmoothMode,
            VisualOption visualOption = FrameDefaults.ViewOption,
            string? label = null)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (grid == null) { grid = new(values.Rows, values.Cols); }
                f.AddGridGraph(values, grid, plotPart, colormap, smoothMode, visualOption, label);
            });
        }

        /// <summary>
        /// adds a complex-valued grid graph into the frame
        /// </summary>
        /// <param name="f"> the frame </param>
        /// <param name="gm"> grid matrix to plot </param>
        /// <param name="plotPart"> part of the complex data to plot </param>
        /// <param name="colormap"> colormap used for the graph </param>
        /// <param name="smoothMode"> graph smooth mode </param>
        /// <param name="visualOption"> visual option </param>
        /// <param name="label"> label of the data </param>
        public static void AddToFrame(VFrame f, Grid2DCplxData gm,
            ComplexPart plotPart = ComplexPart.Magnitude,
            PlotColorMap colormap = FrameDefaults.Colormap,
            GraphInterpolationMode smoothMode = FrameDefaults.SmoothMode,
            VisualOption visualOption = FrameDefaults.ViewOption,
            string? label = null)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MatrixZ temp = gm.Values;
                gm.Phase.AddTo(x: ref temp, grid: gm.GridInfo,
                    part: ComplexPart.Argument);
                f.AddGridGraph(temp, gm.GridInfo, plotPart, colormap, smoothMode, visualOption, label);
            });
        }

        #endregion
        #region ===== add real-valued ScatGraph =====

        //public static void AddToFrame2D(VFrame f, VectorD x, VectorD y,
        //    double markerSize = FrameCommons.defaultMarkerSize,
        //    MarkerShape markerShape = FrameCommons.defaultMarkerShape,
        //    PlotColor plotColor = FrameCommons.defaultPlotColor,
        //    VisualOption visualOption = FrameCommons.defaultVisualOption,
        //    string? label = null)
        //{
        //    Application.Current.Dispatcher.Invoke(() =>
        //    {
        //        f.AddScatGraph(x, y, markerSize, markerShape,
        //            plotColor, visualOption, label);
        //    });
        //}


        #endregion
        #region ===== add BMPGraph =====

        ///// <summary>
        ///// adds a RGB??? BMPGraph into the frame
        ///// </summary>
        ///// <param name="f"> the frame </param>
        ///// <param name="image"> bitmap image to plot </param>
        ///// <param name="x"> anchor position x </param>
        ///// <param name="y"> anchor position y </param>
        ///// <param name="anchor"> anchor alignment </param>
        ///// <param name="visualOption"> visual option </param>
        ///// <param name="label"> label of the data </param>
        //public static void AddToFrame(VFrame f, Bitmap image,
        //    double x = 0.0,
        //    double y = 0.0,
        //    Alignment anchor = Alignment.UpperLeft,
        //    VisualOption visualOption = FrameCommons.defaultVisualOption,
        //    string? label = null)
        //{
        //    Application.Current.Dispatcher.Invoke(() =>
        //    {
        //        f.AddBMPGraph(image, x, y, anchor, visualOption, label);
        //    });
        //}

        #endregion

        #region ===== refresh & show =====

        /// <summary>
        /// refresh the VFrame and show it
        /// </summary>
        /// <param name="f"> the VFrame </param>
        public static void RefreshShow(VFrame f)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (!f.FrameExt.UserSpecifiedXAxisLimits)
                    f.DetectXAxisLimits();
                if (!f.FrameExt.UserSpecifiedYAxisLimits)
                    f.DetectYAxisLimits();

                f.RefreshShow();
            });
        }

        #endregion

        #endregion
        #region ------- Create + Plot + Show -------

        #region ===== show real-valued GridPlot =====

        /// <summary>
        /// creates a frame, adds a grid plot and show
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
        /// <param name="xAxisTickDensity"> density of the ticks along the horizontal direction </param>
        /// <param name="xAxisTickSize"> font size of the horizontal ticks </param>
        /// <param name="yAxisTickDensity"> density of the ticks along the vertical direction </param>
        /// <param name="yAxisTickSize"> font size of the vertical ticks </param>
        /// <param name="xMin"> minimum value of the horizontal axis </param>
        /// <param name="xMax"> maximum value of the horizontal axis </param>
        /// <param name="yMin"> minimum value of the vertical axis </param>
        /// <param name="yMax"> maximum value of the vertical axis </param>
        /// <param name="title"> title of the frame </param>
        /// <param name="titleSize"> font size of the title </param>
        /// <param name="xLabel"> label of the horizontal axis </param>
        /// <param name="xLabelSize"> font size of the label X </param>
        /// <param name="yLabel"> label of the vertical axis </param>
        /// <param name="yLabelSize"> font size of the label Y </param>
        public static void CreateShow(VectorD values, 
            GridInfo1D? grid = null,
            double lineWidth = FrameDefaults.LineWidth, 
            LineStyle lineStyle = FrameDefaults.LineType,
            double markerSize = FrameDefaults.MarkerSize,
            MarkerShape markerShape = FrameDefaults.MarkerType,
            PlotColor plotColor = FrameDefaults.Color,
            VisualOption visualOption = FrameDefaults.ViewOption,
            string? label = null,
            double xAxisTickDensity = FrameDefaults.TickDensity,
            double xAxisTickSize = FrameDefaults.TickSize,
            double yAxisTickDensity = FrameDefaults.TickDensity,
            double yAxisTickSize = FrameDefaults.TickSize,
            double xMin = FrameDefaults.XMin,
            double xMax = FrameDefaults.XMax,
            double yMin = FrameDefaults.YMin,
            double yMax = FrameDefaults.YMax,
            string title = FrameDefaults.Title,
            double titleSize = FrameDefaults.TitleSize,
            string xLabel = FrameDefaults.LabelX,
            double xLabelSize = FrameDefaults.AxisLabelSize,
            string yLabel = FrameDefaults.LabelY,
            double yLabelSize = FrameDefaults.AxisLabelSize)
        {
            // create frame
            VFrame f = CreateFrame();
            // add plot
            AddToFrame(f, values, grid,
                lineWidth, lineStyle, markerSize, markerShape, plotColor, visualOption, label);
            // set frame parameters
            SetFrameParameters(f, 
                xAxisTickDensity, xAxisTickSize, yAxisTickDensity, yAxisTickSize,
                xMin, xMax, yMin, yMax,
                title, titleSize, xLabel, xLabelSize, yLabel, yLabelSize);
            // refresh and show
            RefreshShow(f);
        }

        /// <summary>
        /// creates a frame, adds a grid plot and show
        /// </summary>
        /// <param name="gv"> grid vector to plot </param>
        /// <param name="lineWidth"> line width </param>
        /// <param name="lineStyle"> line style </param>
        /// <param name="markerSize"> marker size </param>
        /// <param name="markerShape"> marker shape </param>
        /// <param name="plotColor"> plot color </param>
        /// <param name="visualOption"> visual option </param>
        /// <param name="label"> label of the data </param>
        /// <param name="xAxisTickDensity"> density of the ticks along the horizontal direction </param>
        /// <param name="xAxisTickSize"> font size of the horizontal ticks </param>
        /// <param name="yAxisTickDensity"> density of the ticks along the vertical direction </param>
        /// <param name="yAxisTickSize"> font size of the vertical ticks </param>
        /// <param name="xMin"> minimum value of the horizontal axis </param>
        /// <param name="xMax"> maximum value of the horizontal axis </param>
        /// <param name="yMin"> minimum value of the vertical axis </param>
        /// <param name="yMax"> maximum value of the vertical axis </param>
        /// <param name="title"> title of the frame </param>
        /// <param name="titleSize"> font size of the title </param>
        /// <param name="xLabel"> label of the horizontal axis </param>
        /// <param name="xLabelSize"> font size of the label X </param>
        /// <param name="yLabel"> label of the vertical axis </param>
        /// <param name="yLabelSize"> font size of the label Y </param>
        public static void CreateShow(Grid1DRealData gv,
            double lineWidth = FrameDefaults.LineWidth,
            LineStyle lineStyle = FrameDefaults.LineType,
            double markerSize = FrameDefaults.MarkerSize,
            MarkerShape markerShape = FrameDefaults.MarkerType,
            PlotColor plotColor = FrameDefaults.Color,
            VisualOption visualOption = FrameDefaults.ViewOption,
            string? label = null,
            double xAxisTickDensity = FrameDefaults.TickDensity,
            double xAxisTickSize = FrameDefaults.TickSize,
            double yAxisTickDensity = FrameDefaults.TickDensity,
            double yAxisTickSize = FrameDefaults.TickSize,
            double xMin = FrameDefaults.XMin,
            double xMax = FrameDefaults.XMax,
            double yMin = FrameDefaults.YMin,
            double yMax = FrameDefaults.YMax,
            string title = FrameDefaults.Title,
            double titleSize = FrameDefaults.TitleSize,
            string xLabel = FrameDefaults.LabelX,
            double xLabelSize = FrameDefaults.AxisLabelSize,
            string yLabel = FrameDefaults.LabelY,
            double yLabelSize = FrameDefaults.AxisLabelSize)
        {
            VFrame f = CreateFrame();
            // add plot
            AddToFrame(f, gv,
                lineWidth, lineStyle, markerSize, markerShape, plotColor, visualOption, label);
            // set frame parameters
            SetFrameParameters(f,
                xAxisTickDensity, xAxisTickSize, yAxisTickDensity, yAxisTickSize,
                xMin, xMax, yMin, yMax,
                title, titleSize, xLabel, xLabelSize, yLabel, yLabelSize);
            // refresh and show
            RefreshShow(f);
        }

        #endregion
        #region ===== show complex-valued GridPlot =====

        /// <summary>
        /// creates a frame, adds a grid plot and show
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
        /// <param name="xAxisTickDensity"> density of the ticks along the horizontal direction </param>
        /// <param name="xAxisTickSize"> font size of the horizontal ticks </param>
        /// <param name="yAxisTickDensity"> density of the ticks along the vertical direction </param>
        /// <param name="yAxisTickSize"> font size of the vertical ticks </param>
        /// <param name="xMin"> minimum value of the horizontal axis </param>
        /// <param name="xMax"> maximum value of the horizontal axis </param>
        /// <param name="yMin"> minimum value of the vertical axis </param>
        /// <param name="yMax"> maximum value of the vertical axis </param>
        /// <param name="title"> title of the frame </param>
        /// <param name="titleSize"> font size of the title </param>
        /// <param name="xLabel"> label of the horizontal axis </param>
        /// <param name="xLabelSize"> font size of the label X </param>
        /// <param name="yLabel"> label of the vertical axis </param>
        /// <param name="yLabelSize"> font size of the label Y </param>
        public static void CreateShow(VectorZ values, 
            GridInfo1D? grid = null,
            ComplexPart plotPart = ComplexPart.Magnitude,
            double lineWidth = FrameDefaults.LineWidth,
            LineStyle lineStyle = FrameDefaults.LineType,
            double markerSize = FrameDefaults.MarkerSize,
            MarkerShape markerShape = FrameDefaults.MarkerType,
            PlotColor plotColor = FrameDefaults.Color,
            VisualOption visualOption = FrameDefaults.ViewOption,
            string? label = null,
            double xAxisTickDensity = FrameDefaults.TickDensity,
            double xAxisTickSize = FrameDefaults.TickSize,
            double yAxisTickDensity = FrameDefaults.TickDensity,
            double yAxisTickSize = FrameDefaults.TickSize,
            double xMin = FrameDefaults.XMin,
            double xMax = FrameDefaults.XMax,
            double yMin = FrameDefaults.YMin,
            double yMax = FrameDefaults.YMax,
            string title = FrameDefaults.Title,
            double titleSize = FrameDefaults.TitleSize,
            string xLabel = FrameDefaults.LabelX,
            double xLabelSize = FrameDefaults.AxisLabelSize,
            string yLabel = FrameDefaults.LabelY,
            double yLabelSize = FrameDefaults.AxisLabelSize)
        {
            // create frame
            VFrame f = CreateFrame();
            // add plot
            AddToFrame(f, values, grid, plotPart,
                lineWidth, lineStyle, markerSize, markerShape, plotColor, visualOption, label);
            // set frame parameters
            SetFrameParameters(f,
                xAxisTickDensity, xAxisTickSize, yAxisTickDensity, yAxisTickSize,
                xMin, xMax, yMin, yMax,
                title, titleSize, xLabel, xLabelSize, yLabel, yLabelSize);
            // refresh and show
            RefreshShow(f);
        }

        /// <summary>
        /// creates a frame, adds a grid plot and show
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
        /// <param name="xAxisTickDensity"> density of the ticks along the horizontal direction </param>
        /// <param name="xAxisTickSize"> font size of the horizontal ticks </param>
        /// <param name="yAxisTickDensity"> density of the ticks along the vertical direction </param>
        /// <param name="yAxisTickSize"> font size of the vertical ticks </param>
        /// <param name="xMin"> minimum value of the horizontal axis </param>
        /// <param name="xMax"> maximum value of the horizontal axis </param>
        /// <param name="yMin"> minimum value of the vertical axis </param>
        /// <param name="yMax"> maximum value of the vertical axis </param>
        /// <param name="title"> title of the frame </param>
        /// <param name="titleSize"> font size of the title </param>
        /// <param name="xLabel"> label of the horizontal axis </param>
        /// <param name="xLabelSize"> font size of the label X </param>
        /// <param name="yLabel"> label of the vertical axis </param>
        /// <param name="yLabelSize"> font size of the label Y </param>
        public static void CreateShow(Grid1DCplxData gv, 
            ComplexPart plotPart = ComplexPart.Magnitude,
            double lineWidth = FrameDefaults.LineWidth,
            LineStyle lineStyle = FrameDefaults.LineType,
            double markerSize = FrameDefaults.MarkerSize,
            MarkerShape markerShape = FrameDefaults.MarkerType,
            PlotColor plotColor = FrameDefaults.Color,
            VisualOption visualOption = FrameDefaults.ViewOption,
            string? label = null,
            double xAxisTickDensity = FrameDefaults.TickDensity,
            double xAxisTickSize = FrameDefaults.TickSize,
            double yAxisTickDensity = FrameDefaults.TickDensity,
            double yAxisTickSize = FrameDefaults.TickSize,
            double xMin = FrameDefaults.XMin,
            double xMax = FrameDefaults.XMax,
            double yMin = FrameDefaults.YMin,
            double yMax = FrameDefaults.YMax,
            string title = FrameDefaults.Title,
            double titleSize = FrameDefaults.TitleSize,
            string xLabel = FrameDefaults.LabelX,
            double xLabelSize = FrameDefaults.AxisLabelSize,
            string yLabel = FrameDefaults.LabelY,
            double yLabelSize = FrameDefaults.AxisLabelSize)
        {
            // create frame
            VFrame f = CreateFrame();
            // add plot
            AddToFrame(f, gv, plotPart,
                lineWidth, lineStyle, markerSize, markerShape, plotColor, visualOption, label);
            // set frame parameters
            SetFrameParameters(f,
                xAxisTickDensity, xAxisTickSize, yAxisTickDensity, yAxisTickSize,
                xMin, xMax, yMin, yMax,
                title, titleSize, xLabel, xLabelSize, yLabel, yLabelSize);
            // refresh and show
            RefreshShow(f);
        }

        #endregion
        #region ===== show real-valued ScatPlot =====

        /// <summary>
        /// creates a frame, adds a scat plot and show
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
        /// <param name="xAxisTickDensity"> density of the ticks along the horizontal direction </param>
        /// <param name="xAxisTickSize"> font size of the horizontal ticks </param>
        /// <param name="yAxisTickDensity"> density of the ticks along the vertical direction </param>
        /// <param name="yAxisTickSize"> font size of the vertical ticks </param>
        /// <param name="xMin"> minimum value of the horizontal axis </param>
        /// <param name="xMax"> maximum value of the horizontal axis </param>
        /// <param name="yMin"> minimum value of the vertical axis </param>
        /// <param name="yMax"> maximum value of the vertical axis </param>
        /// <param name="title"> title of the frame </param>
        /// <param name="titleSize"> font size of the title </param>
        /// <param name="xLabel"> label of the horizontal axis </param>
        /// <param name="xLabelSize"> font size of the label X </param>
        /// <param name="yLabel"> label of the vertical axis </param>
        /// <param name="yLabelSize"> font size of the label Y </param>
        public static void CreateShow(VectorD locations, VectorD values,
            double lineWidth = FrameDefaults.LineWidth,
            LineStyle lineStyle = FrameDefaults.LineType,
            double markerSize = FrameDefaults.MarkerSize,
            MarkerShape markerShape = FrameDefaults.MarkerType,
            PlotColor plotColor = FrameDefaults.Color,
            VisualOption visualOption = FrameDefaults.ViewOption,
            string? label = null,
            double xAxisTickDensity = FrameDefaults.TickDensity,
            double xAxisTickSize = FrameDefaults.TickSize,
            double yAxisTickDensity = FrameDefaults.TickDensity,
            double yAxisTickSize = FrameDefaults.TickSize,
            double xMin = FrameDefaults.XMin,
            double xMax = FrameDefaults.XMax,
            double yMin = FrameDefaults.YMin,
            double yMax = FrameDefaults.YMax,
            string title = FrameDefaults.Title,
            double titleSize = FrameDefaults.TitleSize,
            string xLabel = FrameDefaults.LabelX,
            double xLabelSize = FrameDefaults.AxisLabelSize,
            string yLabel = FrameDefaults.LabelY,
            double yLabelSize = FrameDefaults.AxisLabelSize)
        {
            // create frame
            VFrame f = CreateFrame();
            // add plot
            AddToFrame(f, locations, values,
                lineWidth, lineStyle, markerSize, markerShape, plotColor, visualOption, label);
            // set frame parameters
            SetFrameParameters(f,
                xAxisTickDensity, xAxisTickSize, yAxisTickDensity, yAxisTickSize,
                xMin, xMax, yMin, yMax,
                title, titleSize, xLabel, xLabelSize, yLabel, yLabelSize);
            // refresh and show
            RefreshShow(f);
        }

        /// <summary>
        /// creates a frame, adds a scat plot and show
        /// </summary>
        /// <param name="sv"> scatter vector to plot </param>
        /// <param name="lineWidth"> line width </param>
        /// <param name="lineStyle"> line style </param>
        /// <param name="markerSize"> marker size </param>
        /// <param name="markerShape"> marker shape </param>
        /// <param name="plotColor"> marker color </param>
        /// <param name="visualOption"> visual option </param>
        /// <param name="label"> label of the data </param>
        /// <param name="xAxisTickDensity"> density of the ticks along the horizontal direction </param>
        /// <param name="xAxisTickSize"> font size of the horizontal ticks </param>
        /// <param name="yAxisTickDensity"> density of the ticks along the vertical direction </param>
        /// <param name="yAxisTickSize"> font size of the vertical ticks </param>
        /// <param name="xMin"> minimum value of the horizontal axis </param>
        /// <param name="xMax"> maximum value of the horizontal axis </param>
        /// <param name="yMin"> minimum value of the vertical axis </param>
        /// <param name="yMax"> maximum value of the vertical axis </param>
        /// <param name="title"> title of the frame </param>
        /// <param name="titleSize"> font size of the title </param>
        /// <param name="xLabel"> label of the horizontal axis </param>
        /// <param name="xLabelSize"> font size of the label X </param>
        /// <param name="yLabel"> label of the vertical axis </param>
        /// <param name="yLabelSize"> font size of the label Y </param>
        public static void CreateShow(Scat1DRealData sv,
            double lineWidth = FrameDefaults.LineWidth,
            LineStyle lineStyle = FrameDefaults.LineType,
            double markerSize = FrameDefaults.MarkerSize,
            MarkerShape markerShape = FrameDefaults.MarkerType,
            PlotColor plotColor = FrameDefaults.Color,
            VisualOption visualOption = FrameDefaults.ViewOption,
            string? label = null,
            double xAxisTickDensity = FrameDefaults.TickDensity,
            double xAxisTickSize = FrameDefaults.TickSize,
            double yAxisTickDensity = FrameDefaults.TickDensity,
            double yAxisTickSize = FrameDefaults.TickSize,
            double xMin = FrameDefaults.XMin,
            double xMax = FrameDefaults.XMax,
            double yMin = FrameDefaults.YMin,
            double yMax = FrameDefaults.YMax,
            string title = FrameDefaults.Title,
            double titleSize = FrameDefaults.TitleSize,
            string xLabel = FrameDefaults.LabelX,
            double xLabelSize = FrameDefaults.AxisLabelSize,
            string yLabel = FrameDefaults.LabelY,
            double yLabelSize = FrameDefaults.AxisLabelSize)
        {
            // create frame
            VFrame f = CreateFrame();
            // add plot
            AddToFrame(f, sv,
                lineWidth, lineStyle, markerSize, markerShape, plotColor, visualOption, label);
            // set frame parameters
            SetFrameParameters(f,
                xAxisTickDensity, xAxisTickSize, yAxisTickDensity, yAxisTickSize,
                xMin, xMax, yMin, yMax,
                title, titleSize, xLabel, xLabelSize, yLabel, yLabelSize);
            // refresh and show
            RefreshShow(f);
        }

        #endregion
        #region ===== show complex-valued ScatPlot =====

        /// <summary>
        /// creates a frame, adds a scat plot and show
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
        /// <param name="xAxisTickDensity"> density of the ticks along the horizontal direction </param>
        /// <param name="xAxisTickSize"> font size of the horizontal ticks </param>
        /// <param name="yAxisTickDensity"> density of the ticks along the vertical direction </param>
        /// <param name="yAxisTickSize"> font size of the vertical ticks </param>
        /// <param name="xMin"> minimum value of the horizontal axis </param>
        /// <param name="xMax"> maximum value of the horizontal axis </param>
        /// <param name="yMin"> minimum value of the vertical axis </param>
        /// <param name="yMax"> maximum value of the vertical axis </param>
        /// <param name="title"> title of the frame </param>
        /// <param name="titleSize"> font size of the title </param>
        /// <param name="xLabel"> label of the horizontal axis </param>
        /// <param name="xLabelSize"> font size of the label X </param>
        /// <param name="yLabel"> label of the vertical axis </param>
        /// <param name="yLabelSize"> font size of the label Y </param>
        public static void CreateShow(VectorD locations, VectorZ values, 
            ComplexPart plotPart = ComplexPart.Magnitude,
            double lineWidth = FrameDefaults.LineWidth,
            LineStyle lineStyle = FrameDefaults.LineType,
            double markerSize = FrameDefaults.MarkerSize,
            MarkerShape markerShape = FrameDefaults.MarkerType,
            PlotColor plotColor = FrameDefaults.Color,
            VisualOption visualOption = FrameDefaults.ViewOption,
            string? label = null,
            double xAxisTickDensity = FrameDefaults.TickDensity,
            double xAxisTickSize = FrameDefaults.TickSize,
            double yAxisTickDensity = FrameDefaults.TickDensity,
            double yAxisTickSize = FrameDefaults.TickSize,
            double xMin = FrameDefaults.XMin,
            double xMax = FrameDefaults.XMax,
            double yMin = FrameDefaults.YMin,
            double yMax = FrameDefaults.YMax,
            string title = FrameDefaults.Title,
            double titleSize = FrameDefaults.TitleSize,
            string xLabel = FrameDefaults.LabelX,
            double xLabelSize = FrameDefaults.AxisLabelSize,
            string yLabel = FrameDefaults.LabelY,
            double yLabelSize = FrameDefaults.AxisLabelSize)
        {
            // create frame
            VFrame f = CreateFrame();
            // add plot
            AddToFrame(f, locations, values, plotPart,
                lineWidth, lineStyle, markerSize, markerShape, plotColor, visualOption, label);
            // set frame parameters
            SetFrameParameters(f,
                xAxisTickDensity, xAxisTickSize, yAxisTickDensity, yAxisTickSize,
                xMin, xMax, yMin, yMax,
                title, titleSize, xLabel, xLabelSize, yLabel, yLabelSize);
            // refresh and show
            RefreshShow(f);
        }

        /// <summary>
        /// creates a frame, adds a scat plot and show
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
        /// <param name="xAxisTickDensity"> density of the ticks along the horizontal direction </param>
        /// <param name="xAxisTickSize"> font size of the horizontal ticks </param>
        /// <param name="yAxisTickDensity"> density of the ticks along the vertical direction </param>
        /// <param name="yAxisTickSize"> font size of the vertical ticks </param>
        /// <param name="xMin"> minimum value of the horizontal axis </param>
        /// <param name="xMax"> maximum value of the horizontal axis </param>
        /// <param name="yMin"> minimum value of the vertical axis </param>
        /// <param name="yMax"> maximum value of the vertical axis </param>
        /// <param name="title"> title of the frame </param>
        /// <param name="titleSize"> font size of the title </param>
        /// <param name="xLabel"> label of the horizontal axis </param>
        /// <param name="xLabelSize"> font size of the label X </param>
        /// <param name="yLabel"> label of the vertical axis </param>
        /// <param name="yLabelSize"> font size of the label Y </param>
        public static void CreateShow(Scat1DCplxData sv, 
            ComplexPart plotPart = ComplexPart.Magnitude,
            double lineWidth = FrameDefaults.LineWidth,
            LineStyle lineStyle = FrameDefaults.LineType,
            double markerSize = FrameDefaults.MarkerSize,
            MarkerShape markerShape = FrameDefaults.MarkerType,
            PlotColor plotColor = FrameDefaults.Color,
            VisualOption visualOption = FrameDefaults.ViewOption,
            string? label = null,
            double xAxisTickDensity = FrameDefaults.TickDensity,
            double xAxisTickSize = FrameDefaults.TickSize,
            double yAxisTickDensity = FrameDefaults.TickDensity,
            double yAxisTickSize = FrameDefaults.TickSize,
            double xMin = FrameDefaults.XMin,
            double xMax = FrameDefaults.XMax,
            double yMin = FrameDefaults.YMin,
            double yMax = FrameDefaults.YMax,
            string title = FrameDefaults.Title,
            double titleSize = FrameDefaults.TitleSize,
            string xLabel = FrameDefaults.LabelX,
            double xLabelSize = FrameDefaults.AxisLabelSize,
            string yLabel = FrameDefaults.LabelY,
            double yLabelSize = FrameDefaults.AxisLabelSize)
        {
            // create frame
            VFrame f = CreateFrame();
            // add plot
            AddToFrame(f, sv, plotPart,
                lineWidth, lineStyle, markerSize, markerShape, plotColor, visualOption, label);
            // set frame parameters
            SetFrameParameters(f,
                xAxisTickDensity, xAxisTickSize, yAxisTickDensity, yAxisTickSize,
                xMin, xMax, yMin, yMax,
                title, titleSize, xLabel, xLabelSize, yLabel, yLabelSize);
            // refresh and show
            RefreshShow(f);
        }

        #endregion
        #region ===== show real-valued GridGraph ===== 

        /// <summary>
        /// creates a frame, adds a grid graph and show
        /// </summary>
        /// <param name="values"> data to plot </param>
        /// <param name="grid"> sampling grid info </param>
        /// <param name="colormap"> colormap for the graph </param>
        /// <param name="smoothMode"> graph smooth mode </param>
        /// <param name="visualOption"> visual option </param>
        /// <param name="label"> label of the data </param>
        /// <param name="xAxisTickDensity"> density of the ticks along the horizontal direction </param>
        /// <param name="xAxisTickSize"> font size of the horizontal ticks </param>
        /// <param name="yAxisTickDensity"> density of the ticks along the vertical direction </param>
        /// <param name="yAxisTickSize"> font size of the vertical ticks </param>
        /// <param name="xMin"> minimum value of the horizontal axis </param>
        /// <param name="xMax"> maximum value of the horizontal axis </param>
        /// <param name="yMin"> minimum value of the vertical axis </param>
        /// <param name="yMax"> maximum value of the vertical axis </param>
        /// <param name="title"> title of the frame </param>
        /// <param name="titleSize"> font size of the title </param>
        /// <param name="xLabel"> label of the horizontal axis </param>
        /// <param name="xLabelSize"> font size of the label X </param>
        /// <param name="yLabel"> label of the vertical axis </param>
        /// <param name="yLabelSize"> font size of the label Y </param>
        public static void CreateShow(MatrixD values, 
            GridInfo2D? grid = null,
            PlotColorMap colormap = FrameDefaults.Colormap,
            GraphInterpolationMode smoothMode = FrameDefaults.SmoothMode,
            VisualOption visualOption = FrameDefaults.ViewOption,
            string? label = null,
            double xAxisTickDensity = FrameDefaults.TickDensity,
            double xAxisTickSize = FrameDefaults.TickSize,
            double yAxisTickDensity = FrameDefaults.TickDensity,
            double yAxisTickSize = FrameDefaults.TickSize,
            double xMin = FrameDefaults.XMin,
            double xMax = FrameDefaults.XMax,
            double yMin = FrameDefaults.YMin,
            double yMax = FrameDefaults.YMax,
            string title = FrameDefaults.Title,
            double titleSize = FrameDefaults.TitleSize,
            string xLabel = FrameDefaults.LabelX,
            double xLabelSize = FrameDefaults.AxisLabelSize,
            string yLabel = FrameDefaults.LabelY,
            double yLabelSize = FrameDefaults.AxisLabelSize)
        {
            // create frame
            VFrame f = CreateFrame();
            // add plot
            AddToFrame(f, values, grid,
                colormap, smoothMode, visualOption, label);
            // set frame parameters
            SetFrameParameters(f,
                xAxisTickDensity, xAxisTickSize, yAxisTickDensity, yAxisTickSize,
                xMin, xMax, yMin, yMax,
                title, titleSize, xLabel, xLabelSize, yLabel, yLabelSize);
            // refresh and show
            RefreshShow(f);
        }

        /// <summary>
        /// creates a frame, adds a grid graph and show
        /// </summary>
        /// <param name="values"> data to plot </param>
        /// <param name="colormap"> colormap for the graph </param>
        /// <param name="smoothMode"> graph smooth mode </param>
        /// <param name="visualOption"> visual option </param>
        /// <param name="label"> label of the data </param>
        /// <param name="xAxisTickDensity"> density of the ticks along the horizontal direction </param>
        /// <param name="xAxisTickSize"> font size of the horizontal ticks </param>
        /// <param name="yAxisTickDensity"> density of the ticks along the vertical direction </param>
        /// <param name="yAxisTickSize"> font size of the vertical ticks </param>
        /// <param name="xMin"> minimum value of the horizontal axis </param>
        /// <param name="xMax"> maximum value of the horizontal axis </param>
        /// <param name="yMin"> minimum value of the vertical axis </param>
        /// <param name="yMax"> maximum value of the vertical axis </param>
        /// <param name="title"> title of the frame </param>
        /// <param name="titleSize"> font size of the title </param>
        /// <param name="xLabel"> label of the horizontal axis </param>
        /// <param name="xLabelSize"> font size of the label X </param>
        /// <param name="yLabel"> label of the vertical axis </param>
        /// <param name="yLabelSize"> font size of the label Y </param>
        public static void CreateShow(Grid2DRealData values,
            PlotColorMap colormap = FrameDefaults.Colormap,
            GraphInterpolationMode smoothMode = FrameDefaults.SmoothMode,
            VisualOption visualOption = FrameDefaults.ViewOption,
            string? label = null,
            double xAxisTickDensity = FrameDefaults.TickDensity,
            double xAxisTickSize = FrameDefaults.TickSize,
            double yAxisTickDensity = FrameDefaults.TickDensity,
            double yAxisTickSize = FrameDefaults.TickSize,
            double xMin = FrameDefaults.XMin,
            double xMax = FrameDefaults.XMax,
            double yMin = FrameDefaults.YMin,
            double yMax = FrameDefaults.YMax,
            string title = FrameDefaults.Title,
            double titleSize = FrameDefaults.TitleSize,
            string xLabel = FrameDefaults.LabelX,
            double xLabelSize = FrameDefaults.AxisLabelSize,
            string yLabel = FrameDefaults.LabelY,
            double yLabelSize = FrameDefaults.AxisLabelSize)
        {
            // create frame
            VFrame f = CreateFrame();
            // add plot
            AddToFrame(f, values,
                colormap, smoothMode, visualOption, label);
            // set frame parameters
            SetFrameParameters(f,
                xAxisTickDensity, xAxisTickSize, yAxisTickDensity, yAxisTickSize,
                xMin, xMax, yMin, yMax,
                title, titleSize, xLabel, xLabelSize, yLabel, yLabelSize);
            // refresh and show
            RefreshShow(f);
        }

        #endregion
        #region ===== show complex-valued GridGraph =====

        /// <summary>
        /// creates a frame, adds a grid graph and show
        /// </summary>
        /// <param name="values"> data to plot </param>
        /// <param name="grid"> sampling grid information </param>
        /// <param name="plotPart"> part of the complex data to plot </param>
        /// <param name="colormap"> colormap for the graph </param>
        /// <param name="smoothMode"> graph smooth mode </param>
        /// <param name="visualOption"> visual option </param>
        /// <param name="label"> label of the data </param>
        /// <param name="xAxisTickDensity"> density of the ticks along the horizontal direction </param>
        /// <param name="xAxisTickSize"> font size of the horizontal ticks </param>
        /// <param name="yAxisTickDensity"> density of the ticks along the vertical direction </param>
        /// <param name="yAxisTickSize"> font size of the vertical ticks </param>
        /// <param name="xMin"> minimum value of the horizontal axis </param>
        /// <param name="xMax"> maximum value of the horizontal axis </param>
        /// <param name="yMin"> minimum value of the vertical axis </param>
        /// <param name="yMax"> maximum value of the vertical axis </param>
        /// <param name="title"> title of the frame </param>
        /// <param name="titleSize"> font size of the title </param>
        /// <param name="xLabel"> label of the horizontal axis </param>
        /// <param name="xLabelSize"> font size of the label X </param>
        /// <param name="yLabel"> label of the vertical axis </param>
        /// <param name="yLabelSize"> font size of the label Y </param>
        public static void CreateShow(MatrixZ values,
            GridInfo2D? grid = null,
            ComplexPart plotPart = ComplexPart.Magnitude,
            PlotColorMap colormap = FrameDefaults.Colormap,
            GraphInterpolationMode smoothMode = FrameDefaults.SmoothMode,
            VisualOption visualOption = FrameDefaults.ViewOption,
            string? label = null,
            double xAxisTickDensity = FrameDefaults.TickDensity,
            double xAxisTickSize = FrameDefaults.TickSize,
            double yAxisTickDensity = FrameDefaults.TickDensity,
            double yAxisTickSize = FrameDefaults.TickSize,
            double xMin = FrameDefaults.XMin,
            double xMax = FrameDefaults.XMax,
            double yMin = FrameDefaults.YMin,
            double yMax = FrameDefaults.YMax,
            string title = FrameDefaults.Title,
            double titleSize = FrameDefaults.TitleSize,
            string xLabel = FrameDefaults.LabelX,
            double xLabelSize = FrameDefaults.AxisLabelSize,
            string yLabel = FrameDefaults.LabelY,
            double yLabelSize = FrameDefaults.AxisLabelSize)
        {
            // create frame
            VFrame f = CreateFrame();
            // add plot
            AddToFrame(f, values, grid, plotPart,
                colormap, smoothMode, visualOption, label);
            // set frame parameters
            SetFrameParameters(f,
                xAxisTickDensity, xAxisTickSize, yAxisTickDensity, yAxisTickSize,
                xMin, xMax, yMin, yMax,
                title, titleSize, xLabel, xLabelSize, yLabel, yLabelSize);
            // refresh and show
            RefreshShow(f);
        }

        /// <summary>
        /// creates a frame, adds a grid graph and show
        /// </summary>
        /// <param name="values"> data to plot </param>
        /// <param name="plotPart"> part of the complex data to plot </param>
        /// <param name="colormap"> colormap for the graph </param>
        /// <param name="smoothMode"> graph smooth mode </param>
        /// <param name="visualOption"> visual option </param>
        /// <param name="label"> label of the data </param>
        /// <param name="xAxisTickDensity"> density of the ticks along the horizontal direction </param>
        /// <param name="xAxisTickSize"> font size of the horizontal ticks </param>
        /// <param name="yAxisTickDensity"> density of the ticks along the vertical direction </param>
        /// <param name="yAxisTickSize"> font size of the vertical ticks </param>
        /// <param name="xMin"> minimum value of the horizontal axis </param>
        /// <param name="xMax"> maximum value of the horizontal axis </param>
        /// <param name="yMin"> minimum value of the vertical axis </param>
        /// <param name="yMax"> maximum value of the vertical axis </param>
        /// <param name="title"> title of the frame </param>
        /// <param name="titleSize"> font size of the title </param>
        /// <param name="xLabel"> label of the horizontal axis </param>
        /// <param name="xLabelSize"> font size of the label X </param>
        /// <param name="yLabel"> label of the vertical axis </param>
        /// <param name="yLabelSize"> font size of the label Y </param>
        public static void CreateShow(Grid2DCplxData values,
            ComplexPart plotPart = ComplexPart.Magnitude,
            PlotColorMap colormap = FrameDefaults.Colormap,
            GraphInterpolationMode smoothMode = FrameDefaults.SmoothMode,
            VisualOption visualOption = FrameDefaults.ViewOption,
            string? label = null,
            double xAxisTickDensity = FrameDefaults.TickDensity,
            double xAxisTickSize = FrameDefaults.TickSize,
            double yAxisTickDensity = FrameDefaults.TickDensity,
            double yAxisTickSize = FrameDefaults.TickSize,
            double xMin = FrameDefaults.XMin,
            double xMax = FrameDefaults.XMax,
            double yMin = FrameDefaults.YMin,
            double yMax = FrameDefaults.YMax,
            string title = FrameDefaults.Title,
            double titleSize = FrameDefaults.TitleSize,
            string xLabel = FrameDefaults.LabelX,
            double xLabelSize = FrameDefaults.AxisLabelSize,
            string yLabel = FrameDefaults.LabelY,
            double yLabelSize = FrameDefaults.AxisLabelSize)
        {
            // create frame
            VFrame f = CreateFrame();
            // add plot
            AddToFrame(f, values, plotPart,
                colormap, smoothMode, visualOption, label);
            // set frame parameters
            SetFrameParameters(f,
                xAxisTickDensity, xAxisTickSize, yAxisTickDensity, yAxisTickSize,
                xMin, xMax, yMin, yMax,
                title, titleSize, xLabel, xLabelSize, yLabel, yLabelSize);
            // refresh and show
            RefreshShow(f);
        }

        #endregion

        #endregion

        #endregion

    }
}
