// sampling grid information
var nx = 1501;
var ny = 1501;
var wx = 1.5E-3;
var wy = 1.5E-3;
var grid = new GridInfo2D(rows: ny, cols: nx, spacingY: wy/ny, spacingX: wx/nx);

// Gaussian field parameters
var wavelength = 632.8E-9;
var dx = 200.0E-6; // waist of Gaussian;
var dy = 200.0E-6;
// generate truncated plane wave
var v = new ScalarField.PlaneWave(wavelength: wavelength, n: 1.0,
    diameterX: dx, diameterY: dy, grid: grid, shape: ApertureShape.Circular, 
    edge: 10.0 * wavelength,
    loopMode: LoopMode.Parallel);

// input display
VFrame.CreateShow(values: VMath.Abs(v.Field), grid: v.Grid, 
    colormap: Options.PlotColormap.Jet,
    title: $"Input Abs[E] {v.Domain}", xLabel: "x [m]", yLabel: "y [m]");


// defines transmission
// 1) rectangular aperture
//var t = new Aperture2D.Rectangular(diameterX: 20.0E-6, diameterY: 10.0E-6,
//    edgeWidth: 5.0 * wavelength);
//t.Modulate(v: ref v, loopMode: LoopMode.Parallel);
// 2) circular aperture
//var t = new Aperture2D.Ellptical(diameterX: 15.0E-6, diameterY: 15.0E-6,
//    edgeWidth: 5.0 * wavelength);
//t.Modulate(v: ref v, loopMode: LoopMode.Parallel);
// 3) grating modulation
//var t = new IdealGrating1D.Sinusoidal(period: 10.0E-6);
//t.Modulate(v: ref v, isAlongX: true, loopMode: LoopMode.Parallel);
// display
VFrame.CreateShow(values: VMath.Abs(v.Field), grid: v.Grid, 
    colormap: Options.PlotColormap.Jet,
    title: $"Transmitted Abs[E] {v.Domain}", xLabel: "x [m]", yLabel: "y [m]");

// propagate by distance d
var d = 3000E-6; // 500 um 
v.Propagate(d: d, ModelingDomain.Spatial, LoopMode.Sequential, sizeFactor: 1.0); // this method calls SPW internally

// output display
VFrame.CreateShow(values: VMath.Abs(v.Field), grid: v.Grid, 
    colormap: Options.PlotColormap.Jet,
    title: $"Propagated Abs[E] {v.Domain}", xLabel: "x [m]", yLabel: "y [m]");