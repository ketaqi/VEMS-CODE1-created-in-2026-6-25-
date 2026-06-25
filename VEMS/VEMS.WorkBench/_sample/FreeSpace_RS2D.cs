// sampling parameters for field and detector
var nx = 151; var ny = 151;
var dx = 1.0E-6; var dy = 1.0E-6;
var g = new GridInfo2D(rows: ny, cols: nx, spacingY: dy, spacingX: dx);
var det = new Detector2D(grid: g);


// input field parameters
var wavelength = 632.8E-9;
var wx = 3.5E-6; // waist of Gaussian; 3.5um
var wy = 3.5E-6;
//// generate Gaussian field
//var v = new SCField.Gaussian(
//    wavelength: wavelength, 
//    material: new FuncMaterial(nReal: 1.0),
//    waistRadiusX: wx, waistRadiusY: wy, 
//    grid: g);
// generate truncated plane wave
var v = new SCField.PlaneWave(
    wavelength: wavelength,
    material: new FuncMaterial(nReal: 1.0),
    diameterX: 20 * wx, diameterY: 20 * wy,
    grid: g, shape: ApertureShape.Elliptical,
    edge: wx);
    
    
// detect input
var vIn = det.Sample(v: v, quantity: DetectQuantity.Magnitude);
VFrame.CreateShow(values: vIn, title: $"Input E-field [Magnitude]",
    grid: det.GridInfo * (1E3, 1E3), // scaled to [mm] unit
    xLabel: "x [mm]", yLabel: "y [mm]",
    colormap: Options.PlotColormap.Magma);


// propagate by distance d
var d = 675E-6; // 675 um 
FreeSpace.RayleighSommerfeld2D(ref v, distance: d, loopMode: LoopMode.Parallel);


// detect output 
var vOut = det.Sample(v: v, quantity: DetectQuantity.Magnitude);
VFrame.CreateShow(values: vOut, title: $"Propagated E-field [Magnitude]",
    grid: det.GridInfo * (1E3, 1E3), // scaled to [mm] unit 
    xLabel: "x [m]", yLabel: "y [m]",
    colormap: Options.PlotColormap.Magma);