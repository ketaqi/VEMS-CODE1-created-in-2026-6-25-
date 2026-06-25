using System.Windows.Controls;
using VEMS.MathCore;
using VEMS.Plot5;

namespace VEMS.Plot5Tests
{


    public class MainViewModel
    {
        public string TitleLeft { get; set; }


        public Plot5.Frame Frame { get; set; }
        public VectorD DataLeft { get; set; }
        public Grid1DRealPlotPrty GPPLeft { get; set; }



        public MainViewModel(Plot5.Frame f)
        {
            Frame = f;
            Frame.ViewModel.Dock.Plot.Title("Customized Title");
            DataLeft = new(11, 0.0, 0.1);
            GPPLeft = new()
            {
                LineWidth = 5.0,
                LinePattern = LinePattern.Dashed,
                PlotColor = PlotColor.RosyBrown
            };
            Frame.ViewModel.AddGrid1DRealPlot(values: DataLeft, plotProperty: GPPLeft);
            
        }


    }
}
