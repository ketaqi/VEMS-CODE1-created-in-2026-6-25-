#include "pch.h"
#include "WMathCore.h"
#include <malloc.h>

namespace WMathCore 
{
	/*SPVectorD::SPVectorD() { }

	SPVectorD::SPVectorD(long long n, long long nonZeroes)
	{
		count = n;
		nnz = nonZeroes;
		idx = (long long*)calloc(nnz, sizeof(long long));
		values = (double*)calloc(nnz, sizeof(double));
	}

	SPVectorD::~SPVectorD()
	{
		delete idx;
		delete values;
	}

	void SPVectorD::SetNonZeroIndices(long long* nzIndices)
	{
		for(long long i = 0; i < count; i++)
			*(idx + i) = *(nzIndices + i);
	}

	void SPVectorD::SetNonZeroValues(double* nzValues)
	{
		for (long long i = 0; i < count; i++)
			*(values + i) = *(nzValues + i);
	}

	void SPVectorD::SetNonZeroValue(long long i, long long nzIndex, double nzValue)
	{
		*(idx + i) = nzIndex;
		*(values + i) = nzValue;
	}


	long long SPVectorD::Count::get()
	{
		return count;
	}
	
	long long SPVectorD::NonZeroCount::get()
	{
		return nnz;
	}


	long long* SPVectorD::Idx::get()
	{
		return idx;
	}

	double* SPVectorD::Values::get()
	{
		return values;
	}*/

}

