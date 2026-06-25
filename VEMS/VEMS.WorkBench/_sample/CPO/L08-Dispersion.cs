// parameters ...

// prepares frame
var f = VFrame.CreateFrame();

// dispersion
Func<double, double> nzRe = (nx) => 
{
    if(Math.Abs(nx) <= 1.0){ return Math.Sqrt(1.0 - nx*nx); }
    else{ return 0.0; }
};
Func<double, double> nzIm = (nx) => 
{
    if(Math.Abs(nx) <= 1.0){ return 0.0; }
    else{ return Math.Sqrt(nx*nx - 1.0); }
};
Func<double, double> nzFresnel = (nx) => 1.0 - 0.5 * nx*nx;

// adds into frame
VFrame.AddToFrame(f, func: (nx) => nzRe(nx), lineWidth: 6.0,
    plotColor: Options.PlotColor.LightGray, label: $"(kz/k)-RealPart");
VFrame.AddToFrame(f, func: (nx) => nzIm(nx), lineWidth: 3.0,
    plotColor: Options.PlotColor.Gray, label: $"(kz/k)-ImagPart");
VFrame.AddToFrame(f, func: (nx) => nzFresnel(nx), lineWidth: 3.0,
    plotColor: Options.PlotColor.SteelBlue, label: $"(kz/k)-Fresnel");

// display
VFrame.SetFrameParameters(f, xMin: -1.75, xMax: 1.75, yMin: -0.5, yMax: 1.75,
    xLabel: "kx/k", yLabel: "kz/k", title: "Dispersion Relation(s)");
VFrame.RefreshShow(f);