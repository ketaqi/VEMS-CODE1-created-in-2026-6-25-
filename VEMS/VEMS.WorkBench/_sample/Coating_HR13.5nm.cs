// parameters
var wavelength = 13.5E-9; // wavelength in vacuum
var polar = InPlanePolMode.TE; // set the polarization mode
var angleStart = 0.0; // in degree
var angleEnd = 20.0; // in degree
var nAngle = 181;
//var k0 = 2.0 * Math.PI / wavelength;
Func<double, Complex> nFront = (wl) => 1.0;
Func<double, Complex> nBehind = (wl) => 1.5438;
Func<double, Complex> nMo = (wl) => 0.92318704 + Complex.ImaginaryOne * 0.0065100849; 
Func<double, Complex> nSi = (wl) => 0.99873895 + Complex.ImaginaryOne * 0.0018278216;

// constructs multilayer
var period = 6.97E-9;
var gamma = 0.4;
var tSi = period * (1.0 - gamma);
var tMo = period * gamma;
var coating = new MultiLayer();
coating.AddAlteringLayers(nSi, tSi, nMo, tMo, 50);

// reflectance
var dAngle = (angleEnd - angleStart) / (nAngle - 1);
var reflectance = new VectorZ(nAngle);
// naive loop
for(long i = 0; i < nAngle; i++)
{
    var alpha = Converter.Degree2Radian(angleStart + i * dAngle);
    var kx = 2.0 * Math.PI / wavelength * nFront(wavelength).Real * Math.Sin(alpha);
    (_, Complex s21) = coating.ComputeHalfSMatrix(wavelength, nFront, nBehind, kx, polar);
    double r = MultiLayer.ComputePowerRatio(wavelength, nFront, nFront, kx, s21, polar, false);
    reflectance[i, false] = r;
}
// result display
VFrame.CreateShow(values: reflectance * 100.0, grid: new GridInfo1D(nAngle, angleStart, dAngle),
    xLabel: "angle [degree]", yLabel: "reflectance [%]", label: "R(alpha)");
 

// additional ... 
// lateral shift effects
var aStart = Converter.Degree2Radian(angleStart);
var aEnd = Converter.Degree2Radian(angleEnd);
coating.ComputeSPhase(wavelength, nFront, nBehind, aStart, aEnd, true);
var kxTest = MultiLayer.Angle2SpatialFrequency(wavelength, nFront, Converter.Degree2Radian(6.0));
(double dx, double dPhase) = coating.ComputesLateralShiftEffects(kxTest, InPlanePolMode.TE);
Printer.Write($"dx = {dx}, dPhase = {dPhase}");