using System.Numerics;
using VEMS.MathCore;

namespace VEMS.EMSolver
{
    /// <summary>
    /// base class for 1D transmission 
    /// </summary>
    public class Transmission1D
    {
        #region properties

        /// <summary>
        /// period of the transmission function when applicable
        /// </summary>
        internal double D { get; set; }

        /// <summary>
        /// lateral shift of the transmission function
        /// </summary>
        public double Shift { get; set; }

        /// <summary>
        /// overall constant scaling factor
        /// </summary>
        public double Scaling { get; set; }

        /// <summary>
        /// amplitude function, not necessarily given as periodic function
        /// but will be periodized when applicable
        /// <para> variable: x </para>
        /// <para> return: amp = Amp(x) </para>
        /// </summary>
        internal Func<double, double>? Amp { get; set; }

        /// <summary>
        /// amplitude of the transmission function
        /// <para> variable: x </para>
        /// <para> return: amplitude = Amp(x-x0) </para>
        /// </summary>
        public Func<double, double> Amplitude
        {
            get
            {
                // null case handling
                Amp ??= (x) => 1.0;
                // periodic case handling
                Func<double, double> a = (D == 0.0) ?
                    Amp : Function1D.Periodize(f: Amp, period: D);
                // applies lateral shift
                return (x) => a(x - Shift);
            }
            set => Amp = value;
        }

        /// <summary>
        /// phase function, not necessarily given as periodic function
        /// but will be periodized when applicale
        /// <para> variable: x </para>
        /// <para> return: psi = Psi(x) </para>
        /// </summary>
        internal Func<double, double>? Psi { get; set; }

        /// <summary>
        /// phase of the transmission function
        /// <para> variable: x </para>
        /// <para> return: phase = Psi(x-x0) </para>
        /// </summary>
        public Func<double, double> Phase
        {
            get
            {
                // null case handling
                Psi ??= (x) => 0.0;
                // periodic case handling
                Func<double, double> p = (D == 0.0) ?
                    Psi : Function1D.Periodize(f: Psi, period: D);
                // applies lateral shift
                return (x) => p(x - Shift);
            }
            set => Psi = value;
        }

        /// <summary>
        /// complex transmission function
        /// <para> variable: x </para>
        /// <para> return: f = a(x) Exp(i*Psi(x)) </para>
        /// </summary>
        public Func<double, Complex> F
            => (x) => Amplitude(x) * Complex.Exp(Complex.ImaginaryOne * Phase(x));

        #endregion
        #region constructors

        /// <summary>
        /// default constructor
        /// </summary>
        public Transmission1D() { }

        /// <summary>
        /// constructs a 1D transmission with given parameters
        /// </summary>
        /// <param name="shift"> lateral shift </param>
        /// <param name="scaling"> overall constant scaling factor </param>
        public Transmission1D(double shift, double scaling)
        {
            Shift = shift;
            Scaling = scaling;
        }

        #endregion
        #region methods

        #region ----- sample complex -----

        /// <summary>
        /// samples the complex transmission function 
        /// on a set of sampling locations
        /// </summary>
        /// <param name="xs"> sample locations, either uniform or scattered </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> sampled complex transmission function values </returns>
        public VectorZ Sample(ScatInfo1D xs,
            LoopMode loopMode = Defaults.LoopOption)
            => new Samp1DCplxFunc(f: F).Sample(xs, loopMode);

        /// <summary>
        /// samples the complex transmission function
        /// on a target uniform grid
        /// </summary>
        /// <param name="grid"> target uniform grid </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> sampled complex transmission function values </returns>
        public VectorZ Sample(GridInfo1D grid,
            LoopMode loopMode = Defaults.LoopOption)
            => new Samp1DCplxFunc(f: F).Sample(grid, loopMode);

        /// <summary>
        /// samples the complex transmission function
        /// on a target 2D uniform grid
        /// </summary>
        /// <param name="grid"> target 2D uniform grid </param>
        /// <param name="isAlongX"> whether the modulation is along x or y direction </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> sampled complex transmission function values </returns>
        public MatrixZ Sample(GridInfo2D grid, bool isAlongX,
            LoopMode loopMode = Defaults.LoopOption)
            => new Samp1DCplxFunc(f: F).Sample(grid, isAlongX, loopMode);

        #endregion
        #region ----- sample amplitude -----

        /// <summary>
        /// samples the real-valued amplitude function
        /// on a set of sampling locations
        /// </summary>
        /// <param name="xs"> sample locations, either uniform or scattered </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> sampled amplitude function values </returns>
        public VectorD SampleA(ScatInfo1D xs,
            LoopMode loopMode = Defaults.LoopOption)
            => new Samp1DRealFunc(f: Amplitude).Sample(xs, loopMode);

        /// <summary>
        /// samples the real-valued amplitude function
        /// on a target uniform grid
        /// </summary>
        /// <param name="grid"> target uniform grid </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> sampled amplitude function values </returns>
        public VectorD SampleA(GridInfo1D grid,
            LoopMode loopMode = Defaults.LoopOption)
            => new Samp1DRealFunc(f: Amplitude).Sample(grid, loopMode);

        /// <summary>
        /// samples the real-valued amplitude function
        /// on a target 2D uniform grid
        /// </summary>
        /// <param name="grid"> target 2D uniform grid </param>
        /// <param name="isAlongX"> whether the modulation is along x or y direction </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> sampled amplitude function values </returns>
        public MatrixD SampleA(GridInfo2D grid, bool isAlongX,
            LoopMode loopMode = Defaults.LoopOption)
            => new Samp1DRealFunc(f: Amplitude).Sample(grid, isAlongX, loopMode);

        #endregion
        #region ----- sample phase -----

        /// <summary>
        /// samples the real-valued phase function
        /// on a set of sampling locations
        /// </summary>
        /// <param name="xs"> sample locations, either uniform or scattered </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> sampled phase function values </returns>
        public VectorD SampleP(ScatInfo1D xs,
            LoopMode loopMode = Defaults.LoopOption)
            => new Samp1DRealFunc(f: Phase).Sample(xs, loopMode);

        /// <summary>
        /// samples the real-valued phase function
        /// on a target uniform grid
        /// </summary>
        /// <param name="grid"> target uniform grid </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> sampled phase function values </returns>
        public VectorD SampleP(GridInfo1D grid,
            LoopMode loopMode = Defaults.LoopOption)
            => new Samp1DRealFunc(f: Phase).Sample(grid, loopMode);

        /// <summary>
        /// samples the real-valued phase function
        /// on a target 2D uniform grid
        /// </summary>
        /// <param name="grid"> target 2D uniform grid </param>
        /// <param name="isAlongX"> whether the modulation is along x or y direction </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> sampled phase function values </returns>
        public MatrixD SampleP(GridInfo2D grid, bool isAlongX,
            LoopMode loopMode = Defaults.LoopOption)
            => new Samp1DRealFunc(f: Phase).Sample(grid, isAlongX, loopMode);

        #endregion
        #region ----- modulate ----

        ///// <summary>
        ///// modulates on a field with the transmission function
        ///// </summary>
        ///// <typeparam name="T"> any sub-class derived from ScalarField1D </typeparam>
        ///// <param name="v"> field to be modulated by the transmission function </param>
        ///// <param name="loopMode"> loop-computational mode options </param>
        //[Obsolete]
        //public void Modulate<T>(ref T v,
        //    LoopMode loopMode = Defaults.LoopOption) where T : ScalarField1D
        //{
        //    if (v.Field == null) { throw new ArgumentNullException($"{nameof(v)}"); }
        //    VectorZ t = v.Field;
        //    new Samp1DCplxFunc(f: F).ScaleOn(x: ref t, grid: v.Grid, loopMode);
        //}

        ///// <summary>
        ///// modulates on a field with the transmission function
        ///// </summary>
        ///// <typeparam name="T"> any sub-class derived from ScalarField </typeparam>
        ///// <param name="v"> field to be modulated by the transmission function </param>
        ///// <param name="isAlongX"> whether the modulation is along x or y direction </param>
        ///// <param name="loopMode"> loop-computational mode options </param>
        //[Obsolete]
        //public void Modulate<T>(ref T v, bool isAlongX,
        //    LoopMode loopMode = Defaults.LoopOption) where T : ScalarField
        //{
        //    if (v.Field == null) { throw new ArgumentNullException($"{nameof(v)}"); }
        //    MatrixZ t = v.Field;
        //    new Samp1DCplxFunc(f: F).ScaleOn(x: ref t, grid: v.Grid,
        //        isFuncAlongX: isAlongX, loopMode);
        //}


        /// <summary>
        /// Modulates a 1D scalar field with the transmission function.
        /// The modulation is performed by multiplying the field's residual values <c>UValues</c> by the sampled transmission function on the field's grid <c>UGrid</c>.
        /// </summary>
        /// <typeparam name="T">Any type derived from <see cref="SCField1D"/> representing a 1D scalar field.</typeparam>
        /// <param name="v">Reference to the scalar field to be modulated. The field's <c>UValues</c> vector will be updated in place.</param>
        /// <param name="loopMode">Specifies the loop-computational mode options for performance tuning. Default is <see cref="Defaults.LoopOption"/>.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <c>v.UValues</c> or <c>v.UGrid</c> is <c>null</c>.
        /// </exception>
        public void ModulateOn<T>(ref T v,
            LoopMode loopMode = Defaults.LoopOption)
            where T : SCField1D
        {
            var uValues = v.UValues;
            var uGrid = v.UGrid;
            if (uValues is null) { throw new ArgumentNullException(nameof(v.UValues)); }
            if (uGrid is null) { throw new ArgumentNullException(nameof(v.UGrid)); }
            if (v.Domain == ModelingDomain.SpatialFrequency) { v.SwitchToXDomain(); }
            new Samp1DCplxFunc(f: F).ScaleOn(x: ref uValues, grid: uGrid, loopMode);
        }

        /// <summary>
        /// Modulates a 2D scalar field with the 1D transmission function.
        /// The modulation is applied along either the x or y direction, as specified by <paramref name="isAlongX"/>.
        /// The field data in <c>v.UValues</c> is multiplied element-wise by the sampled transmission function.
        /// </summary>
        /// <typeparam name="T">Any type derived from <see cref="SCField"/> representing a 2D scalar field.</typeparam>
        /// <param name="v">Reference to the scalar field to be modulated. The field's <c>UValues</c> matrix will be updated in place.</param>
        /// <param name="isAlongX">If <c>true</c>, the transmission function is applied along the x-direction; otherwise, along the y-direction.</param>
        /// <param name="loopMode">Specifies the loop-computational mode options for performance tuning. Default is <see cref="Defaults.LoopOption"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown if <c>v.UValues</c> or <c>v.UGrid</c> is <c>null</c>.</exception>
        public void ModulateOn<T>(ref T v, bool isAlongX,
            LoopMode loopMode = Defaults.LoopOption)
            where T : SCField
        {
            var uValues = v.UValues;
            var uGrid = v.UGrid;
            if (uValues is null) { throw new ArgumentNullException(nameof(v.UValues)); }
            if (uGrid is null) { throw new ArgumentNullException(nameof(v.UGrid)); }
            if (v.Domain == ModelingDomain.SpatialFrequency) { v.SwitchToXDomain(); }
            new Samp1DCplxFunc(f: F).ScaleOn(x: ref uValues, grid: uGrid,
                isFuncAlongX: isAlongX, loopMode: loopMode);
        }

        #endregion
        #region ----- coefficients -----

        /// <summary>
        /// calculates the Fourier coefficients of the transmission function
        /// within a given period for fixed number of sampling
        /// </summary>
        /// <param name="period"> period for the transmission function </param>
        /// <param name="nx"> number of samples within the period (odd number) </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> Fourier coefficients (zero-centered) </returns>
        public VectorZ ComputeKCoefficients(double period, long nx,
            LoopMode loopMode = Defaults.LoopOption)
        {
            // determines sampling parameters
            if (nx % 2 == 0) { nx++; }
            GridInfo1D gx = new(n: nx, spacing: period / nx);

            // computes Fourier series
            VectorZ c = Sample(grid: gx, loopMode: loopMode);
            Transform.FFS1D(x: ref c, isForward: true);

            // return
            return c;
        }

        #endregion
        #region ----- modulate in k-domain ----

        /// <summary>
        /// modulates on a field with the transmission function
        /// using k-domain algorithm
        /// </summary>
        /// <typeparam name="T"> ScalarField1D </typeparam>
        /// <param name="v"> field to be modulated, must be given in k-domain </param>
        /// <param name="period"> period for the transmission function </param>
        /// <param name="order"> selected diffraction order </param>
        /// <param name="c"> Fourier series of the transmission function </param>
        /// <param name="isZeroCentered"> whether to center the output field around zero </param>
        /// <param name="intrpl"> interpolation method used in the k-domain </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        [Obsolete]
        public void ModulateInKDomain<T>(ref T v, 
            double period, int order, VectorZ c, 
            bool isZeroCentered = true,
            InterpolationMethod intrpl = InterpolationMethod.Linear,
            LoopMode loopMode = Defaults.LoopOption) where T : ScalarField1D
        {
            // null case handling
            if (v.Field == null) { throw new ArgumentNullException($"{nameof(v.Field)}"); }

            // central index in the Fourier series
            long ctrIdx = (c.Count - 1) / 2;
            // scales on the field
            VectorZ vi = v.Field;
            VMath.ScaleOn(x: ref vi, a: c[ctrIdx + order, false]);
            v.Field = vi;

            // output center option?
            if (isZeroCentered)
            {
                // makes a new grid that is shifted in k-domain
                GridInfo1D gi = new(other: v.Grid);
                gi.GetModified(ctrShift: order * 2.0 * Math.PI / period);
                // samples the current field back to centered coordinate by interpolation
                Grid1DCplxInterpolation itp = new(v: v.Field, grid: gi,
                    method: intrpl,
                    bound: DataBoundary.ConstantZero);
                v.Field = itp.Evaluate(targetGrid: v.Grid,
                    loopMode: loopMode);
            }
        }
            
        #endregion


        #endregion
    }
}
