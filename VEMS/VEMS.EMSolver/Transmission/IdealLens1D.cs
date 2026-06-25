using VEMS.MathCore;

namespace VEMS.EMSolver
{

    /// <summary>
    /// one-dimensional idealized lens
    /// => phase-only transmission
    /// </summary>
    public class IdealLens1D : Transmission1D, ILens
    {
        #region properties

        /// <summary>
        /// focal length
        /// </summary>
        public double FocalLength { get; set; }

        /// <summary>
        /// working wavelength in vacuum
        /// </summary>
        public double Wavelength { get; set; }

        /// <summary>
        /// refractive index [real or real-part only]
        /// </summary>
        public Func<double, double> NReal { get; set; }

        /// <summary>
        /// constant offset of the phase function
        /// </summary>
        public double Offset { get; set; }

        #endregion
        #region constructor

        /// <summary>
        /// default constructor
        /// </summary>
        public IdealLens1D() 
        {
            NReal = (w) => 1.0;
        }

        /// <summary>
        /// constructs a 1D ideal lens with given parameters
        /// </summary>
        /// <param name="focalLength"> focal length </param>
        /// <param name="wavelength"> wavelength in vacuum </param>
        /// <param name="nReal"> refractive index </param>
        /// <param name="offset"> constant offset of the phase function </param>
        /// <param name="shift"> lateral shift </param>
        public IdealLens1D(double focalLength, 
            double wavelength, 
            Func<double, double>? nReal = null,
            double offset = 0.0,
            double shift = 0.0)
            : base(shift, scaling: 1.0)
        {
            FocalLength = focalLength;
            Wavelength = wavelength;
            NReal = nReal ?? ((w) => 1.0); // default is vacuum
            Offset = offset;
        }

        #endregion
        #region methods

        // ...

        #endregion
        #region sub-classes

        /// <summary>
        /// lens function defined by quadratic phase
        /// </summary>
        public class Quadratic : IdealLens1D
        {
            #region constructor

            /// <summary>
            /// constructs a 1D ideal lens with quadratic phase
            /// </summary>
            /// <param name="focalLength"> focal length </param>
            /// <param name="wavelength"> wavelength in vacuum </param>
            /// <param name="nReal"> refractive index </param>
            /// <param name="offset"> constant offset of the phase function </param>
            /// <param name="shift"> lateral shift </param>
            public Quadratic(double focalLength, 
                double wavelength,
                Func<double, double>? nReal = null,
                double offset = 0.0,
                double shift = 0.0) 
                : base(focalLength, wavelength,
                    nReal, offset, shift)
            {
                // parameters
                double k0 = 2.0 * Math.PI / Wavelength;
                double n = (NReal == null)? 1.0 : NReal.Invoke(wavelength);
                // constant offset
                Offset += k0 * n * FocalLength;
                // defines the quadratic phase function
                Phase = (x) => Offset + Function1D.Quadratic(x, 
                    new() { -k0 * n / (2.0 * FocalLength) });
            }

            #endregion
        }

        /// <summary>
        /// lens function defined by cylindric phase
        /// </summary>
        public class Cylindric : IdealLens1D
        {
            #region constructor

            /// <summary>
            /// constructs a 1D ideal lens with cylindric phase
            /// </summary>
            /// <param name="focalLength"> focal length </param>
            /// <param name="wavelength"> wavelength in vacuum </param>
            /// <param name="nReal"> refractive index </param>
            /// <param name="offset"> constant offset of the phase function </param>
            /// <param name="shift"> lateral shift </param>
            public Cylindric(double focalLength,
                double wavelength,
                Func<double, double>? nReal = null,
                double offset = 0.0,
                double shift = 0.0) 
                : base(focalLength, wavelength,
                      nReal, offset, shift)
            {
                // parameters
                double k0 = 2.0 * Math.PI / Wavelength;
                double n = (NReal == null) ? 1.0 : NReal.Invoke(Wavelength);
                // defines the cylindric phase function
                Phase = (x) => Offset + Function1D.Cylindric(x, 
                    new() { -FocalLength, k0 * n });
            }

            #endregion
        }

        #endregion
    }

}
