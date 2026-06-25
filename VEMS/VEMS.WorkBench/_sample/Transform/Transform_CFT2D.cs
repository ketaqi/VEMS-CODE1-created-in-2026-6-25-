// Physical Parameters
double lambda = 633E-9;
double waistRadiusX = 1E-6;
double waistRadiusY = 2E-6;

double q = 10.0 * Math.PI / lambda / (4 * 3.3E-5);
double w = 4E11;
double p = -10.0 * Math.PI / lambda / (4 * 3.3E-5);

double shiftX = 1E-6;
double shiftY = 0.0;
double shiftKx = 0.0;
double shiftKy = 2 * Math.PI / lambda * Math.Sin(20.0 / 180.0 * Math.PI);


// Sampling Parameters
long N = 101;
double spacing = 0.3E-6;

GridInfo2D grid = new GridInfo2D(N, N, spacing, spacing);

// define the source
SCField.Gaussian vIn = new SCField.Gaussian(lambda, new FuncMaterial(1.0), 
    waistRadiusX, waistRadiusY,
    grid, shiftX: shiftX, shiftKy: shiftKy);

// ---------Original Figure----------
MatrixZ input4Display = new MatrixZ(vIn.U.Values);
GridInfo2D grid4Display = new GridInfo2D(vIn.U.GridInfo);
AddLinearPhase(ref input4Display, grid4Display, shiftKx, shiftKy);
AddQuadPhase(ref input4Display, grid4Display, q, p);
AddCrossPhase(ref input4Display, grid4Display, w);

VFrame.CreateShow(new MatrixZ(input4Display), grid4Display, title: "Original Data");


// ------- FFT -------
MatrixZ input4FFT = new MatrixZ(vIn.U.Values);
GridInfo2D grid4FFT = new GridInfo2D(vIn.U.GridInfo);

// Sample Quadratic phase
AddQuadPhase(ref input4FFT, grid4FFT, q, p);
AddCrossPhase(ref input4FFT, grid4FFT, w);

// perform standard FFT
double c1x4FFT = shiftKx;
double c1y4FFT = shiftKy;
Transform.FFT2D(ref input4FFT, ref grid4FFT, 
    ref c1x4FFT, ref c1y4FFT, FFTOptions.Direction.Forward);

// For Display, we sample the analytical linear phase
AddLinearPhase(ref input4FFT, grid4FFT, c1x4FFT, c1y4FFT);
VFrame.CreateShow(new MatrixZ(input4FFT), grid4FFT, title: "FFT result");


// ------- CFT -------
// Create a copy for CFT
MatrixZ input4CFT = new MatrixZ(vIn.U.Values);
GridInfo2D grid4CFT = new GridInfo2D(vIn.U.GridInfo);

// perform CFT
Transform.CFT2D(ref input4CFT, ref grid4CFT, ref shiftKx, ref shiftKy, ref q, ref w, ref p);

//Transform.CFT2D(ref input4CFT, ref grid4CFT, 
//    ref shiftKx, ref shiftKy, ref q, ref w, ref p, FFTOptions.Direction.Backward);

// For Display, we sample all the phase
AddLinearPhase(ref input4CFT, grid4CFT, shiftKx, shiftKy);
AddQuadPhase(ref input4CFT, grid4CFT, q, p);
AddCrossPhase(ref input4CFT, grid4CFT, w);


VFrame.CreateShow(new MatrixZ(input4CFT), grid4CFT, title: "CFT result");


// Calculate standard deviation
Grid2DCplxData GridData4FFT = new Grid2DCplxData(input4FFT, grid4FFT, intrpl: InterpolationMethod.Cubic);
MatrixZ interp = GridData4FFT.FindValues(targetGrid: grid4CFT);
Printer.WriteLine($"Standard deviation: {VMath.StandardDeviation(interp, input4CFT)}");


// ------- static helper methods -------
public static double CrossTerm(double x,double y,double w)
{
    return w * x * y;
}

public static void AddCrossPhase(ref MatrixZ x, GridInfo2D grid,
    double w,
    Complex scalFac = default,
    LoopMode loopMode = Defaults.LoopOption)
    => AddPhase(ref x, grid, (x, y, w) => CrossTerm(x, y, w[0]), new List<double>{w});

public static void AddLinearPhase(ref MatrixZ x, GridInfo2D grid,
    double c1x, double c1y,
    Complex scalFac = default,
    LoopMode loopMode = Defaults.LoopOption)
    => AddPhase(v: ref x, grid: grid,
        f: Function2D.Linear, p: new List<double> {c1x, c1y},
        scalFac: scalFac, loopMode: loopMode);

public static void AddQuadPhase(ref MatrixZ x, GridInfo2D grid,
    double c2x, double c2y,
    Complex scalFac = default,
    LoopMode loopMode = Defaults.LoopOption)
    => AddPhase(v: ref x, grid: grid,
        f: Function2D.Quadratic, p: new List<double> {c2x, c2y},
        scalFac: scalFac, loopMode: loopMode);

public static void AddPhase(ref MatrixZ v, GridInfo2D grid,
    Func<double, double, List<double>, double> f, List<double> p,
    Complex scalFac = default,
    LoopMode loopMode = Defaults.LoopOption)
{
    // initializes scaling factor
    Complex a = scalFac == default ? Complex.One : scalFac;

    // define loop operation
    var t = v;
    void op(long iRow, long iCol)
    {
        (double y, double x) = grid[iRow, iCol];
        double psi = f(x, y, p);
        if (a == Complex.One)
        { t[iRow, iCol, false] *= Complex.Exp(Complex.ImaginaryOne * psi); }
        else
        { t[iRow, iCol, false] *= a * Complex.Exp(Complex.ImaginaryOne * psi); }
    }
    Loop2D loop = new Loop2D(operation: op,
        rowStart: 0, rowEnd: grid.Rows,
        colStart: 0, colEnd: grid.Cols,
        rowStep: 1, colStep: 1);
    loop.Evaluate(mode: loopMode);
}