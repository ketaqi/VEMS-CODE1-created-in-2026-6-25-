using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

using VEMS.MathCore;
using ScottPlot.Plottable;
using ScottPlot.Drawing;
using System.Drawing.Drawing2D;
using System.Windows;
using System.ComponentModel.DataAnnotations;

namespace VEMS.Plot
{
    /// <summary>
    /// GridPlt2D class
    /// </summary>
    public class GridPlt2D : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string properyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(properyName));

        #region properties


        private Grid2DRealData _data { get; set; }
        private double[,] _plotData { get; set; }
        private GridInfo2D _grid { get; set; }
        private string _label { get; set; }
        private string _colorMapName { get; set; }
        private bool _enableValueClip { get; set; }
        private double _clipMaxValue { get; set; }
        private double _clipMinValue { get; set; }
        private InterpolationMode _interpolationMethod { get; set; }
        private bool _isVisible { get; set; }


        /// <summary>
        /// Grid MatrixD that contains the data info
        /// </summary>
        //public Grid2DRealData Data
        //{
        //    get => _data;
        //    set
        //    {
        //        _data = value;
        //        NeedReplot = true;
        //        PlotData = VMath.ConvertMatrixToArray(Data.Values, true, false);
        //    }
        //}


        /// <summary>
        /// double array that contains the data to plot
        /// </summary>
        public double[,] PlotData
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
        public GridInfo2D Grid
        {
            get => _grid;
            set => _grid = value;
        }

        /// <summary>
        /// minimum in data values
        /// </summary>
        private double MinValue { get; set; }

        /// <summary>
        /// maximum in data values
        /// </summary>
        private double MaxValue { get; set; }

        /// <summary>
        /// actual double array used for the plot
        /// </summary>
        //public double[,] PlotData { get; set; }

        /// <summary>
        /// Heatmap plot from ScottPlot
        /// </summary>
        public Heatmap HmPlot { get; set; }

        /// <summary>
        /// label of the graph
        /// </summary>
        public string Label
        {
            get => _label;
            set
            {
                _label = value;
                OnPropertyChanged(Label);
            }
        }

        /// <summary>
        /// coordinate of the first grid point in x 
        /// </summary>
        public double StartX
        {
            get => _grid.StartX;
            set
            {
                _grid.StartX = value;
                OnPropertyChanged(nameof(StartX));
            }
        }

        /// <summary>
        /// coordinate of the first grid point in y
        /// </summary>
        public double StartY
        {
            get => _grid.StartY;
            set
            {
                _grid.StartY = value;
                OnPropertyChanged(nameof(StartY));
            }
        }

        /// <summary>
        /// spacing between two adjacent grid points in x
        /// </summary>
        public double SpacingX
        {
            get => _grid.SpacingX;
            set
            {
                _grid.SpacingX = value;
                OnPropertyChanged(nameof(SpacingX));
            }
        }

        /// <summary>
        /// spacing between two adjacent grid points in y
        /// </summary>
        public double SpacingY
        {
            get => _grid.SpacingY;
            set
            {
                _grid.SpacingY = value;
                OnPropertyChanged(nameof(SpacingY));
            }
        }

        /// <summary>
        /// number of grid points in x 
        /// </summary>
        public long Cols
        {
            get => _grid.Cols; 
            set
            {
                _grid.Cols = value; 
                OnPropertyChanged(nameof(Cols));
            }
        }

        /// <summary>
        /// number of grid points in y
        /// </summary>
        public long Rows 
        {
            get => _grid.Rows;
            set
            {
                _grid.Rows = value; 
                OnPropertyChanged(nameof(Rows));
            }
        }

        /// <summary>
        /// colormap for the graph
        /// </summary>
        public Colormap ColorMap { get; set; }

        /// <summary>
        /// name of the colormap
        /// </summary>
        public string ColorMapName
        {
            get => _colorMapName;
            set
            {
                _colorMapName = value;
                ColorMap = Colormap.GetColormapByName(ColorMapName, false);
                NeedReplot = true;
                OnPropertyChanged(ColorMapName);
            }
        }

        /// <summary>
        /// whether enable the value clipping
        /// </summary>
        public bool EnableValueClip
        {
            get => _enableValueClip;
            set
            {
                _enableValueClip = value;
                if (!EnableValueClip)
                {
                    ClipMinValue = MinValue;
                    ClipMaxValue = MaxValue;
                }
                OnPropertyChanged(nameof(EnableValueClip));
            }
        }

        /// <summary>
        /// the maximum clip value
        /// </summary>
        public double ClipMaxValue
        {
            get => _clipMaxValue;
            set
            {
                _clipMaxValue = value;
                NeedReplot = true;
                OnPropertyChanged(nameof(ClipMaxValue));
            }
        }

        /// <summary>
        /// the minimum clip value
        /// </summary>
        public double ClipMinValue
        {
            get => _clipMinValue;
            set
            {
                _clipMinValue = value;
                NeedReplot = true;
                OnPropertyChanged(nameof(ClipMinValue));
            }
        }

        /// <summary>
        /// colorbar 
        /// </summary>
        public Colorbar ColorBar { get; set; }

        /// <summary>
        /// interpolation method for smooth graph display
        /// </summary>
        public InterpolationMode InterpolationMethod
        {
            get => _interpolationMethod;
            set
            {
                _interpolationMethod = value;
                OnPropertyChanged(nameof(InterpolationMethod));
            }
        }

        /// <summary>
        /// whether the graph is visible
        /// </summary>
        public bool IsVisible
        {
            get => _isVisible;
            set
            {
                _isVisible = value;
                HmPlot.IsVisible = IsVisible;
                OnPropertyChanged(nameof(IsVisible));
            }
        }

        /// <summary>
        /// whether a complete replot is needed
        /// e.g. when the data changes
        /// </summary>
        public bool NeedReplot { get; set; }

        #endregion
        #region constructor

        /// <summary>
        /// constructs a GridPlt2D
        /// shall be used together with Plot.AddSignal(...)
        /// </summary>
        /// <param name="data"> data in a double array </param>
        /// <param name="grid"> sampling grid information </param>
        /// <param name="colorMapName"> name of colormap </param>
        /// <param name="label"> label text </param>
        public GridPlt2D(double[,] data, GridInfo2D grid,
            string colorMapName, string label)
        {
            _plotData = data;
            _grid = grid;
            (_, MinValue, _, MaxValue) = VMath.IndexMinMax(PlotData);
            double[] a = new double[] { 0.0, 0.1};
            a.Max();

            // properties to expose
            StartX = _grid.StartX;
            StartY = _grid.StartY;
            SpacingX = _grid.SpacingX;
            SpacingY = _grid.SpacingY;
            Rows = _grid.Rows;
            Cols = _grid.Cols;
            _label = label;
            _colorMapName = colorMapName;
            _interpolationMethod = InterpolationMode.NearestNeighbor;
        }

        /// <summary>
        /// constructs a GridPlt2D
        /// shall be used together with Plot.AddSignal(...)
        /// </summary>
        /// <param name="data"> data in a grid vector </param>
        /// <param name="colorMapName"> name of colormap </param>
        /// <param name="label"> label text </param>
        public GridPlt2D(Grid2DRealData data,
            string colorMapName, string label) : 
            this(VMath.ConvertMatrixToArray(data.Values), data.GridInfo,
                colorMapName, label)
        { }

        #endregion
        #region methods

        /// <summary>
        /// updates the plot
        /// </summary>
        public void Update()
        {

            HmPlot.CellWidth = SpacingX;
            HmPlot.CellHeight = SpacingY;

            HmPlot.OffsetX = _grid.LowerBoundX;
            HmPlot.OffsetY = _grid.LowerBoundY;

            HmPlot.Smooth = true;
            HmPlot.Interpolation = InterpolationMethod;

            if (NeedReplot)
            {

                HmPlot.Update(PlotData, 
                    ColorMap, 
                    min: ClipMinValue, max: ClipMaxValue);

                ColorBar.UpdateColormap(ColorMap);
                NeedReplot = false;
            }

            // value ticks
            double min = ColorBar.MinValue;
            double max = ColorBar.MaxValue;
            double avg = 0.5 * (max + min);
            ColorBar.SetTicks(
                new double[] { 0.0, 0.5, 1.0 },
                new string[] {
                    Converter.NumberToString(min),
                    Converter.NumberToString(avg),
                    Converter.NumberToString(max)
                });
        }

        #endregion

    }

}
