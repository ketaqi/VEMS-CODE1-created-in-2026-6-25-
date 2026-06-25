// physical parameters
var w = 2.5E-6; // waist of Gaussian set to 7.5um
var wavelength = 632.8E-9; // wavelength in vacuum
var epsilon = Complex.One; // permittivity
// numerical parameters
long n = 101;
double dx = 1.0E-6;


// generate scalar field component
var v = new ScalarField1D.Gaussian(wavelength, epsilon, w, new GridInfo1D(n, dx));
VFrame.CreateShow(values: v.Field, grid: v.Grid, 
    title: $"Input Field {v.Domain}", xLabel: "coordinates [m]", yLabel: "value [A.U.]");

// propagation for within given z-range
var nZ = 7001;
var dZ = 25E-9;
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
//Figure.Show(new GridMatD(gridKxZ, VMath.Abs(vKxZ)), "Abs[V(kx,z)]", "z", "kx");
//Figure.Show(new GridMatD(gridXZ, VMath.Abs(vXZ)), "Abs[V(x,z)]", "z", "x");
//VFrame.CreateShow(values: vKxZ, grid: gridKxZ, title: "V(kx,z)", xLabel: "z", yLabel: "kx");
VFrame.CreateShow(values: vXZ, grid: gridXZ, title: "V(x,z)", xLabel: "z", yLabel: "x");