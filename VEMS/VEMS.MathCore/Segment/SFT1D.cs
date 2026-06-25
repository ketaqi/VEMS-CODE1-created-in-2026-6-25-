

namespace VEMS.MathCore
{
    /// <summary>
    /// Provides segmented Fourier transform (SFT) operations on 1D complex-valued grid data.
    /// Inherits from <see cref="Grid1DSegs.CosRect"/> to support cosine-edged rectangular segment profiles.
    /// </summary>
    /// <remarks>
    /// This class enables extraction of segments from input data, resampling, and performing 1D Fourier transforms
    /// on each segment, supporting various transform and interpolation options.
    /// </remarks>
    public class SFT1D : Grid1DSegs.CosRect
    {
        #region properties

        // ...

        #endregion
        #region constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SFT1D"/> class with default parameters.
        /// </summary>
        internal SFT1D() : base() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SFT1D"/> class with the specified segment centers, unit width, and edge ratio.
        /// </summary>
        /// <param name="centers">The grid information specifying the centers of the segments.</param>
        /// <param name="unitWidth">The width of each segment.</param>
        /// <param name="edgeRatio">The ratio of the edge width to the unit width. The actual edge width is <paramref name="edgeRatio"/> times <paramref name="unitWidth"/>.</param>
        public SFT1D(GridInfo1D centers,
            double unitWidth,
            double edgeRatio = 0.0)
            : base(centers, unitWidth, edgeRatio * unitWidth)
        { }

        #endregion
        #region methods

        /// <summary>
        /// Extracts a segment centered at a specified location from the input complex-valued grid data,
        /// resamples it on a target grid, and performs a 1D Fourier transform on the segment.
        /// </summary>
        /// <param name="dIn">Input distribution as <see cref="Grid1DCplxData"/>.</param>
        /// <param name="x0">Center coordinate of the segment to extract.</param>
        /// <param name="nInSeg">Number of samples in the target segment.</param>
        /// <param name="spInSeg">Sampling distance for the target segment.</param>
        /// <param name="intrpl">Optional interpolation method for segment extraction. If null, the default is used.</param>
        /// <param name="boundary">Optional data boundary option for segment extraction. If null, the default is used.</param>
        /// <param name="loopMode">Loop-computational mode options. Default is <see cref="Defaults.LoopOption"/>.</param>
        /// <param name="direction">Fourier transform direction. Default is <see cref="FFTOptions.Direction.Forward"/>.</param>
        /// <param name="convention">Fourier transform convention. Default is <see cref="FFTOptions.Convention.ZeroCentered"/>.</param>
        /// <param name="conversion">Fourier transform conversion. Default is <see cref="FFTOptions.Conversion.DataShift"/>.</param>
        /// <param name="copyMode">Copy mode for the transform. Default is <see cref="FFTOptions.CopyMode.Block"/>.</param>
        /// <returns>
        /// The transformed segment as <see cref="Grid1DCplxData"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <see cref="Centers"/> or <see cref="Unit"/> is null.
        /// </exception>
        public Grid1DCplxData TransformAt(Grid1DCplxData dIn, double x0,
            long nInSeg, double spInSeg,
            InterpolationMethod? intrpl = null,
            DataBoundary? boundary = null,
            LoopMode loopMode = Defaults.LoopOption,
            FFTOptions.Direction direction = FFTOptions.Direction.Forward,
            FFTOptions.Convention convention = FFTOptions.Convention.ZeroCentered,
            FFTOptions.Conversion conversion = FFTOptions.Conversion.DataShift,
            FFTOptions.CopyMode copyMode = FFTOptions.CopyMode.Block)
        {
            if (Centers == null) { throw new ArgumentNullException(nameof(Centers)); }
            if (Unit == null) { throw new ArgumentNullException(nameof(Unit)); }

            // gets the segment unit
            Seg1D unit = Unit;

            // takes out the data at x0
            GridInfo1D gt = new(n: nInSeg, spacing: spInSeg,
                refPoint: x0, refType: GridRefType.Center);
            Grid1DCplxData di = unit.TakeFrom(dIn: dIn, gTarget: gt,
                x0: x0, intrpl: intrpl, bound: boundary, loopMode: loopMode);

            // performs Fourier transform
            MathCore.Transform.FFT1D(d: ref di,
                direction: direction,
                convention: convention,
                conversion: conversion,
                copyMode: copyMode);

            // return
            return di;
        }

        /// <summary>
        /// Extracts and transforms segments centered at each segment center from the input data.
        /// Each segment is resampled and a 1D Fourier transform is performed.
        /// </summary>
        /// <param name="dIn">Input distribution as <see cref="Grid1DCplxData"/>.</param>
        /// <param name="nInSeg">Number of samples in each target segment.</param>
        /// <param name="spInSeg">Sampling distance for each target segment.</param>
        /// <param name="intrpl">Optional interpolation method for segment extraction. If null, the default is used.</param>
        /// <param name="boundary">Optional data boundary option for segment extraction. If null, the default is used.</param>
        /// <param name="loopMode">Loop-computational mode options. Default is <see cref="Defaults.LoopOption"/>.</param>
        /// <param name="direction">Fourier transform direction. Default is <see cref="FFTOptions.Direction.Forward"/>.</param>
        /// <param name="convention">Fourier transform convention. Default is <see cref="FFTOptions.Convention.ZeroCentered"/>.</param>
        /// <param name="conversion">Fourier transform conversion. Default is <see cref="FFTOptions.Conversion.DataShift"/>.</param>
        /// <param name="copyMode">Copy mode for the transform. Default is <see cref="FFTOptions.CopyMode.Block"/>.</param>
        /// <returns>
        /// List of transformed segments as <see cref="Grid1DCplxData"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <see cref="Centers"/> or <see cref="Unit"/> is null.
        /// </exception>
        public List<Grid1DCplxData> Transform(Grid1DCplxData dIn,
            long nInSeg, double spInSeg,
            InterpolationMethod? intrpl = null,
            DataBoundary? boundary = null,
            LoopMode loopMode = Defaults.LoopOption,
            FFTOptions.Direction direction = FFTOptions.Direction.Forward,
            FFTOptions.Convention convention = FFTOptions.Convention.ZeroCentered,
            FFTOptions.Conversion conversion = FFTOptions.Conversion.DataShift,
            FFTOptions.CopyMode copyMode = FFTOptions.CopyMode.Block)
        {
            if (Centers == null) { throw new ArgumentNullException(nameof(Centers)); }
            if (Unit == null) { throw new ArgumentNullException(nameof(Unit)); }

            // gets the segment unit
            Seg1D unit = Unit;

            // loop
            List<Grid1DCplxData> ds = new(capacity: (int)Centers.Count);
            for (long i = 0; i < Centers.Count; i++)
            {
                double xi = Centers[i];
                // takes out the data at x0
                GridInfo1D gt = new(n: nInSeg, spacing: spInSeg,
                    refPoint: xi, refType: GridRefType.Center);
                Grid1DCplxData di = unit.TakeFrom(dIn: dIn, gTarget: gt,
                    x0: xi, intrpl: intrpl, bound: boundary, loopMode: loopMode);

                // performs Fourier transform
                MathCore.Transform.FFT1D(d: ref di,
                    direction: direction,
                    convention: convention,
                    conversion: conversion,
                    copyMode: copyMode);

                // add to the list
                ds.Add(di);
            }

            // return
            return ds;
        }

        /// <summary>
        /// Transforms the input data using a segmented approach (obsolete overload).
        /// </summary>
        /// <param name="dIn">Input distribution as <see cref="Grid1DCplxData"/>.</param>
        /// <param name="opt">Fourier transform option: forward or backward.</param>
        /// <param name="nTarget">Target number of samples for each segment.</param>
        /// <param name="sdTarget">Target sampling distance for each segment.</param>
        /// <param name="intrpl">Interpolation method used for the segment.</param>
        /// <param name="bound">Data boundary option used for the segment.</param>
        /// <param name="loopMode">Loop-computational mode options.</param>
        /// <returns>List of transformed results for each segment.</returns>
        [Obsolete]
        public List<Grid1DCplxData> Transform(Grid1DCplxData dIn, FTOption opt,
            long nTarget, double sdTarget,
            //GridInfo1D gLocal,
            InterpolationMethod? intrpl = null,
            DataBoundary? bound = null,
            LoopMode loopMode = Defaults.LoopOption)
        {
            // takes all segments from input and samples them
            List<Grid1DCplxData> ds = TakeEachFrom(dIn, nTarget, sdTarget,
                intrpl, bound, loopMode);

            // loop over all segments
            for (int i = 0; i < Centers.Count; i++)
            {
                // FFT kernel
                Grid1DCplxData di = ds[i];
                MathCore.Transform.FFT1D(d: ref di, option: opt);
                //MathCore.Transform.FFT(x: ds[i].Values, grid: ds[i].GridInfo, opt: opt);
                // linear phase handling
                //Samp1DCplxFunc expLinPhase = new(f:
                //    (kx) => Complex.Exp(-Complex.ImaginaryOne * Centers[i] * kx));
                //VectorZ t = ds[i].Values;
                //expLinPhase.ScaleOn(x: ref t, grid: ds[i].GridInfo, loopMode: loopMode);
            }

            // return
            return ds;
        }

        #endregion
    }
}
