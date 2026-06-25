using System.Numerics;
using VEMS.MathCore;

namespace VEMS.EMSolver
{

    /// <summary>
    /// 2D-periodic isotropic layer
    /// for use in RCWA
    /// </summary>
    public class Periodic2DLayer
    {
        #region properties

        /// <summary>
        /// sampling grid information for
        /// permittivity and permeability
        /// </summary>
        public GridInfo2D? Grid { get; set; }

        /// <summary>
        /// period along the x-direction
        /// </summary>
        public double PeriodX { get; set; }

        /// <summary>
        /// period along the y-direction
        /// </summary>
        public double PeriodY { get; set; }

        /// <summary>
        /// layer medium that defines the permittivity epsilon = f(λ; x, y)
        /// and permeability mu = f(λ; x, y)
        /// </summary>
        public Layer2DMedium Medium { get; set; }

        /// <summary>
        /// permittivity distribution @ varying wavelength
        /// </summary>
        public Func<double, GridInfo2D, MatrixZ> Epsilon { get; set; }

        /// <summary>
        /// sampled permittivity data
        /// </summary>
        public MatrixZ? EpsilonData { get; set; }

        /// <summary>
        /// permeability distribution @ varying wavelength
        /// </summary>
        public Func<double, GridInfo2D, MatrixZ>? Mu { get; set; }

        /// <summary>
        /// sampled permeability data
        /// </summary>
        public MatrixZ? MuData { get; set; }

        /// <summary>
        /// thickness of the layer
        /// </summary>
        public double Thickness { get; set; }

        /// <summary>
        /// eigenvalues Gamma
        /// </summary>
        public VectorZ? Gamma { get; set; }

        /// <summary>
        /// eigenvector matrix W1
        /// </summary>
        public MatrixZ? W1 { get; set; }

        /// <summary>
        /// eigenvector matrix W2
        /// </summary>
        public MatrixZ? W2 { get; set; }

        #endregion
        #region consturctor

        /// <summary>
        /// constructs a 2D periodic layer
        /// </summary>
        /// <param name="periodX"> period along x-direction </param>
        /// <param name="periodY"> peirod along y-direction </param>
        /// <param name="medium"> layer medium containing the permittivity and permeability </param>
        /// <param name="thickness"> thickness of the layer </param>
        public Periodic2DLayer(double periodX, double periodY,
            Layer2DMedium medium,
            double thickness)
        {
            PeriodX = periodX;
            PeriodY = periodY;
            Medium = medium;
            Thickness = thickness;
        }

        /// <summary>
        /// constructs a 2D periodic layer class
        /// with gridded permittivity and permeability 
        /// </summary>
        /// <param name="periodX"> period along x-direction </param>
        /// <param name="periodY"> period along y-direction </param>
        /// <param name="epsilon"> permittivity distribution </param>
        /// <param name="mu"> permeability distribution </param>
        /// <param name="thickness"> the thickness of the layer </param>
        public Periodic2DLayer(double periodX, double periodY,
            Func<double, GridInfo2D, MatrixZ> epsilon,
            Func<double, GridInfo2D, MatrixZ> mu,
            double thickness)
        {
            // consistency check
            //if (grid.Rows != epsilon.Rows || grid.Cols != epsilon.Cols)
            //    throw new ArgumentException("Unequal count", nameof(epsilon));
            //if (grid.Rows != mu.Rows || grid.Cols != mu.Cols)
            //    throw new ArgumentException("Unequal count", nameof(mu));

            //Grid = grid;
            PeriodX = periodX;
            PeriodY = periodY;
            Epsilon = epsilon;
            Mu = mu;
            Thickness = thickness;
        }

        /// <summary>
        /// constructs a 2D periodic layer class
        /// with gridded permittivity distribution
        /// and default permeability equal 1.0
        /// </summary>
        /// <param name="periodX"> period along x-direction </param>
        /// <param name="periodY"> period along y-direction </param>
        /// <param name="epsilon"> permittivity distribution </param>
        /// <param name="thickness"> the thickness of the layer </param>
        public Periodic2DLayer(double periodX, double periodY,
            Func<double, GridInfo2D, MatrixZ> epsilon,
            double thickness)
        {
            // consistency check
            //if (grid.Rows != epsilon.Rows || grid.Cols != epsilon.Cols)
            //    throw new ArgumentException("Unequal count", nameof(epsilon));

            //Grid = grid;
            PeriodX = periodX;
            PeriodY = periodY;
            Epsilon = epsilon;
            Thickness = thickness;
        }

        #endregion
        #region ---- FMM ----


        public (VectorZ, MatrixZ, MatrixZ) ComputeModes(double wavelength,
            long fieldsSamplingX, long fieldsSamplingY,
            long mediumSamplingX, long mediumSamplingY,
            double kx0 = 0.0, double ky0 = 0.0,
            ToeplitzMatrixType toeplitztype = ToeplitzMatrixType.Nonperiodic,
            bool saveMediaData = true,
            bool saveModesData = false)
        {
            #region preparations

            double k0 = 2.0 * Math.PI / wavelength;
            // structure information
            double dKx = 2.0 * Math.PI / PeriodX;
            double dKy = 2.0 * Math.PI / PeriodY;
            // generates kx, ky
            VectorD kx = EigenHelper.GenerateKs(n: fieldsSamplingX, dk: dKx, kc: kx0);
            VectorD ky = EigenHelper.GenerateKs(n: fieldsSamplingY, dk: dKy, kc: ky0);
            // normalizes kx and ky (normalized by k0)
            VMath.ScaleOn(ref kx, 1.0 / k0);
            VMath.ScaleOn(ref ky, 1.0 / k0);
            // prepare sampled epsilon and mu
            (MatrixZ epsilon, MatrixZ? mu) = SampleMedium(wavelength,
                nx: mediumSamplingX, ny: mediumSamplingY,
                saveMediaData: saveMediaData);

            #endregion
            #region eigen-decomposition

            // computes F and G matrix
            (MatrixZ F, MatrixZ G) = FG(kx, ky, epsilon, mu, ToeplitzMatrixType.Nonperiodic);
            // construct eigen-matrix
            MatrixZ E = LinAlg.Dot(F, G);
            // matrix eigen-decomposition
            LinAlg.EigenSystem(ref E, out VectorZ eigenValues, out MatrixZ eigenVectors);
            // gamma
            VectorZ gamma = Complex.ImaginaryOne * k0 * VMath.Sqrt(eigenValues);
            EigenHelper.CheckGamma(ref gamma);
            // w1, w2 calculation
            MatrixZ w1 = eigenVectors;
            MatrixZ w2 = -Complex.ImaginaryOne / k0 * LinAlg.DiagonalMatrixHelper.Dot(w1, gamma);
            LinAlg.LinearSolve(ref F, ref w2);  // ... [F]^-1 product ...- fix
            // save mode data? May need huge storage ...
            if (saveModesData)
            {
                Gamma = new(gamma, true);
                W1 = new(w1, true);
                W2 = new(w2, true);
            }

            #endregion

            // return
            return (gamma, w1, w2);
        }

        /// <summary>
        /// samples the medium at a fixed wavelength
        /// </summary>
        /// <param name="wavelength"> wavelength in vacuum </param>
        /// <param name="nx"></param>
        /// <param name="ny"></param>
        /// <param name="saveMediaData"></param>
        /// <returns></returns>
        private (MatrixZ, MatrixZ?) SampleMedium(double wavelength,
            long nx, long ny,
            bool saveMediaData = true)
        {
            // makes uniform sampling grid
            GridInfo2D grid = new(rows: ny, cols: nx, spacingY: PeriodY / ny, spacingX: PeriodX / nx);
            // sampling
            MatrixZ epsilon = Medium.Sample(wavelength, grid, 
                matProperty: MaterialProperty.Epsilon,
                loopMode: Defaults.LoopOption,
                cacheSampleData: saveMediaData);
            MatrixZ? mu = (Medium.Mu == null)?
                null : Medium.Sample(wavelength, grid,
                matProperty: MaterialProperty.Mu,
                loopMode: Defaults.LoopOption,
                cacheSampleData: saveMediaData);
            // return
            return (epsilon, mu);
        }

        /// <summary>
        /// constructs the F and G matrices
        /// </summary>
        /// <param name="kx"> normalized kx vector </param>
        /// <param name="ky"> normalized ky vector </param>
        /// <param name="epsilon"> sampled permittivity matrix </param>
        /// <param name="mu"> sampled permeability matrix </param>
        /// <param name="toeplitztype">Type of Toeplitz matrix (Periodic or Nonperiodic).</param>
        /// <returns> F and G matrix </returns>
        private (MatrixZ, MatrixZ) FG(VectorD kx, VectorD ky,
            MatrixZ epsilon, MatrixZ? mu,
            ToeplitzMatrixType toeplitztype = ToeplitzMatrixType.Nonperiodic)
        {
            // defines matrix ranges
            long n = kx.Count * ky.Count;
            // constructs nx and ny
            VectorD nx = new(count: n);
            VectorD ny = new(count: n);
            for (long iy = 0; iy < ky.Count; iy++)
            {
                for (long ix = 0; ix < kx.Count; ix++)
                {
                    nx[iy * kx.Count + ix, false] = kx[ix, false];
                    ny[iy * kx.Count + ix, false] = ky[iy, false];
                }
            }
            // defiens matrix ranges
            LongRange up = new(start: 0, end: n);
            LongRange down = new(start: n, end: 2 * n);
            LongRange left = new(start: 0, end: n);
            LongRange right = new(start: n, end: 2 * n);
            MatrixZ epsilonDxDy = EigenHelper.DirXDirY(epsilon, kx.Count, ky.Count, toeplitztype: ToeplitzMatrixType.Nonperiodic);
            LinAlg.Inverse(ref epsilonDxDy); // [epsilon]^-1

            #region [F]
            // Omega13
            MatrixZ Omega13 = LinAlg.DiagonalMatrixHelper.Dot(epsilonDxDy, ny);
            LinAlg.DiagonalMatrixHelper.Dot(nx, ref Omega13);
            // Omega14
            MatrixZ Omega14 = LinAlg.DiagonalMatrixHelper.Dot(epsilonDxDy, nx);
            LinAlg.DiagonalMatrixHelper.Dot(-nx, ref Omega14);
            LinAlg.DiagonalMatrixHelper.AddTo(new VectorD(nx.Count, 1.0), ref Omega14);
            // Omega23
            MatrixZ Omega23 = LinAlg.DiagonalMatrixHelper.Dot(epsilonDxDy, ny);
            LinAlg.DiagonalMatrixHelper.Dot(ny, ref Omega23);
            LinAlg.DiagonalMatrixHelper.AddTo(new VectorD(nx.Count, -1.0), ref Omega23);
            // Omega24
            MatrixZ Omega24 = LinAlg.DiagonalMatrixHelper.Dot(epsilonDxDy, nx);
            LinAlg.DiagonalMatrixHelper.Dot(-ny, ref Omega24);
            // construct [F]
            MatrixZ F = new(2 * n, 2 * n, 0.0);
            F[up, left] = Omega13;
            F[up, right] = Omega14;
            F[down, left] = Omega23;
            F[down, right] = Omega24;
            #endregion
            #region [G]
            // Omega31
            MatrixZ Omega31 = LinAlg.DiagonalMatrixHelper.GenerateDenseMatrixZ(-nx * ny);
            // Omega32
            MatrixZ Omega32 = VMath.Scale(EigenHelper.InvYDirX(epsilon, kx.Count, ky.Count, toeplitztype: ToeplitzMatrixType.Nonperiodic), -1.0);
            LinAlg.DiagonalMatrixHelper.AddTo(VMath.Square(nx), ref Omega32);
            // Omega41
            MatrixZ Omega41 = EigenHelper.InvXDirY(epsilon, kx.Count, ky.Count, toeplitztype: ToeplitzMatrixType.Nonperiodic);
            LinAlg.DiagonalMatrixHelper.AddTo(-VMath.Square(ny), ref Omega41);
            // Omega42
            MatrixZ Omega42 = LinAlg.DiagonalMatrixHelper.GenerateDenseMatrixZ(ny * nx);
            // construct [G]
            MatrixZ G = new(2 * n, 2 * n, 0.0);
            G[up, left] = Omega31;
            G[up, right] = Omega32;
            G[down, left] = Omega41;
            G[down, right] = Omega42;
            #endregion

            return (F, G);
        }

        #endregion
        #region ---- FDE ----



        #endregion
        #region ---- BMM ----



        #endregion


        #region ---- eigensolver ----

        /// <summary>
        /// computes the modes for 2D case
        /// [E] = {[E_x], [E_y]}
        /// [H] = {[H_x], [H_y]}
        /// using W1/2 symmetry properties
        /// </summary>
        /// <param name="wavelength"> working wavelength in vacuum </param>
        /// <param name="nKxs"> number of spatial frequencies kx </param>
        /// <param name="nKys"> number of spatial frequencies ky </param>
        /// <param name="kx0"> center shift of kx </param>
        /// <param name="ky0"> center shift of ky </param>
        /// <param name="mediumOverSampX"> oversampling factor of the medium along x , >= 1.0 </param>
        /// <param name="mediumOverSampY"> oversampling factor of the medium along y, >= 1.0 </param>
        /// <param name="saveMediaData"> whether to save sampled media data </param>
        /// <param name="saveModesData"> whether to save computed modes data </param>
        /// <returns> (gamma[vec], w1[mat], w2[mat]) </returns>
        [Obsolete]
        public (VectorZ, MatrixZ, MatrixZ) ComputeModes(double wavelength,
            long nKxs, long nKys,
            double kx0 = 0.0, double ky0 = 0.0,
            double mediumOverSampX = 1.0, double mediumOverSampY = 1.0,
            bool saveMediaData = true,
            bool saveModesData = false)
        {
            // structure information
            double dKx = 2.0 * Math.PI / PeriodX;
            double dKy = 2.0 * Math.PI / PeriodY;
            // generate kx, ky
            VectorD kx = EigenHelper.GenerateKs(nKxs, dKx, kx0);
            VectorD ky = EigenHelper.GenerateKs(nKys, dKy, ky0);
            // normalize kx and ky (normalized by k0)
            double k0 = 2.0 * Math.PI / wavelength;
            VMath.ScaleOn(ref kx, 1.0 / k0);
            VMath.ScaleOn(ref ky, 1.0 / k0);
            // construct nx and ny
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
            // ranges
            LongRange up = new(0, n);
            LongRange down = new(n, 2 * n);
            LongRange left = new(0, n);
            LongRange right = new(n, 2 * n);

            // prepare epsilon and mu
            long numX = 2 * nKxs - 1;
            long numY = 2 * nKys - 1;
            if (mediumOverSampX > 1.0) { numX = (long)Math.Ceiling(mediumOverSampX * numX); }
            if (mediumOverSampY > 1.0) { numY = (long)Math.Ceiling(mediumOverSampY * numY); }
            if (numX % 2 == 0) { numX += 1; }
            if (numY % 2 == 0) { numY += 1; }
            GridInfo2D grid = new(numY, numX, PeriodY / numY, PeriodX / numX);
            MatrixZ epsilon = Epsilon(wavelength, grid);
            // save media data?
            if (saveMediaData)
            {
                Grid = new(grid);
                EpsilonData = new(epsilon, true);
                // mu ...
            }
            MatrixZ epsilonDxDy = EigenHelper.DirXDirY(epsilon, nKxs, nKys);
            LinAlg.Inverse(ref epsilonDxDy); // [epsilon]^-1

            #region [F]
            // Omega13
            MatrixZ Omega13 = LinAlg.DiagonalMatrixHelper.Dot(epsilonDxDy, ny);
            LinAlg.DiagonalMatrixHelper.Dot(nx, ref Omega13);
            // Omega14
            MatrixZ Omega14 = LinAlg.DiagonalMatrixHelper.Dot(epsilonDxDy, nx);
            LinAlg.DiagonalMatrixHelper.Dot(-nx, ref Omega14);
            LinAlg.DiagonalMatrixHelper.AddTo(new VectorD(nx.Count, 1.0), ref Omega14);
            // Omega23
            MatrixZ Omega23 = LinAlg.DiagonalMatrixHelper.Dot(epsilonDxDy, ny);
            LinAlg.DiagonalMatrixHelper.Dot(ny, ref Omega23);
            LinAlg.DiagonalMatrixHelper.AddTo(new VectorD(nx.Count, -1.0), ref Omega23);
            // Omega24
            MatrixZ Omega24 = LinAlg.DiagonalMatrixHelper.Dot(epsilonDxDy, nx);
            LinAlg.DiagonalMatrixHelper.Dot(-ny, ref Omega24);
            // construct [F]
            MatrixZ F = new(2 * n, 2 * n, 0.0);
            F[up, left] = Omega13;
            F[up, right] = Omega14;
            F[down, left] = Omega23;
            F[down, right] = Omega24;
            #endregion
            #region [G]
            // Omega31
            MatrixZ Omega31 = LinAlg.DiagonalMatrixHelper.GenerateDenseMatrixZ(-nx * ny);
            // Omega32
            MatrixZ Omega32 = VMath.Scale(EigenHelper.InvYDirX(epsilon, nKxs, nKys), -1.0);
            LinAlg.DiagonalMatrixHelper.AddTo(VMath.Square(nx), ref Omega32);
            // Omega41
            MatrixZ Omega41 = EigenHelper.InvXDirY(epsilon, nKxs, nKys);
            LinAlg.DiagonalMatrixHelper.AddTo(-VMath.Square(ny), ref Omega41);
            // Omega42
            MatrixZ Omega42 = LinAlg.DiagonalMatrixHelper.GenerateDenseMatrixZ(ny * nx);
            // construct [G]
            MatrixZ G = new(2 * n, 2 * n, 0.0);
            G[up, left] = Omega31;
            G[up, right] = Omega32;
            G[down, left] = Omega41;
            G[down, right] = Omega42;
            #endregion

            // construct eigen-matrix
            MatrixZ E = LinAlg.Dot(F, G);
            // matrix eigen-decomposition
            LinAlg.EigenSystem(ref E, out VectorZ eigenValues, out MatrixZ eigenVectors);
            // gamma
            VectorZ gamma = Complex.ImaginaryOne * k0 * VMath.Sqrt(eigenValues);
            EigenHelper.CheckGamma(ref gamma);
            // w1, w2 calculation
            MatrixZ w1 = eigenVectors;
            MatrixZ w2 = -Complex.ImaginaryOne / k0 * LinAlg.DiagonalMatrixHelper.Dot(w1, gamma);
            LinAlg.LinearSolve(ref F, ref w2);  // ... [F]^-1 product ...- fix
            // save mode data? May need huge storage ...
            if (saveModesData)
            {
                Gamma = new(gamma, true);
                W1 = new(w1, true);
                W2 = new(w2, true);
            }

            // return
            return (gamma, w1, w2);
        }

        #endregion
    }

}
