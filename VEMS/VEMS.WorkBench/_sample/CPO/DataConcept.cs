// starts with a function given in analytical form
var sf = new Samp1DRealFunc(f: (x) => Math.Cos(2.0*Math.PI*x));

// 1) samples the function on a uniform grid
var gx = new GridInfo1D(n: 51, spacing: 0.04);
var ys = sf.Sample(grid: gx);
VFrame.CreateShow(values: ys, grid: gx, lineWidth: 0.1, title: "y-values [initial samples]");


// 2) continuation of sampled data
var cy = new Grid1DRealData(values: ys, gridInfo: gx, 
    interp: InterpolationMethod.Cubic, periodic: true);
    
// 2.1) find values on a denser grid
var gxDense = new GridInfo1D(n: 100, spacing: 0.02);
var ysDense = cy.FindValues(targetGrid: gxDense);
VFrame.CreateShow(values: ysDense, grid: gxDense, lineWidth: 0.1, title: "y-values [densely re-sampled]");

// 2.2) find values on a larger grid
var gxLarge = new GridInfo1D(n: 100, spacing: 0.04);
var ysLarge = cy.FindValues(targetGrid: gxLarge);
VFrame.CreateShow(values: ysLarge, grid: gxLarge, lineWidth: 0.1, title: "y-values [re-sampled on larger range]");