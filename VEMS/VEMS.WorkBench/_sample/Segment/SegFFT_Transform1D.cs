// physical parameters
var waistRadius = 10.0;
// makes a Gaussian function first ...
Func<double, Complex> gauss = (x) => Function1D.Gaussian(x, new List<double>{ waistRadius });
// ... and samples it on a grid
var gx = new GridInfo1D(n: 501, spacing: 0.4); 
var gaussData = new Grid1DCplxData(values: new Samp1DCplxFunc(gauss).Sample(gx), 
    gridInfo: gx, intrpl: InterpolationMethod.Linear);
// direct FFT, for comparison
var fft = new Grid1DCplxData(values: new Samp1DCplxFunc(gauss).Sample(gx), 
    gridInfo: new GridInfo1D(gx), intrpl: InterpolationMethod.Linear);
Transform.FFT1D(d: ref fft, direction: FFTOptions.Direction.Forward);
Printer.Write($"FFT GridInfo: n = {fft.GridInfo.Count}, spacing = {fft.GridInfo.Spacing}");
//VFrame.CreateShow(gaussData, title: $"input Gaussian");

// === varying parameters ===
var nSgs = 5; // number of segments
var eRatio = 0.33; // cosine-edge ratio, not larger than 50%
var sRatio = 2.0 * (1.0 + eRatio); // segment's window size ratio
var sgNx = (long)(sRatio * (gx.Range/nSgs) / gx.Spacing); // sampling points within each segment
// === end of varying parameters ===


// defines segmentation
var ctrs = new GridInfo1D(n: nSgs, spacing: gx.Range/nSgs);
var segmentation = new Grid1DSEG.CosRect(centers: ctrs, diameter: ctrs.Spacing, 
    edge: eRatio * ctrs.Spacing);
var sgx = segmentation.TakeEachFrom(dIn: gaussData, sgNx, gx.Spacing);
var fx = VFrame.CreateFrame();
for(int i = 0; i < sgx.Count; i++)
{ VFrame.AddToFrame(f: fx, gv: sgx[i], plotColor: Options.PlotColor.SteelBlue + i, label: $"Segment [{i}]"); }
//VFrame.RefreshShow(fx);


// Fourier transform of each segment (& summation)
var fKx = VFrame.CreateFrame(); 
//var vKx = new VectorZ(count: sgx[0].GridInfo.Count, 0.0);
var vKx = new VectorZ(count: fft.GridInfo.Count, 0.0);
for(int i = 0; i < sgx.Count; i++)
{
    // transform
    var sgi = sgx[i];
    Transform.FFT1D(d: ref sgi, 
        direction: FFTOptions.Direction.Forward);
    sgi.SamplePhase();
    // interpolation?
    var itp = new Grid1DCplxInterpolation(v: sgi.Values, grid: sgi.GridInfo,
        method: InterpolationMethod.Sinc);
    var vi = itp.Evaluate(targetGrid: fft.GridInfo);
    // sample phase later?
    //sgi.Phase.AddTo(x: ref vi, grid: fft.GridInfo, part: ComplexPart.Argument);
    VFrame.AddToFrame(f: fKx, values: vi, grid: fft.GridInfo, 
        plotColor: Options.PlotColor.SteelBlue + i, label: $"Segment [{i}]");
    // sum
    VMath.AddTo(vi, ref vKx);
}
//VFrame.RefreshShow(fKx);
//var gKx = new GridInfo1D(sgx[0].GridInfo);
var gKx = new GridInfo1D(fft.GridInfo); 
Printer.Write($"SFT GridInfo: n = {gKx.Count}, spacing = {gKx.Spacing}");
VFrame.CreateShow(values: vKx, grid: gKx, 
    xLabel: $"kx", yLabel: $"value", title: $"SFT Result");


// ====================================================== //
// forward Fourier transform (analytical, as a reference)
Func<double, Complex> gaussKx = (kx) => waistRadius / Math.Sqrt(2.0) * 
    Function1D.Gaussian(kx, new List<double>{ 2.0 / waistRadius } );
var aKx = new Samp1DCplxFunc(gaussKx).Sample(gKx);
//VFrame.CreateShow(values: aKx, grid: gKx, 
//    title: "Analytical FT Result", xLabel: "kx", yLabel: "values");
var devAbs = VMath.StandardDeviation(vKx, aKx);
Printer.Write($"Deviation between SFT & reference: Abs=>{devAbs*100}%");
// direct FFT vs analytical result
aKx = new Samp1DCplxFunc(gaussKx).Sample(fft.GridInfo);
devAbs = VMath.StandardDeviation(fft.Values, aKx);
Printer.Write($"Deviation between FFT & reference: Abs=>{devAbs*100}%");