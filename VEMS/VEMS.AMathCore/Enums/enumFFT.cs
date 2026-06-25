namespace VEMS.AMathCore
{

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


    /// <summary>
    /// DFTI error codes
    /// </summary>
    internal enum DFTI_Error
    {
        NoError = 0,
        MemoryError = 1,
        InvalidConfiguration = 2,
        InconsistentConfiguration = 3,
        MultithreadError = 4,
        BadDescriptor = 5,
        Unimplemented = 6,
        MKLInternalError = 7,
        NumberOfThreadsError = 8,
        LengthExceedsINT32 = 9,
        //MemoryExceedsINT32 = 9,
        NoWorkspace = 11,
    }

    internal enum DFTI_ConfigParam
    {
        ForwardDomain = 0,
        Dimension = 1,
        Lengths = 2,
        Precision = 3,
        ForwardScale = 4,
        BackwardScale = 5,
        NumberOfTransforms = 7,
        ComplexStorage = 8,
        RealStorage = 9,
        ConjugateEvenStorage = 10,
        Placement = 11,
        InputStrides = 12,
        OutputStrides = 13,
        InputDistance = 14,
        OutputDistance = 15,
        Workspace = 17,
        Ordering = 18,
        Transpose = 19,
        DescriptorName = 20,
        PackedFormat = 21,
        CommitStatus = 22,
        Version  =23,
        NumberOfUserThreads = 26,
        ThreadLimit = 27,
        DestroyInput = 28,
        FWDDistance = 58,
        BWDDistance = 59
    }

    internal enum DFTI_ConfigValue
    {
        /// <summary>
        /// CommitStatus
        /// </summary>
        Committed = 30,
        Uncommitted = 31,
        
        /// <summary>
        /// ForwardDomain
        /// </summary>
        Complex = 32,
        Real = 33,

        /// <summary>
        /// Precision
        /// </summary>
        Single = 35,
        Double = 36,

        /// <summary>
        /// ComplexStorage and ConjugateEvenStorage
        /// </summary>
        ComplexComplex = 39,
        ComplexReal = 40,

        /// <summary>
        /// RealStorage
        /// </summary>
        RealComplex = 41,
        RealReal = 42,

        /// <summary>
        /// Placement
        /// </summary>
        Inplace = 43,
        NotInplace = 44,

        /// <summary>
        /// Ordering
        /// </summary>
        Ordered = 48,
        BackwardScrambled = 49,

        /// <summary>
        /// Allow/avoid certain usages
        /// </summary>
        Allow = 51,
        Avoid = 52,
        None = 53,

        /// <summary>
        /// PackedFormat
        /// </summary>
        CCSFormat = 54,
        PackFormat = 55,
        PermFormat = 56,
        CCEFormat = 57

    }
}
