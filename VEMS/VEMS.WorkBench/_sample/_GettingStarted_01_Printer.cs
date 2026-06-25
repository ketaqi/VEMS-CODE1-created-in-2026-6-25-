// vector - number format
var v = VStat.RngUniform(n: 7, a: 0.0, b: 0.5);
Printer.Write(prompt: "default format: ", x: v);
Printer.Write(prompt: "more digits: ", x: v, digits: 6);
Printer.Write(prompt: "scientific: ", x: v, numFormat: NumericFormat.Exponential, digits: 3);
Printer.Write(prompt: "fixed decimal: ", x: v, numFormat: NumericFormat.FixedPoint, digits: 4);
Printer.Write(prompt: "percent: ", x: v, numFormat: NumericFormat.Percent, digits: 1);

// complex number
var mRe = VStat.RngUniform(rows: 4, cols: 3, a: 0.0, b: 1.0);
var mIm = VStat.RngGaussian(rows: 4, cols: 3, a: 0.0, sigma: 0.1);
var m = VMath.Construct(realPart: mRe, imagPart: mIm);
Printer.Write(prompt: "real- and imag-parts: ", a: m, cplxFormat: ComplexFormat.RealAndImaginary);
Printer.Write(prompt: "magnitude and phase: ", a: m, cplxFormat: ComplexFormat.MagnitudeAndPhase);