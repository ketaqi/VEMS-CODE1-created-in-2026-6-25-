var v = new VectorD(7, 0.0, 0.33);
var vIm = VStat.RngUniform(7, 0.0, 1.0);
var p = new VectorD(7, 0.0)
{
    [0] = -1.6,
    [1] = -1.0,
    [2] = -0.8,
    [3] = -0.1,
    [4] = 0.15,
    [5] = 0.9,
    [6] = 1.33
};

var g = new GridInfo1D(7, 0.5);

// creates a VFrame
var f = VFrame.CreateFrame();
VFrame.SetTitle(f, "Frame Fundamentals");

// add Grid Plot to the frame
VFrame.AddToFrame(f, 
    values: VMath.Construct(v, vIm),
    grid: g,
    plotPart: ComplexPart.RealPart, 
    lineWidth: 5.0,
    markerSize: 12.0,
    plotColor: Options.PlotColor.Brown);
    
// add Scat Plot to the frame
VFrame.AddToFrame(f, locations: p, values: v,
    lineWidth: 3.0,
    lineStyle: Options.LineStyle.Solid,
    markerSize: 12.0,
    markerShape: Options.MarkerShape.cross,
    plotColor: Options.PlotColor.SteelBlue);
    
// add Func Plot to the frame
var func = new Func<double, Complex?>((x) => Math.Sin(x) * Math.Sin(x / 2));
VFrame.AddToFrame(f, func: func, plotPart: ComplexPart.RealPart,
    lineWidth: 1.0,
    plotColor: Options.PlotColor.Blue);

// refresh and display
VFrame.SetLegend(f, location: Options.LegendLocation.UpperLeft, fontSize: 20.0);
VFrame.RefreshShow(f);