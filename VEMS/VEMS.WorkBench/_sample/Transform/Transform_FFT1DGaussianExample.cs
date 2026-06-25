// physical parameters:
// Gaussian waist
var waistRadius = 2.5; // value 25.0, 2.5
var shift = 0.0;

// numerical objects #1: change sampling distance
var f = new Grid1DRealData(count: 11, spacing: 2.5, initVal: 0.0);
//var f = new Grid1DRealData(15, 2.0, 0.0);
//var f = new Grid1DRealData(21, 1.5, 0.0);
//var f = new Grid1DRealData(31, 1.0, 0.0);
//var f = new Grid1DRealData(41, 0.75, 0.0);
//var f = new Grid1DRealData(61, 0.5, 0.0);
//var f = new Grid1DRealData(75, 0.4, 0.0);

// numerical objects #2: change window size
//var f = new Grid1DRealData(11, 0.4, 0.0);
//var f = new Grid1DRealData(25, 0.4, 0.0);
//var f = new Grid1DRealData(51, 0.4, 0.0);
//var f = new Grid1DRealData(75, 0.4, 0.0);

for (long i = 0; i < f.GridInfo.Count; i++)
{
    var t = f.GridInfo.GetCoordinate(i);
    f.Values[i] = Math.Exp(-(t-shift) * (t-shift) / (waistRadius * waistRadius));
}

// display the input data
VFrame.CreateShow(gv: f, title: "Input Gaussian", xLabel: "t", yLabel: "f(t)");

// forward Fourier transform
var yFT = new Grid1DCplxData(values: new VectorZ(f.Values), gridInfo: f.GridInfo);
Transform.FFT1D(d: ref yFT, direction: FFTOptions.Direction.Forward);
VFrame.CreateShow(gv: yFT, title: "FFT of f(t)", xLabel: "w", yLabel: "F(w)");

// analytical Fourier transform result as a reference
var yFTa = new Grid1DRealData(yFT.GridInfo.Count, yFT.GridInfo.Spacing, 0.0);
for (long i = 0; i < yFTa.GridInfo.Count; i++)
{
    var omega = yFTa.GridInfo.GetCoordinate(i);
    yFTa.Values[i] = waistRadius / Math.Sqrt(2.0)
        * Math.Exp(-waistRadius * waistRadius * omega * omega / 4.0);
}
VFrame.CreateShow(gv: yFTa, title: "Analytucal FT of f(t)", xLabel: "w", yLabel: "F(w)");

// compute difference between analytical and FFT
var diff = yFT.Values - yFTa.Values;
var yDiff = new Grid1DRealData(values: VMath.Abs(diff), gridInfo: yFT.GridInfo);
VFrame.CreateShow(gv: yDiff, title: "difference [FFT - Analytic]", xLabel: "w", yLabel: "Difference");

// minimum and maximum differences
var iAmx = VMath.IAmx(x: diff);
var iAmn = VMath.IAmn(x: diff);
Printer.Write($"Maximum difference = {diff[iAmx]}");
Printer.Write($"Minimum difference = {diff[iAmn]}");