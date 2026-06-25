using ScottPlot;
using ScottPlot.Plottable;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
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


namespace VEMS.Plot
{
    /// <summary>
    /// ScatFig1DControl.xaml 的交互逻辑
    /// </summary>
    [Obsolete]
    public partial class ScatFig1DControl : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string properyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(properyName));


        private const double DefaultLineWidth = 4.0;
        private const MarkerShape DefaultMarkerShape = MarkerShape.filledCircle;
        private const double DefaultMarkerSize = 7.0;
        private const string DefaultLabel = "Line";
        private const string DefaultFigTitle = "VEMS Scattered Plot(s)";
        private const string DefaultFigLabelX = "variable";
        private const string DefaultFigLabelY = "function value";


        #region properties
        // lines
        //private double _lineWidth { get; set; }
        //private double _markerShape { get; set; }
        //private double _markerSize { get; set; }
        // text
        //private string _label { get; set; }
        //private string _figTitle { get; set; }
        //private string _figLabelX { get; set; }
        //private string _figLabelY { get; set; }
        //private string _figLabelYRight { get; set; }
        // axes
        //private bool _needRightYAxis { get; set; }
        //private string _xAxisName { get; set; } 
        private double _xAxisMinValue { get; set; }
        private double _xAxisMaxValue { get; set; }
        //private string _yAxisName { get; set; }
        private double _yAxisMinValue { get; set; }
        private double _yAxisMaxValue { get; set; }
        //private string _yRightAxisName { get; set; }
        private double _yRightAxisMinValue { get; set; }
        private double _yRightAxisMaxValue { get; set; }
        private string _figTitle { get; set; }
        private string _figLabelX { get; set; }
        private string _figLabelY { get; set; }
        private string _figLabelYRight { get; set; }
        private double _cursorLocationX { get; set; }
        private double _cursorLocationY { get; set; }
        private int _cursorPixelX { get; set; }
        private int _cursorPixelY { get; set; }
        private double _nearestSampleX { get; set; }
        private double _nearestSampleY { get; set; }
        private int _activePlotIndex { get; set; }
        private bool _needLeftYAxis { get; set; }
        private bool _needRightYAxis { get; set; }
        private bool _showLegend { get; set; }
        private Alignment _legendLocation { get; set; }


        /// <summary>
        /// the minimum view range value of the x-axis
        /// </summary>
        public double XAxisMinValue
        {
            get => _xAxisMinValue;
            set => _xAxisMinValue = value;
        }

        /// <summary>
        /// the maximum view range value of the x-axis
        /// </summary>
        public double XAxisMaxValue
        {
            get => _xAxisMaxValue;
            set => _xAxisMaxValue = value;
        }

        /// <summary>
        /// the minimum view range value of the y-axis
        /// </summary>
        public double YAxisMinValue
        {
            get => _yAxisMinValue;
            set => _yAxisMinValue = value;
        }

        /// <summary>
        /// the maximum view range value of the y-axis
        /// </summary>
        public double YAxisMaxValue
        {
            get => _yAxisMaxValue;
            set => _yAxisMaxValue = value;
        }


        //public double LineWidth
        //{
        //    get => _lineWidth;
        //    set
        //    {
        //        _lineWidth = value;
        //    }
        //}


        #endregion


        public ScatFig1DControl()
        {
            InitializeComponent();


            // PlotFrame.Plot.AddScatter()
            ScatterPlot sctPlt;
            

            


        }
    }
}
