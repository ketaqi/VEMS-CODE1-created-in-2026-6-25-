// parameters
var wavelength = 1550E-9;
var period = 790.5E-9;
var thickness = 1317E-9;
var matFront = new FuncMaterial(nReal: 1.5);
var matBehind = new FuncMaterial(nReal: 1.0);
var matEmbed = new FuncMaterial(nReal: 1.0);
var matFill = new FuncMaterial(nReal: 2.4);
// manually set sampling parameters
var nFields = RCWAHelper.DetermineSampling(wavelength: wavelength, period: period, 
    kSizeFactor: 7.5);
var nMedium = RCWAHelper.DetermineSampling(period: period, dx: 5.0E-9);

// meta-atom definition
var aRect = new MetaAtom1D.Rect(period: period, height: thickness,
    diameter: 0.5 * period, materialEmbed: matEmbed, materialFill: matFill);

// varying structure parameter(s)
var rMin = 0.1; // minimum fill ratio
var rMax = 0.9; // maximum fill ratio
var n = 64;
var g = new GridInfo1D(n: n, spacing: (rMax-rMin)/(n-1), start: rMin);
var effs = new VectorD(count: n);
var phis = new VectorD(count: n);

for(long i = 0; i < n; i++)
{
    // calculates width of the rectangle
    var ri = rMin + i * (rMax - rMin) / (n - 1);
    // updates parameter => fill factor
    aRect.Diameter = ri * period;
    // computes unit modulation
    (var eff, var phi) = aRect.ComputeModulation(wavelength: wavelength,
        front: matFront, behind: matBehind,
        isTransmission: true,
        polarization: InPlanePolMode.TM,
        fieldsSampling: nFields, 
        mediumSampling: nMedium);
    // adds to result
    effs[i] = eff;
    phis[i] = phi;
}

// display
var f = VFrame.CreateFrame();
VFrame.AddToFrame(f, values: effs * 100.0, grid: g, label: "Efficiency");
VFrame.AddToFrame(f, values: VMath.UnwrapPhase(phis, 0), grid: g, label: "Phase",
    plotColor: Options.PlotColor.SteelBlue,
    visualOption: Options.VisualOption.VisibleRight);
VFrame.SetFrameParameters(f, title: "Meta-Atom Property", xLabel: "fill factor", 
    yLabel: "Efficiency [%]");
VFrame.RefreshShow(f);