//Generate grids
var dSampling = 240e-9;
var gridNumber = 101;
var gridSpacing = dSampling / (gridNumber - 1);
var gridv = new GridInfo1D(gridNumber, gridSpacing);
var gride = new GridInfo1D(gridNumber, gridSpacing);

//Incident EMfield
var wavelength = 13.5e-9;
var k0 = 2 * Math.PI / wavelength;
var polarization = InPlanePolMode.TM;
var window = 100e-9;
var smoothEdge = 4e-8;
var v = new ScalarField1D.PlaneWave(wavelength, Complex.One, window, gridv, smoothEdge, domain: ModelingDomain.Spatial);
VFrame.CreateShow(values: v.Field, grid: v.Grid,
    title: "Ex Input Spatial", xLabel: "x", yLabel: "Value");

//initialize coefficients
v.SwitchToKDomain();
var E = new VectorZ(v.Field);
var cLeft = new VectorZ(2 * E.Count, 0.0);
cLeft[new LongRange(0, E.Count)] = E;
var cRight = new VectorZ(2 * E.Count, 0.0);

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

//medium in the middle
Complex nCenter = 0.9385 + 0.03776 * Complex.ImaginaryOne;
//Complex nCenter = 1.20;
Complex nBoundary = 1.0;
double r = 25e-9;
long GRINratio = 1000;
Func<double, Layer1DMedium> epsilon = (z) =>
        StairFiber_1D(z, nCenter * nCenter, nBoundary * nBoundary, r, GRINratio);

//medium on the left and right
Complex nIn = 1.0;
Complex nOut = 1.0;
UniformLayer mRight = new UniformLayer(n: nIn);
UniformLayer mLeft = new UniformLayer(n: nOut);


//generate RK-BPM solver
var BPMsolverF = new RKBPM1D(RKspanF, epsilon, gridv, wavelength, 0.94 * k0, polarization, false, false);
var BPMsolverB = new RKBPM1D(RKspanB, epsilon, gridv, wavelength, 0.94 * k0, polarization, false, false);
Func<VectorZ, VectorZ> propagateF = (input) => BPMsolverF.PropagateIBVM(input); 
Func<VectorZ, VectorZ> propagateB = (input) => BPMsolverB.PropagateIBVM(input);
VectorZ medium = epsilon(0).Sample(0.0, BPMsolverF.GridX);
VFrame.CreateShow(values: medium, grid: gride,
    title: $"Medium refractive index", xLabel: "x", yLabel: "values");

//BPMsolver.Propagate(ref E, ref H);
long iterations = 12;
double mixingRatio = 0.75;
var IBVMsolver = new IBVA1D(mLeft, mRight, cLeft, cRight, propagateF, propagateB, true);
IBVMsolver.Iteration(wavelength, polarization, gridv, mixingRatio, iterations);


//show iteration
VectorZ Riteration = new VectorZ(iterations); 
VectorZ Titeration = new VectorZ(iterations);
VectorZ R = new VectorZ(gride.Count);
VectorZ T = new VectorZ(gride.Count);
for (int i = 0; i < iterations; i++)
{
   var g1 = new GridInfo1D(IBVMsolver.GridK);
   var g2 = new GridInfo1D(IBVMsolver.GridK);
   R = IBVMsolver.Rleft[i];
   T = IBVMsolver.Tright[i];
   Transform.FFT1D(ref R, ref g1, FTOption.Backward);
   Transform.FFT1D(ref T, ref g2, FTOption.Backward);
   var sum = R.Sum();
   Riteration[i] = sum / R.Count;
   sum = T.Sum();
   Titeration[i] = sum / T.Count;
}
var gridIteration = new GridInfo1D(iterations, 1, 1); 
VFrame.CreateShow(values: Riteration, grid: gridIteration,
    title: $"R iteration", xLabel: "iterations", yLabel: "values");
VFrame.CreateShow(values: Titeration, grid: gridIteration,
    title: $"T iteration", xLabel: "iterations", yLabel: "values");
    
//Compare with RCWA
var matFront = new FuncMaterial(nReal: nIn.Real, nImag: nIn.Imaginary);
var matBehind = new FuncMaterial(nReal: nOut.Real, nImag: nOut.Imaginary);
var solver = new RCWA1Dp(wavelength: wavelength,
    polarization: polarization,
    materialFront: matFront,
    mediumMiddle: epsilon(0),
    period: dSampling, thickness: grinThickness,
    materialBehind: matBehind);
long N = RCWAHelper.DetermineSampling(dSampling, gridSpacing);
solver.ComputeHalfSMatrix(kx0: 0.0, fieldsSampling: N, mediumSampling: N);
VectorZ t1 = LinAlg.Dot(solver.S11[0.0], v.Field);
VectorZ r1 = LinAlg.Dot(solver.S21[0.0], v.Field);
var g1 = new GridInfo1D(IBVMsolver.GridK);
var g2 = new GridInfo1D(IBVMsolver.GridK);
Transform.FFT1D(ref r1, ref g1, FTOption.Backward);
Transform.FFT1D(ref t1, ref g2, FTOption.Backward);

var dt = T - t1;
var dr = R - r1;
VFrame.CreateShow(values: dr, grid: gride,
    title: $"R deviation", xLabel: "x", yLabel: "values");
VFrame.CreateShow(values: dt, grid: gride,
    title: $"T deviation", xLabel: "x", yLabel: "values");

//Display Comparision
var rc = VFrame.CreateFrame();
var tc = VFrame.CreateFrame();
VFrame.AddToFrame(tc, t1, gride, plotColor: Options.PlotColor.Black, label: $"RCWA");
VFrame.AddToFrame(rc, r1, gride, plotColor: Options.PlotColor.Black, label: $"RCWA");
VFrame.AddToFrame(rc, R, gride, plotColor: Options.PlotColor.Blue, label: $"Mixing = {mixingRatio}");
VFrame.AddToFrame(tc, T, gride, plotColor: Options.PlotColor.Blue, label: $"Mixing = {mixingRatio}");
VFrame.SetLabelX(rc, "x");
VFrame.SetLabelY(rc, "Values");
VFrame.SetTitle(rc, $"Comparision of r with GRIN ratio{GRINratio}");
VFrame.SetLabelX(tc, "x");
VFrame.SetLabelY(tc, "Values");
VFrame.SetTitle(tc, $"Comparision of t with GRIN ratio{GRINratio}");
VFrame.RefreshShow(rc);
VFrame.RefreshShow(tc);


//Medium functions
public static Layer1DMedium StairFiber_1D(double z, Complex eCent, Complex eBoun, double r, long steps)
{
    Func<double, double, Complex> Epsilon = (a, x) =>
    {
        double p = r / steps;
        Complex grin;
        grin = eBoun;
        for (int i = 0; i < steps; i++)
        {
            if (Math.Abs(x) >= (i * p) & Math.Abs(x) < ((i + 1) * p))
            {
                grin = eCent - (eCent - eBoun) * Math.Pow((i * p), 2.0) / Math.Pow(r, 2.0);
            }
        }
        return grin;
    };
    var Medium = new Layer1DMedium(epsilon: Epsilon);
    return Medium;
}

