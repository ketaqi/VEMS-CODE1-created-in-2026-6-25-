using System.Numerics;
using VEMS.MathCore;

namespace VEMS.EMSolver
{

    /// <summary>
    /// multilayer (e.g. coating) class
    /// </summary>
    public class MultiLayer
    {

        #region properties

        /// <summary>
        /// number of layers
        /// </summary>
        public int Layers { get; set; }

        // Epsilon
        // Mu

        /// <summary>
        /// list of refractive indices for all the layers
        /// </summary>
        public List<Func<double, Complex>> RefractiveIndex { get; set; }

        /// <summary>
        /// list of thicknesses for all the layers
        /// </summary>
        public List<double> Thickness { get; set; }

        ///// <summary>
        ///// starting angle [rad] for derivative-related analysis
        ///// </summary>
        //public double AlphaStart { get; set; }

        ///// <summary>
        ///// end angle [rad] for derivative-related analysis
        ///// </summary>
        //public double AlphaEnd { get; set; }

        /// <summary>
        /// spatial frequencies used for derivative-related analysis
        /// </summary>
        public VectorD? Kx { get; set; }

        /// <summary>
        /// s-matrix coefficients, either for transmission or reflection
        /// for the TE polarization mode
        /// </summary>
        public VectorZ? STE { get; set; }

        /// <summary>
        /// s-matrix coefficients, either for transmission or reflection
        /// for the TM polarization mode
        /// </summary>
        public VectorZ? STM { get; set; }

        /// <summary>
        /// smooth/unwrapped phase of the s-matrix coefficients
        /// for the TE polarization mode
        /// </summary>
        public ClampedBSpline1D? STEPhase { get; set; }

        /// <summary>
        /// smooth/unwrapped phase of the s-matrix coefficients
        /// for the TM polarization mode
        /// </summary>
        public ClampedBSpline1D? STMPhase { get; set; }

        #endregion
        #region constructors

        /// <summary>
        /// default constructor
        /// </summary>
        public MultiLayer()
        {
            Layers = 0;
            RefractiveIndex = new List<Func<double, Complex>>();
            Thickness = new List<double>();
        }

        /// <summary>
        /// constructs a multilayer coating 
        /// </summary>
        /// <param name="layers"></param>
        /// <param name="refractiveIndex"></param>
        /// <param name="thickness"></param>
        public MultiLayer(int layers, 
            List<Func<double, Complex>> refractiveIndex, 
            List<double> thickness)
        {
            Layers = layers;
            RefractiveIndex = refractiveIndex;
            Thickness = thickness;
        }

        /// <summary>
        /// constructs a multilayer coating by copying from another
        /// </summary>
        /// <param name="other"> source to copy </param>
        public MultiLayer(MultiLayer other)
        {
            Layers = other.Layers;
            RefractiveIndex = new List<Func<double, Complex>>();
            Thickness = new List<double>();
            for (int i = 0; i < Layers; i++)
            {
                RefractiveIndex.Add(other.RefractiveIndex[i]);
                Thickness.Add(other.Thickness[i]);
            }
        }

        #endregion
        #region methods

        #region ---- structure modification ----

        /// <summary>
        /// adds a layer into a multilayer coating at the end
        /// </summary>
        /// <param name="refractiveIndex"> refractive index n = n(lambda) of the layer </param>
        /// <param name="thickness"> thickness of the layer </param>
        /// <param name="replicas"> number of replicas </param>
        public void AddLayer(Func<double, Complex> refractiveIndex, double thickness,
            int replicas = 1)
        {
            if (RefractiveIndex == null || Thickness == null) { return; }
            for (int i = 0; i < replicas; i++)
            {
                RefractiveIndex.Add(refractiveIndex);
                Thickness.Add(thickness);
                Layers += 1;
            }
        }

        /// <summary>
        /// adds altering two layers into a multilayer coating
        /// </summary>
        /// <param name="n1"> refractive index n1(lambda) of the first layer </param>
        /// <param name="t1"> thickness t1 of the first layer </param>
        /// <param name="n2"> refractive index n2(lambda) of the second layer</param>
        /// <param name="t2"> thickness t2 of the secondlayer </param>
        /// <param name="replicas"> number of replicas </param>
        public void AddAlteringLayers(Func<double, Complex> n1, double t1,
            Func<double, Complex> n2, double t2,
            int replicas = 1)
        {
            if (RefractiveIndex == null || Thickness == null) { return; }
            for (int i = 0; i < replicas; i++)
            {
                AddLayer(n1, t1);
                AddLayer(n2, t2);
            }
        }

        /// <summary>
        /// inserts a layer into a multilayer coating at specific index 
        /// </summary>
        /// <param name="index"> index in the original coating </param>
        /// <param name="refractiveIndex"> refractive index n = n(lambda) of the layer </param>
        /// <param name="thickness"> thickness of the layer </param>
        public void InsertLayer(int index,
            Func<double, Complex> refractiveIndex, double thickness)
        {
            if (RefractiveIndex == null || Thickness == null) { return; }
            RefractiveIndex.Insert(index, refractiveIndex);
            Thickness.Insert(index, thickness);
            Layers += 1;
        }

        /// <summary>
        /// removes the layer at specific index from a multilayer coating
        /// </summary>
        /// <param name="index"> index in the original coating </param>
        public void RemoveLayer(int index)
        {
            if (RefractiveIndex == null || Thickness == null) { return; }
            if (index < 0 || index >= Layers) { throw new ArgumentOutOfRangeException("index"); }
            RefractiveIndex.RemoveAt(index);
            Thickness.RemoveAt(index);
            Layers -= 1;
        }

        #endregion
        #region ---- s-matrix calculations ----

        /// <summary>
        /// calculates the half s-matrix for selected
        /// wavelength, spatial frequency and polarization mode
        /// </summary>
        /// <param name="wavelength"> wavelength in vacuum </param>
        /// <param name="nFront"> refractive index of the medium in front </param>
        /// <param name="nBehind"> refractive index of the medium behind </param>
        /// <param name="kx"> spatial frequency </param>
        /// <param name="polarization"> polarization mode </param>
        /// <returns> (S11, S21) i.e. transmission and reflection coefficients </returns>
        public (Complex, Complex) ComputeHalfSMatrix(double wavelength,
            Func<double, Complex> nFront,
            Func<double, Complex> nBehind,
            double kx,
            InPlanePolMode polarization = InPlanePolMode.TE)
        {
            // initializes (refractive index for given wavelength)
            List<Complex> nLayers = new();
            List<double> tLayers = new();
            for (int i = 0; i < Layers; i++)
            {
                nLayers.Add(RefractiveIndex[i].Invoke(wavelength));
                tLayers.Add(Thickness[i]);
            }
            // includes medium behind
            nLayers.Add(nBehind.Invoke(wavelength));
            tLayers.Add(0.0);
            // includes medium in front
            nLayers.Insert(0, nFront.Invoke(wavelength));
            tLayers.Insert(0, 0.0);

            // calls half SMatrix loop
            return CoatingMatrix.HalfSMatrixLoop(wavelength,
                nLayers, tLayers, kx, polarization);
        }

        /// <summary>
        /// calculates the half s-matrix for selected
        /// wavelength, spatial frequencies and polarization mode
        /// [vectorized via VMath methods]
        /// </summary>
        /// <param name="wavelength"> wavelength in vacuum </param>
        /// <param name="nFront"> refractive index of the medium in front </param>
        /// <param name="nBehind"> refractive index of the medium behind </param>
        /// <param name="kx"> spatial frequencies </param>
        /// <param name="polarization"> polarization mode </param>
        /// <returns> (S11, S21) i.e. transmission and reflection coefficients </returns>
        public (VectorZ, VectorZ) ComputeHalfSMatrix(double wavelength,
            Func<double, Complex> nFront,
            Func<double, Complex> nBehind,
            VectorD kx,
            InPlanePolMode polarization = InPlanePolMode.TE)
        {
            // initializes (refractive index for given wavelength)
            List<Complex> nLayers = new();
            List<double> tLayers = new();
            for (int i = 0; i < Layers; i++)
            {
                nLayers.Add(RefractiveIndex[i].Invoke(wavelength));
                tLayers.Add(Thickness[i]);
            }
            // includes medium behind
            nLayers.Add(nBehind.Invoke(wavelength));
            tLayers.Add(0.0);
            // includes medium in front
            nLayers.Insert(0, nFront.Invoke(wavelength));
            tLayers.Insert(0, 0.0);

            // calls half SMatrix loop
            return CoatingMatrix.HalfSMatrixLoop(wavelength, nLayers, tLayers, kx, polarization);
        }

        /// <summary>
        /// calculates the full s-matrix for selected
        /// wavelength, spatial frequency and polarization mode
        /// </summary>
        /// <param name="wavelength"> wavelength in vacuum </param>
        /// <param name="nFront"> refractive index of the medium in front </param>
        /// <param name="nBehind"> refractive index of the medium behind </param>
        /// <param name="kx"> spatial frequency </param>
        /// <param name="polarization"> polarization mode </param>
        /// <returns> (S11, S21, S12, S22) i.e. transmission and reflection coefficients </returns>
        public (Complex, Complex, Complex, Complex) ComputeFullSMatrix(double wavelength,
            Func<double, Complex> nFront,
            Func<double, Complex> nBehind,
            double kx,
            InPlanePolMode polarization = InPlanePolMode.TE)
        {
            // initializes (refractive index for given wavelength)
            List<Complex> nLayers = new();
            List<double> tLayers = new();
            for (int i = 0; i < Layers; i++)
            {
                nLayers.Add(RefractiveIndex[i].Invoke(wavelength));
                tLayers.Add(Thickness[i]);
            }
            // includes medium behind
            nLayers.Add(nBehind.Invoke(wavelength));
            tLayers.Add(0.0);
            // includes medium in front
            nLayers.Insert(0, nFront.Invoke(wavelength));
            tLayers.Insert(0, 0.0);

            // calls full SMatrix loop
            return CoatingMatrix.FullSMatrixLoop(wavelength,
                nLayers, tLayers, kx, polarization);
        }

        /// <summary>
        /// calculates the full s-matrix for selected
        /// wavelength, spatial frequencies and polarization mode
        /// [vectorized via VMath methods]
        /// </summary>
        /// <param name="wavelength"> wavelength in vacuum </param>
        /// <param name="nFront"> refractive index of the medium in front </param>
        /// <param name="nBehind"> refractive index of the medium behind </param>
        /// <param name="kx"> spatial frequencies </param>
        /// <param name="polarization"> polarization mode </param>
        /// <returns> (S11, S21, S12, S22) i.e. transmission and reflection coefficients </returns>
        public (VectorZ, VectorZ, VectorZ, VectorZ) ComputeFullSMatrix(double wavelength,
            Func<double, Complex> nFront,
            Func<double, Complex> nBehind,
            VectorD kx,
            InPlanePolMode polarization = InPlanePolMode.TE)
        {
            // initializes (refractive index for given wavelength)
            List<Complex> nLayers = new();
            List<double> tLayers = new();
            for (int i = 0; i < Layers; i++)
            {
                nLayers.Add(RefractiveIndex[i].Invoke(wavelength));
                tLayers.Add(Thickness[i]);
            }
            // includes medium behind
            nLayers.Add(nBehind.Invoke(wavelength));
            tLayers.Add(0.0);
            // includes medium in front
            nLayers.Insert(0, nFront.Invoke(wavelength));
            tLayers.Insert(0, 0.0);

            // calls full SMatrix loop
            return CoatingMatrix.FullSMatrixLoop(wavelength, nLayers, tLayers, kx, polarization);
        }
        #endregion
        #region ---- lateral shift & etc ----

        /// <summary>
        /// computes the smooth/unwrapped phase of the s-matrix coefficient
        /// for both TE and TM polarization modes
        /// </summary>
        /// <param name="wavelength"> wavelength in vacuum </param>
        /// <param name="nFront"> refractive index of the medium in front </param>
        /// <param name="nBehind"> refractive index of the medium behind </param>
        /// <param name="alphaStart"> start of the angle range under investigation </param>
        /// <param name="alphaEnd"> end of the angle range under investigation </param>
        /// <param name="isReflection"> whether to consider the reflection or not (i.e. transmission) </param>
        /// <param name="nKxs"> number of spatial frequencies for s-matrix coefficients sampling </param>
        /// <param name="fitDegree"> degree of spline fitting </param>
        /// <param name="fitNFac"> n-factor of spline fitting </param>
        public void ComputeSPhase(double wavelength,
            Func<double, Complex> nFront,
            Func<double, Complex> nBehind,
            double alphaStart, double alphaEnd,
            bool isReflection = false,
            long nKxs = 101, long fitDegree = 3, double fitNFac = 0.7)
        {
            // from angle to spatial frequency
            double kxStart = Angle2SpatialFrequency(wavelength: wavelength, n: nFront, alpha: alphaStart);
            double kxEnd = Angle2SpatialFrequency(wavelength: wavelength, n: nBehind, alpha: alphaEnd);
            double dKx = (kxEnd - kxStart) / (nKxs - 1);
            Kx = new(count: nKxs, initVal: kxStart, increment: dKx);
            // computes s-matrix coefficients, for either transmission or reflection
            if(isReflection)
            {
                (_, STE) = ComputeHalfSMatrix(wavelength, nFront, nBehind, Kx, InPlanePolMode.TE);
                (_, STM) = ComputeHalfSMatrix(wavelength, nFront, nBehind, Kx, InPlanePolMode.TM);
            }
            else
            {
                (STE, _) = ComputeHalfSMatrix(wavelength, nFront, nBehind, Kx, InPlanePolMode.TE);
                (STM, _) = ComputeHalfSMatrix(wavelength, nFront, nBehind, Kx, InPlanePolMode.TM);
            }
            // unwraps phase from the s-matrix coefficients and fits it
            STEPhase = new(grid: new GridInfo1D(nKxs, kxStart, dKx),
                values: VMath.UnwrapPhase(STE), 
                degree: fitDegree, nFactor: fitNFac);
            STMPhase = new(grid: new GridInfo1D(nKxs, kxStart, dKx),
                values: VMath.UnwrapPhase(STM),
                degree: fitDegree, nFactor: fitNFac);
        }

        /// <summary>
        /// computes the coating-induced effects
        /// including lateral shift and phase/OPL change
        /// </summary>
        /// <param name="kx"> transverse spatial frequency kx </param>
        /// <param name="polMode"> polarization mode (TE or TM) </param>
        /// <returns> (dx-lateral shift, dPsi-phase change) </returns>
        public (double, double) ComputesLateralShiftEffects(double kx, 
            InPlanePolMode polMode = InPlanePolMode.TE)
        {
            if(Kx == null) { throw new ArgumentNullException("Kx"); }
            if(kx < Kx[0] || kx > Kx[Kx.Count - 1]) { return (0.0, 0.0); }
            // polarization mode switch
            switch(polMode)
            {
                case InPlanePolMode.TE:
                    {
                        if (STEPhase == null) { throw new ArgumentNullException("STEPhase"); }
                        double xShift = -STEPhase.Derive(new VectorD(1, kx))[0];
                        double psiChange = kx * xShift;
                        return (xShift, psiChange);
                    }
                case InPlanePolMode.TM:
                    {
                        if (STMPhase == null) { throw new ArgumentNullException("STMPhase"); }
                        double xShift = -STMPhase.Derive(new VectorD(1, kx))[0];
                        double psiChange = kx * xShift;
                        return (xShift, psiChange);
                    }
                default: goto case InPlanePolMode.TE;
            }
        }

        /// <summary>
        /// computes the coating-induced effects
        /// including lateral shift and phase/OPL change
        /// for all spatial frequencies used in the s-matrix calculation
        /// </summary>
        /// <param name="polMode"> polarization mode (TE or TM) </param>
        /// <returns> (dx-lateral shift, dPsi-phase change) </returns>
        public (VectorD, VectorD) ComputeLateralShiftEffects(InPlanePolMode polMode = InPlanePolMode.TE)
        {
            if (Kx == null) { throw new ArgumentNullException("Kx"); }
            // polarization modes switch
            switch(polMode)
            {
                case InPlanePolMode.TE:
                    {
                        if (STEPhase == null) { throw new ArgumentNullException("STEPhase"); }
                        VectorD xShift = STEPhase.Derive(Kx);
                        VectorD psiChange = VMath.Mul(Kx, xShift);
                        return (xShift, psiChange);
                    }
                case InPlanePolMode.TM:
                    {
                        if (STMPhase == null) { throw new ArgumentNullException("STMPhase"); }
                        VectorD xShift = STMPhase.Derive(Kx);
                        VectorD psiChange = VMath.Mul(Kx, xShift);
                        return (xShift, psiChange);
                    }
                default: goto case InPlanePolMode.TE;
            }
        }

        #endregion
        #region ---- static helpers ----

        /// <summary>
        /// calculates power ratio from selected s-matrix coefficient
        /// e.g. the transmission or the reflection coefficient
        /// </summary>
        /// <param name="wavelength"> wavelength in vacuum </param>
        /// <param name="nIn"> refractive index of the input medium </param>
        /// <param name="nOut"> refractive index of the output medium </param>
        /// <param name="kx"> spatial frequency kx </param>
        /// <param name="s"> selected complex coefficient from s-matrix </param>
        /// <param name="polarization"> polarization mode </param>
        /// <param name="outputIsReflection"> if the output is reflection </param>
        /// <returns> power ratio e.g. transmittance of reflectance </returns>
        public static double ComputePowerRatio(double wavelength,
            Func<double, Complex> nIn,
            Func<double, Complex> nOut,
            double kx, Complex s,
            InPlanePolMode polarization = InPlanePolMode.TE,
            bool outputIsReflection = false)
        {
            // constructs plane waves
            PlaneWaveXZ input = new(wavelength: wavelength,
                n: nIn.Invoke(wavelength),
                kx: kx, polMode: polarization);
            SignFactor d = outputIsReflection ? SignFactor.Negative : SignFactor.Positive;
            PlaneWaveXZ output = new(wavelength: wavelength,
                n: nOut.Invoke(wavelength),
                kx: kx, polMode: polarization, direction: d);

            // sets field coefficients for plane waves 
            input.E = 1.0; // set input plane wave amplitude to 1.0
            output.E = s * input.E; // output calculated via s-matrix coefficient
            // computes Poynting vector z-component
            double szIn = input.ComputeSz();
            double szOut = output.ComputeSz();
            // computes power ratio
            return Math.Abs(szOut / szIn);
        }

        /// <summary>
        /// converts from angle to spatial frequency
        /// [assuming real-valued refractive index]
        /// </summary>
        /// <param name="wavelength"> wavelength in vacuum </param>
        /// <param name="n"> refractive index n = n(wavelength) </param>
        /// <param name="alpha"> angle in radian </param>
        /// <returns> corresponding transverse spatial frequency </returns>
        public static double Angle2SpatialFrequency(double wavelength,
            Func<double, Complex> n, double alpha)
        {
            double k0 = 2.0 * Math.PI / wavelength;
            double k = n(wavelength).Real * k0;
            return Math.Sin(alpha) * k;
        }

        /// <summary>
        /// converts from spatial frequency to angle
        /// </summary>
        /// <param name="wavelength"> wavelength in vacuum </param>
        /// <param name="n"> refractive index n = n(wavelength) </param>
        /// <param name="kx"> transverse spatial frequency </param>
        /// <returns> corresponding angle </returns>
        public static double SpatialFrequency2Angle(double wavelength,
            Func<double, Complex> n, double kx)
        {
            double k0 = 2.0 * Math.PI / wavelength;
            double k = n(wavelength).Real * k0;
            return Math.Asin(kx / k);
        }

        /// <summary>
        /// Converts the s-matrix coefficients from TE/TM polarization
        /// to the X-Y polarization via coordinate transform 
        /// </summary>
        /// <param name="kx"> spatial frequency kx in the X-Y system </param>
        /// <param name="ky"> spatial frequency ky in the X-Y system </param>
        /// <param name="sTE"> s-matrix coefficients for TE polarization </param>
        /// <param name="sTM"> s-matrix coefficients for Tm polarization </param>
        /// <returns> coefficients (xx, xy=yx, yy) for X-Y polarization </returns>
        public static (Complex, Complex, Complex) TETM2XY(
            double kx, double ky,
            Complex sTE, Complex sTM)
        {
            Complex sXX, sXY, sYY;
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
            return (sXX, sXY, sYY);
        }

        #endregion

        #endregion

    }

}
