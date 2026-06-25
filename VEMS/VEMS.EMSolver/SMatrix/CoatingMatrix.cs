using System.Numerics;
using VEMS.MathCore;

namespace VEMS.EMSolver
{

    /// <summary>
    /// s-matrix for multilayer-coating
    /// </summary>
    public class CoatingMatrix
    {
        #region ==== scalar kx ====

        #region ---- kernel ----

        /// <summary>
        /// Iteration kernel for half S-matrix calculation for multilayer coatings.
        /// Updates the S11 and S21 parameters for the current layer based on the previous layer's eigenvalues and eigenvectors.
        /// </summary>
        /// <param name="S11">Reference to S11 parameter (will be overwritten).</param>
        /// <param name="S21">Reference to S21 parameter (will be overwritten).</param>
        /// <param name="gammaLast">Eigenvalue of the last layer.</param>
        /// <param name="wLast">Eigenvector parameter of the last layer.</param>
        /// <param name="w">Eigenvector parameter of the current layer.</param>
        /// <param name="tLast">Thickness of the last layer.</param>
        internal static void HalfSMatrixIterKernel(
            ref Complex S11, ref Complex S21,
            Complex gammaLast, Complex wLast, Complex w, double tLast)
        {
            // calculate P-term
            Complex P = Complex.Exp(gammaLast * tLast);
            S21 = P * S21 * P;

            // calculate auxiliary variables q
            Complex q = wLast / w;

            // calculate t11, t12
            Complex t11 = 0.5 * (1.0 + q); // t22 = t11
            Complex t12 = 0.5 * (1.0 - q); // t21 = t12

            // update S-matrix
            Complex temp = t11 + t12 * S21;
            S11 = S11 * P / temp;
            S21 = (t12 + t11 * S21) / temp;
        }


        /// <summary>
        /// Iteration kernel for full S-matrix calculation for multilayer coatings.
        /// Updates the S11, S21, S12, and S22 parameters for the current layer based on the previous layer's eigenvalues and eigenvectors.
        /// </summary>
        /// <param name="S11">Reference to S11 parameter (will be overwritten).</param>
        /// <param name="S21">Reference to S21 parameter (will be overwritten).</param>
        /// <param name="S12">Reference to S12 parameter (will be overwritten).</param>
        /// <param name="S22">Reference to S22 parameter (will be overwritten).</param>
        /// <param name="gammaLast">Eigenvalue of the last layer.</param>
        /// <param name="wLast">Eigenvector parameter of the last layer.</param>
        /// <param name="w">Eigenvector parameter of the current layer.</param>
        /// <param name="tLast">Thickness of the last layer.</param>
        internal static void FullSMatrixIterKernel(
            ref Complex S11, ref Complex S21,
            ref Complex S12, ref Complex S22,
            Complex gammaLast, Complex wLast, Complex w, double tLast)
        {
            // calculate P-term
            Complex P = Complex.Exp(gammaLast * tLast);
            S21 = P * S21 * P;

            // calculate auxiliary variables q
            Complex q = wLast / w;

            // calculate t11, t12
            Complex t11 = 0.5 * (1.0 + q); // t22 = t11
            Complex t12 = 0.5 * (1.0 - q); // t21 = t12

            // update S-matrix
            Complex temp = t11 + t12 * S21;
            Complex invTemp = 1.0 / temp;
            S11 = S11 * P * invTemp;
            S21 = (t12 + t11 * S21) * invTemp; // t21 = t12, t22 = t11
            S12 = S12 - S11 * t12 * P * S22;
            S22 = (t11 - S21 * t12) * P * S22; // t22 = t11
        }

        #endregion
        #region ---- loop ----

        /// <summary>
        /// Computes the half S-matrix for a multilayer coating using the scalar kx approach.
        /// Iterates through all layers, updating the S11 and S21 parameters based on the eigenvalues and eigenvectors of each layer.
        /// </summary>
        /// <param name="wavelength">The working wavelength in vacuum.</param>
        /// <param name="nLayers">List of refractive indices for all layers (from incident to transmission medium).</param>
        /// <param name="tLayers">List of thicknesses for all layers (same order as <paramref name="nLayers"/>).</param>
        /// <param name="kx">Transverse (x) spatial frequency.</param>
        /// <param name="polarization">Polarization mode (TE or TM). Default is TE.</param>
        /// <returns>
        /// A tuple containing the computed S-matrix parameters:
        /// <list type="bullet">
        /// <item><description><c>S11</c>: Transmission coefficient from incident to transmission medium.</description></item>
        /// <item><description><c>S21</c>: Reflection coefficient from incident to transmission medium.</description></item>
        /// </list>
        /// </returns>
        internal static (Complex, Complex) HalfSMatrixLoop(double wavelength,
            List<Complex> nLayers,
            List<double> tLayers,
            double kx,
            InPlanePolMode polarization = InPlanePolMode.TE)
        {
            double k0 = 2.0 * Math.PI / wavelength;
            double nx = kx / k0;
            // initialize
            Complex s11 = Complex.One;
            Complex s21 = Complex.Zero;

            // eigen info in transmission medium (n2)
            UniformLayer layerLast = new(n: nLayers[^1]);
            (Complex nzLast, _, Complex wLast) = layerLast.ComputeInPlaneModes(
                wavelength: wavelength,
                nx: nx,
                mode: polarization);
            Complex gammaLast = Complex.ImaginaryOne * k0 * nzLast;

            // S-matrix loop through all layers
            Complex gamma, w;
            for (int i = nLayers.Count - 2; i >= 0; i--)
            {
                // eigen info in current layer
                UniformLayer layer = new(n: nLayers[i]);
                (Complex nz, _, w) = layer.ComputeInPlaneModes(
                    wavelength: wavelength,
                    nx: nx,
                    mode: polarization);
                gamma = Complex.ImaginaryOne * k0 * nz;

                // call iteration kernel
                HalfSMatrixIterKernel(ref s11, ref s21, gammaLast, wLast, w, tLayers[i + 1]);

                // update for the next round
                if (i != 0)
                {
                    gammaLast = gamma;
                    wLast = w;
                }
            }

            return (s11, s21);
        }


        /// <summary>
        /// Computes the full S-matrix for a multilayer coating using the scalar kx approach.
        /// Iterates through all layers, updating the S11, S21, S12, and S22 parameters based on the eigenvalues and eigenvectors of each layer.
        /// </summary>
        /// <param name="wavelength">The working wavelength in vacuum.</param>
        /// <param name="nLayers">List of refractive indices for all layers (from incident to transmission medium).</param>
        /// <param name="tLayers">List of thicknesses for all layers (same order as <paramref name="nLayers"/>).</param>
        /// <param name="kx">Transverse (x) spatial frequency.</param>
        /// <param name="polarization">Polarization mode (TE or TM). Default is TE.</param>
        /// <returns>
        /// A tuple containing the computed S-matrix parameters:
        /// <list type="bullet">
        /// <item><description><c>S11</c>: Transmission coefficient from incident to transmission medium.</description></item>
        /// <item><description><c>S21</c>: Reflection coefficient from incident to transmission medium.</description></item>
        /// <item><description><c>S12</c>: Transmission coefficient from transmission to incident medium.</description></item>
        /// <item><description><c>S22</c>: Reflection coefficient from transmission to incident medium.</description></item>
        /// </list>
        /// </returns>
        internal static (Complex, Complex, Complex, Complex) FullSMatrixLoop(
            double wavelength,
            List<Complex> nLayers,
            List<double> tLayers,
            double kx,
            InPlanePolMode polarization = InPlanePolMode.TE)
        {
            double k0 = 2.0 * Math.PI / wavelength;
            double nx = kx / k0;
            // initialize
            Complex s11 = Complex.One;
            Complex s21 = Complex.Zero;
            Complex s12 = Complex.Zero;
            Complex s22 = Complex.One;

            // eigen info in transmission medium (n2)
            UniformLayer layerLast = new(n: nLayers[^1]);
            (Complex nzLast, _, Complex wLast) = layerLast.ComputeInPlaneModes(
                wavelength: wavelength,
                nx: nx,
                mode: polarization);
            Complex gammaLast = Complex.ImaginaryOne * k0 * nzLast;

            // S-matrix loop through all layers
            Complex gamma, w;
            for (int i = nLayers.Count - 2; i >= 0; i--)
            {
                // eigen info in current layer
                UniformLayer layer = new(n: nLayers[i]);
                (Complex nz, _, w) = layer.ComputeInPlaneModes(
                    wavelength: wavelength,
                    nx: nx,
                    mode: polarization);
                gamma = Complex.ImaginaryOne * k0 * nz;

                // call iteration kernel
                FullSMatrixIterKernel(
                    ref s11, ref s21,
                    ref s12, ref s22,
                    gammaLast, wLast, w, tLayers[i + 1]);

                // update for the next round
                if (i != 0)
                {
                    gammaLast = gamma;
                    wLast = w;
                }
            }

            return (s11, s21, s12, s22);
        }

        #endregion

        #endregion
        #region ==== vector kx ====

        #region ---- kernel ----

        /// <summary>
        /// Iteration kernel for half S-matrix calculation for multilayer coatings.
        /// Updates the S11 and S21 parameters for the current layer based on the previous layer's eigenvalues and eigenvectors.
        /// </summary>
        /// <param name="S11">Reference to S11 parameter (will be overwritten).</param>
        /// <param name="S21">Reference to S21 parameter (will be overwritten).</param>
        /// <param name="gammaLast">Eigenvalue of the last layer.</param>
        /// <param name="wLast">Eigenvector parameter of the last layer.</param>
        /// <param name="w">Eigenvector parameter of the current layer.</param>
        /// <param name="tLast">Thickness of the last layer.</param>
        private static void HalfSMatrixIterKernel(ref VectorZ S11, ref VectorZ S21,
            VectorZ gammaLast, VectorZ wLast, VectorZ w, double tLast)
        {
            // calculate P-term
            VectorZ P = VMath.Exp(gammaLast * tLast); //Complex.Exp(gammaLast * tLast);
            S21 = S21 * VMath.Square(P); // P * S21 * P;
            // calculate auxiliary variables q
            VectorZ q = wLast / w;
            // calculate t11, t12
            VectorZ t11 = 0.5 * (1.0 + q); // t22 = t11
            VectorZ t12 = 0.5 * (1.0 - q); // t21 = t12
            // update S-matrix
            VectorZ temp = t11 + t12 * S21;
            S11 = S11 * P / temp;
            S21 = (t12 + t11 * S21) / temp;
        }

        /// <summary>
        /// Iteration kernel for full S-matrix calculation for multilayer coatings.
        /// Updates the S11, S21, S12, and S22 parameters for the current layer based on the previous layer's eigenvalues and eigenvectors.
        /// </summary>
        /// <param name="S11">Reference to S11 parameter (will be overwritten).</param>
        /// <param name="S21">Reference to S21 parameter (will be overwritten).</param>
        /// <param name="S12">Reference to S12 parameter (will be overwritten).</param>
        /// <param name="S22">Reference to S22 parameter (will be overwritten).</param>
        /// <param name="gammaLast">Eigenvalue of the last layer.</param>
        /// <param name="wLast">Eigenvector parameter of the last layer.</param>
        /// <param name="w">Eigenvector parameter of the current layer.</param>
        /// <param name="tLast">Thickness of the last layer.</param>
        private static void FullSMatrixIterKernel(ref VectorZ S11, ref VectorZ S21, ref VectorZ S12, ref VectorZ S22,
            VectorZ gammaLast, VectorZ wLast, VectorZ w, double tLast)
        {
            // calculate P-term
            VectorZ P = VMath.Exp(gammaLast * tLast); //Complex.Exp(gammaLast * tLast);
            S21 = S21 * VMath.Square(P); // P * S21 * P;
                                         // calculate auxiliary variables q
            VectorZ q = wLast / w;
            // calculate t11, t12
            VectorZ t11 = 0.5 * (1.0 + q); // t22 = t11
            VectorZ t12 = 0.5 * (1.0 - q); // t21 = t12
                                           // update S-matrix
            VectorZ temp = t11 + t12 * S21;
            VectorZ ps = P * S22;
            S11 = S11 * P / temp;
            S21 = (t12 + t11 * S21) / temp;
            S12 = S12 - S11 * t12 * ps;
            S22 = (t11 - S21 * t12) * ps;
        }
        #endregion
        #region ---- loop ----

        /// <summary>
        /// Computes the half S-matrix for a multilayer coating using the vector kx approach.
        /// Iterates through all layers, updating the S11 and S21 parameters based on the eigenvalues and eigenvectors of each layer.
        /// </summary>
        /// <param name="wavelength">The working wavelength in vacuum.</param>
        /// <param name="nLayers">List of refractive indices for all layers (from incident to transmission medium).</param>
        /// <param name="tLayers">List of thicknesses for all layers (same order as <paramref name="nLayers"/>).</param>
        /// <param name="kx">Transverse (x) spatial frequency.</param>
        /// <param name="polarization">Polarization mode (TE or TM). Default is TE.</param>
        /// <returns>
        /// A tuple containing the computed S-matrix parameters:
        /// <list type="bullet">
        /// <item><description><c>S11</c>: Transmission coefficient from incident to transmission medium.</description></item>
        /// <item><description><c>S21</c>: Reflection coefficient from incident to transmission medium.</description></item>
        /// </list>
        /// </returns>
        internal static (VectorZ, VectorZ) HalfSMatrixLoop(double wavelength,
            List<Complex> nLayers,
            List<double> tLayers,
            VectorD kx,
            InPlanePolMode polarization = InPlanePolMode.TE)
        {
            double k0 = 2.0 * Math.PI / wavelength;
            // initialize
            VectorZ S11 = new(kx.Count, 1.0); // 1.0;
            VectorZ S21 = new(kx.Count, 0.0); // 0.0;

            // eigen info in transmission medium (n2)
            (VectorZ nzLast, VectorZ wLast) = Eigen.ComputeEigen(wavelength,
                nLayers[^1], kx, polarization, SignFactor.Positive, LoopMode.Vectorized);
            VectorZ gammaLast = VMath.Scale(nzLast, Complex.ImaginaryOne * k0);

            // S-matrix loop through all layers
            VectorZ gamma, w;
            for (int i = nLayers.Count - 2; i >= 0; i--)
            {
                // eigen info in current layer
                (VectorZ nz, w) = Eigen.ComputeEigen(wavelength,
                    nLayers[i], kx, polarization, SignFactor.Positive, LoopMode.Vectorized);
                gamma = VMath.Scale(nz, Complex.ImaginaryOne * k0);

                // call iteration kernel
                HalfSMatrixIterKernel(ref S11, ref S21,
                    gammaLast, wLast, w, tLayers[i + 1]);

                // update for the next round
                if (i != 0)
                {
                    gammaLast = gamma;
                    wLast = w;
                }
            }

            return (S11, S21);
        }

        /// <summary>
        /// Computes the full S-matrix for a multilayer coating using the vector kx approach.
        /// Iterates through all layers, updating the S11, S21, S12, and S22 parameters based on the eigenvalues and eigenvectors of each layer.
        /// </summary>
        /// <param name="wavelength">The working wavelength in vacuum.</param>
        /// <param name="nLayers">List of refractive indices for all layers (from incident to transmission medium).</param>
        /// <param name="tLayers">List of thicknesses for all layers (same order as <paramref name="nLayers"/>).</param>
        /// <param name="kx">Transverse (x) spatial frequency.</param>
        /// <param name="polarization">Polarization mode (TE or TM). Default is TE.</param>
        /// <returns>
        /// A tuple containing the computed S-matrix parameters:
        /// <list type="bullet">
        /// <item><description><c>S11</c>: Transmission coefficient from incident to transmission medium.</description></item>
        /// <item><description><c>S21</c>: Reflection coefficient from incident to transmission medium.</description></item>
        /// <item><description><c>S12</c>: Transmission coefficient from transmission to incident medium.</description></item>
        /// <item><description><c>S22</c>: Reflection coefficient from transmission to incident medium.</description></item>
        /// </list>
        /// </returns>
        internal static (VectorZ, VectorZ, VectorZ, VectorZ) FullSMatrixLoop(double wavelength,
            List<Complex> nLayers,
            List<double> tLayers,
            VectorD kx,
            InPlanePolMode polarization = InPlanePolMode.TE)
        {
            double k0 = 2.0 * Math.PI / wavelength;
            // initialize
            VectorZ S11 = new(kx.Count, 1.0); // 1.0;
            VectorZ S21 = new(kx.Count, 0.0); // 0.0;
            VectorZ S12 = new(kx.Count, 0.0); // 1.0;
            VectorZ S22 = new(kx.Count, 1.0); // 0.0;

            // eigen info in transmission medium (n2)
            (VectorZ nzLast, VectorZ wLast) = Eigen.ComputeEigen(wavelength,
                nLayers[^1], kx, polarization, SignFactor.Positive, LoopMode.Vectorized);
            VectorZ gammaLast = VMath.Scale(nzLast, Complex.ImaginaryOne * k0);

            // S-matrix loop through all layers
            VectorZ gamma, w;
            for (int i = nLayers.Count - 2; i >= 0; i--)
            {
                // eigen info in current layer
                (VectorZ nz, w) = Eigen.ComputeEigen(wavelength,
                    nLayers[i], kx, polarization, SignFactor.Positive, LoopMode.Vectorized);
                gamma = VMath.Scale(nz, Complex.ImaginaryOne * k0);

                // call iteration kernel
                FullSMatrixIterKernel(ref S11, ref S21, ref S12, ref S22,
                    gammaLast, wLast, w, tLayers[i + 1]);

                // update for the next round
                if (i != 0)
                {
                    gammaLast = gamma;
                    wLast = w;
                }
            }

            return (S11, S21, S12, S22);
        }

        #endregion

        #endregion

    }
}
