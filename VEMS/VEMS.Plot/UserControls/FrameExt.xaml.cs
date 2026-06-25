using ScottPlot;
using ScottPlot.Drawing;
using ScottPlot.Plottable;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VEMS.MathCore;
using static VEMS.MathCore.FFTOptions;
using static VEMS.Plot.Options;
using Complex = System.Numerics.Complex;
using Image = ScottPlot.Plottable.Image;
using LineStyle = VEMS.Plot.Options.LineStyle;
using MarkerShape = VEMS.Plot.Options.MarkerShape;
using PlotColor = VEMS.Plot.Options.PlotColor;
using VisualOption = VEMS.Plot.Options.VisualOption;

namespace VEMS.Plot
{
    /// <summary>
    /// FrameExt
    /// </summary>
    public partial class FrameExt : UserControl
    {
        #region defaults

        //// view ranges
        //private double defaultXMin => FrameLite.defaultXMin; // -2.0;
        //private double defaultXMax => FrameLite.defaultXMax; // 2.0;
        //private double defaultYMin = -0.5;
        //private double defaultYMax = 3.5;

        //// axis ticks
        //private double defaultXTickDensity = 1.0;
        //private double defaultYTickDensity = 1.0;
        //private double defaultXTickSize = 12.0;
        //private double defaultYTickSize = 12.0;

        //// figure texts
        //string defaultTitle = "VEMS Plot(s)";
        //string defaultLabelX = "X";
        //string defaultLabelY = "Y";
        //double defaultTitleSize = 20.0;
        //double defaultLabelXSize = 18.0;
        //double defaultLabelYSize = 18.0;

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

        /// <summary>
        /// minimum view range along the vertical direction on the right
        /// </summary>
        public NumberQuantity YRightRangeMin = new()
        {
            Name = "Vertical (Right) Minimum",
            Value = 0.5,
            Unit = 1.0
        };

        /// <summary>
        /// maximum view range along the vertical direction on the right
        /// </summary>
        public NumberQuantity YRightRangeMax = new()
        {
            Name = "Vertical (Right) Maximum",
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
        /// tick density along the vertical direction
        /// </summary>
        public NumberQuantity YTick = new()
        {
            Name = "Vertical Ticks",
            Value = 1.0,
            Unit = 12.0 // font size
        };

        /// <summary>
        /// tick density along the vertial direction
        /// on the right side
        /// </summary>
        public NumberQuantity YRightTick = new()
        {
            Name = "Vertical (Right) Ticks",
            Value = 1.0,
            Unit = 12.0 // font size
        };

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

        /// <summary>
        /// label of the vertical axis
        /// on the right side
        /// </summary>
        public TextQuantity YRightAxisLabel = new()
        {
            Name = "Vertical Label (Right)",
            Value = "Y (Right)",
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
        /// physical location Y-Right of the cursor
        /// </summary>
        public NumberQuantity PhysicalYRight = new()
        {
            Name = "Vertical Location (Y-Right)",
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
        #region fields - axis span
        /// <summary>
        /// physical location X of the cursor 
        /// </summary>
        public NumberQuantity XSpanMin = new()
        {
            Name = "Horizontal Span Start",
            Value = 0.0,
            Unit = 1.0
        };
        /// <summary>
        /// physical location X of the cursor 
        /// </summary>
        public NumberQuantity XSpanMax = new()
        {
            Name = "Horizontal Span End",
            Value = 0.0,
            Unit = 1.0
        };
        /// <summary>
        /// physical location X of the cursor 
        /// </summary>
        public NumberQuantity YSpanMin = new()
        {
            Name = "Vertical Span Start",
            Value = 0.0,
            Unit = 1.0
        };
        /// <summary>
        /// physical location X of the cursor 
        /// </summary>
        public NumberQuantity YSpanMax = new()
        {
            Name = "Vertical Span End",
            Value = 0.0,
            Unit = 1.0
        };
        #endregion

        #region properties        

        /// <summary>
        /// list of all the data for the plots
        /// </summary>
        public List<Object> AllData => FrameLite.AllData; //{ get; set; }

        /// <summary>
        /// list of all the plots in the frame
        /// </summary>
        public List<Object> AllPlots => FrameLite.AllPlots; //{ get; set; }

        /// <summary>
        /// list of all the colorbars in the frame
        /// </summary>
        public List<Colorbar> AllColorbars => FrameLite.AllColorbars;

        /// <summary>
        /// list of all the parts of the data
        /// </summary>
        public List<ComplexPart> AllParts => FrameLite.AllParts;

        /// <summary>
        /// list of view ranges
        /// </summary>
        public List<NumberQuantity>? ViewRange { get; set; }

        /// <summary>
        /// list of axis tickes
        /// </summary>
        public List<NumberQuantity>? AxisTick { get; set; }

        /// <summary>
        /// list of title and axis labels
        /// </summary>
        public List<TextQuantity>? TitleLabel { get; set; }

        /// <summary>
        /// list of cursor location information
        /// </summary>
        public List<NumberQuantity>? CursorInfo { get; set; }

        /// <summary>
        /// list of legend position options
        /// </summary>
        public List<LegendLocation>? LegendOption { get; set; }

        /// <summary>
        /// list of x_span position options
        /// </summary>
        public List<NumberQuantity>? XAxisSpan { get; set; }

        /// <summary>
        /// list of y_span position options
        /// </summary>
        public List<NumberQuantity>? YAxisSpan { get; set; }

        /// <summary>
        /// flag whether user specified x-axis limits
        /// </summary>
        public bool UserSpecifiedXAxisLimits { get; set; } = false;

        /// <summary>
        /// flag whether user specified y-axis limits
        /// </summary>
        public bool UserSpecifiedYAxisLimits { get; set; } = false;

        /// <summary>
        /// flag whether the left y-axis is enabled
        /// </summary>
        public bool EnableYLeftAxis { get; set; } = true;

        /// <summary>
        /// flag whether the right y-axis is enabled
        /// </summary>
        public bool EnableYRightAxis { get; set; } = false;

        #endregion
        #region constructors

        /// <summary>
        /// constructs a Frame1D class
        /// with default properties
        /// </summary>
        public FrameExt()
        {
            InitializeComponent();

            // view range set by default limits => not user specified !!!
            UserSpecifiedXAxisLimits = false;
            UserSpecifiedYAxisLimits = false;

            // set ItemsSource for the data grids
            switch(EnableYLeftAxis, EnableYRightAxis)
            {
                case (true, false): // left y-axis only
                    {
                        ViewRange = new List<NumberQuantity> { XRangeMin, XRangeMax, YRangeMin, YRangeMax };
                        AxisTick = new List<NumberQuantity> { XTick, YTick };
                        TitleLabel = new List<TextQuantity> { FrameTitle, XAxisLabel, YAxisLabel };
                        CursorInfo = new List<NumberQuantity> { PhysicalX, PhysicalY, PixelX, PixelY };
                        XAxisSpan = new List<NumberQuantity> { XSpanMin,XSpanMax };
                        YAxisSpan = new List<NumberQuantity> { YSpanMin,YSpanMax };
                        break;
                    }
                case (false, true): // right y-axis only
                    {
                        ViewRange = new List<NumberQuantity> { XRangeMin, XRangeMax, YRightRangeMin, YRightRangeMax };
                        AxisTick = new List<NumberQuantity> { XTick, YRightTick };
                        TitleLabel = new List<TextQuantity> { FrameTitle, XAxisLabel, YRightAxisLabel };
                        CursorInfo = new List<NumberQuantity> { PhysicalX, PhysicalYRight, PixelX, PixelY };
                        break;
                    }
                case (true, true): // both y-axes
                    {
                        ViewRange = new List<NumberQuantity> { XRangeMin, XRangeMax, YRangeMin, YRangeMax, YRightRangeMin, YRightRangeMax };
                        AxisTick = new List<NumberQuantity> { XTick, YTick, YRightTick };
                        TitleLabel = new List<TextQuantity> { FrameTitle, XAxisLabel, YAxisLabel, YRightAxisLabel };
                        CursorInfo = new List<NumberQuantity> { PhysicalX, PhysicalY, PhysicalYRight, PixelX, PixelY };
                        XAxisSpan = new List<NumberQuantity> { XSpanMin, XSpanMax };
                        YAxisSpan = new List<NumberQuantity> { YSpanMin, YSpanMax };
                        break;
                    }
                default: 
                    break;
            }
            RangeDataGrid.ItemsSource = ViewRange;
            TickDataGrid.ItemsSource = AxisTick;
            TextDataGrid.ItemsSource = TitleLabel;
            CursorDataGrid.ItemsSource = CursorInfo;
            XSpanDataGrid.ItemsSource = XAxisSpan;
            YSpanDataGrid.ItemsSource = YAxisSpan;
            // legend option
            ComboBox_LegendLocation.ItemsSource = Enum.GetValues(typeof(LegendLocation));
            ComboBox_LegendLocation.SelectedItem = LegendLocation.LowerRight;
            SetLegend(enable: (bool)CheckBox_ShowLegend.IsChecked,
                    location: LegendLocation.LowerRight,
                    fontSize: FrameDefaults.LegendSize);

            // spans
            PlotColor[] allPlotColors = (PlotColor[])Enum.GetValues(typeof(PlotColor));
            double[] allOpacities = new double[]{ 0.1, 0.2, 0.3, 0.4, 0.5, 0.6, 0.7, 0.8, 0.9 };
            LineStyle[] allLineStyles = (LineStyle[])Enum.GetValues(typeof(LineStyle));
            LineStyle[] limitedLineStyles = allLineStyles.Where(val => val != LineStyle.None).ToArray();
            X_Span_FillColor.ItemsSource = allPlotColors;
            X_Span_FillColor.SelectedItem = FrameDefaults.HSpanColor;
            Y_Span_FillColor.ItemsSource = allPlotColors;
            Y_Span_FillColor.SelectedItem = FrameDefaults.VSpanColor;
            X_Span_Opacity.ItemsSource = allOpacities;
            X_Span_Opacity.SelectedIndex = 1;
            Y_Span_Opacity.ItemsSource = allOpacities;
            Y_Span_Opacity.SelectedIndex = 1;
            X_Span_LineStyle.ItemsSource = limitedLineStyles;
            X_Span_LineStyle.SelectedItem = FrameDefaults.LineType;
            Y_Span_LineStyle.ItemsSource = limitedLineStyles;
            Y_Span_LineStyle.SelectedItem = FrameDefaults.LineType;

            // crosshair
            Crosshair_Color.ItemsSource = allPlotColors;
            Crosshair_Color.SelectedItem = FrameDefaults.CrosshairColor;
            Crosshair_LineStyle.ItemsSource = limitedLineStyles;
            Crosshair_LineStyle.SelectedItem = FrameDefaults.CrosshairLineType;

            // plot frame settings
            FrameLite.PlotFrame.RightClicked -= FrameLite.PlotFrame.DefaultRightClickEvent;
            FrameLite.PlotFrame.MouseMove += GetCursorInfo;
            FrameLite.PlotFrame.AxesChanged += GetAxisLimits;
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
        public FrameExt(string? title = null, double? titleSize = null,
            string? labelX = null, double? labelXSize = null,
            string? labelY = null, double? labelYSize = null,
            double? xTickDensity = null, double? xTickSize = null,
            double? yTickDensity = null, double? yTickSize = null,
            double? xMin = null, double? xMax = null,
            double? yMin = null, double? yMax = null)
        {
            InitializeComponent();

            // set the view range by default
            SetXAxisLimits(min: xMin ?? FrameLite.defaultXMin, max: xMax ?? FrameLite.defaultXMax);
            SetYAxisLimits(min: yMin ?? FrameLite.defaultYMin, max: yMax ?? FrameLite.defaultYMax);
            UserSpecifiedXAxisLimits = (xMin != null || xMax != null) ? true : false;
            UserSpecifiedYAxisLimits = (yMin != null || yMax != null) ? true : false;

            // axis ticks
            SetXAxisTicks(tickDensity: xTickDensity ?? FrameLite.defaultXTickDensity, fontSize: xTickSize ?? FrameLite.defaultXTickSize);
            SetYAxisTicks(tickDensity: yTickDensity ?? FrameLite.defaultYTickDensity, fontSize: yTickSize ?? FrameLite.defaultYTickSize);

            // frame text by default
            SetTitle(content: title ?? FrameLite.defaultTitle, fontSize: titleSize ?? FrameLite.defaultTitleSize);
            SetLabelX(content: labelX ?? FrameLite.defaultLabelX, fontSize: labelXSize ?? FrameLite.defaultLabelXSize);
            SetLabelY(content: labelY ?? FrameLite.defaultLabelY, fontSize: labelYSize ?? FrameLite.defaultLabelYSize);

            // plot frame settings
            FrameLite.PlotFrame.RightClicked -= FrameLite.PlotFrame.DefaultRightClickEvent;
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
        {
            // => FrameLite method
            FrameLite.SetXAxisTicks(enableGrid, enableTicks, tickDensity,
                fontBold, fontSize, fontFamily, fontColor);
            // update axis tick properties
            XTick.Value = tickDensity;
            XTick.Unit = fontSize;
        }

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
        {
            // => FrameLite method
            FrameLite.SetYAxisTicks(enableGrid, enableTicks, tickDensity,
                fontBold, fontSize, fontFamily, fontColor);
            // update axis tick properties
            YTick.Value = tickDensity;
            YTick.Unit = fontSize;
        }

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
        {
            // => FrameLite method
            FrameLite.SetYRightAxisTicks(enableGrid, enableTicks, tickDensity,
                fontBold, fontSize, fontFamily, fontColor);
            // update axis tick properties
            YRightTick.Value = tickDensity;
            YRightTick.Unit = fontSize;
        }

        #endregion
        #region ===== range =====

        /// <summary>
        /// sets the horizontal axis limits 
        /// </summary>
        /// <param name="min"> minimum value </param>
        /// <param name="max"> maximum value </param>
        public void SetXAxisLimits(double min, double max)
        {
            // => FrameLite method
            if (!XRangeMin.LessThan(XRangeMax)) { return; }
            FrameLite.SetXAxisLimits(min, max);
            UserSpecifiedXAxisLimits = true;
            // update axis limits properties
            XRangeMin.Value = min;
            XRangeMax.Value = max;
        }

        /// <summary>
        /// sets the vertical axis limits on the left
        /// </summary>
        /// <param name="min"> minimum value </param>
        /// <param name="max"> maximum value </param>
        public void SetYAxisLimits(double min, double max)
        {
            // => FrameLite method
            if (!YRangeMin.LessThan(YRangeMax)) { return; }
            FrameLite.SetYAxisLimits(min, max);
            UserSpecifiedYAxisLimits = true;
            // update axis limit properties
            YRangeMin.Value = min;
            YRangeMax.Value = max;
        }

        /// <summary>
        /// sets the vertical axis limits on the right
        /// </summary>
        /// <param name="min"> minimum value </param>
        /// <param name="max"> maximum value </param>
        public void SetYRightAxisLimits(double min, double max)
        {
            // => FrameLite method
            if (!YRightRangeMin.LessThan(YRightRangeMax)) { return; }
            FrameLite.SetYRightAxisLimits(min, max);
            UserSpecifiedYAxisLimits = true;
            // update axis limit properties
            
            YRangeMin.Value = min;
            YRangeMax.Value = max;

        }

        /// <summary>
        /// detects the axis limits automatically
        /// </summary>
        public void DetectAxisLimits()
        {
            // => FrameLite method
            FrameLite.DetectAxisLimits(out double xMin, out double xMax, 
                out double yMin, out double yMax, 
                out double yRightMin, out double yRightMax);
            // update axis limits properties and controls number of digits
            XRangeMin.Value = xMin;// double.Parse(Converter.NumberToString(xMin));
            XRangeMax.Value = xMax;// double.Parse(Converter.NumberToString(xMax));
            YRangeMin.Value = yMin;// double.Parse(Converter.NumberToString(yMin));
            YRangeMax.Value = yMax;// double.Parse(Converter.NumberToString(yMax));
            YRightRangeMin.Value = yRightMin;// double.Parse(Converter.NumberToString(yRightMin));
            YRightRangeMax.Value = yRightMax;// double.Parse(Converter.NumberToString(yRightMax));
        }

        /// <summary>
        /// detects the x-axis limits automatically
        /// </summary>
        public void DetectXAxisLimits()
        {
            // => FrameLite method
            (double xMin, double xMax) = FrameLite.DetectXAxisLimits();
            // update x-axis limits properties and controls number of digits
            XRangeMin.Value = xMin;// double.Parse(Converter.NumberToString(xMin));
            XRangeMax.Value = xMax;// double.Parse(Converter.NumberToString(xMax));
        }

        /// <summary>
        /// detects the y-axis limits automatically
        /// </summary>
        public void DetectYAxisLimits()
        {
            if(EnableYLeftAxis)
            {
                // => FrameLite method
                (double yMin, double yMax) = FrameLite.DetectYAxisLimits();
                // update x-axis limits properties and controls number of digits
                YRangeMin.Value = yMin;// double.Parse(Converter.NumberToString(yMin));
                YRangeMax.Value = yMax;// double.Parse(Converter.NumberToString(yMax));
            }
            if(EnableYRightAxis)
            {
                // => FrameLite method
                (double yRightMin, double yRightMax) = FrameLite.DetectYRightAxisLimits();
                // update x-axis limits properties and controls number of digits
                YRightRangeMin.Value = yRightMin;// double.Parse(Converter.NumberToString(yRightMin));
                YRightRangeMax.Value = yRightMax;// double.Parse(Converter.NumberToString(yRightMax));
            }
        }

        #endregion
        #region ===== scale =====

        /// <summary>
        /// locks or unlocks the axis scale
        /// </summary>
        /// <param name="lockScale"> true: locks scale; false: unlocks </param>
        public void LockAxisScale(bool lockScale = true)
            => FrameLite.LockAxisScale(lockScale);

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
            double fontSize = FrameDefaults.TitleSize,
            string fontFamily = FrameDefaults.FontFamily,
            PlotColor fontColor = FrameDefaults.Color)
        {
            // => FrameLite method
            FrameLite.SetTitle(content, fontBold, fontSize, fontFamily, fontColor);
            
            // update title properties
            FrameTitle.Value = content;
            FrameTitle.Unit = fontSize;
        }

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
        public void SetLabelX(string content,
            bool fontBold = false,
            double fontSize = FrameDefaults.AxisLabelSize,
            string fontFamily = FrameDefaults.FontFamily,
            PlotColor fontColor = FrameDefaults.Color)
        {
            // => FrameLite method
            FrameLite.SetLabelX(content, fontBold, fontSize, fontFamily, fontColor);
            // update label properties
            XAxisLabel.Value = content;
            XAxisLabel.Unit = fontSize;
        }

        /// <summary>
        /// sets the label of the vertical axis
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
        {
            // => FrameLite method
            FrameLite.SetLabelY(content, fontBold, fontSize, fontFamily, fontColor);
            // update label properties
            YAxisLabel.Value = content;
            YAxisLabel.Unit = fontSize;
        }

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
        {
            // => FrameLite method
            FrameLite.SetLabelYRight(content, fontBold, fontSize, fontFamily, fontColor);
            // update label properties
            YRightAxisLabel.Value = content;
            YRightAxisLabel.Unit = fontSize;
        }

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
        {
            FrameLite.SetLegend(enable, location, fontSize);
            CheckBox_ShowLegend.IsChecked = enable;
            ComboBox_LegendLocation.SelectedItem = location;
            TextBox_LegendFontSize.Text = fontSize.ToString();
        }

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
        {
            FrameLite.SaveFig(fullPath, width, height, lowQuality, scale);
        }

        /// <summary>
        /// Returns the actual width of frame
        /// </summary>
        public int GetActualWidth()
        => FrameLite.GetActualWidth();

        /// <summary>
        /// Returns the actual height of frame
        /// </summary>
        public int GetActualHeight()
        => FrameLite.GetActualHeight();

        #endregion
        #region ===== copy image to clipboard =====

        /// <summary>
        /// Copy the picture to the clipboard
        /// </summary>
        /// <param name="lowQuality"> if true, anti-aliasing will be disabled for this render. Default false </param>
        public void CopyToClipboard(bool lowQuality = false)
            => FrameLite.CopyToClipboard(lowQuality);

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
        {
            FrameLite.SetVerticalSpan(start, end,
                fillColor, opacity, lineStyle, numFormat, numDigits, isVisible);
            YSpanMin.Value = start;
            YSpanMax.Value = end;
            Ver_Span.IsChecked= isVisible;
        }

        /// <summary>
        /// modifies the properties of the horizontal axial span
        /// </summary>
        /// <param name="start"> start of the span </param>
        /// <param name="end"> end of the span </param>
        /// <param name="fillColor"> fill color of the span </param>
        /// <param name="opacity"> opacity of the fill color, between 0 and 1 </param>
        /// <param name="lineStyle"> style of the line </param>
        /// <param name="numFormat"> number format </param>
        /// <param name="numDigits"> number of digits to display </param>
        /// <param name="isVisible"> whether to show the span or not </param>
        public void SetHorizontalSpan(double start, double end,
            PlotColor fillColor = FrameDefaults.HSpanColor,
            double opacity = FrameDefaults.SpanColorOpacity,
            LineStyle lineStyle = FrameDefaults.LineType,
            NumericFormat numFormat = Defaults.NumberFormat,
            int numDigits = Defaults.NumberOfDigits,
            bool isVisible = true)
        {
            FrameLite.SetHorizontalSpan(start, end, 
                fillColor, opacity, lineStyle, numFormat, numDigits, isVisible);
            // update axis span properties
            XSpanMin.Value = start;
            XSpanMax.Value = end;
            Hor_Span.IsChecked= isVisible;
        }

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
            PlotColor fontColor = FrameDefaults.Color, 
            PlotColor backgroudColor = FrameDefaults.VSpanColor, 
            double fontSize = FrameDefaults.AxisLabelSize, 
            LegendLocation location = LegendLocation.UpperRight, 
            double marginX = 20, 
            double marginY = 20)
            => FrameLite.SetAnnotation(label, fontColor, backgroudColor, fontSize, location, marginX, marginY);

        #endregion
        #region===== crosshair =====

        /// <summary>
        /// the crosshair plot type draws vertical and horizontal lines
        /// </summary>
        /// <param name="x"> x-axis coordinate (physical) </param>
        /// <param name="y"> y-axis coordinate (physical) </param>
        /// <param name="color"> color to draw the lines </param>
        /// <param name="style"> line style </param>
        /// <param name="numFormat"> number format </param>
        /// <param name="numDigits"> number of digits </param>
        /// <param name="isVisible"> whether to show the crosshair or not </param>
        public void SetCrosshair(double x, double y,
            PlotColor color = FrameDefaults.CrosshairColor,
            LineStyle style = FrameDefaults.CrosshairLineType,
            NumericFormat numFormat = Defaults.NumberFormat,
            int numDigits = Defaults.NumberOfDigits,
            bool isVisible = true)
            => FrameLite.SetCrosshair(x, y,
                color, style, numFormat, numDigits, isVisible);

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
            // FrameLite => add grid plot
            SignalPlot sgPlot = FrameLite.AddGridPlot(values, grid,
                lineWidth, lineStyle, markerSize, markerShape, plotColor, visualOption, label);

            // generate the Expander
            DataPropertyExpander dataProperty = new()
            {
                DataLabel = sgPlot.Label,
                VisualOption = visualOption,
                LineWidth = lineWidth,
                LineStyle = lineStyle,
                MarkerSize = markerSize,
                MarkerShape = markerShape,
                PlotColor = plotColor,
            };
            DataSourcePanel.Children.Add(dataProperty);

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
            // FrameLite => add grid plot
            SignalPlot sgPlot = FrameLite.AddGridPlot(values, grid, plotPart,
                lineWidth, lineStyle, markerSize, markerShape, plotColor, visualOption, label);

            // generate the Expander
            DataPropertyExpander dataProperty = new()
            {
                DataLabel = sgPlot.Label,
                VisualOption = visualOption,
                PlotPart = plotPart,
                LineWidth = lineWidth,
                LineStyle = lineStyle,
                MarkerSize = markerSize,
                MarkerShape = markerShape,
                PlotColor = plotColor,
            };
            DataSourcePanel.Children.Add(dataProperty);

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
        {
            VectorZ temp = gv.Values;
            gv.Phase.AddTo(x: ref temp, grid: gv.GridInfo,
                part: ComplexPart.Argument);
            return AddGridPlot(temp, gv.GridInfo, plotPart,
                lineWidth, lineStyle,
                markerSize, markerShape, plotColor,
                visualOption, label);
        }


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
            // FrameLite => add scat plot
            ScatterPlot scPlot = FrameLite.AddScatPlot(locations, values,
                lineWidth, lineStyle, markerSize, markerShape, plotColor, visualOption, label);

            // generate the Expander
            DataPropertyExpander dataProperty = new()
            {
                DataLabel = scPlot.Label,
                VisualOption = visualOption,
                LineWidth = lineWidth,
                LineStyle = lineStyle,
                MarkerSize = markerSize,
                MarkerShape = markerShape,
                PlotColor = plotColor,
            };
            DataSourcePanel.Children.Add(dataProperty);

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
            // FrameLite => add scat plot
            ScatterPlot scPlot = FrameLite.AddScatPlot(locations, values, plotPart,
                lineWidth, lineStyle, markerSize, markerShape, plotColor, visualOption, label);

            // generate the Expander
            DataPropertyExpander dataProperty = new()
            {
                DataLabel = scPlot.Label,
                VisualOption = visualOption,
                PlotPart = plotPart,
                LineWidth = lineWidth,
                LineStyle = lineStyle,
                MarkerSize = markerSize,
                MarkerShape = markerShape,
                PlotColor = plotColor,
            };
            DataSourcePanel.Children.Add(dataProperty);

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
            // FrameLite => add func plot
            FunctionPlot fcPlot = FrameLite.AddFuncPlot(f,
                lineWidth, lineStyle, plotColor, visualOption, label);

            // generate the Expander
            DataPropertyExpander dataProperty = new()
            {
                DataLabel = fcPlot.Label,
                VisualOption = visualOption,
                LineWidth = lineWidth,
                LineStyle = lineStyle,
                PlotColor = plotColor,
            };
            DataSourcePanel.Children.Add(dataProperty);

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
            // FrameLite => add func plot
            FunctionPlot fcPlot = FrameLite.AddFuncPlot(f, plotPart,
                lineWidth, lineStyle, plotColor, visualOption, label);

            // generate the Expander
            DataPropertyExpander dataProperty = new()
            {
                DataLabel = fcPlot.Label,
                VisualOption = visualOption,
                PlotPart = plotPart,
                LineWidth = lineWidth,
                LineStyle = lineStyle,
                PlotColor = plotColor,
            };
            DataSourcePanel.Children.Add(dataProperty);

            return fcPlot;
        }

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
        public Heatmap AddGridGraph(MatrixD values,
            GridInfo2D? grid = null,
            PlotColormap colormap = FrameDefaults.Colormap,
            GraphInterpolationMode smoothMode = FrameDefaults.SmoothMode,
            VisualOption visualOption = FrameDefaults.ViewOption,
            string? label = null)
        {
            // FrameLite => add grid plot
            Heatmap htMap= FrameLite.AddGridGraph(values, grid,
                colormap, smoothMode, visualOption, label);

            // generate the Expander
            DataPropertyExpander dataProperty = new()
            {
                DataLabel = htMap.Label,
                VisualOption = visualOption,
                Colormap = colormap,
                SmoothMode = smoothMode,
            };
            DataSourcePanel.Children.Add(dataProperty);

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
            // FrameLite => add grid graph
            Heatmap htMap = FrameLite.AddGridGraph(values, grid, plotPart,
                colormap, smoothMode, visualOption, label);

            // generate the Expander
            DataPropertyExpander dataProperty = new()
            {
                DataLabel = htMap.Label,
                VisualOption = visualOption,
                PlotPart = plotPart,
                Colormap = colormap,
                SmoothMode = smoothMode,
            };
            DataSourcePanel.Children.Add(dataProperty);

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
        {
            MatrixZ temp = gm.Values;
            gm.Phase.AddTo(x: ref temp, grid: gm.GridInfo,
                part: ComplexPart.Argument);
            return AddGridGraph(temp, gm.GridInfo, plotPart,
                colormap, smoothMode, visualOption, label);
        }
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
        public FrameCommons.DotsMap AddScatGraph(VectorD x, VectorD y,
            double markerSize = FrameDefaults.MarkerSize,
            MarkerShape markerShape = FrameDefaults.MarkerType,
            PlotColor plotColor = FrameDefaults.Color,
            VisualOption visualOption = FrameDefaults.ViewOption,
            string? label = null)
        {
            // FrameLite => add grid plot
            FrameCommons.DotsMap dotsMap = FrameLite.AddScatGraph(x,y,
                markerSize, markerShape, plotColor, visualOption, label);

            // generate the Expander
            DataPropertyExpander dataProperty = new()
            {
                DataLabel = dotsMap.Label,
                VisualOption = visualOption,
                MarkerSize = markerSize,
                MarkerShape = markerShape,
                PlotColor = plotColor
            };
            DataSourcePanel.Children.Add(dataProperty);

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
            // FrameLite => add grid plot
            Image img = FrameLite.AddBMPGraph(image,
                x, y, anchor, visualOption, label);

            // generate the Expander
            DataPropertyExpander dataProperty = new()
            {
                DataLabel = img.Label,
                VisualOption = visualOption,
                //Colormap = colormap,
            };
            DataSourcePanel.Children.Add(dataProperty);

            return img;
        }

        #endregion

        //
        public void DeletePlot(int i) => FrameLite.DeletePlot(i);
        //

        #endregion
        #region ------- update -------

        /// <summary>
        /// expose the refresh method
        /// </summary>
        public void Refresh() => FrameLite.Refresh();

        /// <summary>
        /// updates all the plots 
        /// according to the property settings
        /// </summary>
        public void UpdatePlots()
        {
            bool needYLeft = false;
            bool needYRight = false;
            // loop for all data sources
            for (int i = 0; i < AllPlots.Count; i++)
            {
                DataPropertyExpander dataProperties = (DataPropertyExpander)DataSourcePanel.Children[i];
                ComplexPart currentPart = AllParts[i];
                ComplexPart targetPart = dataProperties.PlotPart ?? currentPart;
                // check the type of plot
                if (AllPlots[i].GetType() == typeof(SignalPlot)) // Signal Plot
                {
                    VectorZ values = (VectorZ)AllData[i];
                    SignalPlot plot = (SignalPlot)AllPlots[i];
                    // update of line and marker
                    FrameCommons.UpdateGridPlot(plot, dataProperties);
                    // update data if complex part is changed
                    if(currentPart != targetPart)
                    {
                        FrameCommons.UpdateGridPlot(plot, values, targetPart);
                        AllParts[i] = targetPart;
                    }
                    // handle y-axis visual options
                    CheckIfNeedYAxes(dataProperties.VisualOption, ref needYLeft, ref needYRight);
                    EnableYLeftAxis = needYLeft;
                    EnableYRightAxis = needYRight;
                }
                else if (AllPlots[i].GetType() == typeof(ScatterPlot)) // Scatter Plot
                {
                    VectorZ values = (VectorZ)AllData[i];
                    ScatterPlot plot = (ScatterPlot)AllPlots[i];
                    // update of line and marker
                    FrameCommons.UpdateScatPlot(plot, dataProperties);
                    // update data if complex part is changed
                    if (currentPart != targetPart)
                    {
                        FrameCommons.UpdateScatPlot(plot, values, targetPart);
                        AllParts[i] = targetPart;
                    }
                    // handle y-axis visual options
                    CheckIfNeedYAxes(dataProperties.VisualOption, ref needYLeft, ref needYRight);
                    EnableYLeftAxis = needYLeft;
                    EnableYRightAxis = needYRight;
                }
                else if(AllPlots[i].GetType() == typeof(FunctionPlot)) // Function Plot
                {
                    Func<double, Complex?> f = (Func<double, Complex?>)AllData[i];
                    FunctionPlot plot = (FunctionPlot)AllPlots[i];
                    // update of line
                    FrameCommons.UpdateFuncPlot(plot, dataProperties);
                    // update data if complex part is changed
                    if (currentPart != targetPart)
                    {
                        FrameCommons.UpdateFuncPlot(plot, f, targetPart);
                        AllParts[i] = targetPart;
                    }
                    // handle y-axis visual options
                    CheckIfNeedYAxes(dataProperties.VisualOption, ref needYLeft, ref needYRight);
                    EnableYLeftAxis = needYLeft;
                    EnableYRightAxis = needYRight;
                }
                else if (AllPlots[i].GetType() == typeof(Heatmap)) // Heatmap
                {
                    Heatmap graph = (Heatmap)AllPlots[i];
                    Colorbar colorbar = AllColorbars[i];

                    if (AllData[i].GetType() == typeof(MatrixD)) // real-valued case
                    {
                        // gets the data to plot
                        MatrixD values = (MatrixD)AllData[i];
                        // gets the colormaps
                        Colormap currentColormap = graph.Colormap;
                        PlotColormap targetColormap = dataProperties.Colormap ?? FrameDefaults.Colormap;
                        // call update method
                        FrameCommons.UpdateGridGraph(graph, colorbar, dataProperties,
                            values, currentColormap, targetColormap);
                    }
                    else // complex-valued case
                    {
                        // gets the data to plot
                        MatrixZ values = (MatrixZ)AllData[i];
                        // gets the colormaps
                        Colormap currentColormap = graph.Colormap;
                        PlotColormap targetColormap = dataProperties.Colormap ?? FrameDefaults.Colormap;
                        // call update method
                        FrameCommons.UpdateGridGraph(graph, colorbar, dataProperties,
                            values, currentPart, targetPart, 
                            currentColormap, targetColormap);
                        AllParts[i] = targetPart;
                    }
                }
            }

            // handle frame changes
            // common operations
            // 1) x - view range; 2) x - axis ticks; 3) title; 4) label x
            SetXAxisLimits(min: XRangeMin.Value, max: XRangeMax.Value);
            SetXAxisTicks(tickDensity: XTick.Value, fontSize: XTick.Unit);
            SetTitle(content: FrameTitle.Value, fontSize: FrameTitle.Unit);
            SetLabelX(content: XAxisLabel.Value, fontSize: XAxisLabel.Unit);
            // span ...
            SetHorizontalSpan(start: XSpanMin.Value, end: XSpanMax.Value, 
                fillColor: (PlotColor)X_Span_FillColor.SelectedValue, 
                opacity: (double)X_Span_Opacity.SelectedValue,
                lineStyle: (LineStyle)X_Span_LineStyle.SelectedValue, 
                isVisible: (bool)Hor_Span.IsChecked);
            SetVerticalSpan(start: YSpanMin.Value, end: YSpanMax.Value, 
                fillColor: (PlotColor)Y_Span_FillColor.SelectedValue, 
                opacity: (double)Y_Span_Opacity.SelectedValue, 
                lineStyle: (LineStyle)Y_Span_LineStyle.SelectedValue, 
                isVisible: (bool)Ver_Span.IsChecked);
            
            // cases ...
            switch (EnableYLeftAxis, EnableYRightAxis)
            {
                case (true, false): // left y-axis only
                    {
                        // set view ranges
                        SetYAxisLimits(min: YRangeMin.Value, max: YRangeMax.Value);
                        // set axis ticks
                        SetYAxisTicks(enableGrid: EnableYLeftAxis, enableTicks: EnableYLeftAxis,
                            tickDensity: YTick.Value, fontSize: YTick.Unit);
                        SetYRightAxisTicks(enableGrid: EnableYRightAxis, enableTicks: EnableYRightAxis);
                        // set axis labels
                        SetLabelY(content: YAxisLabel.Value, fontSize: YAxisLabel.Unit);
                        FrameLite.PlotFrame.Plot.RightAxis.Label("");
                        // cursors ...
                        // handle data grid items
                        ViewRange = new List<NumberQuantity> { XRangeMin, XRangeMax, YRangeMin, YRangeMax };
                        AxisTick = new List<NumberQuantity> { XTick, YTick };
                        TitleLabel = new List<TextQuantity> { FrameTitle, XAxisLabel, YAxisLabel };
                        CursorInfo = new List<NumberQuantity> { PhysicalX, PhysicalY, PixelX, PixelY };
                        XAxisSpan = new List<NumberQuantity> { XSpanMin, XSpanMax };
                        YAxisSpan = new List<NumberQuantity> { YSpanMin, YSpanMax };
                        break;
                    }
                case (false, true): // right y-axis only
                    {
                        // set view ranges
                        SetYRightAxisLimits(min: YRightRangeMin.Value, max: YRightRangeMax.Value);
                        // set axis ticks
                        SetYAxisTicks(enableGrid: EnableYLeftAxis, enableTicks: EnableYLeftAxis);
                        SetYRightAxisTicks(enableGrid: EnableYRightAxis, enableTicks: EnableYRightAxis,
                            tickDensity: YRightTick.Value, fontSize: YRightTick.Unit);
                        // set axis labels
                        SetLabelYRight(content: YRightAxisLabel.Value, fontSize: YRightAxisLabel.Unit);
                        FrameLite.PlotFrame.Plot.LeftAxis.Label("");
                        // cursors ...
                        // handle data grid items
                        ViewRange = new List<NumberQuantity> { XRangeMin, XRangeMax, YRightRangeMin, YRightRangeMax };
                        AxisTick = new List<NumberQuantity> { XTick, YRightTick };
                        TitleLabel = new List<TextQuantity> { FrameTitle, XAxisLabel, YRightAxisLabel };
                        CursorInfo = new List<NumberQuantity> { PhysicalX, PhysicalYRight, PixelX, PixelY };
                        break;
                    }
                case (true, true): // both y-axes
                    {
                        // set view ranges
                        SetYAxisLimits(min: YRangeMin.Value, max: YRangeMax.Value);
                        SetYRightAxisLimits(min: YRightRangeMin.Value, max: YRightRangeMax.Value);
                        // set axis ticks
                        SetYAxisTicks(enableGrid: EnableYLeftAxis, enableTicks: EnableYLeftAxis,
                            tickDensity: YTick.Value, fontSize: YTick.Unit);
                        SetYRightAxisTicks(enableGrid: EnableYRightAxis, enableTicks: EnableYRightAxis,
                            tickDensity: YRightTick.Value, fontSize: YRightTick.Unit);
                        // set axis labels
                        SetLabelY(content: YAxisLabel.Value, fontSize: YAxisLabel.Unit);
                        SetLabelYRight(content: YRightAxisLabel.Value, fontSize: YRightAxisLabel.Unit);
                        // cursors ...
                        // handle data grid items
                        ViewRange = new List<NumberQuantity> { XRangeMin, XRangeMax, YRangeMin, YRangeMax, YRightRangeMin, YRightRangeMax };
                        AxisTick = new List<NumberQuantity> { XTick, YTick, YRightTick };
                        TitleLabel = new List<TextQuantity> { FrameTitle, XAxisLabel, YAxisLabel, YRightAxisLabel };
                        CursorInfo = new List<NumberQuantity> { PhysicalX, PhysicalY, PhysicalYRight, PixelX, PixelY };
                        XAxisSpan = new List<NumberQuantity> { XSpanMin, XSpanMax };
                        YAxisSpan = new List<NumberQuantity> { YSpanMin, YSpanMax };
                        break;
                    }
                default: // none
                    break;
            }
            // set item sources
            RangeDataGrid.ItemsSource = ViewRange;
            TickDataGrid.ItemsSource = AxisTick;
            TextDataGrid.ItemsSource = TitleLabel;
            CursorDataGrid.ItemsSource = CursorInfo;
            XSpanDataGrid.ItemsSource = XAxisSpan;
            YSpanDataGrid.ItemsSource = YAxisSpan;
            // set legend
            SetLegend(enable: CheckBox_ShowLegend.IsChecked ?? false,
                location: (LegendLocation)ComboBox_LegendLocation.SelectedValue,
                fontSize: double.Parse(TextBox_LegendFontSize.Text));

            // refresh
            Refresh();
        }

        /// <summary>
        /// checks if the left or right Y-axis is needed
        /// according to the selected visual option
        /// </summary>
        /// <param name="visualOption"> selected visual option </param>
        /// <param name="needYLeft"> original state of left y-axis, can be overwritten </param>
        /// <param name="needYRight"> original state of right y-axis, can be overwritten </param>
        private static void CheckIfNeedYAxes(VisualOption visualOption,
            ref bool needYLeft, ref bool needYRight)
        {
            bool iNeedYLeft = visualOption switch
            {
                VisualOption.Visible => true,
                VisualOption.VisibleRight => false,
                _ => false
            };
            needYLeft = (needYLeft || iNeedYLeft);
            bool iNeedYRight = visualOption switch
            {
                VisualOption.Visible => false,
                VisualOption.VisibleRight => true,
                _ => false
            };
            needYRight = (needYRight || iNeedYRight);
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
            double x, yLeft, yRight;
            double px, py;
            (x, yLeft) = FrameLite.PlotFrame.GetMouseCoordinates(0, 0);
            (x, yRight) = FrameLite.PlotFrame.GetMouseCoordinates(0, 1);
            (px, py) = FrameLite.PlotFrame.GetMousePixel();

            PhysicalX.Value = x; //DisplayValue = x;// double.Parse(Converter.NumberToString(x));
            PhysicalY.Value = yLeft; //DisplayValue = yLeft;// double.Parse(Converter.NumberToString(yLeft));
            PhysicalYRight.Value = yRight; //DisplayValue = yRight;// double.Parse(Converter.NumberToString(yRight));

            PixelX.Value = px; //DisplayValue = px;// double.Parse(Converter.NumberToString(px));
            PixelY.Value = py; //DisplayValue = py;// double.Parse(Converter.NumberToString(py));

            SetCrosshair(x: x, y: yLeft, // ...
                color: (PlotColor)Crosshair_Color.SelectedValue,
                style: (LineStyle)Crosshair_LineStyle.SelectedValue,
                numFormat: Defaults.NumberFormat,
                numDigits: Defaults.NumberOfDigits,
                isVisible: (bool)Crosshair_Show.IsChecked);

        }

        /// <summary>
        /// finds the axis limits after it is changed
        /// </summary>
        /// <param name="sender"> sender </param>
        /// <param name="e"> event </param>
        private void GetAxisLimits(object? sender, EventArgs e)
        {
            AxisLimits axisLimit00 = FrameLite.PlotFrame.Plot.GetAxisLimits(xAxisIndex: 0, yAxisIndex: 0);
            // controls number of digits
            XRangeMin.Value = axisLimit00.XMin; //DisplayValue = axisLimit00.XMin;// double.Parse(Converter.NumberToString(axisLimit00.XMin, digits:16));
            XRangeMax.Value = axisLimit00.XMax; //DisplayValue = axisLimit00.XMax;//double.Parse(Converter.NumberToString(axisLimit00.XMax, digits: 16));
            YRangeMin.Value = axisLimit00.YMin; //DisplayValue = axisLimit00.YMin;// double.Parse(Converter.NumberToString(axisLimit00.YMin, digits: 16));
            YRangeMax.Value = axisLimit00.YMax; //DisplayValue = axisLimit00.YMax;// double.Parse(Converter.NumberToString(axisLimit00.YMax, digits: 16));
            if(EnableYRightAxis)
            {
                AxisLimits axisLimit01 = FrameLite.PlotFrame.Plot.GetAxisLimits(xAxisIndex: 0, yAxisIndex: 1);
                // controls number of digits
                YRightRangeMin.Value = axisLimit01.YMin; //DisplayValue = axisLimit01.YMin;// double.Parse(Converter.NumberToString(axisLimit01.YMin, digits: 16));
                YRightRangeMax.Value = axisLimit01.YMax; //DisplayValue = axisLimit01.YMax;// double.Parse(Converter.NumberToString(axisLimit01.YMax, digits: 16));
            }
        }

        #endregion

        #endregion


        #region buttons

        /// <summary>
        /// updates plot(s)
        /// </summary>
        /// <param name="sender"> sender </param>
        /// <param name="e"> e </param>
        private void PlotButton(object sender, RoutedEventArgs e)
            => UpdatePlots();

        /// <summary>
        /// detects horizontal axis limits automatically
        /// </summary>
        /// <param name="sender"> sender </param>
        /// <param name="e"> e </param>
        private void AutoXButton(object sender, RoutedEventArgs e)
        {
            DetectXAxisLimits();
            Refresh();
        }

        /// <summary>
        /// detects vertical axis limits automatically
        /// </summary>
        /// <param name="sender"> sender </param>
        /// <param name="e"> e </param>
        private void AutoYButton(object sender, RoutedEventArgs e)
        {
            DetectYAxisLimits(); 
            Refresh();
        }

        /// <summary>
        /// detects both axes limits automatically
        /// </summary>
        /// <param name="sender"> sender </param>
        /// <param name="e"> e </param>
        private void AutoXYButton(object sender, RoutedEventArgs e)
        {
            DetectAxisLimits();
            Refresh();
        }


        #endregion
        #region special handling ...

        // enabling focusing for TextBox when it enters editing mode
        private void EditingTextBox_Loaded(object sender, RoutedEventArgs e)
        {
            TextBox? tb = sender as TextBox;
            if (tb == null) return;

            tb.SelectAll();
            FocusManager.SetFocusedElement(this, tb);
        }

        #endregion
    }
}
