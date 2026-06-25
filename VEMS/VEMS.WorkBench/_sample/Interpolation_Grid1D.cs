// sampling parameters
var inputGrid = new GridInfo1D(n: 91, spacing: 1.0);
var targetGrid = new GridInfo1D(n: 920, spacing: 0.25);
var itpm = InterpolationMethod.Sinc;
var periodic = true;
// physical parameters
var w = 7.5;

// generate input data
var g = new Samp1DRealFunc(f: Function1D.Gaussian, p: new List<double>{ w, 0.0, 1.0 });
var input = g.Sample(grid: inputGrid);
VFrame.CreateShow(values: input, grid: inputGrid, title: "Gaussian", xLabel: "x", yLabel: "function value");

// performs interpolation explicitly
var intrpl = new Grid1DRealInterpolation(v: input, grid: inputGrid, 
    method: itpm, bound: DataBoundary.Periodic);
var res = intrpl.Evaluate(targetGrid: targetGrid);
VFrame.CreateShow(values: res, grid: targetGrid, title: "Interpolated Result (Explicit)", xLabel: "x", yLabel: "function value");

// alternatively ...
var dist = new Grid1DRealData(values: input, gridInfo: inputGrid, intrpl: itpm, bound: DataBoundary.Periodic);
var eval = dist.FindValues(targetGrid: targetGrid, LoopMode.Sequential);
VFrame.CreateShow(values: eval, grid: targetGrid, title: "Interpolated Result (Data)", xLabel: "x", yLabel: "function value");