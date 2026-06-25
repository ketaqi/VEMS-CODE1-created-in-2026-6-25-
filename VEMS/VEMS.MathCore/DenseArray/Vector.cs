using System.Numerics;

namespace VEMS.MathCore
{
    /// <summary>
    /// Vector[T] class
    /// </summary>
    /// <typeparam name="T"> double or complex </typeparam>
    public class Vector<T> : DenseArrayBase<T>, IVector<T> where T : struct
    {
        #region properties

        /// <summary>
        /// get / set the value of a vector element 
        /// </summary>
        /// <param name="i"> index of the element [Int64] </param>
        /// <param name="checkBound"> whether to check if the index is outside bound </param>
        /// <returns> element value </returns>
        public unsafe T this[long i, bool checkBound = true]
        {
            get
            {
                bool invalidIndex = checkBound && !IsIndexValid(i);
                if (invalidIndex) { throw new IndexOutOfRangeException(nameof(i)); }

#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
                T* iPtr = (T*)VPtr;
#pragma warning restore CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
                T iVal = *(iPtr + i);
                return iVal;

                // --- alternative ---
                //IntPtr pi = IntPtr.Add(DataPtr, (int)(i * Marshal.SizeOf<T>()));
                //T vi = Marshal.PtrToStructure<T>(pi);
                //return vi;
                // --- end of alt. ---
            }
            set
            {
                bool invalidIndex = checkBound && !IsIndexValid(i);
                if (invalidIndex) { return; }

#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
                T* iPtr = (T*)VPtr;
#pragma warning restore CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
                *(iPtr + i) = value;

                // --- alternative ---
                //IntPtr pi = IntPtr.Add(DataPtr, (int)(i * Marshal.SizeOf<T>()));
                //Marshal.StructureToPtr(value, pi, false);
                // --- end of alt. ---
            }
        }

        /// <summary>
        /// get / set the value of a vector element 
        /// </summary>
        /// <param name="i"> index of the element [Int32] </param>
        /// <param name="checkBound"> whether to check if the index is outside bound </param>
        /// <returns> element value </returns>
        public T this[int i, bool checkBound = true]
        {
            get => this[(long)i, checkBound];
            set => this[(long)i, checkBound] = value;
        }

        /// <summary>
        /// get / set the value of a vector element 
        /// </summary>
        /// <param name="i"> index of the element [Int32] </param>
        /// <param name="checkBound"> whether to check if the index is outside bound </param>
        /// <returns> element value </returns>
        public T this[Index i, bool checkBound = true]
        {
            get => this[IndexToInt(i, Count), checkBound];
            set => this[IndexToInt(i, Count), checkBound] = value;
        }

        /// <summary>
        /// get / set the vector elements within a given range
        /// </summary>
        /// <param name="rng"> range with inclusive start and exclusive end indices [Int64] </param>
        /// <param name="loopMode"> loop computational mode option </param>
        /// <returns> vector elements within the range </returns>
        public Vector<T> this[LongRange rng, 
            LoopMode loopMode = Defaults.LoopOption]
        {
            get
            {
                if (!IsRangeValid(rng)) { throw new ArgumentException("Invalid range", nameof(rng)); }
                
                long n = rng.End - rng.Start;
                Vector<T> x = new(count: n, mode: ArrayInitMode.Malloc);

                Action<long> a = (i) => { x[i - rng.Start, false] = this[i, false]; };
                Loop1D loop = new(operation: a, start: rng.Start, end: rng.End);
                loop.Evaluate(loopMode);

                return x;
            }
            set
            {
                if (!IsRangeValid(rng)) { throw new ArgumentException("Invalid range", nameof(rng)); }

                Action<long> a = (i) => { this[i, false] = value[i - rng.Start, false]; };
                Loop1D loop = new(operation: a, start: rng.Start, end: rng.End);
                loop.Evaluate(loopMode);
            }
        }

        /// <summary>
        /// get / set the vector elements within a given range
        /// </summary>
        /// <param name="rng"> range with inclusive start and exclusive end indices [Int32] </param>
        /// <param name="loopMode"> loop computational mode option </param>
        /// <returns> vector elements within the range </returns>
        public Vector<T> this[Range rng, 
            LoopMode loopMode = Defaults.LoopOption]
        {
            get => this[new LongRange(rng, Count), loopMode];
            set => this[new LongRange(rng, Count), loopMode] = value;
        }

        #endregion
        #region constructors

        /// <summary>
        /// default constructor
        /// </summary>
        internal Vector() { }

        /// <summary>
        /// constructs a vector with given length
        /// by default, initializes element values to zeros
        /// </summary>
        /// <param name="count"> count of the elements </param>
        /// <param name="mode"> construct mode option; default is CALLOC </param>
        public Vector(long count,
            ArrayInitMode mode = ArrayInitMode.Calloc)
            : base(count, mode) { }

        /// <summary>
        /// constructs a vector with given length
        /// and sets all elements to the same initial value
        /// </summary>
        /// <param name="count"> count of the element </param>
        /// <param name="initVal"> initial value for all the elements </param>
        /// <param name="loopMode"> loop-computation mode option </param>
        public Vector(long count, T initVal,
            LoopMode loopMode = Defaults.LoopOption)
            : base(count, initVal, loopMode) { }

        /// <summary>
        /// constructs by copying from another
        /// </summary>
        /// <param name="other"> another vector as source </param>
        /// <param name="copyMode"> copy mode option </param>
        public Vector(Vector<T> other, 
            ArrayCopyMode copyMode = ArrayCopyMode.Deep)
            : base(other, copyMode) { }

        #endregion
        #region methods

        // ...
        #region ---- reverse ----

        /// <summary>
        /// Reverses the elements of the vector in place.
        /// </summary>
        /// <remarks>
        /// This method swaps elements from the start and end of the vector, moving towards the center,
        /// until all elements have been reversed. If the vector contains zero or one element, the method returns immediately.
        /// </remarks>
        public unsafe void Reverse()
        {
            if (Count <= 1) { return; }

            long start = 0;
            long end = Count - 1;

            T* ptr = (T*)VPtr;
            while (start < end)
            {
                // exchanges elements using tuple
                (ptr[start], ptr[end]) = (ptr[end], ptr[start]);
                // updates indices
                start++;
                end--;
            }
        }

        #endregion


        #endregion
        #region operator

        //public static explicit operator VectorZ(VectorD v)
        //    => new(part: v, option: ComplexPart.RealPart);

        #endregion
    }

    ///// <summary>
    ///// Int64 vector class
    ///// </summary>
    //public class VectorI : Vector<long>
    //{
    //    #region interface properties 

    //    ///// <summary>
    //    ///// get / set the value of a vector element 
    //    ///// </summary>
    //    ///// <param name="i"> index of the element [Int64] </param>
    //    ///// <param name="checkBound"> whether to check if the index is outside bound </param>
    //    ///// <returns> element value </returns>
    //    //public unsafe long this[long i, bool checkBound = true]
    //    //{
    //    //    get
    //    //    {
    //    //        bool invalidIndex = checkBound && !IsIndexValid(i);
    //    //        if (invalidIndex)
    //    //            return 0; // double.NaN;

    //    //        long* iPtr = (long*)DataPtr.ToPointer();
    //    //        long iVal = *(iPtr + i);
    //    //        return iVal;
    //    //    }
    //    //    set
    //    //    {
    //    //        bool invalidIndex = checkBound && !IsIndexValid(i);
    //    //        if (invalidIndex)
    //    //            return;

    //    //        long* iPtr = (long*)DataPtr.ToPointer();
    //    //        *(iPtr + i) = value;
    //    //    }
    //    //}

    //    ///// <summary>
    //    ///// get / set the value of a vector element 
    //    ///// </summary>
    //    ///// <param name="i"> index of the element [Int32] </param>
    //    ///// <param name="checkBound"> whether to check if the index is outside bound </param>
    //    ///// <returns> element value </returns>
    //    //public long this[int i, bool checkBound = true]
    //    //{
    //    //    get => this[(long)i, checkBound];
    //    //    set => this[(long)i, checkBound] = value;
    //    //}

    //    ///// <summary>
    //    ///// get / set the value of a vector element 
    //    ///// </summary>
    //    ///// <param name="i"> index of the element [Int32] </param>
    //    ///// <param name="checkBound"> whether to check if the index is outside bound </param>
    //    ///// <returns> element value </returns>
    //    //public long this[Index i, bool checkBound = true]
    //    //{
    //    //    get => this[IndexToInt(i, Count), checkBound];
    //    //    set => this[IndexToInt(i, Count), checkBound] = value;
    //    //}

    //    #endregion
    //    #region extension properties

    //    /// <summary>
    //    /// get the type-specific pointer to the values
    //    /// </summary>
    //    public unsafe long* SPtr
    //    {
    //        get => (long*)DataPtr.ToPointer();
    //    }

    //    #endregion
    //    #region constructors

    //    /// <summary>
    //    /// default constructor
    //    /// </summary>
    //    internal VectorI() { }

    //    /// <summary>
    //    /// constructs a vector with given length
    //    /// by default, does not initialize element values
    //    /// </summary>
    //    /// <param name="count"> count of the elements </param>
    //    /// <param name="mode"> construct mode option; default is CALLOC </param>
    //    public VectorI(long count,
    //        ArrayInitMode mode = ArrayInitMode.Calloc)
    //        : base(count, mode) { }

    //    /// <summary>
    //    /// constructs a vector with given length
    //    /// and sets all elements to the same initial value
    //    /// </summary>
    //    /// <param name="count"> count of the element </param>
    //    /// <param name="initVal"> initial value for all the elements </param>
    //    /// <param name="loopMode"> loop-computation mode option </param>
    //    public VectorI(long count, long initVal,
    //        LoopMode loopMode = Defaults.LoopOption)
    //        : base(count, initVal, loopMode) { }

    //    ///// <summary>
    //    ///// constructs a vector with given length
    //    ///// first element to the initial value
    //    ///// and the rest follow the increment
    //    ///// </summary>
    //    ///// <param name="count"> count of the elements </param>
    //    ///// <param name="initVal"> value of the first element </param>
    //    ///// <param name="increment"> increment for the rest elements </param>
    //    ///// <param name="loopMode"> loop-computation mode option </param>
    //    //public VectorI(long count, long initVal, long increment,
    //    //    LoopMode loopMode = Defaults.LoopOption)
    //    //    : this(count)
    //    //{
    //    //    Action<long> a = (i) => { this[i, false] = initVal + i * increment; };
    //    //    Loop1D loop = new(operation: a, start: 0, end: count);
    //    //    loop.Evaluate(loopMode);
    //    //}

    //    /// <summary>
    //    /// constructs a vector by copying from another
    //    /// </summary>
    //    /// <param name="other"> another vector </param>
    //    /// <param name="deepCopy"> copy mode option; default is deep copy </param>
    //    public VectorI(VectorI other, bool deepCopy = true)
    //        : base(other, deepCopy ?
    //              ArrayCopyMode.Deep : ArrayCopyMode.Shallow)
    //    { }

    //    #endregion
    //    #region extension methods

    //    ///// <summary>
    //    ///// !!! for ascending-order only !!!
    //    ///// checks if this vector contains given value
    //    ///// </summary>
    //    ///// <param name="x"> value to check </param>
    //    ///// <param name="idx"> index of the value, if is contained </param>
    //    ///// <returns> result: whether contains or not </returns>
    //    //public bool Contains(long x, out long idx)
    //    //{
    //    //    bool findSpan = NaiveMath.BSearchSpan(this, x, out idx);
    //    //    if (!findSpan) { return false; }
    //    //    else
    //    //    {
    //    //        if (x == this[idx, false]) { return true; }
    //    //        else { return false; }
    //    //    }
    //    //}

    //    #endregion
    //}

    /// <summary>
    /// real vector class
    /// </summary>
    public class VectorD : Vector<double>
    {
        #region fields

        /// <summary>
        /// empty vector with ZERO count
        /// </summary>
        public static VectorD Empty = new(0);

        #endregion
        #region properties

        /// <summary>
        /// get the type-specific pointer to the values
        /// </summary>
        public unsafe double* SPtr
        {
            get => (double*)DataPtr.ToPointer();
        }

        /// <summary>
        /// get / set the vector elements within a given range
        /// </summary>
        /// <param name="rng"> range with inclusive start and exclusive end indices [Int64] </param>
        /// <param name="loopMode"> loop computational mode option </param>
        /// <returns> vector elements within the range </returns>
        public new VectorD this[LongRange rng,
            LoopMode loopMode = Defaults.LoopOption]
        {
            get
            {
                if (!IsRangeValid(rng)) { throw new ArgumentException("Invalid range", nameof(rng)); }

                long n = rng.End - rng.Start;
                VectorD x = new(count: n, mode: ArrayInitMode.Malloc);

                Action<long> a = (i) => { x[i - rng.Start, false] = this[i, false]; };
                Loop1D loop = new(operation: a, start: rng.Start, end: rng.End);
                loop.Evaluate(loopMode);

                return x;
            }
            set
            {
                if (!IsRangeValid(rng)) { throw new ArgumentException("Invalid range", nameof(rng)); }

                Action<long> a = (i) => { this[i, false] = value[i - rng.Start, false]; };
                Loop1D loop = new(operation: a, start: rng.Start, end: rng.End);
                loop.Evaluate(loopMode);
            }
        }

        /// <summary>
        /// get / set the vector elements within a given range
        /// </summary>
        /// <param name="rng"> range with inclusive start and exclusive end indices [Int32] </param>
        /// <param name="loopMode"> loop computational mode option </param>
        /// <returns> vector elements within the range </returns>
        public new VectorD this[Range rng, LoopMode loopMode = Defaults.LoopOption]
        {
            get => this[new LongRange(rng, Count), loopMode];
            set => this[new LongRange(rng, Count), loopMode] = value;
        }

        #endregion
        #region constructor

        /// <summary>
        /// constructs a vector with given length
        /// by default, does not initialize element values
        /// </summary>
        /// <param name="count"> count of the elements </param>
        /// <param name="mode"> construct mode option; default is CALLOC </param>
        public VectorD(long count,
            ArrayInitMode mode = ArrayInitMode.Calloc)
            : base(count, mode) { }

        /// <summary>
        /// constructs a vector with given length
        /// and sets all elements to the same initial value
        /// </summary>
        /// <param name="count"> count of the element </param>
        /// <param name="initVal"> initial value for all the elements </param>
        /// <param name="loopMode"> loop-computation mode option </param>
        public VectorD(long count, double initVal,
            LoopMode loopMode = Defaults.LoopOption)
            : base(count, initVal, loopMode) { }

        /// <summary>
        /// constructs a vector with given length
        /// first element to the initial value
        /// and the rest follow the increment
        /// </summary>
        /// <param name="count"> count of the elements </param>
        /// <param name="initVal"> value of the first element </param>
        /// <param name="increment"> increment for the rest elements </param>
        /// <param name="loopMode"> loop-computation mode option </param>
        public VectorD(long count, double initVal, double increment,
            LoopMode loopMode = Defaults.LoopOption)
            : this(count)
        {
            Action<long> a = (i) => { this[i, false] = initVal + i * increment; };
            Loop1D loop = new(operation: a, start: 0, end: count);
            loop.Evaluate(loopMode);
        }

        ///// <summary>
        ///// constructs a vector with given
        ///// initial value, end value, and length
        ///// </summary>
        ///// <param name="initVal"> value of the first element </param>
        ///// <param name="endVal"> value of the ending element </param>
        ///// <param name="count"> count of the elements </param>
        ///// <param name="loopMode"> loop-computation mode option </param>
        //public VectorD(double initVal, double endVal, long count,
        //    LoopMode loopMode = Defaults.LoopOption)
        //    : this(count, initVal, (endVal - initVal) / (count - 1), loopMode) { }

        ///// <summary>
        ///// constructs a vector with
        ///// initial value, increment, and end value
        ///// </summary>
        ///// <param name="initVal"> value of the first element </param>
        ///// <param name="increment"> increment between two elements </param>
        ///// <param name="endVal"> (expected) ending value </param>
        ///// <param name="loopMode"> loop-computation mode option </param>
        //public VectorD(double initVal, double increment, double endVal,
        //    LoopMode loopMode = Defaults.LoopOption)
        //    : this((long)((endVal - initVal) / increment) + 1, initVal, increment, loopMode) { }

        /// <summary>
        /// constructs a vector by copying from another
        /// </summary>
        /// <param name="other"> another vector </param>
        /// <param name="deepCopy"> copy mode option; default is deep copy </param>
        public VectorD(VectorD other, bool deepCopy = true)
            : this(other.Count, deepCopy ? ArrayInitMode.Malloc : ArrayInitMode.None)
        {
            if (deepCopy)
            {
                VectorD t = this;
                //VMath.CopyD(x: other, y: ref t);
                unsafe { Defaults.IBLAS.Copy(n: other.Count, x: (double*)other.SPtr,
                    y: (double*)t.SPtr, incx: 1, incy: 1); }
            }
            else
            {
                //DataPtr = other.DataPtr; 
                other.MemberwiseClone();
            }
        }

        /// <summary>
        /// constructs a vector by copying from a given ArrayBase
        /// </summary>
        /// <param name="other"> another ArrayBase </param>
        /// <param name="deepCopy"> copy mode option; default is deep copy </param>
        public VectorD(DenseArrayBase<double> other, bool deepCopy = true)
            : this(other.Count, deepCopy ? ArrayInitMode.Malloc : ArrayInitMode.None)
        {
            if (deepCopy)
            {
                DenseArrayBase<double> t = this;
                //VMath.CopyD(x: other, y: ref t);
                unsafe{ Defaults.IBLAS.Copy(n: other.Count, x: (double*)other.VPtr,
                    y: (double*)t.VPtr, incx: 1, incy: 1); }
            }
            else
            { DataPtr = other.DataPtr; }
        }

        #endregion
        #region naive methods

        ///// <summary>
        ///// sums up all the elements
        ///// </summary>
        ///// <returns> summation result </returns>
        //public double Sum(LoopMode mode = Defaults.LoopOption)
        //    => VMath.Sum(x: this, loopMode: mode);

        ///// <summary>
        ///// finds the index of the element with the largest value
        ///// </summary>
        ///// <returns> (index, value) </returns>
        //public (long, double) IndexMax(LoopMode mode = Defaults.LoopOption)
        //    => VMath.Max(x: this, loopMode: mode);

        ///// <summary>
        ///// finds the index of the element with the smallest value
        ///// </summary>
        ///// <returns> (index, value) </returns>
        //public (long, double) IndexMin(LoopMode mode = Defaults.LoopOption)
        //    => VMath.Min(x: this, loopMode: mode);

        ///// <summary>
        ///// sorts the elements in the vector, from smallest to largest
        ///// </summary>
        //public void Sort()
        //{
        //    VectorD t = this;
        //    VMath.Sort(x: ref t);
        //}

        ///// <summary>
        ///// converts to array
        ///// </summary>
        ///// <returns> result array </returns>
        //public double[] ToArray(LoopMode mode = Defaults.LoopOption)
        //    => VMath.ConvertVectorToArray(x: this, loopMode: mode);

        #endregion
        #region extra methods

        ///// <summary>
        ///// check if a given index is valid
        ///// [lower bound = zero] 
        ///// [upper bound = Count]
        ///// </summary>
        ///// <param name="i"> input index </param>
        ///// <returns> return true if the index is valid
        ///// otherwise return false </returns>
        //public new bool IsIndexValid(long i)
        //    => IsIndexValid(i, Count, GetType().Name);

        ///// <summary>
        ///// check if a given range is valid
        ///// [lower bound = zero] 
        ///// [upper bound = Count]
        ///// </summary>
        ///// <param name="rng"> input range </param>
        ///// <returns> return true if the range is valid
        ///// otherwise return false </returns>
        //public new bool IsRangeValid(LongRange rng)
        //    => IsRangeValid(rng, Count, GetType().Name);

        ///// <summary>
        ///// checks whether this vector has
        ///// the same count as the other
        ///// </summary>
        ///// <param name="other"> the other vector </param>
        ///// <returns> return true if same; otherwise false </returns>
        //public bool HasSameCount(VectorD other)
        //{
        //    if (other.Count == Count)
        //        return true;
        //    else
        //        Console.Write(GetType().Name + ": " + "Unequal vector length");
        //    return false;
        //}

        ///// <summary>
        ///// checks whether this vector has
        ///// the same count as the other
        ///// </summary>
        ///// <param name="other"> the other vector </param>
        ///// <returns> return true if same; otherwise false </returns>
        //public bool HasSameCount(VectorZ other)
        //{
        //    if (other.Count == Count)
        //        return true;
        //    else
        //        Console.Write(GetType().Name + ": " + "Unequal vector length");
        //    return false;
        //}

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
        /// <param name="targetCount"> target number of elements </param>
        /// <returns> result vector after padding </returns>
        public VectorD Padding(long targetCount)
        {
            if ((targetCount - Count) % 2 != 0)
            {
                Printer.Warning($"{nameof(targetCount)} must be an even addition to the current value");
                return Empty;
            }
            return Padding(targetCount, (targetCount - Count) / 2, 0.0);
        }

        ///// <summary>
        ///// truncates current vector according to target parameters
        ///// </summary>
        ///// <param name="targetCount"> target number of elements in the truncated vector </param>
        ///// <param name="startIndex"> starting index in the original vector </param>
        ///// <returns> result vector after truncation </returns>
        //public VectorD Truncate(long targetCount, long startIndex)
        //{
        //    if (startIndex + targetCount >= Count)
        //    {
        //        Printer.Warning($"invalid combination of parameters {nameof(targetCount)} and {nameof(startIndex)}");
        //        return Empty;
        //    }

        //    LongRange rng = new(startIndex, startIndex + targetCount);
        //    return this[rng];
        //}

        ///// <summary>
        ///// centered truncation on both sides of the vector
        ///// </summary>
        ///// <param name="targetCount"> target number of elements </param>
        ///// <returns> result vector after truncation </returns>
        //public VectorD Truncate(long targetCount)
        //{
        //    if ((Count - targetCount) % 2 != 0)
        //    {
        //        Printer.Warning($"{nameof(targetCount)} must be an even subtraction of the current value");
        //        return Empty;
        //    }
        //    return Truncate(targetCount, (Count - targetCount) / 2);
        //}

        ///// <summary>
        ///// replicates according to target number of elements
        ///// </summary>
        ///// <param name="targetCount"> target number of elements after replication </param>
        ///// <returns> replicated result </returns>
        //public VectorD Replicate(long targetCount)
        //{
        //    if (targetCount <= Count) { Printer.Warning($"Target count not greater than the current value."); }
        //    VectorD rp = new(targetCount);
        //    // computes replication multiple and residual count
        //    long m = (long)(targetCount / Count);
        //    long rest = targetCount - m * Count;
        //    // loop for multiple replications
        //    for (long i = 0; i < m; i++)
        //    {
        //        LongRange unit = new(i * Count + 0, i * Count + Count);
        //        rp[unit] = this;
        //    }
        //    // loop for residual
        //    for (long i = 0; i < rest; i++)
        //        rp[m * Count + i, false] = this[i, false];

        //    return rp;
        //}

        ///// <summary>
        ///// reverses array using while loop
        ///// </summary>
        ///// <param name="check"> whether to check array's bound </param>
        //public void Reverse(bool check = false)
        //{
        //    long start = 0;
        //    long end = Count - 1;
        //    while (start < end)
        //    {
        //        double temp = this[start, checkBound: check];
        //        this[start, checkBound: check] = this[end, checkBound: check];
        //        this[end, checkBound: check] = temp;
        //        start++;
        //        end--;
        //    }
        //}

        #endregion
        #region operators

        #region v-v add [real]
        /// <summary>
        /// computes the sum of two vectors x and y
        /// res[i] = x[i] + y[i]
        /// </summary>
        /// <param name="x"> input vector x </param>
        /// <param name="y"> input vector y </param>
        /// <returns> result vector </returns>
        public static VectorD operator +(VectorD x, VectorD y)
            => VMath.Add(x, y);
        #endregion
        #region v-s add [real]
        /// <summary>
        /// adds a scalar s to vector x
        /// res[i] = x[i] + s
        /// </summary>
        /// <param name="x"> input vector x </param>
        /// <param name="s"> input scalar s </param>
        /// <returns> result vector </returns>
        public static VectorD operator +(VectorD x, double s)
            => VMath.Add(x, s);

        /// <summary>
        /// adds a scalar s to vector x
        /// res[i] = s + x[i]
        /// </summary>
        /// <param name="s"> input scalar s </param>
        /// <param name="x"> input vector x </param>
        /// <returns> result vector </returns>
        public static VectorD operator +(double s, VectorD x)
            => (x + s);
        #endregion
        #region v-s add [mixed]
        /// <summary>
        /// adds a scalar s to vector x
        /// res[i] = s + x[i]
        /// </summary>
        /// <param name="x"> input vector x </param>
        /// <param name="s"> input scalar s </param>
        /// <returns> result vector </returns>
        public static VectorZ operator +(VectorD x, Complex s)
            => new VectorZ(x) + s;

        /// <summary>
        /// adds a scalar s to vector x
        /// res[i] = s + x[i]
        /// </summary>
        /// <param name="x"> input vector x </param>
        /// <param name="s"> input scalar s </param>
        /// <returns> result vector </returns>
        public static VectorZ operator +(Complex s, VectorD x)
            => x + s;
        #endregion
        #region v-v subtract [real]
        /// <summary>
        /// subtracts one vector y from another vector x
        /// res[i] = x[i] - y[i]
        /// </summary>
        /// <param name="x"> input vector x </param>
        /// <param name="y"> input vector y </param>
        /// <returns> result vector </returns>
        public static VectorD operator -(VectorD x, VectorD y)
            => VMath.Sub(x, y);
        #endregion
        #region v-s subtract [real]
        /// <summary>
        /// subtracts a scalar s from vector x
        /// res[i] = x[i] - s
        /// </summary>
        /// <param name="x"> input vector x </param>
        /// <param name="s"> input scalar s </param>
        /// <returns> result vector </returns>
        public static VectorD operator -(VectorD x, double s)
            => VMath.Sub(x, s);

        /// <summary>
        /// subtracts each element of vector x from a scalar s
        /// res[i] = s - x[i]
        /// </summary>
        /// <param name="s"> input scalar s </param>
        /// <param name="x"> input vector x </param>
        /// <returns> result vector </returns>
        public static VectorD operator -(double s, VectorD x)
            => VMath.Sub(s, x);
        #endregion
        #region v-s subtract [mixed]
        /// <summary>
        /// subtracts a scalar s from vector x
        /// res[i] = x[i] - s
        /// </summary>
        /// <param name="x"> input vector x </param>
        /// <param name="s"> input scalar s </param>
        /// <returns> result vector </returns>
        public static VectorZ operator -(VectorD x, Complex s)
            => new VectorZ(x) - s;

        /// <summary>
        /// subtracts each element of vector x 
        /// from a scalar s
        /// res[i] = s - x[i]
        /// </summary>
        /// <param name="s"> input scalar s </param>
        /// <param name="x"> input vector x </param>
        /// <returns> result vector </returns>
        public static VectorZ operator -(Complex s, VectorD x)
            => s - new VectorZ(x);
        #endregion
        #region v-v multiply [real]
        /// <summary>
        /// performs element by element multiplication 
        /// of two vectors x and y
        /// res[i] = x[i] * y[i]
        /// </summary>
        /// <param name="x"> input vector x </param>
        /// <param name="y"> input vector y </param>
        /// <returns> result vector </returns>
        public static VectorD operator *(VectorD x, VectorD y)
            => VMath.Mul(x, y);
        #endregion
        #region v-s multiply [real]
        /// <summary>
        /// multiplies a vector x with a scalar s
        /// res[i] = x[i] * s
        /// </summary>
        /// <param name="x"> input vector x </param>
        /// <param name="s"> input scalar s </param>
        /// <returns> result vector </returns>
        public static VectorD operator *(VectorD x, double s)
        {
            VectorD y = new VectorD(count: x.Count, mode: ArrayInitMode.Malloc);
            VMath.Copy(x, ref y);
            VMath.ScaleOn(ref y, s);
            return y;
        }
            //=> VMath.Scale(x, s);

        /// <summary>
        /// multiplies a scalar s with a vector x
        /// res[i] = s * x[i]
        /// </summary>
        /// <param name="s"> input scalar s </param>
        /// <param name="x"> input vector x </param>
        /// <returns> result vector </returns>
        public static VectorD operator *(double s, VectorD x)
            => (x * s);
        #endregion
        #region v-s multiply [mixed]
        /// <summary>
        /// multiplies a vector x with a scalar s
        /// res[i] = x[i] * s
        /// </summary>
        /// <param name="x"> input vector x </param>
        /// <param name="s"> input scalar s </param>
        /// <returns> result vector </returns>
        public static VectorZ operator *(VectorD x, Complex s)
            => new VectorZ(x) * s;

        /// <summary>
        /// multiplies a scalar s with a vector x
        /// res[i] = s * x[i]
        /// </summary>
        /// <param name="s"> input scalar s </param>
        /// <param name="x"> input vector x </param>
        /// <returns> result vector </returns>
        public static VectorZ operator *(Complex s, VectorD x)
            => x * s;
        #endregion
        #region v-v divide [real]
        /// <summary>
        /// performs element by element inversion 
        /// of two vectors x and y
        /// res[i] = x[i] / y[i]
        /// </summary>
        /// <param name="x"> input vector x </param>
        /// <param name="y"> input vector y </param>
        /// <returns> result vector </returns>
        public static VectorD operator /(VectorD x, VectorD y)
            => VMath.Div(x, y);
        #endregion
        #region v-s divide [real]
        /// <summary>
        /// divides a vector x by scalar s
        /// res[i] = x[i] / s
        /// </summary>
        /// <param name="x"> input vector x </param>
        /// <param name="s"> input scalar s </param>
        /// <returns> result vector </returns>
        public static VectorD operator /(VectorD x, double s)
        {
            VectorD y = new VectorD(count: x.Count, mode: ArrayInitMode.Malloc);
            VMath.Copy(x, ref y);
            VMath.ScaleOn(ref y, 1.0 / s);
            return y;
        }
            //=> VMath.Scale(x, 1.0 / s);

        /// <summary>
        /// divides a scalar s by vector x
        /// res[i] = s / x[i]
        /// </summary>
        /// <param name="s"> input scalar s </param>
        /// <param name="x"> input vector x </param>
        /// <returns> result vector </returns>
        public static VectorD operator /(double s, VectorD x)
        {
            VectorD res = VMath.Inv(x);
            VMath.ScaleOn(ref res, s);
            return res;
        }
        #endregion
        #region v-s divide [mixed]
        ///// <summary>
        ///// divides a vector x by scalar s
        ///// res[i] = x[i] / s
        ///// </summary>
        ///// <param name="x"> input vector x </param>
        ///// <param name="s"> input scalar s </param>
        ///// <returns> result vector </returns>
        //public static VectorZ operator /(VectorD x, Complex s)
        //    => new VectorZ(x) / s;

        ///// <summary>
        ///// divides a scalar s by vector x
        ///// res[i] = s / x[i]
        ///// </summary>
        ///// <param name="s"> input scalar s </param>
        ///// <param name="x"> input vector x </param>
        ///// <returns> result vector </returns>
        //public static VectorZ operator /(Complex s, VectorD x)
        //    => s / new VectorZ(x);
        #endregion
        #region negative
        /// <summary>
        /// takes the negative of the input vector
        /// </summary>
        /// <param name="x"> input vector x </param>
        /// <returns> result negative vector </returns>
        public static VectorD operator -(VectorD x)
        {
            VectorD y = new VectorD(count: x.Count, mode: ArrayInitMode.Malloc);
            VMath.Copy(x, ref y);
            VMath.ScaleOn(ref y, -1.0);
            return y;
        }
            //=> VMath.Scale(x, -1.0);
        #endregion
        #region explicit conversion

        /// <summary>
        /// explicit conversion from VectorD to VectorZ
        /// </summary>
        /// <param name="v"> input real-part vector </param>
        public static explicit operator VectorZ(VectorD v)
            => new(part: v, option: ComplexPart.RealPart);

        #endregion

        #endregion
    }

    /// <summary>
    /// complex vector class
    /// </summary>
    public class VectorZ : Vector<Complex>
    {
        #region fields

        /// <summary>
        /// empty vector with ZERO count
        /// </summary>
        public static VectorZ Empty = new(0);

        #endregion
        #region properties

        /// <summary>
        /// gets the type-specific pointer to the values
        /// </summary>
        public unsafe Complex* SPtr
        {
            get => (Complex*)DataPtr.ToPointer();
        }

        ///// <summary>
        ///// gets the real part of the complex vector
        ///// </summary>
        //public VectorD RealPart => VMath.RealPart(this);

        ///// <summary>
        ///// gets the imaginary part of the complex vector
        ///// </summary>
        //public VectorD ImagPart => VMath.ImagPart(this);

        ///// <summary>
        ///// gets the magnitude of the complex vector
        ///// </summary>
        //public VectorD Magnitude => VMath.Abs(this);

        ///// <summary>
        ///// gets the argument of the complex vector
        ///// </summary>
        //public VectorD Argument => VMath.Arg(this);

        /// <summary>
        /// get / set the vector elements within a given range
        /// </summary>
        /// <param name="rng"> range with inclusive start and exclusive end indices [Int64] </param>
        /// <param name="loopMode"> loop computational mode option </param>
        /// <returns> vector elements within the range </returns>
        public new VectorZ this[LongRange rng, LoopMode loopMode = Defaults.LoopOption]
        {
            get
            {
                if (!IsRangeValid(rng)) { throw new ArgumentException("Invalid range", nameof(rng)); }

                long n = rng.End - rng.Start;
                VectorZ x = new(count: n, mode: ArrayInitMode.Malloc);

                Action<long> a = (i) => { x[i - rng.Start, false] = this[i, false]; };
                Loop1D loop = new(operation: a, start: rng.Start, end: rng.End);
                loop.Evaluate(loopMode);

                return x;
            }
            set
            {
                //if (value.GetType() != typeof(VectorZ)) { throw new ArgumentException("Invalid input", nameof(value)); }
                if (!IsRangeValid(rng)) { throw new ArgumentException("Invalid range", nameof(rng)); }

                Action<long> a = (i) => { this[i, false] = value[i - rng.Start, false]; };
                Loop1D loop = new(operation: a, start: rng.Start, end: rng.End);
                loop.Evaluate(loopMode);
            }
        }

        /// <summary>
        /// get / set the vector elements within a given range
        /// </summary>
        /// <param name="rng"> range with inclusive start and exclusive end indices [Int32] </param>
        /// <param name="loopMode"> loop computational mode option </param>
        /// <returns> vector elements within the range </returns>
        public new VectorZ this[Range rng, LoopMode loopMode = Defaults.LoopOption]
        {
            get => this[new LongRange(rng, Count), loopMode];
            set => this[new LongRange(rng, Count), loopMode] = value;
        }

        #endregion
        #region constructors

        /// <summary>
        /// constructs a vector with given length
        /// by default, does not initialize element values
        /// </summary>
        /// <param name="count"> count of the elements </param>
        /// <param name="mode"> construct mode option; default is CALLOC </param>
        public VectorZ(long count,
            ArrayInitMode mode = ArrayInitMode.Calloc)
            : base(count, mode) { }

        /// <summary>
        /// constructs a vector with given length
        /// and sets all elements to the same value 
        /// </summary>
        /// <param name="count"> count of the elements </param>
        /// <param name="initVal"> initial value for all elements </param>
        /// <param name="loopMode"> loop-computation mode option </param>
        public VectorZ(long count, Complex initVal,
            LoopMode loopMode = Defaults.LoopOption)
            : base(count, initVal, loopMode) { }

        /// <summary>
        /// constructs a vector with given length
        /// and sets all elements to the same value 
        /// </summary>
        /// <param name="count"> count of the elements </param>
        /// <param name="initVal"> initial value for all elements </param>
        public VectorZ(long count, double initVal)
            : this(count, new Complex(initVal, 0.0)) { }

        /// <summary>
        /// constructs a vector with given length
        /// first element to the initial value
        /// and the rest follow the increment
        /// </summary>
        /// <param name="count"> count of the elements </param>
        /// <param name="initVal"> value of the first element </param>
        /// <param name="increment"> increment for the rest elements </param>
        /// <param name="loopMode"> loop-computation mode option </param>
        public VectorZ(long count, Complex initVal, Complex increment,
            LoopMode loopMode = Defaults.LoopOption)
            : this(count)
        {
            Action<long> a = (i) => { this[i, false] = initVal + i * increment; };
            Loop1D loop = new(operation: a, start: 0, end: count);
            loop.Evaluate(loopMode);
        }

        /// <summary>
        /// constructs a vector with given length
        /// first element to the initial value
        /// and the rest follow the increment
        /// </summary>
        /// <param name="count"> count of the elements </param>
        /// <param name="initVal"> value of the first element </param>
        /// <param name="increment"> increment for the rest elements </param>
        /// <param name="loopMode"> loop-computation mode option </param>
        public VectorZ(long count, double initVal, double increment,
            LoopMode loopMode = Defaults.LoopOption)
            : this(count, new Complex(initVal, 0.0), new Complex(increment, 0.0), loopMode) { }

        ///// <summary>
        ///// constructs a vector with given
        ///// initial value, end value, and length
        ///// </summary>
        ///// <param name="start"> value of the first element </param>
        ///// <param name="end"> value of the ending element </param>
        ///// <param name="count"> count of the elements </param>
        ///// <param name="loopMode"> loop-computation mode option </param>
        //public VectorZ(Complex start, Complex end, long count,
        //    LoopMode loopMode = Defaults.LoopOption)
        //    : this(count, start, (end - start) / (count - 1), loopMode) { }

        ///// <summary>
        ///// constructs a vector with given
        ///// initial value, end value, and length
        ///// </summary>
        ///// <param name="start"> value of the first element </param>
        ///// <param name="end"> value of the ending element </param>
        ///// <param name="count"> count of the elements </param>
        ///// <param name="loopMode"> loop-computation mode option </param>
        //public VectorZ(double start, double end, long count,
        //    LoopMode loopMode = Defaults.LoopOption)
        //    : this(count, start, (end - start) / (count - 1), loopMode) { }

        /// <summary>
        /// constructs a vector by copying from another
        /// </summary>
        /// <param name="other"> another vector </param>
        /// <param name="deepCopy"> copy mode option; default is deep copy </param>
        public VectorZ(VectorZ other, bool deepCopy = true)
            : this(other.Count, deepCopy ? ArrayInitMode.Malloc : ArrayInitMode.None)
        {
            if (deepCopy)
            {
                VectorZ t = this;
                //VMath.CopyZ(other, ref t);
                unsafe{ Defaults.IBLAS.Copy(n: other.Count, x: other.VPtr,
                    y: t.VPtr, incx: 1, incy: 1); }

            }
            else
            { DataPtr = other.DataPtr; }
        }

        /// <summary>
        /// constructs a vector by copying from a given ArrayBase
        /// </summary>
        /// <param name="other"> another ArrayBase </param>
        /// <param name="deepCopy"> copy mode option; default is deep copy </param>
        public VectorZ(DenseArrayBase<Complex> other, bool deepCopy = true)
            : this(other.Count, deepCopy ? ArrayInitMode.Malloc : ArrayInitMode.None)
        {
            if (deepCopy)
            {
                DenseArrayBase<Complex> t = this;
                //VMath.CopyZ(other, ref t);
                unsafe{ Defaults.IBLAS.Copy(n: other.Count, x: other.VPtr,
                    y: t.VPtr, incx: 1, incy: 1); }
            }
            else
            { DataPtr = other.DataPtr; }
        }

        /// <summary>
        /// constructs a complex vector 
        /// with its real or imaginary part only
        /// </summary>
        /// <param name="part"> part of the vector </param>
        /// <param name="option"> option for the complex part; default is real-part </param>
        public VectorZ(VectorD part, ComplexPart option = ComplexPart.RealPart)
            : this(part.Count, 0.0)
        {
            switch (option)
            {
                case ComplexPart.RealPart:
                    {
                        VectorZ t = this;
                        VMath.ModifyReal(part, ref t);
                        break;
                    }
                case ComplexPart.ImagPart:
                    {
                        VectorZ t = this;
                        VMath.ModifyImag(part, ref t);
                        break;
                    }
                default: goto case ComplexPart.RealPart;
            }
        }

        #endregion
        #region naive methods

        ///// <summary>
        /////  sums up all the elements in x
        ///// </summary>
        ///// <returns> summation result </returns>
        //public Complex Sum(LoopMode mode = Defaults.LoopOption)
        //    => VMath.Sum(x: this, loopMode: mode);

        ///// <summary>
        ///// converts to array
        ///// </summary>
        ///// <returns> result array </returns>
        //public Complex[] ToArray(LoopMode mode = Defaults.LoopOption)
        //    => VMath.ConvertVectorToArray(x: this, loopMode: mode);

        #endregion
        #region extra methods

        ///// <summary>
        ///// check if a given index is valid
        ///// the lower bound is set to zero 
        ///// the upper bound is set to Count
        ///// </summary>
        ///// <param name="i"> input index </param>
        ///// <returns> return true if the index is valid
        ///// otherwise return false </returns>
        //public new bool IsIndexValid(long i)
        //    => IsIndexValid(i, Count, GetType().Name);

        ///// <summary>
        ///// check if a given range is valid
        ///// the lower bound is set to zero 
        ///// the upper bound is set to Count
        ///// </summary>
        ///// <param name="rng"> input range </param>
        ///// <returns> return true if the range is valid
        ///// otherwise return false </returns>
        //public new bool IsRangeValid(LongRange rng)
        //    => IsRangeValid(rng, Count, GetType().Name);

        ///// <summary>
        ///// checks whether this vector has
        ///// the same count as the other
        ///// </summary>
        ///// <param name="other"> the other vector </param>
        ///// <returns> return true if same; otherwise false </returns>
        //public bool HasSameCount(VectorD other)
        //{
        //    if (other.Count == Count)
        //        return true;
        //    else
        //        Console.Write(GetType().Name + ": " + "Unequal vector length");
        //    return false;
        //}

        ///// <summary>
        ///// checks whether this vector has
        ///// the same count as the other
        ///// </summary>
        ///// <param name="other"> the other vector </param>
        ///// <returns> return true if same; otherwise false </returns>
        //public bool HasSameCount(VectorZ other)
        //{
        //    if (other.Count == Count)
        //        return true;
        //    else
        //        Console.Write(GetType().Name + ": " + "Unequal vector length");
        //    return false;
        //}

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
        /// <param name="targetCount"> target number of elements </param>
        /// <returns></returns>
        public VectorZ Padding(long targetCount)
        {
            if ((targetCount - Count) % 2 != 0)
            {
                Printer.Warning($"{nameof(targetCount)} must be an even addition to the current value");
                return Empty;
            }
            return Padding(targetCount, (targetCount - Count) / 2, 0.0);
        }

        /// <summary>
        /// truncates current vector according to target parameters
        /// </summary>
        /// <param name="targetCount"> target number of elements in the truncated vector </param>
        /// <param name="startIndex"> starting index in the original vector </param>
        /// <returns> result vector after truncation </returns>
        public VectorZ Truncate(long targetCount, long startIndex)
        {
            if (startIndex + targetCount >= Count)
            {
                Printer.Warning($"invalid combination of parameters {nameof(targetCount)} and {nameof(startIndex)}");
                return Empty;
            }

            LongRange rng = new(startIndex, startIndex + targetCount);
            return this[rng];
        }

        /// <summary>
        /// centered truncation on both sides of the vector
        /// </summary>
        /// <param name="targetCount"> target number of elements </param>
        /// <returns> result vector after truncation </returns>
        public VectorZ Truncate(long targetCount)
        {
            if ((Count - targetCount) % 2 != 0)
            {
                Printer.Warning($"{nameof(targetCount)} must be an even subtraction of the current value");
                return Empty;
            }
            return Truncate(targetCount, (Count - targetCount) / 2);
        }

        /// <summary>
        /// replicates according to target number of elements
        /// </summary>
        /// <param name="targetCount"> target number of elements after replication </param>
        /// <returns> replicated result </returns>
        public VectorZ Replicate(long targetCount)
        {
            if (targetCount <= Count) { Printer.Warning($"Target count not greater than the current"); }
            VectorZ rp = new(targetCount);
            // computes replication multiple and residual count
            long m = (long)(targetCount / Count);
            long rest = targetCount - m * Count;
            // loop for multiple replications
            for (long n = 0; n < m; n++)
            {
                LongRange unit = new(n * Count + 0, n * Count + Count);
                rp[unit] = this;
            }
            // loop for the residuals
            for (long i = 0; i < rest; i++)
                rp[m * Count + i, false] = this[i, false];

            return rp;
        }

        /// <summary>
        /// reverses array using while loop
        /// </summary>
        /// <param name="check"> whether to check array's bound </param>
        public void Reverse(bool check = false)
        {
            long start = 0;
            long end = Count - 1;
            while (start < end)
            {
                Complex temp = this[start, checkBound: check];
                this[start, checkBound: check] = this[end, checkBound: check];
                this[end, checkBound: check] = temp;
                start++;
                end--;
            }
        }

        #endregion
        #region operators

        #region v-v add [complex]
        /// <summary>
        /// computes the sum of two vectors x and y
        /// res[i] = x[i] + y[i]
        /// </summary>
        /// <param name="x"> input vector x </param>
        /// <param name="y"> input vector y </param>
        /// <returns> result vector </returns>
        public static VectorZ operator +(VectorZ x, VectorZ y)
            => VMath.Add(x, y);
        #endregion
        #region v-v add [mixed]
        /// <summary>
        /// computes the sum of two vectors x and y
        /// res[i] = x[i] + y[i]
        /// </summary>
        /// <param name="x"> input vector x </param>
        /// <param name="y"> input vector y </param>
        /// <returns> result vector </returns>
        public static VectorZ operator +(VectorZ x, VectorD y)
            => x + new VectorZ(y);

        /// <summary>
        /// computes the sum of two vectors x and y
        /// res[i] = x[i] + y[i]
        /// </summary>
        /// <param name="x"> input vector x </param>
        /// <param name="y"> input vector y </param>
        /// <returns> result vector </returns>
        public static VectorZ operator +(VectorD x, VectorZ y)
            => y + x;
        #endregion
        #region v-s add [complex]
        /// <summary>
        /// adds a scalar s to vector x
        /// res[i] = x[i] + s
        /// </summary>
        /// <param name="x"> input vector x </param>
        /// <param name="s"> input scalar s </param>
        /// <returns> result vector </returns>
        public static VectorZ operator +(VectorZ x, Complex s)
            => VMath.Add(x, s);

        /// <summary>
        /// adds a scalar s to vector x
        /// res[i] = s + x[i]
        /// </summary>
        /// <param name="s"> input scalar s </param>
        /// <param name="x"> input vector x </param>
        /// <returns> result vector </returns>
        public static VectorZ operator +(Complex s, VectorZ x)
            => VMath.Add(x, s);
        #endregion
        #region v-s add [mixed]
        /// <summary>
        /// adds a scalar s to vector x
        /// res[i] = s + x[i]
        /// </summary>
        /// <param name="x"> input vector x </param>
        /// <param name="s"> input scalar s </param>
        /// <returns> result vector </returns>
        public static VectorZ operator +(VectorZ x, double s)
            => x + new Complex(s, 0.0);

        /// <summary>
        /// adds a scalar s to vector x
        /// res[i] = s + x[i]
        /// </summary>
        /// <param name="s"> input scalar s </param>
        /// <param name="x"> input vector x </param>
        /// <returns> result vector </returns>
        public static VectorZ operator +(double s, VectorZ x)
            => x + s;
        #endregion
        #region v-v subtract [complex]
        /// <summary>
        /// subtracts one vector y from another vector x
        /// res[i] = x[i] - y[i]
        /// </summary>
        /// <param name="x"> input vector x </param>
        /// <param name="y"> input vector y </param>
        /// <returns> result vector </returns>
        public static VectorZ operator -(VectorZ x, VectorZ y)
            => VMath.Sub(x, y);
        #endregion
        #region v-v subtract [mixed]
        /// <summary>
        /// subtracts one vector y from another vector x
        /// res[i] = x[i] - y[i]
        /// </summary>
        /// <param name="x"> input vector x </param>
        /// <param name="y"> input vector y </param>
        /// <returns> result vector </returns>
        public static VectorZ operator -(VectorZ x, VectorD y)
            => x - new VectorZ(y);
        /// <summary>
        /// subtracts one vector y from another vector x
        /// res[i] = x[i] - y[i]
        /// </summary>
        /// <param name="x"> input vector x </param>
        /// <param name="y"> input vector y </param>
        /// <returns> result vector </returns>
        public static VectorZ operator -(VectorD x, VectorZ y)
            => new VectorZ(x) - y;
        #endregion
        #region v-s subtract [complex]
        /// <summary>
        /// subtracts a scalar s from vector x
        /// res[i] = x[i] - s
        /// </summary>
        /// <param name="x"> input vector x </param>
        /// <param name="s"> input scalar s </param>
        /// <returns> result vector </returns>
        /// <returns></returns>
        public static VectorZ operator -(VectorZ x, Complex s)
            => VMath.Sub(x, s);

        /// <summary>
        /// subtracts each element of vector x from a scalar s
        /// res[i] = s - x[i]
        /// </summary>
        /// <param name="s"> input scalar s </param>
        /// <param name="x"> input vector x </param>
        /// <returns> result vector </returns>
        public static VectorZ operator -(Complex s, VectorZ x)
            => VMath.Sub(s, x);
        #endregion
        #region v-s subtract [mixed]
        /// <summary>
        /// subtracts a scalar s from vector x
        /// res[i] = x[i] - s
        /// </summary>
        /// <param name="x"> input vector x </param>
        /// <param name="s"> input scalar s </param>
        /// <returns> result vector </returns>
        public static VectorZ operator -(VectorZ x, double s)
            => x - new Complex(s, 0.0);

        /// <summary>
        /// substracts each element of vector x from a scalar s
        /// res[i] = s - x[i]
        /// </summary>
        /// <param name="s"> input scalar s </param>
        /// <param name="x"> input vector x </param>
        /// <returns> result vector </returns>
        public static VectorZ operator -(double s, VectorZ x)
            => new Complex(s, 0.0) - x;
        #endregion
        #region v-v multiply [complex]
        /// <summary>
        /// performs element by element multiplication 
        /// of two vectors x and y
        /// res[i] = x[i] * y[i]
        /// </summary>
        /// <param name="x"> input vector x </param>
        /// <param name="y"> input vector y </param>
        public static VectorZ operator *(VectorZ x, VectorZ y)
            => VMath.Mul(x, y);
        #endregion
        #region v-v multiply [mixed]
        /// <summary>
        /// performs element by element multiplication 
        /// of two vectors x and y
        /// res[i] = x[i] * y[i]
        /// </summary>
        /// <param name="x"> input vector x </param>
        /// <param name="y"> input vector y </param>
        /// <returns> result vector </returns>
        public static VectorZ operator *(VectorZ x, VectorD y)
            => x * new VectorZ(y);

        /// <summary>
        /// performs element by element multiplication 
        /// of two vectors x and y
        /// res[i] = x[i] * y[i]
        /// </summary>
        /// <param name="x"> input vector x </param>
        /// <param name="y"> input vector y </param>
        /// <returns> result vector </returns>
        public static VectorZ operator *(VectorD x, VectorZ y)
            => y * x;
        #endregion
        #region v-s multiply [complex]
        /// <summary>
        /// multiplies a vector x with a scalar s
        /// res[i] = x[i] * s
        /// </summary>
        /// <param name="x"> input vector x </param>
        /// <param name="s"> input scalar s </param>
        /// <returns> result vector </returns>
        public static VectorZ operator *(VectorZ x, Complex s)
        {
            VectorZ y = new VectorZ(count: x.Count, mode: ArrayInitMode.Malloc);
            VMath.Copy(x, ref y);
            VMath.ScaleOn(ref y, s);
            return y;
        }
        //=> VMath.Scale(x, s);

        /// <summary>
        /// multiplies a scalar s with a vector x
        /// res[i] = s * x[i]
        /// </summary>
        /// <param name="s"> input scalar s </param>
        /// <param name="x"> input vector x </param>
        /// <returns> result vector </returns>
        public static VectorZ operator *(Complex s, VectorZ x)
            => (x * s); //VMath.Scale(x, s);
        #endregion
        #region v-s multiply [mixed]
        /// <summary>
        /// multiplies a vector x with a scalar s
        /// res[i] = x[i] * s
        /// </summary>
        /// <param name="x"> input vector x </param>
        /// <param name="s"> input scalar s </param>
        /// <returns> result vector </returns>
        public static VectorZ operator *(VectorZ x, double s)
            => x * new Complex(s, 0.0);

        /// <summary>
        /// multiplies a scalar s with a vector x
        /// res[i] = s * x[i]
        /// </summary>
        /// <param name="s"> input scalar s </param>
        /// <param name="x"> input vector x </param>
        /// <returns> result vector </returns>
        public static VectorZ operator *(double s, VectorZ x)
            => x * s;
        #endregion
        #region v-v divide [complex]
        /// <summary>
        /// performs element by element inversion 
        /// of two vectors x and y
        /// res[i] = x[i] / y[i]
        /// </summary>
        /// <param name="x"> input vector x </param>
        /// <param name="y"> input vector y </param>
        /// <returns> result vector </returns>
        public static VectorZ operator /(VectorZ x, VectorZ y)
            => VMath.Div(x, y);
        #endregion
        #region v-v divide [mixed]
        /// <summary>
        /// performs element by element inversion 
        /// of two vectors x and y
        /// res[i] = x[i] / y[i]
        /// </summary>
        /// <param name="x"> input vector x </param>
        /// <param name="y"> input vector y </param>
        /// <returns> result vector </returns>
        public static VectorZ operator /(VectorZ x, VectorD y)
            => x / new VectorZ(y);

        /// <summary>
        /// performs element by element inversion 
        /// of two vectors x and y
        /// res[i] = x[i] / y[i]
        /// </summary>
        /// <param name="x"> input vector x </param>
        /// <param name="y"> input vector y </param>
        /// <returns> result vector </returns>
        public static VectorZ operator /(VectorD x, VectorZ y)
            => new VectorZ(x) / y;
        #endregion
        #region v-s divide [complex]
        /// <summary>
        /// divides a vector x by scalar s
        /// res[i] = x[i] / s
        /// </summary>
        /// <param name="x"> input vector x </param>
        /// <param name="s"> input scalar s </param>
        /// <returns> result vector </returns>
        public static VectorZ operator /(VectorZ x, Complex s)
        {
            VectorZ y = new VectorZ(count: x.Count, mode: ArrayInitMode.Malloc);
            VMath.Copy(x, ref y);
            VMath.ScaleOn(ref y, 1.0 / s);
            return y;
        }
            //=> VMath.Scale(x, 1.0 / s);

        /// <summary>
        /// divides a scalar s by vector x
        /// res[i] = s / x[i]
        /// </summary>
        /// <param name="s"> input scalar s </param>
        /// <param name="x"> input vector x </param>
        /// <returns> result vector </returns>
        public static VectorZ operator /(Complex s, VectorZ x)
            => new VectorZ(x.Count, s) / x;
        #endregion
        #region v-s divide [mixed]
        /// <summary>
        /// divides a vector x by scalar s
        /// res[i] = x[i] / s
        /// </summary>
        /// <param name="x"> input vector x </param>
        /// <param name="s"> input scalar s </param>
        /// <returns> result vector </returns>
        public static VectorZ operator /(VectorZ x, double s)
            => x / new Complex(s, 0.0);

        /// <summary>
        /// divides a scalar s by vector x
        /// res[i] = s / x[i]
        /// </summary>
        /// <param name="s"> input scalar s </param>
        /// <param name="x"> input vector x </param>
        /// <returns> result vector </returns>
        public static VectorZ operator /(double s, VectorZ x)
            => new Complex(s, 0.0) / x;
        #endregion
        #region negative
        /// <summary>
        /// takes the negative of the input vector
        /// </summary>
        /// <param name="x"> input vector x </param>
        /// <returns> result negative vector </returns>
        public static VectorZ operator -(VectorZ x)
        {
            VectorZ y = new VectorZ(count: x.Count, mode: ArrayInitMode.Malloc);
            VMath.Copy(x, ref y);
            VMath.ScaleOn(ref y, -1.0);
            return y;
        }
            //=> VMath.Scale(x, -1.0);
        #endregion

        #endregion
    }

    /// <summary>
    /// two-dimensional real-valued vector class
    /// </summary>
    public class VecD2
    {
        #region fields

        /// <summary>
        /// zero vector
        /// </summary>
        public static VecD2 Zeros
        {
            get
            {
                return new VecD2(0.0, 0.0);
            }
        }

        /// <summary>
        /// unit vector along x
        /// </summary>
        public static VecD2 UnitX
        {
            get
            {
                return new VecD2(1.0, 0.0);
            }
        }

        /// <summary>
        /// uni vector along y
        /// </summary>
        public static VecD2 UnitY
        {
            get
            {
                return new VecD2(0.0, 1.0);
            }
        }

        #endregion
        #region properties

        /// <summary>
        /// the x-component of the 2D vector
        /// </summary>
        public double X { get; set; }

        /// <summary>
        /// the y-component of the 2D vector
        /// </summary>
        public double Y { get; set; }

        #endregion
        #region constructors

        /// <summary>
        /// constructs a 2D vector 
        /// with its x and y components
        /// </summary>
        /// <param name="x"> x-component </param>
        /// <param name="y"> y-component </param>
        public VecD2(double x, double y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// constructs a 2D vector
        /// by deep copy from another
        /// </summary>
        /// <param name="a"> another vector a </param>
        public VecD2(VecD2 a)
        {
            X = a.X;
            Y = a.Y;
        }

        /// <summary>
        /// default constructor
        /// with x = 0 and y = 0
        /// </summary>
        public VecD2() : this(0.0, 0.0) { }

        #endregion
        #region methods

        /// <summary>
        /// normalizes the vector 
        /// </summary>
        public void Normalize()
        {
            double rho = Norm(this);
            X /= rho;
            Y /= rho;
        }

        ///// <summary>
        ///// dot-product with another vector
        ///// </summary>
        ///// <param name="a"> another vector </param>
        ///// <returns> result dot product </returns>
        //public double DotWith(IVec2<double> a)
        //    => X * a.X + Y * a.Y;

        ///// <summary>
        ///// computes the squared norm of this vector
        ///// </summary>
        ///// <returns> squared norm </returns>
        //public double NormSquare()
        //    => X * X + Y * Y;

        ///// <summary>
        ///// computes and norm of this vector
        ///// </summary>
        ///// <returns> norm </returns>
        //public double Norm()
        //    => Math.Sqrt(NormSquare());

        ///// <summary>
        ///// adds another vector to this
        ///// </summary>
        ///// <param name="a"> another vector a </param>
        //public void Add(IVec2<double> a)
        //{
        //    X += a.X;
        //    Y += a.Y;
        //}

        ///// <summary>
        ///// subtracts another vector from this
        ///// </summary>
        ///// <param name="a"> another vector a </param>
        //public void Sub(IVec2<double> a)
        //{
        //    X -= a.X;
        //    Y -= a.Y;
        //}

        ///// <summary>
        ///// component-wise multiply another vector on this
        ///// </summary>
        ///// <param name="a"> another vector a </param>
        //public void Mul(IVec2<double> a)
        //{
        //    X *= a.X;
        //    Y *= a.Y;
        //}

        ///// <summary>
        ///// component-wise division by another vector
        ///// </summary>
        ///// <param name="a"> another vector a </param>
        //public void Div(IVec2<double> a)
        //{
        //    X /= a.X;
        //    Y /= a.Y;
        //}

        ///// <summary>
        ///// takes the negative of this vector
        ///// </summary>
        //public void Neg()
        //{
        //    X *= -1;
        //    Y *= -1;
        //}

        #endregion
        #region static methods

        /// <summary>
        /// computes the dot/inner product 
        /// of two 2D vectors a and b
        /// </summary>
        /// <param name="a"> input vector a  </param>
        /// <param name="b"> input vector b </param>
        /// <returns> result dot product </returns>
        public static double Dot(VecD2 a, VecD2 b)
            => a.X * b.X + a.Y * b.Y;

        /// <summary>
        /// computes the squared norm of vector a 
        /// </summary>
        /// <param name="a"> input vector a </param>
        /// <returns> squared norm </returns>
        public static double NormSquare(VecD2 a)
            => a.X * a.X + a.Y * a.Y;

        /// <summary>
        /// computes and norm of vector a
        /// </summary>
        /// <param name="a"> input vector a</param>
        /// <returns> norm </returns>
        public static double Norm(VecD2 a)
            => Math.Sqrt(NormSquare(a));

        #endregion
        #region operators

        #region ===== plus =====

        /// <summary>
        /// sum of two vectors
        /// </summary>
        /// <param name="a"> input vector a </param>
        /// <param name="b"> input vector b </param>
        /// <returns> sum result </returns>
        public static VecD2 operator +(VecD2 a, VecD2 b)
            => new()
            {
                X = a.X + b.X,
                Y = a.Y + b.Y
            };

        /// <summary>
        /// sum of a vector and a scalar
        /// </summary>
        /// <param name="a"> input vector a </param>
        /// <param name="s"> input scalar s </param>
        /// <returns> sum result </returns>
        public static VecD2 operator +(VecD2 a, double s)
            => a + new VecD2(s, s);

        /// <summary>
        /// sum of a scalar and a vector
        /// </summary>
        /// <param name="s"> input scalar s </param>
        /// <param name="a"> input vector a </param>
        /// <returns> sum result </returns>
        public static VecD2 operator +(double s, VecD2 a)
            => a + s;

        #endregion
        #region ===== minus =====

        /// <summary>
        /// subtracts a vector from another
        /// </summary>
        /// <param name="a"> input vector a </param>
        /// <param name="b"> input vector b </param>
        /// <returns> subtraction result </returns>
        public static VecD2 operator -(VecD2 a, VecD2 b)
            => new()
            {
                X = a.X - b.X,
                Y = a.Y - b.Y
            };

        /// <summary>
        /// subtracts a scalar from a vector 
        /// </summary>
        /// <param name="a"> input vector a </param>
        /// <param name="s"> input scalar s </param>
        /// <returns> subtraction result </returns>
        public static VecD2 operator -(VecD2 a, double s)
            => a - new VecD2(s, s);

        /// <summary>
        /// subtracts each vector component from a scalar 
        /// </summary>
        /// <param name="s"> input scalar s </param>
        /// <param name="a"> input vector a </param>
        /// <returns> subtraction result </returns>
        public static VecD2 operator -(double s, VecD2 a)
            => new VecD2(s, s) - a;

        #endregion
        #region ===== multiply =====

        /// <summary>
        /// component-wise multiplication of two vectors
        /// </summary>
        /// <param name="a"> input vector a </param>
        /// <param name="b"> input vector b </param>
        /// <returns> result vector </returns>
        public static VecD2 operator *(VecD2 a, VecD2 b)
            => new()
            {
                X = a.X * b.X,
                Y = a.Y * b.Y
            };

        /// <summary>
        /// multiplies a vector with a scalar
        /// </summary>
        /// <param name="a"> input vector a </param>
        /// <param name="s"> input scalar s </param>
        /// <returns> result vector </returns>
        public static VecD2 operator *(VecD2 a, double s)
            => new()
            {
                X = a.X * s,
                Y = a.Y * s
            };

        /// <summary>
        /// multiplies a scalar with a vector 
        /// </summary>
        /// <param name="s"> input scalar s </param>
        /// <param name="a"> input vector a </param>
        /// <returns> result vector </returns>
        public static VecD2 operator *(double s, VecD2 a)
            => a * s;

        #endregion
        #region ===== divide =====

        /// <summary>
        /// component-wise division of two vectors
        /// </summary>
        /// <param name="a"> input vector a </param>
        /// <param name="b"> input vector b </param>
        /// <returns> result vector </returns>
        public static VecD2 operator /(VecD2 a, VecD2 b)
            => new()
            {
                X = a.X / b.X,
                Y = a.Y / b.Y
            };

        /// <summary>
        /// divides a vector by a scalar
        /// </summary>
        /// <param name="a"> input vector a </param>
        /// <param name="s"> input scalar s </param>
        /// <returns> result vector </returns>
        public static VecD2 operator /(VecD2 a, double s)
            => new()
            {
                X = a.X / s,
                Y = a.Y / s
            };

        /// <summary>
        /// divides a scalar by each component of a vector 
        /// </summary>
        /// <param name="s"> input scalar s </param>
        /// <param name="a"> input vector a </param>
        /// <returns> result vector </returns>
        public static VecD2 operator /(double s, VecD2 a)
            => new()
            {
                X = s / a.X,
                Y = s / a.Y
            };

        #endregion
        #region ===== negative =====

        /// <summary>
        /// takes the negative of the input vector
        /// </summary>
        /// <param name="v"> input vector </param>
        /// <returns> result negative vector </returns>
        public static VecD2 operator -(VecD2 v)
            => new()
            {
                X = -v.X,
                Y = -v.Y
            };

        #endregion

        #endregion
    }

    /// <summary>
    /// two-dimensional complex-valued vector class
    /// </summary>
    public class VecZ2
    {
        #region properties

        /// <summary>
        /// the x-component of the 2D vector
        /// </summary>
        public Complex X { get; set; }

        /// <summary>
        /// the y-component of the 2D vector
        /// </summary>
        public Complex Y { get; set; }

        #endregion
        #region constructors

        /// <summary>
        /// constructs a 2D vector
        /// with its x and y components
        /// </summary>
        /// <param name="x"> x-component </param>
        /// <param name="y"> y-component </param>
        public VecZ2(Complex x, Complex y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// constructs a real-valued 2D vector
        /// with its x and y components
        /// </summary>
        /// <param name="x"> x-component </param>
        /// <param name="y"> y-component </param>
        public VecZ2(double x, double y) :
            this(new Complex(x, 0.0), new Complex(y, 0.0))
        { }

        /// <summary>
        /// defualt constructor
        /// with x = 0 and y = 0
        /// </summary>
        public VecZ2() : this(0.0, 0.0) { }

        #endregion
        #region methods

        /// <summary>
        /// normalizes the vector 
        /// </summary>
        public void Normalize()
        {
            double rho = Norm(this);
            X /= rho;
            Y /= rho;
        }

        /// <summary>
        /// computes the complex conjugate of the vector
        /// </summary>
        public void Conjugate()
        {
            X = Complex.Conjugate(X);
            Y = Complex.Conjugate(Y);
        }

        #endregion
        #region static methods

        /// <summary>
        /// computes the dot/inner product 
        /// of two 2D vectors a and b
        /// </summary>
        /// <param name="a"> input vector a  </param>
        /// <param name="b"> input vector b </param>
        /// <returns> result dot product </returns>
        public static Complex Dot(VecZ2 a, VecZ2 b)
            => a.X * b.X + a.Y * b.Y;

        /// <summary>
        /// computes the dot/inner product 
        /// of two 2D vectors a and b
        /// </summary>
        /// <param name="a"> input vector a  </param>
        /// <param name="b"> input vector b </param>
        /// <returns> result dot product </returns>
        public static Complex Dot(VecZ2 a, VecD2 b)
            => a.X * b.X + a.Y * b.Y;

        /// <summary>
        /// computes the dot/inner product 
        /// of two 2D vectors a and b
        /// </summary>
        /// <param name="a"> input vector a  </param>
        /// <param name="b"> input vector b </param>
        /// <returns> result dot product </returns>
        public static Complex Dot(VecD2 a, VecZ2 b)
            => Dot(b, a);

        /// <summary>
        /// computes the squared norm of vector a
        /// </summary>
        /// <param name="a"> input vector a </param>
        /// <returns> squared norm </returns>
        public static double NormSquare(VecZ2 a)
        {
            double xAbs = Complex.Abs(a.X);
            double yAbs = Complex.Abs(a.Y);
            return xAbs * xAbs + yAbs * yAbs;
        }

        /// <summary>
        /// computes and norm of vector a
        /// </summary>
        /// <param name="a"> input vector a</param>
        /// <returns> norm </returns>
        public static double Norm(VecZ2 a)
            => Math.Sqrt(NormSquare(a));

        #endregion
        #region operators

        #region ===== plus =====

        /// <summary>
        /// sum of two vectors
        /// </summary>
        /// <param name="a"> input vector a </param>
        /// <param name="b"> input vector b </param>
        /// <returns> sum result </returns>
        public static VecZ2 operator +(VecZ2 a, VecZ2 b)
            => new()
            {
                X = a.X + b.X,
                Y = a.Y + b.Y,
            };

        /// <summary>
        /// sum of two vectors
        /// </summary>
        /// <param name="a"> input vector a </param>
        /// <param name="b"> input vector b </param>
        /// <returns> sum result </returns>
        public static VecZ2 operator +(VecZ2 a, VecD2 b)
            => new()
            {
                X = a.X + b.X,
                Y = a.Y + b.Y
            };

        /// <summary>
        /// sum of two vectors
        /// </summary>
        /// <param name="a"> input vector a </param>
        /// <param name="b"> input vector b </param>
        /// <returns> sum result </returns>
        public static VecZ2 operator +(VecD2 a, VecZ2 b)
            => b + a;

        /// <summary>
        /// sum of a vector and a scalar
        /// </summary>
        /// <param name="a"> input vector a </param>
        /// <param name="s"> input scalar s </param>
        /// <returns> sum result </returns>
        public static VecZ2 operator +(VecZ2 a, Complex s)
            => new()
            {
                X = a.X + s,
                Y = a.Y + s
            };

        /// <summary>
        /// sum of a scalar and a vector
        /// </summary>
        /// <param name="s"> input scalar s </param>
        /// <param name="a"> input vector a </param>
        /// <returns> sum result </returns>
        public static VecZ2 operator +(Complex s, VecZ2 a)
            => a + s;

        /// <summary>
        /// sum of a vector and a scalar
        /// </summary>
        /// <param name="a"> input vector a </param>
        /// <param name="s"> input scalar s </param>
        /// <returns> sum result </returns>
        public static VecZ2 operator +(VecZ2 a, double s)
            => a + new Complex(s, 0.0);

        /// <summary>
        /// sum of a scalar and a vector
        /// </summary>
        /// <param name="s"> input scalar s </param>
        /// <param name="a"> input vector a </param>
        /// <returns> sum result </returns>
        public static VecZ2 operator +(double s, VecZ2 a)
            => new Complex(s, 0.0) + a;

        #endregion
        #region ===== minus =====

        /// <summary>
        /// subtracts a vector from another
        /// </summary>
        /// <param name="a"> input vector a </param>
        /// <param name="b"> input vector b </param>
        /// <returns> subtraction result </returns>
        public static VecZ2 operator -(VecZ2 a, VecZ2 b)
            => new()
            {
                X = a.X - b.X,
                Y = a.Y - b.Y
            };

        /// <summary>
        /// subtracts a vector from another
        /// </summary>
        /// <param name="a"> input vector a </param>
        /// <param name="b"> input vector b </param>
        /// <returns> subtraction result </returns>
        public static VecZ2 operator -(VecZ2 a, VecD2 b)
            => new()
            {
                X = a.X - b.X,
                Y = a.Y - b.Y
            };

        /// <summary>
        /// subtracts a vector from another
        /// </summary>
        /// <param name="a"> input vector a </param>
        /// <param name="b"> input vector b </param>
        /// <returns> subtraction result </returns>
        public static VecZ2 operator -(VecD2 a, VecZ2 b)
            => new VecZ2(a.X, a.Y) - b;

        /// <summary>
        /// subtracts a scalar from a vector 
        /// </summary>
        /// <param name="a"> input vector a </param>
        /// <param name="s"> input scalar s </param>
        /// <returns> subtraction result </returns>
        public static VecZ2 operator -(VecZ2 a, Complex s)
            => new()
            {
                X = a.X - s,
                Y = a.Y - s
            };

        /// <summary>
        /// subtracts each vector component from a scalar 
        /// </summary>
        /// <param name="s"> input scalar s </param>
        /// <param name="a"> input vector a </param>
        /// <returns> subtraction result </returns>
        public static VecZ2 operator -(Complex s, VecZ2 a)
            => new()
            {
                X = s - a.X,
                Y = s - a.Y
            };

        /// <summary>
        /// subtracts a scalar from a vector 
        /// </summary>
        /// <param name="a"> input vector a </param>
        /// <param name="s"> input scalar s </param>
        /// <returns> subtraction result </returns>
        public static VecZ2 operator -(VecZ2 a, double s)
            => a - new Complex(s, 0.0);

        /// <summary>
        /// subtracts each vector component from a scalar 
        /// </summary>
        /// <param name="s"> input scalar s </param>
        /// <param name="a"> input vector a </param>
        /// <returns> subtraction result </returns>
        public static VecZ2 operator -(double s, VecZ2 a)
            => new Complex(s, 0.0) - a;

        #endregion
        #region ===== multiply =====

        /// <summary>
        /// component-wise multiplication of two vectors
        /// </summary>
        /// <param name="a"> input vector a </param>
        /// <param name="b"> input vector b </param>
        /// <returns> result vector </returns>
        public static VecZ2 operator *(VecZ2 a, VecZ2 b)
            => new()
            {
                X = a.X * b.X,
                Y = a.Y * b.Y
            };

        /// <summary>
        /// component-wise multiplication of two vectors
        /// </summary>
        /// <param name="a"> input vector a </param>
        /// <param name="b"> input vector b </param>
        /// <returns> result vector </returns>
        public static VecZ2 operator *(VecZ2 a, VecD2 b)
            => new()
            {
                X = a.X * b.X,
                Y = a.Y * b.Y
            };

        /// <summary>
        /// component-wise multiplication of two vectors
        /// </summary>
        /// <param name="a"> input vector a </param>
        /// <param name="b"> input vector b </param>
        /// <returns> result vector </returns>
        public static VecZ2 operator *(VecD2 a, VecZ2 b)
            => b * a;

        /// <summary>
        /// multiplies a vector with a scalar
        /// </summary>
        /// <param name="a"> input vector a </param>
        /// <param name="s"> input scalar s </param>
        /// <returns> result vector </returns>
        public static VecZ2 operator *(VecZ2 a, Complex s)
            => new()
            {
                X = a.X * s,
                Y = a.Y * s
            };

        /// <summary>
        /// multiplies a scalar with a vector 
        /// </summary>
        /// <param name="s"> input scalar s </param>
        /// <param name="a"> input vector a </param>
        /// <returns> result vector </returns>
        public static VecZ2 operator *(Complex s, VecZ2 a)
            => a * s;

        /// <summary>
        /// multiplies a vector with a scalar
        /// </summary>
        /// <param name="a"> input vector a </param>
        /// <param name="s"> input scalar s </param>
        /// <returns> result vector </returns>
        public static VecZ2 operator *(VecZ2 a, double s)
            => a * new Complex(s, 0.0);

        /// <summary>
        /// multiplies a scalar with a vector 
        /// </summary>
        /// <param name="s"> input scalar s </param>
        /// <param name="a"> input vector a </param>
        /// <returns> result vector </returns>
        public static VecZ2 operator *(double s, VecZ2 a)
            => new Complex(s, 0.0) * a;

        #endregion
        #region ===== divide =====

        /// <summary>
        /// component-wise division of two vectors
        /// </summary>
        /// <param name="a"> input vector a </param>
        /// <param name="b"> input vector b </param>
        /// <returns> result vector </returns>
        public static VecZ2 operator /(VecZ2 a, VecZ2 b)
            => new()
            {
                X = a.X / b.X,
                Y = a.Y / b.Y
            };

        /// <summary>
        /// component-wise division of two vectors
        /// </summary>
        /// <param name="a"> input vector a </param>
        /// <param name="b"> input vector b </param>
        /// <returns> result vector </returns>
        public static VecZ2 operator /(VecZ2 a, VecD2 b)
            => new()
            {
                X = a.X / b.X,
                Y = a.Y / b.Y
            };

        /// <summary>
        /// component-wise division of two vectors
        /// </summary>
        /// <param name="a"> input vector a </param>
        /// <param name="b"> input vector b </param>
        /// <returns> result vector </returns>
        public static VecZ2 operator /(VecD2 a, VecZ2 b)
            => new VecZ2(a.X, a.Y) / b;

        /// <summary>
        /// divides a vector by a scalar
        /// </summary>
        /// <param name="a"> input vector a </param>
        /// <param name="s"> input scalar s </param>
        /// <returns> result vector </returns>
        public static VecZ2 operator /(VecZ2 a, Complex s)
            => new()
            {
                X = a.X / s,
                Y = a.Y / s
            };

        /// <summary>
        /// divides a scalar by each component of a vector 
        /// </summary>
        /// <param name="s"> input scalar s </param>
        /// <param name="a"> input vector a </param>
        /// <returns> result vector </returns>
        public static VecZ2 operator /(Complex s, VecZ2 a)
            => new()
            {
                X = s / a.X,
                Y = s / a.Y
            };

        /// <summary>
        /// divides a vector by a scalar
        /// </summary>
        /// <param name="a"> input vector a </param>
        /// <param name="s"> input scalar s </param>
        /// <returns> result vector </returns>
        public static VecZ2 operator /(VecZ2 a, double s)
            => a / new Complex(s, 0.0);

        /// <summary>
        /// divides a scalar by each component of a vector 
        /// </summary>
        /// <param name="s"> input scalar s </param>
        /// <param name="a"> input vector a </param>
        /// <returns> result vector </returns>
        public static VecZ2 operator /(double s, VecZ2 a)
            => new Complex(s, 0.0) / a;

        #endregion
        #region ===== negative =====

        /// <summary>
        /// takes the negative of the input vector
        /// </summary>
        /// <param name="v"> input vector </param>
        /// <returns> result negative vector </returns>
        public static VecZ2 operator -(VecZ2 v)
            => new()
            {
                X = -v.X,
                Y = -v.Y
            };

        #endregion

        #endregion
    }

    /// <summary>
    /// three-dimensional real-valued vector class
    /// </summary>
    public class VecD3
    {
        #region fields

        /// <summary>
        /// zero vector
        /// </summary>
        public static VecD3 Zeros
        {
            get
            {
                return new VecD3(0.0, 0.0, 0.0);
            }
        }

        /// <summary>
        /// unit vector along x
        /// </summary>
        public static VecD3 UnitX
        {
            get
            {
                return new VecD3(1.0, 0.0, 0.0);
            }
        }

        /// <summary>
        /// unit vector along y
        /// </summary>
        public static VecD3 UnitY
        {
            get
            {
                return new VecD3(0.0, 1.0, 0.0);
            }
        }

        /// <summary>
        /// unit vector along z
        /// </summary>
        public static VecD3 UnitZ
        {
            get
            {
                return new VecD3(0.0, 0.0, 1.0);
            }
        }

        #endregion
        #region properties

        /// <summary>
        /// x-component
        /// </summary>
        public double X { get; set; }

        /// <summary>
        /// y-component
        /// </summary>
        public double Y { get; set; }

        /// <summary>
        /// z-component
        /// </summary>
        public double Z { get; set; }

        #endregion
        #region constructor

        /// <summary>
        /// constructor with x, y, z input
        /// </summary>
        /// <param name="x"> x-component </param>
        /// <param name="y"> y-component </param>
        /// <param name="z"> z-component </param>
        public VecD3(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        /// <summary>
        /// default with x = 0, y = 0, z = 1
        /// </summary>
        public VecD3() : this(0.0, 0.0, 1.0) { }

        /// <summary>
        /// constructs a 3D vector by deep copy 
        /// from another one
        /// </summary>
        /// <param name="v"> another vector v </param>
        public VecD3(VecD3 v)
        {
            X = v.X;
            Y = v.Y;
            Z = v.Z;
        }

        #endregion
        #region methods

        /// <summary>
        /// normalize the vector
        /// </summary>
        public void Normalize()
        {
            double r = Norm(this);
            X /= r;
            Y /= r;
            Z /= r;
        }

        #endregion
        #region static methods

        /// <summary>
        /// computes the dot/inner product 
        /// of two 3D vectors a and b
        /// </summary>
        /// <param name="a"> input vector a </param>
        /// <param name="b"> input vector b </param>
        /// <returns> result dot product </returns>
        public static double Dot(VecD3 a, VecD3 b)
            => a.X * b.X + a.Y * b.Y + a.Z * b.Z;

        /// <summary>
        /// computes the cross product 
        /// of two 3D vectors a and b
        /// </summary>
        /// <param name="a"> input vector a </param>
        /// <param name="b"> input vector b </param>
        /// <returns> result vector </returns>
        public static VecD3 Cross(VecD3 a, VecD3 b)
            => new()
            {
                X = a.Y * b.Z - a.Z * b.Y,
                Y = a.Z * b.X - a.X * b.Z,
                Z = a.X * b.Y - a.Y * b.X
            };

        /// <summary>
        /// computes the squared norm of the vector
        /// </summary>
        /// <param name="a"> input vector </param>
        /// <returns> squared norm </returns>
        public static double NormSquare(VecD3 a)
            => a.X * a.X + a.Y * a.Y + a.Z * a.Z;

        /// <summary>
        /// computes the norm of the vector
        /// </summary>
        /// <param name="a"> input vector </param>
        /// <returns> norm of the vector </returns>
        public static double Norm(VecD3 a)
            => Math.Sqrt(NormSquare(a));

        #endregion
        #region operators

        #region ===== plus =====

        /// <summary>
        /// sum of two vectors
        /// </summary>
        /// <param name="a"> input vector a </param>
        /// <param name="b"> input vector b </param>
        /// <returns> sum result </returns>
        public static VecD3 operator +(VecD3 a, VecD3 b)
            => new()
            {
                X = a.X + b.X,
                Y = a.Y + b.Y,
                Z = a.Z + b.Z
            };

        /// <summary>
        /// plus a scalar to each element of a vector
        /// </summary>
        /// <param name="a"> input vector a </param>
        /// <param name="s"> input scalar s </param>
        /// <returns> sum result </returns>
        public static VecD3 operator +(VecD3 a, double s)
            => new()
            {
                X = a.X + s,
                Y = a.Y + s,
                Z = a.Z + s
            };

        /// <summary>
        /// plus a scalar to each element of a vector
        /// </summary>
        /// <param name="s"> scalar </param>
        /// <param name="a"> vector </param>
        /// <returns> sum result </returns>
        public static VecD3 operator +(double s, VecD3 a)
            => a + s;

        #endregion
        #region ===== minus =====

        /// <summary>
        /// subtracts a vector from another
        /// </summary>
        /// <param name="a"> input vector a </param>
        /// <param name="b"> input vector b </param>
        /// <returns> subtraction result </returns>
        public static VecD3 operator -(VecD3 a, VecD3 b)
            => new()
            {
                X = a.X - b.X,
                Y = a.Y - b.Y,
                Z = a.Z - b.Z
            };

        /// <summary>
        /// subtracts a scalar from each element of a vector
        /// </summary>
        /// <param name="a"> input vector a </param>
        /// <param name="s"> input scalar s </param>
        /// <returns> result vector </returns>
        public static VecD3 operator -(VecD3 a, double s)
            => new()
            {
                X = a.X - s,
                Y = a.Y - s,
                Z = a.Z - s
            };

        /// <summary>
        /// subtracts a scalar by each element of a vector
        /// </summary>
        /// <param name="s"> input scalar s </param>
        /// <param name="a"> input vector a </param>
        /// <returns> result vector </returns>
        public static VecD3 operator -(double s, VecD3 a)
            => new()
            {
                X = s - a.X,
                Y = s - a.Y,
                Z = s - a.Z
            };

        #endregion
        #region ===== multiply =====

        /// <summary>
        /// component-wise multiplication of two vectors
        /// </summary>
        /// <param name="a"> input vector a </param>
        /// <param name="b"> input vector b </param>
        /// <returns> result vector </returns>
        public static VecD3 operator *(VecD3 a, VecD3 b)
            => new()
            {
                X = a.X * b.X,
                Y = a.Y * b.Y,
                Z = a.Z * b.Z
            };

        /// <summary>
        /// multiply each element of a vector by a scalar
        /// </summary>
        /// <param name="a"> input vector a </param>
        /// <param name="s"> input scalar s </param>
        /// <returns> result vector </returns>
        public static VecD3 operator *(VecD3 a, double s)
            => new()
            {
                X = a.X * s,
                Y = a.Y * s,
                Z = a.Z * s
            };

        /// <summary>
        /// multiply each element of a vector by a scalar
        /// </summary>
        /// <param name="s"> input scalar s </param>
        /// <param name="a"> input vector a </param>
        /// <returns> result vector </returns>
        public static VecD3 operator *(double s, VecD3 a)
            => a * s;

        #endregion
        #region ===== divide =====

        /// <summary>
        /// component-wise division of two vectors 
        /// </summary>
        /// <param name="a"> input vector a </param>
        /// <param name="b"> input vector b </param>
        /// <returns> result vector </returns>
        public static VecD3 operator /(VecD3 a, VecD3 b)
            => new()
            {
                X = a.X / b.X,
                Y = a.Y / b.Y,
                Z = a.Z / b.Z
            };

        /// <summary>
        /// divides each element of a vector by a scalar
        /// </summary>
        /// <param name="a"> input vector a </param>
        /// <param name="s"> input scalar s </param>
        /// <returns> result vector </returns>
        public static VecD3 operator /(VecD3 a, double s)
            => new()
            {
                X = a.X / s,
                Y = a.Y / s,
                Z = a.Y / s
            };

        /// <summary>
        /// divides a scalar by each element of a vector
        /// </summary>
        /// <param name="s"> input scalar s </param>
        /// <param name="a"> input vector a </param>
        /// <returns> result vector </returns>
        public static VecD3 operator /(double s, VecD3 a)
            => new()
            {
                X = s / a.X,
                Y = s / a.Y,
                Z = s / a.Z
            };

        #endregion
        #region ===== negative =====

        /// <summary>
        /// takes the negative of the input vector
        /// </summary>
        /// <param name="v"> input vector </param>
        /// <returns> result negative vector </returns>
        public static VecD3 operator -(VecD3 v)
            => new()
            {
                X = -v.X,
                Y = -v.Y,
                Z = -v.Z
            };

        #endregion

        #endregion
    }

    /// <summary>
    /// three-dimensional complex-valued vector class
    /// </summary>
    public class VecZ3
    {
        #region properties

        /// <summary>
        /// x-component
        /// </summary>
        public Complex X { get; set; }

        /// <summary>
        /// y-component
        /// </summary>
        public Complex Y { get; set; }

        /// <summary>
        /// z-component
        /// </summary>
        public Complex Z { get; set; }

        #endregion
        #region constructors

        /// <summary>
        /// constructor with x, y, z, input
        /// </summary>
        /// <param name="x"> x-component </param>
        /// <param name="y"> y-component </param>
        /// <param name="z"> z-component </param>
        public VecZ3(Complex x, Complex y, Complex z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        /// <summary>
        /// constructor with real-valued x, y, z input
        /// </summary>
        /// <param name="x"> x-component </param>
        /// <param name="y"> y-component </param>
        /// <param name="z"> z-component </param>
        public VecZ3(double x, double y, double z) :
            this(new Complex(x, 0.0), new Complex(y, 0.0), new Complex(z, 0.0))
        { }

        /// <summary>
        /// default constructor with x = 0, y = 0, z = 1
        /// </summary>
        public VecZ3() : this(0.0, 0.0, 1.0) { }

        #endregion
        #region methods

        /// <summary>
        /// normalize the vector
        /// </summary>
        public void Normalize()
        {
            double r = Norm(this);
            X /= r;
            Y /= r;
            Z /= r;
        }

        /// <summary>
        /// computes the complex conjugate of the vector
        /// </summary>
        public void Conjugate()
        {
            X = Complex.Conjugate(X);
            Y = Complex.Conjugate(Y);
            Z = Complex.Conjugate(Z);
        }

        #endregion
        #region static methods

        /// <summary>
        /// computes the dot/inner product 
        /// of two 3D vectors a and b
        /// </summary>
        /// <param name="a"> input vector a </param>
        /// <param name="b"> input vector b </param>
        /// <returns> result dot product </returns>
        public static Complex Dot(VecZ3 a, VecZ3 b)
            => a.X * b.X + a.Y * b.Y + a.Z * b.Z;

        /// <summary>
        /// computes the dot/inner product 
        /// of two 3D vectors a and b
        /// </summary>
        /// <param name="a"> input vector a </param>
        /// <param name="b"> input vector b </param>
        /// <returns> result dot product </returns>
        public static Complex Dot(VecZ3 a, VecD3 b)
            => a.X * b.X + a.Y * b.Y + a.Z * b.Z;

        /// <summary>
        /// computes the dot/inner product 
        /// of two 3D vectors a and b
        /// </summary>
        /// <param name="a"> input vector a </param>
        /// <param name="b"> input vector b </param>
        /// <returns> result dot product </returns>
        public static Complex Dot(VecD3 a, VecZ3 b)
            => Dot(b, a);

        /// <summary>
        /// computes the cross product 
        /// of two 3D vectors a and b
        /// </summary>
        /// <param name="a"> input vector a </param>
        /// <param name="b"> input vector b </param>
        /// <returns> result vector </returns>
        public static VecZ3 Cross(VecZ3 a, VecZ3 b)
            => new()
            {
                X = a.Y * b.Z - a.Z * b.Y,
                Y = a.Z * b.X - a.X * b.Z,
                Z = a.X * b.Y - a.Y * b.X
            };

        /// <summary>
        /// computes the cross product 
        /// of two 3D vectors a and b
        /// </summary>
        /// <param name="a"> input vector a </param>
        /// <param name="b"> input vector b </param>
        /// <returns> result vector </returns>
        public static VecZ3 Cross(VecZ3 a, VecD3 b)
            => new()
            {
                X = a.Y * b.Z - a.Z * b.Y,
                Y = a.Z * b.X - a.X * b.Z,
                Z = a.X * b.Y - a.Y * b.X
            };

        /// <summary>
        /// computes the cross product 
        /// of two 3D vectors a and b
        /// </summary>
        /// <param name="a"> input vector a </param>
        /// <param name="b"> input vector b </param>
        /// <returns> result vector </returns>
        public static VecZ3 Cross(VecD3 a, VecZ3 b)
            => new()
            {
                X = a.Y * b.Z - a.Z * b.Y,
                Y = a.Z * b.X - a.X * b.Z,
                Z = a.X * b.Y - a.Y * b.X
            };

        /// <summary>
        /// computes the squared norm of the vector
        /// </summary>
        /// <param name="a"> input vector </param>
        /// <returns> squared norm </returns>
        public static double NormSquare(VecZ3 a)
        {
            double xAbs = Complex.Abs(a.X);
            double yAbs = Complex.Abs(a.Y);
            double zAbs = Complex.Abs(a.Z);
            return xAbs * xAbs + yAbs * yAbs + zAbs * zAbs;
        }

        /// <summary>
        /// computes the norm of the vector
        /// </summary>
        /// <param name="a"> input vector </param>
        /// <returns> norm of the vector </returns>
        public static double Norm(VecZ3 a)
            => Math.Sqrt(NormSquare(a));

        #endregion
        #region operators

        #region ===== plus =====

        /// <summary>
        /// sum of two vectors
        /// </summary>
        /// <param name="a"> input vector a </param>
        /// <param name="b"> input vector b </param>
        /// <returns> sum result </returns>
        public static VecZ3 operator +(VecZ3 a, VecZ3 b)
            => new()
            {
                X = a.X + b.X,
                Y = a.Y + b.Y,
                Z = a.Z + b.Z
            };

        /// <summary>
        /// sum of two vectors
        /// </summary>
        /// <param name="a"> input vector a </param>
        /// <param name="b"> input vector b </param>
        /// <returns> sum result </returns>
        public static VecZ3 operator +(VecZ3 a, VecD3 b)
            => new()
            {
                X = a.X + b.X,
                Y = a.Y + b.Y,
                Z = a.Z + b.Z
            };

        /// <summary>
        /// sum of two vectors
        /// </summary>
        /// <param name="a"> input vector a </param>
        /// <param name="b"> input vector b </param>
        /// <returns> sum result </returns>
        public static VecZ3 operator +(VecD3 a, VecZ3 b)
            => b + a;

        /// <summary>
        /// plus a scalar to each element of a vector
        /// </summary>
        /// <param name="a"> input vector a </param>
        /// <param name="s"> input scalar s </param>
        /// <returns> sum result </returns>
        public static VecZ3 operator +(VecZ3 a, Complex s)
            => new()
            {
                X = a.X + s,
                Y = a.Y + s,
                Z = a.Z + s
            };

        /// <summary>
        /// plus a scalar to each element of a vector
        /// </summary>
        /// <param name="s"> scalar </param>
        /// <param name="a"> vector </param>
        /// <returns> sum result </returns>
        public static VecZ3 operator +(Complex s, VecZ3 a)
            => a + s;

        /// <summary>
        /// plus a scalar to each element of a vector
        /// </summary>
        /// <param name="a"> input vector a </param>
        /// <param name="s"> input scalar s </param>
        /// <returns> sum result </returns>
        public static VecZ3 operator +(VecZ3 a, double s)
            => a + new Complex(s, 0.0);

        /// <summary>
        /// plus a scalar to each element of a vector
        /// </summary>
        /// <param name="s"> scalar </param>
        /// <param name="a"> vector </param>
        /// <returns> sum result </returns>
        public static VecZ3 operator +(double s, VecZ3 a)
            => new Complex(s, 0.0) + a;

        #endregion
        #region ===== minus =====

        /// <summary>
        /// subtracts a vector from another
        /// </summary>
        /// <param name="a"> input vector a </param>
        /// <param name="b"> input vector b </param>
        /// <returns> subtraction result </returns>
        public static VecZ3 operator -(VecZ3 a, VecZ3 b)
            => new()
            {
                X = a.X - b.X,
                Y = a.Y - b.Y,
                Z = a.Z - b.Z
            };

        /// <summary>
        /// subtracts a vector from another
        /// </summary>
        /// <param name="a"> input vector a </param>
        /// <param name="b"> input vector b </param>
        /// <returns> subtraction result </returns>
        public static VecZ3 operator -(VecZ3 a, VecD3 b)
            => new()
            {
                X = a.X - b.X,
                Y = a.Y - b.Y,
                Z = a.Z - b.Z
            };

        /// <summary>
        /// subtracts a vector from another
        /// </summary>
        /// <param name="a"> input vector a </param>
        /// <param name="b"> input vector b </param>
        /// <returns> subtraction result </returns>
        public static VecZ3 operator -(VecD3 a, VecZ3 b)
            => new VecZ3(a.X, a.Y, a.Z) - b;

        /// <summary>
        /// subtracts a scalar from each element of a vector
        /// </summary>
        /// <param name="a"> input vector a </param>
        /// <param name="s"> input scalar s </param>
        /// <returns> result vector </returns>
        public static VecZ3 operator -(VecZ3 a, Complex s)
            => new()
            {
                X = a.X - s,
                Y = a.Y - s,
                Z = a.Z - s
            };

        /// <summary>
        /// subtracts a scalar by each element of a vector
        /// </summary>
        /// <param name="s"> input scalar s </param>
        /// <param name="a"> input vector a </param>
        /// <returns> result vector </returns>
        public static VecZ3 operator -(Complex s, VecZ3 a)
            => new()
            {
                X = s - a.X,
                Y = s - a.Y,
                Z = s - a.Z
            };

        /// <summary>
        /// subtracts a scalar from each element of a vector
        /// </summary>
        /// <param name="a"> input vector a </param>
        /// <param name="s"> input scalar s </param>
        /// <returns> result vector </returns>
        public static VecZ3 operator -(VecZ3 a, double s)
            => a - new Complex(s, 0.0);

        /// <summary>
        /// subtracts a scalar by each element of a vector
        /// </summary>
        /// <param name="s"> input scalar s </param>
        /// <param name="a"> input vector a </param>
        /// <returns> result vector </returns>
        public static VecZ3 operator -(double s, VecZ3 a)
            => new Complex(s, 0.0) - a;

        #endregion
        #region ===== multiply =====

        /// <summary>
        /// component-wise multiplication of two vectors
        /// </summary>
        /// <param name="a"> input vector a </param>
        /// <param name="b"> input vector b </param>
        /// <returns> result vector </returns>
        public static VecZ3 operator *(VecZ3 a, VecZ3 b)
            => new()
            {
                X = a.X * b.X,
                Y = a.Y * b.Y,
                Z = a.Z * b.Z
            };

        /// <summary>
        /// component-wise multiplication of two vectors
        /// </summary>
        /// <param name="a"> input vector a </param>
        /// <param name="b"> input vector b </param>
        /// <returns> result vector </returns>
        public static VecZ3 operator *(VecZ3 a, VecD3 b)
            => new()
            {
                X = a.X * b.X,
                Y = a.Y * b.Y,
                Z = a.Z * b.Z
            };

        /// <summary>
        /// component-wise multiplication of two vectors
        /// </summary>
        /// <param name="a"> input vector a </param>
        /// <param name="b"> input vector b </param>
        /// <returns> result vector </returns>
        public static VecZ3 operator *(VecD3 a, VecZ3 b)
            => b * a;

        /// <summary>
        /// multiply each element of a vector by a scalar
        /// </summary>
        /// <param name="a"> input vector a </param>
        /// <param name="s"> input scalar s </param>
        /// <returns> result vector </returns>
        public static VecZ3 operator *(VecZ3 a, Complex s)
            => new()
            {
                X = a.X * s,
                Y = a.Y * s,
                Z = a.Z * s
            };

        /// <summary>
        /// multiply each element of a vector by a scalar
        /// </summary>
        /// <param name="s"> input scalar s </param>
        /// <param name="a"> input vector a </param>
        /// <returns> result vector </returns>
        public static VecZ3 operator *(Complex s, VecZ3 a)
            => a * s;

        /// <summary>
        /// multiply each element of a vector by a scalar
        /// </summary>
        /// <param name="a"> input vector a </param>
        /// <param name="s"> input scalar s </param>
        /// <returns> result vector </returns>
        public static VecZ3 operator *(VecZ3 a, double s)
            => new()
            {
                X = a.X * s,
                Y = a.Y * s,
                Z = a.Z * s
            };

        /// <summary>
        /// multiply each element of a vector by a scalar
        /// </summary>
        /// <param name="s"> input scalar s </param>
        /// <param name="a"> input vector a </param>
        /// <returns> result vector </returns>
        public static VecZ3 operator *(double s, VecZ3 a)
            => a * s;

        #endregion
        #region ===== divide =====

        /// <summary>
        /// component-wise division of two vectors 
        /// </summary>
        /// <param name="a"> input vector a </param>
        /// <param name="b"> input vector b </param>
        /// <returns> result vector </returns>
        public static VecZ3 operator /(VecZ3 a, VecZ3 b)
            => new()
            {
                X = a.X / b.X,
                Y = a.Y / b.Y,
                Z = a.Z / b.Z
            };

        /// <summary>
        /// component-wise division of two vectors 
        /// </summary>
        /// <param name="a"> input vector a </param>
        /// <param name="b"> input vector b </param>
        /// <returns> result vector </returns>
        public static VecZ3 operator /(VecZ3 a, VecD3 b)
            => new()
            {
                X = a.X / b.X,
                Y = a.Y / b.Y,
                Z = a.Z / b.Z
            };

        /// <summary>
        /// component-wise division of two vectors 
        /// </summary>
        /// <param name="a"> input vector a </param>
        /// <param name="b"> input vector b </param>
        /// <returns> result vector </returns>
        public static VecZ3 operator /(VecD3 a, VecZ3 b)
            => new VecZ3(a.X, a.Y, a.Z) / b;

        /// <summary>
        /// divides each element of a vector by a scalar
        /// </summary>
        /// <param name="a"> input vector a </param>
        /// <param name="s"> input scalar s </param>
        /// <returns> result vector </returns>
        public static VecZ3 operator /(VecZ3 a, Complex s)
            => new()
            {
                X = a.X / s,
                Y = a.Y / s,
                Z = a.Z / s
            };

        /// <summary>
        /// divides a scalar by each element of a vector
        /// </summary>
        /// <param name="s"> input scalar s </param>
        /// <param name="a"> input vector a </param>
        /// <returns> result vector </returns>
        public static VecZ3 operator /(Complex s, VecZ3 a)
            => new()
            {
                X = s / a.X,
                Y = s / a.Y,
                Z = s / a.Z
            };

        /// <summary>
        /// divides each element of a vector by a scalar
        /// </summary>
        /// <param name="a"> input vector a </param>
        /// <param name="s"> input scalar s </param>
        /// <returns> result vector </returns>
        public static VecZ3 operator /(VecZ3 a, double s)
            => new()
            {
                X = a.X / s,
                Y = a.Y / s,
                Z = a.Z / s
            };

        /// <summary>
        /// divides a scalar by each element of a vector
        /// </summary>
        /// <param name="s"> input scalar s </param>
        /// <param name="a"> input vector a </param>
        /// <returns> result vector </returns>
        public static VecZ3 operator /(double s, VecZ3 a)
            => new Complex(s, 0.0) / a;

        #endregion
        #region ===== negative =====

        /// <summary>
        /// takes the negative of the input vector
        /// </summary>
        /// <param name="v"> input vector </param>
        /// <returns> result negative vector </returns>
        public static VecZ3 operator -(VecZ3 v)
            => new()
            {
                X = -v.X,
                Y = -v.Y,
                Z = -v.Z
            };

        #endregion

        #endregion
    }



    internal class Test 
    {

        static T Add<T>(T left, T right)
            where T : INumber<T>
        {
            return left + right;
        }


        static T Do<T>(T a, T b)
            where T : INumber<T>
        {
            return a + b;
        }
    }


}
