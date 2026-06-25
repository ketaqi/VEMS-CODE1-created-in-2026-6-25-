// dense vectors as containers
var x = new VectorD(5, 1.0);
var yCSR = new VectorD(5, 0.0);
var yCSC = new VectorD(5, 0.0);
var yCOO = new VectorD(5, 0.0);


// 1) initialize sparse matrix in CSR format 
var rowPtr = new VectorI(5+1){ 
    [0]=0, [1]=3, [2]=5, [3]=8, [4]=11, [5]=13 };
var colIdx = new VectorI(13){
    [0]=0, [1]=1, [2]=3, 
    [3]=0, [4]=1, 
    [5]=2, [6]=3, [7]=4, 
    [8]=0, [9]=2, [10]=3,
    [11]=1, [12]=4 };
var nzCSR = new VectorD(13){
    [0]=1.0, [1]=-1.0, [2]=-3.0,
    [3]=-2.0, [4]=5.0,
    [5]=4.0, [6]=6.0, [7]=4.0,
    [8]=-4.0, [9]=2.0, [10]=7.0,
    [11]=8.0, [12]=-5.0 };
var aiCSR = new MatrixDi(rows: 5, cols: 5, nnz: 13, 
    nzInfo: new MDiCSRInfo(rowPtr, colIdx, nzCSR)); 
Printer.Write($"ai[CSR] status = {aiCSR.Status.ToString()}");

// 2) initialize sparse matrix in CSC format 
var colPtr = new VectorI(5+1){
    [0]=0, [1]=3, [2]=6, [3]=9, [4]=11, [5]=13 };
var rowIdx = new VectorI(13){
    [0]=0, [1]=1, [2]=3,
    [3]=0, [4]=1, [5]=4,
    [6]=0, [7]=2, [8]=3,
    [9]=2, [10]=3,
    [11]=2, [12]=4 };
var nzCSC = new VectorD(13){
    [0]=1.0, [1]=-2.0, [2]=-4.0,
    [3]=-1.0, [4]=5.0, [5]=8.0,
    [6]=-3.0, [7]=4.0, [8]=2.0,
    [9]=6.0, [10]=7.0, 
    [11]=4.0, [12]=-5.0 };
var aiCSC = new MatrixDi(rows: 5, cols: 5, nnz: 13,
    nzInfo: new MDiCSCInfo(colPtr, rowIdx, nzCSC));
Printer.Write($"ai[CSC] status = {aiCSC.Status.ToString()}");

// 3) initialize sparse matrix in COO format 
var rows = new VectorI(13){
    [0]=0, [1]=0, [2]=0,
    [3]=1, [4]=1, 
    [5]=2, [6]=2, [7]=2,
    [8]=3, [9]=3, [10]=3,
    [11]=4, [12]=4 };
var cols = new VectorI(13){
    [0]=0, [1]=1, [2]=3,
    [3]=0, [4]=1, 
    [5]=2, [6]=3, [7]=4,
    [8]=0, [9]=2, [10]=3,
    [11]=1, [12]=4 };
var nzCOO = new VectorD(13){
    [0]=1.0, [1]=-1.0, [2]=-3.0,
    [3]=-2.0, [4]=5.0,
    [5]=4.0, [6]=6.0, [7]=4.0,
    [8]=-4.0, [9]=2.0, [10]=7.0,
    [11]=8.0, [12]=-5.0 };
var aiCOO = new MatrixDi(rows: 5, cols: 5, nnz: 13,
    nzInfo: new MDiCOOInfo(rows, cols, nzCOO));
Printer.Write($"ai[COO] status = {aiCOO.Status.ToString()}");
    
    
// dot test ...
// 1) in the CSR-format
Sparse.MV(aiCSR, x, ref yCSR);
Printer.Write("y[CSR] = ", yCSR);
// 2) in the CSC-format
Sparse.MV(aiCSC, x, ref yCSC);
Printer.Write("y[CSC] = ", yCSC);
// 3) in the COO-format
Sparse.MV(aiCOO, x, ref yCOO);
Printer.Write("y[COO] = ", yCOO);


// procedures
//Sparse.SetMVHint(ref aiCSR);
//Sparse.Optimize(ref aiCSR);
// ...
//Sparse.DestroyWMatrixDi(ref aiCSR);

// export ...
//var nzv = aiCSR.ExportCSRValues();
//Printer.Write(nzv);