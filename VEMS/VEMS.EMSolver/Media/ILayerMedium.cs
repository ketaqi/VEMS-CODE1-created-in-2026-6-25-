using VEMS.MathCore;
using Complex = System.Numerics.Complex;

namespace VEMS.EMSolver
{
    public interface ILayerMedium
    {
        #region properties
        /// <summary>
        /// Gets the permittivity (epsilon) of the layer medium.
        /// </summary>
        Func<double, double, Complex> Epsilon { get; }

        /// <summary>
        /// Gets the permeability (mu) of the layer medium. Can be null.
        /// </summary>
        Func<double, double, Complex>? Mu { get; }

        #region ==== cache info ====

        /// <summary>
        /// Gets or sets a value indicating whether to cache the sampled data.
        /// </summary>
        bool CacheSampleData { get; set; }

        /// <summary>
        /// Gets or sets the sampled medium data at a fixed wavelength
        /// for the selected material property.
        /// </summary>
        VectorZ? SampleData { get; set; }

        /// <summary>
        /// Gets or sets the uniform grid used for sampling.
        /// </summary>
        GridInfo1D? SampGrid { get; set; }

        /// <summary>
        /// Gets or sets the selected material property for sampling.
        /// </summary>
        MaterialProperty SelectedProp { get; set; }

        #endregion

        #endregion
        #region
        /// <summary>
        /// Samples the specified material property of the medium at a fixed wavelength
        /// on a target uniform grid in space.
        /// </summary>
        (VectorZ epsilon, VectorZ? mu) SampleMedium(double wavelength, GridInfo1D grid,
            LoopMode loopMode = Defaults.LoopOption,
            bool cacheSampleData = true);

        /// <summary>
        /// Samples the permittivity in the k-domain (Fourier domain) using the direct rule.
        /// </summary>
        (VectorZ epsilonK, VectorZ? muK) MediumInDirKdomain(double wavelength, GridInfo1D grid,
            LoopMode loopMode = Defaults.LoopOption,
            bool cacheSampleData = false);

        /// <summary>
        /// Samples the permittivity in the k-domain (Fourier domain) using the inverse rule.
        /// </summary>
        (VectorZ epsilonK, VectorZ? muK) MediumInInvKdomain(double wavelength, GridInfo1D grid,
            LoopMode loopMode = Defaults.LoopOption,
            bool cacheSampleData = false);
        #endregion

    }
}
