// create frame
var frame = VFrame.CreateFrame();
var g = new GridInfo1D(n: 39, spacing: 0.25);

// generates the first function f0 = Cos(x + dXi)
for(int i = 0; i < 22; i++)
{
    var dx = 0.2 * i;
    var y = new VectorD(g.Count);
    //var y = new Grid1DRealData(count:39, spacing: 0.25, initVal: 0.0);
    
    for(int j = 0; j < y.Count; j++)
    {
        var x = g.GetCoordinate(j);
        y[j] = Math.Cos(x - dx);
    }
    
    VFrame.AddToFrame(f: frame, values: y, grid: g,
        lineWidth: 2.0,
        markerSize: 0.0,
        plotColor: Options.PlotColor.SteelBlue,
        label: $"y[{i}]");
}

// plots all 
VFrame.SetFrameParameters(f: frame, 
    title: "Multi-plots", xLabel: "x", yLabel: "y");
VFrame.RefreshShow(frame);
