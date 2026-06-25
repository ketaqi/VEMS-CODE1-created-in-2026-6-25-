using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static VEMS.Plot.Options;
using VEMS.MathCore;

namespace VEMS.Plot
{
    /// <summary>
    /// default parameters for frame
    /// </summary>
    public class FrameDefaults
    {

        #region plot line and marker

        /// <summary>
        /// width of line plot
        /// </summary>
        public const double LineWidth = 4.0;

        /// <summary>
        /// style of line plot
        /// </summary>
        public const LineStyle LineType = LineStyle.Solid;

        /// <summary>
        /// size of marker
        /// </summary>
        public const double MarkerSize = 7.0;

        /// <summary>
        /// shape of marker
        /// </summary>
        public const MarkerShape MarkerType = MarkerShape.filledCircle;

        #endregion
        #region span and crosshair

        /// <summary>
        /// color to fill horizontal span
        /// </summary>
        public const PlotColor HSpanColor = PlotColor.SteelBlue; //.Red;

        /// <summary>
        /// color to fill vertical span
        /// </summary>
        public const PlotColor VSpanColor = PlotColor.Teal;  //.Purple;

        /// <summary>
        /// opacity of span color
        /// </summary>
        public const double SpanColorOpacity = 0.20;

        /// <summary>
        /// style of crosshair lines
        /// </summary>
        public const LineStyle CrosshairLineType = LineStyle.Dash;

        /// <summary>
        /// color of crosshair lines
        /// </summary>
        public const PlotColor CrosshairColor = PlotColor.SteelBlue;

        #endregion
        #region graph options

        /// <summary>
        /// colormap for 2D graph
        /// </summary>
        public const PlotColormap Colormap = PlotColormap.Grayscale;

        /// <summary>
        /// smoothing mode for 2D graph
        /// </summary>
        public const GraphInterpolationMode SmoothMode = GraphInterpolationMode.NearestNeighbor;

        #endregion
        #region visual options

        /// <summary>
        /// visual option
        /// </summary>
        public const VisualOption ViewOption = VisualOption.Visible;

        /// <summary>
        /// color of line and marke plots
        /// </summary>
        public const PlotColor Color = PlotColor.Black;

        /// <summary>
        /// color to fill span
        /// </summary>
        public const PlotColor FillColor = PlotColor.LightGray;

        /// <summary>
        /// label text of plot
        /// </summary>
        public const string PlotLabel = "";

        /// <summary>
        /// view range start along x
        /// </summary>
        public const double XMin = -2.0;

        /// <summary>
        /// view range end along x
        /// </summary>
        public const double XMax = 2.0;

        /// <summary>
        /// view range start along y
        /// </summary>
        public const double YMin = -0.5;

        /// <summary>
        /// view range end along y
        /// </summary>
        public const double YMax = 3.5;

        // axis ticks

        /// <summary>
        /// axis tick density
        /// </summary>
        public const double TickDensity = 1.0;
        //public const double defaultYTickDensity = 1.0;

        /// <summary>
        /// axis tick spacing
        /// </summary>
        public const double TickSpacing = 1.0;
        
        /// <summary>
        /// font size of axis ticks
        /// </summary>
        public const double TickSize = 16.0;
        //public const double defaultYTickSize = 12.0;
        //public const string defaultYTickFontFamily = "";

        // figure texts
        /// <summary>
        /// title of the frame
        /// </summary>
        public const string Title = "VEMS Plot(s)";
        
        /// <summary>
        /// label of the frame along x
        /// </summary>
        public const string LabelX = "X";
        
        /// <summary>
        /// label of the frame along y
        /// </summary>
        public const string LabelY = "Y";

        /// <summary>
        /// font size of the title
        /// </summary>
        public const double TitleSize = 20.0;
        
        /// <summary>
        /// font size of the axis labels
        /// </summary>
        public const double AxisLabelSize = 18.0;
        //public const double defaultLabelYSize = 18.0;
        
        /// <summary>
        /// font size of ...
        /// </summary>
        public const string FontFamily = "Calibri";

        // figure legend
        /// <summary>
        /// font size of the legend
        /// </summary>
        public const double LegendSize = 16.0;

        // number format
        /// <summary>
        /// display format of numbers
        /// </summary>
        public static string defaultNumDigits = Converter.NumericFormatToString(Defaults.NumberFormat)
            + Defaults.NumberOfDigits;

        #endregion

    }
}
