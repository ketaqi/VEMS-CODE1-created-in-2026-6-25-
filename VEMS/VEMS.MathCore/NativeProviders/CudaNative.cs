using System.Security;
using System.Runtime.InteropServices;
using System.Numerics;

namespace VEMS.MathCore
{

    #region ======= CUDA Runtime =======

    #region ----- enums & structs -----

    /// <summary>
    /// cuda error information
    /// </summary>
    public enum CUDA_Error
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        Success = 0,
        InvalidValue = 1,
        MemoryAllocation = 2,
        InitializationError = 3,
        CudartUnloading = 4,
        ProfilerDisabled = 5,
        ProfilerNotInitialized = 6,
        ProfilerAlreadyStarted = 7,
        ProfilerAlreadyStopped = 8,
        InvalidConfiguration = 9,
        InvalidPitchValue = 12,
        InvalidSymbol = 13,
        InvalidHostPointer = 16,
        InvalidDevicePointer = 17,
        InvalidTexture = 18,
        InvalidTextureBinding = 19,
        InvalidChannelDescriptor = 20,
        InvalidMemcpyDirection = 21,
        AddressOfConstant = 22,
        TextureFetchFailed = 23,
        TextureNotBound = 24,
        SynchronizationError = 25,
        InvalidFilterSetting = 26,
        InvalidNormSetting = 27,
        MixedDeviceExecution = 28,
        NotYetImplemented = 31,
        MemoryValueTooLarge = 32,
        StubLibrary = 34,
        InsufficientDriver = 35,
        CallRequiresNewerDriver = 36,
        InvalidSurface = 37,
        DuplicateVariableName = 43,
        DuplicateTextureName = 44,
        DuplicateSurfaceName = 45,
        DevicesUnavailable = 46,
        IncompatibleDriverContext = 49,
        MissingConfiguration = 52,
        PriorLaunchFailure = 53,
        LaunchMaxDepthExceeded = 65,
        LaunchFileScopedTex = 66,
        LaunchFileScopedSurf = 67,
        SyncDepthExceeded = 68,
        LaunchPendingCountExceeded = 69,
        InvalidDeviceFunction = 98,
        NoDevice = 100,
        InvalidDevice = 101,
        DeviceNotLicensed = 102,
        SoftwareValidityNotEstablished = 103,
        StartupFailure = 127,
        InvalidKernelImage = 200,
        DeviceUninitialized = 201,
        MapBufferObjectFailed = 205,
        UnmapBufferObjectFailed = 206,
        ArrayIsMapped = 207,
        AlreadyMapped = 208,
        NoKernelImageForDevice = 209,
        AlreadyAcquired = 210,
        NotMapped = 211,
        NotMappedAsArray = 212,
        NotMappedAsPointer = 213,
        ECCUncorrectable = 214,
        UnsupportedLimit = 215,
        DeviceAlreadyInUse = 216,
        PeerAccessUnsupported = 217,
        InvalidPtx = 218,
        InvalidGraphicsContext = 219,
        NvlinkUncorrectable = 220,
        JitCompilerNotFound = 221,
        UnsupportedPtxVersion = 222,
        JitCompilationDisabled = 223,
        UnsupportedExecAffinity = 224,
        UnsupportedDevSideSync = 225,
        Contained = 226,
        InvalidSource = 300,
        FileNotFound = 301,
        SharedObjectSymbolNotFound = 302,
        SharedObjectInitFailed = 303,
        OperatingSystem = 304,
        InvalidResourceHandle = 400,
        IllegalState = 401,
        LossyQuery = 402,
        SymbolNotFound = 500,
        NotReady = 600,
        IllegalAddress = 700,
        LaunchOutOfResources = 701,
        LaunchTimeout = 702,
        LaunchIncompatibleTexturing = 703,
        PeerAccessAlreadyEnabled = 704,
        PeerAccessNotEnabled = 705,
        SetOnActiveProcess = 708,
        ContextIsDestroyed = 709,
        Assert = 710,
        TooManyPeers = 711,
        HostMemoryAlreadyRegistered = 712,
        HostMemoryNotRegistered = 713,
        HardwareStackError = 714,
        IllegalInstruction = 715,
        MisalignedAddress = 716,
        InvalidAddressSpace = 717,
        InvalidPc = 718,
        LaunchFailure = 719,
        CooperativeLaunchTooLarge = 720,
        TensorMemoryLeak = 721,
        NotPermitted = 800,
        NotSupported = 801,
        SystemNotReady = 802,
        SystemDriverMismatch = 803,
        CompatNotSupportedOnDevice = 804,
        rMpsConnectionFailed = 805,
        MpsRpcFailure = 806,
        MpsServerNotReady = 807,
        MpsMaxClientsReached = 808,
        MpsMaxConnectionsReached = 809,
        MpsClientTerminated = 810,
        CdpNotSupported = 811,
        CdpVersionMismatch = 812,
        StreamCaptureUnsupported = 900,
        StreamCaptureInvalidated = 901,
        StreamCaptureMerge = 902,
        StreamCaptureUnmatched = 903,
        StreamCaptureUnjoined = 904,
        StreamCaptureIsolation = 905,
        StreamCaptureImplicit = 906,
        CapturedEvent = 907,
        StreamCaptureWrongThread = 908,
        Timeout = 909,
        GraphExecUpdateFailure = 910,
        ExternalDevice = 911,
        InvalidClusterSize = 912,
        FunctionNotLoaded = 913,
        InvalidResourceType = 914,
        InvalidResourceConfiguration = 915,
        Unknown = 999,
        ApiFailureBase = 10000
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

    }

    /// <summary>
    /// CUDA device properties
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct CUDA_DeviceProp
    {

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
        public byte[] name; // char[256];
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] uuid; // char[16];
        //public CUuuid uuid;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst =8)]
        public byte[] luid; // char[8];
        public uint luidDeviceNodeMask; //unsigned int
        public ulong totalGlobalMem; // size_t
        public ulong sharedMemPerBlock; // size_t
        public int regsPerBlock; // int
        public int warpSize; // int
        public ulong memPitch; // size_t
        public int maxThreadsPerBlock; // int
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public int[] maxThreadsDim; // int[3]
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public int[] maxGridSize; // int[3]
        public int clockRate; // int
        public ulong totalConstMem; // size_t
        public int major; // int
        public int minor; // int
        public ulong textureAlignment; // size_t
        public ulong texturePitchAlignment; // size_t
        public int deviceOverlap; // int
        public int multiProcessorCount; // int
        public int kernelExecTimeoutEnabled; // int
        public int integrated; // int
        public int canMapHostMemory; // int
        public int computeMode; // int
        public int maxTexture1D; // int
        public int maxTexture1DMipmap; // int
        public int maxTexture1DLinear; // int
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public int[] maxTexture2D; // int[2]
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public int[] maxTexture2DMipmap; // int[2]
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public int[] maxTexture2DLinear; // int[3]
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public int[] maxTexture2DGather; // int[2]
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public int[] maxTexture3D; // int[3]
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public int[] maxTexture3DAlt; // int[3]
        public int maxTextureCubemap; // int
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public int[] maxTexture1DLayered; // int[2]
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public int[] maxTexture2DLayered; // int[3]
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public int[] maxTextureCubemapLayered; // int[2]
        public int maxSurface1D; // int
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public int[] maxSurface2D; // int[2]
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public int[] maxSurface3D; // int[3]
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public int[] maxSurface1DLayered; // int[2]
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public int[] maxSurface2DLayered; // int[3]
        public int maxSurfaceCubemap; // int
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public int[] maxSurfaceCubemapLayered; // int[2]
        public ulong surfaceAlignment; // size_t
        public int concurrentKernels; // int
        public int ECCEnabled; // int
        public int pciBusID; // int 
        public int pciDeviceID; // int
        public int pciDomainID; // int
        public int tccDriver; // int
        public int asyncEngineCount; // int
        public int unifiedAddressing; // int
        public int memoryClockRate; // int
        public int memoryBusWidth; // int
        public int l2CacheSize; // int
        public int persistingL2CacheMaxSize; // int
        public int maxThreadsPerMultiProcessor; // int
        public int streamPrioritiesSupported; // int
        public int globalL1CacheSupported; // int
        public int localL1CacheSupported; // int
        public ulong sharedMemPerMultiprocessor; // size_t
        public int regsPerMultiprocessor; // int 
        public int managedMemory; // int
        public int isMultiGpuBoard; // int
        public int multiGpuBoardGroupID; // int
        public int hostNativeAtomicSupported; // int
        public int singleToDoublePrecisionPerfRatio; // int
        public int pageableMemoryAccess; // int
        public int concurrentManagedAccess; // int 
        public int computePreemptionSupported; // int
        public int canUseHostPointerForRegisteredMem; // int
        public int cooperativeLaunch; // int
        public int cooperativeMultiDeviceLaunch; // int
        public ulong sharedMemPerBlockOptin; // size_t
        public int pageableMemoryAccessUsesHostPageTables; // int
        public int directManagedMemAccessFromHost; // int
        public int maxBlocksPerMultiProcessor; // int
        public int accessPolicyMaxWindowSize; // int
        public ulong reservedSharedMemPerBlock; // size_t
        public int hostRegisterSupported; // int
        public int sparseCudaArraySupported; // int
        public int hostRegisterReadOnlySupported; // int
        public int timelineSemaphoreInteropSupported; // int
        public int memoryPoolsSupported; // int
        public int gpuDirectRDMASupported; // int
        public uint gpuDirectRDMAFlushWritesOptions; // unsigned int
        public int gpuDirectRDMAWritesOrdering; // int
        public uint memoryPoolSupportedHandleTypes; // unsigned int
        public int deferredMappingCudaArraySupported; // int
        public int ipcEventSupported; // int
        public int clusterLaunch; // int
        public int unifiedFunctionPointers; // int
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 63)]
        public int[] reserved; // int[63]
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    
    }

    /// <summary>
    /// cuda memory copy kind
    /// </summary>
    public enum CUDA_MemcpyKind
    {
        /// <summary>
        /// from host to host
        /// </summary>
        HostToHost = 0,

        /// <summary>
        /// from host to device
        /// </summary>
        HostToDevice = 1,

        /// <summary>
        /// from device to host
        /// </summary>
        DeviceToHost = 2,

        /// <summary>
        /// from device to device
        /// </summary>
        DeviceToDevice = 3,

        /// <summary>
        /// default
        /// </summary>
        Default = 4
    }

    #endregion
    #region ----- CudaRTNative  -----

    /// <summary>
    /// Cuda Runtime
    /// </summary>
    [SuppressUnmanagedCodeSecurity]
    internal sealed unsafe class CudaRTNative
    {
        private CudaRTNative() { }

        private const string CudaRTDllName = "cudart64_12"; //"cudart64_110"; 
        //private const string CuMATHDll = "CuMath";

        #region ----- Device Management -----

        [DllImport(CudaRTDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // returns the number of devices with compute capability greater
        // or equal to 2.0 that are available for execution
        internal static extern CUDA_Error cudaGetDeviceCount(ref int count);


        [DllImport(CudaRTDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // sets device to be used for GPU executions
        internal static extern CUDA_Error cudaSetDevice(int device);


        [DllImport(CudaRTDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // returns which device is currently being used
        internal static extern CUDA_Error cudaGetDevice(ref int device);


        [DllImport(CudaRTDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // returns the properties of device
        internal static extern CUDA_Error cudaGetDeviceProperties(ref CUDA_DeviceProp prop, int device);


        [DllImport(CudaRTDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // explicitly destroys and cleans up all resources associated
        // with the current device in the current process
        internal static extern CUDA_Error cudaDeviceReset();


        [DllImport(CudaRTDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // blocks until the device has completed all preceding requested tasks
        internal static extern CUDA_Error cudaDeviceSynchronize();


        //[DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        //// Gets the flags for the current device
        //internal static extern CUDA_Error cudaGetDeviceFlags(uint* flags);


        //[DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        //// Sets flags to be used for device executions
        //internal static extern CUDA_Error cudaSetDeviceFlags(uint flags);


        //[DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        //// Set a list of devices that can be used for CUDA
        //internal static extern CUDA_Error cudaSetValidDevices(int* device_arr, int len);

        #endregion
        #region ----- More about Device -----

        //[DllImport(CuMATHDll, CallingConvention = CallingConvention.Cdecl,
        //    ExactSpelling = true, SetLastError = false)]
        //// gets device name for specific device index
        //internal static extern void GetDeviceName(int devIndex, char* devName);


        //[DllImport(CuMATHDll, CallingConvention = CallingConvention.Cdecl,
        //    ExactSpelling = true, SetLastError = false)]
        //// gets device total global memory in bytes
        //internal static extern long GetDeviceMem(int devIndex);

        //[DllImport(CuMATHDll, CallingConvention = CallingConvention.Cdecl,
        //    ExactSpelling = true, SetLastError = false)]
        //// gets device total global memory in kB
        //internal static extern long GetDeviceMemKB(int devIndex);

        //[DllImport(CuMATHDll, CallingConvention = CallingConvention.Cdecl,
        //    ExactSpelling = true, SetLastError = false)]
        //// gets device total global memory in MB
        //internal static extern long GetDeviceMemMB(int devIndex);


        //[DllImport(CuMATHDll, CallingConvention = CallingConvention.Cdecl,
        //    ExactSpelling = true, SetLastError = false)]
        //// gets device mumber of multiprocessors
        //internal static extern int GetDeviceMultiprocessors(int devIndex);


        //[DllImport(CuMATHDll, CallingConvention = CallingConvention.Cdecl,
        //    ExactSpelling = true, SetLastError = false)]
        //// gets device number of CUDA cores
        //internal static extern int GetDeviceCUDACores(int devIndex);


        //[DllImport(CuMATHDll, CallingConvention = CallingConvention.Cdecl,
        //    ExactSpelling = true, SetLastError = false)]
        //// gets device clock rate (in kilohertz)
        //internal static extern int GetDeviceClockRate(int devIndex);


        //[DllImport(CuMATHDll, CallingConvention = CallingConvention.Cdecl,
        //    ExactSpelling = true, SetLastError = false)]
        //// gets device memory clock rate (in kilohertz)
        //internal static extern int GetDeviceMemClockRate(int devIndex);


        //[DllImport(CuMATHDll, CallingConvention = CallingConvention.Cdecl,
        //    ExactSpelling = true, SetLastError = false)]
        //// gets device Single / Double precision ration
        //internal static extern int GetDeviceSingleDoubleRatio(int devIndex);

        #endregion
        #region ----- Thread Management -----

        //[DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
        //    ExactSpelling = true, SetLastError = false)]
        //// Exit and clean up from CUDA launches
        //internal static extern unsafe CudaStatus cudaThreadExit();
        
        #endregion
        #region ----- Memory Management -----
        
        [DllImport(CudaRTDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // allocates memory on the device
        internal static extern CUDA_Error cudaMalloc(ref IntPtr devPtr, long byteSize);


        [DllImport(CudaRTDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // copies data between host and device
        internal static extern CUDA_Error cudaMemcpy(IntPtr dst, IntPtr src,
            long byteSize, CUDA_MemcpyKind kind);


        [DllImport(CudaRTDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // Free memory on the device
        internal static extern CUDA_Error cudaFree(IntPtr devPtr);


        //[DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        //// frees memory
        //internal static extern CudaStatus MemFree(IntPtr dptr);

        #endregion

    }

    #endregion

    #endregion
    #region ======= CuBLAS =======

    #region ----- enums & structs ----- 

    /// <summary>
    /// CuBLAS Status option
    /// </summary>
    public enum CuBLAS_Status
    {
        /// <summary>
        /// successful
        /// </summary>
		Success = 0,

        /// <summary>
        /// not initialized
        /// </summary>
		NotInitialized = 1,

        /// <summary>
        /// allocation failed
        /// </summary>
		AllocFailed = 3,

        /// <summary>
        /// invalid value(s)
        /// </summary>
		InvalidValue = 7,

        /// <summary>
        /// architechture mismatch
        /// </summary>
		ArchMismatch = 8,

        /// <summary>
        /// mapping error
        /// </summary>
		MappingError = 11,

        /// <summary>
        /// execution failed
        /// </summary>
		ExecutionFailed = 13,

        /// <summary>
        /// internal error
        /// </summary>
		InternalError = 14,

        /// <summary>
        /// not supported
        /// </summary>
		NotSupported = 15,

        /// <summary>
        /// license error
        /// </summary>
		LicenseError = 16,
    }

    /// <summary>
    /// cuBLAS fill mode
    /// </summary>
    internal enum CuBLAS_FillMode
    {
        Lower = 0,
        Upper = 1,
        Full = 2,
    }

    internal enum CuBLAS_DiagType
    {
        NonUnit = 0,
        Unit = 1,
    }

    internal enum CuBLAS_SideMode
    {
        Left = 0,
        Right = 1,
    }

    /// <summary>
    /// matrix transpose options
    /// </summary>
	public enum CuBLAS_Operation
    {
        /// <summary>
        /// no transpose
        /// </summary>
		NoTrans = 0,

        /// <summary>
        /// transpose
        /// </summary>
		Trans = 1,

        /// <summary>
        /// conjugate transpose
        /// </summary>
		ConjTrans = 2,

        ///// <summary>
        ///// sunonym if CUBLAS_OP_C
        ///// </summary>
        //Hermitan = 2

        ///// <summary>
        ///// conjugate, placeholder - not supported in the current release
        ///// </summary>
        //Conjg = 3
    }

    internal enum CuBLAS_PointerMode
    {
        Host = 0,
        Device = 1,
    }

    internal enum CuBLAS_AtomicsMode
    {
        NotAllowed = 0,
        Allowed = 1,
    }

    // GemmAlgo

    internal enum CuBLAS_Math
    {
        DefaultMath = 0,
        TensorOpMath = 1,
        PedanticMath = 2,
        TF32TensorOpMath = 3,
        PrecisionReduction = 16,
    }

    internal enum CuBLAS_ComputeType
    {
        Compute16F = 64,
        Compute16FPedantic = 65,
        Compute32F = 68,
        Compute32FPedantic = 69,
        Compute32FFast16F = 74,
        Compute32FFast16BF = 75,
        Compute32FFastTF32 = 77,
        Compute64F = 70,
        Compute64FPedantic = 71,
        Compute32I = 72,
        Compute32IPedantic = 73,
    }

    #endregion
    #region ----- CuBLAS Native -----

    [SuppressUnmanagedCodeSecurity]
    internal sealed unsafe class CuBLASNative
    {
        private CuBLASNative() { }

        private const string CuBLASDllName = "cublas64_12"; //"cublas64_11"; 

        #region ----- support -----

        [DllImport(CuBLASDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // initializes the cuBLAS library and creates a handle to an opaque structure
        // holding the cuBLAS library context
        internal static extern CuBLAS_Status cublasCreate_v2(ref IntPtr handle);


        [DllImport(CuBLASDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // releases hardware resources used by the cuBLAS library
        internal static extern CuBLAS_Status cublasDestroy_v2(IntPtr handle);


        [DllImport(CuBLASDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // returns the version number of the cuBLAS library
        internal static extern CuBLAS_Status cublasGetVersion_v2(IntPtr handle, ref int version);


        [DllImport(CuBLASDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // copies n elements from a vector x in host memory space 
        // to a vector y in GPU memory space
        internal static extern CuBLAS_Status cublasSetVector_64(long n, long elemSize,
            [In] void* x, long incx, [Out] void* y, long incy);


        [DllImport(CuBLASDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // copies n elements from a vector x in GPU memory space 
        // to a vector y in host memory spac
        internal static extern CuBLAS_Status cublasGetVector_64(long n, long elemSize,
            [In] void* x, long incx, [Out] void* y, long incy);


        [DllImport(CuBLASDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // copies a tile of rows x cols elements from a matrix A in host memory space 
        // to a matrix B in GPU memory space
        internal static extern CuBLAS_Status cublasSetMatrix_64(long rows, long cols, long elemSize,
            [In] void* a, long lda, [Out] void* b, long ldb);


        [DllImport(CuBLASDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // copies a tile of rows x cols elements from a matrix A in GPU memory space 
        // to a matrix B in host memory space
        internal static extern CuBLAS_Status cublasGetMatrix_64(long rows, long cols, long elemSize,
            [In] void* a, long lda, [Out] void* b, long ldb);

        #endregion
        #region ----- Dasum, Zasum -----

        [DllImport(CuBLASDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes the sum of the absolute values of the elements of vector x
        internal static extern CuBLAS_Status cublasDasum_v2_64(IntPtr handle,
            long n, [In] void* x, long incx,
            ref double result); // host or device pointer


        [DllImport(CuBLASDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes the sum of the absolute values of the elements of vector x
        internal static extern CuBLAS_Status cublasDzasum_v2_64(IntPtr handle,
            long n, [In] void* x, long incx,
            ref double result); // host or device pointer

        #endregion
        #region ----- Daxpy, Zaxpy -----

        [DllImport(CuBLASDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // multiplies the vector x by the scalar alpha
        // and adds it to the vector y overwriting the latest vector with the result
        internal static extern CuBLAS_Status cublasDaxpy_v2_64(IntPtr handle,
            long n,
            [In] ref double alpha, // host or device pointer
            [In] void* x, long incx,
            [In, Out] void* y, long incy);


        [DllImport(CuBLASDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // multiplies the vector x by the scalar alpha
        // and adds it to the vector y overwriting the latest vector with the result
        internal static extern CuBLAS_Status cublasZaxpy_v2_64(IntPtr handle,
            long n,
            [In] ref Complex alpha, // host or device pointer
            [In] void* x, long incx,
            [In, Out] void* y, long incy);

        #endregion
        #region ----- Dcopy, Zcopy -----

        [DllImport(CuBLASDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // copies the vector x into the vector y
        internal static extern CuBLAS_Status cublasDcopy_v2_64(IntPtr handle,
            long n, [In] void* x, long incx,
            [Out] void* y, long incy);


        [DllImport(CuBLASDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // copies the vector x into the vector y
        internal static extern CuBLAS_Status cublasZcopy_v2_64(IntPtr handle,
            long n, [In] void* x, long incx,
            [Out] void* y, long incy);

        #endregion
        #region ----- Ddot, Zdotu, Zdotc -----

        [DllImport(CuBLASDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes the dot product of vectors x and y
        internal static extern CuBLAS_Status cublasDdot_v2_64(IntPtr handle,
            long n, [In] void* x, long incx,
            [In] void* y, long incy,
            ref double result);  // host or device pointer


        [DllImport(CuBLASDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes the dot product of vectors x and y
        public static extern CuBLAS_Status cublasZdotu_v2_64(IntPtr handle,
            long n, [In] void* x, long incx,
            [In] void* y, long incy,
            ref Complex result); // host or device pointer


        [DllImport(CuBLASDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes the dot product of vectors x and y
        public static extern CuBLAS_Status cublasZdotc_v2_64(IntPtr handle,
            long n, [In] void* x, long incx,
            [In] void* y, long incy,
            ref Complex result); // host or device pointer

        #endregion
        #region ----- Dnrm2, Dznrm2 -----

        [DllImport(CuBLASDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes the Euclidean norm of the vector x
        internal static extern CuBLAS_Status cublasDnrm2_v2_64(IntPtr handle,
            long n, [In] void* x, long incx,
            ref double result);  // host or device pointer


        [DllImport(CuBLASDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // computes the Euclidean norm of the vector x
        public static extern CuBLAS_Status cublasDznrm2_v2_64(IntPtr handle,
            long n, [In] void* x, long incx,
            ref double result);  // host or device pointer

        #endregion
        #region ----- Drot, Zrot, Zdrot -----

        [DllImport(CuBLASDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // applies Givens rotation matrix to vectorx x and y by angle c and s
        internal static extern CuBLAS_Status cublasDrot_v2_64(IntPtr handle,
            long n,
            [In, Out] void* x, long incx,
            [In, Out] void* y, long incy,
            [In] ref double c,  // host or device pointer
            [In] ref double s); // host or device pointer


        [DllImport(CuBLASDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // applies Givens rotation matrix to vectorx x and y by angle c and s
        internal static extern CuBLAS_Status cublasZrot_v2_64(IntPtr handle,
            long n,
            [In, Out] void* x, long incx,
            [In, Out] void* y, long incy,
            [In] ref double c,   // host or device pointer
            [In] ref Complex s); // host or device pointer


        [DllImport(CuBLASDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // applies Givens rotation matrix to vectorx x and y by angle c and s
        internal static extern CuBLAS_Status cublasZdrot_v2_64(IntPtr handle,
            long n,
            [In, Out] void* x, long incx,
            [In, Out] void* y, long incy,
            [In] ref double c,  // host or device pointer
            [In] ref double s); // host or device pointer

        #endregion
        #region ----- Drotg, Zrotg -----

        //[DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
        //	ExactSpelling = true, SetLastError = false)]
        //// constructs the Givens rotation matrix
        //internal static extern CublasStatus cublasDrotg_v2(IntPtr handle,
        //	[In, Out] double* a, [In, Out] double* b,
        //	[Out] double* c, [Out] double* s);

        //[DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
        //	ExactSpelling = true, SetLastError = false)]
        //// constructs the Givens rotation matrix
        //internal static extern CublasStatus cublasZrotg_v2(IntPtr handle,
        //	[In, Out] Complex* a, [In, Out] Complex* b,
        //	[Out] double* c, [Out] Complex* s);

        #endregion
        #region ----- Dscal, Zscal, Zdscal -----

        [DllImport(CuBLASDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // scales the vector x by the scalar α and overwrites it with the result
        internal static extern CuBLAS_Status cublasDscal_v2_64(IntPtr handle,
            long n,
            [In] ref double alpha, // host or device pointer
            [In, Out] void* x, long incx);


        [DllImport(CuBLASDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // scales the vector x by the scalar α and overwrites it with the result
        internal static extern CuBLAS_Status cublasZscal_v2_64(IntPtr handle,
            long n,
            [In] ref Complex alpha, // host or device pointer
            [In, Out] void* x, long incx);


        [DllImport(CuBLASDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // scales the vector x by the scalar α and overwrites it with the result
        internal static extern CuBLAS_Status cublasZdscal_v2_64(IntPtr handle,
            long n,
            [In] ref double alpha, // host or device pointer
            [In, Out] void* x, long incx);

        #endregion
        #region ----- Dswap, Zswap -----

        [DllImport(CuBLASDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // interchanges the elements of vector x and y
        internal static extern CuBLAS_Status cublasDswap_v2_64(IntPtr handle,
            long n,
            [In, Out] void* x, long incx,
            [In, Out] void* y, long incy);


        [DllImport(CuBLASDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // interchanges the elements of vector x and y
        internal static extern CuBLAS_Status cublasZswap_v2_64(IntPtr handle,
            long n,
            [In, Out] void* x, long incx,
            [In, Out] void* y, long incy);

        #endregion
        #region ----- Idamax, Izamax, Idamin, Izamax -----

        [DllImport(CuBLASDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // finds the (smallest) index of the element of the maximum magnitude
        internal static extern CuBLAS_Status cublasIdamax_v2_64(IntPtr handle,
            long n,
            [In] void* x, long incx,
            ref long result); // host or device pointer


        [DllImport(CuBLASDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // finds the (smallest) index of the element of the maximum magnitude
        internal static extern CuBLAS_Status cublasIzamax_v2_64(IntPtr handle,
            long n,
            [In] void* x, long incx,
            ref long result); // host or device pointer


        [DllImport(CuBLASDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // finds the (smallest) index of the element of the minimum magnitude
        internal static extern CuBLAS_Status cublasIdamin_v2_64(IntPtr handle,
            long n,
            [In] void* x, long incx,
            ref long result); // host or device pointer


        [DllImport(CuBLASDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // finds the (smallest) index of the element of the minimum magnitude
        internal static extern CuBLAS_Status cublasIzamin_v2(IntPtr handle,
            long n,
            [In] void* x, long incx,
            ref long result);

        #endregion
        #region ----- Dgemv, Zgemv -----

        [DllImport(CuBLASDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // performs the matrix-vector multiplication
        internal static extern CuBLAS_Status cublasDgemv_v2_64(IntPtr handle,
            CuBLAS_Operation trans,
            long m, long n,
            [In] ref double alpha, // host or device pointer
            [In] void* a, long lda,
            [In] void* x, long incx,
            [In] ref double beta, // host or device pointer
            [In, Out] void* y, long incy);


        [DllImport(CuBLASDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // performs the matrix-vector multiplication
        internal static extern CuBLAS_Status cublasZgemv_v2_64(IntPtr handle,
            CuBLAS_Operation trans,
            long m, long n,
            [In] ref void* alpha, // host or device pointer
            [In] void* a, long lda,
            [In] void* x, long incx,
            [In] ref void* beta, // host or device pointer
            [In, Out] void* y, long incy);

        #endregion
        #region ----- Dgemm, Zgemm -----

        [DllImport(CuBLASDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // performs the matrix-matrix multiplication
        internal static extern CuBLAS_Status cublasDgemm_v2_64(IntPtr handle,
            CuBLAS_Operation transa, CuBLAS_Operation transb,
            long m, long n, long k,
            [In] ref double alpha, //host or device pointer  
            [In] void* a, long lda,
            [In] void* b, long ldb,
            [In] ref double beta, //host or device pointer  
            [In, Out] void* c, long ldc);


        [DllImport(CuBLASDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // performs the matrix-matrix multiplication
        internal static extern CuBLAS_Status cublasZgemm3m_64(IntPtr handle,
            CuBLAS_Operation transa, CuBLAS_Operation transb,
            long m, long n, long k,
            [In] ref void* alpha, /* host or device pointer */
            [In] void* a, long lda,
            [In] void* b, long ldb,
            [In] ref void* beta, /* host or device pointer */
            [In, Out] void* c, long ldc);

        #endregion

    }

    #endregion

    #endregion
    #region ======= CuFFT =======

    #region ----- enums & structs -----

    /// <summary>
    /// CuFFT function reture values
    /// </summary>
    public enum CuFFT_Result
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        Success = 0,
        InvalidPlan = 1,
        AllocFailed = 2,
        InvalidType = 3,
        InvalidValue = 4,
        InternalError = 5,
        ExecFailed = 6,
        SetupFailed = 7,
        InvalidSize = 8,
        UnalignedData = 9,
        IncompleteParameterList = 10,
        InvalidDevice = 11,
        ParseError = 12,
        NoWorkspace = 13,
        NotImplemented = 14,
        LicenseError = 15,
        NotSupported = 16
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

    }

    /// <summary>
    /// CuFFT transform directions
    /// </summary>
    public enum CuFFT_Direction
    {
        /// <summary>
        /// forward
        /// </summary>
        Forward = -1,

        /// <summary>
        /// inverse
        /// </summary>
        Inverse = 1
    }

    /// <summary>
    /// CuFFT transform types
    /// </summary>
    public enum CuFFT_Type
    {
        /// <summary>
        /// real to complex (interleaved)
        /// </summary>
        R2C = 42,

        /// <summary>
        /// complex (interleaved) to real
        /// </summary>
        C2R = 44,

        /// <summary>
        /// complex to complex, interleaved
        /// </summary>
        C2C = 41,

        /// <summary>
        /// double to double complex
        /// </summary>
        D2Z = 106,

        /// <summary>
        /// double complex to double
        /// </summary>
        Z2D = 108,

        /// <summary>
        /// double complex to double complex
        /// </summary>
        Z2Z = 105
    }

    #endregion
    #region ----- CuFFTNative -----

    [SuppressUnmanagedCodeSecurity]
    internal sealed unsafe class CuFFTNative
    {
        private CuFFTNative() { }

        private const string CuFFTDllName = "cufft64_11";

        #region ----- support -----

        [DllImport(CuFFTDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // returns the version number of the cuFFT library
        internal static extern CuFFT_Result cufftGetVersion(ref int version);

        #endregion
        #region ----- plan -----

        [DllImport(CuFFTDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // creates a 1D FFT plan configuration
        internal static extern CuFFT_Result cufftPlan1d(ref IntPtr plan, 
            long nx, CuFFT_Type type, long batch);


        [DllImport(CuFFTDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // creates a 2D FFT plan configuration
        internal static extern CuFFT_Result cufftPlan2d(ref IntPtr plan, 
            long nx, long ny, CuFFT_Type type);


        [DllImport(CuFFTDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // creates a 3D FFT plan configuration
        internal static extern CuFFT_Result cufftPlan3d(ref IntPtr plan, 
            long nx, long ny, long nz, CuFFT_Type type);


        [DllImport(CuFFTDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // creates a plan configuration
        internal static extern CuFFT_Result cufftPlanMany(ref IntPtr plan, 
            long rank, long* n, 
            long* inembed, long istride, long idist, 
            long* onembed, long ostride, long odist, 
            CuFFT_Type type, long batch);


        [DllImport(CuFFTDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // destroys a cuFFT plan
        internal static extern CuFFT_Result cufftDestroy(IntPtr plan);

        #endregion
        #region ----- estimate -----

        [DllImport(CuFFTDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // estimates the work size of work area (1D)
        internal static extern CuFFT_Result cufftEstimate1d(long nx, 
            CuFFT_Type type, long batch, ref long workSize);


        [DllImport(CuFFTDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // estimates the work size of work area (2D)
        internal static extern CuFFT_Result cufftEstimate2d(long nx, long ny,
            CuFFT_Type type, ref long workSize);


        [DllImport(CuFFTDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // estimates the work size of work area (3D)
        internal static extern CuFFT_Result cufftEstimate3d(long nx, long ny, long nz,
            CuFFT_Type type, ref long workSize);


        [DllImport(CuFFTDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // estimates the work size of work area
        internal static extern CuFFT_Result cufftEstimateMany(long rank, long* n,
            long* inembed, long istride, long idist,
            long* onembed, long ostride, long odist,
            CuFFT_Type type, long batch, ref long workSize);

        #endregion
        #region ----- execution -----

        [DllImport(CuFFTDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // executes a single-precision complex-to-complex FFT
        internal static extern CuFFT_Result cufftExecC2C(IntPtr plan,
            Complex* idata, Complex* odata, CuFFT_Direction direction);


        [DllImport(CuFFTDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // executes a single-precision real-to-complex FFT
        internal static extern CuFFT_Result cufftExecR2C(IntPtr plan,
            float* idata, Complex* odata);


        [DllImport(CuFFTDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // executes a single-precision complex-to-real FFT
        internal static extern CuFFT_Result cufftExecC2R(IntPtr plan,
            Complex* idata, float* odata);


        [DllImport(CuFFTDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // executes a double-precision complex-to-complex FFT
        internal static extern CuFFT_Result cufftExecZ2Z(IntPtr plan,
            Complex* idata, Complex* odata, CuFFT_Direction direction);


        [DllImport(CuFFTDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // executes a double-precision real-to-complex FFT
        internal static extern CuFFT_Result cufftExecD2Z(IntPtr plan,
            double* idata, Complex* odata);


        [DllImport(CuFFTDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        // executes a double-precision complex-to-real FFT
        internal static extern CuFFT_Result cufftExecZ2D(IntPtr plan,
            Complex* idata, double* odata);

        #endregion
    }


    #endregion

    #endregion
    #region ======= CuSOLVER =======

    // ...

    #endregion
    #region ======= CuRAND =======

    // ...

    #endregion
}