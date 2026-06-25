// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using VEMS.AMathCore;
using Int = System.Int64;
using Real = System.Double;

Console.WriteLine("VEMS TestConsole [AMathCore]\n");


Printer.WriteLine("==== Sparse Vector Test ====");
var nzi = new Vect<Int>(4) { [0] = 2, [1] = 5, [2] = 6, [3] = 9 };
var nzv = new Vect<Real>(4) { [0] = 1.5, [1] = 2.5, [2] = -1.0, [3] = 0.0 };
var yi = SPVect<Real>.Create(n: 10, nnz: 4, nzIdx: nzi, nzVal: nzv);
Printer.Write(yi.NzIndices, $"Non-zero indices: ");
Printer.Write(yi.NzValues, $"Non-zero values: ");

Sparse.Scal(a: 2.0, x: ref yi);
Printer.Write(yi.NzValues, $"Non-zero values (scaled): ");

var x = Vect<Real>.Create(n: 10, x0: 1.0, dx: 0.1);
Printer.Write(x, $"Dense vector x: ");

var doti = Sparse.Dot(yi, x);
Printer.WriteLine($"Dot product of sparse vector yi and dense vector x = {doti}");

Printer.WriteLine($"==== End of Sparse Vector Test ====");
Printer.WriteLine($"\n");


Printer.WriteLine("==== Sparse Matrix Test ====");
var rowPtr = new DenseArray<Int>(6) { [0] = 0, [1] = 3, [2] = 5, [3] = 8, [4] = 11, [5] = 13 };
var colIdx = new DenseArray<Int>(13) {
    [0] = 0, [1] = 1, [2] = 3,
    [3] = 0, [4] = 1,
    [5] = 2, [6] = 3, [7] = 4,
    [8] = 0, [9] = 2, [10] = 3,
    [11] = 1, [12] = 4
};
var val = new DenseArray<Real>(13) {
    [0] = 1.0, [1] = -1.0, [2] = -3.0,
    [3] = -2.0, [4] = 5.0,
    [5] = 4.0, [6] = 6.0, [7] = 4.0,
    [8] = -4.0, [9] = 2.0, [10] = 7.0,
    [11] = 8.0, [12] = -5.0
};

var ai = SPMatx<Real>.CreateCSR(rows: 5, cols: 5, nnz: 13, 
    rowPtr: rowPtr, colIdx: colIdx, nzVal: val);
var x1 = Vect<Real>.Create(n: 5, x0: 1.0);
Printer.Write(x1, $"Vector x1: ");
var y1 = new Vect<Real>(count: 5);
Sparse.Dot(ai, x1, ref y1, alpha: 1.0);
Printer.Write(y1, $"Product of sparse matrix ai and vector x1: ");

var diag = Vect<Real>.Create(n: 5, x0: 1.0, dx: 1.0);
var bi = SPMatx<Real>.Diagonal(diag); 
var ci = new SPMatx<Real>(rows: 5, cols: 5, nnz: 13);
//Sparse.Add(ai, bi, ref ci);
var di = new SPMatx<Real>();
Sparse.Dot(ai, bi, ref di);
Printer.Write(di.Scatter(), $"Sparse matrix ai * bi = ");
Printer.WriteLine($"\n");
//Sparse.SetValue(ref di, 0, 1, 3.333);
//Printer.Write(di.Scatter(), $"Sparse matrix di (after SetValue) = ");

var x2 = Vect<Real>.Create(n: 5, x0: 0.5);
var y2 = new Vect<Real>(count: 5);
Sparse.Dot(di, x2, ref y2);

Printer.Write(y2, $"Product of sparse matrix ci and vector x2: ");
Printer.WriteLine($"\n");
Printer.WriteLine($"==== End of Sparse matrix tests ====");
Printer.WriteLine($"\n");


Printer.WriteLine("==== PARDISO Test ====");
var valz = new DenseArray<Cplx>(13)
{
    [0] = new Cplx(1.0, 0.5), [1] = -1.0, [2] = -3.0,
    [3] = new Cplx(-2.0, 0.7), [4] = 5.0,
    [5] = new Cplx(4.0, 0.1), [6] = 6.0, [7] = 4.0,
    [8] = new Cplx(-4.0, 0.2), [9] = 2.0, [10] = 7.0,
    [11] = new Cplx(8.0, 0.3), [12] = -5.0
};
var aiz = SPMatx<Cplx>.CreateCSR(rows: 5, cols: 5, nnz: 13,
    rowPtr: rowPtr, colIdx: colIdx, nzVal: valz);
long n = 5;
var b = Vect<Cplx>.Create(n, x0: new Cplx(1.0, 3.0));
var y = new Vect<Cplx>(count: n, initMode: ArrayInitMode.Calloc);
var pardiso = new Pardiso<Cplx>();
pardiso.Solve(aiz, b, ref y);
Printer.Write(y, $"Pardiso solution vector = ");
var t = new Vect<Cplx>(count: n, initMode: ArrayInitMode.Calloc);
Sparse.Dot(aiz, y, ref t);
Printer.Write(t, $"Pardiso check b = A * x = ");
Printer.WriteLine($"==== End of Pardiso tests ====");
Printer.WriteLine($"\n");


Printer.WriteLine("==== FGMRES Test ====");
var ad = new Matx<Cplx>(rows: 5, cols: 5)
{
    [0, 0] = new Cplx(1.0, 0.5), [0, 1] = new Cplx(-1.0, 0.0), [0, 3] = new Cplx(-3.0, 0.0),
    [1, 0] = new Cplx(-2.0, 0.7), [1, 1] = new Cplx(5.0, 0.0),
    [2, 2] = new Cplx(4.0, 0.1), [2, 3] = new Cplx(6.0, 0.0), [2, 4] = new Cplx(4.0, 0.0),
    [3, 0] = new Cplx(-4.0, 0.2), [3, 2] = new Cplx(2.0, 0.0), [3, 3] = new Cplx(7.0, 0.0),
    [4, 1] = new Cplx(8.0, 0.3), [4, 4] = new Cplx(-5.0, 0.0)
};
var rd = new Vect<Cplx>(count: 5, initMode: ArrayInitMode.Calloc);
var fgmres = new Fgmres<Cplx>(n: 5, relTolerance: 1.0E-4);
fgmres.Solve(aiz, b, ref rd);
//fgmres.Solve(ad, b, ref rd);
Printer.Write(rd, $"FGMRES solution vector = ");
LinAlg.Dot(ad, rd, ref t);
Printer.Write(t, $"FGMRES check b = A * x = ");


Printer.WriteLine($"==== End of FGMRES tests ====");
Printer.WriteLine($"\n");


Printer.WriteLine("==== Large matrix tests ====");
// from Shuang LIU
var rng = new Random(123);  // 固定种子以便重现
Int N = 9;
Printer.WriteLine($"Generating a sparse complex matrix of size {N} x {N} ...");

// 1. 生成对角线偏移量（包含0）
Int lowerBound = (int)(-2.0 * N / 3.0);
Int upperBound = (int)(2.0 * N / 3.0);

var offsets = new List<Int> { -6, 0, 3 }; // 确保包含主对角线

//while (offsets.Count < 3)  // 总共8个对角线（包括主对角线）
//{
//    int k = rng.Next(lowerBound, upperBound + 1);
//    if (k != 0)
//    {
//        offsets.Add(k);
//    }
//}

var sortedOffsets = offsets.OrderBy(x => x).ToArray();
for(int i = 0; i < sortedOffsets.Length; i++)
    Printer.WriteLine($" Offset {i}: {sortedOffsets[i]}");
Printer.WriteLine($" 1. 生成对角线偏移量");


Int nnz = 0;
for (int i = 0; i < sortedOffsets.Length; i++)
    nnz += (N - Math.Abs(sortedOffsets[i]));
var largeRowPtr = new DenseArray<Int>(N + 1);
var largeColIdx = new DenseArray<Int>(nnz);
var largeVal = new DenseArray<Cplx>(nnz);

// loop over rows
long iNNZ = 0;
for(long iRow = 0; iRow < N; iRow++)
{
    long nnzInRow = 0;
    // checks offsets
    for (int k = 0; k < sortedOffsets.Length; k++)
    { 
        Int iCol = iRow + sortedOffsets[k];
        // checks ranges of the offset column
        if (iCol >= 0 && iCol < N)
        {
            // fills the colIdx
            largeColIdx[iNNZ] = iCol;

            if (iCol == iRow)
                largeVal[iNNZ] = new Cplx(5.0, 0.0); // condition number ...
            else
                largeVal[iNNZ] = new Cplx(rng.NextDouble(), rng.NextDouble());

            // increments
            nnzInRow++;
            iNNZ ++;
        }
        // fills the rowPtr
        largeRowPtr[iRow + 1] = largeRowPtr[iRow] + nnzInRow;
    }
}

Printer.Write(largeRowPtr, $"largeRowPtr: ");
Printer.Write(largeColIdx, $"largeColIdx: ");


// 2. 收集CSR数据
var rowPtrList = new List<Int> { 0 };
var colIdxList = new List<Int>();
var valList = new List<Cplx>();  // 复数数值列表

for (long i = 0; i < N; i++)
{
    //var rowEntries = new List<(Int col, Cplx val)>();
    //var colEntry = new List<Int>();
    //var valEntry = new List<Cplx>();

    // 遍历所有偏移量（包括0）
    foreach (var k in sortedOffsets)
    {
        long j = i + k;
        if (j >= 0 && j < N)
        {
            Cplx value;
            if (k == 0)
            {
                // 主对角线：固定为 1.0 + 0.0i
                value = new Cplx(5.0, 0.0); // condition number ...
            }
            else
            {
                // 其他对角线：随机复数（实部和虚部都在0-1之间）
                value = new Cplx(rng.NextDouble(), rng.NextDouble());
            }

            //rowEntries.Add(((Int)j, value));
            colIdxList.Add((Int)j);
            valList.Add(value);
        }
    }

    // 按列索引升序排列
    //rowEntries.Sort((a, b) => a.col.CompareTo(b.col));

    // 添加到全局列表
    //foreach (var e in rowEntries)
    //{
    //    colIdxList.Add(e.col);
    //    valList.Add(e.val);
    //}
    //Printer.WriteLine($" Processing row {i + 1} / {N}");
    //Printer.WriteLine($"colIdxList: {colIdxList.Count}");
    rowPtrList.Add((Int)colIdxList.Count);
}
Printer.WriteLine($" 2. 收集CSR数据");

Printer.WriteLine($"rowPtrList count = {rowPtrList.Count}");
for (int i = 0; i < rowPtrList.Count; i++)
    Printer.WriteLine($" rowPtrList[{i}] = {rowPtrList[i]}");

Printer.WriteLine($"colIdxList count = {colIdxList.Count}");
for (int i = 0; i < colIdxList.Count; i++)
    Printer.WriteLine($" colIdxList[{i}] = {colIdxList[i]}");




/*
// 3. 转换为DenseArray并创建稀疏矩阵
Int nnz = colIdxList.Count;
var largeRowPtr = new DenseArray<Int>(rowPtrList.Count);
var largeColIdx = new DenseArray<Int>(nnz);
var largeVal = new DenseArray<Cplx>(nnz);

Printer.WriteLine($"rowPtrList: {rowPtrList.Count}");
for (Int i = 0; i < rowPtrList.Count; i++) largeRowPtr[i, false] = rowPtrList[(int)i];
for (Int i = 0; i < nnz; i++)
{
    largeColIdx[i, false] = colIdxList[(int)i];
    largeVal[i, false] = valList[(int)i];
}
Printer.WriteLine($" 3. 转换为DenseArray并创建稀疏矩阵");

// 创建复数稀疏矩阵
var largeAi = SPMatx<Cplx>.CreateCSR(rows: N, cols: N, nnz: nnz,
    rowPtr: largeRowPtr, colIdx: largeColIdx, nzVal: largeVal);

Printer.WriteLine($"已生成 {N}x{N} 的复数稀疏矩阵，非零元数 = {nnz}");
Printer.WriteLine($"对角线偏移量: {string.Join(", ", sortedOffsets)}");
var largeB = Vect<Cplx>.Create(N, x0: new Cplx(1.0, 1.0));
var largeY = new Vect<Cplx>(count: N, initMode: ArrayInitMode.Calloc);

// ---- Pardiso 求解计时 ----
var swP = Stopwatch.StartNew();

var largePardiso = new Pardiso<Cplx>();
largePardiso.Solve(largeAi, largeB, ref largeY);

swP.Stop();
Printer.WriteLine($"Pardiso 求解耗时: {swP.Elapsed.TotalMilliseconds:F3} ms");
//Printer.Write(largeY, $"Pardiso solution vector = ");


// ---- FGMRES 求解计时 ----
var swF = Stopwatch.StartNew();

var largeR = new Vect<Cplx>(count: N, initMode: ArrayInitMode.Calloc);
Printer.WriteLine("FGMRES constructor ...");
var largeFgmres = new Fgmres<Cplx>(n: N, 
    relTolerance: 1.0E-6);
largeFgmres.Solve(largeAi, largeB, ref largeR);

swF.Stop();
Printer.WriteLine($"FGMRES 求解耗时: {swF.Elapsed.TotalMilliseconds:F3} ms");

// difference
DenseArray<Cplx> largeT = largeY;
VMath.Scal(a: -1.0, x: ref largeT);
VMath.Add(a: 1.0, x: largeR, y: ref largeT);
Printer.WriteLine($"Norm of error: {VMath.Norm(largeT)}");
Printer.WriteLine($"\n");
*/

Printer.WriteLine("==== Sparse QR Test ====");
Printer.Write(ai.Scatter(), $"ai = ");
Sparse.QR_Reorder(ai);
