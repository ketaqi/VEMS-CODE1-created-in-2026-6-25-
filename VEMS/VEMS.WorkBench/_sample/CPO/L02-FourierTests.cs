var fun = new Samp1DRealFunc(f: (x, p) => Math.Cos(3*x));
var n = 1201;
var grid = new GridInfo1D(n: n, spacing: 2.0 * Math.PI / n);
var v = fun.Sample(grid);
VFrame.CreateShow(v);


// projection test ...
var kx = -3.0;
var fKx = Complex.Zero;
Action<long> p = (i) => 
{
    var x = grid[i];
    fKx += v[i] * Complex.Exp(-Complex.ImaginaryOne * kx * x);
};
var loop = new Loop1D(operation: p, start: 0, end: grid.Count);
loop.Evaluate();
fKx *= grid.Spacing / Math.Sqrt(2.0 * Math.PI);
Printer.Write($"f[kx] @kx={kx}: {fKx}");