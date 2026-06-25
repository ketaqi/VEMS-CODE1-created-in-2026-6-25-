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
var pillarDiameters = new double[] { 118E-9, 179E-9, 201E-9, 247E-9, 293E-9 };
var thickness = 545E-9;
// numerical parameters
long nx = 41;
long ny = 11;


// ===== RCWA2D =====
var matFront = new FuncMaterial(nReal: nIn);
var medMiddle = new Layer2DMedium(n: (w, x, y) => GratingRIndex(x, y, 
    pillarDiameters, periodX, periodY, nEmbed, nPillar));
var matBehind = new FuncMaterial(nReal: nOut);
var rSolver = new RCWA2D(wavelength: wavelength,
    materialFront: matFront,
    mediumMiddle: medMiddle,
    periodX: pillarDiameters.Length * periodX, periodY: periodY, thickness: thickness,
    materialBehind: matBehind);
rSolver.ComputeHalfSMatrix(fieldsSamplingX: nx, fieldsSamplingY: ny,
    mediumSamplingX: 1901, mediumSamplingY: 381);
    //mediumSamplingX: 56 * nx - 1, mediumSamplingY: 56 * ny - 1);
// epsilon display
//var gEps = new GridInfo2D(31, 151, periodY/31, 5*periodX/151);
//var eps = rSolver.LayerMiddle.Medium.Sample(wavelength, gEps);
//VFrame.CreateShow(eps, gEps);
// s-matrix grid
//var sGrid = new GridInfo2D(rows: 2 * nx * ny, cols: 2 * nx * ny,
//    spacingY: 2.0 * Math.PI / periodY, spacingX: 2.0 * Math.PI / (pillarDiameters.Length * periodX));
// ===== end of RCWA2D ======



// defines input plane wave, checks TE and TM respectively
var pw = new PlaneWave(wavelength: wavelength, n: nIn, kx: 0.0, ky: 0.0);
pw.Ex = 1.0; // TM
pw.Ey = 0.0; // TE
// computes transmission coefficients
(var tEx, var tEy, var g) = rSolver.ComputeTCoefficients(pw);
// transmitted field in spatial domain
//Transform.FFT(x: tEy, grid: g, opt: FFTOption.Backward);
//rSolver.PeriodicReplicate(ref tEy, ref g, replicationY: 3, replicationX: 3);
//VFrame.CreateShow(values: tEy, grid: g, title: "Transmitted Field Ex(x, y)");
// defines diffraction order indices
var dIx = +1;
var dIy = 0;
var dKx = 2.0 * Math.PI / (pillarDiameters.Length * periodX);
var dKy = 2.0 * Math.PI / periodY;
var idx = (g.Cols - 1) / 2 + dIx; // index x
var idy = (g.Rows - 1) / 2 + dIy; // index y
var pwi = RCWAHelper.CoefficientToPlaneWave(tEx[idy, idx], tEy[idy, idx],
    pillarDiameters.Length * periodX, periodY,
    wavelength: wavelength, epsilon: matBehind.Epsilon(wavelength), mu: 1.0,
    kx: pw.Kx + dIx * dKx, ky: pw.Ky + dIy * dKy, direction: SignFactor.Positive);

var eff = pwi.ComputeSz() / pw.ComputeSz();
Printer.Write($"Eff: {eff * 100}% with input Ex = {pw.Ex} Ey = {pw.Ey}");

// ================================
// grating layer R-index definition
private static Complex GratingRIndex(double x, double y,
    double[] pillarDiameters, double px, double py,
    double nEmbed, double nPillar)
{
    GridInfo1D g = new GridInfo1D(pillarDiameters.Length, px);
    // finds the grid span (and the meta-atom) for given position 
    long i = g.FindGridSpan(ref x, true);
    if (i == -1) { return Complex.One; } // !!! assign vacuum
    else
    {
        // calculates local distance
        double xp = x - g[i];
        if(Math.Abs(xp) <= 0.5 * pillarDiameters[i] 
            && Math.Abs(y) <= 0.5 * pillarDiameters[i])
        { return new Complex(nPillar, 0.0); }
        else
        { return new Complex(nEmbed, 0.0); }
    }

}