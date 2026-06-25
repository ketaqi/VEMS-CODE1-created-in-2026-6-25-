using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using VEMS.MathCore;

namespace VEMS.Plot
{
    /// <summary>
    /// Fig2D
    /// </summary>
    public partial class GridFig2D : Window
    {
        private GridFig2DModel model;
        private const string DefaultLabel = "Graph";
        private const string DefaultFigTitle = "VEMS Plot(s)";
        private const string DefaultFigLabelX = "variable";
        private const string DefaultFigLabelY = "function value";

        #region constructors

        /// <summary>
        /// constructs a Fig2D
        /// for a single input grid matrix data
        /// </summary>
        /// <param name="data"> data to plot </param>
        /// <param name="colormapName"> name of the colormap for the graph </param>
        /// <param name="label"> label of the graph </param>
        /// <param name="figTitle"> title of the figure </param>
        /// <param name="figLabelX"> label of the x axis </param>
        /// <param name="figLabelY"> label of the y axis </param>
        public GridFig2D(Grid2DRealData data,
            string colormapName, string label, string figTitle,
            string figLabelX, string figLabelY)
        {
            InitializeComponent();

            model = new(PlotFrame, data, colormapName,
                label, figTitle, figLabelX, figLabelY);
            DataContext = model;
            // auto detect view range when loaded
            Loaded += (o, e) => { model.DetectViewRange(); };
        }

        /// <summary>
        /// constructs a Fig2D
        /// for a list of input grid matrix data
        /// </summary>
        /// <param name="data"> data to plot </param>
        /// <param name="colormapName"> names of the colormaps for the graphs </param>
        /// <param name="label"> labels of the graphs </param>
        /// <param name="figTitle"> title of the figure </param>
        /// <param name="figLabelX"> label of the x axis </param>
        /// <param name="figLabelY"> label of the y axis </param>
        public GridFig2D(List<Grid2DRealData> data, 
            List<string> colormapName, List<string> label, string figTitle,
            string figLabelX, string figLabelY)
        {
            InitializeComponent();

            model = new(PlotFrame, data, colormapName,
                label, figTitle, figLabelX, figLabelY);
            DataContext = model;
            // auto detect view range when loaded
            Loaded += (o, e) => { model.DetectViewRange(); };
        }

        #endregion
        #region column adjustment

        private void AdvancedCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            AdvColumn.Width = new GridLength(460);
            if (WindowState == WindowState.Normal)
                Width = 1200;
        }

        private void AdvancedCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            AdvColumn.Width = new GridLength(0);
            if (WindowState == WindowState.Normal)
                Width = 736;
        }

        #endregion

    }
}
