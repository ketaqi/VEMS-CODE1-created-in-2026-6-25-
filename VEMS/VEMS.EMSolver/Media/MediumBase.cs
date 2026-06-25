using System.Numerics;
using VEMS.MathCore;

namespace VEMS.EMSolver
{
    /// <summary>
    /// Represents a one-dimensional medium with spatially varying permittivity, permeability, and refractive index.
    /// Provides methods for sampling these properties on 1D grids or scattered points.
    /// </summary>
    public class Medium1D
    {
        #region properties

        /// <summary>
        /// Gets or sets the permittivity distribution function ε(λ, x).
        /// <para>Parameter #1: λ - wavelength in vacuum.</para>
        /// <para>Parameter #2: x - lateral position.</para>
        /// <para>Returns: complex permittivity at the specified parameters.</para>
        /// </summary>
        public Func<double, double, Complex> Epsilon { get; set; }
            = (λ, x) => Complex.One;

        /// <summary>
        /// Gets or sets the permeability distribution function μ(λ, x).
        /// <para>Parameter #1: λ - wavelength in vacuum.</para>
        /// <para>Parameter #2: x - lateral position.</para>
        /// <para>Returns: complex permeability at the specified parameters.</para>
        /// </summary>
        public Func<double, double, Complex>? Mu { get; set; }
            = (λ, x) => Complex.One;

        /// <summary>
        /// Gets or sets the refractive index distribution function n(λ, x).
        /// <para>Parameter #1: λ - wavelength in vacuum.</para>
        /// <para>Parameter #2: x - lateral position.</para>
        /// <para>Returns: complex refractive index at the specified parameters.</para>
        /// </summary>
        public Func<double, double, Complex> N { get; set; }
            = (λ, x) => Complex.One;

        #endregion
        #region constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Medium1D"/> class with default properties (all set to unity).
        /// </summary>
        internal Medium1D() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Medium1D"/> class with specified permittivity and optional permeability functions.
        /// The refractive index is computed as n = sqrt(ε * μ) if μ is provided, otherwise n = sqrt(ε).
        /// </summary>
        /// <param name="epsilon">Permittivity distribution function ε(λ, x).</param>
        /// <param name="mu">Optional permeability distribution function μ(λ, x).</param>
        public Medium1D(Func<double, double, Complex> epsilon,
            Func<double, double, Complex>? mu = null)
        {
            Epsilon = epsilon;
            if (mu != null)
            {
                Mu = mu;
                N = (λ, x) => Complex.Sqrt(epsilon(λ, x) * mu(λ, x));
            }
            else
            {
                N = (λ, x) => Complex.Sqrt(epsilon(λ, x));
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Medium1D"/> class with a specified refractive index function.
        /// The permittivity is set as ε = n².
        /// </summary>
        /// <param name="n">Refractive index distribution function n(λ, x).</param>
        public Medium1D(Func<double, double, Complex> n)
        {
            N = n;
            Epsilon = (λ, x) => n(λ, x) * n(λ, x);
        }

        #endregion
        #region methods

        /// <summary>
        /// Defines a 1D complex-valued function sampler for the specified material property
        /// at a given wavelength.
        /// </summary>
        /// <param name="wavelength">Wavelength in vacuum.</param>
        /// <param name="materialProperty">Material property to sample (N, Epsilon, or Mu).</param>
        /// <returns>
        /// A <see cref="Samp1DCplxFunc"/> for the selected property.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if Mu is requested but not defined.
        /// </exception>
        private Samp1DCplxFunc Get1DFunc(double wavelength,
            MaterialProperty materialProperty)
        {
            Func<double, double, Complex> func = materialProperty switch
            {
                MaterialProperty.N => N,
                MaterialProperty.Epsilon => Epsilon,
                MaterialProperty.Mu => Mu is not null ? Mu : throw new ArgumentNullException(nameof(Mu), "Mu is not defined."),
                _ => N
            };
            return new Samp1DCplxFunc(f: (x) => func(wavelength, x));
        }

        /// <summary>
        /// Samples the specified material property on a set of scattered 1D locations.
        /// </summary>
        /// <param name="wavelength">Wavelength in vacuum.</param>
        /// <param name="scat">Scattered sample locations.</param>
        /// <param name="materialProperty">Material property to sample (default: <see cref="EMDefaults.MatProperty"/>).</param>
        /// <param name="loopMode">Loop-computational mode (default: <see cref="Defaults.LoopOption"/>).</param>
        /// <returns>Sampled values as a <see cref="VectorZ"/>.</returns>
        public VectorZ Sample(double wavelength, ScatInfo1D scat,
            MaterialProperty materialProperty = EMDefaults.MatProperty,
            LoopMode loopMode = Defaults.LoopOption)
        {
            Samp1DCplxFunc sfx = Get1DFunc(wavelength, materialProperty);
            return sfx.Sample(xs: scat, loopMode: loopMode);
        }

        /// <summary>
        /// Samples the specified material property on a uniform 1D grid.
        /// </summary>
        /// <param name="wavelength">Wavelength in vacuum.</param>
        /// <param name="grid">Uniform grid information.</param>
        /// <param name="materialProperty">Material property to sample (default: <see cref="EMDefaults.MatProperty"/>).</param>
        /// <param name="loopMode">Loop-computational mode (default: <see cref="Defaults.LoopOption"/>).</param>
        /// <returns>Sampled values as a <see cref="VectorZ"/>.</returns>
        public VectorZ Sample(double wavelength, GridInfo1D grid,
            MaterialProperty materialProperty = EMDefaults.MatProperty,
            LoopMode loopMode = Defaults.LoopOption)
        {
            Samp1DCplxFunc sfx = Get1DFunc(wavelength, materialProperty);
            return sfx.Sample(grid: grid, loopMode: loopMode);
        }

        #endregion
    }

    /// <summary>
    /// Represents a two-dimensional medium with spatially varying permittivity, permeability, and refractive index.
    /// Provides methods for sampling these properties on 2D grids or scattered points.
    /// </summary>
    public class Medium2D
    {
        #region properties

        /// <summary>
        /// Gets or sets the permittivity distribution function ε(λ, x₁, x₂).
        /// <para>Parameter #1: λ - wavelength in vacuum.</para>
        /// <para>Parameter #2: x₁ - lateral position in the first dimension.</para>
        /// <para>Parameter #3: x₂ - lateral position in the second dimension.</para>
        /// <para>Returns: complex permittivity at the specified parameters.</para>
        /// </summary>
        public Func<double, double, double, Complex> Epsilon { get; set; }
            = (λ, x1, x2) => Complex.One;

        /// <summary>
        /// Gets or sets the permeability distribution function μ(λ, x₁, x₂).
        /// <para>Parameter #1: λ - wavelength in vacuum.</para>
        /// <para>Parameter #2: x₁ - lateral position in the first dimension.</para>
        /// <para>Parameter #3: x₂ - lateral position in the second dimension.</para>
        /// <para>Returns: complex permeability at the specified parameters.</para>
        /// </summary>
        public Func<double, double, double, Complex>? Mu { get; set; }
            = (λ, x1, x2) => Complex.One;

        /// <summary>
        /// Gets or sets the refractive index distribution function n(λ, x₁, x₂).
        /// <para>Parameter #1: λ - wavelength in vacuum.</para>
        /// <para>Parameter #2: x₁ - lateral position in the first dimension.</para>
        /// <para>Parameter #3: x₂ - lateral position in the second dimension.</para>
        /// <para>Returns: complex refractive index at the specified parameters.</para>
        /// </summary>
        public Func<double, double, double, Complex> N { get; set; }
            = (λ, x1, x2) => Complex.One;

        #endregion
        #region constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Medium2D"/> class with default properties (all set to unity).
        /// </summary>
        internal Medium2D() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Medium2D"/> class with specified permittivity and optional permeability functions.
        /// The refractive index is computed as n = sqrt(ε * μ) if μ is provided, otherwise n = sqrt(ε).
        /// </summary>
        /// <param name="epsilon">Permittivity distribution function ε(λ, x₁, x₂).</param>
        /// <param name="mu">Optional permeability distribution function μ(λ, x₁, x₂).</param>
        public Medium2D(Func<double, double, double, Complex> epsilon,
            Func<double, double, double, Complex>? mu = null)
        {
            Epsilon = epsilon;
            if (mu != null)
            {
                Mu = mu;
                N = (λ, x1, x2) => Complex.Sqrt(epsilon(λ, x1, x2) * mu(λ, x1, x2));
            }
            else
            {
                N = (λ, x1, x2) => Complex.Sqrt(epsilon(λ, x1, x2));
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Medium2D"/> class with a specified refractive index function.
        /// The permittivity is set as ε = n².
        /// </summary>
        /// <param name="n">Refractive index distribution function n(λ, x₁, x₂).</param>
        public Medium2D(Func<double, double, double, Complex> n)
        {
            N = n;
            Epsilon = (λ, x1, x2) => n(λ, x1, x2) * n(λ, x1, x2);
        }

        #endregion
        #region methods

        #region ---- sample 2D ----

        /// <summary>
        /// Defines a 2D complex-valued function sampler for the specified material property
        /// at a given wavelength.
        /// </summary>
        /// <param name="wavelength">Wavelength in vacuum.</param>
        /// <param name="materialProperty">Material property to sample (N, Epsilon, or Mu).</param>
        /// <returns>
        /// A <see cref="Samp2DCplxFunc"/> for the selected property.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if Mu is requested but not defined.
        /// </exception>
        private Samp2DCplxFunc Get2DFunc(double wavelength,
            MaterialProperty materialProperty)
        {
            Func<double, double, double, Complex> func = materialProperty switch
            {
                MaterialProperty.N => N,
                MaterialProperty.Epsilon => Epsilon,
                MaterialProperty.Mu => Mu is not null ? Mu : throw new ArgumentNullException(nameof(Mu), "Mu is not defined."),
                _ => N
            };
            return new Samp2DCplxFunc(f: (x1, x2) => func(wavelength, x1, x2));
        }

        /// <summary>
        /// Samples the specified material property on a set of scattered 2D locations.
        /// </summary>
        /// <param name="wavelength">Wavelength in vacuum.</param>
        /// <param name="scat">Scattered sample locations in 2D.</param>
        /// <param name="materialProperty">Material property to sample (default: <see cref="EMDefaults.MatProperty"/>).</param>
        /// <param name="loopMode">Loop-computational mode (default: <see cref="Defaults.LoopOption"/>).</param>
        /// <returns>Sampled values as a <see cref="VectorZ"/>.</returns>
        public VectorZ Sample2D(double wavelength, ScatInfo2D scat,
            MaterialProperty materialProperty = EMDefaults.MatProperty,
            LoopMode loopMode = Defaults.LoopOption)
        {
            Samp2DCplxFunc sfx = Get2DFunc(wavelength, materialProperty);
            return sfx.Sample(rho: scat, loopMode: loopMode);
        }

        /// <summary>
        /// Samples the specified material property on a uniform 2D grid.
        /// </summary>
        /// <param name="wavelength">Wavelength in vacuum.</param>
        /// <param name="grid">Uniform grid information for 2D sampling.</param>
        /// <param name="materialProperty">Material property to sample (default: <see cref="EMDefaults.MatProperty"/>).</param>
        /// <param name="loopMode">Loop-computational mode (default: <see cref="Defaults.LoopOption"/>).</param>
        /// <returns>Sampled values as a <see cref="MatrixZ"/>.</returns>
        public MatrixZ Sample2D(double wavelength, GridInfo2D grid,
            MaterialProperty materialProperty = EMDefaults.MatProperty,
            LoopMode loopMode = Defaults.LoopOption)
        {
            Samp2DCplxFunc sfx = Get2DFunc(wavelength, materialProperty);
            return sfx.Sample(grid: grid, loopMode: loopMode);
        }

        #endregion
        #region ---- sample 1D ----

        /// <summary>
        /// Defines a 1D complex-valued function sampler for a slice of the 2D medium
        /// at a fixed value along one dimension, for a given wavelength and material property.
        /// </summary>
        /// <param name="wavelength">Wavelength in vacuum.</param>
        /// <param name="materialProperty">Material property to sample (N, Epsilon, or Mu).</param>
        /// <param name="c">
        /// The fixed coordinate value for the slice.
        /// If <paramref name="dim"/> is 1, this is x₁ (x1); if 2, this is x₂ (x2).
        /// </param>
        /// <param name="dim">
        /// The dimension to fix: 1 for x₁ (slice along x₂), 2 for x₂ (slice along x₁).
        /// </param>
        /// <returns>
        /// A <see cref="Samp1DCplxFunc"/> representing the 1D function along the unfixed dimension.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if Mu is requested but not defined.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="dim"/> is not 1 or 2.</exception>
        private Samp1DCplxFunc Get1DFunc(double wavelength,
            MaterialProperty materialProperty,
            double c, int dim = 1)
        {
            Func<double, double, double, Complex> func = materialProperty switch
            {
                MaterialProperty.N => N,
                MaterialProperty.Epsilon => Epsilon,
                MaterialProperty.Mu => Mu is not null ? Mu : throw new ArgumentNullException(nameof(Mu), "Mu is not defined."),
                _ => N
            };

            // dimension selection
            if (dim == 1) // x1 fixed @x; x2 varies
            { return new Samp1DCplxFunc(f: (x2) => func(wavelength, c, x2)); }
            else if (dim == 2) // x2 fixed @x; x1 varies
            { return new Samp1DCplxFunc(f: (x1) => func(wavelength, x1, c)); }
            else
            { throw new ArgumentOutOfRangeException(nameof(dim), "Index must be 1 or 2."); }
        }

        /// <summary>
        /// Samples a 1D slice of the specified material property at a fixed coordinate
        /// in one dimension, using scattered sample locations along the other dimension.
        /// </summary>
        /// <param name="wavelength">Wavelength in vacuum.</param>
        /// <param name="c">
        /// The fixed coordinate value for the slice.
        /// If <paramref name="dim"/> is 1, this is x₁ (x1); if 2, this is x₂ (x2).
        /// </param>
        /// <param name="scat">Scattered sample locations along the unfixed dimension.</param>
        /// <param name="materialProperty">Material property to sample (default: <see cref="EMDefaults.MatProperty"/>).</param>
        /// <param name="dim">
        /// The dimension to fix: 1 for x₁ (slice along x₂), 2 for x₂ (slice along x₁).
        /// </param>
        /// <param name="loopMode">Loop-computational mode (default: <see cref="Defaults.LoopOption"/>).</param>
        /// <returns>Sampled values as a <see cref="VectorZ"/>.</returns>
        public VectorZ Sample1D(double wavelength, double c,
            ScatInfo1D scat,
            MaterialProperty materialProperty = EMDefaults.MatProperty,
            int dim = 1, LoopMode loopMode = Defaults.LoopOption)
        {
            Samp1DCplxFunc sfx = Get1DFunc(wavelength, materialProperty, c, dim);
            return sfx.Sample(xs: scat, loopMode: loopMode);
        }

        /// <summary>
        /// Samples a 1D slice of the specified material property at a fixed coordinate
        /// in one dimension, using a uniform grid along the other dimension.
        /// </summary>
        /// <param name="wavelength">Wavelength in vacuum.</param>
        /// <param name="c">
        /// The fixed coordinate value for the slice.
        /// If <paramref name="dim"/> is 1, this is x₁ (x1); if 2, this is x₂ (x2).
        /// </param>
        /// <param name="grid">Uniform grid information along the unfixed dimension.</param>
        /// <param name="materialProperty">Material property to sample (default: <see cref="EMDefaults.MatProperty"/>).</param>
        /// <param name="dim">
        /// The dimension to fix: 1 for x₁ (slice along x₂), 2 for x₂ (slice along x₁).
        /// </param>
        /// <param name="loopMode">Loop-computational mode (default: <see cref="Defaults.LoopOption"/>).</param>
        /// <returns>Sampled values as a <see cref="VectorZ"/>.</returns>
        public VectorZ Sample1D(double wavelength, double c,
            GridInfo1D grid,
            MaterialProperty materialProperty = EMDefaults.MatProperty,
            int dim = 1, LoopMode loopMode = Defaults.LoopOption)
        {
            Samp1DCplxFunc sfx = Get1DFunc(wavelength, materialProperty, c, dim);
            return sfx.Sample(grid: grid, loopMode: loopMode);
        }

        #endregion

        #endregion
    }


    /// <summary>
    /// Represents a three-dimensional medium with spatially varying permittivity, permeability, and refractive index.
    /// Provides methods for sampling these properties on 2D slices or 1D lines.
    /// </summary>
    public class Medium3D
    {
        #region properties

        /// <summary>
        /// Gets or sets the permittivity distribution function ε(λ, x₁, x₂, x₃).
        /// <para>Parameter #1: λ - wavelength in vacuum.</para>
        /// <para>Parameter #2: x₁ - lateral position in the first dimension.</para>
        /// <para>Parameter #3: x₂ - lateral position in the second dimension.</para>
        /// <para>Parameter #4: x₃ - lateral position in the third dimension.</para>
        /// <para>Returns: complex permittivity at the specified parameters.</para>
        /// </summary>
        public Func<double, double, double, double, Complex> Epsilon { get; set; }
            = (λ, x1, x2, x3) => Complex.One;

        /// <summary>
        /// Gets or sets the permeability distribution function μ(λ, x₁, x₂, x₃).
        /// <para>Parameter #1: λ - wavelength in vacuum.</para>
        /// <para>Parameter #2: x₁ - lateral position in the first dimension.</para>
        /// <para>Parameter #3: x₂ - lateral position in the second dimension.</para>
        /// <para>Parameter #4: x₃ - lateral position in the third dimension.</para>
        /// <para>Returns: complex permeability at the specified parameters.</para>
        /// </summary>
        public Func<double, double, double, double, Complex>? Mu { get; set; }
            = (λ, x1, x2, x3) => Complex.One;

        /// <summary>
        /// Gets or sets the refractive index distribution function n(λ, x₁, x₂, x₃).
        /// <para>Parameter #1: λ - wavelength in vacuum.</para>
        /// <para>Parameter #2: x₁ - lateral position in the first dimension.</para>
        /// <para>Parameter #3: x₂ - lateral position in the second dimension.</para>
        /// <para>Parameter #4: x₃ - lateral position in the third dimension.</para>
        /// <para>Returns: complex refractive index at the specified parameters.</para>
        /// </summary>
        public Func<double, double, double, double, Complex> N { get; set; }
            = (λ, x1, x2, x3) => Complex.One;

        #endregion
        #region constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Medium3D"/> class with default properties (all set to unity).
        /// </summary>
        internal Medium3D() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Medium3D"/> class with specified permittivity and optional permeability functions.
        /// The refractive index is computed as n = sqrt(ε * μ) if μ is provided, otherwise n = sqrt(ε).
        /// </summary>
        /// <param name="epsilon">Permittivity distribution function ε(λ, x₁, x₂, x₃).</param>
        /// <param name="mu">Optional permeability distribution function μ(λ, x₁, x₂, x₃).</param>
        public Medium3D(Func<double, double, double, double, Complex> epsilon,
            Func<double, double, double, double, Complex>? mu = null)
        {
            Epsilon = epsilon;
            if (mu != null)
            {
                Mu = mu;
                N = (λ, x1, x2, x3) => Complex.Sqrt(epsilon(λ, x1, x2, x3) * mu(λ, x1, x2, x3));
            }
            else
            {
                N = (λ, x1, x2, x3) => Complex.Sqrt(epsilon(λ, x1, x2, x3));
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Medium3D"/> class with a specified refractive index function.
        /// The permittivity is set as ε = n².
        /// </summary>
        /// <param name="n">Refractive index distribution function n(λ, x₁, x₂, x₃).</param>
        public Medium3D(Func<double, double, double, double, Complex> n)
        {
            N = n;
            Epsilon = (λ, x1, x2, x3) => n(λ, x1, x2, x3) * n(λ, x1, x2, x3);
        }

        #endregion
        #region methods

        #region ---- sample 2D ----

        /// <summary>
        /// Defines a 2D complex-valued function sampler for a slice of the 3D medium
        /// at a fixed value along one dimension, for a given wavelength and material property.
        /// </summary>
        /// <param name="wavelength">Wavelength in vacuum.</param>
        /// <param name="materialProperty">Material property to sample (N, Epsilon, or Mu).</param>
        /// <param name="c">
        /// The fixed coordinate value for the slice.
        /// If <paramref name="dim"/> is 1, this is x₁ (x1); if 2, this is x₂ (x2); if 3, this is x₃ (x3).
        /// </param>
        /// <param name="dim">
        /// The dimension to fix: 1 for x₁ (slice along x₂, x₃), 2 for x₂ (slice along x₁, x₃), 3 for x₃ (slice along x₁, x₂).
        /// </param>
        /// <returns>
        /// A <see cref="Samp2DCplxFunc"/> representing the 2D function along the unfixed dimensions.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if Mu is requested but not defined.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="dim"/> is not 1, 2, or 3.</exception>
        private Samp2DCplxFunc Get2DFunc(double wavelength,
            MaterialProperty materialProperty,
            double c, int dim = 1)
        {
            Func<double, double, double, double, Complex> func = materialProperty switch
            {
                MaterialProperty.N => N,
                MaterialProperty.Epsilon => Epsilon,
                MaterialProperty.Mu => Mu is not null ? Mu : throw new ArgumentNullException(nameof(Mu), "Mu is not defined."),
                _ => N
            };

            // dimension selection
            if (dim == 1) // x1 fixed @x; x2, x3 varies
            { return new Samp2DCplxFunc(f: (x2, x3) => func(wavelength, c, x2, x3)); }
            else if (dim == 2) // x2 fixed @x; x1, x3 varies
            { return new Samp2DCplxFunc(f: (x1, x3) => func(wavelength, x1, c, x3)); }
            else if (dim == 3) // x3 fixed @x; x1, x2 varies
            { return new Samp2DCplxFunc(f: (x1, x2) => func(wavelength, x1, x2, c)); }
            else
            { throw new ArgumentOutOfRangeException(nameof(dim), "Index must be 1, 2 or 3."); }
        }

        /// <summary>
        /// Samples a 2D slice of the specified material property at a fixed coordinate
        /// in one dimension, using scattered sample locations along the other two dimensions.
        /// </summary>
        /// <param name="wavelength">Wavelength in vacuum.</param>
        /// <param name="c">
        /// The fixed coordinate value for the slice.
        /// If <paramref name="dim"/> is 1, this is x₁ (x1); if 2, this is x₂ (x2); if 3, this is x₃ (x3).
        /// </param>
        /// <param name="scat">Scattered sample locations along the unfixed dimensions.</param>
        /// <param name="materialProperty">Material property to sample (default: <see cref="EMDefaults.MatProperty"/>).</param>
        /// <param name="dim">
        /// The dimension to fix: 1 for x₁ (slice along x₂, x₃), 2 for x₂ (slice along x₁, x₃), 3 for x₃ (slice along x₁, x₂).
        /// </param>
        /// <param name="loopMode">Loop-computational mode (default: <see cref="Defaults.LoopOption"/>).</param>
        /// <returns>Sampled values as a <see cref="VectorZ"/>.</returns>
        public VectorZ Sample2D(double wavelength, double c,
            ScatInfo2D scat,
            MaterialProperty materialProperty = EMDefaults.MatProperty,
            int dim = 1, LoopMode loopMode = Defaults.LoopOption)
        {
            Samp2DCplxFunc sfx = Get2DFunc(wavelength, materialProperty, c, dim);
            return sfx.Sample(rho: scat, loopMode: loopMode);
        }

        /// <summary>
        /// Samples a 2D slice of the specified material property at a fixed coordinate
        /// in one dimension, using a uniform grid along the other two dimensions.
        /// </summary>
        /// <param name="wavelength">Wavelength in vacuum.</param>
        /// <param name="c">
        /// The fixed coordinate value for the slice.
        /// If <paramref name="dim"/> is 1, this is x₁ (x1); if 2, this is x₂ (x2); if 3, this is x₃ (x3).
        /// </param>
        /// <param name="grid">Uniform grid information along the unfixed dimensions.</param>
        /// <param name="materialProperty">Material property to sample (default: <see cref="EMDefaults.MatProperty"/>).</param>
        /// <param name="dim">
        /// The dimension to fix: 1 for x₁ (slice along x₂, x₃), 2 for x₂ (slice along x₁, x₃), 3 for x₃ (slice along x₁, x₂).
        /// </param>
        /// <param name="loopMode">Loop-computational mode (default: <see cref="Defaults.LoopOption"/>).</param>
        /// <returns>Sampled values as a <see cref="MatrixZ"/>.</returns>
        public MatrixZ Sample2D(double wavelength, double c,
            GridInfo2D grid,
            MaterialProperty materialProperty = EMDefaults.MatProperty,
            int dim = 1, LoopMode loopMode = Defaults.LoopOption)
        {
            Samp2DCplxFunc sfx = Get2DFunc(wavelength, materialProperty, c, dim);
            return sfx.Sample(grid: grid, loopMode: loopMode);
        }

        #endregion
        #region ---- sample 1D ----

        /// <summary>
        /// Defines a 1D complex-valued function sampler for a line of the 3D medium
        /// at fixed values along two dimensions, for a given wavelength and material property.
        /// </summary>
        /// <param name="wavelength">Wavelength in vacuum.</param>
        /// <param name="materialProperty">Material property to sample (N, Epsilon, or Mu).</param>
        /// <param name="c1">The first fixed coordinate value for the slice.</param>
        /// <param name="c2">The second fixed coordinate value for the slice.</param>
        /// <param name="dim">
        /// The dimension to vary: 1 for x₁ (x₂ = c1, x₃ = c2), 2 for x₂ (x₁ = c2, x₃ = c1), 3 for x₃ (x₁ = c1, x₂ = c2).
        /// </param>
        /// <returns>
        /// A <see cref="Samp1DCplxFunc"/> representing the 1D function along the unfixed dimension.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if Mu is requested but not defined.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="dim"/> is not 1, 2, or 3.</exception>
        private Samp1DCplxFunc Get1DFunc(double wavelength,
            MaterialProperty materialProperty,
            double c1, double c2, int dim = 3)
        {
            Func<double, double, double, double, Complex> func = materialProperty switch
            {
                MaterialProperty.N => N,
                MaterialProperty.Epsilon => Epsilon,
                MaterialProperty.Mu => Mu is not null ? Mu : throw new ArgumentNullException(nameof(Mu), "Mu is not defined."),
                _ => N
            };

            // dimension selection
            if (dim == 1) // x1 varies; x2 fixed @c1, x3 fixed @c2
            { return new Samp1DCplxFunc(f: (x1) => func(wavelength, x1, c1, c2)); }
            else if (dim == 2) // x2 varies; x3 fixed @c1, x1 fixed @c2
            { return new Samp1DCplxFunc(f: (x2) => func(wavelength, c2, x2, c1)); }
            else if (dim == 3) // x3 varies; x1 fixed @c1, x2 fixed @c2
            { return new Samp1DCplxFunc(f: (x3) => func(wavelength, c1, c2, x3)); }
            else
            { throw new ArgumentOutOfRangeException(nameof(dim), "Index must be 1, 2 or 3."); }
        }

        /// <summary>
        /// Samples a 1D line of the specified material property at fixed coordinates
        /// in two dimensions, using scattered sample locations along the remaining dimension.
        /// </summary>
        /// <param name="wavelength">Wavelength in vacuum.</param>
        /// <param name="c1">The first fixed coordinate value for the slice.</param>
        /// <param name="c2">The second fixed coordinate value for the slice.</param>
        /// <param name="scat">Scattered sample locations along the unfixed dimension.</param>
        /// <param name="materialProperty">Material property to sample (default: <see cref="EMDefaults.MatProperty"/>).</param>
        /// <param name="dim">
        /// The dimension to vary: 1 for x₁ (x₂ = c1, x₃ = c2), 2 for x₂ (x₁ = c2, x₃ = c1), 3 for x₃ (x₁ = c1, x₂ = c2).
        /// </param>
        /// <param name="loopMode">Loop-computational mode (default: <see cref="Defaults.LoopOption"/>).</param>
        /// <returns>Sampled values as a <see cref="VectorZ"/>.</returns>
        public VectorZ Sample1D(double wavelength, double c1, double c2,
            ScatInfo1D scat,
            MaterialProperty materialProperty = EMDefaults.MatProperty,
            int dim = 3, LoopMode loopMode = Defaults.LoopOption)
        {
            Samp1DCplxFunc sfx = Get1DFunc(wavelength, materialProperty, c1, c2, dim);
            return sfx.Sample(xs: scat, loopMode: loopMode);
        }

        /// <summary>
        /// Samples a 1D line of the specified material property at fixed coordinates
        /// in two dimensions, using a uniform grid along the remaining dimension.
        /// </summary>
        /// <param name="wavelength">Wavelength in vacuum.</param>
        /// <param name="c1">The first fixed coordinate value for the slice.</param>
        /// <param name="c2">The second fixed coordinate value for the slice.</param>
        /// <param name="grid">Uniform grid information along the unfixed dimension.</param>
        /// <param name="materialProperty">Material property to sample (default: <see cref="EMDefaults.MatProperty"/>).</param>
        /// <param name="dim">
        /// The dimension to vary: 1 for x₁ (x₂ = c1, x₃ = c2), 2 for x₂ (x₁ = c2, x₃ = c1), 3 for x₃ (x₁ = c1, x₂ = c2).
        /// </param>
        /// <param name="loopMode">Loop-computational mode (default: <see cref="Defaults.LoopOption"/>).</param>
        /// <returns>Sampled values as a <see cref="VectorZ"/>.</returns>
        public VectorZ Sample1D(double wavelength, double c1, double c2,
            GridInfo1D grid,
            MaterialProperty materialProperty = EMDefaults.MatProperty,
            int dim = 3, LoopMode loopMode = Defaults.LoopOption)
        {
            Samp1DCplxFunc sfx = Get1DFunc(wavelength, materialProperty, c1, c2, dim);
            return sfx.Sample(grid: grid, loopMode: loopMode);
        }

        #endregion

        #endregion

    }
}
