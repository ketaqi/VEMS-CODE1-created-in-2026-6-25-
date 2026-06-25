//physical parameters
var wavelength = 13.5E-9;
var incAngle = 0.0;  // in degree

// sampling parameters
var N = 151;
var length = 1E-6;
long nx = 21;
long ny = 21;
var grid = new GridInfo2D(N, N, spacingX: length/N, spacingY: length/N);


//define the mask
Complex nStructre = 1.0;
Complex nBasis = 1.01 + Complex.ImaginaryOne * 2.909;
FuncMaterial structre = new FuncMaterial(nReal: nStructre.Real, nImag: nStructre.Imaginary);
FuncMaterial basis = new FuncMaterial(nBasis.Real, nImag: nBasis.Imaginary);

double[] xs = { -300, -400, -400, 400, 400, -300};
VectorD Xs = VMath.ConvertArrayToVector(xs, loopMode: LoopMode.Parallel) * 1E-9;


double[] ys = { -400, -400, 400, 400, 300, 300};
VectorD Ys = VMath.ConvertArrayToVector(ys, loopMode: LoopMode.Parallel) * 1E-9;

List<Geometry2D.Polygon> polygons = new List<Geometry2D.Polygon> {new Geometry2D.Polygon(6, Xs, Ys) };

VFrame.CreateShow(polygons[0].SampleOnGrid(grid));

Photomask mask = new Photomask(polygons, structre, basis);


// ===== RCWA2D =====
double periodX = 1E-6;
double periodY = 1E-6;
var nIn = 1.0;
var matFront = new FuncMaterial(nReal: nIn);
var nOut = 1.0;
var matBehind = new FuncMaterial(nReal: nOut);
var thickness = 50E-9;  
var rSolver = new RCWA2D(wavelength: wavelength, 
    materialFront: matFront,
    mediumMiddle: mask,
    periodX: periodX, periodY: periodY, thickness: thickness,
    materialBehind: matBehind);

var kx = 2.0 * Math.PI / wavelength * nIn * Math.Sin(Converter.Degree2Radian(incAngle));
rSolver.ComputeHalfSMatrix(kx0: kx, fieldsSamplingX: nx, fieldsSamplingY: ny, 
    mediumSamplingX: 12 * nx - 1, mediumSamplingY: 12 * ny - 1);
// ===== end of RCWA2D ======

// defines input plane wave
var pw = new PlaneWave(wavelength: wavelength, n: nIn, kx: 0.0, ky: 0.0);
pw.Ex = 1.0; 
pw.Ey = 0.0;
(var tEx, var tEy, var g) = rSolver.ComputeTCoefficients(pw);
// transmitted field in spatial domain
Transform.FFT2D(x: ref tEx, grid: ref g, option: FTOption.Backward);
//// periodic replication
//rSolver.PeriodicReplicate(ref tEx, ref g, replicationY: 3, replicationX: 3);
VFrame.CreateShow(values: tEx, grid: g, title: "Transmitted Field Ex(x, y)");




