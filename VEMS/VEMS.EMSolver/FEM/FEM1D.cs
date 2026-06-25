using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;
using VEMS.MathCore;

namespace VEMS.EMSolver
{
    /// <summary>
    /// genarate 1D FEM Matrix
    /// </summary>
    public class FEM1D
    {

        #region properties

        /// <summary>
        /// Mesh of the 1D FEM
        /// </summary>
        public FEMMesh1D Mesh { get; set; }

        /// <summary>
        /// Matrix of the 1D FEM
        /// </summary>
        public MatrixD A { get; set; }

        /// <summary>
        /// Coefficient function of the FEM
        /// </summary>
        public Func<int,double> CoefficientFunc { get; set; }

        /// <summary>
        /// Type of the 1D FEM Matrix basis function
        /// </summary>
        public int NType { get; set; }

        /// <summary>
        /// Number of nodes in Gaussian quadrature formula   
        /// </summary>
        public int NGaussian { get; set; }

        #endregion

        #region constructors
        /// <summary>
        /// constructor of 1D FEM Matrix
        /// </summary>
        /// <param name="mesh">Mesh of the 1D FEM</param>
        /// <param name="coefficientFunc">Coefficient function of the FEM</param>
        /// <param name="nType">Order of the 1D FEM basis function</param>
        /// <param name="nGaussian">Number of nodes in Gaussian quadrature formula</param>
        public FEM1D(FEMMesh1D mesh, Func<int, double> coefficientFunc, int nType, int nGaussian)
        {

            Mesh = mesh;
            CoefficientFunc = coefficientFunc;
            NType = nType;
            NGaussian = nGaussian;
            A = new MatrixD(Mesh.CalculationNodesCoordination.Count, Mesh.CalculationNodesCoordination.Count);
        
        }
        #endregion

        #region methods
        /// <summary>
        /// Assemble the FEM Matrix
        /// </summary>
        /// <returns>Result of the FEM</returns>
        public void AssembleMatrix()
        {

            for (int n = 0; n < Mesh.NodesCoordination.Count - 1; n++)
            {

                var v = new VectorD(Mesh.BasisFunctionOrder + 1);
                for (int vi = 0; vi <= Mesh.BasisFunctionOrder; vi++)
                {
                    v[vi] = Mesh.CalculationNodesCoordination[Mesh.CalculationNodesIndex[vi, n] - 1];
                }

                for (int j = 0; j <= Mesh.BasisFunctionOrder; j++)
                {

                    for (int i = 0; i <= Mesh.BasisFunctionOrder; i++)
                    {

                        A[Mesh.CalculationNodesIndex[i, n] - 1, Mesh.CalculationNodesIndex[j, n] - 1] =
                            A[Mesh.CalculationNodesIndex[i, n] - 1, Mesh.CalculationNodesIndex[j, n] - 1] +
                            CoefficientFunc(n) * ElementTrailTest(v, i, j, NType, NGaussian, Mesh.BasisFunctionOrder);

                    }
                }
            }
        }

        /// <summary>
        /// local Trail and Test function calculation in a FEM Element
        /// </summary>        
        private double ElementTrailTest(VectorD v, int trailIndex, int testIndex, int nType, int nGaussian, int basisFunctionOrder)
        {
            double result = 0;
            
            VectorD vGauss = new(2);
            vGauss[0] = v[0];
            vGauss[1] = v[v.Count - 1];
            (VectorD GaussWeight, VectorD GaussPoint) = GenrateGaussFormular(vGauss, nGaussian);

            for (int i = 0; i < GaussPoint.Count; i++)
            {
                result +=  GaussWeight[i] * LocalBasisFuncTest(GaussPoint[i], v, trailIndex, basisFunctionOrder, nType)
                                          * LocalBasisFuncTest(GaussPoint[i], v, testIndex, basisFunctionOrder, nType);
            }
            return result;
        }


        /// <summary>
        /// Local Basis Function calculation with Gaussian quadrature formula
        /// </summary>

        private double LocalBasisFunc(double x, VectorD v, int basisIndex, int basisFunctionOrder, int nType)
        {
            double h = v[1] - v[0];
            if (basisFunctionOrder == 1)
            {
                if (basisIndex == 0)
                {
                    if (nType == 0)
                    {
                        return (v[1] - x) / h;
                    }
                    else if (nType == 1)
                    {
                        return -1 / h;
                    }
                    else
                    {
                        throw new ArgumentException("Invalid der value for basisType 1 and basisIndex 1");
                    }
                }
                else if (basisIndex == 1)
                {
                    if (nType == 0)
                    {
                        return (x - v[0]) / h;
                    }
                    else if (nType == 1)
                    {
                        return 1 / h;
                    }
                    else
                    {
                        throw new ArgumentException("Invalid der value for basisType 1 and basisIndex 2");
                    }
                }
                else
                {
                    throw new ArgumentException("Invalid basisIndex for basisType 1");
                }
            }
            else if (basisFunctionOrder == 2)
            {
                if (basisIndex == 0)
                {
                    if (nType == 0)
                    {
                        return 2 * Math.Pow((x - v[0]) / h, 2) - 3 * (x - v[0]) / h + 1;
                    }
                    else if (nType == 1)
                    {
                        return 4 * (x - v[0]) / (h * h) - 3 / h;
                    }
                    else
                    {
                        throw new ArgumentException("Invalid der value for basisType 2 and basisIndex 1");
                    }
                }
                else if (basisIndex == 1)
                {
                    if (nType == 0)
                    {
                        return -4 * Math.Pow((x - v[0]) / h, 2) + 4 * (x - v[0]) / h;
                    }
                    else if (nType == 1)
                    {
                        return -8 * (x - v[0]) / (h * h) + 4 / h;
                    }
                    else
                    {
                        throw new ArgumentException("Invalid der value for basisType 2 and basisIndex 2");
                    }
                }
                else if (basisIndex == 2)
                {
                    if (nType == 0)
                    {
                        return 2 * Math.Pow((x - v[0]) / h, 2) - (x - v[0]) / h;
                    }
                    else if (nType == 1)
                    {
                        return 4 * (x - v[0]) / (h * h) - 1 / h;
                    }
                    else
                    {
                        throw new ArgumentException("Invalid der value for basisType 2 and basisIndex 3");
                    }
                }
                else
                {
                    throw new ArgumentException("Invalid basisIndex for basisType 2");
                }
            }
            else
            {
                throw new ArgumentException("Invalid basisType");
            }
        }
        /// <summary>
        /// Local Basis Function calculation with Gaussian quadrature formula (for test)
        /// </summary>

        private double LocalBasisFuncTest(double x, VectorD v, int basisIndex, int basisFunctionOrder, int nType)
        {
            
            if (basisFunctionOrder == 1)
            {
                double h = v[1] - v[0];

                if (basisIndex == 0)
                {
                    if (nType == 0)
                    {
                        return (v[1] - x) / h;
                    }
                    else if (nType == 1)
                    {
                        return -1 / h;
                    }
                    else
                    {
                        throw new ArgumentException("Invalid der value for basisType 1 and basisIndex 1");
                    }
                }
                else if (basisIndex == 1)
                {
                    if (nType == 0)
                    {
                        return (x - v[0]) / h;
                    }
                    else if (nType == 1)
                    {
                        return 1 / h;
                    }
                    else
                    {
                        throw new ArgumentException("Invalid der value for basisType 1 and basisIndex 2");
                    }
                }
                else
                {
                    throw new ArgumentException("Invalid basisIndex for basisType 1");
                }
            }
            else if (basisFunctionOrder == 2)
            {
                if (basisIndex == 0)
                {
                    if (nType == 0)
                    {
                        return ((x - v[1]) * (x - v[2])) / ((v[0] - v[1]) * (v[0] - v[2]));
                    }
                    else if (nType == 1)
                    {
                        return ((x - v[1]) + (x - v[2])) / ((v[0] - v[1]) * (v[0] - v[2]));
                    }
                    else
                    {
                        throw new ArgumentException("Invalid der value for basisType 2 and basisIndex 1");
                    }
                }
                else if (basisIndex == 1)
                {
                    if (nType == 0)
                    {
                        return ((x - v[0]) * (x - v[2])) / ((v[1] - v[0]) * (v[1] - v[2]));
                    }
                    else if (nType == 1)
                    {
                        return ((x - v[0]) + (x - v[2])) / ((v[1] - v[0]) * (v[1] - v[2]));
                    }
                    else
                    {
                        throw new ArgumentException("Invalid der value for basisType 2 and basisIndex 2");
                    }
                }
                else if (basisIndex == 2)
                {
                    if (nType == 0)
                    {
                        return ((x - v[0]) * (x - v[1])) / ((v[2] - v[0]) * (v[2] - v[1]));
                    }
                    else if (nType == 1)
                    {
                        return ((x - v[0]) + (x - v[1])) / ((v[2] - v[0]) * (v[2] - v[1]));
                    }
                    else
                    {
                        throw new ArgumentException("Invalid der value for basisType 2 and basisIndex 3");
                    }
                }
                else
                {
                    throw new ArgumentException("Invalid basisIndex for basisType 2");
                }
            }
            else if (basisFunctionOrder == 3)
            {
                if (basisIndex == 0)
                {
                    if (nType == 0)
                    {
                        return ((x - v[1]) * (x - v[2]) * (x - v[3])) / ((v[0] - v[1]) * (v[0] - v[2]) * (v[0] - v[3]));
                    }
                    else if (nType == 1)
                    {
                        return ((x - v[1]) * (x - v[2]) + (x - v[1]) * (x - v[3]) + (x - v[2]) * (x - v[3])) / ((v[0] - v[1]) * (v[0] - v[2]) * (v[0] - v[3]));
                    }
                    else
                    {
                        throw new ArgumentException("Invalid der value for basisType 3 and basisIndex 1");
                    }
                }
                else if (basisIndex == 1)
                {
                    if (nType == 0)
                    {
                        return ((x - v[0]) * (x - v[2]) * (x - v[3])) / ((v[1] - v[0]) * (v[1] - v[2]) * (v[1] - v[3]));
                    }
                    else if (nType == 1)
                    {
                        return ((x - v[0]) * (x - v[2]) + (x - v[0]) * (x - v[3]) + (x - v[2]) * (x - v[3])) / ((v[1] - v[0]) * (v[1] - v[2]) * (v[1] - v[3]));
                    }
                    else
                    {
                        throw new ArgumentException("Invalid der value for basisType 3 and basisIndex 2");
                    }
                }
                else if (basisIndex == 2)
                {
                    if (nType == 0)
                    {
                        return ((x - v[0]) * (x - v[1]) * (x - v[3])) / ((v[2] - v[0]) * (v[2] - v[1]) * (v[2] - v[3]));
                    }
                    else if (nType == 1)
                    {
                        return ((x - v[0]) * (x - v[1]) + (x - v[0]) * (x - v[3]) + (x - v[1]) * (x - v[3])) / ((v[2] - v[0]) * (v[2] - v[1]) * (v[2] - v[3]));
                    }
                    else
                    {
                        throw new ArgumentException("Invalid der value for basisType 3 and basisIndex 3");
                    }
                }
                else if (basisIndex == 3)
                {
                    if (nType == 0)
                    {
                        return ((x - v[0]) * (x - v[1]) * (x - v[2])) / ((v[3] - v[0]) * (v[3] - v[1]) * (v[3] - v[2]));
                    }
                    else if (nType == 1)
                    {
                        return ((x - v[0]) * (x - v[1]) + (x - v[0]) * (x - v[2]) + (x - v[1]) * (x - v[2])) / ((v[3] - v[0]) * (v[3] - v[1]) * (v[3] - v[2]));
                    }
                    else
                    {
                        throw new ArgumentException("Invalid der value for basisType 3 and basisIndex 4");
                    }
                }
                else
                {
                    throw new ArgumentException("Invalid basisIndex for basisType 3");
                }
            }
            else
            {
                throw new ArgumentException("Invalid basisType");
            }
        }

        /// <summary>
        /// Genrate a Gaussian Formular 
        /// </summary>

        public (VectorD, VectorD) GenrateGaussFormular(VectorD v, int nGaussian)
        {

            var GaussWeight = new VectorD(nGaussian);
            var GaussPoint = new VectorD(nGaussian);

            if (nGaussian == 2)
            {
                GaussWeight[0] = 1;GaussWeight[1] = 1;
                GaussPoint[0] = -0.5773502692;GaussPoint[1] = 0.5773502692;
            }
            else if (nGaussian == 3)
            {
                GaussWeight[0] = 0.5555555556; GaussWeight[1] = 0.8888888889; GaussWeight[2] = 0.5555555556 ;
                GaussPoint[0] = -0.7745966692; GaussPoint[1] = 0; GaussPoint[2] = 0.7745966692 ;
            }
            else if (nGaussian == 4)
            {
                GaussPoint[0] = -0.8611363116; GaussPoint[1] = -0.3399810436; GaussPoint[2] = 0.3399810436; GaussPoint[3] = 0.861136316;
                GaussWeight[0] = 0.3478548461; GaussWeight[1] =0.6521451549; GaussWeight[2]=0.6521451549; GaussWeight[3]= 0.3478548461 ;
            }
            else if (nGaussian == 5)
            {
                GaussPoint[0] = -0.9061798459; GaussPoint[1] = -0.5384693101; GaussPoint[2] = 0; GaussPoint[3] = 0.5384693101; GaussPoint[4] = 0.9061798459;
                GaussWeight[0] = 0.2369268851; GaussWeight[1] = 0.4786286705; GaussWeight[2] = 0.5688888889; GaussWeight[3] = 0.4786286705; GaussWeight[4] = 0.2369268851;
            }
            else if (nGaussian == 6)
            {
                GaussPoint[0] = -0.9324695142; GaussPoint[1] = -0.6612093865; GaussPoint[2] = -0.2386191761; GaussPoint[3] = 0.2386191761; GaussPoint[4] = 0.6612093865; GaussPoint[5] = 0.9324695142;
                GaussWeight[0] = 0.1713244924; GaussWeight[1] = 0.3607615730; GaussWeight[2] = 0.4679139346; GaussWeight[3] = 0.4679139346; GaussWeight[4] = 0.3607615730; GaussWeight[5] = 0.1713244924;
            }
            else
            {
                throw new ArgumentException("No such Gauss formula");
            }

            for (int i = 0; i < nGaussian; i++)
            {
                GaussPoint[i] = GaussPoint[i] * (v[1] - v[0]) / 2 + (v[1] + v[0]) / 2;
            }
            for (int i = 0; i < nGaussian; i++)
            {
                GaussWeight[i] = GaussWeight[i] * (v[1] - v[0]) / 2;
            }

            return (GaussWeight, GaussPoint);
        }
        #endregion
    }
}
