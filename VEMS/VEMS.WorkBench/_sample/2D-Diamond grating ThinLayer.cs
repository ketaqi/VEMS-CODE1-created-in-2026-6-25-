// basic parameters
var wavelength = 193E-9;
var incAngle = 0.0; // in degree, not used in this TEA example
var nIn = 1.0;
var nOut = 1.5593;

// grating parameters
var periodX = 6.364E-6;
var periodY = 6.364E-6;
var nEmbed = new Complex(1.0, 0.0);
var nPillar = new Complex(1.3282, 1.6637);

// diamond grating parameters
var diamondDiagonalX = periodX;
var diamondDiagonalY = periodY;

var thickness = 100E-9;


// ================================
// create diamond grating thin layer
// ================================

Complex DiamondGrating2D(double w, double x, double y)
{
    return DiamondRIndex(
        x, y,
        periodX, periodY,
        diamondDiagonalX, diamondDiagonalY,
        nEmbed, nPillar
    );
}

var medium = new Medium2D(n: DiamondGrating2D);

// calculate transmission function
var t = TEA2D.Compute(
    layer: medium,
    wavelength: wavelength,
    thickness: thickness,
    isPhaseOnly: false
);


// ================================
// sampling and display
// ================================

// numerical parameters
long nx = 99;
long ny = 99;

// 建议显示多个周期，例如 3 x 3 个周期
var displayPeriodsX = 3.0;
var displayPeriodsY = 3.0;

var dx = displayPeriodsX * periodX / (nx - 1);
var dy = displayPeriodsY * periodY / (ny - 1);

var gxy = new GridInfo2D(
    rows: ny,
    cols: nx,
    spacingY: dy,
    spacingX: dx,
    refPointY: -0.5 * displayPeriodsY * periodY,
    refPointX: -0.5 * displayPeriodsX * periodX,
    refTypeY: GridRefType.Start,
    refTypeX: GridRefType.Start
);

var txy = t.Sample(grid: gxy, loopMode: LoopMode.Parallel);
var txy1 = txy*0.81;

//VFrame.CreateShow(
//    values: txy1,
//    grid: gxy * (1E6, 1E6), // scaling to [um]
//    plotPart: ComplexPart.Magnitude,
//    title: "Diamond Grating Transmission [Phase]",
//    xLabel: "x [um]",
//    yLabel: "y [um]");

var TEx = new SCField(
    wavelength: wavelength,
    material: new FuncMaterial(nReal: 1.5593),
    uGrid: gxy,
    uValues: txy1,
    uPhase: null,
    shiftX: 0.0,
    shiftY: 0.0,
    shiftKx: 0.0,
    shiftKy: 0.0,
    scaling: 1.0,
    domain: ModelingDomain.Spatial
);

var d = 1000E-6; // 1000 um 
TEx.Propagate(d: d, targetDomain: ModelingDomain.Spatial, 
    loopMode: LoopMode.Parallel); // this method calls SPW internally
var det = new Detector2D(grid: gxy);
var vOut = det.Sample(v: TEx, quantity: DetectQuantity.Magnitude);

VFrame.CreateShow(values: vOut, grid: gxy, title: "Propagated Field Ex(x, y)");



// ================================
// diamond grating R-index definition
// ================================

private static Complex DiamondRIndex(
    double x,
    double y,
    double periodX,
    double periodY,
    double diamondDiagonalX,
    double diamondDiagonalY,
    Complex nEmbed,
    Complex nPillar)
{
    // map x, y into one centered unit cell:
    // x in [-periodX/2, periodX/2)
    // y in [-periodY/2, periodY/2)
    x = x - periodX * Math.Floor(x / periodX + 0.5);
    y = y - periodY * Math.Floor(y / periodY + 0.5);

    double rx = 0.5 * diamondDiagonalX;
    double ry = 0.5 * diamondDiagonalY;

    // diamond condition:
    // |x|/rx + |y|/ry <= 1
    bool insideDiamond = Math.Abs(x) / rx + Math.Abs(y) / ry <= 1.0;

    if (insideDiamond)
        return nPillar;
    else
        return nEmbed;
}
