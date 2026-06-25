// parameters
var wavelength = 193.0E-9;
var incAngle = 0.0; // in degree
var polarization = InPlanePolMode.TM;
var nIn = 1.5593;
var nOut = 1.0;
var nRidge = new Complex (1.3282,1.6637);
var nEmbed = 1.0;
var period = 25.456E-6;
var ridgeWidth = 12.728E-6;
var ridgeCenter = 0.0E-6;
double thickness = 0.1E-6;


// ==== RCWA ====
var matFront = new FuncMaterial(nReal: nIn);

////layermiddle using analytical
VectorD spans = new VectorD(4, 0.0);
spans[0] = -period / 2.0; spans[1] = -ridgeWidth / 2.0;
spans[2] = ridgeWidth / 2.0; spans[3] = period / 2.0;
VectorZ values = new VectorZ(3, 0.0);
values[0] = nEmbed * nEmbed; values[1] = nRidge * nRidge; values[2] = nEmbed * nEmbed;
var grating = new Pwct1DCplxData(spans, values);
var medMiddle = new Layer1DPwctMedium(grating);
var matBehind = new FuncMaterial(nReal: nOut);
var solver = new RCWA1Dp(wavelength: wavelength,
    polarization: polarization,
    materialFront: matFront,
    mediumMiddle: medMiddle,//choose layermiddle using sampling or analytical
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

//Transform.FFT(x: t, grid: g, opt: FFTOption.Backward);、
Transform.FFT1D(x: ref t, grid: ref g, direction: FFTOptions.Direction.Backward);
VFrame.CreateShow(values: t, grid: g, plotPart: ComplexPart.Argument, 
    title: "Output Field Phase Part (x)", yLabel: "E-field", plotColor: Options.PlotColor.SteelBlue); 

VFrame.CreateShow(
    values: t,
    grid: g,
    plotPart: ComplexPart.RealPart,
    title: "Output Field Real Part (x)",
    yLabel: "Re(E(x))",
    plotColor: Options.PlotColor.Green);


// 虚部
VFrame.CreateShow(
    values: t,
    grid: g,
    plotPart: ComplexPart.ImagPart,
    title: "Output Field Imaginary Part (x)",
    yLabel: "Im(E(x))",
    plotColor: Options.PlotColor.Purple);
    
// 能量密度
VFrame.CreateShow(
    values: t*t,
    grid: g,
    plotPart: ComplexPart.Magnitude,
    title: "Output Field Intensity Part (x)",
    yLabel: "Im(E(x))",
    plotColor: Options.PlotColor.Purple);


////layermiddle using sampling
var medMiddleFFT = new Layer1DMedium(n: (w,x) => GratingRIndex(x, ridgeWidth, ridgeCenter, nRidge, nEmbed));

var solverFFT = new RCWA1Dp(wavelength: wavelength,
    polarization: polarization,
    materialFront: matFront,
    mediumMiddle: medMiddleFFT,//choose layermiddle using sampling or analytical
    period: period, thickness: thickness,
    materialBehind: matBehind);
var kxFFT = 2.0 * Math.PI / wavelength * nIn * Math.Sin(Converter.Degree2Radian(incAngle));
solverFFT.ComputeHalfSMatrix(kx0: kxFFT,mediumSampling:10001); 
// input plane wave => output
var pwFFT = new PlaneWaveXZ(wavelength: wavelength, n: nIn, kx: kxFFT, polMode: polarization);
(var tFFT, var gFFT) = solverFFT.ComputeTCoefficients(pw: pwFFT);

Transform.FFT1D(x: ref tFFT, grid: ref gFFT, direction: FFTOptions.Direction.Backward);

VFrame.CreateShow(values: tFFT, grid: gFFT, plotPart: ComplexPart.Argument, 
    title: "Output Field Phase Part FFT (x)", yLabel: "E-field", plotColor: Options.PlotColor.SteelBlue); 

VFrame.CreateShow(
    values: tFFT,
    grid: gFFT,
    plotPart: ComplexPart.RealPart,
    title: "Output Field Real Part FFT (x)",
    yLabel: "Re(E(x))",
    plotColor: Options.PlotColor.Green);


// 虚部
VFrame.CreateShow(
    values: tFFT,
    grid: gFFT,
    plotPart: ComplexPart.ImagPart,
    title: "Output Field Imaginary Part FFT (x) ",
    yLabel: "Im(E(x))",
    plotColor: Options.PlotColor.Purple);
    
// 能量密度
VFrame.CreateShow(
    values: tFFT*tFFT,
    grid: gFFT,
    plotPart: ComplexPart.Magnitude,
    title: "Output Field Intensity Part FFT (x)",
    yLabel: "Im(E(x))",
    plotColor: Options.PlotColor.Purple);

VFrame.CreateShow(values: t - tFFT, grid: gFFT, plotPart: ComplexPart.Argument, 
    title: "Output Field Phase Part Difference (x)", yLabel: "E-field", plotColor: Options.PlotColor.SteelBlue); 

VFrame.CreateShow(
    values: t - tFFT,
    grid: gFFT,
    plotPart: ComplexPart.RealPart,
    title: "Output Field Real Part Difference (x)",
    yLabel: "Re(E(x))",
    plotColor: Options.PlotColor.Green);


// 虚部
VFrame.CreateShow(
    values: t - tFFT,
    grid: gFFT,
    plotPart: ComplexPart.ImagPart,
    title: "Output Field Imaginary Part Difference (x) ",
    yLabel: "Im(E(x))",
    plotColor: Options.PlotColor.Purple);
    
// 能量密度
VFrame.CreateShow(
    values: tFFT*tFFT - t*t,
    grid: gFFT,
    plotPart: ComplexPart.Magnitude,
    title: "Output Field Intensity Part Difference (x)",
    yLabel: "|E(x)|^2)",
    plotColor: Options.PlotColor.Purple);






// ================================
// grating layer R-index definition
private static Complex GratingRIndex(double x, 
    double width, double x0, 
    Complex nInner, Complex nOuter)
{
    double xp = x - x0;
    if (Math.Abs(xp) <= 0.5 * width)
    { return nInner ; }
    else
    { return nOuter ; }
}