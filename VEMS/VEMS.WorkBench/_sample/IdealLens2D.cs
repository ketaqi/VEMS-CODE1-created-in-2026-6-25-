// physical parameters
var wavelength = 632.8E-9;
var f = 20.0E-3;
var vacuum = new FuncMaterial(nReal: 1.0);


// sampling parameters for field and detector
var nx = 901; var ny = 901;
var dx = 1.0E-6; var dy = 1.0E-6;
var g = new GridInfo2D(rows: ny, cols: nx, spacingY: dy, spacingX: dx);
var det = new Detector2D(grid: g);


// defines an idealized lens function [quadratic phase]
var lens = new IdealLens2D.Quadratic(focalLength: f, 
    wavelength: wavelength, 
    nReal: (w) => vacuum.NReal(wavelength),
    offset: 0.0, shiftX: 0.0, shiftY: 0.0);
//var lens = new IdealLens2D.Spheric(focalLength: f,
//    wavelength: wavelength,
//    nReal: (w) => vacuum.NReal(wavelength));


// prepares input Gaussian field
var v = new SCField.Gaussian(wavelength: wavelength,
    material: vacuum,
    waistRadiusX: 250.0E-6, 
    waistRadiusY: 250.0E-6,
    grid: g, // using default sampling ...
    shiftX: 0.0, shiftY: 0.0);
var v0 = det.Sample(v, quantity: DetectQuantity.Magnitude, loopMode: LoopMode.Parallel);
VFrame.CreateShow(values: v0, grid: det.GridInfo * (1E3, 1E3), // scaling to [mm] unit
    title: $"Input Field [Magnitude]", xLabel: $"x [mm]", yLabel: $"y [mm]",
    colormap: Options.PlotColormap.Jet);


// applies the lens modulation
lens.ModulateOn(v: ref v, loopMode: LoopMode.Parallel);
var v1 = det.Sample(v, quantity: DetectQuantity.Argument, loopMode: LoopMode.Parallel);
VFrame.CreateShow(values: v1, grid: det.GridInfo * (1E3, 1E3), // scaling to [mm] unit
    title: "Field behind Lens [Phase]", xLabel: $"x [mm]", yLabel: $"y [mm]",
    colormap: Options.PlotColormap.Grayscale);


// propagates in free space
v.Propagate(d: f, targetDomain: ModelingDomain.Spatial);
var v2 = det.Sample(v, quantity: DetectQuantity.Magnitude);
VFrame.CreateShow(values: v2, grid: det.GridInfo * (1E3, 1E3), 
    title: "Field on Focal Plane [Magnitude]", xLabel: $"x [mm]", yLabel: $"y [mm]",
    colormap: Options.PlotColormap.Jet);