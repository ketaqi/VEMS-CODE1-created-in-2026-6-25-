// load a sampled material
var metal = SampMaterial.Ag_Johnson1972;
VFrame.CreateShow(sv: metal.SampData, plotPart: ComplexPart.RealPart,
    title: "Ag-dispersion", xLabel: "wavelength [um]", yLabel: "n");