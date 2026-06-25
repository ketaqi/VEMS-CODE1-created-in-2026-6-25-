
using System.Numerics;

namespace VEMS.MathCore
{

    /// <summary>
    /// Represents a one-dimensional grid segmentation, where the grid is used to define centers of segments.
    /// </summary>
    /// <remarks>This class provides functionality for segmenting input distributions (functions or data) into
    /// smaller segments based on the defined grid centers and unit segment profile. It supports various operations such
    /// as extracting segments from input functions or data, and applying interpolation or boundary options during
    /// segmentation.</remarks>
    public class Grid1DSegs
    {
        #region properties

        /// <summary>
        /// Gets or sets the centers of the segments.
        /// </summary>
        /// <remarks>
        /// The centers are defined by a <see cref="GridInfo1D"/> object, which specifies the positions of the segment centers
        /// along the one-dimensional grid. Each center corresponds to the location where a segment is extracted or applied.
        /// </remarks>
        public GridInfo1D? Centers { get; set; }

        /// <summary>
        /// Gets or sets the unit segment profile used for segmentation.
        /// </summary>
        /// <remarks>
        /// The <see cref="Unit"/> property defines the segment profile that is applied at each center specified in <see cref="Centers"/>.
        /// This segment is used as a template for extracting or applying segments to input distributions.
        /// </remarks>
        public Seg1D? Unit { get; set; }

        #endregion
        #region constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Grid1DSegs"/> class with default values.
        /// </summary>
        internal Grid1DSegs() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Grid1DSegs"/> class using the specified grid centers.
        /// </summary>
        /// <param name="centers">A <see cref="GridInfo1D"/> grid specifying the uniformly spaced center positions for the segments.</param>
        public Grid1DSegs(GridInfo1D centers)
        {
            Centers = centers;
        }

        #endregion
        #region methods

        #region ---- func => func ----

        /// <summary>
        /// takes each segment out from a given input distribution (as function)
        /// </summary>
        /// <param name="fIn"> input function </param>
        /// <returns> list of segmented distributions (as function) </returns>
        public List<Func<double, double>> TakeEachFrom(Func<double, double> fIn)
        {
            if (Centers == null) { throw new ArgumentNullException(nameof(Centers)); }
            if (Unit == null) { throw new ArgumentNullException(nameof(Unit)); }

            long n = Centers.Count;
            List<Func<double, double>> fs = new(capacity: (int)n);
            GridInfo1D centers = Centers;
            Seg1D unit = Unit;

            for (long i = 0; i < Centers.Count; i++)
            {
                double center = centers[i];
                fs.Add(unit.TakeFrom(fIn, x0: center));
            }

            return fs;
        }

        /// <summary>
        /// takes each segment out from a given input distribution (as function)
        /// </summary>
        /// <param name="fIn"> input function </param>
        /// <returns> list of segmented distributions (as function) </returns>
        public List<Func<double, Complex>> TakeEachFrom(Func<double, Complex> fIn)
        {
            if (Centers == null) { throw new ArgumentNullException(nameof(Centers)); }
            if (Unit == null) { throw new ArgumentNullException(nameof(Unit)); }

            long n = Centers.Count;
            List<Func<double, Complex>> fs = new(capacity: (int)n);
            GridInfo1D centers = Centers;
            Seg1D unit = Unit;

            for (long i = 0; i < Centers.Count; i++)
            {
                double center = centers[i];
                fs.Add(unit.TakeFrom(fIn, x0: center));
            }

            return fs;
        }

        #endregion
        #region ---- func => data ----

        /// <summary>
        /// takes each segment out from a given input distribution (as function)
        /// </summary>
        /// <param name="fIn"> input function </param>
        /// <param name="nTarget"> target number of samples for each segment </param>
        /// <param name="sdTarget"> target sampling distance for each segment </param>
        /// <param name="intrpl"> interpolation method for the segment </param>
        /// <param name="bound"> boundary option for the segment </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> list of segmented distributions </returns>
        public List<Grid1DRealData> TakeEachFrom(Func<double, double> fIn,
            long nTarget, double sdTarget,
            InterpolationMethod? intrpl = null,
            DataBoundary? bound = null,
            LoopMode loopMode = Defaults.LoopOption)
        {
            if (Centers == null) { throw new ArgumentNullException(nameof(Centers)); }
            if (Unit == null) { throw new ArgumentNullException(nameof(Unit)); }

            long n = Centers.Count;
            List<Grid1DRealData> ds = new(capacity: (int)n);
            Seg1D unit = Unit;
            GridInfo1D centers = Centers;

            for (long i = 0; i < n; i++)
            {
                double center = centers[i];
                GridInfo1D gi = new(n: nTarget, spacing: sdTarget,
                    refPoint: center, refType: GridRefType.Center);
                Grid1DRealData di = unit.TakeFrom(fIn, x0: center,
                    gTarget: gi, intrpl: intrpl, bound: bound, loopMode: loopMode);
                ds.Add(di);
            }
            return ds;
        }

        /// <summary>
        /// takes each segment out from a given input distribution (as function)
        /// </summary>
        /// <param name="fIn"> input function </param>
        /// <param name="nTarget"> target number of samples for each segment </param>
        /// <param name="sdTarget"> target sampling distance for each segment </param>
        /// <param name="intrpl"> interpolation method for the segment </param>
        /// <param name="bound"> boundary option for the segment </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> list of segmented distributions </returns>
        public List<Grid1DCplxData> TakeEachFrom(Func<double, Complex> fIn,
            long nTarget, double sdTarget,
            InterpolationMethod? intrpl = null,
            DataBoundary? bound = null,
            LoopMode loopMode = Defaults.LoopOption)
        {
            if (Centers == null) { throw new ArgumentNullException(nameof(Centers)); }
            if (Unit == null) { throw new ArgumentNullException(nameof(Unit)); }

            long n = Centers.Count;
            List<Grid1DCplxData> ds = new(capacity: (int)n);
            Seg1D unit = Unit;
            GridInfo1D centers = Centers;

            for (long i = 0; i < Centers.Count; i++)
            {
                double center = centers[i];
                GridInfo1D gi = new(n: nTarget, spacing: sdTarget,
                    refPoint: center, refType: GridRefType.Center);
                Grid1DCplxData di = unit.TakeFrom(fIn, x0: center,
                    gTarget: gi, intrpl: intrpl, bound: bound, loopMode: loopMode);
                ds.Add(di);
            }
            return ds;
        }

        #endregion
        #region ---- data => func ----

        /// <summary>
        /// takes each segment out from a given input distribution (as GridData)
        /// </summary>
        /// <param name="dIn"> input data distribution </param>
        /// <returns> list of segmented distributions (as function) </returns>
        public List<Func<double, double>> TakeEachFrom(Grid1DRealData dIn)
        {
            if (Centers == null) { throw new ArgumentNullException(nameof(Centers)); }
            if (Unit == null) { throw new ArgumentNullException(nameof(Unit)); }

            long n = Centers.Count;
            List<Func<double, double>> fs = new(capacity: (int)n);
            GridInfo1D centers = Centers;
            Seg1D unit = Unit;

            for (int i = 0; i < n; i++)
            {
                double center = centers[i];
                fs.Add(unit.TakeFrom(dIn, x0: center));
            }

            return fs;
        }

        /// <summary>
        /// takes each segment out from a given input distribution (as GridData)
        /// </summary>
        /// <param name="dIn"> input data distribution </param>
        /// <returns> list of segmented distributions (as function) </returns>
        public List<Func<double, Complex>> TakeEachFrom(Grid1DCplxData dIn)
        {
            if (Centers == null) { throw new ArgumentNullException(nameof(Centers)); }
            if (Unit == null) { throw new ArgumentNullException(nameof(Unit)); }

            long n = Centers.Count;
            List<Func<double, Complex>> fs = new(capacity: (int)n);
            GridInfo1D centers = Centers;
            Seg1D unit = Unit;

            for (int i = 0; i < n; i++)
            {
                double center = centers[i];
                fs.Add(unit.TakeFrom(dIn, x0: center));
            }

            return fs;
        }

        #endregion
        #region ---- data => data ----

        /// <summary>
        /// Extracts a segment of data from the input grid at the specified index, using the provided grid configuration
        /// and interpolation settings.
        /// </summary>
        /// <remarks>This method extracts a segment of data from the input grid based on the specified
        /// grid configuration. The center point of the segment is determined by the <paramref name="idx"/> parameter.
        /// The extracted segment's size and spacing are defined by <paramref name="nInSEG"/> and <paramref
        /// name="spInSEG"/>, respectively.</remarks>
        /// <param name="dIn">The input grid data from which the segment will be extracted.</param>
        /// <param name="idx">The index of the center point in the grid from which the segment will be taken.</param>
        /// <param name="nInSEG">The number of points in the extracted segment.</param>
        /// <param name="spInSEG">The spacing between points in the extracted segment.</param>
        /// <param name="intrpl">The interpolation method to use when extracting the segment. If <see langword="null"/>, the default
        /// interpolation method is used.</param>
        /// <param name="boundary">The boundary handling strategy to apply during extraction. If <see langword="null"/>, the default boundary
        /// handling is used.</param>
        /// <param name="loopMode">Specifies whether the grid should be treated as looping or non-looping. Defaults to <see
        /// cref="Defaults.LoopOption"/>.</param>
        /// <returns>A <see cref="Grid1DCplxData"/> object representing the extracted segment of data.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the unit segment or center points are not defined.</exception>
        public Grid1DCplxData TakeAt(Grid1DCplxData dIn, long idx,
            long nInSEG, double spInSEG,
            InterpolationMethod? intrpl = null,
            DataBoundary? boundary = null,
            LoopMode loopMode = Defaults.LoopOption)
        {
            if (Centers == null) { throw new ArgumentNullException(nameof(Centers)); }
            if (Unit == null) { throw new ArgumentNullException(nameof(Unit)); }
            if (idx < 0 || idx >= Centers.Count)
            { throw new IndexOutOfRangeException("Index is out of range of the segment centers."); }

            Seg1D unit = Unit;
            double x0 = Centers[idx];

            // takes out the data
            Grid1DCplxData di = unit.TakeFrom(dIn: dIn,
                gTarget: new GridInfo1D(n: nInSEG, spacing: spInSEG, refPoint: x0, refType: GridRefType.Center),
                x0: x0, intrpl: intrpl, bound: boundary, loopMode: loopMode);

            // return
            return di;
        }


        /// <summary>
        /// takes each segment out from a given input distribution (as GridData)
        /// </summary>
        /// <param name="dIn"> input data distribution </param>
        /// <param name="nTarget"> target number of samples for each segment </param>
        /// <param name="sdTarget"> target sampling distance for each segment </param>
        /// <param name="intrpl"> interpolation method for the segment </param>
        /// <param name="bound"> boundary option for the segment </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> list of segmented distributions </returns>
        public List<Grid1DRealData> TakeEachFrom(Grid1DRealData dIn,
            long nTarget, double sdTarget,
            InterpolationMethod? intrpl = null,
            DataBoundary? bound = null,
            LoopMode loopMode = Defaults.LoopOption)
        {
            if (Centers == null) { throw new ArgumentNullException(nameof(Centers)); }
            if (Unit == null) { throw new ArgumentNullException(nameof(Unit)); }

            long n = Centers.Count;
            List<Grid1DRealData> ds = new(capacity: (int)n);
            GridInfo1D centers = Centers;
            Seg1D unit = Unit;

            for (int i = 0; i < Centers.Count; i++)
            {
                double center = centers[i];
                GridInfo1D gi = new(n: nTarget, spacing: sdTarget,
                    refPoint: center, refType: GridRefType.Center);
                Grid1DRealData di = unit.TakeFrom(dIn, x0: center,
                    gTarget: gi, intrpl: intrpl, bound: bound, loopMode: loopMode);
                ds.Add(di);
            }
            return ds;
        }

        /// <summary>
        /// takes each segment out from a given input distribution (as GridData)
        /// </summary>
        /// <param name="dIn"> input data distribution </param>
        /// <param name="nTarget"> target number of samples for each segment </param>
        /// <param name="sdTarget"> target sampling distance for each segment </param>
        /// <param name="intrpl"> interpolation method for the segment </param>
        /// <param name="bound"> boundary option for the segment </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> list of segmented distributions </returns>
        public List<Grid1DCplxData> TakeEachFrom(Grid1DCplxData dIn,
            long nTarget, double sdTarget,
            InterpolationMethod? intrpl = null,
            DataBoundary? bound = null,
            LoopMode loopMode = Defaults.LoopOption)
        {
            if (Centers == null) { throw new ArgumentNullException(nameof(Centers)); }
            if (Unit == null) { throw new ArgumentNullException(nameof(Unit)); }

            long n = Centers.Count;
            List<Grid1DCplxData> ds = new(capacity: (int)n);
            GridInfo1D centers = Centers;
            Seg1D unit = Unit;

            for (int i = 0; i < Centers.Count; i++)
            {
                double center = centers[i];
                GridInfo1D gi = new(n: nTarget, spacing: sdTarget,
                    refPoint: center, refType: GridRefType.Center);
                Grid1DCplxData di = unit.TakeFrom(dIn, gTarget: gi,
                    intrpl: intrpl, bound: bound, loopMode: loopMode);
                ds.Add(di);
            }
            return ds;
        }

        #endregion

        #endregion
        #region derived ...

        /// <summary>
        /// uniform segmentation using rectangular-profile segments
        /// </summary>
        public class CosRect : Grid1DSegs
        {
            #region constructor

            internal CosRect() : base() { }

            /// <summary>
            /// uniform segmentation using rectangular-profile segments
            /// with optional cosine-edged profile
            /// </summary>
            /// <param name="centers"> centers of segments </param>
            /// <param name="diameter"> diameter of each segment </param>
            /// <param name="edge"> edge width of each segment </param>
            public CosRect(GridInfo1D centers,
                double diameter,
                double edge = 0.0) : base(centers)
            {
                Unit = new Seg1D.CosRect(diameter: diameter, edge: edge);
            }

            #endregion
        }

        /// <summary>
        /// uniform segmentation using Gaussian-profile segments
        /// </summary>
        public class Gaussian : Grid1DSegs
        {
            #region constructor

            internal Gaussian() : base() { }

            /// <summary>
            /// uniform segmentation using Gaussian-profile segments
            /// </summary>
            /// <param name="centers"> centers of segments </param>
            /// <param name="diameter"> diameter of each segment </param>
            public Gaussian(GridInfo1D centers,
                double diameter) : base(centers)
            {
                Unit = new Seg1D.Gaussian(diameter: diameter);
            }

            #endregion
        }

        #endregion
    }
}
