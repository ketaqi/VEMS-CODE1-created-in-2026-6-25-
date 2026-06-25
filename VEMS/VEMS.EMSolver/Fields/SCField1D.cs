using System.Diagnostics.CodeAnalysis;
using VEMS.MathCore;
using Complex = System.Numerics.Complex;

namespace VEMS.EMSolver
{

    /// <summary>
    /// Represents a 1D scalar electromagnetic field that is invariant along the y-direction.
    /// This class provides methods and properties for modeling, sampling, transforming, and propagating
    /// scalar fields in one dimension, supporting both spatial and spatial-frequency domains.
    /// </summary>
    public class SCField1D : IField
    {
        #region interface properties

        /// <summary>
        /// Gets or sets the wavelength in vacuum.
        /// </summary>
        public double Wavelength { get; set; }

        /// <summary>
        /// Gets the wavenumber in vacuum.
        /// <para>k0 = 2.0 * PI / wavelength</para>
        /// </summary>
        public double K0 { get => 2.0 * Math.PI / Wavelength; }

        /// <summary>
        /// Gets or sets the embedding material of the field.
        /// </summary>
        public Material Material { get; set; }

        /// <summary>
        /// Gets or sets the modeling domain (spatial or spatial-frequency).
        /// </summary>
        public ModelingDomain Domain { get; set; }

        /// <summary>
        /// Gets or sets the overall scaling factor of the field.
        /// </summary>
        public Complex Scaling { get; set; } = Complex.One;

        #endregion
        #region properties

        /// <summary>
        /// Gets or sets the lateral shift of the field in the spatial domain.
        /// </summary>
        public double ShiftX
        {
            get
            {
                switch (Domain)
                {
                    case ModelingDomain.Spatial:
                        return UGrid?.Center ?? 0.0;
                    case ModelingDomain.SpatialFrequency:
                        return -UPhase?.C1 ?? 0.0;
                    default: goto case ModelingDomain.Spatial;
                }
            }
            set
            {
                switch (Domain)
                {
                    case ModelingDomain.Spatial:
                        if (UGrid == null) { throw new ArgumentNullException(nameof(UGrid)); }
                        if (UGrid.Center != value) { UGrid.Center = value; }
                        break;
                    case ModelingDomain.SpatialFrequency:
                        if (UPhase == null) { throw new ArgumentNullException(nameof(UPhase)); }
                        if (UPhase.C1 != -value) { UPhase.C1 = -value; }
                        break;
                    default: goto case ModelingDomain.Spatial;
                }
            }
        }

        /// <summary>
        /// Gets or sets the lateral shift of the field in the spatial frequency domain,
        /// i.e., the linear phase in the spatial domain.
        /// </summary>
        public double ShiftKx
        {
            get
            {
                switch (Domain)
                {
                    case ModelingDomain.Spatial:
                        return UPhase?.C1 ?? 0.0;
                    case ModelingDomain.SpatialFrequency:
                        return UGrid?.Center ?? 0.0;
                    default: goto case ModelingDomain.Spatial;
                }
            }
            set
            {
                switch (Domain)
                {
                    case ModelingDomain.Spatial:
                        if (UPhase == null) { throw new ArgumentNullException(nameof(UPhase)); }
                        if (UPhase.C1 != value) { UPhase.C1 = value; }
                        break;
                    case ModelingDomain.SpatialFrequency:
                        if (UGrid == null) { throw new ArgumentNullException(nameof(UGrid)); }
                        if (UGrid.Center != value) { UGrid.Center = value; }
                        break;
                    default: goto case ModelingDomain.Spatial;
                }
            }
        }

        /// <summary>
        /// Gets or sets the sampling grid of the residual field part U.
        /// </summary>
        internal GridInfo1D? UGrid { get; set; } = null;

        /// <summary>
        /// Gets or sets the values of the residual field part U.
        /// </summary>
        internal VectorZ? UValues { get; set; } = null;

        /// <summary>
        /// Gets or sets the analytical phase term for the residual field part U.
        /// </summary>
        internal Analyt1DPhase? UPhase { get; set; } = null;

        /// <summary>
        /// Gets or sets the interpolation method of the residual field data.
        /// </summary>
        private InterpolationMethod IntrplMethod { get; set; } = Defaults.IntrplOption;

        /// <summary>
        /// Gets the residual field part U that is specified by grid data plus interpolation or fitting techniques.
        /// </summary>
        public Grid1DCplxData U
        {
            get
            {
                if (UValues == null) { throw new ArgumentNullException(nameof(UValues)); }
                UGrid ??= new GridInfo1D(n: UValues.Count);
                UPhase ??= new Analyt1DPhase(c1: 0.0);
                return new Grid1DCplxData(values: UValues,
                    gridInfo: UGrid,
                    a1: UPhase.C1,
                    intrpl: IntrplMethod,
                    bound: Defaults.BoundaryOption);
            }
        }

        /// <summary>
        /// [FUTURE] Gets or sets the smooth phase part Psi that is specified by function or parameterized fitting techniques.
        /// <para>Variable: x</para>
        /// <para>Function: psi = psi(x)</para>
        /// </summary>
        private Func<double, double> Psi { get; set; }

        /// <summary>
        /// [FUTURE] Gets the complete complex field function.
        /// <para>Variable: x</para>
        /// <para>Return: f = a * u(x) * Exp[i*Psi(x)]</para>
        /// </summary>
        private Func<double, bool, Complex> F => (x, includeScaling)
            => (includeScaling ? Scaling : 1.0) * U.FindValue(x)
            * Complex.Exp(Complex.ImaginaryOne * Psi(x));

        /// <summary>
        /// Gets or sets the normalized transverse spatial frequencies along the x-direction.
        /// <para>
        /// nx = kx / k0, where kx is the spatial frequency and k0 is the wavenumber in vacuum.
        /// </para>        
        /// </summary>
        internal VectorD? Nx { get; set; } = null;

        /// <summary>
        /// Gets or sets the normalized eigenvalues along the z-direction.
        /// <para>
        /// nz = kz / k0, where kz is the spatial frequency in the z-direction and k0 is the wavenumber in vacuum.
        /// </para>
        /// </summary>
        internal VectorZ? Nz { get; set; } = null;

        #endregion
        #region constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SCField1D"/> class with default parameters.
        /// The default material is set to a non-dispersive material with a real refractive index of 1.0.
        /// The smooth phase function <c>Psi</c> is initialized to zero for all positions.
        /// </summary>
        internal SCField1D()
        {
            Material = new FuncMaterial(nReal: 1.0);
            Psi = (x) => 0.0;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SCField1D"/> class with the specified parameters.
        /// </summary>
        /// <param name="wavelength">The wavelength in vacuum.</param>
        /// <param name="material">The embedding material of the field.</param>
        /// <param name="uGrid">The sampling grid of the residual field part U.</param>
        /// <param name="uValues">The values of the residual field part U.</param>
        /// <param name="uPhase">The analytical phase term for the residual field part U. If null, a default phase is used.</param>
        /// <param name="intrpl">The interpolation method for the U-field. Defaults to <see cref="Defaults.IntrplOption"/>.</param>
        /// <param name="shiftX">The lateral shift in the x-domain. Default is 0.0.</param>
        /// <param name="shiftKx">The lateral shift in the k-domain. Default is 0.0.</param>
        /// <param name="psi">The smooth phase function Psi. If null, a zero phase function is used.</param>
        /// <param name="scaling">The constant scaling factor for the field values. Default is 1.0.</param>
        /// <param name="domain">The modeling domain: spatial (X) or spatial-frequency (Kx). Default is <see cref="ModelingDomain.Spatial"/>.</param>
        public SCField1D(double wavelength, Material material,
            GridInfo1D uGrid, VectorZ uValues, Analyt1DPhase? uPhase = null,
            InterpolationMethod intrpl = Defaults.IntrplOption,
            double shiftX = 0.0, double shiftKx = 0.0,
            Func<double, double>? psi = null,
            double scaling = 1.0, ModelingDomain domain = ModelingDomain.Spatial)
        {
            Wavelength = wavelength;
            Material = material;
            Domain = domain;
            Scaling = scaling;

            // grid, values, and phase
            UGrid = new GridInfo1D(other: uGrid);
            UValues = new VectorZ(other: uValues, deepCopy: true);
            UPhase = uPhase != null ? new Analyt1DPhase(source: uPhase)
                : new Analyt1DPhase(c1: 0.0);
            // defines interpolation method for U-field
            IntrplMethod = intrpl;

            // assigns shift ... at last
            ShiftX = shiftX;
            ShiftKx = shiftKx;

            // Psi for future
            Psi = psi ?? ((x) => 0.0);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SCField1D"/> class with the specified field data.
        /// </summary>
        /// <param name="wavelength">The wavelength in vacuum.</param>
        /// <param name="material">The embedding material of the field.</param>
        /// <param name="u">The residual field part U, containing grid, values, and phase information.</param>
        /// <param name="psi">The smooth phase part Psi. Currently not supported; if null, a zero phase function is used.</param>
        /// <param name="shiftX">Lateral shift in the x-domain. This value overrides the center of the grid of U.</param>
        /// <param name="shiftKx">Lateral shift in the k-domain. This value overrides the center of the grid of U.</param>
        /// <param name="scaling">Constant scaling factor for the field values.</param>
        /// <param name="domain">Modeling domain: spatial (X) or spatial-frequency (Kx).</param>
        public SCField1D(double wavelength, Material material,
            Grid1DCplxData u,
            Func<double, double>? psi = null,
            double shiftX = 0.0, double shiftKx = 0.0,
            double scaling = 1.0, ModelingDomain domain = ModelingDomain.Spatial)
        {
            Wavelength = wavelength;
            Material = material;
            Domain = domain;
            Scaling = scaling;

            // grid, values, and phase
            UGrid = new GridInfo1D(other: u.GridInfo);
            UValues = new VectorZ(other: u.Values, deepCopy: true);
            UPhase = new Analyt1DPhase(source: u.Phase);
            // defines interpolation method for U-field
            IntrplMethod = u.IntrplMethod;

            // shifts ...at last
            ShiftX = shiftX;
            ShiftKx = shiftKx;

            // Psi for future
            Psi = psi ?? ((x) => 0.0);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SCField1D"/> class using a user-defined function for the residual field part U.
        /// </summary>
        /// <param name="wavelength">The wavelength in vacuum.</param>
        /// <param name="material">The embedding material of the field.</param>
        /// <param name="uFunc">A function that defines the residual field part U as a function of position.</param>
        /// <param name="uGrid">The sampling grid of the residual field part U.</param>
        /// <param name="uPhase">The analytical phase term for the residual field part U. If null, a default phase is used.</param>
        /// <param name="psi">The smooth phase part Psi. Currently not supported; if null, a zero phase function is used.</param>
        /// <param name="shiftX">Lateral shift in the x-domain. This value overrides the center of the grid of U.</param>
        /// <param name="shiftKx">Lateral shift in the k-domain. This value overrides the center of the grid of U.</param>
        /// <param name="scaling">Constant scaling factor for the field values.</param>
        /// <param name="domain">Modeling domain: spatial (X) or spatial-frequency (Kx).</param>
        /// <param name="loopMode">Computational option for loops.</param>
        public SCField1D(double wavelength, Material material,
            Func<double, Complex> uFunc, GridInfo1D uGrid, Analyt1DPhase? uPhase = null,
            Func<double, double>? psi = null,
            double shiftX = 0.0, double shiftKx = 0.0,
            double scaling = 1.0, ModelingDomain domain = ModelingDomain.Spatial,
            LoopMode loopMode = Defaults.LoopOption)
        {
            Wavelength = wavelength;
            Material = material;
            Domain = domain;
            Scaling = scaling;

            // grid, values, and phase
            UGrid = new GridInfo1D(other: uGrid);
            UValues = new Samp1DCplxFunc(f: uFunc).Sample(grid: uGrid, loopMode);
            UPhase = uPhase != null ? new Analyt1DPhase(source: uPhase)
                : new Analyt1DPhase(c1: 0.0);
            // defines interpolation method for U-field
            IntrplMethod = Defaults.IntrplOption;

            // shifts ... at last
            ShiftX = shiftX;
            ShiftKx = shiftKx;

            // Psi for future
            Psi = psi ?? ((x) => 0.0);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SCField1D"/> class by copying from another instance.
        /// </summary>
        /// <param name="other">The source <see cref="SCField1D"/> instance to copy from.</param>
        /// <param name="copyMode">Specifies whether to perform a deep or shallow copy of the internal arrays. Default is <see cref="ArrayCopyMode.Deep"/>.</param>
        public SCField1D(SCField1D other,
            ArrayCopyMode copyMode = ArrayCopyMode.Deep)
        {
            Wavelength = other.Wavelength;
            Material = new(other: other.Material);
            Domain = other.Domain;
            Scaling = other.Scaling;

            // grid, values, and phase
            UGrid = other.UGrid is null ? null : new GridInfo1D(other.UGrid);
            UValues = other.UValues is null ? null : new VectorZ(other: other.UValues, deepCopy: copyMode == ArrayCopyMode.Deep);
            UPhase = other.UPhase is null ? null : new Analyt1DPhase(source: other.UPhase);
            // defines the interpolation method for U-field
            IntrplMethod = other.IntrplMethod;

            // shifts ... at last
            ShiftX = other.ShiftX;
            ShiftKx = other.ShiftKx;

            // Psi for future
            Psi = other.Psi;
        }

        #endregion
        #region methods

        #region ---- sample U ----

        /// <summary>
        /// Samples the residual field on a set of scattered points.
        /// </summary>
        /// <param name="xs">Sample locations, either uniform or scattered.</param>
        /// <param name="includeAnalyticPhase">Whether to include the analytic phase for sampling.</param>
        /// <param name="loopMode">Loop-computational mode options.</param>
        /// <returns>Sampled residual field values.</returns>
        public VectorZ SampleU(ScatInfo1D xs,
            bool includeAnalyticPhase = false,
            LoopMode loopMode = Defaults.LoopOption)
        {
            if (UValues == null) { throw new ArgumentNullException(nameof(UValues)); }
            VectorZ u = U.FindValues(xs, loopMode);
            if (includeAnalyticPhase)
            {
                VectorD p = U.Phase.Sample(xs, loopMode);
                Action<long> a = (i) => u[i, false] *= Complex.Exp(Complex.ImaginaryOne * p[i, false]);
                Loop1D loop = new(operation: a, start: 0, end: xs.Count, step: 1);
                loop.Evaluate(loopMode);
            }
            return u;
        }

        /// <summary>
        /// Samples the residual field on a target uniform grid.
        /// </summary>
        /// <param name="grid">Target uniform grid.</param>
        /// <param name="includeAnalyticPhase">Whether to include the analytic phase for sampling.</param>
        /// <param name="loopMode">Loop-computational mode options.</param>
        /// <returns>Sampled residual field values.</returns>
        public VectorZ SampleU(GridInfo1D grid,
            bool includeAnalyticPhase = false,
            LoopMode loopMode = Defaults.LoopOption)
        {
            if (UValues == null) { throw new ArgumentNullException(nameof(UValues)); }
            VectorZ u = (grid == UGrid) ? new VectorZ(other: UValues, deepCopy: true)
                : U.FindValues(targetGrid: grid, loopMode: loopMode);
            if (includeAnalyticPhase && UPhase != null)
            {
                VectorD p = UPhase.Sample(grid, loopMode);
                Complex imgOne = Complex.ImaginaryOne;
                void op(long i) =>
                    u[i, false] *= Complex.Exp(imgOne * p[i, false]);
                Loop1D loop = new(operation: op, start: 0, end: grid.Count, step: 1);
                loop.Evaluate(loopMode);
            }
            return u;
        }

        #endregion
        #region ---- sample Psi ----

        /// <summary>
        /// Samples the smooth phase Psi on a set of sampling locations.
        /// </summary>
        /// <param name="xs">Sample locations, either uniform or scattered.</param>
        /// <param name="loopMode">Loop-computational mode options.</param>
        /// <returns>Sampled smooth phase values.</returns>
        internal VectorD SamplePsi(ScatInfo1D xs,
            LoopMode loopMode = Defaults.LoopOption)
            => new Samp1DRealFunc(f: Psi).Sample(xs: xs, loopMode: loopMode);

        /// <summary>
        /// Samples the smooth phase Psi on a target uniform grid.
        /// </summary>
        /// <param name="grid">Target uniform grid.</param>
        /// <param name="loopMode">Loop-computational mode options.</param>
        /// <returns>Sampled smooth phase values.</returns>
        internal VectorD SamplePsi(GridInfo1D grid,
            LoopMode loopMode = Defaults.LoopOption)
            => new Samp1DRealFunc(f: Psi).Sample(grid, loopMode);

        #endregion
        #region ---- sample ----

        ///// <summary>
        ///// samples the complete field F on a set of sampling locations
        ///// </summary>
        ///// <param name="xs"> sample locations, either uniform or scattered </param>
        ///// <param name="loopMode"> loop-computational mode options </param>
        ///// <returns> sampled complete field values </returns>
        //public VectorZ Sample(ScatInfo1D xs,
        //    LoopMode loopMode = Defaults.LoopOption)
        //    => new Samp1DCplxFunc(f: F).Sample(xs: xs, loopMode: loopMode);

        ///// <summary>
        ///// samples the complete field F on a target uniform grid
        ///// </summary>
        ///// <param name="grid"> target uniform grid </param>
        ///// <param name="loopMode"> loop-computational mode options </param>
        ///// <returns> sampled complete field values </returns>
        //public VectorZ Sample(GridInfo1D grid,
        //    LoopMode loopMode = Defaults.LoopOption)
        //    => new Samp1DCplxFunc(f: F).Sample(grid, loopMode);

        #endregion
        #region ---- eigen-info ----

        /// <summary>
        /// Computes the eigen information (normalized spatial frequencies and eigenvalues).
        /// </summary>
        /// <param name="loopMode">Loop-computational mode options.</param>
        [MemberNotNull(nameof(Nx), nameof(Nz))]
        public void ComputeEigenInfo(LoopMode loopMode = Defaults.LoopOption)
        {
            if (UGrid == null) { throw new ArgumentNullException(nameof(UGrid)); }
            GridInfo1D kGrid = new(other: UGrid);
            if (Domain == ModelingDomain.Spatial) { kGrid.GetConjugated(isForward: true); }
            Nx = kGrid.GetCoordinates() / K0;
            UniformLayer freeSpace = new(epsilon: Material.Epsilon, mu: Material.Mu, thickness: 0.0);
            Nz = freeSpace.ComputeNz(wavelength: Wavelength, nx: Nx,
                loopMode: loopMode);
        }

        #endregion
        #region ---- transform ----

        /// <summary>
        /// Switches the field representation to the spatial-frequency (K) domain.
        /// </summary>
        public void SwitchToKDomain()
        {
            if (Domain == ModelingDomain.SpatialFrequency) { return; }
            if (UValues == null) { throw new ArgumentNullException(nameof(UValues)); }
            if (UGrid == null) { throw new ArgumentNullException(nameof(UGrid)); }
            if (UPhase == null) { throw new ArgumentNullException(nameof(UPhase)); }

            VectorZ uValues = UValues;
            GridInfo1D uGrid = UGrid;
            double c1 = UPhase.C1;
            Transform.FFT1D(x: ref uValues, grid: ref uGrid, c: ref c1,
                direction: FFTOptions.Direction.Forward,
                convention: FFTOptions.Convention.ZeroCentered,
                conversion: FFTOptions.Conversion.DataShift,
                copyMode: FFTOptions.CopyMode.Block);
            UPhase.C1 = c1;

            Domain = ModelingDomain.SpatialFrequency;
        }

        /// <summary>
        /// Switches the field representation to the spatial (X) domain.
        /// </summary>
        public void SwitchToXDomain()
        {
            if (Domain == ModelingDomain.Spatial) { return; }
            if (UValues == null) { throw new ArgumentNullException(nameof(UValues)); }
            if (UGrid == null) { throw new ArgumentNullException(nameof(UGrid)); }
            if (UPhase == null) { throw new ArgumentNullException(nameof(UPhase)); }

            VectorZ uValues = UValues;
            GridInfo1D uGrid = UGrid;
            double c1 = UPhase.C1;
            Transform.FFT1D(x: ref uValues, grid: ref uGrid, c: ref c1,
                direction: FFTOptions.Direction.Backward,
                convention: FFTOptions.Convention.ZeroCentered,
                conversion: FFTOptions.Conversion.DataShift,
                copyMode: FFTOptions.CopyMode.Block);
            UPhase.C1 = c1;

            Domain = ModelingDomain.Spatial;
        }

        #endregion
        #region ---- resize ----

        /// <summary>
        /// Performs zero-value central padding of the residual field U.
        /// </summary>
        /// <param name="targetCount">Target number of elements after padding.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public void Padding(long targetCount)
        {
            if (UValues == null) { throw new ArgumentNullException(nameof(UValues)); }
            if (UGrid == null) { throw new ArgumentNullException(nameof(UGrid)); }
            UValues = UValues.Padding(targetCount);
            UGrid = new(n: targetCount,
                spacing: UGrid.Spacing,
                refPoint: UGrid.Start - (targetCount - UGrid.Count) / 2 * UGrid.Spacing,
                refType: GridRefType.Start);
        }

        /// <summary>
        /// Performs central truncation of the residual field U.
        /// </summary>
        /// <param name="targetCount">Target number of elements after truncation.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public void Truncate(long targetCount)
        {
            if (UValues == null) { throw new ArgumentNullException(nameof(UValues)); }
            if (UGrid == null) { throw new ArgumentNullException(nameof(UGrid)); }
            UValues = UValues.Truncate(targetCount);
            UGrid = new(n: targetCount,
                spacing: UGrid.Spacing,
                refPoint: UGrid.Start + (UGrid.Count - targetCount) / 2 * UGrid.Spacing,
                refType: GridRefType.Start);
        }

        #endregion
        #region ---- propagate ----

        /// <summary>
        /// Propagates the field U to a parallel plane at a distance d.
        /// </summary>
        /// <param name="d">Propagation distance along the z-axis.</param>
        /// <param name="dx">Expected center shift in the x-domain after propagation.</param>
        /// <param name="targetDomain">Target domain after the propagation.</param>
        /// <param name="loopMode">Loop-computational mode options.</param>
        /// <param name="xSizeFactor">Padding or truncation factor in the x-domain.</param>
        /// <param name="kSizeFactor">(Central) filtering factor in the k-domain.</param>
        /// <param name="kEdgeRatio">Smooth edge factor for k-domain truncation, with respect to truncation width.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public void Propagate(double d,
            double? dx = null,
            ModelingDomain targetDomain = ModelingDomain.SpatialFrequency,
            LoopMode loopMode = Defaults.LoopOption,
            double xSizeFactor = 1.0,
            double kSizeFactor = 1.0, double kEdgeRatio = 0.2)
        {
            if (UValues == null) { throw new ArgumentNullException(nameof(UValues)); }
            if (UGrid == null) { throw new ArgumentNullException(nameof(UGrid)); }
            if (UPhase == null) { throw new ArgumentNullException(nameof(UPhase)); }

            // [possible] input field padding
            if (xSizeFactor > 1.0)
            {
                SwitchToXDomain(); // makes sure to pad in the x-domain
                long targetCount = (long)(UGrid.Count * xSizeFactor);
                if ((targetCount - UGrid.Count) % 2 != 0) { targetCount++; }
                Padding(targetCount);
            }
            
            // [possible] input domain handling
            SwitchToKDomain();
            
            // [possible] truncation in k-domain
            if (kSizeFactor < 1.0)
            {
                // filter width in k-domain
                double wKx = UGrid.Range * kSizeFactor;
                Samp1DRealFunc t = new(f: Function1D.CosEdgeRectangle, p: [wKx, kEdgeRatio * wKx]);
                UValues *= t.Sample(grid: UGrid, loopMode);
            }
            
            // SPW kernel
            if (xSizeFactor != 1.0 || kSizeFactor != 1.0 || Nz == null || Nx == null) 
            { ComputeEigenInfo(loopMode); }
            // automatically handles the center shift in x-domain after propagation
            double kRe = Material.NReal(Wavelength) * K0;
            double kz = Math.Sqrt(kRe * kRe - ShiftKx * ShiftKx);
            if (kz == 0.0) throw new DivideByZeroException(nameof(kz));
            dx ??= ShiftKx / kz * d; //Math.Tan(Math.Asin(ShiftKx / K0)) * d;
            VectorZ uValues = UValues;
            SPW.Propagate1D(wavelength: Wavelength,
                v: ref uValues, // to be modified 
                nx: Nx, nz: Nz, z: d, 
                cLinear: dx.Value, // additional linear phase in the propagation kernel 
                loopMode: loopMode);
            // to compensates the additional linear phase in k-domain, linear coefficient should get (-dx)
            UPhase.C1 -= (dx == null) ? 0.0 : dx.Value;

            // [possible] output field truncation
            if (xSizeFactor < 1.0)
            {
                SwitchToXDomain(); // makes sure to truncate in the x-domain
                long targetCount = (long)(UGrid.Count * xSizeFactor);
                if ((UGrid.Count - targetCount) % 2 != 0) { targetCount--; }
                Truncate(targetCount);
            }

            // [possible] output domain handling
            if (targetDomain == ModelingDomain.Spatial) { SwitchToXDomain(); }
            else { SwitchToKDomain(); }
        }

        #endregion

        #endregion

        #region derived sub-classes ...

        /// <summary>
        /// Represents a Gaussian 1D field at its waist.
        /// </summary>
        public class Gaussian : SCField1D
        {
            #region properties

            /// <summary>
            /// Gets or sets the radius of the waist.
            /// </summary>
            public double WaistRadius { get; set; }

            #endregion
            #region constructor

            /// <summary>
            /// Initializes a new instance of the <see cref="Gaussian"/> class with default parameters.
            /// </summary>
            internal Gaussian() { }

            /// <summary>
            /// Constructs a scalar Gaussian field with specific parameters.
            /// </summary>
            /// <param name="wavelength">Wavelength in vacuum.</param>
            /// <param name="material">Embedding material of the field.</param>
            /// <param name="waistRadius">Waist radius.</param>
            /// <param name="grid">Sampling grid information.</param>
            /// <param name="psi">Smooth phase part Psi - currently NOT supported.</param>
            /// <param name="shiftX">Lateral shift in x-domain; if not null, this value will override the center of the grid.</param>
            /// <param name="shiftKx">Lateral shift in k-domain; if not null, this value will override the center of the grid.</param>
            /// <param name="scaling">Constant scaling of the field values.</param>
            /// <param name="domain">Modeling domain: spatial or spatial-frequency.</param>
            /// <param name="loopMode">Loop-computational mode options.</param>
            public Gaussian(double wavelength, Material material,
                double waistRadius, GridInfo1D grid,
                Func<double, double>? psi = null,
                double shiftX = 0.0, double shiftKx = 0.0,
                double scaling = 1.0, ModelingDomain domain = ModelingDomain.Spatial,
                LoopMode loopMode = Defaults.LoopOption)
                : base(wavelength: wavelength, material: material,
                      uFunc: (x) => Function1D.Gaussian(x, [waistRadius]),
                      uGrid: grid, uPhase: null, psi: psi,
                      shiftX: shiftX, shiftKx: shiftKx,
                      scaling: scaling, domain: domain, loopMode: loopMode)
            {
                WaistRadius = waistRadius;
            }

            #endregion
        }

        /// <summary>
        /// Represents a super-Gaussian 1D field at its waist (flat-top/top-hat beam).
        /// </summary>
        public class SuperGaussian : SCField1D
        {
            #region properties

            /// <summary>
            /// Gets or sets the radius of the waist.
            /// </summary>
            public double WaistRadius { get; set; }

            /// <summary>
            /// Gets or sets the order of the super Gaussian profile.
            /// </summary>
            public long Order { get; set; }

            #endregion
            #region constructor

            /// <summary>
            /// Initializes a new instance of the <see cref="SuperGaussian"/> class with default parameters.
            /// </summary>
            internal SuperGaussian() { }

            /// <summary>
            /// Constructs a scalar super-Gaussian field with specific parameters.
            /// </summary>
            /// <param name="wavelength">Wavelength in vacuum.</param>
            /// <param name="material">Embedding material of the field.</param>
            /// <param name="waistRadius">Waist radius.</param>
            /// <param name="order">Order of the super-Gaussian profile.</param>
            /// <param name="grid">Sampling grid information.</param>
            /// <param name="psi">Smooth phase part Psi - currently NOT supported.</param>
            /// <param name="shiftX">Lateral shift in x-domain; if not null, this value will override the center of the grid.</param>
            /// <param name="shiftKx">Lateral shift in k-domain; if not null, this value will override the center of the grid.</param>
            /// <param name="scaling">Constant scaling of the field values.</param>
            /// <param name="domain">Modeling domain: spatial or spatial-frequency.</param>
            /// <param name="loopMode">Loop-computational mode options.</param>
            public SuperGaussian(double wavelength, Material material,
                double waistRadius, long order, GridInfo1D grid,
                Func<double, double>? psi = null,
                double shiftX = 0.0, double shiftKx = 0.0,
                double scaling = 1.0, ModelingDomain domain = ModelingDomain.Spatial,
                LoopMode loopMode = Defaults.LoopOption)
                : base(wavelength: wavelength, material: material,
                      uFunc: (x) => Function1D.SuperGaussian(x, [waistRadius, order]), uGrid: grid,
                      psi: psi, shiftX: shiftX, shiftKx: shiftKx,
                      scaling: scaling, domain: domain, loopMode: loopMode)
            {
                WaistRadius = waistRadius;
                Order = order;
            }

            #endregion
        }

        /// <summary>
        /// Represents a truncated plane wave 1D field.
        /// </summary>
        public class PlaneWave : SCField1D
        {
            #region properties

            /// <summary>
            /// Gets or sets the truncation diameter of the field.
            /// </summary>
            public double Diameter { get; set; }

            /// <summary>
            /// Gets or sets the absolute edge width (half inside diameter, half outside).
            /// </summary>
            public double EdgeWidth { get; set; }

            #endregion
            #region constructor

            /// <summary>
            /// Initializes a new instance of the <see cref="PlaneWave"/> class with default parameters.
            /// </summary>
            internal PlaneWave() { }

            /// <summary>
            /// Constructs a truncated plane wave with specific parameters.
            /// </summary>
            /// <param name="wavelength">Wavelength in vacuum.</param>
            /// <param name="material">Embedding material of the field.</param>
            /// <param name="diameter">Diameter of the truncation.</param>
            /// <param name="grid">1D uniform sampling grid.</param>
            /// <param name="edge">Absolute edge width (half inside, half outside).</param>
            /// <param name="psi">Smooth phase part Psi - currently NOT supported.</param>
            /// <param name="shiftX">Lateral shift in x-domain; if not null, this value will override the center of the grid.</param>
            /// <param name="shiftKx">Lateral shift in k-domain; if not null, this value will override the center of the grid.</param>
            /// <param name="scaling">Constant scaling of the field values.</param>
            /// <param name="domain">Modeling domain: spatial or spatial-frequency.</param>
            /// <param name="loopMode">Loop-computational mode options.</param>
            public PlaneWave(double wavelength, Material material,
                double diameter, GridInfo1D grid,
                double edge = 0.0,
                Func<double, double>? psi = null,
                double shiftX = 0.0, double shiftKx = 0.0,
                double scaling = 1.0, ModelingDomain domain = ModelingDomain.Spatial,
                LoopMode loopMode = Defaults.LoopOption)
                : base(wavelength: wavelength, material: material,
                      uFunc: (x) => Function1D.CosEdgeRectangle(x, [diameter, edge]), uGrid: grid,
                      psi: psi, shiftX: shiftX, shiftKx: shiftKx,
                      scaling: scaling, domain: domain, loopMode: loopMode)
            {
                Diameter = diameter;
                EdgeWidth = edge;
            }

            #endregion
        }

        /// <summary>
        /// Represents a truncated cylindrical wave 1D field.
        /// </summary>
        public class CylindricalWave : SCField1D
        {
            #region properties

            /// <summary>
            /// Gets or sets the distance from the point source.
            /// </summary>
            public double SourceDistance { get; set; }

            /// <summary>
            /// Gets or sets the truncation diameter of the field.
            /// </summary>
            public double Diameter { get; set; }

            /// <summary>
            /// Gets or sets the absolute edge width (half inside diameter, half outside).
            /// </summary>
            public double EdgeWidth { get; set; }

            #endregion
            #region constructor

            /// <summary>
            /// Initializes a new instance of the <see cref="CylindricalWave"/> class with default parameters.
            /// </summary>
            internal CylindricalWave() { }

            /// <summary>
            /// Constructs a truncated cylindrical wave with specific parameters.
            /// </summary>
            /// <param name="wavelength">Wavelength in vacuum.</param>
            /// <param name="material">Embedding material of the field.</param>
            /// <param name="sourceDistance">Distance from the point source.</param>
            /// <param name="diameter">Diameter of the truncation.</param>
            /// <param name="grid">1D uniform sampling grid.</param>
            /// <param name="edge">Absolute edge width (half inside, half outside).</param>
            /// <param name="psi">Smooth phase part Psi - currently NOT supported.</param>
            /// <param name="shiftX">Lateral shift in x-domain; if not null, this value will override the center of the grid.</param>
            /// <param name="shiftKx">Lateral shift in k-domain; if not null, this value will override the center of the grid.</param>
            /// <param name="scaling">Constant scaling of the field values.</param>
            /// <param name="domain">Modeling domain: spatial or spatial-frequency.</param>
            /// <param name="loopMode">Loop-computational mode options.</param>
            public CylindricalWave(double wavelength, Material material,
                double sourceDistance, double diameter, GridInfo1D grid,
                double edge = 0.0,
                Func<double, double>? psi = null,
                double shiftX = 0.0, double shiftKx = 0.0,
                double scaling = 1.0, ModelingDomain domain = ModelingDomain.Spatial,
                LoopMode loopMode = Defaults.LoopOption)
            {
                SourceDistance = sourceDistance;
                Diameter = diameter;
                EdgeWidth = edge;

                // Precompute constants outside the sampling loop for efficiency
                double k0 = 2.0 * Math.PI / wavelength;
                double nReal = material.NReal(wavelength);
                double k = k0 * nReal;
                double srcDistSq = sourceDistance * sourceDistance;

                // Use a local function to avoid repeated allocations and redundant calculations
                Complex uFunc(double x)
                {
                    double xSq = x * x;
                    double amp = Function1D.CosEdgeRectangle(x, [diameter, edge]) * sourceDistance / Math.Sqrt(srcDistSq + xSq);
                    double phase = k * Function1D.Cylindric(x, [sourceDistance, k]);
                    return amp * Complex.Exp(Complex.ImaginaryOne * phase);
                }

                Wavelength = wavelength;
                Material = material;
                Domain = domain;
                Scaling = scaling;

                // grid, values, and phase
                UGrid = new GridInfo1D(other: grid);
                UValues = new Samp1DCplxFunc(f: uFunc).Sample(grid: UGrid, loopMode);
                UPhase = new Analyt1DPhase(c1: 0.0);
                // defines interpolation method for U-field
                IntrplMethod = Defaults.IntrplOption;

                // shifts ... at last
                ShiftX = shiftX;
                ShiftKx = shiftKx;

                // Psi for future
                Psi = psi ?? ((x) => 0.0);
            }

            #endregion
        }

        /// <summary>
        /// Represents a truncated cylindrical wave 1D field with Gaussian-profile amplitude.
        /// </summary>
        internal class GaussCylindWave : SCField1D
        {
            #region properties

            /// <summary>
            /// Gets or sets the distance from the point source.
            /// </summary>
            public double SourceDistance { get; set; }

            /// <summary>
            /// Gets or sets the truncation diameter of the field (i.e., the diameter of the Gaussian profile at its waist).
            /// </summary>
            public double Diameter { get; set; }

            #endregion
            #region constructor

            /// <summary>
            /// Initializes a new instance of the <see cref="GaussCylindWave"/> class with default parameters.
            /// </summary>
            internal GaussCylindWave() { }

            /// <summary>
            /// Constructs a truncated cylindrical wave with Gaussian-profile amplitude.
            /// </summary>
            /// <param name="wavelength">Wavelength in vacuum.</param>
            /// <param name="material">Embedding material of the field.</param>
            /// <param name="sourceDistance">Distance from the point source.</param>
            /// <param name="diameter">Diameter of the truncation.</param>
            /// <param name="grid">1D uniform sampling grid.</param>
            /// <param name="psi">Smooth phase part Psi - currently NOT supported.</param>
            /// <param name="shiftX">Lateral shift in x-domain; if not null, this value will override the center of the grid.</param>
            /// <param name="shiftKx">Lateral shift in k-domain; if not null, this value will override the center of the grid.</param>
            /// <param name="scaling">Constant scaling of the field values.</param>
            /// <param name="domain">Modeling domain: spatial or spatial-frequency.</param>
            /// <param name="loopMode">Loop-computational mode options.</param>
            public GaussCylindWave(double wavelength, Material material,
                double sourceDistance, double diameter, GridInfo1D grid,
                Func<double, double>? psi = null,
                double shiftX = 0.0, double shiftKx = 0.0,
                double scaling = 1.0, ModelingDomain domain = ModelingDomain.Spatial,
                LoopMode loopMode = Defaults.LoopOption)
            {
                SourceDistance = sourceDistance;
                Diameter = diameter;

                // Precompute constants outside the sampling loop for efficiency
                double k0 = 2.0 * Math.PI / wavelength;
                double nReal = material.NReal(wavelength);
                double k = k0 * nReal;
                double srcDistSq = sourceDistance * sourceDistance;

                // Use a local function to avoid repeated allocations and redundant calculations
                Complex uFunc(double x)
                {
                    double xSq = x * x;
                    double amp = Function1D.Gaussian(x, [0.5 * diameter]);
                    double phase = k * Function1D.Cylindric(x, [sourceDistance, k]);
                    return amp * Complex.Exp(Complex.ImaginaryOne * phase);
                }

                Wavelength = wavelength;
                Material = material;
                Domain = domain;
                Scaling = scaling;

                // grid, values, and phase
                UGrid = new GridInfo1D(other: grid);
                UValues = new Samp1DCplxFunc(f: uFunc).Sample(grid: UGrid, loopMode);
                UPhase = new Analyt1DPhase(c1: 0.0);
                // defines interpolation method for U-field
                IntrplMethod = Defaults.IntrplOption;

                // shifts ... at last
                ShiftX = shiftX;
                ShiftKx = shiftKx;

                // Psi for future
                Psi = psi ?? ((x) => 0.0);
            }

            #endregion
        }

        #endregion

    }
}
