using System.Numerics;
using VEMS.MathCore;

namespace VEMS.EMSolver
{

    /// <summary>
    /// material with its dispersion defined 
    /// with given formula and parameters
    /// </summary>
    public class FuncMaterial : Material
    {
        #region constructors

        /// <summary>
        /// constructs a FormularMaterial by given functions
        /// </summary>
        /// <param name="nReal"> function that defines the real-part nRe = nRe(λ) </param>
        /// <param name="nImag"> function that defines the imag-part nIm = nIm(λ) </param>
        /// <param name="wavelengthMin"> lower bound of the wavelength range </param>
        /// <param name="wavelengthMax"> upper bound of the wavelength range </param>
        public FuncMaterial(Func<double, double> nReal,
            Func<double, double>? nImag = null,
            double wavelengthMin = 0.0,
            double wavelengthMax = double.PositiveInfinity)
        {
            // defines wavelength range
            WavelengthMin = wavelengthMin;
            WavelengthMax = wavelengthMax;
            // real-part
            NReal = (w) => (w < WavelengthMin || w > WavelengthMax) ? double.NaN : nReal(w);
            // imag-part
            nImag ??= (w) => 0.0;
            NImag = (w) => (w < WavelengthMin || w > WavelengthMax) ? double.NaN : nImag(w);
            // complex
            N = (w) => new Complex(NReal(w), NImag(w));
            // permittivity
            Epsilon = (w) => N(w) * N(w);
        }

        /// <summary>
        /// constructs a FormularMaterial with constant 
        /// refractive index
        /// </summary>
        /// <param name="nReal"> constant real-part refractive index </param>
        /// <param name="nImag"> constant imag-part refractive index </param>
        /// <param name="wavelengthMin"> lower bound of the wavelength range </param>
        /// <param name="wavelengthMax"> upper bound of the wavelength range </param>
        public FuncMaterial(double nReal,
            double nImag = 0.0,
            double wavelengthMin = 0.0,
            double wavelengthMax = double.PositiveInfinity)
            : this(nReal: (w) => nReal, nImag: (w) => nImag,
                  wavelengthMin: wavelengthMin, wavelengthMax: wavelengthMax) 
        { }

        #endregion
        #region methods

        // derivative ...

        #endregion
        #region derived sub-classes

        /// <summary>
        /// FuncMaterial defined by Sellmeier equation
        /// </summary>
        public class Sellmeier : FuncMaterial
        {
            #region constructors

            /// <summary>
            /// Sellmeier defined by full parameter list
            /// </summary>
            /// <param name="p"> list of parameters used in the Sellmeier equation </param>
            /// <param name="wavelengthMin"> lower bound of the wavelength range </param>
            /// <param name="wavelengthMax"> upper bound of the wavelength range </param>
            public Sellmeier(List<double> p,
                double wavelengthMin = 0.0, 
                double wavelengthMax = double.PositiveInfinity)
                : base(nReal: (w) => Function1D.Sellmeier(arg1: w, arg2: p),
                      wavelengthMin: wavelengthMin, wavelengthMax: wavelengthMax)
            { }

            /// <summary>
            /// typical glasses described by Sellmeier equation 
            /// with three terms
            /// </summary>
            /// <param name="b1"> coefficient b1 </param>
            /// <param name="c1"> coefficient c1 </param>
            /// <param name="b2"> coefficient b2 </param>
            /// <param name="c2"> coefficient c2 </param>
            /// <param name="b3"> coefficient b3 </param>
            /// <param name="c3"> coefficeint c3 </param>
            /// <param name="wavelengthMin"> lower bound of the wavelength range </param>
            /// <param name="wavelengthMax"> upper bound of the wavelength range </param>
            public Sellmeier(double b1, double c1,
                double b2, double c2, 
                double b3, double c3,
                double wavelengthMin = 0.0, 
                double wavelengthMax = double.PositiveInfinity)
                : this(p: new List<double> { b1, c1, b2, c2, b3, c3},
                       wavelengthMin: wavelengthMin,
                       wavelengthMax: wavelengthMax)
            { }

            #endregion
            #region static fields

            /// <summary>
            /// Fused silica - SiO2
            /// I. H. Malitson. Interspecimen comparison of the refractive 
            /// index of fused silica, J. Opt. Soc. Am. 55, 1205-1208 (1965)
            /// </summary>
            public static Sellmeier SiO2_Malitson1965 = new(p: new List<double>() 
                { 0.6961663, 0.0684043, 0.4079426, 0.1162414, 0.8974794, 9.896161 },
                wavelengthMin: 0.21, 
                wavelengthMax: 6.7);

            /// <summary>
            /// Fused silica - SiO2
            /// Y. Arosa and R. de la Fuente. Refractive index spectroscopy 
            /// and material dispersion in fused silica glass, 
            /// Opt. Lett. 45, 4268-4271 (2020)
            /// </summary>
            public static Sellmeier SiO2_Arosa2020 = new(p: new List<double>()
                { 0.9310, 0.079, 0.1735, 0.130, 2.1121, 14.918 },
                wavelengthMin: 0.26, 
                wavelengthMax: 1.7);

            #endregion
        }


        #endregion
    }
}
