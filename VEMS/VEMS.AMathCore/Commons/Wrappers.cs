using VEMS.AMathCore.XTMethods;

namespace VEMS.AMathCore
{

    [Obsolete]
    public class VectorD : Vect<Real>
    {
        #region properties
        public unsafe double* SPtr
        {
            get => DPtr;
        }

        /// <summary>
        /// get / set the vector elements within a given range
        /// </summary>
        /// <param name="rng"> range with inclusive start and exclusive end indices [Int64] </param>
        /// <returns> vector elements within the range </returns>
        public new VectorD this[LongRange rng]
        {
            get
            {
                Vect<Real> t = base[rng];
                return new(t, false);
            }
            set
            {
                base[rng] = value;
            }
        }

        /// <summary>
        /// get / set the vector elements within a given range
        /// </summary>
        /// <param name="rng"> range with inclusive start and exclusive end indices [Int32] </param>
        /// <returns> vector elements within the range </returns>
        public new VectorD this[Range rng]
        {
            get => this[new LongRange(rng, Count)];
            set => this[new LongRange(rng, Count)] = value;
        }
        #endregion

        #region constructors
        /// <summary>
        /// Constructs a vector with given length.
        /// By default, initializes element values to zeros
        /// </summary>
        /// <param name="count">The total number of vector elements.</param>
        /// <param name="initMode">Initialization mode option; default is CALLOC.</param>
        public VectorD(long count,
            ArrayInitMode initMode = ArrayInitMode.Calloc)
            : base(count, initMode) { }

        /// <summary>
        /// constructs a vector with given length
        /// and sets all elements to the same initial value
        /// </summary>
        /// <param name="count"> count of the element </param>
        /// <param name="initVal"> initial value for all the elements </param>
        /// <param name="loopMode"> loop-computation mode option </param>
        public VectorD(long count, double initVal)
            //,LoopMode loopMode = Defaults.LoopOption)
            : base(Vect<Real>.Create(count, initVal)) { }

        /// <summary>
        /// constructs a vector with given length
        /// first element to the initial value
        /// and the rest follow the increment
        /// </summary>
        /// <param name="count"> count of the elements </param>
        /// <param name="initVal"> value of the first element </param>
        /// <param name="increment"> increment for the rest elements </param>
        /// <param name="loopMode"> loop-computation mode option </param>
        public VectorD(long count, double initVal, double increment)
            //,LoopMode loopMode = Defaults.LoopOption)
            : base(Vect<Real>.Create(count, initVal, increment)) { }

        /// <summary>
        /// constructs a vector by copying from another
        /// </summary>
        /// <param name="other"> another vector </param>
        /// <param name="deepCopy"> copy mode option; default is deep copy </param>
        public VectorD(VectorD other, bool deepCopy = true)
            : base((Vect<Real>)other, deepCopy ? ArrayCopyMode.Deep : ArrayCopyMode.Shallow) { }

        /// <summary>
        /// Initializes a new instance of the VectorD class by copying the elements from another Vect<Real> instance.
        /// </summary>
        /// <param name="other">The source Vect<Real> to copy elements from.</param>
        /// <param name="deepCopy">Indicates whether to perform a deep copy of the elements.</param>
        public VectorD(Vect<Real> other, bool deepCopy = true)
            : base(other, deepCopy ? ArrayCopyMode.Deep : ArrayCopyMode.Shallow) { }
        #endregion

        #region methods
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
                return new(0);
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
                return new(0);
            }
            return Padding(targetCount, (targetCount - Count) / 2, 0.0);
        }
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
            => new(VMath.Operators.Add(x, y), false);
        //=> (VectorD)VMath.Operators.Add(x, y);
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
            => new(VMath.Operators.Add(x, s), false);

        /// <summary>
        /// adds a scalar s to vector x
        /// res[i] = s + x[i]
        /// </summary>
        /// <param name="s"> input scalar s </param>
        /// <param name="x"> input vector x </param>
        /// <returns> result vector </returns>
        public static VectorD operator +(double s, VectorD x)
            => new(VMath.Operators.Add(x, s), false);
        #endregion
        #region v-s add [mixed]
        /// <summary>
        /// adds a scalar s to vector x
        /// res[i] = x[i] + s
        /// </summary>
        /// <param name="x"> input vector x </param>
        /// <param name="s"> input scalar s </param>
        /// <returns> result vector </returns>
        public static VectorZ operator +(VectorD x, Cplx s)
            => new VectorZ(x) + s;

        /// <summary>
        /// adds a scalar s to vector x
        /// res[i] = s + x[i]
        /// </summary>
        /// <param name="x"> input vector x </param>
        /// <param name="s"> input scalar s </param>
        /// <returns> result vector </returns>
        public static VectorZ operator +(Cplx s, VectorD x)
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
            => new(VMath.Operators.Sub(x, y), false);
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
            => new(VMath.Operators.Sub(x, s), false);

        /// <summary>
        /// subtracts each element of vector x from a scalar s
        /// res[i] = s - x[i]
        /// </summary>
        /// <param name="s"> input scalar s </param>
        /// <param name="x"> input vector x </param>
        /// <returns> result vector </returns>
        public static VectorD operator -(double s, VectorD x)
            => s + (-x);
        #endregion
        #region v-s subtract [mixed]
        /// <summary>
        /// subtracts a scalar s from vector x
        /// res[i] = x[i] - s
        /// </summary>
        /// <param name="x"> input vector x </param>
        /// <param name="s"> input scalar s </param>
        /// <returns> result vector </returns>
        public static VectorZ operator -(VectorD x, Cplx s)
            => new VectorZ(x) - s;

        /// <summary>
        /// subtracts each element of vector x 
        /// from a scalar s
        /// res[i] = s - x[i]
        /// </summary>
        /// <param name="s"> input scalar s </param>
        /// <param name="x"> input vector x </param>
        /// <returns> result vector </returns>
        public static VectorZ operator -(Cplx s, VectorD x)
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
            => new(VMath.Operators.Mul(x, y), false);

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
            => new(VMath.Operators.Mul(s, x), false);


        /// <summary>
        /// multiplies a scalar s with a vector x
        /// res[i] = s * x[i]
        /// </summary>
        /// <param name="s"> input scalar s </param>
        /// <param name="x"> input vector x </param>
        /// <returns> result vector </returns>
        public static VectorD operator *(double s, VectorD x)
            => new(VMath.Operators.Mul(s, x), false);

        #endregion
        #region v-s multiply [mixed]
        /// <summary>
        /// multiplies a vector x with a scalar s
        /// res[i] = x[i] * s
        /// </summary>
        /// <param name="x"> input vector x </param>
        /// <param name="s"> input scalar s </param>
        /// <returns> result vector </returns>
        public static VectorZ operator *(VectorD x, Cplx s)
            => new VectorZ(x) * s;

        /// <summary>
        /// multiplies a scalar s with a vector x
        /// res[i] = s * x[i]
        /// </summary>
        /// <param name="s"> input scalar s </param>
        /// <param name="x"> input vector x </param>
        /// <returns> result vector </returns>
        public static VectorZ operator *(Cplx s, VectorD x)
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
            => new(VMath.Operators.Div(x, y), false);
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
            => new(VMath.Operators.Div(x, s), false);

        /// <summary>
        /// divides a scalar s by vector x
        /// res[i] = s / x[i]
        /// </summary>
        /// <param name="s"> input scalar s </param>
        /// <param name="x"> input vector x </param>
        /// <returns> result vector </returns>
        public static VectorD operator /(double s, VectorD x)
            => new(VMath.Operators.Div(s, x), false);
        #endregion
        #region v-s divide [mixed]
        /// <summary>
        /// divides a vector x by scalar s
        /// res[i] = x[i] / s
        /// </summary>
        /// <param name="x"> input vector x </param>
        /// <param name="s"> input scalar s </param>
        /// <returns> result vector </returns>
        public static VectorZ operator /(VectorD x, Cplx s)
            => new VectorZ(x) / s;

        /// <summary>
        /// divides a scalar s by vector x
        /// res[i] = s / x[i]
        /// </summary>
        /// <param name="s"> input scalar s </param>
        /// <param name="x"> input vector x </param>
        /// <returns> result vector </returns>
        public static VectorZ operator /(Cplx s, VectorD x)
            => s / new VectorZ(x);
        #endregion
        #region negative
        /// <summary>
        /// takes the negative of the input vector
        /// </summary>
        /// <param name="x"> input vector x </param>
        /// <returns> result negative vector </returns>
        public static VectorD operator -(VectorD x)
            => new(VMath.Operators.Neg(x), false);
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

    [Obsolete]
    public class VectorZ : Vect<Cplx>
    {
        #region properties
        /// <summary>
        /// gets the type-specific pointer to the values
        /// </summary>
        public unsafe Cplx* SPtr
        {
            get => (Cplx*)DPtr;
        }

        /// <summary>
        /// get / set the vector elements within a given range
        /// </summary>
        /// <param name="rng"> range with inclusive start and exclusive end indices [Int64] </param>
        /// <returns> vector elements within the range </returns>
        public new VectorZ this[LongRange rng]
        {
            get
            {
                Vect<Cplx> t = base[rng];
                return new(t, false);
            }
            set
            {
                base[rng] = value;
            }
        }

        /// <summary>
        /// get / set the vector elements within a given range
        /// </summary>
        /// <param name="rng"> range with inclusive start and exclusive end indices [Int32] </param>
        /// <returns> vector elements within the range </returns>
        public new VectorZ this[Range rng]
        {
            get => this[new LongRange(rng, Count)];
            set => this[new LongRange(rng, Count)] = value;
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
        public VectorZ(long count, Cplx initVal)
            //,LoopMode loopMode = Defaults.LoopOption)
            : base(Vect<Cplx>.Create(count, initVal)) { }

        /// <summary>
        /// constructs a vector with given length
        /// and sets all elements to the same value 
        /// </summary>
        /// <param name="count"> count of the elements </param>
        /// <param name="initVal"> initial value for all elements </param>
        public VectorZ(long count, double initVal)
            : this(count, new Cplx(initVal, 0.0)) { }

        /// <summary>
        /// constructs a vector with given length
        /// first element to the initial value
        /// and the rest follow the increment
        /// </summary>
        /// <param name="count"> count of the elements </param>
        /// <param name="initVal"> value of the first element </param>
        /// <param name="increment"> increment for the rest elements </param>
        /// <param name="loopMode"> loop-computation mode option </param>
        public VectorZ(long count, Cplx initVal, Cplx increment)
            //,LoopMode loopMode = Defaults.LoopOption)
            : base(Vect<Cplx>.Create(count, initVal, increment)) { }

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
            : base(Vect<Cplx>.Create(count, initVal, increment)) { }

        /// <summary>
        /// constructs a vector by copying from another
        /// </summary>
        /// <param name="other"> another vector </param>
        /// <param name="deepCopy"> copy mode option; default is deep copy </param>
        public VectorZ(VectorZ other, bool deepCopy = true)
            : base((Vect<Cplx>)other, deepCopy ? ArrayCopyMode.Deep : ArrayCopyMode.Shallow) { }

        /// <summary>
        /// constructs a complex vector 
        /// with its real or imaginary part only
        /// </summary>
        /// <param name="part"> part of the vector </param>
        /// <param name="option"> option for the complex part; default is real-part </param>
        public VectorZ(VectorD part, ComplexPart option = ComplexPart.RealPart)
            : this(part.Count, ArrayInitMode.Calloc)
        {
            switch (option)
            {
                case ComplexPart.RealPart:
                    {
                        VectorZ t = this;
                        VMath.Modify(part, t, ComplexPart.RealPart);
                        break;
                    }
                case ComplexPart.ImagPart:
                    {
                        VectorZ t = this;
                        VMath.Modify(part, t, ComplexPart.ImagPart);
                        break;
                    }
                default: goto case ComplexPart.RealPart;
            }
        }

        /// <summary>
        /// Initializes a new instance of the VectorZ class by copying the elements from another Vect<Cplx> instance.
        /// </summary>
        /// <param name="other">The source Vect<Real> to copy elements from.</param>
        /// <param name="deepCopy">Indicates whether to perform a deep copy of the elements.</param>
        public VectorZ(Vect<Cplx> other, bool deepCopy = true)
            : base(other, deepCopy ? ArrayCopyMode.Deep : ArrayCopyMode.Shallow) { }
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
                return new(0);
            }

            Cplx paddingValue = new(paddingValueRe, paddingValueIm);
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
                return new(0);
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
                return new(0);
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
                return new(0);
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
                Cplx temp = this[start, checkBound: check];
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
            => new(VMath.Operators.Add(x, y), false);
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
        public static VectorZ operator +(VectorZ x, Cplx s)
            => new(VMath.Operators.Add(x, s));

        /// <summary>
        /// adds a scalar s to vector x
        /// res[i] = s + x[i]
        /// </summary>
        /// <param name="s"> input scalar s </param>
        /// <param name="x"> input vector x </param>
        /// <returns> result vector </returns>
        public static VectorZ operator +(Cplx s, VectorZ x)
            => x + s;
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
            => x + new Cplx(s, 0.0);

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
            => new(VMath.Operators.Sub(x, y), false);
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
        public static VectorZ operator -(VectorZ x, Cplx s)
            => new(VMath.Operators.Sub(x, s));

        /// <summary>
        /// subtracts each element of vector x from a scalar s
        /// res[i] = s - x[i]
        /// </summary>
        /// <param name="s"> input scalar s </param>
        /// <param name="x"> input vector x </param>
        /// <returns> result vector </returns>
        public static VectorZ operator -(Cplx s, VectorZ x)
            => s + (-x);
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
            => new(VMath.Operators.Mul(x, y));
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
        public static VectorZ operator *(VectorZ x, Cplx s)
            => new(VMath.Operators.Mul(s, x));

        /// <summary>
        /// multiplies a scalar s with a vector x
        /// res[i] = s * x[i]
        /// </summary>
        /// <param name="s"> input scalar s </param>
        /// <param name="x"> input vector x </param>
        /// <returns> result vector </returns>
        public static VectorZ operator *(Cplx s, VectorZ x)
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
        public static VectorZ operator *(VectorZ x, double s)
            => x * new Cplx(s, 0.0);

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
            => new(VMath.Operators.Div(x, y));
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
        public static VectorZ operator /(VectorZ x, Cplx s)
            => new(VMath.Operators.Div(x, s));

        /// <summary>
        /// divides a scalar s by vector x
        /// res[i] = s / x[i]
        /// </summary>
        /// <param name="s"> input scalar s </param>
        /// <param name="x"> input vector x </param>
        /// <returns> result vector </returns>
        public static VectorZ operator /(Cplx s, VectorZ x)
            => new(VMath.Operators.Div(s, x));
        //=> new VectorZ(x.Count, s) / x;
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
            => x / new Cplx(s, 0.0);

        /// <summary>
        /// divides a scalar s by vector x
        /// res[i] = s / x[i]
        /// </summary>
        /// <param name="s"> input scalar s </param>
        /// <param name="x"> input vector x </param>
        /// <returns> result vector </returns>
        public static VectorZ operator /(double s, VectorZ x)
            => new Cplx(s, 0.0) / x;
        #endregion
        #region negative
        /// <summary>
        /// takes the negative of the input vector
        /// </summary>
        /// <param name="x"> input vector x </param>
        /// <returns> result negative vector </returns>
        public static VectorZ operator -(VectorZ x)
            => new(VMath.Operators.Neg(x), false);
        #endregion
        #endregion
    }

    [Obsolete]
    public class MatrixD : Matx<Real>
    {
        #region properties
        /// <summary>
        /// gets the type-specific pointer to the values
        /// </summary>
        public unsafe double* SPtr
        {
            get => (double*)DPtr;
        }

        /// <summary>
        /// get / set a row slice from / into the matrix
        /// </summary>
        /// <param name="iRow"> row index [Int64]</param>
        /// <param name="colRng"> column range with inclusive start and exclusive end indices [Int64]  </param>
        /// <returns> row slice vector </returns>
        public new VectorD this[long iRow, LongRange colRng]
        {
            get
            {
                Vect<Real> x = base[iRow, colRng];

                return new(x, false);
            }
            set
            {
                base[iRow, colRng] = value;
            }
        }

        /// <summary>
        /// get / set a row slice from / into the matrix
        /// </summary>
        /// <param name="iRow"> row index [Int32]</param>
        /// <param name="colRng"> column range with inclusive start and exclusive end indices [Int32]  </param>
        /// <returns> row slice vector </returns>
        public new VectorD this[Index iRow, Range colRng]
        {
            get => this[iRow.ToLong(Rows), new LongRange(colRng, Cols)];
            set => this[iRow.ToLong(Rows), new LongRange(colRng, Cols)] = value;
        }

        /// <summary>
        /// get / set a column slice from / into the matrix
        /// </summary>
        /// <param name="rowRng"> row range with inclusive start and exclusive end indices [Int64] </param>
        /// <param name="iCol"> column index [Int64] </param>
        /// <returns> column slice vector </returns>
        public new VectorD this[LongRange rowRng, long iCol]
        {
            get
            {
                Vect<Real> x = base[rowRng, iCol];
                return new(x, false);
            }
            set
            {
                base[rowRng, iCol] = value;
            }
        }

        /// <summary>
        /// get / set a column slice from / into the matrix
        /// </summary>
        /// <param name="rowRng"> row range with inclusive start and exclusive end indices [Int32] </param>
        /// <param name="iCol"> column index [Int32] </param>
        /// <param name="loopMode"> loop computational mode option </param>
        /// <returns> column slice vector </returns>
        public new VectorD this[Range rowRng, Index iCol,
            LoopMode loopMode = Defaults.LoopOption]
        {
            get => this[new LongRange(rowRng, Rows), iCol.ToLong(Cols)];
            set => this[new LongRange(rowRng, Rows), iCol.ToLong(Cols)] = value;
        }

        /// <summary>
        /// get / set a block from / into the matrix
        /// </summary>
        /// <param name="rowRng"> row range with inclusive start and exclusive end indices [Int64] </param>
        /// <param name="colRng"> column range with inclusive start and exclusive end indices [Int64] </param>
        /// <returns> sub-matrix </returns>
        public new MatrixD this[LongRange rowRng, LongRange colRng]
        {
            get
            {
                Matx<Real> x = base[rowRng, colRng];

                return new(x, false);
            }
            set
            {
                Matx<Real> x = value;
            }
        }

        /// <summary>
        /// get / set a block from / into the matrix
        /// </summary>
        /// <param name="rowRng"> row range with inclusive start and exclusive end indices [Int32] </param>
        /// <param name="colRng"> column range with inclusive start and exclusive end indices [Int32] </param>
        /// <param name="loopMode"> loop computational mode option </param>
        /// <returns> sub-matrix </returns>
        public new MatrixD this[Range rowRng, Range colRng]
        {
            get => this[new LongRange(rowRng, Rows), new LongRange(colRng, Cols)];
            set => this[new LongRange(rowRng, Rows), new LongRange(colRng, Cols)] = value;
        }

        #endregion

        #region constructors

        /// <summary>
        /// constructs a matrix with given length
        /// by default, does not initialize element values
        /// </summary>
        /// <param name="rows"> number of rows </param>
        /// <param name="cols"> number of columns </param>
        /// <param name="mode"> construct mode option; default is CALLOC </param>
        public MatrixD(long rows, long cols,
            ArrayInitMode mode = ArrayInitMode.Calloc)
            : base(rows, cols, mode) { }

        /// <summary>
        /// constructs a matrix with given size
        /// and sets all elements to the same value 
        /// </summary>
        /// <param name="rows"> number of rows </param>
        /// <param name="cols"> number of cols </param>
        /// <param name="initVal"> initial value for all the elements </param>//youwenti
        public MatrixD(long rows, long cols, double initVal)
            : base(Create(rows, cols, initVal)) { }



        /// <summary>
        /// constructs a matrix by copying from another
        /// </summary>
        /// <param name="other"> another matrix </param>
        /// <param name="deepCopy"> copy mode option; default is deep copy </param>
        public MatrixD(MatrixD other, bool deepCopy = true)
            : base(other, deepCopy ? ArrayCopyMode.Deep : ArrayCopyMode.Shallow) { }

        /// <summary>
        /// Initializes a new instance of the MatrixD class by copying the elements from another Matx<Real> instance.
        /// </summary>
        /// <param name="other"> another matrix </param>
        /// <param name="deepCopy"> copy mode option; default is deep copy </param>
        public MatrixD(Matx<Real> other, bool deepCopy = true)
            : base(other, deepCopy ? ArrayCopyMode.Deep : ArrayCopyMode.Shallow) { }



        /// <summary>
        /// constructs a matrix by copying from a given ArrayBase
        /// </summary>
        /// <param name="other"> another ArrayBase </param>
        /// <param name="rows"> number of rows </param>
        /// <param name="cols"> number of columns </param>
        /// <param name="deepCopy"> copy mode option; default is deep copy </param>
        public MatrixD(DenseArray<Real> other, long rows, long cols, bool deepCopy = true)
            : base(rows, cols, deepCopy ? ArrayInitMode.Malloc : ArrayInitMode.None)
        { }


        #endregion

        #region methods

        /// <summary>
        /// padding according to target matrix parameters
        /// </summary>
        /// <param name="targetRows"> target number of rows in the padded matrix </param>
        /// <param name="targetCols"> target number of columns in the padded matrix </param>
        /// <param name="startRowIndex"> starting row index in the padded matrix </param>
        /// <param name="startColIndex"> starting column index in the padded matrix </param>
        /// <param name="paddingValue"> value used for the padding </param>
        /// <returns> result matrix after padding </returns>
        public new MatrixD Padding(long targetRows, long targetCols,
            long startRowIndex, long startColIndex,
            double paddingValue = 0.0)
        {
            if (targetRows <= Rows)
            {
                Printer.Warning($"{nameof(targetRows)} must be greater than the current value");
                return new(0, 0);
            }
            if (targetCols <= Cols)
            {
                Printer.Warning($"{nameof(targetCols)} must be greater than the current value");
                return new(0, 0);
            }

            MatrixD y = new(targetRows, targetCols, paddingValue);
            LongRange rowRng = new(startRowIndex, startRowIndex + Rows);
            LongRange colRng = new(startColIndex, startColIndex + Cols);
            y[rowRng, colRng] = this;

            return y;
        }

        /// <summary>
        /// centered zero-padding around each side
        /// </summary>
        /// <param name="targetRows"> target number of rows </param>
        /// <param name="targetCols"> target number of columns </param>
        /// <returns> result matrix after padding </returns>
        public new MatrixD Padding(long targetRows, long targetCols)
        {
            if ((targetRows - Rows) % 2 != 0)
            {
                Printer.Warning($"{nameof(targetRows)} must be an even addition to the current value");
                return new(0, 0);
            }
            if ((targetCols - Cols) % 2 != 0)
            {
                Printer.Warning($"{nameof(targetCols)} must be an even addition to the current value");
                return new(0, 0);
            }
            return Padding(targetRows, targetCols, (targetRows - Rows) / 2, (targetCols - Cols) / 2, 0.0);
        }


        #endregion

        #region operators

        #region m-m add [real]
        /// <summary>
        /// computes the sum of two matrices x and y
        /// res[i,j] = x[i,j] + y[i,j]
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <param name="y"> input matrix y </param>
        /// <returns> result matrix </returns>
        public static MatrixD operator +(MatrixD x, MatrixD y)
            => new(VMath.Operators.Add(x, y), false);
        #endregion
        #region m-s add [real]
        /// <summary>
        /// adds a scalar s to matrix x
        /// res[i,j] = x[i,j] + s
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <param name="s"> input scalar s </param>
        /// <returns> result matrix </returns>
        public static MatrixD operator +(MatrixD x, double s)
            => new(VMath.Operators.Add(x, s), false);

        /// <summary>
        /// adds a scalar s to matrix x
        /// res[i,j] = x[i,j] + s
        /// </summary>
        /// <param name="s"> input scalar s </param>
        /// <param name="x"> input matrix x </param>
        /// <returns> result matrix </returns>
        public static MatrixD operator +(double s, MatrixD x)
            => new(VMath.Operators.Add(x, s), false);
        #endregion
        #region m-s add [mixed]
        /// <summary>
        /// adds a scalar s to matrix x
        /// res[i,j] = x[i,j] + s
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <param name="s"> input scalar s </param>
        /// <returns> result matrix </returns>
        public static MatrixZ operator +(MatrixD x, Cplx s)
            => new MatrixZ(x) + s;

        /// <summary>
        /// adds a scalar s to matrix x
        /// res[i,j] = s + x[i,j]
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <param name="s"> input scalar s </param>
        /// <returns> result matrix </returns>
        public static MatrixZ operator +(Cplx s, MatrixD x)
            => x + s;
        #endregion
        #region m-m subtract [real]
        /// <summary>
        /// subtracts one matrix y from another matrix x
        /// res[i,j] = x[i,j] - y[i,j]
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <param name="y"> input matrix y </param>
        /// <returns> result matrix </returns>
        public static MatrixD operator -(MatrixD x, MatrixD y)
            => new(VMath.Operators.Sub(x, y), false);
        #endregion
        #region m-s subtract [real]
        /// <summary>
        /// subtracts a scalar s from matrix x
        /// res[i,j] = x[i,j] - s
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <param name="s"> input scalar s </param>
        /// <returns> result matrix </returns>
        public static MatrixD operator -(MatrixD x, double s)
            => new(VMath.Operators.Sub(x, s), false);

        /// <summary>
        /// subtracts each element of matrix x from a scalar s
        /// res[i,j] = s - x[i,j]
        /// </summary>
        /// <param name="s"> input scalar s </param>
        /// <param name="x"> input matrix x </param>
        /// <returns> result matrix </returns>
        public static MatrixD operator -(double s, MatrixD x)
            => new(VMath.Operators.Sub(x, s), false);
        #endregion
        #region m-s subtract [mixed]
        /// <summary>
        /// substracts a scalar s from matrix x
        /// res[i,j] = x[i,j] - s
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <param name="s"> input scalar s </param>
        /// <returns> result matrix </returns>
        public static MatrixZ operator -(MatrixD x, Cplx s)
            => new MatrixZ(x) - s;

        /// <summary>
        /// subtracts a scalar s from matrix x
        /// res[i,j] = s - x[i,j]
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <param name="s"> input scalar s </param>
        /// <returns> result matrix </returns>
        public static MatrixZ operator -(Cplx s, MatrixD x)
            => s - new MatrixZ(x);
        #endregion
        #region m-m multiply [real]
        /// <summary>
        /// performs element by element multiplication 
        /// of two matrices x and y
        /// res[i,j] = x[i,j] * y[i,j]
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <param name="y"> input matrix y </param>
        /// <returns> result matrix </returns>
        public static MatrixD operator *(MatrixD x, MatrixD y)
            => new(VMath.Operators.Mul(x, y), false);
        #endregion
        #region m-s multiply [real]
        /// <summary>
        /// multiplies a matrix x with a scalar s
        /// res[i,j] = x[i,j] * s
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <param name="s"> input scalar s </param>
        /// <returns> result matrix </returns>
        public static MatrixD operator *(MatrixD x, double s)
        => new(VMath.Operators.Mul(s, x), false);
        //=> VMath.Scale(x, s);

        /// <summary>
        /// multiplies a scalar s with a matrix x
        /// res[i,j] = s * x[i,j]
        /// </summary>
        /// <param name="s"> input scalar s </param>
        /// <param name="x"> input matrix x </param>
        /// <returns> result matrix </returns>
        public static MatrixD operator *(double s, MatrixD x)
            => new(VMath.Operators.Mul(s, x), false);
        #endregion
        #region m-s multiply [mixed]
        /// <summary>
        /// multiplies a matrix x with a scalar s
        /// res[i,j] = x[i,j] * s
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <param name="s"> input scalar s </param>
        /// <returns> result matrix </returns>
        public static MatrixZ operator *(MatrixD x, Cplx s)
            => new MatrixZ(x) * s;

        /// <summary>
        /// multiplies a scalar s with a matrix x
        /// res[i,j] = s * x[i,j]
        /// </summary>
        /// <param name="s"> input scalar s </param>
        /// <param name="x"> input matrix x </param>
        /// <returns> result matrix </returns>
        public static MatrixZ operator *(Cplx s, MatrixD x)
            => x * s;
        #endregion
        #region m-m divide [real]
        /// <summary>
        /// performs element by element inversion 
        /// of two matrices x and y
        /// res[i,j] = x[i,j] / y[i,j]
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <param name="y"> input matrix y </param>
        /// <returns> result matrix </returns>
        public static MatrixD operator /(MatrixD x, MatrixD y)
           => new(VMath.Operators.Div(x, y), false);
        #endregion
        #region m-s divide [real]
        /// <summary>
        /// divides a matrix x by scalar s
        /// res[i,j] = x[i,j] / s
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <param name="s"> input scalar s </param>
        /// <returns> result matrix </returns>
        public static MatrixD operator /(MatrixD x, double s)
        => new(VMath.Operators.Div(x, s), false);
        //=> VMath.Scale(x, 1.0 / s);

        /// <summary>
        /// divides a scalar s by matrix x
        /// res[i,j] = s / x[i,j]
        /// </summary>
        /// <param name="s"> input scalar s </param>
        /// <param name="x"> input matrix x </param>
        /// <returns> result matrix </returns>
        public static MatrixD operator /(double s, MatrixD x)
         => new(VMath.Operators.Div(s, x), false);
        #endregion
        #region m-s divide [mixed]
        /// <summary>
        /// divides a matrix x by scalar s
        /// res[i,j] = x[i,j] / s
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <param name="s"> input scalar s </param>
        /// <returns> result matrix </returns>
        public static MatrixZ operator /(MatrixD x, Cplx s)
            => new MatrixZ(x) / s;

        /// <summary>
        /// divides a scalar s by matrix x
        /// res[i,j] = s / x[i,j]
        /// </summary>
        /// <param name="s"> input scalar s </param>
        /// <param name="x"> input matrix x </param>
        /// <returns> result matrix </returns>
        public static MatrixZ operator /(Cplx s, MatrixD x)
            => s / new MatrixZ(x);
        #endregion
        #region negative
        /// <summary>
        /// takes the negative of the input matrix
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <returns> result negative matrix </returns>
        public static MatrixD operator -(MatrixD x)
         => new(VMath.Operators.Neg(x), false);
        //=> VMath.Scale(x, -1.0);
        #endregion
        #region explicit conversion

        /// <summary>
        /// explicit conversion from MatrixD to MatrixZ
        /// </summary>
        /// <param name="x"> input real-part matrix </param>
        public static explicit operator MatrixZ(MatrixD x)
            => new(part: x, option: ComplexPart.RealPart);

        #endregion

        #endregion
    }
    [Obsolete]
    public class MatrixZ : Matx<Cplx>
    {
        #region properties
        /// <summary>
        /// empty matrix with ZERO row and column count
        /// </summary>
        public static MatrixZ Empty = new(0, 0);

        /// <summary>
        /// gets the type-specific pointer to the values
        /// </summary>
        public unsafe Cplx* ZPtr
        {
            get => (Cplx*)DPtr;
        }

        /// <summary>
        /// get / set a row slice from / into the matrix
        /// </summary>
        /// <param name="iRow"> row index [Int64]</param>
        /// <param name="colRng"> column range with inclusive start and exclusive end indices [Int64]  </param>
        /// <returns> row slice vector </returns>
        public new VectorZ this[long iRow, LongRange colRng]
        {
            get
            {
                Vect<Cplx> x = base[iRow, colRng];

                return new(x, false);
            }
            set
            {
                base[iRow, colRng] = value;
            }
        }

        /// <summary>
        /// get / set a row slice from / into the matrix
        /// </summary>
        /// <param name="iRow"> row index [Int32]</param>
        /// <param name="colRng"> column range with inclusive start and exclusive end indices [Int32]  </param>
        /// <returns> row slice vector </returns>
        public new VectorZ this[Index iRow, Range colRng]
        {
            get => this[iRow.ToLong(Rows), new LongRange(colRng, Cols)];
            set => this[iRow.ToLong(Rows), new LongRange(colRng, Cols)] = value;
        }

        /// <summary>
        /// get / set a column slice from / into the matrix
        /// </summary>
        /// <param name="rowRng"> row range with inclusive start and exclusive end indices [Int64] </param>
        /// <param name="iCol"> column index [Int64] </param>
        /// <returns> column slice vector </returns>
        public new VectorZ this[LongRange rowRng, long iCol]
        {
            get
            {
                Vect<Cplx> x = base[rowRng, iCol];
                return new(x, false);
            }
            set
            {
                base[rowRng, iCol] = value;
            }
        }

        /// <summary>
        /// get / set a column slice from / into the matrix
        /// </summary>
        /// <param name="rowRng"> row range with inclusive start and exclusive end indices [Int32] </param>
        /// <param name="iCol"> column index [Int32] </param>
        /// <param name="loopMode"> loop computational mode option </param>
        /// <returns> column slice vector </returns>
        public new VectorZ this[Range rowRng, Index iCol,
            LoopMode loopMode = Defaults.LoopOption]
        {
            get => this[new LongRange(rowRng, Rows), iCol.ToLong(Cols)];
            set => this[new LongRange(rowRng, Rows), iCol.ToLong(Cols)] = value;
        }

        /// <summary>
        /// get / set a block from / into the matrix
        /// </summary>
        /// <param name="rowRng"> row range with inclusive start and exclusive end indices [Int64] </param>
        /// <param name="colRng"> column range with inclusive start and exclusive end indices [Int64] </param>
        /// <returns> sub-matrix </returns>
        public new MatrixZ this[LongRange rowRng, LongRange colRng]
        {
            get
            {
                Matx<Cplx> x = base[rowRng, colRng];

                return new(x, false);
            }
            set
            {
                Matx<Cplx> x = value;
            }
        }

        /// <summary>
        /// get / set a block from / into the matrix
        /// </summary>
        /// <param name="rowRng"> row range with inclusive start and exclusive end indices [Int32] </param>
        /// <param name="colRng"> column range with inclusive start and exclusive end indices [Int32] </param>
        /// <returns> sub-matrix </returns>
        public new MatrixZ this[Range rowRng, Range colRng]
        {
            get => this[new LongRange(rowRng, Rows), new LongRange(colRng, Cols)];
            set => this[new LongRange(rowRng, Rows), new LongRange(colRng, Cols)] = value;
        }


        #endregion

        #region constructors
        /// <summary>
        /// constructs a matrix with given length
        /// by default, does not initialize element values
        /// </summary>
        /// <param name="rows"> number of rows </param>
        /// <param name="cols"> number of columns </param>
        /// <param name="mode"> construct mode option; default is CALLOC </param>
        public MatrixZ(long rows, long cols,
            ArrayInitMode mode = ArrayInitMode.Calloc)
            : base(rows, cols, mode) { }

        /// <summary>
        /// constructs a matrix with given size
        /// and sets all elements to the same value 
        /// </summary>
        /// <param name="rows"> number of rows </param>
        /// <param name="cols"> number of cols </param>
        /// <param name="initVal"> initial value for all the elements </param>
        public MatrixZ(long rows, long cols, Cplx initVal)
            : base(Matx<Cplx>.Create(rows, cols, initVal)) { }

        /// <summary>
        /// constructs a matrix with given size
        /// and sets all elements to the same value 
        /// </summary>
        /// <param name="rows"> number of rows </param>
        /// <param name="cols"> number of cols </param>
        /// <param name="initVal"> initial value for all the elements </param>
        /// <param name="loopMode"> loop-computation mode option </param>
        public MatrixZ(long rows, long cols, double initVal) :
            base(Matx<Cplx>.Create(rows, cols, new Cplx(initVal, 0.0)))
        { }

        /// <summary>
        /// constructs a matrix by copying from another
        /// </summary>
        /// <param name="other"> another matrix </param>
        /// <param name="deepCopy"> copy mode option; default is deep copy </param>
        public MatrixZ(MatrixZ other, bool deepCopy = true)
            : base((Matx<Cplx>)other, deepCopy ? ArrayCopyMode.Deep : ArrayCopyMode.Shallow) { }

        /// <summary>
        /// Initializes a new instance of the MatrixD class by copying the elements from another Matx<Real> instance.
        /// </summary>
        /// <param name="other"> another matrix </param>
        /// <param name="deepCopy"> copy mode option; default is deep copy </param>
        public MatrixZ(Matx<Cplx> other, bool deepCopy = true)
            : base(other, deepCopy ? ArrayCopyMode.Deep : ArrayCopyMode.Shallow) { }

        /// <summary>
        /// constructs a matrix by copying from a given ArrayBase
        /// </summary>
        /// <param name="other"> another ArrayBase </param>
        /// <param name="rows"> number of rows </param>
        /// <param name="cols"> number of columns </param>
        /// <param name="deepCopy"> copy mode option; default is deep copy </param>
        public MatrixZ(DenseArray<Cplx> other, long rows, long cols, bool deepCopy = true)
            : base(rows, cols, deepCopy ? ArrayInitMode.Malloc : ArrayInitMode.None)
        { }

        /// <summary>
        /// constructs a complex matrix 
        /// with its real or imaginary part only
        /// </summary>
        /// <param name="part"> part of the matrix </param>
        /// <param name="option"> option for the complex part; default is real-part </param>
        public MatrixZ(MatrixD part, ComplexPart option = ComplexPart.RealPart)
            : this(part.Rows, part.Cols, 0.0)
        {
            switch (option)
            {
                case ComplexPart.RealPart:
                    {
                        MatrixZ t = this;
                        VMath.Modify(part, t, ComplexPart.RealPart);
                        break;
                    }
                case ComplexPart.ImagPart:
                    {
                        MatrixZ t = this;
                        VMath.Modify(part, t, ComplexPart.ImagPart);
                        break;
                    }
                default: goto case ComplexPart.RealPart;
            }
        }

        #endregion

        #region methods

        /// <summary>
        /// padding according to target matrix parameters
        /// </summary>
        /// <param name="targetRows"> target number of rows in the padded matrix </param>
        /// <param name="targetCols"> target number of columns in the padded matrix </param>
        /// <param name="startRowIndex"> starting row index in the padded matrix </param>
        /// <param name="startColIndex"> starting column index in the padded matrix </param>
        /// <param name="paddingValueRe"> real-part of the padding value </param>
        /// <param name="paddingValueIm"> imag-part of the padding value </param>
        /// <returns> result matrix after padding </returns>
        public MatrixZ Padding(long targetRows, long targetCols,
            long startRowIndex, long startColIndex,
            double paddingValueRe = 0.0, double paddingValueIm = 0.0)
        {
            if (targetRows <= Rows)
            {
                Printer.Warning($"{nameof(targetRows)} must be greater than the current value");
                return Empty;
            }
            if (targetCols <= Cols)
            {
                Printer.Warning($"{nameof(targetCols)} must be greater than the current value");
                return Empty;
            }

            Cplx paddingValue = new(paddingValueRe, paddingValueIm);
            MatrixZ y = new(targetRows, targetCols, paddingValue);
            LongRange rowRng = new(startRowIndex, startRowIndex + Rows);
            LongRange colRng = new(startColIndex, startColIndex + Cols);
            y[rowRng, colRng] = this;

            return y;
        }

        /// <summary>
        /// centered zero-padding around each side
        /// </summary>
        /// <param name="targetRows"> target number of rows </param>
        /// <param name="targetCols"> target number of columns </param>
        /// <returns> result matrix after padding </returns>
        public MatrixZ Padding(long targetRows, long targetCols)
        {
            if ((targetRows - Rows) % 2 != 0)
            {
                Printer.Warning($"{nameof(targetRows)} must be an even addition to the current value");
                return Empty;
            }
            if ((targetCols - Cols) % 2 != 0)
            {
                Printer.Warning($"{nameof(targetCols)} must be an even addition to the current value");
                return Empty;
            }
            return Padding(targetRows, targetCols, (targetRows - Rows) / 2, (targetCols - Cols) / 2, 0.0, 0.0);
        }

        /// <summary>
        /// truncates current matrix according to target parameters
        /// </summary>
        /// <param name="targetRows"> target number of rows in the truncated matrix </param>
        /// <param name="targetCols"> target number of columns in the truncated matrix </param>
        /// <param name="startRowIndex"> starting row index in the original matrix </param>
        /// <param name="startColIndex"> starting column index in the original matrix </param>
        /// <returns> result matrix after truncation </returns>
        public MatrixZ Truncate(long targetRows, long targetCols,
            long startRowIndex, long startColIndex)
        {
            if (startRowIndex + targetRows >= Rows)
            {
                Printer.Warning($"invalid combination of parameters {nameof(targetRows)} and {nameof(startRowIndex)}");
                return Empty;
            }
            if (startColIndex + targetCols >= Cols)
            {
                Printer.Warning($"invalid combination of parameters {nameof(targetCols)} and {nameof(startColIndex)}");
                return Empty;
            }

            LongRange rowRng = new(startRowIndex, startRowIndex + targetRows);
            LongRange colRng = new(startColIndex, startColIndex + targetCols);
            return this[rowRng, colRng];
        }

        /// <summary>
        /// centered truncation on each side of the matrix
        /// </summary>
        /// <param name="targetRows"> target number of rows </param>
        /// <param name="targetCols"> target number of columns </param>
        /// <returns> result matrix after truncation </returns>
        public MatrixZ Truncate(long targetRows, long targetCols)
        {
            if ((Rows - targetRows) % 2 != 0)
            {
                Printer.Warning($"{nameof(targetRows)} must be an even subtraction of the current value");
                return Empty;
            }
            if ((Cols - targetCols) % 2 != 0)
            {
                Printer.Warning($"{nameof(targetCols)} must be an even subtraction of the current value");
                return Empty;
            }
            return Truncate(targetRows, targetCols, (Rows - targetRows) / 2, (Cols - targetCols) / 2);
        }

        /// <summary>
        /// replicates according to target number of rows and columns
        /// </summary>
        /// <param name="targetRows"> target number of rows </param>
        /// <param name="targetCols"> target number of columns </param>
        /// <returns> replicated result </returns>
        public MatrixZ Replicate(long targetRows, long targetCols)
        {
            if (targetRows <= Rows) { Printer.Warning($"Target number of rows not greater than the current"); }
            if (targetCols <= Cols) { Printer.Warning($"Target number of columns not greater than the current"); }
            MatrixZ rp = new(targetRows, targetCols);
            // computes replication multiples and residual counts
            long mRow = (long)(targetRows / Rows);
            long mCol = (long)(targetCols / Cols);
            long restRows = targetRows - mRow * Rows;
            long restCols = targetCols = mCol * Cols;
            // loop for multiple replications
            for (long nRow = 0; nRow < mRow; nRow++)
            {
                for (long nCol = 0; nCol < mCol; nCol++)
                {
                    LongRange uRow = new(nRow * Rows + 0, nRow * Rows + Rows);
                    LongRange uCol = new(nCol * Cols + 0, nCol * Cols + Cols);
                    rp[uRow, uCol] = this;
                }
            }
            // loop for the residuals
            for (long iRow = 0; iRow < restRows; iRow++)
            {
                for (long iCol = 0; iCol < restCols; iCol++)
                {
                    rp[mRow * Rows + iRow, mCol * Cols + iCol, false] = this[iRow, iCol, false];
                }
            }

            return rp;
        }

        #endregion

        #region operators

        #region m-m add [complex]
        /// <summary>
        /// computes the sum of two matrices x and y
        /// res[i,j] = x[i,j] + y[i,j]
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <param name="y"> input matrix y </param>
        /// <returns> result matrix </returns>
        public static MatrixZ operator +(MatrixZ x, MatrixZ y)
            => new(VMath.Operators.Add(x, y), false);
        #endregion
        #region m-m add [mixed]
        /// <summary>
        /// computes the sum of two matrices x and y
        /// res[i,j] = x[i,j] + y[i,j]
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <param name="y"> input matrix y </param>
        /// <returns> result matrix </returns>
        public static MatrixZ operator +(MatrixZ x, MatrixD y)
            => x + new MatrixZ(y);

        /// <summary>
        /// computes the sum of two matrices x and y
        /// res[i,j] = x[i,j] + y[i,j]
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <param name="y"> input matrix y </param>
        /// <returns> result matrix </returns>
        public static MatrixZ operator +(MatrixD x, MatrixZ y)
            => y + new MatrixZ(x);
        #endregion
        #region m-s add [complex]
        /// <summary>
        /// adds a scalar s to matrix x
        /// res[i,j] = x[i,j] + s
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <param name="s"> input scalar s </param>
        /// <returns> result matrix </returns>
        public static MatrixZ operator +(MatrixZ x, Cplx s)
            => new(VMath.Operators.Add(x, s));

        /// <summary>
        /// adds a scalar s to matrix x
        /// res[i,j] = x[i,j] + s
        /// </summary>
        /// <param name="s"> input scalar s </param>
        /// <param name="x"> input matrix x </param>
        /// <returns> result matrix </returns>
        public static MatrixZ operator +(Cplx s, MatrixZ x)
            => (x + s);
        #endregion
        #region m-s add [mixed]
        /// <summary>
        /// adds a scalar s to matrix x
        /// res[i,j] = x[i,j] + s
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <param name="s"> input scalar s </param>
        /// <returns> result matrix </returns>
        public static MatrixZ operator +(MatrixZ x, double s)
            => x + new Cplx(s, 0.0);

        /// <summary>
        /// adds a scalar s to matrix x
        /// res[i,j] = x[i,j] + s
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <param name="s"> input scalar s </param>
        /// <returns> result matrix </returns>
        public static MatrixZ operator +(double s, MatrixZ x)
            => x + s;
        #endregion
        #region m-m subtract [complex]
        /// <summary>
        /// subtracts one matrix y from another matrix x
        /// res[i,j] = x[i,j] - y[i,j]
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <param name="y"> input matrix y </param>
        /// <returns> result matrix </returns>
        public static MatrixZ operator -(MatrixZ x, MatrixZ y)
            => new(VMath.Operators.Sub(x, y), false);
        #endregion
        #region m-m subtract [mixed]
        /// <summary>
        /// subtracts one matrix y from another matrix x
        /// res[i,j] = x[i,j] - y[i,j]
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <param name="y"> input matrix y </param>
        /// <returns> result matrix </returns>
        public static MatrixZ operator -(MatrixZ x, MatrixD y)
            => x - new MatrixZ(y);

        /// <summary>
        /// substracts one matrix y from another matrix x
        /// res[i,j] = x[i,j] - y[i,j]
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <param name="y"> input matrix y </param>
        /// <returns> result matrix </returns>
        public static MatrixZ operator -(MatrixD x, MatrixZ y)
            => new MatrixZ(x) - y;
        #endregion
        #region m-s subtract [complex]
        /// <summary>
        /// subtracts a scalar s from matrix x
        /// res[i,j] = x[i,j] - s
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <param name="s"> input scalar s </param>
        /// <returns> result matrix </returns>
        public static MatrixZ operator -(MatrixZ x, Cplx s)
            => new(VMath.Operators.Sub(x, s));

        /// <summary>
        /// subtracts each element of matrix x from a scalar s
        /// res[i,j] = s - x[i,j]
        /// </summary>
        /// <param name="s"> input scalar s </param>
        /// <param name="x"> input matrix x </param>
        /// <returns> result matrix </returns>
        public static MatrixZ operator -(Cplx s, MatrixZ x)
            => s + (-x);
        #endregion
        #region m-s subtract [mixed]
        /// <summary>
        /// subtracts a scalar s from matrix x
        /// res[i,j] = x[i,j] - s
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <param name="s"> input scalar s </param>
        /// <returns> result matrix </returns>
        public static MatrixZ operator -(MatrixZ x, double s)
            => x - new Cplx(s, 0.0);

        /// <summary>
        /// subtracts each element of matrix x from a scalar s
        /// res[i,j] = s - x[i,j]
        /// </summary>
        /// <param name="s"> input scalar s </param>
        /// <param name="x"> input matrix x </param>
        /// <returns> result matrix </returns>
        public static MatrixZ operator -(double s, MatrixZ x)
            => new Cplx(s, 0.0) - x;
        #endregion
        #region m-m multiply [complex]
        /// <summary>
        /// performs element by element multiplication 
        /// of two matrices x and y
        /// res[i,j] = x[i,j] * y[i,j]
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <param name="y"> input matrix y </param>
        /// <returns> result matrix </returns>
        public static MatrixZ operator *(MatrixZ x, MatrixZ y)
            => new(VMath.Operators.Mul(x, y));
        #endregion
        #region m-m multiply [mixed]
        /// <summary>
        /// performs element by element multiplication 
        /// of two matrices x and y
        /// res[i,j] = x[i,j] * y[i,j]
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <param name="y"> input matrix y </param>
        /// <returns> result matrix </returns>
        public static MatrixZ operator *(MatrixZ x, MatrixD y)
            => x * new MatrixZ(y);

        /// <summary>
        /// performs element by element multiplication 
        /// of two matrices x and y
        /// res[i,j] = x[i,j] * y[i,j]
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <param name="y"> input matrix y </param>
        /// <returns> result matrix </returns>
        public static MatrixZ operator *(MatrixD x, MatrixZ y)
            => y * x;
        #endregion
        #region m-s multiply [complex]
        /// <summary>
        /// multiplies a matrix x with a scalar s
        /// res[i,j] = x[i,j] * s
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <param name="s"> input scalar s </param>
        /// <returns> result matrix </returns>
        public static MatrixZ operator *(MatrixZ x, Cplx s)
        => new(VMath.Operators.Mul(s, x));

        /// <summary>
        /// multiplies a scalar s with a matrix x
        /// res[i,j] = s * x[i,j]
        /// </summary>
        /// <param name="s"> input scalar s </param>
        /// <param name="x"> input matrix x </param>
        /// <returns> result matrix </returns>
        public static MatrixZ operator *(Cplx s, MatrixZ x)
            => (x * s);
        #endregion
        #region m-s multiply [mixed]
        /// <summary>
        /// multiplies a matrix x with a scalar s
        /// res[i,j] = x[i,j] * s
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <param name="s"> input scalar s </param>
        /// <returns> result matrix </returns>
        public static MatrixZ operator *(MatrixZ x, double s)
            => x * new Cplx(s, 0.0);

        /// <summary>
        /// multiplies a scalar s with a matrix x
        /// res[i,j] = s * x[i,j]
        /// </summary>
        /// <param name="s"> input scalar s </param>
        /// <param name="x"> input matrix x </param>
        /// <returns> result matrix </returns>
        public static MatrixZ operator *(double s, MatrixZ x)
            => x * s;
        #endregion
        #region m-m divide [complex]
        /// <summary>
        /// performs element by element inversion 
        /// of two matrices x and y
        /// res[i,j] = x[i,j] / y[i,j]
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <param name="y"> input matrix y </param>
        /// <returns> result matrix </returns>
        public static MatrixZ operator /(MatrixZ x, MatrixZ y)
            => new(VMath.Operators.Div(x, y));
        #endregion
        #region m-m divide [mixed]
        /// <summary>
        /// performs element by element inversion 
        /// of two matrices x and y
        /// res[i,j] = x[i,j] / y[i,j]
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <param name="y"> input matrix y </param>
        /// <returns> result matrix </returns>
        public static MatrixZ operator /(MatrixZ x, MatrixD y)
            => x / new MatrixZ(y);

        /// <summary>
        /// performs element by element inversion 
        /// of two matrices x and y
        /// res[i,j] = x[i,j] / y[i,j]
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <param name="y"> input matrix y </param>
        /// <returns> result matrix </returns>
        public static MatrixZ operator /(MatrixD x, MatrixZ y)
            => new MatrixZ(x) / y;
        #endregion
        #region m-s divide [complex]
        /// <summary>
        /// divides a matrix x by scalar s
        /// res[i,j] = x[i,j] / s
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <param name="s"> input scalar s </param>
        /// <returns> result matrix </returns>
        public static MatrixZ operator /(MatrixZ x, Cplx s)
        => new(VMath.Operators.Div(x, s));

        /// <summary>
        /// divides a scalar s by matrix x
        /// res[i,j] = s / x[i,j]
        /// </summary>
        /// <param name="s"> input scalar s </param>
        /// <param name="x"> input matrix x </param>
        /// <returns> result matrix </returns>
        public static MatrixZ operator /(Cplx s, MatrixZ x)
        => new(VMath.Operators.Div(s, x));

        #endregion
        #region m-s divide [mixed]
        /// <summary>
        /// divides a matrix x by scalar s
        /// res[i,j] = x[i,j] / s
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <param name="s"> input scalar s </param>
        /// <returns> result matrix </returns>
        public static MatrixZ operator /(MatrixZ x, double s)
            => x / new Cplx(s, 0.0);

        /// <summary>
        /// divides a scalar s by matrix x
        /// res[i,j] = s / x[i,j]
        /// </summary>
        /// <param name="s"> input scalar s </param>
        /// <param name="x"> input matrix x </param>
        /// <returns> result matrix </returns>
        public static MatrixZ operator /(double s, MatrixZ x)
            => new Cplx(s, 0.0) / x;
        #endregion
        #region negative
        /// <summary>
        /// takes the negative of the input matrix
        /// </summary>
        /// <param name="x"> input matrix x </param>
        /// <returns> result negative matrix </returns>
        public static MatrixZ operator -(MatrixZ x)
        => new(VMath.Operators.Neg(x), false);
        //=> VMath.Scale(x, -1.0);
        #endregion
        #endregion
    }
}
