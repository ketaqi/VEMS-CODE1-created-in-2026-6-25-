#include "pch.h"
#include "WMathCore.h"
#include "malloc.h"

namespace WMathCore
{
	WVectorZi::WVectorZi() { }

	WVectorZi::WVectorZi(long long n, long long nnz)
	{
		if (n < nnz) { return; }
		this->n = n;
		this->nnz = nnz;
		this->nzIdx = calloc(nnz, sizeof(long long)); 
		this->nzVal = calloc(nnz, sizeof(MKL_Complex16)); 
	}

	WVectorZi::WVectorZi(long long n, long long nnz,
		long long* nzIdx, void* nzVal, int mode)
		: WVectorZi(n, nnz)
	{
		SetNonZeroInfo(nzIdx, nzVal, mode);
	}

	WVectorZi::WVectorZi(long long n, long long nnz, 
		WVectorI^ nzIdx, WVectorZ^ nzVal, int mode)
		: WVectorZi(n, nnz, nzIdx->Values, nzVal->Values, mode)
	{ }

	WVectorZi::WVectorZi(WVectorZi^ source, int mode)
		: WVectorZi(source->Count, source->NonZeroCount)
	{
		SetNonZeroInfo(source->NonZeroIndices, source->NonZeroValues, mode);
	}

	WVectorZi::~WVectorZi()
	{
		delete nzIdx;
		delete nzVal;
	}



	void WVectorZi::SetNonZeroIndices(long long* nzIndices)
	{
		for (long long i = 0; i < NonZeroCount; i++)
			*(NonZeroIndices + i) = *(nzIndices + i);
	}

	void WVectorZi::SetNonZeroValues(void* nzValues, int mode)
	{
		// gets complex pointers
		MKL_Complex16* zSource = (MKL_Complex16*)nzValues;
		MKL_Complex16* zTarget = (MKL_Complex16*)NonZeroValues;
		// mode options
		if (mode == 0)
		{
			// sequential for ...
			for (long long i = 0; i < NonZeroCount; i++)
				*(zTarget + i) = *(zSource + i);
		}
		else
		{
			// BLAS copy by oneMKL
			cblas_zcopy_64(NonZeroCount, zSource, 1, zTarget, 1);
		}
	}

	void WVectorZi::SetNonZeroInfo(long long* nzIndices,
		void* nzValues, int mode)
	{
		// mode options
		if (mode == 0)
		{		
			// gets complex pointers
			MKL_Complex16* zSource = (MKL_Complex16*)nzValues;
			MKL_Complex16* zTarget = (MKL_Complex16*)NonZeroValues;
			// sequential for ...
			for (long long i = 0; i < NonZeroCount; i++)
			{
				*(NonZeroIndices + i) = *(nzIndices + i);
				*(zTarget + i) = *(zSource + i);
			}
		}
		else
		{
			SetNonZeroIndices(nzIndices);
			SetNonZeroValues(nzValues, mode);
		}
	}

	void WVectorZi::SetANonZeroValue(long long i, long long idx, 
		WComplex^ value)
	{
		// sets the index
		*(NonZeroIndices + i) = idx;
		// sets the value
		double* dValues = (double*)NonZeroValues;
		*(dValues + 2 * i + 0) = value->Real; 
		*(dValues + 2 * i + 1) = value->Imag;
	}



	long long WVectorZi::Count::get()
	{
		return n;
	}

	long long WVectorZi::NonZeroCount::get()
	{
		return nnz;
	}

	long long* WVectorZi::NonZeroIndices::get()
	{
		return (long long*)nzIdx; 
	}

	void* WVectorZi::NonZeroValues::get()
	{
		return (void*)nzVal; 
	}

}