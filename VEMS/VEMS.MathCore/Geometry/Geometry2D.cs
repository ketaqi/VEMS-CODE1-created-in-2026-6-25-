using static VEMS.MathCore.Geometry2D;

namespace VEMS.MathCore
{
    /// <summary>
    /// simple geometry class
    /// [Numerical Recipes]
    /// </summary>
    public class Geometry2D
    {

        #region ===== Point =====

        /// <summary>
        /// a point in 2D plane
        /// </summary>
        public class Point
        {
            /// <summary>
            /// x-coordinate of the point
            /// </summary>
            public double X { get; set; }

            /// <summary>
            /// y-coordinate of the point
            /// </summary>
            public double Y { get; set; }


            /// <summary>
            /// constructs a point in 2D plane
            /// </summary>
            /// <param name="x"> x-coordinate </param>
            /// <param name="y"> y-coordinate </param>
            public Point(double x, double y)
            {
                X = x;
                Y = y;
            }

            /// <summary>
            /// constructs a point by copying from another
            /// </summary>
            /// <param name="p"> source point to copy </param>
            public Point(Point p)
            {
                X = p.X;
                Y = p.Y;
            }

        }

        #endregion
        #region ===== Box =====

        /// <summary>
        /// a rectangle that is aligned with the coordinate axes
        /// </summary>
        public class Box
        {
            #region properties

            /// <summary>
            /// min of all coordinates
            /// </summary>
            public Point Lo { get; set; }

            /// <summary>
            /// max of all coordinates
            /// </summary>
            public Point Hi { get; set; }

            #endregion
            #region constructor 

            /// <summary>
            /// constructs a box with given diagonally 
            /// opposite corners 
            /// </summary>
            /// <param name="lo"></param>
            /// <param name="hi"></param>
            public Box(Point lo, Point hi)
            {
                Lo = new(lo);
                Hi = new(hi);
            }

            #endregion
            #region methods

            /// <summary>
            /// checks if a point is inside the box
            /// </summary>
            /// <param name="p"> point </param>
            /// <returns> inside or not </returns>
            public bool IsPointInside(Point p)
            {
                double dis = Distance(this, p);
                if (dis > 0.0) { return true; }
                else if (dis == 0) { return false; }
                else { throw new ArithmeticException("Unexpected case!"); }
            }

            #endregion
        }

        #endregion
        #region ===== Circle =====

        /// <summary>
        /// circle
        /// </summary>
        public class Circle
        {
            #region properties

            /// <summary>
            /// certer position of the circle
            /// </summary>
            public Point Center { get; set; }

            /// <summary>
            /// radius of the circle
            /// </summary>
            public double Radius { get; set; }

            #endregion
            #region constructor

            /// <summary>
            /// constructs a circle with given center and radius
            /// </summary>
            /// <param name="p"> center of the circle </param>
            /// <param name="r"> radius of the circle </param>
            public Circle(Point p, double r)
            {
                Center = new(p);
                Radius = r;
            }

            #endregion
            #region methods


            #endregion
        }

        #endregion
        #region ===== Triangle =====

        /// <summary>
        /// triangle
        /// </summary>
        public class Triangle
        {
            #region properties

            /// <summary>
            /// vertex A of the triangle
            /// </summary>
            public Point A { get; set; }

            /// <summary>
            /// vertex B of the triangle
            /// </summary>
            public Point B { get; set; }

            /// <summary>
            /// vertex C of the triangle
            /// </summary>
            public Point C { get; set; }

            #endregion
            #region constructor

            /// <summary>
            /// constructs a triangle with its three vertices
            /// </summary>
            /// <param name="a"> vertex a </param>
            /// <param name="b"> vertex b </param>
            /// <param name="c"> vertex c </param>
            public Triangle(Point a, Point b, Point c)
            {
                A = new(a);
                B = new(b);
                C = new(c);
            }

            #endregion
            #region methods


            #endregion
        }
        #endregion
        #region ---- Polygon ---- 

        /// <summary>
        /// Represents a simple polygon in 2D space.
        /// The polygon is defined by its vertices, with coordinates stored in <see cref="Vx"/> and <see cref="Vy"/>.
        /// Provides methods for geometric operations such as point-in-polygon tests, grid sampling, and pattern generation.
        /// </summary>
        public class Polygon
        {
            #region properties

            /// <summary>
            /// Gets or sets the total number of vertices in the polygon.
            /// </summary>
            public long N { get; set; }

            /// <summary>
            /// Gets or sets the x-coordinates of the polygon's vertices.
            /// </summary>
            public VectorD? Vx { get; set; }

            /// <summary>
            /// Gets or sets the y-coordinates of the polygon's vertices.
            /// </summary>
            public VectorD? Vy { get; set; }

            #endregion
            #region constructors

            /// <summary>
            /// Initializes a new instance of the <see cref="Polygon"/> class with no vertices.
            /// </summary>
            internal Polygon() { }

            /// <summary>
            /// Initializes a new instance of the <see cref="Polygon"/> class with the specified number of vertices and coordinate vectors.
            /// </summary>
            /// <param name="n">The number of vertices.</param>
            /// <param name="vx">The x-coordinates of the vertices.</param>
            /// <param name="vy">The y-coordinates of the vertices.</param>
            public Polygon(long n, VectorD vx, VectorD vy)
            {
                N = n;
                Vx = vx;
                Vy = vy;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="Polygon"/> class with the specified number of vertices.
            /// The coordinate vectors are created empty and can be filled later.
            /// </summary>
            /// <param name="n">The number of vertices.</param>
            public Polygon(long n)
            {
                N = n;
                Vx = new(N);
                Vy = new(N);
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="Polygon"/> class by copying another polygon.
            /// Performs a deep copy of the vertex coordinates if available.
            /// </summary>
            /// <param name="other">The source polygon to copy.</param>
            public Polygon(Polygon other)
            {
                N = other.N;
                if (other.Vx == null || other.Vy == null) { return; }
                Vx = new(other.Vx, deepCopy: true);
                Vy = new(other.Vy, deepCopy: true);
            }

            #endregion
            #region methods

            /// <summary>
            /// Computes the winding number of the polygon around an arbitrary point.
            /// The winding number indicates how many times the polygon winds around the point.
            /// </summary>
            /// <param name="x">The x-coordinate of the point.</param>
            /// <param name="y">The y-coordinate of the point.</param>
            /// <returns>
            /// The winding number. For a simple polygon, the result is 1 (inside), -1 (inside with opposite orientation), or 0 (outside).
            /// </returns>
            /// <exception cref="ArgumentNullException">Thrown if <see cref="Vx"/> or <see cref="Vy"/> is null.</exception>
            internal int Winding(double x, double y)
            {
                if (Vx == null || Vy == null) { throw new ArgumentNullException("Vx/y"); }
                // initializes
                int wind = 0;
                // set the last vertex as "previous" to start
                double px = Vx[N - 1, false];
                double py = Vy[N - 1, false];
                // loop
                for (long i = 0; i < N; i++)
                {
                    // "current" vertex
                    double dx = Vx[i, false];
                    double dy = Vy[i, false];
                    // cases
                    if (py <= y)
                    {
                        // upward-crossing edge ...
                        if (dy > y && (px - x) * (dy - y) - (py - y) * (dx - x) > 0)
                            wind++;
                    }
                    else
                    {
                        // downward-crossing edge ...
                        if (dy <= y && (px - x) * (dy - y) - (py - y) * (dx - x) < 0)
                            wind--;
                    }
                    // set "curernt" vertex as "previous", for the next round ...
                    px = dx;
                    py = dy;
                }
                return wind;
            }

            /// <summary>
            /// Determines whether a point is inside the polygon using the winding number algorithm.
            /// </summary>
            /// <param name="x">The x-coordinate of the point.</param>
            /// <param name="y">The y-coordinate of the point.</param>
            /// <returns>
            /// <c>true</c> if the point is inside the polygon; <c>false</c> if outside.
            /// </returns>
            /// <exception cref="ArithmeticException">Thrown if the polygon is not simple (winding number is not 0, 1, or -1).</exception>
            public bool IsPointInside(double x, double y)
            {
                int wind = Winding(x, y);
                if (wind == 1 || wind == -1) { return true; }
                else if (wind == 0) { return false; }
                else { throw new ArithmeticException("This is not a simple polygon!"); }
            }

            /// <summary>
            /// Samples the polygon on a target uniform grid.
            /// Returns a matrix indicating which grid points are inside the polygon.
            /// </summary>
            /// <param name="grid">The target uniform grid.</param>
            /// <param name="loopMode">The loop-computational mode option.</param>
            /// <returns>
            /// A <see cref="MatrixD"/> where each element is 1.0 if the corresponding grid point is inside the polygon, otherwise 0.0.
            /// </returns>
            /// <exception cref="NotImplementedException">Thrown if <paramref name="loopMode"/> is <see cref="LoopMode.Vectorized"/>.</exception>
            public MatrixD SampleOnGrid(GridInfo2D grid,
                LoopMode loopMode = Defaults.LoopOption)
            {
                MatrixD p = new(grid.Rows, grid.Cols);
                switch (loopMode)
                {
                    case LoopMode.Sequential:
                        {
                            for (long i = 0; i < grid.Rows * grid.Cols; i++)
                            {
                                long iRow = i / grid.Cols;
                                long iCol = i % grid.Cols;
                                double y = grid.GetCoordinateY(iRow);
                                double x = grid.GetCoordinateX(iCol);
                                if (IsPointInside(x, y)) { p[iRow, iCol, false] = 1.0; }
                            }
                            break;
                        }
                    case LoopMode.Parallel:
                        {
                            Parallel.For(0, grid.Rows * grid.Cols, i =>
                            {
                                long iRow = i / grid.Cols;
                                long iCol = i % grid.Cols;
                                double y = grid.GetCoordinateY(iRow);
                                double x = grid.GetCoordinateX(iCol);
                                if (IsPointInside(x, y)) { p[iRow, iCol, false] = 1.0; }
                            });
                            break;
                        }
                    case LoopMode.Vectorized:
                        throw new NotImplementedException();
                    default: goto case LoopMode.Sequential;
                }
                return p;
            }

            #endregion
            #region static methods

            /// <summary>
            /// Fills a value by checking whether a point is inside any one of the given polygons.
            /// Returns <paramref name="innerValue"/> if the point is inside any polygon, otherwise <paramref name="outerValue"/>.
            /// </summary>
            /// <typeparam name="T">The value type (e.g., double or complex).</typeparam>
            /// <param name="x">The x-coordinate of the point.</param>
            /// <param name="y">The y-coordinate of the point.</param>
            /// <param name="polygons">The list of polygons to check.</param>
            /// <param name="innerValue">The value to return if the point is inside a polygon.</param>
            /// <param name="outerValue">The value to return if the point is outside all polygons.</param>
            /// <returns>
            /// <paramref name="innerValue"/> if the point is inside any polygon; otherwise <paramref name="outerValue"/>.
            /// </returns>
            public static T FillValue<T>(double x, double y,
                List<Polygon> polygons,
                T innerValue, T outerValue) where T : struct
            {
                // checks for each polygon
                foreach (Polygon p in polygons)
                {
                    // inside one of the polygons
                    if (p.IsPointInside(x, y))
                    { return innerValue; }
                }
                // not within any polygon
                return outerValue;
            }


            #region ---- create single polygon ----

            /// <summary>
            /// Creates a rectangular polygon centered at the specified coordinates with the given width and height.
            /// The rectangle is axis-aligned and represented as a polygon with 4 vertices.
            /// </summary>
            /// <param name="centerY">The y-coordinate of the rectangle's center.</param>
            /// <param name="centerX">The x-coordinate of the rectangle's center.</param>
            /// <param name="widthY">The height of the rectangle (in the y-direction).</param>
            /// <param name="widthX">The width of the rectangle (in the x-direction).</param>
            /// <returns>
            /// A <see cref="Polygon"/> object representing the rectangle, with vertices ordered counterclockwise starting from the bottom-left.
            /// </returns>
            public static Polygon CreateRectPolygon(
                double centerY, double centerX,
                double widthY, double widthX)
            {
                double halfWidthX = 0.5 * widthX;
                double halfWidthY = 0.5 * widthY;

                // Use stack allocation for the coordinates to avoid repeated property access
                double x0 = centerX - halfWidthX;
                double x1 = centerX + halfWidthX;
                double y0 = centerY - halfWidthY;
                double y1 = centerY + halfWidthY;

                VectorD vx = new(count: 4, mode: ArrayInitMode.Malloc)
                {
                    [0] = x0,
                    [1] = x1,
                    [2] = x1,
                    [3] = x0
                };
                VectorD vy = new(count: 4, mode: ArrayInitMode.Malloc)
                {
                    [0] = y0,
                    [1] = y0,
                    [2] = y1,
                    [3] = y1
                };

                return new Polygon(n: 4, vx: vx, vy: vy);
            }


            /// <summary>
            /// Creates a corner polygon centered at the specified coordinates with the given width and height.
            /// The corner polygon is axis-aligned and represented as a polygon with 6 vertices.
            /// </summary>
            /// <param name="centerY">The y-coordinate of the corner's center.</param>
            /// <param name="centerX">The x-coordinate of the corner's center.</param>
            /// <param name="widthY">The Maximum length of the corner (in the y-direction).</param>
            /// <param name="widthX">The width of the corner (in the x-direction).</param>
            /// <returns>
            /// A <see cref="Polygon"/> object representing the rectangle, with vertices ordered counterclockwise starting from the bottom-left.
            /// </returns>
            public static Polygon CreateCornerPolygon(
                double centerY, double centerX,
                double widthY, double widthX)
            {
                double halfWidthX = 0.5 * widthX;

                // Use stack allocation for the coordinates to avoid repeated property access
                double x0 = centerX - halfWidthX;
                double x1 = centerX + halfWidthX;
                double x2 = centerX + widthY;
                double y0 = centerY - widthY;
                double y1 = centerY + widthY;
                double y2 = centerY + halfWidthX;
                double y3 = centerY - halfWidthX;

                VectorD vx = new(count: 6, mode: ArrayInitMode.Malloc)
                {
                    [0] = x0,
                    [1] = x1,
                    [2] = x1,
                    [3] = x2,
                    [4] = x2,
                    [5] = x0
                };
                VectorD vy = new(count: 6, mode: ArrayInitMode.Malloc)
                {
                    [0] = y0,
                    [1] = y0,
                    [2] = y3,
                    [3] = y3,
                    [4] = y2,
                    [5] = y2
                };

                return new Polygon(n: 6, vx: vx, vy: vy);
            }

            /// <summary>
            /// Creates a rectangular polygon centered at the specified coordinates with the given width and height.
            /// The rectangle is axis-aligned and represented as a polygon with 4 vertices.
            /// </summary>
            /// <param name="centerY">The y-coordinate of the rectangle's center.</param>
            /// <param name="centerX">The x-coordinate of the rectangle's center.</param>
            /// <param name="widthY">The height of the rectangle (in the y-direction).</param>
            /// <param name="widthX">The width of the rectangle (in the x-direction).</param>
            /// <returns>
            /// A <see cref="Polygon"/> object representing the rectangle, with vertices ordered counterclockwise starting from the bottom-left.
            /// </returns>
            public static Polygon CreateCornerRectPolygon(
                double centerY, double centerX,
                double widthY, double widthX)
            {
                double halfWidthX = 0.5 * widthX;
                double halfWidthY = widthY;

                // Use stack allocation for the coordinates to avoid repeated property access
                double x0 = centerX - halfWidthX;
                double x1 = centerX + widthY;
                double y0 = centerY - widthY;
                double y1 = centerY + halfWidthX;

                VectorD vx = new(count: 4, mode: ArrayInitMode.Malloc)
                {
                    [0] = x0,
                    [1] = x1,
                    [2] = x1,
                    [3] = x0
                };
                VectorD vy = new(count: 4, mode: ArrayInitMode.Malloc)
                {
                    [0] = y1,
                    [1] = y1,
                    [2] = y0,
                    [3] = y0
                };

                return new Polygon(n: 4, vx: vx, vy: vy);
            }

            /// <summary>
            /// Creates a circular polygon centered at the specified coordinates with the given radius.
            /// The circle is approximated by a polygon with <paramref name="numSegments"/> vertices.
            /// Vertices are distributed evenly around the circumference.
            /// </summary>
            /// <param name="centerY">The y-coordinate of the circle's center.</param>
            /// <param name="centerX">The x-coordinate of the circle's center.</param>
            /// <param name="radius">The radius of the circle.</param>
            /// <param name="numSegments">
            /// The number of segments (vertices) to use for the circle polygon. Default is 37.
            /// Higher values yield a smoother approximation.
            /// </param>
            /// <returns>
            /// A <see cref="Polygon"/> object representing the circle, with vertices ordered counterclockwise.
            /// </returns>
            public unsafe static Polygon CreateCircPolygon(
                double centerY, double centerX,
                double radius,
                int numSegments = 37)
            {
                VectorD t = new(count: numSegments, mode: ArrayInitMode.Malloc);
                double* pt = (double*)t.VPtr;
                for (long i = 0; i < numSegments; i++)
                { pt[i] = 2.0 * Math.PI * i / numSegments; }

                var (vy, vx) = VMath.SinCos(t);
                for (long i = 0; i < numSegments; i++)
                {
                    vx[i, false] = centerX + radius * vx[i, false];
                    vy[i, false] = centerY + radius * vy[i, false];
                }
                return new Polygon(n: numSegments, vx: vx, vy: vy);
            }

            #endregion
            #region ---- create multi polygons ----

            /// <summary>
            /// Creates a list of rectangular polygons with specified centers and dimensions.
            /// Each rectangle is axis-aligned and represented as a polygon with 4 vertices.
            /// </summary>
            /// <param name="centerY">A <see cref="VectorD"/> containing the y-coordinates of the rectangle centers.</param>
            /// <param name="centerX">A <see cref="VectorD"/> containing the x-coordinates of the rectangle centers.</param>
            /// <param name="widthY">A <see cref="VectorD"/> containing the heights (y-direction) of the rectangles.</param>
            /// <param name="widthX">A <see cref="VectorD"/> containing the widths (x-direction) of the rectangles.</param>
            /// <returns>
            /// A <see cref="List{Polygon}"/> where each element is a rectangle polygon centered at the specified coordinates with the given dimensions.
            /// </returns>
            public unsafe static List<Polygon> CreateRectPolygons(
                VectorD centerY, VectorD centerX,
                VectorD widthY, VectorD widthX)
            {
                int n = (int)(centerY.Count * centerX.Count);
                List<Polygon> polygons = new(capacity: n);

                double* cy = (double*)centerY.DataPtr;
                double* cx = (double*)centerX.DataPtr;
                double* wy = (double*)widthY.DataPtr;
                double* wx = (double*)widthX.DataPtr;

                int count = 0;
                for (int j = 0; j < centerY.Count; j++)
                {
                    for (int i = 0; i < centerX.Count; i++)
                    {
                        Polygon pi = CreateRectPolygon(cy[j], cx[i], wy[count], wx[count]);
                        polygons.Add(pi);
                        count++;
                    }
                }

                return polygons;
            }


            /// <summary>
            /// Creates a list of rectangular polygons with specified centers and dimensions.
            /// Each rectangle is axis-aligned and represented as a polygon with 4 vertices.
            /// </summary>
            /// <param name="centerY">A <see cref="VectorD"/> containing the y-coordinates of the rectangle centers.</param>
            /// <param name="centerX">A <see cref="VectorD"/> containing the x-coordinates of the rectangle centers.</param>
            /// <param name="width1Y">A <see cref="VectorD"/> containing the heights (y-direction) of the rectangles.</param>
            /// <param name="width1X">A <see cref="VectorD"/> containing the widths (x-direction) of the rectangles.</param>
            /// <param name="width2Y">A <see cref="VectorD"/> containing the heights (y-direction) of the rectangles.</param>
            /// <param name="width2X">A <see cref="VectorD"/> containing the widths (x-direction) of the rectangles.</param>
            /// <returns>
            /// A <see cref="List{Polygon}"/> where each element is a rectangle polygon centered at the specified coordinates with the given dimensions.
            /// </returns>
            public unsafe static List<Polygon> CreateCombineRectPolygons(
                VectorD centerY, VectorD centerX,
                VectorD width1Y, VectorD width1X,
                VectorD width2Y, VectorD width2X)
            {
                int n = (int)(centerY.Count * centerX.Count);
                List<Polygon> polygons = new(capacity: n);

                double* cy = (double*)centerY.DataPtr;
                double* cx = (double*)centerX.DataPtr;
                double* w1y = (double*)width1Y.DataPtr;
                double* w1x = (double*)width1X.DataPtr;
                double* w2y = (double*)width2Y.DataPtr;
                double* w2x = (double*)width2X.DataPtr;

                for (int i = 0; i < centerX.Count; i++)
                {
                    for (int j = 0; j < centerY.Count; j++)
                    {
                        if (i % 2 == 0)
                        {
                            Polygon pi = CreateRectPolygon(cy[j], cx[i], w1y[j], w1x[i]);
                            polygons.Add(pi);
                        }
                        else
                        {
                            Polygon pi = CreateRectPolygon(cy[j], cx[i], w2y[j], w2x[i]);
                            polygons.Add(pi);
                        }
                    }
                }
                return polygons;
            }


            /// <summary>
            /// Creates a list of corner polygons with specified centers and dimensions.
            /// Each polygon is axis-aligned and represented as a polygon with 6 vertices.
            /// </summary>
            /// <param name="centerY">A <see cref="VectorD"/> containing the y-coordinates of the rectangle centers.</param>
            /// <param name="centerX">A <see cref="VectorD"/> containing the x-coordinates of the rectangle centers.</param>
            /// <param name="widthX">A <see cref="VectorD"/> containing the minimum width of the Corners.</param>
            /// <param name="spacing"> spacing between patterns.</param> 
            /// <returns>
            /// A <see cref="List{Polygon}"/> where each element is a corner polygon centered at the specified coordinates with the given dimensions.
            /// </returns>
            public unsafe static List<Polygon> CreateCornerPolygons(
                VectorD centerY, VectorD centerX,
                VectorD widthX, double spacing)
            {
                int n = (int)centerY.Count;
                List<Polygon> polygons = new(capacity: n);
                double widthY = (n + 1) * spacing;

                double* cy = (double*)centerY.DataPtr;
                double* cx = (double*)centerX.DataPtr;
                double* wx = (double*)widthX.DataPtr;

                for (int i = 0; i < n; i++)
                {
                    double t = (double)(i + 1) / (centerY.Count + 1);
                    double width = t * widthY;
                    if (i == 0)
                    {
                        Polygon pi = CreateCornerRectPolygon(cy[i], cx[i], width, wx[i]);
                        polygons.Add(pi);
                    }
                    else
                    {
                        Polygon pi = CreateCornerPolygon(cy[i], cx[i], width, wx[i]);
                        polygons.Add(pi);
                    }
                }
                return polygons;
            }


            /// <summary>
            /// Creates a list of circular polygons centered at the specified coordinates with the given radii.
            /// Each circle is approximated by a polygon with <paramref name="numSegments"/> vertices.
            /// Vertices are distributed evenly around the circumference for each circle.
            /// </summary>
            /// <param name="centerY">A <see cref="VectorD"/> containing the y-coordinates of the circle centers.</param>
            /// <param name="centerX">A <see cref="VectorD"/> containing the x-coordinates of the circle centers.</param>
            /// <param name="radius">A <see cref="VectorD"/> containing the radii of the circles.</param>
            /// <param name="numSegments">
            /// The number of segments (vertices) to use for each circle polygon. Default is 37.
            /// Higher values yield a smoother approximation.
            /// </param>
            /// <returns>
            /// A <see cref="List{Polygon}"/> where each element is a circle polygon centered at the specified coordinates with the given radius.
            /// </returns>
            public unsafe static List<Polygon> CreateCircPolygons(
                VectorD centerY, VectorD centerX,
                VectorD radius,
                int numSegments = 37)
            {
                int n = (int)(centerY.Count * centerY.Count);
                List<Polygon> polygons = new(capacity: n);

                double* cy = (double*)centerY.DataPtr;
                double* cx = (double*)centerX.DataPtr;
                double* r = (double*)radius.DataPtr;

                for (int i = 0; i < centerX.Count; i++)
                {
                    for (int j = 0; j < centerY.Count; j++)
                    {
                        Polygon pi = CreateCircPolygon(cy[j], cx[i], r[i], numSegments);
                        polygons.Add(pi);
                    }
                }

                return polygons;
            }

            #endregion
            #region ---- obsolete ----

            ///// <summary>
            ///// Creates a list of rectangular polygon patterns centered at the specified coordinates.
            ///// Each rectangle is defined by its center (<paramref name="cx"/>[i], <paramref name="cy"/>[i]) and dimensions (<paramref name="widthX"/>, <paramref name="widthY"/>).
            ///// The rectangles are represented as polygons with 4 vertices.
            ///// </summary>
            ///// <param name="cy">The y-coordinates of the centers of the rectangles.</param>
            ///// <param name="cx">The x-coordinates of the centers of the rectangles.</param>
            ///// <param name="widthY">The height of each rectangle (in the y-direction).</param>
            ///// <param name="widthX">The width of each rectangle (in the x-direction).</param>
            ///// <returns>
            ///// A list of <see cref="Polygon"/> objects, each representing a rectangle centered at the specified coordinates with the given dimensions.
            ///// </returns>
            //[Obsolete]
            //public static List<Polygon> CreateRectPolygons(
            //    VectorD cy, VectorD cx,
            //    double widthY, double widthX)
            //{
            //    int count = (int)cx.Count;
            //    List<Polygon> patterns = new(capacity: count);

            //    double halfWidthX = 0.5 * widthX;
            //    double halfWidthY = 0.5 * widthY;

            //    for (int i = 0; i < count; i++)
            //    {
            //        double cxVal = cx[i, false];
            //        double cyVal = cy[i, false];

            //        VectorD vx = new(count: 4, mode: ArrayInitMode.Malloc)
            //        {
            //            [0] = cxVal - halfWidthX,
            //            [1] = cxVal + halfWidthX,
            //            [2] = cxVal + halfWidthX,
            //            [3] = cxVal - halfWidthX
            //        };

            //        VectorD vy = new(count: 4, mode: ArrayInitMode.Malloc)
            //        {
            //            [0] = cyVal - halfWidthY,
            //            [1] = cyVal - halfWidthY,
            //            [2] = cyVal + halfWidthY,
            //            [3] = cyVal + halfWidthY
            //        };

            //        Polygon pi = new (n: 4, vx: vx, vy: vy);
            //        patterns.Add(pi);
            //    }
            //    return patterns;
            //}

            ///// <summary>
            ///// Creates a list of circular polygon patterns centered at the specified coordinates.
            ///// Each circle is approximated by a polygon with <paramref name="numSegments"/> vertices.
            ///// The circles are represented as polygons with vertices evenly distributed around the circumference.
            ///// </summary>
            ///// <param name="cy">The y-coordinates of the centers of the circles.</param>
            ///// <param name="cx">The x-coordinates of the centers of the circles.</param>
            ///// <param name="radius">The radius of each circle.</param>
            ///// <param name="numSegments">The number of segments (vertices) to use for each circle polygon. Default is 37.</param>
            ///// <returns>
            ///// A list of <see cref="Polygon"/> objects, each representing a circle centered at the specified coordinates with the given radius.
            ///// </returns>
            //[Obsolete]
            //public unsafe static List<Polygon> CreateCircPolygons(
            //    VectorD cy, VectorD cx,
            //    double radius,
            //    int numSegments = 37)
            //{
            //    int count = (int)cx.Count;
            //    List<Polygon> patterns = new(capacity: count);

            //    for (int i = 0; i < count; i++)
            //    {
            //        double cxVal = cx[i, false];
            //        double cyVal = cy[i, false];

            //        VectorD vx = new(count: numSegments, mode: ArrayInitMode.Malloc);
            //        VectorD vy = new(count: numSegments, mode: ArrayInitMode.Malloc);

            //        double* vxPtr = (double*)vx.VPtr;
            //        double* vyPtr = (double*)vy.VPtr;
            //        for (int k = 0; k < numSegments; k++)
            //        {
            //            double theta = 2.0 * Math.PI * k / numSegments;
            //            vxPtr[k] = cxVal + radius * Math.Cos(theta);
            //            vyPtr[k] = cyVal + radius * Math.Sin(theta);
            //        }

            //        Polygon pi = new(n: numSegments, vx: vx, vy: vy);
            //        patterns.Add(pi);
            //    }

            //    return patterns;
            //}

            #endregion

            #endregion
        }

        #endregion




        #region ===== common methods =====

        /// <summary>
        /// computes the distance between two points
        /// </summary>
        /// <param name="p1"> first point </param>
        /// <param name="p2"> second point </param>
        /// <returns> distance between the two points </returns>
        public static double Distance(Point p1, Point p2)
        {
            double dx = p1.X - p2.X;
            double dy = p1.Y - p2.Y;
            double d2 = dx * dx + dy * dy;
            return Math.Sqrt(d2);
        }

        /// <summary>
        /// computes the distance of a point from a box
        /// i.e. the distance to the nearest point of the box
        /// </summary>
        /// <param name="b"> box </param>
        /// <param name="p"> point </param>
        /// <returns> distance </returns>
        public static double Distance(Box b, Point p)
        {
            double dx = 0.0, dy = 0.0;
            // lo
            if (p.X < b.Lo.X) { dx = p.X - b.Lo.X; }
            if (p.Y < b.Lo.Y) { dy = p.Y - b.Lo.Y; }
            // hi
            if (p.X > b.Hi.X) { dx = p.X - b.Hi.X; }
            if (p.Y > b.Hi.Y) { dy = p.Y - b.Hi.Y; }
            // compute distance
            double d2 = dx * dx + dy * dy;
            return Math.Sqrt(d2);
        }

        #endregion

    }


    /// <summary>
    /// Provides static methods for generating common polygon patterns on a grid.
    /// </summary>
    /// <remarks>This class contains nested types that offer functionality for creating uniform and
    /// non-uniform polygon patterns. Use the <see cref="Uniform"/> class for generating polygons with consistent
    /// dimensions across a grid. Use the <see cref="NonUniform"/> class for generating polygons with varying
    /// dimensions.</remarks>
    public class GridPolygons
    {

        /// <summary>
        /// Provides static methods for generating uniform polygon patterns on a grid.
        /// </summary>
        public class Uniform
        {

            /// <summary>
            /// Generates a list of rectangular bar polygons centered at each grid point in the specified grid.
            /// Each rectangle has the specified width in the x and y directions.
            /// </summary>
            /// <param name="centers">The grid containing the center coordinates for each bar.</param>
            /// <param name="widthY">The height of each bar (in the y-direction).</param>
            /// <param name="widthX">The width of each bar (in the x-direction).</param>
            /// <returns>
            /// A list of <see cref="Geometry2D.Polygon"/> objects, each representing a rectangular bar centered at a grid point.
            /// </returns>
            public static List<Polygon> MultiBars(GridInfo2D centers,
                double widthY, double widthX)
            {
                long n = centers.Rows * centers.Cols;
                var (cy, cx) = centers.GetAllCoordinates();
                VectorD wy = new(count: n, initVal: widthY);
                VectorD wx = new(count: n, initVal: widthX);

                return Polygon.CreateRectPolygons(
                    centerY: cy, centerX: cx,
                    widthY: wy, widthX: wx);
            }


            /// <summary>
            /// Generates a list of square hole polygons centered at each grid point in the specified grid.
            /// Each square has the specified diameter in both x and y directions.
            /// </summary>
            /// <param name="centers">The grid containing the center coordinates for each hole.</param>
            /// <param name="diameter">The diameter of each square hole.</param>
            /// <returns>
            /// A list of <see cref="Geometry2D.Polygon"/> objects, each representing a square hole centered at a grid point.
            /// </returns>
            public static List<Polygon> MultiSquareHoles(GridInfo2D centers,
                double diameter)
            {
                long n = centers.Rows * centers.Cols;
                var (cy, cx) = centers.GetAllCoordinates();
                VectorD d = new(count: n, initVal: diameter);

                return Polygon.CreateRectPolygons(
                    centerY: cy, centerX: cx,
                    widthY: d, widthX: d);
            }


            /// <summary>
            /// Generates a list of circular hole polygons centered at each grid point in the specified grid.
            /// Each circle is approximated by a polygon with <paramref name="numSegments"/> vertices.
            /// </summary>
            /// <param name="centers">The grid containing the center coordinates for each hole.</param>
            /// <param name="radius">The radius of each circular hole.</param>
            /// <param name="numSegments">The number of segments (vertices) to use for each circle polygon. Default is 37.</param>
            /// <returns>
            /// A list of <see cref="Geometry2D.Polygon"/> objects, each representing a circular hole centered at a grid point.
            /// </returns>
            public static List<Polygon> MultiCircHoles(GridInfo2D centers,
                double radius,
                int numSegments = 37)
            {
                long n = centers.Rows * centers.Cols;
                var (cy, cx) = centers.GetAllCoordinates();
                VectorD r = new(count: n, initVal: radius);

                return Polygon.CreateCircPolygons(
                    centerY: cy, centerX: cx,
                    radius: r, numSegments: numSegments);
            }

        }

        /// <summary>
        /// Provides static methods for generating Nonuniform polygon patterns on a grid.
        /// </summary>
        public class NonUniform
        {

            /// <summary>
            /// Generates a list of rectangular bar polygons centered at each grid point in the specified grid.
            /// Each rectangle has the specified width in the x and y directions.
            /// </summary>
            /// <param name="centers">The grid containing the center coordinates for each bar.</param>
            /// <param name="widthY">The height of each bar (in the y-direction).</param>
            /// <param name="widthXMin">The minimum width of bar (in the x-direction).</param>
            /// <param name="widthXMax">The maximum width of bar (in the x-direction).</param>
            /// <returns>
            /// A list of <see cref="Geometry2D.Polygon"/> objects, each representing a rectangular bar centered at a grid point.
            /// </returns>
            public static List<Polygon> MultiBars(GridInfo2D centers,
                double widthY, double widthXMin, double widthXMax)
            {
                long n = centers.Rows * centers.Cols;
                var (cy, cx) = centers.GetAllCoordinates();
                VectorD wy = new(count: n, initVal: widthY);
                double dw = (widthXMax - widthXMin) / (centers.Cols - 1);
                VectorD wx = new(count: n, mode: ArrayInitMode.Malloc);
                for (long i = 0; i < n; i++)
                {
                    long iCol = i % centers.Cols;
                    wx[i, false] = widthXMin + iCol * dw;
                }

                return Polygon.CreateRectPolygons(
                    centerY: cy, centerX: cx,
                    widthY: wy, widthX: wx);
            }


            /// <summary>
            /// Generates a list of square hole polygons centered at each grid point in the specified grid.
            /// Each square has the specified diameter in both x and y directions.
            /// </summary>
            /// <param name="centers">The grid containing the center coordinates for each hole.</param>
            /// <param name="diameterMin">The minimum diameter of square hole.</param>
            /// <param name="diameterMax">The maximum diameter of square hole.</param>
            /// <returns>
            /// A list of <see cref="Geometry2D.Polygon"/> objects, each representing a square hole centered at a grid point.
            /// </returns>
            public static List<Polygon> MultiSquareHoles(GridInfo2D centers,
                double diameterMin, double diameterMax)
            {
                long n = centers.Rows * centers.Cols;
                var (cy, cx) = centers.GetAllCoordinates();
                VectorD w = new(count: n, mode: ArrayInitMode.Malloc);
                double dw = (diameterMax - diameterMin) / (centers.Cols - 1);
                for (long i = 0; i < n; i++)
                {
                    long iCol = i % centers.Cols;
                    w[i, false] = diameterMin + iCol * dw;
                }

                return Polygon.CreateRectPolygons(
                    centerY: cy, centerX: cx,
                    widthY: w, widthX: w);
            }


            /// <summary>
            /// Generates a list of circular hole polygons centered at each grid point in the specified grid.
            /// Each circle is approximated by a polygon with <paramref name="numSegments"/> vertices.
            /// </summary>
            /// <param name="centers">The grid containing the center coordinates for each hole.</param>
            /// <param name="radiusMin">The minimum radius of circular hole.</param>
            /// <param name="radiusMax">The maximum radius of circular hole.</param>
            /// <param name="numSegments">The number of segments (vertices) to use for each circle polygon. Default is 37.</param>
            /// <returns>
            /// A list of <see cref="Geometry2D.Polygon"/> objects, each representing a circular hole centered at a grid point.
            /// </returns>
            public static List<Polygon> MultiCircHoles(GridInfo2D centers,
                double radiusMin, double radiusMax,
                int numSegments = 37)
            {
                long n = centers.Rows * centers.Cols;
                var (cy, cx) = centers.GetAllCoordinates();
                VectorD r = new(count: n, mode: ArrayInitMode.Malloc);
                double dr = (radiusMax - radiusMin) / (centers.Cols - 1);
                for (long i = 0; i < n; i++)
                {
                    long iCol = i % centers.Cols;
                    r[i, false] = radiusMin + iCol * dr;
                }

                return Polygon.CreateCircPolygons(
                    centerY: cy, centerX: cx,
                    radius: r, numSegments: numSegments);
            }


            /// <summary>
            /// Generates a list of rectangle polygons centered at each grid point in the specified grid.
            /// Each rectangle has the specified width in the x and y directions.
            /// </summary>
            /// <param name="centers">The grid containing the center coordinates for each rectangle.</param>
            /// <param name="widthMin">The minimum width of rectangle.</param>
            /// <param name="widthMax">The maximum width of rectangle.</param>
            /// <returns>
            /// A list of <see cref="Geometry2D.Polygon"/> objects, each representing a rectangular bar centered at a grid point.
            /// </returns>
            public static List<Polygon> MultiRects(GridInfo2D centers,
                double widthMin, double widthMax)
            {
                long n = centers.Rows * centers.Cols;
                var (cy, cx) = centers.GetAllCoordinates();
                double dwy = (widthMax - widthMin) / (centers.Cols - 1);
                double dwx = (widthMax - widthMin) / (centers.Rows - 1);
                VectorD wy = new(count: n, mode: ArrayInitMode.Malloc);
                VectorD wx = new(count: n, mode: ArrayInitMode.Malloc);
                for (long i = 0; i < n; i++)
                {
                    long iCol = i / centers.Cols;
                    long iRow = i % centers.Cols;
                    wy[i, false] = widthMin + iCol * dwy;
                    wx[i, false] = widthMin + iRow * dwx;
                }

                return Polygon.CreateRectPolygons(
                    centerY: cy, centerX: cx,
                    widthY: wy, widthX: wx);
            }



            /// <summary>
            /// Generates a list of contact square hole near to bar polygons centered at each grid point in the specified grid.
            /// Each rectangle has the specified width in the x and y directions.
            /// </summary>
            /// <param name="centers">The grid containing the center coordinates for each rectangle.</param>
            /// <param name="widthHole">The width of the contact square hole.</param>
            /// <param name="widthBarY"> The width of the bar(in the y-direction).</param>
            /// <param name="widthBarX"> The width of the bar(in the x-direction).</param>
            /// <returns>
            /// A list of <see cref="Geometry2D.Polygon"/> objects, each representing a rectangle centered at a grid point.
            /// </returns>
            public static List<Polygon> MultiHoleBars(GridInfo2D centers,
                double widthHole, double widthBarY, double widthBarX)
            {
                long n = centers.Rows * centers.Cols;
                var (cy, cx) = centers.GetAllCoordinates();
                VectorD wh = new(count: n, initVal: widthHole);
                VectorD wy = new(count: n, initVal: widthBarY);
                VectorD wx = new(count: n, initVal: widthBarX);

                return Polygon.CreateCombineRectPolygons(
                    centerY: cy, centerX: cx,
                    width1Y: wh, width1X: wh,
                    width2Y: wy, width2X: wx);
            }


            /// <summary>
            /// Generates a list of crossed bar in near contact bar polygons centered at each grid point in the specified grid.
            /// Each rectangle has the specified width in the x and y directions.
            /// </summary>
            /// <param name="centers">The grid containing the center coordinates for each rectangle.</param>
            /// <param name="widthBarY"> The width of the bar(in the y-direction).</param>
            /// <param name="widthBarX"> The width of the bar(in the x-direction).</param>
            /// <param name="widthCrossBarY"> The width of the cross bar(in the y-direction).</param>
            /// <param name="widthCrossBarX"> The width of the cross bar(in the x-direction).</param>
            /// <returns>
            /// A list of <see cref="Geometry2D.Polygon"/> objects, each representing a rectangle centered at a grid point.
            /// </returns>
            public static List<Polygon> MultiCrossBars(GridInfo2D centers,
                 double widthBarY, double widthBarX,
                 double widthCrossBarY, double widthCrossBarX)
            {
                long n = centers.Rows * centers.Cols;
                var (cy, cx) = centers.GetAllCoordinates();
                VectorD wby = new(count: n, initVal: widthBarY);
                VectorD wbx = new(count: n, initVal: widthBarX);
                VectorD wcy = new(count: n, initVal: widthCrossBarY);
                VectorD wcx = new(count: n, initVal: widthCrossBarX);

                return Polygon.CreateCombineRectPolygons(
                    centerY: cy, centerX: cx,
                    width1Y: wby, width1X: wbx,
                    width2Y: wcy, width2X: wcx);
            }


            /// <summary>
            /// Generates a list of corner structures polygons centered at each grid point in the specified grid.
            /// Each polygon has the specified width in the x and y directions.
            /// </summary>
            /// <param name="centers">The grid containing the center coordinates for each polygon.</param>
            /// <param name="widthX"> The minimum width of the corner pattern(in the x-direction).</param>
            /// <param name="spacing"> spacing between patterns.</param>
            /// <returns>
            /// A list of <see cref="Geometry2D.Polygon"/> objects, each representing a polygon centered at a grid point.
            /// </returns>
            public static List<Polygon> MultiCorners(GridInfo2D centers,
                          double widthX, double spacing)
            {
                long n = centers.Rows * centers.Cols;
                var (cy, cx) = centers.GetMainDiagonal();
                VectorD wx = new(count: n, initVal: widthX);

                return Polygon.CreateCornerPolygons(
                    centerY: cy, centerX: cx, widthX: wx, spacing: spacing);
            }

        }

    }


}
