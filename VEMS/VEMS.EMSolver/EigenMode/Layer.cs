using VEMS.MathCore;

namespace VEMS.EMSolver
{

    /// <summary>
    /// base class for all layers
    /// </summary>
    public abstract class EigenLayer
    {
        #region properties

        /// <summary>
        /// thickness of the layer
        /// </summary>
        public double Thickness { get; set; }

        /// <summary>
        /// eigenvalue Gamma
        /// i.e. propagation constants
        /// </summary>
        public VectorZ? Gamma { get; set; }

        /// <summary>
        /// eigenvector matrix W1
        /// i.e. modes 
        /// </summary>
        public MatrixZ? W1 { get; set; }

        /// <summary>
        /// eigenvector matrix W2
        /// i.e. modes
        /// </summary>
        public MatrixZ? W2 { get; set; }

        /// <summary>
        /// loop-computational mode option
        /// </summary>
        public LoopMode LoopOption { get; set; } = LoopMode.Vectorized;

        #endregion
        #region constructors

        #endregion
        #region methods

        #endregion
    }


    

}
