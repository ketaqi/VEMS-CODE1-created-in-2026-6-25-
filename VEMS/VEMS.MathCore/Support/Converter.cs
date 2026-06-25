using System.Globalization;
using System.Text;
using Complex = System.Numerics.Complex;
using Real = System.Double;
using Int = System.Int64;

namespace VEMS.MathCore
{
    /// <summary>
    /// static converter class
    /// </summary>
    public class Converter
    {
        #region number => string

        /// <summary>
        /// converts a NumericFormat to string
        /// </summary>
        /// <param name="f"> input numeric format </param>
        /// <returns> converted string </returns>
        [Obsolete]
        public static string NumericFormatToString(NumericFormat? f)
        {
            return f switch
            {
                NumericFormat.Currency => "C",
                NumericFormat.Exponential => "E",
                NumericFormat.FixedPoint => "F",
                NumericFormat.General => "G",
                NumericFormat.Number => "N",
                NumericFormat.Percent => "P",
                _ => "G",
            };
        }

        /// <summary>
        /// converts a number to string
        /// </summary>
        /// <param name="x"> input real-valued number </param>
        /// <param name="numFormat"> numeric format used for conversion </param>
        /// <param name="digits"> number of digits, either total digits or decimal digits </param>
        /// <param name="culture"> culture selected for conversion </param>
        /// <returns> converted result as a string </returns>
        public static string NumberToString(double x,
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
        public static string NumberToString(Complex x,
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
        public static VectorD Degree2Radian(VectorD angle)
            => angle * (Math.PI / 180.0); //VMath.Scale(angle, Math.PI / 180.0);

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
        public static VectorD Radian2Degree(VectorD angle)
            => angle * (180.0 / Math.PI); //VMath.Scale(angle, 180.0 / Math.PI);

        #endregion
        #region Cartesian <==> polar

        /// <summary>
        /// converts Cartesian coordinates (x, y) to 
        /// polar coordinates (r, theta)
        /// </summary>
        /// <param name="x"> Cartesian coordinate x </param>
        /// <param name="y"> Cartesian coordinate y </param>
        /// <returns> polar coordinates (r, theta) </returns>
        public static (double, double) Cartesian2Polar(double x, double y)
        {
            double r = Math.Sqrt(x * x + y * y);
            double theta = Math.Atan2(y, x);
            return (r, theta);
        }

        /// <summary>
        /// converts polar coordinates (r, theta) to 
        /// Cartesian coordinates (x, y)
        /// </summary>
        /// <param name="r"> radial coordinate r </param>
        /// <param name="theta"> azimuthal coordiante theta </param>
        /// <returns> Cartesian coordiantes (x, y) </returns>
        public static (double, double) Polar2Cartesian(double r, double theta)
        {
            double x = r * Math.Cos(theta);
            double y = r * Math.Sin(theta);
            return (x, y);
        }

        #endregion
        #region Vector <==> Matrix

        /// <summary>
        /// converts a matrix to vector form
        /// </summary>
        /// <param name="x"> input matrix </param>
        /// <param name="deepCopy"> whether to make a deep copy or not </param>
        /// <returns> resulting vector </returns>
        public static VectorD ToVector(MatrixD x, bool deepCopy = true)
            => new(other: x, deepCopy: deepCopy);

        /// <summary>
        /// converts a matrix to vector form
        /// </summary>
        /// <param name="x"> input matrix </param>
        /// <param name="deepCopy"> whether to make a deep copy or not </param>
        /// <returns> resulting vector </returns>
        public static VectorZ ToVector(MatrixZ x, bool deepCopy = true)
            => new(other: x, deepCopy: deepCopy);

        /// <summary>
        /// converts a vector to matrix form
        /// </summary>
        /// <param name="x"> input vector </param>
        /// <param name="rows"> number of rows of the target matrix </param>
        /// <param name="cols"> number of columns of the target matrix </param>
        /// <param name="deepCopy"> whether to make a deep copy or not </param>
        /// <returns> resulting matrix </returns>
        public static MatrixD ToMatrix(VectorD x, long rows, long cols,
            bool deepCopy = true)
            => new(other: x, rows: rows, cols: cols, deepCopy: deepCopy);

        /// <summary>
        /// converts a vector to matrix form
        /// </summary>
        /// <param name="x"> input vector </param>
        /// <param name="rows"> number of rows of the target matrix </param>
        /// <param name="cols"> number of columns of the target matrix </param>
        /// <param name="deepCopy"> whether to make a deep copy or not </param>
        /// <returns> resulting matrix </returns>
        public static MatrixZ ToMatrix(VectorZ x, long rows, long cols,
            bool deepCopy = true)
            => new(other: x, rows: rows, cols: cols, deepCopy: deepCopy);

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
        public static (double, double) CartesianTransform(double x, double y,
            double x0 = 0.0, double y0 = 0.0, 
            double rotation = 0.0)
        {
            double xp = (x0 == 0.0)? x : x - x0;
            double yp = (y0 == 0.0)? y : y - y0;

            if(rotation != 0.0)
            {
                double xt = xp;
                double yt = yp;
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


    /// <summary>
    /// Provides conversion of numbers to string representations in various numeric and complex formats.
    /// </summary>
    /// <remarks>
    /// Supports formatting of real, integer, and complex numbers using specified culture, numeric format, and digit precision.
    /// </remarks>
    public class NumberConverter
    {
        #region properties

        /// <summary>
        /// Gets or sets the culture used for number conversion.
        /// </summary>
        /// <remarks>
        /// The culture determines the formatting of numbers, such as decimal separators and digit grouping, 
        /// according to regional settings.
        /// </remarks>
        public CultureInfo Culture { get; set; }

        /// <summary>
        /// Gets or sets the numeric format used for number conversion.
        /// </summary>
        /// <remarks>
        /// The numeric format determines how numbers are formatted as strings, such as currency, exponential, fixed-point, general, number, or percent.
        /// </remarks>
        public NumericFormat NumFormat { get; set; }

        /// <summary>
        /// Gets the string representation of the current <see cref="NumFormat"/> value.
        /// </summary>
        /// <remarks>
        /// Returns the standard .NET format string corresponding to the <see cref="NumericFormat"/> enumeration.
        /// For example, "C" for currency, "E" for exponential, "F" for fixed-point, "G" for general, "N" for number, and "P" for percent.
        /// </remarks>
        private string NumFormatString
        {
            get => NumFormat switch
            {
                NumericFormat.Currency => "C",
                NumericFormat.Exponential => "E",
                NumericFormat.FixedPoint => "F",
                NumericFormat.General => "G",
                NumericFormat.Number => "N",
                NumericFormat.Percent => "P",
                _ => "G",
            };
        }

        /// <summary>
        /// Gets or sets the number of digits used for the number conversion.
        /// </summary>
        /// <remarks>
        /// This property determines the precision of the formatted number output.
        /// The value specifies either the total number of digits or the number of decimal digits, depending on the numeric format.
        /// </remarks>
        public int Digits { get; set; } = Defaults.NumberOfDigits;

        /// <summary>
        /// Gets the composite format string used for number conversion.
        /// </summary>
        /// <remarks>
        /// The format string is constructed by concatenating the standard .NET format string
        /// (as determined by <see cref="NumFormatString"/>) and the number of digits specified by <see cref="Digits"/>.
        /// For example, if <see cref="NumFormat"/> is <see cref="NumericFormat.FixedPoint"/> and <see cref="Digits"/> is 3,
        /// the resulting format string will be "F3".
        /// </remarks>
        private string Format => NumFormatString + Digits.ToString();

        #endregion
        #region constructors

        /// <summary>
        /// Provides functionality for converting numbers between different formats or representations.
        /// </summary>
        /// <remarks>This class is intended for internal use and is not accessible outside the
        /// assembly.</remarks>
        internal NumberConverter() 
        {
            // default values
            Culture = CultureInfo.CurrentCulture;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NumberConverter"/> class with the specified numeric format, digit precision, and culture.
        /// </summary>
        /// <param name="numFormat">The numeric format option to use for number conversion. If <c>null</c>, the default from <see cref="MathSettings.NumberFormat"/> is used.</param>
        /// <param name="digits">The number of digits to use for formatting. If <c>null</c>, the default from <see cref="MathSettings.NumberOfDigits"/> is used.</param>
        /// <param name="culture">The culture to use for formatting. If <c>null</c>, the default from <see cref="MathSettings.Culture"/> is used.</param>
        public NumberConverter(NumericFormat? numFormat = null,
            int? digits = null,
            CultureName? culture = null)
        {
            // default values
            culture ??= MathSettings.Culture;
            numFormat ??= MathSettings.NumberFormat;
            digits ??= MathSettings.NumberOfDigits;
            // defines parameters
            string? cs = culture.ToString();
            Culture = cs != null ? new(cs.Replace('_', '-')) : CultureInfo.InvariantCulture;
            NumFormat = (NumericFormat)numFormat;
            Digits = (int)digits;
        }

        #endregion
        #region methods

        /// <summary>
        /// Converts a real number to its string representation using the current numeric format,
        /// digit precision, and culture settings.
        /// </summary>
        /// <param name="x">The input real-valued number to convert.</param>
        /// <returns>
        /// A string representation of the input real number, formatted according to the specified
        /// <see cref="NumFormat"/>, <see cref="Digits"/>, and <see cref="Culture"/>.
        /// </returns>
        public string ToString(Real x)
            => x.ToString(format: Format, provider: Culture);

        /// <summary>
        /// Converts a complex number to its string representation using the current numeric format,
        /// digit precision, culture settings, and an optional complex number format.
        /// </summary>
        /// <param name="x">The input complex-valued number to convert.</param>
        /// <param name="cplxFormat">
        /// The complex number format to use for conversion. If <c>null</c>, the default from
        /// <see cref="MathSettings.ComplexNumberFormat"/> is used.
        /// </param>
        /// <returns>
        /// A string representation of the input complex number, formatted according to the specified
        /// <see cref="NumFormat"/>, <see cref="Digits"/>, <see cref="Culture"/>, and <paramref name="cplxFormat"/>.
        /// </returns>
        [Obsolete($"Use Cplx instead")]
        public string ToString(Complex x,
            ComplexFormat? cplxFormat = null)
        {
            // default value
            cplxFormat = cplxFormat ?? MathSettings.ComplexNumberFormat;
            // separates parts
            (double x0, double x1) = cplxFormat switch
            {
                ComplexFormat.MagnitudeAndPhase => (x.Magnitude, x.Phase),
                ComplexFormat.RealAndImaginary => (x.Real, x.Imaginary),
                _ => (x.Real, x.Imaginary)
            };
            // converts each part 
            string x0t = ToString(x0);
            string x1t = ToString(x1);
            // combines parts
            string t = cplxFormat switch
            {
                ComplexFormat.MagnitudeAndPhase => $"{x0t} * Exp[i({x1t})]",
                ComplexFormat.RealAndImaginary => $"{x0t} + i({x1t})",
                _ => $"{x0t} + i({x1t})"
            };
            // return
            return t;
        }

        /// <summary>
        /// Converts a custom complex number (<see cref="Cplx"/>) to its string representation
        /// using the current numeric format, digit precision, culture settings, and an optional complex number format.
        /// </summary>
        /// <param name="x">The input <see cref="Cplx"/> complex-valued number to convert.</param>
        /// <param name="cplxFormat">
        /// The complex number format to use for conversion. If <c>null</c>, the default from
        /// <see cref="MathSettings.ComplexNumberFormat"/> is used.
        /// </param>
        /// <returns>
        /// A string representation of the input <see cref="Cplx"/> number, formatted according to the specified
        /// <see cref="NumFormat"/>, <see cref="Digits"/>, <see cref="Culture"/>, and <paramref name="cplxFormat"/>.
        /// </returns>
        public string ToString(Cplx x,
            ComplexFormat? cplxFormat = null)
        {
            // default value
            cplxFormat = cplxFormat ?? MathSettings.ComplexNumberFormat;
            // separates parts
            (double x0, double x1) = cplxFormat switch
            {
                ComplexFormat.MagnitudeAndPhase => (x.Magnitude, x.Phase),
                ComplexFormat.RealAndImaginary => (x.Real, x.Imag),
                _ => (x.Real, x.Imag)
            };
            // converts each part 
            string x0t = ToString(x0);
            string x1t = ToString(x1);
            // combines parts
            string t = cplxFormat switch
            {
                ComplexFormat.MagnitudeAndPhase => $"{x0t} * Exp[i({x1t})]",
                ComplexFormat.RealAndImaginary => $"{x0t} + i({x1t})",
                _ => $"{x0t} + i({x1t})"
            };
            // return
            return t;
        }

        /// <summary>
        /// Converts a 64-bit integer number to its string representation
        /// using the current numeric format, digit precision, and culture settings.
        /// </summary>
        /// <param name="x">The input integer number to convert.</param>
        /// <returns>
        /// A string representation of the input integer, formatted according to the specified
        /// <see cref="NumFormat"/>, <see cref="Digits"/>, and <see cref="Culture"/>.
        /// </returns>
        public string ToString(Int x)
            => x.ToString(format: Format, provider: Culture);

        #endregion
    }

    /// <summary>
    /// converter for data to string in different forms
    /// </summary>
    public class DataConverter
    {
        #region properties

        /// <summary>
        /// column separator
        /// </summary>
        public char ColumnSeparator { get; set; }

        /// <summary>
        /// number converter
        /// </summary>
        public NumberConverter NumConverter { get; set; }

        #endregion
        #region constructors

        /// <summary>
        /// constructs a DataConverter
        /// </summary>
        /// <param name="columnSeparator"> column separator option </param>
        /// <param name="numFormat"> numeric format used for conversion </param>
        /// <param name="digits"> number of digits </param>
        /// <param name="culture"> culture selected for conversion </param>
        public DataConverter(CharSeparator? columnSeparator = null,
            NumericFormat? numFormat = null,
            int? digits = null,
            CultureName? culture = null)
        {
            // default values
            columnSeparator ??= MathSettings.ColumnSeparator;
            numFormat ??= MathSettings.NumberFormat;
            digits ??= MathSettings.NumberOfDigits;
            culture ??= MathSettings.Culture;
            // sets parameters
            ColumnSeparator = Converter.SeparatorToChar(columnSeparator);
            NumConverter = new NumberConverter(numFormat: numFormat,
                digits: digits,
                culture: culture);
        }

        #endregion
        #region methods: data to string [StringBuilder]

        #region ---- Vector [D/Z] + Grid1D ----

        /// <summary>
        /// converts a real-valued vector to string 
        /// with optional grid information
        /// </summary>
        /// <param name="vs"> real-valued vector </param>
        /// <param name="grid"> optional grid information </param>
        /// <returns> converted string content </returns>
        public StringBuilder ToString(VectorD vs,
            GridInfo1D? grid = null)
        {
            // initializes content StringBuilder
            StringBuilder content = new();

            // grid info
            if (grid != null)
            { throw new NotImplementedException(); }
            // values 
            for (long i = 0; i < vs.Count; i++)
            {
                string vi = NumConverter.ToString(x: vs[i, checkBound: false]);
                content.Append(vi + ColumnSeparator);
            }
            content.Remove(startIndex: content.Length - 1, length: 1);

            // return
            return content;
        }

        /// <summary>
        /// converts a complex-valued vector to string 
        /// with optional grid information
        /// </summary>
        /// <param name="vs"> complex-valued vector </param>
        /// <param name="cplxFormat"> complex number format </param>
        /// <param name="grid"> optional grid information </param>
        /// <returns> converted string content </returns>
        public StringBuilder ToString(VectorZ vs,
            ComplexFormat? cplxFormat = null,
            GridInfo1D? grid = null)
        {
            // default value
            cplxFormat ??= MathSettings.ComplexNumberFormat;
            // initializes content StringBuilder
            StringBuilder content = new();

            // grid info
            if (grid != null)
            { throw new NotImplementedException(); }
            // values 
            for (long i = 0; i < vs.Count; i++)
            {
                string vi = NumConverter.ToString(x: vs[i, checkBound: false],
                    cplxFormat: cplxFormat);
                content.Append(vi + ColumnSeparator);
            }
            content.Remove(startIndex: content.Length - 1, length: 1);

            // return 
            return content;
        }

        #endregion
        #region ---- Matrix [D/Z] + Grid2D ----

        /// <summary>
        /// converts a real-valued matrix to string 
        /// with optional grid information
        /// </summary>
        /// <param name="vs"> real-valued vector </param>
        /// <param name="grid"> optional grid information </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <param name="reverseRows"> control the order of rows when exporting the matrix </param>
        /// <returns> converted string content </returns>
        public StringBuilder ToString(MatrixD vs,
            GridInfo2D? grid = null,
            LoopMode loopMode = Defaults.LoopOption,
            bool reverseRows = false)
        {
            // initializes content StringBuilder
            StringBuilder content = new();

            // grid info
            if (grid != null)
            { throw new NotImplementedException(); }
            // values in lines
            StringBuilder[] lines = new StringBuilder[vs.Rows];
            Action<long> a = (iRow) =>
            {
                //Control the order of rows when exporting the matrix
                long dataRow = reverseRows ? vs.Rows - iRow - 1 : iRow;

                lines[iRow] = new StringBuilder();
                for (long iCol = 0; iCol < vs.Cols; iCol++)
                {
                    string vij = NumConverter.ToString(x: vs[dataRow, iCol, checkBound: false]);
                    lines[iRow].Append(vij + ColumnSeparator);
                }
                lines[iRow].Remove(startIndex: lines[iRow].Length - 1, length: 1);
                lines[iRow].Append('\n');
            };
            Loop1D loop = new(operation: a, start: 0, end: vs.Rows, step: 1);
            loop.Evaluate(mode: loopMode);
            // connects lines
            for (long iRow = 0; iRow < vs.Rows; iRow++)
            { content.Append(lines[iRow]); }
            content.Remove(startIndex: content.Length - 1, length: 1);

            // return
            return content;
        }

        /// <summary>
        /// converts a complex-valued matrix to string 
        /// with optional grid information
        /// </summary>
        /// <param name="vs"> complex-valued vector </param>
        /// <param name="cplxFormat"> complex number format </param>
        /// <param name="grid"> optional grid information </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <param name="reverseRows"> control the order of rows when exporting the matrix </param>
        /// <returns> converted string content </returns>
        public StringBuilder ToString(MatrixZ vs,
            ComplexFormat? cplxFormat = null,
            GridInfo2D? grid = null,
            LoopMode loopMode = Defaults.LoopOption,
            bool reverseRows = false)
        {
            // default value
            cplxFormat ??= MathSettings.ComplexNumberFormat;
            // initializes content StringBuilder
            StringBuilder content = new();

            // grid info
            if (grid != null)
            { throw new NotImplementedException(); }
            // values in lines
            StringBuilder[] lines = new StringBuilder[vs.Rows];
            Action<long> a = (iRow) =>
            {
                //Control the order of rows when exporting the matrix
                long dataRow = reverseRows ? vs.Rows - iRow - 1 : iRow;

                lines[iRow] = new StringBuilder();
                for (long iCol = 0; iCol < vs.Cols; iCol++)
                {
                    string vij = NumConverter.ToString(x: vs[dataRow, iCol, checkBound: false],
                        cplxFormat: cplxFormat);
                    lines[iRow].Append(vij + ColumnSeparator);
                }
                lines[iRow].Remove(startIndex: lines[iRow].Length - 1, length: 1);
                lines[iRow].Append('\n');
            };
            Loop1D loop = new(operation: a, start: 0, end: vs.Rows, step: 1);
            loop.Evaluate(mode: loopMode);
            // connects lines
            for (long iRow = 0; iRow < vs.Rows; iRow++)
            { content.Append(lines[iRow]); }
            content.Remove(startIndex: content.Length - 1, length: 1);

            // return
            return content;
        }

        #endregion
        #region ---- Vector [D/Z] + Scat1D ----

        /// <summary>
        /// converts a real-valued vector to string 
        /// with scattered position information
        /// </summary>
        /// <param name="vs"> real-valued vector </param>
        /// <param name="xs"> scattered points </param>
        /// <returns> converted string content </returns>
        public StringBuilder ToString(VectorD vs,
            ScatInfo1D xs)
        {
            // initializes content StringBuilder
            StringBuilder content = new();

            // points and values
            for (long i = 0; i < vs.Count; i++)
            {
                string xi = NumConverter.ToString(x: xs[i, checkBound: false]);
                string vi = NumConverter.ToString(x: vs[i, checkBound: false]);
                content.Append(xi + ColumnSeparator + vi);
                content.Append('\n');
            }
            content.Remove(startIndex: content.Length - 1, length: 1);

            // return
            return content;
        }

        /// <summary>
        /// converts a complex-valued vector to string 
        /// with scattered position information
        /// </summary>
        /// <param name="vs"> complex-valued vector </param>
        /// <param name="xs"> scattered points </param>
        /// <param name="cplxFormat"> complex number format </param>
        /// <returns> converted string content </returns>
        public StringBuilder ToString(VectorZ vs,
           ScatInfo1D xs,
           ComplexFormat? cplxFormat = null)
        {
            // default value
            cplxFormat ??= MathSettings.ComplexNumberFormat;
            // initializes content StringBuilder
            StringBuilder content = new();

            // points and values 
            for (long i = 0; i < vs.Count; i++)
            {
                string xi = NumConverter.ToString(x: xs[i, checkBound: false]);
                string vi = NumConverter.ToString(x: vs[i, checkBound: false],
                    cplxFormat: cplxFormat);
                content.Append(xi + ColumnSeparator + vi);
                content.Append('\n');
            }
            content.Remove(startIndex: content.Length - 1, length: 1);

            // return
            return content;
        }

        #endregion
        #region ---- Vector [D/Z] + Scat2D ----

        /// <summary>
        /// converts a real-valued matrix to string 
        /// with scattered position information
        /// </summary>
        /// <param name="vs"> real-valued matrix </param>
        /// <param name="rhos"> scattered points </param>
        /// <returns> converted string content </returns>
        public StringBuilder ToString(VectorD vs,
            ScatInfo2D rhos)
        {
            // initializes content StringBuilder
            StringBuilder content = new();

            // points and values
            for (long i = 0; i < vs.Count; i++)
            {
                (double y, double x) = rhos[i, checkBound: false];
                string yi = NumConverter.ToString(x: y);
                string xi = NumConverter.ToString(x: x);
                string vi = NumConverter.ToString(x: vs[i, checkBound: false]);
                content.Append(xi + ColumnSeparator + yi + ColumnSeparator + vi);
                content.Append('\n');
            }
            content.Remove(startIndex: content.Length - 1, length: 1);

            // return
            return content;
        }

        /// <summary>
        /// converts a complex-valued matrix to string 
        /// with scattered position information
        /// </summary>
        /// <param name="vs"> complex-valued vector </param>
        /// <param name="rhos"> scattered points </param>
        /// <param name="cplxFormat"> complex number format </param>
        public StringBuilder ToString(VectorZ vs,
            ScatInfo2D rhos,
            ComplexFormat? cplxFormat = null)
        {
            // default value
            cplxFormat ??= MathSettings.ComplexNumberFormat;
            // initializes content StringBuilder
            StringBuilder content = new();

            // points and values
            for (long i = 0; i < vs.Count; i++)
            {
                (double y, double x) = rhos[i, checkBound: false];
                string yi = NumConverter.ToString(x: y);
                string xi = NumConverter.ToString(x: x);
                string vi = NumConverter.ToString(x: vs[i, checkBound: false],
                    cplxFormat: cplxFormat);
                content.Append(xi + ColumnSeparator + yi + ColumnSeparator + vi);
                content.Append('\n');
            }
            content.Remove(startIndex: content.Length - 1, length: 1);

            // return
            return content;
        }

        #endregion


        #region ---- Vector<T> + Grid1D ----

        /// <summary>
        /// converts a real-valued vector to string 
        /// with optional grid information
        /// </summary>
        /// <param name="vs"> real-valued vector </param>
        /// <param name="grid"> optional grid information </param>
        /// <returns> converted string content </returns>
        public StringBuilder ToString(Vector<double> vs,
            GridInfo1D? grid = null)
        {
            // initializes content StringBuilder
            StringBuilder content = new();

            // grid info
            if (grid != null)
            { throw new NotImplementedException(); }
            // values 
            for (long i = 0; i < vs.Count; i++)
            {
                string vi = NumConverter.ToString(x: vs[i, checkBound: false]);
                content.Append(vi + ColumnSeparator);
            }
            content.Remove(startIndex: content.Length - 1, length: 1);

            // return
            return content;
        }

        /// <summary>
        /// converts a complex-valued vector to string 
        /// with optional grid information
        /// </summary>
        /// <param name="vs"> complex-valued vector </param>
        /// <param name="cplxFormat"> complex number format </param>
        /// <param name="grid"> optional grid information </param>
        /// <returns> converted string content </returns>
        public StringBuilder ToString(Vector<Complex> vs,
            ComplexFormat? cplxFormat = null,
            GridInfo1D? grid = null)
        {
            // default value
            cplxFormat ??= MathSettings.ComplexNumberFormat;
            // initializes content StringBuilder
            StringBuilder content = new();

            // grid info
            if (grid != null)
            { throw new NotImplementedException(); }
            // values 
            for (long i = 0; i < vs.Count; i++)
            {
                string vi = NumConverter.ToString(x: vs[i, checkBound: false],
                    cplxFormat: cplxFormat);
                content.Append(vi + ColumnSeparator);
            }
            content.Remove(startIndex: content.Length - 1, length: 1);

            // return 
            return content;
        }

        /// <summary>
        /// converts an integer-valued vector to string 
        /// with optional grid information
        /// </summary>
        /// <param name="vs"> integer-valued vector </param>
        /// <param name="grid"> optional grid information </param>
        /// <returns> converted string content </returns>
        public StringBuilder ToString(Vector<long> vs,
            GridInfo1D? grid = null)
        {
            // initializes content StringBuilder
            StringBuilder content = new();
            // grid info
            if (grid != null)
            { throw new NotImplementedException(); }
            // values 
            for (long i = 0; i < vs.Count; i++)
            {
                string vi = NumConverter.ToString(x: vs[i, checkBound: false]);
                content.Append(vi + ColumnSeparator);
            }
            content.Remove(startIndex: content.Length - 1, length: 1);
            // return
            return content;
        }

        #endregion
        #region ---- Matrix<T> + Grid2D----

        /// <summary>
        /// converts a real-valued matrix to string 
        /// with optional grid information
        /// </summary>
        /// <param name="vs"> real-valued vector </param>
        /// <param name="grid"> optional grid information </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <param name="revRows"> control the order of rows when exporting the matrix </param>
        /// <returns> converted string content </returns>
        public StringBuilder ToString(Matrix<double> vs,
            GridInfo2D? grid = null,
            LoopMode loopMode = Defaults.LoopOption,
            bool revRows = false)
        {
            // initializes content StringBuilder
            StringBuilder content = new();

            // grid info
            if (grid != null)
            { throw new NotImplementedException(); }
            // values in lines
            StringBuilder[] lines = new StringBuilder[vs.Rows];
            Action<long> a = (iRow) =>
            {
                //Control the order of rows when exporting the matrix
                long dataRow = revRows ? vs.Rows - iRow - 1 : iRow;

                lines[iRow] = new StringBuilder();
                for (long iCol = 0; iCol < vs.Cols; iCol++)
                {
                    string vij = NumConverter.ToString(x: vs[dataRow, iCol, checkBound: false]);
                    lines[iRow].Append(vij + ColumnSeparator);
                }
                lines[iRow].Remove(startIndex: lines[iRow].Length - 1, length: 1);
                lines[iRow].Append('\n');
            };
            Loop1D loop = new(operation: a, start: 0, end: vs.Rows, step: 1);
            loop.Evaluate(mode: loopMode);
            // connects lines
            for (long iRow = 0; iRow < vs.Rows; iRow++)
            { content.Append(lines[iRow]); }
            content.Remove(startIndex: content.Length - 1, length: 1);

            // return
            return content;
        }

        /// <summary>
        /// converts a complex-valued matrix to string 
        /// with optional grid information
        /// </summary>
        /// <param name="vs"> complex-valued vector </param>
        /// <param name="cplxFormat"> complex number format </param>
        /// <param name="grid"> optional grid information </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <param name="revRows"> control the order of rows when exporting the matrix </param>
        /// <returns> converted string content </returns>
        public StringBuilder ToString(Matrix<Complex> vs,
            ComplexFormat? cplxFormat = null,
            GridInfo2D? grid = null,
            LoopMode loopMode = Defaults.LoopOption,
            bool revRows = false)
        {
            // default value
            cplxFormat ??= MathSettings.ComplexNumberFormat;
            // initializes content StringBuilder
            StringBuilder content = new();

            // grid info
            if (grid != null)
            { throw new NotImplementedException(); }
            // values in lines
            StringBuilder[] lines = new StringBuilder[vs.Rows];
            Action<long> a = (iRow) =>
            {
                //Control the order of rows when exporting the matrix
                long dataRow = revRows ? vs.Rows - iRow - 1 : iRow;

                lines[iRow] = new StringBuilder();
                for (long iCol = 0; iCol < vs.Cols; iCol++)
                {
                    string vij = NumConverter.ToString(x: vs[dataRow, iCol, checkBound: false],
                        cplxFormat: cplxFormat);
                    lines[iRow].Append(vij + ColumnSeparator);
                }
                lines[iRow].Remove(startIndex: lines[iRow].Length - 1, length: 1);
                lines[iRow].Append('\n');
            };
            Loop1D loop = new(operation: a, start: 0, end: vs.Rows, step: 1);
            loop.Evaluate(mode: loopMode);
            // connects lines
            for (long iRow = 0; iRow < vs.Rows; iRow++)
            { content.Append(lines[iRow]); }
            content.Remove(startIndex: content.Length - 1, length: 1);

            // return
            return content;
        }

        /// <summary>
        /// converts a real-valued matrix to string 
        /// with optional grid information
        /// </summary>
        /// <param name="vs"> real-valued vector </param>
        /// <param name="grid"> optional grid information </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <param name="revRows"> control the order of rows when exporting the matrix </param>
        /// <returns> converted string content </returns>
        public StringBuilder ToString(Matrix<long> vs,
            GridInfo2D? grid = null,
            LoopMode loopMode = Defaults.LoopOption,
            bool revRows = false)
        {
            // initializes content StringBuilder
            StringBuilder content = new();

            // grid info
            if (grid != null)
            { throw new NotImplementedException(); }
            // values in lines
            StringBuilder[] lines = new StringBuilder[vs.Rows];
            Action<long> a = (iRow) =>
            {
                //Control the order of rows when exporting the matrix
                long dataRow = revRows ? vs.Rows - iRow - 1 : iRow;

                lines[iRow] = new StringBuilder();
                for (long iCol = 0; iCol < vs.Cols; iCol++)
                {
                    string vij = NumConverter.ToString(x: vs[dataRow, iCol, checkBound: false]);
                    lines[iRow].Append(vij + ColumnSeparator);
                }
                lines[iRow].Remove(startIndex: lines[iRow].Length - 1, length: 1);
                lines[iRow].Append('\n');
            };
            Loop1D loop = new(operation: a, start: 0, end: vs.Rows, step: 1);
            loop.Evaluate(mode: loopMode);
            // connects lines
            for (long iRow = 0; iRow < vs.Rows; iRow++)
            { content.Append(lines[iRow]); }
            content.Remove(startIndex: content.Length - 1, length: 1);

            // return
            return content;
        }

        #endregion


        #region ---- Vect<T> + Grid1D ----

        /// <summary>
        /// Converts a real-valued vector to a string representation,
        /// with optional grid information.
        /// </summary>
        /// <param name="vs">The real-valued vector to convert.</param>
        /// <param name="grid">Optional grid information. If provided, not implemented and will throw.</param>
        /// <returns>A <see cref="StringBuilder"/> containing the string representation of the vector.</returns>
        public StringBuilder ToString(Vect<Real> vs,
            GridInfo1D? grid = null)
        {
            // initializes content StringBuilder
            StringBuilder content = new();

            // grid info
            if (grid != null)
            { throw new NotImplementedException(); }
            // values 
            for (long i = 0; i < vs.Count; i++)
            {
                string vi = NumConverter.ToString(x: vs[i, checkBound: false]);
                content.Append(vi + ColumnSeparator);
            }
            content.Remove(startIndex: content.Length - 1, length: 1);

            // return
            return content;
        }

        /// <summary>
        /// Converts a complex-valued vector to a string representation,
        /// with optional grid information.
        /// </summary>
        /// <param name="vs">The complex-valued vector to convert.</param>
        /// <param name="cplxFormat">Optional complex number format. If null, uses default from <see cref="MathSettings.ComplexNumberFormat"/>.</param>
        /// <param name="grid">Optional grid information. If provided, not implemented and will throw.</param>
        /// <returns>A <see cref="StringBuilder"/> containing the string representation of the vector.</returns>
        public StringBuilder ToString(Vect<Cplx> vs,
            ComplexFormat? cplxFormat = null,
            GridInfo1D? grid = null)
        {
            // default value
            cplxFormat ??= MathSettings.ComplexNumberFormat;
            // initializes content StringBuilder
            StringBuilder content = new();

            // grid info
            if (grid != null)
            { throw new NotImplementedException(); }
            // values 
            for (long i = 0; i < vs.Count; i++)
            {
                string vi = NumConverter.ToString(x: vs[i, checkBound: false],
                    cplxFormat: cplxFormat);
                content.Append(vi + ColumnSeparator);
            }
            content.Remove(startIndex: content.Length - 1, length: 1);

            // return 
            return content;
        }

        /// <summary>
        /// Converts an integer-valued vector to a string representation,
        /// with optional grid information.
        /// </summary>
        /// <param name="vs">The integer-valued vector to convert.</param>
        /// <param name="grid">Optional grid information. If provided, not implemented and will throw.</param>
        /// <returns>A <see cref="StringBuilder"/> containing the string representation of the vector.</returns>
        public StringBuilder ToString(Vect<Int> vs,
            GridInfo1D? grid = null)
        {
            // initializes content StringBuilder
            StringBuilder content = new();
            // grid info
            if (grid != null)
            { throw new NotImplementedException(); }
            // values 
            for (long i = 0; i < vs.Count; i++)
            {
                string vi = NumConverter.ToString(x: vs[i, checkBound: false]);
                content.Append(vi + ColumnSeparator);
            }
            content.Remove(startIndex: content.Length - 1, length: 1);
            // return
            return content;
        }

        #endregion
        #region ---- Matx<T> + Grid2D ----

        /// <summary>
        /// Converts a real-valued matrix to a string representation,
        /// with optional grid information.
        /// </summary>
        /// <param name="vs">The real-valued matrix to convert.</param>
        /// <param name="grid">Optional grid information. If provided, not implemented and will throw.</param>
        /// <param name="loopMode">Loop-computational mode options.</param>
        /// <param name="revRows">Controls the order of rows when exporting the matrix. If true, rows are reversed.</param>
        /// <returns>A <see cref="StringBuilder"/> containing the string representation of the matrix.</returns>
        public StringBuilder ToString(Matx<Real> vs,
            GridInfo2D? grid = null,
            LoopMode loopMode = Defaults.LoopOption,
            bool revRows = false)
        {
            // initializes content StringBuilder
            StringBuilder content = new();

            // grid info
            if (grid != null)
            { throw new NotImplementedException(); }
            // values in lines
            StringBuilder[] lines = new StringBuilder[vs.Rows];
            void op(long iRow)
            {
                //Control the order of rows when exporting the matrix
                long dataRow = revRows ? vs.Rows - iRow - 1 : iRow;

                lines[iRow] = new StringBuilder();
                for (long iCol = 0; iCol < vs.Cols; iCol++)
                {
                    string vij = NumConverter.ToString(x: vs[dataRow, iCol, checkBound: false]);
                    lines[iRow].Append(vij + ColumnSeparator);
                }
                lines[iRow].Remove(startIndex: lines[iRow].Length - 1, length: 1);
                lines[iRow].Append('\n');
            }
            Loop1D loop = new(operation: op, start: 0, end: vs.Rows, step: 1);
            loop.Evaluate(mode: loopMode);
            // connects lines
            for (long iRow = 0; iRow < vs.Rows; iRow++)
            { content.Append(lines[iRow]); }
            content.Remove(startIndex: content.Length - 1, length: 1);

            // return
            return content;
        }

        /// <summary>
        /// Converts a complex-valued matrix to a string representation,
        /// with optional grid information.
        /// </summary>
        /// <param name="vs">The complex-valued matrix to convert.</param>
        /// <param name="cplxFormat">Optional complex number format. If null, uses default from <see cref="MathSettings.ComplexNumberFormat"/>.</param>
        /// <param name="grid">Optional grid information. If provided, not implemented and will throw.</param>
        /// <param name="loopMode">Loop-computational mode options.</param>
        /// <param name="revRows">Controls the order of rows when exporting the matrix. If true, rows are reversed.</param>
        /// <returns>A <see cref="StringBuilder"/> containing the string representation of the matrix.</returns>
        public StringBuilder ToString(Matx<Cplx> vs,
            ComplexFormat? cplxFormat = null,
            GridInfo2D? grid = null,
            LoopMode loopMode = Defaults.LoopOption,
            bool revRows = false)
        {
            // default value
            cplxFormat ??= MathSettings.ComplexNumberFormat;
            // initializes content StringBuilder
            StringBuilder content = new();

            // grid info
            if (grid != null)
            { throw new NotImplementedException(); }
            // values in lines
            StringBuilder[] lines = new StringBuilder[vs.Rows];
            void op(long iRow)
            {
                //Control the order of rows when exporting the matrix
                long dataRow = revRows ? vs.Rows - iRow - 1 : iRow;

                lines[iRow] = new StringBuilder();
                for (long iCol = 0; iCol < vs.Cols; iCol++)
                {
                    string vij = NumConverter.ToString(x: vs[dataRow, iCol, checkBound: false],
                        cplxFormat: cplxFormat);
                    lines[iRow].Append(vij + ColumnSeparator);
                }
                lines[iRow].Remove(startIndex: lines[iRow].Length - 1, length: 1);
                lines[iRow].Append('\n');
            }
            Loop1D loop = new(operation: op, start: 0, end: vs.Rows, step: 1);
            loop.Evaluate(mode: loopMode);
            // connects lines
            for (long iRow = 0; iRow < vs.Rows; iRow++)
            { content.Append(lines[iRow]); }
            content.Remove(startIndex: content.Length - 1, length: 1);

            // return
            return content;
        }

        /// <summary>
        /// Converts an integer-valued matrix to a string representation,
        /// with optional grid information.
        /// </summary>
        /// <param name="vs">The integer-valued matrix to convert.</param>
        /// <param name="grid">Optional grid information. If provided, not implemented and will throw.</param>
        /// <param name="loopMode">Loop-computational mode options.</param>
        /// <param name="revRows">Controls the order of rows when exporting the matrix. If true, rows are reversed.</param>
        /// <returns>A <see cref="StringBuilder"/> containing the string representation of the matrix.</returns>
        public StringBuilder ToString(Matx<Int> vs,
            GridInfo2D? grid = null,
            LoopMode loopMode = Defaults.LoopOption,
            bool revRows = false)
        {
            // initializes content StringBuilder
            StringBuilder content = new();

            // grid info
            if (grid != null)
            { throw new NotImplementedException(); }
            // values in lines
            StringBuilder[] lines = new StringBuilder[vs.Rows];
            void op(long iRow)
            {
                //Control the order of rows when exporting the matrix
                long dataRow = revRows ? vs.Rows - iRow - 1 : iRow;

                lines[iRow] = new StringBuilder();
                for (long iCol = 0; iCol < vs.Cols; iCol++)
                {
                    string vij = NumConverter.ToString(x: vs[dataRow, iCol, checkBound: false]);
                    lines[iRow].Append(vij + ColumnSeparator);
                }
                lines[iRow].Remove(startIndex: lines[iRow].Length - 1, length: 1);
                lines[iRow].Append('\n');
            }
            Loop1D loop = new(operation: op, start: 0, end: vs.Rows, step: 1);
            loop.Evaluate(mode: loopMode);
            // connects lines
            for (long iRow = 0; iRow < vs.Rows; iRow++)
            { content.Append(lines[iRow]); }
            content.Remove(startIndex: content.Length - 1, length: 1);

            // return
            return content;
        }

        #endregion


        #endregion
    }

}
