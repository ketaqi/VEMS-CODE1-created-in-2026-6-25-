using System.Numerics;
using VEMS.AMathCore.XTMethods;

namespace VEMS.AMathCore
{

    // naming ...
    // VecX; Vect
    // MatX; Matx
    // SPVecX; SPVect
    // SPMatX; SPMatx

    /// <summary>
    /// Vec[T] class
    /// </summary>
    /// <typeparam name="T"> INumber[T] </typeparam>
    public class Vect<T> : DenseArray<T>, IVect<T> 
        where T : INumber<T>
    {
        #region properties

        #region ---- element ----

        /// <summary>
        /// gets / sets the value of a vector element
        /// </summary>
        /// <param name="i"> index of the element [Int64] </param>
        /// <param name="checkBound"> whether to check if the index is outside bound </param>
        /// <returns> element value </returns>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public unsafe new T this[long i, bool checkBound = true] 
        { 
            get 
            {
                if (checkBound && !IsIndexValid(i)) { throw new IndexOutOfRangeException(nameof(i)); }
                return *((T*)VPtr + i);
            }
            set 
            {
                if (checkBound && !IsIndexValid(i)) { throw new IndexOutOfRangeException(nameof(i)); }
                *((T*)VPtr + i) = value;
            }
        }

        /// <summary>
        /// gets / sets the value of a vector element
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
        /// gets / sets the value of a vector element
        /// </summary>
        /// <param name="i"> index of the element [Int32] </param>
        /// <param name="checkBound"> whether to check if the index is outside bound </param>
        /// <returns> element value </returns>
        public T this[Index i, bool checkBound = true] 
        { 
            get => this[i.ToLong(Count), checkBound]; 
            set => this[i.ToLong(Count), checkBound] = value; 
        }

        #endregion
        #region ---- slice ----

        /// <summary>
        /// gets / sets the vector elements within a given range
        /// </summary>
        /// <param name="rng"> range with inclusive start and exclusive end indices [Int64] </param>
        /// <returns> vector elements within the range </returns>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public unsafe Vect<T> this[LongRange rng]
        {
            get
            {
                if (!IsRangeValid(rng)) { throw new IndexOutOfRangeException(nameof(rng)); }
                // finds target slice
                long n = rng.End - rng.Start;
                if (n <= 0) { throw new IndexOutOfRangeException(); }
                Vect<T> x = new(count: n, initMode: ArrayInitMode.Malloc);
                // memory copy
                Buffer.MemoryCopy(
                    source: (byte*)VPtr + rng.Start * ElementByteSize,
                    destination: x.VPtr,
                    destinationSizeInBytes: n * ElementByteSize,
                    sourceBytesToCopy: n * ElementByteSize);
                return x;
            }
            set
            {
                if (!IsRangeValid(rng)) { throw new IndexOutOfRangeException(nameof(rng)); }
                // finds target slice
                long n = rng.End - rng.Start;
                if (n <= 0 || value == null) { return; }
                // memory copy
                Buffer.MemoryCopy(
                    source: value.VPtr,
                    destination: (byte*)VPtr + rng.Start * ElementByteSize,
                    destinationSizeInBytes: n * ElementByteSize,
                    sourceBytesToCopy: n * ElementByteSize);
            }
        }

        /// <summary>
        /// gets / sets the vector elements within a given range
        /// </summary>
        /// <param name="rng"> range with inclusive start and exclusive end indices [Int32] </param>
        /// <returns> vector elements within the range </returns>
        public Vect<T> this[Range rng]
        {
            get => this[new LongRange(rng, Count)];
            set => this[new LongRange(rng, Count)] = value;
        }

        #endregion

        #endregion
        #region constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        internal Vect() : base() { }

        /// <summary>
        /// Constructs a vector with given length.
        /// By default, initializes element values to zeros
        /// </summary>
        /// <param name="count">The total number of vector elements.</param>
        /// <param name="initMode">Initialization mode option; default is CALLOC.</param>
        public Vect(long count, 
            ArrayInitMode initMode = ArrayInitMode.Calloc) 
            : base(count, initMode) { }

        /// <summary>
        /// Constructs a vector by copying from another vector.
        /// </summary>
        /// <param name="source">Another vector as the source.</param>
        /// <param name="copyMode">Copy mode option: deep or shallow.</param>
        public Vect(Vect<T> source, 
            ArrayCopyMode copyMode = ArrayCopyMode.Deep) 
            : base(source, copyMode) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Vect{T}"/> class by copying from the specified source array.
        /// </summary>
        /// <param name="source">The source array from which to initialize the vector. This array provides the initial data for the vector.</param>
        /// <param name="copyMode">Specifies the copy mode to use when initializing the vector. Use <see cref="ArrayCopyMode.Deep"/> to create
        /// a deep copy of the source array, or <see cref="ArrayCopyMode.Shallow"/> to reference the source array
        /// directly. The default is <see cref="ArrayCopyMode.Deep"/>.</param>
        public Vect(T[] source,
            ArrayCopyMode copyMode = ArrayCopyMode.Deep)
            : base(source, copyMode) { }

        #endregion
        #region methods

        #region ---- creation ----

        /// <summary>
        /// Creates a new vector of the specified size, with all elements initialized to the specified value.
        /// </summary>
        /// <remarks>If <paramref name="x0"/> is equal to <see langword="default"/> for the type
        /// <typeparamref name="T"/>,  the vector is initialized using zeroed memory for performance optimization.
        /// Otherwise, memory is allocated without zeroing, and each element is explicitly set to <paramref
        /// name="x0"/>.</remarks>
        /// <param name="n">The number of elements in the vector. Must be a non-negative value.</param>
        /// <param name="x0">The value to initialize each element of the vector with.</param>
        /// <returns>A new <see cref="Vect{T}"/> instance containing <paramref name="n"/> elements, each set to <paramref
        /// name="x0"/>.</returns>
        public static unsafe Vect<T> Create(long n, T x0)
        {
            Vect<T> x = new (count: n, initMode: x0.Equals(T.Zero) ?
                ArrayInitMode.Calloc : ArrayInitMode.Malloc);
            if (!x0.Equals(T.Zero)) 
            {
                T* ptr = (T*)x.VPtr;
                for (long i = 0; i < n; i++)
                { ptr[i] = x0; }
            }
            return x;
        }

        /// <summary>
        /// Creates a vector of values starting from an initial value and incrementing by a specified step.
        /// </summary>
        /// <remarks>This method allocates memory for the vector using the <see
        /// cref="ArrayInitMode.Malloc"/> initialization mode. The caller is responsible for ensuring that the type
        /// <typeparamref name="T"/> supports addition.</remarks>
        /// <param name="n">The number of elements in the vector. Must be greater than or equal to zero.</param>
        /// <param name="x0">The initial value of the first element in the vector.</param>
        /// <param name="dx">The increment applied to each subsequent element in the vector.</param>
        /// <returns>A <see cref="Vect{T}"/> containing <paramref name="n"/> elements, where the first element is  <paramref
        /// name="x0"/> and each subsequent element is incremented by <paramref name="dx"/>.</returns>
        public static unsafe Vect<T> Create(long n, T x0, T dx)
        {
            Vect<T> x = new (count: n, initMode: ArrayInitMode.Malloc);
            T* ptr = (T*)x.VPtr;
            for (long i = 0; i < n; i++)
            { ptr[i] = x0 + dx * T.CreateChecked(i); }

            //if (typeof(T) == typeof(Real))
            //{

            //}
            //else if (typeof(T) == typeof(Cplx))
            //{

            //}

            return x;
        }

        #endregion
        #region ---- re-size ----

        /// <summary>
        /// Resizes the vector to the specified target count, optionally applying an offset and boundary padding.
        /// </summary>
        /// <param name="targetCount">The desired number of elements in the resized vector. Must be non-negative.</param>
        /// <param name="offset">The starting index in the target vector where the current vector's elements will be copied. Must be within the bounds of the target vector.</param>
        /// <param name="boundary">Specifies the boundary condition to use for padding. <see cref="DataBoundary.ConstantZero"/> pads with zeros; <see cref="DataBoundary.Periodic"/> wraps elements periodically.</param>
        /// <param name="padValue">The value to use for padding if <paramref name="boundary"/> is not <see cref="DataBoundary.ConstantZero"/>. If <paramref name="boundary"/> is <see cref="DataBoundary.ConstantZero"/>, this value is ignored and zero is used.</param>
        /// <returns>
        /// A new <see cref="Vect{T}"/> instance with <paramref name="targetCount"/> elements, containing the original vector's data
        /// starting at <paramref name="offset"/>, and padded according to the specified boundary condition.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="targetCount"/> is negative, or if <paramref name="offset"/> is out of bounds.
        /// </exception>
        public unsafe Vect<T> Resize(long targetCount, 
            long offset = 0, DataBoundary boundary = DataBoundary.ConstantZero, 
            T padValue = default!)
        {
            if (targetCount < 0)
            { throw new ArgumentOutOfRangeException(nameof(targetCount), "Target count must be non-negative."); }
            if (offset < 0 || offset >= targetCount)
            { throw new ArgumentOutOfRangeException(nameof(offset), "Offset must be within the bounds of the target vector."); }

            // initializes the new vector with padding value
            if (boundary == DataBoundary.ConstantZero) { padValue = T.Zero; }
            Vect<T> x = Vect<T>.Create(targetCount, padValue);

            // calculates the number of elements to copy
            long copyCount = Math.Min(Count, targetCount - offset);
            if (copyCount <= 0) { return x; }

            // only performs memory copy if there is something to copy
            Buffer.MemoryCopy(
                source: VPtr,
                destination: (byte*)x.VPtr + offset * ElementByteSize,
                destinationSizeInBytes: (targetCount - offset) * ElementByteSize,
                sourceBytesToCopy: copyCount * ElementByteSize);

            // circular/periodic boundary handling ...
            if (boundary == DataBoundary.Periodic)
            {
                if (offset > 0 && offset + Count > targetCount)
                {
                    // wrap around copy
                    long wrapCount = offset + Count - targetCount;
                    long actualWrap = Math.Min(wrapCount, offset);
                    Buffer.MemoryCopy(
                        source: (byte*)VPtr + (Count - actualWrap) * ElementByteSize,
                        destination: x.VPtr,
                        destinationSizeInBytes: actualWrap * ElementByteSize,
                        sourceBytesToCopy: actualWrap * ElementByteSize);
                }
            }

            // returns the resized vector
            return x;
        }

        #endregion
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
        #region operators
        /*
        // Vector-Vector [Generic]
        #region ---- [+] ----
        /// <summary>
        /// Adds two vectors element-wise.
        /// </summary>
        /// <param name="x">The first vector operand.</param>
        /// <param name="y">The second vector operand.</param>
        /// <returns>
        /// A new <see cref="Vect{T}"/> containing the element-wise sum of <paramref name="x"/> and <paramref name="y"/>.
        /// </returns>
        public static Vect<T> operator +(Vect<T> x, Vect<T> y)
            => VMathOperator.Add(x, y);
        #endregion
        #region ---- [-] ----
        /// <summary>
        /// Subtracts the second vector from the first element-wise.
        /// </summary>
        /// <param name="x">The first vector operand (minuend).</param>
        /// <param name="y">The second vector operand (subtrahend).</param>
        /// <returns>
        /// A new <see cref="Vect{T}"/> containing the element-wise difference of <paramref name="x"/> and <paramref name="y"/>.
        /// </returns>
        public static Vect<T> operator -(Vect<T> x, Vect<T> y)
            => VMathOperator.Sub(x, y);
        #endregion
        #region ---- [*] ----
        /// <summary>
        /// Multiplies two vectors element-wise.
        /// </summary>
        /// <param name="x">The first vector operand.</param>
        /// <param name="y">The second vector operand.</param>
        /// <returns>
        /// A new <see cref="Vect{T}"/> containing the element-wise product of <paramref name="x"/> and <paramref name="y"/>.
        /// </returns>
        public static Vect<T> operator *(Vect<T> x, Vect<T> y)
            => VMathOperator.Mul(x, y);
        #endregion
        #region ---- [/] ----
        /// <summary>
        /// Divides the first vector by the second element-wise.
        /// </summary>
        /// <param name="x">The numerator vector.</param>
        /// <param name="y">The denominator vector.</param>
        /// <returns>
        /// A new <see cref="Vect{T}"/> containing the element-wise quotient of <paramref name="x"/> divided by <paramref name="y"/>.
        /// </returns>
        public static Vect<T> operator /(Vect<T> x, Vect<T> y)
            => VMathOperator.Div(x, y);
        #endregion

        // Vector-Scalar [Generic]
        #region ---- [+] ----
        /// <summary>
        /// Adds a scalar value to each element of the vector.
        /// </summary>
        /// <param name="x">The vector operand.</param>
        /// <param name="s">The scalar value to add.</param>
        /// <returns>
        /// A new <see cref="Vect{T}"/> with <paramref name="s"/> added to each element of <paramref name="x"/>.
        /// </returns>
        public static Vect<T> operator +(Vect<T> x, T s)
            => VMathOperator.Add(x, s);

        /// <summary>
        /// Adds a scalar value to each element of the vector.
        /// </summary>
        /// <param name="s">The scalar value to add.</param>
        /// <param name="x">The vector operand.</param>
        /// <returns>
        /// A new <see cref="Vect{T}"/> with <paramref name="s"/> added to each element of <paramref name="x"/>.
        /// </returns>
        public static Vect<T> operator +(T s, Vect<T> x)
            => VMathOperator.Add(x, s);
        #endregion
        #region ---- [-] ----
        /// <summary>
        /// Subtracts a scalar value from each element of the vector.
        /// </summary>
        /// <param name="x">The vector operand.</param>
        /// <param name="s">The scalar value to subtract.</param>
        /// <returns>
        /// A new <see cref="Vect{T}"/> with <paramref name="s"/> subtracted from each element of <paramref name="x"/>.
        /// </returns>
        public static Vect<T> operator -(Vect<T> x, T s)
            => VMathOperator.Sub(x, s);

        /// <summary>
        /// Subtracts each element of the vector from a scalar value.
        /// </summary>
        /// <param name="s">The scalar value to subtract from.</param>
        /// <param name="x">The vector operand.</param>
        /// <returns>
        /// A new <see cref="Vect{T}"/> containing the result of <paramref name="s"/> minus each element of <paramref name="x"/>.
        /// </returns>
        public static Vect<T> operator -(T s, Vect<T> x)
            => VMathOperator.Sub(s, x);
        #endregion
        #region ---- [*] ----
        /// <summary>
        /// Multiplies each element of the vector by a scalar value.
        /// </summary>
        /// <param name="x">The vector operand.</param>
        /// <param name="s">The scalar value to multiply.</param>
        /// <returns>
        /// A new <see cref="Vect{T}"/> containing the result of multiplying each element of <paramref name="x"/> by <paramref name="s"/>.
        /// </returns>
        public static Vect<T> operator *(Vect<T> x, T s)
            => VMathOperator.Mul(s, x);

        /// <summary>
        /// Multiplies each element of the vector by a scalar value.
        /// </summary>
        /// <param name="s">The scalar value to multiply.</param>
        /// <param name="x">The vector operand.</param>
        /// <returns>
        /// A new <see cref="Vect{T}"/> containing the result of multiplying each element of <paramref name="x"/> by <paramref name="s"/>.
        /// </returns>
        public static Vect<T> operator *(T s, Vect<T> x)
            => VMathOperator.Mul(s, x);
        #endregion
        #region ---- [/] ----
        /// <summary>
        /// Divides each element of the vector by a scalar value.
        /// </summary>
        /// <param name="x">The vector operand (numerator).</param>
        /// <param name="s">The scalar value to divide by (denominator).</param>
        /// <returns>
        /// A new <see cref="Vect{T}"/> containing the result of dividing each element of <paramref name="x"/> by <paramref name="s"/>.
        /// </returns>
        public static Vect<T> operator /(Vect<T> x, T s)
            => VMathOperator.Div(x, s);

        /// <summary>
        /// Divides a scalar value by each element of the vector.
        /// </summary>
        /// <param name="s">The scalar value to divide (numerator).</param>
        /// <param name="x">The vector operand (denominator).</param>
        /// <returns>
        /// A new <see cref="Vect{T}"/> containing the result of dividing <paramref name="s"/> by each element of <paramref name="x"/>.
        /// </returns>
        public static Vect<T> operator /(T s, Vect<T> x)
            => VMathOperator.Div(s, x);
        #endregion

        // Misc
        #region ---- Neg ----
        /// <summary>
        /// Negates each element of the specified <see cref="Vect{T}"/>.
        /// </summary>
        /// <param name="x">The vector to negate.</param>
        /// <returns>
        /// A new <see cref="Vect{T}"/> containing the negated values of <paramref name="x"/>.
        /// </returns>
        public static Vect<T> operator -(Vect<T> x)
            => VMathOperator.Neg(x);
        #endregion
        #region ---- Implicit ----

        /// <summary>
        /// Defines an implicit conversion from a <see cref="Vect{T}"/> to a <see cref="Vect{Cplx}"/>.
        /// The conversion creates a new <see cref="Vect{Cplx}"/> instance with the same number of elements as <paramref name="x"/>,
        /// and copies the real part from <paramref name="x"/> into the real part of each complex element in the result.
        /// The imaginary part of each complex element is set to zero.
        /// </summary>
        /// <param name="x">The source <see cref="Vect{T}"/> to convert.</param>
        /// <returns>
        /// A new <see cref="Vect{Cplx}"/> instance containing the real values from <paramref name="x"/> as the real part of each complex element,
        /// with the imaginary part set to zero.
        /// </returns>
        public static implicit operator Vect<Cplx>(Vect<T> x)
            => VMath.ToCplx(x);

        #endregion
        */
        #endregion
    }

    ///// <summary>
    ///// Represents a vector of 64-bit integers.
    ///// Inherits from <see cref="Vect{Int}"/> and provides constructors for various initialization scenarios,
    ///// as well as a type-specific pointer to the underlying data buffer.
    ///// </summary>
    //public class VectorI : Vect<Int>
    //{
    //    #region properties

    //    /// <summary>
    //    /// Gets the <see cref="Int"/>-type-specific pointer to the underlying data buffer.
    //    /// </summary>
    //    /// <remarks>
    //    /// This property provides access to an unsafe pointer of type <see langword="Int"/>.
    //    /// Use caution when working with unsafe code, as improper usage can lead to memory corruption or undefined behavior.
    //    /// </remarks>
    //    public unsafe new Int* TPtr
    //    {
    //        get => (Int*)VPtr;
    //    }

    //    #endregion
    //    #region constructors

    //    /// <summary>
    //    /// Initializes a new instance of the <see cref="VectorI"/> class with zero elements.
    //    /// </summary>
    //    internal VectorI() : base() { }

    //    /// <summary>
    //    /// Initializes a new instance of the <see cref="VectorI"/> class with the specified number of elements and an optional initialization mode.
    //    /// </summary>
    //    /// <param name="count">The number of elements in the vector. Must be a non-negative value.</param>
    //    /// <param name="initMode">
    //    /// The mode used to initialize the elements of the vector. Defaults to <see cref="ArrayInitMode.Calloc"/>,
    //    /// which initializes all elements to zero.
    //    /// </param>
    //    public VectorI(long count,
    //        ArrayInitMode initMode = ArrayInitMode.Calloc)
    //        : base(count, initMode) { }

    //    /// <summary>
    //    /// Initializes a new instance of the <see cref="VectorI"/> class by copying the contents of the specified source vector.
    //    /// </summary>
    //    /// <param name="source">The source <see cref="Vect{Int}"/> to copy.</param>
    //    /// <param name="copyMode">
    //    /// Specifies the mode of array copying. Use <see cref="ArrayCopyMode.Deep"/> for a deep copy or
    //    /// <see cref="ArrayCopyMode.Shallow"/> for a shallow copy. The default is <see cref="ArrayCopyMode.Deep"/>.
    //    /// </param>
    //    /// <remarks>
    //    /// This constructor allows creating a new vector by duplicating the data from an existing <see cref="Vect{Int}"/> instance.
    //    /// The <paramref name="copyMode"/> parameter determines whether the data is deeply copied (creating a new independent copy)
    //    /// or shallowly copied (referencing the same underlying data).
    //    /// </remarks>
    //    public VectorI(Vect<Int> source,
    //        ArrayCopyMode copyMode = ArrayCopyMode.Deep)
    //        : base(source, copyMode) { }

    //    /// <summary>
    //    /// Initializes a new instance of the <see cref="VectorI"/> class by copying from the specified source array.
    //    /// </summary>
    //    /// <param name="source">The array of 64-bit integers to initialize the vector. Must not be null.</param>
    //    /// <param name="copyMode">
    //    /// Specifies whether the source array should be deeply copied or referenced directly. Defaults to <see cref="ArrayCopyMode.Deep"/>.
    //    /// </param>
    //    /// <remarks>
    //    /// If <paramref name="copyMode"/> is set to <see cref="ArrayCopyMode.Deep"/>, the source array is copied to ensure the vector has its own independent data.
    //    /// If set to <see cref="ArrayCopyMode.Shallow"/>, the vector will reference the original array, and changes to the array will affect the vector.
    //    /// </remarks>
    //    public VectorI(Int[] source,
    //        ArrayCopyMode copyMode = ArrayCopyMode.Deep)
    //        : base(source, copyMode) { }

    //    #endregion
    //    #region methods

    //    // ...

    //    #endregion
    //}

}
