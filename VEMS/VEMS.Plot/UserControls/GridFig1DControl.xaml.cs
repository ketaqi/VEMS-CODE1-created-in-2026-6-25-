using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection.Emit;
using System.Windows.Controls;
using System.Windows.Markup;
using VEMS.MathCore;

namespace VEMS.Plot
{
    /// <summary>
    /// GridFig1DControl.xaml 的交互逻辑
    /// </summary>
    [Obsolete]
    public partial class GridFig1DControl : UserControl
    {
        public GridFig1DModel ViewModel;
        private const double DefaultLineWidth = 4.0;
        private const double DefaultMarkerSize = 7.0;
        private const string DefaultLabel = "Line";
        private const string DefaultFigTitle = "VEMS Gridded Plot(s)";
        private const string DefaultFigLabelX = "variable";
        private const string DefaultFigLabelY = "function value";

        // properties to expose
        public string FigTitle
        {
            get => ViewModel.FigTitle.Value;
            set => ViewModel.FigTitle.Value = value;
        }
        public string FigLabelX
        {
            get => ViewModel.FigLabelX.Value;
            set => ViewModel.FigLabelX.Value = value;
        }
        public string FigLabelY
        {
            get => ViewModel.FigLabelY.Value;
            set => ViewModel.FigLabelY.Value = value;
        }
        public bool NeedRightYAxis
        {
            get => ViewModel.NeedRightYAxis;
            set => ViewModel.NeedRightYAxis = value;
        }
        public string FigLabelYRight
        {
            get => ViewModel.FigLabelYRight.Value;
            set => ViewModel.FigLabelYRight.Value = value;
        }
        

        #region constructors
        
        /// <summary>
        /// default constructor
        /// </summary>
        public GridFig1DControl()
        {
            InitializeComponent();

            ViewModel = new(PlotFrame, 
                new double[1] { 0.0 },
                new GridInfo1D(1, 0.0, 0.25), 
                System.Drawing.Color.Black,
                DefaultLineWidth,
                DefaultMarkerSize,
                DefaultLabel,
                DefaultFigTitle,
                DefaultFigLabelX,
                DefaultFigLabelY);

            DataContext = ViewModel;
        }


        public GridFig1DControl(VectorD data, GridInfo1D grid,
            Color lineColor, double lineWidth, double markerSize,
            string label, string figTitle, 
            string figLabelX, string figLabelY)
        {
            InitializeComponent();

            DataContext = null;
            //model = new(PlotFrame,
            //    VMath.ConvertVectorToArray(data), grid,
            //    lineColor, lineWidth, markerSize,
            //    label, figTitle, figLabelX, figLabelY);

            //DataContext = model;
            PlotFrame.Refresh();
        }

        #endregion
        #region methods

        /// <summary>
        /// clear all the plots
        /// </summary>
        public void ClearAllPlots()
            => ViewModel.ClearAllPlots();


        public void AddGridPlot(double[] data, GridInfo1D grid,
            Color lineColor, double lineWidth, double markerSize,
            DrawOption visualOption, string label)
            => ViewModel.AddGridPlot(data, grid, lineColor, lineWidth,
                markerSize, visualOption, label);


        public void UpdateFigureText()
            => ViewModel.UpdateFigureText();


        public void AutoViewRange()
            => ViewModel.Frame.Plot.AxisAuto();


        public void Refresh()
            => ViewModel.Frame.Refresh();

        public void ChangePlot()
        {
            DataContext = null;

            ViewModel = new(PlotFrame,
                new double[5] { 1.0, 1.0, 1.0, 1.0, 1.0 },
                new GridInfo1D(5, 0.0, 0.25),
                System.Drawing.Color.Black,
                DefaultLineWidth,
                DefaultMarkerSize,
                DefaultLabel,
                DefaultFigTitle,
                DefaultFigLabelX,
                DefaultFigLabelY);

            DataContext = ViewModel;
            PlotFrame.Refresh();
        }

        #endregion

    }
}
