namespace VEMS.MathCore
{

    /// <summary>
    /// Represents a 1D analytical phase term, supporting linear phase, lateral shift, and constant offset.
    /// Provides methods for sampling and adding the phase to data on 1D grids.
    /// </summary>
    public class Analyt1DPhase
    {
        #region fields

        /// <summary>
        /// Linear phase part.
        /// <para>Variable: x</para>
        /// <para>Parameter #1: a - linear (1st-order) coefficient</para>
        /// <para>Function: f(x) = a * x</para>
        /// </summary>
        internal Func<double, List<double>, double> Linear = Function1D.Linear;

        // Quadratic phase part is commented out for future extension.

        #endregion
        #region properties

        /// <summary>
        /// Gets or sets the lateral shift applied to the phase function.
        /// </summary>
        public double Shift { get; set; }

        /// <summary>
        /// Gets or sets the constant offset value added to the phase function.
        /// </summary>
        public double Offset { get; set; }

        ///// <summary>
        ///// quadratic (2nd-order) coefficient
        ///// </summary>
        //public double A2 { get; set; } = 0.0;

        /// <summary>
        /// linear (1st-order) coefficient 
        /// </summary>
        public double C1 { get; set; } = 0.0;

        /// <summary>
        /// Gets the total analytical phase function.
        /// <para>Variable: x</para>
        /// <para>Function: Psi(x) = Offset + Linear(x - Shift, [C1])</para>
        /// </summary>
        public Func<double, double> Psi => (x)
            => Offset + Linear(x - Shift, [C1]);
        // + Quadratic(x, new() { A2, 0.0, 0.0 });

        #endregion
        #region constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="Analyt1DPhase"/> class with default parameters.
        /// </summary>
        public Analyt1DPhase() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Analyt1DPhase"/> class with specified parameters.
        /// </summary>
        /// <param name="c1">Linear (1st-order) coefficient.</param>
        /// <param name="shift">Lateral shift.</param>
        /// <param name="offset">Constant offset.</param>
        public Analyt1DPhase(/*double a2,*/ double c1,
            double shift = 0.0, double offset = 0.0)
        {
            // sets parameters
            //A2 = a2;
            C1 = c1;
            Shift = shift;
            Offset = offset;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Analyt1DPhase"/> class by copying parameters from another instance.
        /// </summary>
        /// <param name="source">The source <see cref="Analyt1DPhase"/> instance to copy from.</param>
        public Analyt1DPhase(Analyt1DPhase source)
        {
            // sets parameters from source
            C1 = source.C1;
            Shift = source.Shift;
            Offset = source.Offset;
        }

        #endregion
        #region methods

        /// <summary>
        /// Samples the analytical phase function on a set of scattered points.
        /// </summary>
        /// <param name="xs">Sample locations, either uniform or scattered.</param>
        /// <param name="loopMode">Loop-computational mode options.</param>
        /// <returns>Sampled phase values.</returns>
        public VectorD Sample(ScatInfo1D xs,
            LoopMode loopMode = Defaults.LoopOption)
            => new Samp1DRealFunc(f: Psi).Sample(xs, loopMode);

        /// <summary>
        /// Samples the analytical phase function on a uniform grid.
        /// </summary>
        /// <param name="grid">Target uniform grid.</param>
        /// <param name="loopMode">Loop-computational mode options.</param>
        /// <returns>Sampled phase values on the target grid.</returns>
        public VectorD Sample(GridInfo1D grid,
            LoopMode loopMode = Defaults.LoopOption)
            => new Samp1DRealFunc(f: Psi).Sample(grid, loopMode);

        /// <summary>
        /// Adds the phase function to uniformly sampled target complex data.
        /// </summary>
        /// <param name="x">Uniformly sampled target data (in/out).</param>
        /// <param name="grid">Sampling grid of the data.</param>
        /// <param name="part">Target complex part in the data to modify.</param>
        /// <param name="loopMode">Loop-computational mode options.</param>
        public void AddTo(ref VectorZ x, GridInfo1D grid,
            ComplexPart part = ComplexPart.Argument,
            LoopMode loopMode = Defaults.LoopOption)
            => new Samp1DRealFunc(f: Psi).AddTo(ref x, grid, part, loopMode);

        #endregion
    }

}
