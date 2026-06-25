namespace VEMS.AMathCore
{

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
