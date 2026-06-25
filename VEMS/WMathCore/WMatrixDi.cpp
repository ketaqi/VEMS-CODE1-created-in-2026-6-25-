#include "pch.h"
#include "WMathCore.h"

namespace WMathCore
{
	WMatrixDi::WMatrixDi() 
	{
		status = SPARSE_STATUS_SUCCESS;
	}

	WMatrixDi::WMatrixDi(long long rows, long long cols, long long nnz)
	{
		this->indexing = SPARSE_INDEX_BASE_ZERO;
		this->rows = rows;
		this->cols = cols;
		this->nnz = nnz;

		//rowIdx = new vector<long long>(nnz);
		//colIdx = new vector<long long>(nnz);
		//nzVal = new vector<double>(nnz);
		//handle = new sparse_matrix_t();
		handle = new sparse_matrix_t();
	}

	WMatrixDi::WMatrixDi(WMatrixDi^ source)
	{
		WLinAlg::SparseCopy(source);
	}

	WMatrixDi::~WMatrixDi()
	{
		WLinAlg::SparseDestroy(this);
	}

	void WMatrixDi::CreateCSR(long long* rowPtr, long long* colIdx, double* nzVal)
	{
		/*this->rowPtr = new vector<long long>(rows + 1);
		for (long long i = 0; i < rows + 1; i++)
			this->rowPtr->at(i) = *(rowPtr + i);

		this->colIdx = new vector<long long>(nnz);
		this->nzVal = new vector<double>(nnz);
		for (long long i = 0; i < nnz; i++)
		{
			this->colIdx->at(i) = *(colIdx + i);
			this->nzVal->at(i) = *(nzVal + i);
		}*/

		/*this->status = IBLAS::SparseCreateCSR(handle, SPARSE_INDEX_BASE_ZERO,
			this->Rows, this->Cols,
			rowPtr, rowPtr + 1, colIdx, nzVal);*/

		/*this->status = mkl_sparse_d_create_csr(handle, SPARSE_INDEX_BASE_ZERO,
			this->Rows, this->Cols, 
			rowPtr, rowPtr+1, colIdx, nzVal);*/
	}

	void WMatrixDi::CreateCSC(long long* colPtr, long long* rowIdx, double* nzVal)
	{
		throw gcnew System::NotImplementedException();
	}

	void WMatrixDi::CreateCOO(long long* rowIdx, long long* colIdx, double* nzVal)
	{
		throw gcnew System::NotImplementedException();
	}

	void WMatrixDi::SetNonZeroValue(long long i, long long row, long long col, double value)
	{
		//rowIdx->at(i) = row;
		//colIdx->at(i) = col;
		//nzVal->at(i) = value;
	}

	//void WMatrixDi::Create()
	//{
	//	double tValues[13] = { 1.0, -1.0,     -3.0,
	//						 -2.0,  5.0,
	//									 4.0, 6.0, 4.0,
	//						 -4.0,       2.0, 7.0,
	//								8.0,          -5.0 };
	//	long long tColumns[13] = { 0,      1,        3,
	//						  0,      1,
	//									   2,   3,   4,
	//						  0,           2,   3,
	//								  1,             4 };
	//	long long tRows[13] = { 0,      0,        0,
	//						  1,      1,
	//									   2,   2,   2,
	//						  3,           3,   3,
	//								  4,             4 };
	//	status = mkl_sparse_d_create_coo(handle, SPARSE_INDEX_BASE_ZERO,
	//		5, 5, 13,
	//		this->RowIndices, this->ColIndices, this->NonZeroValues);
	//}

	void WMatrixDi::Create()
	{
		/*status = mkl_sparse_d_create_coo(handle, SPARSE_INDEX_BASE_ZERO,
			this->Rows, this->Cols, this->NonZeroCount,
			this->RowIndices, this->ColIndices, this->NonZeroValues);*/
	}

	void WMatrixDi::MV(WVectorD^ v, WVectorD^ r)
	{
		matrix_descr matDes;
		matDes.type = SPARSE_MATRIX_TYPE_GENERAL;
		matDes.diag = SPARSE_DIAG_NON_UNIT;
		matDes.mode = SPARSE_FILL_MODE_FULL;

		double tmp[5] = { 5.0, 4.0, 3.0, 2.0, 1.0 };
		status = mkl_sparse_d_mv(SPARSE_OPERATION_NON_TRANSPOSE,
			1.0, *handle, matDes, v->Values, 0.0, r->Values);
	}

	void WMatrixDi::Product(double* x, double* y)
	{
		matrix_descr matDes;
		matDes.type = SPARSE_MATRIX_TYPE_GENERAL;
		matDes.diag = SPARSE_DIAG_NON_UNIT;
		matDes.mode = SPARSE_FILL_MODE_FULL;

		status = mkl_sparse_d_mv(SPARSE_OPERATION_NON_TRANSPOSE,
			1.0, *handle, matDes, x, 0.0, y);

		
	}

	//void WMatrixDi::Scatter(long long rowNum, long long colNum, double* nzVal, int status)
	//{
	//	sparse_index_base_t indexing = SPARSE_INDEX_BASE_ZERO;
	//	long long* rowStart;
	//	long long* rowEnd;
	//	long long* colIdx;
	//	sparse_status_t s = WLinAlg::SparseExportCSR(*handle, &indexing, &rowNum, &colNum,
	//		&rowStart, &rowEnd, &colIdx, &nzVal);
	//	status = WLinAlg::SparseStatusConverter(s);
	//}

	//void WMatrixDi::ExportCSR(long long row, long long col,
	//	long long* rowStart, long long* rowEnd, long long* colIdx, double* nzVal)
	//{
	//	sparse_index_base_t idxing = SPARSE_INDEX_BASE_ZERO;
	//	WLinAlg::SparseExportCSR(*handle, &idxing, &row, &col,
	//		&rowStart, &rowEnd, &colIdx, &nzVal);
	//}

	sparse_index_base_t WMatrixDi::Indexing::get()
	{
		return this->indexing;
	}

	void WMatrixDi::Indexing::set(sparse_index_base_t value)
	{
		this->indexing = value;
	}

	sparse_matrix_t* WMatrixDi::Handle::get()
	{
		return this->handle;
	}

	void WMatrixDi::Handle::set(sparse_matrix_t* value)
	{
		this->handle = value;
	}

	sparse_status_t WMatrixDi::Status::get()
	{
		return this->status;
	}

	void WMatrixDi::Status::set(sparse_status_t value)
	{
		this->status = value;
	}

	long long WMatrixDi::Rows::get()
	{
		return rows;
	}

	long long WMatrixDi::Cols::get()
	{
		return cols;
	}

	long long WMatrixDi::NonZeroCount::get()
	{
		return nnz;
	}

	/*long long* WMatrixDi::RowIndices::get()
	{
		return (long long*)(rowIdx->data());
	}

	long long* WMatrixDi::ColIndices::get()
	{
		return (long long*)(colIdx->data());
	}

	double* WMatrixDi::NonZeroValues::get()
	{
		return (double*)(nzVal->data());
	}*/

}