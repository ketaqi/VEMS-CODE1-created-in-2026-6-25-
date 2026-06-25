#include "pch.h"
#include "WMathCore.h"

namespace WMathCore 
{

	WVectorZ::WVectorZ() { }

	WVectorZ::WVectorZ(long long n)
	{
		count = n;
		values = calloc(n, sizeof(MKL_Complex16));
	}

	WVectorZ::WVectorZ(long long n, WComplex^ initVal, int mode) 
		: WVectorZ(n)
	{
		if (initVal->Real == 0.0 && initVal->Imag == 0.0) { return; }
		Shift(initVal, mode);
	}

	WVectorZ::WVectorZ(WVectorZ^ source, int mode) 
		: WVectorZ(source->Count)
	{
		CopyFrom(source, mode);
	}

	WVectorZ::WVectorZ(WVectorD^ part, bool isRealPart, int mode)
		: WVectorZ(part->Count)
	{
		FillPart(part, isRealPart, mode);
	}


	WVectorZ::~WVectorZ()
	{
		delete values;
	}


	void WVectorZ::CopyFrom(WVectorZ^ source, int mode)
	{
		// gets complex pointers
		MKL_Complex16* zSource = (MKL_Complex16*)source->Values;
		MKL_Complex16* zTarget = (MKL_Complex16*)Values;
		// mode option
		if (mode == 0)
		{
			// sequential for ...
			for (long long i = 0; i < Count; i++)
				*(zTarget + i) = *(zSource + i);
		}
		else
		{
			// BLAS copy by oneMKL
			cblas_zcopy_64(Count, zSource, 1, zTarget, 1);
		}
	}

	void WVectorZ::Shift(WComplex^ s, int mode)
	{
		// gets double pointer
		double* dValues = (double*)Values;
		// mode option
		if (mode == 0)
		{
			// sequential for ...
			for (long long i = 0; i < Count; i++)
			{
				*(dValues + 2 * i + 0) += s->Real;
				*(dValues + 2 * i + 1) += s->Imag;
			}
		}
		else
		{
			// VMF shift by oneMKL
			double* dummy;
			vdLinearFracI(Count, dValues + 0, 2, dummy + 0, 2, 1.0, s->Real, 0.0, 1.0, dValues + 0, 2);
			vdLinearFracI(Count, dValues + 1, 2, dummy + 1, 2, 1.0, s->Imag, 0.0, 1.0, dValues + 1, 2);
		}
	}

	void WVectorZ::FillPart(WVectorD^ part, bool isRealPart, int mode)
	{
		// gets double pointers
		double* dTarget = (double*)Values;
		double* dSource = part->Values;
		// mode option
		if (mode == 0)
		{
			// sequential for ...
			for (long long i = 0; i < Count; i++)
			{
				if (isRealPart == true)
					*(dTarget + 2 * i + 0) = *(dSource + i);
				else
					*(dTarget + 2 * i + 1) = *(dSource + i);
			}
		}
		else
		{
			// BLAS copy by oneMKL
			if (isRealPart == true)
				cblas_dcopy_64(part->Count, dSource, 1, dTarget + 0, 2);
			else
				cblas_dcopy_64(part->Count, dSource, 1, dTarget + 1, 2);
		}
	}

	WVectorZ^ WVectorZ::GetRange(long long start, long long end, int mode)
	{
		long long n = end - start;
		WVectorZ^ y = gcnew WVectorZ(n);
		// gets complex pointers
		MKL_Complex16* zSource = (MKL_Complex16*)Values + start;
		MKL_Complex16* zTarget = (MKL_Complex16*)y->Values;
		// mode options
		if (mode == 0)
		{
			// sequential for ...
			for (long long i = 0; i < y->Count; i++)
				*(zTarget + i) = *(zSource + i);
		}
		else
		{
			// BLAS copy by oneMKL
			cblas_zcopy_64(y->Count, zSource, 1, zTarget, 1);
		}
		return y;
	}

	void WVectorZ::SetRange(long long start, long long end, WVectorZ^ val,
		int mode)
	{
		// gets complex pointers
		MKL_Complex16* zSource = (MKL_Complex16*)val->Values;
		MKL_Complex16* zTarget = (MKL_Complex16*)Values + start;
		// mode options
		if (mode == 0)
		{
			// sequential 
			for (long long i = 0; i < val->Count; i++)
				*(zTarget + i) = *(zSource + i);
		}
		else
		{
			// BLAS copy by oneMKL
			cblas_zcopy_64(val->Count, zSource, 1, zTarget, 1);
		}
	}


	long long WVectorZ::Count::get()
	{
		return count;
	}

	void WVectorZ::Count::set(long long n)
	{
		count = n;
	}

	void* WVectorZ::Values::get()
	{
		return values;
	}

	WComplex^ WVectorZ::default::get(long long i)
	{
		double* dValues = (double*)Values;
		WComplex^ val = gcnew WComplex(*(dValues + 2 * i + 0), *(dValues + 2 * i + 1));
		return val;
	}

	void WVectorZ::default::set(long long i, WComplex^ val)
	{
		double* dValues = (double*)values;
		*(dValues + 2 * i + 0) = val->Real;
		*(dValues + 2 * i + 1) = val->Imag;
	}

}