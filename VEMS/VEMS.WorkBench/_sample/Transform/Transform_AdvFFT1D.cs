// parameters
var a = 1.0;
var x0 = 2.0;
var kx0 = -3.0;
// input function in x-domain
Func<double, Complex> xGauss = (x) => 
    Math.Exp(-a * Math.Pow(x-x0, 2.0)); // linear phase analytical given * Complex.Exp(Complex.ImaginaryOne * kx0 * x);
// output function in k-domain
Func<double, Complex> kGauss = (kx) => 1.0 / Math.Sqrt(2.0 * a)
    * Math.Exp(- Math.Pow(kx-kx0, 2.0)/(4.0*a)) * Complex.Exp(-Complex.ImaginaryOne * (kx-kx0) * x0);
    

// zero-centered grid
var xGrid = new GridInfo1D(n: 101, spacing: 0.1); 
// shifted grid
var start = -0.5 * (xGrid.Count-1) * xGrid.Spacing + x0;
var xGrid2 = new GridInfo1D(n: xGrid.Count, spacing: xGrid.Spacing, refPoint: start, refType: GridRefType.Start);
var xv2 = new Samp1DCplxFunc(f: xGauss).Sample(grid: xGrid2);
VFrame.CreateShow(values: xv2, grid: xGrid2, title: "input in x-domain");
// sampling in k-domain
var kGrid = new GridInfo1D(n: 101, spacing: 0.4);
var kv = new Samp1DCplxFunc(f: kGauss).Sample(grid: kGrid);
VFrame.CreateShow(values: kv, grid: kGrid, title: "output in k-domain");


// advanced FFT test
Transform.FFT1D(x: ref xv2, grid: ref xGrid2, c: ref kx0,
    direction: FFTOptions.Direction.Forward);
Func<double, Complex> linPhaseTerm = (kx) => Complex.Exp(Complex.ImaginaryOne * kx0 * kx);
var lv2 = new Samp1DCplxFunc(f: linPhaseTerm).Sample(grid: xGrid2);
VFrame.CreateShow(values: xv2 * lv2, grid: xGrid2, title: $"Advanced FFT Test");