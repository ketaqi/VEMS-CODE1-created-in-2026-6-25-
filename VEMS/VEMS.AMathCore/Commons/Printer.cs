using System.Numerics;
using System.Text;

namespace VEMS.AMathCore
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
        #region array

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="x"></param>
        /// <param name="prompt"></param>
        /// <param name="columnSeparator"></param>
        /// <param name="numFormat"></param>
        /// <param name="digits"></param>
        /// <param name="culture"></param>
        /// <param name="cplxFormat"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public static void Write<T>(DenseArray<T>? x, string? prompt,
            CharSeparator? columnSeparator = null,
            NumericFormat? numFormat = null,
            int? digits = null,
            CultureName? culture = null,
            ComplexFormat? cplxFormat = null)
            where T : INumber<T>
        {
            // exception handling
            if (x == null) { throw new ArgumentNullException(nameof(x)); }
            // prompt
            WriteLine(prompt);

            // vector elements
            DataConverter dataConv = new(columnSeparator: columnSeparator,
                numFormat: numFormat, digits: digits, culture: culture);
            StringBuilder t = dataConv.ToString(x, cplxFormat: cplxFormat);

            // writes to console
            WriteLine(t);
        }

        #endregion
        #region vector

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="x"></param>
        /// <param name="prompt"></param>
        /// <param name="columnSeparator"></param>
        /// <param name="numFormat"></param>
        /// <param name="digits"></param>
        /// <param name="culture"></param>
        /// <param name="cplxFormat"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public static void Write<T>(Vect<T>? x, string? prompt,
            CharSeparator? columnSeparator = null,
            NumericFormat? numFormat = null,
            int? digits = null,
            CultureName? culture = null,
            ComplexFormat? cplxFormat = null)
            where T : INumber<T>
        {
            // exception handling
            if (x == null) { throw new ArgumentNullException(nameof(x)); }
            // prompt
            WriteLine(prompt);

            // vector elements
            DataConverter dataConv = new(columnSeparator: columnSeparator,
                numFormat: numFormat, digits: digits, culture: culture);
            StringBuilder t = dataConv.ToString(x, cplxFormat: cplxFormat);
            
            // writes to console
            WriteLine(t);
        }

        #endregion
        #region matrix


        public static void Write<T>(Matx<T>? x, string? prompt,
            CharSeparator? columnSeparator = null,
            NumericFormat? numFormat = null,
            int? digits = null,
            CultureName? culture = null,
            ComplexFormat? cplxFormat = null,
            bool revRows = false)
            where T : INumber<T>
        {
            // exception handling
            if (x == null) { throw new ArgumentNullException(nameof(x)); }
            // prompt
            WriteLine(prompt);

            // matrix elements
            DataConverter dataConv = new(columnSeparator: columnSeparator,
                numFormat: numFormat, digits: digits, culture: culture);
            StringBuilder t = dataConv.ToString(x, cplxFormat: cplxFormat, revRows: revRows);

            // writes to console
            WriteLine(t);
        }

        #endregion

    }
}
