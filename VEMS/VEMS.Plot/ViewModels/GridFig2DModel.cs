using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing.Drawing2D;
using System.Windows.Input;

using ScottPlot;
using ScottPlot.Plottable;
using ScottPlot.Drawing;
using VEMS.MathCore;
using System.Windows;

namespace VEMS.Plot
{
    /// <summary>
    /// model for Fig2D class
    /// </summary>
    public class GridFig2DModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string properyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(properyName));

        #region fields

        /// <summary>
        /// range of the x axis
        /// </summary>
        private RangeValue XAxisRange = new()
        {
            Name = "Horizontal",
            MinValue = -1.0,
            MaxValue = 1.0
        };

        /// <summary>
        /// range of the y axis on the left
        /// which is the default
        /// </summary>
        private RangeValue YAxisRange = new()
        {
            Name = "Vertical (Left)",
            MinValue = 0.0,
            MaxValue = 1.0
        };

        private RangeValue DataValueClipRange = new()
        {
            Name = "Data Value Clip",
            //MinValue = 0.0,
            //MaxValue = 1.0
        };

        /// <summary>
        /// title of the figure(s)
        /// </summary>
        private StringValue FigTitle = new()
        {
            Name = "Figure Title",
            Value = ""
        };

        /// <summary>
        /// label of the x axis
        /// </summary>
        private StringValue FigLabelX = new()
        {
            Name = "Horizontal Label",
            Value = ""
        };

        /// <summary>
        /// label of the y axis on the left
        /// which is the default
        /// </summary>
        private StringValue FigLabelY = new()
        {
            Name = "Vertical Label",
            Value = ""
        };

        private PositionValue CursorLocation = new()
        {
            Name = "Cursor Location",
            X = 0.0,
            Y = 0.0
        };

        private PositionValue CursorPixel = new()
        {
            Name = "Cursor Pixel",
            X = 0.0,
            Y = 0.0,
        };

        private PositionValue NearestSample = new()
        {
            Name = "Nearest Sample",
            X = 0.0,
            Y = 0.0
        };

        #endregion
        #region properties

        private ObservableCollection<RangeValue> _viewRange { get; set; }
        private ObservableCollection<StringValue> _figureInfo { get; set; }
        private ObservableCollection<PositionValue> _monitorInfo { get; set; }
        private int _activeGridPlotIndex { get; set; }
        private int _lastActiveIndex { get; set; }
        private string _activeIndexText { get; set; }
        private bool _lockScale { get; set; } = true;


        /// <summary>
        /// frame of the figure(s)
        /// </summary>
        public WpfPlot Frame { get; set; }

        /// <summary>
        /// colorbar of the selected figure
        /// </summary>
        public Colorbar ColorBar { get; set; }

        /// <summary>
        /// collection of GridPlot
        /// </summary>
        public ObservableCollection<GridPlt2D> GridPlotList { get; set; }

        /// <summary>
        /// view range of the figure(s)
        /// </summary>
        public ObservableCollection<RangeValue> ViewRange
        {
            get => _viewRange;
            set
            {
                _viewRange = value;
                // handling changes
                foreach (RangeValue rng in ViewRange)
                    rng.PropertyChanged += (o, e) =>
                    {
                        UpdateViewRange();
                        Frame.Refresh();
                    };
            }
        }

        /// <summary>
        /// figure information including
        /// title, labels ...
        /// </summary>
        public ObservableCollection<StringValue> FigureInfo
        {
            get => _figureInfo;
            set
            {
                _figureInfo = value;
                // handling changes in each element
                foreach (StringValue str in FigureInfo)
                    str.PropertyChanged += (o, e) =>
                    {
                        UpdateFigureText();
                        Frame.Refresh();
                    };
            }
        }

        /// <summary>
        /// monitor information including
        /// cursor position, pixel ...
        /// </summary>
        public ObservableCollection<PositionValue> MonitorInfo
        {
            get => _monitorInfo;
            set => _monitorInfo = value;
        }

        /// <summary>
        /// selected active GridPlot in the GridPlotlist
        /// </summary>
        public GridPlt2D ActiveGridPlot { get; set; }

        /// <summary>
        /// index of the active GridPlot
        /// </summary>
        public int ActiveGridPlotIndex
        {
            get => _activeGridPlotIndex;
            set
            {
                _activeGridPlotIndex = value;
                OnPropertyChanged(nameof(ActiveGridPlotIndex));
                ActiveGridPlot = GridPlotList[ActiveGridPlotIndex];
                ActiveIndexText = (ActiveGridPlotIndex + 1).ToString() + "/"
                    + GridPlotList.Count.ToString() + " " + ActiveGridPlot.Label;
            }
        }

        /// <summary>
        /// display text for the active plot index
        /// </summary>
        public string ActiveIndexText
        {
            get => _activeIndexText;
            set
            {
                _activeIndexText = value;
                OnPropertyChanged(nameof(ActiveIndexText));
            }
        }

        /// <summary>
        /// whether the scale of the figure is locked
        /// </summary>
        public bool LockScale
        {
            get => _lockScale;
            set
            {
                _lockScale = value;
                OnPropertyChanged(nameof(LockScale));
                Frame.Plot.AxisScaleLock(LockScale);
                Frame.Refresh();
            }
        }

        /// <summary>
        /// list of the names of colormap options
        /// </summary>
        public ObservableCollection<string> ColorMapNameList { get; set; }

        /// <summary>
        /// list of the interpolation method for smooth graph display
        /// </summary>
        public ObservableCollection<InterpolationMode> InterpolationMethodList { get; set; }

        #endregion
        #region constructors

        public GridFig2DModel(WpfPlot frame,
            double[,] data, GridInfo2D grid, string colorMapName,
            string label, string figTitle, string figLabelX, string figLabelY)
        {
            // initialize all
            InitiateAll(frame, figTitle, figLabelX, figLabelY);

            // plot data
            AddGridPlot(data, grid, colorMapName, label);
            ActiveGridPlotIndex = 0;
            ActiveGridPlot.IsVisible = true;
            ActiveGridPlot.ColorBar.IsVisible = true;

            // post-processing
            UpdateFigureText();
            DetectViewRange();
            Frame.RightClicked -= Frame.DefaultRightClickEvent;
            Frame.MouseMove += GetMouseCoordinates;

            // commands
            RelayCommands();
        }

        /// <summary>
        /// constructs a Fig2DModel
        /// for a single input grid matrix data
        /// </summary>
        /// <param name="frame"> figure frame </param>
        /// <param name="data"> data to plot </param>
        /// <param name="colorMapName"> name of the colormap for the graph </param>
        /// <param name="label"> label of the graph </param>
        /// <param name="figTitle"> title of the figure </param>
        /// <param name="figLabelX"> label of the x axis </param>
        /// <param name="figLabelY"> label of the y axis </param>
        public GridFig2DModel(WpfPlot frame,
            Grid2DRealData data, string colorMapName,
            string label, string figTitle,
            string figLabelX, string figLabelY) : 
            this(frame, VMath.ConvertMatrixToArray(data.Values), data.GridInfo, 
                colorMapName, label, figTitle, figLabelX, figLabelY)
        { }
        
        /// <summary>
        /// constructs a Fig2DModel
        /// for a list of input grid matrix data
        /// </summary>
        /// <param name="frame"> figure frame </param>
        /// <param name="data"> data to plot </param>
        /// <param name="colorMapName"> names of the colormaps for the graphs </param>
        /// <param name="label"> labels of the graphs </param>
        /// <param name="figTitle"> title of the figure </param>
        /// <param name="figLabelX"> label of the x axis </param>
        /// <param name="figLabelY"> label of the y axis </param>
        public GridFig2DModel(WpfPlot frame,
            List<Grid2DRealData> data, List<string> colorMapName,
            List<string> label, string figTitle,
            string figLabelX, string figLabelY)
        {
            // initialize all
            InitiateAll(frame, figTitle, figLabelX, figLabelY);

            // handling null inputs
            if (colorMapName == null)
            {
                colorMapName = new List<string>();
                for (int i = 0; i < data.Count; i++)
                    colorMapName.Add(ColorMapNameList[i % ColorMapNameList.Count]);
            }
            if (label == null)
            {
                label = new List<string>();
                for (int i = 0; i < data.Count; i++)
                    label.Add("Graph #" + i.ToString());
            }

            // plot data
            for (int i = 0; i < data.Count; i++)
                AddGridPlot(data[i], colorMapName[i], label[i]);

            ActiveGridPlotIndex = 0;
            ActiveGridPlot.IsVisible = true;
            ActiveGridPlot.ColorBar.IsVisible = true;

            // post-processing
            UpdateFigureText();
            DetectViewRange();
            Frame.RightClicked -= Frame.DefaultRightClickEvent;
            Frame.MouseMove += GetMouseCoordinates;

            // commands
            RelayCommands();
        }

        #endregion
        #region commands

        public ICommand AutoViewRangeCommand { get; set; }
        public ICommand PrevActiveIndexCommand { get; set; }
        public ICommand NextActiveIndexCommand { get; set; }
        public ICommand FuncMinMaxCommand { get; set; }
        public ICommand FuncSumCommand { get; set; }
        public ICommand FuncReverseCommand { get; set; }
        public ICommand FuncFFTCommand { get; set; }


        private void RelayCommands()
        {
            AutoViewRangeCommand = new RelayCommand(DetectViewRange);
            PrevActiveIndexCommand = new RelayCommand(PrevActiveIndex);
            NextActiveIndexCommand = new RelayCommand(NextActiveIndex);
            // figure functions
            FuncMinMaxCommand = new RelayCommand(FuncMinMax);
            FuncSumCommand = new RelayCommand(FuncSum);
            FuncReverseCommand = new RelayCommand(FuncReverse);
        }

        #endregion
        #region methods

        /// <summary>
        /// initializations
        /// </summary>
        /// <param name="frame"> input figure frame </param>
        /// <param name="figTitle"> input figure title </param>
        /// <param name="figLabelX"> input figure label in x direction </param>
        /// <param name="figLabelY"> input figure label in y direction </param>
        private void InitiateAll(WpfPlot frame,
            string figTitle, string figLabelX, string figLabelY)
        {
            // color maps
            ColorMapNameList = new();
            string[] cmNames = Colormap.GetColormapNames();
            foreach (string cmName in cmNames)
                ColorMapNameList.Add(cmName);

            // interpolation methods
            InterpolationMethodList = new();
            foreach (InterpolationMode itp in Enum.GetValues(typeof(InterpolationMode)))
                InterpolationMethodList.Add(itp);

            // setup input items
            Frame = frame;
            FigTitle.Value = figTitle;
            FigLabelX.Value = figLabelX;
            FigLabelY.Value = figLabelY;

            // construct the figure elements
            GridPlotList = new();
            ViewRange = new() { XAxisRange, YAxisRange };
            FigureInfo = new() { FigTitle, FigLabelX, FigLabelY };
            MonitorInfo = new() { CursorLocation, CursorPixel };
        }

        /// <summary>
        /// finds the Cursor coordinates in the figure frame
        /// </summary>
        /// <param name="sender"> sender </param>
        /// <param name="e"> event </param>
        private void GetMouseCoordinates(object sender, MouseEventArgs e)
        {
            (double x, double y) = Frame.GetMouseCoordinates();
            (float px, float py) = Frame.GetMousePixel();

            CursorLocation.X = x;
            CursorLocation.Y = y;

            CursorPixel.X = px;
            CursorPixel.Y = py;

            //HighLightPoint.X = x;
            //HighLightPoint.Y = y;
            //Frame.Refresh();
        }

        /// <summary>
        /// adds a new plot to the frame and the list
        /// </summary>
        /// <param name="data"> data to plot in a double array</param>
        /// <param name="grid"> sampling grid information </param>
        /// <param name="colorMapName"> colormap for the plot </param>
        /// <param name="label"> label of the plot </param>
        private void AddGridPlot(double[,] data, GridInfo2D grid,
            string colorMapName, string label)
        {
            // generate heatmap plot
            GridPlt2D plt = new(data, grid, colorMapName, label);
            plt.HmPlot = Frame.Plot.AddHeatmap(plt.PlotData, plt.ColorMap);
            plt.ColorBar = Frame.Plot.AddColorbar(plt.HmPlot);
            plt.Update();
            // settings
            plt.IsVisible = false;
            plt.EnableValueClip = false;
            plt.ColorBar.IsVisible = false;

            // property changed event handling
            plt.PropertyChanged += (o, e) =>
            {
                plt.Update();
                Frame.Refresh();
            };

            // add to ...
            GridPlotList.Add(plt);
        }

        /// <summary>
        /// adds a new plot to the frame and the list
        /// </summary>
        /// <param name="data"> plot data </param>
        /// <param name="colorMapName"> colormap for the plot </param>
        /// <param name="label"> label of the plot </param>
        private void AddGridPlot(Grid2DRealData data,
            string colorMapName, string label) =>
            AddGridPlot(VMath.ConvertMatrixToArray(data.Values),
                data.GridInfo, colorMapName, label);

        /// <summary>
        /// updates the view range
        /// </summary>
        private void UpdateViewRange()
        {
            // range
            Frame.Plot.SetAxisLimits(XAxisRange.MinValue, XAxisRange.MaxValue,
                YAxisRange.MinValue, YAxisRange.MaxValue, 0, 0);
            // scale
            //Frame.Plot.AxisScaleLock(false);
        }

        /// <summary>
        /// updates the figure text
        /// title, labels for x, y, y2 axes
        /// </summary>
        private void UpdateFigureText()
        {
            Frame.Plot.Title(FigTitle.Value);
            Frame.Plot.XAxis.Label(FigLabelX.Value);
            Frame.Plot.YAxis.Label(FigLabelY.Value);
        }

        /// <summary>
        /// detects the view range automatically
        /// for all the plots
        /// </summary>
        public void DetectViewRange()
        {
            Frame.Plot.AxisAuto(0.0, 0.0);

            AxisLimits al = Frame.Plot.GetAxisLimits(0, 0);
            XAxisRange.MinValue = al.XMin;
            XAxisRange.MaxValue = al.XMax;
            YAxisRange.MinValue = al.YMin;
            YAxisRange.MaxValue = al.YMax;

            Frame.Refresh();
        }

        /// <summary>
        /// goes to the previous plot index
        /// </summary>
        private void PrevActiveIndex()
        {
            if (ActiveGridPlotIndex > 0)
            {
                // handle the last active one
                ActiveGridPlot.IsVisible = false;
                ActiveGridPlot.ColorBar.IsVisible = false;
                _lastActiveIndex = ActiveGridPlotIndex;

                // change active index
                ActiveGridPlotIndex--;
                ActiveGridPlot.IsVisible = true;
                ActiveGridPlot.ColorBar.IsVisible = true;

                Frame.Refresh();
            }
        }

        /// <summary>
        /// goes to the next plot index
        /// </summary>
        private void NextActiveIndex()
        {
            if (ActiveGridPlotIndex < GridPlotList.Count - 1)
            {
                // handle the last active one
                ActiveGridPlot.IsVisible = false;
                ActiveGridPlot.ColorBar.IsVisible = false;
                _lastActiveIndex = ActiveGridPlotIndex;

                // change active index
                ActiveGridPlotIndex++;
                ActiveGridPlot.IsVisible = true;
                ActiveGridPlot.ColorBar.IsVisible = true;

                Frame.Refresh();
            }
        }

        #endregion
        #region functions

        private void FuncMinMax()
        {
            for(int i = 0; i < GridPlotList.Count; i++)
            {
                //MatrixD m = GridPlotList[i].Data.Values;
                (_, double min, _, double max) = VMath.IndexMinMax(GridPlotList[i].PlotData);
                MessageBox.Show("Min. value = " + min.ToString() + "\n"
                    + "Max. value = " + max.ToString());
            }
        }

        private void FuncSum()
        {
            for (int i = 0; i < GridPlotList.Count; i++)
            {
                //double s = VMath.Sum(GridPlotList[i].Data.Values);
                //MessageBox.Show("sum of plot [" + i.ToString() + "]: " + s.ToString());
            }
        }

        private void FuncReverse()
        {
            for(int i =0; i<GridPlotList.Count; i++)
            {
                //Grid2DRealData d = GridPlotList[i].Data;
                //GridPlotList[i].Data = new Grid2DRealData(d.Grid, -1.0 * d.Values);
                //GridPlotList[i].Update();
            }
            Frame.Refresh();
        }

        #endregion
    }
}
