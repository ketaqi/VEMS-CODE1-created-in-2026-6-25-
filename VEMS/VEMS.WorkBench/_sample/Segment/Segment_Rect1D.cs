// defines sampling grid
var gx = new GridInfo1D(n: 101, spacing: 1.25);
// defines parameters
var waistRadius = 15.0;
var segWidth = 2.5;
var edgeRatio = 0.5;


// example #0: func => data
// constructs Gaussian function as the input
Func<double, double> gauss = (x) => Function1D.Gaussian(x, new List<double>{ waistRadius });
var d = new Grid1DRealData(values: new Samp1DRealFunc(gauss).Sample(gx),
    gridInfo: gx, intrpl: InterpolationMethod.Sinc, bound: DataBoundary.ConstantZero);
VFrame.CreateShow(values: d.Values, grid: gx, title: "input Gaussian");
// defines segment and takes samples from input
var s = new SEG1D.CosRect(diameter: segWidth, edge: edgeRatio * segWidth);
var gt = new GridInfo1D(n: 51, spacing: 0.1);
gt.GetModified(ctrShift: 20.0);
var sd = s.TakeFrom(fIn: gauss, x0: gt.Center, gTarget: gt);
VFrame.CreateShow(values: sd.Values, grid: sd.GridInfo, title: "segmented data");


/*
// example #1: data => data
// constructs Gaussian data distribution
var d = new Grid1DRealData(values: new Samp1DRealFunc(gauss).Sample(gx),
    gridInfo: gx, intrpl: InterpolationMethod.Sinc, bound: DataBoundary.ConstantZero);
VFrame.CreateShow(values: d.Values, grid: gx, title: "input Gaussian");
var sd = s.TakeFrom(dIn: d, gTarget: gt, loopMode: LoopMode.Parallel);
VFrame.CreateShow(values: sd.Values, grid: sd.GridInfo, title: "segmented data");
*/