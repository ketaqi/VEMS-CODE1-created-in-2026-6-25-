using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;

namespace VEMS.AMathCore
{
    /// <summary>
    /// Represents a complex number with double-precision real and imaginary components.
    /// </summary>
    public struct Cplx : INumber<Cplx>
    {
        #region properties

        /// <summary>
        /// Gets or sets the real part of the complex number.
        /// </summary>
        public double Real { get; set; }

        /// <summary>
        /// Gets or sets the imaginary part of the complex number.
        /// </summary>
        public double Imag { get; set; }

        /// <summary>
        /// Gets the magnitude of the complex number.
        /// </summary>
        public double Magnitude
        {
            get => Real * Real + Imag * Imag;
        }

        /// <summary>
        /// Gets the phase of the complex number.
        /// </summary>
        public double Phase
        {
            get
            {
                if (Real == 0 && Imag == 0)
                    return 0; // Undefined phase for zero complex number
                return Math.Atan2(Imag, Real);
            }
        }

        #endregion
        #region constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public Cplx() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Cplx"/> struct with the specified real and imaginary parts.
        /// </summary>
        /// <param name="real">The real part.</param>
        /// <param name="imag">The imaginary part.</param>
        public Cplx(double real, double imag)
        {
            Real = real;
            Imag = imag;
        }

        #endregion
        #region methods

        #region ---- hash ----

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode() => HashCode.Combine(Real, Imag);

        #endregion
        #region ---- static fields ----

        /// <summary>
        /// Gets the multiplicative identity (1, 0) for complex numbers.
        /// </summary>
        public static Cplx One => new(real: 1, imag: 0);

        /// <summary>
        /// Gets the additive identity (0, 0) for complex numbers.
        /// </summary>
        public static Cplx Zero => new(real: 0, imag: 0);

        /// <summary>
        /// Gets the imaginary unit, represented as a complex number with a real part of 0 and an imaginary part of 1.
        /// </summary>
        public static Cplx ImagOne => new(real: 0, imag: 1);

        /// <summary>
        /// Gets a <see cref="Cplx"/> instance representing infinity.
        /// </summary>
        public static Cplx Infinity => new(real: double.PositiveInfinity, imag: double.PositiveInfinity);

        /// <summary>
        /// Gets a value that represents a complex number with both real and imaginary parts set to <see
        /// cref="double.NaN"/>.
        /// </summary>
        /// <remarks>This property can be used to represent an undefined or invalid complex number.
        /// Operations involving <see langword="NaN"/> may propagate <see langword="NaN"/> values depending on the
        /// operation.</remarks>
        public static Cplx NaN => new(real: double.NaN, imag: double.NaN);

        /// <summary>
        /// Gets the additive identity for complex numbers.
        /// </summary>
        public static Cplx AdditiveIdentity => Zero;

        /// <summary>
        /// Gets the multiplicative identity for complex numbers.
        /// </summary>
        public static Cplx MultiplicativeIdentity => One;

        /// <summary>        
        /// /// Gets the base of the number system used by the <see cref="Cplx"/> type.
        /// For complex numbers, this is always 2 (binary).
        /// </summary>
        public static int Radix => 2;

        #endregion
        #region ---- basic math ----

        /// <summary>
        /// Adds a real and a complex number.
        /// </summary>
        /// <param name="left">The real number.</param>
        /// <param name="right">The complex number.</param>
        /// <returns>The sum of the real and the complex number.</returns>
        public static Cplx Add(double left, Cplx right)
            => new(real: left + right.Real, imag: right.Imag);

        /// <summary>
        /// Adds a complex and a real number.
        /// </summary>
        /// <param name="left">The complex number.</param>
        /// <param name="right">The real number.</param>
        /// <returns>The sum of the complex and the real number.</returns>
        public static Cplx Add(Cplx left, double right)
            => Add(right, left);

        /// <summary>
        /// Adds two complex numbers.
        /// </summary>
        /// <param name="left">The first complex number.</param>
        /// <param name="right">The second complex number.</param>
        /// <returns>The sum of the two complex numbers.</returns>
        public static Cplx Add(Cplx left, Cplx right)
            => new(real: left.Real + right.Real, imag: left.Imag + right.Imag);

        /// <summary>
        /// Subtracts a real number from a complex number, returning the resulting complex number.
        /// </summary>
        /// <param name="left">The complex number to subtract from.</param>
        /// <param name="right">The real number to subtract.</param>
        /// <returns>A <see cref="Cplx"/> representing the result of subtracting <paramref name="right"/> from <paramref
        /// name="left"/>.</returns>
        public static Cplx Subtract(Cplx left, double right)
            => new(real: left.Real - right, imag: left.Imag);

        /// <summary>
        /// Subtracts one complex number from another.
        /// </summary>
        /// <param name="left">The first complex number (minuend).</param>
        /// <param name="right">The second complex number (subtrahend).</param>
        /// <returns>The result of <paramref name="left"/> minus <paramref name="right"/>.</returns>
        public static Cplx Subtract(Cplx left, Cplx right)
            => new(real: left.Real - right.Real, imag: left.Imag - right.Imag);

        /// <summary>
        /// Subtracts a complex number from a real number, returning the resulting complex number.
        /// </summary>
        /// <param name="left">The real number to subtract from.</param>
        /// <param name="right">The complex number to subtract.</param>
        /// <returns>
        /// A <see cref="Cplx"/> representing the result of subtracting <paramref name="right"/> from <paramref name="left"/>.
        /// </returns>
        public static Cplx Subtract(double left, Cplx right)
            => new(real: left - right.Real, imag: -right.Imag);

        /// <summary>
        /// Multiplies a scalar value by a complex number, scaling both the real and imaginary components.
        /// </summary>
        /// <param name="left">The scalar value to multiply with the complex number.</param>
        /// <param name="right">The complex number to be scaled.</param>
        /// <returns>A new <see cref="Cplx"/> instance representing the result of the multiplication.</returns>
        public static Cplx Multiply(double left, Cplx right)
            => new(real: left * right.Real, imag: left * right.Imag);

        /// <summary>
        /// Multiplies a complex number by a scalar value.
        /// </summary>
        /// <param name="left">The complex number to be multiplied.</param>
        /// <param name="right">The scalar value to multiply the complex number by.</param>
        /// <returns>A new <see cref="Cplx"/> instance representing the product of the complex number and the scalar value.</returns>
        public static Cplx Multiply(Cplx left, double right)
            => Multiply(left, right);

        /// <summary>
        /// Multiplies two complex numbers.
        /// Formula: (a + bi) * (c + di) = (ac - bd) + (ad + bc)i
        /// </summary>
        /// <param name="left">The first complex number.</param>
        /// <param name="right">The second complex number.</param>
        /// <returns>The product of the two complex numbers.</returns>
        public static Cplx Multiply(Cplx left, Cplx right)
        {
            double real = left.Real * right.Real - left.Imag * right.Imag;
            double imag = left.Real * right.Imag + left.Imag * right.Real;
            return new Cplx(real, imag);
        }

        ///<summary>
        /// Divides a real number by a complex number.
        /// Formula: (a) / (c + di) = (a * c) / (c^2 + d^2) - (a * d) / (c^2 + d^2) * i
        /// </summary>
        /// <param name="dividend">The real number to be divided (numerator).</param>
        /// <param name="divisor">The complex number to divide by (denominator).</param>
        /// <returns>
        /// A <see cref="Cplx"/> representing the result of dividing <paramref name="dividend"/> by <paramref name="divisor"/>.
        /// </returns>
        public static Cplx Divide(double dividend, Cplx divisor)
        {
            double c = divisor.Real;
            double d = divisor.Imag;
            double denom = c * c + d * d;
            return new Cplx((dividend * c) / denom, (-dividend * d) / denom);
        }

        /// <summary>
        /// Divides a complex number by a real (double) value.
        /// </summary>
        /// <param name="dividend">The complex number to be divided (numerator).</param>
        /// <param name="divisor">The real number to divide by (denominator).</param>
        /// <returns>
        /// A <see cref="Cplx"/> representing the result of dividing <paramref name="dividend"/> by <paramref name="divisor"/>.
        /// </returns>
        public static Cplx Divide(Cplx dividend, double divisor)
        {
            // Fast path for division by 1.0 (no-op)
            if (divisor == 1.0)
                return dividend;

            // Avoid division by zero (let .NET handle the exception/NaN/Infinity)
            return new Cplx(dividend.Real / divisor, dividend.Imag / divisor);
        }

        /// <summary>
        /// Divides one complex number by another.
        /// Formula: (a + bi) / (c + di) = [(ac + bd) + (bc - ad)i] / (c^2 + d^2)
        /// </summary>
        /// <param name="dividend">The numerator complex number.</param>
        /// <param name="divisor">The denominator complex number.</param>
        /// <returns>The quotient of <paramref name="dividend"/> divided by <paramref name="divisor"/>.</returns>
        public static Cplx Divide(Cplx dividend, Cplx divisor)
        {
            double a = dividend.Real;
            double b = dividend.Imag;
            double c = divisor.Real;
            double d = divisor.Imag;
            double denom = c * c + d * d;
            double real = (a * c + b * d) / denom;
            double imag = (b * c - a * d) / denom;
            return new Cplx(real, imag);
        }

        /// <summary>
        /// Returns the multiplicative inverse (reciprocal) of a complex number.
        /// Formula: 1/(a + bi) = (a - bi) / (a^2 + b^2)
        /// </summary>
        /// <param name="value">The complex number.</param>
        /// <returns>The reciprocal of <paramref name="value"/>.</returns>
        public static Cplx Reciprocal(Cplx value)
        {
            double a = value.Real;
            double b = value.Imag;
            double denom = a * a + b * b;
            return new Cplx(a / denom, -b / denom);
        }

        /// <summary>
        /// Returns a new <see cref="Cplx"/> number that is the negation of the specified complex number.
        /// </summary>
        /// <param name="value">The complex number to negate.</param>
        /// <returns>A <see cref="Cplx"/> number with both the real and imaginary components negated.</returns>
        public static Cplx Negate(Cplx value)
            => new(real: -value.Real, imag: -value.Imag);

        /// <summary>
        /// Returns the complex conjugate of the specified complex number.
        /// The conjugate of (a + bi) is (a - bi).
        /// </summary>
        /// <param name="value">The complex number.</param>
        /// <returns>The conjugate of <paramref name="value"/>.</returns>
        public static Cplx Conjugate(Cplx value)
            => new(real: value.Real, imag: -value.Imag);

        /// <summary>
        /// Creates a complex number from polar coordinates.
        /// </summary>
        /// <param name="magnitude">The magnitude (radius, r).</param>
        /// <param name="angle">The angle (theta, in radians).</param>
        /// <returns>A <see cref="Cplx"/> number with the specified magnitude and angle.</returns>
        public static Cplx FromPolar(double magnitude, double angle)
        {
            double real = magnitude * Math.Cos(angle);
            double imag = magnitude * Math.Sin(angle);
            return new Cplx(real, imag);
        }

        #endregion
        #region ---- more math ----

        /// <summary>
        /// Returns the absolute value (magnitude) of the specified complex number as a <see cref="double"/>.
        /// </summary>
        /// <param name="value">The complex number whose magnitude is to be calculated.</param>
        /// <returns>
        /// The magnitude of the complex number, calculated as <c>sqrt(real^2 + imag^2)</c>.
        /// </returns>
        public static double Absolute(Cplx value)
            => Math.Sqrt(value.Real * value.Real + value.Imag * value.Imag);

        /// <summary>
        /// Returns the magnitude of a complex number.
        /// </summary>
        public static Cplx Abs(Cplx value)
            => new(real: Absolute(value), imag: 0);

        /// <summary>
        /// Returns the sine of a complex number.
        /// Formula: sin(z) = sin(a) * cosh(b) + i * cos(a) * sinh(b)
        /// where z = a + i*b
        /// </summary>
        /// <param name="value">The complex number.</param>
        /// <returns>The sine of <paramref name="value"/>.</returns>
        public static Cplx Sin(Cplx value)
        {
            double a = value.Real;
            double b = value.Imag;
            double real = Math.Sin(a) * Math.Cosh(b);
            double imag = Math.Cos(a) * Math.Sinh(b);
            return new Cplx(real, imag);
        }

        /// <summary>
        /// Returns the cosine of a complex number.
        /// Formula: cos(z) = cos(a) * cosh(b) - i * sin(a) * sinh(b)
        /// where z = a + i*b
        /// </summary>
        /// <param name="value">The complex number.</param>
        /// <returns>The cosine of <paramref name="value"/>.</returns>
        public static Cplx Cos(Cplx value)
        {
            double a = value.Real;
            double b = value.Imag;
            double real = Math.Cos(a) * Math.Cosh(b);
            double imag = -Math.Sin(a) * Math.Sinh(b);
            return new Cplx(real, imag);
        }

        /// <summary>
        /// Returns the tangent of a complex number.
        /// Formula: tan(z) = sin(z) / cos(z)
        /// </summary>
        /// <param name="value">The complex number.</param>
        /// <returns>The tangent of <paramref name="value"/>.</returns>
        public static Cplx Tan(Cplx value)
            => Sin(value) / Cos(value);

        /// <summary>
        /// Returns the hyperbolic sine of a complex number.
        /// Formula: sinh(z) = sinh(a) * cos(b) + i * cosh(a) * sin(b)
        /// where z = a + i*b
        /// </summary>
        /// <param name="value">The complex number.</param>
        /// <returns>The hyperbolic sine of <paramref name="value"/>.</returns>
        public static Cplx Sinh(Cplx value)
        {
            double a = value.Real;
            double b = value.Imag;
            double real = Math.Sinh(a) * Math.Cos(b);
            double imag = Math.Cosh(a) * Math.Sin(b);
            return new Cplx(real, imag);
        }

        /// <summary>
        /// Returns the hyperbolic cosine of a complex number.
        /// Formula: cosh(z) = cosh(a) * cos(b) + i * sinh(a) * sin(b)
        /// where z = a + i*b
        /// </summary>
        /// <param name="value">The complex number.</param>
        /// <returns>The hyperbolic cosine of <paramref name="value"/>.</returns>
        public static Cplx Cosh(Cplx value)
        {
            double a = value.Real;
            double b = value.Imag;
            double real = Math.Cosh(a) * Math.Cos(b);
            double imag = Math.Sinh(a) * Math.Sin(b);
            return new Cplx(real, imag);
        }

        /// <summary>
        /// Returns the hyperbolic tangent of a complex number.
        /// Formula: tanh(z) = sinh(z) / cosh(z)
        /// </summary>
        /// <param name="value">The complex number.</param>
        /// <returns>The hyperbolic tangent of <paramref name="value"/>.</returns>
        public static Cplx Tanh(Cplx value)
            => Sinh(value) / Cosh(value);

        /// <summary>
        /// Returns the arc cosine of a complex number.
        /// Formula: acos(z) = -i * log(z + i * sqrt(1 - z^2))
        /// </summary>
        /// <param name="value">The complex number.</param>
        /// <returns>The arc cosine of <paramref name="value"/>.</returns>
        public static Cplx Acos(Cplx value)
        {
            // z = value
            // i = (0, 1)
            // sqrt(1 - z^2)
            double a = value.Real;
            double b = value.Imag;

            // Compute z^2
            double a2 = a * a - b * b;
            double b2 = 2 * a * b;

            // Compute 1 - z^2
            double real1 = 1 - a2;
            double imag1 = -b2;

            // sqrt(1 - z^2)
            double r = Math.Sqrt(Math.Sqrt(real1 * real1 + imag1 * imag1));
            double theta = Math.Atan2(imag1, real1) / 2;
            double sqrtReal = r * Math.Cos(theta);
            double sqrtImag = r * Math.Sin(theta);

            // i * sqrt(1 - z^2)
            double iSqrtReal = -sqrtImag;
            double iSqrtImag = sqrtReal;

            // z + i * sqrt(1 - z^2)
            double sumReal = a + iSqrtReal;
            double sumImag = b + iSqrtImag;

            // log(z + i * sqrt(1 - z^2))
            double modulus = Math.Sqrt(sumReal * sumReal + sumImag * sumImag);
            double arg = Math.Atan2(sumImag, sumReal);
            double logReal = Math.Log(modulus);
            double logImag = arg;

            // -i * log(...)
            double resultReal = logImag;
            double resultImag = -logReal;

            return new Cplx(resultReal, resultImag);
        }

        /// <summary>
        /// Returns the arc sine of a complex number.
        /// Formula: asin(z) = -i * log(i*z + sqrt(1 - z^2))
        /// </summary>
        /// <param name="value">The complex number.</param>
        /// <returns>The arc sine of <paramref name="value"/>.</returns>
        public static Cplx Asin(Cplx value)
        {
            // z = value
            // i = (0, 1)
            double a = value.Real;
            double b = value.Imag;

            // Compute z^2
            double a2 = a * a - b * b;
            double b2 = 2 * a * b;

            // Compute 1 - z^2
            double real1 = 1 - a2;
            double imag1 = -b2;

            // sqrt(1 - z^2)
            double r = Math.Sqrt(Math.Sqrt(real1 * real1 + imag1 * imag1));
            double theta = Math.Atan2(imag1, real1) / 2;
            double sqrtReal = r * Math.Cos(theta);
            double sqrtImag = r * Math.Sin(theta);

            // i*z
            double izReal = -b;
            double izImag = a;

            // i*z + sqrt(1 - z^2)
            double sumReal = izReal + sqrtReal;
            double sumImag = izImag + sqrtImag;

            // log(i*z + sqrt(1 - z^2))
            double modulus = Math.Sqrt(sumReal * sumReal + sumImag * sumImag);
            double arg = Math.Atan2(sumImag, sumReal);
            double logReal = Math.Log(modulus);
            double logImag = arg;

            // -i * log(...)
            double resultReal = -logImag;
            double resultImag = logReal;

            return new Cplx(resultReal, resultImag);
        }

        /// <summary>
        /// Returns the arc tangent of a complex number.
        /// Formula: atan(z) = (i/2) * [log(1 - i*z) - log(1 + i*z)]
        /// </summary>
        /// <param name="value">The complex number.</param>
        /// <returns>The arc tangent of <paramref name="value"/>.</returns>
        public static Cplx Atan(Cplx value)
        {
            // z = value
            // i = (0, 1)
            double a = value.Real;
            double b = value.Imag;

            // Compute i*z
            double izReal = -b;
            double izImag = a;

            // Compute 1 - i*z
            double oneMinusIzReal = 1 - izReal;
            double oneMinusIzImag = -izImag;

            // Compute 1 + i*z
            double onePlusIzReal = 1 + izReal;
            double onePlusIzImag = izImag;

            // log(1 - i*z)
            double mod1 = Math.Sqrt(oneMinusIzReal * oneMinusIzReal + oneMinusIzImag * oneMinusIzImag);
            double arg1 = Math.Atan2(oneMinusIzImag, oneMinusIzReal);
            double log1Real = Math.Log(mod1);
            double log1Imag = arg1;

            // log(1 + i*z)
            double mod2 = Math.Sqrt(onePlusIzReal * onePlusIzReal + onePlusIzImag * onePlusIzImag);
            double arg2 = Math.Atan2(onePlusIzImag, onePlusIzReal);
            double log2Real = Math.Log(mod2);
            double log2Imag = arg2;

            // log(1 - i*z) - log(1 + i*z)
            double diffReal = log1Real - log2Real;
            double diffImag = log1Imag - log2Imag;

            // (i/2) * (diffReal + i*diffImag) = (-diffImag/2, diffReal/2)
            double resultReal = -diffImag / 2.0;
            double resultImag = diffReal / 2.0;

            return new Cplx(resultReal, resultImag);
        }

        /// <summary>
        /// Raises a complex number to a complex power.
        /// Formula: Pow(z, w) = Exp(w * Log(z))
        /// </summary>
        /// <param name="value">The base complex number (z).</param>
        /// <param name="power">The exponent complex number (w).</param>
        /// <returns>The value of <paramref name="value"/> raised to the power <paramref name="power"/>.</returns>
        public static Cplx Pow(Cplx value, Cplx power)
        {
            // Log(z) = ln|z| + i*arg(z)
            double modulus = Math.Sqrt(value.Real * value.Real + value.Imag * value.Imag);
            double arg = Math.Atan2(value.Imag, value.Real);

            // w = c + id
            double c = power.Real;
            double d = power.Imag;

            // w * Log(z) = (c + id) * (ln|z| + i*arg(z))
            // = (c*ln|z| - d*arg(z)) + i(d*ln|z| + c*arg(z))
            double logMod = Math.Log(modulus);
            double x = c * logMod - d * arg;
            double y = d * logMod + c * arg;

            // Exp(x + iy) = exp(x) * (cos(y) + i*sin(y))
            double expx = Math.Exp(x);
            double real = expx * Math.Cos(y);
            double imag = expx * Math.Sin(y);
            return new Cplx(real, imag);
        }

        /// <summary>
        /// Raises a complex number to a real power.
        /// </summary>
        /// <param name="value">The base complex number.</param>
        /// <param name="power">The real exponent.</param>
        /// <returns>The value of <paramref name="value"/> raised to the power <paramref name="power"/>.</returns>
        public static Cplx Pow(Cplx value, double power)
            => Pow(value, new Cplx(real: power, imag: 0));

        /// <summary>
        /// Returns the principal square root of a complex number.
        /// Formula: sqrt(z) = sqrt(r) * (cos(theta/2) + i*sin(theta/2))
        /// where r = |z|, theta = arg(z)
        /// </summary>
        /// <param name="value">The complex number.</param>
        /// <returns>The principal square root of <paramref name="value"/>.</returns>
        public static Cplx Sqrt(Cplx value)
        {
            double a = value.Real;
            double b = value.Imag;

            if (a == 0 && b == 0)
                return new Cplx(0, 0);

            double modulus = Math.Sqrt(a * a + b * b);
            double angle = Math.Atan2(b, a) / 2.0;
            double sqrtMod = Math.Sqrt(modulus);

            double real = sqrtMod * Math.Cos(angle);
            double imag = sqrtMod * Math.Sin(angle);
            return new Cplx(real, imag);
        }

        /// <summary>
        /// Returns the exponential of a complex number.
        /// Formula: exp(z) = exp(a) * (cos(b) + i * sin(b)), where z = a + i*b
        /// </summary>
        /// <param name="value">The complex number.</param>
        /// <returns>The exponential of <paramref name="value"/>.</returns>
        public static Cplx Exp(Cplx value)
        {
            double a = value.Real;
            double b = value.Imag;
            double expA = Math.Exp(a);
            double real = expA * Math.Cos(b);
            double imag = expA * Math.Sin(b);
            return new Cplx(real, imag);
        }

        /// <summary>
        /// Returns the natural (base-e) logarithm of a complex number.
        /// Formula: log(z) = ln|z| + i*arg(z)
        /// </summary>
        /// <param name="value">The complex number.</param>
        /// <returns>The natural logarithm of <paramref name="value"/>.</returns>
        public static Cplx Log(Cplx value)
        {
            double modulus = Math.Sqrt(value.Real * value.Real + value.Imag * value.Imag);
            double arg = Math.Atan2(value.Imag, value.Real);
            return new Cplx(Math.Log(modulus), arg);
        }

        /// <summary>
        /// Returns the base-10 logarithm of a complex number.
        /// Formula: log10(z) = log(z) / ln(10)
        /// </summary>
        /// <param name="value">The complex number.</param>
        /// <returns>The base-10 logarithm of <paramref name="value"/>.</returns>
        public static Cplx Log10(Cplx value)
        {
            Cplx ln = Log(value);
            double invLn10 = 1.0 / Math.Log(10.0);
            return new Cplx(ln.Real * invLn10, ln.Imag * invLn10);
        }

        #endregion
        #region ---- compare ----

        ///<summary>
        /// Compares the magnitude of this complex number to another complex number.
        /// </summary>
        /// <param name="other">The other <see cref="Cplx"/> to compare with.</param>
        /// <returns>
        /// A value less than zero if this instance is less than <paramref name="other"/>; 
        /// zero if this instance is equal to <paramref name="other"/>; 
        /// greater than zero if this instance is greater than <paramref name="other"/>.
        /// </returns>
        public int CompareTo(Cplx other)
        {
            double thisMag = Real * Real + Imag * Imag;
            double otherMag = other.Real * other.Real + other.Imag * other.Imag;
            return thisMag.CompareTo(otherMag);
        }

        /// <summary>
        /// Not supported for complex numbers. Throws <see cref="NotSupportedException"/>.
        /// </summary>
        public int CompareTo(object? obj)
            => throw new NotSupportedException();

        /// <summary>
        /// Determines whether the specified <see cref="Cplx"/> is equal to the current <see cref="Cplx"/>.
        /// </summary>
        /// <param name="other">The complex number to compare with.</param>
        /// <returns><c>true</c> if the values are equal; otherwise, <c>false</c>.</returns>
        public bool Equals(Cplx other) =>
            Real == other.Real && Imag == other.Imag;

        /// <summary>
        /// Determines if the complex number is in canonical form.
        /// </summary>
        public static bool IsCanonical(Cplx value)
            => double.IsFinite(value.Real) && double.IsFinite(value.Imag);

        /// <summary>
        /// Determines whether the specified <see cref="Cplx"/> value is a complex number.
        /// </summary>
        /// <param name="value">The <see cref="Cplx"/> value to check.</param>
        /// <returns>
        /// <c>true</c> if at least one of the real or imaginary parts is non-zero, both parts are finite, and neither part is NaN; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsComplexNumber(Cplx value)
        {
            // A number is a complex number if either the real or imaginary part is non-zero or not NaN/Infinity.
            // Here, we define a complex number as any value where at least one part is not zero, finite, and not NaN.
            return (value.Real != 0.0 || value.Imag != 0.0) &&
                   double.IsFinite(value.Real) && double.IsFinite(value.Imag) &&
                   !double.IsNaN(value.Real) && !double.IsNaN(value.Imag);
        }

        /// <summary>
        /// Determines whether the specified <see cref="Cplx"/> value is a real number.
        /// </summary>
        /// <param name="value">The <see cref="Cplx"/> value to check.</param>
        /// <returns>
        /// <c>true</c> if the imaginary part is zero and the real part is finite; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsRealNumber(Cplx value)
        {
            // A complex number is real if its imaginary part is zero and its real part is finite.
            return value.Imag == 0.0 && double.IsFinite(value.Real);
        }

        /// <summary>
        /// Determines whether the specified <see cref="Cplx"/> value is a pure imaginary number.
        /// </summary>
        /// <param name="value">The <see cref="Cplx"/> value to check.</param>
        /// <returns>
        /// <c>true</c> if the real part is zero, the imaginary part is non-zero and finite; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsImaginaryNumber(Cplx value)
        {
            // A number is a pure imaginary number if its real part is zero and its imaginary part is non-zero and finite.
            return value.Real == 0.0 && value.Imag != 0.0 && double.IsFinite(value.Imag);
        }

        /// <summary>
        /// Determines whether the specified <see cref="Cplx"/> value is an integer.
        /// </summary>
        /// <param name="value">The <see cref="Cplx"/> value to check.</param>
        /// <returns>
        /// <c>true</c> if the imaginary part is zero, the real part is finite, and the real part is an integer; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsInteger(Cplx value)
        {
            // A complex number is an integer if its imaginary part is zero and its real part is a finite integer.
            return value.Imag == 0.0
                && double.IsFinite(value.Real)
                && value.Real == Math.Truncate(value.Real);
        }

        /// <summary>
        /// Determines whether the specified <see cref="Cplx"/> value is an even integer.
        /// </summary>
        /// <param name="value">The <see cref="Cplx"/> value to check.</param>
        /// <returns>
        /// <c>true</c> if the imaginary part is zero, the real part is finite, and the real part is an even integer; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsEvenInteger(Cplx value)
        {
            // A complex number is an even integer if its imaginary part is zero and its real part is an even integer.
            return value.Imag == 0.0 &&
                   double.IsFinite(value.Real) &&
                   value.Real % 2 == 0 &&
                   value.Real == Math.Truncate(value.Real);
        }

        /// <summary>
        /// Determines whether the specified <see cref="Cplx"/> value is an odd integer.
        /// </summary>
        /// <param name="value">The <see cref="Cplx"/> value to check.</param>
        /// <returns>
        /// <c>true</c> if the imaginary part is zero, the real part is finite, is an integer, and the real part is an odd integer; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsOddInteger(Cplx value)
        {
            // A complex number is an odd integer if its imaginary part is zero,
            // the real part is finite, is an integer, and real % 2 == 1 or -1.
            if (value.Imag != 0.0 || !double.IsFinite(value.Real))
                return false;

            double truncated = Math.Truncate(value.Real);
            if (value.Real != truncated)
                return false;

            // Use Math.Abs to handle negative odd integers correctly
            return Math.Abs((long)truncated % 2) == 1;
        }

        /// <summary>
        /// Determines whether both the real and imaginary parts of the specified <see cref="Cplx"/> number are finite values.
        /// </summary>
        /// <param name="value">The <see cref="Cplx"/> number to check.</param>
        /// <returns>
        /// <c>true</c> if both <see cref="Real"/> and <see cref="Imag"/> are finite; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsFinite(Cplx value)
        {
            return double.IsFinite(value.Real) && double.IsFinite(value.Imag);
        }

        /// <summary>
        /// Determines whether either the real or imaginary part of the specified <see cref="Cplx"/> value is infinite.
        /// </summary>
        /// <param name="value">The <see cref="Cplx"/> value to check.</param>
        /// <returns>
        /// <c>true</c> if either the real or imaginary part is positive or negative infinity; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsInfinity(Cplx value)
        {
            // Returns true if either the real or imaginary part is infinity.
            return double.IsInfinity(value.Real) || double.IsInfinity(value.Imag);
        }

        /// <summary>
        /// Determines whether the specified <see cref="Cplx"/> value is zero.
        /// </summary>
        /// <param name="value">The <see cref="Cplx"/> value to check.</param>
        /// <returns>
        /// <c>true</c> if both the real and imaginary parts are zero; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsZero(Cplx value)
        {
            return value.Real == 0.0 && value.Imag == 0.0;
        }

        /// <summary>
        /// Determines whether either the real or imaginary part of the complex number is NaN.
        /// </summary>
        public static bool IsNaN(Cplx value)
            => double.IsNaN(value.Real) || double.IsNaN(value.Imag);

        /// <summary>
        /// Not supported for complex numbers. Throws <see cref="NotSupportedException"/>.
        /// </summary>
        public static bool IsPositive(Cplx value)
            => throw new NotSupportedException();

        /// <summary>
        /// Not supported for complex numbers. Throws <see cref="NotSupportedException"/>.
        /// </summary>
        public static bool IsNegative(Cplx value)
            => throw new NotSupportedException();

        /// <summary>
        /// Not supported for complex numbers. Throws <see cref="NotSupportedException"/>.
        /// </summary>
        public static bool IsPositiveInfinity(Cplx value)
            => throw new NotSupportedException();

        /// <summary>
        /// Not supported for complex numbers. Throws <see cref="NotSupportedException"/>.
        /// </summary>
        public static bool IsNegativeInfinity(Cplx value)
            => throw new NotSupportedException();

        /// <summary>
        /// Determines whether both the real and imaginary parts of the complex number are normal (not zero, subnormal, infinite, or NaN).
        /// </summary>
        /// <param name="value">The <see cref="Cplx"/> value to check.</param>
        /// <returns>
        /// <c>true</c> if both real and imaginary parts are normal floating-point numbers; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsNormal(Cplx value)
            => double.IsNormal(value.Real) && double.IsNormal(value.Imag);

        /// <summary>
        /// Determines whether either the real or imaginary part of the complex number is subnormal.
        /// </summary>
        /// <param name="value">The <see cref="Cplx"/> value to check.</param>
        /// <returns>
        /// <c>true</c> if either the real or imaginary part is subnormal; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsSubnormal(Cplx value)
            => double.IsSubnormal(value.Real) || double.IsSubnormal(value.Imag);

        #endregion
        #region ---- max/min ----

        /// <summary>
        /// Returns the complex number with the greater magnitude (absolute value).
        /// If both have the same magnitude, returns <paramref name="x"/>.
        /// </summary>
        /// <param name="x">The first complex number.</param>
        /// <param name="y">The second complex number.</param>
        /// <returns>The complex number with the greater magnitude.</returns>
        public static Cplx MaxMagnitude(Cplx x, Cplx y)
        {
            double magX = x.Real * x.Real + x.Imag * x.Imag;
            double magY = y.Real * y.Real + y.Imag * y.Imag;
            return (magX >= magY) ? x : y;
        }

        /// <summary>
        /// Returns the complex number with the smaller magnitude (absolute value).
        /// If both have the same magnitude, returns <paramref name="x"/>.
        /// </summary>
        /// <param name="x">The first complex number.</param>
        /// <param name="y">The second complex number.</param>
        /// <returns>The complex number with the smaller magnitude.</returns>
        public static Cplx MinMagnitude(Cplx x, Cplx y)
        {
            double magX = x.Real * x.Real + x.Imag * x.Imag;
            double magY = y.Real * y.Real + y.Imag * y.Imag;
            return (magX <= magY) ? x : y;
        }

        /// <summary>
        /// Returns the complex number with the greater magnitude (absolute value), 
        /// or the one that is not NaN if only one is NaN. 
        /// If both have the same magnitude, returns <paramref name="y"/> if it is not NaN, otherwise <paramref name="x"/>.
        /// </summary>
        /// <param name="x">The first complex number.</param>
        /// <param name="y">The second complex number.</param>
        /// <returns>The complex number with the greater magnitude, or the one that is not NaN.</returns>
        public static Cplx MaxMagnitudeNumber(Cplx x, Cplx y)
        {
            bool xIsNaN = IsNaN(x);
            bool yIsNaN = IsNaN(y);

            if (xIsNaN)
                return y;
            if (yIsNaN)
                return x;

            double magX = x.Real * x.Real + x.Imag * x.Imag;
            double magY = y.Real * y.Real + y.Imag * y.Imag;

            if (magX > magY)
                return x;
            if (magY > magX)
                return y;
            // If magnitudes are equal, return y (matching .NET convention)
            return y;
        }

        /// <summary>
        /// Returns the complex number with the smaller magnitude (absolute value),
        /// or the one that is not NaN if only one is NaN.
        /// If both have the same magnitude, returns <paramref name="y"/> if it is not NaN, otherwise <paramref name="x"/>.
        /// </summary>
        /// <param name="x">The first complex number.</param>
        /// <param name="y">The second complex number.</param>
        /// <returns>The complex number with the smaller magnitude, or the one that is not NaN.</returns>
        public static Cplx MinMagnitudeNumber(Cplx x, Cplx y)
        {
            bool xIsNaN = IsNaN(x);
            bool yIsNaN = IsNaN(y);

            if (xIsNaN)
                return y;
            if (yIsNaN)
                return x;

            double magX = x.Real * x.Real + x.Imag * x.Imag;
            double magY = y.Real * y.Real + y.Imag * y.Imag;

            if (magX < magY)
                return x;
            if (magY < magX)
                return y;
            // If magnitudes are equal, return y (matching .NET convention)
            return y;
        }

        #endregion
        #region ---- string ----

        /// <summary>
        /// Returns a string representation of the complex number in the form "a + bi".
        /// </summary>
        /// <returns>A string representation of the complex number.</returns>
        public override string ToString() => $"{Real} + {Imag}i";

        /// <summary>
        /// Formats the complex number as a string using the specified format and format provider.
        /// </summary>
        /// <param name="format">The format string.</param>
        /// <param name="formatProvider">The format provider.</param>
        /// <returns>A string representation of the complex number.</returns>
        public string ToString(string? format, IFormatProvider? formatProvider)
        {
            // Use the provided format for both real and imaginary parts, defaulting to "G" if null.
            string realStr = Real.ToString(format ?? "G", formatProvider);
            string imagStr = Imag.ToString(format ?? "G", formatProvider);
            char sign = Imag < 0 ? '-' : '+';
            // Always show the sign for the imaginary part, and use the absolute value for formatting.
            return $"{realStr} {sign} {Math.Abs(Imag).ToString(format ?? "G", formatProvider)}i";
        }

        #endregion
        #region ---- parse ----

        /// <summary>
        /// Parses a string representation of a complex number.
        /// Expects format "a+bi" or "a-bi".
        /// </summary>
        /// <param name="s">The string to parse.</param>
        /// <param name="provider">The format provider.</param>
        /// <returns>The parsed <see cref="Cplx"/> value.</returns>
        public static Cplx Parse(string s, IFormatProvider provider)
        {
            // Simple parser: expects "a+bi" or "a-bi"
            s = s.Replace(" ", "").Replace("i", "");
            var parts = s.Split(new[] { '+', '-' }, StringSplitOptions.RemoveEmptyEntries);
            double real = double.Parse(parts[0], provider);
            double imag = parts.Length > 1 ? double.Parse(parts[1], provider) : 0;
            if (s.Contains("-") && s.LastIndexOf('-') > 0)
                imag = -imag;
            return new Cplx(real, imag);
        }

        /// <summary>
        /// Attempts to parse a string representation of a complex number.
        /// </summary>
        /// <param name="s">The string to parse.</param>
        /// <param name="provider">The format provider.</param>
        /// <param name="result">The parsed <see cref="Cplx"/> value if successful; otherwise, <see cref="Zero"/>.</param>
        /// <returns><c>true</c> if parsing succeeded; otherwise, <c>false</c>.</returns>
        public static bool TryParse(string s, IFormatProvider provider, out Cplx result)
        {
            try
            {
                result = Parse(s, provider);
                return true;
            }
            catch
            {
                result = Zero;
                return false;
            }
        }


        /// <summary>
        /// Not implemented. Throws <see cref="NotImplementedException"/>.
        /// </summary>
        public static Cplx Parse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Not implemented. Throws <see cref="NotImplementedException"/>.
        /// </summary>
        public static Cplx Parse(string s, NumberStyles style, IFormatProvider? provider)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Not implemented. Throws <see cref="NotImplementedException"/>.
        /// </summary>
        public static bool TryParse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider, [MaybeNullWhen(false)] out Cplx result)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Not implemented. Throws <see cref="NotImplementedException"/>.
        /// </summary>
        public static bool TryParse([NotNullWhen(true)] string? s, NumberStyles style, IFormatProvider? provider, [MaybeNullWhen(false)] out Cplx result)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Not implemented. Throws <see cref="NotImplementedException"/>.
        /// </summary>
        public static Cplx Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Not implemented. Throws <see cref="NotImplementedException"/>.
        /// </summary>
        public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, [MaybeNullWhen(false)] out Cplx result)
        {
            throw new NotImplementedException();
        }

        #endregion
        #region ---- misc ----

        /// <summary>
        /// Not implemented. Throws <see cref="NotImplementedException"/>.
        /// </summary>
        public static bool TryConvertFromChecked<TOther>(TOther value, [MaybeNullWhen(false)] out Cplx result) 
            where TOther : INumberBase<TOther>
        {
            // Plan / Pseudocode:
            // 1. If the provided value is already a Cplx instance, return it directly.
            // 2. Otherwise attempt a checked conversion to double using the generic numeric helper:
            //      double real = double.CreateChecked<TOther>(value);
            // 3. On success set result = new Cplx(real, 0.0) and return true.
            // 4. If any exception occurs (overflow, invalid cast, etc.), set result = Zero and return false.
            try
            {
                if (value is Cplx c)
                {
                    result = c;
                    return true;
                }

                Real real = Real.CreateChecked<TOther>(value);
                result = new Cplx(real, 0.0);
                return true;
            }
            catch
            {
                result = Zero;
                return false;
            }
        }

        /// <summary>
        /// Not implemented. Throws <see cref="NotImplementedException"/>.
        /// </summary>
        public static bool TryConvertFromSaturating<TOther>(TOther value, [MaybeNullWhen(false)] out Cplx result) where TOther : INumberBase<TOther>
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Not implemented. Throws <see cref="NotImplementedException"/>.
        /// </summary>
        public static bool TryConvertFromTruncating<TOther>(TOther value, [MaybeNullWhen(false)] out Cplx result) where TOther : INumberBase<TOther>
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Not implemented. Throws <see cref="NotImplementedException"/>.
        /// </summary>
        public static bool TryConvertToChecked<TOther>(Cplx value, [MaybeNullWhen(false)] out TOther result) where TOther : INumberBase<TOther>
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Not implemented. Throws <see cref="NotImplementedException"/>.
        /// </summary>
        public static bool TryConvertToSaturating<TOther>(Cplx value, [MaybeNullWhen(false)] out TOther result) where TOther : INumberBase<TOther>
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Not implemented. Throws <see cref="NotImplementedException"/>.
        /// </summary>
        public static bool TryConvertToTruncating<TOther>(Cplx value, [MaybeNullWhen(false)] out TOther result) where TOther : INumberBase<TOther>
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Not implemented. Throws <see cref="NotImplementedException"/>.
        /// </summary>
        public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
        {
            throw new NotImplementedException();
        }

        #endregion

        #endregion
        #region operators

        #region ---- plus ----

        /// <summary>
        /// Returns the same complex number.
        /// </summary>
        /// <param name="value">The complex number.</param>
        /// <returns>The same complex number.</returns>
        public static Cplx operator +(Cplx value) => value;

        /// <summary>
        /// Adds two complex numbers.
        /// </summary>
        /// <param name="left">The first complex number.</param>
        /// <param name="right">The second complex number.</param>
        /// <returns>The sum of the two complex numbers.</returns>
        public static Cplx operator +(Cplx left, Cplx right) => Add(left, right);

        /// <summary>
        /// Adds a complex number and a real number, returning the result as a new 
        /// complex number.
        /// </summary>
        /// <param name="left">The complex number to add.</param>
        /// <param name="right">The real number to add.</param>
        /// <returns>A new complex number representing the sum of 
        /// the compelx and the real number.</returns>
        public static Cplx operator +(Cplx left, double right) => Add(left, right);

        /// <summary>
        /// Adds a real number to a complex number and returns the resulting complex number.
        /// </summary>
        /// <param name="left">The real number to add.</param>
        /// <param name="right">The complex number to add.</param>
        /// <returns>A new complex number representing the sum of 
        /// the real number and the complex number.</returns>
        public static Cplx operator +(double left, Cplx right) => Add(left, right);

        #endregion
        #region ---- minus ----

        /// <summary>
        /// Negates a complex number.
        /// </summary>
        /// <param name="value">The complex number to negate.</param>
        /// <returns>A new complex number with negated real and imaginary parts.</returns>
        public static Cplx operator -(Cplx value) => Negate(value);

        /// <summary>
        /// Subtracts a complex number from a real number.
        /// </summary>
        /// <param name="left">The real number to subtract from.</param>
        /// <param name="right">The complex number to subtract.</param>
        /// <returns>A complex number representing the result of the subtraction.</returns>
        public static Cplx operator -(double left, Cplx right) => Subtract(left, right);

        /// <summary>
        /// Subtracts a real number from a complex number.
        /// </summary>
        /// <param name="left">The complex number to subtract from.</param>
        /// <param name="right">The real number to subtract.</param>
        /// <returns>A complex number representing the result of the subtraction.</returns>
        public static Cplx operator -(Cplx left, double right) => Subtract(left, right);

        /// <summary>
        /// Subtracts a complex number from another complex number.
        /// </summary>
        /// <param name="left">The first complex number.</param>
        /// <param name="right">The second complex number.</param>
        /// <returns>The difference of the two complex numbers.</returns>
        public static Cplx operator -(Cplx left, Cplx right) => Subtract(left, right);

        #endregion
        #region ---- multiply ----

        /// <summary>
        /// Multiplies a complex number by a scalar value.
        /// </summary>
        /// <param name="left">The complex number to be multiplied.</param>
        /// <param name="right">The real number to multiply the complex number by.</param>
        /// <returns>A new complex number that is the result of multiplying <paramref name="left"/> by <paramref
        /// name="right"/>.</returns>
        public static Cplx operator *(Cplx left, double right) => Multiply(left, right);

        /// <summary>
        /// Multiplies a real number by a complex number and returns the resulting complex number.
        /// </summary>
        /// <param name="left">The real number to multiply.</param>
        /// <param name="right">The complex number to multiply.</param>
        /// <returns>A new complex number representing the product of the real number and the complex number.</returns>
        public static Cplx operator *(double left, Cplx right) => Multiply(left, right);

        /// <summary>
        /// Multiplies two complex numbers.
        /// </summary>
        /// <param name="left">The first complex number.</param>
        /// <param name="right">The second complex number.</param>
        /// <returns>The product of the two complex numbers.</returns>
        public static Cplx operator *(Cplx left, Cplx right) => Multiply(left, right);

        #endregion
        #region ---- divide ----

        /// <summary>
        /// Divides one complex number by another.
        /// </summary>
        /// <param name="left">The numerator complex number.</param>
        /// <param name="right">The denominator complex number.</param>
        /// <returns>The quotient of the two complex numbers.</returns>
        public static Cplx operator /(Cplx left, Cplx right) => Divide(left, right);

        /// <summary>
        /// Divides a real number by a complex number and returns the resulting complex number.
        /// </summary>
        /// <param name="left">The real number to be divided.</param>
        /// <param name="right">The complex number to divide by. Must not be zero.</param>
        /// <returns>A new complex number representing the result of dividing <paramref name="left"/> by <paramref
        /// name="right"/>.</returns>
        public static Cplx operator /(double left, Cplx right) => Divide(left, right);

        /// <summary>
        /// Divides a complex number by a real number.
        /// </summary>
        /// <param name="left">The complex number to be divided.</param>
        /// <param name="right">The scalar value by which to divide the complex number.</param>
        /// <returns>A new complex number representing the result of the division.</returns>
        public static Cplx operator /(Cplx left, double right) => Divide(left, right);

        #endregion
        #region ---- in/decrement ----

        /// <summary>
        /// Increments the real part of the complex number by 1.
        /// </summary>
        /// <param name="value">The complex number to increment.</param>
        /// <returns>
        /// A new <see cref="Cplx"/> whose real part is incremented by 1 and whose imaginary part is unchanged.
        /// </returns>
        public static Cplx operator ++(Cplx value)
            => new(real: value.Real + 1.0, imag: value.Imag);

        /// <summary>
        /// Decrements the real part of the complex number by 1.
        /// </summary>
        /// <param name="value">The complex number to decrement.</param>
        /// <returns>
        /// A new <see cref="Cplx"/> whose real part is decremented by 1 and whose imaginary part is unchanged.
        /// </returns>
        public static Cplx operator --(Cplx value)
            => new(real: value.Real - 1.0, imag: value.Imag);

        #endregion
        #region ---- equal/unequal ----

        /// <summary>
        /// Determines whether two <see cref="Cplx"/> numbers are equal.
        /// </summary>
        /// <param name="left">The first complex number to compare.</param>
        /// <param name="right">The second complex number to compare.</param>
        /// <returns>
        /// <c>true</c> if the real and imaginary parts of <paramref name="left"/> and <paramref name="right"/> are equal; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator ==(Cplx left, Cplx right)
            => left.Equals(right);

        /// <summary>
        /// Determines whether two <see cref="Cplx"/> numbers are not equal.
        /// </summary>
        /// <param name="left">The first complex number to compare.</param>
        /// <param name="right">The second complex number to compare.</param>
        /// <returns>
        /// <c>true</c> if the real or imaginary parts of <paramref name="left"/> and <paramref name="right"/> are not equal; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator !=(Cplx left, Cplx right)
            => !left.Equals(right);

        #endregion
        #region ---- not supported ----

        /// <summary>
        /// Not supported for complex numbers. Throws <see cref="NotSupportedException"/>.
        /// </summary>
        public static bool operator >(Cplx left, Cplx right)
            => throw new NotSupportedException();

        /// <summary>
        /// Not supported for complex numbers. Throws <see cref="NotSupportedException"/>.
        /// </summary>
        public static bool operator >=(Cplx left, Cplx right)
            => throw new NotSupportedException();

        /// <summary>
        /// Not supported for complex numbers. Throws <see cref="NotSupportedException"/>.
        /// </summary>
        public static bool operator <(Cplx left, Cplx right)
            => throw new NotSupportedException();

        /// <summary>
        /// Not supported for complex numbers. Throws <see cref="NotSupportedException"/>.
        /// </summary>
        public static bool operator <=(Cplx left, Cplx right)
            => throw new NotSupportedException();

        /// <summary>
        /// Not supported for complex numbers. Throws <see cref="NotSupportedException"/>.
        /// </summary>
        public static Cplx operator %(Cplx left, Cplx right)
            => throw new NotSupportedException();

        #endregion
        #region ---- implicit ----

        /// <summary>
        /// Defines an implicit conversion from an unsigned integer to a <see cref="Cplx"/> number.
        /// </summary>
        /// <param name="value">The unsigned integer value to convert.</param>
        public static implicit operator Cplx(nuint value)
            => new(real: (double)value, imag: 0.0);

        /// <summary>
        /// Implicitly converts a <see cref="byte"/> value to a <see cref="Cplx"/> number.
        /// </summary>
        /// <param name="value">The <see cref="byte"/> value to convert.</param>
        public static implicit operator Cplx(byte value)
            => new(real: (double)value, imag: 0.0);

        /// <summary>
        /// Converts a <see cref="char"/> value to a <see cref="Cplx"/> number.
        /// </summary>
        /// <param name="value">The <see cref="char"/> value to convert.</param>
        public static implicit operator Cplx(char value)
            => new(real: (double)value, imag: 0.0);

        /// <summary>
        /// Implicitly converts a <see cref="double"/> value to a <see cref="Cplx"/> number.
        /// </summary>
        /// <param name="value">The real part of the complex number, represented as a <see cref="double"/>.</param>
        public static implicit operator Cplx(double value)
            => new(real: value, imag: 0.0);

        /// <summary>
        /// Implicitly converts a 16-bit signed integer (<see cref="short"/>) to a <see cref="Cplx"/> number.
        /// </summary>
        /// <param name="value">The 16-bit signed integer to convert.</param>
        public static implicit operator Cplx(short value)
            => new(real: (double)value, imag: 0.0);

        /// <summary>
        /// Converts a <see cref="Half"/> value to a <see cref="Cplx"/> number.
        /// </summary>
        /// <param name="value">The <see cref="Half"/> value to convert.</param>
        public static implicit operator Cplx(Half value)
            => new(real: (double)value, imag: 0.0);

        /// <summary>
        /// Converts a <see cref="long"/> value to a <see cref="Cplx"/> number.
        /// </summary>
        /// <param name="value">The <see cref="long"/> value to convert.</param>
        public static implicit operator Cplx(long value)
            => new(real: (double)value, imag: 0.0);

        /// <summary>
        /// Converts a native integer (<see langword="nint"/>) to a <see cref="Cplx"/> number.
        /// </summary>
        /// <param name="value">The native integer to convert.</param>
        public static implicit operator Cplx(nint value)
            => new(real: (double)value, imag: 0.0);

        /// <summary>
        /// Converts a signed byte (<see langword="sbyte"/>) to a <see cref="Cplx"/> number.
        /// </summary>
        /// <param name="value">The signed byte value to convert.</param>
        public static implicit operator Cplx(sbyte value)
            => new(real: (double)value, imag: 0.0);

        /// <summary>
        /// Converts a single-precision floating-point value to a <see cref="Cplx"/> number.
        /// </summary>
        /// <param name="value">The single-precision floating-point value to convert.</param>
        public static implicit operator Cplx(float value)
            => new(real: (double)value, imag: 0.0);

        /// <summary>
        /// Converts a <see langword="ushort"/> value to a <see cref="Cplx"/> number.
        /// </summary>
        /// <param name="value">The <see langword="ushort"/> value to convert.</param>
        public static implicit operator Cplx(ushort value)
            => new(real: (double)value, imag: 0.0);

        /// <summary>
        /// Defines an implicit conversion from an unsigned integer to a <see cref="Cplx"/> number.
        /// </summary>
        /// <param name="value">The unsigned integer to convert.</param>
        public static implicit operator Cplx(uint value)
            => new(real: (double)value, imag: 0.0);

        /// <summary>
        /// Converts an unsigned long integer to a <see cref="Cplx"/> number.
        /// </summary>
        /// <param name="value">The unsigned long integer to convert.</param>
        public static implicit operator Cplx(ulong value)
            => new(real: (double)value, imag: 0.0);

        /// <summary>
        /// Implicitly converts an integer to a <see cref="Cplx"/> number.
        /// </summary>
        /// <param name="value">The integer value to convert.</param>
        public static implicit operator Cplx(int value)
            => new(real: (double)value, imag: 0.0);

        /// <summary>
        /// Converts a <see cref="BigInteger"/> value to a <see cref="Cplx"/> number.
        /// </summary>
        /// <param name="value">The <see cref="BigInteger"/> value to convert.</param>
        public static implicit operator Cplx(BigInteger value)
            => new(real: (double)value, imag: 0.0);

        /// <summary>
        /// Converts an <see cref="Int128"/> value to a <see cref="Cplx"/> number.
        /// </summary>
        /// <param name="value">The <see cref="Int128"/> value to convert.</param>
        public static implicit operator Cplx(Int128 value)
            => new(real: (double)value, imag: 0.0);

        /// <summary>
        /// Converts a <see cref="decimal"/> value to a <see cref="Cplx"/> number.
        /// </summary>
        /// <param name="value">The <see cref="decimal"/> value to convert.</param>
        public static implicit operator Cplx(decimal value)
            => new(real: (double)value, imag: 0.0);

        /// <summary>
        /// Converts a <see cref="UInt128"/> value to a <see cref="Cplx"/> number.
        /// </summary>
        /// <param name="value">The <see cref="UInt128"/> value to convert.</param>
        public static implicit operator Cplx(UInt128 value)
            => new(real: (double)value, imag: 0.0);

        public override bool Equals(object obj)
        {
            return obj is Cplx && Equals((Cplx)obj);
        }

        #endregion

        #endregion
    }

}
