using System.Numerics;
using VEMS.MathCore;

namespace VEMS.EMSolver
{
    /// <summary>
    /// grid-based 1D metasurface
    /// </summary>
    public class Grid1DMetaLayer : Layer1DMedium
    {
        #region properties

        /// <summary>
        /// grid points as the centers of meta-atoms
        /// for the 1D metasurface
        /// </summary>
        public GridInfo1D GridPoints { get; set; }

        ///// <summary>
        ///// period of each meta-atom
        ///// </summary>
        //public double Period { get => GridPoints.Spacing; }

        /// <summary>
        /// height of the meta-layer i.e. that of each meta-atom
        /// </summary>
        public double Height { get; set; }

        /// <summary>
        /// list of meta-atoms in the 1D metasurface
        /// all should have the same period and height
        /// </summary>
        public List<MetaAtom1D>? MetaAtoms { get; set; }

        #endregion
        #region constructors

        /// <summary>
        /// constructs a grid-based 1D metasurface with
        /// specific grid points and (optional) meta-atoms
        /// </summary>
        /// <param name="gridPoints"> grid points as the centers of meta-atoms </param>
        /// <param name="height"> height of the meta-layer </param>
        /// <param name="metaAtoms"> list of meta-atoms </param>
        /// <param name="periodicBoundary"> assume periodic boundary for the whole surface </param>
        public Grid1DMetaLayer(GridInfo1D gridPoints, double height,
            List<MetaAtom1D>? metaAtoms = null, 
            bool periodicBoundary = true)
        {
            GridPoints = gridPoints;
            Height = height;
            MetaAtoms = metaAtoms;
            // handling null case
            if(MetaAtoms == null) 
            { 
                Epsilon = (w, x) => Complex.One;
                N = (w, x) => Complex.One;
                return; 
            }
            // consistency check
            if(MetaAtoms.Count != GridPoints.Count)
            { throw new ArgumentException("MetaAtoms and GridPoints should have the same length");}
            foreach (MetaAtom1D a in MetaAtoms)
            {
                if(a.Period != GridPoints.Spacing) { throw new Exception("MetaAtoms should have the same period"); }
                if(a.Height != Height) { throw new Exception("MetaAtoms should have the same height"); }
            }
            // initialize permittivity distribution
            Epsilon = (w, x) =>
            {
                // finds the grid span (and the meta-atom) for given position x
                long i = GridPoints.FindGridSpan(ref x, periodicBoundary);
                if (i == -1) { return Complex.One; } // !!! assign vacuum
                else 
                {
                    MetaAtom1D a = MetaAtoms[(int)i];
                    // enters the local coordinate system of the meta-atom
                    double dx = x - GridPoints[i];
                    return a.Epsilon(arg1: w, arg2: dx);
                }
            };
            // initialize refractive index distribution
            N = (w, x) => Complex.Sqrt(Epsilon(w, x));
        }

        #endregion
        #region methods

        /// <summary>
        /// samples the permittivity or permeability distribution
        /// on a uniform grid with n points
        /// </summary>
        /// <param name="wavelength"> wavelength in vacuum </param>
        /// <param name="n"> number of sampling points </param>
        /// <param name="matProperty"> material property option </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <param name="saveSampledData"> saves the sampled data or not </param>
        /// <returns> sampled permittivity or permeability on these locations </returns>
        public (VectorZ, GridInfo1D) Sample(double wavelength, long n,
            MaterialProperty matProperty = EMDefaults.MatProperty,
            LoopMode loopMode = Defaults.LoopOption,
            bool saveSampledData = false)
        {
            // constructs uniform sampling grid
            GridInfo1D g = new(n: n, spacing: GridPoints.Range / n);
            // call base method
            VectorZ v = Sample(wavelength: wavelength,
                grid: g,
                matProperty: matProperty,
                loopMode: loopMode,
                cacheSampleData: saveSampledData);
            return (v, g);
        }

        /// <summary>
        /// creates RCWA1D solver for this meta-layer
        /// </summary>
        /// <param name="front"> material in front of the meta-layer </param>
        /// <param name="behind"> material behind the meta-layer </param>
        /// <returns> RCWA1D solver </returns>
        public RCWA1D CreateSolver(Material front, Material behind)
            => new(materialFront: front,
                mediumMiddle: this, period: GridPoints.Range, thickness: Height,
                materialBehind: behind);

        #endregion
    }
}
