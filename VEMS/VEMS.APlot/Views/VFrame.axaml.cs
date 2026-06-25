using Avalonia.Controls;
using ScottPlot.Avalonia;

namespace VEMS.APlot.Views
{
    public partial class VFrame : Window
    {
        public VFrame()
        {
            InitializeComponent();

            double[] dataX = { 1, 2, 3, 4, 5 };
            double[] dataY = { 1, 4, 9, 16, 25 };

            AvaPlot avaPlot1 = this.Find<AvaPlot>("AvaPlot1");
            avaPlot1.Plot.Add.Scatter(dataX, dataY);
            avaPlot1.Refresh(); 
        }
    }
}