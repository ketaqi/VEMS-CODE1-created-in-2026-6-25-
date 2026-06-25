using System.Numerics;
using VEMS.MathCore.XTMethods;

namespace VEMS.MathCore
{

    /// <summary>
    /// Represents grid sampling information for 1D data.
    /// Provides properties and methods for describing and manipulating a uniform 1D grid,
    /// including grid point count, spacing, start position, and coordinate calculations.
    /// Supports grid modification, conjugation (for Fourier transforms), and equality operations.
    /// </summary>
    /// <remarks>
    /// This class is used to define the structure and properties of a 1D uniform sampling grid.
    /// </remarks>
    public class GridInfo1D : IEquatable<GridInfo1D>, 
        IEqualityOperators<GridInfo1D, GridInfo1D, bool>
    {
        #region properties

        /// <summary>
        /// Gets or sets the number of grid points in the 1D grid.
        /// </summary>
        /// <remarks>
        /// This property defines the total number of uniformly spaced points in the grid.
        /// </remarks>
        public long Count { get; set; }

        /// <summary>
        /// Gets or sets the spacing between two adjacent grid points.
        /// </summary>
        /// <remarks>
        /// This property defines the uniform distance between consecutive points in the grid.
        /// </remarks>
        public double Spacing { get; set; }

        /// <summary>
        /// Gets the total sampling range of the grid.
        /// </summary>
        /// <remarks>
        /// The range is calculated as the product of the number of grid points (<see cref="Count"/>) and the spacing between points (<see cref="Spacing"/>).
        /// </remarks>
        public double Range => Count * Spacing;

        /// <summary>
        /// Gets or sets the coordinate of the first grid point.
        /// </summary>
        /// <remarks>
        /// This property defines the coordinate value of the first point in the 1D grid.
        /// </remarks>
        public double Start { get; set; }

        /// <summary>
        /// Gets or sets the center coordinate of the grid.
        /// (Not necessarily on a grid point.)
        /// </summary>
        /// <remarks>
        /// The center is calculated as the midpoint of the grid's range, offset by half the spacing.
        /// </remarks>
        public double Center
        {
            get => Start + 0.5 * (Range - Spacing);
            set => Start = value - 0.5 * (Range - Spacing);
        }

        /// <summary>
        /// Gets the coordinate of the last grid point.
        /// </summary>
        /// <remarks>
        /// The end coordinate is calculated as <c>Start + Range - Spacing</c>,
        /// which gives the position of the last grid point in the 1D grid.
        /// </remarks>
        public double End => Start + Range - Spacing;

        /// <summary>
        /// Gets the lower bound of the full range of the grid.
        /// </summary>
        /// <remarks>
        /// The lower bound is calculated as the coordinate of the first grid point minus half the grid spacing.
        /// This represents the minimum value covered by the grid, including half-spacing extension.
        /// </remarks>
        public double LowerBound => Start - 0.5 * Spacing;

        /// <summary>
        /// Gets the upper bound of the full range of the grid.
        /// </summary>
        /// <remarks>
        /// The upper bound is calculated as the coordinate of the last grid point plus half the grid spacing.
        /// This represents the maximum value covered by the grid, including half-spacing extension.
        /// </remarks>
        public double UpperBound => Start + Range - 0.5 * Spacing;

        /// <summary>
        /// Gets the coordinate at the specified grid index.
        /// </summary>
        /// <param name="i">The index of the grid point.</param>
        /// <returns>The coordinate value at the specified index.</returns>
        public double this[long i]
        {
            get => GetCoordinate(i);
        }

        /// <summary>
        /// Gets the coordinate at the specified grid index.
        /// </summary>
        /// <param name="i">The index of the grid point.</param>
        /// <returns>The coordinate value at the specified index.</returns>
        public double this[int i]
        {
            get => this[(long)i];
        }

        /// <summary>
        /// Gets the coordinate at the specified grid index using a <see cref="System.Index"/>.
        /// </summary>
        /// <param name="i">
        /// The <see cref="System.Index"/> representing the grid point index. 
        /// If <see cref="Index.IsFromEnd"/> is true, the index is counted from the end of the grid.
        /// </param>
        /// <returns>
        /// The coordinate value at the specified index.
        /// </returns>
        public double this[Index i]
        {
            get => this[i.ToLong(Count)];
        }

        #endregion
        #region constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="GridInfo1D"/> class with default values.
        /// </summary>
        /// <remarks>
        /// The default grid has a single point at coordinate 0.0 with a spacing of 1.0.
        /// </remarks>
        public GridInfo1D()
        {
            Count = 1;
            Spacing = 1.0;
            Start = 0.0;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GridInfo1D"/> class
        /// with the specified number of grid points, grid spacing, and reference point.
        /// The reference point can be interpreted as either the start or the center of the grid,
        /// depending on the <paramref name="refType"/> parameter.
        /// </summary>
        /// <param name="n">The number of grid points.</param>
        /// <param name="spacing">The spacing between grid points.</param>
        /// <param name="refPoint">The location of the reference point for the grid.</param>
        /// <param name="refType">
        /// The type of reference point. If <see cref="GridRefType.Start"/>, <paramref name="refPoint"/> is the coordinate of the first grid point.
        /// If <see cref="GridRefType.Center"/>, <paramref name="refPoint"/> is the center coordinate of the grid.
        /// </param>
        public GridInfo1D(long n, double spacing, double refPoint,
            GridRefType refType = GridRefType.Start)
        {
            Count = n;
            Spacing = spacing;
            switch (refType)
            {
                case GridRefType.Start:
                    Start = refPoint;
                    break;
                case GridRefType.Center:
                    Start = refPoint - 0.5 * (Count - 1) * Spacing;
                    break;
                case GridRefType.LowerBound:
                    Start = refPoint + 0.5 * Spacing;
                    break;
                case GridRefType.UpperBound:
                    Start = refPoint - Range + 0.5 * Spacing;
                    break;
                default: goto case GridRefType.Start;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GridInfo1D"/> class
        /// with the specified number of grid points and grid spacing.
        /// The grid is centered around zero by default.
        /// </summary>
        /// <param name="n">The number of grid points.</param>
        /// <param name="spacing">The spacing between grid points.</param>
        public GridInfo1D(long n, double spacing)
            : this(n: n, spacing: spacing, refPoint: 0.0,
                  refType: GridRefType.Center)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="GridInfo1D"/> class
        /// with the specified number of grid points.
        /// The grid is centered around zero and the grid spacing is set to 1.0 by default.
        /// </summary>
        /// <param name="n">The number of grid points.</param>
        public GridInfo1D(long n)
            : this(n: n, spacing: 1.0)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="GridInfo1D"/> class by deep copying from another instance.
        /// </summary>
        /// <param name="other">The <see cref="GridInfo1D"/> instance to copy from.</param>
        public GridInfo1D(GridInfo1D other)
        {
            Count = other.Count;
            Spacing = other.Spacing;
            Start = other.Start;
        }

        /// <summary>
        /// constructs a GridInfo1D
        /// with given number of points, grid spacing
        /// and start coordinate
        /// </summary>
        /// <param name="n"> number of grid points </param>
        /// <param name="start"> first grid point </param>
        /// <param name="spacing"> grid spacing </param>
        [Obsolete($"Use the constructor with GridRefType option")]
        public GridInfo1D(long n, double start, double spacing)
        {
            Count = n;
            Spacing = spacing;
            Start = start;
        }

        #endregion
        #region methods

        #region ---- coordinate ----

        /// <summary>
        /// gets the coordinate of the grid point
        /// with given index i
        /// Gets the coordinate of the grid point with the given index.
        /// </summary>
        /// <param name="i">The grid point index.</param>
        /// <returns>The coordinate value at the specified index.</returns>
        public double GetCoordinate(long i)
            => (Start + i * Spacing);

        /// <summary>
        /// Computes the coordinates for all the grid points in the 1D grid.
        /// </summary>
        /// <remarks>
        /// This method generates a <see cref="VectorD"/> containing the coordinates of each grid point,
        /// starting from <see cref="Start"/> and incrementing by <see cref="Spacing"/> for each subsequent point.
        /// The number of points is determined by <see cref="Count"/>.
        /// </remarks>
        /// <returns>
        /// A <see cref="VectorD"/> containing the coordinates of all grid points in the 1D grid.
        /// </returns>
        public VectorD GetCoordinates()
            => new(count: Count, initVal: Start, increment: Spacing);

        /// <summary>
        /// Finds the index of the grid span that contains the specified coordinate.
        /// </summary>
        /// <param name="x">
        /// The coordinate value to locate. If <paramref name="periodic"/> is <c>true</c> and <paramref name="x"/> is outside the grid range, 
        /// <paramref name="x"/> will be shifted back within the grid's range.
        /// </param>
        /// <param name="periodic">
        /// If <c>true</c>, assumes periodic boundary conditions and wraps <paramref name="x"/> into the grid's range.
        /// If <c>false</c>, returns <c>-1</c> if <paramref name="x"/> is outside the grid's range.
        /// </param>
        /// <returns>
        /// The index of the grid span containing <paramref name="x"/>. 
        /// Returns <c>-1</c> if <paramref name="x"/> is outside the grid's range and <paramref name="periodic"/> is <c>false</c>.
        /// </returns>
        public long FindGridSpan(ref double x,
            bool periodic = false)
        {
            // calculates the signed multiple
            long m = (long)Math.Floor((x - LowerBound) / Range);
            // updates local distance and finds the grid
            double xp = x - m * Range;
            if (m == 0 || periodic) // within range or periodic boundary
            { x = xp; return (long)((x - LowerBound) / Spacing); }
            else // outside range and no periodic boundary
            { return -1; }
        }

        #endregion
        #region ---- conjugate ----

        /// <summary>
        /// Updates the grid information to its conjugated domain after a Fourier transform,
        /// taking into account the shift theorem and linear phase.
        /// </summary>
        /// <param name="isForward">
        /// Indicates whether the transform is forward (<c>true</c>) or backward (<c>false</c>).
        /// </param>
        /// <param name="cLinear">
        /// The linear phase coefficient. On input, this is the linear phase in the original domain.
        /// On output, it is updated to the linear phase in the conjugated domain.
        /// </param>
        public void GetConjugated(bool isForward, 
            ref double cLinear)
        {
            // number of samples remains the same
            long n = Count;
            // spacing in the conjugated domain
            double spacing = 2.0 * Math.PI / Range;

            // finds the center in the conjugated domain, first
            double center = isForward ? +cLinear : -cLinear;
            // updates the linear phase factor, then
            cLinear = isForward ? -Center : +Center;

            // updates the output grid
            Spacing = spacing;
            Start = center - 0.5 * (n - 1) * Spacing;
        }

        /// <summary>
        /// Updates the grid information to its conjugated domain after a Fourier transform,
        /// taking into account the shift theorem and linear phase.
        /// </summary>
        /// <param name="isForward">
        /// Indicates whether the transform is forward (<c>true</c>) or backward (<c>false</c>).
        /// </param>
        public void GetConjugated(bool isForward)
        {
            double cLinear = 0.0;
            GetConjugated(isForward, ref cLinear);
        }

        /// <summary>
        /// gets the updated grid information in the conjugated domain
        /// after performing transform, with shift theorem considered
        /// </summary>
        /// <param name="option"> Fourier transform option: forawrd or backward </param>
        /// <param name="linearPhaseFactor"> linear phase factor (in/out) </param>
        [Obsolete]
        public void GetConjugated(FTOption option,
            ref double linearPhaseFactor)
        {
            // number of samples remains the same
            long n = Count;
            // spacing in the conjugated domain
            double spacing = 2.0 * Math.PI / Range;

            // finds the center in the conjugated domain, first
            double center = (option == FTOption.Forward) ?
                linearPhaseFactor : -linearPhaseFactor;
            // updates the linear phase factor, then
            linearPhaseFactor = (option == FTOption.Forward) ?
                -Center : Center;

            // updates the output grid
            Spacing = spacing;
            Start = center - 0.5 * (n - 1) * Spacing;
        }

        /// <summary>
        /// gets the updated grid information in the conjugated domain
        /// after performing transform, with shift theorem considered
        /// </summary>
        /// <param name="option"> Fourier transform option: forawrd or backward </param>
        [Obsolete]
        public void GetConjugated(FTOption option)
        {
            double linearPhaseFactor = 0.0;
            GetConjugated(option, ref linearPhaseFactor);
        }

        #endregion
        #region ---- modify ----

        /// <summary>
        /// Modifies the sampling grid according to the specified scaling and shift parameters.
        /// </summary>
        /// <param name="rngFactor">
        /// Sampling range scaling factor. The total range of the grid will be multiplied by this value.
        /// </param>
        /// <param name="spdFactor">
        /// Sampling distance (spacing) scaling factor. The spacing between grid points will be multiplied by this value.
        /// </param>
        /// <param name="ctrShift">
        /// Center shift value. The center of the grid will be shifted by this value.
        /// </param>
        public void GetModified(double rngFactor = 1.0,
            double spdFactor = 1.0,
            double ctrShift = 0.0)
        {
            // updates properties
            // first, sampling distance 
            if (spdFactor != 1.0)
            { Spacing *= spdFactor; }
            // then, range can be larger
            if (spdFactor != 1.0 || rngFactor != 1.0)
            { Count = (long)Math.Ceiling(Range * rngFactor / Spacing); }
            // and, finds start point ...
            if (spdFactor != 1.0 || rngFactor != 1.0 || ctrShift != 0.0)
            { Start = Center + ctrShift - 0.5 * (Count - 1) * Spacing; }
        }

        /// <summary>
        /// Generates a modified sampling grid according to the specified scaling and shift parameters.
        /// </summary>
        /// <param name="rngFactor">
        /// Sampling range scaling factor. The total range of the grid will be multiplied by this value.
        /// </param>
        /// <param name="spdFactor">
        /// Sampling distance (spacing) scaling factor. The spacing between grid points will be multiplied by this value.
        /// </param>
        /// <param name="ctrShift">
        /// Center shift value. The center of the grid will be shifted by this value.
        /// </param>
        /// <returns>
        /// A new <see cref="GridInfo1D"/> instance representing the modified sampling grid.
        /// </returns>
        public GridInfo1D Modify(double rngFactor = 1.0,
            double spdFactor = 1.0, double ctrShift = 0.0)
        {
            GridInfo1D g = new(other: this);
            g.GetModified(rngFactor, spdFactor, ctrShift);
            return g;
        }

        #endregion
        #region ---- equality ----

        /// <summary>
        /// Determines whether the current <see cref="GridInfo1D"/> instance is equal to another <see cref="GridInfo1D"/> instance.
        /// </summary>
        /// <param name="other">The other <see cref="GridInfo1D"/> instance to compare with this instance.</param>
        /// <returns>
        /// <c>true</c> if the specified <see cref="GridInfo1D"/> is equal to the current instance; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(GridInfo1D? other)
        {
            if (ReferenceEquals(this, other)) { return true; }
            if (other is null) { return false; }
            // use BitConverter.DoubleToInt64Bits for exact bitwise comparison of doubles
            return Count == other.Count
                && BitConverter.DoubleToInt64Bits(Start) == BitConverter.DoubleToInt64Bits(other.Start)
                && BitConverter.DoubleToInt64Bits(Spacing) == BitConverter.DoubleToInt64Bits(other.Spacing);
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current <see cref="GridInfo1D"/> instance.
        /// </summary>
        /// <param name="obj">The object to compare with the current <see cref="GridInfo1D"/> instance.</param>
        /// <returns>
        /// <c>true</c> if the specified object is a <see cref="GridInfo1D"/> and is equal to the current instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object? obj)
        {
            return Equals(obj as GridInfo1D);
        }

        #endregion
        #region ---- misc ----

        /// <summary>
        /// Returns a hash code for the current <see cref="GridInfo1D"/> instance.
        /// </summary>
        /// <remarks>
        /// The hash code is computed using the number of grid points, the bitwise representation of the start coordinate,
        /// and the bitwise representation of the grid spacing. This ensures that grids with identical properties produce the same hash code.
        /// </remarks>
        /// <returns>
        /// An integer hash code representing the current <see cref="GridInfo1D"/> instance.
        /// </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + Count.GetHashCode();
                hash = hash * 23 + BitConverter.DoubleToInt64Bits(Start).GetHashCode();
                hash = hash * 23 + BitConverter.DoubleToInt64Bits(Spacing).GetHashCode();
                return hash;
            }
        }

        #endregion
        #region ---- unit scale ----

        /// <summary>
        /// Returns a new <see cref="GridInfo1D"/> instance with the grid spacing scaled by the specified factor.
        /// The reference point for scaling can be specified using <paramref name="refType"/>.
        /// </summary>
        /// <param name="scale">The scale factor to apply to the grid spacing. If 1.0, the current instance is returned.</param>
        /// <param name="refType">
        /// The type of reference point to use for scaling. 
        /// <see cref="GridRefType.Start"/> uses the start coordinate, 
        /// <see cref="GridRefType.Center"/> uses the center coordinate, 
        /// <see cref="GridRefType.LowerBound"/> uses the lower bound, 
        /// <see cref="GridRefType.UpperBound"/> uses the upper bound.
        /// </param>
        /// <returns>
        /// A new <see cref="GridInfo1D"/> instance with the spacing scaled by <paramref name="scale"/> and the reference point preserved as specified.
        /// If <paramref name="scale"/> is 1.0, the current instance is returned.
        /// </returns>
        public GridInfo1D Scale(double scale,
            GridRefType refType = GridRefType.Center)
        {          
            double refPoint = refType switch
            {
                GridRefType.Start => Start,
                GridRefType.Center => Center,
                GridRefType.LowerBound => LowerBound,
                GridRefType.UpperBound => UpperBound,
                _ => Center // default case
            };
            
            return new GridInfo1D(n: Count, spacing: Spacing * scale,
                refPoint: refPoint, refType: refType);
        }

        #endregion

        #endregion
        #region operators

        #region ---- explicit ----

        /// <summary>
        /// Performs an explicit conversion from <see cref="GridInfo1D"/> to <see cref="ScatInfo1D"/>.
        /// </summary>
        /// <param name="g">The <see cref="GridInfo1D"/> instance to convert.</param>
        /// <returns>
        /// A <see cref="ScatInfo1D"/> instance containing the coordinates of the grid points from <paramref name="g"/>.
        /// </returns>
        public static explicit operator ScatInfo1D(GridInfo1D g)
            => new (xs: g.GetCoordinates());

        #endregion
        #region ---- equality ----

        /// <summary>
        /// Determines whether two <see cref="GridInfo1D"/> instances are equal.
        /// </summary>
        /// <param name="left">The first <see cref="GridInfo1D"/> instance to compare, or <see langword="null"/>.</param>
        /// <param name="right">The second <see cref="GridInfo1D"/> instance to compare, or <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if the two instances are equal or both are <see langword="null"/>;  otherwise, <see
        /// langword="false"/>.</returns>
        public static bool operator ==(GridInfo1D? left, GridInfo1D? right)
        {
            if (ReferenceEquals(left, right)) { return true; }
            if (left is null || right is null) { return false; }
            return left.Equals(right);
        }

        /// <summary>
        /// Determines whether two <see cref="GridInfo1D"/> instances are not equal.
        /// </summary>
        /// <param name="left">The first <see cref="GridInfo1D"/> instance to compare, or <see langword="null"/>.</param>
        /// <param name="right">The second <see cref="GridInfo1D"/> instance to compare, or <see langword="null"/>.</param>
        /// <returns>
        /// <see langword="true"/> if the two instances are not equal; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool operator !=(GridInfo1D? left, GridInfo1D? right)
            => !(left == right);

        #endregion
        #region ---- unit scale ----

        /// <summary>
        /// Multiplies the grid spacing of a <see cref="GridInfo1D"/> instance by a specified scale factor.
        /// The resulting grid will have the same number of points and center coordinate, but the spacing between points will be scaled.
        /// </summary>
        /// <param name="scale">The scale factor to apply to the grid spacing.</param>
        /// <param name="g">The <see cref="GridInfo1D"/> instance to scale.</param>
        /// <returns>
        /// A new <see cref="GridInfo1D"/> instance with the spacing scaled by <paramref name="scale"/> and the same number of points and center coordinate as <paramref name="g"/>.
        /// If <paramref name="scale"/> is 1.0, the original instance <paramref name="g"/> is returned.
        /// </returns>
        public static GridInfo1D operator *(GridInfo1D g, double scale)
        {
            if (scale == 1.0) { return g; }
            //return new GridInfo1D(n: g.Count, spacing: g.Spacing * scale,
            //    refPoint: g.Center, refType: GridRefType.Center);
            return g.Scale(scale);
        }

        /// <summary>
        /// Multiplies the grid spacing of a <see cref="GridInfo1D"/> instance by a specified scale factor.
        /// The resulting grid will have the same number of points and center coordinate, but the spacing between points will be scaled.
        /// </summary>
        /// <param name="scale">The scale factor to apply to the grid spacing.</param>
        /// <param name="g">The <see cref="GridInfo1D"/> instance to scale.</param>
        /// <returns>
        /// A new <see cref="GridInfo1D"/> instance with the spacing scaled by <paramref name="scale"/> and the same number of points and center coordinate as <paramref name="g"/>.
        /// If <paramref name="scale"/> is 1.0, the original instance <paramref name="g"/> is returned.
        /// </returns>

        public static GridInfo1D operator *(double scale, GridInfo1D g)
            => g * scale;

        #endregion

        #endregion
    }

}
