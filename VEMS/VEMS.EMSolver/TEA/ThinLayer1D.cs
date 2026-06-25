using VEMS.MathCore;
using Complex = System.Numerics.Complex;

namespace VEMS.EMSolver
{
    /// <summary>
    /// one-dimentional (1D) thin-layer class
    /// </summary>
    [Obsolete]
    public class ThinLayer1D : Layer1DMedium
    {
        #region properties

        /// <summary>
        /// thickness of the layer along the z-direction
        /// </summary>
        public double Thickness { get; set; }

        #endregion
        #region constructors

        /// <summary>
        /// default constructor
        /// </summary>
        internal ThinLayer1D() { }

        /// <summary>
        /// constructs a 1D thin-element with given 
        /// permittivity and/or permeability distribution
        /// </summary>
        /// <param name="epsilon"> permittivity distribution epsilon = f(λ,x) </param>
        /// <param name="mu"> permeability distribution mu = f(λ,x) </param>
        /// <param name="thickness"> thickness of the medium along the z-direction </param>
        public ThinLayer1D(Func<double, double, Complex> epsilon,
            Func<double, double, Complex>? mu = null,
            double thickness = 0.0)
            : base(epsilon, mu)
        { Thickness = thickness; }

        /// <summary>
        /// constructs a 1D thin-element with given 
        /// refractive index distribution
        /// </summary>
        /// <param name="n"> refractive index distribution n = n(λ,x) </param>
        /// <param name="thickness"> thickness of the medium along the z-direction </param>
        public ThinLayer1D(Func<double, double, Complex> n,
            double thickness = 0.0) : base(n)
        { Thickness = thickness; }

        #endregion
        #region methods

        /// <summary>
        /// computes paraxial transmission function using
        /// thin-element-approximation (TEA) method
        /// for a fixed wavelength λ and a given distance
        /// </summary>
        /// <param name="wavelength"> wavelength in vacuum </param>
        /// <param name="grid"> target uniform sampling grid </param>
        /// <param name="d"> propagation distance along the z-direction </param>
        /// <param name="n0Re"> real-part of constant reference refractive index to subtract </param>
        /// <param name="n0Im"> imag-part of constant reference refractive index to subtract </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> complex transmission result </returns>
        [Obsolete]
        public VectorZ ParaxialTransmission(double wavelength,
            GridInfo1D grid,
            double d,
            double n0Re = 0.0, double n0Im = 0.0,
            LoopMode loopMode = Defaults.LoopOption)
        {
            double k0 = 2.0 * Math.PI / wavelength;
            // subtracts reference n0, if n0 is not zero
            Complex n0 = new(n0Re, n0Im);
            Func<double, Complex> n = (x) => N(wavelength, x) - n0;
            // defines transmission function
            Func<double, Complex> t = (x) =>
            Complex.Exp(Complex.ImaginaryOne * k0 * n(x) * d);
            // samples transmission function on the grid
            return new Samp1DCplxFunc(t).Sample(grid, loopMode);
        }

        /// <summary>
        /// computes paraxial transmission function using
        /// thin-element-approximation (TEA) method
        /// for a fixed wavelength λ
        /// </summary>
        /// <param name="wavelength"> wavelength in vacuum </param>
        /// <param name="n0Re"> real-part of constant reference refractive index to subtract </param>
        /// <param name="n0Im"> imag-part of constant reference refractive index to subtract </param>
        /// <returns> phase-only transmission function </returns>
        public Transmission1D ParaxialTransmission(double wavelength,
            double n0Re = 0.0, double n0Im = 0.0)
        {
            double k0 = 2.0 * Math.PI / wavelength;
            // subtracts reference n0, if n0 is not zero
            Complex n0 = new(n0Re, n0Im);
            Func<double, Complex> n = (x) => N(wavelength, x) - n0;
            // defines transmission function
            Transmission1D t = new()
            { Phase = (x) => k0 * n(x).Real * Thickness };
            return t;
        }

        #endregion
    }

}
