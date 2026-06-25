using VEMS.MathCore;

namespace VEMS.EMSolver
{
    /// <summary>
    /// two-dimensional idealized gratings
    /// either amplitude or phase transmission
    /// </summary>
    public class IdealGrating2D : Transmission2D
    {
        #region properties

        /// <summary>
        /// period of the grating along x direction
        /// </summary>
        public double PeriodX 
        { 
            get => Dx;
            set => Dx = value; 
        }

        /// <summary>
        /// period of the grating along y direction
        /// </summary>
        public double PeriodY 
        { 
            get => Dy; 
            set => Dy = value; 
        }

        #endregion
        #region constructors

        /// <summary>
        /// default constructor
        /// </summary>
        public IdealGrating2D() { }

        /// <summary>
        /// constructs an idealized grating with given parameters
        /// </summary>
        /// <param name="periodX"> period of the grating along x direction </param>
        /// <param name="periodY"> period of the grating along y direction </param>
        /// <param name="shiftX"> lateral shift of the grating along x direction </param>
        /// <param name="shiftY"> lateral shift of the grating along y direction </param>
        /// <param name="scaling"> scaling of the grating function </param>
        public IdealGrating2D(double periodX, double periodY,
            double shiftX, double shiftY, 
            double scaling = 1.0)
            : base(shiftX, shiftY, scaling)
        {
            PeriodX = periodX;
            PeriodY = periodY;
        }

        #endregion
        #region methods

        // ...

        #endregion
        #region sub-classes

        /// <summary>
        /// 
        /// </summary>
        internal class Rectangular : IdealGrating1D
        {

        }

        /// <summary>
        /// 
        /// </summary>
        internal class Sinusoidal : IdealGrating1D
        {

        }


        /// <summary>
        /// grating defined by polygon-contour
        /// </summary>
        public class PolyContour: IdealGrating2D
        {
            #region properties

            /// <summary>
            /// list of polygons that defines the grating pattern
            /// </summary>
            public List<Geometry2D.Polygon> Patterns { get; set; }

            /// <summary>
            /// whether the inner region is opaque or transparent
            /// </summary>
            public bool IsInnerOpaque { get; set; } = true;

            #endregion
            #region constructors

            /// <summary>
            /// default constructor
            /// </summary>
            internal PolyContour() { Patterns = []; }

            /// <summary>
            /// constructs a polygon-contour grating with its pattern
            /// defined by multiple polygons
            /// </summary>
            /// <param name="periodX"> period of the grating along the x-direction </param>
            /// <param name="periodY"> period of the grating along the y-direction </param>
            /// <param name="patterns"> multiple polygons used to define the grating pattern </param>
            /// <param name="isInnerOpaque"> whether the inner region is opaque </param>
            /// <param name="shiftX"> lateral shift along the x-direction </param>
            /// <param name="shiftY"> lateral shift along the y-direction </param>
            /// <param name="scaling"> scaling factor </param>
            /// <param name="type"> type of the grating modulation: amplitude or phase </param>
            public PolyContour(double periodX, double periodY,
                List<Geometry2D.Polygon> patterns, bool isInnerOpaque = true,
                double shiftX = 0.0, double shiftY = 0.0, double scaling = 1.0,
                TransmissionType type = TransmissionType.Amplitude)
                : base(periodX, periodY, shiftX, shiftY, scaling)
            {
                Patterns = patterns;
                IsInnerOpaque = isInnerOpaque;
                // defines inner and outer values
                double inValue = IsInnerOpaque ? 0.0 : Scaling;
                double outValue = IsInnerOpaque ? Scaling : 0.0;
                // defines amplitude and phase
                switch (type)
                {
                    case TransmissionType.Amplitude:
                        {
                            Amplitude = (x, y) => Geometry2D.Polygon.FillValue(x, y,
                                Patterns, inValue, outValue);
                            Phase = (x, y) => 0.0;
                            break;
                        }
                    case TransmissionType.Phase:
                        {
                            Amplitude = (x, y) => 1.0;
                            Phase = (x, y) => Geometry2D.Polygon.FillValue(x, y,
                                Patterns, inValue, outValue);
                            break;
                        }
                    default: goto case TransmissionType.Amplitude;
                }
            }

            /// <summary>
            /// constructs a  polygon-contour grating with its pattern
            /// defined by a single polygon
            /// </summary>
            /// <param name="periodX"> period of the grating along the x-direction </param>
            /// <param name="periodY"> period of the grating along the y-direction </param>
            /// <param name="pattern"> polygon used to define the grating pattern </param>
            /// <param name="isInnerOpaque"> whether the inner region is opaque </param>
            /// <param name="shiftX"> lateral shift along the x-direction </param>
            /// <param name="shiftY"> lateral shift along the y-direction </param>
            /// <param name="scaling"> scaling factor </param>
            /// <param name="type"> type of the grating modulation: amplitude or phase </param>
            public PolyContour(double periodX, double periodY,
                Geometry2D.Polygon pattern, bool isInnerOpaque = true,
                double shiftX = 0.0, double shiftY = 0.0, double scaling = 1.0,
                TransmissionType type = TransmissionType.Amplitude)
                : this(periodX, periodY, [pattern], isInnerOpaque,
                      shiftX, shiftY, scaling)
            { }

            #endregion
            #region methods

            // ...

            #endregion
        }

        #endregion
    }
}
