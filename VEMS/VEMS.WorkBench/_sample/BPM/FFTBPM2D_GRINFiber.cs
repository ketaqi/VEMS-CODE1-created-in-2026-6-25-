// define refractive index function for a specific GRIN medium
var n0 = 2.0; //1.5;
var f = 14.0E-3;
var pitch = 4.0 * f;
var g = 2.0 * Math.PI / pitch; 
private Complex Grin(double w, double x, double y)
    => n0 * (1.0 - 0.5 * g * g * (x * x + y * y));


// sampling parameters
var nx = 751; var ny = 751;
var dx = 2.0E-6; var dy = 2.0E-6;
var gxy = new GridInfo2D(rows: ny, cols: nx,
    spacingY: dy, spacingX: dx);


// create 2D medium
var med = new Medium2D(n: Grin);
var wavelength = 532E-9;
var d = f; // 50.0E-6
    

// prepare input field
var w = 250E-6;
var sfx = new Samp2DCplxFunc(f: (x, y) => 
    Function1D.Gaussian(x, new List<double>{w}) * Function1D.Gaussian(y, new List<double>{w}));
var vIn = sfx.Sample(grid: gxy, loopMode: LoopMode.Parallel);
VFrame.CreateShow(values: VMath.Abs(vIn), title: $"Input E-field [Magnitude]",
    grid: gxy * (1E3, 1E3), // scaled to [mm] unit
    xLabel: $"x [mm]", yLabel: $"y [mm]",
    colormap: Options.PlotColormap.Magma);


// FT-BPM
var nLayers = 29;
var bpm = new FFTBPM2D(mediumXY: med);
var vOut = bpm.Propagate(
    wavelength: wavelength,
    vIn: vIn, gIn: gxy,
    zStart: 0.0, zEnd: d, nLayers: nLayers,
    n0: 2.0,
    loopMode: LoopMode.Sequential);


// display output
VFrame.CreateShow(values: VMath.Abs(vOut), title: $"Output E-field [Magnitude]",
    grid: gxy * (1E3, 1E3), // scaled to [mm] unit
    xLabel: $"x [mm]", yLabel: $"y [mm]",
    colormap: Options.PlotColormap.Magma);
    
/*
// x-z plots ...
var gxz = new GridInfo2D(rows: gx.Count, cols: nLayers,
    spacingY: gx.Spacing, refPointY: gx.Start, refTypeY: GridRefType.Start,
    spacingX: d/nLayers, refPointX: 0.5 * d, refTypeX: GridRefType.Center);
    
    
// plot medium
//var allRows = new LongRange(0, gxz.Rows);
var mx = med.Sample(wavelength: wavelength, grid: gx);
var mxz = new MatrixZ(rows: gxz.Rows, gxz.Cols);
for (int iCol = 0; iCol < gxz.Cols; iCol++)
{ mxz[mxz.AllRows, iCol] = mx; }
VFrame.CreateShow(values: VMath.Abs(mxz), title: $"Refractive Index Distribution",
    grid: gxz.Scale(scaleY: 1E3, scaleX: 1E2, refTypeY: GridRefType.Center, refTypeX: GridRefType.LowerBound), 
    xLabel: $"z [dm]", yLabel: $"x [mm]");
    
    
// plot field in x-z
var vxz = new MatrixZ(rows: gxz.Rows, cols: gxz.Cols);
for (int iCol = 0; iCol < gxz.Cols; iCol++)
{ vxz[vxz.AllRows, iCol] = vInside[iCol]; }
VFrame.CreateShow(values: VMath.Abs(vxz), title: $"E-field [Magnitude] Distribution",
    grid: gxz.Scale(scaleY: 1E3, scaleX: 1E2, refTypeY: GridRefType.Center, refTypeX: GridRefType.LowerBound), 
    xLabel: $"z [dm]", yLabel: $"x [mm]",
    colormap: Options.PlotColormap.Jet);
*/