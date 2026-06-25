int nCuda;
var status = CUDA.GetDeviceCount(ref nCuda);
Printer.Write($"number of cuda devices = {nCuda}");

long n = 199999999;
var x = VStat.RngUniform(n);
var cx = new CudaVectorD(x);
var z = VMath.Construct(VStat.RngUniform(n, -1.0, 1.0), VStat.RngUniform(n, -0.1, 0.1));
var cz = new CudaVectorZ(z);

var sw = Stopwatch.StartNew();
var sGPU = cx.ASum();
sw.Stop();
Printer.Write($"Asum (GPU) = {sGPU}");
Printer.Write($"GPU time cost = {sw.ElapsedMilliseconds} [ms]");

sw.Restart();
var sCPU = VMath.AbsSum(x);
sw.Stop();
Printer.Write($"Asum (CPU) = {sCPU}");
Printer.Write($"CPU time cost = {sw.ElapsedMilliseconds} [ms]");

sw.Restart();
var szGPU = cz.ASum();
sw.Stop();
Printer.Write($"complex-Asum (GPU) = {szGPU}");
Printer.Write($"GPU time cost = {sw.ElapsedMilliseconds} [ms]");

sw.Restart();
var szCPU = VMath.AbsSum(z);
Printer.Write($"complex-Asum (CPU) = {szCPU}");
Printer.Write($"CPU time cost = {sw.ElapsedMilliseconds} [ms]");