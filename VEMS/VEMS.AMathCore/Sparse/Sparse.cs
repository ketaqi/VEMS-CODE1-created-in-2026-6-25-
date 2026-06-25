using System.Numerics;

namespace VEMS.AMathCore
{

    /// <summary>
    /// Factory holder for sparse BLAS implementation binding.
    /// Provides an <see cref="ISPBLAS"/> instance used by the <see cref="Sparse"/> helper methods.
    /// </summary>
    /// <summary>
    /// Factory holder for sparse BLAS implementation binding.
    /// Provides an <see cref="ISPBLAS"/> instance used by the <see cref="Sparse"/> helper methods.
    /// </summary>
    internal class SparseFactory
    {
        /// <summary>
        /// Gets or sets the current sparse BLAS implementation.
        /// This is typically initialized from <see cref="Defaults.ISPBLAS"/>.
        /// </summary>
        internal ISPBLAS iSPBLAS { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SparseFactory"/> class
        /// and sets the <see cref="iSPBLAS"/> property to the default implementation.
        /// </summary>
        internal SparseFactory()
        {
            iSPBLAS = Defaults.ISPBLAS;
        }

    }

    /// <summary>
    /// Static helper class exposing sparse BLAS (SPBLAS) level routines and sparse matrix utilities.
    /// Methods dispatch to the platform-specific <see cref="ISPBLAS"/> implementation held in <c>factory</c>.
    /// Generic overloads support both real (<see cref="Real"/>) and complex (<see cref="Cplx"/>) numeric types.
    /// </summary>
    /// <summary>
    /// Static helper class exposing sparse BLAS (SPBLAS) level routines and sparse matrix utilities.
    /// Methods dispatch to the platform-specific <see cref="ISPBLAS"/> implementation held in <c>factory</c>.
    /// Generic overloads support both real (<see cref="Real"/>) and complex (<see cref="Cplx"/>) numeric types.
    /// </summary>
    public unsafe class Sparse
    {
        private static SparseFactory factory = new();

        #region Sparse BLAS Level-1

        #region ---- ASum [T] ----

        /// <summary>
        /// Computes the sum of the absolute values of all non-zero elements in a sparse real or complex vector.
        /// For real vectors computes dasum := |x[0]| + |x[1]| + ... + |x[nz-1]|.
        /// For complex vectors computes the sum of absolute values of the complex entries.
        /// </summary>
        /// <typeparam name="T">Numeric element type: <see cref="Real"/> or <see cref="Cplx"/>.</typeparam>
        /// <param name="x">The sparse vector whose non-zero elements are considered.</param>
        /// <param name="incx">The increment for indexing the vector (default is 1).</param>
        /// <returns>The sum of absolute values of the non-zero elements.</returns>
        public static Real ASum<T>(SPVect<T> x,
            Int incx = 1)
            where T : INumber<T>
        {
            if (typeof(T) == typeof(Real))
                return factory.iSPBLAS.Asum(x.NzCount, x.NzValues.DPtr, incx);
            else if (typeof(T) == typeof(Cplx))
                return factory.iSPBLAS.Asum(x.NzCount, x.NzValues.VPtr, incx);
            else throw new NotSupportedException($"Type {typeof(T)} is not supported.");
        }

        #endregion
        #region ---- Copy [T] ----

        /// <summary>
        /// Copies non-zero elements from sparse vector <paramref name="x"/> to sparse vector <paramref name="y"/>.
        /// This internal variant dispatches to the underlying SPBLAS copy routine for the appropriate numeric type.
        /// </summary>
        /// <typeparam name="T">Numeric element type: <see cref="Real"/> or <see cref="Cplx"/>.</typeparam>
        /// <param name="x">Source sparse vector.</param>
        /// <param name="y">Destination sparse vector.</param>
        /// <param name="incx">Increment for the source vector (default is 1).</param>
        /// <param name="incy">Increment for the destination vector (default is 1).</param>
        internal static void Copy<T>(SPVect<T> x, SPVect<T> y,
            Int incx = 1, Int incy = 1)
            where T : INumber<T>
        {
            if (typeof(T) == typeof(Real)) 
                factory.iSPBLAS.Copy(x.NzCount, x.NzValues.DPtr, 
                    y.NzValues.DPtr, incx, incy);
            else if (typeof(T) == typeof(Cplx))
                factory.iSPBLAS.Copy(x.NzCount, x.NzValues.VPtr, 
                    y.NzValues.VPtr, incx, incy);
            else throw new NotSupportedException($"Type {typeof(T)} is not supported.");
        }

        /// <summary>
        /// Copies the non-zero elements from the source sparse vector <paramref name="x"/> 
        /// to the destination sparse vector <paramref name="y"/>.
        /// This public overload accepts the destination by reference.
        /// </summary>
        /// <typeparam name="T">Numeric element type: <see cref="Real"/> or <see cref="Cplx"/>.</typeparam>
        /// <typeparam name="T">Numeric element type: <see cref="Real"/> or <see cref="Cplx"/>.</typeparam>
        /// <param name="x">The source sparse vector.</param>
        /// <param name="y">The destination sparse vector to copy values into (passed by reference).</param>
        /// <param name="incx">The increment for indexing the source vector (default is 1).</param>
        /// <param name="incy">The increment for indexing the destination vector (default is 1).</param>
        public static void Copy<T>(SPVect<T> x, ref SPVect<T> y,
            Int incx = 1, Int incy = 1)
            where T : INumber<T>
            => Copy(x, ref y, incx, incy);

        #endregion
        #region ---- Norm [T] ----

        /// <summary>
        /// Computes the Euclidean norm (L2 norm) of a sparse vector.
        /// </summary>
        /// <typeparam name="T">Numeric element type: <see cref="Real"/> or <see cref="Cplx"/>.</typeparam>
        /// <param name="x">The sparse vector.</param>
        /// <param name="incx">The increment for indexing the vector (default is 1).</param>
        /// <returns>The Euclidean norm of the non-zero elements of the sparse vector.</returns>
        public static Real Norm<T>(SPVect<T> x, 
            Int incx = 1)
            where T : INumber<T>
        {
            if (typeof(T) == typeof(Real))
                return factory.iSPBLAS.Nrm2(x.NzCount, x.NzValues.DPtr, incx);
            else if (typeof(T) == typeof(Cplx))
                return factory.iSPBLAS.Nrm2(x.NzCount, x.NzIndices.VPtr, incx);
            else throw new NotSupportedException($"Type {typeof(T)} is not supported.");
        }

        #endregion
        #region ---- Scal [T] ----

        /// <summary>
        /// Scales the non-zero elements of a sparse vector by the scalar <paramref name="a"/>.
        /// This internal generic variant dispatches to the appropriate SPBLAS routine for real or complex types.
        /// </summary>
        /// <typeparam name="T">Numeric element type: <see cref="Real"/> or <see cref="Cplx"/>.</typeparam>
        /// <param name="a">The scalar multiplier.</param>
        /// <param name="x">The sparse vector whose non-zero elements will be scaled.</param>
        /// <param name="incx">The increment for indexing the vector (default is 1).</param>
        internal static void Scal<T>(T a, SPVect<T> x,
            Int incx = 1)
            where T : INumber<T>
        {
            if (typeof(T) == typeof(Real))
                factory.iSPBLAS.Scal(x.NzCount, Convert.ToDouble(a), x.NzValues.DPtr, incx);
            else if (typeof(T) == typeof(Cplx))
                factory.iSPBLAS.Scal(x.NzCount, &a, x.NzValues.VPtr, incx);
            else throw new NotSupportedException($"Type {typeof(T)} not supported.");
        }

        /// <summary>
        /// Scales the non-zero elements of a sparse vector by a scalar value.
        /// </summary>
        /// <typeparam name="T">Numeric element type: <see cref="Real"/> or <see cref="Cplx"/>.</typeparam>
        /// <param name="a">The scalar multiplier.</param>
        /// <param name="x">The sparse vector to be scaled (passed by reference).</param>
        /// <param name="x">The sparse vector to be scaled (passed by reference).</param>
        /// <param name="incx">The increment for indexing the vector (default is 1).</param>
        public static void Scal<T>(T a, ref SPVect<T> x,
            Int incx = 1)
            where T : INumber<T>
            => Scal(a, x, incx);

        #endregion
        #region ---- Scal [Z] ----

        internal static void Scal(Real a, SPVect<Cplx> x,
            Int incx = 1)
            => factory.iSPBLAS.Scal(x.NzCount, a, x.NzValues.VPtr, incx);

        /// <summary>
        /// Scales the non-zero elements of a sparse complex vector by a real scalar value.
        /// </summary>
        /// <param name="x">The sparse vector containing complex values to be scaled.</param>
        /// <param name="a">The real scalar multiplier.</param>
        /// <param name="incx">The increment for indexing the vector (default is 1).</param>
        public static void Scal(Real a, ref SPVect<Cplx> x,
            Int incx = 1)
            => Scal(a, x, incx);

        #endregion
        #region ---- IAmx [T] ----

        /// <summary>
        /// Finds the index of the element with the largest absolute value in a sparse vector.
        /// </summary>
        /// <param name="x">The sparse vector.</param>
        /// <param name="incx">The increment for indexing the vector (default is 1).</param>
        /// <returns>The index of the element with the largest absolute value among the non-zero elements.</returns>
        public static Int IAmx<T>(SPVect<T> x,
            Int incx = 1)
            where T : INumber<T>
        {
            if (typeof(T) == typeof(Real))
                return factory.iSPBLAS.Iamax(x.NzCount, x.NzValues.DPtr, incx);
            else if (typeof(T) == typeof(Cplx))
                return factory.iSPBLAS.Iamax(x.NzCount, x.NzValues.VPtr, incx);
            else throw new NotSupportedException($"Type {typeof(T)} not supported.");
        }

        #endregion
        #region ---- IAmn [T] ----

        /// <summary>
        /// Finds the index of the element with the smallest absolute value in a sparse vector.
        /// </summary>
        /// <param name="x">The sparse vector.</param>
        /// <param name="incx">The increment for indexing the vector (default is 1).</param>
        /// <returns>The index of the element with the smallest absolute value among the non-zero elements.</returns>
        public static Int IAmn<T>(SPVect<T> x, 
            Int incx = 1)
            where T : INumber<T>
        {
            if (typeof(T) == typeof(Real)) 
                return factory.iSPBLAS.Iamin(x.NzCount, x.NzValues.DPtr, incx);
            else if (typeof(T) == typeof(Cplx)) 
                return factory.iSPBLAS.Iamin(x.NzCount, x.NzValues.VPtr, incx);
            else throw new NotSupportedException($"Type {typeof(T)} not supported.");
        }

        #endregion

        #region ---- Add [T] ----

        internal static void Add<T>(SPVect<T> x, DenseArray<T> y,
            T a = default!)
            where T : INumber<T>
        {
            if (a == default) { a = T.CreateChecked(1.0); }

            if (typeof(T) == typeof(Real))
                factory.iSPBLAS.Axpy(x.NzCount, Convert.ToDouble(a),
                    x.NzValues.DPtr, (long*)x.NzIndices.DataPtr, y.DPtr);
            else if (typeof(T) == typeof(Cplx))
                factory.iSPBLAS.Axpy(x.NzCount, &a,
                    x.NzValues.VPtr, (long*)x.NzIndices.DataPtr, y.VPtr);
            else throw new NotSupportedException($"Type {typeof(T)} not supported.");
        }

        /// <summary>
        /// Adds a scalar multiple of a compressed sparse vector <paramref name="x"/> to a dense vector <paramref name="y"/>.
        /// Computes y := a * x + y, where only the non-zero elements of <paramref name="x"/> are used.
        /// </summary>
        /// <param name="x">The input sparse vector.</param>
        /// <param name="y">The dense vector to which the scaled sparse vector will be added (passed by reference).</param>
        /// <param name="a">The scalar multiplier (default is 1.0).</param>
        public static void Add<T>(SPVect<T> x, ref DenseArray<T> y,
            T a = default!)
            where T : INumber<T>
            => Add(x, y, a);

        #endregion
        #region ---- Dot [T] ----

        /// <summary>
        /// Computes the dot product of a sparse vector and a dense vector.
        /// The result is the sum of the products of the non-zero elements of <paramref name="x"/>
        /// and the corresponding elements in <paramref name="y"/> at the indices specified by <paramref name="x"/>.
        /// res = x[0]*y[indx[0]] + x[1]*y[indx[1]] + ... + x[nz-1]*y[indx[nz-1]]
        /// </summary>
        /// <param name="x">The sparse vector.</param>
        /// <param name="y">The dense vector.</param>
        /// <returns>The dot product of the sparse and dense vectors.</returns>
        public static T Dot<T>(SPVect<T> x, Vect<T> y)
            where T : INumber<T>
        {
            if (typeof(T) == typeof(Real))
            {
                Real dot = factory.iSPBLAS.Dot(x.NzCount,
                    x.NzValues.DPtr, (long*)x.NzIndices.DataPtr, y.DPtr);
                return (T)Convert.ChangeType(dot, typeof(T));
            }
            else if (typeof(T) == typeof(Cplx))
            {
                Cplx dotu;
                factory.iSPBLAS.Dot(x.NzCount, x.NzValues.VPtr,
                    (long*)x.NzIndices.DataPtr, y.VPtr, &dotu);
                return (T)Convert.ChangeType(dotu, typeof(T));
            }
            else throw new NotSupportedException($"Type {typeof(T)} not supported.");
        }

        #endregion
        #region ---- Dotc [Z] ----

        /// <summary>
        /// Computes the dot product of a conjugated sparse complex vector <paramref name="x"/> 
        /// and a dense complex vector <paramref name="y"/>.
        /// <para>
        /// The result is the sum of the products of the conjugate of each non-zero element of <paramref name="x"/>
        /// and the corresponding element in <paramref name="y"/> at the indices specified by <paramref name="x"/>.
        /// </para>
        /// <para>
        /// res = conj(x[0])*y[indx[0]] + conj(x[1])*y[indx[1]] + ... + conj(x[nz-1])*y[indx[nz-1]]
        /// </para>
        /// </summary>
        /// <param name="x">The sparse vector containing complex values (to be conjugated).</param>
        /// <param name="y">The dense vector containing complex values.</param>
        /// <returns>The dot product of the conjugated sparse vector and the dense vector.</returns>
        public static Cplx Dotc(SPVect<Cplx> x, Vect<Cplx> y)
        {
            Cplx dotc;
            factory.iSPBLAS.Dotc(x.NzCount,
                x.NzValues.VPtr, (long*)x.NzIndices.DataPtr, y.VPtr, &dotc);
            return dotc;
        }

        #endregion
        #region ---- Rot [D] ----

        internal static void Rot(SPVect<Real> x, DenseArray<Real> y,
            Real c, Real s)
            => factory.iSPBLAS.Rot(x.NzCount, x.NzValues.DPtr, 
                x.NzIndices.TPtr, y.DPtr, c, s);

        /// <summary>
        /// Performs a Givens rotation of points in the plane for a sparse real vector <paramref name="x"/> and a dense real vector <paramref name="y"/>.
        /// For each non-zero element in <paramref name="x"/>, updates both <paramref name="x"/> and <paramref name="y"/> as follows:
        /// <code>
        /// x[i] = c * x[i] + s * y[indx[i]]
        /// y[indx[i]] = c * y[indx[i]] - s * x[i]
        /// </code>
        /// </summary>
        /// <param name="x">The sparse vector containing real values to be rotated (passed by reference).</param>
        /// <param name="y">The dense vector to be rotated (passed by reference).</param>
        /// <param name="c">The cosine component of the rotation.</param>
        /// <param name="s">The sine component of the rotation.</param>
        public static void Rot(ref SPVect<Real> x, ref DenseArray<Real> y,
            Real c, Real s)
            => Rot(x, y, c, s);

        #endregion

        #region ---- Gthr [D/Z] ----

        internal static void Gthr<T>(DenseArray<T> y, SPVect<T> x)
            where T : INumber<T>
        {
            if (typeof(T) == typeof(Real))
                factory.iSPBLAS.Gthr(x.NzCount, y.DPtr,
                    x.NzValues.DPtr, x.NzIndices.TPtr);
            else if (typeof(T) == typeof(Cplx))
                factory.iSPBLAS.Gthr(x.NzCount, y.VPtr,
                    x.NzValues.VPtr, x.NzIndices.TPtr);
            else throw new NotSupportedException($"Type {typeof(T)} not supported.");
        }

        /// <summary>
        /// Gathers elements from a dense vector <paramref name="y"/> into a sparse vector <paramref name="x"/>,
        /// using the indices specified by <paramref name="x.NzIndices"/>.
        /// For each i in [0, x.NzCount), sets x.NzValues[i] = y[x.NzIndices[i]].
        /// </summary>
        /// <param name="y">The dense array to gather values from.</param>
        /// <param name="x">The sparse vector to fill with gathered values (passed by reference).</param>
        public static void Gthr<T>(DenseArray<T> y, ref SPVect<T> x)
            where T : INumber<T>
            => Gthr(y, x);

        #endregion
        #region ---- Gthrz [D/Z] ----

        internal static void Gthrz<T>(DenseArray<T> y, SPVect<T> x)
            where T : INumber<T>
        {
            if (typeof(T) == typeof(Real))
                factory.iSPBLAS.Gthrz(x.NzCount, y.DPtr,
                    x.NzValues.DPtr, x.NzIndices.TPtr);
            else if (typeof(T) == typeof(Cplx))
                factory.iSPBLAS.Gthrz(x.NzCount, y.VPtr,
                    x.NzValues.VPtr, x.NzIndices.TPtr);
            else throw new NotSupportedException($"Type {typeof(T)} not supported.");
        }

        /// <summary>
        /// Gathers a full-storage array's elements into compressed sparse form, replacing them by zeros in the source vector.
        /// For each i in [0, x.NzCount), sets x.NzValues[i] = y[x.NzIndices[i]] and then y[x.NzIndices[i]] = 0.
        /// </summary>
        /// <param name="y">The dense array to gather values from and zero out at gathered indices.</param>
        /// <param name="x">The sparse vector to fill with gathered values (passed by reference).</param>
        public static void Gthrz<T>(DenseArray<T> y, ref SPVect<T> x)
            where T : INumber<T>
            => Gthrz(y, x);

        #endregion
        #region ---- Sctr [D/Z] ----

        ///// <summary>
        ///// Converts a compressed sparse real vector <paramref name="x"/> into a full-storage dense real vector <paramref name="y"/>.
        ///// For each non-zero element in <paramref name="x"/>, sets <c>y[x.NzIndices[i]] = x.NzValues[i]</c>.
        ///// </summary>
        ///// <param name="x">The sparse vector containing real values and their indices.</param>
        ///// <param name="y">The dense vector to be updated with the non-zero values from <paramref name="x"/> (passed by reference).</param>
        //public static void Sctr(SPVect<Real> x, ref Vect<Real> y)
        //    => factory.iSPBLAS.Sctr(x.NzCount, x.NzValues.DPtr,
        //        (long*)x.NzIndices.DataPtr, y.DPtr);

        ///// <summary>
        ///// Converts a compressed sparse complex vector <paramref name="x"/> into a full-storage dense complex vector <paramref name="y"/>.
        ///// For each non-zero element in <paramref name="x"/>, sets <c>y[x.NzIndices[i]] = x.NzValues[i]</c>.
        ///// </summary>
        ///// <param name="x">The sparse vector containing complex values and their indices.</param>
        ///// <param name="y">The dense vector to be updated with the non-zero values from <paramref name="x"/> (passed by reference).</param>
        //public static void Sctr(SPVect<Cplx> x, ref Vect<Cplx> y)
        //    => factory.iSPBLAS.Sctr(x.NzCount, x.NzValues.VPtr,
        //        (long*)x.NzIndices.DataPtr, y.VPtr);

        #endregion

        #endregion
        #region Sparse BLAS Level-2 [DEPRACATED]

        // MKL_DEPRECATED ...

        #endregion
        #region Sparse BLAS Level-3 [DEPRACATED]

        // MKL_DEPRECATED ...

        #endregion
        #region Sparse QR routines

        // todo ...
        public static void QR_Reorder<T>(SPMatx<T> a)
            where T : INumber<T>
        {
            // consistency check ...

            SPARSE_Status status = SPARSE_Status.Success;
            if (typeof(T) == typeof(Real))
            {
                status = factory.iSPBLAS.QR_Reorder(a.Handle, a.MatrixDescr);
                if (status != SPARSE_Status.Success) throw new InvalidOperationException($"QR-Reorder failed with status: {status}");
            }
            else if (typeof(T) == typeof(Cplx))
            {
                status = factory.iSPBLAS.QR_Reorder(a.Handle, a.MatrixDescr);
                if (status != SPARSE_Status.Success) throw new InvalidOperationException($"QR-Reorder failed with status: {status}");
            }
            else throw new NotSupportedException($"Type {typeof(T)} not supported.");
        }


        public static void LeastSquare<T>(SPMatx<T> a, Vect<T> b,
            ref Vect<T> x,
            SPARSE_Operation operation = SPARSE_Operation.NonTranspose)
            where T : INumber<T>
        {
            // consistency check ...

            SPARSE_Status status = SPARSE_Status.Success;
            if (typeof(T) == typeof(Real))
            {
                status = factory.iSPBLAS.QR(operation, 
                    a.Handle, a.MatrixDescr, 
                    SPARSE_Layout.RowMajor, 1, x.DPtr, 1, b.DPtr, 1);
                if (status != SPARSE_Status.Success) throw new InvalidOperationException($"Sparse QR failed with status: {status}");
            }
            else if (typeof(T) == typeof(Cplx))
            {
                // ...
                status = factory.iSPBLAS.QR(operation,
                    a.Handle, a.MatrixDescr,
                    SPARSE_Layout.RowMajor, 1, x.DPtr, 1, b.DPtr, 1);
            }
            else throw new NotSupportedException($"Type {typeof(T)} not supported.");
        }


        //public unsafe static VectorD LeastSquare(WMatrixDi a, VectorD b,
        //    int operation = 10, int matrixType = 20, int fillMode = 42, int diagType = 50)
        //{
        //    VectorD x = new(a.Cols, 0.0);
        //    WLinAlg.SparseQR(a, operation, matrixType, fillMode, diagType,
        //        101, 1, x.SPtr, 1, b.SPtr, 1);
        //    return x;
        //}



        #endregion
        #region Sparse BLAS inspector-executer ...

        // ---- CreateCOO ----
        // ---- CreateCSR ----
        // ---- CreateCSC ----
        #region ---- Copy ----

        /// <summary>
        /// Copies the contents of a sparse matrix to another sparse matrix.
        /// </summary>
        /// <typeparam name="T">The numeric type of the matrix elements. Must implement <see cref="INumber{T}"/>.</typeparam>
        /// <param name="source">The source sparse matrix to copy from.</param>
        /// <param name="descr">The descriptor specifying the properties and structure of the sparse matrix.</param>
        /// <param name="dest">A reference to the destination sparse matrix that will receive the copied data.</param>
        /// <exception cref="NotSupportedException">Thrown if <typeparamref name="T"/> is not a supported numeric type for sparse matrix operations.</exception>
        public static void Copy<T>(SPMatx<T> source, 
            SPARSE_MatrixDescr descr,
            ref SPMatx<T> dest)
            where T : INumber<T>
        {
            SPARSE_Status status = SPARSE_Status.Success;
            if (typeof(T) == typeof(Real) || typeof(T) == typeof(Cplx))
            {
                IntPtr src = source.Handle;
                IntPtr dst = dest.Handle;
                status = factory.iSPBLAS.Copy(src, descr, ref dst);
                if (status != SPARSE_Status.Success) throw new InvalidOperationException($"Sparse matrix copy failed with status: {status}");
                dest.Handle = dst;
                dest.MatrixDescr = descr;
            }
            else throw new NotSupportedException($"Type {typeof(T)} not supported.");
        }

        #endregion
        // ---- Destroy ----
        // ---- Convert ----
        // ---- ExportCSR ----
        #region ---- SetValue ----

        /// <summary>
        /// Sets a single element in a sparse matrix to the specified value.
        /// </summary>
        /// <typeparam name="T">The numeric element type of the sparse matrix. Expected to be <see cref="Real"/> or <see cref="Cplx"/>.</typeparam>
        /// <param name="a">The sparse matrix in which to set the element.</param>
        /// <param name="row">Zero-based row index of the element to set.</param>
        /// <param name="col">Zero-based column index of the element to set.</param>
        /// <param name="value">The value to assign to the element. If <c>null</c>, a type-appropriate zero is used.</param>
        /// <remarks>
        /// This method dispatches to the underlying sparse BLAS implementation stored in <c>factory.iSPBLAS</c>.
        /// For <see cref="Real"/>, the value is converted to <see cref="double"/> and the real-specific
        /// <c>SetValue</c> overload is invoked. For <see cref="Cplx"/>, the value is converted to <see cref="Cplx"/>
        /// and the complex-specific overload is invoked.
        /// </remarks>
        public static void SetValue<T>(ref SPMatx<T> a,
            Int row, Int col, T value)
            where T : INumber<T>
        {
            SPARSE_Status status = SPARSE_Status.Success;
            if (typeof(T) == typeof(Real))
            {
                Real dValue = (value == null) ? 0.0 : Convert.ToDouble(value);
                status = factory.iSPBLAS.SetValue(a.Handle, row, col, dValue);
                if (status != SPARSE_Status.Success) throw new InvalidOperationException($"Sparse matrix SetValue failed with status: {status}");
            }
            else if (typeof(T) == typeof(Cplx))
            {
                Cplx zValue = (value == null) ? Cplx.Zero : (Cplx)Convert.ChangeType(value, typeof(Cplx));
                status = factory.iSPBLAS.SetValue(a.Handle, row, col, zValue);
                if (status != SPARSE_Status.Success) throw new InvalidOperationException($"Sparse matrix SetValue failed with status: {status}");
            }
            else throw new NotSupportedException($"Type {typeof(T)} not supported.");
        }

        #endregion
        #region ---- UpdateValues ----

        /// <summary>
        /// Updates multiple entries of the sparse matrix <paramref name="a"/> using the provided index and value arrays.
        /// </summary>
        /// <typeparam name="T">The numeric element type of the matrix. Supported types are <see cref="Real"/> and <see cref="Cplx"/>.</typeparam>
        /// <param name="a">A reference to the sparse matrix whose values will be updated. The matrix handle is used by the underlying sparse BLAS implementation.</param>
        /// <param name="indx">A dense array of row indices for the entries to update. The number of indices determines the number of updates performed.</param>
        /// <param name="indy">A dense array of column indices for the entries to update. Must have the same length as <paramref name="indx"/> and <paramref name="values"/>.</param>
        /// <param name="values">A dense array containing the new values to write into the matrix at positions specified by <paramref name="indx"/> and <paramref name="indy"/>.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the underlying sparse BLAS implementation returns a non-success <see cref="SPARSE_Status"/> value.
        /// The exception message contains the returned status for diagnostics.
        /// </exception>
        /// <exception cref="NotSupportedException">Thrown when the generic type <typeparamref name="T"/> is not supported by the sparse BLAS routines.</exception>
        /// <remarks>
        /// - The three arrays <paramref name="indx"/>, <paramref name="indy"/> and <paramref name="values"/> are expected to have the same <see cref="DenseArray{T}.Count"/>.
        /// - Indices are interpreted according to the conventions of the underlying sparse BLAS implementation (typically zero-based).
        /// - This method dispatches to <c>factory.iSPBLAS.UpdateValues</c> and performs a runtime type-dispatch for <see cref="Real"/> and <see cref="Cplx"/>.
        /// - No structural reallocation of <paramref name="a"/> is performed here; the call updates existing entries of the matrix handle contained in <paramref name="a"/>.
        /// </remarks>
        public static void UpdateValues<T>(ref SPMatx<T> a,
            DenseArray<Int> indx, DenseArray<Int> indy,
            DenseArray<T> values)
            where T : INumber<T>
        {
            // consistency check ...

            Int nvalues = indx.Count;
            SPARSE_Status status = SPARSE_Status.Success;
            if (typeof(T) == typeof(Real))
            {
                status = factory.iSPBLAS.UpdateValues(a.Handle,
                    nvalues, indx.TPtr, indy.TPtr, values.DPtr);
                if (status != SPARSE_Status.Success) throw new InvalidOperationException($"Sparse matrix UpdateValues failed with status: {status}");
            }
            else if (typeof(T) == typeof(Cplx))
            {
                status = factory.iSPBLAS.UpdateValues(a.Handle,
                    nvalues, indx.TPtr, indy.TPtr, values.VPtr);
                if (status != SPARSE_Status.Success) throw new InvalidOperationException($"Sparse matrix UpdateValues failed with status: {status}");
            }
            else throw new NotSupportedException($"Type {typeof(T)} not supported.");
        }

        #endregion
        #region ---- Order ----

        /// <summary>
        /// Requests the sparse BLAS implementation to compute an ordering for the sparse matrix represented by <paramref name="a"/>.
        /// </summary>
        /// <typeparam name="T">
        /// The numeric element type of the sparse matrix. Supported types are <see cref="Real"/> and <see cref="Cplx"/>.
        /// The generic constraint <c>where T : INumber&lt;T&gt;</c> ensures numeric behavior at compile time, but
        /// at runtime only <see cref="Real"/> and <see cref="Cplx"/> are dispatched to the native SPBLAS routines.
        /// </typeparam>
        /// <param name="a">
        /// A reference to the <see cref="SPMatx{T}"/> whose underlying sparse handle will be ordered.
        /// The method uses <see cref="SPMatx{T}.Handle"/> to call the platform-specific ordering routine.
        /// After successful completion the underlying handle's internal ordering state may have changed.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the underlying sparse BLAS implementation returns a non-success <see cref="SPARSE_Status"/> value.
        /// The exception message contains the returned status for diagnostics.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// Thrown when the generic type <typeparamref name="T"/> is not supported by the sparse BLAS routines
        /// (only <see cref="Real"/> and <see cref="Cplx"/> are supported at runtime).
        /// </exception>
        /// <remarks>
        /// - This method dispatches to <c>factory.iSPBLAS.Order</c> and performs a runtime type-dispatch for <see cref="Real"/> and <see cref="Cplx"/>.
        /// - The call is intended to perform symbolic ordering / reordering that can improve the efficiency of
        ///   later numeric factorizations and solves. The exact effect and algorithm depend on the underlying
        ///   SPBLAS implementation bound to <c>factory.iSPBLAS</c>.
        /// - The method operates on the native handle contained in <paramref name="a"/>; callers should ensure that
        ///   no concurrent operations that mutate or destroy the same handle run concurrently, unless the underlying
        ///   implementation documents thread-safety.
        /// - This method does not allocate or return a new managed matrix object; it may mutate internal native state
        ///   associated with <paramref name="a"/>. After calling this method you may wish to inspect or export the
        ///   matrix to observe reordering effects.
        /// </remarks>
        public static void Order<T>(ref SPMatx<T> a)
            where T : INumber<T>
        {
            SPARSE_Status status = SPARSE_Status.Success;
            if (typeof(T) == typeof(Real) || typeof(T) == typeof(Cplx))
            {
                status = factory.iSPBLAS.Order(a.Handle);
                if (status != SPARSE_Status.Success) throw new InvalidOperationException($"Sparse matrix ordering failed with status: {status}");
            }
            else throw new NotSupportedException($"Type {typeof(T)} not supported.");
        }

        #endregion
        #region ---- Optimize ----

        /// <summary>
        /// Requests the underlying sparse BLAS implementation to optimize internal data structures
        /// associated with the provided sparse matrix handle.
        /// </summary>
        /// <typeparam name="T">
        /// The numeric element type of the sparse matrix. At runtime only <see cref="Real"/> and
        /// <see cref="Cplx"/> are dispatched to the native sparse BLAS <c>Optimize</c> routine.
        /// </typeparam>
        /// <param name="a">
        /// A reference to the <see cref="SPMatx{T}"/> whose native handle will be optimized.
        /// The call may mutate internal native state associated with the handle.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the underlying sparse BLAS implementation returns a non-success
        /// <see cref="SPARSE_Status"/> value. The exception message contains the returned status.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// Thrown when <typeparamref name="T"/> is not a supported numeric type for sparse BLAS operations.
        /// </exception>
        /// <remarks>
        /// - This method dispatches to <c>factory.iSPBLAS.Optimize</c>.
        /// - Callers should ensure no concurrent operations that mutate or destroy the same native handle
        ///   run concurrently, unless the underlying implementation documents thread-safety.
        /// - The method does not allocate a new managed matrix object; it operates on the native handle
        ///   contained in <paramref name="a"/> and may adjust internal representation for performance.
        /// </remarks>
        public static void Optimize<T>(ref SPMatx<T> a)
            where T : INumber<T>
        {
            SPARSE_Status status = SPARSE_Status.Success;
            if (typeof(T) == typeof(Real) || typeof(T) == typeof(Cplx))
            {
                status = factory.iSPBLAS.Optimize(a.Handle);
                if (status != SPARSE_Status.Success) throw new InvalidOperationException($"Sparse matrix optimization failed with status: {status}");
            }
            else throw new NotSupportedException($"Type {typeof(T)} not supported.");
        }

        #endregion
        #region ---- Dot (MV) ----

        /// <summary>
        /// Performs a sparse matrix-vector multiplication (SPMV): y := alpha * op(A) * x + beta * y.
        /// Dispatches to the underlying SPBLAS matrix-vector routine and supports real and complex types.
        /// </summary>
        /// <typeparam name="T">Numeric element type: <see cref="Real"/> or <see cref="Cplx"/>.</typeparam>
        /// <param name="a">The sparse matrix operand.</param>
        /// <param name="x">The input dense vector to be multiplied.</param>
        /// <param name="y">The output dense vector which is updated in-place (passed by reference).</param>
        /// <param name="alpha">Scaling factor for the matrix-vector product (defaults to 1.0 when <c>default</c>).</param>
        /// <param name="beta">Scaling factor for the existing contents of <paramref name="y"/> (defaults to 0.0 when <c>default</c>).</param>
        /// <param name="operation">Specifies whether the matrix should be transposed/conjugate-transposed before multiplication.</param>
        public static void Dot<T>(SPMatx<T> a, 
            Vect<T> x, ref Vect<T> y,
            T alpha = default!, T beta = default!,
            SPARSE_Operation operation = SPARSE_Operation.NonTranspose)
            where T : INumber<T>
        {
            // consistency check ...

            SPARSE_Status status = SPARSE_Status.Success;
            if (typeof(T) == typeof(Real))
            {
                Real dAlpha = (alpha == default) ? 1.0 : Convert.ToDouble(alpha);
                Real dBeta = (beta == default) ? 0.0 : Convert.ToDouble(beta);
                status = factory.iSPBLAS.Mv(operation, dAlpha, a.Handle, 
                    a.MatrixDescr, x.DPtr, dBeta, y.DPtr);
                if (status != SPARSE_Status.Success) throw new InvalidOperationException($"Sparse matrix-vector multiplication failed with status: {status}");
            }
            else if (typeof(T) == typeof(Cplx))
            {
                Cplx zAlpha = (alpha == default) ? Cplx.One : (Cplx)Convert.ChangeType(alpha, typeof(Cplx));
                Cplx zBeta = (beta == default) ? Cplx.Zero : (Cplx)Convert.ChangeType(beta, typeof(Cplx));
                status = factory.iSPBLAS.Mv(operation, &zAlpha, a.Handle,
                    a.MatrixDescr, x.VPtr, &zBeta, y.VPtr);
                if (status != SPARSE_Status.Success) throw new InvalidOperationException($"Sparse matrix-vector multiplication failed with status: {status}");
            }
            else throw new NotSupportedException($"Type {typeof(T)} not supported.");
        }


        //internal static T Dot<T>(SPMatx<T> a,
        //    Vect<T> x, ref Vect<T> y, out T d,
        //    T alpha = default!, T beta = default!,
        //    SPARSE_Operation operation = SPARSE_Operation.NonTranspose)
        //    where T : INumber<T>
        //{
        //    // consistency check ...

        //    d = T.Zero;
        //    SPARSE_Status status = SPARSE_Status.Success;
        //    if (typeof(T) == typeof(Real))
        //    {
        //        Real dAlpha = (alpha == default) ? 1.0 : Convert.ToDouble(alpha);
        //        Real dBeta = (beta == default) ? 0.0 : Convert.ToDouble(beta);
        //        Real* pd = (Real*)IntPtr.Zero;
        //        status = factory.iSPBLAS.DotMv(operation, dAlpha, a.Handle,
        //            a.MatrixDescr, x.DPtr, dBeta, y.DPtr, pd);
        //        if (status != SPARSE_Status.Success) throw new InvalidOperationException($"Sparse matrix-vector multiplication failed with status: {status}");
        //        //return *pd;
        //    }
        //    else if (typeof(T) == typeof(Cplx))
        //    {
        //        Cplx zAlpha = (alpha == default) ? Cplx.One : (Cplx)Convert.ChangeType(alpha, typeof(Cplx));
        //        Cplx zBeta = (beta == default) ? Cplx.Zero : (Cplx)Convert.ChangeType(beta, typeof(Cplx));
        //        void* pd = (void*)IntPtr.Zero;
        //        status = factory.iSPBLAS.DotMv(operation, &zAlpha, a.Handle,
        //            a.MatrixDescr, x.VPtr, &zBeta, y.VPtr, pd);
        //        if (status != SPARSE_Status.Success) throw new InvalidOperationException($"Sparse matrix-vector multiplication failed with status: {status}");
        //    }
        //    else throw new NotSupportedException($"Type {typeof(T)} not supported.");

        //}


        #endregion
        // ---- Trsv ----
        // ---- SymgsMv ----
        // ---- LU smoother ----
        #region ---- Dot (MM) ----

        /// <summary>
        /// Performs a sparse matrix–dense matrix multiplication and accumulates the result into a dense matrix.
        /// Computes y := alpha * op(A) * x + beta * y for the sparse matrix <paramref name="a"/> and dense matrix <paramref name="x"/>.
        /// The result is written into <paramref name="y"/> (in-place).
        /// </summary>
        /// <typeparam name="T">
        /// Numeric element type for the operation. At compile-time <c>T</c> must implement <see cref="System.Numerics.INumber{T}"/>,
        /// but at runtime only <see cref="Real"/> and <see cref="Cplx"/> are dispatched to the underlying SPBLAS implementation.
        /// </typeparam>
        /// <param name="a">The sparse matrix operand. Its native handle (<see cref="SPMatx{T}.Handle"/>) and descriptor (<see cref="SPMatx{T}.MatrixDescr"/>) are used by the implementation.</param>
        /// <param name="x">The left-hand dense matrix operand. Data pointer (<c>x.DPtr</c> / <c>x.VPtr</c>) is passed to the SPBLAS routine.</param>
        /// <param name="y">The output dense matrix which is updated in-place with the computed result. The method mutates <paramref name="y"/>.</param>
        /// <param name="alpha">
        /// Scaling factor applied to the matrix product. When <c>alpha</c> equals the default value the implementation uses <c>1.0</c> for real types
        /// and <see cref="Cplx.One"/> for complex types.
        /// </param>
        /// <param name="beta">
        /// Scaling factor applied to the existing contents of <paramref name="y"/> before accumulation. When <c>beta</c> equals the default value
        /// the implementation uses <c>0.0</c> for real types and <see cref="Cplx.Zero"/> for complex types.
        /// </param>
        /// <param name="operation">Specifies whether the sparse matrix <paramref name="a"/> is used as-is, transposed, or conjugate-transposed.</param>
        /// <param name="layout">Specifies the memory layout of the dense matrices (row-major or column-major). Defaults to <see cref="SPARSE_Layout.RowMajor"/>.</param>
        /// <exception cref="System.InvalidOperationException">
        /// Thrown when the underlying sparse BLAS implementation returns a non-success <see cref="SPARSE_Status"/> value.
        /// The exception message contains the returned status for diagnostics.
        /// </exception>
        /// <exception cref="System.NotSupportedException">Thrown when the generic type <c>T</c> is not supported by the sparse BLAS dispatch (only <see cref="Real"/> and <see cref="Cplx"/> at runtime).</exception>
        /// <remarks>
        /// - No structural checks are performed here; callers should ensure that matrix dimensions are compatible for the requested operation.
        /// - The method performs a runtime type-dispatch: for <see cref="Real"/> it converts <c>alpha</c> and <c>beta</c> to <c>double</c>,
        ///   for <see cref="Cplx"/> it converts them to <see cref="Cplx"/> and passes pointers to the native routine.
        /// - The implementation will update <paramref name="y"/>'s data and relies on <see cref="factory.iSPBLAS"/> to perform the computation.
        /// </remarks>
        public static void Dot<T>(SPMatx<T> a,
            Matx<T> x, ref Matx<T> y, 
            T alpha = default!, T beta = default!,
            SPARSE_Operation operation = SPARSE_Operation.NonTranspose,
            SPARSE_Layout layout = SPARSE_Layout.RowMajor)
            where T : INumber<T>
        {
            // consistency check ...

            SPARSE_Status status = SPARSE_Status.Success;
            if (typeof(T) == typeof(Real))
            {
                Real dAlpha = (alpha == default) ? 1.0 : Convert.ToDouble(alpha);
                Real dBeta = (beta == default) ? 0.0 : Convert.ToDouble(beta);
                status = factory.iSPBLAS.Mm(operation, 
                    dAlpha, a.Handle, a.MatrixDescr, 
                    layout, x.DPtr, y.Cols, x.Cols,
                    dBeta, y.DPtr, y.Cols);
                if (status != SPARSE_Status.Success) throw new InvalidOperationException($"Sparse matrix-matrix multiplication failed with status: {status}");
            }
            else if (typeof(T) == typeof(Cplx))
            {
                Cplx zAlpha = (alpha == default) ? Cplx.One : (Cplx)Convert.ChangeType(alpha, typeof(Cplx));
                Cplx zBeta = (beta == default) ? Cplx.Zero : (Cplx)Convert.ChangeType(beta, typeof(Cplx));
                status = factory.iSPBLAS.Mm(operation,
                    &zAlpha, a.Handle, a.MatrixDescr,
                    layout, x.VPtr, y.Cols, x.Cols,
                    &zBeta, y.VPtr,  y.Cols);
                if (status != SPARSE_Status.Success) throw new InvalidOperationException($"Sparse matrix-matrix multiplication failed with status: {status}");
            }
            else throw new NotSupportedException($"Type {typeof(T)} not supported.");
        }

        #endregion
        // ---- Trsm ----
        #region ---- Add ----

        /// <summary>
        /// Computes the sum of two sparse matrices and stores the resulting sparse matrix in <paramref name="c"/>.
        /// </summary>
        /// <typeparam name="T">
        /// Numeric element type of the matrices. At compile time <c>T</c> must implement <see cref="System.Numerics.INumber{T}"/>,
        /// but at runtime only <see cref="Real"/> and <see cref="Cplx"/> are dispatched to the underlying sparse BLAS implementation.
        /// </typeparam>
        /// <param name="a">The first sparse matrix operand. Its native handle is used by the underlying implementation.</param>
        /// <param name="b">The second sparse matrix operand. Its native handle is used by the underlying implementation.</param>
        /// <param name="c">
        /// Reference to the destination sparse matrix which will receive the result.
        /// On success the method will update <c>c.Handle</c> (may be reallocated by the native library) and populate
        /// <c>c.Rows</c>, <c>c.Cols</c> and <c>c.NzCount</c> from the exported CSR representation.
        /// </param>
        /// <param name="alpha">
        /// Scalar multiplier applied to <paramref name="b"/> before addition. When <c>alpha</c> equals the default value
        /// the implementation uses <c>1.0</c> for real types and <see cref="Cplx.One"/> for complex types.
        /// </param>
        /// <param name="operation">
        /// Specifies whether the sparse matrix operands should be used as-is, transposed or conjugate-transposed
        /// when computing the sum. Defaults to <see cref="SPARSE_Operation.NonTranspose"/>.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the underlying sparse BLAS implementation returns a non-success <see cref="SPARSE_Status"/> value.
        /// The exception message will contain the returned status for diagnostics.
        /// </exception>
        /// <exception cref="NotSupportedException">Thrown when the generic type <typeparamref name="T"/> is not supported (only <see cref="Real"/> and <see cref="Cplx"/>).</exception>
        /// <remarks>
        /// - The call dispatches to <c>factory.iSPBLAS.Add</c> and performs a runtime type-dispatch for <see cref="Real"/> and <see cref="Cplx"/>.
        /// - The destination matrix <paramref name="c"/> may be replaced (its native handle updated). After a successful call the method
        ///   exports the CSR of <paramref name="c"/> to set <c>Rows</c>, <c>Cols</c> and <c>NzCount</c>.
        /// - Callers should ensure that matrix dimensions and storage descriptors (if any) are appropriate for the requested operation.
        /// - This method does not perform structural validation beyond invoking the native routine; errors are surfaced via exceptions.
        /// </remarks>
        public static void Add<T>(SPMatx<T> a,
            SPMatx<T> b, ref SPMatx<T> c,
            T alpha = default!,
            SPARSE_Operation operation = SPARSE_Operation.NonTranspose)
            where T : INumber<T>
        {
            // consistency check ...

            SPARSE_Status status = SPARSE_Status.Success;
            if (typeof(T) == typeof(Real))
            {
                Real dAlpha = (alpha == default) ? 1.0 : Convert.ToDouble(alpha);
                IntPtr cPtr = c.Handle;
                status = factory.iSPBLAS.Add(operation, a.Handle, dAlpha,
                    b.Handle, ref cPtr);
                if (status != SPARSE_Status.Success) throw new InvalidOperationException($"Sparse matrix addition failed with status: {status}");
                c.Handle = cPtr;
            }
            else if (typeof(T) == typeof(Cplx))
            {
                Cplx zAlpha = (alpha == default) ? Cplx.One : (Cplx)Convert.ChangeType(alpha, typeof(Cplx));
                IntPtr cPtr = c.Handle;
                status = factory.iSPBLAS.Add(operation, a.Handle, &zAlpha,
                    b.Handle, ref cPtr);
                if (status != SPARSE_Status.Success) throw new InvalidOperationException($"Sparse matrix addition failed with status: {status}");
                c.Handle = cPtr;
            }
            else throw new NotSupportedException($"Type {typeof(T)} not supported.");

            // resulting matrix information
            c.ExportCSR(out _,
                out Int rows, out Int cols, out Int nnz,
                out _, out _, out _);
            c.Rows = rows;
            c.Cols = cols;
            c.NzCount = nnz;
        }

        #endregion
        #region ---- Dot (SPMM/SP2MM) ----

        /// <summary>
        /// Performs a sparse matrix–sparse matrix multiplication and stores the resulting sparse matrix in <paramref name="c"/>.
        /// Computes the product C := op(A) * B (with the <paramref name="operation"/> specifying whether an operand is transposed/conjugate-transposed)
        /// by dispatching to the underlying sparse BLAS implementation bound to <c>factory.iSPBLAS</c>.
        /// </summary>
        /// <typeparam name="T">
        /// The numeric element type of the sparse matrices. At compile-time <c>T</c> must implement <see cref="System.Numerics.INumber{T}"/>,
        /// but at runtime only <see cref="Real"/> and <see cref="Cplx"/> are dispatched to the native SPBLAS routine.
        /// </typeparam>
        /// <param name="a">The left operand sparse matrix. Its native handle (<see cref="SPMatx{T}.Handle"/>) is used by the native routine.</param>
        /// <param name="b">The right operand sparse matrix. Its native handle (<see cref="SPMatx{T}.Handle"/>) is used by the native routine.</param>
        /// <param name="c">
        /// A reference to the destination sparse matrix which will receive the result.
        /// On success the method may update <c>c.Handle</c> (the native handle can be reallocated by the native library).
        /// After the call the method exports the CSR representation of <paramref name="c"/> and sets <see cref="SPMatx{T}.Rows"/>,
        /// <see cref="SPMatx{T}.Cols"/> and <see cref="SPMatx{T}.NzCount"/> from the exported data.
        /// </param>
        /// <param name="operation">Specifies whether the matrix operands should be transposed or conjugate-transposed prior to multiplication. Defaults to <see cref="SPARSE_Operation.NonTranspose"/>.</param>
        /// <exception cref="InvalidOperationException">Thrown when the underlying sparse BLAS implementation returns a non-success <see cref="SPARSE_Status"/> value. The exception message contains the returned status for diagnostics.</exception>
        /// <exception cref="NotSupportedException">Thrown when the generic type <typeparamref name="T"/> is not supported by the sparse BLAS dispatch (only <see cref="Real"/> and <see cref="Cplx"/> at runtime).</exception>
        /// <remarks>
        /// - The method performs no structural validation of matrix dimensions; callers must ensure operands are compatible for the requested operation.
        /// - The call dispatches directly to <c>factory.iSPBLAS.Spmm</c>. The destination matrix <paramref name="c"/> may be replaced at native level;
        ///   after a successful call the managed <paramref name="c"/> object's handle and CSR metadata are updated.
        /// - The exact storage format and indexing semantics are implementation-defined and follow the conventions of the bound SPBLAS library.
        /// </remarks>
        public static void Dot<T>(SPMatx<T> a, SPMatx<T> b,
            ref SPMatx<T> c,
            SPARSE_Operation operation = SPARSE_Operation.NonTranspose)
            where T : INumber<T>
        {
            // consistency check ...

            SPARSE_Status status = SPARSE_Status.Success;
            IntPtr cPtr = c.Handle;
            status = factory.iSPBLAS.Spmm(operation,
                a.Handle, b.Handle, ref cPtr);
            if (status != SPARSE_Status.Success) throw new InvalidOperationException($"Sparse matrix-matrix multiplication failed with status: {status}");
            c.Handle = cPtr;

            // resulting matrix information
            c.ExportCSR(out _, 
                out Int rows, out Int cols, out Int nnz,
                out _, out _, out _);
            c.Rows = rows;
            c.Cols = cols;
            c.NzCount = nnz;
        }


        internal static void Dot<T>(SPMatx<T> a, SPMatx<T> b,
            ref SPMatx<T> c,
            SPARSE_Operation operationA = SPARSE_Operation.NonTranspose,
            SPARSE_MatrixDescr descrA = default,
            SPARSE_Operation operationB = SPARSE_Operation.NonTranspose,
            SPARSE_MatrixDescr descrB = default,
            SPARSE_Request request = SPARSE_Request.FullMult)
            where T : INumber<T>
        {
            // consistency check ...

            SPARSE_Status status = SPARSE_Status.Success;
            IntPtr cPtr = c.Handle;
            status = factory.iSPBLAS.Sp2mm(operationA, descrA, a.Handle, 
                operationB, descrB, b.Handle, request,
                ref cPtr);
            if (status != SPARSE_Status.Success) throw new InvalidOperationException($"Sparse matrix-matrix multiplication failed with status: {status}");
            c.Handle = cPtr;

            // resulting matrix information
            c.ExportCSR(out _, 
                out Int rows, out Int cols, out Int nnz,
                out _, out _, out _);
            c.Rows = rows;
            c.Cols = cols;
            c.NzCount = nnz;
        }

        #endregion
        // ---- Syrk ----
        // ---- Sypr ----
        #region ---- Dot (SPMMD/SP2MD) ----

        /// <summary>
        /// Computes the product of a sparse matrix and another sparse matrix and writes the dense result into <paramref name="c"/>.
        /// <para>
        /// This routine dispatches to the underlying platform-specific sparse BLAS implementation exposed by
        /// <c>factory.iSPBLAS</c> and supports runtime dispatch for the numeric types <see cref="Real"/> and <see cref="Cplx"/>.
        /// </para>
        /// </summary>
        /// <typeparam name="T">
        /// Numeric element type for the operation. At compile time <c>T</c> must implement <see cref="System.Numerics.INumber{T}"/>,
        /// but at runtime only <see cref="Real"/> and <see cref="Cplx"/> are dispatched to the native SPBLAS routine.
        /// </typeparam>
        /// <param name="a">The left-hand sparse matrix operand. Its native handle (<see cref="SPMatx{T}.Handle"/>) is used by the implementation.</param>
        /// <param name="b">The right-hand sparse matrix operand. Its native handle (<see cref="SPMatx{T}.Handle"/>) is used by the implementation.</param>
        /// <param name="c">
        /// The dense matrix that receives the computed result. The method updates <paramref name="c"/> in-place.
        /// The layout and column stride (via <see cref="Matx{T}.Cols"/>) are passed to the native routine.
        /// </param>
        /// <param name="operation">
        /// Specifies whether the sparse matrix <paramref name="a"/> should be transposed or conjugate-transposed
        /// before multiplication. Defaults to <see cref="SPARSE_Operation.NonTranspose"/>.
        /// </param>
        /// <param name="layout">
        /// Specifies the memory layout of the dense result matrix <paramref name="c"/> (row-major or column-major).
        /// Defaults to <see cref="SPARSE_Layout.RowMajor"/>.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the underlying sparse BLAS implementation returns a non-success <see cref="SPARSE_Status"/> value.
        /// The exception message contains the returned status for diagnostics.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// Thrown when the generic type <typeparamref name="T"/> is not supported by the sparse BLAS dispatch
        /// (only <see cref="Real"/> and <see cref="Cplx"/> are supported at runtime).
        /// </exception>
        /// <remarks>
        /// - No structural validation of matrix dimensions is performed in this method; callers must ensure operand
        ///   dimensions are compatible for the requested operation.
        /// - The call performs a runtime type-dispatch: for <see cref="Real"/> it passes the dense data pointer
        ///   (<c>c.DPtr</c>) and for <see cref="Cplx"/> it passes the complex data pointer (<c>c.VPtr</c>).
        /// - Any failure in the native library is propagated as an <see cref="InvalidOperationException"/>.
        /// </remarks>
        public static void Dot<T>(SPMatx<T> a, SPMatx<T> b,
            ref Matx<T> c,
            SPARSE_Operation operation = SPARSE_Operation.NonTranspose,
            SPARSE_Layout layout = SPARSE_Layout.RowMajor)
            where T : INumber<T>
        {
            // consistency check ...

            SPARSE_Status status = SPARSE_Status.Success;
            if (typeof(T) == typeof(Real))
            {
                status = factory.iSPBLAS.Spmmd(operation, a.Handle, b.Handle,
                    layout, c.DPtr, c.Cols);
                if (status != SPARSE_Status.Success) throw new InvalidOperationException($"Sparse matrix-matrix multiplication failed with status: {status}");
            }
            else if (typeof(T) == typeof(Cplx))
            {
                status = factory.iSPBLAS.Spmmd(operation, a.Handle, b.Handle,
                    layout, c.VPtr, c.Cols);
                if (status != SPARSE_Status.Success) throw new InvalidOperationException($"Sparse matrix-matrix multiplication failed with status: {status}");
            }
            else throw new NotSupportedException($"Type {typeof(T)} not supported.");
        }


        internal static void Dot<T>(SPMatx<T> a, SPMatx<T> b,
            ref Matx<T> c,
            T alpha = default!, double beta = default!,
            SPARSE_Operation operationA = SPARSE_Operation.NonTranspose,
            SPARSE_MatrixDescr descrA = default,
            SPARSE_Operation operationB = SPARSE_Operation.NonTranspose,
            SPARSE_MatrixDescr descrB = default,
            SPARSE_Layout layout = SPARSE_Layout.RowMajor)
            where T : INumber<T>
        {
            // consistency check ...

            SPARSE_Status status = SPARSE_Status.Success;
            if (typeof(T) == typeof(Real))
            {
                Real dAlpha = (alpha == default) ? 1.0 : Convert.ToDouble(alpha);
                Real dBeta = (beta == default) ? 0.0 : Convert.ToDouble(beta);
                status = factory.iSPBLAS.Sp2md(operationA, descrA, a.Handle,
                    operationB, descrB, b.Handle,
                    dAlpha, dBeta, c.DPtr, layout, c.Cols);
                if (status != SPARSE_Status.Success) throw new InvalidOperationException($"Sparse matrix-matrix multiplication failed with status: {status}");
            }
            else if (typeof(T) == typeof(Cplx))
            {
                Cplx zAlpha = (alpha == default) ? Cplx.One : (Cplx)Convert.ChangeType(alpha, typeof(Cplx));
                Cplx zBeta = (beta == default) ? Cplx.Zero : (Cplx)Convert.ChangeType(beta, typeof(Cplx));
                status = factory.iSPBLAS.Sp2md(operationA, descrA, a.Handle,
                    operationB, descrB, b.Handle,
                    &zAlpha, &zBeta, c.VPtr, layout, c.Cols);
                if (status != SPARSE_Status.Success) throw new InvalidOperationException($"Sparse matrix-matrix multiplication failed with status: {status}");
            }
            else throw new NotSupportedException($"Type {typeof(T)} not supported.");
        }


        #endregion
        // ---- Syrkd ----
        // ---- Syprd ----
        // ---- Sorv ----

        #endregion


    }
}
