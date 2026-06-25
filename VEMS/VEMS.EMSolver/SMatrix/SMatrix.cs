using System.Diagnostics;
using VEMS.MathCore;
using Complex = System.Numerics.Complex;

namespace VEMS.EMSolver
{

    /// <summary>
    /// the S-matrix class
    /// </summary>
    public class SMatrix
    {

        #region --------- Type Definition ---------

        /// <summary>
        /// variation of the S-matrix formulation
        /// </summary>
        public enum AlgorithmVariation
        {
            /// <summary>
            /// W => t => S
            /// </summary>
            WtS = 0,

            /// <summary>
            /// W => S
            /// </summary>
            WS = 1,
        }

        /// <summary>
        /// work mode of the S-matrix algorithm
        /// </summary>
        public enum UsageOption
        {
            /// <summary>
            /// saves only those necessary modes for the calculation
            /// </summary>
            SaveNecessaryModes = 0,

            /// <summary>
            /// saves all the modes during the calculation
            /// </summary>
            SaceAllModes = 1,
        }

        #endregion

        #region properties

        /// <summary>
        /// variation of the formulation
        /// </summary>
        public AlgorithmVariation Variation { get; set; }


        /// <summary>
        /// work mode of the algorithm
        /// </summary>
        public UsageOption Usage { get; set; }


        public VectorD KX { get; set; }

        public VectorD Kx { get; set; }

        public VectorD Ky { get; set; }

        public List<FourierModes.HomogeneousIsotropic> AllModes { get; set; }

        public List<double> Thicknesses { get; set; }

        #endregion
        #region constructor

        public SMatrix() { }

        public SMatrix(VectorD kX,
            FourierModes.HomogeneousIsotropic leftMode,
            List<FourierModes.HomogeneousIsotropic> layerModes,
            List<double> thicknesses,
            FourierModes.HomogeneousIsotropic rightMode,
            AlgorithmVariation variation = AlgorithmVariation.WtS,
            UsageOption usage = UsageOption.SaveNecessaryModes)
        {
            this.KX = kX;
            //this.AllModes = allModes;
            this.Thicknesses = thicknesses;
            this.Variation = variation;
            this.Usage = usage;
        }

        #endregion
        #region methods

        // half S-matrix iteration kernel
        // single kx value
        // with w1/2 symmetry and w1 = 1.0
        // e.g. for homogeneous layers
        public void HalfSMatrixIterKernel(ref Complex S11, ref Complex S21,
            Complex gammaLast, Complex w2Last,
            Complex w2, double tLast)
        {
            // calculate P-term
            Complex P11 = Complex.Exp(gammaLast * tLast); // invP22 = P11
            // auxiliary variable \Omega ==> save it to S21; the same for W=>S and W=>t=>S
            S21 = P11 * S21 * P11;
            // calculate auxiliary variables Q1 and Q2
            Complex q2 = w2Last / w2; // q1 = 1.0
            // S-matrix variation
            switch (Variation)
            {
                case AlgorithmVariation.WtS:
                    {
                        // calculate t11, t12
                        Complex t11 = 0.5 * (1.0 + q2); // t22 = t11
                        Complex t12 = 0.5 * (1.0 - q2); // t21 = t12
                        // update S-matrix
                        Complex temp = t11 + t12 * S21;
                        S11 = S11 * P11 / temp;
                        S21 = (t12 + t11 * S21) / temp;
                    }
                    break;
                case AlgorithmVariation.WS:
                    {
                        // calculate auxiliary variables F, G, tau
                        Complex f = 1.0 + S21;
                        Complex g = q2 * (1.0 - S21);
                        Complex tau = 1.0 / (f + g);
                        // update S-matrix
                        S11 = 2.0 * S11 * P11 * tau;
                        S21 = 1.0 - 2.0 * g * tau;
                    }
                    break;
            }
        }

        //public void HalfSMatrixIterKernel(ref ComplexVector S11, ref ComplexVector S21,
        //    ComplexVector gammaLast, ComplexVector w2Last,
        //    ComplexVector gamma, ComplexVector w2, double t)
        //{
        //    // calculate P-term
        //    ComplexVector P11 = new(gammaLast.Length, 1.0);
        //    if (t != 0.0) { P11 = VMath.Exp(gammaLast * t); }
        //    // auxiliary variable \Omega ==> save it to S21; the same for W=>S and W=>t=>S
        //    S21 = P11 * S21 * P11;
        //    // calculate auxiliary variables Q1 and Q2
        //    ComplexVector q2 = w2Last / w2;
        //    // S matrix variation
        //    switch (Variation)
        //    {
        //        case AlgorithmVariation.WtS:
        //            {
        //                // calculate t11, t12
        //                ComplexVector t11 = 0.5 * (new ComplexVector(q2.Length, 1.0) + q2);
        //                ComplexVector t12 = 0.5 * (new ComplexVector(q2.Length, 1.0) - q2);
        //                // update S matrix
        //                ComplexVector temp = t11 + t12 * S21;
        //                S11 = S11 * P11 / temp;
        //                S21 = (t12 + t11 * S21) / temp;
        //            }
        //            break;
        //        case AlgorithmVariation.WS:
        //            {

        //            }
        //            break;
        //    }

        //}



        public void CalculateHalfSMatrix(double kX,
            List<FourierModes.HomogeneousIsotropic> allModes,
            List<double> thicknesses,
            out Complex S11, out Complex S21,
            bool TEmode)
        {
            // initialization
            S11 = 1.0;
            S21 = 0.0;
            // eigen mode for the layer behind
            allModes[allModes.Count - 1].CalculateEigenMode(kX,
                out Complex gammaLast,
                out Complex w1Last, out Complex w2Last,
                TEmode);

            // S-matrix loop starts ...
            Complex gamma, w1, w2;
            for (int i = allModes.Count - 2; i >= 0; i--)
            {
                // prepare Fourier mode for the current layer
                allModes[i].CalculateEigenMode(kX,
                    out gamma,
                    out w1,
                    out w2,
                    TEmode);
                // get thickness of the last layer
                var thickness = thicknesses[i + 1];
                // calcualte P-term
                var P11 = Complex.Exp(gammaLast * thickness); // invP22 = P11
                // auxiliary variable \Omega ==> save it to S21; the same for W=>S and W=>t=>S
                S21 = P11 * S21 * P11;
                // S-matrix variation
                switch (this.Variation)
                {
                    case AlgorithmVariation.WtS:
                        {
                            // calculate auxiliary variables Q1 and Q2
                            var q2 = w2Last / w2; // q1TE = 1.0
                            // calculate auxiliary variables F, G, tau
                            var f = 1 + S21;
                            var g = q2 * (1 - S21);
                            var tau = 1.0 / (f + g);
                            // update S-matrix
                            S11 = 2.0 * S11 * P11 * tau;
                            S21 = 1.0 - 2.0 * g * tau;
                        }
                        break;
                    case AlgorithmVariation.WS:
                        {
                            // calculate auxiliary variables Q1 and Q2
                            var q2 = w2Last / w2; // q1TE = 1.0
                            // calculate t11, t12
                            var t11 = 0.5 * (1.0 + q2); // t22 = t11
                            var t12 = 0.5 * (1.0 - q2); // t21 = t12
                            // update S-matrix
                            var temp = t11 + t12 * S21;
                            S11 = S11 * P11 / temp;
                            S21 = (t12 + t11 * S21) / temp;
                        }
                        break;
                }
                // update for the next round
                if (i != 0)
                {
                    gammaLast = gamma;
                    w1Last = w1;
                    w2Last = w2;
                }
            }
        }


        public void CalculateHalfSMatrix(double kX,
            List<FourierModes.HomogeneousIsotropic> allModes,
            List<double> thicknesses,
            out Complex S11TE, out Complex S21TE,
            out Complex S11TM, out Complex S21TM)
        {
            // initialization
            S11TE = 1.0; S21TE = 0.0;
            S11TM = 1.0; S21TM = 0.0;
            // eigen mode for the layer behind (with zero thickness)
            allModes[allModes.Count - 1].CalculateEigenMode(kX, out Complex gammaLast,
                out Complex w1TELast, out Complex w2TELast,
                out Complex w1TMLast, out Complex w2TMLast);

            // S-matrix loop starts ...
            Complex gamma;
            Complex w1TE, w2TE, w1TM, w2TM;
            for (int i = allModes.Count - 2; i >= 0; i--)
            {
                // prepare Fourier mode for the current layer
                allModes[i].CalculateEigenMode(kX, out gamma,
                    out w1TE, out w2TE,
                    out w1TM, out w2TM);
                // get thickness of the last layer
                var thickness = thicknesses[i + 1];
                // calculate P-term
                var P11 = Complex.Exp(gammaLast * thickness); // invP22 = P11
                // auxiliary variable \Omega ==> save it to S21; the same for W=>S and W=>t=>S
                S21TE = P11 * S21TE * P11;
                S21TM = P11 * S21TM * P11;

                // S-matrix variations
                switch (this.Variation)
                {
                    case AlgorithmVariation.WtS:
                        {
                            // calculate auxiliary variables Q1 and Q2
                            var q2TE = w2TELast / w2TE; // q1TE = 1.0
                            var q2TM = w2TMLast / w2TM; // q1TM = 1.0
                            // calculate t11, t12
                            var t11TE = 0.5 * (1.0 + q2TE); // t22 = t11
                            var t12TE = 0.5 * (1.0 - q2TE); // t21 = t12
                            var t11TM = 0.5 * (1.0 + q2TM); // t22 = t11
                            var t12TM = 0.5 * (1.0 - q2TM); // t21 = t12
                            // update S-matrix
                            var tempTE = t11TE + t12TE * S21TE;
                            var tempTM = t11TM + t12TM * S21TM;
                            S11TE = S11TE * P11 / tempTE;
                            S21TE = (t12TE + t11TE * S21TE) / tempTE;
                            S11TM = S11TM * P11 / tempTM;
                            S21TM = (t12TM + t11TM * S21TM) / tempTM;
                        }
                        break;
                    case AlgorithmVariation.WS:
                        {
                            // calculate auxiliary variables Q1 and Q2
                            var q2TE = w2TELast / w2TE; // q1TE = 1.0
                            var q2TM = w2TMLast / w2TM; // q1TM = 1.0
                            // calculate auxiliary variables F, G, tau
                            var fTE = 1 + S21TE;
                            var gTE = q2TE * (1 - S21TE);
                            var tauTE = 1.0 / (fTE + gTE);
                            var fTM = 1 + S21TM;
                            var gTM = q2TM * (1 - S21TM);
                            var tauTM = 1.0 / (fTM + gTM);
                            // update S-matrix
                            S11TE = 2.0 * S11TE * P11 * tauTE;
                            S21TE = 1.0 - 2.0 * gTE * tauTE;
                            S11TM = 2.0 * S11TM * P11 * tauTM;
                            S21TM = 1.0 - 2.0 * gTM * tauTM;
                        }
                        break;
                }
                // update for the next round
                if (i != 0)
                {
                    gammaLast = gamma;
                    w1TELast = w1TE; w2TELast = w2TE;
                    w1TMLast = w1TM; w2TMLast = w2TM;
                }

            }

        }


        public void CalculateHalfSMatrix(double kX,
            FourierModes.HomogeneousIsotropic leftMode,
            List<FourierModes.HomogeneousIsotropic> layerModes,
            List<double> thicknesses,
            FourierModes.HomogeneousIsotropic rightMode,
            out Complex S11, out Complex S21,
            bool TEmode)
        {
            // initialization
            S11 = 1.0;
            S21 = 0.0;
            // left and right modes
            leftMode.CalculateEigenMode(kX, out Complex gammaLeft,
                out Complex w1Left, out Complex w2Left, TEmode);
            rightMode.CalculateEigenMode(kX, out Complex gammaRight,
                out Complex w1Right, out Complex w2Right, TEmode);
            var gammaLast = gammaRight;
            var w1Last = w1Right;
            var w2Last = w2Right;

            // S-matrix loop
            Complex gamma, w1, w2;
            var layerCount = 0;
            if (layerModes != null) { layerCount = layerModes.Count; }
            for (int i = layerCount - 1; i >= -1; i--)
            {
                // prepare Fourier mode for the current layer
                if (i == -1)
                {
                    gamma = gammaLeft;
                    w1 = w1Left;
                    w2 = w2Left;
                }
                else
                {
                    layerModes[i].CalculateEigenMode(kX, out gamma,
                        out w1, out w2, TEmode);
                }
                // get thickness of the last layer
                var thickness = 0.0;
                if (i != layerCount - 1) { thickness = thicknesses[i + 1]; }
                // calculate P-term
                var P11 = Complex.Exp(gammaLast * thickness); // invP22 = P11
                // auxiliary variable \Omega ==> save it to S21; the same for W=>S and W=>t=>S
                S21 = P11 * S21 * P11;
                // S-matrix variations
                switch (this.Variation)
                {
                    case AlgorithmVariation.WtS:
                        {
                            // calculate auxiliary variables Q1 and Q2
                            var q2 = w2Last / w2; // q1TE = 1.0
                            // calculate auxiliary variables F, G, tau
                            var f = 1 + S21;
                            var g = q2 * (1 - S21);
                            var tau = 1.0 / (f + g);
                            // update S-matrix
                            S11 = 2.0 * S11 * P11 * tau;
                            S21 = 1.0 - 2.0 * g * tau;
                        }
                        break;
                    case AlgorithmVariation.WS:
                        {
                            // calculate auxiliary variables Q1 and Q2
                            var q2 = w2Last / w2; // q1TE = 1.0
                            // calculate t11, t12
                            var t11 = 0.5 * (1.0 + q2); // t22 = t11
                            var t12 = 0.5 * (1.0 - q2); // t21 = t12
                            // update S-matrix
                            var temp = t11 + t12 * S21;
                            S11 = S11 * P11 / temp;
                            S21 = (t12 + t11 * S21) / temp;
                        }
                        break;
                }
                // update for the next round
                if (i != -1)
                {
                    gammaLast = gamma;
                    w1Last = w1;
                    w2Last = w2;
                }
            }
        }


        // convert S matrix result from TE-TM to the x-y coordinate system
        public void TransformSMatrixFromTETM2XY(double kx, double ky,
            Complex S11TE, Complex S21TE,
            Complex S11TM, Complex S21TM,
            out MatrixZ S11, out MatrixZ S21)
        {
            // initialization
            S11 = new MatrixZ(2, 2, 1.0);
            S21 = new MatrixZ(2, 2, 0.0);
            // Kx and Ky (in the transformed coordinate X-Y)
            var kx2 = kx * kx;
            var ky2 = ky * ky;
            var kappa2 = kx2 + ky2;
            var kX = Math.Sqrt(kappa2); //double kY = 0;
            if (kappa2 != 0)
            {
                S11[0, 0] = (kx2 * S11TM + ky2 * S11TE) / kappa2;
                S11[0, 1] = kx * ky * (S11TM - S11TE) / kappa2;
                S11[1, 0] = S11[0, 1];
                S11[1, 1] = (ky2 * S11TM + kx2 * S11TE) / kappa2;
                S21[0, 0] = (kx2 * S21TM + ky2 * S21TE) / kappa2;
                S21[0, 1] = kx * ky * (S21TM - S21TE) / kappa2;
                S21[1, 0] = S21[0, 1];
                S21[1, 1] = (ky2 * S21TM + kx2 * S21TE) / kappa2;
            }
            else
            {
                S11[0, 0] = S11TM;
                S11[1, 1] = S11TE;
                S21[0, 0] = S21TM;
                S21[1, 1] = S21TE;
            }
        }


        public void TransformSMatrixFromTETM2XY(double kx, double ky,
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

        #region static methods

        #region ===== kernels =====

        /// <summary>
        /// half S-matrix common computation kernel
        /// for both the W=>t=>S and W=>S variations
        /// with symmetry properties
        /// </summary>
        /// <param name="s11"> s11 sub-matrix </param>
        /// <param name="s21"> s21 sub-matrix </param>
        /// <param name="q1"> auxiliary variable in the computation </param>
        /// <param name="q2"> auxiliary variable in the computation </param>
        /// <param name="p11"> propagation constant term </param>
        /// <param name="useWSvariation"> false (default): W=>t=>S, true: W=>S </param>
        private static void HalfSMatrixKernel(ref MatrixZ s11, ref MatrixZ s21,
            MatrixZ q1, MatrixZ q2, VectorZ p11,
            bool useWSvariation = false)
        {
            if (useWSvariation)
            {
                // S-matrix using W-S variation
                MatrixZ s21p = new(s21, true);
                LinAlg.DiagonalMatrixHelper.AddTo(new VectorD(s21p.Cols, 1.0), ref s21p);
                MatrixZ s21n = new(s21, true);
                VMath.ScaleOn(ref s21n, -1.0);
                LinAlg.DiagonalMatrixHelper.AddTo(new VectorD(s21n.Cols, 1.0), ref s21n);
                MatrixZ f = LinAlg.Dot(q1, s21p);
                MatrixZ g = LinAlg.Dot(q2, s21n);
                MatrixZ tau = f + g;
                LinAlg.Inverse(ref tau);
                // update S-matrix
                s21 = LinAlg.Dot(g, tau);
                VMath.ScaleOn(ref s21, -2.0);
                LinAlg.DiagonalMatrixHelper.AddTo(new VectorD(s21.Cols, 1.0), ref s21);
                LinAlg.DiagonalMatrixHelper.Dot(ref s11, p11);
                s11 = 2.0 * LinAlg.Dot(s11, tau);
            }
            else
            {
                // S-matrix using W=>t=>S variation
                MatrixZ t11 = q1 + q2;
                VMath.ScaleOn(ref t11, 0.5);
                MatrixZ t12 = q1 - q2;
                VMath.ScaleOn(ref t12, 0.5);
                MatrixZ t1 = new(t11, true);
                LinAlg.Dot(t12, s21, ref t1, 1.0, 0.0, 1.0, 0.0);
                LinAlg.Inverse(ref t1);
                // update S-matrix
                MatrixZ t2 = new(t12, true);
                LinAlg.Dot(t11, s21, ref t2, 1.0, 0.0, 1.0, 0.0);
                s21 = LinAlg.Dot(t2, t1);
                LinAlg.DiagonalMatrixHelper.Dot(ref s11, p11);
                s11 = LinAlg.Dot(s11, t1);
            }
        }

        /// <summary>
        /// full S-matrix common computation kernel
        /// for both the W=>t=>S and W=>S variations
        /// with symmetry properties
        /// </summary>
        /// <param name="s11">s11 sub-matrix </param>
        /// <param name="s21">s21 sub-matrix </param>
        /// <param name="s12">s12 sub-matrix </param>
        /// <param name="s22">s22 sub-matrix </param>
        /// <param name="q1"> auxiliary variable in the computation </param>
        /// <param name="q2"> auxiliary variable in the computation </param>
        /// <param name="p11"> propagation constant term </param>
        /// <param name="useWSvariation"> false (default): W=>t=>S, true: W=>S </param>
        private static void FullSMatrixKernel(ref MatrixZ s11, ref MatrixZ s21,
            ref MatrixZ s12, ref MatrixZ s22,
            MatrixZ q1, MatrixZ q2, VectorZ p11,
            bool useWSvariation = false)
        {
            if (useWSvariation)
            {
                // S-matrix using W-S variation
                MatrixZ s21p = new(s21, true);
                LinAlg.DiagonalMatrixHelper.AddTo(new VectorD(s21p.Cols, 1.0), ref s21p);
                MatrixZ s21n = new(s21, true);
                VMath.ScaleOn(ref s21n, -1.0);
                LinAlg.DiagonalMatrixHelper.AddTo(new VectorD(s21n.Cols, 1.0), ref s21n);
                MatrixZ f = LinAlg.Dot(q1, s21p);
                MatrixZ g = LinAlg.Dot(q2, s21n);
                MatrixZ tau = f + g;
                LinAlg.Inverse(ref tau);
                MatrixZ s11p = new(s11, true);
                LinAlg.DiagonalMatrixHelper.Dot(ref s11p, p11);
                MatrixZ s22p = new(s22, true);
                LinAlg.DiagonalMatrixHelper.Dot(p11, ref s22p);
                MatrixZ qq = q2 - q1;
                MatrixZ qs = LinAlg.Dot(qq, s22p);
                MatrixZ tqs = LinAlg.Dot(tau, qs);
                MatrixZ gt = LinAlg.Dot(g, tau);
                MatrixZ gtq = LinAlg.Dot(gt, q1);
                MatrixZ ft = LinAlg.Dot(f, tau);
                LinAlg.Dot(ft, q2, ref gtq, 1.0, 0.0, 1.0, 0.0);
                // update S-matrix
                s21 = LinAlg.Dot(g, tau);
                VMath.ScaleOn(ref s21, -2.0);
                LinAlg.DiagonalMatrixHelper.AddTo(new VectorD(s21.Cols, 1.0), ref s21);
                s11 = 2.0 * LinAlg.Dot(s11p, tau);
                LinAlg.Dot(s11p, tqs, ref s12, 1.0, 0.0, 1.0, 0.0);
                s22 = LinAlg.Dot(gtq, s22p);

            }
            else
            {
                // S-matrix using W=>t=>S variation
                MatrixZ t11 = q1 + q2; //t11 = t22
                VMath.ScaleOn(ref t11, 0.5);
                MatrixZ t12 = q1 - q2; //t12 = t21
                VMath.ScaleOn(ref t12, 0.5);
                MatrixZ t1 = new(t11, true);
                MatrixZ t3 = new(t11, true);
                MatrixZ t2 = new(t12, true);
                MatrixZ t4 = new(t12, true);
                LinAlg.Dot(t12, s21, ref t1, 1.0, 0.0, 1.0, 0.0);
                LinAlg.Inverse(ref t1);
                MatrixZ s22p = LinAlg.DiagonalMatrixHelper.Dot(p11, s22);
                MatrixZ tp = LinAlg.Dot(t4, s22p);
                // update S-matrix
                LinAlg.Dot(t11, s21, ref t2, 1.0, 0.0, 1.0, 0.0);
                s21 = LinAlg.Dot(t2, t1);
                LinAlg.DiagonalMatrixHelper.Dot(ref s11, p11);
                s11 = LinAlg.Dot(s11, t1);
                MatrixZ s1 = new(s11, true);
                LinAlg.Dot(s1, tp, ref s12, -1.0, 0.0, 1.0, 0.0);
                MatrixZ s2 = new(s21, true);
                LinAlg.Dot(s2, t12, ref t3, -1.0, 0.0, 1.0, 0.0);
                s22 = LinAlg.Dot(t3, s22p);
            }
        }
        #endregion
        #region ===== Half boundaries =====

        /// <summary>
        /// half S-matrix boundary
        /// uniform | uniform [backward]
        /// </summary>
        /// <param name="s11"> s11 sub-matrix </param>
        /// <param name="s21"> s21 sub-matrix </param>
        /// <param name="gammaLast"> gamma [positive] of the last uniform layer </param>
        /// <param name="w1Last"> w1 of the last uniform layer, diagonal => vector storage = 1.0</param>
        /// <param name="w2Last"> w2 of the uniform layer, diagonal => vector storage </param>
        /// <param name="w1"> w1 of the current uniform layer, diagonal => vector storage = 1.0 </param>
        /// <param name="w2"> w2 of the current uniform layer, diagonal => vector storage </param>
        /// <param name="tLast"> thickness of the last uniform layer </param>
        /// <param name="useWSvariation"> false (default): W=>t=>S, true: W=>S </param>
        private static void HalfSMatrixBoundary(ref MatrixZ s11, ref MatrixZ s21,
            VectorZ gammaLast, VectorZ w1Last, VectorZ w2Last,
            VectorZ w1, VectorZ w2, double tLast,
            bool useWSvariation = false)
        {
            // calculate P-term
            VectorZ p11 = VMath.Exp(gammaLast * tLast); // invP22 = p11
            // auxiliary variable \Omega ==> save it to S21; the same for W=>S and W=>t=>S
            LinAlg.DiagonalMatrixHelper.Dot(ref s21, p11);
            LinAlg.DiagonalMatrixHelper.Dot(p11, ref s21);
            // calculate auxiliary variables Q1 and Q2; the same for W=>S and W=>t=>S
            MatrixZ q1 = LinAlg.DiagonalMatrixHelper.GenerateDenseMatrixZ(1.0 / w1);
            MatrixZ q2 = LinAlg.DiagonalMatrixHelper.GenerateDenseMatrixZ(1.0 / w2);
            //MatrixZ q1 = new(w1, true); // because w1Last = 1.0 
            //LinAlg.Inverse(ref q1);
            //MatrixZ q2 = new(w2, true);
            //LinAlg.Inverse(ref q2);
            LinAlg.DiagonalMatrixHelper.Dot(ref q2, w2Last);
            // kernel call
            HalfSMatrixKernel(ref s11, ref s21, q1, q2, p11, useWSvariation);
        }


        /// <summary>
        /// half S-matrix boundary
        /// 1D-periodic | uniform [backward]
        /// using the W=>t=>S or W=>S variation with W1/2 symmetry
        /// </summary>
        /// <param name="s11"> s11 sub-matrix </param>
        /// <param name="s21"> s21 sub-matrix </param>
        /// <param name="gammaLast"> gamma [positive] of the uniform layer </param>
        /// <param name="w1Last"> w1 of the uniform layer, diagonal => vector storage = 1.0 </param>
        /// <param name="w2Last"> w2 of the uniform layer, diagonal => vector storage </param>
        /// <param name="w1"> w1 of the periodic layer </param>
        /// <param name="w2"> w2 of the periodic later </param>
        /// <param name="tLast"> thickness of the uniform layer </param>
        /// <param name="useWSvariation"> false (default): W=>t=>S, true: W=>S </param>
        private static void HalfSMatrixBoundary(ref MatrixZ s11, ref MatrixZ s21,
            VectorZ gammaLast, VectorZ w1Last, VectorZ w2Last,
            MatrixZ w1, MatrixZ w2, double tLast,
            bool useWSvariation = false)
        {
            // calculate P-term
            VectorZ p11 = VMath.Exp(gammaLast * tLast); // invP22 = p11
            // auxiliary variable \Omega ==> save it to S21; the same for W=>S and W=>t=>S
            LinAlg.DiagonalMatrixHelper.Dot(ref s21, p11);
            LinAlg.DiagonalMatrixHelper.Dot(p11, ref s21);
            // calculate auxiliary variables Q1 and Q2; the same for W=>S and W=>t=>S
            MatrixZ q1 = new(w1, true); // because w1Last = 1.0 
            LinAlg.Inverse(ref q1);
            MatrixZ q2 = new(w2, true); 
            LinAlg.Inverse(ref q2);
            LinAlg.DiagonalMatrixHelper.Dot(ref q2, w2Last);
            // kernel call
            HalfSMatrixKernel(ref s11, ref s21, q1, q2, p11, useWSvariation);
        }

        /// <summary>
        /// half S-matrix boundary
        /// uniform | 1D-periodic [backward]
        /// using the W=>t=>S formulation with W1/2 symmetry
        /// </summary>
        /// <param name="s11"> s11 sub-matrix </param>
        /// <param name="s21"> s21 sub-matrix </param>
        /// <param name="gammaLast"> gamma [positive] of the periodic layer </param>
        /// <param name="w1Last"> w1 of the periodic layer </param>
        /// <param name="w2Last"> w2 of the periodic layer </param>
        /// <param name="w1"> w1 of the uniform layer, diagonal => vector storage =  1.0 </param>
        /// <param name="w2"> w2 of the uniform layer, diagonal => vector storage </param>
        /// <param name="tLast"> thickness of the periodic layer </param>
        /// <param name="useWSvariation"> false (default): W=>t=>S, true: W=>S </param>
        private static void HalfSMatrixBoundary(ref MatrixZ s11, ref MatrixZ s21,
            VectorZ gammaLast, MatrixZ w1Last, MatrixZ w2Last,
            VectorZ w1, VectorZ w2, double tLast,
            bool useWSvariation = false)
        {
            // calculate P-term
            VectorZ p11 = VMath.Exp(gammaLast * tLast); // invP22 = p11
            // auxiliary variable \Omega ==> save it to S21; the same for W=>S and W=>t=>S
            LinAlg.DiagonalMatrixHelper.Dot(ref s21, p11);
            LinAlg.DiagonalMatrixHelper.Dot(p11, ref s21);
            // calculate auxiliary variables Q1 and Q2; the same for W=>S and W=>t=>S
            MatrixZ q1 = w1Last; // because w1 = 1.0; 
            MatrixZ q2 = w2Last;
            LinAlg.DiagonalMatrixHelper.Dot(1.0 / w2, ref q2);
            // kernel call
            HalfSMatrixKernel(ref s11, ref s21, q1, q2, p11, useWSvariation);
        }

        /// <summary>
        /// half S-matrix boundary
        /// periodic | periodic [backward]
        /// using the W=>S formulation with W1/2 symmetry
        /// </summary>
        /// <param name="s11"> s11 sub-matrix </param>
        /// <param name="s21"> s21 sub-matrix </param>
        /// <param name="gammaLast"> gamma of the last layer </param>
        /// <param name="w1Last"> w1 of the last layer </param>
        /// <param name="w2Last"> w2 of the last layer </param>
        /// <param name="w1"> w1 of the current layer </param>
        /// <param name="w2"> w2 of the current layer </param>
        /// <param name="tLast"> thickness of the last layer </param>
        /// <param name="useWSvariation"> false (default): W=>t=>S, true: W=>S </param>
        private static void HalfSMatrixBoundary(ref MatrixZ s11, ref MatrixZ s21,
            VectorZ gammaLast, MatrixZ w1Last, MatrixZ w2Last,
            MatrixZ w1, MatrixZ w2, double tLast,
            bool useWSvariation = false)
        {
            // calculate P-term;
            VectorZ p11 = VMath.Exp(gammaLast * tLast); // invP22 = p11
            // auxiliary variable \Omega ==> save it to S21; the same for W=>S and W=>t=>S
            LinAlg.DiagonalMatrixHelper.Dot(ref s21, p11);
            LinAlg.DiagonalMatrixHelper.Dot(p11, ref s21);
            // calculate auxiliary variables Q1 and Q2; the same for W=>S and W=>t=>S
            MatrixZ q1 = LinAlg.LinearSolve(w1, w1Last);
            MatrixZ q2 = LinAlg.LinearSolve(w2, w2Last);

            //Printer.Write("q2 = ", VMath.Abs(q2), 1);

            // kernel call
            HalfSMatrixKernel(ref s11, ref s21, q1, q2, p11, useWSvariation);
        }

        // !!! SPARSE ToDo !!!
        // 2D-periodic | uniform [backward]
        private static void HalfSMatrixBoundary(ref MatrixZ s11, ref MatrixZ s21,
            VectorZ gammaLast, VectorZ w111Last, VectorZ w122Last,
            VectorZ w211Last, VectorZ w212Last, VectorZ w221Last, VectorZ w222Last,
            MatrixZ w1, MatrixZ w2, double tLast,
            bool useWSvariation = false)
        {
            // calculate P-term
            VectorZ p11 = VMath.Exp(gammaLast * tLast); // invP22 = p11
            // auxiliary variable \Omega ==> save it to S21; the same for W=>S and W=>t=>S
            LinAlg.DiagonalMatrixHelper.Dot(ref s21, p11);
            LinAlg.DiagonalMatrixHelper.Dot(p11, ref s21);
            // calculate auxiliary variables Q1 and Q2; the same for W=>S and W=>t=>S
            MatrixZ q1 = new(w1, true); // because w1Last = 1.0 
            LinAlg.Inverse(ref q1);
            MatrixZ q2 = new(w2, true);
            LinAlg.Inverse(ref q2);
            // Q2 = [w2]^-1 [w2Last]
            long n = q2.Cols/2;
            LongRange up = new(0, n);
            LongRange down = new(n, 2 * n);
            LongRange left = new(0, n);
            LongRange right = new(n, 2 * n);
            MatrixZ? q211 = q2[up, left];
            MatrixZ? q212 = q2[up, right];
            MatrixZ? q221 = q2[down, left];
            MatrixZ? q222 = q2[down, right];

            if(q211 != null && q212 != null && q221 != null && q222 != null)
            {
                q2[up, left] = LinAlg.DiagonalMatrixHelper.Dot(q211, w211Last)
                    + LinAlg.DiagonalMatrixHelper.Dot(q212, w221Last);
                q2[up, right] = LinAlg.DiagonalMatrixHelper.Dot(q211, w212Last)
                    + LinAlg.DiagonalMatrixHelper.Dot(q212, w222Last);
                q2[down, left] = LinAlg.DiagonalMatrixHelper.Dot(q221, w211Last)
                    + LinAlg.DiagonalMatrixHelper.Dot(q222, w221Last);
                q2[down, right] = LinAlg.DiagonalMatrixHelper.Dot(q221, w212Last)
                    + LinAlg.DiagonalMatrixHelper.Dot(q222, w222Last);
            }

            //Printer.Write("q2 = ", VMath.Abs(q2), 1);

            // kernel call
            HalfSMatrixKernel(ref s11, ref s21, q1, q2, p11, useWSvariation);
        }

        // !!! SPARSE ToDo !!!
        // uniform | 2D-periodic [backward]
        private static void HalfSMatrixBoundary(ref MatrixZ s11, ref MatrixZ s21,
            VectorZ gammaLast, MatrixZ w1Last, MatrixZ w2Last,
            VectorZ w111, VectorZ w122,
            VectorZ w211, VectorZ w212, VectorZ w221, VectorZ w222, double tLast,
            bool useWSvariation = false)
        {
            // calculate P-term
            VectorZ p11 = VMath.Exp(gammaLast * tLast); // invP22 = p11
            // auxiliary variable \Omega ==> save it to S21; the same for W=>S and W=>t=>S
            LinAlg.DiagonalMatrixHelper.Dot(ref s21, p11);
            LinAlg.DiagonalMatrixHelper.Dot(p11, ref s21);
            // calculate auxiliary variables Q1 and Q2; the same for W=>S and W=>t=>S
            MatrixZ q1 = w1Last; // because w1 = 1.0; 
            // [w2]^-1
            LinAlg.DiagonalMatrixHelper.Inverse2x2Block(ref w211, ref w212, ref w221, ref w222);
            // Q2 = [w2]^-1 * [w2Last]
            long n = w2Last.Cols / 2;
            LongRange up = new(0, n);
            LongRange down = new(n, 2 * n);
            LongRange left = new(0, n);
            LongRange right = new(n, 2 * n);
            MatrixZ? w2Last11 = w2Last[up, left];
            MatrixZ? w2Last12 = w2Last[up, right];
            MatrixZ? w2Last21 = w2Last[down, left];
            MatrixZ? w2Last22 = w2Last[down, right];

            MatrixZ q2 = new(w2Last.Rows, w2Last.Cols, 0.0);
            if (w2Last11 != null && w2Last12 != null && w2Last21 != null && w2Last22 != null)
            {
                //Printer.Write("w2Last11 = ", VMath.Abs(w2Last11));
                //Printer.Write("w2Last12 = ", VMath.Abs(w2Last12));
                //Printer.Write("w2Last21 = ", VMath.Abs(w2Last21));
                //Printer.Write("w2Last22 = ", VMath.Abs(w2Last22));
                Printer.Write("w211 = ", VMath.Abs(w211));
                Printer.Write("w212 = ", VMath.Abs(w212));
                Printer.Write("w221 = ", VMath.Abs(w221));
                Printer.Write("w222 = ", VMath.Abs(w222));

                q2[up, left] = LinAlg.DiagonalMatrixHelper.Dot(w211, w2Last11)
                    + LinAlg.DiagonalMatrixHelper.Dot(w212, w2Last21);
                Printer.Write("q211 = ", VMath.Abs(q2[up, left]));

                q2[up, right] = LinAlg.DiagonalMatrixHelper.Dot(w211, w2Last12)
                    + LinAlg.DiagonalMatrixHelper.Dot(w212, w2Last22);
                Printer.Write("q212 = ", VMath.Abs(q2[up, right]));

                q2[down, left] = LinAlg.DiagonalMatrixHelper.Dot(w221, w2Last11)
                    + LinAlg.DiagonalMatrixHelper.Dot(w222, w2Last21);
                Printer.Write("q221 = ", VMath.Abs(q2[down, left]));

                q2[down, right] = LinAlg.DiagonalMatrixHelper.Dot(w221, w2Last12)
                    + LinAlg.DiagonalMatrixHelper.Dot(w222, w2Last22);
                Printer.Write("q22 = ", VMath.Abs(q2[down, right]));

            }

            //Printer.Write("q2 = ", VMath.Abs(q2), 1);

            // kernel call
            HalfSMatrixKernel(ref s11, ref s21, q1, q2, p11, useWSvariation);
        }

        #endregion
        #region===== Full boundaries =====
        /// <summary>
        /// full S-matrix boundary
        /// uniform | uniform [backward]
        /// </summary>
        /// <param name="s11"> s11 sub-matrix </param>
        /// <param name="s21"> s21 sub-matrix </param>
        /// <param name="s12"> s12 sub-matrix </param>
        /// <param name="s22"> s22 sub-matrix </param>
        /// <param name="gammaLast"> gamma [positive] of the last uniform layer </param>
        /// <param name="w1Last"> w1 of the last uniform layer, diagonal => vector storage = 1.0</param>
        /// <param name="w2Last"> w2 of the uniform layer, diagonal => vector storage </param>
        /// <param name="w1"> w1 of the current uniform layer, diagonal => vector storage = 1.0 </param>
        /// <param name="w2"> w2 of the current uniform layer, diagonal => vector storage </param>
        /// <param name="tLast"> thickness of the last uniform layer </param>
        /// <param name="useWSvariation"> false (default): W=>t=>S, true: W=>S </param>
        private static void FullSMatrixBoundary(ref MatrixZ s11, ref MatrixZ s21,
            ref MatrixZ s12, ref MatrixZ s22,
            VectorZ gammaLast, VectorZ w1Last, VectorZ w2Last,
            VectorZ w1, VectorZ w2, double tLast,
            bool useWSvariation = false)
        {
            // calculate P-term
            VectorZ p11 = VMath.Exp(gammaLast * tLast); // invP22 = p11
            // auxiliary variable \Omega ==> save it to S21; the same for W=>S and W=>t=>S
            LinAlg.DiagonalMatrixHelper.Dot(ref s21, p11);
            LinAlg.DiagonalMatrixHelper.Dot(p11, ref s21);
            // calculate auxiliary variables Q1 and Q2; the same for W=>S and W=>t=>S
            MatrixZ q1 = LinAlg.DiagonalMatrixHelper.GenerateDenseMatrixZ(1.0 / w1);
            MatrixZ q2 = LinAlg.DiagonalMatrixHelper.GenerateDenseMatrixZ(1.0 / w2);
            //MatrixZ q1 = new(w1, true); // because w1Last = 1.0 
            //LinAlg.Inverse(ref q1);
            //MatrixZ q2 = new(w2, true);
            //LinAlg.Inverse(ref q2);
            LinAlg.DiagonalMatrixHelper.Dot(ref q2, w2Last);
            // kernel call
            FullSMatrixKernel(ref s11, ref s21, ref s12, ref s22, q1, q2, p11, useWSvariation);
        }

        /// /// <summary>
        /// full S-matrix boundary
        /// 1D-periodic | uniform [backward]
        /// using the W=>t=>S or W=>S variation with W1/2 symmetry
        /// </summary>
        /// <param name="s11"> s11 sub-matrix </param>
        /// <param name="s21"> s21 sub-matrix </param>
        /// <param name="s12"> s12 sub-matrix </param>
        /// <param name="s22"> s22 sub-matrix </param>
        /// <param name="gammaLast"> gamma [positive] of the uniform layer </param>
        /// <param name="w1Last"> w1 of the uniform layer, diagonal => vector storage = 1.0 </param>
        /// <param name="w2Last"> w2 of the uniform layer, diagonal => vector storage </param>
        /// <param name="w1"> w1 of the periodic layer </param>
        /// <param name="w2"> w2 of the periodic later </param>
        /// <param name="tLast"> thickness of the uniform layer </param>
        /// <param name="useWSvariation"> false (default): W=>t=>S, true: W=>S </param>
        private static void FullSMatrixBoundary(ref MatrixZ s11, ref MatrixZ s21,
            ref MatrixZ s12, ref MatrixZ s22,
            VectorZ gammaLast, VectorZ w1Last, VectorZ w2Last,
            MatrixZ w1, MatrixZ w2, double tLast,
            bool useWSvariation = false)
        {
            // calculate P-term
            VectorZ p11 = VMath.Exp(gammaLast * tLast); // invP22 = p11
                                                        // auxiliary variable \Omega ==> save it to S21; the same for W=>S and W=>t=>S
            LinAlg.DiagonalMatrixHelper.Dot(ref s21, p11);
            LinAlg.DiagonalMatrixHelper.Dot(p11, ref s21);
            // calculate auxiliary variables Q1 and Q2; the same for W=>S and W=>t=>S
            MatrixZ q1 = new(w1, true); // because w1Last = 1.0 
            LinAlg.Inverse(ref q1);
            MatrixZ q2 = new(w2, true);
            LinAlg.Inverse(ref q2);
            LinAlg.DiagonalMatrixHelper.Dot(ref q2, w2Last);
            // kernel call
            FullSMatrixKernel(ref s11, ref s21, ref s12, ref s22, q1, q2, p11, useWSvariation);
        }

        /// /// <summary>
        /// full S-matrix boundary
        /// uniform | 1D-periodic [backward]
        /// using the W=>t=>S formulation with W1/2 symmetry
        /// </summary>
        /// <param name="s11"> s11 sub-matrix </param>
        /// <param name="s21"> s21 sub-matrix </param>
        /// <param name="s12"> s12 sub-matrix </param>
        /// <param name="s22"> s22 sub-matrix </param>
        /// <param name="gammaLast"> gamma [positive] of the periodic layer </param>
        /// <param name="w1Last"> w1 of the periodic layer </param>
        /// <param name="w2Last"> w2 of the periodic layer </param>
        /// <param name="w1"> w1 of the uniform layer, diagonal => vector storage =  1.0 </param>
        /// <param name="w2"> w2 of the uniform layer, diagonal => vector storage </param>
        /// <param name="tLast"> thickness of the periodic layer </param>
        /// <param name="useWSvariation"> false (default): W=>t=>S, true: W=>S </param>
        private static void FullSMatrixBoundary(ref MatrixZ s11, ref MatrixZ s21,
            ref MatrixZ s12, ref MatrixZ s22,
            VectorZ gammaLast, MatrixZ w1Last, MatrixZ w2Last,
            VectorZ w1, VectorZ w2, double tLast,
            bool useWSvariation = false)
        {
            // calculate P-term
            VectorZ p11 = VMath.Exp(gammaLast * tLast); // invP22 = p11
                                                        // auxiliary variable \Omega ==> save it to S21; the same for W=>S and W=>t=>S
            LinAlg.DiagonalMatrixHelper.Dot(ref s21, p11);
            LinAlg.DiagonalMatrixHelper.Dot(p11, ref s21);
            // calculate auxiliary variables Q1 and Q2; the same for W=>S and W=>t=>S
            MatrixZ q1 = w1Last; // because w1 = 1.0; 
            MatrixZ q2 = w2Last;
            LinAlg.DiagonalMatrixHelper.Dot(1.0 / w2, ref q2);
            // kernel call
            FullSMatrixKernel(ref s11, ref s21, ref s12, ref s22, q1, q2, p11, useWSvariation);
        }

        /// <summary>
        /// full S-matrix boundary
        /// periodic | periodic [backward]
        /// using the W=>S formulation with W1/2 symmetry
        /// </summary>
        /// <param name="s11"> s11 sub-matrix </param>
        /// <param name="s21"> s21 sub-matrix </param>
        /// <param name="s12"> s12 sub-matrix </param>
        /// <param name="s22"> s22 sub-matrix </param>
        /// <param name="gammaLast"> gamma of the last layer </param> 
        /// <param name="w1Last"> w1 of the last layer </param>
        /// <param name="w2Last"> w2 of the last layer </param>
        /// <param name="w1"> w1 of the current layer </param>
        /// <param name="w2"> w2 of the current layer </param>
        /// <param name="tLast"> thickness of the last layer </param>
        /// <param name="useWSvariation"> false (default): W=>t=>S, true: W=>S </param>
        private static void FullSMatrixBoundary(ref MatrixZ s11, ref MatrixZ s21,
            ref MatrixZ s12, ref MatrixZ s22,
            VectorZ gammaLast, MatrixZ w1Last, MatrixZ w2Last,
            MatrixZ w1, MatrixZ w2, double tLast,
            bool useWSvariation = false)
        {
            // calculate P-term;
            VectorZ p11 = VMath.Exp(gammaLast * tLast); // invP22 = p11
                                                        // auxiliary variable \Omega ==> save it to S21; the same for W=>S and W=>t=>S
            LinAlg.DiagonalMatrixHelper.Dot(ref s21, p11);
            LinAlg.DiagonalMatrixHelper.Dot(p11, ref s21);
            // calculate auxiliary variables Q1 and Q2; the same for W=>S and W=>t=>S
            MatrixZ q1 = LinAlg.LinearSolve(w1, w1Last);
            MatrixZ q2 = LinAlg.LinearSolve(w2, w2Last);

            //Printer.Write("q2 = ", VMath.Abs(q2), 1);

            // kernel call
            FullSMatrixKernel(ref s11, ref s21, ref s12, ref s22, q1, q2, p11, useWSvariation);
        }
        #endregion
        #region ===== single 1D-periodic layer =====

        /// <summary>
        /// half S-matrix for single 1D periodic layer
        /// and for the in-plane situation
        /// using the W=>t=>S or W=>S variation 
        /// with W1/2 symmetry
        /// </summary>
        /// <param name="layerFront"> uniform layer in front </param>
        /// <param name="layerMiddle"> periodic layer in the middle </param>
        /// <param name="layerBehind"> uniform layer behind </param>
        /// <param name="wavelength"> wavelength in vacuum </param>
        /// <param name="mode"> polarization mode (TE or TM) </param>
        /// <param name="kx0"> lateral shift of spatial frequencies along kx </param>
        /// <param name="fieldsSampling"> (odd) number of spatial frequencies for E/H-field sampling </param>
        /// <param name="mediumSampling"> (odd) number of spatial frequencies for medium sampling </param>
        /// <param name="toeplitztype">Type of Toeplitz matrix (Periodic or Nonperiodic).</param>
        /// <param name="useWSvariation"> false: W=>t=>S; true: W=>S </param>
        /// <param name="saveLayerMediaData"> whether to save sampled media data</param>
        /// <param name="saveLayerModesData"> whether to save computed modes data </param>
        /// <returns> (s11, s21) </returns>
        public static (MatrixZ, MatrixZ) HalfSMatrix(UniformLayer layerFront,
            Periodic1DLayer layerMiddle,
            UniformLayer layerBehind,
            double wavelength,
            InPlanePolMode mode,
            double kx0,
            long fieldsSampling,
            long mediumSampling,
            ToeplitzMatrixType toeplitztype = ToeplitzMatrixType.Nonperiodic,
            bool useWSvariation = false,
            bool saveLayerMediaData = true,
            bool saveLayerModesData = false)
        {
            // initialize
            MatrixZ s11 = LinAlg.IdentityMatrixHelper.GenerateDenseMatrixZ(n: fieldsSampling);
            MatrixZ s21 = new(rows: fieldsSampling, cols: fieldsSampling);
            // grid definition
            double dKx = 2.0 * Math.PI / layerMiddle.Period; // grating constant in k-domain
            GridInfo1D gKx = new(n: fieldsSampling, spacing: dKx); // initialized with zero at the center
            gKx.GetModified(ctrShift: kx0); // shift to have kx0 at the center

            // eigen solvers
            VectorZ gammaBehind, gammaMiddle;
            VectorZ w1Behind, w2Behind, w1Front, w2Front;
            MatrixZ w1Middle, w2Middle;

            (gammaBehind, w1Behind, w2Behind) = layerBehind.ComputeInPlaneModes(wavelength, mode, gKx);
            (gammaMiddle, w1Middle, w2Middle) = layerMiddle.ComputeInPlaneModes(wavelength, mode, fieldsSampling, mediumSampling, kx0,
                toeplitztype: toeplitztype,
                saveMediaData: saveLayerMediaData,
                saveModesData: saveLayerModesData);
            (_, w1Front, w2Front) = layerFront.ComputeInPlaneModes(wavelength, mode, gKx);

            // S-matrix application
            // frist, from layer behind to layer middle
            HalfSMatrixBoundary(ref s11, ref s21,
                gammaBehind, w1Behind, w2Behind,
                w1Middle, w2Middle, layerBehind.Thickness,
                useWSvariation);
            // then, from layer middle to layer front
            HalfSMatrixBoundary(ref s11, ref s21,
                gammaMiddle, w1Middle, w2Middle,
                w1Front, w2Front, layerMiddle.Thickness,
                useWSvariation);

            // return
            return (s11, s21);
        }

        /// <summary>
        /// full S-matrix for single 1D periodic layer
        /// and for the in-plane situation
        /// using the W=>t=>S or W=>S variation 
        /// with W1/2 symmetry
        /// </summary>
        /// <param name="layerFront"> uniform layer in front </param>
        /// <param name="layerMiddle"> periodic layer in the middle </param>
        /// <param name="layerBehind"> uniform layer behind </param>
        /// <param name="wavelength"> wavelength in vacuum </param>
        /// <param name="mode"> polarization mode (TE or TM) </param>
        /// <param name="kx0"> lateral shift of spatial frequencies along kx </param>
        /// <param name="fieldsSampling"> (odd) number of spatial frequencies for E/H-field sampling </param>
        /// <param name="mediumSampling"> (odd) number of spatial frequencies for medium sampling </param>
        /// <param name="toeplitztype">Type of Toeplitz matrix (Periodic or Nonperiodic).</param>
        /// <param name="useWSvariation"> false: W=>t=>S; true: W=>S </param>
        /// <param name="saveLayerMediaData"> whether to save sampled media data</param>
        /// <param name="saveLayerModesData"> whether to save computed modes data </param>
        /// <returns> (s11, s21, s12, s22) </returns>
        public static (MatrixZ, MatrixZ, MatrixZ, MatrixZ) FullSMatrix(UniformLayer layerFront,
            Periodic1DLayer layerMiddle,
            UniformLayer layerBehind,
            double wavelength,
            InPlanePolMode mode,
            double kx0,
            long fieldsSampling,
            long mediumSampling,
            ToeplitzMatrixType toeplitztype = ToeplitzMatrixType.Nonperiodic,
            bool useWSvariation = false,
            bool saveLayerMediaData = true,
            bool saveLayerModesData = false)
        {
            // initialize
            MatrixZ s11 = LinAlg.IdentityMatrixHelper.GenerateDenseMatrixZ(n: fieldsSampling);
            MatrixZ s21 = new(rows: fieldsSampling, cols: fieldsSampling);
            MatrixZ s22 = LinAlg.IdentityMatrixHelper.GenerateDenseMatrixZ(n: fieldsSampling);
            MatrixZ s12 = new(rows: fieldsSampling, cols: fieldsSampling);
            // grid definition
            double dKx = 2.0 * Math.PI / layerMiddle.Period; // grating constant in k-domain
            GridInfo1D gKx = new(n: fieldsSampling, spacing: dKx); // initialized with zero at the center
            gKx.GetModified(ctrShift: kx0); // shift to have kx0 at the center

            // eigen solvers
            VectorZ gammaBehind, gammaMiddle;
            VectorZ w1Behind, w2Behind, w1Front, w2Front;
            MatrixZ w1Middle, w2Middle;

            (gammaBehind, w1Behind, w2Behind) = layerBehind.ComputeInPlaneModes(wavelength, mode, gKx);
            (gammaMiddle, w1Middle, w2Middle) = layerMiddle.ComputeInPlaneModes(wavelength, mode, fieldsSampling, mediumSampling, kx0,
                toeplitztype: toeplitztype,
                saveMediaData: saveLayerMediaData,
                saveModesData: saveLayerModesData);
            (_, w1Front, w2Front) = layerFront.ComputeInPlaneModes(wavelength, mode, gKx);

            // S-matrix application
            // frist, from layer behind to layer middle
            FullSMatrixBoundary(ref s11, ref s21, ref s12, ref s22,
                gammaBehind, w1Behind, w2Behind,
                w1Middle, w2Middle, layerBehind.Thickness,
                useWSvariation);
            // then, from layer middle to layer front
            FullSMatrixBoundary(ref s11, ref s21, ref s12, ref s22,
                gammaMiddle, w1Middle, w2Middle,
                w1Front, w2Front, layerMiddle.Thickness,
                useWSvariation);

            // return
            return (s11, s21, s12, s22);
        }
        #endregion
        #region ===== single 1D-periodic layer + Add. Uniform Layer =====

        /// <summary>
        /// 
        /// </summary>
        /// <param name="layerFront"></param>
        /// <param name="layerAdd"></param>
        /// <param name="layerMiddle"></param>
        /// <param name="layerBehind"></param>
        /// <param name="wavelength"></param>
        /// <param name="mode"></param>
        /// <param name="kx0"></param>
        /// <param name="fieldsSampling"></param>
        /// <param name="mediumSampling"></param>
        /// <param name="useWSvariation"></param>
        /// <param name="saveLayerMediaData"></param>
        /// <param name="saveLayerModesData"></param>
        /// <returns></returns>
        public static (MatrixZ, MatrixZ) HalfSMatrix(
            UniformLayer layerFront,
            UniformLayer layerAdd,
            Periodic1DLayer layerMiddle,
            UniformLayer layerBehind,
            double wavelength,
            InPlanePolMode mode,
            double kx0,
            long fieldsSampling,
            long mediumSampling,
            bool useWSvariation = false,
            bool saveLayerMediaData = true,
            bool saveLayerModesData = false)
        {
            // initialize
            MatrixZ s11 = LinAlg.IdentityMatrixHelper.GenerateDenseMatrixZ(n: fieldsSampling);
            MatrixZ s21 = new(rows: fieldsSampling, cols: fieldsSampling);
            // grid definition
            double dKx = 2.0 * Math.PI / layerMiddle.Period; // grating constant in k-domain
            GridInfo1D gKx = new(n: fieldsSampling, spacing: dKx); // initialized with zero at the center
            gKx.GetModified(ctrShift: kx0); // shift to have kx0 at the center

            // eigen solvers
            VectorZ gammaBehind, gammaMiddle, gammaAdd;
            VectorZ w1Behind, w2Behind, w1Add, w2Add, w1Front, w2Front;
            MatrixZ w1Middle, w2Middle;

            (gammaBehind, w1Behind, w2Behind) = layerBehind.ComputeInPlaneModes(wavelength, mode, gKx);
            gammaBehind *= Complex.ImaginaryOne * 2.0 * Math.PI / wavelength;
            (gammaMiddle, w1Middle, w2Middle) = layerMiddle.ComputeInPlaneModes(wavelength, mode, fieldsSampling, mediumSampling, kx0,
                saveMediaData: saveLayerMediaData,
                saveModesData: saveLayerModesData);
            (gammaAdd, w1Add, w2Add) = layerAdd.ComputeInPlaneModes(wavelength, mode, gKx);
            gammaAdd *= Complex.ImaginaryOne * 2.0 * Math.PI / wavelength;
            (_, w1Front, w2Front) = layerFront.ComputeInPlaneModes(wavelength, mode, gKx);

            // S-matrix application
            // frist, from layer behind to layer middle
            HalfSMatrixBoundary(ref s11, ref s21,
                gammaBehind, w1Behind, w2Behind,
                w1Middle, w2Middle, layerBehind.Thickness,
                useWSvariation);
            // next, from layer middle to layer add
            HalfSMatrixBoundary(ref s11, ref s21,
                gammaMiddle, w1Middle, w2Middle,
                w1Add, w2Add, layerMiddle.Thickness,
                useWSvariation);
            // finally, from layer add to layer front
            HalfSMatrixBoundary(ref s11, ref s21,
                gammaAdd, w1Add, w2Add,
                w1Front, w2Front, layerAdd.Thickness,
                useWSvariation);

            // return
            return (s11, s21);
        }


        #endregion
        #region ===== single 2D-periodic layer =====

        /// <summary>
        /// half S-matrix for single periodic layer
        /// using the W=>t=>S or W=>S variation 
        /// with W1/2 symmetry
        /// </summary>
        /// <param name="wavelength"> wavelength in vacuum </param>
        /// <param name="layerFront"> uniform layer in front </param>
        /// <param name="layerMiddle"> periodic layer in the middle </param>
        /// <param name="layerBehind"> uniform layer behind </param>
        /// <param name="nKxs"> number of spatial frequencies kx </param>
        /// <param name="nKys"> number of spatial frequencies ky </param>
        /// <param name="kx0"> central spatial frequency along kx </param>
        /// <param name="ky0"> central spatial frequency along ky </param>
        /// <param name="mediumOverSampX"> oversampling factor of the medium along x, >= 1.0 </param>
        /// <param name="mediumOverSampY"> oversampling factor of the medium along y, >= 1.0 </param>
        /// <param name="useWSvariation"> false (default): W=>t=>S, true: W=>S </param>
        /// <param name="saveLayerMediaData"> whether to save sampled media data</param>
        /// <param name="saveLayerModesData"> whether to save computed modes data </param>
        /// <param name="printTimeInfo"> whether to print computational time info </param>
        /// <returns> (s11, s21) </returns>
        [Obsolete]
        public static (MatrixZ, MatrixZ) HalfSMatrix(double wavelength,
            UniformLayer layerFront,
            Periodic2DLayer layerMiddle,
            UniformLayer layerBehind,
            long nKxs, long nKys,
            double kx0 = 0.0, double ky0 = 0.0,
            double mediumOverSampX = 1.0, double mediumOverSampY = 1.0,
            bool useWSvariation = false,
            bool saveLayerMediaData = true,
            bool saveLayerModesData = false,
            bool printTimeInfo = false)
        {
            // initialize
            long n = nKys * nKxs; //ky.Count * kx.Count; 
            MatrixZ s11 = LinAlg.IdentityMatrixHelper.GenerateDenseMatrixZ(2*n);
            MatrixZ s21 = new(2*n, 2*n, 0.0);

            // eigen solvers
            double dKx = 2.0 * Math.PI / layerMiddle.PeriodX;
            double dKy = 2.0 * Math.PI / layerMiddle.PeriodY;
            VectorZ gammaBehind, gammaMiddle;
            MatrixZ w1Behind, w2Behind;
            //VectorZ w1Behind11, w1Behind22, w2Behind11, w2Behind12, w2Behidn21, w2Behind22;
            MatrixZ w1Front, w2Front;
            //VectorZ w1Front11, w1Front22, w2Front11, w2Front12, w2Front21, w2Front22;
            MatrixZ w1Middle, w2Middle;

            Stopwatch sw = Stopwatch.StartNew();
            (gammaBehind, w1Behind, w2Behind) = layerBehind.ComputeModes(wavelength, nKxs, nKys, dKx, dKy, kx0, ky0);
            //(gammaBehind, w1Behind11, w1Behind22,
            //    w2Behind11, w2Behind12, w2Behidn21, w2Behind22) = layerBehind.ComputeModesSimp(wavelength, nKxs, nKys, dKx, dKy, kx0, ky0);
            (gammaMiddle, w1Middle, w2Middle) = layerMiddle.ComputeModes(wavelength, nKxs, nKys, kx0, ky0, mediumOverSampX, mediumOverSampY,
                saveMediaData: saveLayerMediaData,
                saveModesData: saveLayerModesData);
            (_, w1Front, w2Front) = layerFront.ComputeModes(wavelength, nKxs, nKys, dKx, dKy, kx0, ky0);
            //(_, w1Front11, w1Front22,
            //    w2Front11, w2Front12, w2Front21, w2Front22) = layerFront.ComputeModesSimp(wavelength, nKxs, nKys, dKx, dKy, kx0, ky0);
            sw.Stop();
            if (printTimeInfo) { Printer.Logging($" - eigen solution time cost: {sw.ElapsedMilliseconds} [ms]"); }

            // S-matrix application
            sw.Restart();
            // frist, from layer behind to layer middle
            HalfSMatrixBoundary(ref s11, ref s21,
                gammaBehind, w1Behind, w2Behind,
                w1Middle, w2Middle, layerBehind.Thickness,
                useWSvariation);
            //HalfSMatrixBoundary(ref s11, ref s21,
            //    gammaBehind, w1Behind11, w1Behind22,
            //    w2Behind11, w2Behind12, w2Behidn21, w2Behind22,
            //    w1Middle, w2Middle, layerBehind.Thickness,
            //    useWSvariation);
            sw.Stop();
            if (printTimeInfo) { Printer.Logging($" - first S-matrix boundary time cost: {sw.ElapsedMilliseconds} [ms]"); }
            sw.Restart();
            // then, from layer middle to layer front
            HalfSMatrixBoundary(ref s11, ref s21,
                gammaMiddle, w1Middle, w2Middle,
                w1Front, w2Front, layerMiddle.Thickness,
                useWSvariation);
            //HalfSMatrixBoundary(ref s11, ref s21,
            //    gammaMiddle, w1Middle, w2Middle,
            //    w1Front11, w1Front22,
            //    w2Front11, w2Front12, w2Front21, w2Front22, layerMiddle.Thickness,
            //    useWSvariation);
            sw.Stop();
            if (printTimeInfo) { Printer.Logging($" - second S-matrix boundary time cost: {sw.ElapsedMilliseconds} [ms]"); }

            // done and return :) */
            return (s11, s21);
        }


        /// <summary>
        /// half S-matrix for single periodic layer
        /// using the W=>t=>S or W=>S variation 
        /// with W1/2 symmetry
        /// </summary>
        /// <param name="wavelength"> wavelength in vacuum </param>
        /// <param name="layerFront"> uniform layer in front </param>
        /// <param name="layerMiddle"> periodic layer in the middle </param>
        /// <param name="layerBehind"> uniform layer behind </param>
        /// <param name="kx0"> central spatial frequency along kx </param>
        /// <param name="ky0"> central spatial frequency along ky </param>
        /// <param name="fieldsSamplingX"> sampling number for E/H fields along x direction </param>
        /// <param name="fieldsSamplingY"> sampling number for E/H fields along y direction </param>
        /// <param name="mediumSamplingX"> sampling number for medium along x direction </param>
        /// <param name="mediumSamplingY"> sampling number for medium along y direction </param>
        /// <param name="useWSvariation"> false (default): W=>t=>S, true: W=>S </param>
        /// <param name="saveLayerMediaData"> whether to save sampled media data</param>
        /// <param name="saveLayerModesData"> whether to save computed modes data </param>
        /// <returns> (s11, s21) </returns>
        public static (MatrixZ, MatrixZ) HalfSMatrix(double wavelength,
            UniformLayer layerFront,
            Periodic2DLayer layerMiddle,
            UniformLayer layerBehind,
            double kx0, double ky0,
            long fieldsSamplingX, long fieldsSamplingY,
            long mediumSamplingX, long mediumSamplingY,
            bool useWSvariation = false,
            bool saveLayerMediaData = true,
            bool saveLayerModesData = false)
        {
            // initialize
            long n = fieldsSamplingX * fieldsSamplingY;
            MatrixZ s11 = LinAlg.IdentityMatrixHelper.GenerateDenseMatrixZ(2 * n);
            MatrixZ s21 = new(rows: 2 * n, cols: 2 * n);

            // eigen solvers
            double dKx = 2.0 * Math.PI / layerMiddle.PeriodX;
            double dKy = 2.0 * Math.PI / layerMiddle.PeriodY;
            VectorZ gammaBehind, gammaMiddle;
            MatrixZ w1Behind, w2Behind;
            MatrixZ w1Front, w2Front;
            MatrixZ w1Middle, w2Middle;

            (gammaBehind, w1Behind, w2Behind) = layerBehind.ComputeModes(wavelength, fieldsSamplingX, fieldsSamplingY, dKx, dKy, kx0, ky0);
            (gammaMiddle, w1Middle, w2Middle) = layerMiddle.ComputeModes(wavelength, fieldsSamplingX, fieldsSamplingY, mediumSamplingX, mediumSamplingY, kx0, ky0,
                saveMediaData: saveLayerMediaData,
                saveModesData: saveLayerModesData);
            (_, w1Front, w2Front) = layerFront.ComputeModes(wavelength, fieldsSamplingX, fieldsSamplingY, dKx, dKy, kx0, ky0);

            // S-matrix application
            // frist, from layer behind to layer middle
            HalfSMatrixBoundary(ref s11, ref s21,
                gammaBehind, w1Behind, w2Behind,
                w1Middle, w2Middle, layerBehind.Thickness,
                useWSvariation);
            // then, from layer middle to layer front
            HalfSMatrixBoundary(ref s11, ref s21,
                gammaMiddle, w1Middle, w2Middle,
                w1Front, w2Front, layerMiddle.Thickness,
                useWSvariation);

            // done and return :) 
            return (s11, s21);
        }


        /// <summary>
        /// ... ???
        /// </summary>
        /// <param name="wavelength"></param>
        /// <param name="layerFront"></param>
        /// <param name="layerMiddle"></param>
        /// <param name="layerBehind"></param>
        /// <param name="nKxs"></param>
        /// <param name="nKys"></param>
        /// <param name="kx0"></param>
        /// <param name="ky0"></param>
        /// <param name="mediumOverSampX"></param>
        /// <param name="mediumOverSampY"></param>
        /// <param name="useWSvariation"></param>
        /// <returns></returns>
        [Obsolete]
        public static (MatrixZ, MatrixZ) HalfSMatrix2(double wavelength,
            UniformLayer layerFront,
            Periodic2DLayer layerMiddle,
            UniformLayer layerBehind,
            long nKxs, long nKys,
            double kx0 = 0.0, double ky0 = 0.0,
            double mediumOverSampX = 1.0, double mediumOverSampY = 1.0,
            bool useWSvariation = false)
        {
            // initialize
            long n = nKys * nKxs; //ky.Count * kx.Count; 
            MatrixZ s11 = LinAlg.IdentityMatrixHelper.GenerateDenseMatrixZ(2 * n);
            MatrixZ s21 = new(2 * n, 2 * n, 0.0);

            // eigen solvers
            double dKx = 2.0 * Math.PI / layerMiddle.PeriodX;
            double dKy = 2.0 * Math.PI / layerMiddle.PeriodY;
            VectorZ gammaBehind, gammaMiddle;
            VectorZ w1Behind11, w1Behind22, w2Behind11, w2Behind12, w2Behidn21, w2Behind22;
            VectorZ w1Front11, w1Front22, w2Front11, w2Front12, w2Front21, w2Front22;
            MatrixZ w1Middle, w2Middle;

            Stopwatch sw = Stopwatch.StartNew();
            (gammaBehind, w1Behind11, w1Behind22,
                w2Behind11, w2Behind12, w2Behidn21, w2Behind22) = layerBehind.ComputeModesSimp(wavelength, nKxs, nKys, dKx, dKy, kx0, ky0);
            (gammaMiddle, w1Middle, w2Middle) = layerMiddle.ComputeModes(wavelength, nKxs, nKys, kx0, ky0, mediumOverSampX, mediumOverSampY);
            (_, w1Front11, w1Front22,
                w2Front11, w2Front12, w2Front21, w2Front22) = layerFront.ComputeModesSimp(wavelength, nKxs, nKys, dKx, dKy, kx0, ky0);
            Printer.Write("w1Front11 = ", VMath.Abs(w1Front11));
            Printer.Write("w1Front22 = ", VMath.Abs(w1Front22));
            Printer.Write("w2Front11 = ", VMath.Abs(w2Front11));
            Printer.Write("w2Front12 = ", VMath.Abs(w2Front12));
            Printer.Write("w2Front21 = ", VMath.Abs(w2Front21));
            Printer.Write("w2Front22 = ", VMath.Abs(w2Front22));

            sw.Stop();
            Printer.Logging($" - eigen solution time cost: {sw.ElapsedMilliseconds} [ms]");

            // S-matrix application
            sw.Restart();
            // frist, from layer behind to layer middle
            HalfSMatrixBoundary(ref s11, ref s21,
                gammaBehind, w1Behind11, w1Behind22,
                w2Behind11, w2Behind12, w2Behidn21, w2Behind22,
                w1Middle, w2Middle, layerBehind.Thickness,
                useWSvariation);
            sw.Stop();
            Printer.Logging($" - first S-matrix boundary time cost: {sw.ElapsedMilliseconds} [ms]");
            sw.Restart();
            // then, from layer middle to layer front
            HalfSMatrixBoundary(ref s11, ref s21,
                gammaMiddle, w1Middle, w2Middle,
                w1Front11, w1Front22,
                w2Front11, w2Front12, w2Front21, w2Front22, layerMiddle.Thickness,
                useWSvariation);
            sw.Stop();
            Printer.Logging($" - second S-matrix boundary time cost: {sw.ElapsedMilliseconds} [ms]");

            // done and return :) */
            return (s11, s21);

        }

        #endregion

        #endregion


    }
}
