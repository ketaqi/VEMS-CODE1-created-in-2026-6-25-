var grid = new GridInfo2D(rows: 101, cols: 101, spacingY: 1.0E-6, spacingX: 1.0E-6);
var wavelength = 632.8E-9;
var n = 1.0;
var focalLength = 2000E-3;
var distance = 2000.0E-3;

// advanced version:
// using arbitrary aperture
var semiAxisX = 130E-3;
var semiAxisY = 190E-3;
var apertureGrid = new GridInfo2D(rows: 501, cols: 501, spacingY: 1.0E-3, spacingX: 1.0E-3);
var ellp = new Aperture2D.Ellptical(diameterX: semiAxisX, diameterY: semiAxisY);

var v = Debye.PropagateKernel(wavelength, n, 
    new Grid2DRealData(values: ellp.SampleOnGrid(apertureGrid), gridInfo: apertureGrid), 
    focalLength, distance, grid, ModelingDomain.Spatial, loopMode: LoopMode.Parallel);
var psf = VMath.Square(VMath.Abs(v)); 
VFrame.CreateShow(values: psf, grid: grid, colormap: Options.PlotColormap.Jet,
    title: "PSF-Ellp.", xLabel: "x [m]", yLabel: "y [m]");