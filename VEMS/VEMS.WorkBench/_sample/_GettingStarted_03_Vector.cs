// generate regular vectors
Printer.Write("Generating regular vectors... ");

var x1 = new VectorD(count: 7, initVal: 0.1);
Printer.Write(prompt: "x1 = ", x: x1);

var x2 = new VectorD(count: 6, initVal: 1.0, increment: -3.33);
Printer.Write(prompt: "x2 = ", x: x2);

//var x3 = new VectorD(initVal: -5.0, endVal: 10.0, count: 5);
//Printer.Write(prompt: "x3 = ", x: x3);

// access a vector element
Printer.Write($"The 1st element in x2: {x2[0]}");
Printer.Write($"The 2nd element in x2: {x2[1]}");
Printer.Write($"The last element in x2: {x2[^1]}");

// slice in a vector
var s = x2[2..^0];
Printer.Write(prompt: "A slice from the 3rd to the last element: ", x: s);

// pointwise operations
var a = new VectorD(count: 5, initVal: 0.33);
var b = new VectorD(count: 5, initVal: 0.1, increment: 0.1);
Printer.Write(prompt: "a = ", x: a);
Printer.Write(prompt: "b = ", x: b);
// addition
var sum = a + b;
Printer.Write(prompt: "sum = ", x: sum);
// subtraction
var sub = a - b;
Printer.Write(prompt: "sub = ", x: sub);
// multiply
var mul = a * b;
Printer.Write(prompt: "mul = ", x: mul);
// divide
var div = a / b;
Printer.Write(prompt: "div = ", x: div);

// construct a complex-valued vector
Printer.Write("Generating a complex vector ...");
var v1 = new VectorD(count: 5, initVal: 0.1);
var v2 = new VectorD(count: 5, initVal: 0.0, increment: 1.0);
var x = VMath.Construct(realPart: v1, imagPart: v2);
Printer.Write(prompt: "x = ", x);

// reverses elements in a vector
x.Reverse();
Printer.Write(prompt: "reversed x = ", x);