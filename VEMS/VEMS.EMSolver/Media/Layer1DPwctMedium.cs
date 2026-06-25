using VEMS.MathCore;
using Complex = System.Numerics.Complex;

namespace VEMS.EMSolver
{
    /// <summary>
    /// Represents a one-dimensional (1D) layer medium with
    /// piecewise permittivity and permeability value.
    /// Provides methods for sampling material properties (permittivity, permeability, refractive index)
    /// </summary>
    public class Layer1DPwctMedium : ILayerMedium
    {
        #region properties
        /// <summary>
        /// Piecewise-constant complex permittivity data for the 1D layered medium.
        /// Each piece defines a constant permittivity value over a span.
        /// </summary>
        public Pwct1DCplxData EpsilonData { get; set; }

        /// <summary>
        /// Gets or sets the permittivity distribution function.
        /// <para>epsilon = f(λ, x)</para>
        /// <para>Parameter #1: λ (wavelength in vacuum)</para>
        /// <para>Parameter #2: x (lateral position)</para>
        /// <para>Returns: complex permittivity</para>
        /// </summary>
        public Func<double, double, Complex> Epsilon
        {
            get => (lambda, x) =>
            {
                // 只用x查找区间
                var spans = EpsilonData.Spans;
                var values = EpsilonData.Values;
                int n = (int)values.Count;
                for (int i = 0; i < n; i++)
                {
                    if (x >= spans[i] && x < spans[i + 1])
                        return values[i];
                }
                // 边界处理
                if (x == spans[n]) return values[n - 1];
                return Complex.Zero;
            };
        }

        /// <summary>
        /// Piecewise-constant complex permeability data for the 1D layered medium.
        /// Each piece defines a constant permeability value over a span.
        /// </summary>
        public Pwct1DCplxData? MuData { get; set; }

        /// <summary>
        /// Gets or sets the permeability distribution function.
        /// <para>mu = f(λ, x)</para>
        /// <para>Parameter #1: λ (wavelength in vacuum)</para>
        /// <para>Parameter #2: x (lateral position)</para>
        /// <para>Returns: complex permeability</para>
        /// </summary>
        public Func<double, double, Complex>? Mu
        {
            get => MuData == null ? null : (lambda, x) =>
            {
                var spans = MuData.Spans;
                var values = MuData.Values;
                int n = (int)values.Count;
                for (int i = 0; i < n; i++)
                {
                    if (x >= spans[i] && x < spans[i + 1])
                        return values[i];
                }
                if (x == spans[n]) return values[n - 1];
                return Complex.Zero;
            };
        }

        /// <summary>
        /// Piecewise-constant refractive index data for the 1D layered medium.
        /// Each piece defines a constant refractive index value over a span. 
        /// </summary>
        public Pwct1DCplxData N { get; set; }

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
        #region constructers

        /// <summary>
        /// Initializes a new instance of the <see cref="Layer1DPwctMedium"/> class
        /// with the specified piecewise-constant permittivity and optional permeability data.
        /// </summary>
        /// <param name="epsilon">Piecewise-constant complex permittivity data.</param>
        /// <param name="mu">Piecewise-constant complex permeability data (optional).</param>
        public Layer1DPwctMedium(Pwct1DCplxData epsilon,
            Pwct1DCplxData? mu = null)
        {
            EpsilonData = epsilon;
            MuData = mu;
            //define refractive index
            if (MuData == null) { N = new Pwct1DCplxData(EpsilonData.Spans, VMath.Sqrt(EpsilonData.Values)); }
            else { N = new Pwct1DCplxData(EpsilonData.Spans, VMath.Sqrt(EpsilonData.Values * MuData.Values)); }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Layer1DPwctMedium"/> class
        /// using spans and refractive index values.
        /// </summary>
        /// <param name="spans">Span locations that define the pieces.</param>
        /// <param name="nValues">Refractive index values for each piece.</param>
        public Layer1DPwctMedium(VectorD spans, VectorZ nValues)
        {
            N = new Pwct1DCplxData(spans, nValues);
            // define permittivity
            EpsilonData = new Pwct1DCplxData(spans, nValues * nValues);
        }

        #endregion
        #region methods

        /// <summary>
        /// Samples the specified material property at a given wavelength and sample locations.
        /// Optionally caches the sampled data.
        /// </summary>
        /// <param name="xs">Sample locations (scattered or uniform).</param>
        /// <param name="matProperty">The material property to sample (default: EMDefaults.MatProperty).</param>
        /// <param name="loopMode">Loop computation mode (default: Defaults.LoopOption).</param>
        /// <param name="cacheSampleData">Whether to cache the sampled data (default: false).</param>
        /// <returns>Sampled values as a VectorZ.</returns>
        public VectorZ Sample(ScatInfo1D xs,
            MaterialProperty matProperty = EMDefaults.MatProperty,
            LoopMode loopMode = Defaults.LoopOption,
            bool cacheSampleData = false)
        {
            // Define function sampler at fixed wavelength
            Pwct1DCplxData sfx = matProperty switch
            {
                MaterialProperty.N => N,
                MaterialProperty.Epsilon => EpsilonData,
                MaterialProperty.Mu => MuData is not null 
                    ? MuData : throw new ArgumentNullException(nameof(MuData)),
                _ => N
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
        /// Samples the specified material property on a uniform grid.
        /// Optionally caches the sampled data and grid.
        /// </summary>
        /// <param name="grid">Uniform grid for sampling.</param>
        /// <param name="matProperty">The material property to sample (default: EMDefaults.MatProperty).</param>
        /// <param name="loopMode">Loop computation mode (default: Defaults.LoopOption).</param>
        /// <param name="cacheSampleData">Whether to cache the sampled data and grid (default: false).</param>
        /// <returns>Sampled values as a VectorZ.</returns>
        public VectorZ Sample(GridInfo1D grid,
            MaterialProperty matProperty = EMDefaults.MatProperty,
            LoopMode loopMode = Defaults.LoopOption,
            bool cacheSampleData = false)
        {
            if (cacheSampleData) { SampGrid = new(other: grid); }
            return Sample(xs: (ScatInfo1D)grid,
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
            VectorZ v = Sample(xs: (ScatInfo1D)grid,
                matProperty: MaterialProperty.Epsilon,
                loopMode: loopMode,
                cacheSampleData: cacheSampleData);
            VectorZ? u = (MuData == null) ?
                null : Sample(grid: grid,
                matProperty: MaterialProperty.Mu,
                cacheSampleData: cacheSampleData);

            // return
            return (v, u);
        }

        /// <summary>
        /// Returns the permittivity sampled in the k-domain (Fourier domain) on the given grid.
        /// using the direct rule.
        /// </summary>
        /// <param name="wavelength">Wavelength in vacuum.</param>
        /// <param name="grid">Uniform grid for sampling in the k-domain.</param>
        /// <param name="loopMode">Loop computation mode (default: Defaults.LoopOption).</param>
        /// <param name="cacheSampleData">Whether to cache the sampled data and grid (default: false).</param>
        /// <returns>Sampled values in the k-domain as a VectorZ.</returns>
        public (VectorZ epsilonK, VectorZ? muK) MediumInDirKdomain(double wavelength, GridInfo1D grid,
            LoopMode loopMode = Defaults.LoopOption,
            bool cacheSampleData = false)
        {
            Pwct1DCplxData v = EpsilonData;
            VectorZ kv = Transform.ForwardTransform1D(x: v, startIndex: -(grid.Count - 1) / 2, numCoeff: grid.Count);

            VectorZ ku = null;
            if (MuData != null)
            {
                Pwct1DCplxData? u = MuData;
                ku = Transform.ForwardTransform1D(x: u, startIndex: -(grid.Count - 1) / 2, numCoeff: grid.Count);
            }

            return (kv, ku);
        }

        /// <summary>
        /// Returns the permittivity sampled in the k-domain (Fourier domain) on the given grid.
        /// using the inverse rule.
        /// </summary>
        /// <param name="wavelength">Wavelength in vacuum.</param>
        /// <param name="grid">Uniform grid for sampling in the k-domain.</param>
        /// <param name="loopMode">Loop computation mode (default: Defaults.LoopOption).</param>
        /// <param name="cacheSampleData">Whether to cache the sampled data and grid (default: false).</param>
        /// <returns>Sampled values in the k-domain as a VectorZ.</returns>
        public (VectorZ epsilonK, VectorZ? muK) MediumInInvKdomain(double wavelength, GridInfo1D grid,
            LoopMode loopMode = Defaults.LoopOption,
            bool cacheSampleData = false)
        {
            if (cacheSampleData) { SampGrid = new(other: grid); }

            Pwct1DCplxData v = new(EpsilonData.Spans, 1.0 / EpsilonData.Values);
            VectorZ kv = Transform.ForwardTransform1D(x: v, startIndex: -(grid.Count - 1) / 2, numCoeff: grid.Count);

            VectorZ ku = null;
            if (MuData != null)
            {
                Pwct1DCplxData? u = new(MuData.Spans, 1.0 / MuData.Values);
                ku = Transform.ForwardTransform1D(x: u, startIndex: -(grid.Count - 1) / 2, numCoeff: grid.Count);
            }

            return (kv, ku);
        }
        #endregion
    }
}
