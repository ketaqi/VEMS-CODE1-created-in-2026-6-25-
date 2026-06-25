var x = new VectorZ(count: 5, initVal: 1.0, increment: 1.0);

VEMS.MathCore.Vector<Complex> t = x;
FFTKernel.PeriodicShift(x: ref t, n: -2, useBlockCopy: true);
for(long i = 0; i < t.Count; i++)
    Printer.Write($"t[{i}] = {t[i]}");
