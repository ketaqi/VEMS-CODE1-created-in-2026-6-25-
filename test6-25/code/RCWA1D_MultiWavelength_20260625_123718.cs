using System.Drawing;
using System.Drawing.Drawing2D;
using VEMS.MathCore;
using VEMS.EMSolver;
using GridInfo1D = VEMS.MathCore.GridInfo1D;
using VMath = VEMS.MathCore.VMath;
using Real = System.Double;

string ts = DateTime.Now.ToString("yyyyMMdd_HHmmss");
string outDir = @"c:\Users\Weilun Xu\Desktop\VSCODE test\ai-runs\test6-25\output";
string imgDir = Path.Combine(outDir, "images");
string dataDir = Path.Combine(outDir, "data");
Directory.CreateDirectory(imgDir);
Directory.CreateDirectory(dataDir);

// Fixed parameters (non-wavelength)
var pol = InPlanePolMode.TM;
var nIn = 1.5593;
var nOut = 1.0;
var nRidge = new System.Numerics.Complex(1.3282, 1.6637);
var nEmbed = 1.0;
var period = 25.456E-6;
var ridgeWidth = 12.728E-6;
var thick = 0.1E-6;

// 5 wavelengths to test (nm): DUV, KrF, i-line, g-line, visible green
double[] wavelengths_nm = { 193.0, 248.0, 365.0, 436.0, 532.0 };

Console.WriteLine("============================================");
Console.WriteLine($"  RCWA 1D - Multi-Wavelength Grating Analysis");
Console.WriteLine($"  Timestamp: {ts}");
Console.WriteLine("============================================");
Console.WriteLine();

foreach (var wl_nm in wavelengths_nm)
{
    var wl = wl_nm * 1E-9;
    Console.WriteLine($"--- Wavelength: {wl_nm:F0} nm ---");

    // Build grating medium
    VectorD spans = new VectorD(4, 0.0);
    spans[0] = -period / 2.0; spans[1] = -ridgeWidth / 2.0;
    spans[2] = ridgeWidth / 2.0; spans[3] = period / 2.0;
    VectorZ vals = new VectorZ(3, 0.0);
    vals[0] = nEmbed * nEmbed; vals[1] = nRidge * nRidge; vals[2] = nEmbed * nEmbed;

    var solver = new RCWA1Dp(wavelength: wl, polarization: pol,
        materialFront: new FuncMaterial(nReal: nIn),
        mediumMiddle: new Layer1DPwctMedium(new Pwct1DCplxData(spans, vals)),
        period: period, thickness: thick,
        materialBehind: new FuncMaterial(nReal: nOut));

    var kx = 2.0 * Math.PI / wl * nIn * Math.Sin(0.0);

    var sw = System.Diagnostics.Stopwatch.StartNew();
    solver.ComputeHalfSMatrix(kx0: kx);
    sw.Stop();

    var pw = new PlaneWaveXZ(wavelength: wl, n: nIn, kx: kx, polMode: pol);
    (var t, var gT) = solver.ComputeTCoefficients(pw: pw);
    Transform.FFT1D(x: ref t, grid: ref gT, direction: FFTOptions.Direction.Backward);

    var tMag = VMath.Abs(t);
    var tArg = VMath.Arg(t);
    Console.WriteLine($"  Time: {sw.Elapsed.TotalMilliseconds:F0} ms  Max|E|={VMath.Max(tMag):E4}");

    // --- Save CSV ---
    string label = $"RCWA1D_{wl_nm:F0}nm_{ts}";
    string csvPath = Path.Combine(dataDir, $"{label}.csv");
    using (var w = new StreamWriter(csvPath))
    {
        w.WriteLine("kx,|E|,phase_rad");
        for (int i = 0; i < gT.Count; i++)
            w.WriteLine($"{gT[i]:E8},{(double)tMag[i]:E8},{(double)tArg[i]:E8}");
    }
    Console.WriteLine($"  CSV: {label}.csv");

    // --- Save |E| PNG ---
    string pngMagPath = Path.Combine(imgDir, $"{label}_mag.png");
    SaveLinePlot(pngMagPath, 800, 500,
        $"RCWA 1D Grating |E| (TM, {wl_nm:F0}nm)",
        "kx", "|E|",
        new[] { ("|E|", gT, tMag, Color.DarkGreen) });
    Console.WriteLine($"  PNG: {label}_mag.png");

    // --- Save Phase PNG ---
    string pngPhasePath = Path.Combine(imgDir, $"{label}_phase.png");
    SaveLinePlot(pngPhasePath, 800, 500,
        $"RCWA 1D Grating Phase (TM, {wl_nm:F0}nm)",
        "kx", "Phase [rad]",
        new[] { ("Phase", gT, tArg, Color.SteelBlue) });
    Console.WriteLine($"  PNG: {label}_phase.png");

    Console.WriteLine();
}

Console.WriteLine("============================================");
Console.WriteLine("  ALL 5 WAVELENGTHS COMPLETE");
Console.WriteLine("============================================");

// ============================================================
// Simple chart drawer (System.Drawing)
// ============================================================
static void SaveLinePlot(string path, int w, int h,
    string title, string xLabel, string yLabel,
    (string label, GridInfo1D g, VectorD ys, Color color)[] series)
{
    using var bmp = new Bitmap(w, h);
    using var gfx = Graphics.FromImage(bmp);
    gfx.SmoothingMode = SmoothingMode.AntiAlias;
    gfx.Clear(Color.White);

    float mL = 70, mR = 30, mT = 40, mB = 50;
    float pw = w - mL - mR, ph = h - mT - mB;

    // Find ranges
    double xMin = double.MaxValue, xMax = double.MinValue;
    double yMin = double.MaxValue, yMax = double.MinValue;
    foreach (var s in series)
    {
        for (int i = 0; i < s.g.Count; i++)
        {
            double xv = s.g[i], yv = (double)s.ys[i];
            if (xv < xMin) xMin = xv; if (xv > xMax) xMax = xv;
            if (yv < yMin) yMin = yv; if (yv > yMax) yMax = yv;
        }
    }
    double xPad = (xMax - xMin) * 0.02 + 1e-12;
    double yPad = (yMax - yMin) * 0.05 + 1e-12;
    xMin -= xPad; xMax += xPad;
    yMin -= yPad; yMax += yPad;

    float Tx(double x) => mL + (float)((x - xMin) / (xMax - xMin)) * pw;
    float Ty(double y) => mT + (float)((yMax - y) / (yMax - yMin)) * ph;

    using var fontTitle = new Font("Segoe UI", 12, FontStyle.Bold);
    using var fontAxis = new Font("Segoe UI", 9);
    using var fontLegend = new Font("Segoe UI", 9);
    using var penGrid = new Pen(Color.LightGray, 0.5f);

    // Grid
    for (int i = 0; i <= 5; i++)
    {
        double yv = yMin + (yMax - yMin) * i / 5;
        float gy = Ty(yv);
        gfx.DrawLine(penGrid, mL, gy, mL + pw, gy);
        gfx.DrawString($"{yv:G3}", fontAxis, Brushes.Gray, 2, gy - 8);
    }
    for (int i = 0; i <= 5; i++)
    {
        double xv = xMin + (xMax - xMin) * i / 5;
        float gx = Tx(xv);
        gfx.DrawLine(penGrid, gx, mT, gx, mT + ph);
        gfx.DrawString($"{xv:G3}", fontAxis, Brushes.Gray, gx - 20, mT + ph + 4);
    }

    // Axes
    using var penAxis = new Pen(Color.Black, 1.5f);
    gfx.DrawLine(penAxis, mL, mT, mL, mT + ph);
    gfx.DrawLine(penAxis, mL, mT + ph, mL + pw, mT + ph);

    // Series
    Color[] palette = { Color.DarkGreen, Color.SteelBlue, Color.OrangeRed, Color.Blue, Color.Red };
    float legY = mT + 5;
    for (int si = 0; si < series.Length; si++)
    {
        var s = series[si];
        var c = s.color == default ? palette[si % palette.Length] : s.color;
        using var pen = new Pen(c, 2f);
        var pts = new PointF[s.g.Count];
        for (int i = 0; i < s.g.Count; i++)
            pts[i] = new PointF(Tx(s.g[i]), Ty((double)s.ys[i]));
        if (pts.Length > 1) gfx.DrawLines(pen, pts);

        gfx.FillRectangle(new SolidBrush(c), mL + pw - 140, legY, 12, 12);
        gfx.DrawString(s.label, fontLegend, Brushes.Black, mL + pw - 124, legY - 1);
        legY += 16;
    }

    // Title & labels
    gfx.DrawString(title, fontTitle, Brushes.Black, mL, 8);
    gfx.DrawString(xLabel, fontAxis, Brushes.Black, mL + pw / 2 - 20, mT + ph + 30);
    gfx.DrawString(yLabel, fontAxis, Brushes.Black, 5, mT + ph / 2 - 20);

    gfx.DrawRectangle(new Pen(Color.Black, 1f), mL, mT, pw, ph);
    bmp.Save(path, System.Drawing.Imaging.ImageFormat.Png);
}
