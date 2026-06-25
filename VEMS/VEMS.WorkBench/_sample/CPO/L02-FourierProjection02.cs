// defines source function
var waistRadius = 10.0;
Func<double, Complex> f = (x) => Complex.Exp(- x*x / (waistRadius * waistRadius));
// defines target function
var kx = 0.20;
Func<double, Complex> t = (x) => Complex.Exp(-Complex.ImaginaryOne * kx * x);


// inner product
var prod = new Product1DCplx(source: f, target: t);
var c = prod.Evaluate(samples: 201, start: -75.0, end: 75.0);
Printer.Write($"Inner product = {c}");