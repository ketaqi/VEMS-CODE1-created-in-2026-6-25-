//Polarization Mode
var PolarizationMode = "TE";

//Basis Function Order of FEM
var BasisFuncOrder = 2;

//Number of Calculation Points of Gaussian Quadrature Formula
var GaussPoints = 2;

//Permeability μ
var Mu = 1;

//Refraction Index n
var n = new VectorD(2);
n[0] = 1.458;
n[1] = 1.462;

//Wavelength λ
var lambda = 1.31e-6;

//k0
var k0 = 2 * Math.PI / lambda;

//lengths of the Middle Media
var lengthsMiddle = 8.2e-6;

//lengths of the Whole Media
var lengthsWhole = 8 * lengthsMiddle;

//Number of Elements in the Meddle Media
long nxMiddle = 32;

//lengths of the Element in the Meddle Media
var dxMiddle = lengthsMiddle / nxMiddle;

//Number of Elements outside the Meddle Media(round down)
long nxOutside = (long)(2 * Math.Floor((lengthsWhole - lengthsMiddle) / 2 / dxMiddle));

//lengths of the Element outside the Meddle Media
var dxOutside = (lengthsWhole - lengthsMiddle) / nxOutside;

//Number of Elements
long nxElement = nxMiddle + nxOutside;

//Number of Element Nodes
long nxNodes = nxElement + 1;

//Number of Claculation Element Nodes
long nxCalNodes = BasisFuncOrder * nxElement + 1;

//Analytic solution
var TE = new VectorD(2);
var TM = new VectorD(2);
TE[0] = 7007525.111;
TE[1] = 6995854.244;
TM[0] = 7007511.442;
TM[1] = 6995839.746;

//Coefficient in A and B
Func<int, double> CoefficientFuncA1;
Func<int, double> CoefficientFuncA2;
Func<int, double> CoefficientFuncB1;

if (PolarizationMode == "TE")
{
    CoefficientFuncA1 = (x) => 1;
    CoefficientFuncA2 = (x) => Mesh.Epislon[x] * Mu * k0 * k0;
    CoefficientFuncB1 = (x) => 1;
}
else if (PolarizationMode == "TM")
{
    CoefficientFuncA1 = (x) => 1 / Mesh.Epislon[x];
    CoefficientFuncA2 = (x) => Mu * k0 * k0;
    CoefficientFuncB1 = (x) => 1 / Mesh.Epislon[x];
}

//Coordination of the nodes in the Mesh
var MeshCoordination = new VectorD(nxNodes);
MeshCoordination[0] = -lengthsWhole / 2;

//Epsilon in each element
var Epsilon = new VectorD(nxElement);

for (int i = 0; i < nxElement; i++)
{
    if (i < nxOutside / 2 || i >= nxOutside / 2 + nxMiddle)
    {
        Epsilon[i] = n[0] * n[0];
        MeshCoordination[i + 1] = MeshCoordination[i] + dxOutside;
    }
    else
    {
        Epsilon[i] = n[1] * n[1];
        MeshCoordination[i + 1] = MeshCoordination[i] + dxMiddle;
    }
}

//Generate the index of the mesh nodes
var Mesh = new FEMMesh1D(BasisFuncOrder, MeshCoordination, Epsilon);

//Initial the FEM matrix
var A1 = new FEM1D(Mesh,CoefficientFuncA1,1,2);
var A2 = new FEM1D(Mesh,CoefficientFuncA2,0,2);
var B1 = new FEM1D(Mesh,CoefficientFuncB1,0,2);

//Matrix assembly
A1.AssembleMatrix();
A2.AssembleMatrix();
B1.AssembleMatrix();

var A = - A1.A + A2.A;
var B = B1.A;

//Solve the eigen ploblem
var EigenValues = new VectorZ(nxCalNodes);
var EigenVectors = new MatrixD(nxCalNodes,nxCalNodes);

LinAlg.GeneralizedEigenSystem(ref A,ref B, out EigenValues, out EigenVectors);

var SqrtEigenValues = VMath.Sqrt(EigenValues);

//Find the Kz
var Kz = new VectorD(nxCalNodes);
var v = new MatrixD(nxCalNodes,nxCalNodes);
var err = new VectorD(nxCalNodes);
int nKz = 0;

for (int i = 0; i < nxCalNodes; i++)
{
    if (SqrtEigenValues[i].Real > k0*n[0] && SqrtEigenValues[i].Real < k0*n[1])
    {
        Kz[nKz] = SqrtEigenValues[i].Real;
        v[new LongRange(0, v.Rows), nKz] = EigenVectors[new LongRange(0, EigenVectors.Rows), i];
        nKz++;
    }
}

//Calculate the Error
if (PolarizationMode == "TE")
{
    err[0] = Math.Abs((Kz[0] - TE[0])/TE[0]);
    err[1] = Math.Abs((Kz[1] - TE[1])/TE[1]);
}
else if (PolarizationMode == "TM")
{
    err[0] = Math.Abs((Kz[0] - TM[0])/TM[0]);
    err[1] = Math.Abs((Kz[1] - TM[1])/TM[1]);
}

//Show the figure
var Data0 = new VectorD(v[new LongRange(0, v.Rows), 0]);
var Data1 = new VectorD(v[new LongRange(0, v.Rows), 1]);
VFrame.CreateShow(values:Data0,
    title:PolarizationMode.ToString()+"0:"+Kz[0].ToString("F3")+"    Err:"+err[0].ToString());
VFrame.CreateShow(values:Data1,
    title:PolarizationMode.ToString()+"1:"+Kz[1].ToString("F3")+"    Err:"+err[1].ToString());