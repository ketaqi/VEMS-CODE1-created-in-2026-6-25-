using CommunityToolkit.Mvvm.ComponentModel;
using OpenTK.Graphics.ES20;
using ScottPlot.WPF;
using System.Windows;
using VEMS.MathCore;

namespace VEMS.Plot5
{
    /// <summary>
    /// view mode for Frame
    /// </summary>
    public partial class FrameViewModel : ObservableObject
    {
        #region fields

        /// <summary>
        /// title of the frame
        /// </summary>
        public TextQuantity Title = new()
        {
            Name = "Frame Title",
            Value = "VEMS Plot(s)",
            Unit = 20.0
        };

        /// <summary>
        /// label of the x-axis
        /// </summary>
        public TextQuantity LabelX = new()
        {
            Name = "X-Axis Label",
            Value = "X",
            Unit = 18.0
        };

        /// <summary>
        /// label of the y-axis
        /// </summary>
        public TextQuantity LabelY = new()
        {
            Name = "Y-Axis Label",
            Value = "Y",
            Unit = 18.0
        };

        /// <summary>
        /// label of the y-axis on the right
        /// </summary>
        public TextQuantity LabelYR = new()
        {
            Name = "Right Y-Axis Label",
            Value = "Y (Right)",
            Unit = 18.0
        };

        #endregion
        #region properties

        //[ObservableProperty]
        //private double fullWidth;

        //[ObservableProperty]
        //private double frameWidth;

        [ObservableProperty]
        private Visibility optColumnVisibility;

        [ObservableProperty]
        private double optColumnWidth;

        /// <summary>
        /// reference WpfPlot used as the dock
        /// </summary>
        [ObservableProperty]
        private WpfPlot dock;

        /// <summary>
        /// list of plots in the frame
        /// </summary>
        [ObservableProperty]
        private List<object> plots;





        #endregion
        #region constructor

        /// <summary>
        /// default constructor
        /// </summary>
        /// <param name="dock"> reference WpfPlot used as the dock </param>
        public FrameViewModel(WpfPlot dock)
        {
            // sets parameters
            Dock = dock;
            
            // option column control
            OptColumnWidth = 360;
            OptColumnVisibility = Visibility.Collapsed;
            if(OptColumnVisibility != Visibility.Visible) { OptColumnWidth = 0; }

            // DPI scaling
            Dock.Plot.ScaleFactor = 2.0;

            // initialization
            Plots = new();

        }

        #endregion
        #region methods

        /// <summary>
        /// adds a Grid1DRealPlot to the frame
        /// </summary>
        /// <param name="values"></param>
        /// <param name="grid"> data sampling information </param>
        /// <param name="plotProperty"> collection of plot properties </param>
        public Grid1DRealPlot AddGrid1DRealPlot(VectorD values,
            GridInfo1D? grid = null,
            Grid1DRealPlotPrty? plotProperty = null)
        {
            Grid1DRealPlot p = new(dock: Dock,
                values: values,
                grid: grid,
                plotProperty: plotProperty);
            Plots.Add(p);
            return p;
        }

        #endregion

    }
}
