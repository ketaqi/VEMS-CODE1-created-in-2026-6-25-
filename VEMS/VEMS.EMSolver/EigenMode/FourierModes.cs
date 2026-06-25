using VEMS.MathCore;
using Complex = System.Numerics.Complex;

namespace VEMS.EMSolver
{
    /// <summary>
    /// 
    /// </summary>
    public class FourierModes
    {
        private FourierModes() { }


        /// <summary>
        /// Fourier eigen mode for homogeneous isotropic medium
        /// </summary>
        public class HomogeneousIsotropic
        {

            #region properties 

            /// <summary>
            /// get / set value of the wavelength in vacuum
            /// </summary>
            public double Wavelength { get; set; }


            /// <summary>
            /// get / set value of epsilon (complex value)
            /// </summary>
            public Complex Epsilon { get; set; }


            /// <summary>
            /// get / set value of mu (complex value)
            /// </summary>
            public Complex Mu { get; set; }


            /// <summary>
            /// get / set kR value
            /// after setting new value, eigen mode must be recalculated
            /// </summary>
            public VectorD KR { get; set; }


            /// <summary>
            /// get / set gamma (as vector)
            /// </summary>
            public VectorZ Gamma { get; set; }


            /// <summary>
            /// get / set w1 (as vector)
            /// </summary>
            public VectorZ W1 { get; set; }


            /// <summary>
            /// get / set w2 (as vector)
            /// </summary>
            public VectorZ W2 { get; set; }


            /// <summary>
            /// get / set w1 (as vector) for TE case
            /// </summary>
            public VectorZ W1TE { get; set; }


            /// <summary>
            /// get / set w2 (as vector) for TE case
            /// </summary>
            public VectorZ W2TE { get; set; }


            /// <summary>
            /// get / set w1 (as vector) for TM case
            /// </summary>
            public VectorZ W1TM { get; set; }


            /// <summary>
            /// get / set w2 (as vector) for TM case
            /// </summary>
            public VectorZ W2TM { get; set; }

            #endregion

            #region constructor

            /// <summary>
            /// initialize a default FourierMode_HomogeneousIsotropic class
            /// </summary>
            public HomogeneousIsotropic() { }


            /// <summary>
            /// initialize a FourierMode_HomogeneousIsotropic class
            /// </summary>
            /// <param name="wavelength"> wavelength in vacuum </param>
            /// <param name="epsilon"> complex permittivity epsilon </param>
            public HomogeneousIsotropic(double wavelength,
                Complex epsilon)
            {
                Wavelength = wavelength;
                Epsilon = epsilon;
                Mu = 1.0;
            }


            /// <summary>
            /// initialize a FourierMode_HomogeneousIsotropic class
            /// </summary>
            /// <param name="wavelength"> wavelength in vacuum </param>
            /// <param name="epsilon"> complex permittivity epsilon </param>
            /// <param name="mu"> complex permeability mu </param>
            public HomogeneousIsotropic(double wavelength,
                Complex epsilon,
                Complex mu)
            {
                Wavelength = wavelength;
                Epsilon = epsilon;
                Mu = mu;
            }

            #endregion

            #region methods


            /// <summary>
            /// calculates eigen modes for TE and TM cases
            /// giving the results explicitly
            /// </summary>
            /// <param name="kR"> kR (single scalar value) </param>
            /// <param name="gamma"> gamma (eigenvalue) </param>
            /// <param name="w1TE"> w1 for TE case </param>
            /// <param name="w2TE"> w2 for TE case </param>
            /// <param name="w1TM"> w1 for TM case </param>
            /// <param name="w2TM"> w2 for TM case </param>
            public void CalculateEigenMode(double kR,
                out Complex gamma,
                out Complex w1TE, out Complex w2TE,
                out Complex w1TM, out Complex w2TM)
            {
                // preparation
                double k0 = 2.0 * Math.PI / Wavelength;
                double nx = kR / k0;
                // calculate nz
                Complex nz = Complex.Sqrt(Epsilon * Mu - nx * nx);
                gamma = new Complex(0.0, 1.0) * k0 * nz;
                // calculate w
                w1TE = 1.0;
                w2TE = -nz / Mu;
                w1TM = 1.0;
                w2TM = Epsilon / nz;
            }


            /// calculates eigen modes for TE and TM cases
            /// giving the results explicitly
            /// with w1/2 symmetry and w1 = 1.0
            public void ComputeEigenMode(double kR,
                out Complex gamma,
                out Complex w2TE,
                out Complex w2TM)
            {
                // preparation
                double k0 = 2.0 * Math.PI / Wavelength;
                double nx = kR / k0;
                // calculate nz
                Complex nz = Complex.Sqrt(Epsilon * Mu - nx * nx);
                gamma = new Complex(0.0, 1.0) * k0 * nz;
                // calculate w
                w2TE = -nz / Mu;
                w2TM = Epsilon / nz;
            }


            /// <summary>
            /// calculates eigen modes for either TE or TM case
            /// giving the results explicitly
            /// </summary>
            /// <param name="kR"> kR (single scalar value) </param>
            /// <param name="gamma"> gamma (eigenvalue) </param>
            /// <param name="w1"> w1 for either TE or TM </param>
            /// <param name="w2"> w2 for either TE or TM </param>
            /// <param name="TEmode"> whether to calculate for TE mode </param>
            public void CalculateEigenMode(double kR,
                out Complex gamma,
                out Complex w1, out Complex w2,
                bool TEmode)
            {
                // preparation
                double k0 = 2.0 * Math.PI / Wavelength;
                double nx = kR / k0;
                // calculate nz
                Complex nz = Complex.Sqrt(Epsilon * Mu - nx * nx);
                gamma = new Complex(0.0, 1.0) * k0 * nz;
                // calculate w
                if (TEmode)
                {
                    w1 = 1.0;
                    w2 = -nz / Mu;
                }
                else
                {
                    w1 = 1.0;
                    w2 = Epsilon / nz;
                }
            }


            /// calculates eigen modes for either TE or TM case
            /// giving the results explicitly
            /// with w1/2 symmetry and w1 = 1.0
            public void ComputeEigenMode(double kR,
                out Complex gamma,
                out Complex w2,
                bool TEmode)
            {
                // preparation
                double k0 = 2.0 * Math.PI / Wavelength;
                double nx = kR / k0;
                // calculate nz
                Complex nz = Complex.Sqrt(Epsilon * Mu - nx * nx);
                gamma = new Complex(0.0, 1.0) * k0 * nz;
                // calculate w
                if (TEmode)
                {
                    w2 = -nz / Mu;
                }
                else
                {
                    w2 = Epsilon / nz;
                }
            }


            ///// <summary>
            ///// calculates eigen modes for TE and TM cases
            ///// saving results into mode object
            ///// </summary>
            ///// <param name="kR"> kR (real-valued vector)</param>
            //public void ComputeEigenMode(RealVector kR)
            //{
            //    KR = kR;
            //    // preparation 
            //    double k0 = 2.0 * Math.PI / Wavelength;
            //    RealVector nx = VMath.Scale(kR, 1.0 / k0);
            //    ComplexVector nx2 = new(VMath.Square(nx));
            //    // calculate nz
            //    ComplexVector temp = new ComplexVector(kR.Length, Epsilon * Mu);
            //    ComplexVector nz = VMath.Substract(temp, nx2);
            //    nz = VMath.SquareRoot(nz);
            //    Gamma = VMath.Scale(nz, new Complex(0.0, 1.0) * k0);
            //    // calculate w
            //    W1TE = new ComplexVector(kR.Length, 1.0);
            //    W2TE = VMath.Scale(nz, -1.0 / Mu);
            //    W1TM = new ComplexVector(kR.Length, 1.0);
            //    W2TM = VMath.Divide(new ComplexVector(kR.Length, Epsilon), nz);
            //}


            ///// <summary>
            ///// calculates eigen modes for TE and TM cases
            ///// saving results into mode object
            ///// </summary>
            ///// <param name="kR"> kR (real-valued vector)</param>
            ///// <param name="TEmode"> whether to calculate for TE mode </param>
            //public void ComputeEigenMode(RealVector kR,
            //    bool TEmode)
            //{
            //    KR = kR;
            //    // preparation 
            //    double k0 = 2.0 * Math.PI / Wavelength;
            //    RealVector nx = VMath.Scale(kR, 1.0 / k0);
            //    ComplexVector nx2 = new(VMath.Square(nx));
            //    // calculate nz
            //    ComplexVector temp = new(kR.Length, Epsilon * Mu);
            //    ComplexVector nz = VMath.Substract(temp, nx2);
            //    nz = VMath.SquareRoot(nz);
            //    Gamma = VMath.Scale(nz, new Complex(0.0, 1.0) * k0);
            //    // calculate w
            //    if (TEmode)
            //    {
            //        W1 = new ComplexVector(kR.Length, 1.0);
            //        W2 = VMath.Scale(nz, -1.0 / Mu);
            //    }
            //    else
            //    {
            //        W1 = new ComplexVector(kR.Length, 1.0);
            //        W2 = VMath.Divide(new ComplexVector(kR.Length, Epsilon), nz);
            //    }
            //}


            /// <summary>
            /// resets the mode information storage
            /// cleans KX, Gamma, W1, W2, W1TE, W2TE, W1TM, W2TM
            /// </summary>
            public void ResetStorage()
            {
                KR = null;
                Gamma = null;
                W1 = null;
                W2 = null;
                W1TE = null;
                W2TE = null;
                W1TM = null;
                W2TM = null;
            }


            /// <summary>
            /// checks if this mode is physicall equal to another
            /// including Wavelength, Epsilon, Mu
            /// </summary>
            /// <param name="other"> another mode </param>
            /// <returns> true or false </returns>
            public bool IsPhysicallyEqualWith(HomogeneousIsotropic other)
            {
                if (Wavelength == other.Wavelength
                    && Epsilon == other.Epsilon
                    && Mu == other.Mu)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }


            /// <summary>
            /// checks if this mode is completely equal to another
            /// including physical terms and KX
            /// </summary>
            /// <param name="other"> another mode </param>
            /// <returns> true or false </returns>
            public bool IsCompletelyEqualWith(HomogeneousIsotropic other)
            {
                if (IsPhysicallyEqualWith(other)
                    && KR == other.KR)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            #endregion

        }


    }

    /// <summary>
    /// 
    /// </summary>
    public class HomoIsoLayerMode
    {

    }

}
