// define refractive index function for a specific GRIN medium
var n0 = 2.0; //1.5;
var g = 0.1E3; //250.0;
private Complex Grin(double w, double x)
    => n0 * (1.0 - 0.5 * g * g * x * x);
    

// create 1D medium
var medium = new Medium1D(n: Grin);
var d = 100.0E-6; // 50.0E-6
var wavelength = 532E-9;
// calculate transmission function
var t = TEA1D.Compute(
    layer: medium,
    wavelength: wavelength,
    thickness: d,
    isPhaseOnly: true);
    

// sampling and display
var nx = 301;
var dx = 10.0E-6;
var gx = new GridInfo1D(n: nx, spacing: dx);
var ts = t.Sample(grid: gx, loopMode: LoopMode.Parallel);

VFrame.CreateShow(values: ts, grid: gx * 1E3, // scaling to [mm] unit 
    plotPart: ComplexPart.Argument, 
    title: $"Transmission [Phase]", xLabel: $"x[mm]");