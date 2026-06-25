// create matrix a
var a = new MatrixD(2, 2)
{
    [0, 0] = 1,
    [0, 1] = 3,
    [1, 0] = 2,
    [1, 1] = 4
};
// print matrix a
Printer.Write("a = ", a);

// create vector b
var b = new VectorD(2)
{
    [0] = 3,
    [1] = 5,
};
// print vector b 
Printer.Write("b = ", b);

// call linear solver
// solver vector x
var x = LinAlg.LinearSolve(a, b);
Printer.Write("x = ", x);

// LU factorization of matrix m
var m = new MatrixD(3, 3)
{
    [0, 0] = 0.0,
    [0, 1] = 5.0,
    [0, 2] = 22.0/3.0,
    [1, 0] = 4.0,
    [1, 1] = 2.0,
    [1, 2] = 1.0,
    [2, 0] = 2.0,
    [2, 1] = 7.0,
    [2, 2] = 9.0
};
Printer.Write("m = ", m);
LinAlg.LUFactorize(ref m, out VectorI ipiv);
Printer.Write("factorized m = ", m);
Printer.Write("ipiv = ", ipiv);