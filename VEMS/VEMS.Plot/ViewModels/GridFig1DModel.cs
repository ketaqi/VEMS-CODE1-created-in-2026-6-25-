using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using System.Drawing;

using ScottPlot;
using ScottPlot.Plottable;

using VEMS.MathCore;
using System.Runtime.CompilerServices;
using System.Windows.Media.Animation;

namespace VEMS.Plot
{

    /// <summary>
    /// model for Fig1D class
    /// </summary>
    public class GridFig1DModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string properyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(properyName));

        #region fields

        /// <summary>
        /// range of the x axis
        /// </summary>
        public RangeValue XAxisRange = new()
        {
            Name = "Horizontal",
            MinValue = -1.0,
            MaxValue = 1.0,

        };

        /// <summary>
        /// range of the y axis on the left
        /// which is the default
        /// </summary>
        public RangeValue YAxisRange = new()
        {
            Name = "Vertical (Left)",
            MinValue = 0.0,
            MaxValue = 1.0
        };

        /// <summary>
        /// range of the y axis on the right
        /// only when it is used
        /// </summary>
        public RangeValue YRightAxisRange = new()
        {
            Name = "Vertical (Right)",
            MinValue = 0.0,
            MaxValue = 1.0
        };

        /// <summary>
        /// title of the figure(s)
        /// </summary>
        public StringValue FigTitle = new()
        {
            Name = "Figure Title",
            Value = ""
        };

        /// <summary>
        /// label of the x axis
        /// </summary>
        public StringValue FigLabelX = new()
        {
            Name = "Horizontal Label",
            Value = ""
        };

        /// <summary>
        /// label of the y axis on the left
        /// which is the default
        /// </summary>
        public StringValue FigLabelY = new()
        {
            Name = "Vertical Label",
            Value = ""
        };

        /// <summary>
        /// label of the y axis on the right
        /// only when it is used
        /// </summary>
        public StringValue FigLabelYRight = new()
        {
            Name = "Vertical Label (right)",
            Value = "function value (right)"
        };

        /// <summary>
        /// cursor location values, given in x and y
        /// </summary>
        public PositionValue CursorLocation = new()
        {
            Name = "Cursor Location",
            X = 0.0,
            Y = 0.0
        };

        /// <summary>
        /// cursor pixel locations, given in x and y
        /// </summary>
        public PositionValue CursorPixel = new()
        {
            Name = "Cursor Pixel",
            X = 0.0,
            Y = 0.0,
        };

        /// <summary>
        /// location of the nearest sample
        /// not enabled ... 
        /// </summary>
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
        private int _activeGridPlotIndex { get; set; }
        private string _activeIndexText { get; set; }
        private MarkerPlot _highLightPoint { get; set; }
        private bool _needRightYAxis { get; set; } = true;
        private bool _needLeftYAxis { get; set; } = true;
        private bool _showLegend { get; set; } = true;
        private Alignment _legendLocation { get; set; }


        /// <summary>
        /// frame of the figure(s)
        /// </summary>
        public WpfPlot Frame { get; set; }

        /// <summary>
        /// collection of GridPlot
        /// </summary>
        public ObservableCollection<GridPlt1D> GridPlotList { get; set; }

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
        public ObservableCollection<PositionValue> MonitorInfo { get; set; }

        /// <summary>
        /// selected active GridPlot in the GridPlotlist
        /// </summary>
        public GridPlt1D ActiveGridPlot { get; set; }

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

        // ...
        public MarkerPlot HighLightPoint
        {
            get => _highLightPoint;
            set
            {
                _highLightPoint = value;
                OnPropertyChanged(nameof(HighLightPoint));
            }
        }

        /// <summary>
        /// whether the right y axis is needed
        /// </summary>
        public bool NeedRightYAxis
        {
            get => _needRightYAxis;
            set
            {
                _needRightYAxis = value;
                OnPropertyChanged(nameof(NeedRightYAxis));
                if (NeedRightYAxis)
                {
                    if (!ViewRange.Contains(YRightAxisRange))
                        ViewRange.Add(YRightAxisRange);
                    if (!FigureInfo.Contains(FigLabelYRight))
                        FigureInfo.Add(FigLabelYRight);

                    Frame.Plot.YAxis2.Label(FigLabelYRight.Value);
                    Frame.Plot.YAxis2.Ticks(true);
                    Frame.Plot.YAxis2.Grid(true);
                }
                else
                {
                    if (ViewRange.Contains(YRightAxisRange))
                        ViewRange.Remove(YRightAxisRange);
                    if (FigureInfo.Contains(FigLabelYRight))
                        FigureInfo.Remove(FigLabelYRight);

                    Frame.Plot.YAxis2.Label("");
                    Frame.Plot.YAxis2.Ticks(false);
                    Frame.Plot.YAxis2.Grid(false);
                }
            }
        }

        /// <summary>
        /// whether the left y axis is needed
        /// </summary>
        public bool NeedLeftYAxis
        {
            get => _needLeftYAxis;
            set
            {
                _needLeftYAxis = value;
                OnPropertyChanged(nameof(NeedLeftYAxis));
                if (NeedLeftYAxis)
                {
                    if (!ViewRange.Contains(YAxisRange))
                        ViewRange.Insert(1, YAxisRange);
                    if (!FigureInfo.Contains(FigLabelY))
                        FigureInfo.Insert(2, FigLabelY);

                    Frame.Plot.YAxis.Label(FigLabelY.Value);
                    Frame.Plot.YAxis.Ticks(true);
                    Frame.Plot.YAxis.Grid(true);
                }
                else
                {
                    if (ViewRange.Contains(YAxisRange))
                        ViewRange.Remove(YAxisRange);
                    if (FigureInfo.Contains(FigLabelY))
                        FigureInfo.Remove(FigLabelY);

                    Frame.Plot.YAxis.Label("");
                    Frame.Plot.YAxis.Ticks(false);
                    Frame.Plot.YAxis.Grid(false);
                }
            }
        }

        /// <summary>
        /// whether the figure legend is shown
        /// </summary>
        public bool ShowLegend
        {
            get => _showLegend;
            set
            {
                _showLegend = value;
                OnPropertyChanged(nameof(ShowLegend));
                Frame.Plot.Legend(ShowLegend, LegendLocation);
                Frame.Refresh();
            }
        }

        /// <summary>
        /// location of the figure legend
        /// </summary>
        public Alignment LegendLocation
        {
            get => _legendLocation;
            set
            {
                _legendLocation = value;
                OnPropertyChanged(nameof(LegendLocation));
                Frame.Plot.Legend(ShowLegend, LegendLocation);
                Frame.Refresh();
            }
        }


        public ObservableCollection<Color> LineColorList { get; set; }
        public ObservableCollection<DrawOption> DrawOptionList { get; set; }
        public ObservableCollection<Alignment> LegendLocationList { get; set; }

        #endregion
        #region constructors

        public GridFig1DModel(WpfPlot frame,
            double[] data, GridInfo1D grid, 
            Color lineColor, double lineWidth, double markerSize,
            string label, string figTitle, 
            string figLabelX, string figLabelY)
        {
            // initialize all
            InitiateAll(frame, figTitle, figLabelX, figLabelY);

            // plot data
            AddGridPlot(data, grid, lineColor,
                lineWidth, markerSize, DrawOption.YLeft, label);
            ActiveGridPlotIndex = 0;

            // post-processing
            UpdateFigureText();
            CheckYAxesNeeds();
            DetectViewRange();
            Frame.RightClicked -= Frame.DefaultRightClickEvent;
            Frame.MouseMove += GetMouseCoordinates;

            // commands
            RelayCommands();
        }

        /// <summary>
        /// constructs a Fig1DModel
        /// for a single input grid vector data
        /// </summary>
        /// <param name="frame"> figure frame </param>
        /// <param name="data"> data to plot </param>
        /// <param name="lineColor"> color of the line </param>
        /// <param name="lineWidth"> width of the line </param>
        /// <param name="markerSize"> size of the markers </param>
        /// <param name="label"> label of the line </param>
        /// <param name="figTitle"> title of the figure(s) </param>
        /// <param name="figLabelX"> label of the x axis </param>
        /// <param name="figLabelY"> label of the y axis </param>
        public GridFig1DModel(WpfPlot frame,
            Grid1DRealData data, Color lineColor,
            double lineWidth, double markerSize,
            string label, string figTitle,
            string figLabelX, string figLabelY) :
            this(frame, VMath.ConvertVectorToArray(data.Values), data.GridInfo,
                lineColor, lineWidth, markerSize, label, 
                figTitle, figLabelX, figLabelY)
        { }

        /// <summary>
        /// constructs a Fig1DModel
        /// for a list of input grid vector data
        /// </summary>
        /// <param name="frame"> figure frame </param>
        /// <param name="data"> data to plot </param>
        /// <param name="lineColor"> color of the line </param>
        /// <param name="lineWidth"> width of the line </param>
        /// <param name="markerSize"> size of the markers </param>
        /// <param name="label"> label of the line </param>
        /// <param name="figTitle"> title of the figure(s) </param>
        /// <param name="figLabelX"> label of the x axis </param>
        /// <param name="figLabelY"> label of the y axis </param>
        public GridFig1DModel(WpfPlot frame,
            List<Grid1DRealData> data, List<Color> lineColor,
            double lineWidth, double markerSize,
            List<string> label, string figTitle,
            string figLabelX, string figLabelY)
        {
            // initialize all
            InitiateAll(frame, figTitle, figLabelX, figLabelY);

            // handling null inputs
            if (lineColor == null)
            {
                lineColor = new List<Color>();
                for (int i = 0; i < data.Count; i++)
                    lineColor.Add(LineColorList[i % LineColorList.Count]);
            }
            if (label == null)
            {
                label = new List<string>();
                for (int i = 0; i < data.Count; i++)
                    label.Add("Line #" + i.ToString());
            }

            // plot data
            for (int i = 0; i < data.Count; i++)
                AddGridPlot(data[i], LineColorList[i],
                    lineWidth, markerSize, DrawOption.YLeft, label[i]);
            ActiveGridPlotIndex = 0;

            // post-processing
            UpdateFigureText();
            CheckYAxesNeeds();
            DetectViewRange();
            Frame.RightClicked -= Frame.DefaultRightClickEvent;
            Frame.MouseMove += GetMouseCoordinates;

            // commands
            RelayCommands();
        }

        #endregion
        #region commands

        /// <summary>
        /// auto view range command
        /// </summary>
        public ICommand AutoViewRangeCommand { get; set; }
        public ICommand PrevActiveIndexCommand { get; set; }
        public ICommand NextActiveIndexCommand { get; set; }
        private void RelayCommands()
        {
            AutoViewRangeCommand = new RelayCommand(DetectViewRange);
            PrevActiveIndexCommand = new RelayCommand(PrevActiveIndex);
            NextActiveIndexCommand = new RelayCommand(NextActiveIndex);
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
            // line colors
            LineColorList = new()
            {
                Color.SteelBlue,
                Color.Gray,
                Color.Beige,
                Color.Tomato,
                Color.DarkOliveGreen,
                Color.SlateBlue,
                Color.Linen,
                Color.LimeGreen,
                Color.LightCoral,
                Color.LemonChiffon,
                Color.Lavender,
                Color.Black,
                Color.Aquamarine,
                Color.Bisque,
                Color.Brown,
                Color.BurlyWood,
                Color.Chocolate,
                Color.Sienna,
                Color.DarkOrange,
                Color.DeepPink,
                Color.Orchid
            };
            //foreach (KnownColor color in Enum.GetValues(typeof(KnownColor)))
            //{
            //    Color col = Color.FromKnownColor(color);
            //    LineColorList.Add(col);
            //}
            // draw options
            DrawOptionList = new();
            foreach (DrawOption d in Enum.GetValues(typeof(DrawOption)))
                DrawOptionList.Add(d);
            // legend locations
            LegendLocationList = new();
            foreach (Alignment a in Enum.GetValues(typeof(Alignment)))
                LegendLocationList.Add(a);

            // setup input items
            Frame = frame;
            FigTitle.Value = figTitle;
            FigLabelX.Value = figLabelX;
            FigLabelY.Value = figLabelY;
            // construct the figure elements
            GridPlotList = new();
            ViewRange = new() { XAxisRange, YAxisRange, YRightAxisRange };
            FigureInfo = new() { FigTitle, FigLabelX, FigLabelY, FigLabelYRight };
            //NeedLeftYAxis = true; // set to true by initialization and check later
            //NeedRightYAxis = true; // set to true bu initialization and check later
            //ShowLegend = true;
            LegendLocation = Alignment.LowerRight;
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
        /// adds a GridPlot into the frame
        /// </summary>
        /// <param name="data"> data to plot </param>
        /// <param name="grid"> sampling grid information </param>
        /// <param name="lineColor"> color of the line </param>
        /// <param name="lineWidth"> width of the line </param>
        /// <param name="markerSize"> marker size of the data points </param>
        /// <param name="visualOption"> visual option of the plot </param>
        /// <param name="label"> label of the line</param>
        public void AddGridPlot(double[] data, GridInfo1D grid,
            Color lineColor,
            double lineWidth, double markerSize,
            DrawOption visualOption, string label)
        {
            // generate a GridPlot
            GridPlt1D plt = new(data, grid, lineColor, lineWidth, markerSize,
                visualOption, label);
            plt.SgPlot = Frame.Plot.AddSignal(plt.PlotData);
            plt.Update();

            // property changed event handling
            plt.PropertyChanged += (o, e) =>
            {
                plt.Update();
                CheckYAxesNeeds();
                Frame.Refresh();
            };

            // add to ...
            //Frame.Plot.Add(plt.SgPlot);
            GridPlotList.Add(plt);
        }

        /// <summary>
        /// adds a GridPlot into the frame
        /// </summary>
        /// <param name="data"> data to plot </param>
        /// <param name="lineColor"> color of the line </param>
        /// <param name="lineWidth"> width of the line </param>
        /// <param name="markerSize"> marker size of the data points </param>
        /// <param name="visualOption"> visual option of the plot </param>
        /// <param name="label"> label of the line</param>
        public void AddGridPlot(Grid1DRealData data,
            Color lineColor,
            double lineWidth, double markerSize,
            DrawOption visualOption, string label) =>
            AddGridPlot(VMath.ConvertVectorToArray(data.Values),
                data.GridInfo, lineColor, lineWidth, markerSize,
                visualOption, label);

        public void ClearAllPlots()
            => Frame.Plot.Clear();

        /// <summary>
        /// updates the view range
        /// </summary>
        private void UpdateViewRange()
        {
            if (NeedLeftYAxis)
                Frame.Plot.SetAxisLimits(XAxisRange.MinValue, XAxisRange.MaxValue,
                    YAxisRange.MinValue, YAxisRange.MaxValue, 0, 0);
            if (NeedRightYAxis)
                Frame.Plot.SetAxisLimits(XAxisRange.MinValue, XAxisRange.MaxValue,
                    YRightAxisRange.MinValue, YRightAxisRange.MaxValue, 0, 1);
        }

        /// <summary>
        /// updates the figure text
        /// title, labels for x, y, y2 axes
        /// </summary>
        public void UpdateFigureText()
        {
            Frame.Plot.Title(FigTitle.Value);
            Frame.Plot.XAxis.Label(FigLabelX.Value);
            if (NeedLeftYAxis)
                Frame.Plot.YAxis.Label(FigLabelY.Value);
            if (NeedRightYAxis)
                Frame.Plot.YAxis2.Label(FigLabelYRight.Value);
        }

        /// <summary>
        /// checks if the Y axes are needed
        /// </summary>
        private void CheckYAxesNeeds()
        {
            bool needYLeft = false;
            bool needYRight = false;

            for (int i = 0; i < GridPlotList.Count; i++)
            {
                if (GridPlotList[i].VisualOption == DrawOption.YLeft)
                    needYLeft = true;
                else if (GridPlotList[i].VisualOption == DrawOption.YRight)
                    needYRight = true;
            }

            if (NeedLeftYAxis != needYLeft)
                NeedLeftYAxis = needYLeft;
            if (NeedRightYAxis != needYRight)
                NeedRightYAxis = needYRight;

        }

        /// <summary>
        /// detects the view range automatically
        /// for all the plots
        /// </summary>
        private void DetectViewRange()
        {
            Frame.Plot.AxisAuto();

            if (NeedLeftYAxis)
            {
                AxisLimits al00 = Frame.Plot.GetAxisLimits(0, 0);
                XAxisRange.MinValue = al00.XMin;
                XAxisRange.MaxValue = al00.XMax;
                YAxisRange.MinValue = al00.YMin;
                YAxisRange.MaxValue = al00.YMax;
            }

            if (NeedRightYAxis)
            {
                Frame.Plot.AxisAuto();
                AxisLimits al01 = Frame.Plot.GetAxisLimits(0, 1);
                XAxisRange.MinValue = al01.XMin;
                XAxisRange.MaxValue = al01.XMax;
                YRightAxisRange.MinValue = al01.YMin;
                YRightAxisRange.MaxValue = al01.YMax;
            }

            Frame.Refresh();
        }

        /// <summary>
        /// goes to the previous plot index
        /// </summary>
        private void PrevActiveIndex()
        {
            if (ActiveGridPlotIndex > 0)
                ActiveGridPlotIndex--;
        }

        /// <summary>
        /// goes to the next plot index
        /// </summary>
        private void NextActiveIndex()
        {
            if (ActiveGridPlotIndex < GridPlotList.Count - 1)
                ActiveGridPlotIndex++;
        }

        #endregion

    }

}
