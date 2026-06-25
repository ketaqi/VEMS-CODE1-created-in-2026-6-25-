#include "pch.h"
#include "WMathCore.h"

namespace WMathCore
{

	double IBLAS::Asum(long long n, double* x, long long incx)
	{
		throw gcnew System::NotImplementedException();
	}
	double IBLAS::Asum(long long n, void* x, long long incx)
	{
		throw gcnew System::NotImplementedException();
	}
	void IBLAS::Axpy(long long n, void* alpha, void* x, long long incx, void* y, long long incy)
	{
		throw gcnew System::NotImplementedException();
	}
	void IBLAS::Copy(long long n, double* x, long long incx, double* y, long long incy)
	{
		throw gcnew System::NotImplementedException();
	}
	void IBLAS::Copy(long long n, void* x, long long incx, void* y, long long incy)
	{
		throw gcnew System::NotImplementedException();
	}
	void IBLAS::Axpy(long long n, double alpha, double* x, long long incx, double* y, long long incy)
	{
		throw gcnew System::NotImplementedException();
	}

	void IBLAS::Gemv(CBLAS_LAYOUT layout, CBLAS_TRANSPOSE operation, long long m, long long n, double alpha, double* a, long long lda, double* x, long long incx, double beta, double* y, long long incy)
	{
		throw gcnew System::NotImplementedException();
	}

	void IBLAS::Gemm(CBLAS_LAYOUT layout, CBLAS_TRANSPOSE operationa, CBLAS_TRANSPOSE operationb, long long m, long long n, long long k, double alpha, double* a, long long lda, double* b, long long ldb, double beta, double* c, long long ldc)
	{
		throw gcnew System::NotImplementedException();
	}

	void IBLAS::Axpyi(long long nz, double alpha, double* x, long long* indx, double* y)
	{
		throw gcnew System::NotImplementedException();
	}

	void IBLAS::Axpyi(long long nz, void* alpha, void* x, long long* indx, void* y)
	{
		throw gcnew System::NotImplementedException();
	}

	double IBLAS::Dot(long long n, double* x, long long incx, double* y, long long incy)
	{
		throw gcnew System::NotImplementedException();
	}

	void IBLAS::Dotu(long long n, void* x, long long incx, void* y, long long incy, void* dotu)
	{
		throw gcnew System::NotImplementedException();
	}

	void IBLAS::Dotc(long long n, void* x, long long incx, void* y, long long incy, void* dotc)
	{
		throw gcnew System::NotImplementedException();
	}

	double IBLAS::Doti(long long nz, double* x, long long* indx, double* y)
	{
		throw gcnew System::NotImplementedException();
	}

	void IBLAS::Dotui(long long nz, void* x, long long* indx, void* y, void* dotu)
	{
		throw gcnew System::NotImplementedException();
	}

	void IBLAS::Dotci(long long nz, void* x, long long* indx, void* y, void* dotc)
	{
		throw gcnew System::NotImplementedException();
	}

	void IBLAS::Gthr(long long nz, double* y, double* x, long long* indx)
	{
		throw gcnew System::NotImplementedException();
	}

	void IBLAS::Gthr(long long nz, void* y, void* x, long long* indx)
	{
		throw gcnew System::NotImplementedException();
	}

	void IBLAS::Roti(long long nz, double* x, long long* indx, double* y, double c, double s)
	{
		throw gcnew System::NotImplementedException();
	}

	void IBLAS::Roti(long long nz, void* x, long long* indx, void* y, double c, double s)
	{
		throw gcnew System::NotImplementedException();
	}

	void IBLAS::Sctr(long long nz, double* x, long long* indx, double* y)
	{
		throw gcnew System::NotImplementedException();
	}

	void IBLAS::Sctr(long long nz, void* x, long long* indx, void* y)
	{
		throw gcnew System::NotImplementedException();
	}

	sparse_status_t IBLAS::SparseCreateCSR(sparse_matrix_t* A, const sparse_index_base_t indexing, long long rows, long long cols, long long* rows_start, long long* rows_end, long long* col_indx, double* values)
	{
		return sparse_status_t();
	}

	sparse_status_t IBLAS::SparseCreateCSR(sparse_matrix_t* handle, sparse_index_base_t indexing, long long rows, long long cols, long long* rows_start, long long* rows_end, long long* col_indx, void* values)
	{
		return sparse_status_t();
	}

	sparse_status_t IBLAS::SparseCreateCSC(sparse_matrix_t* handle, sparse_index_base_t indexing, long long rows, long long cols, long long* cols_start, long long* cols_end, long long* row_indx, double* values)
	{
		return sparse_status_t();
	}

	sparse_status_t IBLAS::SparseCreateCSC(sparse_matrix_t* handle, sparse_index_base_t indexing, long long rows, long long cols, long long* cols_start, long long* cols_end, long long* row_indx, void* values)
	{
		return sparse_status_t();
	}

	sparse_status_t IBLAS::SparseCreateCOO(sparse_matrix_t* A, sparse_index_base_t indexing, long long rows, long long cols, long long nnz, long long* row_indx, long long* col_indx, double* values)
	{
		return sparse_status_t();
	}

	sparse_status_t IBLAS::SparseCreateCOO(sparse_matrix_t* handle, sparse_index_base_t indexing, long long rows, long long cols, long long nnz, long long* row_indx, long long* col_indx, void* values)
	{
		return sparse_status_t();
	}

	sparse_status_t IBLAS::SparseCopy(sparse_matrix_t source, matrix_descr descr, sparse_matrix_t* dest)
	{
		return sparse_status_t();
	}

	sparse_status_t IBLAS::SparseDestroy(sparse_matrix_t a)
	{
		return sparse_status_t();
	}

	sparse_status_t IBLAS::SparseConvert2CSR(sparse_matrix_t source, sparse_operation_t operation, sparse_matrix_t* dest)
	{
		return sparse_status_t();
	}

	sparse_status_t IBLAS::SparseExportCSR(sparse_matrix_t source, sparse_index_base_t* indexing, long long* rows, long long* cols, long long** rows_start, long long** rows_end, long long** col_indx, double** values)
	{
		return sparse_status_t();
	}

	sparse_status_t IBLAS::SparseExportCSC(sparse_matrix_t source, sparse_index_base_t* indexing, long long* rows, long long* cols, long long** cols_start, long long** cols_end, long long** row_indx, double** values)
	{
		return sparse_status_t();
	}

	sparse_status_t IBLAS::SparseSetValue(sparse_matrix_t a, long long row, long long col, double value)
	{
		return sparse_status_t();
	}

	sparse_status_t IBLAS::SparseSetValue(sparse_matrix_t a, long long row, long long col, WComplex^ value)
	{
		return sparse_status_t();
	}

	sparse_status_t IBLAS::SparseUpdateValues(sparse_matrix_t a, long long nvalues, long long* row_indx, long long* col_indx, double* values)
	{
		return sparse_status_t();
	}

	sparse_status_t IBLAS::SparseUpdateValues(sparse_matrix_t a, long long nvalues, long long* row_indx, long long* col_indx, void* values)
	{
		return sparse_status_t();
	}

	sparse_status_t IBLAS::SparseSetMVHint(sparse_matrix_t a, sparse_operation_t operation, matrix_descr descr, long long expected_calls)
	{
		return sparse_status_t();
	}

	sparse_status_t IBLAS::SparseSetSVHint(sparse_matrix_t a, sparse_operation_t operation, matrix_descr descr, long long expected_calls)
	{
		return sparse_status_t();
	}

	sparse_status_t IBLAS::SparseSetMMHint(sparse_matrix_t a, sparse_operation_t operation, matrix_descr descr, sparse_layout_t layout, long long dense_matrix_size, long long expected_calls)
	{
		return sparse_status_t();
	}

	sparse_status_t IBLAS::SparseSetSMHint(sparse_matrix_t a, sparse_operation_t operation, matrix_descr descr, sparse_layout_t layout, long long dense_matrix_size, long long expected_calls)
	{
		return sparse_status_t();
	}

	sparse_status_t IBLAS::SparseSetMemoryHint(sparse_matrix_t a, sparse_memory_usage_t policy)
	{
		return sparse_status_t();
	}

	sparse_status_t IBLAS::SparseOptimize(sparse_matrix_t a)
	{
		return sparse_status_t();
	}

	sparse_status_t IBLAS::SparseMV(sparse_operation_t operation, double alpha, sparse_matrix_t a, matrix_descr descr, double* x, double beta, double* y)
	{
		return sparse_status_t();
	}

	sparse_status_t IBLAS::SparseTRSV(sparse_operation_t operation, double alpha, sparse_matrix_t a, matrix_descr descr, double* x, double* y)
	{
		return sparse_status_t();
	}

	sparse_status_t IBLAS::SparseMM(sparse_operation_t operation, double alpha, sparse_matrix_t a, matrix_descr descr, sparse_layout_t layout, double* b, long long columns, long long ldb, double beta, double* c, long long ldc)
	{
		return sparse_status_t();
	}

	sparse_status_t IBLAS::SparseTRSM(sparse_operation_t operation, double alpha, sparse_matrix_t a, matrix_descr descr, const sparse_layout_t layout, double* x, long long columns, long long ldx, double* y, long long ldy)
	{
		return sparse_status_t();
	}

	sparse_status_t IBLAS::SparseAdd(sparse_operation_t operation, sparse_matrix_t a, double alpha, sparse_matrix_t b, sparse_matrix_t* c)
	{
		return sparse_status_t();
	}

	sparse_status_t IBLAS::SparseSPMM(sparse_operation_t operation, sparse_matrix_t a, sparse_matrix_t b, sparse_matrix_t* c)
	{
		return sparse_status_t();
	}

	sparse_status_t IBLAS::SparseSPMMD(sparse_operation_t operation, sparse_matrix_t a, sparse_matrix_t b, sparse_layout_t layout, double* c, long long ldc)
	{
		return sparse_status_t();
	}

	sparse_status_t IBLAS::SparseSetQRHint(sparse_matrix_t a, sparse_qr_hint_t hint)
	{
		return sparse_status_t();
	}

	sparse_status_t IBLAS::SparseQR(sparse_operation_t operation, sparse_matrix_t A, matrix_descr descr, sparse_layout_t layout, long long columns, double* x, long long ldx, double* b, long long ldb)
	{
		return sparse_status_t();
	}

	sparse_status_t IBLAS::SparseQRReorder(sparse_matrix_t a, matrix_descr descr)
	{
		return sparse_status_t();
	}

	sparse_status_t IBLAS::SparseQRFactorize(sparse_matrix_t a, double* alt_values)
	{
		return sparse_status_t();
	}

	sparse_status_t IBLAS::SparseQRSolve(sparse_operation_t operation, sparse_matrix_t a, double* alt_values, sparse_layout_t layout, long long columns, double* x, long long ldx, double* b, long long ldb)
	{
		return sparse_status_t();
	}

	sparse_status_t IBLAS::SparseQRQmult(sparse_operation_t operation, sparse_matrix_t a, sparse_layout_t layout, long long columns, double* x, long long ldx, double* b, long long ldb)
	{
		return sparse_status_t();
	}

	sparse_status_t IBLAS::SparseQRRsolve(sparse_operation_t operation, sparse_matrix_t a, sparse_layout_t layout, long long columns, double* x, long long ldx, double* b, long long ldb)
	{
		return sparse_status_t();
	}

	double IBLAS::Nrm2(long long n, double* x, long long incx)
	{
		throw gcnew System::NotImplementedException();
	}

	double IBLAS::Nrm2(long long n, void* x, long long incx)
	{
		throw gcnew System::NotImplementedException();
	}

	void IBLAS::Rot(long long n, double* x, long long incx, double* y, long long incy, double c, double s)
	{
		throw gcnew System::NotImplementedException();
	}

	void IBLAS::Rot(long long n, void* x, long long incx, void* y, long long incy, double c, double s)
	{
		throw gcnew System::NotImplementedException();
	}

	void IBLAS::Scal(long long n, double alpha, double* x, long long incx)
	{
		throw gcnew System::NotImplementedException();
	}

	void IBLAS::Scal(long long n, void* alpha, void* x, long long incx)
	{
		throw gcnew System::NotImplementedException();
	}

	long long IBLAS::Iamax(long long n, double* x, long long incx)
	{
		throw gcnew System::NotImplementedException();
	}

	long long IBLAS::Iamax(long long n, void* x, long long incx)
	{
		throw gcnew System::NotImplementedException();
	}

	long long IBLAS::Iamin(long long n, double* x, long long incx)
	{
		throw gcnew System::NotImplementedException();
	}

	long long IBLAS::Iamin(long long n, void* x, long long incx)
	{
		throw gcnew System::NotImplementedException();
	}

}