// create matrix m
var m = new MatrixD(7, 5)
{
    [0,0]= 1.0, [0,1]=-1.0, [0,2]= 0.0, [0,3]=-3.0, [0,4]= 0.0,
    [1,0]=-2.0, [1,1]= 5.0, [1,2]= 0.0, [1,3]= 0.0, [1,4]= 0.0,
    [2,0]= 0.0, [2,1]= 0.0, [2,2]= 0.0, [2,3]= 6.0, [2,4]= 4.0,
    [3,0]=-4.0, [3,1]= 0.0, [3,2]= 2.0, [3,3]= 7.0, [3,4]= 0.0,
    [4,0]= 0.0, [4,1]= 8.0, [4,2]= 0.0, [4,3]= 0.0, [4,4]=-5.0,
    [5,0]= 0.0, [5,1]= 0.0, [5,2]= 1.0, [5,3]= 0.0, [5,4]= 0.0,
    [6,0]= 2.0, [6,1]= 0.0, [6,2]= 0.0, [6,3]= 0.0, [6,4]= 1.0
};
// print matrix m
Printer.Write("m = ", m);

// create vector
var b = new VectorD(7){
    [0]=-1.0, [1]= 8.0, [2]= 4.0, [3]= 2.0, [4]=11.0, [5]= 3.0, [6]= 1.0 };
// print vector 
Printer.Write("b = ", b);

// call least square solver
var x = LinAlg.LeastSquare(m, b);

// print vector
Printer.Write("x = ", x);


// ...
// naive method test ...
var x2 = LinAlg.QRLeastSquare(m, b);
Printer.Write($"x2 = ", x2);