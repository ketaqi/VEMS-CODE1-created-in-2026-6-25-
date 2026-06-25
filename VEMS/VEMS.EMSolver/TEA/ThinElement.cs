using VEMS.MathCore;
using Complex = System.Numerics.Complex;

namespace VEMS.EMSolver
{
    /// <summary>
    /// 
    /// </summary>
    [Obsolete]
    public class ThinElement1D : MediumXZ
    {
        #region properties

        /// <summary>
        /// length of the layer along the z-direction
        /// </summary>
        public double Length { get; set; }

        #endregion
        #region constructors

        /// <summary>
        /// default constructor
        /// </summary>
        internal ThinElement1D() { }

        /// <summary>
        /// constructs a 1D thin-element with given 
        /// refractive index distribution 
        /// </summary>
        /// <param name="n"> refractive index n = n(λ,x,z) </param>
        /// <param name="length"></param>
        public ThinElement1D(Func<double, double, double, Complex> n, 
            double length = 0.0) : base(n)
        { Length = length; }

        #endregion
        #region methods

        // ...

        #endregion
    }

    /// <summary>
    /// thin-element approximation for 1D cases
    /// </summary>
    [Obsolete]
    public class TEApprox1D
    {
        #region helpers 

        /// <summary>
        /// quantizes the input phase function
        /// </summary>
        /// <param name="phi"> input phase function </param>
        /// <param name="enforce2Pi"> whether enforces 2Pi phase range </param>
        /// <param name="nLevel"> number of phase quantization levels </param>
        /// <param name="offset"> constant offset added to the result </param>
        /// <returns> quantized phase function </returns>
        public static Func<double, double> QuantizePhase(Func<double, double> phi,
            bool enforce2Pi = true,
            long nLevel = 2, double offset = 0.0)
        {
            // defines phase step size
            double dPhi = 2.0 * Math.PI / nLevel;
            Func<double, double> qPhi = (x) =>
            {
                long m = (long)Math.Floor((phi(x) + Math.PI) / dPhi);
                if (enforce2Pi)
                {
                    if (m >= nLevel) { m = nLevel - 1; }
                    if (m < 0) { m = 0; }
                }
                return m * dPhi - Math.PI + offset;
            };
            return qPhi;
        }

        /// <summary>
        /// wraps the input phase function within 2Pi
        /// </summary>
        /// <param name="phi"> input phase fuinction </param>
        /// <param name="offset"> constant offset added to the result </param>
        /// <returns> wrapped phase function </returns>
        public static Func<double, double> WrapPhase(Func<double, double> phi,
            double offset = 0.0)
        {
            Func<double, double> wPhi = (x) =>
            {
                var z = Complex.Exp(Complex.ImaginaryOne * phi(x));
                return z.Phase + offset;
            };
            return wPhi;
        }

        #endregion
        #region paraxial - single layer

        /// <summary>
        /// computes paraxial transmission function for 
        /// medium with given thickness at certain wavelength
        /// </summary>
        /// <typeparam name="T"> LayerMedium1D </typeparam>
        /// <param name="layer"> layer medium 1D </param>
        /// <param name="wavelength"> working wavelength in vacuum </param>
        /// <param name="thickness"> thickness of the medium along z </param>
        /// <param name="isPhaseOnly"> whether the transmission is phase only </param>
        /// <returns> transmission function </returns>
        public static Transmission1D ParaxialT<T>(T layer,
            double wavelength, double thickness,
            bool isPhaseOnly = true)
            where T : Layer1DMedium
        {
            double k0 = 2.0 * Math.PI / wavelength;
            Func<double, double> nRe = (x) => layer.N(wavelength, x).Real;
            Func<double, double> nIm = (x) => layer.N(wavelength, x).Imaginary;
            // defines transmission function
            Transmission1D t = new()
            {  Phase = (x) => k0 * nRe(x) * thickness };
            if(!isPhaseOnly)
            { t.Amplitude = (x) => -k0 * nIm(x) * thickness; }
            return t;
        }


        public static Transmission1D T<T>(T layer,
            double wavelength, double thickness,
            bool isPhaseOnly = true)
            where T : Medium1D
        {
            // gets parameters
            double k0 = 2.0 * Math.PI / wavelength;
            Func<double, double> nRe = (x) => layer.N(wavelength, x).Real;
            Func<double, double> nIm = (x) => layer.N(wavelength, x).Imaginary;
            // defines transmission
            Transmission1D t = new()
            { Phase = (x) => k0 * nRe(x) * thickness };
            if (!isPhaseOnly)
            { t.Amplitude = (x) => -k0 * nIm(x) * thickness; }
            return t;
        }


        #endregion
        #region paraxial - multi-layer

            // ...
        internal static Transmission1D ParaxialT(MediumXZ medium,
            double wavelength, (double, double) zRange, long nLayers,
            bool isPhaseOnly = true)
        {
            return new Transmission1D();
        }

        #endregion
        #region parabasal

        // ...

        #endregion
        #region inverse


        // ...

        //public static Layer1DMedium InvDesign(Func<double, double>)


        #endregion
    }




    /// <summary>
    /// two-dimensional (2D) thin-layer class
    /// </summary>
    [Obsolete]
    public class ThinLayer
    {

        #region properties

        /// <summary>
        /// refractive index N = n(λ, x, y; {p})
        /// </summary>
        public Func<double, double, double, List<double>, Complex> NFunc { get; set; }

        /// <summary>
        /// length of the layer along the z-direction
        /// </summary>
        public double Length { get; set; }

        #endregion
        #region constructor

        /// <summary>
        /// constructs a 1D thin-element with given
        /// refractive index distribution
        /// </summary>
        /// <param name="nFunc"> refractive index N = n(λ, x, y; {p}) </param>
        /// <param name="length"> length of the medium along the z-direction </param>
        public ThinLayer(Func<double, double, double, List<double>, Complex> nFunc, double length)
        {
            NFunc = nFunc;
            Length = length;
        }

        #endregion
        #region methods

        /// <summary>
        /// samples N = n(λ, x, y; {p}) on given grid
        /// for a fixed wavelength λ
        /// </summary>
        /// <param name="wavelength"> wavelength in vacuum </param>
        /// <param name="parameters"> list of parameters {p} in n(λ, x; {p}) </param>
        /// <param name="grid"> target uniform sampling grid </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> sampled epsilon on the target grid </returns>
        /// <exception cref="ArgumentNullException"></exception>
        public MatrixZ SampleN(double wavelength, List<double> parameters, GridInfo2D grid,
            LoopMode loopMode = Defaults.LoopOption)
        {
            if (NFunc == null) { throw new ArgumentNullException(nameof(NFunc)); }

            // makes a new function at fixed wavelength
            Complex nFuncXY(double x, double y, List<double> p) => NFunc.Invoke(wavelength, x, y, p);

            // uniform-grid sample of the function
            Samp2DCplxFunc gridFunc = new(nFuncXY, parameters);
            return gridFunc.Sample(grid, loopMode);
        }

        /// <summary>
        /// computes paraxial transmission function using
        /// thin-element-approximation (TEA) method
        /// for a fixed wavelength λ and a given distance
        /// </summary>
        /// <param name="wavelength"> wavelength in vacuum </param>
        /// <param name="parameters"> list of parameters {p} in n(λ, x, y; {p}) </param>
        /// <param name="grid"> target uniform sampling grid </param>
        /// <param name="d"> propagation distance along the z-direction </param>
        /// <param name="n0Re"> real-part of constant reference refractive index to subtract </param>
        /// <param name="n0Im"> imag-part of constant reference refractive index to subtract </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> complex transmission result </returns>
        public MatrixZ ParaxialTransmission(double wavelength,
            List<double> parameters,
            GridInfo2D grid,
            double d,
            double n0Re = 0.0, double n0Im = 0.0,
            LoopMode loopMode = Defaults.LoopOption)
        {
            double k0 = 2.0 * Math.PI / wavelength;
            MatrixZ n = SampleN(wavelength, parameters, grid, loopMode);
            // subtract reference n0, if n0 is not zero
            Complex n0 = new(n0Re, n0Im);
            if (n0 != 0.0) { n -= n0; }

            // loop mode cases
            MatrixZ t = new(grid.Rows, grid.Cols);
            switch (loopMode)
            {
                case LoopMode.Sequential:
                    {
                        for (long iRow = 0; iRow < t.Rows; iRow++)
                        {
                            for(long iCol = 0; iCol < t.Cols; iCol ++)
                            {
                                t[iRow, iCol, false] = Complex.Exp(Complex.ImaginaryOne
                                    * k0 * n[iRow, iCol, false] * d);
                            }
                        }
                        break;
                    }
                case LoopMode.Parallel:
                    {
                        Parallel.For(0, t.Count, iRow =>
                        {
                            for(long iCol = 0; iCol < t.Cols; iCol++)
                            {
                                t[iRow, iCol, false] = Complex.Exp(Complex.ImaginaryOne
                                    * k0 * n[iRow, iCol, false] * d);   
                            }
                        });
                        break;
                    }
                case LoopMode.Vectorized:
                    {
                        t = VMath.Exp(VMath.Scale(n, Complex.ImaginaryOne * k0 * d));
                        break;
                    }
                default: goto case LoopMode.Sequential;
            }

            return t;
        }


        // ...



        #endregion

    }




}
