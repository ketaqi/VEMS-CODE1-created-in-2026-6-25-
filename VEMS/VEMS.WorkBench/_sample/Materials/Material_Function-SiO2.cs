// using a pre-defined material
var mat = FuncMaterial.Sellmeier.SiO2_Malitson1965;
var n = 51;
var dw = (mat.WavelengthMax - mat.WavelengthMin) / (n - 1);
var w = new GridInfo1D(n: n, start: mat.WavelengthMin, spacing: dw);
VFrame.CreateShow(values: mat.Sample(grid: w, matProperty: MaterialProperty.N), 
    grid: w, plotPart: ComplexPart.RealPart,
    title: "SiO2-Maltson1965", xLabel: "wavelength [um]", yLabel: "n");

// define a new material using Sellmeier equation with 3 terms
var SiO2 = new FuncMaterial.Sellmeier(b1: 0.6961663, c1: 0.0684043,
    b2: 0.4079426, c2: 0.1162414, b3: 0.8974794, c3: 9.896161,
    wavelengthMin: 0.21, wavelengthMax: 6.7);
VFrame.CreateShow(values: SiO2.Sample(grid: w, matProperty: MaterialProperty.N), 
    grid: w, plotPart: ComplexPart.RealPart,
    title: "SiO2-dispersion", xLabel: "wavelength [um]", yLabel: "n");