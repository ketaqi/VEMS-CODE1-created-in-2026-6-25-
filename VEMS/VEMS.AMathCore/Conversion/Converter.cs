namespace VEMS.AMathCore
{

    /// <summary>
    /// static converter class
    /// </summary>
    public class Converter
    {
        #region number => string

        /// <summary>
        /// converts a number to string
        /// </summary>
        /// <param name="x"> input real-valued number </param>
        /// <param name="numFormat"> numeric format used for conversion </param>
        /// <param name="digits"> number of digits, either total digits or decimal digits </param>
        /// <param name="culture"> culture selected for conversion </param>
        /// <returns> converted result as a string </returns>
        public static string NumberToString(Real x,
            NumericFormat? numFormat = null,
            int? digits = null,
            CultureName? culture = null)
        {
            // creates a converter
            NumberConverter numConv = new(numFormat: numFormat, digits: digits,
                culture: culture);
            // return
            return numConv.ToString(x);
            //// default values
            //numFormat ??= UserSetting.NumberFormat;
            //digits ??= UserSetting.NumberOfDigits;
            //culture ??= UserSetting.CultureName;
            //// defines format
            //string format = NumericFormatToString(f: numFormat) + digits.ToString();
            //// defines culture
            //CultureInfo cultureInfo = new (name: culture);
            //// converts
            //return x.ToString(format: format, provider: cultureInfo);
        }

        /// <summary>
        /// converts a number to string
        /// </summary>
        /// <param name="x"> input complex-valued number </param>
        /// <param name="numFormat"> numeric format </param>
        /// <param name="digits"> number of digits, either total digits or decimal digits </param>
        /// <param name="culture"> culture selected for conversion </param>
        /// <param name="cplxFormat"> complex number format </param>
        /// <returns> converted result as in string </returns>
        public static string NumberToString(Cplx x,
            NumericFormat? numFormat = null,
            int? digits = null,
            CultureName? culture = null,
            ComplexFormat? cplxFormat = null)
        {
            // creates a converter
            NumberConverter numConv = new(numFormat: numFormat, digits: digits,
                culture: culture);
            // return
            return numConv.ToString(x, cplxFormat: cplxFormat);
            //// default values
            //numFormat ??= UserSetting.NumberFormat;
            //digits ??= UserSetting.NumberOfDigits;
            //culture ??= UserSetting.CultureName;
            //// defines format
            //string format = NumericFormatToString(f: numFormat) + digits.ToString();
            //// separates parts
            //(double x0, double x1) = cplxFormat switch
            //{
            //    ComplexFormat.MagnitudeAndPhase => (x.Magnitude, x.Phase),
            //    ComplexFormat.RealAndImaginary => (x.Real, x.Imaginary),
            //    _ => (x.Real, x.Imaginary)
            //};
            //// converts each part
            //string x0t = NumberToString(x: x0, numFormat: numFormat, digits: digits, culture: culture);
            //string x1t = NumberToString(x: x1, numFormat: numFormat, digits: digits, culture: culture);
            //// combines parts
            //string t = cplxFormat switch
            //{
            //    ComplexFormat.MagnitudeAndPhase => $"{x0t} * Exp[i({x1t})]",
            //    ComplexFormat.RealAndImaginary => $"{x0t} + i({x1t})",
            //    _ => $"{x0t} + i({x1t})"
            //};
            //return t;
        }

        #endregion
        #region degree <==> radian

        /// <summary>
        /// converts an angle from degree to radian
        /// </summary>
        /// <param name="angle"> angle given in degree </param>
        /// <returns> angle converted into radian </returns>
        public static double Degree2Radian(double angle)
            => Math.PI * angle / 180.0;

        /// <summary>
        /// converts angles from degree to radian
        /// </summary>
        /// <param name="angle"> angles given in degree </param>
        /// <returns> angles converted into radian </returns>
        public static Vect<Real> Degree2Radian(Vect<Real> angle)
        //=> angle * (Math.PI / 180.0); //VMath.Scale(angle, Math.PI / 180.0);
        {
            Vect<Real> result = new (source: angle, copyMode: ArrayCopyMode.Deep);
            VMath.Scal(a: Math.PI / 180.0, x: result);
            return result;
        }

        /// <summary>
        /// converts an angle from radian to degree
        /// </summary>
        /// <param name="angle"> angle givne in radian </param>
        /// <returns> angle converted into degree </returns>
        public static double Radian2Degree(double angle)
            => 180.0 * angle / Math.PI;

        /// <summary>
        /// converts angles from radian to degree
        /// </summary>
        /// <param name="angle"> angles givne in radian </param>
        /// <returns> angles converted into degree </returns>
        public static Vect<Real> Radian2Degree(Vect<Real> angle)
        //=> angle * (180.0 / Math.PI); //VMath.Scale(angle, 180.0 / Math.PI);
        {
            Vect<Real> result = new (source: angle, copyMode: ArrayCopyMode.Deep);
            VMath.Scal(a: 180.0 / Math.PI, x: result);
            return result;
        }

        #endregion
        #region Cartesian <==> polar

        /// <summary>
        /// converts Cartesian coordinates (x, y) to 
        /// polar coordinates (r, theta)
        /// </summary>
        /// <param name="x"> Cartesian coordinate x </param>
        /// <param name="y"> Cartesian coordinate y </param>
        /// <returns> polar coordinates (r, theta) </returns>
        public static (Real, Real) Cartesian2Polar(Real x, Real y)
        {
            Real r = Math.Sqrt(x * x + y * y);
            Real theta = Math.Atan2(y, x);
            return (r, theta);
        }

        /// <summary>
        /// converts polar coordinates (r, theta) to 
        /// Cartesian coordinates (x, y)
        /// </summary>
        /// <param name="r"> radial coordinate r </param>
        /// <param name="theta"> azimuthal coordiante theta </param>
        /// <returns> Cartesian coordiantes (x, y) </returns>
        public static (Real, Real) Polar2Cartesian(Real r, Real theta)
        {
            Real x = r * Math.Cos(theta);
            Real y = r * Math.Sin(theta);
            return (x, y);
        }

        #endregion
        #region Vector <==> Matrix

        ///// <summary>
        ///// converts a matrix to vector form
        ///// </summary>
        ///// <param name="x"> input matrix </param>
        ///// <param name="deepCopy"> whether to make a deep copy or not </param>
        ///// <returns> resulting vector </returns>
        //public static VectorD ToVector(MatrixD x, bool deepCopy = true)
        //    => new(other: x, deepCopy: deepCopy);

        ///// <summary>
        ///// converts a matrix to vector form
        ///// </summary>
        ///// <param name="x"> input matrix </param>
        ///// <param name="deepCopy"> whether to make a deep copy or not </param>
        ///// <returns> resulting vector </returns>
        //public static VectorZ ToVector(MatrixZ x, bool deepCopy = true)
        //    => new(other: x, deepCopy: deepCopy);

        ///// <summary>
        ///// converts a vector to matrix form
        ///// </summary>
        ///// <param name="x"> input vector </param>
        ///// <param name="rows"> number of rows of the target matrix </param>
        ///// <param name="cols"> number of columns of the target matrix </param>
        ///// <param name="deepCopy"> whether to make a deep copy or not </param>
        ///// <returns> resulting matrix </returns>
        //public static MatrixD ToMatrix(VectorD x, long rows, long cols,
        //    bool deepCopy = true)
        //    => new(other: x, rows: rows, cols: cols, deepCopy: deepCopy);

        ///// <summary>
        ///// converts a vector to matrix form
        ///// </summary>
        ///// <param name="x"> input vector </param>
        ///// <param name="rows"> number of rows of the target matrix </param>
        ///// <param name="cols"> number of columns of the target matrix </param>
        ///// <param name="deepCopy"> whether to make a deep copy or not </param>
        ///// <returns> resulting matrix </returns>
        //public static MatrixZ ToMatrix(VectorZ x, long rows, long cols,
        //    bool deepCopy = true)
        //    => new(other: x, rows: rows, cols: cols, deepCopy: deepCopy);

        #endregion
        #region Cartesian 2D transform

        /// <summary>
        /// coordinate transform in 2D Cartesian system 
        /// </summary>
        /// <param name="x"> input x-coordinate </param>
        /// <param name="y"> input y-coordinate </param>
        /// <param name="x0"> shift along x-direction </param>
        /// <param name="y0"> shift along y-direction </param>
        /// <param name="rotation"> rotation angle [rad] after shift </param>
        /// <returns> (x', y') as the new coordinates </returns>
        public static (Real, Real) CartesianTransform(Real x, Real y,
            Real x0 = 0.0, Real y0 = 0.0,
            Real rotation = 0.0)
        {
            Real xp = (x0 == 0.0) ? x : x - x0;
            Real yp = (y0 == 0.0) ? y : y - y0;

            if (rotation != 0.0)
            {
                Real xt = xp;
                Real yt = yp;
                xp = Math.Cos(rotation) * xt + Math.Sin(rotation) * yt;
                yp = -Math.Sin(rotation) * xt + Math.Cos(rotation) * yt;
            }
            return (xp, yp);
        }


        #endregion
        #region character separator => char

        /// <summary>
        /// converts column separator from given name to char
        /// </summary>
        /// <param name="separatorName"> name of the column separator </param>
        /// <returns> corresponding char for the column separator </returns>
        public static char SeparatorToChar(CharSeparator? separatorName)
        {
            return separatorName switch
            {
                CharSeparator.Tab => '\t',
                CharSeparator.Comma => ',',
                CharSeparator.Space => ' ',
                CharSeparator.SemiColon => ';',
                _ => '\t',
            };
        }

        #endregion
        #region index => int/long

        /// <summary>
        /// converts an Index to integer [Int64]
        /// </summary>
        /// <param name="i"> input index </param>
        /// <param name="count"> total count </param>
        /// <returns> index in integer form </returns>
        public static long IndexToInt(Index i, long count)
        {
            if (i.IsFromEnd) { return (count - i.Value); }
            else { return i.Value; }
        }

        #endregion

    }



}
