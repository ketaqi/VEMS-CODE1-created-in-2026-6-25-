using VEMS.MathCore;

namespace VEMS.Plot5
{
    /// <summary>
    /// default plot parameters
    /// </summary>
    public class Defaults
    {
        #region plot line and marker

        /// <summary>
        /// width of line plot
        /// </summary>
        public const double LineWidth = 4.0;

        /// <summary>
        /// style of line plot
        /// </summary>
        [Obsolete("ScottPlot4")]
        public const LineStyle LineType = LineStyle.Solid;

        /// <summary>
        /// pattern of the line
        /// </summary>
        public const LinePattern Pattern = LinePattern.Solid;

        /// <summary>
        /// size of marker
        /// </summary>
        public const double MarkerSize = 7.0;

        /// <summary>
        /// shape of marker
        /// </summary>
        [Obsolete("ScottPlot4")]
        public const MarkerShape MarkerType = MarkerShape.FilledCircle;

        /// <summary>
        /// shape of the marker
        /// </summary>
        public const MarkerShape Shape = MarkerShape.FilledCircle;

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
        [Obsolete("ScottPlot4")]
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
        public const DisplayOption DispOption = DisplayOption.Left;

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
        public static string defaultNumDigits = Converter.NumericFormatToString(MathCore.Defaults.NumberFormat)
            + MathCore.Defaults.NumberOfDigits;

        #endregion
        #region complex-part

        /// <summary>
        /// complex-part option
        /// </summary>
        public const ComplexPart CplxPart = ComplexPart.RealPart;

        #endregion
    }

    /// <summary>
    /// plot color options
    /// </summary>
    public enum PlotColor
    {
        /// <summary>
        /// 
        /// </summary>
        Transparent,

        /// <summary>
        /// 
        /// </summary>
        AliceBlue,

        /// <summary>
        /// 
        /// </summary>
        AntiqueWhite,

        /// <summary>
        /// 
        /// </summary>
        Aqua,

        /// <summary>
        /// 
        /// </summary>
        Aquamarine,

        /// <summary>
        /// 
        /// </summary>
        Azure,

        /// <summary>
        /// 
        /// </summary>
        Beige,

        /// <summary>
        /// 
        /// </summary>
        Bisque,

        /// <summary>
        /// 
        /// </summary>
        Black,

        /// <summary>
        /// 
        /// </summary>
        BalanchedAlmond,

        /// <summary>
        /// 
        /// </summary>
        Blue,

        /// <summary>
        /// 
        /// </summary>
        BlueViolet,

        /// <summary>
        /// 
        /// </summary>
        Brown,

        /// <summary>
        /// 
        /// </summary>
        BurlyWood,

        /// <summary>
        /// 
        /// </summary>
        CadeBlue,

        /// <summary>
        /// 
        /// </summary>
        Chartreuse,

        /// <summary>
        /// 
        /// </summary>
        Chocolate,

        /// <summary>
        /// 
        /// </summary>
        Coral,

        /// <summary>
        /// 
        /// </summary>
        CornflowerBlue,

        /// <summary>
        /// 
        /// </summary>
        Comsilk,

        /// <summary>
        /// 
        /// </summary>
        Crimson,

        /// <summary>
        /// 
        /// </summary>
        Cyan,

        /// <summary>
        /// 
        /// </summary>
        DarkBlue,

        /// <summary>
        /// 
        /// </summary>
        DarkCyan,

        /// <summary>
        /// 
        /// </summary>
        DarkGoldenrod,

        /// <summary>
        /// 
        /// </summary>
        DarkGray,

        /// <summary>
        /// 
        /// </summary>
        DarkGreen,

        /// <summary>
        /// 
        /// </summary>
        DarkKhaki,

        /// <summary>
        /// 
        /// </summary>
        DarkMagenta,

        /// <summary>
        /// 
        /// </summary>
        DarkOliveGreen,

        /// <summary>
        /// 
        /// </summary>
        DarkOrange,

        /// <summary>
        /// 
        /// </summary>
        DarkOrchid,

        /// <summary>
        /// 
        /// </summary>
        DarkRed,

        /// <summary>
        /// 
        /// </summary>
        DarkSalmon,

        /// <summary>
        /// 
        /// </summary>
        DarkSeaGreen,

        /// <summary>
        /// 
        /// </summary>
        DarkSlateBlue,

        /// <summary>
        /// 
        /// </summary>
        DarkSlateGray,

        /// <summary>
        /// 
        /// </summary>
        DarkTurquoise,

        /// <summary>
        /// 
        /// </summary>
        DarkViolet,

        /// <summary>
        /// 
        /// </summary>
        DeepPink,

        /// <summary>
        /// 
        /// </summary>
        DeepSkyBlue,

        /// <summary>
        /// 
        /// </summary>
        DimGray,

        /// <summary>
        /// 
        /// </summary>
        DodgeBlue,

        /// <summary>
        /// 
        /// </summary>
        Firebrick,

        /// <summary>
        /// 
        /// </summary>
        FloralWhite,

        /// <summary>
        /// 
        /// </summary>
        ForestGreen,

        /// <summary>
        /// 
        /// </summary>
        Fuchsia,

        /// <summary>
        /// 
        /// </summary>
        Gainsboro,

        /// <summary>
        /// 
        /// </summary>
        GhostWhite,

        /// <summary>
        /// 
        /// </summary>
        Gold,

        /// <summary>
        /// 
        /// </summary>
        Goldenrod,

        /// <summary>
        /// 
        /// </summary>
        Gray,

        /// <summary>
        /// 
        /// </summary>
        Green,

        /// <summary>
        /// 
        /// </summary>
        GreenYellow,

        /// <summary>
        /// 
        /// </summary>
        Honeydew,

        /// <summary>
        /// 
        /// </summary>
        HotPink,

        /// <summary>
        /// 
        /// </summary>
        IndianRed,

        /// <summary>
        /// 
        /// </summary>
        Indigo,

        /// <summary>
        /// 
        /// </summary>
        Ivory,

        /// <summary>
        /// 
        /// </summary>
        Khahi,

        /// <summary>
        /// 
        /// </summary>
        Lavender,

        /// <summary>
        /// 
        /// </summary>
        LavenderBlush,

        /// <summary>
        /// 
        /// </summary>
        LawnGreen,

        /// <summary>
        /// 
        /// </summary>
        LemonChiffon,

        /// <summary>
        /// 
        /// </summary>
        LightBlue,

        /// <summary>
        /// 
        /// </summary>
        LightCoral,

        /// <summary>
        /// 
        /// </summary>
        LightCyan,

        /// <summary>
        /// 
        /// </summary>
        LightGoldenrodYellow,
        
        /// <summary>
        /// 
        /// </summary>
        LightGray,
        
        /// <summary>
        /// 
        /// </summary>
        LightGreen,
        
        /// <summary>
        /// 
        /// </summary>
        LightPink,
        
        /// <summary>
        /// 
        /// </summary>
        LightSalmon,
        
        /// <summary>
        /// 
        /// </summary>
        LightSeaGreen,
        
        /// <summary>
        /// 
        /// </summary>
        LightSkyBlue,
        
        /// <summary>
        /// 
        /// </summary>
        LightSlateGray,
        
        /// <summary>
        /// 
        /// </summary>
        LightYellow,
        
        /// <summary>
        /// 
        /// </summary>
        Lime,
        
        /// <summary>
        /// 
        /// </summary>
        LimeGreen,
        
        /// <summary>
        /// 
        /// </summary>
        Linen,
        
        /// <summary>
        /// 
        /// </summary>
        Magenta,
        
        /// <summary>
        /// 
        /// </summary>
        Maroon,
        
        /// <summary>
        /// 
        /// </summary>
        MediumAquamarine,
        
        /// <summary>
        /// 
        /// </summary>
        MediumBlue,
        
        /// <summary>
        /// 
        /// </summary>
        MediumOrchid,
        
        /// <summary>
        /// 
        /// </summary>
        MediumPurple,
        
        /// <summary>
        /// 
        /// </summary>
        MedimSeaGreen,
        
        /// <summary>
        /// 
        /// </summary>
        MediumSlateBlue,
        
        /// <summary>
        /// 
        /// </summary>
        MediumSpringGreen,
        
        /// <summary>
        /// 
        /// </summary>
        MediumTurquoise,
        
        /// <summary>
        /// 
        /// </summary>
        MediumVioletRed,
        
        /// <summary>
        /// 
        /// </summary>
        MidnightBlue,
        
        /// <summary>
        /// 
        /// </summary>
        MintCream,
        
        /// <summary>
        /// 
        /// </summary>
        MistyRose,
        
        /// <summary>
        /// 
        /// </summary>
        Moccasin,
        
        /// <summary>
        /// 
        /// </summary>
        NavajoWhite,
        
        /// <summary>
        /// 
        /// </summary>
        Navy,
        
        /// <summary>
        /// 
        /// </summary>
        OldLace,
        
        /// <summary>
        /// 
        /// </summary>
        Olive,
        
        /// <summary>
        /// 
        /// </summary>
        OliveDrab,
        
        /// <summary>
        /// 
        /// </summary>
        Orange,
        
        /// <summary>
        /// 
        /// </summary>
        OrangeRed,
        
        /// <summary>
        /// 
        /// </summary>
        Orchid,
        
        /// <summary>
        /// 
        /// </summary>
        PaleGoldenrod,
        
        /// <summary>
        /// 
        /// </summary>
        PaleGreen,
        
        /// <summary>
        /// 
        /// </summary>
        PaleTurquoise,
        
        /// <summary>
        /// 
        /// </summary>
        PaleVioletRed,
        
        /// <summary>
        /// 
        /// </summary>
        PapayaWhip,
        
        /// <summary>
        /// 
        /// </summary>
        PeachPuff,
        
        /// <summary>
        /// 
        /// </summary>
        Peru,
        
        /// <summary>
        /// 
        /// </summary>
        Pink,
        
        /// <summary>
        /// 
        /// </summary>
        Plum,
        
        /// <summary>
        /// 
        /// </summary>
        PowderBlue,
        
        /// <summary>
        /// 
        /// </summary>
        Purple,
        
        /// <summary>
        /// 
        /// </summary>
        Red,
        
        /// <summary>
        /// 
        /// </summary>
        RosyBrown,
        
        /// <summary>
        /// 
        /// </summary>
        RoyalBlue,
        
        /// <summary>
        /// 
        /// </summary>
        SaddleBrown,
        
        /// <summary>
        /// 
        /// </summary>
        Salmon,
        
        /// <summary>
        /// 
        /// </summary>
        SandyBrown,
        
        /// <summary>
        /// 
        /// </summary>
        SeaGreen,
        
        /// <summary>
        /// 
        /// </summary>
        SeaShell,
        
        /// <summary>
        /// 
        /// </summary>
        Sienna,
        
        /// <summary>
        /// 
        /// </summary>
        Silver,
        
        /// <summary>
        /// 
        /// </summary>
        SkyBlue,
        
        /// <summary>
        /// 
        /// </summary>
        SlateBlue,
        
        /// <summary>
        /// 
        /// </summary>
        SlateGray,
        
        /// <summary>
        /// 
        /// </summary>
        Snow,
        
        /// <summary>
        /// 
        /// </summary>
        SpringGreen,
        
        /// <summary>
        /// 
        /// </summary>
        SteelBlue,
        
        /// <summary>
        /// 
        /// </summary>
        Tan,
        
        /// <summary>
        /// 
        /// </summary>
        Teal,
        
        /// <summary>
        /// 
        /// </summary>
        Thistle,
        
        /// <summary>
        /// 
        /// </summary>
        Tomato,
        
        /// <summary>
        /// 
        /// </summary>
        Torquoise,
        
        /// <summary>
        /// 
        /// </summary>
        Violet,
        
        /// <summary>
        /// 
        /// </summary>
        Wheat,
        
        /// <summary>
        /// 
        /// </summary>
        White,
        
        /// <summary>
        /// 
        /// </summary>
        WhiteSmoke,
        
        /// <summary>
        /// 
        /// </summary>
        Yellow,
        
        /// <summary>
        /// 
        /// </summary>
        YellowGreen

    }

    /// <summary>
    /// plot colormap options
    /// </summary>
    public enum PlotColormap
    {
        /// <summary>
        /// 
        /// </summary>
        Algae,
        
        /// <summary>
        /// 
        /// </summary>
        Amp,

        /// <summary>
        /// 
        /// </summary>
        Balance,
        
        /// <summary>
        /// 
        /// </summary>
        Blues,
        
        /// <summary>
        /// 
        /// </summary>
        Curl,
        
        /// <summary>
        /// 
        /// </summary>
        Deep,
        
        /// <summary>
        /// 
        /// </summary>
        Delta,
        
        /// <summary>
        /// 
        /// </summary>
        Dense,
        
        /// <summary>
        /// 
        /// </summary>
        Diff,
        
        /// <summary>
        /// 
        /// </summary>
        Grayscale,
        
        /// <summary>
        /// 
        /// </summary>
        GrayscaleR,
        
        /// <summary>
        /// 
        /// </summary>
        Greens,
        
        /// <summary>
        /// 
        /// </summary>
        Haline,
        
        /// <summary>
        /// 
        /// </summary>
        Ice,
        
        /// <summary>
        /// 
        /// </summary>
        Inferno,
        
        /// <summary>
        /// 
        /// </summary>
        Jet,
        
        /// <summary>
        /// 
        /// </summary>
        Magma,
        
        /// <summary>
        /// 
        /// </summary>
        Matter,
        
        /// <summary>
        /// 
        /// </summary>
        Oxy,
        
        /// <summary>
        /// 
        /// </summary>
        Phase,
        
        /// <summary>
        /// 
        /// </summary>
        Plasma,
        
        /// <summary>
        /// 
        /// </summary>
        Rain,
        
        /// <summary>
        /// 
        /// </summary>
        Solar,
        
        /// <summary>
        /// 
        /// </summary>
        Speed,
        
        /// <summary>
        /// 
        /// </summary>
        Tarn,
        
        /// <summary>
        /// 
        /// </summary>
        Tempo,
        
        /// <summary>
        /// 
        /// </summary>
        Thermal,
        
        /// <summary>
        /// 
        /// </summary>
        Topo,
        
        /// <summary>
        /// 
        /// </summary>
        Turbid,
        
        /// <summary>
        /// 
        /// </summary>
        Turbo,
        
        /// <summary>
        /// 
        /// </summary>
        Viridis
    }

    /// <summary>
    /// plot line style options
    /// </summary>
    [Obsolete("ScottPlot4")]
    public enum LineStyle
    {
        /// <summary>
        /// 
        /// </summary>
        None,
        
        /// <summary>
        /// 
        /// </summary>
        Solid,
        
        /// <summary>
        /// 
        /// </summary>
        Dash,
        
        /// <summary>
        /// 
        /// </summary>
        DashDot,
        
        /// <summary>
        /// 
        /// </summary>
        DashDotDot,
        
        /// <summary>
        /// 
        /// </summary>
        Dot
    }

    /// <summary>
    /// LineStyel.Pattern options
    /// </summary>
    public enum LinePattern
    {
        /// <summary>
        /// 
        /// </summary>
        Solid,

        /// <summary>
        /// 
        /// </summary>
        Dashed,

        /// <summary>
        /// 
        /// </summary>
        DenselyDashed,
        
        /// <summary>
        /// 
        /// </summary>
        Dotted
    }

    /// <summary>
    /// plot marker shape options
    /// </summary>
    public enum MarkerShape
    {
        /// <summary>
        /// 
        /// </summary>
        None,

        /// <summary>
        /// 
        /// </summary>
        FilledCircle,
        
        /// <summary>
        /// 
        /// </summary>
        FilledSquare,
        
        /// <summary>
        /// 
        /// </summary>
        OpenCircle,
        
        /// <summary>
        /// 
        /// </summary>
        OpenSquare,
        
        /// <summary>
        /// 
        /// </summary>
        FilledDiamond,
        
        /// <summary>
        /// 
        /// </summary>
        OpenDiamond,
        
        /// <summary>
        /// 
        /// </summary>
        Asterisk,
        
        /// <summary>
        /// 
        /// </summary>
        HashTag,
        
        /// <summary>
        /// 
        /// </summary>
        Cross,
        
        /// <summary>
        /// 
        /// </summary>
        Eks,
        
        /// <summary>
        /// 
        /// </summary>
        VerticalBar,
        
        /// <summary>
        /// 
        /// </summary>
        TriUp,
        
        /// <summary>
        /// 
        /// </summary>
        TriDown,
        
        /// <summary>
        /// 
        /// </summary>
        FilledTriangleUp,
        
        /// <summary>
        /// 
        /// </summary>
        FilledTriangleDown,
        
        /// <summary>
        /// 
        /// </summary>
        OpenTriangleUp,
        
        /// <summary>
        /// 
        /// </summary>
        OpenTriangleDown
    }

    /// <summary>
    /// plot display options
    /// </summary>
    public enum DisplayOption
    {
        /// <summary>
        /// display on the left 
        /// </summary>
        Left = 0,

        /// <summary>
        /// hidden 
        /// </summary>
        Hidden = 1,
        
        /// <summary>
        /// display on the right
        /// </summary>
        Right = 2
    }

    /// <summary>
    /// legend location options
    /// </summary>
    public enum LegendLocation
    {
        /// <summary>
        /// 
        /// </summary>
        UpperLeft,
        
        /// <summary>
        /// 
        /// </summary>
        UpperRight,
        
        /// <summary>
        /// 
        /// </summary>
        UpperCenter,
        
        /// <summary>
        /// 
        /// </summary>
        MiddleLeft,
        
        /// <summary>
        /// 
        /// </summary>
        MiddleCenter,
        
        /// <summary>
        /// 
        /// </summary>
        MiddleRight,
        
        /// <summary>
        /// 
        /// </summary>
        LowerLeft,
        
        /// <summary>
        /// 
        /// </summary>
        LowerRight,
        
        /// <summary>
        /// 
        /// </summary>
        LowerCenter
    }

    /// <summary>
    /// interpolation option of 2D graph
    /// </summary>
    public enum GraphInterpolationMode
    {
        /// <summary>
        /// default mode
        /// </summary>
        Default = 0,

        /// <summary>
        /// low quality interpolation
        /// </summary>
        Low = 1,

        /// <summary>
        /// high quality interpolation
        /// </summary>
        High = 2,

        /// <summary>
        /// bilinear interpolation. No pre-filtering is done. 
        /// This mode is not suitable for shrinking an image 
        /// below 50 percent of its original size.
        /// </summary>
        Bilinear = 3,

        /// <summary>
        /// bicubic interpolation. No pre-filtering is done. 
        /// This mode is not suitable for shrinking an image 
        /// below 25 percent of its original size.
        /// </summary>
        Bicubic = 4,

        /// <summary>
        /// nearest-neighbor interpolation
        /// </summary>
        NearestNeighbor = 5,

        /// <summary>
        /// high-quality, bilinear interpolation. Pre-filtering 
        /// is performed to ensure high-quality shrinking.
        /// </summary>
        HighQualityBilinear = 6,

        /// <summary>
        /// high-quality, bicubic interpolation. Pre-filtering 
        /// is performed to ensure high-quality shrinking. 
        /// This mode produces the highest quality transformed images.
        /// </summary>
        HighQualityBicubic = 7
    }


}
