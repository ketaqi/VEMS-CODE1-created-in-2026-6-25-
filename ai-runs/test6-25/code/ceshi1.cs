// RCWA 1D Rectangular Grating Diffraction
var wavelength = 193.0E-9;
var polarization = InPlanePolMode.TM;
var nIn = 1.5593;
var nOut = 1.0;
var nRidge = new Complex(1.3282, 1.6637);
var nEmbed = 1.0;
var period = 25.456E-6;
var ridgeWidth = 12.728E-6;
var thickness = 0.1E-6;

// Build grating: piecewise-constant permittivity
VectorD spans = new VectorD(4, 0.0);
spans[0] = -period / 2.0; spans[1] = -ridgeWidth / 2.0;
spans[2] = ridgeWidth / 2.0; spans[3] = period / 2.0;
VectorZ values = new VectorZ(3, 0.0);
values[0] = nEmbed * nEmbed; values[1] = nRidge * nRidge; values[2] = nEmbed * nEmbed;

var solver = new RCWA1Dp(wavelength: wavelength,
    polarization: polarization,
    materialFront: new FuncMaterial(nReal: nIn),
    mediumMiddle: new Layer1DPwctMedium(new Pwct1DCplxData(spans, values)),
    period: period, thickness: thickness,
    materialBehind: new FuncMaterial(nReal: nOut));

var kx = 2.0 * Math.PI / wavelength * nIn * Math.Sin(0.0);

var sw = System.Diagnostics.Stopwatch.StartNew();
solver.ComputeHalfSMatrix(kx0: kx);
sw.Stop();

var pw = new PlaneWaveXZ(wavelength: wavelength, n: nIn, kx: kx, polMode: polarization);
(var t, var gT) = solver.ComputeTCoefficients(pw: pw);
Transform.FFT1D(x: ref t, grid: ref gT, direction: FFTOptions.Direction.Backward);

var tMag = VMath.Abs(t);
VFrame.CreateShow(values: tMag, grid: gT,
    title: $"RCWA |E| Transmitted (TM, {sw.Elapsed.TotalMilliseconds:F0}ms)",
    xLabel: "kx", yLabel: "|E|", plotColor: Options.PlotColor.DarkGreen);

var tArg = VMath.Arg(t);
VFrame.CreateShow(values: tArg, grid: gT,
    title: "RCWA Phase (Argument)",
    xLabel: "kx", yLabel: "Phase [rad]", plotColor: Options.PlotColor.SteelBlue);
