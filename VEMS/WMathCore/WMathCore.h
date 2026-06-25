#pragma once
#include <vector>
#include "mkl.h"
//#include <ppl.h>

using namespace System;
using namespace std;
//using namespace concurrency;

namespace WMathCore 
{

	/// <summary>
	/// double-precision complex-number structure
	/// </summary>
	public ref struct WComplex
	{
	private:

		double real;
		double imag;

	public:

		/// <summary>
		/// default complex number constructor
		/// real- and imag-parts both set to 0.0
		/// </summary>
		WComplex();

		/// <summary>
		/// constructs a complex number
		/// </summary>
		/// <param name="re"> real-part </param>
		/// <param name="im"> imaginary-part </param>
		WComplex(double re, double im);

		/// <summary>
		/// constructs a complex number by copying from another
		/// </summary>
		/// <param name="other"> another complex number </param>
		WComplex(const WComplex ^other);

		/// <summary>
		/// real-part of the complex number
		/// </summary>
		property double Real
		{
			/// <summary>
			/// gets the real-part of the complex number
			/// </summary>
			/// <returns> real-part </returns>
			double get();

			/// <summary>
			/// sets the real-part of the complex number
			/// </summary>
			/// <param name="val"> desired value </param>
			void set(double val);
		}

		/// <summary>
		/// imaginary-part of the complex number
		/// </summary>
		property double Imag
		{
			/// <summary>
			/// gets the imaginary-part of the complex number
			/// </summary>
			/// <returns> imaginary-part </returns>
			double get();

			/// <summary>
			/// sets the imag-part of the complex number
			/// </summary>
			/// <param name="val"> desired value </param>
			void set(double val);
		}

	};


	/// <summary>
	/// integer-valued vector class
	/// </summary>
	public ref class WVectorI
	{
	private:

		/// <summary>
		/// number of vector elements
		/// </summary>
		long long count;

		/// <summary>
		/// pointer to the values of vector elements
		/// </summary>
		void* values;
		
	public:

		#pragma region constructors

		/// <summary>
		/// default constructor
		/// </summary>
		WVectorI();

		/// <summary>
		/// constructs a vector with given length
		/// and initialize all elements to zero
		/// </summary>
		/// <param name="n"> total number of elements </param>
		WVectorI(long long n);

		/// <summary>
		/// constructs a vector with given length
		/// and sets all element to a specific value 
		/// </summary>
		/// <param name="n"> total number of elements </param>
		/// <param name="initVal"> initial value of all the elements </param>
		WVectorI(long long n, long long initVal);

		/// <summary>
		/// copy constructor (deep copy)
		/// </summary>
		/// <param name="other"> another vector as the source </param>
		WVectorI(WVectorI^ source);

		/// <summary>
		/// destructor
		/// </summary>
		~WVectorI();

		#pragma endregion
		#pragma region properties

		/// <summary>
		/// total number of elements
		/// </summary>
		property long long Count
		{
			/// <summary>
			/// gets the total number of elements
			/// </summary>
			/// <returns> total number of elements </returns>
			long long get();

			/// <summary>
			/// sets the total number of elements
			/// </summary>
			/// <param name="n"> desired value </param>
			void set(long long n);
		}

		/// <summary>
		/// vector elements values
		/// </summary>
		property long long* Values
		{
			/// <summary>
			/// gets the vector element values
			/// </summary>
			/// <returns> vector element values </returns>
			long long* get();
		}

		/// <summary>
		/// gets / sets element value at given index 
		/// </summary>
		property long long default[long long]
		{
			/// <summary>
			/// gets the element value at index i
			/// </summary>
			/// <param name="i"> index i </param>
			/// <returns> element value </returns>
			long long get(long long i);

			/// <summary>
			/// sets the element value at index i
			/// </summary>
			/// <param name="i"> index i </param>
			/// <param name="val"> desired value </param>
			void set(long long i, long long val);
		}

		#pragma endregion
		#pragma region methods

		/// <summary>
		/// copies values from another vector
		/// </summary>
		/// <param name="source"> another vector as the source </param>
		void CopyFrom(WVectorI^ source);

		/// <summary>
		/// shifts the values by a constant number
		/// </summary>
		/// <param name="s"> shift value </param>
		/// <returns> a new vector after constant shift </returns>
		void Shift(long long s);

		/// <summary>
		/// gets values within a selected range
		/// </summary>
		/// <param name="start"> inclusive start index </param>
		/// <param name="end"> exclusive end index </param>
		/// <returns> values within the selected range </returns>
		WVectorI^ GetRange(long long start, long long end);

		/// <summary>
		/// sets values within a selected range
		/// </summary>
		/// <param name="start"> inclusive start index </param>
		/// <param name="end"> exclusive end index </param>
		/// <param name="val"> values to fill in the selected range </param>
		void SetRange(long long start, long long end, WVectorI^ val);

		#pragma endregion

	};


	/// <summary>
	/// real-valued vector class
	/// </summary>
	public ref class WVectorD
	{
	private:

		/// <summary>
		/// number of vector elements
		/// </summary>
		long long count;

		/// <summary>
		/// pointer to the values of vector elements
		/// </summary>
		void* values;
		//vector<double>* values;

	public:

		#pragma region constructors

		/// <summary>
		/// default constructor
		/// </summary>
		WVectorD();

		/// <summary>
		/// constructs a vector with given length
		/// and initialize all elements to zero
		/// </summary>
		/// <param name="n"> total number of elements </param>
		WVectorD(long long n);

		/// <summary>
		/// constructs a vector with given length
		/// and sets all element to a specific value 
		/// </summary>
		/// <param name="n"> total number of elements </param>
		/// <param name="initVal"> initial value of all the elements </param>
		/// <param name="mode"> #0: sequential; else: vectorized </param>
		WVectorD(long long n, double initVal, int mode);

		/// <summary>
		/// copy constructor (deep copy)
		/// </summary>
		/// <param name="other"> another vector as the source </param>
		/// <param name="mode"> #0: sequential; else: vectorized </param>
		WVectorD(WVectorD^ source, int mode);

		/// <summary>
		/// destructor
		/// </summary>
		~WVectorD();

		#pragma endregion 
		#pragma region properties

		/// <summary>
		/// total number of elements
		/// </summary>
		property long long Count
		{
			/// <summary>
			/// gets the total number of elements
			/// </summary>
			/// <returns> total number of elements </returns>
			long long get();

			/// <summary>
			/// sets the total number of elements
			/// </summary>
			/// <param name="n"> desired value </param>
			void set(long long n);
		}

		/// <summary>
		/// vector elements values
		/// </summary>
		property double* Values
		{
			/// <summary>
			/// gets the vector element values
			/// </summary>
			/// <returns> vector element values </returns>
			double* get();
		}

		/// <summary>
		/// gets / sets element value at given index 
		/// </summary>
		property double default[long long]
		{
			/// <summary>
			/// gets the element value at index i
			/// </summary>
			/// <param name="i"> index i </param>
			/// <returns> element value </returns>
			double get(long long i);
			
			/// <summary>
			/// sets the element value at index i
			/// </summary>
			/// <param name="i"> index i </param>
			/// <param name="val"> desired value </param>
			void set(long long i, double val);
		}

		#pragma endregion
		#pragma region methods

		/// <summary>
		/// copies values from another vector
		/// </summary>
		/// <param name="source"> another vector as the source </param>
		/// <param name="mode"> #0: seq. for; else: vectorized </param>
		void CopyFrom(WVectorD^ source, int mode);

		/// <summary>
		/// shifts the values by a constant number
		/// </summary>
		/// <param name="s"> shift value </param>
		/// <param name="mode"> #0: seq. for; else: vectorized </param>
		/// <returns> a new vector after constant shift </returns>
		void Shift(double s, int mode);

		/// <summary>
		/// gets values within a selected range
		/// </summary>
		/// <param name="start"> inclusive start index </param>
		/// <param name="end"> exclusive end index </param>
		/// <param name="mode"> #0: seq. for; else: vectorized </param>
		/// <returns> values within the selected range </returns>
		WVectorD^ GetRange(long long start, long long end, int mode);

		/// <summary>
		/// sets values within a selected range
		/// </summary>
		/// <param name="start"> inclusive start index </param>
		/// <param name="end"> exclusive end index </param>
		/// <param name="val"> values to fill in the selected range </param>
		/// <param name="mode"> #0: seq. for; else: vectorized </param>
		void SetRange(long long start, long long end, WVectorD^ val, int mode);

		#pragma endregion

	};

	/// <summary>
    /// real-valued sparse vector class
    /// </summary>
	public ref class WVectorDi
	{
	private:

		#pragma region properties

		/// <summary>
		/// total count of vector elements
		/// </summary>
		long long n;

		/// <summary>
		/// number of non-zero elements
		/// </summary>
		long long nnz;

		/// <summary>
		/// indices of non-zero elements
		/// </summary>
		void* nzIdx;
		//vector<long long>* nzIdx;

		/// <summary>
		/// values of the non-zero elements
		/// </summary>
		void* nzVal;
		//vector<double>* nzVal;

		#pragma endregion

	public:

		#pragma region constructors

		/// <summary>
		/// default constructor
		/// </summary>
		WVectorDi();

		/// <summary>
		/// constructs a sparse vector with
		/// totol number of elements and 
		/// non-zero element number
		/// </summary>
		/// <param name="n"> total number of vector elements </param>
		/// <param name="nnz"> number of non-zero elements </param>
		WVectorDi(long long n, long long nnz);

		/// <summary>
		/// constructs a sparse vector with
		/// its all information directly
		/// </summary>
		/// <param name="n"> total number of vector elements </param>
		/// <param name="nnz"> number of non-zero elements </param>
		/// <param name="nzIdx"> pointer to the non-zero indices </param>
		/// <param name="nzVal"> pointer to the non-zero values </param>
		/// <param name="mode"> #0: sequential; else: vectorized </param>
		WVectorDi(long long n, long long nnz,
			long long* nzIdx, double* nzVal, int mode);

		/// <summary>
		/// constructs a sparse vector with
		/// its all information directly
		/// </summary>
		/// <param name="n"> total number of vector elements </param>
		/// <param name="nnz"> number of non-zero elements </param>
		/// <param name="nzIdx"> vector containing the non-zero indices </param>
		/// <param name="nzVal"> vector containing the non-zero values </param>
		/// <param name="mode"> #0: sequential; else: vectorized </param>
		WVectorDi(long long n, long long nnz,
			WVectorI^ nzIdx, WVectorD^ nzVal, int mode);

		/// <summary>
		/// copy constructor (deep copy)
		/// </summary>
		/// <param name="other"> another vector as the source </param>
		/// <param name="mode"> #0: sequential; else: vectorized </param>
		WVectorDi(WVectorDi^ source, int mode);

		/// <summary>
		/// desctructor
		/// </summary>
		~WVectorDi();

		#pragma endregion
		#pragma region properties

		/// <summary>
		/// number of vector elements
		/// </summary>
		property long long Count
		{
			/// <summary>
			/// gets the number of vector elements
			/// </summary>
			/// <returns> total number of elements </returns>
			long long get();
		}

		/// <summary>
		/// number of non-zero elements
		/// </summary>
		property long long NonZeroCount
		{
			/// <summary>
			/// gets the number of non-zero elements
			/// </summary>
			/// <returns> number of non-zero elements </returns>
			long long get();
		}

		/// <summary>
		/// pointer to the indices of non-zero elements
		/// </summary>
		property long long* NonZeroIndices
		{
			/// <summary>
			/// gets the indices of non-zero elements
			/// </summary>
			/// <returns> indices of non-zero elements </returns>
			long long* get();
		}

		/// <summary>
		/// values of the non-zero elements
		/// </summary>
		property double* NonZeroValues
		{
			/// <summary>
			/// gets the values of non-zero elements
			/// </summary>
			/// <returns> values of non-zero elements </returns>
			double* get();
		}

		#pragma endregion
		#pragma region methods

		/// <summary>
		/// sets the indices of non-zero elements
		/// </summary>
		/// <param name="nzIndices"> indices of non-zero elements </param>
		void SetNonZeroIndices(vector<long long>* nzIndices);

		/// <summary>
		/// sets the indices of non-zero elements
		/// </summary>
		/// <param name="nzIndices"> pointer to the non-zero indices </param>
		void SetNonZeroIndices(long long* nzIndices);

		/// <summary>
		/// sets the values of non-zero elements 
		/// </summary>
		/// <param name="nzValues"> values of non-zero elements </param>
		void SetNonZeroValues(vector<double>* nzValues);

		/// <summary>
		/// sets the values of non-zero elements
		/// </summary>
		/// <param name="nzValues"> pointer to the non-zero values </param>
		/// <param name="mode"> #0: sequential; else: vectorized </param>
		void SetNonZeroValues(double* nzValues, int mode);

		/// <summary>
		/// sets the non-zero indices and values
		/// </summary>
		/// <param name="nzIndices"> pointer to the non-zero indices </param>
		/// <param name="nzValues"> pointer to the non-zero values </param>
		/// <param name="mode"> #0: sequential; else: vectorized </param>
		void SetNonZeroInfo(long long* nzIndices, double* nzValues,
			int mode);

		/// <summary>
		/// sets the i-th non-zero element with
		/// its index in the vector and its value 
		/// </summary>
		/// <param name="i"> index in all the non-zero elements </param>
		/// <param name="index"> index in the full vector </param>
		/// <param name="value"> value of the element </param>
		void SetANonZeroValue(long long i, long long index, 
			double value);

		#pragma endregion
		#pragma region computational methods

		///// <summary>
		///// adds a scalar multiple of compressed sparse vector 
		///// to a full-storage vector
		///// </summary>
		///// <param name="alpha"> scalar multiple </param>
		///// <param name="y"> full-storage vector (overwritte on exit) </param>
		//void Axpyi(double alpha, double* y);

		///// <summary>
		///// computes the dot product of a compressed sparse vector
		///// by a full-storage vector
		///// </summary>
		///// <param name="y"> full-storage vector </param>
		//double Doti(double* y);

		///// <summary>
		///// gathers a full-storage sparse vector's elements into compressed form
		///// </summary>
		///// <param name="n"> total number of elements in vector y </param>
		///// <param name="y"> full-storage vector y </param>
		///// <param name="nnz"> number of elements of vector y to be gathered </param>
		///// <param name="indx"> indicies of elements to be gathered </param>
		///// <param name="x"> x </param>
		//void Gthr(long long n, double* y, long long* indx, WVectorDi^ x);


		//void Gthr(long long nnz, double* y, double* x, long long* indx);

		#pragma endregion

	};


	/// <summary>
	/// complex-valued vector class
	/// </summary>
	public ref class WVectorZ
	{
	private:

        #pragma region properties

		/// <summary>
		/// number of vector elements
		/// </summary>
		long long count;

		/// <summary>
		/// pointer to the values of the vector elements
		/// </summary>
		void* values;

		#pragma endregion

	public:

		#pragma region constructors

		/// <summary>
		/// default constructor
		/// </summary>
		WVectorZ();

		/// <summary>
		/// constructs a vector with given length
		/// and initialize all elements to zero
		/// </summary>
		/// <param name="n"> total number of elements </param>
		WVectorZ(long long n);

		/// <summary>
		/// constructs a vector with given length
		/// and sets all element to a specific value 
		/// </summary>
		/// <param name="n"> total number of elements </param>
		/// <param name="initVal"> initial value of all the elements </param>
		/// <param name="mode"> #0: sequential; (#1: parallel;) else: vectorized </param>
		WVectorZ(long long n, WComplex^ initVal, int mode);

		/// <summary>
		/// copy constructor (deep copy)
		/// </summary>
		/// <param name="other"> another vector as the source </param>
		/// <param name="mode"> #0: sequential; (#1: parallel;) else: vectorized </param>
		WVectorZ(WVectorZ^ source, int mode);

		/// <summary>
		/// constructs a complex vector with its
		/// real- or imaginary part only
		/// </summary>
		/// <param name="part"> part of the complex vector </param>
		/// <param name="isRealPart"> is real- or imaginary-part </param>
		/// <param name="mode"> #0: seq. for; else: vectorized </param>
		WVectorZ(WVectorD^ part, bool isRealPart, int mode);

		/// <summary>
		/// destructor
		/// </summary>
		~WVectorZ();

		#pragma endregion 
		#pragma region properties

		/// <summary>
		/// total number of elements
		/// </summary>
		property long long Count
		{
			/// <summary>
			/// gets the total number of elements
			/// </summary>
			/// <returns> total number of elements </returns>
			long long get();

			/// <summary>
			/// sets the total number of elements
			/// </summary>
			/// <param name="n"> desired value </param>
			void set(long long n);
		}

		/// <summary>
		/// vector elements values
		/// </summary>
		property void* Values
		{
			/// <summary>
			/// gets the vector element values
			/// </summary>
			/// <returns> vector element values </returns>
			void* get();
		}

		/// <summary>
		/// gets / sets element value at given index 
		/// </summary>
		property WComplex^ default[long long]
		{
			/// <summary>
			/// gets the element value at index i
			/// </summary>
			/// <param name="i"> index i </param>
			/// <returns> element value </returns>
			WComplex^ get(long long i);

			/// <summary>
			/// sets the element value at index i
			/// </summary>
			/// <param name="i"> index i </param>
			/// <param name="val"> desired value </param>
			void set(long long i, WComplex^ val);
		}

		#pragma endregion
		#pragma region methods

		/// <summary>
		/// copies values from another vector
		/// </summary>
		/// <param name="source"> another vector as the source </param>
		/// <param name="mode"> #0: seq. for; else: vectorized </param>
		void CopyFrom(WVectorZ^ source, int mode);

		/// <summary>
		/// shifts the values by a constant number
		/// </summary>
		/// <param name="s"> shift value </param>
		/// <param name="mode"> #0: seq. for; else: vectorized </param>
		/// <returns> a new vector after constant shift </returns>
		void Shift(WComplex^ s, int mode);

		/// <summary>
		/// fills the selected part of the vector
		/// either real- or imaginary-part
		/// </summary>
		/// <param name="part"> values of the selected part </param>
		/// <param name="isRealPart"> is real- or imaginary part </param>
		/// <param name="mode"> #0: seq. for; else: vectorized </param>
		void FillPart(WVectorD^ part, bool isRealPart, int mode);

		/// <summary>
		/// gets values within a selected range
		/// </summary>
		/// <param name="start"> inclusive start index </param>
		/// <param name="end"> exclusive end index </param>
		/// <param name="mode"> #0: seq. for; else: vectorized </param>
		/// <returns> values within the selected range </returns>
		WVectorZ^ GetRange(long long start, long long end, int mode);

		/// <summary>
		/// sets values within a selected range
		/// </summary>
		/// <param name="start"> inclusive start index </param>
		/// <param name="end"> exclusive end index </param>
		/// <param name="val"> values to fill in the selected range </param>
		/// <param name="mode"> #0: seq. for; else: vectorized </param>
		void SetRange(long long start, long long end, WVectorZ^ val,
			int mode);

		#pragma endregion
	};

	/// <summary>
	/// complex-valued sparse vector class
	/// </summary>
	public ref class WVectorZi 
	{
	private:

		#pragma region properties

		/// <summary>
		/// total count of vector elements
		/// </summary>
		long long n;

		/// <summary>
		/// number of non-zero elements
		/// </summary>
		long long nnz;

		/// <summary>
		/// indices of non-zero elements
		/// </summary>
		void* nzIdx;

		/// <summary>
		/// values of the non-zero elements
		/// </summary>
		void* nzVal;

		#pragma endregion

	public:

		#pragma region constructors

		/// <summary>
		/// default constructor
		/// </summary>
		WVectorZi();

		/// <summary>
		/// constructs a sparse vector with
		/// totol number of elements and 
		/// non-zero element number
		/// </summary>
		/// <param name="n"> total number of vector elements </param>
		/// <param name="nnz"> number of non-zero elements </param>
		WVectorZi(long long n, long long nnz);

		/// <summary>
		/// constructs a sparse vector with
		/// its all information directly
		/// </summary>
		/// <param name="n"> total number of vector elements </param>
		/// <param name="nnz"> number of non-zero elements </param>
		/// <param name="nzIdx"> pointer to the non-zero indices </param>
		/// <param name="nzVal"> pointer to the non-zero values </param>
		/// <param name="mode"> #0: sequential; else: vectorized </param>
		WVectorZi(long long n, long long nnz,
			long long* nzIdx, void* nzVal, int mode);

		/// <summary>
		/// constructs a sparse vector with
		/// its all information directly
		/// </summary>
		/// <param name="n"> total number of vector elements </param>
		/// <param name="nnz"> number of non-zero elements </param>
		/// <param name="nzIdx"> vector containing the non-zero indices </param>
		/// <param name="nzVal"> vector containing the non-zero values </param>
		/// <param name="mode"> #0: sequential; else: vectorized </param>
		WVectorZi(long long n, long long nnz,
			WVectorI^ nzIdx, WVectorZ^ nzVal, int mode);

		/// <summary>
		/// copy constructor (deep copy)
		/// </summary>
		/// <param name="other"> another vector as the source </param>
		/// <param name="mode"> #0: sequential; else: vectorized </param>
		WVectorZi(WVectorZi^ source, int mode);

		/// <summary>
		/// desctructor
		/// </summary>
		~WVectorZi();

		#pragma endregion
		#pragma region properties

		/// <summary>
		/// number of vector elements
		/// </summary>
		property long long Count
		{
			/// <summary>
			/// gets the number of vector elements
			/// </summary>
			/// <returns> total number of elements </returns>
			long long get();
		}

		/// <summary>
		/// number of non-zero elements
		/// </summary>
		property long long NonZeroCount
		{
			/// <summary>
			/// gets the number of non-zero elements
			/// </summary>
			/// <returns> number of non-zero elements </returns>
			long long get();
		}

		/// <summary>
		/// pointer to the indices of non-zero elements
		/// </summary>
		property long long* NonZeroIndices
		{
			/// <summary>
			/// gets the indices of non-zero elements
			/// </summary>
			/// <returns> indices of non-zero elements </returns>
			long long* get();
		}

		/// <summary>
		/// values of the non-zero elements
		/// </summary>
		property void* NonZeroValues
		{
			/// <summary>
			/// gets the values of non-zero elements
			/// </summary>
			/// <returns> values of non-zero elements </returns>
			void* get();
		}

		#pragma endregion
		#pragma region methods

		/// <summary>
		/// sets the indices of non-zero elements
		/// </summary>
		/// <param name="nzIndices"> pointer to the non-zero indices </param>
		void SetNonZeroIndices(long long* nzIndices);

		/// <summary>
		/// sets the values of non-zero elements
		/// </summary>
		/// <param name="nzValues"> pointer to the non-zero values </param>
		/// <param name="mode"> #0: sequential; else: vectorized </param>
		void SetNonZeroValues(void* nzValues, int mode);

		/// <summary>
		/// sets the non-zero indices and values
		/// </summary>
		/// <param name="nzIndices"> pointer to the non-zero indices </param>
		/// <param name="nzValues"> pointer to the non-zero values </param>
		/// <param name="mode"> #0: sequential; else: vectorized </param>
		void SetNonZeroInfo(long long* nzIndices, void* nzValues,
			int mode);

		/// <summary>
		/// sets the i-th non-zero element with
		/// its index in the vector and its value 
		/// </summary>
		/// <param name="i"> index in all the non-zero elements </param>
		/// <param name="index"> index in the full vector </param>
		/// <param name="value"> value of the element </param>
		void SetANonZeroValue(long long i, long long index,
			WComplex^ value);

		#pragma endregion
	};


	/// <summary>
	/// real-valued matrix class
	/// </summary>
	public ref class WMatrixD 
	{
	private:

		/// <summary>
		/// number of rows
		/// </summary>
		long long rows;

		/// <summary>
		/// number of columns
		/// </summary>
		long long cols;

		/// <summary>
		/// pointer to the values of matrix elements
		/// </summary>
		void* values;
		//vector<double>* values;

	public:

		#pragma region constructors

		/// <summary>
		/// default constructor
		/// </summary>
		WMatrixD();

		/// <summary>
		/// constructs a matrix with given rows and columns
		/// with all the elements set to zero
		/// </summary>
		/// <param name="nRows"> number of rows </param>
		/// <param name="nCols"> number of columns </param>
		WMatrixD(long long nRows, long long nCols);

		/// <summary>
		/// constructs a matrix with given rows and columns
		/// with all the elements set to a given initial value
		/// </summary>
		/// <param name="nRows"> number of rows </param>
		/// <param name="nCols"> number of columns </param>
		/// <param name="initVal"> initial value </param>
		/// <param name="mode"> #0: sequential; else: vectorized </param>
		WMatrixD(long long nRows, long long nCols, double initVal,
			int mode);

		/// <summary>
		/// copy constructor (deep copy)
		/// </summary>
		/// <param name="source"> another vector as the source </param>
		/// <param name="mode"> #0: sequential; else: vectorized </param>
		WMatrixD(WMatrixD^ source, int mode);

		/// <summary>
		/// destructor
		/// </summary>
		~WMatrixD();

		#pragma endregion
		#pragma region properties

		/// <summary>
		/// number of rows
		/// </summary>
		property long long Rows
		{
			/// <summary>
			/// gets the number of rows
			/// </summary>
			/// <returns> number of rows </returns>
			long long get();

			/// <summary>
			/// sets the number of rows
			/// </summary>
			/// <param name="n"> desired value </param>
			void set(long long nRows);
		}

		/// <summary>
		/// number of columns
		/// </summary>
		property long long Cols
		{
			/// <summary>
			/// gets the number of columns
			/// </summary>
			/// <returns> number of columns </returns>
			long long get();

			/// <summary>
			/// sets the number of columns
			/// </summary>
			/// <param name="n"> desired value </param>
			void set(long long nCols);
		}

		/// <summary>
		/// vector elements values
		/// </summary>
		property double* Values
		{
			/// <summary>
			/// gets the vector element values
			/// </summary>
			/// <returns> vector element values </returns>
			double* get();
		}

		/// <summary>
		/// gets / sets element value at given index pair
		/// </summary>
		property double default[long long, long long]
		{
			/// <summary>
			/// gets the element value at [iRow, iCol]
			/// </summary>
			/// <param name="iRow"> row index </param>
			/// <param name="iCol"> column index </param>
			/// <returns> element value </returns>
			double get(long long iRow, long long iCol);

			/// <summary>
			/// sets the element value at [iRow, iCol]
			/// </summary>
			/// <param name="iRow"> row index </param>
			/// <param name="iCol"> column index </param>
			/// <param name="val"> desired value </param>
			void set(long long iRow, long long iCol, double val);
		}

		#pragma endregion
		#pragma region methods

		/// <summary>
		/// copies values from another vector
		/// </summary>
		/// <param name="source"> another vector as the source </param>
		/// <param name="mode"> #0: seq. for; else: vectorized </param>
		void CopyFrom(WMatrixD^ source, int mode);

		/// <summary>
		/// shifts the values by a constant number
		/// </summary>
		/// <param name="s"> shift value </param>
		/// <param name="mode"> #0: seq. for; else: vectorized </param>
		/// <returns> a new vector after constant shift </returns>
		void Shift(double s, int mode);

		#pragma endregion

	};

	/// <summary>
	/// real-valued sparse matrix class
	/// </summary>
	public ref class WMatrixDi
	{
	private:

		sparse_index_base_t indexing;
		long long rows;
		long long cols;
		long nnz;
		//sparse_matrix_t mat;
		sparse_matrix_t* handle; // = &mat;
		//vector<double>* nzVal;
		sparse_status_t status;
		
		// csr
		//vector<long long>* rowPtr;
		//vector<long long>* colIdx;

		// csc
		//vector<long long>* colPtr;
		//vector<long long>* rowIdx;

		// coo
		//vector<long long>* rowIdx;
		//vector<long long>* colIdx;

	public:

		#pragma region constructors

		/// <summary>
		/// default constructor
		/// </summary>
		WMatrixDi();

		/// <summary>
		/// constructs a WMatrixDi with given parameters
		/// </summary>
		/// <param name="rows"> number of rows </param>
		/// <param name="cols"> number of columns </param>
		/// <param name="nnz"> number of non-zero elements </param>
		WMatrixDi(long long rows, long long cols, long long nnz);

		/// <summary>
		/// copy constructor (deep copy)
		/// </summary>
		/// <param name="other"> another sparse matrix as the source </param>
		WMatrixDi(WMatrixDi^ source);

		/// <summary>
		/// destructor
		/// </summary>
		~WMatrixDi();

		#pragma endregion
		#pragma region properties

		/// <summary>
		/// indicates the indexing (zero- or one-based)
		/// </summary>
		property sparse_index_base_t Indexing
		{
			/// <summary>
			/// gets the indexing
			/// </summary>
			/// <returns> indexing </returns>
			sparse_index_base_t get();

			/// <summary>
			/// sets the indexing value
			/// </summary>
			/// <param name="value"> indexing value </param>
			void set(sparse_index_base_t value);
		}

		/// <summary>
		/// handle containing the internal data of the sparse matrix 
		/// </summary>
		property sparse_matrix_t* Handle
		{
			/// <summary>
			/// gets the handle of the sparse matrix 
			/// </summary>
			/// <returns> handle of the sparse matrix </returns>
			sparse_matrix_t* get();
			
			/// <summary>
			/// sets the handle value
			/// </summary>
			/// <param name="value"> handle value </param>
			void set(sparse_matrix_t* value);
		}


		/// <summary>
		/// number of rows
		/// </summary>
		property long long Rows
		{
			/// <summary>
			/// gets the number of rows
			/// </summary>
			/// <returns> number of rows </returns>
			long long get();
		}

		/// <summary>
		/// number of columns
		/// </summary>
		property long long Cols
		{
			/// <summary>
			/// gets the number of colums
			/// </summary>
			/// <returns> number of colums </returns>
			long long get();
		}

		/// <summary>
		/// number of non-zero elements
		/// </summary>
		property long long NonZeroCount
		{
			/// <summary>
			/// gets the number of non-zero elements
			/// </summary>
			/// <returns> number of non-zero elements </returns>
			long long get();
		}

		/// <summary>
		/// row indices
		/// </summary>
		/*property long long* RowIndices
		{
			/// <summary>
			/// gets the row indices
			/// </summary>
			/// <returns> row indices </returns>
			long long* get();
		}*/

		/// <summary>
		/// column indices
		/// </summary>
		/*property long long* ColIndices
		{
			/// <summary>
			/// gets the column indices
			/// </summary>
			/// <returns> column indices </returns>
			long long* get();
		}*/

		/// <summary>
		/// non-zero values
		/// </summary>
		/*property double* NonZeroValues
		{
			/// <summary>
			/// gets the non-zero values
			/// </summary>
			/// <returns> non-zero values </returns>
			double* get();
		}*/

		// status of the sparse matrix
		property sparse_status_t Status
		{
			/// <summary>
			/// gets the sparse status
			/// </summary>
			/// <returns> sparse status </returns>
			sparse_status_t get();

			/// <summary>
			/// sets the sparse status
			/// </summary>
			/// <param name="value"> status value </param>
			void set(sparse_status_t value);
		}


		#pragma endregion
		#pragma region methods

		void CreateCSR(long long* rowPtr, long long* colIdx, double* nzVal);
		void CreateCSC(long long* colPtr, long long* rowIdx, double* nzVal);
		void CreateCOO(long long* rowIdx, long long* colIdx, double* nzVal);

		/// <summary>
		/// sets the i-th non-zero element with
		/// its row & column indices in the vector 
		/// and the element value 
		/// </summary>
		/// <param name="i"> index in all the non-zero elements </param>
		/// <param name="row"> row index </param>
		/// <param name="col"> column index </param>
		/// <param name="value"> value of the element </param>
		void SetNonZeroValue(long long i, 
			long long row, long long col, double value);

		/// <summary>
		/// creates the sparse matrix
		/// </summary>
		void Create();

		/// <summary>
		/// computes the matrix-vector product
		/// </summary>
		void MV(WVectorD^ v, WVectorD^ r);


		void Product(double* x, double* y);


		//void Scatter(long long rowNum, long long colNum, double* nzVal, int status);
		//void ExportCSR(long long row, long long col, 
		//	long long* rowStart, long long* rowEnd, long long* colIdx,
		//	double* nzVal);

		#pragma endregion

	};


	/// <summary>
	/// complex-valued matrix class
	/// </summary>
	public ref class WMatrixZ 
	{
	private:

		/// <summary>
		/// number of rows
		/// </summary>
		long long rows;

		/// <summary>
		/// number of columns
		/// </summary>
		long long cols;

		/// <summary>
		/// pointer to the values of matrix elements
		/// </summary>
		void* values;
	
	public:

		#pragma region constructors

		/// <summary>
		/// default constructor
		/// </summary>
		WMatrixZ();

		/// <summary>
		/// constructs a matrix with given rows and columns
		/// with all the elements set to zero
		/// </summary>
		/// <param name="nRows"> number of rows </param>
		/// <param name="nCols"> number of columns </param>
		WMatrixZ(long long nRows, long long nCols);

		/// <summary>
		/// constructs a matrix with given rows and columns
		/// with all the elements set to a given initial value
		/// </summary>
		/// <param name="nRows"> number of rows </param>
		/// <param name="nCols"> number of columns </param>
		/// <param name="initVal"> initial value </param>
		/// <param name="mode"> #0: sequential; else: vectorized </param>
		WMatrixZ(long long nRows, long long nCols, WComplex^ initVal,
			int mode);

		/// <summary>
		/// constructs a complex matrix with its
		/// real- or imaginary part only
		/// </summary>
		/// <param name="part"> part of the complex vector </param>
		/// <param name="isRealPart"> is real- or imaginary-part </param>
		/// <param name="mode"> #0: seq. for; else: vectorized </param>
		WMatrixZ(WMatrixD^ part, bool isRealPart, int mode);

		/// <summary>
		/// copy constructor (deep copy)
		/// </summary>
		/// <param name="source"> another vector as the source </param>
		/// <param name="mode"> #0: sequential; else: vectorized </param>
		WMatrixZ(WMatrixZ^ source, int mode);

		/// <summary>
		/// destructor
		/// </summary>
		~WMatrixZ();

		#pragma endregion
		#pragma region properties

		/// <summary>
		/// number of rows
		/// </summary>
		property long long Rows
		{
			/// <summary>
			/// gets the number of rows
			/// </summary>
			/// <returns> number of rows </returns>
			long long get();

			/// <summary>
			/// sets the number of rows
			/// </summary>
			/// <param name="n"> desired value </param>
			void set(long long nRows);
		}

		/// <summary>
		/// number of columns
		/// </summary>
		property long long Cols
		{
			/// <summary>
			/// gets the number of columns
			/// </summary>
			/// <returns> number of columns </returns>
			long long get();

			/// <summary>
			/// sets the number of columns
			/// </summary>
			/// <param name="n"> desired value </param>
			void set(long long nCols);
		}

		/// <summary>
		/// vector elements values
		/// </summary>
		property void* Values
		{
			/// <summary>
			/// gets the vector element values
			/// </summary>
			/// <returns> vector element values </returns>
			void* get();
		}

		/// <summary>
		/// gets / sets element value at given index pair
		/// </summary>
		property WComplex^ default[long long, long long]
		{
			/// <summary>
			/// gets the element value at [iRow, iCol]
			/// </summary>
			/// <param name="iRow"> row index </param>
			/// <param name="iCol"> column index </param>
			/// <returns> element value </returns>
			WComplex^ get(long long iRow, long long iCol);

			/// <summary>
			/// sets the element value at [iRow, iCol]
			/// </summary>
			/// <param name="iRow"> row index </param>
			/// <param name="iCol"> column index </param>
			/// <param name="val"> desired value </param>
			void set(long long iRow, long long iCol, WComplex^ val);
		}

		#pragma endregion
		#pragma region methods

		/// <summary>
		/// copies values from another vector
		/// </summary>
		/// <param name="source"> another vector as the source </param>
		/// <param name="mode"> #0: seq. for; else: vectorized </param>
		void CopyFrom(WMatrixZ^ source, int mode);

		/// <summary>
		/// shifts the values by a constant number
		/// </summary>
		/// <param name="s"> shift value </param>
		/// <param name="mode"> #0: seq. for; else: vectorized </param>
		/// <returns> a new vector after constant shift </returns>
		void Shift(WComplex^ s, int mode);

		/// <summary>
		/// fills the selected part of the matrix
		/// either real- or imaginary-part
		/// </summary>
		/// <param name="part"> values of the selected part </param>
		/// <param name="isRealPart"> is real- or imaginary part </param>
		/// <param name="mode"> #0: seq. for; else: vectorized </param>
		void FillPart(WMatrixD^ part, bool isRealPart, int mode);

		#pragma endregion

	};

	/// <summary>
	/// complex-valued sparse matrix class
	/// </summary>
	public ref class WMatrixZi 
	{
	private:
		sparse_index_base_t indexing;
		long long rows;
		long long cols;
		long nnz;
		sparse_matrix_t* handle;
		sparse_status_t status;
	public:

		#pragma region constructors

		/// <summary>
		/// default constructor
		/// </summary>
		WMatrixZi();

		/// <summary>
		/// constructs a WMatrixZi with given parameters
		/// </summary>
		/// <param name="rows"> number of rows </param>
		/// <param name="cols"> number of columns </param>
		/// <param name="nnz"> number of non-zero elements </param>
		WMatrixZi(long long rows, long long cols, long long nnz);

		/// <summary>
		/// copy constructor (deep copy)
		/// </summary>
		/// <param name="other"> another sparse matrix as the source </param>
		WMatrixZi(WMatrixZi^ source);

		/// <summary>
		/// destructor
		/// </summary>
		~WMatrixZi();

		#pragma endregion
		#pragma region properties

		/// <summary>
		/// indicates the indexing (zero- or one-based)
		/// </summary>
		property sparse_index_base_t Indexing
		{
			/// <summary>
			/// gets the indexing
			/// </summary>
			/// <returns> indexing </returns>
			sparse_index_base_t get();

			/// <summary>
			/// sets the indexing value
			/// </summary>
			/// <param name="value"> indexing value </param>
			void set(sparse_index_base_t value);
		}

		/// <summary>
		/// handle containing the internal data of the sparse matrix 
		/// </summary>
		property sparse_matrix_t* Handle
		{
			/// <summary>
			/// gets the handle of the sparse matrix 
			/// </summary>
			/// <returns> handle of the sparse matrix </returns>
			sparse_matrix_t* get();

			/// <summary>
			/// sets the handle value
			/// </summary>
			/// <param name="value"> handle value </param>
			void set(sparse_matrix_t* value);
		}

		/// <summary>
		/// number of rows
		/// </summary>
		property long long Rows
		{
			/// <summary>
			/// gets the number of rows
			/// </summary>
			/// <returns> number of rows </returns>
			long long get();
		}

		/// <summary>
		/// number of columns
		/// </summary>
		property long long Cols
		{
			/// <summary>
			/// gets the number of colums
			/// </summary>
			/// <returns> number of colums </returns>
			long long get();
		}

		/// <summary>
		/// number of non-zero elements
		/// </summary>
		property long long NonZeroCount
		{
			/// <summary>
			/// gets the number of non-zero elements
			/// </summary>
			/// <returns> number of non-zero elements </returns>
			long long get();
		}

		// status of the sparse matrix
		property sparse_status_t Status
		{
			/// <summary>
			/// gets the sparse status
			/// </summary>
			/// <returns> sparse status </returns>
			sparse_status_t get();

			/// <summary>
			/// sets the sparse status
			/// </summary>
			/// <param name="value"> status value </param>
			void set(sparse_status_t value);
		}

		#pragma endregion
		#pragma region methods

		// ...

		#pragma endregion

	};



	/// <summary>
	/// BLAS interface
	/// </summary>
	public ref class IBLAS
	{
	public:

		#pragma region BLAS Level-1

		/// <summary>
		/// computes the sum of magnitudes of the vector elements
		/// [also works with sparse vectors]
		/// </summary>
		/// <param name="n"> the number of vector element </param>
		/// <param name="x"> vector x </param>
		/// <param name="incx"> increment in x </param>
		/// <returns> sum of magnitude of all elements </returns>
		virtual double Asum(long long n,
			double* x, long long incx);

		/// <summary>
		/// computes the sum of magnitudes of the vector elements
		/// [also works with sparse vectors]
		/// </summary>
		/// <param name="n"> the number of vector element </param>
		/// <param name="x"> vector x </param>
		/// <param name="incx"> increment in x </param>
		/// <returns> sum of magnitude of all elements </returns>
		virtual double Asum(long long n,
			void* x, long long incx);

		/// <summary>
		/// computes a scalar-array product and 
		/// adds the result to another array
		/// y := alpha * x + y
		/// </summary>
		/// <param name="n"> number of array elements </param>
		/// <param name="alpha"> scalar alpha </param>
		/// <param name="x"> array x </param>
		/// <param name="incx"> increment in x </param>
		/// <param name="y"> array y </param>
		/// <param name="incy"> increment in y </param>
		virtual void Axpy(long long n,
			double alpha, double* x, long long incx,
			double* y, long long incy);

		/// <summary>
		/// computes a scalar-array product and 
		/// adds the result to another array
		/// y := alpha * x + y
		/// </summary>
		/// <param name="n"> number of array elements </param>
		/// <param name="alpha"> scalar alpha </param>
		/// <param name="x"> array x </param>
		/// <param name="incx"> increment in x </param>
		/// <param name="y"> array y </param>
		/// <param name="incy"> increment in y </param>
		virtual void Axpy(long long n,
			void* alpha, void* x, long long incx,
			void* y, long long incy);

		/// <summary>
		/// copies vector x to vector y
		/// [also works with sparse vectors]
		/// </summary>
		/// <param name="n"> the number of vector elements </param>
		/// <param name="x"> vector x </param>
		/// <param name="incx"> increment in x </param>
		/// <param name="y"> vector y </param>
		/// <param name="incy"> increment in y</param>
		virtual void Copy(long long n,
			double* x, long long incx,
			double* y, long long incy);

		/// <summary>
		/// copies vector x to vector y
		/// [also works with sparse vectors]
		/// </summary>
		/// <param name="n"> the number of vector elements </param>
		/// <param name="x"> vector x </param>
		/// <param name="incx"> increment in x </param>
		/// <param name="y"> vector y </param>
		/// <param name="incy"> increment in y</param>
		virtual void Copy(long long n,
			void* x, long long incx,
			void* y, long long incy);

		/// <summary>
		/// computes a vector-vector dot product
		/// </summary>
		/// <param name="n"> number of vector elements </param>
		/// <param name="x"> vector x </param>
		/// <param name="incx"> increment in x </param>
		/// <param name="y"> vector y </param>
		/// <param name="incy"> increment in y</param>
		/// <returns> dot product </returns>
		virtual double Dot(long long n,
			double* x, long long incx,
			double* y, long long incy);

		/// <summary>
		/// computes a vector-vector dot product
		/// </summary>
		/// <param name="n"> number of vector elements </param>
		/// <param name="x"> vector x </param>
		/// <param name="incx"> increment in x </param>
		/// <param name="y"> vector y </param>
		/// <param name="incy"> increment in y </param>
		/// <param name="dotu"> dot product </param>
		virtual void Dotu(long long n,
			void* x, long long incx,
			void* y, long long incy, void* dotu);

		/// <summary>
		/// computes the dot product of a conjugated vector
		/// with another vector
		/// </summary>
		/// <param name="n"> number of vector elements </param>
		/// <param name="x"> vector x (to be conjugated) </param>
		/// <param name="incx"> increment in x </param>
		/// <param name="y"> vector y </param>
		/// <param name="incy"> increment in y</param>
		/// <param name="dotc"> dot product </param>
		virtual void Dotc(long long n,
			void* x, long long incx,
			void* y, long long incy, void* dotc);

		/// <summary>
		/// computes the Euclidean norm of a vector
		/// res = || x ||
		/// [also works with sparse vectors]
		/// </summary>
		/// <param name="n"> the number of vector elements </param>
		/// <param name="x"> vector x </param>
		/// <param name="incx"> increment in x </param>
		/// <returns> Euclidean norm </returns>
		virtual double Nrm2(long long n,
			double* x, long long incx);

		/// <summary>
		/// computes the Euclidean norm of a vector
		/// res = || x ||
		/// [also works with sparse vectors]
		/// </summary>
		/// <param name="n"> the number of vector elements </param>
		/// <param name="x"> vector x </param>
		/// <param name="incx"> increment in x </param>
		/// <returns> Euclidean norm </returns>
		virtual double Nrm2(long long n,
			void* x, long long incx);

		/// <summary>
		/// performs rotation of point in the plane
		/// xi = c * xi + s * yi
		/// yi = c * yi - s * xi
		/// </summary>
		/// <param name="n"> the number of elements in vectors </param>
		/// <param name="x"> vector x </param>
		/// <param name="incx"> increment in x </param>
		/// <param name="y"> vector y </param>
		/// <param name="incy"> increment in y </param>
		/// <param name="c"> a scalar </param>
		/// <param name="s"> a scalar </param>
		virtual void Rot(long long n,
			double* x, long long incx,
			double* y, long long incy,
			double c, double s);

		/// <summary>
		/// performs rotation of point in the plane
		/// xi = c * xi + s * yi
		/// yi = c * yi - s * xi
		/// </summary>
		/// <param name="n"> the number of elements in vectors </param>
		/// <param name="x"> vector x </param>
		/// <param name="incx"> increment in x </param>
		/// <param name="y"> vector y </param>
		/// <param name="incy"> increment in y </param>
		/// <param name="c"> a scalar </param>
		/// <param name="s"> a scalar </param>
		virtual void Rot(long long n,
			void* x, long long incx,
			void* y, long long incy,
			double c, double s);

		/// <summary>
		/// computes the product of a vector by a scalar
		/// [also works with sparse vectors]
		/// </summary>
		/// <param name="n"> the number of vector elements </param>
		/// <param name="alpha"> scalar multiple </param>
		/// <param name="x"> vector x </param>
		/// <param name="incx"> increment in x </param>
		virtual void Scal(long long n, double alpha,
			double* x, long long incx);

		/// <summary>
		/// computes the product of a vector by a scalar
		/// [also works with sparse vectors]
		/// </summary>
		/// <param name="n"> the number of vector elements </param>
		/// <param name="alpha"> scalar multiple </param>
		/// <param name="x"> vector x </param>
		/// <param name="incx"> increment in x </param>
		virtual void Scal(long long n, void* alpha,
			void* x, long long incx);

		/// <summary>
		/// finds the index of the element in the vector
		/// with maximum absolute value
		/// [also works with sparse vectors]
		/// </summary>
		/// <param name="n"> the number of vector elements </param>
		/// <param name="x"> vector x </param>
		/// <param name="incx"> increment in x </param>
		/// <returns> index of the element </returns>
		virtual long long Iamax(long long n,
			double* x, long long incx);

		/// <summary>
		/// finds the index of the element in the vector
		/// with maximum absolute value
		/// [also works with sparse vectors]
		/// </summary>
		/// <param name="n"> the number of vector elements </param>
		/// <param name="x"> vector x </param>
		/// <param name="incx"> increment in x </param>
		/// <returns> index of the element </returns>
		virtual long long Iamax(long long n,
			void* x, long long incx);

		/// <summary>
		/// finds the index of the element in the vector
		/// with minimum absolute value
		/// [also works with sparse vectors]
		/// </summary>
		/// <param name="n"> the number of vector elements </param>
		/// <param name="x"> vector x </param>
		/// <param name="incx"> increment in x </param>
		/// <returns> index of the element </returns>
		virtual long long Iamin(long long n,
			double* x, long long incx);

		/// <summary>
		/// finds the index of the element in the vector
		/// with minimum absolute value
		/// [also works with sparse vectors]
		/// </summary>
		/// <param name="n"> the number of vector elements </param>
		/// <param name="x"> vector x </param>
		/// <param name="incx"> increment in x </param>
		/// <returns> index of the element </returns>
		virtual long long Iamin(long long n,
			void* x, long long incx);

		#pragma endregion
		#pragma region BLAS Level-2

		/// <summary>
		/// computes a matrix-vector product using a general matrix
		/// y := alpha * a * x + beta * y, or
		/// y := alpha * a' * y + beta * y
		/// y := alpha * conj(a') * x + beta * y            
		/// </summary>
		/// <param name="layout"> matrix storage </param>
		/// <param name="operation"> matrix operation </param>
		/// <param name="m"> number of rows </param>
		/// <param name="n"> number of columns </param>
		/// <param name="alpha"> scalar parameter </param>
		/// <param name="a"> matrix a </param>
		/// <param name="lda"> leading dimension of matrix a </param>
		/// <param name="x"> vector x </param>
		/// <param name="incx"> increment in x </param>
		/// <param name="beta"> scalar parameter </param>
		/// <param name="y"> vector y </param>
		/// <param name="incy"> increment in y </param>
		virtual void Gemv(CBLAS_LAYOUT layout, CBLAS_TRANSPOSE operation,
			long long m, long long n, 
			double alpha, double* a, long long lda,
			double* x, long long incx,
			double beta, double* y, long long incy);

		#pragma endregion
		#pragma region BLAS Level-3

		/// <summary>
		/// computes a matrix-matrix product with general
		/// c := alpha * op(a) * op(b) + beta * c
		/// op(x) can be 1) identical; 2) transpose
		/// </summary>
		/// <param name="layout"> matrix storage </param>
		/// <param name="transa"> matrix a operation </param>
		/// <param name="transb"> matrix b operation </param>
		/// <param name="m"> number of rows of op(a) </param>
		/// <param name="n"> number of columns of op(b) </param>
		/// <param name="k"> number of columns of op(a) </param>
		/// <param name="alpha"> scalar parameter </param>
		/// <param name="a"> matrix a </param>
		/// <param name="lda"> leading dimension of matrix a </param>
		/// <param name="b"> matrix b </param>
		/// <param name="ldb"> leading dimension of matrix b </param>
		/// <param name="beta"> scalar parameter </param>
		/// <param name="c"> matrix c </param>
		/// <param name="ldc"> leading dimension of matrix c </param>
		virtual void Gemm(CBLAS_LAYOUT layout,
			CBLAS_TRANSPOSE operationa, CBLAS_TRANSPOSE operationb,
			long long m, long long n, long long k,
			double alpha, double* a, long long lda,
			double* b, long long ldb,
			double beta, double* c, long long ldc);

		#pragma endregion

		#pragma region SPARSE BLAS Level-1

		/// <summary>
		/// Adds a scalar multiple of compressed sparse vector
		/// to a full-storage vector
		/// </summary>
		/// <param name="nz"> the number of non-zero elements </param>
		/// <param name="alpha"> scalar alpha </param>
		/// <param name="x"> array containing the non-zero elements </param>
		/// <param name="indx"> array specifuing the indices of non-zero elements </param>
		/// <param name="y"> the full-storage vector [will be updated] </param>
		virtual void Axpyi(long long nz,
			double alpha, double* x, long long* indx,
			double* y);

		/// <summary>
		/// Adds a scalar multiple of compressed sparse vector
		/// to a full-storage vector
		/// </summary>
		/// <param name="nz"> the number of non-zero elements </param>
		/// <param name="alpha"> scalar alpha </param>
		/// <param name="x"> array containing the non-zero elements </param>
		/// <param name="indx"> array specifuing the indices of non-zero elements </param>
		/// <param name="y"> the full-storage vector [will be updated] </param>
		virtual void Axpyi(long long nz,
			void* alpha, void* x, long long* indx,
			void* y);

		/// <summary>
		/// computes the dot product of a compressed sparse vector x
		/// by a full-storage vector y
		/// </summary>
		/// <param name="nz"> the number of non-zero elements </param>
		/// <param name="x"> array containing the non-zero elements </param>
		/// <param name="indx"> array specifuing the indices of non-zero elements </param>
		/// <param name="y"> the full-storage vector </param>
		/// <returns> dot product </returns>
		virtual double Doti(long long nz,
			double* x, long long* indx,
			double* y);

		/// <summary>
		/// computes the dot product of a compressed sparse vector x
		/// by a full-storage vector y
		/// </summary>
		/// <param name="nz"> the number of non-zero elements </param>
		/// <param name="x"> array containing the non-zero elements </param>
		/// <param name="indx"> array specifuing the indices of non-zero elements </param>
		/// <param name="y"> the full-storage vector </param>
		/// <param name="dotu"> dot product </param>
		virtual void Dotui(long long nz,
			void* x, long long* indx,
			void* y, void* dotu);

		/// <summary>
		/// computes the conjugated dot product of a compressed 
		/// sparse complex vector with a full-storage complex vector
		/// </summary>
		/// <param name="nz"> the number of non-zero elements </param>
		/// <param name="x"> array containing the non-zero elements (to be conjugated) </param>
		/// <param name="indx"> array specifuing the indices of non-zero elements </param>
		/// <param name="y"> the full-storage vector </param>
		/// <param name="dotc"> dot product </param>
		virtual void Dotci(long long nz,
			void* x, long long* indx,
			void* y, void* dotc);

		/// <summary>
		/// gathers a full-storage sparse vector's elements 
		/// into compressed form
		/// </summary>
		/// <param name="nz"> the number of elements in y to be gathered </param>
		/// <param name="y"> the full-storage vector y </param>
		/// <param name="x"> the compressed sparse vector x </param>
		/// <param name="indx"> the indices of elements in y to be gathered </param>
		virtual void Gthr(long long nz,
			double* y,
			double* x, long long* indx);

		/// <summary>
		/// gathers a full-storage sparse vector's elements 
		/// into compressed form
		/// </summary>
		/// <param name="nz"> the number of elements in y to be gathered </param>
		/// <param name="y"> the full-storage vector y </param>
		/// <param name="x"> the compressed sparse vector x </param>
		/// <param name="indx"> the indices of elements in y to be gathered </param>
		virtual void Gthr(long long nz,
			void* y,
			void* x, long long* indx);

		/// <summary>
		/// applies Givens rotation to sparse vectors 
		/// one of which is in compressed form
		/// </summary>
		/// <param name="nz"> number of non-zero element in sparse vector x </param>
		/// <param name="x"> non-zero values in sparse vector x </param>
		/// <param name="indx"> non-zero indices in sparse vector x </param>
		/// <param name="y"> dense vector y </param>
		/// <param name="c"> scalar constant c </param>
		/// <param name="s"> scalar constant s </param>
		virtual void Roti(long long nz, double* x, long long* indx, 
			double* y, double c, double s);

		/// <summary>
		/// applies Givens rotation to sparse vectors 
		/// one of which is in compressed form
		/// </summary>
		/// <param name="nz"> number of non-zero element in sparse vector x </param>
		/// <param name="x"> non-zero values in sparse vector x </param>
		/// <param name="indx"> non-zero indices in sparse vector x </param>
		/// <param name="y"> dense vector y </param>
		/// <param name="c"> scalar constant c </param>
		/// <param name="s"> scalar constant s </param>
		virtual void Roti(long long nz, void* x, long long* indx,
			void* y, double c, double s);

		/// <summary>
		/// converts a compressed sparse vector x 
		/// into full-storage vector y
		/// </summary>
		/// <param name="nz"> the number of non-zero elemente </param>
		/// <param name="x"> compressed sparse vector x </param>
		/// <param name="indx"> the indices of non-zero elements </param>
		/// <param name="y"> full-storage vector y </param>
		virtual void Sctr(long long nz, 
			double* x, long long* indx, 
			double* y);

		/// <summary>
		/// converts a compressed sparse vector x 
		/// into full-storage vector y
		/// </summary>
		/// <param name="nz"> the number of non-zero elemente </param>
		/// <param name="x"> compressed sparse vector x </param>
		/// <param name="indx"> the indices of non-zero elements </param>
		/// <param name="y"> full-storage vector y </param>
		virtual void Sctr(long long nz,
			void* x, long long* indx,
			void* y);

		#pragma endregion
		#pragma region SPARSE BLAS - Matrix Manipulation

		/// <summary>
		/// creates a handle for a CSR-format matrix [real-valued]
		/// </summary>
		/// <param name="handle"> handle containing internal data for subsequent 
		/// Inspector-executor Sparse BLAS operations </param>
		/// <param name="indexing"> indicates how input arrays are indexed </param>
		/// <param name="rows"> number of rows of the matrix </param>
		/// <param name="cols"> number of columns of the matrix </param>
		/// <param name="rows_start"> pointer to the starting indices of rows </param>
		/// <param name="rows_end"> pointer to the ending indices of rows </param>
		/// <param name="col_indx"> pointer to the non-zero column indices </param>
		/// <param name="values"> pointer to the non-zero values </param>
		/// <returns> value indicating whether the operation was successful or not </returns>
		virtual sparse_status_t SparseCreateCSR(sparse_matrix_t* handle, 
			sparse_index_base_t indexing, long long rows, long long cols, 
			long long* rows_start, long long* rows_end, long long* col_indx, 
			double* values);

		/// <summary>
		/// creates a handle for a CSR-format matrix [complex-valued]
		/// </summary>
		/// <param name="handle"> handle containing internal data for subsequent 
		/// Inspector-executor Sparse BLAS operations </param>
		/// <param name="indexing"> indicates how input arrays are indexed </param>
		/// <param name="rows"> number of rows of the matrix </param>
		/// <param name="cols"> number of columns of the matrix </param>
		/// <param name="rows_start"> pointer to the starting indices of rows </param>
		/// <param name="rows_end"> pointer to the ending indices of rows </param>
		/// <param name="col_indx"> pointer to the non-zero column indices </param>
		/// <param name="values"> pointer to the non-zero values </param>
		/// <returns> value indicating whether the operation was successful or not </returns>
		virtual sparse_status_t SparseCreateCSR(sparse_matrix_t* handle,
			sparse_index_base_t indexing, long long rows, long long cols,
			long long* rows_start, long long* rows_end, long long* col_indx,
			void* values);

		/// <summary>
		/// creates a handle for a CSC-format matrix [real-valued]
		/// </summary>
		/// <param name="handle"> handle containing internal data for subsequent 
		/// Inspector-executor Sparse BLAS operations </param>
		/// /// <param name="indexing"> indicates how input arrays are indexed </param>
		/// <param name="rows"> number of rows of the matrix </param>
		/// <param name="cols"> number of columns of the matrix </param>
		/// <param name="cols_start"> pointer to the starting indices of columns </param>
		/// <param name="cols_end"> pointer to the ending indices of columns </param>
		/// <param name="row_indx"> pointer to the non-zero row indices </param>
		/// <param name="values"> pointer to the non-zero values </param>
		/// <returns> value indicating whether the operation was successful or not </returns>
		virtual sparse_status_t SparseCreateCSC(sparse_matrix_t* handle,
			sparse_index_base_t indexing, long long rows, long long cols, 
			long long* cols_start, long long* cols_end, long long* row_indx, 
			double* values);

		/// <summary>
		/// creates a handle for a CSC-format matrix [complex-valued]
		/// </summary>
		/// <param name="handle"> handle containing internal data for subsequent 
		/// Inspector-executor Sparse BLAS operations </param>
		/// /// <param name="indexing"> indicates how input arrays are indexed </param>
		/// <param name="rows"> number of rows of the matrix </param>
		/// <param name="cols"> number of columns of the matrix </param>
		/// <param name="cols_start"> pointer to the starting indices of columns </param>
		/// <param name="cols_end"> pointer to the ending indices of columns </param>
		/// <param name="row_indx"> pointer to the non-zero row indices </param>
		/// <param name="values"> pointer to the non-zero values </param>
		/// <returns> value indicating whether the operation was successful or not </returns>
		virtual sparse_status_t SparseCreateCSC(sparse_matrix_t* handle,
			sparse_index_base_t indexing, long long rows, long long cols,
			long long* cols_start, long long* cols_end, long long* row_indx,
			void* values);

		/// <summary>
		/// creates a handle for a COO-format matrix [real-valued]
		/// </summary>
		/// <param name="handle"> handle containing internal data for subsequent 
		/// Inspector-executor Sparse BLAS operations </param>
		/// /// <param name="indexing"> indicates how input arrays are indexed </param>
		/// <param name="rows"> number of rows of the matrix </param>
		/// <param name="cols"> number of columns of the matrix </param>
		/// <param name="nnz"> number of non-zero elements </param>
		/// <param name="row_indx"> pointer to the non-zero row indices </param>
		/// <param name="col_indx"> pointer to the non-zero column indices </param>
		/// <param name="values"> pointer to the non-zero values </param>
		/// <returns> value indicating whether the operation was successful or not </returns>
		virtual sparse_status_t SparseCreateCOO(sparse_matrix_t* handle,
			sparse_index_base_t indexing, long long rows, long long cols,
			long long nnz, long long* row_indx, long long* col_indx, 
			double* values);

		/// <summary>
		/// creates a handle for a COO-format matrix [complex-valued]
		/// </summary>
		/// <param name="handle"> handle containing internal data for subsequent 
		/// Inspector-executor Sparse BLAS operations </param>
		/// /// <param name="indexing"> indicates how input arrays are indexed </param>
		/// <param name="rows"> number of rows of the matrix </param>
		/// <param name="cols"> number of columns of the matrix </param>
		/// <param name="nnz"> number of non-zero elements </param>
		/// <param name="row_indx"> pointer to the non-zero row indices </param>
		/// <param name="col_indx"> pointer to the non-zero column indices </param>
		/// <param name="values"> pointer to the non-zero values </param>
		/// <returns> value indicating whether the operation was successful or not </returns>
		virtual sparse_status_t SparseCreateCOO(sparse_matrix_t* handle,
			sparse_index_base_t indexing, long long rows, long long cols,
			long long nnz, long long* row_indx, long long* col_indx,
			void* values);

		/// <summary>
		/// creates a copy of a matrix handle
		/// </summary>
		/// <param name="source"> source sparse matrix to copy </param>
		/// <param name="descr"> structure specifying sparse matrix properties </param>
		/// <param name="dest"> handle containing the copied sparse matrix </param>
		/// <returns> value indicating whether the operation was successful or not </returns>
		virtual sparse_status_t SparseCopy(sparse_matrix_t source,
			matrix_descr descr, sparse_matrix_t* dest);

		/// <summary>
		/// frees memory allocated for matrix handle
		/// </summary>
		/// <param name="a"> handle containing the sparse matrix </param>
		/// <returns></returns>
		virtual sparse_status_t SparseDestroy(sparse_matrix_t a);

		/// <summary>
		/// converts internal matrix representation to CSR format
		/// </summary>
		/// <param name="source"> source sparse matrix to convert </param>
		/// <param name="operation"> specifies operation on source matrix </param>
		/// <param name="dest"> handle containing the converted sparse matrix </param>
		/// <returns> value indicating whether the operation was successful or not </returns>
		virtual sparse_status_t SparseConvert2CSR(sparse_matrix_t source,
			sparse_operation_t operation, sparse_matrix_t* dest);

		/// <summary>
		/// exports CSR matrix from internal representation
		/// </summary>
		/// <param name="source"> handle containing the sparse matrix </param>
		/// <param name="indexing"> indicates how input arrays are indexed </param>
		/// <param name="rows"> number of rows of the matrix </param>
		/// <param name="cols"> number of columns of the matrix </param>
		/// <param name="rows_start"> pointer to the starting indices of rows </param>
		/// <param name="rows_end"> pointer to the ending indices of rows </param>
		/// <param name="col_indx"> pointer to the non-zero column indices </param>
		/// <param name="values"> pointer to the non-zero values </param>
		/// <returns> value indicating whether the operation was successful or not </returns>
		virtual sparse_status_t SparseExportCSR(sparse_matrix_t source,
			sparse_index_base_t* indexing, long long* rows, long long* cols, 
			long long** rows_start, long long** rows_end, long long** col_indx, 
			double** values);

		/// <summary>
		/// exports CSC matrix from internal representation
		/// </summary>
		/// <param name="source"> handle containing the sparse matrix </param>
		/// <param name="indexing"> indicates how input arrays are indexed </param>
		/// <param name="rows"> number of rows of the matrix </param>
		/// <param name="cols"> number of columns of the matrix </param>
		/// <param name="cols_start"> pointer to the starting indices of columns </param>
		/// <param name="cols_end"> pointer to the ending indices of columns </param>
		/// <param name="row_indx"> pointer to the non-zero row indices </param>
		/// <param name="values"> pointer to the non-zero values </param>
		/// <returns> value indicating whether the operation was successful or not </returns>
		virtual sparse_status_t SparseExportCSC(sparse_matrix_t source,
			sparse_index_base_t* indexing, long long* rows, long long* cols, 
			long long** cols_start, long long** cols_end, long long** row_indx, 
			double** values);

		/// <summary>
		/// changes a single element value in a sparse matrix [real-valued]
		/// in its internal representation.
		/// </summary>
		/// <param name="a"> handle containing the sparse matrix </param>
		/// <param name="row"> row index </param>
		/// <param name="col"> column index </param>
		/// <param name="value"> target value </param>
		/// <returns> value indicating whether the operation was successful or not </returns>
		virtual sparse_status_t SparseSetValue(sparse_matrix_t a,
			long long row, long long col, double value);

		/// <summary>
		/// changes a single element value in a sparse matrix [complex-valued]
		/// in its internal representation.
		/// </summary>
		/// <param name="a"> handle containing the sparse matrix </param>
		/// <param name="row"> row index </param>
		/// <param name="col"> column index </param>
		/// <param name="value"> target value </param>
		/// <returns> value indicating whether the operation was successful or not </returns>
		virtual sparse_status_t SparseSetValue(sparse_matrix_t a,
			long long row, long long col, WComplex^ value);

		/// <summary>
		/// changes all or selected elements values in a sparse matrix [real-valued]
		/// in its internal representation
		/// </summary>
		/// <param name="a"> handle containing the sparse matrix </param>
		/// <param name="nvalues"> number of values changed </param>
		/// <param name="row_indx"> pointer to the row indices of the new values </param>
		/// <param name="col_indx"> pointer to the column indices of the new values </param>
		/// <param name="values"> pointer to the new values </param>
		/// <returns> value indicating whether the operation was successful or not </returns>
		virtual sparse_status_t SparseUpdateValues(sparse_matrix_t a,
			long long nvalues, long long* row_indx, long long* col_indx, 
			double* values);

		/// <summary>
		/// changes all or selected elements values in a sparse matrix [complex-valued]
		/// in its internal representation
		/// </summary>
		/// <param name="a"> handle containing the sparse matrix </param>
		/// <param name="nvalues"> number of values changed </param>
		/// <param name="row_indx"> pointer to the row indices of the new values </param>
		/// <param name="col_indx"> pointer to the column indices of the new values </param>
		/// <param name="values"> pointer to the new values </param>
		/// <returns> value indicating whether the operation was successful or not </returns>
		virtual sparse_status_t SparseUpdateValues(sparse_matrix_t a,
			long long nvalues, long long* row_indx, long long* col_indx,
			void* values);

		#pragma endregion
		#pragma region SPARSE BLAS - Analysis Routines
		
		/// <summary>
		/// provides estimate of number and type of upcoming
		/// matrix - vector operations
		/// </summary>
		/// <param name="a"> handle containing the sparse matrix </param>
		/// <param name="operation"> specifies operation on the input matrix </param>
		/// <param name="descr"> structure specifying sparse matrix properties </param>
		/// <param name="expected_calls"> number of expected calls to execution routine </param>
		/// <returns> value indicating whether the operation was successful or not </returns>
		virtual sparse_status_t SparseSetMVHint(sparse_matrix_t a, 
			sparse_operation_t operation, matrix_descr descr, 
			long long expected_calls);

		/// <summary>
		/// provides estimate of number and type of upcoming
		/// triangular system solver operations
		/// </summary>
		/// <param name="a"> handle containing the sparse matrix </param>
		/// <param name="operation"> specifies operation on the input matrix </param>
		/// <param name="descr"> structure specifying sparse matrix properties </param>
		/// <param name="expected_calls"> number of expected calls to execution routine </param>
		/// <returns> value indicating whether the operation was successful or not </returns>
		virtual sparse_status_t SparseSetSVHint(sparse_matrix_t a,
			sparse_operation_t operation, matrix_descr descr, 
			long long expected_calls);

		/// <summary>
		/// provides estimate of number and type of upcoming
		/// matrix - matrix multiplication operations
		/// </summary>
		/// <param name="a"> handle containing the sparse matrix </param>
		/// <param name="operation"> specifies operation on the input matrix </param>
		/// <param name="descr"> structure specifying sparse matrix properties </param>
		/// <param name="layout"> specifies layout of elements </param>
		/// <param name="dense_matrix_size"> number of columns in dense matrix </param>
		/// <param name="expected_calls"> number of expected calls to execution routine </param>
		/// <returns> value indicating whether the operation was successful or not </returns>
		virtual sparse_status_t SparseSetMMHint(sparse_matrix_t a, 
			sparse_operation_t operation, matrix_descr descr, sparse_layout_t layout, 
			long long dense_matrix_size, long long expected_calls);

		/// <summary>
		/// provides estimate of number and type of upcoming
		/// triangular matrix solve with multiple right hand sides
		/// operations
		/// </summary>
		/// <param name="a"> handle containing the sparse matrix </param>
		/// <param name="operation"> specifies operation on the input matrix </param>
		/// <param name="descr"> structure specifying sparse matrix properties </param>
		/// <param name="layout"> specifies layout of elements </param>
		/// <param name="dense_matrix_size"> number of columns in dense matrix </param>
		/// <param name="expected_calls"> number of expected calls to execution routine </param>
		/// <returns> value indicating whether the operation was successful or not </returns>
		virtual sparse_status_t SparseSetSMHint(sparse_matrix_t a,
			sparse_operation_t operation, struct matrix_descr descr, sparse_layout_t layout,
			long long dense_matrix_size, long long expected_calls);

		/// <summary>
		/// provides memory requirements for performance
		/// optimization purposes
		/// </summary>
		/// <param name="a"> handle containing the sparse matrix </param>
		/// <param name="policy"> Specify memory utilization policy for 
		/// optimization routine using these types </param>
		/// <returns> value indicating whether the operation was successful or not </returns>
		virtual sparse_status_t SparseSetMemoryHint(sparse_matrix_t a,
			sparse_memory_usage_t policy);

		/// <summary>
		/// analyzes matrix structure and performs optimizations
		/// using the hints provided in the handle.
		/// </summary>
		/// <param name="a"> handle containing the sparse matrix </param>
		/// <returns> value indicating whether the operation was successful or not </returns>
		virtual sparse_status_t SparseOptimize(sparse_matrix_t a);

		#pragma endregion
		#pragma region SPARSE BLAS - Execution Routines

		/// <summary>
		/// computes a sparse matrix- vector product
		/// y := alpha*op(A)*x + beta*y
		/// </summary>
		/// <param name="operation"> specifies operation on the input matrix a </param>
		/// <param name="alpha"> scalar constant alpha </param>
		/// <param name="a"> input sparse matrix a </param>
		/// <param name="descr"> structure specifying sparse matrix properties </param>
		/// <param name="x"> vector x </param>
		/// <param name="beta"> scalar constant beta </param>
		/// <param name="y"> vector y (to be overwritten) </param>
		/// <returns> value indicating whether the operation was successful or not </returns>
		virtual sparse_status_t SparseMV(sparse_operation_t operation, 
			double alpha, sparse_matrix_t a, matrix_descr descr, 
			double* x, double beta, double* y);

		/// <summary>
		/// solves a system of linear equations for 
		/// a triangular sparse matrix
		/// op(A)*y = alpha * x
		/// </summary>
		/// <param name="operation"> specifies operation on the input matrix a </param>
		/// <param name="alpha"> scalar constant alpha </param>
		/// <param name="a"> input sparse matrix a </param>
		/// <param name="descr"> structure specifying sparse matrix properties </param>
		/// <param name="x"> vector x </param>
		/// <param name="y"> vector y (to be overwritten) </param>
		/// <returns> value indicating whether the operation was successful or not </returns>
		virtual sparse_status_t SparseTRSV(sparse_operation_t operation,
			double alpha, sparse_matrix_t a, matrix_descr descr, 
			double* x, double* y);

		/// <summary>
		/// computes the product of a sparse matrix and a dense
		/// matrix and stores the result as a dense matrix
		/// C := alpha*op(A)*B + beta*C
		/// </summary>
		/// <param name="operation"> specifies operation on the input matrix a </param>
		/// <param name="alpha"> scalar constant alpha </param>
		/// <param name="a"> input sparse matrix a </param>
		/// <param name="descr"> structure specifying sparse matrix properties </param>
		/// <param name="layout"> describes the storage scheme for the dense matrix </param>
		/// <param name="b"> dense matrix b </param>
		/// <param name="columns"> number of colums in matrix c </param>
		/// <param name="ldb"> leading dimension of matrix b. </param>
		/// <param name="beta"> scalar constant beta </param>
		/// <param name="c"> dense matrix c (to be overwrittem) </param>
		/// <param name="ldc"> leading dimension of matrix c </param>
		/// <returns> value indicating whether the operation was successful or not </returns>
		virtual sparse_status_t SparseMM(sparse_operation_t operation,
			double alpha, sparse_matrix_t a, matrix_descr descr, sparse_layout_t layout,
			double* b, long long columns, long long ldb, double beta,
			double* c, long long ldc);

		/// <summary>
		/// solves a system of linear equations with multiple 
		/// right-hand sides for a triangular sparse matrix
		/// Y := alpha*inv(op(A))*X
		/// op(A)*y = alpha * x
		/// </summary>
		/// <param name="operation"> specifies operation on the input matrix a </param>
		/// <param name="alpha"> scalar constant alpha </param>
		/// <param name="a"> input sparse matrix a </param>
		/// <param name="descr"> structure specifying sparse matrix properties </param>
		/// <param name="layout"> describes the storage scheme for the dense matrix </param>
		/// <param name="x"> dense matrix x </param>
		/// <param name="columns"> number of columns in matrix y </param>
		/// <param name="ldx"> leading dimension of matrix x </param>
		/// <param name="y"> dense matrix y (to be overwritten) </param>
		/// <param name="ldy"> leading dimension in matrix y </param>
		/// <returns> value indicating whether the operation was successful or not </returns>
		virtual sparse_status_t SparseTRSM(sparse_operation_t operation,
			double alpha, sparse_matrix_t a, matrix_descr descr, const sparse_layout_t layout,
			double* x, long long columns, long long ldx, double* y, long long ldy);

		/// <summary>
		/// computes the sum of two sparse matrices and store
		/// the result in a newly allocated sparse matrix
		/// C := alpha*op(A) + B
		/// </summary>
		/// <param name="operation"> specifies operation on the input matrix a </param>
		/// <param name="a"> input sparse matrix a </param>
		/// <param name="alpha"> scalar constant alpha </param>
		/// <param name="b"> input sparse matrix b </param>
		/// <param name="c"> result sparse matrix c </param>
		/// <returns> value indicating whether the operation was successful or not </returns>
		virtual sparse_status_t SparseAdd(sparse_operation_t operation,
			sparse_matrix_t a, double alpha, sparse_matrix_t b, 
			sparse_matrix_t* c);

		/// <summary>
		/// computes the product of two sparse matrices and store
		/// the result in a newly allocated sparse matrix
		/// C := op(A) *B
		/// </summary>
		/// <param name="operation"> specifies operation on the input matrix a </param>
		/// <param name="a"> input sparse matrix a </param>
		/// <param name="b"> input sparse matrix b </param>
		/// <param name="c"> result sparse matrix c </param>
		/// <returns> value indicating whether the operation was successful or not </returns>
		virtual sparse_status_t SparseSPMM(sparse_operation_t operation,
			sparse_matrix_t a, sparse_matrix_t b, sparse_matrix_t* c);

		/// <summary>
		/// computes the product of two sparse matrices and
		/// stores the result as a dense matrix
		/// C := op(A)*B
		/// </summary>
		/// <param name="operation"> specifies operation on the input matrix a </param>
		/// <param name="a"> input sparse matrix a </param>
		/// <param name="b"> input sparse matrix b </param>
		/// <param name="layout"> describes the storage scheme for the dense matrix </param>
		/// <param name="c"> result dense matrix c </param>
		/// <param name="ldc"> leading dimension of matrix c </param>
		/// <returns> value indicating whether the operation was successful or not </returns>
		virtual sparse_status_t SparseSPMMD(sparse_operation_t operation,
			sparse_matrix_t a, sparse_matrix_t b, sparse_layout_t layout,
			double* c, long long ldc);

		#pragma endregion
		#pragma region SPARSE BLAS - QR Routines

		/// <summary>
		/// defines the pivot strategy for further calls
		/// </summary>
		/// <param name="a"> handle containing a sparse matrix </param>
		/// <param name="hint"> value specifying whether to use pivoting </param>
		/// <returns> value indicating whether the operation was successful or not </returns>
		virtual sparse_status_t SparseSetQRHint(sparse_matrix_t a,
			sparse_qr_hint_t hint);

		/// <summary>
		/// computes the QR decomposition for the matrix of a
		/// sparse linear system and calculates the solution
		/// A*x = b, so that A = Q*R
		/// </summary>
		/// <param name="operation"> operation on the sparse matrix a </param>
		/// <param name="a"> sparse matrix a </param>
		/// <param name="descr"> structure specifying sparse matrix properties </param>
		/// <param name="layout"> storage scheme for the dense matrix </param>
		/// <param name="columns"> number of columns in matrix b </param>
		/// <param name="x"> dense matrix x (to be overwritten) </param>
		/// <param name="ldx"> leading dimension of matrix x </param>
		/// <param name="b"> dense matrix b </param>
		/// <param name="ldb"> leading dimension of matrix b </param>
		/// <returns> value indicating whether the operation was successful or not </returns>
		virtual sparse_status_t SparseQR(sparse_operation_t operation,
			sparse_matrix_t a, matrix_descr descr,
			sparse_layout_t layout, long long columns, double* x,
			long long ldx, double* b, long long ldb);

		/// <summary>
		/// reordering step of SPARSE QR solver
		/// </summary>
		/// <param name="a"> sparse matrix a </param>
		/// <param name="descr"> structure specifying sparse matrix properties </param>
		/// <returns> value indicating whether the operation was successful or not </returns>
		virtual sparse_status_t SparseQRReorder(sparse_matrix_t a,
			matrix_descr descr);

		/// <summary>
		/// factorization step of the SPARSE QR solver
		/// </summary>
		/// <param name="a"> sparse matrix a </param>
		/// <param name="alt_values"> alternative values </param>
		/// <returns> value indicating whether the operation was successful or not </returns>
		virtual sparse_status_t SparseQRFactorize(sparse_matrix_t a,
			double* alt_values);

		/// <summary>
		/// solving step of the SPARSE QR solver
		/// </summary>
		/// <param name="operation"> operation on the sparse matrix a </param>
		/// <param name="a"> sparse matrix a </param>
		/// <param name="alt_values"> alternative values </param>
		/// <param name="layout"> storage scheme for the dense matrix </param>
		/// <param name="columns"> number of columns in matrix b </param>
		/// <param name="x"> dense matrix x </param>
		/// <param name="ldx"> leading dimension of matrix x </param>
		/// <param name="b"> dense matrix b </param>
		/// <param name="ldb"> leading dimension of matrix b </param>
		/// <returns> value indicating whether the operation was successful or not </returns>
		virtual sparse_status_t SparseQRSolve(sparse_operation_t operation,
			sparse_matrix_t a, double* alt_values, 
			sparse_layout_t layout, long long columns, double* x, long long ldx,
			double* b, long long ldb);

		/// <summary>
		/// first stage of the solving step of the SPARSE QR solver
		/// </summary>
		/// <param name="operation"> operation on the sparse matrix a </param>
		/// <param name="a"> sparse matrix a </param>
		/// <param name="layout"> storage scheme for the dense matrix </param>
		/// <param name="columns"> number of columns in matrix b </param>
		/// <param name="x"> dense matrix x </param>
		/// <param name="ldx"> leading dimension of matrix x </param>
		/// <param name="b"> dense matrix b </param>
		/// <param name="ldb"> leading dimension of matrix b </param>
		/// <returns> value indicating whether the operation was successful or not </returns>
		virtual sparse_status_t SparseQRQmult(sparse_operation_t operation,
			sparse_matrix_t a, sparse_layout_t layout, long long columns, 
			double* x, long long ldx, double* b, long long ldb);

		/// <summary>
		/// second stage of the solving step of the SPARSE QR solver
		/// </summary>
		/// <param name="operation"> operation on the sparse matrix a </param>
		/// <param name="a"> sparse matrix a </param>
		/// <param name="layout"> storage scheme for the dense matrix </param>
		/// <param name="columns"> number of columns in matrix b </param>
		/// <param name="x"> dense matrix x </param>
		/// <param name="ldx"> leading dimension of matrix x </param>
		/// <param name="b"> dense matrix b </param>
		/// <param name="ldb"> leading dimension of matrix b </param>
		/// <returns> value indicating whether the operation was successful or not </returns>
		virtual sparse_status_t SparseQRRsolve(sparse_operation_t operation,
			sparse_matrix_t a, sparse_layout_t layout, long long columns, 
			double* x, long long ldx, double* b, long long ldb);

		#pragma endregion

	};


	/// <summary>
	/// IntelMKL class
	/// </summary>
	public ref class IntelMKL : public IBLAS
	{
	public:

		IntelMKL();

		#pragma region BLAS Level-1

		double Asum(long long n,
			double* x, long long incx) override;

		double Asum(long long n,
			void* x, long long incx) override;

		void Axpy(long long n,
			double alpha, double* x, long long incx,
			double* y, long long incy) override;

		void Axpy(long long n,
			void* alpha, void* x, long long incx,
			void* y, long long incy) override;

		void Copy(long long n,
			double* x, long long incx,
			double* y, long long incy) override;

		void Copy(long long n,
			void* x, long long incx,
			void* y, long long incy) override;

		double Dot(long long n,
			double* x, long long incx,
			double* y, long long incy) override;

		void Dotc(long long n,
			void* x, long long incx,
			void* y, long long incy, 
			void* dotc) override;

		void Dotu(long long n,
			void* x, long long incx,
			void* y, long long incy, 
			void* dotu) override;

		double Nrm2(long long n,
			double* x, long long incx) override;

		double Nrm2(long long n,
			void* x, long long incx)override;

		void Scal(long long n, double alpha,
			double* x, long long incx) override;

		void Scal(long long n, void* alpha,
			void* x, long long incx) override;

		void Scal(long long n, double alpha,
			void* x, long long incx) override;

		void Swap(long long n,
			double* x, long long incx,
			double* y, long long incy) override;

		void Swap(long long n,
			void* x, long long incx,
			void* y, long long incy) override;

		long long Iamax(long long n,
			double* x, long long incx) override;

		long long Iamax(long long n,
			void* x, long long incx) override;

		long long Iamin(long long n,
			double* x, long long incx) override;

		long long Iamin(long long n,
			void* x, long long incx) override;

		void Rot(long long n,
			double* x, long long incx,
			double* y, long long incy,
			double c, double s) override;

		void Rot(long long n,
			void* x, long long incx,
			void* y, long long incy,
			double c, void* s) override;

		void Rot(long long n,
			void* x, long long incx,
			void* y, long long incy,
			double c, double s) override;

		double Cabs(void* z) override;

		#pragma endregion
		#pragma region BLAS Level-2

		void Gemv(CBLAS_LAYOUT layout, CBLAS_TRANSPOSE operation,
			long long m, long long n,
			double alpha, double* a, long long lda,
			double* x, long long incx,
			double beta, double* y, long long incy) override;

		void Gemv(CBLAS_LAYOUT layout, CBLAS_TRANSPOSE operation,
			long long m, long long n,
			MKL_Complex16 alpha, MKL_Complex16* a, long long lda,
			MKL_Complex16* x, long long incx,
			MKL_Complex16 beta, MKL_Complex16* y, long long incy) override;

		#pragma endregion
		#pragma region BLAS Level-3

		void Gemm(CBLAS_LAYOUT layout,
			CBLAS_TRANSPOSE operationa, CBLAS_TRANSPOSE operationb,
			long long m, long long n, long long k,
			double alpha, double* a, long long lda,
			double* b, long long ldb,
			double beta, double* c, long long ldc) override;

		void Gemm(CBLAS_LAYOUT layout,
			CBLAS_TRANSPOSE operationa, CBLAS_TRANSPOSE operationb,
			long long m, long long n, long long k,
			MKL_Complex16 alpha, MKL_Complex16* a, long long lda,
			MKL_Complex16* b, long long ldb,
			MKL_Complex16 beta, MKL_Complex16* c, long long ldc) override;

		#pragma endregion
		#pragma region SPARSE BLAS Level-1

		void Axpyi(long long nz,
			double alpha, double* x,
			long long* indx, double* y) override;

		void Axpyi(long long nz,
			void* alpha, void* x,
			long long* indx, void* y) override;

		double Doti(long long nz,
			double* x, long long* indx,
			double* y) override;

		void Dotci(long long nz,
			void* x, long long* indx,
			void* y, void* dotc) override;

		void Dotui(long long nz,
			void* x, long long* indx,
			void* y, void* dotu) override;

		void Gthr(long long nz,
			double* y,
			double* x, long long* indx) override;

		void Gthr(long long nz,
			void* y, void* x, long long* indx) override;

		void Gthrz(long long nz,
			double* y, double* x, long long* indx) override;

		void Gthrz(long long nz,
			void* y, void* x, long long* indx) override;

		void Roti(long long nz, double* x, long long* indx,
			double* y, double c, double s) override;

		void Sctr(long long nz,
			double* x, long long* indx,
			double* y) override;

		void Sctr(long long nz,
			void* x, long long* indx,
			void* y) override;

		#pragma endregion
		#pragma region SPARSE BLAS - Maxtrix Manipulation

		sparse_status_t SparseCreateCSR(sparse_matrix_t* handle,
			sparse_index_base_t indexing, long long rows, long long cols,
			long long* rows_start, long long* rows_end, long long* col_indx, 
			double* values) override;

		sparse_status_t SparseCreateCSR(sparse_matrix_t* handle,
			sparse_index_base_t indexing, long long rows, long long cols,
			long long* rows_start, long long* rows_end, long long* col_indx,
			void* values) override;

		sparse_status_t SparseCreateCSC(sparse_matrix_t* handle,
			sparse_index_base_t indexing, long long rows, long long cols,
			long long* cols_start, long long* cols_end, long long* row_indx,
			double* values) override;

		sparse_status_t SparseCreateCSC(sparse_matrix_t* handle,
			sparse_index_base_t indexing, long long rows, long long cols,
			long long* cols_start, long long* cols_end, long long* row_indx,
			void* values) override;

		sparse_status_t SparseCreateCOO(sparse_matrix_t* handle,
			sparse_index_base_t indexing, long long rows, long long cols,
			long long nnz, long long* row_indx, long long* col_indx,
			double* values) override;

		sparse_status_t SparseCreateCOO(sparse_matrix_t* handle,
			sparse_index_base_t indexing, long long rows, long long cols,
			long long nnz, long long* row_indx, long long* col_indx,
			void* values) override;

		sparse_status_t SparseCopy(sparse_matrix_t source,
			matrix_descr descr, sparse_matrix_t* dest) override;

		sparse_status_t SparseDestroy(sparse_matrix_t a) override;

		sparse_status_t SparseConvert2CSR(sparse_matrix_t source,
			sparse_operation_t operation, sparse_matrix_t* dest) override;

		sparse_status_t SparseExportCSR(sparse_matrix_t source,
			sparse_index_base_t* indexing, long long* rows, long long* cols,
			long long** rows_start, long long** rows_end, long long** col_indx,
			double** values) override;

		sparse_status_t SparseExportCSC(sparse_matrix_t source,
			sparse_index_base_t* indexing, long long* rows, long long* cols,
			long long** cols_start, long long** cols_end, long long** row_indx,
			double** values) override;

		sparse_status_t SparseSetValue(sparse_matrix_t a,
			long long row, long long col, double value) override;

		sparse_status_t SparseSetValue(sparse_matrix_t a,
			long long row, long long col, WComplex^ value) override;

		sparse_status_t SparseUpdateValues(sparse_matrix_t a,
			long long nvalues, long long* row_indx, long long* col_indx,
			double* values) override;

		sparse_status_t SparseUpdateValues(sparse_matrix_t a,
			long long nvalues, long long* row_indx, long long* col_indx,
			void* values) override;

		#pragma endregion
		#pragma region SPARSE BLAS - Analysis Routines
		
		sparse_status_t SparseSetMVHint(sparse_matrix_t a,
			sparse_operation_t operation, matrix_descr descr,
			long long expected_calls) override;

		sparse_status_t SparseSetSVHint(sparse_matrix_t a,
			sparse_operation_t operation, matrix_descr descr,
			long long expected_calls) override;

		sparse_status_t SparseSetMMHint(sparse_matrix_t a,
			sparse_operation_t operation, matrix_descr descr, sparse_layout_t layout,
			long long dense_matrix_size, long long expected_calls) override;

		sparse_status_t SparseSetSMHint(sparse_matrix_t a,
			sparse_operation_t operation, struct matrix_descr descr, sparse_layout_t layout,
			long long dense_matrix_size, long long expected_calls) override;

		sparse_status_t SparseSetMemoryHint(sparse_matrix_t a,
			sparse_memory_usage_t policy) override;

		sparse_status_t SparseOptimize(sparse_matrix_t a) override;

		#pragma endregion
		#pragma region SPARSE BLAS - Execution Routines

		sparse_status_t SparseMV(sparse_operation_t operation,
			double alpha, sparse_matrix_t a, matrix_descr descr,
			double* x, double beta, double* y) override;

		sparse_status_t SparseTRSV(sparse_operation_t operation,
			double alpha, sparse_matrix_t a, matrix_descr descr,
			double* x, double* y) override;

		sparse_status_t SparseMM(sparse_operation_t operation,
			double alpha, sparse_matrix_t a, matrix_descr descr, sparse_layout_t layout,
			double* b, long long columns, long long ldb, double beta,
			double* c, long long ldc) override;

		sparse_status_t SparseTRSM(sparse_operation_t operation,
			double alpha, sparse_matrix_t a, matrix_descr descr, const sparse_layout_t layout,
			double* x, long long columns, long long ldx, double* y, long long ldy) override;

		sparse_status_t SparseAdd(sparse_operation_t operation,
			sparse_matrix_t a, double alpha, sparse_matrix_t b,
			sparse_matrix_t* c) override;

		sparse_status_t SparseSPMM(sparse_operation_t operation,
			sparse_matrix_t a, sparse_matrix_t b, sparse_matrix_t* c) override;

		sparse_status_t SparseSPMMD(sparse_operation_t operation,
			sparse_matrix_t a, sparse_matrix_t b, sparse_layout_t layout,
			double* c, long long ldc) override;

		#pragma endregion
		#pragma region SPARSE BLAS - QR Routines

		sparse_status_t SparseSetQRHint(sparse_matrix_t a, 
			sparse_qr_hint_t hint) override;

		sparse_status_t SparseQR(sparse_operation_t operation,
			sparse_matrix_t a, matrix_descr descr,
			sparse_layout_t layout, long long columns, double* x,
			long long ldx, double* b, long long ldb) override;
		
		sparse_status_t SparseQRReorder(sparse_matrix_t a,
			matrix_descr descr) override;

		sparse_status_t SparseQRFactorize(sparse_matrix_t a,
			double* alt_values) override;

		sparse_status_t SparseQRSolve(sparse_operation_t operation,
			sparse_matrix_t a, double* alt_values,
			sparse_layout_t layout, long long columns, double* x, long long ldx,
			const double* b, long long ldb) override;

		sparse_status_t SparseQRQmult(sparse_operation_t operation,
			sparse_matrix_t a, sparse_layout_t layout, long long columns,
			double* x, long long ldx, const double* b, long long ldb) override;

		sparse_status_t SparseQRRsolve(sparse_operation_t operation,
			sparse_matrix_t a, sparse_layout_t layout, long long columns,
			double* x, long long ldx, double* b, long long ldb) override;

		#pragma endregion


	};


	/// <summary>
	/// linear algebra factory
	/// </summary>
	public ref class LinAlgFactory
	{
	public:
		IBLAS^ iBLAS;
		//ILAPACK^ iLAPACK
		//IVMF^ iVMF
		//IFFT^ iFFT

		LinAlgFactory()
		{
			// here defines which libraries to use
			iBLAS = gcnew IntelMKL();
			//iLAPACK ...
		}
		~LinAlgFactory(){ }
	};

	/// <summary>
	/// linear algebra class
	/// </summary>
	public ref class WLinAlg
	{

	private:
		static LinAlgFactory^ factory = gcnew LinAlgFactory();

	public:
		
		/// <summary>
		/// default constructor
		/// </summary>
		WLinAlg();

		#pragma region methods: auxillary

		/// <summary>
		/// checks whether the lengths of two vectors are equal
		/// </summary>
		/// <param name="x"> full-storage vector x </param>
		/// <param name="y"> full-storage vector y </param>
		/// <returns> whether the lengths are equal </returns>
		static bool AreVectorLengthsEqual(WVectorD^ x, WVectorD^ y);

		/// <summary>
		/// checks whether the lengths of two vectors are equal
		/// </summary>
		/// <param name="x"> full-storage vector x </param>
		/// <param name="y"> full-storage vector y </param>
		/// <returns> whether the lengths are equal </returns>
		static bool AreVectorLengthsEqual(WVectorZ^ x, WVectorZ^ y);

		/// <summary>
		/// checks whether the lengths of two vectors are equal
		/// </summary>
		/// <param name="x"> compressed sparse vector x </param>
		/// <param name="y"> full-storage vector y </param>
		/// <returns> whether the lengths are equal </returns>
		static bool AreVectorLengthsEqual(WVectorDi^ x, WVectorD^ y);

		/// <summary>
		/// checks whether the lengths of two vectors are equal
		/// </summary>
		/// <param name="x"> compressed sparse vector x </param>
		/// <param name="y"> compressed sparse vector y </param>
		/// <returns> whether the lengths are equal </returns>
		static bool AreVectorLengthsEqual(WVectorDi^ x, WVectorDi^ y);

		/// <summary>
		/// check whether the matrix size and vector length
		/// are valid for dot product
		/// </summary>
		/// <param name="a"> full-storage matrix a </param>
		/// <param name="x"> full-storage vector x </param>
		/// <returns> whether dot-product is possible </returns>
		static bool AreMatrixVectorDottable(WMatrixD^ a, WVectorD^ x);

		/// <summary>
		/// check whether the matrix size and vector length
		/// are valid for dot product
		/// </summary>
		/// <param name="a"> full-storage matrix a </param>
		/// <param name="x"> full-storage vector x </param>
		/// <returns> whether dot-product is possible </returns>
		static bool AreMatrixVectorDottable(WMatrixZ^ a, WVectorZ^ x);

		/// <summary>
		/// checks whether the sizes of the matrices 
		/// are valid for dot product
		/// </summary>
		/// <param name="a"> full-storage matrix a </param>
		/// <param name="b"> full-storage matrix b </param>
		/// <returns> whether dot-product is possible </returns>
		static bool AreMatrixMatrixDottable(WMatrixD^ a, WMatrixD^ b);

		/// <summary>
		/// checks whether the sizes of the matrices 
		/// are valid for dot product
		/// </summary>
		/// <param name="a"> full-storage matrix a </param>
		/// <param name="b"> full-storage matrix b </param>
		/// <returns> whether dot-product is possible </returns>
		static bool AreMatrixMatrixDottable(WMatrixZ^ a, WMatrixZ^ b);

		#pragma endregion
		#pragma region methods: BLAS Level-1 

		/// <summary>
		/// computes the sum of magnitudes of all the elements
		/// of a full-storage vector
		/// </summary>
		/// <param name="x"> full-storage vector x </param>
		/// <returns> sum of magnitudes of all elements </returns>
		static double AbsoluteSum(WVectorD^ x);

		/// <summary>
		/// computes the sum of magnitudes of all the elements
		/// of a full-storage vector
		/// </summary>
		/// <param name="x"> full-storage vector x </param>
		/// <returns> sum of magnitudes of all elements </returns>
		static double AbsoluteSum(WVectorZ^ x);

		/// <summary>
		/// adds vector x into another vector y
		/// y := x + y
		/// </summary>
		/// <param name="x"> vector x </param>
		/// <param name="y"> vector y [will be updated] </param>
		static void AddTo(WVectorD^ x, WVectorD^ y);

		/// <summary>
		/// adds vector x into another vector y
		/// y := x + y
		/// </summary>
		/// <param name="x"> vector x </param>
		/// <param name="y"> vector y [will be updated] </param>
		static void AddTo(WVectorZ^ x, WVectorZ^ y);

		/// <summary>
		/// adds a scalar multiple of a vector x
		/// into another vector y
		/// y := alpha * x + y
		/// </summary>
		/// <param name="alpha"> scalar multiple </param>
		/// <param name="x"> vector x </param>
		/// <param name="y"> vector y [will be updated] </param>
		static void AddTo(double alpha,
			WVectorD^ x, WVectorD^ y);

		/// <summary>
		/// adds a scalar multiple of a vector x
		/// into another vector y
		/// y := alpha * x + y
		/// </summary>
		/// <param name="alpha"> scalar multiple </param>
		/// <param name="x"> vector x </param>
		/// <param name="y"> vector y [will be updated] </param>
		static void AddTo(WComplex alpha,
			WVectorZ^ x, WVectorZ^ y);

		/// <summary>
		/// copies full-storage vector x to vector y
		/// </summary>
		/// <param name="x"> full-storage vector x </param>
		/// <param name="y"> full-storage vector y </param>
		static void Copy(WVectorD^ x, WVectorD^ y);

		/// <summary>
		/// copies full-storage vector x to vector y
		/// </summary>
		/// <param name="x"> full-storage vector x </param>
		/// <param name="y"> full-storage vector y </param>
		static void Copy(WVectorZ^ x, WVectorZ^ y);

		/// <summary>
		/// computes the dot product between vector x and y
		/// </summary>
		/// <param name="x"> vector x </param>
		/// <param name="y"> vector y </param>
		/// <returns> dot product </returns>
		static double Dot(WVectorD^ x, WVectorD^ y);

		/// <summary>
		/// computes the dot product between vector x and y
		/// </summary>
		/// <param name="x"> vector x </param>
		/// <param name="y"> vector y </param>
		/// <returns> dot product </returns>
		static WComplex^ Dot(WVectorZ^ x, WVectorZ^ y);

		/// <summary>
		/// computes the dot product of a conjugated vector x
		/// with another vector y
		/// </summary>
		/// <param name="x"> vector x (to be conjugated) </param>
		/// <param name="y"> vector y </param>
		/// <returns> dot product </returns>
		static WComplex^ Dotc(WVectorZ^ x, WVectorZ^ y);

		/// <summary>
		/// computes the Euclidean norm of a vector
		/// res = || x ||
		/// </summary>
		/// <param name="x"> full-storage vector x </param>
		/// <returns> Euclidean norm </returns>
		static double Norm(WVectorD^ x);

		/// <summary>
		/// computes the Euclidean norm of a vector
		/// res = || x ||
		/// </summary>
		/// <param name="x"> full-storage vector x </param>
		/// <returns> Euclidean norm </returns>
		static double Norm(WVectorZ^ x);

		/// <summary>
		/// computes the product of a vector by a scalar
		/// </summary>
		/// <param name="alpha"> scalar multiple </param>
		/// <param name="x"> full-storage vector x </param>
		static void Scale(double alpha, WVectorD^ x);

		/// <summary>
		/// computes the product of a vector by a scalar
		/// </summary>
		/// <param name="alpha"> scalar multiple </param>
		/// <param name="x"> full-storage vector x </param>
		static void Scale(WComplex alpha, WVectorZ^ x);

		/// <summary>
		/// finds the index of the element in the vector
		/// with maximum absolute value
		/// </summary>
		/// <param name="x"> full-storage vector x </param>
		/// <returns> index of the element </returns>
		static long long IndexMaxAbsolute(WVectorD^ x);

		/// <summary>
		/// finds the index of the element in the vector
		/// with maximum absolute value
		/// </summary>
		/// <param name="x"> full-storage vector x </param>
		/// <returns> index of the element </returns>
		static long long IndexMaxAbsolute(WVectorZ^ x);

		/// <summary>
		/// finds the index of the element in the vector
		/// with minimum absolute value
		/// </summary>
		/// <param name="x"> full-storage vector x </param>
		/// <returns> index of the element </returns>
		static long long IndexMinAbsolute(WVectorD^ x);

		/// <summary>
		/// finds the index of the element in the vector
		/// with minimum absolute value
		/// </summary>
		/// <param name="x"> full-storage vector x </param>
		/// <returns> index of the element </returns>
		static long long IndexMinAbsolute(WVectorZ^ x);

		/// <summary>
		/// performs rotation of point in the plane
		/// xi = c * xi + s * yi
		/// yi = c * yi - s * xi
		/// </summary>
		/// <param name="x"> full-storage vector x </param>
		/// <param name="y"> full-storage vector y </param>
		/// <param name="c"> scalar parameter </param>
		/// <param name="s"> scalar parameter </param>
		static void Rotate(WVectorD^ x, WVectorD^ y, 
			double c, double s);

		/// <summary>
		/// performs rotation of point in the plane
		/// xi = c * xi + s * yi
		/// yi = c * yi - s * xi
		/// </summary>
		/// <param name="x"> full-storage vector x </param>
		/// <param name="y"> full-storage vector y </param>
		/// <param name="c"> scalar parameter </param>
		/// <param name="s"> scalar parameter </param>
		static void Rotate(WVectorZ^ x, WVectorZ^ y,
			double c, double s);

		#pragma endregion
		#pragma region methods: BLAS Level-2

		/// <summary>
		/// computes the matrix - vector product
		/// y := alpha * a * x + beta * y		
		/// </summary>
		/// <param name="alpha"> scalar parameter </param>
		/// <param name="a"> matrix a </param>
		/// <param name="x"> vector x </param>
		/// <param name="beta"> scalar parameter </param>
		/// <param name="y"> vector y [will be updated]</param>
		static void Dot(double alpha, WMatrixD^ a, WVectorD^ x, 
			double beta, WVectorD^ y);

		/// <summary>
		/// computes the matrix - vector product
		/// y = a * x
		/// </summary>
		/// <param name="a"> matrix a </param>
		/// <param name="x"> vector x </param>
		/// <returns> result vector y </returns>
		static WVectorD^ Dot(WMatrixD^ a, WVectorD^ x);

		#pragma endregion
		#pragma region methods: BLAS Level-3

		/// <summary>
		/// computes a matrix-matrix product
		/// m := alpha * a * b + beta * c
		/// </summary>
		/// <param name="alpha"> scalar parameter </param>
		/// <param name="a"> matrix a </param>
		/// <param name="b"> matrix b </param>
		/// <param name="beta"> scalar parameter </param>
		/// <param name="c"> matrix c [will be updated]</param>
		static void Dot(double alpha, WMatrixD^ a, WMatrixD^ b,
			double beta, WMatrixD^ c);

		/// <summary>
		/// computes a matrix-matrix product
		/// m = a * b
		/// </summary>
		/// <param name="a"> matrix a </param>
		/// <param name="b"> matirx b </param>
		/// <returns> result matrix m </returns>
		static WMatrixD^ Dot(WMatrixD^ a, WMatrixD^ b);

		#pragma endregion

		#pragma region methods: SPARSE BLAS Level-1

		/// <summary>
		/// Adds a scalar multiple of compressed sparse vector
		/// to a full-storage vector
		/// y := a*x + y
		/// </summary>
		/// <param name="x"> input sparse vector x </param>
		/// <param name="y"> dense vector y (to be overwritten) </param>
		/// <param name="alpha"> scalar constant alpha </param>
		static void Axpyi(WVectorDi^ xi, double* y,
			double alpha);

		/// <summary>
		/// Adds a scalar multiple of compressed sparse vector
		/// to a full-storage vector
		/// y := a*x + y
		/// </summary>
		/// <param name="x"> input sparse vector x </param>
		/// <param name="y"> dense vector y (to be overwritten) </param>
		/// <param name="alpha"> scalar constant alpha </param>
		static void Axpyi(WVectorZi^ xi, void* y,
			WComplex^ alpha);

		/// <summary>
		/// computes the dot product of a compressed sparse vector x
		/// by a full-storage vector y
		/// res = x[0]*y[indx[0]] + x[1]*y[indx[1]] +...+ x[nz-1]*y[indx[nz-1]]
		/// </summary>
		/// <param name="xi"> input sparse vector x </param>
		/// <param name="y"> input dense vector y </param>
		/// <returns> dot-product </returns>
		static double Doti(WVectorDi^ xi, double* y);

		/// <summary>
		/// computes the dot product of a compressed sparse vector x
		/// by a full-storage vector y
		/// res = x[0]*y[indx[0]] + x[1]*y[indx[1]] +...+ x[nz-1]*y[indx[nz-1]]
		/// </summary>
		/// <param name="xi"> input sparse vector x </param>
		/// <param name="y"> input dense vector y </param>
		/// <returns> dot-product </returns>
		static WComplex^ Doti(WVectorZi^ xi, void* y);

		/// <summary>
		/// computes the dot product of a conjugated compressed 
		/// sparse vector x by a full-storage vector y
		/// </summary>
		/// <param name="xi"> input sparse vector x (to be conjugated) </param>
		/// <param name="y"> input dense vector y </param>
		/// <returns> dot-product </returns>
		static WComplex^ Dotci(WVectorZi^ xi, void* y);

		/// <summary>
		/// gathers a full-storage sparse vector's elements 
		/// into compressed form
		/// x[i] = y[indx[i]], for i=0,1,... ,nz-1
		/// </summary>
		/// <param name="n"> totol elements in the dense vector y </param>
		/// <param name="y"> input dense vector y </param>
		/// <param name="nnz"> number of non-zero elements </param>
		/// <param name="indx"> indices of non-zero elements in y </param>
		/// <returns> result sparse vector x </returns>
		static WVectorDi^ Gthr(long long n, double* y, long long nnz, long long* indx);

		/// <summary>
		/// gathers a full-storage sparse vector's elements 
		/// into compressed form
		/// x[i] = y[indx[i]], for i=0,1,... ,nz-1
		/// </summary>
		/// <param name="n"> totol elements in the dense vector y </param>
		/// <param name="y"> input dense vector y </param>
		/// <param name="nnz"> number of non-zero elements </param>
		/// <param name="x"> output sparse vector x </param>
		/// <param name="indx"> indices of non-zero elements in y </param>
		static void Gthr(long long n, double* y, long long nnz, double* x, long long* indx);

		/// <summary>
		/// gathers a full-storage sparse vector's elements 
		/// into compressed form
		/// x[i] = y[indx[i]], for i=0,1,... ,nz-1
		/// </summary>
		/// <param name="n"> totol elements in the dense vector y </param>
		/// <param name="y"> input dense vector y </param>
		/// <param name="nnz"> number of non-zero elements </param>
		/// <param name="indx"> indices of non-zero elements in y </param>
		/// <returns> result sparse vector x </returns>
		static WVectorZi^ Gthr(long long n, void* y, long long nnz, long long* indx);

		/// <summary>
		/// gathers a full-storage sparse vector's elements 
		/// into compressed form
		/// x[i] = y[indx[i]], for i=0,1,... ,nz-1
		/// </summary>
		/// <param name="n"> totol elements in the dense vector y </param>
		/// <param name="y"> input dense vector y </param>
		/// <param name="nnz"> number of non-zero elements </param>
		/// <param name="x"> output sparse vector x </param>
		/// <param name="indx"> indices of non-zero elements in y </param>
		static void Gthr(long long n, void* y, long long nnz, void* x, long long* indx);

		/// <summary>
		/// performs rotation of point in the plane
		/// xi = c * xi + s * yi
		/// yi = c * yi - s * xi
		/// </summary>
		/// <param name="xi"> compressed sparse vector x </param>
		/// <param name="y"> dense vector y </param>
		/// <param name="c"> scalar parameter </param>
		/// <param name="s"> scalar parameter </param>
		static void Roti(WVectorDi^ xi, double* y, double c, double s);

		/// <summary>
		/// performs rotation of point in the plane
		/// xi = c * xi + s * yi
		/// yi = c * yi - s * xi
		/// </summary>
		/// <param name="xi"> compressed sparse vector x </param>
		/// <param name="y"> dense vector y </param>
		/// <param name="c"> scalar parameter </param>
		/// <param name="s"> scalar parameter </param>
		static void Roti(WVectorZi^ xi, void* y, double c, double s);

		/// <summary>
		/// converts a compressed sparse vector x 
		/// into full-storage vector y
		/// y[indx[i]] = x[i], for i=0,1,... ,nz-1
		/// </summary>
		/// <param name="xi"> input sparse vector x </param>
		/// <param name="y"> result dense vector y </param>
		static void Sctr(WVectorDi^ xi, double* y);

		/// <summary>
		/// converts a compressed sparse vector x 
		/// into full-storage vector y
		/// y[indx[i]] = x[i], for i=0,1,... ,nz-1
		/// </summary>
		/// <param name="xi"> input sparse vector x </param>
		/// <param name="y"> result dense vector y </param>
		static void Sctr(WVectorZi^ xi, void* y);

		/// <summary>
		/// computes the sum of magnitudes of all the elements
		/// of a compressed sparse vector 
		/// res = |Re(x1)| + |Im(x1)| + |Re(x2)| + |Im(x2)|+ 
		/// ... + |Re(xn)| + |Im(xn)|
		/// </summary>
		/// <param name="xi"> compressed sparse vector x </param>
		/// <param name="incx"> increment </param>
		/// <returns> absolute sum of all the vector elements </returns>
		static double Asum(WVectorDi^ xi, long long incx);

		/// <summary>
		/// computes the sum of magnitudes of all the elements
		/// of a compressed sparse vector 
		/// res = |Re(x1)| + |Im(x1)| + |Re(x2)| + |Im(x2)|+ 
		/// ... + |Re(xn)| + |Im(xn)|
		/// </summary>
		/// <param name="xi"> compressed sparse vector x </param>
		/// <param name="incx"> increment </param>
		/// <returns> absolute sum of all the vector elements </returns>
		static double Asum(WVectorZi^ xi, long long incx);

		/// <summary>
		/// copies compressed sparse vector x to vector y
		/// y = x
		/// </summary>
		/// <param name="xi"> compressed sparse vector xi </param>
		/// <param name="yi"> compressed sparse vector yi </param>
		/// <param name="incx"> increment in x </param>
		/// <param name="incy"> increment in y </param>
		static void Copy(WVectorDi^ xi, WVectorDi^ yi,
			long long incx, long long incy);

		/// <summary>
		/// copies compressed sparse vector x to vector y
		/// y = x
		/// </summary>
		/// <param name="xi"> compressed sparse vector xi </param>
		/// <param name="yi"> compressed sparse vector yi </param>
		/// <param name="incx"> increment in x </param>
		/// <param name="incy"> increment in y </param>
		static void Copy(WVectorZi^ xi, WVectorZi^ yi,
			long long incx, long long incy);

		/// <summary>
		/// computes the Euclidean norm of a vector
		/// res = || x ||
		/// </summary>
		/// <param name="xi"> compressed sparse vector x </param>
		/// <param name="incx"> increment </param>
		/// <returns> Euclidean norm </returns>
		static double Nrm2(WVectorDi^ xi, long long incx);

		/// <summary>
		/// computes the Euclidean norm of a vector
		/// res = || x ||
		/// </summary>
		/// <param name="xi"> compressed sparse vector x </param>
		/// <param name="incx"> increment </param>
		/// <returns> Euclidean norm </returns>
		static double Nrm2(WVectorZi^ xi, long long incx);

		/// <summary>
		/// computes the product of a vector by a scalar
		/// x = a*x
		/// </summary>
		/// <param name="alpha"> scalar multiple </param>
		/// <param name="xi"> compressed sparse vector x </param>
		/// <param name="incx"> increment </param>
		static void Scal(double alpha, WVectorDi^ xi, long long incx);

		/// <summary>
		/// computes the product of a vector by a scalar
		/// x = a*x
		/// </summary>
		/// <param name="alpha"> scalar multiple </param>
		/// <param name="xi"> compressed sparse vector x </param>
		/// <param name="incx"> increment </param>
		static void Scal(WComplex^ alpha, WVectorZi^ xi, long long incx);

		/// <summary>
		/// finds the index of the element in the vector
		/// with maximum absolute value
		/// </summary>
		/// <param name="xi"> compressed sparse vector x </param>
		/// <param name="incx"> increment </param>
		/// <returns> index of the element </returns>
		static long long Iamax(WVectorDi^ xi, long long incx);

		/// <summary>
		/// finds the index of the element in the vector
		/// with maximum absolute value
		/// </summary>
		/// <param name="xi"> compressed sparse vector x </param>
		/// <param name="incx"> increment </param>
		/// <returns> index of the element </returns>
		static long long Iamax(WVectorZi^ xi, long long incx);

		/// <summary>
		/// finds the index of the element in the vector
		/// with minimum absolute value
		/// </summary>
		/// <param name="xi"> compressed sparse vector x </param>
		/// <param name="incx"> increment </param>
		/// <returns> index of the element </returns>
		static long long Iamin(WVectorDi^ xi, long long incx);

		/// <summary>
		/// finds the index of the element in the vector
		/// with minimum absolute value
		/// </summary>
		/// <param name="xi"> compressed sparse vector x </param>
		/// <param name="incx"> increment </param>
		/// <returns> index of the element </returns>
		static long long Iamin(WVectorZi^ xi, long long incx);

		#pragma endregion
		#pragma region methods: SPARSE BLAS - Matrix Manipulation

		/// <summary>
		/// creates a CSR-format matrix [real-valued]
		/// </summary>
		/// <param name="a"> sparse matrix a </param>
		/// <param name="rowPtr"> row start/end indices </param>
		/// <param name="col_indx"> non-zero column indices </param>
		/// <param name="values"> non-zero element values </param>
		static void SparseCreateCSR(WMatrixDi^ a,
			long long* rowPtr, long long* col_indx,
			double* values);

		/// <summary>
		/// creates a CSR-format matrix [complex-valued]
		/// </summary>
		/// <param name="a"> sparse matrix a </param>
		/// <param name="rowPtr"> row start/end indices </param>
		/// <param name="col_indx"> non-zero column indices </param>
		/// <param name="values"> non-zero element values </param>
		static void SparseCreateCSR(WMatrixZi^ a,
			long long* rowPtr, long long* col_indx,
			void* values);

		/// <summary>
		/// creates a CSC-format matrix [real-valued]
		/// </summary>
		/// <param name="a"> sparse matrix a </param>
		/// <param name="colPtr"> column start/end indices </param>
		/// <param name="row_indx"> non-zero row indices </param>
		/// <param name="values"> non-zero values </param>
		static void SparseCreateCSC(WMatrixDi^ a,
			long long* colPtr, long long* row_indx,
			double* values);

		/// <summary>
		/// creates a CSC-format matrix [complex-valued]
		/// </summary>
		/// <param name="a"> sparse matrix a </param>
		/// <param name="colPtr"> column start/end indices </param>
		/// <param name="row_indx"> non-zero row indices </param>
		/// <param name="values"> non-zero values </param>
		static void SparseCreateCSC(WMatrixZi^ a,
			long long* colPtr, long long* row_indx,
			void* values);

		/// <summary>
		/// creates a COO-format matrix [real-valued]
		/// </summary>
		/// <param name="a"> sparse matrix a </param>
		/// <param name="row_indx"> non-zero row indices </param>
		/// <param name="col_indx"> non-zero column indices </param>
		/// <param name="values"> non-zero values </param>
		static void SparseCreateCOO(WMatrixDi^ a, 
			long long* row_indx, long long* col_indx,
			double* values);

		/// <summary>
		/// creates a COO-format matrix [complex-valued]
		/// </summary>
		/// <param name="a"> sparse matrix a </param>
		/// <param name="row_indx"> non-zero row indices </param>
		/// <param name="col_indx"> non-zero column indices </param>
		/// <param name="values"> non-zero values </param>
		static void SparseCreateCOO(WMatrixZi^ a,
			long long* row_indx, long long* col_indx,
			void* values);

		/// <summary>
		/// creates a copy of a sparse matrix [real-valued]
		/// </summary>
		/// <param name="source"> source sparse matrix a </param>
		/// <returns> copy of the sparse matrix </returns>
		static WMatrixDi^ SparseCopy(WMatrixDi^ a);

		/// <summary>
		/// creates a copy of a sparse matrix [complex-valued]
		/// </summary>
		/// <param name="source"> source sparse matrix a </param>
		/// <returns> copy of the sparse matrix </returns>
		static WMatrixZi^ SparseCopy(WMatrixZi^ a);

		/// <summary>
		/// frees memory allocated for sparse matrix [real-valued]
		/// </summary>
		/// <param name="a"> sparse matrix a </param>
		static void SparseDestroy(WMatrixDi^ a);

		/// <summary>
		/// frees memory allocated for sparse matrix [complex-valued]
		/// </summary>
		/// <param name="a"> sparse matrix a </param>
		static void SparseDestroy(WMatrixZi^ a);

		/// <summary>
		/// converts a sparse matrix to CSR format [real-valued]
		/// </summary>
		/// <param name="a"> source sparse matrix a </param>
		/// <param name="operation"> operation on the input matrix </param>
		/// <returns> result sparse matrix in CSR format </returns>
		static WMatrixDi^ SparseConvert2CSR(WMatrixDi^ a, int operation);

		/// <summary>
		/// converts a sparse matrix to CSR format [complex-valued]
		/// </summary>
		/// <param name="a"> source sparse matrix a </param>
		/// <param name="operation"> operation on the input matrix </param>
		/// <returns> result sparse matrix in CSR format </returns>
		static WMatrixZi^ SparseConvert2CSR(WMatrixZi^ a, int operation);

		/// <summary>
		/// exports CSR matrix from internal representation
		/// </summary>
		/// <param name="source"> handle containing the sparse matrix </param>
		/// <param name="indexing"> indicates how input arrays are indexed </param>
		/// <param name="rows"> number of rows of the matrix </param>
		/// <param name="cols"> number of columns of the matrix </param>
		/// <param name="rows_start"> pointer to the starting indices of rows </param>
		/// <param name="rows_end"> pointer to the ending indices of rows </param>
		/// <param name="col_indx"> pointer to the non-zero column indices </param>
		/// <param name="values"> pointer to the non-zero values </param>
		/// <returns> value indicating whether the operation was successful or not </returns>
		static sparse_status_t SparseExportCSR(sparse_matrix_t source,
			sparse_index_base_t* indexing, long long* rows, long long* cols,
			long long** rows_start, long long** rows_end, long long** col_indx,
			double** values);

		/// <summary>
		/// exports CSC matrix from internal representation
		/// </summary>
		/// <param name="source"> handle containing the sparse matrix </param>
		/// <param name="indexing"> indicates how input arrays are indexed </param>
		/// <param name="rows"> number of rows of the matrix </param>
		/// <param name="cols"> number of columns of the matrix </param>
		/// <param name="cols_start"> pointer to the starting indices of columns </param>
		/// <param name="cols_end"> pointer to the ending indices of columns </param>
		/// <param name="row_indx"> pointer to the non-zero row indices </param>
		/// <param name="values"> pointer to the non-zero values </param>
		/// <returns> value indicating whether the operation was successful or not </returns>
		///virtual sparse_status_t SparseExportCSC(sparse_matrix_t source,
		///	sparse_index_base_t* indexing, long long* rows, long long* cols,
		///	long long** cols_start, long long** cols_end, long long** row_indx,
		///	double** values);

		/// <summary>
		/// changes a single element value in a sparse matrix [real-valued]
		/// </summary>
		/// <param name="a"> sparse matrix a </param>
		/// <param name="row"> row index </param>
		/// <param name="col"> column index </param>
		/// <param name="value"> target value of the selected element </param>
		static void SparseSetValue(WMatrixDi^ a,
			long long row, long long col, double value);

		/// <summary>
		/// changes a single element value in a sparse matrix [complex-valued]
		/// </summary>
		/// <param name="a"> sparse matrix a </param>
		/// <param name="row"> row index </param>
		/// <param name="col"> column index </param>
		/// <param name="value"> target value of the selected element </param>
		static void SparseSetValue(WMatrixZi^ a,
			long long row, long long col, WComplex^ value);

		/// <summary>
		/// changes all or selected elements values in a sparse matrix
		/// </summary>
		/// <param name="a"> sparse matrix a </param>
		/// <param name="nvalues"> number of values changed </param>
		/// <param name="row_indx"> pointer to the row indices of the new values </param>
		/// <param name="col_indx"> pointer to the column indices of the new values </param>
		/// <param name="values"> pointer to the new values </param>
		static void SparseUpdateValues(WMatrixDi^ a,
			long long nvalues, long long* row_indx, long long* col_indx,
			double* values);

		/// <summary>
		/// changes all or selected elements values in a sparse matrix
		/// </summary>
		/// <param name="a"> sparse matrix a </param>
		/// <param name="nvalues"> number of values changed </param>
		/// <param name="row_indx"> pointer to the row indices of the new values </param>
		/// <param name="col_indx"> pointer to the column indices of the new values </param>
		/// <param name="values"> pointer to the new values </param>
		static void SparseUpdateValues(WMatrixZi^ a,
			long long nvalues, long long* row_indx, long long* col_indx,
			void* values);

		#pragma endregion
		#pragma region methods: SPARSE BLAS - Analysis Routines

		/// <summary>
		/// provides estimate of number and type of upcoming
		/// matrix - vector operations
		/// </summary>
		/// <param name="a"> input sparse matrix a </param>
		/// <param name="operation"> operation on input matrix a </param>
		/// <param name="matrixType"> sparse matrix type option </param>
		/// <param name="fillMode"> sparse fill mode option </param>
		/// <param name="diagType"> sparse diagnoal type option </param>
		/// <param name="expected_calls"> number of expected calls to execution routine </param>
		static void SparseSetMVHint(WMatrixDi^ a,
			int operation, int matrixType, int fillMode, int diagType,
			long long expected_calls);

		/// <summary>
		/// provides estimate of number and type of upcoming
		/// triangular system solver operations
		/// </summary>
		/// <param name="a"> input sparse matrix a </param>
		/// <param name="operation"> operation on input matrix a </param>
		/// <param name="matrixType"> sparse matrix type option </param>
		/// <param name="fillMode"> sparse fill mode option </param>
		/// <param name="diagType"> sparse diagnoal type option </param>
		/// <param name="expected_calls"> number of expected calls to execution routine </param>
		static void SparseSetSVHint(WMatrixDi^ a,
			int operation, int matrixType, int fillMode, int diagType, 
			long long expected_calls);

		/// <summary>
		/// provides estimate of number and type of upcoming
		/// matrix - matrix multiplication operations
		/// </summary>
		/// <param name="a"> input sparse matrix a </param>
		/// <param name="operation"> operation on input matrix a </param>
		/// <param name="matrixType"> sparse matrix type option </param>
		/// <param name="fillMode"> sparse fill mode option </param>
		/// <param name="diagType"> sparse diagnoal type option </param>
		/// <param name="layout"> sparse layout option </param>
		/// <param name="dense_matrix_size"> number of columns in dense matrix </param>
		/// <param name="expected_calls"> number of expected calls to execution routine </param>
		static void SparseSetMMHint(WMatrixDi^ a,
			int operation, int matrixType, int fillMode, int diagType, 
			int layout, long long dense_matrix_size, long long expected_calls);

		/// <summary>
		/// provides estimate of number and type of upcoming
		/// triangular matrix solve with multiple right hand sides
		/// </summary>
		/// <param name="a"> input sparse matrix a </param>
		/// <param name="operation"> operation on input matrix a </param>
		/// <param name="matrixType"> sparse matrix type option </param>
		/// <param name="fillMode"> sparse fill mode option </param>
		/// <param name="diagType"> sparse diagnoal type option </param>
		/// <param name="layout"> sparse layout option </param>
		/// <param name="dense_matrix_size"> number of columns in dense matrix </param>
		/// <param name="expected_calls"> number of expected calls to execution routine </param>
		static void SparseSetSMHint(WMatrixDi^ a,
			int operation, int matrixType, int fillMode, int diagType, int layout,
			long long dense_matrix_size, long long expected_calls);

		/// <summary>
		/// provides memory requirements for performance
		/// optimization purposes
		/// </summary>
		/// <param name="a"> input sparse matrix a </param>
		/// <param name="policy"> memory utilization policy </param>
		static void SparseSetMemoryHint(WMatrixDi^ a, int policy);

		/// <summary>
		/// analyzes matrix structure and performs optimizations
		/// using the hints provided in the handle
		/// </summary>
		/// <param name="a"> input sparse matrix a </param>
		static void SparseOptimize(WMatrixDi^ a);

		#pragma endregion
		#pragma region methods: SPARSE BLAS - Execution Routines

		/// <summary>
		/// computes a sparse matrix- vector product
		/// y := alpha*op(A)*x + beta*y
		/// </summary>
		/// <param name="operation"> specifies operation on the input matrix a </param>
		/// <param name="alpha"> scalar constant alpha </param>
		/// <param name="a"> input sparse matrix a </param>
		/// <param name="matrixType"> sparse matrix type option </param>
		/// <param name="fillMode"> sparse fill mode option </param>
		/// <param name="diagType"> sparse diagnoal type option </param>
		/// <param name="x"> vector x </param>
		/// <param name="beta"> scalar constant beta </param>
		/// <param name="y"> vector y (to be overwritten) </param>
		static void SparseMV(int operation,
			double alpha, WMatrixDi^ a, int matrixType, int fillMode, int diagType,
			double* x, double beta, double* y);

		/// <summary>
		/// solves a system of linear equations for 
		/// a triangular sparse matrix
		/// op(A)*y = alpha * x
		/// </summary>
		/// <param name="operation"> specifies operation on the input matrix a </param>
		/// <param name="alpha"> scalar constant alpha </param>
		/// <param name="a"> input sparse matrix a </param>
		/// <param name="matrixType"> sparse matrix type option </param>
		/// <param name="fillMode"> sparse fill mode option </param>
		/// <param name="diagType"> sparse diagnoal type option </param>
		/// <param name="x"> vector x </param>
		/// <param name="y"> vector y (to be overwritten) </param>
		static void SparseTRSV(int operation,
			double alpha, WMatrixDi^ a, int matrixType, int fillMode, int diagType,
			double* x, double* y);

		/// <summary>
		/// computes the product of a sparse matrix and a dense
		/// matrix and stores the result as a dense matrix
		/// C := alpha*op(A)*B + beta*C
		/// </summary>
		/// <param name="operation"> specifies operation on the input matrix a </param>
		/// <param name="alpha"> scalar constant alpha </param>
		/// <param name="a"> input sparse matrix a </param>
		/// <param name="matrixType"> sparse matrix type option </param>
		/// <param name="fillMode"> sparse fill mode option </param>
		/// <param name="diagType"> sparse diagnoal type option </param>
		/// <param name="layout"> storage scheme for the dense matrix </param>
		/// <param name="b"> dense matrix b </param>
		/// <param name="columns"> number of colums in matrix c </param>
		/// <param name="ldb"> leading dimension of matrix b. </param>
		/// <param name="beta"> scalar constant beta </param>
		/// <param name="c"> dense matrix c (to be overwrittem) </param>
		/// <param name="ldc"> leading dimension of matrix c </param>
		static void SparseMM(int operation,
			double alpha, WMatrixDi^ a, int matrixType, int fillMode, int diagType,
			int layout,
			double* b, long long columns, long long ldb, double beta,
			double* c, long long ldc);

		/// <summary>
		/// solves a system of linear equations with multiple 
		/// right-hand sides for a triangular sparse matrix
		/// Y := alpha*inv(op(A))*X
		/// op(A)*y = alpha * x
		/// </summary>
		/// <param name="operation"> specifies operation on the input matrix a </param>
		/// <param name="alpha"> scalar constant alpha </param>
		/// <param name="a"> input sparse matrix a </param>
		/// <param name="matrixType"> sparse matrix type option </param>
		/// <param name="fillMode"> sparse fill mode option </param>
		/// <param name="diagType"> sparse diagnoal type option </param>
		/// <param name="layout"> storage scheme for the dense matrix </param>
		/// <param name="x"> dense matrix x </param>
		/// <param name="columns"> number of columns in matrix y </param>
		/// <param name="ldx"> leading dimension of matrix x </param>
		/// <param name="y"> dense matrix y (to be overwritten) </param>
		/// <param name="ldy"> leading dimension in matrix y </param>
		static void SparseTRSM(int operation,
			double alpha, WMatrixDi^ a, int matrixType, int fillMode, int diagType, 
			int layout,
			double* x, long long columns, long long ldx, double* y, long long ldy);

		/// <summary>
		/// computes the sum of two sparse matrices and store
		/// the result in a newly allocated sparse matrix
		/// C := alpha*op(A) + B
		/// </summary>
		/// <param name="operation"> specifies operation on the input matrix a </param>
		/// <param name="a"> input sparse matrix a </param>
		/// <param name="alpha"> scalar constant alpha </param>
		/// <param name="b"> input sparse matrix b </param>
		/// <param name="c"> result sparse matrix c </param>
		/// <returns> value indicating whether the operation was successful or not </returns>
		
		/// <summary>
		/// computes the sum of two sparse matrices and store
		/// the result in a newly allocated sparse matrix
		/// C := alpha*op(A) + B
		/// </summary>
		/// <param name="operation"> specifies operation on the input matrix a </param>
		/// <param name="a"> input sparse matrix a </param>
		/// <param name="alpha"> scalar constant alpha </param>
		/// <param name="b"> input sparse matrix b </param>
		/// <returns> result sparse matrix c </returns>
		static WMatrixDi^ SparseAdd(int operation,
			WMatrixDi^ a, double alpha, WMatrixDi^ b);

		///// <summary>
		///// computes the product of two sparse matrices and store
		///// the result in a newly allocated sparse matrix
		///// C := op(A) *B
		///// </summary>
		///// <param name="operation"> specifies operation on the input matrix a </param>
		///// <param name="a"> input sparse matrix a </param>
		///// <param name="b"> input sparse matrix b </param>
		///// <param name="c"> result sparse matrix c </param>
		///// <returns> value indicating whether the operation was successful or not </returns>
		//virtual sparse_status_t SparseSPMM(sparse_operation_t operation,
		//	sparse_matrix_t a, sparse_matrix_t b, sparse_matrix_t* c);

		///// <summary>
		///// computes the product of two sparse matrices and
		///// stores the result as a dense matrix
		///// C := op(A)*B
		///// </summary>
		///// <param name="operation"> specifies operation on the input matrix a </param>
		///// <param name="a"> input sparse matrix a </param>
		///// <param name="b"> input sparse matrix b </param>
		///// <param name="layout"> describes the storage scheme for the dense matrix </param>
		///// <param name="c"> result dense matrix c </param>
		///// <param name="ldc"> leading dimension of matrix c </param>
		///// <returns> value indicating whether the operation was successful or not </returns>
		//virtual sparse_status_t SparseSPMMD(sparse_operation_t operation,
		//	sparse_matrix_t a, sparse_matrix_t b, sparse_layout_t layout,
		//	double* c, long long ldc);

		#pragma endregion
		#pragma region methods: SPARSE BLAS - QR Routines

		/// <summary>
		/// defines the pivot strategy for further calls
		/// </summary>
		/// <param name="a"> handle containing a sparse matrix </param>
		static void SparseSetQRHint(WMatrixDi^ a);

		/// <summary>
		/// computes the QR decomposition for the matrix of a
		/// sparse linear system and calculates the solution
		/// A*x = b, so that A = Q*R
		/// </summary>
		/// <param name="a"> sparse matrix a </param>
		/// <param name="operation"> operation on the sparse matrix a </param>
		/// <param name="matrixType"> sparse matrix type option </param>
		/// <param name="fillMode"> sparse fill mode option </param>
		/// <param name="diagType"> sparse diagnoal type option </param>
		/// <param name="layout"> storage scheme for the dense matrix </param>
		/// <param name="columns"> number of columns in matrix b </param>
		/// <param name="x"> dense matrix x (to be overwritten) </param>
		/// <param name="ldx"> leading dimension of matrix x </param>
		/// <param name="b"> dense matrix b </param>
		/// <param name="ldb"> leading dimension of matrix b </param>
		static void SparseQR(WMatrixDi^ a, 
			int operation, int matrixType, int fillMode, int diagType,
			int layout, long long columns, double* x,
			long long ldx, double* b, long long ldb);

		/// <summary>
		/// reordering step of SPARSE QR solver
		/// </summary>
		/// <param name="a"> sparse matrix a </param>
		/// <param name="matrixType"> sparse matrix type option </param>
		/// <param name="fillMode"> sparse fill mode option </param>
		/// <param name="diagType"> sparse diagnoal type option </param>
		static void SparseQRReorder(WMatrixDi^ a,
			int matrixType, int fillMode, int diagType);

		/// <summary>
		/// factorization step of the SPARSE QR solver
		/// </summary>
		/// <param name="a"> sparse matrix a </param>
		/// <param name="alt_values"> alternative values </param>
		/// <returns> value indicating whether the operation was successful or not </returns>
		static void SparseQRFactorize(WMatrixDi^ a,
			double* alt_values);

		/// <summary>
		/// solving step of the SPARSE QR solver
		/// </summary>
		/// <param name="a"> sparse matrix a </param>
		/// <param name="alt_values"> alternative values </param>
		/// <param name="operation"> operation on the sparse matrix a </param>
		/// <param name="layout"> storage scheme for the dense matrix </param>
		/// <param name="columns"> number of columns in matrix b </param>
		/// <param name="x"> dense matrix x </param>
		/// <param name="ldx"> leading dimension of matrix x </param>
		/// <param name="b"> dense matrix b </param>
		/// <param name="ldb"> leading dimension of matrix b </param>
		static void SparseQRSolve(WMatrixDi^ a, double* alt_values,
			int operation, int layout, long long columns, double* x, long long ldx,
			double* b, long long ldb);

		/// <summary>
		/// first stage of the solving step of the SPARSE QR solver
		/// </summary>
		/// <param name="a"> sparse matrix a </param>
		/// <param name="operation"> operation on the sparse matrix a </param>
		/// <param name="layout"> storage scheme for the dense matrix </param>
		/// <param name="columns"> number of columns in matrix b </param>
		/// <param name="x"> dense matrix x </param>
		/// <param name="ldx"> leading dimension of matrix x </param>
		/// <param name="b"> dense matrix b </param>
		/// <param name="ldb"> leading dimension of matrix b </param>
		static void SparseQRQmult(WMatrixDi^ a,
			int operation, int layout, long long columns,
			double* x, long long ldx, double* b, long long ldb);

		/// <summary>
		/// second stage of the solving step of the SPARSE QR solver
		/// </summary>
		/// <param name="a"> sparse matrix a </param>
		/// <param name="operation"> operation on the sparse matrix a </param>
		/// <param name="layout"> storage scheme for the dense matrix </param>
		/// <param name="columns"> number of columns in matrix b </param>
		/// <param name="x"> dense matrix x </param>
		/// <param name="ldx"> leading dimension of matrix x </param>
		/// <param name="b"> dense matrix b </param>
		/// <param name="ldb"> leading dimension of matrix b </param>
		static void SparseQRRsolve(WMatrixDi^ a,
			int operation, int layout, long long columns,
			double* x, long long ldx, double* b, long long ldb);

		#pragma endregion

		#pragma region methods: SPARSE BLAS - converters

		/// <summary>
		/// convetst operation option from integer format
		/// to the sparse_operation_t format
		/// </summary>
		/// <param name="operation"> input in the integer format </param>
		/// <returns> output in the sparse_operation_t format </returns>
		static sparse_operation_t SparseOperationConverter(int operation);

		/// <summary>
		/// converts sparse matrix type from integer format
		/// to the sparse_matrix_type_t format
		/// </summary>
		/// <param name="type"> input in the integer format </param>
		/// <returns> output in the sparse_matrix_type_t format </returns>
		static sparse_matrix_type_t SparseMatrixTypeConverter(int type);

		/// <summary>
		/// converts sparse fill mode from integer format
		/// to the sparse_fill_mode_t format
		/// </summary>
		/// <param name="mode"> input in the integer format </param>
		/// <returns> output in the sparse_fill_mode_t format </returns>
		static sparse_fill_mode_t SparseFillModeConverter(int mode);

		/// <summary>
		/// converts sparse diagonal type from integer format
		/// into the sparse_diag_type_t format
		/// </summary>
		/// <param name="type"> input in the integer format </param>
		/// <returns> output in the sparse_diag_type_t format </returns>
		static sparse_diag_type_t SparseDiagTypeConverter(int type);

		/// <summary>
		/// converts sparse layout from integer format
		/// into the sparse_layout_t format
		/// </summary>
		/// <param name="layout"> input in the integer format </param>
		/// <returns> output in the sparse_layout_t format </returns>
		static sparse_layout_t SparseLayoutConverter(int layout);

		/// <summary>
		/// converts sparse memory usage from integer format
		/// into the sparse_memoery_usage_t format
		/// </summary>
		/// <param name="usage"> input in the integer format </param>
		/// <returns> output in the sparse_memory_usage_t format </returns>
		static sparse_memory_usage_t SparseMemoryUsageConverter(int usage);

		/// <summary>
		/// converts sparse status to integer format
		/// </summary>
		/// <param name="status"> input sparse status </param>
		/// <returns> output in integer format </returns>
		static int SparseStatusConverter(sparse_status_t status);

		#pragma endregion

	


	};

}
