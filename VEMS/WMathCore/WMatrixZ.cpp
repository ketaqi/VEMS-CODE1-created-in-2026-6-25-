#include "pch.h"
#include "WMathCore.h"

namespace WMathCore
{

	WMatrixZ::WMatrixZ() { }

	WMatrixZ::WMatrixZ(long long nRows, long long nCols)
	{
		rows = nRows;
		cols = nCols;
		values = calloc(rows * cols, sizeof(MKL_Complex16));
	}

	WMatrixZ::WMatrixZ(long long nRows, long long nCols, WComplex^ initVal, 
		int mode) : WMatrixZ(nRows, nCols)
	{
		Shift(initVal, mode);
	}

	WMatrixZ::WMatrixZ(WMatrixD^ part, bool isRealPart, 
		int mode) : WMatrixZ(part->Rows, part->Cols)
	{
		FillPart(part, isRealPart, mode);
	}

	WMatrixZ::WMatrixZ(WMatrixZ^ source, int mode)
		: WMatrixZ(source->Rows, source->Cols)
	{
		CopyFrom(source, mode);
	}

	WMatrixZ::~WMatrixZ()
	{
		delete values;
	}


	void WMatrixZ::CopyFrom(WMatrixZ^ source, int mode)
	{
		long long n = Rows * Cols;
		// gets complex pointers
		MKL_Complex16* zSource = (MKL_Complex16*)source->Values;
		MKL_Complex16* zTarget = (MKL_Complex16*)Values;
		// mode option
		if (mode == 0)
		{
			// sequential for ...
			for (long long i = 0; i < n; i++)
				*(zTarget + i) = *(zSource + i);
		}
		else
		{
			// BLAS copy by oneMKL
			cblas_zcopy_64(n, zSource, 1, zTarget, 1);
		}
	}

	void WMatrixZ::Shift(WComplex^ s, int mode)
	{
		long long n = Rows * Cols;
		// gets double pointer
		double* dValues = (double*)Values;
		// mode option
		if (mode == 0)
		{
			// sequential for ...
			for (long long i = 0; i < n; i++)
			{
				*(dValues + 2 * i + 0) += s->Real;
				*(dValues + 2 * i + 1) += s->Imag;
			}
		}
		else
		{
			// VMF shift by oneMKL
			double* dummy;
			vdLinearFracI(n, dValues + 0, 2, dummy + 0, 2, 1.0, s->Real, 0.0, 1.0, dValues + 0, 2);
			vdLinearFracI(n, dValues + 1, 2, dummy + 1, 2, 1.0, s->Imag, 0.0, 1.0, dValues + 1, 2);
		}
	}

	void WMatrixZ::FillPart(WMatrixD^ part, bool isRealPart, int mode)
	{
		// gets double pointers
		double* dTarget = (double*)Values;
		double* dSource = part->Values;
		// mode options
		long long n = Rows * Cols;
		if (mode == 0)
		{
			// sequential for ...
			for (long long i = 0; i < n; i++)
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
				cblas_dcopy_64(n, dSource, 1, dTarget + 0, 2);
			else
				cblas_dcopy_64(n, dSource, 1, dTarget + 1, 2);
		}
	}


	long long WMatrixZ::Rows::get()
	{
		return rows;
	}

	void WMatrixZ::Rows::set(long long nRows)
	{
		rows = nRows;
	}

	long long WMatrixZ::Cols::get()
	{
		return cols;
	}

	void WMatrixZ::Cols::set(long long nCols)
	{
		rows = nCols;
	}

	void* WMatrixZ::Values::get()
	{
		return values; 
	}

	WComplex^ WMatrixZ::default::get(long long iRow, long long iCol)
	{
		long long idx = iRow * Rows + iCol;
		double* dValues = (double*)Values;
		WComplex^ val = gcnew WComplex(*(dValues + 2 * idx + 0), *(dValues + 2 * idx + 1));
		return val;
	}

	void WMatrixZ::default::set(long long iRow, long long iCol, 
		WComplex^ val)
	{
		long long idx = iRow * Rows + iCol;
		double* dValues = (double*)Values;
		*(dValues + 2 * idx + 0) = val->Real;
		*(dValues + 2 * idx + 1) = val->Imag;
	}


}