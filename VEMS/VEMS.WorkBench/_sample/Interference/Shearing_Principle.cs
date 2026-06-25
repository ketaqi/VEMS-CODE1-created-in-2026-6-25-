// physical parameters
var shift = 6.9E-6; // lateral shift distance
var shearingAlongX = false;
// sampling parameters
var wx = 0.5E-3; var wy = 0.5E-3; // window sizes
var nx = 1001; var ny = 1001; // number of samples
var gxy = new GridInfo2D(rows: ny, cols: nx, spacingY: wy/ny, spacingX: wx/nx);


// creates complex data containing the input test phase
var a = new Aperture2D.Ellptical(diameterX: 0.9*wx, diameterY: 0.9*wy, edgeWidth: 5E-6);
//var t = new IdealGrating1D.Sinusoidal(period: 15E-6, 
//    scaling: 1.0, type: TransmissionType.Phase);
var t = new Aberration2D.Zernike(idx: 6, 
    indexing: CommonFunction.ZernikeIndexing.Fringe,
    refRadius: 0.5 * 0.9 * wx, scaling: 12.5);
// sampling
var va = a.Sample(grid: gxy, loopMode: LoopMode.Parallel);
var vt = t.Sample(grid: gxy, loopMode: LoopMode.Parallel);
var vIn = va * vt;
// display
VFrame.CreateShow(values: vIn, grid: gxy, plotPart: ComplexPart.Argument, 
    title: $"input phase");


// adds shift manually
var guv = shearingAlongX ? 
    gxy.Modify(ctrShiftX: shift, ctrShiftY: 0.0) : gxy.Modify(ctrShiftX: 0.0, ctrShiftY: shift);
var dSt = new Grid2DCplxData(values: vIn, gridInfo: guv, 
    intrpl: InterpolationMethod.Linear);
var vSt = dSt.FindValues(targetGrid: gxy, loopMode: LoopMode.Parallel);
// sums up two fields
var vSum = vIn + vSt;
// display
VFrame.CreateShow(values: vSum, grid: gxy, 
    colormap: Options.PlotColormap.Jet,
    title: $"interference amplitude");