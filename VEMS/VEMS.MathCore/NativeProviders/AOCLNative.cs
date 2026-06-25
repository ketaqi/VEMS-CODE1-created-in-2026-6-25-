using System.Security;
using System.Runtime.InteropServices;
using Complex = System.Numerics.Complex;

namespace VEMS.MathCore
{
    // AMD BLIS
    [SuppressUnmanagedCodeSecurity]
    internal sealed unsafe class AOCLNative
    {
        private AOCLNative() { }

		private const string DllName = "AOCL-LibBlis-Win-MT-dll";
		private const string FlameDllName = "libfftw-dll";

        #region BLAS

        #region ------------- dasum, dzasum ---------------

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
             ExactSpelling = true, SetLastError = false)]
        // computes sum of the magnitudes of elements of a real vector
        internal static extern double cblas_dasum(int n,
			[In] double* x, int incx);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
			 ExactSpelling = true, SetLastError = false)]
		// computes the sum of magnitudes of the matrix elements
		internal static extern double cblas_dasum(long n,
			[In] double* x, long incx);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
			 ExactSpelling = true, SetLastError = false)]
		// computes the sum of magnitudes of the matrix elements
		internal static extern double cblas_dzasum(int n,
			[In] Complex* x, int incx);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
			 ExactSpelling = true, SetLastError = false)]
		// computes the sum of magnitudes of the matrix elements
		internal static extern double cblas_dzasum(long n,
			[In] Complex* x, long incx);

        #endregion
        #region ------------- daxpy, zaxpy ---------------

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
			 ExactSpelling = true, SetLastError = false)]
		// compute a vector-scalar product and adds the result to a vector
		internal static extern void cblas_daxpy(int n,
			double a, [In] double* x, int incx,
			[In, Out] double* y, int incy);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
			 ExactSpelling = true, SetLastError = false)]
		// compute a matrix-scalar product and adds the result to a matrix
		internal static extern void cblas_daxpy(long n,
			double a, [In] double* x, long incx,
			[In, Out] double* y, long incy);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
			 ExactSpelling = true, SetLastError = false)]
		// compute a vector-scalar product and adds the result to a vector
		internal static extern void cblas_zaxpy(int n,
			Complex* a, [In] Complex* x, int incx,
			[In, Out] Complex* y, int incy);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
			 ExactSpelling = true, SetLastError = false)]
		// compute a matrix-scalar product and adds the result to a matrix
		internal static extern void cblas_zaxpy(long n,
			Complex* a, [In] Complex* x, long incx,
			[In, Out] Complex* y, long incy);

        #endregion
        #region ------------- dcopy, zcopy ---------------

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
			 ExactSpelling = true, SetLastError = false)]
		// copies vector x to vector y
		internal static extern void cblas_dcopy(int n,
			[In] double* x, int incx,
			[Out] double* y, int incy);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
			 ExactSpelling = true, SetLastError = false)]
		// copies vector x to vector y
		internal static extern void cblas_dcopy(long n,
			[In] double* x, long incx,
			[Out] double* y, long incy);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
			 ExactSpelling = true, SetLastError = false)]
		// copies vector x to vector y
		internal static extern void cblas_zcopy(int n,
			[In] Complex* x, int incx,
			[Out] Complex* y, int incy);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
			 ExactSpelling = true, SetLastError = false)]
		// copies vector x to vector y
		internal static extern void cblas_zcopy(long n,
			[In] Complex* x, long incx,
			[Out] Complex* y, long incy);

        #endregion
        #region  ------------- ddot, zdotu, zdotc ---------------

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
			 ExactSpelling = true, SetLastError = false)]
		// computes a vector-vector dot product
		internal static extern double cblas_ddot(int n,
			[In] double* x, int incx,
			[In] double* y, int incy);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
			 ExactSpelling = true, SetLastError = false)]
		// computes a vector-vector dot product
		internal static extern double cblas_ddot(long n,
			[In] double* x, long incx,
			[In] double* y, long incy);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
			 ExactSpelling = true, SetLastError = false)]
		// computes a vector-vector dot product
		internal static extern void cblas_zdotu_sub(int n,
			[In] Complex* x, int incx,
			[In] Complex* y, int incy,
			[Out] Complex* dotu);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
			 ExactSpelling = true, SetLastError = false)]
		// computes a vector-vector dot product
		internal static extern void cblas_zdotu_sub(long n,
			[In] Complex* x, long incx,
			[In] Complex* y, long incy,
			[Out] Complex* dotu);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
			 ExactSpelling = true, SetLastError = false)]
		// compute a dot product of a conjugated vector with another vector
		internal static extern void cblas_zdotc_sub(int n,
			[In] Complex* x, int incx,
			[In] Complex* y, int incy,
			[Out] Complex* dotc);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
			 ExactSpelling = true, SetLastError = false)]
		// compute a dot product of a conjugated vector with another vector
		internal static extern void cblas_zdotc_sub(long n,
			[In] Complex* x, long incx,
			[In] Complex* y, long incy,
			[Out] Complex* dotc);

        #endregion
        #region ------------- dnrm2, dznrm2 ---------------
        
		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
			 ExactSpelling = true, SetLastError = false)]
		// computes the Euclidean norm of a vector
		internal static extern double cblas_dnrm2(int n,
			[In] double* x, int incx);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
			 ExactSpelling = true, SetLastError = false)]
		// computes the Euclidean norm of a vector
		internal static extern double cblas_dnrm2(long n,
			[In] double* x, long incx);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
			 ExactSpelling = true, SetLastError = false)]
		// computes the Euclidean norm of a vector
		internal static extern double cblas_dznrm2(int n,
			[In] Complex* x, int incx);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
			 ExactSpelling = true, SetLastError = false)]
		// computes the Euclidean norm of a vector
		internal static extern double cblas_dznrm2(long n,
			[In] Complex* x, long incx);

		#endregion
		#region ------------- drot, zdrot ---------------
		
		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
			 ExactSpelling = true, SetLastError = false)]
		// performs rotation of points in the plane
		internal static extern void cblas_drot(int n,
			[In, Out] double* x, int incx,
			[In, Out] double* y, int incy,
			double c, double s);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
			 ExactSpelling = true, SetLastError = false)]
		// performs rotation of points in the plane
		internal static extern void cblas_drot(long n,
			[In, Out] double* x, long incx,
			[In, Out] double* y, long incy,
			double c, double s);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
			 ExactSpelling = true, SetLastError = false)]
		// performs rotation of points in the plane
		internal static extern void cblas_zdrot(int n,
			[In, Out] Complex* x, int incx,
			[In, Out] Complex* y, int incy,
			double c, double s);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
			 ExactSpelling = true, SetLastError = false)]
		// performs rotation of points in the plane
		internal static extern void cblas_zdrot(long n,
			[In, Out] Complex* x, long incx,
			[In, Out] Complex* y, long incy,
			double c, double s);

        #endregion
        #region  ------------- dscal, zdscal, zscal  ---------------
        
		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
			 ExactSpelling = true, SetLastError = false)]
		// computes the product of a vector vy a scalar
		internal static extern void cblas_dscal(int n, double a,
			[In] double* x, int incx);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
			 ExactSpelling = true, SetLastError = false)]
		// computes the product of a matrix vy a scalar
		internal static extern void cblas_dscal(long n, double a,
			[In] double* x, long incx);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
			 ExactSpelling = true, SetLastError = false)]
		// computes the product of a vector by a scalar
		internal static extern void cblas_zdscal(int n, double a,
			[In] Complex* x, int incx);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
			 ExactSpelling = true, SetLastError = false)]
		// computes the product of a matrix by a scalar
		internal static extern void cblas_zdscal(long n, double a,
			[In] Complex* x, long incx);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
			 ExactSpelling = true, SetLastError = false)]
		// computes the product of a vector by a scalar
		internal static extern void cblas_zscal(int n, Complex* pa,
			[In] Complex* x, int incx);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
			 ExactSpelling = true, SetLastError = false)]
		// computes the product of a matrix by a scalar
		internal static extern void cblas_zscal(long n, Complex* pa,
			[In] Complex* x, long incx);

		#endregion
		#region ------------- dswap, zswap -------------
		
		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
			 ExactSpelling = true, SetLastError = false)]
		// given two vectors x and y, returns vector y and x swapped
		internal static extern void cblas_dswap(int n,
			[In, Out] double* x, int incx,
			[In, Out] double* y, int incy);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
			 ExactSpelling = true, SetLastError = false)]
		// given two vectors x and y, returns vector y and x swapped
		internal static extern void cblas_dswap(long n,
			[In, Out] double* x, long incx,
			[In, Out] double* y, long incy);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
			 ExactSpelling = true, SetLastError = false)]
		// given two vectors x and y, returns vector y and x swapped
		internal static extern void cblas_zswap(int n,
			[In, Out] Complex* x, int incx,
			[In, Out] Complex* y, int incy);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
			 ExactSpelling = true, SetLastError = false)]
		// given two vectors x and y, returns vector y and x swapped
		internal static extern void cblas_zswap(long n,
			[In, Out] Complex* x, long incx,
			[In, Out] Complex* y, long incy);

		#endregion
		#region ------------- idamax, idamin, izamax, izamin ---------------
		
		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
			 ExactSpelling = true, SetLastError = false)]
		// finds the index of the element with maximum absolute value
		internal static extern int cblas_idamax(int n,
			[In] double* x, int incx);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
			 ExactSpelling = true, SetLastError = false)]
		// finds the index of the element with maximum absolute value
		internal static extern int cblas_idamax(long n,
			[In] double* x, long incx);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
			 ExactSpelling = true, SetLastError = false)]
		// finds the index of the element with the snmallest absolute value
		internal static extern int cblas_idamin(int n,
			[In] double* x, int incx);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
			 ExactSpelling = true, SetLastError = false)]
		// finds the index of the element with the snmallest absolute value
		internal static extern int cblas_idamin(long n,
			[In] double* x, long incx);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
			 ExactSpelling = true, SetLastError = false)]
		// finds the index of the element with maximum absolute value
		internal static extern int cblas_izamax(int n,
			[In] Complex* x, int incx);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
			 ExactSpelling = true, SetLastError = false)]
		// finds the index of the element with maximum absolute value
		internal static extern int cblas_izamax(long n,
			[In] Complex* x, long incx);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
			 ExactSpelling = true, SetLastError = false)]
		// finds the index of the element with the smallest absolute value
		internal static extern int cblas_izamin(int n,
			[In] Complex* x, int incx);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
			 ExactSpelling = true, SetLastError = false)]
		// finds the index of the element with the smallest absolute value
		internal static extern int cblas_izamin(long n,
			[In] Complex* x, long incx);

        #endregion
        #region ------------- dgemv, zgemv ---------------
        
		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
			 ExactSpelling = true, SetLastError = false)]
		// computes a matrix-vector product using a general matrix
		// y := alpha*A*x + beta*y
		internal static extern void cblas_dgemv(BLAS_Layout layout, BLAS_Transpose trans,
			int m, int n, double alpha, [In] double* a, int lda,
			[In] double* x, int incx,
			double beta, [In, Out] double* y, int incy);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
			 ExactSpelling = true, SetLastError = false)]
		// computes a matrix-vector product using a general matrix
		// y := alpha*A*x + beta*y
		internal static extern void cblas_dgemv(BLAS_Layout layout, BLAS_Transpose trans,
			long m, long n, double alpha, [In] double* a, long lda,
			[In] double* x, long incx,
			double beta, [In, Out] double* y, long incy);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
			 ExactSpelling = true, SetLastError = false)]
		// computes a matrix-vector product using a general matrix
		internal static extern void cblas_zgemv(BLAS_Layout layout, BLAS_Transpose trans,
			int m, int n, Complex* alpha, [In] Complex* a, int lda,
			[In] Complex* x, int incx,
			Complex* beta, [In, Out] Complex* y, int incy);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
			 ExactSpelling = true, SetLastError = false)]
		// computes a matrix-vector product using a general matrix
		internal static extern void cblas_zgemv(BLAS_Layout layout, BLAS_Transpose trans,
			long m, long n, Complex* alpha, [In] Complex* a, long lda,
			[In] Complex* x, long incx,
			Complex* beta, [In, Out] Complex* y, long incy);

        #endregion
        #region ------------- dgemm, zgemm ---------------
        
		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
			 ExactSpelling = true, SetLastError = false)]
		// computes a matrix-matrix product with general matrices
		internal static extern void cblas_dgemm(BLAS_Layout layout, BLAS_Transpose transa, BLAS_Transpose transb,
			int m, int n, int k,
			double alpha, double* a, int lda, double* b, int ldb,
			double beta, double* c, int ldc);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
			 ExactSpelling = true, SetLastError = false)]
		// computes a matrix-matrix product with general matrices
		internal static extern void cblas_dgemm(BLAS_Layout layout, BLAS_Transpose transa, BLAS_Transpose transb,
			long m, long n, long k,
			double alpha, double* a, long lda, double* b, long ldb,
			double beta, double* c, long ldc);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
			 ExactSpelling = true, SetLastError = false)]
		// computes a matrix-matrix product with general matrices
		internal static extern void cblas_zgemm(BLAS_Layout layout, BLAS_Transpose transa, BLAS_Transpose transb,
			int m, int n, int k,
			Complex* alpha, [In] Complex* a, int lda, [In] Complex* b, int ldb,
			Complex* beta, [In, Out] Complex* c, int ldc);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
			 ExactSpelling = true, SetLastError = false)]
		// computes a matrix-matrix product with general matrices
		internal static extern void cblas_zgemm(BLAS_Layout layout, BLAS_Transpose transa, BLAS_Transpose transb,
			long m, long n, long k,
			Complex* alpha, [In] Complex* a, long lda, [In] Complex* b, long ldb,
			Complex* beta, [In, Out] Complex* c, long ldc);
        
		#endregion
        #region ------------- daxpby, zaxpby (?) ---------------
        
		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
			 ExactSpelling = true, SetLastError = false)]
		// scales two vectors, adds them to one another and stores result in the vector
		internal static extern void cblas_daxpby(int n,
			double a, [In] double* x, int incx,
			double b, [In, Out] double* y, int incy);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
			 ExactSpelling = true, SetLastError = false)]
		// scales two vectors, adds them to one another and stores result in the vector
		internal static extern void cblas_zaxpby(int n,
			Complex* pa, [In] Complex* x, int incx,
			Complex* pb, [In, Out] Complex* y, int incy);

		#endregion

		#endregion

		#region BLAS-like Extensions

		//...

		#endregion

		#region LAPACK

		// ...

		#region ------------- dgesv, zgesv ---------------
		
		[DllImport(FlameDllName, CallingConvention = CallingConvention.Cdecl,
			ExactSpelling = true, SetLastError = false)]
		// Computes the solution to the system of linear equations 
		// with a square coefficient matrix A and multiple right-hand sides
		internal static extern int LAPACKE_dgesv(LAPACK_Layout layout, int n, int nrhs,
			double* a, int lda, int* ipiv,
			double* b, int ldb);

		[DllImport(FlameDllName, CallingConvention = CallingConvention.Cdecl,
			ExactSpelling = true, SetLastError = false)]
		// Computes the solution to the system of linear equations 
		// with a square coefficient matrix A and multiple right-hand sides
		internal static extern int LAPACKE_dgesv(LAPACK_Layout layout, long n, long nrhs,
			double* a, long lda, long* ipiv,
			double* b, long ldb);

		[DllImport(FlameDllName, CallingConvention = CallingConvention.Cdecl,
			ExactSpelling = true, SetLastError = false)]
		// Computes the solution to the system of linear equations 
		// with a square coefficient matrix A and multiple right-hand sides
		internal static extern int LAPACKE_zgesv(LAPACK_Layout layout, int n, int nrhs,
			Complex* a, int lda, int* ipiv,
			Complex* b, int ldb);

		[DllImport(FlameDllName, CallingConvention = CallingConvention.Cdecl,
			ExactSpelling = true, SetLastError = false)]
		// Computes the solution to the system of linear equations 
		// with a square coefficient matrix A and multiple right-hand sides
		internal static extern int LAPACKE_zgesv(LAPACK_Layout layout, long n, long nrhs,
			Complex* a, long lda, long* ipiv,
			Complex* b, long ldb);

		#endregion

		#endregion

		#region FFTW

		// ...

		#endregion



	}

}