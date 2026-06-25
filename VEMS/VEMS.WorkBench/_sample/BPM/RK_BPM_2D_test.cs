//Generate grids
var dSampling = 60e-6;
var gridNumber = 6001;
var axis = (gridNumber - 1) / 2;
var gridSpacing = dSampling / (gridNumber - 1);
var gridvx = new GridInfo2D(gridNumber, gridNumber, gridSpacing, gridSpacing);
var gridvy = new GridInfo2D(gridNumber, gridNumber, gridSpacing, gridSpacing);
var gridm = new GridInfo2D(gridNumber, gridNumber, gridSpacing, gridSpacing);

//Incident EMfield
var wavelength = 532e-9;
var windowEx = 41E-6;
var windowEy = 0.0;
var smoothEdgex = 2E-6;
var smoothEdgey = 0.0;
var ex = new ScalarField.PlaneWave(wavelength, 1.0, windowEx, windowEx, gridvx, ApertureShape.Elliptical, edge: smoothEdgex);
var ey = new ScalarField.PlaneWave(wavelength, 1.0, windowEy, windowEy, gridvy, ApertureShape.Elliptical, edge: smoothEdgey);
VFrame.CreateShow(values: ex.Field, grid: ex.Grid, colormap: Options.PlotColormap.Magma,
    title: "Ex Input Spatial", xLabel: "x", yLabel: "Value");
VFrame.CreateShow(values: ey.Field, grid: ey.Grid, colormap: Options.PlotColormap.Magma,
    title: "Ey Input Spatial", xLabel: "x", yLabel: "Value");

//switch v to k-domain and calculate H with polarization
ex.SwitchToKDomain();
ey.SwitchToKDomain();
(var Hx, var Hy) = GetH(ex, ey);
MatrixZ Ex = new MatrixZ(ex.Field);
MatrixZ Ey = new MatrixZ(ey.Field);


//generate a systematic sampling zSpan for RK iterations 
var grinThickness = 40e-6;
var RKsteps = 4001;
var dRK = grinThickness / (RKsteps - 1);
var RKspan = new VectorD(RKsteps);
for (int i = 0; i < RKsteps; i++)
{
    RKspan[i] = i * dRK;
}

//medium
Complex n0 = 1.0;
Complex n1 = Complex.Sqrt(2.0);
double r = 20e-6;
Func<double, Layer2DMedium> n = (z) =>
        LunerburgLens_2D(z, n1, n0, r);


//generate RK-BPM solver
var BPMsolver = new RKBPM2D(RKspan, n, gridvx, wavelength, 1.18e7, 1.18e7, showProgress: true, saveEyInside: false, saveExInside: false);
BPMsolver.Propagate(ref Ex, ref Ey, ref Hx, ref Hy);

//show output
ex.Field = Ex;
ey.Field = Ey;
VFrame.CreateShow(values: Ex, grid: ex.Grid, colormap: Options.PlotColormap.Magma,
    title: $"Ex output SF", xLabel: "x", yLabel: "values");
VFrame.CreateShow(values: Ey, grid: ey.Grid, colormap: Options.PlotColormap.Magma,
    title: $"Ey output SF", xLabel: "x", yLabel: "values");
ex.SwitchToXDomain();
ey.SwitchToXDomain();
VFrame.CreateShow(values: ex.Field, grid: ex.Grid, colormap: Options.PlotColormap.Magma,
    title: $"Ex output", xLabel: "x", yLabel: "values");
VFrame.CreateShow(values: ey.Field, grid: ey.Grid, colormap: Options.PlotColormap.Magma,
    title: $"Ey output", xLabel: "x", yLabel: "values");
Printer.Write($"Total time: {BPMsolver.CalculatingTime.TotalSeconds}[s]; " + 
$"Convolution: {BPMsolver.ConvolutionTime.TotalSeconds}[s]; " +
$"Multiplication : {BPMsolver.MultiplicationTime.TotalSeconds}[s]; " +
$"Converting : {BPMsolver.ConvertingTime.TotalSeconds}[s]");

////Show propagation
////to make sure the number of the matrix's elements is under 1e7 because of the limit of VFrame
//int deSamplingX = 1;
//int deSamplingZ = 2;
//long zCount = (RKsteps - 1) / deSamplingZ;
//long xCount = (gridNumber - 1) / deSamplingX + 1;
//double zSpacing = dRK * deSamplingZ;
//double xSpacing = gridSpacing * deSamplingX;
//var propagation = new Grid2DCplxData(xCount, zCount, -dSampling / 2, 0.0, xSpacing, zSpacing, 0.0);
//double a = 0.0;
//gridm.GetConjugated(FTOption.Forward, ref a, ref a);
//for (int i = 1; i <= zCount; i++)
//{
//    var e = new MatrixZ(BPMsolver.ExInside[i * deSamplingZ - 1]);
//    Transform.FFT2D(ref e, ref gridm, FTOption.Backward);
//    Parallel.For(0, zCount, i =>
//    {
//        Parallel.For(0, xCount, j =>
//        {
//            propagation.Values[j, i] = e[j * deSamplingX, axis];
//        });
//    });

//    Transform.FFT2D(ref e, ref gridm, FTOption.Forward);
//}
//VFrame.CreateShow(values: propagation.Values, grid: propagation.GridInfo, ComplexPart.Magnitude, Options.PlotColormap.Magma,
//    title: $"E propagation", xLabel: "z", yLabel: "x");




//Medium functions
public static Layer2DMedium LunerburgLens_2D(double z, Complex nCent, Complex nBoun, double r)
{
    Func<double, double, double, Complex> N = (wl, x, y) =>
    {
        Complex grin;
        if (Math.Pow(x, 2.0) + Math.Pow(y, 2.0) + Math.Pow((z - r), 2.0) <= Math.Pow(r, 2.0))
        {
            //n = sqrt( n0^2 - (n0^2 - n1^2) * r^2 / R^2)
            grin = Complex.Sqrt((nCent * nCent) - (nCent * nCent - nBoun * nBoun) * (Math.Pow(x, 2.0) + Math.Pow(y, 2.0) + Math.Pow((z - r), 2.0)) / Math.Pow(r, 2.0));
        }
        else
        {
            grin = nBoun;
        }
        return grin;
    };
    var Medium = new Layer2DMedium(N);
    return Medium;
}

//Electric field to magnetic field
public static (MatrixZ Hx, MatrixZ Hy) GetH(ScalarField Ex, ScalarField Ey)
{

    MatrixZ Hx = new MatrixZ(Ey.Field);
    MatrixZ Hy = new MatrixZ(Ex.Field);
    Parallel.For(0, Ex.Field.Cols, ix =>
    {
        Parallel.For(0, Ex.Field.Rows, iy =>
        {
            Hx[ix, iy] = (-Ex.Nx[ix] * Ex.Ny[iy] / Ex.Nz[ix, iy]) * Ex.Field[ix, iy] + (-Ex.Nz[ix, iy] - Ex.Ny[iy] * Ex.Ny[iy] / Ex.Nz[ix, iy]) * Ey.Field[ix, iy];
            Hy[ix, iy] = (Ex.Nz[ix, iy] + Ex.Nx[ix] * Ex.Nx[ix] / Ex.Nz[ix, iy]) * Ex.Field[ix, iy] + (Ex.Nx[ix] * Ex.Ny[iy] / Ex.Nz[ix, iy]) * Ey.Field[ix, iy];
        });
    });
    return (Hx, Hy);
}
