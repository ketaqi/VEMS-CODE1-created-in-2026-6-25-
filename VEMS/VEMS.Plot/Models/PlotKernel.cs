using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ScottPlot;
using ScottPlot.Plottable;

using VEMS.MathCore;

namespace VEMS.Plot
{
    /// <summary>
    /// core class for plot 1D
    /// </summary>
    public class PlotKernel
    {
        // here i make some modifications ...
        #region properties
        // ...
        #endregion
        #region constructor
        // ...
        #endregion
        #region methods
        // ...
        #endregion
        #region static methods

        #region 1D SignalPlot

        /// <summary>
        /// signal plot method
        /// </summary>
        /// <param name="wpfPlot"> exisitng wpfPlot from ScottPlot </param>
        /// <param name="gridInfo"> sampling grid infomation </param>
        /// <param name="data"> regular data to plot </param>
        /// <param name="color"> line color </param>
        /// <param name="width"> line width </param>
        /// <param name="markerScale"> marker scale </param>
        /// <param name="noMargin"> whether to leave margin in the plot </param>
        /// <returns> signal plot </returns>
        public static SignalPlot Plot(WpfPlot wpfPlot,
            GridInfo1D gridInfo, double[] data,
            System.Drawing.Color color,
            double width = 4.0,
            double markerScale = 2.0,
            bool noMargin = false)
        {
            // plot
            SignalPlot plt = wpfPlot.Plot.AddSignal(data,
                1.0 / gridInfo.Spacing);
            plt.OffsetX = gridInfo.Start;
            plt.Color = color;
            plt.LineWidth = width;
            plt.MarkerSize = (float)(width * markerScale);
            // margin
            if (noMargin == true)
                wpfPlot.Plot.Margins(0, 0);
            // return
            return plt;
        }

        /// <summary>
        /// signal plot method
        /// </summary>
        /// <param name="wpfPlot"> existing wpfPLot from ScottPlot </param>
        /// <param name="gridInfo"> sampling grid infomation </param>
        /// <param name="data"> regular data to plot </param>
        /// <param name="color"> line color </param>
        /// <param name="width"> line width </param>
        /// <param name="markerScale"> marker scale </param>
        /// <param name="noMargin"> whether to leave margin in the plot </param>
        /// <returns> signal plot </returns>
        public static SignalPlot Plot(WpfPlot wpfPlot,
            GridInfo1D gridInfo, VectorD data,
            System.Drawing.Color color,
            double width = 4.0,
            double markerScale = 2.0,
            bool noMargin = false)
            => Plot(wpfPlot,
                gridInfo, VMath.ConvertVectorToArray(data),
                color, width, markerScale, noMargin);

        #endregion
        #region 1D ScatterPlot

        /// <summary>
        /// scatter plot method
        /// </summary>
        /// <param name="wpfPlot"> existing wpfPLot from ScottPlot </param>
        /// <param name="coordinates"> coordinates array </param>
        /// <param name="data"> data array </param>
        /// <param name="color"> line color </param>
        /// <param name="width"> line width </param>
        /// <param name="markerScale"> marker scale </param>
        /// <param name="noMargin"> whether to leave margin in the plot </param>
        /// <returns> scatter plot </returns>
        public static ScatterPlot Plot(WpfPlot wpfPlot,
            double[] coordinates, double[] data,
            System.Drawing.Color color,
            double width = 1.0,
            double markerScale = 10.0,
            bool noMargin = false)
        {
            // plot
            ScatterPlot plt = wpfPlot.Plot.AddScatter(coordinates, data,
                color, (float)width, (float)(width * markerScale),
                MarkerShape.filledDiamond,
                LineStyle.Dash);
            // margin
            if (noMargin == true)
                wpfPlot.Plot.Margins(0, 0);
            // refresh
            //wpfPlot.Refresh();
            return plt;
        }

        /// <summary>
        /// scatter plot method
        /// </summary>
        /// <param name="wpfPlot"> existing wpfPLot from ScottPlot </param>
        /// <param name="coordinates"> coordinates vector </param>
        /// <param name="data"> data vector </param>
        /// <param name="color"> line color </param>
        /// <param name="width"> line width </param>
        /// <param name="markerScale"> marker scale </param>
        /// <param name="noMargin"> whether to leave margin in the plot </param>
        /// <returns> scatter plot </returns>
        public static ScatterPlot Plot(WpfPlot wpfPlot,
            VectorD coordinates, VectorD data,
            System.Drawing.Color color,
            double width = 1.0,
            double markerScale = 10.0,
            bool noMargin = false)
            => Plot(wpfPlot,
                VMath.ConvertVectorToArray(coordinates),
                VMath.ConvertVectorToArray(data),
                color, width, markerScale);

        #endregion
        #region 2D Plot

        /// <summary>
        /// plots a heatmap
        /// </summary>
        /// <param name="wpfPlot"> existing wpfPlot from ScottPlot </param>
        /// <param name="gridInfo"> sampling grid information </param>
        /// <param name="data"> regular data to plot </param>
        /// <param name="cm"> color map for the plot </param>
        /// <param name="noMargin"> whether to leave margin in the plot </param>
        /// <returns> heatmap plot </returns>
        public static Heatmap Plot(WpfPlot wpfPlot,
            GridInfo2D gridInfo, double[,] data,
            ScottPlot.Drawing.Colormap cm,
            bool noMargin = true)
        {
            // plot
            Heatmap hm = wpfPlot.Plot.AddHeatmap(data, colormap: cm);
            hm.XMin = gridInfo.LowerBoundX;
            hm.XMax = gridInfo.UpperBoundX;
            hm.YMin = gridInfo.LowerBoundY;
            hm.YMax = gridInfo.UpperBoundY;
            wpfPlot.Plot.AddColorbar(hm);
            // margin
            if (noMargin == true)
                wpfPlot.Plot.Margins(0, 0);
            // return
            return hm;
        }

        /// <summary>
        /// plots a heatmap
        /// </summary>
        /// <param name="wpfPlot"></param>
        /// <param name="gridInfo"></param>
        /// <param name="data"></param>
        /// <param name="cm"></param>
        /// <param name="noMargin"> whether to leave margin in the plot </param>
        /// <returns></returns>
        public static Heatmap Plot(WpfPlot wpfPlot,
            GridInfo2D gridInfo, MatrixD data,
            ScottPlot.Drawing.Colormap cm,
            bool noMargin = true)
            => Plot(wpfPlot, gridInfo,
                VMath.ConvertMatrixToArray(data), cm, noMargin);

        /// <summary>
        /// adds captions to an existing plot
        /// </summary>
        /// <param name="wpfPlot"> existing wpfPlot from ScottPlot </param>
        /// <param name="title"> plot title </param>
        /// <param name="xLabel"> plot label x </param>
        /// <param name="yLabel"> plot label y </param>
        public static void PlotCaptions(WpfPlot wpfPlot,
            string title = "2D plot",
            string xLabel = "x",
            string yLabel = "y")
        {
            wpfPlot.Plot.Title(title);
            wpfPlot.Plot.XLabel(xLabel);
            wpfPlot.Plot.YLabel(yLabel);
        }

        #endregion

        #endregion






    }

}
