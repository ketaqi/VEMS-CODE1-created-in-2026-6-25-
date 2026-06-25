Printer.Write("_____Start_____");
var wavelength = 13.5E-9;
var k0 = 2.0 * Math.PI / wavelength;
var x0 = 0;//光源的位置
var z = 12210E-9;//光源到膜层表面的距离
var l = 9800E-9;
var i = Complex.ImaginaryOne;
var dx = 0.33 * wavelength;
var N = (long)(l / dx) + 1;
if(N %2 == 0){ N += 1; }
var allXFigs = VFrame.CreateFrame();
VFrame.SetFrameParameters(allXFigs, xLabel: "x", yLabel: "values", title: "x-figures");

//定义球面波
var gridX = new GridInfo1D (n: N, spacing: dx);
var xs = gridX.GetCoordinates();
var rho = VMath.Sqrt(VMath.Square(xs - x0) + z * z);
var a0 = z; // scaling factor
var vxIn = new Grid1DCplxData(gridInfo: gridX, values: new VectorZ(gridX.Count, 0.0));
vxIn.Values = a0 * 1.0 / rho * VMath.Exp(i * k0 * rho);
// quick figure
//VFrame.CreateShow(gv: vxIn, title: "vxIn");

//Smooth-Edge Envelope
var edgeRatio = 0.1;
var edge = edgeRatio * l;
var offset = 0.1 * l;
var edgeLeft = vxIn.GridInfo.Start + edge + offset;
var edgeRight = vxIn.GridInfo.End - edge - offset;
// left side edge
for(long ii = 0; ii < vxIn.GridInfo.Count; ii++)
{
    var ix = vxIn.GridInfo.GetCoordinate(ii);
    if(ix >= edgeLeft)
        break;
    else if(ix >= edgeLeft - edge)
        vxIn.Values[ii] *= 0.5*(Math.Sin(Math.PI *(ix-edgeLeft+0.5*edge)/edge) + 1.0);
    else
        vxIn.Values[ii] = 0.0;
}
// right side edge
for(long ii = vxIn.GridInfo.Count; ii >= 0; ii--)
{
    var ix = vxIn.GridInfo.GetCoordinate(ii);
    if(ix <= edgeRight)
        break;
    else if(ix <= edgeRight + edge)
        vxIn.Values[ii] *= 0.5*(-Math.Sin(Math.PI *(ix-edgeRight-0.5*edge)/edge) + 1.0);
    else
        vxIn.Values[ii] = 0.0;
}
// add to frame
var vxInArg = new Grid1DRealData(gridInfo: vxIn.GridInfo, values: VMath.Arg(vxIn.Values));
vxInArg.Values = SimplePhaseUnwrap(vxInArg.Values);
VFrame.AddToFrame(f: allXFigs, gv: vxInArg, 
    plotColor: Options.PlotColor.Gray, lineWidth: 1.5, label: "vIn(x)-Phase");


//膜层信息
var cycleThick = 1.0 * 6.97E-9;
int NumberLay = 40;
var gamma = 0.4;                                       //Periodic thickness ratio of Mo
var nFront = 1.000;                                    // air in front 
var nBehind = 1.5438;                                  // fused silica behind    0.21um
var nMo = new Complex(0.92318704, 0.0065100849);
var nSi = new Complex(0.99873895, 0.0018278216);
//定义膜层
var nLayers = new List<Complex>();
var tLayers = new List<double>();
for (int i = 0; i < NumberLay; i++)
{
    nLayers.Add(nMo);
    nLayers.Add(nSi);
    tLayers.Add(cycleThick * gamma);
    tLayers.Add(cycleThick * (1 - gamma));
}


// FFT method as reference
var vkIn = new Grid1DCplxData(values: vxIn.Values, gridInfo: vxIn.GridInfo);
Transform.FFT(x: vkIn.Values, grid: vkIn.GridInfo, FFTOption.Forward); // Now, this this used as input
var kx = vkIn.GridInfo.GetCoordinates();
(var _, var s21) = CoatingCalculator.ComputeCoatingMatrix(wavelength, 
    nFront, nLayers, tLayers, nBehind, kx, InPlanePolMode.TE);  
var vxOut = new Grid1DCplxData(values: vkIn.Values * s21, gridInfo: vkIn.GridInfo);
Transform.FFT(x: vxOut.Values, grid: vxOut.GridInfo, FFTOption.Backward);
var vxOutArg = new Grid1DRealData(values: VMath.Arg(vxOut.Values), gridInfo: vxOut.GridInfo);
vxOutArg.Values = SimplePhaseUnwrap(vxOutArg.Values);
// add to frame
VFrame.AddToFrame(f: allXFigs, gv: vxOutArg,
    plotColor: Options.PlotColor.Black, lineWidth: 3.0, 
    label: "vOut(x)-Phase");


// GFT method for test
var scatKx = k0 * vxIn.GridInfo.GetCoordinates() / rho;
//VFrame.CreateShow(grid: vxIn.Grid, values: scatKx, title: "Kx");
(var _, var scatS21) = CoatingCalculator.ComputeCoatingMatrix(wavelength, 
    nFront, nLayers, tLayers, nBehind, scatKx, InPlanePolMode.TE); 
var scatS21Arg = VMath.Arg(scatS21);
scatS21Arg = SimplePhaseUnwrap(scatS21Arg, false);
// quick figure
VFrame.CreateShow(grid: vxIn.GridInfo, values: scatS21Arg, title: "S21-Phase");
var scatDx = new VectorD(scatKx.Count, 0.0);
for(long j = 1; j < scatKx.Count - 1; j++)
{
    scatDx[j] = (scatS21Arg[j+1] - scatS21Arg[j-1])
        / (scatKx[j+1] - scatKx[j-1]);
}
//VFrame.CreateShow(grid: vxIn.Grid, values: scatDx, title: "scat dX");
var geopDx = new VectorD(scatKx, true);
for(long j = 0; j < scatKx.Count; j++)
{
    var kx = scatKx[j];
    var kz = Math.Sqrt(k0 * k0 - kx * kx);
    var tan = kz / k0;
    geopDx[j] = 2.0 * cycleThick * NumberLay / tan;
}
//VFrame.CreateShow(grid: vxIn.Grid, values: geopDx, title: "deop dX");

// construct output field
var vxGut = new Scat1DCplxData(xs: vxIn.GridInfo.GetCoordinates() - scatDx,
    values: vxIn.Values * scatS21 * VMath.Exp(-i * scatKx * scatDx));
var vxGutArg = new Scat1DRealData(xs: vxGut.Points, values: VMath.Arg(vxGut.Values));
vxGutArg.Values = SimplePhaseUnwrap(vxGutArg.Values);
VFrame.AddToFrame(f: allXFigs, sv: vxGutArg, 
    plotColor: Options.PlotColor.SteelBlue, lineWidth: 1.0,
    markerShape: Options.MarkerShape.filledDiamond, label: "vGut(x)-Phase");
  
  
// pure geometric method for comparison
var vxGeo = new Grid1DCplxData(gridInfo: vxIn.GridInfo, 
    values: vxIn.Values * scatS21);
var vxGeoArg = new Grid1DRealData(values: VMath.Arg(vxGeo.Values), gridInfo: vxGeo.GridInfo);
vxGeoArg.Values = SimplePhaseUnwrap(vxGeoArg.Values);
VFrame.AddToFrame(f: allXFigs, gv: vxGeoArg,
    plotColor: Options.PlotColor.OrangeRed, lineWidth: 1.0,
    markerShape: Options.MarkerShape.openTriangleUp, label: "vGeo(x)-Phase");


//ScatVecZ vxGFT = new ScatVecZ();



// display figures
VFrame.RefreshShow(allXFigs);



// phase unwrap
private VectorD SimplePhaseUnwrap(VectorD phaseIn, bool isIncremental = true)
{
    VectorD phaseOut = new VectorD(phaseIn, true);
    long n = 0;
    for(long i = (phaseOut.Count-1)/2+1; i < phaseOut.Count; i++)
    {
        double directDiff = Math.Abs(phaseIn[i] - phaseIn[i-1]);
        double testDiff = isIncremental == true?
            Math.Abs(phaseIn[i] + 2.0 * Math.PI - phaseIn[i-1]) : Math.Abs(phaseIn[i] - 2.0 * Math.PI - phaseIn[i-1]);
        if(testDiff < directDiff)
        {
            n = n + 1;
        }
        phaseOut[i] = isIncremental == true?
            phaseIn[i] + n * 2.0 * Math.PI : phaseIn[i] - n * 2.0 * Math.PI;

    }
    return phaseOut; // new VectorD(1, 0.0);
}