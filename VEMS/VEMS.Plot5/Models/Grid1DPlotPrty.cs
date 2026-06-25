using System.Reflection;
using VEMS.MathCore;

namespace VEMS.Plot5
{

    /// <summary>
    /// plot visual properties for Grid1DRealPlot
    /// </summary>
    public class Grid1DRealPlotPrty : IGrid1DPlotProperty
    {
        #region properties

        /// <summary>
        /// width of the line
        /// </summary>
        public double? LineWidth { get; set; }

        /// <summary>
        /// pattern of the line
        /// </summary>
        public LinePattern? LinePattern { get; set; }

        /// <summary>
        /// color of the plot
        /// </summary>
        public PlotColor? PlotColor { get; set; }

        /// <summary>
        /// display options: left, right, hidden
        /// </summary>
        public DisplayOption? DisplayOption { get; set; }

        /// <summary>
        /// label of the plot
        /// </summary>
        public string? Label { get; set; }

        #endregion
        #region constructor

        /// <summary>
        /// default constructor
        /// </summary>
        public Grid1DRealPlotPrty() { }

        #endregion
        #region methods

        /// <summary>
        /// gets the list of properties for this class
        /// </summary>
        /// <returns> list of properties </returns>
        public PropertyInfo[] GetPropertyList()
        {
            Type t = GetType();
            return t.GetProperties();
        }

        #endregion
    }


    /// <summary>
    /// plot visual properties for Grid1DCplxPlot
    /// </summary>
    public class Grid1DCplxPlotPrty : Grid1DRealPlotPrty
    {
        #region [additional] properties

        /// <summary>
        /// selected part of the complex data
        /// </summary>
        public ComplexPart? ComplexPart { get; set; }

        #endregion

        /// <summary>
        /// default constructor
        /// </summary>
        public Grid1DCplxPlotPrty() { }
    }
}
