using Complex = System.Numerics.Complex;
using VEMS.MathCore;

namespace VEMS.EMSolver
{

    /// <summary>
    /// uniform diagonally anisotropic layer
    /// with 3x non-zero diagonal elements in the 
    /// permittivity and permeability tensor
    /// for use in RCWA and more
    /// </summary>
    public class Uniform3x1Layer : EigenLayer
    {
        #region properties

        /// <summary>
        /// (1,1)-element in the permittivity tensor 
        /// <para> variable: wavelength </para>
        /// <para> function: complex-valued epsilon[1,1] </para>
        /// </summary>
        public Func<double, Complex> Epsilon11 { get; set; }

        /// <summary>
        /// (2,2)-element in the permittivity tensor 
        /// <para> variable: wavelength </para>
        /// <para> function: complex-valued epsilon[2,2] </para>
        /// </summary>
        public Func<double, Complex> Epsilon22 { get; set; }

        /// <summary>
        /// (3,3)-element in the permittivity tensor
        /// <para> variable: wavelength </para>
        /// <para> function: complex-valued epsilon[3,3] </para>
        /// </summary>
        public Func<double, Complex> Epsilon33 { get; set; }

        /// <summary>
        /// (1,1)-element in the permeability tensor
        /// <para> variable: wavelength </para>
        /// <para> function: complex-valued mu[1,1] </para>
        /// </summary>
        public Func<double, Complex> Mu11 { get; set; }

        /// <summary>
        /// (2,2)-element in the permeability tensor
        /// <para> variable: wavelength </para>
        /// <para> function: complex-valued mu[2,2] </para>
        /// </summary>
        public Func<double, Complex> Mu22 { get; set; }

        /// <summary>
        /// (3,3)-element in the permeability tensor
        /// <para> variable: wavelength </para>
        /// <para> function: complex mu[3,3] </para>
        /// </summary>
        public Func<double, Complex> Mu33 { get; set; }



        #endregion
        #region constructors

        /// <summary>
        /// default constructor
        /// </summary>
        internal Uniform3x1Layer()
        {
            Epsilon11 = (w) => 1;
            Epsilon22 = (w) => 1;
            Epsilon33 = (w) => 1;
            Mu11 = (w) => 1;
            Mu22 = (w) => 1;
            Mu33 = (w) => 1;
        }

        /// <summary>
        /// constructs a uniform diagonally anisotropic layer
        /// with permittvity and permeability tensor functions
        /// </summary>
        /// <param name="epsilon11"> (1,1)-element in the permittvity tensor </param>
        /// <param name="epsilon22"> (2,2)-element in the permittvity tensor </param>
        /// <param name="epsilon33"> (3,3)-element in the permittvity tensor </param>
        /// <param name="mu11"> (1,1)-element in the permeability tensor </param>
        /// <param name="mu22"> (2,2)-element in the peameability tensor </param>
        /// <param name="mu33"> (3,3)-element in the peameability tensor </param>
        /// <param name="thickness"> thickness of the uniform layer </param>
        public Uniform3x1Layer(Func<double, Complex> epsilon11, 
            Func<double, Complex> epsilon22, 
            Func<double, Complex> epsilon33, 
            Func<double, Complex> mu11, 
            Func<double, Complex> mu22, 
            Func<double, Complex> mu33,
            double thickness = 0.0)
        {
            Epsilon11 = epsilon11;
            Epsilon22 = epsilon22;
            Epsilon33 = epsilon33;
            Mu11 = mu11;
            Mu22 = mu22;
            Mu33 = mu33;
            Thickness = thickness;
        }

        /// <summary>
        /// constructs a uniform diagonally anisotropic layer
        /// with permittvity and permeability tensor values
        /// </summary>
        /// <param name="epsilon11"> (1,1)-element in the permittvity tensor </param>
        /// <param name="epsilon22"> (2,2)-element in the permittvity tensor </param>
        /// <param name="epsilon33"> (3,3)-element in the permittvity tensor </param>
        /// <param name="mu11"> (1,1)-element in the permeability tensor </param>
        /// <param name="mu22"> (2,2)-element in the peameability tensor </param>
        /// <param name="mu33"> (3,3)-element in the peameability tensor </param>
        /// <param name="thickness"> thickness of the uniform layer </param>
        public Uniform3x1Layer(Complex epsilon11, Complex epsilon22, Complex epsilon33,
            Complex mu11, Complex mu22, Complex mu33,
            double thickness = 0.0) 
            : this(epsilon11: (w) => epsilon11, 
                  epsilon22: (w) => epsilon22,
                  epsilon33: (w) => epsilon33,
                  mu11: (w) => mu11,
                  mu22: (w) => mu22,
                  mu33: (w) => mu33,
                  thickness: thickness)
        { }

        /// <summary>
        /// constructs a uniform diagonally anisotropic layer
        /// with refractive index tensor values
        /// </summary>
        /// <param name="n11"> (1,1)-element in the refractive index tensor </param>
        /// <param name="n22"> (2,2)-element in the refractive index tensor </param>
        /// <param name="n33"> (3,3)-element in the refractive index tensor </param>
        /// <param name="thickness"> thickness of the uniform layer </param>
        public Uniform3x1Layer(Complex n11, Complex n22, Complex n33,
            double thickness = 0.0)
            : this(epsilon11: (w) => n11 * n11, 
                  epsilon22: (w) => n22 * n22,
                  epsilon33: (w) => n33 * n33,
                  mu11: (w) => 1, mu22: (w) => 1, mu33: (w) => 1,
                  thickness: thickness)
        { }

        #endregion
        #region ==== eigenvalues ====

        /// <summary>
        /// computes nz i.e. kz/k0 normalized 
        /// for scalar nx for in-plane cases
        /// </summary>
        /// <param name="wavelength"> wavelength in vacuum </param>
        /// <param name="polarization"> in-plane polarization mode option </param>
        /// <param name="nx"> normalized kx (nx = kx/k0) </param>
        /// <param name="scal"> additional scaling factor </param>
        /// <returns> result = (scal*) nz </returns>
        public Complex ComputePNz(double wavelength,
            InPlanePolMode polarization,
            double nx,
            double scal = 1.0)
        {
            Complex epsilon11, epsilon22, epsilon33;
            Complex mu11, mu22, mu33;
            switch (polarization)
            {
                case InPlanePolMode.TM:
                    {
                        epsilon11 = Epsilon11(wavelength);
                        epsilon33 = Epsilon33(wavelength);
                        mu22 = Mu22(wavelength);
                        return scal * Complex.Sqrt(epsilon11 * mu22 - epsilon11/epsilon33 * nx * nx);
                    }
                case InPlanePolMode.TE:
                    {
                        epsilon22 = Epsilon22(wavelength);
                        mu11 = Mu11(wavelength);
                        mu33 = Mu33(wavelength);
                        return scal * Complex.Sqrt(epsilon22 * mu11 - mu11 / mu33 * nx * nx);
                    }
                default: goto case InPlanePolMode.TE;
            }
        }


        //public 

        // computes Nz ...

        #endregion

        // only VMath for RCWA routines
        #region ==== RCWA1Dp ====

        /// <summary>
        /// computes the eigen information for in-plane case
        /// used for 1D RCWA calculation
        /// TM mode: [E] = [E_x] \\ [H] = [H_y]
        /// TE mode: [E] = [E_y] \\ [H] = [H_x]        
        /// </summary>
        /// <param name="wavelength"> working wavelength in vacuum </param>
        /// <param name="polarization"> in-plane polarization mode </param>
        /// <param name="gKx"> uniform grid that defines the kx-values </param>
        /// <returns> (nz, w1, w2) as vectors </returns>
        public (VectorZ, VectorZ, VectorZ) ComputePModes(double wavelength,
            InPlanePolMode polarization,
            GridInfo1D gKx)
        {
            // prepares parameters
            double k0 = 2.0 * Math.PI / wavelength;
            Complex epsilon11, epsilon22, epsilon33;
            Complex mu11, mu22, mu33;

            // generates kx-values
            VectorD kx = gKx.GetCoordinates();
            VectorD nx = kx / k0;

            // computes eigen information
            VectorZ nz;
            VectorZ w1 = new(count: nx.Count, initVal: 1.0);
            VectorZ w2;
            switch (polarization)
            {
                case InPlanePolMode.TM:
                    {
                        epsilon11 = Epsilon11(wavelength);
                        epsilon33 = Epsilon33(wavelength);
                        mu22 = Mu22(wavelength);
                        nz = VMath.Sqrt(epsilon11 * mu22 - epsilon11 / epsilon33 * VMath.Square(nx));
                        w2 = epsilon11 / nz;
                        break;
                    }
                case InPlanePolMode.TE:
                    {
                        epsilon22 = Epsilon22(wavelength);
                        mu11 = Mu11(wavelength);
                        mu33 = Mu33(wavelength);
                        nz = VMath.Sqrt(epsilon22 * mu11 - mu11 / mu33 * VMath.Square(nx));
                        w2 = (mu11 == 1.0) ? -nz : VMath.Scale(x: nz, a: -1.0 / mu11);
                        break;
                    }
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

        // ...

        #endregion

    }
}
