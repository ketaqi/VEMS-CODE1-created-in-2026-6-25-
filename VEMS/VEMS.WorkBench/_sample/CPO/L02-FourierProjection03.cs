// prepares Zernikes ...
var z31 = new CommonFunction.Zernike(n: 3, m: 1);
var z33 = new CommonFunction.Zernike(n: 3, m: 3);

// defines source function
Func<double, double, double> f = (x, y) => 
{
    (var rho, var phi) = Converter.Cartesian2Polar(x, y);
    return 1.5 * z31.Evaluate(rho, phi) + 0.5 * z33.Evaluate(rho, phi);
};
// defines target function
Func<double, double, double> t = (x, y) =>
{
    (var rho, var phi) = Converter.Cartesian2Polar(x, y);
    return z33.Evaluate(rho, phi);
};


// inner product
var prod = new Product2DReal(source: f, target: t);
var c = prod.Evaluate(rowSamples: 81, colSamples: 81,
    rowStart: -1.0, rowEnd: 1.0,
    colStart: -1.0, colEnd: 1.0,
    loopMode: LoopMode.Parallel);
Printer.Write($"Inner product = {c}");