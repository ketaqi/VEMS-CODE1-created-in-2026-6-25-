int nCuda;
var status = CUDA.GetDeviceCount(ref nCuda);
Printer.Write($"number of cuda devices = {nCuda}");

long n = 5001;

Printer.Write($"... generating random matrices on the host ...");
var a = VStat.RngUniform(n, n, 0.0, 1.0);
var b = VStat.RngUniform(n, n, 0.1, 0.2);

Printer.Write($"... setting matrices to the cuda device ...");
var sw = Stopwatch.StartNew();
var da = new CudaMatrixD(a);
var db = new CudaMatrixD(b);
sw.Stop();
Printer.Write($"set matrix to device time cost = {sw.ElapsedMilliseconds} [ms]");

var gpuRuns = 11;
for(long i = 0; i < gpuRuns; i++)
{
    GPUMMTestBench(da, db);
}

var y = new MatrixD(n, n);
Printer.Write($"... perform matrix-matrix product on the host device ...");
sw.Restart();
LinAlg.Dot(a, b, ref y);
sw.Stop();
Printer.Write($"CPU MM-Dot time cost = {sw.ElapsedMilliseconds} [ms]");


private void GPUMMTestBench(CudaMatrixD a, CudaMatrixD b)
{
    var dy = new CudaMatrixD(n, n);
    //var sw = Stopwatch.StartNew();
    Printer.Write($"... perform matrix-matrix product on the cuda device ...");
    var timer = Stopwatch.StartNew();
    CudaLinAlg.Dot(da, db, ref dy);
    timer.Stop();
    Printer.Write($"GPU MM-Dot time cost = {timer.ElapsedMilliseconds} [ms]");
}