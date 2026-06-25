using VEMS.MathCore;

namespace VEMS.EMSolver
{

    /// <summary>
    /// Represents a one-dimensional (1D) field detector with a uniform sampling grid.
    /// </summary>
    /// <remarks>
    /// Constructs a detector with a uniform sampling grid.
    /// </remarks>
    /// <param name="grid">Uniform sampling grid of the detector.</param>
    public class Detector1D(GridInfo1D grid) : IDetector
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

        #region properties

        /// <summary>
        /// Gets or sets the uniform sampling grid.
        /// </summary>
        public GridInfo1D GridInfo { get; set; } = grid;

        // polarization ...

        #endregion
        #region constructors

        // ...

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
        public VectorD Sample(ScalarField1D v,
            DetectQuantity quantity = DetectQuantity.Magnitude,
            PixelationMode pixelMode = PixelationMode.LinearFit,
            LoopMode loopMode = Defaults.LoopOption)
        {
            if (quantity == DetectQuantity.PowerDensity) { throw new NotImplementedException(nameof(quantity)); }
            if (pixelMode == PixelationMode.Integral) { throw new NotImplementedException(nameof(pixelMode)); }
            // null case handling
            if (v.Field == null) { throw new ArgumentNullException(nameof(v.Field)); }

            // takes out corresponding field quantity
            VectorD q = quantity switch
            {
                DetectQuantity.RealPart => VMath.RealPart(v.Field),
                DetectQuantity.ImagPart => VMath.ImagPart(v.Field),
                DetectQuantity.Magnitude => VMath.Abs(v.Field),
                DetectQuantity.Argument => VMath.Arg(v.Field),
                DetectQuantity.SquaredMagnitude => VMath.Abs(v.Field),
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
            Grid1DRealInterpolation itp = new(v: q, grid: v.Grid, method: method);
            VectorD d = itp.Evaluate(targetGrid: GridInfo, loopMode: loopMode);

            // additional handling for squared magnitude
            if (quantity == DetectQuantity.SquaredMagnitude)
            { d = VMath.Square(d); }

            // return
            return d;
        }

        /// <summary>
        /// Takes out the field quantity and (possibly) resamples it.
        /// </summary>
        /// <param name="v">Field to detect on the detector.</param>
        /// <param name="includeScaling">Whether to include the scaling for the detector.</param>
        /// <param name="quantity">Field quantity to detect.</param>
        /// <param name="targetGrid">Target sampling grid.</param>
        /// <param name="pixelMode">Pixelation mode option.</param>
        /// <param name="loopMode">Loop-computational mode options.</param>
        /// <returns>Selected (possibly resampled) field quantity.</returns>
        private static VectorD TakeResample(SCField1D v,
            bool includeScaling,
            DetectQuantity quantity,
            GridInfo1D targetGrid,
            PixelationMode pixelMode,
            LoopMode loopMode)
        {
            if (v.UValues == null) { throw new ArgumentNullException(nameof(v.UValues)); }
            if (v.UGrid == null) { throw new ArgumentNullException(nameof(v.UGrid)); }

            // scaling?
            VectorZ vals = v.UValues;
            if (includeScaling && v.Scaling != 1.0)
            { vals = VMath.Scale(x: vals, a: v.Scaling); }
            // takes out the field quantity into q
            VectorD q;
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

            // needs resampling?
            if (v.UGrid == targetGrid)
            {
                // no need for resampling ...
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
                Grid1DRealInterpolation itp = new(v: q, grid: v.UGrid, method: intrpl);
                VectorD d = itp.Evaluate(targetGrid: targetGrid, loopMode: loopMode);
                // additional handling for squared magnitude
                if (quantity == DetectQuantity.SquaredMagnitude)
                { d = VMath.Square(d); }
                return d;
            }
        }

        /// <summary>
        /// Samples the field on the detector's grid.
        /// </summary>
        /// <param name="v">Field to detect on the detector.</param>
        /// <param name="includeScaling">Whether to include the scaling for the detector.</param>
        /// <param name="quantity">Field quantity to detect.</param>
        /// <param name="pixelMode">Pixelation mode option.</param>
        /// <param name="loopMode">Loop-computational mode options.</param>
        /// <returns>Sampled field quantity on the detector's grid.</returns>
        /// <exception cref="NotImplementedException">Thrown if the quantity or pixelation mode is not implemented.</exception>
        /// <exception cref="ArgumentNullException">Thrown if the field values are null.</exception>
        public VectorD Sample(SCField1D v,
            bool includeScaling = true,
            DetectQuantity quantity = DetectQuantity.SquaredMagnitude,
            PixelationMode pixelMode = PixelationMode.LinearFit,
            LoopMode loopMode = Defaults.LoopOption)
        {
            if (quantity == DetectQuantity.PowerDensity) { throw new NotImplementedException(nameof(quantity)); }
            if (pixelMode == PixelationMode.Integral) { throw new NotImplementedException(nameof(pixelMode)); }
            // null case handling
            if (v.UValues == null) { throw new ArgumentNullException(nameof(v.UValues)); }

            // takes out corresponding field quantity and (possibly) resamples it
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
        /// <exception cref="ArgumentNullException">Thrown if the field list or any field values are null.</exception>
        public VectorD Sample(Grid1DSCFields v,
            bool includeScaling = true,
            //SumMode sumMode = SumMode.Incoherent,
            DetectQuantity quantity = DetectQuantity.SquaredMagnitude,
            PixelationMode pixelMode = PixelationMode.LinearFit,
            LoopMode loopMode = Defaults.LoopOption)
        {
            if (quantity == DetectQuantity.PowerDensity) { throw new NotImplementedException(nameof(quantity)); }
            if (pixelMode == PixelationMode.Integral) { throw new NotImplementedException(nameof(pixelMode)); }
            // null case handling
            if (v.Fields == null) { throw new ArgumentNullException(nameof(v.Fields)); }

            // loop over all fields
            VectorD d = new(count: GridInfo.Count);
            for (int i = 0; i < v.GridPoints.Count; i++)
            {
                SCField1D vi = v.Fields[i];
                if (vi.UValues == null) { throw new ArgumentNullException(nameof(vi.UValues) + $" @index [{i}]"); }
                VectorD di = TakeResample(vi, includeScaling, quantity, GridInfo, pixelMode, loopMode);
                VMath.AddTo(x: di, y: ref d);
            }
            return d;
        }

        #endregion

        #region derived

        /// <summary>
        /// Represents a beam parameter detector with a uniform sampling grid.
        /// </summary>
        /// <remarks>
        /// Constructs a beam parameter detector with a uniform sampling grid.
        /// </remarks>
        /// <param name="grid">Uniform sampling grid of the detector.</param>
        [Obsolete]
        public class BeamParameters(GridInfo1D grid) : Detector1D(grid)
        {
            #region properties

            /// <summary>
            /// Gets or sets the (paraxial) intensity distribution of the beam.
            /// </summary>
            internal VectorD? Intensity { get; set; } = null;

            #endregion
            #region constructor

            // ...

            #endregion
            #region methods

            /// <summary>
            /// Finds the centroid of the beam using first-order momentum.
            /// </summary>
            /// <param name="v">Field to detect on this detector.</param>
            /// <param name="pixelMode">Pixelation mode option.</param>
            /// <param name="loopMode">Loop-computational mode options.</param>
            /// <returns>Centroid of the beam.</returns>
            public double FindCentroid(SCField1D v,
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
                double iSum = 0.0;
                Action<long> a = i =>
                {
                    double t = Intensity[i, checkBound: false];
                    iSum += t;
                    xISum += t * GridInfo[i];
                };
                Loop1D loop = new(operation: a, start: 0, end: GridInfo.Count);
                loop.Evaluate(mode: loopMode);

                // centroid calculation
                return xISum / iSum;
            }

            /// <summary>
            /// Finds the 1/e^2 radius using second-order momentum.
            /// </summary>
            /// <param name="v">Field to detect on this detector.</param>
            /// <param name="refCenter">Reference beam center; if null, will be calculated automatically.</param>
            /// <param name="pixelMode">Pixelation mode option.</param>
            /// <param name="loopMode">Loop-computational mode options.</param>
            /// <returns>Radius of the beam.</returns>
            public double FindRadius(SCField1D v,
                double? refCenter = null,
                PixelationMode pixelMode = PixelationMode.LinearFit,
                LoopMode loopMode = Defaults.LoopOption)
            {
                // centroid?
                refCenter ??= FindCentroid(v, pixelMode, loopMode);
                { Printer.Write($"Reference beam center calculated at {refCenter.Value}"); }

                // prepares the (paraxial) intensity distribution
                Intensity ??= Sample(v: v, includeScaling: false,
                    quantity: DetectQuantity.SquaredMagnitude,
                    pixelMode: pixelMode,
                    loopMode: loopMode);

                // second-order momentum
                double x2ISum = 0.0;
                double iSum = 0.0;
                Action<long> a = i =>
                {
                    double t = Intensity[i, checkBound: false];
                    iSum += t;
                    double xi = GridInfo[i] - refCenter.Value;
                    x2ISum += t * xi * xi;
                };
                Loop1D loop = new(operation: a, start: 0, end: GridInfo.Count);
                loop.Evaluate(mode: loopMode);

                // beam radius calculation
                return 2.0 * Math.Sqrt(x2ISum / iSum);
            }

            #endregion
        }

        #endregion
    }

}
