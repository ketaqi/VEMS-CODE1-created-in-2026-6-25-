// parameters
var wavelength = 632.8E-9;
var diameter = 100.0E-6;
var na = 0.5;
var nAir = 1.0;
// sampling
var nx = 2001; var ny = 2001;
var wx = 200.0E-6; var wy = 200.0E-6;
var g = new GridInfo2D(rows: ny, cols: nx,
    spacingY: wy/ny, spacingX: wx/nx);


// prepares input plane wave
var v = new ScalarField.PlaneWave(wavelength: wavelength, n: nAir,
    diameterX: diameter, diameterY: diameter, grid: g,
    shape: ApertureShape.Circular, edge: 5.0*wavelength,
    loopMode: LoopMode.Parallel);
// display
//VFrame.CreateShow(values: v.Field, grid: v.Grid, title: "input plane wave");


// creates lens transmission
var a = Math.Asin(na/nAir);
var f = 0.5 * diameter / Math.Tan(a);
//var t = new IdealLens2D.Quadratic(focalLength: f, 
//    wavelength: wavelength, nReal: (w) => nAir);
var t = new IdealLens2D.Spheric(focalLength: f, 
    wavelength: wavelength, nReal: (w) => nAir);
// modulates on the input
t.Modulate(ref v, loopMode: LoopMode.Parallel);
// display
//VFrame.CreateShow(values: v.Field, grid: v.Grid, title: "field behind lens");


// propagates to focal plane
//v.SwitchToKDomain();
//FreeSpace.SPW2D(ref v, distance: f, loopMode: LoopMode.Parallel);
//v.SwitchToXDomain();
FreeSpace.Fresnel2D(ref v, distance: f, loopMode: LoopMode.Parallel);
// display
VFrame.CreateShow(values: v.Field, grid: v.Grid, title: "field at focal plane",
    colormap: Options.PlotColormap.Jet);