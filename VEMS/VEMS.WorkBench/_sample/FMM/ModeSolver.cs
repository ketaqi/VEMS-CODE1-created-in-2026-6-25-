var wavelength = 632.8E-9;
var pillarRadius = 0.5E-6;
// refractive index distribution
Complex n(double w, double x, double y)
{
    var rho = Math.Sqrt(x*x + y*y);
    if(rho <= pillarRadius){ return 1.5; }
    else{ return 1.0; }
}
var medium = new Layer2DMedium(n);
var g = new GridInfo2D(rows: 51, cols: 51, spacingY: 0.02E-6, spacingX: 0.02E-6);
var nidx = medium.Sample(wavelength, g, MaterialProperty.N);
// display
//VFrame.CreateShow(values: nidx, grid: g);

// periodic layer
var px = 1.0E-6;
var py = 1.0E-6;
var layer = new Periodic2DLayer(periodX: px, periodY: py, medium, thickness: 2.0E-6);
(var gamma, var w1, var w2) = layer.ComputeModes(wavelength, fieldsSamplingX: 15, fieldsSamplingY: 13, 
    mediumSamplingX: 27, mediumSamplingY: 7);
// display
VFrame.CreateShow(values: gamma);