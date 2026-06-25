using System.Numerics;
using System.Runtime.InteropServices;

namespace VEMS.MathCore
{
    /// <summary>
    /// Represents a dense array of elements of type <typeparamref name="T"/>,
    /// providing memory management and basic array operations.
    /// </summary>
    /// <typeparam name="T">
    /// The element type of the array. Must implement <see cref="INumber{T}"/>.
    /// </typeparam>
    public class DenseArray<T> : IDisposable
        where T : INumber<T>
    {
        #region properties

        /// <summary>
        /// Gets or sets the pointer to the underlying data buffer.
        /// </summary>
        public IntPtr DataPtr { get; set; }

        /// <summary>
        /// Gets the type-specific pointer to the underlying data buffer for elements of type <typeparamref name="T"/>.
        /// </summary>
        /// <remarks>
        /// This property provides access to an unsafe pointer of type <typeparamref name="T"/>. 
        /// Use caution when working with unsafe code, as improper usage can lead to memory corruption or undefined behavior.
        /// </remarks>
        internal unsafe T* TPtr => (T*)DataPtr.ToPointer();

        /// <summary>
        /// Gets the void pointer to the underlying data buffer.
        /// </summary>
        internal unsafe void* VPtr => (void*)DataPtr.ToPointer();

        /// <summary>
        /// Gets a pointer to the underlying data as a double-precision floating-point value.
        /// </summary>
        /// <remarks>This property provides direct access to the underlying data in memory. Ensure that
        /// the pointer is used in a safe context to avoid undefined behavior.</remarks>
        internal unsafe double* DPtr => (double*)DataPtr.ToPointer();

        /// <summary>
        /// Gets or sets the number of elements in the array.
        /// </summary>
        public long Count { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use Intel MKL's memory allocation functions.
        /// </summary>
        public bool UseMKLalloc { get; set; } = true;

        /// <summary>
        /// Gets or sets the size in bytes of a single element.
        /// </summary>
        public int ElementByteSize { get; set; }

        /// <summary>
        /// Gets or sets the total size in bytes of the array.
        /// </summary>
        private IntPtr TotalByteSize { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the array has been disposed.
        /// </summary>
        private bool Disposed { get; set; } = false;

        #endregion
        #region constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="DenseArray{T}"/> class with default values.
        /// </summary>
        internal DenseArray() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="DenseArray{T}"/> class with the specified number of elements and allocation mode.
        /// </summary>
        /// <param name="count">The number of elements in the array.</param>
        /// <param name="initMode">The array initialization mode. Default is <see cref="ArrayInitMode.Malloc"/> (uninitialized memory).</param>
        public DenseArray(long count,
            ArrayInitMode initMode = ArrayInitMode.Malloc)
        {
            Count = count;
            ElementByteSize = Marshal.SizeOf(typeof(T));
            TotalByteSize = new IntPtr(Count * ElementByteSize);

            // cases
            switch (initMode)
            {
                case ArrayInitMode.None:
                    DataPtr = IntPtr.Zero;
                    UseMKLalloc = false;
                    break;
                case ArrayInitMode.Calloc:
                    DataPtr = IntelMKL.Calloc(Count, ElementByteSize);
                    break;
                case ArrayInitMode.Malloc:
                    DataPtr = IntelMKL.Malloc(TotalByteSize);
                    break;
                default: goto case ArrayInitMode.Malloc;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DenseArray{T}"/> class with the specified number of elements,
        /// setting all elements to the given initial value.
        /// </summary>
        /// <param name="count">The number of elements in the array.</param>
        /// <param name="initVal">The initial value for all elements.</param>
        /// <param name="loopMode">The loop-computation mode option.</param>
        public unsafe DenseArray(long count, T initVal,
            LoopMode loopMode = Defaults.LoopOption)
            : this(count, initVal.Equals(0.0) ? ArrayInitMode.Calloc : ArrayInitMode.Malloc)
        {
            if (!initVal.Equals(0.0))
            {
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
                Action<long> a = (i) => { *((T*)VPtr + i) = initVal; };
#pragma warning restore CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
                Loop1D loop = new(operation: a, start: 0, end: Count, step: 1);
                loop.Evaluate(loopMode);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DenseArray{T}"/> class by copying from another <see cref="DenseArray{T}"/>.
        /// </summary>
        /// <param name="other">The source array to copy from.</param>
        /// <param name="copyMode">The copy mode option. <see cref="ArrayCopyMode.Deep"/> for deep copy, <see cref="ArrayCopyMode.Shallow"/> for shallow copy.</param>
        public unsafe DenseArray(DenseArray<T> other,
            ArrayCopyMode copyMode = ArrayCopyMode.Deep)
        {
            Count = other.Count;
            ElementByteSize = Marshal.SizeOf(typeof(T));
            TotalByteSize = other.TotalByteSize;
            Disposed = false;
            UseMKLalloc = true;

            // copy options
            switch (copyMode)
            {
                case ArrayCopyMode.Deep:
                    DataPtr = IntelMKL.Malloc(TotalByteSize);
                    Buffer.MemoryCopy(other.VPtr, VPtr, TotalByteSize.ToInt64(), TotalByteSize.ToInt64());
                    break;
                case ArrayCopyMode.Shallow:
                    DataPtr = new IntPtr(other.DataPtr.ToInt64());
                    break;
                default: goto case ArrayCopyMode.Shallow;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DenseArray{T}"/> class by copying data from the specified array.
        /// </summary>
        /// <remarks>
        /// This constructor allocates memory for the array using Intel MKL's memory allocation
        /// functions when performing a deep copy. The <paramref name="other"/> array must not be null, and its elements
        /// must be compatible with the type <typeparamref name="T"/>.
        /// </remarks>
        /// <param name="other">The source array from which data will be copied.</param>
        /// <param name="copyMode">
        /// Specifies the copy mode for the operation. Use <see cref="ArrayCopyMode.Deep"/> to perform a deep copy.
        /// <see cref="ArrayCopyMode.Shallow"/> is not supported and will throw an exception.
        /// </param>
        /// <exception cref="NotSupportedException">
        /// Thrown if <paramref name="copyMode"/> is set to <see cref="ArrayCopyMode.Shallow"/>, as shallow copying is not supported.
        /// </exception>
        public unsafe DenseArray(T[] other,
            ArrayCopyMode copyMode = ArrayCopyMode.Deep)
        {
            Count = other.Length;
            ElementByteSize = Marshal.SizeOf(typeof(T));
            TotalByteSize = new IntPtr(Count * ElementByteSize);
            Disposed = false;
            UseMKLalloc = true;

            // copy options
            switch (copyMode)
            {
                case ArrayCopyMode.Deep:
                    DataPtr = IntelMKL.Malloc(TotalByteSize);
                    fixed (T* pData = other)
                    { Buffer.MemoryCopy(pData, VPtr, TotalByteSize.ToInt64(), TotalByteSize.ToInt64()); }
                    break;
                case ArrayCopyMode.Shallow:
                    throw new NotSupportedException("Shallow copy from array is not supported.");
                default: goto case ArrayCopyMode.Shallow;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DenseArray{T}"/> class by copying the contents of a
        /// two-dimensional array into a dense array structure.
        /// </summary>
        /// <remarks>This constructor allocates memory for the dense array using Intel MKL's memory
        /// allocation mechanism when <see cref="ArrayCopyMode.Deep"/> is specified. The array data is copied deeply
        /// into the allocated memory. The <paramref name="other"/> array must be fixed in memory during the copy
        /// operation.</remarks>
        /// <param name="other">The two-dimensional array to copy. This array must not be null.</param>
        /// <param name="copyMode">Specifies the copy mode for the operation. Use <see cref="ArrayCopyMode.Deep"/> to perform a deep copy of
        /// the array. <see cref="ArrayCopyMode.Shallow"/> is not supported and will throw an exception.</param>
        /// <exception cref="NotSupportedException">Thrown if <paramref name="copyMode"/> is set to <see cref="ArrayCopyMode.Shallow"/>.</exception>
        public unsafe DenseArray(T[,] other,
            ArrayCopyMode copyMode = ArrayCopyMode.Deep)
        {
            Count = other.LongLength;
            ElementByteSize = Marshal.SizeOf(typeof(T));
            TotalByteSize = new IntPtr(Count * ElementByteSize);
            Disposed = false;
            UseMKLalloc = true;

            // copy options
            switch (copyMode)
            {
                case ArrayCopyMode.Deep:
                    DataPtr = IntelMKL.Malloc(TotalByteSize);
                    fixed (T* pData = other)
                    { Buffer.MemoryCopy(pData, VPtr, TotalByteSize.ToInt64(), TotalByteSize.ToInt64()); }
                    break;
                case ArrayCopyMode.Shallow:
                    throw new NotSupportedException("Shallow copy from array is not supported.");
                default: goto case ArrayCopyMode.Shallow;
            }
        }

        #endregion
        #region methods

        /// <summary>
        /// Checks if a given index is valid for this array.
        /// The lower bound is zero and the upper bound is <see cref="Count"/>.
        /// </summary>
        /// <param name="i">The input index.</param>
        /// <returns>True if the index is valid; otherwise, false.</returns>
        public bool IsIndexValid(long i)
            => IsIndexValid(i, Count, GetType().Name);

        /// <summary>
        /// Checks if a given range is valid for this array.
        /// The lower bound is zero and the upper bound is <see cref="Count"/>.
        /// </summary>
        /// <param name="rng">The input range.</param>
        /// <returns>True if the range is valid; otherwise, false.</returns>
        public bool IsRangeValid(LongRange rng)
            => IsRangeValid(rng, Count, GetType().Name);

        /// <summary>
        /// Checks if a given index is valid for a specified bound.
        /// </summary>
        /// <param name="i">The input index.</param>
        /// <param name="bound">The upper bound for the index. The lower bound is zero by default.</param>
        /// <param name="prompt">An optional message for warnings.</param>
        /// <returns>True if the index is valid; otherwise, false.</returns>
        internal static bool IsIndexValid(long i, long bound,
            string prompt = "")
        {
            bool isIndexValid = true;
            if (i < 0)
            {
                isIndexValid = false;
                Printer.Warning($"{prompt}: index must be non-negative.");
                return isIndexValid;
            }
            if (i >= bound)
            {
                isIndexValid = false;
                Printer.Warning($"{prompt}: index outside bound.");
                return isIndexValid;
            }
            return isIndexValid;
        }

        /// <summary>
        /// Checks if a given range is valid for a specified bound.
        /// </summary>
        /// <param name="rng">The input range.</param>
        /// <param name="bound">The upper bound for the end of the range. The lower bound for the start is zero by default.</param>
        /// <param name="prompt">An optional message for warnings.</param>
        /// <returns>True if the range is valid; otherwise, false.</returns>
        internal static bool IsRangeValid(LongRange rng, long bound,
            string prompt = "")
        {
            bool isRangeValid = true;
            if (rng.Start < 0)
            {
                isRangeValid = false;
                Printer.Write($"{prompt}: start of range must be non-negative.");
                return isRangeValid;
            }
            if (rng.End > bound)
            {
                isRangeValid = false;
                Printer.Write($"{prompt}: end of range outside bound.");
                return isRangeValid;
            }
            return isRangeValid;
        }

        #endregion


        //public static explicit operator DenseArray<T>(Vect<T> x)
        //{
        //    return x as DenseArray<T>;
        //}


        #region dispose

        /// <summary>
        /// Releases all resources used by the <see cref="DenseArray{T}"/> instance.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            IntelMKL.Free(DataPtr);
            //Marshal.FreeHGlobal(_dataPtr);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="DenseArray{T}"/> and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">
        /// True if called from <see cref="Dispose()"/>; false if called from the finalizer.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (Disposed == false)
            {
                if (disposing) { }
                if (UseMKLalloc) { IntelMKL.Free(DataPtr); }
                else { Marshal.FreeHGlobal(DataPtr); }
                //IntelMKLNative.MKL_free(_dataPtr);
                Disposed = true;
            }
        }

        /// <summary>
        /// Finalizer for the <see cref="DenseArray{T}"/> class.
        /// Invoked by the garbage collector if <see cref="Dispose()"/> was not called.
        /// </summary>
        ~DenseArray()
        {
            Dispose(false);
        }

        #endregion
    }


    
    namespace XTMethods
    {

        /// <summary>
        /// Provides extension methods for the <see cref="Index"/> type.
        /// </summary>
        public static class IndexExtensions
        {
            /// <summary>
            /// Converts an <see cref="Index"/> to a 64-bit integer index, given the total count.
            /// </summary>
            /// <param name="i">The <see cref="Index"/> to convert.</param>
            /// <param name="count">The total number of elements in the collection.</param>
            /// <returns>
            /// The zero-based index as a <see cref="long"/>. If <paramref name="i"/> is from the end, returns <c>count - i.Value</c>;
            /// otherwise, returns <c>i.Value</c>.
            /// </returns>
            public static long ToLong(this Index i, long count)
            {
                if (i.IsFromEnd) { return (count - i.Value); }
                else { return i.Value; }
            }
        }
    }

    

}
