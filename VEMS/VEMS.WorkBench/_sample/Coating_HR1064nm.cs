// parameters
var wavelength = 1064E-9; // wavelength in vacuum
var polar = InPlanePolMode.TE; // set the polarization mode
var angleStart = 0.0; // in degree
var angleEnd = 89.0; // in degree
var nAngle = 90;
//var k0 = 2.0 * Math.PI / wavelength;
Func<double, Complex> nFront = (wl) => 1.000274;
Func<double, Complex> nBehind = (wl) => 1.5066 + Complex.ImaginaryOne * 1.0888e-8;
Func<double, Complex> nHfO2 = (wl) => 1.8810; 
Func<double, Complex> nSiO2 = (wl) => 1.4496;

// constructs multilayer
var tHfO2 = 0.25 * wavelength / nHfO2(wavelength).Real;
var tSiO2 = 0.25 * wavelength / nSiO2(wavelength).Real;
var coating = new MultiLayer();
coating.AddLayer(nHfO2, tHfO2);
coating.AddAlteringLayers(nSiO2, tSiO2, nHfO2, tHfO2, 12);

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