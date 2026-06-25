using System.Runtime.InteropServices;

namespace VEMS.AMathCore
{

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

}
