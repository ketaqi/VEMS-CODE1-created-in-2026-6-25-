var p = new VecD3()
{
    X = 0.0,
    Y = 0.86602540,
    Z = 0.5
};

var sIn = VecD3.UnitZ;
var sOut = new VecD3()
{
    X = 0.0,
    Y = 0.332938,
    Z = 0.942948
};

var sxIn = VecD3.Dot(sIn, p);
var sxOut = VecD3.Dot(sOut, p);

Printer.Write($"sxIn = {sxIn}");
Printer.Write($"sxOut = {sxOut}");

var wavelength = 587.5618E-9;
var k0 = 2.0 * Math.PI / wavelength;
var dK = k0*(sxOut - sxIn);
var d = 2.0 * Math.PI / dK;

Printer.Write($"Grating period = {d}");