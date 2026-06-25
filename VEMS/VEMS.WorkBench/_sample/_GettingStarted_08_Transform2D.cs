// physical parameters
var wx = 12.5;
var wy = 12.5;
// makes a rectangular function first ...
var rect = new SampXYRealFunc(f: new FunctionXY.CosEdgeRectangle(), 
    px: new List<double>{ wx, 0.1 * wx }, 
    py: new List<double>{ wy, 0.1 * wy });
// ... and samples it on a grid
var uniGrid = new GridInfo2D(rows: 501, cols: 501, spacingY: 0.1, spacingX: 0.1); // will change after transforms 
var v = rect.Sample(grid: uniGrid);
VFrame.CreateShow(values: v, grid: uniGrid,
    title: "Input", xLabel: "x", yLabel: "y");

// forward Fourier transform (FFT)
var a = new MatrixZ(part: v, option: ComplexPart.RealPart); // will change after transforms
Transform.FFT2D(x: ref a, grid: ref uniGrid, 
    direction: FFTOptions.Direction.Forward,
    conversion: FFTOptions.Conversion.DataShift,
    copyMode: FFTOptions.CopyMode.Block,
    loopMode: FFTOptions.LoopMode.Sequential);
VFrame.CreateShow(values: VMath.Abs(a), grid: uniGrid,
    title: "FFT result [magnitude]", xLabel: "kx", yLabel: "ky");

// backward Fourier transform (FFT)
Transform.FFT2D(x: ref a, grid: ref uniGrid, 
    direction: FFTOptions.Direction.Backward,
    conversion: FFTOptions.Conversion.DataShift,
    copyMode: FFTOptions.CopyMode.Block,
    loopMode: FFTOptions.LoopMode.Sequential);
VFrame.CreateShow(values: VMath.Abs(a), grid: uniGrid,
    title: "IFFT result [magnitude]", xLabel: "x", yLabel: "y");
// deviation
var devAbs = VMath.StandardDeviation(VMath.Abs(a), VMath.Abs(v));
Printer.WriteLine($"Deviation between IFFT result and input: {devAbs}");