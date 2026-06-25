using System.Numerics;

namespace VEMS.AMathCore
{

    /// <summary>
    /// Factory for vector math methods
    /// </summary>
    internal class VMathFactory
    {
        internal IBLAS iBLAS { get; set; }
        internal IVMF iVMF { get; set; }
        //internal ISPBLAS iSPBLAS { get; set; }
        internal VMathFactory()
        {
            iBLAS = Defaults.IBLAS;
            iVMF = Defaults.IVMF;
            //iSPBLAS = Defaults.ISPBLAS;
        }
    }

    /// <summary>
    /// vector math methods
    /// </summary>
    public unsafe class VMath
    {
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

        internal static void AddTo<T>(DenseArray<T> x, T s,
            LoopMode loopMode = Defaults.LoopOption)
            where T : INumber<T>
            => NMath.Axps<T>(n: x.Count, x: (T*)x.VPtr, s: s,
                a: default!, incx: 1, loopMode);

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
        public static (Int, T) Max<T>(DenseArray<T> x,
            LoopMode loopMode = Defaults.LoopOption)
            where T : INumber<T>
        {
            Int iMax = NMath.IMax(n: x.Count, x: x.TPtr, incx: 1);
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
        public static (Int, T) Min<T>(DenseArray<T> x,
            LoopMode loopMode = Defaults.LoopOption)
            where T : INumber<T>
        {
            Int iMin = NMath.IMin(n: x.Count, x: x.TPtr, incx: 1);
            return (iMin, *(x.TPtr + iMin));
        }

        #endregion
        #region ---- Sort ----

        internal static void Sort<T>(DenseArray<T> x)
            where T : INumber<T>
            => NMath.Sort(x: x.TPtr, low: 0, high: x.Count - 1);

        /// <summary>
        /// Sorts the elements of the specified <see cref="DenseArray{T}"/> in ascending order.
        /// </summary>
        /// <typeparam name="T">The numeric type of the array elements. Must implement <see cref="INumber{T}"/>.</typeparam>
        /// <param name="x">The array to sort. The array is sorted in place.</param>
        public static void Sort<T>(ref DenseArray<T> x)
            where T : INumber<T>
             => Sort(x);

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
            T value, out Int index) where T : INumber<T>
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
            Int rows = x.GetLength(0);
            Int cols = x.GetLength(1);
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
            Int startIndex = 0)
        {
            DenseArray<Real> y = new(count: x.Count, initMode: ArrayInitMode.Malloc);
            Copy(x, ref y);
            NMath.Unwrap2PI(n: y.Count, x: y.DPtr, startIndex);
            return y;
        }

        #endregion

        #endregion
        #region BLAS methods

        #region helpers 

        // ...

        #endregion
        #region ---- IAmx [T] ----

        /// <summary>
        /// Finds the index of the element with the largest absolute value (magnitude) in an array.
        /// </summary>
        /// <param name="x">The input array to examine.</param>
        /// <param name="incx">Increment for indexing <paramref name="x"/> (default is 1).</param>
        /// <returns> The zero-based index of the element whose absolute value is maximal.</returns>
        public static Int IAmx<T>(DenseArray<T> x,
            Int incx = 1)
            where T : INumber<T>
        {
            if (typeof(T) == typeof(Real)) 
                return factory.iBLAS.Iamax(x.Count, x.DPtr, incx);
            else if (typeof(T) == typeof(Cplx)) 
                return factory.iBLAS.Iamax(x.Count, x.VPtr, incx);
            else throw new NotSupportedException($"Type {typeof(T)} not supported.");
        }

        #endregion
        #region ---- IAmn [T] ----

        /// <summary>
        /// Finds the index of the element with the smallest absolute value (magnitude) in an array.
        /// </summary>
        /// <param name="x">The input array to examine.</param>
        /// <param name="incx">Increment for indexing <paramref name="x"/> (default is 1).</param>
        /// <returns> The zero-based index of the element whose absolute value is maximal.</returns>
        public static Int IAmn<T>(DenseArray<T> x,
            Int incx = 1)
            where T : INumber<T>
        {
            if (typeof(T) == typeof(Real)) 
                return factory.iBLAS.Iamin(x.Count, x.DPtr, incx);
            else if (typeof(T) == typeof(Cplx)) 
                return factory.iBLAS.Iamin(x.Count, x.VPtr, incx);
            else throw new NotSupportedException($"Type {typeof(T)} not supported.");
        }

        #endregion
        #region ---- ASum [T] ----

        /// <summary>
        /// Computes the sum of absolute values of elements in the specified array.
        /// </summary>
        /// <param name="x">The input array whose absolute values will be summed.</param>
        /// <param name="incx">Increment for indexing <paramref name="x"/> (default is 1).</param>
        /// <returns>The sum of absolute values of the elements in <paramref name="x"/> as a <see cref="Real"/>.</returns>
        public static Real ASum<T>(DenseArray<T> x,
            Int incx = 1)
            where T : INumber<T>
        {
            if (typeof(T) == typeof(Real)) 
                return factory.iBLAS.Asum(x.Count, x.DPtr, incx);
            else if (typeof(T) == typeof(Cplx)) 
                return factory.iBLAS.Asum(x.Count, x.VPtr, incx);
            else throw new NotSupportedException($"Type {typeof(T)} not supported.");
        }

        #endregion
        #region ---- Copy [T] ----

        internal static void Copy<T>(DenseArray<T> x, DenseArray<T> y,
            Int incx = 1, Int incy = 1)
            where T : INumber<T>
        {
            if (typeof(T) == typeof(Real)) 
                factory.iBLAS.Copy(x.Count, x.DPtr, y.DPtr, incx, incy);
            else if (typeof(T) == typeof(Cplx)) 
                factory.iBLAS.Copy(x.Count, x.VPtr, y.VPtr, incx, incy);
            else throw new NotSupportedException($"Type {typeof(T)} not supported.");
        }

        /// <summary>
        /// Copies elements from the source array <paramref name="x"/> to the destination array <paramref name="y"/>.
        /// </summary>
        /// <param name="x">The source array to copy from. The number of elements copied is <c>x.Count</c>.</param>
        /// <param name="y">The destination array to copy to. This is modified in-place and must be allocated with sufficient length.</param>
        /// <param name="incx">Increment (stride) for the source array <paramref name="x"/>. Default is <c>1</c>.</param>
        /// <param name="incy">Increment (stride) for the destination array <paramref name="y"/>. Default is <c>1</c>.</param>
        public static void Copy<T>(DenseArray<T> x, ref DenseArray<T> y,
            Int incx = 1, Int incy = 1)
            where T : INumber<T>
            => Copy(x, y, incx, incy);

        #endregion
        #region ---- Swap [T] ----

        internal static void Swap<T>(DenseArray<T> x, DenseArray<T> y,
            Int incx = 1, Int incy = 1)
            where T : INumber<T>
        {
            if (typeof(T) == typeof(Real)) 
                factory.iBLAS.Swap(x.Count, x.DPtr, y.DPtr, incx, incy);
            else if (typeof(T) == typeof(Cplx)) 
                factory.iBLAS.Swap(x.Count, x.VPtr, y.VPtr, incx, incy);
            else throw new NotSupportedException($"Type {typeof(T)} not supported.");
        }

        /// <summary>
        /// Swaps the elements of two dense arrays of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="x">The first array.</param>
        /// <param name="y">The second array.</param>
        /// <param name="incx">Increment for indexing <paramref name="x"/>. Default is 1.</param>
        /// <param name="incy">Increment for indexing <paramref name="y"/>. Default is 1.</param>
        public static void Swap<T>(ref DenseArray<T> x, ref DenseArray<T> y,
            Int incx = 1, Int incy = 1)
            where T : INumber<T>
            => Swap(x, y, incx, incy);

        #endregion
        #region ---- Add [T] ----

        internal static void Add<T>(DenseArray<T> x, DenseArray<T> y,
            T a = default!, Int incx = 1, Int incy = 1)
            where T : INumber<T>
        {
            if (a == default) { a = T.CreateChecked(1.0); }

            if (typeof(T) == typeof(Real)) 
                factory.iBLAS.Axpy(x.Count, Convert.ToDouble(a), x.DPtr, y.DPtr, incx, incy);
            else if (typeof(T) == typeof(Cplx)) 
                factory.iBLAS.Axpy(x.Count, &a, x.VPtr, y.VPtr, incx, incy);
            else throw new NotSupportedException($"Type {typeof(T)} not supported.");
        }

        /// <summary>
        /// Performs the operation: y := a * x + y
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="x">Input array x.</param>
        /// <param name="y">Input/output array y.</param>
        /// <param name="a">Scalar multiplier for <paramref name="x"/>. Default is 1.</param>
        /// <param name="incx">Increment for indexing <paramref name="x"/>. Default is 1.</param>
        /// <param name="incy">Increment for indexing <paramref name="y"/>. Default is 1.</param>
        public static void Add<T>(DenseArray<T> x, ref DenseArray<T> y,
            T a = default!, Int incx = 1, Int incy = 1)
            where T : INumber<T>
            => Add(x, y, a, incx, incy);

        #endregion
        #region ---- Scal [T] ----

        internal static void Scal<T>(T a, DenseArray<T> x,
            Int incx = 1)
            where T : INumber<T>
        {
            if (typeof(T) == typeof(Real)) 
                factory.iBLAS.Scal(x.Count, Convert.ToDouble(a), x.DPtr, incx);
            else if (typeof(T) == typeof(Cplx)) 
                factory.iBLAS.Scal(x.Count, &a, x.VPtr, incx);
            else throw new NotSupportedException($"Type {typeof(T)} not supported.");
        }

        /// <summary>
        /// Performs the operation: x := a * x
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="a">The scalar multiplier.</param>
        /// <param name="x">The array to be scaled.</param>
        /// <param name="incx">Increment for indexing <paramref name="x"/>. Default is 1.</param>
        public static void Scal<T>(T a, ref DenseArray<T> x,
            Int incx = 1)
            where T : INumber<T>
            => Scal(a, x, incx);

        #endregion
        #region ---- Scal [Z] ----

        internal static void Scal(Real a, DenseArray<Cplx> x,
            Int incx = 1)
            => factory.iBLAS.Scal(x.Count, a, x.VPtr, incx);

        /// <summary>
        /// Performs the operation: x := a * x
        /// </summary>
        /// <param name="a">The real scalar multiplier.</param>
        /// <param name="x">The complex array to be scaled.</param>
        /// <param name="incx">Increment for indexing <paramref name="x"/>. Default is 1.</param>
        public static void Scal(Real a, ref DenseArray<Cplx> x,
            Int incx = 1)
            => Scal(a, x, incx);

        #endregion
        #region ---- Norm [T] ----

        /// <summary>
        /// Computes the Euclidean norm (L2 norm) of an array.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="x">The array whose norm is to be computed.</param>
        /// <param name="incx">Increment for indexing <paramref name="x"/>. Default is 1.</param>
        /// <returns>The Euclidean norm (L2 norm) of the array.</returns>
        public static Real Norm<T>(DenseArray<T> x,
            Int incx = 1)
            where T : INumber<T>
        {
            if (typeof(T) == typeof(Real)) 
                return factory.iBLAS.Nrm2(x.Count, x.DPtr, incx);
            else if (typeof(T) == typeof(Cplx)) 
                return factory.iBLAS.Nrm2(x.Count, x.VPtr, incx);
            else throw new NotSupportedException($"Type {typeof(T)} not supported.");
        }

        #endregion
        #region ---- Rot [T] ----

        internal static void Rot<T>(DenseArray<T> x, DenseArray<T> y,
            Real c, Real s, Int incx = 1, Int incy = 1)
            where T : INumber<T>
        {
            if (typeof(T) == typeof(Real)) 
                factory.iBLAS.Rot(x.Count, x.DPtr, y.DPtr, c, s, incx, incy);
            else if (typeof(T) == typeof(Cplx)) 
                factory.iBLAS.Rot(x.Count, x.VPtr, y.VPtr, c, s, incx, incy);
            else throw new NotSupportedException($"Type {typeof(T)} not supported.");
        } 

        /// <summary>
        /// Performs rotation of points in the plane for two arrays.
        /// Each element is updated as:
        /// xi = c * xi + s * yi
        /// yi = c * yi - s * xi
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="x">The first array.</param>
        /// <param name="y">The second array.</param>
        /// <param name="c">Cosine of the rotation angle.</param>
        /// <param name="s">Sine of the rotation angle.</param>
        /// <param name="incx">Increment for indexing <paramref name="x"/>. Default is 1.</param>
        /// <param name="incy">Increment for indexing <paramref name="y"/>. Default is 1.</param>
        public static void Rot<T>(ref DenseArray<T> x, ref DenseArray<T> y,
            Real c, Real s, Int incx = 1, Int incy = 1)
            where T : INumber<T>
            => Rot(x, y, c, s, incx, incy);

        #endregion

        #endregion
        #region VMF methods

        #region ---- Abs [D/Z] ----

        internal static void Abs<T>(DenseArray<T> x, DenseArray<Real> y)
            where T : INumber<T>
        {
            if (typeof(T) == typeof(Real)) factory.iVMF.Abs(x.Count, x.DPtr, y.DPtr);
            else if (typeof(T) == typeof(Cplx)) factory.iVMF.Abs(x.Count, x.VPtr, y.DPtr);
            else throw new NotSupportedException($"Type {typeof(T)} not supported.");
        }

        /// <summary>
        /// Computes the absolute value of each element in the input array <paramref name="x"/> 
        /// and stores the result in <paramref name="y"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="x">The input array containing elements to compute the absolute value for.</param>
        /// <param name="y">The output array where the absolute values will be stored.</param>
        /// <exception cref="NotSupportedException"></exception>
        public static void Abs<T>(DenseArray<T> x, ref DenseArray<Real> y)
            where T : INumber<T>
            => Abs(x, y);

        #endregion
        #region ---- Arg [Z] ----

        internal static void Arg(DenseArray<Cplx> x, DenseArray<Real> y)
            => factory.iVMF.Arg(x.Count, x.VPtr, y.DPtr);

        /// <summary>
        /// Computes the argument (phase angle) of each element in a complex array.
        /// yi = arg[xi]
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="x">The input array of complex numbers.</param>
        /// <param name="y">The output array to store the arguments (phase angles).</param>
        /// <exception cref="NotSupportedException"></exception>
        public static void Arg(DenseArray<Cplx> x, ref DenseArray<Real> y)
            => Arg(x, y);

        #endregion
        #region ---- Add [D/Z] ----

        internal static void Add<T>(DenseArray<T> x, DenseArray<T> y,
            DenseArray<T> z)
            where T : INumber<T>
        {
            if (typeof(T) == typeof(Real)) factory.iVMF.Add(x.Count, x.DPtr, y.DPtr, z.DPtr);
            else if (typeof(T) == typeof(Cplx)) factory.iVMF.Add(x.Count, x.VPtr, y.VPtr, z.VPtr);
            else throw new NotSupportedException($"Type {typeof(T)} not supported.");
        }

        /// <summary>
        /// Performs element-wise addition of two arrays and stores the result in a third array.
        /// </summary>
        /// <typeparam name="T">The numeric type of the array elements, implementing <see cref="INumber{T}"/>.</typeparam>
        /// <param name="x">The first input array.</param>
        /// <param name="y">The second input array.</param>
        /// <param name="z">The array to store the result of the addition.</param>
        public static void Add<T>(DenseArray<T> x, DenseArray<T> y,
            ref DenseArray<T> z)
            where T : INumber<T>
            => Add(x, y, z);

        #endregion
        #region ---- Sub [D/Z] ----

        internal static void Sub<T>(DenseArray<T> x, DenseArray<T> y,
            DenseArray<T> z)
            where T : INumber<T>
        {
            if (typeof(T) == typeof(Real)) factory.iVMF.Sub(x.Count, x.DPtr, y.DPtr, z.DPtr);
            else if (typeof(T) == typeof(Cplx)) factory.iVMF.Sub(x.Count, x.VPtr, y.VPtr, z.VPtr);
            else throw new NotSupportedException($"Type {typeof(T)} not supported.");
        }

        /// <summary>
        /// Performs element-wise subtraction of two arrays.
        /// Each element of <paramref name="y"/> is subtracted from the corresponding element of <paramref name="x"/>,
        /// and the result is stored in <paramref name="z"/>.
        /// </summary>
        /// <typeparam name="T">A numeric type implementing <see cref="INumber{T}"/>.</typeparam>
        /// <param name="x">The first input array.</param>
        /// <param name="y">The second input array to subtract from <paramref name="x"/>.</param>
        /// <param name="z">The array to store the result of the subtraction.</param>
        public static void Sub<T>(DenseArray<T> x, DenseArray<T> y,
            ref DenseArray<T> z)
            where T : INumber<T>
            => Sub(x, y, z);

        #endregion
        #region ---- Inv [D/Z?] ----

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

        /// <summary>
        /// Computes the element-wise inverse of the input array <paramref name="x"/> and stores the result in <paramref name="y"/>.
        /// Each element is calculated as y[i] = 1 / x[i].
        /// </summary>
        /// <typeparam name="T">An array type implementing <see cref="INumber{T}"/>.</typeparam>
        /// <param name="x">The input array whose elements will be inverted.</param>
        /// <param name="y">The output array where the inverted elements will be stored.</param>
        public static void Inv<T>(DenseArray<T> x, ref DenseArray<T> y)
            where T : INumber<T>
            => Inv(x, y);

        #endregion
        #region ---- Sqrt [D/Z] ----

        internal static void Sqrt<T>(DenseArray<T> x, DenseArray<T> y)
            where T : INumber<T>
        {
            if (typeof(T) == typeof(Real)) factory.iVMF.Sqrt(x.Count, x.DPtr, y.DPtr);
            else if (typeof(T) == typeof(Cplx)) factory.iVMF.Sqrt(x.Count, x.VPtr, y.VPtr);
            else throw new NotSupportedException($"Type {typeof(T)} not supported.");
        }

        /// <summary>
        /// Computes the square root of each element in the input array <paramref name="x"/> and stores the result in the output array <paramref name="y"/>.
        /// </summary>
        /// <typeparam name="T">The numeric type of the array elements. Must implement <see cref="INumber{T}"/>.</typeparam>
        /// <param name="x">The input array containing the values to compute the square root for.</param>
        /// <param name="y">The output array where the computed square root values will be stored.</param>
        public static void Sqrt<T>(DenseArray<T> x, ref DenseArray<T> y)
            where T : INumber<T>
            => Sqrt(x, y);

        #endregion
        #region ---- InvSqrt [D] ----

        internal static void InvSqrt(DenseArray<Real> x, DenseArray<Real> y)
            => factory.iVMF.InvSqrt(x.Count, x.DPtr, y.DPtr);

        /// <summary>
        /// Computes the inverse square root of each element in an array of real numbers.
        /// Each element of the output array <paramref name="y"/> is set to 1.0 divided by the square root of the corresponding element of the input array <paramref name="x"/>.
        /// y[i] = 1.0 / sqrt(x[i])
        /// </summary>
        /// <param name="x">The input array of real numbers.</param>
        /// <param name="y">The output array to store the inverse square roots. Must be pre-allocated and have the same length as <paramref name="x"/>.</param>
        public static void InvSqrt(DenseArray<Real> x, ref DenseArray<Real> y)
            => InvSqrt(x, y);

        #endregion
        #region ---- Cbrt [D] ----

        internal static void Cbrt(DenseArray<Real> x, DenseArray<Real> y)
            => factory.iVMF.Cbrt(x.Count, x.DPtr, y.DPtr);

        /// <summary>
        /// Computes the cube root of each element in an array of real numbers.
        /// Each element of the output array <paramref name="y"/> is set to the cube root of the corresponding element of the input array <paramref name="x"/>.
        /// y[i] = cbrt(x[i])
        /// </summary>
        /// <param name="x">The input array of real numbers.</param>
        /// <param name="y">The output array to store the cube roots. Must be pre-allocated and have the same length as <paramref name="x"/>.</param>
        public static void Cbrt(DenseArray<Real> x, ref DenseArray<Real> y)
            => Cbrt(x, y);

        #endregion
        #region ---- InvCbrt [D] ----

        internal static void InvCbrt(DenseArray<Real> x, DenseArray<Real> y)
            => factory.iVMF.InvCbrt(x.Count, x.DPtr, y.DPtr);

        /// <summary>
        /// Computes the inverse cube root of each element in the input array <paramref name="x"/> and stores the result in <paramref name="y"/>.
        /// Each element of the output array y is set to 1.0 divided by the cube root of the corresponding element of the input array x.
        /// y[i] = 1.0 / cbrt(x[i])
        /// </summary>
        /// <param name="x">The input array of real numbers.</param>
        /// <param name="y">The output array to store the inverse cube roots. Must be pre-allocated and have the same length as <paramref name="x"/>.</param>
        public static void InvCbrt(DenseArray<Real> x, ref DenseArray<Real> y)
            => InvCbrt(x, y);

        #endregion
        #region ---- Sqr [D] ----

        internal static void Sqr(DenseArray<Real> x, DenseArray<Real> y)
            => factory.iVMF.Sqr(x.Count, x.DPtr, y.DPtr);

        /// <summary>
        /// Computes the element-wise square of each element in an array of real numbers.
        /// Each element of the output array <paramref name="y"/> is set to the square of the corresponding element of the input array <paramref name="x"/>.
        /// y[i] = x[i]^2
        /// </summary>
        /// <param name="x">The input array of real numbers.</param>
        /// <param name="y">The output array to store the squared values. Must be pre-allocated and have the same length as <paramref name="x"/>.</param>
        public static void Sqr(DenseArray<Real> x, ref DenseArray<Real> y)
            => factory.iVMF.Sqr(x.Count, x.DPtr, y.DPtr);

        #endregion
        #region ---- Exp [D/Z] ----

        internal static void Exp<T>(DenseArray<T> x, DenseArray<T> y)
            where T : INumber<T>
        {
            if (typeof(T) == typeof(Real)) factory.iVMF.Exp(x.Count, x.DPtr, y.DPtr);
            else if (typeof(T) == typeof(Cplx)) factory.iVMF.Exp(x.Count, x.VPtr, y.VPtr);
            else throw new NotSupportedException($"Type {typeof(T)} not supported.");
        }

        /// <summary>
        /// Computes the exponential of each element in the input array <paramref name="x"/> and stores the result in <paramref name="y"/>.
        /// </summary>
        /// <typeparam name="T">The type of the array elements, must implement <see cref="INumber{T}"/>.</typeparam>
        /// <param name="x">The input array whose elements will be exponentiated.</param>
        /// <param name="y">The output array where the results will be stored.</param>
        public static void Exp<T>(DenseArray<T> x, ref DenseArray<T> y)
            where T : INumber<T>
            => Exp(x, y);

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

        internal static void Ln<T>(DenseArray<T> x, DenseArray<T> y)
            where T : INumber<T>
        {
            if (typeof(T) == typeof(Real)) factory.iVMF.Ln(x.Count, x.DPtr, y.DPtr);
            else if (typeof(T) == typeof(Cplx)) factory.iVMF.Ln(x.Count, x.VPtr, y.VPtr);
            else throw new NotSupportedException($"Type {typeof(T)} not supported.");
        }

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

        internal static void Log10<T>(DenseArray<T> x, DenseArray<T> y)
            where T : INumber<T>
        {
            if (typeof(T) == typeof(Real)) factory.iVMF.Log10(x.Count, x.DPtr, y.DPtr);
            else if (typeof(T) == typeof(Cplx)) factory.iVMF.Log10(x.Count, x.VPtr, y.VPtr);
            else throw new NotSupportedException($"Type {typeof(T)} not supported.");
        }

        /// <summary>
        /// Computes the base 10 logarithm of each element in the input array <paramref name="x"/> and stores the result in <paramref name="y"/>.
        /// </summary>
        /// <typeparam name="T">A numeric type implementing <see cref="INumber{T}"/>.</typeparam>
        /// <param name="x">Input array whose elements will be processed.</param>
        /// <param name="y">Reference to the output array where the results will be stored.</param>
        public static void Log10<T>(DenseArray<T> x, ref DenseArray<T> y)
            where T : INumber<T>
            => Log10(x, y);

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

        internal static void Cos<T>(DenseArray<T> x, DenseArray<T> y)
            where T : INumber<T>
        {
            if (typeof(T) == typeof(Real)) factory.iVMF.Cos(x.Count, x.DPtr, y.DPtr);
            else if (typeof(T) == typeof(Cplx)) factory.iVMF.Cos(x.Count, x.VPtr, y.VPtr);
            else throw new NotSupportedException($"Type {typeof(T)} not supported.");
        }

        /// <summary>
        /// Computes the cosine of each element in the input array <paramref name="x"/> and stores the result in <paramref name="y"/>.
        /// </summary>
        /// <typeparam name="T">The numeric type of the array elements, implementing <see cref="INumber{T}"/>.</typeparam>
        /// <param name="x">The input array containing the values for which to compute the cosine.</param>
        /// <param name="y">The output array where the computed cosine values will be stored.</param>
        public static void Cos<T>(DenseArray<T> x, ref DenseArray<T> y)
            where T : INumber<T>
            => Cos(x, y);

        #endregion
        #region ---- Sin [D/Z] ----

        internal static void Sin<T>(DenseArray<T> x, DenseArray<T> y)
            where T : INumber<T>
        {
            if (typeof(T) == typeof(Real)) factory.iVMF.Sin(x.Count, x.DPtr, y.DPtr);
            else if (typeof(T) == typeof(Cplx)) factory.iVMF.Sin(x.Count, x.VPtr, y.VPtr);
            else throw new NotSupportedException($"Type {typeof(T)} not supported.");
        }

        /// <summary>
        /// Computes the sine of each element in the input array <paramref name="x"/> and stores the result in <paramref name="y"/>.
        /// </summary>
        /// <typeparam name="T">The numeric type of the array elements, implementing <see cref="INumber{T}"/>.</typeparam>
        /// <param name="x">The input array containing the values for which to compute the sine.</param>
        /// <param name="y">The output array where the computed sine values will be stored.</param>
        public static void Sin<T>(DenseArray<T> x, ref DenseArray<T> y)
            where T : INumber<T>
            => Sin(x, y);

        #endregion
        #region ---- Tan [D/Z] ----

        internal static void Tan<T>(DenseArray<T> x, DenseArray<T> y)
            where T : INumber<T>
        {
            if (typeof(T) == typeof(Real)) factory.iVMF.Tan(x.Count, x.DPtr, y.DPtr);
            else if (typeof(T) == typeof(Cplx)) factory.iVMF.Tan(x.Count, x.VPtr, y.VPtr);
            else throw new NotSupportedException($"Type {typeof(T)} not supported.");
        }

        /// <summary>
        /// Computes the tangent of each element in the input array <paramref name="x"/> and stores the result in <paramref name="y"/>.
        /// </summary>
        /// <typeparam name="T">The numeric type of the array elements, implementing <see cref="INumber{T}"/>.</typeparam>
        /// <param name="x">The input array containing the values for which to compute the tangent.</param>
        /// <param name="y">The output array where the computed tangent values will be stored.</param>

        public static void Tan<T>(DenseArray<T> x, ref DenseArray<T> y)
            where T : INumber<T>
            => Tan(x, y);

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

        internal static void Cosh<T>(DenseArray<T> x, DenseArray<T> y)
            where T : INumber<T>
        {
            if (typeof(T) == typeof(Real)) factory.iVMF.Cosh(x.Count, x.DPtr, y.DPtr);
            else if (typeof(T) == typeof(Cplx)) factory.iVMF.Cosh(x.Count, x.VPtr, y.VPtr);
            else throw new NotSupportedException($"Type {typeof(T)} not supported.");
        }

        /// <summary>
        /// Computes the hyperbolic cosine of each element in the input array <paramref name="x"/> and stores the result in <paramref name="y"/>.
        /// </summary>
        /// <typeparam name="T">The numeric type of the array elements, implementing <see cref="INumber{T}"/>.</typeparam>
        /// <param name="x">The input array whose elements will be processed.</param>
        /// <param name="y">The output array where the results will be stored.</param>
        public static void Cosh<T>(DenseArray<T> x, ref DenseArray<T> y)
            where T : INumber<T>
            => Cosh(x, y);

        #endregion
        #region ---- Sinh [D/Z] ----

        internal static void Sinh<T>(DenseArray<T> x, DenseArray<T> y)
            where T : INumber<T>
        {
            if (typeof(T) == typeof(Real)) factory.iVMF.Sinh(x.Count, x.DPtr, y.DPtr);
            else if (typeof(T) == typeof(Cplx)) factory.iVMF.Sinh(x.Count, x.VPtr, y.VPtr);
            else throw new NotSupportedException($"Type {typeof(T)} not supported.");
        }

        /// <summary>
        /// Computes the hyperbolic sine of each element in the input array <paramref name="x"/> and stores the result in <paramref name="y"/>.
        /// </summary>
        /// <typeparam name="T">The numeric type of the array elements, implementing <see cref="INumber{T}"/>.</typeparam>
        /// <param name="x">The input array containing the values for which to compute the hyperbolic sine.</param>
        /// <param name="y">The output array where the computed hyperbolic sine values will be stored.</param>
        public static void Sinh<T>(DenseArray<T> x, ref DenseArray<T> y)
            where T : INumber<T>
            => Sinh(x, y);

        #endregion
        #region ---- Tanh [D/Z] ----

        internal static void Tanh<T>(DenseArray<T> x, DenseArray<T> y)
            where T : INumber<T>
        {
            if (typeof(T) == typeof(Real)) factory.iVMF.Tanh(x.Count, x.DPtr, y.DPtr);
            else if (typeof(T) == typeof(Cplx)) factory.iVMF.Tanh(x.Count, x.VPtr, y.VPtr);
            else throw new NotSupportedException($"Type {typeof(T)} not supported.");
        }

        /// <summary>
        /// Computes the hyperbolic tangent of each element in the input array <paramref name="x"/> and stores the result in <paramref name="y"/>.
        /// </summary>
        /// <typeparam name="T">The numeric type of the array elements, implementing <see cref="INumber{T}"/>.</typeparam>
        /// <param name="x">The input array containing the values for which to compute the hyperbolic tangent.</param>
        /// <param name="y">The output array where the computed hyperbolic tangent values will be stored.</param>
        public static void Tanh<T>(DenseArray<T> x, ref DenseArray<T> y)
            where T : INumber<T>
            => Tanh(x, y);

        #endregion
        #region ---- Acos [D/Z] ----

        internal static void Acos<T>(DenseArray<T> x, DenseArray<T> y)
            where T : INumber<T>
        {
            if (typeof(T) == typeof(Real)) factory.iVMF.Acos(x.Count, x.DPtr, y.DPtr);
            else if (typeof(T) == typeof(Cplx)) factory.iVMF.Acos(x.Count, x.VPtr, y.VPtr);
            else throw new NotSupportedException($"Type {typeof(T)} not supported.");
        }

        /// <summary>
        /// Computes the arccos of each element in the input array <paramref name="x"/> and stores the result in <paramref name="y"/>.
        /// </summary>
        /// <typeparam name="T">The numeric type of the array elements, implementing <see cref="INumber{T}"/>.</typeparam>
        /// <param name="x">The input array containing the values for which to compute the arccos.</param>
        /// <param name="y">The output array where the computed arccos values will be stored.</param>
        public static void Acos<T>(DenseArray<T> x, ref DenseArray<T> y)
            where T : INumber<T>
            => Acos(x, y);

        #endregion
        #region ---- Asin [D/Z] ----

        internal static void Asin<T>(DenseArray<T> x, DenseArray<T> y)
            where T : INumber<T>
        {
            if (typeof(T) == typeof(Real)) factory.iVMF.Asin(x.Count, x.DPtr, y.DPtr);
            else if (typeof(T) == typeof(Cplx)) factory.iVMF.Asin(x.Count, x.VPtr, y.VPtr);
            else throw new NotSupportedException($"Type {typeof(T)} not supported.");
        }

        /// <summary>
        /// Computes the arcsin of each element in the input array <paramref name="x"/> and stores the result in <paramref name="y"/>.
        /// </summary>
        /// <typeparam name="T">The numeric type of the array elements, implementing <see cref="INumber{T}"/>.</typeparam>
        /// <param name="x">The input array containing the values for which to compute the arcsin.</param>
        /// <param name="y">The output array where the computed arcsin values will be stored.</param>
        public static void Asin<T>(DenseArray<T> x, ref DenseArray<T> y)
            where T : INumber<T>
            => Asin(x, y);

        #endregion
        #region ---- Atan [D/Z] ----

        internal static void Atan<T>(DenseArray<T> x, DenseArray<T> y)
            where T : INumber<T>
        {
            if (typeof(T) == typeof(Real)) factory.iVMF.Atan(x.Count, x.DPtr, y.DPtr);
            else if (typeof(T) == typeof(Cplx)) factory.iVMF.Atan(x.Count, x.VPtr, y.VPtr);
            else throw new NotSupportedException($"Type {typeof(T)} not supported.");
        }

        /// <summary>
        /// Computes the arctan of each element in the input array <paramref name="x"/> and stores the result in <paramref name="y"/>.
        /// </summary>
        /// <typeparam name="T">The numeric type of the array elements, implementing <see cref="INumber{T}"/>.</typeparam>
        /// <param name="x">The input array containing the values for which to compute the arctan.</param>
        /// <param name="y">The output array where the computed arctan values will be stored.</param>
        public static void Atan<T>(DenseArray<T> x, ref DenseArray<T> y)
            where T : INumber<T>
            => Atan(x, y);

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

        internal static void Acosh<T>(DenseArray<T> x, DenseArray<T> y)
            where T : INumber<T>
        {
            if (typeof(T) == typeof(Real)) factory.iVMF.Acosh(x.Count, x.DPtr, y.DPtr);
            else if (typeof(T) == typeof(Cplx)) factory.iVMF.Acosh(x.Count, x.VPtr, y.VPtr);
            else throw new NotSupportedException($"Type {typeof(T)} not supported.");
        }

        /// <summary>
        /// Computes the acosh of each element in the input array <paramref name="x"/> and stores the result in <paramref name="y"/>.
        /// </summary>
        /// <typeparam name="T">The numeric type of the array elements, implementing <see cref="INumber{T}"/>.</typeparam>
        /// <param name="x">The input array containing the values for which to compute the acosh.</param>
        /// <param name="y">The output array where the computed acosh values will be stored.</param>
        public static void Acosh<T>(DenseArray<T> x, ref DenseArray<T> y)
            where T : INumber<T>
            => Acosh(x, y);

        #endregion
        #region ---- Asinh [D/Z] ----

        internal static void Asinh<T>(DenseArray<T> x, DenseArray<T> y)
            where T : INumber<T>
        {
            if (typeof(T) == typeof(Real)) factory.iVMF.Asinh(x.Count, x.DPtr, y.DPtr);
            else if (typeof(T) == typeof(Cplx)) factory.iVMF.Asinh(x.Count, x.VPtr, y.VPtr);
            else throw new NotSupportedException($"Type {typeof(T)} not supported.");
        }

        /// <summary>
        /// Computes the asinh of each element in the input array <paramref name="x"/> and stores the result in <paramref name="y"/>.
        /// </summary>
        /// <typeparam name="T">The numeric type of the array elements, implementing <see cref="INumber{T}"/>.</typeparam>
        /// <param name="x">The input array containing the values for which to compute the asinh.</param>
        /// <param name="y">The output array where the computed asinh values will be stored.</param>
        public static void Asinh<T>(DenseArray<T> x, ref DenseArray<T> y)
            where T : INumber<T>
            => Asinh(x, y);

        #endregion
        #region ---- Atanh [D/Z] ----

        internal static void Atanh<T>(DenseArray<T> x, DenseArray<T> y)
            where T : INumber<T>
        {
            if (typeof(T) == typeof(Real)) factory.iVMF.Atanh(x.Count, x.DPtr, y.DPtr);
            else if (typeof(T) == typeof(Cplx)) factory.iVMF.Atanh(x.Count, x.VPtr, y.VPtr);
            else throw new NotSupportedException($"Type {typeof(T)} not supported.");
        }

        /// <summary>
        /// Computes the atanh of each element in the input array <paramref name="x"/> and stores the result in <paramref name="y"/>.
        /// </summary>
        /// <typeparam name="T">The numeric type of the array elements, implementing <see cref="INumber{T}"/>.</typeparam>
        /// <param name="x">The input array containing the values for which to compute the atanh.</param>
        /// <param name="y">The output array where the computed atanh values will be stored.</param>
        public static void Atanh<T>(DenseArray<T> x, ref DenseArray<T> y)
            where T : INumber<T>
            => Atanh(x, y);

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

        internal static void Mul<T>(DenseArray<T> x, DenseArray<T> y,
            DenseArray<T> z)
            where T : INumber<T>
        {
            if (typeof(T) == typeof(Real)) factory.iVMF.Mul(x.Count, x.DPtr, y.DPtr, z.DPtr);
            else if (typeof(T) == typeof(Cplx)) factory.iVMF.Mul(x.Count, x.VPtr, y.VPtr, z.VPtr);
            else throw new NotSupportedException($"Type {typeof(T)} not supported.");
        }

        /// <summary>
        /// Performs element-wise multiplication of two arrays.
        /// </summary>
        /// <typeparam name="T">
        /// The numeric type of the array elements. Must implement <see cref="INumber{T}"/>.
        /// </typeparam>
        /// <param name="x">The first input array.</param>
        /// <param name="y">The second input array.</param>
        /// <param name="z">The result array, which will contain the product of <paramref name="x"/> and <paramref name="y"/>.</param>
        public static void Mul<T>(DenseArray<T> x, DenseArray<T> y, ref DenseArray<T> z)
            where T : INumber<T>
            => Mul(x, y, z);

        #endregion
        #region ---- Div [D/Z] ----

        internal static void Div<T>(DenseArray<T> x, DenseArray<T> y,
            DenseArray<T> z)
            where T : INumber<T>
        {
            if (typeof(T) == typeof(Real)) factory.iVMF.Div(x.Count, x.DPtr, y.DPtr, z.DPtr);
            else if (typeof(T) == typeof(Cplx)) factory.iVMF.Div(x.Count, x.VPtr, y.VPtr, z.VPtr);
            else throw new NotSupportedException($"Type {typeof(T)} not supported.");
        }

        /// <summary>
        /// Performs element-wise division of two arrays.
        /// Each element of <paramref name="x"/> is divided by the corresponding element of <paramref name="y"/>,
        /// and the result is stored in <paramref name="z"/>.
        /// </summary>
        /// <typeparam name="T">
        /// The numeric type of the array elements. Must implement <see cref="INumber{T}"/>.
        /// </typeparam>
        /// <param name="x">The dividend array.</param>
        /// <param name="y">The divisor array.</param>
        /// <param name="z">The result array to store the division output.</param>
        public static void Div<T>(DenseArray<T> x, DenseArray<T> y, ref DenseArray<T> z)
            where T : INumber<T>
            => Div(x, y, z);

        #endregion
        #region ---- Pow [D/Z] ----

        internal static void Pow<T>(DenseArray<T> x, DenseArray<T> y,
            DenseArray<T> z)
            where T : INumber<T>
        {
            if (typeof(T) == typeof(Real)) factory.iVMF.Pow(x.Count, x.DPtr, y.DPtr, z.DPtr);
            else if (typeof(T) == typeof(Cplx)) factory.iVMF.Pow(x.Count, x.VPtr, y.VPtr, z.VPtr);
            else throw new NotSupportedException($"Type {typeof(T)} not supported.");
        }

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

        #endregion
        #region ---- Powx [D/Z] ----

        internal static void Powx<T>(DenseArray<T> x, T s,
            DenseArray<T> y)
            where T : INumber<T>
        {
            if (typeof(T) == typeof(Real)) factory.iVMF.Powx(x.Count, x.DPtr, Convert.ToDouble(s), y.DPtr);
            else if (typeof(T) == typeof(Cplx)) factory.iVMF.Powx(x.Count, x.VPtr, &s, y.VPtr);
            else throw new NotSupportedException($"Type {typeof(T)} not supported.");
        }

        /// <summary>
        /// Raises each element of the array <paramref name="x"/> to the scalar power <paramref name="s"/> and stores the result in <paramref name="y"/>.
        /// </summary>
        /// <typeparam name="T">Numeric type.</typeparam>
        /// <param name="x">Input dense array.</param>
        /// <param name="s">Scalar exponent.</param>
        /// <param name="y">Result dense array.</param>
        public static void Powx<T>(DenseArray<T> x, T s, ref DenseArray<T> y)
            where T : INumber<T>
            => Powx(x, s, y);

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

        #endregion
        // ...
        #region --------- PackI (...) ---------
        // ...
        #endregion
        #region --------- UnpackI (...) ---------
        // ...
        #endregion

        #endregion
        #region Extensions

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

        #endregion


        #region Operators

        internal class Operators
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
            #region ---- Array - Scalar [T] ----

            internal static Vect<T> Sub<T>(Vect<T> x, T s)
                where T : INumber<T>
                => Add(x, -s);
            internal static Matx<T> Sub<T>(Matx<T> x, T s)
                where T : INumber<T>
                => Add(x, -s);

            #endregion
            #region ---- Array * Scalar [T] ----

            internal static Vect<T> Mul<T>(T s, Vect<T> x)
                where T : INumber<T>
            {
                Vect<T> y = new(count: x.Count, initMode: ArrayInitMode.Malloc);
                VMath.Copy(x, y);
                VMath.Scal(a: s, y);
                return y;
            }
            internal static Matx<T> Mul<T>(T s, Matx<T> x)
                where T : INumber<T>
            {
                Matx<T> y = new(rows: x.Rows, cols: x.Cols, initMode: ArrayInitMode.Malloc);
                VMath.Copy(x, y);
                VMath.Scal(a: s, y);
                return y;
            }

            #endregion
            #region ---- Array / Scalar [T] ----

            internal static Vect<T> Div<T>(Vect<T> x, T s)
                where T : INumber<T>
            {
                Vect<T> y = new(count: x.Count, initMode: ArrayInitMode.Malloc);
                VMath.Copy(x, y);
                VMath.Scal(a: T.One / s, y);
                return y;
            }
            internal static Matx<T> Div<T>(Matx<T> x, T s)
                where T : INumber<T>
            {
                Matx<T> y = new(rows: x.Rows, cols: x.Cols, initMode: ArrayInitMode.Malloc);
                VMath.Copy(x, y);
                VMath.Scal(a: T.One / s, y);
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
                VMath.Scal(a: s, y);
                return y;
            }
            internal static Matx<T> Div<T>(T s, Matx<T> x)
                where T : INumber<T>
            {
                Matx<T> y = new(rows: x.Rows, cols: x.Cols, initMode: ArrayInitMode.Malloc);
                VMath.Copy(x, y);
                VMath.Inv(y, y);
                VMath.Scal(a: s, y);
                return y;
            }

            #endregion

            #region ---- (- Array) [T] ----

            internal static Vect<T> Neg<T>(Vect<T> x)
                where T : INumber<T>
            {
                Vect<T> y = new(count: x.Count, initMode: ArrayInitMode.Malloc);
                VMath.Copy(x, y);
                VMath.Scal(a: -T.One, y);
                return y;
            }
            internal static Matx<T> Neg<T>(Matx<T> x)
                where T : INumber<T>
            {
                Matx<T> y = new(rows: x.Rows, cols: x.Cols, initMode: ArrayInitMode.Malloc);
                VMath.Copy(x, y);
                VMath.Scal(a: -T.One, y);
                return y;
            }

            #endregion
            #region ---- ToCplx [T] ----

            internal static Vect<Cplx> ToCplx(Vect<Real> x)
                => VMath.ToCplx(x);
            internal static Matx<Cplx> ToCplx(Matx<Real> x)
                => VMath.ToCplx(x);

            #endregion
        }


        #endregion
    }




}
