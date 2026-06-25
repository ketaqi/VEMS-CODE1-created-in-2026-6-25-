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
using System.Windows.Navigation;
using System.Windows.Shapes;

using VEMS.MathCore;

namespace VEMS.Plot
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class PlotMenu : Window
    {
        public PlotMenu()
        {
            InitializeComponent();

            var viewModel = new PlotMenuViewModel();
            DataContext = viewModel;
            //Config.SetMathLibsDirectory();
        }

        /*
        [Obsolete]
        private void BtnVPlot1D(object sender, RoutedEventArgs e)
        {
            GridInfo1D grid = new(17, 0.5);
            VectorD v = new(grid.Count); // { [0] = 0, [1] = 1, [2] = 4, [3] = 2 };
            for(long i = 0; i<v.Count;i++)
                v[i] = Math.Cos(grid.GetCoordinate(i));

            //Figure1D.Show(v, grid, System.Drawing.Color.SteelBlue);
        }

        [Obsolete]
        private void BtnVPlot1DMulti(object sender, RoutedEventArgs e)
        {
            GridInfo1D grid0 = new(19, 0.5);
            GridInfo1D grid1 = new(11, 1.0);
            VectorD v0 = new(grid0.Count); // { [0] = 0, [1] = 1, [2] = 4, [3] = 2 };
            VectorD v1 = new(grid1.Count); // { [0] = 5, [1] = 4, [2] = 3, [3] = 2 };
            
            for(long i = 0; i <v0.Count;i++)
                v0[i] = Math.Cos(grid0.GetCoordinate(i));
            for(long i =0; i <v1.Count;i++)
                v1[i] =Math.Sin(grid1.GetCoordinate(i));

            //Figure1D.Show(new List<VectorD> { v0, v1 },
            //    new List<GridInfo1D> { grid0, grid1 },
            //    null);
        }

        [Obsolete]
        private void BtnVPlot2D(object sender, RoutedEventArgs e)
        {
            GridInfo2D grid = new(51, 71, 0.2, 0.2);
            MatrixD v = new(grid.Rows, grid.Cols);
            for(long iRow = 0; iRow < v.Rows; iRow++)
            {
                double y = grid.GetCoordinateY(iRow);
                for(long iCol = 0; iCol < v.Cols; iCol++)
                {
                    double x = grid.GetCoordinateX(iCol);
                    v[iRow, iCol] = -0.0000134*Math.Sin(x);
                }
            }

            //Figure2D.Show(v, grid, "Grayscale");
        }

        [Obsolete]
        private void BtnVPlot2DMulti(object sender, RoutedEventArgs e)
        {
            GridInfo2D grid0 = new(51, 71, 0.2, 0.2);
            MatrixD v0 = new(grid0.Rows, grid0.Cols);
            for (long iRow = 0; iRow < v0.Rows; iRow++)
            {
                double y = grid0.GetCoordinateY(iRow);
                for (long iCol = 0; iCol < v0.Cols; iCol++)
                {
                    double x = grid0.GetCoordinateX(iCol);
                    v0[iRow, iCol] = Math.Sin(x);
                }
            }

            GridInfo2D grid1 = new(51, 71, 0.2, 0.2);
            MatrixD v1 = new(grid1.Rows, grid1.Cols);
            for (long iRow = 0; iRow < v1.Rows; iRow++)
            {
                double y = grid1.GetCoordinateY(iRow);
                for (long iCol = 0; iCol < v1.Cols; iCol++)
                {
                    double x = grid1.GetCoordinateX(iCol);
                    v1[iRow, iCol] = Math.Sin(y);
                }
            }

            //Figure2D.Show(new List<MatrixD> { v0, v1 },
            //    new List<GridInfo2D> { grid0, grid1 },
            //    null, null);

        }


        private void BtnGridFig1D(object sender, RoutedEventArgs e)
        {
            Grid1DRealData a = new(17, 0.5, 0.0);
            for (long i = 0; i < a.Values.Count; i++)
            {
                var x = a.Grid.GetCoordinate(i);
                a.Values[i] = Math.Cos(x);
            }

            //VectorD a = new(17, 0.0);
            //for (long i = 0; i < a.Count; i++)
            //    a[i] = Math.Cos(i);

            Figure.Show(a);
        }

        private void BtnGridFig1DMulti(object sender, RoutedEventArgs e)
        {
            Grid1DRealData a0 = new(19, 0.5, 0.0);
            Grid1DRealData a1 = new(11, 1.0, 0.0);

            for (long i = 0; i < a0.Values.Count; i++)
                a0.Values[i] = Math.Cos(a0.Grid.GetCoordinate(i));
            for (long i = 0; i < a1.Values.Count; i++)
                a1.Values[i] = Math.Sin(a1.Grid.GetCoordinate(i));

            Figure.Show(new List<Grid1DRealData> { a0, a1 });
        }

        private void BtnGridFig2D(object sender, RoutedEventArgs e)
        {
            Grid2DRealData a = new(51, 71, 0.2, 0.2, 0.0);
            for (long iRow = 0; iRow < a.Values.Rows; iRow++)
            {
                var y = a.Grid.GetCoordinateY(iRow);
                for (long iCol = 0; iCol < a.Values.Cols; iCol++)
                {
                    var x = a.Grid.GetCoordinateX(iCol);
                    a.Values[iRow, iCol] = -0.0000134 * Math.Sin(x);
                }
            }
            Figure.Show(a);
        }

        private void BtnGridFig2DMulti(object sender, RoutedEventArgs e)
        {
            Grid2DRealData a0 = new(51, 71, 0.2, 0.2, 0.0);
            Grid2DRealData a1 = new(51, 71, 0.2, 0.2, 0.0);

            for (long iRow = 0; iRow < a0.Values.Rows; iRow++)
            {
                double y = a0.Grid.GetCoordinateY(iRow);
                for (long iCol = 0; iCol < a0.Values.Cols; iCol++)
                {
                    double x = a0.Grid.GetCoordinateX(iCol);
                    a0.Values[iRow, iCol] = Math.Sin(x);
                }
            }

            for (long iRow = 0; iRow < a1.Values.Rows; iRow++)
            {
                double y = a1.Grid.GetCoordinateY(iRow);
                for (long iCol = 0; iCol < a1.Values.Cols; iCol++)
                {
                    double x = a1.Grid.GetCoordinateX(iCol);
                    a1.Values[iRow, iCol] = Math.Sin(y);
                }
            }

            Figure.Show(new List<Grid2DRealData> { a0, a1 });
        }

        private void BtnFrame1D(object sender, RoutedEventArgs e)
        {
            Frame1D f = new ();
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

        private void BtnVFrame(object sender, RoutedEventArgs e)
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

        */
    
    }
}
