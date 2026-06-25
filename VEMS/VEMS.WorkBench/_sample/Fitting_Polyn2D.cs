var f = new SampXYRealFunc(f: new FunctionXY.Gaussian(), 
    px: new List<double>{ 2.0, 0.0, 1.0 },
    py: new List<double>{ 1.0, 0.0, 1.0 });
var gs = new GridInfo2D(rows: 51, cols: 51, spacingY: 0.25, spacingX: 0.25);
var v = f.Sample(grid: gs);
VFrame.CreateShow(values: v, grid: gs);

// generates samples 
var xs = new VectorD(count: v.Count);
var ys = new VectorD(count: v.Count);
var vs = new VectorD(count: v.Count);
for(long i = 0; i < v.Count; i++)
{
    long iRow = i / v.Cols;
    long iCol = i % v.Cols;
    xs[i, false] = gs.GetCoordinateX(iCol);
    ys[i, false] = gs.GetCoordinateY(iRow);
    vs[i, false] = v[iRow, iCol, false];
}

// creates fitter
var fitter = new Polyn2D(xs: xs, ys: ys, vs: vs, degreeX: 25, degreeY: 25);

// evaluation grid
var ge = new GridInfo2D(rows: 41, cols: 71, spacingY: 0.10, spacingX: 0.10);
var xe = new VectorD(count: ge.Rows * ge.Cols);
var ye = new VectorD(count: ge.Rows * ge.Cols);
for(long i = 0; i < ge.Rows * ge.Cols; i++)
{
    long iRow = i / ge.Cols;
    long iCol = i % ge.Cols;
    xe[i, false] = ge.GetCoordinateX(iCol);
    ye[i, false] = ge.GetCoordinateY(iRow);
}

// evaluates
var ve = fitter.Evaluate(xe: xe, ye: ye);

// transforms to 2D
var z = new MatrixD(rows: ge.Rows, cols: ge.Cols);
for(long i = 0; i < ge.Rows * ge.Cols; i++)
{
    long iRow = i / ge.Cols;
    long iCol = i % ge.Cols;
    z[iRow, iCol, false] = ve[i, false];
}
VFrame.CreateShow(values: z, grid: ge);