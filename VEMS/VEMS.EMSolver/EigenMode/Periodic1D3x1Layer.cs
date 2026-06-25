using VEMS.MathCore;

namespace VEMS.EMSolver
{

    /// <summary>
    /// 1D-periodic (diagonally) anisotropic layer
    /// for use in RCWA
    /// </summary>
    public class Periodic1D3x1Layer
    {

        /// <summary>
        /// uniform sampling grid for 
        /// permittivity and permeability
        /// </summary>
        public GridInfo1D? Grid { get; set; }

        /// <summary>
        /// period along x-direction
        /// </summary>
        public double Period { get; set; }

        /// <summary>
        /// epsilon_33 
        /// </summary>
        public Func<double, GridInfo1D, VectorZ> Epsilon11 { get; set; }  

        public Func<double, GridInfo1D, VectorZ> Epsilon22 { get; set; }

        public Func<double, GridInfo1D, VectorZ> Epsilon33 { get; set; }

        public VectorZ? Epsilon11Data { get; set; }
        public VectorZ? Epsilon22Data { get; set; }
        public VectorZ? Epsilon33Data { get; set; }




    }
}
