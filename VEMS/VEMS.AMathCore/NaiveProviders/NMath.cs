using System.Numerics;
using System.Runtime.InteropServices;

namespace VEMS.AMathCore
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

}
