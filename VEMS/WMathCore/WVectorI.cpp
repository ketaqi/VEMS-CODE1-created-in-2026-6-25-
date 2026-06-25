#include "pch.h"
#include "WMathCore.h"

namespace WMathCore
{


	WVectorI::WVectorI() { }

	WVectorI::WVectorI(long long n)
	{
		count = n;
		values = calloc(n, sizeof(long long));
	}

	WVectorI::WVectorI(long long n, long long initVal)
		: WVectorI(n)
	{
		Shift(initVal);
	}

	WVectorI::WVectorI(WVectorI^ source)
	{
		CopyFrom(source);
	}

	WVectorI::~WVectorI()
	{
		delete values;
	}

	void WVectorI::CopyFrom(WVectorI^ source)
	{
		for (long long i = 0; i < Count; i++)
			*(Values + i) = *(source->Values + i);
	}

	void WVectorI::Shift(long long s)
	{
		for (long long i = 0; i < Count; i++)
			*(Values + 1) += s;
	}

	WVectorI^ WVectorI::GetRange(long long start, long long end)
	{
		long long n = end - start;
		WVectorI^ y = gcnew WVectorI(n);
		// gets pointers
		long long* iSource = Values + start;
		long long* iTarget = y->Values;
		// loop
		for (long long i = 0; i < y->Count; i++)
			*(iTarget + i) = *(iSource + i);
		return y;
	}

	void WVectorI::SetRange(long long start, long long end, WVectorI^ val)
	{
		// gets pointers
		long long* iSource = val->Values;
		long long* iTarget = Values + start;
		// loop
		for(long long i = 0; i < val->Count; i++)
			*(iTarget + i) = *(iSource + i);
	}



	long long WVectorI::Count::get()
	{
		return count;
	}

	void WVectorI::Count::set(long long n)
	{
		count = n; 
	}

	long long* WVectorI::Values::get()
	{
		return (long long*)values;
	}

	long long WVectorI::default::get(long long i)
	{
		return *(Values + i);
	}

	void WVectorI::default::set(long long i, long long val)
	{
		*(Values + i) = val;
	}

}