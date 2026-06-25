// defines input Gaussian function in t-domain
Func<double, double> gt = (t) => Math.Exp(- t * t);
// makes sampling
var dt = 0.5;
var tGrid = new GridInfo1D(n: (long)(10.0/dt) + 1, spacing: dt);
var sGt = new Samp1DRealFunc(f: gt).Sample(tGrid);

// creates a VFrame for t-domain
var tFrame = VFrame.CreateFrame();
VFrame.SetTitle(tFrame, "Sampling in t-domain");
// add Plots to the frame
VFrame.AddToFrame(tFrame, values: sGt, grid: tGrid,
    lineWidth: 0.0, markerSize: 8.0, plotColor: Options.PlotColor.SteelBlue,
    label: $"sample values fs(t)");
VFrame.AddToFrame(tFrame, func: (t) => gt(t), 
    lineWidth: 1.0, plotColor: Options.PlotColor.Black,
    label: $"Gaussian function f(t)");
// refresh and display
VFrame.SetLegend(tFrame, location: Options.LegendLocation.UpperRight, fontSize: 18.0);
VFrame.SetLabelX(tFrame, content: "t");
VFrame.SetLabelY(tFrame, content: "f(t)");
VFrame.SetXAxisLimits(tFrame, min: -5.2, max: 5.2);
VFrame.SetYAxisLimits(tFrame, min: -0.2, max: 1.4);
VFrame.RefreshShow(tFrame);


// defines output Gaussian function in w-domain
Func<double, double> gw = (w) => 1.0 / Math.Sqrt(2.0) * Math.Exp(- w * w / 4.0);
// defines the convolution in w-domain
var m = 17; // finite multiples for the convolution
Func<double, double> gwConv = (w) => 
{ 
    var result = 0.0;
    for(int i = -(m-1)/2; i <= (m-1)/2; i++)
    {
        var wp = w - i * 2.0 * Math.PI / dt;
        result += 1.0 / Math.Sqrt(2.0) * Math.Exp(- wp * wp / 4.0);
    }
    return result;
};
// creates a VFrame for w-domain
var wFrame = VFrame.CreateFrame();
VFrame.SetTitle(wFrame, "Convolution in w-domain");
// add Plots to the frame
VFrame.AddToFrame(wFrame, func: (w) => gwConv(w), 
    lineWidth: 5.0, plotColor: Options.PlotColor.DarkOrange,
    label: $"Convolution result Fs(w)");
VFrame.AddToFrame(wFrame, func: (w) => gw(w), 
    lineWidth: 1.0, plotColor: Options.PlotColor.Black,
    label: $"Gaussian function F(w)");
// refresh and display
VFrame.SetLegend(wFrame, location: Options.LegendLocation.UpperRight, fontSize: 18.0);
VFrame.SetLabelX(wFrame, content: "w");
VFrame.SetLabelY(wFrame, content: "F(w)");
VFrame.SetXAxisLimits(wFrame, min: -20.0, max: 20.0);
VFrame.SetYAxisLimits(wFrame, min: -0.2, max: 1.0);
// add range
VFrame.SetHorizontalSpan(wFrame, start: -Math.PI/dt, end: Math.PI/dt, 
    fillColor: Options.PlotColor.DarkOrange);
VFrame.RefreshShow(wFrame);