using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

using VEMS.MathCore;
using ScottPlot.Plottable;
using System.Drawing;

namespace VEMS.Plot
{
    /// <summary>
    /// Gridded Plot 1D (GridPlt1D) class
    /// </summary>
    public class GridPlt1D : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        #region properties

        private double[] _plotData { get; set; }
        private GridInfo1D _grid { get; set; }
        private string _label { get; set; }
        private double _lineWidth { get; set; }
        private double _markerSize { get; set; }
        private Color _lineColor { get; set; }
        private DrawOption _visualOption { get; set; }


        /// <summary>
        /// actual double array used for the plot
        /// </summary>
        public double[] PlotData 
        {
            get => _plotData;
            set
            {
                _plotData = value;
                NeedReplot = true;
            }
        }

        /// <summary>
        /// sampling grid information
        /// </summary>
        public GridInfo1D Grid
        {
            get => _grid;
            set => _grid = value;
        }

        /// <summary>
        /// SignalPlot from ScottPlot
        /// (the actual plot object)
        /// </summary>
        public SignalPlot SgPlot { get; set; }

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
        /// coordinate of the first grid point
        /// </summary>
        public double Start
        {
            get => _grid.Start;
            set
            {
                _grid.Start = value;
                OnPropertyChanged(nameof(Start));
            }
        }

        /// <summary>
        /// spacing between two adjacent grid points
        /// </summary>
        public double Spacing
        {
            get => _grid.Spacing;
            set
            {
                _grid.Spacing = value;
                OnPropertyChanged(nameof(Spacing));
            }
        }

        /// <summary>
        /// the total number of grid points
        /// </summary>
        public long Points
        {
            get => _grid.Count;
            //set
            //{
            //    _grid.Count = value;
            //    OnPropertyChanged(nameof(Points));
            //}
        }

        /// <summary>
        /// width of the line
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
        /// e.g. when the data changes
        /// </summary>
        public bool NeedReplot { get; set; } = false;

        #endregion
        #region constructor

        /// <summary>
        /// construcst a GridPlt1D 
        /// shall be used together with Plot.AddSignal(...)
        /// </summary>
        /// <param name="data"> data in a double array </param>
        /// <param name="grid"> sampling grid information </param>
        /// <param name="lineColor"> line color </param>
        /// <param name="lineWidth"> line width </param>
        /// <param name="markerSize"> marker size </param>
        /// <param name="visualOption"> visibility option </param>
        /// <param name="label"></param>
        public GridPlt1D(double[] data, GridInfo1D grid,
            Color lineColor,
            double lineWidth, double markerSize,
            DrawOption visualOption, string label)
        {
            _plotData = data;
            _grid = grid;
            // SgPlot ...
            // properties to expose
            Start = grid.Start;
            Spacing = grid.Spacing;
            //Points = grid.Count;
            _label = label;
            _lineColor = lineColor;
            _lineWidth = lineWidth;
            _markerSize = markerSize;
            _visualOption = visualOption;
        }

        /// <summary>
        /// construcst a GridPlt1D 
        /// shall be used together with Plot.AddSignal(...)
        /// </summary>
        /// <param name="data"> data in a grid vector </param>
        /// <param name="lineColor"> line color </param>
        /// <param name="lineWidth"> line width </param>
        /// <param name="markerSize"> marker size </param>
        /// <param name="visualOption"> visibility option </param>
        /// <param name="label"> label text </param>
        public GridPlt1D(Grid1DRealData data,
            Color lineColor,
            double lineWidth, double markerSize,
            DrawOption visualOption, string label) :
            this(VMath.ConvertVectorToArray(data.Values),
                data.GridInfo, lineColor, lineWidth, markerSize,
                visualOption, label)
        { }

        #endregion
        #region methods

        /// <summary>
        /// updates changes in the plot 
        /// </summary>
        public void Update()
        {
            SgPlot.SamplePeriod = Spacing;
            SgPlot.OffsetX = Start;

            SgPlot.Color = LineColor;
            SgPlot.LineWidth = LineWidth;
            SgPlot.MarkerSize = (float)MarkerSize;
            SgPlot.Label = Label;

            switch (VisualOption)
            {
                case DrawOption.None:
                    {
                        SgPlot.IsVisible = false;
                        SgPlot.YAxisIndex = 0;
                        break;
                    }
                case DrawOption.YRight:
                    {
                        SgPlot.IsVisible = true;
                        SgPlot.YAxisIndex = 1;
                        break;
                    }
                default:
                    {
                        SgPlot.IsVisible = true;
                        SgPlot.YAxisIndex = 0;
                        break;
                    }
            }

            if (NeedReplot)
            {
                SgPlot.Update(PlotData);
                NeedReplot = false;
            }
        }


        #endregion

    }

}
