// sampling grid information
var gridInfo = new GridInfo2D (2001, 2001, 10.0E-6, 10.0E-6);
var displayGrid = new GridInfo2D (gridInfo.Rows, gridInfo.Cols,
    gridInfo.SpacingY*1E3, gridInfo.SpacingX*1E3);

// Gaussian waist values
var wx = 2.5E-3; // waist of Gaussian; 7.5mm
var wy = 2.5E-3;

// generate input Gaussian
var v = new MatrixZ(gridInfo.Rows, gridInfo.Cols); 
for (long iRow = 0; iRow < v.Rows; iRow++)
{
    var iy = gridInfo.GetCoordinateY(iRow);
    for (long iCol = 0; iCol < v.Cols; iCol++)
    {
        var ix = gridInfo.GetCoordinateX(iCol); 
        //if(Math.Abs(iy) <= wy || Math.Abs(ix) <= wx)
            v[iRow, iCol] = Math.Exp(-(ix*ix)/(wx*wx)) * Math.Exp(-(iy*iy)/(wy*wy));
    }
}
var Field = new EMField(1064E-9, Complex.One, gridInfo, v);

// plot input
VFrame.CreateShow(values: Field.E, grid: displayGrid, colormap: Options.PlotColormap.Jet,
    title: "input field", xLabel: "x [mm]", yLabel: "y [mm]");
     
// transmission function
var alpha = -0.62 * Math.PI / 180; // convergence angle
var k1 = Field.K0 * Math.Sin(alpha); // radial slope #1
var k2 = Field.K0 * Math.Sin(alpha + 1.0 * Math.PI); // radial slope #2
var t = new MatrixZ(gridInfo.Rows, gridInfo.Cols);
for(long iRow = 0; iRow < t.Rows; iRow++)
{
    var iy = gridInfo.GetCoordinateY(iRow);
    for(long iCol = 0; iCol < t.Cols; iCol++)
    {
        var ix = gridInfo.GetCoordinateX(iCol);
        var iRho2 = ix * ix + iy * iy;
        var iRho = Math.Sqrt(iRho2);
        if(iy >= 0.0 )
            t[iRow, iCol] = Complex.Exp(Complex.ImaginaryOne * k1 * iRho);
        else
            t[iRow, iCol] = Complex.Exp(Complex.ImaginaryOne * (k2 * iRho + 0.0* Math.PI));
    }
}

// plot transmission
VFrame.CreateShow(values: t, grid: displayGrid, plotPart: ComplexPart.Argument, 
    colormap: Options.PlotColormap.Grayscale,
    title: "transmission phase", xLabel: "x [mm]", yLabel: "y [mm]");
    
// field modification
Field.E *= t;

Field.Transform();
//VFrame.CreateShow(values: Field.E, grid: displayGrid, colormap: Options.PlotColormap.Jet,
//    title: "transmitted field", xLabel: "x [mm]", yLabel: "y [mm]");

// field propagation
var d = 110E-3; // 30mm
Field.Propagate(d); 

// transform back to x domain
Field.Transform();
// compute intensity
MatrixD Intensity = VMath.Abs(Field.E);
Intensity *= Intensity;
VFrame.CreateShow(values: Intensity, grid: displayGrid, colormap: Options.PlotColormap.Jet,
    title: "output field intensity", xLabel: "x [mm]", yLabel: "y [mm]");

/*
var zSampInfo = new SampInfo1D(25, 0, 10E-3);
var onAxisIntensity = new VectorD(zSampInfo.Count);
for(int iz = 0; iz < zSampInfo.Count; iz++)
{
    Field.Propagate(zSampInfo.Spacing);
    Field.Transform();
    var Intensity = VMath.Abs(Field.E);
    Intensity *= Intensity;
    onAxisIntensity[iz] = Intensity[(Field.GridInfo.Rows-1)/2, (Field.GridInfo.Cols-1)/2];
}
VFrame.CreateShow(values: onAxisIntensity, grid: zSampInfo,
    title: "On-Axis Intensity", xLabel: "z", yLabel: "Intensity");
*/