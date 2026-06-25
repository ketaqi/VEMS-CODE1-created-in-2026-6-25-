// defines the unit test: choose from the standard tests below ...
//Func<long, TimeSpan> test = MatrixBench.RealMatrixProduct;
Func<long, TimeSpan> test = MatrixBench.CplxMatrixProduct;
//Func<long, TimeSpan> test = MatrixBench.RealLinearSystem;
//Func<long, TimeSpan> test = MatrixBench.CplxLinearSystem;
//Func<long, TimeSpan> test = MatrixBench.RealMatrixSVD;
//Func<long, TimeSpan> test = MatrixBench.CplxMatrixSVD;
//Func<long, TimeSpan> test = MatrixBench.RealMatrixEigen;
//Func<long, TimeSpan> test = MatrixBench.CplxMatrixEigen;
//Func<long, TimeSpan> test = MatrixBench.CplxMatrixFFT;

// constructs the standard benchmark
var bm = new MatrixBench(unitTest: test,
    runs: 3,
    matSizes: new List<long>{ 101, 201, 501, 1001, 2001, 5001 });
// and run it ...
bm.Run();