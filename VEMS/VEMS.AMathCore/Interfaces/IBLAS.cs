using System.Runtime.InteropServices;

namespace VEMS.AMathCore
{

    /// <summary>
    /// BLAS interface
    /// </summary>
    public unsafe interface IBLAS
    {
        #region Level 1 

        #region ---- Asum [D/Z] ----

        /// <summary>
        /// Computes the sum of magnitudes of the elements in a double-precision array.
        /// dasum := |x[0]| + |x[1]| + ... + |x[n-1]|
        /// </summary>
        /// <param name="n">Number of elements in the array.</param>
        /// <param name="x">Pointer to the double-precision array.</param>
        /// <param name="incx">Increment for indexing x.</param>
        /// <returns>Sum of the magnitudes of the elements.</returns>
        double Asum(long n, [In] double* x,
            long incx = 1);

        /// <summary>
        /// Computes the sum of magnitudes of the elements in a complex double-precision array.
        /// dzasum := |Re(x[0])| + |Im(x[0])| + |Re(x[1])| + |Im(x[1])| + ... + |Re(x[n-1])| + |Im(x[n-1])|
        /// </summary>
        /// <param name="n">Number of elements in the array.</param>
        /// <param name="x">Pointer to the complex double-precision array (as void*).</param>
        /// <param name="incx">Increment for indexing x.</param>
        /// <returns>Sum of the magnitudes of the real and imaginary parts of the elements.</returns>
        double Asum(long n, [In] void* x,
            long incx = 1);

        #endregion
        #region ---- Axpy [D/Z] ----

        /// <summary>
        /// Computes a scalar-array product and adds the result to another array (double precision).
        /// Performs the operation: y := a * x + y
        /// </summary>
        /// <param name="n">Number of array elements.</param>
        /// <param name="a">Scalar multiplier.</param>
        /// <param name="x">Pointer to the input array x.</param>
        /// <param name="y">Pointer to the input/output array y.</param>
        /// <param name="incx">Increment for indexing x (default is 1).</param>
        /// <param name="incy">Increment for indexing y (default is 1).</param>
        void Axpy(long n, double a, [In] double* x,
            [In, Out] double* y,
            long incx = 1, long incy = 1);

        /// <summary>
        /// Computes a scalar-array product and adds the result to another array (complex double precision).
        /// Performs the operation: y := a * x + y
        /// </summary>
        /// <param name="n">Number of array elements.</param>
        /// <param name="a">Pointer to the scalar multiplier (complex).</param>
        /// <param name="x">Pointer to the input array x (complex).</param>
        /// <param name="y">Pointer to the input/output array y (complex).</param>
        /// <param name="incx">Increment for indexing x (default is 1).</param>
        /// <param name="incy">Increment for indexing y (default is 1).</param>
        void Axpy(long n, void* a, [In] void* x,
            [In, Out] void* y,
            long incx = 1, long incy = 1);
        
        #endregion
        #region ---- Copy [D/Z] ----

        /// <summary>
        /// Copies elements from the source array <paramref name="x"/> to the destination array <paramref name="y"/>.
        /// y := x
        /// </summary>
        /// <param name="n">Number of elements to copy.</param>
        /// <param name="x">Pointer to the source array.</param>
        /// <param name="y">Pointer to the destination array.</param>
        /// <param name="incx">Increment for the elements of <paramref name="x"/>. Default is 1.</param>
        /// <param name="incy">Increment for the elements of <paramref name="y"/>. Default is 1.</param>
        void Copy(long n, [In] double* x, [Out] double* y,
            long incx = 1, long incy = 1);

        /// <summary>
        /// Copies elements from the source array <paramref name="x"/> to the destination array <paramref name="y"/> (complex version).
        /// y := x
        /// </summary>
        /// <param name="n">Number of elements to copy.</param>
        /// <param name="x">Pointer to the source array (complex values).</param>
        /// <param name="y">Pointer to the destination array (complex values).</param>
        /// <param name="incx">Increment for the elements of <paramref name="x"/>. Default is 1.</param>
        /// <param name="incy">Increment for the elements of <paramref name="y"/>. Default is 1.</param>
        public void Copy(long n, [In] void* x, [Out] void* y,
            long incx = 1, long incy = 1);


        //// temp
        ///// <summary>
        ///// copies x to y with pointers
        ///// y := x
        ///// </summary>
        ///// <param name="n"> number of elements </param>
        ///// <param name="x"> pointer to array x </param>
        ///// <param name="y"> pointer to array y </param>
        ///// <param name="incx"> increment for indexing x </param>
        ///// <param name="incy"> increment for indexing y </param>
        //unsafe void Copy(long n, Complex* x, Complex* y,
        //    long incx = 1, long incy = 1);

        #endregion
        #region ---- Dot [D/Z] ----

        /// <summary>
        /// Computes the vector-vector dot product for double-precision arrays.
        /// res = x[0]*y[0] + x[1]*y[1] +...+ x[n-1]*y[n-1]
        /// </summary>
        /// <param name="n">Number of elements in the vectors.</param>
        /// <param name="x">Pointer to the first input vector.</param>
        /// <param name="y">Pointer to the second input vector.</param>
        /// <param name="incx">Increment for indexing elements of <paramref name="x"/>. Default is 1.</param>
        /// <param name="incy">Increment for indexing elements of <paramref name="y"/>. Default is 1.</param>
        /// <returns>The result of the dot product of <paramref name="x"/> and <paramref name="y"/>.</returns>
        double Dot(long n, [In] double* x, [In] double* y,
            long incx = 1, long incy = 1);

        /// <summary>
        /// Computes the unconjugated dot product of two complex double-precision vectors.
        /// res = x[0]*y[0] + x[1]*y[1] +...+ x[n-1]*y[n-1]
        /// </summary>
        /// <param name="n">Number of elements in the vectors.</param>
        /// <param name="x">Pointer to the first input vector.</param>
        /// <param name="y">Pointer to the second input vector.</param>
        /// <param name="dotu">Pointer to the result (output) complex value.</param>
        /// <param name="incx">Increment for indexing elements of <paramref name="x"/>. Default is 1.</param>
        /// <param name="incy">Increment for indexing elements of <paramref name="y"/>. Default is 1.</param>
        void Dot(long n, [In] void* x, [In] void* y, void* dotu,
            long incx = 1, long incy = 1);

        #endregion
        #region ---- Dotc [Z] ----

        /// <summary>
        /// Computes the dot product of two complex vectors, conjugating the first vector.
        /// <para>dotc = conj(x[0])*y[0] + conj(x[1])*y[1] + ... + conj(x[n-1])*y[n-1]</para>
        /// </summary>
        /// <param name="n">Number of elements in the vectors.</param>
        /// <param name="x">Pointer to the first complex vector (to be conjugated).</param>
        /// <param name="y">Pointer to the second complex vector.</param>
        /// <param name="dotc">Pointer to the result (output).</param>
        /// <param name="incx">Increment for indexing <paramref name="x"/> (default is 1).</param>
        /// <param name="incy">Increment for indexing <paramref name="y"/> (default is 1).</param>
        void Dotc(long n, [In] void* x, [In] void* y, void* dotc,
            long incx = 1, long incy = 1);

        #endregion
        #region ---- Nrm2 [D/Z] ----

        /// <summary>
        /// Computes the Euclidean norm (2-norm) of a vector of double-precision values.
        /// res = ||x||
        /// </summary>
        /// <param name="n">The number of elements in the vector <paramref name="x"/>.</param>
        /// <param name="x">Pointer to the first element of the vector.</param>
        /// <param name="incx">The increment for the elements of <paramref name="x"/>. Default is 1.</param>
        /// <returns>The Euclidean norm of the vector.</returns>
        double Nrm2(long n, [In] double* x,
            long incx = 1);

        /// <summary>
        /// Computes the Euclidean norm (2-norm) of a vector of complex double-precision values.
        /// res = ||x||
        /// </summary>
        /// <param name="n">The number of elements in the vector <paramref name="x"/>.</param>
        /// <param name="x">Pointer to the first element of the complex vector.</param>
        /// <param name="incx">The increment for the elements of <paramref name="x"/>. Default is 1.</param>
        /// <returns>The Euclidean norm of the complex vector.</returns>
        double Nrm2(long n, [In] void* x,
            long incx = 1);

        #endregion
        #region ---- Rot [D/Z] ----

        /// <summary>
        /// Performs rotation of points in the plane for double-precision arrays.
        /// xi = c * xi + s * yi
        /// yi = c * yi - s * xi
        /// </summary>
        /// <param name="n">Number of elements in the arrays.</param>
        /// <param name="x">Pointer to the first element of array x.</param>
        /// <param name="y">Pointer to the first element of array y.</param>
        /// <param name="c">Cosine of the rotation angle.</param>
        /// <param name="s">Sine of the rotation angle.</param>
        /// <param name="incx">Increment for indexing x (default is 1).</param>
        /// <param name="incy">Increment for indexing y (default is 1).</param>
        void Rot(long n, [In, Out] double* x, [In, Out] double* y,
            double c, double s,
            long incx = 1, long incy = 1);

        /// <summary>
        /// Performs rotation of points in the plane for complex arrays (applies real rotation to complex data).
        /// xi = c * xi + s * yi
        /// yi = c * yi - s * xi
        /// </summary>
        /// <param name="n">Number of elements in the arrays.</param>
        /// <param name="x">Pointer to the first element of array x (complex data).</param>
        /// <param name="y">Pointer to the first element of array y (complex data).</param>
        /// <param name="c">Cosine of the rotation angle.</param>
        /// <param name="s">Sine of the rotation angle.</param>
        /// <param name="incx">Increment for indexing x (default is 1).</param>
        /// <param name="incy">Increment for indexing y (default is 1).</param>
        void Rot(long n, [In, Out] void* x, [In, Out] void* y,
            double c, double s,
            long incx = 1, long incy = 1);

        #endregion
        #region ---- Scal [D/Z] -----

        /// <summary>
        /// Scales a vector by a scalar value (double precision).
        /// x = a * x
        /// </summary>
        /// <param name="n">Number of elements in the vector x.</param>
        /// <param name="a">Scalar multiplier.</param>
        /// <param name="x">Pointer to the vector to be scaled.</param>
        /// <param name="incx">Increment for the elements of x (default is 1).</param>
        void Scal(long n, double a,
            [In, Out] double* x,
            long incx = 1);

        /// <summary>
        /// Scales a complex vector by a real scalar value (double precision).
        /// x = a * x, where x is complex and a is real.
        /// </summary>
        /// <param name="n">Number of elements in the vector x.</param>
        /// <param name="a">Scalar multiplier (real).</param>
        /// <param name="x">Pointer to the complex vector to be scaled.</param>
        /// <param name="incx">Increment for the elements of x (default is 1).</param>
        void Scal(long n, double a,
            [In, Out] void* x,
            long incx = 1);

        /// <summary>
        /// Scales a complex vector by a complex scalar value.
        /// x = a * x, where both a and x are complex.
        /// </summary>
        /// <param name="n">Number of elements in the vector x.</param>
        /// <param name="a">Pointer to the complex scalar multiplier.</param>
        /// <param name="x">Pointer to the complex vector to be scaled.</param>
        /// <param name="incx">Increment for the elements of x (default is 1).</param>
        void Scal(long n, void* a,
            [In, Out] void* x,
            long incx = 1);

        #endregion
        #region ---- Swap [D/Z] ----

        /// <summary>
        /// Swaps the elements of two double-precision arrays using pointers.
        /// </summary>
        /// <param name="n">Number of elements to swap.</param>
        /// <param name="x">Pointer to the first array.</param>
        /// <param name="y">Pointer to the second array.</param>
        /// <param name="incx">Increment for the elements of <paramref name="x"/>. Default is 1.</param>
        /// <param name="incy">Increment for the elements of <paramref name="y"/>. Default is 1.</param>
        void Swap(long n,
            [In, Out] double* x, [In, Out] double* y,
            long incx = 1, long incy = 1);

        /// <summary>
        /// Swaps the elements of two complex arrays using pointer access.
        /// </summary>
        /// <param name="n">The number of elements to swap.</param>
        /// <param name="x">Pointer to the first array.</param>
        /// <param name="y">Pointer to the second array.</param>
        /// <param name="incx">Increment for indexing <paramref name="x"/> (default is 1).</param>
        /// <param name="incy">Increment for indexing <paramref name="y"/> (default is 1).</param>
        void Swap(long n,
            [In, Out] void* x, [In, Out] void* y,
            long incx = 1, long incy = 1);

        #endregion
        #region ---- Iamax [D/Z] ----

        /// <summary>
        /// Finds the index of the element with the maximum absolute value in a double array.
        /// </summary>
        /// <param name="n">Number of elements in the array.</param>
        /// <param name="x">Pointer to the array of doubles.</param>
        /// <param name="incx">Increment for indexing x (default is 1).</param>
        /// <returns>Index of the element with the largest absolute value.</returns>
        long Iamax(long n, [In] double* x,
            long incx = 1);

        /// <summary>
        /// Finds the index of the element with the maximum absolute value in a complex array.
        /// </summary>
        /// <param name="n">Number of elements in the array.</param>
        /// <param name="x">Pointer to the array of complex numbers (as void*).</param>
        /// <param name="incx">Increment for indexing x (default is 1).</param>
        /// <returns>Index of the element with the largest absolute value.</returns>
        long Iamax(long n, [In] void* x,
            long incx = 1);

        #endregion
        #region ---- Iamin [D/Z] ----

        /// <summary>
        /// Finds the index of the element with the smallest absolute value in a double array.
        /// </summary>
        /// <param name="n">Number of elements in the array <paramref name="x"/>.</param>
        /// <param name="x">Pointer to the array of doubles.</param>
        /// <param name="incx">Increment for indexing <paramref name="x"/> (default is 1).</param>
        /// <returns>Index of the element with the smallest absolute value.</returns>
        long Iamin(long n, [In] double* x,
            long incx = 1);

        /// <summary>
        /// Finds the index of the element with the smallest absolute value in a complex array.
        /// </summary>
        /// <param name="n">Number of elements in the array <paramref name="x"/>.</param>
        /// <param name="x">Pointer to the array of complex numbers (as void*).</param>
        /// <param name="incx">Increment for indexing <paramref name="x"/> (default is 1).</param>
        /// <returns>Index of the element with the smallest absolute value.</returns>
        long Iamin(long n, [In] void* x,
            long incx = 1);

        #endregion

        #endregion
        #region Level 2

        #region ---- Gemv [D/Z] ----

        /// <summary>
        /// Computes a matrix-vector product using a general matrix (double precision).
        /// y := alpha * op(a) * x + beta * y
        /// </summary>
        /// <param name="layout">Specifies array storage: row- or column-major.</param>
        /// <param name="trans">Specifies matrix a transpose operation.</param>
        /// <param name="m">Number of rows of the matrix a.</param>
        /// <param name="n">Number of columns of the matrix a.</param>
        /// <param name="alpha">Scalar alpha.</param>
        /// <param name="a">Pointer to array (matrix) a.</param>
        /// <param name="lda">Leading dimension of a.</param>
        /// <param name="x">Pointer to array (vector) x.</param>
        /// <param name="beta">Scalar beta.</param>
        /// <param name="y">Pointer to array (vector) y.</param>
        /// <param name="incx">Increment for the elements of x.</param>
        /// <param name="incy">Increment for the elements of y.</param>
        void Gemv(BLAS_Layout layout, BLAS_Transpose trans,
            long m, long n, double alpha, [In] double* a, long lda,
            [In] double* x, double beta, [In, Out] double* y,
            long incx = 1, long incy = 1);

        /// <summary>
        /// Computes a matrix-vector product using a general matrix (complex precision).
        /// y := alpha * op(a) * x + beta * y
        /// </summary>
        /// <param name="layout">Specifies array storage: row- or column-major.</param>
        /// <param name="trans">Specifies matrix a transpose operation.</param>
        /// <param name="m">Number of rows of the matrix a.</param>
        /// <param name="n">Number of columns of the matrix a.</param>
        /// <param name="alpha">Pointer to scalar alpha (complex).</param>
        /// <param name="a">Pointer to array (matrix) a (complex).</param>
        /// <param name="lda">Leading dimension of a.</param>
        /// <param name="x">Pointer to array (vector) x (complex).</param>
        /// <param name="beta">Pointer to scalar beta (complex).</param>
        /// <param name="y">Pointer to array (vector) y (complex).</param>
        /// <param name="incx">Increment for the elements of x.</param>
        /// <param name="incy">Increment for the elements of y.</param>
        void Gemv(BLAS_Layout layout, BLAS_Transpose trans,
            long m, long n, void* alpha, [In] void* a, long lda,
            [In] void* x, void* beta, [In, Out] void* y,
            long incx = 1, long incy = 1);

        #endregion
        #region ---- Gbmv [D/Z] ----

        /// <summary>
        /// Computes a matrix-vector product with a general band matrix (double precision).
        /// y := alpha * A * x + beta * y
        /// </summary>
        /// <param name="layout">Specifies array storage: row- or column-major.</param>
        /// <param name="trans">Specifies matrix A transpose operation.</param>
        /// <param name="m">Number of rows of the matrix A.</param>
        /// <param name="n">Number of columns of the matrix A.</param>
        /// <param name="kl">Number of sub-diagonals of the matrix A.</param>
        /// <param name="ku">Number of super-diagonals of the matrix A.</param>
        /// <param name="alpha">Scalar alpha.</param>
        /// <param name="a">Pointer to the band matrix A.</param>
        /// <param name="lda">Leading dimension of A.</param>
        /// <param name="x">Pointer to the vector x.</param>
        /// <param name="beta">Scalar beta.</param>
        /// <param name="y">Pointer to the vector y (result is stored here).</param>
        /// <param name="incx">Increment for the elements of x.</param>
        /// <param name="incy">Increment for the elements of y.</param>
        void Gbmv(BLAS_Layout layout, BLAS_Transpose trans,
            long m, long n, long kl, long ku,
            double alpha, [In] double* a, long lda, [In] double* x,
            double beta, [In, Out] double* y,
            long incx = 1, long incy = 1);

        /// <summary>
        /// Computes a matrix-vector product with a general band matrix (complex double precision).
        /// y := alpha * A * x + beta * y
        /// </summary>
        /// <param name="layout">Specifies array storage: row- or column-major.</param>
        /// <param name="trans">Specifies matrix A transpose operation.</param>
        /// <param name="m">Number of rows of the matrix A.</param>
        /// <param name="n">Number of columns of the matrix A.</param>
        /// <param name="kl">Number of sub-diagonals of the matrix A.</param>
        /// <param name="ku">Number of super-diagonals of the matrix A.</param>
        /// <param name="alpha">Pointer to the scalar alpha (complex).</param>
        /// <param name="a">Pointer to the band matrix A (complex).</param>
        /// <param name="lda">Leading dimension of A.</param>
        /// <param name="x">Pointer to the vector x (complex).</param>
        /// <param name="beta">Pointer to the scalar beta (complex).</param>
        /// <param name="y">Pointer to the vector y (complex, result is stored here).</param>
        /// <param name="incx">Increment for the elements of x.</param>
        /// <param name="incy">Increment for the elements of y.</param>
        void Gbmv(BLAS_Layout layout, BLAS_Transpose trans,
            long m, long n, long kl, long ku,
            void* alpha, [In] void* a, long lda, [In] void* x,
            void* beta, [In, Out] void* y,
            long incx = 1, long incy = 1);

        #endregion
        #region ---- Trmv [D/Z] ----

        /// <summary>
        /// Computes a matrix-vector product using a triangular matrix (double precision).
        /// x := op(A) * x
        /// </summary>
        /// <param name="layout">Specifies array storage: row- or column-major.</param>
        /// <param name="uplo">Specifies whether the matrix A is upper or lower triangular.</param>
        /// <param name="trans">Specifies matrix A transpose operation.</param>
        /// <param name="diag">Specifies whether the matrix A is unit triangular.</param>
        /// <param name="n">The order of the matrix A.</param>
        /// <param name="a">Pointer to the array (matrix) A.</param>
        /// <param name="lda">Leading dimension of A.</param>
        /// <param name="x">Pointer to the array (vector) x.</param>
        /// <param name="incx">Increment for the elements of x.</param>
        void Trmv(BLAS_Layout layout, BLAS_Uplo uplo,
            BLAS_Transpose trans, BLAS_Diag diag,
            long n, [In] double* a, long lda,
            [In, Out] double* x, long incx = 1);

        /// <summary>
        /// Computes a matrix-vector product using a triangular matrix (complex precision).
        /// x := op(A) * x
        /// </summary>
        /// <param name="layout">Specifies array storage: row- or column-major.</param>
        /// <param name="uplo">Specifies whether the matrix A is upper or lower triangular.</param>
        /// <param name="trans">Specifies matrix A transpose operation.</param>
        /// <param name="diag">Specifies whether the matrix A is unit triangular.</param>
        /// <param name="n">The order of the matrix A.</param>
        /// <param name="a">Pointer to the array (matrix) A (complex type).</param>
        /// <param name="lda">Leading dimension of A.</param>
        /// <param name="x">Pointer to the array (vector) x (complex type).</param>
        /// <param name="incx">Increment for the elements of x.</param>
        void Trmv(BLAS_Layout layout, BLAS_Uplo uplo,
            BLAS_Transpose trans, BLAS_Diag diag,
            long n, [In] void* a, long lda,
            [In, Out] void* x, long incx = 1);

        #endregion
        #region ---- Tbmv [D/Z] ----

        /// <summary>
        /// Computes a matrix-vector product using a triangular band matrix (double precision).
        /// x := op(A) * x
        /// </summary>
        /// <param name="layout">Specifies array storage: row- or column-major.</param>
        /// <param name="uplo">Specifies whether the matrix A is upper or lower triangular.</param>
        /// <param name="trans">Specifies matrix A transpose operation.</param>
        /// <param name="diag">Specifies whether the matrix A is unit triangular.</param>
        /// <param name="n">The order of the matrix A.</param>
        /// <param name="k">The number of super-diagonals of the matrix A.</param>
        /// <param name="a">Pointer to the matrix A.</param>
        /// <param name="lda">Leading dimension of A.</param>
        /// <param name="x">Pointer to the vector x (input/output).</param>
        /// <param name="incx">Increment for the elements of x.</param>
        void Tbmv(BLAS_Layout layout, BLAS_Uplo uplo,
            BLAS_Transpose trans, BLAS_Diag diag,
            long n, long k, [In] double* a, long lda,
            [In, Out] double* x, long incx = 1);

        /// <summary>
        /// Computes a matrix-vector product using a triangular band matrix (complex double precision).
        /// x := op(A) * x
        /// </summary>
        /// <param name="layout">Specifies array storage: row- or column-major.</param>
        /// <param name="uplo">Specifies whether the matrix A is upper or lower triangular.</param>
        /// <param name="trans">Specifies matrix A transpose operation.</param>
        /// <param name="diag">Specifies whether the matrix A is unit triangular.</param>
        /// <param name="n">The order of the matrix A.</param>
        /// <param name="k">The number of super-diagonals of the matrix A.</param>
        /// <param name="a">Pointer to the matrix A (complex values).</param>
        /// <param name="lda">Leading dimension of A.</param>
        /// <param name="x">Pointer to the vector x (complex, input/output).</param>
        /// <param name="incx">Increment for the elements of x.</param>
        void Tbmv(BLAS_Layout layout, BLAS_Uplo uplo,
            BLAS_Transpose trans, BLAS_Diag diag,
            long n, long k, [In] void* a, long lda,
            [In, Out] void* x, long incx = 1);

        #endregion
        #region ---- Tpmv [D/Z] ----

        /// <summary>
        /// Computes a matrix-vector product using a triangular packed matrix for double-precision values.
        /// x := op(A) * x,
        /// </summary>
        /// <param name="layout">Specifies array storage: row- or column-major.</param>
        /// <param name="uplo">Specifies whether the matrix A is upper or lower triangular.</param>
        /// <param name="trans">Specifies matrix A transpose operation.</param>
        /// <param name="diag">Specifies whether the matrix A is unit triangular.</param>
        /// <param name="n">The order of the matrix A.</param>
        /// <param name="k">The number of super-diagonals of the matrix A.</param>
        /// <param name="a">Pointer to the packed matrix A.</param>
        /// <param name="x">Pointer to the vector x (overwritten on exit).</param>
        /// <param name="incx">Increment for the elements of x.</param>
        void Tpmv(BLAS_Layout layout, BLAS_Uplo uplo,
            BLAS_Transpose trans, BLAS_Diag diag,
            long n, long k, [In] double* a,
            [In, Out] double* x, long incx = 1);

        /// <summary>
        /// Computes a matrix-vector product using a triangular packed matrix for complex values.
        /// x := op(A) * x,
        /// </summary>
        /// <param name="layout">Specifies array storage: row- or column-major.</param>
        /// <param name="uplo">Specifies whether the matrix A is upper or lower triangular.</param>
        /// <param name="trans">Specifies matrix A transpose operation.</param>
        /// <param name="diag">Specifies whether the matrix A is unit triangular.</param>
        /// <param name="n">The order of the matrix A.</param>
        /// <param name="k">The number of super-diagonals of the matrix A.</param>
        /// <param name="a">Pointer to the packed matrix A (complex values).</param>
        /// <param name="x">Pointer to the vector x (complex values, overwritten on exit).</param>
        /// <param name="incx">Increment for the elements of x.</param>
        void Tpmv(BLAS_Layout layout, BLAS_Uplo uplo,
            BLAS_Transpose trans, BLAS_Diag diag,
            long n, long k, [In] void* a,
            [In, Out] void* x, long incx = 1);

        #endregion
        #region ---- Trsv [D/Z] ----

        /// <summary>
        /// Solves a system of linear equations whose coefficients are in a triangular matrix.
        /// Solves op(A) * x = b for x, where A is a triangular matrix.
        /// </summary>
        /// <param name="layout">Specifies array storage: row- or column-major.</param>
        /// <param name="uplo">Specifies whether the matrix A is upper or lower triangular.</param>
        /// <param name="trans">Specifies matrix A transpose operation.</param>
        /// <param name="diag">Specifies whether the matrix A is unit triangular.</param>
        /// <param name="n">The order of the matrix A.</param>
        /// <param name="a">Pointer to the array (matrix) A.</param>
        /// <param name="lda">Leading dimension of A.</param>
        /// <param name="x">Pointer to the array (vector) x (right-hand side, overwritten by the solution).</param>
        /// <param name="incx">Increment for the elements of x.</param>
        void Trsv(BLAS_Layout layout, BLAS_Uplo uplo,
            BLAS_Transpose trans, BLAS_Diag diag,
            long n, [In] double* a, long lda,
            [In, Out] double* x, long incx = 1);

        /// <summary>
        /// Solves a system of linear equations whose coefficients are in a triangular matrix (complex version).
        /// Solves op(A) * x = b for x, where A is a triangular matrix.
        /// </summary>
        /// <param name="layout">Specifies array storage: row- or column-major.</param>
        /// <param name="uplo">Specifies whether the matrix A is upper or lower triangular.</param>
        /// <param name="trans">Specifies matrix A transpose operation.</param>
        /// <param name="diag">Specifies whether the matrix A is unit triangular.</param>
        /// <param name="n">The order of the matrix A.</param>
        /// <param name="a">Pointer to the array (matrix) A (complex values).</param>
        /// <param name="lda">Leading dimension of A.</param>
        /// <param name="x">Pointer to the array (vector) x (right-hand side, overwritten by the solution).</param>
        /// <param name="incx">Increment for the elements of x.</param>
        void Trsv(BLAS_Layout layout, BLAS_Uplo uplo,
            BLAS_Transpose trans, BLAS_Diag diag,
            long n, [In] void* a, long lda,
            [In, Out] void* x, long incx = 1);

        #endregion
        #region ---- Tbsv [D/Z] ----

        /// <summary>
        /// Solves a system of linear equations whose coefficients are in a triangular band matrix.
        /// The operation performed is op(A) * x = b, where op(A) is determined by <paramref name="trans"/>.
        /// </summary>
        /// <param name="layout">Specifies array storage: row- or column-major.</param>
        /// <param name="uplo">Specifies whether the matrix A is upper or lower triangular.</param>
        /// <param name="trans">Specifies the matrix A transpose operation.</param>
        /// <param name="diag">Specifies whether the matrix A is unit triangular.</param>
        /// <param name="n">The order of the matrix A.</param>
        /// <param name="k">The number of superdiagonals of the matrix A.</param>
        /// <param name="a">Pointer to the array containing the matrix A.</param>
        /// <param name="lda">Leading dimension of A.</param>
        /// <param name="x">Pointer to the array containing the vector x (right-hand side and solution).</param>
        /// <param name="incx">Increment for the elements of x.</param>
        void Tbsv(BLAS_Layout layout, BLAS_Uplo uplo,
            BLAS_Transpose trans, BLAS_Diag diag,
            long n, long k, [In] double* a, long lda,
            [In, Out] double* x, long incx = 1);

        /// <summary>
        /// Solves a system of linear equations whose coefficients are in a complex triangular band matrix.
        /// The operation performed is op(A) * x = b, where op(A) is determined by <paramref name="trans"/>.
        /// </summary>
        /// <param name="layout">Specifies array storage: row- or column-major.</param>
        /// <param name="uplo">Specifies whether the matrix A is upper or lower triangular.</param>
        /// <param name="trans">Specifies the matrix A transpose operation.</param>
        /// <param name="diag">Specifies whether the matrix A is unit triangular.</param>
        /// <param name="n">The order of the matrix A.</param>
        /// <param name="k">The number of superdiagonals of the matrix A.</param>
        /// <param name="a">Pointer to the array containing the complex matrix A.</param>
        /// <param name="lda">Leading dimension of A.</param>
        /// <param name="x">Pointer to the array containing the complex vector x (right-hand side and solution).</param>
        /// <param name="incx">Increment for the elements of x.</param>
        void Tbsv(BLAS_Layout layout, BLAS_Uplo uplo,
            BLAS_Transpose trans, BLAS_Diag diag,
            long n, long k, [In] void* a, long lda,
            [In, Out] void* x, long incx = 1);

        #endregion
        #region ---- Tpsv [D/Z] ----

        /// <summary>
        /// Solves a system of linear equations whose coefficients
        /// are in a triangular packed matrix <c>op(A) * x = b</c> for double-precision values.
        /// </summary>
        /// <param name="layout">Specifies array storage: row- or column-major.</param>
        /// <param name="uplo">Specifies whether the matrix A is upper or lower triangular.</param>
        /// <param name="trans">Specifies matrix A transpose operation.</param>
        /// <param name="diag">Specifies whether the matrix A is unit triangular.</param>
        /// <param name="n">The order of the matrix A.</param>
        /// <param name="a">Pointer to the packed triangular matrix A.</param>
        /// <param name="x">Pointer to the vector x (right-hand side, overwritten by the solution).</param>
        /// <param name="incx">Increment for the elements of x.</param>
        void Tpsv(BLAS_Layout layout, BLAS_Uplo uplo,
            BLAS_Transpose trans, BLAS_Diag diag,
            long n, [In] double* a,
            [In, Out] double* x, long incx = 1);

        /// <summary>
        /// Solves a system of linear equations whose coefficients
        /// are in a triangular packed matrix <c>op(A) * x = b</c> for complex values.
        /// </summary>
        /// <param name="layout">Specifies array storage: row- or column-major.</param>
        /// <param name="uplo">Specifies whether the matrix A is upper or lower triangular.</param>
        /// <param name="trans">Specifies matrix A transpose operation.</param>
        /// <param name="diag">Specifies whether the matrix A is unit triangular.</param>
        /// <param name="n">The order of the matrix A.</param>
        /// <param name="a">Pointer to the packed triangular matrix A (complex values).</param>
        /// <param name="x">Pointer to the vector x (right-hand side, overwritten by the solution, complex values).</param>
        /// <param name="incx">Increment for the elements of x.</param>
        void Tpsv(BLAS_Layout layout, BLAS_Uplo uplo,
            BLAS_Transpose trans, BLAS_Diag diag,
            long n, [In] void* a,
            [In, Out] void* x, long incx = 1);

        #endregion
        #region ---- Symv [D] ----

        /// <summary>
        /// Computes a matrix-vector product for a symmetric matrix using double-precision values.
        /// Performs the operation y := alpha * A * x + beta * y, where A is a symmetric matrix.
        /// </summary>
        /// <param name="layout">Specifies array storage: row- or column-major.</param>
        /// <param name="uplo">Specifies whether the matrix A is upper or lower triangular.</param>
        /// <param name="n">The order of the matrix A.</param>
        /// <param name="alpha">Scalar multiplier for A * x.</param>
        /// <param name="a">Pointer to the symmetric matrix A.</param>
        /// <param name="lda">Leading dimension of A.</param>
        /// <param name="x">Pointer to the input vector x.</param>
        /// <param name="beta">Scalar multiplier for y.</param>
        /// <param name="y">Pointer to the output vector y (overwritten on exit).</param>
        /// <param name="incx">Increment for the elements of x.</param>
        /// <param name="incy">Increment for the elements of y.</param>
        void Symv(BLAS_Layout layout, BLAS_Uplo uplo,
            long n, double alpha, [In] double* a, long lda,
            [In] double* x, double beta, [In, Out] double* y,
            long incx = 1, long incy = 1);

        #endregion
        #region ---- Sbmv [D] ----

        /// <summary>
        /// Computes a matrix-vector product for a symmetric band matrix.
        /// Performs the operation y := alpha * A * x + beta * y, where A is a symmetric band matrix.
        /// </summary>
        /// <param name="layout">Specifies array storage: row- or column-major.</param>
        /// <param name="uplo">Specifies whether the matrix A is upper or lower triangular.</param>
        /// <param name="n">The order of the matrix A.</param>
        /// <param name="k">The number of super-diagonals of the matrix A.</param>
        /// <param name="alpha">Scalar alpha.</param>
        /// <param name="a">Pointer to the array containing the symmetric band matrix A.</param>
        /// <param name="lda">Leading dimension of A.</param>
        /// <param name="x">Pointer to the array containing the vector x.</param>
        /// <param name="beta">Scalar beta.</param>
        /// <param name="y">Pointer to the array containing the vector y (result is stored here).</param>
        /// <param name="incx">Increment for the elements of x.</param>
        /// <param name="incy">Increment for the elements of y.</param>
        void Sbmv(BLAS_Layout layout, BLAS_Uplo uplo,
            long n, long k, double alpha, [In] double* a, long lda,
            [In] double* x, double beta, [In, Out] double* y,
            long incx = 1, long incy = 1);

        #endregion
        #region ---- Spmv [D] ----

        /// <summary>
        /// Computes a matrix-vector product for a symmetric packed matrix.
        /// Performs the operation y := alpha * A * x + beta * y, where A is a symmetric packed matrix.
        /// </summary>
        /// <param name="layout">Specifies array storage: row- or column-major.</param>
        /// <param name="uplo">Specifies whether the matrix A is upper or lower triangular.</param>
        /// <param name="n">The order of the matrix A.</param>
        /// <param name="alpha">Scalar alpha.</param>
        /// <param name="a">Pointer to the packed symmetric matrix A.</param>
        /// <param name="lda">Leading dimension of A.</param>
        /// <param name="x">Pointer to the input vector x.</param>
        /// <param name="beta">Scalar beta.</param>
        /// <param name="y">Pointer to the output vector y (overwritten on exit).</param>
        /// <param name="incx">Increment for the elements of x.</param>
        /// <param name="incy">Increment for the elements of y.</param>
        void Spmv(BLAS_Layout layout, BLAS_Uplo uplo,
            long n, double alpha, [In] double* a, long lda,
            [In] double* x, double beta, [In, Out] double* y,
            long incx = 1, long incy = 1);

        #endregion
        #region ---- Ger [D] ----

        /// <summary>
        /// Performs a rank-1 update of a general matrix using double-precision values.
        /// Computes A := alpha * x * y' + A, where A is an m-by-n matrix, x is a vector of length m, and y is a vector of length n.
        /// </summary>
        /// <param name="layout">Specifies array storage: row- or column-major.</param>
        /// <param name="m">Number of rows of the matrix A.</param>
        /// <param name="n">Number of columns of the matrix A.</param>
        /// <param name="alpha">Scalar multiplier for the rank-1 update.</param>
        /// <param name="x">Pointer to the vector x (length m).</param>
        /// <param name="y">Pointer to the vector y (length n).</param>
        /// <param name="a">Pointer to the matrix A (size m-by-n).</param>
        /// <param name="lda">Leading dimension of A.</param>
        /// <param name="incx">Increment for the elements of x. Default is 1.</param>
        /// <param name="incy">Increment for the elements of y. Default is 1.</param>
        void Ger(BLAS_Layout layout, long m, long n,
            double alpha, [In] double* x, [In] double* y,
            [In, Out] double* a, long lda,
            long incx = 1, long incy = 1);

        #endregion
        #region ---- Syr [D] ----

        /// <summary>
        /// Performs a rank-1 update of a symmetric matrix using double-precision values.
        /// Computes A := alpha * x * x' + A, where A is a symmetric matrix.
        /// </summary>
        /// <param name="layout">Specifies array storage: row- or column-major.</param>
        /// <param name="uplo">Specifies whether the matrix A is upper or lower triangular.</param>
        /// <param name="n">The order of the matrix A.</param>
        /// <param name="alpha">Scalar alpha.</param>
        /// <param name="x">Pointer to the vector x.</param>
        /// <param name="a">Pointer to the symmetric matrix A (overwritten on exit).</param>
        /// <param name="lda">Leading dimension of A.</param>
        /// <param name="incx">Increment for the elements of x (default is 1).</param>
        void Syr(BLAS_Layout layout, BLAS_Uplo uplo,
            long n, double alpha, [In] double* x,
            [In, Out] double* a, long lda,
            long incx = 1);

        #endregion
        #region ---- Spr [D] ----

        /// <summary>
        /// Performs a symmetric rank-1 update of a symmetric packed matrix.
        /// A := alpha * x * x' + A
        /// </summary>
        /// <param name="layout">Specifies array storage: row- or column-major.</param>
        /// <param name="uplo">Specifies whether the matrix a is upper or lower triangular.</param>
        /// <param name="n">Number of columns of the matrix a.</param>
        /// <param name="alpha">Scalar alpha.</param>
        /// <param name="x">Pointer to array (vector) x.</param>
        /// <param name="a">Pointer to array (matrix) a.</param>
        /// <param name="incx">Increment for the elements of x. Default is 1.</param>
        void Spr(BLAS_Layout layout, BLAS_Uplo uplo,
            long n, double alpha, [In] double* x,
            [In, Out] double* a,
            long incx = 1);

        #endregion
        #region ---- Syr2 [D] ----

        /// <summary>
        /// Performs a rank-2 update of a symmetric matrix.
        /// Computes A := alpha * x * y' + alpha * y * x' + A,
        /// where A is a symmetric matrix, x and y are vectors, and alpha is a scalar.
        /// </summary>
        /// <param name="layout">Specifies array storage: row- or column-major.</param>
        /// <param name="uplo">Specifies whether the matrix A is upper or lower triangular.</param>
        /// <param name="n">The order of the matrix A.</param>
        /// <param name="alpha">Scalar alpha.</param>
        /// <param name="x">Pointer to the first input vector x.</param>
        /// <param name="y">Pointer to the second input vector y.</param>
        /// <param name="a">Pointer to the symmetric matrix A (updated in place).</param>
        /// <param name="lda">Leading dimension of A.</param>
        /// <param name="incx">Increment for the elements of x (default is 1).</param>
        /// <param name="incy">Increment for the elements of y (default is 1).</param>
        void Syr2(BLAS_Layout layout, BLAS_Uplo uplo,
            long n, double alpha, [In] double* x, [In] double* y,
            [In, Out] double* a, long lda,
            long incx = 1, long incy = 1);

        #endregion
        #region ---- Spr2 [D] ----

        /// <summary>
        /// Performs a symmetric rank-2 update of a symmetric packed matrix.
        /// <para>
        /// A := alpha * x * y' + alpha * y * x' + A
        /// </para>
        /// </summary>
        /// <param name="layout">Specifies array storage: row- or column-major.</param>
        /// <param name="uplo">Specifies whether the matrix <paramref name="a"/> is upper or lower triangular.</param>
        /// <param name="n">The order of the matrix <paramref name="a"/>.</param>
        /// <param name="alpha">Scalar multiplier for the rank-2 update.</param>
        /// <param name="x">Pointer to the first input vector <c>x</c>.</param>
        /// <param name="y">Pointer to the second input vector <c>y</c>.</param>
        /// <param name="a">Pointer to the packed symmetric matrix <c>a</c> (updated in place).</param>
        /// <param name="incx">Increment for the elements of <paramref name="x"/> (default is 1).</param>
        /// <param name="incy">Increment for the elements of <paramref name="y"/> (default is 1).</param>
        void Spr2(BLAS_Layout layout, BLAS_Uplo uplo,
            long n, double alpha, [In] double* x, [In] double* y,
            [In, Out] double* a,
            long incx = 1, long incy = 1);

        #endregion
        #region ---- Hemv [Z] ----

        /// <summary>
        /// Computes a matrix-vector product using a Hermitian matrix.
        /// <para>y := alpha * A * x + beta * y</para>
        /// </summary>
        /// <param name="layout">Specifies array storage: row- or column-major.</param>
        /// <param name="uplo">Specifies whether the matrix A is upper or lower triangular.</param>
        /// <param name="n">The order of the matrix A.</param>
        /// <param name="alpha">Pointer to the scalar alpha.</param>
        /// <param name="a">Pointer to the Hermitian matrix A.</param>
        /// <param name="lda">Leading dimension of A.</param>
        /// <param name="x">Pointer to the vector x.</param>
        /// <param name="beta">Pointer to the scalar beta.</param>
        /// <param name="y">Pointer to the vector y.</param>
        /// <param name="incx">Increment for the elements of x.</param>
        /// <param name="incy">Increment for the elements of y.</param>
        void Hemv(BLAS_Layout layout, BLAS_Uplo uplo,
            long n, void* alpha, [In] void* a, long lda,
            [In] void* x, void* beta, [In, Out] void* y,
            long incx = 1, long incy = 1);

        #endregion
        #region ---- Hbmv [Z] ----

        /// <summary>
        /// Computes a matrix-vector product using a Hermitian band matrix.
        /// <para>y := alpha * A * x + beta * y</para>
        /// </summary>
        /// <param name="layout">Specifies array storage: row- or column-major.</param>
        /// <param name="uplo">Specifies whether the matrix A is upper or lower triangular.</param>
        /// <param name="n">The order of the matrix A.</param>
        /// <param name="k">The number of super-diagonals of the matrix A.</param>
        /// <param name="alpha">Pointer to the scalar alpha.</param>
        /// <param name="a">Pointer to the Hermitian band matrix A.</param>
        /// <param name="lda">Leading dimension of A.</param>
        /// <param name="x">Pointer to the vector x.</param>
        /// <param name="beta">Pointer to the scalar beta.</param>
        /// <param name="y">Pointer to the vector y (result).</param>
        /// <param name="incx">Increment for the elements of x.</param>
        /// <param name="incy">Increment for the elements of y.</param>
        void Hbmv(BLAS_Layout layout, BLAS_Uplo uplo,
            long n, long k, void* alpha, [In] void* a, long lda,
            [In] void* x, void* beta, [In, Out] void* y,
            long incx = 1, long incy = 1);

        #endregion
        #region ---- Hpmv [Z] ----

        /// <summary>
        /// Computes a matrix-vector product using a Hermitian packed matrix.
        /// Performs the operation y := alpha * A * x + beta * y, where A is a Hermitian packed matrix.
        /// </summary>
        /// <param name="layout">Specifies array storage: row- or column-major.</param>
        /// <param name="uplo">Specifies whether the matrix A is upper or lower triangular.</param>
        /// <param name="n">The order of the matrix A.</param>
        /// <param name="alpha">Pointer to the scalar alpha.</param>
        /// <param name="a">Pointer to the Hermitian packed matrix A.</param>
        /// <param name="x">Pointer to the vector x.</param>
        /// <param name="beta">Pointer to the scalar beta.</param>
        /// <param name="y">Pointer to the vector y (result).</param>
        /// <param name="incx">Increment for the elements of x. Default is 1.</param>
        /// <param name="incy">Increment for the elements of y. Default is 1.</param>
        void Hpmv(BLAS_Layout layout, BLAS_Uplo uplo,
            long n, void* alpha, [In] void* a,
            [In] void* x, void* beta, [In, Out] void* y,
            long incx = 1, long incy = 1);

        #endregion
        #region ---- Geru [Z] ----

        /// <summary>
        /// Performs a rank-1 update (unconjugated) of a general complex matrix.
        /// </summary>
        /// <param name="layout">Specifies array storage: row- or column-major.</param>
        /// <param name="m">Number of rows of the matrix <paramref name="a"/>.</param>
        /// <param name="n">Number of columns of the matrix <paramref name="a"/>.</param>
        /// <param name="alpha">Pointer to the scalar multiplier for the rank-1 update.</param>
        /// <param name="x">Pointer to the complex vector <c>x</c> of length at least <paramref name="m"/>.</param>
        /// <param name="y">Pointer to the complex vector <c>y</c> of length at least <paramref name="n"/>.</param>
        /// <param name="a">Pointer to the complex matrix <c>a</c> to be updated (size at least <paramref name="lda"/> × <paramref name="n"/>).</param>
        /// <param name="lda">Leading dimension of <paramref name="a"/>.</param>
        /// <param name="incx">Increment for the elements of <paramref name="x"/> (default is 1).</param>
        /// <param name="incy">Increment for the elements of <paramref name="y"/> (default is 1).</param>
        /// <remarks>
        /// The operation performed is: <c>A := alpha * x * y' + A</c>,
        /// where <c>y'</c> is the conjugate transpose of <c>y</c>.
        /// This method wraps the native Intel MKL cblas_zgeru_64 function.
        /// </remarks>
        void Geru(BLAS_Layout layout, long m, long n,
            void* alpha, [In] void* x, [In] void* y,
            [In, Out] void* a, long lda,
            long incx = 1, long incy = 1);

        #endregion
        #region ---- Gerc [Z] ----

        /// <summary>
        /// Performs a rank-1 update (conjugated) of a general matrix.
        /// Computes A := alpha * x * y' + A, where y' is the conjugate transpose of y.
        /// </summary>
        /// <param name="layout">Specifies array storage: row- or column-major.</param>
        /// <param name="m">Number of rows of the matrix a.</param>
        /// <param name="n">Number of columns of the matrix a.</param>
        /// <param name="alpha">Pointer to the scalar alpha (Complex*).</param>
        /// <param name="x">Pointer to the vector x (Complex*).</param>
        /// <param name="y">Pointer to the vector y (Complex*).</param>
        /// <param name="a">Pointer to the matrix a (Complex*), updated in place.</param>
        /// <param name="lda">Leading dimension of a.</param>
        /// <param name="incx">Increment for the elements of x.</param>
        /// <param name="incy">Increment for the elements of y.</param>
        void Gerc(BLAS_Layout layout, long m, long n,
            void* alpha, [In] void* x, [In] void* y,
            [In, Out] void* a, long lda,
            long incx = 1, long incy = 1);

        #endregion
        #region ---- Her [Z] ----

        /// <summary>
        /// Performs a rank-1 update of a Hermitian matrix using double-precision data.
        /// <para>
        /// A := alpha * x * conj(x') + A
        /// </para>
        /// </summary>
        /// <param name="layout">Specifies array storage: row- or column-major.</param>
        /// <param name="uplo">Specifies whether the matrix <paramref name="a"/> is upper or lower triangular.</param>
        /// <param name="n">The order of the matrix <paramref name="a"/>.</param>
        /// <param name="alpha">The real scalar multiplier for the rank-1 update.</param>
        /// <param name="x">Pointer to the input vector <c>x</c> (complex values, interleaved as double[]).</param>
        /// <param name="a">Pointer to the Hermitian matrix <c>a</c> (complex values, interleaved as double[]), updated in-place.</param>
        /// <param name="lda">The leading dimension of <paramref name="a"/>.</param>
        /// <param name="incx">Increment for the elements of <paramref name="x"/> (default is 1).</param>
        void Her(BLAS_Layout layout, BLAS_Uplo uplo,
            long n, double alpha, [In] double* x,
            [In, Out] double* a, long lda,
            long incx = 1);

        #endregion
        #region ---- Hpr [Z] ----

        /// <summary>
        /// Performs a rank-1 update of a Hermitian packed matrix.
        /// <para>A := alpha * x * conj(x') + A</para>
        /// </summary>
        /// <param name="layout">Specifies array storage: row- or column-major.</param>
        /// <param name="uplo">Specifies whether the matrix a is upper or lower triangular.</param>
        /// <param name="n">The order of the matrix a.</param>
        /// <param name="alpha">Scalar alpha.</param>
        /// <param name="x">Pointer to the input vector x.</param>
        /// <param name="a">Pointer to the packed Hermitian matrix a (updated in place).</param>
        /// <param name="incx">Increment for indexing x (default is 1).</param>
        void Hpr(BLAS_Layout layout, BLAS_Uplo uplo,
            long n, double alpha, [In] void* x,
            [In, Out] void* a,
            long incx = 1);

        #endregion
        #region ---- Her2 [Z] ----

        /// <summary>
        /// Performs a rank-2 update of a Hermitian matrix using complex double-precision data.
        /// <para>
        /// A := alpha * x * conjg(y') + conjg(alpha) * y * conjg(x') + A
        /// </para>
        /// </summary>
        /// <param name="layout">Specifies array storage: row- or column-major.</param>
        /// <param name="uplo">Specifies whether the matrix A is upper or lower triangular.</param>
        /// <param name="n">The order of the matrix A.</param>
        /// <param name="alpha">Pointer to the scalar alpha (complex double).</param>
        /// <param name="x">Pointer to the first vector x (complex double).</param>
        /// <param name="y">Pointer to the second vector y (complex double).</param>
        /// <param name="a">Pointer to the Hermitian matrix A (complex double), updated in-place.</param>
        /// <param name="lda">Leading dimension of the matrix A.</param>
        /// <param name="incx">Increment for the elements of x. Default is 1.</param>
        /// <param name="incy">Increment for the elements of y. Default is 1.</param>
        void Her2(BLAS_Layout layout, BLAS_Uplo uplo,
            long n, void* alpha, [In] void* x, [In] void* y,
            [In, Out] void* a, long lda,
            long incx = 1, long incy = 1);

        #endregion
        #region ---- Hpr2 [Z] ----

        /// <summary>
        /// Performs a rank-2 update of a Hermitian packed matrix.
        /// <para>
        /// A := alpha * x * conj(y') + conj(alpha) * y * conj(x') + A
        /// </para>
        /// </summary>
        /// <param name="layout">Specifies array storage: row- or column-major.</param>
        /// <param name="uplo">Specifies whether the matrix <paramref name="a"/> is upper or lower triangular.</param>
        /// <param name="n">The order of the matrix <paramref name="a"/>.</param>
        /// <param name="alpha">Pointer to the scalar alpha (complex value).</param>
        /// <param name="x">Pointer to the array (vector) x.</param>
        /// <param name="y">Pointer to the array (vector) y.</param>
        /// <param name="a">Pointer to the array (matrix) a (packed Hermitian matrix).</param>
        /// <param name="incx">Increment for the elements of <paramref name="x"/>. Default is 1.</param>
        /// <param name="incy">Increment for the elements of <paramref name="y"/>. Default is 1.</param>
        void Hpr2(BLAS_Layout layout, BLAS_Uplo uplo,
            long n, void* alpha, [In] void* x, [In] void* y,
            [In, Out] void* a,
            long incx = 1, long incy = 1);

        #endregion

        #endregion
        #region Level 3

        #region ---- Gemm [D/Z] ----

        /// <summary>
        /// Computes a matrix-matrix product with general matrices (double precision).
        /// C := alpha * op(A) * op(B) + beta * C
        /// </summary>
        /// <param name="layout">Specifies array storage: row- or column-major.</param>
        /// <param name="transa">Specifies matrix A transpose operation.</param>
        /// <param name="transb">Specifies matrix B transpose operation.</param>
        /// <param name="m">Number of rows of the matrix op(A).</param>
        /// <param name="n">Number of columns of the matrix op(B).</param>
        /// <param name="k">Number of columns of the matrix op(A).</param>
        /// <param name="alpha">Scalar alpha.</param>
        /// <param name="a">Pointer to array (matrix) A.</param>
        /// <param name="lda">Leading dimension of matrix A.</param>
        /// <param name="b">Pointer to array (matrix) B.</param>
        /// <param name="ldb">Leading dimension of matrix B.</param>
        /// <param name="beta">Scalar beta.</param>
        /// <param name="c">Pointer to array (matrix) C.</param>
        /// <param name="ldc">Leading dimension of matrix C.</param>
        void Gemm(BLAS_Layout layout,
            BLAS_Transpose transa, BLAS_Transpose transb,
            long m, long n, long k, double alpha, [In] double* a, long lda,
            [In] double* b, long ldb, double beta,
            [In, Out] double* c, long ldc);

        /// <summary>
        /// Computes a matrix-matrix product with general matrices (complex double precision).
        /// C := alpha * op(A) * op(B) + beta * C
        /// </summary>
        /// <param name="layout">Specifies array storage: row- or column-major.</param>
        /// <param name="transa">Specifies matrix A transpose operation.</param>
        /// <param name="transb">Specifies matrix B transpose operation.</param>
        /// <param name="m">Number of rows of the matrix op(A).</param>
        /// <param name="n">Number of columns of the matrix op(B).</param>
        /// <param name="k">Number of columns of the matrix op(A).</param>
        /// <param name="alpha">Pointer to scalar alpha (complex).</param>
        /// <param name="a">Pointer to array (matrix) A (complex).</param>
        /// <param name="lda">Leading dimension of matrix A.</param>
        /// <param name="b">Pointer to array (matrix) B (complex).</param>
        /// <param name="ldb">Leading dimension of matrix B.</param>
        /// <param name="beta">Pointer to scalar beta (complex).</param>
        /// <param name="c">Pointer to array (matrix) C (complex).</param>
        /// <param name="ldc">Leading dimension of matrix C.</param>
        void Gemm(BLAS_Layout layout,
            BLAS_Transpose transa, BLAS_Transpose transb,
            long m, long n, long k, void* alpha, [In] void* a, long lda,
            [In] void* b, long ldb, void* beta,
            [In, Out] void* c, long ldc);

        #endregion
        #region ---- Symm [D/Z] ----

        /// <summary>
        /// Computes a matrix-matrix product where one input matrix is symmetric.
        /// C := alpha * A * B + beta * C, or
        /// C := alpha * B * A + beta * C
        /// </summary>
        /// <param name="layout">Specifies array storage: row- or column-major.</param>
        /// <param name="side">Specifies whether the symmetric matrix A appears on the left or right in the operation.</param>
        /// <param name="uplo">Specifies whether the upper or lower triangular part of the symmetric matrix A is used.</param>
        /// <param name="m">Number of rows of the matrix op(A).</param>
        /// <param name="n">Number of columns of the matrix op(B).</param>
        /// <param name="alpha">Scalar alpha.</param>
        /// <param name="a">Pointer to the symmetric matrix A.</param>
        /// <param name="lda">Leading dimension of matrix A.</param>
        /// <param name="b">Pointer to matrix B.</param>
        /// <param name="ldb">Leading dimension of matrix B.</param>
        /// <param name="beta">Scalar beta.</param>
        /// <param name="c">Pointer to matrix C (result).</param>
        /// <param name="ldc">Leading dimension of matrix C.</param>
        void Symm(BLAS_Layout layout,
            BLAS_Side side, BLAS_Uplo uplo,
            long m, long n, double alpha, [In] double* a, long lda,
            [In] double* b, long ldb, double beta,
            [In, Out] double* c, long ldc);

        /// <summary>
        /// Computes a matrix-matrix product where one input matrix is symmetric (complex version).
        /// C := alpha * A * B + beta * C, or
        /// C := alpha * B * A + beta * C
        /// </summary>
        /// <param name="layout">Specifies array storage: row- or column-major.</param>
        /// <param name="side">Specifies whether the symmetric matrix A appears on the left or right in the operation.</param>
        /// <param name="uplo">Specifies whether the upper or lower triangular part of the symmetric matrix A is used.</param>
        /// <param name="m">Number of rows of the matrix op(A).</param>
        /// <param name="n">Number of columns of the matrix op(B).</param>
        /// <param name="alpha">Pointer to scalar alpha (complex).</param>
        /// <param name="a">Pointer to the symmetric matrix A (complex).</param>
        /// <param name="lda">Leading dimension of matrix A.</param>
        /// <param name="b">Pointer to matrix B (complex).</param>
        /// <param name="ldb">Leading dimension of matrix B.</param>
        /// <param name="beta">Pointer to scalar beta (complex).</param>
        /// <param name="c">Pointer to matrix C (complex, result).</param>
        /// <param name="ldc">Leading dimension of matrix C.</param>
        void Symm(BLAS_Layout layout,
            BLAS_Side side, BLAS_Uplo uplo,
            long m, long n, void* alpha, [In] void* a, long lda,
            [In] void* b, long ldb, void* beta,
            [In, Out] void* c, long ldc);

        #endregion
        #region ---- Syrk [D/Z] ----

        /// <summary>
        /// Performs a symmetric rank-k update for double-precision matrices.
        /// Computes C := alpha * A * A' + beta * C, or C := alpha * A' * A + beta * C.
        /// </summary>
        /// <param name="layout">Specifies array storage: row- or column-major.</param>
        /// <param name="uplo">Specifies whether the upper or lower triangular part of the symmetric matrix is used.</param>
        /// <param name="trans">Specifies matrix A transpose operation.</param>
        /// <param name="n">Number of columns of the matrix op(B).</param>
        /// <param name="k">The number of columns/rows (NoTrans/Trans) of the matrix A.</param>
        /// <param name="alpha">Scalar alpha.</param>
        /// <param name="a">Pointer to the input matrix A.</param>
        /// <param name="lda">Leading dimension of matrix A.</param>
        /// <param name="beta">Scalar beta.</param>
        /// <param name="c">Pointer to the output matrix C.</param>
        /// <param name="ldc">Leading dimension of matrix C.</param>
        void Syrk(BLAS_Layout layout,
            BLAS_Uplo uplo, BLAS_Transpose trans,
            long n, long k, double alpha, [In] double* a, long lda,
            double beta, [In, Out] double* c, long ldc);

        /// <summary>
        /// Performs a symmetric rank-k update for complex double-precision matrices.
        /// Computes C := alpha * A * A' + beta * C, or C := alpha * A' * A + beta * C.
        /// </summary>
        /// <param name="layout">Specifies array storage: row- or column-major.</param>
        /// <param name="uplo">Specifies whether the upper or lower triangular part of the symmetric matrix is used.</param>
        /// <param name="trans">Specifies matrix A transpose operation.</param>
        /// <param name="n">Number of columns of the matrix op(B).</param>
        /// <param name="k">The number of columns/rows (NoTrans/Trans) of the matrix A.</param>
        /// <param name="alpha">Pointer to the scalar alpha (complex).</param>
        /// <param name="a">Pointer to the input matrix A (complex).</param>
        /// <param name="lda">Leading dimension of matrix A.</param>
        /// <param name="beta">Pointer to the scalar beta (complex).</param>
        /// <param name="c">Pointer to the output matrix C (complex).</param>
        /// <param name="ldc">Leading dimension of matrix C.</param>
        void Syrk(BLAS_Layout layout,
            BLAS_Uplo uplo, BLAS_Transpose trans,
            long n, long k, void* alpha, [In] void* a, long lda,
            void* beta, [In, Out] void* c, long ldc);

        #endregion
        #region ---- Syr2k [D/Z] ----

        /// <summary>
        /// Performs a symmetric rank-2k update for double-precision matrices.
        /// C := alpha * A * B' + alpha * B * A' + beta * C, or
        /// C := alpha * A' * B + alpha * B' * A + beta * C
        /// </summary>
        /// <param name="layout">Specifies array storage: row- or column-major.</param>
        /// <param name="uplo">Specifies whether the upper or lower triangular part of the symmetric matrix C is used.</param>
        /// <param name="trans">Specifies matrix transpose operation.</param>
        /// <param name="n">Number of columns of the matrix C.</param>
        /// <param name="k">Number of columns/rows (NoTrans/Trans) of the matrices A and B.</param>
        /// <param name="alpha">Scalar alpha.</param>
        /// <param name="a">Pointer to matrix A.</param>
        /// <param name="lda">Leading dimension of matrix A.</param>
        /// <param name="b">Pointer to matrix B.</param>
        /// <param name="ldb">Leading dimension of matrix B.</param>
        /// <param name="beta">Scalar beta.</param>
        /// <param name="c">Pointer to matrix C.</param>
        /// <param name="ldc">Leading dimension of matrix C.</param>
        void Syr2k(BLAS_Layout layout,
            BLAS_Uplo uplo, BLAS_Transpose trans,
            long n, long k, double alpha, [In] double* a, long lda,
            [In] double* b, long ldb, double beta,
            [In, Out] double* c, long ldc);

        /// <summary>
        /// Performs a symmetric rank-2k update for complex double-precision matrices.
        /// C := alpha * A * B' + alpha * B * A' + beta * C, or
        /// C := alpha * A' * B + alpha * B' * A + beta * C
        /// </summary>
        /// <param name="layout">Specifies array storage: row- or column-major.</param>
        /// <param name="uplo">Specifies whether the upper or lower triangular part of the symmetric matrix C is used.</param>
        /// <param name="trans">Specifies matrix transpose operation.</param>
        /// <param name="n">Number of columns of the matrix C.</param>
        /// <param name="k">Number of columns/rows (NoTrans/Trans) of the matrices A and B.</param>
        /// <param name="alpha">Pointer to scalar alpha (complex).</param>
        /// <param name="a">Pointer to matrix A (complex).</param>
        /// <param name="lda">Leading dimension of matrix A.</param>
        /// <param name="b">Pointer to matrix B (complex).</param>
        /// <param name="ldb">Leading dimension of matrix B.</param>
        /// <param name="beta">Pointer to scalar beta (complex).</param>
        /// <param name="c">Pointer to matrix C (complex).</param>
        /// <param name="ldc">Leading dimension of matrix C.</param>
        void Syr2k(BLAS_Layout layout,
            BLAS_Uplo uplo, BLAS_Transpose trans,
            long n, long k, void* alpha, [In] void* a, long lda,
            [In] void* b, long ldb, void* beta,
            [In, Out] void* c, long ldc);

        #endregion
        #region ---- Trmm [D/Z] ----

        /// <summary>
        /// Computes a matrix-matrix product where one input matrix is triangular.
        /// B := alpha * op(A) * B, or B := alpha * B * op(A)
        /// </summary>
        /// <param name="layout">Specifies array storage: row- or column-major.</param>
        /// <param name="side">Specifies whether the triangular matrix A appears on the left or right in the operation.</param>
        /// <param name="uplo">Specifies whether the upper or lower triangular part of the matrix A is used.</param>
        /// <param name="trans">Specifies matrix A transpose operation.</param>
        /// <param name="diag">Specifies whether the matrix A is unit triangular.</param>
        /// <param name="m">Number of rows of the matrix op(A).</param>
        /// <param name="n">Number of columns of the matrix op(B).</param>
        /// <param name="alpha">Scalar alpha.</param>
        /// <param name="a">Pointer to array (matrix) A.</param>
        /// <param name="lda">Leading dimension of matrix A.</param>
        /// <param name="b">Pointer to array (matrix) B.</param>
        /// <param name="ldb">Leading dimension of matrix B.</param>
        void Trmm(BLAS_Layout layout, BLAS_Side side,
            BLAS_Uplo uplo, BLAS_Transpose trans, BLAS_Diag diag,
            long m, long n, double alpha, [In] double* a, long lda,
            [In, Out] double* b, long ldb);

        /// <summary>
        /// Computes a matrix-matrix product where one input matrix is triangular (complex version).
        /// B := alpha * op(A) * B, or B := alpha * B * op(A)
        /// </summary>
        /// <param name="layout">Specifies array storage: row- or column-major.</param>
        /// <param name="side">Specifies whether the triangular matrix A appears on the left or right in the operation.</param>
        /// <param name="uplo">Specifies whether the upper or lower triangular part of the matrix A is used.</param>
        /// <param name="trans">Specifies matrix A transpose operation.</param>
        /// <param name="diag">Specifies whether the matrix A is unit triangular.</param>
        /// <param name="m">Number of rows of the matrix op(A).</param>
        /// <param name="n">Number of columns of the matrix op(B).</param>
        /// <param name="alpha">Pointer to scalar alpha (complex).</param>
        /// <param name="a">Pointer to array (matrix) A (complex).</param>
        /// <param name="lda">Leading dimension of matrix A.</param>
        /// <param name="b">Pointer to array (matrix) B (complex).</param>
        /// <param name="ldb">Leading dimension of matrix B.</param>
        void Trmm(BLAS_Layout layout, BLAS_Side side,
            BLAS_Uplo uplo, BLAS_Transpose trans, BLAS_Diag diag,
            long m, long n, void* alpha, [In] void* a, long lda,
            [In, Out] void* b, long ldb);

        #endregion
        #region ---- Trsm [D/Z] ----

        /// <summary>
        /// Computes the solution to a triangular matrix equation for double-precision matrices.
        /// Solves op(A) * X = alpha * B or X * op(A) = alpha * B, where A is a triangular matrix.
        /// </summary>
        /// <param name="layout">Specifies array storage: row- or column-major.</param>
        /// <param name="side">Specifies whether the triangular matrix A appears on the left or right in the operation.</param>
        /// <param name="uplo">Specifies whether the matrix A is upper or lower triangular.</param>
        /// <param name="trans">Specifies matrix A transpose operation.</param>
        /// <param name="diag">Specifies whether the matrix A is unit triangular.</param>
        /// <param name="m">Number of rows of the matrix op(A).</param>
        /// <param name="n">Number of columns of the matrix op(B).</param>
        /// <param name="alpha">Scalar alpha.</param>
        /// <param name="a">Pointer to the array (matrix) A.</param>
        /// <param name="lda">Leading dimension of matrix A.</param>
        /// <param name="b">Pointer to the array (matrix) B.</param>
        /// <param name="ldb">Leading dimension of matrix B.</param>
        void Trsm(BLAS_Layout layout, BLAS_Side side,
            BLAS_Uplo uplo, BLAS_Transpose trans, BLAS_Diag diag,
            long m, long n, double alpha, [In] double* a, long lda,
            [In, Out] double* b, long ldb);

        /// <summary>
        /// Computes the solution to a triangular matrix equation for complex double-precision matrices.
        /// Solves op(A) * X = alpha * B or X * op(A) = alpha * B, where A is a triangular matrix.
        /// </summary>
        /// <param name="layout">Specifies array storage: row- or column-major.</param>
        /// <param name="side">Specifies whether the triangular matrix A appears on the left or right in the operation.</param>
        /// <param name="uplo">Specifies whether the matrix A is upper or lower triangular.</param>
        /// <param name="trans">Specifies matrix A transpose operation.</param>
        /// <param name="diag">Specifies whether the matrix A is unit triangular.</param>
        /// <param name="m">Number of rows of the matrix op(A).</param>
        /// <param name="n">Number of columns of the matrix op(B).</param>
        /// <param name="alpha">Pointer to the scalar alpha (complex).</param>
        /// <param name="a">Pointer to the array (matrix) A (complex).</param>
        /// <param name="lda">Leading dimension of matrix A.</param>
        /// <param name="b">Pointer to the array (matrix) B (complex).</param>
        /// <param name="ldb">Leading dimension of matrix B.</param>
        void Trsm(BLAS_Layout layout, BLAS_Side side,
            BLAS_Uplo uplo, BLAS_Transpose trans, BLAS_Diag diag,
            long m, long n, void* alpha, [In] void* a, long lda,
            [In, Out] double* b, long ldb);

        #endregion
        #region ---- Hemm [Z] ----

        /// <summary>
        /// Computes a matrix-matrix product where one input matrix is Hermitian.
        /// C := alpha * A * B + beta * C, or
        /// C := alpha * B * A + beta * C
        /// </summary>
        /// <param name="layout">Specifies array storage: row- or column-major.</param>
        /// <param name="side">Specifies whether the Hermitian matrix A appears on the left or right in the operation.</param>
        /// <param name="uplo">Specifies whether the upper or lower triangular part of the Hermitian matrix A is used.</param>
        /// <param name="m">Number of rows of the matrix op(A).</param>
        /// <param name="n">Number of columns of the matrix op(B).</param>
        /// <param name="alpha">Pointer to the scalar alpha.</param>
        /// <param name="a">Pointer to the Hermitian matrix A.</param>
        /// <param name="lda">Leading dimension of matrix A.</param>
        /// <param name="b">Pointer to the matrix B.</param>
        /// <param name="ldb">Leading dimension of matrix B.</param>
        /// <param name="beta">Pointer to the scalar beta.</param>
        /// <param name="c">Pointer to the matrix C (result).</param>
        /// <param name="ldc">Leading dimension of matrix C.</param>
        void Hemm(BLAS_Layout layout, BLAS_Side side, BLAS_Uplo uplo,
            long m, long n, void* alpha, [In] void* a, long lda,
            [In] void* b, long ldb, void* beta,
            [In, Out] void* c, long ldc);

        #endregion
        #region ---- Herk [Z] ----

        /// <summary>
        /// Performs a Hermitian rank-k update operation on a complex matrix.
        /// <para>
        /// C := alpha * A * A<sup>H</sup> + beta * C, or
        /// C := alpha * A<sup>H</sup> * A + beta * C
        /// </para>
        /// </summary>
        /// <param name="layout">Specifies array storage: row- or column-major.</param>
        /// <param name="uplo">Specifies whether the upper or lower triangular part of the Hermitian matrix C is used.</param>
        /// <param name="trans">Specifies the operation applied to matrix A.</param>
        /// <param name="n">The order of the matrix C (number of rows and columns).</param>
        /// <param name="k">If <paramref name="trans"/> is NoTrans, the number of columns of A; otherwise, the number of rows of A.</param>
        /// <param name="alpha">Scaling factor for the rank-k product.</param>
        /// <param name="a">Pointer to the input matrix A.</param>
        /// <param name="lda">Leading dimension of matrix A.</param>
        /// <param name="beta">Scaling factor for matrix C.</param>
        /// <param name="c">Pointer to the Hermitian matrix C (updated in place).</param>
        /// <param name="ldc">Leading dimension of matrix C.</param>
        void Herk(BLAS_Layout layout, BLAS_Uplo uplo, BLAS_Transpose trans,
            long n, long k, double alpha, [In] void* a, long lda,
            double beta, [In, Out] void* c, long ldc);

        #endregion
        #region ---- Her2k [Z] ----

        /// <summary>
        /// Performs a Hermitian rank-2k update for complex double-precision matrices.
        /// <para>
        /// C := alpha * A * Bᴴ + conj(alpha) * B * Aᴴ + beta * C, or
        /// C := alpha * Aᴴ * B + conj(alpha) * Bᴴ * A + beta * C
        /// </para>
        /// </summary>
        /// <param name="layout">Specifies array storage: row- or column-major.</param>
        /// <param name="uplo">Specifies whether the upper or lower triangular part of the Hermitian matrix C is used.</param>
        /// <param name="trans">Specifies the operation applied to matrices A and B.</param>
        /// <param name="n">The order of the matrix C (number of rows and columns).</param>
        /// <param name="k">The number of columns (if not transposed) or rows (if transposed) of matrices A and B.</param>
        /// <param name="alpha">Pointer to the scalar alpha (complex double).</param>
        /// <param name="a">Pointer to the matrix A (complex double).</param>
        /// <param name="lda">Leading dimension of matrix A.</param>
        /// <param name="b">Pointer to the matrix B (complex double).</param>
        /// <param name="ldb">Leading dimension of matrix B.</param>
        /// <param name="beta">Pointer to the scalar beta (complex double).</param>
        /// <param name="c">Pointer to the Hermitian matrix C (complex double), updated in place.</param>
        /// <param name="ldc">Leading dimension of matrix C.</param>
        void Her2k(BLAS_Layout layout, BLAS_Uplo uplo, BLAS_Transpose trans,
            long n, long k, void* alpha, [In] void* a, long lda,
            void* b, long ldb, void* beta, [In, Out] void* c, long ldc);

        #endregion

        #endregion
        #region extensions

        #region ---- Axpby [D/Z] ----

        /// <summary>
        /// Computes a scaled vector addition with two scalars.
        /// Performs the operation y := a*x + b*y for double-precision arrays.
        /// </summary>
        /// <param name="n">Number of elements in the vectors.</param>
        /// <param name="a">Scalar multiplier for x.</param>
        /// <param name="x">Pointer to the input vector x.</param>
        /// <param name="b">Scalar multiplier for y.</param>
        /// <param name="y">Pointer to the input/output vector y.</param>
        /// <param name="incx">Increment for the elements of x (default is 1).</param>
        /// <param name="incy">Increment for the elements of y (default is 1).</param>
        void Axpby(long n, double a, [In] double* x,
            double b, [In, Out] double* y,
            long incx = 1, long incy = 1);

        /// <summary>
        /// Computes a scaled vector addition with two scalars for complex arrays.
        /// Performs the operation y := a*x + b*y for complex vectors.
        /// </summary>
        /// <param name="n">Number of elements in the vectors.</param>
        /// <param name="a">Pointer to the scalar multiplier for x (complex).</param>
        /// <param name="x">Pointer to the input vector x (complex).</param>
        /// <param name="b">Pointer to the scalar multiplier for y (complex).</param>
        /// <param name="y">Pointer to the input/output vector y (complex).</param>
        /// <param name="incx">Increment for the elements of x (default is 1).</param>
        /// <param name="incy">Increment for the elements of y (default is 1).</param>
        void Axpby(long n, void* a, [In] void* x,
            void* b, [In, Out] void* y,
            long incx = 1, long incy = 1);

        #endregion
        #region ---- Gemm3m [Z] ----

        /// <summary>
        /// Computes a matrix-matrix product for complex matrices using the 3m algorithm.
        /// C := alpha * op(A) * op(B) + beta * C
        /// </summary>
        /// <param name="layout">Specifies array storage: row- or column-major.</param>
        /// <param name="transa">Specifies matrix A transpose operation.</param>
        /// <param name="transb">Specifies matrix B transpose operation.</param>
        /// <param name="m">Number of rows of the matrix op(A).</param>
        /// <param name="n">Number of columns of the matrix op(B).</param>
        /// <param name="k">Number of columns of the matrix op(A) and rows of op(B).</param>
        /// <param name="alpha">Pointer to the scalar alpha (complex).</param>
        /// <param name="a">Pointer to the input matrix A (complex).</param>
        /// <param name="lda">Leading dimension of matrix A.</param>
        /// <param name="b">Pointer to the input matrix B (complex).</param>
        /// <param name="ldb">Leading dimension of matrix B.</param>
        /// <param name="beta">Pointer to the scalar beta (complex).</param>
        /// <param name="c">Pointer to the output matrix C (complex), overwritten on exit.</param>
        /// <param name="ldc">Leading dimension of matrix C.</param>
        void Gemm3m(BLAS_Layout layout,
            BLAS_Transpose transa, BLAS_Transpose transb,
            long m, long n, long k, void* alpha,
            [In] void* a, long lda, [In] void* b, long ldb,
            void* beta, [In, Out] void* c, long ldc);

        #endregion
        #region ---- Imatcopy ----

        /// <summary>
        /// Performs in-place scaling and transposition/copying of a double-precision matrix.
        /// </summary>
        /// <param name="layout">Specifies the layout of the input matrix (row-major or column-major).</param>
        /// <param name="operation">Specifies the operation to perform (e.g., transpose).</param>
        /// <param name="rows">Number of rows before the operation.</param>
        /// <param name="cols">Number of columns before the operation.</param>
        /// <param name="alpha">Scaling factor applied to the matrix elements.</param>
        /// <param name="ab">Pointer to the matrix data (overwritten on exit).</param>
        /// <param name="lda">Leading dimension of the matrix before the operation.</param>
        /// <param name="ldb">Leading dimension of the matrix after the operation.</param>
        void ImatCopy(BLAS_Layout layout, BLAS_Transpose operation,
            long rows, long cols, double alpha,
            [In, Out] double* ab, long lda, long ldb);

        /// <summary>
        /// Performs in-place scaling and transposition/copying of a complex-precision matrix.
        /// </summary>
        /// <param name="layout">Specifies the layout of the input matrix (row-major or column-major).</param>
        /// <param name="operation">Specifies the operation to perform (e.g., transpose).</param>
        /// <param name="rows">Number of rows before the operation.</param>
        /// <param name="cols">Number of columns before the operation.</param>
        /// <param name="alpha">Pointer to the scaling factor (complex) applied to the matrix elements.</param>
        /// <param name="ab">Pointer to the matrix data (overwritten on exit).</param>
        /// <param name="lda">Leading dimension of the matrix before the operation.</param>
        /// <param name="ldb">Leading dimension of the matrix after the operation.</param>
        void ImatCopy(BLAS_Layout layout, BLAS_Transpose operation,
            long rows, long cols, void* alpha,
            [In, Out] void* ab, long lda, long ldb);

        #endregion
        #region ---- Omatcopy ----

        /// <summary>
        /// Performs scaling and out-place transposition/copying of matrices for double-precision values.
        /// </summary>
        /// <param name="layout">Layout of the input matrix (row-major or column-major).</param>
        /// <param name="operation">Specifies whether to transpose the matrix.</param>
        /// <param name="rows">Number of rows before the operation.</param>
        /// <param name="cols">Number of columns before the operation.</param>
        /// <param name="alpha">Scaling factor alpha.</param>
        /// <param name="a">Pointer to the input matrix data before the operation.</param>
        /// <param name="lda">Leading dimension of the input matrix before the operation.</param>
        /// <param name="b">Pointer to the output matrix data after the operation.</param>
        /// <param name="ldb">Leading dimension of the output matrix after the operation.</param>
        void OmatCopy(BLAS_Layout layout, BLAS_Transpose operation,
            long rows, long cols, double alpha, [In] double* a, long lda,
            [Out] double* b, long ldb);

        /// <summary>
        /// Performs scaling and out-place transposition/copying of matrices for complex values.
        /// </summary>
        /// <param name="layout">Layout of the input matrix (row-major or column-major).</param>
        /// <param name="operation">Specifies whether to transpose the matrix.</param>
        /// <param name="rows">Number of rows before the operation.</param>
        /// <param name="cols">Number of columns before the operation.</param>
        /// <param name="alpha">Pointer to the scaling factor alpha (complex).</param>
        /// <param name="a">Pointer to the input matrix data before the operation (complex).</param>
        /// <param name="lda">Leading dimension of the input matrix before the operation.</param>
        /// <param name="b">Pointer to the output matrix data after the operation (complex).</param>
        /// <param name="ldb">Leading dimension of the output matrix after the operation.</param>
        void OmatCopy(BLAS_Layout layout, BLAS_Transpose operation,
            long rows, long cols, void* alpha, [In] void* a, long lda,
            [Out] void* b, long ldb);

        #endregion

        #endregion
    }

}
