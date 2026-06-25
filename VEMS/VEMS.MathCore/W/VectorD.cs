using WMathCore;

namespace VEMS.MathCore.W
{
    /// <summary>
    /// real-valued vector class
    /// wrapper of [WVectorD] and extensions 
    /// </summary>
    public class VectorD : WVectorD, IVector<double>
    {
        #region fields

        /// <summary>
        /// empty vector with ZERO count
        /// </summary>
        public static VectorD Empty = new(0);

        #endregion
        #region properties

        /// <summary>
        /// mode option
        /// </summary>
        internal const int mode = 1;

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

        #region === wrappers ===

        /// <summary>
        /// gets the type-specific pointer to the values
        /// </summary>
        public unsafe double* SPtr => Values;

        /// <summary>
        /// gets / sets the value of a vector element 
        /// </summary>
        /// <param name="i"> index of the element [Int64] </param>
        /// <param name="checkBound"> whether to check if the index is outside bound </param>
        /// <returns> element value </returns>
        public double this[long i, bool checkBound = true]
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
        /// gets / sets the value of a vector element 
        /// </summary>
        /// <param name="i"> index of the element [Int32] </param>
        /// <param name="checkBound"> whether to check if the index is outside bound </param>
        /// <returns> element value </returns>
        public double this[int i, bool checkBound = true]
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
        public double this[Index i, bool checkBound = true]
        {
            get => this[i.Value, checkBound];
            set => this[i.Value, checkBound] = value;
        }

        /// <summary>
        /// gets / sets the vector elements within a given range
        /// </summary>
        /// <param name="rng"> range with inclusive start and exclusive end indices [Int64] </param>
        /// <returns> vector elements within the range </returns>
        public VectorD this[LongRange rng]
        {
            get
            {
                if (!IsRangeValid(rng)) { throw new IndexOutOfRangeException(); }
                return (VectorD)GetRange(rng.Start, rng.End, mode);
            }
            set
            {
                if (value.GetType() != typeof(VectorD)) { throw new ArgumentException("Invalid input", nameof(value)); }
                if (!IsRangeValid(rng)) { throw new IndexOutOfRangeException(); }
                SetRange(rng.Start, rng.End, value, mode);
            }
        }

        #endregion
        #region === extended ===


        /// <summary>
        /// gets / sets the vector elements within a given range
        /// </summary>
        /// <param name="rng"> range with inclusive start and exclusive end indices [Int32] </param>
        /// <returns> vector elements within the range </returns>
        public VectorD this[Range rng]
        {
            get => this[new LongRange(rng, Count)];
            set => this[new LongRange(rng, Count)] = value;
        }

        #endregion

        #endregion
        #region constructors

        #region === wrappers ===

        /// <summary>
        /// constructs a vector with given length
        /// </summary>
        /// <param name="count"> count of the elements </param>
        public VectorD(long count)
            : base(count)
        { }

        /// <summary>
        /// constructs a vector with given length
        /// and sets all elements to the same initial value
        /// </summary>
        /// <param name="count"> count of the element </param>
        /// <param name="initVal"> initial value for all the elements </param>
        public VectorD(long count, double initVal)
            : base(count, initVal, mode)
        { }

        /// <summary>
        /// constructs a vector by copying from another
        /// [deep copy]
        /// </summary>
        /// <param name="source"> another vector </param>
        public VectorD(VectorD source)
            : base(source, mode)
        { }

        #endregion
        #region === extended ===

        /// <summary>
        /// constructs a vector with given length
        /// first element to the initial value
        /// and the rest follow the increment
        /// </summary>
        /// <param name="count"> count of the elements </param>
        /// <param name="start"> value of the first element </param>
        /// <param name="increment"> increment for the rest elements </param>
        public unsafe VectorD(long count, double start, double increment)
            : this(count)
        {
            for (long i = 0; i < Count; i++)
                *(Values + i) = start + i * increment;
        }

        /// <summary>
        /// constructs a vector with given
        /// initial value, end value, and length
        /// </summary>
        /// <param name="start"> value of the first element </param>
        /// <param name="end"> value of the ending element </param>
        /// <param name="count"> count of the elements </param>
        public VectorD(double start, double end, long count)
            : this(count, start, (end - start) / (count - 1))
        { }

        /// <summary>
        /// constructs a vector with
        /// initial value, increment, and end value
        /// </summary>
        /// <param name="start"> value of the first element </param>
        /// <param name="increment"> increment between two elements </param>
        /// <param name="end"> (expected) ending value </param>
        public VectorD(double start, double increment, double end)
            : this((long)((end - start) / increment) + 1, start, increment)
        { }

        #endregion

        #endregion
        #region methods

        #region === interface ===

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

        #endregion

        /// <summary>
        /// padding according to target vector parameters
        /// </summary>
        /// <param name="targetCount"> target number of elements in the padded vector </param>
        /// <param name="startIndex"> starting index in the padded vector </param>
        /// <param name="paddingValue"> value used for the padding </param>
        /// <returns> result vector after padding </returns>
        public VectorD Padding(long targetCount, long startIndex,
            double paddingValue = 0.0)
        {
            if (targetCount <= Count)
            {
                Printer.Warning($"{nameof(targetCount)} must be greater than the current value");
                return Empty;
            }

            VectorD y = new(targetCount, paddingValue);
            LongRange rng = new(startIndex, startIndex + Count);
            y[rng] = this;

            return y;
        }

        /// <summary>
        /// centered zero-padding on both sides
        /// </summary>
        /// <param name="targetCount"> target [even] number of elements </param>
        /// <returns> result vector after padding </returns>
        public VectorD Padding(long targetCount)
        {
            if ((targetCount - Count) % 2 != 0)
            {
                Printer.Warning($"{nameof(targetCount)} must be even number");
                return Empty;
            }
            return Padding(targetCount, (targetCount - Count) / 2, 0.0);
        }

        #endregion
        // operators ...

    }

}
