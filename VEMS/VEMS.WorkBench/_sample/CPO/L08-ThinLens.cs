// sampling grid information
var nx = 2001;
var ny = 2001;
var wx = 1.0E-3;
var wy = 1.0E-3;
var grid = new GridInfo2D(rows: ny, cols: nx, spacingY: wy/ny, spacingX: wx/nx);

// Gaussian field parameters
var wavelength = 632.8E-9;
var dx = 750.0E-6; // waist of Gaussian;
var dy = 750.0E-6;
// generate truncated plane wave
var v = new ScalarField.PlaneWave(wavelength: wavelength, n: 1.0,
    diameterX: dx, diameterY: dy, grid: grid, shape: ApertureShape.Circular, 
    edge: 10.0 * wavelength,
    loopMode: LoopMode.Parallel);

// input display
VFrame.CreateShow(values: VMath.Abs(v.Field), grid: v.Grid, 
    colormap: Options.PlotColormap.Jet,
    title: $"Input Abs[E] {v.Domain}", xLabel: "x [m]", yLabel: "y [m]");


// defines thin lens
var t = new IdealLens2D.Spheric(focalLength: 3000E-6, wavelength: wavelength,
    nReal: (w) => 1.0);
t.Modulate(v: ref v, loopMode: LoopMode.Parallel);
// display
VFrame.CreateShow(values: VMath.Arg(v.Field), grid: v.Grid, 
    colormap: Options.PlotColormap.Grayscale,
    title: $"Transmitted Arg[E] {v.Domain}", xLabel: "x [m]", yLabel: "y [m]");

// propagate by distance d
var d = 3000E-6; // 500 um 
v.Propagate(d: d, ModelingDomain.Spatial, LoopMode.Sequential, sizeFactor: 1.0); // this method calls SPW internally

// output display
VFrame.CreateShow(values: VMath.Abs(v.Field), grid: v.Grid, 
    colormap: Options.PlotColormap.Jet,
    title: $"Propagated Abs[E] {v.Domain}", xLabel: "x [m]", yLabel: "y [m]");