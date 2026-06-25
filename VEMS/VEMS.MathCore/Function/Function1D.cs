namespace VEMS.MathCore
{

    /// <summary>
    /// collection of typical one-dimentional functions
    /// </summary>
    public class Function1D
    {
        #region ---- Constant ----

        /// <summary>
        /// kernel of constant function
        /// </summary>
        /// <param name="x"> variable x </param>
        /// <param name="scaling"> constant magnitude scaling factor </param>
        /// <returns> result constant value </returns>
        public static double Const(double x,
            double scaling = 1.0)
            => scaling;

        /// <summary>
        /// kernel of constant function
        /// </summary>
        /// <param name="x"> variable x </param>
        /// <returns> result: 1.0 </returns>
        public static double Const(double x)
            => 1.0;

        /// <summary>
        /// constant function, with
        /// <para> variable: x; </para> 
        /// <para> function: f(x) = 1.0 </para>
        /// </summary>
        public static readonly Func<double, List<double>?,
            double> Constant = (x, p) => Const(x);

        #endregion
        #region ---- Linear ----

        /// <summary>
        /// kernel of linear function 
        /// </summary>
        /// <param name="x"> variable </param>
        /// <param name="a"> linear coefficient </param>
        /// <param name="b"> constant offset </param>
        /// <param name="x0"> lateral shift along x </param>
        /// <returns> result y = a*x + b </returns>
        public static double Lin(double x, double a,
            double b = 0.0, double x0 = 0.0)
            => a * (x-x0) + b;

        /// <summary>
        /// kernel of linear function
        /// </summary>
        /// <param name="x"> variable </param>
        /// <param name="a"> linear (1st-order) coefficient </param>
        /// <returns> result: f = a * x </returns>
        public static double Lin(double x, double a)
            => a * x;

        /// <summary>
        /// linear function, with
        /// <para> variable: t; </para>
        /// <para> parameter #1: a - linear (1st-order) coefficient; </para>
        /// <para> function: f(x) = a * x </para>
        /// </summary>
        public static readonly Func<double, List<double>?,
            double> Linear = (x, p) => Lin(x, a: p?[0] ?? 1.0);

        #endregion
        #region ---- Quadratic ----

        /// <summary>
        /// kernel of quadratic function
        /// </summary>
        /// <param name="x"> variable </param>
        /// <param name="a"> quadratic (2nd-order) coefficient </param>
        /// <param name="b"> constant offset </param>
        /// <param name="x0"> lateral shift along x </param>
        /// <returns> y = a*x^2 + b </returns>
        public static double Quad(double x, double a,
            double b = 0.0, double x0 = 0.0)
            => a * (x-x0) * (x-x0) + b;

        /// <summary>
        /// kernel of quadratic function
        /// </summary>
        /// <param name="x"> variable </param>
        /// <param name="a"> quadratic (2nd-order) coefficient </param>
        /// <returns> result: f = a * x^2 </returns>
        public static double Quad(double x, double a)
            => a * x * x;

        /// <summary>
        /// quadratic function, with
        /// <para> variable: x; </para>
        /// <para> parameter #1: a - quadratic (2nd-order) coefficient; </para>
        /// <para> function: f(x) = a * x^2 </para>
        /// </summary>
        public static readonly Func<double, List<double>?,
            double> Quadratic = (x, p) => Quad(x, a: p?[0] ?? 1.0);

        #endregion
        #region ---- Cylindric ----

        /// <summary>
        /// kernel of cylindrical function
        /// </summary>
        /// <param name="x"> variable </param>
        /// <param name="z"> distance from point (line) source </param>
        /// <param name="c"> cylindrical coefficient </param>
        /// <param name="b"> constant offset </param>
        /// <param name="x0"> lateral shift along x </param>
        /// <returns> y = c * Sqrt[x^2 + z^2] + b </returns>
        public static double Cylind(double x, double z, double c, 
            double b = 0.0, double x0 = 0.0)
            => Math.Sign(z) * c * Math.Sqrt((x-x0) * (x-x0) + z * z) + b;

        /// <summary>
        /// kernel of cylindrical function
        /// </summary>
        /// <param name="x"> variable </param>
        /// <param name="z"> distance from point (line) source </param>
        /// <param name="c"> cylindrical coefficient </param>
        /// <returns> result: f = c * Sqrt[x^2 + z^2] + b </returns>
        public static double Cylind(double x, double z, double c)
            => Math.Sign(z) * c * Math.Sqrt(x * x + z * z);

        /// <summary>
        /// cylindrical function, with
        /// <para> variable: x; </para>
        /// <para> parameter #1: z - distance from point (line) source, can be negative; </para>
        /// <para> parameter #2: c - cylindrical coefficient </para>
        /// <para> function: f(x) = c * Sqrt[x^2 + z^2] </para>
        /// </summary>
        public static readonly Func<double, List<double>?,
            double> Cylindric = (x, p) => Cylind(x, z: p?[0] ?? 1.0, c: p?[1] ?? 1.0);

        #endregion
        #region ---- Gaussian ----

        /// <summary>
        /// fundamental Gaussian function
        /// </summary>
        /// <param name="x"> variable x </param>
        /// <param name="waist"> waist radius of the Gaussian </param>
        /// <param name="x0"> lateral shift along x </param>
        /// <param name="scaling"> constant magnitude scaling factor </param>
        /// <returns> value of the Gaussian function at given x </returns>
        public static double Gauss(double x, double waist,
            double x0 = 0.0, double scaling = 1.0)
        {
            double xp = x - x0; // shift
            return scaling * Math.Exp(-xp * xp / (waist * waist));
        }

        /// <summary>
        /// fundamental Gaussian function
        /// </summary>
        /// <param name="x"> variable x </param>
        /// <param name="w"> waist radius of the Gaussian </param>
        /// <returns> result: f = Exp(-x^2/w^2) </returns>
        public static double Gauss(double x, double w)
            => Math.Exp(-x * x / (w * w));

        /// <summary>
        /// fundamental Gaussian function, with
        /// <para> variable: x; </para>
        /// <para> parameter #1: w - waist radius of the Gaussian; </para>  
        /// <para> function: f(x) = Exp(-x^2/w^2) </para>
        /// </summary>
        public static readonly Func<double, List<double>?,
            double> Gaussian = (x, p) => Gauss(x, w: p?[0] ?? 1.0);

        #endregion
        #region ---- Super Gaussian ----

        /// <summary>
        /// super Gaussian function
        /// </summary>
        /// <param name="x"> variable x </param>
        /// <param name="waist"> waist radius of the Gaussian </param>
        /// <param name="n"> order of the super Gaussian </param>
        /// <param name="x0"> lateral shift along x </param>
        /// <param name="scaling"> constant magnitude scaling factor </param>
        /// <returns> value of the super Gaussian function at given x </returns>
        public static double SuperGauss(double x, double waist, double n,
            double x0 = 0.0, double scaling = 1.0)
        {
            double xp = x - x0; // shift
            return scaling * Math.Exp(-Math.Pow(Math.Abs(xp/waist), n));
        }

        /// <summary>
        /// super Gaussian function
        /// </summary>
        /// <param name="x"> variable x </param>
        /// <param name="w"> waist radius of the Gaussian </param>
        /// <param name="n"> order of the super Gaussian </param>
        /// <returns> result: f = Exp[-(x/w)^n] </returns>
        public static double SuperGauss(double x, double w, double n)
            => Math.Exp(-Math.Pow(Math.Abs(x/w), n));

        /// <summary>
        /// super Gaussian function, with
        /// <para> variable: x; </para>
        /// <para> parameter #1: w - waist radius of the Gaussian; </para>
        /// <para> parameter #2: n - order of the super Gaussian; </para>
        /// <para> function: f(x) = Exp[-(x/w)^n] </para>
        /// </summary>
        public static readonly Func<double, List<double>?,
            double> SuperGaussian = (x, p) => SuperGauss(x, w: p?[0] ?? 1.0, n: p?[1] ?? 2.0);

        #endregion
        #region ---- Rect ----

        /// <summary>
        /// rectangular function
        /// </summary>
        /// <param name="x"> variable x </param>
        /// <param name="width"> full width of the rectangle </param>
        /// <param name="x0"> lateral shift along x </param>
        /// <param name="scaling"> constant amplitude scaling factor </param>
        /// <returns> value of the rectangle function at given x </returns>
        public static double Rect(double x, double width,
            double x0 = 0.0, double scaling = 1.0)
        {
            double y = 0.0;
            double xp = Math.Abs(x - x0);

            if (xp <= 0.5 * width)
                y = 1.0;

            return (scaling * y);
        }

        /// <summary>
        /// kernel of rectangular function
        /// </summary>
        /// <param name="x"> variable x </param>
        /// <param name="d"> diameter (full width) of the rectangle </param>
        /// <returns> result: f = Rect(x; d) </returns>
        public static double Rect(double x, double d)
            => (Math.Abs(x) <= 0.5 * d) ? 1.0 : 0.0;

        /// <summary>
        /// rectangule function, with
        /// <para> variable: x; </para> 
        /// <para> parameter #1: d - diameter (full width) of the rectangle; </para>
        /// <para> function: f(x) = Rect(x; d) </para>
        /// </summary>
        public static readonly Func<double, List<double>?,
            double> Rectangle = (x, p) => Rect(x, d: p?[0] ?? 1.0);

        #endregion
        #region ---- ConsineEdgeRect ----

        /// <summary>
        /// rectangular function with optional cosine-smoothed 
        /// edges on both sides
        /// </summary>
        /// <param name="x"> variable x </param>
        /// <param name="width"> full width of the rectangle (when there is not smooth edges) </param>
        /// <param name="edgeWidth"> absolute edge width (half within, half outside) </param>
        /// <param name="x0"> lateral shift along x </param>
        /// <param name="scaling"> constant amplitude scaling factor </param>
        /// <returns> value of the rectangle function at given x </returns>
        public static double CosEdgeRect(double x, double width,
            double edgeWidth = 0.0,
            double x0 = 0.0, double scaling = 1.0)
        {
            double y = 0.0;
            double xp = Math.Abs(x - x0);

            if (xp <= 0.5 * width - 0.5 * edgeWidth)
                y = 1.0;
            else if (xp <= 0.5 * width + 0.5 * edgeWidth)
                y = 0.5 * (Math.Cos(Math.PI * (xp - 0.5 * width + 0.5 * edgeWidth) / edgeWidth) + 1.0);

            return (scaling * y);
        }

        /// <summary>
        /// kernel of rectangular function with cosine-smoothed edges
        /// </summary>
        /// <param name="x"> variable x </param>
        /// <param name="d"> diameter (full width) of the rectangle (when there is not smooth edges) </param>
        /// <param name="e"> absolute edge width (half within, half outside) </param>
        /// <returns> result: rectangular function with cosine-smoothed edges </returns>
        public static double CosEdgeRect(double x, double d, double e = 0.0)
        {
            double y = 0.0;
            double xp = Math.Abs(x);
            if (xp <= 0.5 * (d - e))
            { y = 1.0; }
            else if (xp <= 0.5 * (d + e))
            { y = 0.5 * (Math.Cos(Math.PI * (xp - 0.5 * d + 0.5 * e) / e) + 1.0); }
            return y;
        }

        /// <summary>
        /// rectangule function with cosine smooth edges, with
        /// <para> variable: x; </para>
        /// <para> parameter #1: d - diameter (full width) of the rectangle; </para>
        /// <para> parameter #2: e - absolute edge width (half within, half outside); </para>
        /// <para> function: f(x) </para>
        /// </summary>
        public static readonly Func<double, List<double>?,
            double> CosEdgeRectangle = (x, p) => CosEdgeRect(x, d: p?[0] ?? 1.0, e: p?[1] ?? 0.0);

        #endregion
        #region ---- GaussianEdgeRect ----

        /// <summary>
        /// rectangular function with optional Gaussian smoothed
        /// edge on both sides
        /// </summary>
        /// <param name="x"> variable x </param>
        /// <param name="width"> full width of the rectangle (when there is not smooth edges) </param>
        /// <param name="edgeWidth"> absolute edge width (half within width, half outside) </param>
        /// <param name="x0"> lateral shift along x </param>
        /// <param name="scaling"> constant amplitude scaling factor </param>
        /// <returns> value of the rectangle function at given x </returns>
        public static double GaussEdgeRect(double x, double width,
            double edgeWidth = 0.0,
            double x0 = 0.0, double scaling = 1.0)
        {
            double y;
            double xp = Math.Abs(x - x0);

            if (xp <= 0.5 * width - 0.5 * edgeWidth)
                y = 1.0;
            else
            {
                double xc = 0.5 * width - 0.5 * edgeWidth;
                double dx = xp - xc;
                y = Math.Exp(-dx * dx / (edgeWidth * edgeWidth));
            }

            return (scaling * y);
        }

        /// <summary>
        /// kernel of rectangular function with Gaussian-smoothed edges
        /// </summary>
        /// <param name="x"> variable x </param>
        /// <param name="d"> diameter (full-width) of the rectangle (when there is not smooth edges) </param>
        /// <param name="e"> absolute edge width (half within width, half outside) </param>
        /// <returns> result: rectangular function with Gaussian-smoothed edges </returns>
        public static double GaussEdgeRect(double x, double d, double e = 0.0)
        {
            double y = 0.0;
            double xp = Math.Abs(x);
            if (xp <= 0.5 * (d - e))
            { y = 1.0; }
            else
            {
                double xc = 0.5 * (d - e);
                double dx = xp - xc;
                y = Math.Exp(-dx * dx / (e * e));
            }
            return y;
        }

        /// <summary>
        /// rectangular function with Gaussian smooth edges, with 
        /// <para> variable: x; </para>
        /// <para> parameter #1: d - diameter (full width) of the rectangle; </para>
        /// <para> parameter #2: e - absolute edge width (half within, half outside); </para>
        /// <para> function: f(x) </para>
        /// </summary>
        public static readonly Func<double, List<double>?,
            double> GaussEdgeRectangle = (x, p) => GaussEdgeRect(x, d: p?[0] ?? 1.0, e: p?[1] ?? 0.0);

        #endregion
        #region ---- Polynomial ----

        /// <summary>
        /// polynomial kernel
        /// </summary>
        /// <param name="x"> variable x </param>
        /// <param name="p"> list of coefficients </param>
        /// <returns> y = p[0]*x^(N-1) + p[1]*x^(N-2) + ... + p[N-2]*x + p[N-1] </returns>
        public static double Polyn(double x, List<double>? p)
        {
            if (p == null) { throw new ArgumentNullException(nameof(p)); }  
            double y = 0.0;
            for (int i = 0; i < p.Count; i++)
            {
                if (p[i] != 0.0)
                    y += p[i] * Math.Pow(x, p.Count - 1 - i);
            }
            return y;
        }

        /// <summary>
        /// polynomial kernel
        /// </summary>
        /// <param name="x"> variable x </param>
        /// <param name="p"> list of coefficients [int32] </param>
        /// <returns> y = p[0]*x^(N-1) + p[1]*x^(N-2) + ... + p[N-2]*x + p[N-1] </returns>
        public static double Polyn(double x, List<int>? p)
        {
            if (p == null) { throw new ArgumentNullException(nameof(p)); }
            double y = 0.0;
            for (int i = 0; i < p.Count; i++)
            {
                if (p[i] != 0.0)
                    y += p[i] * Math.Pow(x, p.Count - 1 - i);
            }
            return y;
        }

        /// <summary>
        /// polynomial function, with
        /// <para> variable: x; </para> 
        /// <para> parameter #1: p[0] - coefficient of x^(N-1); </para>
        /// <para> parameter #2: p[1] - coefficient of x^(N-2); </para>
        /// <para> parameter # ...; </para>
        /// <para> parameter #(N-1): p[N-2] - coefficient of x^1; </para>
        /// <para> parameter #(N-0): p[N-1] - coefficient of x^0; </para>
        /// <para> function: y = p[0]*x^(N-1) + p[1]*x^(N-2) + ... + p[N-2]*x + p[N-1] </para>
        /// </summary>
        public static readonly Func<double, List<double>?,
            double> Polynomial = (x, p) => 
            Polyn(x: x, p: p);

        #endregion
        #region ---- Radial Zernike ----

        /// <summary>
        /// computes the coefficients for the radial part 
        /// of the Zernike polynomial, with n>=m>=0
        /// </summary>
        /// <param name="n"> lower index </param>
        /// <param name="m"> upper index </param>
        /// <returns> coefficients for the polynomial </returns>
        public static int[] CoefficientsRnm(int n, int m)
        {
            int dnm = n - m;
            if (dnm < 0) { throw new ArgumentException(); }
            if (dnm % 2 == 1) { return new int[]{ 0 }; }

            int s = dnm / 2;
            int[] p = new int[n + 1];
            for (int l = 0; l <= s; l++)
            {
                int sign = (l % 2 == 0) ? 1 : -1;
                int c = sign * CommonFunction.Factorial(n - l) /
                    (CommonFunction.Factorial(l) * CommonFunction.Factorial((n + m) / 2 - l) * CommonFunction.Factorial(s - l));
                p[2 * l] = c; // re-ordered coefficient
            }
            return p;
        }

        /// <summary>
        /// radial Zernike polynomial R_n^m(\rho), with n>=m>=0
        /// </summary>
        /// <param name="rho"> radial variable (between 0 and 1) </param>
        /// <param name="n"> lower index </param>
        /// <param name="m"> upper index </param>
        /// <returns> value of R_n^m(\rho) </returns>
        public static double RnmKernel(double rho, int n, int m)
        {
            if(rho < 0.0 || rho > 1.0) { return 0.0; }
            int[] c = CoefficientsRnm(n, m);
            return Polyn(rho, c.ToList());
        }

        /// <summary>
        /// radial Zernike polynomial R_n^m(\rho), with
        /// <para> variable: rho; </para>
        /// <para> parameter #1: n - lower index; </para>
        /// <para> parameter #2: m - upper index; </para>
        /// <para> function: y = R_n^m(\rho) </para>
        /// </summary>
        public static readonly Func<double, int, int,
            double> Rnm = (rho, n, m) => RnmKernel(rho, n, m);

        #endregion
        #region ---- Cauchy ----

        /// <summary>
        /// Cauchy equation as an empirical relationship 
        /// between refractive index and wavelength 
        /// </summary>
        /// <param name="w"> wavelength in vacuum, given in [um] </param>
        /// <param name="p"> list of coefficients </param>
        /// <returns> refractive index value n </returns>
        public static double Cauch(double w, List<double>? p)
        {
            if(p == null) { throw new ArgumentNullException(nameof(p)); }
            double n = 0.0;
            for(int i = 0; i < p.Count; i++)
            {
                double wi = Math.Pow(w, 2 * i);
                n += p[i] / wi;
            }
            return n;
        }

        /// <summary>
        /// Cauchy equation, with
        /// <para> variable: w, given in [um] </para>
        /// <para> parameter #1: p[0] for w^-0 term; </para>
        /// <para> parameter #2: p[1] for w^-2 term; </para>
        /// <para> parameter #3: p[2] for w^-4 term; </para>
        /// <para> parameter # ...; </para>
        /// <para> function: refractive index value </para>
        /// </summary>
        public static readonly Func<double, List<double>?, 
            double> Cauchy = (x, p) => Cauch(w: x, p: p);

        #endregion
        #region ---- Sellmeier ----

        /// <summary>
        /// Sellmeier equation as an empirical relationship 
        /// between refractive index and wavelength 
        /// </summary>
        /// <param name="w"> wavelength in vacuum, given in [um] </param>
        /// <param name="b"> list of coefficients b_i </param>
        /// <param name="c"> list of coefficients c_i </param>
        /// <returns> refractive index value n </returns>
        public static double Sellm(double w, List<double> b, List<double> c)
        {
            double n2 = 1.0;
            double w2 = w * w;
            for(int i = 0; i < b.Count; i++)
            {
                if (b[i] != 0.0)
                    n2 += b[i] * w2 / (w2 - c[i] * c[i]);
            }
            return Math.Sqrt(n2);
        }

        /// <summary>
        /// Sellmeier equation, with
        /// <para> variable: w, given in [um]; </para>
        /// <para> parameter #1: p[0] = b[0]; </para>
        /// <para> parameter #2: p[1] = c[0]; </para>
        /// <para> parameter # ...; </para>
        /// <para> parameter #(2j): p[2j] = b[j]; </para>
        /// <para> parameter #(2j+1): p[2j+1] = c[j]; </para>
        /// <para> function: refractive index value </para>
        /// </summary>
        public static readonly Func<double, List<double>?,
            double> Sellmeier = (x, p) =>
            {
                if(p == null) { throw new ArgumentNullException(nameof(p)); }
                int n = p.Count;
                List<double> b = new();
                List<double> c = new();
                for (int i = 0; i < p.Count / 2; i++)
                {
                    b.Add(p[2 * i + 0]);
                    c.Add(p[2 * i + 1]);
                }
                return Sellm(x, b, c);
            };

        #endregion

        #region === Periodize ===

        /// <summary>
        /// makes an input function periodic according to given period
        /// </summary>
        /// <param name="f"> input function, which is not necessarily periodic 
        /// <para> variable: x; </para>
        /// <para> return: f(x) </para>
        /// </param>
        /// <param name="period"> period of the function: d </param>
        /// <returns> resulting periodic function
        /// <para> variable: x; </para>
        /// <para> return f(x) = f(x-d) </para>
        /// </returns>
        public static Func<double, double> Periodize(Func<double, double> f,
            double period)
        {
            Func<double, double> p = (double x) =>
            {
                double lowerBound = -0.5 * period;
                long m = (long)Math.Floor((x - lowerBound) / period);
                double xp = x - m * period;
                return f(xp);
            };
            return p;
        }

        #endregion
    }

}
