using System.Text;
using Complex = System.Numerics.Complex;
using Real = System.Double;
using Int = System.Int64;

namespace VEMS.MathCore
{
    /// <summary>
    /// printer class
    /// </summary>
    public class Printer
    {
        #region default fields

        private static readonly string ErrorPrefix = " :( ";
        private static readonly string WarningPrefix = " :@ ";
        private static readonly string LoggingPrefix = " >> ";
        private static readonly string SuccessPrefix = " :) ";

        #endregion
        #region generic

        /// <summary>
        /// Writes the specified value to the standard output.
        /// </summary>
        /// <typeparam name="T">The type of the value to write.</typeparam>
        /// <param name="x">The value to write. If <paramref name="x"/> is <see langword="null"/>, nothing is written.</param>
        public static void Write<T>(T? x)
            => Console.Write(x);

        /// <summary>
        /// Writes the specified value, followed by the current line terminator, to the standard output.
        /// </summary>
        /// <typeparam name="T">The type of the value to write.</typeparam>
        /// <param name="x">The value to write. If <paramref name="x"/> is <see langword="null"/>, only the line terminator is written.</param>
        public static void WriteLine<T>(T? x)
            => Console.WriteLine(x);

        #endregion
        #region plain text

        /// <summary>
        /// writes info with prefix
        /// </summary>
        /// <param name="prefix"> prefix in front </param>
        /// <param name="mainInfo"> main information </param>
        /// <param name="includeDataTime"> whether to include DataTime </param>
        /// <param name="prompt"> additional prompt before main info </param>
        internal static void WriteLinePrefix(string? prefix, string? mainInfo,
            bool includeDataTime = true,
            string? prompt = "")
        {
            string? fullInfo = prefix;
            if (includeDataTime)
                fullInfo += DateTime.Now.ToString() + " ";
            fullInfo += prompt + " " + mainInfo;
            WriteLine(fullInfo);
        }

        /// <summary>
        /// writes info with warning prefix
        /// </summary>
        /// <param name="mainInfo"> main information </param>
        /// <param name="includeDataTime"> whether to include DataTime </param>
        /// <param name="prompt"> additional prompt before main info </param>
        public static void Warning(string? mainInfo,
            bool includeDataTime = true, string? prompt = "[Warning]")
            => WriteLinePrefix(WarningPrefix, mainInfo, includeDataTime, prompt);

        /// <summary>
        /// writes info with error prefix
        /// </summary>
        /// <param name="mainInfo"> main information </param>
        /// <param name="includeDataTime"> whether to include DataTime </param>
        /// <param name="prompt"> additional prompt before main info </param>
        public static void Error(string? mainInfo,
            bool includeDataTime = true, string? prompt = "")
            => WriteLinePrefix(ErrorPrefix, mainInfo, includeDataTime, prompt);

        /// <summary>
        /// writes info with logging prefix
        /// </summary>
        /// <param name="mainInfo"> main information </param>
        /// <param name="includeDataTime"> whether to include DataTime </param>
        /// <param name="prompt"> additional prompt before main info </param>
        public static void Logging(string? mainInfo,
            bool includeDataTime = true, string? prompt = "")
            => WriteLinePrefix(LoggingPrefix, mainInfo, includeDataTime, prompt);

        /// <summary>
        /// writes info with logging prefix
        /// </summary>
        /// <param name="mainInfo"> main information </param>
        /// <param name="includeDataTime"> whether to include DataTime </param>
        /// <param name="prompt"> additional prompt before main info </param>
        public static void Success(string? mainInfo,
            bool includeDataTime = true, string? prompt = "")
            => WriteLinePrefix(SuccessPrefix, mainInfo, includeDataTime, prompt);

        #endregion
        #region vectors

        /// <summary>
        /// writes a vector element by element
        /// </summary>
        /// <param name="prompt"> message </param>
        /// <param name="x"> vector to print </param>
        public static void Write(string? prompt, VectorI? x)
        {
            // exception handling
            if (x == null) { throw new ArgumentNullException(nameof(x)); }
            // prompt
            WriteLine(prompt);
            // vector elements ...
            StringBuilder t = new();
            for (long i = 0; i < x.Count; i++)
            { t.Append("\t" + x[i, checkBound: false].ToString()); } 
            WriteLine(t);
        }

        /// <summary>
        /// writes a vector element by element
        /// </summary>
        /// <param name="x"> vector to print </param>
        public static void Write(VectorI? x)
            => Write($"{typeof(VectorI).Name}:", x);

        /// <summary>
        /// writes a vector element by element
        /// </summary>
        /// <param name="prompt"> message </param>
        /// <param name="x"> input real-valued vector to write </param>
        /// <param name="columnSeparator"> column separator option </param>
        /// <param name="numFormat"> number format </param>
        /// <param name="digits"> number of digits, either total digits or decimal digits </param>
        /// <param name="culture"> culture selected for printer e.g. "en-US" </param>
        public static void Write(string? prompt, VectorD? x,
            CharSeparator? columnSeparator = null,
            NumericFormat? numFormat = null,
            int? digits = null,
            CultureName? culture = null)
        {
            // exception handling
            if(x == null) 
            { throw new ArgumentNullException(nameof(x)); }
            // prompt
            WriteLine(prompt);
            // vector elements ...
            DataConverter dataConv = new(columnSeparator: columnSeparator,
                numFormat: numFormat, digits: digits, culture: culture);
            StringBuilder t = dataConv.ToString(x);
            // writes to console
            WriteLine(t);
        }

        /// <summary>
        /// writes a vector element by element
        /// </summary>
        /// <param name="x"> input real-valued vector to write </param>
        public static void Write(VectorD? x)
            => Write($"{typeof(VectorD).Name}:", x);

        /// <summary>
        /// writes a vector element by element
        /// </summary>
        /// <param name="prompt"> message </param>
        /// <param name="x"> input complex-valued vector to write </param>
        /// <param name="columnSeparator"> column separator option </param>
        /// <param name="numFormat"> number format </param>
        /// <param name="digits"> number of digits, either total digits or decimal digits </param>
        /// <param name="culture"> culture selected for printer e.g. "en-US" </param>
        /// <param name="cplxFormat"> complex number format </param>
        public static void Write(string? prompt, VectorZ? x,
            CharSeparator? columnSeparator = null,
            NumericFormat? numFormat = null,
            int? digits = null,
            CultureName? culture = null,
            ComplexFormat? cplxFormat = null)
        {
            if(x == null) 
            { throw new ArgumentNullException(nameof(x)); }
            // prompt
            WriteLine(prompt);
            // vector elements
            DataConverter dataConv = new(columnSeparator: columnSeparator,
                numFormat: numFormat, digits: digits, culture: culture);
            StringBuilder t = dataConv.ToString(x, cplxFormat: cplxFormat);
            // writes to console
            WriteLine(t);
        }

        /// <summary>
        /// writes a vector element by element
        /// </summary>
        /// <param name="x"> complex-valued vector to print </param>
        public static void Write(VectorZ? x)
            => Write($"{typeof(VectorZ).Name}:", x);

        #endregion
        #region matrix

        /// <summary>
        /// writes a matrix element by element
        /// </summary>
        /// <param name="prompt"> message </param>
        /// <param name="a"> input real-valued matrix to print </param>
        /// <param name="columnSeparator"> column separator option </param>
        /// <param name="numFormat"> number format </param>
        /// <param name="digits"> number of digits, either total digits or decimal digits </param>
        /// <param name="culture"> culture selected for printer e.g. "en-US" </param>
        public static void Write(string? prompt, MatrixD? a,
            CharSeparator? columnSeparator = null,
            NumericFormat? numFormat = null,
            int? digits = null,
            CultureName? culture = null)
        {
            if(a == null) 
            { throw new ArgumentNullException(nameof(a)); }
            // prompt
            WriteLine(prompt);
            // matrix elements
            DataConverter dataConv = new(columnSeparator: columnSeparator,
                numFormat: numFormat, digits: digits, culture: culture);
            StringBuilder t = dataConv.ToString(a);
            // writes to console
            WriteLine(t);
        }

        /// <summary>
        /// writes a matrix element by element
        /// </summary>
        /// <param name="x"> input real-valued matrix to print </param>
        public static void Write(MatrixD? x)
            => Write($"{typeof(MatrixD).Name}:", x);

        /// <summary>
        /// writes a matrix element by element
        /// </summary>
        /// <param name="prompt"> message </param>
        /// <param name="a"> input complex-valued matrix to print </param>
        /// <param name="columnSeparator"> column separator option </param>
        /// <param name="numFormat"> number format </param>
        /// <param name="digits"> number of digits, either total digits or decimal digits </param>
        /// <param name="culture"> culture selected for printer e.g. "en-US" </param>
        /// <param name="cplxFormat"> complex number format </param>
        public static void Write(string? prompt, MatrixZ? a,
            CharSeparator? columnSeparator = null,
            NumericFormat? numFormat = null,
            int? digits = null,
            CultureName? culture = null,
            ComplexFormat? cplxFormat = null)
        {
            if(a == null) 
            { throw new ArgumentNullException(nameof(a)); }
            // prompt
            WriteLine(prompt);
            // matrix elements
            DataConverter dataConv = new(columnSeparator: columnSeparator, 
                numFormat: numFormat, digits: digits, culture: culture);
            StringBuilder t = dataConv.ToString(a, cplxFormat: cplxFormat);
            // writes to console
            WriteLine(t);
        }

        /// <summary>
        /// writes a matrix element by element
        /// </summary>
        /// <param name="x"> input complex-valued matrix to print </param>
        public static void Write(MatrixZ? x)
            => Write($"{typeof(MatrixZ).Name}:", x);

        #endregion
        #region progress

        /// <summary>
        /// Displays a progress bar in the console output.
        /// </summary>
        /// <param name="total">The total number of steps for the progress bar.</param>
        /// <param name="idx">The current step index (progress so far).</param>
        /// <param name="width">The width of the progress bar in characters. Default is 50.</param>
        /// <param name="prompt">An optional prompt message to display before the progress bar.</param>
        public static void ProgressBar(long total, long idx,
            int width = 50, string? prompt = null)
        {
            int progress = (int)((idx / (double)total) * width);
            string bar = new string('#', progress).PadRight(width, '-');
            int percentage = (int)((idx / (double)total) * 100);
            Write($"\r{prompt} [{bar}] {percentage}% ");
            if (percentage == 100) { Write($"\n"); }
        }

        #endregion


        #region ---- Vector<T> ----


        /// <summary>
        /// Writes a real-valued vector element by element to the standard output.
        /// </summary>
        /// <param name="x">The real-valued vector to print.</param>
        /// <param name="prompt">An optional message to display before the vector.</param>
        /// <param name="columnSeparator">Column separator option.</param>
        /// <param name="numFormat">Numeric format used for conversion.</param>
        /// <param name="digits">Number of digits.</param>
        /// <param name="culture">Culture selected for conversion.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="x"/> is null.</exception>
        public static void Write(Vector<Real>? x,
            string? prompt = null,
            CharSeparator? columnSeparator = null,
            NumericFormat? numFormat = null,
            int? digits = null,
            CultureName? culture = null)
        {
            // exception handling
            if (x is null) { throw new ArgumentNullException(nameof(x)); }
            // prompt
            WriteLine(prompt ?? nameof(Vector<Real>));
            // vector elements ...
            DataConverter dc = new(columnSeparator, numFormat, digits, culture);
            WriteLine(dc.ToString(x));
        }

        /// <summary>
        /// Writes a complex-valued vector element by element to the standard output.
        /// </summary>
        /// <param name="x">The complex-valued vector to print.</param>
        /// <param name="prompt">An optional message to display before the vector.</param>
        /// <param name="columnSeparator">Column separator option.</param>
        /// <param name="numFormat">Numeric format used for conversion.</param>
        /// <param name="digits">Number of digits.</param>
        /// <param name="culture">Culture selected for conversion.</param>
        /// <param name="cplxFormat">Complex number format.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="x"/> is null.</exception>
        public static void Write(Vector<Complex>? x,
            string? prompt = null,
            CharSeparator? columnSeparator = null,
            NumericFormat? numFormat = null,
            int? digits = null,
            CultureName? culture = null,
            ComplexFormat? cplxFormat = null)
        {
            // exception handling
            if (x is null) { throw new ArgumentNullException(nameof(x)); }
            // prompt
            WriteLine(prompt ?? nameof(Vector<Cplx>));
            // vector elements ...
            DataConverter dc = new(columnSeparator, numFormat, digits, culture);
            WriteLine(dc.ToString(x, cplxFormat));
        }

        /// <summary>
        /// Writes an integer-valued vector element by element to the standard output.
        /// </summary>
        /// <param name="x">The integer-valued vector to print.</param>
        /// <param name="prompt">An optional message to display before the vector.</param>
        /// <param name="columnSeparator">Column separator option.</param>
        /// <param name="numFormat">Numeric format used for conversion.</param>
        /// <param name="digits">Number of digits.</param>
        /// <param name="culture">Culture selected for conversion.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="x"/> is null.</exception>
        public static void Write(Vector<Int>? x,
            string? prompt = null,
            CharSeparator? columnSeparator = null,
            NumericFormat? numFormat = null,
            int? digits = null,
            CultureName? culture = null)
        {
            // exception handling
            if (x is null) { throw new ArgumentNullException(nameof(x)); }
            // prompt
            WriteLine(prompt ?? nameof(Vector<Int>));
            // vector elements ...
            DataConverter dc = new(columnSeparator, numFormat, digits, culture);
            WriteLine(dc.ToString(x));
        }





        #endregion


        #region ---- Vect<T> ----

        /// <summary>
        /// Writes a real-valued vector element by element to the standard output.
        /// </summary>
        /// <param name="x">The real-valued vector to print.</param>
        /// <param name="prompt">An optional message to display before the vector.</param>
        /// <param name="columnSeparator">Column separator option.</param>
        /// <param name="numFormat">Numeric format used for conversion.</param>
        /// <param name="digits">Number of digits.</param>
        /// <param name="culture">Culture selected for conversion.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="x"/> is null.</exception>
        public static void Write(Vect<Real>? x,
            string? prompt = null,
            CharSeparator? columnSeparator = null,
            NumericFormat? numFormat = null,
            int? digits = null,
            CultureName? culture = null)
        {
            // exception handling
            if (x is null) { throw new ArgumentNullException(nameof(x)); }
            // prompt
            WriteLine(prompt ?? nameof(Vector<Real>));
            // vector elements ...
            DataConverter dc = new(columnSeparator, numFormat, digits, culture);
            WriteLine(dc.ToString(x));
        }

        /// <summary>
        /// Writes a complex-valued vector element by element to the standard output.
        /// </summary>
        /// <param name="x">The complex-valued vector to print.</param>
        /// <param name="prompt">An optional message to display before the vector.</param>
        /// <param name="columnSeparator">Column separator option.</param>
        /// <param name="numFormat">Numeric format used for conversion.</param>
        /// <param name="digits">Number of digits.</param>
        /// <param name="culture">Culture selected for conversion.</param>
        /// <param name="cplxFormat">Complex number format.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="x"/> is null.</exception>
        public static void Write(Vect<Cplx>? x,
            string? prompt = null,
            CharSeparator? columnSeparator = null,
            NumericFormat? numFormat = null,
            int? digits = null,
            CultureName? culture = null,
            ComplexFormat? cplxFormat = null)
        {
            // exception handling
            if (x is null) { throw new ArgumentNullException(nameof(x)); }
            // prompt
            WriteLine(prompt ?? nameof(Vector<Cplx>));
            // vector elements ...
            DataConverter dc = new(columnSeparator, numFormat, digits, culture);
            WriteLine(dc.ToString(x, cplxFormat));
        }

        /// <summary>
        /// Writes an integer-valued vector element by element to the standard output.
        /// </summary>
        /// <param name="x">The integer-valued vector to print.</param>
        /// <param name="prompt">An optional message to display before the vector.</param>
        /// <param name="columnSeparator">Column separator option.</param>
        /// <param name="numFormat">Numeric format used for conversion.</param>
        /// <param name="digits">Number of digits.</param>
        /// <param name="culture">Culture selected for conversion.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="x"/> is null.</exception>
        public static void Write(Vect<Int>? x,
            string? prompt = null,
            CharSeparator? columnSeparator = null,
            NumericFormat? numFormat = null,
            int? digits = null,
            CultureName? culture = null)
        {
            // exception handling
            if (x is null) { throw new ArgumentNullException(nameof(x)); }
            // prompt
            WriteLine(prompt ?? nameof(Vector<Int>));
            // vector elements ...
            DataConverter dc = new(columnSeparator, numFormat, digits, culture);
            WriteLine(dc.ToString(x));
        }

        #endregion
        #region ---- Matx<T> ----

        /// <summary>
        /// Writes a real-valued matrix element by element to the standard output.
        /// </summary>
        /// <param name="x">The real-valued matrix to print.</param>
        /// <param name="prompt">An optional message to display before the matrix.</param>
        /// <param name="columnSeparator">Column separator option.</param>
        /// <param name="numFormat">Numeric format used for conversion.</param>
        /// <param name="digits">Number of digits.</param>
        /// <param name="culture">Culture selected for conversion.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="x"/> is null.</exception>
        public static void Write(Matx<double>? x,
            string? prompt,
            CharSeparator? columnSeparator = null,
            NumericFormat? numFormat = null,
            int? digits = null,
            CultureName? culture = null)
        {
            if (x is null) { throw new ArgumentNullException(nameof(x)); }
            // prompt
            WriteLine(prompt ?? nameof(Matx<Real>));
            // matrix elements
            DataConverter dc = new(columnSeparator, numFormat, digits, culture);
            WriteLine(dc.ToString(x));
        }

        /// <summary>
        /// Writes a complex-valued matrix element by element to the standard output.
        /// </summary>
        /// <param name="x">The complex-valued matrix to print.</param>
        /// <param name="prompt">An optional message to display before the matrix.</param>
        /// <param name="columnSeparator">Column separator option.</param>
        /// <param name="numFormat">Numeric format used for conversion.</param>
        /// <param name="digits">Number of digits.</param>
        /// <param name="culture">Culture selected for conversion.</param>
        /// <param name="cplxFormat">Complex number format.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="x"/> is null.</exception>
        public static void Write(Matx<Cplx>? x,
            string? prompt,
            CharSeparator? columnSeparator = null,
            NumericFormat? numFormat = null,
            int? digits = null,
            CultureName? culture = null,
            ComplexFormat? cplxFormat = null)
        {
            if (x is null) { throw new ArgumentNullException(nameof(x)); }
            // prompt
            WriteLine(prompt ?? nameof(Matx<Cplx>));
            // matrix elements
            DataConverter dc = new(columnSeparator, numFormat, digits, culture);
            WriteLine(dc.ToString(x, cplxFormat));
        }

        /// <summary>
        /// Writes an integer-valued matrix element by element to the standard output.
        /// </summary>
        /// <param name="x">The integer-valued matrix to print.</param>
        /// <param name="prompt">An optional message to display before the matrix.</param>
        /// <param name="columnSeparator">Column separator option.</param>
        /// <param name="numFormat">Numeric format used for conversion.</param>
        /// <param name="digits">Number of digits.</param>
        /// <param name="culture">Culture selected for conversion.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="x"/> is null.</exception>
        public static void Write(Matx<Int>? x,
            string? prompt,
            CharSeparator? columnSeparator = null,
            NumericFormat? numFormat = null,
            int? digits = null,
            CultureName? culture = null)
        {
            if (x is null) { throw new ArgumentNullException(nameof(x)); }
            // prompt
            WriteLine(prompt ?? nameof(Matx<Int>));
            // matrix elements
            DataConverter dc = new(columnSeparator, numFormat, digits, culture);
            WriteLine(dc.ToString(x));
        }

        #endregion


    }
}
