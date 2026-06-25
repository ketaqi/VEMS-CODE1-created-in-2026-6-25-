using VEMS.MathCore;

namespace VEMS.EMSolver
{
    /// <summary>
    /// 1D-RCWA class
    /// </summary>
    [Obsolete]
    public class RCWA1D
    {
        #region properties

        /// <summary>
        /// wavelength in vacuum
        /// </summary>
        public double Wavelength { get; set; }

        /// <summary>
        /// in-plane polarization mode option
        /// </summary>
        public InPlanePolMode Polarization { get; set; }

        /// <summary>
        /// uniform layer in front 
        /// </summary>
        public UniformLayer? LayerFront { get; set; }

        /// <summary>
        /// 1D-periodic layer in the middle
        /// </summary>
        public Periodic1DLayer? LayerMiddle { get; set; }

        // for future development
        //public List<Periodic1DLayer> LayersMiddle { get; set; }

        /// <summary>
        /// uniform layer behind
        /// </summary>
        public UniformLayer? LayerBehind { get; set; }

        /// <summary>
        /// oversampling factor for the layer 
        /// in the middle
        /// </summary>
        public double LayerOverSamp { get; set; }

        // for future development
        //public List<double> LayersOverSamp { get; set; }

        /// <summary>
        /// central spatial frequency along the x-direction
        /// </summary>
        public double Kx0 { get; set; }

        /// <summary>
        /// number of sampling for E/H-fields
        /// </summary>
        public long FieldsSampling { get; set; }

        /// <summary>
        /// number of sampling for medium (epsilon/mu)
        /// </summary>
        public long MediumSampling { get; set; }

        /// <summary>
        /// whether to use the W=>S variation
        /// in the S-matrix calculation
        /// </summary>
        public bool UseWSVariation { get; set; }

        /// <summary>
        /// whether to save the media data
        /// of the middle layer
        /// </summary>
        public bool SaveLayerMediaData { get; set; }

        /// <summary>
        /// whether to save the modes data
        /// of the middle layer
        /// </summary>
        public bool SaveLayerModesData { get; set; }

        /// <summary>
        /// [1,1]-block of the full S-Matrix
        /// forward in - forward out
        /// </summary>
        public MatrixZ? S11 { get; set; }

        /// <summary>
        /// [2,1]-block of the full S-matrix
        /// forward in - backward out
        /// </summary>
        public MatrixZ? S21 { get; set; }

        /// <summary>
        /// [1,2]-block of the full S-matrix
        /// backward in - forward out
        /// </summary>
        public MatrixZ? S12 { get; set; }

        /// <summary>
        /// [2,2]-block of the full S-matrix
        /// backward in - backward out
        /// </summary>
        public MatrixZ? S22 { get; set; }


        // s-matrix storage for different (wavelength, kx0) cases 
        internal Dictionary<(double, double), MatrixZ>? S11s { get; set; }
        internal Dictionary<(double, double), MatrixZ>? S21s { get; set; }
        internal Dictionary<(double, double), MatrixZ>? S12s { get; set; }
        internal Dictionary<(double, double), MatrixZ>? S22s { get; set; }



        #endregion
        #region constructors

        /// <summary>
        /// constructs a default 1D-RCWA solver
        /// </summary>
        public RCWA1D() { }

        /// <summary>
        /// constructs a RCWA (1D) solver
        /// </summary>
        /// <param name="materialFront"> material of the front layer </param>
        /// <param name="mediumMiddle"> medium of the middle layer </param>
        /// <param name="period"> period of the middle layer </param>
        /// <param name="thickness"> thickness of the middle layer </param>
        /// <param name="materialBehind"> material of the behind layer </param>
        public RCWA1D(Material materialFront,
            Layer1DMedium mediumMiddle,
            double period, double thickness,
            Material materialBehind)
        {
            // layers
            LayerFront = new(m: materialFront);
            LayerMiddle = new(period: period, medium: mediumMiddle, thickness: thickness);
            LayerBehind = new(m: materialBehind);
        }

        #endregion
        #region methods

        #region ==== S-matrix ====

        /// <summary>
        /// computes half S-matrix for specific spatial frequencies
        /// and polarization mode for in-plane incidence
        /// </summary>
        /// <param name="wavelength"> vacuum wavelength </param>
        /// <param name="mode"> polarization mode option </param>
        /// <param name="kx0"> central spatial frequency along the x-direction </param>
        /// <param name="fieldsSampling"> sampling number for E/H-fields </param>
        /// <param name="mediumSampling"> sampling number for medium </param>
        /// <param name="useWSVariation"> whether to use W->S variation or not </param>
        /// <param name="saveLayerMediaData"> whether to save the medium data of the middle layer </param>
        /// <param name="saveLayerModesData"> whether to save the modes data of the middle layer </param>
        public void ComputeHalfSMatrix(double wavelength,
            InPlanePolMode mode = InPlanePolMode.TE,
            double kx0 = 0.0,
            long fieldsSampling = 0,
            long mediumSampling = 0,
            bool useWSVariation = false,
            bool saveLayerMediaData = true,
            bool saveLayerModesData = false)
        {
            // exception
            if (LayerFront == null || LayerMiddle == null || LayerBehind == null)
                throw new ArgumentNullException($"{nameof(LayerFront)} or {nameof(LayerMiddle)} or {nameof(LayerBehind)}");
            // solvers parameters
            Wavelength = wavelength;
            Polarization = mode;
            // spatial frequencies
            Kx0 = kx0;
            FieldsSampling = (fieldsSampling == 0) ?
                RCWAHelper.DetermineSampling(wavelength: wavelength, period: LayerMiddle.Period, kSizeFactor: 5.0) : fieldsSampling;
            MediumSampling = (mediumSampling == 0) ?
                RCWAHelper.DetermineSampling(period: LayerMiddle.Period, dx: 0.1 * wavelength) : mediumSampling;
            // other parameters
            UseWSVariation = useWSVariation;
            // cache option ...
            SaveLayerMediaData = saveLayerMediaData;
            SaveLayerModesData = saveLayerModesData;
            // computation            
            (S11, S21) = SMatrix.HalfSMatrix(layerFront: LayerFront,
                layerMiddle: LayerMiddle,
                layerBehind: LayerBehind,
                wavelength: Wavelength,
                mode: Polarization,
                kx0: Kx0,
                fieldsSampling: FieldsSampling,
                mediumSampling: MediumSampling,
                useWSvariation: UseWSVariation,
                saveLayerMediaData: SaveLayerMediaData,
                saveLayerModesData: SaveLayerModesData);
        }

        // conical case ...

        #endregion
        #region ==== coefficients ====

        /// <summary>
        /// computes mode coefficients for a general incidence
        /// [must be used after half s-matrix call]
        /// </summary>
        /// <param name="vIn"> incident field </param>
        /// <param name="isTransmission"> whether to compute transmission or reflection </param>
        /// <param name="interp"> interpolation method option </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> resulting coefficients (transmission or reflection) </returns>
        /// <exception cref="ArgumentNullException"></exception>
        public VectorZ ComputeCoefficients(ScalarField1D vIn,
            bool isTransmission = true,
            InterpolationMethod interp = Defaults.IntrplOption,
            LoopMode loopMode = Defaults.LoopOption)
        {
            // exception handling
            if (LayerMiddle == null || S11 == null || S21 == null)
            { throw new ArgumentNullException(); }
            // input handling
            vIn.SwitchToKDomain();
            VectorZ cIn = (vIn.Field == null)? VectorZ.Empty : new(other: vIn.Field, deepCopy: true);
            // input interpolation in k-domain => sampling matching
            double dKx = 2.0 * Math.PI / LayerMiddle.Period;
            GridInfo1D gTarget = new(n: FieldsSampling,
                spacing: dKx,
                start: Kx0 - (FieldsSampling - 1) / 2 * dKx);
            Grid1DCplxInterpolation itpl = new(v: cIn, grid: vIn.Grid, method: interp);
            cIn = itpl.Evaluate(targetGrid: gTarget, loopMode: loopMode);
            // input => output
            VectorZ cOut = (isTransmission) ?
                LinAlg.Dot(S11, cIn) : LinAlg.Dot(S21, cIn);
            // return
            return cOut;
        }

        /// <summary>
        /// computes mode coefficients for a plane wave incidence
        /// [must be used after half s-matrix call]
        /// </summary>
        /// <param name="pIn"> incident plane wave </param>
        /// <param name="isTransmission"> whether to compute transmission or reflection </param>
        /// <returns> resulting coefficients (transmission or reflection) </returns>
        /// <exception cref="ArgumentNullException"></exception>
        public VectorZ ComputeCoefficients(PlaneWaveXZ pIn,
            bool isTransmission = true)
        {
            // exception handling
            if (S11 == null || S21 == null || LayerMiddle == null)
            { throw new ArgumentNullException(); }
            // input => output
            VectorZ cIn = new(count: FieldsSampling, initVal: 0.0);
            // plane wave amplitude scaling according to the period
            double scal = LayerMiddle.Period / Math.Sqrt(2.0 * Math.PI);
            cIn[(FieldsSampling - 1) / 2, false] = scal * pIn.E;
            VectorZ cOut = (isTransmission) ?
                LinAlg.Dot(S11, cIn) : LinAlg.Dot(S21, cIn);
            // return
            return cOut;
        }

        #endregion
        #region ==== conversion ====

        /// <summary>
        /// converts coefficients to scalar field
        /// </summary>
        /// <param name="c"> mode coefficients </param>
        /// <param name="isTransmission"> whether transmission or reflection </param>
        /// <returns> result scalar fiels </returns>
        /// <exception cref="ArgumentNullException"></exception>
        public ScalarField1D CoefficientsToField(VectorZ c,
            bool isTransmission = true)
        {
            // exception handling
            if (LayerMiddle == null || LayerBehind == null || LayerFront == null)
            { throw new ArgumentNullException(); }
            
            // defines uniform grid in k-domain
            double dKx = 2.0 * Math.PI / LayerMiddle.Period;
            GridInfo1D g = new(n: FieldsSampling, spacing: dKx,
                start: Kx0 - (FieldsSampling - 1) / 2 * dKx);

            // constructs field
            UniformLayer outputLayer = isTransmission ? LayerBehind : LayerFront;
            ScalarField1D vOut = new(wavelength: Wavelength,
                epsilon: outputLayer.Epsilon(Wavelength),
                mu: outputLayer.Mu(Wavelength), 
                grid: g, fieldValues: c,
                domain: ModelingDomain.SpatialFrequency,
                direction: isTransmission? SignFactor.Positive : SignFactor.Negative);
            
            // return
            return vOut;
        }

        #endregion
        #region ==== replication ====

        /// <summary>
        /// replicates result field in the x-domain
        /// periodically according to a multiple number
        /// </summary>
        /// <param name="v"> field values </param>
        /// <param name="g"> uniform sampling grid </param>
        /// <param name="replication"> number of periodic replication </param>
        public void PeriodicReplicate(ref VectorZ v, ref GridInfo1D g,
            long replication = 1)
        {
            if (replication <= 1) { return; }
            v = v.Replicate(replication * v.Count);
            g = new(n: g.Count * replication, spacing: g.Spacing);
        }

        #endregion

        #endregion
    }


    
    
    // conical?
    internal class RCWA1Dc
    {

    }
}
