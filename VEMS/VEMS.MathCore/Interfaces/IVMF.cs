using System.Runtime.InteropServices;
using Complex = System.Numerics.Complex;

namespace VEMS.MathCore
{
    /// <summary>
    /// VMF interface 
    /// </summary>
    public unsafe interface IVMF
    {
        #region ---- Abs ----

        /// <summary>
        /// Computes the absolute value of each element in the input array <paramref name="a"/> and stores the result in <paramref name="y"/>.
        /// y[i] = |a[i]|
        /// </summary>
        /// <param name="n">The number of elements in the input array.</param>
        /// <param name="a">Pointer to the input array of double values.</param>
        /// <param name="y">Pointer to the output array where the absolute values will be stored.</param>
        void Abs(long n, [In] double* a, [Out] double* y);

        /// <summary>
        /// Computes the absolute value of each element in the input complex array <paramref name="a"/> and stores the result in <paramref name="y"/>.
        /// y[i] = |a[i]|
        /// </summary>
        /// <param name="n">The number of elements in the input array.</param>
        /// <param name="a">Pointer to the input array of complex values.</param>
        /// <param name="y">Pointer to the output array where the absolute values will be stored as doubles.</param>
        void Abs(long n, [In] void* a, [Out] double* y);


        // wrapper
        /// <summary>
        /// computes absolute value of array elements
        /// </summary>
        /// <typeparam name="T"> ArrayBase[double] </typeparam>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="y"> result array y </param>
        void AbsD<T>(long n, T a, ref T y)
            where T : DenseArrayBase<double>;

        /// <summary>
        /// computes absolute value of array elements
        /// </summary>
        /// <typeparam name="T1"> ArrayBase[Complex] </typeparam>
        /// <typeparam name="T2"> ArrayBase[double] </typeparam>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="y"> result array y </param>
        public void AbsZ<T1, T2>(long n, T1 a, ref T2 y)
            where T1 : DenseArrayBase<Complex>
            where T2 : DenseArrayBase<double>;

        #endregion
        #region ---- Arg ----

        /// <summary>
        /// Computes the argument (phase angle) of each element in a complex array.
        /// y[i] = arg(a[i])
        /// </summary>
        /// <param name="n">The number of elements in the array.</param>
        /// <param name="a">Pointer to the input array of complex numbers.</param>
        /// <param name="y">Pointer to the output array for the computed arguments (in radians).</param>
        void Arg(long n, [In] void* a, [Out] double* y);


        // wrapper
        /// <summary>
        /// computes argument of a complex array's elements
        /// </summary>
        /// <typeparam name="T1"> ArrayBase[Complex] </typeparam>
        /// <typeparam name="T2"> ArrayBase[double] </typeparam>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="y"> result array y </param>
        public void ArgZ<T1, T2>(long n, T1 a, ref T2 y)
            where T1 : DenseArrayBase<Complex>
            where T2 : DenseArrayBase<double>;

        #endregion
        #region ---- Add ----

        /// <summary>
        /// Performs element-wise addition of two double arrays.
        /// y[i] = a[i] + b[i]
        /// </summary>
        /// <param name="n">Number of elements in the arrays.</param>
        /// <param name="a">Pointer to the first input array.</param>
        /// <param name="b">Pointer to the second input array.</param>
        /// <param name="y">Pointer to the output array where the result is stored.</param>
        void Add(long n, [In] double* a, [In] double* b, [Out] double* y);

        /// <summary>
        /// Performs element-wise addition of two complex arrays.
        /// y[i] = a[i] + b[i]
        /// </summary>
        /// <param name="n">Number of elements in the arrays.</param>
        /// <param name="a">Pointer to the first input array (complex values).</param>
        /// <param name="b">Pointer to the second input array (complex values).</param>
        /// <param name="y">Pointer to the output array where the result is stored (complex values).</param>
        void Add(long n, [In] void* a, [In] void* b, [Out] void* y);


        // wrapper
        /// <summary>
        /// performs element by element addition of array a and array b
        /// y = a + b
        /// </summary>
        /// <typeparam name="T"> ArrayBase[double] </typeparam>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="b"> input array b </param>
        /// <param name="y"> result array y </param>
        void AddD<T>(long n, T a, T b, ref T y)
            where T : DenseArrayBase<double>;

        /// <summary>
        /// performs element by element addition of array a and array b
        /// y = a + b
        /// </summary>
        /// <typeparam name="T"> ArrayBase[Complex] </typeparam>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="b"> input array b </param>
        /// <param name="y"> result array y </param>
        void AddZ<T>(long n, T a, T b, ref T y)
            where T : DenseArrayBase<Complex>;

        #endregion
        #region ---- Sub ----

        /// <summary>
        /// Performs element-wise subtraction of two arrays of doubles.
        /// y[i] = a[i] - b[i]
        /// </summary>
        /// <param name="n">Number of elements.</param>
        /// <param name="a">Pointer to the first input array.</param>
        /// <param name="b">Pointer to the second input array.</param>
        /// <param name="y">Pointer to the output array where the result is stored.</param>
        void Sub(long n, [In] double* a, [In] double* b, [Out] double* y);

        /// <summary>
        /// Performs element-wise subtraction of two arrays of complex numbers.
        /// y[i] = a[i] - b[i]
        /// </summary>
        /// <param name="n">Number of elements.</param>
        /// <param name="a">Pointer to the first input array (complex).</param>
        /// <param name="b">Pointer to the second input array (complex).</param>
        /// <param name="y">Pointer to the output array where the result is stored (complex).</param>
        void Sub(long n, [In] void* a, [In] void* b, [Out] void* y);


        // wrapper
        /// <summary>
        /// performs element by element substraction of array b from array a
        /// y = a - b
        /// </summary>
        /// <typeparam name="T"> ArrayBase[double] </typeparam>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="b"> input array b </param>
        /// <param name="y"> result array y </param>
        void SubD<T>(long n, T a, T b, ref T y)
            where T : DenseArrayBase<double>;

        /// <summary>
        /// performs element by element substraction of array b from array a
        /// y = a - b
        /// </summary>
        /// <typeparam name="T"> ArrayBase[Complex] </typeparam>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="b"> input array b </param>
        /// <param name="y"> result array y </param>
        void SubZ<T>(long n, T a, T b, ref T y)
            where T : DenseArrayBase<Complex>;

        #endregion
        #region ---- Inv ----

        /// <summary>
        /// Computes the element-wise inverse of a double-precision array.
        /// y[i] = 1.0 / a[i]
        /// </summary>
        /// <param name="n">The number of elements in the input array.</param>
        /// <param name="a">Pointer to the input array.</param>
        /// <param name="y">Pointer to the output array where the result is stored.</param>
        void Inv(long n, [In] double* a, [Out] double* y);

        
        // wrapper
        /// <summary>
        /// performs element by element inversion of the array
        /// </summary>
        /// <typeparam name="T"> ArrayBase[double] </typeparam>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="y"> result array y </param>
        void InvD<T>(long n, T a, ref T y)
            where T : DenseArrayBase<double>;

        #endregion
        #region ---- Sqrt ----

        /// <summary>
        /// Computes the square root of each element in the input array <paramref name="a"/> and stores the result in <paramref name="y"/>.
        /// y[i] = a[i]^0.5
        /// </summary>
        /// <param name="n">The number of elements in the input and output arrays.</param>
        /// <param name="a">Pointer to the input array of double values.</param>
        /// <param name="y">Pointer to the output array where the square roots will be stored.</param>
        void Sqrt(long n, [In] double* a, [Out] double* y);

        /// <summary>
        /// Computes the square root of each element in the input array <paramref name="a"/> (complex values) and stores the result in <paramref name="y"/>.
        /// y[i] = a[i]^0.5
        /// </summary>
        /// <param name="n">The number of elements in the input and output arrays.</param>
        /// <param name="a">Pointer to the input array of complex values.</param>
        /// <param name="y">Pointer to the output array where the square roots will be stored.</param>
        void Sqrt(long n, [In] void* a, [Out] void* y);


        // wrapper
        /// <summary>
        /// computes a square root of array elements
        /// </summary>
        /// <typeparam name="T"> ArrayBase[double] </typeparam>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="y"> result array y </param>
        void SqrtD<T>(long n, T a, ref T y)
            where T : DenseArrayBase<double>;

        /// <summary>
        /// computes a square root of array elements
        /// </summary>
        /// <typeparam name="T"> ArrayBase[Complex] </typeparam>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="y"> result array y </param>
        void SqrtZ<T>(long n, T a, ref T y)
            where T : DenseArrayBase<Complex>;

        #endregion
        #region ---- InvSqrt ----

        /// <summary>
        /// Computes the inverse square root of each element in the input array <paramref name="a"/> and stores the result in the output array <paramref name="y"/>.
        /// y[i] = 1/a[i]^0.5
        /// </summary>
        /// <param name="n">The number of elements to process.</param>
        /// <param name="a">Pointer to the input array of double values.</param>
        /// <param name="y">Pointer to the output array where the results will be stored.</param>
        void InvSqrt(long n, [In] double* a, [Out] double* y);


        // wrapper
        /// <summary>
        /// computes an inverse square root of array elements
        /// </summary>
        /// <typeparam name="T"> ArrayBase[double] </typeparam>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="y"> result array y </param>
        void InvSqrtD<T>(long n, T a, ref T y)
            where T : DenseArrayBase<double>;

        #endregion
        #region ---- Cbrt ----

        /// <summary>
        /// Computes the cube root of each element in the input array <paramref name="a"/> and stores the result in the output array <paramref name="y"/>.
        /// y[i] = a[i]^(1/3)
        /// </summary>
        /// <param name="n">The number of elements in the input and output arrays.</param>
        /// <param name="a">Pointer to the input array of double-precision values.</param>
        /// <param name="y">Pointer to the output array where the cube root results will be stored.</param>
        void Cbrt(long n, [In] double* a, [Out] double* y);


        // wrapper
        /// <summary>
        /// computes a cube root of array elements
        /// </summary>
        /// <typeparam name="T"> ArrayBase[double] </typeparam>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="y"> result array y </param>
        void CbrtD<T>(long n, T a, ref T y)
            where T : DenseArrayBase<double>;

        #endregion
        #region ---- InvCbrt ----

        /// <summary>
        /// Computes the inverse cube root of each element in the input array <paramref name="a"/> and stores the result in <paramref name="y"/>.
        /// y[i] = 1/a[i]^(1/3)
        /// </summary>
        /// <param name="n">The number of elements in the input and output arrays.</param>
        /// <param name="a">Pointer to the input array of doubles.</param>
        /// <param name="y">Pointer to the output array of doubles where the results will be stored.</param>
        void InvCbrt(long n, [In] double* a, [Out] double* y);


        // wrapper
        /// <summary>
        /// computes an inverse cube root of array elements
        /// </summary>
        /// <typeparam name="T"> ArrayBase[double] </typeparam>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="y"> result array y </param>
        void InvCbrtD<T>(long n, T a, ref T y)
            where T : DenseArrayBase<double>;

        #endregion
        #region ---- Sqr ----

        /// <summary>
        /// Computes the element-wise square of the input array <paramref name="a"/> and stores the result in <paramref name="y"/>.
        /// y[i] = a[i]^2
        /// </summary>
        /// <param name="n">The number of elements in the input array.</param>
        /// <param name="a">Pointer to the input array of doubles.</param>
        /// <param name="y">Pointer to the output array of doubles where the result will be stored.</param>
        void Sqr(long n, [In] double* a, [Out] double* y);


        // wrapper
        /// <summary>
        /// performs element by element squaring of the array
        /// </summary>
        /// <typeparam name="T"> ArrayBase[double] </typeparam>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="y"> result array y </param>
        void SqrD<T>(long n, T a, ref T y)
            where T : DenseArrayBase<double>;

        #endregion
        #region ---- Exp ----

        /// <summary>
        /// Computes the exponential of each element in the input array <paramref name="a"/> and stores the result in <paramref name="y"/>.
        /// y[i] = e^a[i]
        /// </summary>
        /// <param name="n">The number of elements in the input and output arrays.</param>
        /// <param name="a">Pointer to the input array of doubles.</param>
        /// <param name="y">Pointer to the output array of doubles where the results will be stored.</param>
        void Exp(long n, [In] double* a, [Out] double* y);

        /// <summary>
        /// Computes the exponential of each element in the input array <paramref name="a"/> (complex values) and stores the result in <paramref name="y"/>.
        /// y[i] = e^a[i]
        /// </summary>
        /// <param name="n">The number of elements in the input and output arrays.</param>
        /// <param name="a">Pointer to the input array of complex values.</param>
        /// <param name="y">Pointer to the output array of complex values where the results will be stored.</param>
        void Exp(long n, [In] void* a, [Out] void* y);


        // wrapper
        /// <summary>
        /// computes an exponential of array elements
        /// </summary>
        /// <typeparam name="T"> ArrayBase[double] </typeparam>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="y"> result array y </param>
        void ExpD<T>(long n, T a, ref T y)
            where T : DenseArrayBase<double>;

        /// <summary>
        /// computes an exponential of array elements
        /// </summary>
        /// <typeparam name="T"> ArrayBase[Complex] </typeparam>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="y"> result array y </param>
        void ExpZ<T>(long n, T a, ref T y)
            where T : DenseArrayBase<Complex>;

        #endregion
        #region ---- Exp2 ----

        /// <summary>
        /// Computes the base-2 exponential of each element in the input array <paramref name="a"/> and stores the result in the output array <paramref name="y"/>.
        /// y[i] = 2^a[i]
        /// </summary>
        /// <param name="n">The number of elements to process.</param>
        /// <param name="a">Pointer to the input array of double-precision values.</param>
        /// <param name="y">Pointer to the output array where the results will be stored.</param>
        void Exp2(long n, [In] double* a, [Out] double* y);

        #endregion
        #region ---- Exp10 ----

        /// <summary>
        /// Computes the base 10 exponential of each element in the input array <paramref name="a"/> and stores the result in the output array <paramref name="y"/>.
        /// y[i] = 10^a[i]
        /// </summary>
        /// <param name="n">The number of elements to process.</param>
        /// <param name="a">Pointer to the input array of double values.</param>
        /// <param name="y">Pointer to the output array where the results will be stored.</param>
        void Exp10(long n, [In] double* a, [Out] double* y);

        #endregion
        #region ---- Expm1 ----

        /// <summary>
        /// Computes exp(a) - 1 for each element of the input array <paramref name="a"/> and stores the result in <paramref name="y"/>.
        /// y[i] = e^a[i] -1
        /// </summary>
        /// <param name="n">The number of elements in the input and output arrays.</param>
        /// <param name="a">Pointer to the input array of double values.</param>
        /// <param name="y">Pointer to the output array where the results will be stored.</param>
        void Expm1(long n, [In] double* a, [Out] double* y);

        #endregion
        #region ---- Ln ----

        /// <summary>
        /// Computes the natural logarithm of each element in a double-precision array.
        /// y[i] = ln(a[i])
        /// </summary>
        /// <param name="n">Number of elements in the input array.</param>
        /// <param name="a">Pointer to the input array of double values.</param>
        /// <param name="y">Pointer to the output array where the results will be stored.</param>
        void Ln(long n, [In] double* a, [Out] double* y);

        /// <summary>
        /// Computes the natural logarithm of each element in a complex double-precision array.
        /// y[i] = ln(a[i])
        /// </summary>
        /// <param name="n">Number of elements in the input array.</param>
        /// <param name="a">Pointer to the input array of complex values.</param>
        /// <param name="y">Pointer to the output array where the results will be stored.</param>
        void Ln(long n, [In] void* a, [Out] void* y);


        // wrapper
        /// <summary>
        /// computes natural logarithm of array elements
        /// </summary>
        /// <typeparam name="T"> ArrayBase[double] </typeparam>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="y"> result array y </param>
        void LnD<T>(long n, T a, ref T y)
            where T : DenseArrayBase<double>;

        /// <summary>
        /// computes natural logarithm of array elements
        /// </summary>
        /// <typeparam name="T"> ArrayBase[Complex] </typeparam>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="y"> result array y </param>
        void LnZ<T>(long n, T a, ref T y)
            where T : DenseArrayBase<Complex>;

        #endregion
        #region ---- Log2 ----

        /// <summary>
        /// Computes the base 2 logarithm of each element in the input array <paramref name="a"/> and stores the result in the output array <paramref name="y"/>.
        /// y[i] = lb(a[i])
        /// </summary>
        /// <param name="n">The number of elements to process.</param>
        /// <param name="a">Pointer to the input array of double values.</param>
        /// <param name="y">Pointer to the output array where the results will be stored.</param>
        void Log2(long n, [In] double* a, [Out] double* y);

        #endregion
        #region ---- Log10 ----

        /// <summary>
        /// Computes the base 10 logarithm of each element in the input array <paramref name="a"/> and stores the result in <paramref name="y"/>.
        /// y[i] = lg(a[i])
        /// </summary>
        /// <param name="n">The number of elements in the input array.</param>
        /// <param name="a">Pointer to the input array of type double.</param>
        /// <param name="y">Pointer to the output array of type double where the results will be stored.</param>
        void Log10(long n, [In] double* a, [Out] double* y);

        /// <summary>
        /// Computes the base 10 logarithm of each element in the input array <paramref name="a"/> (complex values) and stores the result in <paramref name="y"/>.
        /// y[i] = lg(a[i])
        /// </summary>
        /// <param name="n">The number of elements in the input array.</param>
        /// <param name="a">Pointer to the input array of complex values.</param>
        /// <param name="y">Pointer to the output array where the results will be stored.</param>
        void Log10(long n, [In] void* a, [Out] void* y);


        // wrapper
        /// <summary>
        /// computes the base 10 logarithm of array elements
        /// </summary>
        /// <typeparam name="T"> ArrayBase[double] </typeparam>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="y"> result array y </param>
        void Log10D<T>(long n, T a, ref T y)
            where T : DenseArrayBase<double>;

        /// <summary>
        /// computes the base 10 logarithm of array elements
        /// </summary>
        /// <typeparam name="T"> ArrayBase[double] </typeparam>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="y"> result array y </param>
        void Log10Z<T>(long n, T a, ref T y)
            where T : DenseArrayBase<Complex>;

        #endregion
        #region ---- Log1p ----

        /// <summary>
        /// Computes the natural logarithm of (1 + a) for each element of the input array <paramref name="a"/>,
        /// and stores the result in the output array <paramref name="y"/>.
        /// y[i] = log(1+a[i])
        /// </summary>
        /// <param name="n">The number of elements in the input and output arrays.</param>
        /// <param name="a">Pointer to the input array of double values.</param>
        /// <param name="y">Pointer to the output array where the results will be stored.</param>
        void Log1p(long n, [In] double* a, [Out] double* y);

        #endregion
        #region ---- Logb ----

        /// <summary>
        /// Computes the unbiased exponent (base 2) for each element of the input array <paramref name="a"/>,
        /// and stores the result in the output array <paramref name="y"/>.
        /// y[i] = logb(a[i])
        /// </summary>
        /// <param name="n">The number of elements in the input and output arrays.</param>
        /// <param name="a">Pointer to the input array of double-precision floating-point values.</param>
        /// <param name="y">Pointer to the output array where the results will be stored.</param>
        void Logb(long n, [In] double* a, [Out] double* y);

        #endregion
        #region ---- Cos ----

        /// <summary>
        /// Computes the cosine of each element in the input array <paramref name="a"/> and stores the result in <paramref name="y"/>.
        /// y[i] = cos(a[i])
        /// </summary>
        /// <param name="n">The number of elements in the input array.</param>
        /// <param name="a">Pointer to the input array of double values.</param>
        /// <param name="y">Pointer to the output array where the cosine values will be stored.</param>
        void Cos(long n, [In] double* a, [Out] double* y);

        /// <summary>
        /// Computes the cosine of each element in the input array <paramref name="a"/> (complex values) and stores the result in <paramref name="y"/>.
        /// y[i] = cos(a[i])
        /// </summary>
        /// <param name="n">The number of elements in the input array.</param>
        /// <param name="a">Pointer to the input array of complex values.</param>
        /// <param name="y">Pointer to the output array where the cosine values will be stored.</param>
        void Cos(long n, [In] void* a, [Out] void* y);


        // wrapper
        /// <summary>
        /// computes cosine of array elements
        /// </summary>
        /// <typeparam name="T"> ArrayBase[double] </typeparam>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="y"> result array y </param>
        void CosD<T>(long n, T a, ref T y)
            where T : DenseArrayBase<double>;

        /// <summary>
        /// computes cosine of array elements
        /// </summary>
        /// <typeparam name="T"> ArrayBase[Complex] </typeparam>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="y"> result array y </param>
        void CosZ<T>(long n, T a, ref T y)
            where T : DenseArrayBase<Complex>;

        #endregion
        #region ---- Sin ----

        /// <summary>
        /// Computes the sine of each element in the input array <paramref name="a"/> and stores the result in <paramref name="y"/>.
        /// y[i] = sin(a[i])
        /// </summary>
        /// <param name="n">The number of elements in the input array.</param>
        /// <param name="a">Pointer to the input array of type double.</param>
        /// <param name="y">Pointer to the output array of type double where the results will be stored.</param>
        void Sin(long n, [In] double* a, [Out] double* y);

        /// <summary>
        /// Computes the sine of each element in the input array <paramref name="a"/> (complex values) and stores the result in <paramref name="y"/>.
        /// y[i] = sin(a[i])
        /// </summary>
        /// <param name="n">The number of elements in the input array.</param>
        /// <param name="a">Pointer to the input array of complex values.</param>
        /// <param name="y">Pointer to the output array of complex values where the results will be stored.</param>
        void Sin(long n, [In] void* a, [Out] void* y);


        // wrapper
        /// <summary>
        /// computes sine of array elements
        /// </summary>
        /// <typeparam name="T"> ArrayBase[double] </typeparam>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="y"> result array y </param>
        void SinD<T>(long n, T a, ref T y)
            where T : DenseArrayBase<double>;

        /// <summary>
        /// computes sine of array elements
        /// </summary>
        /// <typeparam name="T"> ArrayBase[Complex] </typeparam>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="y"> result array y </param>
        void SinZ<T>(long n, T a, ref T y)
            where T : DenseArrayBase<Complex>;

        #endregion
        #region ---- Tan ----

        /// <summary>
        /// Computes the tangent of each element in the input array <paramref name="a"/> and stores the result in <paramref name="y"/>.
        /// y[i] = tan(a[i])
        /// </summary>
        /// <param name="n">The number of elements in the input array.</param>
        /// <param name="a">Pointer to the input array of type double.</param>
        /// <param name="y">Pointer to the output array of type double, where the results will be stored.</param>
        void Tan(long n, [In] double* a, [Out] double* y);

        /// <summary>
        /// Computes the tangent of each element in the input array <paramref name="a"/> (complex values) and stores the result in <paramref name="y"/>.
        /// y[i] = tan(a[i])
        /// </summary>
        /// <param name="n">The number of elements in the input array.</param>
        /// <param name="a">Pointer to the input array of complex values.</param>
        /// <param name="y">Pointer to the output array of complex values, where the results will be stored.</param>
        void Tan(long n, [In] void* a, [Out] void* y);


        // wrapper
        /// <summary>
        /// computes tangent of array elements
        /// </summary>
        /// <typeparam name="T"> ArrayBase[double] </typeparam>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="y"> result array y </param>
        void TanD<T>(long n, T a, ref T y)
            where T : DenseArrayBase<double>;

        /// <summary>
        /// computes tangent of array elements
        /// </summary>
        /// <typeparam name="T"> ArrayBase[Complex] </typeparam>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="y"> result array y </param>
        void TanZ<T>(long n, T a, ref T y)
            where T : DenseArrayBase<Complex>;

        #endregion
        #region ---- Cospi ----

        /// <summary>
        /// Computes the cosine of each element in the input array <paramref name="a"/>,
        /// where the argument is interpreted as a multiple of π (i.e., computes cos(π * a[i]) for each element).
        /// y[i] = cos(a[i]*PI)
        /// </summary>
        /// <param name="n">The number of elements in the input array.</param>
        /// <param name="a">Pointer to the input array of double values.</param>
        /// <param name="y">Pointer to the output array where the results will be stored.</param>
        void Cospi(long n, [In] double* a, [Out] double* y);

        #endregion
        #region ---- Sinpi ----

        /// <summary>
        /// Computes the sine of each element of the input array multiplied by pi.
        /// y[i] = sin(a[i]*PI)
        /// </summary>
        /// <param name="n">The number of elements in the input array.</param>
        /// <param name="a">Pointer to the input array of double values.</param>
        /// <param name="y">Pointer to the output array where the results will be stored.</param>
        void Sinpi(long n, [In] double* a, [Out] double* y);

        #endregion
        #region ---- Tanpi ----

        /// <summary>
        /// Computes the tangent of each element of the input array multiplied by pi.
        /// y[i] = tan(a[i]*PI)
        /// </summary>
        /// <param name="n">The number of elements in the input array.</param>
        /// <param name="a">Pointer to the input array of double values.</param>
        /// <param name="y">Pointer to the output array where the results will be stored.</param>
        void Tanpi(long n, [In] double* a, [Out] double* y);

        #endregion
        #region ---- Cosd ----

        /// <summary>
        /// Computes the cosine of each element in the input array <paramref name="a"/> (in degrees)
        /// and stores the result in the output array <paramref name="y"/>.
        /// y[i] = cos(a[i]*PI/180)
        /// </summary>
        /// <param name="n">The number of elements in the input and output arrays.</param>
        /// <param name="a">Pointer to the input array of double values (in degrees).</param>
        /// <param name="y">Pointer to the output array where the cosine values will be stored.</param>
        void Cosd(long n, [In] double* a, [Out] double* y);

        #endregion
        #region ---- Sind ----

        /// <summary>
        /// Computes the sine of each element in the input array <paramref name="a"/> (in degrees)
        /// and stores the result in the output array <paramref name="y"/>.
        /// y[i] = sin(a[i]*PI/180)
        /// </summary>
        /// <param name="n">The number of elements in the input and output arrays.</param>
        /// <param name="a">Pointer to the input array of double values (in degrees).</param>
        /// <param name="y">Pointer to the output array where the sine values will be stored.</param>
        void Sind(long n, [In] double* a, [Out] double* y);

        #endregion
        #region ---- Tand ----

        /// <summary>
        /// Computes the tangent (in degrees) of each element in the input array <paramref name="a"/>,
        /// storing the result in the output array <paramref name="y"/>.
        /// y[i] = tan(a[i]*PI/180)
        /// </summary>
        /// <param name="n">The number of elements in the input and output arrays.</param>
        /// <param name="a">Pointer to the input array of double values (angles in degrees).</param>
        /// <param name="y">Pointer to the output array where the tangent values will be stored.</param>
        void Tand(long n, [In] double* a, [Out] double* y);

        #endregion
        #region ---- Cosh ----

        /// <summary>
        /// Computes the hyperbolic cosine of each element in the input array <paramref name="a"/> and stores the result in <paramref name="y"/>.
        /// y[i] = ch(a[i])
        /// </summary>
        /// <param name="n">The number of elements in the input array.</param>
        /// <param name="a">Pointer to the input array of type double.</param>
        /// <param name="y">Pointer to the output array of type double where the results will be stored.</param>
        void Cosh(long n, [In] double* a, [Out] double* y);

        /// <summary>
        /// Computes the hyperbolic cosine of each element in the input array <paramref name="a"/> (complex values) and stores the result in <paramref name="y"/>.
        /// y[i] = ch(a[i])
        /// </summary>
        /// <param name="n">The number of elements in the input array.</param>
        /// <param name="a">Pointer to the input array of complex values.</param>
        /// <param name="y">Pointer to the output array of complex values where the results will be stored.</param>
        void Cosh(long n, [In] void* a, [Out] void* y);


        // wrapper
        /// <summary>
        /// computes hyperbolic cosine of array elements
        /// </summary>
        /// <typeparam name="T"> ArrayBase[double] </typeparam>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="y"> result array y </param>
        void CoshD<T>(long n, T a, ref T y)
            where T : DenseArrayBase<double>;

        /// <summary>
        /// computes hyperbolic cosine of array elements
        /// </summary>
        /// <typeparam name="T"> ArrayBase[Complex] </typeparam>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="y"> result array y </param>
        void CoshZ<T>(long n, T a, ref T y)
            where T : DenseArrayBase<Complex>;

        #endregion
        #region ---- Sinh ----

        /// <summary>
        /// Computes the hyperbolic sine of each element in the input array <paramref name="a"/> and stores the result in <paramref name="y"/>.
        /// y[i] = sh(a[i])
        /// </summary>
        /// <param name="n">The number of elements in the input array.</param>
        /// <param name="a">Pointer to the input array of type double.</param>
        /// <param name="y">Pointer to the output array of type double where the results will be stored.</param>
        void Sinh(long n, [In] double* a, [Out] double* y);

        /// <summary>
        /// Computes the hyperbolic sine of each element in the input array <paramref name="a"/> (complex values) and stores the result in <paramref name="y"/>.
        /// y[i] = sh(a[i])
        /// </summary>
        /// <param name="n">The number of elements in the input array.</param>
        /// <param name="a">Pointer to the input array of complex values.</param>
        /// <param name="y">Pointer to the output array of complex values where the results will be stored.</param>
        void Sinh(long n, [In] void* a, [Out] void* y);


        // wrapper
        /// <summary>
        /// computes hyperbolic sine of array elements
        /// </summary>
        /// <typeparam name="T"> ArrayBase[double] </typeparam>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="y"> result array y </param>
        void SinhD<T>(long n, T a, ref T y)
            where T : DenseArrayBase<double>;

        /// <summary>
        /// computes hyperbolic sine of array elements
        /// </summary>
        /// <typeparam name="T"> ArrayBase[Complex] </typeparam>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="y"> result array y </param>
        void SinhZ<T>(long n, T a, ref T y)
            where T : DenseArrayBase<Complex>;

        #endregion
        #region ---- Tanh ----

        /// <summary>
        /// Computes the hyperbolic tangent of each element in the input array <paramref name="a"/> and stores the result in <paramref name="y"/>.
        /// y[i] = th(a[i])
        /// </summary>
        /// <param name="n">The number of elements in the input array.</param>
        /// <param name="a">Pointer to the input array of type double.</param>
        /// <param name="y">Pointer to the output array of type double where the results are stored.</param>
        void Tanh(long n, [In] double* a, [Out] double* y);

        /// <summary>
        /// Computes the hyperbolic tangent of each element in the input array <paramref name="a"/> (complex values) and stores the result in <paramref name="y"/>.
        /// y[i] = th(a[i])
        /// </summary>
        /// <param name="n">The number of elements in the input array.</param>
        /// <param name="a">Pointer to the input array of complex values.</param>
        /// <param name="y">Pointer to the output array of complex values where the results are stored.</param>
        void Tanh(long n, [In] void* a, [Out] void* y);


        // wrapper
        /// <summary>
        /// computes hyperbolic tangent of array elements
        /// </summary>
        /// <typeparam name="T"> ArrayBase[double] </typeparam>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="y"> result array y </param>
        void TanhD<T>(long n, T a, ref T y)
            where T : DenseArrayBase<double>;

        /// <summary>
        /// computes hyperbolic tangent of array elements
        /// </summary>
        /// <typeparam name="T"> ArrayBase[Complex] </typeparam>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="y"> result array y </param>
        void TanhZ<T>(long n, T a, ref T y)
            where T : DenseArrayBase<Complex>;

        #endregion
        #region ---- Acos ----

        /// <summary>
        /// Computes the inverse cosine (arccos) of each element in the input array <paramref name="a"/>,
        /// storing the result in the output array <paramref name="y"/>.
        /// y[i] = arccos(a[i])
        /// /summary>
        /// <param name="n">The number of elements in the input and output arrays.</param>
        /// <param name="a">Pointer to the input array of double values.</param>
        /// <param name="y">Pointer to the output array where the results will be stored.</param>
        void Acos(long n, [In] double* a, [Out] double* y);

        /// <summary>
        /// Computes the inverse cosine (arccos) of each element in the input array <paramref name="a"/>,
        /// storing the result in the output array <paramref name="y"/>. This overload supports complex input.
        /// y[i] = arccos(a[i])
        /// </summary>
        /// <param name="n">The number of elements in the input and output arrays.</param>
        /// <param name="a">Pointer to the input array of complex values.</param>
        /// <param name="y">Pointer to the output array where the results will be stored.</param>
        void Acos(long n, [In] void* a, [Out] void* y);


        // wrapper
        /// <summary>
        /// computes inverse cosine of array elements
        /// </summary>
        /// <typeparam name="T"> ArrayBase[double] </typeparam>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="y"> result array y </param>
        void AcosD<T>(long n, T a, ref T y)
            where T : DenseArrayBase<double>;

        /// <summary>
        /// computes inverse cosine of array elements
        /// </summary>
        /// <typeparam name="T"> ArrayBase[Complex] </typeparam>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="y"> result array y </param>
        void AcosZ<T>(long n, T a, ref T y)
            where T : DenseArrayBase<Complex>;

        #endregion
        #region ---- Asin ----

        /// <summary>
        /// Computes the inverse sine (arcsin) of each element in the input array <paramref name="a"/>,
        /// storing the result in the output array <paramref name="y"/>.
        /// y[i] = arcsin(a[i])
        /// </summary>
        /// <param name="n">The number of elements in the input and output arrays.</param>
        /// <param name="a">Pointer to the input array of type double.</param>
        /// <param name="y">Pointer to the output array of type double, where the results will be stored.</param>
        void Asin(long n, [In] double* a, [Out] double* y);

        /// <summary>
        /// Computes the inverse sine (arcsin) of each element in the input array <paramref name="a"/> (complex),
        /// storing the result in the output array <paramref name="y"/> (complex).
        /// y[i] = arcsin(a[i])
        /// </summary>
        /// <param name="n">The number of elements in the input and output arrays.</param>
        /// <param name="a">Pointer to the input array of type complex (void*).</param>
        /// <param name="y">Pointer to the output array of type complex (void*), where the results will be stored.</param>
        void Asin(long n, [In] void* a, [Out] void* y);


        // wrapper
        /// <summary>
        /// computes inverse sine of array elements
        /// </summary>
        /// <typeparam name="T"> ArrayBase[double] </typeparam>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="y"> result array y </param>
        void AsinD<T>(long n, T a, ref T y)
            where T : DenseArrayBase<double>;

        /// <summary>
        /// computes inverse sine of array elements
        /// </summary>
        /// <typeparam name="T"> ArrayBase[Complex] </typeparam>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="y"> result array y </param>
        void AsinZ<T>(long n, T a, ref T y)
            where T : DenseArrayBase<Complex>;

        #endregion
        #region ---- Atan ----

        /// <summary>
        /// Computes the element-wise inverse tangent (arctangent) of a double-precision array.
        /// y[i] = arctan(a[i])
        /// </summary>
        /// <param name="n">Number of elements in the input array.</param>
        /// <param name="a">Pointer to the input array of double values.</param>
        /// <param name="y">Pointer to the output array where the results will be stored.</param>
        void Atan(long n, [In] double* a, [Out] double* y);

        /// <summary>
        /// Computes the element-wise inverse tangent (arctangent) of a complex array.
        /// y[i] = arctan(a[i])
        /// </summary>
        /// <param name="n">Number of elements in the input array.</param>
        /// <param name="a">Pointer to the input array of complex values.</param>
        /// <param name="y">Pointer to the output array where the results will be stored.</param>
        void Atan(long n, [In] void* a, [Out] void* y);


        // wrapper
        /// <summary>
        /// Computes inverse tangent of array elements
        /// </summary>
        /// <typeparam name="T"> ArrayBase[double] </typeparam>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="y"> result array y </param>
        void AtanD<T>(long n, T a, ref T y)
            where T : DenseArrayBase<double>;

        /// <summary>
        /// Computes inverse tangent of array elements
        /// </summary>
        /// <typeparam name="T"> ArrayBase[Complex] </typeparam>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="y"> result array y </param>
        void AtanZ<T>(long n, T a, ref T y)
            where T : DenseArrayBase<Complex>;

        #endregion
        #region ---- Acospi ----

        /// <summary>
        /// Computes the inverse cosine (in units of pi) of each element in the input array <paramref name="a"/>,
        /// storing the result in the output array <paramref name="y"/>.
        /// y[i] = arccos(a[i])/PI
        /// </summary>
        /// <param name="n">The number of elements in the input and output arrays.</param>
        /// <param name="a">Pointer to the input array of double values.</param>
        /// <param name="y">Pointer to the output array where the results will be stored.</param>
        void Acospi(long n, [In] double* a, [Out] double* y);

        #endregion
        #region ---- Asinpi ----

        /// <summary>
        /// Computes the inverse sine of each element in the input array <paramref name="a"/> (in units of pi), 
        /// and stores the result in the output array <paramref name="y"/>.
        /// y[i] = arcsin(a[i])/PI
        /// </summary>
        /// <param name="n">The number of elements in the input and output arrays.</param>
        /// <param name="a">Pointer to the input array of double values.</param>
        /// <param name="y">Pointer to the output array where the results will be stored.</param>
        void Asinpi(long n, [In] double* a, [Out] double* y);

        #endregion
        #region ---- Atanpi ----

        /// <summary>
        /// Computes the inverse tangent (arctangent) of each element in the input array <paramref name="a"/>,
        /// with the result expressed in multiples of π (pi), and stores the result in the output array <paramref name="y"/>.
        /// y[i] = arctan(a[i])/PI
        /// </summary>
        /// <param name="n">The number of elements in the input and output arrays.</param>
        /// <param name="a">Pointer to the input array of double-precision values.</param>
        /// <param name="y">Pointer to the output array where the results will be stored.</param>
        void Atanpi(long n, [In] double* a, [Out] double* y);

        #endregion
        #region ---- Acosh ----

        /// <summary>
        /// Computes the inverse hyperbolic cosine (acosh) of each element in the input array <paramref name="a"/>,
        /// storing the result in the output array <paramref name="y"/>.
        /// y[i] = arcch(a[i])
        /// </summary>
        /// <param name="n">The number of elements in the input and output arrays.</param>
        /// <param name="a">Pointer to the input array of double-precision values.</param>
        /// <param name="y">Pointer to the output array where the results will be stored.</param>
        void Acosh(long n, [In] double* a, [Out] double* y);

        /// <summary>
        /// Computes the inverse hyperbolic cosine (acosh) of each element in the input array <paramref name="a"/>,
        /// storing the result in the output array <paramref name="y"/>. This overload supports complex values.
        /// y[i] = arcch(a[i])
        /// </summary>
        /// <param name="n">The number of elements in the input and output arrays.</param>
        /// <param name="a">Pointer to the input array of complex values.</param>
        /// <param name="y">Pointer to the output array where the results will be stored.</param>
        void Acosh(long n, [In] void* a, [Out] void* y);


        // wrapper
        /// <summary>
        /// computes inverse hyperbolic cosine (nonnegative) of array elements
        /// </summary>
        /// <typeparam name="T"> ArrayBase[double] </typeparam>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="y"> result array y </param>
        void AcoshD<T>(long n, T a, ref T y)
            where T : DenseArrayBase<double>;

        /// <summary>
        /// computes inverse hyperbolic cosine (nonnegative) of array elements
        /// </summary>
        /// <typeparam name="T"> ArrayBase[Complex] </typeparam>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="y"> result array y </param>
        void AcoshZ<T>(long n, T a, ref T y)
            where T : DenseArrayBase<Complex>;

        #endregion
        #region ---- Asinh ----

        /// <summary>
        /// Computes the inverse hyperbolic sine (asinh) of each element in a double-precision array.
        /// y[i] = arcsh(a[i])
        /// </summary>
        /// <param name="n">Number of elements in the input array.</param>
        /// <param name="a">Pointer to the input array of type double.</param>
        /// <param name="y">Pointer to the output array of type double, where the results will be stored.</param>
        void Asinh(long n, [In] double* a, [Out] double* y);

        /// <summary>
        /// Computes the inverse hyperbolic sine (asinh) of each element in a complex array.
        /// y[i] = arcsh(a[i])
        /// </summary>
        /// <param name="n">Number of elements in the input array.</param>
        /// <param name="a">Pointer to the input array of type Complex (as void*).</param>
        /// <param name="y">Pointer to the output array of type Complex (as void*), where the results will be stored.</param>
        void Asinh(long n, [In] void* a, [Out] void* y);


        // wrapper
        /// <summary>
        /// computes inverse hyperbolic sine of array elements
        /// </summary>
        /// <typeparam name="T"> ArrayBase[double] </typeparam>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="y"> result array y </param>
        void AsinhD<T>(long n, T a, ref T y)
            where T : DenseArrayBase<double>;

        /// <summary>
        /// computes inverse hyperbolic sine of array elements
        /// </summary>
        /// <typeparam name="T"> ArrayBase[Complex] </typeparam>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="y"> result array y </param>
        void AsinhZ<T>(long n, T a, ref T y)
            where T : DenseArrayBase<Complex>;

        #endregion
        #region ---- Atanh ----

        /// <summary>
        /// Computes the inverse hyperbolic tangent (atanh) of each element in the input array <paramref name="a"/>,
        /// storing the result in the output array <paramref name="y"/>.
        /// y[i] = arcth(a[i])
        /// </summary>
        /// <param name="n">The number of elements in the input and output arrays.</param>
        /// <param name="a">Pointer to the input array of type double.</param>
        /// <param name="y">Pointer to the output array of type double.</param>
        void Atanh(long n, [In] double* a, [Out] double* y);

        /// <summary>
        /// Computes the inverse hyperbolic tangent (atanh) of each element in the input array <paramref name="a"/>,
        /// storing the result in the output array <paramref name="y"/>. This overload supports complex input and output.
        /// y[i] = arcth(a[i])
        /// </summary>
        /// <param name="n">The number of elements in the input and output arrays.</param>
        /// <param name="a">Pointer to the input array of type Complex (as void*).</param>
        /// <param name="y">Pointer to the output array of type Complex (as void*).</param>
        void Atanh(long n, [In] void* a, [Out] void* y);


        /// <summary>
        /// computes inverse hyperbolic tangent of array elements
        /// </summary>
        /// <typeparam name="T"> ArrayBase[double] </typeparam>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="y"> result array y </param>
        void AtanhD<T>(long n, T a, ref T y)
            where T : DenseArrayBase<double>;

        /// <summary>
        /// computes inverse hyperbolic tangent of array elements
        /// </summary>
        /// <typeparam name="T"> ArrayBase[Complex] </typeparam>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="y"> result array y </param>
        void AtanhZ<T>(long n, T a, ref T y)
            where T : DenseArrayBase<Complex>;

        #endregion
        #region ---- Erf ----

        /// <summary>
        /// Computes the error function erf(a) for each element of the input array <paramref name="a"/>,
        /// storing the result in the output array <paramref name="y"/>.
        /// y[i] = erf(a[i])
        /// </summary>
        /// <param name="n">The number of elements in the input and output arrays.</param>
        /// <param name="a">Pointer to the input array of double values.</param>
        /// <param name="y">Pointer to the output array where the computed error function values will be stored.</param>
        void Erf(long n, [In] double* a, [Out] double* y);

        #endregion
        #region ---- ErfInv ----

        /// <summary>
        /// Computes the inverse error function for each element of the input array.
        /// y[i] = erfinv(a[i])
        /// </summary>
        /// <param name="n">The number of elements in the input array.</param>
        /// <param name="a">Pointer to the input array of double values.</param>
        /// <param name="y">Pointer to the output array where the results will be stored.</param>
        void ErfInv(long n, [In] double* a, [Out] double* y);

        #endregion
        #region ---- Hypot ----

        /// <summary>
        /// Computes the element-wise hypotenuse of two arrays.
        /// For each element i: y[i] = sqrt(a[i]^2 + b[i]^2)
        /// </summary>
        /// <param name="n">The number of elements in the input arrays.</param>
        /// <param name="a">Pointer to the first input array.</param>
        /// <param name="b">Pointer to the second input array.</param>
        /// <param name="y">Pointer to the output array where results are stored.</param>
        void Hypot(long n, [In] double* a, [In] double* b,
            [Out] double* y);


        // wrapper
        /// <summary>
        /// computes a square root of sum of two squared elements
        /// </summary>
        /// <typeparam name="T"> ArrayBase[double] </typeparam>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="b"> input array b </param>
        /// <param name="y"> result array y </param>
        void HypotD<T>(long n, T a, T b, ref T y)
            where T : DenseArrayBase<double>;

        #endregion
        #region ---- Erfc ----

        /// <summary>
        /// Computes the complementary error function for each element in the input array.
        /// y[i] = 1 - erf(a[i])
        /// </summary>
        /// <param name="n">The number of elements in the input array.</param>
        /// <param name="a">Pointer to the input array of double values.</param>
        /// <param name="y">Pointer to the output array where the results will be stored.</param>
        void Erfc(long n, [In] double* a, [Out] double* y);

        #endregion
        #region ---- ErfcInv ----

        /// <summary>
        /// Computes the inverse complementary error function for each element of the input array.
        /// y[i] = erfcinv(a[i])
        /// </summary>
        /// <param name="n">The number of elements in the input array.</param>
        /// <param name="a">Pointer to the input array of double values.</param>
        /// <param name="y">Pointer to the output array where the results will be stored.</param>
        void ErfcInv(long n, [In] double* a, [Out] double* y);

        #endregion
        #region ---- Erfcx ----

        /// <summary>
        /// Computes the scaled complementary error function for an array of double-precision values.
        /// y[i] = erfcx(a[i])
        /// </summary>
        /// <param name="n">The number of elements in the input and output arrays.</param>
        /// <param name="a">Pointer to the input array of double-precision values.</param>
        /// <param name="y">Pointer to the output array where the results will be stored.</param>
        void Erfcx(long n, [In] double* a, [Out] double* y);

        #endregion
        #region ---- CdfNorm ----

        /// <summary>
        /// Computes the cumulative distribution function (CDF) of the standard normal distribution
        /// for each element in the input array <paramref name="a"/> and stores the result in <paramref name="y"/>.
        /// y[i] = cdfnorm(a[i])
        /// </summary>
        /// <param name="n">The number of elements in the input and output arrays.</param>
        /// <param name="a">Pointer to the input array containing the values for which to compute the CDF.</param>
        /// <param name="y">Pointer to the output array where the computed CDF values will be stored.</param>
        void CdfNorm(long n, [In] double* a, [Out] double* y);

        #endregion
        #region ---- CdfNormInv ----

        /// <summary>
        /// Computes the inverse of the cumulative distribution function (quantile function) 
        /// of the standard normal distribution for each element in the input array.
        /// y[i] = cdfnorminv(a[i])
        /// </summary>
        /// <param name="n">The number of elements in the input array <paramref name="a"/> and output array <paramref name="y"/>.</param>
        /// <param name="a">Pointer to the input array containing probability values (in the range [0, 1]).</param>
        /// <param name="y">Pointer to the output array where the computed quantile values will be stored.</param>
        void CdfNormInv(long n, [In] double* a, [Out] double* y);

        #endregion
        #region ---- LGamma ----

        /// <summary>
        /// Computes the natural logarithm of the gamma function for each element in the input array.
        /// y[i] = lgamma(a[i])
        /// </summary>
        /// <param name="n">The number of elements in the input array.</param>
        /// <param name="a">Pointer to the input array of double values.</param>
        /// <param name="y">Pointer to the output array where the results will be stored.</param>
        void LGamma(long n, [In] double* a, [Out] double* y);

        #endregion
        #region ---- TGamma ----

        /// <summary>
        /// Computes the gamma function for each element of the input array <paramref name="a"/> and stores the result in <paramref name="y"/>.
        /// y[i] = tgamma(a[i])
        /// </summary>
        /// <param name="n">The number of elements in the input and output arrays.</param>
        /// <param name="a">Pointer to the input array of double values.</param>
        /// <param name="y">Pointer to the output array where the computed gamma values will be stored.</param>
        void TGamma(long n, [In] double* a, [Out] double* y);

        #endregion
        #region ---- I0 ----

        /// <summary>
        /// Computes the modified Bessel function of the first kind of order zero (I0) for each element in the input array.
        /// y[i] = i0(a[i])
        /// </summary>
        /// <param name="n">The number of elements in the input array <paramref name="a"/> and output array <paramref name="y"/>.</param>
        /// <param name="a">Pointer to the input array of double values.</param>
        /// <param name="y">Pointer to the output array where the computed I0 values will be stored.</param>
        void I0(long n, [In] double* a, [Out] double* y);

        #endregion
        #region ---- I1 ----

        /// <summary>
        /// Computes the modified Bessel function of the first kind, order 1, for each element in the input array.
        /// y[i] = i1(a[i])
        /// </summary>
        /// <param name="n">The number of elements in the input array <paramref name="a"/> and output array <paramref name="y"/>.</param>
        /// <param name="a">Pointer to the input array of double values.</param>
        /// <param name="y">Pointer to the output array where the computed values will be stored.</param>
        void I1(long n, [In] double* a, [Out] double* y);

        #endregion
        #region ---- J0 ----

        /// <summary>
        /// Computes the Bessel function of the first kind of order 0 (J0) for each element in the input array.
        /// y[i] = j0(a[i])
        /// </summary>
        /// <param name="n">The number of elements in the input array <paramref name="a"/> and output array <paramref name="y"/>.</param>
        /// <param name="a">Pointer to the input array of double values.</param>
        /// <param name="y">Pointer to the output array where the computed J0 values will be stored.</param>
        void J0(long n, [In] double* a, [Out] double* y);

        #endregion
        #region ---- J1 ----

        /// <summary>
        /// Computes the Bessel function of the first kind of order 1 for each element in the input array.
        /// y[i] = j1(a[i])
        /// </summary>
        /// <param name="n">The number of elements in the input array.</param>
        /// <param name="a">Pointer to the input array of double values.</param>
        /// <param name="y">Pointer to the output array where the results will be stored.</param>
        void J1(long n, [In] double* a, [Out] double* y);

        #endregion
        #region ---- Y0 ----

        /// <summary>
        /// Computes the Bessel function of the second kind, order zero, for each element in the input array.
        /// y[i] = y0(a[i])
        /// </summary>
        /// <param name="n">The number of elements in the input array <paramref name="a"/> and output array <paramref name="y"/>.</param>
        /// <param name="a">Pointer to the input array of double values.</param>
        /// <param name="y">Pointer to the output array where the computed results will be stored.</param>
        void Y0(long n, [In] double* a, [Out] double* y);

        #endregion
        #region ---- Y1 ----

        /// <summary>
        /// Computes the Bessel function of the second kind, order one (Y1), for each element in the input array.
        /// y[i] = y1(a[i])
        /// </summary>
        /// <param name="n">The number of elements in the input array <paramref name="a"/> and output array <paramref name="y"/>.</param>
        /// <param name="a">Pointer to the input array of double values.</param>
        /// <param name="y">Pointer to the output array where the computed Y1 values will be stored.</param>
        void Y1(long n, [In] double* a, [Out] double* y);

        #endregion
        #region ---- Jn ----

        /// <summary>
        /// Computes the Bessel function of the first kind Jn(a[i], b) for each element of the input array.
        /// y[i] = jn(a[i],b)
        /// </summary>
        /// <param name="n">Number of elements in the input array <paramref name="a"/> and output array <paramref name="y"/>.</param>
        /// <param name="a">Pointer to the input array of double values (orders for the Bessel function).</param>
        /// <param name="b">Scalar double value (argument for the Bessel function).</param>
        /// <param name="y">Pointer to the output array of double values where the results will be stored.</param>
        void Jn(long n, [In] double* a, double b,
            [Out] double* y);

        #endregion
        #region ---- Yn ----

        /// <summary>
        /// Computes the Bessel function of the second kind Yn(a[i], b) for each element in the input array.
        /// y[i] = yn(a[i],b)
        /// </summary>
        /// <param name="n">The number of elements in the input array <paramref name="a"/> and output array <paramref name="y"/>.</param>
        /// <param name="a">Pointer to the input array of orders for the Bessel function.</param>
        /// <param name="b">The value at which the Bessel function is evaluated for all elements.</param>
        /// <param name="y">Pointer to the output array where the results are stored.</param>
        void Yn(long n, [In] double* a, double b,
            [Out] double* y);

        #endregion
        #region ---- Atan2 ----

        /// <summary>
        /// Computes the four-quadrant inverse tangent (arctangent) of elements of two input arrays.
        /// For each element i, computes y[i] = atan2(a[i], b[i]).
        /// r[i] = arctan(a[i]/b[i])
        /// </summary>
        /// <param name="n">Number of elements in the input arrays.</param>
        /// <param name="a">Pointer to the first input array (numerator values).</param>
        /// <param name="b">Pointer to the second input array (denominator values).</param>
        /// <param name="y">Pointer to the output array where results are stored.</param>
        void Atan2(long n, [In] double* a, [In] double* b,
            [Out] double* y);


        // wrapper
        /// <summary>
        /// computes four-quadrant inverse tangent of elements of two vectors
        /// </summary>
        /// <typeparam name="T"> ArrayBase[double] </typeparam>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input vector a </param>
        /// <param name="b"> input vector b </param>
        /// <param name="y"> result vector y </param>
        void Atan2D<T>(long n, T a, T b, ref T y)
            where T : DenseArrayBase<double>;

        #endregion
        #region ---- Atan2pi ----

        /// <summary>
        /// Computes the four-quadrant inverse tangent (atan2) of elements of two input arrays, divided by π.
        /// r[i] = arctan(a[i]/b[i])/PI
        /// </summary>
        /// <param name="n">The number of elements in the input arrays.</param>
        /// <param name="a">Pointer to the first input array.</param>
        /// <param name="b">Pointer to the second input array.</param>
        /// <param name="y">Pointer to the output array where the results are stored.</param>
        /// <remarks>
        /// For each element i, computes y[i] = atan2(a[i], b[i]) / π.
        /// </remarks>
        void Atan2pi(long n, [In] double* a, [In] double* b,
            [Out] double* y);

        #endregion
        #region ---- Mul ----

        /// <summary>
        /// Performs element-wise multiplication of two double arrays.
        /// y[i] = a[i] * b[i]
        /// </summary>
        /// <param name="n">Number of elements in the arrays.</param>
        /// <param name="a">Pointer to the first input array.</param>
        /// <param name="b">Pointer to the second input array.</param>
        /// <param name="y">Pointer to the output array where the result is stored.</param>
        void Mul(long n, [In] double* a, [In] double* b,
            [Out] double* y);

        /// <summary>
        /// Performs element-wise multiplication of two complex arrays.
        /// y[i] = a[i] * b[i]
        /// </summary>
        /// <param name="n">Number of elements in the arrays.</param>
        /// <param name="a">Pointer to the first input array (complex values).</param>
        /// <param name="b">Pointer to the second input array (complex values).</param>
        /// <param name="y">Pointer to the output array where the result is stored (complex values).</param>
        void Mul(long n, [In] void* a, [In] void* b,
            [Out] void* y);

        
        // wrapper
        /// <summary>
        /// performs element by element multiplication of array a and array b
        /// </summary>
        /// <typeparam name="T"> ArrayBase[double] </typeparam>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="b"> input array b </param>
        /// <param name="y"> result array y </param>
        void MulD<T>(long n, T a, T b, ref T y)
            where T : DenseArrayBase<double>;

        /// <summary>
        /// performs element by element multiplication of array a and array b
        /// </summary>
        /// <typeparam name="T"> ArrayBase[Complex] </typeparam>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="b"> input array b </param>
        /// <param name="y"> result array y </param>
        void MulZ<T>(long n, T a, T b, ref T y)
            where T : DenseArrayBase<Complex>;

        #endregion
        #region ---- Div ----

        /// <summary>
        /// Performs element-wise division of two arrays of doubles.
        /// y[i] = a[i] / b[i]
        /// </summary>
        /// <param name="n">Number of elements in the arrays.</param>
        /// <param name="a">Pointer to the first input array (dividend).</param>
        /// <param name="b">Pointer to the second input array (divisor).</param>
        /// <param name="y">Pointer to the output array (result).</param>
        void Div(long n, [In] double* a, [In] double* b,
            [Out] double* y);

        /// <summary>
        /// Performs element-wise division of two arrays of complex numbers.
        /// y[i] = a[i] / b[i]
        /// </summary>
        /// <param name="n">Number of elements in the arrays.</param>
        /// <param name="a">Pointer to the first input array (dividend).</param>
        /// <param name="b">Pointer to the second input array (divisor).</param>
        /// <param name="y">Pointer to the output array (result).</param>
        void Div(long n, [In] void* a, [In] void* b,
            [Out] void* y);

        
        // wrapper
        /// <summary>
        /// performs element by element division of array a by array b
        /// </summary>
        /// <typeparam name="T"> ArrayBase[double] </typeparam>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="b"> input array b </param>
        /// <param name="y"> result array y </param>
        void DivD<T>(long n, T a, T b, ref T y)
            where T : DenseArrayBase<double>;

        /// <summary>
        /// performs element by element division of array a by array b
        /// </summary>
        /// <typeparam name="T"> ArrayBase[Complex] </typeparam>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="b"> input array b </param>
        /// <param name="y"> result array y </param>
        void DivZ<T>(long n, T a, T b, ref T y)
            where T : DenseArrayBase<Complex>;

        #endregion
        #region ---- Pow ----

        /// <summary>
        /// Computes the element-wise power of two arrays of double-precision floating-point numbers.
        /// Each element of the output array y is computed as y[i] = a[i] ^ b[i].
        /// </summary>
        /// <param name="n">The number of elements in the input arrays.</param>
        /// <param name="a">Pointer to the base array.</param>
        /// <param name="b">Pointer to the exponent array.</param>
        /// <param name="y">Pointer to the output array where the result is stored.</param>
        void Pow(long n, [In] double* a, [In] double* b,
            [Out] double* y);

        /// <summary>
        /// Computes the element-wise power of two arrays of complex numbers.
        /// Each element of the output array y is computed as y[i] = a[i] ^ b[i].
        /// </summary>
        /// <param name="n">The number of elements in the input arrays.</param>
        /// <param name="a">Pointer to the base array (complex values).</param>
        /// <param name="b">Pointer to the exponent array (complex values).</param>
        /// <param name="y">Pointer to the output array where the result is stored (complex values).</param>
        void Pow(long n, [In] void* a, [In] void* b,
            [Out] void* y);


        // wrapper
        /// <summary>
        /// computes a to the power b for elements of two arrays
        /// </summary>
        /// <typeparam name="T"> ArrayBase[double] </typeparam>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input arrays a </param>
        /// <param name="b"> input arrays b </param>
        /// <param name="y"> result arrays y </param>
        void PowD<T>(long n, T a, T b, ref T y)
            where T : DenseArrayBase<double>;

        /// <summary>
        /// computes a to the power b for elements of two arrays
        /// </summary>
        /// <typeparam name="T"> ArrayBase[Complex] </typeparam>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="b"> input array b </param>
        /// <param name="y"> result array y </param>
        void PowZ<T>(long n, T a, T b, ref T y)
            where T : DenseArrayBase<Complex>;

        #endregion
        #region ---- Pow3o2 ----

        /// <summary>
        /// Computes the square root of the cube of each element in the input array.
        /// That is, for each element a[i], computes y[i] = a[i]^(3/2).
        /// </summary>
        /// <param name="n">The number of elements in the input array.</param>
        /// <param name="a">Pointer to the input array of doubles.</param>
        /// <param name="y">Pointer to the output array of doubles, where the results are stored.</param>
        void Pow3o2(long n, [In] double* a, [Out] double* y);


        // wrapper
        /// <summary>
        /// computes the square root of the cube of each array element
        /// </summary>
        /// <typeparam name="T"> ArrayBase[double] </typeparam>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="y"> result array y </param>
        void Pow3o2D<T>(long n, T a, ref T y)
            where T : DenseArrayBase<double>;

        #endregion
        #region ---- Pow2o3 ----

        /// <summary>
        /// Computes the cube root of the square of each element in the input array.
        /// y[i] = a[i]^(2/3)
        /// </summary>
        /// <param name="n">The number of elements in the input array.</param>
        /// <param name="a">Pointer to the input array of doubles.</param>
        /// <param name="y">Pointer to the output array of doubles where the results will be stored.</param>
        void Pow2o3(long n, [In] double* a, [Out] double* y);


        // wrapper
        /// <summary>
        /// computes the cube root of the square of each array element
        /// </summary>
        /// <typeparam name="T"> ArrayBase[double] </typeparam>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="y"> result array y </param>
        void Pow2o3D<T>(long n, T a, ref T y)
            where T : DenseArrayBase<double>;

        #endregion
        #region ---- Powx ----

        /// <summary>
        /// Raises each element of the input array <paramref name="a"/> to the scalar power <paramref name="b"/> (double precision).
        /// y[i] = a[i]^b
        /// </summary>
        /// <param name="n">Number of elements in the input array.</param>
        /// <param name="a">Pointer to the input array.</param>
        /// <param name="b">Scalar exponent.</param>
        /// <param name="y">Pointer to the output array where the results are stored.</param>
        void Powx(long n, [In] double* a, double b, [Out] double* y);

        /// <summary>
        /// Raises each element of the input array <paramref name="a"/> to the scalar power <paramref name="b"/> (complex double precision).
        /// y[i] = a[i]^b
        /// </summary>
        /// <param name="n">Number of elements in the input array.</param>
        /// <param name="a">Pointer to the input array (complex values).</param>
        /// <param name="b">Pointer to the scalar exponent (complex value).</param>
        /// <param name="y">Pointer to the output array where the results are stored (complex values).</param>
        void Powx(long n, [In] void* a, void* b, [Out] void* y);


        // wrapper
        /// <summary>
        /// computes each element of array a to the scalar power b
        /// </summary>
        /// <typeparam name="T"> ArrayBase[double] </typeparam>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="b"> input scalar b </param>
        /// <param name="y"> result array y </param>
        void PowxD<T>(long n, T a, double b, ref T y)
            where T : DenseArrayBase<double>;

        /// <summary>
        /// computes each element of array a to the scalar power b
        /// </summary>
        /// <typeparam name="T"> ArrayBase[Complex] </typeparam>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="b"> input scalar b </param>
        /// <param name="y"> result array y </param>
        void PowxZ<T>(long n, T a, Complex b, ref T y)
            where T : DenseArrayBase<Complex>;

        #endregion
        #region ---- Powr ----

        /// <summary>
        /// Raises each element of the input array <paramref name="a"/> to the power of the corresponding element in array <paramref name="b"/>,
        /// storing the result in array <paramref name="y"/>.
        /// y[i] = a[i]^b[i]
        /// </summary>
        /// <param name="n">The number of elements in the arrays.</param>
        /// <param name="a">Pointer to the input array of base values.</param>
        /// <param name="b">Pointer to the input array of exponent values.</param>
        /// <param name="y">Pointer to the output array where the results are stored.</param>
        void Powr(long n, [In] double* a, double* b, [Out] double* y);

        #endregion
        #region ---- SinCos ----

        /// <summary>
        /// Computes the sine and cosine of each element in the input array <paramref name="a"/>.
        /// y[i] = sin(a[i]), z[i]=cos(a[i])
        /// </summary>
        /// <param name="n">The number of elements in the input array.</param>
        /// <param name="a">Pointer to the input array of double values (angles in radians).</param>
        /// <param name="sin">Pointer to the output array that will receive the sine values.</param>
        /// <param name="cos">Pointer to the output array that will receive the cosine values.</param>
        void SinCos(long n, [In] double* a,
            [Out] double* sin, [Out] double* cos);


        // wrapper
        /// <summary>
        /// computes sine and cosine of array elements
        /// </summary>
        /// <typeparam name="T"> ArrayBase[double] </typeparam>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="sin"> result array sin </param>
        /// <param name="cos"> result array cos </param>
        void SinCosD<T>(long n, T a, ref T sin, ref T cos)
            where T : DenseArrayBase<double>;

        #endregion
        #region ---- SinCospi ----

        /// <summary>
        /// Computes the sine and cosine of each element in the input array, where the input values are interpreted as multiples of π (pi).
        /// For each element a[i], computes sin(π * a[i]) and cos(π * a[i]).
        /// y[i] = sinpi(a[i]), z[i]=cospi(a[i])
        /// </summary>
        /// <param name="n">The number of elements in the input array.</param>
        /// <param name="a">Pointer to the input array of double values, each representing a multiple of π.</param>
        /// <param name="sin">Pointer to the output array that will receive the sine values.</param>
        /// <param name="cos">Pointer to the output array that will receive the cosine values.</param>
        void SinCospi(long n, [In] double* a,
            [Out] double* sin, [Out] double* cos);

        #endregion
        #region ---- LinearFrac ----

        /// <summary>
        /// Performs a linear fraction transformation of two double arrays with scalar parameters.
        /// Computes y[i] = (scalea * a[i] + shifta) / (scaleb * b[i] + shiftb) for each element.
        /// </summary>
        /// <param name="n">Number of elements in the arrays.</param>
        /// <param name="a">Pointer to the first input array.</param>
        /// <param name="b">Pointer to the second input array.</param>
        /// <param name="scalea">Scale factor for array a.</param>
        /// <param name="shifta">Shift value for array a.</param>
        /// <param name="scaleb">Scale factor for array b.</param>
        /// <param name="shiftb">Shift value for array b.</param>
        /// <param name="y">Pointer to the output array where the result is stored.</param>
        void LinearFrac(long n, [In] double* a, [In] double* b,
            double scalea, double shifta, double scaleb, double shiftb,
            [Out] double* y);


        // wrapper
        /// <summary>
        /// performs linear fraction transformation of 
        /// vectors a and b with scalar parameters
        /// y[i] = (scalea*a[i]+shifta) / (scaleb*b[i]+shiftb)
        /// </summary>
        /// <typeparam name="T"> ArrayBase[double] </typeparam>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="b"> input array b </param>
        /// <param name="scalea"> scale of a </param>
        /// <param name="shifta"> shift of a </param>
        /// <param name="scaleb"> scale of b </param>
        /// <param name="shiftb"> shift of b </param>
        /// <param name="y"> result array y </param>
        void LinearFracD<T>(long n, T a, T b,
            double scalea, double shifta, double scaleb, double shiftb,
            ref T y) where T : DenseArrayBase<double>;

        /// <summary>
        /// performs linear fraction transformation of 
        /// vectors a and b with scalar parameters
        /// real-part: y[i].Re = (scalea*a[i].Re + shifta.Re) / (scaleb*b[i].Re + shiftb.Re)
        /// imag-part: y[i].Im = (scalea*a[i].Im + shifta.Im) / (scaleb*b[i].Im + shiftb.Im)
        /// </summary>
        /// <typeparam name="T"> ArrayBase[Complex] </typeparam>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="b"> input array b </param>
        /// <param name="scalea"> scale of a </param>
        /// <param name="shifta"> shift of a </param>
        /// <param name="scaleb"> scale of b </param>
        /// <param name="shiftb"> shift of b </param>
        /// <param name="y"> result array y </param>
        void LinearFracZ<T>(long n, T a, T b,
            double scalea, Complex shifta, double scaleb, Complex shiftb,
            ref T y) where T : DenseArrayBase<Complex>;

        #endregion
        #region ---- Ceil ----

        /// <summary>
        /// Computes the ceiling of each element in the input array <paramref name="a"/> and stores the result in <paramref name="y"/>.
        /// y[i] = ceil(a[i])
        /// </summary>
        /// <param name="n">The number of elements in the input array.</param>
        /// <param name="a">Pointer to the input array of double values.</param>
        /// <param name="y">Pointer to the output array where the ceiling values will be stored.</param>
        void Ceil(long n, [In] double* a, [Out] double* y);

        #endregion
        #region ---- Floor ----

        /// <summary>
        /// Computes the floor of each element in the input array <paramref name="a"/> and stores the result in <paramref name="y"/>.
        /// y[i] = floor(a[i])
        /// </summary>
        /// <param name="n">The number of elements in the input and output arrays.</param>
        /// <param name="a">Pointer to the input array of double values.</param>
        /// <param name="y">Pointer to the output array where the floored values will be stored.</param>
        void Floor(long n, [In] double* a, [Out] double* y);

        #endregion
        #region ---- Frac ----

        /// <summary>
        /// Computes the fractional part of each element in the input array <paramref name="a"/>,
        /// storing the result in the output array <paramref name="y"/>.
        /// y[i] = a[i] - |a[i]|
        /// </summary>
        /// <param name="n">The number of elements to process.</param>
        /// <param name="a">Pointer to the input array of doubles.</param>
        /// <param name="y">Pointer to the output array of doubles, where the fractional parts will be stored.</param>
        void Frac(long n, [In] double* a, [Out] double* y);

        #endregion
        #region ---- Modf ----

        /// <summary>
        /// Splits each element of the input array <paramref name="a"/> into its fractional and integer parts.
        /// y[i] = |a[i]|, z[i] = a[i] - |a[i]|
        /// </summary>
        /// <param name="n">The number of elements in the input array.</param>
        /// <param name="a">Pointer to the input array of doubles.</param>
        /// <param name="y">Pointer to the output array that will receive the fractional parts.</param>
        /// <param name="i">Pointer to the output array that will receive the integer parts.</param>
        void Modf(long n, [In] double* a,
            [Out] double* y, [Out] double* i);

        #endregion
        #region ---- Fmod ----

        /// <summary>
        /// Computes the element-wise floating-point remainder of division operation
        /// for two input arrays a and b, storing the result in y.
        /// y[i] = fmod(a[i], b[i])
        /// </summary>
        /// <param name="n">Number of elements in the input arrays.</param>
        /// <param name="a">Pointer to the first input array.</param>
        /// <param name="b">Pointer to the second input array.</param>
        /// <param name="y">Pointer to the output array where the result is stored.</param>
        void Fmod(long n, [In] double* a, [In] double* b, [Out] double* y);

        #endregion
        #region ---- Remainder ----

        /// <summary>
        /// Computes the element-wise remainder of division of two double arrays.
        /// y[i] = remainder(a[i], b[i])
        /// </summary>
        /// <param name="n">The number of elements in the arrays.</param>
        /// <param name="a">Pointer to the first input array.</param>
        /// <param name="b">Pointer to the second input array.</param>
        /// <param name="y">Pointer to the output array where the result is stored.</param>
        void Remainder(long n, [In] double* a, [In] double* b, [Out] double* y);

        #endregion
        #region ---- NextAfter ----

        /// <summary>
        /// Computes the next representable double-precision floating-point value after each element of <paramref name="a"/> in the direction of the corresponding element in <paramref name="b"/>.
        /// y[i] = nextafter(a[i], b[i])
        /// </summary>
        /// <param name="n">The number of elements in the input arrays.</param>
        /// <param name="a">Pointer to the input array of double-precision values.</param>
        /// <param name="b">Pointer to the input array indicating the direction for each element in <paramref name="a"/>.</param>
        /// <param name="y">Pointer to the output array where the result is stored.</param>
        void NextAfter(long n, [In] double* a, [In] double* b, [Out] double* y);

        #endregion
        #region ---- CopySign ----

        /// <summary>
        /// Copies the sign of each element in array <paramref name="b"/> to the corresponding element in array <paramref name="a"/>,
        /// storing the result in array <paramref name="y"/>.
        /// y[i] = copysign(a[i], b[i])
        /// </summary>
        /// <param name="n">The number of elements to process.</param>
        /// <param name="a">Pointer to the input array whose magnitudes are used.</param>
        /// <param name="b">Pointer to the input array whose signs are used.</param>
        /// <param name="y">Pointer to the output array where the result is stored.</param>
        void CopySign(long n, [In] double* a, [In] double* b, [Out] double* y);

        #endregion
        #region ---- Fdim ----

        /// <summary>
        /// Computes the positive difference of two arrays element-wise.
        /// For each element i: y[i] = max(a[i] - b[i], 0)
        /// </summary>
        /// <param name="n">Number of elements in the arrays.</param>
        /// <param name="a">Pointer to the first input array.</param>
        /// <param name="b">Pointer to the second input array.</param>
        /// <param name="y">Pointer to the output array where the result is stored.</param>
        void Fdim(long n, [In] double* a, [In] double* b, [Out] double* y);

        #endregion
        #region ---- Fmax ----

        /// <summary>
        /// Computes the element-wise maximum of two double-precision arrays.
        /// y[i] = fmax(a[i], b[i])
        /// </summary>
        /// <param name="n">The number of elements in the input arrays.</param>
        /// <param name="a">Pointer to the first input array.</param>
        /// <param name="b">Pointer to the second input array.</param>
        /// <param name="y">Pointer to the output array where the result is stored.</param>
        void Fmax(long n, [In] double* a, [In] double* b, [Out] double* y);

        #endregion
        #region ---- Fmin ----

        /// <summary>
        /// Computes the element-wise minimum of two double-precision arrays.
        /// y[i] = fmin(a[i], b[i])
        /// </summary>
        /// <param name="n">The number of elements in the input arrays.</param>
        /// <param name="a">Pointer to the first input array.</param>
        /// <param name="b">Pointer to the second input array.</param>
        /// <param name="y">Pointer to the output array where the result is stored.</param>
        void Fmin(long n, [In] double* a, [In] double* b, [Out] double* y);

        #endregion
        #region ---- MaxMag ----

        /// <summary>
        /// Computes the element-wise maximum magnitude of two double arrays.
        /// y[i] = maxmag(a[i], b[i])
        /// </summary>
        /// <param name="n">The number of elements in the input arrays.</param>
        /// <param name="a">Pointer to the first input array.</param>
        /// <param name="b">Pointer to the second input array.</param>
        /// <param name="y">Pointer to the output array where the result is stored.</param>
        void MaxMag(long n, [In] double* a, [In] double* b, [Out] double* y);

        #endregion
        #region ---- MinMag ----

        /// <summary>
        /// Computes the element-wise minimum magnitude of two double arrays.
        /// For each element i, y[i] = min(|a[i]|, |b[i]|).
        /// </summary>
        /// <param name="n">Number of elements in the arrays.</param>
        /// <param name="a">Pointer to the first input array.</param>
        /// <param name="b">Pointer to the second input array.</param>
        /// <param name="y">Pointer to the output array where the result is stored.</param>
        void MinMag(long n, [In] double* a, [In] double* b, [Out] double* y);

        #endregion
        #region ---- Round ----

        /// <summary>
        /// Rounds each element of the input array <paramref name="a"/> to the nearest integer value
        /// and stores the result in the output array <paramref name="y"/>.
        /// y[i] = round(a[i])
        /// </summary>
        /// <param name="n">The number of elements to process.</param>
        /// <param name="a">Pointer to the input array of double values.</param>
        /// <param name="y">Pointer to the output array where rounded values will be stored.</param>
        void Round(long n, [In] double* a, [Out] double* y);

        #endregion
        #region ---- Conj ----

        /// <summary>
        /// Performs element-wise conjugation of a complex array.
        /// y[i] = conj(a[i])
        /// </summary>
        /// <param name="n">Number of elements in the array.</param>
        /// <param name="a">Pointer to the input array of complex numbers.</param>
        /// <param name="y">Pointer to the output array where the conjugated values will be stored.</param>
        void Conj(long n, [In] void* a, [Out] void* y);


        // wrapper
        /// <summary>
        /// performs element by element conjugation of the array
        /// </summary>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="y"> result array y </param>
        void ConjZ<T>(long n, T a, ref T y)
            where T : DenseArrayBase<Complex>;

        #endregion
        #region ---- MulByConj ----

        /// <summary>
        /// Performs element-wise multiplication of array <paramref name="a"/> and the conjugate of array <paramref name="b"/>.
        /// The result is stored in <paramref name="y"/>.
        /// y[i] = mulbyconj(a[i],b[i])
        /// </summary>
        /// <param name="n">Number of elements to process.</param>
        /// <param name="a">Pointer to the first input array.</param>
        /// <param name="b">Pointer to the second input array (to be conjugated).</param>
        /// <param name="y">Pointer to the output array where the result is stored.</param>
        void MulByConj(long n, [In] void* a, [In] void* b, [Out] void* y);


        // wrapper
        /// <summary>
        /// performs element by element multiplication of array a element 
        /// and conjugated array b element
        /// </summary>
        /// <typeparam name="T"> ArrayBase[double] </typeparam>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="b"> input array b </param>
        /// <param name="y"> result array y </param>
        void MulByConjZ<T>(long n, T a, T b, ref T y)
            where T : DenseArrayBase<Complex>;

        #endregion

        // ...
        #region ---- PackI ----

        /// <summary>
        /// Copies elements of an array with specified indexing to an array with unit increment (double precision).
        /// </summary>
        /// <param name="n">Number of elements to copy.</param>
        /// <param name="a">Pointer to the input array <c>a</c>.</param>
        /// <param name="y">Pointer to the output array <c>y</c> (unit increment).</param>
        /// <param name="inca">Increment for the elements of <c>a</c>. Default is 1.</param>
        void PackI(long n, [In] double* a, [Out] double* y,
            long inca = 1);

        /// <summary>
        /// Copies elements of a complex array with specified indexing to an array with unit increment.
        /// </summary>
        /// <param name="n">Number of elements to copy.</param>
        /// <param name="a">Pointer to the input complex array <c>a</c>.</param>
        /// <param name="y">Pointer to the output complex array <c>y</c> (unit increment).</param>
        /// <param name="inca">Increment for the elements of <c>a</c>. Default is 1.</param>
        void PackI(long n, [In] void* a, [Out] void* y,
            long inca = 1);

        // wrapper
        /// <summary>
        /// copies elements of an array with specified indexing 
        /// to an array with unit increment
        /// </summary>
        /// <typeparam name="T"> ArrayBase[double] </typeparam>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="y"> result array y </param>
        /// <param name="inca"> increment for the elements of a </param>
        void PackID<T>(long n, T a, ref T y,
            long inca = 1) where T : DenseArrayBase<double>;

        /// <summary>
        /// copies elements of an array with specified indexing 
        /// to an array with unit increment
        /// </summary>
        /// <typeparam name="T"> ArrayBase[Complex] </typeparam>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="inca"> increment for the elements of a </param>
        /// <param name="y"> result array y </param>
        void PackIZ<T>(long n, T a, ref T y,
            long inca = 1) where T : DenseArrayBase<Complex>;

        #endregion
        #region ---- UnpackI ----

        /// <summary>
        /// Copies elements of an array with unit increment to an array with specified indexing (double precision).
        /// </summary>
        /// <param name="n">Number of elements to copy.</param>
        /// <param name="a">Pointer to the input array <c>a</c> (double precision, unit increment).</param>
        /// <param name="y">Pointer to the output array <c>y</c> (double precision, with specified increment).</param>
        /// <param name="incy">Increment for the elements of <c>y</c>. Default is 1.</param>
        void UnpackI(long n, [In] double* a, [Out] double* y,
            long incy = 1);

        /// <summary>
        /// Copies elements of an array with unit increment to an array with specified indexing (complex double precision).
        /// </summary>
        /// <param name="n">Number of elements to copy.</param>
        /// <param name="a">Pointer to the input array <c>a</c> (complex double precision, unit increment).</param>
        /// <param name="y">Pointer to the output array <c>y</c> (complex double precision, with specified increment).</param>
        /// <param name="incy">Increment for the elements of <c>y</c>. Default is 1.</param>
        void UnpackI(long n, [In] void* a, [Out] void* y,
            long incy = 1);


        // wrapper
        /// <summary>
        /// Copies elements of an array with unit increment 
        /// to an array with specified indexing
        /// </summary>
        /// <typeparam name="T"> ArrayBase[double] </typeparam>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="y"> result array y </param>
        /// <param name="incy"> increment for the elements of y </param>
        void UnpackID<T>(long n, T a, ref T y,
            long incy = 1) where T : DenseArrayBase<double>;

        /// <summary>
        /// Copies elements of an array with unit increment 
        /// to an array with specified indexing
        /// </summary>
        /// <typeparam name="T"> ArrayBase[Complex] </typeparam>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="y"> result array y </param>
        /// <param name="incy"> increment for the elements of y </param>
        void UnpackIZ<T>(long n, T a, ref T y,
            long incy = 1) where T : DenseArrayBase<Complex>;

        #endregion
        
        #region ---- Part ----

        /// <summary>
        /// takes the real part of each complex array element
        /// </summary>
        /// <typeparam name="T1"> ArrayBase[Complex] </typeparam>
        /// <typeparam name="T2"> ArrayBase[double] </typeparam>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="y"> result array y </param>
        void RealPart<T1, T2>(long n, T1 a, ref T2 y)
            where T1 : DenseArrayBase<Complex>
            where T2 : DenseArrayBase<double>;

        /// <summary>
        /// takes the imaginary part of each complex array element
        /// </summary>
        /// <typeparam name="T1"> ArrayBase[Complex] </typeparam>
        /// <typeparam name="T2"> ArrayBase[double] </typeparam>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="y"> result array y </param>
        void ImagPart<T1, T2>(long n, T1 a, ref T2 y)
            where T1 : DenseArrayBase<Complex>
            where T2 : DenseArrayBase<double>;

        /// <summary>
        /// takes the real and imaginary parts of each complex array element
        /// </summary>
        /// <typeparam name="T1"> ArrayBase[Complex] </typeparam>
        /// <typeparam name="T2"> ArrayBase[double] </typeparam>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="re"> result real part array </param>
        /// <param name="im"> result imaginary part array </param>
        void RealImagParts<T1, T2>(long n, T1 a, ref T2 re, ref T2 im)
            where T1 : DenseArrayBase<Complex>
            where T2 : DenseArrayBase<double>;

        #endregion
        #region ---- Modify ----

        /// <summary>
        /// modifies the real and imaginary part of a complex array
        /// </summary>
        /// <typeparam name="T1"> ArrayBase[double] </typeparam>
        /// <typeparam name="T2"> ArrayBase[Complex] </typeparam>
        /// <param name="n"> number of elements </param>
        /// <param name="re"> real part array </param>
        /// <param name="im"> imaginary part array </param>
        /// <param name="y"> result array (input and modified) </param>
        void Modify<T1, T2>(long n, T1 re, T1 im, ref T2 y)
            where T1 : DenseArrayBase<double>
            where T2 : DenseArrayBase<Complex>;

        /// <summary>
        /// modifies the real part of a complex array
        /// </summary>
        /// <typeparam name="T1"> ArrayBase[double] </typeparam>
        /// <typeparam name="T2"> ArrayBase[Complex] </typeparam>
        /// <param name="n"> number of elements </param>
        /// <param name="re"> real part </param>
        /// <param name="y"> input and modified array result </param>
        void ModifyReal<T1, T2>(long n, T1 re, ref T2 y)
            where T1 : DenseArrayBase<double>
            where T2 : DenseArrayBase<Complex>;

        /// <summary>
        /// modifies the imaginary part of a complex array
        /// </summary>
        /// <typeparam name="T1"> ArrayBase[double] </typeparam>
        /// <typeparam name="T2"> ArrayBase[Complex] </typeparam>
        /// <param name="n"> number of elements </param>
        /// <param name="im"> imaginary part </param>
        /// <param name="y"> input and modified array result </param>
        void ModifyImag<T1, T2>(long n, T1 im, ref T2 y)
            where T1 : DenseArrayBase<double>
            where T2 : DenseArrayBase<Complex>;


        #endregion

    }
}
