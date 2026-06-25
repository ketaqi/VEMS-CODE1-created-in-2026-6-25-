using VEMS.MathCore;
using Complex = System.Numerics.Complex;

namespace VEMS.EMSolver
{

    /// <summary>
    /// finite-difference eigenmode solver
    /// </summary>
    public class FDLayer1D
    {

        #region properties

        /// <summary>
        /// refractive index N = n(λ, x; {p})
        /// currently only supporting real-valued parameters
        /// </summary>
        public Func<double, double, List<double>, Complex> NFunc { get; set; }

        ///// <summary>
        ///// permittivity Epsilon = epsilon(λ, x; {p})
        ///// </summary>
        //public Func<double, double, List<double>, Complex> EpsilonFunc { get; set; }

        ///// <summary>
        ///// permeability Mu = mu(λ, x; {p})
        ///// </summary>
        //public Func<double, double, List<double>, Complex> MuFunc { get; set; }

        /// <summary>
        /// length of the layer along the z-direction
        /// </summary>
        public double Length { get; set; }

        #endregion
        #region constructor

        ///// <summary>
        ///// constructs a FTLayer1D for finite-difference eigensolution
        ///// </summary>
        ///// <param name="epsilon"> permittivity Epsilon = epsilon(x, λ; {p}) </param>
        ///// <param name="mu"> permeability Mu = mu(x, λ; {p}) </param>
        ///// <param name="length"> length of the layer along the z-direction </param>
        //public FDLayer1D(Func<double, double, List<double>, Complex> epsilon,
        //    Func<double, double, List<double>, Complex> mu,
        //    double length = 0.0)
        //{
        //    EpsilonFunc = epsilon;
        //    MuFunc = mu;
        //    NFunc = (w, x, p) => Complex.Sqrt(EpsilonFunc(w, x, p) * MuFunc(w, x, p));
        //    Length = length;
        //}

        /// <summary>
        /// constructs a 1D FD-layer with given
        /// refractivce index distribution
        /// </summary>
        /// <param name="nFunc"> refractive index N = n(λ, x; {p}) </param>
        /// <param name="length"> length of the medium along the z-direction </param>
        public FDLayer1D(Func<double, double, List<double>, Complex> nFunc,
            double length = 0.0)
        {
            NFunc = nFunc; 
            Length = length;
        }

        #endregion
        #region methods

        /// <summary>
        /// samples N = n(λ, x; {p}) on given grid
        /// for a fixed wavelength λ
        /// </summary>
        /// <param name="wavelength"> wavelength in vacuum </param>
        /// <param name="parameters"> list of parameters {p} in n(λ, x; {p}) </param>
        /// <param name="grid"> target uniform sampling grid </param>
        /// <returns> sampled epsilon </returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// 
        public VectorZ SampleN(double wavelength,
            List<double> parameters,
            GridInfo1D grid)
        {
            if (NFunc == null) { throw new ArgumentNullException(nameof(NFunc)); }

            // makes a new function at fixed wavelength
            Complex nFuncx(double x, List<double> p) => NFunc(wavelength, x, p);

            // uniform-grid sample of the function
            Samp1DCplxFunc gridFunc = new(nFuncx, parameters);
            return gridFunc.Sample(grid);
        }





        ///// <summary>
        ///// samples Epsilon = epsilon(x, y, λ; {p}) on given grid
        ///// for a fixed wavelength λ
        ///// </summary>
        ///// <param name="wavelength"> wavelength in vacuum </param>
        ///// <param name="parameters"> list of parameters {p} in epsilon(x, λ; {p}) </param>
        ///// <param name="grid"> target uniform sampling grid </param>
        ///// <returns>  </returns>
        ///// <exception cref="ArgumentNullException"></exception>
        //internal VectorZ SampleEpsilon(double wavelength,
        //    List<double> parameters,
        //    GridInfo1D grid)
        //{
        //    if (FEpsilon == null) { throw new ArgumentNullException(nameof(FEpsilon)); }
        //    // makes a new function at fixed wavelength
        //    Complex f(double x, List<double> p) => FEpsilon(x, wavelength, p);
        //    //  uniform-grid sample of the function
        //    Grid1DCplxFunc gridFunc = new(f, parameters);
        //    return gridFunc.SampleOnGrid(grid);
        //}


        //internal VectorZ SampleMu(double wavelength,
        //    List<double> parameters,
        //    GridInfo1D grid)
        //{
        //    if (FMu == null) { throw new System.ArgumentNullException(nameof(FMu)); }
        //    // makes a new function at fixed wavelength
        //    Complex f(double x, List<double> p) => FMu(x, wavelength, p);
        //    //  uniform-grid sample of the function
        //    Grid1DCplxFunc gridFunc = new(f, parameters);
        //    return gridFunc.SampleOnGrid(grid);
        //}

        #endregion

    }
}
