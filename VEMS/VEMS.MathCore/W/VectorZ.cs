using System.Numerics;
using WMathCore;

namespace VEMS.MathCore.W
{
    /// <summary>
    /// complex-valued vector class
    /// wrapper of [WVectorZ] and extensions
    /// </summary>
    public class VectorZ : WVectorZ, IVector<Complex>
    {
        #region fields

        /// <summary>
        /// empty vector with ZERO count
        /// </summary>
        public static VectorZ Empty = new(0);

        #endregion
        #region constructors

        #region === wrappers ===

        /// <summary>
        /// constructs a vector with given length
        /// </summary>
        /// <param name="count"> count of the elements </param>
        public VectorZ(long count)
            : base(count)
        { }

        /// <summary>
        /// constructs a vector with given length
        /// and sets all elements to the same value 
        /// </summary>
        /// <param name="count"> count of the elements </param>
        /// <param name="initVal"> initial value for all elements </param>
        public VectorZ(long count, Complex initVal)
            : base(count, new WComplex(initVal.Real, initVal.Imaginary), mode)
        { }

        /// <summary>
        /// constructs a vector by copying from another
        /// [deep copy]
        /// </summary>
        /// <param name="source"> another vector </param>
        public VectorZ(VectorZ source)
            : base(source, mode) 
        { }

        /// <summary>
        /// constructs a complex vector with its
		/// real- or imaginary part only
        /// </summary>
        /// <param name="part"> part of the complex vector </param>
        /// <param name="isRealPart"> is real- or imaginary-part </param>
        public VectorZ(VectorD part, bool isRealPart = true)
            : base(part, isRealPart, mode)
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
        public unsafe VectorZ(long count, Complex start, Complex increment)
            : base(count)
        {
            Complex* zValues = (Complex*)Values;
            for (long i = 0; i < Count; i++)
                *(zValues + i) = start + i * increment;
        }

        /// <summary>
        /// constructs a vector with given
        /// initial value, end value, and length
        /// </summary>
        /// <param name="start"> value of the first element </param>
        /// <param name="end"> value of the ending element </param>
        /// <param name="count"> count of the elements </param>
        public VectorZ(Complex start, Complex end, long count)
            : this(count, start, (end - start) / (count - 1))
        { }

        #endregion

        #endregion
        #region properties

        #region === helpers ===

        /// <summary>
        /// mode option
        /// </summary>
        internal const int mode = 1;

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
        public unsafe Complex* SPtr => (Complex*)Values;

        /// <summary>
        /// gets / sets the value of a vector element 
        /// </summary>
        /// <param name="i"> index of the element [Int64] </param>
        /// <param name="checkBound"> whether to check if the index is outside bound </param>
        /// <returns> element value </returns>
        public Complex this[long i, bool checkBound = true]
        {
            get
            {
                bool invalidIndex = checkBound && !IsIndexValid(i);
                if (invalidIndex) { throw new IndexOutOfRangeException(); }
                return new(base[i].Real, base[i].Imag); 
            }
            set
            {
                bool invalidIndex = checkBound && !IsIndexValid(i);
                if (invalidIndex) { throw new IndexOutOfRangeException(); }
                base[i] = new(value.Real, value.Imaginary); 
            }
        }

        /// <summary>
        /// gets / sets the vector elements within a given range
        /// </summary>
        /// <param name="rng"> range with inclusive start and exclusive end indices [Int64] </param>
        /// <returns> vector elements within the range </returns>
        public VectorZ this[LongRange rng]
        {
            get
            {
                if (!IsRangeValid(rng)) { throw new IndexOutOfRangeException(); }
                return (VectorZ)GetRange(rng.Start, rng.End, mode);
            }
            set
            {
                if (value.GetType() != typeof(VectorZ)) { throw new ArgumentException("Invalid input", nameof(value)); }
                if (!IsRangeValid(rng)) { throw new IndexOutOfRangeException(); }
                SetRange(rng.Start, rng.End, value, mode);
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
        public Complex this[int i, bool checkBound = true]
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
        public Complex this[Index i, bool checkBound = true]
        {
            get => this[i.Value, checkBound];
            set => this[i.Value, checkBound] = value;
        }

        /// <summary>
        /// gets / sets the vector elements within a given range
        /// </summary>
        /// <param name="rng"> range with inclusive start and exclusive end indices [Int32] </param>
        /// <returns> vector elements within the range </returns>
        public VectorZ this[Range rng]
        {
            get => this[new LongRange(rng, Count)];
            set => this[new LongRange(rng, Count)] = value;
        }

        ///// <summary>
        ///// gets the real part of the complex vector
        ///// </summary>
        //public XVectorD RealPart => VMath.RealPart(this);

        ///// <summary>
        ///// gets the imaginary part of the complex vector
        ///// </summary>
        //public XVectorD ImagPart => VMath.ImagPart(this);

        ///// <summary>
        ///// gets the magnitude of the complex vector
        ///// </summary>
        //public XVectorD Magnitude => VMath.Abs(this);

        ///// <summary>
        ///// gets the argument of the complex vector
        ///// </summary>
        //public XVectorD Argument => VMath.Arg(this);

        #endregion

        #endregion
        #region methods

        /// <summary>
        /// padding according to given target parameters
        /// </summary>
        /// <param name="targetCount"> target number of elements in the padded vector </param>
        /// <param name="startIndex"> starting index in the padded vector </param>
        /// <param name="paddingValueRe"> real-part of the padding value </param>
        /// <param name="paddingValueIm"> imag-part of the padding value </param>
        /// <returns> result vector after padding </returns>
        public VectorZ Padding(long targetCount, long startIndex,
            double paddingValueRe = 0.0, double paddingValueIm = 0.0)
        {
            if (targetCount <= Count)
            {
                Printer.Warning($"{nameof(targetCount)} must be greater than the current value");
                return Empty;
            }

            Complex paddingValue = new(paddingValueRe, paddingValueIm);
            VectorZ y = new(targetCount, paddingValue);
            LongRange rng = new(startIndex, startIndex + Count);
            y[rng] = this;

            return y;
        }

        /// <summary>
        /// centered zero-padding on both sides
        /// </summary>
        /// <param name="targetCount"> target [even] number of elements </param>
        /// <returns></returns>
        public VectorZ Padding(long targetCount)
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
