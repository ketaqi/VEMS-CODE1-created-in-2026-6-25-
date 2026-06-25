// parameters
var wavelength = 1.31E-06;
var polarization = InPlanePolMode.TE;

var nclad = 1.458;
var ncore = 1.462;
double width = 8.2E-06;
var k0 = 2.0 * Math.PI / wavelength;

var grid = new GridInfo1D(551, 1E-07);

//=========anlaytical method==============
var slab = new SlabWaveguide(wavelength, polarization, ncore, nclad, width);

//calculate the select field mode
slab.ComputeWaveguideMode(0, grid);
var TEfield1 = slab.Field;
VFrame.CreateShow(TEfield1, title: " select: TE0");

//calculate all field mode
for (int i = 0; i < slab.M; i++)
{
    slab.ComputeWaveguideMode(i, grid);
    var TEfield1 = slab.Field;
    VFrame.CreateShow(TEfield1, title: $" TE{i}");
}