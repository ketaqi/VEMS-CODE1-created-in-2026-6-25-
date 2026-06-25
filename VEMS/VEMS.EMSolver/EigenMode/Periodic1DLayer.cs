using Complex = System.Numerics.Complex;
using VEMS.MathCore;

namespace VEMS.EMSolver
{
    /// <summary>
    /// 1D-periodic isotopic layer 
    /// for use in RCWA
    /// </summary>
    public class Periodic1DLayer
    {
        #region properties

        /// <summary>
        /// period along x-direction
        /// </summary>
        public double Period { get; set; }

        /// <summary>
        /// layer medium that inherited by ILayerMedium that can be defined by function or piecewise value
        /// </summary>
        public ILayerMedium Medium { get; set; }

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
        /// constructs a 1D periodic layer 
        /// </summary>
        /// <param name="period"> period along x-direction </param>
        /// <param name="medium"> layer medium containing the permittivity and permeability </param>
        /// <param name="thickness"> the thickness of the layer </param>
        public Periodic1DLayer(double period,
            ILayerMedium medium,
            double thickness)
        {
            Period = period;
            Medium = medium;
            Thickness = thickness;
        }

        #endregion
        #region ---- FMM ----

        #region ==== in-plane (ky = 0)====
/*
        /// <summary>
        /// computes the in-plane modes (@ ky = 0)
        /// TM mode: [E] = [E_x], [H] = [H_y]
        /// TE mode: [E] = [E_y], [H] = [H_x]
        /// using W1/2 symmetry properties
        /// </summary>
        /// <param name="wavelength"> wavelength in vacuum </param>
        /// <param name="mode"> in-plane polarization mode </param>
        /// <param name="nKxs"> number of spatial frequencies kx </param>
        /// <param name="kx0"> center shift of kx </param>
        /// <param name="mediumOverSampX"> oversampling factor of the medium, >= 1.0 </param>
        /// <param name="saveMediaData"> whether to save sampled media data </param>
        /// <param name="saveModesData"> whether to save computed modes data </param>
        /// <returns> (gamma[vec], w1[mat], w2[mat]) </returns>
        [Obsolete]
        public (VectorZ, MatrixZ, MatrixZ) ComputeInPlaneModes(double wavelength,
            InPlanePolMode mode,
            long nKxs,
            double kx0 = 0.0,
            double mediumOverSampX = 1.0,
            bool saveMediaData = true,
            bool saveModesData = false)
        {
            #region preparations 

            double k0 = 2.0 * Math.PI / wavelength;
            // generates kx and nx
            VectorD kx = EigenHelper.GenerateKs(n: nKxs, dk: 2.0 * Math.PI / Period, kc: kx0);
            VectorD nx = kx / k0;
            // prepares epsilon and mu
            long n = 2 * nKxs - 1;
            if (mediumOverSampX > 1.0) { n = (long)Math.Ceiling(mediumOverSampX * n); }
            if (n % 2 == 0) { n += 1; }
            (VectorZ epsilon, VectorZ? mu) = SampleMedium(wavelength, n, saveMediaData);

            #endregion

            // construct [F] and [G]
            (MatrixZ? F, MatrixZ G) = mode switch
            {
                InPlanePolMode.TM => TMModeFG(nx, epsilon, mu),
                InPlanePolMode.TE => TEModeFG(nx, epsilon, mu),
                _ => TEModeFG(nx, epsilon, mu)
            };

            #region eigen-decomposition

            LinAlg.EigenSystem(ref G, out VectorZ eigenValues, out MatrixZ eigenVectors);
            // gamma
            VectorZ gamma = Complex.ImaginaryOne * k0 * VMath.Sqrt(eigenValues);
            EigenHelper.CheckGamma(ref gamma);
            // w1, w2 calculation
            MatrixZ w1 = eigenVectors;
            MatrixZ w2 = new(w1, true);
            LinAlg.DiagonalMatrixHelper.Dot(ref w2, gamma);
            VMath.ScaleOn(ref w2, -Complex.ImaginaryOne / k0);
            if (F != null)
                LinAlg.LinearSolve(ref F, ref w2);
            else
                VMath.ScaleOn(ref w2, -1.0); // [F] = -1
            // save modes data? may need hugh storage ...
            if (saveModesData)
            {
                Gamma = new(gamma, true);
                W1 = new(w1, true);
                W2 = new(w2, true);
            }

            #endregion

            // return
            return (gamma, w1, w2);
        }*/

        /// <summary>
        /// computes the in-plane modes (@ ky = 0)
        /// TM mode: [E] = [E_x], [H] = [H_y]
        /// TE mode: [E] = [E_y], [H] = [H_x]
        /// using W1/2 symmetry properties
        /// </summary>
        /// <param name="wavelength"> wavelength in vacuum </param>
        /// <param name="mode"> in-plane polarization mode </param>
        /// <param name="fieldsSampling"> (odd) number of spatial frequencies for E/H-field sampling </param>
        /// <param name="mediumSampling"> (odd) number of spatial frequencies for medium sampling </param>
        /// <param name="kx0"> lateral shift of spatial frequencies along kx </param>
        /// <param name="toeplitztype">Type of Toeplitz matrix (Periodic or Nonperiodic).</param>
        /// <param name="saveMediaData"> whether to save sampled medium data </param>
        /// <param name="saveModesData"> whether to save computed modes data </param>
        /// <returns> (gamma[vec], w1[mat], w2[mat]) </returns>
        public (VectorZ, MatrixZ, MatrixZ) ComputeInPlaneModes(double wavelength,
            InPlanePolMode mode,
            long fieldsSampling, // odd
            long mediumSampling, // odd
            double kx0 = 0.0,
            ToeplitzMatrixType toeplitztype = ToeplitzMatrixType.Nonperiodic,
            bool saveMediaData = true,
            bool saveModesData = false)
        {
            #region preparations 

            double k0 = 2.0 * Math.PI / wavelength;
            // generates kx and nx
            VectorD kx = EigenHelper.GenerateKs(n: fieldsSampling, dk: 2.0 * Math.PI / Period, kc: kx0);
            VectorD nx = kx / k0;

            (VectorZ epsilon, VectorZ? mu) = Medium.SampleMedium(wavelength, new GridInfo1D(mediumSampling, Period / mediumSampling),
                             loopMode: LoopMode.Sequential, cacheSampleData: saveMediaData);
            (VectorZ epsilonk, VectorZ? muk) = Medium.MediumInDirKdomain(wavelength, new GridInfo1D(mediumSampling, Period / mediumSampling), 
                             loopMode: LoopMode.Sequential, cacheSampleData: saveMediaData);
            (VectorZ epsilonInvk, VectorZ? muInvk) = Medium.MediumInInvKdomain(wavelength, new GridInfo1D(mediumSampling, Period / mediumSampling),
                             loopMode: LoopMode.Sequential, cacheSampleData: saveMediaData);
            // construct [F] and [G]
            (MatrixZ? F, MatrixZ G) = mode switch
            {
                InPlanePolMode.TM => TMModeFG(nx, epsilonk, muk, epsilonInvk, muInvk, toeplitztype: toeplitztype),
                InPlanePolMode.TE => TEModeFG(nx, epsilonk, muk, epsilonInvk, muInvk, toeplitztype: toeplitztype),
                _ => TEModeFG(nx, epsilonk, muk, epsilonInvk, muInvk, toeplitztype: toeplitztype)
            };
            
            #endregion
            #region eigen-decomposition
            LinAlg.EigenSystem(ref G, out VectorZ eigenValues, out MatrixZ eigenVectors);
            // gamma
            VectorZ gamma = Complex.ImaginaryOne * k0 * VMath.Sqrt(eigenValues);
            EigenHelper.CheckGamma(ref gamma);
            // w1, w2 calculation
            MatrixZ w1 = eigenVectors;
            MatrixZ w2 = new(other: w1, deepCopy: true);
            LinAlg.DiagonalMatrixHelper.Dot(ref w2, gamma);
            VMath.ScaleOn(ref w2, -Complex.ImaginaryOne / k0);
            if (F != null)
                LinAlg.LinearSolve(ref F, ref w2);
            else
                VMath.ScaleOn(ref w2, -1.0); // [F] = -1
            // save modes data? may need hugh storage ...
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
        /// computes F and G matrices for TE mode
        /// </summary>
        /// <param name="nx"> normalized spatial frequencies </param>
        /// <param name="epsilon"> sampled epsilon data in spatial domain </param>
        /// <param name="mu"> sampled mu data in spatial domain </param>
        /// <param name="invepsilon"> sampled epsilon data in spactial frequancy domain with using inverse rule </param>
        /// <param name="invmu"> sampled mu data in spactial frequancy domain with using inverse rule </param>
        /// <param name="toeplitztype">Type of Toeplitz matrix (Periodic or Nonperiodic).</param>
        /// <returns> F and G </returns>
        private (MatrixZ?, MatrixZ) TEModeFG(VectorD nx,
            VectorZ epsilon, 
            VectorZ? mu,
            VectorZ invepsilon,
            VectorZ? invmu,
            ToeplitzMatrixType toeplitztype = ToeplitzMatrixType.Nonperiodic)
        {
            // toeplitz matrices
            long nKxs = nx.Count;
            MatrixZ epsilonDx = EigenHelper.ToeplitzMatrix(f: epsilon, nKxs: nKxs, toeplitztype: toeplitztype);
            MatrixZ? muDx = (mu == null) ? null : EigenHelper.ToeplitzMatrix(f: mu, nKxs: nKxs, toeplitztype: toeplitztype);
            MatrixZ? muIx = (invmu == null) ? null : EigenHelper.ToeplitzMatrix(f: invmu, nKxs: nKxs, toeplitztype: toeplitztype);

            // constructs [F] and [G]
            MatrixZ? F;
            MatrixZ G = epsilonDx;
            if (muDx != null && muIx != null)
            {
                // [F] = -[1/mu]^-1
                // [G] = [nx][mu]^-1[nx] - [epsilon]
                F = muIx;
                LinAlg.Inverse(ref F); // this is -F
                // further computation of (-G)
                LinAlg.Inverse(ref muDx);
                LinAlg.DiagonalMatrixHelper.Dot(ref muDx, -nx);
                LinAlg.DiagonalMatrixHelper.Dot(nx, ref muDx);
                VMath.AddTo(muDx, ref G);
                // saves FG product to G
                G = LinAlg.Dot(F, G);
            }
            else
            {
                // let [F] = -1
                // let [G] = [nx^2] - [epsilon]
                F = null;
                // thus, add kx on the matrix diagonal
                LinAlg.DiagonalMatrixHelper.AddTo(-VMath.Square(nx), ref G);
            }

            return (F, G);
        }

        /// <summary>
        /// computes F and G matrices for TM mode
        /// </summary>
        /// <param name="nx"> normalized spatial frequencies </param>
        /// <param name="epsilon"> sampled epsilon data in spactial frequancy domain </param>
        /// <param name="mu"> sampled mu data in spactial frequancy domain </param>
        /// <param name="invepsilon"> sampled epsilon data in spactial frequancy domain with using inverse rule </param>
        /// <param name="invmu"> sampled mu data in spactial frequancy domain with using inverse rule </param>
        /// <param name="toeplitztype">Type of Toeplitz matrix (Periodic or Nonperiodic).</param>
        /// <returns> F and G </returns>
        private (MatrixZ?, MatrixZ) TMModeFG(VectorD nx,
            VectorZ epsilon, 
            VectorZ? mu,
            VectorZ invepsilon,
            VectorZ? invmu,
            ToeplitzMatrixType toeplitztype = ToeplitzMatrixType.Nonperiodic)
        {
            // toeplitz matrices
            long nKxs = nx.Count;
            MatrixZ epsilonDx = EigenHelper.ToeplitzMatrix(f: epsilon, nKxs: nKxs, toeplitztype: toeplitztype);
            MatrixZ epsilonIx = EigenHelper.ToeplitzMatrix(f: invepsilon, nKxs: nKxs, toeplitztype: toeplitztype);
            MatrixZ? muDx = (mu == null) ? null : EigenHelper.ToeplitzMatrix(f: mu, nKxs: nKxs, toeplitztype: toeplitztype);

            // construct [F] and [G]
            MatrixZ G = epsilonIx;
            LinAlg.Inverse(ref G);
            MatrixZ F = epsilonDx;
            LinAlg.Inverse(ref F);
            LinAlg.DiagonalMatrixHelper.Dot(-nx, ref F);
            LinAlg.DiagonalMatrixHelper.Dot(ref F, nx);
            if (muDx != null) { VMath.AddTo(muDx, ref F); }
            else
            {
                // [mu] = 1.0
                // thus, simply add 1.0 on the matrix diagonal 
                LinAlg.DiagonalMatrixHelper.AddTo(new VectorD(nx.Count, 1.0), ref F);
            }
            // saves FG product to G
            G = LinAlg.Dot(F, G);

            return (F, G);
        }

        #endregion
        #region ==== conical (ky != 0) ====

        // ...

        internal (VectorZ, MatrixZ, MatrixZ) ComputeConicalModes(double wavelength,
            long nKxs, double ky,
            double kx0 = 0.0,
            double mediumOverSampX = 1.0,
            bool saveMediaData = true,
            bool saveModesData = false)
        {


            return (null, null, null);

        }

        #endregion

        #endregion
        #region ---- FDE ----

        // ...

        #endregion
        #region ---- BMM ----

        // ...

        #endregion

    }
}
