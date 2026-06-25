#include "pch.h"
#include "WMathCore.h"
#include "malloc.h"

namespace WMathCore
{

	WVectorDi::WVectorDi() { }

	WVectorDi::WVectorDi(long long n, long long nnz)
	{
		if (n < nnz ) { return; }
		this->n = n;
		this->nnz = nnz;
		this->nzIdx = calloc(nnz, sizeof(long long)); //new vector<long long>(nnz);
		this->nzVal = calloc(nnz, sizeof(double)); //new vector<double>(nnz); 
	}

	WVectorDi::WVectorDi(long long n, long long nnz, 
		long long* nzIdx, double* nzVal, int mode) : WVectorDi(n, nnz)
	{
		SetNonZeroInfo(nzIdx, nzVal, mode);
	}

	WVectorDi::WVectorDi(long long n, long long nnz, 
		WVectorI^ nzIdx, WVectorD^ nzVal, int mode) 
		: WVectorDi(n, nnz, nzIdx->Values, nzVal->Values, mode)
	{ }

	WVectorDi::WVectorDi(WVectorDi^ source, int mode)
		: WVectorDi(source->Count, source->NonZeroCount)
	{
		SetNonZeroInfo(source->NonZeroIndices, source->NonZeroValues, mode);
	}

	WVectorDi::~WVectorDi()
	{
		delete nzIdx;
		delete nzVal;
	}


	void WVectorDi::SetNonZeroIndices(vector<long long>* nzIndices)
	{
		vector<long long>& nzi = *nzIndices;
		nzIdx = new vector<long long>(nzi);
	}

	void WVectorDi::SetNonZeroIndices(long long* nzIndices)
	{
		for (long long i = 0; i < NonZeroCount; i++)
			*(NonZeroIndices + i) = *(nzIndices + i);
	}

	void WVectorDi::SetNonZeroValues(vector<double>* nonZeroValues)
	{
		vector<double>& nzv = *nonZeroValues;
		nzVal = new vector<double>(nzv);
	}

	void WVectorDi::SetNonZeroValues(double* nzValues, int mode)
	{
		// mode options
		if (mode == 0)
		{
			// sequential for ...
			for (long long i = 0; i < NonZeroCount; i++)
				*(NonZeroValues + i) = *(nzValues + i);
		}
		else
		{
			// BLAS copy by oneMKL
			cblas_dcopy_64(NonZeroCount, nzValues, 1, NonZeroValues, 1);
		}
	}

	void WVectorDi::SetNonZeroInfo(long long* nzIndices, 
		double* nzValues, int mode)
	{
		// mode options
		if (mode == 0)
		{
			// sequential for ...
			for (long long i = 0; i < NonZeroCount; i++)
			{
				*(NonZeroIndices + i) = *(nzIndices + i);
				*(NonZeroValues + i) = *(nzValues + i);
			}
		}
		else
		{
			SetNonZeroIndices(nzIndices);
			SetNonZeroValues(nzValues, mode);
		}
	}

	void WVectorDi::SetANonZeroValue(long long i, long long idx, double value)
	{
		*(NonZeroIndices + i) = idx; //nzIdx->at(i) = idx;
		*(NonZeroValues + i) = value; //nzVal->at(i) = val;
	}

	//void WVectorDi::Axpyi(double alpha, double* y)
	//{
	//	cblas_daxpyi_64(this->NonZeroCount, alpha, this->NonZeroValues, this->NonZeroIndices, y);
	//}

	//double WVectorDi::Doti(double* y)
	//{
	//	return cblas_ddoti_64(this->NonZeroCount, this->NonZeroValues, this->NonZeroIndices, y);
	//}

	//void WVectorDi::Gthr(long long n, double* y, long long* indx, WVectorDi^ x)
	//{
	//	// set non-zero values
	//	double* px;
	//	cblas_dgthr_64(n, y, px, indx);
	//	//throw gcnew System::NotImplementedException();
	//}

	//WVectorDi^ WVectorDi::Gthr(long long n, double* y, long long nnz, long long* indx)
	//{
	//	// generate new WVectorDi
	//	WVectorDi^ x = gcnew WVectorDi(n, nnz);
	//	// copy non-zero indices
	//	//for (long long i = 0; i < nnz; i++)
	//	//	x->nzIdx->at(0) = *(indx + i);
	//	// set non-zero values
	//	cblas_dgthr_64(n, y, x->NonZeroValues, indx);
	//	// return
	//	return x;
	//}

	long long WVectorDi::Count::get()
	{
		return n;
	}

	long long WVectorDi::NonZeroCount::get()
	{
		return nnz;
	}


	long long* WVectorDi::NonZeroIndices::get()
	{
		return (long long*)nzIdx; //(long long*)(nzIdx->data());
	}

	double* WVectorDi::NonZeroValues::get()
	{
		return (double*)nzVal; //(double*)(nzVal->data());
	}

}