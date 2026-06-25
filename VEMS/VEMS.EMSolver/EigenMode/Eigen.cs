using VEMS.MathCore;
using Complex = System.Numerics.Complex;


namespace VEMS.EMSolver
{

    /// <summary>
    /// ...
    /// </summary>
    public class Eigen
    {
        #region === kernels ===

        /// <summary>
        /// computes nz for in-plane case
        /// </summary>
        /// <param name="epsilon"> complex permittivity </param>
        /// <param name="mu"> complex permeability </param>
        /// <param name="nx"> normalized kx (nx = kx/k0) </param>
        /// <param name="ny"> normalized ky (ny = ky/k0) </param>
        /// <param name="scal"> additional scaling factor </param>
        /// <returns> nz = (scal*) kz/k0 </returns>
        public static Complex ComputeNz(Complex epsilon, Complex mu,
            double nx, double ny = 0.0,
            double scal = 1.0)
            => scal * Complex.Sqrt(epsilon * mu - nx * nx - ny * ny);

        /// <summary>
        /// computes nz using dispersion relation
        /// for an isotropic media
        /// </summary>
        /// <param name="epsilon"> permittivity </param>
        /// <param name="mu"> permeability </param>
        /// <param name="nx"> vector nx = kx / k0 </param>
        /// <param name="ny"> scalar nx = ky / k0, by default set to zero </param>
        /// <param name="scal"> constant scaling factor </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> nz vector </returns>
        public static VectorZ ComputeNz(Complex epsilon, Complex mu,
            VectorD nx, double ny = 0.0,
            double scal = 1.0,
            LoopMode loopMode = Defaults.LoopOption)
        {
            Complex emp = epsilon * mu;
            VectorZ nz = new(nx.Count);
            double ny2 = ny * ny;

            // loop mode switch
            switch (loopMode)
            {
                case LoopMode.Sequential:
                    {
                        for (long i = 0; i < nz.Count; i++)
                        {
                            double inx = nx[i, false];
                            nz[i, false] = scal * Complex.Sqrt(emp - inx * inx - ny2);
                        }
                        break;
                    }
                case LoopMode.Parallel:
                    {
                        Parallel.For(0, nz.Count, i =>
                        {
                            double inx = nx[i, false];
                            nz[i, false] = scal * Complex.Sqrt(emp - inx * inx - ny2);
                        });
                        break;
                    }
                case LoopMode.Vectorized:
                    {
                        nz = VMath.Sqrt(emp - ny2 - VMath.Square(nx));
                        if (scal != 1.0) { VMath.ScaleOn(ref nz, scal); }
                        break;
                    }
                default: goto case LoopMode.Parallel;
            }

            return nz;
        }

        /// <summary>
        /// computes nz using dispersion relation
        /// for an isotropic media
        /// </summary>
        /// <param name="epsilon"> permittivity </param>
        /// <param name="mu"> permeability </param>
        /// <param name="nx"> vector nx = kx / k0 </param>
        /// <param name="ny"> vector ny = ky / k0 </param>
        /// <param name="scal"> constant scaling factor </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> nz matrix </returns>
        public static MatrixZ ComputeNz(Complex epsilon, Complex mu,
            VectorD nx, VectorD ny,
            double scal = 1.0,
            LoopMode loopMode = Defaults.LoopOption)
        {
            Complex emp = epsilon * mu;
            MatrixZ nz = new(ny.Count, nx.Count);

            // loop mode switch
            switch (loopMode)
            {
                case LoopMode.Sequential:
                    {
                        for(long i = 0; i <  nz.Rows * nz.Cols; i++)
                        {
                            long iRow = i / nz.Cols;
                            long iCol = i % nz.Cols;
                            double iny = ny[iRow, false];
                            double inx = nx[iCol, false];
                            double iny2 = iny * iny;
                            double inx2 = inx * inx;
                            nz[iRow, iCol, false] = scal * Complex.Sqrt(emp - inx2 - iny2);
                        }
                        break;
                    }
                case LoopMode.Parallel:
                    {
                        Parallel.For(0, nz.Rows * nz.Cols, i =>
                        {
                            long iRow = i / nz.Cols;
                            long iCol = i % nz.Cols;
                            double iny = ny[iRow, false];
                            double inx = nx[iCol, false];
                            double iny2 = iny * iny;
                            double inx2 = inx * inx;
                            nz[iRow, iCol, false] = scal * Complex.Sqrt(emp - inx2 - iny2);
                        });
                        break;
                    }
                case LoopMode.Vectorized:
                    {
                        VectorD nx2 = VMath.Square(nx);
                        VectorD ny2 = VMath.Square(ny);
                        MatrixD nx2m = LinAlg.GenerateRowMatrix(nx2, nz.Rows);
                        MatrixD ny2m = LinAlg.GenerateRowMatrix(ny2, nz.Cols);
                        nz = VMath.Sqrt(emp - nx2m - ny2m);
                        if (scal != 1.0) { VMath.ScaleOn(ref nz, scal); }
                        break;
                    }
                default: goto case LoopMode.Parallel;
            }

            return nz;
        }

        #endregion
        #region === in-plane: scalar kx ===

        /// <summary>
        /// computes the eigen information for in-plane case
        /// for a selected polarization mode
        /// </summary>
        /// <param name="wavelength"> wavelength in vacuum </param>
        /// <param name="epsilon"> complex permittivity @wavelength </param>
        /// <param name="mu"> complex permeability @wavelength </param>
        /// <param name="kx"> transverse spatial frequency </param>
        /// <param name="polarization"> in-plane polarization mode: TE or TM </param>
        /// <param name="direction"> direction of propagation </param>
        /// <returns> eigen-info (nz, w) </returns>
        public static (Complex, Complex) ComputeEigen(double wavelength,
            Complex epsilon, Complex mu, double kx,
            InPlanePolMode polarization = InPlanePolMode.TE,
            SignFactor direction = SignFactor.Positive)
        {
            double k0 = 2.0 * Math.PI / wavelength;
            double nx = kx / k0;
            // computes nz
            Complex nz = ComputeNz(epsilon, mu, nx, 
                ny: 0.0, scal: (int)direction);
            // computes w
            Complex w = polarization switch
            {
                InPlanePolMode.TE => -nz / mu,
                InPlanePolMode.TM => epsilon / nz,
                _ => -nz / mu
            };
            return (nz, w);
        }

        /// <summary>
        /// computes the eigen information for in-plane case
        /// for a selected polarization mode
        /// </summary>
        /// <param name="wavelength"> wavelength in vacuum </param>
        /// <param name="refractiveIndex"> refractive index @wavelength </param>
        /// <param name="kx"> transverse spatial frequency </param>
        /// <param name="polarization"> in-plane polarization mode: TE or TM </param>
        /// <param name="direction"> direction of propagation </param>
        /// <returns> eigen-info (nz, w) </returns>
        public static (Complex, Complex) ComputeEigen(double wavelength,
            Complex refractiveIndex, double kx,
            InPlanePolMode polarization = InPlanePolMode.TE,
            SignFactor direction = SignFactor.Positive)
            => ComputeEigen(wavelength: wavelength,
                epsilon: refractiveIndex * refractiveIndex,
                mu: 1.0,
                kx: kx,
                polarization: polarization,
                direction: direction);

        /// <summary>
        /// computes the eigen information for in-plane case
        /// for both TE and TM polarization modes
        /// </summary>
        /// <param name="wavelength"> wavelength in vacuum </param>
        /// <param name="epsilon"> complex permittivity @wavelength </param>
        /// <param name="mu"> complex permeability @wavelength </param>
        /// <param name="kx"> transverse spatial frequency </param>
        /// <param name="direction"> direction of propagation </param>
        /// <returns> eigen-info (nz, wC, wD) </returns>
        public static (Complex, Complex, Complex) ComputeEigen(double wavelength,
            Complex epsilon, Complex mu, double kx,
            SignFactor direction = SignFactor.Positive)
        {
            double k0 = 2.0 * Math.PI / wavelength;
            double nx = kx / k0;
            // computes nz
            Complex nz = ComputeNz(epsilon, mu, nx, 
                ny: 0.0, scal: (int)direction);
            // compute wc and wd
            Complex wc = epsilon / nz; // TM
            Complex wd = -nz / mu; // TE
            return (nz, wc, wd);
        }

        /// <summary>
        /// computes the eigen information for in-plane case
        /// for both TE and TM polarization modes
        /// </summary>
        /// <param name="wavelength"> wavelength in vacuum </param>
        /// <param name="refractiveIndex"> refractive index @wavelength </param>
        /// <param name="kx"> transverse spatial frequency </param>
        /// <param name="direction"> direction of propagation </param>
        /// <returns> eigen-info (nz, wC, wD) </returns>
        public static (Complex, Complex, Complex) ComputeEigen(double wavelength,
            Complex refractiveIndex, double kx,
            SignFactor direction = SignFactor.Positive)
            => ComputeEigen(wavelength: wavelength,
                epsilon: refractiveIndex * refractiveIndex,
                mu: 1.0,
                kx: kx,
                direction: direction);

        #endregion
        #region === in-plane: vector kx ===

        /// <summary>
        /// [vectorized] eigen information for in-plane case
        /// for a selected polarization mode
        /// </summary>
        /// <param name="wavelength"> wavelength in vacuum </param>
        /// <param name="epsilon"> complex permittivity @wavelength </param>
        /// <param name="mu"> complex permeability @wavelength </param>
        /// <param name="kx"> transverse spatial frequency </param>
        /// <param name="polarization"> in-plane polarization mode: TE or TM </param>
        /// <param name="direction"> direction of propagation </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> eigen-info (nz, w) </returns>
        public static (VectorZ, VectorZ) ComputeEigen(double wavelength,
            Complex epsilon, Complex mu, VectorD kx,
            InPlanePolMode polarization = InPlanePolMode.TE,
            SignFactor direction = SignFactor.Positive,
            LoopMode loopMode = LoopMode.Vectorized) //Defaults.LoopOption)
        {
            double k0 = 2.0 * Math.PI / wavelength;
            VectorD nx = VMath.Scale(kx, 1.0/k0);
            // computes nz
            VectorZ nz = ComputeNz(epsilon, mu, nx, 
                ny: 0.0, scal: (int)direction, loopMode: loopMode);
            // computes w
            VectorZ w = polarization switch
            {
                InPlanePolMode.TE => (mu == 1.0) ? -nz : VMath.Scale(nz, -1.0 / mu),
                InPlanePolMode.TM => epsilon / nz,
                _ => (mu == 1.0) ? -nz : VMath.Scale(nz, -1.0 / mu)
            };
            return (nz, w);
        }

        /// <summary>
        /// [vectorized] eigen information for in-plane case
        /// for a selected polarization mode
        /// </summary>
        /// <param name="wavelength"> wavelength in vacuum </param>
        /// <param name="refractiveIndex"> refractive index @wavelength </param>
        /// <param name="kx"> transverse spatial frequency </param>
        /// <param name="polarization"> in-plane polarization mode: TE or TM </param>
        /// <param name="direction"> direction of propagation </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> eigen-info (nz, w) </returns>
        public static (VectorZ, VectorZ) ComputeEigen(double wavelength,
            Complex refractiveIndex, VectorD kx,
            InPlanePolMode polarization = InPlanePolMode.TE,
            SignFactor direction = SignFactor.Positive,
            LoopMode loopMode = LoopMode.Vectorized)
            => ComputeEigen(wavelength: wavelength,
                epsilon: refractiveIndex * refractiveIndex,
                mu: 1.0,
                kx: kx,
                polarization: polarization,
                direction: direction,
                loopMode: loopMode);

        /// <summary>
        /// [vectorized] eigen information for in-plane case
        /// for both TE and TM polarization modes
        /// </summary>
        /// <param name="wavelength"> wavelength in vacuum </param>
        /// <param name="epsilon"> complex permittivity @wavelength </param>
        /// <param name="mu"> complex permeability @wavelength </param>
        /// <param name="kx"> transverse spatial frequency </param>
        /// <param name="direction"> direction of propagation </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> eigen-info (nz, wC, wD) </returns>
        public static (VectorZ, VectorZ, VectorZ) ComputeEigen(double wavelength,
            Complex epsilon, Complex mu, VectorD kx,
            SignFactor direction = SignFactor.Positive,
            LoopMode loopMode = LoopMode.Vectorized) //Defaults.LoopOption)
        {
            double k0 = 2.0 * Math.PI / wavelength;
            VectorD nx = VMath.Scale(kx, 1.0 / k0);
            // computes nz
            VectorZ nz = ComputeNz(epsilon, mu, nx,
                ny: 0.0, scal: (int)direction, loopMode: loopMode);
            // compute wc and wd
            VectorZ wc = epsilon / nz;  // TM
            VectorZ wd = (mu == 1.0) ? -nz : VMath.Scale(nz, -1.0 / mu);
            return (nz, wc, wd);
        }

        /// <summary>
        /// [vectorized] eigen information for in-plane case
        /// for both TE and TM polarization modes
        /// </summary>
        /// <param name="wavelength"> wavelength in vacuum </param>
        /// <param name="refractiveIndex"> refractive index @wavelength </param>
        /// <param name="kx"> transverse spatial frequency </param>
        /// <param name="direction"> direction of propagation </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> eigen-info (nz, wC, wD) </returns>
        public static (VectorZ, VectorZ, VectorZ) ComputeEigen(double wavelength,
            Complex refractiveIndex, VectorD kx,
            SignFactor direction = SignFactor.Positive,
            LoopMode loopMode = LoopMode.Vectorized)
            => ComputeEigen(wavelength: wavelength,
                epsilon: refractiveIndex * refractiveIndex,
                mu: 1.0,
                kx: kx,
                direction: direction,
                loopMode: loopMode);

        #endregion
        #region === conical: scalar (kx, ky) ===


        //public static (Complex, Complex, Complex, Complex) ComputeConicalEigen(double wavelength,
        //    Complex epsilon, Complex mu, double kx,
        //    InPlanePolMode polarization = InPlanePolMode.TE,
        //    SignFactor direction = SignFactor.Positive)
        //{
        //    double k0 = 2.0 * Math.PI / wavelength;
        //    double nx = kx / k0;
        //    // computes nz
        //    Complex nz = ComputeNz(epsilon, mu, nx,
        //        ny: 0.0, scal: (int)direction);
        //    // computes w
        //    Complex w = polarization switch
        //    {
        //        InPlanePolMode.TE => -nz / mu,
        //        InPlanePolMode.TM => epsilon / nz,
        //        _ => -nz / mu
        //    };
        //    return (nz, w);
        //}


        #endregion
    }


    /// <summary>
    /// polarization modes in the in-plane case
    /// </summary>
    public enum InPlanePolMode
    {
        /// <summary>
        /// transverse electric
        /// </summary>
        TE,
        /// <summary>
        /// transverse magnetic
        /// </summary>
        TM
    }

    /// <summary>
    /// Make different type of Toeplitz Matrix
    /// </summary>
    public enum ToeplitzMatrixType
    {
        /// <summary>
        /// Periodic Toeplitz Matrix
        /// </summary>
        Periodic,
        /// <summary>
        /// Truncated/Padding Toeplitz Matrix
        /// </summary>
        Nonperiodic,
    }

    ////
    //public class Distribution
    //{

    //    public static void FuncExperiment()
    //    {

    //        Func<double, double> SingleVariableFunction = Add1Definition;
    //        Func<double, double, double> DoubleVariableFunction = Add2Definition;
    //        Func<double, double, double, double> ThreeVariableFunction = Add3Definition;
    //    }


    //    public static Func<double, double> SingleVarFunc = Add1Definition;
    //    public static Func<double, double, double> DoubleVarFunc = Add2Definition;


    //    public static double Add1Definition(double x1)
    //    {
    //        double x2 = 1.0;
    //        return Add2Definition(x1, x2);
    //    }

    //    public static double Add2Definition(double x1, double x2)
    //    {
    //        double x3 = 0.5;
    //        return Add3Definition(x1, x2, x3);
    //    }

    //    public static double Add3Definition(double x1, double x2, double x3)
    //    {
    //        return (x1 + x2 + x3);
    //    }


    //}

    ///// <summary>
    ///// Fourier eigen modes in a uniform layer structure
    ///// </summary>
    //[Obsolete("Use UniformLayer instead")]
    //public class UniformLayerFourierMode
    //{
    //    //private double _wavelength { get; set; }
    //    //private Complex _epsilon { get; set; }
    //    //private Complex _mu = 1.0; // seldom used
    //    //private double _thickness = 0.0;

    //    #region properties

    //    /// <summary>
    //    /// wavelength in vacuum
    //    /// </summary>
    //    public double Wavelength { get; set; }

    //    /// <summary>
    //    /// permittivity @ wavelength
    //    /// </summary>
    //    public Complex Epsilon { get; set; }

    //    /// <summary>
    //    /// permeability @ wavelength
    //    /// </summary>
    //    public Complex Mu { get; set; }

    //    /// <summary>
    //    /// thickness of the layer
    //    /// </summary>
    //    public double Thickness { get; set; }

    //    ///// <summary>
    //    ///// kx vector
    //    ///// </summary>
    //    //public VectorD Kx { get; set; }

    //    /// <summary>
    //    /// eigenvalues 
    //    /// </summary>
    //    public VectorZ Gamma { get; set; }

    //    /// <summary>
    //    /// eigenvector matrix
    //    /// </summary>
    //    public MatrixZ W { get; set; }

    //    /// <summary>
    //    /// complex mode coefficients
    //    /// </summary>
    //    public VectorZ ModeCoefficients { get; set; }

    //    #endregion
    //    #region constructor

    //    /// <summary>
    //    /// constructs a UniformLayerFourierMode
    //    /// with uniform permittivity distribution within the layer        
    //    /// and the default permeability is 1.0
    //    /// </summary>
    //    /// <param name="wavelength"> working wavelength in vacuum </param>
    //    /// <param name="epsilon"> permittivity @ wavelength </param>
    //    /// <param name="thickness"> thickness of the uniform layer </param>
    //    public UniformLayerFourierMode(double wavelength,
    //        Complex epsilon,
    //        double thickness = 0.0)
    //    {
    //        Wavelength = wavelength;
    //        Epsilon = epsilon;
    //        Mu = 1.0;
    //        Thickness = thickness;
    //    }

    //    #endregion
    //    #region methods

    //    /// <summary>
    //    /// computes the TE modes
    //    /// results are stored as properties
    //    /// </summary>
    //    public void ComputeTEModes(VectorD kx)
    //    {
    //        // normalize kx by k0
    //        double k0 = 2.0 * Math.PI / Wavelength;
    //        VectorD nx = kx / k0;
    //        // calculating nz = kz/k0
    //        VectorZ nz = new(nx.Count);
    //        for (long i = 0; i < nz.Count; i++)
    //            nz[i] = Complex.Sqrt(Epsilon * Mu - nx[i] * nx[i]);
    //        // define eigenvalues Gamma as output
    //        Gamma = new VectorZ(2 * nx.Count);
    //        for (long i = 0; i < nz.Count; i++)
    //        {
    //            Gamma[i] = Complex.ImaginaryOne * k0 * nz[i];
    //            Gamma[nz.Count + i] = -Complex.ImaginaryOne * k0 * nz[i];
    //        }
    //        // define eigenvectors W as output
    //        W = new MatrixZ(2 * nx.Count, 2 * nx.Count);
    //        for (long i = 0; i < nz.Count; i++)
    //        {
    //            W[i, i] = 1.0; // W[1,1]
    //            W[i, i + nz.Count] = 1.0; // W[1,2]
    //            W[i + nz.Count, i] = -nz[i] / Mu; // W[2,1]
    //            W[i + nz.Count, i + nz.Count] = nz[i] / Mu; // W[2,2]
    //        }
    //    }

    //    /// <summary>
    //    /// computes the TM modes
    //    /// results are stored as properties
    //    /// </summary>
    //    public void ComputeTMModes(VectorD kx)
    //    {
    //        // normalize kx by k0
    //        double k0 = 2.0 * Math.PI / Wavelength;
    //        VectorD nx = kx / k0;
    //        // calculating nz = kz/k0
    //        VectorZ nz = new(nx.Count);
    //        for (long i = 0; i < nz.Count; i++)
    //            nz[i] = Complex.Sqrt(Epsilon * Mu - nx[i] * nx[i]);
    //        // define eigenvalues Gamma as output
    //        Gamma = new VectorZ(2 * nx.Count);
    //        for (long i = 0; i < nz.Count; i++)
    //        {
    //            Gamma[i] = Complex.ImaginaryOne * k0 * nz[i];
    //            Gamma[nz.Count + i] = -Complex.ImaginaryOne * k0 * nz[i];
    //        }
    //        // define eigenvectors W as output
    //        W = new MatrixZ(2 * nx.Count, 2 * nx.Count);
    //        for (long i = 0; i < nz.Count; i++)
    //        {
    //            W[i, i] = 1.0; // W[1,1]
    //            W[i, i + nz.Count] = 1.0; // W[1,2]
    //            W[i + nz.Count, i] = Epsilon / nz[i]; // W[2,1]
    //            W[i + nz.Count, i + nz.Count] = -Epsilon / nz[i]; // W[2,2]
    //        }
    //    }

    //    #endregion

    //}

    ///// <summary>
    ///// Fourier eigen modes in a 1D-periodic
    ///// layer structure
    ///// </summary>
    //[Obsolete("Use Periodic1DLayer instead")]
    //public class Periodic1DLayerFourierMode
    //{

    //    #region properties

    //    /// <summary>
    //    /// wavelength in vacuum
    //    /// </summary>
    //    public double Wavelength { get; set; }

    //    /// <summary>
    //    /// piecewise grid locations
    //    /// </summary>
    //    public VectorD PieceGrid { get; set; }

    //    /// <summary>
    //    /// locations where the material change happens
    //    /// within a period
    //    /// </summary>
    //    public VectorD TransisionPoints { get; set; }

    //    /// <summary>
    //    /// permittivity distribution @ wavelength
    //    /// according to the transition points
    //    /// </summary>
    //    public VectorZ Epsilon { get; set; }

    //    /// <summary>
    //    /// permeability distribution @ wavelength
    //    /// according to the transition points
    //    /// </summary>
    //    public VectorZ Mu { get; set; }

    //    /// <summary>
    //    /// thickness of the layer
    //    /// </summary>
    //    public double Thickness { get; set; }

    //    /// <summary>
    //    /// kx vector
    //    /// </summary>
    //    //public VectorD Kx { get; set; }

    //    /// <summary>
    //    /// eigenvalues 
    //    /// </summary>
    //    public VectorZ Gamma { get; set; }

    //    /// <summary>
    //    /// eigenvector matrix
    //    /// </summary>
    //    public MatrixZ W { get; set; }

    //    /// <summary>
    //    /// complex mode coefficients
    //    /// </summary>
    //    public VectorZ ModeCoefficients { get; set; }

    //    #endregion
    //    #region constructor

    //    /// <summary>
    //    /// constructs a Periodic1DLayerFourierMode
    //    /// with 1D-periodic permittivity distribution within the layer
    //    /// and the default permeability is 1.0
    //    /// </summary>
    //    /// <param name="wavelength"> working wavelength in vacuum </param>
    //    /// <param name="pieceGrid"> locations within a period where the material changes </param>
    //    /// <param name="epsilon"> permittivity distribution according to the transition points </param>
    //    /// <param name="thickness"> thickness of the layer </param>
    //    public Periodic1DLayerFourierMode(double wavelength,
    //        VectorD pieceGrid,
    //        VectorZ epsilon,
    //        double thickness)
    //    {
    //        Wavelength = wavelength;
    //        PieceGrid = pieceGrid;
    //        Epsilon = epsilon;
    //        Mu = new VectorZ(epsilon.Count, 1.0);
    //        Thickness = thickness;
    //    }

    //    #endregion
    //    #region methods

    //    /// <summary>
    //    /// computes the TE modes
    //    /// results stored as properties
    //    /// </summary>
    //    public void ComputeTEMode(VectorD kx)
    //    {
    //        double k0 = 2.0 * Math.PI / Wavelength;
    //        Pwct1DCplxData eps = new(PieceGrid, Epsilon);
    //        VectorZ vEps = Transform.ForwardTransform1D(eps, -kx.Count + 1, 2 * kx.Count - 1);

    //        // toeplitz epsilon matrix
    //        MatrixZ mEps = LinAlg.GenerateToeplitzMatrixZ(vEps);

    //        // construct field matrix
    //        VectorZ kx2 = new(kx * kx);
    //        MatrixZ mField = -k0 * k0 * mEps + LinAlg.DiagonalMatrixHelper.GenerateDenseMatrixZ(kx2);

    //        // matrix eigen-decomposition
    //        LinAlg.EigenSystem(ref mField, out VectorZ eigenValues, out MatrixZ eigenVectors);

    //        // gamma
    //        eigenValues = VMath.Sqrt(eigenValues);
    //        for (int i = 0; i < eigenValues.Count; i++)
    //        {
    //            double Re = eigenValues[i].Real;
    //            double Im = eigenValues[i].Imaginary;
    //            if (Re == 0)
    //            {
    //                if (Im < 0) // in fact, this does not matter
    //                    eigenValues[i] = -eigenValues[i];
    //            }
    //            else
    //            {
    //                if (Re > 0) // reverse the sign, when realpart of gamma is positive
    //                    eigenValues[i] = -eigenValues[i];
    //            }
    //        }
    //        Gamma = new VectorZ(2 * kx.Count)
    //        {
    //            [new LongRange(0, kx.Count)] = eigenValues,
    //            [new LongRange(kx.Count, 2 * kx.Count)] = -eigenValues
    //        };

    //        // W
    //        MatrixZ w1 = eigenVectors;
    //        MatrixZ w2 = Complex.ImaginaryOne / k0 * LinAlg.Dot(w1, LinAlg.DiagonalMatrixHelper.GenerateDenseMatrixZ(eigenValues));
    //        LongRange top = new LongRange(0, kx.Count);
    //        LongRange bottom = new LongRange(kx.Count, 2 * kx.Count);
    //        LongRange left = new LongRange(0, kx.Count);
    //        LongRange right = new LongRange(kx.Count, 2 * kx.Count);
    //        W = new MatrixZ(2 * kx.Count, 2 * kx.Count)
    //        {
    //            [top, left] = w1,
    //            [top, right] = w1,
    //            [bottom, left] = w2,
    //            [bottom, right] = -w2
    //        };
    //    }


    //    /// <summary>
    //    /// Fourier series calculation
    //    /// [reference method for comparison]
    //    /// </summary>
    //    /// <param name="tp"> transition points </param>
    //    /// <param name="f"> function values </param>
    //    /// <param name="N"> total number of Fourier coefficients </param>
    //    /// <returns></returns>
    //    public static VectorZ FourierSeriesReference(VectorD tp,
    //        VectorZ f,
    //        long N)
    //    {
    //        // ensure odd number of Fourier coefficients
    //        if (N % 2 == 0)
    //            N += 1;

    //        // find the period value
    //        (_, double period) = VMath.IndexMax(tp);
    //        period *= 2.0;

    //        // generate tps
    //        VectorD tps = new(tp.Count);
    //        tps[0] = -0.5 * period;
    //        for (long i = 1; i < tps.Count; i++)
    //            tps[i] = tp[i - 1];

    //        // initialize output
    //        VectorZ FourierSeries = new(N);
    //        for (long i = -(N - 1) / 2; i <= (N - 1) / 2; i++)
    //        {
    //            if (i == 0)
    //                FourierSeries[(N - 1) / 2] = VMath.Sum(f * (tp - tps)) / period;
    //            else
    //                FourierSeries[i + (N - 1) / 2] = Complex.ImaginaryOne / (2.0 * Math.PI * i) *
    //                    VMath.Sum(f * (VMath.Exp(-Complex.ImaginaryOne * 2.0 * Math.PI * i * tp / period) - VMath.Exp(-Complex.ImaginaryOne * 2.0 * Math.PI * i * tps / period)));
    //        }

    //        // return 
    //        return FourierSeries;
    //    }


    //    #endregion

    //}

    ///// <summary>
    ///// 1D-periodic isotropic layer
    ///// </summary>
    //[Obsolete("Use Periodic1DLayer instead")]
    //public class Periodic1DLayerPiecewise
    //{
    //    #region properties

    //    /// <summary>
    //    /// piecewise grid locations
    //    /// </summary>
    //    public VectorD GridPoints { get; set; }

    //    /// <summary>
    //    /// permittivity distribution @ wavelength
    //    /// according to the transition points
    //    /// </summary>
    //    public VectorZ Epsilon { get; set; }

    //    /// <summary>
    //    /// permeability distribution @ wavelength
    //    /// according to the transition points
    //    /// </summary>
    //    public VectorZ Mu { get; set; }

    //    /// <summary>
    //    /// thickness of the layer
    //    /// </summary>
    //    public double Thickness { get; set; }

    //    /// <summary>
    //    /// working wavelength in vacuum
    //    /// </summary>
    //    public double Wavelength { get; set; }

    //    /// <summary>
    //    /// eigenvalues Gamma
    //    /// </summary>
    //    public VectorZ? Gamma { get; set; }

    //    /// <summary>
    //    /// eigenvector matrix W1
    //    /// </summary>
    //    public MatrixZ? W1 { get; set; }

    //    /// <summary>
    //    /// eigenvector matrix W2
    //    /// </summary>
    //    public MatrixZ? W2 { get; set; }

    //    #endregion
    //    #region constructor

    //    /// <summary>
    //    /// constructs a Periodic1DLayer class
    //    /// with 1D-periodic permittivity and permeability distribution
    //    /// </summary>
    //    /// <param name="gridPoints"> locations within a period where the material changes </param>
    //    /// <param name="epsilon"> permittivity distribution </param>
    //    /// <param name="mu"> permeability distribution </param>
    //    /// <param name="thickness"> thickness of the layer </param>
    //    public Periodic1DLayerPiecewise(VectorD gridPoints,
    //        VectorZ epsilon, VectorZ mu,
    //        double thickness)
    //    {
    //        GridPoints = gridPoints;
    //        Epsilon = epsilon;
    //        Mu = mu;
    //        Thickness = thickness;
    //    }

    //    /// <summary>
    //    /// constructs a Periodic1DLayerFourierMode
    //    /// with 1D-periodic permittivity distribution 
    //    /// and the default permeability is 1.0
    //    /// </summary>
    //    /// <param name="gridPoints"> locations within a period where the material changes </param>
    //    /// <param name="epsilon"> permittivity distribution </param>
    //    /// <param name="thickness"> thickness of the layer </param>
    //    public Periodic1DLayerPiecewise(VectorD gridPoints,
    //        VectorZ epsilon,
    //        double thickness) :
    //        this(gridPoints, epsilon, new VectorZ(epsilon.Count, 1.0), thickness)
    //    { }
    //    //{
    //    //    GridPoints = gridPoints;
    //    //    Epsilon = epsilon;
    //    //    Mu = new VectorZ(epsilon.Count, 1.0);
    //    //    Thickness = thickness;
    //    //}

    //    #endregion
    //    #region methods

    //    /// <summary>
    //    /// computes the TE modes
    //    /// results stored as properties
    //    /// </summary>
    //    public (VectorZ, MatrixZ) ComputeTEMode(double wavelength, VectorD kx)
    //    {
    //        double k0 = 2.0 * Math.PI / wavelength;
    //        Pwct1DCplxData epsilon = new(GridPoints, Epsilon);
    //        VectorZ vEps = Transform.ForwardTransform1D(epsilon, -kx.Count + 1, 2 * kx.Count - 1);

    //        // toeplitz epsilon matrix
    //        MatrixZ mEps = LinAlg.GenerateToeplitzMatrixZ(vEps);

    //        // construct field matrix
    //        VectorZ kx2 = new(kx * kx);
    //        MatrixZ mField = -k0 * k0 * mEps + LinAlg.DiagonalMatrixHelper.GenerateDenseMatrixZ(kx2);

    //        // matrix eigen-decomposition
    //        LinAlg.EigenSystem(ref mField, out VectorZ eigenValues, out MatrixZ eigenVectors);

    //        // gamma
    //        eigenValues = VMath.Sqrt(eigenValues);
    //        for (int i = 0; i < eigenValues.Count; i++)
    //        {
    //            double Re = eigenValues[i].Real;
    //            double Im = eigenValues[i].Imaginary;
    //            if (Re == 0)
    //            {
    //                if (Im < 0) // in fact, this does not matter
    //                    eigenValues[i] = -eigenValues[i];
    //            }
    //            else
    //            {
    //                if (Re > 0) // reverse the sign, when realpart of gamma is positive
    //                    eigenValues[i] = -eigenValues[i];
    //            }
    //        }
    //        VectorZ gamma = new VectorZ(2 * kx.Count)
    //        {
    //            [new LongRange(0, kx.Count)] = eigenValues,
    //            [new LongRange(kx.Count, 2 * kx.Count)] = -eigenValues
    //        };

    //        // W
    //        MatrixZ w1 = eigenVectors;
    //        MatrixZ w2 = Complex.ImaginaryOne / k0 * LinAlg.DotWithDiagonalMatrixZ(w1, eigenValues); // Dot(w1, LinAlg.GenerateDiagonalMatrixZ(eigenValues));
    //        LongRange top = new(0, kx.Count);
    //        LongRange bottom = new(kx.Count, 2 * kx.Count);
    //        LongRange left = new(0, kx.Count);
    //        LongRange right = new(kx.Count, 2 * kx.Count);
    //        MatrixZ w = new(2 * kx.Count, 2 * kx.Count)
    //        {
    //            [top, left] = w1,
    //            [top, right] = w1,
    //            [bottom, left] = w2,
    //            [bottom, right] = -w2
    //        };
    //        return (gamma, w);
    //    }

    //    /// <summary>
    //    /// computes the TE modes 
    //    /// with W1/2 symmetry
    //    /// </summary>
    //    /// <param name="wavelength"> wavelength in vacuum </param>
    //    /// <param name="kx"> spatial frequencies </param>
    //    /// <returns> (gamma, w1, w2) </returns>
    //    public (VectorZ, MatrixZ, MatrixZ) ComputeTEModesSym(double wavelength, VectorD kx)
    //    {
    //        double k0 = 2.0 * Math.PI / wavelength;
    //        Pwct1DCplxData epsilon = new(GridPoints, Epsilon);
    //        VectorZ vEps = Transform.ForwardTransform1D(epsilon, -kx.Count + 1, 2 * kx.Count - 1);

    //        // toeplitz epsilon matrix
    //        long mid = (vEps.Count - 1) / 2;
    //        VectorZ firstCol = new(mid + 1, 0.0);
    //        VectorZ firstRow = new(mid + 1, 0.0);
    //        for (long i = 0; i < mid + 1; i++)
    //        {
    //            firstCol[i] = vEps[mid + i];
    //            firstRow[i] = vEps[mid - i];
    //        }
    //        MatrixZ mEps = LinAlg.GenerateToeplitzMatrixZ(firstCol, firstRow);

    //        // construct field matrix
    //        VectorZ kx2 = new(kx * kx);
    //        MatrixZ mField = -k0 * k0 * mEps + LinAlg.DiagonalMatrixHelper.GenerateDenseMatrixZ(kx2);

    //        // matrix eigen-decomposition
    //        LinAlg.EigenSystem(ref mField, out VectorZ eigenValues, out MatrixZ eigenVectors);

    //        // gamma
    //        VectorZ gamma = VMath.Sqrt(eigenValues);
    //        for (int i = 0; i < eigenValues.Count; i++)
    //        {
    //            double Re = gamma[i].Real;
    //            double Im = gamma[i].Imaginary;
    //            if (Re == 0)
    //            {
    //                if (Im < 0) // in fact, this does not matter
    //                    gamma[i] = -gamma[i];
    //            }
    //            else
    //            {
    //                if (Re > 0) // reverse the sign, when realpart of gamma is positive
    //                    gamma[i] = -gamma[i];
    //            }
    //        }

    //        // W
    //        MatrixZ w1 = eigenVectors;
    //        MatrixZ w2 = Complex.ImaginaryOne / k0 * LinAlg.DotWithDiagonalMatrixZ(w1, gamma);

    //        return (gamma, w1, w2);
    //    }


    //    /// <summary>
    //    /// makes a multiple periodization of the given structure
    //    /// </summary>
    //    /// <param name="multiple"> multiple number for periodization </param>
    //    [Obsolete("periodization should be done outside eigen solver")]
    //    public void Periodize(long multiple)
    //    {
    //        // get original grid spacings
    //        double period = GridPoints[GridPoints.Count - 1] - GridPoints[0];
    //        //Printer.Write("Period = " + period.ToString());
    //        VectorD unitGridDistances = new(GridPoints.Count - 1);
    //        for (long i = 0; i < unitGridDistances.Count; i++)
    //        {
    //            // get the distances of each point from the first point
    //            unitGridDistances[i] = GridPoints[i + 1] - GridPoints[0];
    //        }
    //        //Printer.Write("unitGridDistances ", unitGridDistances);

    //        // copy the values in the original period
    //        VectorZ unitGridValues = new(Epsilon);
    //        //Printer.Write("unitGridValues = ", unitGridValues);

    //        // generates the new grid vector and epsilon vector
    //        long newCounts = multiple * (GridPoints.Count - 1) + 1;
    //        double newFirstPoint = -0.5 * multiple * period;

    //        GridPoints = new(newCounts, 0.0);
    //        Epsilon = new(newCounts - 1, 0.0);

    //        // fill the grid points and epsilon values
    //        GridPoints[0] = newFirstPoint;
    //        for (long i = 0; i < multiple; i++)
    //        {
    //            for (long j = 0; j < unitGridDistances.Count; j++)
    //            {
    //                GridPoints[i * unitGridDistances.Count + j + 1] = //unitGridDistances[j];
    //                    newFirstPoint + i * period + unitGridDistances[j];
    //                Epsilon[i * unitGridDistances.Count + j] = unitGridValues[j];
    //            }
    //        }
    //    }

    //    #endregion
    //}


}
