using System.Numerics;
using VEMS.MathCore;

namespace VEMS.EMSolver
{

    /// <summary>
    /// uniform segmentation for 1D meta-layer
    /// </summary>
    public class SegMeta1D
    {
        #region properties

        /// <summary>
        /// centers of the segments
        /// </summary>
        public GridInfo1D Centers { get; set; }

        /// <summary>
        /// elementary rectangular segment unit for field
        /// with varying center and fixed width and edge
        /// </summary>
        public Segment1D.CosRect UnitField { get; set; }
        
        /// <summary>
        /// internally used field segmentation
        /// </summary>
        internal SegField1D SField { get; set; }

        /// <summary>
        /// elementary rectangular segment unit for layer medium
        /// with varying center and fixed width and edge
        /// </summary>
        public Segment1D.CosRect UnitLayer { get; set; }
        
        /// <summary>
        /// internally used layer segmentation
        /// </summary>
        internal SegLayer1D SLayer { get; set; }

        /// <summary>
        /// local grid for each segment, same for both field and layer medium
        /// </summary>
        public GridInfo1D? LocalGrid { get; set; }

        /// <summary>
        /// list of layer media for each segment
        /// </summary>
        public List<Layer1DMedium>? Media { get; set; }

        /// <summary>
        /// list of local RCWA solvers for each segment
        /// </summary>
        public List<RCWA1D>? Solvers { get; set; }

        #endregion
        #region constructors

        /// <summary>
        /// constructs segmentation for 1D field
        /// with uniform rectangular segments
        /// </summary>
        /// <param name="centers"> centers of the segments </param>
        /// <param name="unitField"> elementary segment for the field </param>
        /// <param name="unitLayer"> elementary segment for the layer medium </param>
        public SegMeta1D(GridInfo1D centers,
            Segment1D.CosRect unitField,
            Segment1D.CosRect unitLayer)
        {
            Centers = centers;
            UnitField = unitField;
            UnitLayer = unitLayer;
            // initializes
            SLayer = new(centers: centers, unit: unitLayer);
            SField = new(centers: centers, unit: unitField);
        }

        #endregion
        #region methods

        /// <summary>
        /// creates local RCWA1D solvers for each segment in its local coordinate
        /// and saves them in the list "Solvers"
        /// </summary>
        /// <param name="front"> material in front of the meta-layer </param>
        /// <param name="middle"> meta-layer in the middle </param>
        /// <param name="behind"> material behind the meta-layer </param>
        public void CreateLocalSolvers(Material front,
            Grid1DMetaLayer middle, 
            Material behind)
        {
            // initialize lists
            Media = new();
            Solvers = new();

            // loops over segments
            for (int i = 0; i < Centers.Count; i++)
            {
                // takes local medium
                Layer1DMedium mi = SLayer.TakeFrom(iSeg: i, medium: middle);
                Media.Add(mi);
                // makes local solver
                RCWA1D si = new(materialFront: front,
                    mediumMiddle: Media[i],
                    period: SLayer.Units[i].Width,
                    thickness: middle.Height,
                    materialBehind: behind);
                // adds to list
                Solvers.Add(si);
            }
        }

        /// <summary>
        /// computes half S-matrices for each segment
        /// with the same parameters for all segments
        /// </summary>
        /// <param name="wavelength"> vacuum wavelength </param>
        /// <param name="mode"> polarization mode option </param>
        /// <param name="kx0"> central spatial frequency along the x-direction </param>
        /// <param name="fieldsSampling"> sampling number for E/H-fields </param>
        /// <param name="mediumSampling"> sampling number for medium </param>
        /// <param name="useWSVariation"> whether to use W->S variation or not </param>
        /// <param name="saveLayerMediaData"> whether to save the medium data of the middle layer </param>
        /// <param name="saveLayerModesData"> whether to save the modes data of the middle layer </param>
        /// <param name="logInfo"> whether to log the step information </param>
        /// <exception cref="ArgumentNullException"></exception>
        public void ComputeHalfSMatrices(double wavelength,
            InPlanePolMode mode = InPlanePolMode.TE,
            double kx0 = 0.0,
            long fieldsSampling = 0,
            long mediumSampling = 0,
            bool useWSVariation = false,
            bool saveLayerMediaData = true,
            bool saveLayerModesData = false,
            bool logInfo = true)
        {
            if (Solvers == null) { throw new ArgumentNullException(nameof(Solvers)); }
            for (int i = 0; i < Solvers.Count; i++)
            {
                if (logInfo) { Printer.Write($"calculating local s-matrix for segment [{i}/{Centers.Count}] ..."); }
                // let local solver to compute its half S-matrix
                Solvers[i].ComputeHalfSMatrix(wavelength: wavelength,
                    mode: mode,
                    kx0: kx0,
                    fieldsSampling: fieldsSampling,
                    mediumSampling: mediumSampling,
                    useWSVariation: useWSVariation,
                    saveLayerMediaData: saveLayerMediaData,
                    saveLayerModesData: saveLayerModesData);
            }
        }

        /// <summary>
        /// generates local fields within each segment for a plane wave
        /// </summary>
        /// <param name="pIn"> incident plane wave </param>
        /// <returns> list of local field </returns>
        /// <exception cref="ArgumentNullException"></exception>
        public List<ScalarField1D> GenerateLocalFields(PlaneWaveXZ pIn)
        {
            if (LocalGrid == null)
            { throw new ArgumentNullException(); }
            // loop
            List<ScalarField1D> v = new();
            for (int i = 0; i < Centers.Count; i++)
            {
                // !!!
                Func<double, Complex> fi = SField.Units[i].TakeFrom(fIn: (x) => pIn.E);
                //SField.TakeFrom(iSeg: i, f: (x) => pIn.E);
                // !!!
                Samp1DCplxFunc sfi = new (f: (x, p) => fi(x));
                ScalarField1D vi = new(wavelength: pIn.Wavelength,
                    epsilon: pIn.Epsilon, mu: pIn.Mu,
                    grid: new(LocalGrid), fieldValues: sfi.Sample(LocalGrid));
                v.Add(vi);
            }
            return v;
        }

        /// <summary>
        /// computes coefficients for localized incident fields
        /// within each segment
        /// </summary>
        /// <param name="vIn"> list of local incident fields </param>
        /// <param name="isTransmission"> whether to compute transmission or reflection </param>
        /// <param name="interp"> interpolation method option </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> resulting local coefficients (transmission or reflection) </returns>
        /// <exception cref="ArgumentNullException"></exception>
        public List<VectorZ> ComputeLocalCoefficients(List<ScalarField1D> vIn,
            bool isTransmission = true,
            InterpolationMethod interp = Defaults.IntrplOption,
            LoopMode loopMode = Defaults.LoopOption)
        {
            if (Solvers == null || LocalGrid == null)
            { throw new ArgumentNullException(); }
            List<VectorZ> c = new();
            for(int i = 0; i < Centers.Count; i++)
            {
                VectorZ ci = Solvers[i].ComputeCoefficients(vIn[i],
                    isTransmission: isTransmission,
                    interp: interp, loopMode: loopMode);
                c.Add(ci);
            }
            return c;
        }

        /// <summary>
        /// computes the summed output field from the local segments
        /// </summary>
        /// <param name="c"> output coefficients for each segments </param>
        /// <param name="kRange"> k-domain range </param>
        /// <param name="itpl"> interpolation method </param>
        /// <returns> result summed field in the k-domain </returns>
        /// <exception cref="ArgumentNullException"></exception>
        public (VectorZ, GridInfo1D) ComputeSummedField
            (List<VectorZ> c, double kRange,
            InterpolationMethod itpl = InterpolationMethod.Linear)
        {
            if (Solvers == null || LocalGrid == null)
            { throw new ArgumentNullException(); }

            // defines the output grid
            double dk = 2.0 * Math.PI / Centers.Range;
            long nk = (long)(kRange / dk);
            if (nk % 2 == 0) { nk++; }
            GridInfo1D g = new(n: nk, spacing: dk);
            VectorZ v = new(count: nk);
            // loop
            for (int i = 0; i < Centers.Count; i++)
            {
                // local field sampling in k-domain
                GridInfo1D gi = new(n: c[i].Count, spacing: 2.0 * Math.PI / SLayer.Units[i].Width);
                // interpolation
                Grid1DCplxInterpolation itp = new (v: c[i], grid: gi, method: itpl);
                VectorZ vi = itp.Evaluate(targetGrid: g);
                // handles the linear phase
                Samp1DRealFunc linPhase = new (f: Function1D.Linear,
                    p: new List<double> { -Centers[i], 0.0, 0.0 });
                vi *= VMath.Exp(Complex.ImaginaryOne * linPhase.Sample(g));
                // sums into result
                v += vi;
            }

            // return
            return (v, g);
        }

        #endregion
    }
}
