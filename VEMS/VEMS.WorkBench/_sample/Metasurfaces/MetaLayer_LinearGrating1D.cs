// basic parameters
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
var grids = new GridInfo1D(n: 3 * unit, spacing: period);
var atoms = new List<MetaAtom1D>();
for (int i = 0; i < grids.Count/unit; i++)
{ 
    atoms.Add(new MetaAtom1D.Rect(period, height, 0.10 * period, embed, fill));
    atoms.Add(new MetaAtom1D.Rect(period, height, 0.30 * period, embed, fill));
    atoms.Add(new MetaAtom1D.Rect(period, height, 0.50 * period, embed, fill));
    atoms.Add(new MetaAtom1D.Rect(period, height, 0.70 * period, embed, fill));
    atoms.Add(new MetaAtom1D.Rect(period, height, 0.90 * period, embed, fill));
}
var metaLayer = new Grid1DMetaLayer(gridPoints: grids, 
    height: height, 
    metaAtoms: atoms,
    periodicBoundary: true);


// manually set sampling parameters, if needed
var nFields = RCWAHelper.DetermineSampling(wavelength: wavelength, 
    period: metaLayer.GridPoints.Range,
    kSizeFactor: 15.0);
var nMedium = RCWAHelper.DetermineSampling(
    period: metaLayer.GridPoints.Range, 
    dx: 1.0E-9);
// checks information ...
Printer.WriteLine($"nFields = {nFields}; nMedium = {nMedium}");
var gx = new GridInfo1D(nMedium, metaLayer.GridPoints.Range/nMedium);
var mx = metaLayer.Sample(wavelength: wavelength,
    grid: gx, matProperty: MaterialProperty.N, 
    loopMode: LoopMode.Parallel);
VFrame.CreateShow(values: mx, title: $"Refractive Index Distribution",
    grid: gx * 1E6, xLabel: $"x [um]");


// solve by RCWA
var solver = metaLayer.CreateSolver(front, behind);
solver.ComputeHalfSMatrix(wavelength: wavelength, mode: InPlanePolMode.TM, kx0: 0.0,
    fieldsSampling: nFields, mediumSampling: nMedium);
// plane wave incidence => output plane waves & efficiencies
var pIn = new PlaneWaveXZ(wavelength: wavelength, n: front.N(wavelength), 
    kx: solver.Kx0, polMode: solver.Polarization);
var t = solver.ComputeCoefficients(pIn, isTransmission: true);
var vEffs = new VectorD(count: t.Count);
var dKx = 2.0 * Math.PI / metaLayer.GridPoints.Range;
var gEffs = new GridInfo1D(n: t.Count, spacing: dKx, 
    refPoint: solver.Kx0 - (t.Count - 1) / 2 * dKx,
    refType: GridRefType.Start);
for(long i = 0; i < t.Count; i++)
{
    var pwi = RCWAHelper.CoefficientToPlaneWave(c: t[i], period: metaLayer.GridPoints.Range,
        wavelength: wavelength, epsilon: behind.Epsilon(wavelength), mu: 1.0,
        kx: gEffs[i], polarization: polarization, direction: SignFactor.Positive);
    vEffs[i] = pwi.ComputeSz() / pIn.ComputeSz();
}


// results display
VFrame.CreateShow(values: vEffs * 100.0, title: $"Efficiency",
    grid: gEffs, 
    xLabel: "Spatial Frequency Kx", 
    yLabel: "Efficiency [%]");
