// sampling grid information
var grid = new GridInfo2D(rows: 151, cols: 151, spacingY: 1.0E-6, spacingX: 1.0E-6);

// Gaussian field parameters
var wavelength = 632.8E-9;
var wx = 2.5E-6; // waist of Gaussian; 3.5um
var wy = 25E-6;
// generate Gaussian field
var v = new ScalarField.Gaussian(wavelength: wavelength, n: Complex.One, waistRadiusX: wx, waistRadiusY: wy, grid: grid);
// generate truncated plane wave

// input display
VFrame.CreateShow(values: v.Field, grid: v.Grid, ComplexPart.Magnitude, Options.PlotColormap.Magma,
    title: "Input Abs[E] " + v.Domain.ToString(), xLabel: "x [m]", yLabel: "y [m]");

// propagate by distance d
var d = 500E-6; // 500 um 
v.Propagate(d: d, ModelingDomain.Spatial, LoopMode.Sequential, sizeFactor: 1.0); // this method calls SPW internally

// output display
VFrame.CreateShow(values: v.Field, grid: v.Grid, ComplexPart.Magnitude, Options.PlotColormap.Magma,
    title: "Propagated Abs[E] " + v.Domain.ToString(), xLabel: "x [m]", yLabel: "y [m]");