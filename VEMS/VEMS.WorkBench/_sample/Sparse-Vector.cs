// dense vector containers
var v = new VectorD(count: 4, initVal: 0.5);
var vr = new VectorD(count: 4, initVal: 0.5, increment: 0.5);

// generating sparse vectors
// sparse vector non-zero info
var nzi = new VectorI(count: 2){ [0] = 1, [1] = 3 };
var nzv = new VectorD(count: 2){ [0] = 1.0, [1] = 2.0 };
var vi = new VectorDi(n: 4, nnz: 2, nzIdx: nzi, nzVal: nzv); // nzInfo: new SPVInfo<double>(nzi, nzv)); 

// scatter to dense form
var vs = vi.Scatter();
Printer.Write("vs = ", vs);

// dot product tests ...
var dp = Sparse.Dot(x: vi, y: v);
Printer.Write($"dp = {dp}");

// absolute sum test ...
var absSum = Sparse.ASum(x: vi);
Printer.Write($"absSum1 = {absSum}");

// addTo test ...
var addTo = new VectorD(count: 4, initVal: 0.1, increment: -0.2);
Sparse.AddTo(x: vi, y: ref addTo);
Printer.Write("addTo = ", addTo);

// complex-valued sparse vector
var c = VMath.Construct(realPart: VStat.RngUniform(7), 
    imagPart: VStat.RngGaussian(7, 0.5, 0.1));
var cnz = new VectorI(2); cnz[0] = 2; cnz[1] = 5;
var ci = new VectorZi(y: c, nzIdx: cnz);
var y = new VectorZ(count: 7, initVal: 1.0);
var p = Sparse.Dot(x: ci, y: y); 
Printer.Write($"p = {p}");