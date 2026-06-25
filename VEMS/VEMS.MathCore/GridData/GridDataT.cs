using System.Numerics;

namespace VEMS.MathCore
{
    /// <summary>
    /// One-dimensionla real-valued grid gata
    /// </summary>
    public abstract class Grid1DData<T> where T : INumber<T>
    {
        #region properties

        /// <summary>
        /// data values in a vector
        /// </summary>
        public Vect<T> Values { get; set; }

        /// <summary>
        /// sampling grid information
        /// </summary>
        public GridInfo1D GridInfo { get; set; }

        /// <summary>
        /// data boundary option: periodic or zero
        /// </summary>
        public DataBoundary Boundary { get; set; }

        ///// <summary>
        ///// internal interpolation of the data
        ///// </summary>
        //public Grid1DRealInterpolation Interpolation { get; set; }

        /// <summary>
        /// interpolation method used for continuation
        /// </summary>

        public InterpolationMethod IntrplMethod
        {
            get; set;
            //get => Interpolation.Method;
            //[MemberNotNull(nameof(Interpolation))]
            //set => Interpolation = new(v: Values, grid: GridInfo,
            //    method: value, bound: Boundary);
        }

        #endregion
        #region constructors

        /// <summary>
        /// constructs a default Grid1DRealData
        /// with one value of 0.0
        /// </summary>
        internal Grid1DData()
        {
            GridInfo = new();
            Values = new(count: 1);
            Boundary = Defaults.BoundaryOption;
            IntrplMethod = Defaults.IntrplOption;
        }

        /// <summary>
        /// constructs a Grid1DRealData with given
        /// sampling grid and data values without 
        /// deep copy of the actual data
        /// </summary>
        /// <param name="values"> data values in a vector </param>
        /// <param name="gridInfo"> sampling grid of the data </param>
        /// <param name="intrpl"> interpolation method </param>
        /// <param name="bound"> data boundary option </param>
        public Grid1DData(Vect<T> values, GridInfo1D? gridInfo = null,
            InterpolationMethod intrpl = Defaults.IntrplOption,
            DataBoundary bound = Defaults.BoundaryOption)
        {
            if (gridInfo != null && gridInfo.Count != values.Count)
            { throw new ArgumentException($"Unequal number of input sampling"); }

            Values = values;
            GridInfo = gridInfo ?? new(n: values.Count);
            Boundary = bound;
            IntrplMethod = intrpl;
        }


        #endregion
        #region methods

        /// <summary>
        /// evaluates value at a single location  
        /// </summary>
        /// <param name="x"> location for evaluation </param>
        /// <returns> interpolated value on the evaluation location </returns>
        public abstract double FindValue(double x);

        #endregion
    }
}
