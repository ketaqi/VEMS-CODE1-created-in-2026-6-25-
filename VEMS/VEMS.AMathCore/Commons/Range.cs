namespace VEMS.AMathCore
{

    /// <summary>
    /// long integer [Int64] index
    /// </summary>
    public class LongIndex
    {
        #region properties

        /// <summary>
        /// value of the index
        /// </summary>
        public long Value { get; set; }
        
        /// <summary>
        /// whether to count from the end
        /// </summary>
        public bool IsFromEnd { get; set; }

        #endregion
        #region constructor

        /// <summary>
        /// constructs a LongIndex
        /// </summary>
        /// <param name="i"> index value </param>
        /// <param name="isFromEnd"> whether to count from the end </param>
        public LongIndex(long i, bool isFromEnd = false)
        {
            Value = i;
            IsFromEnd = isFromEnd;
        }

        #endregion
        #region methods

        /// <summary>
        /// converts a long integer number to a long index
        /// </summary>
        /// <param name="value"> the long integer to convert </param>
        public static implicit operator LongIndex(long value)
            => new (value);


        internal bool CheckIndexOutsideBound(long min, long max)
        {
            bool indexOutsideBound = false;
            if(Value < min || Value > max)
            {
                indexOutsideBound = true;
                Console.WriteLine("index outside bound(s)");
            }
            return indexOutsideBound;
        }

        #endregion
    }

    /// <summary>
    /// long integer [Int64] range
    /// </summary>
    public class LongRange
    {
        /// <summary>
        /// start index (inclusive)
        /// </summary>
        public long Start { get; set; }

        /// <summary>
        /// end index (exclusive)
        /// </summary>
        public long End { get; set; }

        /// <summary>
        /// constructs a LongRange
        /// </summary>
        /// <param name="start"> inclusive start index </param>
        /// <param name="end"> exclusive end index </param>
        public LongRange(long start, long end)
        {
            Start = start;
            End = end;
        }

        /// <summary>
        /// converts a Range to LongRange with given total count
        /// </summary>
        /// <param name="rng"> input range </param>
        /// <param name="count"> total count of elements </param>
        public LongRange(Range rng, long count)
        {
            if (rng.Start.IsFromEnd)
                Start = count - rng.Start.Value;
            else
                Start = rng.Start.Value;

            if (rng.End.IsFromEnd)
                End = count - rng.End.Value;
            else
                End = rng.End.Value;
        }

        /// <summary>
        /// converts a LongRange to Range
        /// </summary>
        /// <returns> converted Range </returns>
        public Range ToRange()
            => new ((int)Start, (int)End);
        

    }

}