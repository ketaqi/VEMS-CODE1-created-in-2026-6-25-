using System;
using System.Diagnostics;

using System.Security;
using System.Runtime.InteropServices;

using Complex = System.Numerics.Complex;

namespace VEMS.MathCore
{
    class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("MathCore Test Environment");

            #region system information
            Console.WriteLine($"This is a {IntPtr.Size * 8}-bit application");
            if (System.Numerics.Vector.IsHardwareAccelerated)
                Console.WriteLine($"SIMD acceleration ENABLED");
            else
                Console.WriteLine($"SIMD acceleration DISABLED");
            #endregion

            #region settings

            // set Intel MKL interface
            Console.WriteLine("Setting Intel MKL Interface ...");
            int MKL_Interface_LP64 = 0x0;
            int MKL_Interface_ILP64 = 0x1;
            int MKL_Interface_GNU = 0x2;
            //IntelMKLNative.mkl_set_interface_layer(ref MKL_Interface_LP64);
            //_ = IntelMKLNative.mkl_set_interface_layer(ref MKL_Interface_ILP64);
            //IntelMKLNative.mkl_set_interface_layer(ref MKL_Interface_GNU);
            Console.WriteLine("MKL interface set.");

            // number of threads for Intel MKL
            Console.WriteLine("Checking Intel MKL number of threads ...");
            //var nx = IntelMKLConfig.GetMaxThreads(); //Config.GetMKLMaxThreads();
            //Console.WriteLine("Initial Intel MKL number of threads: " + nx.ToString() + "");
            //var nt = 48;
            //Console.WriteLine("Setting Intel MKL number of threads to " + nt.ToString() + " ...");
            //IntelMKLNative.mkl_set_num_threads(ref nt);
            //Console.WriteLine("MKL number of threads set."); 

            #endregion

            #region basic linear algebra examples

            //ComplexIndexMaxAbsSample(3);
            //RealIndexMaxAbsSample(3);

            #endregion

            #region performance benchmarks

            long n = 7501;
            int nl = 595001;
            //ComplexEigenSystemBench(n, LAPACK.SourceLibrary.OpenBLAS64);
            //ComplexLinearSolveBench(n, LAPACK.SourceLibrary.IntelMKL); 
            for(int i = 0; i < 20; i++)
            {
                //Console.WriteLine("i = " + i.ToString());
                //ComplexLinearSolveBench(n);
                //ComplexLinearSolveLiteBench(nl);
                //ZgesvTest(nl);
                
                //DataShiftBeforeBench(nl); // check in release mode!
                //DataShiftBeforeV2Bench(nl); // check in release mode!
            }
            //RealEigenSystemBench(n);
            //RealLinearSolveBench(n);

            #endregion

        }



        /// <summary>
        /// simple logger with a flag
        /// </summary>
        /// <param name="showLogging"> flag whether to show the logging or not </param>
        /// <param name="info"> info to show </param>
        public static void Logger(bool showLogging, string info)
        {
            if (showLogging)
            {
                Console.WriteLine(info);
            }
        }


    }
}
