// parameters 
var x = new VectorD(count: 5);
x[0] = 0.0; x[1] = 1.0; x[2] = 2.0; x[3] = 3.0; x[4] = 4.0;
var v = new VectorD(count: 4);
v[0] = 3.0; v[1] = 2.5; v[2] = 2.0; v[3] = 1.5;

// defines a piecewise-constant data
var pc = new Pwct1DRealData(spans: x, values: v);

// converts to uniformly sampled data
(var u, var g) = pc.SampleOnGrid(n: 170);

// display
VFrame.CreateShow(values: u, grid: g);