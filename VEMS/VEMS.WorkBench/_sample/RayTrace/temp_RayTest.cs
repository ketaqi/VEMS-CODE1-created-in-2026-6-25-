// input ray info
var rIn = new VecD3(){ X = 4.5E-3, Y = 6.0E-3, Z = 0.0 };
var angle = Math.PI / 6.0;
var sIn = new VecD3(){ X = Math.Sin(angle), Y = 0.0, Z = Math.Cos(angle) };
// spherical surface
var d = 0.0E-3;
var r = 25.0E-3; 
var rho = 1.0 / r;
// refractive indices
double nFront = 1.0;
double nBehind = 1.5;

// intersect
RayBase.Intersect(rIn, sIn, d, r,
    out VecD3 rOut,
    out double p);  
Printer.Write($"rOut: X = {rOut.X}, Y = {rOut.Y}, Z = {rOut.Z}.");

// reflect
var sOut = RayBase.Reflect(rOut, sIn, rho); 
Printer.Write($"sOut: X = {sOut.X}, Y = {sOut.Y}, Z = {sOut.Z}.");

// refract
var normal = RayBase.ComputeSphericalSurfaceNormal(rOut, r);
var t = RayBase.Refract(sIn, normal, nFront, nBehind);
Printer.Write($"t: X = {t.X}, Y = {t.Y}, Z = {t.Z}.");
