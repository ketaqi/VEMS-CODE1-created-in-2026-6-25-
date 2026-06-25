// basic parameters
var wavelength = 1550E-9;
var polarization = InPlanePolMode.TM;
var period = 790.5E-9;
var height = 1317.0E-9;
var embed = new FuncMaterial(nReal: 1.0);
var fill = new FuncMaterial(nReal: 2.4);
var front = new FuncMaterial(nReal: 1.5);
var behind = new FuncMaterial(nReal: 1.0);


// meta-layer information
var workPath = DirectoryHelper.SampleDirectory + @"\Metasurfaces";
var rectDiameters = Import.Txt2VectorD(workPath + @"\a04_initial pillar diameter.txt", '\t');
if(rectDiameters == null){ return; }
var grids = new GridInfo1D(n: rectDiameters.Count, spacing: period);
Printer.Write($"grids: Count = {grids.Count}");
var atoms = new List<MetaAtom1D>();
for(long i = 0; i < rectDiameters.Count; i++)
{ atoms.Add(new MetaAtom1D.Rect(period, height, rectDiameters[i], embed, fill)); }
var metaLayer = new Grid1DMetaLayer(gridPoints: grids, height: height, metaAtoms: atoms); 

// manually set sampling parameters, if needed
var nFields = RCWAHelper.DetermineSampling(wavelength: wavelength, period: metaLayer.GridPoints.Range,
    kSizeFactor: 6.5);
var nMedium = RCWAHelper.DetermineSampling(period: metaLayer.GridPoints.Range, dx: 0.5E-9);
Printer.Write($"nFields = {nFields}; nMedium = {nMedium}");

// solve by RCWA
var solver = metaLayer.CreateSolver(front, behind);
var pIn = new PlaneWaveXZ(wavelength: wavelength, n: front.N(wavelength), kx: 0.0, polMode: polarization);
solver.ComputeHalfSMatrix(wavelength: pIn.Wavelength, mode: pIn.Polarization, kx0: pIn.Kx,
    fieldsSampling: nFields, mediumSampling: nMedium);
var t = solver.ComputeCoefficients(pIn, isTransmission: true);
// converts to plane waves and computes efficiencies
var dKx = 2.0 * Math.PI / metaLayer.GridPoints.Range;
var vEffs = new VectorD(count: t.Count);
var gEffs = new GridInfo1D(n: t.Count, spacing: dKx, start: solver.Kx0 - (t.Count-1)/2 * dKx);
for(long i = 0; i < t.Count; i++)
{
    var pwi = RCWAHelper.CoefficientToPlaneWave(c: t[i], period: metaLayer.GridPoints.Range,
        wavelength: wavelength, epsilon: behind.Epsilon(wavelength), mu: 1.0,
        kx: gEffs[i], polarization: polarization, direction: SignFactor.Positive);
    vEffs[i] = pwi.ComputeSz() / pIn.ComputeSz();
}

// display
VFrame.CreateShow(values: vEffs * 100.0, grid: gEffs, title: "Efficiency", 
    xLabel: "spatial frequency kx", yLabel: "Efficiency [%]");
    