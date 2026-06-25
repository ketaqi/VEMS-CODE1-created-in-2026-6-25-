// defines sampling grid information
var grid = new GridInfo1D(n: 151, spacing: 1.0E-6);
// defines Gaussian waist values
var w = 7.5E-6; // waist radius of Gaussian; 7.5um
var wavelength = 632.8E-9;
//var v = new SCField1D.Gaussian(wavelength: wavelength, 
//    material: new FuncMaterial(nReal: 1.0),
//    waistRadius: w, grid: grid, shiftX: 1.0 * w);
var v = new SCField1D.PlaneWave(wavelength: wavelength, 
    material: new FuncMaterial(nReal: 1.0), 
    diameter: 10.0 * w, edge: 2.0 * w, grid: grid); 
// prepares detector
var det = new Detector1D(grid: grid);

// input display
VFrame.CreateShow(values: det.Sample(v, quantity: DetectQuantity.Magnitude), grid: det.GridInfo,
    title: $"Input E {v.Domain}", xLabel: "coordinates [m]", yLabel: "value [A.U.]");

// propagate by distance d
var d = 675E-6; // 675um
v.Propagate(d: d, targetDomain: ModelingDomain.Spatial); // this method calls SPW internally

// output display
VFrame.CreateShow(values: det.Sample(v, quantity: DetectQuantity.Magnitude), grid: det.GridInfo,
    title: $"Propagated E {v.Domain}", xLabel: "coordinates [m]", yLabel: "value [A.U.]");