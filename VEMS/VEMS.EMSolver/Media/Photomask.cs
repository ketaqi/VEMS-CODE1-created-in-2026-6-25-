using System.Numerics;
using VEMS.MathCore;

namespace VEMS.EMSolver
{

    /// <summary>
    /// The Mask class represents a 2D medium layer with a mask defined by polygons.
    /// It inherits from the Layer2DMedium class and provides functionality to initialize
    /// the refractive index and permittivity based on the given material structure and basis.
    /// </summary>
    public class Photomask : Layer2DMedium
    {
        #region properties

        #endregion
        #region constructors

        /// <summary>
        /// default constructor
        /// </summary>
        public Photomask() { }

        /// <summary>
        /// constructs a photomask with a list of polygon patterns
        /// </summary>
        /// <param name="patterns"> list of polygons that define the patterns </param>
        /// <param name="opaque"> opaque material that fills within the polygon patterns </param>
        /// <param name="transparent"> transparent material that is outside the polygon patterns  </param>
        /// <param name="isInnerOpaque"> whether the inner region is opaque </param>
        /// <exception cref="ArgumentException"></exception>
        public Photomask(List<Geometry2D.Polygon> patterns,
            Material opaque, Material transparent,
            bool isInnerOpaque = true)
        {
            // exception handling
            if (patterns.Count == 0)
            { throw new ArgumentException("The photomask must contain at least one polygon pattern"); }

            // initialize the refractive index and permittivity
            N = (w, x, y) =>
            {
                Complex nInner = isInnerOpaque ? opaque.N(w) : transparent.N(w);
                Complex nOuter = isInnerOpaque ? transparent.N(w) : opaque.N(w);
                return Geometry2D.Polygon.FillValue(x, y, patterns, nInner, nOuter);
            };
            Epsilon = (w, x, y) => N(w, x, y) * N(w, x, y);
        }

        #endregion
        #region static methods

        ///// <summary>
        ///// Create the polygon patterns.
        ///// </summary>
        ///// <param name="cy">the y-coordinates of all grid points.</param>
        ///// <param name="cx">the x-coordinates of all grid points.</param>
        ///// <param name="widthY"> The width of the polygon patterns in the y-direction.</param>
        ///// <param name="widthX"> The width of the polygon patterns in the x-direction.</param>
        //public static List<Geometry2D.Polygon> PolyPatts(VectorD cy, VectorD cx, double widthY, double widthX)
        //{
        //    List<Geometry2D.Polygon> patterns = new();
        //    for (int i = 0; i < cx.Count; i++)
        //    {
        //        Geometry2D.Polygon pi = new(4);
        //        pi.Vx = new VectorD(pi.N)
        //        {
        //            [0] = cx[i] - 0.5 * widthX,
        //            [1] = cx[i] + 0.5 * widthX,
        //            [2] = cx[i] + 0.5 * widthX,
        //            [3] = cx[i] - 0.5 * widthX
        //        };
        //        pi.Vy = new VectorD(pi.N)
        //        {
        //            [0] = cy[i] - 0.5 * widthY,
        //            [1] = cy[i] - 0.5 * widthY,
        //            [2] = cy[i] + 0.5 * widthY,
        //            [3] = cy[i] + 0.5 * widthY
        //        };
        //        patterns.Add(pi);
        //    }
        //    return patterns;
        //}

        #endregion
        #region initialization

        ///// <summary>
        ///// initializes the refractive index distribution of the photomask
        ///// </summary>
        ///// <param name="wavelength"> wavelength in vacuum </param>
        ///// <param name="x"> coordinate along x-direction </param>
        ///// <param name="y"> coordinate along y-direction </param>
        ///// <param name="patterns"> list of polygons that define the patterns </param>
        ///// <param name="opaque"> opaque material that fills wihtin the polygon patterns </param>
        ///// <param name="transparent"> transparent material that is outside the polygon patterns  </param>
        ///// <returns> complex refractive index at a given (x, y) location </returns>
        //private static Complex InitRefractiveIndex(double wavelength, double x, double y,
        //    List<Geometry2D.Polygon> patterns, Material opaque, Material transparent)
        //{
        //    // gets the values of refractive indices
        //    Complex nOpaque = opaque.N(wavelength);
        //    Complex nTransparent = transparent.N(wavelength);
        //    // checks if the point is inside the polygon patterns
        //    return Geometry2D.Polygon.FillValue(x, y, patterns, nOpaque, nTransparent);
        //}

        #endregion
        #region derived sub classes

        /// <summary>  
        /// represents a photomask with multiple uniform patterns.  
        /// this class is a specialized type of <see cref="Photomask"/> that generates a grid of patterns
        /// based on the specified number of rows, columns, spacing, and dimensions.  
        /// </summary>  
        /// <remarks>  
        /// the refractive index and permittivity of the photomask are initialized based on the provided opaque  
        /// and transparent materials, as well as the specified inner region configuration.  
        /// </remarks>  
        public class Uniform()
        {

            //   /// <summary>
            //   /// represents a photomask with multiple rectangular bar patterns.
            //   /// </summary>
            //public class MultiBars : Photomask
            //{
            //    /// <param name="rows"> number of rows of bars </param>
            //    /// <param name="cols"> number of columns of bars </param>
            //    /// <param name="spacingY"> vertical spacing between bars </param>
            //    /// <param name="spacingX"> horizontal spacing between bars </param>
            //    /// <param name="widthY"> height of each bar </param>
            //    /// <param name="widthX"> width of each bar </param>
            //    /// <param name="opaque"> opaque material that fills within the bars </param>
            //    /// <param name="transparent"> transparent material that is outside the bars </param>
            //    /// <param name="isInnerOpaque"> indicates whether the inner region is opaque </param>
            //    public MultiBars(long rows, long cols, 
            //        double spacingY, double spacingX, 
            //        VectorD widthY, VectorD widthX,
            //        Material opaque, Material transparent,
            //        bool isInnerOpaque = true)
            //    {
            //        // derive the center's coordinate of each bar
            //        GridInfo2D centers = new(rows, cols, spacingY, spacingX);
            //        //(VectorD cy, VectorD cx) = centers.GetGrid();
            //        (VectorD cy, VectorD cx) = centers.GetAllCoordinates();

            //        // create the polygon patterns
            //        //List<Geometry2D.Polygon> patterns = PolyPatts(cy, cx, widthY, widthX);
            //        List<Geometry2D.Polygon> patterns = Geometry2D.Polygon.
            //           CreateRectPolygons(cy, cx, widthY, widthX);


            //        // initialize the refractive index and permittivity
            //        N = (w, x, y) =>
            //        {
            //            Complex nInner = isInnerOpaque ? opaque.N(w) : transparent.N(w);
            //            Complex nOuter = isInnerOpaque ? transparent.N(w) : opaque.N(w);
            //            return Geometry2D.Polygon.FillValue(x, y, patterns, nInner, nOuter);
            //        };
            //        Epsilon = (w, x, y) => N(w, x, y) * N(w, x, y);
            //    }
            //}


            /// <summary>
            /// represents a photomask with multiple rectangular bar patterns.
            /// </summary>
            public class MultiBars : Photomask
            {
                /// <param name="rows"> number of rows of bars.</param>
                /// <param name="cols"> number of columns of bars.</param>
                /// <param name="spacingY"> vertical spacing between bars.</param>
                /// <param name="spacingX"> horizontal spacing between bars.</param>
                /// <param name="widthY"> The height of each bar (in the y-direction).</param>
                /// <param name="widthX"> The width of each bar (in the x-direction).</param>
                /// <param name="opaque"> opaque material that fills within the bars.</param>
                /// <param name="transparent"> transparent material that is outside the bars.</param>
                /// <param name="isInnerOpaque"> indicates whether the inner region is opaque.</param>
                public MultiBars(long rows, long cols,
                    double spacingY, double spacingX,
                    double widthY, double widthX,
                    Material opaque, Material transparent,
                    bool isInnerOpaque = true)
                {
                    // derive the center's coordinate of each bar
                    GridInfo2D centers = new(rows, cols, spacingY, spacingX);

                    // Use the static method to generate polygons
                    List<Geometry2D.Polygon> patterns = GridPolygons.
                        Uniform.MultiBars(centers, widthY, widthX);

                    // initialize the refractive index and permittivity
                    N = (w, x, y) =>
                    {
                        Complex nInner = isInnerOpaque ? opaque.N(w) : transparent.N(w);
                        Complex nOuter = isInnerOpaque ? transparent.N(w) : opaque.N(w);
                        return Geometry2D.Polygon.FillValue(x, y, patterns, nInner, nOuter);
                    };
                    Epsilon = (w, x, y) => N(w, x, y) * N(w, x, y);
                }
            }


            ///// <summary>  
            ///// represents a photomask with multiple contact square hole patterns.  
            ///// </summary>  
            //public class MultiSquaHoles : Photomask
            //{   
            //    /// <param name="rows"> number of rows of contact square holes </param>  
            //    /// <param name="cols"> number of columns of contact square holes </param>  
            //    /// <param name="spacingY"> vertical spacing between square holes </param>  
            //    /// <param name="spacingX"> horizontal spacing between square holes </param>  
            //    /// <param name="width"> width of each square hole </param>  
            //    /// <param name="opaque"> opaque material that fills within the square holes </param>  
            //    /// <param name="transparent"> transparent material that is outside the square holes </param>  
            //    /// <param name="isInnerOpaque"> indicates whether the inner region is opaque </param>  
            //    public MultiSquaHoles(long rows, long cols, double spacingY, double spacingX, double width,
            //        Material opaque, Material transparent,
            //            bool isInnerOpaque = true)
            //    {
            //        // Derive the center's coordinate of each contact hole  
            //        GridInfo2D centers = new(rows, cols, spacingY, spacingX);
            //        (VectorD cy, VectorD cx) = centers.GetGrid();

            //        // Create the polygon patterns  
            //        List<Geometry2D.Polygon> patterns = PolyPatts(cy, cx, width, width);
            //        // Initialize the refractive index and permittivity  
            //        N = (w, x, y) =>
            //        {
            //            Complex nInner = isInnerOpaque ? opaque.N(w) : transparent.N(w);
            //            Complex nOuter = isInnerOpaque ? transparent.N(w) : opaque.N(w);
            //            return Geometry2D.Polygon.FillValue(x, y, patterns, nInner, nOuter);
            //        };
            //        Epsilon = (w, x, y) => N(w, x, y) * N(w, x, y);
            //    }
            //}


            /// <summary>  
            /// represents a photomask with multiple contact square hole patterns.  
            /// </summary>  
            public class MultiSquareHoles : Photomask
            {
                /// <param name="rows"> number of rows of contact square holes.</param>  
                /// <param name="cols"> number of columns of contact square holes.</param>  
                /// <param name="spacingY"> vertical spacing between square holes.</param>  
                /// <param name="spacingX"> horizontal spacing between square holes.</param>  
                /// <param name="diameter"> The diameter of each square hole.</param>  
                /// <param name="opaque"> opaque material that fills within the square holes.</param>  
                /// <param name="transparent"> transparent material that is outside the square holes.</param>  
                /// <param name="isInnerOpaque"> indicates whether the inner region is opaque.</param>  
                public MultiSquareHoles(long rows, long cols,
                    double spacingY, double spacingX,
                    double diameter,
                    Material opaque, Material transparent,
                        bool isInnerOpaque = true)
                {
                    // Derive the center's coordinate of each contact hole  
                    GridInfo2D centers = new(rows, cols, spacingY, spacingX);
                    //(VectorD cy, VectorD cx) = centers.GetAllCoordinates();

                    //// Create the polygon patterns  
                    //List<Geometry2D.Polygon> patterns = Geometry2D.Polygon.
                    //    CreateRectPolygons(cy, cx, width, width);

                    // Use the static method to generate polygons
                    List<Geometry2D.Polygon> patterns = GridPolygons.
                        Uniform.MultiSquareHoles(centers, diameter);

                    // Initialize the refractive index and permittivity  
                    N = (w, x, y) =>
                    {
                        Complex nInner = isInnerOpaque ? opaque.N(w) : transparent.N(w);
                        Complex nOuter = isInnerOpaque ? transparent.N(w) : opaque.N(w);
                        return Geometry2D.Polygon.FillValue(x, y, patterns, nInner, nOuter);
                    };
                    Epsilon = (w, x, y) => N(w, x, y) * N(w, x, y);
                }
            }


            ///// <summary>  
            ///// represents a photomask with multiple contact circle hole patterns.  
            ///// </summary>  
            //public class MultiCirHoles : Photomask
            //{
            //    /// <param name="rows"> number of rows of contact circle holes </param>  
            //    /// <param name="cols"> number of columns of contact circle holes </param>  
            //    /// <param name="spacingY"> vertical spacing between circle holes </param>  
            //    /// <param name="spacingX"> horizontal spacing between circle holes </param>  
            //    /// <param name="radius"> radius of each circle hole </param>  
            //    /// <param name="opaque"> opaque material that fills within the circle holes </param>  
            //    /// <param name="transparent"> transparent material that is outside the circle holes </param>  
            //    /// <param name="isInnerOpaque"> indicates whether the inner region is opaque </param> 
            //    public MultiCirHoles(long rows, long cols, double spacingY, double spacingX, double radius,
            //        Material opaque, Material transparent,
            //            bool isInnerOpaque = true)
            //    {
            //        // Derive the center's coordinate of each contact hole  
            //        GridInfo2D centers = new(rows, cols, spacingY, spacingX);
            //        (VectorD cy, VectorD cx) = centers.GetGrid();

            //        // Create the polygon patterns  
            //        List<Geometry2D.Polygon> patterns = new();
            //        int numSegments = 37; // Number of vertices to approximate the circle

            //        for (int i = 0; i < cx.Count; i++)
            //        {
            //            Geometry2D.Polygon pi = new(numSegments);
            //            VectorD vx = new VectorD(numSegments);
            //            VectorD vy = new VectorD(numSegments);

            //            for (int k = 0; k < numSegments; k++)
            //            {
            //                double theta = 2.0 * Math.PI * k / numSegments;
            //                vx[k] = cx[i] + radius * Math.Cos(theta);
            //                vy[k] = cy[i] + radius * Math.Sin(theta);
            //            }

            //            pi.Vx = vx;
            //            pi.Vy = vy;
            //            patterns.Add(pi);
            //        }
            //        // Initialize the refractive index and permittivity  
            //        N = (w, x, y) =>
            //        {
            //            Complex nInner = isInnerOpaque ? opaque.N(w) : transparent.N(w);
            //            Complex nOuter = isInnerOpaque ? transparent.N(w) : opaque.N(w);
            //            return Geometry2D.Polygon.FillValue(x, y, patterns, nInner, nOuter);
            //        };
            //        Epsilon = (w, x, y) => N(w, x, y) * N(w, x, y);
            //    }
            //}


            /// <summary>  
            /// represents a photomask with multiple contact circle hole patterns.  
            /// </summary>  
            public class MultiCircHoles : Photomask
            {
                /// <param name="rows"> number of rows of contact circle holes.</param>  
                /// <param name="cols"> number of columns of contact circle holes.</param>  
                /// <param name="spacingY"> vertical spacing between circle holes.</param>  
                /// <param name="spacingX"> horizontal spacing between circle holes.</param>  
                /// <param name="radius"> radius of each circle hole.</param>  
                /// <param name="opaque"> opaque material that fills within the circle holes.</param>  
                /// <param name="transparent"> transparent material that is outside the circle holes.</param>  
                /// <param name="isInnerOpaque"> indicates whether the inner region is opaque.</param> 
                /// <param name="numSegments">The number of segments (vertices) to use for each circle polygon. Default is 37.</param>
                public MultiCircHoles(long rows, long cols,
                    double spacingY, double spacingX,
                    double radius,
                    Material opaque, Material transparent,
                    bool isInnerOpaque = true,
                    int numSegments = 37)
                {
                    // Derive the center's coordinate of each contact hole  
                    GridInfo2D centers = new(rows, cols, spacingY, spacingX);
                    //(VectorD cy, VectorD cx) = centers.GetGrid();

                    //// Create the polygon patterns  
                    //List<Geometry2D.Polygon> patterns = Geometry2D.Polygon.
                    //    CreateCircPolygons(cy, cx, radius, numSegments);

                    // Use the static method to generate polygons
                    List<Geometry2D.Polygon> patterns = GridPolygons.
                        Uniform.MultiCircHoles(centers, radius, numSegments);

                    // Initialize the refractive index and permittivity  
                    N = (w, x, y) =>
                    {
                        Complex nInner = isInnerOpaque ? opaque.N(w) : transparent.N(w);
                        Complex nOuter = isInnerOpaque ? transparent.N(w) : opaque.N(w);
                        return Geometry2D.Polygon.FillValue(x, y, patterns, nInner, nOuter);
                    };
                    Epsilon = (w, x, y) => N(w, x, y) * N(w, x, y);
                }
            }

        }


        /// <summary>  
        /// represents a photomask with multiple nonuniform patterns.  
        /// this class is a specialized type of <see cref="Photomask"/> that generates a grid of patterns
        /// based on the specified number of rows, columns, spacing, and dimensions.  
        /// </summary>  
        /// <remarks>  
        /// the refractive index and permittivity of the photomask are initialized based on the provided opaque  
        /// and transparent materials, as well as the specified inner region configuration.  
        /// </remarks>  
        public class NonUniform()
        {
            /// <summary>
            /// represents a photomask with multiple rectangular bar patterns.
            /// </summary>
            public class MultiBars : Photomask
            {
                /// <param name="rows"> number of rows of bars.</param>
                /// <param name="cols"> number of columns of bars.</param>
                /// <param name="spacingY"> vertical spacing between bars.</param>
                /// <param name="spacingX"> horizontal spacing between bars.</param>
                /// <param name="widthY"> The height of each bar (in the y-direction).</param>
                /// <param name="widthXMin"> The minimum width of bar (in the x-direction).</param>
                /// <param name="widthXMax"> The maximum width of bar (in the x-direction).</param>
                /// <param name="opaque"> opaque material that fills within the bars.</param>
                /// <param name="transparent"> transparent material that is outside the bars.</param>
                /// <param name="isInnerOpaque"> indicates whether the inner region is opaque.</param>
                public MultiBars(long rows, long cols, double spacingY, double spacingX,
                       double widthY, double widthXMin, double widthXMax,
                       Material opaque, Material transparent,
                       bool isInnerOpaque = true)
                {
                    // derive the center's coordinate of each bar
                    GridInfo2D centers = new(rows, cols, spacingY, spacingX);

                    // Use the static method to generate polygons
                    List<Geometry2D.Polygon> patterns = GridPolygons.
                        NonUniform.MultiBars(centers, widthY, widthXMin, widthXMax);

                    // initialize the refractive index and permittivity
                    N = (w, x, y) =>
                    {
                        Complex nInner = isInnerOpaque ? opaque.N(w) : transparent.N(w);
                        Complex nOuter = isInnerOpaque ? transparent.N(w) : opaque.N(w);
                        return Geometry2D.Polygon.FillValue(x, y, patterns, nInner, nOuter);
                    };
                    Epsilon = (w, x, y) => N(w, x, y) * N(w, x, y);
                }
            }


            /// <summary>  
            /// represents a photomask with multiple contact square hole patterns.  
            /// </summary>  
            public class MultiSquareHoles : Photomask
            {
                /// <param name="rows"> number of rows of contact square holes.</param>  
                /// <param name="cols"> number of columns of contact square holes.</param>  
                /// <param name="spacingY"> vertical spacing between square holes.</param>  
                /// <param name="spacingX"> horizontal spacing between square holes.</param>  
                /// <param name="diameterMin">The minimum diameter of square hole.</param>  
                /// <param name="diameterMax">The maximum diameter of square hole.</param>
                /// <param name="opaque"> opaque material that fills within the square holes.</param>  
                /// <param name="transparent"> transparent material that is outside the square holes.</param>  
                /// <param name="isInnerOpaque"> indicates whether the inner region is opaque.</param>  
                public MultiSquareHoles(long rows, long cols, double spacingY, double spacingX,
                    double diameterMin, double diameterMax,
                    Material opaque, Material transparent,
                        bool isInnerOpaque = true)
                {
                    // Derive the center's coordinate of each contact hole  
                    GridInfo2D centers = new(rows, cols, spacingY, spacingX);

                    // Use the static method to generate polygons
                    List<Geometry2D.Polygon> patterns = GridPolygons.
                        NonUniform.MultiSquareHoles(centers, diameterMin, diameterMax);

                    // Initialize the refractive index and permittivity  
                    N = (w, x, y) =>
                    {
                        Complex nInner = isInnerOpaque ? opaque.N(w) : transparent.N(w);
                        Complex nOuter = isInnerOpaque ? transparent.N(w) : opaque.N(w);
                        return Geometry2D.Polygon.FillValue(x, y, patterns, nInner, nOuter);
                    };
                    Epsilon = (w, x, y) => N(w, x, y) * N(w, x, y);
                }
            }


            /// <summary>  
            /// represents a photomask with multiple contact circle hole patterns.  
            /// </summary>  
            public class MultiCircHoles : Photomask
            {
                /// <param name="rows"> number of rows of contact circle holes.</param>  
                /// <param name="cols"> number of columns of contact circle holes.</param>  
                /// <param name="spacingY"> vertical spacing between circle holes.</param>  
                /// <param name="spacingX"> horizontal spacing between circle holes.</param>  
                /// <param name="radiusMin">The minimum radius of circular hole.</param>
                /// <param name="radiusMax">The maximum radius of circular hole.</param>
                /// <param name="opaque"> opaque material that fills within the circle holes.</param>  
                /// <param name="transparent"> transparent material that is outside the circle holes.</param>  
                /// <param name="isInnerOpaque"> indicates whether the inner region is opaque.</param> 
                /// <param name="numSegments">The number of segments (vertices) to use for each circle polygon. Default is 37.</param>
                public MultiCircHoles(long rows, long cols, double spacingY, double spacingX,
                    double radiusMin, double radiusMax,
                    Material opaque, Material transparent,
                    bool isInnerOpaque = true,
                    int numSegments = 37)
                {
                    // Derive the center's coordinate of each contact hole  
                    GridInfo2D centers = new(rows, cols, spacingY, spacingX);

                    // Use the static method to generate polygons
                    List<Geometry2D.Polygon> patterns = GridPolygons.
                        NonUniform.MultiCircHoles(centers, radiusMin, radiusMax, numSegments);

                    // Initialize the refractive index and permittivity  
                    N = (w, x, y) =>
                    {
                        Complex nInner = isInnerOpaque ? opaque.N(w) : transparent.N(w);
                        Complex nOuter = isInnerOpaque ? transparent.N(w) : opaque.N(w);
                        return Geometry2D.Polygon.FillValue(x, y, patterns, nInner, nOuter);
                    };
                    Epsilon = (w, x, y) => N(w, x, y) * N(w, x, y);
                }
            }


            /// <summary>  
            /// represents a photomask rectangle with different shape patterns.  
            /// </summary>  
            public class MultiRects : Photomask
            {
                /// <param name="rows"> number of rows of patterns.</param>  
                /// <param name="cols"> number of columns of patterns.</param>  
                /// <param name="spacingY"> vertical spacing between patterns.</param>  
                /// <param name="spacingX"> horizontal spacing between patterns.</param>  
                /// <param name="widthMin"> The minimum width of patterns (in the x-direction).</param>
                /// <param name="widthMax"> The maximum width of patterns (in the x-direction).</param>
                /// <param name="opaque"> opaque material that fills within the patterns.</param>  
                /// <param name="transparent"> transparent material that is outside the patterns.</param>  
                /// <param name="isInnerOpaque"> indicates whether the inner region is opaque.</param> 
                public MultiRects(long rows, long cols, double spacingY, double spacingX,
                    double widthMin, double widthMax,
                    Material opaque, Material transparent,
                    bool isInnerOpaque = true)
                {
                    // Derive the center's coordinate of each rectangle 
                    GridInfo2D centers = new(rows, cols, spacingY, spacingX);

                    // Use the static method to generate polygons
                    List<Geometry2D.Polygon> patterns = GridPolygons.
                        NonUniform.MultiRects(centers, widthMin, widthMax);

                    // Initialize the refractive index and permittivity  
                    N = (w, x, y) =>
                    {
                        Complex nInner = isInnerOpaque ? opaque.N(w) : transparent.N(w);
                        Complex nOuter = isInnerOpaque ? transparent.N(w) : opaque.N(w);
                        return Geometry2D.Polygon.FillValue(x, y, patterns, nInner, nOuter);
                    };
                    Epsilon = (w, x, y) => N(w, x, y) * N(w, x, y);
                }
            }


            /// <summary>  
            /// represents a photomask with contact square hole near to bar patterns.  
            /// </summary>  
            public class MultiHoleBars : Photomask
            {
                /// <param name="rows"> number of rows of patterns.</param>  
                /// <param name="cols"> number of columns of patterns.</param>  
                /// <param name="spacingY"> vertical spacing between patterns.</param>  
                /// <param name="spacingX"> horizontal spacing between patterns.</param>  
                /// <param name="widthHole"> The width of the contact square hole.</param>
                /// <param name="widthBarY"> The y-width of the bar.</param>
                /// <param name="widthBarX"> The x-width of the bar.</param>
                /// <param name="opaque"> opaque material that fills within the patterns.</param>  
                /// <param name="transparent"> transparent material that is outside the patterns.</param>  
                /// <param name="isInnerOpaque"> indicates whether the inner region is opaque.</param> 
                public MultiHoleBars(long rows, long cols, double spacingY, double spacingX,
                    double widthHole, double widthBarY, double widthBarX,
                    Material opaque, Material transparent,
                    bool isInnerOpaque = true)
                {
                    // Derive the center's coordinate of each pattern
                    GridInfo2D centers = new(rows, cols, spacingY, spacingX);

                    // Use the static method to generate polygons
                    List<Geometry2D.Polygon> patterns = GridPolygons.
                        NonUniform.MultiHoleBars(centers, widthHole, widthBarY, widthBarX);

                    // Initialize the refractive index and permittivity  
                    N = (w, x, y) =>
                    {
                        Complex nInner = isInnerOpaque ? opaque.N(w) : transparent.N(w);
                        Complex nOuter = isInnerOpaque ? transparent.N(w) : opaque.N(w);
                        return Geometry2D.Polygon.FillValue(x, y, patterns, nInner, nOuter);
                    };
                    Epsilon = (w, x, y) => N(w, x, y) * N(w, x, y);
                }
            }


            /// <summary>  
            /// represents a photomask with crossed bar in near contact bar patterns.  
            /// </summary>  
            public class MultiCrossBars : Photomask
            {
                /// <param name="rows"> number of rows of patterns.</param>  
                /// <param name="cols"> number of columns of patterns.</param>  
                /// <param name="spacingY"> vertical spacing between patterns.</param>  
                /// <param name="spacingX"> horizontal spacing between patterns.</param>  
                /// <param name="widthBarY"> The y-width of the bar.</param>
                /// <param name="widthBarX"> The x-width of the bar.</param>
                /// <param name="widthCrossBarY"> The y-width of the crossed bar.</param>
                /// <param name="widthCrossBarX"> The x-width of the crossed bar.</param>
                /// <param name="opaque"> opaque material that fills within the patterns.</param>  
                /// <param name="transparent"> transparent material that is outside the patterns.</param>  
                /// <param name="isInnerOpaque"> indicates whether the inner region is opaque.</param> 
                public MultiCrossBars(long rows, long cols, double spacingY, double spacingX,
                    double widthBarY, double widthBarX, double widthCrossBarY, double widthCrossBarX,
                    Material opaque, Material transparent,
                    bool isInnerOpaque = true)
                {
                    // Derive the center's coordinate of each pattern 
                    GridInfo2D centers = new(rows, cols, spacingY, spacingX);

                    // Use the static method to generate polygons
                    List<Geometry2D.Polygon> patterns = GridPolygons.
                        NonUniform.MultiCrossBars(centers, widthBarY, widthBarX, widthCrossBarY, widthCrossBarX);

                    // Initialize the refractive index and permittivity  
                    N = (w, x, y) =>
                    {
                        Complex nInner = isInnerOpaque ? opaque.N(w) : transparent.N(w);
                        Complex nOuter = isInnerOpaque ? transparent.N(w) : opaque.N(w);
                        return Geometry2D.Polygon.FillValue(x, y, patterns, nInner, nOuter);
                    };
                    Epsilon = (w, x, y) => N(w, x, y) * N(w, x, y);
                }
            }


            /// <summary>  
            /// represents a photomask with different corner structures patterns.  
            /// </summary>  
            public class MultiCorners : Photomask
            {
                /// <param name="number"> number of corner patterns.</param>       
                /// <param name="widthX"> The minimum width of corner pattern.</param>
                /// <param name="spacing"> spacing between patterns.</param>  
                /// <param name="opaque"> opaque material that fills within the patterns.</param>  
                /// <param name="transparent"> transparent material that is outside the patterns.</param>  
                /// <param name="isInnerOpaque"> indicates whether the inner region is opaque.</param> 
                public MultiCorners(long number, double widthX, double spacing,
                    Material opaque, Material transparent,
                    bool isInnerOpaque = true)
                {
                    // Derive the center's coordinate of each pattern
                    GridInfo2D centers = new(number, number, spacing, spacing);

                    // Use the static method to generate polygons
                    List<Geometry2D.Polygon> patterns = GridPolygons.
                        NonUniform.MultiCorners(centers, widthX, spacing);

                    // Initialize the refractive index and permittivity  
                    N = (w, x, y) =>
                    {
                        Complex nInner = isInnerOpaque ? opaque.N(w) : transparent.N(w);
                        Complex nOuter = isInnerOpaque ? transparent.N(w) : opaque.N(w);
                        return Geometry2D.Polygon.FillValue(x, y, patterns, nInner, nOuter);
                    };
                    Epsilon = (w, x, y) => N(w, x, y) * N(w, x, y);
                }
            }

            ///// <summary>
            ///// represents a photomask with multiple rectangular bar patterns.
            ///// </summary>
            //public class MultiBars : Photomask
            //{
            //    /// <param name="rows"> number of rows of bars </param>
            //    /// <param name="cols"> number of columns of bars </param>
            //    /// <param name="spacingY"> vertical spacing between bars </param>
            //    /// <param name="spacingX"> horizontal spacing between bars </param>
            //    /// <param name="widthY"> height of each bar </param>
            //    /// <param name="widthX1"> width of the narrowest bar </param>
            //    /// <param name="widthX2"> width of the widest bar </param>
            //    /// <param name="opaque"> opaque material that fills within the bars </param>
            //    /// <param name="transparent"> transparent material that is outside the bars </param>
            //    /// <param name="isInnerOpaque"> indicates whether the inner region is opaque </param>
            //    public MultiBars(long rows, long cols, double spacingY, double spacingX,
            //           double widthY, double widthX1, double widthX2,
            //           Material opaque, Material transparent,
            //           bool isInnerOpaque = true)
            //    {
            //        // derive the center's coordinate of each bar
            //        GridInfo2D centers = new(rows, cols, spacingY, spacingX);
            //        (VectorD cy, VectorD cx) = centers.GetGrid();

            //        // create the polygon patterns
            //        List<Geometry2D.Polygon> patterns = new();
            //        int Num = (int)(cx.Count / rows);

            //        for (int i = 0; i < cx.Count; i++)
            //        {
            //            // determine the index within the current group (take the remainder)
            //            int Numi = i % Num;
            //            // interpolate widthX within the group
            //            double widthX = (Num == 1)
            //                ? widthX1
            //                : widthX1 + (widthX2 - widthX1) * Numi / (Num - 1);

            //            Geometry2D.Polygon pi = new(4);
            //            pi.Vx = new VectorD(pi.N)
            //            {
            //                [0] = cx[i] - 0.5 * widthX,
            //                [1] = cx[i] + 0.5 * widthX,
            //                [2] = cx[i] + 0.5 * widthX,
            //                [3] = cx[i] - 0.5 * widthX
            //            };
            //            pi.Vy = new VectorD(pi.N)
            //            {
            //                [0] = cy[i] - 0.5 * widthY,
            //                [1] = cy[i] - 0.5 * widthY,
            //                [2] = cy[i] + 0.5 * widthY,
            //                [3] = cy[i] + 0.5 * widthY
            //            };
            //            patterns.Add(pi);
            //        }
            //        // initialize the refractive index and permittivity
            //        N = (w, x, y) =>
            //        {
            //            Complex nInner = isInnerOpaque ? opaque.N(w) : transparent.N(w);
            //            Complex nOuter = isInnerOpaque ? transparent.N(w) : opaque.N(w);
            //            return Geometry2D.Polygon.FillValue(x, y, patterns, nInner, nOuter);
            //        };
            //        Epsilon = (w, x, y) => N(w, x, y) * N(w, x, y);
            //    }
            //}


            ///// <summary>  
            ///// represents a photomask with multiple contact square hole patterns.  
            ///// </summary>  
            //public class MultiSquaHoles : Photomask
            //{  
            //    /// <param name="rows"> number of rows of contact square holes </param>  
            //    /// <param name="cols"> number of columns of contact square holes </param>  
            //    /// <param name="spacingY"> vertical spacing between square holes </param>  
            //    /// <param name="spacingX"> horizontal spacing between square holes </param>  
            //    /// <param name="width1"> width of the smallest square holes </param>  
            //    /// <param name="width2"> width of the largest square holes </param> 
            //    /// <param name="opaque"> opaque material that fills within the square holes </param>  
            //    /// <param name="transparent"> transparent material that is outside the square holes </param>  
            //    /// <param name="isInnerOpaque"> indicates whether the inner region is opaque </param>  
            //    public MultiSquaHoles(long rows, long cols, double spacingY, double spacingX, double width1, double width2,
            //        Material opaque, Material transparent,
            //            bool isInnerOpaque = true)
            //    {
            //        // Derive the center's coordinate of each contact hole  
            //        GridInfo2D centers = new(rows, cols, spacingY, spacingX);
            //        (VectorD cy, VectorD cx) = centers.GetGrid();

            //        // Create the polygon patterns  
            //        List<Geometry2D.Polygon> patterns = new();
            //        int Num = (int)(cx.Count / rows);

            //        for (int i = 0; i < cx.Count; i++)
            //        {
            //            // determine the index within the current group (take the remainder)
            //            int Numi = i % Num;
            //            // interpolate widthX within the group
            //            double width = (Num == 1)
            //                ? width1
            //                : width1 + (width2 - width1) * Numi / (Num - 1);

            //            Geometry2D.Polygon pi = new(4);
            //            pi.Vx = new VectorD(pi.N)
            //            {
            //                [0] = cx[i] - 0.5 * width,
            //                [1] = cx[i] + 0.5 * width,
            //                [2] = cx[i] + 0.5 * width,
            //                [3] = cx[i] - 0.5 * width
            //            };
            //            pi.Vy = new VectorD(pi.N)
            //            {
            //                [0] = cy[i] - 0.5 * width,
            //                [1] = cy[i] - 0.5 * width,
            //                [2] = cy[i] + 0.5 * width,
            //                [3] = cy[i] + 0.5 * width
            //            };
            //            patterns.Add(pi);
            //        }
            //        // Initialize the refractive index and permittivity  
            //        N = (w, x, y) =>
            //        {
            //            Complex nInner = isInnerOpaque ? opaque.N(w) : transparent.N(w);
            //            Complex nOuter = isInnerOpaque ? transparent.N(w) : opaque.N(w);
            //            return Geometry2D.Polygon.FillValue(x, y, patterns, nInner, nOuter);
            //        };
            //        Epsilon = (w, x, y) => N(w, x, y) * N(w, x, y);
            //    }
            //}

            ///// <summary>  
            ///// represents a photomask with multiple contact circle hole patterns.  
            ///// </summary>  
            //public class MultiCirHoles : Photomask
            //{
            //    /// <param name="rows"> number of rows of contact circle holes </param>  
            //    /// <param name="cols"> number of columns of contact circle holes </param>  
            //    /// <param name="spacingY"> vertical spacing between circle holes </param>  
            //    /// <param name="spacingX"> horizontal spacing between circle holes </param>  
            //    /// <param name="radiusS"> radius of the smallest circle holes </param>
            //    /// <param name="radiusL"> radius of the largest circle holes </param>
            //    /// <param name="opaque"> opaque material that fills within the circle holes </param>  
            //    /// <param name="transparent"> transparent material that is outside the circle holes </param>  
            //    /// <param name="isInnerOpaque"> indicates whether the inner region is opaque </param> 
            //    public MultiCirHoles(long rows, long cols, double spacingY, double spacingX, double radiusS, double radiusL,
            //        Material opaque, Material transparent,
            //            bool isInnerOpaque = true)
            //    {
            //        // Derive the center's coordinate of each contact hole  
            //        GridInfo2D centers = new(rows, cols, spacingY, spacingX);
            //        (VectorD cy, VectorD cx) = centers.GetGrid();

            //        // Create the polygon patterns  
            //        List<Geometry2D.Polygon> patterns = new();
            //        int numSegs = 37; // Number of vertices to approximate the circle
            //        int Num = (int)(cx.Count / rows);

            //        for (int i = 0; i < cx.Count; i++)
            //        {
            //            // Determine the index within the current group
            //            int Numi = i % Num;
            //            // Linear interpolation for radius
            //            double radius = (cx.Count == 1)
            //                ? radiusS
            //                : radiusS + (radiusL - radiusS) * Numi / (Num - 1);

            //            Geometry2D.Polygon pi = new(numSegs);
            //            VectorD vx = new VectorD(numSegs);
            //            VectorD vy = new VectorD(numSegs);

            //            for (int k = 0; k < numSegs; k++)
            //            {
            //                double theta = 2.0 * Math.PI * k / numSegs;
            //                vx[k] = cx[i] + radius * Math.Cos(theta);
            //                vy[k] = cy[i] + radius * Math.Sin(theta);
            //            }

            //            pi.Vx = vx;
            //            pi.Vy = vy;
            //            patterns.Add(pi);
            //        }
            //        // Initialize the refractive index and permittivity  
            //        N = (w, x, y) =>
            //        {
            //            Complex nInner = isInnerOpaque ? opaque.N(w) : transparent.N(w);
            //            Complex nOuter = isInnerOpaque ? transparent.N(w) : opaque.N(w);
            //            return Geometry2D.Polygon.FillValue(x, y, patterns, nInner, nOuter);
            //        };
            //        Epsilon = (w, x, y) => N(w, x, y) * N(w, x, y);
            //    }
            //}

            ///// <summary>  
            ///// represents a photomask rectangle with different shape patterns.  
            ///// </summary>  
            //public class MultiRectShapes : Photomask
            //{  
            //    /// <param name="rows"> number of rows of patterns </param>  
            //    /// <param name="cols"> number of columns of patterns </param>  
            //    /// <param name="spacingY"> vertical spacing between patterns </param>  
            //    /// <param name="spacingX"> horizontal spacing between patterns </param>  
            //    /// <param name="widthS"> radius of the smallest patterns </param>
            //    /// <param name="widthL"> radius of the largest patterns </param>
            //    /// <param name="opaque"> opaque material that fills within the patterns </param>  
            //    /// <param name="transparent"> transparent material that is outside the patterns </param>  
            //    /// <param name="isInnerOpaque"> indicates whether the inner region is opaque </param> 
            //    public MultiRectShapes(long rows, long cols, double spacingY, double spacingX, double widthS, double widthL,
            //        Material opaque, Material transparent,
            //                bool isInnerOpaque = true)
            //    {
            //        // Derive the center's coordinate of each rectangle 
            //        GridInfo2D centers = new(rows, cols, spacingY, spacingX);
            //        (VectorD cy, VectorD cx) = centers.GetGrid();

            //        // create the polygon patterns
            //        List<Geometry2D.Polygon> patterns = new();
            //        int NumX = (int)(cx.Count / rows);
            //        int NumY = (int)(cy.Count / cols);

            //        for (int i = 0; i < cx.Count; i++)
            //        {
            //            // determine the index within the current group
            //            int NumXi = i % NumX; // take the remainder
            //            int NumYi = i / NumY;
            //            // Linear interpolation for width
            //            double widthX = (NumX == 1)
            //                ? widthS
            //                : widthS + (widthL - widthS) * NumXi / (NumX - 1);
            //            double widthY = (NumY == 1)
            //               ? widthS
            //               : widthS + (widthL - widthS) * NumYi / (NumY - 1);

            //            Geometry2D.Polygon pi = new(4);
            //            pi.Vx = new VectorD(pi.N)
            //            {
            //                [0] = cx[i] - 0.5 * widthX,
            //                [1] = cx[i] + 0.5 * widthX,
            //                [2] = cx[i] + 0.5 * widthX,
            //                [3] = cx[i] - 0.5 * widthX
            //            };
            //            pi.Vy = new VectorD(pi.N)
            //            {
            //                [0] = cy[i] - 0.5 * widthY,
            //                [1] = cy[i] - 0.5 * widthY,
            //                [2] = cy[i] + 0.5 * widthY,
            //                [3] = cy[i] + 0.5 * widthY
            //            };
            //            patterns.Add(pi);
            //        }
            //        // Initialize the refractive index and permittivity  
            //        N = (w, x, y) =>
            //            {
            //                Complex nInner = isInnerOpaque ? opaque.N(w) : transparent.N(w);
            //                Complex nOuter = isInnerOpaque ? transparent.N(w) : opaque.N(w);
            //                return Geometry2D.Polygon.FillValue(x, y, patterns, nInner, nOuter);
            //            };
            //        Epsilon = (w, x, y) => N(w, x, y) * N(w, x, y);
            //    }
            //}

            ///// <summary>  
            ///// represents a photomask with contact square hole near to bar patterns.  
            ///// </summary>  
            //public class MultiHoleBars : Photomask
            //{
            //    /// <param name="rows"> number of rows of patterns </param>  
            //    /// <param name="cols"> number of columns of patterns </param>  
            //    /// <param name="spacingY"> vertical spacing between patterns </param>  
            //    /// <param name="spacingX"> horizontal spacing between patterns </param>  
            //    /// <param name="widthHole"> width of the contact square hole </param>
            //    /// <param name="widthBarY"> y-width of the bar </param>
            //    /// <param name="widthBarX"> x-width of the bar </param>
            //    /// <param name="opaque"> opaque material that fills within the patterns </param>  
            //    /// <param name="transparent"> transparent material that is outside the patterns </param>  
            //    /// <param name="isInnerOpaque"> indicates whether the inner region is opaque </param> 
            //    public MultiHoleBars(long rows, long cols, double spacingY, double spacingX, 
            //        double widthHole, double widthBarY, double widthBarX,
            //        Material opaque, Material transparent,
            //        bool isInnerOpaque = true)
            //    {
            //        // Derive the center's coordinate of each pattern
            //        GridInfo2D centers = new(rows, cols, spacingY, spacingX);
            //        (VectorD cy, VectorD cx) = centers.GetGrid();

            //        // create the polygon patterns  
            //        List<Geometry2D.Polygon> patterns = new();

            //        for (int i = 0; i < cx.Count; i++)
            //        {
            //            Geometry2D.Polygon pi = new(4);
            //            if (i % 2 == 0)
            //            {
            //                pi.Vx = new VectorD(pi.N)
            //                {
            //                    [0] = cx[i] - 0.5 * widthHole,
            //                    [1] = cx[i] + 0.5 * widthHole,
            //                    [2] = cx[i] + 0.5 * widthHole,
            //                    [3] = cx[i] - 0.5 * widthHole
            //                };
            //                pi.Vy = new VectorD(pi.N)
            //                {
            //                    [0] = cy[i] - 0.5 * widthHole,
            //                    [1] = cy[i] - 0.5 * widthHole,
            //                    [2] = cy[i] + 0.5 * widthHole,
            //                    [3] = cy[i] + 0.5 * widthHole
            //                };
            //            }
            //            else
            //            {
            //                pi.Vx = new VectorD(pi.N)
            //                {
            //                    [0] = cx[i] - 0.5 * widthBarX,
            //                    [1] = cx[i] + 0.5 * widthBarX,
            //                    [2] = cx[i] + 0.5 * widthBarX,
            //                    [3] = cx[i] - 0.5 * widthBarX
            //                };
            //                pi.Vy = new VectorD(pi.N)
            //                {
            //                    [0] = cy[i] - 0.5 * widthBarY,
            //                    [1] = cy[i] - 0.5 * widthBarY,
            //                    [2] = cy[i] + 0.5 * widthBarY,
            //                    [3] = cy[i] + 0.5 * widthBarY
            //                };
            //            }
            //            patterns.Add(pi);
            //        }
            //        // Initialize the refractive index and permittivity  
            //        N = (w, x, y) =>
            //        {
            //            Complex nInner = isInnerOpaque ? opaque.N(w) : transparent.N(w);
            //            Complex nOuter = isInnerOpaque ? transparent.N(w) : opaque.N(w);
            //            return Geometry2D.Polygon.FillValue(x, y, patterns, nInner, nOuter);
            //        };
            //        Epsilon = (w, x, y) => N(w, x, y) * N(w, x, y);
            //    }
            //}

            ///// <summary>  
            ///// represents a photomask with crossed bar in near contact bar patterns.  
            ///// </summary>  
            //public class MultiCrossBars : Photomask
            //{
            //    /// <param name="rows"> number of rows of patterns </param>  
            //    /// <param name="cols"> number of columns of patterns </param>  
            //    /// <param name="spacingY"> vertical spacing between patterns </param>  
            //    /// <param name="spacingX"> horizontal spacing between patterns </param>  
            //    /// <param name="widthBarY"> y-width of the bar </param>
            //    /// <param name="widthBarX"> x-width of the bar </param>
            //    /// <param name="widthCrosBarY"> y-width of the crossed bar </param>
            //    /// <param name="widthCrosBarX"> x-width of the crossed bar </param>
            //    /// <param name="opaque"> opaque material that fills within the patterns </param>  
            //    /// <param name="transparent"> transparent material that is outside the patterns </param>  
            //    /// <param name="isInnerOpaque"> indicates whether the inner region is opaque </param> 
            //    public MultiCrossBars(long rows, long cols, double spacingY, double spacingX,
            //        double widthBarY, double widthBarX, double widthCrosBarY, double widthCrosBarX,
            //        Material opaque, Material transparent,
            //        bool isInnerOpaque = true)
            //    {
            //        // Derive the center's coordinate of each pattern 
            //        GridInfo2D centers = new(rows, cols, spacingY, spacingX);
            //        (VectorD cy, VectorD cx) = centers.GetGrid();

            //        // create the polygon patterns  
            //        List<Geometry2D.Polygon> patterns = new();

            //        for (int i = 0; i < cx.Count; i++)
            //        {
            //            Geometry2D.Polygon pi = new(4);
            //            if (i % 2 == 0)
            //            {
            //                pi.Vx = new VectorD(pi.N)
            //                {
            //                    [0] = cx[i] - 0.5 * widthBarX,
            //                    [1] = cx[i] + 0.5 * widthBarX,
            //                    [2] = cx[i] + 0.5 * widthBarX,
            //                    [3] = cx[i] - 0.5 * widthBarX
            //                };
            //                pi.Vy = new VectorD(pi.N)
            //                {
            //                    [0] = cy[i] - 0.5 * widthBarY,
            //                    [1] = cy[i] - 0.5 * widthBarY,
            //                    [2] = cy[i] + 0.5 * widthBarY,
            //                    [3] = cy[i] + 0.5 * widthBarY
            //                };
            //            }
            //            else
            //            {
            //                pi.Vx = new VectorD(pi.N)
            //                {
            //                    [0] = cx[i] - 0.5 * widthCrosBarX,
            //                    [1] = cx[i] + 0.5 * widthCrosBarX,
            //                    [2] = cx[i] + 0.5 * widthCrosBarX,
            //                    [3] = cx[i] - 0.5 * widthCrosBarX
            //                };
            //                pi.Vy = new VectorD(pi.N)
            //                {
            //                    [0] = cy[i] - 0.5 * widthCrosBarY,
            //                    [1] = cy[i] - 0.5 * widthCrosBarY,
            //                    [2] = cy[i] + 0.5 * widthCrosBarY,
            //                    [3] = cy[i] + 0.5 * widthCrosBarY
            //                };
            //            }
            //            patterns.Add(pi);
            //        }
            //        // Initialize the refractive index and permittivity  
            //        N = (w, x, y) =>
            //        {
            //            Complex nInner = isInnerOpaque ? opaque.N(w) : transparent.N(w);
            //            Complex nOuter = isInnerOpaque ? transparent.N(w) : opaque.N(w);
            //            return Geometry2D.Polygon.FillValue(x, y, patterns, nInner, nOuter);
            //        };
            //        Epsilon = (w, x, y) => N(w, x, y) * N(w, x, y);
            //    }
            //}

            ///// <summary>  
            ///// represents a photomask with different corner structures patterns.  
            ///// </summary>  
            //public class MultiCorners : Photomask
            //{
            //    /// <param name="number"> number of corner patterns </param>      
            //    /// <param name="spacing"> spacing between patterns </param>   
            //    /// <param name="widthS"> width of corner pattern </param>
            //    /// <param name="opaque"> opaque material that fills within the patterns </param>  
            //    /// <param name="transparent"> transparent material that is outside the patterns </param>  
            //    /// <param name="isInnerOpaque"> indicates whether the inner region is opaque </param> 
            //    public MultiCorners(long number, double spacing, double widthS,
            //        Material opaque, Material transparent,
            //        bool isInnerOpaque = true)
            //    {
            //        // Derive the center's coordinate of each pattern
            //        GridInfo2D centers = new(number, number, spacing, spacing);
            //        (VectorD cy, VectorD cx) = centers.GetMainDiagonal();

            //        // create the polygon patterns
            //        List<Geometry2D.Polygon> patterns = new();
            //        double widthL = (number + 1) * spacing;
            //        for (int i = 0; i < number; i++)
            //        {
            //            Geometry2D.Polygon pi;
            //            double t = (double)(i + 1) / (number + 1);
            //            double width = t * widthL;

            //            if (i == 0)
            //            {
            //                pi = new Geometry2D.Polygon(4);
            //                pi.Vx = new VectorD(pi.N)
            //                {
            //                    [0] = cx[i] - 0.5 * widthS,
            //                    [1] = cx[i] + width,
            //                    [2] = cx[i] + width,
            //                    [3] = cx[i] - 0.5 * widthS
            //                };
            //                pi.Vy = new VectorD(pi.N)
            //                {
            //                    [0] = cy[i] + 0.5 * widthS,
            //                    [1] = cy[i] + 0.5 * widthS,
            //                    [2] = cy[i] - width,
            //                    [3] = cy[i] - width
            //                };
            //            }
            //            else
            //            {
            //                pi = new Geometry2D.Polygon(6);
            //                pi.Vx = new VectorD(pi.N)
            //                {
            //                    [0] = cx[i] - 0.5 * widthS,
            //                    [1] = cx[i] + 0.5 * widthS,
            //                    [2] = cx[i] + 0.5 * widthS,
            //                    [3] = cx[i] + width,
            //                    [4] = cx[i] + width,
            //                    [5] = cx[i] - 0.5 * widthS
            //                };
            //                pi.Vy = new VectorD(pi.N)
            //                {
            //                    [0] = cy[i] - width,
            //                    [1] = cy[i] - width,
            //                    [2] = cy[i] - 0.5 * widthS,
            //                    [3] = cy[i] - 0.5 * widthS,
            //                    [4] = cy[i] + 0.5 * widthS,
            //                    [5] = cy[i] + 0.5 * widthS
            //                };
            //            }
            //            patterns.Add(pi);
            //        }
            //        // Initialize the refractive index and permittivity  
            //        N = (w, x, y) =>
            //        {
            //            Complex nInner = isInnerOpaque ? opaque.N(w) : transparent.N(w);
            //            Complex nOuter = isInnerOpaque ? transparent.N(w) : opaque.N(w);
            //            return Geometry2D.Polygon.FillValue(x, y, patterns, nInner, nOuter);
            //        };
            //        Epsilon = (w, x, y) => N(w, x, y) * N(w, x, y);
            //    }
            //}
        }
        #endregion
    }
}
