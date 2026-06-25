using System.Drawing;
using VEMS.MathCore;
using VEMS.EMSolver;
using GridInfo1D = VEMS.MathCore.GridInfo1D;
using VMath = VEMS.MathCore.VMath;
using Real = System.Double;
using Int = System.Int64;

string outDir = @"c:\Users\Weilun Xu\Desktop\VSCODE test\ai-runs\test6-24\output";
string dataDir = Path.Combine(outDir, "data");
string imageDir = Path.Combine(outDir, "images");
Directory.CreateDirectory(dataDir);
Directory.CreateDirectory(imageDir);

Console.WriteLine("=== VEMS Simulation Suite - test6-24 ===\n");

// ============================================================
// SIM 1: FreeSpace SPW 1D Propagation
// ============================================================
Console.WriteLine("--- SIM 1: FreeSpace SPW 1D ---");
var nx = 151; var dx = 1.0E-6;
var g = new GridInfo1D(n: nx, spacing: dx);
var det = new Detector1D(grid: g);
var w = 7.5E-6; var wl = 632.8E-9;

var v = new SCField1D.PlaneWave(wavelength: wl,
    material: new FuncMaterial(nReal: 1.0),
    diameter: 10.0 * w, grid: g, edge: 2.0 * w);

var vIn = det.Sample(v: v, quantity: DetectQuantity.Magnitude);
var sumIn = VMath.Sum(vIn);
Console.WriteLine($"Input  max|E|={VMath.Max(vIn):E4}  energy={sumIn*dx*1E6:E4}");

var sw = System.Diagnostics.Stopwatch.StartNew();
v.Propagate(d: 675E-6, targetDomain: ModelingDomain.Spatial, loopMode: LoopMode.Parallel);
sw.Stop();

var vOut = det.Sample(v: v, quantity: DetectQuantity.Magnitude);
var sumOut = VMath.Sum(vOut);
Console.WriteLine($"Output max|E|={VMath.Max(vOut):E4}  energy={sumOut*dx*1E6:E4}");
Console.WriteLine($"Time: {sw.Elapsed.TotalMilliseconds:F1} ms  Ratio: {sumOut/sumIn:F6}");
Console.WriteLine();

// CSV
using (var writer = new StreamWriter(Path.Combine(dataDir, "spw1d_propagation.csv")))
{
    writer.WriteLine("x_um,|E|_input,|E|_output");
    for (int i = 0; i < nx; i++)
        writer.WriteLine($"{g[i]*1E6:F6},{(double)vIn[i]:E8},{(double)vOut[i]:E8}");
}

// PNG
var xs_spw = new double[nx];
var ysIn = new double[nx];
var ysOut = new double[nx];
for (int i = 0; i < nx; i++) { xs_spw[i] = g[i]*1E6; ysIn[i] = (double)vIn[i]; ysOut[i] = (double)vOut[i]; }
SimplePlot.SaveLines(Path.Combine(imageDir, "spw1d_propagation.png"), 800, 500,
    "FreeSpace SPW 1D - Field Propagation", "x [um]", "|E|",
    new[] { ("Input |E|", xs_spw, ysIn, Color.SteelBlue),
            ("Output |E| (z=675um)", xs_spw, ysOut, Color.OrangeRed) });

// ============================================================
// SIM 2: RCWA 1D Rectangular Grating
// ============================================================
Console.WriteLine("--- SIM 2: RCWA 1D Rectangular Grating ---");
var wl2 = 193.0E-9; var pol = InPlanePolMode.TM;
var nIn_rc = 1.5593; var nOut_rc = 1.0;
var nRidge = new System.Numerics.Complex(1.3282, 1.6637); var nEmbed = 1.0;
var period = 25.456E-6; var rw = 12.728E-6; var thick = 0.1E-6;
Console.WriteLine($"wl={wl2*1E9:F0}nm period={period*1E6:F3}um ridge={rw*1E6:F3}um thick={thick*1E9:F0}nm");

VectorD spans = new VectorD(4, 0.0);
spans[0] = -period / 2.0; spans[1] = -rw / 2.0;
spans[2] = rw / 2.0; spans[3] = period / 2.0;
VectorZ vals = new VectorZ(3, 0.0);
vals[0] = nEmbed * nEmbed; vals[1] = nRidge * nRidge; vals[2] = nEmbed * nEmbed;

var solver = new RCWA1Dp(wavelength: wl2, polarization: pol,
    materialFront: new FuncMaterial(nReal: nIn_rc),
    mediumMiddle: new Layer1DPwctMedium(new Pwct1DCplxData(spans, vals)),
    period: period, thickness: thick,
    materialBehind: new FuncMaterial(nReal: nOut_rc));

var kx = 2.0 * Math.PI / wl2 * nIn_rc * Math.Sin(0.0);
Console.Write("Computing S-matrix... ");
var sw2 = System.Diagnostics.Stopwatch.StartNew();
solver.ComputeHalfSMatrix(kx0: kx);
sw2.Stop();
Console.WriteLine($"done ({sw2.Elapsed.TotalMilliseconds:F0} ms)");

var pw = new PlaneWaveXZ(wavelength: wl2, n: nIn_rc, kx: kx, polMode: pol);
(var t, var gT) = solver.ComputeTCoefficients(pw: pw);
Transform.FFT1D(x: ref t, grid: ref gT, direction: FFTOptions.Direction.Backward);

var tMag = VMath.Abs(t);
var tPhase = VMath.Arg(t);
Console.WriteLine($"Max |E|={VMath.Max(tMag):E4}  grid pts={gT.Count}");
Console.WriteLine();

// CSV
using (var writer = new StreamWriter(Path.Combine(dataDir, "rcwa1d_grating.csv")))
{
    writer.WriteLine("kx,|E|,phase_rad");
    for (int i = 0; i < gT.Count; i++)
        writer.WriteLine($"{gT[i]:E8},{(double)tMag[i]:E8},{(double)tPhase[i]:E8}");
}

// PNG: transmitted field
int nPts = (int)gT.Count;
var kxs = new double[nPts];
var mags = new double[nPts];
for (int i = 0; i < nPts; i++) { kxs[i] = gT[i]; mags[i] = (double)tMag[i]; }
SimplePlot.SaveLines(Path.Combine(imageDir, "rcwa1d_grating.png"), 800, 500,
    "RCWA 1D - Grating Diffraction (TM, 193nm)", "kx", "|E|",
    new[] { ("|E| transmitted", kxs, mags, Color.DarkGreen) });

// PNG: grating profile
var xProf = new double[] { -period/2*1E6, -rw/2*1E6, -rw/2*1E6, rw/2*1E6, rw/2*1E6, period/2*1E6 };
var nProf = new double[] { 1.0, 1.0, nRidge.Real, nRidge.Real, 1.0, 1.0 };
var kProf = new double[] { 1.0, 1.0, nRidge.Imaginary, nRidge.Imaginary, 1.0, 1.0 };
SimplePlot.SaveLines(Path.Combine(imageDir, "rcwa1d_grating_profile.png"), 800, 400,
    "Grating Refractive Index Profile (1 period)", "x [um]", "n, k",
    new[] { ("n (real)", xProf, nProf, Color.Blue),
            ("k (imag)", xProf, kProf, Color.Red) });

Console.WriteLine("\n=== ALL SIMULATIONS COMPLETE ===");

// ============================================================
// Minimal chart drawer using System.Drawing
// ============================================================
static class SimplePlot
{
    public static void SaveLines(string path, int w, int h,
        string title, string xLabel, string yLabel,
        (string label, double[] xs, double[] ys, Color color)[] series)
    {
        using var bmp = new System.Drawing.Bitmap(w, h);
        using var gfx = System.Drawing.Graphics.FromImage(bmp);
        gfx.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        gfx.Clear(Color.White);

        float marginL = 70, marginR = 30, marginT = 40, marginB = 50;
        float pw = w - marginL - marginR;
        float ph = h - marginT - marginB;

        // Find ranges
        double xMin = double.MaxValue, xMax = double.MinValue;
        double yMin = double.MaxValue, yMax = double.MinValue;
        foreach (var s in series)
        {
            for (int i = 0; i < s.xs.Length; i++)
            {
                if (s.xs[i] < xMin) xMin = s.xs[i];
                if (s.xs[i] > xMax) xMax = s.xs[i];
                if (s.ys[i] < yMin) yMin = s.ys[i];
                if (s.ys[i] > yMax) yMax = s.ys[i];
            }
        }
        double xPad = (xMax - xMin) * 0.02;
        double yPad = (yMax - yMin) * 0.05;
        if (xPad == 0) xPad = 1;
        if (yPad == 0) yPad = 0.1;
        xMin -= xPad; xMax += xPad;
        yMin -= yPad; yMax += yPad;

        float Tx(double x) => marginL + (float)((x - xMin) / (xMax - xMin)) * pw;
        float Ty(double y) => marginT + (float)((yMax - y) / (yMax - yMin)) * ph;

        using var fontTitle = new System.Drawing.Font("Segoe UI", 12, FontStyle.Bold);
        using var fontAxis = new System.Drawing.Font("Segoe UI", 9);
        using var fontLegend = new System.Drawing.Font("Segoe UI", 9);
        using var penGrid = new System.Drawing.Pen(Color.LightGray, 0.5f);

        // Grid
        int nGridY = 5;
        for (int i = 0; i <= nGridY; i++)
        {
            double yv = yMin + (yMax - yMin) * i / nGridY;
            float gy = Ty(yv);
            gfx.DrawLine(penGrid, marginL, gy, marginL + pw, gy);
            gfx.DrawString($"{yv:G3}", fontAxis, Brushes.Gray, 2, gy - 8);
        }
        int nGridX = 5;
        for (int i = 0; i <= nGridX; i++)
        {
            double xv = xMin + (xMax - xMin) * i / nGridX;
            float gx = Tx(xv);
            gfx.DrawLine(penGrid, gx, marginT, gx, marginT + ph);
            gfx.DrawString($"{xv:G3}", fontAxis, Brushes.Gray, gx - 20, marginT + ph + 4);
        }

        // Axes
        using var penAxis = new System.Drawing.Pen(Color.Black, 1.5f);
        gfx.DrawLine(penAxis, marginL, marginT, marginL, marginT + ph);
        gfx.DrawLine(penAxis, marginL, marginT + ph, marginL + pw, marginT + ph);

        // Series
        Color[] palette = { Color.SteelBlue, Color.OrangeRed, Color.DarkGreen, Color.Blue, Color.Red, Color.Purple };
        float legendY = marginT + 5;
        for (int si = 0; si < series.Length; si++)
        {
            var s = series[si];
            var c = s.color == default ? palette[si % palette.Length] : s.color;
            using var pen = new System.Drawing.Pen(c, 2f);
            var pts = new System.Drawing.PointF[s.xs.Length];
            for (int i = 0; i < s.xs.Length; i++)
                pts[i] = new System.Drawing.PointF(Tx(s.xs[i]), Ty(s.ys[i]));
            if (pts.Length > 1)
                gfx.DrawLines(pen, pts);

            // Legend
            gfx.FillRectangle(new System.Drawing.SolidBrush(c), marginL + pw - 150, legendY, 12, 12);
            gfx.DrawString(s.label, fontLegend, Brushes.Black, marginL + pw - 134, legendY - 1);
            legendY += 16;
        }

        // Title & labels
        gfx.DrawString(title, fontTitle, Brushes.Black, marginL, 8);
        gfx.DrawString(xLabel, fontAxis, Brushes.Black, marginL + pw / 2 - 30, marginT + ph + 30);
        gfx.DrawString(yLabel, fontAxis, Brushes.Black, 5, marginT + ph / 2 - 20);

        // Border
        gfx.DrawRectangle(new System.Drawing.Pen(Color.Black, 1f), marginL, marginT, pw, ph);

        bmp.Save(path, System.Drawing.Imaging.ImageFormat.Png);
        Console.WriteLine($"Saved: {path}");
    }
}
