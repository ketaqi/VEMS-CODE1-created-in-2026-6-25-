using System.Numerics;

namespace VEMS.AMathCore
{
    /// <summary>
    /// Provides functionality to compute finite-difference derivatives on a 1D grid
    /// for vectors of numeric type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">A numeric type that implements <see cref="INumber{T}"/>.</typeparam>
    public class GridFiDi1D<T> where T : INumber<T>
    {
        #region properties

        /// <summary>
        /// Gets or sets the number of grid points for which derivatives can be computed.
        /// This value is set by the constructor and used when evaluating all derivative values.
        /// </summary>
        private long Count { get; set; }

        /// <summary>
        /// Function delegate that computes the derivative at a given grid index.
        /// </summary>
        /// <remarks>
        /// The delegate takes a single <see cref="long"/> argument specifying the index
        /// in the underlying vector and returns the derivative value of type <typeparamref name="T"/>.
        /// The delegate may be null for an instance created by the internal default constructor.
        /// </remarks>
        public Func<long, T>? FindDerivative { get; set; }

        #endregion
        #region constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="GridFiDi1D{T}"/> class.
        /// </summary>
        /// <remarks>
        /// Internal default constructor intended for internal or test usage where the
        /// instance is configured after construction.
        /// </remarks>
        internal GridFiDi1D() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="GridFiDi1D{T}"/> class that
        /// computes derivatives for the provided vector using the specified grid and option.
        /// </summary>
        /// <param name="vs">Vector of function values stored on the grid.</param>
        /// <param name="grid">Optional uniform sampling grid corresponding to <paramref name="vs"/>.</param>
        /// <param name="option">Option specifying which derivative to compute (first or second order).</param>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="grid"/> is provided and its <see cref="GridInfo1D.Count"/>
        /// does not match <paramref name="vs"/>.<see cref="Vect{T}.Count"/>.
        /// </exception>
        public GridFiDi1D(Vect<T> vs,
            GridInfo1D? grid = null,
            FiDi1DOption option = FiDi1DOption.Dt)
        {
            if (grid != null && vs.Count != grid.Count) { throw new ArgumentException(); }

            Count = vs.Count;
            switch (option)
            {
                case FiDi1DOption.Dt:
                    FindDerivative = (i) => Dt(vs, i, grid, checkBound: false);
                    break;
                case FiDi1DOption.Dtt:
                    FindDerivative = (i) => Dtt(vs, i, grid, checkBound: false);
                    break;
                default: goto case FiDi1DOption.Dt;
            }
        }

        #endregion
        #region methods

        /// <summary>
        /// Evaluates derivative values at all grid locations using the configured delegate.
        /// </summary>
        /// <param name="loopMode">Loop computation mode used for parallelization or sequential execution.</param>
        /// <returns>
        /// A <see cref="Vect{T}"/> containing derivative values for every grid index (same length as the original vector).
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when the instance's <see cref="FindDerivative"/> delegate is null.</exception>
        public Vect<T> EvaluateAll(LoopMode loopMode = Defaults.LoopOption)
        {
            if (FindDerivative == null) { throw new ArgumentNullException(nameof(FindDerivative)); }

            // initialize
            Vect<T> ve = new(count: Count, initMode: ArrayInitMode.Malloc);
            // defines loop operation
            Loop1D loop = new(operation: (i) => ve[i, false] = FindDerivative(i),
                start: 0, end: Count, step: 1);
            loop.Evaluate(mode: loopMode);

            // return
            return ve;
        }

        #endregion
        #region static methods

        /// <summary>
        /// Computes the first-order finite difference (derivative) at a specific index
        /// for a generic vector type. Uses central difference except at the boundaries,
        /// where forward or backward difference is used.
        /// </summary>
        /// <param name="f">Input vector of values.</param>
        /// <param name="i">Target index for the derivative evaluation.</param>
        /// <param name="grid">Optional: Uniform sampling grid of the input values. If provided, the result is divided by the grid spacing.</param>
        /// <param name="checkBound">If true, checks if the index is out of bounds and returns zero if so.</param>
        /// <returns>The first-order derivative at the specified index.</returns>
        public static T Dt(Vect<T> f, long i,
            GridInfo1D? grid = null,
            bool checkBound = false)
        {
            T d;
            T two = T.CreateChecked(2.0);

            // outside bound?
            if (checkBound && (i < 0 || i >= f.Count))
                return T.Zero;

            // within bound and at the edges
            if (i == 0)
            {
                // forced to forward difference
                d = f[i + 1, false] - f[i, false];
            }
            else if (i == f.Count - 1) 
            {
                // forced to backward difference
                d = f[i, false] - f[i - 1, false];
            }
            else
            {
                // central difference
                d = (f[i + 1, false] - f[i - 1, false]) / two;
            }

            if (grid != null) 
            {
                T spacing = T.CreateChecked(grid.Spacing);
                d /= spacing;
            }

            return d;
        }

        /// <summary>
        /// Computes the second-order finite difference (second derivative) at a specific index
        /// for a generic vector type. Uses central difference except at the boundaries,
        /// where forward or backward difference is used.
        /// </summary>
        /// <typeparam name="T">The numeric type of the vector elements, must implement <see cref="INumber{T}"/>.</typeparam>
        /// <param name="f">Input vector of values.</param>
        /// <param name="i">Target index for the derivative evaluation.</param>
        /// <param name="grid">Optional: Uniform sampling grid of the input values. If provided, the result is divided by the square of the grid spacing.</param>
        /// <param name="checkBound">If true, checks if the index is out of bounds and returns zero if so.</param>
        /// <returns>The second-order derivative at the specified index.</returns>
        public static T Dtt(Vect<T> f, long i,
            GridInfo1D? grid = null,
            bool checkBound = false)
        {
            T d;
            T two = T.CreateChecked(2.0);

            // outside bound?
            if (checkBound && (i < 0 || i >= f.Count))
                return T.Zero;

            // within bound
            if (i == 0) 
            {
                // forced to forward difference
                d = f[i + 2, false] + f[i, false] - two * f[i + 1, false];
            }
            else if (i == f.Count - 1) 
            {
                // forced to backward difference
                d = f[i, false] + f[i - 2, false] - two * f[i - 1, false]; 
            }
            else
            { 
                // using central difference
                d = (f[i + 1, false] + f[i - 1, false] - two * f[i, false]); 
            }

            if (grid != null) 
            { 
                T spacing = T.CreateChecked(grid.Spacing);
                d /= spacing * spacing; 
            }

            return d;
        }

        #endregion
    }
}
