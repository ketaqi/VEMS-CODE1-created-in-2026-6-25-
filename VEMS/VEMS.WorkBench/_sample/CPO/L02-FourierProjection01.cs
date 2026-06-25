// defines source function
Func<double, double> f = (x) => 1.0 * Math.Cos(2.0 * Math.PI * 1.0 * x) 
    + 2.0 * Math.Cos(2.0 * Math.PI * 2.0 * x)
    + 3.0 * Math.Cos(2.0 * Math.PI * 3.0 * x);
// defines target function
Func<double, double> t = (x) => Math.Cos(2.0 * Math.PI * 1.0 * x);


// inner product
var p = new Product1DReal(source: f, target: t);
var c = p.Evaluate(samples: 101, start: -1.0, end: 1.0);
Printer.Write($"Inner product = {c}");