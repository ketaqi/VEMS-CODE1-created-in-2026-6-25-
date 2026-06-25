using System.Runtime.InteropServices;

namespace VEMS.MathCore
{

    #region ----- enums -----

    /// <summary>
    /// array initialization mode options
    /// </summary>
    public enum ArrayInitMode
    {
        /// <summary>
        /// shallow generation without
        /// memory allocation
        /// </summary>
        None, // = 00, // commented out to avoid possible ambiguity ...

        /// <summary>
        /// deep generation with
        /// actual memory allocation 
        /// and all values set to zero
        /// </summary>
        Calloc, // = 01,

        /// <summary>
        /// deep generation with
        /// actual memory allocation 
        /// NO value initialization
        /// </summary>
        Malloc // = 02
    }

    /// <summary>
    /// copy mode
    /// </summary>
    public enum ArrayCopyMode
    {
        /// <summary>
        /// shallow copy
        /// </summary>
        Shallow = 00,

        /// <summary>
        /// deep copy
        /// </summary>
        Deep = 01,
    }

    #endregion

    /// <summary>
    /// VEMS array base class
    /// </summary>
    /// <typeparam name="T"> double or complex </typeparam>
    [Obsolete]
    public class DenseArrayBase<T> : IDisposable where T : struct
    {
        #region properties

        /// <summary>
        /// internal pointer to the data
        /// </summary>
        private IntPtr _dataPtr;

        /// <summary>
        /// internal byte size of single element 
        /// </summary>
        private int _elementByteSize;

        /// <summary>
        /// internal total byte size
        /// </summary>
        private IntPtr _totalByteSize;

        /// <summary>
        /// internal flag whether disposed or not
        /// </summary>
        private bool _disposed;

        /// <summary>
        /// get / set the count of the elements
        /// </summary>
        private long _count;

        /// <summary>
        /// whether to use malloc/calloc from Intel MKL
        /// </summary>
        private bool _useMKLalloc = true;

        /// <summary>
        /// whether to use malloc/calloc from Intel MKL
        /// </summary>
        public bool UseMKLalloc {
            get => _useMKLalloc;
            set => _useMKLalloc = value;
        }

        /// <summary>
        /// get/set the count of elements
        /// </summary>
        public long Count 
        { 
            get => _count;
            set => _count = value;
        }

        /// <summary>
        /// get/set the pointer to the data
        /// </summary>
        public IntPtr DataPtr 
        { 
            get => _dataPtr; 
            set => _dataPtr = value;
        }

        /// <summary>
        /// gets the void pointer to the data
        /// </summary>
        public unsafe void* VPtr => (void*)DataPtr.ToPointer();

        #endregion
        #region constructor

        /// <summary>
        /// empty constructor
        /// </summary>
        public DenseArrayBase() { }

        /// <summary>
        /// constructs an ArrayBase
        /// </summary>
        /// <param name="count"> count of the elements </param>
        /// <param name="mode"> array generation mode; default is MALLOC </param>
        public DenseArrayBase(long count,
            ArrayInitMode mode = ArrayInitMode.Malloc)
        {
            _count = count;
            _elementByteSize = Marshal.SizeOf(typeof(T));
            _totalByteSize = new IntPtr(count * _elementByteSize);
            _disposed = false;
            _useMKLalloc = true;

            // cases
            switch (mode)
            {
                case ArrayInitMode.None:
                    _dataPtr = IntPtr.Zero;
                    _useMKLalloc = false;
                    break;
                case ArrayInitMode.Calloc:
                    _dataPtr = IntelMKL.Calloc(_count, _elementByteSize);
                    break;
                case ArrayInitMode.Malloc:
                    _dataPtr = IntelMKL.Malloc(_totalByteSize);
                    break;
                default: goto case ArrayInitMode.Malloc;
            }
        }

        /// <summary>
        /// constructs an ArrayBase with given length
        /// and sets all elements to the same initial value
        /// </summary>
        /// <param name="count"> count of the element </param>
        /// <param name="initVal"> initial value for all the elements </param>
        /// <param name="loopMode"> loop-computation mode option </param>
        public unsafe DenseArrayBase(long count, T initVal,
            LoopMode loopMode = Defaults.LoopOption)
            : this(count, initVal.Equals(0.0) ? ArrayInitMode.Calloc : ArrayInitMode.Malloc)
        {
            if (!initVal.Equals(0.0))
            {
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
                Action<long> a = (i) => { *((T*)VPtr + i) = initVal; };
#pragma warning restore CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
                Loop1D loop = new(operation: a, start: 0, end: Count);
                loop.Evaluate(loopMode);
            }
        }

        /// <summary>
        /// constructs by copying from another
        /// </summary>
        /// <param name="other"> another ArrayBase </param>
        /// <param name="copyMode"> copy mode option </param>
        public unsafe DenseArrayBase(DenseArrayBase<T> other, 
            ArrayCopyMode copyMode = ArrayCopyMode.Deep)
        {
            _count = other._count;
            _elementByteSize = other._elementByteSize;
            _totalByteSize = other._totalByteSize;
            _disposed = other._disposed;
            _useMKLalloc = other._useMKLalloc;

            // copy options
            switch(copyMode)
            {
                case ArrayCopyMode.Deep:
                    _dataPtr = IntelMKL.Malloc(_totalByteSize);
                    Buffer.MemoryCopy(other.VPtr, VPtr, _totalByteSize.ToInt64(), _totalByteSize.ToInt64());
                    break;
                case ArrayCopyMode.Shallow:
                    _dataPtr = new IntPtr(other._dataPtr.ToInt64());
                    break;
                default: goto case ArrayCopyMode.Shallow;
            }
        }

        #endregion
        #region methods

        /// <summary>
        /// check if a given index is valid
        /// the lower bound is set to zero 
        /// the upper bound is set to Count
        /// </summary>
        /// <param name="i"> input index </param>
        /// <returns> return true if the index is valid
        /// otherwise return false </returns>
        public bool IsIndexValid(long i)
            => IsIndexValid(i, Count, GetType().Name);

        /// <summary>
        /// check if a given range is valid
        /// the lower bound is set to zero 
        /// the upper bound is set to Count
        /// </summary>
        /// <param name="rng"> input range </param>
        /// <returns> return true if the range is valid
        /// otherwise return false </returns>
        public bool IsRangeValid(LongRange rng)
            => IsRangeValid(rng, Count, GetType().Name);

        /// <summary>
        /// check if a given index is valid
        /// </summary>
        /// <param name="i"> input index </param>
        /// <param name="bound"> upper bound for the index 
        /// the lower bound is set to zero by default </param>
        /// <param name="prompt"> message </param>
        /// <returns> return true if the index is valid; otherwise return false </returns>
        internal static bool IsIndexValid(long i, long bound,
            string prompt = "")
        {
            bool isIndexValid = true;
            if(i < 0)
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
        /// check if a given range is valid
        /// </summary>
        /// <param name="rng"> input range </param>
        /// <param name="bound"> upper bound for the end of the range
        /// the lower bound for the start is set to zero by default </param>
        /// <param name="prompt"> message </param>
        /// <returns> return true if the range is valid; otherwise return false </returns>
        internal static bool IsRangeValid(LongRange rng, long bound,
            string prompt = "")
        {
            bool isRangeValid = true;
            if(rng.Start < 0)
            {
                isRangeValid = false;
                Printer.Write($"{prompt}: start of range must be non-negative.");
                return isRangeValid;
            }
            if(rng.End > bound)
            {
                isRangeValid = false;
                Printer.Write($"{prompt}: end of range outside bound.");
                return isRangeValid;
            }
            return isRangeValid;
        }

        /// <summary>
        /// converts an Index to integer [Int64]
        /// </summary>
        /// <param name="i"> input Index </param>
        /// <param name="count"> total count </param>
        /// <returns> integer index </returns>
        internal static long IndexToInt(Index i, long count)
        {
            if (i.IsFromEnd) { return (count - i.Value); }
            else { return i.Value; }
        }

        #endregion
        #region dispose

        /// <summary>
        /// implements IDisposable
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            IntelMKL.Free(_dataPtr);
            //Marshal.FreeHGlobal(_dataPtr);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// disposes
        /// if disposing equals true, the method has been called directly
        /// if disposing equals false, the method has been called by the 
        /// runtime from inside the finalizer
        /// </summary>
        /// <param name="disposing"> flag </param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed == false)
            {
                if (disposing) { }
                if (UseMKLalloc) { IntelMKL.Free(_dataPtr); }
                else { Marshal.FreeHGlobal(_dataPtr); }
                //IntelMKLNative.MKL_free(_dataPtr);
                _disposed = true;
            }
        }

        /// <summary>
        /// uses C# destructor syntax for finalization code.
        /// This destructor will run only if the Dispose method
        /// does not get called.
        /// it gives your base class the opportunity to finalize.
        /// </summary>
        ~DenseArrayBase()
        {
            Dispose(false);
        }

        #endregion
    }



}
