var grid = new GridInfo2D(101, 101, 1E-6, 1E-6);
//var kgrid = grid.ComputeConjugateInfo();
var wavelength = 632.8E-9;
var n = 1.0;
var focalLength = 2000E-3;
var distance = 2000.0E-3;

// basic version:
// using circular aperture
var diameter = 460E-3;
var v = Debye.PropagateKernel(wavelength, n,
    diameter, focalLength, distance, grid, ModelingDomain.Spatial,
    0.0, 0.0, loopMode: LoopMode.Parallel); //10400.0, 10400.0);

// plot
var psf = new Grid2DRealData(values: VMath.Square(VMath.Abs(v)), gridInfo: grid);
//Figure.Show(psf, "Abs[E]", "x [m]", "y [m]", "Jet");
VFrame.CreateShow(values: psf, colormap: Options.PlotColormap.Jet,
    title: "PSF-circ.", xLabel: "x [m]", yLabel: "y [m]");