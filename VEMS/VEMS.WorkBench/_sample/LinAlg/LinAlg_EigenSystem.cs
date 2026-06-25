// print code info
Printer.Write("Matrix Eigen System ... ");

var a = new MatrixD(3, 3)
{
    [0, 0] = 1.0,
    [0, 1] = 2.0,
    [0, 2] = 3.0,
    [1, 0] = 3.0,
    [1, 1] = 2.0,
    [1, 2] = 1.0,
    [2, 0] = 2.0,
    [2, 1] = 1.0,
    [2, 2] = 3.0
};
Printer.Write("a = ", a);

LinAlg.EigenSystem(ref a, out VectorZ w, out MatrixD v);
Printer.Write("w = ", w);
Printer.Write("v = ", v);