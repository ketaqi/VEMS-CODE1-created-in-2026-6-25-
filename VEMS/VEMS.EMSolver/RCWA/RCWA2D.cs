using VEMS.MathCore;
using Complex = System.Numerics.Complex;

namespace VEMS.EMSolver
{
    /// <summary>
    /// 2D-RCWA class
    /// </summary>
    public class RCWA2D
    {

        #region properties

        /// <summary>
        /// wavelength in vacuum
        /// </summary>
        public double Wavelength { get; set; }

        /// <summary>
        /// uniform layer in front 
        /// </summary>
        public UniformLayer? LayerFront { get; set; }

        /// <summary>
        /// 2D-periodic layer in the middle
        /// </summary>
        public Periodic2DLayer? LayerMiddle { get; set; }

        // for future development
        //public List<Periodic2DLayer> LayersMiddle { get; set; }

        /// <summary>
        /// uniform layer behind
        /// </summary>
        public UniformLayer? LayerBehind { get; set; }

        ///// <summary>
        ///// in-plane polarization mode option
        ///// </summary>
        //public InPlanePolMode PolMode { get; set; }

        /// <summary>
        /// central spatial frequency along the x-direction
        /// </summary>
        public double Kx0 { get; set; }

        /// <summary>
        /// central spatial frequency along the y-direction
        /// </summary>
        public double Ky0 { get; set; }

        /// <summary>
        /// number of spatial frequencies along x-direction
        /// for E/H-field decomposition
        /// </summary>
        public long NKxs { get; set; }

        /// <summary>
        /// number of spatial frequencies along y-direction
        /// for E/H-field decomposition
        /// </summary>
        public long NKys { get; set; }

        /// <summary>
        /// oversampling factor for the layer 
        /// in the middle along x-direction
        /// </summary>
        public double LayerOverSampX { get; set; }

        /// <summary>
        /// oversampling factor for the layer 
        /// in the middle along y-direction
        /// </summary>
        public double LayerOverSampY { get; set; }


        /// <summary>
        /// number of sampling for E/H fields along x direction
        /// </summary>
        public long FieldsSamplingX { get; set; }

        /// <summary>
        /// number of sampling for E/H fields along y direction
        /// </summary>
        public long FieldsSamplingY { get; set; }

        /// <summary>
        /// number of sampling for medium along x direction
        /// </summary>
        public long MediumSamplingX { get; set; }

        /// <summary>
        /// number of sampling for medium along y direction
        /// </summary>
        public long MediumSamplingY { get; set; }

        // for future development
        //public List<double> LayersOverSampX { get; set; }
        //public List<double> LayersOverSampY { get; set; }

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

        #endregion
        #region constructors

        /// <summary>
        /// constructs a default 2D-RCWA solver
        /// </summary>
        public RCWA2D() { }

        /// <summary>
        /// constructs a comparct 2D RCWA solver with parameters
        /// following the typical grating scenario
        /// </summary>
        /// <param name="wavelength"> wavelength in vacuum </param>
        /// <param name="nFront"> nFront = n(λ; {p}) </param>
        /// <param name="pFront"> parameters {p} in nFront </param>
        /// <param name="nMiddle"> nMiddle = n(λ, x, y; {p}) </param>
        /// <param name="pMiddle"> parameters {p} in nMiddle </param>
        /// <param name="periodX"> period of the middle layer along x-direction </param>
        /// <param name="periodY"> period of the middle layer along y-direction </param>
        /// <param name="thickness"> thickness of the middle layer </param>
        /// <param name="nBehind"> nBehind = n(λ; {p}) </param>
        /// <param name="pBehind"> parameters {p} in nBehind </param>
        /// <param name="nKxs"> number of spatial frequencies along x-direction </param>
        /// <param name="nKys"> number of spatial frequencies along y-direction </param>
        /// <param name="kx0"> central spatial frequency along x-direction </param>
        /// <param name="ky0"> central spatial frequency along y-direction </param>
        /// <param name="layerOverSampX"> oversampling factor for the middle layer along x-direction </param>
        /// <param name="layerOverSampY"> oversampling factor for the middle layer along y-direction </param>
        /// <param name="useWSVariation"> whether to use the W=>S variation </param>
        /// <param name="saveLayerMediaData"> whethet to save the media data </param>
        /// <param name="saveLayerModesData"> whether to save the modes data </param>
        public RCWA2D(double wavelength,
            Func<double, List<double>, Complex> nFront, List<double> pFront,
            Func<double, double, double, List<double>, Complex> nMiddle, List<double> pMiddle,
            double periodX, double periodY, double thickness,
            Func<double, List<double>, Complex> nBehind, List<double> pBehind,
            //InPlanePolMode mode,
            long nKxs, long nKys,
            double kx0 = 0.0, double ky0 = 0.0,
            double layerOverSampX = 1.0, double layerOverSampY = 1.0,
            bool useWSVariation = false,
            bool saveLayerMediaData = true,
            bool saveLayerModesData = false)
        {
            Wavelength = wavelength;
            // front
            Complex eFront(double w) => Complex.Pow(nFront.Invoke(w, pFront), 2.0);
            LayerFront = new(eFront);
            // middle
            MatrixZ eMiddleSamp(double w, GridInfo2D g, List<double> p)
            {
                MatrixZ epsilon = new(g.Rows, g.Cols, 1.0);
                for (long iRow = 0; iRow < g.Rows; iRow++)
                {
                    double y = g.GetCoordinateY(iRow);
                    for (long iCol = 0; iCol < g.Cols; iCol++)
                    {
                        double x = g.GetCoordinateX(iCol);
                        Complex n = nMiddle.Invoke(w, x, y, p);
                        epsilon[iRow, iCol, false] = n * n;
                    }
                }
                return epsilon;
            }
            MatrixZ eMiddle(double w, GridInfo2D g) => eMiddleSamp(w, g, pMiddle);
            LayerMiddle = new(periodX, periodY, eMiddle, thickness);
            // behind
            Complex eBehind(double w) => Complex.Pow(nBehind.Invoke(w, pBehind), 2.0);
            LayerBehind = new(eBehind);
            // other parameters
            NKxs = nKxs;
            NKys = nKys;
            Kx0 = kx0;
            Ky0 = ky0;
            LayerOverSampX = layerOverSampX;
            LayerOverSampY = layerOverSampY;
            UseWSVariation = useWSVariation;
            // defaults
            SaveLayerMediaData = saveLayerMediaData;
            SaveLayerModesData = saveLayerModesData;
        }

        /// <summary>
        /// constructs a RCWA (2D) solver
        /// </summary>
        /// <param name="wavelength"> working wavelength defined in vacuum </param>
        /// <param name="materialFront"> material of the front layer </param>
        /// <param name="mediumMiddle"> medium of the middle layer </param>
        /// <param name="periodX"> period of the middle layer along x direction </param>
        /// <param name="periodY"> period of the middle layer along y direction </param>
        /// <param name="thickness"> thickness of the middle layer </param>
        /// <param name="materialBehind"> material of the behind layer </param>
        public RCWA2D(double wavelength,
            Material materialFront,
            Layer2DMedium mediumMiddle,
            double periodX, double periodY, double thickness,
            Material materialBehind)
        {
            Wavelength = wavelength;
            // layers
            LayerFront = new(materialFront);
            LayerMiddle = new(periodX, periodY, mediumMiddle, thickness);
            LayerBehind = new(materialBehind);
        }

        #endregion
        #region methods

        #region ==== S-matrix ====

        /// <summary>
        /// computes half S-matrix for specific spatial frequencies
        /// </summary>
        /// <param name="kx0"> central spatial frequency along x-direction </param>
        /// <param name="ky0"> central spatial frequency along y-direction </param>
        /// <param name="fieldsSamplingX"> sampling number for E/H-fields along x direction </param>
        /// <param name="fieldsSamplingY"> sampling number for E/H-fields along y direction </param>
        /// <param name="mediumSamplingX"> sampling number for medium along x direction </param>
        /// <param name="mediumSamplingY"> sampling number for medium along y direction </param>
        /// <param name="useWSVariation"> whether to use W->S variation or not </param>
        /// <param name="saveLayerMediaData"> whether to save the medium data of the middle layer </param>
        /// <param name="saveLayerModesData"> whether to save the modes data of the middle layer </param>
        public void ComputeHalfSMatrix(double kx0 = 0.0, double ky0 = 0.0,
            long fieldsSamplingX = 0, long fieldsSamplingY = 0,
            long mediumSamplingX = 0, long mediumSamplingY = 0,
            bool useWSVariation = false,
            bool saveLayerMediaData = true,
            bool saveLayerModesData = false)
        {
            // exceptions
            if(LayerFront == null) { throw new ArgumentNullException(nameof(LayerFront)); }
            if(LayerMiddle == null) { throw new ArgumentNullException(nameof(LayerMiddle)); }
            if(LayerBehind == null) { throw new ArgumentNullException(nameof(LayerBehind)); }
            
            // spatial frequencies sampling
            if(fieldsSamplingX == 0) { fieldsSamplingX = RCWAHelper.DetermineSampling(wavelength: Wavelength, period: LayerMiddle.PeriodX, kSizeFactor: 5.0); }
            if(fieldsSamplingY == 0) { fieldsSamplingY = RCWAHelper.DetermineSampling(wavelength: Wavelength, period: LayerMiddle.PeriodY, kSizeFactor: 5.0); }
            if(mediumSamplingX == 0) { mediumSamplingX = RCWAHelper.DetermineSampling(period: LayerMiddle.PeriodX, dx: 0.1 * Wavelength); }
            if(mediumSamplingX == 0) { mediumSamplingY = RCWAHelper.DetermineSampling(period: LayerMiddle.PeriodY, dx: 0.1 * Wavelength); }
            // computes half S-matrix
            (MatrixZ s11, MatrixZ s21) = SMatrix.HalfSMatrix(wavelength: Wavelength,
                layerFront: LayerFront,
                layerMiddle: LayerMiddle,
                layerBehind: LayerBehind,
                kx0: kx0, ky0: ky0,
                fieldsSamplingX: fieldsSamplingX, fieldsSamplingY: fieldsSamplingY,
                mediumSamplingX: mediumSamplingX, mediumSamplingY: mediumSamplingY,
                useWSvariation: useWSVariation,
                saveLayerMediaData: saveLayerMediaData,
                saveLayerModesData: saveLayerModesData);

            S11 = s11;
            S21 = s21;

            // to be deleted ...
            NKxs = fieldsSamplingX;
            NKys = fieldsSamplingY;
        }


        #endregion
        #region ==== replication ====

        /// <summary>
        /// replicates the resulting field in the x-domain
        /// periodically according to multiple numbers
        /// </summary>
        /// <param name="v"> field values </param>
        /// <param name="g"> uniform sampling grid </param>
        /// <param name="replicationX"> number of periodic replications along y-direction </param>
        /// <param name="replicationY"> number of periodic replications along x-direction </param>
        public void PeriodicReplicate(ref MatrixZ v, ref GridInfo2D g,
            long replicationY = 1, long replicationX = 1)
        {
            if(replicationX <= 1 & replicationY <= 1) { return; }
            v = v.Replicate(targetRows: replicationY * v.Rows, targetCols: replicationX * v.Cols);
            g = new(rows: replicationY * g.Rows, cols: replicationX * g.Cols, spacingY: g.SpacingY, spacingX: g.SpacingX);
        }

        #endregion

        #region wrapper methods

        #region ==== plane wave coefficients ====

        /// <summary>
        /// computes the transmission coefficients for a plane wave input
        /// </summary>
        /// <param name="pw"> input plane wave </param>
        /// <returns> (tEx, tEy, grid) </returns>
        public (MatrixZ, MatrixZ, GridInfo2D) ComputeTCoefficients(PlaneWave pw)
        {
            // availability check
            if (LayerMiddle == null) { throw new ArgumentNullException(nameof(LayerMiddle)); }
            if (S11 == null) { throw new ArgumentNullException(nameof(S11)); }

            // defines input coefficients
            MatrixZ s = S11;
            (MatrixZ cEx, MatrixZ cEy, GridInfo2D g) = RCWAHelper.PlaneWaveToCoefficients(pw,
                periodX: LayerMiddle.PeriodX, periodY: LayerMiddle.PeriodY, 
                nx: NKxs, ny: NKys);
            VectorZ cIn = RCWAHelper.Convert2X2DFieldTo1D(fieldEx: cEx, fieldEy: cEy);
            // calculates output coefficients
            VectorZ cOut = LinAlg.Dot(s, cIn);
            // converts output coefficients to 2D fields
            (cEx, cEy) = RCWAHelper.Convert1DFieldTo2X2D(v: cOut, nRow: NKys, nCol: NKxs);

            // return
            return (cEx, cEy, g);
        }

        /// <summary>
        /// computes the reflection coefficients for a plane wave input
        /// </summary>
        /// <param name="pw"> input plane wave </param>
        /// <returns> (rEx, rEy, grid) </returns>
        public (MatrixZ, MatrixZ, GridInfo2D) ComputeRCoefficients(PlaneWave pw)
        {
            // availability check
            if (LayerMiddle == null) { throw new ArgumentNullException(nameof(LayerMiddle)); }
            if (S21 == null) { throw new ArgumentNullException(nameof(S11)); }

            // defines input coefficients
            MatrixZ s = S21;
            (MatrixZ cEx, MatrixZ cEy, GridInfo2D g) = RCWAHelper.PlaneWaveToCoefficients(pw,
                periodX: LayerMiddle.PeriodX, periodY: LayerMiddle.PeriodY,
                nx: NKxs, ny: NKys);
            VectorZ cIn = RCWAHelper.Convert2X2DFieldTo1D(fieldEx: cEx, fieldEy: cEy);
            // calculates output coefficients
            VectorZ cOut = LinAlg.Dot(s, cIn);
            // converts output coefficients to 2D fields
            (cEx, cEy) = RCWAHelper.Convert1DFieldTo2X2D(v: cOut, nRow: NKys, nCol: NKxs);

            // return
            return (cEx, cEy, g);
        }

        #endregion

        #endregion


        /// <summary>
        /// computes half S-matrix
        /// </summary>
        /// <returns> (s11, s21) </returns>
        [Obsolete]
        public void ComputeHalfSMatrix()
        {
            // exception
            if (LayerFront == null || LayerMiddle == null || LayerBehind == null)
                throw new ArgumentNullException($"{nameof(LayerFront)} or {nameof(LayerMiddle)} or {nameof(LayerBehind)}");
            // computation
            (S11, S21) = SMatrix.HalfSMatrix(wavelength: Wavelength,
                layerFront: LayerFront,
                layerMiddle: LayerMiddle,
                layerBehind: LayerBehind,
                nKxs: NKxs, nKys: NKys,
                kx0: Kx0, ky0: Ky0,
                mediumOverSampX: LayerOverSampX, mediumOverSampY: LayerOverSampY,
                useWSvariation: UseWSVariation,
                saveLayerMediaData: SaveLayerMediaData,
                saveLayerModesData: SaveLayerModesData);
        }

        /// <summary>
        /// computes output coefficients for 
        /// transmission only
        /// </summary>
        /// <param name="inputEx"> input coefficients for Ex-field </param>
        /// <param name="inputEy"> input coefficients for Ey-field </param>
        /// <returns> coefficients of output (Ex, Ey) </returns>
        /// <exception cref="ArgumentNullException"></exception>
        [Obsolete]
        public (MatrixZ, MatrixZ) ComputeTCoefficients(MatrixZ inputEx, MatrixZ inputEy)
        {
            // dimensions
            long nY = inputEx.Rows;
            long nX = inputEx.Cols;
            // computes half S-matrix first, if not yet done
            if (S11 == null) { ComputeHalfSMatrix(); }
            // exception
            if (S11 == null) { throw new ArgumentNullException(nameof(S11)); }
            // computes output coefficients (T)
            var cIn = RCWAHelper.Convert2X2DFieldTo1D(inputEx, inputEy);
            VectorZ cOut = LinAlg.Dot(S11, cIn);
            MatrixZ outputEx, outputEy;
            (outputEx, outputEy) = RCWAHelper.Convert1DFieldTo2X2D(v:cOut, nRow:nY, nCol:nX);
            return (outputEx, outputEy);
        }


        #endregion

    }
}
