using ScottPlot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using VEMS.MathCore;
using LineStyle = VEMS.Plot.Options.LineStyle;
using MarkerShape = VEMS.Plot.Options.MarkerShape;
using PlotColor = VEMS.Plot.Options.PlotColor;
using VisualOption = VEMS.Plot.Options.VisualOption;
using PlotColormap = VEMS.Plot.Options.PlotColormap;
using GraphInterpolationMode = VEMS.Plot.Options.GraphInterpolationMode;
using System.Drawing.Drawing2D;

namespace VEMS.Plot
{
    /// <summary>
    /// DataPropertyExpander.xaml 的交互逻辑
    /// </summary>
    public partial class DataPropertyExpander : UserControl
    {

        #region properties

        /// <summary>
        /// data label
        /// </summary>
        public string DataLabel
        {
            get => LabelTextBox.Text;
            set => LabelTextBox.Text = value;
        }

        /// <summary>
        /// visualization option of the data
        /// </summary>
        public VisualOption VisualOption
        {
            get => (VisualOption)VisualComboBox.SelectedValue;
            set
            {
                VisualComboBox.ItemsSource = Enum.GetValues(typeof(VisualOption));
                VisualComboBox.SelectedValue = value;
            }

        }

        /// <summary>
        /// complex part for the plot
        /// </summary>
        public ComplexPart? PlotPart
        {
            get => (ComplexPart?)PartComboBox.SelectedValue;
            set
            {
                PartLabel.Visibility = value == null ?
                    Visibility.Collapsed : Visibility.Visible;
                PartComboBox.Visibility = value == null ?
                    Visibility.Collapsed : Visibility.Visible;
                PartComboBox.ItemsSource = value == null ?
                    null : Enum.GetValues(typeof(ComplexPart));
                PartComboBox.SelectedValue = value;
            }
        }

        /// <summary>
        /// line width
        /// </summary>
        public double? LineWidth
        {
            get => double.Parse(LineWidthTextBox.Text);
            set
            {
                LineWidthLabel.Visibility = value == null ?
                    Visibility.Collapsed : Visibility.Visible;
                LineWidthTextBox.Visibility = value == null ?
                    Visibility.Collapsed : Visibility.Visible;
                LineWidthTextBox.Text = value.ToString();
            }
        }

        /// <summary>
        /// line style
        /// </summary>
        public LineStyle? LineStyle
        {
            get => (LineStyle?)LineStyleComboBox.SelectedValue;
            set
            {
                LineStyleLabel.Visibility = value == null ?
                    Visibility.Collapsed : Visibility.Visible;
                LineStyleComboBox.Visibility = value == null ?
                    Visibility.Collapsed : Visibility.Visible;
                LineStyleComboBox.ItemsSource = value == null ?
                    null : Enum.GetValues(typeof(LineStyle));
                LineStyleComboBox.SelectedValue = value;
            }
        }

        /// <summary>
        /// line color
        /// </summary>
        public PlotColor? LineColor
        {
            get => (PlotColor?)LineColorComboBox.SelectedValue;
            set
            {
                LineColorLabel.Visibility = value == null ?
                    Visibility.Collapsed : Visibility.Visible;
                LineColorComboBox.Visibility = value == null ?
                    Visibility.Collapsed : Visibility.Visible;
                LineColorComboBox.ItemsSource = value == null ?
                    null : Enum.GetValues(typeof(PlotColor));
                LineColorComboBox.SelectedValue = value;
            }
        }

        /// <summary>
        /// marker size
        /// </summary>
        public double? MarkerSize
        {
            get => double.Parse(MarkerSizeTextBox.Text);
            set
            {
                MarkerSizeLabel.Visibility = value == null ?
                    Visibility.Collapsed : Visibility.Visible;
                MarkerSizeTextBox.Visibility = value == null ?
                    Visibility.Collapsed : Visibility.Visible;
                MarkerSizeTextBox.Text = value.ToString();
            }
        }

        /// <summary>
        /// marker shape
        /// </summary>
        public MarkerShape? MarkerShape
        {
            get => (MarkerShape?)MarkerShapeComboBox.SelectedValue;
            set
            {
                MarkerShapeLabel.Visibility = value == null ?
                    Visibility.Collapsed : Visibility.Visible;
                MarkerShapeComboBox.Visibility = value == null ?
                    Visibility.Collapsed : Visibility.Visible;
                MarkerShapeComboBox.ItemsSource = value == null ?
                    null : Enum.GetValues(typeof(MarkerShape));
                MarkerShapeComboBox.SelectedValue = value;
            }
        }

        /// <summary>
        /// marker color
        /// </summary>
        public PlotColor? MarkerColor
        {
            get => (PlotColor?)MarkerColorComboBox.SelectedValue;
            set
            {
                MarkerColorLabel.Visibility = value == null ?
                    Visibility.Collapsed : Visibility.Visible;
                MarkerColorComboBox.Visibility = value == null ?
                    Visibility.Collapsed : Visibility.Visible;
                MarkerColorComboBox.ItemsSource = value == null ?
                    null : Enum.GetValues(typeof(PlotColor));
                MarkerColorComboBox.SelectedValue = value;
            }
        }

        /// <summary>
        /// plot color (line / marker )
        /// </summary>
        public PlotColor? PlotColor
        {
            get => (PlotColor?)PlotColorComboBox.SelectedValue;
            set
            {
                PlotColorLabel.Visibility = value == null ?
                    Visibility.Collapsed : Visibility.Visible;
                PlotColorComboBox.Visibility = value == null ?
                    Visibility.Collapsed : Visibility.Visible;
                PlotColorComboBox.ItemsSource = value == null ?
                    null : Enum.GetValues(typeof(PlotColor));
                PlotColorComboBox.SelectedValue = value;
            }
        }

        /// <summary>
        /// fill color 
        /// </summary>
        public PlotColor? FillColor
        {
            get => (PlotColor?)FillColorComboBox.SelectedValue;
            set
            {
                FillColorLabel.Visibility = value == null ?
                    Visibility.Collapsed : Visibility.Visible;
                FillColorComboBox.Visibility = value == null ?
                    Visibility.Collapsed : Visibility.Visible;
                FillColorComboBox.ItemsSource = value == null ?
                    null : Enum.GetValues(typeof(PlotColor));
                FillColorComboBox.SelectedValue = value;
            }
        }

        /// <summary>
        /// colormap option
        /// </summary>
        public PlotColormap? Colormap
        {
            get => (PlotColormap?)ColormapComboBox.SelectedValue;
            set
            {
                ColormapLabel.Visibility = value == null ?
                    Visibility.Collapsed : Visibility.Visible;
                ColormapComboBox.Visibility = value == null ?
                    Visibility.Collapsed : Visibility.Visible;
                ColormapComboBox.ItemsSource = value == null ?
                    null : Enum.GetValues(typeof(PlotColormap));
                ColormapComboBox.SelectedValue = value;
            }
        }

        /// <summary>
        /// graph smooth mode
        /// </summary>
        public GraphInterpolationMode? SmoothMode
        {
            get => (GraphInterpolationMode?)SmoothComboBox.SelectedValue;
            set
            {
                SmoothLabel.Visibility = value == null ?
                    Visibility.Collapsed : Visibility.Visible;
                SmoothComboBox.Visibility = value == null ?
                    Visibility.Collapsed : Visibility.Visible;
                SmoothComboBox.ItemsSource = value == null ?
                    null : Enum.GetValues(typeof(GraphInterpolationMode));
                SmoothComboBox.SelectedValue = value;
            }
        }

        #endregion



        public DataPropertyExpander()
        {
            InitializeComponent();

            // link label to header
            LabelTextBox.TextChanged += (o, e) => HeaderTextBlock.Text = DataLabel;
        }
    }



}
