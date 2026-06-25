//一个非球面光线追迹案例
//定义高阶非球面的系数
VectorD co=new VectorD(1)
{
    [0]=2.32504754288e-009,
};
Asphere a=new Asphere(-82.6696785676,-4.11808129846,co);

//光线在上一表面的坐标系下的出射点，方向向量（在光线追迹程序中，
//通常上一个表面的输出结果会作为下一个表面的输入）
VecD3 rIn0 = new VecD3( 5.3694879645857, 120.8341202467397 , -24.4893801111658);
VecD3 sIn0 = new VecD3( 0.0118487833011,-0.6570018863526,-0.7537958129782);
//将上一个表面的输出转换到当前局部坐标系
VecD3 rIn = CoordinateSystemTransform.CoordinateTranslationPoint(rIn0,new VecD3(){X=0.0,Y=2.37838474446,Z=0.0-135.264753441},
new VecD3(){X=6.499696704,Y=0.0,Z=0.0});
VecD3 sIn = CoordinateSystemTransform.CoordinateTranslationVector(sIn0,new VecD3(){X=0.0,Y=2.37838474446,Z=0.0},
new VecD3(){X=6.499696704,Y=0.0,Z=0.0});
//Printer.Write($"rIn: X = {rIn.X}, Y = {rIn.Y}, Z = {rIn.Z}.");
//Printer.Write($"sIn: X = {sIn.X}, Y = {sIn.Y}, Z = {sIn.Z}.");
sIn.Normalize();
//计算交点
a.Intersect(rIn,sIn,out VecD3 rOut,out double p);
Printer.Write($"rOut: X = {rOut.X}, Y = {rOut.Y}, Z = {rOut.Z}.");
new VecD3 norm;
//交点处的法线向量
norm = a.ComputeNormal(rOut);
//Printer.Write($"norm: X = {norm.X}, Y = {norm.Y}, Z = {norm.Z}.");
new VecD3 sOut;
//计算反射方向向量
sOut= Conic.Reflect(sIn,norm);
Printer.Write($"sOut: X = {sOut.X}, Y = {sOut.Y}, Z = {sOut.Z}.");
