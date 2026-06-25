using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using VEMS.MathCore;

namespace VEMS.EMSolver
{
    /// <summary>
    /// slab-waveguide solver
    /// </summary>
    public class SlabWaveguide
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
        /// the permittivity of the core
        /// </summary>
        public double EpsillonCore { get; set; }

        /// <summary>
        /// the permittivity of the cladding
        /// </summary>
        public double EpsillonClad { get; set; }

        /// <summary>
        /// the permeability of the core
        /// </summary>
        public double MuCore { get; set; }

        /// <summary>
        /// the permeability of the cladding
        /// </summary>
        public double MuClad { get; set; }

        /// <summary>
        /// the width of the waveguide
        /// </summary>
        public double Width { get; set; }

        /// <summary>
        /// the wave number in vacuum
        /// </summary>
        public double K0 { get; set; }

        /// <summary>
        /// the normalized frequency
        /// </summary>
        public double V { get; set; }

        /// <summary>
        /// the number of modes
        /// </summary>
        public int M { get; set; }

        /// <summary>
        /// The select propagation constant of waveguide
        /// </summary>
        public double Beta { get; set; }

        /// <summary>
        /// The slect parameters in the process of calculate waveguide propagation constant
        /// p in the clad
        /// </summary>
        public double P { get; set; }

        /// <summary>
        /// The select parameters in the process of calculate waveguide propagation constant
        /// q in the core
        /// </summary>
        public double Q { get; set; }

        /// <summary>
        /// Select propogate field function in waveguide
        /// </summary>
        public Func<double, double>? Fieldfunc { get; set; }

        /// <summary>
        /// Private fieldto store the value of Electric Field.
        /// Default to null if not set
        /// </summary>
        private VectorZ? _field;

        /// <summary>
        /// Sampling the select propogate field in waveguide.
        /// </summary>

        public VectorZ Field
        {
            get
            {
                if (_field == null)
                    return VectorZ.Empty;
                return _field;
            }
            set
            {
                _field = value;
            }
        }

        #endregion
        #region constructors
        /// <summary>
        /// constructor the analytical waveguide with permittivity and permeability
        /// </summary>
        /// <param name="wavelength">wavelength in vacuum</param>
        /// <param name="mode">in-plane polarization mode option</param>
        /// <param name="epsilonCore">the permittivity of the core</param>
        /// <param name="epsilonClad">the permittivity of the cladding</param>
        /// <param name="width">the width of the waveguide</param>
        /// <param name="muCore">the permeability of the core</param>
        /// <param name="muClad">the permeability of the cladding</param>
        public SlabWaveguide(double wavelength, InPlanePolMode mode, 
            double epsilonCore, double epsilonClad, double width, double muCore, double muClad)
        {
            Wavelength = wavelength;
            Polarization = mode;
            Width = width;
            EpsillonCore = epsilonCore;
            EpsillonClad = epsilonClad;
            MuCore = muCore;
            MuClad = muClad;

            K0 = 2.0 * Math.PI / Wavelength;
            V = K0 * Width / 2 * Math.Sqrt(EpsillonCore * MuCore - EpsillonClad * MuClad);
            M = (int)Math.Ceiling(2 * V / Math.PI);
        }

        /// <summary>
        /// constructor the analytical waveguide with refractive index
        /// </summary>
        /// <param name="wavelength">wavelength in vacuum</param>
        /// <param name="mode">in-plane polarization mode option</param>
        /// <param name="nCore">the refractive index of the core</param>
        /// <param name="nClad">the refractive index of the cladding</param>
        /// <param name="width">the width of the waveguide</param>
        public SlabWaveguide(double wavelength, InPlanePolMode mode, 
            double nCore, double nClad, double width)
            : this(wavelength, mode, nCore * nCore, nClad * nClad, width, 1.0, 1.0)
        { }
        #endregion
        #region methods
        /// <summary>
        /// Compute the select waveguidemode contain propagate constant and fields
        /// </summary>
        /// <param name="modeIndex">The index of select mode</param>
        /// <param name="grid">The grid for sampling the function</param>
        /// <param name="scaling">The scaling of function</param>
        public void ComputeWaveguideMode(int modeIndex, GridInfo1D grid, double scaling = 1.0)
        {
            if (modeIndex < 0 || modeIndex >= M)
            {
                Printer.Write($"Invalid ModeIndex: {modeIndex}. It must be between 0 and {M - 1}.");

                return;
            }

            (Beta, P, Q) = ComputeMode(modeIndex);
            Fieldfunc = SlabWaveguideField(i: modeIndex, scaling: scaling);
            Field = SampleField(grid);
        }

        #endregion
        #region coefficients

        /// <summary>
        /// compute the coefficients of the waveguide
        /// </summary>
        /// <param name="x">the intersection of waveguide eigen-equation</param>
        /// <returns>(Beta, p, q)</returns>
        private (double, double, double) Betapq(double x)
        {
            double d = Width / 2;
            double a = Math.Sqrt(K0 * K0 * EpsillonCore * MuCore - Math.Pow(x / d, 2));
            double b = Math.Sqrt(Math.Pow(a, 2) - K0 * K0 * EpsillonClad * MuClad);
            double c = Math.Sqrt(K0 * K0 * EpsillonCore * MuCore - Math.Pow(a, 2));
            return (a, b, c);
        }

        /// <summary>
        /// compute all the coefficient or select a coefficient of the waveguide
        /// </summary>
        /// <param name="i">The index of select mode</param>
        /// <returns>[double]beta,[double]p,[double]q</returns>
        public (double, double, double) ComputeMode(int i)
        {
            double X, beta, p, q;

            Func<double, double> evenFunc = (x) => (Polarization == InPlanePolMode.TE)
                    ? x * Math.Tan(x) - MuCore / MuClad * Math.Sqrt(Math.Pow(V, 2) - Math.Pow(x, 2))
                    : x * Math.Tan(x) - EpsillonCore / EpsillonClad * Math.Sqrt(Math.Pow(V, 2) - Math.Pow(x, 2));

            Func<double, double> oddFunc = (x) => (Polarization == InPlanePolMode.TE)
                    ? -x / Math.Tan(x) - MuCore / MuClad * Math.Sqrt(Math.Pow(V, 2) - Math.Pow(x, 2))
                    : -x / Math.Tan(x) - EpsillonCore / EpsillonClad * Math.Sqrt(Math.Pow(V, 2) - Math.Pow(x, 2));

            if (i % 2 == 0)
            {
                int evenIndex = i / 2;
                if ((1 + 2 * evenIndex) * Math.PI / 2 > V)
                    X = FindRoot.Bisection(evenFunc, lowerBound: evenIndex * Math.PI, upperBound: V, accuracy:1E-9);
                else
                    X = FindRoot.Bisection(evenFunc, lowerBound: evenIndex * Math.PI, upperBound: (1 + 2 * evenIndex) * Math.PI / 2 - 1E-14, accuracy: 1E-9);//We can never get a value of PI/2
            }
            else
            {
                int oddIndex = (i - 1) / 2;
                if ((1 + oddIndex) * Math.PI > V)
                    X = FindRoot.Bisection(oddFunc, lowerBound: (1 + 2 * oddIndex) * Math.PI / 2, upperBound: V, accuracy: 1E-9);
                else
                    X = FindRoot.Bisection(oddFunc, lowerBound: (1 + 2 * oddIndex) * Math.PI / 2, upperBound: (1 + oddIndex) * Math.PI - 1E-14, accuracy: 1E-9);
            }
            (beta, p, q) = Betapq(X);

            return (beta, p, q);

        }

        #endregion
        #region field-function

        /// <summary>
        /// compute the electric field of the waveguide
        /// </summary>
        /// <param name="i">The index of select mode</param>
        /// <param name="scaling">The scaling of field</param>
        /// <returns>field function</returns>
        public Func<double, double> SlabWaveguideField(int i, double scaling = 1.0)
        {
            double d = Width / 2;
            Func<double, Complex> Field;

            if (Polarization == InPlanePolMode.TE)
            {
                Field = (x) =>
                {
                    if (x <= -d)
                        return scaling * Complex.Exp(-Complex.ImaginaryOne * Q * d) * Complex.Exp(P * (x + d));
                    else if (x >= d)
                        return scaling * Complex.Exp(Complex.ImaginaryOne * Q * d) * Complex.Exp(-P * (x - d));
                    else
                        return scaling * Complex.Exp(Complex.ImaginaryOne * Q * x);
                };
            }
            else
            {
                Field = (x) =>
                {
                    if (x <= -d)
                        return scaling * (Beta / (K0 * EpsillonClad * MuClad)) * Complex.Exp(-Complex.ImaginaryOne * Q * d) * Complex.Exp(P * (x + d));
                    else if (x >= d)
                        return scaling * (Beta / (K0 * EpsillonClad * MuClad)) * Complex.Exp(Complex.ImaginaryOne * Q * d) * Complex.Exp(-P * (x - d));
                    else
                        return scaling * (Beta / (K0 * EpsillonCore * MuCore)) * Complex.Exp(Complex.ImaginaryOne * Q * x);
                };
            }

            // Select the real or imaginary part of the field based on the index
            Func<double, double> FieldFunc = i % 2 == 0 ? x => Field(x).Real : x => Field(x).Imaginary;

            return FieldFunc;
        }
        #endregion
        #region sample
        /// <summary>
        /// Sample the field of the waveguide
        /// </summary>
        /// <param name="grid">The grid for sampling the function</param>
        /// <returns>field vector</returns>
        public VectorZ SampleField(GridInfo1D grid)
        {
            if (Fieldfunc == null) { throw new ArgumentNullException(nameof(Fieldfunc)); }

            //sampling
            VectorZ field = new(grid.Count, 0.0);
            Samp1DRealFunc SampField = new(Fieldfunc);
            field = new(SampField.Sample(grid));

            return field;
        }
        #endregion
    }
}
