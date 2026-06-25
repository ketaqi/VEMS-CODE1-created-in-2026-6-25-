//Local software path
var path = UserSetting.SampleCodeDir;

//Data source: https://refractiveindex.info

//Import Refractive index data
var nAg = Import.Txt2Scat1DCplxData(filePath: path+@"\Materials\Ag_Ferrera2020.txt",
columnSeparator: ' ',startLine: 9);

var metal_Ag_Ferrera2020 = new SampMaterial(sampData: nAg);

VFrame.CreateShow(sv: metal_Ag_Ferrera2020.SampData, plotPart: ComplexPart.RealPart,
    title: "Ag_Ferrera2020", xLabel: "wavelength [um]", yLabel: "n");
    
//***************************************************************************************   

//Data source: https://henke.lbl.gov/optical_constants 

//Import Refractive index data
var nMo = Import.Txt2Scat1DCplxData(filePath:path+@"\Materials\Mo_CXRO.txt",
//var nMo = Import.Txt2Scat1DCplxData(filePath:path+@"\Materials\Mo_Werner2009.txt",
columnSeparator:' ',startLine:9);

var metal_Mo = new SampMaterial(sampData: nMo);

VFrame.CreateShow(sv: metal_Mo.SampData, plotPart: ComplexPart.RealPart,
    title: "Mo", xLabel: "wavelength [um]", yLabel: "n");
