#include "pch.h"
#include "WMathCore.h"

namespace WMathCore
{

	WMatrixZi::WMatrixZi() 
	{
		status = SPARSE_STATUS_SUCCESS;
	}

	WMatrixZi::WMatrixZi(long long rows, long long cols, long long nnz)
	{
		this->indexing = SPARSE_INDEX_BASE_ZERO;
		this->rows = rows;
		this->cols = cols;
		this->nnz = nnz;
		handle = new sparse_matrix_t();
	}

	WMatrixZi::WMatrixZi(WMatrixZi^ source)
	{
		WLinAlg::SparseCopy(source);
	}

	WMatrixZi::~WMatrixZi()
	{
		//WLinAlg::SparseDestroy()
		throw gcnew System::NotImplementedException();
	}

	sparse_index_base_t WMatrixZi::Indexing::get()
	{
		return this->indexing;
	}

	void WMatrixZi::Indexing::set(sparse_index_base_t value)
	{
		this->indexing = value;
	}

	sparse_matrix_t* WMatrixZi::Handle::get()
	{
		return this->handle;
	}

	void WMatrixZi::Handle::set(sparse_matrix_t* value)
	{
		this->handle = value;
	}

	sparse_status_t WMatrixZi::Status::get()
	{
		return this->status;
	}

	void WMatrixZi::Status::set(sparse_status_t value)
	{
		this->status = value;
	}

	long long WMatrixZi::Rows::get()
	{
		return rows;
	}

	long long WMatrixZi::Cols::get()
	{
		return cols;
	}

	long long WMatrixZi::NonZeroCount::get()
	{
		return nnz;
	}
}