using System.Runtime.InteropServices;

namespace VEMS.AMathCore
{

    /// <summary>
    /// Intel MKL interface options 
    /// </summary>
    public enum MKL_Interface : int
    {
        /// <summary>
        /// LP64 option
        /// </summary>
        LP64 = 0,

        /// <summary>
        /// ILP64 option (default)
        /// </summary>
        ILP64 = 1,

        /// <summary>
        /// GNU option
        /// </summary>
        GNU = 2
    }

    /// <summary>
    /// Intel MKL instruction options
    /// </summary>
    public enum MKL_Instructions
    {
        /// <summary>
        /// SSE4.2
        /// </summary>
        SSE4_2 = 0,

        /// <summary>
        /// AVX
        /// </summary>
        AVX = 1,

        /// <summary>
        /// AVX2
        /// </summary>
        AVX2 = 2,

        /// <summary>
        /// AVX512_MIC
        /// </summary>
        AVX512_MIC = 3,

        /// <summary>
        /// AVX512
        /// </summary>
        AVX512 = 4,

        /// <summary>
        /// AVX512_MIC_E1
        /// </summary>
        AVX512_MIC_E1 = 5,

        /// <summary>
        /// AVX512_MIC_E2
        /// </summary>
        AVX512_E1 = 6,

        /// <summary>
        /// AVX512_E2  
        /// </summary>
        AVX512_E2 = 7,

        /// <summary>
        /// AVX512_E3
        /// </summary>
        AVX512_E3 = 8,

        /// <summary>
        /// AVX512_E4
        /// </summary>
        AVX512_E4 = 9,

        /// <summary>
        /// AVX2_E1
        /// </summary>
        AVX2_E1 = 10,

        /// <summary>
        /// AVX512_E5
        /// </summary>
        AVX512_E5 = 11
    }

    /// <summary>
    /// Intel MKL version
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct MKLVersion
    {
        /// <summary>
        /// major version number
        /// </summary>
        public long Major;

        /// <summary>
        /// minor version number
        /// </summary>
        public long Minor;

        /// <summary>
        /// update number
        /// </summary>
        public long Update;

        /// <summary>
        /// patch number
        /// </summary>
        public int Patch;

        /// <summary>
        /// product status
        /// </summary>
        public IntPtr ProductStatus;

        /// <summary>
        /// build
        /// </summary>
        public IntPtr Build;

        /// <summary>
        /// processor
        /// </summary>
        public IntPtr Processor;

        /// <summary>
        /// platform
        /// </summary>
        public IntPtr Platform;
    }

    internal enum MKL_Layout
    {
        RowMajor = 101,
        ColMajor = 102
    }

    internal enum MKL_Transpose
    {
        NoTrans = 111,
        Trans = 112,
        ConjTrans = 113,
        Conj = 114
    }

    internal enum MKL_Uplo
    {
        Upper = 121,
        Lower = 122
    }

    internal enum MKL_Diag
    {
        NonUnit = 131,
        Unit = 132
    }

    internal enum MKL_Side
    {
        Left = 141,
        Right = 142
    }

    internal enum MKL_CompactPack
    {
        SSE = 181,
        AVX = 182,
        AVX512 = 183
    }

    /// <summary>
    /// Jit status
    /// </summary>
    public enum MKL_JitStatus
    {
        /// <summary>
        /// Success
        /// </summary>
        Success = 0,

        /// <summary>
        /// No Jit
        /// </summary>
        NoJit = 1,

        /// <summary>
        /// Jit error
        /// </summary>
        Error = 2

    }

}
