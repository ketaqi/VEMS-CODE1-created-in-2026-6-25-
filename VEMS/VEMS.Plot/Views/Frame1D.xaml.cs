using ScottPlot;
using ScottPlot.Plottable;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using VEMS.MathCore;
using Color = System.Drawing.Color;
using PlotColor = VEMS.Plot.Options.PlotColor;
using VisualOption = VEMS.Plot.Options.VisualOption;

namespace VEMS.Plot
{
    /// <summary>
    /// Frame1D.xaml 的交互逻辑
    /// </summary>
    public partial class Frame1D : Window
    {
        #region constants

        private double fullWidth = 1080;
        private double advWidth = 340;
        
        private static double defaultLineWidth = 4.0;
        private LineStyle defaultLineStyle = LineStyle.Solid;

        private double defaultMarkerSize = 7.0;
        private MarkerShape defaultMarkerStyle = MarkerShape.filledCircle;

        private PlotColor defaultPlotColor = PlotColor.Black;

        #endregion
        #region field - view range

        /// <summary>
        /// minimum view range along the horizontal direction
        /// </summary>
        public NumberQuantity XRangeMin = new()
        {
            Name = "Horizontal Minimum",
            Value = -2.0,
            Unit = 1.0
        };
        /// <summary>
        /// maximum view range along the horizontal direction
        /// </summary>
        public NumberQuantity XRangeMax = new()
        {
            Name = "Horizontal Maximum",
            Value = 2.0,
            Unit = 1.0
        };
        /// <summary>
        /// minimum view range along the vertical direction
        /// </summary>
        public NumberQuantity YRangeMin = new() 
        { 
            Name = "Vertical Minimum", 
            Value = 0.5,
            Unit = 1.0
        };
        /// <summary>
        /// maximum view range along the vertical direction
        /// </summary>
        public NumberQuantity YRangeMax = new() 
        { 
            Name = "Vertical Maximum", 
            Value = 3.5,
            Unit = 1.0
        };

        #endregion
        #region field - axis tick

        /// <summary>
        /// tick density along the horizontal direction
        /// </summary>
        public NumberQuantity XTick = new()
        {
            Name = "Horizontal Ticks",
            Value = 1.0,
            Unit = 12.0 // font size
        };
        /// <summary>
        /// tick font size along the horizontal direction
        /// </summary>
        //public NumberTuple XTickFontSize = new()
        //{
        //    DisplayName = "HOR. Font Size",
        //    NumValue = 12.0
        //};
        /// <summary>
        /// tick density along the vertical direction
        /// </summary>
        public NumberQuantity YTick = new()
        {
            Name = "Vertical Ticks",
            Value = 1.0,
            Unit = 12.0 // font size
        };
        /// <summary>
        /// tick font size along the vertical direction
        /// </summary>
        //public NumberTuple YTickFontSize = new()
        //{
        //    DisplayName = "VER. Font Size",
        //    NumValue = 12.0
        //};

        #endregion
        #region field - texts

        /// <summary>
        /// title of the plot frame
        /// </summary>
        public TextQuantity FrameTitle = new()
        {
            Name = "Frame Title",
            Value = "VEMS Plot(s)",
            Unit = 20.0
        };
        /// <summary>
        /// label of the horizontal axis
        /// </summary>
        public TextQuantity XAxisLabel = new()
        {
            Name = "Horizontal Label",
            Value = "X",
            Unit = 18.0
        };
        /// <summary>
        /// label of the vertical axis
        /// </summary>
        public TextQuantity YAxisLabel = new()
        {
            Name = "Vertical Label",
            Value = "Y",
            Unit = 18.0
        };

        #endregion
        #region fields - cursor info

        /// <summary>
        /// physical location X of the cursor 
        /// </summary>
        public NumberQuantity PhysicalX = new()
        {
            Name = "Horizontal Location (X)",
            Value = 0.0,
            Unit = 1.0
        };
        /// <summary>
        /// physical location Y of the cursor
        /// </summary>
        public NumberQuantity PhysicalY = new()
        {
            Name = "Vertical Location (Y)",
            Value = 0.0,
            Unit = 1.0
        };
        /// <summary>
        /// pixel location X of the cursor
        /// </summary>
        public NumberQuantity PixelX = new()
        {
            Name = "Horizontal Pixel",
            Value = 0.0,
            Unit = 1.0
        };
        /// <summary>
        /// pixel location Y of the cursor
        /// </summary>
        public NumberQuantity PixelY = new()
        {
            Name = "Vertical Pixel",
            Value = 0.0,
            Unit = 1.0
        };

        #endregion

        #region properties

        /// <summary>
        /// list of view ranges
        /// </summary>
        public List<NumberQuantity> ViewRange { get; set; }

        /// <summary>
        /// list of axis tickes
        /// </summary>
        public List<NumberQuantity> AxisTick { get; set; }

        /// <summary>
        /// list of title and axis labels
        /// </summary>
        public List<TextQuantity> TitleLabel { get; set; }

        /// <summary>
        /// list of cursor location information
        /// </summary>
        public List<NumberQuantity> CursorInfo { get; set; }


        private bool _legendVisibility { get; set; }
        private Alignment _legendLocation { get; set; }

        /// <summary>
        /// list of all the data for the plots
        /// </summary>
        public List<Object> AllData { get; set; }

        /// <summary>
        /// list of all the plots in the frame
        /// </summary>
        public List<Object> AllPlots { get; set; }


        public bool LegendVisibility
        {
            get => _legendVisibility;
            set
            {
                _legendVisibility = value;
                //OnPropertyChanged(nameof(LegendVisibility));
                PlotFrame.Plot.Legend(_legendVisibility, _legendLocation);
                PlotFrame.Refresh();
            }
        }
        public Alignment LegendLocation
        {
            get => _legendLocation;
            set
            {
                _legendLocation = value;
                //OnPropertyChanged(nameof(_legendLocation));
                PlotFrame.Plot.Legend(_legendVisibility, _legendLocation);
                PlotFrame.Refresh();
            }
        }


        #endregion
        #region constructors

        /// <summary>
        /// constructs an empty Frame1D class
        /// </summary>
        public Frame1D()
        {
            InitializeComponent();

            // set the view range by default
            SetAxisXLimits(min: XRangeMin.Value, max: XRangeMax.Value);
            SetAxisYLimits(min: YRangeMin.Value, max: YRangeMax.Value);

            // axis tick by default
            SetAxisXDensity(density: XTick.Value, fontBold: false, fontSize: XTick.Unit);
            SetAxisYDensity(density: YTick.Value, fontBold: false, fontSize: YTick.Unit);
            
            // frame text by default
            SetTitle(content: FrameTitle.Value, fontBold: true, fontSize: FrameTitle.Unit);
            SetLabelX(content: XAxisLabel.Value, fontBold: false, fontSize: XAxisLabel.Unit);
            SetLabelY(content: YAxisLabel.Value, fontBold: false, fontSize: YAxisLabel.Unit);
            SetLabelYRight(content: "");

            // set ItemsSource for the data grids
            ViewRange = new List<NumberQuantity> { XRangeMin, XRangeMax, YRangeMin, YRangeMax };
            RangeDataGrid.ItemsSource = ViewRange;
            AxisTick = new List<NumberQuantity> { XTick, YTick };
            TickDataGrid.ItemsSource = AxisTick;
            TitleLabel = new List<TextQuantity> { FrameTitle, XAxisLabel, YAxisLabel };
            TextDataGrid.ItemsSource = TitleLabel;
            CursorInfo = new List<NumberQuantity> { PhysicalX, PhysicalY, PixelX, PixelY };
            CursorDataGrid.ItemsSource = CursorInfo;

            // plot frame settings
            PlotFrame.RightClicked -= PlotFrame.DefaultRightClickEvent;
            PlotFrame.MouseMove += GetCursorInfo;
            PlotFrame.AxesChanged += GetAxisLimits;

            // initialize plot list
            AllData = new();
            AllPlots = new();
        }

        /// <summary>
        /// constructs a Frame1D class, with given
        /// title, labels, tick densities, axis limits
        /// </summary>
        /// <param name="title"> title of the frame </param>
        /// <param name="labelX"> label along horitonzal direction </param>
        /// <param name="labelY"> label along vertical direction </param>
        /// <param name="xTickDensity"> tick density along horizontal direction </param>
        /// <param name="yTickDensity"> tick density along vertical direction </param>
        /// <param name="xMin"> minimum value along horizontal direction </param>
        /// <param name="xMax"> maximum value along horizontal direction </param>
        /// <param name="yMin"> minimum value along vertical direction </param>
        /// <param name="yMax"> maximum value along vertical direction </param>
        public Frame1D(string title, string labelX, string labelY, 
            double xTickDensity, double yTickDensity,
            double xMin, double xMax, double yMin, double yMax)
        {
            InitializeComponent();

            // frame text
            SetTitle(content: title, fontBold: true);
            SetLabelX(content: labelX, fontBold: false);
            SetLabelY(content: labelY, fontBold: false);
            SetLabelYRight(content: "");

            // axis setting
            SetAxisXDensity(density: xTickDensity, fontBold: false, fontSize: 12.0);
            SetAxisYDensity(density: yTickDensity, fontBold: false, fontSize: 12.0);
            SetAxisXLimits(xMin, xMax);
            SetAxisYLimits(yMin, yMax);
        }

        #endregion
        #region methods

        #region title, labels and legend

        /// <summary>
        /// sets the title of the frame
        /// </summary>
        /// <param name="content"> title of the frame </param>
        /// <param name="fontBold"> whether to set the title bold </param>
        /// <param name="fontSize"> font size of the title </param>
        /// <param name="fontFamily"> font family of the title </param>
        /// <param name="fontColor"> color of the title </param>
        public void SetTitle(string content, 
            bool? fontBold = true,
            double? fontSize = null,
            string? fontFamily = null,
            Color? fontColor = null)
            => PlotFrame.Plot.Title(content, 
                bold: fontBold, 
                size: (float?)fontSize, 
                fontName: fontFamily,
                color: fontColor);

        /// <summary>
        /// sets the label of the horizontal axis
        /// </summary>
        /// <param name="content"> label of the horizontal axis </param>
        /// <param name="fontBold"> whether to set the label bold </param>
        /// <param name="fontSize"> font size of the label </param>
        /// <param name="fontFamily"> font family of the label </param>
        /// <param name="fontColor"> color of the label </param>
        public void SetLabelX(string content,
            bool? fontBold = false,
            double? fontSize = null,
            string? fontFamily = null,
            Color? fontColor = null)
            => PlotFrame.Plot.XAxis.Label(content,
                bold: fontBold,
                size: (float?)fontSize,
                fontName: fontFamily,
                color: fontColor);

        /// <summary>
        /// sets the label of the vertical axis
        /// </summary>
        /// <param name="content"> label of the vertical axis </param>
        /// <param name="fontBold"> whether to set the label bold </param>
        /// <param name="fontSize"> font size of the label </param>
        /// <param name="fontFamily"> font family of the label </param>
        /// <param name="fontColor"> color of the label </param>
        public void SetLabelY(string content, 
            bool? fontBold = false, 
            double? fontSize = null, 
            string? fontFamily = null, 
            Color? fontColor = null) 
            => PlotFrame.Plot.YAxis.Label(content, 
                bold: fontBold, 
                size: (float?)fontSize, 
                fontName: fontFamily, 
                color: fontColor);

        /// <summary>
        /// sets the label of the vertical axis on the right
        /// </summary>
        /// <param name="content"> lable of the vertical axis on the right </param>
        /// <param name="fontBold"> whether to set the label bold </param>
        /// <param name="fontSize"> font size of the label </param>
        /// <param name="fontFamily"> font family of the label </param>
        /// <param name="fontColor"> color of the label </param>
        public void SetLabelYRight(string content,
            bool? fontBold = false,
            double? fontSize = null,
            string? fontFamily = null,
            Color? fontColor = null)
        {
            if(PlotFrame.Plot.YAxis2.IsVisible)
                PlotFrame.Plot.YAxis2.Label(content,
                bold: fontBold,
                size: (float?)fontSize,
                fontName: fontFamily,
                color: fontColor);
        }

        /// <summary>
        /// sets the legend of the frame
        /// </summary>
        /// <param name="enable"> whether to enable the legend </param>
        /// <param name="location"> if enabled, set its location </param>
        public void SetLegend(bool enable = true,
            Alignment location = Alignment.LowerRight)
            => PlotFrame.Plot.Legend(enable, location);

        #endregion
        #region axes, ticks and ranges

        /// <summary>
        /// sets the horizontal axis 
        /// by defining tick density
        /// </summary>
        /// <param name="enableGrid"> enable grid </param>
        /// <param name="enableTicks"> enable ticks </param>
        /// <param name="density"> density of the ticks </param>
        /// <param name="fontBold"> whether to set the ticks bold </param>
        /// <param name="fontSize"> font size of the ticks </param>
        /// <param name="fontFamily"> font family of the ticks </param>
        /// <param name="fontColor"> color of the ticks </param>
        public void SetAxisXDensity(bool enableGrid = true,
            bool enableTicks = true,
            double density = 1.0,
            bool? fontBold = false,
            double? fontSize = null,
            string? fontFamily = null,
            Color? fontColor = null)
        {
            PlotFrame.Plot.XAxis.Grid(enableGrid);
            PlotFrame.Plot.XAxis.Ticks(enableTicks);
            PlotFrame.Plot.XAxis.TickDensity(density);
            PlotFrame.Plot.XAxis.TickLabelStyle(color: fontColor,
                fontName: fontFamily, fontSize: (float?)fontSize, fontBold: fontBold);
        }

        /// <summary>
        /// sets the horizontal axis
        /// by defining tick spacing
        /// </summary>
        /// <param name="enableGrid"> enable grid </param>
        /// <param name="enableTicks"> enable ticks </param>
        /// <param name="spacing"> spacing of the ticks </param>
        /// <param name="fontBold"> whether to set the ticks bold </param>
        /// <param name="fontSize"> font size of the ticks </param>
        /// <param name="fontFamily"> font family of the ticks </param>
        /// <param name="fontColor"> color of the ticks </param>
        public void SetAxisXSpacing(bool enableGrid = true,
            bool enableTicks = true,
            double spacing = 1.0,
            bool? fontBold = false,
            double? fontSize = null,
            string? fontFamily = null,
            Color? fontColor = null)
        {
            PlotFrame.Plot.XAxis.Grid(enableGrid);
            PlotFrame.Plot.XAxis.Ticks(enableTicks);
            PlotFrame.Plot.XAxis.ManualTickSpacing(spacing);
            PlotFrame.Plot.XAxis.TickLabelStyle(color: fontColor,
                fontName: fontFamily, fontSize: (float?)fontSize, fontBold: fontBold);
        }

        /// <summary>
        /// sets the vertical axis on the left
        /// by defining tick density
        /// </summary>
        /// <param name="enableGrid"> enable grid </param>
        /// <param name="enableTicks"> enable ticks </param>
        /// <param name="density"> density of the ticks </param>
        /// <param name="fontBold"> whether to set the ticks bold </param>
        /// <param name="fontSize"> font size of the ticks </param>
        /// <param name="fontFamily"> font family of the ticks </param>
        /// <param name="fontColor"> color of the ticks </param>
        public void SetAxisYDensity(bool enableGrid = true,
            bool enableTicks = true,
            double density = 1.0,
            bool? fontBold = false,
            double? fontSize = null,
            string? fontFamily = null,
            Color? fontColor = null)
        {
            PlotFrame.Plot.YAxis.Grid(enableGrid);
            PlotFrame.Plot.YAxis.Ticks(enableTicks);
            PlotFrame.Plot.YAxis.TickDensity(density);
            PlotFrame.Plot.YAxis.TickLabelStyle(color: fontColor,
                fontName: fontFamily, fontSize: (float?)fontSize, fontBold: fontBold);
        }

        /// <summary>
        /// sets the vertical axis on the left
        /// by defining tick spacing
        /// </summary>
        /// <param name="enableGrid"> enable grid </param>
        /// <param name="enableTicks"> enable ticks </param>
        /// <param name="spacing"> spacing of the ticks </param>
        /// <param name="fontBold"> whether to set the ticks bold </param>
        /// <param name="fontSize"> font size of the ticks </param>
        /// <param name="fontFamily"> font family of the ticks </param>
        /// <param name="fontColor"> color of the ticks </param>
        public void SetAxisYSpacing(bool enableGrid = true,
            bool enableTicks = true,
            double spacing = 1.0,
            bool? fontBold = false,
            double? fontSize = null,
            string? fontFamily = null,
            Color? fontColor = null)
        {
            PlotFrame.Plot.YAxis.Grid(enableGrid);
            PlotFrame.Plot.YAxis.Ticks(enableTicks);
            PlotFrame.Plot.YAxis.ManualTickSpacing(spacing);
            PlotFrame.Plot.YAxis.TickLabelStyle(color: fontColor,
                fontName: fontFamily, fontSize: (float?)fontSize, fontBold: fontBold);
        }

        /// <summary>
        /// sets the vertical axis on the right
        /// by defining tick density
        /// </summary>
        /// <param name="enableGrid"> enable grid </param>
        /// <param name="enableTicks"> enable ticks </param>
        /// <param name="density"> density of the ticks </param>
        /// <param name="fontBold"> whether to set the ticks bold </param>
        /// <param name="fontSize"> font size of the ticks </param>
        /// <param name="fontFamily"> font family of the ticks </param>
        /// <param name="fontColor"> color of the ticks </param>
        public void SetAxisYRightDensity(bool enableGrid = true,
            bool enableTicks = true,
            double density = 1.0,
            bool? fontBold = false,
            double? fontSize = null,
            string? fontFamily = null,
            Color? fontColor = null)
        {
            PlotFrame.Plot.YAxis2.Grid(enableGrid);
            PlotFrame.Plot.YAxis2.Ticks(enableTicks);
            PlotFrame.Plot.YAxis2.TickDensity(density);
            PlotFrame.Plot.YAxis2.TickLabelStyle(color: fontColor,
                fontName: fontFamily, fontSize: (float?)fontSize, fontBold: fontBold);
        }

        /// <summary>
        /// sets the vertical axis on the right
        /// by defining tick spacing
        /// </summary>
        /// <param name="enableGrid"> enable grid </param>
        /// <param name="enableTicks"> enable ticks </param>
        /// <param name="spacing"> spacing of the ticks </param>
        /// <param name="fontBold"> whether to set the ticks bold </param>
        /// <param name="fontSize"> font size of the ticks </param>
        /// <param name="fontFamily"> font family of the ticks </param>
        /// <param name="fontColor"> color of the ticks </param>
        public void SetAxisYRightSpacing(bool enableGrid = true,
            bool enableTicks = true,
            double spacing = 1.0,
            bool? fontBold = false,
            double? fontSize = null,
            string? fontFamily = null,
            Color? fontColor = null)
        {
            PlotFrame.Plot.YAxis2.Grid(enableGrid);
            PlotFrame.Plot.YAxis2.Ticks(enableTicks);
            PlotFrame.Plot.YAxis2.ManualTickSpacing(spacing);
            PlotFrame.Plot.YAxis2.TickLabelStyle(color: fontColor,
                fontName: fontFamily, fontSize: (float?)fontSize, fontBold: fontBold);
        }

        /// <summary>
        /// sets the horizontal axis limits 
        /// </summary>
        /// <param name="min"> minimum value </param>
        /// <param name="max"> maximum value </param>
        public void SetAxisXLimits(double min, double max)
            => PlotFrame.Plot.SetAxisLimitsX(min, max);

        /// <summary>
        /// sets the vertical axis limits on the left
        /// </summary>
        /// <param name="min"> minimum value </param>
        /// <param name="max"> maximum value </param>
        public void SetAxisYLimits(double min, double max)
            => PlotFrame.Plot.SetAxisLimitsY(min, max);

        /// <summary>
        /// sets the vertical axis limits on the right
        /// </summary>
        /// <param name="min"> minimum value </param>
        /// <param name="max"> maximum value </param>
        public void SetAxisYRightLimits(double min, double max)
        {
            AxisLimits al00 = PlotFrame.Plot.GetAxisLimits(0, 0);
            double xMin = al00.XMin;
            double xMax = al00.XMin;
            PlotFrame.Plot.SetAxisLimits(xMin, xMax, min, max, 0, 1);
        }

        #endregion
        #region add real-valued GridData [SignalPlot]

        /// <summary>
        /// adds a real-valued grid plot into the frame
        /// </summary>
        /// <param name="grid"> sampling grid information </param>
        /// <param name="values"> data to plot </param>
        /// <param name="lineWidth"> line width </param>
        /// <param name="lineStyle"> line style </param>
        /// <param name="markerSize"> marker size </param>
        /// <param name="markerShape"> marker shape </param>
        /// <param name="plotColor"> plot color </param>
        /// <param name="visualOption"> visual option </param>
        /// <param name="label"> label of the data </param>
        /// <returns> resulting plot </returns>
        public SignalPlot AddGridPlot(GridInfo1D grid, VectorD values,
            double? lineWidth = null,
            LineStyle? lineStyle = null,
            double? markerSize = null,
            MarkerShape? markerShape = null,
            PlotColor? plotColor = null,
            VisualOption? visualOption = null,
            string? label = null)
        {
            string defaultLabel = "#" + (AllPlots.Count + 1).ToString() + ": GridData [Real]";
            SignalPlot sgPlot = PlotFrame.Plot.AddSignal(VMath.ConvertVectorToArray(values));
            DataPropExpander dataProperties = new()
            {
                IsPlotPartEnabled = false,
                PlotPart = ComplexPart.RealPart
            };

            // sampling settings for grid data
            sgPlot.SamplePeriod = grid.Spacing;
            sgPlot.OffsetX = grid.Start;

            // SignalPlot: line, marker, visual, label, ...
            sgPlot.LineWidth = lineWidth == null ? defaultLineWidth : (double)lineWidth;
            sgPlot.LineStyle = lineStyle == null ? defaultLineStyle : (LineStyle)lineStyle;
            sgPlot.LineColor = plotColor == null ? Color.FromName(defaultPlotColor.ToString()) : Color.FromName(plotColor.ToString());
            sgPlot.MarkerSize = markerSize == null ? (float)defaultMarkerSize : (float)markerSize;
            sgPlot.MarkerShape = markerShape == null ? defaultMarkerStyle : (MarkerShape)markerShape;
            sgPlot.MarkerColor = plotColor == null ? Color.FromName(defaultPlotColor.ToString()) : Color.FromName(plotColor.ToString());
            sgPlot.IsVisible = visualOption switch
            {
                null => true,
                VisualOption.Visible => true,
                VisualOption.Hidden => false,
                _ => true
            };
            sgPlot.Label = label ?? defaultLabel;

            // Expander: line settings
            dataProperties.LineWidth = lineWidth == null ? defaultLineWidth : (double)lineWidth;
            dataProperties.LineStyle = lineStyle == null ? defaultLineStyle : (LineStyle)lineStyle;
            dataProperties.MarkerSize = markerSize == null ? defaultMarkerSize : (double)markerSize;
            dataProperties.MarkerShape = markerShape == null ? defaultMarkerStyle : (MarkerShape)markerShape;
            dataProperties.PlotColor = plotColor == null ? defaultPlotColor : (PlotColor)plotColor;
            dataProperties.VisualOption = visualOption == null ? VisualOption.Visible : (VisualOption)visualOption;
            dataProperties.DataLabel = label ?? defaultLabel;
            //dataProperties.TestColor = plotColor;

            // add to lists & generate DaraPropExpander
            AllData.Add(null);
            AllPlots.Add(sgPlot);
            DataSourcePanel.Children.Add(dataProperties);

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
            double? lineWidth = null,
            LineStyle? lineStyle = null,
            double? markerSize = null,
            MarkerShape? markerShape = null,
            PlotColor? plotColor = null,
            VisualOption? visualOption = null,
            string? label = null,
            string? header = null)
            => AddGridPlot(gv.GridInfo, gv.Values,
                    lineWidth, lineStyle,
                    markerSize, markerShape, plotColor,
                    visualOption, label);

        #endregion
        #region add complex-valued GridData [SignalPlot]

        /// <summary>
        /// adds a complex-valued grid plot into the frame
        /// </summary>
        /// <param name="grid"> sampling grid information </param>
        /// <param name="values"> data to plot </param>
        /// <param name="plotPart"> part of the complex data to plot </param>
        /// <param name="lineWidth"> line width </param>
        /// <param name="lineStyle"> line style </param>
        /// <param name="markerSize"> marker size </param>
        /// <param name="markerShape"> marker shape </param>
        /// <param name="plotColor"> plot color </param>
        /// <param name="visualOption"> visual option </param>
        /// <param name="label"> label of the data </param>
        /// <returns> resulting plot </returns>
        public SignalPlot AddGridPlot(GridInfo1D grid, VectorZ values,
            ComplexPart? plotPart = null,
            double? lineWidth = null,
            LineStyle? lineStyle = null,
            double? markerSize = null,
            MarkerShape? markerShape = null,
            PlotColor? plotColor = null,
            VisualOption? visualOption = null,
            string? label = null)
        {
            VectorD valuePart = plotPart switch
            {
                null => VMath.Abs(values),
                ComplexPart.RealPart => VMath.RealPart(values),
                ComplexPart.ImagPart => VMath.ImagPart(values),
                ComplexPart.Magnitude => VMath.Abs(values),
                ComplexPart.Argument => VMath.Arg(values),
                _ => VMath.Abs(values)
            };

            string defaultLabel = "#" + (AllPlots.Count + 1).ToString() + ": GridData [Complex]";
            SignalPlot sgPlot = PlotFrame.Plot.AddSignal(VMath.ConvertVectorToArray(valuePart));
            DataPropExpander dataProperties = new()
            {
                IsPlotPartEnabled = true,
                PlotPart = plotPart ?? ComplexPart.Magnitude
            };

            // sampling settings for grid data
            sgPlot.SamplePeriod = grid.Spacing;
            sgPlot.OffsetX = grid.Start;

            // SignalPlot: line, marker, visual, label, ...
            sgPlot.LineWidth = lineWidth == null ? defaultLineWidth : (double)lineWidth;
            sgPlot.LineStyle = lineStyle == null ? defaultLineStyle : (LineStyle)lineStyle;
            sgPlot.LineColor = plotColor == null ? Color.FromName(defaultPlotColor.ToString()) : Color.FromName(plotColor.ToString());
            sgPlot.MarkerSize = markerSize == null ? (float)defaultMarkerSize : (float)markerSize;
            sgPlot.MarkerShape = markerShape == null ? defaultMarkerStyle : (MarkerShape)markerShape;
            sgPlot.MarkerColor = plotColor == null ? Color.FromName(defaultPlotColor.ToString()) : Color.FromName(plotColor.ToString());
            sgPlot.IsVisible = visualOption switch
            {
                null => true,
                VisualOption.Visible => true,
                VisualOption.Hidden => false,
                _ => true
            };
            sgPlot.Label = label ?? defaultLabel;

            // Expander: line settings
            dataProperties.LineWidth = lineWidth == null ? defaultLineWidth : (double)lineWidth;
            dataProperties.LineStyle = lineStyle == null ? defaultLineStyle : (LineStyle)lineStyle;
            dataProperties.MarkerSize = markerSize == null ? defaultMarkerSize : (double)markerSize;
            dataProperties.MarkerShape = markerShape == null ? defaultMarkerStyle : (MarkerShape)markerShape;
            dataProperties.PlotColor = plotColor == null ? defaultPlotColor : (PlotColor)plotColor;
            dataProperties.VisualOption = visualOption == null ? VisualOption.Visible : (VisualOption)visualOption;
            dataProperties.DataLabel = label ?? defaultLabel;

            // add to list & generate DaraPropExpander
            AllData.Add(values);
            AllPlots.Add(sgPlot);
            DataSourcePanel.Children.Add(dataProperties);

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
            ComplexPart? plotPart = null,
            double? lineWidth = null,
            LineStyle? lineStyle = null,
            double? markerSize = null,
            MarkerShape? markerShape = null,
            PlotColor? plotColor = null,
            VisualOption? visualOption = null,
            string? label = null)
            => AddGridPlot(gv.GridInfo, gv.Values, plotPart,
                lineWidth, lineStyle,
                markerSize, markerShape, plotColor,
                visualOption, label);

        #endregion
        #region add real-valued ScatData [ScatterPlot]

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
            double? lineWidth = null,
            LineStyle? lineStyle = null,
            double? markerSize = null,
            MarkerShape? markerShape = null,
            PlotColor? plotColor = null,
            VisualOption? visualOption = null,
            string? label = null)
        {
            string defaultLabel = "#" + (AllPlots.Count + 1).ToString() + ": ScatData [Real]";
            ScatterPlot scPlot = PlotFrame.Plot.AddScatter(VMath.ConvertVectorToArray(locations), 
                VMath.ConvertVectorToArray(values));
            DataPropExpander dpExpand = new()
            {
                IsPlotPartEnabled = false,
                PlotPart = ComplexPart.RealPart
            };

            // ScatterPlot: line, marker, visual, label, ...
            scPlot.LineWidth = lineWidth == null ? defaultLineWidth : (double)lineWidth;
            scPlot.LineStyle = lineStyle == null ? defaultLineStyle : (LineStyle)lineStyle;
            scPlot.LineColor = plotColor == null ? Color.FromName(defaultPlotColor.ToString()) : Color.FromName(plotColor.ToString());
            scPlot.MarkerSize = markerSize == null ? (float)defaultMarkerSize : (float)markerSize;
            scPlot.MarkerShape = markerShape == null ? defaultMarkerStyle : (MarkerShape)markerShape;
            scPlot.MarkerColor = plotColor == null ? Color.FromName(defaultPlotColor.ToString()) : Color.FromName(plotColor.ToString());
            scPlot.IsVisible = visualOption switch
            {
                null => true,
                VisualOption.Visible => true,
                VisualOption.Hidden => false,
                _ => true
            };
            scPlot.Label = label ?? defaultLabel;

            // Expander: line & marker settings
            dpExpand.LineWidth = lineWidth == null ? defaultLineWidth : (double)lineWidth;
            dpExpand.LineStyle = lineStyle == null ? defaultLineStyle : (LineStyle)lineStyle;
            dpExpand.MarkerSize = markerSize == null ? defaultMarkerSize : (double)markerSize;
            dpExpand.MarkerShape = markerShape == null ? defaultMarkerStyle : (MarkerShape)markerShape;
            dpExpand.PlotColor = plotColor == null ? defaultPlotColor : (PlotColor)plotColor;
            dpExpand.VisualOption = visualOption == null ? VisualOption.Visible : (VisualOption)visualOption;
            dpExpand.DataLabel = label ?? defaultLabel;

            // add to list & generate DaraPropExpander
            AllData.Add(null);
            AllPlots.Add(scPlot);
            DataSourcePanel.Children.Add(dpExpand);

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
            double? lineWidth = null,
            LineStyle? lineStyle = null,
            double? markerSize = null,
            MarkerShape? markerShape = null,
            PlotColor? plotColor = null,
            VisualOption? visualOption = null,
            string? label = null)
            => AddScatPlot(sv.Points, sv.Values,
                lineWidth, lineStyle, 
                markerSize, markerShape, plotColor,
                visualOption, label);

        #endregion
        #region add complex-valued ScatData [ScatterPlot]

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
            ComplexPart? plotPart = null,
            double? lineWidth = null,
            LineStyle? lineStyle = null,
            double? markerSize = null,
            MarkerShape? markerShape = null,
            PlotColor? plotColor = null,
            VisualOption? visualOption = null,
            string? label = null)
        {
            VectorD valuePart = plotPart switch
            {
                null => VMath.Abs(values),
                ComplexPart.RealPart => VMath.RealPart(values),
                ComplexPart.ImagPart => VMath.ImagPart(values),
                ComplexPart.Magnitude => VMath.Abs(values),
                ComplexPart.Argument => VMath.Arg(values),
                _ => VMath.Abs(values)
            };

            string defaultLabel = "#" + (AllPlots.Count + 1).ToString() + ": ScatData [Complex]";
            ScatterPlot scPlot = PlotFrame.Plot.AddScatter(VMath.ConvertVectorToArray(locations),
                VMath.ConvertVectorToArray(valuePart));
            DataPropExpander dpExpand = new()
            {
                IsPlotPartEnabled = false,
                PlotPart = ComplexPart.RealPart
            };

            // ScatterPlot: line, marker, visual, label, ...
            scPlot.LineWidth = lineWidth == null ? defaultLineWidth : (double)lineWidth;
            scPlot.LineStyle = lineStyle == null ? defaultLineStyle : (LineStyle)lineStyle;
            scPlot.LineColor = plotColor == null ? Color.FromName(defaultPlotColor.ToString()) : Color.FromName(plotColor.ToString());
            scPlot.MarkerSize = markerSize == null ? (float)defaultMarkerSize : (float)markerSize;
            scPlot.MarkerShape = markerShape == null ? defaultMarkerStyle : (MarkerShape)markerShape;
            scPlot.MarkerColor = plotColor == null ? Color.FromName(defaultPlotColor.ToString()) : Color.FromName(plotColor.ToString());
            scPlot.IsVisible = visualOption switch
            {
                null => true,
                VisualOption.Visible => true,
                VisualOption.Hidden => false,
                _ => true
            };
            scPlot.Label = label ?? defaultLabel;

            // Expander: line & marker settings
            dpExpand.LineWidth = lineWidth == null ? defaultLineWidth : (double)lineWidth;
            dpExpand.LineStyle = lineStyle == null ? defaultLineStyle : (LineStyle)lineStyle;
            dpExpand.MarkerSize = markerSize == null ? defaultMarkerSize : (double)markerSize;
            dpExpand.MarkerShape = markerShape == null ? defaultMarkerStyle : (MarkerShape)markerShape;
            dpExpand.PlotColor = plotColor == null ? defaultPlotColor : (PlotColor)plotColor;
            dpExpand.VisualOption = visualOption == null ? VisualOption.Visible : (VisualOption)visualOption;
            dpExpand.DataLabel = label ?? defaultLabel;

            // add to list & generate DaraPropExpander
            AllData.Add(values);
            AllPlots.Add(scPlot);
            DataSourcePanel.Children.Add(dpExpand);

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
            ComplexPart? plotPart = null,
            double? lineWidth = null,
            LineStyle? lineStyle = null,
            double? markerSize = null,
            MarkerShape? markerShape = null,
            PlotColor? plotColor = null,
            VisualOption? visualOption = null,
            string? label = null)
            => AddScatPlot(sv.Points, sv.Values, plotPart,
                lineWidth, lineStyle,
                markerSize, markerShape, plotColor,
                visualOption, label);

        #endregion
        #region visualize & update

        /// <summary>
        /// refreshes the plots and 
        /// displays the frame
        /// </summary>
        public void Visualize()
        {
            PlotFrame.Refresh();
            Show();
        }

        /// <summary>
        /// updates all the plots in the frame
        /// </summary>
        public void UpdatePlots()
        {
            // data sources
            for (int i = 0; i < AllPlots.Count; i++)
            {
                DataPropExpander dataProperties = (DataPropExpander)DataSourcePanel.Children[i];
                // check plot type
                if (AllPlots[i].GetType() == typeof(SignalPlot))
                {
                    // general update for both real and complex cases
                    SignalPlot plot = (SignalPlot)AllPlots[i];
                    plot.LineWidth = dataProperties.LineWidth;
                    plot.LineStyle = dataProperties.LineStyle;
                    plot.LineColor = Color.FromName(dataProperties.PlotColor.ToString());
                    plot.MarkerSize = (float)dataProperties.MarkerSize;
                    plot.MarkerShape = dataProperties.MarkerShape;
                    plot.MarkerColor = Color.FromName(dataProperties.PlotColor.ToString());
                    plot.IsVisible = dataProperties.VisualOption switch
                    {
                        VisualOption.Visible => true,
                        VisualOption.Hidden => false,
                        _ => true
                    };
                    plot.Label = dataProperties.DataLabel;

                    // handling complex cases
                    if (AllData[i] != null && AllData[i].GetType() == typeof(VectorZ))
                    {
                        VectorZ values = (VectorZ)AllData[i];
                        VectorD valuesPart = dataProperties.PlotPart switch
                        {
                            ComplexPart.RealPart => VMath.RealPart(values),
                            ComplexPart.ImagPart => VMath.ImagPart(values),
                            ComplexPart.Magnitude => VMath.Abs(values),
                            ComplexPart.Argument => VMath.Arg(values)
                        };
                        plot.Update(VMath.ConvertVectorToArray(valuesPart));
                    }

                }
                else if (AllPlots[i].GetType() == typeof(ScatterPlot))
                {
                    // general update for both real and complex cases
                    ScatterPlot plot = (ScatterPlot)AllPlots[i];
                    plot.LineWidth = dataProperties.LineWidth;
                    plot.LineStyle = dataProperties.LineStyle;
                    plot.LineColor = Color.FromName(dataProperties.PlotColor.ToString());
                    plot.MarkerSize = (float)dataProperties.MarkerSize;
                    plot.MarkerShape = dataProperties.MarkerShape;
                    plot.MarkerColor = Color.FromName(dataProperties.PlotColor.ToString());
                    plot.IsVisible = dataProperties.VisualOption switch
                    {
                        VisualOption.Visible => true,
                        VisualOption.Hidden => false,
                        _ => true
                    };
                    plot.Label = dataProperties.DataLabel;

                    // handling complex cases
                    if (AllData[i] != null && AllData[i].GetType() == typeof(VectorZ))
                    {
                        VectorZ values = (VectorZ)AllData[i];
                        VectorD valuesPart = dataProperties.PlotPart switch
                        {
                            ComplexPart.RealPart => VMath.RealPart(values),
                            ComplexPart.ImagPart => VMath.ImagPart(values),
                            ComplexPart.Magnitude => VMath.Abs(values),
                            ComplexPart.Argument => VMath.Arg(values)
                        };
                        plot.UpdateY(VMath.ConvertVectorToArray(valuesPart));
                    }
                }

            }

            // view ranges
            SetAxisXLimits(XRangeMin.Value, XRangeMax.Value);
            SetAxisYLimits(YRangeMin.Value, YRangeMax.Value);

            // axis ticks
            SetAxisXDensity(density: XTick.Value, fontSize: XTick.Unit);
            SetAxisYDensity(density: YTick.Value, fontSize: YTick.Unit);
            
            // label texts
            SetTitle(content: FrameTitle.Value, fontBold: true, fontSize: FrameTitle.Unit);
            SetLabelX(content: XAxisLabel.Value, fontBold: false, fontSize: XAxisLabel.Unit);
            SetLabelY(content: YAxisLabel.Value, fontBold: false, fontSize: YAxisLabel.Unit);

            // refresh
            PlotFrame.Refresh();
        }

        #endregion
        #region cursor and axis limit info

        /// <summary>
        /// finds the Cursor coordinates in the figure frame
        /// </summary>
        /// <param name="sender"> sender </param>
        /// <param name="e"> event </param>
        private void GetCursorInfo(object sender, MouseEventArgs e)
        {
            (double x, double y) = PlotFrame.GetMouseCoordinates();
            (float px, float py) = PlotFrame.GetMousePixel();

            PhysicalX.Value = double.Parse(Converter.NumberToString(x));
            PhysicalY.Value = double.Parse(Converter.NumberToString(y));

            PixelX.Value = double.Parse(Converter.NumberToString(px));
            PixelY.Value = double.Parse(Converter.NumberToString(py));
                    
            //HighLightPoint.X = x;
            //HighLightPoint.Y = y;
            //Frame.Refresh();
        }

        /// <summary>
        /// finds the axis limits after it is changed
        /// </summary>
        /// <param name="sender"> sender </param>
        /// <param name="e"> event </param>
        private void GetAxisLimits(object? sender, EventArgs e)
        {
            AxisLimits al = PlotFrame.Plot.GetAxisLimits();
            XRangeMin.Value = double.Parse(Converter.NumberToString(al.XMin));
            XRangeMax.Value = double.Parse(Converter.NumberToString(al.XMax));
            YRangeMin.Value = double.Parse(Converter.NumberToString(al.YMin));
            YRangeMax.Value = double.Parse(Converter.NumberToString(al.YMax));

            //MessageBox.Show("Frame axis changed");
        }

        #endregion
        #region advanced setting column control

        private void ShowAdvColumn()
        {
            AdvColumn.Width = new GridLength(advWidth);
            if (WindowState == WindowState.Normal && Width != fullWidth)
                Width += advWidth;
        }

        private void HideAdvColumn()
        {
            AdvColumn.Width = new GridLength(0);
            if (WindowState == WindowState.Normal)
                Width -= advWidth;
        }

        #endregion

        #endregion

        #region buttons

        private void AdvCheckBox_Checked(object sender, RoutedEventArgs e)
            => ShowAdvColumn();

        private void AdvCheckBox_Unchecked(object sender, RoutedEventArgs e)
            => HideAdvColumn();

        private void PlotButton(object sender, RoutedEventArgs e)
            => UpdatePlots();


        private void ShowDataLabel_Click(object sender, RoutedEventArgs e)
        {
            DataPropExpander expand0 = (DataPropExpander)DataSourcePanel.Children[0];
            //MessageBox.Show(expand0.DataLabel);

            DataPropExpander expand1 = (DataPropExpander)DataSourcePanel.Children[1];
            //MessageBox.Show(expand1.DataLabel);

            // check plot type
            string t0 = AllPlots[0].GetType().ToString();
            MessageBox.Show("t0 = " + t0);


            //dpExp.HeaderTextBlock.Text = "Test";
        }

        #endregion

    }


    
}
