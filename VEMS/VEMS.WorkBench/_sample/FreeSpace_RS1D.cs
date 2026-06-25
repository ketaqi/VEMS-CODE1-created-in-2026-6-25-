// sampling parameters for field and detector
var nx = 151;
var dx = 1.0E-6; 
var g = new GridInfo1D(n: nx, spacing: dx);
var det = new Detector1D(grid: g);


// define input field parameters
var w = 7.5E-6; // waist of Gaussian; 7.5um
var wavelength = 632.8E-9;
//var v = new SCField1D.Gaussian(
//    wavelength: wavelength, 
//    material: new FuncMaterial(nReal: 1.0), 
//    waistRadius: w, 
//    grid: g);
var v = new SCField1D.PlaneWave(
    wavelength: wavelength,
    material: new FuncMaterial(nReal: 1.0),
    diameter: 10.0 * w,
    grid: g, 
    edge: 2.0 * w);
    

// detect input 
var vIn = det.Sample(v: v, quantity: DetectQuantity.Magnitude);
VFrame.CreateShow(values: vIn, title: $"Input E-field [Magnitude]",  
    grid: det.GridInfo * 1E3, // scaled to [mm] unit
    xLabel: "x [mm]", yLabel: "value [A.U.]");


// propagate by distance d
var d = 675E-6; // 675um
FreeSpace.RayleighSommerfeld1D(v: ref v, distance: d, loopMode: LoopMode.Parallel);


// detect output
var vOut = det.Sample(v: v, quantity: DetectQuantity.Magnitude);
VFrame.CreateShow(values: vOut, title: $"Propagated E-field [Magnitude]", 
    grid: det.GridInfo * 1E3, // scaled to [mm] unit
    xLabel: "x [m]", yLabel: "value [A.U.]");