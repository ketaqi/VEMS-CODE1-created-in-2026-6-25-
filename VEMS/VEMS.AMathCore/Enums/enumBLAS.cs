namespace VEMS.AMathCore
{

    /// <summary>
    /// matrix layout option
    /// </summary>
    public enum BLAS_Layout : int
    {
        /// <summary>
        /// row-major layout = 101
        /// </summary>
        RowMajor = 101,
        /// <summary>
        /// column-major layout = 102
        /// </summary>
        ColMajor = 102
    }

    /// <summary>
    /// matrix transpose option
    /// </summary>
    public enum BLAS_Transpose : int
    {
        /// <summary>
        /// no transpose = 111
        /// </summary>
        NoTrans = 111,
        /// <summary>
        /// transpose = 112
        /// </summary>
        Trans = 112,
        /// <summary>
        /// conjugate transpose = 113
        /// </summary>
        ConjTrans = 113
    }

    /// <summary>
    /// matrix triangular indicator
    /// </summary>
    public enum BLAS_Uplo : int
    {
        /// <summary>
        /// upper triangular = 121
        /// </summary>
        Upper = 121,

        /// <summary>
        /// lower triangular = 122
        /// </summary>
        Lower = 122,
    }

    /// <summary>
    /// matrix diagonal type
    /// </summary>
    public enum BLAS_Diag : int
    {
        /// <summary>
        /// non-unit diagonal = 131
        /// </summary>
        NonUnit = 131,
        /// <summary>
        /// unit diagonal = 132
        /// </summary>
        Unit = 132
    }

    /// <summary>
    /// matrix side option
    /// </summary>
    public enum BLAS_Side : int
    {
        /// <summary>
        /// left side = 141
        /// </summary>
        Left = 141,

        /// <summary>
        /// right side = 142
        /// </summary>
        Right = 142
    }

    /// <summary>
    /// matrix storage option
    /// </summary>
    public enum BLAS_Storage : int
    {
        /// <summary>
        /// packed storage = 151
        /// </summary>
        Packed = 151
    }

    /// <summary>
    /// matrix identifier option
    /// </summary>
    public enum BLAS_Identifier : int
    {
        /// <summary>
        /// matrix A = 161
        /// </summary>
        AMatrix = 161,

        /// <summary>
        /// matrix B = 162
        /// </summary>
        BMatrix = 162,
    }

    /// <summary>
    /// matrix offset option
    /// </summary>
    public enum BLAS_Offset : int
    {
        /// <summary>
        /// row offset = 171
        /// </summary>
        RowOffset = 171,

        /// <summary>
        /// column offset = 172
        /// </summary>
        ColOffset = 172,

        /// <summary>
        /// fix offset = 173
        /// </summary>
        FixOffset = 173
    }

}
