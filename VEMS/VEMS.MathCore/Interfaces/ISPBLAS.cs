using System.Numerics;
using System.Runtime.InteropServices;

namespace VEMS.MathCore
{

    #region ----- spBLAS Enums -----

    /// <summary>
    /// sparse matrix status
    /// </summary>
    public enum SPARSE_Status
    {
        /// <summary>
        /// the operation was successful
        /// </summary>
        Success = 0,

        /// <summary>
        /// empty handle or matrix arrays
        /// </summary>
        NotInitialized = 1,

        /// <summary>
        /// internal error: memory allocation failed
        /// </summary>
        AllocFailed = 2,

        /// <summary>
        /// invalid input value
        /// </summary>
        InvalidValue = 3,

        /// <summary>
        /// e.g. 0-diagonal element for triangular solver, etc.
        /// </summary>
        ExecutionFailed = 4,

        /// <summary>
        /// internal error
        /// </summary>
        InternalError = 5,

        /// <summary>
        /// e.g. operation for double precision doesn't support other types
        /// </summary>
        NotSupported = 6
    }

    /// <summary>
    /// sparse matrix operation
    /// </summary>
    public enum SPARSE_Operation
    {
        /// <summary>
        /// no transpose
        /// </summary>
        NonTranspose = 10,

        /// <summary>
        /// transpose
        /// </summary>
        Transpose = 11,

        /// <summary>
        /// conjugate transpose
        /// </summary>
        ConjugateTranspose = 12
    }

    /// <summary>
    /// sparse matrix type
    /// </summary>
    public enum SPARSE_MatrixType
    {
        /// <summary>
        /// general case
        /// </summary>
        General = 20,

        /// <summary>
        /// symmetric
        /// </summary>
        Symmetric = 21,

        /// <summary>
        /// Hermitian
        /// </summary>
        Hermitian = 22,

        /// <summary>
        /// triangular
        /// </summary>
        Triangular = 23,

        /// <summary>
        /// diagonal matrix; only diagonal elements will be processed
        /// </summary>
        Diagonal = 24,

        /// <summary>
        /// block triangular
        /// </summary>
        BlockTriangular = 25,

        /// <summary>
        /// block-diagonal matrix; only diagonal blocks will be processed
        /// </summary>
        BlockDiagonal = 26
    }

    /// <summary>
    /// sparse matrix index base
    /// </summary>
    public enum SPARSE_IndexBase
    {
        /// <summary>
        /// zero-based indexing
        /// C-style indexing
        /// </summary>
        ZeroBase = 0,

        /// <summary>
        /// one-based indexing
        /// Fortran-style indexing
        /// </summary>
        OneBase = 1
    }

    /// <summary>
    /// sparse matrix fill mode
    /// applies to triangular matrices only
    /// [Symmetric, Hermitian, Triangular]
    /// </summary>
    public enum SPARSE_FillMode
    {
        /// <summary>
        /// lower triangular part of the matrix is stored
        /// </summary>
        Lower = 40,

        /// <summary>
        /// upper triangular part of the matrix is stored
        /// </summary>
        Upper = 41,

        /// <summary>
        /// upper triangular part of the matrix is stored
        /// </summary>
        Full = 42
    }

    /// <summary>
    /// sparse matrix diagonal type
    /// applies to triangular matrices only
    /// [Symmetric, Hermitian, Triangular]
    /// </summary>
    public enum SPARSE_DiagType
    {
        /// <summary>
        /// triangular matrix with non-unit diagonal
        /// </summary>
        NonUnit = 50,

        /// <summary>
        /// triangular matrix with unit diagonal
        /// </summary>
        Unit = 51
    }

    /// <summary>
    /// sparse matrix structure
    /// applicable for Level-3 operations with dense matrices
    /// </summary>
    public enum SPARSE_Layout
    {
        /// <summary>
        /// row-major layout
        /// C-style
        /// </summary>
        RowMajor = 101,

        /// <summary>
        /// column-major layout
        /// Fortran-style
        /// </summary>
        ColumnMajor = 102
    }

    /// <summary>
    /// sparse matrix verbose mode
    /// </summary>
    public enum SPARSE_VerboseMode
    {
        /// <summary>
        /// verbose mode off
        /// </summary>
        Off = 70,

        /// <summary>
        /// output contains high-level information 
        /// about optimization algorithms, issues, etc.
        /// </summary>
        Basic = 71,

        /// <summary>
        /// provide detailed output information
        /// </summary>
        Extended = 72
    }

    /// <summary>
    /// sparse matrix memory optimization hints from user
    /// </summary>
    public enum SPARSE_MemoryUsage
    {
        /// <summary>
        /// no memory should be allocated for matrix values and structures; 
        /// auxiliary structures could be created only for workload balancing, parallelization, etc.
        /// </summary>
        None = 80,

        /// <summary>
        /// matrix could be converted to any internal format 
        /// </summary>
        Aggressive = 81
    }

    /// <summary>
    /// sparse matrix request
    /// </summary>
    public enum SPARSE_Request
    {
        /// <summary>
        /// 
        /// </summary>
        FullMult = 90,

        /// <summary>
        /// 
        /// </summary>
        NNZCount = 91,

        /// <summary>
        /// 
        /// </summary>
        FinalizeMult = 92,

        /// <summary>
        /// 
        /// </summary>
        FullMultNoVal = 93,

        /// <summary>
        /// 
        /// </summary>
        FinalizeMultNoVal = 94
    }

    /// <summary>
    /// applies to SOR interface
    /// define type of (S)SOR operation to perform
    /// </summary>
    public enum SPARSE_SorType
    {
        /// <summary>
        /// (omega∗L + D)∗x^1 = (D - omega*D - omega*U)∗alpha*x^0 + omega*b
        /// </summary>
        Forward = 110,

        /// <summary>
        /// (omega∗U + D)∗x^1 = (D - omega*D - omega*L)∗alpha*x^0 + omega*b
        /// </summary>
        Backward = 111,

        /// <summary>
        /// SSOR, for e.g. with omega == 1 and alpha == 1, equal to solving a system:
        /// (L + D)∗x^1 = b - U*x; (U + D)∗x = b - L * x ^ 1
        /// </summary>
        Symmetruc = 112
    }

    /// <summary>
    /// QR factorization hints
    /// </summary>
    public enum SPARSE_QRHint
    {
        /// <summary>
        /// with pivoting
        /// </summary>
        WtihPivots
    }

    /// <summary>
    /// descriptor of sparse matrix properties
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct SPARSE_MatrixDescr
    {
        /// <summary>
        /// matrix type: general, diagonal 
        /// or triangular / symmetric / hermitian
        /// </summary>
        public SPARSE_MatrixType Type;

        /// <summary>
        /// upper or lower triangular part of the matrix 
        /// (for triangular / symmetric / hermitian case)
        /// </summary>
        public SPARSE_FillMode Mode;

        /// <summary>
        /// unit or non-unit diagonal 
        /// (for triangular / symmetric / hermitian case)
        /// </summary>
        public SPARSE_DiagType Diag;
    }


    /// <summary>
    /// sparse matrix storage format
    /// </summary>
    public enum SPARSE_StorageFormat
    {
        /// <summary>
        /// compressed sparse row
        /// </summary>
        CSR = 200,

        /// <summary>
        /// compressed sparse column
        /// </summary>
        CSC = 201,

        /// <summary>
        /// coordinate format
        /// </summary>
        COO = 202,

        /// <summary>
        /// block compressed sparse row
        /// </summary>
        BSR

        ///// <summary>
        ///// diagonal format
        ///// </summary>
        //DIA = 203,
        ///// <summary>
        ///// block diagonal format
        ///// </summary>
        //BDI = 204,
        ///// <summary>
        ///// ellpack format
        ///// </summary>
        //ELL = 205,
        ///// <summary>
        ///// jagged diagonal format
        ///// </summary>
        //JAD = 206,
        ///// <summary>
        ///// block compressed sparse row
        ///// </summary>
        //BCSR = 207,
        ///// <summary>
        ///// block compressed sparse column
        ///// </summary>
        //BCSC = 208,
        ///// <summary>
        ///// block ellpack format
        ///// </summary>
        //BELL = 209,
        ///// <summary>
        ///// block jagged diagonal format
        ///// </summary>
        //BJAD = 210
    }


    // ...


    #endregion



    /// <summary>
    /// Sparse BLAS interface
    /// </summary>
    public unsafe interface ISPBLAS
    {
        #region Level 1

        #region ---- Asum [D/Z] ----

        /// <summary>
        /// Computes the sum of magnitudes of the non-zero elements in a sparse double-precision array.
        /// dasum := |x[0]| + |x[1]| + ... + |x[n-1]|
        /// </summary>
        /// <param name="n">Number of non-zero elements in the array.</param>
        /// <param name="x">Pointer to the array of double values.</param>
        /// <param name="incx">Increment for indexing x (default is 1).</param>
        /// <returns>Sum of the magnitudes of the non-zero elements.</returns>
        double Asum(long n, [In] double* x, long incx = 1);

        /// <summary>
        /// Computes the sum of magnitudes of the non-zero elements in a sparse complex double-precision array.
        /// dzasum := |Re(x[0])| + |Im(x[0])| + |Re(x[1])| + |Im(x[1])| + ... + |Re(x[n-1])| + |Im(x[n-1])|
        /// </summary>
        /// <param name="n">Number of non-zero elements in the array.</param>
        /// <param name="x">Pointer to the array of complex values (as void*).</param>
        /// <param name="incx">Increment for indexing x (default is 1).</param>
        /// <returns>Sum of the magnitudes of the real and imaginary parts of the elements.</returns>
        double Asum(long n, [In] void* x, long incx = 1);


        // wrapper
        /// <summary>
        /// computes the sum of magnitudes of the elements
        /// dasum := |x[0]| + |x[1]| + ... + |x[n-1]|
        /// </summary>
        /// <typeparam name="T"> SPVector[double] </typeparam>
        /// <param name="n"> number of non-zero array elements </param>
        /// <param name="x"> sparse vector x </param>
        /// <param name="incx"> increment for indexing x </param>
        /// <returns> sum of the elements magnitudes </returns>
        double AsumD<T>(long n, T x,
            long incx = 1) where T : SPVector<double>;

        /// <summary>
        /// computes the sum of magnitudes of the elements
        /// dzasum := |Re(x[0])| + |Im(x[0])| + |Re(x[1])| + |Im(x[1])| + ... + |Re(x[n-1])| + |Im(x[n-1])|
        /// </summary>
        /// <typeparam name="T"> SPVector[Complex] </typeparam>
        /// <param name="n"> number of non-zero array elements </param>
        /// <param name="x"> sparse vector x </param>
        /// <param name="incx"> increment for indexing x </param>
        /// <returns> sum of the elements magnitudes </returns>
        double AsumZ<T>(long n, T x,
            long incx = 1) where T : SPVector<Complex>;

        #endregion
        #region ---- Axpy [D/Z] ----

        /// <summary>
        /// Adds a scalar multiple of a compressed sparse vector to a full-storage vector.
        /// Computes y := a * x + y, where x is a sparse vector and y is a dense vector.
        /// </summary>
        /// <param name="n">Number of non-zero elements in the sparse vector x.</param>
        /// <param name="a">Scalar multiplier.</param>
        /// <param name="x">Pointer to the values of the sparse vector x.</param>
        /// <param name="indx">Pointer to the indices of the sparse vector x.</param>
        /// <param name="y">Pointer to the dense vector y.</param>
        void Axpy(long n, double a,
            [In] double* x, [In] long* indx,
            [In, Out] double* y);

        /// <summary>
        /// Adds a scalar multiple of a compressed sparse complex vector to a full-storage complex vector.
        /// Computes y := a * x + y, where x is a sparse complex vector and y is a dense complex vector.
        /// </summary>
        /// <param name="n">Number of non-zero elements in the sparse vector x.</param>
        /// <param name="a">Pointer to the scalar multiplier (complex).</param>
        /// <param name="x">Pointer to the values of the sparse complex vector x.</param>
        /// <param name="indx">Pointer to the indices of the sparse vector x.</param>
        /// <param name="y">Pointer to the dense complex vector y.</param>
        void Axpy(long n, void* a,
            [In] void* x, [In] long* indx,
            [In, Out] void* y);


        // wrapper
        /// <summary>
        /// adds a scalar multiple of compressed sparse vector to
        /// a full-storage vector  y := a * x + y
        /// </summary>
        /// <typeparam name="T1"> SPVector[double] </typeparam>
        /// <typeparam name="T2"> Vector[double] </typeparam>
        /// <param name="n"> number of non-zero array elements </param>
        /// <param name="a"> scalar a </param>
        /// <param name="x"> sparse vector x </param>
        /// <param name="y"> dense vector y </param>
        void AxpyD<T1, T2>(long n, double a, T1 x, ref T2 y)
            where T1 : SPVector<double>
            where T2 : Vector<double>;

        /// <summary>
        /// adds a scalar multiple of compressed sparse vector to
        /// a full-storage vector  y := a * x + y
        /// </summary>
        /// <typeparam name="T1"> SPVector[Complex] </typeparam>
        /// <typeparam name="T2"> Vector[Complex] </typeparam>
        /// <param name="n"> number of non-zero array elements </param>
        /// <param name="a"> scalar a </param>
        /// <param name="x"> sparse vector x </param>
        /// <param name="y"> dense vector y </param>
        void AxpyZ<T1, T2>(long n, Complex a, T1 x, ref T2 y)
            where T1 : SPVector<Complex>
            where T2 : Vector<Complex>;

        #endregion
        #region ---- Copy [D/Z] ----

        /// <summary>
        /// Copies elements from the source array <paramref name="x"/> to the destination array <paramref name="y"/>.
        /// </summary>
        /// <param name="n">Number of elements to copy.</param>
        /// <param name="x">Pointer to the source array.</param>
        /// <param name="y">Pointer to the destination array.</param>
        /// <param name="incx">Increment for the elements of <paramref name="x"/>. Default is 1.</param>
        /// <param name="incy">Increment for the elements of <paramref name="y"/>. Default is 1.</param>
        void Copy(long n, [In] double* x,
            [In, Out] double* y,
            long incx = 1, long incy = 1);

        /// <summary>
        /// Copies elements from the source complex array <paramref name="x"/> to the destination complex array <paramref name="y"/>.
        /// </summary>
        /// <param name="n">Number of elements to copy.</param>
        /// <param name="x">Pointer to the source complex array.</param>
        /// <param name="y">Pointer to the destination complex array.</param>
        /// <param name="incx">Increment for the elements of <paramref name="x"/>. Default is 1.</param>
        /// <param name="incy">Increment for the elements of <paramref name="y"/>. Default is 1.</param>
        void Copy(long n, [In] void* x,
            [In, Out] void* y,
            long incx = 1, long incy = 1);


        // wrapper
        /// <summary>
        /// copies x to y
        /// y := x
        /// </summary>
        /// <typeparam name="T"> SPVector[double] </typeparam>
        /// <param name="n"> number non-zero of elements </param>
        /// <param name="x"> sparse vector x </param>
        /// <param name="y"> sparse vector y </param>
        /// <param name="incx"> increment for indexing x </param>
        /// <param name="incy"> increment for indexing y </param>
        void CopyD<T>(long n, T x, ref T y,
            long incx = 1, long incy = 1) where T : SPVector<double>;

        /// <summary>
        /// copies x to y
        /// y := x
        /// </summary>
        /// <typeparam name="T"> SPVector[Complex] </typeparam>
        /// <param name="n"> number non-zero of elements </param>
        /// <param name="x"> sparse vector x </param>
        /// <param name="y"> sparse vector y </param>
        /// <param name="incx"> increment for indexing x </param>
        /// <param name="incy"> increment for indexing y </param>
        void CopyZ<T>(long n, T x, ref T y,
            long incx = 1, long incy = 1) where T : SPVector<Complex>;

        #endregion
        #region ---- Dot [D/Z] ----

        /// <summary>
        /// Computes the dot product of a sparse vector and a dense vector.
        /// res = x[0]*y[indx[0]] + x[1]*y[indx[1]] +...+ x[nz-1]*y[indx[nz-1]]
        /// </summary>
        /// <param name="n">Number of non-zero elements in the sparse vector <paramref name="x"/>.</param>
        /// <param name="x">Pointer to the sparse vector values.</param>
        /// <param name="indx">Pointer to the indices of the non-zero elements in the dense vector <paramref name="y"/>.</param>
        /// <param name="y">Pointer to the dense vector values.</param>
        /// <returns>The result of the dot product of <paramref name="x"/> and <paramref name="y"/>.</returns>
        double Dot(long n, [In] double* x, [In] long* indx,
            [In] double* y);

        /// <summary>
        /// Computes the dot product of a sparse complex vector and a dense complex vector.
        /// res = x[0]*y[indx[0]] + x[1]*y[indx[1]] +...+ x[nz-1]*y[indx[nz-1]]
        /// </summary>
        /// <param name="n">Number of non-zero elements in the sparse vector <paramref name="x"/>.</param>
        /// <param name="x">Pointer to the sparse complex vector values.</param>
        /// <param name="indx">Pointer to the indices of the non-zero elements in the dense vector <paramref name="y"/>.</param>
        /// <param name="y">Pointer to the dense complex vector values.</param>
        /// <param name="dotu">Pointer to the result of the dot product (output parameter).</param>
        void Dot(long n, [In] void* x, [In] long* indx,
            [In] void* y, void* dotu);

        
        // wrapper
        /// <summary>
        /// computes a vector-vector dot product
        /// res = x[0]*y[indx[0]] + x[1]*y[indx[1]] +...+ x[nz-1]*y[indx[nz-1]]
        /// </summary>
        /// <typeparam name="T1"> SPVector[double] </typeparam>
        /// <typeparam name="T2"> Vector[double] </typeparam>
        /// <param name="n"> number of non-zero elements in x </param>
        /// <param name="x"> sparse vector x </param>
        /// <param name="y"> dense vector y </param>
        /// <returns> result of the dot product of x and y </returns>
        double DotD<T1, T2>(long n, T1 x, T2 y)
            where T1 : SPVector<double>
            where T2 : Vector<double>;

        /// <summary>
        /// computes a vector-vector dot product
        /// res = x[0]*y[indx[0]] + x[1]*y[indx[1]] +...+ x[nz-1]*y[indx[nz-1]]
        /// </summary>
        /// <typeparam name="T1"> SPVector[double] </typeparam>
        /// <typeparam name="T2"> Vector[double] </typeparam>
        /// <param name="n"> number of non-zero elements in x </param>
        /// <param name="x"> sparse vector x </param>
        /// <param name="y"> dense vector y </param>
        /// <returns> result of the dot product of x and y </returns>
        Complex DotZ<T1, T2>(long n, T1 x, T2 y)
            where T1 : SPVector<Complex>
            where T2 : Vector<Complex>;

        #endregion
        #region ---- Dotc [Z] ----

        /// <summary>
        /// Computes the dot product of a conjugated complex sparse vector with a dense complex vector.
        /// <para>res = conj(x[0])*y[indx[0]] + ... + conj(x[n-1])*y[indx[n-1]]</para>
        /// </summary>
        /// <param name="n">Number of non-zero elements in the sparse vector x.</param>
        /// <param name="x">Pointer to the sparse complex vector x (to be conjugated).</param>
        /// <param name="indx">Pointer to the index array for the sparse vector x.</param>
        /// <param name="y">Pointer to the dense complex vector y.</param>
        /// <param name="dotc">Pointer to the result (output complex value).</param>
        void Dotc(long n, [In] void* x, [In] long* indx,
            [In] void* y, void* dotc);


        // wrapper
        /// <summary>
        /// computes a dot product of a conjugated vector with another vector
        /// res = conj(x[0])*y[0] + ... + conj(x[n-1])*y[n-1]
        /// </summary>
        /// <typeparam name="T1"> SPVector[double] </typeparam>
        /// <typeparam name="T2"> Vector[double] </typeparam>
        /// <param name="n"> number of non-zero elements in x </param>
        /// <param name="x"> sparse vector x (to be conjugated) </param>
        /// <param name="y"> dense vector y </param>
        /// <returns> result of the conjugate product </returns>
        Complex DotcZ<T1, T2>(long n, T1 x, T2 y)
            where T1 : SPVector<Complex>
            where T2 : Vector<Complex>;

        #endregion
        #region ---- Nrm2 [D/Z] ----

        /// <summary>
        /// Computes the Euclidean norm (L2 norm) of a vector of doubles.
        /// </summary>
        /// <param name="n">The number of elements in the vector.</param>
        /// <param name="x">Pointer to the vector of doubles.</param>
        /// <param name="incx">The increment for the elements of <paramref name="x"/>. Default is 1.</param>
        /// <returns>The Euclidean norm of the vector.</returns>
        double Nrm2(long n, [In] double* x, long incx = 1);

        /// <summary>
        /// Computes the Euclidean norm (L2 norm) of a vector of complex numbers.
        /// </summary>
        /// <param name="n">The number of elements in the vector.</param>
        /// <param name="x">Pointer to the vector of complex numbers.</param>
        /// <param name="incx">The increment for the elements of <paramref name="x"/>. Default is 1.</param>
        /// <returns>The Euclidean norm of the vector.</returns>
        double Nrm2(long n, [In] void* x, long incx = 1);


        // wrapper
        /// <summary>
        /// computes the Euclidean norm of an array
        /// res = ||x||
        /// </summary>
        /// <typeparam name="T"> SPVector[double] </typeparam>
        /// <param name="n"> number of non-zero elements in x </param>
        /// <param name="x"> sparse vector x </param>
        /// <param name="incx"> increment for indexing x </param>
        /// <returns> Euclidean norm </returns>
        double Nrm2D<T>(long n, T x,
            long incx = 1) where T : SPVector<double>;

        /// <summary>
        /// computes the Euclidean norm of an array
        /// res = ||x||
        /// </summary>
        /// <typeparam name="T"> SPVector[Complex] </typeparam>
        /// <param name="n"> number of non-zero elements in x </param>
        /// <param name="x"> sparse vector x </param>
        /// <param name="incx"> increment for indexing x </param>
        /// <returns> Euclidean norm </returns>
        double Nrm2Z<T>(long n, T x,
            long incx = 1) where T : SPVector<Complex>;

        #endregion
        #region ---- Rot [D] ----

        /// <summary>
        /// Performs a Givens rotation of points in the plane for sparse vectors.
        /// </summary>
        /// <param name="n">Number of non-zero elements in <paramref name="x"/>.</param>
        /// <param name="x">Pointer to the sparse vector <c>x</c>.</param>
        /// <param name="indx">Pointer to the index array for <c>y</c>.</param>
        /// <param name="y">Pointer to the dense vector <c>y</c>.</param>
        /// <param name="c">Cosine component of the rotation.</param>
        /// <param name="s">Sine component of the rotation.</param>
        /// <remarks>
        /// For each i: 
        /// <c>x[i] = c * x[i] + s * y[indx[i]]</c><br/>
        /// <c>y[indx[i]] = c * y[indx[i]] - s * x[i]</c>
        /// </remarks>
        void Rot(long n, [In, Out] double* x, [In] long* indx,
            [In, Out] double* y, double c, double s);


        // wrapper
        /// <summary>
        /// performs rotation of points in the plane
        /// x[i] = c*x[i] + s*y[indx[i]]
        /// y[indx[i]] = c*y[indx[i]] - s*x[i]
        /// </summary>
        /// <typeparam name="T1"> SPVector[double] </typeparam>
        /// <typeparam name="T2"> Vector[double] </typeparam>
        /// <param name="n"> number of non-zero elements in x </param>
        /// <param name="x"> sparse vector x </param>
        /// <param name="y"> array y </param>
        /// <param name="c"> scalar c </param>
        /// <param name="s"> scalar s </param>
        void RotD<T1, T2>(long n, ref T1 x, ref T2 y,
            double c, double s)
            where T1 : SPVector<double>
            where T2 : Vector<double>;

        #endregion
        #region ---- Scal [D/Z] ----

        /// <summary>
        /// Scales a vector by a scalar value (double precision, dense).
        /// </summary>
        /// <param name="n">Number of elements in the vector.</param>
        /// <param name="a">Scalar multiplier.</param>
        /// <param name="x">Pointer to the vector to be scaled.</param>
        /// <param name="incx">Increment for the elements of <paramref name="x"/> (default is 1).</param>
        void Scal(long n, double a,
            [In, Out] double* x,
            long incx = 1);

        /// <summary>
        /// Scales a vector by a scalar value (double precision, complex, dense).
        /// </summary>
        /// <param name="n">Number of elements in the vector.</param>
        /// <param name="a">Scalar multiplier.</param>
        /// <param name="x">Pointer to the complex vector to be scaled.</param>
        /// <param name="incx">Increment for the elements of <paramref name="x"/> (default is 1).</param>
        void Scal(long n, double a,
            [In, Out] void* x,
            long incx = 1);

        /// <summary>
        /// Scales a vector by a scalar value (complex precision, dense).
        /// </summary>
        /// <param name="n">Number of elements in the vector.</param>
        /// <param name="a">Pointer to the complex scalar multiplier.</param>
        /// <param name="x">Pointer to the complex vector to be scaled.</param>
        /// <param name="incx">Increment for the elements of <paramref name="x"/> (default is 1).</param>
        void Scal(long n, void* a,
            [In, Out] void* x,
            long incx = 1);


        // wrapper
        /// <summary>
        /// computes the product of an array by a scalar
        /// x = a*x 
        /// </summary>
        /// <typeparam name="T"> SPVector[double] </typeparam>
        /// <param name="n"> number of non-zero elements in x </param>
        /// <param name="a"> scalar a </param>
        /// <param name="x"> sparse vector x </param>
        /// <param name="incx"> increment for indexing x </param>
        void ScalD<T>(long n, double a, ref T x,
            long incx = 1) where T : SPVector<double>;

        /// <summary>
        /// computes the product of an array by a scalar
        /// x = a*x 
        /// </summary>
        /// <typeparam name="T"> SPVector[Complex] </typeparam>
        /// <param name="n"> number of non-zero elements in x </param>
        /// <param name="a"> scalar a </param>
        /// <param name="x"> sparse vector x </param>
        /// <param name="incx"> increment for indexing x </param>
        void ScalZd<T>(long n, double a, ref T x,
            long incx = 1) where T : SPVector<Complex>;

        /// <summary>
        /// computes the product of an array by a scalar
        /// x = a*x 
        /// </summary>
        /// <typeparam name="T"> SPVector[Complex] </typeparam>
        /// <param name="n"> number of non-zero elements in x </param>
        /// <param name="a"> scalar a </param>
        /// <param name="x"> sparse vector x </param>
        /// <param name="incx"> increment for indexing x </param>
        void ScalZ<T>(long n, Complex a, ref T x,
            long incx = 1) where T : SPVector<Complex>;

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


        // wrapper
        /// <summary>
        /// finds the index of the element with maximum absolute value
        /// </summary>
        /// <typeparam name="T"> SPVector[double] </typeparam>
        /// <param name="n"> number of non-zero elements in x </param>
        /// <param name="x"> sparse vector x </param>
        /// <param name="incx"> increment for indexing x </param>
        /// <returns> index of the element with largest absolute value </returns>
        long IamaxD<T>(long n, T x,
            long incx = 1) where T : SPVector<double>;

        /// <summary>
        /// finds the index of the element with maximum absolute value
        /// </summary>
        /// <typeparam name="T"> SPVector[Complex] </typeparam>
        /// <param name="n"> number of non-zero elements in x </param>
        /// <param name="x"> sparse vector x </param>
        /// <param name="incx"> increment for indexing x </param>
        /// <returns> index of the element with largest absolute value </returns>
        long IamaxZ<T>(long n, T x,
            long incx = 1) where T : SPVector<Complex>;

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


        // wrapper
        /// <summary>
        /// finds the index of the element with minimum absolute value
        /// </summary>
        /// <typeparam name="T"> SPVector[double] </typeparam>
        /// <param name="n"> number of non-zero elements in x </param>
        /// <param name="x"> sparse vector x </param>
        /// <param name="incx"> increment for indexing x </param>
        /// <returns> index of the element with smallest absolute value </returns>
        long IaminD<T>(long n, T x,
            long incx = 1) where T : SPVector<double>;

        /// <summary>
        /// finds the index of the element with minimum absolute value
        /// </summary>
        /// <typeparam name="T"> SPVector[Complex] </typeparam>
        /// <param name="n"> number of non-zero elements in x </param>
        /// <param name="x"> sparse vector x </param>
        /// <param name="incx"> increment for indexing x </param>
        /// <returns> index of the element with smallest absolute value </returns>
        long IaminZ<T>(long n, T x,
            long incx = 1) where T : SPVector<Complex>;

        #endregion
        #region ---- Gthr [D/Z] ----

        /// <summary>
        /// Gathers elements from a dense vector <paramref name="y"/> into a sparse vector <paramref name="x"/>,
        /// using the indices specified by <paramref name="indx"/>.
        /// For each i in [0, n), sets x[i] = y[indx[i]].
        /// </summary>
        /// <param name="n">Number of elements to gather.</param>
        /// <param name="y">Pointer to the dense source vector.</param>
        /// <param name="x">Pointer to the sparse destination vector.</param>
        /// <param name="indx">Pointer to the array of indices specifying which elements to gather.</param>
        void Gthr(long n, [In] double* y,
            [In, Out] double* x, [In] long* indx);

        /// <summary>
        /// Gathers elements from a dense complex vector <paramref name="y"/> into a sparse complex vector <paramref name="x"/>,
        /// using the indices specified by <paramref name="indx"/>.
        /// For each i in [0, n), sets x[i] = y[indx[i]].
        /// </summary>
        /// <param name="n">Number of elements to gather.</param>
        /// <param name="y">Pointer to the dense complex source vector.</param>
        /// <param name="x">Pointer to the sparse complex destination vector.</param>
        /// <param name="indx">Pointer to the array of indices specifying which elements to gather.</param>
        void Gthr(long n, [In] void* y,
            [In, Out] void* x, [In] long* indx);


        // wrapper
        /// <summary>
        /// gathers a full-storage sparse vector's elements into
        /// compressed form x[i] = y[indx[i]]
        /// </summary>
        /// <typeparam name="T1"> Vector[double] </typeparam>
        /// <typeparam name="T2"> SPVector[double] </typeparam>
        /// <param name="n"> number of non-zero array elements </param>
        /// <param name="y"> dense vector y </param>
        /// <param name="x"> sparse vector x </param>
        void GthrD<T1, T2>(long n, T1 y, ref T2 x)
            where T1 : Vector<double>
            where T2 : SPVector<double>;

        /// <summary>
        /// gathers a full-storage sparse vector's elements into
        /// compressed form x[i] = y[indx[i]]
        /// </summary>
        /// <typeparam name="T1"> Vector[Complex] </typeparam>
        /// <typeparam name="T2"> SPVector[Complex] </typeparam>
        /// <param name="n"> number of non-zero array elements </param>
        /// <param name="y"> dense vector y </param>
        /// <param name="x"> sparse vector x </param>
        void GthrZ<T1, T2>(long n, T1 y, ref T2 x)
            where T1 : Vector<Complex>
            where T2 : SPVector<Complex>;

        #endregion
        #region ---- Gthrz [D/Z] ----

        /// <summary>
        /// Gathers a full-storage sparse vector's elements into compressed form, replacing them by zeros.
        /// For each i in [0, n-1]: x[i] = y[indx[i]]; y[indx[i]] = 0.
        /// </summary>
        /// <param name="n">Number of non-zero array elements.</param>
        /// <param name="y">Pointer to the dense vector y.</param>
        /// <param name="x">Pointer to the sparse vector x.</param>
        /// <param name="indx">Pointer to the index array.</param>
        void Gthrz(long n, [In, Out] double* y,
            [In, Out] double* x, [In] long* indx);

        /// <summary>
        /// Gathers a full-storage sparse complex vector's elements into compressed form, replacing them by zeros.
        /// For each i in [0, n-1]: x[i] = y[indx[i]]; y[indx[i]] = 0.
        /// </summary>
        /// <param name="n">Number of non-zero array elements.</param>
        /// <param name="y">Pointer to the dense complex vector y.</param>
        /// <param name="x">Pointer to the sparse complex vector x.</param>
        /// <param name="indx">Pointer to the index array.</param>
        void Gthrz(long n, [In, Out] void* y,
            [In, Out] void* x, [In] long* indx);


        // wrapper
        /// <summary>
        /// gathers a full-storage sparse vector's elements into
        /// compressed form, replacing them by zeros
        /// </summary>
        /// <typeparam name="T1"> Vector[double] </typeparam>
        /// <typeparam name="T2"> SPVector[double] </typeparam>
        /// <param name="n"> number of non-zero array elements </param>
        /// <param name="y"> dense vector y </param>
        /// <param name="x"> sparse vector x </param>
        void GthrzD<T1, T2>(long n, ref T1 y, ref T2 x)
            where T1 : Vector<double>
            where T2 : SPVector<double>;

        /// <summary>
        /// gathers a full-storage sparse vector's elements into
        /// compressed form, replacing them by zeros
        /// </summary>
        /// <typeparam name="T1"> Vector[Complex] </typeparam>
        /// <typeparam name="T2"> SPVector[Complex] </typeparam>
        /// <param name="n"> number of non-zero array elements </param>
        /// <param name="y"> dense vector y </param>
        /// <param name="x"> sparse vector x </param>
        void GthrzZ<T1, T2>(long n, ref T1 y, ref T2 x)
            where T1 : Vector<Complex>
            where T2 : SPVector<Complex>;

        #endregion
        #region ---- Sctr [D/Z] ----

        /// <summary>
        /// Converts a compressed sparse vector into full-storage form for double precision values.
        /// </summary>
        /// <param name="n">Number of non-zero elements in the sparse vector.</param>
        /// <param name="x">Pointer to the values of the sparse vector.</param>
        /// <param name="indx">Pointer to the indices of the sparse vector elements.</param>
        /// <param name="y">Pointer to the dense vector to be updated.</param>
        void Sctr(long n, [In] double* x, [In] long* indx,
            [Out] double* y);

        /// <summary>
        /// Converts a compressed sparse vector into full-storage form for complex values.
        /// </summary>
        /// <param name="n">Number of non-zero elements in the sparse vector.</param>
        /// <param name="x">Pointer to the values of the sparse vector (complex).</param>
        /// <param name="indx">Pointer to the indices of the sparse vector elements.</param>
        /// <param name="y">Pointer to the dense vector to be updated (complex).</param>
        void Sctr(long n, [In] void* x, [In] long* indx,
            [Out] void* y);


        // wrapper
        /// <summary>
        /// converts compressed sparse vector into full-storage form
        /// </summary>
        /// <typeparam name="T1"> SPVector[double] </typeparam>
        /// <typeparam name="T2"> Vector[double] </typeparam>
        /// <param name="n"> number of non-zero array elements </param>
        /// <param name="x"> sparse vector x </param>
        /// <param name="y"> dense vector y </param>
        void SctrD<T1, T2>(long n, T1 x, ref T2 y)
            where T1 : SPVector<double>
            where T2 : Vector<double>;

        /// <summary>
        /// converts compressed sparse vector into full-storage form
        /// </summary>
        /// <typeparam name="T1"> SPVector[Complex] </typeparam>
        /// <typeparam name="T2"> Vector[Complex] </typeparam>
        /// <param name="n"> number of non-zero array elements </param>
        /// <param name="x"> sparse vector x </param>
        /// <param name="y"> dense vector y </param>
        void SctrZ<T1, T2>(long n, T1 x, ref T2 y)
            where T1 : SPVector<Complex>
            where T2 : Vector<Complex>;

        #endregion

        #endregion
        #region Level 2 [DEPRECATED]

        // MKL_DEPRECATED ...

        #endregion
        #region Level 3 [DEPRECATED]

        // MKL_DEPRECATED ...

        #endregion
        #region QR routines

        #region ---- QR ----

        /// <summary>
        /// computes the QR decomposition for the matrix of a sparse
        /// linear system and calculates the solution A * x = b
        /// </summary>
        /// <param name="operation">Specifies operation op() on sparse matrix a.</param>
        /// <param name="a">Sparse matrix handle.</param>
        /// <param name="descr">Structure specifying sparse matrix properties.</param>
        /// <param name="layout">Describes the storage scheme for the dense matrix.</param>
        /// <param name="columns">Number of columns in matrix b.</param>
        /// <param name="x">Pointer to the solution array x (size at least rows * columns).</param>
        /// <param name="ldx">Specifies the leading dimension of matrix x.</param>
        /// <param name="b">Pointer to the right-hand side array b (size at least rows * columns).</param>
        /// <param name="ldb">Specifies the leading dimension of matrix b.</param>
        /// <returns>Result status of the operation.</returns>
        SPARSE_Status QR(SPARSE_Operation operation, IntPtr a,
            SPARSE_MatrixDescr descr, SPARSE_Layout layout,
            long columns, [In, Out] double* x, long ldx,
            [In] double* b, long ldb);


        // wrapper
        /// <summary>
        /// computes the QR decomposition for the matrix of a sparse
        /// linear system and calculates the solution A * x = b
        /// </summary>
        /// <param name="operation"> specifies operation op() on sparse matrix a </param>
        /// <param name="a"> sparse matrix handle </param>
        /// <param name="descr"> structure specifying sparse matrix properties </param>
        /// <param name="layout"> describes the storage scheme for the dense matrix </param>
        /// <param name="columns"> number of columns in matrix b </param>
        /// <param name="x"> array with size of at least rows * cols </param>
        /// <param name="ldx"> specifies the leading dimension of matrix x </param>
        /// <param name="b"> array with size of at least rows * cols </param>
        /// <param name="ldb"> specifies the leading dimension of matrix b </param>
        /// <returns> result status </returns>
        SPARSE_Status QR(SPARSE_Operation operation, IntPtr a,
            SPARSE_MatrixDescr descr, SPARSE_Layout layout,
            long columns, IntPtr x, long ldx,
            [In] IntPtr b, long ldb);

        #endregion
        #region ---- set hint ----

        /// <summary>
        /// defines the pivot strategy for further calls 
        /// mkl_sparse_? _qr
        /// </summary>
        /// <param name="a"> sparse matrix handle </param>
        /// <param name="hint"> value specifying whether to use pivoting </param>
        /// <returns> result status </returns>
        SPARSE_Status QR_SetHint(IntPtr a, SPARSE_QRHint hint);

        #endregion
        #region ---- reorder ----

        /// <summary>
        /// reordering step of SPARSE QR solver
        /// </summary>
        /// <param name="a"> sparse matrix handle </param>
        /// <param name="descr"> structure specifying sparse matrix properties </param>
        /// <returns> result status </returns>
        SPARSE_Status QR_Reorder(IntPtr a, SPARSE_MatrixDescr descr);

        #endregion
        #region ---- factorize ----

        /// <summary>
        /// factorization step of SPARSE QR solver
        /// </summary>
        /// <param name="a">Sparse matrix handle.</param>
        /// <param name="alt_values">
        /// Pointer to an array with alternative values; must be the size of the non-zeroes in the initial input matrix.
        /// </param>
        /// <returns>
        /// <see cref="SPARSE_Status"/> indicating the result of the operation.
        /// </returns>
        SPARSE_Status QR_Factorize(IntPtr a, double* alt_values);


        // wrapper
        /// <summary>
        /// factorization step of SPARSE QR solver
        /// </summary>
        /// <param name="a"> sparse matrix handle </param>
        /// <param name="alt_values"> array with alternative values; 
        /// must be the size of the non-zeroes in the initial input matrix </param>
        /// <returns> result status </returns>
        SPARSE_Status QR_Factorize(IntPtr a, IntPtr alt_values);

        #endregion
        #region ---- solve ----

        /// <summary>
        /// Solves a system of linear equations A * x = b using the QR decomposition for a sparse matrix.
        /// </summary>
        /// <param name="operation">Specifies the operation op() to be performed on the sparse matrix <paramref name="a"/>.</param>
        /// <param name="a">Handle to the sparse matrix.</param>
        /// <param name="alt_values">Pointer to an array of alternative values for the matrix (can be null).</param>
        /// <param name="layout">Describes the storage scheme for the dense matrix (row-major or column-major).</param>
        /// <param name="columns">Number of columns in the right-hand side matrix <paramref name="b"/>.</param>
        /// <param name="x">Pointer to the solution array (output), with size at least rows * columns.</param>
        /// <param name="ldx">Leading dimension of the solution matrix <paramref name="x"/>.</param>
        /// <param name="b">Pointer to the right-hand side array, with size at least rows * columns.</param>
        /// <param name="ldb">Leading dimension of the right-hand side matrix <paramref name="b"/>.</param>
        /// <returns>Status code indicating the result of the operation.</returns>
        SPARSE_Status QR_Solve(SPARSE_Operation operation, IntPtr a,
            double* alt_values, SPARSE_Layout layout,
            long columns, [In, Out] double* x, long ldx,
            [In] double* b, long ldb);


        // wrapper
        /// <summary>
        /// solving step of SPARSE QR solver
        /// </summary>
        /// <param name="operation"> specifies operation op() on sparse matrix a </param>
        /// <param name="a"> sparse matrix handle </param>
        /// <param name="alt_values"> array with alternative values </param>
        /// <param name="layout"> describes the storage scheme for the dense matrix </param>
        /// <param name="columns"> number of columns in matrix b </param>
        /// <param name="x"> array with size of at least rows * cols </param>
        /// <param name="ldx"> specifies the leading dimension of matrix x </param>
        /// <param name="b"> array with size of at least rows * cols </param>
        /// <param name="ldb"> specifies the leading dimension of matrix b </param>
        /// <returns> result status </returns>
        SPARSE_Status QR_Solve(SPARSE_Operation operation, IntPtr a,
            IntPtr alt_values, SPARSE_Layout layout,
            long columns, IntPtr x, long ldx, IntPtr b, long ldb);

        #endregion
        #region ---- qmult ----

        /// <summary>
        /// Computes the matrix product Q * x or Q<sup>T</sup> * x for the QR decomposition of a sparse matrix.
        /// </summary>
        /// <param name="operation">Specifies the operation op() on the sparse matrix <paramref name="a"/>.</param>
        /// <param name="a">Handle to the sparse matrix.</param>
        /// <param name="layout">Describes the storage scheme for the dense matrix.</param>
        /// <param name="columns">Number of columns in matrix <paramref name="b"/>.</param>
        /// <param name="x">Pointer to the input matrix x (size: rows * columns).</param>
        /// <param name="ldx">Specifies the leading dimension of matrix x.</param>
        /// <param name="b">Pointer to the output matrix b (size: rows * columns).</param>
        /// <param name="ldb">Specifies the leading dimension of matrix b.</param>
        /// <returns>Status of the operation as <see cref="SPARSE_Status"/>.</returns>
        SPARSE_Status QR_QMult(SPARSE_Operation operation, IntPtr a,
            SPARSE_Layout layout, long columns, [In, Out] double* x, long ldx,
            [In] double* b, long ldb);


        // wrapper
        /// <summary>
        /// first stage of the solving step of the SPARSE QR solver
        /// </summary>
        /// <param name="operation"> specifies operation op() on sparse matrix a </param>
        /// <param name="a"> sparse matrix handle </param>
        /// <param name="layout"> describes the storage scheme for the dense matrix </param>
        /// <param name="columns"> number of columns in matrix b </param>
        /// <param name="x"> array with size of at least rows * cols </param>
        /// <param name="ldx"> specifies the leading dimension of matrix x </param>
        /// <param name="b"> array with size of at least rows * cols </param>
        /// <param name="ldb"> specifies the leading dimension of matrix b </param>
        /// <returns> result status </returns>
        SPARSE_Status QR_QMult(SPARSE_Operation operation, IntPtr a,
            SPARSE_Layout layout, long columns, IntPtr x, long ldx,
            [In] IntPtr b, long ldb);

        #endregion
        #region ---- rsolve ----

        /// <summary>
        /// Performs the second stage of the solving step of the SPARSE QR solver.
        /// Solves the system R * x = b, where R is the upper triangular matrix from the QR factorization.
        /// </summary>
        /// <param name="operation">Specifies the operation op() on the sparse matrix <paramref name="a"/>.</param>
        /// <param name="a">Sparse matrix handle.</param>
        /// <param name="layout">Describes the storage scheme for the dense matrix.</param>
        /// <param name="columns">Number of columns in matrix <paramref name="b"/>.</param>
        /// <param name="x">Pointer to the solution array with size of at least rows * columns.</param>
        /// <param name="ldx">Specifies the leading dimension of matrix <paramref name="x"/>.</param>
        /// <param name="b">Pointer to the right-hand side array with size of at least rows * columns.</param>
        /// <param name="ldb">Specifies the leading dimension of matrix <paramref name="b"/>.</param>
        /// <returns>Returns the result status of the operation.</returns>
        SPARSE_Status QR_RSolve(SPARSE_Operation operation, IntPtr a,
            SPARSE_Layout layout, long columns, [In, Out] double* x, long ldx,
            [In] double* b, long ldb);


        // wrapper
        /// <summary>
        /// second stage of the solving step of the SPARSE solver
        /// </summary>
        /// <param name="operation"> specifies operation op() on sparse matrix a </param>
        /// <param name="a"> sparse matrix handle </param>
        /// <param name="layout"> describes the storage scheme for the dense matrix </param>
        /// <param name="columns"> number of columns in matrix b </param>
        /// <param name="x"> array with size of at least rows * cols </param>
        /// <param name="ldx"> specifies the leading dimension of matrix x </param>
        /// <param name="b"> array with size of at least rows * cols </param>
        /// <param name="ldb"> specifies the leading dimension of matrix b </param>
        /// <returns> result status </returns>
        SPARSE_Status QR_RSolve(SPARSE_Operation operation, IntPtr a,
            SPARSE_Layout layout, long columns, IntPtr x, long ldx,
            [In] IntPtr b, long ldb);

        #endregion

        #endregion
        #region inspector-executer

        #region ---- CreateCOO [D/Z] ----

        /// <summary>
        /// Creates a handle for a sparse matrix in COO format with double values.
        /// </summary>
        /// <param name="a">Reference to the sparse matrix handle to be created.</param>
        /// <param name="indexing">Specifies zero-based (C-style) or one-based (Fortran-style) indexing.</param>
        /// <param name="rows">Number of rows of the matrix.</param>
        /// <param name="cols">Number of columns of the matrix.</param>
        /// <param name="nnz">Number of non-zero elements in the matrix.</param>
        /// <param name="row_indx">Pointer to the array of row indices for non-zero elements.</param>
        /// <param name="col_indx">Pointer to the array of column indices for non-zero elements.</param>
        /// <param name="values">Pointer to the array of non-zero values.</param>
        /// <returns>Status of the sparse matrix creation operation.</returns>
        SPARSE_Status CreateCOO(ref IntPtr a,
            SPARSE_IndexBase indexing, long rows, long cols, long nnz,
            [In] long* row_indx, [In] long* col_indx, [In] double* values);

        /// <summary>
        /// Creates a handle for a sparse matrix in COO format with complex values.
        /// </summary>
        /// <param name="a">Reference to the sparse matrix handle to be created.</param>
        /// <param name="indexing">Specifies zero-based (C-style) or one-based (Fortran-style) indexing.</param>
        /// <param name="rows">Number of rows of the matrix.</param>
        /// <param name="cols">Number of columns of the matrix.</param>
        /// <param name="nnz">Number of non-zero elements in the matrix.</param>
        /// <param name="row_indx">Pointer to the array of row indices for non-zero elements.</param>
        /// <param name="col_indx">Pointer to the array of column indices for non-zero elements.</param>
        /// <param name="values">Pointer to the array of non-zero values (complex type).</param>
        /// <returns>Status of the sparse matrix creation operation.</returns>
        SPARSE_Status CreateCOO(ref IntPtr a,
            SPARSE_IndexBase indexing, long rows, long cols, long nnz,
            [In] long* row_indx, [In] long* col_indx, [In] void* values);


        // wrapper
        /// <summary>
        /// creates a handle for a sparse matrix in COO format
        /// </summary>
        /// <typeparam name="T1"> ArrayBase[long] </typeparam>
        /// <typeparam name="T2"> ArrayBase[double] </typeparam>
        /// <param name="a"> sparse matrix handle </param>
        /// <param name="indexing"> zero-based (C-style) or one-based (Fortran-style) </param>
        /// <param name="rows"> number of rows of matrix a </param>
        /// <param name="cols"> number of cols of matrix a </param>
        /// <param name="nnz"> number of non-zero elements of the matrix a </param>
        /// <param name="row_indx"> row indices of the non-zero elements </param>
        /// <param name="col_indx"> column indices of the non-zero elements </param>
        /// <param name="values"> values of the non-zero elements </param>
        /// <returns> result status </returns>
        SPARSE_Status CreateCOOD<T1, T2>(ref IntPtr a,
            SPARSE_IndexBase indexing, long rows, long cols, long nnz,
            T1 row_indx, T1 col_indx, T2 values)
            where T1 : DenseArrayBase<long>
            where T2 : DenseArrayBase<double>;

        /// <summary>
        /// creates a handle for a sparse matrix in COO format
        /// </summary>
        /// <typeparam name="T1"> ArrayBase[long] </typeparam>
        /// <typeparam name="T2"> ArrayBase[Complex] </typeparam>
        /// <param name="a"> sparse matrix handle </param>
        /// <param name="indexing"> zero-based (C-style) or one-based (Fortran-style) </param>
        /// <param name="rows"> number of rows of matrix a </param>
        /// <param name="cols"> number of cols of matrix a </param>
        /// <param name="nnz"> number of non-zero elements of the matrix a </param>
        /// <param name="row_indx"> row indices of the non-zero elements </param>
        /// <param name="col_indx"> column indices of the non-zero elements </param>
        /// <param name="values"> values of the non-zero elements </param>
        /// <returns> result status </returns>
        SPARSE_Status CreateCOOZ<T1, T2>(ref IntPtr a,
            SPARSE_IndexBase indexing, long rows, long cols, long nnz,
            T1 row_indx, T1 col_indx, T2 values)
            where T1 : DenseArrayBase<long>
            where T2 : DenseArrayBase<Complex>;

        #endregion
        #region ---- CreateCSR [D/Z] ----

        /// <summary>
        /// Creates a handle for a sparse matrix in CSR format with double values.
        /// </summary>
        /// <param name="a">Reference to the sparse matrix handle to be created.</param>
        /// <param name="indexing">Specifies zero-based (C-style) or one-based (Fortran-style) indexing.</param>
        /// <param name="rows">Number of rows of the matrix.</param>
        /// <param name="cols">Number of columns of the matrix.</param>
        /// <param name="rows_start">Pointer to the array of row start indices (size: rows).</param>
        /// <param name="rows_end">Pointer to the array of row end indices (size: rows).</param>
        /// <param name="col_indx">Pointer to the array of column indices of the non-zero elements.</param>
        /// <param name="values">Pointer to the array of non-zero values (double).</param>
        /// <returns>Status of the sparse matrix creation operation.</returns>
        SPARSE_Status CreateCSR(ref IntPtr a,
            SPARSE_IndexBase indexing, long rows, long cols,
            [In] long* rows_start, [In] long* rows_end,
            [In] long* col_indx, [In] double* values);

        /// <summary>
        /// Creates a handle for a sparse matrix in CSR format with complex values.
        /// </summary>
        /// <param name="a">Reference to the sparse matrix handle to be created.</param>
        /// <param name="indexing">Specifies zero-based (C-style) or one-based (Fortran-style) indexing.</param>
        /// <param name="rows">Number of rows of the matrix.</param>
        /// <param name="cols">Number of columns of the matrix.</param>
        /// <param name="rows_start">Pointer to the array of row start indices (size: rows).</param>
        /// <param name="rows_end">Pointer to the array of row end indices (size: rows).</param>
        /// <param name="col_indx">Pointer to the array of column indices of the non-zero elements.</param>
        /// <param name="values">Pointer to the array of non-zero values (complex, as void*).</param>
        /// <returns>Status of the sparse matrix creation operation.</returns>
        SPARSE_Status CreateCSR(ref IntPtr a,
            SPARSE_IndexBase indexing, long rows, long cols,
            [In] long* rows_start, [In] long* rows_end,
            [In] long* col_indx, [In] void* values);


        // wrapper
        /// <summary>
        /// creates a handle for a sparse matrix in CSR format
        /// </summary>
        /// <typeparam name="T1"> ArrayBase[long] </typeparam>
        /// <typeparam name="T2"> ArrayBase[double] </typeparam>
        /// <param name="a"> sparse matrix handle </param>
        /// <param name="indexing"> zero-based (C-style) or one-based (Fortran-style) </param>
        /// <param name="rows"> number of rows of matrix a </param>
        /// <param name="cols"> number of cols of matrix a </param>
        /// <param name="row_ptr"> row start/end indices of the non-zero elements </param>
        /// <param name="col_indx"> column indices of the non-zero elements </param>
        /// <param name="values"> values of the non-zero elements </param>
        /// <returns> result status </returns>
        SPARSE_Status CreateCSRD<T1, T2>(ref IntPtr a,
            SPARSE_IndexBase indexing, long rows, long cols,
            T1 row_ptr, T1 col_indx, T2 values)
            where T1 : DenseArrayBase<long>
            where T2 : DenseArrayBase<double>;

        /// <summary>
        /// creates a handle for a sparse matrix in CSR format
        /// </summary>
        /// <typeparam name="T1"> ArrayBase[long] </typeparam>
        /// <typeparam name="T2"> ArrayBase[Complex] </typeparam>
        /// <param name="a"> sparse matrix handle </param>
        /// <param name="indexing"> zero-based (C-style) or one-based (Fortran-style) </param>
        /// <param name="rows"> number of rows of matrix a </param>
        /// <param name="cols"> number of cols of matrix a </param>
        /// <param name="row_ptr"> row start/end indices of the non-zero elements </param>
        /// <param name="col_indx"> column indices of the non-zero elements </param>
        /// <param name="values"> values of the non-zero elements </param>
        /// <returns> result status </returns>
        SPARSE_Status CreateCSRZ<T1, T2>(ref IntPtr a,
            SPARSE_IndexBase indexing, long rows, long cols,
            T1 row_ptr, T1 col_indx, T2 values)
            where T1 : DenseArrayBase<long>
            where T2 : DenseArrayBase<Complex>;

        #endregion
        #region ---- CreateCSC [D/Z] ----

        /// <summary>
        /// Creates a handle for a sparse matrix in CSC (Compressed Sparse Column) format with double values.
        /// </summary>
        /// <param name="a">Reference to the sparse matrix handle to be created.</param>
        /// <param name="indexing">Specifies zero-based (C-style) or one-based (Fortran-style) indexing.</param>
        /// <param name="rows">Number of rows in the matrix.</param>
        /// <param name="cols">Number of columns in the matrix.</param>
        /// <param name="cols_start">Pointer to the array of column start indices.</param>
        /// <param name="cols_end">Pointer to the array of column end indices.</param>
        /// <param name="row_indx">Pointer to the array of row indices for non-zero elements.</param>
        /// <param name="values">Pointer to the array of non-zero values (double).</param>
        /// <returns>Status of the sparse matrix creation operation.</returns>
        SPARSE_Status CreateCSC(ref IntPtr a,
            SPARSE_IndexBase indexing, long rows, long cols,
            [In] long* cols_start, [In] long* cols_end,
            [In] long* row_indx, [In] double* values);

        /// <summary>
        /// Creates a handle for a sparse matrix in CSC (Compressed Sparse Column) format with complex values.
        /// </summary>
        /// <param name="a">Reference to the sparse matrix handle to be created.</param>
        /// <param name="indexing">Specifies zero-based (C-style) or one-based (Fortran-style) indexing.</param>
        /// <param name="rows">Number of rows in the matrix.</param>
        /// <param name="cols">Number of columns in the matrix.</param>
        /// <param name="cols_start">Pointer to the array of column start indices.</param>
        /// <param name="cols_end">Pointer to the array of column end indices.</param>
        /// <param name="row_indx">Pointer to the array of row indices for non-zero elements.</param>
        /// <param name="values">Pointer to the array of non-zero values (complex, as void*).</param>
        /// <returns>Status of the sparse matrix creation operation.</returns>
        SPARSE_Status CreateCSC(ref IntPtr a,
            SPARSE_IndexBase indexing, long rows, long cols,
            [In] long* cols_start, [In] long* cols_end,
            [In] long* row_indx, [In] void* values);


        // wrapper
        /// <summary>
        /// creates a handle for a sparse matrix in CSC format
        /// </summary>
        /// <typeparam name="T1"> ArrayBase[long] </typeparam>
        /// <typeparam name="T2"> ArrayBase[double] </typeparam>
        /// <param name="a"> sparse matrix handle </param>
        /// <param name="indexing"> zero-based (C-style) or one-based (Fortran-style) </param>
        /// <param name="rows"> number of rows of matrix a </param>
        /// <param name="cols"> number of cols of matrix a </param>
        /// <param name="col_ptr"> column start/end indices of the non-zero elements </param>
        /// <param name="row_indx"> row indices of the non-zero elements </param>
        /// <param name="values"> values of the non-zero elements </param>
        /// <returns> result status </returns>
        SPARSE_Status CreateCSCD<T1, T2>(ref IntPtr a,
            SPARSE_IndexBase indexing, long rows, long cols,
            T1 col_ptr, T1 row_indx, T2 values)
            where T1 : DenseArrayBase<long>
            where T2 : DenseArrayBase<double>;

        /// <summary>
        /// creates a handle for a sparse matrix in CSC format
        /// </summary>
        /// <typeparam name="T1"> ArrayBase[long] </typeparam>
        /// <typeparam name="T2"> ArrayBase[Complex] </typeparam>
        /// <param name="a"> sparse matrix handle </param>
        /// <param name="indexing"> zero-based (C-style) or one-based (Fortran-style) </param>
        /// <param name="rows"> number of rows of matrix a </param>
        /// <param name="cols"> number of cols of matrix a </param>
        /// <param name="col_ptr"> column start/end indices of the non-zero elements </param>
        /// <param name="row_indx"> row indices of the non-zero elements </param>
        /// <param name="values"> values of the non-zero elements </param>
        /// <returns> result status </returns>
        SPARSE_Status CreateCSCZ<T1, T2>(ref IntPtr a,
            SPARSE_IndexBase indexing, long rows, long cols,
            T1 col_ptr, T1 row_indx, T2 values)
            where T1 : DenseArrayBase<long>
            where T2 : DenseArrayBase<Complex>;

        #endregion
        #region ---- Copy ----

        /// <summary>
        /// creates a copy of a sparse matrix handle
        /// </summary>
        /// <param name="source"> handle of the source sparse matrix </param>
        /// <param name="descr"> structure specifying sparse matrix properties </param>
        /// <param name="dest"> copied handle containing internal data </param>
        /// <returns> result status </returns>
        SPARSE_Status Copy(IntPtr source, SPARSE_MatrixDescr descr,
            ref IntPtr dest);

        #endregion
        #region ---- Destroy ----

        /// <summary>
        /// frees memory allocated for a sparse matrix handle
        /// </summary>
        /// <param name="a"> handle of the sparse matrix </param>
        /// <returns> result status </returns>
        SPARSE_Status Destroy(IntPtr a);

        #endregion
        #region ---- Convert ----

        /// <summary>
        /// converts internal matrix representation to CSR format
        /// </summary>
        /// <param name="source"> handle of the source sparse matrix </param>
        /// <param name="operation"> specifies operation op() on input matrix </param>
        /// <param name="dest"> result handle containing internal data </param>
        /// <returns> result status </returns>
        SPARSE_Status ConvertCSR(IntPtr source, SPARSE_Operation operation,
            ref IntPtr dest);

        #endregion
        #region ---- ExportCSR [D/Z] ---

        /// <summary>
        /// Exports a sparse matrix in CSR format with double values.
        /// </summary>
        /// <param name="a">Sparse matrix handle.</param>
        /// <param name="indexing">Zero-based (C-style) or one-based (Fortran-style) indexing.</param>
        /// <param name="rows">Pointer to the number of rows of the matrix.</param>
        /// <param name="cols">Pointer to the number of columns of the matrix.</param>
        /// <param name="row_start">Pointer to the array of row start indices.</param>
        /// <param name="row_end">Pointer to the array of row end indices.</param>
        /// <param name="col_indx">Pointer to the array of column indices.</param>
        /// <param name="values">Pointer to the array of non-zero values.</param>
        /// <returns>Status of the export operation.</returns>
        SPARSE_Status ExportCSR(IntPtr a,
            ref SPARSE_IndexBase indexing, long* rows, long* cols,
            long** row_start, long** row_end,
            long** col_indx, double** values);

        /// <summary>
        /// Exports a sparse matrix in CSR format with complex values.
        /// </summary>
        /// <param name="a">Sparse matrix handle.</param>
        /// <param name="indexing">Zero-based (C-style) or one-based (Fortran-style) indexing.</param>
        /// <param name="rows">Pointer to the number of rows of the matrix.</param>
        /// <param name="cols">Pointer to the number of columns of the matrix.</param>
        /// <param name="row_start">Pointer to the array of row start indices.</param>
        /// <param name="row_end">Pointer to the array of row end indices.</param>
        /// <param name="col_indx">Pointer to the array of column indices.</param>
        /// <param name="values">Pointer to the array of non-zero values (complex type).</param>
        /// <returns>Status of the export operation.</returns>
        SPARSE_Status ExportCSR(IntPtr a,
            ref SPARSE_IndexBase indexing, long* rows, long* cols,
            long** row_start, long** row_end,
            long** col_indx, void** values);


        // wrapper
        /// <summary>
        /// exports a sparse matrix in CSR format
        /// </summary>
        /// <param name="a"> sparse matrix handle </param>
        /// <param name="indexing"> zero-based (C-style) or one-based (Fortran-style) </param>
        /// <param name="rows"> number of rows of matrix a </param>
        /// <param name="cols"> number of cols of matrix a </param>
        /// <param name="row_start"> row start indices of the non-zero elements </param>
        /// <param name="row_end"> row end indices of the non-zero elements </param>
        /// <param name="col_indx"> column indices of the non-zero elements </param>
        /// <param name="values"> values of the non-zero elements </param>
        /// <returns> result status </returns>
        SPARSE_Status ExportCSRD(IntPtr a,
            ref SPARSE_IndexBase indexing, ref long rows, ref long cols,
            ref IntPtr row_start, ref IntPtr row_end,
            ref IntPtr col_indx, ref IntPtr values);

        /// <summary>
        /// exports a sparse matrix in CSR format
        /// </summary>
        /// <param name="a"> sparse matrix handle </param>
        /// <param name="indexing"> zero-based (C-style) or one-based (Fortran-style) </param>
        /// <param name="rows"> number of rows of matrix a </param>
        /// <param name="cols"> number of cols of matrix a </param>
        /// <param name="row_start"> row start indices of the non-zero elements </param>
        /// <param name="row_end"> row end indices of the non-zero elements </param>
        /// <param name="col_indx"> column indices of the non-zero elements </param>
        /// <param name="values"> values of the non-zero elements </param>
        /// <returns> result status </returns>
        SPARSE_Status ExportCSRZ(IntPtr a,
            ref SPARSE_IndexBase indexing, ref long rows, ref long cols,
            ref IntPtr row_start, ref IntPtr row_end,
            ref IntPtr col_indx, ref IntPtr values);

        #endregion
        #region ---- SetValue [D/Z] ----

        /// <summary>
        /// changes a single value of matrix in internal representation
        /// </summary>
        /// <param name="a"> sparse matrix handle </param>
        /// <param name="row"> indicates row of matrix in which to set value </param>
        /// <param name="col"> indicates column of matrix in which to set value </param>
        /// <param name="value"> target value </param>
        /// <returns> result status </returns>
        SPARSE_Status SetValueD(IntPtr a, long row, long col, double value);

        /// <summary>
        /// changes a single value of matrix in internal representation
        /// </summary>
        /// <param name="a"> sparse matrix handle </param>
        /// <param name="row"> indicates row of matrix in which to set value </param>
        /// <param name="col"> indicates column of matrix in which to set value </param>
        /// <param name="value"> target value </param>
        /// <returns> result status </returns>
        SPARSE_Status SetValueZ(IntPtr a, long row, long col, Complex value);

        #endregion
        #region ---- UpdateValues [D/Z] ----

        /// <summary>
        /// Changes all or selected matrix values in internal representation for a real-valued sparse matrix.
        /// </summary>
        /// <param name="a">Sparse matrix handle.</param>
        /// <param name="nvalues">Total number of elements to change.</param>
        /// <param name="indx">Pointer to the row indices for the new values.</param>
        /// <param name="indy">Pointer to the column indices for the new values.</param>
        /// <param name="values">Pointer to the new values.</param>
        /// <returns>Result status of the update operation.</returns>
        SPARSE_Status UpdateValuesD(IntPtr a, long nvalues,
            long* indx, long* indy, double* values);

        /// <summary>
        /// Changes all or selected matrix values in internal representation for a complex-valued sparse matrix.
        /// </summary>
        /// <param name="a">Sparse matrix handle.</param>
        /// <param name="nvalues">Total number of elements to change.</param>
        /// <param name="indx">Pointer to the row indices for the new values.</param>
        /// <param name="indy">Pointer to the column indices for the new values.</param>
        /// <param name="values">Pointer to the new complex values.</param>
        /// <returns>Result status of the update operation.</returns>
        SPARSE_Status UpdateValuesZ(IntPtr a, long nvalues,
            long* indx, long* indy, void* values);


        // wrapper
        /// <summary>
        /// changes all or selected matrix values in internal representation
        /// </summary>
        /// <typeparam name="T1"> ArrayBase[long] </typeparam>
        /// <typeparam name="T2"> ArrayBase[double] </typeparam>
        /// <param name="a"> sparse matrix handle </param>
        /// <param name="nvalues"> total number of elements changed </param>
        /// <param name="indx"> row indices for the new values </param>
        /// <param name="indy"> column indices for the new values </param>
        /// <param name="values"> new values </param>
        /// <returns> result status </returns>
        SPARSE_Status UpdateValuesD<T1, T2>(IntPtr a, long nvalues,
            T1 indx, T1 indy, T2 values)
            where T1 : DenseArrayBase<long>
            where T2 : DenseArrayBase<double>;

        /// <summary>
        /// changes all or selected matrix values in internal representation
        /// </summary>
        /// <typeparam name="T1"> ArrayBase[long] </typeparam>
        /// <typeparam name="T2"> ArrayBase[Complex] </typeparam>
        /// <param name="a"> sparse matrix handle </param>
        /// <param name="nvalues"> total number of elements changed </param>
        /// <param name="indx"> row indices for the new values </param>
        /// <param name="indy"> column indices for the new values </param>
        /// <param name="values"> new values </param>
        /// <returns> result status </returns>
        SPARSE_Status UpdateValuesZ<T1, T2>(IntPtr a, long nvalues,
            T1 indx, T1 indy, T2 values)
            where T1 : DenseArrayBase<long>
            where T2 : DenseArrayBase<Complex>;

        #endregion
        #region ---- Order ----

        /// <summary>
        /// performs ordering of column indexes of the matrix in CSR format
        /// </summary>
        /// <param name="a"> sparse matrix handle </param>
        /// <returns> result status </returns>
        SPARSE_Status Order(IntPtr a);

        #endregion
        // analysis routines
        // ...
        // execution routines
        #region ---- Optimize ----

        /// <summary>
        /// optimize matrix described by the handle
        /// </summary>
        /// <param name="a"> sparse matrix handle </param>
        /// <returns> result status </returns>
        SPARSE_Status Optimize(IntPtr a);

        #endregion
        #region ---- Mv [D/Z] ----

        /// <summary>
        /// Computes the sparse matrix-vector product for a real-valued sparse matrix.
        /// </summary>
        /// <param name="operation">Specifies the operation op() to apply to the sparse matrix <paramref name="a"/>.</param>
        /// <param name="alpha">Scalar multiplier for the matrix-vector product.</param>
        /// <param name="a">Handle to the sparse matrix.</param>
        /// <param name="descr">Descriptor specifying the properties of the sparse matrix.</param>
        /// <param name="x">Pointer to the input dense vector.</param>
        /// <param name="beta">Scalar multiplier for the output vector <paramref name="y"/>.</param>
        /// <param name="y">Pointer to the output dense vector, which is updated in place.</param>
        /// <returns>Status code indicating the result of the operation.</returns>
        SPARSE_Status Mv(SPARSE_Operation operation,
            double alpha, [In] IntPtr a, SPARSE_MatrixDescr descr,
            [In] double* x, double beta, [In, Out] double* y);

        /// <summary>
        /// Computes the sparse matrix-vector product for a complex-valued sparse matrix.
        /// </summary>
        /// <param name="operation">Specifies the operation op() to apply to the sparse matrix <paramref name="a"/>.</param>
        /// <param name="alpha">Pointer to the scalar multiplier for the matrix-vector product (complex value).</param>
        /// <param name="a">Handle to the sparse matrix.</param>
        /// <param name="descr">Descriptor specifying the properties of the sparse matrix.</param>
        /// <param name="x">Pointer to the input dense vector (complex values).</param>
        /// <param name="beta">Pointer to the scalar multiplier for the output vector <paramref name="y"/> (complex value).</param>
        /// <param name="y">Pointer to the output dense vector (complex values), which is updated in place.</param>
        /// <returns>Status code indicating the result of the operation.</returns>
        SPARSE_Status Mv(SPARSE_Operation operation,
            void* alpha, [In] IntPtr a, SPARSE_MatrixDescr descr,
            [In] void* x, void* beta, [In, Out] void* y);


        // wrapper
        /// <summary>
        /// computes a sparse matrix-vector product
        /// y = alpha * op(A) * x + beta * y
        /// </summary>
        /// <typeparam name="T1"> SPMatrix[double] </typeparam>
        /// <typeparam name="T2"> Vector[double] </typeparam>
        /// <param name="operation"> ppecifies operation op() on sparse matrix a </param>
        /// <param name="alpha"> scalar constant alpha </param>
        /// <param name="a"> sparse matrix a </param>
        /// <param name="descr"> structure specifying sparse matrix properties </param>
        /// <param name="x"> dense vector x </param>
        /// <param name="beta"> scalar constant beta </param>
        /// <param name="y"> dense vector y </param>
        /// <returns> result status </returns>
        SPARSE_Status MvD<T1, T2>(SPARSE_Operation operation,
            double alpha, T1 a, SPARSE_MatrixDescr descr,
            T2 x, double beta, ref T2 y)
            where T1 : SPMatrix<double>
            where T2 : Vector<double>;

        /// <summary>
        /// computes a sparse matrix-vector product
        /// y = alpha * op(A) * x + beta * y
        /// </summary>
        /// <typeparam name="T1"> SPMatrix[double] </typeparam>
        /// <typeparam name="T2"> Vector[double] </typeparam>
        /// <param name="operation"> ppecifies operation op() on sparse matrix a </param>
        /// <param name="alpha"> scalar constant alpha </param>
        /// <param name="a"> sparse matrix a </param>
        /// <param name="descr"> structure specifying sparse matrix properties </param>
        /// <param name="x"> dense vector x </param>
        /// <param name="beta"> scalar constant beta </param>
        /// <param name="y"> dense vector y </param>
        /// <returns> result status </returns>
        SPARSE_Status MvZ<T1, T2>(SPARSE_Operation operation,
            Complex alpha, T1 a, SPARSE_MatrixDescr descr,
            T2 x, Complex beta, ref T2 y)
            where T1 : SPMatrix<Complex>
            where T2 : Vector<Complex>;

        #endregion
        #region ---- DotMv [D/Z] ----

        /// <summary>
        /// Computes y = alpha * op(A) * x + beta * y and d = (x, y) - the l2 inner product
        /// for a sparse matrix and dense vectors (double precision).
        /// </summary>
        /// <param name="operation">Specifies the operation op() on the sparse matrix a.</param>
        /// <param name="alpha">Scalar constant alpha.</param>
        /// <param name="a">Handle to the sparse matrix.</param>
        /// <param name="descr">Structure specifying sparse matrix properties.</param>
        /// <param name="x">Pointer to the dense input vector x.</param>
        /// <param name="beta">Scalar constant beta.</param>
        /// <param name="y">Pointer to the dense output vector y (in/out).</param>
        /// <param name="d">Pointer to the result inner product (output).</param>
        /// <returns>Status of the operation.</returns>
        SPARSE_Status DotMv(SPARSE_Operation operation,
            double alpha, [In] IntPtr a, SPARSE_MatrixDescr descr,
            [In] double* x, double beta,
            [In, Out] double* y, double* d);

        /// <summary>
        /// Computes y = alpha * op(A) * x + beta * y and d = (x, y) - the l2 inner product
        /// for a sparse matrix and dense vectors (complex precision).
        /// </summary>
        /// <param name="operation">Specifies the operation op() on the sparse matrix a.</param>
        /// <param name="alpha">Pointer to the scalar constant alpha (complex).</param>
        /// <param name="a">Handle to the sparse matrix.</param>
        /// <param name="descr">Structure specifying sparse matrix properties.</param>
        /// <param name="x">Pointer to the dense input vector x (complex).</param>
        /// <param name="beta">Pointer to the scalar constant beta (complex).</param>
        /// <param name="y">Pointer to the dense output vector y (complex, in/out).</param>
        /// <param name="d">Pointer to the result inner product (complex, output).</param>
        /// <returns>Status of the operation.</returns>
        SPARSE_Status DotMv(SPARSE_Operation operation,
            void* alpha, [In] IntPtr a, SPARSE_MatrixDescr descr,
            [In] void* x, void* beta,
            [In, Out] void* y, void* d);


        // wrapper
        /// <summary>
        /// computes y = alpha * A * x + beta * y
        /// and d = (x, y) - the l2 inner product
        /// </summary>
        /// <typeparam name="T1"> SPMatrix[double] </typeparam>
        /// <typeparam name="T2"> Vector[double] </typeparam>
        /// <param name="operation"> specifies operation op() on sparse matrix a </param>
        /// <param name="alpha"> scalar constant alpha </param>
        /// <param name="a"> sparse matrix a </param>
        /// <param name="descr"> structure specifying sparse matrix properties </param>
        /// <param name="x"> dense vector x </param>
        /// <param name="beta"> scalar constant beta </param>
        /// <param name="y"> dense vector y </param>
        /// <param name="d"> inner product </param>
        /// <returns> result status </returns>
        SPARSE_Status DotMvD<T1, T2>(SPARSE_Operation operation,
            double alpha, T1 a, SPARSE_MatrixDescr descr,
            T2 x, double beta, ref T2 y, ref double d)
            where T1 : SPMatrix<double>
            where T2 : Vector<double>;

        /// <summary>
        /// computes y = alpha * A * x + beta * y
        /// and d = (x, y) - the l2 inner product
        /// </summary>
        /// <typeparam name="T1"> SPMatrix[Complex] </typeparam>
        /// <typeparam name="T2"> Vector[Complex] </typeparam>
        /// <param name="operation"> specifies operation op() on sparse matrix a </param>
        /// <param name="alpha"> scalar constant alpha </param>
        /// <param name="a"> sparse matrix a </param>
        /// <param name="descr"> structure specifying sparse matrix properties </param>
        /// <param name="x"> dense vector x </param>
        /// <param name="beta"> scalar constant beta </param>
        /// <param name="y"> dense vector y </param>
        /// <param name="d"> inner product </param>
        /// <returns> result status </returns>
        SPARSE_Status DotMvZ<T1, T2>(SPARSE_Operation operation,
            Complex alpha, T1 a, SPARSE_MatrixDescr descr,
            T2 x, Complex beta, T2 y, ref Complex d)
            where T1 : SPMatrix<Complex>
            where T2 : Vector<Complex>;

        #endregion
        #region ---- Trsv [D/Z] ----

        /// <summary>
        /// Solves a triangular sparse linear system y = alpha * A^{-1} * x for real-valued matrices.
        /// </summary>
        /// <param name="operation">Specifies the operation op() on the sparse matrix a.</param>
        /// <param name="alpha">Scalar constant alpha.</param>
        /// <param name="a">Handle to the sparse matrix.</param>
        /// <param name="descr">Structure specifying sparse matrix properties.</param>
        /// <param name="x">Pointer to the input dense vector x.</param>
        /// <param name="y">Pointer to the output dense vector y.</param>
        /// <returns>Status of the sparse operation.</returns>
        SPARSE_Status Trsv(SPARSE_Operation operation,
            double alpha, [In] IntPtr a, SPARSE_MatrixDescr descr,
            [In] double* x, [Out] double* y);

        /// <summary>
        /// Solves a triangular sparse linear system y = alpha * A^{-1} * x for complex-valued matrices.
        /// </summary>
        /// <param name="operation">Specifies the operation op() on the sparse matrix a.</param>
        /// <param name="alpha">Pointer to the scalar constant alpha (complex).</param>
        /// <param name="a">Handle to the sparse matrix.</param>
        /// <param name="descr">Structure specifying sparse matrix properties.</param>
        /// <param name="x">Pointer to the input dense vector x (complex).</param>
        /// <param name="y">Pointer to the output dense vector y (complex).</param>
        /// <returns>Status of the sparse operation.</returns>
        SPARSE_Status Trsv(SPARSE_Operation operation,
            void* alpha, [In] IntPtr a, SPARSE_MatrixDescr descr,
            [In] void* x, [Out] void* y);


        // wrapper
        /// <summary>
        /// solves triangular system
        /// y = alpha * A^{-1} * x 
        /// </summary>
        /// <typeparam name="T1"> SPMatrix[double] </typeparam>
        /// <typeparam name="T2"> Vector[double] </typeparam>
        /// <param name="operation"> specifies operation op() on sparse matrix a </param>
        /// <param name="alpha"> scalar constant alpha </param>
        /// <param name="a"> sparse matrix a </param>
        /// <param name="descr"> structure specifying sparse matrix properties </param>
        /// <param name="x"> dense vector x </param>
        /// <param name="y"> dense vector y </param>
        /// <returns> result status </returns>
        SPARSE_Status TrsvD<T1, T2>(SPARSE_Operation operation,
            double alpha, T1 a, SPARSE_MatrixDescr descr,
            T2 x, ref T2 y)
            where T1 : SPMatrix<double>
            where T2 : Vector<double>;

        /// <summary>
        /// solves triangular system
        /// y = alpha * A^{-1} * x 
        /// </summary>
        /// <typeparam name="T1"> SPMatrix[Complex] </typeparam>
        /// <typeparam name="T2"> Vector[Complex] </typeparam>
        /// <param name="operation"> specifies operation op() on sparse matrix a </param>
        /// <param name="alpha"> scalar constant alpha </param>
        /// <param name="a"> sparse matrix a </param>
        /// <param name="descr"> structure specifying sparse matrix properties </param>
        /// <param name="x"> dense vector x </param>
        /// <param name="y"> dense vector y </param>
        /// <returns> result status </returns>
        SPARSE_Status TrsvZ<T1, T2>(SPARSE_Operation operation,
            Complex alpha, T1 a, SPARSE_MatrixDescr descr,
            T2 x, ref T2 y)
            where T1 : SPMatrix<Complex>
            where T2 : Vector<Complex>;

        #endregion
        #region ---- Symgs [D/Z] ----

        /// <summary>
        /// Applies the symmetric Gauss-Seidel preconditioner to a symmetric system A * x = b
        /// for a real-valued sparse matrix.
        /// </summary>
        /// <param name="operation">Specifies the operation op() on the sparse matrix a.</param>
        /// <param name="a">Handle to the sparse matrix.</param>
        /// <param name="descr">Structure specifying sparse matrix properties.</param>
        /// <param name="alpha">Scalar constant alpha.</param>
        /// <param name="b">Pointer to the dense vector b.</param>
        /// <param name="x">Pointer to the dense vector x (output).</param>
        /// <returns>Result status of the operation.</returns>
        SPARSE_Status Symgs(SPARSE_Operation operation,
            [In] IntPtr a, SPARSE_MatrixDescr descr,
            double alpha, [In] double* b, [Out] double* x);

        /// <summary>
        /// Applies the symmetric Gauss-Seidel preconditioner to a symmetric system A * x = b
        /// for a complex-valued sparse matrix.
        /// </summary>
        /// <param name="operation">Specifies the operation op() on the sparse matrix a.</param>
        /// <param name="a">Handle to the sparse matrix.</param>
        /// <param name="descr">Structure specifying sparse matrix properties.</param>
        /// <param name="alpha">Pointer to the scalar constant alpha (complex).</param>
        /// <param name="b">Pointer to the dense vector b (complex).</param>
        /// <param name="x">Pointer to the dense vector x (complex, output).</param>
        /// <returns>Result status of the operation.</returns>
        SPARSE_Status Symgs(SPARSE_Operation operation,
            [In] IntPtr a, SPARSE_MatrixDescr descr,
            void* alpha, [In] void* b, [Out] void* x);


        // wrapper
        /// <summary>
        /// applies symmetric Gauss-Seidel preconditioner to
        /// symmetric system A * x = b
        /// </summary>
        /// <typeparam name="T1"> SPMatrix[double] </typeparam>
        /// <typeparam name="T2"> Vector[double] </typeparam>
        /// <param name="operation"> specifies operation op() on sparse matrix a </param>
        /// <param name="a"> sparse matrix a </param>
        /// <param name="descr"> structure specifying sparse matrix properties </param>
        /// <param name="alpha"> scalar constant alpha </param>
        /// <param name="b"> dense vector b </param>
        /// <param name="x"> dense vector x </param>
        /// <returns> result status </returns>
        SPARSE_Status SymgsD<T1, T2>(SPARSE_Operation operation,
            T1 a, SPARSE_MatrixDescr descr, double alpha, T2 b, ref T2 x)
            where T1 : SPMatrix<double>
            where T2 : Vector<double>;

        /// <summary>
        /// applies symmetric Gauss-Seidel preconditioner to
        /// symmetric system A * x = b
        /// </summary>
        /// <typeparam name="T1"> SPMatrix[Complex] </typeparam>
        /// <typeparam name="T2"> Vector[Complex] </typeparam>
        /// <param name="operation"> specifies operation op() on sparse matrix a </param>
        /// <param name="a"> sparse matrix a </param>
        /// <param name="descr"> structure specifying sparse matrix properties </param>
        /// <param name="alpha"> scalar constant alpha </param>
        /// <param name="b"> dense vector b </param>
        /// <param name="x"> dense vector x </param>
        /// <returns> result status </returns>
        SPARSE_Status SymgsZ<T1, T2>(SPARSE_Operation operation,
            T1 a, SPARSE_MatrixDescr descr, Complex alpha, T2 b, ref T2 x)
            where T1 : SPMatrix<Complex>
            where T2 : Vector<Complex>;

        #endregion
        #region ---- SymgsMv [D/Z] ----

        /// <summary>
        /// Applies the symmetric Gauss-Seidel preconditioner to a symmetric system A * x = b,
        /// followed by a matrix-vector product, and returns y = A * x for double-precision values.
        /// </summary>
        /// <param name="operation">Specifies the operation op() on the sparse matrix a.</param>
        /// <param name="a">Handle to the sparse matrix.</param>
        /// <param name="descr">Structure specifying sparse matrix properties.</param>
        /// <param name="alpha">Scalar constant alpha.</param>
        /// <param name="b">Pointer to the dense vector b.</param>
        /// <param name="x">Pointer to the dense vector x (input/output).</param>
        /// <param name="y">Pointer to the dense vector y (output).</param>
        /// <returns>Status of the operation as <see cref="SPARSE_Status"/>.</returns>
        SPARSE_Status SymgsMv(SPARSE_Operation operation,
            [In] IntPtr a, SPARSE_MatrixDescr descr,
            double alpha, [In] double* b, [In, Out] double* x,
            [Out] double* y);

        /// <summary>
        /// Applies the symmetric Gauss-Seidel preconditioner to a symmetric system A * x = b,
        /// followed by a matrix-vector product, and returns y = A * x for complex values.
        /// </summary>
        /// <param name="operation">Specifies the operation op() on the sparse matrix a.</param>
        /// <param name="a">Handle to the sparse matrix.</param>
        /// <param name="descr">Structure specifying sparse matrix properties.</param>
        /// <param name="alpha">Pointer to the scalar constant alpha (complex).</param>
        /// <param name="b">Pointer to the dense vector b (complex).</param>
        /// <param name="x">Pointer to the dense vector x (complex, input/output).</param>
        /// <param name="y">Pointer to the dense vector y (complex, output).</param>
        /// <returns>Status of the operation as <see cref="SPARSE_Status"/>.</returns>
        SPARSE_Status SymgsMv(SPARSE_Operation operation,
            [In] IntPtr a, SPARSE_MatrixDescr descr,
            void* alpha, [In] void* b, [In, Out] void* x,
            [Out] void* y);


        // wrapper
        /// <summary>
        /// applies symmetric Gauss-Seidel preconditioner to
        /// symmetric system A * x = b, followed by a matr-vector product
        /// and returns y = A * x
        /// </summary>
        /// <typeparam name="T1"> SPMatrix[double] </typeparam>
        /// <typeparam name="T2"> Vector[double] </typeparam>
        /// <param name="operation"> specifies operation op() on sparse matrix a </param>
        /// <param name="a"> sparse matrix a </param>
        /// <param name="descr"> structure specifying sparse matrix properties </param>
        /// <param name="alpha"> scalar constant alpha </param>
        /// <param name="b"> dense vector b </param>
        /// <param name="x"> dense vector x </param>
        /// <param name="y"> dense vector y </param>
        /// <returns> result status </returns>
        SPARSE_Status SymgsMvD<T1, T2>(SPARSE_Operation operation,
            T1 a, SPARSE_MatrixDescr descr, double alpha, T2 b,
            ref T2 x, ref T2 y)
            where T1 : SPMatrix<double>
            where T2 : Vector<double>;

        /// <summary>
        /// applies symmetric Gauss-Seidel preconditioner to
        /// symmetric system A * x = b, followed by a matr-vector product
        /// and returns y = A * x
        /// </summary>
        /// <typeparam name="T1"> SPMatrix[Complex] </typeparam>
        /// <typeparam name="T2"> Vector[Complex] </typeparam>
        /// <param name="operation"> specifies operation op() on sparse matrix a </param>
        /// <param name="a"> sparse matrix a </param>
        /// <param name="descr"> structure specifying sparse matrix properties </param>
        /// <param name="alpha"> scalar constant alpha </param>
        /// <param name="b"> dense vector b </param>
        /// <param name="x"> dense vector x </param>
        /// <param name="y"> dense vector y </param>
        /// <returns> result status </returns>
        SPARSE_Status SymgsMvZ<T1, T2>(SPARSE_Operation operation,
            T1 a, SPARSE_MatrixDescr descr, Complex alpha, T2 b,
            ref T2 x, ref T2 y)
            where T1 : SPMatrix<Complex>
            where T2 : Vector<Complex>;

        #endregion
        #region ---- LU smoother ---- 

        // ...

        #endregion
        #region ---- Mm [D/Z] ----

        /// <summary>
        /// Computes the product of a sparse matrix and a dense matrix and stores the result as a dense matrix.
        /// y = alpha * op(A) * x + beta * y
        /// </summary>
        /// <param name="operation">Specifies the operation op() on the sparse matrix a.</param>
        /// <param name="alpha">Scalar alpha.</param>
        /// <param name="a">Sparse matrix handle.</param>
        /// <param name="descr">Structure specifying sparse matrix properties.</param>
        /// <param name="layout">Describes the storage scheme for the dense matrix.</param>
        /// <param name="x">Pointer to the dense matrix x.</param>
        /// <param name="columns">Number of columns of matrix y.</param>
        /// <param name="ldx">Leading dimension of matrix x.</param>
        /// <param name="beta">Scalar beta.</param>
        /// <param name="y">Pointer to the dense matrix y.</param>
        /// <param name="ldy">Leading dimension of matrix y.</param>
        /// <returns>Result status.</returns>
        SPARSE_Status Mm(SPARSE_Operation operation,
            double alpha, [In] IntPtr a, SPARSE_MatrixDescr descr,
            SPARSE_Layout layout, [In] double* x, long columns, long ldx,
            double beta, [In, Out] double* y, long ldy);

        /// <summary>
        /// Computes the product of a sparse complex matrix and a dense complex matrix and stores the result as a dense matrix.
        /// y = alpha * op(A) * x + beta * y
        /// </summary>
        /// <param name="operation">Specifies the operation op() on the sparse matrix a.</param>
        /// <param name="alpha">Pointer to the scalar alpha (complex).</param>
        /// <param name="a">Sparse matrix handle.</param>
        /// <param name="descr">Structure specifying sparse matrix properties.</param>
        /// <param name="layout">Describes the storage scheme for the dense matrix.</param>
        /// <param name="x">Pointer to the dense matrix x (complex).</param>
        /// <param name="columns">Number of columns of matrix y.</param>
        /// <param name="ldx">Leading dimension of matrix x.</param>
        /// <param name="beta">Pointer to the scalar beta (complex).</param>
        /// <param name="y">Pointer to the dense matrix y (complex).</param>
        /// <param name="ldy">Leading dimension of matrix y.</param>
        /// <returns>Result status.</returns>
        SPARSE_Status Mm(SPARSE_Operation operation,
            void* alpha, [In] IntPtr a, SPARSE_MatrixDescr descr,
            SPARSE_Layout layout, [In] void* x, long columns, long ldx,
            void* beta, [In, Out] void* y, long ldy);


        // wrapper
        /// <summary>
        /// computes the product of a sparse matrix and a dense
        /// matrix and stores the result as a dense matrix
        /// y = alpha * op(A) * x + beta * y
        /// </summary>
        /// <typeparam name="T1"> SPMatrix[double] </typeparam>
        /// <typeparam name="T2"> Matrix[double] </typeparam>
        /// <param name="operation"> ppecifies operation op() on sparse matrix a </param>
        /// <param name="alpha"> scalar alpha </param>
        /// <param name="a"> sparse matrix a </param>
        /// <param name="descr"> structure specifying sparse matrix properties </param>
        /// <param name="layout"> describes the storage scheme for the dense matrix </param>
        /// <param name="x"> dense matrix x </param>
        /// <param name="columns"> number of columns of matrix y </param>
        /// <param name="ldx"> leading dimension of matrix x </param>
        /// <param name="beta"> scalar beta </param>
        /// <param name="y"> dense matrix y </param>
        /// <param name="ldy"> leading dimension of matrix y </param>
        /// <returns> result status </returns>
        SPARSE_Status MmD<T1, T2>(SPARSE_Operation operation,
            double alpha, T1 a, SPARSE_MatrixDescr descr, SPARSE_Layout layout,
            T2 x, long columns, long ldx,
            double beta, ref T2 y, long ldy)
            where T1 : SPMatrix<double>
            where T2 : Matrix<double>;

        /// <summary>
        /// computes the product of a sparse matrix and a dense
        /// matrix and stores the result as a dense matrix
        /// y = alpha * op(A) * x + beta * y
        /// </summary>
        /// <typeparam name="T1"> SPMatrix[Complex] </typeparam>
        /// <typeparam name="T2"> Matrix[Complex] </typeparam>
        /// <param name="operation"> ppecifies operation op() on sparse matrix a </param>
        /// <param name="alpha"> scalar alpha </param>
        /// <param name="a"> sparse matrix a </param>
        /// <param name="descr"> structure specifying sparse matrix properties </param>
        /// <param name="layout"> describes the storage scheme for the dense matrix </param>
        /// <param name="x"> dense matrix x </param>
        /// <param name="columns"> number of columns of matrix y </param>
        /// <param name="ldx"> leading dimension of matrix x </param>
        /// <param name="beta"> scalar beta </param>
        /// <param name="y"> dense matrix y </param>
        /// <param name="ldy"> leading dimension of matrix y </param>
        /// <returns> result status </returns>
        SPARSE_Status MmZ<T1, T2>(SPARSE_Operation operation,
            Complex alpha, T1 a, SPARSE_MatrixDescr descr, SPARSE_Layout layout,
            T2 x, long columns, long ldx,
            Complex beta, ref T2 y, long ldy)
            where T1 : SPMatrix<Complex>
            where T2 : Matrix<Complex>;

        #endregion
        #region ---- Trsm [D/Z] ----

        /// <summary>
        /// Solves a system of linear equations with multiple right hand sides for a triangular sparse matrix.
        /// Computes <c>y = alpha * op(A)^{-1} * x</c> for a real-valued matrix.
        /// </summary>
        /// <param name="operation">Specifies the operation <c>op()</c> on the sparse matrix <paramref name="a"/>.</param>
        /// <param name="alpha">Scalar multiplier for the solution.</param>
        /// <param name="a">Handle to the sparse matrix.</param>
        /// <param name="descr">Structure specifying sparse matrix properties.</param>
        /// <param name="layout">Describes the storage scheme for the dense matrix.</param>
        /// <param name="x">Pointer to the input dense matrix <c>x</c>.</param>
        /// <param name="columns">Number of columns in matrix <c>y</c>.</param>
        /// <param name="ldx">Leading dimension of matrix <c>x</c>.</param>
        /// <param name="y">Pointer to the output dense matrix <c>y</c>.</param>
        /// <param name="ldy">Leading dimension of matrix <c>y</c>.</param>
        /// <returns>Result status of the operation.</returns>
        SPARSE_Status Trsm(SPARSE_Operation operation,
            double alpha, [In] IntPtr a, SPARSE_MatrixDescr descr,
            SPARSE_Layout layout, [In] double* x, long columns, long ldx,
            [In, Out] double* y, long ldy);

        /// <summary>
        /// Solves a system of linear equations with multiple right hand sides for a triangular sparse matrix.
        /// Computes <c>y = alpha * op(A)^{-1} * x</c> for a complex-valued matrix.
        /// </summary>
        /// <param name="operation">Specifies the operation <c>op()</c> on the sparse matrix <paramref name="a"/>.</param>
        /// <param name="alpha">Pointer to the scalar multiplier for the solution (complex value).</param>
        /// <param name="a">Handle to the sparse matrix.</param>
        /// <param name="descr">Structure specifying sparse matrix properties.</param>
        /// <param name="layout">Describes the storage scheme for the dense matrix.</param>
        /// <param name="x">Pointer to the input dense matrix <c>x</c> (complex values).</param>
        /// <param name="columns">Number of columns in matrix <c>y</c>.</param>
        /// <param name="ldx">Leading dimension of matrix <c>x</c>.</param>
        /// <param name="y">Pointer to the output dense matrix <c>y</c> (complex values).</param>
        /// <param name="ldy">Leading dimension of matrix <c>y</c>.</param>
        /// <returns>Result status of the operation.</returns>
        SPARSE_Status Trsm(SPARSE_Operation operation,
            void* alpha, [In] IntPtr a, SPARSE_MatrixDescr descr,
            SPARSE_Layout layout, [In] void* x, long columns, long ldx,
            [In, Out] void* y, long ldy);


        // wrapper
        /// <summary>
        /// solves a system of linear equations with multiple 
        /// right hand sides for a triangular sparse matrix
        /// y = alpha * op(A)^{-1} * x
        /// </summary>
        /// <typeparam name="T1"> SPMatrix[double] </typeparam>
        /// <typeparam name="T2"> Matrix[double] </typeparam>
        /// <param name="operation"> specifies operation op() on sparse matrix a </param>
        /// <param name="alpha"> scalar alpha </param>
        /// <param name="a"> sparse matrix a </param>
        /// <param name="descr"> structure specifying sparse matrix properties </param>
        /// <param name="layout"> describes the storage scheme for the dense matrix </param>
        /// <param name="x"> dense matrix x </param>
        /// <param name="columns"> number of columns of matrix y </param>
        /// <param name="ldx"> leading dimension of matrix x </param>
        /// <param name="y"> dense matrix y </param>
        /// <param name="ldy"> leading dimension of matrix y </param>
        /// <returns> result status </returns>
        SPARSE_Status TrsmD<T1, T2>(SPARSE_Operation operation,
            double alpha, T1 a, SPARSE_MatrixDescr descr, SPARSE_Layout layout,
            T2 x, long columns, long ldx,
            ref T2 y, long ldy)
            where T1 : SPMatrix<double>
            where T2 : Matrix<double>;

        /// <summary>
        /// solves a system of linear equations with multiple 
        /// right hand sides for a triangular sparse matrix
        /// y = alpha * op(A)^{-1} * x
        /// </summary>
        /// <typeparam name="T1"> SPMatrix[Complex] </typeparam>
        /// <typeparam name="T2"> Matrix[Complex] </typeparam>
        /// <param name="operation"> specifies operation op() on sparse matrix a </param>
        /// <param name="alpha"> scalar alpha </param>
        /// <param name="a"> sparse matrix a </param>
        /// <param name="descr"> structure specifying sparse matrix properties </param>
        /// <param name="layout"> describes the storage scheme for the dense matrix </param>
        /// <param name="x"> dense matrix x </param>
        /// <param name="columns"> number of columns of matrix y </param>
        /// <param name="ldx"> leading dimension of matrix x </param>
        /// <param name="y"> dense matrix y </param>
        /// <param name="ldy"> leading dimension of matrix y </param>
        /// <returns> result status </returns>
        SPARSE_Status TrsmZ<T1, T2>(SPARSE_Operation operation,
            Complex alpha, T1 a, SPARSE_MatrixDescr descr, SPARSE_Layout layout,
            T2 x, long columns, long ldx,
            ref T2 y, long ldy)
            where T1 : SPMatrix<Complex>
            where T2 : Matrix<Complex>;

        #endregion
        #region ---- Add [D/Z] ----

        /// <summary>
        /// Computes the sum of two sparse matrices for double-precision values.
        /// C = alpha * op(A) + B, result is sparse
        /// </summary>
        /// <param name="operation">Specifies the operation op() on sparse matrix a.</param>
        /// <param name="a">Handle to the first sparse matrix.</param>
        /// <param name="alpha">Scalar multiplier for matrix a.</param>
        /// <param name="b">Handle to the second sparse matrix.</param>
        /// <param name="c">Reference to the handle for the resulting sparse matrix. The result is stored in a newly allocated matrix.</param>
        /// <returns>Status of the sparse operation.</returns>
        SPARSE_Status Add(SPARSE_Operation operation,
            [In] IntPtr a, double alpha, [In] IntPtr b,
            ref IntPtr c);

        /// <summary>
        /// Computes the sum of two sparse matrices for complex values.
        /// C = alpha * op(A) + B, result is sparse
        /// </summary>
        /// <param name="operation">Specifies the operation op() on sparse matrix a.</param>
        /// <param name="a">Handle to the first sparse matrix.</param>
        /// <param name="alpha">Pointer to the scalar multiplier for matrix a (complex value).</param>
        /// <param name="b">Handle to the second sparse matrix.</param>
        /// <param name="c">Reference to the handle for the resulting sparse matrix. The result is stored in a newly allocated matrix.</param>
        /// <returns>Status of the sparse operation.</returns>
        SPARSE_Status Add(SPARSE_Operation operation,
            [In] IntPtr a, void* alpha, [In] IntPtr b,
            ref IntPtr c);


        // wrapper
        /// <summary>
        /// computes the sum of two sparse matrices
        /// C = alpha * op(A) + B
        /// the result is stored in a newly allocated sparse matrix
        /// </summary>
        /// <typeparam name="T"> SPMatrix[double] </typeparam>
        /// <param name="operation"> specifies operation op() on sparse matrix a </param>
        /// <param name="a"> sparse matrix a </param>
        /// <param name="alpha"> scalar alpha </param>
        /// <param name="b"> sparse matrix b </param>
        /// <param name="c"> sparse matrix c </param>
        /// <returns> result status </returns>
        SPARSE_Status AddD<T>(SPARSE_Operation operation,
            T a, double alpha, T b, ref T c)
            where T : SPMatrix<double>;

        /// <summary>
        /// computes the sum of two sparse matrices
        /// C = alpha * op(A) + B
        /// the result is stored in a newly allocated sparse matrix
        /// </summary>
        /// <typeparam name="T"> SPMatrix[Complex] </typeparam>
        /// <param name="operation"> specifies operation op() on sparse matrix a </param>
        /// <param name="a"> sparse matrix a </param>
        /// <param name="alpha"> scalar alpha </param>
        /// <param name="b"> sparse matrix b </param>
        /// <param name="c"> sparse matrix c </param>
        /// <returns> result status </returns>
        SPARSE_Status AddZ<T>(SPARSE_Operation operation,
            T a, Complex alpha, T b, ref T c)
            where T : SPMatrix<Complex>;

        #endregion
        #region ---- Spmm ----

        /// <summary>
        /// Computes the product of two sparse matrices.
        /// C = op(A) * B, result is sparse
        /// </summary>
        /// <param name="operation">Specifies the operation op() on sparse matrix a.</param>
        /// <param name="a">Handle to the first sparse matrix.</param>
        /// <param name="b">Handle to the second sparse matrix.</param>
        /// <param name="c">Reference to the handle for the resulting sparse matrix. The result is stored in a newly allocated sparse matrix.</param>
        /// <returns>Status of the sparse matrix-matrix multiplication operation.</returns>
        SPARSE_Status Spmm(SPARSE_Operation operation,
            [In] IntPtr a, [In] IntPtr b, ref IntPtr c);


        // wrapper
        /// <summary>
        /// computes the product of two sparse matrices
        /// C = op(A) * B
        /// the result is stored in a newly allocated sparse matrix
        /// </summary>
        /// <typeparam name="T"> SPMatrix[double] </typeparam>
        /// <param name="operation"> specifies operation op() on sparse matrix a </param>
        /// <param name="a"> sparse matrix a </param>
        /// <param name="b"> sparse matrix b </param>
        /// <param name="c"> sparse matrix c </param>
        /// <returns> result status </returns>
        SPARSE_Status SpmmD<T>(SPARSE_Operation operation,
            T a, T b, ref T c)
            where T : SPMatrix<double>;

        /// <summary>
        /// computes the product of two sparse matrices
        /// C = op(A) * B
        /// the result is stored in a newly allocated sparse matrix
        /// </summary>
        /// <typeparam name="T"> SPMatrix[Complex] </typeparam>
        /// <param name="operation"> specifies operation op() on sparse matrix a </param>
        /// <param name="a"> sparse matrix a </param>
        /// <param name="b"> sparse matrix b </param>
        /// <param name="c"> sparse matrix c </param>
        /// <returns> result status </returns>
        SPARSE_Status SpmmZ<T>(SPARSE_Operation operation,
            T a, T b, ref T c)
            where T : SPMatrix<Complex>;

        #endregion
        #region ---- Sp2mm ----

        /// <summary>
        /// Computes the product of two sparse matrices using the Intel MKL SPBLAS interface.
        /// C = opA(A) * opB(B)
        /// </summary>
        /// <param name="transA">Specifies the operation op() to be applied to sparse matrix <paramref name="a"/>.</param>
        /// <param name="descrA">Structure specifying properties of sparse matrix <paramref name="a"/>.</param>
        /// <param name="a">Handle to the first sparse matrix.</param>
        /// <param name="transB">Specifies the operation op() to be applied to sparse matrix <paramref name="b"/>.</param>
        /// <param name="descrB">Structure specifying properties of sparse matrix <paramref name="b"/>.</param>
        /// <param name="b">Handle to the second sparse matrix.</param>
        /// <param name="request">Specifies whether the full computation is performed at once or using a two-stage algorithm.</param>
        /// <param name="c">Reference to the handle of the resulting sparse matrix. The result is stored in a newly allocated sparse matrix.</param>
        /// <returns>Status of the sparse operation.</returns>
        SPARSE_Status Sp2mm(SPARSE_Operation transA,
            SPARSE_MatrixDescr descrA, [In] IntPtr a,
            SPARSE_Operation transB, SPARSE_MatrixDescr descrB,
            [In] IntPtr b, SPARSE_Request request, ref IntPtr c);


        // wrapper
        /// <summary>
        /// computes the product of two sparse matrices
        /// C = opA(A) * opB(B)
        /// the result is stored in a newly allocated sparse matrix
        /// </summary>
        /// <typeparam name="T"> SPMatrix[double] </typeparam>
        /// <param name="transA"> specifies operation op() on sparse matrix a </param>
        /// <param name="descrA"> structure specifying sparse matrix a's properties</param>
        /// <param name="a"> sparse matrix a </param>
        /// <param name="transB"> specifies operation op() on sparse matrix b </param>
        /// <param name="descrB"> structure specifying sparse matrix b's properties </param>
        /// <param name="b"> sparse matrix b </param>
        /// <param name="request"> specifies whether the full computations are performed at once 
        /// or using the two-stage algorithm </param>
        /// <param name="c"> sparse matrix c </param>
        /// <returns> result status </returns>
        SPARSE_Status Sp2mD<T>(
            SPARSE_Operation transA, SPARSE_MatrixDescr descrA, T a,
            SPARSE_Operation transB, SPARSE_MatrixDescr descrB, T b,
            SPARSE_Request request, ref T c)
            where T : SPMatrix<double>;

        /// <summary>
        /// computes the product of two sparse matrices
        /// C = opA(A) * opB(B)
        /// the result is stored in a newly allocated sparse matrix
        /// </summary>
        /// <typeparam name="T"> SPMatrix[Complex] </typeparam>
        /// <param name="transA"> specifies operation op() on sparse matrix a </param>
        /// <param name="descrA"> structure specifying sparse matrix a's properties</param>
        /// <param name="a"> sparse matrix a </param>
        /// <param name="transB"> specifies operation op() on sparse matrix b </param>
        /// <param name="descrB"> structure specifying sparse matrix b's properties </param>
        /// <param name="b"> sparse matrix b </param>
        /// <param name="request"> specifies whether the full computations are performed at once 
        /// or using the two-stage algorithm </param>
        /// <param name="c"> sparse matrix c </param>
        /// <returns> result status </returns>
        SPARSE_Status Sp2mZ<T>(
            SPARSE_Operation transA, SPARSE_MatrixDescr descrA, T a,
            SPARSE_Operation transB, SPARSE_MatrixDescr descrB, T b,
            SPARSE_Request request, ref T c)
            where T : SPMatrix<Complex>;

        #endregion
        #region ---- Syrk ----

        /// <summary>
        /// Computes the product of a sparse matrix with its transpose or conjugate transpose.
        /// C = op(A) * op(A)^T (for real) or C = op(A) * op(A)^H (for complex).
        /// The result is stored in a newly allocated sparse matrix.
        /// </summary>
        /// <param name="operation">Specifies the operation op() to apply to the sparse matrix <paramref name="a"/>.</param>
        /// <param name="a">Handle to the input sparse matrix.</param>
        /// <param name="c">Reference to the handle of the output sparse matrix. The result will be stored here.</param>
        /// <returns>Status of the sparse operation.</returns>
        SPARSE_Status Syrk(SPARSE_Operation operation,
            [In] IntPtr a, ref IntPtr c);

        
        // wrapper
        /// <summary>
        /// computes the product of sparse matrix with its transpose 
        /// C = op(A) * op(A)^T 
        /// and stores the result in a newly allocated sparse matrix
        /// </summary>
        /// <typeparam name="T"> SPMatrix[double] </typeparam>
        /// <param name="operation"> specifies operation op() on sparse matrix a </param>
        /// <param name="a"> sparse matrix a </param>
        /// <param name="c"> sparse matrix c </param>
        /// <returns> result status </returns>
        SPARSE_Status SyrkD<T>(SPARSE_Operation operation,
            T a, ref T c)
            where T : SPMatrix<double>;

        /// <summary>
        /// computes the product of sparse matrix with its conjugate transpose 
        /// C = op(A) * op(A)^H
        /// and stores the result in a newly allocated sparse matrix
        /// </summary>
        /// <typeparam name="T"> SPMatrix[Complex] </typeparam>
        /// <param name="operation"> specifies operation op() on sparse matrix a </param>
        /// <param name="a"> sparse matrix a </param>
        /// <param name="c"> sparse matrix c </param>
        /// <returns> result status </returns>
        SPARSE_Status SyrkZ<T>(SPARSE_Operation operation,
            T a, ref T c)
            where T : SPMatrix<Complex>;

        #endregion
        #region ---- Sypr ----

        /// <summary>
        /// Computes the symmetric product of three sparse matrices:
        /// C = op(A) * B * op(A)<sup>H</sup>,
        /// and stores the result in a newly allocated sparse matrix.
        /// </summary>
        /// <param name="transA">Specifies the operation op() on sparse matrix A.</param>
        /// <param name="a">Handle to the first sparse matrix (A).</param>
        /// <param name="b">Handle to the second sparse matrix (B).</param>
        /// <param name="descrB">Structure specifying properties of matrix B.</param>
        /// <param name="c">Reference to the handle for the resulting sparse matrix (C).</param>
        /// <param name="request">Specifies whether the full computation is performed at once or using a two-stage algorithm.</param>
        /// <returns>Status of the operation as <see cref="SPARSE_Status"/>.</returns>
        SPARSE_Status Sypr(SPARSE_Operation transA,
            [In] IntPtr a, [In] IntPtr b, SPARSE_MatrixDescr descrB,
            ref IntPtr c, SPARSE_Request request);


        // wrapper
        /// <summary>
        /// computes the symmetric product of three sparse
        /// C = op(A) * B * op(A)^T
        /// and stores the result in a newly allocated sparse matrix
        /// </summary>
        /// <typeparam name="T"> SPMatrix[double] </typeparam>
        /// <param name="transA"> specifies operation op() on sparse matrix a </param>
        /// <param name="a"> sparse matrix a </param>
        /// <param name="b"> sparse matrix b </param>
        /// <param name="descrB"> structure specifying sparse matrix b's properties </param>
        /// <param name="c"> sparse matrix c </param>
        /// <param name="request"> specifies whether the full computations are performed at once 
        /// or using the two-stage algorithm </param>
        /// <returns> result status </returns>
        SPARSE_Status SyprD<T>(
            SPARSE_Operation transA, T a, T b, SPARSE_MatrixDescr descrB,
            ref T c, SPARSE_Request request)
            where T : SPMatrix<double>;

        /// <summary>
        /// computes the symmetric product of three sparse
        /// C = op(A) * B * op(A)^H
        /// and stores the result in a newly allocated sparse matrix
        /// </summary>
        /// <typeparam name="T"> SPMatrix[Complex] </typeparam>
        /// <param name="transA"> specifies operation op() on sparse matrix a </param>
        /// <param name="a"> sparse matrix a </param>
        /// <param name="b"> sparse matrix b </param>
        /// <param name="descrB"> structure specifying sparse matrix b's properties </param>
        /// <param name="c"> sparse matrix c </param>
        /// <param name="request"> specifies whether the full computations are performed at once 
        /// or using the two-stage algorithm </param>
        /// <returns> result status </returns>
        SPARSE_Status Sypr<T>(
            SPARSE_Operation transA, T a, T b, SPARSE_MatrixDescr descrB,
            ref T c, SPARSE_Request request)
            where T : SPMatrix<Complex>;

        #endregion
        #region ---- Spmmd [D/Z] ----

        /// <summary>
        /// Computes the product of two sparse matrices and stores the result as a dense matrix (double precision).
        /// C = op(A) * B, result is dense
        /// </summary>
        /// <param name="operation">Specifies the operation op() on sparse matrix a.</param>
        /// <param name="a">Handle to the first sparse matrix.</param>
        /// <param name="b">Handle to the second sparse matrix.</param>
        /// <param name="layout">Describes the storage scheme for the dense matrix.</param>
        /// <param name="c">Pointer to the output dense matrix.</param>
        /// <param name="ldc">Leading dimension of matrix c.</param>
        /// <returns>Result status of the operation.</returns>
        SPARSE_Status Spmmd(SPARSE_Operation operation,
            [In] IntPtr a, [In] IntPtr b, SPARSE_Layout layout,
            [Out] double* c, long ldc);

        /// <summary>
        /// Computes the product of two sparse matrices and stores the result as a dense matrix (complex precision).
        /// C = op(A) * B, result is dense
        /// </summary>
        /// <param name="operation">Specifies the operation op() on sparse matrix a.</param>
        /// <param name="a">Handle to the first sparse matrix.</param>
        /// <param name="b">Handle to the second sparse matrix.</param>
        /// <param name="layout">Describes the storage scheme for the dense matrix.</param>
        /// <param name="c">Pointer to the output dense matrix.</param>
        /// <param name="ldc">Leading dimension of matrix c.</param>
        /// <returns>Result status of the operation.</returns>
        SPARSE_Status Spmmd(SPARSE_Operation operation,
            [In] IntPtr a, [In] IntPtr b, SPARSE_Layout layout,
            [Out] void* c, long ldc);


        // wrapper
        /// <summary>
        /// computes the product of two sparse matrices
        /// C = op(A) * B
        /// and stores the result as a dense matrix
        /// </summary>
        /// <typeparam name="T1"> SPMatrix[double] </typeparam>
        /// <typeparam name="T2"> Matrix[double] </typeparam>
        /// <param name="operation"> specifies operation op() on sparse matrix a </param>
        /// <param name="a"> sparse matrix a </param>
        /// <param name="b"> sparse matrix b </param>
        /// <param name="layout"> describes the storage scheme for the dense matrix </param>
        /// <param name="c"> dense matrix c </param>
        /// <param name="ldc"> leading dimension of matrix c </param>
        /// <returns> result status </returns>
        SPARSE_Status SpmmdD<T1, T2>(SPARSE_Operation operation,
            T1 a, T1 b, SPARSE_Layout layout,
            T2 c, long ldc)
            where T1 : SPMatrix<double>
            where T2 : Matrix<double>;

        /// <summary>
        /// computes the product of two sparse matrices
        /// C = op(A) * B
        /// and stores the result as a dense matrix
        /// </summary>
        /// <typeparam name="T1"> SPMatrix[Complex] </typeparam>
        /// <typeparam name="T2"> Matrix[Complex] </typeparam>
        /// <param name="operation"> specifies operation op() on sparse matrix a </param>
        /// <param name="a"> sparse matrix a </param>
        /// <param name="b"> sparse matrix b </param>
        /// <param name="layout"> describes the storage scheme for the dense matrix </param>
        /// <param name="c"> dense matrix c </param>
        /// <param name="ldc"> leading dimension of matrix c </param>
        /// <returns> result status </returns>
        SPARSE_Status SpmmdZ<T1, T2>(SPARSE_Operation operation,
            T1 a, T1 b, SPARSE_Layout layout,
            T2 c, long ldc)
            where T1 : SPMatrix<Complex>
            where T2 : Matrix<Complex>;

        #endregion
        #region ---- Sp2md [D/Z] ----

        /// <summary>
        /// Computes the product of two sparse matrices and stores the result as a dense matrix.
        /// C = alpha * opA(A) * opB(B) + beta * C
        /// </summary>
        /// <param name="transA">Specifies operation op() on sparse matrix A.</param>
        /// <param name="descrA">Structure specifying sparse matrix A's properties.</param>
        /// <param name="a">Sparse matrix A handle.</param>
        /// <param name="transB">Specifies operation op() on sparse matrix B.</param>
        /// <param name="descrB">Structure specifying sparse matrix B's properties.</param>
        /// <param name="b">Sparse matrix B handle.</param>
        /// <param name="alpha">Scalar constant alpha.</param>
        /// <param name="beta">Scalar constant beta.</param>
        /// <param name="c">Pointer to the output dense matrix C.</param>
        /// <param name="layout">Describes the storage scheme for the dense matrix.</param>
        /// <param name="ldc">Leading dimension of matrix C.</param>
        /// <returns>Result status.</returns>
        SPARSE_Status Sp2md(SPARSE_Operation transA,
            SPARSE_MatrixDescr descrA, [In] IntPtr a,
            SPARSE_Operation transB, SPARSE_MatrixDescr descrB,
            [In] IntPtr b, double alpha, double beta,
            [In, Out] double* c, SPARSE_Layout layout, long ldc);

        /// <summary>
        /// Computes the product of two sparse matrices and stores the result as a dense matrix (complex version).
        /// C = alpha * opA(A) * opB(B) + beta * C
        /// </summary>
        /// <param name="transA">Specifies operation op() on sparse matrix A.</param>
        /// <param name="descrA">Structure specifying sparse matrix A's properties.</param>
        /// <param name="a">Sparse matrix A handle.</param>
        /// <param name="transB">Specifies operation op() on sparse matrix B.</param>
        /// <param name="descrB">Structure specifying sparse matrix B's properties.</param>
        /// <param name="b">Sparse matrix B handle.</param>
        /// <param name="alpha">Pointer to scalar constant alpha (complex).</param>
        /// <param name="beta">Pointer to scalar constant beta (complex).</param>
        /// <param name="c">Pointer to the output dense matrix C (complex).</param>
        /// <param name="layout">Describes the storage scheme for the dense matrix.</param>
        /// <param name="ldc">Leading dimension of matrix C.</param>
        /// <returns>Result status.</returns>
        SPARSE_Status Sp2md(SPARSE_Operation transA,
            SPARSE_MatrixDescr descrA, [In] IntPtr a,
            SPARSE_Operation transB, SPARSE_MatrixDescr descrB,
            [In] IntPtr b, void* alpha, void* beta,
            [In, Out] void* c, SPARSE_Layout layout, long ldc);


        // wrapper
        /// <summary>
        /// computes the product of two sparse matrices
        /// C = alpha * opA(A) * opB(B) + beta * C
        /// and stores the result as a dense matrix
        /// </summary>
        /// <typeparam name="T1"> SPMatrix[double] </typeparam>
        /// <typeparam name="T2"> Matrix[double] </typeparam>
        /// <param name="transA"> specifies operation op() on sparse matrix a </param>
        /// <param name="descrA"> structure specifying sparse matrix a's properties</param>
        /// <param name="a"> sparse matrix a </param>
        /// <param name="transB"> specifies operation op() on sparse matrix b </param>
        /// <param name="descrB"> structure specifying sparse matrix b's properties </param>
        /// <param name="b"> sparse matrix b </param>
        /// <param name="alpha"> scalar constant alpha </param>
        /// <param name="beta"> scalar constant beta </param>
        /// <param name="c"> dense matrix c </param>
        /// <param name="layout"> cescribes the storage scheme for the dense matrix </param>
        /// <param name="ldc"> leading dimension of matrix c </param>
        /// <returns> result status </returns>
        SPARSE_Status Sp2mdD<T1, T2>(
            SPARSE_Operation transA, SPARSE_MatrixDescr descrA, T1 a,
            SPARSE_Operation transB, SPARSE_MatrixDescr descrB, T1 b,
            double alpha, double beta, T2 c, SPARSE_Layout layout, long ldc)
            where T1 : SPMatrix<double>
            where T2 : Matrix<double>;

        /// <summary>
        /// computes the product of two sparse matrices
        /// C = alpha * opA(A) * opB(B) + beta * C
        /// and stores the result as a dense matrix
        /// </summary>
        /// <typeparam name="T1"> SPMatrix[Complex] </typeparam>
        /// <typeparam name="T2"> Matrix[Complex] </typeparam>
        /// <param name="transA"> specifies operation op() on sparse matrix a </param>
        /// <param name="descrA"> structure specifying sparse matrix a's properties</param>
        /// <param name="a"> sparse matrix a </param>
        /// <param name="transB"> specifies operation op() on sparse matrix b </param>
        /// <param name="descrB"> structure specifying sparse matrix b's properties </param>
        /// <param name="b"> sparse matrix b </param>
        /// <param name="alpha"> scalar constant alpha </param>
        /// <param name="beta"> scalar constant beta </param>
        /// <param name="c"> dense matrix c </param>
        /// <param name="layout"> cescribes the storage scheme for the dense matrix </param>
        /// <param name="ldc"> leading dimension of matrix c </param>
        /// <returns> result status </returns>
        SPARSE_Status Sp2mdZ<T1, T2>(
            SPARSE_Operation transA, SPARSE_MatrixDescr descrA, T1 a,
            SPARSE_Operation transB, SPARSE_MatrixDescr descrB, T1 b,
            Complex alpha, Complex beta, T2 c, SPARSE_Layout layout, long ldc)
            where T1 : SPMatrix<Complex>
            where T2 : Matrix<Complex>;

        #endregion
        #region ---- Syrkd [D/Z] ----

        /// <summary>
        /// Computes the symmetric rank-k update for a sparse matrix and stores the result as a dense matrix.
        /// C = beta * C + alpha * op(A) * op(A)^T
        /// </summary>
        /// <param name="operation">Specifies the operation op() on the sparse matrix a.</param>
        /// <param name="a">Handle to the sparse matrix a.</param>
        /// <param name="alpha">Scalar constant alpha.</param>
        /// <param name="beta">Scalar constant beta.</param>
        /// <param name="c">Handle to the dense matrix c (output).</param>
        /// <param name="layout">Describes the storage scheme for the dense matrix.</param>
        /// <param name="ldc">Leading dimension of matrix c.</param>
        /// <returns>Result status of the operation.</returns>
        SPARSE_Status Syrkd(SPARSE_Operation operation,
            [In] IntPtr a, double alpha, double beta,
            [In, Out] double* c, SPARSE_Layout layout, long ldc);

        /// <summary>
        /// Computes the symmetric rank-k update for a sparse complex matrix and stores the result as a dense matrix.
        /// C = beta * C + alpha * op(A) * op(A)^H
        /// </summary>
        /// <param name="operation">Specifies the operation op() on the sparse matrix a.</param>
        /// <param name="a">Handle to the sparse matrix a.</param>
        /// <param name="alpha">Pointer to the scalar constant alpha (complex).</param>
        /// <param name="beta">Pointer to the scalar constant beta (complex).</param>
        /// <param name="c">Handle to the dense matrix c (output).</param>
        /// <param name="layout">Describes the storage scheme for the dense matrix.</param>
        /// <param name="ldc">Leading dimension of matrix c.</param>
        /// <returns>Result status of the operation.</returns>
        SPARSE_Status Syrkd(SPARSE_Operation operation,
            [In] IntPtr a, void* alpha, void* beta,
            [In, Out] void* c, SPARSE_Layout layout, long ldc);


        // wrapper
        /// <summary>
        /// computes the product of sparse matrix with its transpose 
        /// C = beta * C + alpha * op(A) * op(A)^T 
        /// and stores the result as a dense matrix
        /// </summary>
        /// <typeparam name="T1"> SPMatrix[double] </typeparam>
        /// <typeparam name="T2"> Matrix[double] </typeparam>
        /// <param name="operation"> specifies operation op() on sparse matrix a </param>
        /// <param name="a"> sparse matrix a </param>
        /// <param name="alpha"> scalar constant alpha </param>
        /// <param name="beta"> scalar constant beta </param>
        /// <param name="c"> dense matrix c </param>
        /// <param name="layout"> describes the storage scheme for the dense matrix </param>
        /// <param name="ldc"> leading dimension of matrix c </param>
        /// <returns> result status </returns>
        SPARSE_Status SyrkdD<T1, T2>(SPARSE_Operation operation,
            T1 a, double alpha, double beta,
            T2 c, SPARSE_Layout layout, long ldc)
            where T1 : SPMatrix<double>
            where T2 : Matrix<double>;

        /// <summary>
        /// computes the product of sparse matrix with its conjugate transpose 
        /// C = beta * C + alpha * op(A) * op(A)^H 
        /// and stores the result as a dense matrix
        /// </summary>
        /// <typeparam name="T1"> SPMatrix[Complex] </typeparam>
        /// <typeparam name="T2"> Matrix[Complex] </typeparam>
        /// <param name="operation"> specifies operation op() on sparse matrix a </param>
        /// <param name="a"> sparse matrix a </param>
        /// <param name="alpha"> scalar constant alpha </param>
        /// <param name="beta"> scalar constant beta </param>
        /// <param name="c"> dense matrix c </param>
        /// <param name="layout"> describes the storage scheme for the dense matrix </param>
        /// <param name="ldc"> leading dimension of matrix c </param>
        /// <returns> result status </returns>
        SPARSE_Status SyrkdZ<T1, T2>(SPARSE_Operation operation,
            T1 a, Complex alpha, Complex beta,
            T2 c, SPARSE_Layout layout, long ldc)
            where T1 : SPMatrix<Complex>
            where T2 : Matrix<Complex>;

        #endregion
        #region ---- Syprd [D/Z] ----

        /// <summary>
        /// Computes the symmetric triple product of a sparse matrix and a dense matrix,
        /// C = alpha * op(A) * B * op(A)^T + beta * C,
        /// and stores the result as a dense matrix.
        /// </summary>
        /// <param name="operation">Specifies operation op() on sparse matrix a.</param>
        /// <param name="a">Handle to the sparse matrix A.</param>
        /// <param name="b">Pointer to the dense matrix B.</param>
        /// <param name="layoutB">Describes the storage scheme for the dense matrix B.</param>
        /// <param name="ldb">Leading dimension of matrix B.</param>
        /// <param name="alpha">Scalar constant alpha.</param>
        /// <param name="beta">Scalar constant beta.</param>
        /// <param name="c">Pointer to the dense matrix C (input/output).</param>
        /// <param name="layoutC">Describes the storage scheme for the dense matrix C.</param>
        /// <param name="ldc">Leading dimension of matrix C.</param>
        /// <returns>Result status of the operation.</returns>
        SPARSE_Status Syprd(SPARSE_Operation operation,
            [In] IntPtr a, [In] IntPtr b, SPARSE_Layout layoutB, long ldb,
            double alpha, double beta,
            [In, Out] double* c, SPARSE_Layout layoutC, long ldc);

        /// <summary>
        /// Computes the symmetric triple product of a sparse matrix and a dense matrix (complex version),
        /// C = alpha * op(A) * B * op(A)^H + beta * C,
        /// and stores the result as a dense matrix.
        /// </summary>
        /// <param name="operation">Specifies operation op() on sparse matrix a.</param>
        /// <param name="a">Handle to the sparse matrix A.</param>
        /// <param name="b">Pointer to the dense matrix B.</param>
        /// <param name="layoutB">Describes the storage scheme for the dense matrix B.</param>
        /// <param name="ldb">Leading dimension of matrix B.</param>
        /// <param name="alpha">Pointer to the scalar constant alpha (complex).</param>
        /// <param name="beta">Pointer to the scalar constant beta (complex).</param>
        /// <param name="c">Pointer to the dense matrix C (input/output, complex).</param>
        /// <param name="layoutC">Describes the storage scheme for the dense matrix C.</param>
        /// <param name="ldc">Leading dimension of matrix C.</param>
        /// <returns>Result status of the operation.</returns>
        SPARSE_Status Syprd(SPARSE_Operation operation,
            [In] IntPtr a, [In] IntPtr b, SPARSE_Layout layoutB, long ldb,
            void* alpha, void* beta,
            [In, Out] void* c, SPARSE_Layout layoutC, long ldc);


        // wrapper
        /// <summary>
        /// computes the symmetric triple product of 
        /// a sparse matrix and a dense matrix
        /// C = alpha * op(A) * B * op(A)^T + beta * C
        /// and stores the result as a dense matrix
        /// </summary>
        /// <typeparam name="T1"> SPMatrix[double] </typeparam>
        /// <typeparam name="T2"> Matrix[double] </typeparam>
        /// <param name="operation"> specifies operation op() on sparse matrix a </param>
        /// <param name="a"> sparse matrix a </param>
        /// <param name="b"> dense matrix b </param>
        /// <param name="layoutB"> describes the storage scheme for the dense matrix b </param>
        /// <param name="ldb"> leading dimension of matrix b </param>
        /// <param name="alpha"> scalar constant alpha </param>
        /// <param name="beta"> scalar constant beta </param>
        /// <param name="c"> dense matrix c </param>
        /// <param name="layoutC"> describes the storage scheme for the dense matrix c </param>
        /// <param name="ldc"> leading dimension of matrix c </param>
        /// <returns> result status </returns>
        SPARSE_Status SyprdD<T1, T2>(SPARSE_Operation operation,
            T1 a, T2 b, SPARSE_Layout layoutB, long ldb,
            double alpha, double beta,
            T2 c, SPARSE_Layout layoutC, long ldc)
            where T1 : SPMatrix<double>
            where T2 : Matrix<double>;

        /// <summary>
        /// computes the symmetric triple product of 
        /// a sparse matrix and a dense matrix
        /// C = alpha * op(A) * B * op(A)^H + beta * C
        /// and stores the result as a dense matrix
        /// </summary>
        /// <typeparam name="T1"> SPMatrix[Complex] </typeparam>
        /// <typeparam name="T2"> Matrix[Complex] </typeparam>
        /// <param name="operation"> specifies operation op() on sparse matrix a </param>
        /// <param name="a"> sparse matrix a </param>
        /// <param name="b"> dense matrix b </param>
        /// <param name="layoutB"> describes the storage scheme for the dense matrix b </param>
        /// <param name="ldb"> leading dimension of matrix b </param>
        /// <param name="alpha"> scalar constant alpha </param>
        /// <param name="beta"> scalar constant beta </param>
        /// <param name="c"> dense matrix c </param>
        /// <param name="layoutC"> describes the storage scheme for the dense matrix c </param>
        /// <param name="ldc"> leading dimension of matrix c </param>
        /// <returns> result status </returns>
        SPARSE_Status SyprdZ<T1, T2>(SPARSE_Operation operation,
            T1 a, T2 b, SPARSE_Layout layoutB, long ldb,
            Complex alpha, Complex beta,
            T2 c, SPARSE_Layout layoutC, long ldc)
            where T1 : SPMatrix<Complex>
            where T2 : Matrix<Complex>;

        #endregion
        #region ---- Sorv [D] ----

        /// <summary>
        /// Computes forward, backward sweeps or a symmetric
        /// successive over-relaxation (SOR) preconditioner operation
        /// for a sparse matrix.
        /// </summary>
        /// <param name="type">Specifies the operation performed by the SORV preconditioner.</param>
        /// <param name="descrA">Structure specifying sparse matrix properties.</param>
        /// <param name="a">Sparse matrix handle.</param>
        /// <param name="omega">Relaxation factor.</param>
        /// <param name="alpha">Parameter that could be used to normalize or set to zero the vector x that holds the initial guess.</param>
        /// <param name="x">Initial guess on input (dense) vector x. Modified in place.</param>
        /// <param name="b">Right-hand side (dense) vector b.</param>
        /// <returns>Result status of the SORV operation.</returns>
        SPARSE_Status Sorv(SPARSE_SorType type,
            SPARSE_MatrixDescr descrA, [In] IntPtr a, double omega,
            double alpha, [In, Out] double* x, [In] double* b);


        // wrapper
        /// <summary>
        /// computes forward, backward sweeps or a symmetric
        /// successive over-relaxation preconditioner operation
        /// </summary>
        /// <typeparam name="T1"> SPMatrix[double] </typeparam>
        /// <typeparam name="T2"> Matrix[double] </typeparam>
        /// <param name="type"> specifies the operation performed by the SORV preconditioner </param>
        /// <param name="descrA"> structure specifying sparse matrix properties </param>
        /// <param name="a"> sparse matrix a </param>
        /// <param name="omega"> relaxation factor </param>
        /// <param name="alpha"> parameter that could be used to normalize or set to zero the vector x that holds the initial guess </param>
        /// <param name="x"> initial guess on input (dense) vector x </param>
        /// <param name="b"> righr-hand side (dense) vector b </param>
        /// <returns> result status </returns>
        SPARSE_Status SorvD<T1, T2>(SPARSE_SorType type,
            SPARSE_MatrixDescr descrA, T1 a, double omega, double alpha,
            T2 x, T2 b)
            where T1 : SPMatrix<double>
            where T2 : Matrix<double>;

        #endregion

        #endregion

    }
}
