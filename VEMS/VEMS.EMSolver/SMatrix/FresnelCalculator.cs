using Complex = System.Numerics.Complex;

namespace VEMS.EMSolver
{
    /// <summary>
    /// Fresnel matrix class
    /// </summary>
    public class FresnelCalculator
    {

        #region ==== half S-matrix ====

        /// <summary>
        /// Computes the Fresnel matrix for a single interface between two homogeneous media
        /// with refractive indices <paramref name="n1"/> and <paramref name="n2"/> for a selected polarization mode.
        /// </summary>
        /// <param name="wavelength">Wavelength in vacuum.</param>
        /// <param name="n1">Complex refractive index on the incidence side.</param>
        /// <param name="n2">Complex refractive index on the transmission side.</param>
        /// <param name="kx">Transverse spatial frequency.</param>
        /// <param name="polarization">Polarization mode: TE or TM. Default is TE.</param>
        /// <returns>
        /// A tuple containing the Fresnel transmission coefficient (S11) and reflection coefficient (S21).
        /// </returns>
        public static (Complex, Complex) ComputeFresnelMatrix(double wavelength,
            Complex n1, Complex n2, double kx,
            InPlanePolMode polarization = InPlanePolMode.TE)
        {
            double k0 = 2.0 * Math.PI / wavelength;
            double nx = kx / k0;

            // eigen info in transmission medium (n2)
            UniformLayer layerLast = new(n: n2);
            (_, _, Complex wLast) = layerLast.ComputeInPlaneModes(
                wavelength: wavelength, nx: nx, mode: polarization);

            // eigen info in incidence medium (n1)
            UniformLayer layer = new(n: n1);
            (_, _, Complex w) = layer.ComputeInPlaneModes(
                wavelength: wavelength, nx: nx, mode: polarization);

            // Fresnel coefficients calculation
            Complex temp = w + wLast;
            Complex s11 = 2.0 * w / temp;
            Complex s21 = (w - wLast) / temp;
            return (s11, s21);
        }


        /// <summary>
        /// Computes the half S-matrix (Fresnel transmission and reflection coefficients)
        /// for a single interface between two homogeneous media with refractive indices <paramref name="n1"/> and <paramref name="n2"/>
        /// for a selected polarization mode.
        /// </summary>
        /// <param name="wavelength">Wavelength in vacuum.</param>
        /// <param name="n1">Complex refractive index on the incidence side.</param>
        /// <param name="n2">Complex refractive index on the transmission side.</param>
        /// <param name="kx">Transverse spatial frequency.</param>
        /// <param name="polarization">Polarization mode: TE or TM. Default is TE.</param>
        /// <returns>
        /// A tuple containing the Fresnel transmission coefficient (S11) and reflection coefficient (S21).
        /// S11: Forward transmission coefficient.
        /// S21: Forward reflection coefficient.
        /// </returns>
        public static (Complex, Complex) ComputeHalfSMatrix(
            double wavelength,
            Complex n1, Complex n2, double kx,
            InPlanePolMode polarization = InPlanePolMode.TE)
        {
            double k0 = 2.0 * Math.PI / wavelength;
            double nx = kx / k0;

            // eigen info in transmission medium (n2)
            UniformLayer layerLast = new(n: n2);
            (_, _, Complex wLast) = layerLast.ComputeInPlaneModes(
                wavelength: wavelength, nx: nx, mode: polarization);

            // eigen info in incidence medium (n1)
            UniformLayer layer = new(n: n1);
            (_, _, Complex w) = layer.ComputeInPlaneModes(
                wavelength: wavelength, nx: nx, mode: polarization);

            // Fresnel coefficients calculation
            Complex temp = w + wLast;
            Complex s11 = 2.0 * w / temp;
            Complex s21 = (w - wLast) / temp;
            return (s11, s21);
        }

        #endregion
        #region ==== full S-matrix ====

        /// <summary>
        /// Computes the full Fresnel matrix for a single interface between two homogeneous media
        /// with refractive indices <paramref name="n1"/> and <paramref name="n2"/> for a selected polarization mode.
        /// Returns all four S-matrix coefficients: S11, S21, S12, S22.
        /// </summary>
        /// <param name="wavelength">Wavelength in vacuum.</param>
        /// <param name="n1">Complex refractive index on the incidence side.</param>
        /// <param name="n2">Complex refractive index on the transmission side.</param>
        /// <param name="kx">Transverse spatial frequency.</param>
        /// <param name="polarization">Polarization mode: TE or TM. Default is TE.</param>
        /// <returns>
        /// A tuple containing the Fresnel transmission and reflection coefficients:
        /// S11 (forward transmission), S21 (forward reflection), S12 (backward reflection), S22 (backward transmission).
        /// </returns>
        public static (Complex, Complex, Complex, Complex) ComputeFullSMatrix(
            double wavelength,
            Complex n1, Complex n2, double kx,
            InPlanePolMode polarization = InPlanePolMode.TE)
        {
            double k0 = 2.0 * Math.PI / wavelength;
            double nx = kx / k0;

            // eigen info in transmission medium (n2)
            UniformLayer layerLast = new(n: n2);
            (_, _, Complex wLast) = layerLast.ComputeInPlaneModes(
                wavelength: wavelength, nx: nx, mode: polarization);

            // eigen info in incidence medium (n1)
            UniformLayer layer = new(n: n1);
            (_, _, Complex w) = layer.ComputeInPlaneModes(
                wavelength: wavelength, nx: nx, mode: polarization);

            // Fresnel coefficients calculation
            Complex temp = w + wLast;
            Complex s11 = 2.0 * w / temp;
            Complex s21 = (w - wLast) / temp;
            Complex s12 = (wLast - w) / temp;
            Complex s22 = 2.0 * wLast / temp;
            return (s11, s21, s12, s22);
        }

        #endregion

        /// <summary>
        /// computes the Fresnel power ratio for a single interface
        /// between two homogeneonus media with R.I. of n1 and n2
        /// for a selected polarization mode
        /// </summary>
        /// <param name="wavelength"> wavelength in vaccum </param>
        /// <param name="n1"> complex refractive index on the incidence side </param>
        /// <param name="n2"> complex refractive index on the transmission side </param>
        /// <param name="kx"> transverse spatial frequency </param>
        /// <param name="polarization"> polarization mode: TE or TM </param>
        /// <returns> (transmittance, reflectance) </returns>
        public static (double, double) ComputeFresnelRatio(double wavelength,
            Complex n1, Complex n2, double kx,
            InPlanePolMode polarization = InPlanePolMode.TE)
        {
            // call Fresnel matrix
            (Complex s11, Complex s21) = ComputeFresnelMatrix(wavelength,
                n1, n2, kx, polarization);

            // constructs incident, reflected and transmitted plane waves
            PlaneWaveXZ incPW = new(wavelength, n1 * n1, kx, polMode: polarization);
            PlaneWaveXZ traPW = new(wavelength, n2 * n2, kx, polMode: polarization);
            PlaneWaveXZ refPW = new(wavelength, n1 * n1, kx, polMode: polarization, direction: SignFactor.Negative);

            // sets the field coefficients for plane waves 
            incPW.E = 1.0; // set input plane wave amplitude to 1.0
            double incSz = incPW.ComputeSz();
            traPW.E = s11 * incPW.E;
            refPW.E = s21 * incPW.E;
            // reflectance & transmittance
            double T = traPW.ComputeSz() / incSz;
            double R = -refPW.ComputeSz() / incSz;
            return (T, R);
        }

        ///// <summary>
        ///// computes the Fresnel matrix for a single interface
        ///// between two homogeneous media with R.I. of n1 and n2
        ///// for both polarization modes
        ///// </summary>
        ///// <param name="wavelength"> wavelength in vaccum </param>
        ///// <param name="n1"> complex refractive index on the incidence side </param>
        ///// <param name="n2"> complex refractive index on the transmission side </param>
        ///// <param name="kx"> transverse spatial frequency </param>
        ///// <returns> (S11TE, S21TE, S11TM, S21TM) </returns>
        //public static (Complex, Complex, Complex, Complex) ComputeFresnelMatrix(double wavelength,
        //    Complex n1, Complex n2, double kx)
        //{
        //    // eigen info in transmission medium (n2)
        //    (_, Complex wcLast, Complex wdLast) = Eigen.ComputeEigen(wavelength, n2, kx, SignFactor.Positive);

        //    // eigen info in incidence medium (n1)
        //    (_, Complex wc, Complex wd) = Eigen.ComputeEigen(wavelength, n1, kx, SignFactor.Positive);

        //    // Fresnel coefficients calculation
        //    Complex s11TE = 2.0 * wd / (wd + wdLast);
        //    Complex s21TE = (wd - wdLast) / (wd + wdLast);
        //    Complex s11TM = 2.0 * wc / (wc + wcLast);
        //    Complex s21TM = (wc - wcLast) / (wc + wcLast);
        //    return (s11TE, s21TE, s11TM, s21TM);
        //}

        ///// <summary>
        ///// computes the Fresnel power ratio for a single interface
        ///// between two homogeneous media with R.I. of n1 and n2
        ///// for both polarization modes
        ///// </summary>
        ///// <param name="wavelength"> wavelength in vaccum </param>
        ///// <param name="n1"> complex refractive index on the incidence side </param>
        ///// <param name="n2"> complex refractive index on the transmission side </param>
        ///// <param name="kx"> transverse spatial frequency </param>
        ///// <returns> (transmittanceTE, reflectanceTE, transmittanceTM, reflectanceTM) </returns>
        //public static (double, double, double, double) ComputeFresnelRatio(double wavelength,
        //    Complex n1, Complex n2, double kx)
        //{
        //    // call Fresnel matrix
        //    (Complex s11TE, Complex s21TE, Complex s11TM, Complex s21TM) 
        //        = ComputeFresnelMatrix(wavelength, n1, n2, kx);

        //    // constructs plane waves: incidence, reflection and transmission
        //    PlaneWaveXZ incPWTE = new (wavelength, n1 * n1, kx, polMode: InPlanePolMode.TE);
        //    PlaneWaveXZ traPWTE = new (wavelength, n2 * n2, kx, polMode: InPlanePolMode.TE);
        //    PlaneWaveXZ refPWTE = new (wavelength, n1 * n1, kx, polMode: InPlanePolMode.TE, direction: SignFactor.Negative);
        //    PlaneWaveXZ incPWTM = new (wavelength, n1 * n1, kx, polMode: InPlanePolMode.TM);
        //    PlaneWaveXZ traPWTM = new (wavelength, n2 * n2, kx, polMode: InPlanePolMode.TM);
        //    PlaneWaveXZ refPWTM = new (wavelength, n1 * n1, kx, polMode: InPlanePolMode.TM, direction: SignFactor.Negative);

        //    // sets field coefficients for the plane waves
        //    incPWTE.E = 1.0;
        //    double incSzTE = incPWTE.ComputeSz();
        //    traPWTE.E = s11TE * incPWTE.E;
        //    refPWTE.E = s21TE * incPWTE.E;
        //    incPWTM.E = 1.0;
        //    double incSzTM = incPWTM.ComputeSz();
        //    traPWTM.E = s11TM * incPWTM.E;
        //    refPWTM.E = s21TM * incPWTM.E;
        //    // reflectance & transmittance
        //    double transmittanceTE = traPWTE.ComputeSz() / incSzTE;
        //    double reflectanceTE = -refPWTE.ComputeSz() / incSzTE;
        //    double transmittanceTM = traPWTM.ComputeSz() / incSzTM;
        //    double reflectanceTM = -refPWTM.ComputeSz() / incSzTM;
        //    return (transmittanceTE, reflectanceTE, transmittanceTM, reflectanceTM);
        //}

    }
}
