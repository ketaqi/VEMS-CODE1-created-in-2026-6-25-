namespace VEMS.MathCore
{

    /// <summary>
    /// collection of typical two-dimentional functions
    /// </summary>
    public class Function2D
    {

        #region ------- Ellipse region -------

        /// <summary>
        /// ellipse region 
        /// </summary>
        /// <param name="x"> variable x </param>
        /// <param name="y"> variable y </param>
        /// <param name="a"> semiaxis along x direction </param>
        /// <param name="b"> semiaxis along y direction </param>
        /// <param name="x0"> lateral shift along x direction </param>
        /// <param name="y0"> lateral shift along y direction </param>
        /// <param name="rotation"> rotation angle [rad] after shift </param>
        /// <param name="scaling"> constant magnitude scaling factor </param>
        /// <returns> ellipse region </returns>
        public static double Ellp(double x, double y,
            double a, double b,
            double x0 = 0.0, double y0 = 0.0,
            double rotation = 0.0,
            double scaling = 1.0)
        {
            // handles possible coordinate transform
            (double xp, double yp) = Converter.CartesianTransform(x, y, x0, y0, rotation);
            // converts to polar coordinate
            (double r, double theta) = Converter.Cartesian2Polar(xp, yp);
            // computes radius of the ellipse
            double bp = b * Math.Cos(theta);
            double ap = a * Math.Sin(theta);
            double radius = a * b / Math.Sqrt(bp * bp + ap * ap);
            // compares r with radius
            return Function1D.Rect(x: r, width: 2.0 * radius, scaling: scaling);
        }

        /// <summary>
        /// ellipse region
        /// </summary>
        /// <param name="x"> variable x </param>
        /// <param name="y"> variable y </param>
        /// <param name="a"> semiaxis along x direction </param>
        /// <param name="b"> semiaxis along y direction </param>
        /// <returns> result: ellipse region </returns>
        public static double Ellp(double x, double y,
            double a, double b)
        {
            // converts to polar coordinate
            (double r, double theta) = Converter.Cartesian2Polar(x, y);
            // computes radius of the ellipse
            double bp = b * Math.Cos(theta);
            double ap = a * Math.Sin(theta);
            double radius = a * b / Math.Sqrt(bp * bp + ap * ap);
            // compares r with radius
            return Function1D.Rect(x: r, d: 2.0 * radius);
        }

        /// <summary>
        /// ellipse region function, with
        /// <para> variables: x, y; </para>
        /// <para> parameter #1: a - semiaxis along x direction; </para>
        /// <para> parameter #2: b - semiaxis along y direction; </para>
        /// <para> function: ellipse region function </para>
        /// </summary>
        public static readonly Func<double, double, List<double>?,
            double> Ellipse = (x, y, p) => Ellp(x: x, y: y, a: p?[0] ?? 1.0, b: p?[1] ?? 1.0);

        #endregion
        #region ------- Cosine-edge ellipse -------

        /// <summary>
        /// ellipse region with cosine-profile soft edge
        /// </summary>
        /// <param name="x"> variable x </param>
        /// <param name="y"> variable y </param>
        /// <param name="a"> semiaxis along x direction </param>
        /// <param name="b"> semiaxis along y direction </param>
        /// <param name="edgeWidth"> absolute edge width (half within, half outside) </param>
        /// <param name="x0"> lateral shift along x direction </param>
        /// <param name="y0"> lateral shift along y direction </param>
        /// <param name="rotation"> rotation angle [rad] after shift </param>
        /// <param name="scaling"> constant magnitude scaling factor </param>
        /// <returns> ellipse region </returns>
        public static double CosEdgeEllp(double x, double y,
            double a, double b,
            double edgeWidth = 0.0,
            double x0 = 0.0, double y0 = 0.0,
            double rotation = 0.0,
            double scaling = 1.0)
        {
            // handles possible coordinate transform
            (double xp, double yp) = Converter.CartesianTransform(x, y, x0, y0, rotation);
            // converts to polar coordinate
            (double r, double theta) = Converter.Cartesian2Polar(xp, yp);
            // computes radius of the ellipse
            double bp = b * Math.Cos(theta);
            double ap = a * Math.Sin(theta);
            double radius = a * b / Math.Sqrt(bp * bp + ap * ap);
            // check r with radius
            return Function1D.CosEdgeRect(x: r, width: 2.0 * radius, edgeWidth: edgeWidth, scaling: scaling);
        }

        /// <summary>
        /// ellipse region with cosine-smoothed edges
        /// </summary>
        /// <param name="x"> variable x </param>
        /// <param name="y"> variable y </param>
        /// <param name="a"> semiaxis along x direction </param>
        /// <param name="b"> semiaxis along y direction </param>
        /// <param name="e"> absolute edge width (half within, half outside) </param>
        /// <returns> result: ellipse region with cosine-smoothed edges </returns>
        public static double CosEdgeEllp(double x, double y,
            double a, double b, double e = 0.0)
        {
            // converts to polar coordinate
            (double r, double theta) = Converter.Cartesian2Polar(x, y);
            // computes radius of the ellipse
            double bp = b * Math.Cos(theta);
            double ap = a * Math.Sin(theta);
            double radius = a * b / Math.Sqrt(bp * bp + ap * ap);
            // check r with radius
            return Function1D.CosEdgeRect(x: r, d: 2.0 * radius, e: e);
        }

        /// <summary>
        /// ellipse region with cosine-profile soft edge, with
        /// <para> variables: x, y; </para>
        /// <para> parameter #1: a - semiaxis along x direction; </para>
        /// <para> parameter #2: b - semiaxis along y direction; </para>
        /// <para> parameter #3: e - absolute edge width (half within, half outside) </para>
        /// <para> function: ellipse region with cosine-smoothed edges </para>
        /// </summary>
        public static readonly Func<double, double, List<double>?,
            double> CosEdgeEllipse = (x, y, p) => 
            CosEdgeEllp(x, y, a: p?[0] ?? 1.0, b: p?[1] ?? 1.0, e: p?[2] ?? 0.0);

        #endregion
        #region ------- Gaussian-edge ellipse -------

        /// <summary>
        /// ellipse region with Gaussian-profile soft edge
        /// </summary>
        /// <param name="x"> variable x </param>
        /// <param name="y"> variable y </param>
        /// <param name="a"> semiaxis along x direction </param>
        /// <param name="b"> semiaxis along y direction </param>
        /// <param name="edgeWidth"> absolute edge width (half within, half outside) </param>
        /// <param name="x0"> lateral shift along x direction </param>
        /// <param name="y0"> lateral shift along y direction </param>
        /// <param name="rotation"> rotation angle [rad] after shift </param>
        /// <param name="scaling"> constant magnitude scaling factor </param>
        /// <returns> ellipse region </returns>
        public static double GaussEdgeEllp(double x, double y,
            double a, double b,
            double edgeWidth = 0.0,
            double x0 = 0.0, double y0 = 0.0,
            double rotation = 0.0,
            double scaling = 1.0)
        {
            // handles possible coordinate transform
            (double xp, double yp) = Converter.CartesianTransform(x, y, x0, y0, rotation);
            // converts to polar coordinate
            (double r, double theta) = Converter.Cartesian2Polar(xp, yp);
            // computes radius of the ellipse
            double bp = b * Math.Cos(theta);
            double ap = a * Math.Sin(theta);
            double radius = a * b / Math.Sqrt(bp * bp + ap * ap);
            // check r with radius
            return Function1D.GaussEdgeRect(x: r, width: 2.0 * radius, edgeWidth: edgeWidth, scaling: scaling);
        }

        /// <summary>
        /// ellipse region with Gaussian-smoothed edges
        /// </summary>
        /// <param name="x"> variable x </param>
        /// <param name="y"> variable y </param>
        /// <param name="a"> semiaxis along x direction </param>
        /// <param name="b"> semiaxis along y direction </param>
        /// <param name="e"> absolute edge width (half within, half outside) </param>
        /// <returns> result: ellipse region with Gaussian-smoothed edges </returns>
        public static double GaussEdgeEllp(double x, double y,
            double a, double b, double e = 0.0)
        {
            // converts to polar coordinate
            (double r, double theta) = Converter.Cartesian2Polar(x, y);
            // computes radius of the ellipse
            double bp = b * Math.Cos(theta);
            double ap = a * Math.Sin(theta);
            double radius = a * b / Math.Sqrt(bp * bp + ap * ap);
            // check r with radius
            return Function1D.GaussEdgeRect(x: r, d: 2.0 * radius, e: e);
        }

        /// <summary>
        /// ellipse region with Gaussian-smoothed edges, with
        /// <para> variables: x, y; </para>
        /// <para> parameter #1: a - semiaxis along x direction; </para>
        /// <para> parameter #2: b - semiaxis along y direction; </para>
        /// <para> parameter #3: e - absolute edge width (half within, half outside) </para>
        /// <para> function: ellipse region with Gaussian-smoothed edges </para>
        /// </summary>
        public static readonly Func<double, double, List<double>?,
            double> GaussEdgeEllipse = (x, y, p) => 
            CosEdgeEllp(x, y, a: p?[0] ?? 1.0, b: p?[1] ?? 1.0, e: p?[2] ?? 0.0);

        #endregion

        #region ------- Linear -------

        /// <summary>
        /// kernel of 2D linear function
        /// </summary>
        /// <param name="x"> variable x </param>
        /// <param name="y"> variable y </param>
        /// <param name="ax"> linear coefficient for x </param>
        /// <param name="ay"> linear coefficient for y </param>
        /// <param name="b"> constant offset </param>
        /// <param name="x0"> lateral shift along x </param>
        /// <param name="y0"> lateral shift along y </param>
        /// <returns> result f = ax*x + ay*y + b </returns>
        public static double Lin(double x, double y,
            double ax, double ay, 
            double b = 0.0, double x0 = 0.0, double y0 = 0.0)
            => ax * (x-x0) + ay * (y-y0) + b;

        /// <summary>
        /// kernel of linear function
        /// </summary>
        /// <param name="x"> variable x </param>
        /// <param name="y"> variable y </param>
        /// <param name="ax"> linear (1st-order) coefficient along x </param>
        /// <param name="ay"> linear (1st-order) coefficient along y </param>
        /// <returns> result: y = ax * x + ay * y </returns>
        public static double Lin(double x, double y,
            double ax, double ay)
            => ax * x + ay * y;

        /// <summary>
        /// linear function, with
        /// <para> variable: x; </para>
        /// <para> variable: y; </para>
        /// <para> parameter #1: ax - linear (1st-order) coefficient along x; </para>
        /// <para> parameter #2: ay - linear (1st-order) coefficient along y; </para>
        /// <para> function: f(x, y) = ax * x + ay * y </para>
        /// </summary>
        public static readonly Func<double, double, List<double>?,
            double> Linear = (x, y, p) => Lin(x, y, ax: p?[0] ?? 1.0, ay: p?[1] ?? 1.0);

        #endregion
        #region ------- Quadratic -------

        /// <summary>
        /// kernel of 2D quadratic function
        /// </summary>
        /// <param name="x"> variable x </param>
        /// <param name="y"> variable y </param>
        /// <param name="ax"> quadratic coefficient for x </param>
        /// <param name="ay"> quadratic coefficient for y </param>
        /// <param name="b"> constant offset </param>
        /// <param name="x0"> lateral shift along x </param>
        /// <param name="y0"> lateral shift along y </param>
        /// <returns> result y = ax*x^2 + ay*y^2 + b </returns>
        public static double Quad(double x, double y,
            double ax, double ay,
            double b = 0.0, double x0 = 0.0, double y0 = 0.0)
            => ax * (x-x0) * (x-x0) + ay * (y-y0) * (y-y0) + b;

        /// <summary>
        /// kernel of quadratic function
        /// </summary>
        /// <param name="x"> variable x </param>
        /// <param name="y"> variable y </param>
        /// <param name="ax"> quadratic (2nd-order) coefficient along x </param>
        /// <param name="ay"> quadratic (2nd-order) coefficient along y </param>
        /// <returns> result: f = ax * x^2 + ay * y^2 </returns>
        public static double Quad(double x, double y,
            double ax, double ay)
            => ax * x*x + ay * y * y;

        /// <summary>
        /// quadratic function, with
        /// <para> variable: x; </para>
        /// <para> variable: y; </para>
        /// <para> parameter #1: ax - quadratic (2nd-order) coefficient for x; </para>
        /// <para> parameter #2: ay - quadratic (2nd-order) coefficient for y; </para>
        /// <para> function: f(x, y) = ax * x^2 + ay * y^2 </para>
        /// </summary>
        public static readonly Func<double, double, List<double>?,
            double> Quadratic = (x, y, p) => Quad(x, y, ax: p?[0] ?? 1.0, ay: p?[1] ?? 1.0);

        #endregion
        #region ------- Spheric -------

        // !!! TO BE moved to rho-theta-separable function

        /// <summary>
        /// kernel of spherical function
        /// </summary>
        /// <param name="x"> variable along x </param>
        /// <param name="y"> variable along y </param>
        /// <param name="z"> distance from point source </param>
        /// <param name="c"> spherical coefficient </param>
        /// <param name="b"> constant offset </param>
        /// <param name="x0"> lateral shift along x </param>
        /// <param name="y0"> lateral shift along y </param>
        /// <returns> y = c * Sqrt[x^2 + y^2 + z^2] + b </returns>
        public static double Sph(double x, double y,
            double z, double c,
            double b = 0.0, double x0 = 0.0, double y0 = 0.0)
        {
            double xp = x - x0;
            double yp = y - y0;
            double r = Math.Sqrt(xp * xp + yp * yp + z * z);
            return Math.Sign(z) * c * r + b; ;
        }

        /// <summary>
        /// kernel of spherical function
        /// </summary>
        /// <param name="x"> variable along x </param>
        /// <param name="y"> variable along y </param>
        /// <param name="z"> distance from point source </param>
        /// <param name="c"> spherical coefficient </param>
        /// <returns> result: f = c * Sqrt[x^2 + y^2 + z^2] </returns>
        public static double Sph(double x, double y,
            double z, double c)
        {
            double r = Math.Sqrt(x * x + y * y + z * z);
            return Math.Sign(z) * c * r; ;
        }

        /// <summary>
        /// spherical function, with
        /// <para> variable: x; </para>
        /// <para> variable: y; </para>
        /// <para> parameter #1: z - distance from point source, can be negative; </para>
        /// <para> parameter #2: c - spherical coefficient; </para>
        /// <para> function: f(x, y) = c * Sqrt[x^2 + y^2 + z^2] </para>
        /// </summary>
        public static readonly Func<double, double, List<double>?,
            double> Spheric = (x, y, p) => Sph(x, y, z: p?[0] ?? 1.0, c: p?[1] ?? 1.0);

        #endregion

        #region === Periodize ===

        /// <summary>
        /// makes an input function periodic according to given periods
        /// </summary>
        /// <param name="f"> input function, which is not necessarily periodic 
        /// <para> variable: x; </para>
        /// <para> variable: y; </para>
        /// <para> return: f(x, y) </para>
        /// </param>
        /// <param name="periodX"> period along x-direction: dx </param>
        /// <param name="periodY"> period along y-direction: dy </param>
        /// <returns> resulting periodic function
        /// <para> variable: x; </para>
        /// <para> variable: y; </para>
        /// <para> return: f(x, y) = f(x-dx, y-dy) </para>
        /// </returns>
        public static Func<double, double, double> Periodize(Func<double, double, double> f,
            double periodX, double periodY)
        {
            Func<double, double, double> p = (double x, double y) =>
            {
                double lowerBoundX = -0.5 * periodX;
                double lowerBoundY = -0.5 * periodY;
                long mx = (long)Math.Floor((x - lowerBoundX) / periodX);
                long my = (long)Math.Floor((y - lowerBoundY) / periodY);
                double xp = x - mx * periodX;
                double yp = y - my * periodY;
                return f(xp, yp);
            };
            return p;
        }

        #endregion
    }



}
