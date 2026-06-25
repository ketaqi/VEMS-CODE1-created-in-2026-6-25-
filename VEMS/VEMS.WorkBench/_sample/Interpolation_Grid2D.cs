// sampling parameters
var inputGrid = new GridInfo2D(rows: 91, cols: 91, spacingY: 1.0, spacingX: 1.0);
var targetGrid = new GridInfo2D(rows: 720, cols: 181, spacingY: 0.33, spacingX: 0.33);
var itpm = InterpolationMethod.Sinc;
var periodic = true;
// physical parameters
var wx = 7.5;
var wy = 7.5;

// generate input data
var g = new SampXYRealFunc(f: new FunctionXY.Gaussian(),
    px: new List<double>{ wx, 0.0, 1.0 }, 
    py: new List<double>{ wy, 0.0, 1.0});
var input = g.Sample(grid: inputGrid);
VFrame.CreateShow(values: input, grid: inputGrid, title: "Gaussian", xLabel: "x", yLabel: "y");

// performs interpolation explicitly
var intrpl = new Grid2DRealInterpolation(v: input, grid: inputGrid, 
    method: itpm, boundX: DataBoundary.Periodic, boundY: DataBoundary.Periodic);
var res = intrpl.Evaluate(targetGrid: targetGrid, LoopMode.Parallel);
VFrame.CreateShow(values: res, grid: targetGrid, title: "interpolated result (explicit)", xLabel: "x", yLabel: "y");

// alternatively ...
var dist = new Grid2DRealData(values: input, gridInfo: inputGrid, intrpl: itpm, boundX: DataBoundary.Periodic, boundY: DataBoundary.Periodic);
var eval = dist.FindValues(targetGrid: targetGrid, LoopMode.Parallel);
VFrame.CreateShow(values: eval, grid: targetGrid, title: "Interpolated Result (Data)", xLabel: "x", yLabel: "y");