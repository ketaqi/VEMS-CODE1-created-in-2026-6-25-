using WMathCore;

namespace VEMS.MathCore.W
{
    /// <summary>
    /// Int64 vector class
    /// wrapper of [WVectorI]
    /// </summary>
    public class VectorI : WVectorI, IVector<long>
    {
        #region fields

        /// <summary>
        /// empty vector with ZERO count
        /// </summary>
        public static VectorD Empty = new(0);

        #endregion
        #region constructors

        /// <summary>
        /// constructs a vector with given length
        /// </summary>
        /// <param name="count"> count of the elements </param>
        public VectorI(long count)
            : base(count)
        { }

        /// <summary>
        /// constructs a vector with given length
        /// and sets all elements to the same initial value
        /// </summary>
        /// <param name="count"> count of the element </param>
        /// <param name="initVal"> initial value for all the elements </param>
        public VectorI(long count, long initVal)
            : base(count, initVal) 
        { }

        /// <summary>
        /// constructs a vector by copying from another
        /// [deep copy]
        /// </summary>
        /// <param name="source"> another vector </param>
        public VectorI(VectorI source)
            : base(source) 
        { }

        #endregion
        #region properties

        #region === helpers ===

        /// <summary>
        /// checks if a given index is valid
        /// [lower bound = zero] 
        /// [upper bound = Count]
        /// </summary>
        /// <param name="i"> input index </param>
        /// <returns> return true if the index is valid
        /// otherwise return false </returns>
        public bool IsIndexValid(long i)
        {
            if (i >= 0 && i < Count) { return true; }
            else { return false; }
        }

        /// <summary>
        /// checks if a given range is valid
        /// [lower bound = zero] 
        /// [upper bound = Count]
        /// </summary>
        /// <param name="rng"> input range </param>
        /// <returns> return true if the range is valid
        /// otherwise return false </returns>
        internal bool IsRangeValid(LongRange rng)
        {
            if (rng.Start >= 0 && rng.End < Count) { return true; }
            else { return false; }
        }

        #endregion
        #region === wrappers ===

        /// <summary>
        /// gets the type-specific pointer to the values
        /// </summary>
        public unsafe long* SPtr => Values;

        /// <summary>
        /// gets / sets the value of a vector element 
        /// </summary>
        /// <param name="i"> index of the element [Int64] </param>
        /// <param name="checkBound"> whether to check if the index is outside bound </param>
        /// <returns> element value </returns>
        public long this[long i, bool checkBound = true] 
        { 
            get
            {
                bool invalidIndex = checkBound && !IsIndexValid(i);
                if (invalidIndex) { throw new IndexOutOfRangeException(); }
                return base[i]; 
            }
            set
            {
                bool invalidIndex = checkBound && !IsIndexValid(i);
                if (invalidIndex) { throw new IndexOutOfRangeException(); }
                base[i] = value; 
            }
        }

        /// <summary>
        /// gets / sets the vector elements within a given range
        /// </summary>
        /// <param name="rng"> range with inclusive start and exclusive end indices [Int64] </param>
        /// <returns> vector elements within the range </returns>
        public VectorI this[LongRange rng]
        {
            get
            {
                if (!IsRangeValid(rng)) { throw new ArgumentException("Invalid range", nameof(rng)); }
                return (VectorI)GetRange(rng.Start, rng.End);
            }
            set
            {
                if (value.GetType() != typeof(VectorI)) { throw new ArgumentException("Invalid input", nameof(value)); }
                if (!IsRangeValid(rng)) { throw new ArgumentException("Invalid range", nameof(rng)); }
                SetRange(rng.Start, rng.End, value);
            }
        }

        #endregion
        #region === extended ===

        /// <summary>
        /// gets / sets the value of a vector element 
        /// </summary>
        /// <param name="i"> index of the element [Int32] </param>
        /// <param name="checkBound"> whether to check if the index is outside bound </param>
        /// <returns> element value </returns>
        public long this[int i, bool checkBound = true] 
        {
            get => this[(long)i, checkBound]; 
            set => this[(long)i, checkBound] = value; 
        }

        /// <summary>
        /// gets / sets the value of a vector element 
        /// </summary>
        /// <param name="i"> index of the element [Int32] </param>
        /// <param name="checkBound"> whether to check if the index is outside bound </param>
        /// <returns> element value </returns>
        public long this[Index i, bool checkBound = true] 
        {
            get => this[i.Value, checkBound];
            set => this[i.Value, checkBound] = value;
        }

        /// <summary>
        /// gets / sets the vector elements within a given range
        /// </summary>
        /// <param name="rng"> range with inclusive start and exclusive end indices [Int32] </param>
        /// <returns> vector elements within the range </returns>
        public VectorI this[Range rng]
        {
            get => this[new LongRange(rng, Count)];
            set => this[new LongRange(rng, Count)] = value;
        }

        #endregion

        #endregion
        // methods ...
        // operators ...

    }
}
