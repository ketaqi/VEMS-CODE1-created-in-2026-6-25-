using System.Security;
using System.Runtime.InteropServices;
using Complex = System.Numerics.Complex;

namespace VEMS.MathCore.NativeProviders
{

    [SuppressUnmanagedCodeSecurity]
    internal sealed unsafe class CuMATHNative
    {
        private CuMATHNative() { }
        private const string DllName = "CuMath";

        #region ------------- vdAdd, vzAdd, vdSub, vzSub ---------------

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        // performs element by element addition of vector a and vector b
        public static extern void vdAdd(long n,
            [In] double* a, [In] double* b,
            [Out] double* y,
            long blocks, long threads);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        // performs element by element addition of vector a and vector b
        internal static extern void vzAdd(long n,
            [In] Complex* a, [In] Complex* b,
            [Out] Complex* y);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        // performs element by element substraction of vector b from vector a
        internal static extern void vdSub(int n,
            [In] double* a, [In] double* b,
            [Out] double* y);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        // performs element by element substraction of vector b from vector a
        internal static extern void vdSub(long n,
            [In] double* a, [In] double* b,
            [Out] double* y);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        // performs element by element substraction of vector b from vector a
        internal static extern void vzSub(int n,
            [In] Complex* a, [In] Complex* b,
            [Out] Complex* y);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        // performs element by element substraction of vector b from vector a
        internal static extern void vzSub(long n,
            [In] Complex* a, [In] Complex* b,
            [Out] Complex* y);

        #endregion






    }


}
