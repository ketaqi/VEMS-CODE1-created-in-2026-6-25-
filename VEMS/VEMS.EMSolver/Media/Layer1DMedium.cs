using VEMS.MathCore;
using Complex = System.Numerics.Complex;

namespace VEMS.EMSolver
{

    /// <summary>
    /// Represents a one-dimensional (1D) layer medium with
    /// spatially varying permittivity and permeability distributions.
    /// Provides methods for sampling material properties (permittivity, permeability, refractive index)
    /// at specified wavelengths and spatial locations.
    /// </summary>
    public class Layer1DMedium : ILayerMedium
    {
        #region properties

        /// <summary>
        /// Gets or sets the permittivity distribution function.
        /// <para>epsilon = f(λ, x)</para>
        /// <para>Parameter #1: λ (wavelength in vacuum)</para>
        /// <para>Parameter #2: x (lateral position)</para>
        /// <para>Returns: complex permittivity</para>
        /// </summary>
        public Func<double, double, Complex> Epsilon { get; set; }

        /// <summary>
        /// Gets or sets the permeability distribution function.
        /// <para>mu = f(λ, x)</para>
        /// <para>Parameter #1: λ (wavelength in vacuum)</para>
        /// <para>Parameter #2: x (lateral position)</para>
        /// <para>Returns: complex permeability</para>
        /// </summary>
        public Func<double, double, Complex>? Mu { get; set; }

        /// <summary>
        /// Gets or sets the refractive index distribution function.
        /// <para>n = f(λ, x)</para>
        /// <para>Parameter #1: λ (wavelength in vacuum)</para>
        /// <para>Parameter #2: x (lateral position)</para>
        /// <para>Returns: complex refractive index</para>
        /// </summary>
        public Func<double, double, Complex> N { get; set; }

        #region ==== cache info ====

        /// <summary>
        /// Gets or sets a value indicating whether to cache the sampled data.
        /// </summary>
        public bool CacheSampleData { get; set; }

        /// <summary>
        /// Gets or sets the sampled medium data at a fixed wavelength
        /// for the selected material property.
        /// </summary>
        public VectorZ? SampleData { get; set; }

        /// <summary>
        /// Gets or sets the uniform grid used for sampling.
        /// </summary>
        public GridInfo1D? SampGrid { get; set; }

        /// <summary>
        /// Gets or sets the selected material property for sampling.
        /// </summary>
        public MaterialProperty SelectedProp { get; set; }

        #endregion

        #endregion
        #region constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Layer1DMedium"/> class
        /// with default permittivity and refractive index (both set to 1).
        /// </summary>
        internal Layer1DMedium()
        {
            Epsilon = (w, x) => Complex.One;
            N = (w, x) => Complex.One;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Layer1DMedium"/> class
        /// with specified permittivity and optional permeability distributions.
        /// The refractive index is defined as sqrt(epsilon) if mu is null,
        /// otherwise as sqrt(epsilon * mu).
        /// </summary>
        /// <param name="epsilon">Permittivity distribution function (epsilon = f(λ, x)).</param>
        /// <param name="mu">Optional permeability distribution function (mu = f(λ, x)).</param>
        public Layer1DMedium(Func<double, double, Complex> epsilon,
            Func<double, double, Complex>? mu = null)
        {
            Epsilon = epsilon;
            Mu = mu;
            //define refractive index
            if (Mu == null) { N = (w, x) => Complex.Sqrt(Epsilon(w, x)); }
            else { N = (w, x) => Complex.Sqrt(Epsilon(w, x) * Mu(w, x)); }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Layer1DMedium"/> class
        /// with a specified refractive index distribution.
        /// The permittivity is defined as n^2.
        /// </summary>
        /// <param name="n">Refractive index distribution function (n = f(λ, x)).</param>
        public Layer1DMedium(Func<double, double, Complex> n)
        {
            N = n;
            // defines permittivity
            Epsilon = (w, x) => N(w, x) * N(w, x);
        }

        #endregion
        #region methods

        /// <summary>
        /// Samples the specified material property of the medium at a fixed wavelength
        /// on a set of spatial locations (either uniform or scattered).
        /// </summary>
        /// <param name="wavelength">Wavelength in vacuum.</param>
        /// <param name="xs">Spatial sampling locations.</param>
        /// <param name="matProperty">Material property to sample (default: <see cref="EMDefaults.MatProperty"/>).</param>
        /// <param name="loopMode">Loop-computation option (default: <see cref="Defaults.LoopOption"/>).</param>
        /// <param name="cacheSampleData">Whether to save the sampled data.</param>
        /// <returns>Sampled medium property as a <see cref="VectorZ"/>.</returns>
        public VectorZ Sample(double wavelength, ScatInfo1D xs,
            MaterialProperty matProperty = EMDefaults.MatProperty,
            LoopMode loopMode = Defaults.LoopOption,
            bool cacheSampleData = false)
        {
            // Define function sampler at fixed wavelength
            Samp1DCplxFunc sfx = matProperty switch
            {
                MaterialProperty.N => new(f: (x, p) => N(wavelength, x)),
                MaterialProperty.Epsilon => new(f: (x, p) => Epsilon(wavelength, x)),
                MaterialProperty.Mu => Mu is not null
                    ? new(f: (x, p) => Mu(wavelength, x))
                    : throw new ArgumentNullException(nameof(Mu)),
                _ => new(f: (x, p) => N(wavelength, x))
            };

            // Directly sample and cache if needed
            VectorZ v = sfx.Sample(xs, loopMode);
            if (cacheSampleData)
            {
                CacheSampleData = true;
                SampleData = v;
                SelectedProp = matProperty;
            }
            else
            { CacheSampleData = false; }

            // Return
            return v;
        }

        /// <summary>
        /// Samples the specified material property of the medium at a fixed wavelength
        /// on a target uniform grid in space.
        /// </summary>
        /// <param name="wavelength">Wavelength in vacuum.</param>
        /// <param name="grid">Target uniform sampling grid.</param>
        /// <param name="matProperty">Material property to sample (default: <see cref="EMDefaults.MatProperty"/>).</param>
        /// <param name="loopMode">Loop-computation option (default: <see cref="Defaults.LoopOption"/>).</param>
        /// <param name="cacheSampleData">Whether to save the sampled data.</param>
        /// <returns>Sampled medium property as a <see cref="VectorZ"/>.</returns>
        public VectorZ Sample(double wavelength, GridInfo1D grid,
            MaterialProperty matProperty = EMDefaults.MatProperty,
            LoopMode loopMode = Defaults.LoopOption,
            bool cacheSampleData = false)
        {
            if (cacheSampleData) { SampGrid = new(other: grid); }
            return Sample(wavelength: wavelength, xs: (ScatInfo1D)grid,
                matProperty: matProperty,
                loopMode: loopMode,
                cacheSampleData: cacheSampleData);
        }

        /// <summary>
        /// samples the medium at a fixed wavelength
        /// </summary>
        /// <param name="wavelength">Wavelength in vacuum.</param>
        /// <param name="grid">Target uniform sampling grid.</param>
        /// <param name="loopMode">Loop-computation option (default: <see cref="Defaults.LoopOption"/>).</param>
        /// <param name="cacheSampleData">Whether to save the sampled data.</param>
        /// <returns> sampled permittivity (and permeability) data </returns>
        public (VectorZ epsilon, VectorZ? mu) SampleMedium(double wavelength, GridInfo1D grid,
            LoopMode loopMode = Defaults.LoopOption,
            bool cacheSampleData = true)
        {
            if (cacheSampleData) { SampGrid = new(other: grid); }
            VectorZ v = Sample(wavelength: wavelength, xs: (ScatInfo1D)grid,
                matProperty: MaterialProperty.Epsilon,
                loopMode: loopMode,
                cacheSampleData: cacheSampleData);
            VectorZ? u = (Mu == null) ?
                null : Sample(wavelength: wavelength,
                grid: grid,
                matProperty: MaterialProperty.Mu,
                cacheSampleData: cacheSampleData);

            // return
            return (v, u);
        }

        /// <summary>
        /// Samples the permittivity in the k-domain (Fourier domain) using the direct rule.
        /// </summary>
        /// <param name="wavelength">Wavelength in vacuum.</param>
        /// <param name="grid">Target uniform sampling grid.</param>
        /// <param name="loopMode">Loop-computation option (default: <see cref="Defaults.LoopOption"/>).</param>
        /// <param name="cacheSampleData">Whether to save the sampled data.</param>
        /// <returns>Sampled and transformed medium property as a <see cref="VectorZ"/>.</returns>
        public (VectorZ epsilonK, VectorZ? muK) MediumInDirKdomain(double wavelength, GridInfo1D grid,
            LoopMode loopMode = Defaults.LoopOption,
            bool cacheSampleData = false)
        {
            (VectorZ v, VectorZ? u) = SampleMedium(wavelength, grid, loopMode, cacheSampleData);

            // Switch to k-domain using FFT
            VectorZ kv = new(v, deepCopy: true);
            VectorZ ku = null;
            Transform.FFS1D(x: ref kv, isForward: true);
            if (u != null)
            {
                ku = new(u, deepCopy: true);
                Transform.FFS1D(x: ref ku, isForward: true);
            }

            //return
            return (kv, ku);
        }

        /// <summary>
        /// Samples the permittivity in the k-domain (Fourier domain) using the inverse rule.
        /// </summary>
        /// <param name="wavelength">Wavelength in vacuum.</param>
        /// <param name="grid">Target uniform sampling grid.</param>
        /// <param name="loopMode">Loop-computation option (default: <see cref="Defaults.LoopOption"/>).</param>
        /// <param name="cacheSampleData">Whether to save the sampled data.</param>
        /// <returns>Sampled and transformed medium property as a <see cref="VectorZ"/>.</returns>
        public (VectorZ epsilonK, VectorZ? muK) MediumInInvKdomain(double wavelength, GridInfo1D grid,
            LoopMode loopMode = Defaults.LoopOption,
            bool cacheSampleData = false)
        {
            (VectorZ v, VectorZ? u) = SampleMedium(wavelength, grid, loopMode, cacheSampleData);

            // Switch to k-domain using FFT
            VectorZ kv = new(1.0 / v, deepCopy: true);
            VectorZ ku = null;
            Transform.FFS1D(x: ref kv, isForward: true);
            if (u != null)
            {
                ku = new(1.0 / u, deepCopy: true);
                Transform.FFS1D(x: ref ku, isForward: true);
            }

            //return
            return (kv, ku);
        }

        #endregion

        #region sub-classes

        /// <summary>
        /// Represents a multi-rectangle 1D layer medium.
        /// (Implementation placeholder for future extension.)
        /// </summary>
        class MultiRect : Layer1DMedium
        {

        }

        #endregion
    }

}
