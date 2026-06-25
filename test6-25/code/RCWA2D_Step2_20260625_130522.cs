// RCWA 2D Step 2: S-Matrix + Near-Field |E|
var wl = 633E-9; var periodX = 5.0E-6; var periodY = 5.0E-6;
var pwX = 2.5E-6; var pwY = 2.5E-6; var thickness = 0.5E-6;
var nPillar = 2.0; var nEmbed = 1.0;

var medium = new Layer2DMedium(n: (w, x, y) => {
    double xp = x - periodX * Math.Floor(x / periodX + 0.5);
    double yp = y - periodY * Math.Floor(y / periodY + 0.5);
    return (Math.Abs(xp) <= pwX / 2 && Math.Abs(yp) <= pwY / 2) ? nPillar : nEmbed;
});

var solver = new RCWA2D(wavelength: wl,
    materialFront: new FuncMaterial(nReal: 1.0),
    mediumMiddle: medium, periodX: periodX, periodY: periodY,
    thickness: thickness, materialBehind: new FuncMaterial(nReal: 1.0));

var sw = System.Diagnostics.Stopwatch.StartNew();
solver.ComputeHalfSMatrix(kx0: 0, ky0: 0,
    fieldsSamplingX: 31, fieldsSamplingY: 31,
    mediumSamplingX: 101, mediumSamplingY: 101);
sw.Stop();

// S-Matrix
VFrame.CreateShow(values: VMath.Abs(solver.S11),
    title: $"S11 Transmission ({sw.Elapsed.TotalMilliseconds:F0}ms)",
    colormap: Options.PlotColormap.Jet);
VFrame.CreateShow(values: VMath.Abs(solver.S21),
    title: "S21 Reflection",
    colormap: Options.PlotColormap.Jet);

// Near-field
var pw = new PlaneWave(wavelength: wl, n: 1.0, kx: 0, ky: 0, direction: SignFactor.Positive);
pw.Ex = 1.0;
(var tEx, var tEy, var gridT) = solver.ComputeTCoefficients(pw: pw);

Transform.FFT2D(x: ref tEx, grid: ref gridT, direction: FFTOptions.Direction.Backward);
Transform.FFT2D(x: ref tEy, grid: ref gridT, direction: FFTOptions.Direction.Backward);

var magE = VMath.Sqrt(VMath.Abs(tEx) * VMath.Abs(tEx) + VMath.Abs(tEy) * VMath.Abs(tEy));
VFrame.CreateShow(values: magE, grid: gridT,
    title: "Near-Field |E| at Grating Output",
    colormap: Options.PlotColormap.Jet);

// 1mm Propagation via SCField
var scField = new SCField(wavelength: wl,
    material: new FuncMaterial(nReal: 1.0),
    grid: gridT, fieldValues: magE);

scField.Propagate(d: 1.0E-3, targetDomain: ModelingDomain.Spatial);

var vOut = new Detector2D(grid: gridT).Sample(v: scField, quantity: DetectQuantity.Magnitude);
VFrame.CreateShow(values: vOut, grid: gridT,
    title: "Far-Field |E| after 1mm",
    colormap: Options.PlotColormap.Jet);
