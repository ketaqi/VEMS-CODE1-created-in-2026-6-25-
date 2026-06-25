// create matrix #1
var m1 = new MatrixD(2, 2)
{
    [0, 0] = 1,
    [0, 1] = 3,
    [1, 0] = 2,
    [1, 1] = 4
};
// print matrix #1
Printer.Write("m1 = ", m1);

// create matrix #2
var m2 = new MatrixD(2, 2)
{
    [0, 0] = 3,
    [0, 1] = 0,
    [1, 0] = 1,
    [1, 1] = 5
};
// print matrix #2
Printer.Write("m2 = ", m2);

// matrix-matrix multiplication
var p = LinAlg.Dot(m1, m2);
// print product
Printer.Write("p = m1 * m2 = ", a: p, digits: 2);