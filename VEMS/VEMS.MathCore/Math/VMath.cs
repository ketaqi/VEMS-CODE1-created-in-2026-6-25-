using Int = System.Int64;
using Real = System.Double;
//using Complex = VEMS.MathCore.Cplx;
using Complex = System.Numerics.Complex;
using System.Numerics;

namespace VEMS.MathCore
{
    //internal class VMathKernel
    //{
    //    internal IBLAS iBLAS { get; set; }
    //    internal IVMF iVMF { get; set; }
    //    internal VMathKernel()
    //    {
    //        iBLAS = Defaults.IBLAS; 
    //        iVMF = Defaults.IVMF; 
    //    }
    //}

    internal class VMathFactory
    {
        internal IBLAS iBLAS { get; set; }
        internal IVMF iVMF { get; set; }
        internal VMathFactory()
        {
            iBLAS = Defaults.IBLAS;
            iVMF = Defaults.IVMF;
        }
    }

    /// <summary>
    /// vector math methods
    /// </summary>
    public unsafe class VMath
    {
        //private static VMathKernel kernel = new();
        private static VMathFactory factory = new();

        #region Basic setting methods

        /// <summary>
        /// set up the BLAS interface with options
        /// from IntelMKL, OpenBLAS, etc
        /// </summary>
        /// <param name="option"> BLAS interface options </param>
        public static void SetIBLAS(IBLAS option)
            => factory.iBLAS = option;

        /// <summary>
        /// set up the VMF interface with options
        /// from IntelMKL, OpenBLAS, etc
        /// </summary>
        /// <param name="option"> VMF interface options </param>
        public static void SetIVMF(IVMF option)
            => factory.iVMF = option;

        #endregion

        #region NMath methods

        #region ---- AddTo ----

        /// <summary>
        /// Adds a scalar value <paramref name="s"/> to each element of the dense array <paramref name="x"/>.
        /// </summary>
        /// <typeparam name="T">The numeric type of the array elements, implementing <see cref="INumber{T}"/>.</typeparam>
        /// <param name="s">The scalar value to add to each element.</param>
        /// <param name="x">The dense array whose elements will be updated in place.</param>
        /// <param name="loopMode">Specifies the loop-computation mode option. Default is <see cref="Defaults.LoopOption"/>.</param>
        public static void AddTo<T>(ref DenseArray<T> x, T s,
            LoopMode loopMode = Defaults.LoopOption)
            where T : INumber<T>
            => AddTo(x, s, loopMode);

        internal static void AddTo<T>(DenseArray<T> x, T s,
            LoopMode loopMode = Defaults.LoopOption)
            where T : INumber<T>
            => NMath.Axps<T>(n: x.Count, x: (T*)x.VPtr, s: s,
                a: default!, incx: 1, loopMode);

        #endregion
        #region ---- Sum ----

        /// <summary>
        /// Computes the sum of all elements in the specified dense array.
        /// </summary>
        /// <typeparam name="T">The numeric type of the array elements.</typeparam>
        /// <param name="x">The dense array whose elements will be summed.</param>
        /// <param name="loopMode">The loop-computation mode option. Default is <see cref="Defaults.LoopOption"/>.</param>
        /// <returns>The sum of all elements in the array.</returns>
        public static T Sum<T>(DenseArray<T> x,
            LoopMode loopMode = Defaults.LoopOption)
            where T : INumber<T>
            => NMath.Sum<T>(n: x.Count, x: (T*)x.VPtr, incx: 1, loopMode);

        #endregion
        #region ---- Max ----

        /// <summary>
        /// Finds the index and value of the maximum element in a dense array.
        /// </summary>
        /// <typeparam name="T">The numeric type of the array elements.</typeparam>
        /// <param name="x">The dense array to search.</param>
        /// <param name="loopMode">The loop computation mode option (optional).</param>
        /// <returns>
        /// A tuple containing:
        /// <list type="bullet">
        /// <item><description>The index of the maximum element.</description></item>
        /// <item><description>The value of the maximum element.</description></item>
        /// </list>
        /// </returns>
        public static (long, T) Max<T>(DenseArray<T> x,
            LoopMode loopMode = Defaults.LoopOption)
            where T : INumber<T>
        {
            long iMax = NMath.IMax(n: x.Count, x: x.TPtr, incx: 1);
            return (iMax, *(x.TPtr + iMax));
        }

        #endregion
        #region ---- Min ----

        /// <summary>
        /// Finds the index and value of the element with the minimum absolute value in a dense array.
        /// </summary>
        /// <typeparam name="T">The numeric type of the array elements, must implement <see cref="INumber{T}"/>.</typeparam>
        /// <param name="x">The dense array to search.</param>
        /// <param name="loopMode">The loop-computation mode option. Default is <see cref="Defaults.LoopOption"/>.</param>
        /// <returns>
        /// A tuple containing:
        /// <list type="bullet">
        /// <item><description>The index of the element with the minimum absolute value.</description></item>
        /// <item><description>The value of the element at that index.</description></item>
        /// </list>
        /// </returns>
        public static (long, T) Min<T>(DenseArray<T> x,
            LoopMode loopMode = Defaults.LoopOption)
            where T : INumber<T>
        {
            long iMin = NMath.IMin(n: x.Count, x: x.TPtr, incx: 1);
            return (iMin, *(x.TPtr + iMin));
        }

        #endregion
        #region ---- Sort ----

        /// <summary>
        /// Sorts the elements of the specified <see cref="DenseArray{T}"/> in ascending order.
        /// </summary>
        /// <typeparam name="T">The numeric type of the array elements. Must implement <see cref="INumber{T}"/>.</typeparam>
        /// <param name="x">The array to sort. The array is sorted in place.</param>
        public static void Sort<T>(ref DenseArray<T> x)
            where T : INumber<T>
             => Sort(x);

        internal static void Sort<T>(DenseArray<T> x)
            where T : INumber<T>
            => NMath.Sort(x: x.TPtr, low: 0, high: x.Count - 1);

        #endregion
        #region ---- BinarySearch ----

        /// <summary>
        /// Performs a binary search for a value in a sorted dense array.
        /// </summary>
        /// <typeparam name="T">Numeric type.</typeparam>
        /// <param name="x">Dense array to search.</param>
        /// <param name="value">Value to search for.</param>
        /// <param name="index">Index of the found value, if present.</param>
        /// <returns>True if value is found; otherwise, false.</returns>
        public static bool BinarySearch<T>(DenseArray<T> x,
            T value, out long index) where T : INumber<T>
            => NMath.BinarySearch(n: x.Count, x: x.TPtr, xk: value, out index);

        #endregion
        // conversion
        #region ---- Convert To ----

        /// <summary>
        /// Converts a <see cref="Vect{T}"/> to an array of nullable <typeparamref name="T"/> values.
        /// </summary>
        /// <typeparam name="T">The numeric type of the vector elements.</typeparam>
        /// <param name="x">The input vector to convert.</param>
        /// <param name="revOrder">If <c>true</c>, reverses the order of the elements in the output array.</param>
        /// <param name="loopMode">Specifies the loop-computation mode option.</param>
        /// <returns>An array of nullable <typeparamref name="T"/> values representing the vector elements.</returns>
        public static T?[] ConvertTo<T>(Vect<T> x,
            bool revOrder = false,
            LoopMode loopMode = Defaults.LoopOption)
            where T : INumber<T>
            => NMath.CnvtTo<T>(n: x.Count, x: x.TPtr, revOrder, loopMode);

        /// <summary>
        /// Converts a <see cref="Matx{T}"/> to a two-dimensional array of nullable <typeparamref name="T"/> values.
        /// </summary>
        /// <typeparam name="T">The numeric type of the matrix elements.</typeparam>
        /// <param name="x">The input matrix to convert.</param>
        /// <param name="revRows">If <c>true</c>, reverses the order of the rows in the output array.</param>
        /// <param name="revCols">If <c>true</c>, reverses the order of the columns in the output array.</param>
        /// <param name="loopMode">Specifies the loop-computation mode option.</param>
        /// <returns>A two-dimensional array of nullable <typeparamref name="T"/> values representing the matrix elements.</returns>
        public static T?[,] ConvertTo<T>(Matx<T> x,
            bool revRows = false, bool revCols = false,
            LoopMode loopMode = Defaults.LoopOption)
            where T : INumber<T>
            => NMath.CnvtTo<T>(rows: x.Rows, cols: x.Cols, x: x.TPtr,
                revRows, revCols, loopMode);

        #endregion
        #region ---- Convert From ----

        /// <summary>
        /// Converts a nullable array of type <typeparamref name="T"/> to a <see cref="Vect{T}"/>.
        /// </summary>
        /// <typeparam name="T">The numeric type implementing <see cref="INumber{T}"/>.</typeparam>
        /// <param name="x">The source array of nullable values.</param>
        /// <param name="revOrder">If true, reverses the order of elements during conversion.</param>
        /// <param name="loopMode">Specifies the loop computation mode.</param>
        /// <returns>A <see cref="Vect{T}"/> containing the converted values.</returns>
        public static Vect<T> ConvertFrom<T>(T?[] x,
            bool revOrder = false,
            LoopMode loopMode = Defaults.LoopOption)
            where T : INumber<T>
        {
            Vect<T> y = new(count: x.Length, initMode: ArrayInitMode.Malloc);
            NMath.CnvtFrom<T>(n: x.Length, x: x, y: y.TPtr, revOrder, loopMode);
            return y;
        }

        /// <summary>
        /// Converts a nullable 2D array of type <typeparamref name="T"/> to a <see cref="Matx{T}"/>.
        /// </summary>
        /// <typeparam name="T">The numeric type implementing <see cref="INumber{T}"/>.</typeparam>
        /// <param name="x">The source 2D array of nullable values.</param>
        /// <param name="revRows">If true, reverses the order of rows during conversion.</param>
        /// <param name="revCols">If true, reverses the order of columns during conversion.</param>
        /// <param name="loopMode">Specifies the loop computation mode.</param>
        /// <returns>A <see cref="Matx{T}"/> containing the converted values.</returns>
        public static Matx<T> ConvertFrom<T>(T?[,] x,
            bool revRows = false, bool revCols = false,
            LoopMode loopMode = Defaults.LoopOption)
            where T : INumber<T>
        {
            long rows = x.GetLength(0);
            long cols = x.GetLength(1);
            Matx<T> y = new(rows: rows, cols: cols, initMode: ArrayInitMode.Malloc);
            NMath.CnvtFrom<T>(rows: rows, cols: cols, x: x, y: y.TPtr,
                revRows, revCols, loopMode);
            return y;
        }

        #endregion
        // phase
        #region ---- Unwrap [D] ----

        /// <summary>
        /// Unwraps phase values in <paramref name="x"/> to the range [0, 2π).
        /// </summary>
        /// <param name="x">Input dense array of phase values.</param>
        /// <param name="startIndex">Starting index for unwrapping.</param>
        /// <returns>Unwrapped phase array.</returns>
        public static DenseArray<Real> Unwrap2PI(DenseArray<Real> x,
            long startIndex = 0)
        {
            DenseArray<Real> y = new(count: x.Count, initMode: ArrayInitMode.Malloc);
            Copy(x, ref y);
            NMath.Unwrap2PI(n: y.Count, x: y.DPtr, startIndex);
            return y;
        }

        #endregion

        #endregion

        // obsolete ...
        #region Naive methods

        #region ---- AddTo [D/Z] ----

        /// <summary>
        /// adds a scalar s to vector x
        /// xi := xi + s
        /// </summary>
        /// <param name="s"> scalar s </param>
        /// <param name="x"> vector x </param>
        /// <param name="loopMode"> loop-computational mode </param>
        public static void AddTo(double s, ref VectorD x,
            LoopMode loopMode = Defaults.LoopOption)
            => NaiveMath.AddToD(s, ref x, loopMode);

        /// <summary>
        /// adds a scalar s to matrix x
        /// xi := xi + s
        /// </summary>
        /// <param name="s"> scalar s </param>
        /// <param name="x"> matrix x </param>
        /// <param name="loopMode"> loop-computational mode </param>
        public static void AddTo(double s, ref MatrixD x,
            LoopMode loopMode = Defaults.LoopOption)
            => NaiveMath.AddToD(s, ref x, loopMode);

        /// <summary>
        /// adds a scalar s to vector x
        /// xi := xi + s
        /// </summary>
        /// <param name="s"> scalar s </param>
        /// <param name="x"> vector x </param>
        /// <param name="loopMode"> loop-computational mode </param>
        public static void AddTo(Complex s, ref VectorZ x,
            LoopMode loopMode = Defaults.LoopOption)
            => NaiveMath.AddToZ(s, ref x, loopMode);

        /// <summary>
        /// adds a scalar s to matrix x
        /// xi := xi + s
        /// </summary>
        /// <param name="s"> scalar s </param>
        /// <param name="x"> matrix x </param>
        /// <param name="loopMode"> loop-computational mode </param>
        public static void AddTo(Complex s, ref MatrixZ x,
            LoopMode loopMode = Defaults.LoopOption)
            => NaiveMath.AddToZ(s, ref x, loopMode);

        #endregion
        #region ---- Sum [D/Z] ----

        /// <summary>
        /// sums up all the elements in x
        /// </summary>
        /// <param name="x"> input vector x </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> summation result </returns>
        public static double Sum(VectorD x,
            LoopMode loopMode = Defaults.LoopOption)
            => NaiveMath.SumD(x, loopMode); //NaiveMath.Sum(x, loopMode);

        /// <summary>
        /// sums up all the elements in x
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> summation result </returns>
        public static double Sum(MatrixD x,
            LoopMode loopMode = Defaults.LoopOption)
            => NaiveMath.SumD(x, loopMode); //NaiveMath.Sum(x, loopMode);

        /// <summary>
        /// sums up all the elements in x
        /// </summary>
        /// <param name="x"> input vector x </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> summation result </returns>
        public static Complex Sum(VectorZ x,
            LoopMode loopMode = Defaults.LoopOption)
            => NaiveMath.SumZ(x, loopMode); //NaiveMath.Sum(x, loopMode);

        /// <summary>
        /// sums up all the elements in x
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> summation result </returns>
        public static Complex Sum(MatrixZ x,
            LoopMode loopMode = Defaults.LoopOption)
            => NaiveMath.SumZ(x, loopMode); //NaiveMath.Sum(x, loopMode);

        #endregion
        #region ---- Max [D] ----

        /// <summary>
        /// finds the index of the element with the largest value
        /// </summary>
        /// <param name="x"> input vector x </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> (index, value) </returns>
        public static (long, double) Max(VectorD x, 
            LoopMode loopMode = Defaults.LoopOption)
            => NaiveMath.MaxD(x, loopMode);

        /// <summary>
        /// finds the index of the element with the largest value
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> (index, value) </returns>
        public static (long, double) Max(MatrixD x, 
            LoopMode loopMode = Defaults.LoopOption)
            => NaiveMath.MaxD(x, loopMode);

        #endregion
        #region ---- Min [D] ----

        /// <summary>
        /// finds the index of the element with the smallest value
        /// </summary>
        /// <param name="x"> input vector x </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> (index, value) </returns>
        public static (long, double) Min(VectorD x,
            LoopMode loopMode = Defaults.LoopOption)
            => NaiveMath.MinD(x, loopMode);

        /// <summary>
        /// finds the index of the element with the smallest value
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> (index, value) </returns>
        public static (long, double) Min(MatrixD x,
            LoopMode loopMode = Defaults.LoopOption)
            => NaiveMath.MinD(x, loopMode);

        #endregion
        #region ---- MinMax [D] ----

        /// <summary>
        /// finds the indices of the smallest and largest elements
        /// </summary>
        /// <param name="x"> input vector x </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> (index-min, min-value, index-max, max-value) </returns>
        public static (long, double, long, double) IndexMinMax(VectorD x,
            LoopMode loopMode = Defaults.LoopOption)
            => NaiveMath.MinMaxD(x, loopMode);

        /// <summary>
        /// finds the indices of the smallest and largest elements
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> (index-min, min-value, index-max, max-value) </returns>
        public static (long, double, long, double) IndexMinMax(MatrixD x,
            LoopMode loopMode = Defaults.LoopOption)
            => NaiveMath.MinMaxD(x, loopMode);

        /// <summary>
        /// finds the indices of the smallest and largest elements
        /// </summary>
        /// <param name="x"> input array x </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> (index-min, min-value, index-max, max-value) </returns>
        public static (long, double, long, double) IndexMinMax(double[,] x,
            LoopMode loopMode = Defaults.LoopOption)
            => NaiveMath.IndexMinMax(x, loopMode);

        #endregion
        #region ---- Sort [D] ----

        /// <summary>
        /// sorts the elements in a vector
        /// </summary>
        /// <param name="x"> input vector </param>
        public static void Sort(ref VectorD x)
            => NaiveMath.SortD(ref x, low: 0, high: x.Count - 1);

        /// <summary>
        /// sorts the elements in a matrix
        /// </summary>
        /// <param name="x"> input matrix </param>
        public static void Sort(ref MatrixD x)
            => NaiveMath.SortD(ref x, low: 0, high: x.Count - 1);

        #endregion
        #region ---- BinarySearch [D] ----

        /// <summary>
        /// !!! for ascending order only !!!
        /// binary search to find the span where the value is located
        /// </summary>
        /// <param name="x"> vector with ascending values </param>
        /// <param name="value"> target value to check </param>
        /// <param name="index"> index of the span, in which the value is found </param>
        /// <returns> result: whether found or not </returns>
        public static bool BinarySearch(VectorD x, double value, out long index)
            => NaiveMath.BSearchSpan(x, value, out index);

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
            => NaiveMath.ConvertVectorToArray(x, revOrder, loopMode);

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
            => NaiveMath.ConvertVector2Array(x, revOrder, loopMode);

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
            => NaiveMath.ConvertVectorToArray(x, revOrder, loopMode);

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
            => NaiveMath.ConvertArrayToVector(x, revOrder, loopMode);

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
            => NaiveMath.ConvertArrayToVector(x, revOrder, loopMode);

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
            => NaiveMath.ConvertMatrixToArray(x, revRows, revCols, loopMode);

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
            => NaiveMath.ConvertMatrix2Array(x, revRows, revCols, loopMode);

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
            => NaiveMath.ConvertMatrixToArray(x, revRows, revCols, loopMode);

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
            => NaiveMath.ConvertArrayToMatrix(x, revRows, revCols, loopMode);

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
            => NaiveMath.ConvertArrayToMatrix(x, revRows, revCols, loopMode);

        #endregion

        #endregion
        #region ---- Unwrap [D] ----

        /// <summary>
        /// unwraps the phase given within 2Pi
        /// </summary>
        /// <param name="phase2PI"> input phase within 2Pi </param>
        /// <param name="startIdx"> start index for the unwrapping </param>
        /// <returns> unwrapped phase distribution </returns>
        public static VectorD UnwrapPhase(VectorD phase2PI, long startIdx = 0)
            => NaiveMath.UnwrapPhase(phase2PI, startIdx);

        /// <summary>
        /// unwraps the phase part of a complex data
        /// </summary>
        /// <param name="x"> input complex data </param>
        /// <param name="startIdx"> start index for the unwrapping </param>
        /// <returns> unwrapped phase distribution </returns>
        public static VectorD UnwrapPhase(VectorZ x, long startIdx = 0)
        {
            // takes argument from the complex input
            VectorD arg = Arg(x);
            // calls unwrap method
            return UnwrapPhase(arg, startIdx);
        }

        #endregion

        #endregion
        // obsolete ...
        #region Naive-Extensions

        #region ---- Add [D/Z] ----

        /// <summary>
        /// adds a scalar s to vector x
        /// yi = s + xi
        /// </summary>
        /// <param name="x"> vector x </param>
        /// <param name="s"> scalar s </param>
        /// <param name="loopMode"> loop-computational mode </param>
        /// <returns> result vector y </returns>
        public static VectorD Add(VectorD x, double s,
            LoopMode loopMode = Defaults.LoopOption)
        {
            VectorD y = new(other: x, deepCopy: true);
            AddTo(s, ref y, loopMode);
            return y;
        }

        /// <summary>
        /// adds a scalar s to matrix x
        /// yi = s + xi
        /// </summary>
        /// <param name="x"> matrix x </param>
        /// <param name="s"> scalar s </param>
        /// <param name="loopMode"> loop-computational mode </param>
        /// <returns> matrix vector y </returns>
        public static MatrixD Add(MatrixD x, double s,
            LoopMode loopMode = Defaults.LoopOption)
        {
            MatrixD y = new(other: x, deepCopy: true);
            AddTo(s, ref y, loopMode);
            return y;
        }

        /// <summary>
        /// adds a scalar s to vector x
        /// yi = s + xi
        /// </summary>
        /// <param name="x"> vector x </param>
        /// <param name="s"> scalar s </param>
        /// <param name="loopMode"> loop-computational mode </param>
        /// <returns> result vector y </returns>
        public static VectorZ Add(VectorZ x, Complex s,
            LoopMode loopMode = Defaults.LoopOption)
        {
            VectorZ y = new(other: x, deepCopy: true);
            AddTo(s, ref y, loopMode);
            return y;
        }

        /// <summary>
        /// adds a scalar s to matrix x
        /// yi = s + xi
        /// </summary>
        /// <param name="x"> matrix x </param>
        /// <param name="s"> scalar s </param>
        /// <param name="loopMode"> loop-computational mode </param>
        /// <returns> matrix vector y </returns>
        public static MatrixZ Add(MatrixZ x, Complex s,
            LoopMode loopMode = Defaults.LoopOption)
        {
            MatrixZ y = new(other: x, deepCopy: true);
            AddTo(s, ref y, loopMode);
            return y;
        }

        #endregion
        #region ---- Sub [D/Z] ----

        /// <summary>
        /// subtracts a scalar s from vector x
        /// yi = xi - s
        /// </summary>
        /// <param name="x"> vector x </param>
        /// <param name="s"> scalar s </param>
        /// <param name="loopMode"> loop-computational mode </param>
        /// <returns> result vector y </returns>
        public static VectorD Sub(VectorD x, double s,
            LoopMode loopMode = Defaults.LoopOption)
            => Add(x, -s, loopMode);

        /// <summary>
        /// subtracts a scalar s from matrix x
        /// yi = xi - s
        /// </summary>
        /// <param name="x"> matrix x </param>
        /// <param name="s"> scalar s </param>
        /// <param name="loopMode"> loop-computational mode </param>
        /// <returns> result matrix y </returns>
        public static MatrixD Sub(MatrixD x, double s,
            LoopMode loopMode = Defaults.LoopOption)
            => Add(x, -s, loopMode);

        /// <summary>
        /// subtracts a scalar s from vector x
        /// yi = xi - s
        /// </summary>
        /// <param name="x"> vector x </param>
        /// <param name="s"> scalar s </param>
        /// <param name="loopMode"> loop-computational mode </param>
        /// <returns> result vector y </returns>
        public static VectorZ Sub(VectorZ x, Complex s,
            LoopMode loopMode = Defaults.LoopOption)
            => Add(x, -s, loopMode);

        /// <summary>
        /// subtracts a scalar s from matrix x
        /// yi = xi - s
        /// </summary>
        /// <param name="x"> matrix x </param>
        /// <param name="s"> scalar s </param>
        /// <param name="loopMode"> loop-computational mode </param>
        /// <returns> result matrix y </returns>
        public static MatrixZ Sub(MatrixZ x, Complex s,
            LoopMode loopMode = Defaults.LoopOption)
            => Add(x, -s, loopMode);

        #endregion
        #region ---- Sub [D/Z] ----

        /// <summary>
        /// subtracts each element of vector x from a scalar s
        /// yi = s - xi
        /// </summary>
        /// <param name="s"> scalar s </param>
        /// <param name="x"> vector x </param>
        /// <param name="loopMode"> loop-computational mode </param>
        /// <returns> result vector y </returns>
        public static VectorD Sub(double s, VectorD x,
            LoopMode loopMode = Defaults.LoopOption)
            => Add(-x, s, loopMode); //Add(Scale(x, -1.0), s, loopMode);

        /// <summary>
        /// subtracts each element of matrix x from a scalar s
        /// yi = s - xi
        /// </summary>
        /// <param name="s"> scalar s </param>
        /// <param name="x"> matrix x </param>
        /// <param name="loopMode"> loop-computational mode </param>
        /// <returns> result matrix y </returns>
        public static MatrixD Sub(double s, MatrixD x,
            LoopMode loopMode = Defaults.LoopOption)
            => Add(-x, s, loopMode); //Add(Scale(x, -1.0), s, loopMode);

        /// <summary>
        /// subtracts each element of vector x from a scalar s
        /// yi = s - xi
        /// </summary>
        /// <param name="s"> scalar s </param>
        /// <param name="x"> vector x </param>
        /// <param name="loopMode"> loop-computational mode </param>
        /// <returns> result vector y </returns>
        public static VectorZ Sub(Complex s, VectorZ x,
            LoopMode loopMode = Defaults.LoopOption)
            => Add(-x, s, loopMode); //Add(Scale(x, -1.0), s, loopMode);

        /// <summary>
        /// subtracts each element of matrix x from a scalar s
        /// yi = s - xi
        /// </summary>
        /// <param name="s"> scalar s </param>
        /// <param name="x"> matrix x </param>
        /// <param name="loopMode"> loop-computational mode </param>
        /// <returns> result matrix y </returns>
        public static MatrixZ Sub(Complex s, MatrixZ x,
            LoopMode loopMode = Defaults.LoopOption)
            => Add(-x, s, loopMode); //Add(Scale(x, -1.0), s, loopMode);

        #endregion

        #endregion

        #region BLAS methods

        #region helpers 

        [Obsolete]
        private static void ExArrayLengthD<T>(T x, T y) where T : DenseArrayBase<double>
        {
            if (x.Count != y.Count)
            //{ throw new ArgumentException($"Array lengths not equal"); }
            { Printer.Warning($"Array lengths not equal"); }
        }

        [Obsolete]
        private static void ExArrayLengthZ<T>(T x, T y) where T : DenseArrayBase<Complex>
        {
            if (x.Count != y.Count)
            //{ throw new ArgumentException($"Array lengths not equal"); }
            { Printer.Warning($"Array lengths not equal"); }
        }

        #endregion
        #region ---- IAmx [D/Z] ----

        /// <summary>
        /// Finds the index of the element with the maximum absolute value in a dense array.
        /// </summary>
        /// <typeparam name="T">
        /// The numeric type of the array elements. Supported types are <see cref="Real"/> (double) and <see cref="Cplx"/> (complex).
        /// </typeparam>
        /// <param name="x">The dense array whose elements are to be examined.</param>
        /// <param name="incx">The increment for indexing <paramref name="x"/>. Default is 1.</param>
        /// <returns>
        /// The index of the element with the largest absolute value.
        /// </returns>
        /// <exception cref="NotSupportedException">
        /// Thrown if <typeparamref name="T"/> is not <see cref="Real"/> or <see cref="Cplx"/>.
        /// </exception>
        public static long IAmx<T>(DenseArray<T> x, long incx = 1)
            where T : INumber<T>
        {
            if (typeof(T) == typeof(Real)) return factory.iBLAS.Iamax(x.Count, x.DPtr, incx);
            else if (typeof(T) == typeof(Cplx)) return factory.iBLAS.Iamax(x.Count, x.VPtr, incx);
            else throw new NotSupportedException($"Type {typeof(T)} not supported.");
        }


        ///// <summary>
        ///// finds the index of the element with the largest absolute value
        ///// </summary>
        ///// <typeparam name="T"> ArrayBase[double] </typeparam>
        ///// <param name="x"> input array x </param>
        ///// <returns> index of the element with largest absolute value </returns>
        //public static long IAmxD<T>(T x) where T : DenseArrayBase<double>
        //    => factory.iBLAS.IamaxD(x.Count, x);

        ///// <summary>
        ///// finds the index of the element with the largest absolute value
        ///// </summary>
        ///// <typeparam name="T"> ArrayBase[Complex] </typeparam>
        ///// <param name="x"> input array x </param>
        ///// <returns> index of the element with largest absolute value </returns>
        //public static long IAmxZ<T>(T x) where T : DenseArrayBase<Complex>
        //    => factory.iBLAS.IamaxZ(x.Count, x);

        /// <summary>
        /// finds the index of the element with the largest absolute value
        /// </summary>
        /// <param name="x"> input vector x </param>
        /// <returns> index of the element with largest absolute value </returns>
        public static long IAmx(VectorD x)
            => factory.iBLAS.IamaxD(x.Count, x); //IAmxD(x);

        /// <summary>
        /// finds the index of the element with the largest absolute value
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <returns> index of the element with largest absolute value </returns>
        public static long IAmx(MatrixD x)
            => factory.iBLAS.IamaxD(x.Count, x); //IAmxD(x);

        /// <summary>
        /// finds the index of the element with the largest absolute value
        /// </summary>
        /// <param name="x"> input vector x </param>
        /// <returns> index of the element with largest absolute value </returns>
        public static long IAmx(VectorZ x)
            => factory.iBLAS.IamaxZ(x.Count, x); //IAmxZ(x);

        /// <summary>
        /// finds the index of the element with the largest absolute value
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <returns> index of the element with largest absolute value </returns>
        public static long IAmx(MatrixZ x)
            => factory.iBLAS.IamaxZ(x.Count, x); //IAmxZ(x);

        #endregion
        #region ---- IAmn [D/Z] ----

        /// <summary>
        /// Finds the index of the element with the smallest absolute value in a dense array.
        /// </summary>
        /// <typeparam name="T">
        /// The numeric type of the array elements. Supported types are <see cref="Real"/> (double) and <see cref="Cplx"/> (complex).
        /// </typeparam>
        /// <param name="x">The dense array to search.</param>
        /// <param name="incx">The increment for indexing <paramref name="x"/>. Default is 1.</param>
        /// <returns>
        /// The index of the element with the smallest absolute value in <paramref name="x"/>.
        /// </returns>
        /// <exception cref="NotSupportedException">
        /// Thrown when <typeparamref name="T"/> is not <see cref="Real"/> or <see cref="Cplx"/>.
        /// </exception>
        public static long IAmn<T>(DenseArray<T> x, long incx = 1)
            where T : INumber<T>
        {
            if (typeof(T) == typeof(Real)) return factory.iBLAS.Iamin(x.Count, x.DPtr, incx);
            else if (typeof(T) == typeof(Cplx)) return factory.iBLAS.Iamin(x.Count, x.VPtr, incx);
            else throw new NotSupportedException($"Type {typeof(T)} not supported.");
        }


        ///// <summary>
        ///// finds the index of the element with the smallest absolute value
        ///// </summary>
        ///// <typeparam name="T"> ArrayBase[double] </typeparam>
        ///// <param name="x"> input array x </param>
        ///// <returns> index of the element with smallest absolute value </returns>
        //public static long IAmnD<T>(T x) where T : DenseArrayBase<double>
        //    => factory.iBLAS.IaminD(x.Count, x);

        ///// <summary>
        ///// finds the index of the element with the smallest absolute value
        ///// </summary>
        ///// <typeparam name="T"> ArrayBase[Complex] </typeparam>
        ///// <param name="x"> input array x </param>
        ///// <returns> index of the element with smallest absolute value </returns>
        //public static long IAmnZ<T>(T x) where T : DenseArrayBase<Complex>
        //    => factory.iBLAS.IaminZ(x.Count, x);

        /// <summary>
        /// finds the index of the element with the smallest absolute value
        /// </summary>
        /// <param name="x"> input vector x </param>
        /// <returns> index of the element with smallest absolute value </returns>
        public static long IAmn(VectorD x)
            => factory.iBLAS.IaminD(x.Count, x); //IAmnD(x);

        /// <summary>
        /// finds the index of the element with the smallest absolute value
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <returns> index of the element with smallest absolute value </returns>
        public static long IAmn(MatrixD x)
            => factory.iBLAS.IaminD(x.Count, x); //IAmnD(x);

        /// <summary>
        /// finds the index of the element with the smallest absolute value
        /// </summary>
        /// <param name="x"> input vector x </param>
        /// <returns> index of the element with smallest absolute value </returns>
        public static long IAmn(VectorZ x)
            => factory.iBLAS.IaminZ(x.Count, x); //IAmnZ(x);

        /// <summary>
        /// finds the index of the element with the smallest absolute value
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <returns> index of the element with smallest absolute value </returns>
        public static long IAmn(MatrixZ x)
            => factory.iBLAS.IaminZ(x.Count, x); //IAmnZ(x);

        #endregion
        #region ---- ASum [D/Z] ----

        /// <summary>
        /// Computes the sum of magnitudes of the elements in a dense array.
        /// For real arrays, returns the sum of absolute values: |x[0]| + |x[1]| + ... + |x[n-1]|.
        /// For complex arrays, returns the sum of magnitudes of real and imaginary parts: |Re(x[0])| + |Im(x[0])| + ... + |Re(x[n-1])| + |Im(x[n-1])|.
        /// </summary>
        /// <typeparam name="T">
        /// The numeric type of the array elements. Supported types are <see cref="Real"/> (double) and <see cref="Cplx"/> (complex).
        /// </typeparam>
        /// <param name="x">The dense array whose elements' magnitudes will be summed.</param>
        /// <param name="incx">The increment for indexing <paramref name="x"/>. Default is 1.</param>
        /// <returns>
        /// The sum of magnitudes of the elements in <paramref name="x"/>.
        /// </returns>
        /// <exception cref="NotSupportedException">
        /// Thrown if <typeparamref name="T"/> is not <see cref="Real"/> or <see cref="Cplx"/>.
        /// </exception>
        public static Real ASum<T>(DenseArray<T> x, long incx = 1)
            where T : INumber<T>
        {
            if (typeof(T) == typeof(Real)) return factory.iBLAS.Asum(x.Count, x.DPtr, incx);
            else if (typeof(T) == typeof(Cplx)) return factory.iBLAS.Asum(x.Count, x.VPtr, incx);
            else throw new NotSupportedException($"Type {typeof(T)} not supported.");
        }


        ///// <summary>
        ///// calculates the sum of the absolute values 
        ///// of all elements in x
        ///// </summary>
        ///// <typeparam name="T"> ArrayBase[double] </typeparam>
        ///// <param name="x"> input array x </param>
        ///// <returns> summed absolute values </returns>
        //public static double ASumD<T>(T x) where T : DenseArrayBase<double>
        //    => factory.iBLAS.AsumD(x.Count, x);

        ///// <summary>
        ///// calculates the sum of the absolute values 
        ///// of all elements in x
        ///// </summary>
        ///// <typeparam name="T"> ArrayBase[Complex] </typeparam>
        ///// <param name="x"> input array x </param>
        ///// <returns> summed absolute values </returns>
        //public static double ASumZ<T>(T x) where T : DenseArrayBase<Complex>
        //    => factory.iBLAS.AsumZ(x.Count, x);

        /// <summary>
        /// calculates the sum of the absolute values 
        /// of all elements in x
        /// </summary>
        /// <param name="x"> input vector x </param>
        /// <returns> summed absolute values </returns>
        public static double ASum(VectorD x)
            => factory.iBLAS.AsumD(x.Count, x); //ASumD(x);

        /// <summary>
        /// calculates the sum of the absolute values 
        /// of all elements in x
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <returns> summed absolute values </returns>
        public static double ASum(MatrixD x)
            => factory.iBLAS.AsumD(x.Count, x); //ASumD(x);

        /// <summary>
        /// calculates the sum of the absolute values 
        /// of all elements in x
        /// </summary>
        /// <param name="x"> input vector x </param>
        /// <returns> summed absolute values </returns>
        public static double ASum(VectorZ x)
            => factory.iBLAS.AsumZ(x.Count, x); //ASumZ(x);

        /// <summary>
        /// calculates the sum of the absolute values 
        /// of all elements in x
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <returns> summed absolute values </returns>
        public static double ASum(MatrixZ x)
            => factory.iBLAS.AsumZ(x.Count, x); //ASumZ(x);

        #endregion
        #region ---- Copy [D/Z] ----

        /// <summary>
        /// Copies elements from the source array <paramref name="x"/> to the destination array <paramref name="y"/>.
        /// </summary>
        /// <typeparam name="T">The numeric type of the array elements. Supported types: <see cref="Real"/>, <see cref="Cplx"/>.</typeparam>
        /// <param name="x">The source array to copy from.</param>
        /// <param name="y">The destination array to copy to. Passed by reference and will be modified.</param>
        /// <param name="incx">Increment for indexing elements of <paramref name="x"/>. Default is 1.</param>
        /// <param name="incy">Increment for indexing elements of <paramref name="y"/>. Default is 1.</param>
        /// <exception cref="NotSupportedException">Thrown if <typeparamref name="T"/> is not <see cref="Real"/> or <see cref="Cplx"/>.</exception>
        public static void Copy<T>(DenseArray<T> x, ref DenseArray<T> y,
            long incx = 1, long incy = 1)
            where T : INumber<T>
            => Copy(x, y, incx, incy);

        internal static void Copy<T>(DenseArray<T> x, DenseArray<T> y,
            long incx = 1, long incy = 1)
            where T : INumber<T>
        {
            if (typeof(T) == typeof(Real)) factory.iBLAS.Copy(x.Count, x.DPtr, y.DPtr, incx, incy);
            else if (typeof(T) == typeof(Cplx)) factory.iBLAS.Copy(x.Count, x.VPtr, y.VPtr, incx, incy);
            else throw new NotSupportedException($"Type {typeof(T)} not supported.");
        }


        ///// <summary>
        ///// copies x to y
        ///// </summary>
        ///// <typeparam name="T"> ArrayBase[double] </typeparam>
        ///// <param name="x"> array x </param>
        ///// <param name="y"> array y </param>
        ///// <param name="checkLength"> whether checks the lengths of input arrays </param>
        //public static void CopyD<T>(T x, ref T y,
        //    bool checkLength = true) where T : DenseArrayBase<double>
        //{
        //    if (checkLength) { ExArrayLengthD(x, y); }
        //    factory.iBLAS.CopyD(x.Count, x, ref y);
        //}

        ///// <summary>
        ///// copies x to y
        ///// </summary>
        ///// <typeparam name="T"> ArrayBase[Complex] </typeparam>
        ///// <param name="x"> array x </param>
        ///// <param name="y"> array y </param>
        ///// <param name="checkLength"> whether checks the lengths of input arrays </param>
        //public static void CopyZ<T>(T x, ref T y,
        //    bool checkLength = true) where T : DenseArrayBase<Complex>
        //{
        //    if (checkLength) { ExArrayLengthZ(x, y); }
        //    factory.iBLAS.CopyZ(x.Count, x, ref y);
        //}

        /// <summary>
        /// copies x to y
        /// </summary>
        /// <param name="x"> vector x </param>
        /// <param name="y"> vector y </param>
        /// <param name="checkLength"> whether checks the lengths of input arrays </param>
        public static void Copy(VectorD x, ref VectorD y,
            bool checkLength = true)
            => factory.iBLAS.CopyD(x.Count, x, ref y); //CopyD(x, ref y, checkLength);

        /// <summary>
        /// copies x to y
        /// </summary>
        /// <param name="x"> matrix x </param>
        /// <param name="y"> matrix y </param>
        /// <param name="checkLength"> whether checks the lengths of input arrays </param>
        public static void Copy(MatrixD x, ref MatrixD y,
            bool checkLength = true)
            => factory.iBLAS.CopyD(x.Count, x, ref y); //CopyD(x, ref y, checkLength);

        /// <summary>
        /// copies x to y
        /// </summary>
        /// <param name="x"> vector x </param>
        /// <param name="y"> vector y </param>
        /// <param name="checkLength"> whether checks the lengths of input arrays </param>
        public static void Copy(VectorZ x, ref VectorZ y,
            bool checkLength = true)
            => factory.iBLAS.CopyZ(x.Count, x, ref y); //CopyZ(x, ref y, checkLength);

        /// <summary>
        /// copies x to y
        /// </summary>
        /// <param name="x"> matrix x </param>
        /// <param name="y"> matrix y </param>
        /// <param name="checkLength"> whether checks the lengths of input arrays </param>
        public static void Copy(MatrixZ x, ref MatrixZ y,
            bool checkLength = true)
            => factory.iBLAS.CopyZ(x.Count, x, ref y); //CopyZ(x, ref y, checkLength);

        #endregion
        #region ---- Swap [D/Z] ----

        /// <summary>
        /// Swaps the elements of two dense arrays of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The numeric type of the array elements. Must implement <see cref="INumber{T}"/>.</typeparam>
        /// <param name="x">Reference to the first dense array.</param>
        /// <param name="y">Reference to the second dense array.</param>
        /// <param name="incx">Increment for indexing <paramref name="x"/>. Default is 1.</param>
        /// <param name="incy">Increment for indexing <paramref name="y"/>. Default is 1.</param>
        /// <remarks>
        /// This method swaps the contents of <paramref name="x"/> and <paramref name="y"/> using the specified increments.
        /// </remarks>
        public static void Swap<T>(ref DenseArray<T> x, ref DenseArray<T> y,
            long incx = 1, long incy = 1)
            where T : INumber<T>
            => Swap(x, y, incx, incy);

        internal static void Swap<T>(DenseArray<T> x, DenseArray<T> y,
            long incx = 1, long incy = 1)
            where T : INumber<T>
        {
            if (typeof(T) == typeof(Real)) factory.iBLAS.Swap(x.Count, x.DPtr, y.DPtr, incx, incy);
            else if (typeof(T) == typeof(Cplx)) factory.iBLAS.Swap(x.Count, x.VPtr, y.VPtr, incx, incy);
            else throw new NotSupportedException($"Type {typeof(T)} not supported.");
        }


        ///// <summary>
        ///// given two arrays x and y, returns arrays y and x swapped
        ///// </summary>
        ///// <typeparam name="T"> ArrayBase[double] </typeparam>
        ///// <param name="x"> array x </param>
        ///// <param name="y"> array y </param>
        ///// <param name="checkLength"> whether checks the lengths of input arrays </param>
        //public static void SwapD<T>(ref T x, ref T y,
        //    bool checkLength = true) where T : DenseArrayBase<double>
        //{
        //    if (checkLength) { ExArrayLengthD(x, y); }
        //    factory.iBLAS.SwapD(x.Count, ref x, ref y);
        //}

        ///// <summary>
        ///// given two arrays x and y, returns arrays y and x swapped
        ///// </summary>
        ///// <typeparam name="T"> ArrayBase[Complex] </typeparam>
        ///// <param name="x"> array x </param>
        ///// <param name="y"> array y </param>
        ///// <param name="checkLength"> whether checks the lengths of input arrays </param>
        //public static void SwapZ<T>(ref T x, ref T y,
        //    bool checkLength = true) where T : DenseArrayBase<Complex>
        //{
        //    if (checkLength) { ExArrayLengthZ(x, y); }
        //    factory.iBLAS.SwapZ(x.Count, ref x, ref y);
        //}

        /// <summary>
        /// given two vectors x and y, returns vector y and x swapped
        /// </summary>
        /// <param name="x"> vector x </param>
        /// <param name="y"> vectot y </param>
        /// <param name="checkLength"> whether checks the lengths of input arrays </param>
        public static void Swap(ref VectorD x, ref VectorD y,
            bool checkLength = true)
            => factory.iBLAS.SwapD(x.Count, ref x, ref y); //SwapD(ref x, ref y, checkLength);

        /// <summary>
        /// given two matrices x and y, returns matrix y and x swapped
        /// </summary>
        /// <param name="x"> matrix x </param>
        /// <param name="y"> matrix y </param>
        /// <param name="checkLength"> whether checks the lengths of input arrays </param>
        public static void Swap(ref MatrixD x, ref MatrixD y,
            bool checkLength = true)
            => factory.iBLAS.SwapD(x.Count, ref x, ref y); //SwapD(ref x, ref y, checkLength);

        /// <summary>
        /// given two vectors x and y, returns vector y and x swapped
        /// </summary>
        /// <param name="x"> vector x </param>
        /// <param name="y"> vectot y </param>
        /// <param name="checkLength"> whether checks the lengths of input arrays </param>
        public static void Swap(ref VectorZ x, ref VectorZ y,
            bool checkLength = true)
            => factory.iBLAS.SwapZ(x.Count, ref x, ref y); //SwapZ(ref x, ref y, checkLength);

        /// <summary>
        /// given two matrices x and y, returns matrix y and x swapped
        /// </summary>
        /// <param name="x"> matrix x </param>
        /// <param name="y"> matrix y </param>
        /// <param name="checkLength"> whether checks the lengths of input arrays </param>
        public static void Swap(ref MatrixZ x, ref MatrixZ y,
            bool checkLength = true)
            => factory.iBLAS.SwapZ(x.Count, ref x, ref y); //SwapZ(ref x, ref y, checkLength);

        #endregion
        #region ---- AddTo [D/Z] ----

        /// <summary>
        /// Adds a scaled version of array <paramref name="x"/> to array <paramref name="y"/>.
        /// Performs the operation: y := a * x + y
        /// </summary>
        /// <typeparam name="T">Type of array elements, must implement <see cref="INumber{T}"/>.</typeparam>
        /// <param name="x">Input array to be scaled and added.</param>
        /// <param name="y">Reference to the array to which the scaled <paramref name="x"/> is added.</param>
        /// <param name="a">Scalar multiplier for <paramref name="x"/>. Default is the default value of <typeparamref name="T"/>.</param>
        /// <param name="incx">Increment for indexing <paramref name="x"/>. Default is 1.</param>
        /// <param name="incy">Increment for indexing <paramref name="y"/>. Default is 1.</param>
        public static void AddTo<T>(DenseArray<T> x, ref DenseArray<T> y,
            T a = default!, long incx = 1, long incy = 1)
            where T : INumber<T>
            => AddTo(x, y, a, incx, incy);

        internal static void AddTo<T>(DenseArray<T> x, DenseArray<T> y,
            T a = default!, long incx = 1, long incy = 1)
            where T : INumber<T>
        {
            if (typeof(T) == typeof(Real)) factory.iBLAS.Axpy(x.Count, &a, x.DPtr, y.DPtr, incx, incy);
            else if (typeof(T) == typeof(Cplx)) factory.iBLAS.Axpy(x.Count, &a, x.VPtr, y.VPtr, incx, incy);
            else throw new NotSupportedException($"Type {typeof(T)} not supported.");
        }


        // obsolete ...
        ///// <summary>
        ///// adds scalar multiple of array x to array y
        ///// y := a * x + y
        ///// </summary>
        ///// <typeparam name="T"> ArrayBase[double] </typeparam>
        ///// <param name="x"> array x </param>
        ///// <param name="y"> array y </param>
        ///// <param name="a"> scalar a </param>
        ///// <param name="checkLength"> whether checks the lengths of input arrays </param>
        //public static void AddToD<T>(T x, ref T y,
        //    double a = 1.0, bool checkLength = true) where T : DenseArrayBase<double>
        //{
        //    if (checkLength) { ExArrayLengthD(x, y); }
        //    factory.iBLAS.AxpyD(x.Count, a, x, ref y);
        //}

        ///// <summary>
        ///// adds scalar multiple of array x to array y
        ///// y := x + y
        ///// </summary>
        ///// <typeparam name="T"> ArrayBase[Complex] </typeparam>
        ///// <param name="x"> array x </param>
        ///// <param name="y"> array y </param>
        ///// <param name="aRe"> real-part of complex scalar a </param>
        ///// <param name="aIm"> imaginary-part of complex scalar a </param>
        ///// <param name="checkLength"> whether checks the lengths of input arrays </param>
        //public static void AddToZ<T>(T x, ref T y,
        //    double aRe = 1.0, double aIm = 0.0, 
        //    bool checkLength = true) where T : DenseArrayBase<Complex>
        //{
        //    if (checkLength) { ExArrayLengthZ(x, y); }
        //    Complex a = new(aRe, aIm);
        //    factory.iBLAS.AxpyZ(x.Count, a, x, ref y);
        //}

        /// <summary>
        /// adds vector x to vector y
        /// y := x + y
        /// </summary>
        /// <param name="x"> vector x </param>
        /// <param name="y"> vector y (to be overwritten) </param>
        /// <param name="checkLength"> whether checks the lengths of input arrays </param>
        public static void AddTo(VectorD x, ref VectorD y,
            bool checkLength = true)
            => factory.iBLAS.AxpyD(n: x.Count, a: 1.0, x, ref y);
            //=> AddToD(x, ref y, 1.0, checkLength);

        /// <summary>
        /// adds matrix x to matrix y
        /// y := x + y
        /// </summary>
        /// <param name="x"> matrix x </param>
        /// <param name="y"> matrix y (to be overwritten) </param>
        /// <param name="checkLength"> whether checks the lengths of input arrays </param>
        public static void AddTo(MatrixD x, ref MatrixD y,
            bool checkLength = true)
            => factory.iBLAS.AxpyD(n: x.Count, a: 1.0, x, ref y);
        //=> AddToD(x, ref y, 1.0, checkLength);

        /// <summary>
        /// adds vector x to vector y
        /// y := x + y
        /// </summary>
        /// <param name="x"> vector x </param>
        /// <param name="y"> vector y (to be overwritten) </param>
        /// <param name="checkLength"> whether checks the lengths of input arrays </param>
        public static void AddTo(VectorZ x, ref VectorZ y,
            bool checkLength = true)
            => factory.iBLAS.AxpyZ(n: x.Count, a: 1.0, x, ref y);
            //=> AddToZ(x, ref y, 1.0, 0.0, checkLength);

        /// <summary>
        /// adds matrix x to matrix y
        /// y := x + y
        /// </summary>
        /// <param name="x"> matrix x </param>
        /// <param name="y"> matrix y (to be overwritten) </param>
        /// <param name="checkLength"> whether checks the lengths of input arrays </param>
        public static void AddTo(MatrixZ x, ref MatrixZ y,
            bool checkLength = true)
            => factory.iBLAS.AxpyZ(n: x.Count, a: 1.0, x, ref y);
        //=> AddToZ(x, ref y, 1.0, 0.0, checkLength);

        #endregion
        #region ---- ScaleOn [D/Z] ----

        /// <summary>
        /// Scales the elements of a dense array <paramref name="x"/> by a scalar value <paramref name="a"/>.
        /// </summary>
        /// <typeparam name="T">The numeric type of the array elements, implementing <see cref="INumber{T}"/>.</typeparam>
        /// <param name="a">The scalar multiplier.</param>
        /// <param name="x">The dense array to be scaled (passed by reference).</param>
        /// <param name="incx">The increment for indexing elements of <paramref name="x"/>. Default is 1.</param>
        public static void ScaleOn<T>(T a, ref DenseArray<T> x,
            long incx = 1)
            where T : INumber<T>
            => ScaleOn(a, x, incx);

        internal static void ScaleOn<T>(T a, DenseArray<T> x,
            long incx = 1)
            where T : INumber<T>
        {
            if (typeof(T) == typeof(Real)) factory.iBLAS.Scal(x.Count, &a, x.DPtr, incx);
            else if (typeof(T) == typeof(Cplx)) factory.iBLAS.Scal(x.Count, &a, x.VPtr, incx);
            else throw new NotSupportedException($"Type {typeof(T)} not supported.");
        }


        // obsolete ...
        ///// <summary>
        ///// scales array x by constant a
        ///// x := a * x
        ///// </summary>
        ///// <typeparam name="T"> ArrayBase[double] </typeparam>
        ///// <param name="x"> array x </param>
        ///// <param name="a"> constant a </param>
        //public static void ScaleOnD<T>(ref T x, double a) 
        //    where T : DenseArrayBase<double>
        //    => factory.iBLAS.ScalD(x.Count, a, ref x);

        ///// <summary>
        ///// scales array x by constant a
        ///// x := a * x
        ///// </summary>
        ///// <typeparam name="T"> ArrayBase[Complex] </typeparam>
        ///// <param name="x"> array x </param>
        ///// <param name="a"> constant a </param>
        //public static void ScaleOnZ<T>(ref T x, Complex a) 
        //    where T : DenseArrayBase<Complex>
        //    => factory.iBLAS.ScalZ(x.Count, a, ref x);

        ///// <summary>
        ///// scales array x by constant a
        ///// x := a * x
        ///// </summary>
        ///// <typeparam name="T"> ArrayBase[Complex] </typeparam>
        ///// <param name="x"> array x </param>
        ///// <param name="a"> constant a </param>
        //public static void ScaleOnZ<T>(ref T x, double a)
        //    where T : DenseArrayBase<Complex>
        //    => factory.iBLAS.ScalZd(x.Count, a, ref x);

        /// <summary>
        /// scales vector x by constant a
        /// x := a * x
        /// </summary>
        /// <param name="x"> vector x </param>
        /// <param name="a"> constant a </param>
        public static void ScaleOn(ref VectorD x, double a)
            => factory.iBLAS.ScalD(x.Count, a, ref x); //ScaleOnD(ref x, a);

        /// <summary>
        /// scales matrix by constant a
        /// x := a * x
        /// </summary>
        /// <param name="x"> matrix x </param>
        /// <param name="a"> constant a </param>
        public static void ScaleOn(ref MatrixD x, double a)
            => factory.iBLAS.ScalD(x.Count, a, ref x); //ScaleOnD(ref x, a);

        /// <summary>
        /// scales vector x by constant a
        /// x := a * x
        /// </summary>
        /// <param name="x"> vector x </param>
        /// <param name="a"> constant a </param>
        public static void ScaleOn(ref VectorZ x, Complex a)
            => factory.iBLAS.ScalZ(x.Count, a, ref x); //ScaleOnZ(ref x, a);

        /// <summary>
        /// scales a matrix by constant a
        /// x := a * x
        /// </summary>
        /// <param name="x"> matrix x </param>
        /// <param name="a"> constant a </param>
        public static void ScaleOn(ref MatrixZ x, Complex a)
            => factory.iBLAS.ScalZ(x.Count, a, ref x); //ScaleOnZ(ref x, a);

        #endregion
        #region ---- DScaleOn ----

        /// <summary>
        /// Scales a dense array of complex values by a real scalar value in-place.
        /// x[i] := a * x[i]
        /// </summary>
        /// <param name="a">The real scalar multiplier.</param>
        /// <param name="x">Reference to the dense array of complex values to be scaled.</param>
        /// <param name="incx">The increment for the elements of <paramref name="x"/> (default is 1).</param>
        public static void DScaleOn(Real a, ref DenseArray<Cplx> x,
            long incx = 1)
            => factory.iBLAS.Scal(x.Count, a, x.VPtr, incx);


        /// <summary>
        /// scales vector x by constant a
        /// x := a * x
        /// </summary>
        /// <param name="x"> vector x </param>
        /// <param name="a"> constant a </param>
        public static void DScaleOn(ref VectorZ x, double a)
            => factory.iBLAS.ScalZd(x.Count, a, ref x); //ScaleOnZ(ref x, a);

        /// <summary>
        /// scales a matrix by constant a
        /// x := a * x
        /// </summary>
        /// <param name="x"> matrix x </param>
        /// <param name="a"> constant a </param>
        public static void DScaleOn(ref MatrixZ x, double a)
            => factory.iBLAS.ScalZd(x.Count, a, ref x); //ScaleOnZ(ref x, a);

        #endregion
        #region ---- Norm [D/Z] ----

        /// <summary>
        /// Computes the Euclidean norm (L2 norm) of a dense array of numeric values.
        /// </summary>
        /// <typeparam name="T">
        /// The numeric type of the array elements. Supported types are <see cref="Real"/> (double) and <see cref="Cplx"/> (complex).
        /// </typeparam>
        /// <param name="x">The dense array whose norm is to be computed.</param>
        /// <param name="incx">The increment for indexing elements in <paramref name="x"/>. Default is 1.</param>
        /// <returns>
        /// The Euclidean norm (L2 norm) of the array.
        /// </returns>
        /// <exception cref="NotSupportedException">
        /// Thrown if <typeparamref name="T"/> is not <see cref="Real"/> or <see cref="Cplx"/>.
        /// </exception>
        public static Real Norm<T>(DenseArray<T> x,
            long incx = 1)
            where T : INumber<T>
        {
            if (typeof(T) == typeof(Real)) return factory.iBLAS.Nrm2(x.Count, x.DPtr, incx);
            else if (typeof(T) == typeof(Cplx)) return factory.iBLAS.Nrm2(x.Count, x.VPtr, incx);
            else throw new NotSupportedException($"Type {typeof(T)} not supported.");
        }


        // obsolete ...
        ///// <summary>
        ///// computes the Euclidean norm of an array
        ///// </summary>
        ///// <typeparam name="T"> ArrayBase[double] </typeparam>
        ///// <param name="x"> array x </param>
        ///// <returns> Euclidean norm </returns>
        //public static double NormD<T>(T x) where T : DenseArrayBase<double>
        //    => factory.iBLAS.Nrm2D(x.Count, x);

        ///// <summary>
        ///// computes the Euclidean norm of an array
        ///// </summary>
        ///// <typeparam name="T"> ArrayBase[Complex] </typeparam>
        ///// <param name="x"> array x </param>
        ///// <returns> Euclidean norm </returns>
        //public static double NormZ<T>(T x) where T : DenseArrayBase<Complex>
        //    => factory.iBLAS.Nrm2Z(x.Count, x);

        /// <summary>
        /// computes the Euclidean norm of a vector
        /// </summary>
        /// <param name="x"> vector x </param>
        /// <returns> Euclidean norm </returns>
        public static double Norm(VectorD x)
            => factory.iBLAS.Nrm2D(x.Count, x); //NormD(x);

        /// <summary>
        /// computes the Euclidean norm of a matrix
        /// </summary>
        /// <param name="x"> matrix x </param>
        /// <returns> Euclidean norm </returns>
        public static double Norm(MatrixD x)
            => factory.iBLAS.Nrm2D(x.Count, x); //NormD(x);

        /// <summary>
        /// computes the Euclidean norm of a vector
        /// </summary>
        /// <param name="x"> vector x </param>
        /// <returns> Euclidean norm </returns>
        public static double Norm(VectorZ x)
            => factory.iBLAS.Nrm2Z(x.Count, x); //NormZ(x);

        /// <summary>
        /// computes the Euclidean norm of a matrix
        /// </summary>
        /// <param name="x"> matrix x </param>
        /// <returns> Euclidean norm </returns>
        public static double Norm(MatrixZ x)
            => factory.iBLAS.Nrm2Z(x.Count, x); //NormZ(x);

        #endregion
        #region ---- Rot [D/Z] ----

        /// <summary>
        /// Performs rotation of points in the plane for two dense arrays.
        /// Each element is updated as:
        /// <code>
        /// xi = c * xi + s * yi
        /// yi = c * yi - s * xi
        /// </code>
        /// </summary>
        /// <typeparam name="T">Type of the array elements, must implement <see cref="INumber{T}"/>.</typeparam>
        /// <param name="x">Reference to the first dense array.</param>
        /// <param name="y">Reference to the second dense array.</param>
        /// <param name="c">Cosine of the rotation angle.</param>
        /// <param name="s">Sine of the rotation angle.</param>
        /// <param name="incx">Increment for indexing <paramref name="x"/>. Default is 1.</param>
        /// <param name="incy">Increment for indexing <paramref name="y"/>. Default is 1.</param>
        public static void Rot<T>(ref DenseArray<T> x, ref DenseArray<T> y,
            double c, double s, long incx = 1, long incy = 1)
            where T : INumber<T>
            => Rot(x, y, c, s, incx, incy);

        internal static void Rot<T>(DenseArray<T> x, DenseArray<T> y,
            double c, double s, long incx = 1, long incy = 1)
            where T : INumber<T>
        {
            if (typeof(T) == typeof(Real)) factory.iBLAS.Rot(x.Count, x.DPtr, y.DPtr, c, s, incx, incy);
            else if (typeof(T) == typeof(Cplx)) factory.iBLAS.Rot(x.Count, x.VPtr, y.VPtr, c, s, incx, incy);
            else throw new NotSupportedException($"Type {typeof(T)} not supported.");
        }


        // obsolete ...
        ///// <summary>
        ///// performs rotation of points in the plane
        ///// xi = c*xi + s*yi
        ///// yi = c*yi - s*xi
        ///// </summary>
        ///// <typeparam name="T"> ArrayBase[double] </typeparam>
        ///// <param name="x"> array x (replaced by c*x+s*y) </param>
        ///// <param name="y"> array y (replaced by c*y-s*x)</param>
        ///// <param name="c"> constant c </param>
        ///// <param name="s"> constant s </param>
        ///// <param name="checkLength"> whether checks the lengths of input arrays </param>
        //public static void RotD<T>(ref T x, ref T y, double c, double s,
        //    bool checkLength = true)
        //    where T : DenseArrayBase<double>
        //{
        //    if (checkLength) { ExArrayLengthD(x, y); }
        //    factory.iBLAS.RotD(x.Count, ref x, ref y, c, s);
        //}

        ///// <summary>
        ///// performs rotation of points in the plane
        ///// xi = c*xi + s*yi
        ///// yi = c*yi - s*xi
        ///// </summary>
        ///// <typeparam name="T"> ArrayBase[Complex] </typeparam>
        ///// <param name="x"> array x (replaced by c*x+s*y) </param>
        ///// <param name="y"> array y (replaced by c*y-s*x)</param>
        ///// <param name="c"> constant c </param>
        ///// <param name="s"> constant s </param>
        ///// <param name="checkLength"> whether checks the lengths of input arrays </param>
        //public static void RotZ<T>(ref T x, ref T y, double c, double s,
        //    bool checkLength = true)
        //    where T : DenseArrayBase<Complex>
        //{
        //    if (checkLength) { ExArrayLengthZ(x, y); }
        //    factory.iBLAS.RotZ(x.Count, ref x, ref y, c, s);
        //}

        /// <summary>
        /// performs rotation of points in the plane
        /// xi = c*xi + s*yi
        /// yi = c*yi - s*xi
        /// </summary>
        /// <param name="x"> vector x (replaced by c*x+s*y) </param>
        /// <param name="y"> vector y (replaced by c*y-s*x)</param>
        /// <param name="c"> constant c </param>
        /// <param name="s"> constant s </param>
        /// <param name="checkLength"> whether checks the lengths of input arrays </param>
        public static void Rotation(ref VectorD x, ref VectorD y,
            double c, double s, bool checkLength = true)
            => factory.iBLAS.RotD(x.Count, ref x, ref y, c, s); //RotD(ref x, ref y, c, s, checkLength);

        /// <summary>
        /// performs rotation of points in the plane
        /// xi = c*xi + s*yi
        /// yi = c*yi - s*xi
        /// </summary>
        /// <param name="x"> matrix x (replaced by c*x+s*y) </param>
        /// <param name="y"> matrix y (replaced by c*y-s*x)</param>
        /// <param name="c"> constant c </param>
        /// <param name="s"> constant s </param>
        /// <param name="checkLength"> whether checks the lengths of input arrays </param>
        public static void Rotation(ref MatrixD x, ref MatrixD y,
            double c, double s, bool checkLength = true)
            => factory.iBLAS.RotD(x.Count, ref x, ref y, c, s); //RotD(ref x, ref y, c, s, checkLength);

        /// <summary>
        /// performs rotation of points in the plane
        /// xi = c*xi + s*yi
        /// yi = c*yi - s*xi
        /// </summary>
        /// <param name="x"> vector x (replaced by c*x+s*y) </param>
        /// <param name="y"> vector y (replaced by c*y-s*x)</param>
        /// <param name="c"> constant c </param>
        /// <param name="s"> constant s </param>
        /// <param name="checkLength"> whether checks the lengths of input arrays </param>
        public static void Rotation(ref VectorZ x, ref VectorZ y,
            double c, double s, bool checkLength = true)
            => factory.iBLAS.RotZ(x.Count, ref x, ref y, c, s); //RotZ(ref x, ref y, c, s, checkLength);

        /// <summary>
        /// performs rotation of points in the plane
        /// xi = c*xi + s*yi
        /// yi = c*yi - s*xi
        /// </summary>
        /// <param name="x"> matrix x (replaced by c*x+s*y) </param>
        /// <param name="y"> matrix y (replaced by c*y-s*x)</param>
        /// <param name="c"> constant c </param>
        /// <param name="s"> constant s </param>
        /// <param name="checkLength"> whether checks the lengths of input arrays </param>
        public static void Rotation(ref MatrixZ x, ref MatrixZ y,
            double c, double s, bool checkLength = true)
            => factory.iBLAS.RotZ(x.Count, ref x, ref y, c, s); //RotZ(ref x, ref y, c, s, checkLength);

        #endregion

        #endregion
        #region BLAS-Extensions

        #region ---- Scale [D/Z] ----

        /// <summary>
        /// scales a vector x by constant a
        /// y = a * x
        /// </summary>
        /// <param name="x"> vector x </param>
        /// <param name="a"> constant a </param>
        /// <returns> result vector </returns>
        [Obsolete]
        public static VectorD Scale(VectorD x, double a)
        {
            VectorD y = new(count: x.Count, mode: ArrayInitMode.Malloc);
            Copy(x, ref y);
            ScaleOn(ref y, a);
            return y;
        }

        /// <summary>
        /// scales a matrix x by constant a
        /// y = a * x
        /// </summary>
        /// <param name="x"> matrix x </param>
        /// <param name="a"> constant a </param>
        /// <returns> result matrix </returns>
        [Obsolete]
        public static MatrixD Scale(MatrixD x, double a)
        {
            MatrixD y = new(rows: x.Rows, cols: x.Cols, mode: ArrayInitMode.Malloc);
            Copy(x, ref y);
            ScaleOn(ref y, a);
            return y;
        }

        /// <summary>
        /// scales a vector x by constant a
        /// y = a * x
        /// </summary>
        /// <param name="x"> vector x </param>
        /// <param name="a"> constant a </param>
        /// <returns> result vector </returns>
        [Obsolete]
        public static VectorZ Scale(VectorZ x, Complex a)
        {
            VectorZ y = new(count: x.Count, mode: ArrayInitMode.Malloc);
            Copy(x, ref y);
            ScaleOn(ref y, a);
            return y;
        }

        /// <summary>
        /// scales a matrix x by constant a
        /// y = a * x
        /// </summary>
        /// <param name="x"> matrix x </param>
        /// <param name="a"> constant a </param>
        /// <returns> result matrix </returns>
        [Obsolete]
        public static MatrixZ Scale(MatrixZ x, Complex a)
        {
            MatrixZ y = new(rows: x.Rows, cols: x.Cols, mode: ArrayInitMode.Malloc);
            Copy(x, ref y);
            ScaleOn(ref y, a);
            return y;
        }

        #endregion

        // ...

        #endregion

        #region VMF standard methods

        #region ---- Abs [D/Z] ----
        internal static void Abs<T>(DenseArray<T> x, DenseArray<Real> y)
    where T : INumber<T>
        {
            if (typeof(T) == typeof(Real)) factory.iVMF.Abs(x.Count, x.DPtr, y.DPtr);
            else if (typeof(T) == typeof(Cplx)) factory.iVMF.Abs(x.Count, x.VPtr, y.DPtr);
            else throw new NotSupportedException($"Type {typeof(T)} not supported.");
        }

        /// <summary>
        /// Computes the absolute value of each element in the input array <paramref name="x"/> and stores the result in <paramref name="y"/>.
        /// </summary>
        /// <typeparam name="T">A type that implements <see cref="INumber{T}"/> and represents the input array element type.</typeparam>
        /// <param name="x">The input array containing elements to compute the absolute value for.</param>
        /// <param name="y">The output array where the absolute values will be stored.</param>
        public static void Abs<T>(DenseArray<T> x, ref DenseArray<Real> y)
            where T : INumber<T>
            => Abs(x, y);

        private static Vect<Real> Abs<T>(Vect<T> x)
            where T : INumber<T>
        {
            Vect<Real> y = new(count: x.Count, initMode: ArrayInitMode.Malloc);
            Abs(x, y);
            return y;
        }
        private static Matx<Real> Abs<T>(Matx<T> x)
            where T : INumber<T>
        {
            Matx<Real> y = new(rows: x.Rows, cols: x.Cols, initMode: ArrayInitMode.Malloc);
            Abs(x, y);
            return y;
        }


        // obsolete ...
        ///// <summary>
        ///// computes absolute value of array elements
        ///// yi = |xi|
        ///// </summary>
        ///// <typeparam name="T"> ArrayBase[double] </typeparam>
        ///// <param name="x"> array x </param>
        ///// <param name="y"> array y </param>
        ///// <param name="checkLength"> whether checks the lengths of input arrays </param>
        //public static void AbsD<T>(T x, ref T y,
        //    bool checkLength = true) where T : DenseArrayBase<double>
        //{
        //    if (checkLength) { ExArrayLengthD(x, y); }
        //    factory.iVMF.AbsD(x.Count, x, ref y);
        //}

        ///// <summary>
        ///// computes absolute value of array elements
        ///// yi = |xi|
        ///// </summary>
        ///// <typeparam name="T1"> ArrayBase[Complex] </typeparam>
        ///// <typeparam name="T2"> ArrayBase[double] </typeparam>        
        ///// <param name="x"> array x </param>
        ///// <param name="y"> array y </param>
        ///// <param name="checkLength"> whether checks the lengths of input arrays </param>
        //public static void AbsZ<T1, T2>(T1 x, ref T2 y,
        //    bool checkLength = true)
        //    where T1 : DenseArrayBase<Complex>
        //    where T2 : DenseArrayBase<double>
        //{
        //    if (checkLength && (x.Count != y.Count))
        //    { Printer.Warning($"Array lengths not equal"); }
        //    factory.iVMF.AbsZ(x.Count, x, ref y);
        //}

        /// <summary>
        /// computes absolute value of vector elements
        /// </summary>
        /// <param name="x"> input vector x </param>
        /// <returns> result vector </returns>
        public static VectorD Abs(VectorD x)
        {
            VectorD y = new(count: x.Count, mode: ArrayInitMode.Malloc);
            factory.iVMF.AbsD(x.Count, x, ref y);
            //AbsD(x, ref y, checkLength: false);
            factory.iVMF.AbsD(x.Count, x, ref y);
            return y;
        }

        /// <summary>
        /// computes absolute value of matrix elements
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <returns> result matrix </returns>
        public static MatrixD Abs(MatrixD x)
        {
            MatrixD y = new(rows: x.Rows, cols: x.Cols, mode: ArrayInitMode.Malloc);
            //AbsD(x, ref y, checkLength: false);
            factory.iVMF.AbsD(x.Count, x, ref y);
            return y;
        }

        /// <summary>
        /// computes absolute value of vector elements
        /// </summary>
        /// <param name="x"> input vector x </param>
        /// <returns> result vector </returns>
        public static VectorD Abs(VectorZ x)
        {
            VectorD y = new(count: x.Count, mode: ArrayInitMode.Malloc);
            //AbsZ(x, ref y, checkLength: false);
            factory.iVMF.AbsZ(x.Count, x, ref y);
            return y;
        }

        /// <summary>
        /// computes absolute value of matrix elements
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <returns> result matrix </returns>
        public static MatrixD Abs(MatrixZ x)
        {
            MatrixD y = new(rows: x.Rows, cols: x.Cols, mode: ArrayInitMode.Malloc);
            //AbsZ(x, ref y, checkLength: false);
            factory.iVMF.AbsZ(x.Count, x, ref y);
            return y;
        }

        #endregion
        #region ---- Arg [Z] ----

        /// <summary>
        /// Computes the argument (phase angle) of each element in a complex dense array.
        /// yi = arg[xi]
        /// </summary>
        /// <param name="x">The input dense array of complex numbers.</param>
        /// <param name="y">The output dense array to store the arguments (phase angles). Must be pre-allocated and have the same length as <paramref name="x"/>.</param>
        public static void Arg(DenseArray<Cplx> x, ref DenseArray<Real> y)
            => Arg(x, y);

        internal static void Arg(DenseArray<Cplx> x, DenseArray<Real> y)
            => factory.iVMF.Arg(x.Count, x.VPtr, y.DPtr);


        // obsolete ...
        ///// <summary>
        ///// computes argument of a complex array's elements
        ///// yi = arg[xi]
        ///// </summary>
        ///// <typeparam name="T1"> ArrayBase[Complex] </typeparam>
        ///// <typeparam name="T2"> ArrayBase[double] </typeparam>
        ///// <param name="x"> array x </param>
        ///// <param name="y"> array y </param>
        ///// <param name="checkLength"> whether checks the lengths of input arrays </param>
        //public static void ArgZ<T1, T2>(T1 x, ref T2 y,
        //    bool checkLength = true)
        //    where T1 : DenseArrayBase<Complex>
        //    where T2 : DenseArrayBase<double>
        //{
        //    if (checkLength && (x.Count != y.Count))
        //    { Printer.Warning($"Array lengths not equal"); }
        //    factory.iVMF.ArgZ(x.Count, x, ref y);
        //}

        /// <summary>
        /// computes argument of a complex vector's elements
        /// </summary>
        /// <param name="x"> input vector x </param>
        /// <returns> result vector </returns>
        public static VectorD Arg(VectorZ x)
        {
            VectorD y = new(count: x.Count, mode: ArrayInitMode.Malloc);
            //ArgZ(x, ref y, checkLength: false);
            factory.iVMF.ArgZ(x.Count, x, ref y);
            return y;
        }

        /// <summary>
        /// computes argument of a complex matrix's elements
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <returns> result matrix </returns>
        public static MatrixD Arg(MatrixZ x)
        {
            MatrixD y = new(rows: x.Rows, cols: x.Cols, mode: ArrayInitMode.Malloc);
            //ArgZ(x, ref y, checkLength: false);
            factory.iVMF.ArgZ(x.Count, x, ref y);
            return y;
        }

        #endregion
        #region ---- Add [D/Z] ----

        /// <summary>
        /// Performs element-wise addition of two dense arrays and stores the result in a third array.
        /// </summary>
        /// <typeparam name="T">The numeric type of the array elements, implementing <see cref="INumber{T}"/>.</typeparam>
        /// <param name="x">The first input dense array.</param>
        /// <param name="y">The second input dense array.</param>
        /// <param name="z">The dense array to store the result of the addition.</param>
        public static void Add<T>(DenseArray<T> x, DenseArray<T> y,
            ref DenseArray<T> z)
            where T : INumber<T>
            => Add(x, y, z);

        internal static void Add<T>(DenseArray<T> x, DenseArray<T> y,
            DenseArray<T> z)
            where T : INumber<T>
        {
            if (typeof(T) == typeof(Real)) factory.iVMF.Add(x.Count, x.DPtr, y.DPtr, z.DPtr);
            else if (typeof(T) == typeof(Cplx)) factory.iVMF.Add(x.Count, x.VPtr, y.VPtr, z.VPtr);
            else throw new NotSupportedException($"Type {typeof(T)} not supported.");
        }


        // obsolete ...
        ///// <summary>
        ///// adds up two arrays x and y
        ///// result z = x + y
        ///// </summary>
        ///// <typeparam name="T"> ArrayBase[double] </typeparam>
        ///// <param name="x"> array x </param>
        ///// <param name="y"> array y </param>
        ///// <param name="z"> array z </param>
        ///// <param name="checkLength"> whether checks the lengths of input arrays </param>
        //public static void AddD<T>(T x, T y, ref T z,
        //    bool checkLength = true)
        //    where T : DenseArrayBase<double>
        //{
        //    if (checkLength) { ExArrayLengthD(x, y); }
        //    if (checkLength) { ExArrayLengthD(x, z); }
        //    factory.iVMF.AddD(x.Count, x, y, ref z);
        //}

        ///// <summary>
        ///// adds up two arrays x and y
        ///// result z = x + y
        ///// </summary>
        ///// <typeparam name="T"> ArrayBase[Complex] </typeparam>
        ///// <param name="x"> array x </param>
        ///// <param name="y"> array y </param>
        ///// <param name="z"> array z </param>
        ///// <param name="checkLength"> whether checks the lengths of input arrays </param>
        //public static void AddZ<T>(T x, T y, ref T z,
        //    bool checkLength = true)
        //    where T : DenseArrayBase<Complex>
        //{
        //    if (checkLength) { ExArrayLengthZ(x, y); }
        //    if (checkLength) { ExArrayLengthZ(x, z); }
        //    factory.iVMF.AddZ(x.Count, x, y, ref z);
        //}

        /// <summary>
        /// adds up two vectors x and y
        /// result = x + y
        /// </summary>
        /// <param name="x"> input vector x </param>
        /// <param name="y"> input vector y </param>
        /// <param name="checkLength"> whether checks the lengths of input arrays </param>
        /// <returns> result vector </returns>
        public static VectorD Add(VectorD x, VectorD y,
            bool checkLength = true)
        {
            if (checkLength) { ExArrayLengthD(x, y); }
            VectorD z = new(count: x.Count, mode: ArrayInitMode.Malloc);
            //AddD(x, y, ref z, checkLength: false);
            factory.iVMF.AddD(x.Count, x, y, ref z);
            return z;
        }

        /// <summary>
        /// adds up two matrices x and y
        /// result = x + y
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <param name="y"> input matrix y </param>
        /// <param name="checkLength"> whether checks the lengths of input arrays </param>
        /// <returns> result matrix </returns>
        public static MatrixD Add(MatrixD x, MatrixD y,
            bool checkLength = true)
        {
            if (checkLength) { ExArrayLengthD(x, y); }
            MatrixD z = new(rows: x.Rows, cols: x.Cols, mode: ArrayInitMode.Malloc);
            //AddD(x, y, ref z, checkLength: false);
            factory.iVMF.AddD(x.Count, x, y, ref z);
            return z;
        }

        /// <summary>
        /// adds up two vectors x and y
        /// result = x + y
        /// </summary>
        /// <param name="x"> input vector x </param>
        /// <param name="y"> input vector y </param>
        /// <param name="checkLength"> whether checks the lengths of input arrays </param>
        /// <returns> result vector </returns>
        public static VectorZ Add(VectorZ x, VectorZ y,
            bool checkLength = true)
        {
            if (checkLength) { ExArrayLengthZ(x, y); }
            VectorZ z = new(count: x.Count, mode: ArrayInitMode.Malloc);
            //AddZ(x, y, ref z, checkLength: false);
            factory.iVMF.AddZ(x.Count, x, y, ref z);
            return z;
        }

        /// <summary>
        /// adds up two matrices x and y
        /// result = x + y
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <param name="y"> input matrix y </param>
        /// <param name="checkLength"> whether checks the lengths of input arrays </param>
        /// <returns> result matrix </returns>
        public static MatrixZ Add(MatrixZ x, MatrixZ y,
            bool checkLength = true)
        {
            if (checkLength) { ExArrayLengthZ(x, y); }
            MatrixZ z = new(rows: x.Rows, cols: x.Cols, mode: ArrayInitMode.Malloc);
            //AddZ(x, y, ref z, checkLength: false);
            factory.iVMF.AddZ(x.Count, x, y, ref z);
            return z;
        }

        #endregion
        #region ---- Sub [D/Z] ----

        /// <summary>
        /// Performs element-wise subtraction of two dense arrays.
        /// Each element of <paramref name="y"/> is subtracted from the corresponding element of <paramref name="x"/>,
        /// and the result is stored in <paramref name="z"/>.
        /// </summary>
        /// <typeparam name="T">A numeric type implementing <see cref="INumber{T}"/>.</typeparam>
        /// <param name="x">The first input dense array.</param>
        /// <param name="y">The second input dense array to subtract from <paramref name="x"/>.</param>
        /// <param name="z">The dense array to store the result of the subtraction.</param>
        public static void Sub<T>(DenseArray<T> x, DenseArray<T> y,
            ref DenseArray<T> z)
            where T : INumber<T>
            => Sub(x, y, z);

        internal static void Sub<T>(DenseArray<T> x, DenseArray<T> y,
            DenseArray<T> z)
            where T : INumber<T>
        {
            if (typeof(T) == typeof(Real)) factory.iVMF.Sub(x.Count, x.DPtr, y.DPtr, z.DPtr);
            else if (typeof(T) == typeof(Cplx)) factory.iVMF.Sub(x.Count, x.VPtr, y.VPtr, z.VPtr);
            else throw new NotSupportedException($"Type {typeof(T)} not supported.");
        }


        // obsolete ...
        ///// <summary>
        ///// subtracts two arrays x and y
        ///// result z = x - y
        ///// </summary>
        ///// <typeparam name="T"> ArrayBase[double] </typeparam>
        ///// <param name="x"> array x </param>
        ///// <param name="y"> array y </param>
        ///// <param name="z"> array z </param>
        ///// <param name="checkLength"> whether checks the lengths of input arrays </param>
        //public static void SubD<T>(T x, T y, ref T z,
        //    bool checkLength = true)
        //    where T : DenseArrayBase<double>
        //{
        //    if (checkLength) { ExArrayLengthD(x, y); }
        //    if (checkLength) { ExArrayLengthD(x, z); }
        //    factory.iVMF.SubD(x.Count, x, y, ref z);
        //}

        ///// <summary>
        ///// subtracts two arrays x and y
        ///// result z = x - y
        ///// </summary>
        ///// <typeparam name="T"> ArrayBase[Complex] </typeparam>
        ///// <param name="x"> array x </param>
        ///// <param name="y"> array y </param>
        ///// <param name="z"> array z </param>
        ///// <param name="checkLength"> whether checks the lengths of input arrays </param>
        //public static void SubZ<T>(T x, T y, ref T z,
        //    bool checkLength = true)
        //    where T : DenseArrayBase<Complex>
        //{
        //    if (checkLength) { ExArrayLengthZ(x, y); }
        //    if (checkLength) { ExArrayLengthZ(x, z); }
        //    factory.iVMF.SubZ(x.Count, x, y, ref z);
        //}

        /// <summary>
        /// subtracts two vectors x and y
        /// result = x - y
        /// </summary>
        /// <param name="x"> input vector x </param>
        /// <param name="y"> input vector y </param>
        /// <param name="checkLength"> whether checks the lengths of input arrays </param>
        /// <returns> result vector </returns>
        public static VectorD Sub(VectorD x, VectorD y,
            bool checkLength = true)
        {
            if (checkLength) { ExArrayLengthD(x, y); }
            VectorD z = new(count: x.Count, mode: ArrayInitMode.Malloc);
            //SubD(x, y, ref z, checkLength: false);
            factory.iVMF.SubD(x.Count, x, y, ref z);
            return z;
        }

        /// <summary>
        /// subtracts two matrices x and y
        /// result = x - y
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <param name="y"> input matrix y </param>
        /// <param name="checkLength"> whether checks the lengths of input arrays </param>
        /// <returns> result matrix </returns>
        public static MatrixD Sub(MatrixD x, MatrixD y,
            bool checkLength = true)
        {
            if (checkLength) { ExArrayLengthD(x, y); }
            MatrixD z = new(rows: x.Rows, cols: x.Cols, mode: ArrayInitMode.Malloc);
            //SubD(x, y, ref z, checkLength: false);
            factory.iVMF.SubD(x.Count, x, y, ref z);
            return z;
        }

        /// <summary>
        /// subtracts two vectors x and y
        /// result = x - y
        /// </summary>
        /// <param name="x"> input vector x </param>
        /// <param name="y"> input vector y </param>
        /// <param name="checkLength"> whether checks the lengths of input arrays </param>
        /// <returns> result vector </returns>
        public static VectorZ Sub(VectorZ x, VectorZ y,
            bool checkLength = true)
        {
            if (checkLength) { ExArrayLengthZ(x, y); }
            VectorZ z = new(count: x.Count, mode: ArrayInitMode.Malloc);
            //SubZ(x, y, ref z, checkLength: false); 
            factory.iVMF.SubZ(x.Count, x, y, ref z);
            return z;
        }

        /// <summary>
        /// subtracts two matrices x and y
        /// result = x - y
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <param name="y"> input matrix y </param>
        /// <param name="checkLength"> whether checks the lengths of input arrays </param>
        /// <returns> result matrix </returns>
        public static MatrixZ Sub(MatrixZ x, MatrixZ y,
            bool checkLength = true)
        {
            if (checkLength) { ExArrayLengthZ(x, y); }
            MatrixZ z = new(rows: x.Rows, cols: x.Cols, mode: ArrayInitMode.Malloc);
            //SubZ(x, y, ref z, checkLength: false);
            factory.iVMF.SubZ(x.Count, x, y, ref z);
            return z;
        }

        #endregion
        #region ---- Inv [D/Z?] ----

        /// <summary>
        /// Computes the element-wise inverse of the input array <paramref name="x"/> and stores the result in <paramref name="y"/>.
        /// Each element is calculated as y[i] = 1 / x[i].
        /// </summary>
        /// <typeparam name="T">A dense array type implementing <see cref="INumber{T}"/>.</typeparam>
        /// <param name="x">The input array whose elements will be inverted.</param>
        /// <param name="y">The output array where the inverted elements will be stored.</param>
        public static void Inv<T>(DenseArray<T> x, ref DenseArray<T> y)
            where T : INumber<T>
            => Inv(x, y);

        internal static void Inv<T>(DenseArray<T> x, DenseArray<T> y)
            where T : INumber<T>
        {
            if (typeof(T) == typeof(Real)) factory.iVMF.Inv(x.Count, x.DPtr, y.DPtr);
            else if (typeof(T) == typeof(Cplx))
            {
                Cplx negOne = -Cplx.One;
                factory.iVMF.Powx(x.Count, x.VPtr, &negOne, y.VPtr);
            }
            else throw new NotSupportedException($"Type {typeof(T)} not supported.");
        }


        // obsolete ...
        ///// <summary>
        ///// performs element by element inversion of the array
        ///// yi = 1.0 / xi
        ///// </summary>
        ///// <typeparam name="T"> ArrayBase[double] </typeparam>
        ///// <param name="x"> array x </param>
        ///// <param name="y"> array y </param>
        ///// <param name="checkLength"> whether checks the lengths of input arrays </param>
        //public static void InvD<T>(T x, ref T y,
        //    bool checkLength = true)
        //    where T : DenseArrayBase<double>
        //{
        //    if (checkLength) { ExArrayLengthD(x, y); }
        //    factory.iVMF.InvD(x.Count, x, ref y);
        //}

        /// <summary>
        /// performs element by element inversion of the vector
        /// </summary>
        /// <param name="x"> input vector x </param>
        /// <returns> result vector </returns>
        public static VectorD Inv(VectorD x)
        {
            VectorD y = new(count: x.Count, mode: ArrayInitMode.Malloc);
            //InvD(x, ref y, checkLength: false);
            factory.iVMF.InvD(x.Count, x, ref y);
            return y;
        }

        /// <summary>
        /// performs element by element inversion of the matrix
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <returns> result matrix </returns>
        public static MatrixD Inv(MatrixD x)
        {
            MatrixD y = new(rows: x.Rows, cols: x.Cols, mode: ArrayInitMode.Malloc);
            //InvD(x, ref y, checkLength: false);
            factory.iVMF.InvD(x.Count, x, ref y);
            return y;
        }

        #endregion
        #region ---- Sqrt [D/Z] ----

        /// <summary>
        /// Computes the square root of each element in the input array <paramref name="x"/> and stores the result in the output array <paramref name="y"/>.
        /// </summary>
        /// <typeparam name="T">The numeric type of the array elements. Must implement <see cref="INumber{T}"/>.</typeparam>
        /// <param name="x">The input array containing the values to compute the square root for.</param>
        /// <param name="y">The output array where the computed square root values will be stored.</param>
        public static void Sqrt<T>(DenseArray<T> x, ref DenseArray<T> y)
            where T : INumber<T>
            => Sqrt(x, y);

        internal static void Sqrt<T>(DenseArray<T> x, DenseArray<T> y)
            where T : INumber<T>
        {
            if (typeof(T) == typeof(Real)) factory.iVMF.Sqrt(x.Count, x.DPtr, y.DPtr);
            else if (typeof(T) == typeof(Cplx)) factory.iVMF.Sqrt(x.Count, x.VPtr, y.VPtr);
            else throw new NotSupportedException($"Type {typeof(T)} not supported.");
        }


        // obsolete ...
        ///// <summary>
        ///// computes the square root of array elements
        ///// yi = sqrt[xi]
        ///// </summary>
        ///// <typeparam name="T"> ArrayBase[double] </typeparam>
        ///// <param name="x"> array x </param>
        ///// <param name="y"> array y </param>
        ///// <param name="checkLength"> whether checks the lengths of input arrays </param>
        //public static void SqrtD<T>(T x, ref T y,
        //    bool checkLength = true)
        //    where T : DenseArrayBase<double>
        //{
        //    if (checkLength) { ExArrayLengthD(x, y); }
        //    factory.iVMF.SqrtD(x.Count, x, ref y);
        //}

        ///// <summary>
        ///// computes the square root of array elements
        ///// yi = sqrt[xi]
        ///// </summary>
        ///// <typeparam name="T"> ArrayBase[Complex] </typeparam>
        ///// <param name="x"> array x </param>
        ///// <param name="y"> array y </param>
        ///// <param name="checkLength"> whether checks the lengths of input arrays </param>
        //public static void SqrtZ<T>(T x, ref T y,
        //    bool checkLength = true)
        //    where T : DenseArrayBase<Complex>
        //{
        //    if (checkLength) { ExArrayLengthZ(x, y); }
        //    factory.iVMF.SqrtZ(x.Count, x, ref y);
        //}

        /// <summary>
        /// computes the square root of vector elements
        /// </summary>
        /// <param name="x"> input vector </param>
        /// <returns> result vector </returns>
        public static VectorD Sqrt(VectorD x)
        {
            VectorD y = new(count: x.Count, mode: ArrayInitMode.Malloc);
            //SqrtD(x, ref y, checkLength: false);
            factory.iVMF.SqrtD(x.Count, x, ref y);
            return y;
        }

        /// <summary>
        /// computes the square root of matrix elements
        /// </summary>
        /// <param name="x"> input matrix </param>
        /// <returns> result matrix </returns>
        public static MatrixD Sqrt(MatrixD x)
        {
            MatrixD y = new(rows: x.Rows, cols: x.Cols, mode: ArrayInitMode.Malloc);
            //SqrtD(x, ref y, checkLength: false);
            factory.iVMF.SqrtD(x.Count, x, ref y);
            return y;
        }

        /// <summary>
        /// computes the square root of vector elements
        /// </summary>
        /// <param name="x"> input vector </param>
        /// <returns> result vector </returns>
        public static VectorZ Sqrt(VectorZ x)
        {
            VectorZ y = new(count: x.Count, mode: ArrayInitMode.Malloc);
            //SqrtZ(x, ref y, checkLength: false);
            factory.iVMF.SqrtZ(x.Count, x, ref y);
            return y;
        }

        /// <summary>
        /// computes the square root of matrix elements
        /// </summary>
        /// <param name="x"> input matrix </param>
        /// <returns> result matrix </returns>
        public static MatrixZ Sqrt(MatrixZ x)
        {
            MatrixZ y = new(rows: x.Rows, cols: x.Cols, mode: ArrayInitMode.Malloc);
            //SqrtZ(x, ref y, checkLength: false);
            factory.iVMF.SqrtZ(x.Count, x, ref y);
            return y;
        }

        #endregion
        #region ---- InvSqrt [D] ----

        /// <summary>
        /// Computes the inverse square root of each element in a dense array of real numbers.
        /// Each element of the output array <paramref name="y"/> is set to 1.0 divided by the square root of the corresponding element of the input array <paramref name="x"/>.
        /// y[i] = 1.0 / sqrt(x[i])
        /// </summary>
        /// <param name="x">The input dense array of real numbers.</param>
        /// <param name="y">The output dense array to store the inverse square roots. Must be pre-allocated and have the same length as <paramref name="x"/>.</param>
        public static void InvSqrt(DenseArray<Real> x, ref DenseArray<Real> y)
            => factory.iVMF.InvSqrt(x.Count, x.DPtr, y.DPtr);


        // obsolete ...
        ///// <summary>
        ///// computes the inverse square root of array elements
        ///// yi = 1.0 / sqrt[xi]
        ///// </summary>
        ///// <typeparam name="T"> ArrayBase[double] </typeparam>
        ///// <param name="x"> array x </param>
        ///// <param name="y"> array y </param>
        ///// <param name="checkLength"> whether checks the lengths of input arrays </param>
        //public static void InvSqrtD<T>(T x, ref T y,
        //    bool checkLength = true)
        //    where T : DenseArrayBase<double>
        //{
        //    if (checkLength) { ExArrayLengthD(x, y); }
        //    factory.iVMF.InvSqrtD(x.Count, x, ref y);
        //}

        /// <summary>
        /// computes the inverse square root of vector elements
        /// </summary>
        /// <param name="x"> input vector x </param>
        /// <returns> result vector </returns>
        public static VectorD InvSqrt(VectorD x)
        {
            VectorD y = new(count: x.Count, mode: ArrayInitMode.Malloc);
            //InvSqrtD(x, ref y, checkLength: false);
            factory.iVMF.InvSqrtD(x.Count, x, ref y);
            return y;
        }

        /// <summary>
        /// computes the inverse square root of matrix elements
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <returns> result matrix </returns>
        public static MatrixD InvSqrt(MatrixD x)
        {
            MatrixD y = new(rows: x.Rows, cols: x.Cols, mode: ArrayInitMode.Malloc);
            //InvSqrtD(x, ref y, checkLength: false);
            factory.iVMF.InvSqrtD(x.Count, x, ref y);
            return y;
        }

        #endregion
        #region ---- Cbrt [D] ----

        /// <summary>
        /// Computes the cube root of each element in a dense array of real numbers.
        /// Each element of the output array <paramref name="y"/> is set to the cube root of the corresponding element of the input array <paramref name="x"/>.
        /// y[i] = cbrt(x[i])
        /// </summary>
        /// <param name="x">The input dense array of real numbers.</param>
        /// <param name="y">The output dense array to store the cube roots. Must be pre-allocated and have the same length as <paramref name="x"/>.</param>
        public static void Cbrt(DenseArray<Real> x, ref DenseArray<Real> y)
            => factory.iVMF.Cbrt(x.Count, x.DPtr, y.DPtr);


        // obsolete ...
        ///// <summary>
        ///// computes the cube root of array elements
        ///// yi = cbrt[xi]
        ///// </summary>
        ///// <typeparam name="T"> ArrayBase[double] </typeparam>
        ///// <param name="x"> array x </param>
        ///// <param name="y"> arary y </param>
        ///// <param name="checkLength"> whether checks the lengths of input arrays </param>
        //public static void CbrtD<T>(T x, ref T y,
        //    bool checkLength = true)
        //    where T : DenseArrayBase<double>
        //{
        //    if (checkLength) { ExArrayLengthD(x, y); }
        //    factory.iVMF.CbrtD(x.Count, x, ref y);
        //}

        /// <summary>
        /// Computes the cube root of vector elements
        /// </summary>
        /// <param name="x"> input vector x </param>
        /// <returns> result vector </returns>
        public static VectorD Cbrt(VectorD x)
        {
            VectorD y = new(count: x.Count, mode: ArrayInitMode.Malloc);
            //CbrtD(x, ref y, checkLength: false);
            factory.iVMF.CbrtD(x.Count, x, ref y);
            return y;
        }

        /// <summary>
        /// Computes the cube root of matrix elements
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <returns> result matrix </returns>
        public static MatrixD Cbrt(MatrixD x)
        {
            MatrixD y = new(rows: x.Rows, cols: x.Cols, mode: ArrayInitMode.Malloc);
            //CbrtD(x, ref y, checkLength: false);
            factory.iVMF.CbrtD(x.Count, x, ref y);
            return y;
        }

        #endregion
        #region ---- InvCbrt [D] ----

        /// <summary>
        /// Computes the inverse cube root of each element in the input dense array <paramref name="x"/> and stores the result in <paramref name="y"/>.
        /// Each element of the output array y is set to 1.0 divided by the cube root of the corresponding element of the input array x.
        /// y[i] = 1.0 / cbrt(x[i])
        /// </summary>
        /// <param name="x">The input dense array of real numbers.</param>
        /// <param name="y">The output dense array to store the inverse cube roots. Must be pre-allocated and have the same length as <paramref name="x"/>.</param>
        public static void InvCbrt(DenseArray<Real> x, ref DenseArray<Real> y)
            => factory.iVMF.InvCbrt(x.Count, x.DPtr, y.DPtr);


        // obsolete ...
        ///// <summary>
        ///// computes the inverse cube root of array elements
        ///// yi = 1.0 / cbrt[xi]
        ///// </summary>
        ///// <typeparam name="T"> ArrayBase[double] </typeparam>
        ///// <param name="x"> array x </param>
        ///// <param name="y"> array y </param>
        ///// <param name="checkLength"> whether checks the lengths of input arrays </param>
        //public static void InvCbrtD<T>(T x, ref T y,
        //    bool checkLength = true)
        //    where T : DenseArrayBase<double>
        //{
        //    if (checkLength) { ExArrayLengthD(x, y); }
        //    factory.iVMF.InvCbrtD(x.Count, x, ref y);
        //}

        /// <summary>
        /// computes the inverse cube root of vector elements
        /// </summary>
        /// <param name="x"> input vector x </param>
        /// <returns> result vector </returns>
        public static VectorD InvCbrt(VectorD x)
        {
            VectorD y = new(count: x.Count, mode: ArrayInitMode.Malloc);
            //InvCbrtD(x, ref y, checkLength: false);
            factory.iVMF.InvCbrtD(x.Count, x, ref y);
            return y;
        }

        /// <summary>
        /// computes the inverse cube root of matrix elements
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <returns> result matrix </returns>
        public static MatrixD InvCbrt(MatrixD x)
        {
            MatrixD y = new(rows: x.Rows, cols: x.Cols, mode: ArrayInitMode.Malloc);
            //InvCbrtD(x, ref y, checkLength: false);
            factory.iVMF.InvCbrtD(x.Count, x, ref y);
            return y;
        }

        #endregion
        #region ---- Sqr [D] ----

        /// <summary>
        /// Computes the element-wise square of each element in a dense array of real numbers.
        /// Each element of the output array <paramref name="y"/> is set to the square of the corresponding element of the input array <paramref name="x"/>.
        /// y[i] = x[i]^2
        /// </summary>
        /// <param name="x">The input dense array of real numbers.</param>
        /// <param name="y">The output dense array to store the squared values. Must be pre-allocated and have the same length as <paramref name="x"/>.</param>
        public static void Sqr(DenseArray<Real> x, ref DenseArray<Real> y)
            => factory.iVMF.Sqr(x.Count, x.DPtr, y.DPtr);


        // obsolete ...
        ///// <summary>
        ///// performs element by element squaring of array x
        ///// y = x^2
        ///// </summary>
        ///// <typeparam name="T"> ArrayBase[double] </typeparam>
        ///// <param name="x"> array x </param>
        ///// <param name="y"> array y </param>
        ///// <param name="checkLength"> whether checks the lengths of input arrays </param>
        //public static void SquareD<T>(T x, ref T y,
        //    bool checkLength = true) 
        //    where T : DenseArrayBase<double>
        //{
        //    if (checkLength) { ExArrayLengthD(x, y); }
        //    factory.iVMF.SqrD(x.Count, x, ref y);
        //}

        ///// <summary>
        ///// performs element by element squaring of array x
        ///// y = x^2
        ///// </summary>
        ///// <typeparam name="T"> ArrayBase[Complex] </typeparam>
        ///// <param name="x"> array x </param>
        ///// <param name="y"> array y </param>
        ///// <param name="checkLength"> whether checks the lengths of input arrays </param>
        //public static void SquareZ<T>(T x, ref T y,
        //    bool checkLength = true) 
        //    where T : DenseArrayBase<Complex>
        //{
        //    if (checkLength) { ExArrayLengthZ(x, y); }
        //    factory.iVMF.PowxZ(x.Count, x, 2.0, ref y);
        //}

        /// <summary>
        /// performs element by element squaring of vector x
        /// </summary>
        /// <param name="x"> input vector x </param>
        /// <returns> result vector </returns>
        public static VectorD Square(VectorD x)
        {
            VectorD y = new(count: x.Count, mode: ArrayInitMode.Malloc);
            //SquareD(x, ref y, checkLength: false);
            factory.iVMF.SqrD(x.Count, x, ref y);
            return y;
        }

        /// <summary>
        /// performs element by element squaring of matrix x
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <returns> result matrix </returns>
        public static MatrixD Square(MatrixD x)
        {
            MatrixD y = new(rows: x.Rows, cols: x.Cols, mode: ArrayInitMode.Malloc);
            //SquareD(x, ref y, checkLength: false);
            factory.iVMF.SqrD(x.Count, x, ref y);
            return y;
        }

        /// <summary>
        /// performs element by element squaring of vector x
        /// </summary>
        /// <param name="x"> input vector x </param>
        /// <returns> result vector </returns>
        public static VectorZ Square(VectorZ x)
        {
            VectorZ y = new(count: x.Count, mode: ArrayInitMode.Malloc);
            //SquareZ(x, ref y, checkLength: false);
            factory.iVMF.PowxZ(x.Count, x, 2.0, ref y);
            return y;
        }

        /// <summary>
        /// performs element by element squaring of matrix x
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <returns> result matrix </returns>
        public static MatrixZ Square(MatrixZ x)
        {
            MatrixZ y = new(rows: x.Rows, cols: x.Cols, mode: ArrayInitMode.Malloc);
            //SquareZ(x, ref y, checkLength: false);
            factory.iVMF.PowxZ(x.Count, x, 2.0, ref y);
            return y;
        }

        #endregion
        #region ---- Exp [D/Z] ----

        /// <summary>
        /// Computes the exponential of each element in the input array <paramref name="x"/> and stores the result in <paramref name="y"/>.
        /// </summary>
        /// <typeparam name="T">The type of the array elements, must implement <see cref="INumber{T}"/>.</typeparam>
        /// <param name="x">The input array whose elements will be exponentiated.</param>
        /// <param name="y">The output array where the results will be stored.</param>
        public static void Exp<T>(DenseArray<T> x, ref DenseArray<T> y)
            where T : INumber<T>
            => Exp(x, y);

        internal static void Exp<T>(DenseArray<T> x, DenseArray<T> y)
            where T : INumber<T>
        {
            if (typeof(T) == typeof(Real)) factory.iVMF.Exp(x.Count, x.DPtr, y.DPtr);
            else if (typeof(T) == typeof(Cplx)) factory.iVMF.Exp(x.Count, x.VPtr, y.VPtr);
            else throw new NotSupportedException($"Type {typeof(T)} not supported.");
        }


        // obsolete ...
        ///// <summary>
        ///// computes an exponential of array elements
        ///// yi = exp[xi]
        ///// </summary>
        ///// <typeparam name="T"> ArrayBase[double] </typeparam>
        ///// <param name="x"> array x </param>
        ///// <param name="y"> array y </param>
        ///// <param name="checkLength"> whether checks the lengths of input arrays </param>
        //public static void ExpD<T>(T x, ref T y,
        //    bool checkLength = true)
        //    where T : DenseArrayBase<double>
        //{
        //    if (checkLength) { ExArrayLengthD(x, y); }
        //    factory.iVMF.ExpD(x.Count, x, ref y);
        //}

        ///// <summary>
        ///// computes an exponential of array elements
        ///// yi = exp[xi]
        ///// </summary>
        ///// <typeparam name="T"> ArrayBase[Complex] </typeparam>
        ///// <param name="x"> array x </param>
        ///// <param name="y"> array y </param>
        ///// <param name="checkLength"> whether checks the lengths of input arrays </param>
        //public static void ExpZ<T>(T x, ref T y,
        //    bool checkLength = true)
        //    where T : DenseArrayBase<Complex>
        //{
        //    if (checkLength) { ExArrayLengthZ(x, y); }
        //    factory.iVMF.ExpZ(x.Count, x, ref y);
        //}

        /// <summary>
        /// computes an exponential of vector elements
        /// </summary>
        /// <param name="x"> input vector </param>
        /// <returns> result vector </returns>
        public static VectorD Exp(VectorD x)
        {
            VectorD y = new(count: x.Count, mode: ArrayInitMode.Malloc);
            //ExpD(x, ref y, checkLength: false);
            factory.iVMF.ExpD(x.Count, x, ref y);
            return y;
        }

        /// <summary>
        /// computes an exponential of matrix elements
        /// </summary>
        /// <param name="x"> input matrix </param>
        /// <returns> result matrix </returns>
        public static MatrixD Exp(MatrixD x)
        {
            MatrixD y = new(rows: x.Rows, cols: x.Cols, mode: ArrayInitMode.Malloc);
            //ExpD(x, ref y, checkLength: false);
            factory.iVMF.ExpD(x.Count, x, ref y);
            return y;
        }

        /// <summary>
        /// computes an exponential of vector elements
        /// </summary>
        /// <param name="x"> input vector </param>
        /// <returns> result vector </returns>
        public static VectorZ Exp(VectorZ x)
        {
            VectorZ y = new(count: x.Count, mode: ArrayInitMode.Malloc);
            //ExpZ(x, ref y, checkLength: false);
            factory.iVMF.ExpZ(x.Count, x, ref y);
            return y;
        }

        /// <summary>
        /// computes an exponential of matrix elements
        /// </summary>
        /// <param name="x"> input matrix </param>
        /// <returns> result matrix </returns>
        public static MatrixZ Exp(MatrixZ x)
        {
            MatrixZ y = new(rows: x.Rows, cols: x.Cols, mode: ArrayInitMode.Malloc);
            //ExpZ(x, ref y, checkLength: false);
            factory.iVMF.ExpZ(x.Count, x, ref y);
            return y;
        }

        #endregion
        #region ---- Exp2 [D] ----

        /// <summary>
        /// Computes the base-2 exponential of each element in the input dense array <paramref name="x"/> and stores the result in <paramref name="y"/>.
        /// Each element of the output array y is set to 2 raised to the power of the corresponding element of the input array x.
        /// y[i] = 2^x[i]
        /// </summary>
        /// <param name="x">The input dense array of real numbers.</param>
        /// <param name="y">The output dense array to store the results. Must be pre-allocated and have the same length as <paramref name="x"/>.</param>
        public static void Exp2(DenseArray<Real> x, ref DenseArray<Real> y)
            => factory.iVMF.Exp2(x.Count, x.DPtr, y.DPtr);

        #endregion
        #region ---- Exp10 [D] ----

        /// <summary>
        /// Computes the base-10 exponential of each element in the input dense array <paramref name="x"/> and stores the result in <paramref name="y"/>.
        /// Each element of the output array <paramref name="y"/> is set to 10 raised to the power of the corresponding element of the input array <paramref name="x"/>.
        /// y[i] = 10^x[i]
        /// </summary>
        /// <param name="x">The input dense array of real numbers.</param>
        /// <param name="y">The output dense array to store the results. Must be pre-allocated and have the same length as <paramref name="x"/>.</param>
        public static void Exp10(DenseArray<Real> x, ref DenseArray<Real> y)
            => factory.iVMF.Exp10(x.Count, x.DPtr, y.DPtr);

        #endregion
        #region ---- Expm1 [D] ----

        /// <summary>
        /// Computes exp(x) - 1 for each element in the input dense array <paramref name="x"/> and stores the result in <paramref name="y"/>.
        /// This function is more accurate than computing exp(x) - 1 directly for small values of x.
        /// y[i] = e^x[i] - 1
        /// </summary>
        /// <param name="x">The input dense array of real numbers.</param>
        /// <param name="y">The output dense array to store the results. Must be pre-allocated and have the same length as <paramref name="x"/>.</param>
        public static void Expm1(DenseArray<Real> x, ref DenseArray<Real> y)
            => factory.iVMF.Expm1(x.Count, x.DPtr, y.DPtr);

        #endregion
        #region ---- Ln [D/Z] ----

        /// <summary>
        /// Computes the natural logarithm of each element in the input array <paramref name="x"/> and stores the result in <paramref name="y"/>.
        /// </summary>
        /// <typeparam name="T">
        /// The numeric type of the array elements. Must implement <see cref="INumber{T}"/>.
        /// </typeparam>
        /// <param name="x">The input array containing the values for which to compute the natural logarithm.</param>
        /// <param name="y">The output array where the computed natural logarithm values will be stored.</param>
        public static void Ln<T>(DenseArray<T> x, ref DenseArray<T> y)
            where T : INumber<T>
            => Ln(x, y);

        internal static void Ln<T>(DenseArray<T> x, DenseArray<T> y)
            where T : INumber<T>
        {
            if (typeof(T) == typeof(Real)) factory.iVMF.Ln(x.Count, x.DPtr, y.DPtr);
            else if (typeof(T) == typeof(Cplx)) factory.iVMF.Ln(x.Count, x.VPtr, y.VPtr);
            else throw new NotSupportedException($"Type {typeof(T)} not supported.");
        }


        // obsolete ...
        ///// <summary>
        ///// computes natural logarithm of array elements
        ///// yi = ln[xi]
        ///// </summary>
        ///// <typeparam name="T"> ArrayBase[double] </typeparam>
        ///// <param name="x"> array x </param>
        ///// <param name="y"> array y </param>
        ///// <param name="checkLength"> whether checks the lengths of input arrays </param>
        //public static void LnD<T>(T x, ref T y,
        //    bool checkLength = true)
        //    where T : DenseArrayBase<double>
        //{
        //    if (checkLength) { ExArrayLengthD(x, y); }
        //    factory.iVMF.LnD(x.Count, x, ref y);
        //}

        ///// <summary>
        ///// computes natural logarithm of array elements
        ///// yi = ln[xi]
        ///// </summary>
        ///// <typeparam name="T"> ArrayBase[Complex] </typeparam>
        ///// <param name="x"> array x </param>
        ///// <param name="y"> array y </param>
        ///// <param name="checkLength"> whether checks the lengths of input arrays </param>
        //public static void LnZ<T>(T x, ref T y,
        //    bool checkLength = true)
        //    where T : DenseArrayBase<Complex>
        //{
        //    if (checkLength) { ExArrayLengthZ(x, y); }
        //    factory.iVMF.LnZ(x.Count, x, ref y);
        //}

        /// <summary>
        /// computes natural logarithm of vector elements
        /// </summary>
        /// <param name="x"> input vector x </param>
        /// <returns> result vector </returns>
        public static VectorD Ln(VectorD x)
        {
            VectorD y = new(count: x.Count, mode: ArrayInitMode.Malloc);
            //LnD(x, ref y, checkLength: false);
            factory.iVMF.LnD(x.Count, x, ref y);
            return y;
        }

        /// <summary>
        /// computes natural logarithm of matrix elements
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <returns> result matrix </returns>
        public static MatrixD Ln(MatrixD x)
        {
            MatrixD y = new(rows: x.Rows, cols: x.Cols, mode: ArrayInitMode.Malloc);
            //LnD(x, ref y, checkLength: false);
            factory.iVMF.LnD(x.Count, x, ref y);
            return y;
        }

        /// <summary>
        /// computes natural logarithm of vector elements
        /// </summary>
        /// <param name="x"> input vector x </param>
        /// <returns> result vector </returns>
        public static VectorZ Ln(VectorZ x)
        {
            VectorZ y = new(count: x.Count, mode: ArrayInitMode.Malloc);
            //LnZ(x, ref y, checkLength: false);
            factory.iVMF.LnZ(x.Count, x, ref y);
            return y;
        }

        /// <summary>
        /// computes natural logarithm of matrix elements
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <returns> result matrix </returns>
        public static MatrixZ Ln(MatrixZ x)
        {
            MatrixZ y = new(rows: x.Rows, cols: x.Cols, mode: ArrayInitMode.Malloc);
            //LnZ(x, ref y, checkLength: false);
            factory.iVMF.LnZ(x.Count, x, ref y);
            return y;
        }

        #endregion
        #region ---- Log2 [D] ----

        /// <summary>
        /// Computes the base-2 logarithm of each element in the input dense array <paramref name="x"/> and stores the result in <paramref name="y"/>.
        /// Each element of the output array <paramref name="y"/> is set to the base-2 logarithm of the corresponding element of the input array <paramref name="x"/>.
        /// y[i] = log2(x[i])
        /// </summary>
        /// <param name="x">The input dense array of real numbers.</param>
        /// <param name="y">The output dense array to store the results. Must be pre-allocated and have the same length as <paramref name="x"/>.</param>
        public static void Log2(DenseArray<Real> x, ref DenseArray<Real> y)
            => factory.iVMF.Log2(x.Count, x.DPtr, y.DPtr);

        #endregion
        #region ---- Log10 [D/Z] ----

        /// <summary>
        /// Computes the base 10 logarithm of each element in the input array <paramref name="x"/> and stores the result in <paramref name="y"/>.
        /// </summary>
        /// <typeparam name="T">A numeric type implementing <see cref="INumber{T}"/>.</typeparam>
        /// <param name="x">Input dense array whose elements will be processed.</param>
        /// <param name="y">Reference to the output dense array where the results will be stored.</param>
        public static void Log10<T>(DenseArray<T> x, ref DenseArray<T> y)
            where T : INumber<T>
            => Log10(x, y);

        internal static void Log10<T>(DenseArray<T> x, DenseArray<T> y)
            where T : INumber<T>
        {
            if (typeof(T) == typeof(Real)) factory.iVMF.Log10(x.Count, x.DPtr, y.DPtr);
            else if (typeof(T) == typeof(Cplx)) factory.iVMF.Log10(x.Count, x.VPtr, y.VPtr);
            else throw new NotSupportedException($"Type {typeof(T)} not supported.");
        }


        // obsolete ...
        ///// <summary>
        ///// computes the base 10 logarithm of array elements
        ///// yi = log10[xi]
        ///// </summary>
        ///// <typeparam name="T"> ArrayBase[double] </typeparam>
        ///// <param name="x"> array x </param>
        ///// <param name="y"> array y </param>
        ///// <param name="checkLength"> whether checks the lengths of input arrays </param>
        //public static void Log10D<T>(T x, ref T y,
        //    bool checkLength = true)
        //    where T : DenseArrayBase<double>
        //{
        //    if (checkLength) { ExArrayLengthD(x, y); }
        //    factory.iVMF.Log10D(x.Count, x, ref y);
        //}

        ///// <summary>
        ///// computes the base 10 logarithm of array elements
        ///// yi = log10[xi]
        ///// </summary>
        ///// <typeparam name="T"> ArrayBase[Complex] </typeparam>
        ///// <param name="x"> array x </param>
        ///// <param name="y"> array y </param>
        ///// <param name="checkLength"> whether checks the lengths of input arrays </param>
        //public static void Log10Z<T>(T x, ref T y,
        //    bool checkLength = true)
        //    where T : DenseArrayBase<Complex>
        //{
        //    if (checkLength) { ExArrayLengthZ(x, y); }
        //    factory.iVMF.Log10Z(x.Count, x, ref y);
        //}

        /// <summary>
        /// computes the base 10 logarithm of vector elements
        /// </summary>
        /// <param name="x"> input vector x </param>
        /// <returns> result vector </returns>
        public static VectorD Log10(VectorD x)
        {
            VectorD y = new(count: x.Count, mode: ArrayInitMode.Malloc);
            //Log10D(x, ref y, checkLength: false);
            factory.iVMF.Log10D(x.Count, x, ref y);
            return y;
        }

        /// <summary>
        /// computes the base 10 logarithm of matrix elements
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <returns> result matrix </returns>
        public static MatrixD Log10(MatrixD x)
        {
            MatrixD y = new(rows: x.Rows, cols: x.Cols, mode: ArrayInitMode.Malloc);
            //Log10D(x, ref y, checkLength: false);
            factory.iVMF.Log10D(x.Count, x, ref y);
            return y;
        }

        /// <summary>
        /// computes the base 10 logarithm of vector elements
        /// </summary>
        /// <param name="x"> input vector x </param>
        /// <returns> result vector </returns>
        public static VectorZ Log10(VectorZ x)
        {
            VectorZ y = new(count: x.Count, mode: ArrayInitMode.Malloc);
            //Log10Z(x, ref y, checkLength: false);
            factory.iVMF.Log10Z(x.Count, x, ref y);
            return y;
        }

        /// <summary>
        /// computes the base 10 logarithm of matrix elements
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <returns> result matrix </returns>
        public static MatrixZ Log10(MatrixZ x)
        {
            MatrixZ y = new(rows: x.Rows, cols: x.Cols, mode: ArrayInitMode.Malloc);
            //Log10Z(x, ref y, checkLength: false);
            factory.iVMF.Log10Z(x.Count, x, ref y);
            return y;
        }

        #endregion
        #region ---- Log1p [D] ----

        /// <summary>
        /// Computes the natural logarithm of one plus each element in the input dense array <paramref name="x"/> and stores the result in <paramref name="y"/>.
        /// This function is more accurate than computing <c>log(1 + x)</c> directly for small values of <paramref name="x"/>.
        /// <para>y[i] = log(1 + x[i])</para>
        /// </summary>
        /// <param name="x">The input dense array of real numbers.</param>
        /// <param name="y">The output dense array to store the results. Must be pre-allocated and have the same length as <paramref name="x"/>.</param>
        public static void Log1p(DenseArray<Real> x, ref DenseArray<Real> y)
            => factory.iVMF.Log1p(x.Count, x.DPtr, y.DPtr);

        #endregion
        #region ---- Logb [D] ----

        /// <summary>
        /// Computes the exponent of the absolute value of each element in a dense array of real numbers.
        /// Each element of the output array <paramref name="y"/> is set to the exponent of the absolute value of the corresponding element of the input array <paramref name="x"/>.
        /// y[i] = logb(x[i])
        /// </summary>
        /// <param name="x">The input dense array of real numbers.</param>
        /// <param name="y">The output dense array to store the exponents. Must be pre-allocated and have the same length as <paramref name="x"/>.</param>
        public static void Logb(DenseArray<Real> x, ref DenseArray<Real> y)
            => factory.iVMF.Logb(x.Count, x.DPtr, y.DPtr);

        #endregion
        #region ---- Cos [D/Z] ----

        /// <summary>
        /// Computes the cosine of each element in the input array <paramref name="x"/> and stores the result in <paramref name="y"/>.
        /// </summary>
        /// <typeparam name="T">The numeric type of the array elements, implementing <see cref="INumber{T}"/>.</typeparam>
        /// <param name="x">The input array containing the values for which to compute the cosine.</param>
        /// <param name="y">The output array where the computed cosine values will be stored.</param>
        public static void Cos<T>(DenseArray<T> x, ref DenseArray<T> y)
            where T : INumber<T>
            => Cos(x, y);

        internal static void Cos<T>(DenseArray<T> x, DenseArray<T> y)
            where T : INumber<T>
        {
            if (typeof(T) == typeof(Real)) factory.iVMF.Cos(x.Count, x.DPtr, y.DPtr);
            else if (typeof(T) == typeof(Cplx)) factory.iVMF.Cos(x.Count, x.VPtr, y.VPtr);
            else throw new NotSupportedException($"Type {typeof(T)} not supported.");
        }


        // obsolete ...
        ///// <summary>
        ///// computes cosine of array elements
        ///// yi = cos[xi]
        ///// </summary>
        ///// <typeparam name="T"> ArrayBase[double] </typeparam>
        ///// <param name="x"> array x </param>
        ///// <param name="y"> array y </param>
        ///// <param name="checkLength"> whether checks the lengths of input arrays </param>
        //public static void CosD<T>(T x, ref T y,
        //    bool checkLength = true)
        //    where T : DenseArrayBase<double>
        //{
        //    if (checkLength) { ExArrayLengthD(x, y); }
        //    factory.iVMF.CosD(x.Count, x, ref y);
        //}

        ///// <summary>
        ///// computes cosine of array elements
        ///// yi = cos[xi]
        ///// </summary>
        ///// <typeparam name="T"> ArrayBase[Complex] </typeparam>
        ///// <param name="x"> array x </param>
        ///// <param name="y"> array y </param>
        ///// <param name="checkLength"> whether checks the lengths of input arrays </param>
        //public static void CosZ<T>(T x, ref T y,
        //    bool checkLength = true)
        //    where T : DenseArrayBase<Complex>
        //{
        //    if (checkLength) { ExArrayLengthZ(x, y); }
        //    factory.iVMF.CosZ(x.Count, x, ref y);
        //}

        /// <summary>
        /// computes cosine of vector elements
        /// </summary>
        /// <param name="x"> input vector x </param>
        /// <returns> result vector </returns>
        public static VectorD Cos(VectorD x)
        {
            VectorD y = new(count: x.Count, mode: ArrayInitMode.Malloc);
            //CosD(x, ref y, checkLength: false);
            factory.iVMF.CosD(x.Count, x, ref y);
            return y;
        }

        /// <summary>
        /// computes cosine of matrix elements
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <returns> result matrix </returns>
        public static MatrixD Cos(MatrixD x)
        {
            MatrixD y = new(rows: x.Rows, cols: x.Cols, mode: ArrayInitMode.Malloc);
            //CosD(x, ref y, checkLength: false);
            factory.iVMF.CosD(x.Count, x, ref y);
            return y;
        }

        /// <summary>
        /// computes cosine of vector elements
        /// </summary>
        /// <param name="x"> input vector x </param>
        /// <returns> result vector </returns>
        public static VectorZ Cos(VectorZ x)
        {
            VectorZ y = new(count: x.Count, mode: ArrayInitMode.Malloc);
            //CosZ(x, ref y, checkLength: false);
            factory.iVMF.CosZ(x.Count, x, ref y);
            return y;
        }

        /// <summary>
        /// computes cosine of matrix elements
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <returns> result matrix </returns>
        public static MatrixZ Cos(MatrixZ x)
        {
            MatrixZ y = new(rows: x.Rows, cols: x.Cols, mode: ArrayInitMode.Malloc);
            //CosZ(x, ref y, checkLength: false);
            factory.iVMF.CosZ(x.Count, x, ref y);
            return y;
        }

        #endregion
        #region ---- Sin [D/Z] ----

        /// <summary>
        /// Computes the sine of each element in the input array <paramref name="x"/> and stores the result in <paramref name="y"/>.
        /// </summary>
        /// <typeparam name="T">The numeric type of the array elements, implementing <see cref="INumber{T}"/>.</typeparam>
        /// <param name="x">The input array containing the values for which to compute the sine.</param>
        /// <param name="y">The output array where the computed sine values will be stored.</param>
        public static void Sin<T>(DenseArray<T> x, ref DenseArray<T> y)
            where T : INumber<T>
            => Sin(x, y);

        internal static void Sin<T>(DenseArray<T> x, DenseArray<T> y)
            where T : INumber<T>
        {
            if (typeof(T) == typeof(Real)) factory.iVMF.Sin(x.Count, x.DPtr, y.DPtr);
            else if (typeof(T) == typeof(Cplx)) factory.iVMF.Sin(x.Count, x.VPtr, y.VPtr);
            else throw new NotSupportedException($"Type {typeof(T)} not supported.");
        }


        // obsolete ...
        ///// <summary>
        ///// computes sine of array elements
        ///// yi = sin[xi]
        ///// </summary>
        ///// <typeparam name="T"> ArrayBase[double] </typeparam>
        ///// <param name="x"> array x </param>
        ///// <param name="y"> array y </param>
        ///// <param name="checkLength"> whether checks the lengths of input arrays </param>
        //public static void SinD<T>(T x, ref T y,
        //    bool checkLength = true)
        //    where T : DenseArrayBase<double>
        //{
        //    if (checkLength) { ExArrayLengthD(x, y); }
        //    factory.iVMF.SinD(x.Count, x, ref y);
        //}

        ///// <summary>
        ///// computes sine of array elements
        ///// yi = sin[xi]
        ///// </summary>
        ///// <typeparam name="T"> ArrayBase[Complex] </typeparam>
        ///// <param name="x"> array x </param>
        ///// <param name="y"> array y </param>
        ///// <param name="checkLength"> whether checks the lengths of input arrays </param>
        //public static void SinZ<T>(T x, ref T y,
        //    bool checkLength = true)
        //    where T : DenseArrayBase<Complex>
        //{
        //    if (checkLength) { ExArrayLengthZ(x, y); }
        //    factory.iVMF.SinZ(x.Count, x, ref y);
        //}

        /// <summary>
        /// computes sine of vector elements
        /// </summary>
        /// <param name="x"> input vector x </param>
        /// <returns> result vector </returns>
        public static VectorD Sin(VectorD x)
        {
            VectorD y = new(count: x.Count, mode: ArrayInitMode.Malloc);
            //SinD(x, ref y, checkLength: false);
            factory.iVMF.SinD(x.Count, x, ref y);
            return y;
        }

        /// <summary>
        /// computes sine of matrix elements
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <returns> result matrix </returns>
        public static MatrixD Sin(MatrixD x)
        {
            MatrixD y = new(rows: x.Rows, cols: x.Cols, mode: ArrayInitMode.Malloc);
            //SinD(x, ref y, checkLength: false);
            factory.iVMF.SinD(x.Count, x, ref y);
            return y;
        }

        /// <summary>
        /// computes sine of vector elements
        /// </summary>
        /// <param name="x"> input vector x </param>
        /// <returns> result vector </returns>
        public static VectorZ Sin(VectorZ x)
        {
            VectorZ y = new(count: x.Count, mode: ArrayInitMode.Malloc);
            //SinZ(x, ref y, checkLength: false);
            factory.iVMF.SinZ(x.Count, x, ref y);
            return y;
        }

        /// <summary>
        /// computes sine of matrix elements
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <returns> result matrix </returns>
        public static MatrixZ Sin(MatrixZ x)
        {
            MatrixZ y = new(rows: x.Rows, cols: x.Cols, mode: ArrayInitMode.Malloc);
            //SinZ(x, ref y, checkLength: false);
            factory.iVMF.SinZ(x.Count, x, ref y);
            return y;
        }

        #endregion
        #region ---- Tan [D/Z] ----

        /// <summary>
        /// Computes the tangent of each element in the input array <paramref name="x"/> and stores the result in <paramref name="y"/>.
        /// </summary>
        /// <typeparam name="T">The numeric type of the array elements, implementing <see cref="INumber{T}"/>.</typeparam>
        /// <param name="x">The input array containing the values for which to compute the tangent.</param>
        /// <param name="y">The output array where the computed tangent values will be stored.</param>

        public static void Tan<T>(DenseArray<T> x, ref DenseArray<T> y)
            where T : INumber<T>
            => Tan(x, y);

        internal static void Tan<T>(DenseArray<T> x, DenseArray<T> y)
            where T : INumber<T>
        {
            if (typeof(T) == typeof(Real)) factory.iVMF.Tan(x.Count, x.DPtr, y.DPtr);
            else if (typeof(T) == typeof(Cplx)) factory.iVMF.Tan(x.Count, x.VPtr, y.VPtr);
            else throw new NotSupportedException($"Type {typeof(T)} not supported.");
        }


        // obsolete ...
        ///// <summary>
        ///// computes tangent of array elements
        ///// yi = tan[xi]
        ///// </summary>
        ///// <typeparam name="T"> ArrayBase[double] </typeparam>
        ///// <param name="x"> array x </param>
        ///// <param name="y"> array y </param>
        ///// <param name="checkLength"> whether checks the lengths of input arrays </param>
        //public static void TanD<T>(T x, ref T y,
        //    bool checkLength = true)
        //    where T : DenseArrayBase<double>
        //{
        //    if (checkLength) { ExArrayLengthD(x, y); }
        //    factory.iVMF.TanD(x.Count, x, ref y);
        //}

        ///// <summary>
        ///// computes tangent of array elements
        ///// yi = tan[xi]
        ///// </summary>
        ///// <typeparam name="T"> ArrayBase[Complex] </typeparam>
        ///// <param name="x"> array x </param>
        ///// <param name="y"> array y </param>
        ///// <param name="checkLength"> whether checks the lengths of input arrays </param>
        //public static void TanZ<T>(T x, ref T y,
        //    bool checkLength = true)
        //    where T : DenseArrayBase<Complex>
        //{
        //    if (checkLength) { ExArrayLengthZ(x, y); }
        //    factory.iVMF.TanZ(x.Count, x, ref y);
        //}

        /// <summary>
        /// computes tangent of vector elements
        /// </summary>
        /// <param name="x"> input vector x </param>
        /// <returns> result vector </returns>
        public static VectorD Tan(VectorD x)
        {
            VectorD y = new(count: x.Count, mode: ArrayInitMode.Malloc);
            //TanD(x, ref y, checkLength: false);
            factory.iVMF.TanD(x.Count, x, ref y);
            return y;
        }

        /// <summary>
        /// computes tangent of matrix elements
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <returns> result matrix </returns>
        public static MatrixD Tan(MatrixD x)
        {
            MatrixD y = new(rows: x.Rows, cols: x.Cols, mode: ArrayInitMode.Malloc);
            //TanD(x, ref y, checkLength: false);
            factory.iVMF.TanD(x.Count, x, ref y);
            return y;
        }

        /// <summary>
        /// computes tangent of vector elements
        /// </summary>
        /// <param name="x"> input vector x </param>
        /// <returns> result vector </returns>
        public static VectorZ Tan(VectorZ x)
        {
            VectorZ y = new(count: x.Count, mode: ArrayInitMode.Malloc);
            //TanZ(x, ref y, checkLength: false);
            factory.iVMF.TanZ(x.Count, x, ref y);
            return y;
        }

        /// <summary>
        /// computes tangent of matrix elements
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <returns> result matrix </returns>
        public static MatrixZ Tan(MatrixZ x)
        {
            MatrixZ y = new(rows: x.Rows, cols: x.Cols, mode: ArrayInitMode.Malloc);
            //TanZ(x, ref y, checkLength: false);
            factory.iVMF.TanZ(x.Count, x, ref y);
            return y;
        }

        #endregion
        #region ---- CosPI [D] ----

        /// <summary>
        /// Computes the cosine of each element in the input array <paramref name="a"/>,
        /// where the argument is interpreted as a multiple of π (i.e., computes cos(π * a[i]) for each element).
        /// </summary>
        /// <param name="a">The input dense array of real numbers.</param>
        /// <param name="y">The output dense array to store the results. Must be pre-allocated and have the same length as <paramref name="a"/>.</param>
        public static void CosPI(DenseArray<Real> a, ref DenseArray<Real> y)
            => factory.iVMF.Cospi(a.Count, a.DPtr, y.DPtr);

        #endregion
        #region ---- SinPI [D] ----

        /// <summary>
        /// Computes the sine of each element of the input array multiplied by pi.
        /// Each element of the output array <paramref name="y"/> is set to sin(a[i] * π).
        /// </summary>
        /// <param name="a">The input dense array of real numbers.</param>
        /// <param name="y">The output dense array to store the results. Must be pre-allocated and have the same length as <paramref name="a"/>.</param>
        public static void SinPI(DenseArray<Real> a, ref DenseArray<Real> y)
            => factory.iVMF.Sinpi(a.Count, a.DPtr, y.DPtr);

        #endregion
        #region ---- TanPI [D] ----

        /// <summary>
        /// Computes the tangent of each element of the input array multiplied by pi.
        /// Each element of the output array <paramref name="y"/> is set to tan(a[i] * π).
        /// </summary>
        /// <param name="a">The input dense array of real numbers.</param>
        /// <param name="y">The output dense array to store the results. Must be pre-allocated and have the same length as <paramref name="a"/>.</param>
        public static void TanPI(DenseArray<Real> a, ref DenseArray<Real> y)
            => factory.iVMF.Tanpi(a.Count, a.DPtr, y.DPtr);

        #endregion
        #region ---- Cosd [D] ----

        /// <summary>
        /// Computes the cosine of each element in the input array, where the input is interpreted in degrees.
        /// y[i] = cos(x[i]*PI/180)
        /// </summary>
        /// <param name="a">The input dense array containing angle values in degrees.</param>
        /// <param name="y">The dense array to store the resulting cosine values.</param>
        public static void Cosd(DenseArray<Real> a, ref DenseArray<Real> y)
            => factory.iVMF.Cosd(a.Count, a.DPtr, y.DPtr);

        #endregion
        #region ---- Sind [D] ----

        /// <summary>
        /// Computes the sine of each element in the input dense array <paramref name="a"/>,
        /// storing the result in the output dense array <paramref name="y"/>.
        /// y[i] = sin(x[i]*PI/180)
        /// </summary>
        /// <param name="a">The input dense array containing the values (in radians) for which to compute the sine.</param>
        /// <param name="y">The output dense array to store the computed sine values. Must be the same length as <paramref name="a"/>.</param>
        /// <remarks>
        /// This method delegates to the underlying vector math function provider (IVMF) implementation.
        /// </remarks>
        public static void Sind(DenseArray<Real> a, ref DenseArray<Real> y)
            => factory.iVMF.Sind(a.Count, a.DPtr, y.DPtr);

        #endregion
        #region ---- Tand [D] ----

        /// <summary>
        /// Computes the tangent of each element in the input dense array <paramref name="a"/> and stores the result in <paramref name="y"/>.
        /// y[i] = tan(x[i]*PI/180)
        /// </summary>
        /// <param name="a">The input dense array containing the values (in radians) for which to compute the tangent.</param>
        /// <param name="y">The dense array to store the computed tangent values. Must be the same length as <paramref name="a"/>.</param>
        public static void Tand(DenseArray<Real> a, ref DenseArray<Real> y)
            => factory.iVMF.Tand(a.Count, a.DPtr, y.DPtr);

        #endregion
        #region ---- Cosh [D/Z] ----

        /// <summary>
        /// Computes the hyperbolic cosine of each element in the input dense array <paramref name="x"/> and stores the result in <paramref name="y"/>.
        /// </summary>
        /// <typeparam name="T">The numeric type of the array elements, implementing <see cref="INumber{T}"/>.</typeparam>
        /// <param name="x">The input dense array whose elements will be processed.</param>
        /// <param name="y">The output dense array where the results will be stored.</param>
        public static void Cosh<T>(DenseArray<T> x, ref DenseArray<T> y)
            where T : INumber<T>
            => Cosh(x, y);

        internal static void Cosh<T>(DenseArray<T> x, DenseArray<T> y)
            where T : INumber<T>
        {
            if (typeof(T) == typeof(Real)) factory.iVMF.Cosh(x.Count, x.DPtr, y.DPtr);
            else if (typeof(T) == typeof(Cplx)) factory.iVMF.Cosh(x.Count, x.VPtr, y.VPtr);
            else throw new NotSupportedException($"Type {typeof(T)} not supported.");
        }


        // obsolete ...
        ///// <summary>
        ///// computes hyperbolic cosine of array elements
        ///// yi = cosh[xi]
        ///// </summary>
        ///// <typeparam name="T"> ArrayBase[double] </typeparam>
        ///// <param name="x"> array x </param>
        ///// <param name="y"> array y </param>
        ///// <param name="checkLength"> whether checks the lengths of input arrays </param>
        //public static void CoshD<T>(T x, ref T y,
        //    bool checkLength = true)
        //    where T : DenseArrayBase<double>
        //{
        //    if (checkLength) { ExArrayLengthD(x, y); }
        //    factory.iVMF.CoshD(x.Count, x, ref y);
        //}

        ///// <summary>
        ///// computes hyperbolic cosine of array elements
        ///// yi = cosh[xi]
        ///// </summary>
        ///// <typeparam name="T"> ArrayBase[Complex] </typeparam>
        ///// <param name="x"> array x </param>
        ///// <param name="y"> array y </param>
        ///// <param name="checkLength"> whether checks the lengths of input arrays </param>
        //public static void CoshZ<T>(T x, ref T y,
        //    bool checkLength = true)
        //    where T : DenseArrayBase<Complex>
        //{
        //    if (checkLength) { ExArrayLengthZ(x, y); }
        //    factory.iVMF.CoshZ(x.Count, x, ref y);
        //}

        /// <summary>
        /// computes hyperbolic cosine of vector elements
        /// </summary>
        /// <param name="x"> input vector x </param>
        /// <returns> result vector </returns>
        public static VectorD Cosh(VectorD x)
        {
            VectorD y = new(count: x.Count, mode: ArrayInitMode.Malloc);
            //CoshD(x, ref y, checkLength: false);
            factory.iVMF.CoshD(x.Count, x, ref y);
            return y;
        }

        /// <summary>
        /// computes hyperbolic cosine of matrix elements
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <returns> result matrix </returns>
        public static MatrixD Cosh(MatrixD x)
        {
            MatrixD y = new(rows: x.Rows, cols: x.Cols, mode: ArrayInitMode.Malloc);
            //CoshD(x, ref y, checkLength: false);
            factory.iVMF.CoshD(x.Count, x, ref y);
            return y;
        }

        /// <summary>
        /// computes hyperbolic cosine of vector elements
        /// </summary>
        /// <param name="x"> input vector x </param>
        /// <returns> result vector </returns>
        public static VectorZ Cosh(VectorZ x)
        {
            VectorZ y = new(count: x.Count, mode: ArrayInitMode.Malloc);
            //CoshZ(x, ref y, checkLength: false);
            factory.iVMF.CoshZ(x.Count, x, ref y);
            return y;
        }

        /// <summary>
        /// computes hyperbolic cosine of matrix elements
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <returns> result matrix </returns>
        public static MatrixZ Cosh(MatrixZ x)
        {
            MatrixZ y = new(rows: x.Rows, cols: x.Cols, mode: ArrayInitMode.Malloc);
            //CoshZ(x, ref y, checkLength: false);
            factory.iVMF.CoshZ(x.Count, x, ref y);
            return y;
        }

        #endregion
        #region ---- Sinh [D/Z] ----

        /// <summary>
        /// Computes the hyperbolic sine of each element in the input array <paramref name="x"/> and stores the result in <paramref name="y"/>.
        /// </summary>
        /// <typeparam name="T">The numeric type of the array elements, implementing <see cref="INumber{T}"/>.</typeparam>
        /// <param name="x">The input array containing the values for which to compute the hyperbolic sine.</param>
        /// <param name="y">The output array where the computed hyperbolic sine values will be stored.</param>
        public static void Sinh<T>(DenseArray<T> x, ref DenseArray<T> y)
            where T : INumber<T>
            => Sinh(x, y);

        internal static void Sinh<T>(DenseArray<T> x, DenseArray<T> y)
            where T : INumber<T>
        {
            if (typeof(T) == typeof(Real)) factory.iVMF.Sinh(x.Count, x.DPtr, y.DPtr);
            else if (typeof(T) == typeof(Cplx)) factory.iVMF.Sinh(x.Count, x.VPtr, y.VPtr);
            else throw new NotSupportedException($"Type {typeof(T)} not supported.");
        }


        // obsolete ...
        ///// <summary>
        ///// computes hyperbolic sine of array elements
        ///// yi = sinh[xi]
        ///// </summary>
        ///// <typeparam name="T"> ArrayBase[double] </typeparam>
        ///// <param name="x"> array x </param>
        ///// <param name="y"> array y </param>
        ///// <param name="checkLength"> whether checks the lengths of input arrays </param>
        //public static void SinhD<T>(T x, ref T y,
        //    bool checkLength = true)
        //    where T : DenseArrayBase<double>
        //{
        //    if (checkLength) { ExArrayLengthD(x, y); }
        //    factory.iVMF.SinhD(x.Count, x, ref y);
        //}

        ///// <summary>
        ///// computes hyperbolic sine of array elements
        ///// yi = sinh[xi]
        ///// </summary>
        ///// <typeparam name="T"> ArrayBase[Complex] </typeparam>
        ///// <param name="x"> array x </param>
        ///// <param name="y"> array y </param>
        ///// <param name="checkLength"> whether checks the lengths of input arrays </param>
        //public static void SinhZ<T>(T x, ref T y,
        //    bool checkLength = true)
        //    where T : DenseArrayBase<Complex>
        //{
        //    if (checkLength) { ExArrayLengthZ(x, y); }
        //    factory.iVMF.SinhZ(x.Count, x, ref y);
        //}

        /// <summary>
        /// computes hyperbolic sine of vector elements
        /// </summary>
        /// <param name="x"> input vector x </param>
        /// <returns> result vector </returns>
        public static VectorD Sinh(VectorD x)
        {
            VectorD y = new(count: x.Count, mode: ArrayInitMode.Malloc);
            //SinhD(x, ref y, checkLength: false);
            factory.iVMF.SinhD(x.Count, x, ref y);
            return y;
        }

        /// <summary>
        /// computes hyperbolic sine of matrix elements
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <returns> result matrix </returns>
        public static MatrixD Sinh(MatrixD x)
        {
            MatrixD y = new(rows: x.Rows, cols: x.Cols, mode: ArrayInitMode.Malloc);
            //SinhD(x, ref y, checkLength: false);
            factory.iVMF.SinhD(x.Count, x, ref y);
            return y;
        }

        /// <summary>
        /// computes hyperbolic sine of vector elements
        /// </summary>
        /// <param name="x"> input vector x </param>
        /// <returns> result vector </returns>
        public static VectorZ Sinh(VectorZ x)
        {
            VectorZ y = new(count: x.Count, mode: ArrayInitMode.Malloc);
            //SinhZ(x, ref y, checkLength: false);
            factory.iVMF.SinhZ(x.Count, x, ref y);
            return y;
        }

        /// <summary>
        /// computes hyperbolic sine of matrix elements
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <returns> result matrix </returns>
        public static MatrixZ Sinh(MatrixZ x)
        {
            MatrixZ y = new(rows: x.Rows, cols: x.Cols, mode: ArrayInitMode.Malloc);
            //SinhZ(x, ref y, checkLength: false);
            factory.iVMF.SinhZ(x.Count, x, ref y);
            return y;
        }

        #endregion
        #region ---- Tanh [D/Z] ----

        /// <summary>
        /// Computes the hyperbolic tangent of each element in the input array <paramref name="x"/> and stores the result in <paramref name="y"/>.
        /// </summary>
        /// <typeparam name="T">The numeric type of the array elements, implementing <see cref="INumber{T}"/>.</typeparam>
        /// <param name="x">The input array containing the values for which to compute the hyperbolic tangent.</param>
        /// <param name="y">The output array where the computed hyperbolic tangent values will be stored.</param>
        public static void Tanh<T>(DenseArray<T> x, ref DenseArray<T> y)
            where T : INumber<T>
            => Tanh(x, y);

        internal static void Tanh<T>(DenseArray<T> x, DenseArray<T> y)
            where T : INumber<T>
        {
            if (typeof(T) == typeof(Real)) factory.iVMF.Tanh(x.Count, x.DPtr, y.DPtr);
            else if (typeof(T) == typeof(Cplx)) factory.iVMF.Tanh(x.Count, x.VPtr, y.VPtr);
            else throw new NotSupportedException($"Type {typeof(T)} not supported.");
        }


        // obsolete ...
        ///// <summary>
        ///// computes hyperbolic tangent of array elements
        ///// yi = tanh[xi]
        ///// </summary>
        ///// <typeparam name="T"> ArrayBase[double] </typeparam>
        ///// <param name="x"> array x </param>
        ///// <param name="y"> array y </param>
        ///// <param name="checkLength"> whether checks the lengths of input arrays </param>
        //public static void TanhD<T>(T x, ref T y,
        //    bool checkLength = true)
        //    where T : DenseArrayBase<double>
        //{
        //    if (checkLength) { ExArrayLengthD(x, y); }
        //    factory.iVMF.TanhD(x.Count, x, ref y);
        //}

        ///// <summary>
        ///// computes hyperbolic tangent of array elements
        ///// yi = tanh[xi]
        ///// </summary>
        ///// <typeparam name="T"> ArrayBase[Complex] </typeparam>
        ///// <param name="x"> array x </param>
        ///// <param name="y"> array y </param>
        ///// <param name="checkLength"> whether checks the lengths of input arrays </param>
        //public static void TanhZ<T>(T x, ref T y,
        //    bool checkLength = true)
        //    where T : DenseArrayBase<Complex>
        //{
        //    if (checkLength) { ExArrayLengthZ(x, y); }
        //    factory.iVMF.TanhZ(x.Count, x, ref y);
        //}

        /// <summary>
        /// computes hyperbolic tangent of vector elements
        /// </summary>
        /// <param name="x"> input vector x </param>
        /// <returns> result vector </returns>
        public static VectorD Tanh(VectorD x)
        {
            VectorD y = new(count: x.Count, mode: ArrayInitMode.Malloc);
            //TanhD(x, ref y, checkLength: false);
            factory.iVMF.TanhD(x.Count, x, ref y);
            return y;
        }

        /// <summary>
        /// computes hyperbolic tangent of matrix elements
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <returns> result matrix </returns>
        public static MatrixD Tanh(MatrixD x)
        {
            MatrixD y = new(rows: x.Rows, cols: x.Cols, mode: ArrayInitMode.Malloc);
            //TanhD(x, ref y, checkLength: false);
            factory.iVMF.TanhD(x.Count, x, ref y);
            return y;
        }

        /// <summary>
        /// computes hyperbolic tangent of vector elements
        /// </summary>
        /// <param name="x"> input vector x </param>
        /// <returns> result vector </returns>
        public static VectorZ Tanh(VectorZ x)
        {
            VectorZ y = new(count: x.Count, mode: ArrayInitMode.Malloc);
            //TanhZ(x, ref y, checkLength: false);
            factory.iVMF.TanhZ(x.Count, x, ref y);
            return y;
        }

        /// <summary>
        /// computes hyperbolic tangent of matrix elements
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <returns> result matrix </returns>
        public static MatrixZ Tanh(MatrixZ x)
        {
            MatrixZ y = new(rows: x.Rows, cols: x.Cols, mode: ArrayInitMode.Malloc);
            //TanhZ(x, ref y, checkLength: false);
            factory.iVMF.TanhZ(x.Count, x, ref y);
            return y;
        }

        #endregion
        #region ---- Acos [D/Z] ----

        /// <summary>
        /// Computes the arccos of each element in the input array <paramref name="x"/> and stores the result in <paramref name="y"/>.
        /// </summary>
        /// <typeparam name="T">The numeric type of the array elements, implementing <see cref="INumber{T}"/>.</typeparam>
        /// <param name="x">The input array containing the values for which to compute the arccos.</param>
        /// <param name="y">The output array where the computed arccos values will be stored.</param>
        public static void Acos<T>(DenseArray<T> x, ref DenseArray<T> y)
            where T : INumber<T>
            => Acos(x, y);

        internal static void Acos<T>(DenseArray<T> x, DenseArray<T> y)
            where T : INumber<T>
        {
            if (typeof(T) == typeof(Real)) factory.iVMF.Acos(x.Count, x.DPtr, y.DPtr);
            else if (typeof(T) == typeof(Cplx)) factory.iVMF.Acos(x.Count, x.VPtr, y.VPtr);
            else throw new NotSupportedException($"Type {typeof(T)} not supported.");
        }


        // obsolete ...
        ///// <summary>
        ///// computes inverse cosine of array elements
        ///// yi = acos[xi]
        ///// </summary>
        ///// <typeparam name="T"> ArrayBase[double] </typeparam>
        ///// <param name="x"> array x </param>
        ///// <param name="y"> array y </param>
        ///// <param name="checkLength"> whether checks the lengths of input arrays </param>
        //public static void AcosD<T>(T x, ref T y,
        //    bool checkLength = true)
        //    where T : DenseArrayBase<double>
        //{
        //    if (checkLength) { ExArrayLengthD(x, y); }
        //    factory.iVMF.AcosD(x.Count, x, ref y);
        //}

        ///// <summary>
        ///// computes inverse cosine of array elements
        ///// yi = acos[xi]
        ///// </summary>
        ///// <typeparam name="T"> ArrayBase[Complex] </typeparam>
        ///// <param name="x"> array x </param>
        ///// <param name="y"> array y </param>
        ///// <param name="checkLength"> whether checks the lengths of input arrays </param>
        //public static void AcosZ<T>(T x, ref T y,
        //    bool checkLength = true)
        //    where T : DenseArrayBase<Complex>
        //{
        //    if (checkLength) { ExArrayLengthZ(x, y); }
        //    factory.iVMF.AcosZ(x.Count, x, ref y);
        //}

        /// <summary>
        /// computes inverse cosine of vector elements
        /// </summary>
        /// <param name="x"> input vector x </param>
        /// <returns> result vector </returns>
        public static VectorD Acos(VectorD x)
        {
            VectorD y = new(count: x.Count, mode: ArrayInitMode.Malloc);
            //AcosD(x, ref y, checkLength: false);
            factory.iVMF.AcosD(x.Count, x, ref y);
            return y;
        }

        /// <summary>
        /// computes inverse cosine of matrix elements
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <returns> result matrix </returns>
        public static MatrixD Acos(MatrixD x)
        {
            MatrixD y = new(rows: x.Rows, cols: x.Cols, mode: ArrayInitMode.Malloc);
            //AcosD(x, ref y, checkLength: false);
            factory.iVMF.AcosD(x.Count, x, ref y);
            return y;
        }

        /// <summary>
        /// computes inverse cosine of vector elements
        /// </summary>
        /// <param name="x"> input vector x </param>
        /// <returns> result vector </returns>
        public static VectorZ Acos(VectorZ x)
        {
            VectorZ y = new(count: x.Count, mode: ArrayInitMode.Malloc);
            //AcosZ(x, ref y, checkLength: false);
            factory.iVMF.AcosZ(x.Count, x, ref y);
            return y;
        }

        /// <summary>
        /// computes inverse cosine of matrix elements
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <returns> result matrix </returns>
        public static MatrixZ Acos(MatrixZ x)
        {
            MatrixZ y = new(rows: x.Rows, cols: x.Cols, mode: ArrayInitMode.Malloc);
            //AcosZ(x, ref y, checkLength: false);
            factory.iVMF.AcosZ(x.Count, x, ref y);
            return y;
        }

        #endregion
        #region ---- Asin [D/Z] ----

        /// <summary>
        /// Computes the arcsin of each element in the input array <paramref name="x"/> and stores the result in <paramref name="y"/>.
        /// </summary>
        /// <typeparam name="T">The numeric type of the array elements, implementing <see cref="INumber{T}"/>.</typeparam>
        /// <param name="x">The input array containing the values for which to compute the arcsin.</param>
        /// <param name="y">The output array where the computed arcsin values will be stored.</param>
        public static void Asin<T>(DenseArray<T> x, ref DenseArray<T> y)
            where T : INumber<T>
            => Asin(x, y);

        internal static void Asin<T>(DenseArray<T> x, DenseArray<T> y)
            where T : INumber<T>
        {
            if (typeof(T) == typeof(Real)) factory.iVMF.Asin(x.Count, x.DPtr, y.DPtr);
            else if (typeof(T) == typeof(Cplx)) factory.iVMF.Asin(x.Count, x.VPtr, y.VPtr);
            else throw new NotSupportedException($"Type {typeof(T)} not supported.");
        }


        // obsolete ...
        ///// <summary>
        ///// computes inverse sine of array elements
        ///// yi = asin[xi]
        ///// </summary>
        ///// <typeparam name="T"> ArrayBase[double] </typeparam>
        ///// <param name="x"> array x </param>
        ///// <param name="y"> array y </param>
        ///// <param name="checkLength"> whether checks the lengths of input arrays </param>
        //public static void AsinD<T>(T x, ref T y,
        //    bool checkLength = true)
        //    where T : DenseArrayBase<double>
        //{
        //    if (checkLength) { ExArrayLengthD(x, y); }
        //    factory.iVMF.AsinD(x.Count, x, ref y);
        //}

        ///// <summary>
        ///// computes inverse sine of array elements
        ///// yi = asin[xi]
        ///// </summary>
        ///// <typeparam name="T"> ArrayBase[Complex] </typeparam>
        ///// <param name="x"> array x </param>
        ///// <param name="y"> array y </param>
        ///// <param name="checkLength"> whether checks the lengths of input arrays </param>
        //public static void AsinZ<T>(T x, ref T y,
        //    bool checkLength = true)
        //    where T : DenseArrayBase<Complex>
        //{
        //    if (checkLength) { ExArrayLengthZ(x, y); }
        //    factory.iVMF.AsinZ(x.Count, x, ref y);
        //}

        /// <summary>
        /// computes inverse sine of vector elements
        /// </summary>
        /// <param name="x"> input vector x </param>
        /// <returns> result vector </returns>
        public static VectorD ArcSin(VectorD x)
        {
            VectorD y = new(count: x.Count, mode: ArrayInitMode.Malloc);
            //AsinD(x, ref y, checkLength: false);
            factory.iVMF.AsinD(x.Count, x, ref y);
            return y;
        }

        /// <summary>
        /// computes inverse sine of matrix elements
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <returns> result matrix </returns>
        public static MatrixD ArcSin(MatrixD x)
        {
            MatrixD y = new(rows: x.Rows, cols: x.Cols, mode: ArrayInitMode.Malloc);
            //AsinD(x, ref y, checkLength: false);
            factory.iVMF.AsinD(x.Count, x, ref y);
            return y;
        }

        /// <summary>
        /// computes inverse sine of vector elements
        /// </summary>
        /// <param name="x"> input vector x </param>
        /// <returns> result vector </returns>
        public static VectorZ ArcSin(VectorZ x)
        {
            VectorZ y = new(count: x.Count, mode: ArrayInitMode.Malloc);
            //AsinZ(x, ref y, checkLength: false);
            factory.iVMF.AsinZ(x.Count, x, ref y);
            return y;
        }

        /// <summary>
        /// computes inverse sine of matrix elements
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <returns> result matrix </returns>
        public static MatrixZ ArcSin(MatrixZ x)
        {
            MatrixZ y = new(rows: x.Rows, cols: x.Cols);
            //AsinZ(x, ref y, checkLength: false);
            factory.iVMF.AsinZ(x.Count, x, ref y);
            return y;
        }

        #endregion
        #region ---- Atan [D/Z] ----

        /// <summary>
        /// Computes the arctan of each element in the input array <paramref name="x"/> and stores the result in <paramref name="y"/>.
        /// </summary>
        /// <typeparam name="T">The numeric type of the array elements, implementing <see cref="INumber{T}"/>.</typeparam>
        /// <param name="x">The input array containing the values for which to compute the arctan.</param>
        /// <param name="y">The output array where the computed arctan values will be stored.</param>
        public static void Atan<T>(DenseArray<T> x, ref DenseArray<T> y)
            where T : INumber<T>
            => Atan(x, y);

        internal static void Atan<T>(DenseArray<T> x, DenseArray<T> y)
            where T : INumber<T>
        {
            if (typeof(T) == typeof(Real)) factory.iVMF.Atan(x.Count, x.DPtr, y.DPtr);
            else if (typeof(T) == typeof(Cplx)) factory.iVMF.Atan(x.Count, x.VPtr, y.VPtr);
            else throw new NotSupportedException($"Type {typeof(T)} not supported.");
        }


        // obsolete ...
        ///// <summary>
        ///// computes inverse tangent of array elements
        ///// yi = atan[xi]
        ///// </summary>
        ///// <typeparam name="T"> ArrayBase[double] </typeparam>
        ///// <param name="x"> array x </param>
        ///// <param name="y"> array y </param>
        ///// <param name="checkLength"> whether checks the lengths of input arrays </param>
        //public static void AtanD<T>(T x, ref T y,
        //    bool checkLength = true)
        //    where T : DenseArrayBase<double>
        //{
        //    if (checkLength) { ExArrayLengthD(x, y); }
        //    factory.iVMF.AtanD(x.Count, x, ref y);
        //}

        ///// <summary>
        ///// computes inverse tangent of array elements
        ///// yi = atan[xi]
        ///// </summary>
        ///// <typeparam name="T"> ArrayBase[Complex] </typeparam>
        ///// <param name="x"> array x </param>
        ///// <param name="y"> array y </param>
        ///// <param name="checkLength"> whether checks the lengths of input arrays </param>
        //public static void AtanZ<T>(T x, ref T y,
        //    bool checkLength = true)
        //    where T : DenseArrayBase<Complex>
        //{
        //    if (checkLength) { ExArrayLengthZ(x, y); }
        //    factory.iVMF.AtanZ(x.Count, x, ref y);
        //}

        /// <summary>
        /// computes inverse tangent of vector elements
        /// </summary>
        /// <param name="x"> input vector </param>
        /// <returns> result vector </returns>
        public static VectorD Atan(VectorD x)
        {
            VectorD y = new(count: x.Count, mode: ArrayInitMode.Malloc);
            //AtanD(x, ref y, checkLength: false);
            factory.iVMF.AtanD(x.Count, x, ref y);
            return y;
        }

        /// <summary>
        /// computes inverse tangent of matrix elements
        /// </summary>
        /// <param name="x"> input matrix </param>
        /// <returns> result matrix </returns>
        public static MatrixD ArcTan(MatrixD x)
        {
            MatrixD y = new(rows: x.Rows, cols: x.Cols, mode: ArrayInitMode.Malloc);
            //AtanD(x, ref y, checkLength: false);
            factory.iVMF.AtanD(x.Count, x, ref y);
            return y;
        }

        /// <summary>
        /// computes inverse tangent of vector elements
        /// </summary>
        /// <param name="x"> input vector </param>
        /// <returns> result vector </returns>
        public static VectorZ ArcTan(VectorZ x)
        {
            VectorZ y = new(count: x.Count, mode: ArrayInitMode.Malloc);
            //AtanZ(x, ref y, checkLength: false);
            factory.iVMF.AtanZ(x.Count, x, ref y);
            return y;
        }

        /// <summary>
        /// computes inverse tangent of matrix elements
        /// </summary>
        /// <param name="x"> input matrix </param>
        /// <returns> result matrix </returns>
        public static MatrixZ ArcTan(MatrixZ x)
        {
            MatrixZ y = new(rows: x.Rows, cols: x.Cols, mode: ArrayInitMode.Malloc);
            //AtanZ(x, ref y, checkLength: false);
            factory.iVMF.AtanZ(x.Count, x, ref y);
            return y;
        }

        #endregion
        #region ---- AcosPI [D] ----

        /// <summary>
        /// Computes the inverse cosine (in units of π) of each element in the input array <paramref name="a"/>,
        /// and stores the result in the output array <paramref name="y"/>.
        /// y[i] = arccos(x[i])/PI
        /// </summary>
        /// <param name="a">The input dense array containing the values for which to compute the inverse cosine (in units of π).</param>
        /// <param name="y">The output dense array to store the computed results. Must be the same length as <paramref name="a"/>.</param>
        /// <remarks>
        /// This method delegates to the underlying vector math function provider (IVMF) implementation.
        /// </remarks>
        public static void AcosPI(DenseArray<Real> a, ref DenseArray<Real> y)
            => factory.iVMF.Acospi(a.Count, a.DPtr, y.DPtr);

        #endregion
        #region ---- AsinPI [D] ----

        /// <summary>
        /// Computes the inverse sine (in units of π) of each element in the input array <paramref name="a"/>,
        /// and stores the result in the output array <paramref name="y"/>.
        /// y[i] = arcsin(x[i])/PI
        /// </summary>
        /// <param name="a">The input dense array containing the values for which to compute the inverse cosine (in units of π).</param>
        /// <param name="y">The output dense array to store the computed results. Must be the same length as <paramref name="a"/>.</param>
        /// <remarks>
        /// This method delegates to the underlying vector math function provider (IVMF) implementation.
        /// </remarks>
        public static void AsinPI(DenseArray<Real> a, ref DenseArray<Real> y)
            => factory.iVMF.Asinpi(a.Count, a.DPtr, y.DPtr);

        #endregion
        #region ---- AtanPI [D] ----

        /// <summary>
        /// Computes the inverse tangent (in units of π) of each element in the input array <paramref name="a"/>,
        /// and stores the result in the output array <paramref name="y"/>.
        /// y[i] = arctan(x[i])/PI
        /// </summary>
        /// <param name="a">The input dense array containing the values for which to compute the inverse cosine (in units of π).</param>
        /// <param name="y">The output dense array to store the computed results. Must be the same length as <paramref name="a"/>.</param>
        /// <remarks>
        /// This method delegates to the underlying vector math function provider (IVMF) implementation.
        /// </remarks>
        public static void AtanPI(DenseArray<Real> a, ref DenseArray<Real> y)
            => factory.iVMF.Atanpi(a.Count, a.DPtr, y.DPtr);

        #endregion
        #region ---- Acosh [D/Z] ----

        /// <summary>
        /// Computes the acosh of each element in the input array <paramref name="x"/> and stores the result in <paramref name="y"/>.
        /// </summary>
        /// <typeparam name="T">The numeric type of the array elements, implementing <see cref="INumber{T}"/>.</typeparam>
        /// <param name="x">The input array containing the values for which to compute the acosh.</param>
        /// <param name="y">The output array where the computed acosh values will be stored.</param>
        public static void Acosh<T>(DenseArray<T> x, ref DenseArray<T> y)
            where T : INumber<T>
            => Acosh(x, y);

        internal static void Acosh<T>(DenseArray<T> x, DenseArray<T> y)
            where T : INumber<T>
        {
            if (typeof(T) == typeof(Real)) factory.iVMF.Acosh(x.Count, x.DPtr, y.DPtr);
            else if (typeof(T) == typeof(Cplx)) factory.iVMF.Acosh(x.Count, x.VPtr, y.VPtr);
            else throw new NotSupportedException($"Type {typeof(T)} not supported.");
        }


        // obsolete ...
        ///// <summary>
        ///// computes inverse hyperbolic cosine (nonnegative) of array elements
        ///// yi = acosh[xi]
        ///// </summary>
        ///// <typeparam name="T"> ArrayBase[double] </typeparam>
        ///// <param name="x"> array x </param>
        ///// <param name="y"> array y </param>
        ///// <param name="checkLength"> whether checks the lengths of input arrays </param>
        //public static void AcoshD<T>(T x, ref T y,
        //    bool checkLength = true)
        //    where T : DenseArrayBase<double>
        //{
        //    if (checkLength) { ExArrayLengthD(x, y); }
        //    factory.iVMF.AcoshD(x.Count, x, ref y);
        //}

        ///// <summary>
        ///// computes inverse hyperbolic cosine (nonnegative) of array elements
        ///// yi = acosh[xi]
        ///// </summary>
        ///// <typeparam name="T"> ArrayBase[Complex] </typeparam>
        ///// <param name="x"> array x </param>
        ///// <param name="y"> array y </param>
        ///// <param name="checkLength"> whether checks the lengths of input arrays </param>
        //public static void AcoshZ<T>(T x, ref T y,
        //    bool checkLength = true)
        //    where T : DenseArrayBase<Complex>
        //{
        //    if (checkLength) { ExArrayLengthZ(x, y); }
        //    factory.iVMF.AcoshZ(x.Count, x, ref y);
        //}

        /// <summary>
        /// computes inverse hyperbolic cosine (nonnegative) of vector elements
        /// </summary>
        /// <param name="x"> input vector x </param>
        /// <returns> result vector </returns>
        public static VectorD Acosh(VectorD x)
        {
            VectorD y = new(count: x.Count, mode: ArrayInitMode.Malloc);
            //AcoshD(x, ref y, checkLength: false);
            factory.iVMF.AcoshD(x.Count, x, ref y);
            return y;
        }

        /// <summary>
        /// computes inverse hyperbolic cosine (nonnegative) of matrix elements
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <returns> result matrix </returns>
        public static MatrixD ArcCosh(MatrixD x)
        {
            MatrixD y = new(rows: x.Rows, cols: x.Cols, mode: ArrayInitMode.Malloc);
            //AcoshD(x, ref y, checkLength: false);
            factory.iVMF.AcoshD(x.Count, x, ref y);
            return y;
        }

        /// <summary>
        /// computes inverse hyperbolic cosine (nonnegative) of vector elements
        /// </summary>
        /// <param name="x"> input vector x </param>
        /// <returns> result vector </returns>
        public static VectorZ ArcCosh(VectorZ x)
        {
            VectorZ y = new(count: x.Count, mode: ArrayInitMode.Malloc);
            //AcoshZ(x, ref y, checkLength: false);
            factory.iVMF.AcoshZ(x.Count, x, ref y);
            return y;
        }

        /// <summary>
        /// computes inverse hyperbolic cosine (nonnegative) of matrix elements
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <returns> result matrix </returns>
        public static MatrixZ ArcCosh(MatrixZ x)
        {
            MatrixZ y = new(rows: x.Rows, cols: x.Cols, mode: ArrayInitMode.Malloc);
            //AcoshZ(x, ref y, checkLength: false);
            factory.iVMF.AcoshZ(x.Count, x, ref y);
            return y;
        }

        #endregion
        #region ---- Asinh [D/Z] ----

        /// <summary>
        /// Computes the asinh of each element in the input array <paramref name="x"/> and stores the result in <paramref name="y"/>.
        /// </summary>
        /// <typeparam name="T">The numeric type of the array elements, implementing <see cref="INumber{T}"/>.</typeparam>
        /// <param name="x">The input array containing the values for which to compute the asinh.</param>
        /// <param name="y">The output array where the computed asinh values will be stored.</param>
        public static void Asinh<T>(DenseArray<T> x, ref DenseArray<T> y)
            where T : INumber<T>
            => Asinh(x, y);

        internal static void Asinh<T>(DenseArray<T> x, DenseArray<T> y)
            where T : INumber<T>
        {
            if (typeof(T) == typeof(Real)) factory.iVMF.Asinh(x.Count, x.DPtr, y.DPtr);
            else if (typeof(T) == typeof(Cplx)) factory.iVMF.Asinh(x.Count, x.VPtr, y.VPtr);
            else throw new NotSupportedException($"Type {typeof(T)} not supported.");
        }


        // obsolete ...
        ///// <summary>
        ///// computes inverse hyperbolic sine of array elements
        ///// yi = asinh[xi]
        ///// </summary>
        ///// <typeparam name="T"> ArrayBase[double] </typeparam>
        ///// <param name="x"> array x </param>
        ///// <param name="y"> array y </param>
        ///// <param name="checkLength"> whether checks the lengths of input arrays </param>
        //public static void AsinhD<T>(T x, ref T y,
        //    bool checkLength = true)
        //    where T : DenseArrayBase<double>
        //{
        //    if (checkLength) { ExArrayLengthD(x, y); }
        //    factory.iVMF.AsinhD(x.Count, x, ref y);
        //}

        ///// <summary>
        ///// computes inverse hyperbolic sine of array elements
        ///// yi = asinh[xi]
        ///// </summary>
        ///// <typeparam name="T"> ArrayBase[Complex] </typeparam>
        ///// <param name="x"> array x </param>
        ///// <param name="y"> array y </param>
        ///// <param name="checkLength"> whether checks the lengths of input arrays </param>
        //public static void AsinhZ<T>(T x, ref T y,
        //    bool checkLength = true)
        //    where T : DenseArrayBase<Complex>
        //{
        //    if (checkLength) { ExArrayLengthZ(x, y); }
        //    factory.iVMF.AsinhZ(x.Count, x, ref y);
        //}

        /// <summary>
        /// computes inverse hyperbolic sine of vector elements
        /// </summary>
        /// <param name="x"> input vector x </param>
        /// <returns> result vector </returns>
        public static VectorD ArcSinh(VectorD x)
        {
            VectorD y = new(count: x.Count, mode: ArrayInitMode.Malloc);
            //AsinhD(x, ref y, checkLength: false);
            factory.iVMF.AsinhD(x.Count, x, ref y);
            return y;
        }

        /// <summary>
        /// computes inverse hyperbolic sine of matrix elements
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <returns> result matrix </returns>
        public static MatrixD ArcSinh(MatrixD x)
        {
            MatrixD y = new(rows: x.Rows, cols: x.Cols, mode: ArrayInitMode.Malloc);
            //AsinhD(x, ref y, checkLength: false);
            factory.iVMF.AsinhD(x.Count, x, ref y);
            return y;
        }

        /// <summary>
        /// computes inverse hyperbolic sine of vector elements
        /// </summary>
        /// <param name="x"> input vector x </param>
        /// <returns> result vector </returns>
        public static VectorZ ArcSinh(VectorZ x)
        {
            VectorZ y = new(count: x.Count, mode: ArrayInitMode.Malloc);
            //AsinhZ(x, ref y, checkLength: false);
            factory.iVMF.AsinhZ(x.Count, x, ref y);
            return y;
        }

        /// <summary>
        /// computes inverse hyperbolic sine of matrix elements
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <returns> result matrix </returns>
        public static MatrixZ ArcSinh(MatrixZ x)
        {
            MatrixZ y = new(rows: x.Rows, cols: x.Cols, mode: ArrayInitMode.Malloc);
            //AsinhZ(x, ref y, checkLength: false);
            factory.iVMF.AsinhZ(x.Count, x, ref y);
            return y;
        }

        #endregion
        #region ---- Atanh [D/Z] ----

        /// <summary>
        /// Computes the atanh of each element in the input array <paramref name="x"/> and stores the result in <paramref name="y"/>.
        /// </summary>
        /// <typeparam name="T">The numeric type of the array elements, implementing <see cref="INumber{T}"/>.</typeparam>
        /// <param name="x">The input array containing the values for which to compute the atanh.</param>
        /// <param name="y">The output array where the computed atanh values will be stored.</param>
        public static void Atanh<T>(DenseArray<T> x, ref DenseArray<T> y)
            where T : INumber<T>
            => Atanh(x, y);

        internal static void Atanh<T>(DenseArray<T> x, DenseArray<T> y)
            where T : INumber<T>
        {
            if (typeof(T) == typeof(Real)) factory.iVMF.Atanh(x.Count, x.DPtr, y.DPtr);
            else if (typeof(T) == typeof(Cplx)) factory.iVMF.Atanh(x.Count, x.VPtr, y.VPtr);
            else throw new NotSupportedException($"Type {typeof(T)} not supported.");
        }


        // obsolete ...
        ///// <summary>
        ///// computes inverse hyperbolic tangent of array elements
        ///// yi = atanh[xi]
        ///// </summary>
        ///// <typeparam name="T"> ArrayBase[double] </typeparam>
        ///// <param name="x"> array x </param>
        ///// <param name="y"> array y </param>
        ///// <param name="checkLength"> whether checks the lengths of input arrays </param>
        //public static void AtanhD<T>(T x, ref T y,
        //    bool checkLength = true)
        //    where T : DenseArrayBase<double>
        //{
        //    if (checkLength) { ExArrayLengthD(x, y); }
        //    factory.iVMF.AtanhD(x.Count, x, ref y);
        //}

        ///// <summary>
        ///// computes inverse hyperbolic tangent of array elements
        ///// yi = atanh[xi]
        ///// </summary>
        ///// <typeparam name="T"> ArrayBase[Complex] </typeparam>
        ///// <param name="x"> array x </param>
        ///// <param name="y"> array y </param>
        ///// <param name="checkLength"> whether checks the lengths of input arrays </param>
        //public static void AtanhZ<T>(T x, ref T y,
        //    bool checkLength = true)
        //    where T : DenseArrayBase<Complex>
        //{
        //    if (checkLength) { ExArrayLengthZ(x, y); }
        //    factory.iVMF.AtanhZ(x.Count, x, ref y);
        //}

        /// <summary>
        /// computes inverse hyperbolic tangent of vector elements
        /// </summary>
        /// <param name="x"> input vector x </param>
        /// <returns> result vector </returns>
        public static VectorD ArcTanh(VectorD x)
        {
            VectorD y = new(count: x.Count, mode: ArrayInitMode.Malloc);
            //AtanhD(x, ref y, checkLength: false);
            factory.iVMF.AtanhD(x.Count, x, ref y);
            return y;
        }

        /// <summary>
        /// computes inverse hyperbolic tangent of matrix elements
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <returns> result matrix </returns>
        public static MatrixD ArcTanh(MatrixD x)
        {
            MatrixD y = new(rows: x.Rows, cols: x.Cols, mode: ArrayInitMode.Malloc);
            //AtanhD(x, ref y, checkLength: false);
            factory.iVMF.AtanhD(x.Count, x, ref y);
            return y;
        }

        /// <summary>
        /// computes inverse hyperbolic tangent of vector elements
        /// </summary>
        /// <param name="x"> input vector x </param>
        /// <returns> result vector </returns>
        public static VectorZ ArcTanh(VectorZ x)
        {
            VectorZ y = new(count: x.Count, mode: ArrayInitMode.Malloc);
            //AtanhZ(x, ref y, checkLength: false);
            factory.iVMF.AtanhZ(x.Count, x, ref y);
            return y;
        }

        /// <summary>
        /// computes inverse hyperbolic tangent of matrix elements
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <returns> result matrix </returns>
        public static MatrixZ ArcTanh(MatrixZ x)
        {
            MatrixZ y = new(rows: x.Rows, cols: x.Cols, mode: ArrayInitMode.Malloc);
            //AtanhZ(x, ref y, checkLength: false);
            factory.iVMF.AtanhZ(x.Count, x, ref y);
            return y;
        }

        #endregion
        #region ---- Erf [D] ----

        /// <summary>
        /// Provides mathematical operations for dense arrays of double-precision floating-point numbers.
        /// Computes the error function (erf) for each element in the input array <paramref name="x"/> and stores the result in <paramref name="y"/>.
        /// y[i] = erf(x[i])
        /// </summary>
        /// <param name="x">The input dense array of double-precision floating-point numbers.</param>
        /// <param name="y">The dense array to store the computed error function values.</param>
        /// <remarks>
        /// This method delegates to the underlying vector math function provider (IVMF) implementation.
        /// </remarks>
        public static void Erf(DenseArray<Real> x, ref DenseArray<Real> y)
            => factory.iVMF.Erf(x.Count, x.DPtr, y.DPtr);

        #endregion
        #region ---- ErfInv [D] ----

        /// <summary>
        /// Computes the inverse error function for each element in the input array.
        /// y[i] = erfinv(x[i])
        /// </summary>
        /// <remarks>The inverse error function, denoted as <c>erfinv</c>, is the inverse of the error
        /// function <c>erf</c>.  For each element in the input array <paramref name="x"/>, the corresponding result is
        /// stored in the output array <paramref name="y"/>.</remarks>
        /// <param name="x">The input array of double-precision floating-point values for which the inverse error function will be
        /// computed.</param>
        /// <param name="y">A reference to the output array where the computed results will be stored.</param>
        public static void ErfInv(DenseArray<Real> x, ref DenseArray<Real> y)
            => factory.iVMF.ErfInv(x.Count, x.DPtr, y.DPtr);

        #endregion
        #region ---- Hypt [D] ----

        /// <summary>
        /// Computes the element-wise hypotenuse of two input arrays and stores the result in the output array.
        /// z[i] = sqrt(x[i]^2 + y[i]^2)
        /// </summary>
        /// <remarks>This method calculates the hypotenuse for each pair of corresponding elements in the
        /// input arrays  <paramref name="x"/> and <paramref name="y"/> using the formula √(x² + y²). The result is
        /// stored  in the output array <paramref name="z"/>. The input and output arrays must have the same
        /// length.</remarks>
        /// <param name="x">The first input array containing double-precision floating-point values.</param>
        /// <param name="y">The second input array containing double-precision floating-point values.</param>
        /// <param name="z">The output array where the computed hypotenuse values will be stored.  Must have the same length as
        /// <paramref name="x"/> and <paramref name="y"/>.</param>
        public static void Hypt(DenseArray<Real> x, DenseArray<Real> y, 
            ref DenseArray<Real> z)
            => factory.iVMF.Hypot(x.Count, x.DPtr, y.DPtr, z.DPtr);


        // obsolete ...
        ///// <summary>
        ///// computes a square root of sum of two squared elements
        ///// zi = sqrt[xi*xi + yi*yi]
        ///// </summary>
        ///// <typeparam name="T"> ArrayBase[double] </typeparam>
        ///// <param name="x"> array x </param>
        ///// <param name="y"> array y </param>
        ///// <param name="z"> array z </param>
        ///// <param name="checkLength"> whether checks the lengths of input arrays </param>
        //public static void HyptD<T>(T x, T y, ref T z,
        //    bool checkLength = true)
        //    where T : DenseArrayBase<double>
        //{
        //    if (checkLength) { ExArrayLengthD(x, y); }
        //    if (checkLength) { ExArrayLengthD(x, z); }
        //    factory.iVMF.HypotD(x.Count, x, y, ref z);
        //}

        /// <summary>
        /// computes a square root of sum of two squared elements
        /// </summary>
        /// <param name="x"> input vector x </param>
        /// <param name="y"> input vector y </param>
        /// <param name="checkLength"> whether checks the lengths of input arrays </param>
        /// <returns> result vector </returns>
        public static VectorD Hypt(VectorD x, VectorD y,
            bool checkLength = true)
        {
            if (checkLength) { ExArrayLengthD(x, y); }
            VectorD z = new(count: x.Count, mode: ArrayInitMode.Malloc);
            //HyptD(x, y, ref z, checkLength: false);
            factory.iVMF.HypotD(x.Count, x, y, ref z);
            return z;
        }

        /// <summary>
        /// computes a square root of sum of two squared elements
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <param name="y"> input matrix y </param>
        /// <param name="checkLength"> whether checks the lengths of input arrays </param>
        /// <returns> result matrix </returns>
        public static MatrixD Hypt(MatrixD x, MatrixD y,
            bool checkLength = true)
        {
            if (checkLength) { ExArrayLengthD(x, y); }
            MatrixD z = new(rows: x.Rows, cols: x.Cols, mode: ArrayInitMode.Malloc);
            //HyptD(x, y, ref z, checkLength: false);
            factory.iVMF.HypotD(x.Count, x, y, ref z);
            return z;
        }

        #endregion
        #region ---- Erfc [D] ----

        /// <summary>
        /// Computes the complementary error function for each element in the input array.
        /// </summary>
        /// <remarks>The complementary error function is defined as 1 - erf(x), where erf(x) is the error
        /// function. This method computes the complementary error function for each element in the input array
        /// <paramref name="x"/>  and stores the results in the output array <paramref name="y"/>.</remarks>
        /// <param name="x">The input array of double-precision floating-point values.</param>
        /// <param name="y">A reference to the output array where the results will be stored.</param>
        public static void Erfc(DenseArray<Real> x, ref DenseArray<Real> y)
            => factory.iVMF.Erfc(x.Count, x.DPtr, y.DPtr);

        #endregion
        #region ---- ErfcInv [D] ----

        /// <summary>
        /// Computes the inverse complementary error function for each element in the input array.
        /// </summary>
        /// <remarks>This method calculates the inverse complementary error function for each element in
        /// the input array <paramref name="x"/> and stores the results in the output array <paramref name="y"/>. The
        /// input and output arrays must have the same length.</remarks>
        /// <param name="x">The input array of double-precision floating-point values for which the inverse complementary error function
        /// will be computed.</param>
        /// <param name="y">The output array where the computed results will be stored.</param>
        public static void ErfcInv(DenseArray<Real> x, ref DenseArray<Real> y)
            => factory.iVMF.ErfcInv(x.Count, x.DPtr, y.DPtr);

        #endregion
        #region ---- Erfcx [D] ----

        /// <summary>
        /// Computes the scaled complementary error function for the elements of the input array.
        /// </summary>
        /// <remarks>The scaled complementary error function, often denoted as <c>erfcx(x)</c>, is defined
        /// as: <c>erfcx(x) = exp(x^2) * erfc(x)</c>, where <c>erfc(x)</c> is the complementary error function. This
        /// method computes the function element-wise for all values in the input array <paramref name="x"/>  and stores
        /// the results in the output array <paramref name="y"/>.</remarks>
        /// <param name="x">The input array of double-precision values for which the scaled complementary error function will be
        /// computed.</param>
        /// <param name="y">A reference to the output array where the computed results will be stored.</param>
        public static void Erfcx(DenseArray<Real> x, ref DenseArray<Real> y)
            => factory.iVMF.Erfcx(x.Count, x.DPtr, y.DPtr);

        #endregion
        #region ---- CdfNorm [D] ----

        /// <summary>
        /// Computes the cumulative distribution function (CDF) of the standard normal distribution for each element in
        /// the input array <paramref name="x"/> and stores the results in <paramref name="y"/>.
        /// </summary>
        /// <remarks>The method calculates the CDF for each element in the input array and writes the
        /// results to the corresponding positions in the output array. The input and output arrays must have the same
        /// length, and the output array must be pre-allocated before calling this method.</remarks>
        /// <param name="x">The input array containing the values for which to compute the CDF.</param>
        /// <param name="y">The output array where the computed CDF values will be stored.</param>
        public static void CdfNorm(DenseArray<Real> x, ref DenseArray<Real> y)
            => factory.iVMF.CdfNorm(x.Count, x.DPtr, y.DPtr);

        #endregion
        #region ---- CdfNormInv [D] ----

        /// <summary>
        /// Computes the inverse of the cumulative distribution function (quantile function)  of the standard normal
        /// distribution for each element in the input array.
        /// </summary>
        /// <remarks>This method calculates the quantile (inverse CDF) for the standard normal
        /// distribution,  which is useful in statistical applications such as hypothesis testing or generating  random
        /// samples from a normal distribution.</remarks>
        /// <param name="x">The input array containing probability values, where each value must be in the range [0, 1].</param>
        /// <param name="y">The output array where the computed quantile values will be stored.</param>
        public static void CdfNormInv(DenseArray<Real> x, ref DenseArray<Real> y)
            => factory.iVMF.CdfNormInv(x.Count, x.DPtr, y.DPtr);

        #endregion
        #region ---- LGamma [D] ----

        /// <summary>
        /// Computes the natural logarithm of the gamma function for each element in the input array.
        /// </summary>
        /// <remarks>This method calculates the natural logarithm of the gamma function for each element
        /// in the input array <paramref name="x"/>  and stores the results in the output array <paramref name="y"/>.
        /// The input and output arrays must have the same length.</remarks>
        /// <param name="x">A <see cref="DenseArray{T}"/> of double values representing the input array. Each element is used as input
        /// to the gamma function.</param>
        /// <param name="y">A reference to a <see cref="DenseArray{T}"/> of double values where the computed results will be stored.</param>
        public static void LGamma(DenseArray<Real> x, ref DenseArray<Real> y)
            => factory.iVMF.LGamma(x.Count, x.DPtr, y.DPtr);

        #endregion
        #region ---- TGamma [D] ----

        /// <summary>
        /// Computes the gamma function for each element in the input array <paramref name="x"/>  and stores the results
        /// in the output array <paramref name="y"/>.
        /// </summary>
        /// <remarks>The gamma function is defined as an extension of the factorial function to real and
        /// complex numbers.  This method computes the gamma function for all elements in the input array and writes the
        /// results  to the corresponding indices in the output array.</remarks>
        /// <param name="x">The input array containing the values for which the gamma function will be computed.</param>
        /// <param name="y">The output array where the computed gamma function values will be stored.</param>
        public static void TGamma(DenseArray<Real> x, ref DenseArray<Real> y)
            => factory.iVMF.TGamma(x.Count, x.DPtr, y.DPtr);

        #endregion
        #region ---- I0 [D] ----

        /// <summary>
        /// Computes the modified Bessel function of the first kind of order zero (I0) for each element in the input
        /// array.
        /// </summary>
        /// <remarks>The modified Bessel function of the first kind of order zero (I0) is commonly used in
        /// mathematical and scientific computations. This method processes all elements in the input array <paramref
        /// name="x"/> and stores the results in the output array <paramref name="y"/>. Ensure that the output array is
        /// properly initialized and has sufficient capacity to store the results.</remarks>
        /// <param name="x">The input array of double values for which the I0 function will be computed.</param>
        /// <param name="y">A reference to the output array where the computed I0 values will be stored.</param>
        public static void I0(DenseArray<Real> x, ref DenseArray<Real> y)
            => factory.iVMF.I0(x.Count, x.DPtr, y.DPtr);

        #endregion
        #region ---- I1 [D] ----

        /// <summary>
        /// Computes the modified Bessel function of the first kind, order 1, for each element in the input array.
        /// </summary>
        /// <remarks>The method calculates the modified Bessel function of the first kind, order 1, for
        /// each element in the input array <paramref name="x"/> and stores the results in the output array <paramref
        /// name="y"/>. The input and output arrays must have the same length.</remarks>
        /// <param name="x">The input array of double-precision floating-point values for which the modified Bessel function will be
        /// computed.</param>
        /// <param name="y">The output array where the computed values will be stored.</param>
        public static void I1(DenseArray<Real> x, ref DenseArray<Real> y)
            => factory.iVMF.I1(x.Count, x.DPtr, y.DPtr);

        #endregion
        #region ---- J0 [D] ----

        /// <summary>
        /// Computes the Bessel function of the first kind of order 0 (J0) for each element in the input array.
        /// </summary>
        /// <remarks>The method calculates the J0 function for each element in the input array <paramref
        /// name="x"/>  and stores the results in the corresponding positions of the output array <paramref name="y"/>.
        /// Ensure that the input and output arrays are properly initialized and have matching lengths before calling
        /// this method.</remarks>
        /// <param name="x">The input array of double values for which the J0 function will be computed.</param>
        /// <param name="y">A reference to the output array where the computed J0 values will be stored.</param>
        public static void J0(DenseArray<Real> x, ref DenseArray<Real> y)
            => factory.iVMF.J0(x.Count, x.DPtr, y.DPtr);

        #endregion
        #region ---- J1 [D] ----

        /// <summary>
        /// Computes the Bessel function of the first kind of order 1 for each element in the input array.
        /// </summary>
        /// <remarks>This method calculates the Bessel function of the first kind of order 1 (J₁) for each
        /// element in the input array. The results are stored in the corresponding positions of the output array. The
        /// caller must ensure that the input and output arrays are properly allocated and of equal length.</remarks>
        /// <param name="x">The input array of double-precision floating-point values for which the Bessel function will be computed.</param>
        /// <param name="y">The output array where the computed results will be stored.</param>
        public static void J1(DenseArray<Real> x, ref DenseArray<Real> y)
            => factory.iVMF.J1(x.Count, x.DPtr, y.DPtr);

        #endregion
        #region ---- Y0 [D] ----

        /// <summary>
        /// Computes the Bessel function of the second kind, order zero, for each element in the input array.
        /// </summary>
        /// <remarks>The method calculates the Bessel function of the second kind, order zero (Y₀), for
        /// each element in the input array <paramref name="x"/>.  The results are stored in the output array <paramref
        /// name="y"/>.  Ensure that the input and output arrays are properly initialized and have matching lengths
        /// before calling this method.</remarks>
        /// <param name="x">The input array of double-precision floating-point values for which the Bessel function will be computed.</param>
        /// <param name="y">A reference to the output array where the computed results will be stored.</param>
        public static void Y0(DenseArray<Real> x, ref DenseArray<Real> y)
            => factory.iVMF.Y0(x.Count, x.DPtr, y.DPtr);

        #endregion
        #region ---- Y1 [D] ----

        /// <summary>
        /// Computes the Bessel function of the second kind, order one (Y1), for each element in the input array.
        /// </summary>
        /// <remarks>The Bessel function of the second kind, order one (Y1), is computed for each element
        /// in the input array <paramref name="x"/>.  The results are stored in the corresponding positions of the
        /// output array <paramref name="y"/>.</remarks>
        /// <param name="x">The input array of double-precision floating-point values for which the Y1 function will be computed.</param>
        /// <param name="y">A reference to the output array where the computed Y1 values will be stored.</param>
        public static void Y1(DenseArray<Real> x, ref DenseArray<Real> y)
            => factory.iVMF.Y1(x.Count, x.DPtr, y.DPtr);

        #endregion
        #region ---- Jn [D] ----

        /// <summary>
        /// Computes the Bessel function of the first kind, Jn, for each element in the input array.
        /// </summary>
        /// <remarks>This method calculates the Bessel function of the first kind, Jn, for each element in
        /// the input array <paramref name="x"/> using the scalar argument <paramref name="b"/>. The results are stored
        /// in the output array <paramref name="y"/>.</remarks>
        /// <param name="x">A <see cref="DenseArray{T}"/> of double values representing the orders for the Bessel function.</param>
        /// <param name="b">A scalar double value representing the argument for the Bessel function.</param>
        /// <param name="y">A reference to a <see cref="DenseArray{T}"/> of double values where the computed results will be stored.</param>
        public static void Jn(DenseArray<Real> x, Real b, ref DenseArray<Real> y)
            => factory.iVMF.Jn(x.Count, x.DPtr, b, y.DPtr);

        #endregion
        #region ---- Yn [D] ----

        /// <summary>
        /// Computes the Bessel function of the second kind, Yn, for each element in the input array.
        /// </summary>
        /// <remarks>This method evaluates the Bessel function of the second kind, Yn, for each element in
        /// the input array <paramref name="x"/>  using the specified value <paramref name="b"/>. The results are stored
        /// in the output array <paramref name="y"/>.</remarks>
        /// <param name="x">The input array containing the orders for the Bessel function.</param>
        /// <param name="b">The value at which the Bessel function is evaluated for all elements in the input array.</param>
        /// <param name="y">A reference to the output array where the computed results are stored.</param>
        public static void Yn(DenseArray<Real> x, Real b, ref DenseArray<Real> y)
            => factory.iVMF.Yn(x.Count, x.DPtr, b, y.DPtr);

        #endregion
        #region ---- Atan2 [D] ----

        /// <summary>
        /// Computes the element-wise arctangent of the quotient of two arrays.
        /// z[i] = arctan(x[i]/y[i])
        /// </summary>
        /// <remarks>This method calculates the arctangent of the quotient of corresponding elements in
        /// <paramref name="x"/> and <paramref name="y"/>  and stores the results in <paramref name="z"/>. The operation
        /// is performed element-wise, and all arrays must have the same length.</remarks>
        /// <param name="x">The array containing the numerator values for the arctangent computation.</param>
        /// <param name="y">The array containing the denominator values for the arctangent computation.</param>
        /// <param name="z">The array to store the resulting arctangent values, in radians. Must have the same length as <paramref
        /// name="x"/> and <paramref name="y"/>.</param>
        public static void Atan2(DenseArray<Real> x, DenseArray<Real> y, 
            ref DenseArray<Real> z)
            => factory.iVMF.Atan2(x.Count, x.DPtr, y.DPtr, z.DPtr);


        // wrapper
        ///// <summary>
        ///// computes four-quadrant inverse tangent of elements of two arrays
        ///// zi = atan2(xi, yi)
        ///// </summary>
        ///// <typeparam name="T"> ArrayBase[double] </typeparam>
        ///// <param name="x"> array x </param>
        ///// <param name="y"> array y </param>
        ///// <param name="z"> array z </param>
        ///// <param name="checkLength"> whether checks the lengths of input arrays </param>
        //public static void Atan2D<T>(T x, T y, ref T z,
        //    bool checkLength = true)
        //    where T : DenseArrayBase<double>
        //{
        //    if (checkLength) { ExArrayLengthD(x, y); }
        //    if (checkLength) { ExArrayLengthD(x, z); }
        //    factory.iVMF.Atan2D(x.Count, x, y, ref z);
        //}

        /// <summary>
        /// computes four-quadrant inverse tangent of elements of two vectors
        /// </summary>
        /// <param name="x"> input vector x </param>
        /// <param name="y"> input vector y </param>
        /// <param name="checkLength"> whether checks the lengths of input arrays </param>
        /// <returns> result vector </returns>
        public static VectorD Atan2(VectorD x, VectorD y,
            bool checkLength = true)
        {
            if (checkLength) { ExArrayLengthD(x, y); }
            VectorD z = new(count: x.Count, mode: ArrayInitMode.Malloc);
            //Atan2D(x, y, ref z, checkLength: false);
            factory.iVMF.Atan2D(x.Count, x, y, ref z);
            return z;
        }

        /// <summary>
        /// computes four-quadrant inverse tangent of elements of two matrices
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <param name="y"> input matrix y </param>
        /// <param name="checkLength"> whether checks the lengths of input arrays </param>
        /// <returns> result matrix </returns>
        public static MatrixD ArcTan2(MatrixD x, MatrixD y,
            bool checkLength = true)
        {
            if (checkLength) { ExArrayLengthD(x, y); }
            MatrixD z = new(rows: x.Rows, cols: x.Cols, mode: ArrayInitMode.Malloc);
            //Atan2D(x, y, ref z, checkLength: false);
            factory.iVMF.Atan2D(x.Count, x, y, ref z);
            return z;
        }

        #endregion
        #region ---- Atan2PI [D] ----

        /// <summary>
        /// Computes the four-quadrant inverse tangent (atan2) of corresponding elements in two input arrays, divided by π.
        /// z[i] = arctan(x[i]/z[i]) / PI
        /// </summary>
        /// <remarks>The method calculates the angle in radians, normalized by π, for each pair of
        /// elements in the input arrays. The input arrays <paramref name="x"/> and <paramref name="y"/> must have the
        /// same length as the output array  <paramref name="y"/>. The operation is performed element-wise.</remarks>
        /// <param name="x">The first input array containing the numerator values.</param>
        /// <param name="y">The second input array containing the denominator values.</param>
        /// <param name="z">A reference to the output array where the results are stored. Each element is computed as  <c>atan2(a[i],
        /// b[i]) / π</c>.</param>
        public static void Atan2PI(DenseArray<Real> x, DenseArray<Real> y, 
            ref DenseArray<Real> z)
            => factory.iVMF.Atan2pi(x.Count, x.DPtr, y.DPtr, z.DPtr);

        #endregion
        #region ---- Mul [D/Z] ----

        /// <summary>
        /// Performs element-wise multiplication of two dense arrays.
        /// </summary>
        /// <typeparam name="T">
        /// The numeric type of the array elements. Must implement <see cref="INumber{T}"/>.
        /// </typeparam>
        /// <param name="x">The first input dense array.</param>
        /// <param name="y">The second input dense array.</param>
        /// <param name="z">The result dense array, which will contain the product of <paramref name="x"/> and <paramref name="y"/>.</param>
        public static void Mul<T>(DenseArray<T> x, DenseArray<T> y, ref DenseArray<T> z)
            where T : INumber<T>
            => Mul(x, y, z);

        internal static void Mul<T>(DenseArray<T> x, DenseArray<T> y, 
            DenseArray<T> z)
            where T : INumber<T>
        {
            if (typeof(T) == typeof(Real)) factory.iVMF.Mul(x.Count, x.DPtr, y.DPtr, z.DPtr);
            else if (typeof(T) == typeof(Cplx)) factory.iVMF.Mul(x.Count, x.VPtr, y.VPtr, z.VPtr);
            else throw new NotSupportedException($"Type {typeof(T)} not supported.");
        }


        // obsolete ...
        ///// <summary>
        ///// performs element by element multiplication of array x and array y
        ///// zi = xi * yi
        ///// </summary>
        ///// <typeparam name="T"> ArrayBase[double] </typeparam>
        ///// <param name="x"> array x </param>
        ///// <param name="y"> array y </param>
        ///// <param name="z"> array z </param>
        ///// <param name="checkLength"> whether checks the lengths of input arrays </param>
        //public static void MulD<T>(T x, T y, ref T z,
        //    bool checkLength = true) 
        //    where T : DenseArrayBase<double>
        //{
        //    if (checkLength) { ExArrayLengthD(x, y); }
        //    if (checkLength) { ExArrayLengthD(x, z); }
        //    factory.iVMF.MulD(x.Count, x, y, ref z);
        //}

        ///// <summary>
        ///// performs element by element multiplication of array x and array y
        ///// zi = xi * yi
        ///// </summary>
        ///// <typeparam name="T"> ArrayBase[Complex] </typeparam>
        ///// <param name="x"> array x </param>
        ///// <param name="y"> array y </param>
        ///// <param name="z"> array z </param>
        ///// <param name="checkLength"> whether checks the lengths of input arrays </param>
        //public static void MulZ<T>(T x, T y, ref T z,
        //    bool checkLength = true)
        //    where T : DenseArrayBase<Complex>
        //{
        //    if (checkLength) { ExArrayLengthZ(x, y); }
        //    if (checkLength) { ExArrayLengthZ(x, z); }
        //    factory.iVMF.MulZ(x.Count, x, y, ref z);
        //}

        /// <summary>
        /// performs element by element multiplication of vector x and vector y
        /// </summary>
        /// <param name="x"> input vector x </param>
        /// <param name="y"> input vector y </param>
        /// <param name="checkLength"> whether checks the lengths of input arrays </param>
        /// <returns> result vector </returns>
        public static VectorD Mul(VectorD x, VectorD y,
            bool checkLength = true)
        {
            if (checkLength) { ExArrayLengthD(x, y); }
            VectorD z = new(count: x.Count, mode: ArrayInitMode.Malloc);
            //MulD(x, y, ref z, checkLength: false);
            factory.iVMF.MulD(x.Count, x, y, ref z);
            return z;
        }

        /// <summary>
        /// performs element by element multiplication of matrix x and matrix y
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <param name="y"> input matrix y </param>
        /// <param name="checkLength"> whether checks the lengths of input arrays </param>
        /// <returns> result matrix </returns>
        public static MatrixD Mul(MatrixD x, MatrixD y,
            bool checkLength = true)
        {
            if (checkLength) { ExArrayLengthD(x, y); }
            MatrixD z = new(rows: x.Rows, cols: x.Cols, mode: ArrayInitMode.Malloc);
            //MulD(x, y, ref z, checkLength: false);
            factory.iVMF.MulD(x.Count, x, y, ref z);
            return z;
        }

        /// <summary>
        /// performs element by element multiplication of vector x and vector y
        /// </summary>
        /// <param name="x"> input vector x </param>
        /// <param name="y"> input vector y </param>
        /// <param name="checkLength"> whether checks the lengths of input arrays </param>
        /// <returns> result vector </returns>
        public static VectorZ Mul(VectorZ x, VectorZ y,
            bool checkLength = true)
        {
            if (checkLength) { ExArrayLengthZ(x, y); }
            VectorZ z = new(count: x.Count, mode: ArrayInitMode.Malloc);
            //MulZ(x, y, ref z, checkLength: false);
            factory.iVMF.MulZ(x.Count, x, y, ref z);
            return z;
        }

        /// <summary>
        /// performs element by element multiplication of matrix x and matrix y
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <param name="y"> input matrix y </param>
        /// <param name="checkLength"> whether checks the lengths of input arrays </param>
        /// <returns> result matrix </returns>
        public static MatrixZ Mul(MatrixZ x, MatrixZ y,
            bool checkLength = true)
        {
            if (checkLength) { ExArrayLengthZ(x, y); }
            MatrixZ z = new(rows: x.Rows, cols: x.Cols, mode: ArrayInitMode.Malloc);
            //MulZ(x, y, ref z, checkLength: false);
            factory.iVMF.MulZ(x.Count, x, y, ref z);
            return z;
        }

        /// <summary>
        /// performs element by element multiplication of vector x and vector y
        /// </summary>
        /// <param name="x"> input vector x </param>
        /// <param name="y"> input vector y </param>
        /// <param name="checkLength"> whether checks the lengths of input arrays </param>
        /// <returns> result vector </returns>
        public static VectorZ Mul(VectorZ x, VectorD y,
            bool checkLength = true)
        {
            if (checkLength && (x.Count != y.Count)) 
            { Printer.Warning($"Array lengths not equal"); }
            VectorZ z = new(count: x.Count, mode: ArrayInitMode.Malloc);
            //MulZ(x, ConstructReal(y), ref z, checkLength: false);
            factory.iVMF.MulZ(x.Count, x, ConstructReal(y), ref z);
            return z;
        }

        /// <summary>
        /// performs element by element multiplication of matrix x and matrix y
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <param name="y"> input matrix y </param>
        /// <param name="checkLength"> whether checks the lengths of input arrays </param>
        /// <returns> result matrix </returns>
        public static MatrixZ Mul(MatrixZ x, MatrixD y,
            bool checkLength = true)
        {
            if (checkLength && (x.Count != y.Count))
            { Printer.Warning($"Array lengths not equal"); }
            MatrixZ z = new(rows: x.Rows, cols: x.Cols, mode: ArrayInitMode.Malloc);
            //MulZ(x, ConstructReal(y), ref z, checkLength: false);
            factory.iVMF.MulZ(x.Count, x, ConstructReal(y), ref z);
            return z;
        }

        #endregion
        #region ---- Div [D/Z] ----

        /// <summary>
        /// Performs element-wise division of two dense arrays.
        /// Each element of <paramref name="x"/> is divided by the corresponding element of <paramref name="y"/>,
        /// and the result is stored in <paramref name="z"/>.
        /// </summary>
        /// <typeparam name="T">
        /// The numeric type of the array elements. Must implement <see cref="INumber{T}"/>.
        /// </typeparam>
        /// <param name="x">The dividend dense array.</param>
        /// <param name="y">The divisor dense array.</param>
        /// <param name="z">The result dense array to store the division output.</param>
        public static void Div<T>(DenseArray<T> x, DenseArray<T> y, ref DenseArray<T> z)
            where T : INumber<T>
            => Div(x, y, z);

        internal static void Div<T>(DenseArray<T> x, DenseArray<T> y, 
            DenseArray<T> z)
            where T : INumber<T>
        {
            if (typeof(T) == typeof(Real)) factory.iVMF.Div(x.Count, x.DPtr, y.DPtr, z.DPtr);
            else if (typeof(T) == typeof(Cplx)) factory.iVMF.Div(x.Count, x.VPtr, y.VPtr, z.VPtr);
            else throw new NotSupportedException($"Type {typeof(T)} not supported.");
        }


        // obsolete ...
        ///// <summary>
        ///// performs element by element division of array x by array y
        ///// zi = xi / yi
        ///// </summary>
        ///// <typeparam name="T"> ArrayBase[double] </typeparam>
        ///// <param name="x"> array x </param>
        ///// <param name="y"> array y </param>
        ///// <param name="z"> array z </param>
        ///// <param name="checkLength"> whether checks the lengths of input arrays </param>
        //public static void DivD<T>(T x, T y, ref T z,
        //    bool checkLength = true)
        //    where T : DenseArrayBase<double>
        //{
        //    if (checkLength) { ExArrayLengthD(x, y); }
        //    if (checkLength) { ExArrayLengthD(x, z); }
        //    factory.iVMF.DivD(x.Count, x, y, ref z);
        //}

        ///// <summary>
        ///// performs element by element division of array x by array y
        ///// zi = xi / yi
        ///// </summary>
        ///// <typeparam name="T"> ArrayBase[Complex] </typeparam>
        ///// <param name="x"> array x </param>
        ///// <param name="y"> array y </param>
        ///// <param name="z"> array z </param>
        ///// <param name="checkLength"> whether checks the lengths of input arrays </param>
        //public static void DivZ<T>(T x, T y, ref T z,
        //    bool checkLength = true)
        //    where T : DenseArrayBase<Complex>
        //{
        //    if (checkLength) { ExArrayLengthZ(x, y); }
        //    if (checkLength) { ExArrayLengthZ(x, z); }
        //    factory.iVMF.DivZ(x.Count, x, y, ref z);
        //}

        /// <summary>
        /// performs element by element division of vector x by vector y
        /// </summary>
        /// <param name="x"> input vector x </param>
        /// <param name="y"> input vector y </param>
        /// <param name="checkLength"> whether checks the lengths of input arrays </param>
        /// <returns> result vector </returns>
        public static VectorD Div(VectorD x, VectorD y,
            bool checkLength = true)
        {
            if (checkLength) { ExArrayLengthD(x, y); }
            VectorD z = new(count: x.Count, mode: ArrayInitMode.Malloc);
            //DivD(x, y, ref z, checkLength: false);
            factory.iVMF.DivD(x.Count, x, y, ref z);
            return z;
        }

        /// <summary>
        /// performs element by element division of matrix x by matrix y
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <param name="y"> input matrix y </param>
        /// <param name="checkLength"> whether checks the lengths of input arrays </param>
        /// <returns> result matrix </returns>
        public static MatrixD Div(MatrixD x, MatrixD y,
            bool checkLength = true)
        {
            if (checkLength) { ExArrayLengthD(x, y); }
            MatrixD z = new(rows: x.Rows, cols: x.Cols, mode: ArrayInitMode.Malloc);
            //DivD(x, y, ref z, checkLength: false);
            factory.iVMF.DivD(x.Count, x, y, ref z);
            return z;
        }

        /// <summary>
        /// performs element by element division of vector x by vector y
        /// </summary>
        /// <param name="x"> input vector x </param>
        /// <param name="y"> input vector y </param>
        /// <param name="checkLength"> whether checks the lengths of input arrays </param>
        /// <returns> result vector </returns>
        public static VectorZ Div(VectorZ x, VectorZ y,
            bool checkLength = true)
        {
            if (checkLength) { ExArrayLengthZ(x, y); }
            VectorZ z = new(count: x.Count, mode: ArrayInitMode.Malloc);
            //DivZ(x, y, ref z, checkLength: false);
            factory.iVMF.DivZ(x.Count, x, y, ref z);
            return z;
        }

        /// <summary>
        /// performs element by element division of matrix x by matrix y
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <param name="y"> input matrix y </param>
        /// <param name="checkLength"> whether checks the lengths of input arrays </param>
        /// <returns> result matrix </returns>
        public static MatrixZ Div(MatrixZ x, MatrixZ y,
            bool checkLength = true)
        {
            if (checkLength) { ExArrayLengthZ(x, y); }
            MatrixZ z = new(rows: x.Rows, cols: x.Cols, mode: ArrayInitMode.Malloc);
            //DivZ(x, y, ref z, checkLength: false);
            factory.iVMF.DivZ(x.Count, x, y, ref z);
            return z;
        }

        #endregion
        #region ---- Pow [D/Z] ----

        /// <summary>
        /// Computes the element-wise power of two arrays.
        /// Each element of the output array <paramref name="z"/> is computed as <c>z[i] = x[i] ^ y[i]</c>.
        /// </summary>
        /// <typeparam name="T">
        /// The numeric type of the array elements. Must implement <see cref="INumber{T}"/>.
        /// </typeparam>
        /// <param name="x">The base array.</param>
        /// <param name="y">The exponent array.</param>
        /// <param name="z">The result array. On output, contains the result of raising each element of <paramref name="x"/> to the corresponding power in <paramref name="y"/>.</param>
        public static void Pow<T>(DenseArray<T> x, DenseArray<T> y, ref DenseArray<T> z)
            where T : INumber<T>
            => Pow(x, y, z);

        internal static void Pow<T>(DenseArray<T> x, DenseArray<T> y, 
            DenseArray<T> z)
            where T : INumber<T>
        {
            if (typeof(T) == typeof(Real)) factory.iVMF.Pow(x.Count, x.DPtr, y.DPtr, z.DPtr);
            else if (typeof(T) == typeof(Cplx)) factory.iVMF.Pow(x.Count, x.VPtr, y.VPtr, z.VPtr);
            else throw new NotSupportedException($"Type {typeof(T)} not supported.");
        }


        // obsolete ...
        ///// <summary>
        ///// computes x to the power y for elements of two arrays
        ///// zi = xi^(yi)
        ///// </summary>
        ///// <typeparam name="T"> ArrayBase[double] </typeparam>
        ///// <param name="x"> array x </param>
        ///// <param name="y"> array y </param>
        ///// <param name="z"> array z </param>
        ///// <param name="checkLength"> whether checks the lengths of input arrays </param>
        //public static void PowD<T>(T x, T y, ref T z,
        //    bool checkLength = true)
        //    where T : DenseArrayBase<double>
        //{
        //    if (checkLength) { ExArrayLengthD(x, y); }
        //    if (checkLength) { ExArrayLengthD(x, z); }
        //    factory.iVMF.PowD(x.Count, x, y, ref z);
        //}

        ///// <summary>
        ///// computes x to the power y for elements of two arrays
        ///// zi = xi^(yi)
        ///// </summary>
        ///// <typeparam name="T"> ArrayBase[Complex] </typeparam>
        ///// <param name="x"> array x </param>
        ///// <param name="y"> array y </param>
        ///// <param name="z"> array z </param>
        ///// <param name="checkLength"> whether checks the lengths of input arrays </param>
        //public static void PowZ<T>(T x, T y, ref T z,
        //    bool checkLength = true)
        //    where T : DenseArrayBase<Complex>
        //{
        //    if (checkLength) { ExArrayLengthZ(x, y); }
        //    if (checkLength) { ExArrayLengthZ(x, z); }
        //    factory.iVMF.PowZ(x.Count, x, y, ref z);
        //}

        /// <summary>
        /// computes x to the power y for elements of two vectors
        /// </summary>
        /// <param name="x"> input vector x </param>
        /// <param name="y"> input vector y </param>
        /// <param name="checkLength"> whether checks the lengths of input arrays </param>
        /// <returns> result vector </returns>
        public static VectorD Pow(VectorD x, VectorD y,
            bool checkLength = true)
        {
            if (checkLength) { ExArrayLengthD(x, y); }
            VectorD z = new(count: x.Count, mode: ArrayInitMode.Malloc);
            //PowD(x, y, ref z, checkLength: false);
            factory.iVMF.PowD(x.Count, x, y, ref z);
            return z;
        }

        /// <summary>
        /// computes x to the power y for elements of two matrices
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <param name="y"> input matrix y </param>
        /// <param name="checkLength"> whether checks the lengths of input arrays </param>
        /// <returns> result matrix </returns>
        public static MatrixD Pow(MatrixD x, MatrixD y,
            bool checkLength = true)
        {
            if (checkLength) { ExArrayLengthD(x, y); }
            MatrixD z = new(rows: x.Rows, cols: x.Cols, mode: ArrayInitMode.Malloc);
            //PowD(x, y, ref z, checkLength: false);
            factory.iVMF.PowD(x.Count, x, y, ref z);
            return z;
        }

        /// <summary>
        /// computes x to the power y for elements of two vectors
        /// </summary>
        /// <param name="x"> input vector x </param>
        /// <param name="y"> input vector y </param>
        /// <param name="checkLength"> whether checks the lengths of input arrays </param>
        /// <returns> result vector </returns>
        public static VectorZ Pow(VectorZ x, VectorZ y,
            bool checkLength = true)
        {
            if (checkLength) { ExArrayLengthZ(x, y); }
            VectorZ z = new(count: x.Count, mode: ArrayInitMode.Malloc);
            //PowZ(x, y, ref z, checkLength: false);
            factory.iVMF.PowZ(x.Count, x, y, ref z);
            return z;
        }

        /// <summary>
        /// computes x to the power y for elements of two matrices
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <param name="y"> input matrix y </param>
        /// <param name="checkLength"> whether checks the lengths of input arrays </param>
        /// <returns> result matrix </returns>
        public static MatrixZ Pow(MatrixZ x, MatrixZ y,
            bool checkLength = true)
        {
            if (checkLength) { ExArrayLengthZ(x, y); }
            MatrixZ z = new(rows: x.Rows, cols: x.Cols, mode: ArrayInitMode.Malloc);
            //PowZ(x, y, ref z, checkLength: false);
            factory.iVMF.PowZ(x.Count, x, y, ref z);
            return z;
        }

        #endregion
        #region ---- Pow3o2 [D] ----

        /// <summary>
        /// Computes the element-wise power of 3/2 for the input array and stores the result in the output array.
        /// y[i] = x[i]^(3/2)
        /// </summary>
        /// <remarks>This method performs an in-place operation on the output array <paramref name="y"/>. 
        /// The input and output arrays must have the same length to avoid undefined behavior.</remarks>
        /// <param name="x">The input array containing the elements to be raised to the power of 3/2. Must not be null.</param>
        /// <param name="y">A reference to the output array where the computed results will be stored.  The array must be pre-allocated
        /// and have the same length as <paramref name="x"/>.</param>
        public static void Pow3o2(DenseArray<Real> x, 
            ref DenseArray<Real> y)
            => factory.iVMF.Pow3o2(x.Count, x.DPtr, y.DPtr);


        // obsolete ...
        ///// <summary>
        ///// computes the square root of the cube of each array element
        ///// yi = xi^(3/2)
        ///// </summary>
        ///// <typeparam name="T"> ArrayBase[double] </typeparam>
        ///// <param name="x"> array x </param>
        ///// <param name="y"> array y </param>
        ///// <param name="checkLength"> whether checks the lengths of input arrays </param>
        //public static void Pow3o2D<T>(T x, ref T y,
        //    bool checkLength = true)
        //    where T : DenseArrayBase<double>
        //{
        //    if (checkLength) { ExArrayLengthD(x, y); }
        //    factory.iVMF.Pow3o2D(x.Count, x, ref y);
        //}

        /// <summary>
        /// computes the square root of the cube of each vector element
        /// </summary>
        /// <param name="x"> input vector x </param>
        /// <returns> result vector </returns>
        public static VectorD Pow3o2(VectorD x)
        {
            VectorD y = new(count: x.Count, mode: ArrayInitMode.Malloc);
            //Pow3o2D(x, ref y, checkLength: false);
            factory.iVMF.Pow3o2D(x.Count, x, ref y);
            return y;
        }

        /// <summary>
        /// computes the square root of the cube of each matrix element
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <returns> result matrix </returns>
        public static MatrixD Pow3o2(MatrixD x)
        {
            MatrixD y = new(rows: x.Rows, cols: x.Cols, mode: ArrayInitMode.Malloc);
            //Pow3o2D(x, ref y, checkLength: false);
            factory.iVMF.Pow3o2D(x.Count, x, ref y);
            return y;
        }

        #endregion
        #region ---- Pow2o3 [D] ----

        /// <summary>
        /// Computes the two-thirds power of each element in the input array and stores the results in the output array.
        /// y[i] = x[i]^(2/3)
        /// </summary>
        /// <remarks>This method modifies the contents of the output array <paramref name="y"/> in place. 
        /// Ensure that the input and output arrays are properly initialized and of the same size before calling this
        /// method.</remarks>
        /// <param name="x">The input array containing the elements to be processed.</param>
        /// <param name="y">The output array where the computed results will be stored.  Must have the same length as the input array.</param>
        public static void Pow2o3(DenseArray<Real> x, 
            ref DenseArray<Real> y)
            => factory.iVMF.Pow2o3(x.Count, x.DPtr, y.DPtr);


        // obsolete ...
        ///// <summary>
        ///// computes the cube root of the square of each array element
        ///// yi = xi^(2/3)
        ///// </summary>
        ///// <typeparam name="T"> ArrayBase[double] </typeparam>
        ///// <param name="x"> array x </param>
        ///// <param name="y"> array y </param>
        ///// <param name="checkLength"> whether checks the lengths of input arrays </param>
        //public static void Pow2o3D<T>(T x, ref T y,
        //    bool checkLength = true)
        //    where T : DenseArrayBase<double>
        //{
        //    if (checkLength) { ExArrayLengthD(x, y); }
        //    factory.iVMF.Pow2o3D(x.Count, x, ref y);
        //}

        /// <summary>
        /// computes the cube root of the square of each vector element
        /// </summary>
        /// <param name="x"> input vector x </param>
        /// <returns> result vector </returns>
        public static VectorD Pow2o3(VectorD x)
        {
            VectorD y = new(count: x.Count, mode: ArrayInitMode.Malloc);
            //Pow2o3D(x, ref y, checkLength: false);
            factory.iVMF.Pow2o3D(x.Count, x, ref y);
            return y;
        }

        /// <summary>
        /// computes the cube root of the square of each matrix element
        /// </summary>
        /// <param name="x"> matrix vector x </param>
        /// <returns> result matrix </returns>
        public static MatrixD Pow2o3(MatrixD x)
        {
            MatrixD y = new(rows: x.Rows, cols: x.Cols, mode: ArrayInitMode.Malloc);
            //Pow2o3D(x, ref y, checkLength: false);
            factory.iVMF.Pow2o3D(x.Count, x, ref y);
            return y;
        }

        #endregion
        #region ---- Powx [D/Z] ----

        /// <summary>
        /// Raises each element of the dense array <paramref name="x"/> to the scalar power <paramref name="s"/> and stores the result in <paramref name="y"/>.
        /// </summary>
        /// <typeparam name="T">Numeric type.</typeparam>
        /// <param name="x">Input dense array.</param>
        /// <param name="s">Scalar exponent.</param>
        /// <param name="y">Result dense array.</param>
        public static void Powx<T>(DenseArray<T> x, T s, ref DenseArray<T> y)
            where T : INumber<T>
            => Powx(x, s, y);

        internal static void Powx<T>(DenseArray<T> x, T s, 
            DenseArray<T> y)
            where T : INumber<T>
        {
            if (typeof(T) == typeof(Real)) factory.iVMF.Powx(x.Count, x.DPtr, &s, y.DPtr);
            else if (typeof(T) == typeof(Cplx)) factory.iVMF.Powx(x.Count, x.VPtr, &s, y.VPtr);
            else throw new NotSupportedException($"Type {typeof(T)} not supported.");
        }


        // obsolete ...
        ///// <summary>
        ///// computes each element of array x to the scalar power s
        ///// yi = xi^s
        ///// </summary>
        ///// <typeparam name="T"> ArrayBase[double] </typeparam>
        ///// <param name="x"> array x </param>
        ///// <param name="s"> scalar s </param>
        ///// <param name="y"> array y </param>
        ///// <param name="checkLength"> whether checks the lengths of input arrays </param>
        //public static void PowxD<T>(T x, double s, ref T y,
        //    bool checkLength = true)
        //    where T : DenseArrayBase<double>
        //{
        //    if (checkLength) { ExArrayLengthD(x, y); }
        //    factory.iVMF.PowxD(x.Count, x, s, ref y);
        //}

        ///// <summary>
        ///// computes each element of array x to the scalar power s
        ///// yi = xi^s
        ///// </summary>
        ///// <typeparam name="T"> ArrayBase[Complex] </typeparam>
        ///// <param name="x"> array x </param>
        ///// <param name="s"> scalar s </param>
        ///// <param name="y"> array y </param>
        ///// <param name="checkLength"> whether checks the lengths of input arrays </param>
        //public static void PowxZ<T>(T x, Complex s, ref T y,
        //    bool checkLength = true)
        //    where T : DenseArrayBase<Complex>
        //{
        //    if (checkLength) { ExArrayLengthZ(x, y); }
        //    factory.iVMF.PowxZ(x.Count, x, s, ref y);
        //}

        /// <summary>
        /// computes each element of vector x to the scalar power s
        /// </summary>
        /// <param name="x"> input vector x </param>
        /// <param name="s"> input scalar s </param>
        /// <returns> result vector </returns>
        public static VectorD Powx(VectorD x, double s)
        {
            VectorD y = new(count: x.Count, mode: ArrayInitMode.Malloc);
            //PowxD(x, s, ref y, checkLength: false);
            factory.iVMF.PowxD(x.Count, x, s, ref y);
            return y;
        }

        /// <summary>
        /// computes each element of matrix x to the scalar power s
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <param name="s"> input scalar s </param>
        /// <returns> result matrix </returns>
        public static MatrixD Powx(MatrixD x, double s)
        {
            MatrixD y = new(rows: x.Rows, cols: x.Cols, mode: ArrayInitMode.Malloc);
            //PowxD(x, s, ref y, checkLength: false);
            factory.iVMF.PowxD(x.Count, x, s, ref y);
            return y;
        }

        /// <summary>
        /// computes each element of vector x to the scalar power s
        /// </summary>
        /// <param name="x"> input vector x </param>
        /// <param name="s"> input scalar s </param>
        /// <returns> result vector </returns>
        public static VectorZ Powx(VectorZ x, Complex s)
        {
            VectorZ y = new(count: x.Count, mode: ArrayInitMode.Malloc);
            //PowxZ(x, s, ref y, checkLength: false);
            factory.iVMF.PowxZ(x.Count, x, s, ref y);
            return y;
        }

        /// <summary>
        /// computes each element of matrix x to the scalar power s
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <param name="s"> input scalar s </param>
        /// <returns> result matrix </returns>
        public static MatrixZ Powx(MatrixZ x, Complex s)
        {
            MatrixZ y = new(rows: x.Rows, cols: x.Cols, mode: ArrayInitMode.Malloc);
            //PowxZ(x, s, ref y, checkLength: false);
            factory.iVMF.PowxZ(x.Count, x, s, ref y);
            return y;
        }

        #endregion
        #region ---- Powr [D] ----

        /// <summary>
        /// Raises each element of the input array <paramref name="x"/> to the power of the corresponding element in
        /// array <paramref name="y"/>  and stores the result in the output array <paramref name="z"/>.
        /// z[i] = x[i]^y[i]
        /// </summary>
        /// <remarks>This method performs element-wise exponentiation, where each element in <paramref
        /// name="z"/> is calculated as <c>z[i] = x[i]^y[i]</c>. The input arrays <paramref name="x"/> and <paramref
        /// name="y"/> must have the same length.</remarks>
        /// <param name="x">The input array containing the base values.</param>
        /// <param name="y">The input array containing the exponent values. Must have the same length as <paramref name="x"/>.</param>
        /// <param name="z">A reference to the output array where the results are stored.</param>
        public static void Powr(DenseArray<Real> x, DenseArray<Real> y,
            ref DenseArray<Real> z)
            => factory.iVMF.Powr(x.Count, x.DPtr, y.DPtr, z.DPtr);

        #endregion
        #region ---- SinCos [D] ----

        /// <summary>
        /// Computes the sine and cosine of each element in the input array.
        /// sin[i] = sin(a[i]), cos[i]=cos(a[i])
        /// </summary>
        /// <remarks>The method calculates the sine and cosine for each element in the input array
        /// <paramref name="x"/> and stores the results in the corresponding positions of the <paramref name="sin"/> and
        /// <paramref name="cos"/> arrays.</remarks>
        /// <param name="x">The input array containing the angles, in radians, for which to compute the sine and cosine.</param>
        /// <param name="sin">A reference to the array where the computed sine values will be stored.</param>
        /// <param name="cos">A reference to the array where the computed cosine values will be stored.</param>
        public static void SinCos(DenseArray<Real> x, 
            ref DenseArray<Real> sin, ref DenseArray<Real> cos)
            => factory.iVMF.SinCos(x.Count, x.DPtr, sin.DPtr, cos.DPtr);


        // obsolete ...
        ///// <summary>
        ///// computes sine and cosine of array elements
        ///// sini = sin[xi]
        ///// cosi = cos[xi]
        ///// </summary>
        ///// <typeparam name="T"> ArrayBase[double] </typeparam>
        ///// <param name="x"> array x </param>
        ///// <param name="sin"> array sin </param>
        ///// <param name="cos"> array cos </param>
        ///// <param name="checkLength"> whether checks the lengths of input arrays </param>
        //public static void SinCosD<T>(T x, ref T sin, ref T cos,
        //    bool checkLength = true)
        //    where T : DenseArrayBase<double>
        //{
        //    if (checkLength) { ExArrayLengthD(x, sin); }
        //    if (checkLength) { ExArrayLengthD(x, cos); }
        //    factory.iVMF.SinCosD(x.Count, x, ref sin, ref cos);
        //}

        /// <summary>
        /// computes sine and cosine of vector elements
        /// </summary>
        /// <param name="x"> input vector x </param>
        /// <returns> sin and cos values in two vectors </returns>
        public static (VectorD, VectorD) SinCos(VectorD x)
        {
            VectorD sin = new(count: x.Count, mode: ArrayInitMode.Malloc);
            VectorD cos = new(count: x.Count, mode: ArrayInitMode.Malloc);
            //SinCosD(x, ref sin, ref cos, checkLength: false);
            factory.iVMF.SinCosD(x.Count, x, ref sin, ref cos);
            return (sin, cos);
        }

        /// <summary>
        /// computes sine and cosine of matrix elements
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <returns> sin and cos values in two matrices </returns>
        public static (MatrixD, MatrixD) SinCos(MatrixD x)
        {
            MatrixD sin = new(rows: x.Rows, cols: x.Cols, mode: ArrayInitMode.Malloc);
            MatrixD cos = new(rows: x.Rows, cols: x.Cols, mode: ArrayInitMode.Malloc);
            //SinCosD(x, ref sin, ref cos, checkLength: false);
            factory.iVMF.SinCosD(x.Count, x, ref sin, ref cos);
            return (sin, cos);
        }

        #endregion
        #region ---- SinCosPI [D] ----

        /// <summary>
        /// Computes the sine and cosine of each element in the input array, where the input values are interpreted as multiples of π (pi).
        /// sin[i] = sinpi(a[i]), cos[i]=cospi(a[i])
        /// </summary>
        /// <remarks>For each element in the input array <paramref name="x"/>, this method computes the
        /// sine and cosine of π times the element's value. The results are stored in the corresponding positions of the
        /// <paramref name="sin"/> and <paramref name="cos"/> arrays.</remarks>
        /// <param name="x">The input array of double values, where each value represents a multiple of π.</param>
        /// <param name="sin">A reference to the output array that will receive the sine values.</param>
        /// <param name="cos">A reference to the output array that will receive the cosine values.</param>
        public static void SinCosPI(DenseArray<Real> x, 
            ref DenseArray<Real> sin, ref DenseArray<Real> cos)
            => factory.iVMF.SinCospi(x.Count, x.DPtr, sin.DPtr, cos.DPtr);

        #endregion
        // ...
        #region --------- LinearFrac (...) ---------
        // ...
        #endregion
        #region ---- Ceil [D] ----

        /// <summary>
        /// Computes the ceiling of each element in the input array <paramref name="x"/> and stores the results in the
        /// output array <paramref name="y"/>.
        /// </summary>
        /// <remarks>The ceiling operation rounds each element in the input array <paramref name="x"/> to
        /// the smallest integer value  that is greater than or equal to the element. The results are stored in the
        /// corresponding positions of the output array <paramref name="y"/>.</remarks>
        /// <param name="x">The input array containing double-precision floating-point values.</param>
        /// <param name="y">The output array where the ceiling values will be stored.</param>
        public static void Ceil(DenseArray<Real> x, 
            ref DenseArray<Real> y)
            => factory.iVMF.Ceil(x.Count, x.DPtr, y.DPtr);

        #endregion
        #region ---- Floor [D] ----

        /// <summary>
        /// Computes the floor of each element in the input array <paramref name="x"/> and stores the result in the
        /// output array <paramref name="y"/>.
        /// </summary>
        /// <remarks>The floor operation rounds each element in the input array <paramref name="x"/> down
        /// to the nearest integer value. The output array <paramref name="y"/> must be pre-allocated and have
        /// sufficient capacity to store the results.</remarks>
        /// <param name="x">The input array of double values.</param>
        /// <param name="y">The output array where the floored values will be stored.</param>
        public static void Floor(DenseArray<Real> x, 
            ref DenseArray<Real> y)
            => factory.iVMF.Floor(x.Count, x.DPtr, y.DPtr);

        #endregion
        #region ---- Frac [D] ----

        /// <summary>
        /// Computes the fractional part of each element in the input array <paramref name="x"/>  and stores the result
        /// in the output array <paramref name="y"/>.
        /// y[i] = x[i] - |x[i]|
        /// </summary>
        /// <remarks>The fractional part of a number is calculated as the difference between the number
        /// and its floor value. This method assumes that <paramref name="y"/> has been properly initialized and has
        /// sufficient capacity to store the results.</remarks>
        /// <param name="x">The input array containing the elements for which the fractional parts will be computed.</param>
        /// <param name="y">The output array where the fractional parts of the elements in <paramref name="x"/> will be stored.</param>
        public static void Frac(DenseArray<Real> x, 
            ref DenseArray<Real> y)
            => factory.iVMF.Frac(x.Count, x.DPtr, y.DPtr);

        #endregion
        #region ---- Modf [D] ----

        /// <summary>
        /// Splits each element of the input array into its fractional and integer parts.
        /// y[i] = |x[i]|, z[i] = x[i] - |x[i]|
        /// </summary>
        /// <remarks>The method processes each element of the input array <paramref name="x"/> such that:
        /// <list type="bullet"> <item><description>The fractional part of each element is stored in <paramref
        /// name="y"/>.</description></item> <item><description>The integer part of each element is stored in <paramref
        /// name="z"/>.</description></item> </list> The input and output arrays must have the same length. The method
        /// does not perform bounds checking.</remarks>
        /// <param name="x">The input array of double-precision floating-point numbers.</param>
        /// <param name="y">A reference to the output array that will receive the fractional parts of the elements in <paramref name="x"/>.</param>
        /// <param name="z">A reference to the output array that will receive the integer parts of the elements in <paramref name="x"/>.</param>
        public static void Modf(DenseArray<Real> x, 
            ref DenseArray<Real> y, ref DenseArray<Real> z)
            => factory.iVMF.Modf(x.Count, x.DPtr, y.DPtr, z.DPtr);

        #endregion
        #region ---- Fmod [D] ----

        /// <summary>
        /// Computes the element-wise floating-point remainder of division for two input arrays.
        /// z[i] = fmod(x[i], y[i])
        /// </summary>
        /// <remarks>The operation calculates the remainder of the division of each corresponding element
        /// in <paramref name="x"/> by <paramref name="y"/>.</remarks>
        /// <param name="x">The first input array containing the dividend values.</param>
        /// <param name="y">The second input array containing the divisor values.</param>
        /// <param name="z">The output array where the computed remainders are stored. Each element in <paramref name="z"/> is the
        /// result of the operation <c>fmod(x[i], y[i])</c>.</param>
        public static void Fmod(DenseArray<Real> x, DenseArray<Real> y,
            ref DenseArray<Real> z)
            => factory.iVMF.Fmod(x.Count, x.DPtr, y.DPtr, z.DPtr);

        #endregion
        #region ---- Remainder [D] ----

        /// <summary>
        /// Computes the element-wise remainder of division between two dense arrays of double-precision values.
        /// z[i] = remainder(x[i], y[i])
        /// </summary>
        /// <remarks>This method calculates the remainder for each corresponding pair of elements in the
        /// input arrays  <paramref name="x"/> and <paramref name="y"/> and stores the results in the output array
        /// <paramref name="z"/>. The operation performed is equivalent to the mathematical remainder
        /// operation.</remarks>
        /// <param name="x">The first input array containing the dividend values.</param>
        /// <param name="y">The second input array containing the divisor values.</param>
        /// <param name="z">A reference to the output array where the computed remainders are stored.</param>
        public static void Remainder(DenseArray<Real> x, DenseArray<Real> y,
            ref DenseArray<Real> z)
            => factory.iVMF.Remainder(x.Count, x.DPtr, y.DPtr, z.DPtr);

        #endregion
        #region ---- NextAfter [D] ----

        /// <summary>
        /// Computes the next representable double-precision floating-point value for each element in the input array
        /// <paramref name="x"/> in the direction of the corresponding element in the input array <paramref name="y"/>.
        /// z[i] = nextafter(x[i], y[i])
        /// </summary>
        /// <remarks>This method calculates the next representable value for each element in <paramref
        /// name="x"/> in the direction of the corresponding element in <paramref name="y"/>. The result is stored in
        /// <paramref name="z"/>. The operation is performed element-wise.</remarks>
        /// <param name="x">The input array of double-precision values for which the next representable values are computed.</param>
        /// <param name="y">The input array indicating the direction for each corresponding element in <paramref name="x"/>.</param>
        /// <param name="z">The output array where the computed next representable values are stored.</param>
        public static void NextAfter(DenseArray<Real> x, DenseArray<Real> y,
            ref DenseArray<Real> z)
            => factory.iVMF.NextAfter(x.Count, x.DPtr, y.DPtr, z.DPtr);

        #endregion
        #region ---- CopySign [D] ----

        /// <summary>
        /// Copies the sign of each element in the array <paramref name="y"/> to the corresponding element in the array
        /// <paramref name="x"/>, storing the result in the array <paramref name="z"/>.
        /// </summary>
        /// <remarks>The operation performed is equivalent to <c>z[i] = copysign(x[i], y[i])</c> for each
        /// element in the arrays.</remarks>
        /// <param name="x">The input array whose magnitudes are used.</param>
        /// <param name="y">The input array whose signs are used.</param>
        /// <param name="z">The output array where the result is stored. Each element in <paramref name="z"/> will have the magnitude of
        /// the corresponding element in <paramref name="x"/> and the sign of the corresponding element in <paramref name="y"/>.</param>
        public static void CopySign(DenseArray<Real> x, DenseArray<Real> y,
            ref DenseArray<Real> z)
            => factory.iVMF.CopySign(x.Count, x.DPtr, y.DPtr, z.DPtr);

        #endregion
        #region ---- Fdim [D] ----

        /// <summary>
        /// Computes the positive difference between two arrays element-wise.
        /// </summary>
        /// <remarks>The operation is performed element-wise such that for each index <c>i</c>,  <c>z[i] =
        /// max(x[i] - y[i], 0)</c>. The input arrays <paramref name="x"/> and <paramref name="y"/> must have the same
        /// length,  and the output array <paramref name="z"/> must be pre-allocated.</remarks>
        /// <param name="x">The first input array.</param>
        /// <param name="y">The second input array.</param>
        /// <param name="z">A reference to the output array where the result is stored.  Each element in the output array is calculated
        /// as the maximum of the difference  between the corresponding elements in <paramref name="x"/> and <paramref name="y"/>, or 0.</param>
        public static void Fdim(DenseArray<Real> x, DenseArray<Real> y,
            ref DenseArray<Real> z)
            => factory.iVMF.Fdim(x.Count, x.DPtr, y.DPtr, z.DPtr);

        #endregion
        #region ---- Fmax [D] ----

        /// <summary>
        /// Computes the element-wise maximum of two double-precision arrays.
        /// </summary>
        /// <remarks>This method calculates the maximum value for each corresponding element in the input
        /// arrays <paramref name="x"/> and <paramref name="y"/>  and stores the result in the output array <paramref
        /// name="z"/>.</remarks>
        /// <param name="x">The first input array containing double-precision values.</param>
        /// <param name="y">The second input array containing double-precision values.</param>
        /// <param name="z">A reference to the output array where the element-wise maximum values are stored.</param>
        public static void Fmax(DenseArray<Real> x, DenseArray<Real> y,
            ref DenseArray<Real> z)
            => factory.iVMF.Fmax(x.Count, x.DPtr, y.DPtr, z.DPtr);

        #endregion
        #region ---- Fmin [D] ----

        /// <summary>
        /// Computes the element-wise minimum of two double-precision arrays.
        /// </summary>
        /// <remarks>This method calculates the minimum value for each corresponding element in the input
        /// arrays  <paramref name="x"/> and <paramref name="y"/>, and stores the results in the output array <paramref
        /// name="z"/>.</remarks>
        /// <param name="x">The first input array.</param>
        /// <param name="y">The second input array.</param>
        /// <param name="z">A reference to the output array where the element-wise minimum values are stored.</param>
        public static void Fmin(DenseArray<Real> x, DenseArray<Real> y,
            ref DenseArray<Real> z)
            => factory.iVMF.Fmin(x.Count, x.DPtr, y.DPtr, z.DPtr);

        #endregion
        #region ---- MaxMag [D] ----

        /// <summary>
        /// Computes the element-wise maximum magnitude of two arrays of double-precision floating-point numbers.
        /// </summary>
        /// <remarks>The input arrays <paramref name="x"/> and <paramref name="y"/> must have the same
        /// length as the output array <paramref name="z"/>. The operation is performed element-wise.</remarks>
        /// <param name="x">The first input array. Each element represents one of the values to compare.</param>
        /// <param name="y">The second input array. Each element represents the other value to compare.</param>
        /// <param name="z">The output array where the result is stored. Each element will contain the value with the greater magnitude 
        /// from the corresponding elements of <paramref name="x"/> and <paramref name="y"/>.</param>
        public static void MaxMag(DenseArray<Real> x, DenseArray<Real> y,
            ref DenseArray<Real> z)
            => factory.iVMF.MaxMag(x.Count, x.DPtr, y.DPtr, z.DPtr);

        #endregion
        #region ---- MinMag [D] ----

        /// <summary>
        /// Computes the element-wise minimum magnitude of two dense arrays. For each element <c>i</c>, the result is
        /// stored in the output array as <c>z[i] = min(|x[i]|, |y[i]|)</c>.
        /// </summary>
        /// <remarks>This method performs an in-place computation of the minimum magnitude for each
        /// corresponding element in the input arrays.</remarks>
        /// <param name="x">The first input array. Must not be <c>null</c>.</param>
        /// <param name="y">The second input array. Must not be <c>null</c>.</param>
        /// <param name="z">The output array where the result is stored.</param>
        public static void MinMag(DenseArray<Real> x, DenseArray<Real> y,
            ref DenseArray<Real> z)
            => factory.iVMF.MinMag(x.Count, x.DPtr, y.DPtr, z.DPtr);

        #endregion
        #region ---- Round [D] ----

        /// <summary>
        /// Rounds each element of the input array <paramref name="x"/> to the nearest integer value and stores the
        /// result in the output array <paramref name="y"/>.
        /// </summary>
        /// <remarks>The method processes all elements in the input array <paramref name="x"/> and writes
        /// the rounded  results to the corresponding positions in the output array <paramref name="y"/>.  The operation
        /// assumes that <paramref name="y"/> has been properly allocated and is of sufficient size to store the results.</remarks>
        /// <param name="x">The input array of double values to be rounded.</param>
        /// <param name="y">The output array where the rounded values will be stored.</param>
        public static void Round(DenseArray<Real> x, ref DenseArray<Real> y)
            => factory.iVMF.Round(x.Count, x.DPtr, y.DPtr);

        #endregion
        #region ---- Conj [Z] ----

        /// <summary>
        /// Computes the complex conjugate of each element in the input array and stores the result in the output array.
        /// y[i] = conj(x[i])
        /// </summary>
        /// <remarks>The method assumes that the input and output arrays are properly allocated and have
        /// matching lengths.</remarks>
        /// <param name="x">The input array containing complex numbers.</param>
        /// <param name="y">The output array where the complex conjugates will be stored.</param>
        public static void Conj(DenseArray<Cplx> x, ref DenseArray<Cplx> y)
            => factory.iVMF.Conj(x.Count, x.VPtr, y.VPtr);


        // obsolete ...
        ///// <summary>
        ///// performs element by element conjugation of the array
        ///// yi = conj[xi]
        ///// </summary>
        ///// <typeparam name="T"> ArrayBase[Complex] </typeparam>
        ///// <param name="x"> array x </param>
        ///// <param name="y"> array y </param>
        ///// <param name="checkLength"> whether checks the lengths of input arrays </param>
        //public static void ConjZ<T>(T x, ref T y,
        //    bool checkLength = true) where T : DenseArrayBase<Complex>
        //{
        //    if (checkLength) { ExArrayLengthZ(x, y); }
        //    factory.iVMF.ConjZ(x.Count, x, ref y);
        //}

        /// <summary>
        /// performs element by element conjugation of the vector
        /// </summary>
        /// <param name="x"> input vector x </param>
        /// <returns> result vector </returns>
        public static VectorZ Conj(VectorZ x)
        {
            VectorZ y = new(count: x.Count, mode: ArrayInitMode.Malloc);
            //ConjZ(x, ref y, checkLength: false);
            factory.iVMF.ConjZ(x.Count, x, ref y);
            return y;
        }

        /// <summary>
        /// performs element by element conjugation of the matrix
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <returns> result matrix </returns>
        public static MatrixZ Conj(MatrixZ x)
        {
            MatrixZ y = new(rows: x.Rows, cols: x.Cols, mode: ArrayInitMode.Malloc);
            //ConjZ(x, ref y, checkLength: false);
            factory.iVMF.ConjZ(x.Count, x, ref y);
            return y;
        }

        #endregion
        #region ---- MulByConj [Z] ----

        /// <summary>
        /// Multiplies each element of the first array by the conjugate of the corresponding element in the second array.
        /// </summary>
        /// <remarks>The operation is performed element-wise, and the result is stored in the <paramref name="z"/> array.</remarks>
        /// <param name="x">The first array of complex numbers to be multiplied.</param>
        /// <param name="y">The second array of complex numbers whose conjugates are used in the multiplication.</param>
        /// <param name="z">The array where the result of the element-wise multiplication is stored.</param>
        public static void MulByConj(DenseArray<Cplx> x, DenseArray<Cplx> y,
            ref DenseArray<Cplx> z)
            => factory.iVMF.MulByConj(x.Count, x.VPtr, y.VPtr, z.VPtr);


        // obsolete ...
        ///// <summary>
        ///// performs element by element multiplication of array x element 
        ///// and conjugated array y element
        ///// zi = xi * conj(yi)
        ///// </summary>
        ///// <typeparam name="T"> ArrayBase[Complex] </typeparam>
        ///// <param name="x"> array x </param>
        ///// <param name="y"> array y </param>
        ///// <param name="z"> array z </param>
        ///// <param name="checkLength"> whether checks the lengths of input arrays </param>
        //public static void MulByConjZ<T>(T x, T y, ref T z,
        //    bool checkLength = true)
        //    where T : DenseArrayBase<Complex>
        //{
        //    if (checkLength) { ExArrayLengthZ(x, y); }
        //    if (checkLength) { ExArrayLengthZ(x, z); }
        //    factory.iVMF.MulByConjZ(x.Count, x, y, ref z);
        //}

        /// <summary>
        /// performs element by element multiplication of vector x element 
        /// and conjugated vector y element
        /// </summary>
        /// <param name="x"> input vector x </param>
        /// <param name="y"> input vector y </param>
        /// <param name="checkLength"> whether checks the lengths of input arrays </param>
        /// <returns> result vector </returns>
        public static VectorZ MulByConj(VectorZ x, VectorZ y,
            bool checkLength = true)
        {
            if (checkLength) { ExArrayLengthZ(x, y); }
            VectorZ z = new(count: x.Count, mode: ArrayInitMode.Malloc);
            //MulByConjZ(x, y, ref z, checkLength: false);
            factory.iVMF.MulByConjZ(x.Count, x, y, ref z);
            return z;
        }

        /// <summary>
        /// performs element by element multiplication of matrix x element 
        /// and conjugated matrix y element
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <param name="y"> input matrix y </param>
        /// <param name="checkLength"> whether checks the lengths of input arrays </param>
        /// <returns> result matrix </returns>
        public static MatrixZ MulByConj(MatrixZ x, MatrixZ y,
            bool checkLength = true)
        {
            if (checkLength) { ExArrayLengthZ(x, y); }
            MatrixZ z = new(rows: x.Rows, cols: x.Cols, mode: ArrayInitMode.Malloc);
            //MulByConjZ(x, y, ref z, checkLength: false);
            factory.iVMF.MulByConjZ(x.Count, x, y, ref z);
            return z;
        }

        #endregion
        // ...
        #region --------- PackI (...) ---------
        // ...
        #endregion
        #region --------- UnpackI (...) ---------
        // ...
        #endregion

        #endregion
        #region VMF-Extensions

        #region ---- Take ----

        internal static void Take<T>(DenseArray<T> x, DenseArray<Real> y,
            ComplexPart part = ComplexPart.RealPart)
            where T : INumber<T>
        {
            if (typeof(T) == typeof(Cplx))
                if (part == ComplexPart.RealPart) factory.iVMF.PackI(n: x.Count, a: x.DPtr, y: y.DPtr, inca: 2);
                else if (part == ComplexPart.ImagPart) factory.iVMF.PackI(n: x.Count, a: x.DPtr + 1, y: y.DPtr, inca: 2);
                else throw new NotSupportedException($"ComplexPart {part} not supported.");
            else if (typeof(T) == typeof(Real)) throw new NotSupportedException($"Type {typeof(T)} is Real");
            else throw new NotSupportedException($"Type {typeof(T)} not supported.");
        }

        /// <summary>
        /// Extracts the specified part (real or imaginary) from a complex array and stores it in a real array.
        /// </summary>
        /// <param name="x">The input dense array of complex numbers.</param>
        /// <param name="y">The output dense array to store the extracted real values.</param>
        /// <param name="part">
        /// Specifies which part of the complex number to extract.
        /// <see cref="ComplexPart.RealPart"/> extracts the real part.
        /// <see cref="ComplexPart.ImagPart"/> extracts the imaginary part.
        /// Default is <see cref="ComplexPart.RealPart"/>.
        /// </param>
        public static void Take(DenseArray<Cplx> x, ref DenseArray<Real> y,
            ComplexPart part = ComplexPart.RealPart)
            => Take(x, y, part);

        #endregion
        #region ---- Modify ----

        internal static void Modify<T>(DenseArray<T> x, DenseArray<Cplx> y,
            ComplexPart part = ComplexPart.RealPart)
            where T : INumber<T>
        {
            if (typeof(T) == typeof(Real))
                if (part == ComplexPart.RealPart) factory.iVMF.UnpackI(n: x.Count, a: x.DPtr, y: y.DPtr, incy: 2);
                else if (part == ComplexPart.ImagPart) factory.iVMF.UnpackI(n: x.Count, a: x.DPtr, y: y.DPtr + 1, incy: 2);
                else throw new NotSupportedException($"ComplexPart {part} not supported.");
            else if (typeof(T) == typeof(Cplx)) throw new NotSupportedException($"Type {typeof(T)} is Cplx");
            else throw new NotSupportedException($"Type {typeof(T)} not supported.");
        }

        /// <summary>
        /// Modifies the complex part of the destination array <paramref name="y"/> using the real values from <paramref name="x"/>.
        /// The <paramref name="part"/> parameter specifies which complex part (real or imaginary) to modify.
        /// </summary>
        /// <param name="x">Source array of real values.</param>
        /// <param name="y">Destination array of complex values to be modified.</param>
        /// <param name="part">Specifies which complex part to modify (default is <see cref="ComplexPart.RealPart"/>).</param>
        public static void Modify(DenseArray<Real> x, ref DenseArray<Cplx> y,
            ComplexPart part = ComplexPart.RealPart)
            => Modify(x, y, part);

        #endregion
        #region ---- ToCplx ----

        internal static void ToCplx<T>(DenseArray<T> x, DenseArray<Cplx> y)
            where T : INumber<T>
            => Modify(x, y, part: ComplexPart.RealPart);


        public static Vect<Cplx> ToCplx<T>(Vect<T> x)
            where T : INumber<T>
        {
            Vect<Cplx> y = new(count: x.Count, initMode: ArrayInitMode.Calloc);
            ToCplx(x, y);
            return y;
        }


        public static Matx<Cplx> ToCplx<T>(Matx<T> x)
            where T : INumber<T>
        {
            Matx<Cplx> y = new(rows: x.Rows, cols: x.Cols, initMode: ArrayInitMode.Calloc);
            ToCplx(x, y);
            return y;
        }

        /// <summary>
        /// Converts a real-valued vector to a complex-valued vector, setting the real part from the input and the imaginary part to zero.
        /// </summary>
        /// <param name="x">The input real-valued vector.</param>
        /// <returns>
        /// A <see cref="Vect{Cplx}"/> instance with the same length as <paramref name="x"/>, where each element's real part is set from <paramref name="x"/> and the imaginary part is zero.
        /// </returns>
        public static Vect<Cplx> ToCplx(Vect<Real> x)
        {
            Vect<Cplx> y = new(count: x.Count, initMode: ArrayInitMode.Calloc);
            ToCplx(x, y);
            return y;
        }

        /// <summary>
        /// Converts a real-valued matrix to a complex-valued matrix, setting the real part from the input and the imaginary part to zero.
        /// </summary>
        /// <param name="x">The input real-valued matrix.</param>
        /// <returns>
        /// A <see cref="Matx{Cplx}"/> instance with the same dimensions as <paramref name="x"/>, where each element's real part is set from <paramref name="x"/> and the imaginary part is zero.
        /// </returns>
        public static Matx<Cplx> ToCplx(Matx<Real> x)
        {
            Matx<Cplx> y = new(rows: x.Rows, cols: x.Cols, initMode: ArrayInitMode.Calloc);
            ToCplx(x, y);
            return y;
        }

        #endregion

        #region ---- RealPart ----

        // wrapper 
        /// <summary>
        /// takes the real part of each complex vector element
        /// </summary>
        /// <param name="x"> input vector x </param>
        /// <returns> result vector </returns>
        public static VectorD RealPart(VectorZ x)
        {
            VectorD res = new(x.Count);
            factory.iVMF.RealPart(res.Count, x, ref res);
            return res;
        }

        /// <summary>
        /// takes the real part of each complex matrix element
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <returns> result matrix </returns>
        public static MatrixD RealPart(MatrixZ x)
        {
            MatrixD res = new(x.Rows, x.Cols);
            factory.iVMF.RealPart(res.Count, x, ref res);
            return res;
        }

        #endregion
        #region ---- ImagPart ----

        // wrapper
        /// <summary>
        /// takes the imaginary part of each complex vector element
        /// </summary>
        /// <param name="x"> input vector x </param>
        /// <returns> result vector </returns>
        public static VectorD ImagPart(VectorZ x)
        {
            VectorD res = new(x.Count);
            factory.iVMF.ImagPart(res.Count, x, ref res);
            return res;
        }

        /// <summary>
        /// takes the imaginary part of each complex matrix element
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <returns> result matrix </returns>
        public static MatrixD ImagPart(MatrixZ x)
        {
            MatrixD res = new(x.Rows, x.Cols);
            factory.iVMF.ImagPart(res.Count, x, ref res);
            return res;
        }

        #endregion
        #region ---- RealImagPart ----

        /// <summary>
        /// takes the real and imaginary parts of each complex vector element
        /// </summary>
        /// <param name="x"> input vector x </param>
        /// <param name="realPart"> result real-part vector </param>
        /// <param name="imagPart"> result imagimary-part vector </param>
        public static void RealImagParts(VectorZ x,
            ref VectorD realPart, ref VectorD imagPart)
            => factory.iVMF.RealImagParts(x.Count, x, ref realPart, ref imagPart);

        /// <summary>
        /// takes the real and imaginary parts of each complex matrix element
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <param name="realPart"> result real-part matrix </param>
        /// <param name="imagPart"> result imagimary-part matrix </param>
        public static void RealImagParts(MatrixZ x,
            ref MatrixD realPart, ref MatrixD imagPart)
            => factory.iVMF.RealImagParts(x.Count, x, ref realPart, ref imagPart);

        #endregion
        #region ---- Modify ----

        // wrapper
        /// <summary>
        /// modifies the real part of a complex vector
        /// </summary>
        /// <param name="realPart"> real part vector </param>
        /// <param name="res"> result vector (input and modified) </param>
        public static void ModifyReal(VectorD realPart,
            ref VectorZ res)
            => factory.iVMF.ModifyReal(res.Count, realPart, ref res);

        /// <summary>
        /// modifies the real part of a complex matrix
        /// </summary>
        /// <param name="realPart"> real part matrix </param>
        /// <param name="res"> result matrix (input and modified) </param>
        public static void ModifyReal(MatrixD realPart,
            ref MatrixZ res)
            => factory.iVMF.ModifyReal(res.Count, realPart, ref res);

        /// <summary>
        /// modifies the imaginary part of a complex vector
        /// </summary>
        /// <param name="imagPart"> imaginary part vector </param>
        /// <param name="res"> result vector (input and modified) </param>
        public static void ModifyImag(VectorD imagPart,
            ref VectorZ res)
            => factory.iVMF.ModifyImag(res.Count, imagPart, ref res);

        /// <summary>
        /// modifies the imaginary part of a complex matrix
        /// </summary>
        /// <param name="imagPart"> imaginary part matrix </param>
        /// <param name="res"> result matrix (input and modified) </param>
        public static void ModifyImag(MatrixD imagPart,
            ref MatrixZ res)
            => factory.iVMF.ModifyImag(res.Count, imagPart, ref res);

        /// <summary>
        /// modifies the real part of a complex vector
        /// </summary>
        /// <param name="realPart"> real part vector </param>
        /// <param name="imagPart"> imaginary part vector </param>
        /// <param name="res"> result vector (input and modified) </param>
        public static void Modify(VectorD realPart, VectorD imagPart,
            ref VectorZ res)
            => factory.iVMF.Modify(res.Count, realPart, imagPart, ref res);

        /// <summary>
        /// modifies the real part of a complex matrix
        /// </summary>
        /// <param name="realPart"> real part matrix </param>
        /// <param name="imagPart"> imaginary part matrix </param>
        /// <param name="res"> result matrix (input and modified) </param>
        public static void Modify(MatrixD realPart, MatrixD imagPart,
            ref MatrixZ res)
            => factory.iVMF.Modify(res.Count, realPart, imagPart, ref res);

        #endregion
        #region ---- Construct ----

        /// <summary>
        /// constructs a complex vector with its real part only
        /// </summary>
        /// <param name="realPart"> input real-part vector </param>
        /// <returns> result complex vector </returns>
        public static VectorZ ConstructReal(VectorD realPart)
        {
            VectorZ res = new(realPart.Count, 0.0);
            ModifyReal(realPart, ref res);
            return res;
        }

        /// <summary>
        /// constructs a complex matrix with its real part only
        /// </summary>
        /// <param name="realPart"> input real-part matrix </param>
        /// <returns> result complex matrix </returns>
        public static MatrixZ ConstructReal(MatrixD realPart)
        {
            MatrixZ res = new(realPart.Rows, realPart.Cols, 0.0);
            ModifyReal(realPart, ref res);
            return res;
        }

        /// <summary>
        /// constructs a complex vector with its imaginary part only
        /// </summary>
        /// <param name="imagPart"> input imaginary-part vector </param>
        /// <returns> result complex vector </returns>
        public static VectorZ ConstructImag(VectorD imagPart)
        {
            VectorZ res = new(imagPart.Count, 0.0);
            ModifyImag(imagPart, ref res);
            return res;
        }

        /// <summary>
        /// constructs a complex matrix with its imaginary part only
        /// </summary>
        /// <param name="imagPart"> input imaginary-part matrix </param>
        /// <returns> result complex matrix </returns>
        public static MatrixZ ConstructImag(MatrixD imagPart)
        {
            MatrixZ res = new(imagPart.Rows, imagPart.Cols, 0.0);
            ModifyImag(imagPart, ref res);
            return res;
        }

        /// <summary>
        /// constructs a complex vector with its real and imaginary parts
        /// </summary>
        /// <param name="realPart"> input real-part vector </param>
        /// <param name="imagPart"> input imag-part vector </param>
        /// <returns> result complex vector </returns>
        public static VectorZ Construct(VectorD realPart,
            VectorD imagPart)
        {
            VectorZ res = new(realPart.Count);
            Modify(realPart, imagPart, ref res);
            return res;
        }

        /// <summary>
        /// constructs a complex matrix with its real and imaginary parts
        /// </summary>
        /// <param name="realPart"> input real-part matrix </param>
        /// <param name="imagPart"> input imag-part matrix </param>
        /// <returns> result complex matrix </returns>
        public static MatrixZ Construct(MatrixD realPart,
            MatrixD imagPart)
        {
            MatrixZ res = new(realPart.Rows, realPart.Cols);
            Modify(realPart, imagPart, ref res);
            return res;
        }

        #endregion

        #region ---- Variance ----

        #region real-valued

        /// <summary>
        /// computes the absolute variance of a vector
        /// with given avarage or expected value
        /// 1/N * SUM[ x_i - x_Bar ]^2
        /// </summary>
        /// <param name="x"> vector x </param>
        /// <param name="xBar"> average or expected value </param>
        /// <returns> absolute variance </returns>
        public static double Variance(VectorD x, double xBar)
        {
            VectorD d = (xBar == 0.0)? x : (x - xBar);
            double d2Sum = Sum(Square(d));
            return d2Sum / x.Count;
        }

        /// <summary>
        /// computes the absolute variance of a matrix
        /// with given avarage or expected value
        /// 1/N * SUM[ x_i - x_Bar ]^2
        /// </summary>
        /// <param name="x"> matrix x </param>
        /// <param name="xBar"> average or expected value </param>
        /// <returns> absolute variance </returns>
        public static double Variance(MatrixD x, double xBar)
        {
            MatrixD d = (xBar == 0.0) ? x : (x - xBar);
            double d2Sum = Sum(Square(d));
            return d2Sum / x.Count;
        }

        /// <summary>
        /// computes the absolute variance of a vector
        /// with respect to its avarage 
        /// calculated from the values
        /// </summary>
        /// <param name="x"> vector x </param>
        /// <returns> absolute variance </returns>
        public static double Variance(VectorD x)
        {
            double xBar = Sum(x) / x.Count;
            return Variance(x, xBar);
        }

        /// <summary>
        /// computes the absolute variance of a matrix
        /// with respect to its avarage 
        /// calculated from the values
        /// </summary>
        /// <param name="x"> matrix x </param>
        /// <returns> absolute variance </returns>
        public static double Variance(MatrixD x)
        {
            double xBar = Sum(x) / x.Count;
            return Variance(x, xBar);
        }

        /// <summary>
        /// computes the relative variance 
        /// between vector x and y (reference)
        /// </summary>
        /// <param name="x"> vector x </param>
        /// <param name="y"> vector y (reference) </param>
        /// <returns> relative variance </returns>
        public static double Variance(VectorD x, VectorD y)
        {
            VectorD d = x - y;
            double dAbsVar = Variance(d, 0.0);
            double yAbsVar = Variance(y, 0.0);
            return dAbsVar / yAbsVar;
        }

        /// <summary>
        /// computes the relative variance 
        /// between matrix x and y (reference)
        /// </summary>
        /// <param name="x"> matrix x </param>
        /// <param name="y"> matrix y (reference) </param>
        /// <returns> relative variance </returns>
        public static double Variance(MatrixD x, MatrixD y)
        {
            MatrixD d = x - y;
            double dAbsVar = Variance(d, 0.0);
            double yAbsVar = Variance(y, 0.0);
            return dAbsVar / yAbsVar;
        }

        #endregion
        #region complex-valued

        /// <summary>
        /// computes the absolute variance of a vector
        /// with given avarage or expected value
        /// 1/N * SUM[ x_i - x_Bar ]^2
        /// </summary>
        /// <param name="x"> vector x </param>
        /// <param name="xBar"> average or expected value </param>
        /// <returns> absolute variance </returns>
        public static double Variance(VectorZ x, Complex xBar)
        {
            VectorZ d = (xBar == 0.0) ? x : (x - xBar);
            double d2Sum = Sum(Square(Abs(d)));
            return d2Sum / x.Count;
        }

        /// <summary>
        /// computes the absolute variance of a matrix
        /// with given avarage or expected value
        /// 1/N * SUM[ x_i - x_Bar ]^2
        /// </summary>
        /// <param name="x"> matrix x </param>
        /// <param name="xBar"> average or expected value </param>
        /// <returns> absolute variance </returns>
        public static double Variance(MatrixZ x, Complex xBar)
        {
            MatrixZ d = (xBar == 0.0) ? x : (x - xBar);
            double d2Sum = Sum(Square(Abs(d)));
            return d2Sum / x.Count;
        }

        /// <summary>
        /// computes the absolute variance of a vector
        /// with respect to its avarage 
        /// calculated from the values
        /// </summary>
        /// <param name="x"> vector x </param>
        /// <returns> absolute variance </returns>
        public static double Variance(VectorZ x)
        {
            Complex xBar = Sum(x) / x.Count;
            return Variance(x, xBar);
        }

        /// <summary>
        /// computes the absolute variance of a matrix
        /// with respect to its avarage 
        /// calculated from the values
        /// </summary>
        /// <param name="x"> matrix x </param>
        /// <returns> absolute variance </returns>
        public static double Variance(MatrixZ x)
        {
            Complex xBar = Sum(x) / x.Count;
            return Variance(x, xBar);
        }

        /// <summary>
        /// computes the relative variance 
        /// between vector x and y (reference)
        /// </summary>
        /// <param name="x"> vector x </param>
        /// <param name="y"> vector y (reference) </param>
        /// <returns> relative variance </returns>
        public static double Variance(VectorZ x, VectorZ y)
        {
            VectorZ d = x - y;
            double dAbsVar = Variance(d, 0.0);
            double yAbsVar = Variance(y, 0.0);
            return dAbsVar / yAbsVar;
        }

        /// <summary>
        /// computes the relative variance 
        /// between matrix x and y (reference)
        /// </summary>
        /// <param name="x"> matrix x </param>
        /// <param name="y"> matrix y (reference) </param>
        /// <returns> relative variance </returns>
        public static double Variance(MatrixZ x, MatrixZ y)
        {
            MatrixZ d = x - y;
            double dAbsVar = Variance(d, 0.0);
            double yAbsVar = Variance(y, 0.0);
            return dAbsVar / yAbsVar;
        }

        #endregion

        #endregion
        #region ---- StandardDeviation ----

        #region real-valued

        /// <summary>
        /// computes the absolute standard deviation of a vector
        /// with given avarage or expected value
        /// Sqrt{ 1/N * SUM[ x_i - x_Bar ]^2 }
        /// </summary>
        /// <param name="x"> vector x </param>
        /// <param name="xBar"> average or expected value </param>
        /// <returns> absolute standard deviation </returns>
        public static double StandardDeviation(VectorD x, double xBar)
            => Math.Sqrt(Variance(x, xBar));

        /// <summary>
        /// computes the absolute standard deviation of a matrix
        /// with given avarage or expected value
        /// Sqrt{ 1/N * SUM[ x_i - x_Bar ]^2 }
        /// </summary>
        /// <param name="x"> matrix x </param>
        /// <param name="xBar"> average or expected value </param>
        /// <returns> absolute standard deviation </returns>
        public static double StandardDeviation(MatrixD x, double xBar)
            => Math.Sqrt(Variance(x, xBar));

        /// <summary>
        /// computes the absolute standard deviation of a vector
        /// with respect to its avarage 
        /// calculated from the values
        /// </summary>
        /// <param name="x"> vector x </param>
        /// <returns> absolute standard deviation </returns>
        public static double StandardDeviation(VectorD x)
            => Math.Sqrt(Variance(x));

        /// <summary>
        /// computes the absolute standard deviation of a matrix
        /// with respect to its avarage 
        /// calculated from the values
        /// </summary>
        /// <param name="x"> matrix x </param>
        /// <returns> absolute standard deviation </returns>
        public static double StandardDeviation(MatrixD x)
            => Math.Sqrt(Variance(x));

        /// <summary>
        /// computes the relative standard deviation 
        /// between vector x and y (reference)
        /// </summary>
        /// <param name="x"> vector x </param>
        /// <param name="y"> vector y (reference) </param>
        /// <returns> relative standard deviation </returns>
        public static double StandardDeviation(VectorD x, VectorD y)
        {
            VectorD d = x - y;
            double dAbsDev = StandardDeviation(d, 0.0);
            double yAbsDev = StandardDeviation(y, 0.0);
            return dAbsDev / yAbsDev;
        }

        /// <summary>
        /// computes the relative standard deviation 
        /// between matrix x and y (reference)
        /// </summary>
        /// <param name="x"> matrix x </param>
        /// <param name="y"> matrix y (reference) </param>
        /// <returns> relative standard deviation </returns>
        public static double StandardDeviation(MatrixD x, MatrixD y)
        {
            MatrixD d = x - y;
            double dAbsDev = StandardDeviation(d, 0.0);
            double yAbsDev = StandardDeviation(y, 0.0);
            return dAbsDev / yAbsDev;
        }

        #endregion
        #region complex-valued

        /// <summary>
        /// computes the absolute standard deviation of a vector
        /// with given avarage or expected value
        /// Sqrt{ 1/N * SUM[ x_i - x_Bar ]^2 }
        /// </summary>
        /// <param name="x"> vector x </param>
        /// <param name="xBar"> average or expected value </param>
        /// <returns> absolute standard deviation </returns>
        public static double StandardDeviation(VectorZ x, Complex xBar)
            => Math.Sqrt(Variance(x, xBar));

        /// <summary>
        /// computes the absolute standard deviation of a matrix
        /// with given avarage or expected value
        /// Sqrt{ 1/N * SUM[ x_i - x_Bar ]^2 }
        /// </summary>
        /// <param name="x"> matrix x </param>
        /// <param name="xBar"> average or expected value </param>
        /// <returns> absolute standard deviation </returns>
        public static double StandardDeviation(MatrixZ x, Complex xBar)
            => Math.Sqrt(Variance(x, xBar));

        /// <summary>
        /// computes the absolute standard deviation of a vector
        /// with respect to its avarage 
        /// calculated from the values
        /// </summary>
        /// <param name="x"> vector x </param>
        /// <returns> absolute standard deviation </returns>
        public static double StandardDeviation(VectorZ x)
            => Math.Sqrt(Variance(x));

        /// <summary>
        /// computes the absolute standard deviation of a matrix
        /// with respect to its avarage 
        /// calculated from the values
        /// </summary>
        /// <param name="x"> matrix x </param>
        /// <returns> absolute standard deviation </returns>
        public static double StandardDeviation(MatrixZ x)
            => Math.Sqrt(Variance(x));

        /// <summary>
        /// computes the relative standard deviation 
        /// between vector x and y (reference)
        /// </summary>
        /// <param name="x"> vector x </param>
        /// <param name="y"> vector y (reference) </param>
        /// <returns> relative standard deviation </returns>
        public static double StandardDeviation(VectorZ x, VectorZ y)
        {
            VectorZ d = x - y;
            double dAbsDev = StandardDeviation(d, 0.0);
            double yAbsDev = StandardDeviation(y, 0.0);
            return dAbsDev / yAbsDev;
        }

        /// <summary>
        /// computes the relative standard deviation 
        /// between matrix x and y (reference)
        /// </summary>
        /// <param name="x"> matrix x </param>
        /// <param name="y"> matrix y (reference) </param>
        /// <returns> relative standard deviation </returns>
        public static double StandardDeviation(MatrixZ x, MatrixZ y)
        {
            MatrixZ d = x - y;
            double dAbsDev = StandardDeviation(d, 0.0);
            double yAbsDev = StandardDeviation(y, 0.0);
            return dAbsDev / yAbsDev;
        }

        #endregion

        #endregion
        #region ---- (Root) Mean-Square Error (R)MSE ---- 

        #region real-valued

        /// <summary>
        /// computes the mean-square error between
        /// vector x and y (reference)
        /// </summary>
        /// <param name="x"> vector x </param>
        /// <param name="y"> vector y (reference) </param>
        /// <returns> mean-square error </returns>
        public static double MSE(VectorD x, VectorD y)
            => Variance(x - y, 0.0);

        /// <summary>
        /// computes the root mean-square error between
        /// vector x and y (reference)
        /// </summary>
        /// <param name="x"> vector x </param>
        /// <param name="y"> vector y (reference) </param>
        /// <returns> root mean-square error </returns>
        public static double RMSE(VectorD x, VectorD y)
            => Math.Sqrt(MSE(x, y));

        /// <summary>
        /// computes the mean-square error between
        /// matrix x and y (reference)
        /// </summary>
        /// <param name="x"> matrix x </param>
        /// <param name="y"> matrix y (reference) </param>
        /// <returns> mean-square error </returns>
        public static double MSE(MatrixD x, MatrixD y)
            => Variance(x - y, 0.0);

        /// <summary>
        /// computes the root mean-square error between
        /// matrix x and y (reference)
        /// </summary>
        /// <param name="x"> matrix x </param>
        /// <param name="y"> matrix y (reference) </param>
        /// <returns> root mean-square error </returns>
        public static double RMSE(MatrixD x, MatrixD y)
            => Math.Sqrt(MSE(x, y));

        #endregion
        #region complex-valued

        /// <summary>
        /// computes the mean-square error between
        /// vector x and y (reference)
        /// </summary>
        /// <param name="x"> vector x </param>
        /// <param name="y"> vector y (reference) </param>
        /// <returns> mean-square error </returns>
        public static double MSE(VectorZ x, VectorZ y)
            => Variance(x - y, 0.0);

        /// <summary>
        /// computes the root mean-square error between
        /// vector x and y (reference)
        /// </summary>
        /// <param name="x"> vector x </param>
        /// <param name="y"> vector y (reference) </param>
        /// <returns> root mean-square error </returns>
        public static double RMSE(VectorZ x, VectorZ y)
            => Math.Sqrt(MSE(x, y));

        /// <summary>
        /// computes the mean-square error between
        /// matrix x and y (reference)
        /// </summary>
        /// <param name="x"> matrix x </param>
        /// <param name="y"> matrix y (reference) </param>
        /// <returns> mean-square error </returns>
        public static double MSE(MatrixZ x, MatrixZ y)
            => Variance(x - y, 0.0);

        /// <summary>
        /// computes the root mean-square error between
        /// matrix x and y (reference)
        /// </summary>
        /// <param name="x"> matrix x </param>
        /// <param name="y"> matrix y (reference) </param>
        /// <returns> root mean-square error </returns>
        public static double RMSE(MatrixZ x, MatrixZ y)
            => Math.Sqrt(MSE(x, y));

        #endregion

        #endregion
        #region ---- Correlation ----

        #region real-valued

        /// <summary>
        /// computes the correlation between 
        /// two vectors x and y
        /// </summary>
        /// <param name="x"> vector x </param>
        /// <param name="y"> vector y </param>
        /// <returns> correlation </returns>
        public static double Correlation(VectorD x, VectorD y)
        {
            double xBar = Sum(x) / x.Count;
            double yBar = Sum(y) / y.Count;

            double xStd = StandardDeviation(x, xBar);
            double yStd = StandardDeviation(y, yBar);

            VectorD dx = x - xBar;
            VectorD dy = y - yBar;
            double cov = Sum(dx * dy) / x.Count;

            return cov / (xStd * yStd);
        }

        /// <summary>
        /// computes the correlation between 
        /// two matrices x and y
        /// </summary>
        /// <param name="x"> matrix x </param>
        /// <param name="y"> matrix y </param>
        /// <returns> correlation </returns>
        public static double Correlation(MatrixD x, MatrixD y)
        {
            double xBar = Sum(x) / x.Count;
            double yBar = Sum(y) / y.Count;

            double xStd = StandardDeviation(x, xBar);
            double yStd = StandardDeviation(y, yBar);

            MatrixD dx = x - xBar;
            MatrixD dy = y - yBar;
            double cov = Sum(dx * dy) / x.Count;

            return cov / (xStd * yStd);
        }

        #endregion
        #region complex-valued

        /// <summary>
        /// computes the correlation between 
        /// two vectors x and y
        /// </summary>
        /// <param name="x"> vector x </param>
        /// <param name="y"> vector y </param>
        /// <returns> correlation </returns>
        public static double Correlation(VectorZ x, VectorZ y)
        {
            Complex xBar = Sum(x) / x.Count;
            Complex yBar = Sum(y) / y.Count;

            double xStd = StandardDeviation(x, xBar);
            double yStd = StandardDeviation(y, yBar);

            VectorZ dx = x - xBar;
            VectorZ dy = VMath.Conj(y) - yBar; // ??? y - yBar; 
            double cov = ASum(dx * dy) / x.Count;

            return cov / (xStd * yStd);
        }

        /// <summary>
        /// computes the correlation between 
        /// two matrices x and y
        /// </summary>
        /// <param name="x"> matrix x </param>
        /// <param name="y"> matrix y </param>
        /// <returns> correlation </returns>
        public static double Correlation(MatrixZ x, MatrixZ y)
        {
            Complex xBar = Sum(x) / x.Count;
            Complex yBar = Sum(y) / y.Count;

            double xStd = StandardDeviation(x, xBar);
            double yStd = StandardDeviation(y, yBar);

            MatrixZ dx = x - xBar;
            MatrixZ dy = VMath.Conj(y) - yBar; // ??? y - yBar; 
            double cov = ASum(dx * dy) / x.Count;

            return cov / (xStd * yStd);
        }

        #endregion

        #endregion
        #region ---- Difference ----

        #region real-valued

        /// <summary>
        /// computes the first-order central difference 
        /// of y=y(x) in terms of x
        /// result = dy(x)/dx
        /// </summary>
        /// <param name="x"> variables x </param>
        /// <param name="y"> function values y=y(x) </param>
        /// <returns> first-order difference dy(x)/dx </returns>
        public static VectorD Difference1(VectorD x, VectorD y)
        {
            VectorD dx = new(x.Count - 2);
            VectorD dy = new(x.Count - 2);

            new IntelMKL.VMF().Sub(dx.Count, x, x, dx, starta: 2, startb: 0);
            new IntelMKL.VMF().Sub(dy.Count, y, y, dy, starta: 2, startb: 0);

            return Div(dy, dx);
        }

        /// <summary>
        /// computes the first-order central difference 
        /// of y=y(x) in terms of x
        /// result = dy(x)/dx
        /// </summary>
        /// <param name="y"> function y=y(x), stored in scattered data format </param>
        /// <returns> first-order difference dy(x)/dx, stored in scattered data format </returns>
        public static Scat1DRealData Difference1(Scat1DRealData y)
        {
            long n = y.Points.Count;
            VectorD xt = y.Points[new LongRange(1, n - 1)];
            VectorD dy = Difference1(y.Points, y.Values);
            return new Scat1DRealData(xt, dy);
        }

        /// <summary>
        /// computes the second-order central difference 
        /// of y=y(x) in terms of x
        /// result = d2y(x)/dx2
        /// </summary>
        /// <param name="x"> variables x </param>
        /// <param name="y"> function value y=y(x) </param>
        /// <returns> second-order different d2y(x)/dx2 </returns>
        public static VectorD Difference2(VectorD x, VectorD y)
        {
            VectorD dx10 = new(x.Count - 2);
            VectorD dx21 = new(x.Count - 2);
            VectorD dy10 = new(x.Count - 2);
            VectorD dy21 = new(x.Count - 2);

            new IntelMKL.VMF().Sub(dx10.Count, x, x, dx10, starta: 1, startb: 0);
            new IntelMKL.VMF().Sub(dx21.Count, x, x, dx21, starta: 2, startb: 1);
            new IntelMKL.VMF().Sub(dy10.Count, y, y, dy10, starta: 1, startb: 0);
            new IntelMKL.VMF().Sub(dy21.Count, y, y, dy21, starta: 2, startb: 1);

            return Div(dy21 - dy10, dx21 * dx10);
        }

        /// <summary>
        /// computes the second-order central difference 
        /// of y=y(x) in terms of x
        /// result = d2y(x)/dx2
        /// </summary>
        /// <param name="y"> function y=y(x), stored in scattered data format </param>
        /// <returns> second-order different d2y(x)/dx2, stored in scattered data format </returns>
        public static Scat1DRealData Difference2(Scat1DRealData y)
        {
            long n = y.Points.Count;
            VectorD xt = y.Points[new LongRange(1, n - 1)];
            VectorD dy = Difference2(y.Points, y.Values);
            return new Scat1DRealData(xt, dy);
        }

        /// <summary>
        /// computes the first-order central difference 
        /// of y=y(x) in terms of x
        /// result = dy(x)/dx
        /// </summary>
        /// <param name="x"> variables x </param>
        /// <param name="y"> function values y=y(x) </param>
        /// <returns> first-order difference dy(x)/dx </returns>
        [Obsolete("for better performance please use Difference1 instead")]
        public static VectorD Diff(VectorD x, VectorD y)
        {
            VectorD Diff_1 = new VectorD(y.Count - 2, 0.0);
            for (int i = 0; i < Diff_1.Count; i++)
            {
                Diff_1[i] = (y[i + 2] - y[i]) / (x[i + 2] - x[i]);
            }
            return Diff_1;
        }

        /// <summary>
        /// computes the second-order central difference 
        /// of y=y(x) in terms of x
        /// result = d2y(x)/dx2
        /// </summary>
        /// <param name="x"> variables x </param>
        /// <param name="y"> function value y=y(x) </param>
        /// <returns> second-order different d2y(x)/dx2 </returns>
        [Obsolete("for better performance please use Difference2 instead")]
        public static VectorD Diff2(VectorD x, VectorD y)
        {
            VectorD Diff_2 = new VectorD(y.Count - 2, 0.0);
            for (int i = 0; i < Diff_2.Count; i++)
            {
                Diff_2[i] = (y[i + 2] + y[i] - 2 * y[i + 1]) / ((x[i + 2] - x[i + 1]) * (x[i + 1] - x[i]));
            }
            return Diff_2;
        }

        #endregion
        #region complex-valued

        // ...

        #endregion

        #endregion

        #endregion

    }


    internal class VMathOperator
    {

        #region ---- Array + Array [T] ----

        internal static Vect<T> Add<T>(Vect<T> x, Vect<T> y)
            where T : INumber<T>
        {
            Vect<T> z = new(count: x.Count, initMode: ArrayInitMode.Malloc);
            VMath.Add(x, y, z);
            return z;
        }
        internal static Matx<T> Add<T>(Matx<T> x, Matx<T> y)
            where T : INumber<T>
        {
            Matx<T> z = new(rows: x.Rows, cols: x.Cols, initMode: ArrayInitMode.Malloc);
            VMath.Add(x, y, z);
            return z;
        }

        #endregion
        #region ---- Array + Array [D/Z] ----

        internal static T.VectorD Add(T.VectorD x, T.VectorD y)
        {
            T.VectorD z = new(count: x.Count, initMode: ArrayInitMode.Malloc);
            VMath.Add(x, y, z);
            return z;
        }
        internal static T.VectorZ Add(T.VectorZ x, T.VectorZ y)
        {
            T.VectorZ z = new(count: x.Count, initMode: ArrayInitMode.Malloc);
            VMath.Add(x, y, z);
            return z;
        }

        #endregion

        #region ---- Array - Array [T] ----

        internal static Vect<T> Sub<T>(Vect<T> x, Vect<T> y)
            where T : INumber<T>
        {
            Vect<T> z = new(count: x.Count, initMode: ArrayInitMode.Malloc);
            VMath.Sub(x, y, z);
            return z;
        }
        internal static Matx<T> Sub<T>(Matx<T> x, Matx<T> y)
            where T : INumber<T>
        {
            Matx<T> z = new(rows: x.Rows, cols: x.Cols, initMode: ArrayInitMode.Malloc);
            VMath.Sub(x, y, z);
            return z;
        }

        #endregion
        #region ---- Array - Array [D/Z] ----

        internal static T.VectorD Sub(T.VectorD x, T.VectorD y)
        {
            T.VectorD z = new(count: x.Count, initMode: ArrayInitMode.Malloc);
            VMath.Sub(x, y, z);
            return z;
        }
        internal static T.VectorZ Sub(T.VectorZ x, T.VectorZ y)
        {
            T.VectorZ z = new(count: x.Count, initMode: ArrayInitMode.Malloc);
            VMath.Sub(x, y, z);
            return z;
        }

        #endregion

        #region ---- Array * Array [T] ----

        internal static Vect<T> Mul<T>(Vect<T> x, Vect<T> y)
            where T : INumber<T>
        {
            Vect<T> z = new(count: x.Count, initMode: ArrayInitMode.Malloc);
            VMath.Mul(x, y, z);
            return z;
        }
        internal static Matx<T> Mul<T>(Matx<T> x, Matx<T> y)
            where T : INumber<T>
        {
            Matx<T> z = new(rows: x.Rows, cols: x.Cols, initMode: ArrayInitMode.Malloc);
            VMath.Mul(x, y, z);
            return z;
        }

        #endregion
        #region ---- Array * Array [D/Z] ----

        internal static T.VectorD Mul(T.VectorD x, T.VectorD y)
        {
            T.VectorD z = new(count: x.Count, initMode: ArrayInitMode.Malloc);
            VMath.Mul(x, y, z);
            return z;
        }
        internal static T.VectorZ Mul(T.VectorZ x, T.VectorZ y)
        {
            T.VectorZ z = new(count: x.Count, initMode: ArrayInitMode.Malloc);
            VMath.Mul(x, y, z);
            return z;
        }

        #endregion

        #region ---- Array / Array [T] ----

        internal static Vect<T> Div<T>(Vect<T> x, Vect<T> y)
            where T : INumber<T>
        {
            Vect<T> z = new(count: x.Count, initMode: ArrayInitMode.Malloc);
            VMath.Div(x, y, z);
            return z;
        }
        internal static Matx<T> Div<T>(Matx<T> x, Matx<T> y)
            where T : INumber<T>
        {
            Matx<T> z = new(rows: x.Rows, cols: x.Cols, initMode: ArrayInitMode.Malloc);
            VMath.Div(x, y, z);
            return z;
        }

        #endregion
        #region ---- Array / Array [D/Z] ----

        internal static T.VectorD Div(T.VectorD x, T.VectorD y)
        {
            T.VectorD z = new(count: x.Count, initMode: ArrayInitMode.Malloc);
            VMath.Div(x, y, z);
            return z;
        }
        internal static T.VectorZ Div(T.VectorZ x, T.VectorZ y)
        {
            T.VectorZ z = new(count: x.Count, initMode: ArrayInitMode.Malloc);
            VMath.Div(x, y, z);
            return z;
        }

        #endregion

        #region ---- Array + Scalar [T] ----

        internal static Vect<T> Add<T>(Vect<T> x, T s)
            where T : INumber<T>
        {
            Vect<T> y = new(count: x.Count, initMode: ArrayInitMode.Malloc);
            VMath.Copy(x, y);
            VMath.AddTo(y, s);
            return y;
        }
        internal static Matx<T> Add<T>(Matx<T> x, T s)
            where T : INumber<T>
        {
            Matx<T> y = new(rows: x.Rows, cols: x.Cols, initMode: ArrayInitMode.Malloc);
            VMath.Copy(x, y);
            VMath.AddTo(y, s);
            return y;
        }

        #endregion
        #region ---- Array + Scalar [D/Z] ----

        internal static T.VectorD Add(T.VectorD x, Real s)
        {
            T.VectorD y = new(count: x.Count, initMode: ArrayInitMode.Malloc);
            VMath.Copy(x, y);
            VMath.AddTo(y, s);
            return y;
        }
        internal static T.VectorZ Add(T.VectorZ x, Cplx s)
        {
            T.VectorZ y = new(count: x.Count, initMode: ArrayInitMode.Malloc);
            VMath.Copy(x, y);
            VMath.AddTo(y, s);
            return y;
        }

        #endregion

        #region ---- Array - Scalar [T] ----

        internal static Vect<T> Sub<T>(Vect<T> x, T s) 
            where T : INumber<T>
            => Add(x, -s);
        internal static Matx<T> Sub<T>(Matx<T> x, T s)
            where T : INumber<T>
            => Add(x, -s);

        #endregion
        #region ---- Array - Scalar [D/Z] ----

        internal static T.VectorD Sub(T.VectorD x, Real s)
            => Add(x, -s);
        internal static T.VectorZ Sub(T.VectorZ x, Cplx s)
            => Add(x, -s);

        #endregion

        #region ---- Scalar - Array [T] ----

        internal static Vect<T> Sub<T>(T s, Vect<T> x)
            where T : INumber<T>
        {
            Vect<T> y = new(count: x.Count, initMode: ArrayInitMode.Malloc);
            VMath.Copy(x, y);
            VMath.ScaleOn(a: -T.One, y);
            return Add(x, s);
        }
        internal static Matx<T> Sub<T>(T s, Matx<T> x)
            where T : INumber<T>
        {
            Matx<T> y = new(rows: x.Rows, cols: x.Cols, initMode: ArrayInitMode.Malloc);
            VMath.Copy(x, y);
            VMath.ScaleOn(a: -T.One, y);
            return Add(x, s);
        }

        #endregion
        #region ---- Scalar - Array [D/Z] ----

        internal static T.VectorD Sub(Real s, T.VectorD x)
        {
            T.VectorD y = new(count: x.Count, initMode: ArrayInitMode.Malloc);
            VMath.Copy(x, y);
            VMath.ScaleOn(a: -1.0, y);
            return Add(x, s);
        }
        internal static T.VectorZ Sub(Cplx s, T.VectorZ x)
        {
            T.VectorZ y = new(count: x.Count, initMode: ArrayInitMode.Malloc);
            VMath.Copy(x, y);
            VMath.ScaleOn(a: -1.0, y);
            return Add(x, s);
        }

        #endregion

        #region ---- Array * Scalar [T] ----

        internal static Vect<T> Mul<T>(T s, Vect<T> x)
            where T : INumber<T>
        {
            Vect<T> y = new(count: x.Count, initMode: ArrayInitMode.Malloc);
            VMath.Copy(x, y);
            VMath.ScaleOn(a: s, y);
            return y;
        }
        internal static Matx<T> Mul<T>(T s, Matx<T> x)
            where T : INumber<T>
        {
            Matx<T> y = new(rows: x.Rows, cols: x.Cols, initMode: ArrayInitMode.Malloc);
            VMath.Copy(x, y);
            VMath.ScaleOn(a: s, y);
            return y;
        }

        #endregion
        #region ---- Array * Scalar [D/Z] ----

        internal static T.VectorD Mul(Real s, T.VectorD x)
        {
            T.VectorD y = new(count: x.Count, initMode: ArrayInitMode.Malloc);
            VMath.Copy(x, y);
            VMath.ScaleOn(a: s, y);
            return y;
        }
        internal static T.VectorZ Mul(Cplx s, T.VectorZ x)
        {
            T.VectorZ y = new(count: x.Count, initMode: ArrayInitMode.Malloc);
            VMath.Copy(x, y);
            VMath.ScaleOn(a: s, y);
            return y;
        }

        #endregion

        #region ---- Array / Scalar [T] ----

        internal static Vect<T> Div<T>(Vect<T> x, T s)
            where T : INumber<T>
        {
            Vect<T> y = new(count: x.Count, initMode: ArrayInitMode.Malloc);
            VMath.Copy(x, y);
            VMath.ScaleOn(a: T.One / s, y);
            return y;
        }
        internal static Matx<T> Div<T>(Matx<T> x, T s)
            where T : INumber<T>
        {
            Matx<T> y = new(rows: x.Rows, cols: x.Cols, initMode: ArrayInitMode.Malloc);
            VMath.Copy(x, y);
            VMath.ScaleOn(a: T.One / s, y);
            return y;
        }

        #endregion
        #region ---- Array / Scalar [D/Z] ----

        internal static T.VectorD Div(T.VectorD x, Real s)
        {
            T.VectorD y = new(count: x.Count, initMode: ArrayInitMode.Malloc);
            VMath.Copy(x, y);
            VMath.ScaleOn(a: 1.0 / s, y);
            return y;
        }
        internal static T.VectorZ Div(T.VectorZ x, Cplx s)
        {
            T.VectorZ y = new(count: x.Count, initMode: ArrayInitMode.Malloc);
            VMath.Copy(x, y);
            VMath.ScaleOn(a: 1.0 / s, y);
            return y;
        }

        #endregion

        #region ---- Scalar / Array [T] ----

        internal static Vect<T> Div<T>(T s, Vect<T> x)
            where T : INumber<T>
        {
            Vect<T> y = new(count: x.Count, initMode: ArrayInitMode.Malloc);
            VMath.Copy(x, y);
            VMath.Inv(y, y);
            VMath.ScaleOn(a: s, y);
            return y;
        }
        internal static Matx<T> Div<T>(T s, Matx<T> x)
            where T : INumber<T>
        {
            Matx<T> y = new(rows: x.Rows, cols: x.Cols, initMode: ArrayInitMode.Malloc);
            VMath.Copy(x, y);
            VMath.Inv(y, y);
            VMath.ScaleOn(a: s, y);
            return y;
        }

        #endregion
        #region ---- Scalar / Array [D/Z] ----

        internal static T.VectorD Div(Real s, T.VectorD x)
        {
            T.VectorD y = new(count: x.Count, initMode: ArrayInitMode.Malloc);
            VMath.Copy(x, y);
            VMath.Inv(y, y);
            VMath.ScaleOn(a: s, y);
            return y;
        }
        internal static T.VectorZ Div(Cplx s, T.VectorZ x)
        {
            T.VectorZ y = new(count: x.Count, initMode: ArrayInitMode.Malloc);
            VMath.Copy(x, y);
            VMath.Inv(y, y);
            VMath.ScaleOn(a: s, y);
            return y;
        }

        #endregion

        #region ---- (- Array) [T] ----

        internal static Vect<T> Neg<T>(Vect<T> x)
            where T : INumber<T>
        {
            Vect<T> y = new(count: x.Count, initMode: ArrayInitMode.Malloc);
            VMath.Copy(x, y);
            VMath.ScaleOn(a: -T.One, y);
            return y;
        }
        internal static Matx<T> Neg<T>(Matx<T> x)
            where T : INumber<T>
        {
            Matx<T> y = new(rows: x.Rows, cols: x.Cols, initMode: ArrayInitMode.Malloc);
            VMath.Copy(x, y);
            VMath.ScaleOn(a: -T.One, y);
            return y;
        }

        #endregion
        #region ---- (- Array) [D/Z] ----

        internal static T.VectorD Neg(T.VectorD x)
        {
            T.VectorD y = new(count: x.Count, initMode: ArrayInitMode.Malloc);
            VMath.Copy(x, y);
            VMath.ScaleOn(a: -1.0, y);
            return y;
        }
        internal static T.VectorZ Neg(T.VectorZ x)
        {
            T.VectorZ y = new(count: x.Count, initMode: ArrayInitMode.Malloc);
            VMath.Copy(x, y);
            VMath.ScaleOn(a: -1.0, y);
            return y;
        }

        #endregion

        #region ---- ToCplx [T] ----

        internal static Vect<Cplx> ToCplx(Vect<Real> x)
            => VMath.ToCplx(x);
        internal static Matx<Cplx> ToCplx(Matx<Real> x)
            => VMath.ToCplx(x);

        #endregion
        #region ---- ToCplx [D/Z] ----

        internal static T.VectorZ ToCplx(T.VectorD x)
        {
            T.VectorZ y = new(count: x.Count, initMode: ArrayInitMode.Malloc);
            VMath.Modify(x, y, part: ComplexPart.RealPart);
            return y;
        }


        #endregion

    }


}
