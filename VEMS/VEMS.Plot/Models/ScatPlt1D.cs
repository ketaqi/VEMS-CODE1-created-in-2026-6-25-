using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using VEMS.MathCore;
using ScottPlot.Plottable;
using System.Drawing;
using ScottPlot;
using System.Xml.Schema;
using System.Net.Http.Headers;

namespace VEMS.Plot
{
    /// <summary>
    /// Scattered Plot 1D (ScatPlt1D) class
    /// </summary>
    public class ScatPlt1D : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        #region properties

        private double[] _dataLocations { get; set; }
        private double[] _dataValues { get; set; }
        private string _label { get; set; }
        private double _lineWidth { get; set; }
        private MarkerShape _markerShape { get; set; }
        private double _markerSize { get; set; }
        private Color _lineColor { get; set; }
        private DrawOption _visualOption { get; set; }


        /// <summary>
        /// list of the data point locations
        /// </summary>
        public double[] DataLocations
        {
            get => _dataLocations;
            set
            {
                _dataLocations = value;
                NeedReplot = true;
            }
        }

        /// <summary>
        /// list of the data values
        /// </summary>
        public double[] DataValues
        {
            get => _dataValues;
            set
            {
                _dataValues = value;
                NeedReplot = true;
            }
        }

        /// <summary>
        /// ScatterPlot from ScottPlot
        /// (the actual plot object)
        /// </summary>
        public ScatterPlot ScPlot { get; set; }

        /// <summary>
        /// label of the plot
        /// </summary>
        public string Label
        {
            get => _label;
            set
            {
                _label = value;
                OnPropertyChanged(nameof(Label));
            }
        }

        /// <summary>
        /// the total number of data points
        /// </summary>
        public long Points
        {
            get => _dataLocations.LongLength;
        }

        /// <summary>
        /// line width of the line
        /// </summary>
        public double LineWidth
        {
            get => _lineWidth;
            set
            {
                _lineWidth = value;
                OnPropertyChanged(nameof(LineWidth));
            }
        }

        /// <summary>
        /// shape of the marker
        /// </summary>
        public MarkerShape MarkerShape 
        {
            get => _markerShape;
            set
            {
                _markerShape = value;
                OnPropertyChanged(nameof(MarkerShape));
            }
        }

        /// <summary>
        /// size of the marker(s)
        /// </summary>
        public double MarkerSize
        {
            get => _markerSize;
            set
            {
                _markerSize = value;
                OnPropertyChanged(nameof(MarkerSize));
            }
        }

        /// <summary>
        /// color of the line
        /// </summary>
        public Color LineColor
        {
            get => _lineColor;
            set
            {
                _lineColor = value;
                OnPropertyChanged(nameof(LineColor));
            }
        }

        /// <summary>
        /// visual option of the line
        /// </summary>
        public DrawOption VisualOption
        {
            get => _visualOption;
            set
            {
                _visualOption = value;
                OnPropertyChanged(nameof(VisualOption));
            }
        }

        /// <summary>
        /// whether a complete replot is required
        /// e.g. when the data values are changed
        /// </summary>
        public bool NeedReplot { get; set; } = false;

        #endregion
        #region constructors


        public ScatPlt1D(double[] dataLocations, double[] dataValues, 
            Color lineColor, double lineWidth, MarkerShape markerShape, double markerSize,
            DrawOption visualOption, string label)
        {
            _dataLocations = dataLocations;
            _dataValues = dataValues;

            _label = label;
            _lineColor = lineColor;
            _lineWidth = lineWidth;
            _markerShape = markerShape;
            _markerSize = markerSize;
            _visualOption = visualOption;
        }


        public ScatPlt1D(Scat1DRealData data,
            Color lineColor, double lineWidth, MarkerShape markerShape, double markerSize,
            DrawOption visualOption, string label) : 
            this(VMath.ConvertVectorToArray(data.Points), 
                VMath.ConvertVectorToArray(data.Values),
                lineColor, lineWidth, markerShape, markerSize, visualOption, label)
        { }

        #endregion
        #region methods

        /// <summary>
        /// updates changes in the plot
        /// </summary>
        public void Update()
        {
            ScPlot.Color = LineColor;
            ScPlot.LineWidth = LineWidth;
            ScPlot.MarkerShape = MarkerShape;
            ScPlot.MarkerSize = (float)MarkerSize;
            ScPlot.Label = Label;

            switch(VisualOption)
            {
                case DrawOption.None:
                    {
                        ScPlot.IsVisible = false;
                        ScPlot.YAxisIndex = 0;
                        break;
                    }
                case DrawOption.YRight:
                    {
                        ScPlot.IsVisible = true;
                        ScPlot.YAxisIndex = 1;
                        break;
                    }
                default:
                    {
                        ScPlot.IsVisible = true;
                        ScPlot.YAxisIndex = 0;
                        break;
                    }
            }

            if(NeedReplot)
            {
                // ...
                NeedReplot = false;
            }
        }


        #endregion

    }
}
