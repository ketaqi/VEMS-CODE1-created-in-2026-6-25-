// parameters
var wavelength = 532.0E-9;
var incAngle = 0.0; // in degree
var polarization = InPlanePolMode.TM;
var nIn = 1.0;
var nOut = 1.0;
var nRidge = 1.5;
var nEmbed = 1.0;
var period = 5.0E-6;
var ridgeWidth = 1.0E-6;
var ridgeCenter = -0.5E-6;
double thickness = 1.0E-6;
// grating definition 
Complex GratingRIndex(double x, double width, double x0, double nInner, double nOuter)
    => Function1D.Rectangle(x, new List<double>{width, x0, (nInner - nOuter)}) + nOuter;


// ==== RCWA ====
var matFront = new FuncMaterial(nReal: nIn);
var medMiddle = new Layer1DMedium(n: (w,x) => GratingRIndex(x, ridgeWidth, ridgeCenter, nRidge, nEmbed));
var matBehind = new FuncMaterial(nReal: nOut);
var solver = new RCWA1Dp(wavelength: wavelength,
    polarization: polarization,
    materialFront: matFront,
    mediumMiddle: medMiddle,
    period: period, thickness: thickness,
    materialBehind: matBehind);
var kx = 2.0 * Math.PI / wavelength * nIn * Math.Sin(Converter.Degree2Radian(incAngle));
solver.ComputeHalfSMatrix(kx0: kx); 
// input plane wave => output
var pw = new PlaneWaveXZ(wavelength: wavelength, n: nIn, kx: kx, polMode: polarization);
(var t, var g) = solver.ComputeTCoefficients(pw: pw);
// ==== end of RCWA ====


// display
if (solver.LayerMiddle != null && solver.LayerMiddle.Medium.CacheSampleData == true)
{
    VFrame.CreateShow(values: solver.LayerMiddle.Medium.SampleData,
        grid: solver.LayerMiddle.Medium.SampGrid,
        title: "Medium", yLabel: $"{solver.LayerMiddle.Medium.SelectedProp}", lineWidth: 1.5, markerSize: 0.0, plotColor: Options.PlotColor.Brown);
}
Transform.FFT(x: t, grid: g, opt: FFTOption.Backward);
VFrame.CreateShow(values: t, grid: g, plotPart: ComplexPart.Argument, 
    title: "Output Field (x)", yLabel: "E-field", plotColor: Options.PlotColor.SteelBlue); 