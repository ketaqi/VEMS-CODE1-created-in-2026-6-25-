using VEMS.MathCore;
using Complex = System.Numerics.Complex;


namespace VEMS.EMSolver
{

    /// <summary>
    /// pointwise mapper for x/y-separable case
    /// </summary>
    public class MapperXY
    {
        #region properties

        /// <summary>
        /// one-to-one mapping relation from input to output coordinate X
        /// <para> variable: input coordinate </para>
        /// <para> return: output coordinate </para>
        /// </summary>
        public Func<double, double> InOutX { get; set; }

        /// <summary>
        /// one-to-one mapping relation from input to output coordinate Y
        /// <para> variable: input coordinate </para>
        /// <para> return: output coordinate </para>
        /// </summary>
        public Func<double, double> InOutY { get; set; }

        /// <summary>
        /// one-to-one mapping relation from output to input coordinate X
        /// <para> variable: output coordinate </para>
        /// <para> return: input coordinate </para>
        /// </summary>
        public Func<double, double> OutInX { get; set; }

        /// <summary>
        /// one-to-one mapping relation from output to input coordinate Y
        /// <para> variable: output coordinate </para>
        /// <para> return: input coordinate </para>
        /// </summary>
        public Func<double, double> OutInY { get; set; }

        #endregion
        #region constructors

        /// <summary>
        /// default constructor with identity mapping
        /// </summary>
        internal MapperXY()
        {
            InOutX = (x) => x;
            InOutY = (y) => y;
            OutInX = (x) => x;
            OutInY = (y) => y;
            //H = (k) => 1.0;
        }

        /// <summary>
        /// constructs a mapper for 1D case with given 
        /// mapping fromt input to output coordinate
        /// </summary>
        /// <param name="inOutX"> one-to-one mapping from input to output coordinate </param>
        /// <param name="inOutY"> one-to-one mapping from input to output coordinate </param>
        /// <param name="outInX"> one-to-one mapping from output to input coordinate </param>
        /// <param name="outInY"> one-to-one mapping from output to input coordinate </param>
        public MapperXY(Func<double, double> inOutX, Func<double, double> inOutY,
            Func<double, double> outInX, Func<double, double> outInY)
        {
            InOutX = inOutX;
            InOutY = inOutY;
            OutInX = outInX;
            OutInY = outInY;
            //H = h;
        }

        #endregion
        #region methods

        /// <summary>
        /// applies the mapping to given data
        /// </summary>
        /// <param name="dIn"> input data </param>
        /// <param name="gOut"> target grid of the output data </param>
        /// <param name="loopMode"> computational option for loops </param>
        /// <returns> output data after applying the mapping </returns>
        internal Grid2DRealData Apply(Grid2DRealData dIn,
            GridInfo2D? gOut = null,
            LoopMode loopMode = Defaults.LoopOption)
        {
            throw new NotImplementedException("MapperXY.Apply for Grid2DRealData is not implemented yet.");
        }

        /// <summary>
        /// applies the mapping to given data
        /// </summary>
        /// <param name="dIn"> input data </param>
        /// <param name="gOut"> target grid of the output data </param>
        /// <param name="loopMode"> computational option for loops </param>
        /// <returns> output data after applying the mapping </returns>
        internal Grid2DCplxData Apply(Grid2DCplxData dIn,
            GridInfo2D? gOut = null,
            LoopMode loopMode = Defaults.LoopOption)
        {
            throw new NotFiniteNumberException("MapperXY.Apply for Grid2DCplxData is not implemented yet.");
        }

        #endregion
        #region derived-classes

        /// <summary>
        /// linear mapping for x/y-separable case
        /// </summary>
        public class Linear : MapperXY
        {
            #region properties

            /// <summary>
            /// Gets or sets the coefficient in the linear translation relation 
            /// in the input-to-output mapping along the x-direction.
            /// </summary>
            public double C1x{ get; set; }

            /// <summary>
            /// Gets or sets the coefficient in the linear translation relation 
            /// in the input-to-output mapping along the y-direction.
            /// </summary>
            public double C1y { get; set; }

            /// <summary>
            /// Gets or sets the constant offset in the input-to-output mapping
            /// along the x-direction.
            /// </summary>
            public double OffsetX { get; set; }

            /// <summary>
            /// Gets or sets the constant offset in the input-to-output mapping
            /// along the y-direction.
            /// </summary>
            public double OffsetY { get; set; }

            #endregion
            #region constructor

            /// <summary>
            /// Initializes a new instance of the <see cref="Linear"/> class with default mapping coefficients.
            /// </summary>
            /// <remarks>This constructor sets up the default linear mapping relations for both X and
            /// Y coordinates. The default coefficients are: <list type="bullet"> <item><description><c>C1x</c> and
            /// <c>C1y</c> are set to 1.0, representing a 1:1 scaling factor.</description></item>
            /// <item><description><c>OffsetX</c> and <c>OffsetY</c> are set to 0.0, representing no
            /// offset.</description></item> </list> The mapping relations are defined as follows: <list type="bullet">
            /// <item><description><c>InOutX</c>: Maps an input X value to an output X value using the formula <c>C1x *
            /// x + OffsetX</c>.</description></item> <item><description><c>InOutY</c>: Maps an input Y value to an
            /// output Y value using the formula <c>C1y * y + OffsetY</c>.</description></item>
            /// <item><description><c>OutInX</c>: Maps an output X value back to an input X value using the formula
            /// <c>(x - OffsetX) / C1x</c>.</description></item> <item><description><c>OutInY</c>: Maps an output Y
            /// value back to an input Y value using the formula <c>(y - OffsetY) / C1y</c>.</description></item>
            /// </list></remarks>
            internal Linear()
            {
                C1x = 1.0;
                C1y = 1.0;
                OffsetX = 0.0;
                OffsetY = 0.0;
                // defines the mapping relations
                InOutX = (x) => C1x * x + OffsetX;
                InOutY = (y) => C1y * y + OffsetY;
                OutInX = (x) => (x - OffsetX) / C1x;
                OutInY = (y) => (y - OffsetY) / C1y;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="Linear"/> class, which defines linear mapping
            /// transformations for input and output coordinates.
            /// </summary>
            /// <remarks>This constructor sets up the linear mapping relations for both forward and
            /// inverse transformations: <list type="bullet"> <item><description><c>InOutX</c>: Maps an input
            /// X-coordinate to an output X-coordinate using the formula <c>outputX = c1x * inputX +
            /// offsetX</c>.</description></item> <item><description><c>InOutY</c>: Maps an input Y-coordinate to an
            /// output Y-coordinate using the formula <c>outputY = c1y * inputY + offsetY</c>.</description></item>
            /// <item><description><c>OutInX</c>: Maps an output X-coordinate back to an input X-coordinate using the
            /// formula <c>inputX = (outputX - offsetX) / c1x</c>.</description></item>
            /// <item><description><c>OutInY</c>: Maps an output Y-coordinate back to an input Y-coordinate using the
            /// formula <c>inputY = (outputY - offsetY) / c1y</c>.</description></item> </list></remarks>
            /// <param name="c1x">The scaling factor for the X-axis transformation.</param>
            /// <param name="c1y">The scaling factor for the Y-axis transformation.</param>
            /// <param name="offsetX">The offset to apply to the X-axis transformation. Defaults to 0.0.</param>
            /// <param name="offsetY">The offset to apply to the Y-axis transformation. Defaults to 0.0.</param>
            public Linear(double c1x, double c1y,
                double offsetX = 0.0, double offsetY = 0.0)
            {
                C1x = c1x;
                C1y = c1y;
                OffsetX = offsetX;
                OffsetY = offsetY;
                // defines the mapping relations
                InOutX = (x) => C1x * x + OffsetX;
                InOutY = (y) => C1y * y + OffsetY;
                OutInX = (x) => (x - OffsetX) / C1x;
                OutInY = (y) => (y - OffsetY) / C1y;
            }

            #endregion
            #region methods

            /// <summary>
            /// Applies the specific linear mapping to the given real-valued grid data.
            /// Updates the grid information, reverses values if necessary, and scales the data.
            /// </summary>
            /// <param name="d">
            /// The <see cref="Grid2DRealData"/> instance to be modified by the mapping.
            /// The grid information and values will be updated in place.
            /// </param>
            /// <param name="scal">
            /// Optional scaling factor to apply to the data values after mapping.
            /// If <c>null</c>, a default scaling of <c>1.0 / |C1y * C1y|</c> is used.
            /// </param>
            public void Apply(ref Grid2DRealData d,
                double? scal = 1.0)
            {
                // defines output grid according to linear mapping
                GridInfo2D g = d.GridInfo;
                long rows = g.Rows; long cols = g.Cols;
                double spx = g.SpacingX * Math.Abs(C1x);
                double spy = g.SpacingY * Math.Abs(C1y);
                double stx = (C1x >= 0.0) ?
                    g.StartX * C1x + OffsetX : g.EndX * C1x + OffsetX;
                double sty = (C1y >= 0.0) ?
                    g.StartY * C1y + OffsetY : g.EndY * C1y + OffsetY;
                // updates grid
                d.GridInfo = new GridInfo2D(rows: rows, cols: cols,
                    spacingY: spy, spacingX: spx,
                    refPointY: sty, refTypeY: GridRefType.Start,
                    refPointX: stx, refTypeX: GridRefType.Start);
                // reverse values?
                if (C1x < 0.0 && C1y < 0.0) 
                { d.Values.Reverse(); } // reverse all values
                else if (C1x < 0.0) // reverse x-values only
                { d.Values.ReverseCols(); }
                else if (C1y < 0.0) // reverse y-values only
                { d.Values.ReverseRows(); }
                // scales values if needed
                scal ??= 1.0 / Math.Abs(C1y * C1y);
                if (scal != 1.0)
                {
                    MatrixD t = d.Values;
                    //VMath.ScaleOnD(x: ref t, a: scal.Value);
                    VMath.ScaleOn(x: ref t, a: scal.Value);
                }
            }

            /// <summary>
            /// Applies the specific linear mapping to the given complex-valued grid data.
            /// Updates the grid information, reverses values if necessary, scales the data, and updates the phase.
            /// </summary>
            /// <param name="d">
            /// The <see cref="Grid2DCplxData"/> instance to be modified by the mapping.
            /// The grid information, values, and phase will be updated in place.
            /// </param>
            /// <param name="scal">
            /// Optional scaling factor to apply to the data values after mapping.
            /// If <c>null</c>, a default scaling of <c>1.0 / |C1y * C1y|</c> is used.
            /// </param>
            public void Apply(ref Grid2DCplxData d,
                double? scal = 1.0)
            {
                // defines output grid according to linear mapping
                GridInfo2D g = d.GridInfo;
                long rows = g.Rows; long cols = g.Cols;
                double spx = g.SpacingX * Math.Abs(C1x);
                double spy = g.SpacingY * Math.Abs(C1y);
                double stx = (C1x >= 0.0) ?
                    g.StartX * C1x + OffsetX : g.EndX * C1x + OffsetX;
                double sty = (C1y >= 0.0) ?
                    g.StartY * C1y + OffsetY : g.EndY * C1y + OffsetY;
                // updates grid
                d.GridInfo = new GridInfo2D(rows: rows, cols: cols,
                    spacingY: spy, spacingX: spx,
                    refPointY: sty, refTypeY: GridRefType.Start,
                    refPointX: stx, refTypeX: GridRefType.Start);
                // reverse values?
                if (C1x < 0.0 && C1y < 0.0) 
                { d.Values.Reverse(); } // reverse all values
                else if (C1x < 0.0) // reverse x-values only
                { d.Values.ReverseCols(); }
                else if (C1y < 0.0) // reverse y-values only
                { d.Values.ReverseRows(); }
                // scales values if needed
                scal ??= 1.0 / Math.Abs(C1y * C1y);
                if (scal != 1.0)
                {
                    MatrixZ t = d.Values;
                    //VMath.ScaleOnZ(x: ref t, a: scal.Value);
                    VMath.ScaleOn(x: ref t, a: scal.Value);
                }
                // updates phase
                d.Phase.C1x /= C1x;
                d.Phase.C1y /= C1y;
            }

            #endregion
        }

        #endregion
    }



    /// <summary>
    /// 
    /// </summary>
    public class IdealizedImagingXY
    {
        #region properties

        /// <summary>
        /// Gets or sets the linear translation mapping applied to the field.
        /// </summary>
        /// <remarks>
        /// The <see cref="Translation"/> property defines the linear mapping of spatial coordinates
        /// for the idealized imaging system. This mapping determines how input coordinates are
        /// transformed to output coordinates, including scaling (magnification) and translation (offsets)
        /// along the X and Y axes. The mapping is represented by a <see cref="MapperXY.Linear"/> instance,
        /// which provides both forward and inverse coordinate transformations.
        /// </remarks>
        public MapperXY.Linear Translation { get; set; }

        /// <summary>
        /// Gets or sets the modulation (amplitude and phase) applied by the idealized imaging system.
        /// </summary>
        /// <remarks>
        /// The <see cref="Modulation"/> property defines the transmission function that modulates the field
        /// in the frequency domain. This modulation can include both amplitude and phase modifications as a function
        /// of spatial frequencies (kx, ky). The property is represented by a <see cref="Transmission2D"/> instance,
        /// which provides customizable amplitude and phase functions for the imaging system.
        /// </remarks>
        public Transmission2D Modulation { get; set; }

        #endregion
        #region constructors


        internal IdealizedImagingXY()
        {
            // default translation is identity mapping
            Translation = new MapperXY.Linear();
            // default modulation is identity transmission
            Modulation = new Transmission2D();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IdealizedImagingXY"/> class, which models an idealized imaging
        /// system with configurable magnification, offsets, and optional modulation functions for amplitude and phase.
        /// </summary>
        /// <remarks>The <see cref="IdealizedImagingXY"/> class provides a linear mapping of spatial
        /// coordinates with optional modulation of amplitude and phase in the frequency domain. The <paramref
        /// name="mx"/> and <paramref name="my"/> parameters determine the scaling factors for the X and Y axes,
        /// respectively, while <paramref name="offsetX"/> and <paramref name="offsetY"/> specify translation offsets.
        /// The optional <paramref name="amplitude"/> and <paramref name="phase"/> functions allow customization of the
        /// modulation behavior.</remarks>
        /// <param name="mx">The magnification factor along the X-axis. Must be non-zero.</param>
        /// <param name="my">The magnification factor along the Y-axis. Must be non-zero.</param>
        /// <param name="offsetX">The offset along the X-axis, in units of the output coordinate system. Defaults to 0.0.</param>
        /// <param name="offsetY">The offset along the Y-axis, in units of the output coordinate system. Defaults to 0.0.</param>
        /// <param name="amplitude">An optional function that defines the amplitude modulation as a function of spatial frequencies.
        /// If not provided, a constant amplitude of 1.0 is used.</param>
        /// <param name="phase">An optional function that defines the phase modulation as a function of spatial frequencies.
        /// If not provided, a constant phase of 0.0 is used.</param>
        public IdealizedImagingXY(double mx, double my, 
            double offsetX = 0.0, double offsetY = 0.0,
            Func<double, double, double>? amplitude = null,
            Func<double, double, double>? phase = null)
        {
            double c1x = 1.0 / mx;
            double c1y = 1.0 / my;
            Translation = new MapperXY.Linear(c1x, c1y, offsetX, offsetY);
            Modulation = new()
            {
                Amplitude = amplitude ?? ((kx, ky) => 1.0),
                Phase = phase ?? ((kx, ky) => 0.0),
            };
        }

        #endregion
        #region methods

        /// <summary>
        /// Applies the configured linear mapping and modulation to a scalar field in the k-domain.
        /// Updates the field's grid, reverses and scales values as needed, applies modulation, and updates the phase.
        /// </summary>
        /// <typeparam name="T">A type derived from <see cref="SCField"/> representing the scalar field to be modulated.</typeparam>
        /// <param name="v">
        /// Reference to the scalar field to be modulated. The method updates the field's grid, values, and phase in place.
        /// </param>
        /// <param name="scal">
        /// Optional scaling factor to apply to the field values after mapping and modulation.
        /// If <c>null</c>, a default scaling of <c>1.0 / |Translation.C1y * Translation.C1y|</c> is used.
        /// </param>
        /// <param name="loopMode">
        /// Computational option for loops, controlling parallelization or vectorization behavior.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="v"/> has <c>UValues</c>, <c>UGrid</c>, or <c>UPhase</c> set to <c>null</c>.
        /// </exception>
        public void ApplyModulateOn<T>(ref T v,
            double? scal = 1.0,
            LoopMode loopMode = Defaults.LoopOption)
            where T : SCField
        {
            if (v.UValues == null) { throw new ArgumentNullException(nameof(v.UValues), "UValues cannot be null."); }
            if (v.UGrid == null) { throw new ArgumentNullException(nameof(v.UGrid), "UGrid cannot be null."); }
            if (v.UPhase == null) { throw new ArgumentNullException(nameof(v.UPhase), "UPhase cannot be null."); }

            // makes sure the field is in k-domain
            v.SwitchToKDomain();

            // defines output grid according to linear mapping
            GridInfo2D g = v.UGrid;
            long rows = g.Rows; long cols = g.Cols;
            double spx = g.SpacingX * Math.Abs(Translation.C1x);
            double spy = g.SpacingY * Math.Abs(Translation.C1y);
            double stx = (Translation.C1x >= 0.0) ?
                g.StartX * Translation.C1x + Translation.OffsetX
                : g.EndX * Translation.C1x + Translation.OffsetX;
            double sty = (Translation.C1y >= 0.0) ?
                g.StartY * Translation.C1y + Translation.OffsetY
                : g.EndY * Translation.C1y + Translation.OffsetY;
            // updates grid
            v.UGrid = new GridInfo2D(rows: rows, cols: cols,
                spacingY: spy, spacingX: spx,
                refPointY: sty, refTypeY: GridRefType.Start,
                refPointX: stx, refTypeX: GridRefType.Start);
            // reverse values?
            if (Translation.C1x < 0.0 && Translation.C1y < 0.0)
            { v.UValues.Reverse(); } // reverse all values
            else if (Translation.C1x < 0.0) // reverse x-values only
            { v.UValues.ReverseCols(); }
            else if (Translation.C1y < 0.0) // reverse y-values only
            { v.UValues.ReverseRows(); }
            // scales values if needed
            scal ??= 1.0 / Math.Abs(Translation.C1y * Translation.C1y);
            MatrixZ t = v.UValues;
            Complex f(double kx, double ky) => Modulation.F(kx, ky) * scal.Value;
            new Samp2DCplxFunc(f: f).ScaleOn(x: ref t, grid: v.UGrid, loopMode: loopMode);
            // updates phase
            v.UPhase.C1x /= Translation.C1x;
            v.UPhase.C1y /= Translation.C1y;
        }


        #endregion
    }


}
