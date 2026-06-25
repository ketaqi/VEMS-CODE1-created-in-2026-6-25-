using VEMS.MathCore;

namespace VEMS.EMSolver
{

    /// <summary>
    /// material with its dispersion defined 
    /// with given sampled data
    /// </summary>
    public class SampMaterial : Material
    {
        #region properties

        /// <summary>
        /// sampled n-data (both real and imaginary parts)
        /// </summary>
        public Scat1DCplxData? SampData { get; set; }


        ///// <summary>
        ///// real-part of refractive index nRe = nRe(λ)
        ///// at a single wavelength λ
        ///// </summary>
        //public Func<double, double>? NReal { get; set; }

        /// <summary>
        /// real-part of refractive index nRe = nRe(λ)
        /// for multiple wavelengths λ1, λ2, ... λn
        /// </summary>
        public Func<VectorD, VectorD>? NReals {  get; set; }


        ///// <summary>
        ///// real-part of refractive index nIm = nIm(λ)
        ///// at a single wavelength λ
        ///// </summary>
        //public Func<double, double>? NImag { get; set; }

        /// <summary>
        /// imaginary-part of refractive index nIm = nIm(λ)
        /// for multiple wavelengths λ1, λ2, ... λn
        /// </summary>
        public Func<VectorD, VectorD>? NImags { get; set; }

        #endregion
        #region constructors

        /// <summary>
        /// default constructor
        /// </summary>
        internal SampMaterial() 
            : base() 
        { }

        /// <summary>
        /// constructs a SampMaterial from combined input complex data
        /// </summary>
        /// <param name="sampData"> combined input complex data </param>
        public SampMaterial(Scat1DCplxData sampData)
            : base(sampData.Points[0], sampData.Points[sampData.Points.Count-1])
        {
            SampData = sampData;

            // splits SampN into real and imag parts
            ScatInfo1D scatInfo = new(SampData.Points);
            Scat1DRealData sampNReal = new(scatInfo: scatInfo, values: VMath.RealPart(SampData.Values));
            Scat1DRealData sampNImag = new(scatInfo: scatInfo, values: VMath.ImagPart(SampData.Values));
            // fitters
            //BSpline1D fitterNReal = new(samples: sampNReal, degree: 3, nFactor: 0.1);
            //BSpline1D fitterNImag = new(samples: sampNImag, degree: 3, nFactor: 0.1);
            // defines functions
            NReal = (w) => (w < WavelengthMin || w > WavelengthMax) ?
                double.NaN : Interpolation.Linear(xs: sampNReal.Points, vs: sampNReal.Values, x: w); // fitterNReal.Evaluate(xe: w);
            NImag = (w) => (w < WavelengthMin || w > WavelengthMax) ?
                double.NaN : Interpolation.Linear(xs: sampNImag.Points, vs: sampNImag.Values, x: w); //fitterNImag.Evaluate(xe: w);
            NReals = (w) => (w[0] < WavelengthMin || w[w.Count - 1] > WavelengthMax) ?
                VectorD.Empty : Interpolation.Linear(xs: sampNReal.Points, vs: sampNReal.Values, xe: w); //fitterNReal.Evaluate(xe: w);
            NImags = (w) => (w[0] < WavelengthMin || w[w.Count - 1] > WavelengthMax) ?
                VectorD.Empty : Interpolation.Linear(xs: sampNImag.Points, vs: sampNImag.Values, xe: w); //fitterNImag.Evaluate(xe: w);
        }

        ///// <summary>
        ///// constructs a SampMaterial from input data
        ///// </summary>
        ///// <param name="w"> sampled locations in wavelength </param>
        ///// <param name="nReal"> sampled real-part of n </param>
        ///// <param name="nImag"> sampled imag-part of n </param>
        //public SampMaterial(VectorD w, VectorD nReal, VectorD nImag) 
        //    : base(wavelengthMin: w[0], wavelengthMax: w[w.Count-1]) 
        //{
        //    VectorZ n = VMath.Construct(realPart: nReal, imagPart: nImag);
        //    SampData = new(points: w, values: n);
        //}

        #endregion
        #region methods

        // ...

        #endregion
        #region static fields

        /// <summary>
        /// Ag (Silver)
        /// P. B. Johnson and R. W. Christy. Optical constants of 
        /// the noble metals, Phys. Rev. B 6, 4370-4379 (1972)
        /// </summary>
        public static SampMaterial Ag_Johnson1972 = new(sampData: new Scat1DCplxData(
            scatInfo: new(49) 
            {   [0] = 0.1879, 
                [1] = 0.1916, 
                [2] = 0.1953,
                [3] = 0.1993,
                [4] = 0.2033,
                [5] = 0.2073,
                [6] = 0.2119,
                [7] = 0.2164,
                [8] = 0.2214,
                [9] = 0.2262,
                [10] = 0.2313,
                [11] = 0.2371,
                [12] = 0.2426,
                [13] = 0.2490, 
                [14] = 0.2551,
                [15] = 0.2616,
                [16] = 0.2689,
                [17] = 0.2761,
                [18] = 0.2844,
                [19] = 0.2924,
                [20] = 0.3009,
                [21] = 0.3107,
                [22] = 0.3204,
                [23] = 0.3315,
                [24] = 0.3425,
                [25] = 0.3542,
                [26] = 0.3679,
                [27] = 0.3815,
                [28] = 0.3974,
                [29] = 0.4133,
                [30] = 0.4305,
                [31] = 0.4509,
                [32] = 0.4714,
                [33] = 0.4959,
                [34] = 0.5209,
                [35] = 0.5486,
                [36] = 0.5821,
                [37] = 0.6168,
                [38] = 0.6595,
                [39] = 0.7045,
                [40] = 0.7560,
                [41] = 0.8211,
                [42] = 0.8920,
                [43] = 0.9840,
                [44] = 1.0880,
                [45] = 1.2160,
                [46] = 1.3930,
                [47] = 1.6100,
                [48] = 1.9370
            },
            values: new(49) 
            {
                [0] = new(1.07, 1.212),
                [1] = new(1.10, 1.232),
                [2] = new(1.12, 1.255),
                [3] = new(1.14, 1.277),
                [4] = new(1.15, 1.296),
                [5] = new(1.18, 1.312),
                [6] = new(1.20, 1.325),
                [7] = new(1.22, 1.336),
                [8] = new(1.25, 1.342),
                [9] = new(1.26, 1.344),
                [10] = new(1.28, 1.357),
                [11] = new(1.28, 1.367),
                [12] = new(1.30, 1.378),
                [13] = new(1.31, 1.389),
                [14] = new(1.33, 1.393),
                [15] = new(1.35, 1.387),
                [16] = new(1.38, 1.372),
                [17] = new(1.41, 1.331),
                [18] = new(1.41, 1.264),
                [19] = new(1.39, 1.161),
                [20] = new(1.34, 0.964),
                [21] = new(1.13, 0.616),
                [22] = new(0.81, 0.392),
                [23] = new(0.17, 0.829),
                [24] = new(0.14, 1.142),
                [25] = new(0.10, 1.419),
                [26] = new(0.07, 1.657),
                [27] = new(0.05, 1.864),
                [28] = new(0.05, 2.070),
                [29] = new(0.05, 2.275),
                [30] = new(0.04, 2.462),
                [31] = new(0.04, 2.657),
                [32] = new(0.05, 2.869),
                [33] = new(0.05, 3.093),
                [34] = new(0.05, 3.324),
                [35] = new(0.06, 3.586),
                [36] = new(0.05, 3.858),
                [37] = new(0.06, 4.152),
                [38] = new(0.05, 4.483),
                [39] = new(0.04, 4.838),
                [40] = new(0.03, 5.242),
                [41] = new(0.04, 5.727),
                [42] = new(0.04, 6.312),
                [43] = new(0.04, 6.992),
                [44] = new(0.04, 7.795),
                [45] = new(0.09, 8.828),
                [46] = new(0.13, 10.10),
                [47] = new(0.15, 11.85),
                [48] = new(0.24, 14.08)
            }));

        #endregion

    }
}
