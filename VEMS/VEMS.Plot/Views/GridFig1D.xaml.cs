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
    /// Fig1D.xaml 的交互逻辑
    /// </summary>
    public partial class GridFig1D : Window
    {
        private GridFig1DModel model;
        private const double DefaultLineWidth = 4.0;
        private const double DefaultMarkerSize = 7.0;
        private const string DefaultLabel = "Line";
        private const string DefaultFigTitle = "VEMS Plot(s)";
        private const string DefaultFigLabelX = "variable";
        private const string DefaultFigLabelY = "function value";

        #region constructors

        /// <summary>
        /// constructs a Fig1D
        /// for single input grid vector data
        /// </summary>
        /// <param name="data"> input grid vector data </param>
        /// <param name="lineColor"> color of the line </param>
        /// <param name="lineWidth"> width of the line </param>
        /// <param name="markerSize"> size of the markers </param>
        /// <param name="label"> label of the line </param>
        /// <param name="figTitle"> title of the figure(s) </param>
        /// <param name="figLabelX"> label of the x axis </param>
        /// <param name="figLabelY"> label of the y axis </param>
        public GridFig1D(Grid1DRealData data,
            System.Drawing.Color lineColor,
            double lineWidth, double markerSize,
            string label, string figTitle,
            string figLabelX, string figLabelY)
        {
            InitializeComponent();

            model = new(PlotFrame, data, lineColor,
                lineWidth, markerSize,
                label, figTitle,
                figLabelX, figLabelY);
            DataContext = model;
        }

        /// <summary>
        /// constructs a Fig1D
        /// for a list of input grid vector data
        /// </summary>
        /// <param name="data"> input grid vector data </param>
        /// <param name="lineColor"> color of the line </param>
        /// <param name="lineWidth"> width of the line </param>
        /// <param name="markerSize"> size of the markers </param>
        /// <param name="label"> label of the line </param>
        /// <param name="figTitle"> title of the figure(s) </param>
        /// <param name="figLabelX"> label of the x axis </param>
        /// <param name="figLabelY"> label of the y axis </param>
        public GridFig1D(List<Grid1DRealData> data,
            List<System.Drawing.Color> lineColor,
            double lineWidth, double markerSize,
            List<string> label, string figTitle,
            string figLabelX, string figLabelY)
        {
            InitializeComponent();

            model = new GridFig1DModel(PlotFrame, data, lineColor,
                lineWidth, markerSize, label,
                figTitle, figLabelX, figLabelY);
            DataContext = model;
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
