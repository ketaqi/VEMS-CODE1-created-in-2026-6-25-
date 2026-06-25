int nCuda;
var status = CUDA.Runtime.GetDeviceCount(ref nCuda);
Printer.Write($"number of cuda devices = {nCuda}");

// host vector generation
var x = VStat.RngUniform(6);
Printer.Write($"host vector x: ", x);
Printer.Write($"ASum of x = {VMath.AbsSum(x)}");

// set vector to device
var dx = new CudaVectorD(x);
Printer.Write($"ASum of dx = {CudaVMath.AbsSum(dx)}");
// scale device vector
CudaVMath.ScaleOn(ref dx, 1.0);
// get scaled device vector back to host
var dxh = dx.GetValues();
Printer.Write($"device vector back on host: ", dxh);


// host matrix generation
var a = VStat.RngExponential(3, 3, 1.0, 0.2);
var b = VStat.RngUniform(3, 3, 0.0, 1.0);
Printer.Write($"host matrix a = ", a);
Printer.Write($"host matrix b = ", b);

// set matrix to device
var da = new CudaMatrixD(a);
var db = new CudaMatrixD(b);
// compute matrix-product on the device
var dc = new CudaMatrixD(3, 3);
CudaLinAlg.Dot(da, db, ref dc);
// get result device matrix back to host
var dch = dc.GetValues();
Printer.Write($"device matrix back on host: ", dch);

// reference matrix-product on the host
var c = LinAlg.Dot(a, b);
Printer.Write($"reference matrix-product on the host: ", c);