// RCWA 1D - 5 Wavelength Grating Analysis (WorkBench VFrame)
// Timestamp: 20260625_124035

string ts = "20260625_124035";
string outImg = @"c:\Users\Weilun Xu\Desktop\VSCODE test\ai-runs\test6-25\output\images";

var pol = InPlanePolMode.TM;
var nIn = 1.5593; var nOut = 1.0;
var nRidge = new Complex(1.3282, 1.6637); var nEmbed = 1.0;
var period = 25.456E-6; var ridgeWidth = 12.728E-6; var thick = 0.1E-6;

double[] wavelengths_nm = { 193.0, 248.0, 365.0, 436.0, 532.0 };

foreach (var wl_nm in wavelengths_nm)
{
    var wl = wl_nm * 1E-9;
    var label = $"RCWA1D_{wl_nm:F0}nm_{ts}";

    // Build grating & solve
    VectorD spans = new VectorD(4, 0.0);
    spans[0] = -period/2.0; spans[1] = -ridgeWidth/2.0;
    spans[2] = ridgeWidth/2.0; spans[3] = period/2.0;
    VectorZ vals = new VectorZ(3, 0.0);
    vals[0] = nEmbed*nEmbed; vals[1] = nRidge*nRidge; vals[2] = nEmbed*nEmbed;

    var solver = new RCWA1Dp(wavelength: wl, polarization: pol,
        materialFront: new FuncMaterial(nReal: nIn),
        mediumMiddle: new Layer1DPwctMedium(new Pwct1DCplxData(spans, vals)),
        period: period, thickness: thick,
        materialBehind: new FuncMaterial(nReal: nOut));

    var kx = 2.0 * Math.PI / wl * nIn * Math.Sin(0.0);
    solver.ComputeHalfSMatrix(kx0: kx);

    var pw = new PlaneWaveXZ(wavelength: wl, n: nIn, kx: kx, polMode: pol);
    (var t, var gT) = solver.ComputeTCoefficients(pw: pw);
    Transform.FFT1D(x: ref t, grid: ref gT, direction: FFTOptions.Direction.Backward);

    var tMag = VMath.Abs(t);
    var tArg = VMath.Arg(t);

    // --- Save |E| image via VFrame ---
    var fMag = VFrame.CreateFrame();
    VFrame.AddToFrame(fMag, tMag);
    VFrame.SetTitle(fMag, $"RCWA 1D |E| (TM, {wl_nm:F0}nm)");
    VFrame.SetLabelX(fMag, "kx");
    VFrame.SetLabelY(fMag, "|E|");
    fMag.Show();
    System.Threading.Thread.Sleep(500);
    VFrame.SaveFig(fMag, System.IO.Path.Combine(outImg, $"{label}_mag.png"), width: 800, height: 500);
    fMag.Close();

    // --- Save Phase image via VFrame ---
    var fArg = VFrame.CreateFrame();
    VFrame.AddToFrame(fArg, tArg);
    VFrame.SetTitle(fArg, $"RCWA 1D Phase (TM, {wl_nm:F0}nm)");
    VFrame.SetLabelX(fArg, "kx");
    VFrame.SetLabelY(fArg, "Phase [rad]");
    fArg.Show();
    System.Threading.Thread.Sleep(500);
    VFrame.SaveFig(fArg, System.IO.Path.Combine(outImg, $"{label}_phase.png"), width: 800, height: 500);
    fArg.Close();
}

// Show final verification plot
VFrame.CreateShow(values: null, title: "All 5 wavelengths saved!");
