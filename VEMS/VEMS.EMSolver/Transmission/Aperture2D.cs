using VEMS.MathCore;

namespace VEMS.EMSolver
{

    /// <summary>
    /// two-dimensional aperture
    /// </summary>
    public class Aperture2D : Transmission2D, IOpticalComponent
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
        /// diameter of the aperture along x direction
        /// </summary>
        public double DiameterX { get; set; }

        /// <summary>
        /// diameter of the aperture along y direction
        /// </summary>
        public double DiameterY { get; set; }

        #endregion
        #region constructor

        /// <summary>
        /// default constructor
        /// </summary>
        public Aperture2D() { }

        /// <summary>
        /// constructs a 2D aperture
        /// </summary>
        /// <param name="diameterX"> diameter of the aperture along x direction </param>
        /// <param name="diameterY"> diameter of the aperture along y direction </param>
        /// <param name="shiftX"> lateral shift of the field along x direction </param>
        /// <param name="shiftY"> lateral shift of the field along y direction </param>
        /// <param name="scaling"> overall scaling of the aperture transmission </param>
        public Aperture2D(double diameterX, double diameterY,
            double shiftX = 0.0, double shiftY = 0.0,
            double scaling = 1.0)
            : base(shiftX, shiftY, scaling)
        {
            DiameterX = diameterX;
            DiameterY = diameterY;
        }

        /// <summary>
        /// constructs a 2D aperture
        /// </summary>
        /// <param name="diameterX"> diameter of the aperture along x direction </param>
        /// <param name="diameterY"> diameter of the aperture along y direction </param>
        /// <param name="shiftX"> lateral shift of the field along x direction </param>
        /// <param name="shiftY"> lateral shift of the field along y direction </param>
        /// <param name="scaling"> overall scaling of the aperture transmission </param>
        /// <param name="label"></param>
        /// <param name="coordinate"> coordinate system of the aperture </param>
        /// <param name="loopMode"></param>
        public Aperture2D(double diameterX, double diameterY,
            double shiftX = 0.0, double shiftY = 0.0,
            double scaling = 1.0, 
            string? label = null,
            CoordinateSystem? coordinate = null,
            LoopMode loopMode = Defaults.LoopOption)
            : base(shiftX, shiftY, scaling)
        {
            DiameterX = diameterX;
            DiameterY = diameterY;
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
        /// 2D rectangular aperture with smoothed edges
        /// </summary>
        public class Rectangular : Aperture2D
        {
            #region properties

            /// <summary>
            /// absolute edge width of the aperture
            /// </summary>
            public double EdgeWidth { get; set; }

            #endregion
            #region constructor

            /// <summary>
            /// constructs an aperture with rectangular shape
            /// and possibly smoothed edges
            /// </summary>
            /// <param name="diameterX"> diameter of the aperture along x direction </param>
            /// <param name="diameterY"> diameter of the aperture along y direction </param>
            /// <param name="edgeWidth"> absolute edge width (half within, half outside) </param>
            /// <param name="shiftX"> lateral shift of the field along x direction </param>
            /// <param name="shiftY"> lateral shift of the field along y direction </param>
            /// <param name="scaling"> overall scaling of the aperture transmission </param>
            public Rectangular(double diameterX, double diameterY,
                double edgeWidth = 0.0,
                double shiftX = 0.0, double shiftY = 0.0,
                double scaling = 1.0)
                : base(diameterX, diameterY, shiftX, shiftY, scaling)
            {
                EdgeWidth = edgeWidth;
                // defines rectangular aperture function
                Amplitude = (x, y) => Scaling * Function1D.CosEdgeRectangle(x, new() { DiameterX, EdgeWidth })
                    * Function1D.CosEdgeRectangle(y, new() { DiameterY, EdgeWidth }); 
            }

            /// <summary>
            /// constructs an aperture with rectangular shape
            /// and possibly smoothed edges
            /// </summary>
            /// <param name="diameterX"> diameter of the aperture along x direction </param>
            /// <param name="diameterY"> diameter of the aperture along y direction </param>
            /// <param name="edgeWidth"> absolute edge width (half within, half outside) </param>
            /// <param name="shiftX"> lateral shift of the field along x direction </param>
            /// <param name="shiftY"> lateral shift of the field along y direction </param>
            /// <param name="scaling"> overall scaling of the aperture transmission </param>
            /// <param name="label"></param>
            /// <param name="coordinate"></param>
            /// <param name="loopMode"></param>
            public Rectangular(double diameterX, double diameterY,
                double edgeWidth = 0.0,
                double shiftX = 0.0, double shiftY = 0.0,
                double scaling = 1.0,
                string? label = null,
                CoordinateSystem? coordinate = null,
                LoopMode loopMode = Defaults.LoopOption)
                : base(diameterX, diameterY, shiftX, shiftY, scaling, 
                      label, coordinate, loopMode)
            {
                EdgeWidth = edgeWidth;
                // defines rectangular aperture function
                Amplitude = (x, y) => Scaling 
                    * Function1D.CosEdgeRectangle(x, [DiameterX, EdgeWidth])
                    * Function1D.CosEdgeRectangle(y, [DiameterY, EdgeWidth]);
            }

            #endregion
        }

        /// <summary>
        /// 2D elliptical aperture with smoothed edges
        /// </summary>
        public class Ellptical : Aperture2D
        {
            #region properties

            /// <summary>
            /// semi axis of the ellipse along x direction
            /// </summary>
            private double SemiAxisX => 0.5 * DiameterX;

            /// <summary>
            /// semi axis of the ellipse along y direction
            /// </summary>
            private double SemiAxisY => 0.5 * DiameterY;

            /// <summary>
            /// absolute edge width of the aperture
            /// </summary>
            public double EdgeWidth { get; set; }

            #endregion
            #region constructor

            /// <summary>
            /// constructs an aperture with elliptical shape
            /// and possibly smoothed edges
            /// </summary>
            /// <param name="diameterX"> diameter of the aperture along x direction </param>
            /// <param name="diameterY"> diameter of the aperture along y direction </param>
            /// <param name="edgeWidth"> absolute edge width (half within, half outside) </param>
            /// <param name="shiftX"> lateral shift of the field along x direction </param>
            /// <param name="shiftY"> lateral shift of the field along y direction </param>
            /// <param name="scaling"> overall scaling of the aperture transmission </param>
            public Ellptical(double diameterX, double diameterY,
                double edgeWidth = 0.0,
                double shiftX = 0.0, double shiftY = 0.0,
                double scaling = 1.0)
                : base(diameterX, diameterY, shiftX, shiftY, scaling)
            {
                EdgeWidth = edgeWidth;
                // defines elliptical aperture function
                Amplitude = (x, y) => Scaling * Function2D.
                CosEdgeEllipse(x, y, [SemiAxisX, SemiAxisY, EdgeWidth]);
            }

            /// <summary>
            /// constructs an aperture with elliptical shape
            /// and possibly smoothed edges
            /// </summary>
            /// <param name="diameterX"> diameter of the aperture along x direction </param>
            /// <param name="diameterY"> diameter of the aperture along y direction </param>
            /// <param name="edgeWidth"> absolute edge width (half within, half outside) </param>
            /// <param name="shiftX"> lateral shift of the field along x direction </param>
            /// <param name="shiftY"> lateral shift of the field along y direction </param>
            /// <param name="scaling"> overall scaling of the aperture transmission </param>
            /// <param name="label"></param>
            /// <param name="coordinate"></param>
            /// <param name="loopMode"></param>
            public Ellptical(double diameterX, double diameterY,
                double edgeWidth = 0.0,
                double shiftX = 0.0, double shiftY = 0.0,
                double scaling = 1.0,
                string? label = null,
                CoordinateSystem? coordinate = null,
                LoopMode loopMode = Defaults.LoopOption)
                : base(diameterX, diameterY, shiftX, shiftY, scaling,
                      label, coordinate, loopMode)
            {
                EdgeWidth = edgeWidth;
                // defines elliptical aperture function
                Amplitude = (x, y) => Scaling * Function2D.
                CosEdgeEllipse(x, y, [SemiAxisX, SemiAxisY, EdgeWidth]);
            }

            #endregion
        }

        /// <summary>
        /// 2D Gaussian aperture with smoothed edges
        /// </summary>
        public class Gaussian : Aperture2D
        {
            #region properties

            /// <summary>
            /// waist radius of the Gaussian profile along x-direction
            /// </summary>
            private double WaistRadiusX => 0.5 * DiameterX;

            /// <summary>
            /// waist radius of the Gaussian profile along y-direction
            /// </summary>
            private double WaistRadiusY => 0.5 * DiameterY;

            #endregion
            #region constructor

            /// <summary>
            /// constructs a 2D aperture with Gaussian profile
            /// </summary>
            /// <param name="diameterX"> diameter of the aperture along x direction </param>
            /// <param name="diameterY"> diameter of the aperture along y direction </param>
            /// <param name="shiftX"> lateral shift of the field along x direction </param>
            /// <param name="shiftY"> lateral shift of the field along y direction </param>
            /// <param name="scaling"> overall scaling of the aperture transmission </param>
            public Gaussian(double diameterX, double diameterY,
                double shiftX = 0.0, double shiftY = 0.0,
                double scaling = 1.0)
                : base(diameterX, diameterY, shiftX, shiftY, scaling)
            {
                // defines Gaussian aperture function
                Amplitude = (x, y) => Scaling * Function1D.Gaussian(x, [WaistRadiusX])
                    * Function1D.Gaussian(y, [WaistRadiusY]);
            }

            /// <summary>
            /// constructs a 2D aperture with Gaussian profile
            /// </summary>
            /// <param name="diameterX"> diameter of the aperture along x direction </param>
            /// <param name="diameterY"> diameter of the aperture along y direction </param>
            /// <param name="shiftX"> lateral shift of the field along x direction </param>
            /// <param name="shiftY"> lateral shift of the field along y direction </param>
            /// <param name="scaling"> overall scaling of the aperture transmission </param>
            /// <param name="label"></param>
            /// <param name="coordinate"></param>
            /// <param name="loopMode"></param>
            public Gaussian(double diameterX, double diameterY,
                double shiftX = 0.0, double shiftY = 0.0,
                double scaling = 1.0,
                string? label = null,
                CoordinateSystem? coordinate = null,
                LoopMode loopMode = Defaults.LoopOption)
                : base(diameterX, diameterY, shiftX, shiftY, scaling,
                      label, coordinate, loopMode)
            {
                // defines Gaussian aperture function
                Amplitude = (x, y) => Scaling * Function1D.Gaussian(x, [WaistRadiusX])
                    * Function1D.Gaussian(y, [WaistRadiusY]);
            }

            #endregion
        }


        // polygon ...

        #endregion
    }

}
