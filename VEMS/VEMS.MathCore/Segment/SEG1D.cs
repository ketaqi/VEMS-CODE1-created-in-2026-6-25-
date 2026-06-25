using System.Numerics;
using Complex = System.Numerics.Complex;

namespace VEMS.MathCore
{
    
    ///<summary>
    /// Represents a one-dimensional segment with an arbitrary local profile.
    /// Provides methods to extract segments from input distributions (functions or grid data)
    /// and to generate derived segment types (e.g., cosine-edged rectangle, Gaussian).
    /// </summary>
    public class Seg1D
    {
        #region properties

        /// <summary>
        /// Function that defines the segment's profile l = l(x) in the local coordinate system.
        /// <para>Variable: x - local coordinate centered around x0.</para>
        /// <para>Result: l = l(x).</para>
        /// </summary>
        internal Func<double, double> L { get; set; }

        /// <summary>
        /// Function that defines the segment's profile p = p(x) in the global coordinate system.
        /// <para>Variable: x - global coordinate.</para>
        /// <para>Parameter: x0 - center position defined in global coordinate.</para>
        /// <para>Result: p = p(x; x0) = l(x-x0).</para>
        /// </summary>
        public Func<double, double, double> P => (x, x0) => L(x - x0);

        /// <summary>
        /// Diameter (full-width) of the segment.
        /// </summary>
        public double Diameter { get; set; }

        #endregion
        #region constructors

        /// <summary>
        /// Default constructor. Initializes the segment with a constant profile l(x) = 1.0.
        /// </summary>
        internal Seg1D()
        {
            L = (x) => 1.0;
        }

        /// <summary>
        /// Constructs a 1D segment with an arbitrary local profile.
        /// </summary>
        /// <param name="l">Function that defines the local profile of the segment.</param>
        internal Seg1D(Func<double, double> l)
        {
            L = l;
        }

        #endregion
        #region methods

        #region ---- func => func ----

        /// <summary>
        /// Takes the segment out of a given input distribution (as function) for a generic numeric type.
        /// </summary>
        /// <typeparam name="T">Numeric type implementing <see cref="INumber{T}"/>.</typeparam>
        /// <param name="fIn">Input distribution (as function).</param>
        /// <param name="x0">Center of the segment.</param>
        /// <returns>Segmented distribution (as function) of type <typeparamref name="T"/>.</returns>
        internal Func<double, T> TakeFrom<T>(Func<double, T> fIn, 
            double x0)
            where T : INumber<T>
            => (x) => fIn(x) * T.CreateChecked(P(x, x0));

        /// <summary>
        /// Takes the segment out of a given input distribution (as function).
        /// </summary>
        /// <param name="fIn">Input distribution (as function).</param>
        /// <param name="x0">Center of the segment.</param>
        /// <returns>Segmented distribution (as function).</returns>
        public Func<double, double> TakeFrom(Func<double, double> fIn,
            double x0)
            => (x) => fIn(x) * P(x, x0);

        /// <summary>
        /// Takes the segment out of a given input distribution (as function) for complex-valued input.
        /// </summary>
        /// <param name="fIn">Input distribution (as function).</param>
        /// <param name="x0">Center of the segment.</param>
        /// <returns>Segmented distribution (as function).</returns>
        public Func<double, Complex> TakeFrom(Func<double, Complex> fIn,
            double x0)
            => (x) => fIn(x) * P(x, x0);

        #endregion
        #region ---- func => data ----

        /// <summary>
        /// Takes the segment out of a given input distribution (as function) and samples it on a target grid.
        /// </summary>
        /// <param name="fIn">Input distribution (as function).</param>
        /// <param name="x0">Center of the segment.</param>
        /// <param name="gTarget">Target sampling grid for the segment; not necessarily centered around x0.</param>
        /// <param name="intrpl">Interpolation method for the segment.</param>
        /// <param name="bound">Boundary option for the segment.</param>
        /// <param name="loopMode">Loop-computational mode options.</param>
        /// <returns>Segmented distribution (as <see cref="Grid1DRealData"/>).</returns>
        public Grid1DRealData TakeFrom(Func<double, double> fIn,
            double x0,
            GridInfo1D gTarget,
            InterpolationMethod? intrpl = null,
            DataBoundary? bound = null,
            LoopMode loopMode = Defaults.LoopOption)
        {
            Samp1DRealFunc sf = new(f: TakeFrom(fIn, x0));
            VectorD x = sf.Sample(grid: gTarget, loopMode: loopMode);
            return new Grid1DRealData(values: x,
                gridInfo: new(gTarget), // makes a new grid
                intrpl: intrpl ?? Defaults.IntrplOption,
                bound: bound ?? Defaults.BoundaryOption);
        }

        /// <summary>
        /// Takes the segment out of a given input distribution (as function) and samples it on a target grid (complex-valued).
        /// </summary>
        /// <param name="fIn">Input distribution (as function).</param>
        /// <param name="x0">Center of the segment.</param>
        /// <param name="gTarget">Target sampling grid for the segment; not necessarily centered around x0.</param>
        /// <param name="intrpl">Interpolation method for the segment.</param>
        /// <param name="bound">Boundary option for the segment.</param>
        /// <param name="loopMode">Loop-computational mode options.</param>
        /// <returns>Segmented distribution (as <see cref="Grid1DCplxData"/>).</returns>
        public Grid1DCplxData TakeFrom(Func<double, Complex> fIn,
            double x0,
            GridInfo1D gTarget,
            InterpolationMethod? intrpl = null,
            DataBoundary? bound = null,
            LoopMode loopMode = Defaults.LoopOption)
        {
            Samp1DCplxFunc sf = new(f: TakeFrom(fIn, x0));
            VectorZ x = sf.Sample(grid: gTarget, loopMode: loopMode);
            return new Grid1DCplxData(values: x,
                gridInfo: new(gTarget), // makes a new grid
                intrpl: intrpl ?? Defaults.IntrplOption,
                bound: bound ?? Defaults.BoundaryOption);
        }

        #endregion
        #region ---- data => func ----

        /// <summary>
        /// Takes the segment out of a given input distribution (as <see cref="Grid1DRealData"/>).
        /// </summary>
        /// <param name="dIn">Input distribution (as <see cref="Grid1DRealData"/>).</param>
        /// <param name="x0">Center of the segment.</param>
        /// <returns>Segmented distribution (as function).</returns>
        public Func<double, double> TakeFrom(Grid1DRealData dIn,
            double x0)
            => (x) => dIn.FindValue(x) * P(x, x0);

        /// <summary>
        /// Takes the segment out of a given input distribution (as <see cref="Grid1DCplxData"/>).
        /// </summary>
        /// <param name="dIn">Input distribution (as <see cref="Grid1DCplxData"/>).</param>
        /// <param name="x0">Center of the segment.</param>
        /// <returns>Segmented distribution (as function).</returns>
        public Func<double, Complex> TakeFrom(Grid1DCplxData dIn,
            double x0)
            => (x) => dIn.FindValue(x) * P(x, x0);

        #endregion
        #region ---- data => data ----

        /// <summary>
        /// Takes the segment out of a given input distribution (as <see cref="Grid1DRealData"/>)
        /// and samples it on a target grid.
        /// </summary>
        /// <param name="dIn">Input distribution (as <see cref="Grid1DRealData"/>).</param>
        /// <param name="gTarget">Target sampling grid for the segment.</param>
        /// <param name="x0">Center of the segment; if null, takes the center of the target grid.</param>
        /// <param name="intrpl">Interpolation method for the segment.</param>
        /// <param name="bound">Boundary option for the segment.</param>
        /// <param name="loopMode">Loop-computational mode options.</param>
        /// <returns>Segmented distribution (as <see cref="Grid1DRealData"/>).</returns>
        public Grid1DRealData TakeFrom(Grid1DRealData dIn,
            GridInfo1D gTarget,
            double? x0 = null,
            InterpolationMethod? intrpl = null,
            DataBoundary? bound = null,
            LoopMode loopMode = Defaults.LoopOption)
        {
            Samp1DRealFunc sf = new(f: TakeFrom(dIn, x0: x0 ?? gTarget.Center));
            VectorD x = sf.Sample(grid: gTarget, loopMode: loopMode);
            return new Grid1DRealData(values: x,
                gridInfo: new(gTarget), // makes a new grid
                intrpl: intrpl ?? dIn.Interpolation.Method,
                bound: bound ?? Defaults.BoundaryOption);
        }

        /// <summary>
        /// Takes the segment out of a given input distribution (as <see cref="Grid1DCplxData"/>)
        /// and samples it on a target grid.
        /// </summary>
        /// <param name="dIn">Input distribution (as <see cref="Grid1DCplxData"/>).</param>
        /// <param name="gTarget">Target sampling grid for the segment.</param>
        /// <param name="x0">Center of the segment; if null, takes the center of the target grid.</param>
        /// <param name="intrpl">Interpolation method for the segment.</param>
        /// <param name="bound">Boundary option for the segment.</param>
        /// <param name="loopMode">Loop-computational mode options.</param>
        /// <returns>Segmented distribution (as <see cref="Grid1DCplxData"/>).</returns>
        public Grid1DCplxData TakeFrom(Grid1DCplxData dIn,
            GridInfo1D gTarget,
            double? x0 = null,
            InterpolationMethod? intrpl = null,
            DataBoundary? bound = null,
            LoopMode loopMode = Defaults.LoopOption)
        {
            Samp1DCplxFunc sf = new(f: TakeFrom(dIn, x0: x0 ?? gTarget.Center));
            VectorZ x = sf.Sample(grid: gTarget, loopMode: loopMode);
            return new Grid1DCplxData(values: x,
                gridInfo: new(gTarget), // makes a new grid
                intrpl: intrpl ?? dIn.Interpolation.Method,
                bound: bound ?? Defaults.BoundaryOption);
        }

        #endregion

        #endregion
        #region derived ...

        /// <summary>
        /// 1D segment with (cosine-edged) rectangular profile.
        /// </summary>
        public class CosRect : Seg1D
        {
            #region properties

            /// <summary>
            /// Absolute edge width (half within, half outside).
            /// </summary>
            public double EdgeWidth { get; set; }

            #endregion
            #region constructor

            /// <summary>
            /// Constructs a 1D segment with cosine-edged rectangular profile.
            /// </summary>
            /// <param name="diameter">Diameter (full width) of the rectangle.</param>
            /// <param name="edge">Absolute edge width (half within, half outside).</param>
            public CosRect(double diameter,
                double edge = 0.0)
            {
                // parameters
                EdgeWidth = edge;
                // base parameters
                Diameter = diameter;
                L = (x) => Function1D.CosEdgeRectangle(x, new() { Diameter, EdgeWidth });
            }

            #endregion
        }

        /// <summary>
        /// 1D segment with Gaussian profile.
        /// </summary>
        public class Gaussian : Seg1D
        {
            #region properties

            /// <summary>
            /// Waist radius of the Gaussian profile.
            /// </summary>
            public double WaistRadius => 0.5 * Diameter;

            #endregion
            #region constructor

            /// <summary>
            /// Constructs a 1D segment with Gaussian profile.
            /// </summary>
            /// <param name="diameter">Waist diameter of the Gaussian profile.</param>
            public Gaussian(double diameter)
            {
                // base parameters
                Diameter = diameter;
                L = (x) => Function1D.Gaussian(x, new() { WaistRadius });
            }

            #endregion
        }

        #endregion
    }


    //public class Seg1D : SEG1D { }



    /// <summary>
    /// one-dimensional segment class
    /// </summary>
    [Obsolete]
    public class Segment1D
    {
        #region properties

        /// <summary>
        /// center position of the segment
        /// with respect to the global coordinate system
        /// </summary>
        public double X0 { get; set; }

        /// <summary>
        /// function that defines the segment's 
        /// aperture/profile p = f(x)
        /// <para> variable: x - local coordinate centered around x0 </para>
        /// <para> function: magnitude of the profile </para>
        /// </summary>
        public Func<double, double> Profile { get; set; }

        #endregion
        #region constructor

        /// <summary>
        /// default constructor
        /// </summary>
        internal Segment1D()
        {
            Profile = (x) => 0.0;
        }

        /// <summary>
        /// constructs a 1D segment with given parameters
        /// </summary>
        /// <param name="x0"> center of the segment </param>
        /// <param name="profile"> function that defines the aperture/profile of the segment </param>
        public Segment1D(double x0, Func<double, double> profile)
        {
            X0 = x0;
            Profile = profile;
        }

        #endregion
        #region methods

        /// <summary>
        /// takes the segment out of a given input distribution (function)
        /// </summary>
        /// <param name="fIn"> input distribution (as function) </param>
        /// <returns> segmented distribution (as function) in the local coordinate system </returns>
        public Func<double, double> TakeFrom(Func<double, double> fIn)
            => (x) => fIn(x + X0) * Profile(x);

        /// <summary>
        /// takes the segment out of a given input distribution (function)
        /// </summary>
        /// <param name="fIn"> input distribution (as function) </param>
        /// <returns> segmented distributino (as function) in the local coordinate system </returns>
        public Func<double, Complex> TakeFrom(Func<double, Complex> fIn)
            => (x) => fIn(x + X0) * Profile(x);

        /// <summary>
        /// takes the segment out of a given input distribution (function)
        /// </summary>
        /// <param name="fIn"> input distribution (as function) </param>
        /// <param name="gLocal"> local uniform sampling grid for the segment </param>
        /// <param name="intrpl"> interpolation method used for the segment </param>
        /// <param name="bound"> data boundary option used for the segment </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> segmented distribution (as GridData) in the local coordinate system </returns>
        public Grid1DRealData TakeFrom(Func<double, double> fIn,
            GridInfo1D gLocal,
            InterpolationMethod intrpl = Defaults.IntrplOption,
            DataBoundary bound = Defaults.BoundaryOption,
            LoopMode loopMode = Defaults.LoopOption)
        {
            Samp1DRealFunc sf = new(f: TakeFrom(fIn));
            VectorD v = sf.Sample(grid: gLocal, loopMode: loopMode);
            return new Grid1DRealData(values: v, gridInfo: new(gLocal), // makes a new grid
                intrpl: intrpl, bound: bound);
        }

        /// <summary>
        /// takes the segment out of a given input distribution (function)
        /// </summary>
        /// <param name="fIn"> input distribution (as function) </param>
        /// <param name="gLocal"> local uniform sampling grid for the segment </param>
        /// <param name="intrpl"> interpolation method used for the segment </param>
        /// <param name="bound"> data boundary option used for the segment </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> segmented distribution (as GridData) in the local coordinate system </returns>
        public Grid1DCplxData TakeFrom(Func<double, Complex> fIn,
            GridInfo1D gLocal,
            InterpolationMethod intrpl = Defaults.IntrplOption,
            DataBoundary bound = Defaults.BoundaryOption,
            LoopMode loopMode = Defaults.LoopOption)
        {
            Samp1DCplxFunc sf = new(f: TakeFrom(fIn));
            VectorZ v = sf.Sample(grid: gLocal, loopMode: loopMode);
            return new Grid1DCplxData(values: v, gridInfo: new(gLocal), // makes a new grid
                intrpl: intrpl, bound: bound);
        }

        /// <summary>
        /// takes the segment out of a given input distribution (as GridData)
        /// with specific interpolation method
        /// </summary>
        /// <param name="dIn"> input distribution (as GridData) </param>
        /// <returns> segmented distribution (as function) in the local coordinate system </returns>
        public Func<double, double> TakeFrom(Grid1DRealData dIn)
            => (x) => dIn.FindValue(x + X0) * Profile(x);

        /// <summary>
        /// takes the segment out of a given input distribution (as GridData)
        /// with specific interpolation method
        /// </summary>
        /// <param name="dIn"> input distribution (as GridData) </param>
        /// <returns> segmented distribution (as a function) in the local coordinate system </returns>
        public Func<double, Complex> TakeFrom(Grid1DCplxData dIn)
            => (x) => dIn.FindValue(x + X0) * Profile(x);

        /// <summary>
        /// takes the segment out of a given input distribution (as GridData)
        /// with specific interpolation method
        /// </summary>
        /// <param name="dIn"> input distribution (as GridData) </param>
        /// <param name="gLocal"> local uniform sampling grid for the segment </param>
        /// <param name="intrpl"> interpolation method used for the segment </param>
        /// <param name="bound"> data boundary option used for the segment </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> segmented distribution (as GridData) in the local coordinate system </returns>
        public Grid1DRealData TakeFrom(Grid1DRealData dIn,
            GridInfo1D gLocal,
            InterpolationMethod? intrpl = null,
            DataBoundary? bound = null,
            LoopMode loopMode = Defaults.LoopOption)
        {
            Samp1DRealFunc sf = new(f: TakeFrom(dIn));
            VectorD x = sf.Sample(grid: gLocal, loopMode: loopMode);
            return new Grid1DRealData(values: x, gridInfo: new(gLocal), // makes a new grid
                intrpl: intrpl ?? dIn.Interpolation.Method, bound: bound ?? Defaults.BoundaryOption);
        }

        /// <summary>
        /// takes the segment out of a given input distribution (as GridData)
        /// with specific interpolation method
        /// </summary>
        /// <param name="dIn"> input distribution (as GridData) </param>
        /// <param name="gLocal"> local uniform sampling grid for the segment </param>
        /// <param name="intrpl"> interpolation method used for the segment </param>
        /// <param name="bound"> data boundary option used for the segment </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> segmented distribution (as GridData) in the local coordinate system </returns>
        public Grid1DCplxData TakeFrom(Grid1DCplxData dIn,
            GridInfo1D gLocal,
            InterpolationMethod? intrpl = null,
            DataBoundary? bound = null,
            LoopMode loopMode = Defaults.LoopOption)
        {
            Samp1DCplxFunc sf = new(f: TakeFrom(dIn));
            VectorZ x = sf.Sample(grid: gLocal, loopMode: loopMode);
            return new Grid1DCplxData(values: x, gridInfo: new(gLocal), // makes a new grid
                intrpl: intrpl ?? dIn.Interpolation.Method, bound: bound ?? Defaults.BoundaryOption);
        }

        ///// <summary>
        ///// takes the segment out of a given input data distribution
        ///// and samples it on a target uniform grid
        ///// </summary>
        ///// <param name="dIn"> input data distribution </param>
        ///// <param name="gLocal"> local uniform sampling grid for the segment </param>
        ///// <param name="loopMode"> loop-computational mode options </param>
        ///// <returns> sampled values within the segment </returns>
        //[Obsolete]
        //public VectorD SampleFrom(Grid1DRealData dIn, GridInfo1D gLocal,
        //    LoopMode loopMode = Defaults.LoopOption)
        //{
        //    Samp1DRealFunc sf = new(f: TakeFrom(dIn));
        //    return sf.Sample(grid: gLocal, loopMode: loopMode);
        //}

        ///// <summary>
        ///// takes the segment out of a given input data distribution
        ///// and samples it on a target uniform grid
        ///// </summary>
        ///// <param name="dIn"> input data distribution </param>
        ///// <param name="gLocal"> local uniform sampling grid for the segment </param>
        ///// <param name="loopMode"> loop-computational mode options </param>
        ///// <returns> sampled values within the segment </returns>
        //[Obsolete]
        //public VectorZ SampleFrom(Grid1DCplxData dIn, GridInfo1D gLocal,
        //    LoopMode loopMode = Defaults.LoopOption)
        //{
        //    Samp1DCplxFunc sf = new(f: TakeFrom(dIn));
        //    return sf.Sample(grid: gLocal, loopMode: loopMode);
        //}

        #endregion
        #region derived sub-classes

        /// <summary>
        /// 1D segment with (cosine-edged) rectangular profile
        /// </summary>
        public class CosRect : Segment1D
        {
            #region properties

            /// <summary>
            /// full width of the rectangle (when there is no smooth edges)
            /// </summary>
            public double Width { get; set; }

            /// <summary>
            /// absolute edge width (half within, half outside)
            /// </summary>
            public double Edge { get; set; }

            #endregion
            #region constructor

            /// <summary>
            /// constructs a 1D segment with cosine-edged 
            /// rectangular profile
            /// </summary>
            /// <param name="x0"> center of the segment </param>
            /// <param name="width"> full width of the rectangle (when there is not smooth edges) </param>
            /// <param name="edge"> absolute edge width (half within, half outside) </param>
            public CosRect(double x0, double width,
                double edge = 0.0)
            {
                // parameters
                Width = width;
                Edge = edge;
                // base parameters
                X0 = x0;
                Profile = (x) => Function1D.CosEdgeRectangle(arg1: x,
                    arg2: new List<double> { Width, Edge, 0.0, 1.0 });
            }

            #endregion
        }

        /// <summary>
        /// 1D segment with Gaussian profile
        /// </summary>
        public class Gaussian : Segment1D
        {
            #region properties

            /// <summary>
            /// waist radius of the Gaussian profile
            /// </summary>
            public double Waist { get; set; }

            #endregion
            #region constructor

            /// <summary>
            /// constructs a 1D segment with Gaussian profile
            /// </summary>
            /// <param name="x0"> center of the segment </param>
            /// <param name="waist"> waist radius of the Gaussian profile </param>
            public Gaussian(double x0, double waist)
            {
                // parameters
                Waist = waist;
                // base parameters
                X0 = x0;
                Profile = (x) => Function1D.Gaussian(arg1: x,
                    arg2: new List<double> { Waist, 0.0, 1.0 });
            }

            #endregion
        }

        #endregion
    }

    /// <summary>
    /// uniform segmentation with rectangular segments
    /// </summary>
    [Obsolete]
    public class UniformSeg1D
    {
        #region properties

        /// <summary>
        /// centers of the segments
        /// </summary>
        public GridInfo1D Centers { get; set; }

        /// <summary>
        /// list of unit/elementary rectangular segment units
        /// with varying center and fixed width and edge
        /// </summary>
        public List<Segment1D.CosRect> Units { get; set; }

        #endregion
        #region constructors

        /// <summary>
        /// constructs uniform rectangular segments with 
        /// arbitary centers and elementary segment
        /// </summary>
        /// <param name="centers"> centers of the segments </param>
        /// <param name="unit"> elementary segment unit with varying center </param>
        public UniformSeg1D(GridInfo1D centers,
            Segment1D.CosRect unit)
        {
            Centers = centers;
            Units = new List<Segment1D.CosRect>();
            for (int i = 0; i < Centers.Count; i++)
            { Units.Add(new(x0: Centers[i], width: unit.Width, edge: unit.Edge)); }
        }

        /// <summary>
        /// constructs uniform rectangular segments in 
        /// complementary manner 
        /// </summary>
        /// <param name="ns"> number of segments </param>
        /// <param name="unitWidth"> width of the unit/elementary segment </param>
        /// <param name="edgeRatio"> edge ratio of the elementary segment </param>
        public UniformSeg1D(int ns,
            double unitWidth,
            double edgeRatio = 0.0)
        {
            Centers = new(n: ns, spacing: unitWidth); // spacing fixed by unitWidth
            Units = new List<Segment1D.CosRect>();
            for (int i = 0; i < Centers.Count; i++)
            { Units.Add(new(x0: Centers[i], width: unitWidth, edge: edgeRatio * unitWidth)); }
        }

        #endregion
        #region methods 

        /// <summary>
        /// takes each segments out from a given input distribution (as function)
        /// </summary>
        /// <param name="fIn"> input function </param>
        /// <returns> list of segmented distributions (as function) in their local coordinate </returns>
        public List<Func<double, double>> TakeEachFrom(Func<double, double> fIn)
        {
            List<Func<double, double>> fs = new();
            for (int i = 0; i < Units.Count; i++)
            { fs.Add(Units[i].TakeFrom(fIn)); }
            return fs;
        }

        /// <summary>
        /// takes each segments out from a given input distribution (as function)
        /// </summary>
        /// <param name="fIn"> input function </param>
        /// <returns> list of segmented distributions (as function) in their local coordinate </returns>
        public List<Func<double, Complex>> TakeEachFrom(Func<double, Complex> fIn)
        {
            List<Func<double, Complex>> fs = new();
            for (int i = 0; i < Units.Count; i++)
            { fs.Add(Units[i].TakeFrom(fIn)); }
            return fs;
        }

        /// <summary>
        /// takes each segments out from a given input distribution (as GridData)
        /// </summary>
        /// <param name="dIn"> input data distribution </param>
        /// <returns> list of segmented distributions (as function) in their local coordinate </returns>
        public List<Func<double, double>> TakeEachFrom(Grid1DRealData dIn)
        {
            List<Func<double, double>> fs = new();
            for (int i = 0; i < Units.Count; i++)
            { fs.Add(Units[i].TakeFrom(dIn)); }
            return fs;
        }

        /// <summary>
        /// takes each segments out from a given input distribution (as GridData)
        /// </summary>
        /// <param name="dIn"> input data distribution </param>
        /// <returns> list of segmented distributions (as function) in their local coordinate </returns>
        public List<Func<double, Complex>> TakeEachFrom(Grid1DCplxData dIn)
        {
            List<Func<double, Complex>> fs = new();
            for (int i = 0; i < Units.Count; i++)
            { fs.Add(Units[i].TakeFrom(dIn)); }
            return fs;
        }

        /// <summary>
        /// takes each segment out of a given input distribution (as GridData)
        /// and samples them on the same local uniform grid
        /// </summary>
        /// <param name="dIn"> input data distribution </param>
        /// <param name="gLocal"> local uniform sampling grid for each segment </param>
        /// <param name="intrpl"> interpolation method used for the segment </param>
        /// <param name="bound"> data boundary option used for the segment </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> list of segmented distributions (as GridData) in their local coordinate </returns>
        public List<Grid1DRealData> TakeEachFrom(Grid1DRealData dIn,
            GridInfo1D gLocal,
            InterpolationMethod? intrpl = null,
            DataBoundary? bound = null,
            LoopMode loopMode = Defaults.LoopOption)
        {
            List<Grid1DRealData> ds = new();
            for (int i = 0; i < Units.Count; i++)
            { ds.Add(Units[i].TakeFrom(dIn, gLocal, intrpl, bound, loopMode)); }
            return ds;
        }

        /// <summary>
        /// takes each segment out of a given input distribution (as GridData)
        /// and samples them on the same local uniform grid
        /// </summary>
        /// <param name="dIn"> input data distribution </param>
        /// <param name="gLocal"> local uniform sampling grid for each segment </param>
        /// <param name="intrpl"> interpolation method used for the segment </param>
        /// <param name="bound"> data boundary option used for the segment </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> list of segmented distributions (as GridData) in their local coordinate </returns>
        public List<Grid1DCplxData> TakeEachFrom(Grid1DCplxData dIn,
            GridInfo1D gLocal,
            InterpolationMethod? intrpl = null,
            DataBoundary? bound = null,
            LoopMode loopMode = Defaults.LoopOption)
        {
            List<Grid1DCplxData> ds = new();
            for (int i = 0; i < Units.Count; i++)
            { ds.Add(Units[i].TakeFrom(dIn, gLocal, intrpl, bound, loopMode)); }
            return ds;
        }

        #endregion
        #region wrapper methods

        ///// <summary>
        ///// takes content of a segment out of a given input data
        ///// </summary>
        ///// <param name="iSeg"> index of the segment </param>
        ///// <param name="vIn"> input values </param>
        ///// <param name="gIn"> sampling grid for the input </param>
        ///// <param name="g"> local sampling grid co-centered with the segment </param>
        ///// <param name="interp"> interpolation method option </param>
        ///// <param name="loopMode"> loop-computational mode options </param>
        ///// <returns> content within the segment </returns>
        //public VectorD TakeFrom(int iSeg, VectorD vIn, GridInfo1D gIn,
        //    GridInfo1D g,
        //    InterpolationMethod interp = Defaults.InterpOption,
        //    LoopMode loopMode = Defaults.LoopOption)
        //    => Units[iSeg].TakeValuesFrom(vIn, gIn, g, interp, loopMode);

        ///// <summary>
        ///// takes content of a segment out of a given input data
        ///// </summary>
        ///// <param name="iSeg"> index of the segment </param>
        ///// <param name="vIn"> input values </param>
        ///// <param name="gIn"> sampling grid for the input </param>
        ///// <param name="g"> local sampling grid co-centered with the segment </param>
        ///// <param name="interp"> interpolation method option </param>
        ///// <param name="loopMode"> loop-computational mode options </param>
        ///// <returns> content within the segment </returns>
        //public VectorZ TakeFrom(int iSeg, VectorZ vIn, GridInfo1D gIn,
        //    GridInfo1D g,
        //    InterpolationMethod interp = Defaults.InterpOption,
        //    LoopMode loopMode = Defaults.LoopOption)
        //    => Units[iSeg].TakeValuesFrom(vIn, gIn, g, interp, loopMode);

        ///// <summary>
        ///// takes content of a segment out of a given input data
        ///// </summary>
        ///// <param name="iSeg"> index of the segment </param>
        ///// <param name="f"> input function </param>
        ///// <returns> content within the segment </returns>
        //public Func<double, double> TakeFrom(int iSeg, Func<double, double> f)
        //    => Units[iSeg].TakeFrom(f); 

        ///// <summary>
        ///// takes content of a segment out of a given input data
        ///// </summary>
        ///// <param name="iSeg"> index of the segment </param>
        ///// <param name="f"> input function </param>
        ///// <returns> content within the segment </returns>
        //public Func<double, Complex> TakeFrom(int iSeg, Func<double, Complex> f)
        //    => Units[iSeg].TakeFrom(f); 

        #endregion
    }


    ///// <summary>
    ///// uniform segmentation
    ///// </summary>
    //public class UniformSEG1D
    //{
    //    #region properties

    //    /// <summary>
    //    /// centers of the segments
    //    /// </summary>
    //    public GridInfo1D Centers { get; set; }

    //    /// <summary>
    //    /// unit segment
    //    /// </summary>
    //    public SEG1D Unit { get; set; }

    //    #endregion
    //    #region constructors

    //    /// <summary>
    //    /// uniform segmentation with default unit segment
    //    /// </summary>
    //    /// <param name="centers"> uniformly spaced center positions </param>
    //    internal UniformSEG1D(GridInfo1D centers)
    //    {
    //        Centers = centers;
    //        Unit = new SEG1D.CosRect(diameter: 1.0);
    //    }

    //    #endregion
    //    #region methods

    //    #region ---- func => func ----

    //    /// <summary>
    //    /// takes each segment out from a given input distribution (as function)
    //    /// </summary>
    //    /// <param name="fIn"> input function </param>
    //    /// <returns> list of segmented distributions (as function) </returns>
    //    public List<Func<double, double>> TakeEachFrom(Func<double, double> fIn)
    //    {
    //        List<Func<double, double>> fs = [];
    //        for (long i = 0; i < Centers.Count; i++)
    //        { fs.Add(Unit.TakeFrom(fIn, x0: Centers[i])); }
    //        return fs;
    //    }

    //    /// <summary>
    //    /// takes each segment out from a given input distribution (as function)
    //    /// </summary>
    //    /// <param name="fIn"> input function </param>
    //    /// <returns> list of segmented distributions (as function) </returns>
    //    public List<Func<double, Complex>> TakeEachFrom(Func<double, Complex> fIn)
    //    {
    //        List<Func<double, Complex>> fs = [];
    //        for (long i = 0; i < Centers.Count; i++)
    //        { fs.Add(Unit.TakeFrom(fIn, x0: Centers[i])); }
    //        return fs;
    //    }

    //    #endregion
    //    #region ---- func => data ----

    //    /// <summary>
    //    /// takes each segment out from a given input distribution (as function)
    //    /// </summary>
    //    /// <param name="fIn"> input function </param>
    //    /// <param name="nTarget"> target number of samples for each segment </param>
    //    /// <param name="sdTarget"> target sampling distance for each segment </param>
    //    /// <param name="intrpl"> interpolation method for the segment </param>
    //    /// <param name="bound"> boundary option for the segment </param>
    //    /// <param name="loopMode"> loop-computational mode options </param>
    //    /// <returns> list of segmented distributions </returns>
    //    public List<Grid1DRealData> TakeEachFrom(Func<double, double> fIn,
    //        long nTarget, double sdTarget,
    //        InterpolationMethod? intrpl = null,
    //        DataBoundary? bound = null,
    //        LoopMode loopMode = Defaults.LoopOption)
    //    {
    //        List<Grid1DRealData> ds = [];
    //        for (long i = 0; i < Centers.Count; i++)
    //        {
    //            GridInfo1D gi = new(n: nTarget, spacing: sdTarget,
    //                refPoint: Centers[i], refType: GridRefType.Center);
    //            Grid1DRealData di = Unit.TakeFrom(fIn, x0: Centers[i],
    //                gTarget: gi, intrpl: intrpl, bound: bound, loopMode: loopMode);
    //            ds.Add(di); 
    //        }
    //        return ds;
    //    }

    //    /// <summary>
    //    /// takes each segment out from a given input distribution (as function)
    //    /// </summary>
    //    /// <param name="fIn"> input function </param>
    //    /// <param name="nTarget"> target number of samples for each segment </param>
    //    /// <param name="sdTarget"> target sampling distance for each segment </param>
    //    /// <param name="intrpl"> interpolation method for the segment </param>
    //    /// <param name="bound"> boundary option for the segment </param>
    //    /// <param name="loopMode"> loop-computational mode options </param>
    //    /// <returns> list of segmented distributions </returns>
    //    public List<Grid1DCplxData> TakeEachFrom(Func<double, Complex> fIn,
    //        long nTarget, double sdTarget,
    //        InterpolationMethod? intrpl = null,
    //        DataBoundary? bound = null,
    //        LoopMode loopMode = Defaults.LoopOption)
    //    {
    //        List<Grid1DCplxData> ds = [];
    //        for (long i = 0; i < Centers.Count; i++)
    //        {
    //            GridInfo1D gi = new(n: nTarget, spacing: sdTarget,
    //                refPoint: Centers[i], refType: GridRefType.Center);
    //            Grid1DCplxData di = Unit.TakeFrom(fIn, x0: Centers[i],
    //                gTarget: gi, intrpl: intrpl, bound: bound, loopMode: loopMode);
    //            ds.Add(di);
    //        }
    //        return ds;
    //    }

    //    #endregion
    //    #region ---- data => func ----

    //    /// <summary>
    //    /// takes each segment out from a given input distribution (as GridData)
    //    /// </summary>
    //    /// <param name="dIn"> input data distribution </param>
    //    /// <returns> list of segmented distributions (as function) </returns>
    //    public List<Func<double, double>> TakeEachFrom(Grid1DRealData dIn)
    //    {
    //        List<Func<double, double>> fs = [];
    //        for (int i = 0; i < Centers.Count; i++)
    //        { fs.Add(Unit.TakeFrom(dIn, x0: Centers[i])); }
    //        return fs;
    //    }

    //    /// <summary>
    //    /// takes each segment out from a given input distribution (as GridData)
    //    /// </summary>
    //    /// <param name="dIn"> input data distribution </param>
    //    /// <returns> list of segmented distributions (as function) </returns>
    //    public List<Func<double, Complex>> TakeEachFrom(Grid1DCplxData dIn)
    //    {
    //        List<Func<double, Complex>> fs = [];
    //        for (int i = 0; i < Centers.Count; i++)
    //        { fs.Add(Unit.TakeFrom(dIn, x0: Centers[i])); }
    //        return fs;
    //    }

    //    #endregion
    //    #region ---- data => data ----

    //    /// <summary>
    //    /// takes each segment out from a given input distribution (as GridData)
    //    /// </summary>
    //    /// <param name="dIn"> input data distribution </param>
    //    /// <param name="nTarget"> target number of samples for each segment </param>
    //    /// <param name="sdTarget"> target sampling distance for each segment </param>
    //    /// <param name="intrpl"> interpolation method for the segment </param>
    //    /// <param name="bound"> boundary option for the segment </param>
    //    /// <param name="loopMode"> loop-computational mode options </param>
    //    /// <returns> list of segmented distributions </returns>
    //    public List<Grid1DRealData> TakeEachFrom(Grid1DRealData dIn,
    //        long nTarget, double sdTarget,
    //        InterpolationMethod? intrpl = null,
    //        DataBoundary? bound = null,
    //        LoopMode loopMode = Defaults.LoopOption)
    //    {
    //        List<Grid1DRealData> ds = [];
    //        for (int i = 0; i < Centers.Count; i++)
    //        {
    //            GridInfo1D gi = new(n: nTarget, spacing: sdTarget,
    //                refPoint: Centers[i], refType: GridRefType.Center);
    //            Grid1DRealData di = Unit.TakeFrom(dIn, gTarget: gi,
    //                intrpl: intrpl, bound: bound, loopMode: loopMode);
    //            ds.Add(di);
    //        }
    //        return ds;
    //    }

    //    /// <summary>
    //    /// takes each segment out from a given input distribution (as GridData)
    //    /// </summary>
    //    /// <param name="dIn"> input data distribution </param>
    //    /// <param name="nTarget"> target number of samples for each segment </param>
    //    /// <param name="sdTarget"> target sampling distance for each segment </param>
    //    /// <param name="intrpl"> interpolation method for the segment </param>
    //    /// <param name="bound"> boundary option for the segment </param>
    //    /// <param name="loopMode"> loop-computational mode options </param>
    //    /// <returns> list of segmented distributions </returns>
    //    public List<Grid1DCplxData> TakeEachFrom(Grid1DCplxData dIn,
    //        long nTarget, double sdTarget,
    //        InterpolationMethod? intrpl = null,
    //        DataBoundary? bound = null,
    //        LoopMode loopMode = Defaults.LoopOption)
    //    {
    //        List<Grid1DCplxData> ds = [];
    //        for (int i = 0; i < Centers.Count; i++)
    //        {
    //            GridInfo1D gi = new(n: nTarget, spacing: sdTarget,
    //                refPoint: Centers[i], refType: GridRefType.Center);
    //            Grid1DCplxData di = Unit.TakeFrom(dIn, gTarget: gi,
    //                intrpl: intrpl, bound: bound, loopMode: loopMode);
    //            ds.Add(di);
    //        }
    //        return ds;
    //    }

    //    #endregion

    //    #endregion
    //    #region derived sub-classes

    //    /// <summary>
    //    /// uniform segmentation using rectangular-profile segments
    //    /// </summary>
    //    public class CosRect : UniformSEG1D
    //    {
    //        #region constructor

    //        /// <summary>
    //        /// uniform segmentation using rectangular-profile segments
    //        /// with optional cosine-edged profile
    //        /// </summary>
    //        /// <param name="centers"> centers of segments </param>
    //        /// <param name="diameter"> diameter of each segment </param>
    //        /// <param name="edge"> edge width of each segment </param>
    //        public CosRect(GridInfo1D centers,
    //            double diameter, 
    //            double edge = 0.0) : base(centers)
    //        {
    //            Unit = new SEG1D.CosRect(diameter: diameter, edge: edge);
    //        }

    //        #endregion
    //    }

    //    /// <summary>
    //    /// uniform segmentation using Gaussian-profile segments
    //    /// </summary>
    //    public class Gaussian : UniformSEG1D
    //    {
    //        #region constructor

    //        /// <summary>
    //        /// uniform segmentation using Gaussian-profile segments
    //        /// </summary>
    //        /// <param name="centers"> centers of segments </param>
    //        /// <param name="diameter"> diameter of each segment </param>
    //        public Gaussian(GridInfo1D centers,
    //            double diameter) : base(centers)
    //        {
    //            Unit = new SEG1D.Gaussian(diameter: diameter);
    //        }

    //        #endregion
    //    }

    //    #endregion
    //}






}
