// input samples
int usCount = 451;
var start = -4.0;
var end = 4.0;
var us = //new VectorD(initVal: start, endVal: end, count: usCount);
    new VectorD(count: usCount, initVal: start, increment: (end-start)/(usCount-1));
var ys = VMath.Sin(us); 


// B-spline class construction
var fitter = new BSpline1D(xs: us, ys: ys,
    degree: 5,
    nFactor: 0.7,
    knotsType: BSpline.KnotsType.Clamped,
    checkFitError: false);


// evaluation positions
var num = 181;
var spacing = (end - start) / (num-1);
var evaGrid = new GridInfo1D(num, start, spacing);
// evaluation
var ye = fitter.Evaluate(evaGrid.GetCoordinates());
// derivative
var dy = fitter.Derivative(evaGrid.GetCoordinates(), 1);


// display
var fig = VFrame.CreateFrame();
// input samples
VFrame.AddToFrame(f: fig, locations: us, values: ys,
    lineWidth: 0.0,  
    markerShape: Options.MarkerShape.cross,
    markerSize: 12.0,
    label: "Input Samples");
// fitted result
VFrame.AddToFrame(f: fig, 
    values: dy, //ye,
    grid: evaGrid,
    plotColor: Options.PlotColor.SteelBlue,
    label: "fitting result");
// show
VFrame.SetFrameParameters(f: fig, title: "B-Spline Fitting", xLabel: "x", yLabel: "y");
VFrame.RefreshShow(fig);


// deviation w.r.t. reference
var y0 = VMath.Sin(evaGrid.GetCoordinates());
var dy0 = VMath.Cos(evaGrid.GetCoordinates()); 
Printer.Write($"Deviation of function values: {VMath.StandardDeviation(y0, ye)}");
Printer.Write($"Deviation of derivatives: {VMath.StandardDeviation(dy0, dy)}");