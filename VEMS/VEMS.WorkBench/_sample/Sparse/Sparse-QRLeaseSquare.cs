// dense vectors on the right
var b = new VectorD(7){
    [0]=-1.0, [1]=8.0, [2]=4.0, [3]=2.0, [4]=11.0, [5]=3.0, [6]=1.0 };

// initialize sparse matrix in COO format 
var rows = new VectorI(15){
    [0]=0, [1]=0, [2]=0,
    [3]=1, [4]=1, 
    [5]=2, [6]=2, 
    [7]=3, [8]=3, [9]=3,
    [10]=4, [11]=4,
    [12]=5,
    [13]=6, [14]=6 };
var cols = new VectorI(15){
    [0]=0, [1]=1, [2]=3,
    [3]=0, [4]=1, 
    [5]=3, [6]=4, 
    [7]=0, [8]=2, [9]=3,
    [10]=1, [11]=4,
    [12]=2,
    [13]=0, [14]=4 };
var nzCOO = new VectorD(15){
    [0]= 1.0, [1]=-1.0, [2]=-3.0,
    [3]=-2.0, [4]= 5.0,
    [5]= 6.0, [6]= 4.0, 
    [7]=-4.0, [8]= 2.0, [9]= 7.0,
    [10]=8.0, [11]=-5.0,
    [12]=1.0,
    [13]=2.0, [14]=1.0 };
var aiCOO = Sparse.InitWMatrixDi(7, 5, 15);
Sparse.FillWMatrixDiCOO(ref aiCOO, rows, cols, nzCOO);
Printer.Write($"ai[COO] status = {aiCOO.Status.ToString()}");

// MUST first converts sparse matrix into CSR format 
var aiCSR = Sparse.ConvertWMatrixDi2CSR(aiCOO, 10);  //.InitWMatrixDi(7, 5, 15);
//Sparse.FillWMatrixDiCSR(ref ai, rowPtr, colIdx, nzCSR);
Printer.Write($"ai[CSR] status = {aiCSR.Status.ToString()}");
    
// then perform QR least square on the sparse matrix in CSR format
var x = Sparse.LeastSquare(aiCSR, b);
Printer.Write("x = ", x);

// QR least square in steps
// ...