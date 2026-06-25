//Generate grids
var dSampling = 60e-6;
var gridNumber = 6001;
var gridWidth = dSampling / (gridNumber - 1);
var gridv = new GridInfo1D(gridNumber, gridWidth);
var gride = new GridInfo1D(gridNumber, gridWidth);

//Incident EMfield
var wavelength = 532e-9;
var polarization = InPlanePolMode.TE;
var window = 41e-6;
var smoothEdge = 1e-6;
var v = new ScalarField1D.PlaneWave(wavelength, Complex.One, window, gridv, smoothEdge, domain: ModelingDomain.Spatial);
VFrame.CreateShow(values: v.Field, grid: v.Grid,
    title: "Ex Input Spatial", xLabel: "x", yLabel: "Value");

//switch v to k-domain and calculate H with polarization
v.SwitchToKDomain();
var H = GetH(v, polarization);
var E = new VectorZ(v.Field);

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
Func<double, Layer1DMedium> epsilon = (z) =>
        LunerburgLens_1D(z, 2.0, 1.0, 20e-6);


//generate RK-BPM solver
var BPMsolver = new RKBPM1D(RKspan, epsilon, gridv, wavelength, 1.18e7, polarization, saveFieldsInside: true);
BPMsolver.Propagate(ref E, ref H);

//show output
v.Field = E;
v.SwitchToXDomain();
VFrame.CreateShow(values: v.Field, grid: v.Grid,
    title: $"E output", xLabel: "x", yLabel: "values");
Printer.Write($"Total time: {BPMsolver.CalculatingTime.TotalSeconds}[s]; " + 
$"Convolution: {BPMsolver.ConvolutionTime.TotalSeconds}[s]; " +
$"Multiplication : {BPMsolver.MultiplicationTime.TotalSeconds}[s]; " +
$"Converting : {BPMsolver.ConvertingTime.TotalSeconds}[s]");

//Show propagatio
//to make sure the number of the matrix's elements is under 1e7 because of the limit of VFrame
int deSamplingX = 2;
int deSamplingZ = 2;
long zCount = (RKsteps - 1) / deSamplingZ;
long xCount = (gridv.Count - 1) / deSamplingX + 1;
double zSpacing = dRK * deSamplingZ;
double xSpacing = gridv.Spacing * deSamplingX;
var propagation = new Grid2DCplxData(xCount, zCount, gridv.Start, 0.0, xSpacing, zSpacing, 0.0);
double a = 0.0;
gride.GetConjugated(FTOption.Forward, ref a);
for (int i = 1; i <= zCount; i++)
{
    var e = new VectorZ(BPMsolver.FieldsInside[i * deSamplingZ - 1]);
    Transform.FFT1D(ref e, ref gride, FTOption.Backward);
    Action<long> Show = ix =>
    {
        propagation.Values[ix, (i - 1)] = e[ix * deSamplingX];
    };
    Loop1D loopS = new Loop1D(Show, 0, xCount);
    loopS.Evaluate(LoopMode.Parallel);
    Transform.FFT1D(ref e, ref gride, FTOption.Forward);
}
VFrame.CreateShow(values: propagation.Values, grid: propagation.GridInfo, ComplexPart.Magnitude, Options.PlotColormap.Magma,
    title: $"E propagation", xLabel: "z", yLabel: "x");


//Medium functions
public static Layer1DMedium LunerburgLens_1D(double z, Complex eCent, Complex eBoun, double r)
{
    Func<double, double, Complex> Epsilon = (wl, x) =>
    {
        Complex grin;
        if (Math.Pow(x, 2.0) + Math.Pow((z - r), 2.0) <= Math.Pow(r, 2.0))
        {
            //n = sqrt( n0^2 - (n0^2 - n1^2) * r^2 / R^2)
            grin = eCent - (eCent - eBoun) * (Math.Pow(x, 2.0) + Math.Pow((z - r), 2.0)) / Math.Pow(r, 2.0);
        }
        else
        {
            grin = eBoun;
        }
        return grin;
    };
    var Medium = new Layer1DMedium(epsilon: Epsilon);
    return Medium;
}

//Electric field to magnetic field
public static VectorZ GetH(ScalarField1D v, InPlanePolMode polarization)
{
    VectorZ H = new VectorZ(v.Field.Count, 0.0);
    switch (polarization)
    {
        case InPlanePolMode.TE:
            // H = epsilon * E / nz;
            H += v.Epsilon * v.Field / v.Nz;
            return H;

        case InPlanePolMode.TM:
            // H = -E / nz / mu;
            H -= v.Field / v.Nz / v.Mu;
            return H;
        default:
            throw new ArgumentException();

    }
}

