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
var xd2 = new Grid1DCplxData(values: xv2, gridInfo: xGrid2, a1: kx0);
VFrame.CreateShow(values: xv2, grid: xGrid2, title: "input in x-domain");
// sampling in k-domain
var kGrid = new GridInfo1D(n: 101, spacing: 0.4);
var kv = new Samp1DCplxFunc(f: kGauss).Sample(grid: kGrid);
//VFrame.CreateShow(values: kv, grid: kGrid, title: "output in k-domain");


// advanced FFT test - forward transform
Transform.FFT1D(d: ref xd2, direction: FFTOptions.Direction.Forward);
var v = new VectorZ(other: xd2.Values, deepCopy: true);
xd2.Phase.AddTo(x: ref v, grid: xd2.GridInfo); // adds linear phase ...
VFrame.CreateShow(values: v, grid: xd2.GridInfo, title: "Advanced FFT - Forward Transform");


// backward transform
Transform.FFT1D(d: ref xd2, direction: FFTOptions.Direction.Backward);
VFrame.CreateShow(values: xd2.Values, grid: xd2.GridInfo, title: "Advanced FFT - Backward Transform");