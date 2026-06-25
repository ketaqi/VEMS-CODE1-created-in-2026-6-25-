using VEMS.MathCore;
using Complex = System.Numerics.Complex;

namespace VEMS.EMSolver
{

    /// <summary>
    /// Fourier-transform-based beam propagation method
    /// for one-dimensional cases
    /// </summary>
    [Obsolete]
    public class FTBPM1D
    {
        #region properties

        /// <summary>
        /// refractive index N = n(λ, x; {p}) distribution
        /// </summary>
        public ThinLayer1D MediumX { get; set; }

        /// <summary>
        /// parameters {p} used to define N = n(λ, x; {p})
        /// </summary>
        public List<double> MediumParameters { get; set; }

        /// <summary>
        /// total propagation distance along z-direction
        /// </summary>
        public double Distance { get; set; }

        /// <summary>
        /// number of layer divisions
        /// </summary>
        public long Layers { get; set; }

        /// <summary>
        /// thickness of a single layer
        /// </summary>
        public double LayerThickness => Distance / Layers;

        /// <summary>
        /// loop-computational mode options
        /// </summary>
        public LoopMode LoopMode { get; set; }

        /// <summary>
        /// whether to save the fields inside layers
        /// generated during computation
        /// </summary>
        public bool SaveFieldsInside { get; set; }

        /// <summary>
        /// container for the fields insider laters
        /// </summary>
        public List<VectorZ>? FieldsInside { get; set; }

        #endregion
        #region constructor

        /// <summary>
        /// constructs a FT-BMP class
        /// </summary>
        /// <param name="mediumX"> refractive index N = n(λ, x; {p}) distribution </param>
        /// <param name="p"> list of parameters {p} in n(λ, x; {p}) </param>
        /// <param name="z"> total propagation distance along z-direction </param>
        /// <param name="layers"> number of layer divisions </param>
        /// <param name="saveFieldsInside"> whether to save the intermediate fields </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        public FTBPM1D(ThinLayer1D mediumX, List<double> p,
            double z, long layers,
            bool saveFieldsInside = false,
            LoopMode loopMode = Defaults.LoopOption)
        {
            MediumX = mediumX;
            MediumParameters = p;
            Distance = z;
            Layers = layers;
            SaveFieldsInside = saveFieldsInside;
            if (saveFieldsInside) { FieldsInside = new List<VectorZ>(); }
            LoopMode = loopMode;
        }

        #endregion
        #region methods

        /// <summary>
        /// performs the FT-BPM computation for given input field
        /// </summary>
        /// <param name="vIn"> input field </param>
        /// <returns> output field </returns>
        public ScalarField1D Propagate(ScalarField1D vIn)
        {
            // makes a copy of the input field
            ScalarField1D v = new(vIn);

            // layer thickness
            double dz = LayerThickness;
            // layer transmission
            Complex n0 = Complex.Sqrt(v.Epsilon * v.Mu);
            VectorZ t = MediumX.ParaxialTransmission(v.Wavelength, //MediumParameters, 
                v.Grid, dz,
                n0.Real, n0.Imaginary, 
                LoopMode);

            // first half layer
            v.Propagate(0.5 * dz, ModelingDomain.Spatial);
            if (SaveFieldsInside) { FieldsInside?.Add(new VectorZ(v.Field, true)); }
            v.Field *= t;

            // loop for inner layers
            for (long i = 0; i < Layers - 1; i++)
            {
                v.Propagate(dz, ModelingDomain.Spatial);
                if (SaveFieldsInside) { FieldsInside?.Add(new VectorZ(v.Field, true)); }
                v.Field *= t;
            }

            // last half layer
            v.Propagate(0.5 * dz, ModelingDomain.Spatial);
            return v;
        }

        #endregion
    }

    /// <summary>
    /// Fourier-transform-based beam propagation method
    /// for general two-dimensional cases
    /// </summary>
    [Obsolete]
    public class FTBPM
    {
        #region properties

        /// <summary>
        /// refractive index N = n(λ, x, y; {p}) distribution
        /// </summary>
        public ThinLayer MediumXY { get; set; }

        /// <summary>
        /// parameters {p} used to define N = n(λ, x, y; {p})
        /// </summary>
        public List<double> MediumParameters { get; set; }

        /// <summary>
        /// total propagation distance along z-direction
        /// </summary>
        public double Distance { get; set; }

        /// <summary>
        /// number of layer divisions
        /// </summary>
        public long Layers { get; set; }

        /// <summary>
        /// thickness of a single layer
        /// </summary>
        public double LayerThickness => Distance / Layers;

        /// <summary>
        /// loop-computational mode options
        /// </summary>
        public LoopMode LoopMode { get; set; }

        /// <summary>
        /// whether to save the fields inside layers
        /// generated during computation
        /// </summary>
        public bool SaveFieldsInside { get; set; }

        /// <summary>
        /// container for the fields insider laters
        /// </summary>
        public List<MatrixZ>? FieldsInside { get; set; }

        #endregion
        #region constructor

        /// <summary>
        /// constructs a FT-BMP class
        /// </summary>
        /// <param name="mediumXY"> refractive index N = n(λ, x, y; {p}) distribution </param>
        /// <param name="p"> list of parameters {p} in n(λ, x, y; {p}) </param>
        /// <param name="z"> total propagation distance along z-direction </param>
        /// <param name="layers"> number of layer divisions </param>
        /// <param name="saveFieldsInside"> whether to save the intermediate fields </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        public FTBPM(ThinLayer mediumXY, List<double> p,
            double z, long layers,
            bool saveFieldsInside = false,
            LoopMode loopMode = Defaults.LoopOption)
        {
            MediumXY = mediumXY;
            MediumParameters = p;
            Distance = z;
            Layers = layers;
            SaveFieldsInside = saveFieldsInside;
            if (saveFieldsInside) { FieldsInside = new List<MatrixZ>(); }
            LoopMode = loopMode;
        }

        #endregion
        #region methods

        /// <summary>
        /// performs the FT-BPM computation for given input field
        /// </summary>
        /// <param name="vIn"> input field </param>
        /// <returns> output field </returns>
        public ScalarField Propagate(ScalarField vIn)
        {
            // makes a copy of the input field
            ScalarField v = new(vIn);

            // layer thickness
            double dz = LayerThickness;
            // layer transmission
            Complex n0 = Complex.Sqrt(v.Epsilon * v.Mu);
            MatrixZ t = MediumXY.ParaxialTransmission(v.Wavelength, MediumParameters,
                v.Grid, dz,
                n0.Real, n0.Imaginary,
                LoopMode);

            // first half layer
            v.Propagate(0.5 * dz, ModelingDomain.Spatial);
            if (SaveFieldsInside) { FieldsInside?.Add(new MatrixZ(v.Field, true)); }
            v.Field *= t;

            // loop for inner layers
            for (long i = 0; i < Layers - 1; i++)
            {
                v.Propagate(dz, ModelingDomain.Spatial);
                if (SaveFieldsInside) { FieldsInside?.Add(new MatrixZ(v.Field, true)); }
                v.Field *= t;
            }

            // last half layer
            v.Propagate(0.5 * dz, ModelingDomain.Spatial);
            return v;
        }

        #endregion
    }


}
