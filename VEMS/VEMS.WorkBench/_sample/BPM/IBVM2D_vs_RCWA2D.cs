//Generate grids
var dSampling = 240e-9;
var gridNumber = 101;
var gridSpacing = dSampling / (gridNumber - 1);
var gridvx = new GridInfo2D(gridNumber, gridNumber, gridSpacing, gridSpacing);
var gridvy = new GridInfo2D(gridNumber, gridNumber, gridSpacing, gridSpacing);
var gridm = new GridInfo2D(gridNumber, gridNumber, gridSpacing, gridSpacing);

//Incident EMfield
var wavelength = 13.5e-9;
var k0 = 2 * Math.PI / wavelength;
var windowExa = 100e-9;
var windowExb = 100e-9;
var windowEya = 100e-9;
var windowEyb = 100e-9;
var smoothEdgex = 4e-8;
var smoothEdgey = 4e-8;
var ex = new ScalarField.PlaneWave(wavelength, 1.0, windowExa, windowExb, gridvx, ApertureShape.Rectangular, edge: smoothEdgex);
var ey = new ScalarField.PlaneWave(wavelength, 1.0, windowEya, windowEyb, gridvy, ApertureShape.Rectangular, edge: smoothEdgey);
VFrame.CreateShow(values: ex.Field, grid: ex.Grid, colormap: Options.PlotColormap.Magma,
    title: "Ex Input Spatial", xLabel: "x", yLabel: "Value");
VFrame.CreateShow(values: ey.Field, grid: ey.Grid, colormap: Options.PlotColormap.Magma,
    title: "Ey Input Spatial", xLabel: "x", yLabel: "Value");

//switch v to k-domain and calculate H with polarization
ex.SwitchToKDomain();
ey.SwitchToKDomain();
MatrixZ cpx = new MatrixZ(ex.Field);
MatrixZ cpy = new MatrixZ(ey.Field);
MatrixZ cLeft = new MatrixZ(4 * cpx.Rows, cpx.Cols, 0.0);
MatrixZ cRight = new MatrixZ(4 * cpx.Rows, cpx.Cols, 0.0);


cLeft[new LongRange(0, cpx.Rows), new LongRange(0, cpx.Cols)] = cpx;
cLeft[new LongRange(cpy.Rows, 2 * cpy.Rows), new LongRange(0, cpy.Cols)] = cpy;

//generate a systematic sampling zSpan for RK iterations 
var grinThickness = 100e-9;
var RKsteps = 201;
var dRK = grinThickness / (RKsteps - 1);
//Forward RK Span
var RKspanF = new VectorD(RKsteps);
//Backward RK Span
var RKspanB = new VectorD(RKsteps);
for (int i = 0; i < RKsteps; i++)
{
    RKspanF[i] = i * dRK;
    RKspanB[RKsteps - i - 1] = i * dRK;
}


//medium
Complex n0 = 1.0;
Complex n1 = 1.20;
//Complex n1 = 0.9385 + 0.03776 * Complex.ImaginaryOne;
double rx = 25e-9;
double ry = 25e-9;
Func<double, Layer2DMedium> n = (z) =>
        FiberXY_2D(z, n1, n0, rx, ry);


long GRINratio = 1;
//Func<double, Layer2DMedium> n = (z) =>
//        StepFiber_2D_Circle(z, n1, n0, r, GRINratio);

//double dArray = 30e-9;
//GridInfo2D gridArray = new GridInfo2D(4, 4, dArray, dArray);
//double rx = 6e-9;
//double ry = 6e-9;
//Func<double, Layer2DMedium> n = (z) =>
//        Square_Array_2D(z, n1, n0, gridArray, rx, ry);

//medium on the left and right
Complex nIn = 1.0;
Complex nOut = 1.0;
UniformLayer mRight = new UniformLayer(n: nIn);
UniformLayer mLeft = new UniformLayer(n: nOut);


//generate RK-BPM solver
var BPMsolverF = new RKBPM2D(RKspanF, n, gridvx, wavelength, 0.99 * k0, 0.99 * k0, showProgress: false, saveEyInside: false, saveExInside: false);
var BPMsolverB = new RKBPM2D(RKspanB, n, gridvx, wavelength, 0.99 * k0, 0.99 * k0, showProgress: false, saveEyInside: false, saveExInside: false);
Func<MatrixZ, MatrixZ> propagateF = (input) => BPMsolverF.PropagateIBVM(input);
Func<MatrixZ, MatrixZ> propagateB = (input) => BPMsolverB.PropagateIBVM(input);
MatrixZ medium = n(0).Sample(0.0, BPMsolverF.GridX);
VFrame.CreateShow(values: medium, grid: gridm, colormap: Options.PlotColormap.Magma,
    title: $"Medium refractive index", xLabel: "x", yLabel: "y");

//Generate IBVM silver
long iterations = 12;
double mixingRatio = 0.75;
var IBVMsolver = new IBVA2D(mLeft, mRight, cLeft, cRight, propagateF, propagateB, true);
IBVMsolver.Iteration(wavelength, gridvx, mixingRatio, iterations);

//show iteration and R and T
VectorZ Riterationx = new VectorZ(iterations);
VectorZ Riterationy = new VectorZ(iterations);
VectorZ Titerationx = new VectorZ(iterations);
VectorZ Titerationy = new VectorZ(iterations);
MatrixZ Rx = new MatrixZ(gridm.Rows, gridm.Cols);
MatrixZ Ry = new MatrixZ(gridm.Rows, gridm.Cols);
MatrixZ Tx = new MatrixZ(gridm.Rows, gridm.Cols);
MatrixZ Ty = new MatrixZ(gridm.Rows, gridm.Cols);
for (int i = 0; i < iterations; i++)
{
    var g1 = new GridInfo2D(IBVMsolver.GridK);
    var g2 = new GridInfo2D(IBVMsolver.GridK);
    var g3 = new GridInfo2D(IBVMsolver.GridK);
    var g4 = new GridInfo2D(IBVMsolver.GridK);
    Rx = IBVMsolver.RleftX[i];
    Ry = IBVMsolver.RleftY[i];
    Tx = IBVMsolver.TrightX[i];
    Ty = IBVMsolver.TrightY[i];
    Transform.FFT2D(ref Rx, ref g1, FTOption.Backward);
    Transform.FFT2D(ref Ty, ref g2, FTOption.Backward);
    Transform.FFT2D(ref Ry, ref g3, FTOption.Backward);
    Transform.FFT2D(ref Tx, ref g4, FTOption.Backward);
    var sum = Rx.Sum();
    Riterationx[i] = sum / Rx.Count;
    sum = Tx.Sum();
    Titerationx[i] = sum / Tx.Count;
    sum = Ry.Sum();
    Riterationy[i] = sum / Ry.Count;
    sum = Ty.Sum();
    Titerationy[i] = sum / Ty.Count;
}
Printer.Write("IBVM Finished");
var gridIteration = new GridInfo1D(iterations, 1, 1);
VFrame.CreateShow(values: Riterationx, grid: gridIteration,
    title: $"Rx iteration", xLabel: "iterations", yLabel: "values");
VFrame.CreateShow(values: Titerationx, grid: gridIteration,
    title: $"Tx iteration", xLabel: "iterations", yLabel: "values");
VFrame.CreateShow(values: Riterationy, grid: gridIteration,
    title: $"Ry iteration", xLabel: "iterations", yLabel: "values");
VFrame.CreateShow(values: Titerationy, grid: gridIteration,
    title: $"Ty iteration", xLabel: "iterations", yLabel: "values");
VFrame.CreateShow(values: Rx, grid: gridm, colormap: Options.PlotColormap.Magma,
    title: $"Rx by IBVM", xLabel: "x", yLabel: "y");
VFrame.CreateShow(values: Tx, grid: gridm, colormap: Options.PlotColormap.Magma,
    title: $"Tx by IBVM", xLabel: "x", yLabel: "y");
VFrame.CreateShow(values: Ry, grid: gridm, colormap: Options.PlotColormap.Magma,
    title: $"Ry by IBVM", xLabel: "x", yLabel: "y");
VFrame.CreateShow(values: Ty, grid: gridm, colormap: Options.PlotColormap.Magma,
    title: $"Ty by IBVM", xLabel: "x", yLabel: "y");

// ===== RCWA2D =====
var mask = n(0);
var matFront = new FuncMaterial(nReal: nIn.Real);
var matBehind = new FuncMaterial(nReal: nOut.Real);
var thickness = 100E-9;
var RCWASolver = new RCWA2D(wavelength: wavelength,
    materialFront: matFront,
    mediumMiddle: mask,
    periodX: dSampling, periodY: dSampling, thickness: thickness,
    materialBehind: matBehind);

RCWASolver.ComputeHalfSMatrix(kx0: 0.0, fieldsSamplingX: gridNumber, fieldsSamplingY: gridNumber,
    mediumSamplingX: 2 * gridNumber + 1, mediumSamplingY: 2 * gridNumber + 1);
// ===== end of RCWA2D ======
var cIn = RCWAHelper.Convert2X2DFieldTo1D(cpx, cpy);
VectorZ T = LinAlg.Dot(RCWASolver.S11, cIn);
VectorZ R = LinAlg.Dot(RCWASolver.S21, cIn);
MatrixZ TEx, TEy, REx, REy;
(TEx, TEy) = RCWAHelper.Convert1DFieldTo2X2D(v: T, nRow: gridNumber, nCol: gridNumber);
(REx, REy) = RCWAHelper.Convert1DFieldTo2X2D(v: R, nRow: gridNumber, nCol: gridNumber);
var g1 = new GridInfo2D(IBVMsolver.GridK);
var g2 = new GridInfo2D(IBVMsolver.GridK);
var g3 = new GridInfo2D(IBVMsolver.GridK);
var g4 = new GridInfo2D(IBVMsolver.GridK);
Transform.FFT2D(ref REx, ref g1, FTOption.Backward);
Transform.FFT2D(ref TEy, ref g2, FTOption.Backward);
Transform.FFT2D(ref REy, ref g3, FTOption.Backward);
Transform.FFT2D(ref TEx, ref g4, FTOption.Backward);
Printer.Write("RCWA Finished");

VFrame.CreateShow(values: REx, grid: gridm, colormap: Options.PlotColormap.Magma,
    title: $"Rx by RCWA", xLabel: "x", yLabel: "y");
VFrame.CreateShow(values: TEx, grid: gridm, colormap: Options.PlotColormap.Magma,
    title: $"Tx by RCWA", xLabel: "x", yLabel: "y");
VFrame.CreateShow(values: REy, grid: gridm, colormap: Options.PlotColormap.Magma,
    title: $"Ry by RCWA", xLabel: "x", yLabel: "y");
VFrame.CreateShow(values: TEy, grid: gridm, colormap: Options.PlotColormap.Magma,
    title: $"Ty by RCWA", xLabel: "x", yLabel: "y");
var DRx = Rx - REx;
var DRy = Ry - REy;
var DTx = Tx - TEx;
var DTy = Ty - TEy;
VFrame.CreateShow(values: DRx, grid: gridm, colormap: Options.PlotColormap.Magma,
    title: $"Deviation of Rx", xLabel: "x", yLabel: "y");
VFrame.CreateShow(values: DTx, grid: gridm, colormap: Options.PlotColormap.Magma,
    title: $"Deviation of Tx", xLabel: "x", yLabel: "y");
VFrame.CreateShow(values: DRy, grid: gridm, colormap: Options.PlotColormap.Magma,
    title: $"Deviation of Ry", xLabel: "x", yLabel: "y");
VFrame.CreateShow(values: DTy, grid: gridm, colormap: Options.PlotColormap.Magma,
    title: $"Deviation of Ty", xLabel: "x", yLabel: "y");

//Medium Functions
public static Layer2DMedium FiberXY_2D(double z, Complex nCenter, Complex nBoundary, double rx, double ry)
{
    Func<double, double, double, Complex> N = (wl, x, y) =>
       {
           Complex grin;
           grin = nBoundary;
           if (Math.Abs(x) <= rx && Math.Abs(y) <= ry)
           {
               grin = nCenter;
           }
           return grin;
       };
    var Medium = new Layer2DMedium(N);
    return Medium;
}

public static Layer2DMedium StepFiber_2D_Square(double z, Complex nCenter, Complex nBoundary, double r, long steps)
{
    Func<double, double, double, Complex> N = (wl, x, y) =>
       {
           double p = r / steps;
           Complex grin;
           grin = nBoundary;
           for (int i = 0; i < steps; i++)
           {
               if (Math.Abs(x) >= (i * p) && Math.Abs(y) >= (i * p) && Math.Abs(x) < ((i + 1) * p) && Math.Abs(y) < ((i + 1) * p))
               {
                   grin = Complex.Sqrt(nCenter * nCenter - (nCenter * nCenter - nBoundary * nBoundary) * Math.Pow((i * p), 2.0) / Math.Pow(r, 2.0));
               }
           }
           return grin;
       };
    var Medium = new Layer2DMedium(N);
    return Medium;
}



public static Layer2DMedium StepFiber_2D_Circle(double z, Complex nCenter, Complex nBoundary, double r, long steps)
{
    Func<double, double, double, Complex> N = (wl, x, y) =>
       {
           double p = r / steps;
           Complex grin;
           grin = nBoundary;
           for (int i = 0; i < steps; i++)
           {
               if (Math.Pow(x, 2.0) + Math.Pow(y, 2.0) >= Math.Pow((i * p), 2.0) && Math.Pow(x, 2.0) + Math.Pow(y, 2.0) < Math.Pow(((i + 1) * p), 2.0))
               {
                   grin = Complex.Sqrt(nCenter * nCenter - (nCenter * nCenter - nBoundary * nBoundary) * Math.Pow((i * p), 2.0) / Math.Pow(r, 2.0));
               }
           }
           return grin;
       };
    var Medium = new Layer2DMedium(N);
    return Medium;
}


public static Layer2DMedium Square_Array_2D(double z, Complex nCenter, Complex nBoundary, GridInfo2D grid, double rx, double ry)
{
    Func<double, double, double, Complex> N = (wl, x, y) =>
       {
           Complex grin = nBoundary;
           for (int ix = 0; ix < grid.Cols; ix++)
           {
               double x0 = grid.GetCoordinateX(ix);
               for (int iy = 0; iy < grid.Rows; iy++)
               {
                   double y0 = grid.GetCoordinateY(iy);
                   if (Math.Abs(x - x0) <= Math.Abs(rx) && Math.Abs(y - y0) <= Math.Abs(ry))
                   {
                       grin = nCenter;
                   }
               }
           }
           return grin;
       };
    var Medium = new Layer2DMedium(N);
    return Medium;
}