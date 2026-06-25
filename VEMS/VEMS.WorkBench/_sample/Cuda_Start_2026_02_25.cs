// 1. Detect number of available GPUs
int nCuda = 0;
var status = CUDA.Runtime.GetDeviceCount(ref nCuda);
Printer.Write($"number of cuda devices = {nCuda}");

// 2. Print basic info for devices at indices 1 and 0
int devIndex1 = 0;
CUDA.Runtime.PrintDeviceBasicInfo(devIndex1);
int devIndex2 = 1;
CUDA.Runtime.PrintDeviceBasicInfo(devIndex2);

// 3. Host-side vector generation and summation
var x = VStat.RngUniform(10);
var xz = new VectorZ(x);
//Printer.Write("host vector x: ", x);
//Printer.Write($"Asum of x (CPU) = {VMath.Sum(x)}");

// 4. Transfer vector to GPU and compute absolute sum
var dxd = new CudaVectorD(x);
var dxz = new CudaVectorZ(xz);
var dxdV = dxd.GetValues();
var dxzV = dxz.GetValues();
//Printer.Write("device vector dxd: ", dxdV);
//Printer.Write("device vector dxz: ", dxzV);
//Printer.Write($"Asum of x (GPU) = {CudaVMath.AbsSum(dxd)}");
//Printer.Write($"Asum of x (GPU) = {CudaVMath.AbsSum(dxz)}");

// 5. Host-side matrix generation
var ad = VStat.RngExponential(3, 3, 1.0, 0.2);
var bd = VStat.RngExponential(3, 3, 0.0, 1.0);
var az = new MatrixZ(ad);
var bz = new MatrixZ(bd);
Printer.Write("host matrix a = ", az);
Printer.Write("host matrix b = ", bz);

// 6. Transfer matrices to GPU and retrieve result matrix from GPU
var azG = new CudaMatrixZ(az);
var bzG = new CudaMatrixZ(bz);
var azGV = azG.GetValues();
var bzGV = bzG.GetValues();
Printer.Write("device matrix back on host: ", azGV);
Printer.Write("device matrix back on host: ", bzGV);