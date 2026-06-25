// print code info
Printer.Write("Matrix Generalized Eigen System ... ");

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

var b = new MatrixD(3, 3)
{
    [0, 0] = 4.0,
    [0, 1] = 0.0,
    [0, 2] = 0.0,
    [1, 0] = 0.0,
    [1, 1] = 5.0,
    [1, 2] = 0.0,
    [2, 0] = 0.0,
    [2, 1] = 0.0,
    [2, 2] = 6.0
};

Printer.Write("a = ", a);
Printer.Write("b = ", b);

// output the eigen value directly
LinAlg.GeneralizedEigenSystem(ref a, ref b, out VectorZ w, out MatrixD v);

Printer.Write("w = ", w);
Printer.Write("v = ", v);

// output the eigen value with alpha and beta, which eigen value = alpha/beta
LinAlg.GeneralizedEigenSystem(ref a, ref b, out VectorZ alpha, out VectorD beta,out MatrixD vec);

var val = alpha / beta;
Printer.Write("alpha = ", alpha);
Printer.Write("beta = ", beta);
Printer.Write("w = ", val);
Printer.Write("v = ", vec);
