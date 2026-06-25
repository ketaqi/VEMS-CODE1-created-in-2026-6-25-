using Complex = System.Numerics.Complex;

namespace VEMS.MathCore
{
    /// <summary>
    /// managed VMF using parallel for from C sharp
    /// </summary>
    public class CSharpMKL //: IVMF, IBLAS
    {
        /// <summary>
        /// internally stored loop mode option
        /// </summary>
        private LoopMode Mode { get; set; }

        /// <summary>
        /// constructs a C# math kernel library
        /// </summary>
        /// <param name="mode"> loop-computational mode option </param>
        public CSharpMKL(LoopMode mode = LoopMode.Sequential)
        {
            Mode = mode;
        }

        #region BLAS

        #region --------- Asum ---------

        /// <summary>
        /// computes the sum of magnitudes of the elements
        /// </summary>
        /// <param name="n"> number of array elements </param>
        /// <param name="x"> array x </param>
        /// <param name="incx"> increment for indexing x </param>
        /// <param name="startx"> starting index in x </param>
        /// <returns> sum of the elements magnitudes </returns>
        public unsafe double Asum(long n, DenseArrayBase<double> x,
            long incx = 1, long startx = 0)
        {
            double* px = (double*)x.DataPtr.ToPointer() + startx;
            
            // initialization
            double asum = 0.0;
            // defines loop operation
            Action<long> op = (i) =>
            {
                double xi = *(px + i * incx);
                asum += Math.Abs(xi);
            };
            Loop1D loop = new(operation: op, start: 0, end: n, step: 1);
            loop.Evaluate(mode: Mode);
            
            // return
            return asum;
        }

        /// <summary>
        /// computes the sum of magnitudes of the elements
        /// </summary>
        /// <param name="n"> number of array elements </param>
        /// <param name="x"> array x </param>
        /// <param name="incx"> increment for indexing x </param>
        /// <param name="startx"> starting index in x </param>
        /// <returns> sum of the elements magnitudes </returns>
        public unsafe double Asum(long n, DenseArrayBase<Complex> x,
            long incx = 1, long startx = 0)
        {
            Complex* px = (Complex*)x.DataPtr.ToPointer() + startx;

            // initialization
            double asum = 0.0;
            // defines loop operation
            Action<long> op = (i) =>
            {
                Complex xi = *(px + i * incx);
                asum += Complex.Abs(xi);
            };
            Loop1D loop = new(operation: op, start: 0, end: n, step: 1);
            loop.Evaluate(mode: Mode);

            // return
            return asum;
        }

        #endregion
        #region --------- Axpy ---------

        /// <summary>
        /// computes a scalar-array product and 
        /// adds the result to another array
        /// y := a*x + y
        /// </summary>
        /// <param name="n"> number of array elements </param>
        /// <param name="a"> scalar a </param>
        /// <param name="x"> array x </param>
        /// <param name="y"> array y </param>
        /// <param name="incx"> increment for indexing x </param>
        /// <param name="incy"> increment for indexing y </param>
        /// <param name="startx"> starting index in x </param>
        /// <param name="starty"> starting index in y </param>
        public unsafe void Axpy(long n, double a,
            DenseArrayBase<double> x, DenseArrayBase<double> y,
            long incx = 1, long incy = 1,
            long startx = 0, long starty = 0)
        {
            double* px = (double*)x.DataPtr.ToPointer() + startx;
            double* py = (double*)y.DataPtr.ToPointer() + starty;

            // defines loop operation
            Action<long> op = (i) =>
            {
                double xi = *(px + i * incx);
                double yi = *(py + i * incy);
                *(py + i * incy) = a * xi + yi;
            };
            Loop1D loop = new(operation: op, start: 0, end: n, step: 1);
            loop.Evaluate(mode: Mode);
        }

        /// <summary>
        /// computes a scalar-array product and 
        /// adds the result to another array
		/// y := a*x + y
        /// </summary>
        /// <param name="n"> number of array elements </param>
        /// <param name="a"> scalar a </param>
        /// <param name="x"> array x </param>
        /// <param name="y"> array y </param>
        /// <param name="incx"> increment for indexing x </param>
        /// <param name="incy"> increment for indexing y </param>
        /// <param name="startx"> starting index in x </param>
        /// <param name="starty"> starting index in y </param>
        public unsafe void Axpy(long n, Complex a,
            DenseArrayBase<Complex> x, DenseArrayBase<Complex> y,
            long incx = 1, long incy = 1,
            long startx = 0, long starty = 0)
        {
            Complex* px = (Complex*)x.DataPtr.ToPointer() + startx;
            Complex* py = (Complex*)y.DataPtr.ToPointer() + starty;

            // defines loop operation
            Action<long> op = (i) =>
            {
                Complex xi = *(px + i * incx);
                Complex yi = *(py + i * incy);
                *(py + i * incy) = a * xi + yi;
            };
            Loop1D loop = new(operation: op, start: 0, end: n, step: 1);
            loop.Evaluate(mode: Mode);
        }

        #endregion
        #region --------- Copy ---------

        /// <summary>
        /// copies x to y
        /// </summary>
        /// <param name="n"> number of elements </param>
        /// <param name="x"> array x </param>
        /// <param name="y"> array y </param>
        /// <param name="incx"> increment for indexing x </param>
        /// <param name="incy"> increment for indexing y </param>
        /// <param name="startx"> starting index in x </param>
        /// <param name="starty"> starting index in y </param>
        public unsafe void Copy(long n,
            DenseArrayBase<double> x, DenseArrayBase<double> y,
            long incx = 1, long incy = 1,
            long startx = 0, long starty = 0)
        {
            double* px = (double*)x.DataPtr.ToPointer() + startx;
            double* py = (double*)y.DataPtr.ToPointer() + starty;

            // defines loop operation
            Action<long> op = (i) =>
            {
                *(py + i * incy) = *(px + i * incx);
            };
            Loop1D loop = new(operation: op, start: 0, end: n, step: 1);
            loop.Evaluate(mode: Mode);
        }

        /// <summary>
        /// copies x to y
        /// </summary>
        /// <param name="n"> number of elements </param>
        /// <param name="x"> array x </param>
        /// <param name="y"> array y </param>
        /// <param name="incx"> increment for indexing x </param>
        /// <param name="incy"> increment for indexing y </param>
        /// <param name="startx"> starting index in x </param>
        /// <param name="starty"> starting index in y </param>
        public unsafe void Copy(long n,
            DenseArrayBase<Complex> x, DenseArrayBase<Complex> y,
            long incx = 1, long incy = 1,
            long startx = 0, long starty = 0)
        {
            Complex* px = (Complex*)x.DataPtr.ToPointer() + startx;
            Complex* py = (Complex*)y.DataPtr.ToPointer() + starty;

            // defines loop operation
            Action<long> op = (i) =>
            {
                *(py + i * incy) = *(px + i * incx);
            };
            Loop1D loop = new(operation: op, start: 0, end: n, step: 1);
            loop.Evaluate(mode: Mode);
        }

        #endregion
        #region --------- Dot ---------

        /// <summary>
        /// computes a vector-vector dot product
        /// </summary>
        /// <param name="n"> number of elements in x </param>
        /// <param name="x"> array x </param>
        /// <param name="y"> array y </param>
        /// <param name="incx"> increment for indexing x </param>
        /// <param name="incy"> increment for indexing y </param>
        /// <param name="startx"> starting index in x </param>
        /// <param name="starty"> starting index in y </param>
        /// <returns> result of the dot product of x and y </returns>
        public unsafe double Dot(long n,
            DenseArrayBase<double> x, DenseArrayBase<double> y,
            long incx = 1, long incy = 1,
            long startx = 0, long starty = 0)
        {
            double* px = (double*)x.DataPtr.ToPointer() + startx;
            double* py = (double*)y.DataPtr.ToPointer() + starty;

            // initialization
            double dot = 0.0;
            // defines loop operation
            Action<long> op = (i) =>
            {
                double xi = *(px + i * incx);
                double yi = *(py + i * incy);
                dot += xi * yi;
            };
            Loop1D loop = new(operation: op, start: 0, end: n, step: 1);
            loop.Evaluate(mode: Mode);

            // return
            return dot;
        }

        /// <summary>
        /// computes a vector-vector dot product
        /// </summary>
        /// <param name="n"> number of elements in x </param>
        /// <param name="x"> array x </param>
        /// <param name="y"> array y </param>
        /// <param name="incx"> increment for indexing x </param>
        /// <param name="incy"> increment for indexing y </param>
        /// <param name="startx"> starting index in x </param>
        /// <param name="starty"> starting index in y </param>
        /// <returns> result of the dot product of x and y </returns>
        public unsafe Complex Dot(long n,
            DenseArrayBase<Complex> x, DenseArrayBase<Complex> y,
            long incx = 1, long incy = 1,
            long startx = 0, long starty = 0)
        {
            Complex* px = (Complex*)x.DataPtr.ToPointer() + startx;
            Complex* py = (Complex*)y.DataPtr.ToPointer() + starty;

            // initialization
            Complex dotu = 0.0;
            // defines loop operation
            Action<long> op = (i) =>
            {
                Complex xi = *(px + i * incx);
                Complex yi = *(py + i * incy);
                dotu += xi * yi;
            };
            Loop1D loop = new(operation: op, start: 0, end: n, step: 1);
            loop.Evaluate(mode: Mode);

            // return
            return dotu;
        }

        #endregion
        #region --------- Dotc ---------

        /// <summary>
        /// computes a dot product of a conjugated vector with another vector
        /// </summary>
        /// <param name="n"> number of elements in x </param>
        /// <param name="x"> array x (to be conjugated) </param>
        /// <param name="y"> array y </param>
        /// <param name="incx"> increment for indexing x </param>
        /// <param name="incy"> increment for indexing y </param>
        /// <param name="startx"> starting index in x </param>
        /// <param name="starty"> starting index in y </param>
        /// <returns> result of the conjugate product </returns>
        public unsafe Complex Dotc(long n,
            DenseArrayBase<Complex> x, DenseArrayBase<Complex> y,
            long incx = 1, long incy = 1,
            long startx = 0, long starty = 0)
        {
            Complex* px = (Complex*)x.DataPtr.ToPointer() + startx;
            Complex* py = (Complex*)y.DataPtr.ToPointer() + starty;
            
            // initialization
            Complex dotc = 0.0;
            // defines loop operation
            Action<long> op = (i) =>
            {
                Complex xi = *(px + i * incx);
                Complex yi = *(py + i * incy);
                dotc += Complex.Conjugate(xi) * yi;
            };
            Loop1D loop = new(operation: op, start: 0, end: n, step: 1);
            loop.Evaluate(mode: Mode);

            // return
            return dotc;
        }

        #endregion
        #region --------- Nrm2 ---------

        /// <summary>
        /// computes the Euclidean norm of an array
        /// </summary>
        /// <param name="n"> number of elements in x </param>
        /// <param name="x"> array x </param>
        /// <param name="incx"> increment for indexing x </param>
        /// <param name="startx"> starting index in x </param>
        /// <returns> Euclidean norm </returns>
        public unsafe double Nrm2(long n, DenseArrayBase<double> x,
            long incx = 1, long startx = 0)
        {
            double* px = (double*)x.DataPtr.ToPointer() + startx;

            // initialization
            double n2 = 0.0;
            // defines loop operation
            Action<long> op = (i) =>
            {
                double xi = *(px + i * incx);
                n2 += xi * xi;
            };
            Loop1D loop = new (operation: op, start: 0, end: n, step: 1);
            loop.Evaluate(mode: Mode);

            // return
            return Math.Sqrt(n2);
        }

        /// <summary>
        /// computes the Euclidean norm of an array
        /// </summary>
        /// <param name="n"> number of elements in x </param>
        /// <param name="x"> array x </param>
        /// <param name="incx"> increment for indexing x </param>
        /// <param name="startx"> starting index in x </param>
        /// <returns> Euclidean norm </returns>
        public unsafe double Nrm2(long n, DenseArrayBase<Complex> x,
            long incx = 1, long startx = 0)
        {
            Complex* px = (Complex*)x.DataPtr.ToPointer() + startx;

            // initialization
            double n2 = 0.0;
            // defines loop operation
            Action<long> op = (i) =>
            {
                Complex xi = *(px + i * incx);
                double xia = Complex.Abs(xi);
                n2 += xia * xia;
            };
            Loop1D loop = new(operation: op, start: 0, end: n, step: 1);
            loop.Evaluate(mode: Mode);

            // return
            return Math.Sqrt(n2);
        }

        #endregion
        #region --------- Rot ---------

        /// <summary>
        /// performs rotation of points in the plane
        /// xi = c*xi + s*yi
        /// yi = c*yi - s*xi
        /// </summary>
        /// <param name="n"> number of elements in x </param>
        /// <param name="x"> array x </param>
        /// <param name="y"> array y </param>
        /// <param name="c"> scalar c </param>
        /// <param name="s"> scalar s </param>
        /// <param name="incx"> increment for indexing x </param>
        /// <param name="incy"> increment for indexing y </param>
        /// <param name="startx"> starting index in x </param>
        /// <param name="starty"> starting index in y </param>
        public unsafe void Rot(long n,
             DenseArrayBase<double> x, DenseArrayBase<double> y,
             double c, double s,
             long incx = 1, long incy = 1,
             long startx = 0, long starty = 0)
        {
            double* px = (double*)x.DataPtr.ToPointer() + startx;
            double* py = (double*)y.DataPtr.ToPointer() + starty;

            // defines loop operation
            Action<long> op = (i) =>
            {
                double xi = *(px + i * incx);
                double yi = *(py + i * incy);
                *(px + i * incx) = c * xi + s * yi;
                *(py + i * incy) = c * yi - s * xi;
            };
            Loop1D loop = new(operation: op, start: 0, end: n, step: 1);
            loop.Evaluate(mode: Mode);
        }

        /// <summary>
        /// performs rotation of points in the plane
        /// xi = c*xi + s*yi
        /// yi = c*yi - s*xi
        /// </summary>
        /// <param name="n"> number of elements in x </param>
        /// <param name="x"> array x </param>
        /// <param name="y"> array y </param>
        /// <param name="c"> scalar c </param>
        /// <param name="s"> scalar s </param>
        /// <param name="incx"> increment for indexing x </param>
        /// <param name="incy"> increment for indexing y </param>
        /// <param name="startx"> starting index in x </param>
        /// <param name="starty"> starting index in y </param>
        public unsafe void Rot(long n,
            DenseArrayBase<Complex> x, DenseArrayBase<Complex> y,
            double c, double s,
            long incx = 1, long incy = 1,
            long startx = 0, long starty = 0)
        {
            Complex* px = (Complex*)x.DataPtr.ToPointer() + startx;
            Complex* py = (Complex*)y.DataPtr.ToPointer() + starty;

            // defines loop operation
            Action<long> op = (i) =>
            {
                Complex xi = *(px + i * incx);
                Complex yi = *(py + i * incy);
                *(px + i * incx) = c * xi + s * yi;
                *(py + i * incy) = c * yi - s * xi;
            };
            Loop1D loop = new(operation: op, start: 0, end: n, step: 1);
            loop.Evaluate(mode: Mode);
        }

        #endregion
        #region --------- Scal ----------

        /// <summary>
        /// computes the product of an array by a scalar
        /// x = a*x 
        /// </summary>
        /// <param name="n"> number of elements in x </param>
        /// <param name="a"> scalar a </param>
        /// <param name="x"> array x x </param>
        /// <param name="incx"> increment for indexing x </param>
        /// <param name="startx"> starting index in x </param>
        public unsafe void Scal(long n, double a, DenseArrayBase<double> x,
            long incx = 1, long startx = 0)
        {
            double* px = (double*)x.DataPtr.ToPointer() + startx;

            // defines loop operation
            Action<long> op = (i) =>
            {
                double xi = *(px + i * incx);
                *(px + i * incx) = a * xi;
            };
            Loop1D loop = new(operation: op, start: 0, end: n, step: 1);
            loop.Evaluate(mode: Mode);
        }

        /// <summary>
        /// computes the product of an array by a scalar
        /// x = a*x 
        /// </summary>
        /// <param name="n"> number of elements in x </param>
        /// <param name="a"> scalar a </param>
        /// <param name="x"> array x x </param>
        /// <param name="incx"> increment for indexing x </param>
        /// <param name="startx"> starting index in x </param>
        public unsafe void Scal(long n, double a, DenseArrayBase<Complex> x,
            long incx = 1, long startx = 0)
        {
            Complex* px = (Complex*)x.DataPtr.ToPointer() + startx;

            // defines loop operation
            Action<long> op = (i) =>
            {
                Complex xi = *(px + i * incx);
                *(px + i * incx) = a * xi;
            };
            Loop1D loop = new(operation: op, start: 0, end: n, step: 1);
            loop.Evaluate(mode: Mode);
        }

        /// <summary>
        /// computes the product of an array by a scalar
        /// x = a*x 
        /// </summary>
        /// <param name="n"> number of elements in x </param>
        /// <param name="a"> scalar a </param>
        /// <param name="x"> array x x </param>
        /// <param name="incx"> increment for indexing x </param>
        /// <param name="startx"> starting index in x </param>
        public unsafe void Scal(long n, Complex a, DenseArrayBase<Complex> x,
            long incx = 1, long startx = 0)
        {
            Complex* px = (Complex*)x.DataPtr.ToPointer();

            // defines loop operation
            Action<long> op = (i) =>
            {
                Complex xi = *(px + i * incx);
                *(px + i * incx) = a * xi;
            };
            Loop1D loop = new(operation: op, start: 0, end: n, step: 1);
            loop.Evaluate(mode: Mode);
        }

        #endregion
        #region --------- Iamax ---------

        /// <summary>
        /// finds the index of the element with maximum absolute value
        /// </summary>
        /// <param name="n"> number of elements in x </param>
        /// <param name="x"> array x </param>
        /// <param name="incx"> increment for indexing x </param>
        /// <param name="startx"> starting index in x </param>
        /// <returns> index of the element with largest absolute value </returns>
        public unsafe long Iamax(long n, DenseArrayBase<double> x,
            long incx = 1, long startx = 0)
        {
            double* px = (double*)x.DataPtr.ToPointer() + startx;

            // initialization
            long iMax = 0;
            double amax = Math.Abs(*(px + iMax));
            // defines loop operation
            Action<long> op = (i) =>
            {
                double xi = *(px + i * incx);
                double xia = Math.Abs(xi);
                if (xia > amax)
                {
                    amax = xia;
                    iMax = i;
                }
            };
            Loop1D loop = new(operation: op, start: 0, end: n, step: 1);
            loop.Evaluate(mode: Mode);

            // return
            return iMax;
        }

        /// <summary>
        /// finds the index of the element with maximum absolute value
        /// </summary>
        /// <param name="n"> number of elements in x </param>
        /// <param name="x"> array x </param>
        /// <param name="incx"> increment for indexing x </param>
        /// <param name="startx"> starting index in x </param>
        /// <returns> index of the element with largest absolute value </returns>
        public unsafe long Iamax(long n, DenseArrayBase<Complex> x,
            long incx = 1, long startx = 0)
        {
            Complex* px = (Complex*)x.DataPtr.ToPointer() + startx;

            // initialization
            long iMax = 0;
            double amax = Complex.Abs(*(px + iMax));
            // defines loop operation
            Action<long> op = (i) =>
            {
                Complex xi = *(px + i * incx);
                double xia = Complex.Abs(xi);
                if (xia > amax)
                {
                    amax = xia;
                    iMax = i;
                }
            };
            Loop1D loop = new(operation: op, start: 0, end: n, step: 1);
            loop.Evaluate(mode: Mode);

            // return
            return iMax;
        }

        #endregion
        #region --------- Iamin ---------

        /// <summary>
        /// finds the index of the element with minimum absolute value
        /// </summary>
        /// <param name="n"> number of elements in x </param>
        /// <param name="x"> array x </param>
        /// <param name="incx"> increment for indexing x </param>
        /// <param name="startx"> starting index in x </param>
        /// <returns> index of the element with smallest absolute value </returns>
        public unsafe long Iamin(long n, DenseArrayBase<double> x,
            long incx = 1, long startx = 0)
        {
            double* px = (double*)x.DataPtr.ToPointer() + startx;

            // initialization
            long iMin = 0;
            double amin = Math.Abs(*(px + iMin));
            // defines loop operation
            Action<long> op = (i) =>
            {
                double xi = *(px + i * incx);
                double xia = Math.Abs(xi);
                if (xia < amin)
                {
                    amin = xia;
                    iMin = i;
                }
            };
            Loop1D loop = new(operation: op, start: 0, end: n, step: 1);
            loop.Evaluate(mode: Mode);

            // return
            return iMin;
        }

        /// <summary>
        /// finds the index of the element with minimum absolute value
        /// </summary>
        /// <param name="n"> number of elements in x </param>
        /// <param name="x"> array x </param>
        /// <param name="incx"> increment for indexing x </param>
        /// <param name="startx"> starting index in x </param>
        /// <returns> index of the element with smallest absolute value </returns>
        public unsafe long Iamin(long n, DenseArrayBase<Complex> x,
            long incx = 1, long startx = 0)
        {
            Complex* px = (Complex*)x.DataPtr.ToPointer() + startx;
            
            // initialization
            long iMin = 0;
            double amin = Complex.Abs(*(px + iMin));
            // defines loop operation
            Action<long> op = (i) =>
            {
                Complex xi = *(px + i * incx);
                double xia = Complex.Abs(xi);
                if (xia < amin)
                {
                    amin = xia;
                    iMin = i;
                }
            };
            Loop1D loop = new(operation: op, start: 0, end: n, step: 1);
            loop.Evaluate(mode: Mode);

            // return
            return iMin;
        }

        #endregion
        #region --------- Swap ---------

        /// <summary>
        /// given two arrays x and y, returns array y and x swapped
        /// </summary>
        /// <param name="n"> number of elements </param>
        /// <param name="x"> array x </param>
        /// <param name="y"> array y </param>
        /// <param name="incx"> increment for indexing x </param>
        /// <param name="incy"> increment for indexing y </param>
        /// <param name="startx"> starting index in x </param>
        /// <param name="starty"> starting index in y </param>
        public unsafe void Swap(long n,
            DenseArrayBase<double> x, DenseArrayBase<double> y,
            long incx = 1, long incy = 1,
            long startx = 0, long starty = 0)
        {
            double* px = (double*)x.DataPtr.ToPointer() + startx;
            double* py = (double*)y.DataPtr.ToPointer() + starty;

            // defines loop operation
            Action<long> op = (i) =>
            {
                double xi = *(px + i * incx);
                double yi = *(py + i * incy);
                *(px + i * incx) = yi;
                *(py + i * incy) = xi;
            };
            Loop1D loop = new(operation: op, start: 0, end: n, step: 1);
            loop.Evaluate(mode: Mode);
        }

        /// <summary>
        /// given two arrays x and y, returns array y and x swapped
        /// </summary>
        /// <param name="n"> number of elements </param>
        /// <param name="x"> array x </param>
        /// <param name="y"> array y </param>
        /// <param name="incx"> increment for indexing x </param>
        /// <param name="incy"> increment for indexing y </param>
        /// <param name="startx"> starting index in x </param>
        /// <param name="starty"> starting index in y </param>
        public unsafe void Swap(long n,
            DenseArrayBase<Complex> x, DenseArrayBase<Complex> y,
            long incx = 1, long incy = 1,
            long startx = 0, long starty = 0)
        {
            Complex* px = (Complex*)x.DataPtr.ToPointer() + startx;
            Complex* py = (Complex*)y.DataPtr.ToPointer() + starty;

            // defines loop operation
            Action<long> op = (i) =>
            {
                Complex xi = *(px + i * incx);
                Complex yi = *(py + i * incy);
                *(px + i * incx) = yi;
                *(py + i * incy) = xi;
            };
            Loop1D loop = new(operation: op, start: 0, end: n, step: 1);
            loop.Evaluate(mode: Mode);
        }

        #endregion
        #region --------- Gemv ---------

        /// <summary>
        /// computes a matrix-vector product using a general matrix
        /// y := alpha*a*x + beta*y, or
        /// y := alpha*a'*y + beta*y
        /// y := alpha*conj(a')*x + beta*y            
        /// </summary>
        /// <param name="layout"> specifies array storage: row- or column-major </param>
        /// <param name="trans"> specifies matrix a transpose operation </param>
        /// <param name="m"> number of rows of the matrix a </param>
        /// <param name="n"> number of columns of the matrix a </param>
        /// <param name="alpha"> scalar alpha </param>
        /// <param name="a"> array (matrix) a </param>
        /// <param name="lda"> leading dimension of a </param>
        /// <param name="x"> array (vector) x </param>
        /// <param name="y"> array (vector) y </param>
        /// <param name="beta"> scalar beta </param>
        /// <param name="incx"> increment for the elements of x </param>
        /// <param name="incy"> increment for the elements of y </param>
        /// <param name="starta"> starting index in a </param>
        /// <param name="startx"> starting index in x </param>
        /// <param name="starty"> starting index in y </param>
        public unsafe void Gemv(BLAS_Layout layout, BLAS_Transpose trans,
            long m, long n, double alpha, DenseArrayBase<double> a, long lda,
            DenseArrayBase<double> x, double beta, DenseArrayBase<double> y,
            long incx = 1, long incy = 1,
            long starta = 0, long startx = 0, long starty = 0)
        {
            double* pa = (double*)a.DataPtr.ToPointer() + starta;
            double* px = (double*)x.DataPtr.ToPointer() + startx;
            double* py = (double*)y.DataPtr.ToPointer() + starty;

            switch(trans)
            {
                case BLAS_Transpose.NoTrans:
                    {
                        // defines loop operation
                        Action<long> op = (iRow) =>
                        {
                            double t = 0.0;
                            for (long iCol = 0; iCol < n; iCol++)
                            {
                                double amn = *(pa + iRow * n + iCol);
                                double xn = *(px + iCol * incx);
                                t += amn * xn;
                            }
                            *(py + iRow * incy) = alpha * t;
                        };
                        Loop1D loop = new(operation: op, start: 0, end: m, step: 1);
                        loop.Evaluate(mode: Mode);

                        break;
                    }
                case BLAS_Transpose.Trans:
                    throw new NotImplementedException();
                case BLAS_Transpose.ConjTrans:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// computes a matrix-vector product using a general matrix
        /// y := alpha*a*x + beta*y, or
        /// y := alpha*a'*y + beta*y
        /// y := alpha*conj(a')*x + beta*y            
        /// </summary>
        /// <param name="layout"> specifies array storage: row- or column-major </param>
        /// <param name="trans"> specifies matrix a transpose operation </param>
        /// <param name="m"> number of rows of the matrix a </param>
        /// <param name="n"> number of columns of the matrix a </param>
        /// <param name="alpha"> scalar alpha </param>
        /// <param name="a"> array (matrix) a </param>
        /// <param name="lda"> leading dimension of a </param>
        /// <param name="x"> array (vector) x </param>
        /// <param name="y"> array (vector) y </param>
        /// <param name="beta"> scalar beta </param>
        /// <param name="incx"> increment for the elements of x </param>
        /// <param name="incy"> increment for the elements of y </param>
        /// <param name="starta"> starting index in a </param>
        /// <param name="startx"> starting index in x </param>
        /// <param name="starty"> starting index in y </param>
        public unsafe void Gemv(BLAS_Layout layout, BLAS_Transpose trans,
            long m, long n, Complex alpha, DenseArrayBase<Complex> a, long lda,
            DenseArrayBase<Complex> x, Complex beta, DenseArrayBase<Complex> y,
            long incx = 1, long incy = 1,
            long starta = 0, long startx = 0, long starty = 0)
        {
            Complex* pa = (Complex*)a.DataPtr.ToPointer() + starta;
            Complex* px = (Complex*)x.DataPtr.ToPointer() + startx;
            Complex* py = (Complex*)y.DataPtr.ToPointer() + starty;

            switch (trans)
            {
                case BLAS_Transpose.NoTrans:
                    {
                        // defines loop operation
                        Action<long> op = (iRow) =>
                        {
                            Complex t = 0.0;
                            for (long iCol = 0; iCol < n; iCol++)
                            {
                                Complex amn = *(pa + iRow * n + iCol);
                                Complex xn = *(px + iCol * incx);
                                t += amn * xn;
                            }
                            *(py + iRow * incy) = alpha * t;
                        };
                        Loop1D loop = new(operation: op, start: 0, end: m, step: 1);
                        loop.Evaluate(mode: Mode);

                        break;
                    }
                case BLAS_Transpose.Trans:
                    throw new NotImplementedException();
                case BLAS_Transpose.ConjTrans:
                    throw new NotImplementedException();
            }
        }

        #endregion
        #region --------- Gemm ---------

        /// <summary>
        /// computes a matrix-matrix product with general
        /// c := alpha*op(a)*op(b) + beta*c
        /// op(x) can be 1) identical; 2) transpose
        /// </summary>
        /// <param name="layout"> specifies array storage: row- or column-major </param>
        /// <param name="transa"> specifies matrix a transpose operation </param>
        /// <param name="transb"> specifies matrix b transpose operation </param>
        /// <param name="m"> number of rows of the matrix op(a) </param>
        /// <param name="n"> number of columns of the matrix op(b) </param>
        /// <param name="k"> number of columns of the matrix op(a) </param>
        /// <param name="alpha"> scalar alpha </param>
        /// <param name="a"> array (matrix) a </param>
        /// <param name="lda"> leading dimension of matrix a </param>
        /// <param name="b"> array (matrix) b </param>
        /// <param name="ldb"> leading dimension of matrix b</param>
        /// <param name="beta"> scalar beta </param>
        /// <param name="c"> array (matrix) c </param>
        /// <param name="ldc"> leading dimension of matrix c </param>
        /// <param name="starta"> starting index in a </param>
        /// <param name="startb"> starting index in b </param>
        /// <param name="startc"> starting index in c </param>
        public unsafe void Gemm(BLAS_Layout layout, BLAS_Transpose transa, BLAS_Transpose transb,
            long m, long n, long k, double alpha,
            DenseArrayBase<double> a, long lda,
            DenseArrayBase<double> b, long ldb, double beta,
            DenseArrayBase<double> c, long ldc,
            long starta = 0, long startb = 0, long startc = 0)
        {
            double* pa = (double*)a.DataPtr.ToPointer() + starta;
            double* pb = (double*)b.DataPtr.ToPointer() + startb;
            double* pc = (double*)c.DataPtr.ToPointer() + startc;

            // defines loop operation
            Action<long> op = (i) =>
            {
                long iRow = i / n;
                long iCol = i - iRow * n;
                double t = 0.0;
                for (long j = 0; j < k; j++)
                {
                    double aj = *(pa + iRow * k + j);
                    double bj = *(pb + j * n + iCol);
                    t += aj * bj;
                }
                double ct = *(pc + i);
                *(pc + i) = alpha * t + beta * ct;
            };
            Loop1D loop = new(operation: op, start: 0, end: m * n, step: 1);
            loop.Evaluate(mode: Mode);
        }

        /// <summary>
        /// computes a matrix-matrix product with general
        /// c := alpha*op(a)*op(b) + beta*c
        /// op(x) can be 1) identical; 2) transpose
        /// </summary>
        /// <param name="layout"> specifies array storage: row- or column-major </param>
        /// <param name="transa"> specifies matrix a transpose operation </param>
        /// <param name="transb"> specifies matrix b transpose operation </param>
        /// <param name="m"> number of rows of the matrix op(a) </param>
        /// <param name="n"> number of columns of the matrix op(b) </param>
        /// <param name="k"> number of columns of the matrix op(a) </param>
        /// <param name="alpha"> scalar alpha </param>
        /// <param name="a"> array (matrix) a </param>
        /// <param name="lda"> leading dimension of matrix a </param>
        /// <param name="b"> array (matrix) b </param>
        /// <param name="ldb"> leading dimension of matrix b</param>
        /// <param name="beta"> scalar beta </param>
        /// <param name="c"> array (matrix) c </param>
        /// <param name="ldc"> leading dimension of matrix c </param>
        /// <param name="starta"> starting index in a </param>
        /// <param name="startb"> starting index in b </param>
        /// <param name="startc"> starting index in c </param>
        public unsafe void Gemm(BLAS_Layout layout, BLAS_Transpose transa, BLAS_Transpose transb,
            long m, long n, long k, Complex alpha,
            DenseArrayBase<Complex> a, long lda,
            DenseArrayBase<Complex> b, long ldb, Complex beta,
            DenseArrayBase<Complex> c, long ldc,
            long starta = 0, long startb = 0, long startc = 0)
        {
            Complex* pa = (Complex*)a.DataPtr.ToPointer();
            Complex* pb = (Complex*)b.DataPtr.ToPointer();
            Complex* pc = (Complex*)c.DataPtr.ToPointer();

            // defines loop operation
            Action<long> op = (i) =>
            {
                long iRow = i / n;
                long iCol = i - iRow * n;
                Complex t = 0.0;
                for (long j = 0; j < k; j++)
                {
                    Complex aj = *(pa + iRow * k + j);
                    Complex bj = *(pb + j * n + iCol);
                    t += aj * bj;
                }
                Complex ct = *(pc + i);
                *(pc + i) = alpha * t + beta * ct;
            };
            Loop1D loop = new(operation: op, start: 0, end: m * n, step: 1);
            loop.Evaluate(mode: Mode);
        }

        #endregion

        #endregion
        #region BLAS-like extensions

        /// <summary>
        /// performs scaling and in-place transposition/copying of matrices
        /// </summary>
        /// <param name="layout"> layout of the input matrix </param>
        /// <param name="operation"> whether to transpose the matrix </param>
        /// <param name="rows"> number of rows before operation </param>
        /// <param name="cols"> number of columns before operation </param>
        /// <param name="alpha"> scaling factor alpha </param>
        /// <param name="ab"> array (matrix) ab - overwritten on exit </param>
        /// <param name="lda"> leading dimension of matrix before operation </param>
        /// <param name="ldb"> leading dimension of matrix after operation </param>
        /// <param name="startab"> starting index in ab </param>
        public unsafe void ImatCopy(BLAS_Layout layout, BLAS_Transpose operation,
            long rows, long cols,
            double alpha, DenseArrayBase<double> ab, long lda, long ldb,
            long startab = 0)
        {
            byte layoutChar = (byte)'R';
            byte operationChar = (byte)'N';
            switch (layout)
            {
                case BLAS_Layout.ColMajor:
                    layoutChar = (byte)'C';
                    break;
                default:
                    break;
            }
            switch (operation)
            {
                case BLAS_Transpose.Trans:
                    operationChar = (byte)'T';
                    break;
                case BLAS_Transpose.ConjTrans:
                    operationChar = (byte)'C';
                    break;
                default:
                    break;
            }
            double* pab = (double*)ab.DataPtr.ToPointer() + startab;
            IntelMKLNative.MKL_Dimatcopy(layoutChar, operationChar,
                rows, cols, alpha, pab, lda, ldb);
        }

        /// <summary>
        /// performs scaling and in-place transposition/copying of matrices
        /// </summary>
        /// <param name="layout"> layout of the input matrix </param>
        /// <param name="operation"> whether to transpose the matrix </param>
        /// <param name="rows"> number of rows before operation </param>
        /// <param name="cols"> number of columns before operation </param>
        /// <param name="alpha"> scaling factor alpha </param>
        /// <param name="ab"> array (matrix) ab - overwritten on exit </param>
        /// <param name="lda"> leading dimension of matrix before operation </param>
        /// <param name="ldb"> leading dimension of matrix after operation </param>
        /// <param name="startab"> starting index in ab </param>
        public unsafe void ImatCopy(BLAS_Layout layout, BLAS_Transpose operation,
            long rows, long cols,
            Complex alpha, DenseArrayBase<Complex> ab, long lda, long ldb,
            long startab = 0)
        {
            byte layoutChar = (byte)'R';
            byte operationChar = (byte)'N';
            switch (layout)
            {
                case BLAS_Layout.ColMajor:
                    layoutChar = (byte)'C';
                    break;
                default:
                    break;
            }
            switch (operation)
            {
                case BLAS_Transpose.Trans:
                    operationChar = (byte)'T';
                    break;
                case BLAS_Transpose.ConjTrans:
                    operationChar = (byte)'C';
                    break;
                default:
                    break;
            }
            Complex* pab = (Complex*)ab.DataPtr.ToPointer() + startab;
            IntelMKLNative.MKL_Zimatcopy(layoutChar, operationChar,
                rows, cols, &alpha, pab, lda, ldb);
        }

        /// <summary>
        /// performs scaling and out-place transposition/copying of matrices
        /// </summary>
        /// <param name="layout"> layout of the input matrix </param>
        /// <param name="operation"> whether to transpose the matrix </param>
        /// <param name="rows"> number of rows before operation </param>
        /// <param name="cols"> number of columns before operation </param>
        /// <param name="alpha"> scaling factor alpha </param>
        /// <param name="a"> array (matrix) a before operation </param>
        /// <param name="lda"> leading dimension of matrix before operation </param>
        /// <param name="b"> array (matrix) b after operation </param>
        /// <param name="ldb"> leading dimension of matrix after operation </param>
        /// <param name="starta"> starting index in a </param>
        /// <param name="startb"> starting index in b </param>
        public unsafe void OmatCopy(BLAS_Layout layout, BLAS_Transpose operation,
            long rows, long cols,
            double alpha, DenseArrayBase<double> a, long lda,
            DenseArrayBase<double> b, long ldb,
            long starta = 0, long startb = 0)
        {
            byte layoutChar = (byte)'R';
            byte operationChar = (byte)'N';
            switch (layout)
            {
                case BLAS_Layout.ColMajor:
                    layoutChar = (byte)'C';
                    break;
                default:
                    break;
            }
            switch (operation)
            {
                case BLAS_Transpose.Trans:
                    operationChar = (byte)'T';
                    break;
                case BLAS_Transpose.ConjTrans:
                    operationChar = (byte)'C';
                    break;
                default:
                    break;
            }
            double* pa = (double*)a.DataPtr.ToPointer() + starta;
            double* pb = (double*)b.DataPtr.ToPointer() + startb;
            IntelMKLNative.MKL_Domatcopy(layoutChar, operationChar,
                rows, cols, alpha, pa, lda, pb, ldb);
        }

        /// <summary>
        /// performs scaling and out-place transposition/copying of matrices
        /// </summary>
        /// <param name="layout"> layout of the input matrix </param>
        /// <param name="operation"> whether to transpose the matrix </param>
        /// <param name="rows"> number of rows before operation </param>
        /// <param name="cols"> number of columns before operation </param>
        /// <param name="alpha"> scaling factor alpha </param>
        /// <param name="a"> array (matrix) a before operation </param>
        /// <param name="lda"> leading dimension of matrix before operation </param>
        /// <param name="b"> array (matrix) b after operation </param>
        /// <param name="ldb"> leading dimension of matrix after operation </param>
        /// <param name="starta"> starting index in a </param>
        /// <param name="startb"> starting index in b </param>
        public unsafe void OmatCopy(BLAS_Layout layout, BLAS_Transpose operation,
            long rows, long cols,
            Complex alpha, DenseArrayBase<Complex> a, long lda,
            DenseArrayBase<Complex> b, long ldb,
            long starta = 0, long startb = 0)
        {
            byte layoutChar = (byte)'R';
            byte operationChar = (byte)'N';
            switch (layout)
            {
                case BLAS_Layout.ColMajor:
                    layoutChar = (byte)'C';
                    break;
                default:
                    break;
            }
            switch (operation)
            {
                case BLAS_Transpose.Trans:
                    operationChar = (byte)'T';
                    break;
                case BLAS_Transpose.ConjTrans:
                    operationChar = (byte)'C';
                    break;
                default:
                    break;
            }
            Complex* pa = (Complex*)a.DataPtr.ToPointer() + starta;
            Complex* pb = (Complex*)b.DataPtr.ToPointer() + startb;
            IntelMKLNative.MKL_Zomatcopy(layoutChar, operationChar,
                rows, cols, &alpha, pa, lda, pb, ldb);
        }

        #endregion
        #region VMF

        #region --------- Add ---------

        /// <summary>
        /// performs element by element addition of array a and array b
        /// y = a + b
        /// </summary>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="b"> input array b </param>
        /// <param name="y"> result array y </param>
        /// <param name="starta"> starting index in a </param>
        /// <param name="startb"> starting index in b </param>
        /// <param name="starty"> starting index in y </param>
        public unsafe void Add(long n, DenseArrayBase<double> a,
            DenseArrayBase<double> b, DenseArrayBase<double> y,
            long starta = 0, long startb = 0, long starty = 0)
        {
            double* pa = (double*)a.DataPtr.ToPointer() + starta;
            double* pb = (double*)b.DataPtr.ToPointer() + startb;
            double* py = (double*)y.DataPtr.ToPointer() + starty;

            // defines loop operation
            Action<long> op = (i) =>
            {
                double ia = *(pa + i);
                double ib = *(pb + i);
                *(py + i) = ia + ib;
            };
            Loop1D loop = new(operation: op, start: 0, end: n, step: 1);
            loop.Evaluate(mode: Mode);
        }

        /// <summary>
        /// performs element by element addition of array a and array b
        /// y = a + b
        /// </summary>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="b"> input array b </param>
        /// <param name="y"> result array y </param>
        /// <param name="starta"> starting index in a </param>
        /// <param name="startb"> starting index in b </param>
        /// <param name="starty"> starting index in y </param>
        public unsafe void Add(long n, DenseArrayBase<Complex> a,
            DenseArrayBase<Complex> b, DenseArrayBase<Complex> y,
            long starta = 0, long startb = 0, long starty = 0)
        {
            Complex* pa = (Complex*)a.DataPtr.ToPointer() + starta;
            Complex* pb = (Complex*)b.DataPtr.ToPointer() + startb;
            Complex* py = (Complex*)y.DataPtr.ToPointer() + starty;

            // defines loop operation
            Action<long> op = (i) =>
            {
                Complex ia = *(pa + i);
                Complex ib = *(pb + i);
                *(py + i) = ia + ib;
            };
            Loop1D loop = new(operation: op, start: 0, end: n, step: 1);
            loop.Evaluate(mode: Mode);
        }

        #endregion
        #region --------- Sub ---------

        /// <summary>
        /// performs element by element substraction of array b from array a
        /// y = a - b
        /// </summary>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="b"> input array b </param>
        /// <param name="y"> result array y </param>
        /// <param name="starta"> starting index in a </param>
        /// <param name="startb"> starting index in b </param>
        /// <param name="starty"> starting index in y </param>
        public unsafe void Sub(long n, DenseArrayBase<double> a,
            DenseArrayBase<double> b, DenseArrayBase<double> y,
            long starta = 0, long startb = 0, long starty = 0)
        {
            double* pa = (double*)a.DataPtr.ToPointer() + starta;
            double* pb = (double*)b.DataPtr.ToPointer() + startb;
            double* py = (Double*)y.DataPtr.ToPointer() + starty;

            // defines loop operation
            Action<long> op = (i) =>
            {
                double ia = *(pa + i);
                double ib = *(pb + i);
                *(py + i) = ia - ib;
            };
            Loop1D loop = new(operation: op, start: 0, end: n, step: 1);
            loop.Evaluate(mode: Mode);
        }

        /// <summary>
        /// performs element by element substraction of array b from array a
        /// y = a - b
        /// </summary>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="b"> input array b </param>
        /// <param name="y"> result array y </param>
        /// <param name="starta"> starting index in a </param>
        /// <param name="startb"> starting index in b </param>
        /// <param name="starty"> starting index in y </param>
        public unsafe void Sub(long n, DenseArrayBase<Complex> a,
            DenseArrayBase<Complex> b, DenseArrayBase<Complex> y,
            long starta = 0, long startb = 0, long starty = 0)
        {
            Complex* pa = (Complex*)a.DataPtr.ToPointer() + starta;
            Complex* pb = (Complex*)b.DataPtr.ToPointer() + startb;
            Complex* py = (Complex*)y.DataPtr.ToPointer() + starty;

            // defines loop operation
            Action<long> op = (i) =>
            {
                Complex ia = *(pa + i);
                Complex ib = *(pb + i);
                *(py + i) = ia - ib;
            };
            Loop1D loop = new(operation: op, start: 0, end: n, step: 1);
            loop.Evaluate(mode: Mode);

        }

        #endregion
        #region --------- Sqr ---------

        /// <summary>
        /// performs element by element squaring of the array
        /// </summary>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="y"> result array y </param>
        /// <param name="starta"> starting index in a </param>
        /// <param name="starty"> starting index in y </param>
        public unsafe void Sqr(long n,
            DenseArrayBase<double> a, DenseArrayBase<double> y,
            long starta = 0, long starty = 0)
        {
            double* pa = (double*)a.DataPtr.ToPointer() + starta;
            double* py = (double*)y.DataPtr.ToPointer() + starty;

            // defines loop operation
            Action<long> op = (i) =>
            {
                double ia = *(pa + i);
                *(py + i) = ia * ia;
            };
            Loop1D loop = new(operation: op, start: 0, end: n, step: 1);
            loop.Evaluate(mode: Mode);
        }

        /// <summary>
        /// performs element by element squaring of the array
        /// </summary>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="y"> result array y </param>
        public unsafe void Sqr(long n,
            DenseArrayBase<Complex> a,
            DenseArrayBase<Complex> y)
        {
            Complex* pa = (Complex*)a.DataPtr.ToPointer();
            Complex* py = (Complex*)y.DataPtr.ToPointer();

            // defines loop operation
            Action<long> op = (i) =>
            {
                Complex ia = *(pa + i);
                *(py + i) = ia * ia;
            };
            Loop1D loop = new(operation: op, start: 0, end: n, step: 1);
            loop.Evaluate(mode: Mode);
        }

        #endregion
        #region --------- Mul ---------

        /// <summary>
        /// performs element by element multiplication of array a and array b
        /// </summary>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="b"> input array b </param>
        /// <param name="y"> result array y </param>
        public unsafe void Mul(long n,
            DenseArrayBase<double> a, DenseArrayBase<double> b,
            DenseArrayBase<double> y)
        {
            double* pa = (double*)a.DataPtr.ToPointer();
            double* pb = (double*)b.DataPtr.ToPointer();
            double* py = (double*)y.DataPtr.ToPointer();

            // defines loop operation
            Action<long> op = (i) =>
            {
                double ia = *(pa + i);
                double ib = *(pb + i);
                *(py + i) = ia * ib;
            };
            Loop1D loop = new(operation: op, start: 0, end: n, step: 1);
            loop.Evaluate(mode: Mode);
        }

        /// <summary>
        /// performs element by element multiplication of array a and array b
        /// </summary>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="b"> input array b </param>
        /// <param name="y"> result array y </param>
        public unsafe void Mul(long n,
            DenseArrayBase<Complex> a, DenseArrayBase<Complex> b,
            DenseArrayBase<Complex> y)
        {
            Complex* pa = (Complex*)a.DataPtr.ToPointer();
            Complex* pb = (Complex*)b.DataPtr.ToPointer();
            Complex* py = (Complex*)y.DataPtr.ToPointer();

            // defines loop operation
            Action<long> op = (i) =>
            {
                Complex ia = *(pa + i);
                Complex ib = *(pb + i);
                *(py + i) = ia * ib;
            };
            Loop1D loop = new(operation: op, start: 0, end: n, step: 1);
            loop.Evaluate(mode: Mode);
        }

        #endregion
        #region --------- MulByConj ---------

        /// <summary>
        /// performs element by element multiplication of array a element 
        /// and conjugated array b element
        /// </summary>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="b"> input array b </param>
        /// <param name="y"> result array y </param>
        public unsafe void MulByConj(long n,
            DenseArrayBase<Complex> a, DenseArrayBase<Complex> b,
            DenseArrayBase<Complex> y)
        {
            Complex* pa = (Complex*)a.DataPtr.ToPointer();
            Complex* pb = (Complex*)b.DataPtr.ToPointer();
            Complex* py = (Complex*)y.DataPtr.ToPointer();

            // defines loop operation
            Action<long> op = (i) =>
            {
                Complex ia = *(pa + i);
                Complex ib = *(pb + i);
                ib = Complex.Conjugate(ib);
                *(py + i) = ia * ib;
            };
            Loop1D loop = new(operation: op, start: 0, end: n, step: 1);
            loop.Evaluate(mode: Mode);
        }

        #endregion
        #region --------- Conj ---------

        /// <summary>
        /// performs element by element conjugation of the array
        /// </summary>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="y"> result array y </param>
        public unsafe void Conj(long n,
            DenseArrayBase<Complex> a,
            DenseArrayBase<Complex> y)
        {
            Complex* pa = (Complex*)a.DataPtr.ToPointer();
            Complex* py = (Complex*)y.DataPtr.ToPointer();

            // defines loop operation
            Action<long> op = (i) =>
            {
                Complex ia = *(pa + i);
                *(py + i) = Complex.Conjugate(ia);
            };
            Loop1D loop = new(operation: op, start: 0, end: n, step: 1);
            loop.Evaluate(mode: Mode);
        }

        #endregion
        #region --------- Real/ImaginaryPart ---------

        /// <summary>
        /// takes the real part of each complex array element
        /// </summary>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="y"> result array y </param>
        public unsafe void RealPart(long n,
            DenseArrayBase<Complex> a,
            DenseArrayBase<double> y)
        {
            Complex* pa = (Complex*)a.DataPtr.ToPointer();
            double* py = (double*)y.DataPtr.ToPointer();

            // defines loop operation
            Action<long> op = (i) =>
            {
                Complex ia = *(pa + i);
                *(py + i) = ia.Real;
            };
            Loop1D loop = new(operation: op, start: 0, end: n, step: 1);
            loop.Evaluate(mode: Mode);
        }

        /// <summary>
        /// takes the imaginary part of each complex array element
        /// </summary>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="y"> result array y </param>
        public unsafe void ImaginaryPart(long n,
            DenseArrayBase<Complex> a,
            DenseArrayBase<double> y)
        {
            Complex* pa = (Complex*)a.DataPtr.ToPointer();
            double* py = (double*)y.DataPtr.ToPointer();

            // defines loop operation
            Action<long> op = (i) =>
            {
                Complex ia = *(pa + i);
                *(py + i) = ia.Imaginary;
            };
            Loop1D loop = new(operation: op, start: 0, end: n, step: 1);
            loop.Evaluate(mode: Mode);
        }

        /// <summary>
        /// takes the real and imaginary parts of each complex array element
        /// </summary>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="re"> result real part array </param>
        /// <param name="im"> result imaginary part array </param>
        public unsafe void RealImagParts(long n,
            DenseArrayBase<Complex> a,
            DenseArrayBase<double> re, DenseArrayBase<double> im)
        {
            Complex* pa = (Complex*)a.DataPtr.ToPointer();
            double* pre = (double*)re.DataPtr.ToPointer();
            double* pim = (double*)im.DataPtr.ToPointer();

            // defines loop operation
            Action<long> op = (i) =>
            {
                Complex ia = *(pa + i);
                *(pre + i) = ia.Real;
                *(pim + i) = ia.Imaginary;
            };
            Loop1D loop = new(operation: op, start: 0, end: n, step: 1);
            loop.Evaluate(mode: Mode);
        }

        #endregion
        #region --------- ModifyComplex ---------

        /// <summary>
        /// modifies the real and imaginary part of a complex array
        /// </summary>
        /// <param name="n"> number of elements </param>
        /// <param name="re"> real part array </param>
        /// <param name="im"> imaginary part array </param>
        /// <param name="y"> result array (input and modified) </param>
        public unsafe void Modify(long n,
            DenseArrayBase<double> re, DenseArrayBase<double> im,
            DenseArrayBase<Complex> y)
        {
            double* pre = (double*)re.DataPtr.ToPointer();
            double* pim = (double*)im.DataPtr.ToPointer();
            Complex* py = (Complex*)y.DataPtr.ToPointer();

            // defines loop operation
            Action<long> op = (i) =>
            {
                double iRe = *(pre + i);
                double iIm = *(pim + i);
                *(py + i) = new(iRe, iIm);
            };
            Loop1D loop = new(operation: op, start: 0, end: n, step: 1);
            loop.Evaluate(mode: Mode);
        }

        /// <summary>
        /// modifies the real part of a complex array
        /// </summary>
        /// <param name="n"> number of elements </param>
        /// <param name="re"> real part </param>
        /// <param name="y"> input and modified array result </param>
        public unsafe void ModifyRealOnly(long n,
            DenseArrayBase<double> re,
            DenseArrayBase<Complex> y)
        {
            double* pre = (double*)re.DataPtr.ToPointer();
            Complex* py = (Complex*)y.DataPtr.ToPointer();

            // defines loop operation
            Action<long> op = (i) =>
            {
                double iRe = *(pre + i);
                Complex iy = *(py + i);
                *(py + i) = new(iRe, iy.Imaginary);
            };
            Loop1D loop = new(operation: op, start: 0, end: n, step: 1);
            loop.Evaluate(mode: Mode);
        }

        /// <summary>
        /// modifies the imaginary part of a complex array
        /// </summary>
        /// <param name="n"> number of elements </param>
        /// <param name="im"> imaginary part </param>
        /// <param name="y"> input and modified array result </param>
        public unsafe void ModifyImagOnly(long n,
            DenseArrayBase<double> im,
            DenseArrayBase<Complex> y)
        {
            double* pim = (double*)im.DataPtr.ToPointer();
            Complex* py = (Complex*)y.DataPtr.ToPointer();

            // defines loop operation
            Action<long> op = (i) =>
            {
                double iIm = *(pim + i);
                Complex iy = *(py + i);
                *(py + i) = new(iy.Real, iIm);
            };
            Loop1D loop = new(operation: op, start: 0, end: n, step: 1);
            loop.Evaluate(mode: Mode);
        }

        #endregion
        #region --------- Abs ---------

        /// <summary>
        /// computes absolute value of array elements
        /// </summary>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="y"> result array y </param>
        public unsafe void Abs(long n,
            DenseArrayBase<double> a,
            DenseArrayBase<double> y)
        {
            double* pa = (double*)a.DataPtr.ToPointer();
            double* py = (double*)y.DataPtr.ToPointer();

            // defines loop operation
            Action<long> op = (i) =>
            {
                double ia = *(pa + i);
                *(py + i) = Math.Abs(ia);
            };
            Loop1D loop = new(operation: op, start: 0, end: n, step: 1);
            loop.Evaluate(mode: Mode);
        }

        /// <summary>
        /// computes absolute value of array elements
        /// </summary>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="y"> result array y </param>
        public unsafe void Abs(long n,
            DenseArrayBase<Complex> a,
            DenseArrayBase<double> y)
        {
            Complex* pa = (Complex*)a.DataPtr.ToPointer();
            double* py = (double*)y.DataPtr.ToPointer();

            // defines loop operation
            Action<long> op = (i) =>
            {
                Complex ia = *(pa + i);
                *(py + i) = Complex.Abs(ia);
            };
            Loop1D loop = new(operation: op, start: 0, end: n, step: 1);
            loop.Evaluate(mode: Mode);
        }

        #endregion
        #region --------- Arg ---------

        /// <summary>
        /// computes argument of a complex array's elements
        /// </summary>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="y"> result array y </param>
        public unsafe void Arg(long n,
            DenseArrayBase<Complex> a,
            DenseArrayBase<double> y)
        {
            Complex* pa = (Complex*)a.DataPtr.ToPointer();
            double* py = (double*)y.DataPtr.ToPointer();

            // defines loop operation
            Action<long> op = (i) =>
            {
                Complex ia = *(pa + i);
                *(py + i) = ia.Phase;
            };
            Loop1D loop = new(operation: op, start: 0, end: n, step: 1);
            loop.Evaluate(mode: Mode);
        }

        #endregion
        #region --------- LinearFrac ---------

        /// <summary>
        /// performs linear fraction transformation of 
        /// vectors a and b with scalar parameters
        /// y[i] = (scalea*a[i]+shifta) / (scaleb*b[i]+shiftb)
        /// </summary>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="b"> input array b </param>
        /// <param name="scalea"> scale of a </param>
        /// <param name="shifta"> shift of a </param>
        /// <param name="scaleb"> scale of b </param>
        /// <param name="shiftb"> shift of b </param>
        /// <param name="y"> result array y </param>
        public unsafe void LinearFrac(long n,
            DenseArrayBase<double> a, DenseArrayBase<double> b,
            double scalea, double shifta, double scaleb, double shiftb,
            DenseArrayBase<double> y)
        {
            double* pa = (double*)a.DataPtr.ToPointer();
            double* pb = (double*)b.DataPtr.ToPointer();
            double* py = (double*)y.DataPtr.ToPointer();

            // defines loop operation
            Action<long> op = (i) =>
            {
                double ia = *(pa + i);
                double ib = *(pb + i);
                *(py + i) = (scalea * ia + shifta) / (scaleb * ib + shiftb);
            };
            Loop1D loop = new(operation: op, start: 0, end: n, step: 1);
            loop.Evaluate(mode: Mode);
        }

        /// <summary>
        /// performs linear fraction transformation of 
        /// vectors a and b with scalar parameters
        /// real-part: y[i].Re = (scalea*a[i].Re + shifta.Re) / (scaleb*b[i].Re + shiftb.Re)
        /// imag-part: y[i].Im = (scalea*a[i].Im + shifta.Im) / (scaleb*b[i].Im + shiftb.Im)
        /// </summary>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="b"> input array b </param>
        /// <param name="scalea"> scale of a </param>
        /// <param name="shifta"> shift of a </param>
        /// <param name="scaleb"> scale of b </param>
        /// <param name="shiftb"> shift of b </param>
        /// <param name="y"> result array y </param>
        public unsafe void LinearFrac(long n,
            DenseArrayBase<Complex> a, DenseArrayBase<Complex> b,
            double scalea, Complex shifta, double scaleb, Complex shiftb,
            DenseArrayBase<Complex> y)
        {
            Complex* pa = (Complex*)a.DataPtr.ToPointer();
            Complex* pb = (Complex*)b.DataPtr.ToPointer();
            Complex* py = (Complex*)y.DataPtr.ToPointer();

            // defines loop operation
            Action<long> op = (i) =>
            {
                Complex ia = *(pa + i);
                Complex ib = *(pb + i);
                double re = (scalea * ia.Real + shifta.Real) / (scaleb * ib.Real + shiftb.Real);
                double im = (scalea * ia.Imaginary + shifta.Imaginary) / (scaleb * ib.Imaginary + shiftb.Imaginary);
                *(py + i) = new Complex(re, im);
            };
            Loop1D loop = new(operation: op, start: 0, end: n, step: 1);
            loop.Evaluate(mode: Mode);
        }

        #endregion
        #region --------- Inv ---------

        /// <summary>
        /// performs element by element inversion of the array
        /// </summary>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="y"> result array y </param>
        public unsafe void Inv(long n,
            DenseArrayBase<double> a,
            DenseArrayBase<double> y)
        {
            double* pa = (double*)a.DataPtr.ToPointer();
            double* py = (double*)y.DataPtr.ToPointer();

            // defines loop operation
            Action<long> op = (i) =>
            {
                double ia = *(pa + i);
                *(py + i) = 1.0 / ia;
            };
            Loop1D loop = new(operation: op, start: 0, end: n, step: 1);
            loop.Evaluate(mode: Mode);
        }

        /// <summary>
        /// performs element by element inversion of the array
        /// </summary>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="y"> result array y </param>
        public unsafe void Inv(long n,
            DenseArrayBase<Complex> a,
            DenseArrayBase<Complex> y)
        {
            Complex* pa = (Complex*)a.DataPtr.ToPointer();
            Complex* py = (Complex*)y.DataPtr.ToPointer();

            // defines loop operation
            Action<long> op = (i) =>
            {
                Complex ia = *(pa + i);
                *(py + i) = 1.0 / ia;
            };
            Loop1D loop = new(operation: op, start: 0, end: n, step: 1);
            loop.Evaluate(mode: Mode);
        }

        #endregion
        #region --------- Div ---------

        /// <summary>
        /// performs element by element division of array a by array b
        /// </summary>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="b"> input array b </param>
        /// <param name="y"> result array y </param>
        public unsafe void Div(long n,
            DenseArrayBase<double> a, DenseArrayBase<double> b,
            DenseArrayBase<double> y)
        {
            double* pa = (double*)a.DataPtr.ToPointer();
            double* pb = (double*)b.DataPtr.ToPointer();
            double* py = (double*)y.DataPtr.ToPointer();

            // defines loop operation
            Action<long> op = (i) =>
            {
                double ia = *(pa + i);
                double ib = *(pb + i);
                *(py + i) = ia / ib;
            };
            Loop1D loop = new(operation: op, start: 0, end: n, step: 1);
            loop.Evaluate(mode: Mode);
        }

        /// <summary>
        /// performs element by element division of array a by array b
        /// </summary>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="b"> input array b </param>
        /// <param name="y"> result array y </param>
        public unsafe void Div(long n,
            DenseArrayBase<Complex> a, DenseArrayBase<Complex> b,
            DenseArrayBase<Complex> y)
        {
            Complex* pa = (Complex*)a.DataPtr.ToPointer();
            Complex* pb = (Complex*)b.DataPtr.ToPointer();
            Complex* py = (Complex*)y.DataPtr.ToPointer();

            // defines loop operation
            Action<long> op = (i) =>
            {
                Complex ia = *(pa + i);
                Complex ib = *(pb + i);
                *(py + i) = ia / ib;
            };
            Loop1D loop = new(operation: op, start: 0, end: n, step: 1);
            loop.Evaluate(mode: Mode);
        }

        #endregion
        #region --------- Sqrt ---------

        /// <summary>
        /// computes a square root of array elements
        /// </summary>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="y"> result array y </param>
        public unsafe void Sqrt(long n,
            DenseArrayBase<double> a,
            DenseArrayBase<double> y)
        {
            double* pa = (double*)a.DataPtr.ToPointer();
            double* py = (double*)y.DataPtr.ToPointer();

            // defines loop operation
            Action<long> op = (i) =>
            {
                double ia = *(pa + i);
                *(py + i) = Math.Sqrt(ia);
            };
            Loop1D loop = new(operation: op, start: 0, end: n, step: 1);
            loop.Evaluate(mode: Mode);
        }

        /// <summary>
        /// computes a square root of array elements
        /// </summary>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="y"> result array y </param>
        public unsafe void Sqrt(long n,
            DenseArrayBase<Complex> a,
            DenseArrayBase<Complex> y)
        {
            Complex* pa = (Complex*)a.DataPtr.ToPointer();
            Complex* py = (Complex*)y.DataPtr.ToPointer();

            // defines loop operation
            Action<long> op = (i) =>
            {
                Complex ia = *(pa + i);
                *(py + i) = Complex.Sqrt(ia);
            };
            Loop1D loop = new(operation: op, start: 0, end: n, step: 1);
            loop.Evaluate(mode: Mode);
        }

        #endregion
        #region --------- InvSqrt ---------

        /// <summary>
        /// computes an inverse square root of array elements
        /// </summary>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="y"> result array y </param>
        public unsafe void InvSqrt(long n,
            DenseArrayBase<double> a,
            DenseArrayBase<double> y)
        {
            double* pa = (double*)a.DataPtr.ToPointer();
            double* py = (double*)y.DataPtr.ToPointer();

            // defines loop operation
            Action<long> op = (i) =>
            {
                double ia = *(pa + i);
                *(py + i) = 1.0 / Math.Sqrt(ia);
            };
            Loop1D loop = new(operation: op, start: 0, end: n, step: 1);
            loop.Evaluate(mode: Mode);
        }

        #endregion
        #region --------- Cbrt ---------

        /// <summary>
        /// Computes a cube root of array elements
        /// </summary>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="y"> result array y </param>
        public unsafe void Cbrt(long n,
            DenseArrayBase<double> a,
            DenseArrayBase<double> y)
        {
            double* pa = (double*)a.DataPtr.ToPointer();
            double* py = (double*)y.DataPtr.ToPointer();

            // defines loop operation
            Action<long> op = (i) =>
            {
                double ia = *(pa + i);
                *(py + i) = Math.Cbrt(ia);
            };
            Loop1D loop = new(operation: op, start: 0, end: n, step: 1);
            loop.Evaluate(mode: Mode);
        }

        #endregion
        #region --------- InvCbrt ---------

        /// <summary>
        /// computes an inverse cube root of array elements
        /// </summary>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="y"> result array y </param>
        public unsafe void InvCbrt(long n,
            DenseArrayBase<double> a,
            DenseArrayBase<double> y)
        {
            double* pa = (double*)a.DataPtr.ToPointer();
            double* py = (double*)y.DataPtr.ToPointer();

            // defines loop operation
            Action<long> op = (i) =>
            {
                double ia = *(pa + i);
                *(py + i) = 1.0 / Math.Cbrt(ia);
            };
            Loop1D loop = new(operation: op, start: 0, end: n, step: 1);
            loop.Evaluate(mode: Mode);
        }

        #endregion
        #region --------- Pow2o3 ---------

        /// <summary>
        /// computes the cube root of the square of each array element
        /// </summary>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="y"> result array y </param>
        public unsafe void Pow2o3(long n,
            DenseArrayBase<double> a,
            DenseArrayBase<double> y)
        {
            double* pa = (double*)a.DataPtr.ToPointer();
            double* py = (double*)y.DataPtr.ToPointer();

            // defines loop operation
            Action<long> op = (i) =>
            {
                double ia = *(pa + i);
                *(py + i) = Math.Pow(ia, 2.0 / 3.0);
            };
            Loop1D loop = new(operation: op, start: 0, end: n, step: 1);
            loop.Evaluate(mode: Mode);
        }

        #endregion
        #region --------- Pow3o2 ---------

        /// <summary>
        /// computes the square root of the cube of each array element
        /// </summary>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="y"> result array y </param>
        public unsafe void Pow3o2(long n,
            DenseArrayBase<double> a,
            DenseArrayBase<double> y)
        {
            double* pa = (double*)a.DataPtr.ToPointer();
            double* py = (double*)y.DataPtr.ToPointer();

            // defines loop operation
            Action<long> op = (i) =>
            {
                double ia = *(pa + i);
                *(py + i) = Math.Pow(ia, 3.0 / 2.0);
            };
            Loop1D loop = new(operation: op, start: 0, end: n, step: 1);
            loop.Evaluate(mode: Mode);
        }

        #endregion
        #region --------- Pow ---------

        /// <summary>
        /// computes a to the power b for elements of two arrays
        /// </summary>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input arrays a </param>
        /// <param name="b"> input arrays b </param>
        /// <param name="y"> result arrays y </param>
        public unsafe void Pow(long n,
            DenseArrayBase<double> a, DenseArrayBase<double> b,
            DenseArrayBase<double> y)
        {
            double* pa = (double*)a.DataPtr.ToPointer();
            double* pb = (double*)b.DataPtr.ToPointer();
            double* py = (double*)y.DataPtr.ToPointer();

            // defines loop operation
            Action<long> op = (i) =>
            {
                double ia = *(pa + i);
                double ib = *(pb + i);
                *(py + i) = Math.Pow(ia, ib);
            };
            Loop1D loop = new(operation: op, start: 0, end: n, step: 1);
            loop.Evaluate(mode: Mode);
        }

        /// <summary>
        /// computes each element of array a to the scalar power b
        /// </summary>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="b"> input scalar b </param>
        /// <param name="y"> result array y </param>
        public unsafe void Pow(long n,
            DenseArrayBase<double> a, double b,
            DenseArrayBase<double> y)
        {
            double* pa = (double*)a.DataPtr.ToPointer();
            double* py = (double*)y.DataPtr.ToPointer();

            // defines loop operation
            Action<long> op = (i) =>
            {
                double ia = *(pa + i);
                *(py + i) = Math.Pow(ia, b);
            };
            Loop1D loop = new(operation: op, start: 0, end: n, step: 1);
            loop.Evaluate(mode: Mode);
        }

        /// <summary>
        /// computes a to the power b for elements of two arrays
        /// </summary>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="b"> input array b </param>
        /// <param name="y"> result array y </param>
        public unsafe void Pow(long n,
            DenseArrayBase<Complex> a, DenseArrayBase<Complex> b,
            DenseArrayBase<Complex> y)
        {
            Complex* pa = (Complex*)a.DataPtr.ToPointer();
            Complex* pb = (Complex*)b.DataPtr.ToPointer();
            Complex* py = (Complex*)y.DataPtr.ToPointer();

            // defines loop operation
            Action<long> op = (i) =>
            {
                Complex ia = *(pa + i);
                Complex ib = *(pb + i);
                *(py + i) = Complex.Pow(ia, ib);
            };
            Loop1D loop = new(operation: op, start: 0, end: n, step: 1);
            loop.Evaluate(mode: Mode);
        }

        /// <summary>
        /// computes each element of array a to the scalar power b
        /// </summary>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="b"> input scalar b </param>
        /// <param name="y"> result array y </param>
        public unsafe void Pow(long n,
            DenseArrayBase<Complex> a, Complex b,
            DenseArrayBase<Complex> y)
        {
            Complex* pa = (Complex*)a.DataPtr.ToPointer();
            Complex* py = (Complex*)y.DataPtr.ToPointer();

            // defines loop operation
            Action<long> op = (i) =>
            {
                Complex ia = *(pa + i);
                *(py + i) = Complex.Pow(ia, b);
            };
            Loop1D loop = new(operation: op, start: 0, end: n, step: 1);
            loop.Evaluate(mode: Mode);
        }

        #endregion
        #region --------- Hypot ---------

        /// <summary>
        /// computes a square root of sum of two squared elements
        /// </summary>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="b"> input array b </param>
        /// <param name="y"> result array y </param>
        public unsafe void Hypot(long n,
            DenseArrayBase<double> a, DenseArrayBase<double> b,
            DenseArrayBase<double> y)
        {
            double* pa = (double*)a.DataPtr.ToPointer();
            double* pb = (double*)b.DataPtr.ToPointer();
            double* py = (double*)y.DataPtr.ToPointer();

            // defines loop operation
            Action<long> op = (i) =>
            {
                double ia = *(pa + i);
                double ib = *(pb + i);
                *(py + i) = Math.Sqrt(ia * ia + ib * ib);
            };
            Loop1D loop = new(operation: op, start: 0, end: n, step: 1);
            loop.Evaluate(mode: Mode);
        }

        #endregion
        #region --------- Exp ---------

        /// <summary>
        /// computes an exponential of array elements
        /// </summary>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="y"> result array y </param>
        public unsafe void Exp(long n,
            DenseArrayBase<double> a,
            DenseArrayBase<double> y)
        {
            double* pa = (double*)a.DataPtr.ToPointer();
            double* py = (double*)y.DataPtr.ToPointer();

            // defines loop operation
            Action<long> op = (i) =>
            {
                double ia = *(pa + i);
                *(py + i) = Math.Exp(ia);
            };
            Loop1D loop = new(operation: op, start: 0, end: n, step: 1);
            loop.Evaluate(mode: Mode);
        }

        /// <summary>
        /// computes an exponential of array elements
        /// </summary>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="y"> result array y </param>
        public unsafe void Exp(long n,
            DenseArrayBase<Complex> a,
            DenseArrayBase<Complex> y)
        {
            Complex* pa = (Complex*)a.DataPtr.ToPointer();
            Complex* py = (Complex*)y.DataPtr.ToPointer();

            // defines loop operation
            Action<long> op = (i) =>
            {
                Complex ia = *(pa + i);
                *(py + i) = Complex.Exp(ia);
            };
            Loop1D loop = new(operation: op, start: 0, end: n, step: 1);
            loop.Evaluate(mode: Mode);
        }

        #endregion
        #region --------- Ln ---------

        /// <summary>
        /// computes natural logarithm of array elements
        /// </summary>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="y"> result array y </param>
        public unsafe void Ln(long n,
            DenseArrayBase<double> a,
            DenseArrayBase<double> y)
        {
            double* pa = (double*)a.DataPtr.ToPointer();
            double* py = (double*)y.DataPtr.ToPointer();

            // defines loop operation
            Action<long> op = (i) =>
            {
                double ia = *(pa + i);
                *(py + i) = Math.Log(ia);
            };
            Loop1D loop = new(operation: op, start: 0, end: n, step: 1);
            loop.Evaluate(mode: Mode);
        }

        /// <summary>
        /// computes natural logarithm of array elements
        /// </summary>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="y"> result array y </param>
        public unsafe void Ln(long n,
            DenseArrayBase<Complex> a,
            DenseArrayBase<Complex> y)
        {
            Complex* pa = (Complex*)a.DataPtr.ToPointer();
            Complex* py = (Complex*)y.DataPtr.ToPointer();

            // defines loop operation
            Action<long> op = (i) =>
            {
                Complex ia = *(pa + i);
                *(py + i) = Complex.Log(ia);
            };
            Loop1D loop = new(operation: op, start: 0, end: n, step: 1);
            loop.Evaluate(mode: Mode);
        }

        #endregion
        #region --------- Log10 ---------

        /// <summary>
        /// computes the base 10 logarithm of array elements
        /// </summary>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="y"> result array y </param>
        public unsafe void Log10(long n,
            DenseArrayBase<double> a,
            DenseArrayBase<double> y)
        {
            double* pa = (double*)a.DataPtr.ToPointer();
            double* py = (double*)y.DataPtr.ToPointer();

            // defines loop operation
            Action<long> op = (i) =>
            {
                double ia = *(pa + i);
                *(py + i) = Math.Log10(ia);
            };
            Loop1D loop = new(operation: op, start: 0, end: n, step: 1);
            loop.Evaluate(mode: Mode);
        }

        /// <summary>
        /// computes the base 10 logarithm of array elements
        /// </summary>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="y"> result array y </param>
        public unsafe void Log10(long n,
            DenseArrayBase<Complex> a,
            DenseArrayBase<Complex> y)
        {
            Complex* pa = (Complex*)a.DataPtr.ToPointer();
            Complex* py = (Complex*)y.DataPtr.ToPointer();

            // defines loop operation
            Action<long> op = (i) =>
            {
                Complex ia = *(pa + i);
                *(py + i) = Complex.Log10(ia);
            };
            Loop1D loop = new(operation: op, start: 0, end: n, step: 1);
            loop.Evaluate(mode: Mode);
        }

        #endregion
        #region --------- Cos ---------

        /// <summary>
        /// computes cosine of array elements
        /// </summary>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="y"> result array y </param>
        public unsafe void Cos(long n,
            DenseArrayBase<double> a,
            DenseArrayBase<double> y)
        {
            double* pa = (double*)a.DataPtr.ToPointer();
            double* py = (double*)y.DataPtr.ToPointer();

            // defines loop operation
            Action<long> op = (i) =>
            {
                double ia = *(pa + i);
                *(py + i) = Math.Cos(ia);
            };
            Loop1D loop = new(operation: op, start: 0, end: n, step: 1);
            loop.Evaluate(mode: Mode);
        }

        /// <summary>
        /// computes cosine of array elements
        /// </summary>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="y"> result array y </param>
        public unsafe void Cos(long n,
            DenseArrayBase<Complex> a,
            DenseArrayBase<Complex> y)
        {
            Complex* pa = (Complex*)a.DataPtr.ToPointer();
            Complex* py = (Complex*)y.DataPtr.ToPointer();

            // defines loop operation
            Action<long> op = (i) =>
            {
                Complex ia = *(pa + i);
                *(py + i) = Complex.Cos(ia);
            };
            Loop1D loop = new(operation: op, start: 0, end: n, step: 1);
            loop.Evaluate(mode: Mode);
        }

        #endregion
        #region --------- Sin ---------

        /// <summary>
        /// computes sine of array elements
        /// </summary>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="y"> result array y </param>
        public unsafe void Sin(long n,
            DenseArrayBase<double> a,
            DenseArrayBase<double> y)
        {
            double* pa = (double*)a.DataPtr.ToPointer();
            double* py = (double*)y.DataPtr.ToPointer();

            // defines loop operation
            Action<long> op = (i) =>
            {
                double ia = *(pa + i);
                *(py + i) = Math.Sin(ia);
            };
            Loop1D loop = new(operation: op, start: 0, end: n, step: 1);
            loop.Evaluate(mode: Mode);
        }

        /// <summary>
        /// computes sine of array elements
        /// </summary>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="y"> result array y </param>
        public unsafe void Sin(long n,
            DenseArrayBase<Complex> a,
            DenseArrayBase<Complex> y)
        {
            Complex* pa = (Complex*)a.DataPtr.ToPointer();
            Complex* py = (Complex*)y.DataPtr.ToPointer();

            // defines loop operation
            Action<long> op = (i) =>
            {
                Complex ia = *(pa + i);
                *(py + i) = Complex.Sin(ia);
            };
            Loop1D loop = new(operation: op, start: 0, end: n, step: 1);
            loop.Evaluate(mode: Mode);
        }

        #endregion
        #region --------- SinCos ---------

        /// <summary>
        /// computes sine and cosine of array elements
        /// </summary>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="sin"> result array sin </param>
        /// <param name="cos"> result array cos </param>
        public unsafe void SinCos(long n,
            DenseArrayBase<double> a,
            DenseArrayBase<double> sin, DenseArrayBase<double> cos)
        {
            double* pa = (double*)a.DataPtr.ToPointer();
            double* psin = (double*)sin.DataPtr.ToPointer();
            double* pcos = (double*)cos.DataPtr.ToPointer();

            // defines loop operation
            Action<long> op = (i) =>
            {
                double ia = *(pa + i);
                *(psin + i) = Math.Sin(ia);
                *(pcos + i) = Math.Cos(ia);
            };
            Loop1D loop = new(operation: op, start: 0, end: n, step: 1);
            loop.Evaluate(mode: Mode);
        }

        #endregion
        #region --------- Tan ---------

        /// <summary>
        /// computes tangent of array elements
        /// </summary>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="y"> result array y </param>
        public unsafe void Tan(long n,
            DenseArrayBase<double> a,
            DenseArrayBase<double> y)
        {
            double* pa = (double*)a.DataPtr.ToPointer();
            double* py = (double*)y.DataPtr.ToPointer();

            // defines loop operation
            Action<long> op = (i) =>
            {
                double ia = *(pa + i);
                *(py + i) = Math.Tan(ia);
            };
            Loop1D loop = new(operation: op, start: 0, end: n, step: 1);
            loop.Evaluate(mode: Mode);
        }

        /// <summary>
        /// computes tangent of array elements
        /// </summary>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="y"> result array y </param>
        public unsafe void Tan(long n,
            DenseArrayBase<Complex> a,
            DenseArrayBase<Complex> y)
        {
            Complex* pa = (Complex*)a.DataPtr.ToPointer();
            Complex* py = (Complex*)y.DataPtr.ToPointer();

            // defines loop operation
            Action<long> op = (i) =>
            {
                Complex ia = *(pa + i);
                *(py + i) = Complex.Tan(ia);
            };
            Loop1D loop = new(operation: op, start: 0, end: n, step: 1);
            loop.Evaluate(mode: Mode);
        }

        #endregion
        #region --------- Acos ---------

        /// <summary>
        /// computes inverse cosine of array elements
        /// </summary>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="y"> result array y </param>
        public unsafe void Acos(long n,
            DenseArrayBase<double> a,
            DenseArrayBase<double> y)
        {
            double* pa = (double*)a.DataPtr.ToPointer();
            double* py = (double*)y.DataPtr.ToPointer();

            // defines loop operation
            Action<long> op = (i) =>
            {
                double ia = *(pa + i);
                *(py + i) = Math.Acos(ia);
            };
            Loop1D loop = new(operation: op, start: 0, end: n, step: 1);
            loop.Evaluate(mode: Mode);
        }

        /// <summary>
        /// computes inverse cosine of array elements
        /// </summary>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="y"> result array y </param>
        public unsafe void Acos(long n,
            DenseArrayBase<Complex> a,
            DenseArrayBase<Complex> y)
        {
            Complex* pa = (Complex*)a.DataPtr.ToPointer();
            Complex* py = (Complex*)y.DataPtr.ToPointer();

            // defines loop operation
            Action<long> op = (i) =>
            {
                Complex ia = *(pa + i);
                *(py + i) = Complex.Acos(ia);
            };
            Loop1D loop = new(operation: op, start: 0, end: n, step: 1);
            loop.Evaluate(mode: Mode);
        }

        #endregion
        #region --------- Asin ---------

        /// <summary>
        /// computes inverse sine of array elements
        /// </summary>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="y"> result array y </param>
        public unsafe void Asin(long n,
            DenseArrayBase<double> a,
            DenseArrayBase<double> y)
        {
            double* pa = (double*)a.DataPtr.ToPointer();
            double* py = (double*)y.DataPtr.ToPointer();

            // defines loop operation
            Action<long> op = (i) =>
            {
                double ia = *(pa + i);
                *(py + i) = Math.Asin(ia);
            };
            Loop1D loop = new(operation: op, start: 0, end: n, step: 1);
            loop.Evaluate(mode: Mode);
        }

        /// <summary>
        /// computes inverse sine of array elements
        /// </summary>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="y"> result array y </param>
        public unsafe void Asin(long n,
            DenseArrayBase<Complex> a,
            DenseArrayBase<Complex> y)
        {
            Complex* pa = (Complex*)a.DataPtr.ToPointer();
            Complex* py = (Complex*)y.DataPtr.ToPointer();

            // defines loop operation
            Action<long> op = (i) =>
            {
                Complex ia = *(pa + i);
                *(py + i) = Complex.Asin(ia);
            };
            Loop1D loop = new(operation: op, start: 0, end: n, step: 1);
            loop.Evaluate(mode: Mode);
        }

        #endregion
        #region --------- Atan ---------

        /// <summary>
        /// Computes inverse tangent of array elements
        /// </summary>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="y"> result array y </param>
        public unsafe void Atan(long n,
            DenseArrayBase<double> a,
            DenseArrayBase<double> y)
        {
            double* pa = (double*)a.DataPtr.ToPointer();
            double* py = (double*)y.DataPtr.ToPointer();

            // defines loop operation
            Action<long> op = (i) =>
            {
                double ia = *(pa + i);
                *(py + i) = Math.Atan(ia);
            };
            Loop1D loop = new(operation: op, start: 0, end: n, step: 1);
            loop.Evaluate(mode: Mode);
        }

        /// <summary>
        /// Computes inverse tangent of array elements
        /// </summary>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="y"> result array y </param>
        public unsafe void Atan(long n,
            DenseArrayBase<Complex> a,
            DenseArrayBase<Complex> y)
        {
            Complex* pa = (Complex*)a.DataPtr.ToPointer();
            Complex* py = (Complex*)y.DataPtr.ToPointer();

            // defines loop operation
            Action<long> op = (i) =>
            {
                Complex ia = *(pa + i);
                *(py + i) = Complex.Atan(ia);
            };
            Loop1D loop = new(operation: op, start: 0, end: n, step: 1);
            loop.Evaluate(mode: Mode);
        }

        #endregion
        #region --------- Atan2 ---------

        /// <summary>
        /// computes four-quadrant inverse tangent of elements of two vectors
        /// </summary>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input vector a </param>
        /// <param name="b"> input vector b </param>
        /// <param name="y"> result vector y </param>
        public unsafe void Atan2(long n,
            DenseArrayBase<double> a, DenseArrayBase<double> b,
            DenseArrayBase<double> y)
        {
            double* pa = (double*)a.DataPtr.ToPointer();
            double* pb = (double*)b.DataPtr.ToPointer();
            double* py = (double*)y.DataPtr.ToPointer();

            // defines loop operation
            Action<long> op = (i) =>
            {
                double ia = *(pa + i);
                double ib = *(pb + i);
                *(py + i) = Math.Atan2(ia, ib);
            };
            Loop1D loop = new(operation: op, start: 0, end: n, step: 1);
            loop.Evaluate(mode: Mode);
        }

        #endregion
        #region --------- Cosh ---------

        /// <summary>
        /// computes hyperbolic cosine of array elements
        /// </summary>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="y"> result array y </param>
        public unsafe void Cosh(long n,
            DenseArrayBase<double> a,
            DenseArrayBase<double> y)
        {
            double* pa = (double*)a.DataPtr.ToPointer();
            double* py = (double*)y.DataPtr.ToPointer();

            // defines loop operation
            Action<long> op = (i) =>
            {
                double ia = *(pa + i);
                *(py + i) = Math.Cosh(ia);
            };
            Loop1D loop = new(operation: op, start: 0, end: n, step: 1);
            loop.Evaluate(mode: Mode);
        }

        /// <summary>
        /// computes hyperbolic cosine of array elements
        /// </summary>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="y"> result array y </param>
        public unsafe void Cosh(long n,
            DenseArrayBase<Complex> a,
            DenseArrayBase<Complex> y)
        {
            Complex* pa = (Complex*)a.DataPtr.ToPointer();
            Complex* py = (Complex*)y.DataPtr.ToPointer();

            // defines loop operation
            Action<long> op = (i) =>
            {
                Complex ia = *(pa + i);
                *(py + i) = Complex.Cosh(ia);
            };
            Loop1D loop = new(operation: op, start: 0, end: n, step: 1);
            loop.Evaluate(mode: Mode);
        }

        #endregion
        #region --------- Sinh ---------

        /// <summary>
        /// computes hyperbolic sine of array elements
        /// </summary>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="y"> result array y </param>
        public unsafe void Sinh(long n,
            DenseArrayBase<double> a,
            DenseArrayBase<double> y)
        {
            double* pa = (double*)a.DataPtr.ToPointer();
            double* py = (double*)y.DataPtr.ToPointer();

            // defines loop operation
            Action<long> op = (i) =>
            {
                double ia = *(pa + i);
                *(py + i) = Math.Sinh(ia);
            };
            Loop1D loop = new(operation: op, start: 0, end: n, step: 1);
            loop.Evaluate(mode: Mode);
        }

        /// <summary>
        /// computes hyperbolic sine of array elements
        /// </summary>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="y"> result array y </param>
        public unsafe void Sinh(long n,
            DenseArrayBase<Complex> a,
            DenseArrayBase<Complex> y)
        {
            Complex* pa = (Complex*)a.DataPtr.ToPointer();
            Complex* py = (Complex*)y.DataPtr.ToPointer();

            // defines loop operation
            Action<long> op = (i) =>
            {
                Complex ia = *(pa + i);
                *(py + i) = Complex.Sinh(ia);
            };
            Loop1D loop = new(operation: op, start: 0, end: n, step: 1);
            loop.Evaluate(mode: Mode);
        }

        #endregion
        #region --------- Tanh ---------

        /// <summary>
        /// computes hyperbolic tangent of array elements
        /// </summary>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="y"> result array y </param>
        public unsafe void Tanh(long n,
            DenseArrayBase<double> a,
            DenseArrayBase<double> y)
        {
            double* pa = (double*)a.DataPtr.ToPointer();
            double* py = (double*)y.DataPtr.ToPointer();

            // defines loop operation
            Action<long> op = (i) =>
            {
                double ia = *(pa + i);
                *(py + i) = Math.Tanh(ia);
            };
            Loop1D loop = new(operation: op, start: 0, end: n, step: 1);
            loop.Evaluate(mode: Mode);
        }

        /// <summary>
        /// computes hyperbolic tangent of array elements
        /// </summary>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="y"> result array y </param>
        public unsafe void Tanh(long n,
            DenseArrayBase<Complex> a,
            DenseArrayBase<Complex> y)
        {
            Complex* pa = (Complex*)a.DataPtr.ToPointer();
            Complex* py = (Complex*)y.DataPtr.ToPointer();

            // defines loop operation
            Action<long> op = (i) =>
            {
                Complex ia = *(pa + i);
                *(py + i) = Complex.Tanh(ia);
            };
            Loop1D loop = new(operation: op, start: 0, end: n, step: 1);
            loop.Evaluate(mode: Mode);
        }

        #endregion
        #region --------- Acosh ---------

        /// <summary>
        /// computes inverse hyperbolic cosine (nonnegative) of array elements
        /// </summary>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="y"> result array y </param>
        public unsafe void Acosh(long n,
            DenseArrayBase<double> a,
            DenseArrayBase<double> y)
        {
            double* pa = (double*)a.DataPtr.ToPointer();
            double* py = (double*)y.DataPtr.ToPointer();

            // defines loop operation
            Action<long> op = (i) =>
            {
                double ia = *(pa + i);
                *(py + i) = Math.Acosh(ia);
            };
            Loop1D loop = new(operation: op, start: 0, end: n, step: 1);
            loop.Evaluate(mode: Mode);
        }

        /// <summary>
        /// computes inverse hyperbolic cosine (nonnegative) of array elements
        /// </summary>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="y"> result array y </param>
        public unsafe void Acosh(long n,
            DenseArrayBase<Complex> a,
            DenseArrayBase<Complex> y)
        {
            throw new NotSupportedException();
        }

        #endregion
        #region --------- Asinh ---------

        /// <summary>
        /// computes inverse hyperbolic sine of array elements
        /// </summary>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="y"> result array y </param>
        public unsafe void Asinh(long n,
            DenseArrayBase<double> a,
            DenseArrayBase<double> y)
        {
            double* pa = (double*)a.DataPtr.ToPointer();
            double* py = (double*)y.DataPtr.ToPointer();

            // defines loop operation
            Action<long> op = (i) =>
            {
                double ia = *(pa + i);
                *(py + i) = Math.Asinh(ia);
            };
            Loop1D loop = new(operation: op, start: 0, end: n, step: 1);
            loop.Evaluate(mode: Mode);
        }

        /// <summary>
        /// computes inverse hyperbolic sine of array elements
        /// </summary>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="y"> result array y </param>
        public unsafe void Asinh(long n,
            DenseArrayBase<Complex> a,
            DenseArrayBase<Complex> y)
        {
            throw new NotSupportedException();
        }

        #endregion
        #region --------- Atanh ---------

        /// <summary>
        /// computes inverse hyperbolic tangent of array elements
        /// </summary>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="y"> result array y </param>
        public unsafe void Atanh(long n,
            DenseArrayBase<double> a,
            DenseArrayBase<double> y)
        {
            double* pa = (double*)a.DataPtr.ToPointer();
            double* py = (double*)y.DataPtr.ToPointer();

            // defines loop operation
            Action<long> op = (i) =>
            {
                double ia = *(pa + i);
                *(py + i) = Math.Atanh(ia);
            };
            Loop1D loop = new(operation: op, start: 0, end: n, step: 1);
            loop.Evaluate(mode: Mode);
        }

        /// <summary>
        /// computes inverse hyperbolic tangent of array elements
        /// </summary>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="y"> result array y </param>
        public unsafe void Atanh(long n,
            DenseArrayBase<Complex> a,
            DenseArrayBase<Complex> y)
        {
            throw new NotSupportedException();
        }

        #endregion
        #region --------- PackI ---------

        /// <summary>
        /// copies elements of an array with specified indexing 
        /// to an array with unit increment
        /// </summary>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="y"> result array y </param>
        /// <param name="inca"> increment for the elements of a </param>
        /// <param name="startA"> starting index in a </param>
        /// <param name="startY"> starting index in y </param>
        public unsafe void PackI(long n, DenseArrayBase<double> a, DenseArrayBase<double> y,
            long inca = 1, long startA = 0, long startY = 0)
        {
            double* pa = (double*)a.DataPtr.ToPointer() + startA;
            double* py = (double*)y.DataPtr.ToPointer() + startY;

            // defines loop operation
            Action<long> op = (i) =>
            {
                double ia = *(pa + i * inca);
                *(py + i) = ia;
            };
            Loop1D loop = new(operation: op, start: 0, end: n, step: 1);
            loop.Evaluate(mode: Mode);
        }

        /// <summary>
        /// copies elements of an array with specified indexing 
        /// to an array with unit increment
        /// </summary>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="inca"> increment for the elements of a </param>
        /// <param name="y"> result array y </param>
        /// <param name="startA"> starting index in a </param>
        /// <param name="startY"> starting index in y </param>
        public unsafe void PackI(long n, DenseArrayBase<Complex> a, DenseArrayBase<Complex> y,
            long inca = 1, long startA = 0, long startY = 0)
        {
            Complex* pa = (Complex*)a.DataPtr.ToPointer() + startA;
            Complex* py = (Complex*)y.DataPtr.ToPointer() + startY;

            // defines loop operation
            Action<long> op = (i) =>
            {
                Complex ia = *(pa + i * inca);
                *(py + i) = ia;
            };
            Loop1D loop = new(operation: op, start: 0, end: n, step: 1);
            loop.Evaluate(mode: Mode);
        }

        #endregion
        #region --------- UnpackI ---------

        /// <summary>
        /// Copies elements of an array with unit increment 
        /// to an array with specified indexing
        /// </summary>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="y"> result array y </param>
        /// <param name="incy"> increment for the elements of y </param>
        /// <param name="startA"> starting index in a </param>
        /// <param name="startY"> starting index in y </param>
        public unsafe void UnpackI(long n, DenseArrayBase<double> a, DenseArrayBase<double> y,
            long incy = 1, long startA = 0, long startY = 0)
        {
            double* pa = (double*)a.DataPtr.ToPointer() + startA;
            double* py = (double*)y.DataPtr.ToPointer() + startY;

            // defines loop operation
            Action<long> op = (i) =>
            {
                double ia = *(pa + i);
                *(py + i * incy) = ia;
            };
            Loop1D loop = new(operation: op, start: 0, end: n, step: 1);
            loop.Evaluate(mode: Mode);
        }

        /// <summary>
        /// Copies elements of an array with unit increment 
        /// to an array with specified indexing
        /// </summary>
        /// <param name="n"> number of elements </param>
        /// <param name="a"> input array a </param>
        /// <param name="y"> result array y </param>
        /// <param name="incy"> increment for the elements of y </param>
        /// <param name="startA"> starting index in a </param>
        /// <param name="startY"> starting index in y </param>
        public unsafe void UnpackI(long n, DenseArrayBase<Complex> a, DenseArrayBase<Complex> y,
            long incy = 1, long startA = 0, long startY = 0)
        {
            Complex* pa = (Complex*)a.DataPtr.ToPointer() + startA;
            Complex* py = (Complex*)y.DataPtr.ToPointer() + startY;

            // defines loop operation
            Action<long> op = (i) =>
            {
                Complex ia = *(pa + i);
                *(py + i * incy) = ia;
            };
            Loop1D loop = new(operation: op, start: 0, end: n, step: 1);
            loop.Evaluate(mode: Mode);
        }

        #endregion

        #endregion

    }


}
