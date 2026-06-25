using System.Numerics;

namespace VEMS.AMathCore
{

    /// <summary>
    /// Flexible GMRES (FGMRES) wrapper that uses Intel MKL's reverse-communication interface (RCI)
    /// to solve linear systems A * x = b for numeric types implementing <see cref="System.Numerics.INumber{T}"/>.
    /// The class manages MKL internal parameter arrays and a workspace required by MKL routines.
    /// </summary>
    /// <typeparam name="T">
    /// The numeric element type. Supported types: <c>Real</c> and <c>Cplx</c>.
    /// If <c>T</c> is <c>Cplx</c>, internal workspace sizes are adjusted (real representation).
    /// </typeparam>
    public unsafe class Fgmres<T>
        where T : INumber<T>
    {
        #region properties

        /// <summary>
        /// Integer parameter array passed to MKL FGMRES routines (<c>ipar</c> in MKL).
        /// </summary>
        private DenseArray<Int> iParam { get; set; }

        /// <summary>
        /// Double-precision parameter array passed to MKL FGMRES routines (<c>dpar</c> in MKL).
        /// </summary>
        private DenseArray<Real> dParam { get; set; }

        /// <summary>
        /// Temporary double-precision workspace used by MKL for intermediate vectors and scalars.
        /// The workspace layout and offsets are used during reverse communication callbacks.
        /// </summary>
        private Vect<Real> tmp { get; set; }

        #endregion
        #region constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Fgmres{T}"/> class.
        /// Prepares internal MKL RCI structures and checks parameters.
        /// </summary>
        /// <param name="n">
        /// Problem dimension (number of rows/columns of matrix <c>A</c>).
        /// For complex types (<c>Cplx</c>), the effective internal dimension is doubled.
        /// </param>
        /// <param name="residualStopTest">
        /// If <c>true</c>, enable MKL residual-based stopping test.
        /// </param>
        /// <param name="userStopTest">
        /// If <c>true</c>, enable user-defined stopping test (user must implement checks).
        /// </param>
        /// <param name="autoCheckNorn">
        /// If <c>true</c>, enable automatic checking of the norm of the next generated vector.
        /// Note: parameter name follows original code ("autoCheckNorn").
        /// </param>
        /// <param name="relTolerance">
        /// Relative tolerance passed to MKL as initial convergence criterion.
        /// </param>
        /// <exception cref="NotSupportedException">
        /// Thrown if <typeparamref name="T"/> is neither <c>Real</c> nor <c>Cplx</c>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when MKL initialization or parameter checking reports an unexpected RCI request.
        /// </exception>
        public Fgmres(Int n,
            bool residualStopTest = true,
            bool userStopTest = false,
            bool autoCheckNorn = true,
            //Int restartNumber = 2,
            Real relTolerance = 1.0E-3)
        {
            // common parameters
            iParam = new DenseArray<Int>(count: 128, initMode: ArrayInitMode.Calloc);
            dParam = new DenseArray<Real>(count: 128, initMode: ArrayInitMode.Calloc);

            // temporary array
            if (typeof(T) == typeof(Cplx)) { n *= 2; }
            else if (typeof(T) != typeof(Real)) throw new NotSupportedException($"FGMRES does not support type {typeof(T)}");
            
            Int m = Math.Min(150, n); // restart parameter
            Int tempSize = n * (2 * m + 1) + (m * (m + 9)) / 2 + 1;
            //Int tempSize = n * (2 * n + 1) + (n * (n + 9)) / 2 + 1;
            tmp = new Vect<Real>(count: tempSize, initMode: ArrayInitMode.Calloc);

            // local variables
            Int rci_Request = 0;

            // initialize
            Printer.WriteLine("FGMRES: Initializing MKL FGMRES solver.");
            IntelMKLNative.dfgmres_init(&n, null, null,
                &rci_Request, iParam.TPtr, dParam.DPtr, tmp.DPtr);
            if (rci_Request != 0) throw new InvalidOperationException($"FGMRES initialization failed with RCI_Request: {rci_Request}");

            // check parameters
            iParam[8] = residualStopTest ? 1 : 0; // do residual stopping test
            iParam[9] = userStopTest ? 1 : 0; // no user-defined stopping test
            iParam[11] = autoCheckNorn ? 1 : 0;  // automatic check for norm of next generated vector
            iParam[14] = m; // restart parameter
            dParam[0] = relTolerance; // relative tolerance
            Printer.WriteLine("FGMRES: Checking MKL FGMRES parameters.");
            IntelMKLNative.dfgmres_check(&n, null, null,
                &rci_Request, iParam.TPtr, dParam.DPtr, tmp.DPtr);
            if (rci_Request != 0 && rci_Request != -1001)
                throw new InvalidOperationException($"FGMRES parameter check failed with RCI_Request: {rci_Request}");
        }

        #endregion
        #region methods

        /// <summary>
        /// Solves the linear system A * x = b using FGMRES for a dense matrix <paramref name="a"/>.
        /// This method drives MKL's reverse communication loop and performs requested matrix-vector
        /// products when MKL issues an RCI request.
        /// </summary>
        /// <param name="a">The dense matrix <c>A</c> (row-major layout expected by MKL BLAS wrapper).</param>
        /// <param name="b">Right-hand side vector <c>b</c>. The vector length determines problem dimension.</param>
        /// <param name="x">
        /// On entry: initial guess for the solution (may be zero). On exit: updated with the computed solution.
        /// </param>
        /// <exception cref="NotSupportedException">
        /// Thrown if <typeparamref name="T"/> is not supported by this wrapper (only <c>Real</c> and <c>Cplx</c>).
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if an unexpected MKL RCI request code is encountered or MKL reports an internal error.
        /// </exception>
        /// <remarks>
        /// The method repeatedly calls <c>IntelMKLNative.dfgmres</c> until MKL returns <c>rci_Request == 0</c>.
        /// When MKL sets <c>rci_Request == 1</c>, it requests the computation of a matrix-vector product:
        /// compute A * tmp[ipar[21]-1] and store the result in tmp[ipar[22]-1]. The method performs the
        /// multiply using BLAS for dense matrices:
        /// - For <c>Real</c>: calls <c>cblas_dgemv_64</c>.
        /// - For <c>Cplx</c>: calls <c>cblas_zgemv_64</c>.
        /// After the loop completes, the method calls <c>dfgmres_get</c> to obtain final status and iteration count.
        /// </remarks>
        public void Solve(Matx<T> a, Vect<T> b,
            ref Vect<T> x)
        {
            Int rci_Request = 0;
            Int n = b.Count;
            if (typeof(T) == typeof(Cplx)) { n *= 2; }
            else if (typeof(T) != typeof(Real)) throw new NotSupportedException($"FGMRES does not support type {typeof(T)}");

            // reverse communication loop
            while (true)
            {
                IntelMKLNative.dfgmres(&n, x.DPtr, b.DPtr,
                    &rci_Request, iParam.TPtr, dParam.DPtr, tmp.DPtr);
                if (rci_Request == 0)
                { break; }
                else if (rci_Request == 1)
                {
                    // request: compute A * tmp[ipar[21]-1] -> tmp[ipar[22]-1]
                    // ipar[21] and ipar[22] contain Fortran-style addresses (1-based)
                    Int srcIndex = iParam[21] - 1;
                    Int dstIndex = iParam[22] - 1;
                    Real* pt = tmp.DPtr;
                    // tmp holds contiguous vectors starting at those positions
                    // perform y = A * x_tmp
                    if (typeof(T) == typeof(Real))
                        IntelMKLNative.cblas_dgemv_64(BLAS_Layout.RowMajor, BLAS_Transpose.NoTrans,
                            a.Rows, a.Cols, 1.0, a.DPtr, a.Cols, &pt[srcIndex], 1, 0.0, &pt[dstIndex], 1);
                    else if (typeof(T) == typeof(Cplx))
                    {
                        Cplx alpha = Cplx.One;
                        Cplx beta = Cplx.Zero;
                        IntelMKLNative.cblas_zgemv_64(BLAS_Layout.RowMajor, BLAS_Transpose.NoTrans,
                            a.Rows, a.Cols, &alpha, a.VPtr, a.Cols, &pt[srcIndex], 1, &beta, &pt[dstIndex], 1);
                    }
                    // continue the RCI loop
                    continue;
                }
                else throw new InvalidOperationException($"FGMRES unexpected RCI_Request: {rci_Request}");
            }

            // get results
            Int iterCount = 0;
            IntelMKLNative.dfgmres_get(&n, x.DPtr, b.DPtr,
                &rci_Request, iParam.TPtr, dParam.DPtr, tmp.DPtr, &iterCount);
        }


        /// <summary>
        /// Solves the linear system A * x = b using FGMRES for a sparse matrix <paramref name="a"/>.
        /// This method drives MKL's reverse communication loop and performs requested sparse matrix-vector
        /// products when MKL issues an RCI request.
        /// </summary>
        /// <param name="a">The sparse matrix <c>A</c> represented by <see cref="SPMatx{T}"/> and a MKL handle.</param>
        /// <param name="b">Right-hand side vector <c>b</c>. The vector length determines problem dimension.</param>
        /// <param name="x">
        /// On entry: initial guess for the solution (may be zero). On exit: updated with the computed solution.
        /// </param>
        /// <exception cref="NotSupportedException">
        /// Thrown if <typeparamref name="T"/> is not supported by this wrapper (only <c>Real</c> and <c>Cplx</c>).
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if an unexpected MKL RCI request code is encountered or MKL reports an internal error.
        /// </exception>
        /// <remarks>
        /// Behavior mirrors the dense <see cref="Solve(Matx{T}, Vect{T}, ref Vect{T})"/> overload except that
        /// sparse matrix-vector multiplication is performed using MKL sparse routines:
        /// - For <c>Real</c>: calls <c>mkl_sparse_d_mv_64</c>.
        /// - For <c>Cplx</c>: calls <c>mkl_sparse_z_mv_64</c>.
        /// The method responds to MKL's <c>rci_Request == 1</c> by computing A * tmp[src] -> tmp[dst].
        /// </remarks>
        public void Solve(SPMatx<T> a, Vect<T> b,
            ref Vect<T> x)
        {
            Int rci_Request = 0;
            Int n = b.Count;
            if (typeof(T) == typeof(Cplx)) { n *= 2; }
            else if (typeof(T) != typeof(Real)) throw new NotSupportedException($"FGMRES does not support type {typeof(T)}");

            // reverse communication loop
            while (true)
            {
                IntelMKLNative.dfgmres(&n, x.DPtr, b.DPtr,
                    &rci_Request, iParam.TPtr, dParam.DPtr, tmp.DPtr);
                if (rci_Request == 0)
                { break; }
                else if (rci_Request == 1)
                {
                    //Printer.WriteLine("FGMRES: Performing sparse matrix-vector product as requested by MKL RCI.");
                    // request: compute A * tmp[ipar[21]-1] -> tmp[ipar[22]-1]
                    // ipar[21] and ipar[22] contain Fortran-style addresses (1-based)
                    Int srcIndex = iParam[21] - 1;
                    Int dstIndex = iParam[22] - 1;
                    Real* pt = tmp.DPtr;
                    // tmp holds contiguous vectors starting at those positions
                    // perform y = A * x_tmp
                    if (typeof(T) == typeof(Real))
                        IntelMKLNative.mkl_sparse_d_mv_64(SPARSE_Operation.NonTranspose,
                            1.0, a.Handle, a.MatrixDescr, &pt[srcIndex], 0.0, &pt[dstIndex]);
                    else if (typeof(T) == typeof(Cplx))
                    {
                        Cplx alpha = Cplx.One;
                        Cplx beta = Cplx.Zero;
                        IntelMKLNative.mkl_sparse_z_mv_64(SPARSE_Operation.NonTranspose,
                            &alpha, a.Handle, a.MatrixDescr, &pt[srcIndex], &beta, &pt[dstIndex]);
                    }
                    // continue the RCI loop
                    continue;
                }
                else throw new InvalidOperationException($"FGMRES unexpected RCI_Request: {rci_Request}");
            }

            // get results
            Int iterCount = 0;
            IntelMKLNative.dfgmres_get(&n, x.DPtr, b.DPtr,
                &rci_Request, iParam.TPtr, dParam.DPtr, tmp.DPtr, &iterCount);
        }



        internal void Solve(IMatx<T> a, Vect<T> b,
            ref Vect<T> x)
        {
            Int rci_Request = 0;
            Int n = b.Count;
            if (typeof(T) == typeof(Cplx)) { n *= 2; }
            else if (typeof(T) != typeof(Real)) throw new NotSupportedException($"FGMRES does not support type {typeof(T)}");

            // reverse communication loop
            while (true)
            {
                IntelMKLNative.dfgmres(&n, x.DPtr, b.DPtr,
                    &rci_Request, iParam.TPtr, dParam.DPtr, tmp.DPtr);
                if (rci_Request == 0)
                { break; }
                else if (rci_Request == 1)
                {
                    // request: compute A * tmp[ipar[21]-1] -> tmp[ipar[22]-1]
                    // ipar[21] and ipar[22] contain Fortran-style addresses (1-based)
                    Int srcIndex = iParam[21] - 1;
                    Int dstIndex = iParam[22] - 1;
                    Real* pt = tmp.DPtr;
                    // tmp holds contiguous vectors starting at those positions
                    // perform y = A * x_tmp
                    Vect<T> x_tmp = Vect<T>.Create(n, default);
                    Vect<T> y_tmp = Vect<T>.Create(n, default);
                    if (typeof(T) == typeof(Real))
                    {
                        if (typeof(IMatx<>) == typeof(SPMatx<>))
                        { } // Sparse.Dot(a, x_tmp, ref y_tmp); 
                        else if (typeof(IMatx<>) == typeof(Matx<>))
                        { } // LinAlg.Dot(a, x_tmp, ref y_tmp);
                        else throw new NotSupportedException();
                    }
                    else if (typeof(T) == typeof(Cplx))
                    {
                        Cplx alpha = Cplx.One;
                        Cplx beta = Cplx.Zero;
                        if (typeof(IMatx<>) == typeof(SPMatx<>))
                        { }
                        else if (typeof(IMatx<>) == typeof(Matx<>))
                        { }
                        else throw new NotSupportedException();
                    }
                    // continue the RCI loop
                    continue;
                }
                else throw new InvalidOperationException($"FGMRES unexpected RCI_Request: {rci_Request}");
            }

            // get results
            Int iterCount = 0;
            IntelMKLNative.dfgmres_get(&n, x.DPtr, b.DPtr,
                &rci_Request, iParam.TPtr, dParam.DPtr, tmp.DPtr, &iterCount);

        }

        #endregion
    }

}
