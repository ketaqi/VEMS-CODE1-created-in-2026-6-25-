// physical parameters
var lambda = 193.368E-9; // wavelength
var rIndex = 1.0; // refractive index
var na = 0.3; // numerical aperture
var dist = 0.75E-3; // point distance
var focalShift = 0.02E-3; // shift from actual focal plane
var p = 10.0E-6; // grating period
var sourceEdge = 25.0E-6;
var gratingEdge = 1.0E-6;
// sampling parameters
var wx = 1.0E-3; var wy = 0.6E-3; // window sizes
var nx = 5001; var ny = 3001; // number of samples
var gxy = new GridInfo2D(rows: ny, cols: nx, spacingY: wy/ny, spacingX: wx/nx);
// prepare detectors
var detFine = new Detector2D(grid: gxy);
var nu = 501; var nv = 251;
var detCoarse = new Detector2D(grid: new GridInfo2D(rows: nv, cols: nu, 
    spacingY: wy/nv, spacingX: wx/nu));
// timer ...
var sw = new Stopwatch();


// creates input field [exit pupil]
var ax = Math.Asin(na/rIndex);
var ay = Math.Asin(na/rIndex);
sw.Start();
var v = new ScalarField.SphericalWave(wavelength: lambda, n: rIndex, z: -dist,
    alphaX: ax, alphaY: ay, grid: gxy, shape: ApertureShape.Circular, edge: sourceEdge,
    loopMode: LoopMode.Parallel);
sw.Stop();
Printer.Logging($"input field generated => time cost: {sw.ElapsedMilliseconds} [ms]");


// adds aberration
var aber = new Aberration2D.Zernike(idx: 6, 
    indexing: CommonFunction.ZernikeIndexing.Fringe,
    refRadius: dist * Math.Tan(ax) + sourceEdge, 
    scaling: 1.5);
sw.Restart();
aber.Modulate(v: ref v, loopMode: LoopMode.Parallel);
sw.Stop();
Printer.Logging($"aberration added => time cost: {sw.ElapsedMilliseconds} [ms]");
// input display
//VFrame.CreateShow(values: VMath.Abs(v.Field), grid: v.Grid, title: $"input amplitude");
//VFrame.CreateShow(values: VMath.Arg(v.Field), grid: v.Grid, title: $"input phase");


// propagates to grating
sw.Restart();
v.SwitchToKDomain();
FreeSpace.SPW2D(v: ref v, distance: dist - focalShift, loopMode: LoopMode.Parallel);
sw.Stop();
Printer.Logging($"propagated to grating => time cost: {sw.ElapsedMilliseconds} [ms]");
// display
//VFrame.CreateShow(values: VMath.Abs(v.Field), grid: v.Grid, title: $"amplitude in front of grating");
//VFrame.CreateShow(values: VMath.Arg(v.Field), grid: v.Grid, title: $"phase in front of grating");
    

// grating transmission 
var t = new IdealGrating1D.Rectangular(period: p, width: 0.5 * p, 
    edgeWidth: gratingEdge, 
    scaling: 1.0, 
    shift: 0.0 * p,
    type: TransmissionType.Amplitude);
sw.Restart();
t.ModulateInKDomain(v: ref v, isAlongX: true, minOrder: -3, maxOrder: 3,
    intrpl: InterpolationMethod.Linear,
    loopMode: LoopMode.Parallel);
sw.Stop();
Printer.Logging($"grating modulation added => time cost: {sw.ElapsedMilliseconds} [ms]");
// dispaly
//VFrame.CreateShow(values: VMath.Abs(v.Field), grid: v.Grid, title: $"amplitude behind grating");
//VFrame.CreateShow(values: VMath.Arg(v.Field), grid: v.Grid, title: $"phase behind grating");


// propagates to detector
sw.Restart();
FreeSpace.SPW2D(v: ref v, distance: dist + focalShift, loopMode: LoopMode.Parallel);
v.SwitchToXDomain();
sw.Stop();
Printer.Logging($"propagated to detector => time cost: {sw.ElapsedMilliseconds} [ms]");


// detects and display squared magnitude
VFrame.CreateShow(values: detCoarse.Sample(v: v, quantity: DetectQuantity.Magnitude), 
    grid: detCoarse.GridInfo, 
    colormap: Options.PlotColormap.Jet,
    title: $"squared amplitude at detector");