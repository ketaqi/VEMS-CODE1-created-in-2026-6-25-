using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace VEMS.EMSolver
{
    /// <summary>
    /// medium with refractive index distribution n = n(λ, x, z)
    /// </summary>
    public class MediumXZ
    {
        #region properties

        private Func<double, double, double, Complex> epsilon;
        private Func<double, double, double, Complex> mu = (w, x, z) => Complex.One;
        private Func<double, double, double, Complex> n;


        /// <summary>
        /// permittivity distribution epsilon = f(λ, x, z)
        /// <para> parameter #1: λ wavelength in vacuum </para>
        /// <para> parameter #2: x lateral position </para>
        /// <para> parameter #3: z longitudinal position </para>
        /// <para> result: complex permittivity distribution </para>
        /// </summary>
        public Func<double, double, double, Complex> Epsilon
        {
            get => epsilon;
            [MemberNotNull(nameof(n), nameof(epsilon))]
            set
            {
                epsilon = value;
                n = (w, x, z) => Complex.Sqrt(epsilon(w, x, z) * mu(w, x, z));
            }
        }

        /// <summary>
        /// permeability distribution mu = f(λ, x, z)
        /// <para> parameter #1: λ wavelength in vacuum </para>
        /// <para> parameter #2: x lateral position </para>
        /// <para> parameter #3: z longitudinal position </para>
        /// <para> result: complex permeability distribution </para>
        /// </summary>
        public Func<double, double, double, Complex> Mu
        {
            get => mu;
            set => mu = value;
        }

        /// <summary>
        /// refractive index distribution n = n(λ, x, z)
        /// <para> parameter #1: λ wavelength in vacuum </para>
        /// <para> parameter #2: x lateral position </para>
        /// <para> parameter #3: z longitudinal position </para>
        /// <para> result: complex refractive index </para>
        /// </summary>
        public Func<double, double, double, Complex> N
        {
            get => n;
            [MemberNotNull(nameof(n), nameof(epsilon))]
            set
            {
                n = value;
                epsilon = (w, x, z) => n(w, x, z) * n(w, x, z);
            }
        }

        #endregion
        #region constructors

        /// <summary>
        /// default constructor
        /// </summary>
        internal MediumXZ()
        {
            Epsilon = (λ, x, z) => Complex.One;
        }

        /// <summary>
        /// constructs a MediumXZ with given permittivity and 
        /// permeability distributions
        /// </summary>
        /// <param name="epsilon"> permittivity distribution epsilon = f(λ, x, z) </param>
        /// <param name="mu"> permeability distribution mu = f(λ, x, z) </param>
        public MediumXZ(Func<double, double, double, Complex> epsilon,
            Func<double, double, double, Complex>? mu = null)
        {
            Epsilon = epsilon;
            if (mu != null) { Mu = mu; }
        }

        /// <summary>
        /// constructs a MediumXZ with given
        /// refractive index distribution 
        /// </summary>
        /// <param name="n"> refractive index n = n(λ,x,z) </param>
        public MediumXZ(Func<double, double, double, Complex> n)
        {
            N = n;
        }

        #endregion
        #region methods

        /// <summary>
        /// computes the refractive index distribution n = n(λ,x,z0) at a fixed z-position
        /// <para> parameter #1: λ wavelength in vacuum </para>
        /// <para> parameter #2: x lateral position </para>
        /// <para> result: complex refractive index </para>
        /// </summary>
        /// <param name="z0"> fixed z-position </param>
        /// <returns></returns>
        public Func<double, double, Complex> Slide(double z0)
            => (λ, x) => N(λ, x, z0);

        #endregion
    }


}
