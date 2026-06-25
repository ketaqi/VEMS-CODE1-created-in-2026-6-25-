using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Xml.Linq;
using VEMS.MathCore;

namespace VEMS.Plot
{
    internal class PlotMenuViewModel : INotifyPropertyChanged
    {
        #region PropertyChanged ...
        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        #endregion

        private FrameLite frame { get; set; }
        public FrameLite Frame
        {
            get => frame;
            set
            {
                frame = value;
            }
        } 

        private string figureTitle { get; set; }
        public string FigureTitle
        {
            get => figureTitle;
            set
            {
                figureTitle = value;
                OnPropertyChanged(nameof(FigureTitle));
            }
        }


        public PlotMenuViewModel() 
        {
            //Config.SetMathLibsDirectory();
           
        }


        #region methods


        private void GridFig1D()
        {
            Grid1DRealData a = new(17, 0.5, 0.0);
            for (long i = 0; i < a.Values.Count; i++)
            {
                var x = a.GridInfo.GetCoordinate(i);
                a.Values[i] = Math.Cos(x);
            }
            Figure.Show(a);
        }
        public ICommand GridFig1DCommand
            => new RelayCommand(GridFig1D);


        private void GridFig1DMulti()
        {
            Grid1DRealData a0 = new(19, 0.5, 0.0);
            Grid1DRealData a1 = new(11, 1.0, 0.0);

            for (long i = 0; i < a0.Values.Count; i++)
                a0.Values[i] = Math.Cos(a0.GridInfo.GetCoordinate(i));
            for (long i = 0; i < a1.Values.Count; i++)
                a1.Values[i] = Math.Sin(a1.GridInfo.GetCoordinate(i));

            Figure.Show(new List<Grid1DRealData> { a0, a1 });
        }
        public ICommand GridFig1DMultiCommand
            => new RelayCommand(GridFig1DMulti);


        private void GridFig2D()
        {
            Grid2DRealData a = new(51, 71, 0.2, 0.2, 0.0);
            for (long iRow = 0; iRow < a.Values.Rows; iRow++)
            {
                var y = a.GridInfo.GetCoordinateY(iRow);
                for (long iCol = 0; iCol < a.Values.Cols; iCol++)
                {
                    var x = a.GridInfo.GetCoordinateX(iCol);
                    a.Values[iRow, iCol] = -0.0000134 * Math.Sin(x);
                }
            }
            Figure.Show(a);
        }
        public ICommand GridFig2DCommand
            => new RelayCommand(GridFig2D);


        private void GridFig2DMulti()
        {
            Grid2DRealData a0 = new(51, 71, 0.2, 0.2, 0.0);
            Grid2DRealData a1 = new(51, 71, 0.2, 0.2, 0.0);

            for (long iRow = 0; iRow < a0.Values.Rows; iRow++)
            {
                double y = a0.GridInfo.GetCoordinateY(iRow);
                for (long iCol = 0; iCol < a0.Values.Cols; iCol++)
                {
                    double x = a0.GridInfo.GetCoordinateX(iCol);
                    a0.Values[iRow, iCol] = Math.Sin(x);
                }
            }

            for (long iRow = 0; iRow < a1.Values.Rows; iRow++)
            {
                double y = a1.GridInfo.GetCoordinateY(iRow);
                for (long iCol = 0; iCol < a1.Values.Cols; iCol++)
                {
                    double x = a1.GridInfo.GetCoordinateX(iCol);
                    a1.Values[iRow, iCol] = Math.Sin(y);
                }
            }

            Figure.Show(new List<Grid2DRealData> { a0, a1 });
        }
        public ICommand GridFig2DMultiCommand
            => new RelayCommand(GridFig2D);


        private void Frame1D()
        {
            Frame1D f = new();
            //f.Owner = (Window)this.Parent;
            //f.SetTitle("My Frame");
            //f.SetLabelX("Horizontal Axis");
            //f.SetLabelY("Vertical Axis");
            //f.SetLabelYRight("");

            //f.SetAxisXDensity(density: 0.3, fontSize: 14.0);
            //f.SetAxisYDensity(density: 1.0, fontSize: 14.0);

            //f.SetAxisXLimits(-2.0, 3.0);
            //f.SetAxisYLimits(0.5, 3.5);

            VectorD d = new(7, 0.0)
            {
                [0] = 1,
                [1] = 1.3,
                [2] = 1.9,
                [3] = 2.0,
                [4] = 2.5,
                [5] = 2.7,
                [6] = 3.0
            };

            GridInfo1D g = new(7, 0.5);
            f.AddGridPlot(g, d,
                lineWidth: 5.0,
                lineStyle: ScottPlot.LineStyle.DashDot,
                markerSize: 12.0,
                plotColor: Options.PlotColor.Green);

            VectorD p = new(7, 0.0)
            {
                [0] = -1.5,
                [1] = -0.4,
                [2] = 0.0,
                [3] = 0.1,
                [4] = 0.8,
                [5] = 0.9,
                [6] = 1.8
            };
            f.AddScatPlot(p, d,
                lineWidth: 0.0,
                markerSize: 16.0,
                markerShape: ScottPlot.MarkerShape.openTriangleDown,
                plotColor: Options.PlotColor.SteelBlue);

            VectorZ z = new(7, 0.0)
            {
                [0] = new System.Numerics.Complex(0.9, 1.5),
                [1] = new System.Numerics.Complex(1.4, 2.8),
                [2] = new System.Numerics.Complex(2.7, 3.0),
                [3] = new System.Numerics.Complex(3.0, 1.9),
                [4] = new System.Numerics.Complex(3.1, 1.2),
                [5] = new System.Numerics.Complex(3.0, 1.5),
                [6] = new System.Numerics.Complex(2.9, 1.4)
            };
            f.AddGridPlot(grid: g, values: z, plotPart: ComplexPart.ImagPart,
                lineWidth: 3.0,
                lineStyle: ScottPlot.LineStyle.Solid,
                markerSize: 7.0,
                plotColor: Options.PlotColor.SlateGray);

            f.Visualize();
        }
        public ICommand Frame1DCommand
            => new RelayCommand(Frame1D);


        private void VFrame()
        {
            //VFrame vf = new VFrame();

            //VectorD d = new(7, 0.0)
            //{
            //    [0] = 1,
            //    [1] = 1.3,
            //    [2] = 1.9,
            //    [3] = 2.0,
            //    [4] = 2.5,
            //    [5] = 2.7,
            //    [6] = 3.0
            //};

            //GridInfo1D g = new(7, 0.5);

            //vf.AddGridPlot(g, d,
            //    lineWidth: 5.0,
            //    lineStyle: ScottPlot.LineStyle.Dot,
            //    markerSize: 12.0,
            //    plotColor: Options.PlotColor.Green);
            //vf.Refresh();

            //vf.Show();
        }
        public ICommand VFrameCommand
            => new RelayCommand(VFrame);


        #endregion

    }
}
