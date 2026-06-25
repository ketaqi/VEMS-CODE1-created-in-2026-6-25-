using System.Numerics;

namespace VEMS.MathCore
{
    /// <summary>
    /// vector interface
    /// </summary>
    /// <typeparam name="T"> type: double or complex </typeparam>
    public interface IVector<T> where T : struct
    {
        #region properties

        /// <summary>
        /// get / set the value of a vector element 
        /// </summary>
        /// <param name="i"> index of the element [Int64] </param>
        /// <param name="checkBound"> whether to check if the index is outside bound </param>
        /// <returns> element value </returns>
        T this[long i, bool checkBound = true] { get; set; }

        /// <summary>
        /// get / set the value of a vector element 
        /// </summary>
        /// <param name="i"> index of the element [Int32] </param>
        /// <param name="checkBound"> whether to check if the index is outside bound </param>
        /// <returns> element value </returns>
        T this[int i, bool checkBound = true] { get; set; }

        /// <summary>
        /// get / set the value of a vector element 
        /// </summary>
        /// <param name="i"> index of the element [Int32] </param>
        /// <param name="checkBound"> whether to check if the index is outside bound </param>
        /// <returns> element value </returns>
        T this[Index i, bool checkBound = true] { get; set; }

        #endregion
        #region methods

        /// <summary>
        /// checks if a given index is valid
        /// [lower bound = zero] 
        /// [upper bound = Count]
        /// </summary>
        /// <param name="i"> input index </param>
        /// <returns> true: if index is valid </returns>
        bool IsIndexValid(long i);

        ///// <summary>
        ///// sums up all the vector elements
        ///// </summary>
        ///// <returns> summed result </returns>
        //T Sum();

        ///// <summary>
        ///// finds the index of the element 
        ///// with minimum absolute value
        ///// </summary>
        ///// <returns> index of the element with minimum abs </returns>
        //long FindMinAbsIndex();

        ///// <summary>
        ///// finds the index of the element 
        ///// with maximum absolute value
        ///// </summary>
        ///// <returns> index of the element with maximum abs </returns>
        //long FindMaxAbsIndex();

        #endregion

        //static abstract IVector<T> operator +(IVector<T> x, IVector<T> y);
    
    }


    /// <summary>
    /// two-dimensional vector interface
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IVec2<T> where T : struct
    {
        #region properties

        /// <summary>
        /// x-component of the vector
        /// </summary>
        T X { get; set; }

        /// <summary>
        /// y-component of the vector
        /// </summary>
        T Y { get; set; }

        #endregion
        #region methods

        /// <summary>
        /// normalizes the vector 
        /// </summary>
        void Normalize();

        /// <summary>
        /// computes the squared norm of this vector
        /// </summary>
        /// <returns> squared norm </returns>
        double NormSquare();

        /// <summary>
        /// computes and norm of this vector
        /// </summary>
        /// <returns> norm </returns>
        double Norm();

        /// <summary>
        /// dot-product with another vector
        /// </summary>
        /// <param name="a"> another vector </param>
        /// <returns> result dot product </returns>
        T DotWith(IVec2<T> a);

        /// <summary>
        /// adds another vector to this
        /// </summary>
        /// <param name="a"> another vector a </param>
        void Add(IVec2<T> a);

        /// <summary>
        /// subtracts another vector from this
        /// </summary>
        /// <param name="a"> another vector a </param>
        void Sub(IVec2<T> a);

        /// <summary>
        /// component-wise multiply another vector on this
        /// </summary>
        /// <param name="a"> another vector a </param>
        void Mul(IVec2<T> a);

        /// <summary>
        /// component-wise division by another vector
        /// </summary>
        /// <param name="a"> another vector a </param>
        void Div(IVec2<T> a);

        /// <summary>
        /// takes the negative of this vector
        /// </summary>
        void Neg();

        #endregion
    }

}
