// physical parameters
var waistRadius = 10.0;
// makes a Gaussian function first ...
Func<double, double> gx = (x) => Function1D.Gaussian(x, new List<double>(){ waistRadius }); 
var g = new Samp1DRealFunc(f: gx);
// ... and samples it on a grid
var uniGrid = new GridInfo1D(n: 201, spacing: 0.8); // will be changed after transforms
var v = g.Sample(grid: uniGrid); 
VFrame.CreateShow(values: v, grid: uniGrid, 
    title: "Input Gaussian", xLabel: "x", yLabel: "value");


// forward Fourier transform (FFT)
var y = new VectorZ(part: v, option: ComplexPart.RealPart); // will be changed after transforms
Transform.FFT1D(x: ref y, grid: ref uniGrid, 
    direction: FFTOptions.Direction.Forward,
    conversion: FFTOptions.Conversion.DataShift,
    copyMode: FFTOptions.CopyMode.Block,
    loopMode: FFTOptions.LoopMode.Sequential);
VFrame.CreateShow(values: VMath.Abs(y), grid: uniGrid,
    title: $"Forward FFT result [magnitude]", xLabel: "kx", yLabel: "value");


// forward Fourier transform => analytical result as a reference
Func<double, double> gk = (k) => waistRadius/Math.Sqrt(2.0) * Function1D.Gaussian(k, new List<double>(){ 2.0/waistRadius }); 
var af = new Samp1DRealFunc(f: gk);
var y0 = af.Sample(grid: uniGrid);
VFrame.CreateShow(values: y0, grid: uniGrid, 
    title: "Forward analytical transform result", xLabel: "kx", yLabel: "values");
// deviation between FFT and analytical results
var devAbs = VMath.StandardDeviation(VMath.Abs(y), VMath.Abs(y0));
Printer.WriteLine($"Deviation between FFT and analytical result: {devAbs}");


// backward transform
Transform.FFT1D(x: ref y, grid: ref uniGrid,
    direction: FFTOptions.Direction.Backward,
    conversion: FFTOptions.Conversion.DataShift,
    copyMode: FFTOptions.CopyMode.Block,
    loopMode: FFTOptions.LoopMode.Sequential);
VFrame.CreateShow(values: VMath.Abs(y), grid: uniGrid,
    title: $"Backward FFT result [magnitude]", xLabel: "x", yLabel: "values");