int nCuda = CudaHelper.GetDeviceCount();
Printer.Write($"number of cuda devices = {nCuda}");


int devIndex = CudaHelper.GetDevice();
Printer.Write($"device index = {devIndex}");
string? devName = CudaHelper.GetDeviceName(devIndex);
Printer.Write($"device name = {devName}");
long devMemoryMB = CudaHelper.GetDeviceMemMB(devIndex);
Printer.Write($"device memory = {devMemoryMB} [MB]");
int devMultiprocessors = CudaHelper.GetDeviceMultiprocessors(devIndex);
Printer.Write($"device multiprocessors = {devMultiprocessors}");
int devCUDACores = CudaHelper.GetDeviceCUDACores(devIndex);
Printer.Write($"device CUDA cores = {devCUDACores}");
int devClockRate = CudaHelper.GetDeviceClockRate(devIndex);
Printer.Write($"device clock rate = {devClockRate} [kHz]");
int devMemRate = CudaHelper.GetDeviceMemClockRate(devIndex);
Printer.Write($"device memory rate = {devMemRate} [kHz]");
int devSDRatio = CudaHelper.GetDeviceSingleDoubleRatio(devIndex);
Printer.Write($"device Single-Double ratio = {devSDRatio}");


var cuHandle = CudaHelper.CreateCuBLAS(); 
var version = CudaHelper.GetCuBLASVersion(cuHandle); 
Printer.Write($"cuBLAS version = {version}");