using WMathCore;
using Real = System.Double;
using Complex = System.Numerics.Complex;

namespace VEMS.MathCore
{

    internal class SparseFactory
    {
        internal ISPBLAS iSPBLAS { get; set; }
        internal SparseFactory()
        {
            iSPBLAS = Defaults.ISPBLAS;
        }

    }

    /// <summary>
    /// sparse vector and matrix
    /// </summary>
    public unsafe class Sparse
    {
        private static SparseFactory factory = new();


        #region ---- helpers ----

        //private static void ExArrayLengthI<T>(T x, T y) 
        //    where T : ArrayBase<long>
        //{
        //    if (x.Count != y.Count)
        //    { throw new ArgumentException($"Array lengths not equal"); }
        //}

        private static void ExArrayLengthD<T>(T x, T y)
            where T : DenseArrayBase<double>
        {
            if (x.Count != y.Count)
            { throw new ArgumentException($"Array lengths not equal"); }
        }

        private static void ExArrayLengthZ<T>(T x, T y)
            where T : DenseArrayBase<Complex>
        {
            if (x.Count != y.Count)
            { throw new ArgumentException($"Array lengths not equal"); }
        }

        #endregion
        #region Sparse BLAS Level-1

        #region ---- ASum [D/Z] ----

        /// <summary>
        /// Computes the sum of the absolute values of all non-zero elements in a sparse real vector.
        /// dasum := |x[0]| + |x[1]| + ... + |x[n-1]|
        /// </summary>
        /// <param name="x">The sparse vector containing real values.</param>
        /// <param name="incx">The increment for indexing the vector (default is 1).</param>
        /// <returns>The sum of the absolute values of the non-zero elements.</returns>
        public static double ASum(SPVect<Real> x, long incx = 1)
            => factory.iSPBLAS.Asum(x.NzCount, x.NzValues.DPtr, incx);

        /// <summary>
        /// Computes the sum of the absolute values of all non-zero elements in a sparse complex vector.
        /// dzasum := |Re(x[0])| + |Im(x[0])| + |Re(x[1])| + |Im(x[1])| + ... + |Re(x[n-1])| + |Im(x[n-1])|
        /// </summary>
        /// <param name="x">The sparse vector containing complex values.</param>
        /// <param name="incx">The increment for indexing the vector (default is 1).</param>
        /// <returns>The sum of the absolute values of the real and imaginary parts of the non-zero elements.</returns>
        public static double ASum(SPVect<Cplx> x, long incx = 1)
            => factory.iSPBLAS.Asum(x.NzCount, x.NzValues.VPtr, incx);


        // obsolete ...
        /// <summary>
        /// calculates the sum of the absolute values 
        /// of all elements in x
        /// </summary>
        /// <typeparam name="T"> SPVector[double] </typeparam>
        /// <param name="x"> sparse vector x </param>
        /// <returns> summed absolute values </returns>
        public static double ASumD<T>(T x) where T : SPVector<double>
            => factory.iSPBLAS.AsumD(x.NzCount, x);

        /// <summary>
        /// calculates the sum of the absolute values 
        /// of all elements in x
        /// </summary>
        /// <typeparam name="T"> SPVector[Complex] </typeparam>
        /// <param name="x"> sparse vector x </param>
        /// <returns> summed absolute values </returns>
        public static double ASumZ<T>(T x) where T : SPVector<Complex>
            => factory.iSPBLAS.AsumZ(x.NzCount, x);


        /// <summary>
        /// calculates the sum of the absolute values 
        /// of all elements in x
        /// </summary>
        /// <param name="x"> sparse vector x </param>
        /// <returns> summed absolute values </returns>
        public static double ASum(VectorDi x)
            => ASumD(x);

        /// <summary>
        /// calculates the sum of the absolute values 
        /// of all elements in x
        /// </summary>
        /// <param name="x"> sparse vector x </param>
        /// <returns> summed absolute values </returns>
        public static double ASum(VectorZi x)
            => ASumZ(x);

        #endregion
        #region ---- Copy [D/Z] ----

        /// <summary>
        /// Copies the non-zero elements from the source sparse real vector <paramref name="x"/> 
        /// to the destination sparse real vector <paramref name="y"/>.
        /// </summary>
        /// <param name="x">The source sparse vector containing real values.</param>
        /// <param name="y">The destination sparse vector to copy values into (passed by reference).</param>
        /// <param name="incx">The increment for indexing the source vector (default is 1).</param>
        /// <param name="incy">The increment for indexing the destination vector (default is 1).</param>
        public static void Copy(SPVect<Real> x, ref SPVect<Real> y,
            long incx = 1, long incy = 1)
            => factory.iSPBLAS.Copy(x.NzCount,
                x.NzValues.DPtr, y.NzValues.DPtr,
                incx, incy);

        /// <summary>
        /// Copies the non-zero elements from the source sparse complex vector <paramref name="x"/> 
        /// to the destination sparse complex vector <paramref name="y"/>.
        /// </summary>
        /// <param name="x">The source sparse vector containing complex values.</param>
        /// <param name="y">The destination sparse vector to copy values into (passed by reference).</param>
        /// <param name="incx">The increment for indexing the source vector (default is 1).</param>
        /// <param name="incy">The increment for indexing the destination vector (default is 1).</param>
        public static void Copy(SPVect<Cplx> x, ref SPVect<Cplx> y,
            long incx = 1, long incy = 1)
            => factory.iSPBLAS.Copy(x.NzCount,
                x.NzValues.VPtr, y.NzValues.VPtr,
                incx, incy);


        // obsolete ...
        /// <summary>
        /// copies the non-zero elements from x to y
        /// </summary>
        /// <typeparam name="T"> SPVector[double] </typeparam>
        /// <param name="x"> sparse vector x </param>
        /// <param name="y"> sparse vector y </param>
        public static void CopyD<T>(T x, ref T y) where T : SPVector<double>
        {
            ExArrayLengthD(x.NzValues, y.NzValues);
            factory.iSPBLAS.CopyD(x.NzCount, x, ref y);
        }

        /// <summary>
        /// copies the non-zero elements from x to y
        /// </summary>
        /// <typeparam name="T"> SPVector[double] </typeparam>
        /// <param name="x"> sparse vector x </param>
        /// <param name="y"> sparse vector y </param>
        public static void CopyZ<T>(T x, ref T y) where T : SPVector<Complex>
        {
            ExArrayLengthZ(x.NzValues, y.NzValues);
            factory.iSPBLAS.CopyZ(x.NzCount, x, ref y);
        }


        /// <summary>
        /// copies the non-zero elements from x to y
        /// </summary>
        /// <param name="x"> sparse vector x </param>
        /// <param name="y"> sparse vector y </param>
        public static void Copy(VectorDi x, ref VectorDi y)
            => CopyD(x, ref y);

        /// <summary>
        /// copies the non-zero elements from x to y
        /// </summary>
        /// <param name="x"> sparse vector x </param>
        /// <param name="y"> sparse vector y </param>
        public static void Copy(VectorZi x, ref VectorZi y)
            => CopyZ(x, ref y);

        #endregion
        #region ---- Norm [D/Z] ----

        /// <summary>
        /// Computes the Euclidean norm (L2 norm) of a sparse real vector.
        /// </summary>
        /// <param name="x">The sparse vector containing real values.</param>
        /// <param name="incx">The increment for indexing the vector (default is 1).</param>
        /// <returns>The Euclidean norm of the sparse vector.</returns>
        public static double Norm(SPVect<Real> x, long incx = 1)
            => factory.iSPBLAS.Nrm2(x.NzCount, x.NzValues.DPtr, incx);

        /// <summary>
        /// Computes the Euclidean norm (L2 norm) of a sparse complex vector.
        /// </summary>
        /// <param name="x">The sparse vector containing complex values.</param>
        /// <param name="incx">The increment for indexing the vector (default is 1).</param>
        /// <returns>The Euclidean norm of the sparse complex vector.</returns>
        public static double Norm(SPVect<Cplx> x, long incx = 1)
            => factory.iSPBLAS.Nrm2(x.NzCount, x.NzIndices.VPtr, incx);


        // obsolete ...
        /// <summary>
        /// computes the Euclidean norm of a sparse vector
        /// </summary>
        /// <typeparam name="T"> SPVector[double] </typeparam>
        /// <param name="x"> sparse vector x </param>
        /// <returns> Euclidean norm </returns>
        public static double NormD<T>(T x) where T : SPVector<double>
            => factory.iSPBLAS.Nrm2D(x.NzCount, x);

        /// <summary>
        /// computes the Euclidean norm of a sparse vector
        /// </summary>
        /// <typeparam name="T"> SPVector[Complex] </typeparam>
        /// <param name="x"> sparse vector x </param>
        /// <returns> Euclidean norm </returns>
        public static double NormZ<T>(T x) where T : SPVector<Complex>
            => factory.iSPBLAS.Nrm2Z(x.NzCount, x);


        /// <summary>
        /// computes the Euclidean norm of a vector
        /// </summary>
        /// <param name="x"> sparse vector x </param>
        /// <returns> Euclidean norm </returns>
        public static double Norm(VectorDi x)
            => NormD(x);

        /// <summary>
        /// computes the Euclidean norm of a vector
        /// </summary>
        /// <param name="x"> sparse vector x </param>
        /// <returns> Euclidean norm </returns>
        public static double Norm(VectorZi x)
            => NormZ(x);

        #endregion
        #region ---- Scal [D/Z] ----

        /// <summary>
        /// Scales the non-zero elements of a sparse real vector by a real scalar value.
        /// </summary>
        /// <param name="x">The sparse vector containing real values to be scaled.</param>
        /// <param name="a">The real scalar multiplier.</param>
        /// <param name="incx">The increment for indexing the vector (default is 1).</param>
        public static void Scal(SPVect<Real> x, Real a, long incx = 1)
            => factory.iSPBLAS.Scal(x.NzCount, a, x.NzValues.DPtr, incx);

        /// <summary>
        /// Scales the non-zero elements of a sparse complex vector by a real scalar value.
        /// </summary>
        /// <param name="x">The sparse vector containing complex values to be scaled.</param>
        /// <param name="a">The real scalar multiplier.</param>
        /// <param name="incx">The increment for indexing the vector (default is 1).</param>
        public static void Scal(SPVect<Cplx> x, Real a, long incx = 1)
            => factory.iSPBLAS.Scal(x.NzCount, a, x.NzValues.VPtr, incx);

        /// <summary>
        /// Scales the non-zero elements of a sparse complex vector by a complex scalar value.
        /// </summary>
        /// <param name="x">The sparse vector containing complex values to be scaled.</param>
        /// <param name="a">The complex scalar multiplier.</param>
        /// <param name="incx">The increment for indexing the vector (default is 1).</param>
        public static void Scal(SPVect<Cplx> x, Cplx a, long incx = 1)
            => factory.iSPBLAS.Scal(x.NzCount, &a, x.NzValues.VPtr, incx);


        // obsolete ...
        /// <summary>
        /// scales sparse vector x by constant a
        /// x := a * x
        /// </summary>
        /// <typeparam name="T"> SPVector[double] </typeparam>
        /// <param name="x"> sparse vector x </param>
        /// <param name="a"> constant a </param>
        public static void ScaleOnD<T>(ref T x, double a)
            where T : SPVector<double>
            => factory.iSPBLAS.ScalD(x.NzCount, a, ref x);

        /// <summary>
        /// scales sparse vector x by constant a
        /// x := a * x
        /// </summary>
        /// <typeparam name="T"> SPVector[Complex] </typeparam>
        /// <param name="x"> sparse vector x </param>
        /// <param name="a"> constant a </param>
        public static void ScaleOnZ<T>(ref T x, Complex a)
            where T : SPVector<Complex>
            => factory.iSPBLAS.ScalZ(x.NzCount, a, ref x);

        /// <summary>
        /// scales sparse vector x by constant a
        /// x := a * x
        /// </summary>
        /// <typeparam name="T"> SPVector[Complex] </typeparam>
        /// <param name="x"> sparse vector x </param>
        /// <param name="a"> constant a </param>
        public static void ScaleOnZ<T>(ref T x, double a)
            where T : SPVector<Complex>
            => factory.iSPBLAS.ScalZd(x.NzCount, a, ref x);


        /// <summary>
        /// scales vector x by constant a
        /// x := a * x
        /// </summary>
        /// <param name="x"> vector x </param>
        /// <param name="a"> constant a </param>
        public static void ScaleOn(ref VectorDi x, double a)
            => ScaleOnD(ref x, a);

        /// <summary>
        /// scales vector x by constant a
        /// x := a * x
        /// </summary>
        /// <param name="x"> vector x </param>
        /// <param name="a"> constant a </param>
        public static void ScaleOn(ref VectorZi x, Complex a)
            => ScaleOnZ(ref x, a);

        /// <summary>
        /// scales vector x by constant a
        /// x := a * x
        /// </summary>
        /// <param name="x"> vector x </param>
        /// <param name="a"> constant a </param>
        public static void ScaleOn(ref VectorZi x, double a)
            => ScaleOnZ(ref x, a);

        #endregion
        #region ---- IAmx [D/Z] ----

        /// <summary>
        /// Finds the index of the element with the largest absolute value in a sparse real vector.
        /// </summary>
        /// <param name="x">The sparse vector containing real values.</param>
        /// <param name="incx">The increment for indexing the vector (default is 1).</param>
        /// <returns>The index of the element with the largest absolute value among the non-zero elements.</returns>
        public static long IAmx(SPVect<Real> x, long incx = 1)
            => factory.iSPBLAS.Iamax(x.NzCount, x.NzValues.DPtr, incx);

        /// <summary>
        /// Finds the index of the element with the largest absolute value in a sparse complex vector.
        /// </summary>
        /// <param name="x">The sparse vector containing complex values.</param>
        /// <param name="incx">The increment for indexing the vector (default is 1).</param>
        /// <returns>The index of the element with the largest absolute value among the non-zero elements.</returns>
        public static long IAmx(SPVect<Cplx> x, long incx = 1)
            => factory.iSPBLAS.Iamax(x.NzCount, x.NzValues.VPtr, incx);


        // obsolete ...
        /// <summary>
        /// finds the index of the element with the largest absolute value
        /// </summary>
        /// <typeparam name="T"> SPVector[double] </typeparam>
        /// <param name="x"> sparse vector x </param>
        /// <returns> index of the element with largest absolute value </returns>
        public static long IAmxD<T>(T x) where T : SPVector<double>
            => factory.iSPBLAS.IamaxD(x.NzCount, x);

        /// <summary>
        /// finds the index of the element with the largest absolute value
        /// </summary>
        /// <typeparam name="T"> SPVector[Complex] </typeparam>
        /// <param name="x"> sparse vector x </param>
        /// <returns> index of the element with largest absolute value </returns>
        public static long IAmxZ<T>(T x) where T : SPVector<Complex>
            => factory.iSPBLAS.IamaxZ(x.NzCount, x);


        /// <summary>
        /// finds the index of the element with the largest absolute value
        /// </summary>
        /// <param name="x"> sparse vector x </param>
        /// <returns> index of the element with largest absolute value </returns>
        public static long IAmx(VectorDi x)
            => IAmxD(x);

        /// <summary>
        /// finds the index of the element with the largest absolute value
        /// </summary>
        /// <param name="x"> sparse vector x </param>
        /// <returns> index of the element with largest absolute value </returns>
        public static long IAmx(VectorZi x)
            => IAmxZ(x);

        #endregion
        #region ---- IAmn [D/Z] ----

        /// <summary>
        /// Finds the index of the element with the smallest absolute value in a sparse real vector.
        /// </summary>
        /// <param name="x">The sparse vector containing real values.</param>
        /// <param name="incx">The increment for indexing the vector (default is 1).</param>
        /// <returns>The index of the element with the smallest absolute value among the non-zero elements.</returns>
        public static long IAmn(SPVect<Real> x, long incx = 1)
            => factory.iSPBLAS.Iamin(x.NzCount, x.NzValues.DPtr, incx);

        /// <summary>
        /// Finds the index of the element with the smallest absolute value in a sparse complex vector.
        /// </summary>
        /// <param name="x">The sparse vector containing complex values.</param>
        /// <param name="incx">The increment for indexing the vector (default is 1).</param>
        /// <returns>The index of the element with the smallest absolute value among the non-zero elements.</returns>
        public static long IAmn(SPVect<Cplx> x, long incx = 1)
            => factory.iSPBLAS.Iamin(x.NzCount, x.NzValues.VPtr, incx);


        // obsolete ...
        /// <summary>
        /// finds the index of the element with the smallest absolute value
        /// </summary>
        /// <typeparam name="T"> SPVector[double] </typeparam>
        /// <param name="x"> sparse vector x </param>
        /// <returns> index of the element with smallest absolute value </returns>
        public static long IAmnD<T>(T x) where T : SPVector<double>
            => factory.iSPBLAS.IaminD(x.NzCount, x);

        /// <summary>
        /// finds the index of the element with the smallest absolute value
        /// </summary>
        /// <typeparam name="T"> SPVector[Complex] </typeparam>
        /// <param name="x"> sparse vector x </param>
        /// <returns> index of the element with smallest absolute value </returns>
        public static long IAmnZ<T>(T x) where T : SPVector<Complex>
            => factory.iSPBLAS.IaminZ(x.NzCount, x);


        /// <summary>
        /// finds the index of the element with the smallest absolute value
        /// </summary>
        /// <param name="x"> sparse vector x </param>
        /// <returns> index of the element with smallest absolute value </returns>
        public static long IAmn(VectorDi x)
            => IAmnD(x);

        /// <summary>
        /// finds the index of the element with the smallest absolute value
        /// </summary>
        /// <param name="x"> sparse vector x </param>
        /// <returns> index of the element with smallest absolute value </returns>
        public static long IAmn(VectorZi x)
            => IAmnZ(x);

        #endregion

        #region ---- AddTo [D/Z] ----

        /// <summary>
        /// Adds a scalar multiple of a compressed sparse real vector <paramref name="x"/> to a dense real vector <paramref name="y"/>.
        /// Computes y := a * x + y, where only the non-zero elements of <paramref name="x"/> are used.
        /// </summary>
        /// <param name="x">The input sparse vector containing real values.</param>
        /// <param name="y">The dense vector to which the scaled sparse vector will be added (passed by reference).</param>
        /// <param name="a">The scalar multiplier (default is 1.0).</param>
        public static void AddTo(SPVect<Real> x,
            ref Vect<Real> y, Real a = 1.0)
            => factory.iSPBLAS.Axpy(x.NzCount, a,
                x.NzValues.DPtr, (long*)x.NzIndices.DataPtr,
                y.DPtr);

        /// <summary>
        /// Adds a scalar multiple of a compressed sparse complex vector <paramref name="x"/> to a dense complex vector <paramref name="y"/>.
        /// Computes y := a * x + y, where only the non-zero elements of <paramref name="x"/> are used.
        /// </summary>
        /// <param name="x">The input sparse vector containing complex values.</param>
        /// <param name="y">The dense vector to which the scaled sparse vector will be added (passed by reference).</param>
        /// <param name="a">The complex scalar multiplier (default is <see cref="Cplx.One"/>).</param>
        public static void AddTo(SPVect<Cplx> x,
            ref Vect<Cplx> y, Cplx a = default)
        {
            if (a == default) { a = Cplx.One; }
            factory.iSPBLAS.Axpy(x.NzCount, &a,
                x.NzValues.VPtr, (long*)x.NzIndices.DataPtr,
                y.VPtr);
        }


        // obsolete ...
        /// <summary>
        /// adds a scalar multiple of compressed sparse vector 
        /// to a full-storage vector y := a * x + y
        /// </summary>
        /// <typeparam name="T1"> SPVector[double] </typeparam>
        /// <typeparam name="T2"> Vector[double] </typeparam>
        /// <param name="x"> sparse vector x </param>
        /// <param name="y"> dense vector y </param>
        /// <param name="a"> scalar a </param>
        public static void AddToD<T1, T2>(T1 x, ref T2 y,
            double a = 1.0) 
            where T1 : SPVector<double>
            where T2 : Vector<double>
        {
            if (x.Count != y.Count) { throw new ArgumentException($"Vector lengths not equal"); }
            factory.iSPBLAS.AxpyD(x.NzCount, a, x, ref y);
        }

        /// <summary>
        /// adds a scalar multiple of compressed sparse vector 
        /// to a full-storage vector y := a * x + y
        /// </summary>
        /// <typeparam name="T1"> SPVector[Complex] </typeparam>
        /// <typeparam name="T2"> Vector[Complex] </typeparam>
        /// <param name="x"> sparse vector x </param>
        /// <param name="y"> dense vector y </param>
        /// <param name="aRe"> real-part of scalar a </param>
        /// <param name="aIm"> imaginary-part of scalar a </param>
        public static void AddToZ<T1, T2>(T1 x, ref T2 y,
            double aRe = 1.0, double aIm = 0.0)
            where T1 : SPVector<Complex>
            where T2 : Vector<Complex>
        {
            if (x.Count != y.Count) { throw new ArgumentException($"Vector lengths not equal"); }
            Complex a = new(aRe, aIm);
            factory.iSPBLAS.AxpyZ(x.NzCount, a, x, ref y);
        }

        /// <summary>
        /// adds vector x to vector y
        /// y := x + y
        /// </summary>
        /// <param name="x"> sparse vector x </param>
        /// <param name="y"> dense vector y (to be overwritten) </param>
        public static void AddTo(VectorDi x, ref VectorD y)
            => AddToD(x, ref y);

        /// <summary>
        /// adds vector x to vector y
        /// y := x + y
        /// </summary>
        /// <param name="x"> sparse vector x </param>
        /// <param name="y"> dense vector y (to be overwritten) </param>
        public static void AddTo(VectorZi x, ref VectorZ y)
            => AddToZ(x, ref y);

        #endregion
        #region ---- SubFm [D/Z] ----

        /// <summary>
        /// subtracts a scalar multiple of compressed sparse vector y 
        /// from a full-storage vector x
        /// x := x - a * y
        /// </summary>
        /// <typeparam name="T1"> Vector[double] </typeparam>
        /// <typeparam name="T2"> SPVector[double] </typeparam>
        /// <param name="x"> dense vector x </param>
        /// <param name="y"> sparse vector y </param>
        /// <param name="a"> scalar a </param>
        public static void SubFmD<T1, T2>(ref T1 x, T2 y,
            double a = 1.0)
            where T1 : Vector<double>
            where T2 : SPVector<double>
        {
            if (x.Count != y.Count) { throw new ArgumentException($"Vector lengths not equal"); }
            factory.iSPBLAS.AxpyD(y.NzCount, -a, y, ref x);
        }

        /// <summary>
        /// subtracts a scalar multiple of compressed sparse vector y 
        /// from a full-storage vector x
        /// x := x - a * y
        /// </summary>
        /// <typeparam name="T1"> Vector[double] </typeparam>
        /// <typeparam name="T2"> SPVector[double] </typeparam>
        /// <param name="x"> dense vector x </param>
        /// <param name="y"> sparse vector y </param>
        /// <param name="aRe"> real-part of scalar a </param>
        /// <param name="aIm"> iamginary-part of scalar a </param>
        public static void SubFmZ<T1, T2>(ref T1 x, T2 y,
            double aRe = 1.0, double aIm = 0.0) 
            where T1 : Vector<Complex>
            where T2 : SPVector<Complex>
        {
            if (x.Count != y.Count) { throw new ArgumentException($"Vector lengths not equal"); }
            Complex a = new(aRe, aIm);
            factory.iSPBLAS.AxpyZ(y.NzCount, -a, y, ref x);
        }

        /// <summary>
        /// subtracts vector y from vector x
        /// x := x - y
        /// </summary>
        /// <param name="x"> dense vector x (to be overwritten) </param>
        /// <param name="y"> sparse vector y </param>
        public static void SubFm(ref VectorD x, VectorDi y)
            => SubFmD(ref x, y);

        /// <summary>
        /// subtracts vector y from vector x
        /// x := x - y
        /// </summary>
        /// <param name="x"> dense vector x (to be overwritten) </param>
        /// <param name="y"> sparse vector y </param>
        public static void SubFm(ref VectorZ x, VectorZi y)
            => SubFmZ(ref x, y);

        #endregion
        #region ---- Dot [D/Z] ----

        /// <summary>
        /// Computes the dot product of a sparse real vector and a dense real vector.
        /// The result is the sum of the products of the non-zero elements of <paramref name="x"/>
        /// and the corresponding elements in <paramref name="y"/> at the indices specified by <paramref name="x"/>.
        /// res = x[0]*y[indx[0]] + x[1]*y[indx[1]] + ... + x[nz-1]*y[indx[nz-1]]
        /// </summary>
        /// <param name="x">The sparse vector containing real values and their indices.</param>
        /// <param name="y">The dense vector containing real values.</param>
        /// <returns>The dot product of the sparse and dense vectors.</returns>
        public static double Dot(SPVect<Real> x, Vect<Real> y)
            => factory.iSPBLAS.Dot(x.NzCount,
                x.NzValues.DPtr, (long*)x.NzIndices.DataPtr,
                y.DPtr);

        /// <summary>
        /// Computes the dot product of a sparse complex vector and a dense complex vector.
        /// The result is the sum of the products of the non-zero elements of <paramref name="x"/>
        /// and the corresponding elements in <paramref name="y"/> at the indices specified by <paramref name="x"/>.
        /// res = x[0]*y[indx[0]] + x[1]*y[indx[1]] + ... + x[nz-1]*y[indx[nz-1]]
        /// </summary>
        /// <param name="x">The sparse vector containing complex values and their indices.</param>
        /// <param name="y">The dense vector containing complex values.</param>
        /// <returns>The dot product of the sparse and dense complex vectors.</returns>
        public static Cplx Dot(SPVect<Cplx> x, Vect<Cplx> y)
        {
            Cplx dotu;
            factory.iSPBLAS.Dot(x.NzCount,
                x.NzValues.VPtr, (long*)x.NzIndices.DataPtr,
                y.VPtr, &dotu);
            return dotu;
        }
            

        // obsolete ...
        /// <summary>
        /// computes a vector-vector dot product
        /// res = x[0]*y[indx[0]] + x[1]*y[indx[1]] +...+ x[nz-1]*y[indx[nz-1]]
        /// </summary>
        /// <typeparam name="T1"> SPVector[double] </typeparam>
        /// <typeparam name="T2"> Vector[double] </typeparam>
        /// <param name="x"> sparse vector x </param>
        /// <param name="y"> dense vector y </param>
        public static double DotD<T1, T2>(T1 x, T2 y)
            where T1 : SPVector<double>
            where T2 : Vector<double>
        {
            if (x.Count != y.Count) { throw new ArgumentException($"Vector lengths not equal"); }
            return factory.iSPBLAS.DotD(x.NzCount, x, y);
        }

        /// <summary>
        /// computes a vector-vector dot product
        /// res = x[0]*y[indx[0]] + x[1]*y[indx[1]] +...+ x[nz-1]*y[indx[nz-1]]
        /// </summary>
        /// <typeparam name="T1"> SPVector[Complex] </typeparam>
        /// <typeparam name="T2"> Vector[Complex] </typeparam>
        /// <param name="x"> sparse vector x </param>
        /// <param name="y"> dense vector y </param>
        public static Complex DotZ<T1, T2>(T1 x, T2 y)
            where T1 : SPVector<Complex>
            where T2 : Vector<Complex>
        {
            if (x.Count != y.Count) { throw new ArgumentException($"Vector lengths not equal"); }
            return factory.iSPBLAS.DotZ(x.NzCount, x, y);
        }

        /// <summary>
        /// computes a vector-vector dot product
        /// </summary>
        /// <param name="x"> sparse vector x </param>
        /// <param name="y"> dense vector y </param>
        /// <returns> dot product result </returns>
        public static double Dot(VectorDi x, VectorD y)
            => DotD(x, y);

        /// <summary>
        /// computes a vector-vector dot product
        /// </summary>
        /// <param name="x"> sparse vector x </param>
        /// <param name="y"> dense vector y </param>
        /// <returns> dot product result </returns>
        public static Complex Dot(VectorZi x, VectorZ y)
            => DotZ(x, y);

        #endregion
        #region ---- Dotc [Z] ----

        /// <summary>
        /// Computes the dot product of a conjugated sparse complex vector <paramref name="x"/> 
        /// and a dense complex vector <paramref name="y"/>.
        /// <para>
        /// The result is the sum of the products of the conjugate of each non-zero element of <paramref name="x"/>
        /// and the corresponding element in <paramref name="y"/> at the indices specified by <paramref name="x"/>.
        /// </para>
        /// <para>
        /// res = conj(x[0])*y[indx[0]] + conj(x[1])*y[indx[1]] + ... + conj(x[nz-1])*y[indx[nz-1]]
        /// </para>
        /// </summary>
        /// <param name="x">The sparse vector containing complex values (to be conjugated).</param>
        /// <param name="y">The dense vector containing complex values.</param>
        /// <returns>The dot product of the conjugated sparse vector and the dense vector.</returns>
        public static Cplx Dotc(SPVect<Cplx> x, Vect<Cplx> y)
        {
            Cplx dotc;
            factory.iSPBLAS.Dotc(x.NzCount,
                x.NzValues.VPtr, (long*)x.NzIndices.DataPtr,
                y.VPtr, &dotc);
            return dotc;
        }


        // obsolete ...
        /// <summary>
        /// computes a dot product of a conjugated vector with another vector
        /// res = conj(x[0])*y[0] + ... + conj(x[n-1])*y[n-1]
        /// </summary>
        /// <typeparam name="T1"> SPVector[Complex] </typeparam>
        /// <typeparam name="T2"> Vector[Complex] </typeparam>
        /// <param name="x"> sparse vector x </param>
        /// <param name="y"> dense vector y </param>
        public static Complex DotcZ<T1, T2>(T1 x, T2 y)
            where T1 : SPVector<Complex>
            where T2 : Vector<Complex>
        {
            if (x.Count != y.Count) { throw new ArgumentException($"Vector lengths not equal"); }
            return factory.iSPBLAS.DotcZ(x.NzCount, x, y);
        }

        /// <summary>
        /// computes a dot product of a conjugated vector 
        /// with another vector
        /// </summary>
        /// <param name="x"> sparse vector x, to be conjugated </param>
        /// <param name="y"> dense vector y </param>
        /// <returns> dot product result </returns>
        public static Complex Dotc(VectorZi x, VectorZ y)
            => DotcZ(x, y);

        #endregion
        #region ---- Rot [D] ----

        /// <summary>
        /// Performs a Givens rotation of points in the plane for a sparse real vector <paramref name="x"/> and a dense real vector <paramref name="y"/>.
        /// For each non-zero element in <paramref name="x"/>, updates both <paramref name="x"/> and <paramref name="y"/> as follows:
        /// <code>
        /// x[i] = c * x[i] + s * y[indx[i]]
        /// y[indx[i]] = c * y[indx[i]] - s * x[i]
        /// </code>
        /// </summary>
        /// <param name="x">The sparse vector containing real values to be rotated (passed by reference).</param>
        /// <param name="y">The dense vector to be rotated (passed by reference).</param>
        /// <param name="c">The cosine component of the rotation.</param>
        /// <param name="s">The sine component of the rotation.</param>
        public static void Rot(ref SPVect<Real> x, ref Vect<Real> y,
            Real c, Real s)
            => factory.iSPBLAS.Rot(x.NzCount,
                x.NzValues.DPtr, (long*)x.NzIndices.DataPtr,
                y.DPtr, c, s);


        // obsolete ...
        /// <summary>
        /// performs rotation of points in the plane
        /// x[i] = c*x[i] + s*y[indx[i]]
        /// y[indx[i]] = c*y[indx[i]] - s*x[i]
        /// </summary>
        /// <typeparam name="T1"> SPVector[double] </typeparam>
        /// <typeparam name="T2"> Vector[double] </typeparam>
        /// <param name="x"> array x (replaced by c*x+s*y) </param>
        /// <param name="y"> array y (replaced by c*y-s*x)</param>
        /// <param name="c"> constant c </param>
        /// <param name="s"> constant s </param>
        public static void RotD<T1, T2>(ref T1 x, ref T2 y, double c, double s)
            where T1 : SPVector<double>
            where T2 : Vector<double>
        {
            if (x.Count != y.Count) { throw new ArgumentException($"Vector lengths not equal"); }
            factory.iSPBLAS.RotD(x.NzCount, ref x, ref y, c, s);
        }

        /// <summary>
        /// performs rotation of points in the plane
        /// xi = c*xi + s*yi
        /// yi = c*yi - s*xi
        /// </summary>
        /// <param name="x"> vector x (replaced by c*x+s*y) </param>
        /// <param name="y"> vector y (replaced by c*y-s*x)</param>
        /// <param name="c"> constant c </param>
        /// <param name="s"> constant s </param>
        public static void Rotation(ref VectorDi x, ref VectorD y,
            double c, double s)
            => RotD(ref x, ref y, c, s);

        #endregion

        #region ---- Gthr [D/Z] ----

        /// <summary>
        /// Gathers elements from a dense real vector <paramref name="y"/> into a sparse real vector <paramref name="x"/>,
        /// using the indices specified by <paramref name="x.NzIndices"/>.
        /// For each i in [0, x.NzCount), sets x.NzValues[i] = y[x.NzIndices[i]].
        /// </summary>
        /// <param name="y">The dense real vector to gather values from.</param>
        /// <param name="x">The sparse real vector to fill with gathered values (passed by reference).</param>
        public static void Gthr(Vect<Real> y, ref SPVect<Real> x)
            => factory.iSPBLAS.Gthr(x.NzCount, y.DPtr,
                x.NzValues.DPtr, (long*)x.NzIndices.DataPtr);

        /// <summary>
        /// Gathers elements from a dense complex vector <paramref name="y"/> into a sparse complex vector <paramref name="x"/>,
        /// using the indices specified by <paramref name="x.NzIndices"/>.
        /// For each i in [0, x.NzCount), sets x.NzValues[i] = y[x.NzIndices[i]].
        /// </summary>
        /// <param name="y">The dense complex vector to gather values from.</param>
        /// <param name="x">The sparse complex vector to fill with gathered values (passed by reference).</param>
        public static void Gthr(Vect<Cplx> y, ref SPVect<Cplx> x)
            => factory.iSPBLAS.Gthr(x.NzCount, y.VPtr,
                x.NzValues.VPtr, (long*)x.NzIndices.DataPtr);


        // obsolete ...
        /// <summary>
        /// gathers a full-storage sparse vector's elements into
        /// compressed form x[i] = y[indx[i]]
        /// </summary>
        /// <typeparam name="T1"> Vector[double] </typeparam>
        /// <typeparam name="T2"> SPVector[double] </typeparam>
        /// <param name="y"> dense vector y </param>
        /// <param name="x"> sparse vector x </param>
        public static void GthrD<T1, T2>(T1 y, ref T2 x)
            where T1 : Vector<double>
            where T2 : SPVector<double>
        {
            if (x.Count != y.Count) { throw new ArgumentException($"Vector lengths not equal"); }
            factory.iSPBLAS.GthrD(x.NzCount, y, ref x);
        }

        /// <summary>
        /// gathers a full-storage sparse vector's elements into
        /// compressed form x[i] = y[indx[i]]
        /// </summary>
        /// <typeparam name="T1"> Vector[Complex] </typeparam>
        /// <typeparam name="T2"> SPVector[Complex] </typeparam>
        /// <param name="y"> dense vector y </param>
        /// <param name="x"> sparse vector x </param>
        public static void GthrZ<T1, T2>(T1 y, ref T2 x)
            where T1 : Vector<Complex>
            where T2 : SPVector<Complex>
        {
            if (x.Count != y.Count) { throw new ArgumentException($"Vector lengths not equal"); }
            factory.iSPBLAS.GthrZ(x.NzCount, y, ref x);
        }

        /// <summary>
        /// gathers a full-storage sparse vector's elements into
        /// compressed form x[i] = y[indx[i]]
        /// </summary>
        /// <param name="y"> dense vector y </param>
        /// <param name="x"> sparse vector x </param>
        public static void Gthr(VectorD y, ref VectorDi x)
            => GthrD(y, ref x);

        /// <summary>
        /// gathers a full-storage sparse vector's elements into
        /// compressed form x[i] = y[indx[i]]
        /// </summary>
        /// <param name="y"> dense vector y </param>
        /// <param name="x"> sparse vector x </param>
        public static void Gthr(VectorZ y, ref VectorZi x)
            => GthrZ(y, ref x);

        #endregion
        #region ---- Gthrz [D/Z] ----

        /// <summary>
        /// Gathers a full-storage real vector's elements into compressed sparse form, replacing them by zeros in the source vector.
        /// For each i in [0, x.NzCount), sets x.NzValues[i] = y[x.NzIndices[i]] and then y[x.NzIndices[i]] = 0.
        /// </summary>
        /// <param name="y">The dense real vector to gather values from and zero out at gathered indices.</param>
        /// <param name="x">The sparse real vector to fill with gathered values (passed by reference).</param>
        public static void Gthrz(Vect<Real> y, ref SPVect<Real> x)
            => factory.iSPBLAS.Gthrz(x.NzCount, y.DPtr,
                x.NzValues.DPtr, (long*)x.NzIndices.DataPtr);

        /// <summary>
        /// Gathers a full-storage complex vector's elements into compressed sparse form, replacing them by zeros in the source vector.
        /// For each i in [0, x.NzCount), sets x.NzValues[i] = y[x.NzIndices[i]] and then y[x.NzIndices[i]] = 0.
        /// </summary>
        /// <param name="y">The dense complex vector to gather values from and zero out at gathered indices.</param>
        /// <param name="x">The sparse complex vector to fill with gathered values (passed by reference).</param>
        public static void Gthrz(Vect<Cplx> y, ref SPVect<Cplx> x)
            => factory.iSPBLAS.Gthrz(x.NzCount, y.VPtr,
                x.NzValues.VPtr, (long*)x.NzIndices.DataPtr);


        // obsolete ...
        /// <summary>
        /// gathers a full-storage sparse vector's elements into
        /// compressed form, replacing them by zeros
        /// </summary>
        /// <typeparam name="T1"> Vector[double] </typeparam>
        /// <typeparam name="T2"> SPVector[double] </typeparam>
        /// <param name="y"> dense vector y </param>
        /// <param name="x"> sparse vector x </param>
        public static void GthrzD<T1, T2>(ref T1 y, ref T2 x)
            where T1 : Vector<double>
            where T2 : SPVector<double>
        {
            if (x.Count != y.Count) { throw new ArgumentException($"Vector lengths not equal"); }
            factory.iSPBLAS.GthrzD(x.NzCount, ref y, ref x);
        }

        /// <summary>
        /// gathers a full-storage sparse vector's elements into
        /// compressed form, replacing them by zeros
        /// </summary>
        /// <typeparam name="T1"> Vector[Complex] </typeparam>
        /// <typeparam name="T2"> SPVector[Complex] </typeparam>
        /// <param name="y"> dense vector y </param>
        /// <param name="x"> sparse vector x </param>
        public static void GthrzZ<T1, T2>(ref T1 y, ref T2 x)
            where T1 : Vector<Complex>
            where T2 : SPVector<Complex>
        {
            if (x.Count != y.Count) { throw new ArgumentException($"Vector lengths not equal"); }
            factory.iSPBLAS.GthrzZ(x.NzCount, ref y, ref x);
        }

        /// <summary>
        /// gathers a full-storage sparse vector's elements into
        /// compressed form, replacing them by zeros
        /// </summary>
        /// <param name="y"> dense vector y </param>
        /// <param name="x"> sparse vector x </param>
        public static void Gthrz(ref VectorD y, ref VectorDi x)
            => GthrzD(ref y, ref x);

        /// <summary>
        /// gathers a full-storage sparse vector's elements into
        /// compressed form, replacing them by zeros
        /// </summary>
        /// <param name="y"> dense vector y </param>
        /// <param name="x"> sparse vector x </param>
        public static void Gthrz(ref VectorZ y, ref VectorZi x)
            => GthrzZ(ref y, ref x);

        #endregion
        #region ---- Sctr [D/Z] ----

        /// <summary>
        /// Converts a compressed sparse real vector <paramref name="x"/> into a full-storage dense real vector <paramref name="y"/>.
        /// For each non-zero element in <paramref name="x"/>, sets <c>y[x.NzIndices[i]] = x.NzValues[i]</c>.
        /// </summary>
        /// <param name="x">The sparse vector containing real values and their indices.</param>
        /// <param name="y">The dense vector to be updated with the non-zero values from <paramref name="x"/> (passed by reference).</param>
        public static void Sctr(SPVect<Real> x, ref Vect<Real> y)
            => factory.iSPBLAS.Sctr(x.NzCount, x.NzValues.DPtr,
                (long*)x.NzIndices.DataPtr, y.DPtr);

        /// <summary>
        /// Converts a compressed sparse complex vector <paramref name="x"/> into a full-storage dense complex vector <paramref name="y"/>.
        /// For each non-zero element in <paramref name="x"/>, sets <c>y[x.NzIndices[i]] = x.NzValues[i]</c>.
        /// </summary>
        /// <param name="x">The sparse vector containing complex values and their indices.</param>
        /// <param name="y">The dense vector to be updated with the non-zero values from <paramref name="x"/> (passed by reference).</param>
        public static void Sctr(SPVect<Cplx> x, ref Vect<Cplx> y)
            => factory.iSPBLAS.Sctr(x.NzCount, x.NzValues.VPtr,
                (long*)x.NzIndices.DataPtr, y.VPtr);


        // obsolete ...
        /// <summary>
        /// converts compressed sparse vector into full-storage form
        /// </summary>
        /// <typeparam name="T1"> SPVector[double] </typeparam>
        /// <typeparam name="T2"> Vector[double] </typeparam>
        /// <param name="x"> sparse vector x </param>
        /// <param name="y"> dense vector y </param>
        public static void SctrD<T1, T2>(T1 x, ref T2 y)
            where T1 : SPVector<double>
            where T2 : Vector<double>
        {
            if (x.Count != y.Count) { throw new ArgumentException($"Vector lengths not equal"); }
            factory.iSPBLAS.SctrD(x.NzCount, x, ref y);
        }

        /// <summary>
        /// converts compressed sparse vector into full-storage form
        /// </summary>
        /// <typeparam name="T1"> SPVector[Complex] </typeparam>
        /// <typeparam name="T2"> Vector[Complex] </typeparam>
        /// <param name="x"> sparse vector x </param>
        /// <param name="y"> dense vector y </param>
        public static void SctrZ<T1, T2>(T1 x, ref T2 y)
            where T1 : SPVector<Complex>
            where T2 : Vector<Complex>
        {
            if (x.Count != y.Count) { throw new ArgumentException($"Vector lengths not equal"); }
            factory.iSPBLAS.SctrZ(x.NzCount, x, ref y);
        }

        /// <summary>
        /// converts compressed sparse vector into full-storage form
        /// </summary>
        /// <param name="x"> sparse vector x </param>
        /// <param name="y"> dense vector y </param>
        public static void Sctr(VectorDi x, ref VectorD y)
            => SctrD(x, ref y);

        /// <summary>
        /// converts compressed sparse vector into full-storage form
        /// </summary>
        /// <param name="x"> sparse vector x </param>
        /// <param name="y"> dense vector y </param>
        public static void Sctr(VectorZi x, ref VectorZ y)
            => SctrZ(x, ref y);

        #endregion

        #endregion
        #region Sparse BLAS Level-2 [DEPRACATED]

        // MKL_DEPRECATED ...

        #endregion
        #region Sparse BLAS Level-3 [DEPRACATED]

        // MKL_DEPRECATED ...

        #endregion
        #region Sparse QR routines [D!]

        #region ---- QR ----

        /// <summary>
        /// Computes the QR decomposition for the matrix of a sparse linear system and calculates the solution A * x = b.
        /// </summary>
        /// <param name="a">The sparse matrix <paramref name="a"/> in SPMatx&lt;Real&gt; format.</param>
        /// <param name="x">The dense matrix <paramref name="x"/> to store the solution (output).</param>
        /// <param name="b">The dense matrix <paramref name="b"/> representing the right-hand side.</param>
        /// <param name="operation">Specifies the operation op() to be performed on the sparse matrix <paramref name="a"/>. Default is <see cref="SPARSE_Operation.NonTranspose"/>.</param>
        /// <param name="descr">Optional structure specifying sparse matrix properties. If null, a default general, full, non-unit matrix description is used.</param>
        /// <param name="layout">Describes the storage scheme for the dense matrix. Default is <see cref="SPARSE_Layout.RowMajor"/>.</param>
        /// <returns>The result status of the QR decomposition and solve operation as <see cref="SPARSE_Status"/>.</returns>
        public static SPARSE_Status QR(SPMatx<Real> a, 
            Matx<Real> x, Matx<Real> b,
            SPARSE_Operation operation = SPARSE_Operation.NonTranspose,
            SPARSE_MatrixDescr? descr = null,
            SPARSE_Layout layout = SPARSE_Layout.RowMajor)
        {
            // default matrix description
            SPARSE_MatrixDescr matDes = descr ?? new()
            {
                Type = SPARSE_MatrixType.General,
                Mode = SPARSE_FillMode.Full,
                Diag = SPARSE_DiagType.NonUnit
            };
            // let x and b be column vectors
            long cols = 1;
            long ldx = 1;
            long ldb = 1;
            // calls factory interface method
            return factory.iSPBLAS.QR(operation, a.Handle, matDes, layout,
                cols, x.DataPtr, ldx, b.DataPtr, ldb);
        }


        // obsolete ...
        /// <summary>
        /// computes the QR decomposition for the matrix of a sparse
        /// linear system and calculates the solution A * x = b
        /// </summary>
        /// <typeparam name="T1"> SPMatrix[double] </typeparam>
        /// <typeparam name="T2"> Vector[double] </typeparam>
        /// <param name="a"> sparse matrix a </param>
        /// <param name="x"> unknown dense vector x </param>
        /// <param name="b"> right-hand side dense vector b </param>
        /// <param name="operation"> specifies operation op() on sparse matrix a </param>
        /// <param name="descr"> structure specifying sparse matrix properties </param>
        /// <param name="layout"> describes the storage scheme for the dense matrix </param>
        /// <returns> result status </returns>
        public static SPARSE_Status QR<T1, T2>(T1 a, ref T2 x, T2 b,
            SPARSE_Operation operation = SPARSE_Operation.NonTranspose,
            SPARSE_MatrixDescr? descr = null,
            SPARSE_Layout layout = SPARSE_Layout.RowMajor)
            where T1 : SPMatrix<double>
            where T2 : Vector<double>
        {
            // default matrix description
            SPARSE_MatrixDescr matDes = descr ?? new()
            {
                Type = SPARSE_MatrixType.General,
                Mode = SPARSE_FillMode.Full,
                Diag = SPARSE_DiagType.NonUnit
            };
            long cols = 1;
            long ldx = 1;
            long ldb = 1;
            // calls factory interface method
            return factory.iSPBLAS.QR(operation, a.Handle, matDes, layout,
                cols, x.DataPtr, ldx, b.DataPtr, ldb);
        }


        /// <summary>
        /// computes the QR decomposition for the matrix of a sparse
        /// linear system and calculates the solution A * x = b
        /// </summary>
        /// <param name="a"> sparse matrix a </param>
        /// <param name="x"> unknown dense vector x </param>
        /// <param name="b"> right-hand side dense vector b </param>
        /// <param name="operation"> specifies operation op() on sparse matrix a </param>
        /// <param name="descr"> structure specifying sparse matrix properties </param>
        /// <param name="layout"> describes the storage scheme for the dense matrix </param>
        /// <returns> result status </returns>
        public static SPARSE_Status QR(MatDi a, ref VectorDi x, VectorDi b,
            SPARSE_Operation operation = SPARSE_Operation.NonTranspose,
            SPARSE_MatrixDescr? descr = null,
            SPARSE_Layout layout = SPARSE_Layout.RowMajor)
            => QR(a, ref x, b, operation, descr, layout);

        #endregion
        #region ---- set hint ----

        /// <summary>
        /// sets the hints for sparse matrix QR decomposition 
        /// </summary>
        /// <typeparam name="T"> SPMatrix[double] </typeparam>
        /// <param name="a"> sparse matrix a </param>
        /// <param name="hint"> value specifying whether to use pivoting </param>
        /// <returns> result status </returns>
        public static SPARSE_Status QR_SetHintD<T>(T a, SPARSE_QRHint hint)
            where T : SPMatrix<double>
            => factory.iSPBLAS.QR_SetHint(a.Handle, hint);


        /// <summary>
        /// sets the hints for sparse matrix QR decomposition 
        /// </summary>
        /// <param name="a"> sparse matrix a </param>
        /// <param name="hint"> value specifying whether to use pivoting </param>
        /// <returns> result status </returns>
        public static SPARSE_Status QR_SetHint(MatDi a, SPARSE_QRHint hint)
            => QR_SetHintD(a, hint);

        #endregion
        #region ---- reorder ----

        /// <summary>
        /// reordering step of SPARSE QR solver
        /// </summary>
        /// <typeparam name="T"> SPMatrix[double] </typeparam>
        /// <param name="a"> sparse matrix a </param>
        /// <param name="descr"> structure specifying sparse matrix properties </param>
        /// <returns> result status </returns>
        public static SPARSE_Status QR_ReorderD<T>(ref T a, SPARSE_MatrixDescr descr)
            where T : SPMatrix<double>
            => factory.iSPBLAS.QR_Reorder(a.Handle, descr);


        /// <summary>
        /// reordering step of SPARSE QR solver
        /// </summary>
        /// <param name="a"> sparse matrix a </param>
        /// <param name="descr"> structure specifying sparse matrix properties </param>
        /// <returns> result status </returns>
        public static SPARSE_Status QR_Reorder(MatDi a, SPARSE_MatrixDescr descr)
            => QR_ReorderD(ref a, descr);

        #endregion
        #region ---- factorize ----

        /// <summary>
        /// factorization step of SPARSE QR solver
        /// </summary>
        /// <typeparam name="T1"> SPMatrix[double] </typeparam>
        /// <typeparam name="T2"> Vector[double] </typeparam>
        /// <param name="a"> sparse matrix a </param>
        /// <param name="altValues"> array with alternative values </param>
        /// <returns> result status </returns>
        public static SPARSE_Status QR_FactorizeD<T1, T2>(ref T1 a, 
            T2? altValues = null)
            where T1 : SPMatrix<double>
            where T2 : Vector<double>
        {
            IntPtr p = altValues == null? IntPtr.Zero : altValues.DataPtr;
            return factory.iSPBLAS.QR_Factorize(a.Handle, p);
        }            


        /// <summary>
        /// factorization step of SPARSE QR solver
        /// </summary>
        /// <param name="a"> sparse matrix a </param>
        /// <param name="altValues"> array with alternative values </param>
        /// <returns> result status </returns>
        public static SPARSE_Status QR_Factorize(MatDi a, 
            VectorD? altValues = null)
            => QR_FactorizeD(ref a, altValues);

        #endregion
        #region ---- solve ----

        /// <summary>
        /// solving step of SPARSE QR solver
        /// </summary>
        /// <typeparam name="T1"> SPMatrix[double] </typeparam>
        /// <typeparam name="T2"> Vector[double] </typeparam>
        /// <param name="a"> sparse matrix a </param>
        /// <param name="x"> unknown dense vector x </param>
        /// <param name="b"> right-hand side dense vector b </param>
        /// <param name="operation"> specifies operation op() on sparse matrix a </param>
        /// <param name="altValues"> array with alternative values </param>
        /// <param name="layout"> describes the storage scheme for the dense matrix </param>
        /// <returns> result status </returns>
        public static SPARSE_Status QR_SolveD<T1, T2>(T1 a, ref T2 x, T2 b,
            SPARSE_Operation operation = SPARSE_Operation.NonTranspose,
            T2? altValues = null,
            SPARSE_Layout layout = SPARSE_Layout.RowMajor)
            where T1 : SPMatrix<double>
            where T2 : Vector<double>
        {
            IntPtr p = altValues == null ? IntPtr.Zero : altValues.DataPtr;
            long cols = 1;
            long ldx = 1;
            long ldb = 1;
            // calls factory interface method
            return factory.iSPBLAS.QR_Solve(operation, a.Handle,
                p, layout, cols, x.DataPtr, ldx, b.DataPtr, ldb);
        }


        /// <summary>
        /// solving step of SPARSE QR solver
        /// </summary>
        /// <param name="a"> sparse matrix a </param>
        /// <param name="x"> unknown dense vector x </param>
        /// <param name="b"> right-hand side dense vector b </param>
        /// <param name="operation"> specifies operation op() on sparse matrix a </param>
        /// <param name="altValues"> array with alternative values </param>
        /// <param name="layout"> describes the storage scheme for the dense matrix </param>
        /// <returns> result status </returns>
        public static SPARSE_Status QR_Solve(MatDi a, ref VectorD x, VectorD b,
            SPARSE_Operation operation = SPARSE_Operation.NonTranspose,
            VectorD? altValues = null,
            SPARSE_Layout layout = SPARSE_Layout.RowMajor)
            => QR_SolveD(a, ref x, b, operation, altValues, layout);

        #endregion
        // ---- qmult ... ----
        // ---- rsolve ... ----

        #endregion
        #region Sparse BLAS inspector-executer ...

        #region ---- CreateCOO [D/Z] ----

        /// <summary>
        /// creates a sparse matrix in COO format 
        /// </summary>
        /// <typeparam name="T"> SPMatrix[double] </typeparam>
        /// <typeparam name="T1"> ArrayBase[long] </typeparam>
        /// <typeparam name="T2"> ArrayBase[double] </typeparam>
        /// <param name="a"> sparse matrix a </param>
        /// <param name="row_indx"> row indices of the non-zero elements </param>
        /// <param name="col_indx"> column indices of the non-zero elements </param>
        /// <param name="values"> values of the non-zero elements </param>
        /// <param name="indexing"> zero-based (C-style) or one-based (Fortran-style) </param>
        /// <returns> result status </returns>
        internal static SPARSE_Status CreateCOOD<T, T1, T2>(ref T a, 
            T1 row_indx, T1 col_indx, T2 values,
            SPARSE_IndexBase indexing = SPARSE_IndexBase.ZeroBase)
            where T : SPMatrix<double>
            where T1 : DenseArrayBase<long>
            where T2 : DenseArrayBase<double>
        {
            IntPtr handle = a.Handle;
            a.Status = factory.iSPBLAS.CreateCOOD(ref handle, indexing, 
                a.Rows, a.Cols, a.NzCount, row_indx, col_indx, values);
            a.Handle = handle;
            return a.Status;
        }

        /// <summary>
        /// creates a sparse matrix in COO format 
        /// </summary>
        /// <typeparam name="T"> SPMatrix[Complex] </typeparam>
        /// <typeparam name="T1"> ArrayBase[long] </typeparam>
        /// <typeparam name="T2"> ArrayBase[Complex] </typeparam>
        /// <param name="a"> sparse matrix a </param>
        /// <param name="row_indx"> row indices of the non-zero elements </param>
        /// <param name="col_indx"> column indices of the non-zero elements </param>
        /// <param name="values"> values of the non-zero elements </param>
        /// <param name="indexing"> zero-based (C-style) or one-based (Fortran-style) </param>
        /// <returns> result status </returns>
        internal static SPARSE_Status CreateCOOZ<T, T1, T2>(ref T a,
            T1 row_indx, T1 col_indx, T2 values,
            SPARSE_IndexBase indexing = SPARSE_IndexBase.ZeroBase)
            where T : SPMatrix<Complex>
            where T1 : DenseArrayBase<long>
            where T2 : DenseArrayBase<Complex>
        {
            IntPtr handle = a.Handle;
            a.Status = factory.iSPBLAS.CreateCOOZ(ref handle, indexing,
                a.Rows, a.Cols, a.NzCount, row_indx, col_indx, values);
            a.Handle = handle;
            return a.Status;
        }

        #endregion
        #region ---- CreateCSR [D/Z] ----

        /// <summary>
        /// creates a sparse matrix in CSR format 
        /// </summary>
        /// <typeparam name="T"> SPMatrix[double] </typeparam>
        /// <typeparam name="T1"> ArrayBase[long] </typeparam>
        /// <typeparam name="T2"> ArrayBase[double] </typeparam>
        /// <param name="a"> sparse matrix a </param>
        /// <param name="row_ptr"> row start/end indices of the non-zero elements </param>
        /// <param name="col_indx"> column indices of the non-zero elements </param>
        /// <param name="values"> values of the non-zero elements </param>
        /// <param name="indexing"> zero-based (C-style) or one-based (Fortran-style) </param>
        /// <returns> result status </returns>
        internal static SPARSE_Status CreateCSRD<T, T1, T2>(ref T a,
            T1 row_ptr, T1 col_indx, T2 values,
            SPARSE_IndexBase indexing = SPARSE_IndexBase.ZeroBase)
            where T : SPMatrix<double>
            where T1 : DenseArrayBase<long>
            where T2 : DenseArrayBase<double>
        {
            IntPtr handle = a.Handle;
            a.Status = factory.iSPBLAS.CreateCSRD(ref handle, indexing,
                a.Rows, a.Cols, row_ptr, col_indx, values);
            a.Handle = handle;
            return a.Status;
        }

        /// <summary>
        /// creates a sparse matrix in CSR format 
        /// </summary>
        /// <typeparam name="T"> SPMatrix[Complex] </typeparam>
        /// <typeparam name="T1"> ArrayBase[long] </typeparam>
        /// <typeparam name="T2"> ArrayBase[Complex] </typeparam>
        /// <param name="a"> sparse matrix a </param>
        /// <param name="row_ptr"> row start/end indices of the non-zero elements </param>
        /// <param name="col_indx"> column indices of the non-zero elements </param>
        /// <param name="values"> values of the non-zero elements </param>
        /// <param name="indexing"> zero-based (C-style) or one-based (Fortran-style) </param>
        /// <returns> result status </returns>
        internal static SPARSE_Status CreateCSRZ<T, T1, T2>(ref T a,
            T1 row_ptr, T1 col_indx, T2 values,
            SPARSE_IndexBase indexing = SPARSE_IndexBase.ZeroBase)
            where T : SPMatrix<Complex>
            where T1 : DenseArrayBase<long>
            where T2 : DenseArrayBase<Complex>
        {
            IntPtr handle = a.Handle;
            a.Status = factory.iSPBLAS.CreateCSRZ(ref handle, indexing,
                a.Rows, a.Cols, row_ptr, col_indx, values);
            a.Handle = handle;
            return a.Status;
        }

        #endregion
        #region ---- CreateCSC [D/Z] ----

        /// <summary>
        /// creates a sparse matrix in CSC format 
        /// </summary>
        /// <typeparam name="T"> SPMatrix[double] </typeparam>
        /// <typeparam name="T1"> ArrayBase[long] </typeparam>
        /// <typeparam name="T2"> ArrayBase[double] </typeparam>
        /// <param name="a"> sparse matrix a </param>
        /// <param name="col_ptr"> column start/end indices of the non-zero elements </param>
        /// <param name="row_indx"> row indices of the non-zero elements </param>
        /// <param name="values"> values of the non-zero elements </param>
        /// <param name="indexing"> zero-based (C-style) or one-based (Fortran-style) </param>
        /// <returns> result status </returns>
        internal static SPARSE_Status CreateCSCD<T, T1, T2>(ref T a,
            T1 col_ptr, T1 row_indx, T2 values,
            SPARSE_IndexBase indexing = SPARSE_IndexBase.ZeroBase)
            where T : SPMatrix<double>
            where T1 : DenseArrayBase<long>
            where T2 : DenseArrayBase<double>
        {
            IntPtr handle = a.Handle;
            a.Status = factory.iSPBLAS.CreateCSCD(ref handle, indexing,
                a.Rows, a.Cols, col_ptr, row_indx, values);
            a.Handle = handle;
            return a.Status;
        }

        /// <summary>
        /// creates a sparse matrix in CSC format 
        /// </summary>
        /// <typeparam name="T"> SPMatrix[Complex] </typeparam>
        /// <typeparam name="T1"> ArrayBase[long] </typeparam>
        /// <typeparam name="T2"> ArrayBase[Complex] </typeparam>
        /// <param name="a"> sparse matrix a </param>
        /// <param name="col_ptr"> column start/end indices of the non-zero elements </param>
        /// <param name="row_indx"> row indices of the non-zero elements </param>
        /// <param name="values"> values of the non-zero elements </param>
        /// <param name="indexing"> zero-based (C-style) or one-based (Fortran-style) </param>
        /// <returns> result status </returns>
        internal static SPARSE_Status CreateCSCZ<T, T1, T2>(ref T a,
            T1 col_ptr, T1 row_indx, T2 values,
            SPARSE_IndexBase indexing = SPARSE_IndexBase.ZeroBase)
            where T : SPMatrix<Complex>
            where T1 : DenseArrayBase<long>
            where T2 : DenseArrayBase<Complex>
        {
            IntPtr handle = a.Handle;
            a.Status = factory.iSPBLAS.CreateCSCZ(ref handle, indexing,
                a.Rows, a.Cols, col_ptr, row_indx, values);
            a.Handle = handle;
            return a.Status;
        }

        #endregion
        #region ---- Copy ----

        /// <summary>
        /// creates a copy of a sparse matrix handle
        /// </summary>
        /// <param name="source"> handle of the source sparse matrix </param>
        /// <param name="dest"> copied handle containing internal data </param>
        /// <param name="matType"> sparse matrix type </param>
        /// <param name="fillMode"> sparse matrix fill mode </param>
        /// <param name="diagType"> sparse matrix diagonal type </param>
        /// <returns> result status </returns>
        internal static SPARSE_Status Copy(IntPtr source, ref IntPtr dest,
            SPARSE_MatrixType matType = SPARSE_MatrixType.General,
            SPARSE_FillMode fillMode = SPARSE_FillMode.Full,
            SPARSE_DiagType diagType = SPARSE_DiagType.NonUnit)
        {
            SPARSE_MatrixDescr descr = new()
            {
                Type = matType,
                Mode = fillMode,
                Diag = diagType
            };
            return factory.iSPBLAS.Copy(source, descr, ref dest);
        }

        //public static SPARSE_Status Copy<T>(T source, ref T dest)

        #endregion
        #region ---- Destroy ----

        /// <summary>
        /// frees memory allocated for a sparse matrix handle
        /// </summary>
        /// <param name="a"> handle of the sparse matrix </param>
        /// <returns> result status </returns>
        internal static SPARSE_Status Destroy(IntPtr a)
            => factory.iSPBLAS.Destroy(a);

        #endregion
        #region ---- Convert ----

        /// <summary>
        /// converts internal matrix representation to CSR format
        /// </summary>
        /// <typeparam name="T"> SPMatrix[double] </typeparam>
        /// <param name="a"> sparse matrix a </param>
        /// <param name="operation"> specifies operation op() on input matrix a </param>
        /// <returns> result status </returns>
        public static SPARSE_Status Convert2CSRD<T>(ref T a,
            SPARSE_Operation operation = SPARSE_Operation.NonTranspose)
            where T : SPMatrix<double>
        {
            IntPtr handle = a.Handle;
            SPARSE_Status status = factory.iSPBLAS.ConvertCSR(handle, operation, ref handle);
            a.Handle = handle;
            return status;
        }

        /// <summary>
        /// converts internal matrix representation to CSR format
        /// </summary>
        /// <typeparam name="T"> SPMatrix[Complex] </typeparam>
        /// <param name="a"> sparse matrix a </param>
        /// <param name="operation"> specifies operation op() on input matrix a </param>
        /// <returns> result status </returns>
        public static SPARSE_Status Convert2CSRZ<T>(ref T a,
            SPARSE_Operation operation = SPARSE_Operation.NonTranspose)
            where T : SPMatrix<Complex>
        {
            IntPtr handle = a.Handle;
            SPARSE_Status status = factory.iSPBLAS.ConvertCSR(handle, operation, ref handle);
            a.Handle = handle;
            return status;
        }


        /// <summary>
        /// converts internal matrix representation to CSR format
        /// </summary>
        /// <param name="a"> sparse matrix a </param>
        /// <param name="operation"> specifies operation op() on input matrix a </param>
        /// <returns> result status </returns>
        public static SPARSE_Status Convert2CSR(MatDi a,
            SPARSE_Operation operation = SPARSE_Operation.NonTranspose)
            => Convert2CSRD(ref a, operation);

        /// <summary>
        /// converts internal matrix representation to CSR format
        /// </summary>
        /// <param name="a"> sparse matrix a </param>
        /// <param name="operation"> specifies operation op() on input matrix a </param>
        /// <returns> result status </returns>
        public static SPARSE_Status Convert2CSR(MatZi a,
            SPARSE_Operation operation = SPARSE_Operation.NonTranspose)
            => Convert2CSRZ(ref a, operation);

        #endregion
        #region ---- Export [D/Z] ----

        /// <summary>
        /// exports a sparse matrix in CSR format
        /// </summary>
        /// <param name="a"> sparse matrix handle </param>
        /// <param name="indexing"> zero-based (C-style) or one-based (Fortran-style) </param>
        /// <param name="rows"> number of rows of matrix a </param>
        /// <param name="cols"> number of cols of matrix a </param>
        /// <param name="row_start"> row start indices of the non-zero elements </param>
        /// <param name="row_end"> row end indices of the non-zero elements </param>
        /// <param name="col_indx"> column indices of the non-zero elements </param>
        /// <param name="values"> values of the non-zero elements </param>
        /// <returns> result status </returns>
        internal static SPARSE_Status ExporCSRD(IntPtr a, 
            ref SPARSE_IndexBase indexing, ref long rows, ref long cols,
            ref IntPtr row_start, ref IntPtr row_end,
            ref IntPtr col_indx, ref IntPtr values)
            => factory.iSPBLAS.ExportCSRD(a, ref indexing, ref rows, ref cols, 
                ref row_start, ref row_end, ref col_indx, ref values);

        /// <summary>
        /// exports a sparse matrix in CSR format
        /// </summary>
        /// <typeparam name="T"> SPMatrix[double] </typeparam>
        /// <param name="a"> sparse matrix handle </param>
        /// <param name="indexing"> zero-based (C-style) or one-based (Fortran-style) </param>
        /// <param name="rows"> number of rows of matrix a </param>
        /// <param name="cols"> number of cols of matrix a </param>
        /// <param name="row_start"> row start indices of the non-zero elements </param>
        /// <param name="row_end"> row end indices of the non-zero elements </param>
        /// <param name="col_indx"> column indices of the non-zero elements </param>
        /// <param name="values"> values of the non-zero elements </param>
        /// <returns> result status </returns>
        public unsafe static SPARSE_Status ExportCSRD<T>(T a,
            out SPARSE_IndexBase indexing, out long rows, out long cols,
            out Vector<long> row_start, out Vector<long> row_end,
            out Vector<long> col_indx, out Vector<double> values)
            where T : SPMatrix<double>
        {
            indexing = SPARSE_IndexBase.ZeroBase;
            rows = 0; cols = 0;
            IntPtr r = IntPtr.Zero;
            IntPtr r1 = IntPtr.Zero;
            IntPtr c = IntPtr.Zero;
            IntPtr v = IntPtr.Zero;
            
            SPARSE_Status status = ExporCSRD(a.Handle, 
                ref indexing, ref rows, ref cols,
                ref r, ref r1, ref c, ref v);

            row_start = new Vector<long>(count: rows, mode: ArrayInitMode.Malloc);
            row_end = new Vector<long>(count: rows, mode: ArrayInitMode.Malloc);
            col_indx = new Vector<long>(count: a.NzCount, mode: ArrayInitMode.Malloc);
            values = new Vector<double>(count: a.NzCount, mode: ArrayInitMode.Malloc);
            Buffer.MemoryCopy(source: r.ToPointer(), destination: row_start.VPtr,
                rows * sizeof(long), rows * sizeof(long));
            Buffer.MemoryCopy(source: r1.ToPointer(), destination: row_end.VPtr,
                rows * sizeof(long), rows * sizeof(long));
            Buffer.MemoryCopy(source: c.ToPointer(), destination: col_indx.VPtr,
                a.NzCount * sizeof(long), a.NzCount * sizeof(long));
            Buffer.MemoryCopy(source: v.ToPointer(), destination: values.VPtr,
                a.NzCount * sizeof(double), a.NzCount * sizeof(double));

            return status;
        }

        /// <summary>
        /// exports a sparse matrix in CSR format
        /// </summary>
        /// <param name="a"> sparse matrix handle </param>
        /// <param name="indexing"> zero-based (C-style) or one-based (Fortran-style) </param>
        /// <param name="rows"> number of rows of matrix a </param>
        /// <param name="cols"> number of cols of matrix a </param>
        /// <param name="row_start"> row start indices of the non-zero elements </param>
        /// <param name="row_end"> row end indices of the non-zero elements </param>
        /// <param name="col_indx"> column indices of the non-zero elements </param>
        /// <param name="values"> values of the non-zero elements </param>
        /// <returns> result status </returns>
        internal static SPARSE_Status ExporCSRZ(IntPtr a,
            ref SPARSE_IndexBase indexing, ref long rows, ref long cols,
            ref IntPtr row_start, ref IntPtr row_end,
            ref IntPtr col_indx, ref IntPtr values)
            => factory.iSPBLAS.ExportCSRD(a, ref indexing, ref rows, ref cols,
                ref row_start, ref row_end, ref col_indx, ref values);

        /// <summary>
        /// exports a sparse matrix in CSR format
        /// </summary>
        /// <typeparam name="T"> SPMatrix[Complex] </typeparam>
        /// <param name="a"> sparse matrix handle </param>
        /// <param name="indexing"> zero-based (C-style) or one-based (Fortran-style) </param>
        /// <param name="rows"> number of rows of matrix a </param>
        /// <param name="cols"> number of cols of matrix a </param>
        /// <param name="row_start"> row start indices of the non-zero elements </param>
        /// <param name="row_end"> row end indices of the non-zero elements </param>
        /// <param name="col_indx"> column indices of the non-zero elements </param>
        /// <param name="values"> values of the non-zero elements </param>
        /// <returns> result status </returns>
        public unsafe static SPARSE_Status ExportCSRZ<T>(T a,
            out SPARSE_IndexBase indexing, out long rows, out long cols,
            out Vector<long> row_start, out Vector<long> row_end,
            out Vector<long> col_indx, out Vector<Complex> values)
            where T : SPMatrix<Complex>
        {
            indexing = SPARSE_IndexBase.ZeroBase;
            rows = 0; cols = 0;
            IntPtr r = IntPtr.Zero;
            IntPtr r1 = IntPtr.Zero;
            IntPtr c = IntPtr.Zero;
            IntPtr v = IntPtr.Zero;

            SPARSE_Status status = ExporCSRD(a.Handle,
                ref indexing, ref rows, ref cols,
                ref r, ref r1, ref c, ref v);

            row_start = new Vector<long>(count: rows, mode: ArrayInitMode.Malloc);
            row_end = new Vector<long>(count: rows, mode: ArrayInitMode.Malloc);
            col_indx = new Vector<long>(count: a.NzCount, mode: ArrayInitMode.Malloc);
            values = new Vector<Complex>(count: a.NzCount, mode: ArrayInitMode.Malloc);
            Buffer.MemoryCopy(source: r.ToPointer(), destination: row_start.VPtr,
                rows * sizeof(long), rows * sizeof(long));
            Buffer.MemoryCopy(source: r1.ToPointer(), destination: row_end.VPtr,
                rows * sizeof(long), rows * sizeof(long));
            Buffer.MemoryCopy(source: c.ToPointer(), destination: col_indx.VPtr,
                a.NzCount * sizeof(long), a.NzCount * sizeof(long));
            Buffer.MemoryCopy(source: v.ToPointer(), destination: values.VPtr,
                a.NzCount * sizeof(Complex), a.NzCount * sizeof(Complex));

            return status;
        }

        #endregion
        #region ---- SetValue [D/Z] ----

        /// <summary>
        /// changes a single value of matrix in internal representation
        /// </summary>
        /// <param name="a"> sparse matrix handle </param>
        /// <param name="row"> indicates row of matrix in which to set value </param>
        /// <param name="col"> indicates column of matrix in which to set value </param>
        /// <param name="value"> target value </param>
        /// <returns> result status </returns>
        internal static SPARSE_Status SetValueD(IntPtr a, 
            long row, long col, double value)
            => factory.iSPBLAS.SetValueD(a, row, col, value);

        /// <summary>
        /// changes a single value of matrix in internal representation
        /// </summary>
        /// <param name="a"> sparse matrix handle </param>
        /// <param name="row"> indicates row of matrix in which to set value </param>
        /// <param name="col"> indicates column of matrix in which to set value </param>
        /// <param name="value"> target value </param>
        /// <returns> result status </returns>
        internal static SPARSE_Status SetValueZ(IntPtr a,
            long row, long col, Complex value)
            => factory.iSPBLAS.SetValueZ(a, row, col, value);

        #endregion
        #region ---- Order ----

        /// <summary>
        /// performs ordering of column indexes of the matrix in CSR format
        /// </summary>
        /// <param name="a"> sparse matrix handle </param>
        /// <returns> result status </returns>
        public static SPARSE_Status Order(IntPtr a)
            => factory.iSPBLAS.Order(a);

        #endregion
        // analysis routines
        // ...
        // execution routines
        #region ---- Mv [D/Z] ----


        //public static SPARSE_Status MvD<T1, T2>(SPARSE_Operation operation,
        //    double alpha, T1 a, SPARSE_MatrixDescr descr,
        //    T2 x, double beta, ref T2 y)
        //    where T1 : SPMatrix<double>
        //    where T2 : Vector<double>
        //{

        //}

        #endregion


        #endregion


        // ... obsolete ...
        #region --- WVectorDi helper ---

        /// <summary>
        /// initializes a sparse vector
        /// </summary>
        /// <param name="n"> total number of elements, including zeros </param>
        /// <param name="nnz"> number of non-zero elements </param>
        /// <returns> an empty sparse vector </returns>
        [Obsolete]
        public unsafe static WVectorDi InitWVectorDi(long n, long nnz)
            => new(n, nnz);

        /// <summary>
        /// fills an initialized sparse vector
        /// </summary>
        /// <param name="vi"> the initialized sparse vector </param>
        /// <param name="nzi"> non-zero indices </param>
        /// <param name="nzv"> non-zero values </param>
        [Obsolete]
        public unsafe static void FillWVectorDi(ref WVectorDi vi, VectorI nzi, VectorD nzv)
            => vi.SetNonZeroInfo(nzi.TPtr, nzv.SPtr, 0);

        /// <summary>
        /// generates a sparse vector by gathering the
        /// non-zero elements from a sparse vector
        /// </summary>
        /// <param name="v"> dense vector </param>
        /// <param name="nzi"> non-zero indices </param>
        /// <returns> result sparse vector </returns>
        [Obsolete]
        public unsafe static WVectorDi GatherWVectorDi(VectorD v, VectorI nzi)
            => WLinAlg.Gthr(v.Count, v.SPtr, nzi.Count, nzi.TPtr);

        /// <summary>
        /// generate a dense vector by scattering the
        /// non-zero element from a sparse vector
        /// </summary>
        /// <param name="vi"> sparse vector </param>
        /// <returns> dense vector </returns>
        [Obsolete]
        public unsafe static VectorD ScatterWVectorDi(WVectorDi vi)
        {
            VectorD v = new(vi.Count, 0.0);
            WLinAlg.Sctr(vi, v.SPtr);
            return v;
        }

        #endregion
        #region --- Sparse BLAS Level-1 ---

        /// <summary>
        /// Adds a scalar multiple of compressed sparse vector
		/// to a full-storage vector
		/// y := a*x + y
        /// </summary>
        /// <param name="xi"> input sparse vector x </param>
        /// <param name="y"> dense vector y (to be overwritten) </param>
        /// <param name="alpha"> scalar constant alpha (default = 1.0) </param>
        public unsafe static void AddTo(WVectorDi xi, ref VectorD y,
            double alpha = 1.0)
            => WLinAlg.Axpyi(xi, y.SPtr, alpha);

        /// <summary>
        /// Adds a scalar multiple of compressed sparse vector
		/// to a full-storage vector
		/// y := a*x + y
        /// </summary>
        /// <param name="xi"> input sparse vector x </param>
        /// <param name="y"> dense vector y (to be overwritten) </param>
        /// <param name="alpha"> scalar constant alpha (default = 1.0) </param>
        public unsafe static void AddTo(WVectorZi xi, ref VectorZ y,
            Complex? alpha = null)
        {
            alpha ??= Complex.One;
            WComplex a = new(re: alpha.Value.Real, im: alpha.Value.Imaginary);
            WLinAlg.Axpyi(xi, y.SPtr, a);
        }

        /// <summary>
        /// computes the dot product of a compressed sparse vector x
		/// by a full-storage vector y
		/// res = x[0]*y[indx[0]] + x[1]*y[indx[1]] +...+ x[nz-1]*y[indx[nz-1]]
        /// </summary>
        /// <param name="xi"> input sparse vector x </param>
        /// <param name="y"> input dense vector y </param>
        /// <returns> dot-product </returns>
        public unsafe static double Dot(WVectorDi xi, VectorD y)
            => WLinAlg.Doti(xi, y.SPtr);

        /// <summary>
        /// computes the dot product of a compressed sparse vector x
		/// by a full-storage vector y
		/// res = x[0]*y[indx[0]] + x[1]*y[indx[1]] +...+ x[nz-1]*y[indx[nz-1]]
        /// </summary>
        /// <param name="xi"> input sparse vector x </param>
        /// <param name="y"> input dense vector y </param>
        /// <returns> dot-product </returns>
        public unsafe static Complex Dot(WVectorZi xi, VectorZ y)
        {
            WComplex p = WLinAlg.Doti(xi, y.SPtr);
            return new Complex(p.Real, p.Imag);
        }

        /// <summary>
        /// computes the dot product of a conjugated compressed 
		/// sparse vector x by a full-storage vector y
        /// </summary>
        /// <param name="xi"> input sparse vector x (to be conjugated) </param>
        /// <param name="y"> input dense vector y </param>
        /// <returns> dot-product </returns>
        public unsafe static Complex Dotc(WVectorZi xi, VectorZ y)
        {
            WComplex p = WLinAlg.Dotci(xi, y.SPtr);
            return new Complex(p.Real, p.Imag);
        }

        /// <summary>
        /// performs rotation of point in the plane
		/// xi = c * xi + s * yi
		/// yi = c * yi - s * xi
		/// </summary>
		/// <param name="xi"> compressed sparse vector x </param>
		/// <param name="y"> dense vector y </param>
		/// <param name="c"> scalar parameter </param>
		/// <param name="s"> scalar parameter </param>
        public unsafe static void Rotate(WVectorDi xi, VectorD y, double c, double s)
            => WLinAlg.Roti(xi, y.SPtr, c, s);

        /// <summary>
        /// performs rotation of point in the plane
		/// xi = c * xi + s * yi
		/// yi = c * yi - s * xi
		/// </summary>
		/// <param name="xi"> compressed sparse vector x </param>
		/// <param name="y"> dense vector y </param>
		/// <param name="c"> scalar parameter </param>
		/// <param name="s"> scalar parameter </param>
        public unsafe static void Rotate(WVectorZi xi, VectorZ y, double c, double s)
            => WLinAlg.Roti(xi, y.SPtr, c, s);

        /// <summary>
        /// computes the sum of magnitudes of all the elements
        /// of a compressed sparse vector 
        /// res = |Re(x1)| + |Im(x1)| + |Re(x2)| + |Im(x2)|+ 
        /// ... + |Re(xn)| + |Im(xn)|
        /// </summary>
        /// <param name="xi"> input sparse vector x </param>
        /// <param name="incx"> increment in x (default = 1) </param>
        /// <returns> absolute sum of all the vector elements </returns>
        public static double AbsSum(WVectorDi xi, long incx = 1)
            => WLinAlg.Asum(xi, incx);

        /// <summary>
        /// computes the sum of magnitudes of all the elements
        /// of a compressed sparse vector 
        /// res = |Re(x1)| + |Im(x1)| + |Re(x2)| + |Im(x2)|+ 
        /// ... + |Re(xn)| + |Im(xn)|
        /// </summary>
        /// <param name="xi"> input sparse vector x </param>
        /// <param name="incx"> increment in x (default = 1) </param>
        /// <returns> absolute sum of all the vector elements </returns>
        public static double AbsSum(WVectorZi xi, long incx = 1)
            => WLinAlg.Asum(xi, incx);

        /// <summary>
        /// copies compressed sparse vector x to vector y
		/// y = x
        /// </summary>
        /// <param name="xi"> sparse vector xi </param>
		/// <param name="yi"> sparse vector yi </param>
		/// <param name="incx"> increment in x (default = 1) </param>
		/// <param name="incy"> increment in y (default = 1) </param>
        public static void Copy(WVectorDi xi, ref WVectorDi yi, 
            long incx = 1, long incy = 1)
            => WLinAlg.Copy(xi, yi, incx, incy);

        /// <summary>
        /// copies compressed sparse vector x to vector y
		/// y = x
        /// </summary>
        /// <param name="xi"> sparse vector xi </param>
		/// <param name="yi"> sparse vector yi </param>
		/// <param name="incx"> increment in x (default = 1) </param>
		/// <param name="incy"> increment in y (default = 1) </param>
        public static void Copy(WVectorZi xi, ref WVectorZi yi,
            long incx = 1, long incy = 1)
            => WLinAlg.Copy(xi, yi, incx, incy);

        /// <summary>
        /// computes the Euclidean norm of a vector
		/// res = || x ||
		/// </summary>
		/// <param name="xi"> compressed sparse vector x </param>
        /// <param name="incx"> increment (default = 1) </param>
        /// <returns> Euclidean norm </returns>
        public static double Norm(WVectorDi xi, long incx = 1)
            => WLinAlg.Nrm2(xi, incx);

        /// <summary>
        /// computes the Euclidean norm of a vector
		/// res = || x ||
		/// </summary>
		/// <param name="xi"> compressed sparse vector x </param>
        /// <param name="incx"> increment (default = 1) </param>
        /// <returns> Euclidean norm </returns>
        public static double Norm(WVectorZi xi, long incx = 1)
            => WLinAlg.Nrm2(xi, incx);

        /// <summary>
        /// computes the product of a vector by a scalar
		/// x = a*x
        /// </summary>
        /// <param name="alpha"> scalar constant multiple </param>
        /// <param name="xi"> compressed sparse vector x (to be overwritten) </param>
        /// <param name="incx"> increment (default = 1) </param>
        public static void ScaleOn(double alpha, ref WVectorDi xi, long incx = 1)
            => WLinAlg.Scal(alpha, xi, incx);

        /// <summary>
        /// computes the product of a vector by a scalar
		/// x = a*x
        /// </summary>
        /// <param name="alpha"> scalar constant multiple </param>
        /// <param name="xi"> compressed sparse vector x (to be overwritten) </param>
        /// <param name="incx"> increment (default = 1) </param>
        public static void ScaleOn(Complex alpha, ref WVectorZi xi, long incx = 1)
        {
            WComplex a = new(re: alpha.Real, im: alpha.Imaginary);
            WLinAlg.Scal(a, xi, incx);
        }

        /// <summary>
        /// finds the index of the element in the vector
		/// with maximum absolute value
		/// </summary>
		/// <param name="xi"> compressed sparse vector x </param>
		/// <param name="incx"> increment </param>
		/// <returns> index of the element </returns>
        public static long IndexMaxAbs(WVectorDi xi, long incx = 1)
            => WLinAlg.Iamax(xi, incx);

        /// <summary>
        /// finds the index of the element in the vector
		/// with maximum absolute value
		/// </summary>
		/// <param name="xi"> compressed sparse vector x </param>
		/// <param name="incx"> increment </param>
		/// <returns> index of the element </returns>
        public static long IndexMaxAbs(WVectorZi xi, long incx = 1)
            => WLinAlg.Iamax(xi, incx);

        /// <summary>
        /// finds the index of the element in the vector
		/// with minimum absolute value
		/// </summary>
		/// <param name="xi"> compressed sparse vector x </param>
		/// <param name="incx"> increment </param>
		/// <returns> index of the element </returns>
        public static long IndexMinAbs(WVectorDi xi, long incx = 1)
            => WLinAlg.Iamin(xi, incx);

        /// <summary>
        /// finds the index of the element in the vector
		/// with minimum absolute value
		/// </summary>
		/// <param name="xi"> compressed sparse vector x </param>
		/// <param name="incx"> increment </param>
		/// <returns> index of the element </returns>
        public static long IndexMinAbs(WVectorZi xi, long incx = 1)
            => WLinAlg.Iamin(xi, incx);

        #endregion

        #region --- WMatrixDi helper ---

        /// <summary>
        /// initializes a sparse matrix
        /// </summary>
        /// <param name="rows"> number of rows </param>
        /// <param name="cols"> number of columns </param>
        /// <param name="nnz"></param>
        /// <returns> result empty sparse matrix </returns>
        [Obsolete]
        public static WMatrixDi InitWMatrixDi(long rows, long cols, long nnz)
            => new(rows, cols, nnz);

        /// <summary>
        /// fills an initialized sparse vector
        /// in the CSR-format
        /// </summary>
        /// <param name="ai"> input empty sparse matrix </param>
        /// <param name="rowPtr"> indices of row start/end </param>
        /// <param name="colIdx"> column indices of non-zero elements </param>
        /// <param name="nzVal"> values of non-zero element  </param>
        [Obsolete]
        public unsafe static void FillWMatrixDiCSR(ref WMatrixDi ai,
            VectorI rowPtr, VectorI colIdx, VectorD nzVal)
            => WLinAlg.SparseCreateCSR(ai, rowPtr.TPtr, colIdx.TPtr, nzVal.SPtr);

        /// <summary>
        /// fills an initialized sparse vector
        /// in the CSC-format
        /// </summary>
        /// <param name="ai"> input empty sparse matrix </param>
        /// <param name="colPtr"> indices of column start/end </param>
        /// <param name="rowIdx"> row indices of non-zero elements </param>
        /// <param name="nzVal"> values of non-zero element  </param>
        [Obsolete]
        public unsafe static void FillWMatrixDiCSC(ref WMatrixDi ai,
            VectorI colPtr, VectorI rowIdx, VectorD nzVal)
            => WLinAlg.SparseCreateCSC(ai, colPtr.TPtr, rowIdx.TPtr, nzVal.SPtr);

        /// <summary>
        /// fills an initialized sparse vector
        /// in the COO-format
        /// </summary>
        /// <param name="ai"> input empty sparse matrix </param>
        /// <param name="rowIdx"> row indices of non-zero elements </param>
        /// <param name="colIdx"> column indices of non-zero elements </param>
        /// <param name="nzVal"> values of non-zero elements </param>
        [Obsolete]
        public unsafe static void FillWMatrixDiCOO(ref WMatrixDi ai,
            VectorI rowIdx, VectorI colIdx, VectorD nzVal)
            => WLinAlg.SparseCreateCOO(ai, rowIdx.TPtr, colIdx.TPtr, nzVal.SPtr);

        /// <summary>
        /// creates a copy of a sparse matrix
        /// </summary>
        /// <param name="ai"> input sparse matrix </param>
        /// <returns> copy of the sparse matrix </returns>
        public static WMatrixDi CopyWMatrixDi(WMatrixDi ai)
            => WLinAlg.SparseCopy(ai);

        /// <summary>
        /// frees memory allocated for a sparse matrix
        /// </summary>
        /// <param name="ai"> sparse matrix to destroy </param>
        public static void DestroyWMatrixDi(ref WMatrixDi ai)
            => WLinAlg.SparseDestroy(ai);

        /// <summary>
        /// converts a sparse matrix to CSR format
        /// </summary>
        /// <param name="ai"> input sparse matrix </param>
        /// <param name="operation"> operation on the input matrix </param>
        /// <returns> result sparse matrix in CSR format </returns>
        public static WMatrixDi ConvertWMatrixDi2CSR(WMatrixDi ai, int operation)
            => WLinAlg.SparseConvert2CSR(ai, operation);

        /// <summary>
        /// changes the value of a single element 
        /// in a sparse matrix
        /// </summary>
        /// <param name="ai"> sparse matrix </param>
        /// <param name="row"> row index of the element </param>
        /// <param name="col"> column index of the element </param>
        /// <param name="value"> target value of the element </param>
        public static void SetWMatrixDiValue(ref WMatrixDi ai,
            long row, long col, double value)
            => WLinAlg.SparseSetValue(ai, row, col, value);

        /// <summary>
        /// changes all or selected elements values in a sparse matrix
        /// </summary>
        /// <param name="ai"> sparse matrix </param>
        /// <param name="rowIdx"> row indices of the new values </param>
        /// <param name="colIdx"> column indices of the new values </param>
        /// <param name="values"> new values </param>
        public unsafe static void UpdateWMatrixDiValues(ref WMatrixDi ai,
            VectorI rowIdx, VectorI colIdx, VectorD values)
            => WLinAlg.SparseUpdateValues(ai, values.Count, rowIdx.TPtr, colIdx.TPtr, values.SPtr);

        #endregion
        #region --- Sparse Analysis Routines ---

        /// <summary>
        /// provides estimate of number and type of upcoming
        /// matrix - vector operations
        /// </summary>
        /// <param name="ai"> sparse matrix </param>
        /// <param name="operation"> operation on the input matrix </param>
        /// <param name="matrixType"> sparse matrix type option </param>
        /// <param name="fillMode"> sparse fill mode option </param>
        /// <param name="diagType"> sparse diagonal type option </param>
        /// <param name="expectedCalls"> number of expected calls to execution routine </param>
        public static void SetMVHint(ref WMatrixDi ai,
            int operation = 10, int matrixType = 20, int fillMode = 42, int diagType = 50,
            long expectedCalls = 1)
            => WLinAlg.SparseSetMVHint(ai, operation, matrixType, fillMode,
                diagType, expectedCalls);

        /// <summary>
        /// provides estimate of number and type of upcoming
		/// triangular system solver operations
        /// </summary>
        /// <param name="ai"> sparse matrix </param>
        /// <param name="operation"> operation on the input matrix </param>
        /// <param name="matrixType"> sparse matrix type option </param>
        /// <param name="fillMode"> sparse fill mode option </param>
        /// <param name="diagType"> sparse diagonal type option </param>
        /// <param name="expectedCalls"> number of expected calls to execution routine </param>
        public static void SetSVHint(ref WMatrixDi ai,
            int operation = 10, int matrixType = 20, int fillMode = 42, int diagType = 50,
            long expectedCalls = 1)
            => WLinAlg.SparseSetSVHint(ai, operation, matrixType, fillMode,
                diagType, expectedCalls);

        /// <summary>
        /// provides estimate of number and type of upcoming
		/// matrix - matrix multiplication operations
        /// </summary>
        /// <param name="ai"> sparse matrix </param>
        /// <param name="denseMatSize"> number of columns in the dense matrix </param>
        /// <param name="operation">operation on the input matrix </param>
        /// <param name="matrixType"> sparse matrix type option </param>
        /// <param name="fillMode"> sparse fill mode option </param>
        /// <param name="diagType"> sparse diagonal type option </param>
        /// <param name="expectedCalls"> number of expected calls to execution routine </param>
        public static void SetMMHint(ref WMatrixDi ai, int denseMatSize,
            int operation = 10, int matrixType = 20, int fillMode = 42, int diagType = 50,
            long expectedCalls = 1)
            => WLinAlg.SparseSetMMHint(ai, operation, matrixType, fillMode, diagType,
                101, denseMatSize, expectedCalls);

        /// <summary>
        /// provides estimate of number and type of upcoming
		/// triangular matrix solve with multiple right hand sides
        /// </summary>
        /// <param name="ai"> sparse matrix </param>
        /// <param name="denseMatSize"> number of columns in the dense matrix </param>
        /// <param name="operation">operation on the input matrix </param>
        /// <param name="matrixType"> sparse matrix type option </param>
        /// <param name="fillMode"> sparse fill mode option </param>
        /// <param name="diagType"> sparse diagonal type option </param>
        /// <param name="expectedCalls"> number of expected calls to execution routine </param>
        public static void SetSMHint(ref WMatrixDi ai, int denseMatSize,
            int operation = 10, int matrixType = 20, int fillMode = 42, int diagType = 50,
            long expectedCalls = 1)
            => WLinAlg.SparseSetSMHint(ai, operation, matrixType, fillMode, diagType,
                101, denseMatSize, expectedCalls);

        /// <summary>
        /// analyzes matrix structure and performs 
        /// optimizations using the hints
        /// </summary>
        /// <param name="ai"> sparse matrix </param>
        public static void Optimize(ref WMatrixDi ai)
            => WLinAlg.SparseOptimize(ai);

        #endregion
        #region --- Sparse Execution Routines ---

        /// <summary>
        /// computes a sparse matrix- vector product
		/// y := alpha*op(A)*x + beta*y
        /// </summary>
        /// <param name="ai"> input sparse matrix a </param>
        /// <param name="x"> input vector x </param>
        /// <param name="y"> vector y (to be overwritten) </param>
        /// <param name="alpha"> scalar constant alpha (default = 1.0) </param>
        /// <param name="operation"> operation on the sparse matrix a </param>
        /// <param name="matrixType"> sparse matrix type option </param>
        /// <param name="fillMode"> sparse fill mode option </param>
        /// <param name="diagType"> sparse diagonal type option </param>
        /// <param name="beta"> scalar constant beta (default = 0.0) </param>
        public unsafe static void MV(WMatrixDi ai, VectorD x, ref VectorD y,
            double alpha = 1.0, int operation = 10, int matrixType = 20, int fillMode = 42, int diagType = 50,
            double beta = 0.0)
            => WLinAlg.SparseMV(operation, alpha, ai,
                matrixType, fillMode, diagType,
                x.SPtr, beta, y.SPtr);

        /// <summary>
        /// solves a system of linear equations for 
		/// a triangular sparse matrix
		/// op(A)*y = alpha * x
        /// </summary>
        /// <param name="ai"> input sparse matrix a </param>
        /// <param name="x"> input vector x </param>
        /// <param name="y"> vector y (to be overwritten) </param>
        /// <param name="alpha"> scalar constant alpha (default = 1.0) </param>
        /// <param name="operation"> operation on the sparse matrix a </param>
        /// <param name="matrixType"> sparse matrix type option </param>
        /// <param name="fillMode"> sparse fill mode option </param>
        /// <param name="diagType"> sparse diagonal type option </param>
        public unsafe static void TRSV(WMatrixDi ai, VectorD x, ref VectorD y,
            double alpha = 1.0, int operation = 10, int matrixType = 20, int fillMode = 42, int diagType = 50)
            => WLinAlg.SparseTRSV(operation, alpha, ai,
                matrixType, fillMode, diagType, x.SPtr, y.SPtr);

        /// <summary>
        /// computes the product of a sparse matrix and a dense
		/// matrix and stores the result as a dense matrix
		/// C := alpha*op(A)*B + beta*C
        /// </summary>
        /// <param name="ai"> input sparse matrix a </param>
        /// <param name="b"> input dense matrix b </param>
        /// <param name="c"> dense matrix c (to be overwritten) </param>
        /// <param name="alpha"> scalar constant alpha (default = 1.0) </param>
        /// <param name="operation"> operation on the sparse matrix a </param>
        /// <param name="matrixType"> sparse matrix type option </param>
        /// <param name="fillMode"> sparse fill mode option </param>
        /// <param name="diagType"> sparse diagonal type option </param>
        /// <param name="beta"> scalar constant beta (default = 0.0) </param>
        public unsafe static void MM(WMatrixDi ai, MatrixD b, ref MatrixD c,
            double alpha = 1.0, int operation = 10, int matrixType = 20, int fillMode = 42, int diagType = 50,
            double beta = 0.0)
            => WLinAlg.SparseMM(operation, alpha,
                ai, matrixType, fillMode, diagType,
                101, b.SPtr, c.Cols, b.Cols, beta, c.SPtr, c.Cols);

        /// <summary>
        /// solves a system of linear equations with multiple 
		/// right-hand sides for a triangular sparse matrix
		/// op(A)*y = alpha * x
        /// </summary>
        /// <param name="ai"> input sparse matrix a </param>
        /// <param name="x"> input dense matrix x </param>
        /// <param name="y"> dense matrix y (to be overwritten) </param>
        /// <param name="alpha"> scalar constant alpha (default = 1.0) </param>
        /// <param name="operation"> operation on the sparse matrix a </param>
        /// <param name="matrixType"> sparse matrix type option </param>
        /// <param name="fillMode"> sparse fill mode option </param>
        /// <param name="diagType"> sparse diagonal type option </param>
        public unsafe static void TRSM(WMatrixDi ai, MatrixD x, ref MatrixD y,
            double alpha = 1.0, int operation = 10, int matrixType = 20, int fillMode = 42, int diagType = 50)
            => WLinAlg.SparseTRSM(operation, alpha,
                ai, matrixType, fillMode, diagType,
                101, x.SPtr, y.Cols, x.Cols, y.SPtr, y.Cols);

        /// <summary>
        /// computes the sum of two sparse matrices and store
		/// the result in a newly allocated sparse matrix
		/// C := alpha*op(A) + B
        /// </summary>
        /// <param name="ai"> input sparse matrix a </param>
        /// <param name="bi"> input sparse matrix b</param>
        /// <param name="alpha"> scalar constant alpha (default = 1.0) </param>
        /// <param name="operation"> operation on the sparse matrix a </param>
        /// <returns> newly allocated sparse matrix c </returns>
        public static WMatrixDi Add(WMatrixDi ai, WMatrixDi bi,
            double alpha = 1.0, int operation = 10)
            => WLinAlg.SparseAdd(operation, ai, alpha, bi);

        #endregion
        #region --- Sparse QR Routines ---

        /// <summary>
        /// defines the pivot strategy for further calls
        /// </summary>
        /// <param name="a"> input sparse matrix a </param>
        public static void SetQRHint(WMatrixDi a)
            => WLinAlg.SparseSetQRHint(a);

        /// <summary>
        /// computes the QR decomposition for the matrix of a
		/// sparse linear system and calculates the solution
		/// A*x = b
        /// </summary>
        /// <param name="a"> input sparse matrix a </param>
        /// <param name="b"> input dense vector b </param>
        /// <param name="operation"> operation on the sparse matrix a </param>
        /// <param name="matrixType"> sparse matrix type option </param>
        /// <param name="fillMode"> sparse fill mode option </param>
        /// <param name="diagType"> sparse diagonal type option </param>
        /// <returns> output dense vector x </returns>
        public unsafe static VectorD LeastSquare(WMatrixDi a, VectorD b,
            int operation = 10, int matrixType = 20, int fillMode = 42, int diagType = 50)
        {
            VectorD x = new(a.Cols, 0.0);
            WLinAlg.SparseQR(a, operation, matrixType, fillMode, diagType,
                101, 1, x.SPtr, 1, b.SPtr, 1);
            return x;
        }

        ///// <summary>
        ///// computes the QR decomposition for the matrix of a
		///// sparse linear system and calculates the solution
		///// A*x = b
        ///// </summary>
        ///// <param name="a"> input sparse matrix a </param>
        ///// <param name="b"> input dense matrix b </param>
        ///// <param name="operation"> operation on the sparse matrix a </param>
        ///// <param name="matrixType"> sparse matrix type option </param>
        ///// <param name="fillMode"> sparse fill mode option </param>
        ///// <param name="diagType"> sparse diagonal type option </param>
        ///// <returns> output dense matrix x </returns>
        //public unsafe static MatrixD QR(WMatrixDi a, MatrixD b,
        //    int operation = 10, int matrixType = 20, int fillMode = 42, int diagType = 50)
        //{
        //    MatrixD x = new(a.Cols, b.Cols, 0.0);
        //    WLinAlg.SparseQR(a, operation, matrixType, fillMode, diagType,
        //        101, b.Cols, x.SPtr, x.Cols, b.SPtr, b.Cols);
        //    return x;
        //}

        ///// <summary>
        ///// reordering step of SPARSE QR solver
        ///// </summary>
        ///// <param name="a"> input sparse matrix a </param>
        ///// <param name="matrixType"> sparse matrix type option </param>
        ///// <param name="fillMode"> sparse fill mode option </param>
        ///// <param name="diagType"> sparse diagonal type option </param>
        //public static void QRReorder(WMatrixDi a,
        //    int matrixType = 20, int fillMode = 42, int diagType = 50)
        //{
        //    int s = WLinAlg.SparseQRReorder(a, matrixType, fillMode, diagType);
        //    Printer.Write($"status = {s}");
        //}

        ///// <summary>
        ///// factorization step of the SPARSE QR solver
        ///// </summary>
        ///// <param name="a"> input sparse matrix a (after reorder step) </param>
        //public unsafe static void QRFactorize(WMatrixDi a, VectorD nu)
        //{
        //    int s = WLinAlg.SparseQRFactorize(a, nu.SPtr);
        //    Printer.Write($"status = {s}");
        //}

        ///// <summary>
        ///// solving step of the SPARSE QR solver
        ///// </summary>
        ///// <param name="a"> input sparse matrix a (after factorize step) </param>
        ///// <param name="b"> input dense vector b </param>
        ///// <param name="operation"> operation on the sparse matrix a </param>
        ///// <returns> output dense vector x </returns>
        //public unsafe static VectorD QRSolve(WMatrixDi a, VectorD b,
        //    int operation = 10)
        //{
        //    VectorD x = new(a.Cols, 0.0);
        //    int s = WLinAlg.SparseQRSolve(a, b.SPtr, operation,
        //        101, 1, x.SPtr, 1, b.SPtr, 1);
        //    Printer.Write($"status = {s}");
        //    return x;
        //}

        #endregion


    }
}
