namespace VEMS.MathCore
{

    /// <summary>
    /// piecewise VectorD class
    /// </summary>
    public class Pwct1DRealData
    {
        #region properties

        /// <summary>
        /// values of the constants within the spans/pieces
        /// </summary>
        public VectorD Values { get; set; }

        /// <summary>
        /// locations that defines the spans/pieces
        /// number equals value count plus one
        /// </summary>
        public VectorD Spans { get; set; }

        /// <summary>
        /// number of spans/pieces
        /// </summary>
        public long Pieces => Spans.Count - 1;

        /// <summary>
        /// gets the full range
        /// </summary>
        public double Range => Spans[Pieces] - Spans[0];

        #endregion
        #region constructor

        /// <summary>
        /// constructs a piecewise-constant data with
        /// specific spans and values
        /// </summary>
        /// <param name="spans"> locations that defines the spans/pieces </param>
        /// <param name="values"> values of the constants within those pieces </param>
        /// <exception cref="ArgumentException"></exception>
        public Pwct1DRealData(VectorD spans, VectorD values)
        {
            // exception handling
            if (spans.Count - values.Count != 1) 
            { throw new ArgumentException("Vector counts not match", nameof(values)); }
            // sets values
            Spans = spans;
            Values = values;
        }

        /// <summary>
        /// constructs a piecewise VectorD with given
        /// grid and values
        /// </summary>
        /// <param name="grid"> grid point locations </param>
        /// <param name="values"> piecewise values </param>
        /// <param name="deepCopy"> copy mode option; default is deep copy </param>
        [Obsolete]
        public Pwct1DRealData(VectorD grid, VectorD values, bool deepCopy = true)
        {
            if(grid.Count - values.Count != 1) { throw new ArgumentException("Vector conts not match", nameof(values)); }
            // deep copy for both the grid and the values
            Spans = new(grid, deepCopy);
            Values = new(values, deepCopy);
        }

        #endregion
        #region method

        /// <summary>
        /// re-samples the data on a set of given locations
        /// either uniform or scattered
        /// </summary>
        /// <param name="xs"> sample locations, either uniform or scattered </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> sampled values on these locations </returns>
        public VectorD Sample(ScatInfo1D xs,
            LoopMode loopMode = Defaults.LoopOption)
        {
            VectorD vs = new(count: xs.Count);
            switch (loopMode)
            {
                case LoopMode.Sequential:
                    {
                        for(long i = 0; i < xs.Count; i++)
                        {
                            // searches for the span/piece that contains x
                            long p = 0;
                            Search.Binary(xs: Spans, x: xs[i], k: out p);
                            // takes value from this span/piece
                            vs[i, false] = Values[p, false];
                        }
                        break;
                    }
                case LoopMode.Parallel:
                    {
                        Parallel.For(0, xs.Count, i =>
                        {
                            // searches for the span/piece that contains x
                            long p = 0;
                            Search.Binary(xs: Spans, x: xs[i], k: out p);
                            // takes value from this span/piece
                            vs[i, false] = Values[p, false];
                        });
                        break;
                    }
                case LoopMode.Vectorized:
                    throw new NotImplementedException();
                default: goto case LoopMode.Sequential;
            }
            return vs;
        }

        /// <summary>
        /// re-samples the data on a target uniform grid
        /// </summary>
        /// <param name="grid"> target uniform grid </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> sampled values on the target grid </returns>
        public VectorD SampleOnGrid(GridInfo1D grid,
            LoopMode loopMode = Defaults.LoopOption)
            => Sample(xs: (ScatInfo1D)grid, loopMode: loopMode);

        /// <summary>
        /// re-samples the data on uniform grid covering 
        /// full range with specific number of samples
        /// </summary>
        /// <param name="n"> number of samples </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> sampled values on the uniform grid </returns>
        public (VectorD, GridInfo1D) SampleOnGrid(long n,
            LoopMode loopMode = Defaults.LoopOption)
        {
            // defines uniform grid 
            GridInfo1D g = new(n: n,
                start: Spans[0] + 0.5 * Range / n,
                spacing: Range / n);
            // samples on the uniform grid
            VectorD v = SampleOnGrid(grid: g, loopMode: loopMode);
            // return
            return (v, g);
        }

        /// <summary>
        /// Convert from piesewise to grid samplings
        /// </summary>
        /// <param name="n"> grid sampling count number </param>
        /// <returns> grid sampling vector </returns>
        [Obsolete]
        public Grid1DRealData Piece2Grid1DRealData(long n)
        {
            // changes to grid sampling
            GridInfo1D gridInfo = new(n: n, 
                start: Spans[0] + 0.5 * Range/n, 
                spacing: Range/n);
            VectorD gridValues = new(count: n);
            // fills in values
            for (long i = 0; i < n; i++)
            {
                double x = gridInfo.GetCoordinate(i);
                for (int p = 1; p < Spans.Count; p++)
                {
                    if (Spans[p - 1] <= x && x < Spans[p])
                        gridValues[i, false] = Values[p - 1, false];
                }
            }
            // return
            return new(values: gridValues, gridInfo: gridInfo);
        }

        #endregion
    }

}
