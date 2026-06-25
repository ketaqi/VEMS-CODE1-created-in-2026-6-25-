// parameters
var waistRadius = 2.50; 
var shift = 7.50;
var w = 2.5; // waist radius
var x0 = 7.5; // shift 
// input function in x-domain
Func<double, Complex> xGauss = (x) => Math.Exp(- Math.Pow((x-x0)/w, 2.0)); 

// sampling grid in x-domain
var xGrid = new GridInfo1D(n: 501, spacing: 0.5); 
var xv = new Samp1DCplxFunc(f: xGauss).Sample(grid: xGrid);
var xFrame = VFrame.CreateFrame();
VFrame.AddToFrame(f: xFrame, values: VMath.RealPart(xv), grid: xGrid, plotColor: Options.PlotColor.SteelBlue, label: "Re[f(x)]");
VFrame.AddToFrame(f: xFrame, values: VMath.ImagPart(xv), grid: xGrid, plotColor: Options.PlotColor.Gray, label: "Im[f(x)]");
VFrame.SetFrameParameters(f: xFrame, title: "Input Gaussian", xLabel: "x", yLabel: "f(x)");
VFrame.RefreshShow(xFrame);

// Fourier transform
var kv = new VectorZ(other: xv, deepCopy: true);
var kGrid = new GridInfo1D(other: xGrid);
Transform.FFT1D(x: ref kv, grid: ref kGrid, option: FTOption.Forward);
var kFrame = VFrame.CreateFrame();
VFrame.AddToFrame(f: kFrame, values: VMath.RealPart(kv), grid: kGrid, plotColor: Options.PlotColor.SteelBlue, label: "Re[F(kx)]");
VFrame.AddToFrame(f: kFrame, values: VMath.ImagPart(kv), grid: kGrid, plotColor: Options.PlotColor.Gray, label: "Im[F(kx)]");
VFrame.SetFrameParameters(f: kFrame, title: "Transformed Result", xLabel: "kx", yLabel: "F(kx)");
VFrame.RefreshShow(kFrame);