namespace VEMS.EMSolver
{
    /// <summary>
    /// default parameters for the EM-solver
    /// </summary>
    public class EMDefaults
    {
        /// <summary>
        /// default wavelength for the EM-solver
        /// </summary>
        public const double Wavelength = 632.8E-9;

        /// <summary>
        /// default material property for the EM-solver
        /// </summary>
        public const MaterialProperty MatProperty = MaterialProperty.N;

        /// <summary>
        /// default algorithm option for free-space propagation
        /// </summary>
        public const FreeSpaceOption FreeSpaceAlgorithm = FreeSpaceOption.SPW;
    
    }


    /// <summary>
    /// options for material property
    /// </summary>
    public enum MaterialProperty
    {
        /// <summary>
        /// permittivity
        /// </summary>
        Epsilon,

        /// <summary>
        /// permeability
        /// </summary>
        Mu,

        /// <summary>
        /// complex refractive index
        /// </summary>
        N,

        ///// <summary>
        ///// real part of refractive index
        ///// </summary>
        //NReal,

        ///// <summary>
        ///// imaginary part of refractive index
        ///// </summary>
        //NImag,
    }


    /// <summary>
    /// options for free-space propagation calculation
    /// </summary>
    public enum FreeSpaceOption
    {
        /// <summary>
        /// angular spectrum of plane waves
        /// </summary>
        SPW,

        /// <summary>
        /// Rayleigh-Sommerfeld integral
        /// </summary>
        RayleighSommerfeld,

        /// <summary>
        /// Fresnel diffraction integral 
        /// </summary>
        Fresnel,

    }

}
