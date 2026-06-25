// parameters
var wavelength = 1550E-9;
var gaussWaist = 0.8 * wavelength;
var period = 790.5E-9;
var height = 1317.0E-9;
var embed = new FuncMaterial(nReal: 1.0);
var fill = new FuncMaterial(nReal: 2.4);
var front = new FuncMaterial(nReal: 1.5);
var behind = new FuncMaterial(nReal: 1.0);

// constructs meta-layer
var unit = 5;
var grids = new GridInfo1D(n: 3 * unit, spacing: period);
var atoms = new List<MetaAtom1D>();
for (int i = 0; i < grids.Count / unit; i++)
{
    atoms.Add(new MetaAtom1D.Rect(period, height, 0.10 * period, embed, fill));
    atoms.Add(new MetaAtom1D.Rect(period, height, 0.30 * period, embed, fill));
    atoms.Add(new MetaAtom1D.Rect(period, height, 0.50 * period, embed, fill));
    atoms.Add(new MetaAtom1D.Rect(period, height, 0.70 * period, embed, fill));
    atoms.Add(new MetaAtom1D.Rect(period, height, 0.90 * period, embed, fill));
}
var metaLayer = new Grid1DMetaLayer(gridPoints: grids, height: height, metaAtoms: atoms);
var ml = new Grid1DMetaLayer(gridPoints: grids, height: height, metaAtoms: atoms,
    periodicBoundary: true);

// manually set sampling parameters
var nMedium = RCWAHelper.DetermineSampling(period: metaLayer.GridPoints.Range, dx: 5.0E-9);
var gIn = new GridInfo1D(n: nMedium, spacing: metaLayer.GridPoints.Range / nMedium);
var mIn = metaLayer.Sample(wavelength: wavelength, grid: gIn, matProperty: MaterialProperty.N);
VFrame.CreateShow(values: mIn, grid: gIn, title: "Structure Medium (N)");


// segmentation of the layer medium
var dIn = new Grid1DCplxData(values: mIn, gridInfo: gIn, 
    intrpl: InterpolationMethod.Nearest, bound: DataBoundary.Periodic);
var ns = 5;
var centers = new GridInfo1D(n: ns, spacing: ml.GridPoints.Range / ns);
var uniSEG = new UniformSEG1D.CosRect(centers, diameter: 7.0 * period);
// takes segments out ...
var nt = 51;
var dt = 3.0 * period / nt;
var ds = uniSEG.TakeEachFrom(dIn, nt, dt);
// check results
var f = VFrame.CreateFrame();
for(int i = 0; i < ds.Count; i++)
{
    VFrame.AddToFrame(f, values: ds[i].Values, grid: ds[i].GridInfo, 
        plotPart: ComplexPart.Magnitude, 
        plotColor: Options.PlotColor.SteelBlue + i,
        label: $"Segment [{i}]");
}
VFrame.RefreshShow(f);