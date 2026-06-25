#include "pch.h"
#include "WMathCore.h"

namespace WMathCore
{

    IntelMKL::IntelMKL() { }

    double IntelMKL::Asum(long long n, double* x, long long incx)
    {
        return cblas_dasum_64(n, x, incx);
    }

    double IntelMKL::Asum(long long nz, void* x, long long incx)
    {
        return cblas_dzasum_64(nz, x, incx);
    }

    void IntelMKL::Copy(long long n, double* x, long long incx, double* y, long long incy)
    {
        cblas_dcopy_64(n, x, incx, y, incy);
    }

    void IntelMKL::Copy(long long n, void* x, long long incx, void* y, long long incy)
    {
        cblas_zcopy_64(n, x, incx, y, incy);
    }

    void IntelMKL::Axpy(long long n,
        double alpha, double* x, long long incx,
        double* y, long long incy) 
    {
        cblas_daxpy_64(n, alpha, x, incx, y, incy);
    }

    void IntelMKL::Axpy(long long n, void* alpha, void* x, long long incx, void* y, long long incy)
    {
        cblas_zaxpy_64(n, alpha, x, incx, y, incy);
    }

    void IntelMKL::Axpyi(long long nz, double alpha,
        double* x, long long* indx, double* y)
    {
        cblas_daxpyi_64(nz, alpha, x, indx, y);
    }

    void IntelMKL::Axpyi(long long nz, void* alpha, void* x, long long* indx, void* y)
    {
        cblas_zaxpyi_64(nz, alpha, x, indx, y);
    }

    double IntelMKL::Dot(long long n, double* x, long long incx, double* y, long long incy)
    {
        return cblas_ddot_64(n, x, incx, y, incy);
    }

    void IntelMKL::Dotc(long long n, void* x, long long incx, void* y, long long incy, void* dotc)
    {
        cblas_zdotc_sub_64(n, x, incx, y, incy, dotc);
    }

    void IntelMKL::Dotu(long long n, void* x, long long incx, void* y, long long incy, void* dotu)
    {
        cblas_zdotu_sub_64(n, x, incx, y, incy, dotu);
    }

    double IntelMKL::Doti(long long nz, double* x, long long* indx, double* y)
    {
        return cblas_ddoti_64(nz, x, indx, y);
    }

    void IntelMKL::Dotci(long long nz, void* x, long long* indx, void* y, void* dotc)
    {
        cblas_zdotci_sub_64(nz, x, indx, y, dotc);
    }

    void IntelMKL::Dotui(long long nz, void* x, long long* indx, void* y, void* dotu)
    {
        cblas_zdotui_sub_64(nz, x, indx, y, dotu);
    }

    double IntelMKL::Nrm2(long long n, double* x, long long incx)
    {
        return cblas_dnrm2_64(n, x, incx);
    }

    double IntelMKL::Nrm2(long long n, void* x, long long incx)
    {
        return cblas_dznrm2_64(n, x, incx);
    }

    void IntelMKL::Scal(long long n, double alpha, double* x, long long incx)
    {
        cblas_dscal_64(n, alpha, x, incx);
    }

    void IntelMKL::Scal(long long n, void* alpha, void* x, long long incx)
    {
        cblas_zscal_64(n, alpha, x, incx);
    }

    void IntelMKL::Scal(long long n, double alpha, void* x, long long incx)
    {
        cblas_zdscal_64(n, alpha, x, incx);
    }

    void IntelMKL::Swap(long long n, double* x, long long incx, double* y, long long incy)
    {
        cblas_dswap_64(n, x, incx, y, incy);
    }

    void IntelMKL::Swap(long long n, void* x, long long incx, void* y, long long incy)
    {
        cblas_zswap_64(n, x, incx, y, incy);
    }

    long long IntelMKL::Iamax(long long n, double* x, long long incx)
    {
        return cblas_idamax_64(n, x, incx);
    }

    long long IntelMKL::Iamax(long long n, void* x, long long incx)
    {
        return cblas_izamax_64(n, x, incx);
    }

    long long IntelMKL::Iamin(long long n, double* x, long long incx)
    {
        return cblas_idamin_64(n, x, incx);
    }

    long long IntelMKL::Iamin(long long n, void* x, long long incx)
    {
        return cblas_izamin_64(n, x, incx);
    }

    void IntelMKL::Rot(long long n, double* x, long long incx, double* y, long long incy, double c, double s)
    {
        cblas_drot_64(n, x, incx, y, incy, c, s);
    }

    void IntelMKL::Rot(long long n, void* x, long long incx, void* y, long long incy, double c, void* s)
    {
        cblas_zrot_64(n, x, incx, y, incy, c, s);
    }

    void IntelMKL::Rot(long long n, void* x, long long incx, void* y, long long incy, double c, double s)
    {
        cblas_zdrot_64(n, x, incx, y, incy, c, s);
    }

    double IntelMKL::Cabs(void* z)
    {
        return cblas_dcabs1(z);
    }

    void IntelMKL::Gemv(CBLAS_LAYOUT layout, CBLAS_TRANSPOSE operation, long long m, long long n, double alpha, double* a, long long lda, double* x, long long incx, double beta, double* y, long long incy)
    {
        cblas_dgemv_64(layout, operation, m, n, alpha, a, lda,
            x, incx, beta, y, incy);
    }

    void IntelMKL::Gemv(CBLAS_LAYOUT layout, CBLAS_TRANSPOSE operation, long long m, long long n, MKL_Complex16 alpha, MKL_Complex16* a, long long lda, MKL_Complex16* x, long long incx, MKL_Complex16 beta, MKL_Complex16* y, long long incy)
    {
        cblas_zgemv_64(layout, operation, m, n, &alpha, a, lda,
            x, incx, &beta, y, incy);
    }

    void IntelMKL::Gemm(CBLAS_LAYOUT layout, CBLAS_TRANSPOSE operationa, CBLAS_TRANSPOSE operationb, long long m, long long n, long long k, double alpha, double* a, long long lda, double* b, long long ldb, double beta, double* c, long long ldc)
    {
        cblas_dgemm_64(layout, operationa, operationb,
            m, n, k, alpha, a, lda, b, ldb, beta, c, ldc);
    }

    void IntelMKL::Gemm(CBLAS_LAYOUT layout, CBLAS_TRANSPOSE operationa, CBLAS_TRANSPOSE operationb, long long m, long long n, long long k, MKL_Complex16 alpha, MKL_Complex16* a, long long lda, MKL_Complex16* b, long long ldb, MKL_Complex16 beta, MKL_Complex16* c, long long ldc)
    {
        cblas_zgemm_64(layout, operationa, operationb,
            m, n, k, &alpha, a, lda, b, ldb, &beta, c, ldc);
    }

    void IntelMKL::Gthr(long long nz, double* y, double* x, long long* indx)
    {
        cblas_dgthr_64(nz, y, x, indx);
    }

    void IntelMKL::Gthr(long long nz, void* y, void* x, long long* indx)
    {
        cblas_zgthr_64(nz, y, x, indx);
    }

    void IntelMKL::Gthrz(long long nz, double* y, double* x, long long* indx)
    {
        cblas_dgthrz_64(nz, y, x, indx);
    }

    void IntelMKL::Gthrz(long long nz, void* y, void* x, long long* indx)
    {
        cblas_zgthrz_64(nz, y, x, indx);
    }

    void IntelMKL::Roti(long long nz, double* x, long long* indx, double* y, double c, double s)
    {
        cblas_droti_64(nz, x, indx, y, c, s);
    }

    void IntelMKL::Sctr(long long nz, double* x, long long* indx, double* y)
    {
        cblas_dsctr_64(nz, x, indx, y);
    }

    void IntelMKL::Sctr(long long nz, void* x, long long* indx, void* y)
    {
        cblas_zsctr_64(nz, x, indx, y);
    }

    sparse_status_t IntelMKL::SparseCreateCSR(sparse_matrix_t* handle, sparse_index_base_t indexing, long long rows, long long cols, long long* rows_start, long long* rows_end, long long* col_indx, double* values)
    {
        return mkl_sparse_d_create_csr(handle, indexing, rows, cols,
            rows_start, rows_end, col_indx, values);
    }

    sparse_status_t IntelMKL::SparseCreateCSR(sparse_matrix_t* handle, sparse_index_base_t indexing, long long rows, long long cols, long long* rows_start, long long* rows_end, long long* col_indx, void* values)
    {
        return mkl_sparse_z_create_csr(handle, indexing, rows, cols,
            rows_start, rows_end, col_indx, (MKL_Complex16*)values);
    }

    sparse_status_t IntelMKL::SparseCreateCSC(sparse_matrix_t* handle, sparse_index_base_t indexing, long long rows, long long cols, long long* cols_start, long long* cols_end, long long* row_indx, double* values)
    {
        return mkl_sparse_d_create_csc(handle, indexing, rows, cols,
            cols_start, cols_end, row_indx, values);
    }

    sparse_status_t IntelMKL::SparseCreateCSC(sparse_matrix_t* handle, sparse_index_base_t indexing, long long rows, long long cols, long long* cols_start, long long* cols_end, long long* row_indx, void* values)
    {
        return mkl_sparse_z_create_csc(handle, indexing, rows, cols,
            cols_start, cols_end, row_indx, (MKL_Complex16*)values);
    }

    sparse_status_t IntelMKL::SparseCreateCOO(sparse_matrix_t* handle, sparse_index_base_t indexing, long long rows, long long cols, long long nnz, long long* row_indx, long long* col_indx, double* values)
    {
        return mkl_sparse_d_create_coo(handle, indexing, rows, cols, nnz,
            row_indx, col_indx, values);
    }

    sparse_status_t IntelMKL::SparseCreateCOO(sparse_matrix_t* handle, sparse_index_base_t indexing, long long rows, long long cols, long long nnz, long long* row_indx, long long* col_indx, void* values)
    {
        return mkl_sparse_z_create_coo(handle, indexing, rows, cols, nnz,
            row_indx, col_indx, (MKL_Complex16*)values);
    }

    sparse_status_t IntelMKL::SparseCopy(sparse_matrix_t source, matrix_descr descr, sparse_matrix_t* dest)
    {
        return mkl_sparse_copy(source, descr, dest);
    }

    sparse_status_t IntelMKL::SparseDestroy(sparse_matrix_t a)
    {
        return mkl_sparse_destroy(a);
    }

    sparse_status_t IntelMKL::SparseConvert2CSR(sparse_matrix_t source, sparse_operation_t operation, sparse_matrix_t* dest)
    {
        return mkl_sparse_convert_csr(source, operation, dest);
    }

    sparse_status_t IntelMKL::SparseExportCSR(sparse_matrix_t source, sparse_index_base_t* indexing, long long* rows, long long* cols, long long** rows_start, long long** rows_end, long long** col_indx, double** values)
    {
        return mkl_sparse_d_export_csr(source, indexing, rows, cols, 
            rows_start, rows_end, col_indx, values);
    }

    sparse_status_t IntelMKL::SparseExportCSC(sparse_matrix_t source, sparse_index_base_t* indexing, long long* rows, long long* cols, long long** cols_start, long long** cols_end, long long** row_indx, double** values)
    {
        return mkl_sparse_d_export_csc(source, indexing, rows, cols, 
            cols_start, cols_end, row_indx, values);
    }

    sparse_status_t IntelMKL::SparseSetValue(sparse_matrix_t a, long long row, long long col, double value)
    {
        return mkl_sparse_d_set_value(a, row, col, value);
    }

    sparse_status_t IntelMKL::SparseSetValue(sparse_matrix_t a, long long row, long long col, WComplex^ value)
    {
        MKL_Complex16 val = {value->Real, value->Imag};
        return mkl_sparse_z_set_value(a, row, col, val);
    }

    sparse_status_t IntelMKL::SparseUpdateValues(sparse_matrix_t a, long long nvalues, long long* row_indx, long long* col_indx, double* values)
    {
        return mkl_sparse_d_update_values(a, nvalues, row_indx, col_indx, values);
    }

    sparse_status_t IntelMKL::SparseUpdateValues(sparse_matrix_t a, long long nvalues, long long* row_indx, long long* col_indx, void* values)
    {
        return mkl_sparse_z_update_values(a, nvalues, row_indx, col_indx, (MKL_Complex16*)values);
    }

    sparse_status_t IntelMKL::SparseSetMVHint(sparse_matrix_t a, sparse_operation_t operation, matrix_descr descr, long long expected_calls)
    {
        return mkl_sparse_set_mv_hint(a, operation, descr, expected_calls);
    }

    sparse_status_t IntelMKL::SparseSetSVHint(sparse_matrix_t a, sparse_operation_t operation, matrix_descr descr, long long expected_calls)
    {
        return mkl_sparse_set_sv_hint(a, operation, descr, expected_calls);
    }

    sparse_status_t IntelMKL::SparseSetMMHint(sparse_matrix_t a, sparse_operation_t operation, matrix_descr descr, sparse_layout_t layout, long long dense_matrix_size, long long expected_calls)
    {
        return mkl_sparse_set_mm_hint(a, operation, descr, layout, dense_matrix_size, expected_calls);
    }

    sparse_status_t IntelMKL::SparseSetSMHint(sparse_matrix_t a, sparse_operation_t operation, matrix_descr descr, sparse_layout_t layout, long long dense_matrix_size, long long expected_calls)
    {
        return mkl_sparse_set_mm_hint(a, operation, descr, layout, dense_matrix_size, expected_calls);
    }

    sparse_status_t IntelMKL::SparseSetMemoryHint(sparse_matrix_t a, sparse_memory_usage_t policy)
    {
        return mkl_sparse_set_memory_hint(a, policy);
    }

    sparse_status_t IntelMKL::SparseOptimize(sparse_matrix_t a)
    {
        return mkl_sparse_optimize(a);
    }

    sparse_status_t IntelMKL::SparseMV(sparse_operation_t operation, double alpha, sparse_matrix_t a, matrix_descr descr, double* x, double beta, double* y)
    {
        return mkl_sparse_d_mv(operation, alpha, a, descr, x, beta, y);
    }

    sparse_status_t IntelMKL::SparseTRSV(sparse_operation_t operation, double alpha, sparse_matrix_t a, matrix_descr descr, double* x, double* y)
    {
        return mkl_sparse_d_trsv(operation, alpha, a, descr, x, y);
    }

    sparse_status_t IntelMKL::SparseMM(sparse_operation_t operation, double alpha, sparse_matrix_t a, matrix_descr descr, sparse_layout_t layout, double* b, long long columns, long long ldb, double beta, double* c, long long ldc)
    {
        return mkl_sparse_d_mm(operation, alpha, a, descr, layout, b, columns, ldb, beta, c, ldc);
    }

    sparse_status_t IntelMKL::SparseTRSM(sparse_operation_t operation, double alpha, sparse_matrix_t a, matrix_descr descr, const sparse_layout_t layout, double* x, long long columns, long long ldx, double* y, long long ldy)
    {
        return mkl_sparse_d_trsm(operation, alpha, a, descr, layout, x, columns, ldx, y, ldy);
    }

    sparse_status_t IntelMKL::SparseAdd(sparse_operation_t operation, sparse_matrix_t a, double alpha, sparse_matrix_t b, sparse_matrix_t* c)
    {
        return mkl_sparse_d_add(operation, a, alpha, b, c);
    }

    sparse_status_t IntelMKL::SparseSPMM(sparse_operation_t operation, sparse_matrix_t a, sparse_matrix_t b, sparse_matrix_t* c)
    {
        return mkl_sparse_spmm(operation, a, b, c);
    }

    sparse_status_t IntelMKL::SparseSPMMD(sparse_operation_t operation, sparse_matrix_t a, sparse_matrix_t b, sparse_layout_t layout, double* c, long long ldc)
    {
        return mkl_sparse_d_spmmd(operation, a, b, layout, c, ldc);
    }

    sparse_status_t IntelMKL::SparseSetQRHint(sparse_matrix_t a, sparse_qr_hint_t hint)
    {
        return mkl_sparse_set_qr_hint(a, hint);
    }

    sparse_status_t IntelMKL::SparseQR(sparse_operation_t operation, sparse_matrix_t a, matrix_descr descr, sparse_layout_t layout, long long columns, double* x, long long ldx, double* b, long long ldb)
    {
        return mkl_sparse_d_qr(operation, a, descr, layout, columns, x, ldx, b, ldb);
    }

    sparse_status_t IntelMKL::SparseQRReorder(sparse_matrix_t a, matrix_descr descr)
    {
        return mkl_sparse_qr_reorder(a, descr);
    }

    sparse_status_t IntelMKL::SparseQRFactorize(sparse_matrix_t a, double* alt_values)
    {
        return mkl_sparse_d_qr_factorize(a, alt_values);
    }

    sparse_status_t IntelMKL::SparseQRSolve(sparse_operation_t operation, sparse_matrix_t a, double* alt_values, sparse_layout_t layout, long long columns, double* x, long long ldx, const double* b, long long ldb)
    {
        return mkl_sparse_d_qr_solve(operation, a, alt_values, layout, columns, x, ldx, b, ldb);
    }

    sparse_status_t IntelMKL::SparseQRQmult(sparse_operation_t operation, sparse_matrix_t a, sparse_layout_t layout, long long columns, double* x, long long ldx, const double* b, long long ldb)
    {
        return mkl_sparse_d_qr_qmult(operation, a, layout, columns, x, ldx, b, ldb);
    }

    sparse_status_t IntelMKL::SparseQRRsolve(sparse_operation_t operation, sparse_matrix_t a, sparse_layout_t layout, long long columns, double* x, long long ldx, double* b, long long ldb)
    {
        return mkl_sparse_d_qr_rsolve(operation, a, layout, columns, x, ldx, b, ldb);
    }




}

