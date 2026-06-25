using VEMS.MathCore;

namespace VEMS.EMSolver
{

    /// <summary>
    /// Represents a two-dimensional (2D) field detector with a uniform sampling grid.
    /// </summary>
    /// <remarks>
    /// Constructs a detector with a uniform sampling grid for field detection and analysis.
    /// </remarks>
    /// <param name="grid">Uniform sampling grid of the detector.</param>
    public class Detector2D(GridInfo2D grid) : IDetector, IOpticalComponent
    {
        #region ---- IDetector ----

        /// <summary>
        /// Gets or sets the field quantity to detect.
        /// </summary>
        public DetectQuantity Quantity { get; set; }
            = DetectQuantity.SquaredMagnitude;

        /// <summary>
        /// Gets or sets the pixelation mode of the detector.
        /// </summary>
        public PixelationMode PixelMode { get; set; } 
            = PixelationMode.LinearFit;

        #endregion
        #region ---- IOpticalComponent ----

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
        /// This function defines how the component detects or measures an <see cref="SCField"/> and returns the resulting <see cref="MatrixD"/>.
        /// </summary>
        public Func<SCField, Grid2DRealData>? Detect { get; set; } = null;

        /// <summary>
        /// Gets or sets the coordinate system associated with the optical component.
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
        /// Gets or sets the uniform sampling grid information for the detector.
        /// </summary>
        public GridInfo2D GridInfo { get; set; } = grid;

        // polarization ...

        #endregion
        #region constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Detector2D"/> class with the specified grid, coordinate system, and loop mode.
        /// </summary>
        /// <param name="grid">The uniform sampling grid of the detector.</param>
        /// <param name="label"></param>
        /// <param name="coordinate">The coordinate system associated with the detector. If <c>null</c>, <see cref="CoordinateSystem.Origin"/> is used.</param>
        /// <param name="loopMode">The loop-computational mode options for detection operations.</param>
        public Detector2D(GridInfo2D grid,
            string? label = null,
            CoordinateSystem? coordinate = null,
            LoopMode loopMode = Defaults.LoopOption)
            : this(grid)
        {
            // optical component
            Label = label ?? GetType().FullName;
            Coordinate = coordinate ?? CoordinateSystem.Origin;
            //Process = (v) => v; // no processing
            Detect = (v) =>
            {
                MatrixD t = Sample(v, includeScaling: true,
                    quantity: Quantity, pixelMode: PixelMode, loopMode: loopMode);
                return new Grid2DRealData(values: t, gridInfo: GridInfo);
            };
            OutputCoordinate = Coordinate;
        }

        #endregion
        #region methods

        /// <summary>
        /// Samples the field on the detector's grid.
        /// </summary>
        /// <param name="v">Field to detect on this detector.</param>
        /// <param name="quantity">Field quantity to detect.</param>
        /// <param name="pixelMode">Pixelation mode option.</param>
        /// <param name="loopMode">Loop-computational mode options.</param>
        /// <returns>Sampled field quantity on the detector's grid.</returns>
        [Obsolete]
        public MatrixD Sample(ScalarField v,
            DetectQuantity quantity = DetectQuantity.Magnitude,
            PixelationMode pixelMode = PixelationMode.LinearFit,
            LoopMode loopMode = Defaults.LoopOption)
        {
            if (quantity == DetectQuantity.PowerDensity) { throw new NotImplementedException(nameof(quantity)); }
            if (pixelMode == PixelationMode.Integral) { throw new NotImplementedException(nameof(pixelMode)); }
            // null case handling
            if (v.Field == null) { throw new ArgumentNullException(nameof(v.Field)); }

            // takes out corresponding field quantity
            MatrixD q = quantity switch
            {
                DetectQuantity.RealPart => VMath.RealPart(v.Field),
                DetectQuantity.ImagPart => VMath.ImagPart(v.Field),
                DetectQuantity.Magnitude => VMath.Abs(v.Field),
                DetectQuantity.Argument => VMath.Arg(v.Field),
                DetectQuantity.SquaredMagnitude => VMath.Square(VMath.Abs(v.Field)),
                _ => VMath.Abs(v.Field)
            };

            // checks if the grids are equal
            if (v.Grid == GridInfo)
            {
                if (quantity == DetectQuantity.SquaredMagnitude)
                { q = VMath.Square(q); }
                return q;
            }

            // interpolates onto detector's sampling grid
            InterpolationMethod method = pixelMode == PixelationMode.LinearFit ?
                InterpolationMethod.Linear : InterpolationMethod.Nearest;
            Grid2DRealInterpolation itp = new(v: q, grid: v.Grid, method: method);
            MatrixD d = itp.Evaluate(targetGrid: GridInfo, loopMode: loopMode);

            // additional handling for squared magnitude
            if (quantity == DetectQuantity.SquaredMagnitude)
            { d = VMath.Square(d); }

            // return
            return d;
        }

        /// <summary>
        /// Takes out the field quantity and (possibly) resamples it on a target grid.
        /// </summary>
        /// <param name="v">Field to detect on the detector.</param>
        /// <param name="includeScaling">Whether to include the scaling for the detector.</param>
        /// <param name="quantity">Field quantity to detect.</param>
        /// <param name="targetGrid">Target sampling grid.</param>
        /// <param name="pixelMode">Pixelation mode option.</param>
        /// <param name="loopMode">Loop-computational mode options.</param>
        /// <returns>Selected (possibly resampled) field quantity.</returns>
        private static MatrixD TakeResample(SCField v,
            bool includeScaling,
            DetectQuantity quantity,
            GridInfo2D targetGrid,
            PixelationMode pixelMode,
            LoopMode loopMode)
        {
            if (v.UValues == null) { throw new ArgumentNullException(nameof(v.UValues)); }
            if (v.UGrid == null) { throw new ArgumentNullException(nameof(v.UGrid)); }

            // scaling?
            MatrixZ vals = v.UValues;
            if (includeScaling && v.Scaling != 1.0)
            { vals = VMath.Scale(x: vals, a: v.Scaling); }
            // takes out corresponding field quantity
            MatrixD q;
            switch (quantity)
            {
                case DetectQuantity.RealPart:
                    q = VMath.RealPart(vals);
                    break;
                case DetectQuantity.ImagPart:
                    q = VMath.ImagPart(vals);
                    break;
                case DetectQuantity.Magnitude:
                    q = VMath.Abs(vals);
                    break;
                case DetectQuantity.Argument:
                    q = VMath.Arg(vals);
                    break;
                case DetectQuantity.SquaredMagnitude: goto case DetectQuantity.Magnitude;
                default: goto case DetectQuantity.Magnitude;
            }
            ;

            // needs resampling?
            if (v.UGrid == targetGrid)
            {
                // no need for resampling
                if (quantity == DetectQuantity.SquaredMagnitude)
                { q = VMath.Square(q); }
                return q;
            }
            else
            {
                // resamples the field quantity according to the target grid
                InterpolationMethod intrpl;
                switch (pixelMode)
                {
                    case PixelationMode.Original:
                        intrpl = v.U.IntrplMethod;
                        break;
                    case PixelationMode.LinearFit:
                        intrpl = InterpolationMethod.Linear;
                        break;
                    case PixelationMode.Nearest:
                        intrpl = InterpolationMethod.Nearest;
                        break;
                    default: goto case PixelationMode.Original;
                }
                Grid2DRealInterpolation itp = new(v: q, grid: v.UGrid, method: intrpl);
                MatrixD d = itp.Evaluate(targetGrid: targetGrid, loopMode: loopMode);
                // additional handling for squared magnitude
                if (quantity == DetectQuantity.SquaredMagnitude)
                { d = VMath.Square(d); }
                return d;
            }
        }

        /// <summary>
        /// Samples the field on the detector's grid.
        /// </summary>
        /// <param name="v">Field to detect on this detector.</param>
        /// <param name="includeScaling">Whether to include the scaling for the detector.</param>
        /// <param name="quantity">Field quantity to detect.</param>
        /// <param name="pixelMode">Pixelation mode option.</param>
        /// <param name="loopMode">Loop-computational mode options.</param>
        /// <returns>Sampled field quantity on the detector's grid.</returns>
        /// <exception cref="NotImplementedException">Thrown if the quantity or pixelation mode is not implemented.</exception>
        /// <exception cref="ArgumentNullException">Thrown if the field values are null.</exception>
        public MatrixD Sample(SCField v,
            bool includeScaling = true,
            DetectQuantity quantity = DetectQuantity.SquaredMagnitude,
            PixelationMode pixelMode = PixelationMode.LinearFit,
            LoopMode loopMode = Defaults.LoopOption)
        {
            if (quantity == DetectQuantity.PowerDensity) { throw new NotImplementedException(nameof(quantity)); }
            if (pixelMode == PixelationMode.Integral) { throw new NotImplementedException(nameof(pixelMode)); }
            // null case handling
            if (v.UValues == null) { throw new ArgumentNullException(nameof(v.UValues)); }

            // takes out corresponding field quantity
            if (v.Domain == ModelingDomain.SpatialFrequency) { v.SwitchToXDomain(); }
            return TakeResample(v, includeScaling, quantity, GridInfo, pixelMode, loopMode);
        }

        /// <summary>
        /// Samples a list of fields on the detector's grid and sums the results.
        /// </summary>
        /// <param name="v">List of fields to detect on the detector.</param>
        /// <param name="includeScaling">Whether to include the scaling for the detector.</param>
        /// <param name="quantity">Field quantity to detect.</param>
        /// <param name="pixelMode">Pixelation mode option.</param>
        /// <param name="loopMode">Loop-computational mode options.</param>
        /// <returns>Sampled and summed result on the detector's grid.</returns>
        /// <exception cref="NotImplementedException">Thrown if the quantity or pixelation mode is not implemented.</exception>
        /// <exception cref="ArgumentNullException">Thrown if the field list or field values are null.</exception>
        public MatrixD Sample(Grid2DSCFields v,
            bool includeScaling = true,
            DetectQuantity quantity = DetectQuantity.SquaredMagnitude,
            PixelationMode pixelMode = PixelationMode.LinearFit,
            LoopMode loopMode = Defaults.LoopOption)
        {
            if (quantity == DetectQuantity.PowerDensity) { throw new NotImplementedException(nameof(quantity)); }
            if (pixelMode == PixelationMode.Integral) { throw new NotImplementedException(nameof(pixelMode)); }
            // null case handling
            if (v.Fields == null) { throw new ArgumentNullException(nameof(v.Fields)); }

            // loop over all fields
            MatrixD d = new(rows: GridInfo.Rows, cols: GridInfo.Cols);
            for (long iRow = 0; iRow < v.GridPoints.Rows; iRow++)
            {
                for (long iCol = 0; iCol < v.GridPoints.Cols; iCol++)
                {
                    SCField vi = v.Fields[iRow, iCol];
                    if (vi.UValues == null) { throw new ArgumentNullException(nameof(vi.UValues) + $" @row [{iRow}] & col[{iCol}]"); }
                    MatrixD di = TakeResample(vi, includeScaling, quantity, GridInfo, pixelMode, loopMode);
                    VMath.AddTo(x: di, y: ref d);
                }
            }
            return d;
        }

        #endregion

        #region derived

        /// <summary>
        /// Represents a beam parameter detector for analyzing beam properties on a uniform sampling grid.
        /// </summary>
        /// <remarks>
        /// Constructs a beam parameter detector with a uniform sampling grid.
        /// </remarks>
        /// <param name="grid">Uniform sampling grid of the detector.</param>
        [Obsolete]
        public class BeamParameters(GridInfo2D grid) : Detector2D(grid)
        {
            #region properties

            /// <summary>
            /// Gets or sets the (paraxial) intensity distribution of the beam.
            /// </summary>
            internal MatrixD? Intensity { get; set; } = null;

            #endregion
            #region constructor

            // ...

            #endregion
            #region methods

            /// <summary>
            /// Finds the centroid of the beam using first-order momentum along x- and y-direction respectively.
            /// </summary>
            /// <param name="v">Field to detect on this detector.</param>
            /// <param name="pixelMode">Pixelation mode option.</param>
            /// <param name="loopMode">Loop-computational mode options.</param>
            /// <returns>Tuple containing centroid X and centroid Y.</returns>
            public (double, double) FindCentroid(SCField v,
                PixelationMode pixelMode = PixelationMode.LinearFit,
                LoopMode loopMode = Defaults.LoopOption)
            {
                // prepares the (paraxial) intensity distribution
                Intensity ??= Sample(v: v, includeScaling: false,
                    quantity: DetectQuantity.SquaredMagnitude,
                    pixelMode: pixelMode,
                    loopMode: loopMode);

                // first-order momentum
                double xISum = 0.0;
                double yISum = 0.0;
                double iSum = 0.0;
                Action<long, long> a = (iRow, iCol) =>
                {
                    double t = Intensity[iRow, iCol, checkBound: false];
                    iSum += t;
                    xISum += t * GridInfo.GetCoordinateX(iCol);
                    yISum += t * GridInfo.GetCoordinateY(iRow);
                };
                Loop2D loop = new(operation: a,
                    rowStart: 0, rowEnd: GridInfo.Rows,
                    colStart: 0, colEnd: GridInfo.Cols);
                loop.Evaluate(mode: loopMode);

                // centroid calculation
                return (xISum / iSum, yISum / iSum);
            }

            /// <summary>
            /// Finds the 1/e^2 radius using second-order momentum along x- and y-direction respectively.
            /// </summary>
            /// <param name="v">Field to detect on this detector.</param>
            /// <param name="refCenterX">Reference beam center along x-direction; if null, will be calculated automatically.</param>
            /// <param name="refCenterY">Reference beam center along y-direction; if null, will be calculated automatically.</param>
            /// <param name="pixelMode">Pixelation mode option.</param>
            /// <param name="loopMode">Loop-computational mode options.</param>
            /// <returns>Tuple containing radius X and radius Y.</returns>
            public (double, double) FindRadius(SCField v,
                double? refCenterX = null, double? refCenterY = null,
                PixelationMode pixelMode = PixelationMode.LinearFit,
                LoopMode loopMode = Defaults.LoopOption)
            {
                // centroid?
                if (refCenterX == null || refCenterY == null)
                {
                    (double x0, double y0) = FindCentroid(v, pixelMode, loopMode);
                    if (refCenterX == null)
                        refCenterX = x0;
                    Printer.Write($"Reference beam center calculated at {refCenterX.Value} along x-direction");
                    if (refCenterY == null)
                        refCenterY = y0;
                    Printer.Write($"Reference beam center calculated at {refCenterY.Value} along y-direction");
                }

                // prepares the (paraxial) intensity distribution
                Intensity ??= Sample(v: v, includeScaling: false,
                    quantity: DetectQuantity.SquaredMagnitude,
                    pixelMode: pixelMode,
                    loopMode: loopMode);

                // second-order momentum
                double x2ISum = 0.0;
                double y2ISum = 0.0;
                double iSum = 0.0;
                Action<long, long> a = (iRow, iCol) =>
                {
                    double t = Intensity[iRow, iCol, checkBound: false];
                    iSum += t;
                    double xi = GridInfo.GetCoordinateX(iCol) - refCenterX.Value;
                    x2ISum += t * xi * xi;
                    double yi = GridInfo.GetCoordinateY(iRow) - refCenterY.Value;
                    y2ISum += t * yi * yi;
                };
                Loop2D loop = new(operation: a,
                    rowStart: 0, rowEnd: GridInfo.Rows,
                    colStart: 0, colEnd: GridInfo.Cols);
                loop.Evaluate(mode: loopMode);

                // beam radius calculation
                double wx = 2.0 * Math.Sqrt(x2ISum / iSum);
                double wy = 2.0 * Math.Sqrt(y2ISum / iSum);
                return (wx, wy);
            }

            #endregion
        }

        #endregion
    }

}
