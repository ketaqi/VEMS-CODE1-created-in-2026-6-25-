// basic parameters
double wavelength = 633E-9;
double incAngle = 0.0; //55.60; // in degree
double nIn = 1.0;
double nOut = 1.52;
// grating parameters
var periodX = 380E-9;
var periodY = 380E-9;
var nEmbed = 1.0;
var nPillar = 2.30;
var pillarDiameter = 200E-9;
var thickness = 545E-9;
// numerical parameters
long N = 23; // ...


// ===== RCWA2D =====
var rSolver = new RCWA2D(wavelength: wavelength,
    nFront: (w, p) => nIn, pFront: null,
    nMiddle: (w, x, y, p) => GratingRIndex(x, y, p[0], p[1], p[2]),
    pMiddle: new List<double> { pillarDiameter, nEmbed, nPillar },
    periodX: periodX, periodY: periodY,
    thickness: thickness,
    nBehind: (w, p) => nOut, pBehind: null,
    nKxs: N, nKys: N,
    kx0: 0.0, ky0: 0.0,
    layerOverSampX: 1.0, layerOverSampY: 1.0,
    useWSVariation: false,
    saveLayerMediaData: true,
    saveLayerModesData: false);
rSolver.ComputeHalfSMatrix();
if(rSolver.LayerMiddle.EpsilonData != null){ Printer.Write("epsilon data not null"); }
// ===== end of RCWA2D ======


// define input field in k-domain
var Ex = new MatrixZ(N, N, 0.0);
Ex[(N-1)/2, (N-1)/2] = 1.0;
var Ey = new MatrixZ(N, N, 0.0);
(var tEx, var tEy) = rSolver.ComputeTCoefficients(Ex, Ey);
long midX = (N-1)/2;
long midY = (N-1)/2;
for(long ix = -1; ix <= 1; ix++)
{
    for(long iy = -1; iy <= 1; iy++)
    {
        var t = tEx[midY + iy, midX + ix];
        //Printer.Write($"t[{ix}, {iy}]: Abs = {t.Magnitude}; Arg = {t.Phase}"); 
    }
}

// transmitted field
Transform.FFT(tEx, FFTOption.Backward);
VFrame.CreateShow(tEx, title: "t-Ex(x, y)");



// ================================
// grating layer R-index definition
private static Complex GratingRIndex(double x, double y,
    double pillarDiameter,
    double nEmbed, double nPillar)
{
    double rho2 = x*x + y*y;
    double rho = Math.Sqrt(rho2);
    double radius = 0.5 * pillarDiameter; 
    if (rho <= radius)
        return new Complex(nPillar, 0.0);
    else
        return new Complex(nEmbed, 0.0);
}