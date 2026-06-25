var n = 7;
// containers for vertices
var vx = new VectorD(n);
var vy = new VectorD(n);
// circle information
var radius = 1.5;
var dPhi = 2.0 * Math.PI / n;
for(long i = 0; i < n; i++)
{
    var phi = dPhi * i;
    vx[i] = radius * Math.Cos(phi);
    vy[i] = radius * Math.Sin(phi);
}

// generates polygon
var p = new Geometry2D.Polygon(n: n, vx: vx, vy: vy);
var targetGrid = new GridInfo2D(rows: 131, cols: 131, spacingY: 0.03, spacingX: 0.03);
var sampPoly = p.SampleOnGrid(targetGrid);
// display
VFrame.CreateShow(values: sampPoly, grid: targetGrid);