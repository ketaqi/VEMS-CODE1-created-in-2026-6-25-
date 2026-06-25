// sampling grid information
var grid = new GridInfo2D(rows: 151, cols: 151, spacingY: 1.0E-6, spacingX: 1.0E-6);

// Gaussian field parameters
var wavelength = 632.8E-9;
var wx = 3.5E-6; // waist of Gaussian; 3.5um
var wy = 3.5E-6;
// generate Gaussian field
//var v = new SCField.Gaussian(wavelength: wavelength, 
//    material: new FuncMaterial(nReal: 1.0), 
//    waistRadiusX: wx, waistRadiusY: wy, grid: grid);
// generate truncated plane wave
var v = new SCField.PlaneWave(wavelength: wavelength, 
    material: new FuncMaterial(nReal: 1.0), shape: ApertureShape.Elliptical, 
    diameterX: 20*wx, diameterY: 20*wy, edge: wx, grid: grid);
    
// prepares detector
var det = new Detector2D(grid: grid);

// input display
VFrame.CreateShow(values: det.Sample(v, quantity: DetectQuantity.Magnitude), 
    grid: v.U.GridInfo, Options.PlotColormap.Magma,
    title: "Input Abs[E] " + v.Domain.ToString(), xLabel: "x [m]", yLabel: "y [m]");

// propagate by distance d
var d = 675E-6; // 675 um 
v.Propagate(d: d, ModelingDomain.Spatial, LoopMode.Sequential, xSizeFactor: 1.0); // this method calls SPW internally

// output display
VFrame.CreateShow(values: det.Sample(v, quantity: DetectQuantity.Magnitude), 
    grid: v.U.GridInfo, Options.PlotColormap.Magma,
    title: "Propagated Abs[E] " + v.Domain.ToString(), xLabel: "x [m]", yLabel: "y [m]");