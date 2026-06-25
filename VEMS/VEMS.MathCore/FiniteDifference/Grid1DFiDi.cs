using System.Numerics;

namespace VEMS.MathCore
{

    /// <summary>
    /// 
    /// </summary>
    public class Grid1DFiDi<T> where T : INumber<T>
    {
        #region properties

        private long Count { get; set; }

        /// <summary>
        /// finds derivative value at specific grid index 
        /// <para> arg #1: index that specifies a grid location </para>
        /// <para> result: derivative value at this location </para>
        /// </summary>
        public Func<long, T>? FindDerivative { get; set; }

        #endregion
        #region constructors

        /// <summary>
        /// default constructor
        /// </summary>
        internal Grid1DFiDi() { }

        /// <summary>
        /// constructs a Grid1DRealFiDi class
        /// </summary>
        /// <param name="vs"> function values stored in a vector </param>
        /// <param name="grid"> sampling grid of the function values </param>
        /// <param name="option"> derivative option </param>
        public Grid1DFiDi(Vect<T> vs,
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
        /// evaluates the derivative values at
        /// all the grid locations 
        /// </summary>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> resulting derivative values at all grid locations </returns>
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
        #region pointwise static methods

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
            if (checkBound)
            { if (i < 0 || i >= f.Count) { d = T.Zero; } }
            // within bound and at the edges
            if (i == 0) // forced to forward difference
            { d = f[i + 1, false] - f[i, false]; }
            else if (i == f.Count - 1) // forced to backward difference
            { d = f[i, false] - f[i - 1, false]; }
            else
            { d = (f[i + 1, false] - f[i - 1, false]) / two; }

            if (grid != null) { d /= T.CreateChecked(grid.Spacing); }
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
            if (checkBound)
            { if (i < 0 || i >= f.Count) { d = T.Zero; } }
            // within bound
            if (i == 0) // forced to forward difference
            { d = f[i + 2, false] + f[i, false] - two * f[i + 1, false]; }
            else if (i == f.Count - 1) // forced to backward difference
            { d = f[i, false] + f[i - 2, false] - two * f[i - 1, false]; }
            else // using central difference
            { d = (f[i + 1, false] + f[i - 1, false] - two * f[i, false]); }

            if (grid != null) { d /= T.CreateChecked(grid.Spacing * grid.Spacing); }
            return d;
        }

        #endregion
    }


    internal class Grid1DRealFD : Grid1DFiDi<double>
    {
        
    }

    internal class Grid1DCplxFD : Grid1DFiDi<Cplx>
    {
        
    }

}
