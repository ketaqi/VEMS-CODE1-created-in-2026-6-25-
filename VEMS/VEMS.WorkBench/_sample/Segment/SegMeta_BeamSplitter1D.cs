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
var useSegmentation = true;
var sw = new Stopwatch();

// constructs meta-layer
var workPath = DirectoryHelper.SampleDirectory + @"\Metasurfaces";
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


if (useSegmentation)
{
    sw.Restart();
    // ===== localized solution =====
    var ns = 255;
    var centers = new GridInfo1D(n: ns, spacing: metaLayer.GridPoints.Range / ns);
    var segGridRange = 31 * period;
    var segGridSamples = 251;
    var segGrid = new GridInfo1D(n: segGridSamples, spacing: segGridRange / segGridSamples);
    var uField = new Segment1D.CosRect(x0: 0.0, width: centers.Spacing, edge: 0.25 * centers.Spacing);
    var uLayer = new Segment1D.CosRect(x0: 0.0, width: segGridRange, edge: 0.0);
    var sMeta = new SegMeta1D(centers: centers, unitField: uField, unitLayer: uLayer);
    sMeta.LocalGrid = new GridInfo1D(other: segGrid);
    // workflow ...
    // 1) local solvers and local s-matrices
    sMeta.CreateLocalSolvers(front, metaLayer, behind);
    var nLocalF = RCWAHelper.DetermineSampling(wavelength: wavelength, period: sMeta.UnitLayer.Width, kSizeFactor: 6.5);
    var nLocalM = RCWAHelper.DetermineSampling(period: sMeta.UnitLayer.Width, dx: 1.0E-9);
    sMeta.ComputeHalfSMatrices(wavelength: wavelength,
        mode: InPlanePolMode.TM,
        kx0: 0.0,
        fieldsSampling: nLocalF,
        mediumSampling: nLocalM,
        logInfo: false);
    // 2) defines local input fields and computes local coefficients
    var vIn = sMeta.GenerateLocalFields(pIn: pIn); // sMeta.GenerateLocalFields(field: (w) => Complex.One); // plane wave incidence
    var c = sMeta.ComputeLocalCoefficients(vIn: vIn, isTransmission: true, 
        interp: InterpolationMethod.Linear);
    // 3) computes summed field
    (var vSum, var gSum) = sMeta.ComputeSummedField(c, kRange: 6.5 * 2.0 * Math.PI / wavelength,
        itpl: InterpolationMethod.Linear);
    // converts to efficiency
    var sEffs = new VectorD(count: gSum.Count);
    for (long i = 0; i < sEffs.Count; i++)
    {
        var pwi = RCWAHelper.CoefficientToPlaneWave(c: vSum[i], period: metaLayer.GridPoints.Range,
            wavelength: wavelength, epsilon: behind.Epsilon(wavelength), mu: 1.0,
            kx: gSum[i], polarization: polarization, direction: SignFactor.Positive);
        sEffs[i] = pwi.ComputeSz() / pIn.ComputeSz();
        if (sEffs[i] >= 0.05) { Printer.Write($"kx = {pwi.Kx}; Efficiency = {sEffs[i] * 100}%"); }
    }
    VFrame.CreateShow(values: sEffs * 100.0, grid: gSum, title: $"Efficiencies [Local Solutions]",
        xLabel: "Spatial Frequency Kx [1/m]", yLabel: "Efficiency [%]");
    // ===== end of localized solution =====
    sw.Stop();
    Printer.Write($"segmented solution time cost = {sw.Elapsed.TotalSeconds} [s]");
}
else
{
    sw.Restart();
    // ==== global solution ====
    var nFields = RCWAHelper.DetermineSampling(wavelength: wavelength, period: metaLayer.GridPoints.Range,
        kSizeFactor: 6.5);
    var nMedium = RCWAHelper.DetermineSampling(period: metaLayer.GridPoints.Range, dx: 1.0E-9);
    // checks information ...
    Printer.Write($"nFields = {nFields}; nMedium = {nMedium}");
    //VFrame.CreateShow(values: mIn, grid: gIn, title: "Structure Medium (N)");
    var solver = metaLayer.CreateSolver(front, behind);
    solver.ComputeHalfSMatrix(wavelength: wavelength, mode: polarization, kx0: 0.0,
        fieldsSampling: nFields, mediumSampling: nMedium);
    var t = solver.ComputeCoefficients(pIn, isTransmission: true);
    var dKx = 2.0 * Math.PI / metaLayer.GridPoints.Range;
    var vEffs = new VectorD(t.Count);
    var gEffs = new GridInfo1D(n: t.Count, spacing: dKx, start: solver.Kx0 - (t.Count-1)/2 * dKx);
    for (int i = 0; i < vEffs.Count; i++)
    {
        var pwi = RCWAHelper.CoefficientToPlaneWave(c: t[i], period: metaLayer.GridPoints.Range,
            wavelength: wavelength, epsilon: behind.Epsilon(wavelength), mu: 1.0,
            kx: gEffs[i], polarization: polarization, direction: SignFactor.Positive);
        vEffs[i] = pwi.ComputeSz() / pIn.ComputeSz();
        if (vEffs[i] >= 0.05) { Printer.Write($"kx = {pwi.Kx}; Efficiency = {vEffs[i] * 100}%"); }
    }
    VFrame.CreateShow(values: vEffs * 100.0, grid: gEffs, title: "Efficiencies [Global Solution]", xLabel: "spatial frequency kx", yLabel: "efficiency [%]");
    // ===== end of global solver =====
    sw.Stop();
    Printer.Write($"global solution time cost = {sw.Elapsed.TotalSeconds} [s]");
}
