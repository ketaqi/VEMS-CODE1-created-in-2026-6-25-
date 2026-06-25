#include "pch.h"
#include "WMathCore.h"
#include <malloc.h>

namespace WMathCore
{

	WMatrixD::WMatrixD() { }

	WMatrixD::WMatrixD(long long nRows, long long nCols)
	{
		rows = nRows;
		cols = nCols;
		values = calloc(rows * cols, sizeof(double)); //new vector<double>(rows * cols);
	}

	WMatrixD::WMatrixD(long long nRows, long long nCols, 
		double initVal, int mode) 
		: WMatrixD(nRows, nCols)
	{
		if (initVal == 0.0) { return; }
		Shift(initVal, mode);
		//values = new vector<double>(rows * cols, initVal);
	}

	WMatrixD::WMatrixD(WMatrixD^ source, int mode) 
		: WMatrixD(source->Rows, source->Cols)
	{
		CopyFrom(source, mode);
		//vector<double>& other = *(matrixToCopy.values);
		//values = new vector<double>(other);
	}

	WMatrixD::~WMatrixD()
	{
		delete values;
		//values->~vector();
	}

	void WMatrixD::CopyFrom(WMatrixD^ source, int mode)
	{
		long long n = Rows * Cols;
		if (mode == 0)
		{
			// sequential for ...
			for (long long i = 0; i < n; i++)
				*(Values + i) = *(source->Values + i);
		}
		else
		{
			// BLAS copy by oneMKL
			cblas_dcopy_64(n, source->Values, 1, Values, 1);
		}
	}

	void WMatrixD::Shift(double s, int mode)
	{
		long long n = Rows * Cols;
		if (mode == 0)
		{
			// sequential for ...
			for (long long i = 0; i < n; i++)
				*(Values + i) += s;
		}
		else
		{
			// VMF shift by oneMKL
			double* dummy;
			vdLinearFrac(n, Values, dummy, 1.0, s, 0.0, 1.0, Values);
		}
	}


	long long WMatrixD::Rows::get()
	{
		return rows;
	}

	void WMatrixD::Rows::set(long long nRows)
	{
		rows = nRows;
	}

	long long WMatrixD::Cols::get()
	{
		return cols;
	}

	void WMatrixD::Cols::set(long long nCols)
	{
		rows = nCols;
	}

	double* WMatrixD::Values::get()
	{
		return (double*)values; //(double*)(values->data());
	}

	double WMatrixD::default::get(long long iRow, long long iCol)
	{
		long long idx = iRow * Rows + iCol;
		return *(Values + idx); //values->at(iRow * cols + iCol); 
	}

	void WMatrixD::default::set(long long iRow, long long iCol, double val)
	{
		long long idx = iRow * Rows + iCol;
		*(Values + idx) = val; //values->at(iRow * cols + iCol) = val;
	}
}