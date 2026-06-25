// parameters
var wavelength = 1550E-9;
var polarization = InPlanePolMode.TM;
var period = 790.5E-9;
var height = 1317.0E-9;
var embed = new FuncMaterial(nReal: 1.0);
var fill = new FuncMaterial(nReal: 2.4);
var front = new FuncMaterial(nReal: 1.5);
var behind = new FuncMaterial(nReal: 1.0);
// control
var sw = new Stopwatch();

// constructs meta-layer
var workPath = UserSetting.SampleCodeDir + @"\Metasurfaces";
var rectDiameters = Import.Txt2VectorD(workPath + @"\a04_initial pillar diameter.txt", '\t');
if (rectDiameters == null) { return; }
var grids = new GridInfo1D(n: rectDiameters.Count, spacing: period);
Printer.Write($"number of grids = {grids.Count}");
var atoms = new List<MetaAtom1D>();
for (long i = 0; i < rectDiameters.Count; i++)
{ atoms.Add(new MetaAtom1D.Rect(period, height, rectDiameters[i], embed, fill)); }
var metaLayer = new Grid1DMetaLayer(gridPoints: grids, height: height, metaAtoms: atoms);
// plane wave incidence => output plane waves & efficiencies
var pIn = new PlaneWaveXZ(wavelength: wavelength, n: front.N(wavelength),
    kx: 0.0, polMode: polarization);


// RCWA sampling
var nFields = RCWAHelper.DetermineSampling(wavelength: wavelength, period: period,
    kSizeFactor: 6.5, addNum: 0);
var nMedium = RCWAHelper.DetermineSampling(period: period, dx: 5.0E-9);
Printer.Write($"nFields = {nFields}; nMedium = {nMedium}");


sw.Restart();
// loop over all meta-atoms
var v = new VectorZ(count: metaLayer.GridPoints.Count, initVal: 1.0);
for (int i = 0; i < metaLayer.GridPoints.Count; i++)
{
    //Printer.Write($"i = {i} ...");
    var a = metaLayer.MetaAtoms[i];
    var c = a.ComputeCplxModulation(wavelength, polarization,
        front, behind, true,
        fieldsSampling: nFields,
        mediumSampling: nMedium);
    v[i] *= c;
}
var g = new GridInfo1D(other: metaLayer.GridPoints);
Transform.FFT(x: v, grid: g, opt: FFTOption.Forward);
VFrame.CreateShow(v, g);
// converts to efficiency
var sEffs = new VectorD(count: v.Count);
for (long i = 0; i < sEffs.Count; i++)
{
    var pwi = RCWAHelper.CoefficientToPlaneWave(c: v[i], period: metaLayer.GridPoints.Range,
        wavelength: wavelength, epsilon: behind.Epsilon(wavelength), mu: 1.0,
        kx: g[i], polarization: polarization, direction: SignFactor.Positive);
    sEffs[i] = pwi.ComputeSz() / pIn.ComputeSz();
    if (sEffs[i] >= 0.05) { Printer.Write($"kx = {pwi.Kx}; Efficiency = {sEffs[i] * 100}%"); }
}
VFrame.CreateShow(values: sEffs * 100.0, grid: g, title: $"Efficiencies [Local Solutions]",
    xLabel: "Spatial Frequency Kx [1/m]", yLabel: "Efficiency [%]");


// timer
sw.Stop();
Printer.Write($"local periodic approximation time cost = {sw.Elapsed.TotalSeconds} [s]");