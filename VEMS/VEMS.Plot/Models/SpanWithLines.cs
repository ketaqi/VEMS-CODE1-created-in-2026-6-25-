using ScottPlot;
using ScottPlot.Plottable;
using VEMS.MathCore;
using static VEMS.Plot.Options;
using LineStyle = VEMS.Plot.Options.LineStyle;

namespace VEMS.Plot
{

    /// <summary>
    /// horizontal span with lines at both sides
    /// </summary>
    public class HSpanWithLines //: SpanWithLines
    {
        #region properties

        /// <summary>
        /// the span
        /// </summary>
        public HSpan Span { get; set; }
        
        /// <summary>
        /// line at the start position
        /// </summary>
        public VLine LineStart { get; set; }

        /// <summary>
        /// line at the end position
        /// </summary>
        public VLine LineEnd { get; set; }

        #endregion
        #region constructor

        /// <summary>
        /// initializes a HSpanWithLine object
        /// for a given WpfPlot
        /// </summary>
        /// <param name="wp"> WpfPlot </param>
        /// <param name="isVisible"> whether to show or not </param>
        public HSpanWithLines(WpfPlot wp, bool isVisible = false)
        {
            Span = wp.Plot.AddHorizontalSpan(0.0, 0.0);
            LineStart = wp.Plot.AddVerticalLine(0.0);
            LineEnd = wp.Plot.AddVerticalLine(0.0);

            Span.IsVisible = isVisible;
            LineStart.IsVisible = isVisible;
            LineEnd.IsVisible = isVisible;
        }

        #endregion
        #region methods

        /// <summary>
        /// modifies the properties
        /// </summary>
        /// <param name="start"> start of the span </param>
        /// <param name="end"> end of the span </param>
        /// <param name="color"> color to fill the span and to draw the lines </param>
        /// <param name="opacity"> opacity of the fill color </param>
        /// <param name="lineStyle"> style of the lines </param>
        /// <param name="numFormat"> number format </param>
        /// <param name="numDigits"> number of digits </param>
        /// <param name="isVisible"> whether to show or not </param>
        public void SetProperties(double start, double end,
            PlotColor color = FrameDefaults.HSpanColor,
            double opacity = FrameDefaults.SpanColorOpacity,
            LineStyle lineStyle = FrameDefaults.LineType,
            NumericFormat numFormat = Defaults.NumberFormat,
            int numDigits = Defaults.NumberOfDigits,
            bool isVisible = true)
            => FrameCommons.SetHSpanLines(this, start, end, color, opacity, 
                lineStyle, numFormat, numDigits, isVisible);

        #endregion
    }

    /// <summary>
    /// vertical span with lines at both sides
    /// </summary>
    public class VSpanWithLines
    {
        #region properties

        /// <summary>
        /// the span
        /// </summary>
        public VSpan Span { get; set; }
        
        /// <summary>
        /// line at the start position
        /// </summary>
        public HLine LineStart { get; set; }
        
        /// <summary>
        /// line at the end position
        /// </summary>
        public HLine LineEnd { get; set; }

        #endregion
        #region constructor

        /// <summary>
        /// initializes a VSpanWithLine object
        /// for a given WpfPlot
        /// </summary>
        /// <param name="wp"> WpfPlot </param>
        /// <param name="isVisible"> whether to show or not </param>
        public VSpanWithLines(WpfPlot wp, bool isVisible = false)
        {
            Span = wp.Plot.AddVerticalSpan(0.0, 0.0);
            LineStart = wp.Plot.AddHorizontalLine(0.0);
            LineEnd = wp.Plot.AddHorizontalLine(0.0);

            Span.IsVisible = isVisible;
            LineStart.IsVisible = isVisible;
            LineEnd.IsVisible = isVisible;
        }

        #endregion
        #region methods

        /// <summary>
        /// modifies the properties
        /// </summary>
        /// <param name="start"> start of the span </param>
        /// <param name="end"> end of the span </param>
        /// <param name="color"> color to fill the span and to draw the lines </param>
        /// <param name="opacity"> opacity of the fill color </param>
        /// <param name="lineStyle"> style of the lines </param>
        /// <param name="numFormat"> number format </param>
        /// <param name="numDigits"> number of digits </param>
        /// <param name="isVisible"> whether to show or not </param>
        public void SetProperties(double start, double end,
            PlotColor color = FrameDefaults.VSpanColor,
            double opacity = FrameDefaults.SpanColorOpacity,
            LineStyle lineStyle = FrameDefaults.LineType,
            NumericFormat numFormat = Defaults.NumberFormat,
            int numDigits = Defaults.NumberOfDigits,
            bool isVisible = true)
            => FrameCommons.SetVSpanLines(this, start, end, color, opacity, 
                lineStyle, numFormat, numDigits, isVisible);

        #endregion
    }





}
