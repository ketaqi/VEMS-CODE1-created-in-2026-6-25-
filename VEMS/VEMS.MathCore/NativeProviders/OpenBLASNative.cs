using System;
using System.Security;
using System.Runtime.InteropServices;
using Complex = System.Numerics.Complex;

namespace VEMS.MathCore
{
	// openBLAS
	[SuppressUnmanagedCodeSecurity]
	internal sealed unsafe class OpenBLASNative
	{
		private OpenBLASNative() { }

		private const string DllName = "libopenblas64_";

        #region BLAS

        #region ------------- dasum, dzasum ---------------
        
		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
			ExactSpelling = true, SetLastError = false)]
		// sum of the magnitudes of elements of a real vector
		internal static extern double cblas_dasum(int n,
			[In] double* x, int incx);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
			ExactSpelling = true, SetLastError = false)]
		// sum of the magnitudes of elements of a real vector
		internal static extern double cblas_dasum(long n,
			[In] double* x, long incx);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
			ExactSpelling = true, SetLastError = false)]
		// sum of the magnitudes of elements of a real vector
		internal static extern double cblas_dzasum(int n,
			[In] Complex* x, int incx);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
			ExactSpelling = true, SetLastError = false)]
		// sum of the magnitudes of elements of a real vector
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
		// compute a vector-scalar product and adds the result to a vector
		internal static extern void cblas_daxpy(long n,
			double a, [In] double* x, long incx,
			[In, Out] double* y, long incy);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
			ExactSpelling = true, SetLastError = false)]
		// compute a vector-scalar product and adds the result to a vector
		internal static extern void cblas_zaxpy(int n,
			Complex* pa, [In] Complex* x, int incx,
			[In, Out] Complex* y, int incy);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
			ExactSpelling = true, SetLastError = false)]
		// compute a vector-scalar product and adds the result to a vector
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
		#region ------------- ddot, zdotu, zdotc ---------------
		
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
		// ???
		internal static extern void cblas_zdrot(int n,
			[In, Out] Complex* x, int incx,
			[In, Out] Complex* y, int incy,
			double c, double s);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
			ExactSpelling = true, SetLastError = false)]
		// performs rotation of points in the plane
		// ???
		internal static extern void cblas_zdrot(long n,
			[In, Out] Complex* x, long incx,
			[In, Out] Complex* y, long incy,
			double c, double s);
		
		#endregion
		#region ------------- dscal, zdscal, zscal  ---------------
		
		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
			ExactSpelling = true, SetLastError = false)]
		// computes the product of a vector vy a scalar
		internal static extern void cblas_dscal(int n, double a,
			[In] double* x, int incx);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
			ExactSpelling = true, SetLastError = false)]
		// computes the product of a vector vy a scalar
		internal static extern void cblas_dscal(long n, double a,
			[In] double* x, long incx);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
			ExactSpelling = true, SetLastError = false)]
		// computes the product of a vector by a scalar
		// ???
		internal static extern void cblas_zdscal(int n, double a,
			[In] Complex* x, int incx);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
			ExactSpelling = true, SetLastError = false)]
		// computes the product of a vector by a scalar
		// ???
		internal static extern void cblas_zdscal(long n, double a,
			[In] Complex* x, long incx);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
			ExactSpelling = true, SetLastError = false)]
		// computes the product of a vector by a scalar
		internal static extern void cblas_zscal(int n, Complex* a,
			[In] Complex* x, int incx);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
			ExactSpelling = true, SetLastError = false)]
		// computes the product of a vector by a scalar
		internal static extern void cblas_zscal(long n, Complex* a,
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
		internal static extern void cblas_dgemv(BLAS_Layout order, BLAS_Transpose trans,
			int m, int n, double alpha, [In] double* a, int lda,
			[In] double* x, int incx,
			double beta, [In, Out] double* y, int incy);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
			ExactSpelling = true, SetLastError = false)]
		// computes a matrix-vector product using a general matrix
		// y := alpha*A*x + beta*y
		internal static extern void cblas_dgemv(BLAS_Layout order, BLAS_Transpose trans,
			long m, long n, double alpha, [In] double* a, long lda,
			[In] double* x, long incx,
			double beta, [In, Out] double* y, long incy);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
			ExactSpelling = true, SetLastError = false)]
		// computes a matrix-vector product using a general matrix
		internal static extern void cblas_zgemv(BLAS_Layout order, BLAS_Transpose trans,
			int m, int n, Complex* alpha, [In] Complex* a, int lda,
			[In] Complex* x, int incx,
			Complex* beta, [In, Out] Complex* y, int incy);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
			ExactSpelling = true, SetLastError = false)]
		// computes a matrix-vector product using a general matrix
		internal static extern void cblas_zgemv(BLAS_Layout order, BLAS_Transpose trans,
			long m, long n, Complex* alpha, [In] Complex* a, long lda,
			[In] Complex* x, long incx,
			Complex* beta, [In, Out] Complex* y, long incy);
		
		#endregion
		#region ------------- dgemm, zgemm ---------------
		
		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
			ExactSpelling = true, SetLastError = false)]
		// computes a matrix-matrix product with general matrices
		internal static extern void cblas_dgemm(BLAS_Layout order, BLAS_Transpose transa, BLAS_Transpose transb,
			int m, int n, int k,
			double alpha, [In] double* a, int lda, [In] double* b, int ldb,
			double beta, [In, Out] double* c, int ldc);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
			ExactSpelling = true, SetLastError = false)]
		// computes a matrix-matrix product with general matrices
		internal static extern void cblas_dgemm(BLAS_Layout order, BLAS_Transpose transa, BLAS_Transpose transb,
			long m, long n, long k,
			double alpha, double* a, long lda, double* b, long ldb,
			double beta, double* c, long ldc);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
			ExactSpelling = true, SetLastError = false)]
		// computes a matrix-matrix product with general matrices
		internal static extern void cblas_zgemm(BLAS_Layout order, BLAS_Transpose transa, BLAS_Transpose transb,
			int m, int n, int k,
			Complex* alpha, [In] Complex* a, int lda, [In] Complex* b, int ldb,
			Complex* beta, [In, Out] Complex* c, int ldc);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
			ExactSpelling = true, SetLastError = false)]
		// computes a matrix-matrix product with general matrices
		internal static extern void cblas_zgemm(BLAS_Layout order, BLAS_Transpose transa, BLAS_Transpose transb,
			long m, long n, long k,
			Complex* alpha, [In] Complex* a, long lda, [In] Complex* b, long ldb,
			Complex* beta, [In, Out] Complex* c, long ldc);
		
		#endregion

		#endregion

		#region BLAS-like Extensions

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
		internal static extern void cblas_daxpby(long n,
			double a, [In] double* x, long incx,
			double b, [In, Out] double* y, long incy);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
			ExactSpelling = true, SetLastError = false)]
		// scales two vectors, adds them to one another and stores result in the vector
		internal static extern void cblas_zaxpby(int n,
			Complex* a, [In] Complex* x, int incx,
			Complex* b, [In, Out] Complex* y, int incy);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
			ExactSpelling = true, SetLastError = false)]
		// scales two vectors, adds them to one another and stores result in the vector
		internal static extern void cblas_zaxpby(long n,
			Complex* a, [In] Complex* x, long incx,
			Complex* b, [In, Out] Complex* y, long incy);
		
		#endregion
		#region ------------- dgemm3m (???) -------------
		
		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
			ExactSpelling = true, SetLastError = false)]
		// computes a scalar-matrix-matrix product
		internal static extern void cblas_dgemm3m(BLAS_Layout layout, BLAS_Transpose transa, BLAS_Transpose transb,
			int m, int n, int k,
			double alpha, [In] double* a, int lda, [In] double* b, int ldb,
			double beta, [In, Out] double* c, int ldc);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
			ExactSpelling = true, SetLastError = false)]
		// computes a scalar-matrix-matrix product
		internal static extern void cblas_dgemm3m(BLAS_Layout layout, BLAS_Transpose transa, BLAS_Transpose transb,
			long m, long n, long k,
			double alpha, [In] double* a, long lda, [In] double* b, long ldb,
			double bta, [In, Out] double* c, long ldc);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
			ExactSpelling = true, SetLastError = false)]
		// computes a scalar-matrix-matrix product
		internal static extern void cblas_zgemm3m(BLAS_Layout layout, BLAS_Transpose transa, BLAS_Transpose transb,
			int m, int n, int k,
			Complex* alpha, [In] Complex* a, int lda, [In] Complex* b, int ldb,
			Complex* beta, [In, Out] Complex* c, int ldc);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
			ExactSpelling = true, SetLastError = false)]
		// computes a scalar-matrix-matrix product
		internal static extern void cblas_zgemm3m(BLAS_Layout layout, BLAS_Transpose transa, BLAS_Transpose transb,
			long m, long n, long k,
			Complex* alpha, [In] Complex* a, long lda, [In] Complex* b, long ldb,
			Complex* beta, [In, Out] Complex* c, long ldc);
		
		#endregion
		#region ------------- dimatcopy, zimatcopy -------------
		
		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
			ExactSpelling = true, SetLastError = false)]
		// performs scaling and in-place transposition/copying of matrices
		internal static extern void cblas_dimatcopy(BLAS_Layout layout, BLAS_Transpose trans,
			int rows, int cols,
			double alpha, double* ab, int lda, int ldb);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
			ExactSpelling = true, SetLastError = false)]
		// performs scaling and in-place transposition/copying of matrices
		internal static extern void cblas_dimatcopy(BLAS_Layout layout, BLAS_Transpose trans,
			long rows, long cols,
			double alpha, double* ab, long lda, long ldb);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
			ExactSpelling = true, SetLastError = false)]
		// performs scaling and in-place transposition/copying of matrices
		internal static extern void cblas_zimatcopy(BLAS_Layout layout, BLAS_Transpose trans,
			int rows, int cols,
			Complex* alpha, Complex* ab, int lda, int ldb);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
			ExactSpelling = true, SetLastError = false)]
		// performs scaling and in-place transposition/copying of matrices
		internal static extern void cblas_zimatcopy(BLAS_Layout layout, BLAS_Transpose trans,
			long rows, long cols,
			Complex* alpha, Complex* ab, long lda, long ldb);
		
		#endregion
		#region ------------- domatcopy, zomatcopy -------------
		
		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
			ExactSpelling = true, SetLastError = false)]
		// performs scaling and out-place transposition/copying of matrices
		internal static extern void cblas_domatcopy(BLAS_Layout layout, BLAS_Transpose trans,
			int rows, int cols,
			double alpha, double* a, int lda, double* b, int ldb);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
			ExactSpelling = true, SetLastError = false)]
		// performs scaling and out-place transposition/copying of matrices
		internal static extern void cblas_domatcopy(BLAS_Layout layout, BLAS_Transpose trans,
			long rows, long cols,
			double alpha, double* a, long lda, double* b, long ldb);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
			ExactSpelling = true, SetLastError = false)]
		// performs scaling and out-place transposition/copying of matrices
		internal static extern void cblas_zomatcopy(BLAS_Layout layout, BLAS_Transpose trans,
			int rows, int cols,
			Complex* alpha, Complex* a, int lda, Complex* b, int ldb);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
			ExactSpelling = true, SetLastError = false)]
		// performs scaling and out-place transposition/copying of matrices
		internal static extern void cblas_zomatcopy(BLAS_Layout layout, BLAS_Transpose trans,
			long rows, long cols,
			Complex* alpha, Complex* a, long lda, Complex* b, long ldb);
		
		#endregion

		#endregion

		#region LAPACK

		#region ------------- dgetrf, zgetrf ---------------
		
		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
			ExactSpelling = true, SetLastError = false)]
		// computes the LU factorization of a general m-by-n matrix
		internal static extern int LAPACKE_dgetrf(LAPACK_Layout layout, int m, int n,
			[In, Out] double* a, int lda, [Out] int* ipiv);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
			ExactSpelling = true, SetLastError = false)]
		// computes the LU factorization of a general m-by-n matrix
		internal static extern int LAPACKE_dgetrf(LAPACK_Layout layout, long m, long n,
			[In, Out] double* a, long lda, [Out] long* ipiv);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
			ExactSpelling = true, SetLastError = false)]
		// computes the LU factorization of a general m-by-n matrix
		internal static extern int LAPACKE_zgetrf(LAPACK_Layout layout, int m, int n,
			[In, Out] Complex* a, int lda, [Out] int* ipiv);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
			ExactSpelling = true, SetLastError = false)]
		// computes the LU factorization of a general m-by-n matrix
		internal static extern int LAPACKE_zgetrf(LAPACK_Layout layout, long m, long n,
			[In, Out] Complex* a, long lda, [Out] long* ipiv);
		
		#endregion
		#region ------------- dgetrs, zgetrs -------------
		
		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
			ExactSpelling = true, SetLastError = false)]
		// solves a system of linear equations 
		// with an LU-factored square coefficient matrix
		internal static extern int LAPACKE_dgetrs(LAPACK_Layout layout, LAPACK_Transpose trans,
			int n, int nrhs, double* a, int lda, int* ipiv, double* b, int ldb);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
			ExactSpelling = true, SetLastError = false)]
		// solves a system of linear equations 
		// with an LU-factored square coefficient matrix
		internal static extern int LAPACKE_dgetrs(LAPACK_Layout layout, LAPACK_Transpose trans,
			long n, long nrhs, double* a, long lda, long* ipiv, double* b, long ldb);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
			ExactSpelling = true, SetLastError = false)]
		// solves a system of linear equations 
		// with an LU-factored square coefficient matrix
		internal static extern int LAPACKE_zgetrs(LAPACK_Layout layout, LAPACK_Transpose trans,
			int n, int nrhs, Complex* a, int lda, int* ipiv, Complex* b, int ldb);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
			ExactSpelling = true, SetLastError = false)]
		// solves a system of linear equations 
		// with an LU-factored square coefficient matrix
		internal static extern int LAPACKE_zgetrs(LAPACK_Layout layout, LAPACK_Transpose trans,
			long n, long nrhs, Complex* a, long lda, long* ipiv, Complex* b, long ldb);
		
		#endregion
		#region ------------- dgetri, zgetri -------------
		
		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
			ExactSpelling = true, SetLastError = false)]
		// computes the inverse of an LU-factored general matrix
		internal static extern int LAPACKE_dgetri(LAPACK_Layout layout, int n,
			[In, Out] double* a, int lda, [In] int* ipiv);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
			ExactSpelling = true, SetLastError = false)]
		// computes the inverse of an LU-factored general matrix
		internal static extern int LAPACKE_dgetri(LAPACK_Layout layout, long n,
			[In, Out] double* a, long lda, [In] long* ipiv);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
			ExactSpelling = true, SetLastError = false)]
		// computes the inverse of an LU-factored general matrix
		internal static extern int LAPACKE_zgetri(LAPACK_Layout layout, int n,
			[In, Out] Complex* a, int lda, [In] int* ipiv);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
			ExactSpelling = true, SetLastError = false)]
		// computes the inverse of an LU-factored general matrix
		internal static extern int LAPACKE_zgetri(LAPACK_Layout layout, long n,
			[In, Out] Complex* a, long lda, [In] long* ipiv);
		
		#endregion
		#region ------------- dgesv, zgesv ---------------
		
		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
			ExactSpelling = true, SetLastError = false)]
		// Computes the solution to the system of linear equations 
		// with a square coefficient matrix A and multiple right-hand sides
		internal static extern int LAPACKE_dgesv(LAPACK_Layout layout, int n, int nrhs,
			double* a, int lda, int* ipiv,
			double* b, int ldb);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
			ExactSpelling = true, SetLastError = false)]
		// Computes the solution to the system of linear equations 
		// with a square coefficient matrix A and multiple right-hand sides
		internal static extern int LAPACKE_dgesv(LAPACK_Layout layout, long n, long nrhs,
			double* a, long lda, long* ipiv,
			double* b, long ldb);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
			ExactSpelling = true, SetLastError = false)]
		// Computes the solution to the system of linear equations 
		// with a square coefficient matrix A and multiple right-hand sides
		internal static extern int LAPACKE_zgesv(LAPACK_Layout layout, int n, int nrhs,
			Complex* a, int lda, int* ipiv,
			Complex* b, int ldb);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
			ExactSpelling = true, SetLastError = false)]
		// Computes the solution to the system of linear equations 
		// with a square coefficient matrix A and multiple right-hand sides
		internal static extern int LAPACKE_zgesv(LAPACK_Layout layout, long n, long nrhs,
			Complex* a, long lda, long* ipiv,
			Complex* b, long ldb);
		
		#endregion
		#region ------------- dgeev, zgeev ---------------
		
		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
			ExactSpelling = true, SetLastError = false)]
		// Computes the eigenvalues and left and right eigenvectors of a general matrix
		internal static extern int LAPACKE_dgeev(LAPACK_Layout layout, LAPACK_Job jobvl, LAPACK_Job jobvr, int n,
			double* a, int lda,
			double* wr, double* wi,
			double* vl, int ldvl, double* vr, int ldvr);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
			ExactSpelling = true, SetLastError = false)]
		// Computes the eigenvalues and left and right eigenvectors of a general matrix
		internal static extern int LAPACKE_dgeev(LAPACK_Layout layout, LAPACK_Job jobvl, LAPACK_Job jobvr, long n,
			double* a, long lda,
			double* wr, double* wi,
			double* vl, long ldvl, double* vr, long ldvr);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
			ExactSpelling = true, SetLastError = false)]
		// Computes the eigenvalues and left and right eigenvectors of a general matrix
		internal static extern int LAPACKE_zgeev(LAPACK_Layout layout, LAPACK_Job jobvl, LAPACK_Job jobvr, int n,
			Complex* a, int lda,
			Complex* w,
			Complex* vl, int ldvl, Complex* vr, int ldvr);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
			ExactSpelling = true, SetLastError = false)]
		// Computes the eigenvalues and left and right eigenvectors of a general matrix
		internal static extern int LAPACKE_zgeev(LAPACK_Layout layout, LAPACK_Job jobvl, LAPACK_Job jobvr, long n,
			Complex* a, long lda,
			Complex* w,
			Complex* vl, long ldvl, Complex* vr, long ldvr);
		
		#endregion
		#region ------------- dsyev, zheev ---------------
		
		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
			ExactSpelling = true, SetLastError = false)]
		// Computes all eigenvalues and, optionally, eigenvectors 
		// of a real symmetric matrix
		internal static extern int LAPACKE_dsyev(LAPACK_Layout layout, LAPACK_Job jobz, char uplo, int n,
			double* a, int lda, double* w);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
			ExactSpelling = true, SetLastError = false)]
		// Computes all eigenvalues and, optionally, eigenvectors 
		// of a real symmetric matrix
		internal static extern int LAPACKE_dsyev(LAPACK_Layout layout, LAPACK_Job jobz, char uplo, long n,
			double* a, long lda, double* w);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
			ExactSpelling = true, SetLastError = false)]
		// Computes all eigenvalues and, optionally, eigenvectors
		// of a Hermitian matrix
		internal static extern int LAPACKE_zheev(LAPACK_Layout layout, LAPACK_Job jobz, char uplo, int n,
			Complex* a, int lda, double* w);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
			ExactSpelling = true, SetLastError = false)]
		// Computes all eigenvalues and, optionally, eigenvectors
		// of a Hermitian matrix
		internal static extern int LAPACKE_zheev(LAPACK_Layout layout, LAPACK_Job jobz, char uplo, long n,
			Complex* a, long lda, double* w);
		
		#endregion
		#region ------------- dgesvd, zgesvd ---------------
		
		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
			ExactSpelling = true, SetLastError = false)]
		// Computes the singular value decomposition of a general rectangular matrix
		internal static extern int LAPACKE_dgesvd(LAPACK_Layout layout, LAPACK_Job jobu, LAPACK_Job jobvt,
			int m, int n,
			double* a, int lda, double* s,
			double* u, int ldu,
			double* vt, int ldvt, double* superb);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
			ExactSpelling = true, SetLastError = false)]
		// Computes the singular value decomposition of a general rectangular matrix
		internal static extern int LAPACKE_dgesvd(LAPACK_Layout layout, LAPACK_Job jobu, LAPACK_Job jobvt,
			long m, long n,
			double* a, long lda, double* s,
			double* u, long ldu,
			double* vt, long ldvt, double* superb);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
			ExactSpelling = true, SetLastError = false)]
		// Computes the singular value decomposition of a general rectangular matrix
		internal static extern int LAPACKE_zgesvd(LAPACK_Layout layout, LAPACK_Job jobu, LAPACK_Job jobvt,
			int m, int n,
			Complex* a, int lda, double* s,
			Complex* u, int ldu,
			Complex* vt, int ldvt, double* superb);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
			ExactSpelling = true, SetLastError = false)]
		// Computes the singular value decomposition of a general rectangular matrix
		internal static extern int LAPACKE_zgesvd(LAPACK_Layout layout, LAPACK_Job jobu, LAPACK_Job jobvt,
			long m, long n,
			Complex* a, long lda, double* s,
			Complex* u, long ldu,
			Complex* vt, long ldvt, double* superb);
		
		#endregion
		#region ------------- dgels, zgels ---------------
		
		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
			ExactSpelling = true, SetLastError = false)]
		// Uses QR or LQ factorization to solve a overdetermined or 
		// underdetermined linear system with full rank
		internal static extern int LAPACKE_dgels(LAPACK_Layout layout, LAPACK_Transpose trans,
			int m, int n, int nrhs,
			double* a, int lda,
			double* b, int ldb);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
			ExactSpelling = true, SetLastError = false)]
		// Uses QR or LQ factorization to solve a overdetermined or 
		// underdetermined linear system with full rank
		internal static extern int LAPACKE_dgels(LAPACK_Layout layout, LAPACK_Transpose trans,
			long m, long n, long nrhs,
			double* a, long lda,
			double* b, long ldb);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
			ExactSpelling = true, SetLastError = false)]
		// Uses QR or LQ factorization to solve a overdetermined or 
		// underdetermined linear system with full rank
		internal static extern int LAPACKE_zgels(LAPACK_Layout layout, LAPACK_Transpose trans,
			int m, int n, int nrhs,
			Complex* a, int lda,
			Complex* b, int ldb);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
			ExactSpelling = true, SetLastError = false)]
		// Uses QR or LQ factorization to solve a overdetermined or 
		// underdetermined linear system with full rank
		internal static extern int LAPACKE_zgels(LAPACK_Layout layout, LAPACK_Transpose trans,
			long m, long n, long nrhs,
			Complex* a, long lda,
			Complex* b, long ldb);
		
		#endregion
		#region ------------- zlacgv ---------------
		
		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
			ExactSpelling = true, SetLastError = false)]
		// Conjugates a complex vector
		internal static extern int LAPACKE_zlacgv(int n,
			Complex* x, int incx);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
			ExactSpelling = true, SetLastError = false)]
		// Conjugates a complex vector
		internal static extern int LAPACKE_zlacgv(long n,
			Complex* x, int incx);
		
		#endregion

		#endregion



	}
}