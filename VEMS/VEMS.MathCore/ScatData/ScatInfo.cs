namespace VEMS.MathCore
{

    /// <summary>
    /// scattered location information for 1D data
    /// </summary>
    public class ScatInfo1D
    {
        #region properties

        /// <summary>
        /// internally stored scattered points
        /// </summary>
        internal VectorD Xs { get; set; }

        /// <summary>
        /// number of scattered points
        /// </summary>
        public long Count => Xs.Count;

        /// <summary>
        /// gets / sets the coordinate at given index
        /// </summary>
        /// <param name="i"> index </param>
        /// <param name="checkBound"> whether to check if the index is outside bound </param>
        /// <returns> coordinate at this index </returns>
        public double this[long i, bool checkBound = true]
        {
            get => Xs[i, checkBound];
            set => Xs[i, checkBound] = value;
        }

        #endregion
        #region constructor

        /// <summary>
        /// constructs a ScatInfo1D with given scattered points
        /// </summary>
        /// <param name="xs"> scattered points </param>
        public ScatInfo1D(VectorD xs)
        {
            Xs = xs;
        }

        /// <summary>
        /// constructs a ScatInfo1D with given count of points
        /// </summary>
        /// <param name="count"> count of scattered points </param>
        public ScatInfo1D(long count)
        {
            Xs = new (count);
        }

        #endregion
    }


    /// <summary>
    /// scattered location information for XY-separable data
    /// </summary>
    public class ScatInfoXY
    {
        #region properties

        /// <summary>
        /// internally stored scattered x-coordinates
        /// </summary>
        internal VectorD Xs { get; set; }

        /// <summary>
        /// internally stored scattered y-coordinates
        /// </summary>
        internal VectorD Ys { get; set; }

        /// <summary>
        /// number of rows along y-direction
        /// </summary>
        public long Rows => Ys.Count;

        /// <summary>
        /// number of columns along x-direction
        /// </summary>
        public long Cols => Xs.Count;

        /// <summary>
        /// gets the (y, x)-coordinate at given 
        /// row and column index-pair
        /// </summary>
        /// <param name="iRow"> row index </param>
        /// <param name="iCol"> column index </param>
        /// <param name="checkBound"> whether to check if the index is outside bound </param>
        /// <returns> (y, x)-coordinate </returns>
        public (double, double) this[long iRow, long iCol, bool checkBound = true]
        {
            get => (Ys[iRow, checkBound], Xs[iCol, checkBound]);
        }

        /// <summary>
        /// gets/ sets either the y-coordinate at given row index
        /// or the x-coordinate at given column index
        /// </summary>
        /// <param name="i"> index of either row or column </param>
        /// <param name="isRowIndex"> flag whether the index is for row or column </param>
        /// <param name="checkBound"> whether to check if the index is outside bound </param>
        /// <returns> y- or x-coordinate </returns>
        public double this[long i, bool isRowIndex, bool checkBound = true]
        {
            get => isRowIndex ? Ys[i, checkBound] : Xs[i, checkBound];
            set
            {
                if (isRowIndex) { Ys[i, checkBound] = value; }
                else { Xs[i, checkBound] = value; }
            }
        }

        #endregion
        #region constructor

        /// <summary>
        /// constructs a ScatInfoXY with given scattered points
        /// along y- and x-direction respectively
        /// </summary>
        /// <param name="ys"> scattered y-coordinates </param>
        /// <param name="xs"> scattered x-coordinates </param>
        public ScatInfoXY(VectorD ys, VectorD xs)
        {
            Ys = ys;
            Xs = xs;
        }

        /// <summary>
        /// constructs a ScatInfoXY with given number of
        /// rows and columns respectively i.e. the number of 
        /// scattered points along y- and x-direction
        /// </summary>
        /// <param name="rows"> number of rows </param>
        /// <param name="cols"> number of columns </param>
        public ScatInfoXY(long rows, long cols)
        {
            Ys = new(rows);
            Xs = new(cols);
        }

        #endregion
        #region methods

        /// <summary>
        /// gets the ScatInfo1D along x direction
        /// </summary>
        /// <returns></returns>
        public ScatInfo1D ScatXInfo()
            => new(xs: Xs);

        /// <summary>
        /// gets the ScatInfo1D along y direction
        /// </summary>
        /// <returns></returns>
        public ScatInfo1D ScatYInfo()
            => new(xs: Ys);

        #endregion
    }

    /// <summary>
    /// scattered location information for general 2D data
    /// </summary>
    public class ScatInfo2D
    {
        #region properties

        /// <summary>
        /// internally stored points' x-coordinates
        /// </summary>
        internal VectorD Xs { get; set; }

        /// <summary>
        /// internally stored points' y-coordinates
        /// </summary>
        internal VectorD Ys { get; set; }

        /// <summary>
        /// number of scattered points
        /// </summary>
        public long Count
        {
            get => (Xs.Count == Ys.Count) ? Xs.Count : 0;
        }

        /// <summary>
        /// gets / sets the (y, x)-coordinates of a given index
        /// </summary>
        /// <param name="i"> index </param>
        /// <param name="checkBound"> whether to check if the index is outside bound </param>
        /// <returns> (y, x)-coordinates </returns>
        public (double, double) this[long i, bool checkBound = true]
        {
            get => (Ys[i, checkBound], Xs[i, checkBound]);
            set => (Ys[i, checkBound], Xs[i, checkBound]) = value;
        }

        #endregion
        #region constructor

        /// <summary>
        /// constructs a ScatInfo2D with given scattered points
        /// </summary>
        /// <param name="ys"> x-coordinates of the scattered points </param>
        /// <param name="xs"> y-coordinates of the scattered points </param>
        public ScatInfo2D(VectorD ys, VectorD xs)
        {
            if(ys.Count != xs.Count) { throw new ArgumentException(); }
            Ys = ys;
            Xs = xs;
        }

        /// <summary>
        /// constructs a ScatInfo2D with given number of
        /// scattered points 
        /// </summary>
        /// <param name="count"> number of scattered points </param>
        public ScatInfo2D(long count)
        {
            Ys = new(count);
            Xs = new(count);
        }

        #endregion
    }
}
