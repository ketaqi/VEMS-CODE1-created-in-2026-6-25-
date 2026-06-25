using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using VEMS.MathCore;

namespace VEMS.Plot
{
    /// <summary>
    /// GridFig2DControl.xaml 的交互逻辑
    /// </summary>
    [Obsolete]
    public partial class GridFig2DControl : UserControl
    {

        private GridFig2DModel model;
        private const string DefaultLabel = "Graph";
        private const string DefaultFigTitle = "VEMS Plot(s)";
        private const string DefaultFigLabelX = "variable";
        private const string DefaultFigLabelY = "function value";

        public GridFig2DControl()
        {
            InitializeComponent();

            model = new(PlotFrame,
                new double[3, 3] { { 1.0, 2.0, 3.0 }, { 2.5, 2.0, 1.5 }, { 1.5, 1.0, 0.5 } },
                new GridInfo2D(3, 3, 0.5, 0.5),
                "GrayScale",
                DefaultLabel,
                DefaultFigTitle,
                DefaultFigLabelX,
                DefaultFigLabelY);

            DataContext = model;
        }
    }
}
