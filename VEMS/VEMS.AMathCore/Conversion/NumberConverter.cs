using System.Globalization;
using System.Numerics;

namespace VEMS.AMathCore
{
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
        /// Converts the specified numeric value to its string representation 
        /// using the configured format and culture settings.
        /// </summary>
        /// <remarks>Supported types include Real, Int, and Cplx. For complex numbers, the string
        /// representation can be formatted as either magnitude and phase or real and imaginary components, depending on
        /// the cplxFormat parameter.</remarks>
        /// <typeparam name="T">The numeric type to convert. Must implement the INumber<T> interface.</typeparam>
        /// <param name="x">The value to convert to a string representation.</param>
        /// <param name="cplxFormat">An optional format specifying how to represent complex numbers. If null, the default format is used.</param>
        /// <returns>A string representation of the specified value, formatted according to the configured settings. For complex
        /// numbers, the format depends on the value of cplxFormat.</returns>
        /// <exception cref="NotSupportedException">Thrown if the type specified by T is not supported.</exception>
        public string ToString<T>(T x,
            ComplexFormat? cplxFormat = null)
            where T : INumber<T>
        {
            if (typeof(T) == typeof(Real))
            {
                Real t = Convert.ToDouble(x);
                return t.ToString(format: Format, provider: Culture);
            }
            else if (typeof(T) == typeof(Int))
            {
                Int t = Convert.ToInt64(x);
                return t.ToString(format: Format, provider: Culture);
            }
            else if (typeof(T) == typeof(Cplx)) 
            { 
                Cplx t = (Cplx)(object)x;
                switch (cplxFormat)
                {
                    case ComplexFormat.MagnitudeAndPhase:
                        {
                            string mag = ToString(t.Magnitude);
                            string phase = ToString(t.Phase);
                            return $"{mag} * Exp[i({phase})]";
                        }
                    case ComplexFormat.RealAndImaginary:
                        {
                            string real = ToString(t.Real);
                            string imag = ToString(t.Imag);
                            return $"{real} + i({imag})";
                        }
                    default: goto case ComplexFormat.RealAndImaginary;
                }
            }
            else throw new NotSupportedException($"Type {typeof(T).Name} is not supported.");
        }

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
            switch (cplxFormat)
            {
                case ComplexFormat.MagnitudeAndPhase:
                    {
                        string mag = ToString(x.Magnitude);
                        string phase = ToString(x.Phase);
                        return $"{mag} * Exp[i({phase})]";
                    }
                case ComplexFormat.RealAndImaginary:
                    {
                        string real = ToString(x.Real);
                        string imag = ToString(x.Imag);
                        return $"{real} + i({imag})";
                    }
                default: goto case ComplexFormat.RealAndImaginary;
            }
        }

        #endregion
    }
}
