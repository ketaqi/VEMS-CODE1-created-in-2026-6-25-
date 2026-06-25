using System.Numerics;
using VEMS.MathCore;

namespace VEMS.EMSolver
{

    /// <summary>
    /// Specifies the type of grid used for field sampling.
    /// </summary>
    public enum FieldGridType
    {
        /// <summary>
        /// Grid is defined in the spatial domain.
        /// </summary>
        Spatial,

        /// <summary>
        /// Grid is defined in the angular domain.
        /// </summary>
        Angular,

        /// <summary>
        /// Grid is defined in the numerical aperture domain.
        /// </summary>
        NumericalAperture,
    }

    /// <summary>
    /// Provides common field operations for 1D and 2D cases, including phase addition and phase manipulation
    /// for complex-valued vectors and matrices on uniform grids.
    /// </summary>
    internal class FieldCommons
    {

        #region ---- 1D case ----

        /// <summary>
        /// Adds an additional phase to a complex vector field sampled on a 1D grid.
        /// </summary>
        /// <param name="v">Reference to the complex vector field to modify (in/out).</param>
        /// <param name="grid">Sampling grid of the data.</param>
        /// <param name="f">Function that defines the phase as a function of position and parameters.</param>
        /// <param name="p">Parameters used in the phase definition.</param>
        /// <param name="scalFac">Optional overall scaling factor for the result. Default is 1 (no scaling).</param>
        /// <param name="loopMode">Loop-computational mode options.</param>
        public static void AddPhase(ref VectorZ v, GridInfo1D grid,
            Func<double, List<double>, double> f, List<double> p,
            Complex scalFac = default,
            LoopMode loopMode = Defaults.LoopOption)
        {
            // initializes scaling factor
            Complex a = scalFac == default ? Complex.One : scalFac;

            // define loop operation
            var t = v;
            void op(long i)
            {
                double x = grid[i];
                double psi = f(x, p);
                if (a == Complex.One)
                { t[i, false] *= Complex.Exp(Complex.ImaginaryOne * psi); }
                else
                { t[i, false] *= a * Complex.Exp(Complex.ImaginaryOne * psi); }
            }
            Loop1D loop = new(operation: op,
                start: 0, end: t.Count,
                step: 1);
            loop.Evaluate(mode: loopMode);
        }


        /// <summary>
        /// Adds an additional quadratic phase to a uniformly sampled target data.
        /// </summary>
        /// <param name="x">Target data to modify (in/out).</param>
        /// <param name="grid">Sampling grid of the data.</param>
        /// <param name="a">Quadratic coefficient for x^2 term in the phase.</param>
        /// <param name="scalFac">Optional overall scaling factor for the result. Default is 1 (no scaling).</param>
        /// <param name="loopMode">Loop-computational mode options.</param>
        public static void AddQuadPhase(ref VectorZ x, GridInfo1D grid,
            double a, 
            Complex scalFac = default,
            LoopMode loopMode = Defaults.LoopOption)
            => AddPhase(v: ref x, grid: grid,
                f: Function1D.Quadratic, p: [a],
                scalFac: scalFac, loopMode: loopMode);


        /// <summary>
        /// Adds an additional cylindric phase to a uniformly sampled target data.
        /// </summary>
        /// <param name="x">Target data to modify (in/out).</param>
        /// <param name="grid">Sampling grid of the data.</param>
        /// <param name="z">Distance to the point source.</param>
        /// <param name="c">Cylindrical coefficient.</param>
        /// <param name="x0">Lateral shift (default is 0.0).</param>
        /// <param name="scalFac">Optional overall scaling factor for the result. Default is 1 (no scaling).</param>
        /// <param name="loopMode">Loop-computational mode options.</param>
        public static void AddCylindPhase(ref VectorZ x, GridInfo1D grid,
            double z, double c,
            double x0 = 0.0,
            Complex scalFac = default,
            LoopMode loopMode = Defaults.LoopOption)
            => AddPhase(v: ref x, grid: grid,
                f: Function1D.Cylindric, p: [z, c],
                scalFac: scalFac, loopMode: loopMode);


        /// <summary>
        /// Adds an additional linear phase to a uniformly sampled target data (Obsolete).
        /// </summary>
        /// <param name="x">Target data to modify (in/out).</param>
        /// <param name="grid">Sampling grid of the data.</param>
        /// <param name="a">Linear coefficient for x^1 term in the phase.</param>
        /// <param name="loopMode">Loop-computational mode options.</param>
        [Obsolete($"Linear phase does not need sampling in most cases")]
        internal static void AddLinearPhase(ref VectorZ x, GridInfo1D grid,
            double a, LoopMode loopMode = Defaults.LoopOption)
        {
            Samp1DRealFunc psi = new(f: (x) => Function1D.Linear(x, [a]));
            psi.AddTo(x: ref x, grid: grid, part: ComplexPart.Argument, loopMode: loopMode);
        }

        #endregion
        #region ---- 2D case ----

        /// <summary>
        /// Adds an additional phase to a complex matrix field sampled on a 2D grid.
        /// </summary>
        /// <param name="v">Reference to the complex matrix field to modify (in/out).</param>
        /// <param name="grid">Sampling grid of the complex field.</param>
        /// <param name="f">Function that defines the phase as a function of (x, y) and parameters.</param>
        /// <param name="p">Parameters used in the phase definition.</param>
        /// <param name="scalFac">Optional overall scaling factor for the result. Default is 1 (no scaling).</param>
        /// <param name="loopMode">Loop-computational mode options.</param>
        public static void AddPhase(ref MatrixZ v, GridInfo2D grid,
            Func<double, double, List<double>, double> f, List<double> p,
            Complex scalFac = default,
            LoopMode loopMode = Defaults.LoopOption)
        {
            // initializes scaling factor
            Complex a = scalFac == default ? Complex.One : scalFac;

            // define loop operation
            var t = v;
            void op(long iRow, long iCol)
            {
                (double y, double x) = grid[iRow, iCol];
                double psi = f(x, y, p);
                if (a == Complex.One)
                { t[iRow, iCol, false] *= Complex.Exp(Complex.ImaginaryOne * psi); }
                else
                { t[iRow, iCol, false] *= a * Complex.Exp(Complex.ImaginaryOne * psi); }
            }
            Loop2D loop = new(operation: op,
                rowStart: 0, rowEnd: grid.Rows,
                colStart: 0, colEnd: grid.Cols,
                rowStep: 1, colStep: 1);
            loop.Evaluate(mode: loopMode);
        }


        /// <summary>
        /// Adds an additional quadratic phase to the complex field.
        /// </summary>
        /// <param name="x">Complex field to modify (in/out).</param>
        /// <param name="grid">Sampling grid of the complex field.</param>
        /// <param name="c2x">Coefficient for x^2 term in the phase.</param>
        /// <param name="c2y">Coefficient for y^2 term in the phase.</param>
        /// <param name="scalFac">Optional overall scaling factor for the result.</param>
        /// <param name="loopMode">Loop-computational mode options.</param>
        public static void AddQuadPhase(ref MatrixZ x, GridInfo2D grid,
            double c2x, double c2y,
            Complex scalFac = default,
            LoopMode loopMode = Defaults.LoopOption)
            => AddPhase(v: ref x, grid: grid,
                f: Function2D.Quadratic, p: [c2x, c2y],
                scalFac: scalFac, loopMode: loopMode);


        /// <summary>
        /// Adds an additional spheric (cylindric) phase to the complex field.
        /// </summary>
        /// <param name="x">Complex field to modify (in/out).</param>
        /// <param name="grid">Sampling grid of the complex field.</param>
        /// <param name="z">Distance to the point source.</param>
        /// <param name="c">Spheric coefficient.</param>
        /// <param name="scalFac">Optional overall scaling factor for the result.</param>
        /// <param name="loopMode">Loop-computational mode options.</param>
        public static void AddSphericPhase(ref MatrixZ x, GridInfo2D grid,
            double z, double c,
            Complex scalFac = default,
            LoopMode loopMode = Defaults.LoopOption)
            => AddPhase(v: ref x, grid: grid,
                f: Function2D.Spheric, p: [z, c],
                scalFac: scalFac, loopMode: loopMode);


        /// <summary>
        /// Adds an additional linear phase to the complex field (Obsolete).
        /// </summary>
        /// <param name="x">Complex field to modify (in/out).</param>
        /// <param name="grid">Sampling grid of the complex field.</param>
        /// <param name="c1x">Coefficient for x^1 term in the phase.</param>
        /// <param name="c1y">Coefficient for y^1 term in the phase.</param>
        /// <param name="scalFac">Optional overall scaling factor for the result.</param>
        /// <param name="loopMode">Loop-computational mode options.</param>
        [Obsolete($"Linear phase does not need sampling in most cases")]
        internal static void AddLinearPhase(ref MatrixZ x, GridInfo2D grid,
            double c1x, double c1y,
            Complex scalFac = default,
            LoopMode loopMode = Defaults.LoopOption)
            => AddPhase(v: ref x, grid: grid,
                f: Function2D.Linear, p: [c1x, c1y],
                scalFac: scalFac, loopMode: loopMode);

        #endregion

    }

}
