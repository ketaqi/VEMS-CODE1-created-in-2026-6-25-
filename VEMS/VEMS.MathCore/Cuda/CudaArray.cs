using System.Runtime.InteropServices;

namespace VEMS.MathCore
{

    /// <summary>
    /// ArrayBase interface
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public unsafe interface ICudaArray<T> : IDisposable where T : struct
    {
        #region properties 

        /// <summary>
        /// number of array elements
        /// </summary>
        long Count { get; set; }

        /// <summary>
        /// IntPtr pointer to the data
        /// </summary>
        IntPtr DataPtr { get; set; }

        /// <summary>
        /// void pointer to the data
        /// </summary>
        void* VPtr { get; }

        #endregion
        #region methods

        ///// <summary>
        ///// check if a given index is valid
        ///// the lower bound is set to zero 
        ///// the upper bound is set to Count
        ///// </summary>
        ///// <param name="i"> input index </param>
        ///// <returns> return true if the index is valid
        ///// otherwise return false </returns>
        //bool IsIndexValid(long i);

        ///// <summary>
        ///// check if a given range is valid
        ///// the lower bound is set to zero 
        ///// the upper bound is set to Count
        ///// </summary>
        ///// <param name="rng"> input range </param>
        ///// <returns> return true if the range is valid
        ///// otherwise return false </returns>
        //bool IsRangeValid(LongRange rng);

        #endregion
    }

    /// <summary>
    /// Cuda array base class
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public unsafe class CudaArray<T> : ICudaArray<T> where T : struct
    {
        #region properties

        /// <summary>
        /// byte-size of a single array element
        /// </summary>
        private SizeT ElementByteSize { get; set; }

        /// <summary>
        /// total byte-size of the array
        /// </summary>
        private SizeT TotalByteSize { get; set; }

        /// <summary>
        /// number of array elements
        /// </summary>
        public long Count { get; set; }

        /// <summary>
        /// IntPtr pointer to the data
        /// </summary>
        public IntPtr DataPtr { get; set; }

        /// <summary>
        /// gets the void pointer to the data
        /// </summary>
        public unsafe void* VPtr => DataPtr.ToPointer();

        /// <summary>
        /// flag whether the array is disposed
        /// </summary>
        public bool Disposed { get; set; }

        #endregion
        #region constructors

        /// <summary>
        /// constructs an empty CudaVectorD
        /// with given number of elements 
        /// </summary>
        /// <param name="count"> number of elements </param>
        public CudaArray(long count)
        {
            Count = count;
            ElementByteSize = Marshal.SizeOf(typeof(T));
            TotalByteSize = Count * ElementByteSize;
            Disposed = false;
            // allocate space on device 
            //void* dp;
            IntPtr dp = new();
            CUDA_Error status = CUDA.Runtime.Malloc(ref dp, TotalByteSize);
            //_ = CudaRTNative.cudaMalloc(&dp, TotalByteSize);
            DataPtr = dp; // new(dp);
        }

        ///// <summary>
        ///// constructs a CudaVector from another Vector on CPU
        ///// </summary>
        ///// <param name="x"> vector on CPU </param>
        //public CudaArray(VectorD x) : this(x.Count)
        //    => CuBLASNative.cublasSetVector((int)Count, 
        //        Marshal.SizeOf(typeof(double)),
        //        x.SPtr, 1,
        //        VPtr, 1);

        #endregion
        #region dispose

        /// <summary>
        /// implements IDisposable
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
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
            if (Disposed == false)
            {
                if (disposing)
                {
                    // release managed resources
                }
                // release unmanaged resources
                CUDA.Runtime.Free(DataPtr);
                // CudaRTNative.cudaFree(VPtr);
                Disposed = true;
            }
        }

        /// <summary>
        /// uses C# destructor syntax for finalization code.
        /// This destructor will run only if the Dispose method
        /// does not get called.
        /// it gives your base class the opportunity to finalize.
        /// </summary>
        ~CudaArray()
        {
            Dispose(false);
        }

        #endregion
    }


    /// <summary>
    /// idea of a SizeT type from http://blogs.hoopoe-cloud.com/index.php/tag/cudanet/, entry from Tuesday, September 15th, 2009
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct SizeT
    {
        private UIntPtr value;

        #region operators 

        #region ----- converters ----- 

        /// <summary>
        /// type converter
        /// </summary>
        /// <param name="value"> input value </param>
        public SizeT(int value)
            => this.value = new UIntPtr((uint)value);

        /// <summary>
        /// type converter
        /// </summary>
        /// <param name="value"> input value </param>
        public SizeT(uint value)
            => this.value = new UIntPtr(value);

        /// <summary>
        /// type converter 
        /// </summary>
        /// <param name="value"> input value </param>
        public SizeT(long value)
            => this.value = new UIntPtr((ulong)value);

        /// <summary>
        /// type converter
        /// </summary>
        /// <param name="value"> input value </param>
        public SizeT(ulong value)
            => this.value = new UIntPtr(value);

        /// <summary>
        /// type converter
        /// </summary>
        /// <param name="value"> input value </param>
        public SizeT(UIntPtr value)
            => this.value = value;

        /// <summary>
        /// type converter
        /// </summary>
        /// <param name="value"> input value </param>
        public SizeT(IntPtr value)
            => this.value = new UIntPtr((ulong)value.ToInt64());

        #endregion
        #region ----- implicity converters -----

        /// <summary>
        /// implicit converter
        /// </summary>
        /// <param name="t"> input value </param>
        /// <returns> output value </returns>
        public static implicit operator int(SizeT t)
            => (int)t.value.ToUInt32();

        /// <summary>
        /// implicit converter
        /// </summary>
        /// <param name="t"> input value </param>
        /// <returns> output value </returns>
        public static implicit operator uint(SizeT t)
            => t.value.ToUInt32();

        /// <summary>
        /// implicit converter
        /// </summary>
        /// <param name="t"> input value </param>
        /// <returns> output value </returns>
        public static implicit operator long(SizeT t)
            => (long)t.value.ToUInt64();

        /// <summary>
        /// implicit converter
        /// </summary>
        /// <param name="t"> input value </param>
        /// <returns> output value </returns>
        public static implicit operator ulong(SizeT t)
            => t.value.ToUInt64();

        /// <summary>
        /// implicit converter
        /// </summary>
        /// <param name="t"> input value </param>
        /// <returns> output value </returns>
        public static implicit operator UIntPtr(SizeT t)
            => t.value;

        /// <summary>
        /// implicit converter
        /// </summary>
        /// <param name="t"> input value </param>
        /// <returns> output value </returns>
        public static implicit operator IntPtr(SizeT t)
            => new((long)t.value.ToUInt64());

        /// <summary>
        /// implicit converter
        /// </summary>
        /// <param name="value"> input value </param>
        /// <returns> output value </returns>
        public static implicit operator SizeT(int value)
            => new(value);

        /// <summary>
        /// implicit converter 
        /// </summary>
        /// <param name="value"> input value </param>
        /// <returns> output value </returns>
        public static implicit operator SizeT(uint value)
            => new(value);

        /// <summary>
        /// implicit converter
        /// </summary>
        /// <param name="value"> input value </param>
        /// <returns> output value </returns>
        public static implicit operator SizeT(long value)
            => new(value);

        /// <summary>
        /// implicit converter 
        /// </summary>
        /// <param name="value"> input value </param>
        /// <returns> output value </returns>
        public static implicit operator SizeT(ulong value)
            => new(value);

        /// <summary>
        /// implicit converter 
        /// </summary>
        /// <param name="value"> input value </param>
        /// <returns> output value </returns>
        public static implicit operator SizeT(IntPtr value)
            => new(value);

        /// <summary>
        /// implicit converter 
        /// </summary>
        /// <param name="value"> input value </param>
        /// <returns> output value </returns>
        public static implicit operator SizeT(UIntPtr value)
            => new(value);

        #endregion
        #region ----- equal/unequal -----

        /// <summary>
        /// unequal
        /// </summary>
        /// <param name="val1"> input #1 </param>
        /// <param name="val2"> input #2 </param>
        /// <returns> whether inputs are unequal </returns>
        public static bool operator !=(SizeT val1, SizeT val2)
            => (val1.value != val2.value);

        /// <summary>
        /// equal 
        /// </summary>
        /// <param name="val1"> input #1 </param>
        /// <param name="val2"> input #2 </param>
        /// <returns> whether inputs are equal </returns>
        public static bool operator ==(SizeT val1, SizeT val2)
            => (val1.value == val2.value);

        #endregion
        #region ----- plus -----

        /// <summary>
        /// defines operator + on converted to ulong values 
        /// to avoid fall back to int
        /// </summary>
        /// <param name="val1"> input #1 </param>
        /// <param name="val2"> input #2 </param>
        /// <returns> sum of inputs </returns>
        public static SizeT operator +(SizeT val1, SizeT val2)
            => new(val1.value.ToUInt64() + val2.value.ToUInt64());

        /// <summary>
        /// defines operator + on converted to ulong values 
        /// to avoid fall back to int
        /// </summary>
        /// <param name="val1"> input #1 </param>
        /// <param name="val2"> input #2 </param>
        /// <returns> sum of inputs </returns>
        public static SizeT operator +(SizeT val1, int val2)
            => new(val1.value.ToUInt64() + (ulong)val2);

        /// <summary>
        /// defines operator + on converted to ulong values 
        /// to avoid fall back to int
        /// </summary>
        /// <param name="val1"> input #1 </param>
        /// <param name="val2"> input #2 </param>
        /// <returns> sum of inputs </returns>
        public static SizeT operator +(int val1, SizeT val2)
            => new((ulong)val1 + val2.value.ToUInt64());

        /// <summary>
        /// defines operator + on converted to ulong values 
        /// to avoid fall back to int
        /// </summary>
        /// <param name="val1"> input #1 </param>
        /// <param name="val2"> input #2 </param>
        /// <returns> sum of inputs </returns>
        public static SizeT operator +(uint val1, SizeT val2)
            => new((ulong)val1 + val2.value.ToUInt64());

        /// <summary>
        /// defines operator + on converted to ulong values 
        /// to avoid fall back to int
        /// </summary>
        /// <param name="val1"> input #1 </param>
        /// <param name="val2"> input #2 </param>
        /// <returns> sum of inputs </returns>
        public static SizeT operator +(SizeT val1, uint val2)
            => new(val1.value.ToUInt64() + (ulong)val2);

        #endregion
        #region ----- minus -----

        /// <summary>
        /// defines operator - on converted to ulong values 
        /// to avoid fall back to int
        /// </summary>
        /// <param name="val1"> input #1 </param>
        /// <param name="val2"> input #2 </param>
        /// <returns> input #1 minus #2 </returns>
        public static SizeT operator -(SizeT val1, SizeT val2)
            => new(val1.value.ToUInt64() - val2.value.ToUInt64());

        /// <summary>
        /// defines operator - on converted to ulong values 
        /// to avoid fall back to int
        /// </summary>
        /// <param name="val1"> input #1 </param>
        /// <param name="val2"> input #2 </param>
        /// <returns> input #1 minus #2 </returns>
        public static SizeT operator -(SizeT val1, int val2)
            => new(val1.value.ToUInt64() - (ulong)val2);

        /// <summary>
        /// define operator - on converted to ulong values 
        /// to avoid fall back to int
        /// </summary>
        /// <param name="val1"> input #1 </param>
        /// <param name="val2"> input #2 </param>
        /// <returns> input #1 minus #2 </returns>
        public static SizeT operator -(int val1, SizeT val2)
            => new((ulong)val1 - val2.value.ToUInt64());

        /// <summary>
        /// defines operator - on converted to ulong values 
        /// to avoid fall back to int
        /// </summary>
        /// <param name="val1"> input #1 </param>
        /// <param name="val2"> input #2 </param>
        /// <returns> input #1 minus #2 </returns>
        public static SizeT operator -(SizeT val1, uint val2)
            => new(val1.value.ToUInt64() - (ulong)val2);

        /// <summary>
        /// defines operator - on converted to ulong values 
        /// to avoid fall back to int
        /// </summary>
        /// <param name="val1"> input #1 </param>
        /// <param name="val2"> input #2 </param>
        /// <returns> input #1 minus #2 </returns>
        public static SizeT operator -(uint val1, SizeT val2)
            => new((ulong)val1 - val2.value.ToUInt64());

        #endregion
        #region ----- multiply -----

        /// <summary>
        /// defines operator * on converted to ulong values 
        /// to avoid fall back to int
        /// </summary>
        /// <param name="val1"> input #1 </param>
        /// <param name="val2"> input #2 </param>
        /// <returns> product of inputs </returns>
        public static SizeT operator *(SizeT val1, SizeT val2)
            => new(val1.value.ToUInt64() * val2.value.ToUInt64());

        /// <summary>
        /// defines operator * on converted to ulong values 
        /// to avoid fall back to int
        /// </summary>
        /// <param name="val1"> input #1 </param>
        /// <param name="val2"> input #2 </param>
        /// <returns> product of inputs </returns>
        public static SizeT operator *(SizeT val1, int val2)
            => new(val1.value.ToUInt64() * (ulong)val2);

        /// <summary>
        /// defines operator * on converted to ulong values 
        /// to avoid fall back to int
        /// </summary>
        /// <param name="val1"> input #1 </param>
        /// <param name="val2"> input #2 </param>
        /// <returns> product of inputs </returns>
        public static SizeT operator *(int val1, SizeT val2)
            => new((ulong)val1 * val2.value.ToUInt64());

        /// <summary>
        /// defines operator * on converted to ulong values 
        /// to avoid fall back to int
        /// </summary>
        /// <param name="val1"> input #1 </param>
        /// <param name="val2"> input #2 </param>
        /// <returns> product of inputs </returns>
        public static SizeT operator *(SizeT val1, uint val2)
            => new(val1.value.ToUInt64() * (ulong)val2);

        /// <summary>
        /// defines operator * on converted to ulong values 
        /// to avoid fall back to int
        /// </summary>
        /// <param name="val1"> input #1 </param>
        /// <param name="val2"> input #2 </param>
        /// <returns> product of inputs </returns>
        public static SizeT operator *(uint val1, SizeT val2)
            => new((ulong)val1 * val2.value.ToUInt64());

        #endregion
        #region ----- divide -----

        /// <summary>
        /// defines operator / on converted to ulong values 
        /// to avoid fall back to int
        /// </summary>
        /// <param name="val1"> input #1 </param>
        /// <param name="val2"> input #2 </param>
        /// <returns> input #1 divided by #2 </returns>
        public static SizeT operator /(SizeT val1, SizeT val2)
            => new(val1.value.ToUInt64() / val2.value.ToUInt64());

        /// <summary>
        /// defines operator / on converted to ulong values 
        /// to avoid fall back to int
        /// </summary>
        /// <param name="val1"> input #1 </param>
        /// <param name="val2"> input #2 </param>
        /// <returns> input #1 divided by #2 </returns>
        public static SizeT operator /(SizeT val1, int val2)
            => new(val1.value.ToUInt64() / (ulong)val2);

        /// <summary>
        /// defines operator / on converted to ulong values 
        /// to avoid fall back to int
        /// </summary>
        /// <param name="val1"> input #1 </param>
        /// <param name="val2"> input #2 </param>
        /// <returns> input #1 divided by #2 </returns>
        public static SizeT operator /(int val1, SizeT val2)
            => new((ulong)val1 / val2.value.ToUInt64());

        /// <summary>
        /// defines operator / on converted to ulong values 
        /// to avoid fall back to int
        /// </summary>
        /// <param name="val1"> input #1 </param>
        /// <param name="val2"> input #2 </param>
        /// <returns> input #1 divided by #2 </returns>
        public static SizeT operator /(SizeT val1, uint val2)
            => new(val1.value.ToUInt64() / (ulong)val2);

        /// <summary>
        /// defines operator / on converted to ulong values 
        /// to avoid fall back to int
        /// </summary>
        /// <param name="val1"> input #1 </param>
        /// <param name="val2"> input #2 </param>
        /// <returns> input #1 divided by #2 </returns>
        public static SizeT operator /(uint val1, SizeT val2)
            => new((ulong)val1 / val2.value.ToUInt64());

        #endregion
        #region ----- greater -----

        /// <summary>
        /// defines operator &gt; on converted to ulong values 
        /// to avoid fall back to int
        /// </summary>
        /// <param name="val1"> input #1 </param>
        /// <param name="val2"> input #2 </param>
        /// <returns> whether input #1 is greater than #2 </returns>
        public static bool operator >(SizeT val1, SizeT val2)
            => val1.value.ToUInt64() > val2.value.ToUInt64();

        /// <summary>
        /// defines operator &gt; on converted to ulong values 
        /// to avoid fall back to int
        /// </summary>
        /// <param name="val1"> input #1 </param>
        /// <param name="val2"> input #2 </param>
        /// <returns> whether input #1 is greater than #2 </returns>
        public static bool operator >(SizeT val1, int val2)
            => val1.value.ToUInt64() > (ulong)val2;

        /// <summary>
        /// defines operator &gt; on converted to ulong values 
        /// to avoid fall back to int
        /// </summary>
        /// <param name="val1"> input #1 </param>
        /// <param name="val2"> input #2 </param>
        /// <returns> whether input #1 is greater than #2 </returns>
        public static bool operator >(int val1, SizeT val2)
            => (ulong)val1 > val2.value.ToUInt64();

        /// <summary>
        /// defines operator &gt; on converted to ulong values 
        /// to avoid fall back to int
        /// </summary>
        /// <param name="val1"> input #1 </param>
        /// <param name="val2"> input #2 </param>
        /// <returns> whether input #1 is greater than #2 </returns>
        public static bool operator >(SizeT val1, uint val2)
            => val1.value.ToUInt64() > (ulong)val2;

        /// <summary>
        /// defines operator &gt; on converted to ulong values 
        /// to avoid fall back to int
        /// </summary>
        /// <param name="val1"> input #1 </param>
        /// <param name="val2"> input #2 </param>
        /// <returns> whether input #1 is greater than #2 </returns>
        public static bool operator >(uint val1, SizeT val2)
            => (ulong)val1 > val2.value.ToUInt64();

        #endregion
        #region ----- smaller -----

        /// <summary>
        /// defines operator &lt; on converted to ulong values 
        /// to avoid fall back to int
        /// </summary>
        /// <param name="val1"> input #1 </param>
        /// <param name="val2"> input #2 </param>
        /// <returns> whether input #2 is smaller than #2 </returns>
        public static bool operator <(SizeT val1, SizeT val2)
            => val1.value.ToUInt64() < val2.value.ToUInt64();

        /// <summary>
        /// defines operator &lt; on converted to ulong values 
        /// to avoid fall back to int
        /// </summary>
        /// <param name="val1"> input #1 </param>
        /// <param name="val2"> input #2 </param>
        /// <returns> whether input #2 is smaller than #2 </returns>
        public static bool operator <(SizeT val1, int val2)
            => val1.value.ToUInt64() < (ulong)val2;

        /// <summary>
        /// defines operator &lt; on converted to ulong values 
        /// to avoid fall back to int
        /// </summary>
        /// <param name="val1"> input #1 </param>
        /// <param name="val2"> input #2 </param>
        /// <returns> whether input #2 is smaller than #2 </returns>
        public static bool operator <(int val1, SizeT val2)
            => (ulong)val1 < val2.value.ToUInt64();

        /// <summary>
        /// defines operator &lt; on converted to ulong values 
        /// to avoid fall back to int
        /// </summary>
        /// <param name="val1"> input #1 </param>
        /// <param name="val2"> input #2 </param>
        /// <returns> whether input #2 is smaller than #2 </returns>
        public static bool operator <(SizeT val1, uint val2)
            => val1.value.ToUInt64() < (ulong)val2;

        /// <summary>
        /// defines operator &lt; on converted to ulong values 
        /// to avoid fall back to int
        /// </summary>
        /// <param name="val1"> input #1 </param>
        /// <param name="val2"> input #2 </param>
        /// <returns> whether input #2 is smaller than #2 </returns>
        public static bool operator <(uint val1, SizeT val2)
            => (ulong)val1 < val2.value.ToUInt64();

        #endregion

        #endregion
        #region bscis methods 

        /// <summary>
        /// equals 
        /// </summary>
        /// <param name="obj"> another object </param>
        /// <returns> whether equals </returns>
        public override bool Equals(object obj)
        {
            if (!(obj is SizeT)) return false;
            SizeT o = (SizeT)obj;
            return this.value.Equals(o.value);
        }

        /// <summary>
        /// returns this.value.ToString()
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (IntPtr.Size == 4)
                return ((uint)this.value.ToUInt32()).ToString();
            else
                return ((ulong)this.value.ToUInt64()).ToString();
        }

        /// <summary>
        /// Returns this.value.GetHashCode()
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return this.value.GetHashCode();
        }

        #endregion

    }
}
