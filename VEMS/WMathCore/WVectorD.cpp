#include "pch.h"
#include "WMathCore.h"
#include <malloc.h>


namespace WMathCore
{

	WVectorD::WVectorD(){ }

	WVectorD::WVectorD(long long n)
	{
		count = n;
		values = calloc(n, sizeof(double));
	}

	WVectorD::WVectorD(long long n, double initVal, int mode) 
		: WVectorD(n)
	{
		if (initVal == 0.0) { return; }
		Shift(initVal, mode);	
	}

	WVectorD::WVectorD(WVectorD^ source, int mode) 
		: WVectorD(source->Count)
	{
		CopyFrom(source, mode);
	}

	WVectorD::~WVectorD()
	{
		delete values; // ??
	}


	void WVectorD::CopyFrom(WVectorD^ source, int mode)
	{
		if (mode == 0)
		{
			// sequential for ...
			for (long long i = 0; i < Count; i++)
				*(Values + i) = *(source->Values + i);
		}
		else
		{
			// BLAS copy by oneMKL
			cblas_dcopy_64(Count, source->Values, 1, Values, 1);
		}
	}

	void WVectorD::Shift(double s, int mode)
	{
		if (mode == 0)
		{
			// sequential for ...
			for (long long i = 0; i < Count; i++)
				*(Values + i) += s;
		}
		else
		{
			// VMF shift by oneMKL
			double* dummy;
			vdLinearFrac(Count, Values, dummy, 1.0, s, 0.0, 1.0, Values);
		}
	}

	WVectorD^ WVectorD::GetRange(long long start, long long end, int mode)
	{
		long long n = end - start;
		WVectorD^ y = gcnew WVectorD(n);
		// gets double pointers
		double* dSource = Values + start;
		double* dTarget = y->Values;
		// mode options
		if (mode == 0)
		{
			// sequential for ...
			for (long long i = 0; i < y->Count; i++)
				*(dTarget + i) = *(dSource + i);
		}
		else
		{
			// BLAS copy by oneMKL
			cblas_dcopy_64(y->Count, dSource, 1, dTarget, 1);
		}
		return y;
	}

	void WVectorD::SetRange(long long start, long long end, WVectorD^ val,
		int mode)
	{
		// gets double pointers
		double* dSource = val->Values;
		double* dTarget = Values + start;
		// mode options
		if (mode == 0)
		{
			// sequential for ...
			for (long long i = 0; i < val->Count; i++)
				*(dTarget + i) = *(dSource + i);
		}
		else
		{
			// BLAS copy by oneMKL
			cblas_dcopy_64(val->Count, dSource, 1, dTarget, 1);
		}
	}



	long long WVectorD::Count::get()
	{
		return count;
	}

	void WVectorD::Count::set(long long n)
	{
		count = n;
	}

	double* WVectorD::Values::get()
	{
		return (double*)values;
	}

	double WVectorD::default::get(long long i)
	{
		return *(Values + i); 
	}

	void WVectorD::default::set(long long i, double val)
	{
		*(Values + i) = val;
	}

}