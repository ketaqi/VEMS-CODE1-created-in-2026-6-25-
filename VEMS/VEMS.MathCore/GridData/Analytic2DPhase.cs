namespace VEMS.MathCore
{


    /// <summary>
    /// 2D analytical phase term
    /// </summary>
    public class Analyt2DPhase
    {
        #region fields

        /// <summary>
        /// linear phase part
        /// <para> variable: x; </para>
        /// <para> variable: y; </para>
        /// <para> parameter #1: ax - linear (1st-order) coefficient for x; </para>
        /// <para> parameter #2: ay - linear (1st-order) coefficient for y; </para>
        /// <para> function: f(x, y) = ax * x + ay * y </para>
        /// </summary>
        public Func<double, double, List<double>, double> Linear = Function2D.Linear;

        ///// <summary>
        ///// quadratic phase part
        ///// <para> variable: x; </para>
        ///// <para> variable: y; </para>
        ///// <para> parameter #1: ax - quadratic (2nd-order) coefficient for x; </para>
        ///// <para> parameter #2: ay - quadratic (2nd-order) coefficient for y; </para>
        ///// <para> function: f(x, y) = ax * x^2 + ay * y^2 </para>
        ///// </summary>
        //public Func<double, double, List<double>, double> Quadratic = Function2D.Quadratic;

        #endregion
        #region properties

        /// <summary>
        /// lateral shift along x-direction
        /// </summary>
        public double ShiftX { get; set; }

        /// <summary>
        /// lateral shift along y-direction
        /// </summary>
        public double ShiftY { get; set; }

        /// <summary>
        /// constant offset value
        /// </summary>
        public double Offset { get; set; }

        ///// <summary>
        ///// quadratic (2nd-order) coefficient along x
        ///// </summary>
        //public double A2x { get; set; }

        ///// <summary>
        ///// quadratic (2nd-order) coefficient along y
        ///// </summary>
        //public double A2y { get; set; }

        /// <summary>
        /// linear (1st-order) coefficient along x
        /// </summary>
        public double C1x { get; set; }

        /// <summary>
        /// linear (1st-order) coefficient along y
        /// </summary>
        public double C1y { get; set; }

        /// <summary>
        /// total analytical phase function
        /// <para> variable: x; </para>
        /// <para> variable: y; </para>
        /// <para> function: Psi(x, y) = Linear(x, y) + Quadratic(x, y) </para>
        /// </summary>
        public Func<double, double, double> Psi => (x, y)
            => Offset + Linear(x - ShiftX, y - ShiftY, [C1x, C1y]);
        // + Quadratic(x, y, new() { A2x, A2y, 0.0, 0.0, 0.0 });

        #endregion
        #region constructor

        /// <summary>
        /// default constructor
        /// </summary>
        public Analyt2DPhase() { }

        /// <summary>
        /// construct a 2D analytical phase term
        /// with parameters
        /// </summary>
        /// <param name="c2x"> quadratic (2nd-order) coefficient along x </param>
        /// <param name="c2y"> quadratic (2nd-order) coefficient along y </param>
        /// <param name="c1x"> linear (1st-order) coefficient along x </param>
        /// <param name="c1y"> linear (1st-order) coefficient along y </param>
        /// <param name="shiftX"> lateral shift along x-direction </param>
        /// <param name="shiftY"> lateral shift along y-direction </param>
        /// <param name="offset"> constant offset </param>
        public Analyt2DPhase(/*double a2x, double a2y,*/ double c1x, double c1y,
            double shiftX = 0.0, double shiftY = 0.0, double offset = 0.0)
        {
            //A2x = a2x;
            //A2y = a2y;
            C1x = c1x;
            C1y = c1y;
            ShiftX = shiftX;
            ShiftY = shiftY;
            Offset = offset;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Analyt2DPhase"/> class by copying the values from an existing
        /// instance.
        /// </summary>
        /// <param name="source">The <see cref="Analyt2DPhase"/> instance to copy values from. Cannot be <see langword="null"/>.</param>
        public Analyt2DPhase(Analyt2DPhase source)
        {
            C1x = source.C1x;
            C1y = source.C1y;
            ShiftX = source.ShiftX;
            ShiftY = source.ShiftY;
            Offset = source.Offset;
        }

        #endregion
        #region methods

        /// <summary>
        /// samples the total phase function on a uniform grid
        /// </summary>
        /// <param name="grid"> target uniform grid </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> sampled phase values on the target grid </returns>
        public MatrixD Sample(GridInfo2D grid,
            LoopMode loopMode = Defaults.LoopOption)
            => new Samp2DRealFunc(f: Psi).Sample(grid, loopMode);

        /// <summary>
        /// adds the function to a uniformly sampled target data
        /// </summary>
        /// <param name="x"> uniformly sampled target data (in/out) </param>
        /// <param name="grid"> sampling grid of the data </param>
        /// <param name="part"> target complex part in the data to modify </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        public void AddTo(ref MatrixZ x, GridInfo2D grid,
            ComplexPart part = ComplexPart.Argument,
            LoopMode loopMode = Defaults.LoopOption)
            => new Samp2DRealFunc(f: Psi).AddTo(ref x, grid, part, loopMode);

        #endregion
    }

}
