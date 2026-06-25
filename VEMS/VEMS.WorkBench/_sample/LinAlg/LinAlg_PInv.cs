// three examples from Wolfram
// https://reference.wolfram.com/language/ref/PseudoInverse.html.zh?source=footer
// #01
var m1 = new MatrixD(rows: 2, cols: 2)
{
    [0, 0] = 1, [0, 1] = 2,
    [1, 0] = 3, [1, 1] = 4
};
var m1i = LinAlg.PInv(m1);
Printer.Write($"pinv({nameof(m1)}) = ", m1i);

// #02
var m2 = new MatrixD(rows: 3, cols: 3)
{
    [0, 0] = 1, [0, 1] = 2, [0, 2] = 3,
    [1, 0] = 4, [1, 1] = 5, [1, 2] = 6,
    [2, 0] = 7, [2, 1] = 8, [2, 2] = 9
};
var m2i = LinAlg.PInv(m2);
Printer.Write($"pinv({nameof(m2)}) = ", m2i);

// #03
var m3 = new MatrixD(rows: 2, cols: 3)
{
    [0, 0] = 1, [0, 1] = 2, [0, 2] = 3,
    [1, 0] = 4, [1, 1] = 5, [1, 2] = 6,
};
var m3i = LinAlg.PInv(m3);
Printer.Write($"pinv({nameof(m3)}) = ", m3i);


// example from Matlab
// https://ww2.mathworks.cn/help/matlab/ref/pinv.html
var a = new MatrixD(rows: 8, cols: 6);
a[0,0] = 64; a[0,1] = 2; a[0,2] = 3; a[0,3] = 61; a[0,4] = 60; a[0,5] = 6;
a[1,0] = 9; a[1,1] = 55; a[1,2] = 54; a[1,3] = 12; a[1,4] = 13; a[1,5] = 51;
a[2,0] = 17; a[2,1] = 47; a[2,2] = 46; a[2,3] = 20; a[2,4] = 21; a[2,5] = 43;
a[3,0] = 40; a[3,1] = 26; a[3,2] = 27; a[3,3] = 37; a[3,4] = 36; a[3,5] = 30;
a[4,0] = 32; a[4,1] = 34; a[4,2] = 35; a[4,3] = 29; a[4,4] = 28; a[4,5] = 38;
a[5,0] = 41; a[5,1] = 23; a[5,2] = 22; a[5,3] = 44; a[5,4] = 45; a[5,5] = 19;
a[6,0] = 49; a[6,1] = 15; a[6,2] = 14; a[6,3] = 52; a[6,4] = 53; a[6,5] = 11;
a[7,0] = 8; a[7,1] = 58; a[7,2] = 59; a[7,3] = 5; a[7,4] = 4; a[7,5] = 62;

var ap = LinAlg.PInv(a);
Printer.Write($"pseudo inverse ap = ", ap);
var b = new VectorD(count: 8, initVal: 260);
var c = LinAlg.Dot(ap, b);
Printer.Write("test result c = ", c, digits: 4);


// solution by least square?
//var c2 = LinAlg.LeastSquare(a, b);
//Printer.Write($"test by least square c2 = ", c2, digits: 4);