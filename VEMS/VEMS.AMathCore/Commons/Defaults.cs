namespace VEMS.AMathCore
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


        private static IntelMKL intelMKL = new();
        private static readonly IntelMKL.BLAS mklBLAS = new();
        private static readonly IntelMKL.SPBLAS mklSPBLAS = new();
        private static readonly IntelMKL.LAPACK mklLAPACK = new();
        private static readonly IntelMKL.VMF mklVMF = new();
        private static readonly IntelMKL.DFTI mklDFFI = new();
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
        public static IFFT IFFT = mklDFFI;

        ///// <summary>
        ///// default VSL interface: IntelMKL
        ///// </summary>
        //public static IVSL IVSL = intelMKL;

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

}
