// parameters
var wavelength = 1.06E-6;
var incAngle = 20.173; // in degree, Littrow configuration
var polarization = InPlanePolMode.TE;
var nIn = 1.45;
var nOut = 1.0;
var nRidge = nIn;
var nEmbed = nOut;
var period = wavelength;
var ridgeWidth = 0.5 * period;
var ridgeCenter = 0.0;
var thickness = 1.85E-6;
// manually set sampling parameters, if needed
var nFields = RCWAHelper.DetermineSampling(wavelength: wavelength, period: period,
    kSizeFactor: 5.0, addNum: 32);
var nMedium = RCWAHelper.DetermineSampling(period: period, dx: 5.0E-9);


// parameters variation
var depthStart = 0.1E-6;
var depthEnd = 10.0E-6;
var depths = 31;
var depthi = (depthEnd - depthStart) / (depths - 1);
var fillStart = 0.2;
var fillEnd = 0.8;
var fills = 25;
var filli = (fillEnd - fillStart) / (fills - 1);
var effs = new MatrixD(rows: fills, cols: depths);
// loop
for (long iFill = 0; iFill < fills; iFill++)
{
    var f = fillStart + iFill * filli;
    for (long iDepth = 0; iDepth < depths; iDepth++)
    {
        var d = depthStart + iDepth * depthi;
        // update structural parameters
        ridgeWidth = f * period;
        thickness = d;
        // ==== RCWA calculation ====
        var matFront = new FuncMaterial(nReal: (w) => nIn);
        var medMiddle = new Layer1DMedium(n: (w, x) => GratingRIndex(x, ridgeWidth, 0.0, nRidge, nEmbed));
        var matBehind = new FuncMaterial(nReal: (w) => nOut);
        var solver = new RCWA1Dp(wavelength: wavelength,
            polarization: polarization,
            materialFront: matFront,
            mediumMiddle: medMiddle,
            period: period, thickness: thickness,
            materialBehind: matBehind);
        var kx = 2.0 * Math.PI / wavelength * nIn * Math.Sin(Converter.Degree2Radian(incAngle));
        solver.ComputeHalfSMatrix(kx0: kx, fieldsSampling: nFields, mediumSampling: nMedium);
        // input => output
        var pwIn = new PlaneWaveXZ(wavelength: wavelength, n: nIn, kx: kx, polMode: polarization);
        (var t, _) = solver.ComputeTCoefficients(pw: pwIn);
        var idx = (t.Count - 1)/2 - 1; // index for the -1st order
        var pwi = RCWAHelper.CoefficientToPlaneWave(c: t[idx], period: period,
            wavelength: wavelength, epsilon: matBehind.Epsilon(wavelength), mu: 1.0,
            kx: kx + (-1) * 2.0 * Math.PI / period, polarization: polarization);
        // ==== end of RCWA ====
        // calculates the -1 order efficiency
        effs[iFill, iDepth] = pwi.ComputeSz() / pwIn.ComputeSz(); 
    }
}
// display
var g = new GridInfo2D(rows: fills, cols: depths,
    startY: fillStart * 100.0, startX: depthStart * 1E6, // changes units
    spacingY: filli * 100.0, spacingX: depthi * 1E6); // changes units
var f = VFrame.CreateFrame();
VFrame.AddToFrame(f, values: effs, grid: g);
VFrame.SetFrameParameters(f, xLabel: "grating depth [um]", yLabel: "fill factor [%]", title: "Efficiencies");
VFrame.LockAxieScale(f, false);
VFrame.RefreshShow(f);


// ================================
// grating layer R-index definition
private static Complex GratingRIndex(double x, 
    double width, double x0, 
    double nInner, double nOuter)
{
    double xp = x - x0;
    if (Math.Abs(xp) <= 0.5 * width)
    { return new Complex(nInner, 0.0); }
    else
    { return new Complex(nOuter, 0.0); }
}