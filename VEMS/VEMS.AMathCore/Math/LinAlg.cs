using System.Numerics;

namespace VEMS.AMathCore
{

    /// <summary>
    /// LinAlg factory
    /// </summary>
    internal class LinAlgFactory
    {
        internal IBLAS iBLAS { get; set; }
        internal ILAPACK iLAPACK { get; set; }
        //internal ISPBLAS iSPBLAS { get; set; }
        internal LinAlgFactory()
        {
            iBLAS = Defaults.IBLAS;
            iLAPACK = Defaults.ILAPACK;
            //iSPBLAS = Defaults.ISPBLAS;
        }
    }


    public unsafe class LinAlg
    {
        private static LinAlgFactory factory = new();

        #region Basic setting methods

        /// <summary>
        /// set up the BLAS interface with options
        /// from IntelMKL, OpenBLAS, etc
        /// </summary>
        /// <param name="option"> BLAS interface options </param>
        public static void SetIBLAS(IBLAS option)
            => factory.iBLAS = option;

        /// <summary>
        /// set up the LAPACK interface with options
        /// from IntelMKL, OpenBLAS, etc
        /// </summary>
        /// <param name="option"> LAPACK interface options </param>
        public static void SetILAPACK(ILAPACK option)
            => factory.iLAPACK = option;

        #endregion

        #region BLAS methods

        #region ---- Dot (VV) ----

        /// <summary>
        /// Computes the dot product of two vectors.
        /// res = x[0]*y[0] + x[1]*y[1] +...+ x[n-1]*y[n-1]
        /// </summary>
        /// <param name="x">The first input vector.</param>
        /// <param name="y">The second input vector.</param>
        /// <param name="incx">Increment for indexing <paramref name="x"/> (default is 1).</param>
        /// <param name="incy">Increment for indexing <paramref name="y"/> (default is 1).</param>
        /// <returns>The result of the dot product of <paramref name="x"/> and <paramref name="y"/>.</returns>
        public static T Dot<T>(Vect<T> x, Vect<T> y,
            Int incx = 1, Int incy = 1)
            where T : INumber<T>
        {
            if (typeof(T) == typeof(Real))
            {
                double dot = factory.iBLAS.Dot(x.Count, x.DPtr, y.DPtr, incx, incy);
                return (T)Convert.ChangeType(dot, typeof(T));
            }
            else if (typeof(T) == typeof(Cplx))
            {
                Cplx dotu;
                factory.iBLAS.Dot(x.Count, x.VPtr, y.VPtr, &dotu, incx, incy);
                return (T)Convert.ChangeType(dotu, typeof(T));
            }
            else throw new NotSupportedException($"Type {typeof(T)} not supported.");
        }

        #endregion
        #region ---- Dotc (VV) ----

        /// <summary>
        /// Computes the dot product of two complex-valued vectors, conjugating the first vector.
        /// <para>
        /// dotc = conj(x[0])*y[0] + conj(x[1])*y[1] + ... + conj(x[n-1])*y[n-1]
        /// </para>
        /// </summary>
        /// <param name="x">The first input vector (to be conjugated).</param>
        /// <param name="y">The second input vector.</param>
        /// <param name="incx">Increment for indexing <paramref name="x"/> (default is 1).</param>
        /// <param name="incy">Increment for indexing <paramref name="y"/> (default is 1).</param>
        /// <returns>The result of the conjugated dot product of <paramref name="x"/> and <paramref name="y"/> as a <see cref="Cplx"/> value.</returns>
        public static Cplx Dotc(Vect<Cplx> x, Vect<Cplx> y,
            Int incx = 1, Int incy = 1)
        {
            Cplx dotc;
            factory.iBLAS.Dotc(x.Count, x.VPtr, y.VPtr, &dotc, incx, incy);
            return dotc;
        }

        #endregion
        #region ---- Dot (MV) ----

        /// <summary>
        /// Computes the matrix-vector product 
        /// y := alpha * op(a) * x + beta * y 
        /// for real-valued matrices and vectors.
        /// </summary>
        /// <param name="a">The input matrix.</param>
        /// <param name="x">The input vector.</param>
        /// <param name="y">The output vector (overwritten as output).</param>
        /// <param name="alpha">Scalar multiplier for the matrix-vector product (default is 1.0).</param>
        /// <param name="beta">Scalar multiplier for the output vector y (default is 0.0).</param>
        /// <param name="operation">Specifies the operation to perform on matrix a (default is <see cref="BLAS_Transpose.NoTrans"/>).</param>
        /// <param name="incx"></param>
        /// <param name="incy"></param>
        public static void Dot<T>(Matx<T> a, Vect<T> x,
            ref Vect<T> y,
            T alpha = default!, T beta = default!,
            BLAS_Transpose operation = BLAS_Transpose.NoTrans,
            Int incx = 1, Int incy = 1)
            where T : INumber<T>
        {
            //Int opaCols = (operation == BLAS_Transpose.NoTrans) ? a.Cols : a.Rows;
            //if (opaCols != x.Count) { throw new ArgumentException($"dimensions of input matrix and vector not match"); }

            if (typeof(T) == typeof(Real))
            {
                double dAlpha = (alpha == default) ? 1.0 : Convert.ToDouble(alpha);
                double dBeta = (beta == default) ? 0.0 : Convert.ToDouble(beta);
                factory.iBLAS.Gemv(BLAS_Layout.RowMajor,
                operation, a.Rows, a.Cols, dAlpha, a.DPtr, a.Cols,
                    x.DPtr, dBeta, y.DPtr, incx, incy);
            }
            else if (typeof(T) == typeof(Cplx))
            {
                Cplx zAlpha = (alpha == default) ? Cplx.One : (Cplx)Convert.ChangeType(alpha, typeof(Cplx));
                Cplx zBeta = (beta == default) ? Cplx.Zero : (Cplx)Convert.ChangeType(beta, typeof(Cplx));
                //Printer.WriteLine($"zAlpha = {zAlpha}; zBeta = {zBeta}");
                factory.iBLAS.Gemv(BLAS_Layout.RowMajor,
                    operation, a.Rows, a.Cols, &zAlpha, a.VPtr, a.Cols,
                    x.VPtr, &zBeta, y.VPtr, incx, incy);
            }
            else
            {
                throw new NotSupportedException($"Type {typeof(T)} not supported.");
            }
        }

        #endregion
        #region ---- Dot (MM) ----

        /// <summary>
        /// Computes the matrix-matrix product for real-valued matrices.
        /// C := alpha * op(A) * op(B) + beta * C
        /// </summary>
        /// <param name="a">The first input matrix.</param>
        /// <param name="b">The second input matrix.</param>
        /// <param name="c">The output matrix (overwritten as output).</param>
        /// <param name="alpha">Scalar multiplier for the matrix-matrix product (default is 1.0).</param>
        /// <param name="beta">Scalar multiplier for the output matrix C (default is 0.0).</param>
        /// <param name="operationA">Specifies the operation to perform on matrix A (default is <see cref="BLAS_Transpose.NoTrans"/>).</param>
        /// <param name="operationB">Specifies the operation to perform on matrix B (default is <see cref="BLAS_Transpose.NoTrans"/>).</param>
        /// <exception cref="ArgumentException">Thrown if the dimensions of the input matrices do not match for multiplication.</exception>
        public static void Dot<T>(Matx<T> a, Matx<T> b,
            ref Matx<T> c,
            T alpha = default!, double beta = default!,
            BLAS_Transpose operationA = BLAS_Transpose.NoTrans,
            BLAS_Transpose operationB = BLAS_Transpose.NoTrans)
            where T : INumber<T>
        {
            //Int opaCols = (operationA == BLAS_Transpose.NoTrans) ? a.Cols : a.Rows;
            //Int opaRows = (operationA == BLAS_Transpose.NoTrans) ? a.Rows : a.Cols;
            //Int opbCols = (operationB == BLAS_Transpose.NoTrans) ? b.Cols : b.Rows;
            //Int opbRows = (operationB == BLAS_Transpose.NoTrans) ? b.Rows : b.Cols;

            //// columns in matrix a must equal to rows in matrix b
            //// and, columns and rows in matrix c must match
            //if (opaCols != opbRows || c.Rows != opaRows || c.Cols != opbCols)
            //{ throw new ArgumentException($"dimensions of input matrices not match"); }

            if (typeof(T) == typeof(Real))
            {
                double dAlpha = (alpha == default) ? 1.0 : Convert.ToDouble(alpha);
                double dBeta = (beta == default) ? 0.0 : Convert.ToDouble(beta);
                factory.iBLAS.Gemm(BLAS_Layout.RowMajor,
                    operationA, operationB,
                    a.Rows, a.Cols, a.Cols, dAlpha, a.DPtr, a.Cols,
                    b.DPtr, b.Cols, dBeta, c.DPtr, c.Cols);
            }
            else if (typeof(T) == typeof(Cplx))
            {
                Cplx zAlpha = (alpha == default) ? Cplx.One : (Cplx)Convert.ChangeType(alpha, typeof(Cplx));
                Cplx zBeta = (beta == default) ? Cplx.Zero : (Cplx)Convert.ChangeType(beta, typeof(Cplx));
                factory.iBLAS.Gemm(BLAS_Layout.RowMajor,
                    operationA, operationB,
                    a.Rows, a.Cols, a.Cols, &zAlpha, a.VPtr, a.Cols,
                    b.VPtr, b.Cols, &zBeta, c.VPtr, c.Cols);
            }
            else
            {
                throw new NotSupportedException($"Type {typeof(T)} not supported.");
            }
        }

        #endregion
        #region ---- Transpose ----

        /// <summary>
        /// Performs an in-place transpose of a real-valued matrix, with optional scaling.
        /// </summary>
        /// <param name="a">The matrix to transpose (overwritten on exit).</param>
        /// <param name="alpha">Scaling factor applied to all elements during transpose (default is 1.0).</param>
        public static void Transpose<T>(ref Matx<T> a,
            T alpha = default!)
            where T : INumber<T>
        {
            Int origRows = a.Rows;
            Int origCols = a.Cols;

            if (typeof(T) == typeof(Real))
            {
                double dAlpha = (alpha == default) ? 1.0 : Convert.ToDouble(alpha);
                factory.iBLAS.ImatCopy(BLAS_Layout.RowMajor,
                    BLAS_Transpose.Trans, a.Rows, a.Cols,
                    dAlpha, a.DPtr, a.Cols, a.Rows);
            }
            else if (typeof(T) == typeof(Cplx))
            {
                Cplx zAlpha = (alpha == default) ? Cplx.One : (Cplx)Convert.ChangeType(alpha, typeof(Cplx));
                factory.iBLAS.ImatCopy(BLAS_Layout.RowMajor,
                    BLAS_Transpose.Trans, a.Rows, a.Cols,
                    &zAlpha, a.DPtr, a.Cols, a.Rows);
            }
            else
            {
                throw new NotSupportedException($"Type {typeof(T)} not supported.");
            }

            a.Rows = origCols;
            a.Cols = origRows;
        }

        #endregion
        #region ---- ConjTrans ----

        /// <summary>
        /// Performs an in-place conjugate transpose of a complex-valued matrix, with optional scaling.
        /// </summary>
        /// <param name="a">The complex matrix to conjugate transpose (overwritten on exit).</param>
        /// <param name="alpha">Scaling factor applied to all elements during conjugate transpose (default is <see cref="Cplx.One"/>).</param>
        public static void ConjTrans(ref Matx<Cplx> a,
            Cplx alpha = default)
        {
            Int origRows = a.Rows;
            Int origCols = a.Cols;

            if (alpha == default) { alpha = Cplx.One; }
            factory.iBLAS.ImatCopy(BLAS_Layout.RowMajor,
                BLAS_Transpose.ConjTrans, a.Rows, a.Cols,
                &alpha, a.VPtr, a.Cols, a.Rows);

            a.Rows = origCols;
            a.Cols = origRows;
        }

        #endregion

        #endregion
        #region LAPACK methods

        #region ---- LUFactorize ----

        /// <summary>
        /// Computes the LU factorization of a real-valued matrix using LAPACK's <c>getrf</c> routine.
        /// The input matrix <paramref name="a"/> is overwritten by its LU decomposition.
        /// </summary>
        /// <param name="a">The input matrix to factorize. On exit, contains the combined L and U factors.</param>
        /// <param name="ipiv">Output vector containing the pivot indices. The length is equal to the number of rows in <paramref name="a"/>.</param>
        public static void LUFactorize<T>(ref Matx<T> a, out Vect<Int> ipiv)
            where T : INumber<T>
        {
            ipiv = new(count: a.Rows, initMode: ArrayInitMode.Calloc);

            if (typeof(T) == typeof(Real))
                factory.iLAPACK.Getrf(LAPACK_Layout.RowMajor, a.Rows, a.Cols,
                    a.DPtr, a.Cols, ipiv.TPtr);
            else if (typeof(T) == typeof(Cplx))
                factory.iLAPACK.Getrf(LAPACK_Layout.RowMajor, a.Rows, a.Cols,
                    a.VPtr, a.Cols, ipiv.TPtr);
            else throw new NotSupportedException($"Type {typeof(T)} not supported.");
        }

        #endregion
        #region ---- Inverse ----

        /// <summary>
        /// Computes the inverse of a real-valued matrix in-place using LAPACK routines.
        /// The input matrix <paramref name="a"/> is overwritten by its inverse.
        /// </summary>
        /// <param name="a">The input matrix to invert. On exit, contains the inverse of the original matrix.</param>
        public static void Inverse<T>(ref Matx<T> a)
            where T : INumber<T>
        {
            Vect<Int> ipiv = new(count: a.Rows, initMode: ArrayInitMode.Calloc);

            if (typeof(T) == typeof(Real))
            {
                factory.iLAPACK.Getrf(LAPACK_Layout.RowMajor, a.Rows, a.Cols,
                    a.DPtr, a.Cols, ipiv.TPtr);
                factory.iLAPACK.Getri(LAPACK_Layout.RowMajor, a.Cols,
                    a.DPtr, a.Cols, ipiv.TPtr);
            }
            else if (typeof(T) == typeof(Cplx))
            {
                factory.iLAPACK.Getrf(LAPACK_Layout.RowMajor, a.Rows, a.Cols,
                a.DPtr, a.Cols, ipiv.TPtr);
                factory.iLAPACK.Getri(LAPACK_Layout.RowMajor, a.Cols,
                    a.VPtr, a.Cols, ipiv.TPtr);
            }
            else throw new NotSupportedException($"Type {typeof(T)} not supported.");
        }

        #endregion
        #region ---- LinearSolve ----

        /// <summary>
        /// Solves a system of linear equations A * x = b for real-valued matrices and vectors.
        /// The input matrix <paramref name="a"/> is overwritten by its LU factorization,
        /// and the input vector <paramref name="b"/> is overwritten by the solution vector x.
        /// </summary>
        /// <param name="a">The coefficient matrix A (overwritten by its LU factorization on exit).
        /// Must be square and have the same number of columns as the length of <paramref name="b"/>.</param>
        /// <param name="b">The right-hand side vector b (overwritten by the solution vector x on exit).
        /// The length must match the number of columns in <paramref name="a"/>.</param>
        /// <exception cref="ArgumentException">
        /// Thrown if the number of columns in <paramref name="a"/> does not match the length of <paramref name="b"/>.
        /// </exception>
        public static void LinearSolve<T>(ref Matx<T> a,
            ref Vect<T> b)
            where T : INumber<T>
        {
            //if (a.Cols != b.Count)
            //{ throw new ArgumentException("matrix size and vector length not match"); }

            Vect<Int> ipiv = new(count: a.Rows, initMode: ArrayInitMode.Calloc);
            if (typeof(T) == typeof(Real))
                factory.iLAPACK.Gesv(LAPACK_Layout.RowMajor, a.Rows, 1,
                    a.DPtr, a.Cols, ipiv.TPtr, b.DPtr, 1);
            else if (typeof(T) == typeof(Cplx))
                factory.iLAPACK.Gesv(LAPACK_Layout.RowMajor, a.Rows, 1,
                    a.VPtr, a.Cols, ipiv.TPtr, b.VPtr, 1);
            else throw new NotSupportedException($"Type {typeof(T)} not supported.");
        }


        /// <summary>
        /// Solves a system of linear equations A * X = B for real-valued matrices.
        /// The input matrix <paramref name="a"/> is overwritten by its LU factorization,
        /// and the input matrix <paramref name="b"/> is overwritten by the solution matrix X.
        /// </summary>
        /// <param name="a">The coefficient matrix A (overwritten by its LU factorization on exit).
        /// Must be square and have the same number of columns as the number of rows in <paramref name="b"/>.</param>
        /// <param name="b">The right-hand side matrix B (overwritten by the solution matrix X on exit).
        /// The number of rows must match the number of columns in <paramref name="a"/>.</param>
        /// <exception cref="ArgumentException">
        /// Thrown if the number of columns in <paramref name="a"/> does not match the number of rows in <paramref name="b"/>.
        /// </exception>
        public static void LinearSolve<T>(ref Matx<T> a,
            ref Matx<T> b)
            where T : INumber<T>
        {
            //if (a.Cols != b.Rows)
            //{ throw new ArgumentException("matrix size and vector length not match"); }
            
            Vect<Int> ipiv = new(count: a.Rows, initMode: ArrayInitMode.Calloc);
            if (typeof(T) == typeof(Real))
                factory.iLAPACK.Gesv(LAPACK_Layout.RowMajor, a.Rows, b.Cols,
                    a.DPtr, a.Cols, ipiv.TPtr, b.DPtr, b.Cols);
            else if(typeof(T) == typeof(Cplx))
                factory.iLAPACK.Gesv(LAPACK_Layout.RowMajor, a.Rows, b.Cols,
                    a.VPtr, a.Cols, ipiv.TPtr, b.VPtr, b.Cols);
            else throw new NotSupportedException($"Type {typeof(T)} not supported.");
        }

        #endregion
        #region ---- LeastSquare ----

        /// <summary>
        /// Computes the minimum-norm solution to a real linear least squares problem using LAPACK's <c>Gels</c> routine.
        /// The input matrix <paramref name="a"/> is overwritten by its QR or LQ factorization,
        /// and the input vector <paramref name="b"/> is overwritten by the solution vector x.
        /// </summary>
        /// <param name="a">The coefficient matrix A (overwritten by its QR or LQ factorization on exit).
        /// The number of rows must match the length of <paramref name="b"/>.</param>
        /// <param name="b">The right-hand side vector b (overwritten by the solution vector x on exit).
        /// The length must match the number of rows in <paramref name="a"/>.</param>
        /// <exception cref="ArgumentException">
        /// Thrown if the number of rows in <paramref name="a"/> does not match the length of <paramref name="b"/>.
        /// </exception>
        public static void LeastSquare<T>(ref Matx<T> a,
            ref Vect<T> b)
            where T : INumber<T>
        {
            //if (a.Rows != b.Count)
            //{ throw new ArgumentException("matrix size and vector length not match"); }

            Vect<Int> ipiv = new(count: Math.Min(a.Rows, a.Cols), initMode: ArrayInitMode.Calloc);
            if (typeof(T) == typeof(Real))
                factory.iLAPACK.Gels(LAPACK_Layout.RowMajor, LAPACK_Transpose.NoTrans,
                    a.Rows, a.Cols, 1,
                    a.DPtr, a.Cols, b.DPtr, 1); // Math.Max(a.Rows, a.Cols));
            else if (typeof(T) == typeof(Cplx))
                factory.iLAPACK.Gels(LAPACK_Layout.RowMajor, LAPACK_Transpose.NoTrans,
                    a.Rows, a.Cols, 1,
                    a.VPtr, a.Cols, b.VPtr, 1); // Math.Max(a.Rows, a.Cols));
            else throw new NotSupportedException($"Type {typeof(T)} not supported.");
        }


        /// <summary>
        /// Computes the minimum-norm solution to a real linear least squares problem using LAPACK's <c>Gels</c> routine.
        /// The input matrix <paramref name="a"/> is overwritten by its QR or LQ factorization,
        /// and the input matrix <paramref name="b"/> is overwritten by the solution matrix X.
        /// </summary>
        /// <param name="a">The coefficient matrix A (overwritten by its QR or LQ factorization on exit).
        /// The number of rows must match the number of rows in <paramref name="b"/>.</param>
        /// <param name="b">The right-hand side matrix B (overwritten by the solution matrix X on exit).
        /// The number of rows must match the number of rows in <paramref name="a"/>.</param>
        /// <exception cref="ArgumentException">
        /// Thrown if the number of rows in <paramref name="a"/> does not match the number of rows in <paramref name="b"/>.
        /// </exception>
        public static void LeastSquare<T>(ref Matx<T> a,
            ref Matx<T> b)
            where T : INumber<T>
        {
            //if (a.Rows != b.Rows)
            //{ throw new ArgumentException("matrix size and vector length not match"); }
            
            Vect<Int> ipiv = new(count: Math.Min(a.Rows, a.Cols), initMode: ArrayInitMode.Calloc);
            if (typeof(T) == typeof(Real))
                factory.iLAPACK.Gels(LAPACK_Layout.RowMajor, LAPACK_Transpose.NoTrans,
                    a.Rows, a.Cols, b.Cols,
                    a.DPtr, a.Cols, b.DPtr, b.Cols); // Math.Max(a.Rows, a.Cols));
            else if (typeof(T) == typeof(Cplx))
                factory.iLAPACK.Gels(LAPACK_Layout.RowMajor, LAPACK_Transpose.NoTrans,
                    a.Rows, a.Cols, b.Cols,
                    a.VPtr, a.Cols, b.VPtr, b.Cols); // Math.Max(a.Rows, a.Cols));
            else throw new NotSupportedException($"Type {typeof(T)} not supported.");
        }

        #endregion
        #region ---- EigenSystem ----

        /// <summary>
        /// Computes the eigenvalues and eigenvectors of a real-valued square matrix using LAPACK's <c>Geev</c> routine.
        /// The input matrix <paramref name="a"/> is overwritten by LAPACK during computation.
        /// </summary>
        /// <param name="a">The real-valued square matrix to analyze. Must be square (same number of rows and columns).
        /// On exit, the contents are overwritten.</param>
        /// <param name="eigenValuesReal">Output vector containing the real parts of the computed eigenvalues. Length equals <c>a.Rows</c>.</param>
        /// <param name="eigenValuesImag">Output vector containing the imaginary parts of the computed eigenvalues. Length equals <c>a.Rows</c>.</param>
        /// <param name="leftEigenVectors">Output matrix containing the left eigenvectors. Dimensions are <c>a.Rows</c> x <c>a.Cols</c>.</param>
        /// <param name="rightEigenVectors">Output matrix containing the right eigenvectors. Dimensions are <c>a.Rows</c> x <c>a.Cols</c>.</param>
        /// <exception cref="Exception">
        /// Thrown if the input matrix <paramref name="a"/> is not square.
        /// </exception>
        public static void EigenSystem<T>(ref Matx<T> a,
            out Vect<Cplx> eigenValues,
            out Matx<T> leftEigenVectors, out Matx<T> rightEigenVectors)
            where T : INumber<T>
        {
            //if (a.Cols != a.Rows)
            //{ throw new Exception("unequal rows and columns"); }

            eigenValues = new(count: a.Rows, initMode: ArrayInitMode.Calloc);
            leftEigenVectors = new(rows: a.Rows, cols: a.Cols, initMode: ArrayInitMode.Calloc);
            rightEigenVectors = new(rows: a.Rows, cols: a.Cols, initMode: ArrayInitMode.Calloc);
            if (typeof(T) == typeof(Real))
            {
                Vect<Real> eigenValuesReal = new(count: a.Rows, initMode: ArrayInitMode.Calloc);
                Vect<Real> eigenValuesImag = new(count: a.Rows, initMode: ArrayInitMode.Calloc);
                factory.iLAPACK.Geev(LAPACK_Layout.RowMajor, LAPACK_Job.V, LAPACK_Job.V, a.Cols,
                    a.DPtr, a.Cols, eigenValuesReal.DPtr, eigenValuesImag.DPtr,
                    leftEigenVectors.DPtr, a.Cols, rightEigenVectors.DPtr, a.Cols);
                VMath.Modify(eigenValuesReal, eigenValues, ComplexPart.RealPart);
                VMath.Modify(eigenValuesImag, eigenValues, ComplexPart.ImagPart);
            }
            else if(typeof(T) == typeof(Cplx))
            {
                factory.iLAPACK.Geev(LAPACK_Layout.RowMajor, LAPACK_Job.V, LAPACK_Job.V, a.Cols,
                    a.VPtr, a.Cols, eigenValues.VPtr,
                    leftEigenVectors.VPtr, a.Cols, rightEigenVectors.VPtr, a.Cols);
            }
            else throw new NotSupportedException($"Type {typeof(T)} not supported.");
        }


        /// <summary>
        /// Computes the eigenvalues and right eigenvectors of a real-valued square matrix using LAPACK's <c>Geev</c> routine.
        /// The input matrix <paramref name="a"/> is overwritten by LAPACK during computation.
        /// </summary>
        /// <param name="a">The real-valued square matrix to analyze. Must be square (same number of rows and columns).
        /// On exit, the contents are overwritten.</param>
        /// <param name="eigenValuesReal">Output vector containing the real parts of the computed eigenvalues. Length equals <c>a.Rows</c>.</param>
        /// <param name="eigenValuesImag">Output vector containing the imaginary parts of the computed eigenvalues. Length equals <c>a.Rows</c>.</param>
        /// <param name="eigenVectors">Output matrix containing the right eigenvectors. Dimensions are <c>a.Rows</c> x <c>a.Cols</c>.</param>
        /// <exception cref="Exception">
        /// Thrown if the input matrix <paramref name="a"/> is not square.
        /// </exception>
        public static void EigenSystem<T>(ref Matx<T> a,
            out Vect<Cplx> eigenValues,
            out Matx<T> eigenVectors)
            where T : INumber<T>
        {
            //if (a.Cols != a.Rows)
            //{ throw new Exception("unequal rows and columns"); }

            eigenValues = new(count: a.Rows, initMode: ArrayInitMode.Calloc);
            eigenVectors = new(rows: a.Rows, cols: a.Cols, initMode: ArrayInitMode.Calloc);

            if (typeof(T) == typeof(Real))
            {
                Vect<Real> eigenValuesReal = new(count: a.Rows, initMode: ArrayInitMode.Calloc);
                Vect<Real> eigenValuesImag = new(count: a.Rows, initMode: ArrayInitMode.Calloc);
                Matx<Real> dummy = new(rows: 1, cols: 1, initMode: ArrayInitMode.Calloc);
                factory.iLAPACK.Geev(LAPACK_Layout.RowMajor, LAPACK_Job.N, LAPACK_Job.V, a.Cols,
                    a.DPtr, a.Cols, eigenValuesReal.DPtr, eigenValuesImag.DPtr,
                    dummy.DPtr, a.Cols, eigenVectors.DPtr, a.Cols);
                VMath.Modify(eigenValuesReal, eigenValues, ComplexPart.RealPart);
                VMath.Modify(eigenValuesImag, eigenValues, ComplexPart.ImagPart);
            }
            else if (typeof(T) == typeof(Cplx))
            {
                Matx<Cplx> dummy = new(rows: 1, cols: 1, initMode: ArrayInitMode.Calloc);
                factory.iLAPACK.Geev(LAPACK_Layout.RowMajor, LAPACK_Job.N, LAPACK_Job.V, a.Cols,
                    a.VPtr, a.Cols, eigenValues.VPtr,
                    dummy.VPtr, a.Cols, eigenVectors.VPtr, a.Cols);
            }
            else throw new NotSupportedException($"Type {typeof(T)} not supported.");
        }


        /// <summary>
        /// Computes the eigenvalues of a real-valued square matrix using LAPACK's <c>Geev</c> routine.
        /// The input matrix <paramref name="a"/> is overwritten by LAPACK during computation.
        /// Only eigenvalues are computed; eigenvectors are not returned.
        /// </summary>
        /// <param name="a">The real-valued square matrix to analyze. Must be square (same number of rows and columns).
        /// On exit, the contents are overwritten.</param>
        /// <param name="eigenValuesReal">Output vector containing the real parts of the computed eigenvalues. Length equals <c>a.Rows</c>.</param>
        /// <param name="eigenValuesImag">Output vector containing the imaginary parts of the computed eigenvalues. Length equals <c>a.Rows</c>.</param>
        /// <exception cref="Exception">
        /// Thrown if the input matrix <paramref name="a"/> is not square.
        /// </exception>
        public static void EigenSystem<T>(ref Matx<T> a,
            out Vect<Cplx> eigenValues)
            where T : INumber<T>
        {
            //if (a.Cols != a.Rows)
            //{ throw new Exception("unequal rows and columns"); }

            eigenValues = new(count: a.Rows, initMode: ArrayInitMode.Calloc);
            if (typeof(T) == typeof(Real))
            {
                Vect<Real> eigenValuesReal = new(count: a.Rows, initMode: ArrayInitMode.Calloc);
                Vect<Real> eigenValuesImag = new(count: a.Rows, initMode: ArrayInitMode.Calloc);
                Matx<Real> dummyL = new(rows: 1, cols: 1, initMode: ArrayInitMode.Calloc);
                Matx<Real> dummyR = new(rows: 1, cols: 1, initMode: ArrayInitMode.Calloc);
                factory.iLAPACK.Geev(LAPACK_Layout.RowMajor, LAPACK_Job.N, LAPACK_Job.N, a.Cols,
                    a.DPtr, a.Cols, eigenValuesReal.DPtr, eigenValuesImag.DPtr,
                    dummyL.DPtr, a.Cols, dummyR.DPtr, a.Cols);
                VMath.Modify(eigenValuesReal, eigenValues, ComplexPart.RealPart);
                VMath.Modify(eigenValuesImag, eigenValues, ComplexPart.ImagPart);
            }
            else if (typeof(T) == typeof(Cplx))
            {
                Matx<Cplx> dummyL = new(rows: 1, cols: 1, initMode: ArrayInitMode.Calloc);
                Matx<Cplx> dummyR = new(rows: 1, cols: 1, initMode: ArrayInitMode.Calloc);
                factory.iLAPACK.Geev(LAPACK_Layout.RowMajor, LAPACK_Job.N, LAPACK_Job.N, a.Cols,
                    a.VPtr, a.Cols, eigenValues.VPtr,
                    dummyL.VPtr, a.Cols, dummyR.VPtr, a.Cols);
            }
            else throw new NotSupportedException($"Type {typeof(T)} not supported.");
        }

        #endregion


        #endregion

    }
}
