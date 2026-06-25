int nCuda;
var status = CUDA.GetDeviceCount(ref nCuda);
Printer.Write($"number of cuda devices = {nCuda}");

long n = 3001; // 30001

Printer.Write($"... generating random matrix and vector ...");
var a = VStat.RngUniform(n, n);
var x = VStat.RngUniform(n);

Printer.Write($"... setting matrix to the cuda device ...");
var sw = Stopwatch.StartNew();
var da = new CudaMatrixD(a);
sw.Stop();
Printer.Write($"... matrix set to device time cost = {sw.ElapsedMilliseconds} [ms]");

Printer.Write($"... setting vector to the cuda device ...");
sw.Restart();
var dx = new CudaVectorD(x);
sw.Stop();
Printer.Write($"... vector set to device time cost = {sw.ElapsedMilliseconds} [ms]");

long gpuRuns = 3001;
for (long i = 0; i < gpuRuns; i++)
{
    GPUMVTestBench(da, dx);
}

var y = new VectorD(n);
Printer.Write($"... perform matrix-vector product on the host device ...");
sw.Restart();
LinAlg.Dot(a, x, ref y);
sw.Stop();
Printer.Write($"CPU MV-Dot time cost = {sw.ElapsedMilliseconds} [ms]");


private void GPUMVTestBench(CudaMatrixD a, CudaVectorD x)
{
    var dy = new CudaVectorD(n);
    Printer.Write($"... perform matrix-vector product on the cuda device ...");
    var timer = Stopwatch.StartNew();
    CudaLinAlg.Dot(da, dx, ref dy);
    timer.Stop();
    Printer.Write($"GPU MV-Dot time cost = {timer.ElapsedMilliseconds} [ms]");
}