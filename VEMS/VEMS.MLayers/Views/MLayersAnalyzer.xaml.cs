//
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Diagnostics;
using System.IO;

using VEMS.Plot;
using VEMS.EMSolver;
using VEMS.MathCore;
using Complex = System.Numerics.Complex;
using System.Data;
using Microsoft.Win32;
//using VEMS.MathCore.Array;

namespace VEMS.MLayers
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MLayersAnalyzer : Window
    {
        // internally set sampling number of angles
        private static int N = 71;
        public ObservableCollection<LayerInfo> MultiLayers { get; set; }
        public ObservableCollection<OpticalInfo> Optics { get; set; }

        private int PeroidLayerNumber = 0;
        private int NumberOfPeroid = 0;
        // constructor
        public MLayersAnalyzer()
        {
            InitializeComponent();
            Config.SetMathLibsDirectory();

            // optical information initialization
            Optics = new()
            {
                new OpticalInfo()
                {
                    Wavelength = 13.5E-9,
                    MinIncidenceAngle = 0,
                    MaxIncidenceAngle = 45
                }
            };
            // layer collection initialization
            MultiLayers = new()
            {
                new LayerInfo()
                {
                    Index = 0,
                    Thickness = 0E-9,
                    Material = "Air",
                    NRe = 1.0003,
                    NIm = 0.0
                },
                new LayerInfo()
                {
                    Index = 1,
                    Thickness = 30E-9,
                    Material = "Mo",
                    NRe = 1.75,
                    NIm = 0.0
                },
                new LayerInfo()
                {
                    Index = 2,
                    Thickness = 0E-9,
                    Material = "Silicon",
                    NRe = 1.5,
                    NIm = 0.0
                }
            };

            // set item source for data grid
            OpticalDataGrid.ItemsSource = Optics;
            layerDataGrid.ItemsSource = MultiLayers;

            #region initialize plots
            // reflectance - upper left
            FigUpperLeft.ClearAllPlots();
            FigUpperLeft.FigTitle = "Reflectance";
            FigUpperLeft.FigLabelX = "Incidence Angle [Deg]";
            FigUpperLeft.FigLabelY = "Power Ratio [%]";
            FigUpperLeft.UpdateFigureText();

            // transmittance - bottom left
            FigBottomLeft.ClearAllPlots();
            FigBottomLeft.FigTitle = "Transmittance";
            FigBottomLeft.FigLabelX = "Incidence Angle [Deg]";
            FigBottomLeft.FigLabelY = "Power Ratio [%]";
            FigBottomLeft.UpdateFigureText();

            // reflection coefficients - upper right
            FigUpperRight.ClearAllPlots();
            FigUpperRight.FigTitle = "Reflection Coefficients";
            FigUpperRight.FigLabelX = "Incidence Angle [Deg]";
            FigUpperRight.FigLabelY = "Magnitude";
            FigUpperRight.NeedRightYAxis = true;
            FigUpperRight.FigLabelYRight = "Phase";
            FigUpperRight.UpdateFigureText();

            // transmission coefficients - bottom right
            FigBottomRight.ClearAllPlots();
            FigBottomRight.FigTitle = "Transmission Coefficients";
            FigBottomRight.FigLabelX = "Incidence Angle [Deg]";
            FigBottomRight.FigLabelY = "Magnitude";
            FigBottomRight.NeedRightYAxis = true;
            FigBottomRight.FigLabelYRight = "Phase";
            FigBottomRight.UpdateFigureText();
            #endregion

        }


        #region methods

        #region adding or delete layers
        //Add an empty layer at chosed position or in the bottom
        private void AddLayer(object sneder, RoutedEventArgs e)
        {
            //To judge if this new layer is added to the chosed position
            if (layerDataGrid.SelectedItems.Count == 1)
            {
                int indexBeforeAdd = layerDataGrid.SelectedIndex;
                AddSingleLayer_(indexBeforeAdd);
            }
            //Delete the chosen layers and then add one new layer
            else if (layerDataGrid.SelectedItems.Count > 1)
            {
                int FirstIndex = layerDataGrid.Items.IndexOf(layerDataGrid.SelectedItems[0]);
                int LastIndex = layerDataGrid.Items.IndexOf(layerDataGrid.SelectedItems[^1]);
                int indexSelected = FirstIndex < LastIndex ? FirstIndex: LastIndex;
                DeleteSelectedLayers_();
                AddSingleLayer_(--indexSelected);                
            }
            else
            {
            MultiLayers.Add(new LayerInfo()
            {
                    Index = MultiLayers.Count,
                Thickness = 0.0,
                Material = "Null",
                NRe = 1.0,
                NIm = 0.0
            });

        }
        }

        private void DeleteLayers(object sneder, RoutedEventArgs e)
        {
            DeleteSelectedLayers_(); 
        }

        private void AddPeriodicLayers(object sneder, RoutedEventArgs e)
        {            
            Peroidic_Inputbox peroidicInputbox = new();
            peroidicInputbox.ShowDialog();
            PeroidLayerNumber = peroidicInputbox.LayerNumber;
            NumberOfPeroid = peroidicInputbox.PeroidNumber;
            //create new MultiLayers of Peroidic Layers
            MultiLayers = new()
            {//First layer, air
                new LayerInfo()
                {
                    Index = 0,
                    Thickness = 0E-9,
                    Material = "Air",
                    NRe = 1.0003,
                    NIm = 0.0
                },
            };
            //Without this line, old layerDataGrid won't disappear
            layerDataGrid.ItemsSource = MultiLayers;
            //Copy the data from Peroidic_Inputbox
            for (int i = 1; i <= NumberOfPeroid; i++)
            {
                for (int j = 1; j <= PeroidLayerNumber; j++)
                {
                    MultiLayers.Add(new LayerInfo()
                    {
                        Index=peroidicInputbox.InputPeroidLayers[j - 1].Index,
                        Thickness = peroidicInputbox.InputPeroidLayers[j - 1].Thickness,
                        Material = peroidicInputbox.InputPeroidLayers[j - 1].Material,
                        NRe = peroidicInputbox.InputPeroidLayers[j - 1].NRe,
                        NIm = peroidicInputbox.InputPeroidLayers[j - 1].NIm
                    });                   
                }
            }
            MultiLayers.Add(new LayerInfo()
            {//At last, add a base layer of Silicon
                Index = 2,
                Thickness = 0E-9,
                Material = "Silicon",
                NRe = 1.5,
                NIm = 0.0
            });
            SortingLayers_();
        }

        



        private void AddSingleLayer_(int indexBeforeAdd)
        {
            MultiLayers.Add(new LayerInfo() //New a empty layer in the bottom
            {
                Index = MultiLayers.Count - 1,
            });
            //From the chosed layer+1, move the information of every layer to the next layer
            for (int i = MultiLayers.Count - 1; i > indexBeforeAdd + 1; i--)
            {
                MultiLayers[i] = MultiLayers[i - 1];
                MultiLayers[i].Index++;
            }//Caution!   MultiLayer[i] and MultiLayer[i-1] point to the same layer information, so we need to new a LayerInfo
            MultiLayers[indexBeforeAdd + 1] = new LayerInfo()
            {
                Index = indexBeforeAdd + 1,
                Thickness = 0,
                Material = "null",
                NRe = 1,
                NIm = 0,
            };
        }
        private void DeleteSelectedLayers_()
        {
            int row = layerDataGrid.SelectedItems.Count;
            if (row == 0)
            {
                MessageBox.Show("没有选中任何行", "Error");
            }
            else
            {
                //difference of choosing from bottom to top or from top to bottom
                int FirstIndex = layerDataGrid.Items.IndexOf(layerDataGrid.SelectedItems[0]);
                int LastIndex = layerDataGrid.Items.IndexOf(layerDataGrid.SelectedItems[^1]);
                if (FirstIndex <= LastIndex)
                {
                    for (int i = FirstIndex; i <= LastIndex; i++)
                    { MultiLayers.RemoveAt(FirstIndex); }
                }
                else
                {
                    for (int i = FirstIndex; i >= LastIndex; i--)
                    { MultiLayers.RemoveAt(LastIndex); }
                }
            }
            SortingLayers_();
        }
        
        //Set the index of every layer
        private void SortingLayers_()
        {
            for (int i = 0; i <= MultiLayers.Count-1; i++)
            {
                MultiLayers[i].Index = i;
            }
        }
        #endregion

        #region Compute Ratios and Coefficients
        // menu items function
        private void MenuItem_ComputePowerRatios(object sender, RoutedEventArgs e)
        {
            #region gether all the input info
            double wavelength = Optics[0].Wavelength;
            double minAOI = Converter.Degree2Radian(Optics[0].MinIncidenceAngle);
            double maxAOI = Converter.Degree2Radian(Optics[0].MaxIncidenceAngle);
            Complex n1 = new(MultiLayers[0].NRe, MultiLayers[0].NIm);
            Complex n2 = new(MultiLayers[^1].NRe, MultiLayers[^1].NIm);
            List<Complex> nLayers = new();
            List<double> tLayers = new();
            if (MultiLayers.Count > 2)
            {
                for (int i = 1; i < MultiLayers.Count - 1; i++)
                {
                    nLayers.Add(new Complex(MultiLayers[i].NRe, MultiLayers[i].NIm));
                    tLayers.Add(MultiLayers[i].Thickness);
                }
            }
            #endregion
            #region computation
            // initialize output containers
            VectorD transmissionTE = new(N, 0.0);
            VectorD transmissionTM = new(N, 0.0);
            VectorD reflectionTE = new(N, 0.0);
            VectorD reflectionTM = new(N, 0.0);
            // computation
            double dAngle = (maxAOI - minAOI) / (N - 1);
            for (int i = 0; i < N; i++)
            {
                double angle = minAOI + i * dAngle;
                double kx = 2.0 * Math.PI / wavelength * Math.Sin(angle);

                double tTE, rTE;
                CoatingCalculator.ComputeCoatingRatio(wavelength,
                    n1, nLayers, tLayers, n2, kx,
                    out tTE, out rTE, PolarizationMode.TE);
                transmissionTE[i] = tTE;
                reflectionTE[i] = rTE;

                double tTM, rTM;
                CoatingCalculator.ComputeCoatingRatio(wavelength,
                    n1, nLayers, tLayers, n2, kx,
                    out tTM, out rTM, PolarizationMode.TM);
                transmissionTM[i] = tTM;
                reflectionTM[i] = rTM;
            }
            #endregion
            #region plots
            // define sampling grid
            GridInfo1D grid = new(N, Converter.Radian2Degree(minAOI), Converter.Radian2Degree(dAngle));

            // clear all plots for initialization
            FigUpperLeft.ClearAllPlots();

            // add new plots
            FigUpperLeft.AddGridPlot(VMath.ConvertVectorToArray(100.0*reflectionTE), grid,
                System.Drawing.Color.DarkOrange, 4.0, 0.0, DrawOption.YLeft, "R-TE");
            FigUpperLeft.AddGridPlot(VMath.ConvertVectorToArray(100.0*reflectionTM), grid,
                System.Drawing.Color.LightSteelBlue, 4.0, 0.0, DrawOption.YLeft, "R-TM");
            FigBottomLeft.AddGridPlot(VMath.ConvertVectorToArray(100.0*transmissionTE), grid,
                System.Drawing.Color.RosyBrown, 4.0, 0.0, DrawOption.YLeft, "T-TE");
            FigBottomLeft.AddGridPlot(VMath.ConvertVectorToArray(100.0 * transmissionTM), grid,
                System.Drawing.Color.DarkBlue, 4.0, 0.0, DrawOption.YLeft, "T-TM");

            // auto view range
            FigUpperLeft.AutoViewRange();
            FigUpperLeft.Refresh();


            // clear all plots for initialization
            FigBottomLeft.ClearAllPlots();

            // add new plots
            FigBottomLeft.AddGridPlot(VMath.ConvertVectorToArray(100.0 * transmissionTE), grid,
                System.Drawing.Color.DarkOrange, 4.0, 0.0, DrawOption.YLeft, "T-TE");
            FigBottomLeft.AddGridPlot(VMath.ConvertVectorToArray(100.0 * transmissionTM), grid,
                System.Drawing.Color.LightSteelBlue, 4.0, 0.0, DrawOption.YLeft, "T-TM");

            // auto view range
            FigBottomLeft.AutoViewRange();
            FigBottomLeft.Refresh();

            #endregion
        }

        private void MenuItem_ComputeCoefficients(object sender, RoutedEventArgs e)
        {
            #region gether all the input info
            double wavelength = Optics[0].Wavelength;
            double minAOI = Converter.Degree2Radian(Optics[0].MinIncidenceAngle);
            double maxAOI = Converter.Degree2Radian(Optics[0].MaxIncidenceAngle);
            Complex n1 = new(MultiLayers[0].NRe, MultiLayers[0].NIm);
            Complex n2 = new(MultiLayers[^1].NRe, MultiLayers[^1].NIm);
            List<Complex> nLayers = new();
            List<double> tLayers = new();
            if (MultiLayers.Count > 2)
            {
                for (int i = 1; i < MultiLayers.Count - 1; i++)
                {
                    nLayers.Add(new Complex(MultiLayers[i].NRe, MultiLayers[i].NIm));
                    tLayers.Add(MultiLayers[i].Thickness);
                }
            }
            #endregion
            #region computation
            // initialize output containers
            VectorZ tcTE = new(N, 0.0);
            VectorZ tcTM = new(N, 0.0);
            VectorZ rcTE = new(N, 0.0);
            VectorZ rcTM = new(N, 0.0);
            // computation
            double dAngle = (maxAOI - minAOI) / (N - 1);
            for (int i = 0; i < N; i++)
            {
                double angle = minAOI + i * dAngle;
                double kx = 2.0 * Math.PI / wavelength * Math.Sin(angle);

                Complex tTE, rTE;
                CoatingCalculator.ComputeCoatingMatrix(wavelength,
                    n1, nLayers, tLayers, n2, kx,
                    out tTE, out rTE, PolarizationMode.TE);
                tcTE[i] = tTE;
                rcTE[i] = rTE;

                Complex tTM, rTM;
                CoatingCalculator.ComputeCoatingMatrix(wavelength,
                    n1, nLayers, tLayers, n2, kx,
                    out tTM, out rTM, PolarizationMode.TM);
                tcTM[i] = tTM;
                rcTM[i] = rTM;
            }
            #endregion
            #region plots
            // define sampling grid
            GridInfo1D grid = new(N, Converter.Radian2Degree(minAOI), Converter.Radian2Degree(dAngle));

            // clear all plots for initialization
            FigUpperRight.ClearAllPlots();

            // add new plots
            VectorD rcTEAbs = VMath.Abs(rcTE);
            VectorD rcTEArg = VMath.Arg(rcTE);
            VectorD rcTMAbs = VMath.Abs(rcTM);
            VectorD rcTMArg = VMath.Arg(rcTM);
            FigUpperRight.AddGridPlot(VMath.ConvertVectorToArray(rcTEAbs), grid,
                System.Drawing.Color.DarkOrange, 4.0, 0.0, DrawOption.YLeft, "Abs[rTE]");
            FigUpperRight.AddGridPlot(VMath.ConvertVectorToArray(rcTEArg), grid,
                System.Drawing.Color.Orange, 1.0, 3.0, DrawOption.YRight, "Arg[rTE]");
            FigUpperRight.AddGridPlot(VMath.ConvertVectorToArray(rcTMAbs), grid,
                System.Drawing.Color.LightSteelBlue, 4.0, 0.0, DrawOption.YLeft, "Abs[rTM]");
            FigUpperRight.AddGridPlot(VMath.ConvertVectorToArray(rcTMArg), grid,
                System.Drawing.Color.LightBlue, 1.0, 3.0, DrawOption.YLeft, "Arg[rTM]");

            // auto view range
            FigUpperRight.AutoViewRange();
            FigUpperRight.Refresh();


            FigBottomRight.ClearAllPlots();

            VectorD tcTEAbs = VMath.Abs(tcTE);
            VectorD tcTEArg = VMath.Arg(tcTE);
            VectorD tcTMAbs = VMath.Abs(tcTM);
            VectorD tcTMArg = VMath.Arg(tcTM);

            FigBottomRight.AddGridPlot(VMath.ConvertVectorToArray(tcTEAbs), grid,
                System.Drawing.Color.DarkOrange, 4.0, 0.0, DrawOption.YLeft, "Abs[tTE]");
            FigBottomRight.AddGridPlot(VMath.ConvertVectorToArray(tcTEArg), grid,
                System.Drawing.Color.Orange, 1.0, 3.0, DrawOption.YRight, "Arg[tTE]");
            FigBottomRight.AddGridPlot(VMath.ConvertVectorToArray(tcTMAbs), grid,
                System.Drawing.Color.LightSteelBlue, 4.0, 0.0, DrawOption.YLeft, "Abs[tTM]");
            FigBottomRight.AddGridPlot(VMath.ConvertVectorToArray(tcTMArg), grid,
                System.Drawing.Color.LightBlue, 1.0, 3.0, DrawOption.YLeft, "Arg[tTM]");

            // auto view range
            FigUpperRight.AutoViewRange();
            FigUpperRight.Refresh();
            FigBottomRight.AutoViewRange();
            FigBottomRight.Refresh();

            #endregion

        }
        #endregion region

        private void MenuItem_LoadDefaultInfo(object sender, RoutedEventArgs e)
        {
            // optical information initialization
            Optics = new()
            {
                new OpticalInfo()
                {
                    Wavelength = 632.8E-9,
                    MinIncidenceAngle = 0,
                    MaxIncidenceAngle = 45
                }
            };
            // layer collection initialization
            MultiLayers = new()
            {
                new LayerInfo()
                {
                    Index = 0,
                    Thickness = 0E-9,
                    Material = "Air",
                    NRe = 1.0,
                    NIm = 0.0
                },
                new LayerInfo()
                {
                    Index = 1,
                    Thickness = 13.43E-9,
                    Material = "HFO2",
                    NRe = 1.84971,
                    NIm = 0.00179
                },
                new LayerInfo()
                {
                    Index = 2,
                    Thickness = 58.96E-9,
                    Material = "MgF2",
                    NRe = 1.35952,
                    NIm = 0.0
                },
                new LayerInfo()
                {
                    Index = 3,
                    Thickness = 32.89E-9,
                    Material = "HFO2",
                    NRe = 1.84971,
                    NIm = 0.00179
                },
                new LayerInfo()
                {
                    Index = 4,
                    Thickness = 56.25E-9,
                    Material = "MgF2",
                    NRe = 1.35952,
                    NIm = 0.0
                },
                new LayerInfo()
                {
                    Index = 5,
                    Thickness = 31.54E-9,
                    Material = "HFO2",
                    NRe = 1.84971,
                    NIm = 0.00179
                },
                new LayerInfo()
                {
                    Index = 6,
                    Thickness = 54.63E-9,
                    Material = "MgF2",
                    NRe = 1.35952,
                    NIm = 0.0
                },
                new LayerInfo()
                {
                    Index = 7,
                    Thickness = 32.73E-9,
                    Material = "HFO2",
                    NRe = 1.84971,
                    NIm = 0.00179
                },
                new LayerInfo()
                {
                    Index = 8,
                    Thickness = 59.05E-9,
                    Material = "MgF2",
                    NRe = 1.35952,
                    NIm = 0.0
                },
                new LayerInfo()
                {
                    Index = 9,
                    Thickness = 39.50E-9,
                    Material = "HFO2",
                    NRe = 1.84971,
                    NIm = 0.00179
                },
                new LayerInfo()
                {
                    Index = 10,
                    Thickness = 80.41E-9,
                    Material = "MgF2",
                    NRe = 1.35952,
                    NIm = 0.0
                },
                new LayerInfo()
                {
                    Index = 11,
                    Thickness = 53.15E-9,
                    Material = "HFO2",
                    NRe = 1.84971,
                    NIm = 0.00179
                },
                new LayerInfo()
                {
                    Index = 12,
                    Thickness = 10E-9,
                    Material = "Al2O3F2",
                    NRe = 1.65866,
                    NIm = 0.0
                },
                new LayerInfo()
                {
                    Index = 13,
                    Thickness = 200E-9,
                    Material = "Ag",
                    NRe = 0.02309,
                    NIm = 4.20864
                },
            };

            // set item source for data grid
            OpticalDataGrid.ItemsSource = null;
            OpticalDataGrid.ItemsSource = Optics;
            layerDataGrid.ItemsSource = null; 
            layerDataGrid.ItemsSource = MultiLayers;
        }

        // tests ...

        private void MenuItem_ShowInfoClick(object sender, RoutedEventArgs e)
        {
            string t = "";
            for(int i = 0; i<MultiLayers.Count; i++)
            {
                LayerInfo layer = MultiLayers[i];
                t += layer.Material + "\t " + layer.Thickness.ToString() + "\n";
            }
            MessageBox.Show(t);
        }

        private void LinearInterpolation(object sneder, RoutedEventArgs e)
        {//测试线性插值用
            int length = 11;
            VectorD myPoint = new VectorD(length);
            for (int i = 0; i < length; i++)
            {
                myPoint[i] = (double)i;
                //MessageBox.Show(myPoint[i].ToString());
            }
            VectorD myValue = new VectorD(length);
            for (int i = 0; i < length; i++)
            {
                myValue[i] = i*i;                
            }
            Scat1DRealData a = new Scat1DRealData(myPoint, myValue);
            var tGrid = new GridInfo1D(1100, 0, 0.1);
            var b = Interpolation.ScatLinear(a, tGrid);
            VEMS.Plot.Figure.Show(b, "Scatter Interpolated Result", "x", "function value");
        }



        #region 导入txt中的折射率信息
        //打开选择的TXT文件
        private string OpenNFile()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "选择数据源文件";
            openFileDialog.Filter = "txt文件|*.txt";
            openFileDialog.FileName = string.Empty;
            openFileDialog.FilterIndex = 1;
            openFileDialog.Multiselect = false;
            openFileDialog.RestoreDirectory = true;
            openFileDialog.DefaultExt = "txt";
            openFileDialog.ShowDialog();
            string txtFile = openFileDialog.FileName;
            return txtFile;
        }

        //整理TXT文件的数据并存入数组
        private void InputTXTData(out Grid1DRealData GridReal, out Grid1DRealData GridImag)
        {
            string TXTName = OpenNFile();          
            long RowNumber = 0;

            string ReadLine;
            string[] array;
            string Path = @TXTName;
            int j = 0;
            StreamReader reader = new StreamReader(Path);
            while (reader.Peek() >= 0)  //先循环一遍获取行数
            {

                ReadLine = reader.ReadLine();
                RowNumber++;
            }
            reader = new StreamReader(Path);

            var NReal = new VectorD(RowNumber);
            var NImag = new VectorD(RowNumber);
            var NWav = new VectorD(RowNumber);
            while (reader.Peek() >= 0)  //一直读取到没有字符为止
            {
                ReadLine = reader.ReadLine();
                if (ReadLine != "")
                {
                    // 字符串将被隔开并分别被读取
                    array = ReadLine.Split("\t");
                    NWav[j] = Convert.ToDouble(array[0]);//波长   
                    NReal[j] = Convert.ToDouble(array[1]);//折射率实部   
                    NImag[j] = Convert.ToDouble(array[2]);//折射率虚部
                }
                j++;
            }
            //插值成等间距数据
            var tGrid = new GridInfo1D(151, 12.0, 0.02);
            var ScatReal = new Scat1DRealData(NWav, NReal);
            //for (int i = 0; i<=MoWavReal.Points.Count; i++) { MessageBox.Show(MoWavReal.Points[i].ToString() + "\n" + MoWavReal.Values[i].ToString()); }
            var ScatImag = new Scat1DRealData(NWav, NImag);
            GridReal = Interpolation.ScatLinear(ScatReal, tGrid);
            GridImag = Interpolation.ScatLinear(ScatImag, tGrid);
        }
        private void ImportNTXT(object sneder, RoutedEventArgs e)
        {          
            //Mo的均匀分布折射率
            InputTXTData(out Grid1DRealData GridReal,out Grid1DRealData GridImag);
            //画图
            VEMS.Plot.Figure.Show(GridReal,"线性插值实部", "wavelength", "Real");
            VEMS.Plot.Figure.Show(GridImag,"线性插值虚部", "wavelength", "Imag");
        }
        #endregion

        #region 比较8°时尧舜的方法和S-Matrix算法计算出的反射率对比
        private void Com8degree(object sender, RoutedEventArgs e)
        {
            int N = 151;
            double wavelength = 12.0e-9;
            double Angle = Converter.Degree2Radian(8);

            Complex n1 = new(1.0003, 0);//空气折射率
            Complex n2;//基底折射率初始化
            List<Complex> nLayers;
            List<double> tLayers = new();
           
            VectorD transmissionTE = new(N, 0.0);
            VectorD transmissionTM = new(N, 0.0);
            VectorD reflectionTE = new(N, 0.0);
            VectorD reflectionTM = new(N, 0.0);
            VectorD reflection2 = new(N, 0.0);

            double kx;

            //首先分别导入Mo和Si随波长变化的折射率
            MessageBox.Show("请选择Mo文件");
            InputTXTData(out Grid1DRealData MoGridReal, out Grid1DRealData MoGridImag);
            MessageBox.Show("请选择Si文件");
            InputTXTData(out Grid1DRealData SiGridReal, out Grid1DRealData SiGridImag);
           
            for (int i = 0; i < MoGridReal.Grid.Count; i++)
            {
                tLayers = new();
                nLayers = new();
                n2 = new Complex(SiGridReal.Values[i], SiGridImag.Values[i]);
                kx = 2 * Math.PI / wavelength * Math.Sin(Angle);
                for (int j = 0; j < 40; j++)
                {
                    tLayers.Add(0.000000004182);//Si
                    tLayers.Add(0.000000002788);//Mo
                    nLayers.Add(new Complex(SiGridReal.Values[i], SiGridImag.Values[i]));
                    nLayers.Add(new Complex(MoGridReal.Values[i], MoGridImag.Values[i]));
                }
                
                CoatingCalculator.ComputeCoatingRatio(wavelength,
                    n1, nLayers, tLayers, n2, kx,
                    out double tTE, out double rTE, PolarizationMode.TE);
                transmissionTE[i] = tTE;
                reflectionTE[i] = rTE;
                CoatingCalculator.ComputeCoatingRatio(wavelength,
                    n1, nLayers, tLayers, n2, kx,
                    out double tTM, out double rTM, PolarizationMode.TM);
                transmissionTM[i] = tTM;
                reflectionTM[i] = rTM;
               
                //MessageBox.Show("i= " + i.ToString() + "\n"
                //    + "wav= " + wavelength.ToString() + "\n"
                //    + "n1= " + n1.ToString() + "\n"
                //    + "n2= " + n2.ToString() + "\n"
                //    + "kx= " + kx.ToString() + "\n"
                //    + "reflectionTM= " + reflectionTM[i].ToString() + "\n"
                //    + "transmissionTM= " + transmissionTM[i].ToString());

                wavelength += 0.02E-9;
            }           
            //分割线++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
            //导入TXT文档中用其他方法计算的结果
            string ReadLine;
            string[] array;
            string Path = @"D:\\Si_Mo.txt";
            int m = 0;
            StreamReader reader = new StreamReader(Path);
            while (reader.Peek() >= 0)  //一直读取到没有字符为止
            {
                ReadLine = reader.ReadLine();
                if (ReadLine != "")
                {
                    array = ReadLine.Split("     ");    // 五个以上空格的字符串将被隔开并分别被读取
                    reflection2[m] = double.Parse(array[2]);

                }
                m++;
            }
            //开始画图
            // define sampling grid
            GridInfo1D grid = new(N, 12.0, 0.02);
            // clear all plots for initialization
            FigUpperLeft.ClearAllPlots();
            // add new plots
            FigUpperLeft.AddGridPlot(VMath.ConvertVectorToArray(100.0 * reflectionTE), grid,
                System.Drawing.Color.DarkOrange, 4.0, 0.0, DrawOption.YLeft, "R-TE");
            FigUpperLeft.AddGridPlot(VMath.ConvertVectorToArray(100.0 * reflectionTM), grid,
                System.Drawing.Color.LightSteelBlue, 4.0, 0.0, DrawOption.YLeft, "R-TM");            
           // auto view range
            FigUpperLeft.AutoViewRange();
            FigUpperLeft.Refresh();
            FigUpperLeft.FigTitle = "TM & TE From VEMS";


            // clear all plots for initialization
            FigUpperRight.ClearAllPlots();

            // add new plots
            //FigBottomLeft.AddGridPlot(VMath.ConvertVectorToArray(100.0 * transmissionTE), grid,
            //    System.Drawing.Color.DarkOrange, 4.0, 0.0, DrawOption.YLeft, "T-TE");
            FigUpperRight.AddGridPlot(VMath.ConvertVectorToArray(100.0*(reflectionTE+reflectionTM)/2), grid,
                System.Drawing.Color.DarkOrange, 4.0, 0.0, DrawOption.YLeft, "Average");

            // auto view range
            FigUpperRight.AutoViewRange();
            FigUpperRight.Refresh();
            FigUpperRight.FigTitle = "Average of TE and TM";

            // clear all plots for initialization
            FigBottomRight.ClearAllPlots();

            // add new plots
            //FigBottomLeft.AddGridPlot(VMath.ConvertVectorToArray(100.0 * transmissionTE), grid,
            //    System.Drawing.Color.DarkOrange, 4.0, 0.0, DrawOption.YLeft, "T-TE");
            FigBottomRight.AddGridPlot(VMath.ConvertVectorToArray(100.0 * (reflectionTE + reflectionTM) / 2), grid,
                System.Drawing.Color.DarkOrange, 4.0, 0.0, DrawOption.YLeft, "VEMS");
            FigBottomRight.AddGridPlot(VMath.ConvertVectorToArray(100.0 * reflection2), grid,
                System.Drawing.Color.LightSteelBlue, 4.0, 0.0, DrawOption.YLeft, "Yao");          
            // auto view range
            FigBottomRight.AutoViewRange();
            FigBottomRight.Refresh();
            FigBottomRight.FigTitle = "Comparing of VEMS and Yao's data";


            // clear all plots for initialization
            FigBottomLeft.ClearAllPlots();

            // add new plots
            //FigBottomLeft.AddGridPlot(VMath.ConvertVectorToArray(100.0 * transmissionTE), grid,
            //    System.Drawing.Color.DarkOrange, 4.0, 0.0, DrawOption.YLeft, "T-TE");
            FigBottomLeft.AddGridPlot(VMath.ConvertVectorToArray(100.0 * reflection2), grid,
                System.Drawing.Color.DarkOrange, 4.0, 0.0, DrawOption.YLeft, "Average");

            // auto view range
            FigBottomLeft.AutoViewRange();
            FigBottomLeft.Refresh();
            FigBottomLeft.FigTitle = "Yao's Data";
        }
        #endregion








        #endregion


    }
}
