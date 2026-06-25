using Complex = System.Numerics.Complex;
using VEMS.MathCore;

namespace VEMS.EMSolver
{

    /// <summary>
    /// uniform isotropic layer
    /// can be used for RCWA and more ...
    /// </summary>
    public class UniformLayer : EigenLayer
    {
        #region properties

        /// <summary>
        /// permittivity @ varying wavelength
        /// </summary>
        public Func<double, Complex> Epsilon { get; set; }

        /// <summary>
        /// permeability @ varying wavelength
        /// </summary>
        public Func<double, Complex> Mu { get; set; }

        /// <summary>
        /// spatial frequencies
        /// </summary>
        public VectorD? Kx { get; set; }

        /// <summary>
        /// eigenvector matrix W1
        /// diagonal matrix stored as vector
        /// </summary>
        public new VectorZ? W1 { get; set; }

        /// <summary>
        /// eigenvector matrix W2
        /// diagonal matrix stored as vector
        /// </summary>
        public new VectorZ? W2 { get; set; } 

        /// <summary>
        /// eigenvector matrix W1
        /// here as dense matrix
        /// future => sparse matrix
        /// </summary>
        public MatrixZ? W1s { get; set; }

        /// <summary>
        /// eigenvector matrix W2
        /// here as dense matrix
        /// future => sparse matrix
        /// </summary>
        public MatrixZ? W2s { get; set; }

        #endregion
        #region constructor

        /// <summary>
        /// constructs a UniformLayer with uniform permittivity
        /// and permeability as functions of wavelength
        /// </summary>
        /// <param name="epsilon"> permittivity epsilon = f(λ) </param>
        /// <param name="mu"> permeability mu = f(λ) </param>
        /// <param name="thickness"> thickness of the uniform layer </param>
        public UniformLayer(Func<double, Complex> epsilon, 
            Func<double, Complex>? mu = null,
            double thickness = 0.0)
        {
            Epsilon = epsilon;
            Mu = mu ?? ((w) => 1.0);
            Thickness = thickness;
        }

        /// <summary>
        /// constructs a UniformLayer with given material
        /// in this case, permeability is set as 1.0
        /// </summary>
        /// <param name="m"></param>
        /// <param name="thickness"></param>
        public UniformLayer(Material m,
            double thickness = 0.0)
            : this(epsilon: (w) => m.N(w) * m.N(w), 
                  thickness: thickness)
        { }

        /// <summary>
        /// constructs a UniformLayer with uniform permittivity
        /// and permeability as constant 1.0
        /// </summary>
        /// <param name="epsilon"> permittivity @wavelength </param>
        /// <param name="mu"> permeability mu @wavelength </param>
        /// <param name="thickness"> thickness of the uniform layer </param>
        public UniformLayer(Complex epsilon, Complex? mu = null,
            double thickness = 0.0) 
            : this(epsilon: (w) => epsilon, 
                  mu: (mu != null)? (w) => (Complex)mu : null,
                  thickness: thickness)
        { }

        /// <summary>
        /// constructs a UniformLayer with uniform 
        /// refractive index as a constant
        /// </summary>
        /// <param name="n"> refractive index @wavelength </param>
        /// <param name="thickness"> thickness of the uniform layer </param>
        public UniformLayer(Complex n,
            double thickness = 0.0)
            : this(epsilon: (w) => n*n, mu: null, thickness: thickness)
        { }

        #endregion
        #region ==== eigenvalues ====

        /// <summary>
        /// computes nz i.e. kz/k0 normalized
        /// for scalar nx and ny (i.e. kx/k0 and ky/k0)
        /// </summary>
        /// <param name="wavelength"> wavelength in vacuum </param>
        /// <param name="nx"> normalized kx (nx = kx/k0) </param>
        /// <param name="ny"> normalized ky (ny = ky/k0) </param>
        /// <param name="scal"> additional scaling factor </param>
        /// <returns> result = (scal*) nz </returns>
        public Complex ComputeNz(double wavelength,
            double nx, double ny = 0.0,
            double scal = 1.0)
        {
            // gets permittivity and permeability @wavelength
            Complex epsilon = Epsilon.Invoke(wavelength);
            Complex mu = Mu.Invoke(wavelength);
            // computes nz
            return scal * Complex.Sqrt(epsilon * mu - nx * nx - ny * ny);
        }

        /// <summary>
        /// computes nz i.e. kz/k0 normalized
        /// for vectorized kx and scalar ky
        /// </summary>
        /// <param name="wavelength"> wavelength in vacuum </param>
        /// <param name="nx"> normalized kx (nx = kx/k0) </param>
        /// <param name="ny"> normalized ky (ny = ky/k0) </param>
        /// <param name="scal"> additional scaling factor </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> result = (scal*) nz </returns>
        public VectorZ ComputeNz(double wavelength,
            VectorD nx, double ny = 0.0,
            double scal = 1.0,
            LoopMode loopMode = Defaults.LoopOption)
        {
            // gets permittivity and permeability @wavelength
            Complex epsilon = Epsilon(wavelength);
            Complex mu = Mu(wavelength);
            // prepares parameters
            Complex emp = epsilon * mu;
            double ny2 = ny * ny;
            VectorZ nz = new(nx.Count);

            // defines loop operation
            Action<long> a = (i) =>
            {
                double nxi = nx[i, checkBound: false];
                nz[i, checkBound: false] = scal * Complex.Sqrt(emp - nxi * nxi - ny2);
            };
            Loop1D loop = new(operation: a, start: 0, end: nz.Count, step: 1);
            loop.Evaluate(mode: loopMode);

            /* loop mode switch
            switch (loopMode)
            {
                case LoopMode.Sequential:
                    {
                        for (long i = 0; i < nz.Count; i++)
                        {
                            double inx = nx[i, false];
                            double nx2 = inx * inx;
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
            */

            // return
            return nz;
        }

        /// <summary>
        /// computes nz i.e. kz/k0 normalized
        /// for vectorized kx and vectorized ky
        /// </summary>
        /// <param name="wavelength"> wavelength in vacuum </param>
        /// <param name="nx"> normalized kx (nx = kx/k0) </param>
        /// <param name="ny"> normalized ky (ny = ky/k0) </param>
        /// <param name="scal"> additional scaling factor </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> result = (scal*) nz </returns>
        public MatrixZ ComputeNz(double wavelength,
            VectorD nx, VectorD ny,
            double scal = 1.0,
            LoopMode loopMode = Defaults.LoopOption)
        {
            // gets permittivity and permeability @wavelength
            Complex epsilon = Epsilon.Invoke(wavelength);
            Complex mu = Mu.Invoke(wavelength);
            // prepares parameters
            Complex emp = epsilon * mu;
            MatrixZ nz = new(ny.Count, nx.Count);

            // defines loop operation
            Action<long, long> a = (iRow, iCol) =>
            {
                double nyi = ny[iRow, checkBound: false];
                double nxi = nx[iCol, checkBound: false];
                nz[iRow, iCol, checkBound: false] = scal * Complex.Sqrt(emp - nxi * nxi - nyi * nyi);
            };
            Loop2D loop = new(operation: a,
                rowStart: 0, rowEnd: nz.Rows,
                colStart: 0, colEnd: nz.Cols,
                rowStep: 1, colStep: 1);
            loop.Evaluate(mode: loopMode);

            /* loop mode switch
            switch (loopMode)
            {
                case LoopMode.Sequential:
                    {
                        for (long i = 0; i < nz.Rows * nz.Cols; i++)
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
            */

            // return
            return nz;
        }

        #endregion
        #region ==== eigenmodes ====

        #region ---- in-plane ----

        /// <summary>
        /// computes the eigen information for in-plane case
        /// TM mode: [E] = [E_x], [H] = [H_y]
        /// TE mode: [E] = [E_y], [H] = [H_x]
        /// </summary>
        /// <param name="wavelength"> wavelength in vacuum </param>
        /// <param name="nx"> normalized kx (nx = kx/k0) </param>
        /// <param name="mode"> in-plane polarization mode: TE or TM </param>
        /// <returns> collection of eigen-info (nz, w1, w2) </returns>
        public (Complex, Complex, Complex) ComputeInPlaneModes(double wavelength,
            double nx,
            InPlanePolMode mode = InPlanePolMode.TE)
        {
            // computes nz
            Complex nz = ComputeNz(wavelength: wavelength, nx: nx);
            // gets permittivity and permeability @wavelength
            Complex epsilon = Epsilon(wavelength);
            Complex mu = Mu(wavelength);
            // computes w1, w2
            Complex w1 = 1.0;
            Complex w2 = mode switch
            {
                InPlanePolMode.TE => -nz / mu,
                InPlanePolMode.TM => epsilon / nz,
                _ => -nz / mu
            };
            // return
            return (nz, w1, w2);
        }

        /// <summary>
        /// computes the eigen information for in-plane case
        /// TM mode: [E] = [E_x], [H] = [H_y]
        /// TE mode: [E] = [E_y], [H] = [H_x]
        /// </summary>
        /// <param name="wavelength"> wavelength in vacuum </param>
        /// <param name="nx"> normalized kx (nx = kx/k0) </param>
        /// <param name="mode"> in-plane polarization mode: TE or TM </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> collection of eigen-info (nz, w1, w2) </returns>
        public (VectorZ, VectorZ, VectorZ) ComputeInPlaneModes(double wavelength,
            VectorD nx,
            InPlanePolMode mode = InPlanePolMode.TE,
            LoopMode loopMode = Defaults.LoopOption)
        {
            // computes nz
            VectorZ nz = ComputeNz(wavelength: wavelength, nx: nx, loopMode: loopMode);
            // gets permittivity and permeability @wavelength
            Complex epsilon = Epsilon(wavelength);
            Complex mu = Mu(wavelength);
            // computes w1 and w2
            VectorZ w1 = new(count: nx.Count, initVal: 1.0);
            VectorZ w2 = new(count: nx.Count);

            // defines loop operation
            Action<long> a = (i) =>
            {
                w2[i, checkBound: false] = mode switch
                {
                    InPlanePolMode.TE => -nz[i, checkBound: false] / mu,
                    InPlanePolMode.TM => epsilon / nz[i, checkBound: false],
                    _ => -nz[i, checkBound: false] / mu
                };
            };
            Loop1D loop = new(operation: a, start: 0, end: nx.Count, step: 1);
            loop.Evaluate(mode: loopMode);

            /*
            switch (LoopOption)
            {
                case LoopMode.Sequential:
                    {
                        for (long i = 0; i < nx.Count; i++)
                        {
                            w2[i, false] = mode switch
                            {
                                InPlanePolMode.TE => -nz[i, false] / mu,
                                InPlanePolMode.TM => epsilon / nz[i, false],
                                _ => -nz[i, false] / mu
                            };
                        }
                        break;
                    }
                case LoopMode.Parallel:
                    {
                        Parallel.For(0, nx.Count, i =>
                        {
                            w2[i, false] = mode switch
                            {
                                InPlanePolMode.TE => -nz[i, false] / mu,
                                InPlanePolMode.TM => epsilon / nz[i, false],
                                _ => -nz[i, false] / mu
                            };
                        });
                        break;
                    }
                case LoopMode.Vectorized:
                    {
                        w2 = mode switch
                        {
                            InPlanePolMode.TE => (mu == 1.0) ? -nz : VMath.Scale(nz, -1.0 / mu),
                            InPlanePolMode.TM => epsilon / nz,
                            _ => (mu == 1.0) ? -nz : VMath.Scale(nz, -1.0 / mu)
                        };
                        break;
                    }
                default: goto case LoopMode.Sequential;
            }
            */

            // return 
            return (nz, w1, w2);
        }

        #endregion
        #region ---- conical ----

        /// <summary>
        /// computes the eigen information for conical case
        /// W1 and W2 stored as 2x2 dense matrix
        /// </summary>
        /// <param name="wavelength"> wavelength in vacuum </param>
        /// <param name="nx"> normalized kx (nx = kx/k0) </param>
        /// <param name="ny"> normalized ky (ny = ky/k0) </param>
        /// <returns> collection of eigen-info (nz, w1, w2) </returns>
        public (Complex, MatrixZ, MatrixZ) ComputeConicalModes(double wavelength, double nx, double ny)
        {
            // computes nz
            Complex nz = ComputeNz(wavelength: wavelength, nx: nx, ny: ny);
            // gets permittivity and permeability @wavelength
            Complex epsilon = Epsilon.Invoke(arg: wavelength);
            Complex mu = Mu.Invoke(arg: wavelength);

            // w1 is diagonal unit matrix
            MatrixZ w1 = new(rows: 2, cols: 2)
            {
                [0, 0] = 1.0,
                [1, 1] = 1.0
            };
            // compute w2 (2x2 matrix)
            Complex wb = nx * ny / (mu * nz);
            Complex wc = nz / mu + nx * nx / (mu * nz);
            Complex wd = nz / mu + ny * ny / (mu * nz);
            MatrixZ w2 = new(rows: 2, cols: 2)
            {
                [0, 0] = -wb,
                [0, 1] = -wd,
                [1, 0] = wc,
                [1, 1] = wb
            };

            // return
            return (nz, w1, w2);
        }

        #endregion
        #region ---- 2D ----

        // ...

        #endregion

        #endregion

        #region ==== converting ====

        /// <summary>
        /// converts the coefficients to field for 1D case
        /// </summary>
        /// <param name="coefficients"></param>
        /// <param name="wavelength"></param>
        /// <param name="nx"></param>
        /// <param name="polarization"></param>
        /// <returns></returns>
        public VectorZ ConvertingToField1D(VectorZ coefficients, double wavelength, double nx, InPlanePolMode polarization)
        {
            if(coefficients.Count != 2)
            {
                throw new ArgumentException();
            }
            (_, Complex wL1, Complex wL2) = ComputeInPlaneModes(wavelength, nx, polarization);
            MatrixZ wL = new(rows: 2, cols: 2);
            wL[0, 0] = wL1; wL[0, 1] = wL1;
            wL[1, 0] = wL2; wL[1, 1] = -wL2;
            VectorZ field = LinAlg.Dot(wL, coefficients);
            return field;
        }

        /// <summary>
        /// converts the field to coefficients for 1D case
        /// </summary>
        /// <param name="field"></param>
        /// <param name="wavelength"></param>
        /// <param name="nx"></param>
        /// <param name="polarization"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public VectorZ ConvertingToCoefficients1D(VectorZ field, double wavelength, double nx, InPlanePolMode polarization)
        {
            if ( field.Count != 2)
            {
                throw new ArgumentException();
            }
            (_, Complex wL1, Complex wL2) = ComputeInPlaneModes(wavelength, nx, polarization);
            MatrixZ wL = new(rows: 2, cols: 2);
            wL[0, 0] = wL1; wL[0, 1] = wL1;
            wL[1, 0] = wL2; wL[1, 1] = -wL2;
            VectorZ coefficients = LinAlg.LinearSolve(wL, field);
            return coefficients;
        }

        /// <summary>
        /// converts the coefficients to field for 2D case
        /// </summary>
        /// <param name="coefficients"></param>
        /// <param name="wavelength"></param>
        /// <param name="nx"></param>
        /// <param name="ny"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public VectorZ ConvertingToField2D(VectorZ coefficients, double wavelength, double nx, double ny)
        {
            if(coefficients.Count != 4)
            {
                throw new ArgumentException();
            }
            (_, MatrixZ w1, MatrixZ w2) = ComputeConicalModes(wavelength, nx, ny);
            MatrixZ w = new MatrixZ(4, 4);
            w[new LongRange(0, 2), new LongRange(0, 2)] = w1;
            w[new LongRange(0, 2), new LongRange(2, 4)] = w1;
            w[new LongRange(2, 4), new LongRange(0, 2)] = w2;
            w[new LongRange(2, 4), new LongRange(2, 4)] = -w2;
            VectorZ field = LinAlg.Dot(w, coefficients);
            return field;
        }

        /// <summary>
        /// converts the field to coefficients for 2D case
        /// </summary>
        /// <param name="field"></param>
        /// <param name="wavelength"></param>
        /// <param name="nx"></param>
        /// <param name="ny"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public VectorZ ConvertingToCoefficients2D(VectorZ field, double wavelength, double nx, double ny)
        {
            if (field.Count != 4)
            {
                throw new ArgumentException();
            }
            (_, MatrixZ w1, MatrixZ w2) = ComputeConicalModes(wavelength, nx, ny);
            MatrixZ w = new MatrixZ(4, 4);
            w[new LongRange(0, 2), new LongRange(0, 2)] = w1;
            w[new LongRange(0, 2), new LongRange(2, 4)] = w1;
            w[new LongRange(2, 4), new LongRange(0, 2)] = w2;
            w[new LongRange(2, 4), new LongRange(2, 4)] = -w2;
            VectorZ coefficients = LinAlg.LinearSolve(w, field);
            return coefficients;
        }

        #endregion

        // only VMath for RCWA routines
        #region ==== RCWA1Dp ====

        /// <summary>
        /// computes the eigen information for in-plane case
        /// used for 1D RCWA calculation
        /// TM mode: [H] = [H_y] \\ [E] = [E_x]
        /// TE mode: [E] = [E_y] \\ [H] = [H_x]
        /// </summary>
        /// <param name="wavelength"> working wavelength in vacuum </param>
        /// <param name="polarization"> in-plane polarization mode </param>
        /// <param name="gKx"> uniform grid that defines the kx-values </param>
        /// <returns> (nz[vec], w1[vec], w2[vec]) </returns>
        public (VectorZ, VectorZ, VectorZ) ComputeInPlaneModes(double wavelength,
            InPlanePolMode polarization,
            GridInfo1D gKx)
        {
            // prepares parameters
            double k0 = 2.0 * Math.PI / wavelength;
            Complex epsilon = Epsilon(wavelength);
            Complex mu = Mu(wavelength);
            Complex emp = epsilon * mu;

            // generates kx-values
            VectorD kx = gKx.GetCoordinates();
            VectorD nx = kx / k0;

            // computes eigenvalues
            VectorZ nz = VMath.Sqrt(emp - VMath.Square(nx));
            // computes w1 and w2
            VectorZ w1 = new(count: nx.Count, initVal: 1.0); 
            VectorZ w2;
            switch(polarization)
            {
                case InPlanePolMode.TE:
                    w2 = (mu == 1.0) ? -nz : VMath.Scale(x: nz, a: -1.0 / mu);
                    break;
                case InPlanePolMode.TM:
                    w2 = epsilon / nz;
                    break;
                default: goto case InPlanePolMode.TE;
            }

            // return
            return (nz, w1, w2);
        }

        #endregion
        #region ==== RCWA1Dc ====

        // ...

        #endregion
        #region ==== RCWA2D ====

        /// <summary>
        /// compute the modes for 2D case
        /// [E] = {[E_x], [E_y]}
        /// [H] = {[H_x], [H_y]}
        /// using the W1/2 summetry properties
        /// </summary>
        /// <param name="wavelength"> working wavelength in vacuum </param>
        /// <param name="nKxs"> number of spatial frequencies kx </param>
        /// <param name="nKys"> number of spatial frequencies ky </param>
        /// <param name="dKx"> uniform distance in kx </param>
        /// <param name="dKy"> uniform distance in ky </param>
        /// <param name="kx0"> center shift of kx </param>
        /// <param name="ky0"> center shift of ky </param>
        /// <param name="saveModesData"> whethet to save the computed modes data </param>
        /// <returns> (gamma[vec], w1[mat], w2[mat]) </returns>
        public (VectorZ, MatrixZ, MatrixZ) ComputeModes(double wavelength,
            long nKxs, long nKys,
            double dKx, double dKy,
            double kx0 = 0.0, double ky0 = 0.0,
            bool saveModesData = false)
        {
            // generate kx and ky vector
            VectorD kx = EigenHelper.GenerateKs(n: nKxs, dk: dKx, kc: kx0);
            VectorD ky = EigenHelper.GenerateKs(n: nKys, dk: dKy, kc: ky0);
            // normalize kx, ky
            double k0 = 2.0 * Math.PI / wavelength;
            VMath.ScaleOn(ref kx, 1.0 / k0);
            VMath.ScaleOn(ref ky, 1.0 / k0);
            long n = kx.Count * ky.Count;
            VectorD nx = new(count: n);
            VectorD ny = new(count: n);
            for (long iy = 0; iy < ky.Count; iy++)
            {
                for (long ix = 0; ix < kx.Count; ix++)
                {
                    nx[iy * kx.Count + ix, checkBound: false] = kx[ix, checkBound: false];
                    ny[iy * kx.Count + ix, checkBound: false] = ky[iy, checkBound: false];
                }
            }

            // calculate nz = kz/k0
            Complex epsilon = Epsilon(wavelength);
            Complex mu = Mu(wavelength);
            VectorZ nz = VMath.Sqrt(epsilon * mu - VMath.Square(nx) - VMath.Square(ny));
            // define eigenvalues gamma
            VectorZ gamma = new(count: 2 * n);
            // define eigenvectors w1 and w2
            MatrixZ w1 = new(rows: 2 * n, cols: 2 * n);
            MatrixZ w2 = new(rows: 2 * n, cols: 2 * n);
            // fill vector and matrix elements
            VectorZ g = VMath.Scale(nz, Complex.ImaginaryOne * k0); //Complex.ImaginaryOne * k0 * nz;
            VectorZ wB = new VectorZ(part: ny * nx, option: ComplexPart.RealPart) / nz;
            VectorZ wC = nz + new VectorZ(part: VMath.Square(nx), option: ComplexPart.RealPart) / nz;
            VectorZ wD = nz + new VectorZ(part: VMath.Square(ny), option: ComplexPart.RealPart) / nz;
            for (long i = 0; i < n; i++)
            {
                // gamma
                gamma[i, checkBound: false] = g[i, checkBound: false];
                gamma[n + i, checkBound: false] = g[i, checkBound: false];
                // w1
                w1[i, i, checkBound: false] = 1.0; // w1[11]
                w1[n + i, n + i, checkBound: false] = 1.0; // w1[22]
                // w2
                w2[i, i, checkBound: false] = -wB[i, checkBound: false]; // w2[11]
                w2[i, n + i, checkBound: false] = -wD[i, checkBound: false]; // w2[12]
                w2[n + i, i, checkBound: false] = wC[i, checkBound: false]; // w2[21]
                w2[n + i, n + i, checkBound: false] = wB[i, checkBound: false]; // w2[22]
            }

            // save modes data?
            if (saveModesData)
            {
                Gamma = new(other: gamma, deepCopy: true);
                W1 = new(other: w1, deepCopy: true);
                W2 = new(other: w2, deepCopy: true);
            }

            // return
            return (gamma, w1, w2);
        }

        #endregion


        // obsolete ...
        #region ---- 1D eigensolver ----

        #region ===== conical (ky != 0) =====

        /// <summary>
        /// computes the modes for conical case (ky!=0)
        /// [E] = {[E_x], [E_y]}
        /// [H] = {[H_x], [H_y]}
        /// using the W1/2 symmetry properties
        /// </summary>
        /// <param name="wavelength"> working wavelength in vacuum </param>
        /// <param name="nKxs"> number of spatial frequencies kx </param>
        /// <param name="dKx"> uniform distance in kx </param>
        /// <param name="ky"> the transverse spatial frequency in y </param>
        /// <param name="kx0"> center shift of kx </param>
        /// <returns> (gamma[vec], w1[mat], w2[mat]) </returns>
        public (VectorZ, MatrixZ, MatrixZ) ComputeConicalModes(double wavelength,
            long nKxs,
            double dKx,
            double ky,
            double kx0 = 0.0)
        {
            // generate kx vector
            VectorD kx = EigenHelper.GenerateKs(nKxs, dKx, kx0);
            // normalize kx, ky
            double k0 = 2.0 * Math.PI / wavelength;
            VectorD nx = kx / k0;
            double ny = ky / k0;
            // calculate nz = kz/k0
            Complex epsilon = Epsilon(wavelength);
            Complex mu = Mu(wavelength);
            VectorZ nz = VMath.Sqrt(epsilon * mu - (ny * ny) - VMath.Square(nx));
            // define eigenvalues gamma, length = 2 * nx.Count
            VectorZ gamma = new(2 * nx.Count, 0.0);
            // define eigenvectors w1 and w2, size = [2 * nx.Count, 2 * nx.Count]
            MatrixZ w1 = new(2 * nx.Count, 2 * nx.Count, 0.0);
            MatrixZ w2 = new(2 * nx.Count, 2 * nx.Count, 0.0);
            // fill vector and matrix elements
            for (long i = 0; i < nx.Count; i++)
            {
                // gamma
                Complex g0 = Complex.ImaginaryOne * k0 * nz[i, false];
                gamma[i, false] = g0; // Ex-part
                gamma[nz.Count + i, false] = g0; // Ey-part
                // w1
                w1[i, i, false] = 1.0; // w1[11]
                w1[nz.Count + i, nz.Count + i, false] = 1.0; // w1[22]
                // w2
                double ny2 = ny * ny;
                Complex wB = ny * nx[i, false] / nz[i, false];
                Complex wC = nz[i, false] + nx[i, false] * nx[i, false] / nz[i, false];
                Complex wD = nz[i, false] + ny2 / nz[i, false];
                w2[i, i, false] = -wB; // w2[11]
                w2[i, nx.Count + i, false] = -wD; // w2[12]
                w2[nx.Count + i, i, false] = wC; // w2[21]
                w2[nx.Count + i, nx.Count + i, false] = wB; // w2[22]
            }
            // return
            return (gamma, w1, w2);
        }

        #endregion

        #endregion
        #region ---- 2D eigensolver ----

        /// <summary>
        /// compute the modes for 2D case
        /// [E] = {[E_x], [E_y]}
        /// [H] = {[H_x], [H_y]}
        /// using the W1/2 summetry properties
        /// </summary>
        /// <param name="wavelength"> working wavelength in vacuum </param>
        /// <param name="nKxs"> number of spatial frequencies kx </param>
        /// <param name="nKys"> number of spatial frequencies ky </param>
        /// <param name="dKx"> uniform distance in kx </param>
        /// <param name="dKy"> uniform distance in ky </param>
        /// <param name="kx0"> center shift of kx </param>
        /// <param name="ky0"> center shift of ky </param>
        /// <returns> (gamma, w1[11], w1[22], w2[11], w2[12], w2[21], w2[22]) </returns>
        public (VectorZ, VectorZ, VectorZ, VectorZ, VectorZ, VectorZ, VectorZ) ComputeModesSimp(double wavelength,
            long nKxs, long nKys,
            double dKx, double dKy,
            double kx0 = 0.0, double ky0 = 0.0)
        {
            // generate kx and ky vector
            VectorD kx = EigenHelper.GenerateKs(nKxs, dKx, kx0);
            VectorD ky = EigenHelper.GenerateKs(nKys, dKy, ky0);
            // normalize kx, ky
            double k0 = 2.0 * Math.PI / wavelength;
            VMath.ScaleOn(ref kx, 1.0 / k0);
            VMath.ScaleOn(ref ky, 1.0 / k0);
            long n = kx.Count * ky.Count;
            VectorD nx = new(n, 0.0);
            VectorD ny = new(n, 0.0);
            for (long iy = 0; iy < ky.Count; iy++)
            {
                for (long ix = 0; ix < kx.Count; ix++)
                {
                    nx[iy * kx.Count + ix, false] = kx[ix, false];
                    ny[iy * kx.Count + ix, false] = ky[iy, false];
                }
            }
            // calculate nz = kz/k0
            Complex epsilon = Epsilon(wavelength);
            Complex mu = Mu(wavelength);
            VectorZ nz = VMath.Sqrt(epsilon * mu - VMath.Square(nx) - VMath.Square(ny));
            // define eigenvalues gamma
            VectorZ gamma = new(2 * nx.Count, 0.0);
            gamma[new LongRange(0, nx.Count)] = nz;
            gamma[new LongRange(nx.Count, 2 * nx.Count)] = nz;
            VMath.ScaleOn(ref gamma, Complex.ImaginaryOne * k0);
            // define auxiliary variables
            VectorZ wB = new VectorZ(ny * nx) / nz;
            VectorZ wC = nz + new VectorZ(VMath.Square(nx)) / nz;
            VectorZ wD = nz + new VectorZ(VMath.Square(ny)) / nz;
            // define eigenvectors w1 and w2
            VectorZ w111 = new(n, 1.0);
            VectorZ w122 = new(n, 1.0);
            //MatrixZ w1 = new(2 * n, 2 * n, 0.0);
            //MatrixZ w2 = new(2 * n, 2 * n, 0.0);
            VectorZ w211 = VMath.Scale(wB, -1.0);
            VectorZ w212 = VMath.Scale(wD, -1.0);
            VectorZ w221 = VMath.Scale(wC, 1.0);
            VectorZ w222 = VMath.Scale(wB, 1.0);

            // return
            return (gamma, w111, w122, w211, w212, w221, w222);
        }

        #endregion

    }

}
