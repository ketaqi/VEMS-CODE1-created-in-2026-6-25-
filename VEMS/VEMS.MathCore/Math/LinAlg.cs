using Real = System.Double;
using Int = System.Int64;
using Complex = System.Numerics.Complex;

namespace VEMS.MathCore
{
    internal class LinAlgKernel
    {
        internal IBLAS iBLAS { get; set; }
        internal ILAPACK iLAPACK { get; set; }
        internal LinAlgKernel()
        {
            iBLAS = Defaults.IBLAS;
            iLAPACK = Defaults.ILAPACK; 
        }
    }

    internal class LinAlgFactory
    {
        internal IBLAS iBLAS { get; set; }
        internal ILAPACK iLAPACK { get; set; }
        internal LinAlgFactory()
        {
            iBLAS = Defaults.IBLAS;
            iLAPACK = Defaults.ILAPACK;
        }
    }

    /// <summary>
    /// linear algebra routines
    /// the static version for LinearAlgebra
    /// </summary>
    public unsafe class LinAlg
    {
        //private static LinAlgKernel kernel = new();
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
        #region BLAS standard methods

        #region ---- Dot (VV) ----

        /// <summary>
        /// Computes the dot product of two real-valued vectors.
        /// res = x[0]*y[0] + x[1]*y[1] +...+ x[n-1]*y[n-1]
        /// </summary>
        /// <param name="x">The first input vector.</param>
        /// <param name="y">The second input vector.</param>
        /// <param name="incx">Increment for indexing <paramref name="x"/> (default is 1).</param>
        /// <param name="incy">Increment for indexing <paramref name="y"/> (default is 1).</param>
        /// <returns>The result of the dot product of <paramref name="x"/> and <paramref name="y"/>.</returns>
        public static double Dot(Vect<Real> x, Vect<Real> y,
            long incx = 1, long incy = 1)
            => factory.iBLAS.Dot(x.Count, x.DPtr, y.DPtr, incx, incy);

        /// <summary>
        /// Computes the dot product of two complex-valued vectors.
        /// res = x[0]*y[0] + x[1]*y[1] +...+ x[n-1]*y[n-1]
        /// </summary>
        /// <param name="x">The first input vector.</param>
        /// <param name="y">The second input vector.</param>
        /// <param name="incx">Increment for indexing <paramref name="x"/> (default is 1).</param>
        /// <param name="incy">Increment for indexing <paramref name="y"/> (default is 1).</param>
        /// <returns>The result of the dot product of <paramref name="x"/> and <paramref name="y"/> as a <see cref="Cplx"/> value.</returns>
        public static Cplx Dot(Vect<Cplx> x, Vect<Cplx> y,
            long incx = 1, long incy = 1)
        {
            Cplx dotu;
            factory.iBLAS.Dot(x.Count, x.VPtr, y.VPtr, &dotu, incx, incy);
            return dotu;
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
            long incx = 1, long incy = 1)
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
        public static void Dot(Matx<Real> a, Vect<Real> x,
            ref Vect<Real> y,
            double alpha = 1.0, double beta = 0.0,
            BLAS_Transpose operation = BLAS_Transpose.NoTrans,
            long incx = 1, long incy = 1)
        {
            long opaCols = (operation == BLAS_Transpose.NoTrans) ? a.Cols : a.Rows;
            if (opaCols != x.Count) { throw new ArgumentException($"dimensions of input matrix and vector not match"); }

            factory.iBLAS.Gemv(BLAS_Layout.RowMajor,
                operation, a.Rows, a.Cols, alpha, a.DPtr, a.Cols,
                x.DPtr, beta, y.DPtr, incx, incy);
        }


        /// <summary>
        /// Computes the matrix-vector product 
        /// y := alpha * op(a) * x + beta * y 
        /// for complex-valued matrices and vectors.
        /// </summary>
        /// <param name="a">The input complex matrix.</param>
        /// <param name="x">The input complex vector.</param>
        /// <param name="y">The output complex vector (overwritten as output).</param>
        /// <param name="alpha">Scalar multiplier for the matrix-vector product (default is <see cref="Cplx.One"/>).</param>
        /// <param name="beta">Scalar multiplier for the output vector y (default is <see cref="Cplx.One"/>).</param>
        /// <param name="operation">Specifies the operation to perform on matrix a (default is <see cref="BLAS_Transpose.NoTrans"/>).</param>
        /// <param name="incx"></param>
        /// <param name="incy"></param>
        public static void Dot(Matx<Cplx> a, Vect<Cplx> x,
            ref Vect<Cplx> y,
            Cplx alpha = default, Cplx beta = default,
            BLAS_Transpose operation = BLAS_Transpose.NoTrans,
            long incx = 1, long incy = 1)
        {
            long opaCols = (operation == BLAS_Transpose.NoTrans) ? a.Cols : a.Rows;
            if (opaCols != x.Count) { throw new ArgumentException($"dimensions of input matrix and vector not match"); }

            if (alpha == default) { alpha = Cplx.One; }
            if (beta == default) { beta = Cplx.Zero; }

            factory.iBLAS.Gemv(BLAS_Layout.RowMajor,
                operation, a.Rows, a.Cols, &alpha, a.VPtr, a.Cols,
                x.VPtr, &beta, y.VPtr, incx, incy);
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
        public static void Dot(Matx<Real> a, Matx<Real> b,
            ref Matx<Real> c,
            double alpha = 1.0, double beta = 0.0,
            BLAS_Transpose operationA = BLAS_Transpose.NoTrans,
            BLAS_Transpose operationB = BLAS_Transpose.NoTrans)
        {
            long opaCols = (operationA == BLAS_Transpose.NoTrans) ? a.Cols : a.Rows;
            long opaRows = (operationA == BLAS_Transpose.NoTrans) ? a.Rows : a.Cols;
            long opbCols = (operationB == BLAS_Transpose.NoTrans) ? b.Cols : b.Rows;
            long opbRows = (operationB == BLAS_Transpose.NoTrans) ? b.Rows : b.Cols;

            // columns in matrix a must equal to rows in matrix b
            // and, columns and rows in matrix c must match
            if (opaCols != opbRows || c.Rows != opaRows || c.Cols != opbCols)
            { throw new ArgumentException($"dimensions of input matrices not match"); }

            factory.iBLAS.Gemm(BLAS_Layout.RowMajor,
                operationA, operationB,
                a.Rows, a.Cols, a.Cols, alpha, a.DPtr, a.Cols,
                b.DPtr, b.Cols, beta, c.DPtr, c.Cols);
        }

        /// <summary>
        /// Computes the matrix-matrix product for complex-valued matrices.
        /// C := alpha * op(A) * op(B) + beta * C
        /// </summary>
        /// <param name="a">The first input complex matrix.</param>
        /// <param name="b">The second input complex matrix.</param>
        /// <param name="c">The output complex matrix (overwritten as output).</param>
        /// <param name="alpha">Scalar multiplier for the matrix-matrix product (default is <see cref="Cplx.One"/>).</param>
        /// <param name="beta">Scalar multiplier for the output matrix C (default is <see cref="Cplx.Zero"/>).</param>
        /// <param name="operationA">Specifies the operation to perform on matrix A (default is <see cref="BLAS_Transpose.NoTrans"/>).</param>
        /// <param name="operationB">Specifies the operation to perform on matrix B (default is <see cref="BLAS_Transpose.NoTrans"/>).</param>
        /// <exception cref="ArgumentException">Thrown if the dimensions of the input matrices do not match for multiplication.</exception>
        public static void Dot(Matx<Cplx> a, Matx<Cplx> b,
            ref Matx<Cplx> c,
            Cplx alpha = default, Cplx beta = default,
            BLAS_Transpose operationA = BLAS_Transpose.NoTrans,
            BLAS_Transpose operationB = BLAS_Transpose.NoTrans)
        {
            long opaCols = (operationA == BLAS_Transpose.NoTrans) ? a.Cols : a.Rows;
            long opaRows = (operationA == BLAS_Transpose.NoTrans) ? a.Rows : a.Cols;
            long opbCols = (operationB == BLAS_Transpose.NoTrans) ? b.Cols : b.Rows;
            long opbRows = (operationB == BLAS_Transpose.NoTrans) ? b.Rows : b.Cols;

            // columns in matrix a must equal to rows in matrix b
            // and, columns and rows in matrix c must match
            if (opaCols != opbRows || c.Rows != opaRows || c.Cols != opbCols)
            { throw new ArgumentException($"dimensions of input matrices not match"); }

            if (alpha == default) { alpha = Cplx.One; }
            if (beta == default) { beta = Cplx.Zero; }

            factory.iBLAS.Gemm(BLAS_Layout.RowMajor,
                operationA, operationB,
                a.Rows, a.Cols, a.Cols, &alpha, a.DPtr, a.Cols,
                b.DPtr, b.Cols, &beta, c.DPtr, c.Cols);
        }

        #endregion
        #region ---- Transpose ----

        /// <summary>
        /// Performs an in-place transpose of a real-valued matrix, with optional scaling.
        /// </summary>
        /// <param name="a">The matrix to transpose (overwritten on exit).</param>
        /// <param name="alpha">Scaling factor applied to all elements during transpose (default is 1.0).</param>
        public static void Transpose(ref Matx<double> a,
            double alpha = 1.0)
        {
            long origRows = a.Rows;
            long origCols = a.Cols;

            factory.iBLAS.ImatCopy(BLAS_Layout.RowMajor,
                BLAS_Transpose.Trans, a.Rows, a.Cols,
                alpha, a.DPtr, a.Cols, a.Rows);

            a.Rows = origCols;
            a.Cols = origRows;
        }

        /// <summary>
        /// Performs an in-place transpose of a complex-valued matrix, with optional scaling.
        /// </summary>
        /// <param name="a">The complex matrix to transpose (overwritten on exit).</param>
        /// <param name="alpha">Scaling factor applied to all elements during transpose (default is <see cref="Cplx.One"/>).</param>
        public static void Transpose(ref Matx<Cplx> a,
            Cplx alpha = default)
        {
            long origRows = a.Rows;
            long origCols = a.Cols;

            if (alpha == default) { alpha = Cplx.One; }
            factory.iBLAS.ImatCopy(BLAS_Layout.RowMajor,
                BLAS_Transpose.Trans, a.Rows, a.Cols,
                &alpha, a.DPtr, a.Cols, a.Rows);

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
            long origRows = a.Rows;
            long origCols = a.Cols;

            if (alpha == default) { alpha = Cplx.One; }
            factory.iBLAS.ImatCopy(BLAS_Layout.RowMajor,
                BLAS_Transpose.ConjTrans, a.Rows, a.Cols,
                &alpha, a.VPtr, a.Cols, a.Rows);

            a.Rows = origCols;
            a.Cols = origRows;
        }

        #endregion


        // obsolete ...
        #region --------- Dot (Vector-Vector) ---------

        #region real-valued

        /// <summary>
        /// computes a vector-vector dot product
        /// </summary>
        /// <param name="x"> input vector x </param>
        /// <param name="y"> input vector y </param>
        /// <returns> dot product result </returns>
        public static double Dot(VectorD x, VectorD y)
            => factory.iBLAS.DotD(x.Count, x, y);

        #endregion
        #region complex-valued

        /// <summary>
        /// computes a vector-vector dot product
        /// </summary>
        /// <param name="x"> input vector x </param>
        /// <param name="y"> input vector y </param>
        /// <returns> dot product result </returns>
        public static Complex Dot(VectorZ x, VectorZ y)
            => factory.iBLAS.DotZ(x.Count, x, y);

        #endregion

        #endregion
        #region --------- Dot Conjugate (Vector-Vector) ---------

        /// <summary>
        /// computes a dot product of a conjugated vector 
        /// with another vector
        /// </summary>
        /// <param name="x"> input vector x, to be conjugated </param>
        /// <param name="y"> input vector y </param>
        /// <returns> dot product result </returns>
        public static Complex DotConjugate(VectorZ x, VectorZ y)
            => factory.iBLAS.DotcZ(x.Count, x, y);

        #endregion
        #region --------- Dot (Matrix-Vector) ---------

        #region real-valued

        /// <summary>
        /// computes a matrix-vector product
        /// y = alpha*op(a) * x
        /// </summary>
        /// <param name="a"> matrix a </param>
        /// <param name="x"> vector x </param>
        /// <param name="alpha"> scalar constant alpha </param>
        /// <param name="operation"> operation on matrix a </param>
        /// <returns> result vector y </returns>
        public static VectorD Dot(MatrixD a, VectorD x, 
            double alpha = 1.0, BLAS_Transpose operation = BLAS_Transpose.NoTrans)
        {
            long opaCols = (operation == BLAS_Transpose.NoTrans)? a.Cols : a.Rows;
            if (opaCols != x.Count) { throw new ArgumentException($"dimensions of input matrix and vector not match"); }
            long opaRows = (operation == BLAS_Transpose.NoTrans) ? a.Rows : a.Cols;

            double beta = 1.0;
            VectorD y = new(count: opaRows, mode: ArrayInitMode.Calloc);
            factory.iBLAS.GemvD(BLAS_Layout.RowMajor,
                operation,
                a.Rows, a.Cols, alpha, a, a.Cols,
                x, beta, ref y);
            return y;
        }

        /// <summary>
        /// computes a matrix-vector product
        /// y: = alpha * a * x + beta * y
        /// </summary>
        /// <param name="a"> matrix a </param>
        /// <param name="x"> vector x </param>
        /// <param name="y"> vector y (overwritten as output) </param>
        /// <param name="alpha"> scalar parameter alpha </param>
        /// <param name="beta"> scalar parameter beta </param>
        /// <param name="operation"> operation on matrix a </param>
        /// <exception cref="ArgumentException"></exception>
        public static void Dot(MatrixD a, VectorD x, ref VectorD y,
            double alpha = 1.0, double beta = 0.0,
            BLAS_Transpose operation = BLAS_Transpose.NoTrans)
        {
            long opaCols = (operation == BLAS_Transpose.NoTrans) ? a.Cols : a.Rows;
            if (opaCols != x.Count) { throw new ArgumentException($"dimensions of input matrix and vector not match"); }

            factory.iBLAS.GemvD(BLAS_Layout.RowMajor,
                operation,
                a.Rows, a.Cols, alpha, a, a.Cols,
                x, beta, ref y);
        }

        #endregion
        #region complex-valued

        /// <summary>
        /// computes a matrix-vector product
        /// y = alpha*op(a) * x
        /// </summary>
        /// <param name="a"> matrix a </param>
        /// <param name="x"> vector x </param>
        /// <param name="alphaRe"> real-part of scalar constant alpha </param>
        /// <param name="alphaIm"> imaginary-part of scalar constant alpha </param>
        /// <param name="operation"> operation on matrix a </param>
        /// <returns> result vector </returns>
        public static VectorZ Dot(MatrixZ a, VectorZ x,
            double alphaRe = 1.0, double alphaIm = 0.0,
            BLAS_Transpose operation = BLAS_Transpose.NoTrans)
        {
            long opaCols = (operation == BLAS_Transpose.NoTrans) ? a.Cols : a.Rows;
            if (opaCols != x.Count) { throw new ArgumentException($"dimensions of input matrix and vector not match"); }
            long opaRows = (operation == BLAS_Transpose.NoTrans) ? a.Rows : a.Cols;

            Complex alpha = new(alphaRe, alphaIm);
            Complex beta = 1.0;
            VectorZ y = new(count: opaRows, mode: ArrayInitMode.Calloc);
            factory.iBLAS.GemvZ(BLAS_Layout.RowMajor,
                BLAS_Transpose.NoTrans,
                a.Rows, a.Cols, alpha, a, a.Cols,
                x, beta, ref y);
            return y;
        }

        /// <summary>
        /// computes a matrix-vector product
        /// y: = alpha * a * x + beta * y
        /// </summary>
        /// <param name="a"> matrix a </param>
        /// <param name="x"> vector x </param>
        /// <param name="y"> vector y (overwritten as output) </param>
        /// <param name="alphaRe"> real-part of scalar alpha </param>
        /// <param name="alphaIm"> imag-part of scalar alpha </param>
        /// <param name="betaRe"> real-part of scalar beta </param>
        /// <param name="betaIm"> imag-part of scalar beta </param>
        /// <param name="operation"> operation on matrix a </param>
        /// <exception cref="ArgumentException"></exception>
        public static void Dot(MatrixZ a, VectorZ x, ref VectorZ y,
            double alphaRe = 1.0, double alphaIm = 0.0,
            double betaRe = 0.0, double betaIm = 0.0,
            BLAS_Transpose operation = BLAS_Transpose.NoTrans)
        {
            long opaCols = (operation == BLAS_Transpose.NoTrans) ? a.Cols : a.Rows;
            if (opaCols != x.Count) { throw new ArgumentException($"dimensions of input matrix and vector not match"); }

            Complex alpha = new(alphaRe, alphaIm);
            Complex beta = new(betaRe, betaIm);
            factory.iBLAS.GemvZ(BLAS_Layout.RowMajor,
                BLAS_Transpose.NoTrans,
                a.Rows, a.Cols, alpha, a, a.Cols,
                x, beta, ref y);
        }

        #endregion

        #endregion
        #region --------- Dot (Matrix-Matrix) ---------

        #region real-valued

        /// <summary>
        /// computes a matrix-matrix product
        /// c = alpha * op(a) * op(b)
        /// </summary>
        /// <param name="a"> matrix a </param>
        /// <param name="b"> matrix b </param>
        /// <param name="alpha"> scalar constant alpha </param>
        /// <param name="operationA"> operation on matrix a </param>
        /// <param name="operationB"> operation on matrix b </param>
        /// <returns> result matrix c </returns>
        public static MatrixD Dot(MatrixD a, MatrixD b,
            double alpha = 1.0,
            BLAS_Transpose operationA = BLAS_Transpose.NoTrans,
            BLAS_Transpose operationB = BLAS_Transpose.NoTrans)
        {
            long opaCols = (operationA == BLAS_Transpose.NoTrans) ? a.Cols : a.Rows;
            long opaRows = (operationA == BLAS_Transpose.NoTrans) ? a.Rows : a.Cols;
            long opbCols = (operationB == BLAS_Transpose.NoTrans) ? b.Cols : b.Rows;
            long opbRows = (operationB == BLAS_Transpose.NoTrans) ? b.Rows : b.Cols;

            // columns in matrix a must equal to rows in matrix b
            //if (a.Cols != b.Rows) 
            if(opaCols != opbRows)
            { throw new ArgumentException($"dimensions of input matrices not match"); }

            double beta = 1.0;
            MatrixD c = new(rows: opaRows, cols: opbCols, mode: ArrayInitMode.Calloc);
            factory.iBLAS.GemmD(BLAS_Layout.RowMajor,
                operationA, operationB,
                opaRows, opbCols, opaCols, alpha, a, opaCols,
                b, opbCols, beta, ref c, c.Cols);
            return c;
        }

        /// <summary>
        /// computes a matrix-matrix product
        /// c: = alpha * op(a) * op(b) + beta * c
        /// </summary>
        /// <param name="a"> matrix a </param>
        /// <param name="b"> matrix b </param>
        /// <param name="c"> matrix c (overwritten as output) </param>
        /// <param name="alpha"> scalar constant alpha </param>
        /// <param name="beta"> scalar constant beta </param>
        /// <param name="operationA"> operation on matrix a </param>
        /// <param name="operationB"> operation on matrix b </param>
        /// <exception cref="ArithmeticException"></exception>
        public static void Dot(MatrixD a, MatrixD b, ref MatrixD c,
            double alpha = 1.0, double beta = 0.0,
            BLAS_Transpose operationA = BLAS_Transpose.NoTrans,
            BLAS_Transpose operationB = BLAS_Transpose.NoTrans)
        {
            long opaCols = (operationA == BLAS_Transpose.NoTrans)? a.Cols : a.Rows;
            long opaRows = (operationA == BLAS_Transpose.NoTrans)? a.Rows : a.Cols;
            long opbCols = (operationB == BLAS_Transpose.NoTrans)? b.Cols : b.Rows;
            long opbRows = (operationB == BLAS_Transpose.NoTrans)? b.Rows : b.Cols;

            // columns in matrix a must equal to rows in matrix b
            // and, columns and rows in matrix c must match
            //if (a.Cols != b.Rows || c.Rows != a.Rows || c.Cols != b.Cols)
            if(opaCols != opbRows || c.Rows != opaRows || c.Cols != opbCols)
            { throw new ArgumentException($"dimensions of input matrices not match"); }

            factory.iBLAS.GemmD(BLAS_Layout.RowMajor,
                operationA, operationB,
                opaRows, opbCols, opaCols, alpha, a, opaCols,
                b, opbCols, beta, ref c, c.Cols);
        }

        #endregion
        #region complex-valued

        /// <summary>
        /// computes a matrix-matrix product
        /// c = alpha * op(a) * op(b)
        /// </summary>
        /// <param name="a"> matrix a </param>
        /// <param name="b"> matrix b </param>
        /// <param name="alphaRe"> real-part of scalar constant alpha </param>
        /// <param name="alphaIm"> imaginary-part of scalar constant alpha </param>
        /// <param name="operationA"> operation on matrix a </param>
        /// <param name="operationB"> operation on matrix b </param>
        /// <returns> result matrix c </returns>
        public static MatrixZ Dot(MatrixZ a, MatrixZ b,
            double alphaRe = 1.0, double alphaIm = 0.0,
            BLAS_Transpose operationA = BLAS_Transpose.NoTrans,
            BLAS_Transpose operationB = BLAS_Transpose.NoTrans)
        {
            long opaCols = (operationA == BLAS_Transpose.NoTrans) ? a.Cols : a.Rows;
            long opaRows = (operationA == BLAS_Transpose.NoTrans) ? a.Rows : a.Cols;
            long opbCols = (operationB == BLAS_Transpose.NoTrans) ? b.Cols : b.Rows;
            long opbRows = (operationB == BLAS_Transpose.NoTrans) ? b.Rows : b.Cols;

            // columns in matrix a must equal to rows in matrix b
            if (opaCols != opbRows)
            { throw new ArgumentException($"dimensions of input matrices not match"); }

            Complex alpha = new(alphaRe, alphaIm);
            double beta = 1.0;
            MatrixZ c = new(rows: opaRows, cols: opbCols, mode: ArrayInitMode.Calloc);
            factory.iBLAS.GemmZ(BLAS_Layout.RowMajor,
                operationA, operationB,
                opaRows, opbCols, opaCols, alpha, a, opaCols,
                b, opbCols, beta, ref c, c.Cols);
            return c;
        }

        /// <summary>
        /// computes a matrix-matrix product
        /// c: = alpha * op(a) * op(b) + beta * c
        /// </summary>
        /// <param name="a"> matrix a </param>
        /// <param name="b"> matrix b </param>
        /// <param name="c"> matrix c (overwritten as output) </param>
        /// <param name="alphaRe"> real-part of scalar alpha </param>
        /// <param name="alphaIm"> imag-part of scalar alpha </param>
        /// <param name="betaRe"> real-part of scalar beta </param>
        /// <param name="betaIm"> imag-part of scalar beta </param>
        /// <param name="operationA"> operation on matrix a </param>
        /// <param name="operationB"> operation on matrix b </param>
        /// <exception cref="ArithmeticException"></exception>
        public static void Dot(MatrixZ a, MatrixZ b, ref MatrixZ c,
            double alphaRe = 1.0, double alphaIm = 0.0,
            double betaRe = 0.0, double betaIm = 0.0,
            BLAS_Transpose operationA = BLAS_Transpose.NoTrans,
            BLAS_Transpose operationB = BLAS_Transpose.NoTrans)
        {
            long opaCols = (operationA == BLAS_Transpose.NoTrans) ? a.Cols : a.Rows;
            long opaRows = (operationA == BLAS_Transpose.NoTrans) ? a.Rows : a.Cols;
            long opbCols = (operationB == BLAS_Transpose.NoTrans) ? b.Cols : b.Rows;
            long opbRows = (operationB == BLAS_Transpose.NoTrans) ? b.Rows : b.Cols;

            // columns in matrix a must equal to rows in matrix b
            // and, columns and rows in matrix c must match
            if (opaCols != opbRows || c.Rows != opaRows || c.Cols != opbCols)
            { throw new ArgumentException($"dimensions of input matrices not match"); }

            Complex alpha = new(alphaRe, alphaIm);
            Complex beta = new(betaRe, betaIm);
            factory.iBLAS.GemmZ(BLAS_Layout.RowMajor,
                operationA, operationB,
                opaRows, opbCols, opaCols, alpha, a, opaCols,
                b, opbCols, beta, ref c, c.Cols);
        }

        #endregion

        #endregion
        #region --------- Transpose (Matrix) ---------

        #region real-valued

        /// <summary>
        /// performs in-place matrix transpose
        /// </summary>
        /// <param name="a"> input matrix a (overwritten on exit) </param>
        public static void Transpose(ref MatrixD a)
        {
            long origRows = a.Rows;
            long origCols = a.Cols;
            factory.iBLAS.ImatCopyD(BLAS_Layout.RowMajor, BLAS_Transpose.Trans,
                a.Rows, a.Cols, 1.0, ref a, a.Cols, a.Rows);
            a.Rows = origCols;
            a.Cols = origRows;
        }

        /// <summary>
        /// performs out-place matrix transpose
        /// </summary>
        /// <param name="a"> input matrix a </param>
        /// <returns> transposed matrix </returns>
        public static MatrixD Transpose(MatrixD a)
        {
            MatrixD b = new(rows: a.Cols, cols: a.Rows, mode: ArrayInitMode.Malloc);
            factory.iBLAS.OmatCopyD(BLAS_Layout.RowMajor, BLAS_Transpose.Trans,
                a.Rows, a.Cols, 1.0, a, a.Cols, ref b, b.Cols);
            return b;
        }

        #endregion
        #region complex-valued

        /// <summary>
        /// performs in-place matrix transpose
        /// </summary>
        /// <param name="a"> input matrix a (overwritten on exit) </param>
        public static void Transpose(ref MatrixZ a)
        {
            long origRows = a.Rows;
            long origCols = a.Cols;
            factory.iBLAS.ImatCopyZ(BLAS_Layout.RowMajor, BLAS_Transpose.Trans,
                a.Rows, a.Cols, 1.0, ref a, a.Cols, a.Rows);
            a.Rows = origCols;
            a.Cols = origRows;
        }

        /// <summary>
        /// performs in-place matrix conjugate transpose
        /// </summary>
        /// <param name="a"> input matrix a (overwritten on exit) </param>
        public static void ConjugateTranspose(ref MatrixZ a)
        {
            long origRows = a.Rows;
            long origCols = a.Cols;
            factory.iBLAS.ImatCopyZ(BLAS_Layout.RowMajor, BLAS_Transpose.ConjTrans,
                a.Rows, a.Cols, 1.0, ref a, a.Cols, a.Rows);
            a.Rows = origCols;
            a.Cols = origRows;
        }

        /// <summary>
        /// performs out-place matrix transpose
        /// </summary>
        /// <param name="a"> input matrix a </param>
        /// <returns> transposed matrix </returns>
        public static MatrixZ Transpose(MatrixZ a)
        {
            MatrixZ b = new(rows: a.Cols, cols: a.Rows, mode: ArrayInitMode.Malloc);
            factory.iBLAS.OmatCopyZ(BLAS_Layout.RowMajor, BLAS_Transpose.Trans,
                a.Rows, a.Cols, 1.0, a, a.Cols, ref b, b.Cols);
            return b;
        }

        /// <summary>
        /// performs out-place matrix conjugate transpose
        /// </summary>
        /// <param name="a"> input matrix a (overwritten on exit) </param>
        /// <returns> conjugate transposed matrix </returns>
        public static MatrixZ ConjugateTranspose(MatrixZ a)
        {
            MatrixZ b = new(rows: a.Cols, cols: a.Rows, mode: ArrayInitMode.Malloc);
            factory.iBLAS.OmatCopyZ(BLAS_Layout.RowMajor, BLAS_Transpose.ConjTrans,
                a.Rows, a.Cols, 1.0, a, a.Cols, ref b, b.Cols);
            return b;
        }

        #endregion

        #endregion

        #endregion
        #region BLAS-based extension methods

        #region --------- DotWithDiagonal (...) ---------
        // ...
        #endregion
        #region --------- DotWithInverse (...) ---------
        // ...
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
        public static void LUFactorize(ref Matx<Real> a, out Vect<Int> ipiv)
        {
            ipiv = new(count: a.Rows, initMode: ArrayInitMode.Calloc);
            factory.iLAPACK.Getrf(LAPACK_Layout.RowMajor, a.Rows, a.Cols,
                a.DPtr, a.Cols, ipiv.TPtr);
        }

        /// <summary>
        /// Computes the LU factorization of a complex-valued matrix using LAPACK's <c>getrf</c> routine.
        /// The input matrix <paramref name="a"/> is overwritten by its LU decomposition.
        /// </summary>
        /// <param name="a">The input complex matrix to factorize. On exit, contains the combined L and U factors.</param>
        /// <param name="ipiv">Output vector containing the pivot indices. The length is equal to the number of rows in <paramref name="a"/>.</param>
        public static void LUFactorize(ref Matx<Cplx> a, out Vect<Int> ipiv)
        {
            ipiv = new(count: a.Rows, initMode: ArrayInitMode.Calloc);
            factory.iLAPACK.Getrf(LAPACK_Layout.RowMajor, a.Rows, a.Cols,
                a.VPtr, a.Cols, ipiv.TPtr);
        }

        #endregion
        #region ---- Inverse ----

        /// <summary>
        /// Computes the inverse of a real-valued matrix in-place using LAPACK routines.
        /// The input matrix <paramref name="a"/> is overwritten by its inverse.
        /// </summary>
        /// <param name="a">The input matrix to invert. On exit, contains the inverse of the original matrix.</param>
        public static void Inverse(ref Matx<Real> a)
        {
            Vect<Int> ipiv = new(count: a.Rows, initMode: ArrayInitMode.Calloc);
            factory.iLAPACK.Getrf(LAPACK_Layout.RowMajor, a.Rows, a.Cols,
                a.DPtr, a.Cols, ipiv.TPtr);
            factory.iLAPACK.Getri(LAPACK_Layout.RowMajor, a.Cols,
                a.DPtr, a.Cols, ipiv.TPtr);
        }

        /// <summary>
        /// Computes the inverse of a complex-valued matrix in-place using LAPACK routines.
        /// The input matrix <paramref name="a"/> is overwritten by its inverse.
        /// </summary>
        /// <param name="a">The input complex matrix to invert. On exit, contains the inverse of the original matrix.</param>
        public static void Inverse(ref Matx<Cplx> a)
        {
            Vect<Int> ipiv = new(count: a.Rows, initMode: ArrayInitMode.Calloc);
            factory.iLAPACK.Getrf(LAPACK_Layout.RowMajor, a.Rows, a.Cols,
                a.DPtr, a.Cols, ipiv.TPtr);
            factory.iLAPACK.Getri(LAPACK_Layout.RowMajor, a.Cols,
                a.VPtr, a.Cols, ipiv.TPtr);
        }

        #endregion
        #region ---- LinearSolve (Vector) ----

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
        public static void LinearSolve(ref Matx<Real> a,
            ref Vect<Real> b)
        {
            if (a.Cols != b.Count)
            { throw new ArgumentException("matrix size and vector length not match"); }
            Vect<Int> ipiv = new(count: a.Rows, initMode: ArrayInitMode.Calloc);
            factory.iLAPACK.Gesv(LAPACK_Layout.RowMajor, a.Rows, 1,
                a.DPtr, a.Cols, ipiv.TPtr, b.DPtr, 1);
        }

        /// <summary>
        /// Solves a system of linear equations A * x = b for complex-valued matrices and vectors.
        /// The input matrix <paramref name="a"/> is overwritten by its LU factorization,
        /// and the input vector <paramref name="b"/> is overwritten by the solution vector x.
        /// </summary>
        /// <param name="a">The coefficient complex matrix A (overwritten by its LU factorization on exit).
        /// Must be square and have the same number of columns as the length of <paramref name="b"/>.</param>
        /// <param name="b">The right-hand side complex vector b (overwritten by the solution vector x on exit).
        /// The length must match the number of columns in <paramref name="a"/>.</param>
        /// <exception cref="ArgumentException">
        /// Thrown if the number of columns in <paramref name="a"/> does not match the length of <paramref name="b"/>.
        /// </exception>
        public static void LinearSolve(ref Matx<Cplx> a,
            ref Vect<Cplx> b)
        {
            if (a.Cols != b.Count)
            { throw new ArgumentException("matrix size and vector length not match"); }
            Vect<Int> ipiv = new(count: a.Rows, initMode: ArrayInitMode.Calloc);
            factory.iLAPACK.Gesv(LAPACK_Layout.RowMajor, a.Rows, 1,
                a.VPtr, a.Cols, ipiv.TPtr, b.VPtr, 1);
        }

        #endregion
        #region ---- LinearSolve (Matrix) ----

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
        public static void LinearSolve(ref Matx<Real> a,
            ref Matx<Real> b)
        {
            if (a.Cols != b.Rows)
            { throw new ArgumentException("matrix size and vector length not match"); }
            Vect<Int> ipiv = new(count: a.Rows, initMode: ArrayInitMode.Calloc);
            factory.iLAPACK.Gesv(LAPACK_Layout.RowMajor, a.Rows, b.Cols,
                a.DPtr, a.Cols, ipiv.TPtr, b.DPtr, b.Cols);
        }

        /// <summary>
        /// Solves a system of linear equations A * X = B for complex-valued matrices.
        /// The input matrix <paramref name="a"/> is overwritten by its LU factorization,
        /// and the input matrix <paramref name="b"/> is overwritten by the solution matrix X.
        /// </summary>
        /// <param name="a">The coefficient complex matrix A (overwritten by its LU factorization on exit).
        /// Must be square and have the same number of columns as the number of rows in <paramref name="b"/>.</param>
        /// <param name="b">The right-hand side complex matrix B (overwritten by the solution matrix X on exit).
        /// The number of rows must match the number of columns in <paramref name="a"/>.</param>
        /// <exception cref="ArgumentException">
        /// Thrown if the number of columns in <paramref name="a"/> does not match the number of rows in <paramref name="b"/>.
        /// </exception>
        public static void LinearSolve(ref Matx<Cplx> a,
            ref Matx<Cplx> b)
        {
            if (a.Cols != b.Rows)
            { throw new ArgumentException("matrix size and vector length not match"); }
            Vect<Int> ipiv = new(count: a.Rows, initMode: ArrayInitMode.Calloc);
            factory.iLAPACK.Gesv(LAPACK_Layout.RowMajor, a.Rows, b.Cols,
                a.VPtr, a.Cols, ipiv.TPtr, b.VPtr, b.Cols);
        }

        #endregion
        #region ---- LeastSquare (Vector) ----

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
        public static void LeastSquare(ref Matx<Real> a,
            ref Vect<Real> b)
        {
            if (a.Rows != b.Count)
            { throw new ArgumentException("matrix size and vector length not match"); }
            Vect<Int> ipiv = new(count: Math.Min(a.Rows, a.Cols), initMode: ArrayInitMode.Calloc);
            factory.iLAPACK.Gels(LAPACK_Layout.RowMajor, LAPACK_Transpose.NoTrans,
                a.Rows, a.Cols, 1,
                a.DPtr, a.Cols, b.DPtr, 1); // Math.Max(a.Rows, a.Cols));
        }

        /// <summary>
        /// Computes the minimum-norm solution to a complex linear least squares problem using LAPACK's <c>Gels</c> routine.
        /// The input matrix <paramref name="a"/> is overwritten by its QR or LQ factorization,
        /// and the input vector <paramref name="b"/> is overwritten by the solution vector x.
        /// </summary>
        /// <param name="a">The coefficient complex matrix A (overwritten by its QR or LQ factorization on exit).
        /// The number of rows must match the length of <paramref name="b"/>.</param>
        /// <param name="b">The right-hand side complex vector b (overwritten by the solution vector x on exit).
        /// The length must match the number of rows in <paramref name="a"/>.</param>
        /// <exception cref="ArgumentException">
        /// Thrown if the number of rows in <paramref name="a"/> does not match the length of <paramref name="b"/>.
        /// </exception>
        public static void LeastSquare(ref Matx<Cplx> a,
            ref Vect<Cplx> b)
        {
            if (a.Rows != b.Count)
            { throw new ArgumentException("matrix size and vector length not match"); }
            Vect<Int> ipiv = new(count: Math.Min(a.Rows, a.Cols), initMode: ArrayInitMode.Calloc);
            factory.iLAPACK.Gels(LAPACK_Layout.RowMajor, LAPACK_Transpose.NoTrans,
                a.Rows, a.Cols, 1,
                a.VPtr, a.Cols, b.VPtr, 1); // Math.Max(a.Rows, a.Cols));
        }

        #endregion
        #region ---- LeastSquare (Matrix) ----

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
        public static void LeastSquare(ref Matx<Real> a,
            ref Matx<Real> b)
        {
            if (a.Rows != b.Rows)
            { throw new ArgumentException("matrix size and vector length not match"); }
            Vect<Int> ipiv = new(count: Math.Min(a.Rows, a.Cols), initMode: ArrayInitMode.Calloc);
            factory.iLAPACK.Gels(LAPACK_Layout.RowMajor, LAPACK_Transpose.NoTrans,
                a.Rows, a.Cols, b.Cols,
                a.DPtr, a.Cols, b.DPtr, b.Cols); // Math.Max(a.Rows, a.Cols));
        }

        /// <summary>
        /// Computes the minimum-norm solution to a complex linear least squares problem using LAPACK's <c>Gels</c> routine.
        /// The input matrix <paramref name="a"/> is overwritten by its QR or LQ factorization,
        /// and the input matrix <paramref name="b"/> is overwritten by the solution matrix X.
        /// </summary>
        /// <param name="a">The coefficient complex matrix A (overwritten by its QR or LQ factorization on exit).
        /// The number of rows must match the number of rows in <paramref name="b"/>.</param>
        /// <param name="b">The right-hand side complex matrix B (overwritten by the solution matrix X on exit).
        /// The number of rows must match the number of rows in <paramref name="a"/>.</param>
        /// <exception cref="ArgumentException">
        /// Thrown if the number of rows in <paramref name="a"/> does not match the number of rows in <paramref name="b"/>.
        /// </exception>
        public static void LeastSquare(ref Matx<Cplx> a,
            ref Matx<Cplx> b)
        {
            if (a.Rows != b.Rows)
            { throw new ArgumentException("matrix size and vector length not match"); }
            Vect<Int> ipiv = new(count: Math.Min(a.Rows, a.Cols), initMode: ArrayInitMode.Calloc);
            factory.iLAPACK.Gels(LAPACK_Layout.RowMajor, LAPACK_Transpose.NoTrans,
                a.Rows, a.Cols, b.Cols,
                a.VPtr, a.Cols, b.VPtr, b.Cols); // Math.Max(a.Rows, a.Cols));
        }

        #endregion
        #region ---- EigenSystem (Full) ----

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
        public static void EigenSystem(ref Matx<Real> a,
            out Vect<Real> eigenValuesReal, out Vect<Real> eigenValuesImag,
            out Matx<Real> leftEigenVectors, out Matx<Real> rightEigenVectors)
        {
            if (a.Cols != a.Rows)
            { throw new Exception("unequal rows and columns"); }

            eigenValuesReal = new(count: a.Rows, initMode: ArrayInitMode.Calloc);
            eigenValuesImag = new(count: a.Rows, initMode: ArrayInitMode.Calloc);
            leftEigenVectors = new(rows: a.Rows, cols: a.Cols, initMode: ArrayInitMode.Calloc);
            rightEigenVectors = new(rows: a.Rows, cols: a.Cols, initMode: ArrayInitMode.Calloc);

            factory.iLAPACK.Geev(LAPACK_Layout.RowMajor, LAPACK_Job.V, LAPACK_Job.V, a.Cols,
                a.DPtr, a.Cols, eigenValuesReal.DPtr, eigenValuesImag.DPtr,
                leftEigenVectors.DPtr, a.Cols, rightEigenVectors.DPtr, a.Cols);
        }

        /// <summary>
        /// Computes the eigenvalues and eigenvectors of a complex-valued square matrix using LAPACK's <c>Geev</c> routine.
        /// The input matrix <paramref name="a"/> is overwritten by LAPACK during computation.
        /// </summary>
        /// <param name="a">The complex-valued square matrix to analyze. Must be square (same number of rows and columns).
        /// On exit, the contents are overwritten.</param>
        /// <param name="eigenValues">Output vector containing the computed complex eigenvalues. Length equals <c>a.Rows</c>.</param>
        /// <param name="leftEigenVectors">Output matrix containing the left eigenvectors. Dimensions are <c>a.Rows</c> x <c>a.Cols</c>.</param>
        /// <param name="rightEigenVectors">Output matrix containing the right eigenvectors. Dimensions are <c>a.Rows</c> x <c>a.Cols</c>.</param>
        /// <exception cref="Exception">
        /// Thrown if the input matrix <paramref name="a"/> is not square.
        /// </exception>
        public static void EigenSystem(ref Matx<Cplx> a,
            out Vect<Cplx> eigenValues,
            out Matx<Cplx> leftEigenVectors, out Matx<Cplx> rightEigenVectors)
        {
            if (a.Cols != a.Rows)
            { throw new Exception("unequal rows and columns"); }

            eigenValues = new(count: a.Rows, initMode: ArrayInitMode.Calloc);
            leftEigenVectors = new(rows: a.Rows, cols: a.Cols, initMode: ArrayInitMode.Calloc);
            rightEigenVectors = new(rows: a.Rows, cols: a.Cols, initMode: ArrayInitMode.Calloc);

            factory.iLAPACK.Geev(LAPACK_Layout.RowMajor, LAPACK_Job.V, LAPACK_Job.V, a.Cols,
                a.VPtr, a.Cols, eigenValues.VPtr,
                leftEigenVectors.VPtr, a.Cols, rightEigenVectors.VPtr, a.Cols);
        }

        #endregion
        #region ---- EigenSystem (Simp) ----

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
        public static void EigenSystem(ref Matx<Real> a,
            out Vect<Real> eigenValuesReal, out Vect<Real> eigenValuesImag,
            out Matx<Real> eigenVectors)
        {
            if (a.Cols != a.Rows)
            { throw new Exception("unequal rows and columns"); }

            eigenValuesReal = new(count: a.Rows, initMode: ArrayInitMode.Calloc);
            eigenValuesImag = new(count: a.Rows, initMode: ArrayInitMode.Calloc);
            Matx<Real> dummy = new(rows: 1, cols: 1, initMode: ArrayInitMode.Calloc);
            eigenVectors = new(rows: a.Rows, cols: a.Cols, initMode: ArrayInitMode.Calloc);

            factory.iLAPACK.Geev(LAPACK_Layout.RowMajor, LAPACK_Job.N, LAPACK_Job.V, a.Cols,
                a.DPtr, a.Cols, eigenValuesReal.DPtr, eigenValuesImag.DPtr,
                dummy.DPtr, a.Cols, eigenVectors.DPtr, a.Cols);
        }

        /// <summary>
        /// Computes the eigenvalues and right eigenvectors of a complex-valued square matrix using LAPACK's <c>Geev</c> routine.
        /// The input matrix <paramref name="a"/> is overwritten by LAPACK during computation.
        /// </summary>
        /// <param name="a">The complex-valued square matrix to analyze. Must be square (same number of rows and columns).
        /// On exit, the contents are overwritten.</param>
        /// <param name="eigenValues">Output vector containing the computed complex eigenvalues. Length equals <c>a.Rows</c>.</param>
        /// <param name="eigenVectors">Output matrix containing the right eigenvectors. Dimensions are <c>a.Rows</c> x <c>a.Cols</c>.</param>
        /// <exception cref="Exception">
        /// Thrown if the input matrix <paramref name="a"/> is not square.
        /// </exception>
        public static void EigenSystem(ref Matx<Cplx> a,
            out Vect<Cplx> eigenValues,
            out Matx<Cplx> eigenVectors)
        {
            if (a.Cols != a.Rows)
            { throw new Exception("unequal rows and columns"); }

            eigenValues = new(count: a.Rows, initMode: ArrayInitMode.Calloc);
            Matx<Cplx> dummy = new(rows: 1, cols: 1, initMode: ArrayInitMode.Calloc);
            eigenVectors = new(rows: a.Rows, cols: a.Cols, initMode: ArrayInitMode.Calloc);

            factory.iLAPACK.Geev(LAPACK_Layout.RowMajor, LAPACK_Job.N, LAPACK_Job.V, a.Cols,
                a.VPtr, a.Cols, eigenValues.VPtr,
                dummy.VPtr, a.Cols, eigenVectors.VPtr, a.Cols);
        }

        #endregion
        #region ---- EigenSystem (Lite) ----

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
        public static void EigenSystem(ref Matx<Real> a,
            out Vect<Real> eigenValuesReal, out Vect<Real> eigenValuesImag)
        {
            if (a.Cols != a.Rows)
            { throw new Exception("unequal rows and columns"); }

            eigenValuesReal = new(count: a.Rows, initMode: ArrayInitMode.Calloc);
            eigenValuesImag = new(count: a.Rows, initMode: ArrayInitMode.Calloc);
            Matx<Real> dummyL = new(rows: 1, cols: 1, initMode: ArrayInitMode.Calloc);
            Matx<Real> dummyR = new(rows: 1, cols: 1, initMode: ArrayInitMode.Calloc);

            factory.iLAPACK.Geev(LAPACK_Layout.RowMajor, LAPACK_Job.N, LAPACK_Job.N, a.Cols,
                a.DPtr, a.Cols, eigenValuesReal.DPtr, eigenValuesImag.DPtr,
                dummyL.DPtr, a.Cols, dummyR.DPtr, a.Cols);
        }

        /// <summary>
        /// Computes the eigenvalues of a complex-valued square matrix using LAPACK's <c>Geev</c> routine.
        /// The input matrix <paramref name="a"/> is overwritten by LAPACK during computation.
        /// Only eigenvalues are computed; eigenvectors are not returned.
        /// </summary>
        /// <param name="a">The complex-valued square matrix to analyze. Must be square (same number of rows and columns).
        /// On exit, the contents are overwritten.</param>
        /// <param name="eigenValues">Output vector containing the computed complex eigenvalues. Length equals <c>a.Rows</c>.</param>
        /// <exception cref="Exception">
        /// Thrown if the input matrix <paramref name="a"/> is not square.
        /// </exception>
        public static void EigenSystem(ref Matx<Cplx> a,
            out Vect<Cplx> eigenValues)
        {
            if (a.Cols != a.Rows)
            { throw new Exception("unequal rows and columns"); }

            eigenValues = new(count: a.Rows, initMode: ArrayInitMode.Calloc);
            Matx<Cplx> dummyL = new(rows: 1, cols: 1, initMode: ArrayInitMode.Calloc);
            Matx<Cplx> dummyR = new(rows: 1, cols: 1, initMode: ArrayInitMode.Calloc);

            factory.iLAPACK.Geev(LAPACK_Layout.RowMajor, LAPACK_Job.N, LAPACK_Job.N, a.Cols,
                a.VPtr, a.Cols, eigenValues.VPtr,
                dummyL.VPtr, a.Cols, dummyR.VPtr, a.Cols);
        }

        #endregion
        #region ---- GeneralizedEigenSystem (Full) ----

        /// <summary>
        /// Computes the generalized eigenvalues and eigenvectors for a pair of real-valued square matrices using LAPACK's <c>Ggev</c> routine.
        /// The input matrices <paramref name="a"/> and <paramref name="b"/> are overwritten during computation.
        /// </summary>
        /// <param name="a">The first real-valued square matrix (overwritten on exit). Must be square.</param>
        /// <param name="b">The second real-valued square matrix (overwritten on exit). Must be square.</param>
        /// <param name="alphaReal">Output vector containing the real parts of the generalized eigenvalues (numerator). Length equals <c>a.Rows</c>.</param>
        /// <param name="alphaImag">Output vector containing the imaginary parts of the generalized eigenvalues (numerator). Length equals <c>a.Rows</c>.</param>
        /// <param name="beta">Output vector containing the denominators of the generalized eigenvalues. Length equals <c>a.Rows</c>.</param>
        /// <param name="leftEigenVectors">Output matrix containing the left eigenvectors. Dimensions are <c>a.Rows</c> x <c>a.Cols</c>.</param>
        /// <param name="rightEigenVectors">Output matrix containing the right eigenvectors. Dimensions are <c>a.Rows</c> x <c>a.Cols</c>.</param>
        /// <exception cref="Exception">
        /// Thrown if either <paramref name="a"/> or <paramref name="b"/> is not square.
        /// </exception>
        public static void GeneralizedEigenSystem(ref Matx<Real> a, ref Matx<Real> b,
            out Vect<Real> alphaReal, out Vect<Real> alphaImag, out Vect<Real> beta,
            out Matx<Real> leftEigenVectors, out Matx<Real> rightEigenVectors)
        {
            if (a.Cols != a.Rows) { throw new Exception("unequal rows and columns in A"); }
            if (b.Cols != b.Rows) { throw new Exception("unequal rows and columns in B"); }

            alphaReal = new(count: a.Rows, initMode: ArrayInitMode.Calloc);
            alphaImag = new(count: a.Rows, initMode: ArrayInitMode.Calloc);
            beta = new(count: a.Rows, initMode: ArrayInitMode.Calloc);
            leftEigenVectors = new(rows: a.Rows, cols: a.Cols, initMode: ArrayInitMode.Calloc);
            rightEigenVectors = new(rows: a.Rows, cols: a.Cols, initMode: ArrayInitMode.Calloc);

            factory.iLAPACK.Ggev(LAPACK_Layout.RowMajor, LAPACK_Job.V, LAPACK_Job.V, a.Cols,
                a.DPtr, a.Cols, b.DPtr, b.Cols,
                alphaReal.DPtr, alphaImag.DPtr, beta.DPtr,
                leftEigenVectors.DPtr, a.Cols, rightEigenVectors.DPtr, a.Cols);
        }

        /// <summary>
        /// Computes the generalized eigenvalues and eigenvectors for a pair of complex-valued square matrices using LAPACK's <c>Ggev</c> routine.
        /// The input matrices <paramref name="a"/> and <paramref name="b"/> are overwritten during computation.
        /// </summary>
        /// <param name="a">The first complex-valued square matrix (overwritten on exit). Must be square.</param>
        /// <param name="b">The second complex-valued square matrix (overwritten on exit). Must be square.</param>
        /// <param name="alpha">Output vector containing the generalized eigenvalues (numerator, complex). Length equals <c>a.Rows</c>.</param>
        /// <param name="beta">Output vector containing the denominators of the generalized eigenvalues (complex). Length equals <c>a.Rows</c>.</param>
        /// <param name="leftEigenVectors">Output matrix containing the left eigenvectors. Dimensions are <c>a.Rows</c> x <c>a.Cols</c>.</param>
        /// <param name="rightEigenVectors">Output matrix containing the right eigenvectors. Dimensions are <c>a.Rows</c> x <c>a.Cols</c>.</param>
        /// <exception cref="Exception">
        /// Thrown if either <paramref name="a"/> or <paramref name="b"/> is not square.
        /// </exception>
        public static void GeneralizedEigenSystem(ref Matx<Cplx> a, ref Matx<Cplx> b,
            out Vect<Cplx> alpha, out Vect<Cplx> beta,
            out Matx<Cplx> leftEigenVectors, out Matx<Cplx> rightEigenVectors)
        {
            if (a.Cols != a.Rows) { throw new Exception("unequal rows and columns in A"); }
            if (b.Cols != b.Rows) { throw new Exception("unequal rows and columns in B"); }

            alpha = new(count: a.Rows, initMode: ArrayInitMode.Calloc);
            beta = new(count: a.Rows, initMode: ArrayInitMode.Calloc);
            leftEigenVectors = new(rows: a.Rows, cols: a.Cols, initMode: ArrayInitMode.Calloc);
            rightEigenVectors = new(rows: a.Rows, cols: a.Cols, initMode: ArrayInitMode.Calloc);

            factory.iLAPACK.Ggev(LAPACK_Layout.RowMajor, LAPACK_Job.V, LAPACK_Job.V, a.Cols,
                a.VPtr, a.Cols, b.VPtr, b.Cols,
                alpha.VPtr, beta.VPtr,
                leftEigenVectors.VPtr, a.Cols, rightEigenVectors.VPtr, a.Cols);
        }

        #endregion
        #region ---- GeneralizedEigenSystem (Simp) ----

        /// <summary>
        /// Computes the generalized eigenvalues and right eigenvectors for a pair of real-valued square matrices using LAPACK's <c>Ggev</c> routine.
        /// The input matrices <paramref name="a"/> and <paramref name="b"/> are overwritten during computation.
        /// Only right eigenvectors are computed; left eigenvectors are not returned.
        /// </summary>
        /// <param name="a">The first real-valued square matrix (overwritten on exit). Must be square.</param>
        /// <param name="b">The second real-valued square matrix (overwritten on exit). Must be square.</param>
        /// <param name="alphaReal">Output vector containing the real parts of the generalized eigenvalues (numerator). Length equals <c>a.Rows</c>.</param>
        /// <param name="alphaImag">Output vector containing the imaginary parts of the generalized eigenvalues (numerator). Length equals <c>a.Rows</c>.</param>
        /// <param name="beta">Output vector containing the denominators of the generalized eigenvalues. Length equals <c>a.Rows</c>.</param>
        /// <param name="eigenVectors">Output matrix containing the right eigenvectors. Dimensions are <c>a.Rows</c> x <c>a.Cols</c>.</param>
        /// <exception cref="Exception">
        /// Thrown if either <paramref name="a"/> or <paramref name="b"/> is not square.
        /// </exception>
        public static void GeneralizedEigenSystem(ref Matx<Real> a, ref Matx<Real> b,
            out Vect<Real> alphaReal, out Vect<Real> alphaImag, out Vect<Real> beta,
            out Matx<Real> eigenVectors)
        {
            if (a.Cols != a.Rows) { throw new Exception("unequal rows and columns in A"); }
            if (b.Cols != b.Rows) { throw new Exception("unequal rows and columns in B"); }

            alphaReal = new(count: a.Rows, initMode: ArrayInitMode.Calloc);
            alphaImag = new(count: a.Rows, initMode: ArrayInitMode.Calloc);
            beta = new(count: a.Rows, initMode: ArrayInitMode.Calloc);
            Matx<Real> dummy = new(rows: 1, cols: 1, initMode: ArrayInitMode.Calloc);
            eigenVectors = new(rows: a.Rows, cols: a.Cols, initMode: ArrayInitMode.Calloc);

            factory.iLAPACK.Ggev(LAPACK_Layout.RowMajor, LAPACK_Job.N, LAPACK_Job.V, a.Cols,
                a.DPtr, a.Cols, b.DPtr, b.Cols,
                alphaReal.DPtr, alphaImag.DPtr, beta.DPtr,
                dummy.DPtr, a.Cols, eigenVectors.DPtr, a.Cols);
        }

        /// <summary>
        /// Computes the generalized eigenvalues and right eigenvectors for a pair of complex-valued square matrices using LAPACK's <c>Ggev</c> routine.
        /// The input matrices <paramref name="a"/> and <paramref name="b"/> are overwritten during computation.
        /// Only right eigenvectors are computed; left eigenvectors are not returned.
        /// </summary>
        /// <param name="a">The first complex-valued square matrix (overwritten on exit). Must be square.</param>
        /// <param name="b">The second complex-valued square matrix (overwritten on exit). Must be square.</param>
        /// <param name="alpha">Output vector containing the generalized eigenvalues (numerator, complex). Length equals <c>a.Rows</c>.</param>
        /// <param name="beta">Output vector containing the denominators of the generalized eigenvalues (complex). Length equals <c>a.Rows</c>.</param>
        /// <param name="eigenVectors">Output matrix containing the right eigenvectors. Dimensions are <c>a.Rows</c> x <c>a.Cols</c>.</param>
        /// <exception cref="Exception">
        /// Thrown if either <paramref name="a"/> or <paramref name="b"/> is not square.
        /// </exception>
        public static void GeneralizedEigenSystem(ref Matx<Cplx> a, ref Matx<Cplx> b,
            out Vect<Cplx> alpha, out Vect<Cplx> beta,
            out Matx<Cplx> eigenVectors)
        {
            if (a.Cols != a.Rows) { throw new Exception("unequal rows and columns in A"); }
            if (b.Cols != b.Rows) { throw new Exception("unequal rows and columns in B"); }

            alpha = new(count: a.Rows, initMode: ArrayInitMode.Calloc);
            beta = new(count: a.Rows, initMode: ArrayInitMode.Calloc);
            Matx<Cplx> dummy = new(rows: 1, cols: 1, initMode: ArrayInitMode.Calloc);
            eigenVectors = new(rows: a.Rows, cols: a.Cols, initMode: ArrayInitMode.Calloc);

            factory.iLAPACK.Ggev(LAPACK_Layout.RowMajor, LAPACK_Job.N, LAPACK_Job.V, a.Cols,
                a.VPtr, a.Cols, b.VPtr, b.Cols,
                alpha.VPtr, beta.VPtr,
                dummy.VPtr, a.Cols, eigenVectors.VPtr, a.Cols);
        }

        #endregion
        #region ---- GeneralizedEigenSystem (Lite) ----

        /// <summary>
        /// Computes the generalized eigenvalues for a pair of real-valued square matrices using LAPACK's <c>Ggev</c> routine.
        /// The input matrices <paramref name="a"/> and <paramref name="b"/> are overwritten during computation.
        /// Only eigenvalues are computed; eigenvectors are not returned.
        /// </summary>
        /// <param name="a">The first real-valued square matrix (overwritten on exit). Must be square.</param>
        /// <param name="b">The second real-valued square matrix (overwritten on exit). Must be square.</param>
        /// <param name="alphaReal">Output vector containing the real parts of the generalized eigenvalues (numerator). Length equals <c>a.Rows</c>.</param>
        /// <param name="alphaImag">Output vector containing the imaginary parts of the generalized eigenvalues (numerator). Length equals <c>a.Rows</c>.</param>
        /// <param name="beta">Output vector containing the denominators of the generalized eigenvalues. Length equals <c>a.Rows</c>.</param>
        /// <exception cref="Exception">
        /// Thrown if either <paramref name="a"/> or <paramref name="b"/> is not square.
        /// </exception>
        public static void GeneralizedEigenSystem(ref Matx<Real> a, ref Matx<Real> b,
            out Vect<Real> alphaReal, out Vect<Real> alphaImag, out Vect<Real> beta)
        {
            if (a.Cols != a.Rows) { throw new Exception("unequal rows and columns in A"); }
            if (b.Cols != b.Rows) { throw new Exception("unequal rows and columns in B"); }

            alphaReal = new(count: a.Rows, initMode: ArrayInitMode.Calloc);
            alphaImag = new(count: a.Rows, initMode: ArrayInitMode.Calloc);
            beta = new(count: a.Rows, initMode: ArrayInitMode.Calloc);
            Matx<Real> dummyL = new(rows: 1, cols: 1, initMode: ArrayInitMode.Calloc);
            Matx<Real> dummyR = new(rows: 1, cols: 1, initMode: ArrayInitMode.Calloc);

            factory.iLAPACK.Ggev(LAPACK_Layout.RowMajor, LAPACK_Job.N, LAPACK_Job.V, a.Cols,
                a.DPtr, a.Cols, b.DPtr, b.Cols,
                alphaReal.DPtr, alphaImag.DPtr, beta.DPtr,
                dummyL.DPtr, a.Cols, dummyR.DPtr, a.Cols);
        }

        /// <summary>
        /// Computes the generalized eigenvalues for a pair of complex-valued square matrices using LAPACK's <c>Ggev</c> routine.
        /// The input matrices <paramref name="a"/> and <paramref name="b"/> are overwritten during computation.
        /// Only eigenvalues are computed; eigenvectors are not returned.
        /// </summary>
        /// <param name="a">The first complex-valued square matrix (overwritten on exit). Must be square.</param>
        /// <param name="b">The second complex-valued square matrix (overwritten on exit). Must be square.</param>
        /// <param name="alpha">Output vector containing the generalized eigenvalues (numerator, complex). Length equals <c>a.Rows</c>.</param>
        /// <param name="beta">Output vector containing the denominators of the generalized eigenvalues (complex). Length equals <c>a.Rows</c>.</param>
        /// <exception cref="Exception">
        /// Thrown if either <paramref name="a"/> or <paramref name="b"/> is not square.
        /// </exception>
        public static void GeneralizedEigenSystem(ref Matx<Cplx> a, ref Matx<Cplx> b,
            out Vect<Cplx> alpha, out Vect<Cplx> beta)
        {
            if (a.Cols != a.Rows) { throw new Exception("unequal rows and columns in A"); }
            if (b.Cols != b.Rows) { throw new Exception("unequal rows and columns in B"); }

            alpha = new(count: a.Rows, initMode: ArrayInitMode.Calloc);
            beta = new(count: a.Rows, initMode: ArrayInitMode.Calloc);
            Matx<Cplx> dummyL = new(rows: 1, cols: 1, initMode: ArrayInitMode.Calloc);
            Matx<Cplx> dummyR = new(rows: 1, cols: 1, initMode: ArrayInitMode.Calloc);

            factory.iLAPACK.Ggev(LAPACK_Layout.RowMajor, LAPACK_Job.N, LAPACK_Job.V, a.Cols,
                a.VPtr, a.Cols, b.VPtr, b.Cols,
                alpha.VPtr, beta.VPtr,
                dummyL.VPtr, a.Cols, dummyR.VPtr, a.Cols);
        }

        #endregion
        #region ---- SVD ----

        /// <summary>
        /// Computes the singular value decomposition (SVD) of a real-valued matrix.
        /// Decomposes the input matrix <paramref name="a"/> into U, S, and VT such that a = U * S * VT.
        /// The input matrix <paramref name="a"/> is overwritten during computation.
        /// </summary>
        /// <param name="a">The input real-valued matrix to decompose. On exit, contents are overwritten.</param>
        /// <param name="s">Output vector containing the singular values, sorted in decreasing order. Length is min(a.Rows, a.Cols).</param>
        /// <param name="u">Output matrix containing the left singular vectors. Dimensions are a.Rows x a.Rows.</param>
        /// <param name="vt">Output matrix containing the right singular vectors (transposed). Dimensions are a.Cols x a.Cols.</param>
        public static void SVD(ref Matx<Real> a,
            out Vect<Real> s,
            out Matx<Real> u,
            out Matx<Real> vt)
        {
            s = new(count: Math.Min(a.Rows, a.Cols), initMode: ArrayInitMode.Calloc);
            u = new(rows: a.Rows, cols: a.Rows, initMode: ArrayInitMode.Calloc);
            vt = new(rows: a.Cols, cols: a.Cols, initMode: ArrayInitMode.Calloc);
            Vect<Real> superb = new(count: Math.Min(a.Rows, a.Cols), initMode: ArrayInitMode.Calloc);

            factory.iLAPACK.Gesvd(LAPACK_Layout.RowMajor, LAPACK_Job.A, LAPACK_Job.A,
                a.Rows, a.Cols, a.DPtr, a.Cols, s.DPtr, u.DPtr, a.Rows, vt.DPtr, a.Cols, superb.DPtr);
        }

        /// <summary>
        /// Computes the singular value decomposition (SVD) of a complex-valued matrix.
        /// Decomposes the input matrix <paramref name="a"/> into U, S, and VT such that a = U * S * VT.
        /// The input matrix <paramref name="a"/> is overwritten during computation.
        /// </summary>
        /// <param name="a">The input complex-valued matrix to decompose. On exit, contents are overwritten.</param>
        /// <param name="s">Output vector containing the singular values, sorted in decreasing order. Length is min(a.Rows, a.Cols).</param>
        /// <param name="u">Output matrix containing the left singular vectors. Dimensions are a.Rows x a.Rows.</param>
        /// <param name="vt">Output matrix containing the right singular vectors (conjugate transposed). Dimensions are a.Cols x a.Cols.</param>
        public static void SVD(ref Matx<Cplx> a,
            out Vect<Real> s,
            out Matx<Cplx> u,
            out Matx<Cplx> vt)
        {
            s = new(count: Math.Min(a.Rows, a.Cols), initMode: ArrayInitMode.Calloc);
            u = new(rows: a.Rows, cols: a.Rows, initMode: ArrayInitMode.Calloc);
            vt = new(rows: a.Cols, cols: a.Cols, initMode: ArrayInitMode.Calloc);
            Vect<Real> superb = new(count: Math.Min(a.Rows, a.Cols), initMode: ArrayInitMode.Calloc);

            factory.iLAPACK.Gesvd(LAPACK_Layout.RowMajor, LAPACK_Job.A, LAPACK_Job.A,
                a.Rows, a.Cols, a.VPtr, a.Cols, s.DPtr, u.VPtr, a.Rows, vt.VPtr, a.Cols, superb.DPtr);
        }

        #endregion


        // obsolete ...
        #region --------- LU ---------

        #region real-valued

        /// <summary>
        /// performs LU factorization with partial pivoting
        /// </summary>
        /// <param name="a"> input matrix </param>
        /// <param name="ipiv"> pivot indices </param>
        public static void LUFactorize(ref MatrixD a, out VectorI ipiv)
        {
            ipiv = new(a.Rows);
            factory.iLAPACK.Getrf(LAPACK_Layout.RowMajor, a.Rows, a.Cols, 
                a.SPtr, a.Cols, ipiv.TPtr);
        }

        #endregion
        #region complex-valued

        /// <summary>
        /// performs LU factorization with partial pivoting
        /// </summary>
        /// <param name="a"> input matrix </param>
        /// <param name="ipiv"> pivot indices </param>
        public static void LUFactorize(ref MatrixZ a, out VectorI ipiv)
        {
            ipiv = new(a.Rows);
            factory.iLAPACK.Getrf(LAPACK_Layout.RowMajor, a.Rows, a.Cols, 
                a.SPtr, a.Cols, ipiv.TPtr);
        }

        #endregion

        #endregion
        #region --------- Inverse ---------

        #region real-valued

        /// <summary>
        /// inverses a matrix 
        /// </summary>
        /// <param name="a"> matrix a (overwritten on exit by its inverse) </param>
        public static void Inverse(ref MatrixD a)
        {
            VectorI ipiv = new(a.Rows); //new(a.Rows);
            factory.iLAPACK.Getrf(LAPACK_Layout.RowMajor, a.Rows, a.Cols, 
                a.SPtr, a.Cols, ipiv.TPtr);
            factory.iLAPACK.Getri(LAPACK_Layout.RowMajor, a.Cols, 
                a.SPtr, a.Cols, ipiv.TPtr);
        }

        /// <summary>
        /// inverses a matrix 
        /// </summary>
        /// <param name="a"> matrix a (overwritten on exit by its inverse) </param>
        /// <returns> inversed matrix </returns>
        public static MatrixD Inverse(MatrixD a)
        {
            MatrixD b = new(a.Rows, a.Cols);
            factory.iBLAS.CopyD(b.Count, a, ref b);
            Inverse(ref b);
            return b;
        }

        #endregion
        #region complex-valued

        /// <summary>
        /// inverses a matrix 
        /// </summary>
        /// <param name="a"> matrix a (overwritten on exit by its inverse) </param>
        public static void Inverse(ref MatrixZ a)
        {
            VectorI ipiv = new(a.Rows); //new(a.Rows);
            factory.iLAPACK.Getrf(LAPACK_Layout.RowMajor, a.Rows, a.Cols, 
                a.SPtr, a.Cols, ipiv.TPtr);
            factory.iLAPACK.Getri(LAPACK_Layout.RowMajor, a.Cols, 
                a.SPtr, a.Cols, ipiv.TPtr);
        }

        /// <summary>
        /// inverses a matrix 
        /// </summary>
        /// <param name="a"> matrix a (overwritten on exit by its inverse) </param>
        /// <returns> inversed matrix </returns>
        public static MatrixZ Inverse(MatrixZ a)
        {
            MatrixZ b = new(a.Rows, a.Cols);
            factory.iBLAS.CopyZ(b.Count, a, ref b);
            Inverse(ref b);
            return b;
        }

        #endregion

        #endregion
        #region --------- LinearSolve (Vector) ---------

        #region real-valued

        /// <summary>
        /// solves a linear equation system
        /// a * x = b, with x beging the unknowns
        /// </summary>
        /// <param name="a"> matrix a (overwritten on exit by the factor L and U) </param>
        /// <param name="b"> vector b (overwritten on exit by the solution vector x)</param>
        public static void LinearSolve(ref MatrixD a,
            ref VectorD b)
        {
            if (a.Cols != b.Count)
            { throw new ArgumentException("matrix size and vector length not match"); }

            VectorI ipiv = new(a.Rows); //new(a.Rows);
            factory.iLAPACK.Gesv(LAPACK_Layout.RowMajor, a.Rows, 1, 
                a.SPtr, a.Cols, ipiv.TPtr, b.SPtr, 1);
        }

        /// <summary>
        /// solves a linear equation system
        /// a * x = b, with x beging the unknowns
        /// </summary>
        /// <param name="a"> matrix a </param>
        /// <param name="b"> vector b </param>
        /// <returns> solution vector x </returns>
        public static VectorD LinearSolve(MatrixD a, VectorD b)
        {
            if (a.Cols != b.Count)
            { throw new ArgumentException("matrix size and vector length not match"); }
            
            MatrixD t = new(a.Rows, a.Cols);
            VectorD x = new(b.Count);
            factory.iBLAS.CopyD(a.Count, a, ref t);
            factory.iBLAS.CopyD(b.Count, b, ref x);
            LinearSolve(ref t, ref x);
            return x;
        }

        #endregion
        #region complex-valued   
        
        /// <summary>
        /// solves a linear equation system
        /// a * x = b, with x beging the unknowns
        /// </summary>
        /// <param name="a"> matrix a (overwritten on exit by the factor L and U) </param>
        /// <param name="b"> vector b (overwritten on exit by the solution vector x)</param>
        public static void LinearSolve(ref MatrixZ a,
            ref VectorZ b)
        {
            if (a.Cols != b.Count)
            { throw new ArgumentException("matrix size and vector length not match"); }
            
            VectorI ipiv = new(a.Rows); //new(a.Rows);
            factory.iLAPACK.Gesv(LAPACK_Layout.RowMajor, a.Rows, 1, 
                a.SPtr, a.Cols, ipiv.TPtr, b.SPtr, 1);
        }

        /// <summary>
        /// solves a linear equation system
        /// a * x = b, with x beging the unknowns
        /// </summary>
        /// <param name="a"> matrix a </param>
        /// <param name="b"> vector b </param>
        /// <returns> solution vector x </returns>
        public static VectorZ LinearSolve(MatrixZ a, VectorZ b)
        {
            if (a.Cols != b.Count)
            { throw new ArgumentException("matrix size and vector length not match"); }
            
            MatrixZ t = new(a.Rows, a.Cols);
            VectorZ x = new(b.Count);
            factory.iBLAS.CopyZ(a.Count, a, ref t);
            factory.iBLAS.CopyZ(b.Count, b, ref x);
            LinearSolve(ref t, ref x);
            return x;
        }

        #endregion

        #endregion
        #region --------- LinearSolve (Matrix) ---------

        #region real-valued

        /// <summary>
        /// solves a linear equation system
        /// a * x = b, with x beging the unknowns
        /// </summary>
        /// <param name="a"> matrix a (overwritten on exit by the factor L and U) </param>
        /// <param name="b"> matrix b (overwritten on exit by the solution matrix x)</param>
        public static void LinearSolve(ref MatrixD a,
            ref MatrixD b)
        {
            if (a.Cols != b.Rows)
            { throw new ArgumentException("matrix size and vector length not match"); }
            
            VectorI ipiv = new(a.Rows); //new(a.Rows);
            factory.iLAPACK.Gesv(LAPACK_Layout.RowMajor, a.Rows, b.Cols, 
                a.SPtr, a.Cols, ipiv.TPtr, b.SPtr, b.Cols);
        }

        /// <summary>
        /// solves a linear equation system
        /// a * x = b, with x beging the unknowns
        /// </summary>
        /// <param name="a"> matrix a </param>
        /// <param name="b"> matrix b </param>
        /// <returns> solution matrix x </returns>
        public static MatrixD LinearSolve(MatrixD a, MatrixD b)
        {
            if (a.Cols != b.Rows)
            { throw new ArgumentException("matrix size and vector length not match"); }

            MatrixD t = new(a.Rows, a.Cols);
            MatrixD x = new(b.Rows, b.Cols);
            factory.iBLAS.CopyD(a.Count, a, ref t);
            factory.iBLAS.CopyD(b.Count, b, ref x);
            LinearSolve(ref t, ref x);
            return x;
        }

        #endregion
        #region complex-valued   
        
        /// <summary>
        /// solves a linear equation system
        /// a * x = b, with x beging the unknowns
        /// </summary>
        /// <param name="a"> matrix a (overwritten on exit by the factor L and U) </param>
        /// <param name="b"> matrix b (overwritten on exit by the solution matrix x)</param>
        public static void LinearSolve(ref MatrixZ a,
            ref MatrixZ b)
        {
            if (a.Cols != b.Rows)
            { throw new ArgumentException("matrix size and vector length not match"); }
            
            VectorI ipiv = new(a.Rows); //new(a.Rows);
            factory.iLAPACK.Gesv(LAPACK_Layout.RowMajor, a.Rows, b.Cols, 
                a.SPtr, a.Cols, ipiv.TPtr, b.SPtr, b.Cols);
        }

        /// <summary>
        /// solves a linear equation system
        /// a * x = b, with x beging the unknowns
        /// </summary>
        /// <param name="a"> matrix a </param>
        /// <param name="b"> matrix b </param>
        /// <returns> solution matrix x </returns>
        public static MatrixZ LinearSolve(MatrixZ a, MatrixZ b)
        {
            if (a.Cols != b.Rows)
            { throw new ArgumentException("matrix size and vector length not match"); }
            
            MatrixZ t = new(a.Rows, a.Cols);
            MatrixZ x = new(b.Rows, b.Cols);
            factory.iBLAS.CopyZ(a.Count, a, ref t);
            factory.iBLAS.CopyZ(b.Count, b, ref x);
            LinearSolve(ref t, ref x);
            return x;
        }

        #endregion

        #endregion
        #region --------- LeastSquare (Vector) ---------

        #region real-valued

        /// <summary>
        /// solves a lin`ear least square problem
        /// either overdetermined => minimize || b - a * x ||_2
        /// or, underdetermined == a * x = b
        /// </summary>
        /// <param name="a"> matrix a (overwritten by the factorization data) </param>
        /// <param name="b"> vector b (overwritten by the solution vector x) </param>
        public static void LeastSquare(ref MatrixD a,
            ref VectorD b)
            => factory.iLAPACK.Gels(LAPACK_Layout.RowMajor, LAPACK_Transpose.NoTrans,
                a.Rows, a.Cols, 1, a.SPtr, a.Cols, b.SPtr, 1);

        /// <summary>
        /// solves a linear least square problem
        /// either overdetermined => minimize || b - a * x ||_2
        /// or, underdetermined == a * x = b
        /// </summary>
        /// <param name="a"> matrix a </param>
        /// <param name="b"> vector b </param>
        /// <returns> result vector x </returns>
        public static VectorD LeastSquare(MatrixD a, VectorD b)
        {
            MatrixD t = new(a.Rows, a.Cols);
            VectorD x = new(b.Count);
            factory.iBLAS.CopyD(a.Count, a, ref t);
            factory.iBLAS.CopyD(b.Count, b, ref x);
            LeastSquare(ref t, ref x);
            return x[new LongRange(0, a.Cols)];
        }

        #endregion
        #region complex-valued

        /// <summary>
        /// solves a linear least square problem
        /// either overdetermined => minimize || b - a * x ||_2
        /// or, underdetermined == a * x = b
        /// </summary>
        /// <param name="a"> matrix a (overwritten by the factorization data) </param>
        /// <param name="b"> vector b (overwritten by the solution vector x) </param>
        public static void LeastSquare(ref MatrixZ a,
            ref VectorZ b)
            => factory.iLAPACK.Gels(LAPACK_Layout.RowMajor, LAPACK_Transpose.NoTrans,
                a.Rows, a.Cols, 1, a.SPtr, a.Cols, b.SPtr, 1);

        /// <summary>
        /// solves a linear least square problem
        /// either overdetermined => minimize || b - a * x ||_2
        /// or, underdetermined == a * x = b
        /// </summary>
        /// <param name="a"> matrix a </param>
        /// <param name="b"> vector b </param>
        /// <returns> result vector x </returns>
        public static VectorZ LeastSquare(MatrixZ a, VectorZ b)
        {
            MatrixZ t = new(a.Rows, a.Cols);
            VectorZ x = new(b.Count);
            factory.iBLAS.CopyZ(a.Count, a, ref t);
            factory.iBLAS.CopyZ(b.Count, b, ref x);
            LeastSquare(ref t, ref x);
            return x;
        }

        #endregion

        #endregion
        #region --------- LeastSquare (Matrix) ---------

        #region real-valued

        /// <summary>
        /// solves a linear least square problem
        /// either overdetermined => minimize || b - a * x ||_2
        /// or, underdetermined == a * x = b
        /// </summary>
        /// <param name="a"> matrix a (overwritten by the factorization data) </param>
        /// <param name="b"> vector b (overwritten by the solution vector b) </param>
        public static void LeastSquare(ref MatrixD a,
            ref MatrixD b)
            => factory.iLAPACK.Gels(LAPACK_Layout.RowMajor, LAPACK_Transpose.NoTrans,
                a.Rows, a.Cols, b.Cols, a.SPtr, a.Cols, b.SPtr, b.Cols);

        /// <summary>
        /// solves a linear least square problem
        /// either overdetermined => minimize || b - a * x ||_2
        /// or, underdetermined == a * x = b
        /// </summary>
        /// <param name="a"> matrix a </param>
        /// <param name="b"> matrix b </param>
        /// <returns> result matrix x </returns>
        public static MatrixD LeastSquare(MatrixD a, MatrixD b)
        {
            MatrixD t = new(a.Rows, a.Cols);
            MatrixD x = new(b.Rows, b.Cols);
            factory.iBLAS.CopyD(a.Count, a, ref t);
            factory.iBLAS.CopyD(b.Count, b, ref x);
            LeastSquare(ref t, ref x);
            return x[new LongRange(0, a.Cols), new LongRange(0, b.Cols)];
        }

        #endregion
        #region complex-valued

        /// <summary>
        /// solves a linear least square problem
        /// either overdetermined => minimize || b - a * x ||_2
        /// or, underdetermined == a * x = b
        /// </summary>
        /// <param name="a"> matrix a (overwritten by the factorization data) </param>
        /// <param name="b"> vector b (overwritten by the solution vector b) </param>
        public static void LeastSquare(ref MatrixZ a,
            ref MatrixZ b)
            => factory.iLAPACK.Gels(LAPACK_Layout.RowMajor, LAPACK_Transpose.NoTrans,
                a.Rows, a.Cols, b.Cols, a.SPtr, a.Cols, b.SPtr, b.Cols);

        /// <summary>
        /// solves a linear least square problem
        /// either overdetermined => minimize || b - a * x ||_2
        /// or, underdetermined == a * x = b
        /// </summary>
        /// <param name="a"> matrix a </param>
        /// <param name="b"> matrix b </param>
        /// <returns> result matrix x </returns>
        public static MatrixZ LeastSquare(MatrixZ a, MatrixZ b)
        {
            MatrixZ t = new(a.Rows, a.Cols);
            MatrixZ x = new(b.Rows, b.Cols);
            factory.iBLAS.CopyZ(a.Count, a, ref t);
            factory.iBLAS.CopyZ(b.Count, b, ref x);
            LeastSquare(ref t, ref x);
            return x[new LongRange(0, a.Cols), new LongRange(0, b.Cols)];
        }

        #endregion

        #endregion
        #region --------- EigenSystem ---------

        #region real-valued

        /// <summary>
        /// solves an eigen system for square matrix a
        /// </summary>
        /// <param name="a"> square matrix a (overwritten on exit) </param>
        /// <param name="eigenValues"> eigenvalues </param>
        /// <param name="leftEigenVectors"> left eigenvectors </param>
        /// <param name="rightEigenVectors"> right eigenvectors </param>
        public static void EigenSystem(ref MatrixD a,
            out VectorZ eigenValues,
            out MatrixD leftEigenVectors, out MatrixD rightEigenVectors)
        {
            if (a.Cols != a.Rows)
            { throw new Exception("unequal rows and columns"); }

            VectorD wr = new(a.Rows);
            VectorD wi = new(a.Rows);
            leftEigenVectors = new(a.Rows, a.Cols);
            rightEigenVectors = new(a.Rows, a.Cols);

            factory.iLAPACK.Geev(LAPACK_Layout.RowMajor, LAPACK_Job.V, LAPACK_Job.V, a.Cols,
                a.SPtr, a.Cols, wr.SPtr, wi.SPtr, 
                leftEigenVectors.SPtr, a.Cols, rightEigenVectors.SPtr, a.Cols);
            eigenValues = VMath.Construct(wr, wi);
            //wr.Dispose();
            //wi.Dispose();
        }

        /// <summary>
        /// solves an eigen system for square matrix a
        /// </summary>
        /// <param name="a"> square matrix a (overwritten on exit) </param>
        /// <param name="eigenValues"> eigenvalues </param>
        /// <param name="eigenVectors"> (right) eigenvectors </param>
        public static void EigenSystem(ref MatrixD a,
            out VectorZ eigenValues,
            out MatrixD eigenVectors)
        {
            if (a.Cols != a.Rows)
            { throw new Exception("unequal rows and columns"); }

            VectorD wr = new(a.Rows);
            VectorD wi = new(a.Rows);
            MatrixD dummy = new(1, 1); //new(a.Rows, a.Cols);
            eigenVectors = new(a.Rows, a.Cols);

            factory.iLAPACK.Geev(LAPACK_Layout.RowMajor, LAPACK_Job.N, LAPACK_Job.V, a.Cols,
                a.SPtr, a.Cols, wr.SPtr, wi.SPtr, 
                dummy.SPtr, a.Cols, eigenVectors.SPtr, a.Cols);
            eigenValues = VMath.Construct(wr, wi); //, eigenValues);
            //wr.Dispose();
            //wi.Dispose();
            //vl.Dispose();
        }

        /// <summary>
        /// solves an eigen system for square matrix a
        /// calculate eigenvalues only
        /// </summary>
        /// <param name="a"> square matrix a (overwritten on exit) </param>
        /// <returns> eigenvalues in a vector </returns>
        public static VectorZ EigenSystem(ref MatrixD a)
        {
            if (a.Cols != a.Rows)
            { throw new Exception("unequal rows and columns"); }

            VectorD wr = new(a.Rows);
            VectorD wi = new(a.Rows);
            MatrixD dummy = new(1, 1); // new(a.Rows, a.Cols);

            factory.iLAPACK.Geev(LAPACK_Layout.RowMajor, LAPACK_Job.N, LAPACK_Job.N, a.Cols,
                a.SPtr, a.Cols, wr.SPtr, wi.SPtr, 
                dummy.SPtr, a.Cols, dummy.SPtr, a.Cols);

            return VMath.Construct(wr, wi);
            //VectorComplex eigenValues = new(a.Rows);
            //Construct(wr, wi, eigenValues);
            //wr.Dispose();
            //wi.Dispose();
            //vl.Dispose();
            //vr.Dispose();
            //return eigenValues;
        }

        #endregion
        #region complex-valued

        /// <summary>
        /// solves an eigen system for square matrix a
        /// </summary>
        /// <param name="a"> square matrix a (overwritten on exit) </param>
        /// <param name="eigenValues"> eigenvalues </param>
        /// <param name="leftEigenVectors"> left eigenvectors </param>
        /// <param name="rightEigenVectors"> right eigenvectors </param>
        public static void EigenSystem(ref MatrixZ a,
            out VectorZ eigenValues,
            out MatrixZ leftEigenVectors, out MatrixZ rightEigenVectors)
        {
            if (a.Cols != a.Rows)
            { throw new Exception("unequal rows and columns"); }

            eigenValues = new(a.Rows);
            leftEigenVectors = new(a.Rows, a.Cols);
            rightEigenVectors = new(a.Rows, a.Cols);

            factory.iLAPACK.Geev(LAPACK_Layout.RowMajor, LAPACK_Job.V, LAPACK_Job.V, a.Cols,
                a.SPtr, a.Cols, eigenValues.SPtr, 
                leftEigenVectors.SPtr, a.Cols, rightEigenVectors.SPtr, a.Cols);
        }

        /// <summary>
        /// solves an eigen system for square matrix a
        /// </summary>
        /// <param name="a"> square matrix a (overwritten on exit) </param>
        /// <param name="eigenValues"> eigenvalues </param>
        /// <param name="eigenVectors"> (right) eigenvectors </param>
        public static void EigenSystem(ref MatrixZ a,
            out VectorZ eigenValues,
            out MatrixZ eigenVectors)
        {
            if (a.Cols != a.Rows)
            { throw new Exception("unequal rows and columns"); }

            eigenValues = new(a.Rows, 0.0);
            MatrixZ dummy = new(1, 1); // new(a.Rows, a.Cols);
            eigenVectors = new(a.Rows, a.Cols, 0.0);

            factory.iLAPACK.Geev(LAPACK_Layout.RowMajor, LAPACK_Job.N, LAPACK_Job.V, a.Cols,
                a.SPtr, a.Cols, eigenValues.SPtr, 
                dummy.SPtr, a.Cols, eigenVectors.SPtr, a.Cols);
            //vl.Dispose();
        }

        /// <summary>
        /// solves an eigen system for square matrix a
        /// calculate eigenvalues only
        /// </summary>
        /// <param name="a"> square matrix a (overwritten on exit) </param>
        /// <returns> eigenvalues in a vector </returns>
        public static VectorZ EigenSystem(ref MatrixZ a)
        {
            if (a.Cols != a.Rows)
            { throw new Exception("unequal rows and columns"); }

            VectorZ w = new(a.Rows);
            MatrixZ dummy = new(1, 1); // new(a.Rows, a.Cols);

            factory.iLAPACK.Geev(LAPACK_Layout.RowMajor, LAPACK_Job.N, LAPACK_Job.N, a.Cols,
                a.SPtr, a.Cols, w.SPtr, 
                dummy.SPtr, a.Cols, dummy.SPtr, a.Cols);
            //vl.Dispose();
            //vr.Dispose();
            return w;
        }

        #endregion

        #endregion
        #region --------- GeneralizedEigenSystem ---------

        #region real-valued

        /// <summary>
        /// solves a generalized eigen system for square matrix a and b
        /// </summary>
        /// <param name="a"> square matrix a (overwritten on exit) </param>
        /// <param name="b"> square matrix b (overwritten on exit) </param>
        /// <param name="eigenValues"> eigenvalues </param>
        /// <param name="leftEigenVectors"> left eigenvectors </param>
        /// <param name="rightEigenVectors"> right eigenvectors </param>
        public static void GeneralizedEigenSystem(ref MatrixD a, ref MatrixD b,
            out VectorZ eigenValues,
            out MatrixD leftEigenVectors, out MatrixD rightEigenVectors)
        {
            if (a.Cols != a.Rows)
            { throw new Exception("unequal rows and columns in A"); }
            if (b.Cols != b.Rows)
            { throw new Exception("unequal rows and columns in B"); }

            VectorD wr = new(a.Rows);
            VectorD wi = new(a.Rows);
            VectorD alphar = new(a.Rows);
            VectorD alphai = new(a.Rows);
            VectorD beta = new(a.Rows);
            leftEigenVectors = new(a.Rows, a.Cols);
            rightEigenVectors = new(a.Rows, a.Cols);

            factory.iLAPACK.Ggev(LAPACK_Layout.RowMajor, LAPACK_Job.V, LAPACK_Job.V, a.Cols,
                a.SPtr, a.Cols, b.SPtr, b.Cols, 
                alphar.SPtr, alphai.SPtr, beta.SPtr, 
                leftEigenVectors.SPtr, a.Cols, rightEigenVectors.SPtr, a.Cols);
            wr = alphar / beta;
            wi = alphai / beta;
            eigenValues = VMath.Construct(wr, wi);
            //wr.Dispose();
            //wi.Dispose();
        }

        /// <summary>
        /// solves a generalized eigen system for square matrix a and b
        /// </summary>
        /// <param name="a"> square matrix a (overwritten on exit) </param>
        /// <param name="b"> square matrix b (overwritten on exit) </param>
        /// <param name="alpha"> part of the eigenvalues => alpha/beta = eigenvalue </param>
        /// <param name="beta"> part of the eigenvalues => alpha/beta = eigenvalue </param>
        /// <param name="leftEigenVectors"> left eigenvectors </param>
        /// <param name="rightEigenVectors"> right eigenvectors </param>
        public static void GeneralizedEigenSystem(ref MatrixD a, ref MatrixD b,
            out VectorZ alpha, out VectorD beta,
            out MatrixD leftEigenVectors, out MatrixD rightEigenVectors)
        {
            if (a.Cols != a.Rows)
            { throw new Exception("unequal rows and columns in A"); }
            if (b.Cols != b.Rows)
            { throw new Exception("unequal rows and columns in B"); }

            VectorD alphar = new(a.Rows);
            VectorD alphai = new(a.Rows);
            beta = new(a.Rows);
            leftEigenVectors = new(a.Rows, a.Cols);
            rightEigenVectors = new(a.Rows, a.Cols);

            factory.iLAPACK.Ggev(LAPACK_Layout.RowMajor, LAPACK_Job.V, LAPACK_Job.V, a.Cols,
                a.SPtr, a.Cols, b.SPtr, b.Cols, 
                alphar.SPtr, alphai.SPtr, beta.SPtr, 
                leftEigenVectors.SPtr, a.Cols, rightEigenVectors.SPtr, a.Cols);

            alpha = VMath.Construct(alphar, alphai);
        }

        /// <summary>
        /// solves a generalized eigen system for square matrix a and b
        /// </summary>
        /// <param name="a"> square matrix a (overwritten on exit) </param>
        /// <param name="b"> square matrix b (overwritten on exit) </param>
        /// <param name="eigenValues"> eigenvalues </param>
        /// <param name="eigenVectors"> (right) eigenvectors </param>
        public static void GeneralizedEigenSystem(ref MatrixD a, ref MatrixD b,
            out VectorZ eigenValues,
            out MatrixD eigenVectors)
        {
            if (a.Cols != a.Rows)
            { throw new Exception("unequal rows and columns in A"); }
            if (b.Cols != b.Rows)
            { throw new Exception("unequal rows and columns in B"); }

            VectorD wr = new(a.Rows);
            VectorD wi = new(a.Rows);
            VectorD alphar = new(a.Rows);
            VectorD alphai = new(a.Rows);
            VectorD beta = new(a.Rows);
            MatrixD dummy = new(1, 1); //new(a.Rows, a.Cols);
            eigenVectors = new(a.Rows, a.Cols);

            factory.iLAPACK.Ggev(LAPACK_Layout.RowMajor, LAPACK_Job.N, LAPACK_Job.V, a.Cols,
                a.SPtr, a.Cols, b.SPtr, b.Cols, 
                alphar.SPtr, alphai.SPtr, beta.SPtr, 
                dummy.SPtr, a.Cols, eigenVectors.SPtr, a.Cols);
            wr = alphar / beta;
            wi = alphai / beta;
            eigenValues = VMath.Construct(wr, wi); //, eigenValues);
            //wr.Dispose();
            //wi.Dispose();
            //vl.Dispose();
        }

        /// <summary>
        /// solves a generalized eigen system for square matrix a and b
        /// </summary>
        /// <param name="a"> square matrix a (overwritten on exit) </param>
        /// <param name="b"> square matrix b (overwritten on exit) </param>
        /// <param name="alpha"> part of the eigenvalues => alpha/beta = eigenvalue </param>
        /// <param name="beta"> part of the eigenvalues => alpha/beta = eigenvalue </param>
        /// <param name="eigenVectors"> (right) eigenvectors </param>
        public static void GeneralizedEigenSystem(ref MatrixD a, ref MatrixD b,
            out VectorZ alpha, out VectorD beta,
            out MatrixD eigenVectors)
        {
            if (a.Cols != a.Rows)
            { throw new Exception("unequal rows and columns in A"); }
            if (b.Cols != b.Rows)
            { throw new Exception("unequal rows and columns in B"); }

            VectorD alphar = new(a.Rows);
            VectorD alphai = new(a.Rows);
            beta = new(a.Rows);
            MatrixD dummy = new(1, 1); //new(a.Rows, a.Cols);
            eigenVectors = new(a.Rows, a.Cols);

            factory.iLAPACK.Ggev(LAPACK_Layout.RowMajor, LAPACK_Job.N, LAPACK_Job.V, a.Cols,
                a.SPtr, a.Cols, b.SPtr, b.Cols, 
                alphar.SPtr, alphai.SPtr, beta.SPtr, 
                dummy.SPtr, a.Cols, eigenVectors.SPtr, a.Cols);

            alpha = VMath.Construct(alphar, alphai); //, eigenValues);
        }

        /// <summary>
        /// solves a generalized eigen system for square matrix a and b
        /// calculate eigenvalues only
        /// </summary>
        /// <param name="a"> square matrix a (overwritten on exit) </param>
        /// <param name="b"> square matrix b (overwritten on exit) </param>
        /// <returns> eigenvalues in a vector </returns>
        public static VectorZ GeneralizedEigenSystem(ref MatrixD a, ref MatrixD b)
        {
            if (a.Cols != a.Rows)
            { throw new Exception("unequal rows and columns in A"); }
            if (b.Cols != b.Rows)
            { throw new Exception("unequal rows and columns in B"); }

            VectorD wr = new(a.Rows);
            VectorD wi = new(a.Rows);
            VectorD alphar = new(a.Rows);
            VectorD alphai = new(a.Rows);
            VectorD beta = new(a.Rows);
            MatrixD dummy = new(1, 1); // new(a.Rows, a.Cols);

            factory.iLAPACK.Ggev(LAPACK_Layout.RowMajor, LAPACK_Job.N, LAPACK_Job.N, a.Cols,
                a.SPtr, a.Cols, b.SPtr, b.Cols, 
                alphar.SPtr, alphai.SPtr, beta.SPtr, 
                dummy.SPtr, a.Cols, dummy.SPtr, a.Cols);
            wr = alphar / beta;
            wi = alphai / beta;
            return VMath.Construct(wr, wi);
            //VectorComplex eigenValues = new(a.Rows);
            //Construct(wr, wi, eigenValues);
            //wr.Dispose();
            //wi.Dispose();
            //vl.Dispose();
            //vr.Dispose();
            //return eigenValues;
        }

        /// <summary>
        /// solves a generalized eigen system for square matrix a and b
        /// calculate eigenvalues only
        /// </summary>
        /// <param name="a"> square matrix a (overwritten on exit) </param>
        /// <param name="b"> square matrix b (overwritten on exit) </param>
        /// <param name="alpha"> part of the eigenvalues => alpha/beta = eigenvalue </param>
        /// <param name="beta"> part of the eigenvalues => alpha/beta = eigenvalue </param>
        public static void GeneralizedEigenSystem(ref MatrixD a, ref MatrixD b,
            out VectorZ alpha, out VectorD beta)
        {
            if (a.Cols != a.Rows)
            { throw new Exception("unequal rows and columns in A"); }
            if (b.Cols != b.Rows)
            { throw new Exception("unequal rows and columns in B"); }

            VectorD wr = new(a.Rows);
            VectorD wi = new(a.Rows);
            VectorD alphar = new(a.Rows);
            VectorD alphai = new(a.Rows);
            beta = new(a.Rows);
            MatrixD dummy = new(1, 1); // new(a.Rows, a.Cols);

            factory.iLAPACK.Ggev(LAPACK_Layout.RowMajor, LAPACK_Job.N, LAPACK_Job.N, a.Cols,
                a.SPtr, a.Cols, b.SPtr, b.Cols, 
                alphar.SPtr, alphai.SPtr, beta.SPtr, 
                dummy.SPtr, a.Cols, dummy.SPtr, a.Cols);

            alpha = VMath.Construct(alphar, alphai);
        }
        #endregion
        #region complex-valued

        /// <summary>
        /// solves a generalized eigen system for square matrix a and b
        /// </summary>
        /// <param name="a"> square matrix a (overwritten on exit) </param>
        /// <param name="b"> square matrix b (overwritten on exit) </param>
        /// <param name="eigenValues"> eigenvalues </param>
        /// <param name="leftEigenVectors"> left eigenvectors </param>
        /// <param name="rightEigenVectors"> right eigenvectors </param>
        public static void GeneralizedEigenSystem(ref MatrixZ a, ref MatrixZ b,
            out VectorZ eigenValues,
            out MatrixZ leftEigenVectors, out MatrixZ rightEigenVectors)
        {
            if (a.Cols != a.Rows)
            { throw new Exception("unequal rows and columns in A"); }
            if (b.Cols != b.Rows)
            { throw new Exception("unequal rows and columns in B"); }

            VectorZ alpha = new(a.Rows);
            VectorZ beta = new(a.Rows);
            leftEigenVectors = new(a.Rows, a.Cols);
            rightEigenVectors = new(a.Rows, a.Cols);

            factory.iLAPACK.Ggev(LAPACK_Layout.RowMajor, LAPACK_Job.V, LAPACK_Job.V, a.Cols,
                a.SPtr, a.Cols, b.SPtr, b.Cols, 
                alpha.SPtr, beta.SPtr, 
                leftEigenVectors.SPtr, a.Cols, rightEigenVectors.SPtr, a.Cols);
            eigenValues = alpha / beta;
        }

        /// <summary>
        /// solves a generalized eigen system for square matrix a and b
        /// </summary>
        /// <param name="a"> square matrix a (overwritten on exit) </param>
        /// <param name="b"> square matrix b (overwritten on exit) </param>
        /// <param name="alpha"> part of the eigenvalues => alpha/beta = eigenvalue </param>
        /// <param name="beta"> part of the eigenvalues => alpha/beta = eigenvalue </param>
        /// <param name="leftEigenVectors"> left eigenvectors </param>
        /// <param name="rightEigenVectors"> right eigenvectors </param>
        public static void GeneralizedEigenSystem(ref MatrixZ a, ref MatrixZ b,
            out VectorZ alpha, out VectorZ beta,
            out MatrixZ leftEigenVectors, out MatrixZ rightEigenVectors)
        {
            if (a.Cols != a.Rows)
            { throw new Exception("unequal rows and columns in A"); }
            if (b.Cols != b.Rows)
            { throw new Exception("unequal rows and columns in B"); }

            alpha = new(a.Rows);
            beta = new(a.Rows);
            leftEigenVectors = new(a.Rows, a.Cols);
            rightEigenVectors = new(a.Rows, a.Cols);

            factory.iLAPACK.Ggev(LAPACK_Layout.RowMajor, LAPACK_Job.V, LAPACK_Job.V, a.Cols,
                a.SPtr, a.Cols, b.SPtr, b.Cols, 
                alpha.SPtr, beta.SPtr, 
                leftEigenVectors.SPtr, a.Cols, rightEigenVectors.SPtr, a.Cols);
            }

        /// <summary>
        /// solves a generalized eigen system for square matrix a and b
        /// </summary>
        /// <param name="a"> square matrix a (overwritten on exit) </param>
        /// <param name="b"> square matrix b (overwritten on exit) </param>
        /// <param name="eigenValues"> eigenvalues </param>
        /// <param name="eigenVectors"> (right) eigenvectors </param>
        public static void GeneralizedEigenSystem(ref MatrixZ a, ref MatrixZ b,
            out VectorZ eigenValues,
            out MatrixZ eigenVectors)
        {
            if (a.Cols != a.Rows)
            { throw new Exception("unequal rows and columns in A"); }
            if (b.Cols != b.Rows)
            { throw new Exception("unequal rows and columns in B"); }

            VectorZ alpha = new(a.Rows); //eigenValues = new(a.Rows, 0.0);
            VectorZ beta = new(a.Rows);
            MatrixZ dummy = new(1, 1); // new(a.Rows, a.Cols);
            eigenVectors = new(a.Rows, a.Cols); //eigenVectors = new(a.Rows, a.Cols, 0.0);

            factory.iLAPACK.Ggev(LAPACK_Layout.RowMajor, LAPACK_Job.N, LAPACK_Job.V, a.Cols,
                a.SPtr, a.Cols, b.SPtr, b.Cols, 
                alpha.SPtr, beta.SPtr, 
                dummy.SPtr, a.Cols, eigenVectors.SPtr, a.Cols);
            eigenValues = alpha / beta;
            //vl.Dispose();
        }

        /// <summary>
        /// solves a generalized eigen system for square matrix a and b
        /// </summary>
        /// <param name="a"> square matrix a (overwritten on exit) </param>
        /// <param name="b"> square matrix b (overwritten on exit) </param>
        /// <param name="alpha"> part of the eigenvalues => alpha/beta = eigenvalue </param>
        /// <param name="beta"> part of the eigenvalues => alpha/beta = eigenvalue </param>
        /// <param name="eigenVectors"> (right) eigenvectors </param>
        public static void GeneralizedEigenSystem(ref MatrixZ a, ref MatrixZ b,
            out VectorZ alpha, out VectorZ beta,
            out MatrixZ eigenVectors)
        {
            if (a.Cols != a.Rows)
            { throw new Exception("unequal rows and columns in A"); }
            if (b.Cols != b.Rows)
            { throw new Exception("unequal rows and columns in B"); }

            alpha = new(a.Rows); //eigenValues = new(a.Rows, 0.0);
            beta = new(a.Rows);
            MatrixZ dummy = new(1, 1); // new(a.Rows, a.Cols);
            eigenVectors = new(a.Rows, a.Cols); //eigenVectors = new(a.Rows, a.Cols, 0.0);

            factory.iLAPACK.Ggev(LAPACK_Layout.RowMajor, LAPACK_Job.N, LAPACK_Job.V, a.Cols,
                a.SPtr, a.Cols, b.SPtr, b.Cols, 
                alpha.SPtr, beta.SPtr, 
                dummy.SPtr, a.Cols, eigenVectors.SPtr, a.Cols);
        }

        /// <summary>
        /// solves a Generalized eigen system for square matrix a and b
        /// calculate eigenvalues only
        /// </summary>
        /// <param name="a"> square matrix a (overwritten on exit) </param>
        /// <param name="b"> square matrix b (overwritten on exit) </param>
        /// <returns> eigenvalues in a vector </returns>
        public static VectorZ GeneralizedEigenSystem(ref MatrixZ a, ref MatrixZ b)
        {
            if (a.Cols != a.Rows)
            { throw new Exception("unequal rows and columns in A"); }
            if (b.Cols != b.Rows)
            { throw new Exception("unequal rows and columns in B"); }

            VectorZ eigenValues = new(a.Rows);
            VectorZ alpha = new(a.Rows);
            VectorZ beta = new(a.Rows);
            MatrixZ dummy = new(1, 1); // new(a.Rows, a.Cols);

            factory.iLAPACK.Ggev(LAPACK_Layout.RowMajor, LAPACK_Job.N, LAPACK_Job.N, a.Cols,
                a.SPtr, a.Cols, b.SPtr, b.Cols, 
                alpha.SPtr, beta.SPtr, 
                dummy.SPtr, a.Cols, dummy.SPtr, a.Cols);
            eigenValues = alpha / beta;
            //vl.Dispose();
            //vr.Dispose();
            return eigenValues;
        }

        /// <summary>
        /// solves a Generalized eigen system for square matrix a and b
        /// calculate eigenvalues only
        /// </summary>
        /// <param name="a"> square matrix a (overwritten on exit) </param>
        /// <param name="b"> square matrix b (overwritten on exit) </param>
        /// <param name="alpha"> part of the eigenvalues => alpha/beta = eigenvalue </param>
        /// <param name="beta"> part of the eigenvalues => alpha/beta = eigenvalue </param>
        public static void GeneralizedEigenSystem(ref MatrixZ a, ref MatrixZ b,
            out VectorZ alpha, out VectorZ beta)
        {
            if (a.Cols != a.Rows)
            { throw new Exception("unequal rows and columns in A"); }
            if (b.Cols != b.Rows)
            { throw new Exception("unequal rows and columns in B"); }
                        
            alpha = new(a.Rows);
            beta = new(a.Rows);
            MatrixZ dummy = new(1, 1); // new(a.Rows, a.Cols);

            factory.iLAPACK.Ggev(LAPACK_Layout.RowMajor, LAPACK_Job.N, LAPACK_Job.N, a.Cols,
                a.SPtr, a.Cols, b.SPtr, b.Cols, 
                alpha.SPtr, beta.SPtr, 
                dummy.SPtr, a.Cols, dummy.SPtr, a.Cols);
        }

        #endregion

        #endregion
        #region --------- SVD ---------

        #region real-valued

        /// <summary>
        /// computes the singular value decomposition of a 
        /// general rectangular matrix
        /// A = U*Σ*VT
        /// </summary>
        /// <param name="a"> general rectangular matrix a </param>
        /// <param name="s"> singular values, sorted so that s[i] >= s[i+1] </param>
        /// <param name="u"> left singular value vector of matrix a </param>
        /// <param name="vt"> (transposed) right singular value vector of matrix a </param>
        public static void SVDecompose(MatrixD a, 
            out VectorD s, out MatrixD u, out MatrixD vt)
        {
            long m = a.Rows;
            long n = a.Cols;

            s = new(count: Math.Min(m, n));
            u = new(rows: m, cols: m);
            vt = new(rows: n, cols: n);
            VectorD superb = new(count: Math.Min(m, n));

            MatrixD t = new(other: a, deepCopy: true);
            factory.iLAPACK.Gesvd(LAPACK_Layout.RowMajor, LAPACK_Job.A, LAPACK_Job.A,
                m, n, t.SPtr, n, s.SPtr, u.SPtr, m, vt.SPtr, n, superb.SPtr);
        }

        /// <summary>
        /// computes the singular value decomposition of a 
        /// general rectangular matrix
        /// A = U*Σ*VT
        /// </summary>
        /// <param name="a"> general rectangular matrix a </param>
        /// <returns> singular values, sorted so that s[i] >= s[i+1] </returns>
        public static VectorD SVDecompose(MatrixD a)
        {
            long m = a.Rows;
            long n = a.Cols;

            VectorD s = new(count: Math.Min(m, n));
            MatrixD u = new(rows: m, cols: m);
            MatrixD vt = new(rows: n, cols: n);
            VectorD superb = new(count: Math.Min(m, n));

            MatrixD t = new(other: a, deepCopy: true);
            factory.iLAPACK.Gesvd(LAPACK_Layout.RowMajor, LAPACK_Job.N, LAPACK_Job.N,
                m, n, t.SPtr, n, s.SPtr, u.SPtr, m, vt.SPtr, n, superb.SPtr);
            return s;
        }

        #endregion
        #region complex-valued

        /// <summary>
        /// computes the singular value decomposition of a 
        /// general rectangular matrix
        /// A = U*Σ*VT
        /// </summary>
        /// <param name="a"> general rectangular matrix a </param>
        /// <param name="s"> singular values, sorted so that s[i] >= s[i+1] </param>
        /// <param name="u"> left singular value vector of matrix a </param>
        /// <param name="vt"> (conjugate transposed) right singular value vector of matrix a </param>
        public static void SVDecompose(MatrixZ a,
            out VectorD s, out MatrixZ u, out MatrixZ vt)
        {
            long m = a.Rows;
            long n = a.Cols;

            s = new(count: Math.Min(m, n));
            u = new(rows: m, cols: m);
            vt = new(rows: n, cols: n);
            VectorD superb = new(count: Math.Min(m, n));

            VectorZ t = new(other: a, deepCopy: true);
            factory.iLAPACK.Gesvd(LAPACK_Layout.RowMajor, LAPACK_Job.A, LAPACK_Job.A,
                m, n, t.SPtr, n, s.SPtr, u.SPtr, m, vt.SPtr, n, superb.SPtr);
        }

        /// <summary>
        /// computes the singular value decomposition of a 
        /// general rectangular matrix
        /// A = U*Σ*VT
        /// </summary>
        /// <param name="a"> general rectangular matrix a </param>
        /// <returns> singular values, sorted so that s[i] >= s[i+1] </returns>
        public static VectorD SVDecompose(MatrixZ a)
        {
            long m = a.Rows;
            long n = a.Cols;

            VectorD s = new(count: Math.Min(m, n));
            MatrixZ u = new(rows: m, cols: m);
            MatrixZ vt = new(rows: n, cols: n);
            VectorD superb = new(count: Math.Min(m, n));

            MatrixZ t = new(other: a, deepCopy: true);
            factory.iLAPACK.Gesvd(LAPACK_Layout.RowMajor, LAPACK_Job.N, LAPACK_Job.N,
                m, n, t.SPtr, n, s.SPtr, u.SPtr, m, vt.SPtr, n, superb.SPtr);
            return s;
        }

        #endregion

        #endregion
        #region --------- Pseuduo Inverse ---------

        /// <summary>
        /// performs pseudo inverse of matrix a
        /// </summary>
        /// <param name="a"> input matrix a </param>
        /// <returns> pseudo inverse of a </returns>
        [Obsolete]
        public static MatrixD PseudoInverse(MatrixD a)
        {
            #region SVD
            long m = a.Rows;
            long n = a.Cols;

            VectorD s = new(n);
            MatrixD u = new(m, m);
            MatrixD vt = new(n, n);
            VectorD superb = new(Math.Min(m, n));

            MatrixD c = new(a, true);
            factory.iLAPACK.Gesvd(LAPACK_Layout.RowMajor, LAPACK_Job.S, LAPACK_Job.S,
                m, n, c.SPtr, n, s.SPtr, u.SPtr, m, vt.SPtr, n, superb.SPtr);
            #endregion

            // inverse of s
            VectorD invS = VMath.Inv(s);
            // u = u * inv(S)
            DiagonalMatrixHelper.Dot(ref u, invS, checkLength: false);
            // (vt)T * (u)T
            MatrixD b = new(vt.Cols, u.Rows);
            factory.iBLAS.GemmD(BLAS_Layout.RowMajor, BLAS_Transpose.Trans, BLAS_Transpose.Trans,
                vt.Cols, u.Rows, vt.Rows, 1.0, vt, vt.Rows, u, u.Rows, 1.0, ref b, b.Cols);
            return b;
        }

        /// <summary>
        /// performs pseudo inverse of matrix a
        /// </summary>
        /// <param name="a"> input matrix a </param>
        /// <returns> pseudo inverse of a </returns>
        [Obsolete]
        public static MatrixZ PseudoInverse(MatrixZ a)
        {
            #region SVD
            long m = a.Rows;
            long n = a.Cols;

            VectorD s = new(n);
            MatrixZ u = new(m, m);
            MatrixZ vt = new(n, n);
            VectorD superb = new(Math.Min(m, n));

            MatrixZ c = new(a, true);
            factory.iLAPACK.Gesvd(LAPACK_Layout.RowMajor, LAPACK_Job.S, LAPACK_Job.S,
                m, n, c.SPtr, n, s.SPtr, u.SPtr, m, vt.SPtr, n, superb.SPtr);
            #endregion

            // inverse of s
            VectorD invS = VMath.Inv(s);
            // u = u * inv(S)
            DiagonalMatrixHelper.Dot(ref u, invS, checkLength: false);
            // (vt)T * (u)T
            MatrixZ b = new(vt.Cols, u.Rows);
            factory.iBLAS.GemmZ(BLAS_Layout.RowMajor, BLAS_Transpose.Trans, BLAS_Transpose.Trans,
                vt.Cols, u.Rows, vt.Rows, 1.0, vt, vt.Rows, u, u.Rows, 1.0, ref b, b.Cols);
            return b;
        }

        /// <summary>
        /// performs pseudo inverse of matrix a
        /// </summary>
        /// <param name="a"> input real-valued matrix a </param>
        /// <returns> pseudo inverse of a </returns>
        public static MatrixD PInv(MatrixD a)
        {
            // SVD preparation
            long m = a.Rows;
            long n = a.Cols;

            VectorD s = new(count: n);
            MatrixD u = new(rows: m, cols: m);
            MatrixD vt = new(rows: n, cols: n);
            VectorD superb = new(count: Math.Min(m, n));

            MatrixD t = new(other: a, deepCopy: true);
            factory.iLAPACK.Gesvd(LAPACK_Layout.RowMajor, LAPACK_Job.A, LAPACK_Job.A,
                m, n, t.SPtr, n, s.SPtr, u.SPtr, m, vt.SPtr, n, superb.SPtr); // Job S ???

            //// U * inv(S) => U
            //LongRange allRows = new (start: 0, end: u.Rows);
            //for (long i = 0; i < Math.Min(m, n); i++)
            //{
            //    double si = s[i, false] > 1.0E-9? 1.0 / s[i, false] : s[i, false];
            //    VectorD coli = u[allRows, i];
            //    u[allRows, i] = VMath.Scale(coli, si);
            //}
            //// V * Ut
            //MatrixD ap = new (rows: n, cols: m);
            //kernel.iBLAS.GemmD(BLAS_Layout.RowMajor, 
            //    transa: BLAS_Transpose.Trans, transb: BLAS_Transpose.Trans,
            //    vt.Cols, u.Rows, vt.Rows, 1.0, a: vt, vt.Rows, b: u, u.Rows, 1.0, c: ref ap, ap.Cols);
            //return ap;

            // constructs D by inversing each element in s as its diagonal element
            // now as a dense matrix
            MatrixD d = new(rows: m, cols: n, initVal: 0.0);
            for (long i = 0; i < Math.Min(m, n); i++)
            { d[i, i, false] = s[i, false] > 1E-9 ? 1.0 / s[i, false] : s[i, false]; }
            // U * D
            u = Dot(u, d);
            // computes pseudo inverse
            MatrixD ap = Dot(u, vt);
            Transpose(ref ap);
            return ap;
        }

        /// <summary>
        /// performs pseudo inverse of matrix a
        /// </summary>
        /// <param name="a"> input real-valued matrix a </param>
        /// <returns> pseudo inverse of a </returns>
        public static MatrixZ PInv(MatrixZ a)
        {
            // SVD preparation
            long m = a.Rows;
            long n = a.Cols;

            VectorD s = new(count: n);
            MatrixZ u = new(rows: m, cols: m);
            MatrixZ vt = new(rows: n, cols: n);
            VectorD superb = new(count: Math.Min(m, n));

            MatrixZ t = new(other: a, deepCopy: true);
            factory.iLAPACK.Gesvd(LAPACK_Layout.RowMajor, LAPACK_Job.A, LAPACK_Job.A,
                m, n, t.SPtr, n, s.SPtr, u.SPtr, m, vt.SPtr, n, superb.SPtr); // Job S ???

            // constructs D by inversing each element in s as its diagonal element
            // now as a dense matrix
            MatrixZ d = new(rows: m, cols: n, initVal: 0.0);
            for (long i = 0; i < Math.Min(m, n); i++)
            { d[i, i, false] = s[i, false] > 1E-9 ? 1.0 / s[i, false] : s[i, false]; }
            // U * D
            u = Dot(u, d);
            // computes pseudo inverse
            MatrixZ ap = Dot(u, vt);
            ConjugateTranspose(ref ap);
            return ap;
        }

        #endregion

        #region ---- QR (naive) ---

        ///// <summary>
        ///// performs QR decomposition of a real matrix 
        ///// using Gram-Schmidt process
        ///// A = Q * R
        ///// </summary>
        ///// <param name="a"> input matrix a[m x n] </param>
        ///// <returns> (Q[m x n], R[n x n]) </returns>
        //public static (MatrixD, MatrixD) QRD(MatrixD a)
        //    => NaiveMath.QRDecomposition(a);

        ///// <summary>
        ///// solves the linear least squares problem 
        ///// using QR decomposition:
        ///// min_x ||A x - b||_2
        ///// </summary>
        ///// <param name="a"> input matrix A (m x n, m >= n) </param>
        ///// <param name="b"> input vector b (length m) </param>
        ///// <returns> solution vector x (length n) </returns>
        //public static VectorD QRLeastSquare(MatrixD a, VectorD b)
        //    => NaiveMath.QRLeastSquare(a, b);

        #endregion

        #endregion

        #region matrix helper methods

        #region --------- Identity Matrix ---------

        /// <summary>
        /// identity matrix helper methods
        /// </summary>
        public class IdentityMatrixHelper
        {
            /// <summary>
            /// generate an identity dense matrix
            /// with n rows and n columns
            /// </summary>
            /// <param name="n"> size of the matrix </param>
            /// <returns> dense identity matrix </returns>
            public static MatrixD GenerateDenseMatrixD(long n)
            {
                MatrixD m = new(n, n, 0.0);
                for (long i = 0; i < n; i++)
                    m[i, i, false] = 1.0;
                return m;
            }

            /// <summary>
            /// generate an identity dense matrix
            /// with n rows and n columns
            /// </summary>
            /// <param name="n"> size of the matrix </param>
            /// <returns> dense identity matrix </returns>
            public static MatrixZ GenerateDenseMatrixZ(long n)
            {
                MatrixZ m = new(n, n, 0.0);
                for (long i = 0; i < n; i++)
                    m[i, i, false] = 1.0;
                return m;
            }
        }

        #endregion
        #region --------- Diagonal Matrix ---------

        /// <summary>
        /// diagonal matrix helper methods
        /// </summary>
        public class DiagonalMatrixHelper
        {
            #region ---- create dense ----

            /// <summary>
            /// generates a dense diagonal matrix
            /// with given diagonal values
            /// </summary>
            /// <param name="diagonalValues"> values of diagonal elements </param>
            /// <returns> result dense matrix </returns>
            public static MatrixD GenerateDenseMatrixD(VectorD diagonalValues)
            {
                MatrixD m = new(diagonalValues.Count, diagonalValues.Count, 0.0);
                for (long i = 0; i < diagonalValues.Count; i++)
                    m[i, i, false] = diagonalValues[i, false];
                return m;
            }

            /// <summary>
            /// generates a dense diagonal matrix
            /// with given diagonal values
            /// </summary>
            /// <param name="diagonalValues"> values of diagonal elements </param>
            /// <returns> result dense matrix </returns>
            public static MatrixZ GenerateDenseMatrixZ(VectorZ diagonalValues)
            {
                MatrixZ m = new(diagonalValues.Count, diagonalValues.Count, 0.0);
                for (long i = 0; i < diagonalValues.Count; i++)
                    m[i, i, false] = diagonalValues[i, false];
                return m;
            }

            /// <summary>
            /// Generates a dense complex-valued diagonal matrix of size <paramref name="n"/> x <paramref name="n"/>.
            /// All diagonal elements are set to the specified complex value <paramref name="x"/>.
            /// Off-diagonal elements are initialized to zero.
            /// </summary>
            /// <param name="n">The size of the matrix (number of rows and columns).</param>
            /// <param name="x">The complex value to assign to each diagonal element.</param>
            /// <returns>
            /// A <see cref="MatrixZ"/> instance representing a dense diagonal matrix
            /// with <paramref name="x"/> on the diagonal and zeros elsewhere.
            /// </returns>
            public static MatrixZ GenerateDenseMatrixZ(long n, Complex x)
            {
                MatrixZ m = new (rows: n, cols: n, mode: ArrayInitMode.Calloc);
                for (long i = 0; i < n; i++)
                { m[i, i, false] = x; }
                return m;
            }

            /// <summary>
            /// generates a dense diagonal matrix
            /// with given diagonal values
            /// </summary>
            /// <param name="diagonalValues"> values of diagonal elements </param>
            /// <returns> result dense matrix </returns>
            public static MatrixZ GenerateDenseMatrixZ(VectorD diagonalValues)
            {
                MatrixZ m = new(diagonalValues.Count, diagonalValues.Count, 0.0);
                for (long i = 0; i < diagonalValues.Count; i++)
                    m[i, i, false] = diagonalValues[i, false];
                return m;
            }

            #endregion
            #region ---- add ----

            /// <summary>
            /// sum of a dense matrix and a diagoanl matrix
            /// </summary>
            /// <param name="m"> dense matrix (overwritten as output) </param>
            /// <param name="d"> diagonal matrix stored as a vector </param>
            public static void AddTo(VectorD d, ref MatrixD m)
            {
                for (long i = 0; i < d.Count; i++)
                    m[i, i, false] += d[i, false];
            }

            /// <summary>
            /// sum of a dense matrix and a diagoanl matrix
            /// </summary>
            /// <param name="m"> dense matrix (overwritten as output) </param>
            /// <param name="d"> diagonal matrix stored as a vector </param>
            public static void AddTo(VectorZ d, ref MatrixZ m)
            {
                for (long i = 0; i < d.Count; i++)
                    m[i, i, false] += d[i, false];
            }

            /// <summary>
            /// sum of a dense matrix and a diagoanl matrix
            /// </summary>
            /// <param name="m"> dense matrix (overwritten as output) </param>
            /// <param name="d"> diagonal matrix stored as a vector </param>
            public static void AddTo(VectorD d, ref MatrixZ m)
            {
                for (long i = 0; i < d.Count; i++)
                    m[i, i, false] += d[i, false];
            }

            #endregion
            #region ---- dot ----

            #region ======== M = M * D ========

            /// <summary>
            /// dot-product of a dense matrix (on the left) 
            /// and a diagonal matrix (on the right)
            /// </summary>
            /// <param name="m"> dense matrix (overwritten as output) </param>
            /// <param name="d"> diagonal matrix stored as a vector </param>
            /// <param name="checkLength"> whether checks the lengths of input arrays </param>
            public static void Dot(ref MatrixD m, VectorD d,
                bool checkLength = true)
            {
                if(checkLength && (m.Rows != d.Count || m.Cols != d.Count))
                { Printer.Warning($"Array lengths not equal"); }

                LongRange allCols = new(0, m.Cols);
                for (long iRow = 0; iRow < m.Rows; iRow++)
                {
                    VectorD? r = m[iRow, allCols];
                    if (r != null)
                    { m[iRow, allCols] = VMath.Mul(r, d, checkLength: false); }
                }
            }

            /// <summary>
            /// dot-product of a dense matrix (on the left) 
            /// and a diagonal matrix (on the right)
            /// </summary>
            /// <param name="m"> dense matrix (overwritten as output) </param>
            /// <param name="d"> diagonal matrix stored as a vector </param>
            /// <param name="checkLength"> whether checks the lengths of input arrays </param>
            public static void Dot(ref MatrixZ m, VectorZ d,
                bool checkLength = true)
            {
                if (checkLength && (m.Rows != d.Count || m.Cols != d.Count))
                { Printer.Warning($"Array lengths not equal"); }

                LongRange allCols = new(0, m.Cols);
                for (long iRow = 0; iRow < m.Rows; iRow++)
                {
                    VectorZ? r = m[iRow, allCols];
                    if (r != null)
                    { m[iRow, allCols] = VMath.Mul(r, d, checkLength: false); }
                }
            }

            /// <summary>
            /// dot-product of a dense matrix (on the left) 
            /// and a diagonal matrix (on the right)
            /// </summary>
            /// <param name="m"> dense matrix (overwritten as output) </param>
            /// <param name="d"> diagonal matrix stored as a vector </param>
            /// <param name="checkLength"> whether checks the lengths of input arrays </param>
            public static void Dot(ref MatrixZ m, VectorD d,
                bool checkLength = true)
            {
                if (checkLength && (m.Rows != d.Count || m.Cols != d.Count))
                { Printer.Warning($"Array lengths not equal"); }

                LongRange allCols = new(0, m.Cols);
                for (long iRow = 0; iRow < m.Rows; iRow++)
                {
                    VectorZ? r = m[iRow, allCols];
                    if (r != null)
                        m[iRow, allCols] = VMath.Mul(r, d, checkLength: false);
                }
            }

            #endregion
            #region ======== M = d * M ========

            /// <summary>
            /// dot-product of a diagonal matrix (on the left) 
            /// and a dense matrix (on the right)
            /// </summary>
            /// <param name="d"> diagonal matrix stored as a vector </param>
            /// <param name="m"> dense matrix (overwritten as output) </param>
            public static void Dot(VectorD d, ref MatrixD m)
            {
                LongRange allCols = new(0, m.Cols);
                for (long iRow = 0; iRow < m.Rows; iRow++)
                {
                    VectorD? r = m[iRow, allCols];
                    if (r != null)
                        m[iRow, allCols] = r * d[iRow, false]; //VMath.Scale(r, d[iRow, false]);
                }
            }

            /// <summary>
            /// dot-product of a diagonal matrix (on the left) 
            /// and a dense matrix (on the right)
            /// </summary>
            /// <param name="d"> diagonal matrix stored as a vector </param>
            /// <param name="m"> dense matrix (overwritten as output) </param>
            public static void Dot(VectorZ d, ref MatrixZ m)
            {
                LongRange allCols = new(0, m.Cols);
                for (long iRow = 0; iRow < m.Rows; iRow++)
                {
                    VectorZ? r = m[iRow, allCols];
                    if (r != null)
                        m[iRow, allCols] = r * d[iRow, false]; //VMath.Scale(r, d[iRow, false]);
                }
            }

            /// <summary>
            /// dot-product of a diagonal matrix (on the left) 
            /// and a dense matrix (on the right)
            /// </summary>
            /// <param name="d"> diagonal matrix stored as a vector </param>
            /// <param name="m"> dense matrix (overwritten as output) </param>
            public static void Dot(VectorD d, ref MatrixZ m)
            {
                LongRange allCols = new(0, m.Cols);
                for (long iRow = 0; iRow < m.Rows; iRow++)
                {
                    VectorZ? r = m[iRow, allCols];
                    if (r != null)
                        m[iRow, allCols] = r * d[iRow, false]; //VMath.Scale(r, d[iRow, false]);
                }
            }

            #endregion
            #region ======== M = D1 * M * D2 ========

            /// <summary>
            /// dot-product of a diagonal matrix (on the left),
            /// a dense matrix (in the middle), 
            /// and a diagonal matrix (on the right)
            /// </summary>
            /// <param name="d1"> diagonal matrix on the left </param>
            /// <param name="m"> dense matrix in the middle (overwritten as output) </param>
            /// <param name="d2"> diagonal matrix on the right </param>
            public static void Dot(VectorD d1, ref MatrixD m, VectorD d2)
            {
                LongRange allCols = new(0, m.Cols);
                for (long iRow = 0; iRow < m.Rows; iRow++)
                {
                    VectorD? r = m[iRow, allCols];
                    if (r != null)
                        m[iRow, allCols] = VMath.Mul(r, d2) * d1[iRow, false]; //VMath.Scale(VMath.Mul(r, d2), d1[iRow, false]);
                }
            }

            /// <summary>
            /// dot-product of a diagonal matrix (on the left),
            /// a dense matrix (in the middle), 
            /// and a diagonal matrix (on the right)
            /// </summary>
            /// <param name="d1"> diagonal matrix on the left </param>
            /// <param name="m"> dense matrix in the middle (overwritten as output) </param>
            /// <param name="d2"> diagonal matrix on the right </param>
            public static void Dot(VectorZ d1, ref MatrixZ m, VectorZ d2)
            {
                LongRange allCols = new(0, m.Cols);
                for (long iRow = 0; iRow < m.Rows; iRow++)
                {
                    VectorZ? r = m[iRow, allCols];
                    if (r != null)
                        m[iRow, allCols] = VMath.Mul(r, d2) * d1[iRow, false]; //VMath.Scale(VMath.Mul(r, d2), d1[iRow, false]);
                }
            }

            /// <summary>
            /// dot-product of a diagonal matrix (on the left),
            /// a dense matrix (in the middle), 
            /// and a diagonal matrix (on the right)
            /// </summary>
            /// <param name="d1"> diagonal matrix on the left </param>
            /// <param name="m"> dense matrix in the middle (overwritten as output) </param>
            /// <param name="d2"> diagonal matrix on the right </param>
            public static void Dot(VectorD d1, ref MatrixZ m, VectorD d2)
            {
                LongRange allCols = new(0, m.Cols);
                for (long iRow = 0; iRow < m.Rows; iRow++)
                {
                    VectorZ? r = m[iRow, allCols];
                    if (r != null)
                        m[iRow, allCols] = VMath.Mul(r, d2) * d1[iRow, false]; //VMath.Scale(VMath.Mul(r, d2), d1[iRow, false]);
                }
            }

            #endregion
            #region ======== X = M * D ========

            /// <summary>
            /// dot-product of a dense matrix (on the left) 
            /// and a diagonal matrix (on the right)
            /// </summary>
            /// <param name="m"> dense matrix </param>
            /// <param name="d"> diagonal matrix stored as a vector </param>
            /// <returns> result dense matrix </returns>
            public static MatrixD Dot(MatrixD m, VectorD d)
            {
                MatrixD res = new(m, true);
                Dot(ref res, d);
                return res;
            }

            /// <summary>
            /// dot-product of a dense matrix (on the left) 
            /// and a diagonal matrix (on the right)
            /// </summary>
            /// <param name="m"> dense matrix </param>
            /// <param name="d"> diagonal matrix stored as a vector </param>
            /// <returns> result dense matrix </returns>
            public static MatrixZ Dot(MatrixZ m, VectorZ d)
            {
                MatrixZ res = new(m, true);
                Dot(ref res, d);
                return res;
            }

            /// <summary>
            /// dot-product of a dense matrix (on the left) 
            /// and a diagonal matrix (on the right)
            /// </summary>
            /// <param name="m"> dense matrix </param>
            /// <param name="d"> diagonal matrix stored as a vector </param>
            /// <returns> result dense matrix </returns>
            public static MatrixZ Dot(MatrixZ m, VectorD d)
            {
                MatrixZ res = new(m, true);
                Dot(ref res, d);
                return res;
            }

            #endregion
            #region ======== X = D * M ========

            /// <summary>
            /// dot-product of a diagonal matrix (on the left) 
            /// and a dense matrix (on the right)
            /// </summary>
            /// <param name="d"> diagonal matrix stored as a vector </param>
            /// <param name="m"> dense matrix </param>
            /// <returns> result dense matrix </returns>
            public static MatrixD Dot(VectorD d, MatrixD m)
            {
                MatrixD res = new(m, true);
                Dot(d, ref res);
                return res;
            }

            /// <summary>
            /// dot-product of a diagonal matrix (on the left) 
            /// and a dense matrix (on the right)
            /// </summary>
            /// <param name="d"> diagonal matrix stored as a vector </param>
            /// <param name="m"> dense matrix </param>
            /// <returns> result dense matrix </returns>
            public static MatrixZ Dot(VectorZ d, MatrixZ m)
            {
                MatrixZ res = new(m, true);
                Dot(d, ref res);
                return res;
            }

            /// <summary>
            /// dot-product of a diagonal matrix (on the left) 
            /// and a dense matrix (on the right)
            /// </summary>
            /// <param name="d"> diagonal matrix stored as a vector </param>
            /// <param name="m"> dense matrix </param>
            /// <returns> result dense matrix </returns>
            public static MatrixZ Dot(VectorD d, MatrixZ m)
            {
                MatrixZ res = new(m, true);
                Dot(d, ref res);
                return res;
            }

            #endregion
            #region ======== X = D1 * M * D2 ========

            /// <summary>
            /// dot-product of a diagonal matrix (on the left),
            /// a dense matrix (in the middle), 
            /// and a diagonal matrix (on the right)
            /// </summary>
            /// <param name="d1"> diagonal matrix on the left </param>
            /// <param name="m"> dense matrix in the middle </param>
            /// <param name="d2"> diagonal matrix on the right </param>
            /// <returns> result dense matrix </returns>
            public static MatrixD Dot(VectorD d1, MatrixD m, VectorD d2)
            {
                MatrixD res = new(m, true);
                Dot(d1, ref res, d2);
                return res;
            }

            /// <summary>
            /// dot-product of a diagonal matrix (on the left),
            /// a dense matrix (in the middle), 
            /// and a diagonal matrix (on the right)
            /// </summary>
            /// <param name="d1"> diagonal matrix on the left </param>
            /// <param name="m"> dense matrix in the middle </param>
            /// <param name="d2"> diagonal matrix on the right </param>
            /// <returns> result dense matrix </returns>
            public static MatrixZ Dot(VectorZ d1, MatrixZ m, VectorZ d2)
            {
                MatrixZ res = new(m, true);
                Dot(d1, ref res, d2);
                return res;
            }

            /// <summary>
            /// dot-product of a diagonal matrix (on the left),
            /// a dense matrix (in the middle), 
            /// and a diagonal matrix (on the right)
            /// </summary>
            /// <param name="d1"> diagonal matrix on the left </param>
            /// <param name="m"> dense matrix in the middle </param>
            /// <param name="d2"> diagonal matrix on the right </param>
            /// <returns> result dense matrix </returns>
            public static MatrixZ Dot(VectorD d1, MatrixZ m, VectorD d2)
            {
                MatrixZ res = new(m, true);
                Dot(d1, ref res, d2);
                return res;
            }

            #endregion

            #endregion
            #region ---- inverse ----

            // ...

            #endregion
            #region ---- inverse [2x2 block] ----


            internal static (VectorD, VectorD, VectorD, VectorD) Inverse2x2Block(
                VectorD m11, VectorD m12,
                VectorD m21, VectorD m22)
            {
                VectorD delta = VMath.Inv(m22 - m21 / m11 * m12);
                // modify blocks
                VectorD o11 = VMath.Inv(m11) + m12 / m11 * delta * m21 / m11;
                VectorD o12 = -m12 / m11 * delta;
                VectorD o21 = -delta * m21 / m11;
                VectorD o22 = delta;
                // return
                return (o11, o12, o21, o22);
            }

            internal static void Inverse2x2Block(
                ref VectorD m11, ref VectorD m12,
                ref VectorD m21, ref VectorD m22)
            {
                VectorD delta = VMath.Inv(m22 - m21 / m11 * m12);
                // modify blocks
                VectorD t11 = new(m11, true);
                VectorD t12 = new(m12, true);
                VectorD t21 = new(m21, true);
                m11 = VMath.Inv(t11) + t12 / t11 * delta * t21 / t11;
                m12 = -t12 / t11 * delta;
                m21 = -delta * t21 / t11;
                m22 = delta;
            }

            internal static (VectorZ, VectorZ, VectorZ, VectorZ) Inverse2x2Block(
                VectorZ m11, VectorZ m12,
                VectorZ m21, VectorZ m22)
            {
                VectorZ delta = 1.0 / (m22 - m21 / m11 * m12);
                // modify blocks
                VectorZ o11 = 1.0 / m11 + m12 / m11 * delta * m21 / m11;
                VectorZ o12 = -m12 / m11 * delta;
                VectorZ o21 = -delta * m21 / m11;
                VectorZ o22 = delta;
                // return
                return (o11, o12, o21, o22);
            }

            /// <summary>
            /// special method to inverse a 2x2-block matrix
            /// </summary>
            /// <param name="m11"> matrix block (1,1) </param>
            /// <param name="m12"> matrix block (1,2) </param>
            /// <param name="m21"> matrix block (2,1) </param>
            /// <param name="m22"> matrix block (2,2) </param>
            public static void Inverse2x2Block(
                ref VectorZ m11, ref VectorZ m12,
                ref VectorZ m21, ref VectorZ m22)
            {
                VectorZ delta = 1.0 / (m22 - m21 / m11 * m12);
                // modify blocks
                VectorZ t11 = new(m11, true);
                VectorZ t12 = new(m12, true);
                VectorZ t21 = new(m21, true);
                m11 = 1.0 / t11 + t12 / t11 * delta * t21 / t11;
                m12 = -t12 / t11 * delta;
                m21 = -delta * t21 / t11;
                m22 = delta;
            }


            #endregion
        }

        #endregion 
        #region --------- Toeplitz Matrix ---------

        /// <summary>
        /// generates a toeplitz dense matrix 
        /// </summary>
        /// <param name="toeplitzValues"> toeplitz values (odd number of elements) </param>
        /// <returns> result dense matrix </returns>
        [Obsolete]
        public static MatrixD GenerateToeplitzMatrixD(VectorD toeplitzValues)
        {
            // number of toeplitz values must be odd
            if (toeplitzValues.Count % 2 == 0)
            {
                Console.WriteLine("Number of toeplitz values must be odd");
                return new MatrixD(1, 1, 0.0);
            }

            long n = (toeplitzValues.Count + 1) / 2;
            MatrixD m = new(n, n, 0.0);
            for (long iRow = 0; iRow < m.Rows; iRow++)
            {
                for (long iCol = 0; iCol < m.Cols; iCol++)
                    m[iRow, iCol, false] = toeplitzValues[n - 1 + iCol - iRow, false];
            }

            // return
            return m;
        }

        /// <summary>
        /// generates a toeplitz dense matrix 
        /// by defining its first column and row
        /// </summary>
        /// <param name="firstCol"> the first column </param>
        /// <param name="firstRow"> the first row </param>
        /// <returns> toeplitz dense matrix </returns>
        public static MatrixD GenerateToeplitzMatrixD(VectorD firstCol, VectorD firstRow)
        {
            // checks the first elements from inputs
            if (firstCol[0] != firstRow[0]) { Printer.Warning($"First elements of input vectors do not match"); }
            // fills the matrix
            MatrixD t = new(firstCol.Count, firstRow.Count);
            for (long iRow = 0; iRow < t.Rows; iRow++)
            {
                for (long iCol = 0; iCol < t.Cols; iCol++)
                {
                    if (iRow >= iCol) // lower-left, including diagonal
                        t[iRow, iCol, false] = firstCol[iRow - iCol, false];
                    else // upper-right
                        t[iRow, iCol, false] = firstRow[iCol - iRow, false];
                }
            }
            return t;
        }

        /// <summary>
        /// generates a toeplitz dense matrix 
        /// </summary>
        /// <param name="toeplitzValues"> toeplitz values (odd number of elements) </param>
        /// <returns> result dense matrix </returns>
        [Obsolete]
        public static MatrixZ GenerateToeplitzMatrixZ(VectorZ toeplitzValues)
        {
            // number of toeplitz values must be odd
            if (toeplitzValues.Count % 2 == 0)
            {
                Console.WriteLine("Number of toeplitz values must be odd");
                return new MatrixZ(1, 1, 0.0);
            }

            long n = (toeplitzValues.Count + 1) / 2;
            MatrixZ m = new(n, n, 0.0);
            for (long iRow = 0; iRow < m.Rows; iRow++)
            {
                for (long iCol = 0; iCol < m.Cols; iCol++)
                    m[iRow, iCol, false] = toeplitzValues[n - 1 + iCol - iRow, false];
            }

            // return
            return m;
        }

        /// <summary>
        /// generates a toeplitz dense matrix 
        /// by defining its first column and row
        /// </summary>
        /// <param name="firstCol"> the first column </param>
        /// <param name="firstRow"> the first row </param>
        /// <returns> toeplitz dense matrix </returns>
        public static MatrixZ GenerateToeplitzMatrixZ(VectorZ firstCol, VectorZ firstRow)
        {
            // checks the first elements from inputs
            if (firstCol[0] != firstRow[0]) { Printer.Warning($"First elements of input vectors do not match"); }
            // fills the matrix
            MatrixZ t = new(firstCol.Count, firstRow.Count, 0.0);
            for (long iRow = 0; iRow < t.Rows; iRow++)
            {
                for (long iCol = 0; iCol < t.Cols; iCol++)
                {
                    if (iRow >= iCol) // lower-left, including diagonal
                        t[iRow, iCol, false] = firstCol[iRow - iCol, false];
                    else // upper-right
                        t[iRow, iCol, false] = firstRow[iCol - iRow, false];
                }
            }
            return t;
        }

        #endregion
        #region --------- Diagonal Matrix Product ---------

        /// <summary>
        /// product of a dense matrix (left) 
        /// with a diagonal matrix (right)
        /// </summary>
        /// <param name="m"> dense matrix (overwritten as output) </param>
        /// <param name="d"> diagonal matrix stored in a vector </param>
        [Obsolete("Use DiagonalMatrixHelper method instead")]
        public static void DotWithDiagonalMatrixD(ref MatrixD m, VectorD d)
        {
            for (long iCol = 0; iCol < m.Cols; iCol++)
                m[new LongRange(0, m.Rows), iCol] *= d[iCol, false];
        }

        /// <summary>
        /// product of a dense matrix (left) 
        /// with a diagonal matrix (right)
        /// </summary>
        /// <param name="m"> dense matrix </param>
        /// <param name="d"> diagonal matrix stored in a vector </param>
        /// <returns> result matrix </returns>
        [Obsolete("Use DiagonalMatrixHelper method instead")]
        public static MatrixD DotWithDiagonalMatrixD(MatrixD m, VectorD d)
        {
            MatrixD res = new(m, true);
            for (long iCol = 0; iCol < res.Cols; iCol++)
                res[new LongRange(0, res.Rows), iCol] *= d[iCol, false];
            return res;
        }

        /// <summary>
        /// product of a diagonal matrix (left)
        /// with a dense matrix (right)
        /// </summary>
        /// <param name="d"> diagonal matrix </param>
        /// <param name="m"> dense matrix (overwritten as output) </param>
        [Obsolete("Use DiagonalMatrixHelper method instead")]
        public static void DotWithDiagonalMatrixD(VectorD d, ref MatrixD m)
        {
            for (long iRow = 0; iRow < m.Rows; iRow++)
                m[iRow, new LongRange(0, m.Cols)] *= d[iRow, false];
        }

        /// <summary>
        /// product of a diagonal matrix (left)
        /// with a dense matrix (right)
        /// </summary>
        /// <param name="d"> diagonal matrix </param>
        /// <param name="m"> dense matrix </param>
        /// <returns> result matrix </returns>
        [Obsolete("Use DiagonalMatrixHelper method instead")]
        public static MatrixD DotWithDiagonalMatrixD(VectorD d, MatrixD m)
        {
            MatrixD res = new(m, true);
            for (long iRow = 0; iRow < res.Rows; iRow++)
                res[iRow, new LongRange(0, res.Cols)] *= d[iRow, false];
            return res;
        }

        /// <summary>
        /// product of a dense matrix (left)
        /// with a diagonal matrix (right)
        /// </summary>
        /// <param name="m"> dense matrix (overwritten as output) </param>
        /// <param name="d"> diagonal matrix </param>
        [Obsolete("Use DiagonalMatrixHelper method instead")]
        public static void DotWithDiagonalMatrixZ(ref MatrixZ m, VectorZ d)
        {
            for (long iCol = 0; iCol < m.Cols; iCol++)
                m[new LongRange(0, m.Rows), iCol] *= d[iCol, false];
        }

        /// <summary>
        /// product of a dense matrix (left)
        /// with a diagonal matrix (right)
        /// </summary>
        /// <param name="m"> dense matrix </param>
        /// <param name="d"> diagonal matrix </param>
        /// <returns> result matrix </returns>
        [Obsolete("Use DiagonalMatrixHelper method instead")]
        public static MatrixZ DotWithDiagonalMatrixZ(MatrixZ m, VectorZ d)
        {
            MatrixZ res = new(m, true);
            for (long iCol = 0; iCol < res.Cols; iCol++)
                res[new LongRange(0, res.Rows), iCol] *= d[iCol, false];
            return res;
        }

        /// <summary>
        /// product of a diagonal matrix (left)
        /// with a dense matrix (right)
        /// </summary>
        /// <param name="d"> diagonal matrix </param>
        /// <param name="m"> dense matrix (overwritten as output) </param>
        [Obsolete("Use DiagonalMatrixHelper method instead")]
        public static void DotWithDiagonalMatrixZ(VectorZ d, ref MatrixZ m)
        {
            for (long iRow = 0; iRow < m.Rows; iRow++)
                m[iRow, new LongRange(0, m.Cols)] *= d[iRow, false];
        }

        /// <summary>
        /// product of a diagonal matrix (left)
        /// with a dense matrix (right)
        /// </summary>
        /// <param name="d"> diagonal matrix </param>
        /// <param name="m"> dense matrix </param>
        /// <returns> result matrix </returns>
        [Obsolete("Use DiagonalMatrixHelper method instead")]
        public static MatrixZ DotWithDiagonalMatrixZ(VectorZ d, MatrixZ m)
        {
            MatrixZ res = new(m, true);
            for (long iRow = 0; iRow < m.Rows; iRow++)
                res[iRow, new LongRange(0, m.Cols)] *= d[iRow, false];
            return res;
        }

        #endregion
        #region --------- Row/Column Matrix ---------

        /// <summary>
        /// generates a matrix by replicating given row elements
        /// </summary>
        /// <param name="rowElements"> elements within the row </param>
        /// <param name="rows"> number of rows </param>
        /// <returns> result matrix </returns>
        public static MatrixD GenerateRowMatrix(VectorD rowElements, long rows)
        {
            MatrixD x = new(rows, rowElements.Count);
            LongRange allCols = new (0, x.Cols);
            for (long i = 0; i < x.Rows; i++)
                x[i, allCols] = rowElements;
            return x;
        }

        /// <summary>
        /// generates a matrix by replicating given row elements
        /// </summary>
        /// <param name="rowElements"> elements within the row </param>
        /// <param name="rows"> number of rows </param>
        /// <returns> result matrix </returns>
        public static MatrixZ GenerateRowMatrix(VectorZ rowElements, long rows)
        {
            MatrixZ x = new(rows, rowElements.Count);
            LongRange allCols = new(0, x.Cols);
            for (long i = 0; i < x.Rows; i++)
                x[i, allCols] = rowElements;
            return x;
        }

        /// <summary>
        /// generates a matrix by replicating given column elements
        /// </summary>
        /// <param name="colElements"> elements within the column </param>
        /// <param name="cols"> number of columns </param>
        /// <returns> result matrix </returns>
        public static MatrixD GenerateColMatrix(VectorD colElements, long cols)
        {
            MatrixD x = new(colElements.Count, cols);
            LongRange allRows = new(0, x.Rows);
            for (long i = 0; i < x.Cols; i++)
                x[allRows, i] = colElements;
            return x;
        }

        /// <summary>
        /// generates a matrix by replicating given column elements
        /// </summary>
        /// <param name="colElements"> elements within the column </param>
        /// <param name="cols"> number of columns </param>
        /// <returns> result matrix </returns>
        public static MatrixZ GenerateColMatrix(VectorZ colElements, long cols)
        {
            MatrixZ x = new(colElements.Count, cols);
            LongRange allRows = new(0, x.Rows);
            for (long i = 0; i < x.Cols; i++)
                x[allRows, i] = colElements;
            return x;
        }

        #endregion

        #endregion

    }



}