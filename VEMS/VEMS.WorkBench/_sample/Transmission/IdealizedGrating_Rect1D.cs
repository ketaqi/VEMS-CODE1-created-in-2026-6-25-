var d = 15.0E-6;
var f = 0.5;

var g = new IdealGrating1D.Rectangular(d, f * d, type: TransmissionType.Phase);
g.Width = 0.33 * d;
g.Shift = 2.0E-6;
g.Scaling = 0.75;
var grid = new GridInfo1D(n: 501, spacing: 0.1E-6);

var gv = g.Sample(grid);
VFrame.CreateShow(values: gv, grid: grid);