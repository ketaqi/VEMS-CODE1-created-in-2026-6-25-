// initializes aberration as transmission
var a = new Transmission2D();
// define Zernike terms
var phases = new List<Func<double, double, double>>();
var phase_count = 37;
var scaling = new VectorD(count: phase_count, initVal: 0.3);

//var scaling = new VectorD(9, 0.0)
//{
//    [0] = 0.0,[1] = 0.0,[2] = 0.0,
//    [3] = 1.0,[4] = 0.0,[5] = 0.0,
//    [6] = 0.0,[7] = 1.0,[8] = 0.0
//};

// go through the top ten items
for (int idx = 0; idx <= phase_count - 1; idx++)
{
    var zernike = new Aberration2D.Zernike(
    idx: idx,
    indexing: CommonFunction.ZernikeIndexing.Standard,
    scaling: scaling[idx]);
    phases.Add(zernike.Phase);
}

a.Phase = a.CombinePhase(phases, useParallel: false);

// samples on target grid with 2.0 diameter along x and y
var nRow = 251;
var nCol = 251;
var g = new GridInfo2D(rows: nRow, cols: nCol,
    spacingY: 2.0 / (nRow - 1), spacingX: 2.0 / (nCol - 1));
var v = a.SampleP(grid: g, loopMode: LoopMode.Parallel);
// display 
VFrame.CreateShow(values: v, grid: g);


// generating aberration with multiple Zernike terms
var m = phase_count;
var indxs = new List<int>();
var scals = new List<double>();
for(int i = 0; i < m; i++)
{ indxs.Add(i); scals.Add(0.3); }
var mz = new Aberration2D.Zernike(indices: indxs, 
    indexing: CommonFunction.ZernikeIndexing.Standard,
    scalings: scals);
VFrame.CreateShow(values: mz.SampleP(g), grid: g, 
    title: $"Aberration with Mutliple Zernikes");