// Physical Parameters
double lambda = 633E-9;
double waistRadius = 3E-6;
double shift = 1E-6;
double Kx0 = 2 * Math.PI / lambda * Math.Sin(30.0 / 180.0 * Math.PI);
double q = 2.0 * Math.PI / lambda * 1.0 / (2 * 3.3E-5);

// Sampling Parameters
long N = 43;
double spacing = waistRadius / 3;
GridInfo1D grid = new GridInfo1D(N, spacing);

// define the source
SCField1D.Gaussian vIn = new SCField1D.Gaussian(lambda, new FuncMaterial(1.0),
                                            waistRadius, grid, shiftX: shift, shiftKx: Kx0);

// ---------Original Figure----------
VectorZ input4Display = new VectorZ(vIn.U.Values);
GridInfo1D grid4Display = new GridInfo1D(vIn.U.GridInfo);
AddQuadPhase(ref input4Display, grid4Display, q);
AddLinearPhase(ref input4Display, grid4Display, Kx0);

VFrame.CreateShow(new VectorZ(input4Display), grid4Display, title: "Original Data");

VFrame f = VFrame.CreateFrame();
// ----------FFT----------
// Create a copy for FFT
VectorZ input4FFT = new VectorZ(vIn.U.Values);
GridInfo1D grid4FFT = new GridInfo1D(vIn.U.GridInfo);
// FFT cannot deal with quadratic term analytically, so we perform a sample.
AddQuadPhase(ref input4FFT, grid4FFT, q);

// perform standard FFT
double c4FFT = Kx0;
Transform.FFT1D(ref input4FFT, ref grid4FFT, ref c4FFT, FFTOptions.Direction.Forward);
// For Display, we sample the analytical linear phase
AddLinearPhase(ref input4FFT, grid4FFT, c4FFT);
VFrame.AddToFrame(f, new VectorZ(input4FFT), grid4FFT,label: "FFT Forward", plotColor: Options.PlotColor.IndianRed, lineWidth: 8);


// ----------CFT----------
// Create a copy for CFT
VectorZ input4CFT = new VectorZ(vIn.U.Values);
GridInfo1D grid4CFT = new GridInfo1D(vIn.U.GridInfo);

// perform CFT
double c4CFT = Kx0;
double q4CFT = q;
Transform.CFT1D(ref input4CFT, ref grid4CFT, ref c4CFT, ref q4CFT, FFTOptions.Direction.Forward);

// For Display, we sample the analytical linear phase and quadratic phase
AddQuadPhase(ref input4CFT, grid4CFT, q4CFT);
AddLinearPhase(ref input4CFT, grid4CFT, c4CFT);
VFrame.AddToFrame(f, input4CFT, grid4CFT,label: "CFT Forward", plotColor: Options.PlotColor.Aqua);

VFrame.RefreshShow(f);





// Helper methods

public static void AddLinearPhase(ref VectorZ x, GridInfo1D grid,
    double a, LoopMode loopMode = Defaults.LoopOption)
{
    Samp1DRealFunc psi = new Samp1DRealFunc(f: (x) => Function1D.Linear(x, new List<double> {a}));
    psi.AddTo(x: ref x, grid: grid, part: ComplexPart.Argument, loopMode: loopMode);
}

public static void AddQuadPhase(ref VectorZ x, GridInfo1D grid,
    double a,
    Complex scalFac = default,
    LoopMode loopMode = Defaults.LoopOption)
    => AddPhase(v: ref x, grid: grid,
        f: Function1D.Quadratic, p: new List<double> { a },
        scalFac: scalFac, loopMode: loopMode);

public static void AddPhase(ref VectorZ v, GridInfo1D grid,
Func<double, List<double>, double> f, List<double> p,
Complex scalFac = default,
LoopMode loopMode = Defaults.LoopOption)
{
    // initializes scaling factor
    Complex a = scalFac == default ? Complex.One : scalFac;

    // define loop operation
    var t = v;
    void op(long i)
    {
        double x = grid[i];
        double psi = f(x, p);
        if (a == Complex.One)
        { t[i, false] *= Complex.Exp(Complex.ImaginaryOne * psi); }
        else
        { t[i, false] *= a * Complex.Exp(Complex.ImaginaryOne * psi); }
    }
    Loop1D loop = new Loop1D(operation: op,
                start: 0, end: t.Count,
                step: 1);
    loop.Evaluate(mode: loopMode);
}