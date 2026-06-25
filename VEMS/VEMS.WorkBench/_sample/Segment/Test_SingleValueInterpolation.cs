double dx = 0.1;
VectorZ x = new VectorZ(count: 3, initVal: 1.0, increment: 1.0);
GridInfo1D g = new GridInfo1D(n: x.Count, spacing: dx);
Transform.FFT(x, grid: g, opt: FFTOption.Forward);
VFrame.CreateShow(x, g, title: $"reference");


VectorZ x1 = new VectorZ(count: 1, initVal: 1.0);
GridInfo1D g1 = new GridInfo1D(n: 1, spacing: 0.1);
Transform.FFT(x1, g1, opt: FFTOption.Forward);

VectorZ x2 = new VectorZ(count: 1, initVal: 2.0);
GridInfo1D g2 = new GridInfo1D(n: 1, spacing: 0.1);
Transform.FFT(x2, g2, opt: FFTOption.Forward);

VectorZ x3 = new VectorZ(count: 1, initVal: 3.0);
GridInfo1D g3 = new GridInfo1D(n: 1, spacing: 0.1);
Transform.FFT(x3, g3, opt: FFTOption.Forward);


VectorZ sum = new VectorZ(x.Count);
x1 = x1.Padding(targetCount: x.Count, startIndex: 0, paddingValueRe: x1[0].Real, paddingValueIm: x1[0].Imaginary);
x2 = x2.Padding(targetCount: x.Count, startIndex: 0, paddingValueRe: x2[0].Real, paddingValueIm: x2[0].Imaginary);
x3 = x3.Padding(targetCount: x.Count, startIndex: 0, paddingValueRe: x3[0].Real, paddingValueIm: x3[0].Imaginary);
Printer.Write("padding finished");
for(int i = 0; i < x.Count; i++)
{
    double kx = g[i]; 
    Printer.Write("get kx");
    x1[i] *= Complex.Exp(Complex.ImaginaryOne * kx * (+1.0) * dx);
    x2[i] *= Complex.Exp(Complex.ImaginaryOne * kx * 0.0 * dx);
    x3[i] *= Complex.Exp(Complex.ImaginaryOne * kx * (-1.0) * dx);
    sum[i] = x1[i] + x2[i] + x3[i];
}
VFrame.CreateShow(sum, g, title: "test");