using VEMS.MathCore;

namespace VEMS.EMSolver
{

    /// <summary>
    /// two-dimensional idealized lens
    /// => phase only transmission
    /// </summary>
    public class IdealLens2D : Transmission2D, ILens, IOpticalComponent
    {
        #region ---- IOptcalComponent ----

        /// <summary>
        /// Gets or sets the label associated with the optical component.
        /// </summary>
        public string? Label { get; set; } = null;

        /// <summary>
        /// Gets or sets the processing function for the optical component.
        /// This function defines how the component processes an <see cref="SCField"/> and returns the resulting <see cref="SCField"/>.
        /// </summary>
        public Func<SCField, SCField>? Process { get; set; } = null;

        /// <summary>
        /// Gets or sets the detection function for the optical component.
        /// This function defines how the component processes an <see cref="SCField"/> and returns the resulting <see cref="MatrixD"/>,
        /// typically representing a detected intensity or measurement on the field.
        /// </summary>
        public Func<SCField, Grid2DRealData>? Detect { get; set; } = null;

        /// <summary>
        /// Coodinate system of the thin component ( Input direction )
        /// </summary>
        public CoordinateSystem? Coordinate { get; set; } = null;

        /// <summary>
        /// Gets the output coordinate system of the optical component.
        /// If the <see cref="Coordinate"/> property is <c>null</c>, returns a new <see cref="CoordinateSystem"/>
        /// at the origin with zero rotation. Otherwise, returns the value of <see cref="Coordinate"/>.
        /// </summary>
        public CoordinateSystem? OutputCoordinate { get; set; } = null;

        #endregion

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
        public IdealLens2D() 
        {
            NReal = (w) => 1.0;
        }

        /// <summary>
        /// constructs a 2D idealized lens with given parameters
        /// </summary>
        /// <param name="focalLength"> focal length </param>
        /// <param name="wavelength"> wavelength in vacuum </param>
        /// <param name="nReal"> refractive index </param>
        /// <param name="offset"> constant offset of the phase function </param>
        /// <param name="shiftX"> lateral shift along x </param>
        /// <param name="shiftY"> lateral shift along y </param>
        public IdealLens2D(double focalLength,
            double wavelength,
            Func<double, double>? nReal = null,
            double offset = 0.0,
            double shiftX = 0.0, double shiftY = 0.0)
            : base(shiftX, shiftY, scaling: 1.0)
        {
            FocalLength = focalLength;
            Wavelength = wavelength;
            NReal = nReal ?? ((w) => 1.0); // default is vacuum
            Offset = offset;
        }

        /// <summary>
        /// constructs a 2D idealized lens with given parameters
        /// </summary>
        /// <param name="focalLength"> focal length </param>
        /// <param name="wavelength"> wavelength in vacuum </param>
        /// <param name="nReal"> refractive index </param>
        /// <param name="offset"> constant offset of the phase function </param>
        /// <param name="shiftX"> lateral shift along x </param>
        /// <param name="shiftY"> lateral shift along y </param>
        /// <param name="label"></param>
        /// <param name="coordinate"> coordinate system of the lens </param>
        /// <param name="loopMode"></param>
        public IdealLens2D(double focalLength,
            double wavelength,
            Func<double, double>? nReal = null,
            double offset = 0.0,
            double shiftX = 0.0, double shiftY = 0.0, 
            string? label = null,
            CoordinateSystem? coordinate = null,
            LoopMode loopMode = Defaults.LoopOption)
            : base(shiftX, shiftY, scaling: 1.0)
        {
            FocalLength = focalLength;
            Wavelength = wavelength;
            NReal = nReal ?? ((w) => 1.0); // default is vacuum
            Offset = offset;
            // optical component
            Label = label ?? GetType().FullName;
            Coordinate = coordinate ?? CoordinateSystem.Origin;
            Process = (v) =>
            {
                SCField result = v;
                ModulateOn(ref result, loopMode);
                return result;
            };
            OutputCoordinate = Coordinate; // output coordinate is the same as the input for transmission models
        }

        #endregion
        #region methods

        // ...

        #endregion
        #region sub-classes

        /// <summary>
        /// lens function defined by quadratic phase
        /// </summary>
        public class Quadratic : IdealLens2D
        {
            #region constructor

            /// <summary>
            /// constructs a 2D ideal lens with quadratic phase
            /// </summary>
            /// <param name="focalLength"> focal length </param>
            /// <param name="wavelength"> wavelength in vacuum </param>
            /// <param name="nReal"> refractive index </param>
            /// <param name="offset"> constant offset of the phase function </param>
            /// <param name="shiftX"> lateral shift along x </param>
            /// <param name="shiftY"> lateral shift along y </param>
            public Quadratic(double focalLength,
                double wavelength,
                Func<double, double>? nReal = null,
                double offset = 0.0,
                double shiftX = 0.0, double shiftY = 0.0)
                : base(focalLength, wavelength,
                      nReal, offset, shiftX, shiftY)
            {
                // parameters
                double k0 = 2.0 * Math.PI / Wavelength;
                double n = (NReal == null) ? 1.0 : NReal(wavelength);
                // constant offset
                Offset += k0 * n * FocalLength;
                double a = -k0 * n / (2.0 * FocalLength);
                // defines the quadratic phase function
                Phase = (x, y) => Offset + Function2D.Quadratic(x, y, [a, a]);
            }

            /// <summary>
            /// constructs a 2D ideal lens with quadratic phase
            /// </summary>
            /// <param name="focalLength"> focal length </param>
            /// <param name="wavelength"> wavelength in vacuum </param>
            /// <param name="nReal"> refractive index </param>
            /// <param name="offset"> constant offset of the phase function </param>
            /// <param name="shiftX"> lateral shift along x </param>
            /// <param name="shiftY"> lateral shift along y </param>
            /// <param name="label"></param>
            /// <param name="coordinate"></param>
            /// <param name="loopMode"></param>
            public Quadratic(double focalLength,
                double wavelength,
                Func<double, double>? nReal = null,
                double offset = 0.0,
                double shiftX = 0.0, double shiftY = 0.0, 
                string? label = null,
                CoordinateSystem? coordinate = null,
                LoopMode loopMode = Defaults.LoopOption)
                : base(focalLength, wavelength, nReal, offset, shiftX, shiftY,
                      label, coordinate, loopMode)
            {
                // parameters
                double k0 = 2.0 * Math.PI / Wavelength;
                double n = (NReal == null) ? 1.0 : NReal(wavelength);
                // constant offset
                Offset += k0 * n * FocalLength;
                double a = -k0 * n / (2.0 * FocalLength);
                // defines the quadratic phase function
                Phase = (x, y) => Offset + Function2D.Quadratic(x, y, [a, a]);
            }

            #endregion
        }

        /// <summary>
        /// lens function defines by spheric phase
        /// </summary>
        public class Spheric : IdealLens2D
        {
            #region constructor

            /// <summary>
            /// constructs a 2D ideal lens with cylindric phase
            /// </summary>
            /// <param name="focalLength"> focal length </param>
            /// <param name="wavelength"> wavelength in vacuum </param>
            /// <param name="nReal"> refractive index </param>
            /// <param name="offset"> constant offset of the phase function </param>
            /// <param name="shiftX"> lateral shift along x </param>
            /// <param name="shiftY"> lateral shift along y </param>
            public Spheric(double focalLength,
                double wavelength,
                Func<double, double>? nReal = null,
                double offset = 0.0,
                double shiftX = 0.0, double shiftY = 0.0)
                : base(focalLength, wavelength, 
                      nReal, offset, shiftX, shiftY)
            {
                // parameters
                double k0 = 2.0 * Math.PI / Wavelength;
                double n = (NReal == null) ? 1.0 : NReal(Wavelength);
                // defines the spheric phase function
                Phase = (x, y) => Offset + Function2D.Spheric(x, y,
                    [-FocalLength, k0 * n]);
            }

            /// <summary>
            /// constructs a 2D ideal lens with cylindric phase
            /// </summary>
            /// <param name="focalLength"> focal length </param>
            /// <param name="wavelength"> wavelength in vacuum </param>
            /// <param name="nReal"> refractive index </param>
            /// <param name="offset"> constant offset of the phase function </param>
            /// <param name="shiftX"> lateral shift along x </param>
            /// <param name="shiftY"> lateral shift along y </param>
            /// <param name="label"></param>
            /// <param name="coordinate"></param>
            /// <param name="loopMode"></param>
            public Spheric(double focalLength,
                double wavelength,
                Func<double, double>? nReal = null,
                double offset = 0.0,
                double shiftX = 0.0, double shiftY = 0.0,
                string? label = null,
                CoordinateSystem? coordinate = null,
                LoopMode loopMode = Defaults.LoopOption)
                : base(focalLength, wavelength, nReal, offset, shiftX, shiftY,
                      label, coordinate, loopMode)
            {
                // parameters
                double k0 = 2.0 * Math.PI / Wavelength;
                double n = (NReal == null) ? 1.0 : NReal(Wavelength);
                // defines the spheric phase function
                Phase = (x, y) => Offset + Function2D.Spheric(x, y,
                    [-FocalLength, k0 * n]);
            }

            #endregion
        }

        #endregion
    }
}
