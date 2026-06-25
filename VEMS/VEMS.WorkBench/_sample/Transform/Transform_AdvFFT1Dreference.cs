// parameters
var a = 1.0;
var x0 = 2.0;
var kx0 = -3.0;
// input function in x-domain
Func<double, Complex> xGauss = (x) => 
    Math.Exp(-a * Math.Pow(x-x0, 2.0)) * Complex.Exp(Complex.ImaginaryOne * kx0 * x);
// output function in k-domain
Func<double, Complex> kGauss = (kx) => 1.0 / Math.Sqrt(2.0 * a)
    * Math.Exp(- Math.Pow(kx-kx0, 2.0)/(4.0*a)) * Complex.Exp(-Complex.ImaginaryOne * (kx-kx0) * x0);
  
  
// samples them on grids
var xGrid = new GridInfo1D(n: 101, spacing: 0.1); 
var xv = new Samp1DCplxFunc(f: xGauss).Sample(grid: xGrid);
VFrame.CreateShow(values: xv, grid: xGrid, title: "input in x-domain");
var kGrid = new GridInfo1D(n: 101, spacing: 0.4);
var kv = new Samp1DCplxFunc(f: kGauss).Sample(grid: kGrid);
VFrame.CreateShow(values: kv, grid: kGrid, title: "output in k-domain");


// standard FFT test
var yv = new VectorZ(other: xv, deepCopy: true);
var yGrid = new GridInfo1D(other: xGrid);
Transform.FFT1D(x: ref yv, grid: ref yGrid, 
    direction: FFTOptions.Direction.Forward);
VFrame.CreateShow(values: yv, grid: yGrid, title: $"FFT Test");