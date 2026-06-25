using System.Reflection;

namespace VEMS.Plot5
{
    /// <summary>
    /// interface for plot properties
    /// </summary>
    public interface IGrid1DPlotProperty
    {
        /// <summary>
        /// width of the line
        /// </summary>
        double? LineWidth { get; set; }

        /// <summary>
        /// pattern of the line
        /// </summary>
        LinePattern? LinePattern { get; set; }

        /// <summary>
        /// color of the plot
        /// </summary>
        PlotColor? PlotColor { get; set; }

        /// <summary>
        /// display options: left, right, hidden
        /// </summary>
        DisplayOption? DisplayOption { get; set; }

        /// <summary>
        /// label of the plot
        /// </summary>
        string? Label { get; set; }

        /// <summary>
        /// gets the list of properties for this class
        /// </summary>
        /// <returns> list of properties </returns>
        PropertyInfo[] GetPropertyList();

    }
}
