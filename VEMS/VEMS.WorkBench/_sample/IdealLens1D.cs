// physical parameters
var wavelength = 500.0E-9;
var f = 20E-3;
var vacuum = new FuncMaterial(nReal: 1.0);


// sampling parameters for field and detector
var nx = 901;
var dx = 1E-6;
var gx = new GridInfo1D(n: nx, spacing: dx);
var det = new Detector1D(grid: gx); 


// defines an idealized lens function [quadratic phase]
var lens = new IdealLens1D.Quadratic(focalLength: f, 
    wavelength: wavelength, 
    nReal: (w) => vacuum.NReal(wavelength));
// defines an idealized lens function [cylindric phase]
//var lens = new IdealLens1D.Cylindric(focalLength: f, 
//    wavelength: wavelength, 
//    nReal: (w) => vacuum.NReal(wavelength));


// prepares the input planewave
var v = new SCField1D.PlaneWave(wavelength: wavelength, 
    material: vacuum,
    diameter: 0.7E-3, 
    grid: gx, // using default sampling ...
    edge: 0.05 * 0.7E-3,
    shiftX: 0.0E-3);
var v0 = det.Sample(v, quantity: DetectQuantity.Magnitude, loopMode: LoopMode.Parallel);
VFrame.CreateShow(values: v0, grid: det.GridInfo * 1E3, // scaling to [mm] unit
    title: $"Input Field [Magnitude]", xLabel: $"x [mm]", yLabel: $"value [a.u.]");


// applies the lens modulation
lens.ModulateOn(v: ref v, loopMode: LoopMode.Parallel);
var v1 = det.Sample(v, quantity: DetectQuantity.Argument, loopMode: LoopMode.Parallel);
VFrame.CreateShow(values: v1, grid: det.GridInfo * 1E3, // scaling to [mm] unit
    title: $"Field behind Lens [Phase]", xLabel: $"x [mm]", yLabel: $"value [a.u.]");


// propagates in free space
v.Propagate(d: f, targetDomain: ModelingDomain.Spatial);
var v2 = det.Sample(v, quantity: DetectQuantity.Magnitude, loopMode: LoopMode.Parallel);
VFrame.CreateShow(values: v2, grid: det.GridInfo * 1E3, // scaling to [mm] unit
    title: "Field on Focal Plane [Magnitude]", xLabel: $"x [mm]", yLabel: $"value [a.u]");