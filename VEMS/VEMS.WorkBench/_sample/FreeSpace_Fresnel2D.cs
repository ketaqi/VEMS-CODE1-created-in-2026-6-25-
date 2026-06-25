// sampling parameters for field and detector
var nx = 151; var ny = 151;
var dx = 1.0E-6; var dy = 1.0E-6;
var g = new GridInfo2D(rows: ny, cols: nx, spacingY: dy, spacingX: dx);
var det = new Detector2D(grid: g);


// Gaussian field parameters
var wavelength = 632.8E-9;
var wx = 3.5E-6; // waist of Gaussian; 3.5um
var wy = 3.5E-6;
// generate Gaussian field
var v = new SCField.Gaussian(
    wavelength: wavelength,
    material: new FuncMaterial(nReal: 1.0),
    waistRadiusX: wx, waistRadiusY: wy,
    grid: g);


// detect input
var vIn = det.Sample(v: v, quantity: DetectQuantity.Magnitude);
VFrame.CreateShow(values: vIn, title: $"Input E-Field [Magnitude]", 
    grid: det.GridInfo * (1E3, 1E3), // scaled to [mm] unit 
    xLabel: "x [mm]", yLabel: "y [mm]");


// propagation by Fresnel diffraction integral
var d = 675E-6;
FreeSpace.Fresnel2D(v: ref v, distance: d, loopMode: LoopMode.Parallel);


// detect output
var vOut = det.Sample(v: v, quantity: DetectQuantity.Magnitude);
VFrame.CreateShow(values: vOut, title: $"Propagared E-Field [Magnitude]",
    grid: det.GridInfo * (1E3, 1E3), // scaled to [mm] unit 
     xLabel: "x [mm]", yLabel: "y [mm]");