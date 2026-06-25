using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using VEMS.MathCore;

namespace VEMS.EMSolver
{


    /// <summary>
    /// Represents a scalar field in the electromagnetic solver.
    /// Provides properties and methods for handling 2D scalar fields, including
    /// grid-based sampling, analytic phase, domain switching, propagation, and resizing.
    /// Supports construction from grid data, functions, and common field types (Gaussian, super-Gaussian, plane wave).
    /// </summary>
    public class SCField : IField
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
        /// Gets or sets the lateral shift of the field in the spatial domain along the x-direction.
        /// <para>
        /// In the spatial domain, this property returns or sets the center coordinate (<see cref="GridInfo2D.CenterX"/>) of the sampling grid.
        /// In the spatial-frequency domain, this property returns or sets the negative of the linear phase coefficient (<see cref="Analyt2DPhase.C1x"/>) of the analytic phase.
        /// </para>
        /// <para>
        /// Setting this property updates the corresponding value depending on the current modeling domain.
        /// </para>
        /// </summary>
        public double ShiftX
        {
            get
            {
                switch (Domain)
                {
                    case ModelingDomain.Spatial:
                        return UGrid?.CenterX ?? 0.0;
                    case ModelingDomain.SpatialFrequency:
                        return -UPhase?.C1x ?? 0.0;
                    default: goto case ModelingDomain.Spatial;
                }
            }
            set
            {
                switch (Domain)
                {
                    case ModelingDomain.Spatial:
                        if (UGrid == null) { throw new ArgumentNullException(nameof(UGrid)); }
                        if (UGrid.CenterX != value) { UGrid.CenterX = value; }
                        break;
                    case ModelingDomain.SpatialFrequency:
                        if (UPhase == null) { throw new ArgumentNullException(nameof(UPhase)); }
                        if (UPhase.C1x != -value) { UPhase.C1x = -value; }
                        break;
                    default: goto case ModelingDomain.Spatial;
                }
            }
        }

        /// <summary>
        /// Gets or sets the lateral shift of the field in the spatial domain along the y-direction.
        /// <para>
        /// In the spatial domain, this property returns or sets the center coordinate (<see cref="GridInfo2D.CenterY"/>) of the sampling grid.
        /// In the spatial-frequency domain, this property returns or sets the negative of the linear phase coefficient (<see cref="Analyt2DPhase.C1y"/>) of the analytic phase.
        /// </para>
        /// <para>
        /// Setting this property updates the corresponding value depending on the current modeling domain.
        /// </para>
        /// </summary>
        public double ShiftY
        {
            get
            {
                switch (Domain)
                {
                    case ModelingDomain.Spatial:
                        return UGrid?.CenterY ?? 0.0;
                    case ModelingDomain.SpatialFrequency:
                        return -UPhase?.C1y ?? 0.0;
                    default: goto case ModelingDomain.Spatial;
                }
            }
            set
            {
                switch (Domain)
                {
                    case ModelingDomain.Spatial:
                        if (UGrid == null) { throw new ArgumentNullException(nameof(UGrid)); }
                        if (UGrid.CenterY != value) { UGrid.CenterY = value; }
                        break;
                    case ModelingDomain.SpatialFrequency:
                        if (UPhase == null) { throw new ArgumentNullException(nameof(UPhase)); }
                        if (UPhase.C1y != -value) { UPhase.C1y = -value; }
                        break;
                    default: goto case ModelingDomain.Spatial;
                }
            }
        }

        /// <summary>
        /// Gets or sets the lateral shift of the field in the spatial frequency domain,
        /// along the kx-direction.
        /// <para>
        /// In the spatial domain, this property represents the linear phase coefficient (<see cref="Analyt2DPhase.C1x"/>) 
        /// of the analytic phase, which corresponds to a shift in the kx-direction in the frequency domain.
        /// In the spatial-frequency domain, this property represents the center coordinate (<see cref="GridInfo2D.CenterX"/>) 
        /// of the sampling grid, which corresponds to a shift in the kx-direction.
        /// </para>
        /// <para>
        /// Setting this property updates the corresponding value depending on the current modeling domain.
        /// </para>
        /// <para>
        /// <b>Spatial domain:</b> <c>ShiftKx</c> gets/sets <c>UPhase.C1x</c>.<br/>
        /// <b>Spatial-frequency domain:</b> <c>ShiftKx</c> gets/sets <c>UGrid.CenterX</c>.
        /// </para>
        /// <exception cref="ArgumentNullException">
        /// Thrown if the required <c>UPhase</c> or <c>UGrid</c> is <c>null</c> when setting the value.
        /// </exception>
        /// </summary>
        public double ShiftKx
        {
            get
            {
                switch (Domain)
                {
                    case ModelingDomain.Spatial:
                        return UPhase?.C1x ?? 0.0;
                    case ModelingDomain.SpatialFrequency:
                        return UGrid?.CenterX ?? 0.0;
                    default: goto case ModelingDomain.Spatial;
                }
            }
            set
            {
                switch (Domain)
                {
                    case ModelingDomain.Spatial:
                        if (UPhase == null) { throw new ArgumentNullException(nameof(UPhase)); }
                        if (UPhase.C1x != value) { UPhase.C1x = value; }
                        break;
                    case ModelingDomain.SpatialFrequency:
                        if (UGrid == null) { throw new ArgumentNullException(nameof(UGrid)); }
                        if (UGrid.CenterX != value) { UGrid.CenterX = value; }
                        break;
                    default: goto case ModelingDomain.Spatial;
                }
            }
        }

        /// <summary>
        /// Gets or sets the lateral shift of the field in the spatial frequency domain,
        /// along the ky-direction.
        /// <para>
        /// In the spatial domain, this property represents the linear phase coefficient (<see cref="Analyt2DPhase.C1y"/>) 
        /// of the analytic phase, which corresponds to a shift in the ky-direction in the frequency domain.
        /// In the spatial-frequency domain, this property represents the center coordinate (<see cref="GridInfo2D.CenterY"/>) 
        /// of the sampling grid, which corresponds to a shift in the ky-direction.
        /// </para>
        /// <para>
        /// Setting this property updates the corresponding value depending on the current modeling domain.
        /// </para>
        /// <para>
        /// <b>Spatial domain:</b> <c>ShiftKy</c> gets/sets <c>UPhase.C1y</c>.<br/>
        /// <b>Spatial-frequency domain:</b> <c>ShiftKy</c> gets/sets <c>UGrid.CenterY</c>.
        /// </para>
        /// <exception cref="ArgumentNullException">
        /// Thrown if the required <c>UPhase</c> or <c>UGrid</c> is <c>null</c> when setting the value.
        /// </exception>
        /// </summary>
        public double ShiftKy 
        {
            get
            {
                switch (Domain)
                {
                    case ModelingDomain.Spatial:
                        return UPhase?.C1y ?? 0.0;
                    case ModelingDomain.SpatialFrequency:
                        return UGrid?.CenterY ?? 0.0;
                    default: goto case ModelingDomain.Spatial;
                }
            }
            set
            {
                switch (Domain)
                {
                    case ModelingDomain.Spatial:
                        if (UPhase == null) { throw new ArgumentNullException(nameof(UPhase)); }
                        if (UPhase.C1y != value) { UPhase.C1y = value; }
                        break;
                    case ModelingDomain.SpatialFrequency:
                        if (UGrid == null) { throw new ArgumentNullException(nameof(UGrid)); }
                        if (UGrid.CenterY != value) { UGrid.CenterY = value; }
                        break;
                    default: goto case ModelingDomain.Spatial;
                }
            }
        }

        /// <summary>
        /// Gets or sets the sampling grid of the residual field part U.
        /// <para>
        /// This property holds the <see cref="GridInfo2D"/> instance that describes the uniform sampling grid
        /// for the residual field component U. The grid defines the spatial or spatial-frequency coordinates
        /// at which the field values are sampled or represented.
        /// </para>
        /// <para>
        /// If <c>null</c>, the grid will be initialized as needed in property accessors or methods that require it.
        /// </para>
        /// </summary>
        internal GridInfo2D? UGrid { get; set; } = null;

        /// <summary>
        /// Gets or sets the values of the residual field part U.
        /// <para>
        /// This property holds the <see cref="MatrixZ"/> instance that contains the complex-valued samples
        /// of the residual field component U on the associated grid (<see cref="UGrid"/>).
        /// The matrix dimensions correspond to the number of rows and columns in the sampling grid.
        /// </para>
        /// <para>
        /// If <c>null</c>, the field data has not been initialized.
        /// </para>
        /// </summary>
        internal MatrixZ? UValues { get; set; } = null;

        /// <summary>
        /// Gets or sets the analytic phase term associated with the residual field part U.
        /// <para>
        /// This property holds the <see cref="Analyt2DPhase"/> instance that describes the analytic phase
        /// for the residual field component U. The analytic phase typically includes linear and possibly
        /// higher-order phase terms, which are used to represent phase variations across the field.
        /// </para>
        /// <para>
        /// If <c>null</c>, the analytic phase will be initialized as needed in property accessors or methods that require it.
        /// </para>
        /// </summary>
        internal Analyt2DPhase? UPhase { get; set; } = null;

        /// <summary>
        /// Gets or sets the interpolation method used for the residual field data.
        /// <para>
        /// This property determines the interpolation technique applied when evaluating or resampling
        /// the residual field component U on different grids or at arbitrary locations.
        /// </para>
        /// <para>
        /// The default value is specified by <see cref="Defaults.IntrplOption"/>.
        /// </para>
        /// </summary>
        private InterpolationMethod IntrplMethod { get; set; } = Defaults.IntrplOption;

        /// <summary>
        /// Gets the residual field part U, which is specified by grid data
        /// and can be accessed with interpolation or fitting techniques.
        /// <para>
        /// This property constructs a <see cref="Grid2DCplxData"/> object using the current
        /// <see cref="UValues"/>, <see cref="UGrid"/>, and <see cref="UPhase"/> values.
        /// The interpolation method and boundary options are set according to the current field settings.
        /// </para>
        /// </summary>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <see cref="UValues"/> is <c>null</c>.
        /// </exception>
        public Grid2DCplxData U
        {
            get
            {
                if (UValues == null) { throw new ArgumentNullException(nameof(UValues)); }
                UGrid ??= new GridInfo2D(rows: UValues.Rows, cols: UValues.Cols);
                UPhase ??= new Analyt2DPhase(c1x: 0.0, c1y: 0.0);
                return new Grid2DCplxData(values: UValues,
                    gridInfo: UGrid,
                    c1x: UPhase.C1x, c1y: UPhase.C1y,
                    intrpl: IntrplMethod,
                    boundX: Defaults.BoundaryOption, boundY: Defaults.BoundaryOption);
            }
        }

        /// <summary>
        /// [FUTURE] Gets or sets the smooth phase part Psi, which is specified by a function
        /// or parameterized fitting techniques.
        /// <para>
        /// This function takes two variables: x and y, and returns the phase value psi = psi(x, y).
        /// </para>
        /// </summary>
        private Func<double, double, double> Psi { get; set; }

        /// <summary>
        /// [FUTURE] Gets the complete complex field function.
        /// <para>
        /// This function takes two variables (x, y) and a boolean flag <paramref name="includeScaling"/>.
        /// It returns the field value as: f = a * u(x, y) * Exp[i * Psi(x, y)],
        /// where a is the scaling factor, u(x, y) is the residual field, and Psi(x, y) is the smooth phase.
        /// </para>
        /// </summary>
        private Func<double, double, bool, Complex> F => (x, y, includeScaling)
            => (includeScaling ? Scaling : 1.0) * U.FindValue(x, y)
            * Complex.Exp(Complex.ImaginaryOne * Psi(x, y));

        /// <summary>
        /// Gets or sets the normalized transverse spatial frequencies along the x-direction.
        /// <para>
        /// nx = kx / k0, where kx is the spatial frequency and k0 is the wavenumber in vacuum.
        /// </para>
        /// </summary>
        internal VectorD? Nx { get; set; } = null;

        /// <summary>
        /// Gets or sets the normalized transverse spatial frequencies along the y-direction.
        /// <para>
        /// ny = ky / k0, where ky is the spatial frequency and k0 is the wavenumber in vacuum.
        /// </para>
        /// </summary>
        internal VectorD? Ny { get; set; } = null;

        /// <summary>
        /// Gets or sets the normalized eigenvalues along the z-direction.
        /// <para>
        /// nz = kz / k0, where kz is the spatial frequency in the z-direction and k0 is the wavenumber in vacuum.
        /// </para>
        /// </summary>
        internal MatrixZ? Nz { get; set; } = null;
        
        #endregion
        #region constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="SCField"/> class with default parameters.
        /// <para>
        /// The default material is set to a non-dispersive material with real refractive index 1.0.
        /// The smooth phase function <c>Psi</c> is set to zero everywhere.
        /// </para>
        /// </summary>
        internal SCField()
        {
            Material = new FuncMaterial(nReal: 1.0);
            Psi = (x, y) => 0.0;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SCField"/> class with explicit grid, values, and phase.
        /// </summary>
        /// <param name="wavelength">Wavelength in vacuum.</param>
        /// <param name="material">Embedding material of the field.</param>
        /// <param name="uGrid">Sampling grid of the residual field part U.</param>
        /// <param name="uValues">Complex-valued samples of the residual field part U.</param>
        /// <param name="uPhase">Analytic phase term associated with the residual field part U (optional).</param>
        /// <param name="intrpl">Interpolation method for the residual field data (optional).</param>
        /// <param name="shiftX">Lateral shift in x-direction (optional).</param>
        /// <param name="shiftY">Lateral shift in y-direction (optional).</param>
        /// <param name="shiftKx">Lateral shift in kx-direction (optional).</param>
        /// <param name="shiftKy">Lateral shift in ky-direction (optional).</param>
        /// <param name="psi">Smooth phase part Psi (optional, currently not supported).</param>
        /// <param name="scaling">Constant scaling of the field values (optional).</param>
        /// <param name="domain">Modeling domain: spatial (X,Y) or spatial-frequency (Kx,Ky) (optional).</param>
        public SCField(double wavelength, Material material,
            GridInfo2D uGrid, MatrixZ uValues, Analyt2DPhase? uPhase = null,
            InterpolationMethod intrpl = Defaults.IntrplOption,
            double shiftX = 0.0, double shiftY = 0.0,
            double shiftKx = 0.0, double shiftKy = 0.0,
            Func<double, double, double>? psi = null,
            double scaling = 1.0, ModelingDomain domain = ModelingDomain.Spatial)
        {
            Wavelength = wavelength;
            Material = material;
            Domain = domain;
            Scaling = scaling;

            // grid, values, and phase
            UGrid = new GridInfo2D(other: uGrid);
            UValues = new MatrixZ(other: uValues, deepCopy: true);
            UPhase = uPhase != null ? new Analyt2DPhase(source: uPhase)
                : new Analyt2DPhase(c1x: 0.0, c1y: 0.0);
            // defines interpolation method for U-field
            IntrplMethod = intrpl;

            // assigns shift ... at last
            ShiftX = shiftX;
            ShiftY = shiftY;
            ShiftKx = shiftKx;
            ShiftKy = shiftKy;

            // Psi for future
            Psi = psi ?? ((x, y) => 0.0);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SCField"/> class from a <see cref="Grid2DCplxData"/> object.
        /// </summary>
        /// <param name="wavelength">Wavelength in vacuum.</param>
        /// <param name="material">Embedding material of the field.</param>
        /// <param name="u">Residual field part U as <see cref="Grid2DCplxData"/>.</param>
        /// <param name="psi">Smooth phase part Psi (optional, currently not supported).</param>
        /// <param name="shiftX">Lateral shift in x-direction (optional).</param>
        /// <param name="shiftY">Lateral shift in y-direction (optional).</param>
        /// <param name="shiftKx">Lateral shift in kx-direction (optional).</param>
        /// <param name="shiftKy">Lateral shift in ky-direction (optional).</param>
        /// <param name="scaling">Constant scaling of the field values (optional).</param>
        /// <param name="domain">Modeling domain: spatial (X,Y) or spatial-frequency (Kx,Ky) (optional).</param>
        public SCField(double wavelength, Material material,
            Grid2DCplxData u,
            Func<double, double, double>? psi = null,
            double shiftX = 0.0, double shiftY = 0.0,
            double shiftKx = 0.0, double shiftKy = 0.0,
            double scaling = 1.0, ModelingDomain domain = ModelingDomain.Spatial)
        {
            Wavelength = wavelength;
            Material = material;
            Domain = domain;
            Scaling = scaling;

            // grid, values, and phase
            UGrid = new GridInfo2D(other: u.GridInfo);
            UValues = new MatrixZ(other: u.Values, deepCopy: true);
            UPhase = new Analyt2DPhase(source: u.Phase);
            // defines interpolation method for U-field
            IntrplMethod = u.IntrplMethod;

            // shifts ...at last
            ShiftX = shiftX;
            ShiftY = shiftY;
            ShiftKx = shiftKx;
            ShiftKy = shiftKy;

            // Psi for future
            Psi = psi ?? ((x, y) => 0.0);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SCField"/> class from a function and grid.
        /// </summary>
        /// <param name="wavelength">Wavelength in vacuum.</param>
        /// <param name="material">Embedding material of the field.</param>
        /// <param name="uFunc">Function that defines the residual field part U.</param>
        /// <param name="uGrid">Sampling grid of the residual field part U.</param>
        /// <param name="uPhase">Analytic phase term associated with the residual field part U (optional).</param>
        /// <param name="psi">Smooth phase part Psi (optional, currently not supported).</param>
        /// <param name="shiftX">Lateral shift in x-direction (optional).</param>
        /// <param name="shiftY">Lateral shift in y-direction (optional).</param>
        /// <param name="shiftKx">Lateral shift in kx-direction (optional).</param>
        /// <param name="shiftKy">Lateral shift in ky-direction (optional).</param>
        /// <param name="scaling">Constant scaling of the field values (optional).</param>
        /// <param name="domain">Modeling domain: spatial (X,Y) or spatial-frequency (Kx,Ky) (optional).</param>
        /// <param name="loopMode">Computational option for loops (optional).</param>
        public SCField(double wavelength, Material material,
            Func<double, double, Complex> uFunc, GridInfo2D uGrid, Analyt2DPhase? uPhase = null,
            Func<double, double, double>? psi = null,
            double shiftX = 0.0, double shiftY = 0.0,
            double shiftKx = 0.0, double shiftKy = 0.0,
            double scaling = 1.0, ModelingDomain domain = ModelingDomain.Spatial,
            LoopMode loopMode = Defaults.LoopOption)
        {
            Wavelength = wavelength;
            Material = material;
            Domain = domain;
            Scaling = scaling;

            // grid, values, and phase
            UGrid = new GridInfo2D(other: uGrid);
            UValues = new Samp2DCplxFunc(f: uFunc).Sample(grid: uGrid, loopMode);
            UPhase = uPhase != null ? new Analyt2DPhase(source: uPhase)
                : new Analyt2DPhase(c1x: 0.0, c1y: 0.0);
            // defines interpolation method for U-field
            IntrplMethod = Defaults.IntrplOption;

            // shifts ... at last
            ShiftX = shiftX;
            ShiftY = shiftY;
            ShiftKx = shiftKx;
            ShiftKy = shiftKy;

            // Psi for future
            Psi = psi ?? ((x, y) => 0.0);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SCField"/> class by copying another instance.
        /// </summary>
        /// <param name="other">Another scalar field as the source.</param>
        /// <param name="copyMode">Copy mode option (deep or shallow copy).</param>
        public SCField(SCField other,
            ArrayCopyMode copyMode = ArrayCopyMode.Deep)
        {
            Wavelength = other.Wavelength;
            Material = new(other.Material);
            Domain = other.Domain;
            Scaling = other.Scaling;

            // grid, values, and phase
            UGrid = other.UGrid is null ? null : new GridInfo2D(other: other.UGrid);
            UValues = other.UValues is null ? null : new MatrixZ(other: other.UValues, deepCopy: copyMode == ArrayCopyMode.Deep);
            UPhase = other.UPhase is null ? null : new Analyt2DPhase(source: other.UPhase);
            // defines the interpolation method for U-field
            IntrplMethod = other.IntrplMethod;

            // shifts ... at last
            ShiftX = other.ShiftX;
            ShiftY = other.ShiftY;
            ShiftKx = other.ShiftKx;
            ShiftKy = other.ShiftKy;

            // Psi for future
            Psi = other.Psi;
        }

        #endregion
        #region methods

        #region ---- sample U ----

        /// <summary>
        /// Samples the residual field U on a specified target uniform grid.
        /// </summary>
        /// <param name="grid">The target uniform grid on which to sample the field.</param>
        /// <param name="includeAnalyticPhase">If <c>true</c>, includes the analytic phase in the sampled values.</param>
        /// <param name="loopMode">The loop-computational mode option for sampling.</param>
        /// <returns>
        /// A <see cref="MatrixZ"/> containing the sampled residual field values on the target grid.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if the underlying field values (<see cref="UValues"/>) are <c>null</c>.
        /// </exception>
        public MatrixZ SampleU(GridInfo2D grid,
            bool includeAnalyticPhase = false,
            LoopMode loopMode = Defaults.LoopOption)
        {
            if (UValues == null) { throw new ArgumentNullException(nameof(UValues)); }
            MatrixZ u = (grid == UGrid) ? new MatrixZ(other: UValues, deepCopy: true)
                : U.FindValues(targetGrid: grid, loopMode: loopMode);
            if (includeAnalyticPhase && UPhase != null)
            {
                MatrixD p = UPhase.Sample(grid, loopMode);
                Complex imgOne = Complex.ImaginaryOne;
                void op(long iRow, long iCol) =>
                    u[iRow, iCol, false] *= Complex.Exp(imgOne * p[iRow, iCol, false]);
                Loop2D loop = new(operation: op,
                    rowStart: 0, rowEnd: grid.Rows,
                    colStart: 0, colEnd: grid.Cols);
                loop.Evaluate(mode: loopMode);
            }
            return u;
        }

        #endregion
        #region ---- sample Psi ----

        // ...

        #endregion
        #region ---- sample ----

        // ...

        #endregion
        #region ---- eigen-info ----

        /// <summary>
        /// Computes the eigen information (normalized spatial frequencies) for the current field.
        /// </summary>
        /// <param name="loopMode">The loop-computational mode option for the calculation.</param>
        /// <remarks>
        /// This method calculates the normalized transverse spatial frequencies (<see cref="Nx"/>, <see cref="Ny"/>)
        /// and the normalized eigenvalues along the z-direction (<see cref="Nz"/>) using the current grid and material.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// Thrown if the sampling grid (<see cref="UGrid"/>) is <c>null</c>.
        /// </exception>
        [MemberNotNull(nameof(Nx), nameof(Ny), nameof(Nz))]
        public void ComputeEigenInfo(LoopMode loopMode = Defaults.LoopOption)
        {
            if (UGrid == null) { throw new ArgumentNullException(nameof(UGrid)); }
            // defines k-domain grid
            GridInfo2D kGrid = new(other: UGrid);
            if (Domain == ModelingDomain.Spatial) { kGrid.GetConjugated(isForward: true); }
            // computes nx and ny
            Ny = kGrid.GetCoordinatesY() / K0;
            Nx = kGrid.GetCoordinatesX() / K0;
            // using UniformLayer for EigenInfo calculation
            UniformLayer freeSpace = new(epsilon: Material.Epsilon, mu: Material.Mu, thickness: 0.0);
            Nz = freeSpace.ComputeNz(wavelength: Wavelength, nx: Nx, ny: Ny,
                loopMode: loopMode);
        }

        #endregion
        #region ---- transform ----

        /// <summary>
        /// Switches the field representation to the spatial frequency (Kx, Ky) domain.
        /// </summary>
        /// <remarks>
        /// Performs a forward 2D FFT on the field data and updates the domain flag.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <see cref="UValues"/>, <see cref="UGrid"/>, or <see cref="UPhase"/> is <c>null</c>.
        /// </exception>
        public void SwitchToKDomain()
        {
            if (Domain == ModelingDomain.SpatialFrequency) { return; }
            if (UValues == null) { throw new ArgumentNullException(nameof(UValues)); }
            if (UGrid == null) { throw new ArgumentNullException(nameof(UGrid)); }
            if (UPhase == null) { throw new ArgumentNullException(nameof(UPhase)); }

            // perform forward transform
            MatrixZ uValues = UValues;
            GridInfo2D uGrid = UGrid;
            double c1x = UPhase.C1x;
            double c1y = UPhase.C1y;
            // FFT call
            Transform.FFT2D(x: ref uValues, grid: ref uGrid,
                cx: ref c1x, cy: ref c1y,
                direction: FFTOptions.Direction.Forward,
                convention: FFTOptions.Convention.ZeroCentered,
                conversion: FFTOptions.Conversion.DataShift,
                copyMode: FFTOptions.CopyMode.Block);
            UPhase.C1x = c1x;
            UPhase.C1y = c1y;

            // change the domain flag
            Domain = ModelingDomain.SpatialFrequency;
        }

        /// <summary>
        /// Switches the field representation to the spatial (X, Y) domain.
        /// </summary>
        /// <remarks>
        /// Performs a backward 2D FFT on the field data and updates the domain flag.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <see cref="UValues"/>, <see cref="UGrid"/>, or <see cref="UPhase"/> is <c>null</c>.
        /// </exception>
        public void SwitchToXDomain()
        {
            if (Domain == ModelingDomain.Spatial) { return; }
            if (UValues == null) { throw new ArgumentNullException(nameof(UValues)); }
            if (UGrid == null) { throw new ArgumentNullException(nameof(UGrid)); }
            if (UPhase == null) { throw new ArgumentNullException(nameof(UPhase)); }

            // perform backward transform
            MatrixZ uValues = UValues;
            GridInfo2D uGrid = UGrid;
            double c1x = UPhase.C1x;
            double c1y = UPhase.C1y;
            // FFT call
            Transform.FFT2D(x: ref uValues, grid: ref uGrid,
                cx: ref c1x, cy: ref c1y,
                direction: FFTOptions.Direction.Backward,
                convention: FFTOptions.Convention.ZeroCentered,
                conversion: FFTOptions.Conversion.DataShift,
                copyMode: FFTOptions.CopyMode.Block);
            UPhase.C1x = c1x;
            UPhase.C1y = c1y;

            // change the domain flag
            Domain = ModelingDomain.Spatial;
        }

        #endregion
        #region ---- resize ----

        /// <summary>
        /// Applies zero-valued central padding to the field data, increasing its size to the specified target dimensions.
        /// </summary>
        /// <param name="targetRows">The target number of rows after padding.</param>
        /// <param name="targetCols">The target number of columns after padding.</param>
        /// <remarks>
        /// The field values and grid information are updated to reflect the new padded size.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// Thrown if the field values (<see cref="UValues"/>) are <c>null</c>.
        /// </exception>
        public void Padding(long targetRows, long targetCols)
        {
            if (UValues == null) { throw new ArgumentNullException(nameof(UValues)); }
            if (UGrid == null) { throw new ArgumentNullException(nameof(UGrid)); }
            UValues = UValues.Padding(targetRows, targetCols);
            UGrid = new(rows: targetRows, cols: targetCols,
                spacingY: UGrid.SpacingY, spacingX: UGrid.SpacingX,
                refPointY: UGrid.StartY - (targetRows - UGrid.Rows) / 2 * UGrid.SpacingY,
                refPointX: UGrid.StartX - (targetCols - UGrid.Cols) / 2 * UGrid.SpacingX,
                refTypeY: GridRefType.Start, refTypeX: GridRefType.Start);
        }

        /// <summary>
        /// Applies central truncation to the field data, reducing its size to the specified target dimensions.
        /// </summary>
        /// <param name="targetRows">The target number of rows after truncation.</param>
        /// <param name="targetCols">The target number of columns after truncation.</param>
        /// <remarks>
        /// The field values and grid information are updated to reflect the new truncated size.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// Thrown if the field values (<see cref="UValues"/>) are <c>null</c>.
        /// </exception>
        public void Truncate(long targetRows, long targetCols)
        {
            if (UValues == null) { throw new ArgumentNullException(nameof(UValues)); }
            if (UGrid == null) { throw new ArgumentNullException(nameof(UGrid)); }
            UValues = UValues.Truncate(targetRows, targetCols);
            UGrid = new(rows: targetRows, cols: targetCols,
                spacingY: UGrid.SpacingY, spacingX: UGrid.SpacingX,
                refPointY: UGrid.StartY + (UGrid.Rows - targetRows) / 2 * UGrid.SpacingY,
                refPointX: UGrid.StartX + (UGrid.Cols - targetCols) / 2 * UGrid.SpacingX,
                refTypeY: GridRefType.Start, refTypeX: GridRefType.Start);
        }

        #endregion
        #region ---- propagate ----

        /// <summary>
        /// Propagates the field in the spatial frequency domain over a specified distance along the z-axis.
        /// </summary>
        /// <param name="d">The propagation distance along the z-axis.</param>
        /// <param name="dx">Optional: Lateral shift in the x-direction after propagation. If <c>null</c>, it is computed automatically.</param>
        /// <param name="dy">Optional: Lateral shift in the y-direction after propagation. If <c>null</c>, it is computed automatically.</param>
        /// <param name="targetDomain">The target modeling domain after propagation (spatial or spatial-frequency).</param>
        /// <param name="loopMode">The loop-computational mode option for propagation.</param>
        /// <param name="xSizeFactor">Padding or truncation factor for field size in the x-domain.</param>
        /// <param name="kSizeFactor">Central filtering factor in the k-domain.</param>
        /// <param name="kEdgeRatio">Smooth edge factor for k-domain truncation, relative to truncation width.</param>
        /// <remarks>
        /// This method handles optional input field padding, domain switching, k-domain truncation, propagation kernel application,
        /// and optional output field truncation and domain switching.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <see cref="UValues"/>, <see cref="UGrid"/>, or <see cref="UPhase"/> is <c>null</c>.
        /// </exception>
        public void Propagate(double d,
            double? dx = null, double? dy = null,
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
                long targetRows = (long)(UGrid.Rows * xSizeFactor);
                long targetCols = (long)(UGrid.Cols * xSizeFactor);
                if ((targetRows - UGrid.Rows) % 2 != 0) { targetRows++; }
                if ((targetCols - UGrid.Cols) % 2 != 0) { targetCols++; }
                Padding(targetRows, targetCols);
            }

            // [possible] input domain handling
            SwitchToKDomain();

            // [possible] truncation in k-domain
            if (kSizeFactor < 1.0)
            {
                // filter width in k-domain: circular aperture filtering
                double wK = Math.Min(UGrid.RangeX, UGrid.RangeY) * kSizeFactor;
                Aperture2D.Ellptical t = new(
                    diameterX: wK,
                    diameterY: wK,
                    edgeWidth: kEdgeRatio * wK,
                    shiftX: 0.0,
                    shiftY: 0.0,
                    scaling: 1.0);
                UValues *= t.Sample(grid: UGrid, loopMode);
            }

            // SPW kernel
            if (xSizeFactor != 1.0 || kSizeFactor != 1.0 || Nz == null || Nx == null || Ny == null)
            { ComputeEigenInfo(loopMode); }
            // automatically handles the center shift in x-domain after propagation
            double kRe = Material.NReal(Wavelength) * K0;
            double kz = Math.Sqrt(kRe * kRe - ShiftKx * ShiftKx - ShiftKy * ShiftKy);
            if (kz == 0.0) throw new DivideByZeroException(nameof(kz));
            dx ??= (ShiftKx / kz) * d;
            dy ??= (ShiftKy / kz) * d;
            MatrixZ uValues = UValues;
            SPW.Propagate2D(wavelength: Wavelength,
                v: ref uValues, // to be modified ... 
                nx: Nx, ny: Ny, nz: Nz, z: d,
                cLinearX: dx.Value, cLinearY: dy.Value,
                loopMode: loopMode);
            // to compensates the additional linear phase in k-domain, linear coefficient should get (-dx, -dy)
            UPhase.C1x -= (dx == null) ? 0.0 : dx.Value;
            UPhase.C1y -= (dy == null) ? 0.0 : dy.Value;

            // [possible] output field truncation
            if (xSizeFactor < 1.0)
            {
                SwitchToXDomain(); // makes sure to truncate in the x-domain
                long targetRows = (long)(UGrid.Rows * xSizeFactor);
                long targetCols = (long)(UGrid.Cols * xSizeFactor);
                if ((UGrid.Rows - targetRows) % 2 != 0) { targetRows--; }
                if ((UGrid.Cols - targetCols) % 2 != 0) { targetCols--; }
                Truncate(targetRows, targetCols);
            }

            // [possible] output domain handling
            if (targetDomain == ModelingDomain.Spatial) { SwitchToXDomain(); }
            else { SwitchToKDomain(); }
        }

        #endregion

        #endregion

        #region derived...

        /// <summary>
        /// Represents a scalar Gaussian field at its waist.
        /// </summary>
        public class Gaussian : SCField
        {
            #region properties

            /// <summary>
            /// Gets or sets the waist radius along the x-direction.
            /// </summary>
            public double WaistRadiusX { get; set; }

            /// <summary>
            /// Gets or sets the waist radius along the y-direction.
            /// </summary>
            public double WaistRadiusY { get; set; }

            #endregion
            #region constructor

            /// <summary>
            /// Initializes a new instance of the <see cref="Gaussian"/> class with default parameters.
            /// </summary>
            internal Gaussian() { }

            /// <summary>
            /// Initializes a new instance of the <see cref="Gaussian"/> class with specific parameters.
            /// </summary>
            /// <param name="wavelength">Wavelength in vacuum.</param>
            /// <param name="material">Embedding material of the field.</param>
            /// <param name="waistRadiusX">Waist radius along the x-direction.</param>
            /// <param name="waistRadiusY">Waist radius along the y-direction.</param>
            /// <param name="grid">Sampling grid information.</param>
            /// <param name="psi">Smooth phase part Psi (currently not supported).</param>
            /// <param name="shiftX">Lateral shift in x-direction; if not null, this value will override the center of the grid of U.</param>
            /// <param name="shiftY">Lateral shift in y-direction; if not null, this value will override the center of the grid of U.</param>
            /// <param name="shiftKx">Lateral shift in kx-direction; if not null, this value will override the center of the grid of U.</param>
            /// <param name="shiftKy">Lateral shift in ky-direction; if not null, this value will override the center of the grid of U.</param>
            /// <param name="scaling">Constant scaling of the field values.</param>
            /// <param name="domain">Modeling domain: spatial (X,Y) or spatial-frequency (Kx,Ky).</param>
            /// <param name="loopMode">Computational option for loops.</param>
            public Gaussian(double wavelength, Material material,
                double waistRadiusX, double waistRadiusY, GridInfo2D grid,
                Func<double, double, double>? psi = null,
                double shiftX = 0.0, double shiftY = 0.0,
                double shiftKx = 0.0, double shiftKy = 0.0,
                double scaling = 1, ModelingDomain domain = ModelingDomain.Spatial,
                LoopMode loopMode = Defaults.LoopOption)
                : base(wavelength: wavelength, material: material,
                      uFunc: (x, y) => Function1D.Gaussian(x, [waistRadiusX])
                      * Function1D.Gaussian(y, [waistRadiusY]),
                      uGrid: grid, uPhase: null, psi: psi,
                      shiftX, shiftY, shiftKx, shiftKy,
                      scaling, domain, loopMode)
            {
                WaistRadiusX = waistRadiusX;
                WaistRadiusY = waistRadiusY;
            }

            #endregion
        }

        /// <summary>
        /// Represents a scalar super-Gaussian field at its waist.
        /// </summary>
        public class SuperGaussian : SCField
        {
            #region properties

            /// <summary>
            /// Gets or sets the waist radius along the x-direction.
            /// </summary>
            public double WaistRadiusX { get; set; }

            /// <summary>
            /// Gets or sets the waist radius along the y-direction.
            /// </summary>
            public double WaistRadiusY { get; set; }

            /// <summary>
            /// Gets or sets the order of the super-Gaussian profile in the x-direction.
            /// </summary>
            public long OrderX { get; set; }

            /// <summary>
            /// Gets or sets the order of the super-Gaussian profile in the y-direction.
            /// </summary>
            public long OrderY { get; set; }

            #endregion
            #region constructor

            /// <summary>
            /// Initializes a new instance of the <see cref="SuperGaussian"/> class with default parameters.
            /// </summary>
            internal SuperGaussian() { }

            /// <summary>
            /// Initializes a new instance of the <see cref="SuperGaussian"/> class with specific parameters.
            /// </summary>
            /// <param name="wavelength">Wavelength in vacuum.</param>
            /// <param name="material">Embedding material of the field.</param>
            /// <param name="waistRadiusX">Waist radius along the x-direction.</param>
            /// <param name="waistRadiusY">Waist radius along the y-direction.</param>
            /// <param name="orderX">Order of the super-Gaussian profile in the x-direction.</param>
            /// <param name="orderY">Order of the super-Gaussian profile in the y-direction.</param>
            /// <param name="grid">Sampling grid information.</param>
            /// <param name="psi">Smooth phase part Psi (currently not supported).</param>
            /// <param name="shiftX">Lateral shift in x-direction; if not null, this value will override the center of the grid of U.</param>
            /// <param name="shiftY">Lateral shift in y-direction; if not null, this value will override the center of the grid of U.</param>
            /// <param name="shiftKx">Lateral shift in kx-direction; if not null, this value will override the center of the grid of U.</param>
            /// <param name="shiftKy">Lateral shift in ky-direction; if not null, this value will override the center of the grid of U.</param>
            /// <param name="scaling">Constant scaling of the field values.</param>
            /// <param name="domain">Modeling domain: spatial (X,Y) or spatial-frequency (Kx,Ky).</param>
            /// <param name="loopMode">Computational option for loops.</param>
            public SuperGaussian(double wavelength, Material material,
                double waistRadiusX, double waistRadiusY,
                long orderX, long orderY, GridInfo2D grid,
                Func<double, double, double>? psi = null,
                double shiftX = 0.0, double shiftY = 0.0,
                double shiftKx = 0.0, double shiftKy = 0.0,
                double scaling = 1, ModelingDomain domain = ModelingDomain.Spatial,
                LoopMode loopMode = Defaults.LoopOption)
                : base(wavelength: wavelength, material: material,
                      uFunc: (x, y) => Function1D.SuperGaussian(x, [waistRadiusX, orderX])
                      * Function1D.SuperGaussian(y, [waistRadiusY, orderY]),
                      uGrid: grid, uPhase: null, psi: psi,
                      shiftX, shiftY, shiftKx, shiftKy,
                      scaling, domain, loopMode)
            {
                WaistRadiusX = waistRadiusX;
                WaistRadiusY = waistRadiusY;
                OrderX = orderX;
                OrderY = orderY;
            }

            #endregion
        }

        /// <summary>
        /// Represents a truncated plane wave field.
        /// </summary>
        public class PlaneWave : SCField
        {
            #region properties

            /// <summary>
            /// Gets or sets the truncation diameter of the field along the x-direction.
            /// </summary>
            public double DiameterX { get; set; }

            /// <summary>
            /// Gets or sets the truncation diameter of the field along the y-direction.
            /// </summary>
            public double DiameterY { get; set; }

            /// <summary>
            /// Gets or sets the absolute edge width (half within, half outside).
            /// </summary>
            public double EdgeWidth { get; set; }

            /// <summary>
            /// Gets or sets the shape of the truncation aperture.
            /// </summary>
            public ApertureShape Shape { get; set; }

            #endregion
            #region constructor

            /// <summary>
            /// Initializes a new instance of the <see cref="PlaneWave"/> class with default parameters.
            /// </summary>
            internal PlaneWave() { }

            /// <summary>
            /// Initializes a new instance of the <see cref="PlaneWave"/> class with specific parameters.
            /// </summary>
            /// <param name="wavelength">Wavelength in vacuum.</param>
            /// <param name="material">Embedding material of the field.</param>
            /// <param name="diameterX">Truncation diameter of the field along the x-direction.</param>
            /// <param name="diameterY">Truncation diameter of the field along the y-direction.</param>
            /// <param name="grid">Sampling grid of the residual field part U.</param>
            /// <param name="shape">Shape of the truncation aperture.</param>
            /// <param name="edge">Absolute edge width (half within, half outside).</param>
            /// <param name="psi">Smooth phase part Psi (currently not supported).</param>
            /// <param name="shiftX">Lateral shift in x-direction; if not null, this value will override the center of the grid of U.</param>
            /// <param name="shiftY">Lateral shift in y-direction; if not null, this value will override the center of the grid of U.</param>
            /// <param name="shiftKx">Lateral shift in kx-direction; if not null, this value will override the center of the grid of U.</param>
            /// <param name="shiftKy">Lateral shift in ky-direction; if not null, this value will override the center of the grid of U.</param>
            /// <param name="scaling">Constant scaling of the field values.</param>
            /// <param name="domain">Modeling domain: spatial (X,Y) or spatial-frequency (Kx,Ky).</param>
            /// <param name="loopMode">Computational option for loops.</param>
            public PlaneWave(double wavelength, Material material,
                double diameterX, double diameterY, GridInfo2D grid,
                ApertureShape shape = ApertureShape.Elliptical, double edge = 0.0,
                Func<double, double, double>? psi = null,
                double shiftX = 0.0, double shiftY = 0.0,
                double shiftKx = 0.0, double shiftKy = 0.0,
                double scaling = 1.0, ModelingDomain domain = ModelingDomain.Spatial,
                LoopMode loopMode = Defaults.LoopOption)
            {
                Wavelength = wavelength;
                Material = material;
                Domain = domain;
                Scaling = scaling;

                // grid, values, and phase
                UGrid = new GridInfo2D(other: grid);
                DiameterX = diameterX;
                DiameterY = diameterY;
                EdgeWidth = edge;
                Shape = shape;
                Samp2DRealFunc a;
                switch (shape)
                {
                    case ApertureShape.Elliptical:
                        a = new(f: Function2D.CosEdgeEllipse, p: [0.5 * DiameterX, 0.5 * DiameterY, EdgeWidth]);
                        break;
                    case ApertureShape.Rectangular:
                        a = new(f: (x, y) => Function1D.CosEdgeRectangle(x, [DiameterX, EdgeWidth]) * Function1D.CosEdgeRectangle(y, [DiameterY, EdgeWidth]));
                        break;
                    default: goto case ApertureShape.Elliptical;
                }
                UValues = new MatrixZ(part: a.Sample(grid, loopMode), option: ComplexPart.RealPart);
                UPhase = new Analyt2DPhase(c1x: 0.0, c1y: 0.0);
                // defines interpolation method for U-field
                IntrplMethod = Defaults.IntrplOption;

                // shifts ... at last
                ShiftX = shiftX;
                ShiftY = shiftY;
                ShiftKx = shiftKx;
                ShiftKy = shiftKy;

                // dummy
                Psi = psi ?? ((x, y) => 0.0);
            }

            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        public class SphericalWave : SCField
        {
            #region properties

            /// <summary>
            /// distance from the point source
            /// </summary>
            public double SourceDistance { get; set; }

            /// <summary>
            /// Gets or sets the truncation diameter of the field along the x-direction.
            /// </summary>
            public double DiameterX { get; set; }

            /// <summary>
            /// Gets or sets the truncation diameter of the field along the y-direction.
            /// </summary>
            public double DiameterY { get; set; }

            /// <summary>
            /// Gets or sets the absolute edge width (half within, half outside).
            /// </summary>
            public double EdgeWidth { get; set; }

            /// <summary>
            /// Gets or sets the shape of the truncation aperture.
            /// </summary>
            public ApertureShape Shape { get; set; }


            #endregion
            #region constructors

            /// <summary>
            /// Represents a spherical wave in the system.
            /// </summary>
            /// <remarks>This class is intended for internal use only and is not accessible outside
            /// the assembly.</remarks>
            internal SphericalWave() { }

            /// <summary>
            /// Initializes a new instance of the <see cref="SphericalWave"/> class.
            /// </summary>
            /// <param name="wavelength">Wavelength in vacuum.</param>
            /// <param name="material">Embedding material of the field.</param>
            /// <param name="z">Distance from the point source (propagation distance).</param>
            /// <param name="alphaX">Angular aperture half-angle along the x-direction.</param>
            /// <param name="alphaY">Angular aperture half-angle along the y-direction.</param>
            /// <param name="grid">Sampling grid information for the field.</param>
            /// <param name="shape">Shape of the truncation aperture (default: Elliptical).)</param>
            /// <param name="edge">Absolut the edge width for aperture smoothing (default: 0.0).</param>
            /// <param name="psi">Smooth phase part Psi (TODO: HFT?).</param>
            /// <param name="shiftX">Lateral shift in x-direction; if not null, this value will override the center of the grid of U.</param>
            /// <param name="shiftY">Lateral shift in y-direction; if not null, this value will override the center of the grid of U.</param>
            /// <param name="shiftKx">Lateral shift in kx-direction; if not null, this value will override the center of the grid of U.</param>
            /// <param name="shiftKy">Lateral shift in ky-direction; if not null, this value will override the center of the grid of U.</param>
            /// <param name="scaling">Constant scaling factor of the field values (default: 1.0).</param>
            /// <param name="domain">Modeling domain: spatial (X,Y) or spatial-frequency (Kx, Ky) (default: Spatial).</param>
            /// <param name="loopMode">Computational option for loops (default: Defaults.LoopOption).</param>
            public SphericalWave(double wavelength, Material material,
                double z, double alphaX, double alphaY, GridInfo2D grid,
                ApertureShape shape = ApertureShape.Elliptical, double edge = 0.0,
                Func<double, double, double>? psi = null,
                double shiftX = 0.0, double shiftY = 0.0,
                double shiftKx = 0.0, double shiftKy = 0.0,
                double scaling = 1.0, ModelingDomain domain = ModelingDomain.Spatial,
                LoopMode loopMode = Defaults.LoopOption)
            {
                // Precompute constants outside the sampling loop for efficiency
                double k0 = 2.0 * Math.PI / wavelength;
                double nReal = material.NReal(wavelength);
                double k = k0 * nReal;
                double zAbs = Math.Abs(z);
                double zSq = z * z;

                // Use a local function to avoid repeated allocations and redundant calculations
                Complex uFunc(double x, double y)
                {
                    double xSq = x * x;
                    double ySq = y * y;
                    double rSq = zSq + xSq + ySq;
                    double r = Math.Sqrt(rSq);

                    // Calculate the spherical wave amplitude
                    double amplitude;
                    switch (shape)
                    {
                        case ApertureShape.Elliptical:
                            amplitude = Function2D.CosEdgeEllipse(
                                x, y, [0.5 * DiameterX, 0.5 * DiameterY, EdgeWidth]) * zAbs / r;
                            break;
                        case ApertureShape.Rectangular:
                            amplitude = Function1D.CosEdgeRectangle(x, [DiameterX, EdgeWidth]) * 
                                        Function1D.CosEdgeRectangle(y, [DiameterY, EdgeWidth]) * zAbs / r;
                            break;
                        default:
                            amplitude = Function2D.CosEdgeEllipse(
                                x, y, [0.5 * DiameterX, 0.5 * DiameterY, EdgeWidth]) * zAbs / r;
                            break;
                    }

                    // Calculate the spherical wave phase
                    double phase = k * r;

                    return amplitude * Complex.Exp(Complex.ImaginaryOne * Math.Sign(z) *phase);
                }

                // Properties setting
                Wavelength = wavelength;
                Material = material;
                Domain = domain;
                Scaling = scaling;
                SourceDistance = zAbs;
                DiameterX = 2.0 * Math.Tan(alphaX) * SourceDistance;
                DiameterY = 2.0 * Math.Tan(alphaY) * SourceDistance;
                EdgeWidth = edge;
                Shape = shape;

                // grid, values, and phase
                UGrid = new GridInfo2D(other: grid);
                UValues = new Samp2DCplxFunc(f: uFunc).Sample(grid: UGrid, loopMode);
                UPhase = new Analyt2DPhase(c1x: 0.0, c1y: 0.0);
                // defines interpolation method for U-field
                IntrplMethod = Defaults.IntrplOption;

                // shifts
                ShiftX = shiftX;
                ShiftY = shiftY;
                ShiftKx = shiftKx;
                ShiftKy = shiftKy;

                // Psi for future
                Psi = psi ?? ((x, y) => 0.0);
            }

            #endregion
        }

        #endregion
    }
}
