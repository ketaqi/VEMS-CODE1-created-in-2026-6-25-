using ScottPlot;
using ScottPlot.Drawing.Colormaps;
using ScottPlot.MarkerShapes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using static VEMS.Plot.Options;

namespace VEMS.Plot
{
    public class Options
    {

        /// <summary>
        /// plot color options
        /// </summary>
        public enum PlotColor
        {
            Transparent,
            AliceBlue,
            AntiqueWhite,
            Aqua,
            Aquamarine,
            Azure,
            Beige,
            Bisque,
            Black,
            BalanchedAlmond,
            Blue,
            BlueViolet,
            Brown,
            BurlyWood,
            CadeBlue,
            Chartreuse,
            Chocolate,
            Coral,
            CornflowerBlue,
            Comsilk,
            Crimson,
            Cyan,
            DarkBlue,
            DarkCyan,
            DarkGoldenrod,
            DarkGray,
            DarkGreen,
            DarkKhaki,
            DarkMagenta,
            DarkOliveGreen,
            DarkOrange,
            DarkOrchid,
            DarkRed,
            DarkSalmon,
            DarkSeaGreen,
            DarkSlateBlue,
            DarkSlateGray,
            DarkTurquoise,
            DarkViolet,
            DeepPink,
            DeepSkyBlue,
            DimGray,
            DodgeBlue,
            Firebrick,
            FloralWhite,
            ForestGreen,
            Fuchsia,
            Gainsboro,
            GhostWhite,
            Gold,
            Goldenrod,
            Gray,
            Green,
            GreenYellow,
            Honeydew,
            HotPink,
            IndianRed,
            Indigo,
            Ivory,
            Khahi,
            Lavender,
            LavenderBlush,
            LawnGreen,
            LemonChiffon,
            LightBlue,
            LightCoral,
            LightCyan,
            LightGoldenrodYellow,
            LightGray,
            LightGreen,
            LightPink,
            LightSalmon,
            LightSeaGreen,
            LightSkyBlue,
            LightSlateGray,
            LightYellow,
            Lime,
            LimeGreen,
            Linen,
            Magenta,
            Maroon,
            MediumAquamarine,
            MediumBlue,
            MediumOrchid,
            MediumPurple,
            MedimSeaGreen,
            MediumSlateBlue,
            MediumSpringGreen,
            MediumTurquoise,
            MediumVioletRed,
            MidnightBlue,
            MintCream,
            MistyRose,
            Moccasin,
            NavajoWhite,
            Navy,
            OldLace,
            Olive,
            OliveDrab,
            Orange,
            OrangeRed,
            Orchid,
            PaleGoldenrod,
            PaleGreen,
            PaleTurquoise,
            PaleVioletRed,
            PapayaWhip,
            PeachPuff,
            Peru,
            Pink,
            Plum,
            PowderBlue,
            Purple,
            Red,
            RosyBrown,
            RoyalBlue,
            SaddleBrown,
            Salmon,
            SandyBrown,
            SeaGreen,
            SeaShell,
            Sienna,
            Silver,
            SkyBlue,
            SlateBlue,
            SlateGray,
            Snow,
            SpringGreen,
            SteelBlue,
            Tan,
            Teal,
            Thistle,
            Tomato,
            Torquoise,
            Violet,
            Wheat,
            White,
            WhiteSmoke,
            Yellow,
            YellowGreen

        }

        /// <summary>
        /// plot colormap options
        /// </summary>
        public enum PlotColormap
        {
            Colormap,
            Amp,
            Balance,
            Blues,
            Curl,
            Deep,
            Delta,
            Dense,
            Diff,
            Grayscale,
            GrayscaleR,
            Greens,
            Haline,
            Ice,
            Inferno,
            Jet,
            Magma,
            Matter,
            Oxy,
            Phase,
            Plasma,
            Rain,
            Solar,
            Speed,
            Tarn,
            Tempo,
            Thermal,
            Topo,
            Turbid,
            Turbo,
            Viridis
        }

        /// <summary>
        /// plot line style options
        /// </summary>
        public enum LineStyle
        {
            None,
            Solid,
            Dash,
            DashDot,
            DashDotDot,
            Dot
        }

        /// <summary>
        /// plot marker shape options
        /// </summary>
        public enum MarkerShape
        {
            none,
            filledCircle,
            filledSquare,
            openCircle,
            openSquare,
            filledDiamond,
            openDiamond,
            asterisk,
            hashTag,
            cross,
            eks,
            verticalBar,
            triUp,
            triDown,
            filledTriangleUp,
            filledTriangleDown,
            openTriangleUp,
            openTriangleDown
        }


        /// <summary>
        /// visualization options
        /// </summary>
        public enum VisualOption
        {
            /// <summary>
            /// visible on the left 
            /// </summary>
            Visible = 0,
            /// <summary>
            /// hidden 
            /// </summary>
            Hidden = 1,
            /// <summary>
            /// visible on the right
            /// </summary>
            VisibleRight = 2
        }

        /// <summary>
        /// legend location options
        /// </summary>
        public enum LegendLocation
        {
            UpperLeft,
            UpperRight,
            UpperCenter,
            MiddleLeft,
            MiddleCenter,
            MiddleRight,
            LowerLeft,
            LowerRight,
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
}
