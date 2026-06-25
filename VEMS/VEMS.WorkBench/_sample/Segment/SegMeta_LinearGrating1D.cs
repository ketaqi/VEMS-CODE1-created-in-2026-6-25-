// parameters
var wavelength = 1550E-9;
var polarization = InPlanePolMode.TM;
var period = 790.5E-9;
var height = 1317.0E-9;
var embed = new FuncMaterial(nReal: 1.0);
var fill = new FuncMaterial(nReal: 2.4);
var front = new FuncMaterial(nReal: 1.5);
var behind = new FuncMaterial(nReal: 1.0);

// constructs meta-layer
var unit = 5;
var grids = new GridInfo1D(n: 21 * unit, spacing: period);
var atoms = new List<MetaAtom1D>();
for (int i = 0; i < grids.Count / unit; i++)
{
    atoms.Add(new MetaAtom1D.Rect(period, height, 0.10 * period, embed, fill));
    atoms.Add(new MetaAtom1D.Rect(period, height, 0.30 * period, embed, fill));
    atoms.Add(new MetaAtom1D.Rect(period, height, 0.50 * period, embed, fill));
    atoms.Add(new MetaAtom1D.Rect(period, height, 0.70 * period, embed, fill));
    atoms.Add(new MetaAtom1D.Rect(period, height, 0.90 * period, embed, fill));
}
var metaLayer = new Grid1DMetaLayer(gridPoints: grids, height: height, metaAtoms: atoms,
    periodicBoundary: true);


// ==== complete solution ====
var nFields = RCWAHelper.DetermineSampling(wavelength: wavelength, period: metaLayer.GridPoints.Range,
    kSizeFactor: 15.0);
var nMedium = RCWAHelper.DetermineSampling(period: metaLayer.GridPoints.Range, dx: 1.0E-9);
// checks information ...
Printer.WriteLine($"nFields = {nFields}; nMedium = {nMedium}");
//VFrame.CreateShow(values: mIn, grid: gIn, title: "Structure Medium (N)");
var solver = metaLayer.CreateSolver(front, behind);
solver.ComputeHalfSMatrix(wavelength: wavelength, mode: polarization, kx0: 0.0,
    fieldsSampling: nFields, mediumSampling: nMedium);
// plane wave incidence => output plane waves & efficiencies
var pIn = new PlaneWaveXZ(wavelength: wavelength, n: front.N(wavelength), 
    kx: solver.Kx0, polMode: polarization);
var t = solver.ComputeCoefficients(pIn, isTransmission: true);
var vEffs = new VectorD(count: t.Count);
var dKx = 2.0 * Math.PI / metaLayer.GridPoints.Range;
var gEffs = new GridInfo1D(n: t.Count, spacing: dKx, 
    start: solver.Kx0 - (t.Count - 1) / 2 * dKx);
for(int i = 0; i < vEffs.Count; i++)
{
    var pwi = RCWAHelper.CoefficientToPlaneWave(c: t[i], period: metaLayer.GridPoints.Range,
        wavelength: wavelength, epsilon: behind.Epsilon(wavelength), mu: 1.0,
        kx: gEffs[i], polarization: polarization, direction: SignFactor.Positive);
    vEffs[i] = pwi.ComputeSz() / pIn.ComputeSz();
    if(vEffs[i] >= 0.01){ Printer.Write($"kx = {pwi.Kx}; Efficiency = {vEffs[i]*100}%"); }
}
VFrame.CreateShow(values: vEffs * 100.0, grid: gEffs, title: "Efficiencies [Global Solution]", xLabel: "Order", yLabel: "Efficiency [%]");
// ===== end of complete solver =====


// ===== localized solution =====
var ns = 25;
var centers = new GridInfo1D(n: ns, spacing: metaLayer.GridPoints.Range / ns);
var segGridRange = 19 * period;
var segGridSamples = 251;
var segGrid = new GridInfo1D(n: segGridSamples, spacing: segGridRange / segGridSamples);
var uField = new Segment1D.CosRect(x0: 0.0, width: centers.Spacing, edge: 0.25 * centers.Spacing);
var uLayer = new Segment1D.CosRect(x0: 0.0, width: segGridRange, edge: 0.0);
var sMeta = new SegMeta1D(centers: centers, unitField: uField, unitLayer: uLayer);
sMeta.LocalGrid = new GridInfo1D(other: segGrid);
// workflow ...
// 1) local solvers and local s-matrices
sMeta.CreateLocalSolvers(front, metaLayer, behind);
var nLocalF = RCWAHelper.DetermineSampling(wavelength: wavelength, period: sMeta.UnitLayer.Width, kSizeFactor: 15.0);
var nLocalM = RCWAHelper.DetermineSampling(period: sMeta.UnitLayer.Width, dx: 1.0E-9);
sMeta.ComputeHalfSMatrices(wavelength: wavelength, 
    mode: InPlanePolMode.TM, 
    kx0: 0.0,
    fieldsSampling: nLocalF,
    mediumSampling: nLocalM,
    logInfo: true);
// 2) defines local input fields and computes local coefficients
var vIn = sMeta.GenerateLocalFields(pIn: pIn);
var c = sMeta.ComputeLocalCoefficients(vIn: vIn, isTransmission: true, 
    interp: InterpolationMethod.Linear);
// 3) computes summed field
(var vSum, var gSum) = sMeta.ComputeSummedField(c, kRange: 30 * 2.0 * Math.PI / wavelength,
    itpl: InterpolationMethod.Linear);
// converts to efficiency
var sEffs = new VectorD(count: gSum.Count);
for(long i = 0; i < sEffs.Count; i++)
{
    var pwi = RCWAHelper.CoefficientToPlaneWave(c: vSum[i], period: metaLayer.GridPoints.Range,
        wavelength: wavelength, epsilon: behind.Epsilon(wavelength), mu: 1.0,
        kx: gSum[i], polarization: polarization, direction: SignFactor.Positive);
    sEffs[i] = pwi.ComputeSz() / pIn.ComputeSz();
    if(sEffs[i] >= 0.01){ Printer.WriteLine($"kx = {pwi.Kx}; Efficiency = {sEffs[i]*100}%"); }
}
VFrame.CreateShow(values: sEffs * 100.0, grid: gSum, title: $"Efficiencies [Local Solutions]",
    xLabel: "Spatial Frequency Kx [1/m]", yLabel: "Efficiency [%]");
// ===== end of localized solution =====