var a = new MatrixD(3, 2){
    [0,0] = 1.0, [0,1] = 2.0, 
    [1,0] = 3.0, [1,1] = 4.0,
    [2,0] = 5.0, [2,1] = 6.0};
Printer.Write("a = ", a);

var x1 = new VectorD(2){ [0] = 0.5, [1] = 0.6 };
Printer.Write("x1 = ", x1);

var y1 = LinAlg.Dot(a, x1);
Printer.Write("y1 = a*x1 ", y1);

var x2 = new VectorD(3){ [0] = 0.5, [1] = 0.6, [2] = 0.7 };
Printer.Write("x2 = ", x2);

var y2 = LinAlg.Dot(a, x2, 1.0, BLAS_Transpose.Trans);
Printer.Write("y2 = aT*x2 ", y2);