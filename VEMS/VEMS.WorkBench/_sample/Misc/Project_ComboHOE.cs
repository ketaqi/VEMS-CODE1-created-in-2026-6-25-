// physical parameters
var wavelength = 780E-9;
var n1 = 1.0;
var n2 = 1.5;
var inputAngleX = 60; // degree
var outputAngleY = 30; // degree
var focalLength = 10E-3;

// sampling parameters
double dx = 0.1E-6;
double dy = 0.1E-6;
long nx = 2001;
long ny = 2001;

// calculating (x)
var kIn = 2.0 * Math.PI / wavelength * n1;
var kxIn = kIn * Math.Sin(Converter.Degree2Radian(inputAngleX));
// calculating (y)
var kOUt = 2.0 * Math.PI / wavelength * n2;
var focalShift = Math.Tan(Converter.Degree2Radian(outputAngleY)) * focalLength;

// generating dPsi
var dPsi = new Grid2DRealData(ny, nx, dy, dx, 0.0);
for(long iRow = 0; iRow < dPsi.GridInfo.Rows; iRow++)
{
    for(long iCol = 0; iCol < dPsi.GridInfo.Cols; iCol++)
    {
        var x = dPsi.GridInfo.GetCoordinateX(iCol);
        var y = dPsi.GridInfo.GetCoordinateY(iRow);
        
        var dPsiX = (0.0 - kxIn) * x;
        var dPsiY = -kOUt * Math.Sqrt(Math.Pow(y-focalShift, 2.0) + Math.Pow(0.0-focalLength, 2.0));
        
        var t = Complex.Exp(Complex.ImaginaryOne * (dPsiX + dPsiY));
        dPsi.Values[iRow, iCol] = t.Phase;// dPsiX + dPsiY;
    }
}

// result display
VFrame.CreateShow(dPsi);