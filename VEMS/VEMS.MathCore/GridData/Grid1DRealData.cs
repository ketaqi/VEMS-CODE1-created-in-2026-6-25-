using System.Diagnostics.CodeAnalysis;

namespace VEMS.MathCore
{

    /// <summary>
    /// One-dimensionla real-valued grid gata
    /// </summary>
    public class Grid1DRealData
    {
        #region properties

        /// <summary>
        /// data values in a vector
        /// </summary>
        public VectorD Values { get; set; }

        /// <summary>
        /// sampling grid information
        /// </summary>
        public GridInfo1D GridInfo { get; set; }

        /// <summary>
        /// data boundary option: periodic or zero
        /// </summary>
        public DataBoundary Boundary { get; set; }

        /// <summary>
        /// internal interpolation of the data
        /// </summary>
        public Grid1DRealInterpolation Interpolation { get; set; }

        /// <summary>
        /// interpolation method used for continuation
        /// </summary>

        public InterpolationMethod IntrplMethod 
        {
            get => Interpolation.Method;
            [MemberNotNull(nameof(Interpolation))]
            set => Interpolation = new(v: Values, grid: GridInfo,
                method: value, bound: Boundary);
        }

        #endregion
        #region constructors

        /// <summary>
        /// constructs a default Grid1DRealData
        /// with one value of 0.0
        /// </summary>
        public Grid1DRealData()
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
        public Grid1DRealData(VectorD values, GridInfo1D? gridInfo = null,
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

        /// <summary>
        /// constructs a Grid1DRealData with given count, 
        /// start and spacing, with a given initial value
        /// </summary>
        /// <param name="count"> number of elements </param>
        /// <param name="start"> first grid point coordinate </param>
        /// <param name="spacing"> grid spacing </param>
        /// <param name="initVal"> the common initial value </param>
        /// <param name="intrpl"> interpolation method </param>
        /// <param name="bound"> data boundary option </param>
        public Grid1DRealData(long count, double start, double spacing,
            double initVal,
            InterpolationMethod intrpl = Defaults.IntrplOption,
            DataBoundary bound = Defaults.BoundaryOption)
        {
            GridInfo = new(n: count, start: start, spacing: spacing);
            Values = new(count, initVal);
            Boundary = bound;
            IntrplMethod = intrpl;
        }

        /// <summary>
        /// constructs a Grid1DRealData with given count
        /// with a given initial value
        /// </summary>
        /// <param name="count"> number of elements </param>
        /// <param name="spacing"> grid spacing </param>
        /// <param name="initVal"> the common initial value </param>
        /// <param name="intrpl"> interpolation method </param>
        /// <param name="bound"> data boundary option </param>
        public Grid1DRealData(long count, double spacing,
            double initVal, 
            InterpolationMethod intrpl = Defaults.IntrplOption,
            DataBoundary bound = Defaults.BoundaryOption)
        {
            GridInfo = new(n: count, spacing: spacing);
            Values = new(count: count, initVal: initVal);
            Boundary = bound;
            IntrplMethod = intrpl;
        }

        /// <summary>
        /// constructs a Grid1DRealData with given count
        /// with a given initial value
        /// </summary>
        /// <param name="count"> number of elements </param>
        /// <param name="initVal"> the common initial value </param>
        /// <param name="intrpl"> interpolation method </param>
        /// <param name="bound"> data boundary option </param>
        public Grid1DRealData(long count, double initVal,
            InterpolationMethod intrpl = Defaults.IntrplOption,
            DataBoundary bound = Defaults.BoundaryOption)
        {
            GridInfo = new(n: count);
            Values = new(count: count, initVal: initVal);
            Boundary = bound;
            IntrplMethod = intrpl;
        }

        #endregion
        #region methods

        #region ----- Extension -----

        /// <summary>
        /// vector continue with the values of edge points
        /// Or peiodic extension to both two direction
        /// </summary>
        /// <param name="n"> number of extension points, Must be EVEN! </param>
        /// <param name="periodize"> true if periodic extension, false if continuous extension </param>
        /// <returns> return periodic extension Grid1DCplxData, if true; otherwise return continuous GridVectorZ </returns>
        public Grid1DRealData? Extension(long n, bool periodize)
        {
            if (n % 2 != 0)
            {
                Console.WriteLine("Extension number n must be EVEN!");
                return null;
            }
            else if (n == 0)
                return this;
            else
            {
                long uniNum = GridInfo.Count;
                GridInfo1D gridInfo = new(uniNum + n, GridInfo.Start - n / 2 * GridInfo.Spacing, GridInfo.Spacing);
                VectorD gridValues = new(uniNum + n, 0.0);


                // periodic extension along both direction
                if (periodize == true)
                {
                    //// copy the value periodically
                    ////-------------- method 1, time-consuming...old version
                    //long index = 1 + (long)Math.Ceiling((double)n / uniNum) * 2;
                    //for (long i = 0; i < gridInfo.Count; i++)
                    //{
                    //    for (long j = 1 - (index - 1) / 2; j <= 1 + (index - 1) / 2; j++)
                    //    {
                    //        if (n / 2 + (j - 1) * uniNum <= i && i <= n / 2 + j * uniNum - 1)
                    //        {
                    //            gridValues[i] = Values[i - n / 2 - (j - 1) * uniNum];
                    //            break;
                    //        }
                    //    }
                    //}
                    ////--------------

                    ////-------------- method 2, speed up
                    // calculate period times
                    long times = (long)Math.Ceiling((double)(n + uniNum) / uniNum);
                    if (times % 2 == 0)
                        times++;
                    long extra = n / 2 - (n / 2 / uniNum) * uniNum; // extra points in the first and last period
                    // copy the value periodically
                    long index = 1 + (long)Math.Ceiling((double)n / uniNum) * 2;
                    for (long i = 0; i < times; i++)
                    {
                        if (i == 0)
                        {
                            for (long j = 0; j < extra; j++)
                            {
                                for (long p = 1 - (index - 1) / 2; p <= 1 + (index - 1) / 2; p++)
                                {
                                    if (n / 2 + (p - 1) * uniNum <= j && j <= n / 2 + p * uniNum - 1)
                                    {
                                        gridValues[j] = Values[j - n / 2 - (p - 1) * uniNum];
                                        break;
                                    }
                                }
                            }
                        }
                        else if (i == (times - 1))
                        {
                            for (long j = extra + (i - 1) * uniNum; j < gridInfo.Count; j++)
                            {
                                for (long p = 1 - (index - 1) / 2; p <= 1 + (index - 1) / 2; p++)
                                {
                                    if (n / 2 + (p - 1) * uniNum <= j && j <= n / 2 + p * uniNum - 1)
                                    {
                                        gridValues[j] = Values[j - n / 2 - (p - 1) * uniNum];
                                        break;
                                    }
                                }
                            }
                        }
                        else
                        {
                            LongRange periodIndex = new(extra + (i - 1) * uniNum, extra + i * uniNum);
                            gridValues[periodIndex] = Values;
                        }
                    }
                    //--------------
                }

                // continuation the end points along both direction
                else
                {
                    //-------------- method 1, speed up
                    LongRange rangeFront = new(n / 2, GridInfo.Count + (n / 2));
                    gridValues[rangeFront] = Values;
                    LongRange rangeMiddle = new(GridInfo.Count + n / 2, gridValues.Count);
                    gridValues[rangeMiddle] = new VectorD(n / 2, Values[^1]);
                    LongRange rangeBehind = new(0, n / 2);
                    gridValues[rangeBehind] = new VectorD(n / 2, Values[0]);
                    //--------------

                    ////-------------- method 2, time-consuming...old version
                    //for (long i = 0; i < gridInfo.Count; i++)
                    //{
                    //    if (n / 2 <= i && i < (Grid.Count + n / 2))
                    //        gridValues[i] = Values[i - n / 2];
                    //    else if (i < n / 2)
                    //        gridValues[i] = Values[0];
                    //    else
                    //        gridValues[i] = Values[^1];
                    //}
                    ////--------------
                }
                return new Grid1DRealData(values: gridValues, gridInfo: gridInfo);
            }
        }

        #endregion
        #region ----- Convert2Scat -----

        /// <summary>
        /// converts the grid vector to scattered format
        /// with all the coordinates explicitly given
        /// </summary>
        /// <returns> result scatter vector </returns>
        public Scat1DRealData Convert2ScatFormat()
            => new(scatInfo: new(GridInfo.GetCoordinates()), values: Values);

        #endregion
        #region ----- Padding -----

        /// <summary>
        /// padding according to target parameters
        /// </summary>
        /// <param name="targetCount"> target number of elements in the padded vector </param>
        /// <param name="startIndex"> starting index in the padded vector </param>
        /// <param name="paddingValue"> value used for the padding </param>
        /// <returns> result grid data after padding </returns>
        public Grid1DRealData? Padding(long targetCount, long startIndex,
            double paddingValue = 0.0)
        {
            // modifies values
            VectorD nv = Values.Padding(targetCount, startIndex, paddingValue);
            if (nv == VectorD.Empty) { return null; }

            // modifies grid
            GridInfo1D ng = new(n: targetCount, spacing: GridInfo.Spacing,
                refPoint: GridInfo.Start - startIndex * GridInfo.Spacing,
                refType: GridRefType.Start);
            // output
            return new(values: nv, gridInfo: ng);
        }

        /// <summary>
        /// centered zero-padding on both sides
        /// </summary>
        /// <param name="targetCount"> target [even] number of elements </param>
        /// <returns></returns>
        public Grid1DRealData? Padding(long targetCount)
        {
            // modifies values
            VectorD nv = Values.Padding(targetCount);
            if (nv == VectorD.Empty) { return null; }

            // modifies grid
            GridInfo1D ng = new(n: targetCount, spacing: GridInfo.Spacing,
                refPoint: GridInfo.Start - (targetCount - GridInfo.Count) / 2 * GridInfo.Spacing,
                refType: GridRefType.Start);

            // output
            return new(values: nv, gridInfo: ng);
        }

        #endregion
        #region ----- FindValues -----

        /// <summary>
        /// evaluates value at a single location  
        /// </summary>
        /// <param name="x"> location for evaluation </param>
        /// <returns> interpolated value on the evaluation location </returns>
        public double FindValue(double x)
            => Interpolation.FindValue(x);

        /// <summary>
        /// evaluates values on a set of scattered locations
        /// </summary>
        /// <param name="xs"> set of scattered locations for evaluation </param>
        /// <param name="loopMode"> computational option for loops </param>
        /// <returns> interpolated values on those evaluation locations </returns>
        public VectorD FindValues(ScatInfo1D xs,
            LoopMode loopMode = Defaults.LoopOption)
            => Interpolation.Evaluate(xs: xs, loopMode: loopMode);

        /// <summary>
        /// evaluates values on a target grid
        /// </summary>
        /// <param name="targetGrid"> target grid for evaluation </param>
        /// <param name="loopMode"> computational option for loops </param>
        /// <returns> interpolated values on the target grid </returns>
        public VectorD FindValues(GridInfo1D targetGrid,
            LoopMode loopMode = Defaults.LoopOption)
            => Interpolation.Evaluate(targetGrid: targetGrid, loopMode: loopMode);

        #endregion

        #endregion
        #region operators


        //public static Grid1DRealData operator +(Grid1DRealData x, Grid1DRealData y)
        //{
        //    if (!x.Grid.IsSameAs(y.Grid))
        //        return null;

        //    Grid1DRealData res = new(x.Grid, x.Values + y.Values);
        //    return res;
        //}

        #endregion
    }

}
