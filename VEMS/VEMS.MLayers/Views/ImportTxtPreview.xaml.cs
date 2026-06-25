using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using VEMS.MathCore;
using VEMS.Plot;

namespace VEMS.MLayers
{
    /// <summary>
    /// ImportTxtPreview.xaml 的交互逻辑
    /// </summary>
    public partial class ImportTxtPreview : Window
    {
        public ImportTxtPreview(Grid1DRealData gridReal,Grid1DRealData gridImag)
        {
            InitializeComponent();
            GridInfo1D grid = gridReal.Grid;
            // clear all plots for initialization
            Fig1.ClearAllPlots();
            // add new plots
            Fig1.AddGridPlot(VMath.ConvertVectorToArray(gridReal.Values), grid,
                System.Drawing.Color.DarkOrange, 4.0, 0.0, DrawOption.YLeft, "Real");

            // auto view range
            Fig1.AutoViewRange();
            Fig1.Refresh();
            Fig1.FigTitle = "Real Part of n";

            Fig2.ClearAllPlots();
            // add new plots
            Fig2.AddGridPlot(VMath.ConvertVectorToArray(gridImag.Values), grid,
                System.Drawing.Color.DodgerBlue, 4.0, 0.0, DrawOption.YLeft, "Imag");

            // auto view range
            Fig2.AutoViewRange();
            Fig2.Refresh();
            Fig2.FigTitle = "Imag Part of n";
        }

        
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            Close();
            MessageBox.Show("Data has been imported");
        }
    }
}
