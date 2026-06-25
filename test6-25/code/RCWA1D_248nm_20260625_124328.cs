// RCWA 1D Grating - 248nm (KrF) - duty=30% thick=200nm
string ts = "20260625_124328";
var wl_nm = 248.0; var wl = wl_nm * 1E-9;
var pol = InPlanePolMode.TM; var nIn = 1.5593; var nOut = 1.0;
var nRidge = new Complex(1.3282, 1.6637); var nEmbed = 1.0;
var period = 20.0E-6; var rw = 6.0E-6; var thick = 0.2E-6;

VectorD spans = new VectorD(4, 0.0); spans[0]=-period/2; spans[1]=-rw/2; spans[2]=rw/2; spans[3]=period/2;
VectorZ vals = new VectorZ(3, 0.0); vals[0]=nEmbed*nEmbed; vals[1]=nRidge*nRidge; vals[2]=nEmbed*nEmbed;

var solver = new RCWA1Dp(wavelength: wl, polarization: pol,
    materialFront: new FuncMaterial(nReal: nIn),
    mediumMiddle: new Layer1DPwctMedium(new Pwct1DCplxData(spans, vals)),
    period: period, thickness: thick, materialBehind: new FuncMaterial(nReal: nOut));
solver.ComputeHalfSMatrix(kx0: 0);
(var t, var gT) = solver.ComputeTCoefficients(pw: new PlaneWaveXZ(wavelength: wl, n: nIn, kx: 0, polMode: pol));
Transform.FFT1D(x: ref t, grid: ref gT, direction: FFTOptions.Direction.Backward);

var tMag = VMath.Abs(t);
VFrame.CreateShow(values: tMag, grid: gT,
    title: "RCWA |E| (TM, 248nm, duty=30%)",
    xLabel: "kx", yLabel: "|E|", plotColor: Options.PlotColor.DarkGreen);
