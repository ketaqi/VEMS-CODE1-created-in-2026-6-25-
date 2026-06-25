// see example from Wolfram
// https://reference.wolfram.com/language/ref/SingularValueDecomposition.html.zh
var m = new MatrixD(rows: 3, cols: 2)
{
    [0, 0] = 1.2, [0, 1] = 3.4,
    [1, 0] = 5.6, [1, 1] = 7.8,
    [2, 0] = 9.0, [2, 1] = 1.2
};
Printer.Write($"input matrix m = ", m);

LinAlg.SVDecompose(a: m,
     s: out VectorD s, 
     u: out MatrixD u, 
     vt: out MatrixD vt);
     
Printer.Write($"left singular vectors u = ", u);
Printer.Write($"singular values s = ", s);
Printer.Write($"transpose of right singular vectors vt = ", vt);

Printer.Write($"input matrix after performing SVD m = ", m);