// physical parameters
var w = 40.0E-6; // width of rectangular function
var wavelength = 632.8E-9; // wavelength in vacuum
var epsilon = Complex.One; // permittivity
// numerical parameters
var n = 451;
var dx = 0.75E-6;


// generate scalar field component
var v = new ScalarField1D.PlaneWave(wavelength, epsilon, w, new GridInfo1D(n, dx));
VFrame.CreateShow(values: VMath.Abs(v.Field), grid: v.Grid, label:"",
    title: "Input Abs[E] " + v.Domain.ToString(), xLabel: "coordinates [m]", yLabel: "function value [A.U.]");


// propagation for within given z-range
var nZ = 2001;
var dZ = 500E-9;
var gridXZ = new GridInfo2D(v.Grid.Count, nZ, v.Grid.Spacing, dZ);
gridXZ.StartX = 0.0;
var gridKxZ = new GridInfo2D(v.Grid.Count, nZ, 2.0 * Math.PI / v.Grid.Range, dZ);
gridKxZ.StartX = 0.0;
var vKxZ = new MatrixZ(gridXZ.Rows, gridXZ.Cols, 0.0);
var vXZ = new MatrixZ(gridXZ.Rows, gridXZ.Cols, 0.0);
var allRows = new LongRange(0, gridXZ.Rows);
// initial values
vXZ[allRows, 0] = v.Field;
v.SwitchToKDomain();
vKxZ[allRows, 0] = v.Field;
// loop
for(long i = 1; i < gridXZ.Cols; i++)
{
    v.Propagate(gridXZ.SpacingX);
    vKxZ[allRows, i] = v.Field;
    v.SwitchToXDomain();
    vXZ[allRows, i] = v.Field;
    v.SwitchToKDomain();
}

// display [Abs]
VFrame.CreateShow(values: vXZ, grid: gridXZ, ComplexPart.Magnitude, Options.PlotColormap.Magma,
    title: "Abs[V(x,z)]", xLabel: "z", yLabel: "x");
    
// display [Arg]
//Figure.Show(new GridMatD(gridKxZ, VMath.Arg(vKxZ)), "Arg[V(kx,z)]", "z", "kx");
//Figure.Show(new GridMatD(gridXZ, VMath.Arg(vXZ)), "Arg[V(x,z)]", "z", "x");