using System;
using System.Collections.Generic;
using System.Windows;
using VEMS.MathCore;
using VEMS.Plot;

using Color = System.Drawing.Color;

namespace VEMS.WorkBench
{
    internal class FigTest
    {

        private const string DefaultLineColor = "SteelBlue";
        private const string DefaultColorMap = "Grayscale";
        private const double DefaultLineWidth = 4.0;
        private const double DefaultMarkerSize = 7.0;
        private const string DefaultLine = "Line";
        private const string DefaultGraph = "Graph";
        private const string DefaultFigTitle = "VEMS Plot(s)";
        private const string DefaultFigLabelX = "x";
        private const string DefaultFigLabelY = "y";



        /// <summary>
        /// plots a grid vector data
        /// </summary>
        /// <param name="data"> grid data to plot </param>
        /// <param name="lineColorName"> line color </param>
        /// <param name="lineWidth"> line width </param>
        /// <param name="markerSize"> marker size </param>
        /// <param name="label"> label text </param>
        /// <param name="figTitle"> figure title text </param>
        /// <param name="figLabelX"> figure label text along x </param>
        /// <param name="figLabelY"> figure label text along y </param>
        public static void Show(Grid1DRealData data,
            string figTitle = "Grid Data 1D",
            string figLabelX = DefaultFigLabelX,
            string figLabelY = DefaultFigLabelY,
            double lineWidth = DefaultLineWidth,
            double markerSize = DefaultMarkerSize,
            string lineColorName = DefaultLineColor,
            string label = DefaultLine)
        {
            Application.Current.Dispatcher.Invoke((Action)delegate
            {
                GridFig1D fig1D = new(data, Color.FromName(lineColorName),
                    lineWidth, markerSize, label,
                    figTitle, figLabelX, figLabelY);
                fig1D.Show();
            });
            //((App)Application.Current).Frames.Add()
        }

        /// <summary>
        /// plots a vector
        /// </summary>
        /// <param name="data"> vector to plot </param>
        /// <param name="lineColorName"> line color </param>
        /// <param name="lineWidth"> line width </param>
        /// <param name="markerSize"> marker size </param>
        /// <param name="label"> label text </param>
        /// <param name="figTitle"> figure title text </param>
        /// <param name="figLabelX"> figure label text along x </param>
        /// <param name="figLabelY"> figure label text along y </param>
        public static void Show(VectorD data,
            string figTitle = "Vector",
            string figLabelX = DefaultFigLabelX,
            string figLabelY = DefaultFigLabelY,
            double lineWidth = DefaultLineWidth,
            double markerSize = DefaultMarkerSize,
            string lineColorName = DefaultLineColor,
            string label = DefaultLine)
        {
            Application.Current.Dispatcher.Invoke((Action)delegate
            {
                GridFig1D fig1D = new(new Grid1DRealData(data), Color.FromName(lineColorName),
                    lineWidth, markerSize, label,
                    figTitle, figLabelX, figLabelY);
                fig1D.Show();
            });
        }

        /// <summary>
        /// plots a list of grid vector data
        /// </summary>
        /// <param name="data"> list of grid data to plot </param>
        /// <param name="lineColor"> list of line color </param>
        /// <param name="lineWidth"> line width </param>
        /// <param name="markerSize"> marker size </param>
        /// <param name="label"> list of label text </param>
        /// <param name="figTitle"> figure title text </param>
        /// <param name="figLabelX"> figure label text along x </param>
        /// <param name="figLabelY"> figure label text along y </param>
        public static void Show(List<Grid1DRealData> data,
            string figTitle = "Grid Data 1D",
            string figLabelX = DefaultFigLabelX,
            string figLabelY = DefaultFigLabelY,
            double lineWidth = DefaultLineWidth,
            double markerSize = DefaultMarkerSize,
            List<Color> lineColor = null,
            List<string> label = null)
        {
            Application.Current.Dispatcher.Invoke((Action)delegate
            {
                GridFig1D fig1D = new(data, lineColor,
                    lineWidth, markerSize, label,
                    figTitle, figLabelX, figLabelY);
                fig1D.Show();
            });
        }
    }
}
