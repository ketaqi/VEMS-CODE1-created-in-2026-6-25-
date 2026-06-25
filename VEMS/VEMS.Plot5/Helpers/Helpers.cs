using ScottPlot;
using System.Drawing.Drawing2D;

namespace VEMS.Plot5
{

    /// <summary>
    /// helper functions
    /// </summary>
    public class Helpers
    {
        #region ==== parsers ====

        internal static ScottPlot.LinePattern ParseLinePattern(LinePattern? pattern)
        {
            LinePattern s = (pattern == null)? Defaults.Pattern : (LinePattern)pattern;
            Type t = typeof(ScottPlot.LinePattern);
            return (ScottPlot.LinePattern)Enum.Parse(enumType: t, value: s.ToString());
        }


        internal static ScottPlot.MarkerShape ParseMarkerShape(MarkerShape? shape)
        {
            MarkerShape s = (shape == null) ? Defaults.Shape : (MarkerShape)shape; 
            Type t = typeof(ScottPlot.MarkerShape);
            return (ScottPlot.MarkerShape)Enum.Parse(enumType: t, value: s.ToString());
        }


        internal static ScottPlot.Color ParseColor(PlotColor? color)
        {
            PlotColor s = (color == null) ? Defaults.Color : (PlotColor)color;
            System.Drawing.Color c = System.Drawing.Color.FromName(name: s.ToString());
            //return c.ToColor();
            return new(red: c.R, green: c.G, blue: c.B);
        }

        //private static System.Drawing.Color ParseColor(int v, PlotColor color)
        //    => Color.FromArgb(v, ParseColor(color));

        internal static ScottPlot.IColormap ParseColormap(PlotColormap? colormap)
        {
            if(colormap == null) { colormap = Defaults.Colormap; }
            switch (colormap)
            {
                case PlotColormap.Amp:
                    return new ScottPlot.Colormaps.Amp();
                case PlotColormap.Algae:
                    return new ScottPlot.Colormaps.Algae();
                case PlotColormap.Balance:
                    return new ScottPlot.Colormaps.Balance();
                case PlotColormap.Blues:
                    return new ScottPlot.Colormaps.Balance();
                case PlotColormap.Curl:
                    return new ScottPlot.Colormaps.Curl();
                case PlotColormap.Deep:
                    return new ScottPlot.Colormaps.Deep();
                case PlotColormap.Delta:
                    return new ScottPlot.Colormaps.Delta();
                case PlotColormap.Dense:
                    return new ScottPlot.Colormaps.Dense();
                case PlotColormap.Diff:
                    return new ScottPlot.Colormaps.Diff();
                case PlotColormap.Grayscale:
                    return new ScottPlot.Colormaps.Grayscale();
                case PlotColormap.GrayscaleR:
                    return new ScottPlot.Colormaps.GrayscaleReversed();
                case PlotColormap.Greens:
                    return new ScottPlot.Colormaps.Greens();
                case PlotColormap.Haline:
                    return new ScottPlot.Colormaps.Haline();
                case PlotColormap.Ice:
                    return new ScottPlot.Colormaps.Ice();
                case PlotColormap.Inferno:
                    return new ScottPlot.Colormaps.Inferno();
                case PlotColormap.Jet:
                    return new ScottPlot.Colormaps.Jet();
                case PlotColormap.Magma:
                    return new ScottPlot.Colormaps.Magma();
                case PlotColormap.Matter:
                    return new ScottPlot.Colormaps.Matter();
                case PlotColormap.Oxy:
                    return new ScottPlot.Colormaps.Oxy();
                case PlotColormap.Phase:
                    return new ScottPlot.Colormaps.Phase();
                case PlotColormap.Plasma:
                    return new ScottPlot.Colormaps.Plasma();
                case PlotColormap.Rain:
                    return new ScottPlot.Colormaps.Rain();
                case PlotColormap.Solar:
                    return new ScottPlot.Colormaps.Solar();
                case PlotColormap.Speed:
                    return new ScottPlot.Colormaps.Speed();
                case PlotColormap.Tarn:
                    return new ScottPlot.Colormaps.Tarn();
                case PlotColormap.Tempo:
                    return new ScottPlot.Colormaps.Tempo();
                case PlotColormap.Thermal:
                    return new ScottPlot.Colormaps.Thermal();
                case PlotColormap.Topo:
                    return new ScottPlot.Colormaps.Topo();
                case PlotColormap.Turbid:
                    return new ScottPlot.Colormaps.Turbid();
                case PlotColormap.Turbo:
                    return new ScottPlot.Colormaps.Turbo();
                case PlotColormap.Viridis:
                    return new ScottPlot.Colormaps.Viridis();
                default: goto case PlotColormap.Grayscale;
            }
        }

        internal static InterpolationMode ParseGraphInterpolationMode(GraphInterpolationMode mode)
            => (InterpolationMode)Enum.Parse(enumType: typeof(InterpolationMode),
                value: mode.ToString());

        internal static Alignment ParseLegendLocation(LegendLocation location)
            => (Alignment)Enum.Parse(enumType: typeof(Alignment),
                value: location.ToString());

        #endregion
        #region ==== line ====

        /// <summary>
        /// sets the properties of the line
        /// </summary>
        /// <param name="line"> line object to modify </param>
        /// <param name="width"> width of the line </param>
        /// <param name="pattern"> pattern of the line </param>
        /// <param name="color"> color of the line </param>
        /// <param name="isVisible"> visibility of the line </param>
        internal static void SetLine(ScottPlot.LineStyle line,
            double width, LinePattern pattern, PlotColor color,
            bool isVisible = true)
        {
            line.Width = (float)width;
            line.Pattern = ParseLinePattern(pattern);
            line.Color = ParseColor(color);
            line.IsVisible = isVisible;
        }

        #endregion
        #region ==== marker ====

        /// <summary>
        /// sets the properties of the marker
        /// </summary>
        /// <param name="marker"> marker object to modify </param>
        /// <param name="size"> size of the marker </param>
        /// <param name="shape"> shape of the marker </param>
        /// <param name="color"> fill color of the marker </param>
        /// <param name="isVisible"> visibility of the marker </param>
        internal static void SetMarker(ScottPlot.MarkerStyle marker,
            double size, MarkerShape shape, PlotColor color,
            bool isVisible = true)
        {
            marker.Size = (float)size;
            marker.Shape = ParseMarkerShape(shape);
            marker.FillColor = ParseColor(color); //new() { Color = ParseColor(color) };
            marker.IsVisible = isVisible;
        }

        #endregion


    }

}
