using VEMS.MathCore;

namespace VEMS.EMSolver
{

    /// <summary>
    /// one-dimensional aberration function
    /// => phase-only transmission
    /// </summary>
    public class Aberration1D : Transmission1D
    {
        #region properties

        // ...

        #endregion
        #region constructors

        /// <summary>
        /// default constructor
        /// </summary>
        public Aberration1D() { }

        /// <summary>
        /// constructs a 1D aberration with given parameters
        /// </summary>
        /// <param name="shift"> lateral shift </param>
        /// <param name="scaling"> overall constant scaling factor </param>
        public Aberration1D(double shift = 0.0,
            double scaling = 1.0)
        {
            Shift = shift;
            Scaling = scaling;
        }

        #endregion
        #region methods

        // ...

        #endregion
        #region sub-classes

        /// <summary>
        /// 1D aberration defined by polynomial 
        /// </summary>
        public class Polynomial : Aberration1D
        {
            #region constructor

            /// <summary>
            /// constructs a 1D aberration using polynomial 
            /// </summary>
            /// <param name="p"> list of polynomial coefficients </param>
            /// <param name="shift"> lateral shift </param>
            /// <param name="scaling"> overall constant scaling factor </param>
            public Polynomial(List<double> p,
                double shift = 0.0, double scaling = 1.0)
                : base(shift, scaling)
            {
                Phase = (x) => Scaling * Function1D.Polynomial(x, p);
            }

            #endregion
        }

        // more ...

        #endregion
    }
}
