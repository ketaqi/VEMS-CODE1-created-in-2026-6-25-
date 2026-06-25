// define refractive index function for a specific GRIN medium
var n0 = 2.0; //1.5;
var g = 0.1E3; //250.0;
private Complex Grin2D(double w, double x, double y)
    => n0 * (1.0 - 0.5 * g * g * (x * x + y * y));


// create thin layer
var medium = new Medium2D(n: Grin2D);
var wavelength = 532E-9;
var d = 100.0E-6; //50.0E-6;
// calculate transmission function
var t = TEA2D.Compute(
    layer: medium,
    wavelength: wavelength,
    thickness: d,
    isPhaseOnly: true);


// sampling and display
var ny = 301; var nx = 301;
var dy = 10.0E-6; var dx = 10.0E-6;
var gxy = new GridInfo2D(rows: ny, cols: nx, spacingY: dy, spacingX: dx);
var txy = t.Sample(grid: gxy, loopMode: LoopMode.Parallel);
VFrame.CreateShow(values: txy, grid: gxy * (1E3, 1E3), // scaling to [mm] unit
    plotPart: ComplexPart.Argument,
    title: $"Transmission [Phase]", xLabel: $"x[mm]", yLabel: $"y[mm]");