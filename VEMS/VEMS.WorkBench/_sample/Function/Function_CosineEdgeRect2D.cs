// define sampling grid
var grid = new GridInfo2D(rows: 301, cols: 301, spacingY: 0.33, spacingX: 0.33);
// define window function parameters
var wx = 50.0; var wy = 40.0;
var rx = 0.33; var ry = 0.33;
var x0 = 0.0; var y0 = 0.0;
var ax = 1.0; var ay = 1.0;

// construct window
var win = new SampXYRealFunc(f: new FunctionXY.CosEdgeRectangle(), 
    px: new List<double>{ wx, rx*wx, x0, ax },
    py: new List<double>{ wy, ry*wy, y0, ay});
// sample on grid
var sampWindow = win.Sample(grid);

// show sampled result
VFrame.CreateShow(values: sampWindow, 
    grid: grid, 
    label: "window",
    title: "Rectangle with Cosine-Smoothed Edge");