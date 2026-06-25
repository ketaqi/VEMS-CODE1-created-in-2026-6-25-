using ScottPlot;
using ScottPlot.Drawing;
using ScottPlot.Plottable;
using System;
using System.Drawing;
using System.Numerics;
using VEMS.MathCore;
using static VEMS.Plot.Options;
using Color = System.Drawing.Color;
using LineStyle = VEMS.Plot.Options.LineStyle;
using MarkerShape = VEMS.Plot.Options.MarkerShape;
using Image = ScottPlot.Plottable.Image;
using System.Drawing.Drawing2D;
using System.Windows;

namespace VEMS.Plot
{
    /// <summary>
    /// common methods for frame
    /// </summary>
    public class FrameCommons
    {
        #region ===== parsers =====

        private static ScottPlot.LineStyle ParseLineStyle(LineStyle lineStyle)
            => (ScottPlot.LineStyle)Enum.Parse(typeof(ScottPlot.LineStyle), lineStyle.ToString());

        private static ScottPlot.MarkerShape ParseMarkerShape(MarkerShape markerShape)
            => (ScottPlot.MarkerShape)Enum.Parse(typeof(ScottPlot.MarkerShape), markerShape.ToString());

        private static System.Drawing.Color ParseColor(PlotColor color)
            => Color.FromName(color.ToString());

        private static System.Drawing.Color ParseColor(int v, PlotColor color)
            => Color.FromArgb(v, ParseColor(color));

        private static ScottPlot.Drawing.Colormap ParseColormap(PlotColormap colormap)
            => Colormap.GetColormapByName(colormap.ToString());


        private static InterpolationMode ParseGraphInterpolationMode(GraphInterpolationMode mode)
        {
            switch(mode)
            {
                case GraphInterpolationMode.Default:
                    return InterpolationMode.Default;
                case GraphInterpolationMode.Low:
                    return InterpolationMode.Low;
                case GraphInterpolationMode.High:
                    return InterpolationMode.High;
                case GraphInterpolationMode.Bilinear:
                    return InterpolationMode.Bilinear;
                case GraphInterpolationMode.Bicubic:
                    return InterpolationMode.Bicubic;
                case GraphInterpolationMode.NearestNeighbor: 
                    return InterpolationMode.NearestNeighbor;
                case GraphInterpolationMode.HighQualityBilinear:
                    return InterpolationMode.HighQualityBilinear;
                case GraphInterpolationMode.HighQualityBicubic:
                    return InterpolationMode.HighQualityBicubic;
                default:
                    return InterpolationMode.Default;
            }
        }

        private static Alignment ParseLegendLocation(LegendLocation location)
        {
            switch(location)
            {
                case LegendLocation.UpperLeft:
                    return Alignment.UpperLeft;
                case LegendLocation.UpperRight:
                    return Alignment.UpperRight;
                case LegendLocation.UpperCenter:
                    return Alignment.UpperCenter;
                case LegendLocation.MiddleLeft:
                    return Alignment.MiddleLeft;
                case LegendLocation.MiddleRight:
                    return Alignment.MiddleRight;
                case LegendLocation.MiddleCenter:
                    return Alignment.MiddleCenter;
                case LegendLocation.LowerLeft:
                    return Alignment.LowerLeft;
                case LegendLocation.LowerRight:
                    return Alignment.LowerRight;
                case LegendLocation.LowerCenter:
                    return Alignment.LowerCenter;
                default:
                    return Alignment.LowerRight;
            }
        }

        #endregion

        #region ------- frame -------

        #region ===== ticks =====

        /// <summary>
        /// sets the horizontal axis of the frame
        /// by defining tick density
        /// </summary>
        /// <param name="frame"> frame </param>
        /// <param name="enableGrid"> enable grid </param>
        /// <param name="enableTicks"> enable ticks </param>
        /// <param name="tickDensity"> density of the ticks </param>
        /// <param name="fontBold"> whether to set the ticks bold </param>
        /// <param name="fontSize"> font size of the ticks </param>
        /// <param name="fontFamily"> font family of the ticks </param>
        /// <param name="fontColor"> color of the ticks </param>
        public static void SetXAxisTicks(WpfPlot frame,
            bool enableGrid = true,
            bool enableTicks = true,
            double tickDensity = FrameDefaults.TickDensity,
            bool fontBold = false,
            double fontSize = FrameDefaults.TickSize,
            string fontFamily = FrameDefaults.FontFamily,
            PlotColor fontColor = FrameDefaults.Color)
        {
            frame.Plot.XAxis.Grid(enableGrid);
            frame.Plot.XAxis.Ticks(enableTicks);
            frame.Plot.XAxis.TickDensity(tickDensity);
            frame.Plot.XAxis.TickLabelStyle(color: ParseColor(fontColor),
                fontName: fontFamily,
                fontSize: (float?)fontSize,
                fontBold: fontBold);
        }

        /// <summary>
        /// sets the vertical axis on the left of the frame
        /// by defining tick density
        /// </summary>
        /// <param name="frame"> frame </param>
        /// <param name="enableGrid"> enable grid </param>
        /// <param name="enableTicks"> enable ticks </param>
        /// <param name="tickDensity"> density of the ticks </param>
        /// <param name="fontBold"> whether to set the ticks bold </param>
        /// <param name="fontSize"> font size of the ticks </param>
        /// <param name="fontFamily"> font family of the ticks </param>
        /// <param name="fontColor"> color of the ticks </param>
        public static void SetYAxisTicks(WpfPlot frame,
            bool enableGrid = true,
            bool enableTicks = true,
            double tickDensity = FrameDefaults.TickDensity,
            bool fontBold = false,
            double fontSize = FrameDefaults.TickSize,
            string fontFamily = FrameDefaults.FontFamily,
            PlotColor fontColor = FrameDefaults.Color)
        {
            frame.Plot.YAxis.Grid(enableGrid);
            frame.Plot.YAxis.Ticks(enableTicks);
            frame.Plot.YAxis.TickDensity(tickDensity);
            frame.Plot.YAxis.TickLabelStyle(color: ParseColor(fontColor),
                fontName: fontFamily,
                fontSize: (float?)fontSize,
                fontBold: fontBold);
        }

        /// <summary>
        /// sets the vertical axis on the right of the frame
        /// by defining tick density
        /// </summary>
        /// <param name="frame"> frame </param>
        /// <param name="enableGrid"> enable grid </param>
        /// <param name="enableTicks"> enable ticks </param>
        /// <param name="tickDensity"> density of the ticks </param>
        /// <param name="fontBold"> whether to set the ticks bold </param>
        /// <param name="fontSize"> font size of the ticks </param>
        /// <param name="fontFamily"> font family of the ticks </param>
        /// <param name="fontColor"> color of the ticks </param>
        public static void SetYRightAxisTicks(WpfPlot frame,
            bool enableGrid = true,
            bool enableTicks = true,
            double tickDensity = FrameDefaults.TickDensity,
            bool fontBold = false,
            double fontSize = FrameDefaults.TickSize,
            string fontFamily = FrameDefaults.FontFamily,
            PlotColor fontColor = FrameDefaults.Color)
        {
            frame.Plot.RightAxis.Grid(enableGrid);
            frame.Plot.RightAxis.Ticks(enableTicks);
            frame.Plot.RightAxis.TickDensity(tickDensity);
            frame.Plot.RightAxis.TickLabelStyle(color: ParseColor(fontColor),
                fontName: fontFamily,
                fontSize: (float?)fontSize,
                fontBold: fontBold);
        }

        /// <summary>
        /// sets the horizontal axis of the frame
        /// by defining tick spacing
        /// </summary>
        /// <param name="frame"> frame </param>
        /// <param name="enableGrid"> enable grid </param>
        /// <param name="enableTicks"> enable ticks </param>
        /// <param name="spacing"> spacing of the ticks </param>
        /// <param name="fontBold"> whether to set the ticks bold </param>
        /// <param name="fontSize"> font size of the ticks </param>
        /// <param name="fontFamily"> font family of the ticks </param>
        /// <param name="fontColor"> color of the ticks </param>
        private static void SetXAxisSpacing(WpfPlot frame,
            bool enableGrid = true,
            bool enableTicks = true,
            double spacing = FrameDefaults.TickSpacing,
            bool fontBold = false,
            double fontSize = FrameDefaults.TickSize,
            string fontFamily = FrameDefaults.FontFamily,
            PlotColor fontColor = FrameDefaults.Color)
        {
            frame.Plot.XAxis.Grid(enableGrid);
            frame.Plot.XAxis.Ticks(enableTicks);
            frame.Plot.XAxis.ManualTickSpacing(spacing);
            frame.Plot.XAxis.TickLabelStyle(color: ParseColor(fontColor),
                fontName: fontFamily,
                fontSize: (float?)fontSize,
                fontBold: fontBold);
        }

        /// <summary>
        /// sets the vertical axis on the left of the frame
        /// by defining tick spacing        
        /// </summary>
        /// <param name="frame"> frame </param>
        /// <param name="enableGrid"> enable grid </param>
        /// <param name="enableTicks"> enable ticks </param>
        /// <param name="spacing"> spacing of the ticks </param>
        /// <param name="fontBold"> whether to set the ticks bold </param>
        /// <param name="fontSize"> font size of the ticks </param>
        /// <param name="fontFamily"> font family of the ticks </param>
        /// <param name="fontColor"> color of the ticks </param>
        private static void SetYAxisSpacing(WpfPlot frame,
            bool enableGrid = true,
            bool enableTicks = true,
            double spacing = FrameDefaults.TickSpacing,
            bool fontBold = false,
            double fontSize = FrameDefaults.TickSize,
            string fontFamily = FrameDefaults.FontFamily,
            PlotColor fontColor = FrameDefaults.Color)
        {
            frame.Plot.YAxis.Grid(enableGrid);
            frame.Plot.YAxis.Ticks(enableTicks);
            frame.Plot.YAxis.ManualTickSpacing(spacing);
            frame.Plot.YAxis.TickLabelStyle(color: ParseColor(fontColor),
                fontName: fontFamily,
                fontSize: (float?)fontSize,
                fontBold: fontBold);
        }

        /// <summary>
        /// sets the vertical axis on the right of the frame
        /// by defining tick spacing
        /// </summary>
        /// <param name="frame"> frame </param>
        /// <param name="enableGrid"> enable grid </param>
        /// <param name="enableTicks"> enable ticks </param>
        /// <param name="spacing"> spacing of the ticks </param>
        /// <param name="fontBold"> whether to set the ticks bold </param>
        /// <param name="fontSize"> font size of the ticks </param>
        /// <param name="fontFamily"> font family of the ticks </param>
        /// <param name="fontColor"> color of the ticks </param>
        private static void SetYRightAxisSpacing(WpfPlot frame,
            bool enableGrid = true,
            bool enableTicks = true,
            double spacing = FrameDefaults.TickSpacing,
            bool fontBold = false,
            double fontSize = FrameDefaults.TickSize,
            string fontFamily = FrameDefaults.FontFamily,
            PlotColor fontColor = FrameDefaults.Color)
        {
            frame.Plot.YAxis2.Grid(enableGrid);
            frame.Plot.YAxis2.Ticks(enableTicks);
            frame.Plot.YAxis2.ManualTickSpacing(spacing);
            frame.Plot.YAxis2.TickLabelStyle(color: ParseColor(fontColor),
                fontName: fontFamily,
                fontSize: (float?)fontSize,
                fontBold: fontBold);
        }

        #endregion
        #region ===== range =====

        /// <summary>
        /// sets the horizontal axis limits of the frame
        /// </summary>
        /// <param name="frame"> frame </param>
        /// <param name="min"> minimum value </param>
        /// <param name="max"> maximum value </param>
        public static void SetXAxisLimits(WpfPlot frame, double min, double max)
            => frame.Plot.SetAxisLimitsX(min, max);

        /// <summary>
        /// sets the vertical axis limits on the left of the frame
        /// </summary>
        /// <param name="frame"> frame </param>
        /// <param name="min"> minimum value </param>
        /// <param name="max"> maximum value </param>
        public static void SetYAxisLimits(WpfPlot frame, double min, double max)
            => frame.Plot.SetAxisLimitsY(min, max, yAxisIndex: frame.Plot.LeftAxis.AxisIndex);

        /// <summary>
        /// sets the vertical axis limits on the right of the frame
        /// </summary>
        /// <param name="frame"> frame </param>
        /// <param name="min"> minimum value </param>
        /// <param name="max"> maximum value </param>
        public static void SetYRightAxisLimits(WpfPlot frame, double min, double max)
            => frame.Plot.SetAxisLimitsY(min, max, yAxisIndex: frame.Plot.RightAxis.AxisIndex);

        /// <summary>
        /// detects the axis limits automatically
        /// </summary>
        /// <param name="frame"> frame </param>
        /// <param name="xMin"> minimum value along horizontal direction </param>
        /// <param name="xMax"> maximum value along horizontal direction </param>
        /// <param name="yMin"> minimum value along vertical direction </param>
        /// <param name="yMax"> maximum value along vertical direction </param>
        public static void DetectAxisLimits(WpfPlot frame, 
            out double xMin, out double xMax,
            out double yMin, out double yMax,
            out double yRightMin, out double yRightMax)
        {
            frame.Plot.AxisAuto();

            AxisLimits axisLimits00 = frame.Plot.GetAxisLimits(0, 0);
            xMin = axisLimits00.XMin;
            xMax = axisLimits00.XMax;
            yMin = axisLimits00.YMin;
            yMax = axisLimits00.YMax;

            AxisLimits axisLimits01 = frame.Plot.GetAxisLimits(0, 1);
            yRightMin = axisLimits01.YMin;
            yRightMax = axisLimits01.YMax;
        }

        /// <summary>
        /// detects the x-axis limits automatically
        /// </summary>
        /// <param name="frame"> frame </param>
        /// <returns> (xMin, xMax) </returns>
        public static (double, double) DetectXAxisLimits(WpfPlot frame)
        {
            frame.Plot.AxisAutoX(xAxisIndex: 0);
            AxisLimits axisLimits = frame.Plot.GetAxisLimits(0, 0);
            return (axisLimits.XMin, axisLimits.XMax);
        }

        /// <summary>
        /// detects the y-axis limits automatically
        /// </summary>
        /// <param name="frame"> frame </param>
        /// <returns> (yMin, yMax) </returns>
        public static (double, double) DetectYAxisLimits(WpfPlot frame)
        {
            frame.Plot.AxisAutoY(yAxisIndex: 0);
            AxisLimits axisLimits = frame.Plot.GetAxisLimits(0, 0);
            return (axisLimits.YMin, axisLimits.YMax);
        }

        /// <summary>
        /// detects the y-axis (right) limits automatically
        /// </summary>
        /// <param name="frame"> frame </param>
        /// <returns> (yMin, yMax) </returns>
        public static (double, double) DetectYRightAxisLimits(WpfPlot frame)
        {
            frame.Plot.AxisAutoY(yAxisIndex: frame.Plot.RightAxis.AxisIndex);
            AxisLimits axisLimits = frame.Plot.GetAxisLimits(0, 1);
            return (axisLimits.YMin, axisLimits.YMax);
        }

        #endregion
        #region ===== scale =====

        /// <summary>
        /// locks or unlocks the axis scale
        /// </summary>
        /// <param name="frame"> reference frame </param>
        /// <param name="lockScale"> true: locks scale; false: unlocks </param>
        public static void LockAxisScale(WpfPlot frame, bool lockScale = true)
            => frame.Plot.AxisScaleLock(lockScale);

        #endregion
        #region ===== title =====

        /// <summary>
        /// sets the title of the frame
        /// </summary>
        /// <param name="frame"> frame </param>
        /// <param name="content"> title of the frame </param>
        /// <param name="fontBold"> whether to set the title bold </param>
        /// <param name="fontSize"> font size of the title </param>
        /// <param name="fontFamily"> font family of the title </param>
        /// <param name="fontColor"> color of the title </param>
        public static void SetTitle(WpfPlot frame, string content,
            bool fontBold = true,
            double fontSize = FrameDefaults.TitleSize,
            string fontFamily = FrameDefaults.FontFamily,
            PlotColor fontColor = FrameDefaults.Color)
            => frame.Plot.Title(label: content,
                bold: fontBold,
                size: (float?)fontSize,
                fontName: fontFamily,
                color: ParseColor(fontColor));

        #endregion
        #region ===== labels =====

        /// <summary>
        /// sets the label of the horizontal axis of the frame
        /// </summary>
        /// <param name="frame"> frame </param>
        /// <param name="content"> label of the horizontal axis </param>
        /// <param name="fontBold"> whether to set the label bold </param>
        /// <param name="fontSize"> font size of the label </param>
        /// <param name="fontFamily"> font family of the label </param>
        /// <param name="fontColor"> color of the label </param>
        public static void SetLabelX(WpfPlot frame, string content,
            bool fontBold = false,
            double fontSize = FrameDefaults.AxisLabelSize,
            string fontFamily = FrameDefaults.FontFamily,
            PlotColor fontColor = FrameDefaults.Color)
            => frame.Plot.XAxis.Label(label: content,
                bold: fontBold,
                size: (float?)fontSize,
                fontName: fontFamily,
                color: ParseColor(fontColor));

        /// <summary>
        /// sets the label of the vertical axis of the frame
        /// </summary>
        /// <param name="frame"> frame </param>
        /// <param name="content"> label of the vertical axis </param>
        /// <param name="fontBold"> whether to set the label bold </param>
        /// <param name="fontSize"> font size of the label </param>
        /// <param name="fontFamily"> font family of the label </param>
        /// <param name="fontColor"> color of the label </param>
        public static void SetLabelY(WpfPlot frame, string content,
            bool fontBold = false,
            double fontSize = FrameDefaults.AxisLabelSize,
            string fontFamily = FrameDefaults.FontFamily,
            PlotColor fontColor = FrameDefaults.Color)
            => frame.Plot.YAxis.Label(label: content,
                bold: fontBold,
                size: (float?)fontSize,
                fontName: fontFamily,
                color: ParseColor(fontColor));

        /// <summary>
        /// sets the label of the vertical axis on the right of the frame
        /// </summary>
        /// <param name="frame"> frame </param>
        /// <param name="content"> lable of the vertical axis on the right </param>
        /// <param name="fontBold"> whether to set the label bold </param>
        /// <param name="fontSize"> font size of the label </param>
        /// <param name="fontFamily"> font family of the label </param>
        /// <param name="fontColor"> color of the label </param>
        public static void SetLabelYRight(WpfPlot frame, string content,
            bool fontBold = false,
            double fontSize = FrameDefaults.AxisLabelSize,
            string fontFamily = FrameDefaults.FontFamily,
            PlotColor fontColor = FrameDefaults.Color)
            => frame.Plot.RightAxis.Label(content,
                    bold: fontBold,
                    size: (float?) fontSize,
                    fontName: fontFamily,
                    color: ParseColor(fontColor));

        #endregion
        #region ===== legend =====

        /// <summary>
        /// sets the legend of the frame
        /// </summary>
        /// <param name="frame"> frame </param>
        /// <param name="enable"> whether to enable the legend </param>
        /// <param name="location"> if enabled, set its location </param>
        /// <param name="fontSize"> font size of the legend </param>
        public static void SetLegend(WpfPlot frame,
            bool enable = true,
            LegendLocation location = LegendLocation.LowerRight,
            double fontSize = FrameDefaults.LegendSize)
        {
            ScottPlot.Renderable.Legend? lgd = frame.Plot.Legend(enable, ParseLegendLocation(location));
            lgd.FontSize = (float)fontSize;
        }

        #endregion
        #region ===== save image =====

        /// <summary>
        /// saves the content of the frame into 
        /// .JPG(.JPEG)/.PNG/.TIF/.TIFF/.BMP format
        /// </summary>
        /// <param name="frame"> the frame </param>
        /// <param name="fullPath"> target file path (full) </param>
        /// <param name="width"> resize the plot to this width (pixels) before rendering  </param>
        /// <param name="height"> resize the plot to this height (pixels) before rendering  </param>
        /// <param name="lowQuality"> if true, anti-aliasing will be disabled for this render. Default false </param>
        /// <param name="scale"> scale the size of the output image by this fraction (without resizing the plot) </param>
        public static void SaveFig(WpfPlot frame, String fullPath, int? width = null, int? height = null, bool lowQuality = false, double scale = 1.0)
        {
            int a = width == 0 ? frame.ActualWidth == 0 ? 702 : (int)frame.ActualWidth : width.Value;
            int b = height == 0 ? frame.ActualHeight == 0 ? 510 : (int)frame.ActualHeight : height.Value;
            frame.Plot.SaveFig(fullPath, a, b, lowQuality, scale);
        }

        /// <summary>
        /// gets the actual width of frame
        /// </summary>
        public static int GetActualWidth(WpfPlot frame)
            =>(int)frame.ActualWidth == 0 ? 702: (int)frame.ActualWidth;

        /// <summary>
        /// gets the actual height of frame
        /// </summary>
        public static int GetActualHeight(WpfPlot frame)
            => (int)frame.ActualHeight == 0 ? 510 : (int)frame.ActualHeight;

        #endregion
        #region ===== copy to clipboard =====

        /// <summary>
        /// Copy the picture to the clipboard
        /// </summary>
        /// <param name="frame"> the frame </param>
        /// <param name="lowQuality"> if true, anti-aliasing will be disabled for this render. Default false </param>
        public static void CopyToClipboard(WpfPlot frame,bool lowQuality = false)
        {
            Bitmap imageToAdd = frame.Plot.Render(lowQuality);
            Clipboard.SetDataObject(imageToAdd);
        }

        #endregion
        #region ===== span & lines =====

        /// <summary>
        /// modifies the properties of axis span
        /// </summary>
        /// <param name="s"> the AxisSpan object to modify </param>
        /// <param name="color"> color to fill the span </param>
        /// <param name="opacity"> opacity of the fill color </param>
        /// <param name="isVisible"> whether to show or not </param>
        internal static void SetSpan(AxisSpan s,
            PlotColor color = FrameDefaults.HSpanColor,
            double opacity = FrameDefaults.SpanColorOpacity,
            bool isVisible = true)
        {
            int alpha = (int)(opacity * 255);
            s.Color = ParseColor(alpha, color);
            s.IsVisible = isVisible;    
        }

        /// <summary>
        /// modifies the properties of axis line
        /// </summary>
        /// <param name="l"> the AxisLine object to modify </param>
        /// <param name="color"> color to draw the line </param>
        /// <param name="style"> style of the line </param>
        /// <param name="numFormat"> number format </param>
        /// <param name="numDigits"> number of digits </param>
        /// <param name="isVisible"> whether to show or not </param>
        internal static void SetLine(AxisLine l,
            PlotColor color = FrameDefaults.CrosshairColor,
            LineStyle style = FrameDefaults.CrosshairLineType,
            NumericFormat numFormat = Defaults.NumberFormat,
            int numDigits = Defaults.NumberOfDigits,
            bool isVisible = true)
        {
            l.Color = ParseColor(color);
            l.LineStyle = ParseLineStyle(style);
            l.PositionFormatter = (x) => x.ToString(
                Converter.NumericFormatToString(numFormat)
                + numDigits);
            l.IsVisible = isVisible;
            l.PositionLabel = isVisible;
            l.PositionLabelBackground = ParseColor(color);
        }

        /// <summary>
        /// modifies the properties of the horizontal span
        /// </summary>
        /// <param name="sl"> the HSpanWithLines object to modify </param>
        /// <param name="start"> start of the span </param>
        /// <param name="end"> end of the span </param>
        /// <param name="color"> color to fill the span and to draw the lines </param>
        /// <param name="opacity"> opacity of the fill color </param>
        /// <param name="lineStyle"> style of the lines </param>
        /// <param name="numFormat"> number format </param>
        /// <param name="numDigits"> number of digits </param>
        /// <param name="isVisible"> whether to show or not </param>
        public static void SetHSpanLines(HSpanWithLines sl, 
            double start, double end,
            PlotColor color = FrameDefaults.HSpanColor,
            double opacity = FrameDefaults.SpanColorOpacity,
            LineStyle lineStyle = FrameDefaults.LineType,
            NumericFormat numFormat = Defaults.NumberFormat,
            int numDigits = Defaults.NumberOfDigits,
            bool isVisible = true)
        {
            // positions
            sl.Span.X1 = start;
            sl.Span.X2 = end;            
            sl.LineStart.X = start;
            sl.LineEnd.X = end;
            // color, style, etc
            SetSpan(sl.Span, color, opacity, isVisible);
            SetLine(sl.LineStart, color, lineStyle, numFormat, numDigits, isVisible);
            SetLine(sl.LineEnd, color, lineStyle, numFormat, numDigits, isVisible);
        }

        /// <summary>
        /// modifies the properties of the vertical span
        /// </summary>
        /// <param name="sl"> the VSpanWithLines object to modify </param>
        /// <param name="start"> start of the span </param>
        /// <param name="end"> end of the span </param>
        /// <param name="color"> color to fill the span and to draw the lines </param>
        /// <param name="opacity"> opacity of the fill color </param>
        /// <param name="lineStyle"> style of the lines </param>
        /// <param name="numFormat"> number format </param>
        /// <param name="numDigits"> number of digits </param>
        /// <param name="isVisible"> whether to show or not </param>
        public static void SetVSpanLines(VSpanWithLines sl,
            double start, double end,
            PlotColor color = FrameDefaults.VSpanColor,
            double opacity = FrameDefaults.SpanColorOpacity,
            LineStyle lineStyle = FrameDefaults.LineType,
            NumericFormat numFormat = Defaults.NumberFormat,
            int numDigits = Defaults.NumberOfDigits,
            bool isVisible = true)
        {
            // positions
            sl.Span.Y1 = start;
            sl.Span.Y2 = end;
            sl.LineStart.Y = start;
            sl.LineEnd.Y = end;
            // color, style, etc
            SetSpan(sl.Span, color, opacity, isVisible);
            SetLine(sl.LineStart, color, lineStyle, numFormat, numDigits, isVisible);
            SetLine(sl.LineEnd, color, lineStyle, numFormat, numDigits, isVisible);
        }

        #endregion
        #region===== annotation =====

        /// <summary>
        /// sets the annotation of the frame
        /// </summary>
        /// <param name="annotation"> annotation to be adjusted in the frame </param>
        /// <param name="fontColor"> font color of the annotation </param>
        /// <param name="backgroudColor"> font backgroud of the annotation </param>
        /// <param name="fontSize"> font size of the annotation  </param>
        /// <param name="location"> location of the annotation  </param>
        /// <param name="marginX"> distance (in pixels) from the edge of the plot area to place the annotation</param>
        /// <param name="marginY"> distance (in pixels) from the edge of the plot area to place the annotation </param>
        public static void SetAnnotation(Annotation annotation, 
            PlotColor fontColor, 
            PlotColor backgroudColor, 
            double fontSize, 
            LegendLocation location, 
            double marginX,
            double marginY)
        {
            annotation.Alignment = ParseLegendLocation(location);
            annotation.MarginX = (float)marginX;
            annotation.MarginY = (float)marginY;
            annotation.Font.Size = (float)fontSize;
            annotation.Font.Color = ParseColor(fontColor);
            annotation.BackgroundColor = Color.FromArgb(25, ParseColor(backgroudColor));
            annotation.Shadow = false;
        }

        #endregion
        #region===== crosshair =====

        /// <summary>
        /// the crosshair plot type draws vertical and horizontal lines
        /// </summary>
        /// <param name="mouseTrack"> the Crosshair </param>
        /// <param name="x"> x-axis coordinate (physical) </param>
        /// <param name="y"> y-axis coordinate (physical) </param>
        /// <param name="color"> color to draw the lines </param>
        /// <param name="style"> line style </param>
        /// <param name="numFormat"> number format </param>
        /// <param name="numDigits"> number of digits </param>
        /// <param name="isVisible"> whether to show the crosshair or not </param>
        public static void SetCrosshair(Crosshair mouseTrack, double x, double y,
            PlotColor color = FrameDefaults.CrosshairColor,
            LineStyle style = FrameDefaults.CrosshairLineType,
            NumericFormat numFormat = Defaults.NumberFormat,
            int numDigits = Defaults.NumberOfDigits,
            bool isVisible = true)
        {
            mouseTrack.X = x;
            mouseTrack.Y = y;
            mouseTrack.Color = ParseColor(color);
            mouseTrack.LineStyle = ParseLineStyle(style);
            string numStyle = Converter.NumericFormatToString(numFormat) + numDigits;
            mouseTrack.HorizontalLine.PositionFormatter = (x) => x.ToString(numStyle);
            mouseTrack.VerticalLine.PositionFormatter = (x) => x.ToString(numStyle);
            mouseTrack.IsVisible = isVisible;
        }

        #endregion

        #endregion
        #region ------- plot -------

        #region ===== add plot properties =====

        /// <summary>
        /// generates properties for the GridPlot
        /// </summary>
        /// <param name="lineWidth"> line width </param>
        /// <param name="lineStyle"> line style </param>
        /// <param name="markerSize"> marker size </param>
        /// <param name="markerShape"> marker shape </param>
        /// <param name="plotColor"> plot color </param>
        /// <param name="visualOption"> visual option </param>
        /// <param name="label"> label of the data </param>
        /// <returns> data properties for the GridPlot </returns>
        [Obsolete]
        public static DataPropExpander AddGridPlotProperties(double? lineWidth = null,
            LineStyle? lineStyle = FrameDefaults.LineType,
            double? markerSize = null,
            MarkerShape? markerShape = null,
            PlotColor? plotColor = FrameDefaults.Color,
            VisualOption? visualOption = null,
            string? label = null)
        {
            // generate the expander
            DataPropExpander dataProperties = new()
            {
                IsPlotPartEnabled = false,
                PlotPart = ComplexPart.RealPart,

                // line settings
                LineWidth = lineWidth ?? FrameDefaults.LineWidth,
                LineStyle = ParseLineStyle((LineStyle)lineStyle),
                MarkerSize = markerSize ?? FrameDefaults.MarkerSize,
                MarkerShape = ParseMarkerShape((MarkerShape)markerShape),
                PlotColor = plotColor ?? FrameDefaults.Color,
                VisualOption = visualOption ?? VisualOption.Visible,
                DataLabel = label
            };

            return dataProperties;
        }

        #endregion
        #region ===== add GridPlot =====

        /// <summary>
        /// adds a real-valued grid plot into the frame
        /// </summary>
        /// <param name="frame"> frame </param>
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
        public static SignalPlot AddGridPlot(WpfPlot frame, GridInfo1D grid, VectorD values,
            double lineWidth = FrameDefaults.LineWidth,
            LineStyle lineStyle = FrameDefaults.LineType,
            double markerSize = FrameDefaults.MarkerSize,
            MarkerShape markerShape = FrameDefaults.MarkerType,
            PlotColor? plotColor = null,
            VisualOption visualOption = FrameDefaults.ViewOption,
            string? label = null)
        {
            SignalPlot sgPlot = frame.Plot.AddSignal(VMath.ConvertVectorToArray(values));           

            // sampling settings for grid data
            sgPlot.SamplePeriod = grid.Spacing;
            sgPlot.OffsetX = grid.Start;

            // SignalPlot: line, marker, visual, label, ...
            sgPlot.LineWidth = lineWidth;
            sgPlot.LineStyle = ParseLineStyle(lineStyle);
            // color palette tests ...
            if(plotColor != null)
            {
                sgPlot.LineColor = ParseColor((PlotColor)plotColor);
                //Printer.Write($"plotColor automatically to {sgPlot.LineColor}");
            }
            sgPlot.MarkerSize = (float)markerSize;
            sgPlot.MarkerShape = ParseMarkerShape(markerShape);
            sgPlot.MarkerColor = sgPlot.LineColor; // marker color follows from the line
            sgPlot.IsVisible = visualOption switch
            {
                VisualOption.Hidden => false,
                VisualOption.Visible => true,
                VisualOption.VisibleRight => true,
                _ => true
            };
            sgPlot.YAxisIndex = visualOption switch
            {
                VisualOption.Hidden => frame.Plot.LeftAxis.AxisIndex,
                VisualOption.Visible => frame.Plot.LeftAxis.AxisIndex,
                VisualOption.VisibleRight => frame.Plot.RightAxis.AxisIndex,
                _ => frame.Plot.LeftAxis.AxisIndex
            };
            sgPlot.Label = label;

            return sgPlot;
        }

        #endregion
        #region ===== add ScatPlot =====

        /// <summary>
        /// adds a real-valued scatter plot into the frame
        /// </summary>
        /// <param name="frame"> frame </param>
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
        public static ScatterPlot AddScatPlot(WpfPlot frame, VectorD locations, VectorD values,
            double lineWidth = FrameDefaults.LineWidth,
            LineStyle lineStyle = FrameDefaults.LineType,
            double markerSize = FrameDefaults.MarkerSize,
            MarkerShape markerShape = FrameDefaults.MarkerType,
            PlotColor plotColor = FrameDefaults.Color,
            VisualOption visualOption = FrameDefaults.ViewOption,
            string? label = null)
        {
            ScatterPlot scPlot = frame.Plot.AddScatter(VMath.ConvertVectorToArray(locations),
                VMath.ConvertVectorToArray(values));

            // ScatterPlot: line, marker, visual, label, ...
            scPlot.LineWidth = lineWidth;
            scPlot.LineStyle = ParseLineStyle(lineStyle);
            scPlot.LineColor = ParseColor(plotColor);
            scPlot.MarkerSize = (float)markerSize;
            scPlot.MarkerShape = ParseMarkerShape(markerShape);
            scPlot.MarkerColor = scPlot.LineColor;
            scPlot.IsVisible = visualOption switch
            {
                VisualOption.Visible => true,
                VisualOption.Hidden => false,
                _ => true
            };
            scPlot.Label = label;

            return scPlot;
        }

        #endregion
        #region ===== add FuncPlot =====

        /// <summary>
        /// adds a real-valued function plot into the frame
        /// </summary>
        /// <param name="frame"> frame </param>
        /// <param name="f"> function to plot </param>
        /// <param name="lineWidth"> line width </param>
        /// <param name="lineStyle"> line style </param>
        /// <param name="plotColor"> marker color </param>
        /// <param name="visualOption"> visual option </param>
        /// <param name="label"> label of the data </param>
        /// <returns> resulting plot </returns>
        public static FunctionPlot AddFuncPlot(WpfPlot frame, Func<double, double?> f,
            double lineWidth = FrameDefaults.LineWidth,
            LineStyle lineStyle = FrameDefaults.LineType,
            PlotColor plotColor = FrameDefaults.Color,
            VisualOption visualOption = FrameDefaults.ViewOption,
            string? label = null)
        {
            FunctionPlot fcPlot = frame.Plot.AddFunction(function: f);

            // FunctionPlot: line, marker, visual, label, ...
            fcPlot.LineWidth = lineWidth;
            fcPlot.LineStyle = ParseLineStyle(lineStyle);
            fcPlot.Color = ParseColor(plotColor);
            fcPlot.IsVisible = visualOption switch
            {
                VisualOption.Visible => true,
                VisualOption.Hidden => false,
                _ => true
            };
            fcPlot.Label = label;
            return fcPlot;
        }

        #endregion
        #region ===== add Polygon =====

        /// <summary>
        /// adds a polygon plot into the frame
        /// </summary>
        /// <param name="frame"> frame </param>
        /// <param name="x"> x-components of the polygon corners </param>
        /// <param name="y"> y-components of the polygon corners </param>
        /// <param name="fillColor"> fill color </param>
        /// <param name="lineWidth"> line width </param>
        /// <param name="lineColor"> line color </param>
        /// <param name="visualOption"> visual option </param>
        /// <param name="label"> label of the data </param>
        /// <returns> resulting plot </returns>
        public static Polygon AddPolygon(WpfPlot frame, VectorD x, VectorD y,
            PlotColor fillColor = FrameDefaults.FillColor,
            double lineWidth = FrameDefaults.LineWidth,
            PlotColor lineColor = FrameDefaults.Color,
            VisualOption visualOption = FrameDefaults.ViewOption,
            string? label = null)
        {
            // convert vectors to arrays
            double[] xs = VMath.ConvertVectorToArray(x);
            double[] ys = VMath.ConvertVectorToArray(y);
            Polygon polygon =  frame.Plot.AddPolygon(xs, ys);

            // PolygonPlot: fill, line, visual, label, ...
            polygon.FillColor = ParseColor(fillColor);
            polygon.LineWidth = lineWidth;
            polygon.LineColor = ParseColor(lineColor);
            polygon.IsVisible = visualOption switch
            {
                VisualOption.Visible => true,
                VisualOption.Hidden => false,
                _ => true
            };
            polygon.Label = label;

            return polygon;
        }

        // add polygons ...

        #endregion
        #region ===== add BMPGraph =====

        /// <summary>
        /// adds a bitmap image into the frame 
        /// </summary>
        /// <param name="frame"> frame </param>
        /// <param name="image"> bitmap image </param>
        /// <param name="x"> anchor position x </param>
        /// <param name="y"> anchor position y </param>
        /// <param name="anchor"> anchor alignment </param>
        /// <param name="visualOption"> visual option </param>
        /// <param name="label"> label of the data </param>
        /// <returns> resulting plot </returns>
        public static Image AddBMPGraph(WpfPlot frame, Bitmap image,
            double x = 0.0,
            double y = 0.0,
            Alignment anchor = Alignment.UpperLeft,
            VisualOption visualOption = FrameDefaults.ViewOption,
            string? label = null)
        {
            Image imPlot = frame.Plot.AddImage(image, x, y);
            
            // BMPGraph: anchor alignment ...
            imPlot.Alignment = anchor;
            imPlot.IsVisible = visualOption switch
            {
                VisualOption.Visible => true,
                VisualOption.Hidden => false,
                _ => true
            };
            imPlot.Label = label;

            return imPlot;
        }

        #endregion
        #region ===== add GridGraph =====

        /// <summary>
        /// adds a heat map into the frame
        /// </summary>
        /// <param name="frame"> frame </param>
        /// <param name="grid"> sampling grid information </param>
        /// <param name="values"> data to plot </param>
        /// <param name="colormap"> color map </param>
        /// <param name="smoothMode"> graph smooth mode </param>
        /// <param name="visualOption"> visual option </param>
        /// <param name="label"> label of the data </param>
        /// <returns> resulting plot </returns>
        public static (Heatmap, Colorbar) AddGridGraph(WpfPlot frame, 
            GridInfo2D grid, MatrixD values,
            PlotColormap colormap = FrameDefaults.Colormap,
            GraphInterpolationMode smoothMode = FrameDefaults.SmoothMode,
            //InterpolationMode smoothMode = defaultSmooth,
            VisualOption visualOption = FrameDefaults.ViewOption,
            string? label = null)
        {
            //double[,] valuesData = VMath.ConvertMatrixToArray(values, revRows: true);
            double?[,] valuesData = VMath.ConvertMatrix2Array(values, revRows: true);
            Heatmap htPlot = frame.Plot.AddHeatmap(intensities: valuesData,
                colormap: ParseColormap(colormap));
            Colorbar colorBar = frame.Plot.AddColorbar(htPlot);
            htPlot.Interpolation = ParseGraphInterpolationMode(smoothMode);

            // sampling settings for grid data
            htPlot.CellWidth = grid.SpacingX;
            htPlot.CellHeight = grid.SpacingY;
            htPlot.OffsetX = grid.LowerBoundX;
            htPlot.OffsetY = grid.LowerBoundY;
            // visual option
            htPlot.IsVisible = visualOption switch
            {
                VisualOption.Visible => true,
                VisualOption.Hidden => false,
                _ => true
            };
            htPlot.Label = label;

            return (htPlot, colorBar);
        }

        #endregion
        #region ===== add ScatGraph =====

        /// <summary>
        /// adds a ScatGraph into the frame
        /// </summary>
        /// <param name="frame"> frame </param>
        /// <param name="x"> x-positions of the scattered dots </param>
        /// <param name="y"> y-positions of the scattereed dots </param>
        /// <param name="markerSize"> marker size </param>
        /// <param name="markerShape"> marker shape </param>
        /// <param name="plotColor"> color of markers </param>
        /// <param name="visualOption"> visual option </param>
        /// <param name="label"> label of the data </param>
        /// <returns> resulting plot </returns>
        public static DotsMap AddScatGraph(WpfPlot frame, VectorD x, VectorD y,
            double markerSize = FrameDefaults.MarkerSize,
            MarkerShape markerShape = FrameDefaults.MarkerType,
            PlotColor plotColor = FrameDefaults.Color,
            VisualOption visualOption = FrameDefaults.ViewOption,
            string? label = null)
        {
            DotsMap dots = new (x.Count);
            for(long i = 0; i<x.Count; i++)
            {
                MarkerPlot mkPlot = frame.Plot.AddMarker(x[i], y[i]);
                
                // markerplot: marker size, shape, ...
                mkPlot.MarkerSize = (float)markerSize;
                mkPlot.MarkerShape = ParseMarkerShape(markerShape);
                mkPlot.MarkerColor = ParseColor(plotColor); 
   
                // add marker
                dots.Markers[i] = mkPlot;
            }
            // visual option
            dots.IsVisible = visualOption switch
            {
                VisualOption.Visible => true,
                VisualOption.Hidden => false,
                _ => true
            };
            dots.Label = label;

            return dots;
        }

        /// <summary>
        /// dots map class
        /// composed of MarkerPlot array 
        /// </summary>
        public class DotsMap
        {
            private MarkerPlot[]? markers { get; set; }
            public MarkerPlot[] Markers
            {
                get => markers ?? Array.Empty<MarkerPlot>();
                set => markers = value;
            }

            private bool isVisible { get; set; }
            public bool IsVisible
            {
                get => isVisible;
                set
                {
                    isVisible = value;
                    foreach(MarkerPlot mkPlot in Markers)
                        mkPlot.IsVisible = isVisible;
                }
            }


            private string? label { get; set; }
            public string? Label
            {
                get => label;
                set => label = value;
            }


            public DotsMap(long n)
            {
                Markers = new MarkerPlot[n];
            }

        }

        #endregion
        #region ===== add VectorGraph =====


        public static void AddVectorGraph()
        {
            // ...
        }

        #endregion

        // delete plot at given index
        public static void DeletePlot(WpfPlot frame, int i)
        {
            frame.Plot.RemoveAt(i);                   
        }
        //

        #endregion
        #region ------- update -------

        #region ===== take data part =====

        /// <summary>
        /// takes selected part from a complex vector
        /// </summary>
        /// <param name="values"> complex values in a vector </param>
        /// <param name="targetPart"> desired part of the values </param>
        /// <returns> result in a vector </returns>
        [Obsolete]
        public static VectorD TakePart(VectorZ values,
            ComplexPart? targetPart)
            => targetPart switch
            {
                null => VMath.Abs(values),
                ComplexPart.RealPart => VMath.RealPart(values),
                ComplexPart.ImagPart => VMath.ImagPart(values),
                ComplexPart.Magnitude => VMath.Abs(values),
                ComplexPart.Argument => VMath.Arg(values),
                _ => VMath.Abs(values)
            };

        /// <summary>
        /// takes the selected part from a complex vector
        /// </summary>
        /// <param name="values"> complex values in a vector </param>
        /// <param name="targetPart"> desired part of the values </param>
        /// <returns> result in a real-valued vector </returns>
        public static VectorD TakePart(VectorZ values,
            ComplexPart targetPart)
            => targetPart switch
            {
                ComplexPart.RealPart => VMath.RealPart(values),
                ComplexPart.ImagPart => VMath.ImagPart(values),
                ComplexPart.Magnitude => VMath.Abs(values),
                ComplexPart.Argument => VMath.Arg(values),
                _ => VMath.Abs(values)
            };

        /// <summary>
        /// takes the selected part from a complex matrix
        /// </summary>
        /// <param name="values"> complex values in a matrix </param>
        /// <param name="targetPart"> desired part of the values </param>
        /// <returns> result in a real-valued matrix </returns>
        public static MatrixD TakePart(MatrixZ values,
            ComplexPart targetPart)
            => targetPart switch
            {
                ComplexPart.RealPart => VMath.RealPart(values),
                ComplexPart.ImagPart => VMath.ImagPart(values),
                ComplexPart.Magnitude => VMath.Abs(values),
                ComplexPart.Argument => VMath.Arg(values),
                _ => VMath.Abs(values)
            };

        /// <summary>
        /// takes the selected part from a complex function
        /// </summary>
        /// <param name="f"> complex-valued function </param>
        /// <param name="targetPart"> desired part of the values </param>
        /// <returns> result in a real-valued function </returns>
        public static Func<double, double?> TakePart(Func<double, Complex?> f,
            ComplexPart targetPart)
            => targetPart switch
            {
                ComplexPart.RealPart => new Func<double, double?>((x) => f(x)?.Real),
                ComplexPart.ImagPart => new Func<double, double?>((x) => f(x)?.Imaginary),
                ComplexPart.Magnitude => new Func<double, double?>((x) => f(x)?.Magnitude),
                ComplexPart.Argument => new Func<double, double?>((x) => f(x)?.Phase),
                _ => new Func<double, double?>((x) => f(x)?.Magnitude)
            };

        #endregion
        #region ===== update GridPlot =====

        /// <summary>
        /// updates a GridPlot [SignalPlot]
        /// by changing its line and marker properties
        /// </summary>
        /// <param name="plot"> GridPlot to update </param>
        /// <param name="dataProperties"> property container </param>
        [Obsolete]
        public static void UpdateGridPlot(SignalPlot plot, 
            DataPropExpander dataProperties)
        {
            // case of the plot is set to hidden
            if (dataProperties.VisualOption == VisualOption.Hidden)
            {
                plot.IsVisible= false;
                return;
            }

            // apply changes, if there is any
            plot.IsVisible= true;
            plot.LineWidth = dataProperties.LineWidth;
            plot.LineStyle = dataProperties.LineStyle;
            plot.LineColor = Color.FromName(dataProperties.PlotColor.ToString());
            plot.MarkerSize = (float)dataProperties.MarkerSize;
            plot.MarkerShape = dataProperties.MarkerShape;
            plot.MarkerColor = Color.FromName(dataProperties.PlotColor.ToString());
            plot.Label = dataProperties.DataLabel;
        }

        /// <summary>
        /// updates a GridPlot [SignalPlot]
        /// by changing its line and marker properties
        /// </summary>
        /// <param name="plot"> GridPlot to update </param>
        /// <param name="dataProperties"> property container </param>
        public static void UpdateGridPlot(SignalPlot plot,
            DataPropertyExpander dataProperties)
        {
            // handling visual options
            switch (dataProperties.VisualOption)
            {
                case VisualOption.Hidden:
                    plot.IsVisible = false;
                    return;
                case VisualOption.Visible:
                    plot.IsVisible = true;
                    plot.YAxisIndex = 0; // Plot.LeftAxis.AxisIndex;
                    break;
                case VisualOption.VisibleRight:
                    plot.IsVisible = true;
                    plot.YAxisIndex = 1; // Plot.RightAxis.AxisIndex;
                    break;
                default:
                    plot.IsVisible = true;
                    plot.YAxisIndex = 0; // Plot.LeftAxis.AxisIndex;
                    break;
            }

            // get changes if there is any
            // color palette tests ...
            PlotColor plotColor = dataProperties.PlotColor ?? FrameDefaults.Color;
            // ...
            double lineWidth = dataProperties.LineWidth ?? FrameDefaults.LineWidth;
            LineStyle lineStyle = dataProperties.LineStyle ?? FrameDefaults.LineType;
            double markerSize = dataProperties.MarkerSize ?? FrameDefaults.MarkerSize;
            MarkerShape markerShape = dataProperties.MarkerShape ?? FrameDefaults.MarkerType;
            // apply changes ...
            plot.IsVisible = true;
            plot.LineWidth = lineWidth;
            plot.LineStyle = ParseLineStyle(lineStyle);
            
            plot.LineColor = ParseColor(plotColor);
            
            plot.MarkerSize = (float)markerSize;
            plot.MarkerShape = ParseMarkerShape(markerShape);
            
            plot.MarkerColor = ParseColor(plotColor);
            
            plot.Label = dataProperties.DataLabel;
        }

        /// <summary>
        /// updates the data-part of a GridPlot [SignalPlot]
        /// </summary>
        /// <param name="plot"> GridPlot to update </param>
        /// <param name="values"> complex values of the data </param>
        /// <param name="targetPart"> target complex part of the plot </param>
        public static void UpdateGridPlot(SignalPlot plot,
            VectorZ values, ComplexPart targetPart)
        {
            VectorD valuesPart = TakePart(values, targetPart);
            plot.Update(VMath.ConvertVectorToArray(valuesPart));
        }

        #endregion
        #region ===== update ScatPlot =====

        /// <summary>
        /// updates a ScatPlot [ScatterPlot]
        /// by changing its line and marker properties
        /// </summary>
        /// <param name="plot"> ScatPlot to update </param>
        /// <param name="dataProperties"> property container </param>
        [Obsolete]
        public static void UpdateScatPlot(ScatterPlot plot,
            DataPropExpander dataProperties)
        {
            // case of the plot is set to hidden
            if (dataProperties.VisualOption == VisualOption.Hidden)
            {
                plot.IsVisible = false;
                return;
            }

            // apply changes, if there is any
            plot.IsVisible = true;
            plot.LineWidth = dataProperties.LineWidth;
            plot.LineStyle = dataProperties.LineStyle;
            plot.LineColor = Color.FromName(dataProperties.PlotColor.ToString());
            plot.MarkerSize = (float)dataProperties.MarkerSize;
            plot.MarkerShape = dataProperties.MarkerShape;
            plot.MarkerColor = Color.FromName(dataProperties.PlotColor.ToString());
            plot.Label = dataProperties.DataLabel;
        }

        /// <summary>
        /// updates a ScatPlot [ScatterPlot]
        /// by changing its line and marker properties
        /// </summary>
        /// <param name="plot"> ScatPlot to update </param>
        /// <param name="dataProperties"> property container </param>
        public static void UpdateScatPlot(ScatterPlot plot,
            DataPropertyExpander dataProperties)
        {
            // handling visual options
            switch (dataProperties.VisualOption)
            {
                case VisualOption.Hidden:
                    plot.IsVisible = false;
                    return;
                case VisualOption.Visible:
                    plot.IsVisible = true;
                    plot.YAxisIndex = 0; // Plot.LeftAxis.AxisIndex;
                    break;
                case VisualOption.VisibleRight:
                    plot.IsVisible = true;
                    plot.YAxisIndex = 1; // Plot.RightAxis.AxisIndex;
                    break;
                default:
                    plot.IsVisible = true;
                    plot.YAxisIndex = 0; // Plot.LeftAxis.AxisIndex;
                    break;
            }

            // get changes if there is any
            PlotColor plotColor = dataProperties.PlotColor ?? FrameDefaults.Color;
            double lineWidth = dataProperties.LineWidth ?? FrameDefaults.LineWidth;
            LineStyle lineStyle = dataProperties.LineStyle ?? FrameDefaults.LineType;
            double markerSize = dataProperties.MarkerSize ?? FrameDefaults.MarkerSize;
            MarkerShape markerShape = dataProperties.MarkerShape ?? FrameDefaults.MarkerType;
            // apply changes ...
            plot.IsVisible = true;
            plot.LineWidth = lineWidth;
            plot.LineStyle = ParseLineStyle(lineStyle); 
            plot.LineColor = ParseColor(plotColor);
            plot.MarkerSize = (float)markerSize;
            plot.MarkerShape = ParseMarkerShape(markerShape);
            plot.MarkerColor = ParseColor(plotColor);
            plot.Label = dataProperties.DataLabel;
        }

        /// <summary>
        /// updates the data-part of a ScatPlot [ScatterPlot]
        /// </summary>
        /// <param name="plot"> ScatPlot to update </param>
        /// <param name="values"> complex values of the data </param>
        /// <param name="targetPart"> target complex part of the plot </param>
        public static void UpdateScatPlot(ScatterPlot plot,
            VectorZ values, ComplexPart targetPart)
        {
            VectorD valuesPart = TakePart(values, targetPart);
            plot.UpdateY(VMath.ConvertVectorToArray(valuesPart));
        }

        #endregion
        #region ===== update FuncPlot =====

        /// <summary>
        /// updates a GridPlot [SignalPlot]
        /// by changing its line and marker properties
        /// </summary>
        /// <param name="plot"> GridPlot to update </param>
        /// <param name="dataProperties"> property container </param>
        public static void UpdateFuncPlot(FunctionPlot plot,
            DataPropertyExpander dataProperties)
        {
            // handling visual options
            switch (dataProperties.VisualOption)
            {
                case VisualOption.Hidden:
                    plot.IsVisible = false;
                    return;
                case VisualOption.Visible:
                    plot.IsVisible = true;
                    plot.YAxisIndex = 0; // Plot.LeftAxis.AxisIndex;
                    break;
                case VisualOption.VisibleRight:
                    plot.IsVisible = true;
                    plot.YAxisIndex = 1; // Plot.RightAxis.AxisIndex;
                    break;
                default:
                    plot.IsVisible = true;
                    plot.YAxisIndex = 0; // Plot.LeftAxis.AxisIndex;
                    break;
            }

            // get changes if there is any
            PlotColor plotColor = dataProperties.PlotColor ?? FrameDefaults.Color;
            double lineWidth = dataProperties.LineWidth ?? FrameDefaults.LineWidth;
            LineStyle lineStyle = dataProperties.LineStyle ?? FrameDefaults.LineType;
            // apply changes ...
            plot.IsVisible = true;
            plot.LineWidth = lineWidth;
            plot.LineStyle = ParseLineStyle(lineStyle);
            plot.Color = ParseColor(plotColor);
            plot.Label = dataProperties.DataLabel;
        }


        /// <summary>
        /// updates the data-part of a GridPlot [SignalPlot]
        /// </summary>
        /// <param name="plot"> GridPlot to update </param>
        /// <param name="f"> complex values of the data </param>
        /// <param name="targetPart"> target complex part of the plot </param>
        public static void UpdateFuncPlot(FunctionPlot plot,
            Func<double, Complex?> f, ComplexPart targetPart)
        {
            Func<double, double?> fPart = TakePart(f, targetPart);
            plot.Function = fPart;
        }

        #endregion
        #region ===== update GridGraph =====

        /// <summary>
        /// updates a real-valued GridGraph [Heatmap]
        /// </summary>
        /// <param name="graph"> the GridGraph to update </param>
        /// <param name="colorbar"> the colorbar to update </param>
        /// <param name="dataProperties"> property container </param>
        /// <param name="values"> actual data values to plot </param>
        /// <param name="currentColormap"> current colormap for the graph </param>
        /// <param name="targetColormap"> target colormap for the graph </param>
        public static void UpdateGridGraph(Heatmap graph, 
            Colorbar colorbar,
            DataPropertyExpander dataProperties,
            MatrixD values, 
            Colormap currentColormap,
            PlotColormap targetColormap)
        {
            // case of the plot is set to hidden
            if (dataProperties.VisualOption == VisualOption.Hidden)
            {
                graph.IsVisible = false;
                return;
            }

            // get changes if there is any
            GraphInterpolationMode smoothMode = dataProperties.SmoothMode ?? FrameDefaults.SmoothMode;
            // PlotColormap targetColormap = dataProperties.Colormap ?? defaultColormap;
            // apply changes ...
            graph.Interpolation = ParseGraphInterpolationMode(smoothMode);
            graph.IsVisible = true;
            graph.Label = dataProperties.DataLabel;

            // handling color map
            Colormap tColormap = ParseColormap(targetColormap);
            if(tColormap != currentColormap)
            {
                graph.Update(intensities: VMath.ConvertMatrixToArray(values, revRows: true),
                    colormap: tColormap);
                colorbar.UpdateColormap(tColormap);
            }
        }

        /// <summary>
        /// updates a complex-valued GridGraph [Heatmap]
        /// </summary>
        /// <param name="graph"> the GridGraph to update </param>
        /// <param name="colorbar"> the colorbar to update </param>
        /// <param name="dataProperties"> property container </param>
        /// <param name="values"> actual data values to plot </param>
        /// <param name="currentPart"> current complex part for the graph </param>
        /// <param name="targetPart"> target complex part for the graph </param>
        /// <param name="currentColormap"> current colormap for the graph </param>
        /// <param name="targetColormap"> target colormap for the graph </param>
        public static void UpdateGridGraph(Heatmap graph,
            Colorbar colorbar,
            DataPropertyExpander dataProperties,
            MatrixZ values,
            ComplexPart currentPart,
            ComplexPart targetPart,
            Colormap currentColormap,
            PlotColormap targetColormap)
        {
            // case of the plot is set to hidden
            if (dataProperties.VisualOption == VisualOption.Hidden)
            {
                graph.IsVisible = false;
                return;
            }

            // get changes if there is any
            GraphInterpolationMode smoothMode = dataProperties.SmoothMode ?? FrameDefaults.SmoothMode;
            // PlotColormap targetColormap = dataProperties.Colormap ?? defaultColormap;
            // apply changes ...
            graph.Interpolation = ParseGraphInterpolationMode(smoothMode);
            graph.IsVisible = true;
            graph.Label = dataProperties.DataLabel;

            // handling ComplexPart and/or Colormap
            Colormap tColormap = ParseColormap(targetColormap);
            if(currentPart != targetPart || currentColormap != tColormap)
            {
                MatrixD valuesPart = TakePart(values, targetPart);
                graph.Update(intensities: VMath.ConvertMatrix2Array(valuesPart, revRows: true), //VMath.ConvertMatrixToArray(valuesPart, revRows: true),
                    colormap: tColormap);
                colorbar.UpdateColormap(tColormap);
            }
        }

        #endregion

        #endregion
        #region cursor and axis limit info

        // ...

        #endregion

    }
}
