using System.Numerics;
using System.Runtime.InteropServices;
using Int = System.Int64;
using Real = System.Double;

namespace VEMS.MathCore
{

    /// <summary>
    /// collection of math functions
    /// </summary>
    internal class NMath
    {
        #region ---- Axpy ----

        /// <summary>
        /// Computes a scaled vector addition (AXPY operation) for generic numeric types.
        /// Performs the operation: y[i * incy] += a * x[i * incx] for each element.
        /// If <paramref name="a"/> is the default value and both <paramref name="incx"/> and <paramref name="incy"/> are 1,
        /// performs simple element-wise addition: y[i] += x[i].
        /// </summary>
        /// <typeparam name="T">A numeric type implementing <see cref="INumber{T}"/>.</typeparam>
        /// <param name="n">Number of elements to process.</param>
        /// <param name="x">Pointer to the input array x.</param>
        /// <param name="y">Pointer to the input/output array y.</param>
        /// <param name="a">Scalar multiplier (default is the default value for <typeparamref name="T"/>).</param>
        /// <param name="incx">Increment for indexing x (default is 1).</param>
        /// <param name="incy">Increment for indexing y (default is 1).</param>
        /// <param name="loopMode">Loop-computational mode option (default is <see cref="Defaults.LoopOption"/>).</param>
        internal static unsafe void Axpy<T>(long n, [In] T* x,
            [In, Out] T* y,
            T a = default!, long incx = 1, long incy = 1,
            LoopMode loopMode = Defaults.LoopOption)
            where T : INumber<T>
        {
            void op(long i)
            {
                if (a == default && incx == 1 && incy == 1)
                { y[i] += x[i]; }
                else if (a == default)
                { y[i * incy] += x[i * incx]; }
                else
                { y[i * incy] += a * x[i * incx]; }
            }
            Loop1D loop = new(operation: op, start: 0, end: n, step: 1);
            loop.Evaluate(mode: loopMode);
        }

        #endregion
        #region ---- Axps ----

        /// <summary>
        /// Computes a scaled vector addition and shift (AXPS operation) for generic numeric types.
        /// Performs the operation: x[i * incx] = a * x[i * incx] + s for each element.
        /// If <paramref name="a"/> is the default value and <paramref name="incx"/> is 1,
        /// performs simple element-wise addition: x[i] += s.
        /// If <paramref name="a"/> is the default value and <paramref name="incx"/> is not 1,
        /// performs: x[i * incx] += s.
        /// Otherwise, performs: x[i * incx] *= a; x[i * incx] += s.
        /// </summary>
        /// <typeparam name="T">A numeric type implementing <see cref="INumber{T}"/>.</typeparam>
        /// <param name="n">Number of elements to process.</param>
        /// <param name="x">Pointer to the input/output array x.</param>
        /// <param name="s">Scalar value to add to each element.</param>
        /// <param name="a">Scalar multiplier (default is the default value for <typeparamref name="T"/>).</param>
        /// <param name="incx">Increment for indexing x (default is 1).</param>
        /// <param name="loopMode">Loop-computational mode option (default is <see cref="Defaults.LoopOption"/>).</param>
        internal static unsafe void Axps<T>(long n, [In, Out] T* x, T s,
            T a = default!, long incx = 1,
            LoopMode loopMode = Defaults.LoopOption)
            where T : INumber<T>
        {
            void op(long i)
            {
                if (a == default && incx == 1)
                { x[i] += s; }
                else if (a == default)
                { x[i * incx] += s; }
                else
                { x[i * incx] *= a; x[i * incx] += s; }
            }
            Loop1D loop = new(operation: op, start: 0, end: n, step: 1);
            loop.Evaluate(mode: loopMode);
        }

        #endregion
        #region ---- Sum ----

        /// <summary>
        /// Computes the sum of elements in a numeric array.
        /// Performs the operation: sum = x[0] + x[1] + x[2] + ... + x[(n-1)].
        /// </summary>
        /// <typeparam name="T">A numeric type implementing <see cref="INumber{T}"/>.</typeparam>
        /// <param name="n">Number of elements to process.</param>
        /// <param name="x">Pointer to the input array.</param>
        /// <param name="incx">Increment for indexing <paramref name="x"/> (default is 1).</param>
        /// <param name="loopMode">Loop-computational mode option (default is <see cref="Defaults.LoopOption"/>).</param>
        /// <returns>The sum of the elements in the array.</returns>
        internal static unsafe T Sum<T>(long n, [In] T* x,
            long incx = 1, LoopMode loopMode = Defaults.LoopOption)
            where T : INumber<T>
        {
            T sum = default!;
            void op(long i)
            {
                if (incx == 1)
                { sum += x[i]; }
                else
                { sum += x[i * incx]; }
            }
            Loop1D loop = new(operation: op, start: 0, end: n, step: 1);
            loop.Evaluate(mode: loopMode);
            return sum;
        }

        #endregion
        #region ---- IMax ----

        /// <summary>
        /// Finds the index of the element with the largest value in a numeric array.
        /// Performs the operation: returns the index <c>iMax</c> such that <c>x[iMax]</c> is the maximum value among <c>x[0]</c> to <c>x[n-1]</c>.
        /// Supports custom increment for indexing and loop-computational mode.
        /// </summary>
        /// <typeparam name="T">A numeric type implementing <see cref="INumber{T}"/>.</typeparam>
        /// <param name="n">Number of elements to process.</param>
        /// <param name="x">Pointer to the input array of type <typeparamref name="T"/>.</param>
        /// <param name="incx">Increment for indexing <paramref name="x"/> (default is 1).</param>
        /// <param name="loopMode">Loop-computational mode option (default is <see cref="Defaults.LoopOption"/>).</param>
        /// <returns>The index of the element with the largest value.</returns>
        internal static unsafe long IMax<T>(long n, [In] T* x,
            long incx = 1, LoopMode loopMode = Defaults.LoopOption)
            where T : INumber<T>
        {
            long idx = 0;
            void op(long i)
            {
                if (incx == 1)
                { if (x[i] > x[idx]) { idx = i; } }
                else
                { if (x[i * incx] > x[idx * incx]) { idx = i; } }
            }
            Loop1D loop = new(operation: op, start: 0, end: n, step: 1);
            loop.Evaluate(mode: loopMode);
            return idx;
        }

        #endregion
        #region ---- IMin ----

        /// <summary>
        /// Finds the index of the element with the smallest value in a numeric array.
        /// Performs the operation: returns the index <c>idx</c> such that <c>x[idx]</c> is the minimum value among <c>x[0]</c> to <c>x[n-1]</c>.
        /// Supports custom increment for indexing and loop-computational mode.
        /// </summary>
        /// <typeparam name="T">A numeric type implementing <see cref="INumber{T}"/>.</typeparam>
        /// <param name="n">Number of elements to process.</param>
        /// <param name="x">Pointer to the input array of type <typeparamref name="T"/>.</param>
        /// <param name="incx">Increment for indexing <paramref name="x"/> (default is 1).</param>
        /// <param name="loopMode">Loop-computational mode option (default is <see cref="Defaults.LoopOption"/>).</param>
        /// <returns>The index of the element with the smallest value.</returns>
        internal static unsafe long IMin<T>(long n, [In] T* x,
            long incx = 1, LoopMode loopMode = Defaults.LoopOption)
            where T : INumber<T>
        {
            long idx = 0;
            void op(long i)
            {
                if (incx == 1)
                { if (x[i] < x[idx]) { idx = i; } }
                else
                { if (x[i * incx] < x[idx * incx]) { idx = i; } }
            }
            Loop1D loop = new(operation: op, start: 0, end: n, step: 1);
            loop.Evaluate(mode: loopMode);
            return idx;
        }

        #endregion
        #region ---- Copy ----

        /// <summary>
        /// Copies elements from the source array <paramref name="x"/> to the destination array <paramref name="y"/>.
        /// Performs the operation: y[i * incy] = x[i * incx] for each element.
        /// If both <paramref name="incx"/> and <paramref name="incy"/> are 1, performs simple element-wise copy: y[i] = x[i].
        /// </summary>
        /// <typeparam name="T">A numeric type implementing <see cref="INumber{T}"/>.</typeparam>
        /// <param name="n">Number of elements to copy.</param>
        /// <param name="x">Pointer to the source array.</param>
        /// <param name="y">Pointer to the destination array.</param>
        /// <param name="incx">Increment for the elements of <paramref name="x"/>. Default is 1.</param>
        /// <param name="incy">Increment for the elements of <paramref name="y"/>. Default is 1.</param>
        /// <param name="loopMode">Loop-computational mode option (default is <see cref="Defaults.LoopOption"/>).</param>
        internal static unsafe void Copy<T>(long n, [In] T* x,
            [Out] T* y,
            long incx = 1, long incy = 1,
            LoopMode loopMode = Defaults.LoopOption)
            where T : INumber<T>
        {
            void op(long i)
            {
                if (incx == 1 && incy == 1)
                { y[i] = x[i]; }
                else
                { y[i * incy] = x[i * incx]; }
            }
            Loop1D loop = new(operation: op, start: 0, end: n, step: 1);
            loop.Evaluate(mode: loopMode);
        }

        #endregion

        // sorting
        #region ---- Sort ----

        /// <summary>
        /// Sorts the values in an array of generic numeric type in ascending order using an in-place quicksort algorithm.
        /// </summary>
        /// <typeparam name="T">A numeric type implementing <see cref="INumber{T}"/>.</typeparam>
        /// <param name="x">Pointer to the input array of type <typeparamref name="T"/>.</param>
        /// <param name="low">Lowest index of the subarray to sort.</param>
        /// <param name="high">Highest index of the subarray to sort.</param>
        /// <remarks>
        /// This method recursively partitions the array and sorts each partition.
        /// The sorting is performed in-place and does not allocate additional memory.
        /// If <paramref name="low"/> is greater than <paramref name="high"/>, the method returns immediately.
        /// </remarks>
        internal static unsafe void Sort<T>([In, Out] T* x,
            long low, long high)
            where T : INumber<T>
        {
            if (low > high) { return; }

            long i = low;
            long j = high;
            T t = x[i];

            while (i < j)
            {
                while (i < j && t <= x[j]) { j--; }
                x[i] = x[j];
                while (i < j && t >= x[i]) { i++; }
                x[j] = x[i];
            }
            x[i] = t;

            Sort(x, low, i - 1);
            Sort(x, i + 1, high);
        }

        #endregion
        #region ---- BinarySerach ----

        /// <summary>
        /// Performs a binary search to find the span (interval) in a sorted array of generic numeric type
        /// where the specified value <paramref name="xk"/> would be located.
        /// The array must be sorted in ascending order.
        /// </summary>
        /// <typeparam name="T">A numeric type implementing <see cref="INumber{T}"/>.</typeparam>
        /// <param name="n">Number of elements in the array.</param>
        /// <param name="x">Pointer to the input array of type <typeparamref name="T"/>.</param>
        /// <param name="xk">The target value to search for.</param>
        /// <param name="k">
        /// When the method returns, contains the index of the span in which <paramref name="xk"/> is found or would be inserted.
        /// If <paramref name="xk"/> is less than the first element or greater than the last element, <paramref name="k"/> is set to <c>n - 1</c>.
        /// </param>
        /// <returns>
        /// <c>true</c> if <paramref name="xk"/> is within the range of <paramref name="x"/>; otherwise, <c>false</c>.
        /// If <paramref name="xk"/> is equal to the last element, <paramref name="k"/> is set to the last index and <c>true</c> is returned.
        /// </returns>
        internal static unsafe bool BinarySearch<T>(long n, [In] T* x,
            T xk, out long k) where T : INumber<T>
        {
            k = n - 1;

            if (n == 0 || xk < x[0] || xk > x[k]) { return false; }
            if (xk == x[k]) { return true; }

            long iBegin = 0;
            long iEnd = k;
            // Standard binary search for the span
            while (iEnd - iBegin > 1)
            {
                long iMid = iBegin + ((iEnd - iBegin) >> 1); // (iEnd - iBegin) >> 1 => (iEnd - iBegin) / 2
                if (xk < x[iMid])
                { iEnd = iMid; }
                else
                { iBegin = iMid; }
            }
            k = iBegin;
            return true;
        }

        #endregion

        // conversion
        #region ---- Convert To ----

        /// <summary>
        /// Converts a pointer-based array of generic numeric type to a managed array of nullable elements.
        /// </summary>
        /// <typeparam name="T">A numeric type implementing <see cref="INumber{T}"/>.</typeparam>
        /// <param name="n">Number of elements to convert.</param>
        /// <param name="x">Pointer to the input array.</param>
        /// <param name="revOrder">If true, reverses the order of elements in the output array.</param>
        /// <param name="loopMode">Loop-computational mode option (default is <see cref="Defaults.LoopOption"/>).</param>
        /// <returns>
        /// A managed array of type <typeparamref name="T"/>? containing the converted elements.
        /// If <paramref name="revOrder"/> is true, the elements are in reverse order.
        /// </returns>
        internal static unsafe T?[] CnvtTo<T>(long n, [In] T* x,
            bool revOrder = false,
            LoopMode loopMode = Defaults.LoopOption)
            where T : INumber<T>
        {
            if (n > int.MaxValue)
            { throw new InvalidCastException("The number of vector elements exceeds the limit"); }

            // initialization
            T?[] y = new T?[n];

            // defines loop operation
            void op(long i)
            {
                long j = revOrder ? n - 1 - i : i;
                y[i] = x[j];
            }
            Loop1D loop = new(operation: op, start: 0, end: n, step: 1);
            loop.Evaluate(mode: loopMode);

            // return
            return y;
        }

        /// <summary>
        /// Converts a pointer-based array of generic numeric type to a managed 2D array of nullable elements.
        /// </summary>
        /// <typeparam name="T">A numeric type implementing <see cref="INumber{T}"/>.</typeparam>
        /// <param name="rows">Number of rows in the matrix.</param>
        /// <param name="cols">Number of columns in the matrix.</param>
        /// <param name="x">Pointer to the input array.</param>
        /// <param name="revRows">If true, reverses the order of rows in the output array.</param>
        /// <param name="revCols">If true, reverses the order of columns in the output array.</param>
        /// <param name="loopMode">Loop-computational mode option (default is <see cref="Defaults.LoopOption"/>).</param>
        /// <returns>
        /// A managed 2D array of type <typeparamref name="T"/>? containing the converted elements.
        /// If <paramref name="revRows"/> or <paramref name="revCols"/> is true, the corresponding dimension is reversed.
        /// </returns>
        internal static unsafe T?[,] CnvtTo<T>(long rows, long cols, [In] T* x,
            bool revRows = false, bool revCols = false,
            LoopMode loopMode = Defaults.LoopOption)
            where T : INumber<T>
        {
            if (rows * cols > int.MaxValue)
            { throw new InvalidCastException("The number of matrix elements exceeds the limit"); }

            // initialization
            T?[,] y = new T?[rows, cols];

            // defines loop operation
            void op(long iRow, long iCol)
            {
                long jRow = revRows ? rows - 1 - iRow : iRow;
                long jCol = revCols ? cols - 1 - iCol : iCol;
                long jdx = jRow * cols + jCol;
                y[iRow, iCol] = x[jdx];
            }
            Loop2D loop = new(operation: op,
                rowStart: 0, rowEnd: rows, rowStep: 1,
                colStart: 0, colEnd: cols, colStep: 1);
            loop.Evaluate(mode: loopMode);

            // return
            return y;
        }

        #endregion
        #region ---- Convert From ----

        /// <summary>
        /// Converts a managed array of nullable elements to a pointer-based array of generic numeric type.
        /// </summary>
        /// <typeparam name="T">A numeric type implementing <see cref="INumber{T}"/>.</typeparam>
        /// <param name="n">Number of elements to convert.</param>
        /// <param name="x">Input managed array of nullable elements.</param>
        /// <param name="y">Pointer to the output array.</param>
        /// <param name="revOrder">If true, reverses the order of elements during conversion.</param>
        /// <param name="loopMode">Loop-computational mode option (default is <see cref="Defaults.LoopOption"/>).</param>
        /// <exception cref="InvalidCastException">Thrown if <paramref name="n"/> exceeds <see cref="int.MaxValue"/>.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="x"/>.Length is less than <paramref name="n"/>.</exception>
        internal static unsafe void CnvtFrom<T>(long n, [In] T?[] x, [Out] T* y,
            bool revOrder = false, LoopMode loopMode = Defaults.LoopOption)
            where T : INumber<T>
        {
            if (n > int.MaxValue)
            { throw new InvalidCastException("The number of vector elements exceeds the limit"); }
            if (x.Length < n)
            { throw new ArgumentException("The length of input array is less than n"); }

            // defines loop operation
            void op(long i)
            {
                long j = revOrder ? n - 1 - i : i;
                y[i] = x[j]; // ?? default!;
            }
            Loop1D loop = new(operation: op, start: 0, end: n, step: 1);
            loop.Evaluate(mode: loopMode);
        }

        /// <summary>
        /// Converts a managed 2D array of nullable elements to a pointer-based array of generic numeric type.
        /// </summary>
        /// <typeparam name="T">A numeric type implementing <see cref="INumber{T}"/>.</typeparam>
        /// <param name="rows">Number of rows in the matrix.</param>
        /// <param name="cols">Number of columns in the matrix.</param>
        /// <param name="x">Input managed 2D array of nullable elements.</param>
        /// <param name="y">Pointer to the output array.</param>
        /// <param name="revRows">If true, reverses the order of rows during conversion.</param>
        /// <param name="revCols">If true, reverses the order of columns during conversion.</param>
        /// <param name="loopMode">Loop-computational mode option (default is <see cref="Defaults.LoopOption"/>).</param>
        /// <exception cref="InvalidCastException">Thrown if <paramref name="rows"/> * <paramref name="cols"/> exceeds <see cref="int.MaxValue"/>.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="x"/>.Length is less than <paramref name="rows"/> * <paramref name="cols"/>.</exception>
        internal static unsafe void CnvtFrom<T>(long rows, long cols,
            [In] T?[,] x, [Out] T* y,
            bool revRows = false, bool revCols = false,
            LoopMode loopMode = Defaults.LoopOption)
            where T : INumber<T>
        {
            if (rows * cols > int.MaxValue)
            { throw new InvalidCastException("The number of matrix elements exceeds the limit"); }
            if (x.Length < rows * cols)
            { throw new ArgumentException("The length of input array is less than rows * cols"); }

            // defines loop operation
            void op(long iRow, long iCol)
            {
                long jRow = revRows ? rows - 1 - iRow : iRow;
                long jCol = revCols ? cols - 1 - iCol : iCol;
                long jdx = jRow * cols + jCol;
                y[jdx] = x[iRow, iCol]; // ?? default!;
            }
            Loop2D loop = new(operation: op,
                rowStart: 0, rowEnd: rows, rowStep: 1,
                colStart: 0, colEnd: cols, colStep: 1);
            loop.Evaluate(mode: loopMode);
        }

        #endregion

        // phase
        #region ---- Unwrap [D] ----

        /// <summary>
        /// Unwraps a phase array in-place so that the phase difference between consecutive elements
        /// does not exceed ±2π. The unwrapping is performed forward from <paramref name="startIdx"/>+1 to <paramref name="n"/>-1,
        /// and backward from <paramref name="startIdx"/>-1 to 0. The input array <paramref name="x"/> is modified in-place.
        /// </summary>
        /// <param name="n">Number of elements in the phase array.</param>
        /// <param name="x">Pointer to the input array of phase values (in radians).</param>
        /// <param name="startIdx">Index from which to start unwrapping (default is 0).</param>
        /// <remarks>
        /// This method ensures that the phase difference between consecutive elements is within [-π, π].
        /// It is useful for processing phase data that may have discontinuities due to wrapping at ±π.
        /// </remarks>
        internal static unsafe void Unwrap2PI(long n, Real* x, long startIdx = 0)
        {
            // Unwrap forward from startIdx+1 to n-1
            if (startIdx < n - 1)
            {
                long iPlus = 0;
                Real prev = x[startIdx];
                for (long i = startIdx + 1; i < n; i++)
                {
                    Real curr = x[i];
                    Real diff = curr - prev;
                    if (diff < -Math.PI) iPlus++;
                    else if (diff > Math.PI) iPlus--;
                    x[i] = curr + iPlus * 2.0 * Math.PI;
                    prev = curr;
                }
            }
            // Unwrap backward from startIdx-1 to 0
            if (startIdx > 0)
            {
                long iMinus = 0;
                Real prev = x[startIdx];
                for (long i = startIdx - 1; i >= 0; i--)
                {
                    Real curr = x[i];
                    Real diff = curr - prev;
                    if (diff < -Math.PI) iMinus++;
                    else if (diff > Math.PI) iMinus--;
                    x[i] = curr + iMinus * 2.0 * Math.PI;
                    prev = curr;
                }
            }
        }

        #endregion

    }


    /// <summary>
    /// collection of some naive math functions
    /// </summary>
    [Obsolete]
    internal class NaiveMath 
    {
        // obsolete ...
        #region ---- helpers ----

        private static void ExArrayLengthD<T>(T x, T y) where T : DenseArrayBase<double>
        {
            if (x.Count != y.Count)
            { throw new ArgumentException($"Array lengths not equal"); }
        }

        private static void ExArrayLengthZ<T>(T x, T y) where T : DenseArrayBase<Complex>
        {
            if (x.Count != y.Count)
            { throw new ArgumentException($"Array lengths not equal"); }
        }

        #endregion
        #region ---- AddTo [D/Z] ----

        // obsolete ...
        /// <summary>
        /// adds the scalar s into each element of y
        /// yi := s + yi
        /// </summary>
        /// <typeparam name="T"> ArrayBase[double] </typeparam>
        /// <param name="s"> scalar s </param>
        /// <param name="y"> array y </param>
        /// <param name="loopMode"> loop-computational mode </param>
        internal static unsafe void AddToD<T>(double s, ref T y,
            LoopMode loopMode = Defaults.LoopOption)
            where T : DenseArrayBase<double>
        {
            double* py = (double*)y.DataPtr.ToPointer();

            Action<long> a = (i) => { *(py + i) += s; };
            Loop1D loop = new(operation: a,
                start: 0, end: y.Count, step: 1);
            loop.Evaluate(mode: loopMode);
        }

        /// <summary>
        /// adds the scalar s into each element of y
        /// yi := s + yi
        /// </summary>
        /// <typeparam name="T"> ArrayBase[Complex] </typeparam>
        /// <param name="s"> scalar s </param>
        /// <param name="y"> array y </param>
        /// <param name="loopMode"> loop-computational mode </param>
        internal static unsafe void AddToZ<T>(Complex s, ref T y,
            LoopMode loopMode = Defaults.LoopOption)
            where T : DenseArrayBase<Complex>
        {
            Complex* py = (Complex*)y.DataPtr.ToPointer();

            Action<long> a = (i) => { *(py + i) += s; };
            Loop1D loop = new(operation: a,
                start: 0, end: y.Count, step: 1);
            loop.Evaluate(mode: loopMode);
        }


        /// <summary>
        /// adds the elements of x to y
        /// yi := xi + yi
        /// </summary>
        /// <typeparam name="T"> ArrayBase[double] </typeparam>
        /// <param name="x"> array x </param>
        /// <param name="y"> array y </param>
        /// <param name="loopMode"> loop-computational mode </param>
        internal static unsafe void AddToD<T>(T x, ref T y, 
            LoopMode loopMode = Defaults.LoopOption)
            where T : DenseArrayBase<double>
        {
            ExArrayLengthD(x, y);

            double* px = (double*)x.DataPtr.ToPointer();
            double* py = (double*)y.DataPtr.ToPointer();

            Action<long> a = (i) => { *(py + i) += *(px + i); };
            Loop1D loop = new(operation: a,
                start: 0, end: x.Count, step: 1);
            loop.Evaluate(mode: loopMode);
        }

        /// <summary>
        /// adds the elements of x to y
        /// yi := xi + yi
        /// </summary>
        /// <typeparam name="T"> ArrayBase[Complex] </typeparam>
        /// <param name="x"> array x </param>
        /// <param name="y"> array y </param>
        /// <param name="loopMode"> loop-computational mode </param>
        internal static unsafe void AddToZ<T>(T x, ref T y,
            LoopMode loopMode = Defaults.LoopOption)
            where T : DenseArrayBase<Complex>
        {
            ExArrayLengthZ(x, y);

            Complex* px = (Complex*)x.DataPtr.ToPointer();
            Complex* py = (Complex*)y.DataPtr.ToPointer();

            Action<long> a = (i) => { *(py + i) += *(px + i); };
            Loop1D loop = new(operation: a,
                start: 0, end: x.Count, step: 1);
            loop.Evaluate(mode: loopMode);
        }

        #endregion
        #region ---- Sum [D/Z] ----

        /// <summary>
        /// sums up all the elements in x
        /// </summary>
        /// <typeparam name="T"> ArrayBase[double] </typeparam>
        /// <param name="x"> input array x </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> summation result </returns>
        internal static unsafe double SumD<T>(T x, 
            LoopMode loopMode = Defaults.LoopOption)
            where T : DenseArrayBase<double>
        {
            double sum = 0.0;
            double* px = (double*)x.DataPtr.ToPointer();

            // defines loop operation
            Action<long> a = (i) => { sum += *(px + i); };
            Loop1D loop = new(operation: a,
                start: 0, end: x.Count, step: 1);
            loop.Evaluate(mode: loopMode);

            // return
            return sum;
        }

        /// <summary>
        /// sums up all the elements in x
        /// </summary>
        /// <typeparam name="T"> ArrayBase[Complex] </typeparam>
        /// <param name="x"> input array x </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> summation result </returns>
        internal static unsafe Complex SumZ<T>(T x, 
            LoopMode loopMode = Defaults.LoopOption)
            where T : DenseArrayBase<Complex>
        {
            Complex sum = 0.0;
            Complex* px = (Complex*)x.DataPtr.ToPointer();

            // defines loop operation
            Action<long> a = (i) => { sum += *(px + i); };
            Loop1D loop = new(operation: a,
                start: 0, end: x.Count, step: 1);
            loop.Evaluate(mode: loopMode);

            // return
            return sum;
        }

        #endregion
        #region ---- Max [D] ----

        /// <summary>
        /// finds the index of the element with the largest value
        /// </summary>
        /// <typeparam name="T"> ArrayBase[double] </typeparam>
        /// <param name="x"> input array x </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> index of the element with largest value </returns>
        internal static unsafe long IMax<T>(T x, 
            LoopMode loopMode = Defaults.LoopOption)
            where T : DenseArrayBase<double>
        {
            long iMax = 0;
            double* p = (double*)x.DataPtr.ToPointer();

            // defines loop operation
            Action<long> a = (i) =>
            { if (*(p + i) > *(p + iMax)) { iMax = i; } };
            Loop1D loop = new(operation: a,
                start: 0, end: x.Count, step: 1);
            loop.Evaluate(mode: loopMode);

            // return
            return iMax;
        }

        /// <summary>
        /// finds the index of the element with the largest value
        /// </summary>
        /// <param name="x"> input array x </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> (index, largest value) </returns>
        internal static unsafe (long, double) MaxD<T>(T x,
            LoopMode loopMode = Defaults.LoopOption)
            where T : DenseArrayBase<double>
        {
            double* p = (double*)x.DataPtr.ToPointer();
            long iMax = IMax(x, loopMode);
            return (iMax, *(p + iMax));
        }

        #endregion
        #region ---- Min [D] ----

        /// <summary>
        /// finds the index of the element with the largest value
        /// </summary>
        /// <typeparam name="T"> ArrayBase[double] </typeparam>
        /// <param name="x"> input array x </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> index of the element with largest value </returns>
        internal static unsafe long IMin<T>(T x,
            LoopMode loopMode = Defaults.LoopOption)
            where T : DenseArrayBase<double>
        {
            long iMin = 0;
            double* p = (double*)x.DataPtr.ToPointer();

            // defines loop operation
            Action<long> a = (i) =>
            { if (*(p + i) < *(p + iMin)) { iMin = i; } };
            Loop1D loop = new(operation: a,
                start: 0, end: x.Count, step: 1);
            loop.Evaluate(mode: loopMode);

            // return
            return iMin;
        }

        /// <summary>
        /// finds the index of the element with the smallest value
        /// </summary>
        /// <param name="x"> input array x </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> (index, smallest value) </returns>
        internal static unsafe (long, double) MinD<T>(T x,
            LoopMode loopMode = Defaults.LoopOption)
            where T : DenseArrayBase<double>
        {
            double* p = (double*)x.DataPtr.ToPointer();
            long iMin = IMin(x, loopMode);
            return (iMin, *(p + iMin));
        }

        #endregion
        #region ---- MinMax [D] ----

        /// <summary>
        /// finds the index of the element with the largest value
        /// </summary>
        /// <typeparam name="T"> ArrayBase[double] </typeparam>
        /// <param name="x"> input array x </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> index of the element with largest value </returns>
        internal static unsafe (long, long) IMinMax<T>(T x,
            LoopMode loopMode = Defaults.LoopOption)
            where T : DenseArrayBase<double>
        {
            long iMin = 0; 
            long iMax = 0;
            double* p = (double*)x.DataPtr.ToPointer();

            // defines loop operation
            Action<long> a = (i) =>
            {
                if (*(p + i) < *(p + iMin)) { iMin = i; }
                if (*(p + i) > *(p + iMax)) { iMax = i; }
            };
            Loop1D loop = new(operation: a,
                start: 0, end: x.Count, step: 1);
            loop.Evaluate(mode: loopMode);

            // return
            return (iMin, iMax);
        }

        /// <summary>
        /// finds the indices of the elements with 
        /// the largest and smallest values
        /// </summary>
        /// <param name="x"> input array x </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> (indexMin, min value, indexMax, max value) </returns>
        internal static unsafe (long, double, long, double) MinMaxD<T>(T x,
            LoopMode loopMode = Defaults.LoopOption)
            where T : DenseArrayBase<double>
        {
            double* p = (double*)x.DataPtr.ToPointer();
            (long iMin, long iMax) = IMinMax(x, loopMode);
            return (iMin, *(p + iMin), iMax, *(p + iMax));
        }


        /// <summary>
        /// finds the index of the element with the largest value
        /// </summary>
        /// <param name="n"> number of array elements </param>
        /// <param name="x"> input array x </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> index of the element with largest value </returns>
        internal static unsafe (long, long) indexMinMax(long n, double* x,
            LoopMode loopMode = Defaults.LoopOption)
        {
            long iMin = 0;
            long iMax = 0;

            // defines loop operation
            Action<long> a = (i) =>
            {
                if (*(x + i) < *(x + iMin)) { iMin = i; }
                if (*(x + i) > *(x + iMax)) { iMax = i; }
            };
            Loop1D loop = new(operation: a,
                start: 0, end: n, step: 1);
            loop.Evaluate(mode: loopMode);

            // return
            return (iMin, iMax);
        }

        /// <summary>
        /// finds the indices of the elements with 
        /// the largest and smallest values
        /// </summary>
        /// <param name="x"> input array x </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> (indexMin, min value, indexMax, max value) </returns>
        internal static unsafe (long, double, long, double) IndexMinMax(double[] x,
            LoopMode loopMode = Defaults.LoopOption)
        {
            fixed(double* p = &x[0])
            {
                (long iMin, long iMax) = indexMinMax(x.LongLength, p, loopMode);
                return (iMin, *(p+iMin), iMax, *(p+iMax));
            }
        }

        /// <summary>
        /// finds the indices of the elements with 
        /// the largest and smallest values
        /// </summary>
        /// <param name="x"> input array x </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> (indexMin, min value, indexMax, max value) </returns>
        public static unsafe(long, double, long, double) IndexMinMax(double[,] x,
            LoopMode loopMode = Defaults.LoopOption)
        {
            fixed(double* p = &x[0,0])
            {
                (long iMin, long iMax) = indexMinMax(x.LongLength, p, loopMode);
                return (iMin, *(p + iMin), iMax, *(p + iMax));
            }
        }

        #endregion
        #region ---- Sort [D] ----

        /// <summary>
        /// sort the values in an array
        /// </summary>
        /// <param name="p"> input array </param>
        /// <param name="low"> lowest index </param>
        /// <param name="high"> highest index </param>
        private static unsafe void sort(double* p, 
            long low, long high)
        {
            if(low > high) { return; }

            long i = low;
            long j = high;
            double t = *(p + i);
            
            while(i < j)
            {
                while(i < j && t <= *(p + j)) { j--; }
                *(p + i) = *(p + j);
                while(i < j && t >= *(p + i)) { i++; }
                *(p + j) = *(p + i);
            }
            *(p + i) = t;

            sort(p, low, i - 1);
            sort(p, i + 1, high);
        }

        /// <summary>
        /// sort the values in an array
        /// </summary>
        /// <param name="x"> input array x </param>
        /// <param name="low"> lowest index </param>
        /// <param name="high"> highest index </param>
        internal static unsafe void SortD<T>(ref T x, 
            long low, long high) where T : DenseArrayBase<double>
        {
            double* p = (double*)x.DataPtr.ToPointer();
            sort(p, low, high);
        }

        #endregion
        #region ---- BinarySerach [D] ----

        /// <summary>
        /// Performs a binary search to find the span (interval) in a sorted array of long integers
        /// where the specified value <paramref name="x"/> would be located.
        /// The array must be sorted in ascending order.
        /// </summary>
        /// <param name="xs">A <see cref="DenseArray{long}"/> containing sorted values in ascending order.</param>
        /// <param name="x">The target value to search for.</param>
        /// <param name="k">
        /// When the method returns, contains the index of the span in which <paramref name="x"/> is found or would be inserted.
        /// If <paramref name="x"/> is less than the first element or greater than the last element, <paramref name="k"/> is set to <c>xs.Count - 1</c>.
        /// </param>
        /// <returns>
        /// <c>true</c> if <paramref name="x"/> is within the range of <paramref name="xs"/>; otherwise, <c>false</c>.
        /// If <paramref name="x"/> is equal to the last element, <paramref name="k"/> is set to the last index and <c>true</c> is returned.
        /// </returns>
        internal static unsafe bool BSearchSpan(DenseArray<long> xs,
            long x, out long k)
        {
            long n = xs.Count;
            k = n - 1;
            long* px = (long*)xs.VPtr;

            if (n == 0 || x < px[0] || x > px[k]) { return false; }
            if (x == px[k]) { return true; }

            long iBegin = 0;
            long iEnd = k;
            // Standard binary search for the span
            while (iEnd - iBegin > 1)
            {
                long iMid = iBegin + ((iEnd - iBegin) >> 1); // (iEnd - iBegin) >> 1 => (iEnd - iBegin) / 2
                if (x < px[iMid])
                { iEnd = iMid; }
                else
                { iBegin = iMid; }
            }
            k = iBegin;
            return true;
        }

        /// <summary>
        /// !!! for ascending order only !!!
        /// binary search to find the span where the value is located
        /// </summary>
        /// <typeparam name="T"> ArrayBase[long] </typeparam>
        /// <param name="xs"> array with ascending values </param>
        /// <param name="x"> target value to check </param>
        /// <param name="k"> index of the span, in which the value is found </param>
        /// <returns> result: whether found or not </returns>
        internal static unsafe bool BSearchSpan<T>(T xs,
            long x, out long k) where T : DenseArrayBase<long>
        {
            k = xs.Count - 1;
            long* px = (long*)xs.VPtr;

            if (x < *(px + 0) || x > *(px + k)) { return false; }
            else if (x == *(px + k)) { return true; }
            else
            {
                long iBegin = 0;
                long iEnd = xs.Count - 1;
                while (iEnd - iBegin != 1)
                {
                    long iMid = iBegin + (iEnd - iBegin) / 2;
                    // within [iBegin, iMid) ?
                    if (x >= *(px + iBegin) && x < *(px + iMid))
                    { iEnd = iMid; }
                    else
                    { iBegin = iMid; }
                }
                k = iBegin;
                return true;
            }
        }

        /// <summary>
        /// !!! for ascending order only !!!
        /// binary search to find the span where the value is located
        /// </summary>
        /// <typeparam name="T"> ArrayBase[double] </typeparam>
        /// <param name="xs"> array with ascending values </param>
        /// <param name="x"> target value to check </param>
        /// <param name="k"> index of the span, in which the value is found </param>
        /// <returns> result: whether found or not </returns>
        internal static unsafe bool BSearchSpan<T>(T xs,
            double x, out long k) where T : DenseArrayBase<double>
        {
            k = xs.Count - 1;
            long* px = (long*)xs.VPtr;

            if (x < *(px + 0) || x > *(px + k)) { return false; }
            else if (x == *(px + k)) { return true; }
            else
            {
                long iBegin = 0;
                long iEnd = xs.Count - 1;
                while (iEnd - iBegin != 1)
                {
                    long iMid = iBegin + (iEnd - iBegin) / 2;
                    // within [iBegin, iMid) ?
                    if (x >= *(px + iBegin) && x < *(px + iMid))
                    { iEnd = iMid; }
                    else
                    { iBegin = iMid; }
                }
                k = iBegin;
                return true;
            }
        }

        #endregion
        #region ---- Array ----

        #region vector => array

        /// <summary>
        /// converts a vector to array
        /// </summary>
        /// <param name="x"> input vector x </param>
        /// <param name="revOrder"> if true, reverse the order of elements </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> result array </returns>
        public static double[] ConvertVectorToArray(VectorD x,
            bool revOrder = false,
            LoopMode loopMode = Defaults.LoopOption)
        {
            if (x.Count > int.MaxValue)
            { throw new InvalidCastException("The number of vector elements exceeds the limit"); }

            // initialization
            double[] y = new double[x.Count];
            // defines loop operation
            Action<long> a = (i) =>
            {
                long j = revOrder ? x.Count - 1 - i : i;
                y[i] = x[j, false];
            };
            Loop1D loop = new(operation: a,
                start: 0, end: y.Length, step: 1);
            loop.Evaluate(mode: loopMode);

            // return
            return y;
        }

        /// <summary>
        /// converts a vector to array, with possible NaN values
        /// </summary>
        /// <param name="x"> input vector x </param>
        /// <param name="revOrder"> if true, reverse the order of elements </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> result array containing possible null values </returns>
        public static double?[] ConvertVector2Array(VectorD x,
            bool revOrder = false,
            LoopMode loopMode = Defaults.LoopOption)
        {
            if (x.Count > int.MaxValue)
            { throw new InvalidCastException("The number of vector elements exceeds the limit"); }

            // initialization
            double?[] y = new double?[x.Count];
            // defines loop operation
            Action<long> a = (i) =>
            {
                long j = revOrder ? x.Count - 1 - i : i;
                double v = x[j, false];
                y[i] = double.IsNaN(v) ? null : v;
            };
            Loop1D loop = new(operation: a,
                start: 0, end: y.Length, step: 1);
            loop.Evaluate(mode: loopMode);

            // return
            return y;
        }

        /// <summary>
        /// converts a vector to array
        /// </summary>
        /// <param name="x"> input vector x </param>
        /// <param name="revOrder"> if true, reverse the order of elements </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> result array </returns>
        public static Complex[] ConvertVectorToArray(VectorZ x,
            bool revOrder = false,
            LoopMode loopMode = Defaults.LoopOption)
        {
            if (x.Count > int.MaxValue)
            { throw new InvalidCastException("The number of vector elements exceeds the limit"); }

            // initialization
            Complex[] y = new Complex[x.Count];
            // defines loop operation
            Action<long> a = (i) =>
            {
                long j = revOrder ? x.Count - 1 - i : i;
                y[i] = x[j, false];
            };
            Loop1D loop = new(operation: a,
                start: 0, end: y.Length, step: 1);
            loop.Evaluate(mode: loopMode);

            // return
            return y;
        }

        #endregion
        #region array => vector

        /// <summary>
        /// converts an Array to vector
        /// </summary>
        /// <param name="x"> input array x </param>
        /// <param name="revOrder"> if true, reverse the order of elements </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> result vector </returns>
        public static VectorD ConvertArrayToVector(double[] x,
            bool revOrder = false,
            LoopMode loopMode = Defaults.LoopOption)
        {
            // initialization
            VectorD y = new(count: x.Length);
            // defines loop operation
            Action<long> a = (i) =>
            {
                long j = revOrder ? x.Length - 1 - i : i;
                y[i, false] = x[j];
            };
            Loop1D loop = new(operation: a,
                start: 0, end: y.Count, step: 1);
            loop.Evaluate(mode: loopMode);

            // return
            return y;
        }

        /// <summary>
        /// converts an Array to vector
        /// </summary>
        /// <param name="x"> input array x </param>
        /// <param name="revOrder"> if true, reverse the order of elements </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> result vector </returns>
        public static VectorZ ConvertArrayToVector(Complex[] x,
            bool revOrder = false,
            LoopMode loopMode = Defaults.LoopOption)
        {
            // initialization
            VectorZ y = new(count: x.Length);
            // defines loop operation
            Action<long> a = (i) =>
            {
                long j = revOrder ? x.Length - 1 - i : i;
                y[i, false] = x[j];
            };
            Loop1D loop = new(operation: a,
                start: 0, end: y.Count, step: 1);
            loop.Evaluate(mode: loopMode);

            // return
            return y;
        }

        #endregion
        #region matrix => array

        /// <summary>
        /// converts a matrix to 2D array
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <param name="revRows"> if true, reverse the order of the rows </param>
        /// <param name="revCols"> if true, reverse the order of the columns </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> result 2D array </returns>
        public static double[,] ConvertMatrixToArray(MatrixD x,
            bool revRows = false, bool revCols = false,
            LoopMode loopMode = Defaults.LoopOption)
        {
            if (x.Count > int.MaxValue)
            { throw new InvalidCastException("The number of matrix elements exceeds the limit"); }

            // initialization
            double[,] y = new double[x.Rows, x.Cols];
            // defines loop operation
            Action<long, long> a = (iRow, iCol) =>
            {
                long jRow = revRows ? x.Rows - 1 - iRow : iRow;
                long jCol = revCols ? x.Cols - 1 - iCol : iCol;
                y[iRow, iCol] = x[jRow, jCol, false];
            };
            Loop2D loop = new(operation: a,
                rowStart: 0, rowEnd: y.GetLength(0),
                colStart: 0, colEnd: y.GetLength(1),
                rowStep: 1, colStep: 1);
            loop.Evaluate(mode: loopMode);

            // return
            return y;
        }

        /// <summary>
        /// converts a matrix to 2D array, with possible NaN values
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <param name="revRows"> if true, reverse the order of the rows </param>
        /// <param name="revCols"> if true, reverse the order of the columns </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> result 2D array, containing possible null values </returns>
        public static double?[,] ConvertMatrix2Array(MatrixD x,
            bool revRows = false, bool revCols = false,
            LoopMode loopMode = Defaults.LoopOption)
        {
            if (x.Count > int.MaxValue)
            { throw new InvalidCastException("The number of matrix elements exceeds the limit"); }

            // initialization
            double?[,] y = new double?[x.Rows, x.Cols];
            // defines loop operation
            Action<long, long> a = (iRow, iCol) =>
            {
                long jRow = revRows ? x.Rows - 1 - iRow : iRow;
                long jCol = revCols ? x.Cols - 1 - iCol : iCol;
                double v = x[jRow, jCol, false];
                y[iRow, iCol] = double.IsNaN(v) ? null : v;
            };
            Loop2D loop = new(operation: a,
                rowStart: 0, rowEnd: y.GetLength(0),
                colStart: 0, colEnd: y.GetLength(1),
                rowStep: 1, colStep: 1);
            loop.Evaluate(mode: loopMode);

            // return
            return y;
        }

        /// <summary>
        /// converts a matrix to 2D array
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <param name="revRows"> if true, reverse the order of the rows </param>
        /// <param name="revCols"> if true, reverse the order of the columns </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> result 2D array </returns>
        public static Complex[,] ConvertMatrixToArray(MatrixZ x,
            bool revRows = false, bool revCols = false,
            LoopMode loopMode = Defaults.LoopOption)
        {
            if (x.Count > int.MaxValue)
            { throw new InvalidCastException("The number of matrix elements exceeds the limit"); }

            // initialization
            Complex[,] y = new Complex[x.Rows, x.Cols];
            // defines loop operation
            Action<long, long> a = (iRow, iCol) =>
            {
                long jRow = revRows ? x.Rows - 1 - iRow : iRow;
                long jCol = revCols ? x.Cols - 1 - iCol : iCol;
                y[iRow, iCol] = x[jRow, jCol, false];
            };
            Loop2D loop = new(operation: a,
                rowStart: 0, rowEnd: y.GetLength(0),
                colStart: 0, colEnd: y.GetLength(1),
                rowStep: 1, colStep: 1);
            loop.Evaluate(mode: loopMode);

            // return
            return y;
        }

        #endregion
        #region array => matrix

        /// <summary>
        /// converts an array to matrix
        /// </summary>
        /// <param name="x"> input array x </param>
        /// <param name="revRows"> if true, reverse the order of the rows </param>
        /// <param name="revCols"> if true, reverse the order of the columns </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> result matrix </returns>
        public static MatrixD ConvertArrayToMatrix(double[,] x,
            bool revRows = false, bool revCols = false,
            LoopMode loopMode = Defaults.LoopOption)
        {
            // initialization
            MatrixD y = new(rows: x.GetLength(0), cols: x.GetLength(1));
            // defines loop operation
            Action<long, long> a = (iRow, iCol) =>
            {
                long jRow = revRows ? x.GetLength(0) - 1 - iRow : iRow;
                long jCol = revCols ? x.GetLength(1) - 1 - iCol : iCol;
                y[iRow, iCol, false] = x[jRow, jCol];
            };
            Loop2D loop = new(operation: a,
                rowStart: 0, rowEnd: y.Rows,
                colStart: 0, colEnd: y.Cols,
                rowStep: 1, colStep: 1);
            loop.Evaluate(mode: loopMode);

            // return
            return y;
        }

        /// <summary>
        /// converts an array to matrix
        /// </summary>
        /// <param name="x"> input array x </param>
        /// <param name="revRows"> if true, reverse the order of the rows </param>
        /// <param name="revCols"> if true, reverse the order of the columns </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> result matrix </returns>
        public static MatrixZ ConvertArrayToMatrix(Complex[,] x,
            bool revRows = false, bool revCols = false,
            LoopMode loopMode = Defaults.LoopOption)
        {
            // initialization
            MatrixZ y = new(rows: x.GetLength(0), cols: x.GetLength(1));
            // defines loop operation
            Action<long, long> a = (iRow, iCol) =>
            {
                long jRow = revRows ? x.GetLength(0) - 1 - iRow : iRow;
                long jCol = revCols ? x.GetLength(1) - 1 - iCol : iCol;
                y[iRow, iCol, false] = x[jRow, jCol];
            };
            Loop2D loop = new(operation: a,
                rowStart: 0, rowEnd: y.Rows,
                colStart: 0, colEnd: y.Cols,
                rowStep: 1, colStep: 1);
            loop.Evaluate(mode: loopMode);

            // return
            return y;
        }

        #endregion

        #endregion
        #region ---- Unwrap ----

        /// <summary>
        /// unwraps the phase given within 2Pi
        /// </summary>
        /// <param name="phase2PI"> input phase within 2Pi </param>
        /// <param name="startIdx"> start index for the unwrapping </param>
        /// <returns> unwrapped phase distribution </returns>
        public static VectorD UnwrapPhase(VectorD phase2PI, long startIdx = 0)
        {
            VectorD psi = new(phase2PI.Count);

            // initialize indices
            long p = 0, n = 0;

            // fixes the phase value at the start index
            psi[startIdx, false] = phase2PI[startIdx, false];
            // fromt start index to the last
            if (startIdx < phase2PI.Count - 1)
            {
                for (long i = startIdx + 1; i < phase2PI.Count; i++)
                {
                    double diff = phase2PI[i, false] - phase2PI[i - 1, false];
                    if (diff < -Math.PI)
                        p++;
                    else if (diff > Math.PI)
                        p--;
                    psi[i, false] = phase2PI[i, false] + 2 * Math.PI * p;
                }
            }
            // from start index to the first
            if (startIdx > 0)
            {
                for (long i = startIdx - 1; i >= 0; i--)
                {
                    double diff = phase2PI[i, false] - phase2PI[i + 1, false];
                    if (diff < -Math.PI)
                        n++;
                    else if (diff > Math.PI)
                        n--;
                    psi[i, false] = phase2PI[i, false] + 2 * Math.PI * n;
                }
            }

            // return
            return psi;
        }

        #endregion

        #region --- QR ----


        // matrix qr decomposition 

        /// <summary>
        /// Performs QR decomposition of a real matrix using Gram-Schmidt process.
        /// A = Q * R
        /// </summary>
        /// <param name="A">Input matrix (m x n)</param>
        /// <param name="Q">Output orthogonal matrix (m x n)</param>
        /// <param name="R">Output upper triangular matrix (n x n)</param>
        private unsafe static (MatrixD, MatrixD) QRDecomposition(MatrixD A)
        {
            long m = A.Rows;
            long n = A.Cols;
            double* pa = (double*)A.VPtr;
            MatrixD Q = new MatrixD(m, n);
            MatrixD R = new MatrixD(n, n);
            double* pq = (double*)Q.VPtr;
            double* pr = (double*)R.VPtr;

            // Temporary storage for column vectors
            VectorD[] v = new VectorD[n];
            
            for (long j = 0; j < n; j++)
            {
                // Copy column j of A into v[j]
                v[j] = new VectorD(m);
                double* pv = (double*)v[j].VPtr;
                for (long i = 0; i < m; i++)
                    //v[j][i, false] = A[i, j, false];
                    *(pv + i) = *(pa + i * n + j);
            }

            for (long k = 0; k < n; k++)
            {
                double* pv = (double*)v[k].VPtr;
                // Orthogonalization
                for (long i = 0; i < k; i++)
                {
                    double dot = 0.0;
                    for (long j = 0; j < m; j++)
                        //dot += Q[j, i, false] * v[k][j, false];
                        dot += *(pq + j * n + i) * *(pv + j);
                    //R[i, k, false] = dot;
                    *(pr + i * n + k) = dot;
                    for (long j = 0; j < m; j++)
                        //v[k][j, false] -= dot * Q[j, i, false];
                        *(pv + j) -= dot * *(pq + j * n + i);
                }

                // Normalization
                double norm = 0.0;
                for (long j = 0; j < m; j++)
                    //norm += v[k][j, false] * v[k][j, false];
                    norm += *(pv + j) * *(pv + j);
                norm = Math.Sqrt(norm);
                //R[k, k, false] = norm;
                *(pr + k * n + k) = norm;
                for (long j = 0; j < m; j++)
                    //Q[j, k, false] = v[k][j, false] / norm;
                    *(pq + j * n + k) = *(pv + j) / norm;
            }

            return (Q, R);
        }


        /// <summary>
        /// Solves the linear least squares problem using QR decomposition:
        /// min_x ||A x - b||_2
        /// </summary>
        /// <param name="A">Input matrix A (m x n, m >= n)</param>
        /// <param name="b">Input vector b (length m)</param>
        /// <returns>Solution vector x (length n)</returns>
        private unsafe static VectorD QRLeastSquare(MatrixD A, VectorD b)
        {
            // QR decomposition: A = Q * R
            (MatrixD Q, MatrixD R) = QRDecomposition(A);
            double* pq = (double*)Q.VPtr;
            double* pr = (double*)R.VPtr;

            // Compute Q^T * b
            VectorD Qtb = new VectorD(R.Rows);
            double* pt = (double*)Qtb.VPtr;
            double* pb = (double*)b.VPtr;
            for (long i = 0; i < R.Rows; i++)
            {
                double sum = 0.0;
                for (long j = 0; j < Q.Rows; j++)
                    //sum += Q[j, i, false] * b[j, false];
                    sum += *(pq + j * R.Rows + i) * *(pb + j);
                //Qtb[i, false] = sum;
                *(pt + i) = sum;
            }

            // Back substitution to solve R x = Q^T b
            VectorD x = new VectorD(R.Cols);
            double* px = (double*)x.VPtr;
            for (long i = R.Cols - 1; i >= 0; i--)
            {
                double sum = Qtb[i, false];
                for (long j = i + 1; j < R.Cols; j++)
                    //sum -= R[i, j, false] * x[j, false];
                    sum -= *(pr + i * R.Cols + j) * *(px + j);
                //x[i, false] = sum / R[i, i, false];
                *(px + i) = sum / *(pr + i * R.Cols + i);
            }
            return x;
        }

        #endregion

    }
}
