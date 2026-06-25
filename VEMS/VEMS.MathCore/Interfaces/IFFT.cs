using Complex = System.Numerics.Complex;

namespace VEMS.MathCore
{
    /// <summary>
    /// FFT interface
    /// </summary>
    public interface IFFT 
    {

        /// <summary>
        /// DftiCreateDescriptor wrapper (1D)
        /// </summary>
        /// <param name="desc"> DFTI descriptor </param>
        /// <param name="precision"> precision (ConfigValue) </param>
        /// <param name="domain"> domain (ConfigValue) </param>
        /// <param name="dimension"> dimension of the transform </param>
        /// <param name="length"> length given in long format </param>
        /// <returns> error information </returns>
        int DftiCreateDescriptor(ref IntPtr desc,
            FFTConfigValue precision, FFTConfigValue domain, int dimension, long length);

        /// <summary>
		/// DftiCreateDescriptor wrapper (2D)
		/// </summary>
		/// <param name="desc"> DFTI descriptor </param>
		/// <param name="precision"> precision (ConfigValue) </param>
		/// <param name="domain"> domain (ConfigValue) </param>
		/// <param name="dimension"> dimension of the transform </param>
		/// <param name="lengths"> lengths given in long format </param>
		/// <returns> error information </returns>
        int DftiCreateDescriptor(ref IntPtr desc,
            FFTConfigValue precision, FFTConfigValue domain, int dimension, long[] lengths);

        /// <summary>
        /// DftiSetValue wrapper
        /// </summary>
        /// <param name="desc"> DFTI descriptor </param>
        /// <param name="config_param"> config_param (ConfigParam) </param>
        /// <param name="config_val"> config_val (int)</param>
        /// <returns> error information </returns>
        int DftiSetValue(IntPtr desc,
            FFTConfigParam config_param, FFTConfigValue config_val);

        /// <summary>
        /// DftiSetValue wrapper
        /// </summary>
		/// <param name="desc"> DFTI descriptor </param>
		/// <param name="config_param"> config_param (ConfigParam) </param>
		/// <param name="config_val"> config_val (double)</param>
		/// <returns> error information </returns> 
        int DftiSetValue(IntPtr desc,
            FFTConfigParam config_param, double config_val);

        /// <summary>
		/// DftiCommitDescriptor wrapper
		/// </summary>
		/// <param name="desc"> DFTI descriptor </param>
		/// <returns> error information </returns>
        int DftiCommitDescriptor(IntPtr desc);

        /// <summary>
        /// DftiComputeForward wrapper
        /// </summary>
        /// <param name="desc"> DFTI descriptor </param>
        /// <param name="x"> array data x (in / out) </param>
        /// <returns> error information </returns>
        int DftiComputeForward(IntPtr desc,
            DenseArrayBase<Complex> x);

        /// <summary>
		/// DftiComputeForward wrapper
		/// </summary>
		/// <param name="desc"> DFTI descriptor </param>
        /// <param name="x_in"> input array data </param>
        /// <param name="x_out"> output array data </param>
        /// <returns> error information </returns>
        int DftiComputeForward(IntPtr desc,
            DenseArrayBase<Complex> x_in, DenseArrayBase<Complex> x_out);

        /// <summary>
		/// DftiComputeBackward wrapper
		/// </summary>
		/// <param name="desc"> DFTI descriptor </param>
        /// <param name="x"> array data x (in / out) </param>
        /// <returns> error information </returns>
        int DftiComputeBackward(IntPtr desc,
            DenseArrayBase<Complex> x);

        /// <summary>
		/// DftiComputeBackward wrapper
		/// </summary>
		/// <param name="desc"> DFTI descriptor </param>
        /// <param name="x_in"> input array data </param>
        /// <param name="x_out"> output array data </param>
        /// <returns> error information </returns>
        int DftiComputeBackward(IntPtr desc,
            DenseArrayBase<Complex> x_in, DenseArrayBase<Complex> x_out);

        /// <summary>
		/// DftiFreeDescriptor wrapper
		/// </summary>
		/// <param name="desc"> DFTI descriptor </param>
		/// <returns> error information </returns>
        int DftiFreeDescriptor(ref IntPtr desc);

    }

    /// <summary>
    /// FFT configuration parameters
    /// </summary>
    public enum FFTConfigParam
    {
        /// <summary>
        /// precision
        /// </summary>
        PRECISION = 3,
        /// <summary>
        /// forward domain
        /// </summary>
        FORWARD_DOMAIN = 0,
        /// <summary>
        /// dimension
        /// </summary>
        DIMENSION = 1,
        /// <summary>
        /// lengths
        /// </summary>
        LENGTHS = 2,
        /// <summary>
        /// number of transforms
        /// </summary>
        NUMBER_OF_TRANSFORMS = 7,
        /// <summary>
        /// forward scale
        /// </summary>
        FORWARD_SCALE = 4,
        /// <summary>
        /// backward scale
        /// </summary>
        BACKWARD_SCALE = 5,
        /// <summary>
        /// placement
        /// </summary>
        PLACEMENT = 11,
        /// <summary>
        /// complex storage
        /// </summary>
        COMPLEX_STORAGE = 8,
        /// <summary>
        /// real storage
        /// </summary>
        REAL_STORAGE = 9,
        /// <summary>
        /// conjugate even storage
        /// </summary>
        CONJUGATE_EVEN_STORAGE = 10,
        /// <summary>
        /// descriptor name
        /// </summary>
        DESCRIPTOR_NAME = 20,
        /// <summary>
        /// packed format
        /// </summary>
        PACKED_FORMAT = 21,
        /// <summary>
        /// number of user threads
        /// </summary>
        NUMBER_OF_USER_THREADS = 26,
        /// <summary>
        /// input distance
        /// </summary>
        INPUT_DISTANCE = 14,
        /// <summary>
        /// output distance
        /// </summary>
        OUTPUT_DISTANCE = 15,
        /// <summary>
        /// input stride
        /// </summary>
        INPUT_STRIDES = 12,
        /// <summary>
        /// output stride
        /// </summary>
        OUTPUT_STRIDES = 13,
        /// <summary>
        /// ordering
        /// </summary>
        ORDERING = 18,
        /// <summary>
        /// transpose
        /// </summary>
        TRANSPOSE = 19,
        /// <summary>
        /// commit status
        /// </summary>
        COMMIT_STATUS = 22,
        /// <summary>
        /// version
        /// </summary>
        VERSION = 23
    }

    /// <summary>
    /// FFT configuration values
    /// </summary>
    public enum FFTConfigValue
    {
        /// <summary>
        /// single precision
        /// </summary>
        SINGLE = 35,
        /// <summary>
        /// double precision
        /// </summary>
        DOUBLE = 36,
        /// <summary>
        /// complex
        /// </summary>
        COMPLEX = 32,
        /// <summary>
        /// real
        /// </summary>
        REAL = 33,
        /// <summary>
        /// in-place
        /// </summary>
        INPLACE = 43,
        /// <summary>
        /// not in-place
        /// </summary>
        NOT_INPLACE = 44,
        /// <summary>
        /// complex-complex
        /// </summary>
        COMPLEX_COMPLEX = 39,
        /// <summary>
        /// real-real
        /// </summary>
        REAL_REAL = 42,
        /// <summary>
        /// complex-real
        /// </summary>
        COMPLEX_REAL = 40,
        /// <summary>
        /// real-complex
        /// </summary>
        REAL_COMPLEX = 41,
        /// <summary>
        /// committed
        /// </summary>
        COMMITTED = 30,
        /// <summary>
        /// uncommitted
        /// </summary>
        UNCOMMITTED = 31,
        /// <summary>
        /// ordered
        /// </summary>
        ORDERED = 48,
        /// <summary>
        /// backward scrambled
        /// </summary>
        BACKWARD_SCRAMBLED = 49,
        /// <summary>
        /// none
        /// </summary>
        NONE = 53,
        /// <summary>
        /// ccs format
        /// </summary>
        CCS_FORMAT = 54,
        /// <summary>
        /// pack format
        /// </summary>
        PACK_FORMAT = 55,
        /// <summary>
        /// perm format
        /// </summary>
        PERM_FORMAT = 56,
        /// <summary>
        /// cce format
        /// </summary>
        CCE_FORMAT = 57,
        /// <summary>
        /// version length
        /// </summary>
        VERSION_LENGTH = 198,
        /// <summary>
        /// max name length
        /// </summary>
        MAX_NAME_LENGTH = 10,
        /// <summary>
        /// max message length
        /// </summary>
        MAX_MESSAGE_LENGTH = 40
    }

    /// <summary>
    /// FFT error codes
    /// </summary>
    internal enum FFTError
    {
        NO_ERROR = 0,
        MEMORY_ERROR = 1,
        INVALID_CONFIGURATION = 2,
        INCONSISTENT_CONFIGURATION = 3,
        NUMBER_OF_THREADS_ERROR = 8,
        MULTITHREADED_ERROR = 4,
        BAD_DESCRIPTOR = 5,
        UNIMPLEMENTED = 6,
        MKL_INTERNAL_ERROR = 7,
        LENGTH_EXCEEDS_INT32 = 9
    }


}
