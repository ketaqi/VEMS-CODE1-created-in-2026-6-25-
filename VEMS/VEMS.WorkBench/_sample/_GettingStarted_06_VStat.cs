// generates a vector with random values
Printer.Write("Random number generation ...");
// with default range from 0 to 1
var x1 = VStat.RngUniform(n: 9);
Printer.Write("x1 = ", x1);
// manually specify the value range
var x2 = VStat.RngUniform(n: 9, a: 0.0, b: 99.0);
Printer.Write("x2 = ", x2);


// performs convolution between two vectors
Printer.Write("convolution 1D ...");
var x = new VectorD(count: 9, initVal: 0.0);
x[3] = 1.0; x[4] = 1.0; x[5] = 1.0;
VFrame.CreateShow(values: x, title: "input #1", xLabel: "t", yLabel: "x(t)");
var y = new VectorD(count: 7, initVal: 0.0);
y[2] = 0.5; y[3] = 0.5; y[4] = 0.5;
VFrame.CreateShow(values: y, title: "input #2", xLabel: "t", yLabel: "y(t)");
// convolution between vector x and y
var z = VStat.Convolution(x: x, y: y, mode: ConvMode.Auto);
VFrame.CreateShow(values: z, title: "convolution result", xLabel: "t", yLabel: "f(t)");


// performs convolution between two matrices
Printer.Write("convolution 2D ...");
var a = new MatrixD(rows: 9, cols: 9, initVal: 0.0);
a[3..6, 3..6] = new MatrixD(rows: 3, cols: 3, initVal: 1.0);
VFrame.CreateShow(values: a, colormap: Options.PlotColormap.Magma,
     title: "input #1", xLabel: "x", yLabel: "y");
var b = new MatrixD(rows: 7, cols: 7, initVal: 0.0);
b[2..5, 2..5] = new MatrixD(rows: 3, cols: 3, initVal: 1.0);
VFrame.CreateShow(values: a, colormap: Options.PlotColormap.Magma, 
    title: "input #2", xLabel: "x", yLabel: "y");
// convolution between a and b
var c = VStat.Convolution(x: a, y: b, mode: ConvMode.Auto);
VFrame.CreateShow(values: c, colormap: Options.PlotColormap.Magma, 
    title: "convolution result", xLabel: "x", yLabel: "y");