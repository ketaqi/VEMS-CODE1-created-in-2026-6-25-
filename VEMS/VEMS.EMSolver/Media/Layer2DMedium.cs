using VEMS.MathCore;
using Complex = System.Numerics.Complex;

namespace VEMS.EMSolver
{
    /// <summary>
    /// two-dimensional (1D) layer medium with
    /// 2D varying permittivity/permeability distribution
    /// </summary>
    public class Layer2DMedium
    {
        #region properties

        /// <summary>
        /// permittivity distribution epsilon = f(λ, x, y)
        /// <para> parameter #1: λ wavelength in vacuum </para>
        /// <para> parameter #2: x lateral position </para>
        /// <para> parameter #3: y layeral position </para>
        /// <para> result: complex permittivity </para>
        /// </summary>
        public Func<double, double, double, Complex> Epsilon { get; set; }

        /// <summary>
        /// permeability distribution mu = f(λ, x, y)
        /// <para> parameter #1: λ wavelength in vacuum </para>
        /// <para> parameter #2: x lateral position </para>
        /// <para> parameter #3: y lateral position </para>
        /// <para> result: complex permeability </para>        
        /// </summary>
        public Func<double, double, double, Complex>? Mu { get; set; }

        /// <summary>
        /// refractive index distribution n = f(λ, x, y)
        /// <para> parameter #1: λ wavelength in vacuum </para>
        /// <para> parameter #2: x lateral position </para>
        /// <para> parameter #3: y lateral position </para>
        /// <para> result: complex refractive index </para>
        /// </summary>
        public Func<double, double, double, Complex> N { get; set; }

        #region ==== cache info ====

        /// <summary>
        /// whether to cache the sampled data
        /// </summary>
        public bool CacheSampleData { get; set; }

        /// <summary>
        /// sampled medium data at a fixed wavelength
        /// for selected material property
        /// </summary>
        public MatrixZ? SampleData { get; set; }

        /// <summary>
        /// uniform grid used for sampling 
        /// </summary>
        public GridInfo2D? SampGrid { get; set; }

        /// <summary>
        /// selected material property for sampling
        /// </summary>
        public MaterialProperty SelectedProp { get; set; }

        #endregion

        #endregion
        #region constructors

        /// <summary>
        /// default constructor for Layer2DMedium class
        /// </summary>
        internal Layer2DMedium()
        {
            Epsilon = (w, x, y) => Complex.One;
            N = (w, x, y) => Complex.One;
        }

        /// <summary>
        /// constructs a Layer2DMedium with permittivity distribution
        /// and/or permeability distribution
        /// </summary>
        /// <param name="epsilon"> permittivity distribution epsilon = f(λ,x,y) </param>
        /// <param name="mu"> permeability distribution mu = f(λ,x,y) </param>
        public Layer2DMedium(Func<double, double, double, Complex> epsilon,
            Func<double, double, double, Complex>? mu)
        {
            Epsilon = epsilon;
            Mu = mu;
            // defines refractive index
            if (Mu == null) { N = (w, x, y) => Complex.Sqrt(Epsilon(w, x, y)); }
            else { N = (w, x, y) => Complex.Sqrt(Epsilon(w, x, y) * Mu(w, x, y)); }
        }

        /// <summary>
        /// constructs a Layer2DMedium with refractive 
        /// index distribution
        /// </summary>
        /// <param name="n"> refractive index distribution n = f(λ,x,y) </param>
        public Layer2DMedium(Func<double, double, double, Complex> n)
        {
            N = n;
            // defines permittivity
            Epsilon = (w, x, y) => N(w, x, y) * N(w, x, y);
        }

        #endregion
        #region methods


        //public VectorZ Sample(double wavelength, ScatInfo2D rho,
        //    MaterialProperty materialProperty = EMDefaults.MatProperty,
        //    LoopMode loopMode = Defaults.LoopOption)
        //{
        //    throw new NotImplementedException();
        //}


        /// <summary>
        /// samples the medium property at a fixed wavelength
        /// on a set of x/y-separable scattered locations
        /// </summary>
        /// <param name="wavelength"> wavelength in vacuum </param>
        /// <param name="xy"> x/y-separable scattered locations </param>
        /// <param name="matProperty"> material property option </param>
        /// <param name="loopMode"> loop-computation option </param>
        /// <param name="cacheSampleData"> whether to save the sample data </param>
        /// <returns> sampled medium property </returns>
        public MatrixZ Sample(double wavelength, ScatInfoXY xy,
            MaterialProperty matProperty = EMDefaults.MatProperty,
            LoopMode loopMode = Defaults.LoopOption,
            bool cacheSampleData = false)
        {
            //defines function sampler at fixed wavelength
            Samp2DCplxFunc sfxy;
            MatrixZ v;
            switch (matProperty)
            {
                case MaterialProperty.N:
                    sfxy = new(f: (x, y, p) => N(wavelength, x, y));
                    break;
                case MaterialProperty.Epsilon:
                    sfxy = new(f: (x, y, p) => Epsilon(wavelength, x, y));
                    break;
                case MaterialProperty.Mu:
                    {
                        if (Mu == null) { throw new ArgumentNullException(nameof(Mu)); }
                        sfxy = new(f: (x, y, p) => Mu(wavelength, x, y));
                        break;
                    }
                default: goto case MaterialProperty.N;
            }
            // sampling
            v = sfxy.Sample(xy: xy, loopMode: loopMode);
            // cache?
            CacheSampleData = cacheSampleData;
            if (CacheSampleData)
            {
                SampleData = v;
                SelectedProp = matProperty;
            }
            // return 
            return v;
        }

        /// <summary>
        /// samples the medium property at a fixed wavelength
        /// on a target uniform grid in space
        /// </summary>
        /// <param name="wavelength"> wavelength in vacuum </param>
        /// <param name="grid"> target uniform sampling grid </param>
        /// <param name="matProperty"> material property option </param>
        /// <param name="loopMode"> loop-computation option </param>
        /// <param name="cacheSampleData"> whether to save the sample data </param>
        /// <returns> sampled medium property </returns>
        public MatrixZ Sample(double wavelength, GridInfo2D grid,
            MaterialProperty matProperty = EMDefaults.MatProperty,
            LoopMode loopMode = Defaults.LoopOption,
            bool cacheSampleData = false)
        {
            if (cacheSampleData) { SampGrid = new(other: grid); }
            return Sample(wavelength: wavelength, xy: (ScatInfoXY)grid,
                matProperty: matProperty,
                loopMode: loopMode,
                cacheSampleData: cacheSampleData);
        }

        #endregion

        #region sub-classes


        class MultiPolygon
        {

        }

        #endregion
    }
}
