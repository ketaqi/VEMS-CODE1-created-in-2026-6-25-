using System.Numerics;
using VEMS.MathCore;

namespace VEMS.EMSolver
{
    /// <summary>
    /// base class for 2D transmission
    /// </summary>
    public class Transmission2D //: IOpticalComponent
    {
        #region properties

        /// <summary>
        /// period of the transmission function 
        /// along x direction, if applicable
        /// </summary>
        internal double Dx { get; set; }

        /// <summary>
        /// period of the transmission function 
        /// along y direction, if applicable
        /// </summary>
        internal double Dy { get; set; }

        /// <summary>
        /// lateral shift of the transmission function
        /// along x direction
        /// </summary>
        public double ShiftX { get; set; }

        /// <summary>
        /// lateral shift of the transmission function
        /// along x direction
        /// </summary>
        public double ShiftY { get; set; }

        /// <summary>
        /// overall constant scaling factor
        /// </summary>
        public double Scaling { get; set; }

        /// <summary>
        /// amplitude function, not necessarily given as periodic function
        /// but will be periodized when applicable
        /// <para> variable: x </para>
        /// <para> variable: y </para>
        /// <para> return: amp = Amp(x, y) </para>
        /// </summary>
        internal Func<double, double, double>? Amp { get; set; }

        /// <summary>
        /// amplitude of the transmission function
        /// <para> variable: x </para>
        /// <para> variable: y </para>
        /// <para> return: amplitude = Amp(x-x0, y-y0) </para>
        /// </summary>
        public Func<double, double, double> Amplitude
        {
            get
            {
                // null case handling
                Amp ??= (x, y) => 1.0;
                // periodic case handling
                Func<double, double, double> a = (Dx == 0.0 && Dy == 0.0) ?
                    Amp : Function2D.Periodize(f: Amp, periodX: Dx, periodY: Dy);
                // applies lateral shift
                return (x, y) => a(x - ShiftX, y - ShiftY);
            }
            set => Amp = value;
        }

        /// <summary>
        /// phase function, not necessarily given as periodic function
        /// but will be periodized when applicale
        /// <para> variable: x </para>
        /// <para> variable: y </para>
        /// <para> return: psi = Psi(x, y) </para>
        /// </summary>
        internal Func<double, double, double>? Psi { get; set; }

        /// <summary>
        /// phase of the transmission function
        /// <para> variable: x </para>
        /// <para> variable: y </para>
        /// <para> return: phase = Psi(x-x0, y-y0) </para>
        /// </summary>
        public Func<double, double, double> Phase
        {
            get
            {
                // null case handling
                Psi ??= (x, y) => 0.0;
                // periodic case handling
                Func<double, double, double> p = (Dx == 0.0 && Dy == 0.0) ?
                    Psi : Function2D.Periodize(f: Psi, periodX: Dx, periodY: Dy);
                // applies lateral shift
                return (x, y) => p(x - ShiftX, y - ShiftY);
            }
            set => Psi = value;
        }

        /// <summary>
        /// complex transmission function
        /// <para> variable: x </para>
        /// <para> variable: y </para>
        /// <para> return: f = a(x, y) Exp(i*Psi(x, y)) </para>
        /// </summary>
        public Func<double, double, Complex> F
            => (x, y) => Amplitude(x, y) * Complex.Exp(Complex.ImaginaryOne * Phase(x, y));

        #endregion
        #region constructors

        /// <summary>
        /// default constructor
        /// </summary>
        public Transmission2D() { }

        /// <summary>
        /// constructs a 2D transmission with given parameters
        /// </summary>
        /// <param name="shiftX"> lateral shift along x direction </param>
        /// <param name="shiftY"> lateral shift along y direction </param>
        /// <param name="scaling"> overall constant scaling factor </param>
        ///// <param name="coordinate"> coordinate system of the transmission </param>
        public Transmission2D(double shiftX, double shiftY, double scaling)//, 
            //CoordinateSystem? coordinate = null)
        {
            ShiftX = shiftX;
            ShiftY = shiftY;
            Scaling = scaling;
            //Coordinate = coordinate;
        }

        #endregion
        #region methods

        #region ---- sample complex ----

        /// <summary>
        /// samples the complex transmission function
        /// on a target uniform grid
        /// </summary>
        /// <param name="grid"> target uniform grid </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> sampled complex transmission function values </returns>
        public MatrixZ Sample(GridInfo2D grid,
            LoopMode loopMode = Defaults.LoopOption)
            => (F == null) ? MatrixZ.Empty :
            new Samp2DCplxFunc(f: F).Sample(grid, loopMode);

        #endregion
        #region ---- sample amplitude ----

        /// <summary>
        /// samples the real-valued amplitude function
        /// on a target uniform grid
        /// </summary>
        /// <param name="grid"> target uniform grid </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> sampled amplitude function values </returns>
        public MatrixD SampleA(GridInfo2D grid,
            LoopMode loopMode = Defaults.LoopOption)
            => (Amplitude == null) ? MatrixD.Empty :
            new Samp2DRealFunc(f: Amplitude).Sample(grid, loopMode);

        #endregion
        #region ---- sample phase ----

        /// <summary>
        /// samples the real-valued phase function
        /// on a target uniform grid
        /// </summary>
        /// <param name="grid"> target uniform grid </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> sampled phase function values </returns>
        public MatrixD SampleP(GridInfo2D grid,
            LoopMode loopMode = Defaults.LoopOption)
            => (Phase == null) ? MatrixD.Empty :
            new Samp2DRealFunc(f: Phase).Sample(grid, loopMode);

        #endregion
        #region ---- modulate ----

        ///// <summary>
        ///// modulates on a field with the transmission function
        ///// </summary>
        ///// <typeparam name="T"> any sub-class derived from ScalarField1D </typeparam>
        ///// <param name="v"> field to be modulated by the transmission function </param>
        ///// <param name="loopMode"> loop-computational mode options </param>
        //public void Process<T>(ref T v,
        //    LoopMode loopMode = Defaults.LoopOption) where T : ScalarField
        //{
        //    if (v.Field == null) { throw new ArgumentNullException(nameof(v)); }
        //    MatrixZ t = v.Field;
        //    new Samp2DCplxFunc(f: F).ScaleOn(x: ref t, grid: v.Grid, loopMode);
        //}

        /// <summary>
        /// Modulates the given scalar field <paramref name="v"/> with the current transmission function.
        /// The modulation is performed in-place on the field's residual part <c>UValues</c> using the
        /// complex transmission function <see cref="F"/> sampled on the field's grid <c>UGrid</c>.
        /// </summary>
        /// <typeparam name="T">A type derived from <see cref="SCField"/> representing the scalar field to modulate.</typeparam>
        /// <param name="v">The scalar field to be modulated, passed by reference. The field's <c>UValues</c> and <c>UGrid</c> must not be <c>null</c>.</param>
        /// <param name="loopMode">The loop-computational mode option for the modulation operation. Defaults to <see cref="Defaults.LoopOption"/>.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="v"/>'s <c>UValues</c> or <c>UGrid</c> is <c>null</c>.
        /// </exception>
        public void ModulateOn<T>(ref T v,
            LoopMode loopMode = Defaults.LoopOption)
            where T : SCField
        {
            var uValues = v.UValues;
            var uGrid = v.UGrid;
            if (uValues is null) throw new ArgumentNullException(nameof(v.UValues));
            if (uGrid is null) throw new ArgumentNullException(nameof(v.UGrid));
            if (v.Domain == ModelingDomain.SpatialFrequency) { v.SwitchToXDomain(); }
            new Samp2DCplxFunc(f: F).ScaleOn(x: ref uValues, grid: uGrid, loopMode);
        }

        #endregion
        #region ---- combine phases ----
        /// <summary>
        /// Combines multiple phase functions into a single phase function.
        /// The results of all phase functions are summed.
        /// </summary>
        /// <param name="phases">A list of phase functions to combine.</param>
        /// <param name="useParallel">Whether to use parallel computation for combining the phases.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">A combined phase function.</exception>
        public Func<double, double, double> CombinePhase(List<Func<double, double, double>> phases, bool useParallel = true)
        {
            if (phases == null || phases.Count == 0)
            {
                throw new ArgumentException("The list of phases cannot be null or empty !");
            }
            if (useParallel)
            {
                // Use parallel processing to combine the phases
                // TODO: May be not calculate more fast every time...
                return (x, y) =>
                {
                    double result = 0.0;
                    object lockObj = new object(); // Local object to synchronize access to result

                    Parallel.ForEach(phases, () => 0.0, (phases, state, localResult) =>
                    {
                        return localResult + phases(x, y);
                    },
                    localResult =>
                    {
                        lock (lockObj)
                        {
                            result += localResult;
                        }
                    });
                    return result;
                };
            }
            else // Use sequential processing to combin the phases
            {
                return (x, y) =>
                {
                    double result = 0.0;
                    foreach (var phase in phases)
                    {
                        result += phase(x, y);
                    }
                    return result;
                };
            }
        }

        #endregion

        #endregion
    }
}
