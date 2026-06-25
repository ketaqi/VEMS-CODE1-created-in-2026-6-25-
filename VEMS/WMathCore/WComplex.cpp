#include "pch.h"
#include "WMathCore.h"

namespace WMathCore
{

	WComplex::WComplex()
	{
		this->Real = 0.0;
		this->Imag = 0.0;
	}

	WComplex::WComplex(double re, double im)
	{
		this->Real = re;
		this->Imag = im;
	}

	WComplex::WComplex(const WComplex^ other)
	{
		this->Real = other->real;
		this->Imag = other->imag;
	}


	double WComplex::Real::get()
	{
		return real;
	}

	void WComplex::Real::set(double val)
	{
		real = val;
	}



	double WComplex::Imag::get()
	{
		return imag;
	}

	void WComplex::Imag::set(double val)
	{
		imag = val;
	}

}