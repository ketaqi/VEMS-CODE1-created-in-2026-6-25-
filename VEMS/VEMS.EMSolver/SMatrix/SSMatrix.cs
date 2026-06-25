using System.Diagnostics.CodeAnalysis;
using VEMS.MathCore;
using Complex = System.Numerics.Complex;

namespace VEMS.EMSolver
{

    /// <summary>
    /// Represents a 1D S-matrix for electromagnetic waveguide calculations.
    /// Encapsulates properties for wavelength, refractive indices, thickness, and grid information.
    /// Provides delegates for S-matrix elements for left and right interfaces.
    /// </summary>
    public class SSMat1D
    {
        #region properties

        #region ---- parameters ----

        /// <summary>
        /// Gets or sets the wavelength in vacuum (in meters).
        /// </summary>
        public double Wavelength { get; set; } = 632.8E-9;

        /// <summary>
        /// Gets or sets the refractive index on the left side of the waveguide.
        /// </summary>
        public Complex NLeft { get; set; } = Complex.One;

        /// <summary>
        /// Gets or sets the refractive index of the waveguide.
        /// </summary>
        public Complex NWaveguide { get; set; } = 1.8;

        /// <summary>
        /// Gets or sets the thickness of the waveguide (in meters).
        /// </summary>
        public double Thickness { get; set; } = 0.5E-3;

        /// <summary>
        /// Gets or sets the refractive index on the right side of the waveguide.
        /// </summary>
        public Complex NRight { get; set; } = Complex.One;

        #endregion
        #region ---- grid info ----

        /// <summary>
        /// Gets or sets the spatial grid information for the x-direction.
        /// </summary>
        public GridInfo1D? GridX { get; set; }

        /// <summary>
        /// Gets or sets the grid information for the kx (wavevector) domain.
        /// </summary>
        public GridInfo1D? GridKx { get; set; }

        #endregion
        #region ---- surfaces ----

        /// <summary>
        /// Gets or sets the locally varying S11 matrix-element of the left interface.
        /// </summary>
        public Func<double, MatrixZ>? SLeft11 { get; set; }

        /// <summary>
        /// Gets or sets the locally varying S21 matrix-element of the left interface.
        /// </summary>
        public Func<double, MatrixZ>? SLeft21 { get; set; }

        /// <summary>
        /// Gets or sets the locally varying S12 matrix-element of the left interface.
        /// </summary>
        public Func<double, MatrixZ>? SLeft12 { get; set; }

        /// <summary>
        /// Gets or sets the locally varying S22 matrix-element of the left interface.
        /// </summary>
        public Func<double, MatrixZ>? SLeft22 { get; set; }

        /// <summary>
        /// Gets or sets the locally varying S11 matrix-element of the right interface.
        /// </summary>
        public Func<double, MatrixZ>? SRight11 { get; set; }

        /// <summary>
        /// Gets or sets the locally varying S21 matrix-element of the right interface.
        /// </summary>
        public Func<double, MatrixZ>? SRight21 { get; set; }

        /// <summary>
        /// Gets or sets the locally varying S12 matrix-element of the right interface.
        /// </summary>
        public Func<double, MatrixZ>? SRight12 { get; set; }

        /// <summary>
        /// Gets or sets the locally varying S22 matrix-element of the right interface.
        /// </summary>
        public Func<double, MatrixZ>? SRight22 { get; set; }

        #endregion
        #region ---- SS-matrices ----

        internal MatrixZ? SSLeft11 { get; set; }
        internal MatrixZ? SSLeft21 { get; set; }
        internal MatrixZ? SSLeft12 { get; set; }
        internal MatrixZ? SSLeft22 { get; set; }
        internal MatrixZ? SSRight11 { get; set; }
        internal MatrixZ? SSRight21 { get; set; }
        internal MatrixZ? SSRight12 { get; set; }
        internal MatrixZ? SSRight22 { get; set; }


        internal MatrixZ? PLR { get; set; }
        internal MatrixZ? PRL { get; set; }


        //internal MatrixZ? QLeft11 { get; set; }
        //internal MatrixZ? QLeft12 { get; set; }
        //internal MatrixZ? QRight21 { get; set; }
        //internal MatrixZ? QRight22 { get; set; }


        internal VectorZ? CLeftInMinus { get; set; }
        internal VectorZ? CRightInPlus { get; set; }

        #endregion

        #endregion
        #region constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SSMat1D"/> class with default values.
        /// </summary>
        internal SSMat1D() { }


        /// <summary>
        /// Initializes a new instance of the <see cref="SSMat1D"/> class with the specified parameters.
        /// </summary>
        /// <param name="wavelength">The wavelength in vacuum (in meters).</param>
        /// <param name="nLeft">The refractive index on the left side of the waveguide.</param>
        /// <param name="nWaveguide">The refractive index of the waveguide.</param>
        /// <param name="thickness">The thickness of the waveguide (in meters).</param>
        /// <param name="nRight">The refractive index on the right side of the waveguide.</param>
        public SSMat1D(double wavelength,
            Complex nLeft,
            Complex nWaveguide, double thickness,
            Complex nRight)
        {
            Wavelength = wavelength;
            NLeft = nLeft;
            NWaveguide = nWaveguide;
            Thickness = thickness;
            NRight = nRight;
        }

        #endregion
        #region methods

        /// <summary>
        /// Initializes the spatial grid information for the x-direction.
        /// Creates a new <see cref="GridInfo1D"/> instance with the specified number of grid points,
        /// grid spacing, and center reference point.
        /// </summary>
        /// <param name="nx">The number of grid points in the x-direction.</param>
        /// <param name="xIn">The center coordinate for the x-grid.</param>
        /// <param name="dx">The spacing between grid points.</param>
        [MemberNotNull(nameof(GridX))]
        internal void InitGridX(long nx, double xIn, double dx)
        {
            GridX = new(n: nx, spacing: dx, refPoint: xIn, refType: GridRefType.Center);
        }


        /// <summary>
        /// Initializes the grid information for the kx (wavevector) domain.
        /// Creates a new <see cref="GridInfo1D"/> instance with the specified number of grid points,
        /// grid spacing, and center reference point.
        /// </summary>
        /// <param name="nkx">The number of grid points in the kx domain.</param>
        /// <param name="kxIn">The center coordinate for the kx-grid.</param>
        /// <param name="dkx">The spacing between grid points in the kx domain.</param>
        [MemberNotNull(nameof(GridKx))]
        internal void InitGridKx(long nkx, double kxIn, double dkx)
        {
            GridKx = new(n: nkx, spacing: dkx, refPoint: kxIn, refType: GridRefType.Center);
        }


        #endregion
        #region derived classes

        /// <summary>
        /// Represents a bare waveguide with specified optical and geometric parameters.
        /// </summary>
        /// <remarks>A bare waveguide is a one-dimensional structure characterized by a core material 
        /// (the waveguide) sandwiched between two cladding materials with different refractive indices. This class
        /// models the waveguide's optical properties based on the provided refractive indices  and geometric thickness.
        /// It is typically used in simulations of optical wave propagation.</remarks>
        /// <remarks>
        /// Initializes a new instance of the <see cref="BareWaveguide"/> class
        /// representing a bare waveguide with specified parameters.
        /// </remarks>
        /// <param name="wavelength">The wavelength in vacuum (in meters).</param>
        /// <param name="nLeft">The refractive index on the left side of the waveguide.</param>
        /// <param name="nWaveguide">The refractive index of the waveguide.</param>
        /// <param name="thickness">The thickness of the waveguide (in meters).</param>
        /// <param name="nRight">The refractive index on the right side of the waveguide.</param>
        public class BareWaveguide(
            double wavelength,
            Complex nLeft,
            Complex nWaveguide,
            double thickness,
            Complex nRight) : SSMat1D(wavelength, nLeft, nWaveguide, thickness, nRight)
        {
            #region properties

            internal new Complex SLeft11 {  get; set; }
            internal new Complex SLeft21 {  get; set; }
            internal new Complex SLeft12 {  get; set; }
            internal new Complex SLeft22 { get; set; }


            internal new Complex SRight11 {  get; set; }
            internal new Complex SRight21 {  get; set; }
            internal new Complex SRight12 {  get; set; }
            internal new Complex SRight22 {  get; set; }

            #endregion
            #region constructors

            // ...

            #endregion
            #region methods

            /// <summary>
            /// Initializes the spatial and wavevector grids for the bare waveguide.
            /// Calculates the grid spacing in the x-direction based on the input wavevector and waveguide parameters.
            /// The x-grid is centered at <paramref name="xIn"/> with <paramref name="nx"/> points and spacing <c>xStep</c>.
            /// The kx-grid is initialized with a single point at <paramref name="kxIn"/>.
            /// </summary>
            /// <param name="kxIn">Input wavevector component in the x-direction.</param>
            /// <param name="xIn">Center coordinate for the x-grid.</param>
            /// <param name="nx">Number of grid points in the x-direction (default is 15).</param>
            public void InitGrids(double kxIn, double xIn, long nx = 15)
            {
                double k0 = 2.0 * Math.PI / Wavelength;
                double kw = k0 * NWaveguide.Real;

                double alpha = Math.Asin(kxIn / kw);
                double xStep = Thickness * Math.Tan(alpha);
                double dx = Math.Abs(xStep);

                InitGridX(nx, xIn, dx);
                InitGridKx(1, kxIn, 1.0);
            }


            /// <summary>
            /// Calculates the surface S-matrices for the left and right interfaces of the bare waveguide.
            /// Computes Fresnel coefficients for both interfaces and converts them into 1x1 complex matrix form.
            /// Assigns constant functions for spatially varying S-matrix elements, which return the computed matrices.
            /// </summary>
            /// <param name="kxIn">Input wavevector component in the x-direction.</param>
            /// <param name="polarization">Polarization mode for the calculation (default is TE).</param>
            [MemberNotNull(nameof(SLeft11), nameof(SLeft21), 
                nameof(SLeft12), nameof(SLeft22),
                nameof(SRight11), nameof(SRight21),
                nameof(SRight12), nameof(SRight22))]
            public void CalcSMats(double kxIn,
                InPlanePolMode polarization = InPlanePolMode.TE)
            {
                (SLeft11, SLeft21, SLeft12, SLeft22) =
                    FresnelCalculator.ComputeFullSMatrix(
                        wavelength: Wavelength,
                        n1: NLeft, n2: NWaveguide, kx: kxIn,
                        polarization: polarization);

                //(SLeft11, SLeft21) = FresnelCalculator.ComputeHalfSMatrix(
                //    wavelength: Wavelength,
                //    n1: NLeft, n2: NWaveguide, kx: kxIn,
                //    polarization: polarization);
                //(SLeft22, SLeft12) = FresnelCalculator.ComputeHalfSMatrix(
                //    wavelength: Wavelength,
                //    n1: NWaveguide, n2: NLeft, kx: kxIn,
                //    polarization: polarization);

                (SRight11, SRight21, SRight12, SRight22) =
                    FresnelCalculator.ComputeFullSMatrix(
                        wavelength: Wavelength,
                        n1: NWaveguide, n2: NRight, kx: kxIn,
                        polarization: polarization);

                //(SRight11, SRight21) = FresnelCalculator.ComputeHalfSMatrix(
                //    wavelength: Wavelength,
                //    n1: NWaveguide, n2: NRight, kx: kxIn,
                //    polarization: polarization);
                //(SRight22, SRight12) = FresnelCalculator.ComputeHalfSMatrix(
                //    wavelength: Wavelength,
                //    n1: NRight, n2: NWaveguide, kx: kxIn,
                //    polarization: polarization);
            }


            /// <summary>
            /// Builds the spatially sampled S-matrices and propagation matrices for the bare waveguide.
            /// Initializes diagonal matrices for the left and right interface S-matrix elements,
            /// and constructs propagation matrices for left-to-right and right-to-left directions.
            /// </summary>
            /// <param name="kxIn">Input wavevector component in the x-direction.</param>
            /// <exception cref="NullReferenceException">
            /// Thrown if <see cref="GridX"/> is <c>null</c>, indicating that the spatial grid has not been initialized.
            /// </exception>
            public void BuildSSMats(double kxIn)
            {
                if (GridX == null) { throw new NullReferenceException(nameof(GridX)); }
                long nx = GridX.Count;

                // Left
                SSLeft11 = new MatrixZ(rows: nx, cols: nx, mode: ArrayInitMode.Calloc);
                SSLeft21 = new MatrixZ(rows: nx, cols: nx, mode: ArrayInitMode.Calloc);
                SSLeft12 = new MatrixZ(rows: nx, cols: nx, mode: ArrayInitMode.Calloc);
                SSLeft22 = new MatrixZ(rows: nx, cols: nx, mode: ArrayInitMode.Calloc);
                for (long ix = 0; ix < nx; ix++)
                {
                    // local s-matrices at current position
                    Complex s11 = SLeft11;
                    Complex s21 = SLeft21;
                    Complex s12 = SLeft12;
                    Complex s22 = SLeft22;
                    // filling diagonal
                    SSLeft11[ix, ix] = s11;
                    SSLeft21[ix, ix] = s21;
                    SSLeft12[ix, ix] = s12;
                    SSLeft22[ix, ix] = s22;
                }

                // Right
                SSRight11 = new MatrixZ(rows: nx, cols: nx, mode: ArrayInitMode.Calloc);
                SSRight21 = new MatrixZ(rows: nx, cols: nx, mode: ArrayInitMode.Calloc);
                SSRight12 = new MatrixZ(rows: nx, cols: nx, mode: ArrayInitMode.Calloc);
                SSRight22 = new MatrixZ(rows: nx, cols: nx, mode: ArrayInitMode.Calloc);
                for (long ix = 0; ix < nx; ix++)
                {
                    // local s-matrices at current position
                    Complex s11 = SRight11;
                    Complex s21 = SRight21;
                    Complex s12 = SRight12;
                    Complex s22 = SRight22;
                    // filling diagonal
                    SSRight11[ix, ix] = s11;
                    SSRight21[ix, ix] = s21;
                    SSRight12[ix, ix] = s12;
                    SSRight22[ix, ix] = s22;
                }

                // propagation
                double k0 = 2.0 * Math.PI / Wavelength;
                double kw = k0 * NWaveguide.Real;
                double alpha = Math.Asin(kxIn / kw);
                double xStep = Thickness * Math.Tan(alpha);
                double OPL = kw * Thickness / Math.Cos(alpha);
                Complex p = Complex.Exp(Complex.ImaginaryOne * OPL);
                double kz = Math.Sqrt(kw * kw - kxIn * kxIn);
                Complex pz = Complex.Exp(Complex.ImaginaryOne * kz * Thickness);
                Complex pxz = Complex.Exp(Complex.ImaginaryOne * (kz * Thickness + kxIn * xStep));
                
                PLR = new MatrixZ(rows: nx, cols: nx, mode: ArrayInitMode.Calloc);
                PRL = new MatrixZ(rows: nx, cols: nx, mode: ArrayInitMode.Calloc);

                for (long ix = 0; ix < nx; ix++)
                {
                    // input index => column index ...
                    long iIn = ix;
                    // converts to position
                    double xIn = GridX[iIn];
                    // finds the corresponding output position
                    double xOut = xIn + xStep;
                    // converts to index
                    long iOut = xStep == 0.0 ? iIn :
                        (long)Math.Round((xOut - GridX.Start) / GridX.Spacing);
                    //Printer.WriteLine($"At input index {iIn}, output index = {iOut}");
                    // output index => row index
                    if (iOut >= 0 && iOut < GridX.Count)
                    {
                        PLR[iOut, iIn, false] = pz;
                        PRL[iOut, iIn, false] = pz;
                    }
                }
            }


            /// <summary>
            /// 
            /// </summary>
            /// <param name="cLInPlus"></param>
            /// <param name="cRInMinus"></param>
            /// <returns></returns>
            /// <exception cref="NullReferenceException"></exception>
            public (VectorZ, VectorZ) Solve(VectorZ cLInPlus, VectorZ cRInMinus)
            {
                if (GridX == null) { throw new NullReferenceException(nameof(GridX)); }
                if (PRL == null) { throw new NullReferenceException(nameof(PRL)); }
                if (PLR == null) { throw new NullReferenceException(nameof(PLR)); }
                if (SSLeft11 == null) { throw new NullReferenceException(nameof(SSLeft11)); }
                if (SSLeft12 == null) { throw new NullReferenceException(nameof(SSLeft12)); }
                if (SSLeft21 == null) { throw new NullReferenceException(nameof(SSLeft21)); }
                if (SSLeft22 == null) { throw new NullReferenceException(nameof(SSLeft22)); }
                if (SSRight11 == null) { throw new NullReferenceException(nameof(SSRight11)); }
                if (SSRight12 == null) { throw new NullReferenceException(nameof(SSRight12)); }
                if (SSRight21 == null) { throw new NullReferenceException(nameof(SSRight21)); }
                if (SSRight22 == null) { throw new NullReferenceException(nameof(SSRight22)); }
                long nx = GridX.Count;

                // inside
                VectorZ cLInMinus;// = new(count: nx);
                VectorZ cRInPlus;// = new(count: nx);
                // output
                VectorZ cLOutMinus;// = new(count: nx);
                VectorZ cROutPlus;// = new(count: nx);
                // identity
                MatrixZ idty = LinAlg.DiagonalMatrixHelper.GenerateDenseMatrixZ(nx, 1.0);

                // aux. variables
                MatrixZ qRight21 = LinAlg.Dot(PRL, SSRight21);
                MatrixZ qLeft12 = LinAlg.Dot(PLR, SSLeft12);
                MatrixZ qRight22 = LinAlg.Dot(PRL, SSRight22);
                MatrixZ qLeft11 = LinAlg.Dot(PLR, SSLeft11);
                MatrixZ qRight21_qLeft11 = LinAlg.Dot(qRight21, qLeft11);
                MatrixZ qLeft12_qRight22 = LinAlg.Dot(qLeft12, qRight22);


                // linear systems
                MatrixZ a1 = idty - LinAlg.Dot(qRight21, qLeft12);
                VectorZ b1 = LinAlg.Dot(qRight22, cRInMinus) + LinAlg.Dot(qRight21_qLeft11, cLInPlus);
                cLInMinus = LinAlg.LinearSolve(a1, b1);
                MatrixZ a2 = idty - LinAlg.Dot(qLeft12, qRight21);
                VectorZ b2 = LinAlg.Dot(qLeft12_qRight22, cRInMinus) + LinAlg.Dot(qLeft11, cLInPlus);
                cRInPlus = LinAlg.LinearSolve(a2, b2);

                // final output
                cLOutMinus = LinAlg.Dot(SSLeft21, cLInPlus) + LinAlg.Dot(SSLeft22, cLInMinus);
                cROutPlus = LinAlg.Dot(SSRight11, cRInPlus) + LinAlg.Dot(SSRight12, cRInMinus);
                return (cLOutMinus, cROutPlus);
            }


            #endregion
        }


        /// <summary>
        /// for normal incidence only !!!
        /// </summary>
        public class SimpDiffWaveguide(
            double wavelength,
            Complex nLeft,
            Complex nWaveguide,
            double thickness,
            Complex nRight) : SSMat1D(wavelength, nLeft, nWaveguide, thickness, nRight)
        {
            #region properties

            internal Periodic1DLayer? DiffLayerLeft { get; set; }
            internal Periodic1DLayer? DiffLayerRight { get; set; }

            internal new MatrixZ? SLeft11 { get; set; }
            internal new MatrixZ? SLeft21 { get; set; }
            internal new MatrixZ? SLeft12 { get; set; }
            internal new MatrixZ? SLeft22 { get; set; }

            internal new MatrixZ? SRight11 { get; set; }
            internal new MatrixZ? SRight21 { get; set; }
            internal new MatrixZ? SRight12 { get; set; }
            internal new MatrixZ? SRight22 { get; set; }

            #endregion
            #region constructors

            /// <summary>
            /// Initializes a new instance of the <see cref="SimpDiffWaveguide"/> class.
            /// Represents a simple diffractive waveguide with optional periodic layers on the left and right interfaces.
            /// </summary>
            /// <param name="wavelength">The wavelength in vacuum (in meters).</param>
            /// <param name="nLeft">The refractive index on the left side of the waveguide.</param>
            /// <param name="nWaveguide">The refractive index of the waveguide core.</param>
            /// <param name="thickness">The thickness of the waveguide (in meters).</param>
            /// <param name="nRight">The refractive index on the right side of the waveguide.</param>
            /// <param name="diffLayerLeft">Optional periodic diffractive layer on the left interface. Default is <c>null</c>.</param>
            /// <param name="diffLayerRight">Optional periodic diffractive layer on the right interface. Default is <c>null</c>.</param>
            public SimpDiffWaveguide(double wavelength,
                Complex nLeft, Complex nWaveguide, double thickness, Complex nRight,
                Periodic1DLayer? diffLayerLeft = null,
                Periodic1DLayer? diffLayerRight = null)
                : this(wavelength, nLeft, nWaveguide, thickness, nRight)
            {
                DiffLayerLeft = diffLayerLeft;
                DiffLayerRight = diffLayerRight;
            }

            #endregion
            #region methods

            /// <summary>
            /// Initializes the spatial and wavevector grids for the simple diffractive waveguide.
            /// Calculates the grid spacing in the x-direction based on the grating period and waveguide parameters.
            /// The x-grid is centered at <paramref name="xIn"/> with <paramref name="nx"/> points and spacing <c>xStep</c>.
            /// The kx-grid is initialized with <paramref name="nKx"/> points, centered at 0, with spacing <c>dKx</c>.
            /// </summary>
            /// <param name="xIn">Center coordinate for the x-grid.</param>
            /// <param name="nx">Number of grid points in the x-direction (default is 15).</param>
            /// <param name="nKx">Number of grid points in the kx domain (default is 3).</param>
            public void InitGrids(double xIn, 
                long nx = 15, long nKx = 3)
            {
                var diffLayerRight = DiffLayerRight ?? throw new NullReferenceException(nameof(DiffLayerRight));
                double period = diffLayerRight.Period;

                double k0 = 2.0 * Math.PI / Wavelength;
                double kw = k0 * NWaveguide.Real;

                double dKx = 2.0 * Math.PI / DiffLayerRight.Period;
                double alpha = Math.Asin(dKx / kw);
                double xStep = Thickness * Math.Tan(alpha);

                InitGridX(nx, xIn, xStep);
                InitGridKx(nKx, 0.0, dKx);
            }


            /// <summary>
            /// Calculates the S-matrices for the left and right interfaces of a simple diffractive waveguide.
            /// For the left interface, fills diagonal S-matrix elements using Fresnel coefficients for each kx.
            /// For the right interface, uses RCWA to compute the full S-matrix for the diffractive grating and
            /// maps the RCWA S-matrix blocks to the local S-matrix elements.
            /// </summary>
            /// <param name="polarization">Polarization mode for the calculation (default is TE).</param>
            /// <param name="fieldSampling">Number of field sampling points for RCWA (default is 51).</param>
            /// <param name="mediumSampling">Number of medium sampling points for RCWA (default is 51).</param>
            /// <exception cref="NullReferenceException">
            /// Thrown if <see cref="GridKx"/> or <see cref="DiffLayerRight"/> is <c>null</c>.
            /// </exception>
            [MemberNotNull(nameof(SLeft11), nameof(SLeft21),
                nameof(SLeft12), nameof(SLeft22),
                nameof(SRight11), nameof(SRight21),
                nameof(SRight12), nameof(SRight22))]
            public void CalcSMats(
                InPlanePolMode polarization = InPlanePolMode.TE,
                long fieldSampling = 51, long mediumSampling = 51)
            {
                if (GridKx == null) { throw new NullReferenceException(nameof(GridKx)); }
                if (DiffLayerRight == null) { throw new NullReferenceException(nameof(DiffLayerRight)); }
                
                long nKx = GridKx.Count;
                long ctrIdx = (nKx - 1) / 2;
                double dKx = 2.0 * Math.PI / DiffLayerRight.Period;

                // surface on the left is supposed to be null ...
                SLeft11 = new MatrixZ(rows: nKx, cols: nKx);
                SLeft21 = new MatrixZ(rows: nKx, cols: nKx);
                SLeft12 = new MatrixZ(rows: nKx, cols: nKx);
                SLeft22 = new MatrixZ(rows: nKx, cols: nKx);
                for (long i = 0; i < nKx; i ++)
                {
                    long dIdx = i - ctrIdx;
                    (SLeft11[i, i], SLeft21[i, i], SLeft12[i, i], SLeft22[i, i]) =
                        FresnelCalculator.ComputeFullSMatrix(
                            wavelength: Wavelength,
                            n1: NLeft, n2: NWaveguide, kx: dIdx * dKx,
                            polarization: polarization);
                }

                // diffractive surface on the right
                SRight11 = new MatrixZ(rows: nKx, cols: nKx);
                SRight21 = new MatrixZ(rows: nKx, cols: nKx);
                SRight12 = new MatrixZ(rows: nKx, cols: nKx);
                SRight22 = new MatrixZ(rows: nKx, cols: nKx);
                // RCWA solver ...
                RCWA1Dp rcwa = new (wavelength: Wavelength,
                    polarization: polarization,
                    materialFront: new FuncMaterial(NWaveguide.Real, NWaveguide.Imaginary),
                    mediumMiddle: DiffLayerRight.Medium,
                    period: DiffLayerRight.Period,
                    thickness: DiffLayerRight.Thickness,
                    materialBehind: new FuncMaterial(nRight.Real, nRight.Imaginary));
                rcwa.ComputeFullSMatrix(kx0: 0.0, fieldSampling, mediumSampling);
                long refCtr = (fieldSampling - 1) / 2;
                MatrixZ s11 = rcwa.S11![0.0];
                MatrixZ s21 = rcwa.S21![0.0];
                MatrixZ s12 = rcwa.S12![0.0];
                MatrixZ s22 = rcwa.S22![0.0];
                for (long iRow = 0; iRow < nKx; iRow++)
                {
                    for (long iCol = 0; iCol < nKx; iCol++)
                    {
                        long dRow = iRow - ctrIdx;
                        long dCol = iCol - ctrIdx;
                        SRight11[iRow, iCol] = s11[refCtr + dRow, refCtr + dCol];
                        SRight21[iRow, iCol] = s21[refCtr + dRow, refCtr + dCol];
                        SRight12[iRow, iCol] = s12[refCtr + dRow, refCtr + dCol];
                        SRight22[iRow, iCol] = s22[refCtr + dRow, refCtr + dCol];
                    }
                }

            }


            /// <summary>
            /// Builds the spatially sampled S-matrices and propagation matrices for the simple diffractive waveguide.
            /// Initializes diagonal matrices for the left and right interface S-matrix elements,
            /// and constructs propagation matrices for left-to-right and right-to-left directions.
            /// </summary>
            /// <exception cref="NullReferenceException">
            /// Thrown if <see cref="GridX"/>, <see cref="GridKx"/>, <see cref="SLeft11"/>, <see cref="SLeft21"/>,
            /// <see cref="SLeft12"/>, <see cref="SLeft22"/>, <see cref="SRight11"/>, <see cref="SRight21"/>,
            /// <see cref="SRight12"/>, or <see cref="SRight22"/> is <c>null</c>, indicating that the required
            /// spatial grid or S-matrix elements have not been initialized.
            /// </exception>
            public void BuildSSMats()
            {
                if (GridX == null) { throw new NullReferenceException(nameof(GridX)); }
                if (GridKx == null) { throw new NullReferenceException(nameof(GridKx)); }
                if (SLeft11 == null) { throw new NullReferenceException(nameof(SLeft11)); }
                if (SLeft21 == null) { throw new NullReferenceException(nameof(SLeft21)); }
                if (SLeft12 == null) { throw new NullReferenceException(nameof(SLeft12)); }
                if (SLeft22 == null) { throw new NullReferenceException(nameof(SLeft22)); }
                if (SRight11 == null) { throw new NullReferenceException(nameof(SRight11)); }
                if (SRight21 == null) { throw new NullReferenceException(nameof(SRight21)); }
                if (SRight12 == null) { throw new NullReferenceException(nameof(SRight12)); }
                if (SRight22 == null) { throw new NullReferenceException(nameof(SRight22)); }
                long nx = GridX.Count;
                long nKx = GridKx.Count;

                #region left surface (no grating)

                SSLeft11 = new MatrixZ(rows: nx * nKx, cols: nx * nKx, mode: ArrayInitMode.Calloc);
                SSLeft21 = new MatrixZ(rows: nx * nKx, cols: nx * nKx, mode: ArrayInitMode.Calloc);
                SSLeft12 = new MatrixZ(rows: nx * nKx, cols: nx * nKx, mode: ArrayInitMode.Calloc);
                SSLeft22 = new MatrixZ(rows: nx * nKx, cols: nx * nKx, mode: ArrayInitMode.Calloc);
                for (long ix = 0; ix < nx; ix++)
                {
                    // local s-matrices at current position
                    MatrixZ s11 = SLeft11;
                    MatrixZ s21 = SLeft21;
                    MatrixZ s12 = SLeft12;
                    MatrixZ s22 = SLeft22;
                    for (long iKx = 0; iKx < nKx; iKx++)
                    {
                        long i = ix * nKx + iKx;
                        SSLeft11[i, i] = s11[iKx, iKx];
                        SSLeft21[i, i] = s21[iKx, iKx];
                        SSLeft12[i, i] = s12[iKx, iKx];
                        SSLeft22[i, i] = s22[iKx, iKx];
                    }
                }

                #endregion
                #region right surface (diffractive grating)

                SSRight11 = new MatrixZ(rows: nx * nKx, cols: nx * nKx, mode: ArrayInitMode.Calloc);
                SSRight21 = new MatrixZ(rows: nx * nKx, cols: nx * nKx, mode: ArrayInitMode.Calloc);
                SSRight12 = new MatrixZ(rows: nx * nKx, cols: nx * nKx, mode: ArrayInitMode.Calloc);
                SSRight22 = new MatrixZ(rows: nx * nKx, cols: nx * nKx, mode: ArrayInitMode.Calloc);
                for (long ix = 0; ix < nx; ix++)
                {
                    // local s-matrices at current position
                    MatrixZ s11 = SRight11;
                    MatrixZ s21 = SRight21;
                    MatrixZ s12 = SRight12;
                    MatrixZ s22 = SRight22;
                    for (long iRow = 0; iRow < nKx; iRow++)
                    {
                        for (long iCol = 0; iCol < nKx; iCol++)
                        {
                            long row = ix * nKx + iRow;
                            long col = ix * nKx + iCol;
                            SSRight11[row, col] = s11[iRow, iCol];
                            SSRight21[row, col] = s21[iRow, iCol];
                            SSRight12[row, col] = s12[iRow, iCol];
                            SSRight22[row, col] = s22[iRow, iCol];
                        }
                    }
                }

                #endregion
                #region propagation

                double k0 = 2.0 * Math.PI / Wavelength;
                double kw = k0 * NWaveguide.Real;
                double dKx = 2.0 * Math.PI / DiffLayerRight.Period;
                // prepares step sizes and propagation terms
                VectorD xStep = new (count: nKx);
                VectorZ p = new (count: nKx);
                for (long i = 0; i < nKx; i++)
                {
                    long di = i - (nKx - 1) / 2;
                    double kxi = di * dKx;
                    double ai = Math.Asin(kxi / kw);
                    xStep[i] = Thickness * Math.Tan(ai);
                    double OPL = kw * Thickness / Math.Cos(ai);
                    p[i] = Complex.Exp(Complex.ImaginaryOne * OPL);
                }
                // propagation matrices
                PLR = new MatrixZ(rows: nx * nKx, cols: nx * nKx, mode: ArrayInitMode.Calloc);
                PRL = new MatrixZ(rows: nx * nKx, cols: nx * nKx, mode: ArrayInitMode.Calloc);
                for(long ix = 0; ix < nx; ix++)
                {
                    // input x-index => column-block index ...
                    long ixIn = ix;
                    // converts to position
                    double xIn = GridX[ixIn];

                    for (long iKx = 0; iKx < nKx; iKx++)
                    {
                        //long di = iKx - (nKx - 1) / 2;
                        // finds corresponding output position
                        double xOut = xIn + xStep[iKx];
                        // converts to index
                        long ixOut = (long)Math.Round((xOut - GridX.Start) / GridX.Spacing);
                        // output index => row-block index
                        if (ixOut >= 0 && ixOut < GridX.Count)
                        {
                            long row = ixOut * nKx + iKx;
                            long col = ix * nKx + iKx;
                            PLR[row, col] = p[iKx];
                            PRL[row, col] = p[iKx];
                        }
                    }
                }

                #endregion
            }


            #endregion
        }


        /// <summary>
        /// 
        /// </summary>
        public class CoatedWaveguide : SSMat1D
        {
            #region properties
            public Complex NCoat { get; set; } = 1.5;
            public double CoatThickness { get; set; } = 0.1E-3;
            #endregion
            #region constructors
            public CoatedWaveguide(
                double wavelength,
                Complex nLeft,
                Complex nWaveguide,
                double thickness,
                Complex nRight,
                Complex nCoat,
                double coatThickness) : base(wavelength, nLeft, nWaveguide, thickness, nRight)
            {
                NCoat = nCoat;
                CoatThickness = coatThickness;
            }
            #endregion
            #region methods
            // ...
            #endregion
        }


        /// <summary>
        /// 
        /// </summary>
        public class DiffWaveguide: SSMat1D
        {
            #region properties
            public Func<double, Complex> NWaveguideFunc { get; set; }
            #endregion
            #region constructors
            public DiffWaveguide(
                double wavelength,
                Complex nLeft,
                Func<double, Complex> nWaveguideFunc,
                double thickness,
                Complex nRight) : base(wavelength, nLeft, Complex.One, thickness, nRight)
            {
                NWaveguideFunc = nWaveguideFunc;
            }
            #endregion
            #region methods
            // ...
            #endregion
        }


        #endregion
    }


    /// <summary>
    /// 
    /// </summary>
    [Obsolete]
    public class SSMat1DCoat
    {
        #region properties

        public double Wavelength { get; set; }
        public Complex NLeft { get; set; }
        public Complex NWaveguide { get; set; }
        public double Thickness { get; set; }
        public Complex NRight { get; set; }

        

        public GridInfo1D? GridX { get; set; }
        public GridInfo1D? GridKx { get; set; }


        internal MatrixZ SLeft { get; set; }
        internal MatrixZ SRight { get; set; }


        public MatrixZ SSLeft11 { get; set; }
        public MatrixZ SSLeft21 { get; set; }
        public MatrixZ SSLeft12 { get; set; }
        public MatrixZ SSLeft22 { get; set; }

        public MatrixZ SSRight11 { get; set; }
        public MatrixZ SSRight21 { get; set; }
        public MatrixZ SSRight12 { get; set; }
        public MatrixZ SSRight22 { get; set; }


        public MatrixZ SPLR { get; set; }
        public MatrixZ SPRL { get; set; }


        #endregion
        #region constructoors

        private SSMat1DCoat()
        {
            Wavelength = 632.8E-9;
            NLeft = Complex.One;
            NWaveguide = 1.8; // new Material(1.8);
            Thickness = 0.5E-3;
            NRight = Complex.One;
        }


        public SSMat1DCoat(double wavelength,
            Complex nLeft, 
            Complex nWaveguide, double thickness, 
            Complex nRight)
        {
            Wavelength = wavelength;
            NLeft = nLeft;
            NWaveguide = nWaveguide;
            Thickness = thickness;
            NRight = nRight;
        }


        #endregion
        #region methods

        #region ==== grids ====


        [MemberNotNull(nameof(GridX), nameof(GridKx))]
        public void InitGrids(double kxIn, double xIn, 
            long nx = 15)
        {
            double k0 = 2.0 * Math.PI / Wavelength;
            double kw = k0 * NWaveguide.Real;
            
            double alpha = Math.Asin(kxIn / kw);
            double xStep = Thickness * Math.Tan(alpha);

            GridX = new GridInfo1D(n: nx, spacing: xStep,
                refPoint: xIn, refType: GridRefType.Center);

            GridKx = new GridInfo1D(n: 1, spacing: 1.0, 
                refPoint: kxIn, refType: GridRefType.Center);
        }

        #endregion
        #region ==== S-matrix calculation ====

        // bare waveguide ...
        [MemberNotNull(nameof(SLeft), nameof(SRight))]
        public void CalcSMatrices(double kxIn, 
            InPlanePolMode polarization = InPlanePolMode.TE)
        {
            SLeft = new MatrixZ(rows: 2, cols: 2, mode: ArrayInitMode.Malloc);
            SRight = new MatrixZ(rows: 2, cols: 2, mode: ArrayInitMode.Malloc);

            (SLeft[0, 0], SLeft[1, 0], SLeft[0, 1], SLeft[1, 1]) = 
                FresnelCalculator.ComputeFullSMatrix(
                wavelength: Wavelength,
                n1: NLeft, n2: NWaveguide, kx: kxIn,
                polarization: polarization);

            (SRight[0, 0], SRight[1, 0], SRight[0, 1], SRight[1, 1]) =
                FresnelCalculator.ComputeFullSMatrix(
                wavelength: Wavelength,
                n1: NWaveguide, n2: NRight, kx: kxIn,
                polarization: polarization);
        }

        #endregion
        #region ==== SSMat ====

        
        public void InitSSMatrices(double kxIn)
        {
            if (GridX == null) { throw new NullReferenceException(nameof(GridX)); }  
            long nx = GridX.Count;

            // Left
            SSLeft11 = LinAlg.DiagonalMatrixHelper.GenerateDenseMatrixZ(nx, SLeft[0, 0, false]);
            SSLeft21 = LinAlg.DiagonalMatrixHelper.GenerateDenseMatrixZ(nx, SLeft[1, 0, false]);
            SSLeft12 = LinAlg.DiagonalMatrixHelper.GenerateDenseMatrixZ(nx, SLeft[0, 1, false]);
            SSLeft22 = LinAlg.DiagonalMatrixHelper.GenerateDenseMatrixZ(nx, SLeft[1, 1, false]);

            // Right
            SSRight11 = LinAlg.DiagonalMatrixHelper.GenerateDenseMatrixZ(nx, SRight[0, 0, false]);
            SSRight21 = LinAlg.DiagonalMatrixHelper.GenerateDenseMatrixZ(nx, SRight[1, 0, false]);
            SSRight12 = LinAlg.DiagonalMatrixHelper.GenerateDenseMatrixZ(nx, SRight[0, 1, false]);
            SSRight22 = LinAlg.DiagonalMatrixHelper.GenerateDenseMatrixZ(nx, SRight[1, 1, false]);
            
            // propagation
            double k0 = 2.0 * Math.PI / Wavelength;
            double kw = k0 * NWaveguide.Real;
            //Printer.WriteLine($"kw = {kw}");
            double alpha = Math.Asin(kxIn / kw);
            //Printer.WriteLine($"alpha = {alpha}");
            double OPL = kw * Thickness / Math.Cos(alpha);
            //Printer.WriteLine($"OPL = {OPL}");
            Complex p = Complex.Exp(Complex.ImaginaryOne * OPL);
            //Printer.WriteLine($"p = {p}");
            SPLR = new MatrixZ(rows: nx, cols: nx, mode: ArrayInitMode.Calloc);
            for (long i = 1; i < nx; i++) { SPLR[i, i - 1, false] = p; }
            SPRL = new MatrixZ(rows: nx, cols: nx, mode: ArrayInitMode.Calloc);
            //for (long i = 1; i < nx; i ++) { SPRL[i, i - 1,  false] = p; }
            for (long i = 1; i < nx; i++) { SPRL[i, i - 1, false] = p; }
        }


        public (VectorZ, VectorZ) Solve(VectorZ cLInPlus, VectorZ cRInMinus)
        {
            if (GridX == null) { throw new NullReferenceException(nameof(GridX)); }
            long nx = GridX.Count;

            // inside
            VectorZ cLInMinus = new (count: nx);
            VectorZ cRInPlus = new(count: nx);
            // output
            VectorZ cLOutMinus = new(count: nx);
            VectorZ cROutPlus = new(count: nx);
            // identity
            MatrixZ idty = LinAlg.DiagonalMatrixHelper.GenerateDenseMatrixZ(nx, 1.0);

            // aux. variables
            MatrixZ QSR21 = LinAlg.Dot(SPRL, SSRight21);
            MatrixZ PSL12 = LinAlg.Dot(SPLR, SSLeft12);
            MatrixZ QSR22 = LinAlg.Dot(SPRL, SSRight22);
            MatrixZ PSL11 = LinAlg.Dot(SPLR, SSLeft11);

            // linear systems
            MatrixZ a1 = idty - LinAlg.Dot(QSR21, PSL12);
            VectorZ b1 = LinAlg.Dot(QSR22, cRInMinus) + LinAlg.Dot(LinAlg.Dot(QSR21, PSL11), cLInPlus);
            cLInMinus = LinAlg.LinearSolve(a1, b1);
            MatrixZ a2 = idty - LinAlg.Dot(PSL12, QSR21);
            VectorZ b2 = LinAlg.Dot(LinAlg.Dot(PSL12, QSR22), cRInMinus) + LinAlg.Dot(PSL11, cLInPlus);
            cRInPlus = LinAlg.LinearSolve(a2, b2);

            // final output
            cLOutMinus = LinAlg.Dot(SSLeft21, cLInPlus) + LinAlg.Dot(SSLeft22, cLInMinus);
            cROutPlus = LinAlg.Dot(SSRight11, cRInPlus) + LinAlg.Dot(SSRight12, cRInMinus);
            return (cLOutMinus, cROutPlus);
        }


        public (VectorZ, VectorZ, VectorZ, VectorZ) Solve1(VectorZ cLInPlus, VectorZ cRInMinus)
        {
            if (GridX == null) { throw new NullReferenceException(nameof(GridX)); }
            long nx = GridX.Count;

            // inside
            VectorZ cLInMinus = new(count: nx);
            VectorZ cRInPlus = new(count: nx);
            // identity
            MatrixZ idty = LinAlg.DiagonalMatrixHelper.GenerateDenseMatrixZ(nx, 1.0);

            // aux. variables
            MatrixZ QSR21 = LinAlg.Dot(SPRL, SSRight21);
            MatrixZ PSL12 = LinAlg.Dot(SPLR, SSLeft12);
            MatrixZ QSR22 = LinAlg.Dot(SPRL, SSRight22);
            MatrixZ PSL11 = LinAlg.Dot(SPLR, SSLeft11);

            // linear systems
            MatrixZ a1 = idty - LinAlg.Dot(QSR21, PSL12); // + idty;
            VectorZ b1 = LinAlg.Dot(QSR22, cRInMinus) + LinAlg.Dot(LinAlg.Dot(QSR21, PSL11), cLInPlus);
            cLInMinus = LinAlg.LinearSolve(a1, b1);
            MatrixZ a2 = idty - LinAlg.Dot(PSL12, QSR21); // + idty;
            VectorZ b2 = LinAlg.Dot(LinAlg.Dot(PSL12, QSR22), cRInMinus) + LinAlg.Dot(PSL11, cLInPlus);
            cRInPlus = LinAlg.LinearSolve(a2, b2);

            var cLOutPlus = LinAlg.LinearSolve(SPLR, cRInPlus);
            var cROutMinus = LinAlg.LinearSolve(SPRL, cLInMinus);

            return (cLInMinus, cRInPlus, cLOutPlus, cROutMinus);
        }

        public (VectorZ, VectorZ) Solve2(
            VectorZ cLInPlus, VectorZ cLInMinus, 
            VectorZ cRInPlus, VectorZ cRInMinus)
        {
            // final output
            var cLOutMinus = LinAlg.Dot(SSLeft21, cLInPlus) + LinAlg.Dot(SSLeft22, cLInMinus);
            var cROutPlus = LinAlg.Dot(SSRight11, cRInPlus) + LinAlg.Dot(SSRight12, cRInMinus);
            return (cLOutMinus, cROutPlus);
        }

        #endregion

        #endregion

    }

}
