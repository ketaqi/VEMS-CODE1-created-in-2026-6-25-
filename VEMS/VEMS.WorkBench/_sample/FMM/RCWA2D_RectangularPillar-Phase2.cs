// basic parameters
double wavelength = 633E-9;
double incAngle = 0.0; //55.60; // in degree
double nIn = 1.0;
double nOut = 1.52;
// grating parameters
var periodX = 380E-9;
var periodY = 380E-9;
var nEmbed = 1.0;
var nPillar = 2.30;
var pillarDiameter = 200E-9;
var thickness = 545E-9;
// numerical parameters
long N = 23; // ...


// variation of pillar diameter
var diameterStart = 0.0E-9;
var diameterEnd = 380.0E-9;
var diameters = 127;
var di = (diameterEnd - diameterStart) / (diameters - 1);
var cx = new VectorZ(diameters);
// loop
for (long i = 0; i < diameters; i++)
{
    Printer.Write($"... scanning step {i}/{diameters} ...");
    pillarDiameter = diameterStart + i * di;
    // ===== RCWA2D =====
    var rSolver = new RCWA2D(wavelength: wavelength,
        nFront: (w, p) => nIn, pFront: null,
        nMiddle: (w, x, y, p) => GratingRIndex(x, y, p[0], p[1], p[2]),
        pMiddle: new List<double> { pillarDiameter, nEmbed, nPillar },
        periodX: periodX, periodY: periodY,
        thickness: thickness,
        nBehind: (w, p) => nOut, pBehind: null,
        nKxs: N, nKys: N,
        kx0: 0.0, ky0: 0.0,
        layerOverSampX: 12.0, layerOverSampY: 12.0,
        useWSVariation: false,
        saveLayerMediaData: true,
        saveLayerModesData: false);
    rSolver.ComputeHalfSMatrix();
    // ===== end of RCWA2D ======
    // input => output
    // input plane wave
    var kx0 = 2.0 * Math.PI / wavelength * nIn * Math.Sin(Converter.Degree2Radian(incAngle));
    var ky0 = 2.0 * Math.PI / wavelength * nIn * Math.Sin(Converter.Degree2Radian(incAngle));
    var incPW = new PlaneWave(wavelength: wavelength, n: nIn, 
        kx: kx0, ky: ky0,
        direction: SignFactor.Positive);
    incPW.Ex = 1.0;
    var cInX = new MatrixZ(N, N);
    var cInY = new MatrixZ(N, N);
    cInX[(N - 1) / 2, (N - 1) / 2] = incPW.Ex;
    // computes transmission
    (var cOutX, var cOutY) = rSolver.ComputeTCoefficients(cInX, cInY);
    // transmission coefficients at given order
    var orderX = 0;
    var orderY = 0;
    var cOutXi = cOutX[(N - 1) / 2 + orderY, (N - 1) / 2 + orderX];
    var cOutYi = cOutY[(N - 1) / 2 + orderY, (N - 1) / 2 + orderX];
    // transmitted plane wave
    var traPW = new PlaneWave(wavelength: wavelength, n: nOut,
        kx: kx0 + orderX * 2.0 * Math.PI / periodX,
        ky: ky0 + orderY * 2.0 * Math.PI / periodY,
        direction: SignFactor.Positive);
    traPW.Ex = cOutXi;
    traPW.Ey = cOutYi;
    // save value
    cx[i] = traPW.Ex;
}
// takes phase from results
var cxPhase = VMath.UnwrapPhase(cx);
var g = new GridInfo1D(n: diameters, start: diameterStart, spacing: di*1E9);
VFrame.CreateShow(values: cxPhase-cxPhase[0], grid: g, 
    xLabel: "diameter [nm]", yLabel: "phase [rad]", title: "transmission coefficients - phase");


// ================================
// grating layer R-index definition
private static Complex GratingRIndex(double x, double y,
    double pillarDiameter,
    double nEmbed, double nPillar)
{
    double radius = 0.5 * pillarDiameter;
    if (Math.Abs(x) <= radius && Math.Abs(y) <= radius)
        return new Complex(nPillar, 0.0);
    else
        return new Complex(nEmbed, 0.0);
}