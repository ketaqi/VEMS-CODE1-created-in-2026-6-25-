var rows = 7;
var cols = 7;
// data matrix
var x2 = new MatrixZ(rows, cols, new Complex(1.3, 0.2));
x2[1, 3] = 0.9; x2[2, 2] = 1.1; x2[4, 2] = 1.0;
// grid
var gx = new GridInfo2D(rows, cols);

// creates a VFrame
var f = VFrame.CreateFrame();
    
// add complex-valued GridGraph into the frame
VFrame.AddToFrame(f, 
    values: x2, 
    grid: gx,
    colormap: Options.PlotColormap.Magma);

// refresh and display
VFrame.RefreshShow(f);