using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace VEMS.MathCore
{
    /// <summary>
    /// complex-valued matrix on Cuda device
    /// </summary>
    public unsafe class CudaMatrixZ : CudaArray<Complex>
    {
        #region properties

        private long rows { get; set; }

        /// <summary>
        /// number of rows
        /// </summary>
        public long Rows
        {
            get => rows;
            set => rows = value;
        }

        private long cols { get; set; }

        /// <summary>
        /// number of columns
        /// </summary>
        public long Cols
        {
            get => cols;
            set => cols = value;
        }

        #endregion
        #region constructor

        /// <summary>
        /// constructs an empty CudaMatrixZ
        /// with given number of rows and columns
        /// </summary>
        /// <param name="rows"> number of rows </param>
        /// <param name="cols"> number of columns </param>
        public CudaMatrixZ (long rows, long cols) : base(rows * cols)
        {
            Rows = rows;
            Cols = cols;
        }

        /// <summary>
        /// constructs a CudaMatrixZ from a given
        /// matrix x on the host memory space
        /// </summary>
        /// <param name="x"> host matrix x as the source </param>
        public CudaMatrixZ(MatrixZ x) : this(x.Rows, x.Cols)
            => SetValues(x);

        #endregion
        #region methods

        /// <summary>
        /// copies a matrix x in host memory space 
        /// to the matrix in device memory space
        /// </summary>
        /// <param name="x"> host matrix x as the source </param>
        public void SetValues(MatrixZ x)
        {
            // first, transposes x, pretents to be column-major
            MatrixZ xt = LinAlg.Transpose(x);
            // then, sets the matrix
            CudaHelper.SetMatrix(x.Rows, x.Cols, sizeof(Complex), xt.VPtr, xt.Rows, VPtr, Rows);
        }

        /// <summary>
        /// copies the matrix in device memory space
        /// to a matrix y in host memory space
        /// </summary>
        /// <param name="y"> host matrix y as the target </param>
        private void GetValues(ref MatrixZ y)
        {
            // first, gets the matrix
            CudaHelper.GetMatrix(Rows, Cols, sizeof(Complex), VPtr, Rows, y.VPtr, y.Rows);
            // then, transpose matrix y to be row-major as result
            LinAlg.Transpose(ref y);
            y.Rows = Cols;
            y.Cols = Rows;
        }

        /// <summary>
        /// copies the matrix in device memory space
        /// to a matrix y in host memory space
        /// </summary>
        /// <returns> result host matrix y </returns>
        public MatrixZ GetValues()
        {
            // first, prepares a transposed container
            MatrixZ yt = new(Cols, Rows);
            // then, gets the matrix to the transposed container
            CudaHelper.GetMatrix(Rows, Cols, sizeof(Complex), VPtr, Rows, yt.VPtr, yt.Rows);
            // finally, transpose the matrix
            LinAlg.Transpose(ref yt);
            // MatrixZ y = LinAlg.Transpose(yt);
            return yt;
        }

        /// <summary>
        /// computes the sum of absolute values of all elements
        /// </summary>
        /// <returns> sum of absolute values </returns>
        public double ASum()
            => CudaVMath.AbsSum(this);

        #endregion
    }
}