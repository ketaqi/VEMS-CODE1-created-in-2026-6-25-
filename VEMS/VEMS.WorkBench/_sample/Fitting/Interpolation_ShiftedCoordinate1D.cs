// parameters
var a = 1.0;
var x0 = 2.0;
var kx0 = -3.0;
// input function in x-domain
Func<double, Complex> xGauss = (x) => 
    Math.Exp(-a * Math.Pow(x-x0, 2.0)); // linear phase analytical given * Complex.Exp(Complex.ImaginaryOne * kx0 * x);

// zero-centered grid
var xGrid = new GridInfo1D(n: 101, spacing: 0.1); 
// shifted grid
var start = -0.5 * (xGrid.Count-1) * xGrid.Spacing + x0;
var xGrid2 = new GridInfo1D(n: xGrid.Count, start: start, spacing: xGrid.Spacing);
var xv2 = new Samp1DCplxFunc(f: xGauss).Sample(grid: xGrid2);
VFrame.CreateShow(values: xv2, grid: xGrid2, title: "input in x-domain");


// denser grid in x-domain
var xGrid3 = new GridInfo1D(n: xGrid.Count * 2 + 1, spacing: xGrid.Spacing / 2);
var intrpl = new Grid1DCplxInterpolation(v: xv2, grid: xGrid2, 
    method: InterpolationMethod.Sinc, bound: DataBoundary.ConstantZero);
var xv3 = intrpl.Evaluate(targetGrid: xGrid3);
VFrame.CreateShow(values: xv3, grid: xGrid3, title: "interpolation in x-domain");