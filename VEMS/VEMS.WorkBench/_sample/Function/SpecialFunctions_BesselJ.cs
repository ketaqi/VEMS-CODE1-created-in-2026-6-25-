var b = new Grid1DRealData(count: 267, spacing: 0.1, initVal: 0.0);
for(long i = 0; i<b.GridInfo.Count; i++)
{
    var x = b.GridInfo.GetCoordinate(i);
    b.Values[i] = SpecialFunctions.BesselJ(n: 1, z: x);
}
VFrame.CreateShow(b);