// basic parameters
var wavelength = 633E-9;
var incAngle = 0.0; //55.60; // in degree
var nIn = 1.0;
var nOut = 1.52;
// grating parameters
var periodX = 380E-9;
var periodY = 380E-9;
var nEmbed = 1.0;
var nPillar = 2.30;
var pillarDiameter = 200E-9;
var thickness = 545E-9;
// numerical parameters
long nx = 13;
long ny = 13;


// ===== RCWA2D =====
var matFront = new FuncMaterial(nReal: nIn);
var medMiddle = new Layer2DMedium(n: (w,x,y) => GratingRIndex(x, y, pillarDiameter, nEmbed, nPillar));
var matBehind = new FuncMaterial(nReal: nOut);
var rSolver = new RCWA2D(wavelength: wavelength,
    materialFront: matFront,
    mediumMiddle: medMiddle,
    periodX: periodX, periodY: periodY, thickness: thickness,
    materialBehind: matBehind);
rSolver.ComputeHalfSMatrix(fieldsSamplingX: nx, fieldsSamplingY: ny, 
    mediumSamplingX: 12 * nx - 1, mediumSamplingY: 12 * ny - 1);
// s-matrix grid
var sGrid = new GridInfo2D(rows: 2 * nx * ny, cols: 2 * nx * ny, 
    spacingY: 2.0 * Math.PI / periodY, spacingX: 2.0 * Math.PI / periodX);
// ===== end of RCWA2D ======


// defines input plane wave
var pw = new PlaneWave(wavelength: wavelength, n: nIn, kx: 0.0, ky: 0.0);
pw.Ex = 1.0; 
pw.Ey = 0.0;
(var tEx, var tEy, var g) = rSolver.ComputeTCoefficients(pw);
// transmitted field in spatial domain
//Transform.FFT(x: tEx, grid: g, opt: FFTOption.Backward);
//VFrame.CreateShow(values: tEx, grid: g, title: "Transmitted Field Ex(x, y)");
// defines diffraction order indices
var dIx = 0;
var dIy = 0;
var dKx = 2.0 * Math.PI / periodX;
var dKy = 2.0 * Math.PI / periodY;
var idx = (g.Cols - 1)/2 + dIx; // index x
var idy = (g.Rows - 1)/2 + dIy; // index y
var pwi = RCWAHelper.CoefficientToPlaneWave(tEx[idy, idx], tEy[idy, idx], periodX, periodY,
    wavelength: wavelength, epsilon: matBehind.Epsilon(wavelength), mu: 1.0,
    kx: pw.Kx + dIx * dKx, ky: pw.Ky + dIy * dKy, direction: SignFactor.Positive);
Printer.Write($"pwi.Ex = {pwi.Ex}");

var eff = pwi.ComputeSz() / pw.ComputeSz();
Printer.Write($"Eff: {eff * 100}%");

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