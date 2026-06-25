#include "pch.h"
#include "WMathCore.h"
#include <iostream>

namespace WMathCore
{

	WLinAlg::WLinAlg(){ }


	bool WLinAlg::AreVectorLengthsEqual(WVectorD^ x, WVectorD^ y)
	{
		if (x->Count == y->Count)
			return true;
		else
			return false;
	}

	bool WLinAlg::AreVectorLengthsEqual(WVectorZ^ x, WVectorZ^ y)
	{
		if (x->Count == y->Count)
			return true;
		else
			return false;
	}

	bool WLinAlg::AreVectorLengthsEqual(WVectorDi^ x, WVectorD^ y)
	{
		if (x->Count == y->Count)
			return true;
		else
			return false;
	}

	bool WLinAlg::AreVectorLengthsEqual(WVectorDi^ x, WVectorDi^ y)
	{
		if (x->Count == y->Count)
			return true;
		else 
			return false;
	}

	bool WLinAlg::AreMatrixVectorDottable(WMatrixD^ a, WVectorD^ x)
	{
		if(a->Cols == x->Count)
			return true;
		else
			return false;
	}

	bool WLinAlg::AreMatrixVectorDottable(WMatrixZ^ a, WVectorZ^ x)
	{
		if (a->Cols == x->Count)
			return true;
		else
			return false;
	}

	bool WLinAlg::AreMatrixMatrixDottable(WMatrixD^ a, WMatrixD^ b)
	{
		if (a->Cols != b->Rows || a->Rows != b->Cols)
			return true;
		else
			return false;
	}

	bool WLinAlg::AreMatrixMatrixDottable(WMatrixZ^ a, WMatrixZ^ b)
	{
		if (a->Cols != b->Rows || a->Rows != b->Cols)
			return true;
		else
			return false;
	}

	double WLinAlg::AbsoluteSum(WVectorD^ x)
	{
		return factory->iBLAS->Asum(x->Count,
			x->Values, 1);
	}

	double WLinAlg::AbsoluteSum(WVectorZ^ x)
	{
		return factory->iBLAS->Asum(x->Count,
			x->Values, 1); 
	}

	void WLinAlg::Copy(WVectorD^ x, WVectorD^ y)
	{
		if (!AreVectorLengthsEqual(x, y)) 
			throw gcnew System::InvalidOperationException();
		factory->iBLAS->Copy(x->Count,
			x->Values, 1,
			y->Values, 1);
	}

	void WLinAlg::Copy(WVectorZ^ x, WVectorZ^ y)
	{
		if (!AreVectorLengthsEqual(x, y))
			throw gcnew System::InvalidOperationException();
		factory->iBLAS->Copy(x->Count,
			x->Values, 1,
			y->Values, 1);
	}

	void WLinAlg::AddTo(WVectorD^ x, WVectorD^ y)
	{
		if (!AreVectorLengthsEqual(x, y)) 
			throw gcnew System::InvalidOperationException();
		factory->iBLAS->Axpy(x->Count, 
			1.0, x->Values, 1,
			y->Values, 1);
	}

	void WLinAlg::AddTo(WVectorZ^ x, WVectorZ^ y)
	{
		if(!AreVectorLengthsEqual(x, y))
			throw gcnew System::InvalidOperationException();
		double* a = new double[2] {1.0, 0.0}; // complex factor alpha
		factory->iBLAS->Axpy(x->Count, 
			(void*)a, x->Values, 1,
			y->Values, 1);
	}

	void WLinAlg::AddTo(double alpha, WVectorD^ x, WVectorD^ y)
	{
		if (!AreVectorLengthsEqual(x, y)) 
			throw gcnew System::InvalidOperationException();
		factory->iBLAS->Axpy(x->Count, 
			alpha, x->Values, 1,
			y->Values, 1);
	}

	void WLinAlg::AddTo(WComplex alpha, WVectorZ^ x, WVectorZ^ y)
	{
		if (!AreVectorLengthsEqual(x, y))
			throw gcnew System::InvalidOperationException();
		double* a = new double[2] {alpha.Real, alpha.Imag};
		factory->iBLAS->Axpy(x->Count,
			(void*)a, x->Values, 1,
			y->Values, 1);
	}

	double WLinAlg::Dot(WVectorD^ x, WVectorD^ y)
	{
		if (!AreVectorLengthsEqual(x, y)) 
			throw gcnew System::InvalidOperationException();
		return factory->iBLAS->Dot(x->Count,
			x->Values, 1,
			y->Values, 1);
	}

	WComplex^ WLinAlg::Dot(WVectorZ^ x, WVectorZ^ y)
	{
		if (!AreVectorLengthsEqual(x, y))
			throw gcnew System::InvalidOperationException();
		double* dotu = new double[2];
		factory->iBLAS->Dotu(x->Count,
			x->Values, 1,
			y->Values, 1, (void*)dotu);
		return gcnew WComplex(dotu[0], dotu[1]);
	}

	WComplex^ WLinAlg::Dotc(WVectorZ^ x, WVectorZ^ y)
	{
		if (!AreVectorLengthsEqual(x, y))
			throw gcnew System::InvalidOperationException();
		double* dotc = new double[2];
		factory->iBLAS->Dotc(x->Count,
			x->Values, 1,
			y->Values, 1, (void*)dotc);
		return gcnew WComplex(dotc[0], dotc[1]);
	}

	double WLinAlg::Norm(WVectorD^ x)
	{
		return factory->iBLAS->Nrm2(x->Count,
			x->Values, 1);
	}

	double WLinAlg::Norm(WVectorZ^ x)
	{
		return factory->iBLAS->Nrm2(x->Count,
			x->Values, 1);
	}

	void WLinAlg::Scale(double alpha, WVectorD^ x)
	{
		factory->iBLAS->Scal(x->Count, alpha,
			x->Values, 1);
	}

	void WLinAlg::Scale(WComplex alpha, WVectorZ^ x)
	{
		double* a = new double[2] {alpha.Real, alpha.Imag};
		factory->iBLAS->Scal(x->Count, (void*)a,
			x->Values, 1);
	}

	long long WLinAlg::IndexMaxAbsolute(WVectorD^ x)
	{
		return factory->iBLAS->Iamax(x->Count,
			x->Values, 1);
	}

	long long WLinAlg::IndexMaxAbsolute(WVectorZ^ x)
	{
		return factory->iBLAS->Iamax(x->Count,
			x->Values, 1);
	}

	long long WLinAlg::IndexMinAbsolute(WVectorD^ x)
	{
		return factory->iBLAS->Iamin(x->Count,
			x->Values, 1);
	}

	long long WLinAlg::IndexMinAbsolute(WVectorZ^ x)
	{
		return factory->iBLAS->Iamin(x->Count,
			x->Values, 1);
	}

	long long WLinAlg::Iamax(WVectorDi^ xi, long long incx)
	{
		return factory->iBLAS->Iamax(xi->NonZeroCount,
			xi->NonZeroValues, incx);
	}

	long long WLinAlg::Iamax(WVectorZi^ xi, long long incx)
	{
		return factory->iBLAS->Iamax(xi->NonZeroCount,
			xi->NonZeroValues, incx);
	}

	long long WLinAlg::Iamin(WVectorDi^ xi, long long incx)
	{
		return factory->iBLAS->Iamin(xi->NonZeroCount,
			xi->NonZeroValues, incx);
	}

	long long WLinAlg::Iamin(WVectorZi^ xi, long long incx)
	{
		return factory->iBLAS->Iamin(xi->NonZeroCount,
			xi->NonZeroValues, incx);
	}

	void WLinAlg::Rotate(WVectorD^ x, WVectorD^ y, double c, double s)
	{
		if (!AreVectorLengthsEqual(x, y)) 
			throw gcnew System::InvalidOperationException();
		factory->iBLAS->Rot(x->Count,
			x->Values, 1,
			y->Values, 1,
			c, s);
	}

	void WLinAlg::Rotate(WVectorZ^ x, WVectorZ^ y, double c, double s)
	{
		if (!AreVectorLengthsEqual(x, y))
			throw gcnew System::InvalidOperationException();
		factory->iBLAS->Rot(x->Count,
			x->Values, 1,
			y->Values, 1,
			c, s);
	}

	void WLinAlg::Dot(double alpha, WMatrixD^ a, WVectorD^ x, double beta, WVectorD^ y)
	{
		if (!AreMatrixVectorDottable(a, x))
			throw gcnew System::InvalidOperationException();
		factory->iBLAS->Gemv(CblasRowMajor, CblasNoTrans, a->Rows, a->Cols,
			alpha, a->Values, a->Cols, x->Values, 1,
			beta, y->Values, 1);
	}

	WVectorD^ WLinAlg::Dot(WMatrixD^ a, WVectorD^ x)
	{
		WVectorD^ y = gcnew WVectorD(a->Rows);
		Dot(1.0, a, x, 0.0, y);
		return y;
	}

	void WLinAlg::Dot(double alpha, WMatrixD^ a, WMatrixD^ b, double beta, WMatrixD^ c)
	{
		if(!AreMatrixMatrixDottable(a, b))
			throw gcnew System::InvalidOperationException();
		factory->iBLAS->Gemm(CblasRowMajor, CblasNoTrans, CblasNoTrans,
			a->Rows, b->Cols, a->Cols, alpha, a->Values, a->Cols,
			b->Values, b->Cols, beta, c->Values, c->Cols);
	}

	WMatrixD^ WLinAlg::Dot(WMatrixD^ a, WMatrixD^ b)
	{
		WMatrixD^ m = gcnew WMatrixD(a->Rows, b->Cols);
		Dot(1.0, a, b, 0.0, m);
		return m;
	}

	void WLinAlg::Axpyi(WVectorDi^ xi, double* y, double alpha)
	{
		factory->iBLAS->Axpyi(xi->NonZeroCount, alpha, xi->NonZeroValues, xi->NonZeroIndices, y);
	}

	void WLinAlg::Axpyi(WVectorZi^ xi, void* y, WComplex^ alpha)
	{
		double* a = new double[2] {alpha->Real, alpha->Imag};
		factory->iBLAS->Axpyi(xi->NonZeroCount, (void*)a, xi->NonZeroValues, xi->NonZeroIndices, y);
	}

	double WLinAlg::Doti(WVectorDi^ xi, double* y)
	{
		return factory->iBLAS->Doti(xi->NonZeroCount, xi->NonZeroValues, xi->NonZeroIndices, y);
	}

	WComplex^ WLinAlg::Doti(WVectorZi^ xi, void* y)
	{
		double* dotu = new double[2];
		factory->iBLAS->Dotui(xi->NonZeroCount, xi->NonZeroValues, xi->NonZeroIndices, 
			y, (void*)dotu);
		return gcnew WComplex(dotu[0], dotu[1]);
	}

	WComplex^ WLinAlg::Dotci(WVectorZi^ xi, void* y)
	{
		double* dotc = new double[2];
		factory->iBLAS->Dotci(xi->NonZeroCount, xi->NonZeroValues, xi->NonZeroIndices, 
			y, (void*)dotc);
		return gcnew WComplex(dotc[0], dotc[1]);
	}

	WVectorDi^ WLinAlg::Gthr(long long n, double* y, long long nnz, long long* indx)
	{
		WVectorDi^ xi = gcnew WVectorDi(n, nnz);
		xi->SetNonZeroIndices(indx);
		factory->iBLAS->Gthr(nnz, y, xi->NonZeroValues, indx);
		return xi;
	}

	void WLinAlg::Gthr(long long n, double* y, long long nnz, double* x, long long* indx)
	{
		factory->iBLAS->Gthr(nnz, y, x, indx);
	}

	WVectorZi^ WLinAlg::Gthr(long long n, void* y, long long nnz, long long* indx)
	{
		WVectorZi^ xi = gcnew WVectorZi(n, nnz);
		xi->SetNonZeroIndices(indx);
		factory->iBLAS->Gthr(nnz, y, xi->NonZeroValues, indx);
		return xi;
	}

	void WLinAlg::Gthr(long long n, void* y, long long nnz, void* x, long long* indx)
	{
		factory->iBLAS->Gthr(nnz, y, x, indx);
	}

	void WLinAlg::Roti(WVectorDi^ xi, double* y, double c, double s)
	{
		factory->iBLAS->Roti(xi->NonZeroCount, xi->NonZeroValues, xi->NonZeroIndices,
			y, c, s);
	}

	void WLinAlg::Roti(WVectorZi^ xi, void* y, double c, double s)
	{
		factory->iBLAS->Roti(xi->NonZeroCount, xi->NonZeroValues, xi->NonZeroIndices,
			y, c, s);
	}

	void WLinAlg::Sctr(WVectorDi^ xi, double* y)
	{
		factory->iBLAS->Sctr(xi->NonZeroCount, xi->NonZeroValues, xi->NonZeroIndices, y);
	}

	void WLinAlg::Sctr(WVectorZi^ xi, void* y)
	{
		factory->iBLAS->Sctr(xi->NonZeroCount, xi->NonZeroValues, xi->NonZeroIndices, y);
	}

	double WLinAlg::Asum(WVectorDi^ xi, long long incx)
	{
		return factory->iBLAS->Asum(xi->Count, xi->NonZeroValues, incx);
	}

	double WLinAlg::Asum(WVectorZi^ xi, long long incx)
	{
		return factory->iBLAS->Asum(xi->Count, xi->NonZeroValues, incx);
	}

	void WLinAlg::Copy(WVectorDi^ xi, WVectorDi^ yi, long long incx, long long incy)
	{
		factory->iBLAS->Copy(xi->NonZeroCount, xi->NonZeroValues, incx,
			yi->NonZeroValues, incy);
	}

	void WLinAlg::Copy(WVectorZi^ xi, WVectorZi^ yi, long long incx, long long incy)
	{
		factory->iBLAS->Copy(xi->NonZeroCount, xi->NonZeroValues, incx,
			yi->NonZeroValues, incy);
	}

	double WLinAlg::Nrm2(WVectorDi^ xi, long long incx)
	{
		return factory->iBLAS->Nrm2(xi->Count, xi->NonZeroValues, incx);
	}

	double WLinAlg::Nrm2(WVectorZi^ xi, long long incx)
	{
		return factory->iBLAS->Nrm2(xi->Count, xi->NonZeroValues, incx);
	}

	void WLinAlg::Scal(double alpha, WVectorDi^ xi, long long incx)
	{
		factory->iBLAS->Scal(xi->NonZeroCount, alpha, xi->NonZeroValues, incx);
	}

	void WLinAlg::Scal(WComplex^ alpha, WVectorZi^ xi, long long incx)
	{
		double* a = new double[2] {alpha->Real, alpha->Imag};
		factory->iBLAS->Scal(xi->NonZeroCount, (void*)a, xi->NonZeroValues, incx);
	}

	void WLinAlg::SparseCreateCSR(WMatrixDi^ a, long long* rowPtr, long long* col_indx, double* values)
	{
		sparse_status_t s = factory->iBLAS->SparseCreateCSR(a->Handle, a->Indexing, a->Rows, a->Cols,
			rowPtr, rowPtr + 1, col_indx, values);
		a->Status = s;
	}

	void WLinAlg::SparseCreateCSR(WMatrixZi^ a, long long* rowPtr, long long* col_indx, void* values)
	{
		sparse_status_t s = factory->iBLAS->SparseCreateCSR(a->Handle, a->Indexing, a->Rows, a->Cols,
			rowPtr, rowPtr + 1, col_indx, values);
		a->Status = s;
	}

	void WLinAlg::SparseCreateCSC(WMatrixDi^ a, long long* colPtr, long long* row_indx, double* values)
	{
		sparse_status_t s = factory->iBLAS->SparseCreateCSC(a->Handle, a->Indexing, a->Rows, a->Cols,
			colPtr, colPtr + 1, row_indx, values);
		a->Status = s;
	}

	void WLinAlg::SparseCreateCSC(WMatrixZi^ a, long long* colPtr, long long* row_indx, void* values)
	{
		sparse_status_t s = factory->iBLAS->SparseCreateCSC(a->Handle, a->Indexing, a->Rows, a->Cols,
			colPtr, colPtr + 1, row_indx, values);
		a->Status = s;
	}

	void WLinAlg::SparseCreateCOO(WMatrixDi^ a, long long* row_indx, long long* col_indx, double* values)
	{
		sparse_status_t s = factory->iBLAS->SparseCreateCOO(a->Handle, a->Indexing, a->Rows, a->Cols, a->NonZeroCount,
			row_indx, col_indx, values);
		a->Status = s;
	}

	void WLinAlg::SparseCreateCOO(WMatrixZi^ a, long long* row_indx, long long* col_indx, void* values)
	{
		sparse_status_t s = factory->iBLAS->SparseCreateCOO(a->Handle, a->Indexing, a->Rows, a->Cols, a->NonZeroCount,
			row_indx, col_indx, values);
		a->Status = s;
	}

	WMatrixDi^ WLinAlg::SparseCopy(WMatrixDi^ a)
	{
		WMatrixDi^ dest = gcnew WMatrixDi(a->Rows, a->Cols, a->NonZeroCount);

		struct matrix_descr matDes;
		matDes.diag = SPARSE_DIAG_NON_UNIT;
		matDes.mode = SPARSE_FILL_MODE_FULL;
		matDes.type = SPARSE_MATRIX_TYPE_GENERAL;
		
		sparse_status_t s = factory->iBLAS->SparseCopy(*(a->Handle), matDes, dest->Handle);
		if(s != SPARSE_STATUS_SUCCESS){ throw gcnew System::Exception(); }
		return dest;
	}

	WMatrixZi^ WLinAlg::SparseCopy(WMatrixZi^ a)
	{
		WMatrixZi^ dest = gcnew WMatrixZi(a->Rows, a->Cols, a->NonZeroCount);

		struct matrix_descr matDes;
		matDes.diag = SPARSE_DIAG_NON_UNIT;
		matDes.mode = SPARSE_FILL_MODE_FULL;
		matDes.type = SPARSE_MATRIX_TYPE_GENERAL;

		sparse_status_t s = factory->iBLAS->SparseCopy(*(a->Handle), matDes, dest->Handle);
		if (s != SPARSE_STATUS_SUCCESS) { throw gcnew System::Exception(); }
		return dest;
	}

	void WLinAlg::SparseDestroy(WMatrixDi^ a)
	{
		sparse_status_t s = factory->iBLAS->SparseDestroy(*(a->Handle));
		a->Status = s;
	}

	void WLinAlg::SparseDestroy(WMatrixZi^ a)
	{
		sparse_status_t s = factory->iBLAS->SparseDestroy(*(a->Handle));
		a->Status = s;
	}

	WMatrixDi^ WLinAlg::SparseConvert2CSR(WMatrixDi^ a, int operation)
	{
		sparse_operation_t op = SparseOperationConverter(operation);
		
		WMatrixDi^ dest;
		if (op == SPARSE_OPERATION_NON_TRANSPOSE)
			dest = gcnew WMatrixDi(a->Rows, a->Cols, a->NonZeroCount);
		else
			dest = gcnew WMatrixDi(a->Cols, a->Rows, a->NonZeroCount);
		
		sparse_status_t s = factory->iBLAS->SparseConvert2CSR(*(a->Handle), op, dest->Handle);
		if (s != SPARSE_STATUS_SUCCESS) { throw gcnew System::Exception(); }
		return dest;
	}

	WMatrixZi^ WLinAlg::SparseConvert2CSR(WMatrixZi^ a, int operation)
	{
		sparse_operation_t op = SparseOperationConverter(operation);

		WMatrixZi^ dest;
		if (op == SPARSE_OPERATION_NON_TRANSPOSE)
			dest = gcnew WMatrixZi(a->Rows, a->Cols, a->NonZeroCount);
		else
			dest = gcnew WMatrixZi(a->Cols, a->Rows, a->NonZeroCount);

		sparse_status_t s = factory->iBLAS->SparseConvert2CSR(*(a->Handle), op, dest->Handle);
		if (s != SPARSE_STATUS_SUCCESS) { throw gcnew System::Exception(); }
		return dest;
	}

	sparse_status_t WLinAlg::SparseExportCSR(sparse_matrix_t source, sparse_index_base_t* indexing, long long* rows, long long* cols, long long** rows_start, long long** rows_end, long long** col_indx, double** values)
	{
		return factory->iBLAS->SparseExportCSR(source, indexing, rows, cols,
			rows_start, rows_end, col_indx, values);
	}

	//sparse_status_t WLinAlg::SparseExportCSR(sparse_matrix_t source,
	//	long long* rows, long long* cols,
	//	long long** rows_start, long long** rows_end, long long** col_indx)
	//{
	//	sparse_index_base_t indexing = SPARSE_INDEX_BASE_ZERO;
	//	return factory->iBLAS->SparseExportCSR(source, &indexing, rows, cols,
	//		rows_start, rows_end, col_indx, values);
	//}

	void WLinAlg::SparseSetValue(WMatrixDi^ a, long long row, long long col, double value)
	{
		sparse_status_t s = factory->iBLAS->SparseSetValue(*(a->Handle), row, col, value);
		if (s != SPARSE_STATUS_SUCCESS) { throw gcnew System::Exception(); }
	}

	void WLinAlg::SparseSetValue(WMatrixZi^ a, long long row, long long col, WComplex^ value)
	{
		sparse_status_t s = factory->iBLAS->SparseSetValue(*(a->Handle), row, col, value);
		if (s != SPARSE_STATUS_SUCCESS) { throw gcnew System::Exception(); }
	}

	void WLinAlg::SparseUpdateValues(WMatrixDi^ a, long long nvalues, long long* row_indx, long long* col_indx, double* values)
	{
		sparse_status_t s = factory->iBLAS->SparseUpdateValues(*(a->Handle),
			nvalues, row_indx, col_indx, values);
		if (s != SPARSE_STATUS_SUCCESS) { throw gcnew System::Exception(); }
	}

	void WLinAlg::SparseUpdateValues(WMatrixZi^ a, long long nvalues, long long* row_indx, long long* col_indx, void* values)
	{
		sparse_status_t s = factory->iBLAS->SparseUpdateValues(*(a->Handle),
			nvalues, row_indx, col_indx, values);
		if (s != SPARSE_STATUS_SUCCESS) { throw gcnew System::Exception(); }
	}

	void WLinAlg::SparseSetMVHint(WMatrixDi^ a, int operation, int matrixType, int fillMode, int diagType, long long expected_calls)
	{
		sparse_operation_t op = SparseOperationConverter(operation);
		
		matrix_descr matDes;
		matDes.type = SparseMatrixTypeConverter(matrixType);
		matDes.mode = SparseFillModeConverter(fillMode);
		matDes.diag = SparseDiagTypeConverter(diagType);

		sparse_status_t s = factory->iBLAS->SparseSetMVHint(*(a->Handle), op, matDes, expected_calls);
		if (s != SPARSE_STATUS_SUCCESS) { throw gcnew System::Exception(); }
	}

	void WLinAlg::SparseSetSVHint(WMatrixDi^ a, int operation, int matrixType, int fillMode, int diagType, long long expected_calls)
	{
		sparse_operation_t op = SparseOperationConverter(operation);

		matrix_descr matDes;
		matDes.type = SparseMatrixTypeConverter(matrixType);
		matDes.mode = SparseFillModeConverter(fillMode);
		matDes.diag = SparseDiagTypeConverter(diagType);

		sparse_status_t s = factory->iBLAS->SparseSetSVHint(*(a->Handle), op, matDes, expected_calls);
		if (s != SPARSE_STATUS_SUCCESS) { throw gcnew System::Exception(); }
	}

	void WLinAlg::SparseSetMMHint(WMatrixDi^ a, int operation, int matrixType, int fillMode, int diagType, int layout, long long dense_matrix_size, long long expected_calls)
	{
		sparse_operation_t op = SparseOperationConverter(operation);

		matrix_descr matDes;
		matDes.type = SparseMatrixTypeConverter(matrixType);
		matDes.mode = SparseFillModeConverter(fillMode);
		matDes.diag = SparseDiagTypeConverter(diagType);

		sparse_layout_t lay = SparseLayoutConverter(layout);

		sparse_status_t s = factory->iBLAS->SparseSetMMHint(*(a->Handle), op, matDes, lay, dense_matrix_size, expected_calls);
		if (s != SPARSE_STATUS_SUCCESS) { throw gcnew System::Exception(); }
	}

	void WLinAlg::SparseSetSMHint(WMatrixDi^ a, int operation, int matrixType, int fillMode, int diagType, int layout, long long dense_matrix_size, long long expected_calls)
	{
		sparse_operation_t op = SparseOperationConverter(operation);

		matrix_descr matDes;
		matDes.type = SparseMatrixTypeConverter(matrixType);
		matDes.mode = SparseFillModeConverter(fillMode);
		matDes.diag = SparseDiagTypeConverter(diagType);

		sparse_layout_t lay = SparseLayoutConverter(layout);

		sparse_status_t s = factory->iBLAS->SparseSetSMHint(*(a->Handle), op, matDes, lay, dense_matrix_size, expected_calls);
		if (s != SPARSE_STATUS_SUCCESS) { throw gcnew System::Exception(); }
	}

	void WLinAlg::SparseSetMemoryHint(WMatrixDi^ a, int policy)
	{
		sparse_memory_usage_t usage = SparseMemoryUsageConverter(policy);
		sparse_status_t s = factory->iBLAS->SparseSetMemoryHint(*(a->Handle), usage);
		if (s != SPARSE_STATUS_SUCCESS) { throw gcnew System::Exception(); }
	}

	void WLinAlg::SparseOptimize(WMatrixDi^ a)
	{
		sparse_status_t s = factory->iBLAS->SparseOptimize(*(a->Handle));
		if (s != SPARSE_STATUS_SUCCESS) { throw gcnew System::Exception(); }
	}

	void WLinAlg::SparseMV(int operation, double alpha, WMatrixDi^ a, int matrixType, int fillMode, int diagType, double* x, double beta, double* y)
	{
		sparse_operation_t op = SparseOperationConverter(operation);

		matrix_descr matDes;
		matDes.type = SparseMatrixTypeConverter(matrixType);
		matDes.mode = SparseFillModeConverter(fillMode);
		matDes.diag = SparseDiagTypeConverter(diagType);

		sparse_status_t s = factory->iBLAS->SparseMV(op, alpha, *(a->Handle), matDes, x, beta, y);
		if (s != SPARSE_STATUS_SUCCESS) { throw gcnew System::Exception(); }
	}

	void WLinAlg::SparseTRSV(int operation, double alpha, WMatrixDi^ a, int matrixType, int fillMode, int diagType, double* x, double* y)
	{
		sparse_operation_t op = SparseOperationConverter(operation);

		matrix_descr matDes;
		matDes.type = SparseMatrixTypeConverter(matrixType);
		matDes.mode = SparseFillModeConverter(fillMode);
		matDes.diag = SparseDiagTypeConverter(diagType);

		sparse_status_t s = factory->iBLAS->SparseTRSV(op, alpha, *(a->Handle), matDes, x, y);
		if (s != SPARSE_STATUS_SUCCESS) { throw gcnew System::Exception(); }
	}

	void WLinAlg::SparseMM(int operation, double alpha, WMatrixDi^ a, int matrixType, int fillMode, int diagType, int layout, double* b, long long columns, long long ldb, double beta, double* c, long long ldc)
	{
		sparse_operation_t op = SparseOperationConverter(operation);

		matrix_descr matDes;
		matDes.type = SparseMatrixTypeConverter(matrixType);
		matDes.mode = SparseFillModeConverter(fillMode);
		matDes.diag = SparseDiagTypeConverter(diagType);

		sparse_layout_t lay = SparseLayoutConverter(layout);

		sparse_status_t s = factory->iBLAS->SparseMM(op, alpha, *(a->Handle), matDes,
			lay, b, columns, ldb, beta, c, ldc);
		if (s != SPARSE_STATUS_SUCCESS) { throw gcnew System::Exception(); }
	}

	void WLinAlg::SparseTRSM(int operation, double alpha, WMatrixDi^ a, int matrixType, int fillMode, int diagType, int layout, double* x, long long columns, long long ldx, double* y, long long ldy)
	{
		sparse_operation_t op = SparseOperationConverter(operation);

		matrix_descr matDes;
		matDes.type = SparseMatrixTypeConverter(matrixType);
		matDes.mode = SparseFillModeConverter(fillMode);
		matDes.diag = SparseDiagTypeConverter(diagType);

		sparse_layout_t lay = SparseLayoutConverter(layout);

		sparse_status_t s = factory->iBLAS->SparseTRSM(op, alpha, *(a->Handle), matDes,
			lay, x, columns, ldx, y, ldy);
		if (s != SPARSE_STATUS_SUCCESS) { throw gcnew System::Exception(); }
	}

	WMatrixDi^ WLinAlg::SparseAdd(int operation, WMatrixDi^ a, double alpha, WMatrixDi^ b)
	{
		WMatrixDi^ c = gcnew WMatrixDi(a->Rows, a->Cols, a->NonZeroCount);
		
		sparse_operation_t op = SparseOperationConverter(operation);
		sparse_status_t s = factory->iBLAS->SparseAdd(op, *(a->Handle), alpha, *(b->Handle), c->Handle);
		if (s != SPARSE_STATUS_SUCCESS) { throw gcnew System::Exception(); }
	}

	void WLinAlg::SparseSetQRHint(WMatrixDi^ a)
	{
		sparse_status_t s = factory->iBLAS->SparseSetQRHint(*(a->Handle), SPARSE_QR_WITH_PIVOTS);
		if (s != SPARSE_STATUS_SUCCESS) { throw gcnew System::Exception(); }
	}

	void WLinAlg::SparseQR(WMatrixDi^ a, int operation, int matrixType, int fillMode, int diagType, int layout, long long columns, double* x, long long ldx, double* b, long long ldb)
	{
		sparse_operation_t op = SparseOperationConverter(operation);

		matrix_descr matDes;
		matDes.type = SparseMatrixTypeConverter(matrixType);
		matDes.mode = SparseFillModeConverter(fillMode);
		matDes.diag = SparseDiagTypeConverter(diagType);

		sparse_layout_t lay = SparseLayoutConverter(layout);
		
		sparse_status_t s = factory->iBLAS->SparseQR(op, *(a->Handle), matDes, lay, columns, x, ldx, b, ldb);
		if (s != SPARSE_STATUS_SUCCESS) { throw gcnew System::Exception(); }
	}

	void WLinAlg::SparseQRReorder(WMatrixDi^ a, int matrixType, int fillMode, int diagType)
	{
		matrix_descr matDes;
		matDes.type = SparseMatrixTypeConverter(matrixType);
		matDes.mode = SparseFillModeConverter(fillMode);
		matDes.diag = SparseDiagTypeConverter(diagType);

		sparse_status_t s = factory->iBLAS->SparseQRReorder(*(a->Handle), matDes);
		if (s != SPARSE_STATUS_SUCCESS) { throw gcnew System::Exception(); }
	}

	void WLinAlg::SparseQRFactorize(WMatrixDi^ a, double* alt_values)
	{
		//sparse_status_t s = factory->iBLAS->SparseQRFactorize(*(a->Handle), alt_values);
		sparse_status_t s = factory->iBLAS->SparseQRFactorize(*(a->Handle), NULL);
		if (s != SPARSE_STATUS_SUCCESS) { throw gcnew System::Exception(); }
	}

	void WLinAlg::SparseQRSolve(WMatrixDi^ a, double* alt_values, int operation, int layout, long long columns, double* x, long long ldx, double* b, long long ldb)
	{
		sparse_operation_t op = SparseOperationConverter(operation);
		sparse_layout_t lay = SparseLayoutConverter(layout);

		//sparse_status_t s = factory->iBLAS->SparseQRSolve(op, *(a->Handle), alt_values, lay, columns, x, ldx, b, ldb);
		sparse_status_t s = factory->iBLAS->SparseQRSolve(op, *(a->Handle), NULL, lay, columns, x, ldx, b, ldb);
		if (s != SPARSE_STATUS_SUCCESS) { throw gcnew System::Exception(); }
	}

	void WLinAlg::SparseQRQmult(WMatrixDi^ a, int operation, int layout, long long columns, double* x, long long ldx, double* b, long long ldb)
	{
		sparse_operation_t op = SparseOperationConverter(operation);
		sparse_layout_t lay = SparseLayoutConverter(layout);

		sparse_status_t s = factory->iBLAS->SparseQRQmult(op, *(a->Handle), lay, columns, x, ldx, b, ldb);
		if (s != SPARSE_STATUS_SUCCESS) { throw gcnew System::Exception(); }
	}

	void WLinAlg::SparseQRRsolve(WMatrixDi^ a, int operation, int layout, long long columns, double* x, long long ldx, double* b, long long ldb)
	{
		sparse_operation_t op = SparseOperationConverter(operation);
		sparse_layout_t lay = SparseLayoutConverter(layout);

		sparse_status_t s = factory->iBLAS->SparseQRRsolve(op, *(a->Handle), lay, columns, x, ldx, b, ldb);
		if (s != SPARSE_STATUS_SUCCESS) { throw gcnew System::Exception(); }
	}




	sparse_operation_t WLinAlg::SparseOperationConverter(int operation)
	{
		if (operation == 10)
			return SPARSE_OPERATION_NON_TRANSPOSE;
		else if (operation == 11)
			return SPARSE_OPERATION_TRANSPOSE;
		else if (operation == 12)
			return SPARSE_OPERATION_CONJUGATE_TRANSPOSE;
		else
			return SPARSE_OPERATION_NON_TRANSPOSE;
	}

	sparse_matrix_type_t WLinAlg::SparseMatrixTypeConverter(int type)
	{
		if (type == 20)
			return SPARSE_MATRIX_TYPE_GENERAL;
		else if (type == 21)
			return SPARSE_MATRIX_TYPE_SYMMETRIC;
		else if (type == 22)
			return SPARSE_MATRIX_TYPE_HERMITIAN;
		else if (type == 23)
			return SPARSE_MATRIX_TYPE_TRIANGULAR;
		else if (type == 24)
			return SPARSE_MATRIX_TYPE_DIAGONAL;
		else if (type == 25)
			return SPARSE_MATRIX_TYPE_BLOCK_TRIANGULAR;
		else if (type == 26)
			return SPARSE_MATRIX_TYPE_BLOCK_DIAGONAL;
		else
			return SPARSE_MATRIX_TYPE_GENERAL;
	}

	sparse_fill_mode_t WLinAlg::SparseFillModeConverter(int mode)
	{
		if (mode == 40)
			return SPARSE_FILL_MODE_LOWER;
		else if (mode == 41)
			return SPARSE_FILL_MODE_UPPER;
		else if (mode == 42)
			return SPARSE_FILL_MODE_FULL;
		else
			return SPARSE_FILL_MODE_FULL;
	}

	sparse_diag_type_t WLinAlg::SparseDiagTypeConverter(int type)
	{
		if (type == 50)
			return SPARSE_DIAG_NON_UNIT;
		else if (type == 51)
			return SPARSE_DIAG_UNIT;
		else
			return SPARSE_DIAG_NON_UNIT;
	}

	/// <summary>
	/// converts sparse layout from integer format
	/// into the sparse_layout_t format
	/// </summary>
	/// <param name="layout"> input in the integer format </param>
	/// <returns> output in the sparse_layout_t format </returns>
	sparse_layout_t WLinAlg::SparseLayoutConverter(int layout)
	{
		if (layout == 101)
			return SPARSE_LAYOUT_ROW_MAJOR;
		else if (layout == 102)
			return SPARSE_LAYOUT_COLUMN_MAJOR;
		else
			return SPARSE_LAYOUT_ROW_MAJOR;
	}

	/// <summary>
	/// converts sparse memory usage from integer format
	/// into the sparse_memoery_usage_t format
	/// </summary>
	/// <param name="usage"> input in the integer format </param>
	/// <returns> output in the sparse_memory_usage_t format </returns>
	sparse_memory_usage_t WLinAlg::SparseMemoryUsageConverter(int usage)
	{
		if (usage == 80)
			return SPARSE_MEMORY_NONE;
		else if (usage == 81)
			return SPARSE_MEMORY_AGGRESSIVE;
		else
			return SPARSE_MEMORY_NONE;
	}

	/// <summary>
	/// converts sparse status to integer format
	/// </summary>
	/// <param name="status"> input sparse status </param>
	/// <returns> output in integer format </returns>
	int WLinAlg::SparseStatusConverter(sparse_status_t status)
	{
		if (status == SPARSE_STATUS_SUCCESS)
			return 0;
		else if (status == SPARSE_STATUS_NOT_INITIALIZED)
			return 1;
		else if (status == SPARSE_STATUS_ALLOC_FAILED)
			return 2;
		else if (status == SPARSE_STATUS_INVALID_VALUE)
			return 3;
		else if (status == SPARSE_STATUS_EXECUTION_FAILED)
			return 4;
		else if (status == SPARSE_STATUS_INTERNAL_ERROR)
			return 5;
		else if (status == SPARSE_STATUS_NOT_SUPPORTED)
			return 6;
	}

}

