using ScottPlot;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using VEMS.MathCore;

using Color = System.Drawing.Color;
using PlotColor = VEMS.Plot.Options.PlotColor;
using VisualOption = VEMS.Plot.Options.VisualOption;


namespace VEMS.Plot
{
    /// <summary>
    /// DataPropExpander.xaml 的交互逻辑
    /// </summary>
    [Obsolete]
    public partial class DataPropExpander : UserControl //, INotifyPropertyChanged
    {
        #region properties

        /// <summary>
        /// data label
        /// </summary>
        public string? DataLabel 
        { 
            get => LabelTextBox.Text;
            set => LabelTextBox.Text = value;
        }

        /// <summary>
        /// complex part for the plot
        /// </summary>
        public ComplexPart PlotPart
        {
            get => (ComplexPart)PartComboBox.SelectedValue;
            set => PartComboBox.SelectedValue = value;
            //{
            //    PartComboBox.SelectedValue = value;
            //    if (value == null)
            //    {
            //        PartLabel.Visibility = Visibility.Collapsed;
            //        PartComboBox.Visibility = Visibility.Collapsed;
            //    }
            //}
        }

        /// <summary>
        /// flag to enable or disable the PlotPart option
        /// </summary>
        public bool IsPlotPartEnabled
        {
            get => IsPlotPartEnabled;
            set
            {
                switch(value)
                {
                    case false:
                        {
                            PartComboBox.IsEnabled = false;
                            PartComboBox.Foreground = System.Windows.Media.Brushes.LightGray;
                            PartLabel.Foreground = System.Windows.Media.Brushes.LightGray;
                            break;
                        }
                    case true:
                        break;
                }
            }
        }

        /// <summary>
        /// visualization option of the data
        /// </summary>
        public VisualOption? VisualOption
        {
            get => (VisualOption)VisualComboBox.SelectedValue;
            set => VisualComboBox.SelectedValue = value;
        }
        
        /// <summary>
        /// line width
        /// </summary>
        public double LineWidth
        {
            get => double.Parse(LineWidthTextBox.Text);
            set => LineWidthTextBox.Text = value.ToString();
        }

        /// <summary>
        /// line style
        /// </summary>
        public LineStyle LineStyle
        {
            get => (LineStyle)LineStyleComboBox.SelectedValue;
            set => LineStyleComboBox.SelectedValue = value;
        }

        /// <summary>
        /// marker size
        /// </summary>
        public double MarkerSize
        {
            get => double.Parse(MarkerSizeTextBox.Text);
            set => MarkerSizeTextBox.Text = value.ToString();
        }

        /// <summary>
        /// marker shape
        /// </summary>
        public MarkerShape MarkerShape
        {
            get => (MarkerShape)MarkerShapeComboBox.SelectedValue;
            set => MarkerShapeComboBox.SelectedValue = value;
        }

        /// <summary>
        /// line / marker color
        /// </summary>
        public PlotColor PlotColor
        {
            get => (PlotColor)PlotColorComboBox.SelectedValue;
            set => PlotColorComboBox.SelectedValue = value;
        }


        public PlotColor? TestColor
        {
            get => (PlotColor)TestColorComboBox.SelectedValue;
            set
            {
                TestColorComboBox.SelectedValue = value;
                TestColorLabel.Visibility = value == null? 
                    Visibility.Collapsed : Visibility.Visible;
                TestColorComboBox.Visibility = value == null?
                    Visibility.Collapsed : Visibility.Visible;
            }
        }

        #endregion

        // constructor
        public DataPropExpander()
        {
            InitializeComponent();

            // set ItemsSources
            PartComboBox.ItemsSource = Enum.GetValues(typeof(ComplexPart));
            VisualComboBox.ItemsSource = Enum.GetValues(typeof(VisualOption));
            LineStyleComboBox.ItemsSource = Enum.GetValues(typeof(LineStyle));
            MarkerShapeComboBox.ItemsSource = Enum.GetValues(typeof(MarkerShape));
            PlotColorComboBox.ItemsSource = Enum.GetValues(typeof(PlotColor));

            // DataLabel => Header
            LabelTextBox.TextChanged += (o, e) => HeaderTextBlock.Text = DataLabel;

        }

    }



}
