using System.Runtime.InteropServices;
using Complex = System.Numerics.Complex;

namespace VEMS.MathCore
{
    /// <summary>
    /// LAPACK interface
    /// </summary>
    public unsafe interface ILAPACK
    {
        #region ---- Getrf [D/Z] ----

        /// <summary>
        /// Computes the LU factorization of a general m-by-n matrix (double precision).
        /// A = P*L*U, where P is a permutation matrix, 
        /// L is lower triangular with unit diagonal elements (lower trapezoidal if m>n),
        /// and U is upper triangular (upper trapezoidal if n>m).
        /// </summary>
        /// <param name="layout">Matrix storage layout (row or column major).</param>
        /// <param name="m">Number of rows in matrix <paramref name="a"/>.</param>
        /// <param name="n">Number of columns in matrix <paramref name="a"/>.</param>
        /// <param name="a">Pointer to the matrix <paramref name="a"/> (overwritten by L and U on exit).</param>
        /// <param name="lda">Leading dimension of <paramref name="a"/>.</param>
        /// <param name="ipiv">Pointer to the pivot indices.</param>
        /// <returns>Status code from the native LAPACK routine.</returns>
        long Getrf(LAPACK_Layout layout, long m, long n,
            [In, Out] double* a, long lda, [Out] long* ipiv);

        /// <summary>
        /// Computes the LU factorization of a general m-by-n matrix (complex double precision).
        /// A = P*L*U, where P is a permutation matrix, 
        /// L is lower triangular with unit diagonal elements (lower trapezoidal if m>n),
        /// and U is upper triangular (upper trapezoidal if n>m).
        /// </summary>
        /// <param name="layout">Matrix storage layout (row or column major).</param>
        /// <param name="m">Number of rows in matrix <paramref name="a"/>.</param>
        /// <param name="n">Number of columns in matrix <paramref name="a"/>.</param>
        /// <param name="a">Pointer to the matrix <paramref name="a"/> (overwritten by L and U on exit, complex values).</param>
        /// <param name="lda">Leading dimension of <paramref name="a"/>.</param>
        /// <param name="ipiv">Pointer to the pivot indices.</param>
        /// <returns>Status code from the native LAPACK routine.</returns>
        long Getrf(LAPACK_Layout layout, long m, long n,
            [In, Out] void* a, long lda, [Out] long* ipiv);

        #endregion
        #region ---- Getrs [D/Z] ----

        /// <summary>
        /// Solves a system of linear equations A * X = B, A^T * X = B, or A^H * X = B with a general N-by-N matrix A using the LU factorization computed by <c>Getrf</c>.
        /// </summary>
        /// <param name="layout">Specifies the matrix layout (row-major or column-major).</param>
        /// <param name="trans">Specifies the form of the system of equations (no transpose, transpose, or conjugate transpose).</param>
        /// <param name="n">The order of the matrix A (number of linear equations, i.e., the number of rows and columns in A).</param>
        /// <param name="nrhs">The number of right-hand sides, i.e., the number of columns of the matrix B.</param>
        /// <param name="a">Pointer to the LU-factored matrix A as computed by <c>Getrf</c>.</param>
        /// <param name="lda">The leading dimension of the array A.</param>
        /// <param name="ipiv">Pointer to the pivot indices as returned by <c>Getrf</c>.</param>
        /// <param name="b">Pointer to the right-hand side matrix B (input) and the solution matrix X (output).</param>
        /// <param name="ldb">The leading dimension of the array B.</param>
        void Getrs(LAPACK_Layout layout, LAPACK_Transpose trans,
            long n, long nrhs, [In] double* a, long lda,
            [In] long* ipiv, [In, Out] double* b, long ldb);

        /// <summary>
        /// Solves a system of linear equations A * X = B, A^T * X = B, or A^H * X = B with a general N-by-N complex matrix A using the LU factorization computed by <c>Getrf</c>.
        /// </summary>
        /// <param name="layout">Specifies the matrix layout (row-major or column-major).</param>
        /// <param name="trans">Specifies the form of the system of equations (no transpose, transpose, or conjugate transpose).</param>
        /// <param name="n">The order of the matrix A (number of linear equations, i.e., the number of rows and columns in A).</param>
        /// <param name="nrhs">The number of right-hand sides, i.e., the number of columns of the matrix B.</param>
        /// <param name="a">Pointer to the LU-factored complex matrix A as computed by <c>Getrf</c>.</param>
        /// <param name="lda">The leading dimension of the array A.</param>
        /// <param name="ipiv">Pointer to the pivot indices as returned by <c>Getrf</c>.</param>
        /// <param name="b">Pointer to the right-hand side complex matrix B (input) and the solution matrix X (output).</param>
        /// <param name="ldb">The leading dimension of the array B.</param>
        void Getrs(LAPACK_Layout layout, LAPACK_Transpose trans,
            long n, long nrhs, [In] void* a, long lda,
            [In] long* ipiv, [In, Out] void* b, long ldb);

        #endregion
        #region ---- Getri [D/Z] ----

        /// <summary>
        /// Computes the inverse of an LU-factored general matrix (double precision).
        /// </summary>
        /// <param name="layout">Specifies the matrix storage layout (row- or column-major).</param>
        /// <param name="n">The order of the matrix A (number of rows and columns).</param>
        /// <param name="a">Pointer to the LU-factored matrix A (overwritten by the inverse on exit).</param>
        /// <param name="lda">The leading dimension of the matrix A.</param>
        /// <param name="ipiv">Pointer to the pivot indices from the LU factorization.</param>
        void Getri(LAPACK_Layout layout, long n,
            [In, Out] double* a, long lda, [In] long* ipiv);

        /// <summary>
        /// Computes the inverse of an LU-factored general matrix (complex double precision).
        /// </summary>
        /// <param name="layout">Specifies the matrix storage layout (row- or column-major).</param>
        /// <param name="n">The order of the matrix A (number of rows and columns).</param>
        /// <param name="a">Pointer to the LU-factored complex matrix A (overwritten by the inverse on exit).</param>
        /// <param name="lda">The leading dimension of the matrix A.</param>
        /// <param name="ipiv">Pointer to the pivot indices from the LU factorization.</param>
        void Getri(LAPACK_Layout layout, long n,
            [In, Out] void* a, long lda, [In] long* ipiv);

        #endregion
        #region ---- Gesv [D/Z] ----

        /// <summary>
        /// Computes the solution to a system of linear equations with a square coefficient matrix A and multiple right-hand sides (double precision).
        /// This method overwrites the input matrix <paramref name="a"/> with the factors L and U from the factorization, and overwrites <paramref name="b"/> with the solution matrix X.
        /// A*X = B
        /// </summary>
        /// <param name="layout">Specifies the matrix storage layout (row-major or column-major).</param>
        /// <param name="n">The order of the matrix A (number of linear equations).</param>
        /// <param name="nrhs">The number of right-hand sides (number of columns of matrix B).</param>
        /// <param name="a">On entry, the coefficient matrix A; on exit, overwritten by the factors L and U from the factorization.</param>
        /// <param name="lda">The leading dimension of A.</param>
        /// <param name="ipiv">The pivot indices that define the permutation matrix P; size at least n.</param>
        /// <param name="b">On entry, the right-hand side matrix B; on exit, overwritten by the solution matrix X.</param>
        /// <param name="ldb">The leading dimension of B.</param>
        void Gesv(LAPACK_Layout layout, long n, long nrhs,
            [In, Out] double* a, long lda, [In, Out] long* ipiv,
            [In, Out] double* b, long ldb);

        /// <summary>
        /// Computes the solution to a system of linear equations with a square coefficient matrix A and multiple right-hand sides (complex double precision).
        /// This method overwrites the input matrix <paramref name="a"/> with the factors L and U from the factorization, and overwrites <paramref name="b"/> with the solution matrix X.
        /// A*X = B
        /// </summary>
        /// <param name="layout">Specifies the matrix storage layout (row-major or column-major).</param>
        /// <param name="n">The order of the matrix A (number of linear equations).</param>
        /// <param name="nrhs">The number of right-hand sides (number of columns of matrix B).</param>
        /// <param name="a">On entry, the coefficient matrix A (complex); on exit, overwritten by the factors L and U from the factorization.</param>
        /// <param name="lda">The leading dimension of A.</param>
        /// <param name="ipiv">The pivot indices that define the permutation matrix P; size at least n.</param>
        /// <param name="b">On entry, the right-hand side matrix B (complex); on exit, overwritten by the solution matrix X.</param>
        /// <param name="ldb">The leading dimension of B.</param>
        void Gesv(LAPACK_Layout layout, long n, long nrhs,
            [In, Out] void* a, long lda, [In, Out] long* ipiv,
            [In, Out] void* b, long ldb);

        #endregion
        #region ---- Geev [D/Z] ----

        /// <summary>
        /// Computes the eigenvalues and, optionally, the left and/or right eigenvectors for a real nonsymmetric matrix.
        /// </summary>
        /// <param name="layout">Specifies the matrix layout (row-major or column-major).</param>
        /// <param name="jobvl">Specifies whether to compute the left eigenvectors.</param>
        /// <param name="jobvr">Specifies whether to compute the right eigenvectors.</param>
        /// <param name="n">The order of the matrix A.</param>
        /// <param name="a">Pointer to the input matrix A. Overwritten on exit.</param>
        /// <param name="lda">The leading dimension of A.</param>
        /// <param name="wr">Pointer to the output array for real parts of computed eigenvalues.</param>
        /// <param name="wi">Pointer to the output array for imaginary parts of computed eigenvalues.</param>
        /// <param name="vl">Pointer to the output array for left eigenvectors (if requested).</param>
        /// <param name="ldvl">The leading dimension of VL.</param>
        /// <param name="vr">Pointer to the output array for right eigenvectors (if requested).</param>
        /// <param name="ldvr">The leading dimension of VR.</param>
        void Geev(LAPACK_Layout layout,
            LAPACK_Job jobvl, LAPACK_Job jobvr,
            long n, [In, Out] double* a, long lda,
            [Out] double* wr, [Out] double* wi,
            [Out] double* vl, long ldvl,
            [Out] double* vr, long ldvr);

        /// <summary>
        /// Computes the eigenvalues and, optionally, the left and/or right eigenvectors for a complex nonsymmetric matrix.
        /// </summary>
        /// <param name="layout">Specifies the matrix layout (row-major or column-major).</param>
        /// <param name="jobvl">Specifies whether to compute the left eigenvectors.</param>
        /// <param name="jobvr">Specifies whether to compute the right eigenvectors.</param>
        /// <param name="n">The order of the matrix A.</param>
        /// <param name="a">Pointer to the input matrix A (complex). Overwritten on exit.</param>
        /// <param name="lda">The leading dimension of A.</param>
        /// <param name="w">Pointer to the output array for computed eigenvalues (complex).</param>
        /// <param name="vl">Pointer to the output array for left eigenvectors (if requested, complex).</param>
        /// <param name="ldvl">The leading dimension of VL.</param>
        /// <param name="vr">Pointer to the output array for right eigenvectors (if requested, complex).</param>
        /// <param name="ldvr">The leading dimension of VR.</param>
        void Geev(LAPACK_Layout layout,
            LAPACK_Job jobvl, LAPACK_Job jobvr,
            long n, [In, Out] void* a, long lda,
            [Out] void* w,
            [Out] void* vl, long ldvl,
            [Out] void* vr, long ldvr);

        #endregion
        #region ---- Ggev [D/Z] ----

        /// <summary>
        /// Computes the generalized eigenvalues and, optionally, the left and/or right eigenvectors for a pair of real matrices (double precision).
        /// </summary>
        /// <param name="layout">Specifies the matrix layout (row-major or column-major).</param>
        /// <param name="jobvl">Specifies whether to compute the left eigenvectors.</param>
        /// <param name="jobvr">Specifies whether to compute the right eigenvectors.</param>
        /// <param name="n">The order of the matrices A and B.</param>
        /// <param name="a">Pointer to the first element of matrix A.</param>
        /// <param name="lda">The leading dimension of matrix A.</param>
        /// <param name="b">Pointer to the first element of matrix B.</param>
        /// <param name="ldb">The leading dimension of matrix B.</param>
        /// <param name="alphar">Pointer to the output array for the real parts of the generalized eigenvalues.</param>
        /// <param name="alphai">Pointer to the output array for the imaginary parts of the generalized eigenvalues.</param>
        /// <param name="beta">Pointer to the output array for the denominators of the generalized eigenvalues.</param>
        /// <param name="vl">Pointer to the output array for the left eigenvectors.</param>
        /// <param name="ldvl">The leading dimension of the left eigenvector matrix.</param>
        /// <param name="vr">Pointer to the output array for the right eigenvectors.</param>
        /// <param name="ldvr">The leading dimension of the right eigenvector matrix.</param>
        void Ggev(LAPACK_Layout layout,
            LAPACK_Job jobvl, LAPACK_Job jobvr,
            long n, [In, Out] double* a, long lda,
            [In] double* b, long ldb,
            [Out] double* alphar, [Out] double* alphai, [Out] double* beta,
            [Out] double* vl, long ldvl,
            [Out] double* vr, long ldvr);

        /// <summary>
        /// Computes the generalized eigenvalues and, optionally, the left and/or right eigenvectors for a pair of complex matrices.
        /// </summary>
        /// <param name="layout">Specifies the matrix layout (row-major or column-major).</param>
        /// <param name="jobvl">Specifies whether to compute the left eigenvectors.</param>
        /// <param name="jobvr">Specifies whether to compute the right eigenvectors.</param>
        /// <param name="n">The order of the matrices A and B.</param>
        /// <param name="a">Pointer to the first element of matrix A (complex).</param>
        /// <param name="lda">The leading dimension of matrix A.</param>
        /// <param name="b">Pointer to the first element of matrix B (complex).</param>
        /// <param name="ldb">The leading dimension of matrix B.</param>
        /// <param name="alpha">Pointer to the output array for the generalized eigenvalues (complex).</param>
        /// <param name="beta">Pointer to the output array for the denominators of the generalized eigenvalues (complex).</param>
        /// <param name="vl">Pointer to the output array for the left eigenvectors (complex).</param>
        /// <param name="ldvl">The leading dimension of the left eigenvector matrix.</param>
        /// <param name="vr">Pointer to the output array for the right eigenvectors (complex).</param>
        /// <param name="ldvr">The leading dimension of the right eigenvector matrix.</param>
        void Ggev(LAPACK_Layout layout,
            LAPACK_Job jobvl, LAPACK_Job jobvr,
            long n, [In, Out] void* a, long lda,
            [In] void* b, long ldb,
            [Out] void* alpha, [Out] void* beta,
            [Out] void* vl, long ldvl,
            [Out] void* vr, long ldvr);

        #endregion
        #region ---- Syev [D] ----

        /// <summary>
        /// Computes all eigenvalues and, optionally, eigenvectors of a real symmetric matrix using the LAPACK dsyev routine.
        /// </summary>
        /// <param name="layout">Specifies the matrix layout (row-major or column-major).</param>
        /// <param name="jobz">Specifies whether to compute eigenvectors ('V') or eigenvalues only ('N').</param>
        /// <param name="uplo">Specifies whether the upper ('U') or lower ('L') triangle of the matrix is used.</param>
        /// <param name="n">The order of the matrix A. Must be at least zero.</param>
        /// <param name="a">Pointer to the input matrix A. On exit, contains eigenvectors if requested.</param>
        /// <param name="lda">The leading dimension of the array A. Must be at least max(1, n).</param>
        /// <param name="w">Pointer to the output array of eigenvalues, in ascending order.</param>
        void Syev(LAPACK_Layout layout, LAPACK_Job jobz,
            char uplo, long n, [In, Out] double* a, long lda,
            [Out] double* w);

        #endregion
        #region ---- Heev [Z] ----

        /// <summary>
        /// Computes all eigenvalues and, optionally, eigenvectors of a complex Hermitian matrix using LAPACK's zheev routine.
        /// </summary>
        /// <param name="layout">Specifies the matrix layout (row-major or column-major).</param>
        /// <param name="jobz">Specifies whether to compute eigenvectors ('N' for eigenvalues only, 'V' for eigenvalues and eigenvectors).</param>
        /// <param name="uplo">Specifies whether the upper or lower triangular part of the matrix is used ('U' or 'L').</param>
        /// <param name="n">The order of the matrix <paramref name="a"/>.</param>
        /// <param name="a">Pointer to the Hermitian matrix. On exit, contains the eigenvectors if requested.</param>
        /// <param name="lda">The leading dimension of <paramref name="a"/>.</param>
        /// <param name="w">Pointer to the output array for eigenvalues, in ascending order.</param>
        void Heev(LAPACK_Layout layout, LAPACK_Job jobz,
            char uplo, long n, [In, Out] void* a, long lda,
            [Out] double* w);

        #endregion
        #region ---- Gesvd [D/Z] ----

        /// <summary>
        /// Computes the singular value decomposition (SVD) of a real M-by-N matrix A.
        /// </summary>
        /// <param name="layout">Specifies the matrix layout (row-major or column-major).</param>
        /// <param name="jobu">Specifies options for computing all or part of the matrix U.</param>
        /// <param name="jobvt">Specifies options for computing all or part of the matrix VT.</param>
        /// <param name="m">The number of rows of the matrix A. m &gt;= 0.</param>
        /// <param name="n">The number of columns of the matrix A. n &gt;= 0.</param>
        /// <param name="a">On entry, the M-by-N matrix A. On exit, contents are overwritten.</param>
        /// <param name="lda">The leading dimension of the array A.</param>
        /// <param name="s">The singular values of A, sorted in decreasing order.</param>
        /// <param name="u">The left singular vectors, if requested.</param>
        /// <param name="ldu">The leading dimension of the array U.</param>
        /// <param name="vt">The right singular vectors, if requested.</param>
        /// <param name="ldvt">The leading dimension of the array VT.</param>
        /// <param name="superb">Array used for intermediate computations.</param>
        void Gesvd(LAPACK_Layout layout,
            LAPACK_Job jobu, LAPACK_Job jobvt, long m, long n,
            [In, Out] double* a, long lda,
            [Out] double* s, [Out] double* u, long ldu,
            [Out] double* vt, long ldvt, [Out] double* superb);

        /// <summary>
        /// Computes the singular value decomposition (SVD) of a complex M-by-N matrix A.
        /// </summary>
        /// <param name="layout">Specifies the matrix layout (row-major or column-major).</param>
        /// <param name="jobu">Specifies options for computing all or part of the matrix U.</param>
        /// <param name="jobvt">Specifies options for computing all or part of the matrix VT.</param>
        /// <param name="m">The number of rows of the matrix A. m &gt;= 0.</param>
        /// <param name="n">The number of columns of the matrix A. n &gt;= 0.</param>
        /// <param name="a">On entry, the M-by-N complex matrix A. On exit, contents are overwritten.</param>
        /// <param name="lda">The leading dimension of the array A.</param>
        /// <param name="s">The singular values of A, sorted in decreasing order.</param>
        /// <param name="u">The left singular vectors, if requested.</param>
        /// <param name="ldu">The leading dimension of the array U.</param>
        /// <param name="vt">The right singular vectors, if requested.</param>
        /// <param name="ldvt">The leading dimension of the array VT.</param>
        /// <param name="superb">Array used for intermediate computations.</param>
        void Gesvd(LAPACK_Layout layout,
            LAPACK_Job jobu, LAPACK_Job jobvt, long m, long n,
            [In, Out] void* a, long lda,
            [Out] double* s, [Out] void* u, long ldu,
            [Out] void* vt, long ldvt, [Out] double* superb);

        #endregion
        #region ---- Gels [D/Z] ----

        /// <summary>
        /// Computes the minimum-norm solution to a real linear least squares problem using the LAPACK DGELS routine.
        /// </summary>
        /// <param name="layout">Specifies the matrix layout (row-major or column-major).</param>
        /// <param name="trans">Specifies the options for transposing the matrix.</param>
        /// <param name="m">The number of rows of the matrix A.</param>
        /// <param name="n">The number of columns of the matrix A.</param>
        /// <param name="nrhs">The number of right-hand sides, i.e., the number of columns of the matrix B.</param>
        /// <param name="a">Pointer to the matrix A. On exit, contains details of its QR or LQ factorization.</param>
        /// <param name="lda">The leading dimension of the array A.</param>
        /// <param name="b">Pointer to the matrix B. On exit, contains the solution matrix X.</param>
        /// <param name="ldb">The leading dimension of the array B.</param>
        void Gels(LAPACK_Layout layout, LAPACK_Transpose trans,
            long m, long n, long nrhs, [In, Out] double* a, long lda,
            [In, Out] double* b, long ldb);

        /// <summary>
        /// Computes the minimum-norm solution to a complex linear least squares problem using the LAPACK ZGELS routine.
        /// </summary>
        /// <param name="layout">Specifies the matrix layout (row-major or column-major).</param>
        /// <param name="trans">Specifies the options for transposing the matrix.</param>
        /// <param name="m">The number of rows of the matrix A.</param>
        /// <param name="n">The number of columns of the matrix A.</param>
        /// <param name="nrhs">The number of right-hand sides, i.e., the number of columns of the matrix B.</param>
        /// <param name="a">Pointer to the complex matrix A. On exit, contains details of its QR or LQ factorization.</param>
        /// <param name="lda">The leading dimension of the array A.</param>
        /// <param name="b">Pointer to the complex matrix B. On exit, contains the solution matrix X.</param>
        /// <param name="ldb">The leading dimension of the array B.</param>
        public void Gels(LAPACK_Layout layout, LAPACK_Transpose trans,
            long m, long n, long nrhs, [In, Out] void* a, long lda,
            [In, Out] void* b, long ldb)
            => IntelMKLNative.LAPACKE_zgels_64(layout, trans,
                m, n, nrhs, a, lda, b, ldb);

        #endregion

        
        // obsolete ...
        //#region --------- Getrf ---------

        ///// <summary>
        ///// computes the LU factorization of a general m-by-n matrix
        ///// </summary>
        ///// <param name="layout"> matrix storage layout is row or column major </param>
        ///// <param name="m"> number of rows in matrix a </param>
        ///// <param name="n"> number of columns in matrix a </param>
        ///// <param name="a"> matrix a (overwritten by L and U on exit) </param>
        ///// <param name="lda"> leading dimension of a </param>
        ///// <param name="ipiv"> pivot indices </param>
        ///// <param name="starta"> starting index in a </param>
        ///// <param name="startipiv"> starting index in ipiv </param>
        //void Getrf(LAPACK_Layout layout, long m, long n,
        //    DenseArrayBase<double> a, long lda, DenseArrayBase<long> ipiv,
        //    long starta = 0, long startipiv = 0);

        ///// <summary>
        ///// computes the LU factorization of a general m-by-n matrix
        ///// </summary>
        ///// <param name="layout"> matrix storage layout is row or column major </param>
        ///// <param name="m"> number of rows in matrix a </param>
        ///// <param name="n"> number of columns in matrix a </param>
        ///// <param name="a"> matrix a (overwritten by L and U on exit) </param>
        ///// <param name="lda"> leading dimension of a </param>
        ///// <param name="ipiv"> pivot indices </param>
        ///// <param name="starta"> starting index in a </param>
        ///// <param name="startipiv"> starting index in ipiv </param>
        //void Getrf(LAPACK_Layout layout, long m, long n,
        //    DenseArrayBase<Complex> a, long lda, DenseArrayBase<long> ipiv,
        //    long starta = 0, long startipiv = 0);

        //#endregion
        //#region --------- Getrs ---------

        ///// <summary>
        ///// solves a system of linear equations a * x = b
        ///// with an LU-factored square coefficient matrix
        ///// </summary>
        ///// <param name="layout"> matrix storage layout is row or column major </param>
        ///// <param name="operation"> whether to solve the system with matrix a transposed </param>
        ///// <param name="n"> number of columns in a; number of rows in b </param>
        ///// <param name="nrhs"> number of right-hand sides </param>
        ///// <param name="a"> result of LU factorization of a </param>
        ///// <param name="lda"> leading dimension of a </param>
        ///// <param name="ipiv"> pivot indices </param>
        ///// <param name="b"> vector or matrix b (overwritte by x on exit) </param>
        ///// <param name="ldb"> leading dimension of b </param>
        ///// <param name="starta"> starting index in a </param>
        ///// <param name="startipiv"> starting index in ipiv </param>
        ///// <param name="startb"> starting index in b </param>
        //void Getrs(LAPACK_Layout layout, LAPACK_Transpose operation,
        //    long n, long nrhs, DenseArrayBase<double> a, long lda, DenseArrayBase<long> ipiv,
        //    DenseArrayBase<double> b, long ldb,
        //    long starta = 0, long startipiv = 0, long startb = 0);

        ///// <summary>
        ///// solves a system of linear equations a * x = b
        ///// with an LU-factored square coefficient matrix
        ///// </summary>
        ///// <param name="layout"> matrix storage layout is row or column major </param>
        ///// <param name="operation"> whether to solve the system with matrix a transposed </param>
        ///// <param name="n"> number of columns in a; number of rows in b </param>
        ///// <param name="nrhs"> number of right-hand sides </param>
        ///// <param name="a"> result of LU factorization of a </param>
        ///// <param name="lda"> leading dimension of a </param>
        ///// <param name="ipiv"> pivot indices </param>
        ///// <param name="b"> vector or matrix b (overwritte by x on exit) </param>
        ///// <param name="ldb"> leading dimension of b </param>
        ///// <param name="starta"> starting index in a </param>
        ///// <param name="startipiv"> starting index in ipiv </param>
        ///// <param name="startb"> starting index in b </param>
        //void Getrs(LAPACK_Layout layout, LAPACK_Transpose operation,
        //    long n, long nrhs, DenseArrayBase<Complex> a, long lda, DenseArrayBase<long> ipiv,
        //    DenseArrayBase<Complex> b, long ldb,
        //    long starta = 0, long startipiv = 0, long startb = 0);

        //#endregion
        //#region --------- Getri ---------

        ///// <summary>
        ///// computes the inverse of an LU-factored general matrix
        ///// </summary>
        ///// <param name="layout"> matrix storage layout is row or column major </param>
        ///// <param name="n"> number of columns in a; number of rows in b </param>
        ///// <param name="a"> result of LU factorization of a </param>
        ///// <param name="lda"> leading dimension of a </param>
        ///// <param name="ipiv"> pivot indices </param>
        ///// <param name="starta"> starting index in a </param>
        ///// <param name="startipiv"> starting index in ipiv </param>
        //void Getri(LAPACK_Layout layout, long n,
        //    DenseArrayBase<double> a, long lda, DenseArrayBase<long> ipiv,
        //    long starta = 0, long startipiv = 0);

        ///// <summary>
        ///// computes the inverse of an LU-factored general matrix
        ///// </summary>
        ///// <param name="layout"> matrix storage layout is row or column major </param>
        ///// <param name="n"> number of columns in a; number of rows in b </param>
        ///// <param name="a"> result of LU factorization of a </param>
        ///// <param name="lda"> leading dimension of a </param>
        ///// <param name="ipiv"> pivot indices </param>
        ///// <param name="starta"> starting index in a </param>
        ///// <param name="startipiv"> starting index in ipiv </param>
        //void Getri(LAPACK_Layout layout, long n,
        //    DenseArrayBase<Complex> a, long lda, DenseArrayBase<long> ipiv,
        //    long starta = 0, long startipiv = 0);

        //#endregion
        //#region --------- Gesv ---------

        ///// <summary>
        ///// Computes the solution to the system of linear equations 
        ///// with a square coefficient matrix A and multiple right-hand sides
        ///// </summary>
        ///// <param name="layout"> matrix storage layout is row or column major </param>
        ///// <param name="n"> the number of linear equations i.e. order of matrix a </param>
        ///// <param name="nrhs"> the number of right-hand sides i.e. columns of matrix b </param>
        ///// <param name="a"> matrix a (overwritten by the factor L and U from the factorization) </param>
        ///// <param name="lda"> leading dimension of a </param>
        ///// <param name="ipiv"> the pivot indices that define the permutation matrix p </param>
        ///// <param name="b"> vector b (overwritten by the solution vector x) </param>
        ///// <param name="ldb"> leading dimension of b </param>
        ///// <param name="starta"> starting index in a </param>
        ///// <param name="startipiv"> starting index in ipiv </param>
        ///// <param name="startb"> starting index in b </param>
        //void Gesv(LAPACK_Layout layout, long n, long nrhs,
        //    DenseArrayBase<double> a, long lda, DenseArrayBase<long> ipiv,
        //    DenseArrayBase<double> b, long ldb,
        //    long starta = 0, long startipiv = 0, long startb = 0);

        ///// <summary>
        ///// Computes the solution to the system of linear equations 
        ///// with a square coefficient matrix A and multiple right-hand sides
        ///// </summary>
        ///// <param name="layout"> matrix storage layout is row or column major </param>
        ///// <param name="n"> the number of linear equations i.e. order of matrix a </param>
        ///// <param name="nrhs"> the number of right-hand sides i.e. columns of matrix b </param>
        ///// <param name="a"> matrix a (overwritten by the factor L and U from the factorization) </param>
        ///// <param name="lda"> leading dimension of a </param>
        ///// <param name="ipiv"> the pivot indices that define the permutation matrix p </param>
        ///// <param name="b"> vector b (overwritten by the solution vector x) </param>
        ///// <param name="ldb"> leading dimension of b </param>
        ///// <param name="starta"> starting index in a </param>
        ///// <param name="startipiv"> starting index in ipiv </param>
        ///// <param name="startb"> starting index in b </param>
        //void Gesv(LAPACK_Layout layout, long n, long nrhs,
        //    DenseArrayBase<Complex> a, long lda, DenseArrayBase<long> ipiv,
        //    DenseArrayBase<Complex> b, long ldb,
        //    long starta = 0, long startipiv = 0, long startb = 0);

        //#endregion
        //#region --------- Gels ---------

        ///// <summary>
        ///// solves overdetermined or underdetermined linear least square problem
        ///// </summary>
        ///// <param name="layout"> matrix storage layout is row or column major </param>
        ///// <param name="operation"> whether to transpose the matrix a. 'N' no transpose; 'T' transpose matrix a </param>
        ///// <param name="m"> number of rows of the matrix a </param>
        ///// <param name="n"> number of columns of the matrix a </param>
        ///// <param name="nrhs"> number of right-hand sides; number of columns in b </param>
        ///// <param name="a"> matrix a (overwritten by the factorization) </param>
        ///// <param name="lda"> leading dimension of the matrix a </param>
        ///// <param name="b"> vector b (overwritten by the solution vector x) </param>
        ///// <param name="ldb"> leading dimension of b </param>
        ///// <param name="starta"> starting index in a </param>
        ///// <param name="startb"> starting index in a </param>
        //void Gels(LAPACK_Layout layout, LAPACK_Transpose operation,
        //    long m, long n, long nrhs,
        //    DenseArrayBase<double> a, long lda,
        //    DenseArrayBase<double> b, long ldb,
        //    long starta = 0, long startb = 0);

        ///// <summary>
        ///// solves overdetermined or underdetermined linear least square problem
        ///// </summary>
        ///// <param name="layout"> matrix storage layout is row or column major </param>
        ///// <param name="operation"> whether to transpose the matrix a. 'N' no transpose; 'T' transpose matrix a </param>
        ///// <param name="m"> number of rows of the matrix a </param>
        ///// <param name="n"> number of columns of the matrix a </param>
        ///// <param name="nrhs"> number of right-hand sides; number of columns in b </param>
        ///// <param name="a"> matrix a (overwritten by the factorization) </param>
        ///// <param name="lda"> leading dimension of the matrix a </param>
        ///// <param name="b"> vector b (overwritten by the solution vector x) </param>
        ///// <param name="ldb"> leading dimension of b </param>
        ///// <param name="starta"> starting index in a </param>
        ///// <param name="startb"> starting index in a </param>
        //void Gels(LAPACK_Layout layout, LAPACK_Transpose operation,
        //    long m, long n, long nrhs,
        //    DenseArrayBase<Complex> a, long lda,
        //    DenseArrayBase<Complex> b, long ldb,
        //    long starta = 0, long startb = 0);

        //#endregion
        //#region --------- Geev ---------

        ///// <summary>
        ///// Computes the eigenvalues and left and right eigenvectors of a general matrix
        ///// </summary>
        ///// <param name="layout"> matrix storage layout is row or column major </param>
        ///// <param name="jobvl"> 'N' left eigenvectors not computed; 'V' left eigenvectors computed </param>
        ///// <param name="jobvr"> 'N' right eigenvectors not computed; 'V' right eigenvectors computed </param>
        ///// <param name="n"> order of matrix a </param>
        ///// <param name="a"> matrix a (overwritten on exit) </param>
        ///// <param name="lda"> leading dimension of a </param>
        ///// <param name="wr"> real part of the computed eigenvalues </param>
        ///// <param name="wi"> imaginary part of the computed eigenvalues </param>
        ///// <param name="vl"> left eigenvectors </param>
        ///// <param name="ldvl"> leading dimensino of vl </param>
        ///// <param name="vr"> right eigenvectors </param>
        ///// <param name="ldvr"> leading dimension of vr </param>
        ///// <param name="starta"> starting index in a </param>
        ///// <param name="startwr"> starting index in wr </param>
        ///// <param name="startwi"> starting index in wi </param>
        ///// <param name="startvl"> starting index in vl </param>
        ///// <param name="startvr"> starting index in vr </param>
        //void Geev(LAPACK_Layout layout, LAPACK_Job jobvl, LAPACK_Job jobvr, long n,
        //    DenseArrayBase<double> a, long lda,
        //    DenseArrayBase<double> wr, DenseArrayBase<double> wi,
        //    DenseArrayBase<double> vl, long ldvl, DenseArrayBase<double> vr, long ldvr,
        //    long starta = 0, long startwr = 0, long startwi = 0,
        //    long startvl = 0, long startvr = 0);

        ///// <summary>
        ///// Computes the eigenvalues and left and right eigenvectors of a general matrix
        ///// </summary>
        ///// <param name="layout"> matrix storage layout is row or column major </param>
        ///// <param name="jobvl"> 'N' left eigenvectors not computed; 'V' left eigenvectors computed </param>
        ///// <param name="jobvr"> 'N' right eigenvectors not computed; 'V' right eigenvectors computed </param>
        ///// <param name="n"> order of matrix a </param>
        ///// <param name="a"> matrix a (overwritten on exit) </param>
        ///// <param name="lda"> leading dimension of a </param>
        ///// <param name="w"> eigenvalues </param>
        ///// <param name="vl"> left eigenvectors </param>
        ///// <param name="ldvl"> leading dimensino of vl </param>
        ///// <param name="vr"> right eigenvectors </param>
        ///// <param name="ldvr"> leading dimension of vr </param>
        ///// <param name="starta"> starting index in a </param>
        ///// <param name="startw"> starting index in wr </param>
        ///// <param name="startvl"> starting index in vl </param>
        ///// <param name="startvr"> starting index in vr </param>
        //void Geev(LAPACK_Layout layout, LAPACK_Job jobvl, LAPACK_Job jobvr, long n,
        //    DenseArrayBase<Complex> a, long lda,
        //    DenseArrayBase<Complex> w,
        //    DenseArrayBase<Complex> vl, long ldvl, DenseArrayBase<Complex> vr, long ldvr,
        //    long starta = 0, long startw = 0,
        //    long startvl = 0, long startvr = 0);

        //#endregion
        //#region --------- Ggev ---------

        ///// <summary>
        ///// Computes the eigenvalues and left and right eigenvectors of general matrixs
        ///// </summary>
        ///// <param name="layout"> matrix storage layout is row or column major </param>
        ///// <param name="jobvl"> 'N' left eigenvectors not computed; 'V' left eigenvectors computed </param>
        ///// <param name="jobvr"> 'N' right eigenvectors not computed; 'V' right eigenvectors computed </param>
        ///// <param name="n"> order of matrix a </param>
        ///// <param name="a"> matrix a (overwritten on exit) </param>
        ///// <param name="lda"> leading dimension of a </param>
        ///// <param name="b"> matrix b (overwritten on exit) </param>
        ///// <param name="ldb"> leading dimension of b </param>
        ///// <param name="alphar"> real part of a part of the computed eigenvalues => alpha/beta = lambda </param>
        ///// <param name="alphai"> imaginary part of a part of the computed eigenvalues => alpha/beta = lambda </param>
        ///// <param name="beta"> part of the computed eigenvalues => alpha/beta = lambda </param>
        ///// <param name="vl"> left eigenvectors </param>
        ///// <param name="ldvl"> leading dimensino of vl </param>
        ///// <param name="vr"> right eigenvectors </param>
        ///// <param name="ldvr"> leading dimension of vr </param>
        ///// <param name="starta"> starting index in a </param>
        ///// <param name="startb"> starting index in b </param>
        ///// <param name="startalphar"> starting index in alphar </param>
        ///// <param name="startalphai"> starting index in alphai </param>
        ///// <param name="startbeta"> starting index in beta </param>
        ///// <param name="startvl"> starting index in vl </param>
        ///// <param name="startvr"> starting index in vr </param>
        //void Ggev(LAPACK_Layout layout, LAPACK_Job jobvl, LAPACK_Job jobvr, long n,
        //    DenseArrayBase<double> a, long lda,
        //    DenseArrayBase<double> b, long ldb,
        //    DenseArrayBase<double> alphar, DenseArrayBase<double> alphai, DenseArrayBase<double> beta,
        //    DenseArrayBase<double> vl, long ldvl, DenseArrayBase<double> vr, long ldvr,
        //    long starta = 0, long startb = 0,
        //    long startalphar = 0, long startalphai = 0, long startbeta = 0,
        //    long startvl = 0, long startvr = 0);

        ///// <summary>
        ///// Computes the eigenvalues and left and right eigenvectors of general matrixs
        ///// </summary>
        ///// <param name="layout"> matrix storage layout is row or column major </param>
        ///// <param name="jobvl"> 'N' left eigenvectors not computed; 'V' left eigenvectors computed </param>
        ///// <param name="jobvr"> 'N' right eigenvectors not computed; 'V' right eigenvectors computed </param>
        ///// <param name="n"> order of matrix a </param>
        ///// <param name="a"> matrix a (overwritten on exit) </param>
        ///// <param name="lda"> leading dimension of a </param>
        ///// <param name="b"> matrix b (overwritten on exit) </param>
        ///// <param name="ldb"> leading dimension of b </param>
        ///// <param name="alpha"> part of the eigenvalues => alpha/beta = lambda </param>
        ///// <param name="beta"> part of the eigenvalues => alpha/beta = lambda </param>
        ///// <param name="vl"> left eigenvectors </param>
        ///// <param name="ldvl"> leading dimensino of vl </param>
        ///// <param name="vr"> right eigenvectors </param>
        ///// <param name="ldvr"> leading dimension of vr </param>
        ///// <param name="starta"> starting index in a </param>
        ///// <param name="startb"> starting index in b </param>
        ///// <param name="startalpha"> starting index in alpha </param>
        ///// <param name="startbeta"> starting index in beta </param>
        ///// <param name="startvl"> starting index in vl </param>
        ///// <param name="startvr"> starting index in vr </param>
        //void Ggev(LAPACK_Layout layout, LAPACK_Job jobvl, LAPACK_Job jobvr, long n,
        //    DenseArrayBase<Complex> a, long lda,
        //    DenseArrayBase<Complex> b, long ldb,
        //    DenseArrayBase<Complex> alpha, DenseArrayBase<Complex> beta,
        //    DenseArrayBase<Complex> vl, long ldvl, DenseArrayBase<Complex> vr, long ldvr,
        //    long starta = 0, long startb = 0,
        //    long startalpha = 0, long startbeta = 0,
        //    long startvl = 0, long startvr = 0);

        //#endregion
        //#region --------- Syev ---------

        ///// <summary>
        ///// computes all eigenvalues and, optionally, eigenvectors 
        ///// of a real symmetric matrix
        ///// </summary>
        ///// <param name="layout"> matrix storage layout is row or column major </param>
        ///// <param name="jobz"> 'N' only eigenvalues computed; 'V' eigenvectors computed too </param>
        ///// <param name="uplo"> 'U' stores upper triangular part of a; 'L' stores lower triangular part of a </param>
        ///// <param name="n"> order of matrix a </param>
        ///// <param name="a"> real symmetric matrix a, either the upper or lower triangular part (overwritten on exit) </param>
        ///// <param name="lda"> leading dimension of a </param>
        ///// <param name="w"> computed eigenvalues </param>
        ///// <param name="starta"> starting index in a </param>
        ///// <param name="startw"> starting index in wr </param>
        //void Syev(LAPACK_Layout layout, LAPACK_Job jobz, char uplo, long n,
        //    DenseArrayBase<double> a, long lda, DenseArrayBase<double> w,
        //    long starta = 0, long startw = 0);

        //#endregion
        //#region --------- Heev ---------

        ///// <summary>
        ///// computes all eigenvalues and, optionally, eigenvectors 
        ///// eigenvectors of a Hermitian matrix
        ///// </summary>
        ///// <param name="layout"> matrix storage layout is row or column major </param>
        ///// <param name="jobz"> 'N' only eigenvalues computed; 'V' eigenvectors computed too </param>
        ///// <param name="uplo"> 'U' stores upper triangular part of a; 'L' stores lower triangular part of a </param>
        ///// <param name="n"> order of matrix a </param>
        ///// <param name="a"> Hermitian matrix a, either the upper or lower triangular part (overwritten on exit) </param>
        ///// <param name="lda"> leading dimension of a </param>
        ///// <param name="w"> computed eigenvalues </param>
        ///// <param name="starta"> starting index in a </param>
        ///// <param name="startw"> starting index in wr </param>
        //void Heev(LAPACK_Layout layout, LAPACK_Job jobz, char uplo, long n,
        //    DenseArrayBase<Complex> a, long lda, DenseArrayBase<double> w,
        //    long starta = 0, long startw = 0);

        //#endregion
        //#region --------- Gesvd ---------

        ///// <summary>
        ///// computes the singular value decomposition of a general rectangular matrix
        ///// A = U*Σ*VT
        ///// </summary>
        ///// <param name="layout"> matrix storage layout is row or column major </param>
        ///// <param name="jobu"> specifies options for computing all or part of the matrix U </param>
        ///// <param name="jobvt"> specifies options for computing all or part of the matrix VT </param>
        ///// <param name="m"> number of rows of matrix a </param>
        ///// <param name="n"> number of columns of matrix a </param>
        ///// <param name="a"> matrix a </param>
        ///// <param name="lda"> leading dimension of matrix a </param>
        ///// <param name="s"> singular values, sorted so that s[i] >= s[i+1] </param>
        ///// <param name="u"> left singular value vector of matrix a </param>
        ///// <param name="ldu"> leading dimension of u </param>
        ///// <param name="vt"> (transposed) right singular value vector of matrix a </param>
        ///// <param name="ldvt"> leading dimension of vt </param>
        ///// <param name="superb"> unconverged superdiagonal elements </param>
        //void Gesvd(LAPACK_Layout layout, LAPACK_Job jobu, LAPACK_Job jobvt,
        //    long m, long n,
        //    DenseArrayBase<double> a, long lda, DenseArrayBase<double> s,
        //    DenseArrayBase<double> u, long ldu,
        //    DenseArrayBase<double> vt, long ldvt, DenseArrayBase<double> superb);

        ///// <summary>
        ///// computes the singular value decomposition of a general rectangular matrix
        ///// A = U*Σ*VH
        ///// </summary>
        ///// <param name="layout"> matrix storage layout is row or column major </param>
        ///// <param name="jobu"> specifies options for computing all or part of the matrix U </param>
        ///// <param name="jobvt"> specifies options for computing all or part of the matrix VT </param>
        ///// <param name="m"> number of rows of matrix a </param>
        ///// <param name="n"> number of columns of matrix a </param>
        ///// <param name="a"> matrix a </param>
        ///// <param name="lda"> leading dimension of matrix a </param>
        ///// <param name="s"> singular values, sorted so that s[i] >= s[i+1] </param>
        ///// <param name="u"> left singular value vector of matrix a </param>
        ///// <param name="ldu"> leading dimension of u </param>
        ///// <param name="vt"> (conjugate transposed) right singular value vector of matrix a </param>
        ///// <param name="ldvt"> leading dimension of vt </param>
        ///// <param name="superb"> unconverged superdiagonal elements </param>
        //void Gesvd(LAPACK_Layout layout, LAPACK_Job jobu, LAPACK_Job jobvt,
        //    long m, long n,
        //    DenseArrayBase<Complex> a, long lda, DenseArrayBase<double> s,
        //    DenseArrayBase<Complex> u, long ldu,
        //    DenseArrayBase<Complex> vt, long ldvt, DenseArrayBase<double> superb);

        //#endregion

    }



    /// <summary>
    /// matrix layout option for LAPACK
    /// </summary>
    public enum LAPACK_Layout : int
    {
        /// <summary>
        /// row-major layout
        /// </summary>
        RowMajor = 101,

        /// <summary>
        /// column-major layout
        /// </summary>
        ColMajor = 102,
    }

    /// <summary>
    /// matrix transpose option for LAPACK
    /// </summary>
    public enum LAPACK_Transpose
    {
        /// <summary>
        /// no transpose
        /// </summary>
        NoTrans = 'N', //111, // trans = 'N'
        /// <summary>
        /// transpose
        /// </summary>
        Trans = 'T', //112, // trans = 'T'
        /// <summary>
        /// conjugate transpose
        /// </summary>
        ConjTrans = 'C' //113 // trans = 'C'
    }

    /// <summary>
    /// job options for LAPACK
    /// </summary>
    public enum LAPACK_Job
    {
        /// <summary>
        /// no columns (no left singular vectors) are computed
        /// </summary>
        N = 'N', //201, // job = 'N'
        /// <summary>
        /// job 'V' ??
        /// </summary>
        V = 'V', // 202, // job = 'V'
        /// <summary>
        /// all m columns are returned in the array
        /// </summary>
        A = 'A', // 203, // job = 'A'
        /// <summary>
        /// the first min(m, n) columns (the left singular vectors)
        /// are returned in the array
        /// </summary>
        S = 'S', // 204, // job = 'S'
        /// <summary>
        /// the first min(m, n) columns (the left singular vectors)
        /// are overwritten on the array a
        /// </summary>
        O = 'O' // 205 // job = 'O'
    }




}
