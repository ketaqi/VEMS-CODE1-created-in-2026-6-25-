var g = new GridInfo2D(rows: 501, cols: 701, 
    spacingY: 0.1, spacingX: 0.1);
var wx = 7.5;
var wy = 12.5;

var func = new SampXYRealFunc(f: new FunctionXY.Gaussian(), 
    px: new List<double>{ wx, 0.0, 1.0 },
    py: new List<double>{ wy, 0.0, 1.0 });
var a = func.Sample(g); 

VFrame.CreateShow(values: a, grid: g, 
    title: "My Plot", xLabel: "x", yLabel: "y");