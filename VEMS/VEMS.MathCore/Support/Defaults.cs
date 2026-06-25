namespace VEMS.MathCore
{
    /// <summary>
    /// default parameters MathCore
    /// </summary>
    public static class Defaults
    {
        #region number format

        /// <summary>
        /// culture option
        /// </summary>
        public const CultureName Culture = CultureName.zh;

        /// <summary>
        /// number format
        /// </summary>
        public const NumericFormat NumberFormat = NumericFormat.Number;

        /// <summary>
        /// number of digits
        /// </summary>
        public const int NumberOfDigits = 3;

        /// <summary>
        /// representation format of complex number
        /// </summary>
        public const ComplexFormat ComplexNumberFormat = ComplexFormat.RealAndImaginary;

        /// <summary>
        /// character used for column separation
        /// </summary>
        public const CharSeparator ColumnSeparator = CharSeparator.Tab;

        #endregion
        #region performance

        /// <summary>
        /// number of physical cores of CPU
        /// </summary>
        public static int NumberOfCores = 3;

        /// <summary>
        /// mode for the loop computation
        /// </summary>
        public const LoopMode LoopOption = LoopMode.Sequential;

        /// <summary>
        /// whether enabling linear fraction to accelerate certain calculations
        /// true: faster but lower accuracy; false: slower but higher accuracy
        /// </summary>
        public const bool EnableLinearFrac = true;

        #endregion
        #region import export

        /// <summary>
        /// chunk size for data import/export
        /// </summary>
        public const int ImExportChunkSize = 4096;

        #endregion
        #region math convention
        /// <summary>
        /// convention of Euler angles
        /// </summary>
        public const EulerAnglesCovention EulerAnglesConvention = EulerAnglesCovention.XConvention;
        #endregion
        #region finite-difference

        /// <summary>
        /// finite difference 1D option
        /// </summary>
        public const FiDi1DOption FD1DOption = FiDi1DOption.Dt;

        /// <summary>
        /// finite difference 2D option 
        /// </summary>
        public const FiDi2DOption FD2DOption = FiDi2DOption.Dx;

        #endregion
        #region interpolation/fitting

        /// <summary>
        /// interpolation method for grid data
        /// </summary>
        public const InterpolationMethod IntrplOption = InterpolationMethod.Nearest;

        /// <summary>
        /// data boundary option
        /// </summary>
        public const DataBoundary BoundaryOption = DataBoundary.ConstantZero;

        #endregion
        #region Intel MKL 

        /// <summary>
        /// default Intel MKL interface: ILP64
        /// </summary>
        public const MKL_Interface mkl_Interface = MKL_Interface.ILP64;

        ///// <summary>
        ///// Intel MKL Interface Option: LP64
        ///// </summary>
        //public const int MKL_Interface_LP64 =  0x0;

        ///// <summary>
        ///// Intel MKL Interface Option: ILP64
        ///// </summary>
        //public const int MKL_Interface_ILP64 = 0x1;

        ///// <summary>
        ///// Intel MKL Interface Option: GNU
        ///// </summary>
        //public const int MKL_Interface_GNU = 0x2;


        private static IntelMKL intelMKL = new();
        private static readonly IntelMKL.BLAS mklBLAS = new();
        private static readonly IntelMKL.SPBLAS mklSPBLAS = new();
        private static readonly IntelMKL.LAPACK mklLAPACK = new();
        private static readonly IntelMKL.VMF mklVMF = new();
        //private static AmdAOCL amdAOCL = new();
        //private static OpenBLAS openBLAS = new();
        //private static NvidiaCUDA nvidiaCUDA = new();

        /// <summary>
        /// default BLAS interface: IntelMKL
        /// </summary>
        public static IBLAS IBLAS = mklBLAS;

        /// <summary>
        /// default BLAS interface: IntelMKL
        /// </summary>
        public static ISPBLAS ISPBLAS = mklSPBLAS;

        /// <summary>
        /// default LAPACL interface: Intel MKL
        /// </summary>
        public static ILAPACK ILAPACK = mklLAPACK;

        /// <summary>
        /// default VMF interface: IntelMKL
        /// </summary>
        public static IVMF IVMF = mklVMF; 

        /// <summary>
        /// default FFT interface: IntelMKL
        /// </summary>
        public static IFFT IFFT = intelMKL;

        /// <summary>
        /// default VSL interface: IntelMKL
        /// </summary>
        public static IVSL IVSL = intelMKL;

        #endregion

    }

    /// <summary>
    /// user-selected parameter
    /// </summary>
    public static class MathSettings
    {
        #region number format

        /// <summary>
        /// culture info name
        /// </summary>
        public static CultureName Culture = CultureName.zh;

        /// <summary>
        /// number format
        /// </summary>
        public static NumericFormat NumberFormat = NumericFormat.Number;

        /// <summary>
        /// number of digits
        /// </summary>
        public static int NumberOfDigits = 3;

        /// <summary>
        /// representation format of complex number
        /// </summary>
        public static ComplexFormat ComplexNumberFormat = ComplexFormat.RealAndImaginary;

        /// <summary>
        /// character used for column separation 
        /// </summary>
        public static CharSeparator ColumnSeparator = CharSeparator.Tab;

        #endregion
        #region performance

        /// <summary>
        /// number of physical cores of CPU
        /// </summary>
        public static int NumberOfCores = 4;

        #endregion
    }

    /// <summary>
    /// conventions of Euler angles
    /// </summary>
    public enum EulerAnglesCovention
    {
        /// <summary>
        /// x convention
        /// the first rotation by angle phi is about the z-axis
        /// the second rotation by angle theta is about the new x`-axis
        /// the third rotation by angle psi is about the new z`-axis
        /// </summary>
        XConvention,

        /// <summary>
        /// Not implemented yet
        /// </summary>
        YConvention,

        /// <summary>
        /// Not implemented yet
        /// </summary>
        XYZConvention,
    }

    /// <summary>
    /// character separator
    /// </summary>
    public enum CharSeparator
    {
        /// <summary>
        /// tab 
        /// </summary>
        Tab,

        /// <summary>
        /// comma
        /// </summary>
        Comma,

        /// <summary>
        /// space
        /// </summary>
        Space,

        /// <summary>
        /// semicolon
        /// </summary>
        SemiColon
    }

    /// <summary>
    /// culture info name 
    /// </summary>
    public enum CultureName
    {
        /// <summary>
        /// Arabic
        /// </summary>
        ar,

        /// <summary>
        /// Bulgarian
        /// </summary>
        bg,

        /// <summary>
        /// Catalan
        /// </summary>
        ca,

        /// <summary>
        /// Czech
        /// </summary>
        cs,

        /// <summary>
        /// Danish
        /// </summary>
        da,

        /// <summary>
        /// German
        /// </summary>
        de,

        /// <summary>
        /// Greek
        /// </summary>
        el,

        /// <summary>
        /// English
        /// </summary>
        en,

        /// <summary>
        /// Spanish
        /// </summary>
        es,

        /// <summary>
        /// Finnish
        /// </summary>
        fi,

        /// <summary>
        /// Chinese
        /// </summary>
        zh,

        ///// <summary>
        ///// Chinese (Simplified)
        ///// </summary>
        //zh_Hans,

        ///// <summary>
        ///// Chinese (Traditional)
        ///// </summary>
        //zh_Hant,
    }

    /// <summary>
    /// format of number
    /// </summary>
    public enum NumericFormat
    {

        /// <summary>
        /// currency value
        /// </summary>
        Currency,

        /// <summary>
        /// exponential notation (scientific)
        /// </summary>
        Exponential,

        /// <summary>
        /// fixed decimal notation
        /// </summary>
        FixedPoint,

        /// <summary>
        /// compact format of either fixed-point 
        /// or scientific notation
        /// </summary>
        General,

        /// <summary>
        /// number group separators notation
        /// </summary>
        Number,

        /// <summary>
        /// multiplied by 100 and displayed with 
        /// a percent symbol
        /// </summary>
        Percent
    }

    /// <summary>
    /// complex number representation format
    /// </summary>
    public enum ComplexFormat
    {
        /// <summary>
        /// represent complex number in its
        /// real- and imag-parts
        /// </summary>
        RealAndImaginary = 0,

        /// <summary>
        /// represent complex number in its
        /// magnitude and argument
        /// </summary>
        MagnitudeAndPhase = 1
    }

    /// <summary>
    /// complex part options
    /// </summary>
    public enum ComplexPart
    {
        /// <summary>
        /// real-part
        /// </summary>
        RealPart = 00,
        /// <summary>
        /// imaginary part
        /// </summary>
        ImagPart = 01,
        /// <summary>
        /// magnitude of data
        /// </summary>
        Magnitude = 02,
        /// <summary>
        /// argument of data
        /// </summary>
        Argument = 03
    }

    /// <summary>
    /// loop-computational mode options
    /// </summary>
    public enum LoopMode
    {
        /// <summary>
        /// using sequential loop
        /// </summary>
        Sequential,

        /// <summary>
        /// using parallel loop
        /// </summary>
        Parallel,

        /// <summary>
        /// using vectorized math
        /// </summary>
        Vectorized
    }

    /// <summary>
    /// options for interpolation method
    /// </summary>
    public enum InterpolationMethod
    {
        /// <summary>
        /// sinc interpolation 
        /// </summary>
        Sinc,

        /// <summary>
        /// sinc-FFT interpolation
        /// </summary>
        SincFFT,

        /// <summary>
        /// nearest interpolation
        /// </summary>
        Nearest,

        /// <summary>
        /// (bi)-linear interpolation
        /// </summary>
        Linear,

        /// <summary>
        /// (bi)-cubic interpolation
        /// </summary>
        Cubic
    }

    /// <summary>
    /// options for 1D finite difference
    /// </summary>
    public enum FiDi1DOption
    {
        /// <summary>
        /// first-order derivative
        /// </summary>
        Dt,
        /// <summary>
        /// second-order derivative
        /// </summary>
        Dtt
    }

    /// <summary>
    /// options for 2D finite difference
    /// </summary>
    public enum FiDi2DOption
    {
        /// <summary>
        /// first-order derivative along x
        /// </summary>
        Dx,
        /// <summary>
        /// first-order derivative along y
        /// </summary>
        Dy,
        /// <summary>
        /// second-order derivative along x
        /// </summary>
        Dxx,
        /// <summary>
        /// second-order derivative along y
        /// </summary>
        Dyy,
        /// <summary>
        /// cross derivative first along x and then along y
        /// </summary>
        Dxy
    }



    /// <summary>
    /// options for fast Fourier transform (FFT)
    /// </summary>
    public enum FFTOption
    {
        /// <summary>
        /// forward transform
        /// </summary>
        Forward,

        /// <summary>
        /// backward transform
        /// </summary>
        Backward,

        ///// <summary>
        ///// forward raw transform
        ///// </summary>
        //ForwardRaw = 10,

        ///// <summary>
        ///// backward raw transform
        ///// </summary>
        //BackwardRaw = 11
    }

    /// <summary>
    /// options for discrete Fourier transform (DFT)
    /// </summary>
    public enum DFTOption
    {
        /// <summary>
        /// forward transform
        /// </summary>
        Forward = 0,

        /// <summary>
        /// backward transform
        /// </summary>
        Backward = 1
    }

    /// <summary>
    /// options for handling of values beyond data range
    /// </summary>
    public enum DataBoundary
    {
        /// <summary>
        /// constant zero beyond data range
        /// </summary>
        ConstantZero,

        /// <summary>
        /// periodic replicated beyond data range
        /// </summary>
        Periodic,

        /// <summary>
        /// Represents a constant value that does not change during the execution of the program.
        /// </summary>
        /// <remarks>This class or member is typically used to define immutable values that are shared
        /// across multiple parts of an application. Constant values are often used for configuration, default settings,
        /// or fixed data.</remarks>
        ConstantValue, 

        // ...
    }

}
