//int a = 4001;
//int b = 708;
//MatrixZ m = new MatrixZ(a, b, 1 + Complex.ImaginaryOne * 1.0);
//LinAlg.PInv(m);

var aRe = new MatrixD(rows: 2, cols: 3)
{
    [0, 0] = 1.0, [0, 1] = 2.0, [0, 2] = 3.0,
    [1, 0] = 4.0, [1, 1] = 5.0, [1, 2] = 6.0
};
var aIm = new MatrixD(rows: 2, cols: 3)
{
    [0, 0] = 0.1, [0, 1] = 0.2, [0, 2] = 0.3,
    [1, 0] = 0.4, [1, 1] = 0.5, [1, 2] = 0.6
};
var a = VMath.Construct(realPart: aRe, imagPart: aIm);
Printer.Write($"input matrix a: ", a);

var ap = LinAlg.PInv(a);
Printer.Write($"pseudo inverse ap: ", ap, digits: 4);