using VEMS.MathCore;

namespace VEMS.EMSolver
{

    /// <summary>
    /// 1D RCWA for in-plane cases
    /// </summary>
    public class RCWA1Dp : RCWA1DBase
    {
        #region properties

        /// <summary>
        /// fixed in-plane polarization mode
        /// </summary>
        public InPlanePolMode Polarization { get; set; }

        /// <summary>
        /// [1,1]-block of the full S-Matrix
        /// i.e. forward in - forward out
        /// for different kx0
        /// </summary>
        public Dictionary<double, MatrixZ>? S11 { get; set; }

        /// <summary>
        /// [2,1]-block of the full S-matrix
        /// i.e. forward in - backward out
        /// for different kx0
        /// </summary>
        public Dictionary<double, MatrixZ>? S21 { get; set; }

        /// <summary>
        /// [1,2]-block of the full S-matrix
        /// i.e. backward in - forward out
        /// for different kx0
        /// </summary>
        public Dictionary<double, MatrixZ>? S12 { get; set; }

        /// <summary>
        /// [2,2]-block of the full S-matrix
        /// i.e. backward in - backward out
        /// for different kx0
        /// </summary>
        public Dictionary<double, MatrixZ>? S22 { get; set; }


        /// <summary>
        /// 
        /// </summary>
        public Dictionary<double, (long, MatrixZ)>? S { get; set; }

        #endregion
        #region constructors

        /// <summary>
        /// constructs a default 1D-RCWA solver
        /// for in-plane case
        /// </summary>
        internal RCWA1Dp() { }

        /// <summary>
        /// constructs a RCWA (1D) solver
        /// for in-plane case
        /// </summary>
        /// <param name="wavelength"> working wavelength given in vacuum </param>
        /// <param name="polarization"> in-plane polarization mode option </param>
        /// <param name="materialFront"> material of the front layer </param>
        /// <param name="mediumMiddle"> medium of the middle layer </param>
        /// <param name="period"> period of the middle layer </param>
        /// <param name="thickness"> thickness of the middle layer </param>
        /// <param name="materialBehind"> material of the behind layer </param>
        public RCWA1Dp(double wavelength,
            InPlanePolMode polarization,
            Material materialFront,
            ILayerMedium mediumMiddle,
            double period, double thickness,
            Material materialBehind)
            : base(wavelength, materialFront, 
                  mediumMiddle, period, thickness, materialBehind)
        {
            // polarization mode
            Polarization = polarization;
            // S-matrices initialization
            S11 = new();
            S21 = new();
            S12 = new();
            S22 = new();
        }

        #endregion
        #region methods

        #region ==== S-matrix ====

        /// <summary>
        /// computes half S-matrix for specific spatial frequencies
        /// and polarization mode for in-plane incidence
        /// </summary>
        /// <param name="kx0"> central spatial frequency along the x-direction </param>
        /// <param name="fieldsSampling"> sampling number for E/H-fields </param>
        /// <param name="mediumSampling"> sampling number for medium </param>
        /// <param name="toeplitztype">Type of Toeplitz matrix (Periodic or Nonperiodic).</param>
        /// <param name="useWSVariation"> whether to use W->S variation or not </param>
        /// <param name="saveLayerMediaData"> whether to save the medium data of the middle layer </param>
        /// <param name="saveLayerModesData"> whether to save the modes data of the middle layer </param>
        public void ComputeHalfSMatrix(double kx0 = 0.0,
            long fieldsSampling = 0,
            long mediumSampling = 0,
            ToeplitzMatrixType toeplitztype = ToeplitzMatrixType.Nonperiodic,
            bool useWSVariation = false,
            bool saveLayerMediaData = true,
            bool saveLayerModesData = false)
        {
            #region exceptions
            if (LayerFront == null) { throw new ArgumentNullException(nameof(LayerFront)); }
            if (LayerMiddle == null) { throw new ArgumentNullException(nameof(LayerMiddle)); }
            if (LayerBehind == null) { throw new ArgumentNullException(nameof(LayerBehind)); }
            if (S11 == null) { throw new ArgumentNullException(nameof(S11)); }
            if (S21 == null) { throw new ArgumentNullException(nameof(S21)); }
            // existing key?
            if (S11.ContainsKey(kx0))
            {
                Printer.Logging($"Key {kx0} already exists in S11 dictionary: will be removed and filled again ...");
                S11.Remove(kx0);
            }
            if (S21.ContainsKey(kx0))
            {
                Printer.Logging($"Key {kx0} already exists in S21 dictionary: will be removed and filled again ...");
                S21.Remove(kx0);
            }
            #endregion

            // spatial frequencies
            if (fieldsSampling == 0) { fieldsSampling = RCWAHelper.DetermineSampling(wavelength: Wavelength, period: LayerMiddle.Period, kSizeFactor: 5.0); }
            if (mediumSampling == 0) { mediumSampling = RCWAHelper.DetermineSampling(period: LayerMiddle.Period, dx: 0.1 * Wavelength); }
            // computation            
            (MatrixZ s11, MatrixZ s21) = SMatrix.HalfSMatrix(wavelength: Wavelength,
                mode: Polarization,
                layerFront: LayerFront,
                layerMiddle: LayerMiddle,
                layerBehind: LayerBehind,
                kx0: kx0,
                fieldsSampling: fieldsSampling,
                mediumSampling: mediumSampling,
                toeplitztype: toeplitztype,
                useWSvariation: useWSVariation,
                saveLayerMediaData: saveLayerMediaData,
                saveLayerModesData: saveLayerModesData);

            // adds to dictionary
            bool addToS11 = S11.TryAdd(key: kx0, value: s11);
            bool addToS21 = S21.TryAdd(key: kx0, value: s21);
            if (addToS11 == false) { Printer.Warning($"Fail to add into S11 @kx: {kx0} ..."); }
            if (addToS21 == false) { Printer.Warning($"Fail to add into S21 @kx: {kx0} ..."); }
        }

        /// <summary>
        /// computes full S-matrix for specific spatial frequencies
        /// and polarization mode for in-plane incidence
        /// </summary>
        /// <param name="kx0"> central spatial frequency along the x-direction </param>
        /// <param name="fieldsSampling"> sampling number for E/H-fields </param>
        /// <param name="mediumSampling"> sampling number for medium </param>
        /// <param name="toeplitztype">Type of Toeplitz matrix (Periodic or Nonperiodic).</param>
        /// <param name="useWSVariation"> whether to use W->S variation or not </param>
        /// <param name="saveLayerMediaData"> whether to save the medium data of the middle layer </param>
        /// <param name="saveLayerModesData"> whether to save the modes data of the middle layer </param>
        public void ComputeFullSMatrix(double kx0 = 0.0,
            long fieldsSampling = 0,
            long mediumSampling = 0,
            ToeplitzMatrixType toeplitztype = ToeplitzMatrixType.Nonperiodic,
            bool useWSVariation = false,
            bool saveLayerMediaData = true,
            bool saveLayerModesData = false)
        {
            #region exceptions
            if (LayerFront == null) { throw new ArgumentNullException(nameof(LayerFront)); }
            if (LayerMiddle == null) { throw new ArgumentNullException(nameof(LayerMiddle)); }
            if (LayerBehind == null) { throw new ArgumentNullException(nameof(LayerBehind)); }
            if (S11 == null) { throw new ArgumentNullException(nameof(S11)); }
            if (S21 == null) { throw new ArgumentNullException(nameof(S21)); }
            if (S12 == null) { throw new ArgumentNullException(nameof(S12)); }
            if (S22 == null) { throw new ArgumentNullException(nameof(S22)); }
            // existing key?
            if (S11.ContainsKey(kx0))
            {
                Printer.Logging($"Key {kx0} already exists in S11 dictionary: will be removed and filled again ...");
                S11.Remove(kx0);
            }
            if (S21.ContainsKey(kx0))
            {
                Printer.Logging($"Key {kx0} already exists in S21 dictionary: will be removed and filled again ...");
                S21.Remove(kx0);
            }
            if (S12.ContainsKey(kx0))
            {
                Printer.Logging($"Key {kx0} already exists in S12 dictionary: will be removed and filled again ...");
                S12.Remove(kx0);
            }
            if (S22.ContainsKey(kx0))
            {
                Printer.Logging($"Key {kx0} already exists in S22 dictionary: will be removed and filled again ...");
                S22.Remove(kx0);
            }
            #endregion

            // spatial frequencies
            if (fieldsSampling == 0) { fieldsSampling = RCWAHelper.DetermineSampling(wavelength: Wavelength, period: LayerMiddle.Period, kSizeFactor: 5.0); }
            if (mediumSampling == 0) { mediumSampling = RCWAHelper.DetermineSampling(period: LayerMiddle.Period, dx: 0.1 * Wavelength); }
            // computation            
            (MatrixZ s11, MatrixZ s21, MatrixZ s12, MatrixZ s22) = SMatrix.FullSMatrix(wavelength: Wavelength,
                mode: Polarization,
                layerFront: LayerFront,
                layerMiddle: LayerMiddle,
                layerBehind: LayerBehind,
                kx0: kx0,
                fieldsSampling: fieldsSampling,
                mediumSampling: mediumSampling,
                toeplitztype: toeplitztype,
                useWSvariation: useWSVariation,
                saveLayerMediaData: saveLayerMediaData,
                saveLayerModesData: saveLayerModesData);

            // adds to dictionary
            bool addToS11 = S11.TryAdd(key: kx0, value: s11);
            bool addToS21 = S21.TryAdd(key: kx0, value: s21);
            bool addToS12 = S12.TryAdd(key: kx0, value: s12);
            bool addToS22 = S22.TryAdd(key: kx0, value: s22);
            if (addToS11 == false) { Printer.Warning($"Fail to add into S11 @kx: {kx0} ..."); }
            if (addToS21 == false) { Printer.Warning($"Fail to add into S21 @kx: {kx0} ..."); }
            if (addToS12 == false) { Printer.Warning($"Fail to add into S12 @kx: {kx0} ..."); }
            if (addToS22 == false) { Printer.Warning($"Fail to add into S22 @kx: {kx0} ..."); }
        }
        #endregion
        #region ==== replication ====

        /// <summary>
        /// replicates the resulting field in the x-domain
        /// periodically according to a multiple number
        /// </summary>
        /// <param name="v"> field values </param>
        /// <param name="g"> uniform sampling grid </param>
        /// <param name="replication"> number of periodic replications </param>
        public void PeriodicReplicate(ref VectorZ v, ref GridInfo1D g,
            long replication = 1)
        {
            if (replication <= 1) { return; }
            v = v.Replicate(targetCount: replication * v.Count);
            g = new(n: g.Count * replication, spacing: g.Spacing);
        }

        #endregion

        #endregion
        #region wrapper methods

        #region ==== plane wave coefficients ====

        /// <summary>
        /// computes the transmission coefficients for a plane wave input
        /// </summary>
        /// <param name="pw"> input plane wave </param>
        /// <returns> (t-coefficients, grid) </returns>
        public (VectorZ, GridInfo1D) ComputeTCoefficients(PlaneWaveXZ pw)
        {
            // availability check
            if (LayerMiddle == null) { throw new ArgumentNullException(nameof(LayerMiddle)); }
            if (S11 == null) { throw new ArgumentNullException(nameof(S11)); }
            if (!S11.ContainsKey(pw.Kx)) { throw new ArgumentOutOfRangeException(nameof(pw.Kx)); }

            // picks up the s-matrix for the given spatial frequency
            MatrixZ s = S11[pw.Kx];
            // defines input coefficients 
            (VectorZ cIn, GridInfo1D g) = RCWAHelper.PlaneWaveToCoefficients(pw,
                period: LayerMiddle.Period, n: s.Rows);           
            // calculates output coefficients
            VectorZ cOut = LinAlg.Dot(s, cIn);

            // return 
            return (cOut, g);
        }

        /// <summary>
        /// computes the reflection coefficients for a plane wave input
        /// </summary>
        /// <param name="pw"> input plane wave </param>
        /// <returns> reflection coefficients </returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public (VectorZ, GridInfo1D) ComputeRCoefficients(PlaneWaveXZ pw)
        {
            // availability check
            if (LayerMiddle == null) { throw new ArgumentNullException(nameof(LayerMiddle)); }
            if (S21 == null) { throw new ArgumentNullException(nameof(S21)); }
            if (!S21.ContainsKey(pw.Kx)) { throw new ArgumentOutOfRangeException(nameof(pw.Kx)); }

            // picks up the s-matrix for the given spatial frequency
            MatrixZ s = S21[pw.Kx];
            // defines input coefficients 
            (VectorZ cIn, GridInfo1D g) = RCWAHelper.PlaneWaveToCoefficients(pw,
                period: LayerMiddle.Period, n: s.Rows);
            // calculates output coefficients
            VectorZ cOut = LinAlg.Dot(s, cIn);

            // return 
            return (cOut, g);
        }

        #endregion
        #region ==== general field ====

        /// <summary>
        /// modulates on a field with the transmission function
        /// using k-domain algorithm
        /// </summary>
        /// <typeparam name="T"> ScalarField1D </typeparam>
        /// <param name="v"> field to be modulated, must be given in k-domain </param>
        /// <param name="order"> selected diffraction order </param>
        /// <param name="fieldsSampling"> sampling number for E/H-fields </param>
        /// <param name="mediumSampling"> sampling number for medium </param>
        /// <param name="isTransmission"> whether output is transmission or reflection </param>
        /// <param name="isZeroCentered"> whether to center the output field around zero </param>
        /// <param name="intrpl"> interpolation method used in the k-domain </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        public void Modulate<T>(ref ScalarField1D v, int order,
            long fieldsSampling = 0,
            long mediumSampling = 0,
            bool isTransmission = true,
            bool isZeroCentered = false,
            InterpolationMethod intrpl = InterpolationMethod.Linear,
            LoopMode loopMode = Defaults.LoopOption) where T : ScalarField1D
        {
            // null case handling
            if (v.Field == null) { throw new ArgumentNullException($"{nameof(v.Field)}"); }

            var s = isTransmission ? S11 : S21;
            for (long i = 0; i < v.Grid.Count; i++)
            {
                double kx = v.Grid[i];
                if (!s.ContainsKey(kx))
                { ComputeHalfSMatrix(kx0: kx, fieldsSampling, mediumSampling); }

                // central index in the Fourier series
                long ctrIdx = (s[kx].Rows - 1) / 2;
                VectorZ cIn = new(count: s[kx].Rows, mode: ArrayInitMode.Calloc);
                cIn[ctrIdx, false] = 1.0;
                VectorZ cOut = LinAlg.Dot(s[kx], cIn);

                // gets the coefficients for the selected order
                v.Field[i, false] *= cOut[ctrIdx + order, false];
            }

            // output center option?
            if (isZeroCentered)
            {
                // makes a new grid that is shifted in k-domain
                GridInfo1D gi = new(other: v.Grid);
                gi.GetModified(ctrShift: order * 2.0 * Math.PI / LayerMiddle.Period);
                // samples the current field back to centered coordinate by interpolation
                Grid1DCplxInterpolation itp = new(v: v.Field, grid: gi,
                    method: intrpl,
                    bound: DataBoundary.ConstantZero);
                v.Field = itp.Evaluate(targetGrid: v.Grid,
                    loopMode: loopMode);
            }
        }

        #endregion
        // ...

        #endregion

    }

}
