// generates variables
var x = new VectorD(count: 51, initVal: 1.1, increment: 0.1);
// VMath calculation
var sqrt = VMath.Sqrt(x);
// reference calculation
var sqrt0 = new VectorD(other: x, deepCopy: true);
for(long i = 0; i < sqrt0.Count; i++)
    sqrt0[i] = Math.Sqrt(sqrt0[i]);
// computes deviation
Printer.Write($"stdDev = {VMath.StandardDeviation(x: sqrt, y: sqrt0)}");


// with manually added difference ...
var v1 = new VectorD(count: x.Count);
VMath.Copy(x: x, y: ref v1);
var v2 = v1 + VStat.RngUniform(n: x.Count, a: 0.0, b: 0.09);
Printer.Write($"Devivation = {VMath.StandardDeviation(x: v2, y: v1)}");
var corr = VMath.Correlation(x: v2, y: v1);
Printer.Write($"Correlation = {Converter.NumberToString(x: corr, numFormat: NumericFormat.Percent)}");