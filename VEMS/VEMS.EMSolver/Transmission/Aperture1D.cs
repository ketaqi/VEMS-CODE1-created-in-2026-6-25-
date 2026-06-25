using VEMS.MathCore;

namespace VEMS.EMSolver
{

    /// <summary>
    /// one-dimensional apeture
    /// => real-valued transmission
    /// </summary>
    public class Aperture1D : Transmission1D
    {
        #region properties

        /// <summary>
        /// diameter of the aperture
        /// </summary>
        public double Diameter { get; set; }

        #endregion
        #region constructor

        /// <summary>
        /// default constructor
        /// </summary>
        public Aperture1D() { }

        /// <summary>
        /// constructs an Aperture1D with given parameters
        /// </summary>
        /// <param name="diameter"> diameter of the aperture </param>
        /// <param name="shift"> lateral shift of the aperture </param>
        /// <param name="scaling"> overall scaling factor </param>
        public Aperture1D(double diameter,
            double shift = 0.0, double scaling = 1.0)
            : base(shift, scaling)
        {
            Diameter = diameter;
        }

        #endregion
        #region methods 

        // ...
            
        #endregion
        #region sub-classes

        /// <summary>
        /// rectangular aperture with cosine-smoothed edges
        /// </summary>
        public class Rectangular : Aperture1D
        {
            #region properties

            /// <summary> : 
            /// absolute edge width of the aperture (half within, half outside)
            /// </summary>
            public double EdgeWidth { get; set; }

            #endregion
            #region constructor

            /// <summary>
            /// constructs an aperture with rectangular shape
            /// and possibly smoothed edges
            /// </summary>
            /// <param name="diameter"> diameter of the rectangle </param>
            /// <param name="edgeWidth"> absolute edge width of the rectangle (half within, half outside) </param>
            /// <param name="shift"> lateral shift of the aperture </param>
            /// <param name="scaling"> overall scaling of the aperture transmission </param>
            public Rectangular(double diameter,
                double edgeWidth = 0.0,
                double shift = 0.0,
                double scaling = 1.0)
                : base(diameter, shift, scaling)
            {
                EdgeWidth = edgeWidth;
                // defines the rectangular aperture function
                Amplitude = (x) => Scaling * Function1D.
                CosEdgeRectangle(x, [Diameter, EdgeWidth]);
            }

            #endregion
        }

        /// <summary>
        /// Gaussian aperture
        /// </summary>
        public class Gaussian : Aperture1D
        {
            #region properties

            /// <summary>
            /// waist radius of the Gaussian profile
            /// </summary>
            private double WaistRadius => 0.5 * Diameter;

            #endregion
            #region constructor

            /// <summary>
            /// constructs a Gaussian aperture
            /// </summary>
            /// <param name="diameter"> diameter of the Gaussian profile (1/e) </param>
            /// <param name="shift"> lateral shift of the aperture </param>
            /// <param name="scaling"> overall scaling of the aperture transmission </param>
            public Gaussian(double diameter,
                double shift = 0.0, 
                double scaling = 1.0)
                : base(diameter, shift, scaling)
            {
                // defines the Gaussian aperture function
                Amplitude = (x) => Scaling * Function1D.
                Gaussian(x, [WaistRadius]);
            }
            
            #endregion
        }

        #endregion
    }
}
