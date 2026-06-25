// defines sampling grid
var grid = new GridInfo1D(n: 301, spacing: 0.33);
// defines window function parameters
var shift = 0.0;
var scale = 1.0;
var gaussianWaist = 15.0;
var rectWidth = 50.0;
var relativeEdgeRatio = 0.33;


// constructs Gaussian function
var g = new Samp1DRealFunc(f: Function1D.Gaussian, 
    p: new List<double>{ gaussianWaist, shift, scale });
// constructs rectangular function
var r = new Samp1DRealFunc(f: Function1D.CosEdgeRectangle,
    p: new List<double>{ rectWidth, relativeEdgeRatio * rectWidth, shift, scale });
// and, samples the functions on uniform grid
var sg = g.Sample(grid);
var sr = r.Sample(grid);


// creates a VFrame and displays the sampled functions
var f = VFrame.CreateFrame();
VFrame.SetTitle(f, content: "Frame Fundamentals");
VFrame.SetLabelX(f, content: "x");
VFrame.SetLabelY(f, content: "value(s)");
VFrame.AddToFrame(f: f, values: sg, grid: grid, label: "Gaussian",
    lineWidth: 5.0, markerSize: 12.0, plotColor: Options.PlotColor.Brown);
VFrame.AddToFrame(f: f, values: sr, grid: grid, label: "rectangle",
    lineWidth: 5.0, markerSize: 12.0, plotColor: Options.PlotColor.SteelBlue);
VFrame.RefreshShow(f);