using VEMS.MathCore;
using Complex = System.Numerics.Complex;


namespace VEMS.EMSolver
{
    /// <summary>
    /// (multilayerd) coating matrix
    /// </summary>
    [Obsolete("Use the MLCoating class and methods there instead")]
    public class CoatingCalculator
    {

        #region ===== iteration kernel =====

        /// <summary>
        /// iteration kernel for half S-matrix calculation
        /// </summary>
        /// <param name="S11"> S11 (to be overwritten) </param>
        /// <param name="S21"> S21 (to be overwritten) </param>
        /// <param name="gammaLast"> eigenvalue of the last layer </param>
        /// <param name="wLast"> eigenvector parameter of the last layer </param>
        /// <param name="w"> eigenvector parameter of the current layer </param>
        /// <param name="tLast"> thickness of the last layer </param>
        private static void HalfSMatrixIterKernel(ref Complex S11, ref Complex S21,
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

        #endregion
        #region ===== iteration kernel (vectorized) =====

        /// <summary>
        /// iteration kernel for half S-matrix calculation
        /// vectorized version
        /// </summary>
        /// <param name="S11"> S11 (to be overwritten) </param>
        /// <param name="S21"> S21 (to be overwritten) </param>
        /// <param name="gammaLast"> eigenvalue of the last layer </param>
        /// <param name="wLast"> eigenvector parameter of the last layer </param>
        /// <param name="w"> eigenvector parameter of the current layer </param>
        /// <param name="tLast"> thickness of the last layer </param>
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

        #endregion
        #region ===== loop =====

        /// <summary>
        /// S matrix loop with full layer information
        /// </summary>
        /// <param name="wavelength"> working wavelength </param>
        /// <param name="nLayers"> refractive indices of all the layers </param>
        /// <param name="tLayers"> thicknesses of all the layers </param>
        /// <param name="kx"> transverse (x) spatial frequency </param>
        /// <param name="polarization"> TE or TM polarization option </param>
        /// <returns> (S11, S21) </returns>
        private static (Complex, Complex) HalfSMatrixLoop(double wavelength,
            List<Complex> nLayers,
            List<double> tLayers,
            double kx,
            InPlanePolMode polarization = InPlanePolMode.TE)
        {
            // initialize
            Complex s11 = 1.0;
            Complex s21 = 0.0;

            // eigen info in transmission medium (n2)
            (Complex nzLast, Complex wLast) = Eigen.ComputeEigen(wavelength, nLayers[^1], kx,
                polarization: polarization);
            Complex gammaLast = Complex.ImaginaryOne * 2.0 * Math.PI / wavelength * nzLast;

            // S-matrix loop through all layers
            Complex gamma, w;
            for (int i = nLayers.Count - 2; i >= 0; i--)
            {
                // eigen info in current layer
                (Complex nz, w) = Eigen.ComputeEigen(wavelength, nLayers[i], kx,
                    polarization: polarization);
                gamma = Complex.ImaginaryOne * 2.0 * Math.PI / wavelength * nz;

                // call iteration kernel
                HalfSMatrixIterKernel(ref s11, ref s21,
                    gammaLast, wLast, w, tLayers[i + 1]);

                // update for the next round
                if (i != 0)
                {
                    gammaLast = gamma;
                    wLast = w;
                }
            }

            return (s11, s21);
        }

        #endregion
        #region ===== loop (vectorized) =====

        /// <summary>
        /// S matrix loop with full layer information
        /// vectorized version
        /// </summary>
        /// <param name="wavelength"> working wavelength </param>
        /// <param name="nLayers"> refractive indices of all the layers </param>
        /// <param name="tLayers"> thicknesses of all the layers </param>
        /// <param name="kx"> transverse (x) spatial frequency </param>
        /// <param name="polarization"> TE or TM polarization option </param>
        /// <returns> (S11, S21) </returns>
        private static (VectorZ, VectorZ) HalfSMatrixLoop(double wavelength,
            List<Complex> nLayers,
            List<double> tLayers,
            VectorD kx,
            InPlanePolMode polarization = InPlanePolMode.TE)
        {
            // initialize
            VectorZ S11 = new(kx.Count, 1.0); // 1.0;
            VectorZ S21 = new(kx.Count, 0.0); // 0.0;

            // eigen info in transmission medium (n2)
            (VectorZ nzLast, VectorZ wLast) = Eigen.ComputeEigen(wavelength,
                nLayers[^1], kx, polarization, SignFactor.Positive, LoopMode.Vectorized);
            VectorZ gammaLast = VMath.Scale(nzLast, Complex.ImaginaryOne * 2.0 * Math.PI / wavelength);

            // S-matrix loop through all layers
            VectorZ gamma, w;
            for (int i = nLayers.Count - 2; i >= 0; i--)
            {
                // eigen info in current layer
                (VectorZ nz, w) = Eigen.ComputeEigen(wavelength,
                    nLayers[i], kx, polarization, SignFactor.Positive, LoopMode.Vectorized);
                gamma = VMath.Scale(nz, Complex.ImaginaryOne * 2.0 * Math.PI / wavelength);

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

        #endregion
        #region ===== coating matrix =====

        /// <summary>
        /// computes the complex transmission and reflection coefficients
        /// for a given multilayered coating structure
        /// </summary>
        /// <param name="wavelength"> working wavelength </param>
        /// <param name="n1"> refractive index in front </param>
        /// <param name="nLayers"> refractive indices of all the layers </param>
        /// <param name="tLayers"> thicknesses of all the layers </param>
        /// <param name="n2"> refractive index behind </param>
        /// <param name="kx"> transverse (x) spatial frequency </param>
        /// <param name="polarization"> TE or TM polarization option </param>
        /// <returns> (S11, S21) </returns>
        public static (Complex, Complex) ComputeCoatingMatrix(double wavelength,
            Complex n1, 
            List<Complex> nLayers,
            List<double> tLayers,
            Complex n2, double kx,
            InPlanePolMode polarization = InPlanePolMode.TE)
        {
            // handles the case without coating layers
            nLayers ??= new();
            tLayers ??= new();
            // includes medium behind
            nLayers.Add(n2);
            tLayers.Add(0.0);
            // includes medium in front
            nLayers.Insert(0, n1);
            tLayers.Insert(0, 0.0);

            // call half SMatrix loop
            return CoatingMatrix.HalfSMatrixLoop(wavelength, 
                nLayers, tLayers, kx, polarization);
        }

        #endregion
        #region ===== coating matrix (vectorized) =====

        /// <summary>
        /// computes the complex transmission and reflection coefficients
        /// </summary>
        /// <param name="wavelength"> working wavelength </param>
        /// <param name="n1"> refractive index in front </param>
        /// <param name="nLayers"> refractive indices of all the layers </param>
        /// <param name="tLayers"> thicknesses of all the layers </param>
        /// <param name="n2"> refractive index behind </param>
        /// <param name="kx"> transverse (x) spatial frequency </param>
        /// <param name="polarization"> TE or TM polarization option </param>
        /// <returns> (S11, S21) </returns>
        public static (VectorZ, VectorZ) ComputeCoatingMatrix(double wavelength,
            Complex n1, 
            List<Complex> nLayers,
            List<double> tLayers,
            Complex n2,
            VectorD kx,
            InPlanePolMode polarization = InPlanePolMode.TE)
        {
            // handles the case without coating layers
            nLayers ??= new();
            tLayers ??= new();
            // includes medium behind
            nLayers.Add(n2);
            tLayers.Add(0.0);
            // includes medium in front
            nLayers.Insert(0, n1);
            tLayers.Insert(0, 0.0);

            // call half SMatrix loop
            return CoatingMatrix.HalfSMatrixLoop(wavelength, nLayers, tLayers, kx, polarization);
        }

        #endregion

        #region ==== helper ====

        /// <summary>
        /// computes transmittance and reflectance for a coating
        /// i.e. the ratio between output and input power densities
        /// </summary>
        /// <param name="wavelength"> wavelength in vacuum </param>
        /// <param name="n1"> refractive index in front </param>
        /// <param name="nLayers"> refractive indices of layers in the middle </param>
        /// <param name="tLayers"> thicknesses of layers in the middle </param>
        /// <param name="n2"> refractive index behind </param>
        /// <param name="kx"> transverse spatial frequency </param>
        /// <param name="mode"> polarization mode: TE or TM </param>
        /// <returns> (transmittance T, reflectance R) </returns>
        public static (double, double) ComputeCoatingRatio(double wavelength,
            Complex n1,
            List<Complex> nLayers,
            List<double> tLayers,
            Complex n2,
            double kx,
            InPlanePolMode mode = InPlanePolMode.TE)
        {
            (Complex s11, Complex s21) = ComputeCoatingMatrix(wavelength,
                n1, nLayers, tLayers, n2, kx, mode);

            // constructs plane waves
            PlaneWaveXZ incPW = new(wavelength, n1 * n1, kx, polMode: mode);
            PlaneWaveXZ traPW = new(wavelength, n2 * n2, kx, polMode: mode);
            PlaneWaveXZ refPW = new(wavelength, n1 * n1, kx, direction: SignFactor.Negative, polMode: mode);

            // sets field coefficients for plane waves 
            incPW.E = 1.0; // set input plane wave amplitude to 1.0
            double szI = incPW.ComputeSz();
            traPW.E = s11 * incPW.E;
            refPW.E = s21 * incPW.E;
            // reflectance & transmittance
            double T = traPW.ComputeSz() / szI;
            double R = -refPW.ComputeSz() / szI;
            return (T, R);
        }

        public static void TransformSMatrixFromTETM2XY(double kx, double ky,
            Complex sTE, Complex sTM,
            out Complex sXX, out Complex sXY, out Complex sYY)
        {
            // Kx and Ky (in the transformed coordinate X-Y)
            var kx2 = kx * kx;
            var ky2 = ky * ky;
            var kappa2 = kx2 + ky2;

            if (kappa2 != 0)
            {
                sXX = (kx2 * sTM + ky2 * sTE) / kappa2;
                sXY = kx * ky * (sTM - sTE) / kappa2;
                sYY = (ky2 * sTM + kx2 * sTE) / kappa2;
            }
            else
            {
                sXX = sTM;
                sXY = 0.0;
                sYY = sTE;
            }
        }

        #endregion

    }

}
