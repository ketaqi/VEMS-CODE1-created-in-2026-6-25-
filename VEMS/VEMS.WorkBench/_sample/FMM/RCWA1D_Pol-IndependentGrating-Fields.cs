// parameters
var wavelength = 1.06E-6;
var incAngle = 20.173; // in degree, Littrow configuration
var polarization = InPlanePolMode.TM;
var nIn = 1.45;
var nOut = 1.0;
var nRidge = nIn;
var nEmbed = nOut;
var period = wavelength;
var ridgeWidth = 0.5 * period;
var ridgeCenter = 0.0;
var thickness = 1.85E-6;
// grating definition 
Complex GratingRIndex(double x, double width)
    => Function1D.Rectangle(x, new List<double>{width, 0.0, (nRidge - nEmbed)}) + nEmbed;
// manually set sampling parameters, if needed
var nFields = RCWAHelper.DetermineSampling(wavelength: wavelength, period: period,
    kSizeFactor: 5.0, addNum: 32);
var nMedium = RCWAHelper.DetermineSampling(period: period, dx: 5.0E-9);


// ==== RCWA calculation ====
var matFront = new FuncMaterial(nReal: nIn);
var medMiddle = new Layer1DMedium(n: (w,x) => GratingRIndex(x, ridgeWidth));
var matBehind = new FuncMaterial(nReal: nOut);
var solver = new RCWA1Dp(wavelength: wavelength,
    polarization: polarization,
    materialFront: matFront,
    mediumMiddle: medMiddle,
    period: period, thickness: thickness,
    materialBehind: matBehind);
var kx = 2.0 * Math.PI / wavelength * nIn * Math.Sin(Converter.Degree2Radian(incAngle));
solver.ComputeHalfSMatrix(kx0: kx, fieldsSampling: nFields, mediumSampling: nMedium);
// input plane wave => output
var pwIn = new PlaneWaveXZ(wavelength: wavelength, n: nIn, kx: kx, polMode: polarization);
(var t, var g) = solver.ComputeTCoefficients(pw: pwIn);
// efficiency
var idx = (t.Count - 1)/2 - 1; // index for the -1st order
var pwOut = RCWAHelper.CoefficientToPlaneWave(c: t[idx], period: period,
    wavelength: wavelength, epsilon: matBehind.Epsilon(wavelength), mu: 1.0,
    kx: kx + (-1) * 2.0 * Math.PI / period, polarization: polarization);
var eff = pwOut.ComputeSz() / pwIn.ComputeSz();
Printer.Write($"Diffraction Efficiency [-1st Order] = {eff * 100} [%]");
// ==== end of RCWA ====


// display
if (solver.LayerMiddle != null && solver.LayerMiddle.Medium.CacheSampleData == true)
{
    VFrame.CreateShow(values: solver.LayerMiddle.Medium.SampleData,
        grid: solver.LayerMiddle.Medium.SampGrid,
        title: "Medium", yLabel: $"{solver.LayerMiddle.Medium.SelectedProp}", lineWidth: 1.5, markerSize: 0.0, plotColor: Options.PlotColor.Brown);
}
Transform.FFT(x: t, grid: g, opt: FFTOption.Backward);
var vOutm = new VectorZ(other: t, deepCopy: true);
var gOutm = new GridInfo1D(other: g);
solver.PeriodicReplicate(v: ref vOutm, g: ref gOutm, replication: 3);
VFrame.CreateShow(values: vOutm, grid: gOutm, plotPart: ComplexPart.Magnitude,
    title: "Output Field (x)", yLabel: "E-field", plotColor: Options.PlotColor.SteelBlue); 