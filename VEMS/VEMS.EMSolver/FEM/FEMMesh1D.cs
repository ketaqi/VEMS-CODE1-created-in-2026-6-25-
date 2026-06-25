using System.Reflection.Metadata.Ecma335;
using VEMS.MathCore;

namespace VEMS.EMSolver
{

    /// <summary>
    /// genarate a 1D FEM mesh
    /// </summary>
    public class FEMMesh1D
    {
        #region properties

        /// <summary>
        /// Coordination of all the mesh nodes
        /// </summary>
        public VectorD NodesCoordination { get; set; }

        /// <summary>
        /// Index of all the mesh nodes
        /// </summary>
        public int[,] NodesIndex { get; set; }

        /// <summary>
        /// Coordination of all the calculation nodes
        /// </summary>
        public VectorD CalculationNodesCoordination { get; set; }

        /// <summary>
        /// Index of all the calculation nodes
        /// </summary>
        public int[,] CalculationNodesIndex { get; set; }

        /// <summary>
        /// Permittivity of each elements
        /// </summary>
        public VectorD Epislon { get; set; }

        /// <summary>
        /// Order of FEM basis function
        /// </summary>
        public int BasisFunctionOrder { get; set; }

        #endregion

        #region constructors

        /// <summary>
        /// constructor of 1D FEM Mesh
        /// </summary>
        /// <param name="basisFunctionOrder">Order of FEM Basis Function</param>
        /// <param name="nodesCoordination">Coordination of FEM Mesh Nodes</param>
        /// <param name="epislon">Permittivity of Each Elements</param>
        public FEMMesh1D(int basisFunctionOrder, VectorD nodesCoordination, VectorD epislon)
        {
            BasisFunctionOrder = basisFunctionOrder;
            NodesCoordination = nodesCoordination;

            NodesIndex = new int[2, NodesCoordination.Count - 1];
            CalculationNodesIndex = new int[1 + BasisFunctionOrder, NodesCoordination.Count - 1];

            for (int i = 0; i < NodesCoordination.Count - 1; i++)
            {
                NodesIndex[0, i] = i + 1;
                NodesIndex[1, i] = i + 2;

                for (int j = 0; j <= BasisFunctionOrder; j++)
                {
                    CalculationNodesIndex[j, i] = i * BasisFunctionOrder + j + 1;
                }
            }            

            CalculationNodesCoordination = new VectorD((NodesCoordination.Count - 1) * BasisFunctionOrder + 1);

            for (int i = 0; i < NodesCoordination.Count - 1; i++)
            {
                for (int j = 0; j < BasisFunctionOrder ; j++)
                {
                    CalculationNodesCoordination[i * BasisFunctionOrder + j] = NodesCoordination[i] + j * (NodesCoordination[i + 1] - NodesCoordination[i]) /  BasisFunctionOrder;
                }
            }
            CalculationNodesCoordination[(NodesCoordination.Count - 1) * BasisFunctionOrder] = NodesCoordination[NodesCoordination.Count - 1];

            Epislon = epislon;
        }

        #endregion

        #region methods

        #endregion
    }
}
