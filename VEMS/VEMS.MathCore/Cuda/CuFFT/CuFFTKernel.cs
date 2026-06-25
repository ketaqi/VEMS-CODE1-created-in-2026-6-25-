using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.Runtime.InteropServices;

namespace VEMS.MathCore
{
    #region ---- Core Wrappers ----
    /// <summary>
    /// CuFFT Exception class
    /// </summary>
    public class CuFFTException : Exception
    {
        #region properties
        /// <summary>
        /// gets the error code
        /// </summary>
        public CuFFT_Result ErrorCode { get; }
        #endregion
        #region constructors
        /// <summary>
        /// constructs a CuFFTException
        /// </summary>
        /// <param name="errorCode"> the error code </param>
        /// <param name="message"> the error message </param>
        public CuFFTException(CuFFT_Result errorCode, string message)
            : base($"{message} (Error Code: {errorCode})")
        {
            ErrorCode = errorCode;
        }
        #endregion
    }

    /// <summary>
    /// CUFFT base class
    /// </summary>
    public static class CuFFTBase
    {
        #region CuFFT Version and Error Handling
        /// <summary>
        /// gets the version of the CUFFT library
        /// </summary>
        /// <returns> the version </returns>
        public static int GetVersion()
        {
            int version = 0;
            var result = CuFFTNative.cufftGetVersion(ref version);
            CheckResult(result, "Get version");
            return version;
        }

        /// <summary>
        /// checks the result of a CUFFT operation
        /// </summary>
        /// <param name="result"> the result to check </param>
        /// <param name="operation"> the operation that was performed </param>
        /// <exception cref="CuFFTException"> if the result is not success </exception>
        private static void CheckResult(CuFFT_Result result, string operation)
        {
            if (result != CuFFT_Result.Success)
                throw new CuFFTException(result, operation);
        }
        #endregion
        #region CuFFT Plan Initialization
        /// <summary>
        /// creates a 1D plan
        /// </summary>
        /// <param name="nx"> the size of the 1D transform </param>
        /// <param name="type"> the type of the transform </param>
        /// <param name="batch"> the number of transforms to perform in batch </param>
        /// <returns> the plan </returns>
        public static IntPtr CreatePlan1D(long nx, CuFFT_Type type, long batch = 1)
        {
            IntPtr plan = IntPtr.Zero;
            var result = CuFFTNative.cufftPlan1d(ref plan, nx, type, batch);
            CheckResult(result, "Create 1D plan");
            return plan;
        }

        /// <summary>
        /// creates a 2D plan
        /// </summary>
        /// <param name="nx"> the size of the 1st dimension </param>
        /// <param name="ny"> the size of the 2nd dimension </param>
        /// <param name="type"> the type of the transform </param>
        /// <returns> the plan </returns>
        public static IntPtr CreatePlan2D(long nx, long ny, CuFFT_Type type)
        {
            IntPtr plan = IntPtr.Zero;
            var result = CuFFTNative.cufftPlan2d(ref plan, nx, ny, type);
            CheckResult(result, "Create 2D plan");
            return plan;
        }

        /// <summary>
        /// creates a 3D plan
        /// </summary>
        /// <param name="nx"> the size of the 1st dimension </param>
        /// <param name="ny"> the size of the 2nd dimension </param>
        /// <param name="nz"> the size of the 3rd dimension </param>
        /// <param name="type"> the type of the transform </param>
        /// <returns> the plan </returns>
        public static IntPtr CreatePlan3D(long nx, long ny, long nz, CuFFT_Type type)
        {
            IntPtr plan = IntPtr.Zero;
            var result = CuFFTNative.cufftPlan3d(ref plan, nx, ny, nz, type);
            CheckResult(result, "Create 3D plan");
            return plan;
        }

        /// <summary>
        /// creates a batch FFT plan for executing multiple FFTs of the same size
        /// </summary>
        /// <param name="rank"> dimensionality of the FFT, must be greater than or equal to 1 </param>
        /// <param name="n"> pointer to array containing the size of each dimension </param>
        /// <param name="inembed"> input data embedded size array pointer, uses default if NULL </param>
        /// <param name="istride"> stride between two consecutive elements in the same position across different batches in input data </param>
        /// <param name="idist"> distance between the first element of one batch to the first element of the next batch in input data </param>
        /// <param name="onembed"> output data embedded size array pointer, uses default if NULL </param>
        /// <param name="ostride"> stride between two consecutive elements in the same position across different batches in output data </param>
        /// <param name="odist"> distance between the first element of one batch to the first element of the next batch in output data </param>
        /// <param name="type"> type of the FFT transform (such as C2C, R2C, C2R, etc.) </param>
        /// <param name="batch"> number of FFT2 to process in the batch </param>
        /// <returns> returns the IntPtr handle of the created FFT plan, or IntPtr.Zero if creation fails </returns>
        public static unsafe IntPtr CreatePlanMany(long rank, long* n,
            long* inembed, long istride, long idist,
            long* onembed, long ostride, long odist,
            CuFFT_Type type, long batch)
        {
            IntPtr plan = IntPtr.Zero;
            var result = CuFFTNative.cufftPlanMany(
                ref plan, rank, n,
                inembed, istride, idist,
                onembed, ostride, odist,
                type, batch);
            CheckResult(result, "Create PlanMany");
            return plan;
        }

        /// <summary>
        /// destroys a plan
        /// </summary>
        /// <param name="plan"> the plan to destroy </param>
        public static void DestroyPlan(IntPtr plan)
        {
            if (plan != IntPtr.Zero)
            {
                var result = CuFFTNative.cufftDestroy(plan);
                CheckResult(result, "Destroy plan");
            }
        }
        #endregion
        #region CuFFT Execution Type
        /// <summary>
        /// executes a complex to complex transform (float)
        /// </summary>
        /// <param name="plan"> FFT plan handle that defines the configuration and parameters of the transform </param>
        /// <param name="input"> pointer to the input complex array </param>
        /// <param name="output"> pointer to the output complex array </param>
        /// <param name="direction"> specifies the direction of the FFT transform (forward or inverse)</param>
        public static unsafe void ExecuteC2C(IntPtr plan, Complex* input, Complex* output,
            CuFFT_Direction direction)
        {
            var result = CuFFTNative.cufftExecC2C(plan, input, output, direction);
            CheckResult(result, "Execute C2C");
        }

        /// <summary>
        /// executes a real to complex transform (float)
        /// </summary>
        /// <param name="plan"> FFT plan handle that defines the configuration and parameters of the transform </param>
        /// <param name="input"> pointer to the input float real array </param>
        /// <param name="output"> pointer to the output complex array</param>
        public static unsafe void ExecuteR2C(IntPtr plan, float* input, Complex* output)
        {
            var result = CuFFTNative.cufftExecR2C(plan, input, output);
            CheckResult(result, "Execute R2C");
        }

        /// <summary>
        /// executes a complex to real transform (float)
        /// </summary>
        /// <param name="plan"> FFT plan handle that defines the configuration and parameters of the transform </param>
        /// <param name="input"> pointer to the input float complex array </param>
        /// <param name="output"> pointer to the output float real array </param>
        public static unsafe void ExecuteC2R(IntPtr plan, Complex* input, float* output)
        {
            var result = CuFFTNative.cufftExecC2R(plan, input, output);
            CheckResult(result, "Execute C2R");
        }

        /// <summary>
        /// executes a complex to complex transform (double)
        /// </summary>
        /// <param name="plan"> FFT plan handle that defines the configuration and parameters of the transform </param>
        /// <param name="input"> pointer to the input double complex array </param>
        /// <param name="output"> pointer to the output double complex array </param>
        /// <param name="direction"> specifies the direction of the FFT transform (forward or inverse) </param>
        public static unsafe void ExecuteZ2Z(IntPtr plan, Complex* input, Complex* output,
            CuFFT_Direction direction)
        {
            var result = CuFFTNative.cufftExecZ2Z(plan, input, output, direction);
            CheckResult(result, "Execute Z2Z");
        }

        /// <summary>
        /// executes a complex to double transform (double)
        /// </summary>
        /// <param name="plan"> FFT plan handle that defines the configuration and parameters of the transform </param>
        /// <param name="input"> pointer to the input double complex array </param>
        /// <param name="output"> pointer to the output double real array </param>
        public static unsafe void ExecuteZ2D(IntPtr plan, Complex* input, double* output)
        {
            var result = CuFFTNative.cufftExecZ2D(plan, input, output);
            CheckResult(result, "Execute Z2D");
        }

        /// <summary>
        /// executes a double to complex transform (double)
        /// </summary>
        /// <param name="plan"> FFT plan handle that defines the configuration and parameters of the transform </param>
        /// <param name="input"> pointer to the input double real array </param>
        /// <param name="output"> pointer to the output double complex array </param>
        public static unsafe void ExecuteD2Z(IntPtr plan, double* input, Complex* output)
        {
            var result = CuFFTNative.cufftExecD2Z(plan, input, output);
            CheckResult(result, "Execute D2Z");
        }
        #endregion
        #region CuFFT Estimate

        /// <summary>
        /// estimates the workspace size required for a 1D FFT
        /// This function calculates the memory workspace size needed to execute a 1D FFT operation,
        /// allowing for optimized memory allocation when creating plans.
        /// </summary>
        /// <param name="nx"> length of input data, representing the number of points in the FFT transform </param>
        /// <param name="type"> type of FFT transform, specifying whether it's real-to-complex, complex-to-real, or complex-to-complex </param>
        /// <param name="batch"> batch count, defaults to 1, used to specify the number of simulaneous FFTs to process </param>
        /// <returns> returns the estimated workspace size in bytes, used for GPU memory allocation </returns>
        public static long EstimateWorkSize1D(long nx, CuFFT_Type type, long batch = 1)
        {
            long workSize = 0;
            var result = CuFFTNative.cufftEstimate1d(nx, type, batch, ref workSize);
            CheckResult(result, "Estimate 1D work size");
            return workSize;
        }

        /// <summary>
        /// estimates the workspace size required for a 2D FFT
        /// This function calculates the memory workspace size needed to execute a 2D FFT operation,
        /// allowing for optimized memory allocation when creating plans.
        /// </summary>
        /// <param name="nx"> length in the x direction </param>
        /// <param name="ny"> length in the t direction </param>
        /// <param name="type"> type of FFT transform, specifying whether it's real-to-complex, complex-to-real, or complex-to-complex </param>
        /// <returns> returns the estimated workspace size in bytes, used for GPU memory allocation </returns>
        public static long EstimateWorkSize2D(long nx, long ny, CuFFT_Type type)
        {
            long workSize = 0;
            var result = CuFFTNative.cufftEstimate2d(nx, ny, type, ref workSize);
            CheckResult(result, "Estimate 2D work size");
            return workSize;
        }

        /// <summary>
        /// estimates the workspace size required for a 3D FFT
        /// This function calculates the memory workspace size needed to execute a 3D FFT operation,
        /// allowing for optimized memory allocation when creating plans.
        /// </summary>
        /// <param name="nx"> length in the x direction </param>
        /// <param name="ny"> length in the y direction </param>
        /// <param name="nz"> length in the z direction </param>
        /// <param name="type"> type of FFT transform, specifying whether it's real-to-complex, complex-to-real, or complex-to-complex </param>
        /// <returns> returns the estimated workspace size in bytes, used for GPU memory allocation </returns>
        public static long EstimateWorkSize3D(long nx, long ny, long nz, CuFFT_Type type)
        {
            long workSize = 0;
            var result = CuFFTNative.cufftEstimate3d(nx, ny, nz, type, ref workSize);
            CheckResult(result, "Estimate 3D work size");
            return workSize;
        }

        /// <summary>
        /// estimates the workspace size required for a many-dimensional FFT
        /// This function calculates the memory workspace size needed to execute a many-dimensional FFT operation,
        /// allowing for optimized memory allocation when creating plans.
        /// </summary>
        /// <param name="rank"> number of dimensions of the transform </param>
        /// <param name="n"> pointer to array containing the size of each dimension </param>
        /// <param name="inembed"> pointer to input data embedded size array </param>
        /// <param name="istride"> stride of input data (distance between adjacent elements) </param>
        /// <param name="idist"> distance between first elements of input batches </param>
        /// <param name="onembed"> pointer to output data embedded size array </param>
        /// <param name="ostride"> stride of output data (distance between adjacent elements) </param>
        /// <param name="odist"> distance between first elements of output batches </param>
        /// <param name="type"> type of FFT transform </param>
        /// <param name="batch"> number of transforms to process in batch </param>
        /// <returns> estimated workspace size in bytes </returns>
        public static unsafe long EstimateWorkSizeMany(long rank, long* n,
            long* inembed, long istride, long idist,
            long* onembed, long ostride, long odist,
            CuFFT_Type type, long batch)
        {
            long workSize = 0;
            var result = CuFFTNative.cufftEstimateMany(
                rank, n,
                inembed, istride, idist,
                onembed, ostride, odist,
                type, batch, ref workSize);
            CheckResult(result, "Estimate many work size");
            return workSize;
        }
        #endregion
    }
    #endregion
}
